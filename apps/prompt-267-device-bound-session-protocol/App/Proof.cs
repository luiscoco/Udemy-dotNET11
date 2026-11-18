using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
public static class Proof
{
    public static string Sign(ECDsa key,string challenge){
        var p=key.ExportParameters(false);
        string header=WebEncoders.Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new{typ="dbsc+jwt",alg="ES256",jwk=new{kty="EC",crv="P-256",x=WebEncoders.Base64UrlEncode(p.Q.X!),y=WebEncoders.Base64UrlEncode(p.Q.Y!)}}));
        string payload=WebEncoders.Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new{jti=challenge,iat=DateTimeOffset.UtcNow.ToUnixTimeSeconds()}));
        string input=header+"."+payload;
        byte[] signature=key.SignData(Encoding.ASCII.GetBytes(input),HashAlgorithmName.SHA256,DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        return input+"."+WebEncoders.Base64UrlEncode(signature);
    }
}
