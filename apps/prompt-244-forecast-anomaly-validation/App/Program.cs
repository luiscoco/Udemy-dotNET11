using Microsoft.ML;
var ml=new MLContext(seed:42);var all=TimeSeriesLab.Days();
var train=args.Contains("problem")?all:all[..70];var validation=all[70..77];var test=all[77..84];
Console.WriteLine($"ML trainLastDay={train[^1].Day} validationFirstDay={validation[0].Day} testFirstDay={test[0].Day}");
Check.That(train[^1].Day<validation[0].Day&&validation[^1].Day<test[0].Day,"training, validation and test preserve time order without future fitting","ML forecast training crossed the time boundary");
var forecast=TimeSeriesLab.Predict(ml,train);
double validationMae=forecast.Zip(validation,(f,r)=>Math.Abs(f-r.Value)).Average();
// After the fixed configuration is chosen, fit through validation and evaluate the untouched next window.
var finalForecast=TimeSeriesLab.Predict(ml,all[..77]);
double testMae=finalForecast.Zip(test,(f,r)=>Math.Abs(f-r.Value)).Average();
bool spike=TimeSeriesLab.DetectSpike(ml);
Console.WriteLine($"ML validationMAE={validationMae:F3} testMAE={testMae:F3} forecastHorizon={finalForecast.Length} spikeAt45={spike}");
Check.That(finalForecast.Length==7&&finalForecast.All(float.IsFinite)&&double.IsFinite(testMae),"actual SSA produces seven finite future predictions");
Check.That(spike,"the separate sequential anomaly detector flags the injected spike");
Harness.Save("ml-evidence.json",new {passed=true,actualMlNet=true,trainLast=69,validationFirst=70,testFirst=77,validationMae,testMae,spike,horizon=7,forecast=finalForecast});
