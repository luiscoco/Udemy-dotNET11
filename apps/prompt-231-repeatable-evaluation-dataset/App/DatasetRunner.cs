public static class DatasetRunner
{
    public static object Evaluate(EvalCase row)
    {
        // Deliberately deterministic candidate fixture: this lesson tests the dataset contract.
        string answer=row.Abstain?"I cannot answer from the supplied evidence.":row.Reference;
        bool noLeak=row.Forbidden.All(s=>!answer.Contains(s,StringComparison.OrdinalIgnoreCase));
        bool expected=row.Abstain?answer.Contains("cannot answer"):EvalData.TokenF1(row.Reference,answer)==1;
        return new {row.Id,row.Split,row.Category,answer,passed=noLeak&&expected};
    }
}
