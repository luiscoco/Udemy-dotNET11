using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
public static class UploadRoutes
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/files",Upload);
        app.MapGet("/files/{id:guid}",(Guid id,TransferStore store) => store.Files.TryGetValue(id,out var file)
            ? Results.File(file.Path,"application/octet-stream",file.DownloadName,enableRangeProcessing:true)
            : Results.NotFound());
    }
    private static async Task<IResult> Upload(HttpRequest request,TransferStore store,CancellationToken ct)
    {
        string name=request.Headers["X-File-Name"].ToString();
        if(name.Length is <1 or >80 || Path.GetFileName(name)!=name || name.Contains('\\') || !name.EndsWith(".txt",StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new {Error="Supply a safe .txt download name"});
        if(request.ContentLength>TransferStore.MaximumBytes) return Results.StatusCode(413);
        Guid id=Guid.NewGuid(); string path=Path.Combine(store.Root,id.ToString("N")+".upload"); bool committed=false;
        try
        {
            long total=0; byte[] buffer=new byte[4096];
            await using(var file=new FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None,4096,FileOptions.Asynchronous))
            {
                int read;
                while((read=await request.Body.ReadAsync(buffer,ct))>0)
                {
                    store.Entered.TrySetResult(); total+=read;
                    if(total>TransferStore.MaximumBytes) return Results.StatusCode(413);
                    await file.WriteAsync(buffer.AsMemory(0,read),ct);
                }
                if(total<7) return Results.BadRequest(new {Error="Missing COURSE signature"});
                file.Position=0; byte[] signature=new byte[7]; await file.ReadExactlyAsync(signature,ct);
                if(!signature.AsSpan().SequenceEqual("COURSE\n"u8)) return Results.BadRequest(new {Error="Invalid COURSE signature"});
            }
            store.Files[id]=new(id,path,name,total); committed=true;
            return Results.Created($"/files/{id}",new FileReceipt(id,total));
        }
        finally { if(!committed) { File.Delete(path); store.Cleaned.TrySetResult(); } }
    }
}
