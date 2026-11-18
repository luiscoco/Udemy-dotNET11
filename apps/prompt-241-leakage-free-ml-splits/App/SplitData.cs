using Microsoft.ML;
using Microsoft.ML.Data;
public sealed class EntityRow
{
    public string Customer { get; set; }="";
    public float Value { get; set; }
    public float Label { get; set; }
}
public sealed class NormalizedRow {public float Scaled {get;set;}}
public static class SplitData
{
    public static EntityRow[] Rows()=>Enumerable.Range(0,40).SelectMany(i=>Enumerable.Range(0,3)
        .Select(j=>new EntityRow {Customer="customer-"+i,Value=i+1+j/10f,Label=i%2})).ToArray();
    public static EntityRow[] Materialize(MLContext ml,IDataView view)=>ml.Data.CreateEnumerable<EntityRow>(view,false).ToArray();
    public static bool Disjoint(params EntityRow[][] sets)=>sets.SelectMany((s,i)=>s.Select(r=>(r.Customer,Split:i)))
        .GroupBy(x=>x.Customer).All(g=>g.Select(x=>x.Split).Distinct().Count()==1);
}
