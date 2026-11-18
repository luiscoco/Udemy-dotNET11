using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
await using var backend = await ApiHarness.StartAsync(_ => {}, app =>
{
    app.MapGet("/courses", () => Results.Ok(new {title="YARP course"}));
    app.MapGet("/slow", async (HttpContext context) =>
    {
        entered.TrySetResult();
        await Task.Delay(TimeSpan.FromSeconds(5),context.RequestAborted);
        return Results.Ok();
    });
});
await using var gateway = await ApiHarness.StartAsync(builder => Gateway.Configure(builder,backend.Client.BaseAddress!.ToString()), Gateway.Map);
using var anonymous = await gateway.Client.GetAsync("/gateway/courses");
Check.That(anonymous.StatusCode == HttpStatusCode.Unauthorized, "the protected route challenges an anonymous request");
if (args.FirstOrDefault() == "anonymous")
    Check.That(anonymous.IsSuccessStatusCode, "incorrect expectation: an anonymous caller can use the protected gateway");
gateway.Client.DefaultRequestHeaders.Add("X-Demo-User","student");
Check.That((await gateway.Client.GetAsync("/gateway/courses")).StatusCode == HttpStatusCode.Forbidden, "an authenticated identity without the required claim is forbidden");
gateway.Client.DefaultRequestHeaders.Remove("X-Demo-User");
gateway.Client.DefaultRequestHeaders.Add("X-Demo-User","reader");
Check.That((await gateway.Client.GetStringAsync("/gateway/courses")).Contains("YARP course"), "the authorized route removes its prefix and forwards to Kestrel");
var slow = gateway.Client.GetAsync("/gateway/slow");
await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
Check.That((await gateway.Client.GetAsync("/gateway/courses")).StatusCode == HttpStatusCode.ServiceUnavailable, "the concurrency policy rejects excess work without queuing");
using var timedOut = await slow;
Check.That(timedOut.StatusCode == HttpStatusCode.GatewayTimeout, "the forwarding activity timeout bounds the slow destination");
Console.WriteLine("service: real YARP forwarding, 401, 403, success, overload and timeout verified");
