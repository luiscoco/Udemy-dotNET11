using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
Directory.CreateDirectory("artifacts");
var file=Path.GetFullPath(Path.Combine("artifacts",Guid.NewGuid()+".db"));
try
{
    using var db=new DeploymentContext(new DbContextOptionsBuilder<DeploymentContext>()
        .UseSqlite($"Data Source={file};Pooling=False").Options);
    var migrator=db.GetService<IMigrator>();
    var script=migrator.GenerateScript();
    File.WriteAllText("artifacts/reviewed-deployment.sql",script);
    Console.WriteLine(script);
    Check.That(script.Contains("CREATE TABLE") && script.Contains("ADD"),
        "generated deployment script includes initial table and additive column");
    migrator.Migrate("20261113000100_Initial");
    db.Database.ExecuteSqlRaw("INSERT INTO Courses (Title) VALUES ('Existing course')");
    migrator.Migrate();
    Check.That(db.Courses.Single().Seats==0,"additive migration preserves existing row with a default");
    var applied=db.Database.GetAppliedMigrations().ToArray();
    Check.That(applied.Length==2,"migration history records both applied migrations");
    if(args.FirstOrDefault()=="destructive")
        throw new InvalidOperationException("Deployment review blocked: removing Seats loses saved seat values");
    db.Courses.Single().Seats=12; db.SaveChanges();
    var backup=file+".backup";
    System.IO.File.Copy(file,backup);
    migrator.Migrate("20261113000100_Initial");
    migrator.Migrate(); db.ChangeTracker.Clear();
    Check.That(db.Courses.Single().Seats==0,"schema rollback cannot reconstruct dropped seat values");
    db.Database.CloseConnection();
    System.IO.File.Copy(backup,file,true);
    db.ChangeTracker.Clear();
    Check.That(db.Courses.Single().Seats==12,"restoring rehearsed backup recovers original seat values");
    System.IO.File.Delete(backup);
    Console.WriteLine("database: isolated rehearsal complete; production needs reviewed provider-specific scripts and backup policy");
}
finally {if(File.Exists(file))File.Delete(file);}


