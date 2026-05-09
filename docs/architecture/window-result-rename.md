# Architectural Document: WindowResult Rename + ExecuteWindowedAsync Primitive

Author: Sarah (architect agent)
Date: 2026-05-09
Implementer: john-backend-dev
Branch off: `master`
Refactors: v0.18.0 (`window-functions-and-paged-load.md`)

---

## 1. Goal

Pooshit.Ocelot v0.18.0 shipped `PagedResult<T>` and `ExecutePagedAsync` as a single-statement paginated load primitive. The terminology — `PagedResult`, `Total: Task<long>`, internal alias `__total` — is welded to the count-for-pagination case, but the underlying mechanism (inject a windowed aggregate into the projection, recover its value off the first row) is fully general. Toni's framing: "we are in preview mode and should there be random consumers they can stay with the old — also it shouldn't be that big of a change — also i like it clean, especially for such a library." This refactor takes Scope B from DiVoid task 149: rename the type to `WindowResult<TItem, TWindow>`, generalize the field to `WindowValue: Task<TWindow>`, lift the bulk of `ExecutePagedAsync`'s body into a new generic `ExecuteWindowedAsync<TWindow>` primitive, and reduce `ExecutePagedAsync` to a thin pagination sugar over it. No back-compat shims, no deprecation aliases — preview mode, clean break.

---

## 2. Type Model — `WindowResult<TItem, TWindow>`

File: `Ocelot/Entities/Operations/Prepared/WindowResult.cs` (replaces `PagedResult.cs`; old file deleted in the same commit).

| Member | Type | Visibility | Semantics |
|---|---|---|---|
| `Items` | `IAsyncEnumerable<TItem>` | public get-only | The streamed page. Iterated with `await foreach`. Elements are fully constructed `TItem`s (same projection logic as `ExecuteEntitiesAsync<T>`). |
| `WindowValue` | `Task<TWindow>` | public get-only | The windowed aggregate's value, recovered from the first row's reserved alias. Resolved exactly once per call. For zero-row results, resolves to `default(TWindow)` (so `0L` for `Task<long>`) without a second query. |

Construction: single internal constructor `WindowResult(IAsyncEnumerable<TItem> items, Task<TWindow> windowValue)`. No public constructor — only `PreparedLoadOperation<T>` instantiates. Plain class (not record) to match Ocelot's existing API style on `netstandard2.1`.

Field-name decision: `WindowValue`. The candidates (`Aggregate`, `Value`, `Window`) were considered. `WindowValue` mirrors the type name, reads naturally at call sites (`paged.WindowValue`, `windowed.WindowValue`), and is unambiguous next to `Items`. Sticking with the default Toni proposed in task 149.

Docstring callouts (preserved from `PagedResult`, retargeted):

- "On SQLite (single-connection), all items are buffered in memory before this property is observable. On multi-connection databases items are streamed live."
- "`WindowValue` resolves from the windowed aggregate column injected into the projection. No second SQL round trip."
- "For a zero-row result set, `WindowValue` resolves to `default(TWindow)`."
- The reserved-alias caveat — see §5.

---

## 3. `ExecuteWindowedAsync<TWindow>` — The Primitive

### 3.1 Final signatures

On `PreparedLoadOperation<T>` (typed variant only — untyped `PreparedLoadOperation` does not need it; `T` is required to project entities):

```
Task<WindowResult<T, TWindow>> ExecuteWindowedAsync<TWindow>(
    WindowedAggregate windowedAggregate,
    CancellationToken cancellationToken = default);

Task<WindowResult<T, TWindow>> ExecuteWindowedAsync<TWindow>(
    Transaction transaction,
    WindowedAggregate windowedAggregate,
    CancellationToken cancellationToken = default);
```

Mirrored on `LoadOperation<T>` as thin pass-throughs (existing convention: call `Prepare(false)`, then forward).

### 3.2 What it does

