open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Falco
open System
open System.Threading.Tasks
open Lumina.Persistence
open Lumina.Features
open Lumina.Domain
open Lumina.Web

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    // Configure database connection string from appsettings
    let cs = builder.Configuration.GetConnectionString("DefaultConnection")
    if String.IsNullOrWhiteSpace(cs) then
        failwith "Missing ConnectionStrings:DefaultConnection in appsettings.json"
    Database.setConnectionString cs
    Database.ensureDatabaseAndTables()
    
    let app = builder.Build()
    
    app.UseStaticFiles() |> ignore
    
    // Define routes using ASP.NET Core minimal API
    // Home (reuse existing simple home page for now - will be reimplemented in new views layer later)
    app.MapGet("/", fun ctx -> Response.ofHtml Views.homePage ctx) |> ignore
    app.MapGet("/appointments", fun ctx -> Response.ofHtml Views.appointmentForm ctx) |> ignore
    // Appointments
    app.MapGet("/appointments/all", AppointmentHandlers.list) |> ignore
    app.MapPost("/appointments", fun ctx -> AppointmentHandlers.create ctx :> Task) |> ignore
    // Blog
    app.MapGet("/blog", BlogHandlers.list) |> ignore
    app.MapGet("/blog/{slug}", BlogHandlers.detail) |> ignore
    app.MapGet("/admin/blog", BlogHandlers.adminCreateGet) |> ignore
    app.MapPost("/admin/blog", fun ctx -> BlogHandlers.adminCreatePost ctx :> Task) |> ignore
    // Photos / Portfolio
    app.MapGet("/portfolio", PhotoHandlers.portfolio) |> ignore
    app.MapGet("/admin/photos", PhotoHandlers.uploadForm) |> ignore
    app.MapPost("/admin/photos", fun ctx -> PhotoHandlers.upload ctx :> Task) |> ignore
    app.MapGet("/admin", fun ctx -> Response.ofHtml Views.adminPage ctx) |> ignore
    
    app.Run()
    0
