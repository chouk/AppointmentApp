namespace Lumina.Web

open Falco.Markup

module Views =
    let private nav =
        Elem.header [ Attr.class' "nav" ] [
            Elem.div [ Attr.class' "nav-inner" ] [
                Elem.a [ Attr.class' "brand"; Attr.href "/" ] [ Text.raw "Lumina" ]
                Elem.nav [] [
                    Elem.a [ Attr.href "/" ] [ Text.raw "Home" ]
                    Elem.a [ Attr.href "/appointments" ] [ Text.raw "Appointments" ]
                    Elem.a [ Attr.href "/portfolio" ] [ Text.raw "Portfolio" ]
                    Elem.a [ Attr.href "/blog" ] [ Text.raw "Blog" ]
                    Elem.a [ Attr.href "/admin" ] [ Text.raw "Admin" ]
                ]
            ]
        ]

    let private shell title headExtra bodyNodes =
        Elem.html [] [
            Elem.head [] ([
                Elem.title [] [ Text.raw title ]
                Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ]
            ] @ headExtra)
            Elem.body [] (nav :: bodyNodes @ [ Elem.footer [ Attr.class' "footer" ] [ Text.raw ("© " + System.DateTime.UtcNow.Year.ToString() + " Lumina Studio") ] ])
        ]

    let homePage =
        shell "Lumina — Photography Studio" [] [
            Elem.main [ Attr.class' "hero" ] [
                Elem.div [ Attr.class' "hero-inner" ] [
                    Elem.h1 [] [ Text.raw "Capture Light. Create Memories." ]
                    Elem.p [ Attr.class' "tagline" ] [ Text.raw "Minimal, modern photography for people and brands." ]
                    Elem.div [ Attr.class' "cta" ] [
                        Elem.a [ Attr.class' "btn"; Attr.href "/appointments" ] [ Text.raw "Book a Session" ]
                        Elem.a [ Attr.class' "link"; Attr.href "/portfolio" ] [ Text.raw "See portfolio →" ]
                    ]
                ]
            ]
        ]

    let appointmentForm =
        shell "Book Appointment" [] [
            Elem.div [ Attr.class' "container" ] [
                Elem.h1 [] [ Text.raw "Book an Appointment" ]
                Elem.form [ Attr.method "post"; Attr.action "/appointments" ] [
                    Elem.div [] [
                        Elem.label [] [ Text.raw "Name" ]
                        Elem.br []
                        Elem.input [ Attr.name "name"; Attr.placeholder "Enter your name"; Attr.required ]
                    ]
                    Elem.br []
                    Elem.div [] [
                        Elem.label [] [ Text.raw "Session Date & Time" ]
                        Elem.br []
                        Elem.input [ Attr.name "sessionDate"; Attr.type' "datetime-local"; Attr.required ]
                    ]
                    Elem.br []
                    Elem.button [ Attr.type' "submit" ] [ Text.raw "Book Appointment" ]
                ]
                Elem.hr []
                Elem.a [ Attr.href "/appointments/all" ] [ Text.raw "View all appointments →" ]
            ]
        ]

    let adminPage =
        shell "Admin" [] [
            Elem.main [ Attr.class' "content" ] [
                Elem.h1 [] [ Text.raw "Admin" ]
                Elem.ul [] [
                    Elem.li [] [ Elem.a [ Attr.href "/admin/blog" ] [ Text.raw "Create blog post" ] ]
                    Elem.li [] [ Elem.a [ Attr.href "/admin/photos" ] [ Text.raw "Upload photo" ] ]
                ]
            ]
        ]
