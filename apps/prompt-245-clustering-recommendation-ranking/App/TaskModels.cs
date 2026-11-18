using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
public sealed class Point {public float X {get;set;}public float Y {get;set;}}
public sealed class Rating {public uint User {get;set;}public uint Item {get;set;}public float Label {get;set;}}
public sealed class Ranked {[KeyType(20)]public uint Query {get;set;}public float Quality {get;set;}public float Label {get;set;}}
public static class TaskModels
{
    public static double Cluster(MLContext ml)
    {
        var rows=Enumerable.Range(0,100).Select(i=>new Point {X=i<50?-5+i*0.01f:5+i*0.01f,Y=i<50?-4:4});
        var data=ml.Data.LoadFromEnumerable(rows);
        var model=ml.Transforms.Concatenate("Features","X","Y").Append(ml.Clustering.Trainers.KMeans(numberOfClusters:2)).Fit(data);
        return ml.Clustering.Evaluate(model.Transform(data),featureColumnName:"Features").DaviesBouldinIndex;
    }
    public static double Recommend(MLContext ml)
    {
        var rows=Enumerable.Range(1,12).SelectMany(u=>Enumerable.Range(1,8).Select(i=>new Rating
            {User=(uint)u,Item=(uint)i,Label=u%2==i%2?5:1})).ToArray();
        var train=ml.Data.LoadFromEnumerable(rows.Where((r,i)=>i%7!=0));var test=ml.Data.LoadFromEnumerable(rows.Where((r,i)=>i%7==0));
        var pipe=ml.Transforms.Conversion.MapValueToKey("UserKey","User")
            .Append(ml.Transforms.Conversion.MapValueToKey("ItemKey","Item"))
            .Append(ml.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
            {LabelColumnName="Label",MatrixColumnIndexColumnName="UserKey",MatrixRowIndexColumnName="ItemKey",NumberOfIterations=30,ApproximationRank=4,NumberOfThreads=1}));
        var model=pipe.Fit(train);return ml.Regression.Evaluate(model.Transform(test)).RootMeanSquaredError;
    }
    public static (double Ndcg,bool Disjoint) Rank(MLContext ml,bool leak)
    {
        var rows=Enumerable.Range(1,20).SelectMany(q=>Enumerable.Range(0,5).Select(i=>new Ranked {Query=(uint)q,Quality=i,Label=i})).ToArray();
        var trainRows=rows.Where(r=>r.Query<=15).ToArray();var testRows=leak?rows.Where((r,i)=>i%4==0).ToArray():rows.Where(r=>r.Query>15).ToArray();
        bool disjoint=!trainRows.Select(r=>r.Query).Intersect(testRows.Select(r=>r.Query)).Any();
        Console.WriteLine($"ML rankingTrainGroups={trainRows.Select(r=>r.Query).Distinct().Count()} rankingTestGroups={testRows.Select(r=>r.Query).Distinct().Count()} disjoint={disjoint}");
        Check.That(disjoint,"ranking query groups remain completely held out","ML ranking groups crossed the held-out split");
        var train=ml.Data.LoadFromEnumerable(trainRows);var test=ml.Data.LoadFromEnumerable(testRows);
        var pipe=ml.Transforms.Concatenate("Features","Quality")
            .Append(ml.Ranking.Trainers.LightGbm(labelColumnName:"Label",rowGroupColumnName:"Query",numberOfIterations:20,minimumExampleCountPerLeaf:1,numberOfLeaves:4));
        var model=pipe.Fit(train);
        return (ml.Ranking.Evaluate(model.Transform(test),rowGroupColumnName:"Query").NormalizedDiscountedCumulativeGains[0],disjoint);
    }
}
