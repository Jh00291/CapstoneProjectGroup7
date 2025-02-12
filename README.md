# TicketSystem

## Overview
TicketSystem is a work ticket tracking application designed to help teams manage and monitor their tasks efficiently. The system enables users to create, assign, update, and track work tickets to ensure smooth workflow and accountability.

## Features
- Create and manage work tickets
- Assign tickets to team members
- Track ticket status and progress
- Filter and search tickets by different criteria
- Generate reports on ticket activity

## Prerequisites
Ensure you have the following installed before setting up the project:
- .NET SDK (latest stable version)
- SQL Server or any compatible database
- Visual Studio (recommended) or any compatible IDE
- Git (optional, for version control)

## Setup Instructions

### 1. Clone the Repository
```sh
[git clone https://github.com/your-repo/TicketSystem.git](https://github.com/Jh00291/CapstoneProjectGroup7.git)
cd TicketSystem
```

### 2. Database Configuration
1. Open SQL Server and create a new database named `TicketSystemDB`.
2. Run the provided SQL script (`database.sql`) to set up the schema and tables.
3. Update the connection string in `appsettings.json`:
    ```
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TicketSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true"
      }
    ```

### 3. Build and Run the Application
#### Using Visual Studio
1. Open `CapstoneProjectG7.sln` in Visual Studio.
2. Set the startup project to `TicketSystem`.
3. Press `F5` to build and run the application.

#### Using .NET CLI
```sh
dotnet restore
dotnet build
dotnet run
```

### 4. Accessing the Application
Once the application is running, you can access it via:
```
http://localhost:[PORT_NUMBER]
```
