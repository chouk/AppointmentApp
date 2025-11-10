# Lumina Studio Web App

An F# / Falco application for a photography studio (Lumina) featuring appointments, portfolio photo uploads, and a database‑backed blog with a lightweight admin area.

## Features

- **Modern Home Page**: Minimal hero & brand navigation
- **Appointments**: Book + view photography session times
- **Portfolio**: Upload images (stored on disk) & display in masonry-style grid
- **Blog**: Create and read posts stored in SQL Server (slug-based)
- **Admin**: Simple unprotected dashboard to add blog posts and upload photos
- **SQL Persistence**: Appointments, BlogPosts, Photos tables auto-created on startup

## Technology Stack

- **F#** - Programming language
- **Falco 5.x** - Lightweight F# web framework
- **ASP.NET Core** - Web hosting
- **.NET 9.0** - Runtime

## Project Structure (Layered)

```
AppointmentApp/
├── Domain/Domain.fs                 # Pure domain types (Appointment, BlogPost, Photo)
├── Persistence/Database.fs          # DB ensure + CRUD modules (Appointments, Blog, Photos)
├── Features/AppointmentHandlers.fs  # Appointment HTTP handlers
├── Features/BlogHandlers.fs         # Blog list/detail + admin create
├── Features/PhotoHandlers.fs        # Portfolio display + photo upload
├── Web/Views.fs                     # Shared view/layout (home, appointment form, admin)
├── Program.fs                       # Composition root (routing, startup)
├── wwwroot/styles.css               # Global CSS (layout, hero, grid)
├── wwwroot/uploads/                 # Stored photo files
└── AppointmentApp.fsproj            # Project file
```

## Running the Application

1. Make sure you have .NET 9.0 SDK installed
2. Navigate to the project directory
3. Run the application:
   ```bash
   dotnet run
   ```
4. Open your browser to `https://localhost:5001`

## Key Endpoints

Public:
- `/` – Home
- `/appointments` – Booking form
- `/appointments/all` – List all appointments
- `/portfolio` – Portfolio grid
- `/blog` – Blog index
- `/blog/{slug}` – Blog post detail

Admin (no auth yet):
- `/admin` – Dashboard
- `/admin/blog` – Create blog post (POST same URL)
- `/admin/photos` – Upload photo (POST same URL)

## Architecture Notes

- **Layered**: Domain (pure), Persistence (infrastructure), Features (HTTP handlers), Web (views), Program (composition)
- **Falco Markup** for strongly-typed HTML generation
- **Minimal API** for route mapping
- **Auto DB Provisioning** for three tables: Appointments, BlogPosts, Photos
- **Static File Storage** for uploaded images (wwwroot/uploads)

## Potential Enhancements

- Authentication & authorization (protect /admin)
- Image dimension extraction & responsive srcset
- Edit/delete blog posts and photos
- Appointment cancellation & status workflow
- Pagination for large blog/portfolio sets
- Tagging & categorization
- Slug auto-generation & uniqueness checks
## Database Setup

The application will attempt to create the database (PhotoAppointments) and the table (dbo.Appointments) automatically if they do not exist. It uses the connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`.

Default connection string (local):

```
Server=localhost;Database=PhotoAppointments;Trusted_Connection=True;TrustServerCertificate=True;
```

Ensure you have a local SQL Server instance running (e.g. SQL Server Developer Edition or LocalDB adapted accordingly).

To use LocalDB instead, change the connection string to:

```
Server=(localdb)\\MSSQLLocalDB;Database=PhotoAppointments;Trusted_Connection=True;
```

## Database Table Schemas (Simplified)

```
Appointments(Id INT IDENTITY PK, Name NVARCHAR(200), SessionDate DATETIME2)
BlogPosts(Id INT IDENTITY PK, Slug NVARCHAR(200) UNIQUE, Title NVARCHAR(200), Excerpt NVARCHAR(1000), Content NVARCHAR(MAX), Published DATETIME2)
Photos(Id INT IDENTITY PK, FileName NVARCHAR(260), Title NVARCHAR(200), Uploaded DATETIME2, Width INT, Height INT)
```

## Notes

* Dates stored as provided (normalize to UTC recommended)
* Validation minimal (improve before production)
* DB provisioning is ad-hoc; use migrations for production
* Admin endpoints are open – secure before deployment

## Photo Upload Notes

- Files saved under `wwwroot/uploads` with GUID names.
- Width/Height currently set to 0 (add image probing later).
- Portfolio layout uses CSS multi-column masonry (`.grid`).

## Running the Application

```powershell
# Build
dotnet build
# Run
dotnet run
# Browse (HTTPS by launch profile)
https://localhost:5001/
```