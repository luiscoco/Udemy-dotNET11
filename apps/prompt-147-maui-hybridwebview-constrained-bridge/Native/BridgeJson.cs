using System.Text.Json.Serialization;
namespace NativeCourse;
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
internal partial class BridgeJson : JsonSerializerContext { }
