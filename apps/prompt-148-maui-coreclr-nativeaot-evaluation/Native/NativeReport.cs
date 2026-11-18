using System.Text.Json.Serialization;
namespace NativeCourse;
internal sealed class NativeReport
{
    public bool passed { get; set; }
    public string mode { get; set; } = "";
    public string scenario { get; set; } = "native-runtime-148";
    public string evidenceKind { get; set; } = "Actual native application and runtime feature checks";
    public List<string> checks { get; set; } = [];
    public string? error { get; set; }
}
[JsonSerializable(typeof(NativeReport))]
internal partial class NativeJson : JsonSerializerContext { }
