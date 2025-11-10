module Appointments

open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open System
open Falco
open Falco.Markup
open Database
open Data

let getAppointmentsHandler (ctx: HttpContext) =
    let html = 
        Elem.html [] [
            Elem.head [] [
                Elem.title [] [ Text.raw "Appointments" ]
                Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ]
            ]
            Elem.body [] [
                Elem.div [ Attr.class' "container" ] [
                    Elem.h1 [] [ Text.raw "All Appointments" ]
                    let items = Database.getAppointments()
                    if List.isEmpty items then
                        Elem.p [] [ Text.raw "No appointments yet." ]
                    else
                        Elem.ul [] (
                            items
                            |> List.map (fun a -> 
                                Elem.li [] [ Text.raw (sprintf "%s — %s" a.Name (a.SessionDate.ToString("yyyy-MM-dd HH:mm"))) ]))
                    Elem.hr []
                    Elem.a [ Attr.href "/" ] [ Text.raw "← Back to booking form" ]
                ]
            ]
        ]
    Response.ofHtml html ctx

let postAppointmentHandler (ctx: HttpContext) = task {
    if ctx.Request.HasFormContentType then
        let name = ctx.Request.Form.["name"].ToString()
        let dateStr = ctx.Request.Form.["sessionDate"].ToString()
        let mutable dt = DateTime.MinValue
        let okDate = DateTime.TryParse(dateStr, &dt)
        if not (String.IsNullOrWhiteSpace(name)) && okDate then
            Database.addAppointment name dt |> ignore
            ctx.Response.Redirect("/appointments")
        else
            ctx.Response.StatusCode <- 400
            do! ctx.Response.WriteAsync("Missing or invalid name or session date")
    else
        ctx.Response.StatusCode <- 400
        do! ctx.Response.WriteAsync("Invalid form data")
}
