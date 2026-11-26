# GearGrid Architecture

This document is the team’s source of truth for GearGrid’s application structure and conventions.

## Tech Stack & Environment

- ASP.NET Core 10.0 with Razor Pages.
- Entity Framework Core 10.0 using the Code-First workflow.
- SQL Server database: `GearGridDB`.
- NuGet warnings `NU1902;NU1903` are suppressed in `GearGrid.csproj`.

## Branch Baseline

- `main` is the repository's default and integration branch.
- Create new feature branches from the latest `main` branch.

## Folder Structure

- `Models/`: Six domain entities — `User`, `Company`, `Category`, `Equipment`, `RentalOrder`, and `RentalAgreement`.
- `Data/`: `ApplicationDbContext.cs` defines the EF Core model; `DbInitializer.cs` applies migrations and seeds default categories and an Admin user on startup.
- `Services/`: Business logic and shared helpers.
- `Pages/`: Module-isolated Razor Pages in `Account/`, `Admin/`, `Browse/`, `Provider/`, and `Renter/`, with shared UI in `Shared/`.

## Database & EF Core Conventions

- `DeleteBehavior.Restrict` is configured on `Company` and `RentalOrder` foreign-key relationships to prevent SQL Server multiple-cascade-path errors.
- Unique indexes are defined on `User.Email` and `RentalOrder.OrderReference`.
- **Never alter table schemas directly in SSMS.** Update `Models/`, create a migration with `Add-Migration`, and include the generated `Migrations/` files in your changes. After pulling, apply migrations locally with `Update-Database`.

## Roles

The supported user roles are `Renter`, `Provider`, and `Admin`.

## UI & Design System (Liquid Morphism)

- Define reusable CSS tokens and utility classes in `wwwroot/css/site.css`.
- Use a Liquid Morphism / frosted-glass aesthetic: `backdrop-filter: blur(...)`, translucent `rgba` surfaces, subtle 1px light borders, soft ambient shadows, crisp geometric typography, and high-contrast readable text.
- All module pages must inherit `Pages/Shared/_Layout.cshtml` and use the shared glassmorphism classes rather than introducing module-specific visual systems.
