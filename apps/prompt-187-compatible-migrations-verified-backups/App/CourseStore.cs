using Microsoft.Data.Sqlite;
public sealed class CourseStore(string file):IDisposable
{
    public SqliteConnection Connection {get;}=new("Data Source="+file);
    public void Open()=>Connection.Open();
    public void Execute(string sql)
    {
        using var command=Connection.CreateCommand(); command.CommandText=sql; command.ExecuteNonQuery();
    }
    public string Read(string sql)
    {
        using var command=Connection.CreateCommand(); command.CommandText=sql; return Convert.ToString(command.ExecuteScalar())!;
    }
    public void BackupTo(string file)
    {
        using var destination=new SqliteConnection("Data Source="+file);
        destination.Open(); Connection.BackupDatabase(destination);
    }
    public void Expand()=>Execute("ALTER TABLE Courses ADD COLUMN Slug TEXT NULL;");
    public void Backfill()=>Execute("UPDATE Courses SET Slug=lower(replace(Title,' ','-')) WHERE Slug IS NULL;");
    public string OldRead(int id)=>Read("SELECT Title FROM Courses WHERE Id="+id);
    public string NewRead(int id)=>Read("SELECT coalesce(Slug,Title) FROM Courses WHERE Id="+id);
    public void Dispose()=>Connection.Dispose();
}
