using System.Text.Json.Serialization;
public sealed record CourseDto([property: JsonRequired] int Id, [property: JsonRequired] string Title);
