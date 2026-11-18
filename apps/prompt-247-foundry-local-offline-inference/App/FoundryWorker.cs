using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Sockets;
using System.Text.Json;
public static class FoundryWorker
{
    public static async Task Execute()
    {
        string work=Environment.GetEnvironmentVariable("COURSE_FOUNDRY_WORKER_DIR")!;
        string path=Environment.GetEnvironmentVariable("COURSE_FOUNDRY_MODEL_PATH")!;
        bool denied=false;string? networkError=null;
        try{using var socket=new TcpClient();await socket.ConnectAsync("1.1.1.1",443).WaitAsync(TimeSpan.FromSeconds(3));}
        catch(SocketException e){networkError=e.SocketErrorCode.ToString();denied=e.SocketErrorCode==SocketError.AccessDenied;}
        if(!denied)throw new InvalidOperationException("Network denial must be the actual OS AccessDenied result");
        await FoundryLocalManager.CreateAsync(new Configuration
        {
            AppName="offline_course247",AppDataDir=work,ModelCacheDir=Path.Combine(work,"cache"),LogsDir=Path.Combine(work,"logs"),
            DisableNonessentialTelemetry=true,CatalogUrls=[("http://127.0.0.1:9/disabled",null)]
        },NullLogger.Instance);
        using var manager=FoundryLocalManager.Instance;
        var local=await manager.GetCatalogAsync(CatalogType.Local);
        var model=await local.RegisterModelAsync(path,"course-qwen:1",new ModelInfoBuilder().SetStringProperty("task","chat-completion"));
        await model.LoadAsync();string text;
        using(var session=new ChatSession(model))
        {
            session.SetOptions(new RequestOptions {Search=new SearchOptions {Temperature=0,DoSample=false,MaxOutputTokens=24}});
            using var request=new Request();request.AddItem(MessageItem.User("Say hello in one short sentence."));
            using var response=await session.ProcessRequestAsync(request,CancellationToken.None);
            text=string.Join(" ",response.OfType<MessageItem>().Where(m=>m.IsSimpleText()).Select(m=>m.GetSimpleText()));
        }
        await model.UnloadAsync();
        if(string.IsNullOrWhiteSpace(text))throw new InvalidOperationException("Native model produced no text");
        File.WriteAllText(Path.Combine(work,"worker-result.json"),JsonSerializer.Serialize(new {networkDenied=denied,networkError,text,
            actualFoundry=true,model="qwen2.5-0.5b-instruct-generic-cpu:4",sdk="2.5.0",modelUnloaded=true}));
    }
}
