using Microsoft.ML;
public sealed class RegressionRow {public float X {get;set;}public float Label {get;set;}}
public sealed class RegressionPrediction {public float Score {get;set;}public float Label {get;set;}}
public static class RegressionLab
{
    public static RegressionRow[] Rows()=>Enumerable.Range(0,200).Select(i=>new RegressionRow
        {X=i/10f,Label=2+3*(i/10f)+(float)Math.Sin(i)*0.1f}).ToArray();
    public static ITransformer Train(MLContext ml,IDataView rows)=>ml.Transforms.Concatenate("Features","X")
        .Append(ml.Transforms.NormalizeMeanVariance("Features"))
        .Append(ml.Regression.Trainers.Sdca(maximumNumberOfIterations:300)).Fit(rows);
}
