namespace Lumina.Domain

open System

[<CLIMutable>]
type Appointment = {
    Id : int
    Name : string
    SessionDate : DateTime
}

[<CLIMutable>]
type BlogPost = {
    Id : int
    Slug : string
    Title : string
    Excerpt : string
    Content : string
    Published : DateTime
}

[<CLIMutable>]
type Photo = {
    Id : int
    FileName : string
    Title : string
    Uploaded : DateTime
    Width : int
    Height : int
}
