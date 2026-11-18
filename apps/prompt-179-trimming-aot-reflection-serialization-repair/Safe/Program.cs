using System.Text.Json;
using System.Text.Json.Serialization;
var registry=new Dictionary<string,Func<CourseDto>>
{
    ["course"]=()=>new CourseDto(11,"AOT")
};
string key=args.FirstOrDefault()??"course";
if(!registry.TryGetValue(key,out var create))
{
    Console.Error.WriteLine("Unknown course type");
    return 2;
}
CourseDto model=create();
string json=JsonSerializer.Serialize(model,CourseJson.Default.CourseDto);
CourseDto? restored=JsonSerializer.Deserialize(json,CourseJson.Default.CourseDto);
if(restored!=model) throw new InvalidOperationException("Round trip failed");
Console.WriteLine(json);
Console.WriteLine("PASS generated metadata, closed registry and normal delegate");
return 0;
public sealed record CourseDto(int Id,string Title);
[JsonSerializable(typeof(CourseDto))]
internal partial class CourseJson:JsonSerializerContext { }
