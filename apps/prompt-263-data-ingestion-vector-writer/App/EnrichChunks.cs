using Microsoft.Extensions.DataIngestion;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
public sealed class EnrichChunks:IngestionChunkProcessor<string>
{
    public override async IAsyncEnumerable<IngestionChunk<string>> ProcessAsync(IAsyncEnumerable<IngestionChunk<string>> chunks,[EnumeratorCancellation]CancellationToken cancellationToken=default)
    {
        await foreach(var chunk in chunks.WithCancellation(cancellationToken)){
            chunk.Metadata["contentSha256"]=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(chunk.Content)));
            chunk.Metadata["source"]=chunk.Document.Identifier;
            yield return chunk;
        }
    }
}
