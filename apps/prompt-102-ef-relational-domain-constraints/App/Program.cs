using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using var fixture=new DatabaseFixture();
var mode=args.FirstOrDefault() ?? "verify";

using var db=fixture.Open();
void Reject(Action<SchoolContext> mutation,string label)
{
    using var attempt=fixture.Open(); mutation(attempt);
    bool rejected=false;
    try {attempt.SaveChanges();} catch(DbUpdateException){rejected=true;}
    Check.That(rejected,label);
}
Reject(x=>x.Courses.Add(new(){Title="Course 1",Seats=1}),"unique title index rejects duplicate");
Reject(x=>x.Enrollments.Add(new(){CourseId=999,Student="Unknown"}),"foreign key rejects orphan");
Reject(x=>x.Courses.Add(new(){Title="Negative",Seats=-1}),"check constraint rejects negative seats");
Reject(x=>x.Enrollments.Add(new(){CourseId=1,Student="Ada 1"}),"composite unique enrollment index rejects duplicate");
using(var attempt=fixture.Open())
{
    attempt.Courses.Remove(attempt.Courses.Single(x=>x.Id==1));
    bool blocked=false;
    try{attempt.SaveChanges();}catch(DbUpdateException){blocked=true;}
    if(mode=="delete") Check.That(!blocked,"incorrect expectation: parent deletion cascades");
    Check.That(blocked,"Restrict preserves courses with existing enrollment rows");
}
using(var transaction=db.Database.BeginTransaction())
{
    db.Enrollments.Where(x=>x.CourseId==1).ExecuteDelete();
    db.Courses.Where(x=>x.Id==1).ExecuteDelete(); transaction.Commit();
}
Check.That(db.Courses.Count()==2 && db.Enrollments.Count()==4,
    "explicit child-first deletion commits the intended aggregate removal");
Console.WriteLine("database: unique, foreign-key, check and restrict constraints verified");
