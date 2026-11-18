using System.Security.Cryptography;
using System.Text;
#pragma warning disable SYSLIB5009 // Explicit opt-in to the experimental HPKE evaluation.
var suite=new HpkeSuite(HpkeKem.DHKEM_P256_HKDF_SHA256,HpkeKdf.HKDF_SHA256,HpkeAead.AES_128_GCM);
Check.That(Hpke.IsSupported(suite),"this host supports the selected classical HPKE suite");
using var recipient=Hpke.GenerateKey(suite);
using var sender=Hpke.ImportEncapsulationKey(suite,recipient.ExportEncapsulationKey());
byte[] info="course-268-v1"u8.ToArray(),aad="document:7"u8.ToArray();
sender.Seal("owned teaching payload"u8,out byte[] encapsulated,out byte[] ciphertext,info,aad);
bool tamperRejected=false;var tampered=ciphertext.ToArray();tampered[^1]^=1;
try{recipient.Open(encapsulated,tampered,info,aad);}catch(CryptographicException){tamperRejected=true;}
Check.That(tamperRejected,"real HPKE rejects modified ciphertext");
byte[] decoded;
try{decoded=recipient.Open(encapsulated,ciphertext,args.Contains("problem")?"wrong-context"u8.ToArray():info,aad);}
catch(CryptographicException e){throw new InvalidOperationException("PREVIEW HPKE context authentication failed",e);}
Check.That(Encoding.UTF8.GetString(decoded)=="owned teaching payload","real HPKE round-trip preserves the payload");
Console.WriteLine($"HPKE suite={suite.Name} ciphertextBytes={ciphertext.Length} tamperRejected={tamperRejected}");
Harness.Save("preview-evidence.json",new{passed=true,actualHpke=true,suite=suite.Name,tamperRejected,classicalKem=true,postQuantumClaim=false,ciphertextBytes=ciphertext.Length});
CryptographicOperations.ZeroMemory(decoded);
