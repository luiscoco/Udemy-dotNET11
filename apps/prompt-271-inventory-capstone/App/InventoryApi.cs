using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

public static class InventoryApi
{
    public static async Task<WebApplication> Create(Authority authority, string file, bool broken, bool serve = false)
    {
        var b = ApiSession.Builder();
        if (serve)
            b.WebHost.UseSetting("urls", Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:8080");
        authority.Configure(b.Services);
        b.Services.AddValidation();
        b.Services.AddDbContext<InventoryDb>(o => o.UseSqlite("Data Source=" + file));
        var app = b.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventoryDb>();
            await db.Database.EnsureCreatedAsync();
            if (!await db.Products.AnyAsync())
            {
                db.Products.Add(new());
                await db.SaveChangesAsync();
            }
        }

        app.MapGet("/inventory", async (InventoryDb db) => Results.Ok(await db.Products.SingleAsync())).RequireAuthorization();
        app.MapPost("/reserve", async (Reservation request, ClaimsPrincipal user, InventoryDb db) =>
        {
            string subject = user.FindFirstValue("sub")!;
            await using var transaction = await db.Database.BeginTransactionAsync();
            var previous = await db.Receipts.FindAsync(request.OperationId);
            if (previous != null)
                return previous.Subject == subject && previous.Quantity == request.Quantity ? Results.Ok(previous) : Results.Conflict();
            var query = db.Products.Where(p => p.Sku == "book");
            if (!broken)
                query = query.Where(p => p.Available >= request.Quantity);
            int changed = await query.ExecuteUpdateAsync(s => s.SetProperty(p => p.Available, p => p.Available - request.Quantity));
            if (changed == 0)
                return Results.Conflict();
            var receipt = new Receipt
            {
                OperationId = request.OperationId,
                Subject = subject,
                Quantity = request.Quantity,
                Remaining = await db.Products.Select(p => p.Available).SingleAsync()
            };
            db.Receipts.Add(receipt);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return Results.Ok(receipt);
        }).RequireAuthorization("write");
        return app;
    }
}
