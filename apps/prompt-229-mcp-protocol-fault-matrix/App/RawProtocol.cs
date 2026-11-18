using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
public static class RawProtocol
{
    public static async Task<(int Status,string Body)> Post(HttpClient http,string body)
    {
        using var request=new HttpRequestMessage(HttpMethod.Post,"/mcp");
        request.Headers.Accept.ParseAdd("application/json");request.Headers.Accept.ParseAdd("text/event-stream");
        request.Headers.Add("MCP-Protocol-Version",McpSession.Protocol);
        request.Content=new StringContent(body,Encoding.UTF8,"application/json");
        using var response=await http.SendAsync(request);return ((int)response.StatusCode,await response.Content.ReadAsStringAsync());
    }
    public static int? ErrorCode(string body)
    {
        string json=body.Split('\n').LastOrDefault(l=>l.StartsWith("data:"))?[5..].Trim()??body;
        try { var data=JsonSerializer.Deserialize<JsonElement>(json);return data.TryGetProperty("error",out var error)?error.GetProperty("code").GetInt32():null; }
        catch(JsonException){return null;}
    }
}
