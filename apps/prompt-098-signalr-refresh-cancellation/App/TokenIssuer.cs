using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
public sealed class TokenIssuer
{
    private readonly SymmetricSecurityKey key=new(RandomNumberGenerator.GetBytes(32));
    public string Issue(string subject,string role,string revision="1",string course="dotnet") =>
        new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("course-local","course-client",
            [new Claim(ClaimTypes.NameIdentifier,subject),new Claim(ClaimTypes.Name,subject),new Claim(ClaimTypes.Role,role),new Claim("revision",revision),new Claim("course",course)],
            expires:DateTime.UtcNow.AddMinutes(15),signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256)));
    public void Configure(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>
        {
            o.TokenValidationParameters=new TokenValidationParameters {ValidateIssuer=true,ValidIssuer="course-local",ValidateAudience=true,
                ValidAudience="course-client",ValidateIssuerSigningKey=true,IssuerSigningKey=key,ValidateLifetime=true,ClockSkew=TimeSpan.Zero};
        });builder.Services.AddAuthorization();
    }
}
