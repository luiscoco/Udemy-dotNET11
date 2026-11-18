using System.Net;
using System.Net.Quic;
await using var server=new TransportServer();await server.StartAsync();
await LoadProbe.RunAsync(server,HttpVersion.Version20);
if(args.FirstOrDefault()=="fallback")
{
    await LoadProbe.RunAsync(server,HttpVersion.Version11);
    Check.That(false,"an HTTP/1.1 result must never be labeled as an HTTP/2 or HTTP/3 measurement");
}
Console.WriteLine("network: QUIC supported on this host: "+QuicConnection.IsSupported);
if(QuicConnection.IsSupported) await LoadProbe.RunAsync(server,HttpVersion.Version30);
else
{
    try {await LoadProbe.RunAsync(server,HttpVersion.Version30);throw new Exception("Unexpected unsupported QUIC success");}
    catch(HttpRequestException error) {Console.WriteLine("network: HTTP/3 exact request failed explicitly: "+error.GetType().Name);}
    Check.That(!QuicConnection.IsSupported,"HTTP/3 unavailability is reported without silently measuring a fallback protocol");
}
