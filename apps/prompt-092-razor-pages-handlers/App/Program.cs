using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
await using var api=await ApiHarness.StartAsync(b=>b.Services.AddRazorPages(),app=>app.MapRazorPages());
if(args.FirstOrDefault()=="browser") { await BrowserRunner.RunAsync(api.Client.BaseAddress!.ToString(),"pages"); return; }
await FormChecks.VerifyAsync(api.Client.BaseAddress!,true,args.FirstOrDefault());
