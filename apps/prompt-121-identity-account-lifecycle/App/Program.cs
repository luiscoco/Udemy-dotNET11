using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
var folder=Path.GetFullPath(Path.Combine("artifacts","identity-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
await using var server=new SecureHarness();var mail=new LocalMail();
await server.StartAsync(b=>
{
    IdentitySetup.Add(b.Services,Path.Combine(folder,"accounts.db"));b.Services.AddSingleton(mail);
    b.Services.AddDataProtection().SetApplicationName("course121").PersistKeysToFileSystem(new DirectoryInfo(folder)).ProtectKeysWithCertificate(server.Certificate);
},app=>
{
    app.UseAuthentication();app.UseAuthorization();AccountEndpoints.Map(app);
    app.MapGet("/",()=>Microsoft.AspNetCore.Http.Results.Content(Page.Html,"text/html"));
});
await IdentitySetup.Initialize(server.App.Services);
using var client=server.CreateClient();await AccountChecks.Run(client,mail,args.FirstOrDefault()??"verify");
if(args.FirstOrDefault()=="browser")await BrowserRunner.RunAsync(server.Address,"identity",server.SpkiPin);
