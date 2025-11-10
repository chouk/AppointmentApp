# Photography Session Appointment App

An F# web application built with Falco and ASP.NET Core for booking and viewing photography session appointments stored in SQL Server.

## Features

- **Book Appointments**: Enter client name & session date/time
- **View Appointments**: Shows all stored appointments (name + date/time)
- **Clean UI**: Responsive design with simple styling
 - **SQL Persistence**: Appointments saved to SQL Server

## Technology Stack

- **F#** - Programming language
- **Falco 5.x** - Lightweight F# web framework
- **ASP.NET Core** - Web hosting
- **.NET 9.0** - Runtime

## Project Structure

```
AppointmentApp/
├── Data.fs                    # Domain model (Appointment record)
├── Database.fs                # SQL Server persistence (create/read/insert)
├── Views.fs                   # HTML markup generation
├── Appointments.fs            # Request handlers and business logic
├── Program.fs                 # Application entry point and routing
├── AppointmentApp.fsproj      # Project configuration
└── wwwroot/
    └── styles.css             # Application styling
```

## Running the Application

1. Make sure you have .NET 9.0 SDK installed
2. Navigate to the project directory
3. Run the application:
   ```bash
   dotnet run
   ```
4. Open your browser to `https://localhost:5001`

## Usage

1. **Book an appointment**: Fill in name and pick session date/time then submit
2. **View appointments**: Click "View all appointments" to see list ordered by most recent session date
3. **Navigate back**: Use the "Back to booking form" link to return to the booking page

## Architecture Notes

- **Simple and Lightweight**: Uses minimal dependencies and clean F# code
- **SQL Storage**: Appointments stored in dbo.Appointments (Id, Name, SessionDate)
- **Functional Design**: Leverages F# functional programming patterns
- **Type Safety**: Benefits from F#'s strong type system
- **Modern Web Stack**: Built on ASP.NET Core with minimal API approach

## Potential Enhancements

- Add edit/update & cancellation
- Add authentication / admin interface
- Add validation for session date (future only) & name length
- Add paging / filtering by date range
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

## Appointment Table Schema

```
CREATE TABLE dbo.Appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    SessionDate DATETIME2 NOT NULL
);
```

## Notes

- All dates are stored in UTC or server local time (no conversion currently). Consider normalizing to UTC.
- No input validation beyond non-empty name and parsable date – add stricter rules as needed.
- For production, move DB creation into migrations and restrict permissions.

- Add appointment cancellation
- Implement user authentication
- Add form validation
- Include appointment categories or types