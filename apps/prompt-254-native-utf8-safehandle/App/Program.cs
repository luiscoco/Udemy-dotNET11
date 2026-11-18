await NativeBuild.Compile();string text="Olá .NET 11";
var handle=Native.Create(text);Check.That(!handle.IsInvalid,"native allocation returns an owned safe handle");
try
{
    var roundtrip=BufferRead.Read(handle);
    Check.That(roundtrip.Text==text&&roundtrip.Bytes>text.Length&&roundtrip.ShortRejected,"UTF-8 multibyte text round-trips and a short destination is rejected");
    if(!args.Contains("problem"))handle.Dispose();
    int live=Native.Live();Console.WriteLine($"NATIVE utf8Bytes={roundtrip.Bytes} managedCharacters={text.Length} liveAllocations={live}");
    Check.That(live==0,"the owned native allocation is released exactly once","EXT native allocation was not released");
    Harness.Save("ext-evidence.json",new {passed=true,actualNative=true,roundtrip.Text,roundtrip.Bytes,roundtrip.ShortRejected,live});
}
finally{handle.Dispose();}
