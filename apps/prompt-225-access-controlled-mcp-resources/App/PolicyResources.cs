using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using System.Security.Claims;
using System.ComponentModel;
public sealed class PolicyStore(bool bypassAcl=false)
{
    public bool Revoked { get; set; }
    public string Read(ClaimsPrincipal user,string tenant,string id)
    {
        if(!System.Text.RegularExpressions.Regex.IsMatch(id,"^[a-z][a-z0-9-]{0,30}$"))throw new McpException("Not found or not permitted.");
        bool permitted=user.Identity?.IsAuthenticated==true&&user.Identity.Name=="alice"&&user.FindFirst("tenant")?.Value==tenant&&tenant=="a"&&!Revoked;
        if((!permitted&&!bypassAcl)||id!="retention")throw new McpException("Not found or not permitted.");
        return "# Synthetic retention policy\nVersion: 1\nRetention: seven days.\nALICE-POLICY-A7";
    }
}
[McpServerResourceType]
public sealed class PolicyResources
{
    [McpServerResource(UriTemplate="docs://policies/{tenant}/{id}",Name="Tenant Policy",MimeType="text/markdown")]
    [Authorize(Policy="mcp.access"),Description("Read an authorized synthetic policy; version appears in the content. IDs are not enumerated.")]
    public static TextResourceContents Read(string tenant,string id,ClaimsPrincipal user,PolicyStore store)=>new()
    { Uri=$"docs://policies/{tenant}/{id}",MimeType="text/markdown",Text=store.Read(user,tenant,id) };
}