1. Take the prepared `LoadOperation<T>`'s SQL, inject the supplied `WindowedAggregate` into the projection under a reserved alias (§3.3).
2. Issue a single `IDBClient.ReaderAsync` / `ReaderPreparedAsync` with the CT.
3. Eagerly read row 1 inside the method (before the outer `Task` completes), extract the aliased column value as `TWindow`, set a `TaskCompletionSource<TWindow>`.
4. On `!MultipleConnectionsSupported`: buffer all remaining rows, dispose the reader (releasing the SQLite semaphore), return a `WindowResult<T, TWindow>` whose `Items` wraps the in-memory buffer.
5. On multi-connection dialects: return a `WindowResult<T, TWindow>` whose `Items` streams from the held reader; reader disposes on iterator dispose.
6. Zero-row case: `TaskCompletionSource<TWindow>` resolves to `default(TWindow)`. No second query.
7. Cancellation / connection-drop: see §6.

### 3.3 Alias-naming decision

**Decision: option (c) — caller-supplied via `WindowedAggregate.Alias`, with a fallback to a reserved generic name when the caller leaves it null.**

Justification (one sentence): the existing `WindowedAggregate` already carries an `Alias` property whose whole purpose is to name the projection column, so honouring it makes the primitive symmetric with how callers already think about the token; the reserved fallback keeps `ExecutePagedAsync` (which constructs the aggregate internally) from needing to thread alias plumbing through.

Concretely:

- If `windowedAggregate.Alias` is non-empty, use it verbatim. The primitive looks up the column by that name on row 1.
- If `windowedAggregate.Alias` is null/empty, the primitive constructs an internal copy of the aggregate with `Alias = "__window"` set, and uses `"__window"` to recover the value. (Implementation note for the implementer: cloning a `WindowedAggregate` with an alias substitution is a straight constructor call given the existing public properties on the token — no new infrastructure needed.)
- Per-row extraction is done via `reader.GetOrdinal(alias)` once on row 1, ordinal cached for subsequent rows.

`ExecutePagedAsync` (§4) hands in `DB.CountOver()` with no alias and lets the primitive default to `__window`. No special-case `__total` alias survives.

### 3.4 Limit / offset parameters

**Decision: no limit/offset parameters on `ExecuteWindowedAsync` itself.** The caller sets `.Limit()` / `.Offset()` on the `LoadOperation<T>` via the existing fluent API before preparing.

