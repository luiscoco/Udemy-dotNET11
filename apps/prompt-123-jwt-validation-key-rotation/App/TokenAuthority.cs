using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
public sealed class TokenAuthority : IDisposable
{
    private readonly Dictionary<string,RSA> privateKeys=new();
    private SecurityKey[] trusted=[];
    public const string Issuer="https://issuer.course.test",Audience="course-api";
    public TokenAuthority(){privateKeys["A"]=RSA.Create(2048);privateKeys["B"]=RSA.Create(2048);Trust("A");}
    public void Trust(params string[] ids)=>trusted=ids.Select(id=>new RsaSecurityKey(privateKeys[id].ExportParameters(false)){KeyId=id}).ToArray();
    public string Issue(string subject="alice",string tenant="tenant-a",string permission="read",string key="A",string? issuer=null,string? audience=null,bool expired=false)
    {
        var claims=new[]{new Claim("sub",subject),new Claim("tenant",tenant),new Claim("permission",permission)};
        var jwt=new JwtSecurityToken(issuer??Issuer,audience??Audience,claims,
            notBefore:DateTime.UtcNow.AddMinutes(-5),expires:DateTime.UtcNow.AddMinutes(expired?-1:10),
            signingCredentials:new SigningCredentials(new RsaSecurityKey(privateKeys[key]){KeyId=key},SecurityAlgorithms.RsaSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    public void Configure(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>
        {
            o.MapInboundClaims=false;o.IncludeErrorDetails=false;
            o.TokenValidationParameters=new TokenValidationParameters
            {
                ValidateIssuer=true,ValidIssuer=Issuer,ValidateAudience=true,ValidAudience=Audience,
                ValidateLifetime=true,RequireExpirationTime=true,ClockSkew=TimeSpan.Zero,
                ValidateIssuerSigningKey=true,RequireSignedTokens=true,ValidAlgorithms=[SecurityAlgorithms.RsaSha256],
                NameClaimType="sub",IssuerSigningKeyResolver=(_,_,kid,_)=>trusted.Where(k=>k.KeyId==kid)
            };
        });services.AddAuthorization();
    }
    public void Dispose(){foreach(var key in privateKeys.Values)key.Dispose();}
}
