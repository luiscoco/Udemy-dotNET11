using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
public static class RegistrationApi
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/users",async (RegisterUser input,IDbContextFactory<CourseDb> factory,CancellationToken ct) =>
        {
            await using var db=await factory.CreateDbContextAsync(ct);
            var row=new UserRow {Email=input.Email}; db.Users.Add(row);
            try { await db.SaveChangesAsync(ct); }
            catch(DbUpdateException error) when(error.InnerException is SqliteException {SqliteErrorCode:19})
            { return Results.Problem(statusCode:409,title:"Email is already registered."); }
            return Results.Created($"/users/{row.Id}",new {row.Id,row.Email});
        });
    }
}
