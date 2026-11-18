Console.WriteLine("MEMORY value="+Caller.Run());
foreach(var a in typeof(Caller).Assembly.GetCustomAttributesData().Concat(typeof(Caller).Module.GetCustomAttributesData()))if(a.AttributeType.Name.Contains("MemorySafety"))Console.WriteLine(a);
public static class NativeLike {public static unsafe int ReadKnownValue(){unsafe{int value=42;int* address=&value;return *address;}}}
