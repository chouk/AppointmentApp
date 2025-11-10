open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Falco
open Appointments
open Views
open System
open System.Threading.Tasks
open Database

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    // Configure database connection string from appsettings
    let cs = builder.Configuration.GetConnectionString("DefaultConnection")
    if String.IsNullOrWhiteSpace(cs) then
        failwith "Missing ConnectionStrings:DefaultConnection in appsettings.json"
    Database.setConnectionString cs
    Database.ensureDatabaseAndTable()
    
    let app = builder.Build()
    
    app.UseStaticFiles() |> ignore
    
    // Define routes using ASP.NET Core minimal API
    app.MapGet("/", fun (ctx: HttpContext) -> Response.ofHtml appointmentForm ctx) |> ignore
    app.MapGet("/appointments", fun (ctx: HttpContext) -> getAppointmentsHandler ctx) |> ignore  
    app.MapPost("/appointments", fun (ctx: HttpContext) -> postAppointmentHandler ctx :> Task) |> ignore
    
    app.Run()
    0
