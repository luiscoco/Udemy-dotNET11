using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
public sealed class ProxyFixture : IAsyncDisposable
{
    private WebApplication backend=null!,proxy=null!;private X509Certificate2 certificate=null!;private HttpClient upstream=null!;
    public string Backend=>backend.Urls.Single();public string Address=>proxy.Urls.Single();public string Thumbprint=>certificate.Thumbprint;
    public async Task StartAsync(bool wrongTrust)
    {
        var b=WebApplication.CreateBuilder();b.Logging.ClearProviders();b.WebHost.UseUrls("http://127.0.0.1:0");b.Services.AddControllersWithViews();
        b.Services.AddLocalization();b.Services.AddHttpsRedirection(o=>o.HttpsPort=443);
        b.Services.Configure<ForwardedHeadersOptions>(o=>
        {o.ForwardedHeaders=ForwardedHeaders.XForwardedFor|ForwardedHeaders.XForwardedProto;o.ForwardLimit=1;o.KnownProxies.Clear();o.KnownIPNetworks.Clear();o.KnownProxies.Add(IPAddress.Parse(wrongTrust?"127.0.0.3":"127.0.0.2"));});
        backend=b.Build();backend.UseForwardedHeaders();
        backend.Use(async(context,next)=>{context.Response.Headers["X-Observed-Scheme"]=context.Request.Scheme;await next(context);});
        backend.UseHttpsRedirection();backend.UseRequestLocalization(o=>o.SetDefaultCulture("en-US").AddSupportedCultures("en-US","fr-FR").AddSupportedUICultures("en-US","fr-FR"));
        backend.MapControllers();await backend.StartAsync();
        using var rsa=RSA.Create(2048);var request=new CertificateRequest("CN=localhost",rsa,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
        using var created=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5),DateTimeOffset.UtcNow.AddDays(1));certificate=X509CertificateLoader.LoadPkcs12(created.Export(X509ContentType.Pfx),null);
        var handler=new SocketsHttpHandler {AllowAutoRedirect=false,ConnectCallback=async(context,ct)=>
        {
            var socket=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.2"),0));
            try{await socket.ConnectAsync(context.DnsEndPoint,ct);return new NetworkStream(socket,ownsSocket:true);}catch{socket.Dispose();throw;}
        }};upstream=new HttpClient(handler);
        var p=WebApplication.CreateBuilder();p.Logging.ClearProviders();p.WebHost.ConfigureKestrel(o=>o.Listen(IPAddress.Loopback,0,listen=>listen.UseHttps(certificate)));
        proxy=p.Build();proxy.MapGet("/info",async(HttpContext context)=>
        {
            using var message=new HttpRequestMessage(HttpMethod.Get,Backend+context.Request.Path+context.Request.QueryString);
            message.Headers.Add("X-Forwarded-For",context.Connection.RemoteIpAddress!.ToString());message.Headers.Add("X-Forwarded-Proto",context.Request.Scheme);
            using var response=await upstream.SendAsync(message,context.RequestAborted);context.Response.StatusCode=(int)response.StatusCode;
            context.Response.ContentType=response.Content.Headers.ContentType?.ToString();await response.Content.CopyToAsync(context.Response.Body,context.RequestAborted);
        });await proxy.StartAsync();
    }
    public async ValueTask DisposeAsync(){if(proxy is not null){await proxy.StopAsync();await proxy.DisposeAsync();}if(backend is not null){await backend.StopAsync();await backend.DisposeAsync();}upstream?.Dispose();certificate?.Dispose();}
}
