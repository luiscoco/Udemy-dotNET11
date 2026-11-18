using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
public sealed class LocalTokens
{
    private readonly SymmetricSecurityKey key=new(RandomNumberGenerator.GetBytes(32));
    public void Register(IServiceCollection services,bool broadWritePolicy=false)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>
        {
            o.MapInboundClaims=false;
            o.TokenValidationParameters=new()
            {
                ValidateIssuer=true,ValidIssuer="course-local-issuer",ValidateAudience=true,ValidAudience="course-prediction",
                ValidateIssuerSigningKey=true,IssuerSigningKey=key,ValidateLifetime=true,ClockSkew=TimeSpan.Zero,
                NameClaimType="sub",RoleClaimType="role"
            };
        });
        services.AddAuthorization(o=>
        {
            o.AddPolicy("prediction.access",p=>p.RequireAuthenticatedUser().RequireClaim("scope","prediction.invoke"));
            o.AddPolicy("stock.write",p=>
            {
                p.RequireAuthenticatedUser();
                if(!broadWritePolicy)p.RequireRole("operator").RequireClaim("scope","mcp.write");
            });
        });
    }
    public string Issue(string subject="alice",string tenant="a",string role="reader",bool write=false,
        bool expired=false,string audience="course-prediction",bool wrongSignature=false,bool read=true)
    {
        var claims=new List<Claim> { new("sub",subject),new("tenant",tenant),new("role",role) };
        if(read)claims.Add(new("scope","prediction.invoke"));
        if(write)claims.Add(new("scope","mcp.write"));
        var signing=wrongSignature?new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)):key;
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("course-local-issuer",audience,claims,
            DateTime.UtcNow.AddMinutes(-20),expired?DateTime.UtcNow.AddMinutes(-10):DateTime.UtcNow.AddMinutes(5),
            new SigningCredentials(signing,SecurityAlgorithms.HmacSha256)));
    }
}
