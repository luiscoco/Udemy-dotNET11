using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
await using var service = await ApiHarness.StartAsync(_ => {}, app =>
{
    app.MapGet("/courses/{id:int}", (int id) => id == 11 ? Results.Ok(new CourseDto(11,"Architecture")) : Results.NotFound());
});
ICourseLookup local = new LocalLookup();
ICourseLookup remote = new HttpLookup(service.Client);
foreach (int id in new[]{11,99})
{
    var before = await local.FindAsync(id, CancellationToken.None);
    var after = await remote.FindAsync(id, CancellationToken.None);
    Check.That(before == after, $"local and extracted HTTP adapters preserve the result for course {id}");
}
await service.App.StopAsync();
var outage = await remote.FindAsync(11, CancellationToken.None);
Check.That(outage.Status == "unavailable", "transport failure stays distinct from a missing course");
if (args.FirstOrDefault() == "collapse")
    Check.That(outage.Status == "missing", "incorrect expectation: a service outage means the course is missing");
using var cancelled = new CancellationTokenSource();
cancelled.Cancel();
bool cancellationPreserved = false;
try { await remote.FindAsync(11, cancelled.Token); }
catch (OperationCanceledException) { cancellationPreserved = true; }
Check.That(cancellationPreserved, "caller cancellation remains observable to the caller");
Console.WriteLine("service: contract parity, not-found semantics, unavailable transport and cancellation verified");
