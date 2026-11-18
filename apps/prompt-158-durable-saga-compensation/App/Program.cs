using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
var reservations = new HashSet<string>();
var releases = new HashSet<string>();
int seats = 1;
object gate = new();
await using var inventory = await ApiHarness.StartAsync(_ => {}, app =>
{
    app.MapPost("/reserve/{id}", (string id) =>
    {
        lock (gate)
        {
            if (reservations.Contains(id)) return Results.Ok();
            if (seats == 0) return Results.Conflict();
            reservations.Add(id);
            seats--;
            return Results.Ok();
        }
    });
    app.MapPost("/release/{id}", (string id) =>
    {
        lock (gate)
        {
            if (!reservations.Contains(id)) return Results.NotFound();
            if (releases.Add(id)) seats++;
            return Results.Ok();
        }
    });
});
await using var payments = await ApiHarness.StartAsync(_ => {}, app =>
    app.MapPost("/charge/{id}", () => Results.UnprocessableEntity()));
Directory.CreateDirectory("artifacts");
var file = Path.Combine("artifacts", "saga-" + Guid.NewGuid() + ".db");
using (var journal = new SagaJournal(file))
{
    try { await new SagaCoordinator(journal,inventory.Client,payments.Client).RunAsync(true); }
    catch (InvalidOperationException exception) { Console.WriteLine("service: " + exception.Message); }
    Check.That(journal.State == "Started" && seats == 0, "the interruption leaves a durable state and a real remote reservation");
    if (args.FirstOrDefault() == "atomic")
        Check.That(seats == 1, "incorrect expectation: local rollback automatically compensates a remote reservation");
}
using (var reopened = new SagaJournal(file))
{
    var coordinator = new SagaCoordinator(reopened,inventory.Client,payments.Client);
    await coordinator.RunAsync();
    await coordinator.RunAsync();
    await inventory.Client.PostAsync("/release/enroll-11",null);
    Check.That(reopened.State == "Compensated", "reopening the durable saga resumes and records compensation");
    Check.That(seats == 1 && reservations.Count == 1 && releases.Count == 1, "idempotent reservation and release prevent duplicate side effects");
}
Console.WriteLine("service: SQLite journal, HTTP participants, interrupted coordinator and explicit compensation verified");
