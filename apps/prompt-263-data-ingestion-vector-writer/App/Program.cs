using Microsoft.Extensions.DataIngestion;
using Microsoft.ML.Tokenizers;
using Microsoft.Data.SqlClient;
await using var connection=await LocalSql.Open();string table="dbo.L263_"+Guid.NewGuid().ToString("N");
await LocalSql.Execute(connection,$"CREATE TABLE {table}(Id int IDENTITY PRIMARY KEY,Content nvarchar(max),Source nvarchar(1000),Hash char(64),Embedding vector(2))");
try{
    var tokenizer=BertTokenizer.Create("assets/bert-vocab.txt");
    var chunker=new HeaderChunker(new IngestionChunkerOptions(tokenizer){MaxTokensPerChunk=32,OverlapTokens=0});
    using var writer=new SqlChunkWriter(connection,table);
    using var pipeline=new IngestionPipeline<string>(new MarkdownReader(),chunker,writer);
    if(!args.Contains("problem"))pipeline.ChunkProcessors.Add(new EnrichChunks());
    var results=new List<IngestionResult>();
    await foreach(var result in pipeline.ProcessAsync(new DirectoryInfo("documents"),"*.md")){results.Add(result);Console.WriteLine($"INGEST document={result.DocumentId} succeeded={result.Succeeded} error={result.Exception?.Message}");}
    Check.That(results.Count==1&&results.All(r=>r.Succeeded),"the actual ingestion pipeline completed every document","PREVIEW ingestion enrichment contract failed");
    using var query=new SqlCommand($"SELECT COUNT(*) FROM {table} WHERE LEN(Hash)=64 AND Source IS NOT NULL AND VECTOR_DISTANCE('cosine',Embedding,CAST('[1,0]' AS vector(2))) IS NOT NULL",connection);
    int stored=Convert.ToInt32(await query.ExecuteScalarAsync());
    Check.That(stored>=2&&stored==writer.Written,"actual SQL vectors retain all enriched chunks");
    Harness.Save("preview-evidence.json",new{passed=true,actualIngestion=true,actualSqlVectors=true,stored,writer.Written,documents=results.Count,embedding="explicit two-dimensional lexical fixture; no neural embedding claim"});
}finally{await LocalSql.Execute(connection,$"DROP TABLE IF EXISTS {table}");}
