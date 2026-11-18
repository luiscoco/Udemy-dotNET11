using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
public sealed class CourseUser : IdentityUser {}
public sealed class AccountContext(DbContextOptions<AccountContext> options):IdentityDbContext<CourseUser>(options){}
public static class IdentitySetup
{
    public static void Add(IServiceCollection services,string file)
    {
        services.AddDbContext<AccountContext>(o=>o.UseSqlite($"Data Source={file};Pooling=False"));
        services.AddIdentity<CourseUser,IdentityRole>(o=>
        {
            o.Password.RequiredLength=12;o.User.RequireUniqueEmail=true;
            o.Lockout.MaxFailedAccessAttempts=3;o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(5);
            o.SignIn.RequireConfirmedEmail=true;
        }).AddEntityFrameworkStores<AccountContext>().AddDefaultTokenProviders();
        services.ConfigureApplicationCookie(o=>
        {
            o.Cookie.Name="__Host-CourseSession";o.Cookie.HttpOnly=true;
            o.Cookie.SecurePolicy=Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            o.Cookie.SameSite=Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            o.ExpireTimeSpan=TimeSpan.FromMinutes(20);o.SlidingExpiration=false;
            o.Events.OnRedirectToLogin=c=>{c.Response.StatusCode=401;return Task.CompletedTask;};
            o.Events.OnRedirectToAccessDenied=c=>{c.Response.StatusCode=403;return Task.CompletedTask;};
        });services.AddAuthorization();
    }
    public static async Task Initialize(IServiceProvider services)
    {using var scope=services.CreateScope();await scope.ServiceProvider.GetRequiredService<AccountContext>().Database.EnsureCreatedAsync();}
}
