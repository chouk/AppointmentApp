module Data

open System

// Domain model for a photography session appointment
type Appointment = {
    Id : int
    Name : string
    SessionDate : DateTime
}
