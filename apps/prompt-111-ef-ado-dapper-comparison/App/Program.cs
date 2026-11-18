using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Dapper;
using var connection=new SqliteConnection("Data Source=:memory:");connection.Open();
using var db=new CourseContext(new DbContextOptionsBuilder<CourseContext>().UseSqlite(connection).Options);
db.Database.EnsureCreated();db.Courses.AddRange(new Course{Title="Data access",Seats=12},new Course{Title="Web apps",Seats=8});db.SaveChanges();
var title=args.FirstOrDefault()=="parameter"?"Data access' OR 1=1 --":"Data access";
var ef=ReadWithEf(db,title);
var ado=ReadWithAdo(connection,title);
var dapper=ReadWithDapper(connection,title);
Console.WriteLine($"database: EF={ef.Count}; ADO.NET={ado.Count}; Dapper={dapper.Count}");
Check.That(ef.SequenceEqual(ado) && ado.SequenceEqual(dapper),"all three parameterized paths return the same rows");
if(args.FirstOrDefault()=="parameter")
    Check.That(ef.Count==1,"incorrect expectation: hostile value matches a course");
Check.That(ef.Single().Seats==12,"selected data is mapped consistently");
Check.That(ReadWithEf(db,"' OR 1=1 --").Count==0 && ReadWithAdo(connection,"' OR 1=1 --").Count==0 && ReadWithDapper(connection,"' OR 1=1 --").Count==0,
    "all three paths bind hostile-looking input as data");
static List<CourseRow> ReadWithEf(CourseContext db,string title)
{
    var query=db.Courses.AsNoTracking().Where(x=>x.Title==title)
        .Select(x=>new CourseRow(x.Id,x.Title,x.Seats));
    Console.WriteLine(query.ToQueryString());return query.ToList();
}
static List<CourseRow> ReadWithAdo(SqliteConnection connection,string title)
{
    using var command=connection.CreateCommand();
    command.CommandText="SELECT Id,Title,Seats FROM Courses WHERE Title=@title";
    command.Parameters.AddWithValue("@title",title);
    using var reader=command.ExecuteReader();List<CourseRow> result=[];
    while(reader.Read())result.Add(new(reader.GetInt32(0),reader.GetString(1),reader.GetInt32(2)));
    return result;
}
static List<CourseRow> ReadWithDapper(SqliteConnection connection,string title)
    => connection.Query<CourseRow>("SELECT Id,Title,Seats FROM Courses WHERE Title=@title",new {title}).ToList();
public sealed record CourseRow(long Id,string Title,long Seats);
public sealed class Course {public int Id {get;set;}public string Title {get;set;}="";public int Seats {get;set;}}
public sealed class CourseContext(DbContextOptions<CourseContext> options):DbContext(options)
{public DbSet<Course> Courses=>Set<Course>();}
