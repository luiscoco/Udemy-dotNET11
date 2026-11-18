using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using var authority=new TokenAuthority();var document=new CourseDocument(1,"tenant-a","alice","Owned lesson notes");
await using var server=await ApiHarness.StartAsync(b=>
{
    authority.Configure(b.Services);b.Services.AddSingleton<IAuthorizationHandler,OwnDocumentHandler>();
    b.Services.AddAuthorization(o=>
    {
        o.AddPolicy("Read",p=>p.RequireAuthenticatedUser().RequireClaim("permission","read","edit"));
        o.AddPolicy("Edit",p=>p.RequireAuthenticatedUser().RequireClaim("permission","edit"));
        o.AddPolicy("OwnDocument",p=>p.AddRequirements(new OwnDocumentRequirement()));
    });
},app=>
{
    app.UseAuthentication();app.UseAuthorization();
    async Task<IResult> Access(HttpContext context,IAuthorizationService authorization,int id)
    {
        if(id!=document.Id || !(await authorization.AuthorizeAsync(context.User,document,"OwnDocument")).Succeeded)return Results.NotFound();
        return Results.Text(document.Text);
    }
    app.MapGet("/documents/{id:int}",Access).RequireAuthorization("Read");
    app.MapPost("/documents/{id:int}",Access).RequireAuthorization("Edit");
});
async Task<HttpStatusCode> Call(HttpMethod method,string? token,int id=1)
{
    using var request=new HttpRequestMessage(method,"/documents/"+id);
    if(token is not null)request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
    using var response=await server.Client.SendAsync(request);return response.StatusCode;
}
Check.That(await Call(HttpMethod.Get,null)==HttpStatusCode.Unauthorized,"anonymous request is challenged before resource access");
Check.That(await Call(HttpMethod.Get,authority.Issue())==HttpStatusCode.OK,"read policy plus tenant and owner checks permit the owned document");
var denied=await Call(HttpMethod.Post,authority.Issue());
if(args.FirstOrDefault()=="permission")Check.That(denied==HttpStatusCode.OK,"incorrect expectation: read permission permits editing");
Check.That(denied==HttpStatusCode.Forbidden,"authenticated reader without edit claim receives HTTP 403");
Check.That(await Call(HttpMethod.Post,authority.Issue(permission:"edit"))==HttpStatusCode.OK,"explicit edit permission and resource ownership both pass");
Check.That(await Call(HttpMethod.Get,authority.Issue(subject:"mallory"))==HttpStatusCode.NotFound,"another owner cannot observe the protected document");
Check.That(await Call(HttpMethod.Get,authority.Issue(tenant:"tenant-b"))==HttpStatusCode.NotFound,"a signed identity from another tenant is denied");
Check.That(await Call(HttpMethod.Get,authority.Issue(),999)==HttpStatusCode.NotFound,"missing and inaccessible resources use the same not-found boundary");
Console.WriteLine("service: policy authorization and resource ownership are separate explicit checks");
