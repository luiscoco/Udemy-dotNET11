using System.Net.Http.Json;
public static class AuthProbe
{
    public static async Task<int> Initialize(HttpClient http)
    {
        using var request=new HttpRequestMessage(HttpMethod.Post,"/mcp");
        request.Headers.Accept.ParseAdd("application/json");request.Headers.Accept.ParseAdd("text/event-stream");
        request.Content=JsonContent.Create(new
        {
            jsonrpc="2.0",id=1,method="initialize",@params=new
            { protocolVersion=McpSession.Protocol,capabilities=new {},clientInfo=new { name="AuthProbe",version="1" } }
        });
        using var response=await http.SendAsync(request);return (int)response.StatusCode;
    }
}
