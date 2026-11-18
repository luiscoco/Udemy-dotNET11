using Microsoft.ML;
var ml=new MLContext(seed:42);var rows=ClassificationLab.Data();
var train=ml.Data.LoadFromEnumerable(rows.Where((r,i)=>i%5!=0));var test=ml.Data.LoadFromEnumerable(rows.Where((r,i)=>i%5==0));
var model=ClassificationLab.Train(ml,train);var scored=model.Transform(test);
var observed=ml.Data.CreateEnumerable<Classified>(scored,false).ToArray();
// Evaluate an explicit decision threshold without changing the trained model.
bool unsafeThreshold=args.Contains("problem");
var decisions=observed.Select(r=>new Classified {Label=r.Label,PredictedLabel=unsafeThreshold?r.Probability>=1.01:r.PredictedLabel,
    Score=unsafeThreshold?r.Probability-1.01f:r.Score,Probability=r.Probability}).ToArray();
var metrics=ml.BinaryClassification.Evaluate(ml.Data.LoadFromEnumerable(decisions));
Console.WriteLine($"ML precision={metrics.PositivePrecision:F3} recall={metrics.PositiveRecall:F3} accuracy={metrics.Accuracy:F3}");
Console.WriteLine(metrics.ConfusionMatrix.GetFormattedConfusionTable());
int tp=decisions.Count(r=>r.Label&&r.PredictedLabel),fn=decisions.Count(r=>r.Label&&!r.PredictedLabel);
Check.That(tp+fn>0&&metrics.PositiveRecall>0.8,"positive-case recall remains above the fixture threshold","ML classifier lost positive-case recall");
Check.That(Math.Abs(metrics.PositiveRecall-tp/(double)(tp+fn))<0.001,"the confusion counts independently reproduce recall");
Harness.Save("ml-evidence.json",new {passed=true,actualMlNet=true,trained=true,trainRows=320,testRows=80,metrics.PositivePrecision,metrics.PositiveRecall,metrics.Accuracy,tp,fn,confusion=metrics.ConfusionMatrix.Counts});
