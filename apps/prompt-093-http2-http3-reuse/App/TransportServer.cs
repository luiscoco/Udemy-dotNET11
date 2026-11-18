using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
public sealed record Observation(string Protocol,string ConnectionId);
public sealed class TransportServer : IAsyncDisposable
{
    private readonly RSA key=RSA.Create(2048); private readonly X509Certificate2 certificate; private WebApplication app=null!;
    public string Address {get;private set;}=""; public string Thumbprint=>certificate.Thumbprint;
    public TransportServer()
    {
        var request=new CertificateRequest("CN=localhost",key,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
        var names=new SubjectAlternativeNameBuilder(); names.AddDnsName("localhost");names.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(names.Build());
        using var created=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5),DateTimeOffset.UtcNow.AddDays(1));
        certificate=X509CertificateLoader.LoadPkcs12(created.Export(X509ContentType.Pfx),null);
    }
    public async Task StartAsync()
    {
        var builder=WebApplication.CreateBuilder();builder.Logging.ClearProviders();
        int port=0;
        for(int attempt=0;attempt<20 && port==0;attempt++)
        {
            var reservation=new System.Net.Sockets.TcpListener(IPAddress.Loopback,0);reservation.Start();
            int candidate=((IPEndPoint)reservation.LocalEndpoint).Port;
            using var udp=new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,System.Net.Sockets.SocketType.Dgram,System.Net.Sockets.ProtocolType.Udp){ExclusiveAddressUse=true};
            try {udp.Bind(new IPEndPoint(IPAddress.Loopback,candidate));port=candidate;}
            catch(System.Net.Sockets.SocketException) { }
            finally {reservation.Stop();}
        }
        if(port==0)throw new IOException("No shared TCP/UDP port was available");
        builder.WebHost.ConfigureKestrel(o=>o.Listen(IPAddress.Loopback,port,listen=>
        {listen.Protocols=HttpProtocols.Http1AndHttp2AndHttp3;listen.UseHttps(certificate);}));
        app=builder.Build();app.MapGet("/measure",async (Microsoft.AspNetCore.Http.HttpContext context)=>
        { await Task.Delay(25,context.RequestAborted);return new Observation(context.Request.Protocol,context.Connection.Id); });
        await app.StartAsync();Address=app.Urls.Single();
    }
    public async ValueTask DisposeAsync() {if(app is not null){await app.StopAsync();await app.DisposeAsync();}certificate.Dispose();key.Dispose();}
}
