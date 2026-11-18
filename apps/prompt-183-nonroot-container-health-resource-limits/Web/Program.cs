using System.Reflection;
if(args.FirstOrDefault()=="--health")
{
    using var probe=new HttpClient {Timeout=TimeSpan.FromSeconds(2)};
    using var response=await probe.GetAsync("http://127.0.0.1:8080/health/ready");
    return response.IsSuccessStatusCode?0:1;
}
var builder=WebApplication.CreateBuilder(args);
var app=builder.Build();
string version=Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
bool fault=Environment.GetEnvironmentVariable("INJECT_FAULT")=="true";
bool Ready()
{
    string? file=Environment.GetEnvironmentVariable("COURSE_TOKEN_FILE");
    return file is null || File.Exists(file)&&File.ReadAllText(file).Trim().Length>=12;
}
app.MapGet("/health/live",()=>Results.Ok());
app.MapGet("/health/ready",()=>Ready()?Results.Ok():Results.StatusCode(503));
app.MapGet("/",()=>fault?Results.StatusCode(503):Results.Json(new {version,course="Deployment"}));
await app.RunAsync();
return 0;
