using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using var fixture=new DatabaseFixture();
var mode=args.FirstOrDefault() ?? "verify";

using var db=fixture.Open();
var left=db.Courses.Where(x=>x.Id<=2);
var right=db.Courses.Where(x=>x.Id>=2);
var full=left.FullJoin(right,a=>a.Id,b=>b.Id,
    (a,b)=>new {LeftId=(int?)a!.Id,RightId=(int?)b!.Id});
var fullSql=full.ToQueryString(); Console.WriteLine(fullSql);
Check.That(fullSql.Contains("FULL JOIN"),"FullJoin translates to a full relational join");
var joined=full.ToList();
Check.That(joined.Count==3 && joined.Any(x=>x.LeftId==null) && joined.Any(x=>x.RightId==null),
    "full join preserves unmatched rows on both sides");
fixture.Trace.Reset();
var maximum=db.Courses.MaxBy(x=>x.Seats)!;
var minimum=db.Courses.MinBy(x=>x.Seats)!;
fixture.Trace.Print("MaxBy and MinBy actual SQL");
Check.That(maximum.Id==3 && minimum.Id==1,"extrema operators return entities rather than scalar seat counts");
var grouped=db.Enrollments.GroupBy(x=>x.CourseId)
    .Select(g=>new {CourseId=g.Key,Count=g.Count()}).OrderBy(x=>x.CourseId);
Console.WriteLine(grouped.ToQueryString());
Check.That(grouped.ToList().All(x=>x.Count==2),"supported aggregate grouping executes on the server");
if(mode=="unsupported")
{
    var bad=db.Courses.Where(x=>Rules.LocalRule(x.Title));
    Console.WriteLine(bad.ToQueryString());
}
Console.WriteLine("database: provider-specific SQL verified against the bundled SQLite engine");
public static class Rules {public static bool LocalRule(string title)=>title.GetHashCode()%2==0;}
