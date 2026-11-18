using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
[TestClass]
public sealed class BrowserTests
{
    [TestMethod]
    public async Task Authentication_validation_and_save_journey()
    {
        await using var app = CourseWeb.Create();
        await app.StartAsync();
        using var playwright = await Playwright.CreateAsync();
        string chrome = Environment.GetEnvironmentVariable("CHROME_PATH") ?? "C:/Program Files/Google/Chrome/Application/chrome.exe";
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless=true, ExecutablePath=chrome });
        var context = await browser.NewContextAsync(new() {ViewportSize=new(){Width=1100,Height=800}});
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(15000);
        page.SetDefaultNavigationTimeout(60000);
        string artifacts = Path.GetFullPath("../../../../artifacts", AppContext.BaseDirectory);
        Directory.CreateDirectory(artifacts);
        try
        {
            await page.GotoAsync(app.Urls.Single() + "/dashboard");
            await Expect(page.GetByRole(AriaRole.Heading,new(){Name="Sign in"})).ToBeVisibleAsync();
            await page.GetByLabel("Student name").FillAsync("student");
            await page.GetByLabel("Password",new(){Exact=true}).FillAsync("wrong-password");
            await page.GetByRole(AriaRole.Button,new(){Name="Sign in",Exact=true}).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Alert)).ToHaveTextAsync("Credentials were rejected.");
            await page.ScreenshotAsync(new(){Timeout=30000,Path=Path.Combine(artifacts,"browser-login-error.png")});
            await page.GetByLabel("Student name").FillAsync("student");
            await page.GetByLabel("Password",new(){Exact=true}).FillAsync("course-demo");
            await page.GetByRole(AriaRole.Button,new(){Name="Sign in",Exact=true}).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Heading,new(){Name="Your course"})).ToBeVisibleAsync();
            await page.GetByLabel("Course title").FillAsync("  ");
            var invalidSubmission = await page.RunAndWaitForResponseAsync(
                () => page.GetByRole(AriaRole.Button,new(){Name="Save course"}).ClickAsync(),
                response => response.Request.Method == "POST" && response.Url.EndsWith("/courses",StringComparison.Ordinal));
            Assert.AreEqual(400,invalidSubmission.Status,"SERVER_VALIDATION_REGRESSION: Whitespace submission must return HTTP 400.");
            await Expect(page.GetByRole(AriaRole.Alert)).ToHaveTextAsync("A course title is required.");
            await page.ScreenshotAsync(new(){Timeout=30000,Path=Path.Combine(artifacts,"browser-validation-error.png")});
            await page.GetByLabel("Course title").FillAsync("Playwright with .NET 11");
            await page.GetByRole(AriaRole.Button,new(){Name="Save course"}).ClickAsync();
            await Expect(page.GetByTestId("saved")).ToHaveTextAsync("Saved Playwright with .NET 11");
            await page.ScreenshotAsync(new(){Timeout=30000,Path=Path.Combine(artifacts,"browser-success.png")});
            await File.WriteAllTextAsync(Path.Combine(artifacts,"browser-ui-verification.json"),"{\"passed\":true,\"scenario\":\"playwright-authentication-validation-167\"}");
        }
        catch
        {
            try { await page.ScreenshotAsync(new(){Timeout=30000,Path=Path.Combine(artifacts,"browser-failure.png")}); }
            catch (PlaywrightException) { /* Preserve the original test exception if diagnostic capture fails. */ }
            throw;
        }
        finally { await context.CloseAsync(); await app.StopAsync(); }
    }
}
