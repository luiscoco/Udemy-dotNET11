using System.Globalization;
using Microsoft.AspNetCore.Mvc;
[Route("info")]
public sealed class InfoController : Controller
{
    [HttpGet]
    public IActionResult Index()=>Json(new ProxyReply(Request.Scheme,HttpContext.Connection.RemoteIpAddress!.ToString(),
        CultureInfo.CurrentCulture.Name,1234.5m.ToString("N1")));
}
public sealed record ProxyReply(string Scheme,string ClientIp,string Culture,string Amount);
