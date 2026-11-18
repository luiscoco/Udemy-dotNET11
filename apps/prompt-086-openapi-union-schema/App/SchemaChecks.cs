using System.Text.Json;
public static class SchemaChecks
{
    public static JsonElement Resolve(JsonElement root,JsonElement schema)
    {
        while(schema.TryGetProperty("$ref",out var reference))
        {
            string[] parts=reference.GetString()!.Split('/').Skip(1).ToArray(); schema=root;
            foreach(string part in parts) schema=schema.GetProperty(part.Replace("~1","/").Replace("~0","~"));
        }
        return schema;
    }
    public static string[] RequestCaseTypes(JsonElement root)
    {
        var schema=root.GetProperty("paths").GetProperty("/messages").GetProperty("post")
            .GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("schema");
        return Resolve(root,schema).GetProperty("anyOf").EnumerateArray()
            .Select(item=>Resolve(root,item).GetProperty("type").GetString()!).Order().ToArray();
    }
}
