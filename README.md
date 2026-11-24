# GearGrid

## Overview

GearGrid is a multi-provider equipment and event-essentials rental platform built with ASP.NET Core Razor Pages. It connects local providers with renters looking for everything from audio and lighting to tents, seating, and power.

## Key Features

- **Renters:** Search and filter available inventory, review daily rates and provider details, and book rentals with digital rental agreements.
- **Providers:** Manage company profiles, list and update equipment with image uploads, and approve or decline incoming rental orders.
- **Admins:** Manage equipment categories, oversee registered users and companies, and monitor platform-wide orders.

## Tech Stack

- ASP.NET Core 10.0 (Razor Pages)
- Entity Framework Core 10.0 (Code-First)
- SQL Server
- Cookie Authentication
- Custom Liquid Morphism UI

## Getting Started

1. Clone the repository and open the solution in Visual Studio.
2. In `appsettings.json`, verify that `ConnectionStrings:DefaultConnection` points to your SQL Server instance and database.
3. In Visual Studio's Package Manager Console, run:

   ```powershell
   Update-Database
   ```

4. Press **F5** to build and run the application.

On startup, `DbInitializer` applies pending migrations and seeds the default equipment categories and admin account if they are not already present.
