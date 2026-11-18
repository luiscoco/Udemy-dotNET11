using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
await using var fixture=new SqlServerFixture();await fixture.StartAsync();
await using var db=new SearchContext(new DbContextOptionsBuilder<SearchContext>().UseSqlServer(fixture.ConnectionString).Options);
await using var deployment=new DeploymentContext(new DbContextOptionsBuilder<DeploymentContext>().UseSqlServer(fixture.ConnectionString).Options);
Check.That(!deployment.Database.HasPendingModelChanges(),"deployment snapshot matches the stable schema model");
var script=deployment.GetService<IMigrator>().GenerateScript();
Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/fulltext-deployment.sql",script);
Check.That(script.Contains("CREATE FULLTEXT CATALOG") && script.Contains("CREATE FULLTEXT INDEX"),
    "reviewed migration script contains the full-text catalog and index DDL");
await deployment.Database.MigrateAsync();
db.Articles.AddRange(new(){Title="Database foundations",Contents="Database design database queries and database transactions."},
    new(){Title="Data access",Contents="A database tutorial for students."},
    new(){Title="Gardening",Contents="Flowers and garden planning."});
await db.SaveChangesAsync();
if(args.FirstOrDefault()=="index")
{
    await db.Database.ExecuteSqlRawAsync("DROP FULLTEXT INDEX ON Articles");
    _=await db.Articles.Where(x=>EF.Functions.FreeText(x.Contents,"database")).ToListAsync();
    throw new Exception("Missing index was not detected");
}
var ranked=db.Articles.Join(db.Articles.FreeTextTable<Article,int>("database",x=>x.Contents),
    article=>article.Id,match=>match.Key,(article,match)=>new {article.Id,article.Title,match.Rank})
    .OrderByDescending(x=>x.Rank).ThenBy(x=>x.Id)
    .Select(x=>new SearchRow(x.Id,x.Title,x.Rank));
Console.WriteLine(ranked.ToQueryString());
List<SearchRow> rows=[];
var deadline=DateTime.UtcNow.AddSeconds(30);
do {rows=await ranked.ToListAsync();if(rows.Count==2)break;await Task.Delay(100);}while(DateTime.UtcNow<deadline);
foreach(var row in rows)Console.WriteLine($"database: ranked search: {row.Id} | rank={row.Rank} | {row.Title}");
Check.That(rows.Count==2 && rows.All(x=>x.Rank>0),"populated full-text index returns only the two matching articles with positive rank");
Check.That(rows.Select(x=>x.Rank).SequenceEqual(rows.Select(x=>x.Rank).OrderDescending()),"ranked results are ordered explicitly");
Check.That(!rows.Any(x=>x.Id==3),"unrelated gardening article is excluded");
Console.WriteLine("database: full-text population is asynchronous; relevance ranks are query-specific, not probabilities");
public sealed record SearchRow(int Id,string Title,int Rank);

