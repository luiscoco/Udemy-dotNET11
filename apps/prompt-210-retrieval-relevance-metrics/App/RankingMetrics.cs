public sealed record Metrics(double Recall, double Mrr, double Ndcg);
public static class RankingMetrics
{
    public static Metrics Measure(string[] ranking, Dictionary<string,int> relevance, int k, bool broken = false)
    {
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k));
        string[] top = ranking.Distinct().Take(k).ToArray();
        int positives = relevance.Count(p => p.Value > 0);
        if (positives == 0) throw new ArgumentException("Question requires positive relevance judgments.");
        int found = top.Count(id => relevance.GetValueOrDefault(id) > 0);
        double recall = (double)found / (broken ? Math.Max(1,found) : positives);
        int first = Array.FindIndex(top,id => relevance.GetValueOrDefault(id) > 0);
        double mrr = first < 0 ? 0 : 1.0/(first+1);
        double dcg = top.Select((id,i) => (Math.Pow(2,relevance.GetValueOrDefault(id))-1)/Math.Log2(i+2)).Sum();
        double ideal = relevance.Values.OrderDescending().Take(k).Select((grade,i)=>(Math.Pow(2,grade)-1)/Math.Log2(i+2)).Sum();
        return new(recall,mrr,dcg/ideal);
    }
}
public sealed record JudgedQuestion(string Question, Dictionary<string,int> Relevance);
