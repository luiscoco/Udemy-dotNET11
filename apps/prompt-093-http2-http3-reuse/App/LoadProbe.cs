using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
public static class LoadProbe
{
    public static async Task<int> RunAsync(TransportServer server,Version version)
    {
        using var handler=new SocketsHttpHandler {MaxConnectionsPerServer=1,EnableMultipleHttp2Connections=false};
        handler.SslOptions.RemoteCertificateValidationCallback=(_,cert,_,_)=>cert is not null &&
            string.Equals(cert.GetCertHashString(),server.Thumbprint,StringComparison.OrdinalIgnoreCase);
        using var client=new HttpClient(handler) {BaseAddress=new Uri(server.Address),Timeout=TimeSpan.FromSeconds(30)};
        var ids=new HashSet<string>();var watch=Stopwatch.StartNew();
        for(int batch=0;batch<2;batch++)
        {
            var rows=await Task.WhenAll(Enumerable.Range(0,12).Select(async _=>
            {
                using var request=new HttpRequestMessage(HttpMethod.Get,"/measure") {Version=version,VersionPolicy=HttpVersionPolicy.RequestVersionExact};
                using var response=await client.SendAsync(request);response.EnsureSuccessStatusCode();
                var row=(await response.Content.ReadFromJsonAsync<Observation>())!;
                if(response.Version!=version || row.Protocol!="HTTP/"+version.Major+(version.Major==1?".1":""))throw new InvalidOperationException("Observed protocol differs from exact request");
                return row;
            }));
            foreach(var row in rows)ids.Add(row.ConnectionId);
        }
        watch.Stop();Console.WriteLine($"network: HTTP/{version}: 24 requests; observed connections {ids.Count}; elapsed {watch.ElapsedMilliseconds} ms");
        Check.That(ids.Count==1,"both load batches reuse the observed connection for this protocol");return ids.Count;
    }
}
