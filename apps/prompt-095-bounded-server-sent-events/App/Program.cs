using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
EventProbe normal=new(),held=new();
await using var api=await ApiHarness.StartAsync(_=>{},app=>app.MapGet("/events",(bool? hold,CancellationToken ct)=>TypedResults.ServerSentEvents(EventStream.Read(hold==true?held:normal,hold==true,ct))));
using var response=await api.Client.GetAsync("/events",HttpCompletionOption.ResponseHeadersRead);
Check.That(response.Content.Headers.ContentType?.MediaType=="text/event-stream","the endpoint exposes the SSE media type");
using var reader=new StreamReader(await response.Content.ReadAsStreamAsync());var values=new List<int>();var ids=new List<string>();string? line;
while((line=await reader.ReadLineAsync()) is not null)
{
    if(line.StartsWith("id:"))ids.Add(line[3..].Trim());
    if(line.StartsWith("data:")) values.Add(System.Text.Json.JsonSerializer.Deserialize<Notice>(line[5..].Trim(),new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!.Number);
}
Console.WriteLine("network: SSE IDs "+string.Join(",",ids)+"; peak queued "+normal.Peak);
if(args.FirstOrDefault()=="buffer")Check.That(normal.Peak>2,"the bounded channel must not grow beyond its two-item capacity");
Check.That(values.SequenceEqual(Enumerable.Range(1,6)) && normal.Peak==2,"all six events arrive in order through a bounded queue");
using(var waiting=await api.Client.GetAsync("/events?hold=true",HttpCompletionOption.ResponseHeadersRead))
{
    using var stream=await waiting.Content.ReadAsStreamAsync();using var input=new StreamReader(stream);
    while((line=await input.ReadLineAsync()) is not null && !line.StartsWith("data:")){}
    Check.That(line is not null,"the held stream delivered its first event before disconnection");
}
await held.Stopped.Task.WaitAsync(TimeSpan.FromSeconds(10));
Check.That(held.Stopped.Task.IsCompletedSuccessfully,"disconnect cancels the producer and joins its cleanup");
