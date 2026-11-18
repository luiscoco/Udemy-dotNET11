using Microsoft.ML;
var ml=new MLContext(seed:42);var original=ml.Data.LoadFromEnumerable(SplitData.Rows());
var outer=ml.Data.TrainTestSplit(original,testFraction:0.2,samplingKeyColumnName:"Customer",seed:42);
var inner=ml.Data.TrainTestSplit(outer.TrainSet,testFraction:0.25,samplingKeyColumnName:"Customer",seed:43);
var train=SplitData.Materialize(ml,inner.TrainSet);var validation=SplitData.Materialize(ml,inner.TestSet);var test=SplitData.Materialize(ml,outer.TestSet);
Check.That(train.Length>0&&validation.Length>0&&test.Length>0&&SplitData.Disjoint(train,validation,test),"the actual ML.NET split keeps every customer in exactly one partition");
// A deliberately extreme held-out sentinel exposes fitting preprocessing on future data.
foreach(var row in validation.Concat(test))row.Value+=1000;
var trainView=ml.Data.LoadFromEnumerable(train);var allView=ml.Data.LoadFromEnumerable(train.Concat(validation).Concat(test));
var normalizer=ml.Transforms.NormalizeMinMax("Scaled","Value").Fit(args.Contains("problem")?allView:trainView);
var scaled=ml.Data.CreateEnumerable<NormalizedRow>(normalizer.Transform(trainView),false).ToArray();
float maximum=scaled.Max(r=>r.Scaled);
Console.WriteLine($"ML train={train.Length} validation={validation.Length} test={test.Length} groupsDisjoint=true trainNormalizedMax={maximum:F4}");
Check.That(Math.Abs(maximum-1)<0.001,"normalization parameters are fitted exclusively on the training partition","ML preprocessing used held-out data");
var repeated=ml.Data.TrainTestSplit(original,testFraction:0.2,samplingKeyColumnName:"Customer",seed:42);
Check.That(test.Select(r=>r.Customer).Order().SequenceEqual(SplitData.Materialize(ml,repeated.TestSet).Select(r=>r.Customer).Order()),"the seeded group split repeats the held-out identity inventory");
Harness.Save("ml-evidence.json",new {passed=true,actualMlNet=true,train=train.Length,validation=validation.Length,test=test.Length,groupsDisjoint=true,maximum,seed=42,trainingOnlyPreprocessing=true});
