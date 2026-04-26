# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Pooshit.Ocelot is a fluent, type-safe ORM for .NET, packaged as the `Pooshit.Ocelot` NuGet package. It targets `netstandard2.1` and supports **SQLite**, **PostgreSQL**, **MySQL/MariaDB**, and **MSSQL** through pluggable dialect implementations. Users interact with entities via `EntityManager` and a fluent API oriented at SQL syntax (Insert/Update/Delete/Load/Create). See [Readme.md](Readme.md) for the public API tour.

## Build and test

```bash
dotnet build Ocelot.sln
dotnet test Ocelot.sln
# single test
dotnet test Ocelot.sln --filter "FullyQualifiedName~BasicOperationTests.TruncateTable"
# single fixture
dotnet test Ocelot.sln --filter "FullyQualifiedName~Pooshit.Ocelot.Tests.FunctionTests"
```

The test project ([Ocelot.Tests/Ocelot.Tests.csproj](Ocelot.Tests/Ocelot.Tests.csproj)) is `net8.0` with **NUnit 3**. Tests use `[TestFixture, Parallelizable]` and run in parallel — assume any new test will run concurrently with siblings. Default test database is **SQLite in-memory**, created via [TestData.CreateDatabaseAccess()](Ocelot.Tests/Data/TestData.cs).

Postgres-backed tests in [Ocelot.Tests/Postgres/PostgresLocalTests.cs](Ocelot.Tests/Postgres/PostgresLocalTests.cs) are gated on the `POSTGRES_CONNECTION` env var and `Assert.Inconclusive` when it's missing — set that variable to run them locally.

The library version is bumped manually in [Ocelot/Ocelot.csproj](Ocelot/Ocelot.csproj) (`AssemblyVersion`/`PackageVersion`); `GeneratePackageOnBuild` is on.

## Architecture

The codebase has three layered concerns: **dialect**, **operation pipeline**, and **entity model**.

### Dialect layer ([Ocelot/Info/](Ocelot/Info/))

`IDBInfo` is the per-database-engine abstraction — parameter prefix, `LIMIT`/`OFFSET` syntax, function translations (`Replace`, `ToUpper`, `Length`, `Now`, etc.), schema introspection (`GetSchema`, `CheckIfTableExists`), and `Truncate`/`DropTable`/`DropView`. Concrete implementations: `SQLiteInfo`, `PostgreInfo`, `MySQLInfo`, `MsSqlInfo`. `DBInfo` (abstract base) registers a `fieldlogic` dispatch table mapping `IDBField` token types to writer delegates — so adding a new SQL token type generally means adding an `AppendXxx` handler in `DBInfo` and (where dialects diverge) overriding it in the engine-specific subclass.

Multiple-connection support is signalled by `IDBInfo.MultipleConnectionsSupported`. SQLite returns false and `ClientFactory.Create` wraps its client in `LockedDBClient` to serialize access. Treat that as a hard constraint: any SQLite path must assume a single shared connection.

### Operation pipeline ([Ocelot/Entities/Operations/](Ocelot/Entities/Operations/))

User-facing fluent operations (`LoadOperation`, `InsertEntitiesOperation`, `UpdateEntitiesOperation`, `DeleteOperation`, `CreateTableOperation`, `UpdateValuesOperation`, `InsertValuesOperation`, …) build SQL by writing **tokens** into an `IOperationPreparator` (see [Ocelot/Entities/Operations/Prepared/](Ocelot/Entities/Operations/Prepared/)). The preparator accumulates `IOperationToken`s (text fragments, parameters, array parameters) and produces a `PreparedOperation` (or a specialized variant: `PreparedLoadOperation`, `PreparedReturnIdOperation`, `PreparedBulkInsertOperation`, `PreparedArrayLoadOperation`). The dialect (`IDBInfo`) is what actually walks LINQ expression trees and emits engine-specific text into the preparator — operations are dialect-agnostic; dialects own the SQL.

