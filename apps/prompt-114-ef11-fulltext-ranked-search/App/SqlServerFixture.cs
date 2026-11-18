using Microsoft.Data.SqlClient;
public sealed class SqlServerFixture : IAsyncDisposable
{
    private readonly string database="course_"+Guid.NewGuid().ToString("N");
    private readonly string master=Environment.GetEnvironmentVariable("COURSE_SQLSERVER") ??
        "Server=127.0.0.1,15433;Database=master;User Id=sa;Password=Course_11_Local!;Encrypt=True;TrustServerCertificate=True;Connect Timeout=15";
    public string ConnectionString {get;private set;}="";
    public async Task StartAsync()
    {
        await using var connection=new SqlConnection(master);await connection.OpenAsync();
        await using var command=connection.CreateCommand();
        command.CommandText=$"CREATE DATABASE [{database}] COLLATE SQL_Latin1_General_CP1_CI_AS";
        await command.ExecuteNonQueryAsync();
        var builder=new SqlConnectionStringBuilder(master){InitialCatalog=database,Pooling=false};
        ConnectionString=builder.ConnectionString;
    }
    public async ValueTask DisposeAsync()
    {
        await using var connection=new SqlConnection(master);await connection.OpenAsync();
        await using var command=connection.CreateCommand();
        command.CommandText=$"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END";
        await command.ExecuteNonQueryAsync();
    }
}
