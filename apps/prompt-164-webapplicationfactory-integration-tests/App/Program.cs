var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICourseRepository, CourseRepository>();
var app = builder.Build();
app.MapGet("/courses/{id:int}", (int id, ICourseRepository repository) =>
    repository.Find(id) is CourseDto course ? Results.Ok(course) : Results.NotFound());
app.MapPost("/courses", (CourseInput input, ICourseRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(input.Title))
        return Results.BadRequest(new { error = "title-required" });
    var course = repository.Add(input.Title.Trim());
    return Results.Created($"/courses/{course.Id}", course);
});
app.Run();
public partial class Program { }
public sealed record CourseInput(string? Title);
public sealed record CourseDto(int Id, string Title);
