using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
var store=new TransferStore();
await using var api=await ApiHarness.StartAsync(b=>b.Services.AddSingleton(store),UploadRoutes.Map);
async Task<HttpResponseMessage> SendAsync(HttpContent body,string name="lesson.txt",CancellationToken token=default)
{
    using var request=new HttpRequestMessage(HttpMethod.Post,"/files") {Content=body}; request.Headers.Add("X-File-Name",name);
    return await api.Client.SendAsync(request,token);
}
byte[] bytes=Encoding.UTF8.GetBytes("COURSE\nStreaming demonstration");
using var upload=await SendAsync(new ByteArrayContent(bytes)); var receipt=(await upload.Content.ReadFromJsonAsync<FileReceipt>())!;
using var download=await api.Client.GetAsync("/files/"+receipt.Id);
Check.That((int)upload.StatusCode==201 && (await download.Content.ReadAsByteArrayAsync()).SequenceEqual(bytes),"streamed storage round-trips the validated bytes without a client storage path");
using var rangeRequest=new HttpRequestMessage(HttpMethod.Get,"/files/"+receipt.Id); rangeRequest.Headers.Range=new RangeHeaderValue(7,11);
using var range=await api.Client.SendAsync(rangeRequest);
Check.That((int)range.StatusCode==206 && (await range.Content.ReadAsByteArrayAsync()).SequenceEqual(bytes.Skip(7).Take(5)),"the physical-file download supports the requested byte range");
using var invalid=await SendAsync(new ByteArrayContent("wrong-format"u8.ToArray()));
using var oversize=await SendAsync(new UploadContent(new byte[TransferStore.MaximumBytes+1]));
if(args.FirstOrDefault()=="size") Check.That((int)oversize.StatusCode==201,"a chunked body cannot bypass the streaming byte counter");
Check.That((int)invalid.StatusCode==400 && (int)oversize.StatusCode==413 && store.Files.Count==1,"signature and streaming size failures retain no published file");
Console.WriteLine($"api: upload {(int)upload.StatusCode}; range {(int)range.StatusCode}; invalid signature {(int)invalid.StatusCode}; oversized chunked body {(int)oversize.StatusCode}");
int before=Directory.GetFiles(store.Root).Length;
// A separate store gives this cancellation case fresh entry and cleanup signals.
var cancelStore=new TransferStore();
await using var cancelApi=await ApiHarness.StartAsync(b=>b.Services.AddSingleton(cancelStore),UploadRoutes.Map);
using var cancellation=new CancellationTokenSource(); using var content=new UploadContent("COURSE\npartial"u8.ToArray(),hold:true);
using var pendingRequest=new HttpRequestMessage(HttpMethod.Post,"/files") {Content=content}; pendingRequest.Headers.Add("X-File-Name","cancelled.txt");
Task<HttpResponseMessage> pending=cancelApi.Client.SendAsync(pendingRequest,cancellation.Token);
await cancelStore.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10)); cancellation.Cancel();
try { using var ignored=await pending; throw new Exception("Expected cancelled transfer"); } catch(OperationCanceledException) { }
await cancelStore.Cleaned.Task.WaitAsync(TimeSpan.FromSeconds(10));
Check.That(cancelStore.Files.IsEmpty && Directory.GetFiles(store.Root).Length==before,"request cancellation removes the incomplete stored file");
