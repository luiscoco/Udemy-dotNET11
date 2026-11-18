using System.Reflection.Emit;
using System.Text.Json;
var method=new DynamicMethod("Generated",typeof(int),Type.EmptyTypes);
Console.WriteLine(method.Name);
Console.WriteLine(JsonSerializer.Serialize(new { Course=11 }));
