# Architectural Document: Window Functions, Single-Statement Paged Load, and CancellationToken Plumbing

Author: Sarah (architect agent)
Date: 2026-05-09
Implementer: john-backend-dev
Branch off: `master`

---

## 1. Goal & Motivation

DiVoid (and any other Pooshit.Ocelot consumer that paginates) currently needs two SQL round trips per page: one for the page itself, one for `SELECT COUNT(*)`. On predicates that involve deep joins or chained subqueries the COUNT is the dominant cost — it does the same WHERE work as the page just to return a number. The downstream task (DiVoid 134, derived from 118 / 122) asks for a single-statement primitive that returns both the streamed page and the windowed total, using `COUNT(*) OVER ()`. This Ocelot-side work delivers that primitive plus the two enabling pieces it depends on: a generic window-function token (§2) and `CancellationToken` plumbing through the async execute path (§4, DiVoid 119) so that aborted HTTP requests actually cancel the underlying ADO.NET command instead of just hopping at boundaries.

Three deliverables, one coordinated bundle. They share test infrastructure and ship together because their consumer shipping (DiVoid 134 + 122) only becomes meaningful once all three exist in one Pooshit.Ocelot package version.

---

## 2. Token Model — Window Functions

### 2.1 The decision

**Introduce a generic `WindowedAggregate` token AND keep `RowNumberOver` unchanged for back-compat.**

`RowNumberOver` stays exactly as it is in `Ocelot/Tokens/Partitions/RowNumberOver.cs`. It is API surface. The new `WindowedAggregate` token lives next to it under `Ocelot/Tokens/Partitions/` and covers the general case: any aggregate-style SQL function with an `OVER (...)` clause — `COUNT(*) OVER ()`, `SUM(x) OVER (PARTITION BY a ORDER BY b)`, `AVG(x) OVER (PARTITION BY a)`, etc. The single primitive that `ExecutePagedAsync` actually needs is `COUNT(*) OVER ()`; `WindowedAggregate` is the structural answer that subsumes that one case while leaving room for `SUM`/`AVG`/`MIN`/`MAX`/`RANK` later without re-litigating the design.

### 2.2 Why generic, not sibling classes (`CountOver`, `SumOver`, …)

- **Sibling classes** mean N near-identical files each carrying the same `OVER (PARTITION BY … ORDER BY …)` rendering logic. Every new aggregate is a new class plus a corresponding sibling token — combinatorially cheap to add but each one is a separate place where dialect quirks could regress independently.
- **Generic `WindowedAggregate(aggregateExpression, partitionBy?, orderBy?)`** centralizes the `OVER (…)` rendering in one token. Aggregates are themselves expressed as `IDBField`/`ISqlToken` arguments (a column, a constant, `*`, a `DB.Count()` token, etc.). Adding a new aggregate is then *just data* — no new class.
- **Trade-off acknowledged:** the generic shape is slightly less self-documenting at the call site. `new RowNumberOver(orderBy)` reads better than the most generic `WindowedAggregate(...)`. We mitigate by adding small static factories on `DB` (see §2.4) so the *call site* is short and named (`DB.CountOver()`, `DB.SumOver(field)`), with the *implementation* unified.
- **Why not refactor `RowNumberOver` away?** Two reasons. First, breaking change — `RowNumberOver` is public API. Second, `ROW_NUMBER()` has no value-bearing inner expression (it is `ROW_NUMBER()`, not `ROW_NUMBER(x)`), and an `ORDER BY` is effectively required for it to be deterministic; modelling it as a generic aggregate-with-optional-order would weaken its constraints. Keep it as the specialized form. `WindowedAggregate` covers everything else.

### 2.3 Skeleton (signatures only)

`WindowedAggregate : SqlToken` carries:

- An aggregate expression token (the part rendered before `OVER`). Conceptually the aggregate itself — `COUNT(*)`, `SUM(x)`, `AVG(x)`, etc. Modeled as an `ISqlToken` so it can be a `DB.Count()`, a column, a constant, or anything the existing token system already understands.
- An optional partition-by field (`IDBField`).
- An optional order-by criterion (`OrderByCriteria`).
- An optional alias string (so the projection column can be named, e.g. the special `__total` column).
- Override of `ToSql(IDBInfo, IOperationPreparator, Func<Type, EntityDescriptor>, string tablealias)` — emits `<aggregate> OVER (PARTITION BY <partitionBy> ORDER BY <orderBy>) [AS <alias>]`. PARTITION BY / ORDER BY emitted only when set; empty `OVER ()` is the form `COUNT(*) OVER ()` requires.

The alias handling is the only subtle bit: the existing `IOperationPreparator` field path (`AppendField`) handles `AS <alias>` for column-style fields via existing alias tokens (`AliasField`/equivalent). `WindowedAggregate` can either compose with the existing alias mechanism (preferred — wrap in `DB.As(...)` at the call site) or emit `AS <alias>` directly when its own optional alias is set. Pick whichever matches the dominant pattern for `RowNumberOver`-with-alias usage in the existing codebase; both are acceptable. The implementer should mirror the alias convention they observe in `RowNumberOver` callers.

### 2.4 Static factories on `DB`

Add to `Ocelot/Tokens/DB.cs`:

