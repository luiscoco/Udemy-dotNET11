using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
public sealed class DayRow {public int Day {get;set;}public float Value {get;set;}}
public sealed class Forecast { [VectorType(7)]public float[] Forecasted {get;set;}=[];}
public sealed class Spike { [VectorType(3)]public double[] Alert {get;set;}=[];}
public static class TimeSeriesLab
{
    public static DayRow[] Days()=>Enumerable.Range(0,91).Select(i=>new DayRow
        {Day=i,Value=50+10*(float)Math.Sin(2*Math.PI*i/7)+i*0.1f}).ToArray();
    public static float[] Predict(MLContext ml,DayRow[] train)
    {
        var view=ml.Data.LoadFromEnumerable(train);
        var model=ml.Forecasting.ForecastBySsa("Forecasted","Value",windowSize:7,seriesLength:21,
            trainSize:train.Length,horizon:7).Fit(view);
        using var engine=model.CreateTimeSeriesEngine<DayRow,Forecast>(ml);
        return engine.Predict().Forecasted;
    }
    public static bool DetectSpike(MLContext ml)
    {
        var rows=Enumerable.Range(0,60).Select(i=>new DayRow {Day=i,Value=i==45?150:10+(i%3)*0.1f});
        var view=ml.Data.LoadFromEnumerable(rows);
        var detector=ml.Transforms.DetectIidSpike("Alert","Value",confidence:95.0,pvalueHistoryLength:20)
            .Fit(ml.Data.LoadFromEnumerable(Array.Empty<DayRow>()));
        return ml.Data.CreateEnumerable<Spike>(detector.Transform(view),false).ElementAt(45).Alert[0]==1;
    }
}
