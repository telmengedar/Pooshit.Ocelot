# Postgres Prepared-Statement Resilience — Architectural Design

> DiVoid task **#3270** · repro evidence **#3287** · diagnosis **#3266** · connection concept **#3231** · DBClient node **#3086**.
> Design Contracts **#1136** (§1 KISS/DRY/YAGNI load-bearing) and Code Contracts **#114** §0 are load-bearing for this document.
> Supersedes the direction of PR #10 (`fix/postgres-prepared-statements`) — see §12.

## 1. Problem Statement

A **held, re-executable prepared statement must survive a physical connection loss transparently** — reconnect, re-prepare, and retry within the same call — instead of throwing on the first re-execution after an idle period, and **without ever double-applying a write**.

User's verbatim framing (load-bearing, quoted so the design is checked against the words, not an elaboration):

> "you can not drop the prepare method - the whole point is that you have a prepared statement in hand which you can execute without rebuilding it."

> "before we claim something as working we try harder to actually reproduce the reported bug. Prepare a statement, execute the prepared statement on the prepared object multiple times - wait 3 minutes and then use the same prepared statement again."

Success criterion: a held prepared **read** statement over Ocelot's single-shared-connection model, executed, then re-executed after the physical connection has been lost (idle-session timeout / `pg_terminate_backend` / firewall idle-TCP reset), succeeds transparently on the re-execution. Writes are covered by an explicit, data-integrity-safe rule (§9).

## 2. Scope & Non-Scope

**In scope**
- A recovery mechanism in the client execution layer that, on a classified connection-lost failure, reconnects + re-prepares + retries **once** for **read** operations.
- A dialect-owned classifier that decides whether a raw driver exception is a connection-loss.
- The salvage fixes that are correct regardless of the recovery design (genuinely-async prepared non-query; explicit parameter typing) — shipped as the **first increment** bundled with this design (§11).

