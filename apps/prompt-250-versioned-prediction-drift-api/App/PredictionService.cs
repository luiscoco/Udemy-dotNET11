using Microsoft.ML;
public sealed class PredictInput {public float X {get;set;}}
public sealed class PredictTraining {public float X {get;set;}public float Label {get;set;}}
public sealed class PredictOutput {public float Score {get;set;}}
public sealed class PredictionService
{
    private readonly MLContext ml=new(seed:42);
    private readonly ITransformer model;
    private readonly object gate=new();
    private readonly List<float> observed=[];
    public const string Version="synthetic-linear-v1";
    public PredictionService()
    {
        var train=ml.Data.LoadFromEnumerable(Enumerable.Range(0,100).Select(i=>new PredictTraining {X=i/10f,Label=2+3*i/10f}));
        model=ml.Transforms.Concatenate("Features","X").Append(ml.Regression.Trainers.Sdca(maximumNumberOfIterations:100)).Fit(train);
    }
    public float Predict(float x,bool skipMonitor)
    {
        if(!float.IsFinite(x)||x is <0 or >100)throw new ArgumentException("X must be finite and between zero and 100");
        lock(gate)
        {
            if(!skipMonitor){observed.Add(x);if(observed.Count>20)observed.RemoveAt(0);}
            // Batch Transform avoids sharing a non-thread-safe PredictionEngine across requests.
            return ml.Data.CreateEnumerable<PredictOutput>(model.Transform(ml.Data.LoadFromEnumerable(new[]{new PredictInput {X=x}})),false).Single().Score;
        }
    }
    public (int Samples,double MeanShift,bool Alert) Monitor()
    {
        lock(gate){double shift=observed.Count==0?0:Math.Abs(observed.Average()-4.95)/2.887;
            return (observed.Count,shift,observed.Count>=10&&shift>3);}
    }
}
