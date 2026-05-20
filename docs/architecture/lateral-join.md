# Architectural Document: LATERAL JOIN expression support

**Status:** Proposed
**Author:** Sarah (Software Architect)
**Target branch:** `feature/lateral-join` (off `master`)
**Tracking:** DiVoid task #449 (this design), task #450 (downstream DiVoid call-site migration)
**Implementer:** John (backend)
**Reviewer:** Jenny (QA)

---

## 1. Problem Statement

Downstream consumer **DiVoid** has a call site in `Backend/Services/Nodes/NodeService.cs:259-263` that currently builds the set of node ids reachable through a link row using a **UNION** of two `LoadOperation<NodeLink>` selects, then plugs that into an `In(...)` predicate against the main node load. The author left a `// TODO: Lateral Join in Ocelot` (commit `7c6abc81`, 2026-05-06) because the natural SQL expression here is a `LATERAL JOIN` (correlated subquery exposed as a joined row source), but Ocelot's fluent API has no idiomatic way to express it.

The broader case for LATERAL JOIN in Ocelot is not the one call site — it is a recurring SQL shape Ocelot users hit when:

- expressing **top-N-per-group** queries (correlated `LIMIT k` subqueries joined per outer row),
- composing **per-row computed projections** that depend on outer columns (e.g. "for each parent row, the latest child row"),
- collapsing the **UNION-then-`IN`** workaround pattern (which DiVoid currently hand-rolls and is the immediate driver),
- bridging to **set-returning functions** that depend on per-row arguments (`unnest`, `generate_series` correlated to a row's bounds).

Success criteria for this design:

1. The DiVoid call-site rewrite (Section 8) is shorter, clearer, and produces a single SQL statement rather than `SELECT ... WHERE id IN (SELECT ... UNION ...)`.
2. The new primitive composes with `LoadOperation<T>`, existing joins, `Where`, `OrderBy`, `Limit`, `Union`, and the recent `ExecuteWindowedAsync` / `ExecutePagedAsync` paths without special cases in the operation pipeline.
3. The dialect strategy is explicit per engine — no `NotImplementedException` surprises at runtime in production paths.
4. The token model is structurally indistinguishable from a regular subquery join, because — at the token level — it nearly is.

## 2. Scope & Non-Scope

**In scope**

- Adding a LATERAL join *shape* to `LoadOperation<T>` (and the non-generic `LoadOperation`).
- Two flavors: **cross-lateral** (inner-equivalent; rows with empty lateral result are dropped) and **left-lateral** (outer-equivalent; rows with empty lateral result are preserved with nulls).
- Dialect rendering for **Postgres** (full), **MySQL/MariaDB** (full on 8.0.14+ / 10.3+), **MSSQL** (via `CROSS APPLY` / `OUTER APPLY`), **SQLite** (clear `NotSupportedException`).
- Composition rules with existing joins, `From`, `Where`, `OrderBy`, `Limit`, `Union`, windowed aggregates.
- The two-arg `LoadOperation<TLoad, TJoin>` continuation type so the `Where(Func<TLoad, TJoin, bool>)` lambda surface works for lateral joins identically to existing joins.

**Out of scope**

- `LATERAL` against a stored table-valued function or a database function with row-set return (e.g. `LATERAL unnest(...)`). The reusable token shape would support it; binding it from C# is a follow-up.
- A C# extension method DSL like `from x in load from y in lateral` LINQ syntax — Ocelot's API is fluent, not query-comprehension, and we should not introduce a second style.
- Translating arbitrary IQueryable expressions to LATERAL joins. Ocelot is explicit, not implicit.
- Re-engineering `JoinOperation` storage. The existing class is the seam.
- Auto-detection of correlation. The user explicitly opts into LATERAL with an explicit method name.

## 3. Assumptions & Constraints

- **Target dialect priority:** Postgres is the production target for DiVoid. MySQL, MSSQL, SQLite must behave predictably but only Postgres has the immediate downstream consumer.
- **`netstandard2.1` library, `net8.0` test project.** Same as the rest of the codebase.
- **`MultipleConnectionsSupported = false` on SQLite** — the wrapped `LockedDBClient` is the single shared connection. LATERAL JOIN is a SQL-shape concern; it does not introduce a second connection. No client-layer change.
- **Async + sync parity** — the existing execution paths off `LoadOperation<T>` already provide both. The new primitive is purely a `Prepare()`-time shape; execution is unchanged.
- **Sub-query correlation is implicit in the user's lambda.** The user writes a `Where` predicate on the *inner* `LoadOperation<TInner>` that references the *outer* type's properties via `DB.Property<TOuter>(...)`. Ocelot already does this correctly for normal subquery joins (via the alias chain in `LoadOperation.Prepare`), so no new visitor logic is required *for the user-facing side*. The LATERAL-specific behavior is purely dialect-level keyword emission.

## 4. Architectural Overview

The design is **minimal**, leveraging that `JoinOperation` already stores `IDatabaseOperation Operation` for subquery joins, and the `LoadOperation.Prepare` loop already emits `<JoinType> JOIN (subquery) AS alias ON criteria`. A LATERAL join differs from a regular subquery join in exactly two ways:

1. The SQL keyword `LATERAL` (Postgres/MySQL) or the entirely different construct `CROSS APPLY` / `OUTER APPLY` (MSSQL) appears in place of plain `JOIN`.
2. The `ON` clause is typically `ON TRUE` (because the correlation is inside the lateral's `WHERE`).

Therefore the design is:

- **Extend `JoinOp` with two values:** `CrossLateral` and `LeftLateral`.
- **Add fluent methods to `LoadOperation<T>`:** `LateralJoin(IDatabaseOperation, criteria, alias)` and `LeftLateralJoin(IDatabaseOperation, criteria, alias)`. The criteria may be `null`, which the renderer translates to `ON TRUE`.
- **Move the "render this `JoinOperation` to SQL" responsibility into `IDBInfo`** as a single new method `AppendJoin`, so the dialect owns the keyword choice. Default implementation (in `DBInfo` base class) covers Inner / Left / CrossLateral (LATERAL) / LeftLateral (LEFT LATERAL) for ANSI-style engines. `MsSqlInfo` overrides to emit `CROSS APPLY` / `OUTER APPLY`. `SQLiteInfo` throws `NotSupportedException` *only* for the two lateral variants.
- **No new SqlToken class** is required. The LATERAL shape is a join-shape concern, not a value-token concern. Adding it as a token (option B in alternatives) would create a parallel API surface and double the documentation surface.

### Component diagram (ASCII)

```
User code                                                                
    |                                                                    
    | LoadOperation<T>.LateralJoin(subop, criteria, alias)              
    v                                                                    
LoadOperation<T>                                                         
  - joinoperations: List<JoinOperation>     <-- new entries flagged       
                                                with JoinOp.CrossLateral  
                                                or JoinOp.LeftLateral     
    |                                                                    
    | Prepare(IOperationPreparator)                                      
    v                                                                    
For each JoinOperation:                                                  
    dbclient.DBInfo.AppendJoin(operation, preparator, ...)               
                            |                                            
              +-------------+-------------+----------------+              
              |             |             |                |              
              v             v             v                v              
        PostgreInfo    MySQLInfo     MsSqlInfo        SQLiteInfo          
        emits LATERAL  emits LATERAL emits APPLY     throws on CrossLateral
        (and JOIN)     (and JOIN)    (and JOIN)      and LeftLateral      
```

The change touches three layers, one method each. No new types except the two enum members.

## 5. Components & Responsibilities

| Component | New responsibility | What it does NOT own |
|---|---|---|
| `JoinOp` (enum) | Carries the *kind* of join, including the two lateral variants. | Has no rendering knowledge. |
| `JoinOperation` (existing class) | Already holds `IDatabaseOperation Operation`, `Expression Criterias`, `JoinOp JoinType`, `string Alias`. Unchanged. | Does not render itself. |
| `LoadOperation<T>` / `LoadOperation` (fluent surface) | Adds `LateralJoin` and `LeftLateralJoin` methods accepting an `IDatabaseOperation` (the inner select), an optional `Expression<Func<T, bool>>` (or null for `ON TRUE`), and an optional alias. The two-arg form `LoadOperation<T, TJoin>` is reached via a parallel generic overload so users can write `Where(Func<T, TJoin, bool>)` on the result. | Does not know dialect SQL. Does not render the join itself. |
| `IDBInfo` / `DBInfo` (dialect base) | New method `AppendJoin(JoinOperation join, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptors, string outerAlias, params string[] knownAliases)`. Default emits `<KIND> JOIN ...` where `<KIND>` is `INNER`, `LEFT`, `LATERAL`, `LEFT LATERAL`, etc., plus the `(subquery) AS alias ON criteria` tail. | Does not own the join-list iteration — that stays in `LoadOperation.Prepare`. |
| `MsSqlInfo` | Overrides `AppendJoin` to translate `CrossLateral` → `CROSS APPLY` and `LeftLateral` → `OUTER APPLY` (no `ON` clause; criteria, if any, becomes a `WHERE` inside the subquery — see Section 9 for the trade-off). For Inner/Left, falls through to base. | Does not change how non-lateral joins render. |
| `SQLiteInfo` | Overrides `AppendJoin` only to intercept `CrossLateral` and `LeftLateral` and throw `NotSupportedException` with a clear message naming the unsupported feature and the engine. | Does not break any existing SQLite test. |
| `LoadOperation.Prepare` (existing flow) | Calls `DBInfo.AppendJoin(operation, preparator, descriptorgetter, tablealias, aliases)` in place of the inlined `INNER/LEFT JOIN (...) AS ... ON ...` block. | Does not branch on `JoinOp`. The dialect owns the keyword choice. |

## 6. Interactions & Data Flow

**Build phase (synchronous, in-process):**

1. User constructs a `LoadOperation<TOuter>` and calls `.LateralJoin(innerOp, criteria, "x")`.
2. `LoadOperation` records a new `JoinOperation(innerOp, criteria, JoinOp.CrossLateral, additional: null, alias: "x")` in its `joinoperations` list.
3. User adds more clauses (`.Where`, `.OrderBy`, etc.) — those remain unaffected.
4. User calls a terminal method (`.ExecuteEntities()`, `.Prepare()`, etc.).
5. `LoadOperation.Prepare(IOperationPreparator)` walks `joinoperations` and for each entry calls `dbclient.DBInfo.AppendJoin(...)`.
6. The dialect-specific `AppendJoin` writes its keyword, opens parens, calls `operation.Prepare(preparator)` (recurses into the inner `LoadOperation`), closes parens, writes alias, writes `ON criteria` (or `ON TRUE` if null, or omits entirely for APPLY).
7. The resulting `PreparedLoadOperation` executes against the underlying client through the unchanged execution stack.

**Execution phase:** identical to today. The new shape only changes the SQL text and parameter ordering; the `IDBClient` paths are untouched.

### Build-order interaction with the existing `Prepare()` body

The current `LoadOperation<T>.Prepare` (lines 870-977 of `LoadOperation.cs`) emits in order:

```
SELECT [DISTINCT] <columns> FROM <entity-or-subselect> [AS t]
  <join-block: today inline>
  WHERE <criterias>
  GROUP BY <fields>
  ORDER BY <fields>
  HAVING <criterias>
  <LIMIT/OFFSET>
  UNION ALL <unions>...
```

The lateral-join token slots **inside the join-block**, before `WHERE`. This is the *only* valid SQL position for LATERAL — it is a `FROM`-clause construct, not a `WHERE` construct. No new pipeline phase is introduced.

## 7. Data Model (Conceptual)

No persistent data model changes. The entity layer is unaffected. The only in-memory model addition is the two `JoinOp` enum values:

```
JoinOp
 +-- Inner          (existing)
 +-- Left           (existing)
 +-- CrossLateral   (NEW)  — Postgres/MySQL: LATERAL.   MSSQL: CROSS APPLY.
 +-- LeftLateral    (NEW)  — Postgres/MySQL: LEFT LATERAL.  MSSQL: OUTER APPLY.
```

The names are chosen for **engine-agnostic clarity**: `CrossLateral` describes the semantic (every outer row joined with every inner-result row, dropping empty laterals); `LeftLateral` preserves outer rows when the lateral yields nothing. Postgres/MySQL syntactically write `INNER JOIN LATERAL ... ON TRUE` and `LEFT JOIN LATERAL ... ON TRUE`, but the *semantics* are exactly cross-apply / outer-apply, hence the chosen enum names.

## 8. Contracts & Interfaces (Abstract)

### 8.1 New fluent contract on `LoadOperation<T>`

| Method | Inputs | Output | Semantics |
|---|---|---|---|
| `LateralJoin(IDatabaseOperation inner, Expression<Func<T, bool>> criteria = null, string joinAlias = null)` | `inner`: a prepared-but-not-executed Ocelot operation (typically another `LoadOperation<TInner>`); `criteria`: optional predicate that may reference both outer and inner aliases via `DB.Property<...>`; `joinAlias`: the SQL alias of the lateral result (defaults to a generated unique alias if null). | `LoadOperation<T>` for fluent chaining. | The inner subquery is evaluated **once per outer row**; rows with no inner result are dropped. Equivalent to SQL `INNER JOIN LATERAL (inner) AS joinAlias ON (criteria | TRUE)` on engines that support LATERAL. |
| `LateralJoin<TInner>(IDatabaseOperation inner, Expression<Func<T, TInner, bool>> criteria = null, string joinAlias = null)` | As above, with a two-typed-parameter criteria. | `LoadOperation<T, TInner>` (existing two-arg shape). | Identical to single-arg form but enables `.Where(Func<T, TInner, bool>)` continuation on the result. |
| `LeftLateralJoin(IDatabaseOperation inner, Expression<Func<T, bool>> criteria = null, string joinAlias = null)` | Same as `LateralJoin`. | `LoadOperation<T>`. | Outer rows are preserved with nulls in the inner columns when the lateral yields nothing. |
| `LeftLateralJoin<TInner>(IDatabaseOperation inner, Expression<Func<T, TInner, bool>> criteria = null, string joinAlias = null)` | Same as two-arg `LateralJoin`. | `LoadOperation<T, TInner>`. | Outer-preserving variant of two-arg form. |

**Invariants:**

- The `inner` argument must be a `LoadOperation` or any `IDatabaseOperation`. If `null`, the method throws `ArgumentNullException`.
- The `criteria` argument may be `null`. The renderer emits `ON TRUE` in that case (engines that support LATERAL) or omits the `ON` entirely (MSSQL APPLY). This matches the natural usage where correlation lives inside the inner's own `WHERE`.
- The `joinAlias` argument may be `null`. The renderer generates a stable alias of the form `lj{N}` where N is the join's zero-based index, ensuring referencability from outer projections without forcing the user to name it.
- Calling these methods on a `LoadOperation` whose `IDBClient.DBInfo` is `SQLiteInfo` is allowed at build time; the throw happens at `Prepare()` time, when `SQLiteInfo.AppendJoin` runs. This is consistent with Ocelot's existing late-binding pattern (e.g. `Truncate` with `ResetIdentity` on base `DBInfo`).

### 8.2 New dialect contract on `IDBInfo`

| Method | Inputs | Output | Semantics |
|---|---|---|---|
| `AppendJoin(JoinOperation join, IOperationPreparator preparator, Func<Type, EntityDescriptor> descriptors, string outerAlias)` | The join descriptor and the current preparator state, plus the outer alias for correlation resolution. | void (mutates preparator). | Emits the complete join clause — keyword, source (subquery or table), alias, and ON-clause — into the preparator. Caller (LoadOperation.Prepare) is responsible only for accumulating aliases for later WHERE/HAVING resolution. |

**Default implementation in `DBInfo`** handles all four `JoinOp` values for ANSI engines:

- `Inner` / `Left` → existing rendering (`INNER JOIN` / `LEFT JOIN`).
- `CrossLateral` → `INNER JOIN LATERAL (<inner>) AS <alias> ON (<criteria> | TRUE)`.
- `LeftLateral` → `LEFT JOIN LATERAL (<inner>) AS <alias> ON (<criteria> | TRUE)`.

`MsSqlInfo.AppendJoin` overrides only the two lateral cases:

- `CrossLateral` → `CROSS APPLY (<inner>) AS <alias>`. If criteria is non-null, it is folded into the inner's filter via a wrapping `SELECT * FROM (...) AS x WHERE (criteria)` — alternative is documented in Section 11.
- `LeftLateral` → `OUTER APPLY (<inner>) AS <alias>`. Same criteria-folding rule.

`SQLiteInfo.AppendJoin` overrides only the two lateral cases and throws `NotSupportedException("SQLite does not support LATERAL joins or CROSS/OUTER APPLY equivalents. Use UNION-based composition or a CTE.")`. Inner/Left fall through to base.

### 8.3 The `inner` argument is just an `IDatabaseOperation`

This is load-bearing: it means a lateral subquery can itself be a `LoadOperation<TInner>` with its own joins, where-clauses, order-by, limit, group-by, and even unions or *nested* lateral joins. The existing `IDatabaseOperation.Prepare(IOperationPreparator)` contract is sufficient.

### 8.4 Three usage examples

**Example A — DiVoid call-site rewrite (the validation test for this design):**

The conceptual shape (described in prose because this is an architecture doc, not code):

- Build a lateral `LoadOperation<NodeLink>` that selects `NodeLink.SourceId` *and* `NodeLink.TargetId`, with a `Where` clause stating "either side equals an outer-row's `Node.Id`, AND either side is in `filter.LinkedTo`".
- The outer `LoadOperation<Node>` is the existing one. Wire the lateral via `.LateralJoin(theInnerOp, criteria: null, joinAlias: "link")`.
- The outer `Where` clause adds: `n => DB.Property<NodeLink>("link", l => l.SourceId) != null && !n.Id.In(filter.LinkedTo)` — i.e. "rows that survived the lateral and aren't themselves in the seed set".

Generated SQL (against Postgres):

```
SELECT n.* FROM nodes AS n
INNER JOIN LATERAL (
  SELECT source_id, target_id FROM node_links
  WHERE (source_id = n.id OR target_id = n.id)
    AND (source_id IN (...) OR target_id IN (...))
  LIMIT 1
) AS link ON TRUE
WHERE n.id NOT IN (...)
```

Single SQL statement, single round-trip, the planner can short-circuit on the first matching link per node (the `LIMIT 1` inside the lateral). The UNION + `IN` workaround required scanning all link rows twice and then a hash semi-join.

**Example B — Top-3 orders per customer:**

- Outer: `LoadOperation<Customer>`.
- Inner (lateral): `LoadOperation<Order>` filtered by `o => o.CustomerId == DB.Property<Customer>(c => c.Id)`, ordered by `OrderDate DESC`, `Limit(3)`.
- `LateralJoin(innerOp, criteria: null, joinAlias: "recent")` and select projection from the joined "recent" alias.

Generated SQL (Postgres):

```
SELECT c.*, recent.order_id, recent.total
FROM customers AS c
INNER JOIN LATERAL (
  SELECT order_id, total FROM orders
  WHERE customer_id = c.id
  ORDER BY order_date DESC
  LIMIT 3
) AS recent ON TRUE
```

A classic top-N-per-group expressed naturally — without the usual `ROW_NUMBER() OVER (PARTITION BY ...)` trick, which would force every order row into the intermediate result.

**Example C — Composition with `ExecutePagedAsync` / windowed aggregate (introduced in v0.18.0 / v0.20.0):**

Because `LateralJoin` returns `LoadOperation<T>` (or its two-arg variant), all existing terminal methods work unchanged:

- `.ExecutePagedAsync(limit, offset)` continues to inject `COUNT(*) OVER()` into the projection.
- The pagination operates over the *post-lateral-join* row set, which is exactly what callers want.
- No special-case branch in `WindowedFromOperation` / `PagedFromOperation` is required.

This composition is the strongest signal that the design fits Ocelot's existing style: the new operation slots into the existing pipeline at the existing seam.

## 9. Cross-Cutting Concerns

| Concern | Handling |
|---|---|
| **Security / SQL injection** | None new. The inner operation is a `LoadOperation` whose parameters are already parameterized through `IOperationPreparator`. Aliases are either user-supplied (free-form strings — same trust model as today's join aliases) or generated (`lj{N}`). No string concatenation of user values. |
| **Observability** | The generated SQL is captured by `StatementException` on failure exactly as today — including the parameters list. Nothing new to instrument. |
| **Error handling** | Three error classes: (1) `ArgumentNullException` from the fluent methods if `inner` is null (build-time, fast). (2) `NotSupportedException` from `SQLiteInfo.AppendJoin` (Prepare-time, with engine and feature in the message). (3) `StatementException` wrapping a SQL-syntax error from old MySQL/MSSQL servers that pre-date LATERAL / APPLY support — these surface naturally through the existing client error path. |
| **Caching** | None — the `EntityDescriptorCache` is per-type. Joins are not cached. Prepared statements containing LATERAL can be cached by the underlying client identically to any other prepared statement. |
| **Concurrency** | Build-time fluent methods are not thread-safe (neither are existing fluent methods — same contract). Once `Prepare()` has been called, the resulting `PreparedLoadOperation` is immutable and safe to share across calls, identical to today. |
| **Idempotency / retries** | Not changed. A LATERAL query is a `SELECT`; the existing retry semantics of the consumer apply. |
| **Consistency model** | Not changed. Single-statement reads inherit the underlying transaction's isolation. |
| **Connection lifetime (SQLite `LockedDBClient`)** | Not impacted. LATERAL is a SQL-text concern; execution path is unchanged. The SQLite throw happens before the single shared connection is touched. |
| **Aliases / `CriteriaVisitor`** | The outer-alias list (collected in `LoadOperation.Prepare`) is extended with the lateral join's alias *before* `WHERE` rendering, exactly as today's joins. The `CriteriaVisitor` already resolves `DB.Property<TOuter>(...)` references against the outer alias — which is what makes correlation work transparently. No visitor change. |

## 10. Quality Attributes & Trade-offs

| Attribute | How addressed | Trade-off accepted |
|---|---|---|
| **Maintainability** | One enum extension, two new fluent methods (× two for the non-generic LoadOperation), one new `IDBInfo` method, three dialect implementations (Postgres/MySQL via base default, MSSQL override, SQLite throw). Total new lines small. | The `JoinOp` enum is now slightly less symmetric — `Inner` and `Left` are pure SQL-shape; `CrossLateral` and `LeftLateral` are pure-SQL-shape *plus* an implicit "this is correlated" semantic. Accepted because LATERAL is genuinely a different kind of join. |
| **Composability** | The new methods return `LoadOperation<T>` / `LoadOperation<T, TInner>` and therefore chain with every existing fluent operation. No special-case branch in `Prepare`. | None. |
| **Performance** | Eliminates the UNION+`IN` round-trip pattern. Postgres' planner is well-tuned for LATERAL. Top-N-per-group with `LIMIT k` inside a lateral is asymptotically better than `ROW_NUMBER() OVER (...) WHERE rn <= k`. | Lateral joins on engines without query-plan support can be a footgun (e.g., older MySQL). Mitigated by the dialect's explicit support matrix and the `NotSupportedException` for SQLite. |
| **Portability** | Postgres, MySQL 8.0.14+, MariaDB 10.3+, and MSSQL all have full support (with MSSQL's APPLY being semantically equivalent). SQLite has no equivalent and throws clearly. | Users who must support both SQLite and other engines must guard the code path themselves (the existing `IDBInfo.MultipleConnectionsSupported`-style branching pattern). This is consistent with how `Truncate` reset-identity is handled. |
| **Discoverability** | Methods sit on `LoadOperation<T>` next to `Join` / `LeftJoin`, sorted alphabetically: `Join`, `LateralJoin`, `LeftJoin`, `LeftLateralJoin`. IntelliSense surfaces them naturally. | None. |

## 11. Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| MSSQL `APPLY` cannot accept an `ON` clause; mapping `criteria` requires folding it into the inner's `WHERE`. | High (it is how SQL Server works). | Medium — user expectations from Postgres-side may not transfer cleanly. | Document that on MSSQL, criteria passed to `LateralJoin` are folded into the inner subquery's filter via a wrapping `SELECT`. Recommend users put correlation inside the inner's own `Where` (which works identically across engines). Alternative folding strategy: throw on MSSQL when criteria is non-null, forcing users to inline. **Decision: do the fold.** It is one of two viable choices, and the wrap-with-`SELECT *` is well-understood. The throw is more conservative but breaks the Postgres-equivalence we are trying to provide. |
| Old MySQL/MariaDB versions in production fail with a SQL-syntax error from the server. | Low–Medium. | Medium — surface as `StatementException` at execution. | Document the minimum versions (MySQL 8.0.14, MariaDB 10.3) in the XML doc on `LateralJoin`. Same posture as `WindowedAggregate` (`DB.CountOver()`), which already documents its minima. |
| Users write a non-correlated `inner` query and unintentionally pay the LATERAL cost. | Low. | Low — Postgres planner often de-correlates anyway. | Doc-comment guidance: "If the inner subquery does not reference outer columns, prefer a regular `Join(IDatabaseOperation, ...)`." Not enforced. |
| An unaliased outer `LoadOperation` (no `joinoperations` and no explicit `Alias`) currently sets `tablealias = null`. A lateral inner cannot correlate without an outer alias. | Medium. | Medium. | The existing `LoadOperation.Prepare` already sets `tablealias = "t"` when `joinoperations.Count > 0`. Because adding a lateral join *is* adding a `joinoperations` entry, this branch is already taken — outer alias is `t`, correlation works. **No new logic required.** This is the design's nicest serendipity. |
| `SQLiteInfo.AppendJoin` throw fires only at `Prepare()`-time, not at `LateralJoin()`-call-time, surprising users who do a lot of build-then-execute decoupling. | Low. | Low. | Match the pattern of `WindowedAggregate` (which also surfaces engine support at execute time via `StatementException`). Document the late-binding in the XML comment. |
| The `lj{N}` auto-alias clashes with a user's explicit alias on another join. | Very low. | Low — query fails at SQL parse with a name-conflict error. | Use a guaranteed-fresh alias generator: scan the existing `joinoperations` for collisions and increment until clean. Cheap (lists are tiny) and bulletproof. |

## 12. Migration / Rollout Strategy

This is purely additive to `Pooshit.Ocelot`:

1. Land the design + implementation in one PR off `feature/lateral-join`. The doc (this file) is part of the PR.
2. Bump `AssemblyVersion`/`PackageVersion` to `0.21.0` (additive feature, no breaking changes — minor bump).
3. Publish the NuGet preview from the merged PR.
4. **Downstream (DiVoid task #450):** in a separate PR against DiVoid, bump the `Pooshit.Ocelot` reference, rewrite `NodeService.cs:259-263` (Section 8 example A), delete the now-unused `Union` of two `LoadOperation<NodeLink>` selects. Remove the `// TODO: Lateral Join in Ocelot` comment.
5. No data migration. No SQL migrations.

The rollout is decoupled: Ocelot's PR can merge before DiVoid migrates; DiVoid keeps using the UNION workaround on the old NuGet version until it picks up the new one.

## 13. Open Questions

1. **MSSQL criteria-folding strategy.** The design proposes folding non-null `criteria` into the inner's `WHERE` via a wrapping `SELECT *`. The alternative — throwing when criteria is non-null on MSSQL — is more conservative. **Recommendation: do the fold; document it; revisit if reviewers (Jenny) flag a concrete query that mis-translates.** (Sarah's decision is to fold. Flagging here because it is the one place semantics are not pixel-identical across dialects.)
2. **MySQL minimum version enforcement.** Do we runtime-check the server version (via `SELECT VERSION()`) before issuing LATERAL? **Recommendation: no — match the existing posture for windowed aggregates, which document minima but do not enforce.** Users on ancient MySQL get a clear error from the server.
3. **Auto-alias scheme name.** `lj{N}` is short but cryptic. Alternatives: `lateral{N}` (verbose, clear), `l{N}` (terser, collides with `LimitField` only conceptually). **Recommendation: `lat{N}` — three chars, no realistic collision, suggests intent.** Confirm with John during implementation.

These are minor — none block the implementation.

## 14. Implementation Guidance for the Next Agent

John, the implementation breaks naturally into the following ordered milestones. Each is its own commit, all within one PR off `feature/lateral-join`:

1. **Enum extension and `JoinOperation` documentation.** Add `CrossLateral` and `LeftLateral` to `JoinOp`. Update XML doc comments to mention the new variants. Zero behavioral change yet — existing tests still pass.

2. **Extract `AppendJoin` into `IDBInfo` / `DBInfo`.** Move the existing inline join rendering (the per-`JoinOperation` foreach inside `LoadOperation<T>.Prepare` and `LoadOperation.Prepare`) into a new `DBInfo.AppendJoin` virtual method. Have `LoadOperation` call it. Run the full test suite — this is a pure refactor; everything must stay green. This isolates the dialect-rendering seam.

3. **Default rendering for `CrossLateral` / `LeftLateral` in `DBInfo`.** Emit `INNER JOIN LATERAL` / `LEFT JOIN LATERAL`, with `ON TRUE` when criteria is null. Add XML doc comments referencing this architecture document.

4. **Fluent surface on `LoadOperation<T>` and the non-generic `LoadOperation`.** Add `LateralJoin`, `LeftLateralJoin`, and the two-typed-parameter overloads. Implement the auto-alias generator (`lat{N}` with collision-avoidance). Add XML docs that reference the per-engine support matrix.

5. **`SQLiteInfo.AppendJoin` override.** Intercept `CrossLateral` / `LeftLateral`, throw `NotSupportedException` with a clear message ("SQLite does not support LATERAL joins. Use UNION-based composition or a recursive CTE."). Inner/Left fall through.

6. **`MsSqlInfo.AppendJoin` override.** Translate `CrossLateral` → `CROSS APPLY (...)`. Translate `LeftLateral` → `OUTER APPLY (...)`. When `criteria` is non-null, wrap the inner operation in `(SELECT * FROM (<inner>) AS w WHERE (<criteria>))` so the user-supplied criterion survives. Inner/Left fall through.

7. **MySQL behavior.** No override needed — base `DBInfo.AppendJoin` emits `LATERAL`, which MySQL 8.0.14+/MariaDB 10.3+ accept natively.

8. **Tests** (always pass `--timeout` per the global rule):

   8a. **SQLite-fallback test.** `[TestFixture, Parallelizable]`. Build a `LoadOperation` with `LateralJoin`, call `.Prepare()`, assert `NotSupportedException` with the expected message. (Build phase succeeds; throw happens at Prepare.)

   8b. **In-memory SQL-text tests** (model: `WindowedAggregateTests`). Build a `LoadOperation` with `LateralJoin` against a `Mock<IDBClient>` returning `PostgreInfo`, render the SQL, assert it contains `INNER JOIN LATERAL`, `AS lat0`, and `ON TRUE`. Do the same for `LeftLateralJoin` (asserts `LEFT JOIN LATERAL`). Use `MsSqlInfo` for `CROSS APPLY` / `OUTER APPLY` assertions.

   8c. **Postgres round-trip test** (model: `PostgresLocalTests`). Gated on `POSTGRES_CONNECTION`. Create two test entities (parent + child), insert seed data, build the equivalent of Example B (top-3-per-group), execute, assert row count and grouping correctness.

   8d. **Composition regression tests:** the lateral-joined operation surface chains cleanly with `.Where`, `.OrderBy`, `.Limit`, `.ExecutePagedAsync`. One test per chain — fast to write because each is a one-line assertion on the resulting `LoadOperation<T>`'s ability to execute (or `.Prepare()`-render the expected SQL skeleton).

   8e. **Correlation test** (Postgres-gated): build the Example A shape against seeded `Nodes` / `NodeLinks` test tables, assert the result set matches a reference-implementation UNION-based query on the same data. This is the empirical confirmation that DiVoid task #450 will produce identical results.

9. **Bump `AssemblyVersion` / `PackageVersion` to `0.21.0`** in `Ocelot/Ocelot.csproj`. Update `Readme.md` with a single short subsection under "Loading entities" titled "Lateral joins" with one of the examples from Section 8.4.

10. **Commit the architecture doc as part of the PR** (it is already committed to `feature/lateral-join` before John starts). The implementation PR includes both the doc and the code.

**Do not open the PR yourself, John — open it when implementation is done and ready for Jenny's review. Sarah's responsibility ends with this document committed and pushed.**

---

## Alternatives Considered and Rejected

### Alternative A: A new `LateralJoinToken` under `Tokens/`, exposed via `DB.Lateral(...)`

Shape: model LATERAL as a value-token, returnable from `DB.Lateral(operation, alias, criteria)` and usable inside the column projection or `Where` clause of a `LoadOperation`. **Rejected** because LATERAL is not a value expression — it is a `FROM`-clause construct. Forcing it through the token vocabulary (which `DBInfo.AddFieldLogic<T>` dispatches) would require `LoadOperation.Prepare` to look for the token in unexpected places and reposition it into the join block. The token pipeline is for inline expressions (SELECT-list, WHERE, partition-by); putting a `FROM`-clause construct through it is a category error. The windowed-aggregate precedent (`WindowedAggregate`) is *not* a counter-example — it is a SELECT-list value, which is exactly what tokens are for.

### Alternative B: A method on `LoadOperation<T>` that returns a *different* type (e.g. `LateralLoadOperation<T, TInner>`)

Shape: rather than extend `JoinOp` and reuse `JoinOperation`, introduce a parallel operation hierarchy specifically for laterals. **Rejected** because it forks the API surface. Every method on `LoadOperation<T>` (`Where`, `OrderBy`, `Limit`, `Union`, `ExecutePagedAsync`, …) would need a parallel definition on the lateral variant. The cost in maintenance and discoverability is enormous, and the conceptual benefit ("lateral joins are different enough to deserve their own type") does not hold up at the SQL level — they are joins, sharing 95% of the rendering and the entire execution stack. The current design exploits the fact that `JoinOperation` already polymorphically holds either a `Type` (entity) or an `IDatabaseOperation` (subquery); lateral is just a flag on that.

### Alternative C: Implement LATERAL as a translation of arbitrary IQueryable expressions

Shape: detect correlated subqueries inside `Where` lambdas and auto-rewrite them as LATERAL joins. **Rejected** because Ocelot is explicit by philosophy — the user writes the SQL shape they want, and Ocelot generates predictable text. Auto-rewriting introduces a layer of magic that competes with EF Core's approach without offering EF Core's depth. Worse, it would make the generated SQL unpredictable, which contradicts the library's `StatementException`-with-original-text debugging story.

---

**End of document.**
