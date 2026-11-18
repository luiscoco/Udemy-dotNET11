using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using var fixture=new DatabaseFixture();
var mode=args.FirstOrDefault() ?? "verify";

var services=new ServiceCollection();
services.AddDbContext<SchoolContext>(o=>o.UseSqlite(
    fixture.ConnectionString));
using var provider=services.BuildServiceProvider(new ServiceProviderOptions {ValidateScopes=true});
if(mode=="scope")
{
    provider.GetRequiredService<SchoolContext>();
    throw new Exception("Root resolution unexpectedly succeeded");
}
using var scope=provider.CreateScope();
var db=scope.ServiceProvider.GetRequiredService<SchoolContext>();
Check.That(ReferenceEquals(db,scope.ServiceProvider.GetRequiredService<SchoolContext>()),
    "one scope receives the same context instance");
using var other=provider.CreateScope();
Check.That(!ReferenceEquals(db,other.ServiceProvider.GetRequiredService<SchoolContext>()),
    "a different scope owns a different context");
var query=db.Courses.Select(x=>new {x.Title,x.Seats}).OrderBy(x=>x.Title);
Console.WriteLine(query.ToQueryString());
Check.That(query.Count()==3,"configured model reads three persisted courses");
Check.That(db.Model.FindEntityType(typeof(Course))!.FindProperty("Title")!.GetMaxLength()==120,
    "explicit configuration contributes title metadata");
Console.WriteLine("database: SQLite does not enforce varchar length; validate input separately");

