using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using var fixture=new DatabaseFixture();
var mode=args.FirstOrDefault() ?? "verify";

using var db=fixture.Open();
var malicious="Course 1' OR 1=1 --";
fixture.Trace.Reset();
var safe=db.Courses.FromSql($"SELECT * FROM Courses WHERE Title = {malicious}").ToList();
Check.That(safe.Count==0,"interpolated FromSql binds hostile input as a parameter");
fixture.Trace.Print("parameterized SQL template");
Check.That(fixture.Trace.Commands.Single().Contains("@p0"),"generated command contains a parameter placeholder");
if(mode=="unsafe")
{
    var unsafeSql="SELECT * FROM Courses WHERE Title = '"+malicious+"'";
    var leaked=db.Courses.FromSqlRaw(unsafeSql).ToList();
    Console.WriteLine($"database: unsafe string concatenation returned {leaked.Count} rows");
    Check.That(leaked.Count==0,"incorrect expectation: concatenation treats input only as data");
}
string requestedSort="seats";
var column=requestedSort switch {"title"=>"Title","seats"=>"Seats",_=>throw new ArgumentException("Sort not allowed")};
var ordered=db.Courses.FromSqlRaw($"SELECT * FROM Courses ORDER BY [{column}] DESC").ToList();
Check.That(ordered.First().Id==3,"allowlisted identifier selects the intended sort column");
var id=1; var seats=7;
var changed=db.Database.ExecuteSql($"UPDATE Courses SET Seats={seats} WHERE Id={id}");
db.ChangeTracker.Clear();
Check.That(changed==1 && db.Courses.AsNoTracking().Single(x=>x.Id==1).Seats==7,
    "parameterized SQL command updates only the selected row");
Check.That(db.Enrollments.Count()==6,"SQL experiment preserves the unrelated table");
Console.WriteLine("database: use LINQ first; inspect SQL when a supported provider feature requires an escape hatch");
