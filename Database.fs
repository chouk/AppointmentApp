module Database

open System
open System.Data
open Microsoft.Data.SqlClient
open Data

let mutable private connectionString : string option = None

let setConnectionString (cs: string) =
    connectionString <- Some cs

let private getConnectionString () =
    match connectionString with
    | Some cs -> cs
    | None -> failwith "Database connection string has not been configured."

let private masterConnectionString () =
    // Replace the Database=... part with 'master' to allow DB creation
    let cs = getConnectionString()
    let builder = SqlConnectionStringBuilder(cs)
    builder.InitialCatalog <- "master"
    builder.ToString()

let ensureDatabaseAndTable () =
    // Create database if it doesn't exist
    let targetDb = (SqlConnectionStringBuilder(getConnectionString())).InitialCatalog
    use masterConn = new SqlConnection(masterConnectionString())
    masterConn.Open()
    use createDbCmd = masterConn.CreateCommand()
    createDbCmd.CommandText <- $"IF DB_ID('{targetDb}') IS NULL CREATE DATABASE [{targetDb}];"
    createDbCmd.ExecuteNonQuery() |> ignore
    masterConn.Close()

    // Create table if it doesn't exist
    use conn = new SqlConnection(getConnectionString())
    conn.Open()
    use cmd = conn.CreateCommand()
    cmd.CommandText <- """
IF OBJECT_ID('dbo.Appointments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Appointments (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        SessionDate DATETIME2 NOT NULL
    );
END
"""
    cmd.ExecuteNonQuery() |> ignore
    conn.Close()

let getAppointments () : Appointment list =
    use conn = new SqlConnection(getConnectionString())
    conn.Open()
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "SELECT Id, Name, SessionDate FROM dbo.Appointments ORDER BY SessionDate DESC"
    use reader = cmd.ExecuteReader()
    let results = ResizeArray<Appointment>()
    while reader.Read() do
        let id = reader.GetInt32(0)
        let name = reader.GetString(1)
        let dt = reader.GetDateTime(2)
        results.Add({ Id = id; Name = name; SessionDate = dt })
    conn.Close()
    results |> List.ofSeq

let addAppointment (name: string) (sessionDate: DateTime) : int =
    use conn = new SqlConnection(getConnectionString())
    conn.Open()
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "INSERT INTO dbo.Appointments (Name, SessionDate) OUTPUT INSERTED.Id VALUES (@n, @d)"
    cmd.Parameters.Add(SqlParameter("@n", SqlDbType.NVarChar, 200, Value = name)) |> ignore
    cmd.Parameters.Add(SqlParameter("@d", SqlDbType.DateTime2, Value = sessionDate)) |> ignore
    let idObj = cmd.ExecuteScalar()
    conn.Close()
    match idObj with
    | :? int as id -> id
    | _ -> 0
