# Lumina F# Minimal Web Architecture Template

This document captures the exact dependency versions, project layout, and implementation patterns used in the current Lumina base so you can replicate the architecture quickly in future F# web projects.

## 1. Technology Stack Versions

Core Runtime / SDK:
- .NET Target Framework: `net9.0`
- .NET SDK (observed via build artifacts): `9.0.301` (adjust as newer SDKs release)

NuGet Packages (resolved versions from `obj/project.assets.json`):
- Falco: `5.1.0`
- Falco.Markup: `1.4.0` (transitive)
- FSharp.Core: `9.0.300`
- Microsoft.Data.SqlClient: `5.2.3`
- Microsoft.Data.SqlClient.SNI.runtime: `5.2.0` (transitive)
- System.Configuration.ConfigurationManager: `8.0.0` (transitive)
- System.Runtime.Caching: `8.0.0` (transitive)
- Azure.Identity: `1.11.4` (transitive via SqlClient) – not actively used; can be trimmed if you don’t need managed identity.
- Azure.Core: `1.38.0` (transitive)
- Supporting identity/security packages (transitive): `Microsoft.Identity.Client 4.61.3`, `Microsoft.IdentityModel.* 6.35.0`

Recommended pinning strategy for reproducibility:
```xml
<PackageReference Include="Falco" Version="5.1.0" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.3" />
<PackageReference Include="FSharp.Core" Version="9.0.300" />
```
If you want to minimize transitive footprint, evaluate removing Azure.Identity by restricting SqlClient features (only basic connection strings). Keep the pinned versions to avoid unexpected API shape changes.

## 2. Directory Layout (Layered Architecture)

```
Domain/                 # Pure domain types only (records, no logic)
Persistence/            # Database setup & CRUD (ADO.NET / SqlClient)
Features/               # HTTP handlers (per bounded context: Appointments, Blog, Photos)
Web/                    # View/layout helpers (Falco markup builders)
wwwroot/                # Static assets (styles.css, uploads/, images)
Program.fs              # Composition root (DI-lite, route mapping, middleware)
ARCHITECTURE_TEMPLATE.md# This blueprint document
README.md               # Project overview & high-level docs
appsettings.json        # Configuration (connection string etc.)
```

## 3. Module Responsibilities

Domain:
- Defines `Appointment`, `BlogPost`, `Photo` records with `[<CLIMutable>]` to allow SqlClient object mapping if needed.
- No functions; ensure business rules stay outside persistence.

Persistence (`Database.fs` inside `Persistence/`):
- `setConnectionString` stores connection at startup.
- `ensureDatabaseAndTables` idempotently creates DB & tables.
- Sub-modules: `Appointments`, `Blog`, `Photos` each expose `list`, `add`, and `tryGet*` functions returning domain records.
- Uses parameterized SQL via `SqlCommand` to prevent injection.

Features:
- Each handler module maps to routes (e.g., `BlogHandlers.list`, `BlogHandlers.detail`).
- Performs minimal validation & transforms HTTP form data to domain objects.
- Returns Falco responses (`Response.ofHtml`, `Response.redirect`, etc.).

Web:
- Provides structural layout helpers (`shell`, `nav`, page-specific builders) centralizing markup styling.
- Avoids business or persistence logic; purely presentation.

Program.fs:
- Configures connection string from configuration.
- Calls `ensureDatabaseAndTables`.
- Sets up middleware (static files, developer exception page if desired).
- Registers routes with `MapGet`, `MapPost` delegating to Feature modules.

## 4. Initialization & Boot Sequence

1. Read configuration (`appsettings.json`) for `ConnectionStrings:Default`.
2. Call `Persistence.Database.setConnectionString`.
3. Run `ensureDatabaseAndTables` (should be idempotent; safe on every start).
4. Register services/middleware:
   - Static files (`UseStaticFiles`).
   - Optional developer support (`UseDeveloperExceptionPage` in dev env).
5. Map routes to handlers.
6. Start the host (`app.Run()`).

## 5. Database Schema (SQL Server)

Tables (simplified):
- `Appointments(Id INT IDENTITY PK, Name NVARCHAR(200), SessionDate DATETIME2)`
- `BlogPosts(Id INT IDENTITY PK, Slug NVARCHAR(160) UNIQUE, Title NVARCHAR(200), Excerpt NVARCHAR(500), Content NVARCHAR(MAX), Published DATETIME2)`
- `Photos(Id INT IDENTITY PK, FileName NVARCHAR(260), Title NVARCHAR(200), Uploaded DATETIME2, Width INT, Height INT)`

