using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
[TestClass]
public sealed class ContractTests
{
    private static string TypeName(JsonElement value) => value.ValueKind == JsonValueKind.Array
        ? string.Join("|",value.EnumerateArray().Select(item => item.GetString()).Order(StringComparer.Ordinal))
        : value.GetString()!;
    [TestMethod]
    public async Task OpenAPI_course_shape_matches_the_reviewed_baseline()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        string document = await client.GetStringAsync("/openapi/v1.json");
        using var json = JsonDocument.Parse(document);
        var root = json.RootElement;
        var operation = root.GetProperty("paths").GetProperty("/courses/{id}").GetProperty("get");
        var schema = operation.GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema");
        if (schema.TryGetProperty("$ref", out var reference))
            schema = root.GetProperty("components").GetProperty("schemas").GetProperty(reference.GetString()!.Split('/').Last());
        var properties = schema.GetProperty("properties").EnumerateObject()
            .OrderBy(property => property.Name).ToDictionary(property => property.Name, property => new
            {
                type = TypeName(property.Value.GetProperty("type")),
                format = property.Value.TryGetProperty("format",out var format) ? format.GetString() : null,
                pattern = property.Value.TryGetProperty("pattern",out var pattern) ? pattern.GetString() : null
            });
        var required = schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order().ToArray();
        var observed = JsonSerializer.SerializeToNode(new {properties,required});
        var expected = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"ContractBaseline.json")));
        string artifacts = Path.GetFullPath("../../../../artifacts",AppContext.BaseDirectory);
        Directory.CreateDirectory(artifacts);
        await File.WriteAllTextAsync(Path.Combine(artifacts,"observed-openapi.json"),document);
        Assert.IsTrue(JsonNode.DeepEquals(expected,observed), "The reviewed OpenAPI course property names, types and required fields must remain stable.");
    }
    [TestMethod]
    public async Task Actual_response_matches_the_public_field_contract()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/courses/11");
        Assert.AreEqual(HttpStatusCode.OK,response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.AreEqual(11,body.RootElement.GetProperty("id").GetInt32());
        Assert.IsTrue(body.RootElement.TryGetProperty("title",out var title), "The wire response must keep the title field.");
        Assert.AreEqual("Contract testing",title.GetString());
        using var missing = await client.GetAsync("/courses/99");
        Assert.AreEqual(HttpStatusCode.NotFound,missing.StatusCode);
    }
}
