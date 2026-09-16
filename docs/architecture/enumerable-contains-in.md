# Architectural Document: `Enumerable.Contains` in predicates (bug #178)

Repo path: `docs/architecture/enumerable-contains-in.md` (branch `fix/enumerable-contains-in`, off `master` `dc48b37`) — DiVoid documentation node **#14161**. Both copies carry the same bytes; the repo file is canonical once merged.

**Status:** Proposed — revision 2 (2026-09-16; r2 after QA #14167 REJECTED on CF-1 — the production change is upheld, the amendment is to the design's premise and coverage; changes marked *r2*)
**Author:** Sarah (Software Architect)
**Branch / worktree:** `fix/enumerable-contains-in` · `.claude/worktrees/contains-in`
**Tracking:** DiVoid bug #178 · umbrella task #14135 (red-suite cluster 4 of 4) · consumer guidance #177 · QA #14167
**Load-bearing standards:** Design Contracts #1136 · Code Contracts #114 (§0, §4) · coverage-row rule #1220 §9 · autonomy #8727

---

## TL;DR

**What.** `array.Contains(row.Field)` and `DBParameter<T[]>.Value.Contains(row.Field)` inside an Ocelot predicate render again — as `IN( … )` with bound parameters on SQLite / MySQL / MSSQL and as `= ANY( … )` with a native array bind on Postgres — exactly what the equivalent `row.Field.In(…)` renders today. The four red tests in `EntityManagerTest` and `ParameterTests` go green.

**How.** One arm of `CriteriaVisitor.VisitMethodCall` widens: the `Contains` case accepts the method whether the compiler bound it to `System.Linq.Enumerable` (C# ≤ 13) or to `System.MemoryExtensions` (C# 14 first-class spans, which is what the .NET 10 SDK on this machine emits for the test project), strips the span-conversion wrapper the C# 14 binding puts around the collection operand, and hands value and collection to the dialect's existing `CreateInFragment` — the same call `.In(…)` makes. No dialect changes, no new helper, no new type. The commented-out block below the arm is deleted.

**Cost.** Zero behavioural change for anything that renders today. One visitor arm grows by two conditions; twelve rendered-SQL tests are added to `CriteriaVisitorTests` (*r2:* was nine — the two hand-built-tree tests are replaced by source-written enum-array tests, and three rows are added for the two QA survivors and the SQLite `.In` parity twin).

**Rejected.** #178's Options A and B (re-teach the visitor / the dialects the two shapes): measured to be unnecessary — the existing `Enumerable` arm already renders both shapes correctly; the tests are red because that arm is never reached. Pinning the test project to `LangVersion` 12/13: turns the suite green while every consumer compiling under C# 14 stays broken — the "hide it" shape Toni rejected.

---

## 0. The ask (verbatim, Toni, 2026-09-16)

> "should the 7 failing tests be the postgres tests then mark them as explicit calls so a regular test run doesn't fail anymore"

The four tests in this cluster are not Postgres-dependent — they run against SQLite in-memory. The answer given in #14135 was *fix, not hide*; this design fixes.

## 1. Problem Statement

Four tests on `master` are red with two exception types from the same line:

| Test | Exception | Where it is thrown |
|---|---|---|
| `EntityManagerTest.TestContains`, `DoesNotContain` | `NotSupportedException: Specified method is not supported` | `CriteriaVisitor.VisitMethodCall`, last statement (the reflective evaluate-to-constant fallthrough) → `GetHost` → `MethodInfo.Invoke` |
| `ParameterTests.ParameterArrayContains`, `ParameterArrayWithOtherParametersContains` | `NotImplementedException: Field has no implementation since it is only used for typed expressions` (wrapped in `TargetInvocationException`) | same fallthrough → `GetHost` evaluates the `DBParameter<int[]>.Value` sentinel getter |

**Measured root cause (this design, 2026-09-16, SDK 10.0.203).** The test project sets `<LangVersion>default</LangVersion>`, which under the .NET 10 SDK's compiler is **C# 14**. C# 14 *first-class spans* make `array.Contains(x)` bind to `MemoryExtensions.Contains<T>(ReadOnlySpan<T>, T)`, and in an expression tree the compiler materialises the array→span conversion as a call node `ReadOnlySpan<T>.op_Implicit(array)` around the collection operand. The visitor's `Contains` arm tests `DeclaringType == typeof(Enumerable)`; that is false, so the call falls through to the reflective evaluator, which cannot invoke a method returning a by-ref-like type (`NotSupportedException`) — or, for the prepared-statement shape, evaluates the `DBParameter` sentinel first (`NotImplementedException`).

**What is *not* broken (measured, same session; *r2:* boundary sharpened by QA #14167 §1).** The `Enumerable` binding renders today, on the unmodified `master` visitor: a hand-built `Enumerable.Contains<int>` tree renders `[integervalue] IN( @1 , @2 )` on SQLite and `"integervalue" = ANY( @1 )` on Postgres, with negation as `NOT …`; under `LangVersion` 12 the compiled predicate binds to `Enumerable` with a plain field-access operand, i.e. the four tests would pass. *r2:* the C# 14 rebinding is **not** universal. `MemoryExtensions.Contains<T>` carries `where T : IEquatable<T>`, so the measured boundary on SDK 10.0.203 is: `int[]`, inline `new[] {…}`, `DBParameter<int[]>.Value`, `string[]` (every `IEquatable<T>` element type) bind to `MemoryExtensions.Contains<T>(ReadOnlySpan<T>, T)` with the `op_Implicit` wrapper; a `TestEnum[]` closure (enums do not implement `IEquatable<T>`), an `IEnumerable<int>` closure (interface-typed collections take no span conversion) and the three-argument comparer overload bind to `Enumerable.Contains<T>` with a plain `MemberAccess` operand. The `Enumerable`-bound source shapes render correctly on `master` today (`[enum] IN( @1 , @2 )`, `[integer] IN( @1 , @2 )` measured) — which means a source-written enum-array predicate is a live guard for the `Enumerable` half of the gate and no hand-built tree is needed. So #178's stated root cause — that `CreateInFragment` is "naive" and the commented-out block is the missing logic — is wrong: the `Enumerable` arm plus `CreateInFragment` delegation already handles closure arrays, inline array literals, `DBParameter<T[]>.Value`, subqueries and negation, because it is the same path `.In(…)` takes (`InCollectionTests`, `CriteriaVisitorTests.TestInWithArray` / `TestNotInWithArray`, `LoadValuesOperationTests.ExecuteSet` are green on that path). This is a **conflict with the fixed facts in the brief and in #178** and is surfaced in §16; the design below fixes the measured cause.

Why it matters beyond the suite: consumers compile their own lambdas. mamgo-backend (or any consumer) on C# 14 produces the `MemoryExtensions` binding regardless of which SDK builds Ocelot; #177's fourteen rewritten sites are this shape. Ocelot must accept both bindings.

Success criteria: the four named tests pass; every `.In(…)` test still passes; the rendered SQL for `x.Contains(field)` is token-identical to `field.In(x)` on every dialect.

## 2. Scope & Non-Scope

**In scope**

- `CriteriaVisitor.VisitMethodCall`, the `Contains` arm only: accept both static bindings, unwrap the span conversion on the collection operand, delegate to `IDBInfo.CreateInFragment`.
- Deleting the commented-out original block under that arm (#114 §4 — dead prose in the touched region).
- Rendered-SQL guard tests in `CriteriaVisitorTests` for both bindings, both dialect families, both collection shapes, and negation.

**Out of scope (explicitly)**

- `IDBInfo.CreateInFragment` and `PostgreInfo.CreateInFragment`: unchanged. The Postgres `= ANY(…)` path is the correct array shape and is not touched.
- Instance-method `Contains` on `List<T>` / `HashSet<T>` / `ICollection<T>` (`node.Object` is the collection): never rendered by the visitor, no consumer has reported it, #177 remains the guidance. Raised in §15, recommended *no*.
- Three-argument `Contains(source, value, comparer)` on either static class: not supported before, not supported after; the arm's existing `default` throw covers it.
- The commented-out `SupportsArrayParameters` branch in `VisitNewArray` and the pre-existing rendering `"integer" = ANY( @1 , @2 )` for `field.In(new[] {…})` on Postgres (measured this session; `ANY` takes one array, so that text is suspect). Different region, different bug — filed separately as DiVoid bug #14160, not fixed here.
- The test project's `<LangVersion>default</LangVersion>`: left as is. It is what exposes the C# 14 binding to the suite, which is the binding consumers will use.
- `Function.Contains<T>(Range<T>, object)` (Postgres range containment): a different arm (`Function`), untouched.

## 3. Assumptions & Constraints

- The `MemoryExtensions.Contains` overload that matters takes two arguments, the first being `ReadOnlySpan<T>`; the compiler wraps a `T[]` operand in a single `op_Implicit` call declared on `ReadOnlySpan<T>` (measured: `ReadOnlySpan<Int32>::op_Implicit`, node type `Call`, one argument, no receiver). *r2 (QA W-5):* the `Span<T>` overload has **no producing shape** — even an explicit `MemoryExtensions.Contains<int>(array, x)` selects the `ReadOnlySpan<T>` overload, and a `Span<T>`-typed operand cannot otherwise appear in an expression tree — so the unwrap recognises `ReadOnlySpan<>` only (#1136 §6, defensive code for impossible scenarios). Overloads with a comparer take three arguments and are refused by arity.
- *r2:* the rebinding applies only to element types implementing `IEquatable<T>` (the constraint on `MemoryExtensions.Contains<T>`); enum arrays and interface-typed collections keep the `Enumerable` binding under C# 14 (§1). Both halves of the gate are therefore reachable from source-written predicates in this test project.
- `DBParameter<T[]>.Value` reaches `AppendMemberValue` as a `MemberAccess` on a `DBParameter<>` declaring type and emits an array parameter token; `ParameterToken` renders it as `[n]` on dialects without array parameters and as `@n` on Postgres; `PreparedArrayOperation` expands `[n]` at execution. This is the existing, tested `.In(DBParameter<T[]>.Value)` mechanism (`LoadValuesOperationTests.ExecuteSet`, `PostgresLocalTests` lines 108–184).
- `AppendConstantValue` never inlines an array element as a literal: every element becomes a parameter, or the whole array becomes one array parameter on Postgres (measured: `@1 , @2` / `@1`). #14114 closed the literal-inlining surface; this design adds no new emission path.
- No CI gates the suite (#14135); the goal is a green `dotnet test Ocelot.sln` on `master`.

## 4. Design decisions (the questions the brief left to the architect)

### 4.1 Where the shape knowledge lives — the visitor arm, and only as *recognition*, not *rendering*

#178 framed the choice as "visitor pre-dispatches on shape (A)" versus "dialect becomes shape-aware (B)". Both presuppose that rendering the two shapes is missing. It is not: `CreateInFragment` receives the collection expression and calls the visitor on it, and the visitor already renders a closure array (`VisitMember` → constant array → parameters), an inline literal (`VisitNewArray` → per-element visit), a `DBParameter<T[]>.Value` (`AppendMemberValue` → array parameter) and a subquery (`AppendConstantValue` → nested prepare). The only knowledge the visitor lacks is *which method call is a Contains* and *that the C# 14 binding wraps the collection*. Both are facts about LINQ expression shape, so they live in the visitor's `Contains` arm. The dialect keeps deciding syntax (`IN(…)` vs `= ANY(…)`) and learns nothing new — which is #178's own argument for A, applied to a smaller change than A.

### 4.2 The `DBParameter<T[]>` array bind renders in the dialect's array form

The brief asks whether the array-bind shape should render `IN (@p)` or the dialect's form on Postgres. It renders whatever `CreateInFragment` renders for `.In(DBParameter<T[]>.Value)`: `"col" = ANY( @1 )` on Postgres (Npgsql binds the array natively; `IN @p` is not valid Postgres), `[col] IN( [0] )` on SQLite / MySQL / MSSQL (expanded at execution). Any other answer would make `Contains` and `.In` diverge for the same intent.

### 4.3 Expanded constants are parameters

Closure arrays and inline literals go through `AppendConstantValue`, which emits one parameter per element (or one array parameter on Postgres). No literal inlining is introduced; there is no path in this design that appends element text.

### 4.4 Negation survives unchanged

`!x.Contains(f)` is a `Not` unary over the call; `VisitUnary` emits `NOT` and visits the operand. Measured on the `Enumerable` binding: `NOT [integervalue] IN( @1 , @2 )` and `NOT "integervalue" = ANY( @1 )`. Both dialects parse `NOT` below `IN` / `=`, so the semantics are "not a member". `EntityManagerTest.DoesNotContain` pins it by row count; two rendered tests pin the text.

### 4.5 The commented-out block is deleted in the same change

#114 §4: no body comments; the block is dead prose in the touched region, and (per §1) it is not the missing logic. It goes.

### 4.6 No catch-and-rethrow diagnostic wrapper — YAGNI

#178 proposes wrapping the arm in a try/catch that rethrows with a pointer to `.In(…)`. With the fix, the supported shapes do not throw, so the wrapper would only fire for shapes the arm does not recognise — and those never enter the arm; they fall to the reflective evaluator like every other unrecognised call, where a wrapper around `Contains` cannot see them. The wrapper would be dead code on day one. Dropped. The arm's existing `default` throw (wrong arity) stays as the one place a recognised-but-unsupported `Contains` overload is refused; its message names the method so the reader is not left with a bare exception.

### 4.7 Why not a general "span conversions are transparent" rule

A visitor-wide rule that any `op_Implicit` declared on `ReadOnlySpan<>` visits through to its operand would also fix this — and would silently apply to any future span-typed expression anywhere in a predicate. Nothing else in the visitor produces or consumes spans; the only known producer is this binding. The narrower placement (inside the `Contains` arm, on the collection operand only) is the KISS choice; if a second span-conversion site ever appears, promote the unwrap then, with the shape in hand.

## 5. Components & Responsibilities

| Component | Owns | Does not own |
|---|---|---|
| `CriteriaVisitor.VisitMethodCall` — `Contains` arm | Recognising a two-argument static `Contains` declared on `System.Linq.Enumerable` or `System.MemoryExtensions`; normalising the collection operand by stripping one span-conversion call (`op_Implicit` declared on a constructed `ReadOnlySpan<>`, one argument — *r2:* `Span<>` dropped, W-5) and nothing else (a one-argument call that is not that conversion is a value expression and stays); passing value (argument 1) and collection (argument 0) to `IDBInfo.CreateInFragment` with the visitor as callback; refusing other arities with the existing throw | Any SQL text; any parameter emission; any dialect difference |
| `IDBInfo.CreateInFragment` (base and Postgres override) | Wrapping the two visited operands in the dialect's membership syntax | Unchanged |
| `VisitMember` / `VisitNewArray` / `AppendMemberValue` / `AppendConstantValue` | Rendering the collection operand as parameters / array parameter / nested statement | Unchanged |

Nothing new is introduced: no helper type, no interface member, no dialect override.

## 6. Interactions & Data Flow (key flows)

**Flow 1 — closure array, C# 14 consumer, SQLite.** Predicate `array.Contains(e.IntegerValue)` arrives as a call to `MemoryExtensions.Contains<int>` with arguments [`op_Implicit(closure.array)`, `e.IntegerValue`]. The arm recognises the declaring type and arity, unwraps the first argument to `closure.array`, and calls `CreateInFragment(value: e.IntegerValue, collection: closure.array)`. The base dialect visits the value (`[integervalue]`), appends `IN(`, visits the collection (`VisitMember` → field on constant closure → `AppendConstantValue` with an `int[]` → `@1 , @2`), appends `)`.

**Flow 2 — same predicate, Postgres.** Identical until the dialect: `PostgreInfo.CreateInFragment` appends `= ANY(`; `AppendConstantValue` sees `SupportsArrayParameters` and emits one array parameter `@1`.

**Flow 3 — `DBParameter<int[]>.Value.Contains(v.Integer)`, prepared, SQLite.** Arguments [`op_Implicit(DBParameter<int[]>.Value)`, `v.Integer`]. Unwrap → `MemberAccess` on `DBParameter<int[]>`; `CreateInFragment` → `VisitMember` → `AppendMemberValue` → array parameter token → `[integer] IN( [0] )`; `PreparedArrayOperation` expands `[0]` with the runtime array at execution. With a second scalar parameter: `[integer] IN( [0] ) AND [double] = @1` (numbering measured on the `.In` twin).

**Flow 4 — C# ≤ 13 consumer.** Arguments [`closure.array`, `e.IntegerValue`] on `Enumerable.Contains<int>`; no wrapper to strip; otherwise Flow 1/2. This is today's behaviour, kept.

**Flow 5 — negation.** `VisitUnary` emits `NOT`, then Flow 1–4.

## 7. Data Model (Conceptual)

None — no entities, no persisted state.

## 8. Contracts (abstract)

**`Contains` arm — input.** A `MethodCallExpression` whose method is static, named `Contains`, declared on `Enumerable` or `MemoryExtensions`, with exactly two arguments. Argument 0 is the collection (possibly wrapped in one span conversion), argument 1 is the value.

**Output.** The preparator receives exactly the tokens `CreateInFragment` emits for the pair (value, unwrapped collection). Invariant: for any collection expression `c` and value expression `v`, the token sequence for `c.Contains(v)` equals the token sequence for `v.In(c)` on the same dialect. This is the property the coverage table pins with literals.

**Refusal.** Any other arity → the arm's existing throw, message naming the method. No other shape reaches the arm.

## 9. Cross-cutting concerns

- **Injection surface:** none added. All values reach SQL as parameters through paths #14114 already audited (`AppendConstantValue`, `AppendParameter`, `AppendArrayParameter`). The falsifier for the table below includes a row that would red if an element were ever emitted as text.
- **Dialects:** MySQL and MSSQL share the base `CreateInFragment`; they inherit the fix with no override. Postgres keeps `= ANY`.
- **Concurrency / async / transactions:** untouched; the change is purely in expression-to-token translation.

## 10. Test battery and coverage

**Files.** New rendered-SQL tests go into the existing `Ocelot.Tests/CriteriaVisitorTests.cs` (fixture already `[TestFixture, Parallelizable]`, already renders against `SQLiteInfo` with a real in-memory client and against `PostgreInfo` with a Moq `IDBClient` whose `DBInfo` is the Postgres info; `GetOperation(client, false)` assigns parameter numbers). The four execution tests are the existing red ones; they are not modified. Each test builds its own client/preparator (2026-08-08 ruling). *r2:* every predicate in the table is written in source — there is no hand-built tree, no reflection helper and no `[Description]`; the method name carries the case. Asserts use `Assert.That(…, Is.EqualTo(…))` (QA W-1). No comments.

**Expected literals are measured**, not predicted: every SQL string below was produced by the visitor through `GetOperation` (so parameter numbers are real) — the r1 rows on unmodified `master` via the `.In(…)` twin, the *r2* rows on the implemented tree in the `contains-in` worktree (QA #14167 §1–§2 and this revision's probe agree on every cell). `ValueModel.Integer` renders as `[integer]` / `"integer"`; `EnumEntity.Enum` renders as `[enum]` / `"enum"`.

**Premise that makes the rows discriminate (*r2*, replaces the r1 paragraph, which was false — QA CF-1):** the test project compiles under C# 14. A source-written `Contains` over an `IEquatable<T>` element type (`int[]`, `new[] {…}`, `DBParameter<int[]>.Value`, `string[]`) binds to `MemoryExtensions` with the span wrapper — the broken shape, red on `master`; those rows pin the `MemoryExtensions` half of the gate and the unwrap. A source-written `Contains` over a `TestEnum[]` closure binds to `Enumerable.Contains<TestEnum>` with a plain member-access operand — the C# ≤ 13 shape, green on `master`; those rows pin the `Enumerable` half of the gate (QA mutant M4 reds exactly them) with the real closure shape a compiler emits, which the r1 hand-built tree (a bare `ConstantExpression`, QA W-4) did not.

| Property | Guard test (name) | Predicate written / built | Expected literal | Why it discriminates | Mutation to run red |
|---|---|---|---|---|---|
| Closure array executes, SQLite | `EntityManagerTest.TestContains` (existing, red → green) | `array.Contains(e.IntegerValue)`, `array = {2, 3}` | 2 rows | rows are only returned if the predicate rendered and executed | drop `MemoryExtensions` from the recognised types — throws again |
| Negation executes, SQLite | `EntityManagerTest.DoesNotContain` (existing) | `!array.Contains(e.IntegerValue)` | 1 row, `IntegerValue == 1` | the complement set, so an `IN` without `NOT` returns 2 rows | make `VisitUnary` skip `NOT` for calls — 2 rows |
| Prepared array bind executes, SQLite | `ParameterTests.ParameterArrayContains` (existing) | `DBParameter<int[]>.Value.Contains(v.Integer)`, executed with `{1, 2}` | 2 rows, `Integer` 1 and 2 | the sentinel getter throws unless the unwrap precedes evaluation | remove the unwrap — `TargetInvocationException` again |
| Prepared array bind with a scalar parameter | `ParameterTests.ParameterArrayWithOtherParametersContains` (existing) | `DBParameter<int[]>.Value.Contains(v.Integer) && v.Double == DBParameter.Double`, executed with `{1, 2}, 1.0` | 1 row, `Integer == 2`, `Double == 1.0` | pins the array/scalar parameter numbering across the expanded placeholder | swap the arguments passed to `CreateInFragment` — rendering breaks |
| Closure array renders `IN`, SQLite | `TestContainsWithArrayRendersInOnSqlite` (new) | `array.Contains(m.Integer)`, `array = {2, 3}` | `[integer] IN( @1 , @2 )` | literal; a text-inlined element would render `2`, not `@1` | emit elements via `AppendText` — reds |
| Negated closure array renders, SQLite | `TestNotContainsWithArrayRendersNotInOnSqlite` (new) | `!array.Contains(m.Integer)` | `NOT [integer] IN( @1 , @2 )` | literal | — |
| Closure array renders `= ANY`, Postgres | `TestContainsWithArrayRendersAnyOnPostgres` (new) | `array.Contains(m.Integer)` | `"integer" = ANY( @1 )` | one array parameter, not two scalars — pins that the dialect path is `CreateInFragment`, not a visitor-side `IN` | hard-code `IN(` in the arm — reds |
| Negated, Postgres | `TestNotContainsWithArrayRendersNotAnyOnPostgres` (new) | `!array.Contains(m.Integer)` | `NOT "integer" = ANY( @1 )` | literal | — |
| Prepared array bind renders placeholder, SQLite | `TestContainsWithParameterArrayRendersArrayPlaceholderOnSqlite` (new) | `DBParameter<int[]>.Value.Contains(m.Integer)` | `[integer] IN( [0] )` | `[0]` is the array-parameter token; `@1` would mean the sentinel was evaluated as a constant | replace `AppendArrayParameter` with `AppendParameter` in `AppendMemberValue` — reds |
| Prepared array bind renders `= ANY`, Postgres | `TestContainsWithParameterArrayRendersAnyOnPostgres` (new) | `DBParameter<int[]>.Value.Contains(m.Integer)` | `"integer" = ANY( @1 )` | this is the brief's `IN (@p)` vs `= ANY(@p)` question, pinned | render `IN(` on Postgres — reds |
| Inline literal renders, SQLite | `TestContainsWithInlineArrayLiteralRendersInOnSqlite` (new) | `new[] {2, 3}.Contains(m.Integer)` | `[integer] IN( @1 , @2 )` | the operand under the span wrapper is a `NewArrayExpression`, a different node than the closure case | unwrap only `MemberExpression` operands — reds |
| `Enumerable` binding still renders, SQLite (*r2*, replaces `TestContainsBoundToEnumerableRendersInOnSqlite` and the reflection helper) | `TestContainsWithEnumArrayRendersInOnSqlite` (new) | `enums.Contains(e.Enum)` over `EnumEntity`, closure `TestEnum[] enums = { Crazy, Insane }` | `[enum] IN( @1 , @2 )` | enums are not `IEquatable<T>`, so this source predicate binds to `Enumerable.Contains<TestEnum>` under C# 14 — the only source-written way to reach the `Enumerable` half; the operand is the real `MemberAccess(Constant(closure), field)` shape | drop `Enumerable` from the recognised types (QA M4) — reds |
| `Enumerable` binding, Postgres (*r2*) | `TestContainsWithEnumArrayRendersAnyOnPostgres` (new) | same predicate | `"enum" = ANY( @1 )` | same | same |
| Unwrap precision (*r2*, QA W-2 / M6 survived) | `TestContainsDoesNotStripNonSpanWrapperCall` (new) | `OnlyFirst(enums).Contains(e.Enum)` on SQLite, where `OnlyFirst` is a private static helper in the fixture returning a one-element `TestEnum[]` from its argument | `[enum] IN( @1 )` | a one-argument call that is *not* the span conversion is a value expression; stripping it would visit the full closure array and render two parameters. SQLite only — on Postgres both renderings are one array parameter (`"enum" = ANY( @1 )` measured either way), so the row could not discriminate there | drop the `op_Implicit` name check or the `ReadOnlySpan<>` declaring-type check (QA M6) — reds on `@1 , @2` |
| Arity refusal (*r2*, QA W-3 / M7 survived; pins §8 "Refusal") | `TestContainsWithComparerOverloadThrows` (new) | `ints.Contains(m.Integer, EqualityComparer<int>.Default)` on SQLite (binds to the three-argument `Enumerable.Contains<int>`) | throws `NotImplementedException`; message contains `Contains[Int32]` and `IEqualityComparer` | with the arity guard removed the arm renders `[integer] IN( @1 , @2 )` and silently drops the comparer; only an exception assertion notices | drop the arity condition on the `Contains` case (QA M7) — reds |
| `.In` parity, SQLite (*r2*, QA W-6) | `TestInWithArrayRendersInOnSqlite` (new) | `m.Integer.In(intArray)`, `intArray = {2, 3}` | `[integer] IN( @1 , @2 )` | the same literal as `TestContainsWithArrayRendersInOnSqlite`; §8's token-identity invariant is now pinned two-sided on both dialects (Postgres already had `TestInWithArray`) | any divergence of the two paths — one of the pair reds |
| `.In` twin unchanged | `CriteriaVisitorTests.TestInWithArray`, `TestNotInWithArray`, `InCollectionTests.*`, `LoadValuesOperationTests.ExecuteSet` (existing, green) | `.In(…)` shapes | as today | the design claims token identity with `.In`; these rows are the other half of that claim | any change to `CreateInFragment` — reds |
| Range containment untouched | `PostgresLocalTests.BigIntegerRange` (existing, gated on `POSTGRES_CONNECTION`; `Function` arm) | `d.Range.Contains(11m)` | as today | the `Function` arm precedes and is not in the diff; the gated test is the only executor | — (no in-diff mutant; the diff must not touch the `Function` arm) |

**Sweep guard for the table itself:** for each row, *would this test still pass against an implementation lacking the property?* The execution rows would not (they are red now); the rendered rows assert full command text with parameter numbers, so an inlined literal, a wrong dialect wrapper, or an evaluated sentinel each changes the string. The range row is the one row with no mutant — stated as such rather than dressed up. *r2:* QA #14167 ran nine mutants against the r1 battery; M1–M5, M8, M9 red as predicted, M6 (unwrap precision) and M7 (arity) survived — the two rows above exist because they survived, and the r1 table's claim that §8's refusal was covered was false by omission (no row named it).

**Every named guard must exist** when John returns: the twelve new names above (*r2:* seven r1 names kept, `TestContainsBoundToEnumerable…` ×2 removed, five added) are the names the orchestrator greps for; renaming one means updating this table in the same diff (#1220 §9, third addendum).

## 11. Quality attributes & trade-offs

- **Maintainability:** one arm, two recognised declaring types, one unwrap. The `.In` and `Contains` paths converge on `CreateInFragment`, so a future dialect change is made once.
- **Correctness across compilers:** both bindings are accepted; consumers need not pin `LangVersion`.
- **Trade-off — narrow unwrap vs general span transparency (§4.7):** the narrow form must be revisited if a second span-conversion site appears. Probability low (nothing else in the visitor is span-typed); cost then is moving the same check up one level. Chosen: narrow.
- **Trade-off — not supporting instance `Contains` (`List<T>`):** a consumer writing `list.Contains(row.Field)` still hits the reflective fallthrough and an unhelpful exception. No consumer has asked; #177's `.In` guidance covers it; adding it means a third recognition rule with a receiver instead of an argument. Deferred to §15.

## 12. Implementation guidance (ordered milestones for John)

1. **Visitor arm.** In `CriteriaVisitor.VisitMethodCall`, replace the `DeclaringType == typeof(Enumerable)` gate with recognition of a static two-argument `Contains` declared on `Enumerable` *or* `MemoryExtensions`. Inside the arm: take the collection operand (argument 0); if it is a call node to `op_Implicit` declared on a constructed `ReadOnlySpan<>` with a single argument (*r2:* `Span<>` is not recognised — W-5), use that argument instead; call `CreateInFragment(argument 1, collection, preparator, Visit)`. Keep the `default` throw for other arities, with the method in its message. Delete the commented-out block. Nothing else in the file changes — in particular `GetHost`, `VisitNewArray` and the `Function` arm are untouched.
2. **Run the four red tests.** They go green with step 1 alone. If any does not, the shape differs from §3's measured assumptions — report the actual tree, do not widen the gate speculatively.
3. **Rendered tests.** Add the twelve new tests to `CriteriaVisitorTests` with the literals in §10, using the fixture's existing SQLite-client and Postgres-mock patterns. *r2 delta on the r1 tree:* delete `TestContainsBoundToEnumerableRendersInOnSqlite`, `TestContainsBoundToEnumerableRendersAnyOnPostgres` and the `BuildEnumerableContainsPredicate` helper; add the two `TestContainsWithEnumArray…` tests (source-written `TestEnum[]` closure over `EnumEntity`), `TestContainsDoesNotStripNonSpanWrapperCall` (with a private static one-argument helper in the fixture), `TestContainsWithComparerOverloadThrows`, and `TestInWithArrayRendersInOnSqlite`; convert the new tests' asserts to `Assert.That`. Re-run QA mutants M4, M6 and M7 and report each red.
4. **Full suite.** `dotnet test Ocelot.sln`; the three other clusters of #14135 are separate PRs, so their tests may still be red — report the count against the #14135 baseline (7 red) and expect exactly 3 remaining if this branch lands first.
5. **Stale-claim sweep (#114 addendum 2026-08-27).** In the repo: `Readme.md` and any doc that tells consumers to avoid `Contains`. In the graph: #178 (root cause and provenance sections are falsified by §1 — the orchestrator patches the node, not John), #177 (the "now refuses both shapes" paragraph becomes historical; the `.In` idiom advice stands on its own merits), #14135 row 1 (mechanism column). Report which you touched and which you routed.
6. **Version.** `Ocelot.csproj` `AssemblyVersion` / `PackageVersion` bump per repo convention (manual, `GeneratePackageOnBuild`).

No `docs/architecture` follow-up is needed; this document is the deliverable and ships in the same PR.

## 13. Risks & mitigations

| Risk | Mitigation |
|---|---|
| A third compiler shape (e.g. a future `Span<T>` binding for a non-array collection, or a comparer overload) appears at a consumer | The arm refuses by arity; anything else falls to the reflective fallthrough as today. Add the shape when a consumer reports it, with its tree measured. |
| Postgres `= ANY( @1 , @2 )` for inline literals is invalid SQL (pre-existing, `.In` and `Contains` alike) | Out of scope; bug #14160 filed. The SQLite rendered test for inline literals is the only one in this table; a Postgres inline-literal row was deliberately not added so as not to pin a suspect string. |
| A test attribute or doc sentence over-generalises the binding again (r1 said "always binds to `MemoryExtensions`"; false for enums — QA CF-1) | *r2:* no `[Description]` at all; the boundary is stated once, in §1 and §3, in terms of the `IEquatable<T>` constraint, and every row names the element type it uses. |

## 14. Pre-Design Checklist (#1136 §5), answered

**KISS / DRY / YAGNI**
- No new type; no new abstraction; no helper (the unwrap is a two-condition check at one site — `block_size × site_count` = 3 × 1, below threshold).
- No "might need later" element: the general span-transparency rule (§4.7) and the diagnostic wrapper (§4.6) were both rejected on this ground.
- No flag, shim, or transition window.

**Existing systems first**
- `CreateInFragment` and the `.In` rendering path already cover every collection shape; the design reuses them wholesale and adds only recognition (§4.1).
- No new persisted data.

**Configurability** — none introduced.

**Less is better**
- Can-it-be-deleted: the commented-out block — yes, deleted. The diagnostic wrapper — never added.
- Trade-offs named in §11.

**Document discipline**
- Cites #1136 and #114 as load-bearing (header).
- Scope / non-scope explicit (§2). Reader inventory: the one arm and the tests that exercise it.
- No predecessor design to supersede.

## 15. Decisions taken autonomously (#8727)

### D1 — Design against the measured cause, not #178's stated cause
- **Chosen:** treat the C# 14 `MemoryExtensions` binding as the root cause; leave `CreateInFragment` and the dialects untouched.
- **Why:** the `Enumerable` arm was measured rendering both shapes correctly on unmodified `master`; the four tests throw from the fallthrough, never from `CreateInFragment`. Designing Option A or B would add code that changes nothing observable.
- **Alternatives:** #178 Option A — re-adds shape dispatch the visitor already performs downstream; Option B — three branches × two dialects for the same nothing.
- **Reversal cost:** cheap — the arm is one site; if a consumer shows a tree neither binding covers, the gate widens there.

### D2 — Unwrap the span conversion inside the `Contains` arm only
- **Chosen:** narrow (§4.7).
- **Alternatives:** visitor-wide `op_Implicit` transparency — broader than any known need.
- **Reversal cost:** cheap — move the same check one level up.

### D3 — No diagnostic catch-and-rethrow
- **Chosen:** drop #178's wrapper (§4.6).
- **Alternatives:** keep it for unrecognised shapes — it cannot see them; they never enter the arm.
- **Reversal cost:** cheap.

### D4 — Instance `Contains` (`List<T>`, `HashSet<T>`) stays out of scope
- **Chosen:** not designed; `.In(…)` remains the idiom (#177).
- **Alternatives:** recognise any instance `Contains` whose receiver evaluates to an in-memory collection — a third rule with different operand positions, no reporting consumer.
- **Reversal cost:** medium — a separate small design if a consumer asks.

### D5 (*r2*, QA CF-1) — replace the hand-built `Enumerable` pair with source-written `TestEnum[]` predicates
- **Chosen:** QA's option (b): two source-written `enums.Contains(e.Enum)` tests over `EnumEntity`, SQLite and Postgres; the reflection helper and both `[Description]`s are deleted.
- **Why:** the r1 premise ("only a hand-built tree reaches the `Enumerable` arm") was measured false — enums are not `IEquatable<T>`, so a source predicate reaches it. Option (b) makes the sentence unnecessary rather than merely true, pins the real closure operand shape (W-4), and reds M4 exactly as the pair did.
- **Alternatives:** option (a), keep the pair and rewrite the two sentences — leaves a reflection helper and a `ConstantExpression` operand no compiler emits, plus two attributes that must stay true across SDK upgrades.
- **Reversal cost:** cheap.

### D6 (*r2*, QA W-5) — the unwrap recognises `ReadOnlySpan<>` only
- **Chosen:** drop `Span<>` from the recognised declaring types (§3, §5, §12 step 1).
- **Why:** no shape produces it — an explicit `MemoryExtensions.Contains<int>(array, x)` still selects the `ReadOnlySpan<T>` overload (QA measured), and no other span-typed operand can appear in an expression tree. #1136 §6: defensive code for an impossible scenario.
- **Alternatives:** keep it with a justification — there is none to give; it would be a claim that a shape exists which nobody can produce.
- **Reversal cost:** cheap — one type in a condition, re-added with the producing shape in hand if one ever appears.

### D7 (*r2*, QA W-2) — the unwrap-precision guard is SQLite-only
- **Chosen:** one row, `TestContainsDoesNotStripNonSpanWrapperCall`, on SQLite.
- **Why:** the property is visible only where element count is visible in the text; on Postgres both the correct and the mutant rendering are one array parameter (`"enum" = ANY( @1 )` measured either way), so a Postgres twin would be a row that cannot discriminate — a wish, per #1220 §9.
- **Alternatives:** a Postgres twin asserting the bound array's contents through a capturing mock — heavier, and the SQLite row already reds M6.
- **Reversal cost:** cheap.

## 16. Open questions for the operator

1. **Conflict with the fixed facts (surfaced, not resolved silently).** #178's root cause ("`CreateInFragment` is naive; the commented-out block is the missing logic"), its provenance narrative (commit `16f1c66` as the regression point) and its Option A/B framing are contradicted by measurement (§1). The fixed facts I was given assumed them. This design proceeds on the measured cause per #8727 — the four tests are the acceptance test either way — but #178's body should be patched by whoever owns the node so the next reader does not design against the wrong cause (that is the graph half of the stale-claim sweep, §12 step 5).
2. **Should the umbrella note that mamgo-backend's May breakage was compiler-side?** #177 attributes the fourteen sites to an Ocelot regression between 0.17.70 and 0.20.0. If mamgo-backend moved to a C# 14 compiler in the same window, the attribution is wrong and the `.In` rewrites were a workaround for a toolchain change, not for Ocelot. I did not verify mamgo-backend's SDK history; recommend a one-line check before #177 is edited. *r2:* when #177 is edited, add the boundary: under C# 14 the breakage is confined to `IEquatable<T>` element types (`int`, `long`, `string`, `Guid`, …); enum arrays and `IEnumerable<T>`-typed collections never broke (QA #14167 §1).
3. **`.In(new[] {…})` on Postgres rendering `= ANY( @1 , @2 )`** — bug #14160 filed from this session's measurement; needs a live-Postgres confirmation (`PostgresLocalTests` line 138 is the gated executor). Not part of this PR.
