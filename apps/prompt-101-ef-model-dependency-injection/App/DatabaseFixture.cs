using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
public sealed class SqlTrace : DbCommandInterceptor
{
    public List<string> Commands { get; } = [];
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData data, InterceptionResult<DbDataReader> result)
    { Commands.Add(command.CommandText); return result; }
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData data, InterceptionResult<int> result)
    { Commands.Add(command.CommandText); return result; }
    public void Reset() => Commands.Clear();
    public void Print(string label)
    {
        Console.WriteLine($"database: {label}: {Commands.Count} commands");
        foreach(var sql in Commands) Console.WriteLine(sql);
    }
}
public sealed class DatabaseFixture : IDisposable
{
    public SqlTrace Trace { get; } = new();
    public DbContextOptions<SchoolContext> Options { get; }
    private readonly string file;
    public string ConnectionString => $"Data Source={file};Pooling=False;Foreign Keys=True";
    public DatabaseFixture()
    {
        Directory.CreateDirectory("artifacts");
        file=Path.GetFullPath(Path.Combine("artifacts",Guid.NewGuid()+".db"));
        Options=new DbContextOptionsBuilder<SchoolContext>()
            .UseSqlite($"Data Source={file};Pooling=False;Foreign Keys=True")
            .AddInterceptors(Trace).Options;
        using var db=Open(); db.Database.EnsureCreated();
        db.Courses.AddRange(Enumerable.Range(1,3).Select(i=>new Course
        {
            Title=$"Course {i}",Seats=i*10,Revision=1,
            Enrollments=[new(){Student=$"Ada {i}"},new(){Student=$"Luis {i}"}]
        }));
        db.SaveChanges(); Trace.Reset();
    }
    public SchoolContext Open()=>new(Options);
    public void Dispose(){if(File.Exists(file)) File.Delete(file);}
}

