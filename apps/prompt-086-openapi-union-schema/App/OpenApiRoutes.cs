using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
public static class OpenApiRoutes
{
    public static void Map(WebApplication app)
    {
        app.MapOpenApi();
        app.MapPost("/messages",(Message body)=>TypedResults.Ok(body)).WithName("EchoMessage");
    }
}
