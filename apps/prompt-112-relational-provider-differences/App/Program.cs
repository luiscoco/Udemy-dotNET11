using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Npgsql;
await using var sqlFixture=new SqlServerFixture();await sqlFixture.StartAsync();
var results=new List<ProviderResult>();
await using(var sql=new SqlConnection(sqlFixture.ConnectionString))
    results.Add(await Compare(sql,"SQL Server","#Samples","nvarchar(100)"));
await using(var pg=new NpgsqlConnection(Environment.GetEnvironmentVariable("COURSE_POSTGRES")??"Host=127.0.0.1;Port=15432;Database=postgres;Username=postgres;Password=course-local;Pooling=false"))
    results.Add(await Compare(pg,"PostgreSQL","samples","text",temporary:true));
await using(var lite=new SqliteConnection("Data Source=:memory:"))
    results.Add(await Compare(lite,"SQLite","samples","text"));
foreach(var row in results)Console.WriteLine($"database: {row.Name}; case matches={row.CaseMatches}; accepted NULL keys={row.NullKeys}; rollback rows={row.RollbackRows}");
Check.That(results.Select(x=>x.NullKeys).SequenceEqual(new[]{1,2,2}),"nullable unique-key semantics differ across the actual providers");
Check.That(results.Select(x=>x.CaseMatches).SequenceEqual(new[]{1,0,0}),"configured SQL Server CI collation differs from default SQLite and PostgreSQL equality");
if(args.FirstOrDefault()=="portable")Check.That(results.Select(x=>x.CaseMatches).Distinct().Count()==1,"incorrect expectation: all collations compare case identically");
Check.That(results.All(x=>x.RollbackRows==0),"explicit rollback preserves the shared transaction contract");
static async Task<ProviderResult> Compare(DbConnection connection,string name,string table,string textType,bool temporary=false)
{
    await connection.OpenAsync();
    async Task<int> Execute(string sql)
    {await using var command=connection.CreateCommand();command.CommandText=sql;return await command.ExecuteNonQueryAsync();}
    async Task<int> Count(string predicate)
    {await using var command=connection.CreateCommand();command.CommandText=$"SELECT COUNT(*) FROM {table} WHERE {predicate}";return Convert.ToInt32(await command.ExecuteScalarAsync());}
    await Execute($"CREATE {(temporary?"TEMPORARY ":"")}TABLE {table} (Id int NOT NULL PRIMARY KEY, Name {textType} NOT NULL, Code int NULL UNIQUE)");
    await Execute($"INSERT INTO {table} (Id,Name,Code) VALUES (1,'DotNet',NULL)");
    try{await Execute($"INSERT INTO {table} (Id,Name,Code) VALUES (2,'Other',NULL)");}catch(DbException){Console.WriteLine($"database: {name} rejects second NULL in unique column");}
    var nulls=await Count("Code IS NULL");
    await using(var transaction=await connection.BeginTransactionAsync())
    {
        await using var command=connection.CreateCommand();command.Transaction=transaction;
        command.CommandText=$"INSERT INTO {table} (Id,Name,Code) VALUES (3,@name,3)";
        var parameter=command.CreateParameter();parameter.ParameterName="@name";parameter.Value="rolled back";command.Parameters.Add(parameter);
        await command.ExecuteNonQueryAsync();await transaction.RollbackAsync();
    }
    return new(name,await Count("Name='dotnet'"),nulls,await Count("Id=3"));
}
public sealed record ProviderResult(string Name,int CaseMatches,int NullKeys,int RollbackRows);
