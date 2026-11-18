using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
public sealed class CourseActions(IAuthorizationService authorization)
{
    public async Task<string> PublishAsync(ClaimsPrincipal user)
    {
        var allowed = await authorization.AuthorizeAsync(user, null, "edit");
        if (!allowed.Succeeded) throw new UnauthorizedAccessException("Editor permission is required.");
        return "Course published by the authorized editor.";
    }
}
