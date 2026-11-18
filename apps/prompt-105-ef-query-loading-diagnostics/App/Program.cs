using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using var fixture=new DatabaseFixture();
var mode=args.FirstOrDefault() ?? "verify";

using var db=fixture.Open();
fixture.Trace.Reset();
var courses=db.Courses.OrderBy(x=>x.Id).ToList();
foreach(var course in courses) db.Entry(course).Collection(x=>x.Enrollments).Load();
var nPlusOne=fixture.Trace.Commands.Count;
fixture.Trace.Print("explicit per-parent loading");
Check.That(nPlusOne==4,"three parents cause one root plus three explicit collection queries");
if(mode=="nplus") Check.That(nPlusOne==1,"incorrect expectation: explicit loop is one query");
db.ChangeTracker.Clear(); fixture.Trace.Reset();
var eager=db.Courses.Include(x=>x.Enrollments).AsSingleQuery().OrderBy(x=>x.Id).ToList();
fixture.Trace.Print("eager single query");
Check.That(fixture.Trace.Commands.Count==1 && eager.Sum(x=>x.Enrollments.Count)==6,
    "eager join loads the same graph in one command");
db.ChangeTracker.Clear(); fixture.Trace.Reset();
var split=db.Courses.Include(x=>x.Enrollments).AsSplitQuery().OrderBy(x=>x.Id).ToList();
fixture.Trace.Print("split collection query");
Check.That(fixture.Trace.Commands.Count==2 && split.Sum(x=>x.Enrollments.Count)==6,
    "split query uses two commands for this one-collection graph");
db.ChangeTracker.Clear(); fixture.Trace.Reset();
var single=db.Courses.Single(x=>x.Id==1);
db.Entry(single).Collection(x=>x.Enrollments).Load();
Check.That(fixture.Trace.Commands.Count==2 && single.Enrollments.Count==2,
    "explicit loading is useful when one selected collection is actually needed");
Console.WriteLine("database: command count is measured, not a universal latency benchmark");
