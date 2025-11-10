namespace Lumina.Features

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Falco
open Falco.Markup
open Lumina.Persistence
open Lumina.Domain

module AppointmentHandlers =
    let list (ctx: HttpContext) =
        let items = Database.Appointments.list()
        let html =
            Elem.html [] [
                Elem.head [] [ Elem.title [] [ Text.raw "Appointments" ]; Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ] ]
                Elem.body [] [
                    Elem.header [ Attr.class' "nav" ] [
                        Elem.div [ Attr.class' "nav-inner" ] [
                            Elem.a [ Attr.class' "brand"; Attr.href "/" ] [ Text.raw "Lumina" ]
                            Elem.nav [] [ Elem.a [ Attr.href "/" ] [ Text.raw "Home" ]; Elem.a [ Attr.href "/appointments" ] [ Text.raw "Appointments" ]; Elem.a [ Attr.href "/blog" ] [ Text.raw "Blog" ]; Elem.a [ Attr.href "/portfolio" ] [ Text.raw "Portfolio" ] ]
                        ]
                    ]
                    Elem.div [ Attr.class' "content" ] [
                        Elem.h1 [] [ Text.raw "All Appointments" ]
                        if List.isEmpty items then Elem.p [] [ Text.raw "No appointments yet." ] else
                        Elem.ul [] (
                            items |> List.map (fun a -> Elem.li [] [ Text.raw (sprintf "%s — %s" a.Name (a.SessionDate.ToString("yyyy-MM-dd HH:mm"))) ]))
                    ]
                ]
            ]
        Response.ofHtml html ctx

    let create (ctx: HttpContext) = task {
        if ctx.Request.HasFormContentType then
            let name = ctx.Request.Form.["name"].ToString()
            let dateStr = ctx.Request.Form.["sessionDate"].ToString()
            let mutable dt = DateTime.MinValue
            if not (String.IsNullOrWhiteSpace(name)) && DateTime.TryParse(dateStr, &dt) then
                Database.Appointments.add name dt |> ignore
                ctx.Response.Redirect("/appointments/all")
            else
                ctx.Response.StatusCode <- 400
                do! ctx.Response.WriteAsync("Missing or invalid name or date")
        else
            ctx.Response.StatusCode <- 400
            do! ctx.Response.WriteAsync("Invalid form data")
    }
