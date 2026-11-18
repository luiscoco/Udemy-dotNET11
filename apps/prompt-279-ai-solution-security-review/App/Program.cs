bool broken = args.Contains("problem");
var store = new Store("review");
store.Execute("CREATE TABLE Records(Tenant TEXT,Title TEXT,Secret TEXT); INSERT INTO Records VALUES('alpha','invoice','alpha-only'),('beta','invoice','beta-private')");
var findings = Review.Analyze(File.ReadAllText("App/Candidate.cs"));
Check.That(findings.Count == 1 && findings[0].Rule == "SQL001", "actual C# syntax review locates SQL concatenation with a source line");
var repository = new ReviewedRepository(store);
const string attack = "' OR 1=1 --";
var insecure = Candidate.Search(store, "alpha", attack);
Check.That(insecure.Contains("beta-private"), "dynamic attack confirms the candidate leaks another tenant record");
var selected = broken ? Candidate.Search(store, "alpha", attack) : await repository.Search("alpha", attack);
Console.WriteLine("REVIEW findings=" + findings.Count + " candidateRows=" + insecure.Count + " selectedRows=" + selected.Count);
Harness.Save("review-findings.json", new { findings, actualSyntaxAnalysis = true, dynamicCandidateLeak = insecure.Count, automatedScope = "bounded SQL-concatenation rule plus executable tenant and cancellation tests; not a complete security audit" });
Check.That(selected.Count == 0, "repaired parameterized query treats injected syntax as data", "CAPSTONE reviewed query tenant boundary failed");
Check.That((await repository.Search("alpha", "invoice")).SequenceEqual(new[] { "alpha-only" }), "normal tenant-scoped query preserves its expected result");
Check.That((await repository.Search("alpha", "missing")).Count == 0, "missing records return a controlled empty result");
using var cancelled = new CancellationTokenSource();
cancelled.Cancel();
bool observed = false;
try
{
    await repository.Search("alpha", "invoice", cancelled.Token);
}
catch (OperationCanceledException)
{
    observed = true;
}

Check.That(observed, "actual async repository observes cancellation");
Harness.Save("capstone-evidence.json", new { passed = true, actualRoslynReview = true, findings, actualCandidateLeak = true, repairedInjectionRows = selected.Count, tenantIsolation = true, missingHandled = true, cancellationObserved = observed, compatibility = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription });
