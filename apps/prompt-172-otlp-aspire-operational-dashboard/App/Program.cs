using Microsoft.Playwright;
const string image="mcr.microsoft.com/dotnet/aspire-dashboard@sha256:2778077b75b72b56787a5b429c1825cc2217390f0d8edbf55a02c325a33a83fd";
string name="course-172-"+Guid.NewGuid().ToString("N");
Directory.CreateDirectory("artifacts");
bool broken=args.FirstOrDefault()=="problem";
try
{
    var start=await Harness.Run("docker","run","--rm","-d","--name",name,"-p","127.0.0.1::18888","-p","127.0.0.1::18890","-e","ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true",image);
    Check.That(start.Code==0,"owned Aspire dashboard started "+start.Text);
    async Task<string> Port(string port)
    {
        var result=await Harness.Run("docker","port",name,port);
        Check.That(result.Code==0,"mapped loopback dashboard port");
        return "http://"+result.Text.Trim();
    }
    string web=await Port("18888/tcp"),otlp=await Port("18890/tcp");
    using var client=new HttpClient();
    bool ready=false;
    for(int i=0;i<100;i++)
    {
        try { using var response=await client.GetAsync(web); ready=response.IsSuccessStatusCode; }
        catch(HttpRequestException) { }
        if(ready) break;
        await Task.Delay(200);
    }
    Check.That(ready,"dashboard HTTP endpoint ready");
    DashboardExport.Send(otlp,broken);
    using var playwright=await Playwright.CreateAsync();
    await using var browser=await playwright.Chromium.LaunchAsync(new(){Channel="chrome",Headless=true});
    var page=await browser.NewPageAsync(new(){Locale="en-US",ViewportSize=new(){Width=1440,Height=900}});
    page.PageError+=(_,error)=>File.AppendAllText("artifacts/dashboard-browser-errors.txt",error+Environment.NewLine);
    await page.GotoAsync(web+"/traces");
    await page.GetByRole(AriaRole.Heading,new(){Name="Traces",Exact=true}).WaitForAsync(new(){State=WaitForSelectorState.Visible,Timeout=60000});
    if(!broken) await page.WaitForFunctionAsync("() => document.body.innerText.includes('course-dashboard')",null,new(){Timeout=60000});
    string traceText=await page.Locator("body").InnerTextAsync();
    File.WriteAllText("artifacts/dashboard-traces.txt",traceText);
    await page.ScreenshotAsync(new(){Path="artifacts/dashboard-traces.png",FullPage=true});
    Console.WriteLine(traceText);
    Check.That(traceText.Contains("course-dashboard"),"EXPORT_REGRESSION backend receives the exported service traces");
    await page.Locator("a[href='/structuredlogs']").ClickAsync();
    await page.WaitForFunctionAsync("() => document.body.innerText.includes('Course request')",null,new(){Timeout=30000});
    string logText=await page.Locator("body").InnerTextAsync();
    File.WriteAllText("artifacts/dashboard-logs.txt",logText);
    await page.ScreenshotAsync(new(){Path="artifacts/dashboard-logs.png",FullPage=true});
    Check.That(logText.Contains("Course request"),"dashboard displays actual structured log records");
    await page.Locator("a[href='/metrics']").ClickAsync();
    await page.WaitForFunctionAsync("() => document.body.innerText.includes('Metrics')",null,new(){Timeout=30000});
    File.WriteAllText("artifacts/dashboard-metrics.html",await page.ContentAsync());
    await page.Locator("fluent-tree-item[title='course.requests']").ClickAsync();
    await page.WaitForTimeoutAsync(1500);
    await page.ScreenshotAsync(new(){Path="artifacts/metric-selection-diagnostic.png",FullPage=true});
    File.WriteAllText("artifacts/metric-selection-diagnostic.html",await page.ContentAsync());
    File.WriteAllText("artifacts/metric-selection-layout.json",await page.EvaluateAsync<string>("() => JSON.stringify({url:location.href,bodyText:document.body.innerText,bodyStyle:getComputedStyle(document.body).cssText,rect:document.body.getBoundingClientRect().toJSON(),htmlStyle:document.documentElement.getAttribute('style')})"));
    await page.WaitForFunctionAsync("() => document.body.innerText.includes('course.requests')",null,new(){Timeout=30000});
    string metricText=await page.Locator("body").InnerTextAsync();
    File.WriteAllText("artifacts/dashboard-metrics.txt",metricText);
    await page.ScreenshotAsync(new(){Path="artifacts/dashboard-metrics.png",FullPage=true});
    Check.That(metricText.Contains("course.requests")&&page.Url.Contains("/metrics/resource/course-dashboard"),"dashboard displays the selected request metric");
    Harness.Save("dashboard-verification.json",new { passed=true,image,signals=new[]{"traces","logs","metrics"},exportedRequests=5,syntheticErrors=1 });
}
finally { await Harness.Run("docker","rm","-f",name); }
