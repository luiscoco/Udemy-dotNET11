using System.Net;
public sealed class CatalogReader(Uri endpoint,HttpClient client)
{
    public async Task<string> Fetch(string requested)
    {
        if(!Uri.TryCreate(requested,UriKind.Absolute,out var uri)||uri.Scheme!="https"||uri.UserInfo.Length!=0||uri.Fragment.Length!=0||
            uri.GetLeftPart(UriPartial.Authority)!=endpoint.GetLeftPart(UriPartial.Authority)||uri.AbsolutePath!="/catalog")
            throw new UnauthorizedAccessException("Destination is not the configured catalog endpoint");
        // Client is configured without automatic redirects and pins the owned fixture certificate.
        using var response=await client.GetAsync(uri,HttpCompletionOption.ResponseHeadersRead);
        if((int)response.StatusCode>=300&&(int)response.StatusCode<400)throw new UnauthorizedAccessException("Redirect destinations are not accepted");
        response.EnsureSuccessStatusCode();using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await using var stream=await response.Content.ReadAsStreamAsync(timeout.Token);
        var bytes=new byte[1025];int used=0;
        while(used<bytes.Length){var read=await stream.ReadAsync(bytes.AsMemory(used),timeout.Token);if(read==0)break;used+=read;}
        if(used>1024)throw new InvalidOperationException("Catalog response exceeds the allowed size");
        return System.Text.Encoding.UTF8.GetString(bytes,0,used);
    }
}
