using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
public static class QualityRun
{
    public static async Task<object[]> Run(EvalClient client)
    {
        ChatMessage[] messages=[new(ChatRole.User,"How many days are backups retained?")];
        var answer=new ChatResponse(new ChatMessage(ChatRole.Assistant,"Backups are retained for seven days."));
        string context="Synthetic policy: backups are retained for seven days.";
        (IEvaluator Evaluator,EvaluationContext[] Context)[] jobs=[
            (new RelevanceEvaluator(),[]),
            (new CompletenessEvaluator(),[new CompletenessEvaluatorContext("Backups are retained for seven days.")]),
            (new RetrievalEvaluator(),[new RetrievalEvaluatorContext([context])]),
            (new GroundednessEvaluator(),[new GroundednessEvaluatorContext(context)])];
        var scores=new List<object>();
        foreach(var job in jobs)
        {
            var result=await job.Evaluator.EvaluateAsync(messages,answer,new ChatConfiguration(new TaggedJudge(client)),job.Context);
            foreach(var metric in result.Metrics.Values)
            {
                double? value=(metric as NumericMetric)?.Value;
                bool valid=value is >=1 and <=5 && metric.Interpretation?.Rating!=EvaluationRating.Inconclusive;
                Console.WriteLine($"EVAL evaluator={job.Evaluator.GetType().Name} metric={metric.Name} score={value} valid={valid}");
                scores.Add(new {evaluator=job.Evaluator.GetType().Name,metric.Name,value,valid,
                    rating=metric.Interpretation?.Rating.ToString(),reason=metric.Interpretation?.Reason,
                    diagnostics=metric.Diagnostics?.Select(d=>d.Message).ToArray()});
            }
        }
        return scores.ToArray();
    }
}
// Reinforce the official evaluator's requested format; no scores are changed or synthesized.
public sealed class TaggedJudge(IChatClient inner):DelegatingChatClient(inner)
{
    public override Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options=null,CancellationToken cancellationToken=default)
        =>base.GetResponseAsync(new[]{new ChatMessage(ChatRole.System,
            "You are a strict XML rating service. Output ONLY <S0>brief assessment</S0><S1>short justification</S1><S2>integer rating from 1 to 5</S2>. Always close every tag. Never output Markdown or text outside these tags. /no_think")}.Concat(messages),options,cancellationToken);
}
