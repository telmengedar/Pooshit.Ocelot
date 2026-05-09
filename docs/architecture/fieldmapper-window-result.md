# Architectural Document: FieldMapper WindowResult Integration

Author: Sarah (architect agent)
Date: 2026-05-09
Implementer: john-backend-dev
Branch off: `master` (post-PR #3, v0.19.0)
DiVoid task: 148

---

## 1. Goal

`ExecuteWindowedAsync<TWindow>` and `ExecutePagedAsync` (v0.19.0) materialize entities through the `EntityDescriptor` path — `BuildSetters` + `ToObjectWindowedAsync` — and never consult the caller's `IFieldMapper<TModel>`. So consumers that compose entities across a join graph via a `FieldMapper` (DiVoid's `NodeService` is the canonical case) cannot use the windowed primitive and stay stuck on the two-query pattern. This task closes that gap with two additive surfaces: a reader-level primitive on `PreparedLoadOperation<T>` exposing the open reader plus the resolved window value, and matching `WindowedFromOperation<TWindow>` / `PagedFromOperation` methods on `IFieldMapper<TModel>`. Shipping this unblocks DiVoid task 122 (`nototal` sentinel removal) for mapper-based callers — they get the same single-statement perf the descriptor-path consumers already have.

---

## 2. New Mapper API

### 2.1 Decision: ship the `PagedFromOperation` sugar on the mapper (option a)

Pagination is the dominant use case and dropping the sugar would force every mapper-based paged call to repeat the `.Limit().Offset() + WindowedFromOperation<long>(DB.CountOver())` recipe. Mirroring `EntitiesFromOperation`'s shape keeps the mapper surface symmetric.

### 2.2 Final signatures on `IFieldMapper<TModel>`

```
Task<WindowResult<TModel, TWindow>> WindowedFromOperation<TWindow>(
    LoadOperation<TModel> operation,
    WindowedAggregate windowedAggregate,
    CancellationToken cancellationToken = default,
    params string[] fields);

Task<WindowResult<TModel, TWindow>> WindowedFromOperation<TWindow, TLoad>(
    LoadOperation<TLoad> operation,
    WindowedAggregate windowedAggregate,
    CancellationToken cancellationToken = default,
    params string[] fields);

Task<WindowResult<TModel, long>> PagedFromOperation(
    LoadOperation<TModel> operation,
    int limit,
    int offset,
    CancellationToken cancellationToken = default,
    params string[] fields);

Task<WindowResult<TModel, long>> PagedFromOperation<TLoad>(
    LoadOperation<TLoad> operation,
    int limit,
    int offset,
    CancellationToken cancellationToken = default,
    params string[] fields);
```

Naming: `WindowedFromOperation` mirrors `EntitiesFromOperation`; `PagedFromOperation` keeps the `Paged` terminology already used by `ExecutePagedAsync`. No better name worth the cost.

### 2.3 Param-ordering note

`params string[] fields` lives last in every existing `*FromOperation` overload. We keep that. Because `params` greedily consumes positional arguments, the `CancellationToken` parameter sits *before* `fields` and defaults to `default` — same trick `ExecutePagedAsync` uses with `(int limit, int offset, CancellationToken)`. Callers that omit the CT and pass field names work as today; callers that want the CT pass it by name (`cancellationToken: ct`) or positionally before the field list.

---

## 3. Reader-Level Primitive

### 3.1 Decision: option (a), public, on `PreparedLoadOperation<T>`