Creation logic uses `IF OBJECT_ID(...) IS NULL` blocks for each table.

## 6. Core Reusable Patterns

SQL Access Pattern:
```fsharp
use conn = new SqlConnection(cs)
conn.Open()
use cmd = conn.CreateCommand()
cmd.CommandText <- "SELECT ... WHERE Id = @id"
cmd.Parameters.Add(SqlParameter("@id", SqlDbType.Int, Value = id)) |> ignore
use reader = cmd.ExecuteReader()
while reader.Read() do ...
```

Falco Markup Builder Pattern:
```fsharp
open Falco.Markup
Elem.div [ Attr.class' "container" ] [ Elem.h1 [] [ Text.raw "Heading" ] ]
```

Handlers Pattern:
```fsharp
let handler (ctx: HttpContext) =
    // parse ctx.Request, call persistence, build markup
    Response.ofHtml (Web.Views.shell "Title" [ ... ]) ctx
```

Routing Pattern (Program.fs excerpt):
```fsharp
app.MapGet("/blog", Func<HttpContext, Task>(BlogHandlers.list)) |> ignore
app.MapPost("/admin/blog", Func<HttpContext, Task>(BlogHandlers.adminCreatePost)) |> ignore
```

## 7. Replication Checklist

1. Create new folder & project:
   ```powershell
   dotnet new web -lang "F#" -n YourApp
   cd YourApp
   ```
2. Add packages:
   ```powershell
   dotnet add package Falco --version 5.1.0
   dotnet add package Microsoft.Data.SqlClient --version 5.2.3
   dotnet add package FSharp.Core --version 9.0.300
   ```
3. Create layer folders: `Domain/`, `Persistence/`, `Features/`, `Web/`, `wwwroot/`.
4. Add `styles.css` & optionally a placeholder `uploads/` (ensure writable).
5. Implement domain record types.
6. Implement `Persistence.Database` with connection setter & ensure function.
7. Add feature handler modules (start with one, e.g., `AppointmentHandlers.fs`).
8. Add `Web/Views.fs` with layout helpers.
9. Wire routes in `Program.fs` (static files + handlers).
10. Add `.gitignore` (bin/, obj/, wwwroot/uploads/).
11. Run & migrate: `dotnet run` (first run creates tables automatically).

## 8. Recommended Enhancements (Optional)

- Validation: centralize required field checks & date parsing helper.
- Slug Generation: ensure lowercase, replace spaces, enforce uniqueness by catching PK or unique index violation.
- Authentication: add minimal cookie auth or JWT for `/admin` endpoints.
- Image Metadata: read width/height using an image library before DB insert.
- Error Boundary: middleware for uniform error HTML pages.
- Observability: add minimal logging around DB operations.

## 9. Portability Notes

- Keep domain pure; persistence changes (switch to Dapper/EF) won’t affect features heavily.
- Avoid large transitive dependency trees; scrutinize packages pulled in by SqlClient. If Azure AD auth to SQL isn’t needed, explicitly configure using only username/password to avoid extra identity libs at runtime.
- Net9.0 target ensures latest F# features; if targeting LTS, adjust to `net8.0` and downgrade package versions to their compatible ranges.

## 10. Minimal Template File Inventory

```
AppointmentApp.fsproj
Program.fs
Domain/Domain.fs
Persistence/Database.fs
Features/AppointmentHandlers.fs (example)
Web/Views.fs
wwwroot/styles.css
appsettings.json
.gitignore
ARCHITECTURE_TEMPLATE.md
```

## 11. Clean Git Hygiene

`.gitignore` essentials:
```
bin/
obj/
wwwroot/uploads/
.vscode/
.vs/
*.user
```

Commit Strategy:
- chore: dependency bumps
- feat: new feature modules
- refactor: internal handler/view changes without behavior changes
- docs: architecture updates
- fix: bug fixes (runtime errors, parsing issues)

## 12. Reuse Strategy

To replicate quickly:
1. Copy this file into new project.
2. Run the replication checklist.
3. Use it as a lived document—update sections 1 & 8 as dependencies or enhancements evolve.

---
Generated on: 2025-11-10
Source project: Lumina base (AppointmentApp)
```
Falco 5.1.0 | SqlClient 5.2.3 | FSharp.Core 9.0.300 | net9.0
```

Happy building! ✨