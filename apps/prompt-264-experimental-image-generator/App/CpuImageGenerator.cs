using Microsoft.Extensions.AI;
using System.Drawing;
#pragma warning disable MEAI001 // Explicit experimental image-generation evaluation.
public sealed class CpuImageGenerator:IImageGenerator
{
    public async Task<ImageGenerationResponse> GenerateAsync(ImageGenerationRequest request,ImageGenerationOptions? options=null,CancellationToken cancellationToken=default)
    {
        if(options?.MediaType!="image/png"||options.Count!=1||options.ImageSize!=new Size(256,256)||options.ModelId!="segmind/tiny-sd")throw new NotSupportedException("PREVIEW unsupported image options rejected");
        if(request.OriginalImages?.Any()==true)throw new NotSupportedException("This provider supports text-to-image only");
        string model=FindModel();Directory.CreateDirectory("artifacts");
        var result=await Harness.Run("docker","run","--rm","--network","none","--read-only","--tmpfs","/tmp:rw,size=256m","-e","HOME=/tmp","-v",model+":/model:ro","-v",Path.GetFullPath("Provider")+":/provider:ro","-v",Path.GetFullPath("artifacts")+":/out","dotnet-course/lesson264-diffusion:local","python","/provider/worker.py",request.Prompt!);
        Console.WriteLine(result.Text);Check.That(result.Code==0,"the actual CPU diffusion worker completed");
        File.WriteAllText("artifacts/provider-output.txt",result.Text);
        return new ImageGenerationResponse([new DataContent(await File.ReadAllBytesAsync("artifacts/generated.png",cancellationToken),"image/png")]);
    }
    public object? GetService(Type serviceType,object? serviceKey=null)=>serviceKey is null&&serviceType.IsInstanceOfType(this)?this:null;
    public void Dispose(){}
    private static string FindModel(){
        string? configured=Environment.GetEnvironmentVariable("COURSE_IMAGE_MODEL");if(configured!=null)return Path.GetFullPath(configured);
        for(var dir=new DirectoryInfo(Directory.GetCurrentDirectory());dir!=null;dir=dir.Parent){string candidate=Path.Combine(dir.FullName,".tools/preview-image/model");if(Directory.Exists(candidate))return candidate;}
        throw new DirectoryNotFoundException("Run Provider/setup.ps1 and set COURSE_IMAGE_MODEL to the downloaded model directory");
    }
}
