namespace Lumina.Persistence

open System
open System.Data
open Microsoft.Data.SqlClient
open Lumina.Domain

module Database =
    let mutable private connectionString : string option = None

    let setConnectionString (cs: string) =
        connectionString <- Some cs

    let private getConnectionString () =
        match connectionString with
        | Some cs -> cs
        | None -> failwith "Database connection string has not been configured."

    let private masterConnectionString () =
        let cs = getConnectionString()
        let b = SqlConnectionStringBuilder(cs)
        b.InitialCatalog <- "master"
        b.ToString()

    let ensureDatabaseAndTables () =
        let targetDb = (SqlConnectionStringBuilder(getConnectionString())).InitialCatalog
        use master = new SqlConnection(masterConnectionString())
        master.Open()
        use mkDb = master.CreateCommand()
        mkDb.CommandText <- $"IF DB_ID('{targetDb}') IS NULL CREATE DATABASE [{targetDb}];"
        mkDb.ExecuteNonQuery() |> ignore
        master.Close()

        use conn = new SqlConnection(getConnectionString())
        conn.Open()
        let exec (sql:string) =
            use cmd = conn.CreateCommand()
            cmd.CommandText <- sql
            cmd.ExecuteNonQuery() |> ignore
        exec """
IF OBJECT_ID('dbo.Appointments','U') IS NULL
BEGIN
  CREATE TABLE dbo.Appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    SessionDate DATETIME2 NOT NULL
  );
END
"""
        exec """
IF OBJECT_ID('dbo.BlogPosts','U') IS NULL
BEGIN
  CREATE TABLE dbo.BlogPosts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Slug NVARCHAR(200) NOT NULL UNIQUE,
    Title NVARCHAR(200) NOT NULL,
    Excerpt NVARCHAR(1000) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Published DATETIME2 NOT NULL
  );
END
"""
        exec """
IF OBJECT_ID('dbo.Photos','U') IS NULL
BEGIN
  CREATE TABLE dbo.Photos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FileName NVARCHAR(260) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Uploaded DATETIME2 NOT NULL,
    Width INT NOT NULL,
    Height INT NOT NULL
  );
END
"""
        conn.Close()

    module Appointments =
        let list () : Appointment list =
            use conn = new SqlConnection(getConnectionString())
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT Id, Name, SessionDate FROM dbo.Appointments ORDER BY SessionDate DESC"
            use r = cmd.ExecuteReader()
            let items = ResizeArray<Appointment>()
            while r.Read() do
                items.Add({ Id = r.GetInt32(0); Name = r.GetString(1); SessionDate = r.GetDateTime(2) })
            conn.Close()
            items |> List.ofSeq

        let add (name:string) (date:DateTime) : int =
            use conn = new SqlConnection(getConnectionString())
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "INSERT INTO dbo.Appointments (Name, SessionDate) OUTPUT INSERTED.Id VALUES (@n, @d)"
            cmd.Parameters.Add(SqlParameter("@n", SqlDbType.NVarChar, 200, Value = name)) |> ignore
            cmd.Parameters.Add(SqlParameter("@d", SqlDbType.DateTime2, Value = date)) |> ignore
            let idObj = cmd.ExecuteScalar()
            conn.Close()
            match idObj with :? int as id -> id | _ -> 0

    module Blog =
        let list () : BlogPost list =
            use conn = new SqlConnection(getConnectionString())
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT Id, Slug, Title, Excerpt, Content, Published FROM dbo.BlogPosts ORDER BY Published DESC"
            use r = cmd.ExecuteReader()
            let items = ResizeArray<BlogPost>()
            while r.Read() do
                items.Add({ Id=r.GetInt32(0); Slug=r.GetString(1); Title=r.GetString(2); Excerpt=r.GetString(3); Content=r.GetString(4); Published=r.GetDateTime(5) })
            conn.Close(); items |> List.ofSeq

        let tryGetBySlug (slug:string) : BlogPost option =
            use conn = new SqlConnection(getConnectionString())
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT TOP 1 Id, Slug, Title, Excerpt, Content, Published FROM dbo.BlogPosts WHERE Slug=@s"
            cmd.Parameters.Add(SqlParameter("@s", SqlDbType.NVarChar, 200, Value = slug)) |> ignore
            use r = cmd.ExecuteReader()
            let res =
                if r.Read() then
                    Some { Id=r.GetInt32(0); Slug=r.GetString(1); Title=r.GetString(2); Excerpt=r.GetString(3); Content=r.GetString(4); Published=r.GetDateTime(5) }
                else None
            conn.Close(); res

        let add (p:BlogPost) : int =
            use conn = new SqlConnection(getConnectionString())
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "INSERT INTO dbo.BlogPosts (Slug, Title, Excerpt, Content, Published) OUTPUT INSERTED.Id VALUES (@slug,@title,@excerpt,@content,@pub)"
            cmd.Parameters.Add(SqlParameter("@slug", SqlDbType.NVarChar, 200, Value = p.Slug)) |> ignore
            cmd.Parameters.Add(SqlParameter("@title", SqlDbType.NVarChar, 200, Value = p.Title)) |> ignore
            cmd.Parameters.Add(SqlParameter("@excerpt", SqlDbType.NVarChar, 1000, Value = p.Excerpt)) |> ignore
            cmd.Parameters.Add(SqlParameter("@content", SqlDbType.NVarChar, -1, Value = p.Content)) |> ignore
            cmd.Parameters.Add(SqlParameter("@pub", SqlDbType.DateTime2, Value = p.Published)) |> ignore
            let idObj = cmd.ExecuteScalar()
            conn.Close(); match idObj with :? int as id -> id | _ -> 0

    module Photos =
        let list () : Photo list =
            use conn = new SqlConnection(getConnectionString())
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT Id, FileName, Title, Uploaded, Width, Height FROM dbo.Photos ORDER BY Uploaded DESC"
            use r = cmd.ExecuteReader()
            let items = ResizeArray<Photo>()
            while r.Read() do
                items.Add({ Id=r.GetInt32(0); FileName=r.GetString(1); Title=r.GetString(2); Uploaded=r.GetDateTime(3); Width=r.GetInt32(4); Height=r.GetInt32(5) })
            conn.Close(); items |> List.ofSeq

        let add (fileName:string) (title:string) (width:int) (height:int) : int =
            use conn = new SqlConnection(getConnectionString())
            conn.Open()
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "INSERT INTO dbo.Photos (FileName, Title, Uploaded, Width, Height) OUTPUT INSERTED.Id VALUES (@f,@t,@u,@w,@h)"
            cmd.Parameters.Add(SqlParameter("@f", SqlDbType.NVarChar, 260, Value = fileName)) |> ignore
            cmd.Parameters.Add(SqlParameter("@t", SqlDbType.NVarChar, 200, Value = title)) |> ignore
            cmd.Parameters.Add(SqlParameter("@u", SqlDbType.DateTime2, Value = DateTime.UtcNow)) |> ignore
            cmd.Parameters.Add(SqlParameter("@w", SqlDbType.Int, Value = width)) |> ignore
            cmd.Parameters.Add(SqlParameter("@h", SqlDbType.Int, Value = height)) |> ignore
            let idObj = cmd.ExecuteScalar()
            conn.Close(); match idObj with :? int as id -> id | _ -> 0
