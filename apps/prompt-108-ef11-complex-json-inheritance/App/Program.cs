using Microsoft.EntityFrameworkCore;
using var db=new CatalogContext();
db.Database.OpenConnection(); db.Database.EnsureCreated();
db.Add(new VideoCourse {Name="Data access",Duration=60,
    Details=new(){Level="Beginner",Language="English"}});
db.SaveChanges(); db.ChangeTracker.Clear();
var query=db.Products.OfType<VideoCourse>().Where(x=>x.Details.Level=="Beginner");
Console.WriteLine(query.ToQueryString());
var course=query.Single();
Check.That(course.Duration==60 && course.Details.Language=="English",
    "TPT derived entity round-trips its complex JSON value");
course.Details.Level="Intermediate"; db.SaveChanges(); db.ChangeTracker.Clear();
if(args.FirstOrDefault()=="stale") Check.That(query.Any(),"incorrect expectation: old JSON filter still matches");
Check.That(!query.Any() && db.Products.Single().Details.Level=="Intermediate",
    "updating the complex value changes the server JSON filter result");
Check.That(db.Model.FindEntityType(typeof(Product))!.FindComplexProperty("Details")!=null,
    "Details is a complex property with no independent entity identity");
Console.WriteLine("database: TPT base/derived tables and JSON property queries verified");
