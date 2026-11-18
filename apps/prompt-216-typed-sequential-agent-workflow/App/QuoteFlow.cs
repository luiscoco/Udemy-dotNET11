using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
public sealed record QuoteFacts(int Quantity,decimal UnitPrice,decimal Total,string Currency);
public sealed record QuoteDraft(decimal Total,string Currency,string Summary);
public sealed record ReviewedQuote(QuoteFacts Facts,QuoteDraft Draft);
public static class QuoteContract
{
    public static readonly string[] Summaries=["A ten percent discount is applied.","The quote includes a ten percent discount."];
    public static JsonElement Schema => JsonSerializer.SerializeToElement(new
    {
        type="object",properties=new
        {
            Total=new { type="number" },Currency=new { type="string" },
            Summary=new { type="string",@enum=Summaries }
        },required=new[]{"Total","Currency","Summary"},additionalProperties=false
    });
}
public sealed class CalculateQuote() : Executor<int,QuoteFacts>("calculate")
{
    public override ValueTask<QuoteFacts> HandleAsync(int quantity,IWorkflowContext context,CancellationToken cancellationToken=default)
    {
        if(quantity is <1 or >10) throw new ArgumentOutOfRangeException(nameof(quantity));
        return ValueTask.FromResult(new QuoteFacts(quantity,10m,quantity*10m*0.9m,"EUR"));
    }
}
public sealed class ExplainQuote(AIAgent writer) : Executor<QuoteFacts,ReviewedQuote>("explain")
{
    public override async ValueTask<ReviewedQuote> HandleAsync(QuoteFacts facts,IWorkflowContext context,CancellationToken cancellationToken=default)
    {
        string json=await AgentRuntime.Ask(writer,"Explain this calculated quote. Copy Total and Currency exactly: "+JsonSerializer.Serialize(facts),cancellationToken:cancellationToken);
        var draft=JsonSerializer.Deserialize<QuoteDraft>(json,new JsonSerializerOptions { PropertyNameCaseInsensitive=true })!;
        return new(facts,draft);
    }
}
public sealed class ValidateQuote(bool broken) : Executor<ReviewedQuote,ReviewedQuote>("validate")
{
    public override ValueTask<ReviewedQuote> HandleAsync(ReviewedQuote quote,IWorkflowContext context,CancellationToken cancellationToken=default)
    {
        // Controlled fixture changes the already generated structured amount.
        var candidate=broken ? quote with { Draft=quote.Draft with { Total=999m } } : quote;
        Check.That(candidate.Draft.Total==candidate.Facts.Total && candidate.Draft.Currency==candidate.Facts.Currency &&
            QuoteContract.Summaries.Contains(candidate.Draft.Summary),
            "structured explanation preserves authoritative price and currency","AGENT workflow accepted unsupported total");
        return ValueTask.FromResult(candidate);
    }
}
