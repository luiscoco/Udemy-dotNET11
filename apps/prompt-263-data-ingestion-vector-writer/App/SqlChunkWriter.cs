using Microsoft.Extensions.DataIngestion;
using Microsoft.Data.SqlClient;
using System.Text.Json;
public sealed class SqlChunkWriter(SqlConnection connection,string table):IngestionChunkWriter<string>
{
    public int Written{get;private set;}
    public override async Task WriteAsync(IAsyncEnumerable<IngestionChunk<string>> chunks,CancellationToken cancellationToken=default)
    {
        await foreach(var chunk in chunks.WithCancellation(cancellationToken)){
            if(!chunk.Metadata.TryGetValue("contentSha256",out var hash)||!chunk.Metadata.TryGetValue("source",out var source))throw new InvalidDataException("Required provenance enrichment missing");
            float[] vector=[1+chunk.Content.Split("async",StringSplitOptions.None).Length,1+chunk.Content.Split("cache",StringSplitOptions.None).Length];
            using var cmd=new SqlCommand($"INSERT INTO {table}(Content,Source,Hash,Embedding) VALUES(@text,@source,@hash,CAST(@vector AS vector(2)))",connection);
            cmd.Parameters.AddWithValue("@text",chunk.Content);cmd.Parameters.AddWithValue("@source",source);cmd.Parameters.AddWithValue("@hash",hash);cmd.Parameters.AddWithValue("@vector",JsonSerializer.Serialize(vector));
            await cmd.ExecuteNonQueryAsync(cancellationToken);Written++;
        }
    }
}
