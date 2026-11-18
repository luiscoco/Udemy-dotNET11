using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
await using var api=await ApiHarness.StartAsync(b=>b.Services.AddProblemDetails(o=>o.CustomizeProblemDetails=context=>
    context.ProblemDetails.Extensions["requestId"]=context.HttpContext.TraceIdentifier),ProblemApi.Map);
using var invalid=await api.Client.PostAsJsonAsync("/orders",new OrderInput(0));
using var missing=await api.Client.GetAsync("/orders/99");
using var unexpected=await api.Client.GetAsync("/explode");
foreach(var (response,expected) in new[]{(invalid,400),(missing,404),(unexpected,500)})
{
    string text=await response.Content.ReadAsStringAsync(); using var json=JsonDocument.Parse(text);
    Check.That((int)response.StatusCode==expected && response.Content.Headers.ContentType?.MediaType=="application/problem+json",$"HTTP {expected} uses the consistent problem media type");
    Check.That(json.RootElement.GetProperty("status").GetInt32()==expected && json.RootElement.TryGetProperty("title",out _),$"HTTP {expected} includes its status and title");
    Console.WriteLine($"api: HTTP {expected}: "+text);
    if(expected==500)
    {
        if(args.FirstOrDefault()=="leak") Check.That(text.Contains("INTERNAL-FIXTURE-DETAIL"),"unexpected error details must remain outside the public problem body");
        Check.That(!text.Contains("INTERNAL-FIXTURE-DETAIL") && json.RootElement.TryGetProperty("requestId",out _),"the unexpected problem includes a request identifier without internal exception details");
    }
}
using var valid=await api.Client.PostAsJsonAsync("/orders",new OrderInput(2));
Check.That((int)valid.StatusCode==200,"valid input remains a normal successful response");
