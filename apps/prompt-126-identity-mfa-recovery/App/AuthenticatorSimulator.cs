using System.Buffers.Binary;
using System.Security.Cryptography;
public static class AuthenticatorSimulator
{
    // Test authenticator only. The account is validated by Identity's actual token provider.
    public static string Code(string base32)
    {
        const string alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";List<byte> bytes=[];int buffer=0,bits=0;
        foreach(char c in base32.ToUpperInvariant().TrimEnd('='))
        {buffer=(buffer<<5)|alphabet.IndexOf(c);bits+=5;if(bits>=8){bits-=8;bytes.Add((byte)(buffer>>bits));}}
        Span<byte> counter=stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counter,DateTimeOffset.UtcNow.ToUnixTimeSeconds()/30);
        var hash=HMACSHA1.HashData(bytes.ToArray(),counter);int offset=hash[^1]&15;
        uint value=BinaryPrimitives.ReadUInt32BigEndian(hash.AsSpan(offset,4))&0x7fffffff;
        return (value%1_000_000).ToString("D6");
    }
}
