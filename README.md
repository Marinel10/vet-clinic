# Vet Clinic
Veterinary clinic a management web application.

## Product Overview

Vet Clinic is a web application intended to help veterinary clinics organize pet information, appointments, and medical records in one place.

## Target Users

- **Pet owners:** Manage their accounts and pets, and request veterinary appointments.
- **Veterinarians:** Review appointment information and maintain pets' medical records.
- **Clinic administrators:** Coordinate appointments and support day-to-day clinic operations.

## Problem Statement

Managing appointments and pet records through phone calls, paper documents, or separate files can make information harder to find and increase the risk of scheduling errors. Vet Clinic aims to provide a centralized system that makes these processes easier to manage and improves access to relevant information.

The functionality described here is the **planned product scope**; individual features are implemented incrementally and should not be assumed complete.

## Technology Stack

- **Backend:** C#, ASP.NET Core MVC (.NET 8)
- **Frontend:** Razor Views, HTML, CSS
- **Data access:** Entity Framework Core 8
- **Database:** Microsoft SQL Server
- **Development tools:** Visual Studio 2022, SQL Server Management Studio (SSMS), Git, GitHub, Jira

## Project Scope

The planned application covers:

- **User management:** account registration, login, and profile updates
- **Pet management:** adding, editing, and viewing pet profiles
- **Appointments:** booking, rescheduling, cancellation, and schedule viewing
- **Medical records:** diagnoses, treatments, vaccinations, and medical history
- **Notifications:** appointment and vaccination reminders

> **Development status:** Features are delivered in separate Jira tasks and pull requests. The list above describes the planned scope, not a claim that every feature is complete.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 with the ASP.NET and web development workload (optional if using the .NET CLI)
- Microsoft SQL Server and SQL Server Management Studio (SSMS)
- Git

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/Marinel10/vet-clinic.git
cd vet-clinic
```

### 2. Prepare the database

Create the **VetClinic** database using the SQL schema provided in the repository's `database/` directory (introduced in Jira task `SCRUM-30`). Check the schema before running it against an existing database.

### 3. Configure the connection string

The MVC application reads the `ConnectionStrings:VetClinic` setting. For local development, configure the connection string for your SQL Server instance, for example with .NET user secrets:

```bash
cd VetClinicMvc
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:VetClinic" "Server=localhost;Database=VetClinic;Trusted_Connection=True;TrustServerCertificate=True;"
```

Replace `localhost` with your SQL Server instance name if needed. Use your own local credentials where applicable. **Do not commit passwords or other secrets.**

### 4. Restore and run

From the `VetClinicMvc` directory:

```bash
dotnet restore
dotnet run
```

Open the local URL printed in the terminal. You can also open `VetClinicMvc/VetClinicMvc.csproj` in Visual Studio and run the application there.

## Repository Structure

```text
vet-clinic/
├── database/                 # SQL Server database scripts
├── VetClinicMvc/             # ASP.NET Core MVC application
│   ├── Controllers/          # HTTP request handling
│   ├── Data/                 # Entity Framework Core DbContext
│   ├── Models/               # Application data models
│   ├── ViewModels/           # View-specific models
│   ├── Views/                # Razor views
│   ├── wwwroot/              # Static files (CSS, etc.)
│   ├── Program.cs            # Application setup and routing
│   └── appsettings.json      # Non-sensitive application settings
├── .gitignore
└── README.md
```

The structure may evolve as new features are added.

## Development Workflow

The project uses Jira for issue tracking and GitHub for source control.

1. Create or select a Jira issue.
2. Create a feature branch from the latest `main`, including the Jira key (for example, `SCRUM-42-update-readme`).
3. Implement and test the change.
4. Commit with the Jira key in the message.
5. Push the branch and open a pull request targeting `main`.
6. Review and merge the pull request, then update the Jira issue status.

Changes should be merged through pull requests rather than committed directly to `main`.

## Notes

- The SQL Server database schema may be managed separately from Entity Framework Core; do not run automatic database creation or migrations against an existing database without checking the schema and configuration.
- Authentication, authorization, validation, and handling of personal information must be reviewed before real-world deployment.

## Project Management

- **Jira project:** Vet Website Development (`SCRUM`)
- **Repository:** [Marinel10/vet-clinic](https://github.com/Marinel10/vet-clinic)
- **Documentation task:** `SCRUM-42` — Update Project README Documentation
