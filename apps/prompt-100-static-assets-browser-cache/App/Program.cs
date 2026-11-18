using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
var probe=new PipelineProbe();string html=await File.ReadAllTextAsync("App/page-template.html");
await using var api=await ApiHarness.StartAsync(b=>b.WebHost.UseStaticWebAssets(),app=>AssetRoutes.Map(app,probe,html));
if(args.FirstOrDefault()=="browser"){await BrowserRunner.RunAsync(api.Client.BaseAddress!.ToString(),"static");return;}
int before=probe.AfterRouting;using var plain=await api.Client.GetAsync("/css/site.css");string css=await plain.Content.ReadAsStringAsync();
Check.That((int)plain.StatusCode==200 && plain.Headers.ETag is not null,"the manifest asset is served with a content validator");
using var conditional=new HttpRequestMessage(HttpMethod.Get,"/css/site.css");conditional.Headers.IfNoneMatch.Add(plain.Headers.ETag!);
using var unchanged=await api.Client.SendAsync(conditional);
if(args.FirstOrDefault()=="validator")Check.That((int)unchanged.StatusCode==200,"an unchanged asset with a matching validator need not resend its body");
Check.That((int)unchanged.StatusCode==304,"the matching asset validator returns not modified");
using var compressedRequest=new HttpRequestMessage(HttpMethod.Get,"/css/site.css");compressedRequest.Headers.AcceptEncoding.ParseAdd("gzip");
using var compressed=await api.Client.SendAsync(compressedRequest);using var bytes=new MemoryStream(await compressed.Content.ReadAsByteArrayAsync());
using var gzip=new GZipStream(bytes,CompressionMode.Decompress);using var reader=new StreamReader(gzip);
Check.That(compressed.Content.Headers.ContentEncoding.Contains("gzip") && await reader.ReadToEndAsync()==css,"the build-time compressed representation expands to the original stylesheet");
Check.That(probe.AfterRouting==before,"short-circuited static assets skip the downstream application middleware");
Console.WriteLine($"browser: plain CSS bytes {css.Length}; gzip bytes {bytes.Length}; conditional status {(int)unchanged.StatusCode}; Cache-Control {plain.Headers.CacheControl}");
