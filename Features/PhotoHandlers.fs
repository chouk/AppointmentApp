namespace Lumina.Features

open System
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Falco
open Falco.Markup
open Lumina.Persistence
open Lumina.Domain

module PhotoHandlers =
    let portfolio (ctx: HttpContext) =
        let photos = Database.Photos.list()
        let html =
            Elem.html [] [
                Elem.head [] [ Elem.title [] [ Text.raw "Portfolio" ]; Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ] ]
                Elem.body [] [
                    Elem.header [ Attr.class' "nav" ] [
                        Elem.div [ Attr.class' "nav-inner" ] [
                            Elem.a [ Attr.class' "brand"; Attr.href "/" ] [ Text.raw "Lumina" ]
                            Elem.nav [] [ Elem.a [ Attr.href "/" ] [ Text.raw "Home" ]; Elem.a [ Attr.href "/appointments" ] [ Text.raw "Appointments" ]; Elem.a [ Attr.href "/blog" ] [ Text.raw "Blog" ]; Elem.a [ Attr.href "/portfolio" ] [ Text.raw "Portfolio" ] ]
                        ]
                    ]
                    Elem.main [ Attr.class' "content" ] [
                        Elem.h1 [] [ Text.raw "Portfolio" ]
                        if List.isEmpty photos then Elem.p [] [ Text.raw "No photos uploaded yet." ] else
                        Elem.div [ Attr.class' "grid" ] (
                            photos |> List.map (fun p ->
                                Elem.figure [ Attr.class' "shot" ] [
                                    Elem.img [ Attr.src ("/uploads/" + p.FileName); Attr.alt p.Title ]
                                    Elem.figcaption [] [ Text.raw p.Title ]
                                ]))
                    ]
                ]
            ]
        Response.ofHtml html ctx

    let uploadForm (ctx: HttpContext) =
        let html =
            Elem.html [] [
                Elem.head [] [ Elem.title [] [ Text.raw "Upload Photo" ]; Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ] ]
                Elem.body [] [
                    Elem.header [ Attr.class' "nav" ] [
                        Elem.div [ Attr.class' "nav-inner" ] [ Elem.a [ Attr.class' "brand"; Attr.href "/" ] [ Text.raw "Lumina" ] ]
                    ]
                    Elem.main [ Attr.class' "content" ] [
                        Elem.h1 [] [ Text.raw "Upload Photo" ]
                        Elem.form [ Attr.method "post"; Attr.action "/admin/photos"; Attr.enctype "multipart/form-data" ] [
                            Elem.div [] [ Elem.label [] [ Text.raw "Title" ]; Elem.br []; Elem.input [ Attr.name "title"; Attr.required ] ]
                            Elem.br []
                            Elem.div [] [ Elem.label [] [ Text.raw "File" ]; Elem.br []; Elem.input [ Attr.type' "file"; Attr.name "file"; Attr.required ] ]
                            Elem.br []
                            Elem.button [ Attr.type' "submit" ] [ Text.raw "Upload" ]
                        ]
                    ]
                ]
            ]
        Response.ofHtml html ctx

    let upload (ctx: HttpContext) = task {
        if ctx.Request.HasFormContentType then
            let form = ctx.Request.Form
            let title = form.["title"].ToString()
            let files = ctx.Request.Form.Files
            if files.Count = 1 then
                let file = files.[0]
                let env = ctx.RequestServices.GetService(typeof<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>) :?> Microsoft.AspNetCore.Hosting.IWebHostEnvironment
                let uploadsPath = Path.Combine(env.WebRootPath, "uploads")
                if not (Directory.Exists uploadsPath) then Directory.CreateDirectory uploadsPath |> ignore
                let fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName)
                let savePath = Path.Combine(uploadsPath, fileName)
                use fs = new FileStream(savePath, FileMode.Create)
                do! file.CopyToAsync(fs)
                // naive size (set 0,0 for now or attempt to read via System.Drawing if available)
                let width, height = 0, 0
                Database.Photos.add fileName title width height |> ignore
                ctx.Response.Redirect("/portfolio")
            else
                ctx.Response.StatusCode <- 400
                do! ctx.Response.WriteAsync("Exactly one file required")
        else
            ctx.Response.StatusCode <- 400
            do! ctx.Response.WriteAsync("Invalid form submission")
    }
