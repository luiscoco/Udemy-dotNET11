public sealed record Document(string Id, string Title, string Body,
    string Tenant = "a", string User = "alice", int Version = 1)
{
    public string Url => "https://course.example.test/docs/" + Id;
}
public static class Corpus
{
    public static readonly Document[] Documents = [
        new("keys", "API key rotation", "Rotate API keys every 30 days. Store keys outside source code."),
        new("security", "Credential reviews", "Review API key rotation every month. Revoke old keys after verifying replacements."),
        new("backup", "Database restoration", "Restore the nightly database backup to a separate database. Verify row counts before cutover."),
        new("containers", "Container permissions", "Run containers as a non-root user. Grant only the filesystem permissions required by the process."),
        new("stream", "Streaming responses", "Stream response text incrementally. Flush each update and cancel work when the client disconnects."),
        new("vectors", "Vector compatibility", "Record the embedding model and dimensions. Rebuild the vector index before changing embedding models."),
        new("cache", "Private response cache", "Scope private cache entries by authenticated user. Bypass cache reads and writes for sensitive content."),
        new("tenant-b", "Other tenant key policy", "EXCLUDE_TENANT_B: rotate keys using another tenant policy.", "b"),
        new("bob-only", "Restricted key policy", "EXCLUDE_BOB_ONLY: rotate keys using a restricted document.", "a", "bob")
    ];
}
