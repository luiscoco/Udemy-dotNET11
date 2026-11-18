using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
await using var api=await ApiHarness.StartAsync(b=>b.Services.AddControllersWithViews(),app=>app.MapControllers());
if(args.FirstOrDefault()=="browser") { await BrowserRunner.RunAsync(api.Client.BaseAddress!.ToString(),"mvc"); return; }
await FormChecks.VerifyAsync(api.Client.BaseAddress!,false,args.FirstOrDefault());
