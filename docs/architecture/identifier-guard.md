# Architectural Document: Identifier Guard (SQL-injection surface on string identifiers)

Repo path: `docs/architecture/identifier-guard.md` (branch `fix/identifier-injection-guard`) — DiVoid documentation node **#14123**. Both copies carry the same bytes; the repo file is canonical once merged.

**Status:** Proposed — revision 3 (2026-09-16; r2 after QA #14141, r3 after QA #14150 APPROVED WITH WARNINGS; changes marked *r2* / *r3*)
**Author:** Sarah (Software Architect)
**Branch / worktree:** `fix/identifier-injection-guard` · `.claude/worktrees/identifier-guard`
**Tracking:** DiVoid task #14114 (umbrella) · audit #14115 (sink inventory + PoCs) · sub-tasks #11258, #3254, #3247
**Load-bearing standards:** Design Contracts #1136 · Code Contracts #114 (§0, §4) · guard discipline #8385 Step 3 · coverage-row rule #1220 §9

---

## TL;DR

**What.** Every caller-supplied SQL identifier (table, column, alias, function, index, constraint, index type — *r2:* and column **type name**) is validated against one fixed grammar where it becomes SQL text; the operation throws `InvalidIdentifierException` otherwise. Two catalog lookups that inline a table name as a `'…'` literal become bound parameters. DDL `DEFAULT` literals are validated. No escaping anywhere.

**How.** One static `IdentifierGuard` with three grammars: *simple* `[A-Za-z_][A-Za-z0-9_]*`, *qualified* (dot-joined simple segments), *r2: type token* (`VARCHAR(255)`, `DOUBLE PRECISION`, `real[]` — bounded to the column-definition slot). `DBInfo.MaskColumn` is a base-class template (validate, then dialect quoting) so every masked column is covered automatically; raw table/alias/function emission sites call the guard inline; `ColumnIndicator` interpolations fold into `MaskColumn`. Model-derived names are validated once, before any statement, at `EntityDescriptor` mutation points and `SchemaService` entry (*r2:* shared per-piece validators; ordering pinned by post-throw catalog tests). The raw hatch survives only as explicit `DataField.Raw(...)`.

**Cost.** SQLite renders entity-operation column names as `[col]` instead of `"col"`; `new DataField("COUNT(*)")` must become `DataField.Raw(...)`; invalid identifiers and (*r2*) invalid SQLite type names now throw where they rendered; *r2:* SQLite `ADD COLUMN` defaults render invariant-culture.

**Rejected.** Delimiter escaping in `MaskColumn`: fixes only the masked class, is dialect-specific, does nothing for raw tables. *r2:* a closed SQLite type map: SQLite has no closed type vocabulary — catalog round-trips (`VARCHAR(255)`) would break.

---

## 0. The ask (verbatim, Toni, 2026-09-16)

> "we have a security gap - some tokens (columntoken for instance) allows to specify a tablename and a columnname as string which is completely unguarded and handed to the query - obvious sql injection surface. Check for all points where this is present and then fix it. The cleanes guard wins here - parameters if type of surface allows for parameters, manual check and throw if it is really just a literal and statement only allows literals."

Policy fixed by the ask (not re-decided here):

1. Anything SQL can bind as a parameter is emitted as a parameter.
2. Identifiers — anything SQL accepts only as a literal token — are validated and the operation throws on an invalid one. Escape-and-hope is not the remedy.

## 1. Problem Statement

Ocelot renders caller-supplied identifier strings into command text through three inconsistent mechanisms — `MaskColumn` (wraps in a delimiter, never escapes it), `ColumnIndicator` interpolation (same, hand-rolled at eleven sites), and raw `AppendText` / `$"…"` (no wrapping at all). All three are bypassable; #14115 executed two injections end to end on SQLite (`Delete(string)` and `Truncate`) and rendered six more. mamgo-backend reaches `DB.Column(string)` from a query-string sort parameter (#11258), so the surface is reachable from the network in production.

Success criteria:

- Every sink in the #14115 inventory (classes A1–A5, B) either binds a parameter or throws on an invalid identifier, before any statement executes.
- Every legitimate shape in #14115 §4 still renders exactly as today.
- A future token or dialect that wants to emit an identifier has exactly one obvious call to make, and a reviewer has exactly one thing to grep for.

## 2. Scope & Non-Scope

**In scope**

- The guard (grammar, exception, placement) and its application to every identifier sink listed in §6, plus the ones found during this design that #14115 did not list (marked *new* in §6).
- Category A2 (table name in a quoted literal) → parameters.
- Category B (column `DEFAULT` literal) → validate-and-throw.
- *r2:* Category A6 — `ColumnDescriptor.Type` emitted raw into SQLite DDL (QA #14141 CF-1) → validate-and-throw with the type-token grammar (§4.2).
- *r3 (forced, pre-existing):* `PostgreInfo.GenerateCreateStatement` loads its template by the stale resource name `NightlyCode.Ocelot.Info.Postgre.createstatement.sql`; row 15's parameterisation cannot be verified while the stream is null, so the name is corrected to `Pooshit.Ocelot.Info.Postgre.createstatement.sql`. This is **#9533 defect 1 only**; defect 2 (the `E '(\n'` on `createstatement.sql` line 1, which Postgres parses as a cast to type `e`) stays with #9533 — the method remains unusable on a live Postgres until that is fixed, and the capturing-mock test cannot see it.
- The `DataField` raw hatch made explicit.
- Folding `ColumnIndicator` interpolations into `MaskColumn`.
- Replacing the ad-hoc check in `SchemaService.RemoveSchema` (`Contains('"') || Contains('[') …`) with the guard.
- The test battery in `Ocelot.Tests/Security/`.

**Out of scope (explicitly)**

- Whole-statement raw SQL APIs: `IDBClient.NonQuery/Scalar/Query(string, …)`, `ViewSchema.Definition`, `[View("…sql")]` embedded resources. These take a *statement*, not an identifier, and are the caller's responsibility by design. They are not identifier sinks.
- Value paths — already parameterized (#14115 Category C).
- Adding quoting to table names, or changing which sinks quote. Rendering stays byte-identical for valid input except where §6 says otherwise.
- Removing `IDBInfo.ColumnIndicator` from the public interface (it becomes unused inside the library; deleting a public member is a separate, breaking change).
- Catalog-derived names (`TableDescriptor` read back from `GetSchema`): the database is trusted for its own catalog.
- The pre-existing bug in `SchemaUpdater.RecreateTable` where the backup-table existence check passes the literal string `"{olddescriptor.Name}{appendix}"` (missing `$`) — filed separately as a DiVoid bug; not touched here.
- Culture-dependent numeric rendering of `DEFAULT` values (`0,5` under de-DE) — pre-existing, unrelated to injection.

## 3. Assumptions & Constraints

- The library targets `netstandard2.1`; the four dialects are `SQLiteInfo`, `PostgreInfo` (full), `MySQLInfo`, `MsSqlInfo` (largely stubs; #3229). The guard must be dialect-independent so the stubs are covered for the members they do implement (`MaskColumn`, `DropTable`, `DropView`, base `Truncate`, `AppendJoin`).
- **No dialect needs a character the grammar rejects.** Verified against every `[Table]`, `[Column]`, `[Index]`, `[Unique]`, `.Alias(…)`, join alias and `DB.Column(…)` literal in `Ocelot/` and `Ocelot.Tests/`: all match `[A-Za-z_][A-Za-z0-9_]*` except the one dotted table `information_schema.columns`. MySQL/Postgres/MSSQL additionally allow `$`, `@`, `#` in unquoted identifiers; nothing in-repo uses them. What would falsify this: a consumer entity declaring such a name — it now throws with the name in the message, and the fix is one character class in one regex.
- Postgres folds unquoted identifiers to lower case; quoted identifiers are case-sensitive. In-repo `[Table("activeData")]`, `[Table("objectIndexDecimal")]` rely on the fold. **This is why the design adds no quoting to table names.**
- Tests run parallel on SQLite in-memory; every test creates its own client (Code Contracts ruling 2026-08-08). Rendered-only assertions on `CommandText` are acceptable for non-SQLite dialects and for sinks whose statement would fail for other reasons.

## 4. Design decisions (the questions the brief left to the architect)

### 4.1 Where the guard sits — the rendering seam, with the column seam made automatic

Four candidate seams were considered:

| Seam | Covers string-built `NonQuery($"…")` sinks? | Knows the identifier's role? | Sees internally composed names (`idx_{t}_{n}`, `{t}_original`, default alias `t`)? | Sites to touch |
|---|---|---|---|---|
| Public API entry (constructors / factories) | no | yes | no | ≈25 |
| Token `ToSql` layer only | no | yes | partly | ≈10, misses schema code |
| Preparator (`AppendIdentifier`) | **no** — a third of the sinks never touch a preparator | yes | yes | ≈25 + rewriting string sinks |
| **Rendering seam: `DBInfo.MaskColumn` template + inline guard where text is emitted** | yes | yes | yes | 1 template + ≈20 inline calls |

The rendering seam wins because it is the only place that (a) every sink already passes through — preparator-based and string-based alike, (b) knows the role of the string (this is about to be a table name in `FROM`), and (c) sees composed names. It is also the place a future author is already editing when they write `AppendText(x)` or `$"… {x} …"`, so the convention *"an identifier string reaches `AppendText` / `NonQuery` only through `IdentifierGuard`"* is one line to review at the point of emission.

Honest limit: no seam makes it structurally impossible for a future token to call `AppendText(callerString)`. What the design gives is (1) exactly one identifier-emitting mechanism for columns (`MaskColumn`, guarded in the base class — a dialect cannot forget it), (2) exactly one guard type to grep for at every other emission, and (3) the coverage table in §10 as the falsifier for the current inventory. A typed `Identifier` value struct was considered and rejected: it would not close that gap either (a token holding a `string` can still exist), and it changes the type of every token/operation field for no additional guarantee.

**Two validation points, each justified once:**

1. **Emission (the guard).** Caller-supplied strings — the string-named public API and every token holding a string — are validated where they become text. For columns this is `MaskColumn`; for tables/aliases/functions/index names it is an inline guard call at the emission site.
2. **Model acceptance (the pre-flight).** Names that live in a model object are validated once when the model is built or accepted: `EntityDescriptor` validates at its own mutation points (table-name set, add column, rename column, add index, add unique) — which covers both attribute discovery in `EntityDescriptor.Create` and every `EntityDescriptorAccess` runtime mutator without touching them; `SchemaService.CreateSchema` / `UpdateSchema` validate the incoming `TableSchema` at entry (*r2:* including each column's `Type`, so a bad type never reaches the recreate path). Emission sites whose only input is a validated model (the entity-typed Load/Insert/Update/Delete operations, `SchemaCreator`, `SchemaUpdater`, `AppendJoin`'s table name) **trust the model and get no inline guard call** — a guard there could never be observed red (#8385 Step 3 question 1) and would be the "trust the invariant *and* assert it" anti-pattern (#1136 §6). `MaskColumn` still validates whatever it receives, model-derived or not, because it is the seam and cannot know its input's origin; that is the seam's contract, not a fallback.

Why the pre-flight exists at all: `SchemaCreator.CreateTable` runs `CREATE TABLE` and then `CREATE INDEX` statements, `SchemaService.RecreateTable` renames the live table before creating the new one, and `UpdateSchema` adds columns one statement at a time — each optionally *without* a caller transaction. A throw on the second statement would leave the schema half-migrated and, on the next run, `CheckIfTableExists` would skip it forever (the soft-lock shape #8385 Step 3 names). Validating the whole model before the first statement is the cheapest way to make the throw happen before any state changes.

*r2 — the ordering property must be pinned, not just stated.* QA #14141 CF-3 removed the `UpdateSchema` pre-flight and the suite stayed green while the SQLite recreate path left only `victim_original` in `sqlite_master`: the single-statement `ADD COLUMN` path used by the original test throws the same exception from `MaskColumn` inside the statement build, so it cannot tell the pre-flight from the emission guard. The tests that discriminate are the ones that drive a **multi-statement** path and assert the **catalog after the throw** (§10 rows *A1 schema DTO — ordering*): the recreate path (an obsolete column on SQLite forces `RecreateTable`) must leave `victim` present and `victim_original` absent; `CreateSchema` with a bad index column must leave no `victim` table at all. Same shape for `EntityDescriptor`: its pre-flight is already pinned by construction, because `EntityDescriptorCache` stores a descriptor only after `Create` returns.

### 4.2 The accepted grammar

Three grammars (*r2:* the third added for column type names), one predicate each, no dialect variation:

| Name | Grammar (as one regex, full match) | Used for |
|---|---|---|
| **simple** | `^[A-Za-z_][A-Za-z0-9_]*$` | column, alias (table alias, join alias, `AS` alias, `DB.Field`), index name, unique/constraint name, index type (`btree`, `gin`) |
| **qualified** | `^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$` — i.e. one or more *simple* segments joined by single dots | table / view name, function name |
| **type token** (*r2*) | `^[A-Za-z_][A-Za-z0-9_]*( [A-Za-z_][A-Za-z0-9_]*)*(\([0-9]+(, ?[0-9]+)*\))?(\[[0-9]*\])?$` — one or more words separated by single spaces, then optionally one parenthesised list of integers, then optionally an array suffix; **null or empty is accepted and emitted as nothing** (a type-less column is legal SQLite) | `ColumnDescriptor.Type` wherever a dialect emits it verbatim (today: `SQLiteInfo.CreateColumn` / `AddColumn`) and in the `TableModel` pre-flight |

**Why a grammar and not a closed type map for SQLite (the QA-offered alternative (a)).** Postgres is safe today because `PostgreInfo.GetDBType(string)` is a closed switch that throws on anything it does not know. SQLite cannot copy that: the engine has no closed type vocabulary — a column type is a free-form affinity hint, `sqlite_master` returns whatever text the table was created with (`VARCHAR(255)`, `NVARCHAR(100)`, `DATETIME`, `NUMERIC(10,2)` all occur in real databases), `SQLiteInfo.GetSchema` reads those back into `ColumnDescriptor.Type`, and `SchemaService` compares and re-creates from them. `SQLiteInfo.GetDBType(string)` maps the *abstract* `Types.*` vocabulary (`string`, `int`) to native text and would throw `unsupported type 'TEXT'` on the native names every in-repo `TableSchema` test already uses. A closed map would therefore either reject legitimate schemas or become a second, ever-growing type list. The grammar bounds the token to the column-definition slot instead: it cannot contain `;`, `)` outside a digit list, `,` outside a digit list, quotes, or `--`/`/*`, so it cannot end the column definition, the statement, or open a string or comment.

**Residual, stated:** a word sequence such as `TEXT PRIMARY KEY` passes the grammar. It cannot escape the column definition; it can only add constraint keywords to *that* column, which is schema authoring, not injection. Accepted.

Literals the type grammar must accept: `TEXT`, `INTEGER`, `FLOAT`, `BOOLEAN`, `BLOB`, `DECIMAL`, `VARCHAR(255)`, `DECIMAL(10,2)`, `DECIMAL(10, 2)`, `DOUBLE PRECISION`, `character varying`, `timestamp without time zone`, `int4range`, `real[]`, `real[10]`, `UNSIGNED BIG INT`, `""`, null. Must reject: `TEXT); CREATE TABLE pwned_type(x); --`, `TEXT;`, `TEXT --`, `TEXT, x TEXT`, `TEXT)`, `VARCHAR(255) DEFAULT 'x'`, `VARCHAR(a)`, `TEXT  INTEGER` (double space), ` TEXT`, `real[a]`.

Postgres keeps its closed switch as the type guard for that dialect (it is stricter than the grammar and already tested through the schema tests); it throws `InvalidOperationException`, not `InvalidIdentifierException` — changing that exception type is out of scope. MySQL/MsSql `CreateColumn` are `NotImplementedException` stubs; when implemented they emit through the same guard call.

Reconciliation against #14115 §4:

| §4 shape | Grammar | Result |
|---|---|---|
| 1. `information_schema.columns`, `pg_views`, `pg_indexes` | qualified | accepted; rendered raw as today (the dot stays a separator, not part of a quoted name) |
| 2. `DB.Column("ss","string")` — first arg is an alias | simple for the qualifier, simple for the column | accepted; renders `ss.[string]` as today |
| 3. `new DataField("COUNT(*)")` | not an identifier — raw hatch, §4.3 | survives as `DataField.Raw("COUNT(*)")` |
| 4. `sq1`, `o666`, `lat0`, `t`, `__total`, `__window` | simple | accepted (leading `_`, embedded digits are in the class; a *leading* digit is not, and no SQL engine accepts one unquoted either) |
| 5. `content`, `uq_uniqueconstraintentity_url`, `idx_*` | simple | accepted |
| 6. `testtable`, `activeData` | qualified | accepted, unquoted as today (case fold preserved) |
| 7. function names; `schema.func` | qualified | accepted — same predicate as tables, zero extra cost |
| 8. composed `{name}_original`, `idx_{table}_{name}` | — | composed from validated parts inside the library; not re-validated (see §4.1 point 2). Note `idx_{schema.table}_{n}` would compose a dotted index name — pre-existing behaviour, untouched |

Null and empty strings are invalid under both grammars. Length is not capped (the engines truncate or reject long names themselves; length is not an injection vector). Reserved words are not the guard's concern — `select` as a column name is quoted by `MaskColumn` and is legal; as a raw table name it is a SQL syntax error, not an injection.

**Dotted handling decision:** a dotted table name is *one* identifier string validated as a sequence of segments — it is not split, not quoted, not reassembled. That keeps table rendering byte-identical and keeps the predicate a single regex.

### 4.3 The raw hatches — explicit opt-in, and one that was not a hatch

| Hatch | Today | Decision |
|---|---|---|
| `DataField(name, isColumn = false)` — `LoadData(...).Columns(params DataField[])` | raw is the **default**; `IsColumn` has a public setter | **Keep the hatch, invert the default, name it.** The constructor becomes `DataField(string name)` = column (validated as simple, masked as today). The raw form is a static factory `DataField.Raw(string sql)`; `IsColumn` becomes get-only so a validated field cannot be flipped to raw afterwards. Justified: `LoadData` is a schema-agnostic browse and aggregates like `COUNT(*)` are its normal use (in-repo: `PostgresLocalTests`). The XML summary of `Raw` states that the argument is emitted verbatim and must not carry caller input. *r2 (QA CF-4):* the requirement that the exception thrown by `new DataField("COUNT(*)")` **names `DataField.Raw`** stands — it is the one migration every consumer of the old default will hit, and the message is where they learn the new spelling. Mechanism in §4.6. |
| `DB.Field(string)` / `FieldToken` and the LINQ `DB.Field` case | raw | **Not a hatch — an identifier.** It "references a field of the statement", i.e. an alias. Validated as *simple*. The only in-repo use is a test that begins with `Assert.Pass("This is supposed to not work")`. |
| `DB.CustomFunction(string, …)` | raw | **Not a hatch — an identifier.** Validated as *qualified*. |
| `IDBClient.NonQuery/Scalar/Query(string)`, `ViewSchema.Definition`, `[View]` resource | raw statement | Out of scope; not identifier sinks (§2). |

No hatch was found unjustified. No new hatch is introduced.

### 4.4 Category A2 — table name inside a `'…'` literal → parameter

Three sites (`SQLiteInfo.GenerateCreateStatement`, `SQLiteInfo.Truncate` identity-reset `WHERE name = '{table}'` ×2, `PostgreInfo.GenerateCreateStatement` via `createstatement.sql` `relname = '{0}'`) use the table name as a **value** in a WHERE clause. Policy 1 applies: they bind the name with the dialect's parameter syntax through the existing `Scalar/NonQuery(string, params object[])` overloads, exactly as `CheckIfTableExists` already does. The Postgres template drops its `string.Format` placeholder in favour of a parameter reference; the resource is read and passed as-is. The table name is *also* validated as an identifier at the start of these methods (it is used as an identifier in the same method), so the parameter is the correct mechanism for the value position rather than a second line of defence.

### 4.5 Category B — column `DEFAULT` literal in DDL → validate and throw

Source: `[DefaultValue(…)]` (compile-time constant), `EntityDescriptorAccess.Default(…)`, `CreateTableOperation.Column(…, defaultvalue)`, and `TableSchema` column descriptors (a JSON-shaped DTO — runtime input). Emission: `SQLiteInfo.CreateColumn`, `SQLiteInfo.AddColumn`, `PostgreInfo.ColumnAttributes`. DDL cannot bind parameters on any of the four engines.

Decision: **validate-and-throw**, not quote-escape. Reasons:

1. It is what the ask says for "really just a literal and statement only allows literals".
2. Escaping is dialect-dependent in the one way that matters: MySQL treats `\` as an escape in string literals unless `NO_BACKSLASH_ESCAPES` is set, so `''`-doubling is incomplete there and over-escaping is wrong under the other mode. The validation below is correct on all four engines regardless of server settings.
3. No legitimate in-repo default contains a quote; defaults are `0`, `0.0`, `""`, short words. A default that genuinely needs a `'` is implausible and, if it ever appears, throws with the value in the message.

The literal predicate, chosen by the branch the sink already takes:

| Rendered form (existing branch) | Predicate (full match) |
|---|---|
| quoted — value is `string`, `Guid`, `DateTime`, `TimeSpan` | text contains no `'`, no `\`, no character below U+0020 |
| bare — every other type, rendered through `Converter` | `^[A-Za-z0-9_.+-]+$` (covers `0`, `-1.5`, `1e10`, `true`, enum names) |

The bare branch matters because `ColumnDescriptor.DefaultValue` is `object`: a runtime value of an arbitrary type reaches `Converter.Convert<string>` and is emitted unquoted today.

### 4.6 The exception

`InvalidIdentifierException` in `Ocelot/Errors/`, deriving from `ArgumentException` (consumers that already map `ArgumentException` to HTTP 400 get the right status without registration). It carries two properties: `Value` (the offending string, verbatim) and `Role` (the short role name the sink passed: `table`, `column`, `alias`, `function`, `index`, `constraint`, `index type`, `default value`). Message shape: `'<value>' is not a valid <role>` followed by the accepted grammar in one clause. *r2:* the constructor takes an optional third argument, `hint`, appended to the message as a final sentence when present; `Role` stays a clean role name. The only hint today is the one `DataField(string)` passes: *"use DataField.Raw(...) to emit a sql expression verbatim"*. No existing exception fits: `StatementException` means a statement was executed and failed (here nothing executes — using it would be a lie), `SchemaException` is the schema-diff failure, `PropertyNotFoundException` / `UnknownFieldException` carry a different payload. One new type, one line of justification (#114 §12).

**Where it is thrown relative to state (#8385 Step 3):**

| Path | Thrown at | State mutated before the throw |
|---|---|---|
| Fluent operations (`LoadData`, `InsertData`, `UpdateData`, `Delete`, `CreateTable`, `AlterTable`, typed Load with `.Alias` / joins, any token) | `Prepare()` — inside the same call chain as `Execute`, before the prepared operation exists | none |
| `IDBInfo.Truncate` (all dialects), `DropTable`, `DropView`, `SQLiteInfo.AddColumn(client, …)`, `GenerateCreateStatement` | first line of the method, before any transaction is opened | none |
| `EntityDescriptor` (attributes via `Create`, runtime via `EntityDescriptorAccess`) | at the descriptor mutation — first `Model<T>()` / first use of the type | none; the descriptor is not cached until construction completes |
| `SchemaService.CreateSchema` / `UpdateSchema` | entry, before `CheckIfTableExists` and before any statement | none |
| `SchemaService.RemoveSchema` | entry (replaces the existing ad-hoc check) | none |
| `DEFAULT` literal from the fluent `CreateTableOperation` / `AlterTableOperation` | while the single `CREATE TABLE` / `ALTER TABLE` text is built | none |
| *r2:* `ColumnDescriptor.Type` from the fluent `CreateTableOperation.Column(ColumnDescriptor)` / `AlterTableOperation.Add(ColumnDescriptor)` | while the single statement text is built (SQLite emission overloads) | none |
| *r2:* `ColumnDescriptor.Type` from a `TableSchema` | `SchemaService` entry, inside the `TableModel` pre-flight | none |

Regarding tokens: validation at `ToSql` (Prepare) rather than at `DB.Column(...)` construction was chosen because the emission site is the single seam (§4.1). For the mamgo call site the difference is invisible — the exception surfaces in the same request, before execution, as an `ArgumentException`.

## 5. Components & Responsibilities

| Component | Owns | Does not own |
|---|---|---|
| **`IdentifierGuard`** (static, `Ocelot/Info/`) | the three grammars (§4.2), the default-literal predicate (§4.5), *r2:* **three per-piece descriptor validators** — `Column(ColumnDescriptor)` (name simple, `Type` type-token when non-empty, default literal; *r3:* DTO-path only — consumed by `TableModel`), `Index(IndexDescriptor)` and `Unique(UniqueDescriptor)` (shared by `TableModel` and `EntityDescriptor`) — and the `TableModel` pre-flight composed of them (table name qualified, then one loop per collection); throwing `InvalidIdentifierException` | quoting, rendering, dialect knowledge |
| **`DBInfo.MaskColumn`** (base, non-virtual) | validate as *simple* with role `column`, then delegate to a new `protected abstract` dialect quoting hook; the four dialect one-liners move into that hook unchanged | table names (never quoted here except by the two pre-existing callers noted in §6, whose quoting is unchanged) |
| **`InvalidIdentifierException`** (`Ocelot/Errors/`) | `Value`, `Role`, message | — |
| **`DataField`** | `DataField(string)` = validated column; `DataField.Raw(string)` = verbatim expression; `IsColumn` get-only | validation itself (it calls the guard) |
| **`EntityDescriptor`** | validating its own mutations (table name, column add/rename, index add, unique add, column default). *r3 (QA #14150 W-1, corrects r2):* `AddIndex` → `IdentifierGuard.Index`, `AddUnique` → `IdentifierGuard.Unique`; **`AddColumn` validates name and default directly (`Simple` + `Default`) and does not call `IdentifierGuard.Column`**, because `EntityColumnDescriptor.Type` is a CLR type name (`int32`, `range`1`), not SQL type text — see D9 | validating `EntityColumnDescriptor.Type`: it is never the emitted string on the CREATE path (`GetDBType(PropertyType)` is) and is guarded at the emission seam on the one path that does emit it (#37) |
| **`SchemaService`** | pre-flight of an incoming `TableSchema` at `CreateSchema` / `UpdateSchema` entry; `RemoveSchema` uses the guard | — |
| **Emission sites** (§6) | one guard call per caller-supplied identifier, at the point of emission | — |

## 6. Sink inventory and treatment

Cited by #14115 class and member, not line number. **This table is the design's coverage claim for the inventory as of 2026-09-16; it does not claim the inventory is complete** (#1220). John re-runs the sweep in §12 step 0 before starting and adds any new hit to the table and to §10.

Legend — *rendering*: `=` unchanged for valid input · `param` becomes a bound parameter · `mask` now goes through `MaskColumn` (SQLite text changes `"x"` → `[x]`; other dialects byte-identical).

### A1 — table / schema name emitted raw

| # | Sink (member) | Origin | Guard | Rendering |
|---|---|---|---|---|
| 1 | `LoadDataOperation.Prepare` — `FROM {tablename}` | caller (`LoadData(string)`) | qualified, role `table`, at emission | = |
| 2 | `InsertDataOperation.Prepare` / `PrepareBulk` — `INSERT INTO {tablename}` | caller | qualified, emission | = |
| 3 | `UpdateDataOperation.Prepare` — `UPDATE {tablename}` | caller | qualified, emission | = |
| 4 | `DeleteOperation` (string ctor) — `DELETE FROM {table}` | caller | qualified, emission | = |
| 5 | `CreateTableOperation.Prepare` — `CREATE TABLE {tablename}` | caller | qualified, emission | = |
| 6 | `AlterTableOperation.Prepare` — `ALTER TABLE {tablename}` | caller | qualified, emission | = |
| 7 | `SchemaCreator.CreateTable` / `CreateIndices` — `descriptor.TableName`, composed `idx_{table}_{name}` | `EntityDescriptor` | model pre-flight (`EntityDescriptor` mutation points); no emission guard | = |
| 8 | `SchemaService.CreateTable` / `RecreateTable` / `UpdateUniques` / `UpdateIndices` — `schema.Name`, `{name}_original`, `drop.Name` (constraint), composed index names | `TableSchema` (runtime DTO) | model pre-flight at `CreateSchema` / `UpdateSchema` entry via `IdentifierGuard.TableModel`; no emission guard | = |
| 9 | `SchemaUpdater.Update` / `RecreateTable` / `UpdateUniques` / `UpdateIndices` | `EntityDescriptor` (+ catalog `TableDescriptor`, trusted) | model pre-flight; no emission guard | = |
| 10 | `DBInfo.DropView` / `DropTable`, `PostgreInfo` and `MsSqlInfo` overrides — `{view.Name}` / `{entity.Name}` | `ViewDescriptor` / `TableDescriptor` — public DTOs, constructible by callers | qualified, role `table`, first line of the method | = |
| 11 | `DBInfo.Truncate` (base; MySQL/MSSQL inherit) — `TRUNCATE {table}` | caller | qualified, first line | = |
| 12 | `SQLiteInfo.AddColumn(IDBClient, string table, …)` — `ALTER TABLE {table}` | caller | qualified, first line | = |
| 12a *new* | `LoadOperation.Prepare` (both classes), `InsertValuesOperation`, `UpdateValuesOperation`, `DeleteOperation` typed path, `Insert/Update/DeleteEntitiesOperation`, `DBInfo.AppendJoin` — `descriptor.TableName` | `EntityDescriptor` | model pre-flight; no emission guard | = |
| 12b *new* | `PostgreInfo.CreateIndexTypeFragment` — `USING {type}` | `[Index(Type=…)]` / `IndexDescriptor.Type` | model pre-flight (index type is *simple*, role `index type`) | = |
| 12c *new* | `EntityDescriptorAccess.Table(string)`, `.Index(name, [type], …)`, `.Column(expr, name)`, `.Default(expr, value)` | caller, runtime | covered by `EntityDescriptor` mutation-point validation | = |
| 12d *new* | `SchemaService.RemoveSchema` — ad-hoc `Contains('"') …` check then `DROP … {MaskColumn(name)}` | caller | replace the ad-hoc check with *simple*, role `table` (kept quoted via `MaskColumn` as today) | = |

### A2 — table name inside a single-quoted literal

| # | Sink | Treatment |
|---|---|---|
| 13 | `SQLiteInfo.GenerateCreateStatement` — `tbl_name = '{table}'` | `param`; plus qualified guard at method entry |
| 14 | `SQLiteInfo.Truncate` identity reset — `WHERE name = '{table}'` (both branches) | `param`; guard at method entry already required by #23 |
| 15 | `PostgreInfo.GenerateCreateStatement` — `createstatement.sql` `relname = '{0}'` via `string.Format` | `param`: template carries the parameter reference, no `string.Format`; guard at entry |

### A3 — column masked with delimiter, not escaped

Covered by the `MaskColumn` template — no per-site change unless noted.

| # | Sink | Note |
|---|---|---|
| 16 | `ColumnToken.ToSql` — `{Table}.{MaskColumn(Name)}` | `Name` via `MaskColumn`; **`Table` qualifier gets an inline *simple* guard, role `alias`**, rendered raw as today |
| 17 | `LoadDataOperation.Prepare` — `DataField.IsColumn` branch | via `MaskColumn`; raw branch is `DataField.Raw` only (§4.3) |
| 18 | `InsertDataOperation` — `Columns(string[])` | via `MaskColumn` |
| 19 | `PropName.ToSql` — column via `MaskColumn`; `{Alias}.` / `{tablealias}.` qualifier raw | qualifier: inline *simple*, role `alias` |
| 20 | `PropertyInfoToken.ToSql` — `MaskColumn(table)`.`MaskColumn(column)` | via `MaskColumn` (a dotted table name now throws here instead of rendering the already-wrong `"schema.table"`; no in-repo caller passes one) |
| 21 | `CriteriaVisitor.GetColumnName` — column via `MaskColumn`; `{alias}.` raw | qualifier: inline *simple*, role `alias` (the alias originates from `LoadOperation.Alias(string)` / `PropertyToken(alias)` / lambda parameter names) |
| 22 | `SchemaCreator`, `SchemaService`, `SchemaUpdater` unique/column lists | via `MaskColumn` (model already validated) |
| 23 | `PostgreInfo.ReturnID`, `PostgreInfo.Truncate`, `SQLiteInfo.Truncate` — `MaskColumn(table)` / `MaskColumn(idcolumn)` | via `MaskColumn`, kept quoted as today; `Truncate` additionally guards at method entry so the throw precedes the transaction |
| 23a *new* | `PostgreInfo.CreateColumn` (both overloads) / `AlterColumn` — `$"\"{column.Name}\""` direct quoting | `mask` (route through `MaskColumn`; byte-identical output on Postgres). *r3:* reverting this fold is an **equivalent mutant** (QA #14150 M60 survived, expected): rendering is byte-identical and every name reaching it is pre-flighted upstream — the fold is mechanism unification, not a guard; no red expected, do not chase it |
| 23b *new* | `PostgreInfo.DropColumn(preparator, string column)` — `$"\"{column}\""`; **caller-supplied** via `AlterTableOperation.Drop(params string[])` | `mask`; the guard test is `AlterTableDropRejectsInjectedColumnName` (rendered-only with `PostgreInfo`; SQLite throws `NotSupportedException` for drop) |

### A4 — `ColumnIndicator` interpolation

All eleven sites fold into `MaskColumn` (`mask`). One column-quoting mechanism remains; `ColumnIndicator` stays on `IDBInfo` but unused inside the library.

| # | Sink | Origin |
|---|---|---|
| 24 | `UpdateDataOperation.Prepare` — `Set(string[])` (4 interpolations) | caller — this is the rendered PoC |
| 25 | `UpdateEntitiesOperation`, `InsertEntitiesOperation`, `DeleteEntitiesOperation` | `EntityDescriptor` (model pre-flight; `MaskColumn` validates anyway) |
| 26 | `SchemaCreator.CreateIndices`, `SchemaService.CreateIndices` — index column list | model |

### A5 — alias / function name raw

| # | Sink | Guard |
|---|---|---|
| 27 | `AliasToken.ToSql` — `AS {Alias}` | simple, role `alias`, at emission |
| 28 | `WindowedAggregate.ToSql` — `AS {Alias}` | simple, `alias` |
| 29 | `DatabaseFunction.ToSql` — `{FunctionName}(` | qualified, role `function` |
| 30 | `DBInfo.AppendFieldToken` — `FieldToken.Field` | simple, `alias` |
| 31 | `DBInfo.Visit` — `"As"` case and `"Field"` case | simple, `alias` |
| 32 | `LoadOperation.Prepare` (both classes) — `AS {tablealias}` | simple, `alias` (also covers the default `t`) |
| 33 | `DBInfo.AppendJoin`, `MsSqlInfo.AppendJoin` — `AS {join.Alias}` | simple, `alias` |

### B — value inlined as a literal

| # | Sink | Guard |
|---|---|---|
| 34 | `SQLiteInfo.CreateColumn` / `AddColumn` — `DEFAULT '{…}'` and bare | default-literal predicate (§4.5), role `default value`, at emission; also in the model pre-flight |
| 35 | `PostgreInfo.ColumnAttributes` — `DEFAULT '…'` (quoted branch only today; bare values are quoted too, which is a Postgres-side type cast and unchanged) | same |

### A6 (*r2*) — column type name emitted raw into DDL (QA #14141 CF-1; not in #14115)

| # | Sink (member) | Origin | Guard | Rendering |
|---|---|---|---|---|
| 36 | `SQLiteInfo.CreateColumn` private overload — `AppendText(type)` (reached from `CreateColumn(…, ColumnDescriptor)` and `CreateColumn(…, EntityColumnDescriptor)`) | caller: `CreateTableOperation.Column(ColumnDescriptor)`, `SchemaService.CreateSchema/UpdateSchema(TableSchema)`; generated: `GetDBType(Type)` on the entity path | type token, role `column type`, at emission in the shared private overload (one call covers both public overloads); null/empty passes through | = |
| 37 | `SQLiteInfo.AddColumn` private overload — `AppendText(type)` (reached from `AddColumn(…, ColumnDescriptor)`, `AlterTableOperation.Add(ColumnDescriptor)`, `SQLiteInfo.AddColumn(IDBClient, string, EntityColumnDescriptor)`) | caller (`AlterTableOperation.Add`, `SchemaService` DTOs); *r3 (QA #14150 W-1, corrects r2's "generated `GetDBType(Type)`"):* on the entity path this overload emits **`EntityColumnDescriptor.Type` — the lower-cased CLR type name — raw**; the only reachable entity caller is the public `SQLiteInfo.AddColumn(IDBClient, string, EntityColumnDescriptor)` (no in-repo caller; pre-existing wrong-type bug filed as #14149), while the `SchemaUpdater` ALTER branch is unreachable today because `SQLiteInfo.MustRecreateTable` is always true (#14148) | same — the type-token guard at this overload is what covers the CLR-name emission (QA M64 red); `TypeToken` accepts `int32`/`string` and throws on `range`1`, a clearer failure than the syntax error it produced before | = |
| 38 | `IdentifierGuard.TableModel` / `IdentifierGuard.Column` — pre-flight | `TableSchema` (SchemaService) and `EntityDescriptor.AddColumn` | type token when `Type` is non-empty, so the `SchemaService` recreate path throws at entry (§4.1) | — |
| 39 | `PostgreInfo.ColumnType` — `GetDBType(column.Type, column.Length)` | same DTOs | already closed by the existing switch (throws `InvalidOperationException` on unknown input); no change | = |

## 7. Interactions & Data Flow (key flows)

**String-named fluent operation, e.g. `em.Delete(userString).Execute()`**
`Execute` → `Prepare` → emission of `DELETE FROM` → `IdentifierGuard` (qualified, `table`) → throws `InvalidIdentifierException` *or* `AppendText(validated)` → prepared operation → `NonQuery`. Nothing reaches the client on the throw path.

**Token in a typed load, e.g. `OrderBy(DB.Column(userString))`**
`Prepare` → `dbinfo.Append(field)` → `ColumnToken.ToSql` → `MaskColumn` → guard (simple, `column`) → dialect quoting hook → `AppendText`.

**Entity first use, e.g. `em.UpdateSchema<T>()`**
`Model<T>` → `EntityDescriptor.Create` → each mutation validates (table qualified; columns, indices, uniques, index types simple; defaults literal) → descriptor cached only on success → `SchemaCreator`/`SchemaUpdater` emit trusted names; `MaskColumn` re-checks columns incidentally.

**Schema DTO, e.g. `schemaService.UpdateSchema(name, tableSchema)`**
entry → `IdentifierGuard.TableModel(tableSchema)` → throws before `GetSchema` *or* proceeds; `RecreateTable`/`UpdateIndices` emit trusted names.

## 8. Contracts (abstract)

| Contract | Input | Output / effect | Invariant |
|---|---|---|---|
| `IdentifierGuard.Simple(value, role)` | any string, role name | returns the same string; throws `InvalidIdentifierException(value, role)` | returned string full-matches the simple grammar |
| `IdentifierGuard.Qualified(value, role)` | same | same | returned string full-matches the qualified grammar |
| `IdentifierGuard.DefaultLiteral(text, quoted, role)` | rendered default text, which branch | returns text or throws | quoted → no `'` `\` control chars; bare → `[A-Za-z0-9_.+-]+` |
| *r2:* `IdentifierGuard.TypeToken(value, role)` | a column type string or null/empty | returns the same string (null/empty unchanged) or throws | non-empty return full-matches the type-token grammar (§4.2) |
| *r2:* `IdentifierGuard.Column(ColumnDescriptor)` / `Index(IndexDescriptor)` / `Unique(UniqueDescriptor)` | one descriptor | throws on the first invalid member, else returns | the piece is safe to emit on any dialect. *r3:* `Column` is the **DTO-path** validator (its `Type` member is SQL type text) and is consumed by `TableModel` only; `Index` and `Unique` are consumed by both `TableModel` and `EntityDescriptor`; `EntityDescriptor.AddColumn` validates name + default directly because its `Type` is a CLR name |
| `IdentifierGuard.TableModel(name, columns, indices, uniques)` | a table's model pieces (`ColumnDescriptor`, `IndexDescriptor`, `UniqueDescriptor` collections) | `Qualified(name)` then the per-piece validators over each collection; throws on the first invalid piece | after return, every emission of these names and types is safe without further checks; consumer: `SchemaService` (the `EntityDescriptor` consumes the pieces directly, see §5) |
| `IDBInfo.MaskColumn(column)` | column or simple identifier | dialect-quoted identifier | never emits an unvalidated string; the quoting hook receives only validated input |
| `DataField.Raw(sql)` | verbatim SQL fragment | `DataField` with `IsColumn == false` | the only constructor of a raw field; documented as caller-trusted |
| `InvalidIdentifierException` | `Value`, `Role`, *r2:* optional `hint` (message-only) | `ArgumentException` subtype | thrown before any statement of the operation executes; `Role` never carries hint text |

## 9. Cross-cutting concerns

- **Security:** the guard is the control. Rendering is unchanged for valid input, so no new quoting semantics are introduced (Postgres case folding preserved).
- **Error handling:** consumers see `ArgumentException`-derived failures at `Prepare`/entry; no `StatementException` (nothing executed). The exception message contains the offending value verbatim — acceptable: it is the caller's own input, and it is what the operator needs to see in a log.
- **Performance:** one regex match (or an equivalent character loop) per identifier per `Prepare`; identifiers are short; cost is nanoseconds against a database round-trip. The regex is a static compiled instance. No caching of validation results — YAGNI.
- **Concurrency:** the guard is stateless. `EntityDescriptor` validation happens inside its existing per-type construction; no new shared state.
- **Observability / logging:** none added. The exception is the signal.
- **Backward compatibility:** three visible changes — (1) invalid identifier strings throw instead of rendering; (2) `new DataField("expr")` now means column, `DataField.Raw` is the expression form (compile error for callers of the two-argument constructor, runtime `InvalidIdentifierException` naming `DataField.Raw` for callers passing an expression to the one-argument form); (3) SQLite rendering of entity-typed Insert/Update/Delete column names changes `"x"` → `[x]`; *r2:* (4) SQLite `ALTER TABLE … ADD COLUMN` default values render through `Converter` (invariant culture) instead of `ToString()` / `$"'{value}'"` (current culture) — a `DateTime` or `double` default on a de-DE host renders differently than before (QA W-8); (5) a `ColumnDescriptor.Type` outside the type-token grammar now throws on SQLite where it previously rendered. All five are intended and go in the release note; the `PackageVersion` bump to 0.24.0 taken by the implementer (QA W-7) is confirmed as consistent with this list, final call with the orchestrator.

## 10. Test battery and coverage

**File:** `Ocelot.Tests/Security/IdentifierInjectionTests.cs` — the auditor's file is kept at its path and rewritten: its eight tests flip from asserting the injectable rendering to asserting the throw (and, for the two executed PoCs, that the seeded victim row survives). A second fixture `Ocelot.Tests/Security/IdentifierGuardTests.cs` pins the predicate with literals. A third, `Ocelot.Tests/Security/LegitimateIdentifierShapesTests.cs`, pins the duals from #14115 §4. All fixtures `[TestFixture, Parallelizable]`, each test builds its own client (Code Contracts 2026-08-08 ruling), no fixture fields.

**Falsifier column:** at design time (r1) no mutation had been executed. *r2:* QA #14141 ran 61 mutants against the r1 implementation; where a row below cites an `M`-number, that is a mutant Jenny observed (red or survived) — a measurement, not a prediction. Rows marked *r2* are new or changed because a survivor showed the r1 row was false. John re-runs the named survivors and reports them red (#8385 Step 3 q2 table as a named deliverable).

**Test-name convention:** the design's names stand for this repo (QA W-2 raised `MethodName_Condition_ExpectedResult`; 149/475 existing tests use underscores, the rest do not — there is no single convention to violate, and the §10 names are what the coverage table greps for). Of the two duplicate alias tests QA found, keep `AliasRejectsInjectedAlias` (the flipped PoC in `IdentifierInjectionTests`) and drop `AliasTokenRejectsInjectedAlias`.

| Class | Sink(s) | Guard test (name) | Why it discriminates | Mutation to run red |
|---|---|---|---|---|
| Predicate | `IdentifierGuard.Simple` / `Qualified` | `SimpleAcceptsEveryInRepoShape` (`a`, `_a`, `A1`, `__total`, `sq1`, `o666`, `t`) · `SimpleRejects` (`""`, null, `a b`, `a;b`, `a]`, `a"`, `` a` ``, `1a`, `a.b`, `a-b`, `ä`, `a\0`) · `QualifiedAcceptsDotted` (`information_schema.columns`, `pg_views`, `a.b.c`) · `QualifiedRejects` (`.a`, `a.`, `a..b`, `a. b`, `a;b.c`) | literal expected values; no collaborator produces them | widen the character class by one char (`]`) — `SimpleRejects` reds; drop the anchor — `SimpleRejects` reds on `a b` |
| Predicate | `DefaultLiteral` | `QuotedDefaultRejectsQuoteBackslashControl` · `BareDefaultRejectsNonToken` · `DefaultAcceptsPlainValues` (`""`, `none`, `0`, `-1.5`, `true`) | literals | remove the `'` from the rejected set — first test reds |
| Exception | `InvalidIdentifierException` | `ExceptionCarriesValueAndRole` · `ExceptionIsArgumentException` | asserts the two properties and the base type | change the base type — second test reds |
| A1 caller | #1–#6 | `LoadDataRejectsInjectedTableName` · `InsertDataRejectsInjectedTableName` · `UpdateDataRejectsInjectedTableName` · `DeleteRejectsInjectedTableNameAndVictimSurvives` (seed one row; assert throw; assert the seeded row is still readable; *r3:* name confirmed against the tree after QA #14150 Insight 1) · `CreateTableRejectsInjectedTableName` · `AlterTableRejectsInjectedTableName` | each test drives one public verb with `victim; CREATE TABLE pwned(x); --`; the throw type is the assertion, not the absence of `pwned` | remove the guard call from that one `Prepare` — only that test reds (per-sink uniqueness, #8385 Step 3 q2) |
| A1 dialect members | #10, #11, #12 | `DropTableRejectsInjectedName` (each of the four `IDBInfo` instances with a `TableDescriptor` — the stubs' overrides are reachable without a connection because the throw precedes the client call) · `DropViewRejectsInjectedName` (same) · `BaseTruncateRejectsInjectedTableBeforeClientCall` (`MySQLInfo`/`MsSqlInfo` against a SQLite client — the throw must come before `NonQueryAsync`) · `SqliteAddColumnRejectsInjectedTable` · *r2 (QA W-4, M16/M17 survived):* `PostgresTruncateRejectsInjectedTableBeforeClientCall` and `PostgresGenerateCreateStatementRejectsInjectedTableBeforeClientCall` — both called with `client: null`; the throw must be `InvalidIdentifierException`, not `NullReferenceException` | the stubs would otherwise reach the client and fail for a different reason; asserting the exception type distinguishes; with `client: null` any statement attempt is a different exception type | remove the entry-line guard in the stub override — that dialect's row reds (M05–M07, M10–M13, M19 red; M16/M17 to be re-run) |
| A1 executed PoC | #14/#23 | `TruncateRejectsInjectedTableNameAndVictimSurvives` (seed a row; throw; row still readable) · *r2 (QA W-4, M21 survived):* `SqliteTruncateRejectsInjectedTableBeforeTransaction` — Moq `IDBClient` with `DBInfo` = `SQLiteInfo`; call `Truncate(mock, "victim]; --")`; assert the throw **and** `Verify(c => c.Transaction(), Times.Never)` | the survival test is shadowed by `MaskColumn` on the next statement (Jenny's finding) — it pins *that nothing executed*, not *where* the guard sits; the mock pins the entry placement | remove the entry guard (keep `MaskColumn`) — only the mock test reds |
| A1 model | #7, #9, #12a, #12b, #12c | `EntityWithInjectedTableAttributeThrowsOnModel` · `EntityWithInjectedColumnAttributeThrowsOnModel` · `EntityWithInjectedIndexNameThrowsOnModel` · `EntityWithInjectedIndexTypeThrowsOnModel` · `EntityWithInjectedUniqueNameThrowsOnModel` · `ModelTableSetterRejectsInjectedName` · `ModelIndexRejectsInjectedName` · `ModelColumnRenameRejectsInjectedName` · `ModelDefaultRejectsQuoteBreakout` | each test declares a private entity type carrying one bad attribute and calls `em.Model<T>()` (or the runtime mutator). *r2:* as shipped these use a real SQLite client, not the mock the r1 row asked for — accepted: `Model<T>()` never touches the client, so the mock added nothing | remove validation from one `EntityDescriptor` mutation point — exactly that test reds (M23–M30 red) |
| A1 schema DTO | #8 | `SchemaServiceCreateRejectsInjectedTableName` · `SchemaServiceCreateRejectsInjectedColumnName` · `SchemaServiceUpdateRejectsInjectedColumnNameBeforeAnyStatement` · `RemoveSchemaRejectsInjectedName` | each pins one guard on the DTO path (M44, M47 red) | remove that guard — its test reds |
| A1 schema DTO — ordering (*r2*, QA CF-3: M45/M46/M56 survived) | #8 pre-flight before any statement | `SchemaServiceUpdateRecreateRejectsInjectedColumnAndLeavesTableIntact` — create `victim(first, second)` through the service on SQLite, then `UpdateSchema("victim", {first, "bad; DROP TABLE x; --"})` (dropping `second` forces `SQLiteInfo.MustRecreateTable` → `RecreateTable`); assert the throw, then assert `sqlite_master` holds `victim` and **not** `victim_original`, and that `victim` still has both columns · `SchemaServiceUpdateRejectsInjectedTargetNameBeforeAnyStatement` — same setup, `targetSchema.Name = "victim; --"`; same catalog assertions (pins the second `Qualified` line, M46) · `SchemaServiceCreateRejectsInjectedIndexColumnAndCreatesNothing` — `CreateSchema` with a valid column and an `IndexDescriptor` whose `Columns` contains `"bad; --"`; assert the throw and that `sqlite_master` has **no** `victim` (pins the `index.Columns` loop, M56) | the recreate path executes `ALTER TABLE … RENAME` *before* the statement that would throw at emission; only a test that reaches that path and reads the catalog afterwards can tell pre-flight from emission guard. Jenny's PoC with the pre-flight removed left only `victim_original` — that is the observation the first test inverts | delete the `TableModel` call in `UpdateSchema` — first test reds on `victim_original` present; delete the `index.Columns` loop — third test reds on `victim` present |
| A2 (*r2*, QA W-5: the r1 "no runnable falsifier" claim was wrong — a capturing mock observes the command text directly) | #13, #15 | `GenerateCreateStatementBindsTableName` (dual: valid name returns the DDL of `victim`, not `other`) · `GenerateCreateStatementRejectsInjectedName` · `SqliteGenerateCreateStatementBindsTableAsParameter` — Moq `IDBClient` (`DBInfo` = `SQLiteInfo`) capturing `ScalarAsync(text, params)`; assert the text contains `@1` and not `'victim'`, and the parameter list contains `victim` · `PostgresGenerateCreateStatementBindsTableAsParameter` — same capture with `PostgreInfo`; assert `relname = @1`, no `'{0}'`, no `'victim'`, parameter `victim` | the capture reads the bytes that reach the client; a literal revert changes them | revert `tbl_name = @1` to `'{table}'` (M20-style) — the SQLite capture test reds; revert `createstatement.sql` to `'{0}'` + `string.Format` — the Postgres capture test reds |
| A2 (*r2*) | #14 | `TruncateResetIdentityStillResetsSequence` (dual) · `SqliteTruncateResetIdentityBindsTableAsParameter` — Moq `IDBClient` (`DBInfo` = `SQLiteInfo`, `Transaction()` returning a mock transaction) capturing every `NonQueryAsync(transaction, text, params)`; assert the `sqlite_sequence` update text contains `name = @1` and not `'victim'`, parameter `victim`; run it once with `options.Transaction` supplied and once without so both branches are captured | same | revert either branch to `'{table}'` — reds (M22 survived in r1 for exactly this reason) |
| A3 template | `MaskColumn` ×4 | `MaskColumnRejectsDelimiterOnEveryDialect` (`TestCaseSource` over `new SQLiteInfo()`, `new PostgreInfo()`, `new MySQLInfo()`, `new MsSqlInfo()`; input `a]; DROP--`, `a"`, `` a` ``) · `MaskColumnStillQuotesValidNameOnEveryDialect` (`[a]`, `"a"`, `` `a` ``, `"a"`) | the four dialects are instantiated without a connection; the base template is the only place that can throw | move validation into one dialect's hook instead of the base — the other three rows red |
| A3 drop column | #23b | `AlterTableDropRejectsInjectedColumnName` (`new AlterTableOperation(client, "t").Drop("a\"; DROP TABLE x; --")`, `PostgreInfo`, rendered-only) | the only caller-supplied column reaching `DropColumn` | revert `DropColumn` to direct quoting — reds |
| A3 tokens | #16, #19, #21 | `ColumnTokenRejectsInjectedColumnName` (flipped PoC) · `ColumnTokenRejectsInjectedTableQualifier` · `PropertyAliasRejectsInjectedAlias` (`DB.Property<T>(expr, alias)` — routes through `PropertyToken` → `CriteriaVisitor`, pins M33) · `LoadAliasRejectsInjectedAlias` (`.Alias("x; --")`, pins the typed `AS` emission M34; its `[Description]` must not claim the criteria path) · `LoadDataColumnsRejectsInjectedColumnName` · `InsertDataColumnsRejectsInjectedColumnName` · *r2 (QA CF-2, M42/M43 survived):* `PropNameAliasRejectsInjectedAlias` — `DB.Property<ValueModel>("Integer", false, "x; --")` (the string-property overload, which renders through `PropName.ToSql`'s `Alias` branch) · `PropNameTableAliasRejectsInjectedAlias` — `new OperationPreparator().AppendField(DB.Property(typeof(ValueModel), "Integer"), new SQLiteInfo(), EntityDescriptor.Create, "x; --")` (the `tablealias` branch, reached directly so the typed `AS` guard cannot shadow it) | each drives one token/overload; the two `PropName` branches are distinct emission lines and get one test each | remove the qualifier guard in `ColumnToken` — only `…TableQualifier` reds; remove either `PropName` guard — its test reds |
| A4 | #24, #25 | `UpdateDataSetRejectsInjectedColumnName` (flipped PoC, `Set(cols)` without values) · *r2 (M59 survived):* `UpdateDataSetWithValuesRejectsInjectedColumnName` (`Set(cols).Values(...)` branch) · `UpdateEntitiesRendersColumnsThroughMaskColumn` (SQLite `CommandText` contains `[integer]=`; pins the fold, M52 red) · *r2 (M57 survived):* `InsertEntitiesRendersColumnsThroughMaskColumn` and `DeleteEntitiesRendersColumnsThroughMaskColumn` (same capture shape; `[integer]` / `[id] IN`) · *r2 (M61 survived):* `AlterTableAddRejectsInjectedColumnName` — `new AlterTableOperation(client, "t").Add(new ColumnDescriptor("a]; --") { Type = "TEXT" }).Prepare()` on SQLite | the render tests pin the mechanism change explicitly because it is the one visible rendering change; the two `UpdateData` branches are separate interpolation sites | revert one site to `ColumnIndicator` — its render test reds on `"integer"`; bypass `MaskColumn` in `SQLiteInfo.AddColumn` — the `AlterTableAdd…` test reds. *r3:* the Postgres `CreateColumn(ColumnDescriptor)` fold revert (#23a) is an equivalent mutant — no red expected |
| A5 | #27–#33 | `AliasRejectsInjectedAlias` (flipped PoC, M50 red) · `WindowedAggregateRejectsInjectedAlias` (`DB.CountOver(alias: …)`, M49) · `CustomFunctionNameRejectsInjectedName` (flipped, M48) · `FieldTokenRejectsInjectedName` (`DB.Field`, M02) · `LoadAliasRejectsInjectedAlias` (typed `LoadOperation<T>`, M34) · `JoinAliasRejectsInjectedAlias` (`Join(..., joinAlias)` — pins `CriteriaVisitor`, not `AppendJoin`, because the ON-criteria renders the alias first) · *r2 (QA CF-2 — four named r1 tests were not implemented, M03/M04/M09/M08 survived):* `LambdaAsRejectsInjectedAlias` (`em.Load<ValueModel>(v => DB.As(v.Integer, "x; --"))`, `DBInfo.Visit` "As" case) · `LambdaFieldRejectsInjectedName` (`v => DB.Field("x; --")`, "Field" case) · `MsSqlJoinAliasRejectsInjectedAlias` (rendered-only, `MsSqlInfo`, `LateralJoin(inner, criteria: null, joinAlias: "x; --")` so `MsSqlInfo.AppendJoin` emits the alias) · `BaseJoinAliasRejectsInjectedAliasWithoutCriteria` (`SQLiteInfo`-independent: `PostgreInfo` rendered-only, `LateralJoin(inner, criteria: null, joinAlias: "x; --")` — the un-shadowed path into `DBInfo.AppendJoin`) · *r2 (M35 survived):* `UntypedLoadAliasRejectsInjectedAlias` (`new LoadOperation(client, …).Alias("x; --").Prepare()`, the non-generic class) · *r2 (M39 survived):* `InsertDataBulkRejectsInjectedTableName` (`InsertData("t; --").Columns("a").PrepareBulk()`) | one test per emission line; where a site is shadowed by an earlier emission of the same string (`AppendJoin` behind `CriteriaVisitor`), the test takes the path with no criteria | remove one site's guard — its test reds; the r1 claim "one test per emission site" is now true for #31, #32 (both classes), #33 (both dialects) and #19 |
| B | #34, #35 | `SqliteCreateColumnRejectsQuoteInStringDefault` · `SqliteAddColumnRejectsQuoteInStringDefault` · `SqliteCreateColumnRejectsBareDefaultWithBreakout` (an `object` whose `ToString` is `0); DROP TABLE x; --`) · `PostgresColumnAttributesRejectsQuoteInStringDefault` (rendered-only, `new PostgreInfo().CreateColumn(preparator, …)`) · `EntityWithQuotedDefaultAttributeThrowsOnModel` (`[DefaultValue("a'b")]`) · `CreateTableFluentRejectsQuoteInDefault` | dialect instances + a bare preparator; no connection | remove the literal check from one branch — its test reds (M14, M18 red) |
| A6 (*r2*, QA CF-1) | #36–#38 | `TypeTokenAcceptsSqlTypeShapes` and `TypeTokenRejects` (the two literal lists in §4.2, predicate level) · `SqliteCreateColumnRejectsInjectedTypeName` — `new SQLiteInfo().CreateColumn(preparator, new ColumnDescriptor("first") { Type = "TEXT); CREATE TABLE pwned_type(x); --" })`, no connection · `SqliteAddColumnRejectsInjectedTypeName` — same through `AddColumn(preparator, …)` · `SqliteCreateColumnAcceptsTypelessColumn` (`Type = null` renders `[first]` followed by the next token, no throw) · `SchemaServiceCreateRejectsInjectedTypeNameAndCreatesNothing` — Jenny's PoC inverted: `CreateSchema` with that `Type`; assert the throw and that `sqlite_master` holds neither `victim` nor `pwned_type` · `SchemaServiceUpdateRecreateRejectsInjectedTypeAndLeavesTableIntact` — recreate path with a bad `Type` on the surviving column; catalog assertions as in the ordering row · `CreateTableFluentRejectsInjectedTypeName` (`CreateTable("t").Column(new ColumnDescriptor("a") { Type = "TEXT); --" }).Execute()` throws, no `pwned` table) · `EntityColumnTypeStillRendersOnSqlite` (dual: `UpdateSchema<ValueModel>` on SQLite still creates the table — the generated `GetDBType(Type)` output passes the grammar) | the private overloads are reached without a connection through a bare preparator; the service tests assert the catalog after the throw, so the pre-flight (#38) is distinguishable from the emission guard (#36/#37) exactly as in the ordering row | remove the guard from the `CreateColumn` overload — the first `Sqlite…` test reds; remove `Type` from `IdentifierGuard.Column` — the `…RecreateRejectsInjectedType…` test reds on `victim_original` present |
| Hatch | `DataField` | `RawDataFieldStillRendersExpression` (`DataField.Raw("COUNT(*)")` → `SELECT COUNT(*) FROM`) · `DataFieldConstructorIsColumnAndRejectsExpression` (`new DataField("COUNT(*)")` throws; *r2 (QA CF-4):* the assertion is `Does.Contain("DataField.Raw")` on the message, and the `[Description]` says exactly that) · `DataFieldIsColumnIsNotSettable` | the first two are the two halves of the hatch decision; the message assertion pins the hint | flip the constructor default back — second test reds; drop the hint argument — second test reds on the message |
| Exception hint (*r2*) | `InvalidIdentifierException` | `ExceptionAppendsHintWhenGiven` (literal hint text appears at the end of the message; `Role` unchanged) · `ExceptionOmitsHintWhenNull` | literal expected strings | drop the hint concatenation — first test reds |
| Duals | #14115 §4 | `SchemaQualifiedTableNameStillRendersRaw` (`LoadData("information_schema.columns")` → `FROM information_schema.columns`) · `AliasQualifiedColumnStillRenders` (`DB.Column("ss","string")` → `ss.[string]`) · `UnderscoreAndDigitAliasesStillRender` (`__total`, `sq1`, `o666`) · `ComposedIndexNameStillCreates` (entity with `[Index("time")]`, `UpdateSchema<T>`, assert `idx_<table>_time` present in `sqlite_master`) · `PostgresIntrospectionEntitiesStillDescribe` (`EntityDescriptor.Create` for `PgColumn`, `PgView`, `PgIndex` does not throw) · `MixedCaseTableNameStillRendersUnquoted` (`[Table("activeData")]` → `FROM activeData`) | these are the rows that would red if the grammar or the no-quoting decision drifted | tighten the qualified grammar to one segment — first and fifth red; add quoting to tables — last reds |

**Sweep guard for the table itself:** for each row, ask *"would this test still pass against an implementation lacking the property?"* *r2:* after QA the two A2 rows no longer answer *yes* — the capturing mock observes the command text. No row is now known to be unfalsifiable; the r1 rows that were false (A2, the A5 "one test per site" claim, the A1 schema DTO ordering claim) are the ones rewritten above.

## 11. Quality attributes & trade-offs

| Decision | Alternative rejected | Why |
|---|---|---|
| Validate-and-throw, one grammar | Escape the delimiter in `MaskColumn` (#14115 §5 proposal) | fixes A3/A4 only; per-dialect escaping rules; raw sinks have nothing to escape into; would require adding quoting to tables, which changes Postgres case folding |
| Rendering seam | Public-API entry validation | cannot see string-built sinks or composed names; ≈ same site count; a future factory is easy to forget |
| Rendering seam | Preparator `AppendIdentifier` | a third of the sinks never touch a preparator |
| `MaskColumn` as base template | guard call in each dialect's `MaskColumn` | one site vs four; a fifth dialect cannot forget |
| Model pre-flight at `EntityDescriptor` mutation points | pre-flight loop in each composite (`SchemaCreator`, `SchemaUpdater`, `EntityDescriptorAccess` ×4) | "state an invariant once, at the mutation" (#8385); one place covers attribute discovery and every runtime mutator |
| Fold `ColumnIndicator` sites into `MaskColumn` | guard call at each of the eleven interpolations | one quoting mechanism to audit; the SQLite text change is semantically nil |
| `DataField.Raw` factory | keep `isColumn` bool with flipped default | a negative flag does not read as "trusted raw SQL"; a named factory is the explicit opt-in the brief asks for |
| Category B validate | `''`-escape | dialect-dependent on MySQL; policy text; no legitimate quoted default exists |
| *r2:* type-token grammar for `ColumnDescriptor.Type` on SQLite | closed type map in `SQLiteInfo` (the way Postgres already works) | SQLite has no closed type vocabulary: the catalog returns free-form type text that `GetSchema` reads back and `SchemaService` re-emits; a closed map rejects real schemas or grows without bound; the grammar bounds the token to the column-definition slot with a stated residual (§4.2) |
| *r2:* per-piece validators (`Column`/`Index`/`Unique`); *r3:* `Index`/`Unique` shared by `TableModel` and `EntityDescriptor`, `Column` DTO-path only | keep D1's re-coded predicates in `EntityDescriptor`; or (r2 as written) route `EntityDescriptor.AddColumn` through `Column` too | QA W-3: the r1 DRY math assumed one bulk call from both models; D1 correctly showed `EntityDescriptorAccess` mutates after construction, so the shared unit is the *piece*. QA #14150 W-1: sharing `Column` would run the SQL type grammar over a CLR type name and reject every generic property at `Model<T>()` for no security gain (D9) |
| No dialect-pluggable grammar | per-dialect grammar hook | no dialect needs it (§3); one regex change if one ever does |
| No length cap, no reserved-word list | — | neither is an injection vector; YAGNI |
| Role as a plain string parameter | `IdentifierRole` enum | the role only feeds the message; an enum is a type with no consumer beyond `ToString` |

Maintainability: every identifier decision lives in one static class and one exception; the rule for new code is one sentence. Performance: negligible (§9). Availability: unchanged.

## 12. Implementation guidance (ordered milestones for John)

0. **Re-run the sweep before touching anything** and reconcile §6/§10 with what the tree holds now: grep `Ocelot/` for `AppendText(`, `$"` inside `NonQuery`/`Scalar`/`Query` calls, `.Append(` on a `StringBuilder`, `string.Format`, and `ColumnIndicator`; classify every string-valued interpolation as *identifier* / *value* / *keyword*. Add any identifier hit missing from §6 to the table and to §10 with a named test. Do not claim completeness; record the grep you ran in the return.
1. `IdentifierGuard` + `InvalidIdentifierException` + `IdentifierGuardTests` (predicate rows). Red first: write the reject tests against a guard that accepts everything.
2. `DBInfo.MaskColumn` template + dialect quoting hooks; `MaskColumnRejectsDelimiterOnEveryDialect`. Then fold the eleven `ColumnIndicator` sites and the four Postgres direct-quote sites into `MaskColumn`; `UpdateEntitiesRendersColumnsThroughMaskColumn`.
3. `EntityDescriptor` mutation-point validation (uses `IdentifierGuard.TableModel` for the column/index/unique pieces where the model is complete, or the per-piece methods at each mutation — John's choice, one mechanism, not both); the A1-model test rows.
4. A1 caller sinks (#1–#6, #10–#12, #12d) and the flipped PoC tests; A2 parameters (#13–#15).
5. Tokens and aliases (A3 #16/#19/#21, A5 #27–#33); `DataField` hatch; the hatch tests.
6. `SchemaService` pre-flight and `RemoveSchema`; the schema-DTO rows.
7. Category B (#34, #35) and its rows.
8. Duals fixture. Full suite green; every row's mutation run and observed red once, tabulated in the return (#8385 Step 3 q2 asks for the table as a named deliverable).
9. Update the one in-repo caller `new DataField("COUNT(*)")` → `DataField.Raw("COUNT(*)")`. XML summaries only; no body comments (#114 §4).

Every step is inside this one PR — one feature, one PR. Steps 2's `ColumnIndicator` fold is part of the feature (it is what makes "one column mechanism" true), not a separate cleanup.

### r2 delta (after QA #14141) — what changes on top of the r1 implementation in the tree

1. **A6 type token.** Add the third grammar to `IdentifierGuard` (`TypeToken(value, role)`; null/empty returns unchanged); call it in the two `SQLiteInfo` private overloads (`CreateColumn` / `AddColumn` with the `string type` parameter) before `AppendText(type)`; add `Type` to the per-piece `Column` validator. Tests: the A6 row.
2. **Per-piece validators (W-3).** Extract `IdentifierGuard.Column(ColumnDescriptor)`, `Index(IndexDescriptor)`, `Unique(UniqueDescriptor)`; `TableModel` becomes `Qualified(name)` plus one loop per collection calling them; `EntityDescriptor.AddIndex/AddUnique` call `Index`/`Unique`. *r3 (corrects r2):* `EntityDescriptor.AddColumn` keeps `Simple(name)` + `Default(value)` and does **not** call `Column` — its `Type` is a CLR name (D9). `EntityDescriptorAccess.Default` keeps its direct `IdentifierGuard.Default` call (it writes `DefaultValue` on an existing column, bypassing `AddColumn` — QA confirmed M30 red).
3. **Exception hint (CF-4).** Optional third constructor argument on `InvalidIdentifierException`; `DataField(string)` passes the `DataField.Raw` hint; the two hint tests; `DataFieldConstructorIsColumnAndRejectsExpression` asserts the message names `DataField.Raw`.
4. **Missing §10 tests (CF-2, W-4, W-5, W-6).** Every test named *r2* in §10; re-run M03, M04, M08, M09, M15, M16, M17, M21, M22, M35, M39, M42, M43, M45, M46, M56, M57, M59, M61 and report each red in the return table.
5. **Ordering tests (CF-3).** The *A1 schema DTO — ordering* row, with catalog assertions after the throw.
6. **Contract items QA owns the wording of (not design):** trim the six `[Description]`s (CF-4), bring the three body comments in edited methods to #114 §4 (CF-5), file-scope `IdentifierGuard.cs` (CF-6), constraint-model asserts (W-1), drop the duplicate `AliasTokenRejectsInjectedAlias`.
7. *r3 (forced, #9533 defect 1 only):* correct the embedded-resource name in `PostgreInfo.GenerateCreateStatement` to `Pooshit.Ocelot.Info.Postgre.createstatement.sql` so the row-15 capturing-mock test can reach `ScalarAsync` (QA #14150 M70 red). Do **not** change `createstatement.sql` line 1 (#9533 defect 2) in this PR; the PR body states the split and #9533 stays open.

## 13. Risks & mitigations

| Risk | Mitigation |
|---|---|
| A consumer passes an identifier the grammar rejects that the engine accepted (e.g. `$` in a MySQL column) | throws with the value and role in the message on first use; extending the grammar is one character class; not silent |
| A consumer relied on `new DataField("expr")` | runtime `InvalidIdentifierException` whose message names `DataField.Raw`; release note |
| A consumer asserted on SQLite `"col"` rendering of entity Insert/Update/Delete | release note; no in-repo test does |
| A future token emits `AppendText(callerString)` | no structural prevention (§4.1); mitigated by the single guard type to grep and the review rule; `docs/architecture/` is where the rule lives, not a code comment |
| `PropertyInfoToken` with a dotted table name now throws where it rendered wrong SQL | equivalent failure, earlier and clearer; no in-repo caller |
| *r2:* a type token such as `TEXT PRIMARY KEY` passes the type grammar and adds a constraint to that column | cannot leave the column-definition slot (no `,` `)` `;` quotes or comment openers); schema authoring, not injection — stated residual in §4.2 |
| *r2:* a legitimate SQLite type text outside the grammar (e.g. a type containing a quoted identifier read back from a hand-written schema) now throws on `SchemaService` round-trips | throws with the value and role `column type`; the grammar is one regex; no in-repo or catalog-observed shape needs it |

## 14. Pre-Design Checklist (#1136 §5), answered

**KISS / DRY / YAGNI**
- No new type mirrors an existing one: `IdentifierGuard` (static) and `InvalidIdentifierException` are the only additions; `DataField.Raw` is a factory on an existing type. ✓
- No abstraction with one implementation: the dialect quoting hook has four. ✓
- No "might need later" element: no dialect grammar, no length cap, no enum, no caching. ✓
- No deprecation period / shim: `DataField` changes in place; breakage is loud. ✓
- DRY math (*r2*, corrected after QA W-3; *r3*, corrected again after QA #14150 W-1): the shared units are `Index` (name + type + columns, ≈4 lines) and `Unique` (name + columns, ≈3 lines), each called from `TableModel` and the matching `EntityDescriptor` mutation point: `≈(4+3) × 2 = 14` lines that would otherwise be duplicated — at the #1267 threshold, extracted because the named-helper test passes (`Index`, `Unique`). `Column` (name + type + default) has one consumer (`TableModel`) by design: `EntityDescriptor.AddColumn`'s two lines (`Simple` + `Default`) are not a duplicate of it, because its `Type` member is a different kind of string (D9). `ColumnIndicator` fold removes eleven 1-line duplicates of the same interpolation. ✓

**Existing systems first**
- `MaskColumn` (existing seam) is reused as the column guard; `CheckIfTableExists` (existing parameter pattern) is the model for A2; existing `Scalar/NonQuery(string, params)` overloads carry the parameters. ✓
- New layer reason: none — the guard is a static function, not a layer. ✓
- No new persisted data. ✓

**Configurability**
- No config knob. The grammar is a `static readonly` regex. ✓

**Less is better**
- Delete / merge / inline check: the role enum was deleted in favour of a string; per-role methods were merged into two grammar methods; the pre-flight was merged into `EntityDescriptor`'s own mutation points. ✓
- Trade-offs named in §11. ✓
- Reader inventory covers AST sites (§6) and string-literal sites (`$"…"` / `string.Format` / `StringBuilder` — §6 rows 7–15, 22–26, 34–35). ✓

**Data deliverables**
- No SQL deliverable. ✓

**Document discipline**
- Cites #114 and #1136 as load-bearing (header). ✓
- Scope / non-scope explicit (§2). ✓
- No superseded predecessor design exists for this feature. ✓

## 15. Decisions taken autonomously (#8727)

### D1 — `DataField` raw hatch spelling
- **Chosen:** constructor = column; `DataField.Raw(string)` = expression; `IsColumn` get-only.
- **Why:** the brief asks for explicit opt-in; a named factory is explicit at the call site; a negative bool is not.
- **Alternatives:** keep `isColumn` bool, flip its default — works, reads badly; remove the hatch — breaks the schema-agnostic aggregate use in `PostgresLocalTests`.
- **Reversal cost:** cheap — one factory, one constructor default.

### D2 — fold `ColumnIndicator` interpolations into `MaskColumn`
- **Chosen:** all eleven sites (and the four Postgres direct-quote sites) render through `MaskColumn`.
- **Why:** one column-quoting mechanism is the property that makes the coverage claim auditable; the guard in the base template then covers them for free.
- **Alternatives:** guard call at each interpolation — eleven more sites, two mechanisms remain.
- **Reversal cost:** cheap, mechanical; the only visible effect is SQLite `"x"` → `[x]` in three entity operations.

### D3 — Category B is validate-and-throw
- **Chosen:** reject `'`, `\`, control characters in quoted defaults; require a bare token for unquoted ones.
- **Why:** §4.5.
- **Alternatives:** `''`-doubling — incomplete on MySQL under default `sql_mode`.
- **Reversal cost:** cheap — replace the predicate with an escape in the same three sites.

### D4 — tokens validate at `ToSql`, not at construction
- **Chosen:** emission time.
- **Why:** single seam (§4.1); indistinguishable for the consumer (same request, before execution).
- **Alternatives:** construction time in the `DB.*` factories — earlier stack frame, second validation site.
- **Reversal cost:** cheap — add the call in the factories and leave the seam as is.

### D5 (*r2*) — `ColumnDescriptor.Type` on SQLite is guarded by a type-token grammar, not a closed map
- **Chosen:** third grammar in `IdentifierGuard` (§4.2), applied at the two SQLite emission overloads and in the `Column` pre-flight piece; Postgres keeps its closed switch.
- **Why:** SQLite's type names are free-form by engine design and round-trip from the catalog through `GetSchema` into `ColumnDescriptor.Type`; a closed map would reject legitimate schemas or become a second unbounded type list. The grammar bounds the token to the column-definition slot.
- **Alternatives:** closed map in `SQLiteInfo.GetDBType(string)` extended with native names — rejects catalog round-trips like `VARCHAR(255)`; escape — nothing to escape into (unquoted token).
- **Reversal cost:** cheap — replace one predicate call with a map lookup at the same two lines.

### D6 (*r2*, amended *r3*) — per-piece descriptor validators; `Index`/`Unique` shared, `Column` DTO-path only
- **Chosen:** `IdentifierGuard.Column / Index / Unique`; `TableModel` loops over all three; `EntityDescriptor.AddIndex/AddUnique` call `Index`/`Unique`; `EntityDescriptor.AddColumn` validates name + default directly (see D9 for why not `Column`).
- **Why:** John's D1 was right that a bulk call cannot cover post-construction mutators; the r1 DRY claim was therefore false (QA W-3). Sharing the index/unique pieces keeps one predicate set for them.
- **Alternatives:** leave D1's re-coded predicates — two places to keep in step; route `AddColumn` through `Column` as r2 wrote — rejected in D9.
- **Reversal cost:** cheap.

### D9 (*r3*, QA #14150 W-1 — implementer deviation upheld) — `EntityDescriptor.AddColumn` does not run the type-token grammar
- **Chosen:** `EntityDescriptor.AddColumn` validates `Simple(name)` + `Default(value)` only; `IdentifierGuard.Column` (which includes `TypeToken`) is consumed by `TableModel` alone.
- **Why:** `EntityColumnDescriptor.Type` is the lower-cased CLR type name (`int32`, `string`, `datetime`, `byte[]`, `range`1`), not SQL type text. On the SQLite CREATE path it is never emitted — `CreateColumn(…, EntityColumnDescriptor)` maps `GetDBType(PropertyType)`; on Postgres it is read by the closed `GetDBType(string)` switch, never emitted raw. The one path that does emit it raw — the public `SQLiteInfo.AddColumn(IDBClient, string, EntityColumnDescriptor)`, no in-repo caller, pre-existing wrong-type bug #14149 — is guarded at the emission seam (§6 #37, QA M64 red); the `SchemaUpdater` ALTER branch that would also reach it is unreachable while `SQLiteInfo.MustRecreateTable` is always true (#14148), and is single-statement inside a transaction anyway. Running the SQL type grammar over the CLR name at `Model<T>()` would reject every generic property on every dialect and every path — including the CREATE path that never emits the value — for no security gain. The r2 premise ("origin: generated `GetDBType(Type)`" for row 37) was false; this block replaces it.
- **Alternatives:** route `AddColumn` through `Column` (r2 text) — false positives on `Range<T>`, `List<T>`; validate the CLR name with the *simple* grammar — still rejects `range`1` and `byte[]`, and validates the wrong string.
- **Reversal cost:** cheap — one call swap — but wrong; if #14148/#14149 are fixed so that entity `.Type` is genuinely emitted, the correct move is to map it through `GetDBType` at that emission, not to validate it at the model.

### D7 (*r2*) — the `DataField` exception names `DataField.Raw` via an optional exception hint
- **Chosen:** keep the r1 requirement; add an optional `hint` argument to `InvalidIdentifierException`, message-only, `Role` unchanged.
- **Why:** it is the one migration every consumer of the old default constructor hits, and the message is where they learn the new spelling.
- **Alternatives:** drop the requirement — cheaper, but the throw then says "not a valid column" to someone who wrote `COUNT(*)` on purpose; stuff the hint into the role string — pollutes `Role`.
- **Reversal cost:** cheap.

### D8 (*r2*) — test names stay as designed; version bump stands
- **Chosen:** §10 names are the convention for this repo (QA W-2); the implementer's 0.24.0 bump (QA W-7) is consistent with the release-note list in §9.
- **Why:** the repo has no single naming convention (149/475 underscore); the coverage table greps for the §10 names. The bump was the orchestrator's call in r1 — recorded here so it is a decision, not an accident.
- **Alternatives:** rename 112 tests to `Method_Condition_Result` — churn with no discriminating value.
- **Reversal cost:** cheap (rename / revert csproj line).

No open question blocks implementation.

## 16. Open questions for the operator

None blocking. Two notes for the release: (1) `PackageVersion` bump — this is a behavioural change on the public surface; (2) mamgo-backend's non-mapper `ApplyFilter` overload (#11258) will now surface `InvalidIdentifierException` (an `ArgumentException`) for a bad sort field — confirm its error middleware maps `ArgumentException` to 400, or file the one-line follow-up there.
