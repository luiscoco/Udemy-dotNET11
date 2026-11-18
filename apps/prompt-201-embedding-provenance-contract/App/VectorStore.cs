using Npgsql;
using System.Globalization;
using System.Text.Json;
public sealed record VectorRecord(Document Document, EmbeddedText Embedding);
public sealed record Hit(string Id, string Title, string Body, string Url, double Score);
public sealed class VectorStore(NpgsqlConnection connection, string schema, bool owner) : IAsyncDisposable
{
    public NpgsqlConnection Connection => connection;
    public string Schema => schema;
    public int Searches { get; private set; }
    public static string ConnectionString => Environment.GetEnvironmentVariable("COURSE_RAG_CONNECTION")
        ?? "Host=127.0.0.1;Port=15439;Database=course;Username=course;Password=Course_Rag_11_local!;Pooling=false";
    public static async Task<VectorStore> Open(int number, string? existing = null)
    {
        string name = existing ?? $"l{number}_{Guid.NewGuid():N}";
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^l[0-9]+_[a-f0-9]{32}$")) throw new ArgumentException("Invalid owned schema");
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        if (existing is null) await new NpgsqlCommand($"CREATE SCHEMA {name}", connection).ExecuteNonQueryAsync();
        await new NpgsqlCommand($"SET search_path TO {name}, public", connection).ExecuteNonQueryAsync();
        return new(connection, name, existing is null);
    }
    public async Task<int> Execute(string sql, params (string Key, object Value)[] values)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var (key,value) in values) command.Parameters.AddWithValue(key,value);
        return await command.ExecuteNonQueryAsync();
    }
    public async Task<T> Scalar<T>(string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync())!;
    }
    public async Task CreateCollection(string table = "items", int dimensions = 384)
    {
        Table(table);
        if (dimensions is not (384 or 768)) throw new ArgumentException("Unsupported fixture dimension");
        await Execute($"CREATE TABLE {table} (id text PRIMARY KEY, title text NOT NULL, body text NOT NULL, tenant text NOT NULL, permitted_user text NOT NULL, version int NOT NULL, url text NOT NULL, model text NOT NULL, digest text NOT NULL, preprocessing text NOT NULL, embedding vector({dimensions}) NOT NULL, search tsvector GENERATED ALWAYS AS (to_tsvector('english',title || ' ' || body)) STORED)");
        await Execute($"CREATE INDEX ON {table} USING hnsw (embedding vector_cosine_ops)");
        await Execute($"CREATE INDEX ON {table} USING gin (search)");
    }
    public static void Table(string table)
    {
        if (table is not ("items" or "candidate")) throw new ArgumentException("Unknown fixture collection");
    }
    public static string VectorText(float[] vector) => "[" + string.Join(',', vector.Select(v => v.ToString("R", CultureInfo.InvariantCulture))) + "]";
    public async Task Upsert(VectorRecord record, string table = "items")
    {
        Table(table); var d = record.Document; var e = record.Embedding;
        await Execute($"INSERT INTO {table}(id,title,body,tenant,permitted_user,version,url,model,digest,preprocessing,embedding) VALUES(@id,@title,@body,@tenant,@user,@version,@url,@model,@digest,@pre,@v::vector) ON CONFLICT(id) DO UPDATE SET title=excluded.title,body=excluded.body,tenant=excluded.tenant,permitted_user=excluded.permitted_user,version=excluded.version,model=excluded.model,digest=excluded.digest,preprocessing=excluded.preprocessing,embedding=excluded.embedding",
            ("id",d.Id),("title",d.Title),("body",d.Body),("tenant",d.Tenant),("user",d.User),("version",d.Version),("url",d.Url),("model",e.Stamp.Model),("digest",e.Stamp.Digest),("pre",e.Stamp.Preprocessing),("v",VectorText(e.Vector)));
    }
    public async Task<Document?> Retrieve(string id)
    {
        await using var command = new NpgsqlCommand("SELECT id,title,body,tenant,permitted_user,version FROM items WHERE id=@id",connection);
        command.Parameters.AddWithValue("id",id);await using var reader=await command.ExecuteReaderAsync();
        return await reader.ReadAsync()?new(reader.GetString(0),reader.GetString(1),reader.GetString(2),reader.GetString(3),reader.GetString(4),reader.GetInt32(5)):null;
    }
    public async Task<float[]?> RetrieveVector(string id)
    {
        await using var command = new NpgsqlCommand("SELECT embedding::text FROM items WHERE id=@id",connection);
        command.Parameters.AddWithValue("id",id);
        var value = await command.ExecuteScalarAsync();
        return value is string text ? JsonSerializer.Deserialize<float[]>(text) : null;
    }
    public Task Delete(string id) => Execute("DELETE FROM items WHERE id=@id",("id",id));
    public async Task Seed(EmbeddingService embeddings, string table = "items")
    {
        foreach (var document in Corpus.Documents)
            await Upsert(new(document, await embeddings.Generate(document.Title + ". " + document.Body)),table);
    }
    public async Task<List<Hit>> Search(float[] query, string text, string kind = "vector", int k = 3,
        string tenant = "a", string user = "alice", bool unsafeAuthorization = false, string table = "items")
    {
        Table(table); Searches++;
        string filter = unsafeAuthorization ? "@tenant IS NOT NULL AND @user IS NOT NULL" : "tenant=@tenant AND permitted_user=@user";
        string columns = "id,title,body,url";
        string sql = RetrievalQueries.Build(kind,table,filter,query.Length);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tenant",tenant);command.Parameters.AddWithValue("user",user);
        command.Parameters.AddWithValue("k",Math.Clamp(k,1,20));
        if (kind is not "keyword")command.Parameters.AddWithValue("q",VectorText(query));
        if (kind is "keyword" or "hybrid")command.Parameters.AddWithValue("text",text);
        await using var reader = await command.ExecuteReaderAsync();var hits = new List<Hit>();
        while(await reader.ReadAsync())hits.Add(new(reader.GetString(0),reader.GetString(1),reader.GetString(2),reader.GetString(3),reader.GetDouble(4)));
        return hits;
    }
    public async ValueTask DisposeAsync()
    {
        if (owner) await Execute($"DROP SCHEMA {schema} CASCADE");
        await connection.DisposeAsync();
    }
}
