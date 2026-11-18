using System.Text.Json;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
if(args.FirstOrDefault()=="worker")
{var request=JsonSerializer.Deserialize<WorkerRequest>((await Console.In.ReadLineAsync())!)!;Console.WriteLine(JsonSerializer.Serialize(KeyWorker.Execute(request)));return;}
var directory=Path.GetFullPath(Path.Combine("artifacts","key-ring-"+Guid.NewGuid().ToString("N")));
Check.That(KeyRepository.Restrict(directory),"new key repository grants access only to the fixture application identity");
await using var certificate=new SecureHarness();const string app="course129",purpose="session/v1",text="Synthetic course session";
var first=await KeyWorker.Run(directory,certificate.Certificate,new("protect",app,purpose,text));
var second=await KeyWorker.Run(directory,certificate.Certificate,new("unprotect",app,purpose,first.Value!));
Check.That(first.ProcessId!=second.ProcessId && second.Success && second.Value==text,"a separate process decrypts with the same persisted keys, application and purpose");
Check.That(Directory.GetFiles(directory,"key-*.xml").All(f=>File.ReadAllText(f).Contains("EncryptedData")),"persisted Data Protection keys are certificate-encrypted at rest");
Check.That(!(await KeyWorker.Run(directory,certificate.Certificate,new("unprotect","another-app",purpose,first.Value!))).Success,"application discriminator isolates another application");
Check.That(!(await KeyWorker.Run(directory,certificate.Certificate,new("unprotect",app,"another-purpose",first.Value!))).Success,"purpose isolates another protected-data use");
using var services=KeyRepository.Build(directory,certificate.Certificate,app);var keys=services.GetRequiredService<IKeyManager>();
var old=keys.GetAllKeys().Single();
var activation=DateTimeOffset.UtcNow.AddSeconds(1);keys.CreateNewKey(activation,activation.AddDays(30));
await Task.Delay(1300); // Accelerated explicit rotation for the isolated recording.
var fresh=await KeyWorker.Run(directory,certificate.Certificate,new("protect",app,purpose,text));
Check.That((await KeyWorker.Run(directory,certificate.Certificate,new("unprotect",app,purpose,first.Value!))).Success,"rotation retains older keys for existing protected sessions");
keys.RevokeKey(old.KeyId,"Explicit fixture revocation");
var revoked=await KeyWorker.Run(directory,certificate.Certificate,new("unprotect",app,purpose,first.Value!));
if(args.FirstOrDefault()=="revoke")Check.That(revoked.Success,"incorrect expectation: a revoked key still decrypts the session");
Check.That(!revoked.Success,"revoking the old key rejects old protected sessions");
Check.That((await KeyWorker.Run(directory,certificate.Certificate,new("unprotect",app,purpose,fresh.Value!))).Success,"newly protected session remains usable after old-key revocation");
Console.WriteLine("service: private encrypted key repository, separate worker processes, purpose isolation, rotation and revocation verified");
