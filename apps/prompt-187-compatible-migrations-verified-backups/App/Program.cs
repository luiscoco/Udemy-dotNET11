using Microsoft.Data.Sqlite;
Directory.CreateDirectory("artifacts");
string prefix=Path.GetFullPath("artifacts/migration-"+Guid.NewGuid().ToString("N"));
using var store=new CourseStore(prefix+".db"); store.Open();
store.Execute("CREATE TABLE Courses(Id INTEGER PRIMARY KEY,Title TEXT NOT NULL); INSERT INTO Courses VALUES(1,'Data Safety');");
store.BackupTo(prefix+"-backup.db");
using var backup=new CourseStore(prefix+"-backup.db"); backup.Open();
Check.That(backup.Read("PRAGMA integrity_check")=="ok","backup database passes integrity_check");
backup.BackupTo(prefix+"-restored.db");
using var restored=new CourseStore(prefix+"-restored.db"); restored.Open();
Check.That(restored.OldRead(1)=="Data Safety","restored backup preserves the original course");
store.Expand();
Check.That(store.OldRead(1)=="Data Safety"&&store.NewRead(1)=="Data Safety","expanded schema supports both application versions");
store.Backfill();
store.Execute("INSERT INTO Courses VALUES(2,'Legacy Writer',NULL);");
Check.That(store.NewRead(1)=="data-safety"&&store.NewRead(2)=="Legacy Writer","backfill and fallback support coexistence");
store.Execute("INSERT INTO Courses VALUES(3,'New Writer','new-writer');");
Check.That(store.OldRead(3)=="New Writer"&&store.NewRead(3)=="new-writer","dual-written row supports both readers");
bool compatible=true;
if(args.FirstOrDefault()=="problem")
{
    store.Execute("ALTER TABLE Courses DROP COLUMN Title;");
    try { store.OldRead(1); }
    catch(SqliteException error) { compatible=false; Console.WriteLine(error.Message); }
}
Harness.Save("migration-verification.json",new {backup=prefix+"-backup.db",restored=prefix+"-restored.db",backupIntegrity=true,restoreVerified=true,oldAndNewReadersCompatible=compatible,backfillVerified=true});
Check.That(compatible,"DATABASE_COMPATIBILITY_REGRESSION old reader remains supported during the compatibility window");
Console.WriteLine("Verified backup restore, additive expansion, backfill, fallback and dual-write compatibility.");
