module Views

open Falco.Markup

let appointmentForm =
    Elem.html [] [
        Elem.head [] [
            Elem.title [] [ Text.raw "Book Appointment" ]
            Elem.link [ Attr.rel "stylesheet"; Attr.href "/styles.css" ]
        ]
        Elem.body [] [
            Elem.div [ Attr.class' "container" ] [
                Elem.h1 [] [ Text.raw "Book an Appointment" ]
                Elem.form [ Attr.method "post"; Attr.action "/appointments" ] [
                    Elem.div [] [
                        Elem.label [] [ Text.raw "Name: " ]
                        Elem.br []
                        Elem.input [ Attr.name "name"; Attr.placeholder "Enter your name"; Attr.required ]
                    ]
                    Elem.br []
                    Elem.div [] [
                        Elem.label [] [ Text.raw "Session Date & Time: " ]
                        Elem.br []
                        Elem.input [ Attr.name "sessionDate"; Attr.type' "datetime-local"; Attr.required ]
                    ]
                    Elem.br []
                    Elem.button [ Attr.type' "submit" ] [ Text.raw "Book Appointment" ]
                ]
                Elem.hr []
                Elem.a [ Attr.href "/appointments" ] [ Text.raw "View all appointments →" ]
            ]
        ]
    ]
