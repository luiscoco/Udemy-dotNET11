using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
await DeploymentTools.Dotnet("build","Legacy","-c","Release");
var unsupported=await Harness.Run(Harness.Dotnet,"build","Unsupported","-c","Release");
File.WriteAllText("artifacts/unsupported-dependency.txt",unsupported.Text);
Check.That(unsupported.Code!=0&&unsupported.Text.Contains("CS0234"),"actual unsupported System.Web dependency is identified");
var legacy=new LegacyAdapter(Path.GetFullPath("Legacy/bin/Release/net48/Legacy.exe"));
string expected=await legacy.Invoice("Cloud & Data");
bool premature=args.FirstOrDefault()=="problem";
await using var gateway=await ApiHarness.StartAsync(_=>{},app=>
{
    app.MapGet("/invoice",()=>Results.Json(new {Title=WebUtility.HtmlEncode("Cloud & Data"),Total=75m},new JsonSerializerOptions {PropertyNamingPolicy=null}));
    app.MapGet("/reports",async ()=>premature?Results.StatusCode(501):Results.Text(await legacy.Report()));
});
string actual=await gateway.Client.GetStringAsync("/invoice");
using var oldContract=JsonDocument.Parse(expected);
using var newContract=JsonDocument.Parse(actual);
Check.That(oldContract.RootElement.GetProperty("Title").GetString()==newContract.RootElement.GetProperty("Title").GetString(),"modern HTML encoding preserves the legacy title contract");
Check.That(oldContract.RootElement.GetProperty("Total").GetDecimal()==newContract.RootElement.GetProperty("Total").GetDecimal(),"modern invoice preserves the legacy amount");
using var report=await gateway.Client.GetAsync("/reports");
Console.WriteLine("Modern invoice="+actual);
Console.WriteLine("Legacy report status="+(int)report.StatusCode);
Harness.Save("modernization-verification.json",new {legacyFrameworkExecuted=true,unsupportedDependencyDetected=true,invoiceContractPreserved=true,legacyReportStatus=(int)report.StatusCode});
Check.That(report.IsSuccessStatusCode&&await report.Content.ReadAsStringAsync()=="Legacy report integration","LEGACY_CUTOVER_REGRESSION unmigrated integration remains available");
