using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
public sealed class SecureHarness : IAsyncDisposable
{
    public WebApplication App {get;private set;}=null!;
    public X509Certificate2 Certificate {get;}
    public string Address=>App.Urls.Single();
    public string SpkiPin=>Convert.ToBase64String(SHA256.HashData(Certificate.GetRSAPublicKey()!.ExportSubjectPublicKeyInfo()));
    public SecureHarness()
    {
        using var rsa=RSA.Create(2048);
        var request=new CertificateRequest("CN=localhost",rsa,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
        var names=new SubjectAlternativeNameBuilder();names.AddIpAddress(IPAddress.Loopback);names.AddDnsName("localhost");request.CertificateExtensions.Add(names.Build());
        using var created=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-2),DateTimeOffset.UtcNow.AddDays(1));
        Certificate=X509CertificateLoader.LoadPkcs12(created.Export(X509ContentType.Pfx),null,X509KeyStorageFlags.Exportable);
    }
    public async Task StartAsync(Action<WebApplicationBuilder> configure,Action<WebApplication> map,string environment="Production",int port=0)
    {
        var builder=WebApplication.CreateBuilder(new WebApplicationOptions{EnvironmentName=environment,ApplicationName=typeof(SecureHarness).Assembly.GetName().Name});
        builder.Logging.ClearProviders();builder.WebHost.ConfigureKestrel(o=>o.Listen(IPAddress.Loopback,port,l=>l.UseHttps(Certificate)));
        configure(builder);App=builder.Build();map(App);await App.StartAsync();
    }
    public HttpClient CreateClient(bool redirects=false)
    {
        var handler=new HttpClientHandler {AllowAutoRedirect=redirects,CookieContainer=new CookieContainer(),ServerCertificateCustomValidationCallback=(_,cert,_,_)=>cert?.Thumbprint==Certificate.Thumbprint};
        return new HttpClient(handler){BaseAddress=new Uri(Address),Timeout=TimeSpan.FromSeconds(20)};
    }
    public async ValueTask DisposeAsync(){if(App is not null){await App.StopAsync();await App.DisposeAsync();}Certificate.Dispose();}
}
