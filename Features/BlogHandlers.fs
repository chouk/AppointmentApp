namespace Lumina.Features

open System
open Microsoft.AspNetCore.Http
open Falco
open Falco.Markup
open Lumina.Domain
open Lumina.Persistence

module BlogHandlers =
    let list (ctx: HttpContext) =
        let posts = Database.Blog.list()
        let html =
            Elem.html [] [
                Elem.head [] [ Elem.title [] [ Text.raw "Lumina Blog" ]; Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ] ]
                Elem.body [] [
                    Elem.header [ Attr.class' "nav" ] [
                        Elem.div [ Attr.class' "nav-inner" ] [
                            Elem.a [ Attr.class' "brand"; Attr.href "/" ] [ Text.raw "Lumina" ]
                            Elem.nav [] [ Elem.a [ Attr.href "/" ] [ Text.raw "Home" ]; Elem.a [ Attr.href "/appointments" ] [ Text.raw "Appointments" ]; Elem.a [ Attr.href "/blog" ] [ Text.raw "Blog" ]; Elem.a [ Attr.href "/portfolio" ] [ Text.raw "Portfolio" ] ]
                        ]
                    ]
                    Elem.main [ Attr.class' "content" ] [
                        Elem.h1 [] [ Text.raw "Lumina Blog" ]
                        Elem.ul [ Attr.class' "posts" ] (
                            posts |> List.map (fun p ->
                                Elem.li [] [
                                    Elem.a [ Attr.href ("/blog/" + p.Slug) ] [ Text.raw p.Title ]
                                    Elem.p [ Attr.class' "excerpt" ] [ Text.raw p.Excerpt ]
                                ]))
                    ]
                ]
            ]
        Response.ofHtml html ctx

    let detail (ctx: HttpContext) =
        let slug = ctx.Request.RouteValues["slug"].ToString()
        match Database.Blog.tryGetBySlug slug with
        | Some post ->
            let html =
                Elem.html [] [
                    Elem.head [] [ Elem.title [] [ Text.raw post.Title ]; Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ] ]
                    Elem.body [] [
                        Elem.header [ Attr.class' "nav" ] [
                            Elem.div [ Attr.class' "nav-inner" ] [
                                Elem.a [ Attr.class' "brand"; Attr.href "/" ] [ Text.raw "Lumina" ]
                                Elem.nav [] [ Elem.a [ Attr.href "/" ] [ Text.raw "Home" ]; Elem.a [ Attr.href "/appointments" ] [ Text.raw "Appointments" ]; Elem.a [ Attr.href "/blog" ] [ Text.raw "Blog" ]; Elem.a [ Attr.href "/portfolio" ] [ Text.raw "Portfolio" ] ]
                            ]
                        ]
                        Elem.main [ Attr.class' "content" ] [
                            Elem.h1 [] [ Text.raw post.Title ]
                            Elem.p [ Attr.class' "published" ] [ Text.raw (post.Published.ToString("yyyy-MM-dd")) ]
                            Elem.p [] [ Text.raw post.Content ]
                            Elem.p [] [ Elem.a [ Attr.href "/blog" ] [ Text.raw "← Back to blog" ] ]
                        ]
                    ]
                ]
            Response.ofHtml html ctx
        | None ->
            ctx.Response.StatusCode <- 404
            Response.ofPlainText "Post not found" ctx

    // Simple admin create page and post
    let adminCreateGet (ctx: HttpContext) =
        let html =
            Elem.html [] [
                Elem.head [] [ Elem.title [] [ Text.raw "New Blog Post" ]; Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ] ]
                Elem.body [] [
                    Elem.header [ Attr.class' "nav" ] [
                        Elem.div [ Attr.class' "nav-inner" ] [
                            Elem.a [ Attr.class' "brand"; Attr.href "/" ] [ Text.raw "Lumina" ]
                            Elem.nav [] [ Elem.a [ Attr.href "/admin" ] [ Text.raw "Admin" ] ]
                        ]
                    ]
                    Elem.main [ Attr.class' "content" ] [
                        Elem.h1 [] [ Text.raw "Create Blog Post" ]
                        Elem.form [ Attr.method "post"; Attr.action "/admin/blog" ] [
                            Elem.div [] [ Elem.label [] [ Text.raw "Slug" ]; Elem.br []; Elem.input [ Attr.name "slug"; Attr.required ] ]
                            Elem.br []
                            Elem.div [] [ Elem.label [] [ Text.raw "Title" ]; Elem.br []; Elem.input [ Attr.name "title"; Attr.required ] ]
                            Elem.br []
                            Elem.div [] [ Elem.label [] [ Text.raw "Excerpt" ]; Elem.br []; Elem.textarea [ Attr.name "excerpt"; Attr.rows "3" ] [] ]
                            Elem.br []
                            Elem.div [] [ Elem.label [] [ Text.raw "Content" ]; Elem.br []; Elem.textarea [ Attr.name "content"; Attr.rows "8" ] [] ]
                            Elem.br []
                            Elem.button [ Attr.type' "submit" ] [ Text.raw "Publish" ]
                        ]
                    ]
                ]
            ]
        Response.ofHtml html ctx

    let adminCreatePost (ctx: HttpContext) = task {
        if ctx.Request.HasFormContentType then
            let slug = ctx.Request.Form["slug"].ToString()
            let title = ctx.Request.Form["title"].ToString()
            let excerpt = ctx.Request.Form["excerpt"].ToString()
            let content = ctx.Request.Form["content"].ToString()
            if not (System.String.IsNullOrWhiteSpace(slug)) && not (System.String.IsNullOrWhiteSpace(title)) then
                let id = Database.Blog.add { Id=0; Slug=slug; Title=title; Excerpt=excerpt; Content=content; Published=System.DateTime.UtcNow }
                ctx.Response.Redirect("/blog/" + slug)
            else
                ctx.Response.StatusCode <- 400
                do! ctx.Response.WriteAsync("Invalid form data")
        else
            ctx.Response.StatusCode <- 400
            do! ctx.Response.WriteAsync("Invalid form data")
    }
