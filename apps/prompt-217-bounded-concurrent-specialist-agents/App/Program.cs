var workers=new[]
{
    new Specialist("security","You review authorization.",
        ["Verify authorization on every tool","Check tenant isolation"]),
    new Specialist("tests","You review automated tests.",
        ["Run regression tests","Exercise cancellation tests"]),
    new Specialist("operations","You review delivery operations.",
        ["Verify rollback steps","Check runtime health probes"])
};
var pool=new SpecialistPool(); using var deadline=new CancellationTokenSource(TimeSpan.FromMinutes(3));
await pool.Run(workers,args.Contains("problem")?3:2,deadline.Token);
Console.WriteLine($"SPECIALISTS peak={pool.Peak} configuredBudget=2 completed={pool.Results.Count}");
Check.That(pool.Peak<=2,"application agent calls stay inside the concurrency budget","AGENT specialist concurrency exceeded budget");
Check.That(pool.Results.Keys.Order().SequenceEqual(workers.Select(w=>w.Id).Order()),"one result is retained for every stable specialist identifier");
var ordered=pool.Results.OrderBy(pair=>pair.Key).ToArray();
foreach(var result in ordered) Console.WriteLine("SPECIALIST "+result.Key+": "+result.Value);
Check.That(pool.Status.Values.All(s=>s=="completed"),"aggregation follows explicit successful completion");
AgentRuntime.Save(new { passed=true,peak=pool.Peak,configuredBudget=2,results=ordered,status=pool.Status,independentSessions=true });
