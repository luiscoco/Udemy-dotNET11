using Microsoft.AspNetCore.Authorization;
public sealed record CourseDocument(int Id,string Tenant,string Owner,string Text);
public sealed class OwnDocumentRequirement:IAuthorizationRequirement {}
public sealed class OwnDocumentHandler:AuthorizationHandler<OwnDocumentRequirement,CourseDocument>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,OwnDocumentRequirement requirement,CourseDocument document)
    {
        var tenant=context.User.FindFirst("tenant")?.Value;
        var subject=context.User.FindFirst("sub")?.Value;
        if(context.User.Identity?.IsAuthenticated==true && tenant==document.Tenant && subject==document.Owner)
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
