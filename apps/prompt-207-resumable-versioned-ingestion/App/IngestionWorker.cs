public static class IngestionWorker
{
    public static async Task Run(string schema, bool broken, bool crash)
    {
        using var embeddings = new EmbeddingService();
        await using var store = await VectorStore.Open(207,schema);
        while (true)
        {
            int checkpoint = await store.Scalar<int>("SELECT last_seq FROM checkpoint WHERE singleton=1");
            await using var command = new Npgsql.NpgsqlCommand("SELECT seq,id,body,version,deleted FROM journal WHERE seq>@seq ORDER BY seq LIMIT 1",store.Connection);
            command.Parameters.AddWithValue("seq",checkpoint);
            int sequence,version; string id,body; bool deleted;
            await using (var reader=await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync()) break;
                sequence=reader.GetInt32(0);id=reader.GetString(1);body=reader.GetString(2);version=reader.GetInt32(3);deleted=reader.GetBoolean(4);
            }
            if (broken && crash && sequence==3)
            {
                await store.Execute("UPDATE checkpoint SET last_seq=@seq WHERE singleton=1",("seq",sequence));
                Console.WriteLine("INGEST bad checkpoint committed before vector update");
                Environment.Exit(42);
            }
            var embedded = deleted ? null : await embeddings.Generate(body);
            await using var transaction = await store.Connection.BeginTransactionAsync();
            if (deleted) await store.Delete(id);
            else await store.Upsert(new(new Document(id,id,body,Version:version),embedded!));
            await store.Execute("UPDATE checkpoint SET last_seq=@seq WHERE singleton=1",("seq",sequence));
            if (crash && sequence==3)
            {
                Console.WriteLine("INGEST controlled process crash before transaction commit");
                Environment.Exit(42);
            }
            await transaction.CommitAsync();
            Console.WriteLine($"INGEST committed seq={sequence} id={id} version={version} deleted={deleted}");
        }
    }
}