SQL tokens beyond LINQ expressions live under [Ocelot/Tokens/](Ocelot/Tokens/) and let callers express things LINQ can't model cleanly: control flow (`Tokens/Control/` — CASE/WHEN/IF), values (`Tokens/Values/` — column refs, casts, aliases, `NowToken`, tuples), aggregates and window functions (`Tokens/Partitions/RowNumberOver`), and translated expression trees (`Tokens/Expressions/Xpr` + `XprTranslator`). Use the static `DB` helper ([Ocelot/Tokens/DB.cs](Ocelot/Tokens/DB.cs)) as the entry point — e.g. `DB.Count()`, `DB.Property<T>(...)`.

### Entity model ([Ocelot/Entities/](Ocelot/Entities/))

`EntityDescriptor` (in [Ocelot/Entities/Descriptors/](Ocelot/Entities/Descriptors/)) is the cached metadata for a CLR type — table name, columns (`EntityColumnDescriptor`), indexes, uniques, primary key. Discovery is attribute-driven from [Ocelot/Entities/Attributes/](Ocelot/Entities/Attributes/) (`Table`, `Column`, `PrimaryKey`, `AutoIncrement`, `Index`, `Unique`, `NotNull`, `DefaultValue`, `Size`, `Ignore`, `View`). `EntityDescriptorCache` memoizes per-type — both `EntityManager` and ad-hoc operations share that cache. `EntityDescriptorAccess<T>` exposes the descriptor for a single type via `EntityManager.Model<T>()`.

`SchemaCreator` builds tables; `SchemaUpdater` migrates them by diffing `EntityDescriptor` against the live schema fetched through `IDBInfo.GetSchema`. Schema-update support varies by engine — SQLite and Postgres are exercised in tests (see [Ocelot.Tests/SchemaUpdateTests.cs](Ocelot.Tests/SchemaUpdateTests.cs) and [Ocelot.Tests/Postgres/PostgresSchemaUpdateTests.cs](Ocelot.Tests/Postgres/PostgresSchemaUpdateTests.cs)); the public docstring on `EntityManager.UpdateSchema<T>` still says "currently this is only implemented for sqlite databases" but Postgres has working coverage.

Views are entities decorated with `[View("Namespace.Resource.sql")]` pointing at an embedded SQL resource. Both [Ocelot/Ocelot.csproj](Ocelot/Ocelot.csproj) and [Ocelot.Tests/Ocelot.Tests.csproj](Ocelot.Tests/Ocelot.Tests.csproj) wire individual `EmbeddedResource` entries — when adding a new `.sql` or test-fixture resource, add it explicitly there; do not rely on globs.

### Client layer ([Ocelot/Clients/](Ocelot/Clients/))

`IDBClient` wraps an ADO.NET `DbConnection` with a `Transaction` API and sync/async query/scalar/set/reader/non-query methods, each in both prepared and ad-hoc variants. Every dialect call ultimately exits through here. `ClientFactory.Create(connection, info)` is the canonical construction path; passing a SQLite connection auto-wraps with `LockedDBClient`.

## Conventions worth knowing

- **Namespaces follow folders**, root namespace `Pooshit.Ocelot`. New files should use file-scoped namespaces consistent with their directory (existing code mixes file-scoped and block-scoped — match the surrounding file).
- **Exceptions**: SQL execution failures are wrapped in `StatementException` with the original command text and parameters captured — preserve that behavior when adding new execution paths.
- **Async**: most operations expose both sync and `*Async` variants. When `IDBInfo.MultipleConnectionsSupported` is false, async set enumeration buffers via `AsyncEnumerableExtensions.Buffer()` to avoid holding the connection across yields — see the pattern in [Ocelot/Clients/DBClient.cs](Ocelot/Clients/DBClient.cs).
- **Adding a new SQL token**: register an `AppendXxx` handler via `DBInfo.AddFieldLogic<T>` in the base class constructor; override in dialect subclasses where SQL differs.
- **Adding dialect-specific behavior**: add the abstract or virtual method to `IDBInfo`/`DBInfo` and implement in all four engine subclasses — don't leave a `NotImplementedException` for engines you didn't test.
