# SafeVault

SafeVault is an ASP.NET Core Razor Pages application targeting .NET 10. It uses
SQLite for storage and Entity Framework Core (EF Core) to access the database.

## Prerequisites

- .NET 10 SDK
- SQLite command-line tool (`sqlite3`)
- EF Core command-line tool (`dotnet-ef`), needed only when regenerating the models

To install the EF Core tool if it is not already installed:

```sh
dotnet tool install --global dotnet-ef --version 10.0.11
```

## Set Up From a Fresh Clone

Run these commands from the repository root. The first command creates the
SQLite database file and `Users` table from the checked-in SQL schema. Run it
once; skip it if that database file already exists.

```sh
sqlite3 SafeVault/SafeVault.db < Database/Users.sql
cd SafeVault
dotnet restore
dotnet build
dotnet run
```

The app uses `Data Source=SafeVault.db`, so run it from the `SafeVault` project
directory. `dotnet run` prints the local URL to open in a browser.

## EF Core Models

The C# model and context were generated from the SQLite schema. They are source
files required to build the app, not build outputs. Keep
`SafeVault/Data/SafeVaultContext.cs` and
`SafeVault/Models/Scaffolded/User.cs` in source control; `dotnet build` does not
regenerate them.

If you change the SQL schema and need to regenerate those files, first create or
update the database, then run the following from the `SafeVault` project
directory:

```sh
dotnet ef dbcontext scaffold \
	"Data Source=SafeVault.db" \
	Microsoft.EntityFrameworkCore.Sqlite \
	--context SafeVaultContext \
	--context-dir Data \
	--output-dir Models/Scaffolded \
	--no-onconfiguring \
	--force
```

`--force` overwrites the generated context and model files. Put custom
application logic outside these generated files.

## Database Files

`Database/Users.sql` is the version-controlled schema. The local
`SafeVault/SafeVault.db` file is created from it and is ignored by Git; do not
commit the database file.
