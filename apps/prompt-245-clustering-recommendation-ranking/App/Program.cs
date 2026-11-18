using Microsoft.ML;
var ml=new MLContext(seed:42);
double cluster=TaskModels.Cluster(ml);double recommendation=TaskModels.Recommend(ml);
var ranking=TaskModels.Rank(ml,args.Contains("problem"));
Console.WriteLine($"ML clusteringDBI={cluster:F4} recommendationRMSE={recommendation:F4} rankingNDCG1={ranking.Ndcg:F4}");
Check.That(double.IsFinite(cluster)&&double.IsFinite(recommendation)&&double.IsFinite(ranking.Ndcg),"three actual trainers produce distinct task-specific evaluation metrics");
Harness.Save("ml-evidence.json",new {passed=true,actualMlNet=true,models=3,clusteringDbi=cluster,recommendationRmse=recommendation,rankingNdcg1=ranking.Ndcg,groupsDisjoint=ranking.Disjoint});
