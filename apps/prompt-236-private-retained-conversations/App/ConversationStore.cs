using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
public sealed class ConversationStore(string file,bool unsafeAccess) : IDisposable
{
    private readonly SqliteConnection db=Open(file);
    private static SqliteConnection Open(string file)
    {
        var db=new SqliteConnection($"Data Source={file};Pooling=False");db.Open();
        using var command=db.CreateCommand();command.CommandText="CREATE TABLE conversations(id TEXT PRIMARY KEY,owner TEXT,expires TEXT,body TEXT)";
        command.ExecuteNonQuery();return db;
    }
    public static string Redact(string text)
    {
        text=Regex.Replace(text,@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}","[EMAIL]",RegexOptions.None,TimeSpan.FromSeconds(1));
        return Regex.Replace(text,@"SECRET-[A-Z0-9-]+","[SECRET]",RegexOptions.None,TimeSpan.FromSeconds(1));
    }
    public void Add(string id,string owner,DateTimeOffset now,string raw)
    {
        if(raw.Length>2000)throw new ArgumentException("Conversation exceeds teaching limit");
        using var command=db.CreateCommand();command.CommandText="INSERT INTO conversations VALUES($id,$owner,$expires,$body)";
        command.Parameters.AddWithValue("$id",id);command.Parameters.AddWithValue("$owner",owner);
        command.Parameters.AddWithValue("$expires",now.AddDays(7).ToString("O"));command.Parameters.AddWithValue("$body",Redact(raw));command.ExecuteNonQuery();
    }
    public string? Read(string id,string trustedOwner,DateTimeOffset now)
    {
        using var command=db.CreateCommand();command.CommandText=unsafeAccess?
            "SELECT body FROM conversations WHERE id=$id AND expires>$now":
            "SELECT body FROM conversations WHERE id=$id AND owner=$owner AND expires>$now";
        command.Parameters.AddWithValue("$id",id);command.Parameters.AddWithValue("$owner",trustedOwner);
        command.Parameters.AddWithValue("$now",now.ToString("O"));return command.ExecuteScalar() as string;
    }
    public int Purge(DateTimeOffset now)
    {
        using var command=db.CreateCommand();command.CommandText="DELETE FROM conversations WHERE expires<=$now";
        command.Parameters.AddWithValue("$now",now.ToString("O"));return command.ExecuteNonQuery();
    }
    public void Dispose()=>db.Dispose();
}
