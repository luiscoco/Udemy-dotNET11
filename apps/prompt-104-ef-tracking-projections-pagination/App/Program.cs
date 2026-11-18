using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using var fixture=new DatabaseFixture();
var mode=args.FirstOrDefault() ?? "verify";

using(var tracked=fixture.Open())
{
    var first=tracked.Courses.Single(x=>x.Id==1);
    Check.That(ReferenceEquals(first,tracked.Courses.Single(x=>x.Id==1)),"tracking resolves identity");
    first.Seats=42; tracked.SaveChanges();
}
using(var detached=fixture.Open())
{
    var row=detached.Courses.AsNoTracking().Single(x=>x.Id==1);
    row.Seats=99; detached.SaveChanges();
    if(mode=="detached") Check.That(detached.Courses.AsNoTracking().Single(x=>x.Id==1).Seats==99,
        "incorrect expectation: detached mutation is persisted");
    Check.That(detached.ChangeTracker.Entries().Count()==0,"no-tracking query creates no tracked entity");
}
using var db=fixture.Open();
var query=db.Courses.AsNoTracking().OrderBy(x=>x.Seats).ThenBy(x=>x.Id)
    .Skip(1).Take(2).Select(x=>new CourseRow(x.Id,x.Title,x.Seats));
Console.WriteLine(query.ToQueryString());
var page=query.ToList();
Check.That(page.Count==2 && page.Select(x=>x.Id).SequenceEqual(new[]{3,1}),
    "stable ordered projection selects the intended second page");
Check.That(db.ChangeTracker.Entries().Count()==0,"scalar DTO projection leaves the tracker empty");
Check.That(db.Courses.AsNoTracking().Single(x=>x.Id==1).Seats==42,
    "tracked change persisted and detached change did not");
public sealed record CourseRow(int Id,string Title,int Seats);