Returns a small named carrier (not a tuple) symmetric with `WindowResult<TItem, TWindow>` but one level lower — the consumer materializes rows themselves. Public because there is no good reason to lock it down: any future consumer that wants to drive its own row construction (custom DTOs, projection shapes the descriptor doesn't model) gets the same affordance the FieldMapper does.

### 3.2 Type — `WindowReader<TWindow>`

File: `Ocelot/Entities/Operations/Prepared/WindowReader.cs`.

| Member | Type | Visibility | Semantics |
|---|---|---|---|
| `Reader` | `Reader` | public get-only | The open data reader, positioned **at the start** of the result set (no `ReadAsync` has been performed by the primitive). Caller owns iteration and must dispose. |
| `WindowValue` | `Task<TWindow>` | public get-only | Resolves to the windowed-aggregate value once the consumer has read at least one row. Resolves to `default(TWindow)` on a zero-row result (set when the consumer's first `ReadAsync` returns false — see §3.4). |
| `WindowOrdinal` | `int` | public get-only | The column ordinal of the windowed aggregate. `-1` until the first row is read. Consumers that materialize rows must skip this ordinal. |

Construction: single internal constructor. Only `PreparedLoadOperation<T>` instantiates. Plain class, get-only properties, matches `WindowResult` style.

### 3.3 Method signature on `PreparedLoadOperation<T>`

```
Task<WindowReader<TWindow>> ExecuteWindowedReaderAsync<TWindow>(
    WindowedAggregate windowedAggregate,
    CancellationToken cancellationToken = default);

Task<WindowReader<TWindow>> ExecuteWindowedReaderAsync<TWindow>(
    Transaction transaction,
    WindowedAggregate windowedAggregate,
    CancellationToken cancellationToken = default);
```

These mirror the `ExecuteWindowedAsync<TWindow>` overload pair shipped in v0.19.0 — same alias resolution, same SQL injection helper (`InjectWindowedColumn`), same reader acquisition path. The only difference is what the method returns: an unread reader instead of a fully-orchestrated `WindowResult`.

Validation matches v0.19.0: null aggregate → synchronous `ArgumentNullException`; pre-canceled CT → faulted task before connection acquisition.

### 3.4 Window-value resolution timing

The primitive does **not** eagerly read row 1 itself — that's the consumer's responsibility (the FieldMapper does it as part of materializing the first entity). To still hand the consumer a `Task<TWindow>` that resolves "for free":

- The primitive constructs a `TaskCompletionSource<TWindow>` and exposes its `Task` as `WindowValue`.
- It wraps the underlying `Reader` in a thin proxy that intercepts the first `ReadAsync` outcome:
  - If the first read returns `true`: extract the windowed column by alias (cached ordinal stored back into `WindowOrdinal`), `TrySetResult(value)` on the TCS.
  - If the first read returns `false`: `TrySetResult(default)` on the TCS.
  - On exception during the first read: `TrySetException`.
- After the first read, the proxy is transparent — subsequent reads pass through unchanged.

**Decision: implement the proxy as a `Reader` subclass (`WindowedReader : Reader`)** that overrides `ReadAsync` (and `Read`, defensively, even though the windowed path is async-only) to perform the first-read interception before delegating. This avoids leaking a "read once via the primitive, then via the property" two-step API. The `WindowReader<TWindow>.Reader` property hands out this subclassed reader; its `WindowOrdinal` reflects the resolved ordinal once the first read occurs (read it after first `ReadAsync` returns).

---

## 4. `EntityFromCurrentRow` on `IFieldMapper<TModel>`

### 4.1 Decision: add a public materialization method, do not promote `CreateEntityFromReader`

Promoting `CreateEntityFromReader` to `protected` would only help subclassing scenarios. A public method whose contract is "the reader is positioned on a row, materialize from the current position" is additively useful in any future "I'm walking the reader myself" scenario and is what the new mapper methods need internally. Keep `CreateEntityFromReader` private; expose the equivalent surface as a documented public method.

### 4.2 Signature on `IFieldMapper<TModel>`

```
TModel EntityFromCurrentRow(Reader reader, params string[] fields);
```

### 4.3 Contract

- The reader **must** be positioned on a valid row. The method does **not** call `ReadAsync` / `Read`.
- If the reader is positioned past the end (no row), behavior is undefined and likely throws — same posture as any other "you misused the API" path in the mapper.
- Materialization logic is identical to the existing `CreateEntityFromReader` private helper: `Activator.CreateInstance<TModel>()`, run `InitializeEntity`, walk `fields` (or `mappings` if `fields` is empty), call `SetValue` per index, run `PostProcessEntity`.
- Ordinal-to-field mapping is positional in `fields` order — same as `CreateEntityFromReader` today. The mapper does **not** look up columns by name. *This is critical for §6:* the caller (the new `WindowedFromOperation`) is responsible for ensuring the `fields` array does not include the windowed-aggregate column, so the positional walk skips that ordinal.

### 4.4 Relationship to existing `EntityFromReader`

| Method | Pre-reads? | Returns on empty stream | When to use |
|---|---|---|---|
| `EntityFromReader(reader, fields)` | Yes — calls `await reader.ReadAsync()` | `default(TModel)` | "Give me the next entity, advance the reader for me." |
| `EntityFromCurrentRow(reader, fields)` | No | Undefined (caller's bug) | "I have already advanced the reader; build me the entity from where it sits." |

Implementation note: `EntityFromReader` becomes a one-liner over the new method (`if (!await reader.ReadAsync()) return default; return EntityFromCurrentRow(reader, fields);`), and `CreateEntityFromReader` is removed (its body becomes the body of `EntityFromCurrentRow`).

---

## 5. SQLite Buffering Decision

**Decision: option (a) — the reader-level primitive returns the open reader; the mapper performs its own SQLite buffering.**

Rationale (one sentence): the v0.19.0 `ExecuteWindowedAsync<TWindow>` body is freshly merged and stable, and changing it to a thin wrapper around `ExecuteWindowedReaderAsync` (option c) is gratuitous churn this PR's scope cap explicitly rules out (§7) — duplicating the small buffering branch in the mapper costs ~10 lines and keeps both paths independently testable. Option (b) — buffering hidden inside the primitive — also fails because the primitive cannot buffer until the consumer has read the first row, and the consumer is the mapper.

The buffering branch in the new mapper code mirrors `ExecuteWindowedAsync`'s body verbatim (post-first-row): if `!MultipleConnectionsSupported`, drain the rest of the reader into a `List<TModel>` (calling `EntityFromCurrentRow` per row, advancing via `await reader.ReadAsync(ct)`), dispose the reader, return a `WindowResult` whose `Items` wraps the buffer; otherwise return a streaming `IAsyncEnumerable<TModel>` that yields from the held reader and disposes on iterator dispose. Cancellation propagation is the same as v0.19.0 — `OperationCanceledException` flows out unwrapped, the reader disposes via `using` / explicit dispose-in-catch, the TCS observes the cancellation if the first read failed.

---

## 6. Eager-Read Flow

`FieldMapper<TModel>.WindowedFromOperation<TWindow>(operation, windowedAggregate, ct, fields)` orchestrates as follows (prose, no code):

1. Resolve `fields` — if the caller passed none, build the field-name array from the mapper's own `FieldNames` so positional materialization later has a deterministic field count. (This matches the existing `EntitiesFromOperation` posture.)
2. Prepare the operation: call `operation.Prepare(false)` to obtain a `PreparedLoadOperation<TLoad>` (or `<TModel>`).
3. Acquire the windowed reader: `await prepared.ExecuteWindowedReaderAsync<TWindow>(windowedAggregate, ct)`. The returned `WindowReader<TWindow>` carries the unread `Reader` plus the unresolved `WindowValue` task.
4. Eagerly read row 1: `await windowReader.Reader.ReadAsync(ct)`. The `WindowedReader` proxy intercepts this read, captures the windowed-column ordinal into `WindowOrdinal`, and resolves `WindowValue` (either to the row's value or to `default` if the read returned false).
5. Zero-row branch: yield an empty `IAsyncEnumerable<TModel>`, dispose the reader, return `new WindowResult<TModel, TWindow>(emptyAsync, windowReader.WindowValue)`.
6. Non-empty branch — first row materialization: call `EntityFromCurrentRow(windowReader.Reader, fields)` to build the first `TModel`. The mapper's positional walk uses `fields.Length`, which does **not** include the windowed-aggregate column (it was injected into the SQL projection but is *not* a field the mapper knows about), so the windowed ordinal is naturally skipped. **The alias-collision concern raised in the brief** ("mapper has a `__window` field") is resolved by this: even if the mapper happens to define a field literally named `__window`, the SQL injection is by ordinal-after-the-projected-columns, and the mapper's positional read consumes only `fields.Length` ordinals starting at 0 — the windowed column sits at the end and is never touched.
7. SQLite buffering branch (`!MultipleConnectionsSupported`): drain remaining rows into a list (calling `EntityFromCurrentRow` per `ReadAsync` that returns true), dispose the reader, return a `WindowResult` whose `Items` is an async-enumerable over the buffer (with the eager first row prepended).
8. Multi-connection streaming branch: return a `WindowResult` whose `Items` is an async iterator that yields the eager first row, then loops `await reader.ReadAsync(ct) → EntityFromCurrentRow → yield return`, and disposes on completion / cancellation / iterator-disposal.

`PagedFromOperation` is the trivial sugar:

1. Validate `limit >= 0`, `offset >= 0` synchronously (mirrors `ExecutePagedAsync`).
2. Call `operation.Limit(limit).Offset(offset)` on the supplied operation (matching the existing `LoadOperation<T>.ExecutePagedAsync` pass-through pattern).
3. Delegate to `WindowedFromOperation<long>(operation, DB.CountOver(), ct, fields)`.

---

## 7. Test Plan

New file: `Ocelot.Tests/Fields/FieldMapperWindowResultTests.cs` (the existing FieldMapper test home is `Ocelot.Tests/Fields/`; mirror that). NUnit 3, parallel-safe. Default SQLite in-memory.

| Test | Purpose |
|---|---|
| `WindowedFromOperation_MaxOver_MapperMaterializesEntities` | Insert N rows; build a multi-field `FieldMapper`; call `WindowedFromOperation<int>(op, DB.MaxOver(scoreField))`; assert `WindowValue == max(score)`, `Items` count == N, each entity carries the mapper-composed shape (not the entity-descriptor shape — verify with a join-graph fixture). Headline test for the gap closure. |
| `WindowedFromOperation_CallerSuppliedAlias_RoundTrips` | Pass `new WindowedAggregate(..., alias: "_custom")`; verify mapper materializes correctly and `WindowValue` resolves via the supplied alias. Confirms the proxy reader picks up the right ordinal. |
| `WindowedFromOperation_AliasCollision_MapperFieldNamedDoubleUnderscoreWindow` | Configure a mapper with a field literally named `__window`; verify entities materialize correctly because the mapper's positional read stops at `fields.Length` and the windowed column sits past it. Guards the alias-collision path called out in the brief. |
| `WindowedFromOperation_ZeroRows_ResolvesDefault` | Empty result; `WindowValue` resolves to `default(TWindow)`, `Items` yields nothing, exactly one statement executed. |
| `PagedFromOperation_TotalMatchesUnpaginatedCount` | Insert N rows, call `PagedFromOperation(limit: K, offset: 0)` with K < N; assert `WindowValue == N`, `Items` count == K. |
| `PagedFromOperation_NegativeLimit_Throws` | Synchronous `ArgumentOutOfRangeException` (mirrors `ExecutePagedAsync`). |
| `WindowedFromOperation_PreCanceledToken_FaultsImmediately` | Pre-canceled CT short-circuits before connection acquisition. |
| `WindowedFromOperation_CancellationMidStream_FaultsBoth` | Cancel mid-`await foreach`; both `Items` and `WindowValue` (if not yet resolved on row 1) observe `OperationCanceledException`, not `StatementException`. |
| `WindowedFromOperation_SingleStatementAssertion` | Counting-decorator over `IDBClient`; assert exactly one `ReaderAsync` / `ReaderPreparedAsync` call across the mapper-driven path. The load-bearing assertion that protects the single-query property over the mapper integration. |

Also add a small unit file `Ocelot.Tests/Fields/EntityFromCurrentRowTests.cs` covering the new public method directly: positioned-on-row materialization works; calling without first reading throws or returns garbage (document whichever is observed); the existing `EntityFromReader` continues to behave identically (delegates to the new method).

Out of scope for this test pass: Postgres mirror file. The existing `Ocelot.Tests/Postgres/PostgresExecutePagedAsyncTests.cs` plus the SQLite mapper tests cover the matrix. If the implementer has cycles, a `Ocelot.Tests/Postgres/PostgresFieldMapperWindowResultTests.cs` mirror gated on `POSTGRES_CONNECTION` is welcome but not required.

The existing `ExecuteWindowedAsyncTests` and `ExecutePagedAsync*Tests` files stay untouched — they have no mapper dependency and the v0.19.0 primitive's behavior is unchanged.

---

## 8. Out of Scope

- Any change to `ExecuteWindowedAsync<TWindow>`'s public signature or body — v0.19.0 stays as shipped.
- Any change to `WindowResult<TItem, TWindow>` — same.
- Refactoring `ExecuteWindowedAsync` to delegate to `ExecuteWindowedReaderAsync` (option c from §5) — explicitly rejected here; if it's worth doing, it's a separate cleanup PR.
- The `InjectWindowedColumn` token-list refactor (DiVoid task 142) — separate.
- Any new methods on `EntityManager` — the FieldMapper is the surface this task touches.
- Adopting the new mapper methods in DiVoid — downstream task once this ships.
- Sync (non-async) variants of `WindowedFromOperation` / `PagedFromOperation` — the primitive is async-only; matches v0.19.0 posture.

---

End of design.
