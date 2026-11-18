using Microsoft.Extensions.AI;
using System.Drawing;
using System.Security.Cryptography;
#pragma warning disable MEAI001 // The lesson explicitly evaluates the experimental image abstraction.
using IImageGenerator generator=new CpuImageGenerator();
var options=new ImageGenerationOptions{ModelId="segmind/tiny-sd",Count=1,ImageSize=new Size(256,256),MediaType=args.Contains("problem")?"image/jpeg":"image/png",ResponseFormat=ImageGenerationResponseFormat.Data};
var response=await generator.GenerateImagesAsync("An orange ceramic teapot on a blue table, studio product photograph",options);
var image=response.Contents.OfType<DataContent>().Single();byte[] bytes=image.Data.ToArray();
Check.That(image.MediaType=="image/png"&&bytes.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}),"actual provider output is a valid PNG signature");
string hash=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
File.AppendAllText("artifacts/image-history.jsonl",System.Text.Json.JsonSerializer.Serialize(new{sha256=hash,actualInference=true,model="segmind/tiny-sd",revision="cad0bd7495fa6c4bcca01b19a723dc91627fe84f"})+Environment.NewLine);
Console.WriteLine($"IMAGE actual model generated {bytes.Length} PNG bytes sha256={hash}");
Harness.Save("preview-evidence.json",new{passed=true,actualImageGenerator=true,actualInference=true,model="segmind/tiny-sd",revision="cad0bd7495fa6c4bcca01b19a723dc91627fe84f",width=256,height=256,steps=12,sha256=hash,bytes=bytes.Length,provider="local CPU Docker",format="image/png"});