Justification (one sentence, sanity-checking Toni's instinct — confirmed): windowing and pagination are orthogonal concerns (e.g., `MaxOver(field)` over an unpaginated result is a perfectly reasonable call), and folding limit/offset into the primitive's signature would either force every windowed call to specify them or duplicate the pagination override logic that already lives in `LoadOperation<T>.Limit/Offset`.

`ExecutePagedAsync` keeps its own `(int limit, int offset, ...)` signature because pagination *is* its purpose; it sets limit/offset on the operation internally before delegating (§4).

### 3.5 Validation

- `windowedAggregate` null → `ArgumentNullException`, synchronous (before returning the task).
- Pre-canceled CT → short-circuit before opening a connection; return a faulted task.
- No upper-bound check on the value-shape `TWindow`; the implementation calls `Convert.ChangeType` (or the existing reader-value-coercion helper Ocelot already uses for scalars — match whatever `ExecuteScalarAsync<T>` does today) to coerce the raw column value into `TWindow`. If coercion fails, the cast exception flows out as a `StatementException` (matches existing scalar-execute behavior).

---

## 4. `ExecutePagedAsync` — The Sugar

### 4.1 What changes

- Return type: `Task<WindowResult<T, long>>` (was `Task<PagedResult<T>>`).
- Signature otherwise unchanged: `(int limit, int offset, CancellationToken)` and the `(Transaction, int, int, CancellationToken)` sibling overload, on both `PreparedLoadOperation<T>` and `LoadOperation<T>`.

### 4.2 What stays

- The `limit < 0` / `offset < 0` argument validation throws synchronously, same as today.
- The pre-canceled-CT short-circuit, same as today.
- The transaction-first overload pair, same as today.

### 4.3 Internal implementation

On `PreparedLoadOperation<T>.ExecutePagedAsync(transaction, limit, offset, ct)`: the prepared shape can no longer be retro-fitted with new limit/offset clauses safely (the prepared SQL is already baked), so `ExecutePagedAsync` on the *prepared* operation continues to do its own limit/offset substitution on the prepared command text — same `InjectTotalColumn`-style work, but renamed and re-pointed (§4.4). Specifically:

- Inject the windowed-count column with `DB.CountOver()` (no alias → primitive defaults to `__window`).
- Strip and re-append LIMIT/OFFSET on the prepared command text in dialect-appropriate syntax.
- Execute via the same single-statement reader path the primitive uses.

On `LoadOperation<T>.ExecutePagedAsync(transaction, limit, offset, ct)`: the *unprepared* path is where it gets clean. It calls `.Limit(limit).Offset(offset)` on the operation, then `Prepare(false)`, then delegates to `ExecuteWindowedAsync<long>(transaction, DB.CountOver(), ct)`. The big chunk of execute-side code (eager-read, SQLite buffering, multi-connection streaming, alias extraction) lives only in the primitive; the unprepared sugar is genuinely a few lines.

### 4.4 `InjectTotalColumn` → `InjectWindowedColumn`

The static helper currently named `InjectTotalColumn(string commandText, int limit, int offset, string dialectTypeName)` is renamed and re-pointed:

`InjectWindowedColumn(string commandText, string aggregateSql, string alias, int? limit, int? offset, string dialectTypeName)`

- `aggregateSql` is the rendered windowed-aggregate text (e.g., `COUNT(*) OVER()`). The helper composes `<aggregateSql> AS <alias>` and injects it into the projection.
- `limit` and `offset` are nullable: when both null, the helper does no LIMIT/OFFSET manipulation (this is the path `ExecuteWindowedAsync` on a prepared operation takes — the prepared SQL already carries whatever limit/offset the caller set fluently).
- When non-null, the helper strips and re-appends LIMIT/OFFSET in dialect-appropriate syntax (the existing MSSQL / others split survives).
- The prepared `ExecuteWindowedAsync<TWindow>` calls `InjectWindowedColumn` with `aggregateSql` rendered from the user-supplied `WindowedAggregate`, `alias` from `windowedAggregate.Alias ?? "__window"`, and `limit`/`offset` null.
- The prepared `ExecutePagedAsync` calls `InjectWindowedColumn` with `aggregateSql` rendered from `DB.CountOver()`, `alias = "__window"`, `limit` and `offset` non-null.

This is a refactor of the existing string-injection helper, not the larger `InjectTotalColumn` → token-list refactor (DiVoid task 142, explicitly out of scope per §9).

---

## 5. `__total` → `__window` Migration

The reserved alias changes from `__total` (literal "total") to `__window` (generic). The reservation behavior is identical — entities with a property named `__window` would have it overwritten with the windowed aggregate value (vanishingly unlikely in real schemas). Document the new reservation in:

- The `ExecuteWindowedAsync<TWindow>` XML docstring: "The reserved alias `__window` is used for the windowed aggregate column when `WindowedAggregate.Alias` is not specified. Avoid entity property names that collide."
- The `WindowResult<TItem, TWindow>` class docstring (replacing the `__total` callout from `PagedResult`).
- The `ExecutePagedAsync` XML docstring: drop the `__total` mention; redirect to the `WindowResult` doc.

Because consumers can supply an explicit alias via `WindowedAggregate.Alias`, callers who hit a real schema collision have an escape hatch — they pass `new WindowedAggregate(..., alias: "_my_safe_name")`.

---

## 6. Eager Read / SQLite Buffering / Streaming

The existing pattern from `PagedResult` carries over verbatim into `ExecuteWindowedAsync<TWindow>`, with one substitution: instead of extracting a `long` from `__total`, the implementation extracts a `TWindow` from the resolved alias (caller-supplied or `__window`). All other behavior is identical: eager first-row read inside the method body before the outer `Task` returns; on `!MultipleConnectionsSupported` (SQLite) buffer all remaining rows and release the semaphore on dispose; on multi-connection dialects stream from the held reader and release on iterator dispose; zero-row → `TaskCompletionSource<TWindow>.TrySetResult(default)`; cancellation propagates as `OperationCanceledException` to both `WindowValue` and the iterator without `StatementException` wrapping. The `LockedDBClient` semaphore-release-on-throw path from §3.4 / §6.1 of the prior design doc is unchanged. `ExecutePagedAsync`'s pre-existing buffering / streaming code disappears — the primitive owns it.

---

## 7. Test Plan

### 7.1 Existing tests — rename and retarget

Existing files under `Ocelot.Tests/Operations/`:

- `ExecutePagedAsyncTests.cs` — assertions still hold; rename type references `PagedResult<T>` → `WindowResult<T, long>` and `paged.Total` → `paged.WindowValue`. No semantic change.
- `ExecutePagedAsyncSingleStatementTests.cs` — same rename pass. The single-statement assertion is the load-bearing one for both `ExecutePagedAsync` and `ExecuteWindowedAsync`.
- `Ocelot.Tests/Postgres/PostgresExecutePagedAsyncTests.cs` (if present) — same rename pass.

### 7.2 New tests

Add `Ocelot.Tests/Operations/ExecuteWindowedAsyncTests.cs`:

| Test | Purpose |
|---|---|
| `ExecuteWindowedAsync_MaxOver_ResolvesMaxValue` | Insert N rows with varying score values; call `ExecuteWindowedAsync<int>(DB.MaxOver(scoreField))`; assert `WindowValue == max(score)` and `Items` contains all N rows. This is the headline test for the generic primitive — verifies a non-count aggregate works end-to-end. |
| `ExecuteWindowedAsync_SumOver_ResolvesSumValue` | Same shape, `DB.SumOver(field)`, `TWindow = long`. Verifies a second non-count aggregate. |
| `ExecuteWindowedAsync_CallerSuppliedAlias_RoundTrips` | Pass `new WindowedAggregate(..., alias: "_custom")`; verify the value extracts correctly via the supplied alias. |
| `ExecuteWindowedAsync_NullAggregate_Throws` | `ArgumentNullException` synchronous. |
| `ExecuteWindowedAsync_ZeroRows_ResolvesDefault` | Empty result set → `WindowValue == default(TWindow)`, `Items` yields nothing, exactly one statement executed. |
| `ExecuteWindowedAsync_SingleStatementAssertion` | Counting-decorator over `IDBClient`; assert exactly one `ReaderAsync`/`ReaderPreparedAsync` call. |
| `ExecuteWindowedAsync_RespectsLimitOffset` | Call `.Limit(K).Offset(M)` fluently before the primitive; verify only K rows return but `WindowValue` reflects the full result set (unwindowed total). |
| `ExecuteWindowedAsync_PreCanceledToken_FaultsImmediately` | Pre-canceled CT short-circuits before opening connection. |
| `ExecuteWindowedAsync_CancellationMidStream_FaultsBoth` | Cancel mid-`await foreach`; both `Items` and `WindowValue` (if not yet resolved) observe `OperationCanceledException`, not `StatementException`. |

Postgres-mirror file: `Ocelot.Tests/Postgres/PostgresExecuteWindowedAsyncTests.cs` — gated on `POSTGRES_CONNECTION`, mirrors the SQLite scenarios. Critically validates multi-connection streaming with `TWindow != long`.

### 7.3 What does *not* need new tests

The eager-read / SQLite-buffering / streaming behavior is exercised by the existing `ExecutePagedAsync*Tests` files (post-rename). Since `ExecutePagedAsync` now delegates to `ExecuteWindowedAsync<long>`, those tests effectively cover the primitive's behavior for the long-count case. The new tests above add coverage for the *generic* dimension only.

---

## 8. Migration / Breaking-Change Notes

Caller-visible diff for any consumer that adopted v0.18.0:

- `PagedResult<T>` → `WindowResult<T, long>` (everywhere).
- `paged.Total` → `paged.WindowValue` (everywhere).
- That's it. Nothing else.

`ExecutePagedAsync`'s parameter list is identical. No new `using` is needed (same namespace). The only build-breaking change is the type/property rename, which is a search-and-replace.

No `[Obsolete]` shim. No transitional alias. No back-compat property. Preview-mode rename, clean break, per Toni.

`Readme.md` (if it carries a paged-load example) gets updated in the same PR to use the new type and property names.

---

## 9. Out of Scope

- FieldMapper integration (DiVoid task 148) — separate PR, ships *after* this rename.
- `InjectTotalColumn` → token-list injection refactor (DiVoid task 142) — separate, lower priority. This doc keeps the string-injection helper, just renames and parameterizes it.
- Any change to the `WindowedAggregate` token itself — already general.
- Any deprecation shim, `[Obsolete]` alias, or back-compat name.
- Sync `ExecuteWindowed` / sync `ExecutePaged` — async-only.
- New `DB.RankOver` / `LagOver` / `LeadOver` factories — not blocking; defer.
- `IDBInfo.SupportsWindowFunctions` runtime probe — same posture as the v0.18.0 doc; not added.
- Any change to `LoadOperation<T>.Limit`/`Offset` semantics.
- Any change to the `__window` reservation behavior (overwrite-property-with-same-name) — same posture as `__total`'s original reservation.

---

## 10. Implementation Guidance for the Next Agent

Recommended build order. Single PR off `master`.

1. **Rename the type.** Create `Ocelot/Entities/Operations/Prepared/WindowResult.cs` with the two-generic-parameter shape from §2. Delete `PagedResult.cs`. Update the docstrings per §2.
2. **Implement the primitive.** Add `ExecuteWindowedAsync<TWindow>` (both transaction overloads) on `PreparedLoadOperation<T>`. Lift the body of the current `ExecutePagedAsync` (lines 707–791 of `PreparedLoadOperation.cs`) into the primitive, generalized: alias resolution per §3.3, value extraction as `TWindow`, `WindowResult<T, TWindow>` construction. Mirror on `LoadOperation<T>` as a pass-through.
3. **Refactor the helper.** Rename `InjectTotalColumn` to `InjectWindowedColumn`, change its signature per §4.4. The MSSQL `OFFSET ... ROWS FETCH NEXT ... ROWS ONLY` branch and the `ORDER BY (SELECT NULL)` synthesis stay, gated on `limit`/`offset` being non-null.
4. **Reduce `ExecutePagedAsync` to sugar.** On `PreparedLoadOperation<T>`: keep the limit/offset substitution work (prepared SQL is baked), but call `InjectWindowedColumn` with `DB.CountOver()` rendered + `__window` alias, then run the same reader path. The eager-read / buffering / streaming code is gone — that lives in the primitive only. On `LoadOperation<T>`: call `.Limit(limit).Offset(offset)`, `Prepare(false)`, then delegate to `ExecuteWindowedAsync<long>(DB.CountOver(), ct)`. Return type changes to `Task<WindowResult<T, long>>`.
5. **Rename existing tests** per §7.1. Update type and property references.
6. **Add new tests** per §7.2. The `MaxOver`-with-`int` test is the headline.
7. **Bump version.** `Ocelot/Ocelot.csproj` `AssemblyVersion`/`PackageVersion` to v0.19.0 (breaking type rename).
8. **Update `Readme.md`** if it carries the v0.18.0 paged example. Replace `PagedResult<T>` / `Total` references with `WindowResult<T, long>` / `WindowValue`.

### 10.1 Things to be careful about

- **The prepared `ExecutePagedAsync` path still mutates command text** (limit/offset substitution on baked SQL). The unprepared `LoadOperation<T>.ExecutePagedAsync` path uses fluent `.Limit()/.Offset()` and is the clean shape. Both must end up calling into the same `ExecuteWindowedAsync<long>` reader path so the eager-read / buffering logic exists in exactly one place.
- **Don't re-inject `__window` if the user already supplied an alias.** The primitive's alias-resolution logic should clone the `WindowedAggregate` with `Alias = "__window"` only when the caller's alias is null/empty. If the caller passed an alias, use that and don't touch the token.
- **The `WindowedAggregate.Alias` is currently emitted as `"AS <alias>"` in `ToSql`** (see `WindowedAggregate.cs:102-105`). The injection helper relies on the rendered SQL already including the `AS <alias>` suffix, so it should render the (alias-bearing) aggregate via the existing `ToSql` path rather than concatenating `" AS " + alias` manually. This keeps alias quoting / escaping consistent with the rest of the token system.
- **`OperationCanceledException` must not be wrapped in `StatementException`.** Same constraint as v0.18.0; carries over to the primitive.
- **`netstandard2.1` constraint** — same posture as v0.18.0; no .NET 6+ shortcuts.

---

End of design.
