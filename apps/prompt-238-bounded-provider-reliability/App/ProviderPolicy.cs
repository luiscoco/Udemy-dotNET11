using System.Net;
using System.Threading.Channels;
public sealed class ProviderPolicy(HttpClient http,bool unsafeFallback)
{
    public int Failures { get; private set; }
    public bool CircuitOpen=>Failures>=2;
    public int Fallbacks { get; private set; }
    public int Attempts { get; private set; }
    public async Task<HttpStatusCode> Send(string primary,bool compatible=true,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(CircuitOpen)throw new InvalidOperationException("Circuit open");
        Attempts++;using var response=await http.GetAsync(primary,cancellationToken);
        bool transient=(int)response.StatusCode>=500;
        if(transient)Failures++;else if(response.IsSuccessStatusCode)Failures=0;
        // Only explicitly compatible, read-only requests may cross this provider boundary.
        if(compatible&&(transient||(unsafeFallback&&response.StatusCode==HttpStatusCode.Unauthorized)))
        {
            Fallbacks++;Attempts++;using var alternate=await http.GetAsync("/alternate",cancellationToken);return alternate.StatusCode;
        }
        return response.StatusCode;
    }
    public void HalfOpenAfterRecoveryProbe(bool elapsedCooldown,bool probeSucceeded)
    {if(elapsedCooldown&&probeSucceeded)Failures=0;}
}
public sealed class RequestAdmission
{
    public Channel<string> Queue { get; }=Channel.CreateBounded<string>(new BoundedChannelOptions(1)
        {SingleReader=true,FullMode=BoundedChannelFullMode.Wait});
    private readonly Dictionary<string,int> usage=[];
    public bool Admit(string tenant,string id)
    {
        if(usage.GetValueOrDefault(tenant)>=2)return false;
        if(!Queue.Writer.TryWrite(id))return false;
        usage[tenant]=usage.GetValueOrDefault(tenant)+1;return true;
    }
}
