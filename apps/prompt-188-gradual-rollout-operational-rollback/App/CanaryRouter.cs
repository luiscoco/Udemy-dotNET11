using Microsoft.AspNetCore.Http;
public sealed class CanaryRouter(string stable,string candidate):IDisposable
{
    private readonly HttpClient client=new();
    private int sequence;
    public int CandidatePercent {get;private set;}
    public void SetWeight(int percent) { CandidatePercent=percent; sequence=0; }
    public async Task<IResult> Forward()
    {
        bool useCandidate=Interlocked.Increment(ref sequence)%100<CandidatePercent;
        using var response=await client.GetAsync(useCandidate?candidate:stable);
        string body=await response.Content.ReadAsStringAsync();
        return Results.Content(body,"application/json",statusCode:(int)response.StatusCode);
    }
    public void Dispose()=>client.Dispose();
}
