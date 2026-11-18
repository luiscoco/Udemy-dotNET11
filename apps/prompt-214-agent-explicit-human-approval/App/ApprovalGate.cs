using System.Security.Cryptography;
using System.Text;
public sealed record Proposal(string Actor,string Target,string Body,DateTimeOffset Expires)
{
    public string Hash => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Actor+"\n"+Target+"\n"+Body)));
}
public sealed class ApprovalGate
{
    private string? grant; private bool consumed;
    public int Effects { get; private set; }
    public Proposal Current { get; set; } = new("alice","local-course-outbox","Publish lesson draft",DateTimeOffset.UtcNow.AddMinutes(5));
    public void Approve(string actor,string displayedHash)
    {
        if(actor!=Current.Actor||displayedHash!=Current.Hash) throw new UnauthorizedAccessException("Approval does not match proposal");
        grant=displayedHash; consumed=false;
    }
    public string Apply(bool bypass=false)
    {
        if(!bypass && (grant!=Current.Hash||consumed||Current.Expires<=DateTimeOffset.UtcNow))
            throw new UnauthorizedAccessException("Fresh exact-payload approval required");
        consumed=true; Effects++;
        Harness.Save("approved-local-outbox.json",new { Current.Actor,Current.Target,Current.Body,Effects });
        return "Local teaching outbox updated. No external message sent.";
    }
}
