using Microsoft.ML;
using Microsoft.ML.Data;
public sealed class Sample
{
    public float X {get;set;}
    public float Y {get;set;}
    public bool Label {get;set;}
}
public sealed class Classified
{
    public bool Label {get;set;}
    public bool PredictedLabel {get;set;}
    public float Score {get;set;}
    public float Probability {get;set;}
}
public static class ClassificationLab
{
    public static Sample[] Data()=>Enumerable.Range(0,400).Select(i=>new Sample
        {X=(i%20-10)/5f,Y=(i/20-10)/5f,Label=(i%20-10)+0.4*(i/20-10)>0}).ToArray();
    public static ITransformer Train(MLContext ml,IDataView train)=>ml.Transforms.Concatenate("Features","X","Y")
        .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression(maximumNumberOfIterations:100)).Fit(train);
}
