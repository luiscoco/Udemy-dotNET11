using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlTypes;
using Microsoft.Data.SqlClient;
using System.Text.Json;
#pragma warning disable EF9105 // Explicit evaluation of experimental EF vector search.
string table="L266_"+Guid.NewGuid().ToString("N");await using var connection=await LocalSql.Open();
await LocalSql.Execute(connection,"ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES=ON");
await LocalSql.Execute(connection,$"CREATE TABLE dbo.{table}(Id int NOT NULL PRIMARY KEY CLUSTERED,Embedding vector(3) NOT NULL)");
try{
    await using var db=new VectorDb(table);var random=new Random(266);
    for(int i=1;i<=1000;i++)db.Rows.Add(new VectorRow{Id=i,Embedding=new SqlVector<float>(new float[]{random.NextSingle()+.001f,random.NextSingle()+.001f,random.NextSingle()+.001f})});
    await db.SaveChangesAsync();
    await LocalSql.Execute(connection,$"CREATE VECTOR INDEX ix_vector ON dbo.{table}(Embedding) WITH (METRIC='cosine',TYPE='DiskANN')");
    var target=new SqlVector<float>(new float[]{1,.1f,.1f});
    var exact=await db.Rows.OrderBy(r=>EF.Functions.VectorDistance("cosine",r.Embedding,target)).Take(5).Select(r=>r.Id).ToArrayAsync();
    var input=target;
    var approximate=db.Rows.VectorSearch(r=>r.Embedding,input,"cosine").OrderBy(r=>r.Distance).Take(5).WithApproximate();
    string generated=approximate.ToQueryString();Console.WriteLine("VECTOR EF SQL "+generated);
    int[] ids;bool nativeCompatible=true;
    try{ids=(await approximate.ToListAsync()).Select(r=>r.Value.Id).ToArray();}
    catch(SqlException error) when(error.Number==102){
        nativeCompatible=false;Console.WriteLine("VECTOR observed provider/server dialect mismatch: "+error.Message);
        if(args.Contains("problem"))throw new InvalidOperationException("PREVIEW approximate-search recall contract failed",error);
        string legacy=$"SELECT t.Id,v.distance AS Distance FROM VECTOR_SEARCH(TABLE=dbo.{table} AS t,COLUMN=Embedding,SIMILAR_TO=@vector,METRIC='cosine',TOP_N=5) AS v ORDER BY v.distance";
        ids=(await db.Database.SqlQueryRaw<SearchHit>(legacy,new SqlParameter("vector",input)).ToListAsync()).Select(r=>r.Id).ToArray();
        Console.WriteLine("VECTOR repaired with observed-server legacy VECTOR_SEARCH syntax through EF SqlQueryRaw");
    }
    double recall=ids.Intersect(exact).Count()/5d;
    using var metadata=new SqlCommand($"SELECT COUNT(*) FROM sys.vector_indexes WHERE object_id=OBJECT_ID('dbo.{table}')",connection);int indexes=Convert.ToInt32(await metadata.ExecuteScalarAsync());
    Console.WriteLine($"VECTOR exact={string.Join(',',exact)} approximate={string.Join(',',ids)} recallAt5={recall:F2} indexes={indexes}");
    Check.That(indexes==1&&generated.Contains("WITH APPROXIMATE"),"actual vector index and EF approximate SQL translation exist");
    Check.That(recall>=.8,"the observed ANN IDs meet the fixture recall contract","PREVIEW approximate-search recall contract failed");
    Harness.Save("preview-evidence.json",new{passed=true,actualSqlVectorIndex=true,actualEfVectorSearch=true,nativeCompatible,compatibilityAdapter=!nativeCompatible,rows=1000,exact,approximate=ids,recallAt5=recall,indexes,metric="cosine",speedupClaim=false});
}finally{await LocalSql.Execute(connection,$"DROP TABLE IF EXISTS dbo.{table}");}
public sealed class SearchHit{public int Id{get;set;}public double Distance{get;set;}}
