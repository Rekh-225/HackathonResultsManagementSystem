# Hackathon Results Management System

A console-based data management application developed in **C# (.NET 9)** as part of the **Advanced Software Development** course at **Óbuda University**. The project demonstrates layered architecture, Entity Framework Core (Code-First), LINQ-based analytical queries, event-driven programming, and data interchange via XML and JSON.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Key Features](#key-features)
3. [Architecture & Solution Structure](#architecture--solution-structure)
4. [Data Model](#data-model)
5. [Prerequisites](#prerequisites)
6. [Getting Started](#getting-started)
7. [Configuration](#configuration)
8. [XML Input Format](#xml-input-format)
9. [Application Menu & Queries](#application-menu--queries)
10. [JSON Output](#json-output)
11. [Technologies Used](#technologies-used)
12. [Known Limitations](#known-limitations)
13. [Handover Document](#handover-document)

---

## Project Overview

The **Hackathon Results Management System** is a multi-project .NET 9 console application that manages hackathon competition records end-to-end:

- Reads project submission data from an **XML file**
- Validates and persists records to a **local SQLite database** using Entity Framework Core
- Provides **15 LINQ analytical queries** (grouped by complexity)
- Exports query results to **JSON files**
- Uses a **custom delegate/event** (`DataImported`) to surface import statistics after each run

The solution is structured with a clean separation between the presentation layer (`HackathonApp.Console`) and the data/business layer (`HackathonApp.Data`).

---

## Key Features

| Feature | Detail |
|---|---|
| XML Import with Validation | Parses `HackathonResults.xml`; validates all fields before insert/update |
| Upsert Logic | Re-running import updates existing records and skips/counts invalid ones |
| EF Core Code-First | Database schema managed via migrations; no manual SQL required |
| SQLite Database | Zero-config local database stored in `Data/Hackathon.db` |
| 15 LINQ Queries | 5 simple · 5 medium · 5 complex; mix of query and method syntax |
| Event/Delegate Pattern | `DataImportedHandler` delegate fires after each import with insert/update/skip stats |
| JSON Export | Every query auto-exports its results to the `Output/` folder |
| Menu-Driven UI | Numbered console menu; no command-line arguments required |

---

## Architecture & Solution Structure

```
HackathonApp.sln
│
├── HackathonApp.Console/          # Presentation / entry-point project
│   ├── Program.cs                 # Main loop, menu handling, query runners
│   ├── JsonExporter.cs            # Static helper – serialises any object to JSON
│   ├── appsettings.json           # Connection string and file paths
│   └── Data/
│       └── HackathonResults.xml   # Source data file (copied to build output)
│
└── HackathonApp.Data/             # Data / business-logic class library
    ├── Entities/
    │   └── Project.cs             # Domain entity
    ├── DTOs/
    │   ├── CategoryCountResult.cs
    │   └── CategoryAverageScoreResult.cs
    ├── Data/
    │   ├── HackathonContext.cs        # DbContext with Fluent API configuration
    │   └── HackathonContextFactory.cs # Design-time factory for EF tooling
    ├── Services/
    │   └── HackathonService.cs    # All business logic: import + 15 queries
    └── Migrations/
        └── 20251122115515_InitialCreate.*  # EF Core initial migration
```

### Layer Responsibilities

| Layer | Project | Responsibility |
|---|---|---|
| Presentation | `HackathonApp.Console` | Console UI, menu loop, JSON export, configuration loading |
| Data / Business | `HackathonApp.Data` | Entity definition, DbContext, EF migrations, all LINQ queries, XML import |

---

## Data Model

### `Project` Entity

| Property | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `int` | Primary Key | Unique identifier for each project submission |
| `TeamName` | `string` | Required, max 100 chars | Name of the competing team |
| `ProjectName` | `string` | Required, max 120 chars | Name of the submitted project |
| `Category` | `string` | Required, max 50 chars | Competition category (e.g., `AI-ML`, `HealthTech`, `SmartCity`, `Energy`) |
| `EventDate` | `DateTime` | Required, stored as `date` | Date the project was submitted/evaluated |
| `Score` | `decimal` | Required, `decimal(5,2)`, 0–100 | Judge-assigned score |
| `Members` | `int` | Required, 1–15 | Number of team members |
| `Captain` | `string` | Required, max 100 chars | Team captain's name |

### Database

- **Engine:** SQLite (file `Data/Hackathon.db`, relative to the executable)
- **Table:** `Projects`
- **Schema management:** EF Core Code-First migrations

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 9.0 or later |
| Visual Studio | 2022 17.8+ (or VS Code with C# Dev Kit) |
| Operating System | Windows, macOS, or Linux |

No additional database server, credentials, or environment variables are required.

---

## Getting Started

### Option A – Visual Studio

1. Open `HackathonApp.sln` in Visual Studio 2022 or later.
2. Right-click the solution → **Restore NuGet Packages** (Visual Studio usually does this automatically).
3. Set **HackathonApp.Console** as the startup project.
4. Press **F5** (debug) or **Ctrl+F5** (run without debugging).

### Option B – .NET CLI

```bash
# From the repository root
dotnet restore
dotnet run --project HackathonApp.Console/HackathonApp.Cli.csproj
```

### First Run

On the first launch the application:
1. Creates the SQLite database file automatically (via `EnsureCreatedAsync`).
2. Presents the main menu — choose **option 1** to import the bundled XML data before running queries.

---

## Configuration

All runtime paths are configured in `HackathonApp.Console/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "HackathonDb": "Data Source=Data/Hackathon.db"
  },
  "Paths": {
    "XmlInput": "Data/HackathonResults.xml",
    "JsonOutput": "Output"
  }
}
```

| Key | Default Value | Purpose |
|---|---|---|
| `ConnectionStrings:HackathonDb` | `Data Source=Data/Hackathon.db` | SQLite database file path (relative to executable) |
| `Paths:XmlInput` | `Data/HackathonResults.xml` | Source XML file to import |
| `Paths:JsonOutput` | `Output` | Directory where JSON result files are written |

Both paths are relative to the executable's working directory. No secrets or environment-specific variables are required.

---

## XML Input Format

The input file (`Data/HackathonResults.xml`) must follow this structure:

```xml
<HackathonResults>
  <Project>
    <Id>1</Id>
    <TeamName>NeuralNova</TeamName>
    <ProjectName>AI Vision Pro</ProjectName>
    <Category>AI-ML</Category>
    <Captain>Alice Smith</Captain>
    <EventDate>2025-10-12</EventDate>
    <Score>95.50</Score>
    <Members>5</Members>
  </Project>
  <!-- More <Project> elements ... -->
</HackathonResults>
```

### Validation Rules Applied at Import

| Field | Rule |
|---|---|
| `Id` | Must be a positive integer |
| `TeamName`, `ProjectName`, `Category`, `Captain` | Must not be blank |
| `EventDate` | Must be a valid date and **not in the future** |
| `Score` | Must be a decimal between **0** and **100** (inclusive) |
| `Members` | Must be an integer between **1** and **15** (inclusive) |

Records that fail validation are **skipped**. Existing records with the same `Id` are **updated**. New records are **inserted**. A summary (inserted / updated / skipped / duration) is printed to the console via the `DataImported` event.

---

## Application Menu & Queries

```
===== Hackathon Results Management System =====
1) Import XML -> Database
2) Run Simple LINQ queries
3) Run Medium LINQ queries
4) Run Complex LINQ queries
5) Export last query results to JSON (manual)
0) Exit
```

### Simple Queries (1–5)

| # | Query | Syntax |
|---|---|---|
| Q1 | All projects submitted by team **NeuralNova** | Query |
| Q2 | All projects submitted on **2025-10-12** | Query |
| Q3 | All projects in the **AI-ML** category | Query |
| Q4 | Projects with **Score > 90**, ordered by score descending | Method |
| Q5 | **Top 5** highest-scoring projects overall | Method |

### Medium Queries (6–10)

| # | Query | Syntax |
|---|---|---|
| Q6 | All projects submitted during **calendar year 2024** | Query |
| Q7 | **HealthTech** projects with Score > 88, ordered by score | Method |
| Q8 | All projects sorted by **EventDate asc**, then **Score desc** | Method |
| Q9 | **Count** of projects per category | Query (GroupBy) |
| Q10 | **Top 3** projects from team **ByteForge** by score | Method |

### Complex Queries (11–15)

| # | Query | Syntax |
|---|---|---|
| Q11 | **Average score per category**, ordered by category name | Query (GroupBy + Average) |
| Q12 | **SmartCity or Energy** projects scoring ≥ their category average | Method (cross-query) |
| Q13 | Projects with **"AI" in the name** and Score > 92 | Method (in-memory filter) |
| Q14 | **Top 5 projects per category** | Method (GroupBy + SelectMany) |
| Q15 | Teams with **≥ 5 members** scoring above the **global average** | Query (AverageAsync) |

Each query group automatically exports all its results to individual JSON files in the `Output/` directory upon execution.

---

## JSON Output

Query results are serialised with indented formatting via `System.Text.Json` and written to the configured output directory. File names correspond to each query:

```
Output/
├── q01_team_neuralnova.json
├── q02_projects_on_date.json
├── q03_aiml_projects.json
├── q04_score_above_90.json
├── q05_top5_projects.json
├── q06_projects_2024.json
├── q07_healthtech_above88.json
├── q08_sorted_by_date_score.json
├── q09_category_counts.json
├── q10_top3_byteforge.json
├── q11_average_scores.json
├── q12_smartcity_energy_above_avg.json
├── q13_ai_name_above92.json
├── q14_top5_per_category.json
└── q15_big_teams_above_avg.json
```

The `Output/` directory is created automatically if it does not exist.

---

## Technologies Used

| Technology | Purpose |
|---|---|
| C# 13 / .NET 9 | Application language and runtime |
| Entity Framework Core 9 | ORM, Code-First schema management, migrations |
| Microsoft.EntityFrameworkCore.Sqlite | SQLite EF Core provider |
| Microsoft.Extensions.Configuration | `appsettings.json`-based configuration |
| System.Xml.Linq (`XDocument`) | XML parsing for data import |
| System.Text.Json | JSON serialisation for export |
| LINQ | Data querying (query and method syntax) |
| Git & GitHub | Version control and source hosting |

---

## Known Limitations

- **Single-user, local only** – The SQLite database is a local file; there is no multi-user or remote access support.
- **Hardcoded query parameters** – Some query targets (e.g., team name "NeuralNova", date "2025-10-12") are hard-coded in the service methods rather than driven by configuration.
- **No unit tests** – The project does not include an automated test suite.
- **Manual export (menu option 5) is incomplete** – The `lastQueryResults` dictionary in `Program.cs` is declared but never populated, so option 5 always exports an empty set.
- **Future-date guard** – The import rejects records whose `EventDate` is in the future; this check uses `DateTime.Today` (local machine time).

---

---

# Handover Document

> **Project:** Hackathon Results Management System  
> **Version:** 1.0  
> **Platform:** .NET 9 / C# · SQLite  
> **Academic Context:** Advanced Software Development – Óbuda University  

---

## 1. Purpose of This Document

This handover document is intended for any developer, lecturer, or assessor who takes ownership of, continues development on, or evaluates this codebase. It captures everything needed to understand, run, and extend the application without further input from the original author.

---

## 2. Repository & Source Control

| Item | Detail |
|---|---|
| Repository | `Rekh-225/HackathonResultsManagementSystem` (GitHub) |
| Default branch | `main` |
| Solution file | `HackathonApp.sln` (repository root) |
| `.gitignore` | Standard Visual Studio / .NET ignore rules |

The SQLite database file (`Hackathon.db`) and the `Output/` directory are excluded from version control and will be generated at runtime.

---

## 3. Environment Setup

### 3.1 Required Tooling

1. **[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)** – includes the runtime and CLI tools.
2. **Visual Studio 2022** (Community edition is free) with the **.NET desktop development** workload, **or** VS Code with the C# Dev Kit extension.
3. No database server installation is required; SQLite is included as a NuGet package.

### 3.2 NuGet Dependencies

All packages are restored automatically via `dotnet restore`. No manual downloads are needed.

| Package | Version | Project |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 9.0.0 | `HackathonApp.Data` |
| `Microsoft.EntityFrameworkCore.Sqlite` | 9.0.0 | `HackathonApp.Data` |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.0 | `HackathonApp.Data` |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.0 | `HackathonApp.Data` (design-time) |
| `Microsoft.EntityFrameworkCore.Design` | 9.0.0 | `HackathonApp.Console` (design-time) |
| `Microsoft.Extensions.Configuration` | 9.0.0 | `HackathonApp.Console` |
| `Microsoft.Extensions.Configuration.Binder` | 9.0.0 | `HackathonApp.Console` |
| `Microsoft.Extensions.Configuration.Json` | 9.0.0 | `HackathonApp.Console` |

---

## 4. Key Source Files – Roles & Responsibilities

### `HackathonApp.Console`

| File | Role |
|---|---|
| `Program.cs` | Application entry point. Builds configuration, creates `HackathonContext` and `HackathonService`, subscribes to the `DataImported` event, and drives the numbered menu loop. |
| `JsonExporter.cs` | Static async helper. Accepts any `object`, serialises it to indented JSON using `System.Text.Json`, and writes it to the configured output folder. |
| `appsettings.json` | Runtime configuration (database connection string, XML input path, JSON output directory). Copied to the build output directory. |
| `Data/HackathonResults.xml` | Sample/seed data file. Copied to the build output directory so the application can locate it at runtime. |

### `HackathonApp.Data`

| File | Role |
|---|---|
| `Entities/Project.cs` | Plain C# entity class that maps to the `Projects` table. |
| `DTOs/CategoryCountResult.cs` | Lightweight DTO returned by Q9 (project count per category). |
| `DTOs/CategoryAverageScoreResult.cs` | Lightweight DTO returned by Q11 (average score per category). |
| `Data/HackathonContext.cs` | EF Core `DbContext`. Configures the `Projects` table schema via Fluent API (column types, max lengths, primary key). |
| `Data/HackathonContextFactory.cs` | `IDesignTimeDbContextFactory` implementation; allows EF CLI tooling to create a context without launching the application. |
| `Services/HackathonService.cs` | Core business logic. Contains `ImportFromXmlAsync` (parse, validate, upsert) and all 15 LINQ query methods. Raises the `DataImported` event on import completion. |
| `Migrations/` | Auto-generated EF Core migration that creates the initial `Projects` table. |

---

## 5. Data Flow Diagram

```
HackathonResults.xml
        │
        ▼
 HackathonService.ImportFromXmlAsync()
        ├─ Parse & validate each <Project> element
        ├─ Upsert into HackathonContext (EF Core → SQLite)
        └─ Fire DataImported event → console summary printed
        │
        ▼
   Hackathon.db  (SQLite file)
        │
        ▼
 HackathonService.GetXxx() methods  (15 LINQ queries)
        │
        ▼
   JsonExporter.ExportAsync()
        │
        ▼
   Output/qNN_*.json  (one file per query)
```

---

## 6. How to Add a New Query

1. Open `HackathonApp.Data/Services/HackathonService.cs`.
2. Add a new `public async Task<List<T>> GetXxxAsync()` method using query or method LINQ syntax and `ToListAsync()` to materialise results.
3. Register the call in the matching `RunXxxQueriesAsync()` helper in `Program.cs`.
4. Add a corresponding `AutoExport("qNN_filename.json", result, outputDir)` call to persist results to JSON.

---

## 7. How to Extend the Data Model

1. Add the new property to `HackathonApp.Data/Entities/Project.cs`.
2. Configure it in `HackathonContext.OnModelCreating` if non-default mapping is needed.
3. Generate a new EF Core migration:
   ```bash
   dotnet ef migrations add <MigrationName> \
     --project HackathonApp.Data \
     --startup-project HackathonApp.Console
   ```
4. The migration is applied automatically at startup (`EnsureCreatedAsync`). To apply it manually:
   ```bash
   dotnet ef database update \
     --project HackathonApp.Data \
     --startup-project HackathonApp.Console
   ```
5. Update `ImportFromXmlAsync` in `HackathonService.cs` to parse the new field from the XML element.

---

## 8. Switching to a Different Database (e.g., SQL Server)

The `HackathonApp.Data` project already references `Microsoft.EntityFrameworkCore.SqlServer`.

1. In `HackathonContext.OnConfiguring`, replace `UseSqlite(...)` with `UseSqlServer(...)`.
2. In `Program.cs`, replace `.UseSqlite(...)` with `.UseSqlServer(...)`.
3. Update `appsettings.json` with a valid SQL Server connection string.
4. Regenerate migrations if the schema has diverged.

---

## 9. Running EF Core Migrations Manually

```bash
# Add a new migration
dotnet ef migrations add <Name> \
  --project HackathonApp.Data \
  --startup-project HackathonApp.Console

# Apply all pending migrations to the database
dotnet ef database update \
  --project HackathonApp.Data \
  --startup-project HackathonApp.Console

# Remove the last unapplied migration
dotnet ef migrations remove \
  --project HackathonApp.Data \
  --startup-project HackathonApp.Console
```

---

## 10. Recommended Future Improvements

| Gap | Recommendation |
|---|---|
| No automated tests | Add an `xUnit` or `NUnit` test project; use the EF Core in-memory provider to unit-test service methods |
| Hardcoded query parameters | Move magic values (team names, thresholds, dates) to `appsettings.json` |
| No structured logging | Replace `Console.WriteLine` calls with `Microsoft.Extensions.Logging` or Serilog |
| Manual export (option 5) is broken | The `lastQueryResults` dictionary is never populated; wire it up or remove the menu option |
| SqlServer package included but unused | Remove `Microsoft.EntityFrameworkCore.SqlServer` from the `.csproj` if SQL Server support is not planned |
| No dependency injection | Introduce `Microsoft.Extensions.DependencyInjection` for cleaner lifetime management as the application grows |
| Single XML source | Consider supporting multiple input files, a directory watch, or a REST API ingestion endpoint |

---

## 11. Contact & Ownership

| Role | Detail |
|---|---|
| Original Author | **Rekh-225** (GitHub) |
| Course | Advanced Software Development – Óbuda University |
| Repository | https://github.com/Rekh-225/HackathonResultsManagementSystem |

---

## License

This project is intended for **educational purposes only** and is not licensed for commercial use.
