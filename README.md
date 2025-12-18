# Hackathon Results Management System

A console-based application developed in **C# (.NET 9)** as part of the **Advanced Software Development** course at **Óbuda University**.  
The project demonstrates object-oriented design, Entity Framework Core, LINQ-based data analysis, and event-driven programming.

---

## Project Overview

This application manages hackathon project results by importing data from an XML file, storing it in a local database, executing analytical queries, and exporting results to JSON format.  
It is implemented as a multi-project Visual Studio solution following a layered architecture.

---

## Key Features

- XML to database data import with validation  
- Entity Framework Core (Code-First) with SQLite database  
- 15 LINQ queries (simple, medium, and complex)  
- Event and delegate mechanism to notify successful data import  
- Menu-driven console interface  
- JSON export of query results  
- Clean separation between presentation and data layers  

---

## Solution Structure

HackathonApp.sln
├── HackathonApp.Console // Console UI and application logic
└── HackathonApp.Data // Data access, entities, and DbContext

yaml
Copy code

---

## Configuration

The application uses a local SQLite database. Configuration is stored in `appsettings.json` inside the `HackathonApp.Console` project.

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
No external credentials are required.

How to Run
Open HackathonApp.sln in Visual Studio 2022 or later

Restore NuGet packages if prompted

Set HackathonApp.Console as the startup project

Run the application

Use the console menu to import data, run queries, and export results

Technologies Used
C#

.NET 9

Entity Framework Core

SQLite

LINQ

XML and JSON

Git and GitHub

Academic Context
This project was developed as part of an academic assignment to practice modern .NET development, clean architecture, and data-driven application design.

License
This project is intended for educational purposes only.
