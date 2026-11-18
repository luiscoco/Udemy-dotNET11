using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;
bool broken=args.Contains("problem");
Directory.CreateDirectory("artifacts");
var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;listener.Stop();
var builder=WebApplication.CreateSlimBuilder();builder.WebHost.UseUrls("http://127.0.0.1:"+port);
await using var app=builder.Build();int successes=0;int rejected=0;int position=0;object gate=new();
app.MapPost("/move",(Move request)=>{
    lock(gate){
    if(request.Version!="v1"){rejected++;return Results.Conflict(new{error="protocol version",expected="v1"});}
    if(request.X!=position)return Results.BadRequest(new{error="stale position"});
    successes++;return Results.Ok(new{x=++position,version="v1"});}
});
await app.StartAsync();
try {
    string engine=GameTool.Find();var version=await Harness.Run(engine,"--headless","--version");
    Console.WriteLine("GAME actual engine "+version.Text.Trim());
    string image=Path.GetFullPath("artifacts/native-ui.png");
    var run=await Harness.Run(engine,"--path",Path.GetFullPath("Game"),"--rendering-method","gl_compatibility","--position","-20000,-20000","--resolution","1000x650","--","http://127.0.0.1:"+port,broken?"v0":"v1",image);
    Console.WriteLine(run.Text);
    Check.That(File.Exists(image),"actual native Godot viewport was captured");
    UiEvidence.Record(image,new{engine=version.Text.Trim(),successes,rejected,broken,actualHttp=true});
    Check.That(run.Code==0&&successes==3&&rejected==0,"EXT game protocol version mismatch");
    Harness.Save("ext-evidence.json",new{passed=true,actualGodot=true,actualHttp=true,engine=version.Text.Trim(),successes,rejected,embeddedClr=false});
    Console.WriteLine("PASS real Godot HTTP client received three validated moves");
} finally {await app.StopAsync();}
public record Move(string Version,int X);

