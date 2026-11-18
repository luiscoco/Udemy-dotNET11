using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
public sealed record CreateTask(string Title);
public sealed record BoundTask(int Id, string Title, bool Verbose);
public sealed class RequestTrace
{
    public List<string> Events { get; } = [];
    public TaskCompletionSource Completed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}
public static class Pipeline
{
    public static void Map(WebApplication app)
    {
        app.Use(async (context,next) =>
        {
            var trace=context.RequestServices.GetRequiredService<RequestTrace>();
            trace.Events.Add("middleware-before"); context.Response.Headers["X-Course-Pipeline"]="middleware";
            try { await next(context); } finally { trace.Events.Add("middleware-after"); trace.Completed.TrySetResult(); }
        });
        var group=app.MapGroup("/api/v1/tasks").WithTags("Tasks");
        group.AddEndpointFilter(async (context,next) =>
        {
            var trace=context.HttpContext.RequestServices.GetRequiredService<RequestTrace>();
            trace.Events.Add("filter-before");
            var input=context.Arguments.OfType<CreateTask>().FirstOrDefault();
            if(input is null || string.IsNullOrWhiteSpace(input.Title) || input.Title.Length>60)
                return Results.ValidationProblem(new Dictionary<string,string[]> { ["Title"]=["Title must contain 1 to 60 characters."] });
            var result=await next(context); trace.Events.Add("filter-after"); return result;
        });
        group.MapPost("/{id:int}", (int id, bool? verbose, [FromBody] CreateTask input, RequestTrace trace) =>
        { trace.Events.Add("handler"); return TypedResults.Created($"/api/v1/tasks/{id}",new BoundTask(id,input.Title,verbose ?? false)); });
    }
}