- `DB.CountOver()` — returns `WindowedAggregate(<count-star>)`. The 95% case for paged execution.
- `DB.CountOver(IDBField partitionBy = null, OrderByCriteria orderBy = null)` — explicit form.
- `DB.SumOver(IDBField field, IDBField partitionBy = null, OrderByCriteria orderBy = null)`.
- `DB.AvgOver(...)`, `DB.MinOver(...)`, `DB.MaxOver(...)` — fill in to round out the set.

Only `DB.CountOver()` is strictly required by `ExecutePagedAsync`. The others are cheap to add given the underlying generic token; ship them so callers don't need a sibling-class follow-up.

### 2.5 Component diagram

```
                 ┌────────────────────────────────────────┐
                 │            Ocelot/Tokens/              │
                 │                                        │
                 │  DB  ────────────────────────┐         │
                 │   ├─ CountOver() ────────┐   │         │
                 │   ├─ SumOver(...) ────┐  │   │         │
                 │   └─ ...              ▼  ▼   ▼         │
                 │                  ┌──────────────────┐  │
                 │   Partitions/    │ WindowedAggregate│  │
                 │   ├─ RowNumberOver  (new)         │  │
                 │   │   (unchanged)    : SqlToken   │  │
                 │   └─ WindowedAggregate ─ ToSql ───┘  │
                 │                                        │
                 └──────────────────────┬─────────────────┘
                                        │ writes via preparator
                                        ▼
                          Ocelot/Entities/Operations/Prepared
                                  PreparedLoadOperation<T>
                                  ExecutePagedAsync (new) — §3
```

`WindowedAggregate` is dialect-agnostic at the C# level. The SQL it emits is portable to all four engines that target their respective minimum versions (§5).

---

## 3. `ExecutePagedAsync` — API & Semantics

### 3.1 Final signatures

Methods added to `PreparedLoadOperation<T>` (the typed variant; the untyped `PreparedLoadOperation` does not need it because `T` is required to project entities):

```
Task<PagedResult<T>> ExecutePagedAsync(
    int limit,
    int offset,
    CancellationToken cancellationToken = default);

Task<PagedResult<T>> ExecutePagedAsync(
    Transaction transaction,
    int limit,
    int offset,
    CancellationToken cancellationToken = default);
```

Mirrored on `LoadOperation<T>` as thin pass-throughs (matching the existing pattern where every `PreparedLoadOperation<T>.ExecuteXxx` has a `LoadOperation<T>.ExecuteXxx` that calls `Prepare(false)` first):

```
Task<PagedResult<T>> ExecutePagedAsync(int limit, int offset, CancellationToken cancellationToken = default);
Task<PagedResult<T>> ExecutePagedAsync(Transaction transaction, int limit, int offset, CancellationToken cancellationToken = default);
```

Why these signatures and not others:

- **`Task<PagedResult<T>>`, not `PagedResult<T>` directly.** Returning the result inside a `Task` lets the implementation `await` the *first row* of the underlying reader before handing the consumer the result. This is what makes `Total` resolvable on multi-connection dialects without forcing the consumer to drain the stream first (see §3.3, §6).
- **`limit`, `offset` as required parameters.** Paged execution without a limit is semantically incoherent — there's no page. The existing fluent `.Limit(...)` / `.Offset(...)` are still allowed on `LoadOperation<T>` but `ExecutePagedAsync(limit, offset, ct)` overrides them. Document explicitly: any prior `.Limit()`/`.Offset()` is replaced. (Implementation: in the `LoadOperation<T>.ExecutePagedAsync` pass-through, call `Limit(limit).Offset(offset)` before `Prepare(false)`.)
- **`int`, not `long`.** Page sizes and offsets that overflow int are not realistic for HTTP pagination. The existing `Limit(long)` is a fluent setter; the paged primitive constrains to int because that's the practical envelope.
- **`CancellationToken` last, defaulting to `default`.** Standard .NET convention. Required by §4.
- **`Transaction transaction` first when present.** Matches every other `Execute*Async` overload pair in `PreparedLoadOperation`.

No CT-less overload. The CT defaults to `default(CancellationToken)`, so callers that don't care just don't pass one. The existing pattern (`ExecuteEntitiesAsync(params object[] parameters)` with optional parameters) cannot be cleanly extended with a CT because `params object[]` greedily eats positional arguments. `ExecutePagedAsync` avoids this by declaring `limit`/`offset` as named ints, so the CT slot is unambiguous.

### 3.2 `PagedResult<T>` — return type

