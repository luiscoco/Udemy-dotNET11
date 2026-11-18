using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
#pragma warning disable ASP0031 // Explicit experimental DBSC evaluation.
using var certificateKey=RSA.Create(2048);
var request=new CertificateRequest("CN=localhost",certificateKey,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
var san=new SubjectAlternativeNameBuilder();san.AddIpAddress(IPAddress.Loopback);san.AddDnsName("localhost");request.CertificateExtensions.Add(san.Build());
using var temporary=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1),DateTimeOffset.UtcNow.AddHours(1));
using var certificate=X509CertificateLoader.LoadPkcs12(temporary.Export(X509ContentType.Pfx),null,X509KeyStorageFlags.Exportable);
var builder=WebApplication.CreateSlimBuilder();builder.WebHost.ConfigureKestrel(k=>k.Listen(IPAddress.Loopback,0,listen=>listen.UseHttps(certificate)));
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddAuthentication("Application").AddCookie("Application").AddDeviceBoundSession("Application",o=>o.ShortLivedCookieExpiration=TimeSpan.FromSeconds(20));
await using var app=builder.Build();app.UseAuthentication();int browserProofs=0;
app.Use(async(c,next)=>{if(c.Request.Headers.UserAgent.ToString().Contains("Chrome")&&c.Request.Headers.ContainsKey("Secure-Session-Response"))Interlocked.Increment(ref browserProofs);await next(c);});
app.MapGet("/login",async(HttpContext context)=>{await context.SignInAsync("Application",new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,"course-user")],"fixture")));await context.Response.WriteAsync("<h1>Device-bound session evaluation</h1><p>Actual experimental package registration header. Browser automatic proof behavior is observed separately from software-key protocol checks.</p>");});
await app.StartAsync();
try{
    string address=Endpoint.Address(app.Services);var cookies=new CookieContainer();
    using var handler=new HttpClientHandler{CookieContainer=cookies,ServerCertificateCustomValidationCallback=(_,cert,_,_)=>cert?.GetCertHashString()==certificate.GetCertHashString()};
    using var client=new HttpClient(handler){BaseAddress=new Uri(address)};
    var login=await client.GetAsync("/login");string header=login.Headers.GetValues("Secure-Session-Registration").Single();
    string challenge=Regex.Match(header,"challenge=\"([^\"]+)\"").Groups[1].Value;
    using var owner=ECDsa.Create(ECCurve.NamedCurves.nistP256);using var attacker=ECDsa.Create(ECCurve.NamedCurves.nistP256);
    async Task<HttpResponseMessage> Send(string url,string? session,string? proof){using var message=new HttpRequestMessage(HttpMethod.Post,url);if(session!=null)message.Headers.Add("Sec-Secure-Session-Id",session);if(proof!=null)message.Headers.Add("Secure-Session-Response",proof);return await client.SendAsync(message);}
    var registered=await Send("/.well-known/dbsc/registration",null,Proof.Sign(owner,challenge));
    Check.That(registered.IsSuccessStatusCode,"actual DBSC middleware registered the software public key");
    var instructions=JsonDocument.Parse(await registered.Content.ReadAsStringAsync()).RootElement;string session=instructions.GetProperty("session_identifier").GetString()!;
    var missing=await Send("/.well-known/dbsc/refresh",session,null);
    string refreshChallenge=Regex.Match(missing.Headers.GetValues("Secure-Session-Challenge").Single(),"\"([^\"]+)\"").Groups[1].Value;
    var rejected=await Send("/.well-known/dbsc/refresh",session,Proof.Sign(attacker,refreshChallenge));
    Check.That(!rejected.IsSuccessStatusCode,"actual middleware rejects a proof from a different key");
    var refreshed=await Send("/.well-known/dbsc/refresh",session,Proof.Sign(args.Contains("problem")?attacker:owner,refreshChallenge));
    Console.WriteLine($"DBSC registered={(int)registered.StatusCode} noProof={(int)missing.StatusCode} wrongKey={(int)rejected.StatusCode} finalRefresh={(int)refreshed.StatusCode}");
    Check.That(refreshed.IsSuccessStatusCode,"the registered key refreshed the actual session","PREVIEW device-bound refresh proof was rejected");
    var browser=await Harness.Run("node","browser-probe.cjs",address);Console.WriteLine(browser.Text);Check.That(browser.Code==0,"actual Chrome observed the registration advertisement");
    Harness.Save("preview-evidence.json",new{passed=true,actualDbscPackage=true,registration=(int)registered.StatusCode,missingProof=(int)missing.StatusCode,wrongKey=(int)rejected.StatusCode,refresh=(int)refreshed.StatusCode,softwareProtocolKey=true,hardwareBindingClaim=false,browserProofs,browser=browser.Text.Trim()});
}finally{await app.StopAsync();}
