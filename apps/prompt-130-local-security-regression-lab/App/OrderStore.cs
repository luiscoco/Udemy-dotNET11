using Microsoft.Data.Sqlite;
public sealed class OrderStore:IDisposable
{
    private readonly SqliteConnection connection=new("Data Source=:memory:");
    public OrderStore()
    {
        connection.Open();using var command=connection.CreateCommand();
        command.CommandText="CREATE TABLE Orders(Id INTEGER PRIMARY KEY,Owner TEXT,Title TEXT);INSERT INTO Orders VALUES(1,'alice','A course'),(2,'bob','B course');";command.ExecuteNonQuery();
    }
    public int Search(string owner,string title,bool concatenate=false)
    {
        using var command=connection.CreateCommand();
        command.CommandText=concatenate?"SELECT COUNT(*) FROM Orders WHERE Owner='"+owner+"' AND Title='"+title+"'":
            "SELECT COUNT(*) FROM Orders WHERE Owner=@owner AND Title=@title";
        if(!concatenate){command.Parameters.AddWithValue("@owner",owner);command.Parameters.AddWithValue("@title",title);}
        return Convert.ToInt32(command.ExecuteScalar());
    }
    public bool Owns(string subject,int id)
    {
        using var command=connection.CreateCommand();command.CommandText="SELECT COUNT(*) FROM Orders WHERE Id=@id AND Owner=@owner";
        command.Parameters.AddWithValue("@id",id);command.Parameters.AddWithValue("@owner",subject);return Convert.ToInt32(command.ExecuteScalar())==1;
    }
    public void Dispose()=>connection.Dispose();
}