**Explicitly NOT in scope (non-scope)**
- **Transparent auto-retry of writes** (`NonQuery`). Structurally excluded for data-integrity reasons (§9). Left as an Open Question (§13) for an explicit opt-in should Toni want it — not built speculatively (YAGNI, #1136 §1).
- **True single-retained-`DbCommand` reuse.** Rejected in favour of re-prepare-with-recovery (§10, decision D2).
- **Mid-result-stream recovery** (connection dies while iterating a large already-open reader). Not the reported failure; ambiguous and rare — YAGNI.
- **Enabling `PreparationSupported` / removing manual `command.Prepare()`.** The flag flip and the recovery wiring are John's **follow-up PR** (§11), gated on this design. This increment leaves `PreparationSupported = false` and manual `Prepare()` intact.
- Pooler (`PgBouncer`) topologies. Production is a direct connection (#3287); `26000` is included in the classifier only as a zero-cost future-safety token, not a supported deployment.
- SQLite / MySQL / MSSQL recovery. Base classifier returns "not a connection-loss"; only Postgres opts in (YAGNI; SQLite is single-connection-locked anyway).

## 3. Assumptions & Constraints

| # | Assumption / Constraint | Source |
|---|---|---|
| A1 | Production Postgres is a **direct** connection, no PgBouncer. | #3287 (Toni-confirmed) |
| A2 | The failing topology is Ocelot's **single-shared connection** (`ClientFactory.Create(DbConnection, info)`, `disposeconnection=false`), which holds one physical connector idle between executes. | #3231, #3287 |
| A3 | The **pooled factory** (`Create(Func<DbConnection>, …, disposeconnection:true)`) never reproduces the bug — a fresh connector per execute. Recovery is a no-op there and must not perturb it. | #3231, #3287 |
| A4 | On the shared model, `ConnectionProvider.Connect()` already reopens a non-`Open` connection; the death is *discovered at first I/O*, so today's failure surfaces on the failing call and self-heals on the **next** call. | #3231, `ConnectionProvider.cs` |
| A5 | `StatementException` wrapping (command text + parameters; `OperationCanceledException` passes through) is a hard convention any new execution path must preserve. | #3231, #3086 |
| C1 | `DBClient` is engine-agnostic — it must not reference Npgsql. Any driver-specific error classification lives in `IDBInfo`/`PostgreInfo`. | #3086, #3231 |
| C2 | KISS/DRY/YAGNI are bouncing-grade constraints. | #1136 §1, #1333 |

## 4. Architectural Overview

Recovery is a **single retry-once decorator around the command-execution step of the read paths in `DBClient`**, keyed by a **dialect-owned connection-loss classifier**. Nothing new is introduced at the operation-pipeline or entity layer; the seam is the one place every statement already exits to ADO.NET and already wraps failures.

```
 EntityManager / PreparedOperation        (unchanged — still re-supplies text+params per execute)
              │
              ▼
        DBClient read method  ── execute ──►  driver
              │  catch(e)
              ▼
   DBInfo.IsConnectionLost(e)?  ── no ──►  wrap in StatementException  (today's behaviour)
              │ yes  (READ paths only)
              ▼
   dispose failed command → Connect() [reopens shared conn] → re-PrepareCommand → execute ONCE more
              │  fail again ─────────────────────────────────────────────►  wrap in StatementException
              ▼
           result  (transparent recovery)
```

The user's "prepared statement in hand" (a held `PreparedOperation` carrying SQL text + constant params) is preserved unchanged: it re-supplies text+params on every `Execute`, and the client rebuilds+re-prepares the `DbCommand` against the (now-reopened) held connection. The prepare method is **not** dropped (satisfies the user's constraint).

## 5. Components & Responsibilities

| Component | Owns | Does NOT own |
|---|---|---|
| **`PreparedOperation` / `PreparedLoadOperation` (operation layer)** | Holding SQL text + constant params; re-supplying them per execute; routing to `*Prepared` vs plain client methods via `dbprepare && PreparationSupported`. | Connection lifecycle; recovery; error classification. **Unchanged by this design.** |
| **`DBClient` (client layer)** | Command build, execute, `StatementException` wrapping; **the retry-once recovery loop on read paths.** | Deciding *what counts* as a connection-loss (delegates to `IDBInfo`); driver specifics. |
| **`IDBInfo` / `DBInfo` (dialect contract/base)** | The `IsConnectionLost(Exception)` contract; base returns "no". Parameter creation incl. explicit typing (salvage). | Execution flow; retry. |
| **`PostgreInfo` (dialect impl)** | Classifying Npgsql/Postgres connection-loss (`57P01`, `57P05`, transport-reset `08xxx`, `26000`, "connection is not open"). | Retry mechanics. |
| **`ConnectionProvider` (connection layer)** | Producing/opening connections; reopening a non-`Open` shared connection. **Unchanged** — reused as the reconnect primitive. | Classification; retry. |

Single-responsibility check: classification (dialect) is separated from mechanism (client) from transport (connection). No component gains a second reason to change.

## 6. Interactions & Data Flow — the recovery sequence (read path)

1. `DBClient` read method materialises `parameters` once (so it can be re-enumerated), calls `PrepareCommand` (connect + build + manual `Prepare()` when enabled), and executes.
2. On exception `e` that is **not** `OperationCanceledException`: ask `DBInfo.IsConnectionLost(e)`.
   - **No** → wrap in `StatementException` (unchanged).
   - **Yes** and this is the **first** attempt → dispose the failed `PreparedCommand` (on the shared model this closes nothing — `disposeconnection=false` — leaving the broken connection for `ConnectionProvider` to reopen), then `Connect()` again (reopens the shared connector / gets a fresh pooled one), `PrepareCommand` again, execute **once** more.
3. If the retry also fails (any exception) → wrap in `StatementException`. **No second retry.**

On the pooled model (A3) step 2's "yes" branch is essentially never taken because the first execute already runs on a healthy connector; the classifier + retry are inert there.

## 7. Data Model (Conceptual)

No entities, tables, or schema change. The only conceptual state is per-call and transient: the (text, materialised-params) pair re-used for the single retry.

## 8. Contracts & Interfaces (Abstract)

**`IDBInfo.IsConnectionLost(exception) → bool`** (new)

| Aspect | Semantics |
|---|---|
| Input | The raw driver exception caught before `StatementException` wrapping. |
| Output | `true` iff the exception denotes a lost/broken physical connection for which a fresh connect+re-prepare is the correct recovery. |
| Base default | `false` (no engine opts in unless it overrides). |
| Postgres impl | `true` for `PostgresException.SqlState` ∈ {`57P01` admin-terminate, `57P05` idle-session-timeout, `26000` prepared-stmt-missing (future pooler safety)}; for transport-level `NpgsqlException` wrapping a socket/IO reset (`08xxx` class); and for the "connection is not open" `InvalidOperationException`. Anything else → `false`. |
| Invariant | Pure predicate, no side effects; never throws. Misclassifying a non-connection error as connection-loss would cause an unsafe retry, so the impl errs narrow (only the enumerated signals). |

**Read execution methods (`Query`/`Scalar`/`Set`/`Reader` + `*Prepared`/`*Async` variants)** gain the recovery loop. **`NonQuery*` methods do not** — this structural split *is* the retry-safety rule (§9). `StatementException` wrapping and `OperationCanceledException` pass-through are preserved in both the first attempt and the retry.

## 9. Cross-Cutting Concerns

- **Retry-safety (the crux).** A read is idempotent → auto-retry-once is unconditionally safe. A write (`NonQuery` INSERT/UPDATE/DELETE) is **not**: a connection-loss discovered during a write cannot be distinguished *by error class alone* between "socket died before the backend applied it" (safe) and "backend committed but the ack was lost" (retry double-applies). Because the ORM has no per-statement idempotency knowledge, **writes are excluded from auto-retry**. They retain today's behaviour: one `StatementException` at the idle-drop boundary, then self-heal on the next call — the caller, who *does* know its idempotency, may retry. The rule is expressed **structurally** (read methods get the wrapper; `NonQuery*` do not) so there is no runtime "is this a write" heuristic to get wrong.
- **Error handling / observability.** `StatementException` (text + params) is preserved on both the first failure-that-triggers-recovery being swallowed-then-retried and on a failed retry. The retry is transparent on success; a failed retry looks identical to today's single failure.
- **Consistency model.** Unchanged. Recovery re-establishes a connection; it does not span a transaction. A statement executed inside an explicit `Transaction` whose connection dies is a transaction abort — recovery does **not** silently re-run it on a new connection (the transaction's connection is owned by the `Transaction`, not re-opened by the read-path loop); it surfaces as `StatementException` as today.
- **Concurrency / single-connection.** SQLite (`MultipleConnectionsSupported=false`) does not opt into the classifier (base returns `false`), and its `LockedDBClient` serialization is untouched.
- **Security.** No new surface; no credentials in scope.

## 10. Quality Attributes & Trade-offs

**Decision D1 — retry only reads, not writes.** *Chosen over* blind write-retry (rejected: double-apply, data corruption) and transaction-wrapping-every-write (rejected: KISS/YAGNI — imposes a transaction on every statement to cover a rare drop). The safe default is architecturally forced by the data-integrity ambiguity, not a preference. Trade-off named: a held **write** prepared statement is *not* made transparent by this design; it still costs one failed op per idle drop. Probability/cost: acceptable — the reproduced case (#3287) is a read (scalar), writes can't be auto-covered safely, and the caller retains the self-heal-next-call escape hatch.

**Decision D2 — re-prepare-with-recovery, not a true retained `DbCommand`.** *Chosen over* holding a single `DbCommand` bound to the connection. Rationale (KISS/YAGNI, #1136 §1): (a) a retained command is bound to the *dead* connection and makes connection-loss strictly worse (the handle must be rebuilt on recovery anyway); (b) Ocelot already achieves server-side plan reuse via the held connection's per-connector cache when `Prepare()` is enabled, so re-prepare-per-execute is not a correctness gap; (c) it preserves the user's mental model ("execute without rebuilding") at the API level — the `PreparedOperation` object *is* the held statement; the per-call `DbCommand` rebuild is an invisible implementation detail. `can-it-be-deleted/merged/inlined` (#1136 §4): the recovery loop is a single helper reused across the read methods (DRY) — it cannot be deleted (it is the feature) and cannot be inlined per-site without duplicating the classify+reconnect+retry block across ~6 methods.

**Scalability/availability.** Recovery converts a guaranteed per-idle-drop read failure into a transparent success; no added steady-state cost (the classifier runs only on the exception path).

## 11. Migration / Rollout Strategy — increments

**Increment 1 (this PR — bundled with this design):** the salvage fixes, correct regardless of the recovery design, branched **fresh from master** (not from PR #10's branch):
- `DBClient.NonQueryPreparedAsync` made genuinely `async`/`await` (fixes early-dispose, #3233 / #3266 surface 4).
- Explicit `DbType` parameter typing (`MapToDbType`) in `DBInfo.CreateParameter` and the scalar branch of `PostgreInfo.CreateParameter`; nulls stay untyped (driver infers), ranges/arrays keep driver inference (#3266 surface 1).
- Tests: async-prepared repeated-insert persistence (SQLite) + parameter-typing assertions.
- **Leaves `PreparationSupported=false` and manual `Prepare()` intact.** No behaviour is enabled; the `DbType` typing improves every existing ad-hoc Postgres query immediately.

**Increment 2 (John follow-up PR, gated on this design):**
- Add `IDBInfo.IsConnectionLost` (base `false`) + `PostgreInfo` classifier.
- Add the read-path retry-once recovery loop in `DBClient`.
- Reorder `Prepare()` after `Transaction` assignment; add async prepare (#3266 surfaces 5-6) as part of enabling.
- Flip `PostgreInfo.PreparationSupported = true`.
- **Load-bearing regression test** (specified here, implemented there): a held prepared **read** over the single-shared model — execute, forcibly lose the connection (`SELECT pg_terminate_backend(...)` and/or `SET idle_session_timeout` + wait), re-execute — must succeed transparently. Validate against Postgres 16 on bazzite (`POSTGRES_CONNECTION`).

## 12. Relationship to PR #10 (superseded direction)

PR #10 enabled prepared statements by **removing** manual `Prepare()` and flipping the flag, relying entirely on Npgsql auto-prepare — the direction Toni rejected ("you can not drop the prepare method"). This design **keeps** the prepare method and adds resilience around it. Salvaged from PR #10: the async fix and the `DbType` typing (Increment 1). Reverted: the `Prepare()` removal and the flag flip (branch is fresh from master).

## 13. Open Questions

1. **Transparent write recovery — opt-in?** The safe default is "writes do not auto-retry" (§9). If Toni wants held **write** prepared statements to also recover transparently, the only safe mechanism is an **explicit per-operation opt-in** (caller asserts idempotency) or transaction-scoping — a bounded Increment-3, not built now (YAGNI). *Does Toni want this, or is reads-only sufficient?* (Reads-only fully covers the reproduced #3287 case.)
2. **`26000` inclusion.** Kept in the classifier as zero-cost future pooler-safety though production is direct-connect. Confirm this is acceptable dead-until-needed, or drop it (strict YAGNI).

## 14. Implementation Guidance for the Next Agent (Increment 2)

Ordered, architectural-unit level (no code):
1. Add `IsConnectionLost` to the `IDBInfo` contract; base `DBInfo` returns `false`. Preserve the pure-predicate/never-throw invariant.
2. Implement the `PostgreInfo` classifier over the enumerated signals (§8). Keep it narrow.
3. Introduce a single private recovery helper in `DBClient` that: materialises params once, runs the execute, and on a classified first failure disposes+reconnects+re-prepares and executes exactly once more, preserving `StatementException`/cancellation semantics.
4. Apply the helper to the **read** execution methods only (`Query`/`Scalar`/`Set`/`Reader` + `*Prepared`/`*Async`). Leave `NonQuery*` untouched — the structural retry-safety boundary.
5. Address the enabling prerequisites (`Prepare()` after `Transaction`; async prepare) then flip `PostgreInfo.PreparationSupported = true`.
6. Add the load-bearing regression test (§11) and validate on bazzite; confirm the pooled path is unperturbed and the SQLite suite is green.

---

## Appendix A — Pre-Design Checklist (#1136 §5)

**KISS / DRY / YAGNI**
- No new type mirroring an existing type — the classifier is one predicate method on the existing `IDBInfo`; no new class.
- No new abstraction with a single implementation-and-no-second — `IsConnectionLost` has a base (`false`) and a Postgres impl, and is a natural extension point for other engines.
- No element justified by "might need later" — write-recovery and `26000` are explicitly parked as Open Questions, not built.
- No deprecation window / feature flag / shim (private lib, atomic release).
- Inline-vs-extract math (D2): the recovery block (classify + reconnect + re-prepare + retry, ≈8 lines) × ≈6 read methods = ≈48 → **above the ~15-20 threshold → extract one helper** (DRY). Named in 1-3 words (`RetryOnConnectionLoss`). Documented as decision D2/§10.

**Existing systems first**
- Recovery lives on the existing `DBClient` execution seam and reuses `ConnectionProvider` as the reconnect primitive; no new layer. Concrete reason it can't live elsewhere: it is the only place that both catches the raw driver exception and holds the (text, params) needed to re-prepare.
- No new persisted data.

**Configurability**
- No new config knob. Retry count is fixed at **one** (a `const`-grade magic number: a second retry cannot help a deterministic failure and risks masking a real outage). No operator will tune it (#1136 §3).

**Less is better**
- Every element passed can-it-be-deleted (classifier = the decision; helper = the mechanism; both load-bearing) / merged (classifier belongs on the dialect, not the client) / inlined (helper crosses the DRY threshold).
- Trade-offs named explicitly: D1 (reads-only) and D2 (re-prepare vs retained command) in §10.
- No consumer-less surface preserved.

**Data deliverables** — none (no SQL/migration/backfill).

**Document discipline**
- Cites Code Contracts (#114) and Design Contracts (#1136) as load-bearing (header).
- Scope/non-scope explicit (§2). Open Questions explicit (§13). Predecessor (PR #10) marked superseded (§12).
- No multi-paragraph "why keep X" for things that obviously stay.
