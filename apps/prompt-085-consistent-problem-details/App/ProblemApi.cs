using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
public sealed record OrderInput(int Quantity);
public static class ProblemApi
{
    public static void Map(WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.MapPost("/orders",(OrderInput input) => input.Quantity>0 ? Results.Ok(new {input.Quantity}) :
            Results.ValidationProblem(new Dictionary<string,string[]> { ["Quantity"]=["Quantity must be positive."] }));
        app.MapGet("/orders/{id:int}",(int id) => id==1 ? Results.Ok(new {Id=1,Quantity=2}) : Results.NotFound());
        app.MapGet("/explode",Explode);
    }
    private static IResult Explode() => throw new InvalidOperationException("INTERNAL-FIXTURE-DETAIL");
}
