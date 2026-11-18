using System.IO.Compression;
using System.Net.Http.Json;
await using var api=await ApiHarness.StartAsync(Policies.Configure,Policies.Map);
var first=(await api.Client.GetFromJsonAsync<PublicReply>("/public"))!;
var second=(await api.Client.GetFromJsonAsync<PublicReply>("/public"))!;
Check.That(first.Generation==second.Generation,"anonymous public output is reused from the output cache");
async Task<HttpResponseMessage> AsUser(string path,string user)
{ using var request=new HttpRequestMessage(HttpMethod.Get,path); request.Headers.Add("X-Test-User",user); return await api.Client.SendAsync(request); }
using var aliceOne=await AsUser("/me","alice"); using var aliceTwo=await AsUser("/me","alice"); using var bobResponse=await AsUser("/me","bob");
var alice=(await aliceOne.Content.ReadFromJsonAsync<PersonalReply>())!;
var again=(await aliceTwo.Content.ReadFromJsonAsync<PersonalReply>())!;
var bob=(await bobResponse.Content.ReadFromJsonAsync<PersonalReply>())!;
if(args.FirstOrDefault()=="cache") Check.That(alice.Generation==again.Generation,"personal responses are explicitly excluded from shared output caching");
Check.That(alice.User=="alice" && bob.User=="bob" && alice.Generation!=again.Generation && again.Generation!=bob.Generation,"personal output executes for each identity and is never reused as shared public output");
using var anonymous=await api.Client.GetAsync("/me"); Check.That((int)anonymous.StatusCode==401,"the personal route requires an authenticated fixture identity");
int[] codes=new int[3]; for(int i=0;i<3;i++) { using var response=await AsUser("/limited","alice"); codes[i]=(int)response.StatusCode; }
using var separate=await AsUser("/limited","bob");
Console.WriteLine("api: alice rate statuses "+string.Join(",",codes)+"; bob "+(int)separate.StatusCode);
if(args.FirstOrDefault()=="rate") Check.That(codes[2]==200,"the third request exceeds the selected two-per-window limit");
Check.That(codes.SequenceEqual(new[]{200,200,429}) && (int)separate.StatusCode==200,"rate limits are enforced in separate identity partitions");
using var gzipRequest=new HttpRequestMessage(HttpMethod.Get,"/text"); gzipRequest.Headers.AcceptEncoding.ParseAdd("gzip");
using var compressed=await api.Client.SendAsync(gzipRequest);
using var packed=new MemoryStream(await compressed.Content.ReadAsByteArrayAsync()); using var gzip=new GZipStream(packed,CompressionMode.Decompress);
using var reader=new StreamReader(gzip); string inflated=await reader.ReadToEndAsync();
using var plain=await api.Client.GetAsync("/text");
Check.That(compressed.Content.Headers.ContentEncoding.Contains("gzip") && inflated==new string('x',4096) && !plain.Content.Headers.ContentEncoding.Any(),"gzip negotiation and the uncompressed representation remain distinct");
Console.WriteLine($"api: public generation {first.Generation}; personal generations {alice.Generation}, {again.Generation}, {bob.Generation}; gzip bytes {packed.Length}");
