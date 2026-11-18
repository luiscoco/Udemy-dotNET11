using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public sealed class Authority : IDisposable
{
    private readonly RSA key = RSA.Create(2048);
    public Authority()
    {
        if (Environment.GetEnvironmentVariable("CAPSTONE_PUBLIC_KEY")is string value)
            key.ImportSubjectPublicKeyInfo(Convert.FromBase64String(value), out _);
    }

    public string PublicKey => Convert.ToBase64String(key.ExportSubjectPublicKeyInfo());

    public string Issue(string subject = "alice", string tenant = "alpha", string permission = "write", bool expired = false)
    {
        var token = new JwtSecurityToken("course-capstone", "course-api", new[] { new Claim("sub", subject), new Claim("tenant", tenant), new Claim("permission", permission) }, notBefore: DateTime.UtcNow.AddMinutes(-20), expires: DateTime.UtcNow.AddMinutes(expired ? -1 : 10), signingCredentials: new SigningCredentials(new RsaSecurityKey(key), SecurityAlgorithms.RsaSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void Configure(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.IncludeErrorDetails = false;
            o.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = "course-capstone",
                ValidateAudience = true,
                ValidAudience = "course-api",
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(key),
                ClockSkew = TimeSpan.Zero,
                ValidAlgorithms = new[]
                {
                    SecurityAlgorithms.RsaSha256
                },
                NameClaimType = "sub"
            };
            o.Events = new JwtBearerEvents
            {
                OnMessageReceived = c =>
                {
                    if (c.Request.Path.StartsWithSegments("/hub") && c.Request.Query.TryGetValue("access_token", out var token))
                        c.Token = token;
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization(o => o.AddPolicy("write", p => p.RequireAuthenticatedUser().RequireClaim("permission", "write")));
    }

    public void Dispose() => key.Dispose();
}
