using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
await using var api=await ApiHarness.StartAsync(b=>
{
    b.Services.ConfigureHttpJsonOptions(o=>o.SerializerOptions.NumberHandling=System.Text.Json.Serialization.JsonNumberHandling.Strict);
    b.Services.AddOpenApi(o=>o.OpenApiVersion=Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_2);
},OpenApiRoutes.Map);
string document=await api.Client.GetStringAsync("/openapi/v1.json");
Directory.CreateDirectory("artifacts"); await File.WriteAllTextAsync("artifacts/openapi-v1.json",document);
using var json=JsonDocument.Parse(document); string version=json.RootElement.GetProperty("openapi").GetString()!;
string[] cases=SchemaChecks.RequestCaseTypes(json.RootElement);
Console.WriteLine($"api: OpenAPI {version}; request anyOf: "+string.Join(", ",cases));
Console.WriteLine("api: generated Message schema "+json.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("Message").GetRawText());
Check.That(version=="3.2.0" && cases.SequenceEqual(new[]{"integer","string"}),"the generated 3.2 document describes both native union request cases");
foreach(string body in new[]{"42","\"hello\""})
{
    using var content=new StringContent(body,System.Text.Encoding.UTF8,"application/json");
    using var response=await api.Client.PostAsync("/messages",content); string result=await response.Content.ReadAsStringAsync();
    Console.WriteLine($"api: request {body}; HTTP {(int)response.StatusCode}; response {result}");
    Check.That((int)response.StatusCode==200 && result==body,"the runtime accepts and echoes the documented JSON token case");
}
using var invalid=await api.Client.PostAsJsonAsync("/messages",true);
if(args.FirstOrDefault()=="unsupported") Check.That((int)invalid.StatusCode==200,"an undeclared boolean token is outside the union request contract");
Check.That((int)invalid.StatusCode==400,"runtime binding rejects an unsupported JSON shape");
