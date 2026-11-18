using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using var fixture=new DatabaseFixture();
var mode=args.FirstOrDefault() ?? "verify";

using(var db=fixture.Open())
{
    using(var transaction=db.Database.BeginTransaction())
    {
        db.Courses.Where(x=>x.Id==1).ExecuteUpdate(s=>s.SetProperty(x=>x.Seats,0));
        transaction.Rollback();
    }
    Check.That(db.Courses.Single(x=>x.Id==1).Seats==10,"rollback restores the original seats");
}
using var first=fixture.Open(); using var second=fixture.Open();
var winner=first.Courses.Single(x=>x.Id==1);
var stale=second.Courses.Single(x=>x.Id==1);
winner.Seats=9; winner.Revision++; first.SaveChanges();
stale.Seats=8; stale.Revision++;
try
{
    second.SaveChanges();
    throw new Exception("Concurrency conflict was not raised");
}
catch(DbUpdateConcurrencyException conflict)
{
    Console.WriteLine("database: optimistic conflict detected for stale revision 1");
    if(mode=="conflict") throw new InvalidOperationException("Unresolved optimistic concurrency conflict",conflict);
    var entry=conflict.Entries.Single();
    var database=entry.GetDatabaseValues()!;
    entry.CurrentValues.SetValues(database);
    entry.OriginalValues.SetValues(database);
    var fresh=(Course)entry.Entity;
    fresh.Seats--; fresh.Revision++; second.SaveChanges();
}
using var verify=fixture.Open();
var final=verify.Courses.AsNoTracking().Single(x=>x.Id==1);
Check.That(final.Seats==8 && final.Revision==3,"reload and reapply business operation preserves both reservations");
Check.That(verify.Enrollments.Count()==6,"unrelated enrollments remain unchanged");
fixture.Trace.Print("concurrency predicate and repaired update");
