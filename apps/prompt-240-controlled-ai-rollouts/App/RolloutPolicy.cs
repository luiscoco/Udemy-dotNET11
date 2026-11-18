using System.Security.Cryptography;
using System.Text;
public sealed record Feedback(string RequestId,string Version,bool Helpful,bool Unsafe);
public sealed class RolloutPolicy
{
    public int CandidatePercent { get; private set; }=10;
    public static int Bucket(string trustedId)
    {
        var bytes=SHA256.HashData(Encoding.UTF8.GetBytes("rollout-v1:"+trustedId));
        return System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes)%100 is uint value?(int)value:0;
    }
    public string Select(string trustedId)=>Bucket(trustedId)<CandidatePercent?"candidate-v2":"baseline-v1";
    public bool ApplyGuard(Feedback[] events,bool skipRollback)
    {
        var baseline=events.Where(e=>e.Version=="baseline-v1").ToArray();var candidate=events.Where(e=>e.Version=="candidate-v2").ToArray();
        if(baseline.Length<5||candidate.Length<5)return false;
        double Good(Feedback[] rows)=>rows.Count(r=>r.Helpful)/(double)rows.Length;
        bool regressed=candidate.Any(e=>e.Unsafe)||Good(candidate)<Good(baseline)-0.1;
        if(regressed&&!skipRollback)CandidatePercent=0;
        return regressed;
    }
}
public sealed class FeedbackStore
{
    private readonly Dictionary<string,(string Owner,string Version)> requests=[];
    private readonly Dictionary<string,Feedback> events=[];
    public void RecordRequest(string id,string owner,string version)=>requests.Add(id,(owner,version));
    public bool Add(string requestId,string trustedOwner,bool helpful)
    {
        if(!requests.TryGetValue(requestId,out var request)||request.Owner!=trustedOwner||events.ContainsKey(requestId))return false;
        events.Add(requestId,new(requestId,request.Version,helpful,false));return true;
    }
    public int Count=>events.Count;
}
