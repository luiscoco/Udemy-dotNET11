using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
if(!OperatingSystem.IsWindows())throw new PlatformNotSupportedException("This isolated User Secrets path fixture uses Windows APPDATA");
var isolated=Path.GetFullPath(Path.Combine("artifacts","secret-home-"+Guid.NewGuid().ToString("N")));
var id="course128-"+Guid.NewGuid().ToString("N");
var path=Path.Combine(isolated,"Microsoft","UserSecrets",id,"secrets.json");Directory.CreateDirectory(Path.GetDirectoryName(path)!);
var synthetic=Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
var original=Environment.GetEnvironmentVariable("APPDATA");
IConfigurationRoot? development=null;
try
{
    Environment.SetEnvironmentVariable("APPDATA",isolated);
    await File.WriteAllTextAsync(path,JsonSerializer.Serialize(new Dictionary<string,string>{{"Mail:ApiKey",synthetic}}));
    development=new ConfigurationBuilder().AddUserSecrets(id,reloadOnChange:false).Build();
    var source=SecretSources.Select("Development",development);
    Check.That(await source.Read("Mail:ApiKey")==synthetic,"actual User Secrets provider supplies the development value without embedding it in source");
    bool blocked=false;
    try{SecretSources.Select("Production",development);}catch(InvalidOperationException){blocked=true;}
    if(args.FirstOrDefault()=="fallback")Check.That(!blocked,"incorrect expectation: production falls back to development secrets");
    Check.That(blocked,"production refuses a missing vault even while development secrets are present");
    var configuration=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Vault:Uri","https://course-fixture.vault.azure.net"}}).Build();
    Check.That(SecretSources.Select("Production",configuration) is VaultSecrets,"production selects the real managed-identity Key Vault adapter");
    Console.WriteLine("service: User Secrets read verified; generated value omitted from logs");
    Console.WriteLine("service: production SDK adapter configured only; no Azure authentication or secret retrieval is claimed");
}
finally
{
    (development as IDisposable)?.Dispose();Environment.SetEnvironmentVariable("APPDATA",original);
    if(File.Exists(path))File.Delete(path);
}
Check.That(!File.Exists(path),"temporary synthetic secrets file is removed after the fixture");
