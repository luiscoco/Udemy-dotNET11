var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();
app.MapOpenApi();
app.MapGet("/courses/{id:int}", (int id) => id == 11
    ? Results.Ok(new CourseDto(11,"Contract testing")) : Results.NotFound())
    .Produces<CourseDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
app.Run();
public partial class Program { }