A new class (not a record — `netstandard2.1` supports records, but Ocelot's existing API style uses plain classes), under `Ocelot/Entities/Operations/Prepared/PagedResult.cs`:

| Member | Type | Semantics |
|---|---|---|
| `Items` | `IAsyncEnumerable<T>` | The streamed page. Consumer iterates with `await foreach`. Each element is a fully constructed `T` (same projection logic as `ExecuteEntitiesAsync<T>`). |
| `Total` | `Task<long>` | The matching total row count (after WHERE, before LIMIT/OFFSET). Resolved exactly once per `ExecutePagedAsync` call without a second SQL round trip. Resolution timing: §3.3. |

A small class wins over a tuple for three reasons:
1. It's discoverable in IntelliSense — `paged.Total` reads like the DiVoid task spec; `paged.Item2` does not.
2. It survives evolution. If we later add `paged.PageNumber`, `paged.HasMore`, etc., extending the class is non-breaking; extending a tuple is.
3. The DiVoid integration code (`AsyncPageResponseWriter<T>`) already takes `Func<Task<long>>` for the total — `() => paged.Total` is a one-liner against a class with named members.

`PagedResult<T>` should expose `Items` and `Total` as get-only properties. Construction is internal — only `PreparedLoadOperation<T>` instantiates it. No public constructor.

### 3.3 Total resolution timing

**Resolved on the arrival of the first row, via a `TaskCompletionSource<long>` that the streaming reader sets.**

Mechanism (described in prose, no code):

1. `ExecutePagedAsync` builds the SQL by injecting a windowed-count column into the projection — specifically, `COUNT(*) OVER () AS __total` (the alias is fixed and reserved; see §3.6). It does this by adding a `WindowedAggregate` field to the column list of an internal copy of the operation before calling `Prepare(false)`.
2. The SQL is executed via `IDBClient.ReaderAsync` (or `ReaderPreparedAsync`) with the CT. The returned `Reader` is held inside the `PagedResult`.
3. A `TaskCompletionSource<long>` (call it `totalTcs`) is created. `PagedResult.Total = totalTcs.Task`.
4. The async-iterator that backs `PagedResult.Items` is wrapped: on the first row read, it extracts the value of the `__total` column, calls `totalTcs.TrySetResult(value)`, then yields the row (with the `__total` column hidden — see §3.6). On every subsequent row, it just yields. On stream end:
   - If `totalTcs` is still unset (zero-row result), set it to `0`. *Why this is correct without a second query:* a windowed count over an empty filtered set returns no rows at all (the `COUNT(*) OVER ()` is computed per-row, and there are no rows). The total *is* zero by construction. No second statement is needed.
   - Dispose the `Reader`.
5. On exception during reader read: propagate the exception to both `totalTcs` (`TrySetException`) and the async iterator. Consumers awaiting `Total` see the same failure as consumers iterating `Items`.
6. On `CancellationToken` cancellation mid-stream: same — both `Total` and the async iterator observe `OperationCanceledException`.

This works on every dialect the same way because `COUNT(*) OVER ()` returns the same total value on every row. Reading row 1 is sufficient.

### 3.4 Edge cases

| Edge case | Behavior |
|---|---|
| Zero-row result | `Total` resolves to `0`. `Items` completes without yielding. No second SQL statement. |
| Partial stream consumption (consumer breaks out of `await foreach` early) | `Total` already resolved on row 1, so it stays resolved. The implementation must dispose the underlying `Reader` (and release the SQLite semaphore on `LockedDBClient`) when the async iterator's enumerator is disposed. The standard `IAsyncEnumerator<T>.DisposeAsync()` path covers this if we propagate properly through the nested `using`. |
| Consumer never reads any row | `Total` never resolves. *This is the user's bug — they didn't iterate.* But to be defensive: if `PagedResult.Total` is awaited *before* iteration begins, the implementation must also start pulling. **Decision: kick off reading at construction time.** The first row read happens eagerly inside `ExecutePagedAsync` itself, before the `Task<PagedResult<T>>` returns. That row is buffered as the head of the stream; `Total` is set during that eager read. This guarantees `await paged.Total` works even if `Items` is never iterated. It also makes the awaited shape of `Task<PagedResult<T>>` meaningful — by the time the task completes, the stream has begun. |
| Connection drop mid-stream | The exception surfaces on the next `Read` (or `ReadAsync`) the consumer triggers. `Total` is already set if drop happened after row 1. If drop happens before row 1, `totalTcs` and the iterator both fault with the underlying `DbException`. |
| Cancellation via the CT | Same as connection drop — the underlying ADO.NET command observes the CT and cancels. `Total` and `Items` both fault with `OperationCanceledException`. |
| Caller passes a CT that's already canceled | `ExecutePagedAsync` short-circuits before opening a connection. `Task<PagedResult<T>>` returns a faulted task. |
| Caller passes `limit < 0` or `offset < 0` | Throw `ArgumentOutOfRangeException` synchronously (before returning the task). The existing `Limit(long)` accepts any value silently — we tighten only at the paged primitive. |
| Operation has no `WHERE` and a giant table | Not our problem. Same as today. The windowed count is no more expensive than `SELECT COUNT(*)` would have been, and it's coupled with the page rather than redundant. |

### 3.5 Transaction parameter

Mirror the existing `Execute*Async(Transaction, ...)` pattern. Pass-through to the underlying `IDBClient.ReaderAsync(transaction, ...)` call. No special semantics. If the consumer wants to wrap the paged read in a transaction with other operations, they supply one; if not, default `null`.

### 3.6 The reserved `__total` column

The injected windowed-count column needs an alias the reader can pick out. Use `__total` (double underscore + "total"). Why not just look at column index = last? Because the projection may include other windowed/computed columns, and the column-to-property mapper in `CreateObjectsAsync<T>` matches by *name* (`reader.GetName(i)` against `descriptor.TryGetColumn(columnname)`). An entity property named `__total` is not going to exist, so the existing mapper will silently skip the column (the `setters[i]` becomes null). That is the correct behavior — `__total` does not become a property of `T`, it just feeds `Total`.

The implementation reads `__total` by name (`reader.GetOrdinal("__total")` once on first row, cache the ordinal) rather than by position. This survives any future change in projection ordering.

If the consumer's entity *actually has* a property named `__total` (vanishingly unlikely in real schemas), the mapper would attempt to set it. Documented limitation: the alias `__total` is reserved by `ExecutePagedAsync`. Mention in the docstring and in `docs/architecture/`.

### 3.7 Non-windowed fallback?

**No.** The DiVoid task spec argues against it; we confirm. The whole point of `ExecutePagedAsync` is the single-statement property. A "fallback" that runs two queries is just `ExecuteEntitiesAsync` plus a manual count query — the consumer can do that themselves with the existing API. Document: if a dialect cannot execute the windowed count (see §5), `ExecutePagedAsync` throws — it does not silently degrade.

---

## 4. CancellationToken Plumbing Strategy

### 4.1 Scope of the change

`IDBClient` does not currently accept any `CancellationToken` on any method. The existing async path in `DBClient` calls `command.ExecuteReaderAsync()`, `ExecuteScalarAsync()`, etc. without a token — so even if a higher layer had a CT, it could not propagate it to ADO.NET. This is the gap DiVoid 119 surfaces.

The dialect classes (`SQLiteInfo`, `PostgreInfo`, `MySQLInfo`, `MsSqlInfo`) do **not** participate in CT plumbing. They live in `Ocelot/Info/` and own SQL emission; they do not own command execution. This significantly simplifies the change: there is exactly one concrete `IDBClient` (`DBClient`) plus one wrapper (`LockedDBClient`). The CT plumbing reaches every engine for free, because every engine goes through the same `DBClient.ReaderAsync` / etc.

### 4.2 What gets new overloads

#### 4.2.1 `IDBClient` interface

Add CT-bearing async overloads for every async method that ultimately invokes an ADO.NET `*Async`. Concretely:

| Existing method | New CT overload |
|---|---|
| `Task<int> NonQueryAsync(string, params object[])` | `Task<int> NonQueryAsync(string, IEnumerable<object>, CancellationToken)` |
| `Task<int> NonQueryAsync(Transaction, string, IEnumerable<object>)` | `Task<int> NonQueryAsync(Transaction, string, IEnumerable<object>, CancellationToken)` |
| `Task<DataTable> QueryAsync(...)` (4 overloads) | `Task<DataTable> QueryAsync(..., CancellationToken)` (paired) |
| `Task<object> ScalarAsync(...)` (4 overloads) | `Task<object> ScalarAsync(..., CancellationToken)` (paired) |
| `IAsyncEnumerable<object> SetAsync(...)` (4 overloads) | `IAsyncEnumerable<object> SetAsync(..., CancellationToken)` (paired) |
| `Task<Reader> ReaderAsync(...)` (4 overloads) | `Task<Reader> ReaderAsync(..., CancellationToken)` (paired) |
| All `*PreparedAsync` equivalents of the above | Same pattern |

In practice add **one** new CT overload per existing async overload that has the canonical `(Transaction, string, IEnumerable<object>)` signature, and let the convenience overloads (`params object[]`, no-transaction) forward to it. Keep the convenience-overload count manageable — a single canonical CT overload per family is sufficient if the existing convenience overloads stay CT-less. *This is the recommended path:* one new method per family, accepting `(Transaction transaction, string, IEnumerable<object>, CancellationToken)`. Callers wanting a CT explicitly call this canonical form.

Rationale for "one CT overload per family, canonical form only":

- Adding CT overloads for every existing convenience overload doubles the interface surface.
- Existing CT-less calls keep working unchanged.
- New code that wants a CT calls the canonical form, which is exactly the form `PreparedLoadOperation` uses internally.
- This is a deliberate trade-off: callers using `client.QueryAsync("SELECT 1")` don't get a CT overload, only `client.QueryAsync(transaction, "SELECT 1", parameters, ct)` does. Acceptable — the convenience overloads are meant for ad-hoc usage where CT support is rarely the priority.

#### 4.2.2 `ADbClient` abstract base

Mirror the new abstract methods on `ADbClient` (the base class `DBClient` and `LockedDBClient` derive from). The non-CT overloads can become `virtual` methods that default-implement by calling the CT overload with `CancellationToken.None`. That way subclasses only need to override the CT-bearing form.

#### 4.2.3 `DBClient` concrete

Override the CT-bearing methods. Each implementation passes the CT to the underlying ADO.NET method:

- `command.ExecuteNonQueryAsync(cancellationToken)`
- `command.ExecuteReaderAsync(cancellationToken)`
- `command.ExecuteScalarAsync(cancellationToken)`

Wrap in the existing `try/catch StatementException` pattern unchanged. `OperationCanceledException` should propagate untouched — *do not* wrap it in `StatementException`. The catch block is currently `catch (Exception e)`; tighten to `catch (OperationCanceledException) { throw; } catch (Exception e) { throw new StatementException(...); }` or equivalent.

For the `SetAsync` family that uses the buffer-when-single-connection pattern: pass the CT into both the `ExecuteReaderAsync` call *and* the buffering logic. The existing `Buffer()` extension is presumably synchronous-collect; either give it a CT-aware overload or check the CT inside the loop that fills the buffer. Recommended: thread the CT through `ReadSetAsync` and check `cancellationToken.ThrowIfCancellationRequested()` in the read loop before each `await reader.ReadAsync()`.

#### 4.2.4 `LockedDBClient` wrapper

Override the same CT methods. For each one, the pattern is identical to today plus one small change: pass the CT into the `connectionlock.WaitAsync(cancellationToken)` call as well. This ensures a caller waiting for the SQLite semaphore can be canceled while waiting, not just while executing.

```
(prose, not code)
LockedDBClient.QueryAsync(null, query, params, ct):
    await connectionlock.WaitAsync(ct);   // <-- CT here too
    try { return await baseclient.QueryAsync(null, query, params, ct); }
    finally { connectionlock.Release(); }
```

For the `ReaderAsync` family that moves the semaphore release responsibility onto the `Reader` itself: same pattern — `WaitAsync(ct)`, then call the base with the CT. The Reader continues to release the semaphore on dispose. If the CT cancels *after* `WaitAsync` returns but *before* `baseclient.ReaderAsync` returns, the existing `catch { connectionlock.Release(); throw; }` block already covers it.

#### 4.2.5 Consumers — `PreparedLoadOperation` and `PreparedLoadOperation<T>`

Add CT overloads to:

- `ExecuteAsync(...)` (returns `DataTable`)
- `ExecuteScalarAsync<TScalar>(...)`
- `ExecuteSetAsync<TScalar>(...)`
- `ExecuteTypesAsync<TType>(...)` (and `ExecuteTypeAsync`)
- `ExecuteEntitiesAsync<T>(...)` (and `ExecuteEntityAsync`)
- `ExecuteReaderAsync(...)`

Pattern: each new overload takes a `CancellationToken` last (default `default`) and forwards into the corresponding new CT-bearing `IDBClient` method. Internal helpers (`CreateObjectsAsync`, `ToObjectAsync`) accept and observe the CT — particularly checking it before each `await reader.ReadAsync()` inside the row loop, so a cancellation interrupts the reader iteration promptly.

Mirror these on `LoadOperation<T>` as pass-throughs (the existing thin-pass-through pattern: `Prepare(false).ExecuteEntitiesAsync(transaction, parameters, ct)`).

`ExecutePagedAsync` (defined in §3) already takes a CT and uses the new CT-bearing `IDBClient.ReaderAsync`.

### 4.3 What does NOT change

- Sync methods (`Query`, `Scalar`, `Set`, `NonQuery`, etc.) — no CT; cancellation does not apply to synchronous ADO.NET calls.
- Dialect classes (`Ocelot/Info/*Info.cs`) — they emit SQL, they do not execute it.
- Existing async signatures — every existing overload keeps working unchanged.
- The `Transaction` API — no CT on `Begin`/`Commit`/`Rollback` in this pass. Out of scope; a later task can add it if anyone needs it. The existing usage is short-blocking enough that the DB's connection-level cancellation handles the runaway case.

### 4.4 Behavior contract under cancellation

When a CT fires:
- An `OperationCanceledException` (or `TaskCanceledException`, the runtime decides) propagates from the awaiting call.
- It is **not** wrapped in `StatementException`. `StatementException` carries SQL-error context; cancellation is not a SQL error.
- The underlying connection / reader is disposed by the existing `using` / `try-finally` blocks.
- For the `LockedDBClient`, the semaphore is released regardless.
- For `ExecutePagedAsync`, both `PagedResult.Total` and `PagedResult.Items` observe the cancellation.

---

## 5. Dialect Compatibility Table

Feature: `COUNT(*) OVER ()` (the windowed count `ExecutePagedAsync` injects).

| Dialect | Min version with full window-function support | Status of *all four* engines we target | Behavior on unsupported version |
|---|---|---|---|
| SQLite | 3.25.0 (Sep 2018) | `Microsoft.Data.Sqlite` shipped with .NET 8/9 bundles SQLite ≥ 3.43 — far past 3.25. Test rig is in-memory SQLite via the same library; supported. | N/A in any realistic deployment. If a consumer somehow loads an older native SQLite, the SQL parser will reject `OVER ()` with a syntax error — surface as `StatementException` (current behavior for any unsupported SQL). No special detection. |
| PostgreSQL | 8.4 (2009) | Universally supported — Postgres versions in the wild that don't support window functions are 17+ years old. | N/A. |
| MySQL | 8.0 (2018) | **Risk surface.** MySQL 5.7 and MariaDB < 10.2 do *not* support window functions. MariaDB 10.2+ (Apr 2017) supports them. | `ExecutePagedAsync` must throw a clear `NotSupportedException` (or a domain-specific `WindowFunctionNotSupportedException` deriving from it) up-front when the dialect is `MySQLInfo` AND a runtime version probe reports < 8.0 / < MariaDB 10.2. See §5.1. |
| MSSQL | 2005 (2005) for `ROW_NUMBER`/`OVER`, full windowing in 2012+; `COUNT(*) OVER ()` works from 2005 onward. | Universally supported. | N/A. |

### 5.1 MySQL/MariaDB version handling

Two choices:

1. **Always emit, let the server reject.** Simplest — if the user is running 5.7, they get a `StatementException` wrapping the MySQL syntax error. Same as any other unsupported SQL.
2. **Detect and throw a typed exception up-front.** Better DX — `WindowFunctionNotSupportedException("MySQL 5.7 does not support window functions; COUNT(*) OVER () requires MySQL 8.0+ or MariaDB 10.2+. Use ExecuteEntitiesAsync with a separate count query instead.")`.

**Recommendation: option 1 for v1.** Reasons:

- Detection requires probing the server version, which is itself a round trip and a stateful piece of dialect knowledge that doesn't exist in the codebase today (`IDBInfo` does not currently expose a version-probe method).
- Adding it just for this feature inflates scope. The error message a MySQL 5.7 server returns for `COUNT(*) OVER ()` is "You have an error in your SQL syntax… near 'OVER()' at line 1" — clear enough.
- Document the minimum version requirement in the `ExecutePagedAsync` XML docstring.
- If a downstream consumer demonstrates they actually need the typed exception (i.e., they actually run on MySQL 5.7 in production), file a follow-up task to add `IDBInfo.SupportsWindowFunctions` (a `bool` property defaulting to `true` on SQLite/Postgres/MSSQL, set conditionally on MySQL based on a one-time version probe at connection setup).

State this trade-off explicitly in the `ExecutePagedAsync` XML doc: "Requires the underlying database to support `COUNT(*) OVER ()`. SQLite 3.25+, PostgreSQL 8.4+, MSSQL 2005+, MySQL 8.0+ / MariaDB 10.2+. Older engines will fail with a `StatementException` wrapping a SQL-syntax error."

### 5.2 Window-function token in general

`WindowedAggregate.ToSql` emits standard SQL window-function syntax. All four target engines (at the minimum versions above) parse it identically. There is no per-dialect override needed. The token does not register through `DBInfo.AddFieldLogic<T>` — it overrides `ToSql` directly, same as `RowNumberOver`. No dialect class needs to be touched for §2.

---

## 6. SQLite Buffering Interaction

The constraint: `IDBInfo.MultipleConnectionsSupported = false` for SQLite. The existing `ExecuteEntitiesAsync<T>` (in `PreparedLoadOperation`) handles this by buffering the entire stream into a `List<T>` before yielding any element to the consumer. Inside `DBClient.SetAsync` the same idea uses `AsyncEnumerableExtensions.Buffer()`.

Question: under buffering, can `Total` resolve before the consumer sees the first element of `Items`?

**Answer: yes, because the buffering happens internally — `Total` is set during the buffer-fill loop, which runs inside `ExecutePagedAsync` before the `PagedResult` is observable to the caller.**

Concretely, on SQLite:

1. `ExecutePagedAsync` calls `IDBClient.ReaderAsync(transaction, sql, params, ct)`. Under `LockedDBClient` this acquires the semaphore and returns a `Reader`.
2. The eager-first-row read described in §3.4 reads row 1 (if any), captures `__total`, sets `totalTcs`. This happens before `ExecutePagedAsync`'s outer `Task<PagedResult<T>>` completes.
3. Now we have a choice:
   - **Option A (buffer all rows immediately):** read all remaining rows, build a `List<T>`, release the semaphore. `PagedResult.Items` is then a wrapper around the in-memory list. Consumer sees fully-resolved `Items` from the start. `Total` resolved during step 2. *Trade-off:* memory cost = full page in RAM, plus we hold the semaphore until step 3 completes; but a paginated page is by definition bounded (limit ≤ a few hundred typically), and we already do this for `ExecuteEntitiesAsync<T>`.
   - **Option B (yield from the held reader, semaphore lives on the iterator):** the consumer's `await foreach` directly drives `Reader.ReadAsync`, and the semaphore is released when the iterator disposes. `Total` still resolved in step 2. *Trade-off:* the SQLite semaphore is held across the consumer's entire iteration — if the consumer is slow, every other DB operation waits.

   **Recommendation: Option A on SQLite.** Match the existing `ExecuteEntitiesAsync<T>` behavior under `!MultipleConnectionsSupported`. The semaphore is held only for the duration of the eager drain, which is the same cost the consumer already pays for `ExecuteEntitiesAsync<T>`. Option B introduces a new failure mode (slow consumer starves the DB) that doesn't exist today.

4. On multi-connection dialects (Postgres, MySQL, MSSQL): take Option B's shape. The reader streams; `Total` was already set on row 1; the consumer sees rows arrive one at a time. The reader is released when the iterator disposes.

So: **`PagedResult.Total` resolves before `Task<PagedResult<T>>` completes, on all four dialects**, because we always read row 1 eagerly. `PagedResult.Items` is fully buffered on SQLite and live-streamed on Postgres/MySQL/MSSQL.

This makes the consumer story uniform: `await ExecutePagedAsync(...)` → `paged.Total` is immediately awaitable, `paged.Items` is iterable. Behavior parity across dialects, with only the streaming-vs-buffered detail differing internally.

### 6.1 Ordering of eager reads under cancellation

If the CT fires *during* the eager first-row read on SQLite, the semaphore is held. The implementation must:
1. Catch the cancellation in the eager-read region.
2. Dispose the reader.
3. Release the semaphore (which the reader's dispose path already handles).
4. Set `totalTcs.TrySetException(operationCanceledException)`.
5. Re-throw so the outer `Task<PagedResult<T>>` faults.

The existing `LockedDBClient.ReaderAsync` already handles release-on-throw for the connection acquisition; the eager-read block needs the same defensive pattern around its own work.

---

## 7. Test Plan

Test files mirror the existing project structure under `Ocelot.Tests/`. NUnit 3, parallel-safe. SQLite in-memory by default; Postgres-gated by `POSTGRES_CONNECTION` env var.

### 7.1 New test files

| File | Purpose |
|---|---|
| `Ocelot.Tests/Tokens/WindowedAggregateTests.cs` | Unit tests for the new token's SQL emission. Pure-text assertion against `IOperationPreparator` output for each of: `COUNT(*) OVER ()`, `COUNT(*) OVER (PARTITION BY x)`, `SUM(x) OVER (PARTITION BY a ORDER BY b)`, with and without an alias. Verifies the empty `OVER ()` form for the windowed-count case. |
| `Ocelot.Tests/Tokens/RowNumberOverBackCompatTests.cs` *(if not already covered)* | Smoke test that existing `RowNumberOver` usage still emits the expected SQL — guards against regressions from the §2 work. Skip if equivalent coverage exists. |
| `Ocelot.Tests/Operations/ExecutePagedAsyncTests.cs` | End-to-end against in-memory SQLite. Test cases: zero rows, one row, exactly-one-page, partial-page, empty stream after partial consumption, `Total` matches a separate `SELECT COUNT(*)` for the same predicate, eager `await Total` before iterating `Items` works, cancellation fires before iteration, cancellation fires mid-iteration, `limit < 0` throws, `offset < 0` throws. |
| `Ocelot.Tests/Operations/ExecutePagedAsyncSingleStatementTests.cs` | The "exactly one statement executed" assertion. Mock `IDBClient` (or use a counting decorator over the real one) — track calls into `ReaderAsync` / `ReaderPreparedAsync`. Run an `ExecutePagedAsync`; assert exactly one underlying reader call. This is the load-bearing assertion that protects the single-query property over time. |
| `Ocelot.Tests/Postgres/PostgresExecutePagedAsyncTests.cs` | Postgres integration. Same scenarios as the SQLite end-to-end file. Gated by `POSTGRES_CONNECTION` (`Assert.Inconclusive` when missing, per the existing convention). Critically, this is where multi-connection streaming behavior gets validated — the SQLite tests can't expose that. |
| `Ocelot.Tests/Clients/CancellationTokenPlumbingTests.cs` | Verify CT propagates through `IDBClient.ReaderAsync(... ct)` to the underlying ADO.NET command. Test by passing a pre-canceled CT and asserting `OperationCanceledException` (not `StatementException`). Test by canceling mid-`await foreach` and asserting both `Items` and a separately-awaited `Total` observe the cancellation. |
| `Ocelot.Tests/Clients/LockedDBClientCancellationTests.cs` | Verify the SQLite semaphore is released on cancel-during-wait. Spin up two concurrent `ExecutePagedAsync` on the same `LockedDBClient`; cancel the second's CT while it's blocked on the semaphore; assert the second faults with `OperationCanceledException` and the semaphore is releasable for a third operation immediately. |

### 7.2 Specific test patterns

**Single-statement assertion (the load-bearing one):**
- Wrap a real `IDBClient` in a counting decorator that increments a counter on each `*Async` and `*PreparedAsync` call.
- Run `ExecutePagedAsync(limit: 10, offset: 0)` with `await foreach` over the items.
- Assert the counter reads exactly 1 (either `ReaderAsync` or `ReaderPreparedAsync`, depending on `DBPrepare`).
- Repeat with zero-row predicate — counter still reads exactly 1.

**Total-correctness:**
- Insert N rows.
- Run `ExecutePagedAsync(limit: K, offset: 0)` where K < N.
- Assert `paged.Total == N`, `paged.Items.Count() == K`.
- Repeat with offset > 0 and offset > N.

**`__total` reservation:**
- Test that an entity with no `__total` property maps cleanly (the column is dropped on the floor). This is the default case.
- Optionally: test that an entity with a `__total` property still works but the property is overwritten with the windowed value (acceptable, document as the reserved-alias caveat).

**Existing test coverage to extend:**
- Add CT overload calls to a sampling of the existing `*Async` test paths — verify the CT-less and CT-bearing overloads return the same results when the CT is `default`.

---

## 8. Out of Scope

- **DiVoid-side adoption.** Rewiring `NodeService.ListPaged`, replacing the `total: -1` sentinel from DiVoid 122 — that's the DiVoid task, runs after this Pooshit version ships.
- **`AsyncPageResponseWriter<T>` changes.** Belongs to `Pooshit.AspNetCore.Services`. The DiVoid task spec confirms no change is needed there for the windowed-count pattern.
- **`nototal=true` opt-out semantics.** DiVoid 122. The Ocelot-side answer is "if you don't want a total, call `ExecuteEntitiesAsync` instead of `ExecutePagedAsync`" — no special API. The DiVoid-side response shape is DiVoid's problem.
- **Cursor-based pagination.** DiVoid 118 considered and deferred this. No work here.
- **`IDBInfo.SupportsWindowFunctions` runtime probe.** Not adding for v1 (§5.1). Filed as a follow-up if MySQL 5.7 deployment ever materializes.
- **Sync `ExecutePaged`.** No sync variant. Pagination is a network-bound operation; consumers should be async. Adding sync is trivial later if anyone asks.
- **CT on sync methods.** Not applicable.
- **CT on `Transaction.Commit/Rollback`.** Out of scope (§4.3).
- **New aggregate factories beyond `CountOver`/`SumOver`/`AvgOver`/`MinOver`/`MaxOver`.** `RankOver`, `DenseRankOver`, `LagOver`, `LeadOver` etc. are easy to add given `WindowedAggregate` exists, but they're not blocking anything. Defer.
- **Refactoring `RowNumberOver` onto `WindowedAggregate`.** Public API; not worth the breaking-change risk.
- **`LimitField` redesign.** The existing `LoadOperation<T>.Limit/Offset` API stays; `ExecutePagedAsync` just sets them internally before preparing.

---

## 9. Open Questions

None. The decisions above are deliberate. If the implementer finds something genuinely ambiguous mid-implementation, raise it as a follow-up rather than guessing — but the design is intentionally complete.

---

## 10. Implementation Guidance for the Next Agent

Recommended build order. Each phase is independently testable and PR-able if you wanted to split, but the sibling-task dependency in DiVoid (134 needs all three pieces) means you'll likely ship them in one branch. Keep one implementation branch off `master`, one PR.

1. **Phase 1 — Window-function token (§2).**
   - Create `Ocelot/Tokens/Partitions/WindowedAggregate.cs`.
   - Add `DB.CountOver()`, `DB.SumOver(...)`, etc. to `Ocelot/Tokens/DB.cs`.
   - Write `WindowedAggregateTests.cs`.
   - This phase has zero dependency on the others — verify it green before moving on.

2. **Phase 2 — CT plumbing on `IDBClient` (§4.2.1 – §4.2.4).**
   - Extend `IDBClient` with one canonical CT overload per family (the `(Transaction, string, IEnumerable<object>, CancellationToken)` shape).
   - Mirror in `ADbClient` (default the non-CT form to call CT form with `CancellationToken.None`).
   - Implement in `DBClient` (pass CT to ADO.NET methods; let `OperationCanceledException` skip the `StatementException` wrap).
   - Implement in `LockedDBClient` (pass CT to `WaitAsync` and to the inner client call).
   - Write `CancellationTokenPlumbingTests.cs` and `LockedDBClientCancellationTests.cs`.

3. **Phase 3 — CT overloads on `PreparedLoadOperation` / `LoadOperation` (§4.2.5).**
   - Add CT overloads to each `Execute*Async` method; route through the new `IDBClient` CT methods.
   - Thread the CT into the row-iteration loops inside `CreateObjectsAsync<T>` and friends — `ThrowIfCancellationRequested` before each `await reader.ReadAsync()`.
   - Mirror on `LoadOperation<T>` as pass-throughs.
   - Light tests verifying the new overloads behave identically to the existing ones when CT is `default`.

4. **Phase 4 — `ExecutePagedAsync` (§3).**
   - Create `Ocelot/Entities/Operations/Prepared/PagedResult.cs`.
   - Add `ExecutePagedAsync` to `PreparedLoadOperation<T>` (only the typed variant). Build the SQL by injecting a `WindowedAggregate` (`COUNT(*) OVER () AS __total`) into a copied operation's column list before `Prepare(false)`. Use the new CT-bearing `IDBClient.ReaderAsync` / `ReaderPreparedAsync`.
   - Implement eager-first-row read + `TaskCompletionSource<long>` for `Total`.
   - SQLite path: buffer-all-rows under `!MultipleConnectionsSupported`, release semaphore. Multi-connection path: stream, release on iterator dispose.
   - Add the pass-through on `LoadOperation<T>` that calls `Limit`/`Offset` then prepares.
   - Write `ExecutePagedAsyncTests.cs` and `ExecutePagedAsyncSingleStatementTests.cs`.

5. **Phase 5 — Postgres integration tests (§7.1).**
   - `Ocelot.Tests/Postgres/PostgresExecutePagedAsyncTests.cs`. Gated on `POSTGRES_CONNECTION`. Mirror SQLite scenarios. Especially verify multi-connection streaming behavior.

6. **Phase 6 — Version bump and final review.**
   - Bump `AssemblyVersion`/`PackageVersion` in `Ocelot/Ocelot.csproj`.
   - Sanity-run `dotnet test Ocelot.sln` end-to-end.
   - Update `Readme.md` with a short `ExecutePagedAsync` example. (Confirm with user whether Readme tour update is in scope or filed separately.)

### 10.1 Things to be careful about during implementation

- **Do not mutate the original `LoadOperation<T>` when injecting `__total`.** Build a new column list, construct a new `LoadOperation<T>` (the existing copy-constructor pattern via `internal LoadOperation(LoadOperation<T> origin)` is your friend), or otherwise scope the injection so a caller who calls `ExecutePagedAsync` and then later `ExecuteEntitiesAsync` on the same operation does not accidentally get `__total` in the second query.
- **`OperationCanceledException` must not be wrapped in `StatementException`.** Tighten the catch blocks in `DBClient` async methods.
- **The `Reader` semaphore-release path under `LockedDBClient` is subtle.** When you add CT to `ReaderAsync`, ensure the existing `catch { connectionlock.Release(); throw; }` still fires for `OperationCanceledException`. Test it (`LockedDBClientCancellationTests.cs`).
- **Eager first-row read happens *before* the outer Task completes.** Don't return the `PagedResult` until row 1 is in hand (or zero-row determined). This is what makes the awaited shape meaningful.
- **The `__total` alias is exposed in error messages** (it shows up in `StatementException.CommandText`). Acceptable. Document.
- **`netstandard2.1` constraint.** No `IAsyncEnumerable<T>.WithCancellation` shortcuts that require .NET 6+; use what `Ocelot/Ocelot.csproj` already pulls in. Records can be used (TFM supports them) but plain classes match the existing style — prefer plain classes for `PagedResult<T>`.

---

End of design.
