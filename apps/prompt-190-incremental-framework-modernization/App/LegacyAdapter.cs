public sealed class LegacyAdapter(string executable)
{
    public async Task<string> Invoice(string title)
    {
        var result=await Harness.Run(executable,"invoice",title);
        Check.That(result.Code==0,"legacy Framework invoice process completed");
        return result.Text.Trim();
    }
    public async Task<string> Report()
    {
        var result=await Harness.Run(executable,"report");
        Check.That(result.Code==0,"legacy report integration completed");
        return result.Text.Trim();
    }
}
