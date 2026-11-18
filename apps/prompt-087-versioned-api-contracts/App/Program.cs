using Asp.Versioning;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
await using var api=await ApiHarness.StartAsync(b=>b.Services.AddApiVersioning(o=>
{
    o.DefaultApiVersion=new ApiVersion(1,0); o.AssumeDefaultVersionWhenUnspecified=false;
    o.ReportApiVersions=true; o.ApiVersionReader=new UrlSegmentApiVersionReader();
}),VersionedRoutes.Map);
using var v1=await api.Client.GetAsync("/api/v1/products/1");
using var v2=await api.Client.GetAsync("/api/v2/products/1");
string first=await v1.Content.ReadAsStringAsync(),second=await v2.Content.ReadAsStringAsync();
using var one=JsonDocument.Parse(first); using var two=JsonDocument.Parse(second);
Check.That((int)v1.StatusCode==200 && (int)v2.StatusCode==200,"the package routes both explicitly declared API versions");
Check.That(!one.RootElement.TryGetProperty("description",out _) && two.RootElement.GetProperty("description").GetString()=="Optional v2 detail","v1 retains its response shape while v2 adds an optional field");
var oldReader=JsonSerializer.Deserialize<ProductV1>(second,new JsonSerializerOptions(JsonSerializerDefaults.Web));
Check.That(oldReader==new ProductV1(1,"Course book"),"the demonstrated tolerant v1 DTO reader still reads the shared v2 fields");
Console.WriteLine("api: v1 "+first); Console.WriteLine("api: v2 "+second);
using var unsupported=await api.Client.GetAsync("/api/v3/products/1");
Console.WriteLine($"api: unsupported URL version returns {(int)unsupported.StatusCode}");
if(args.FirstOrDefault()=="version") Check.That((int)unsupported.StatusCode==200,"an undeclared URL API version is not a supported contract");
Check.That((int)unsupported.StatusCode==404,"the URL-segment versioning policy rejects the undeclared version");
