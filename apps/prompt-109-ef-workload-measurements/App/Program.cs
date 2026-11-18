using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using var fixture=new DatabaseFixture();
var mode=args.FirstOrDefault() ?? "verify";

using var db=fixture.Open();
var compiled=EF.CompileQuery((SchoolContext context,int id)=>
    context.Courses.AsNoTracking().Where(x=>x.Id==id).Select(x=>x.Seats).Single());
_ = compiled(db,1); _ = db.Courses.AsNoTracking().Where(x=>x.Id==1).Select(x=>x.Seats).Single();
long Measure(Action action)
{
    var clock=Stopwatch.StartNew(); for(int i=0;i<100;i++)action();
    return clock.ElapsedTicks;
}
var normal=Measure(()=>_ = db.Courses.AsNoTracking().Where(x=>x.Id==1).Select(x=>x.Seats).Single());
var cached=Measure(()=>_ = compiled(db,1));
Console.WriteLine($"database: 100 warm queries; normal ticks={normal}; compiled ticks={cached}");
Check.That(compiled(db,1)==10,"compiled query returns the same scalar result");
fixture.Trace.Reset();
var tracked=db.Courses.OrderBy(x=>x.Id).ToList(); fixture.Trace.Reset();
foreach(var course in tracked)course.Seats++;
db.SaveChanges(); var trackedCommands=fixture.Trace.Commands.Count;
Console.WriteLine($"database: SaveChanges updated 3 rows using {trackedCommands} commands on SQLite");
fixture.Trace.Reset();
var changed=db.Courses.ExecuteUpdate(s=>s.SetProperty(x=>x.Seats,x=>x.Seats+1));
Check.That(changed==3 && fixture.Trace.Commands.Count==1,"ExecuteUpdate modifies three rows with one command");
if(mode=="tracker") Check.That(tracked[0].Seats==12,"incorrect expectation: bulk update refreshes tracked entities");
Check.That(tracked[0].Seats==11,"bulk update leaves tracked state stale");
db.ChangeTracker.Clear();
Check.That(db.Courses.Single(x=>x.Id==1).Seats==12,"fresh query observes both updates");
var removed=db.Enrollments.Where(x=>x.Student.StartsWith("Ada")).ExecuteDelete();
Check.That(removed==3 && db.Enrollments.Count()==3,"ExecuteDelete removes the filtered rows immediately");
Console.WriteLine("database: measurements are local observations; batching varies by provider");
