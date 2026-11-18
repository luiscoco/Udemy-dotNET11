using AGUI.Client;
using Microsoft.Extensions.AI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../..")));
var builder=WebApplication.CreateBuilder(new WebApplicationOptions{ContentRootPath=Directory.GetCurrentDirectory(),WebRootPath=Path.GetFullPath("App/wwwroot")});builder.WebHost.UseUrls("http://127.0.0.1:0").UseStaticWebAssets();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();builder.Services.AddSingleton<AgentEndpoint>();
builder.Services.AddScoped<IChatClient>(sp=>new AGUIChatClient(new AGUIChatClientOptions(new HttpClient{BaseAddress=new Uri(sp.GetRequiredService<AgentEndpoint>().Address)},"/agent")));
await using var app=builder.Build();app.UseAntiforgery();
var stream=new FixtureStream(args.Contains("problem"));app.MapPost("/agent",stream.Respond);app.MapStaticAssets();app.MapRazorComponents<global::App.App>().AddInteractiveServerRenderMode();
await app.StartAsync();app.Services.GetRequiredService<AgentEndpoint>().Address=Endpoint.Address(app.Services);
try{
    if(args.Contains("--serve")){await app.WaitForShutdownAsync();return;}
    var browser=await Harness.Run("node","browser-test.cjs",Endpoint.Address(app.Services));Console.WriteLine(browser.Text);
    Check.That(browser.Code==0,"actual Chrome rendered the Blazor application");
    var result=JsonDocument.Parse(browser.Text.Split(Environment.NewLine,StringSplitOptions.RemoveEmptyEntries).Last()).RootElement;
    Console.WriteLine($"AGUI actual SSE events={stream.Events} toolCalls={stream.ToolCalls} completed={result.GetProperty("completed")}");
    Check.That(result.GetProperty("completed").GetBoolean()&&stream.ToolCalls>0,"the real browser received the completed stream and tool interaction","PREVIEW AGUI stream did not complete successfully");
    Harness.Save("preview-evidence.json",new{passed=true,actualBlazorComponents=true,actualAguiClient=true,actualSse=true,stream.Events,stream.ToolCalls,browser=result.Clone(),model="explicit deterministic protocol fixture, no LLM claim"});
}finally{await app.StopAsync();}
