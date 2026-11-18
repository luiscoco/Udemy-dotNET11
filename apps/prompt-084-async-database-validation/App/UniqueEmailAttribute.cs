using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
public sealed class UniqueEmailAttribute : AsyncValidationAttribute
{
    protected override ValidationResult? IsValid(object? value,ValidationContext context) =>
        throw new InvalidOperationException("This rule requires asynchronous validation.");
    protected override async Task<ValidationResult?> IsValidAsync(object? value,ValidationContext context,CancellationToken ct)
    {
        if(value is not string email) return ValidationResult.Success;
        var factory=context.GetRequiredService<IDbContextFactory<CourseDb>>();
        await using var db=await factory.CreateDbContextAsync(ct);
        bool exists=await db.Users.AnyAsync(row=>row.Email==email,ct);
        await context.GetRequiredService<ValidationGate>().AfterLookupAsync(email,ct);
        return exists ? new ValidationResult("Email is already registered.",[nameof(RegisterUser.Email)]) : ValidationResult.Success;
    }
}
public sealed class RegisterUser
{
    [Required,EmailAddress,UniqueEmail]
    public string Email {get;set;}="";
}
