using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
public sealed class CourseFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICourseRepository>();
            services.AddSingleton<ICourseRepository, CourseRepository>();
        });
    }
}
[TestClass]
public sealed class ApiTests
{
    [TestMethod]
    public async Task Whitespace_title_is_rejected_through_the_HTTP_pipeline()
    {
        await using var factory = new CourseFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/courses", new {title="  "});
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Whitespace input must receive HTTP 400.");
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "title-required");
    }
    [TestMethod]
    public async Task Created_course_is_retrievable_in_the_same_factory()
    {
        await using var factory = new CourseFactory();
        using var client = factory.CreateClient();
        using var created = await client.PostAsJsonAsync("/courses", new {title=" Integration testing "});
        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode);
        var course = await client.GetFromJsonAsync<CourseDto>(created.Headers.Location);
        Assert.AreEqual("Integration testing", course!.Title);
    }
    [TestMethod]
    public async Task Separate_factories_have_independent_repository_state()
    {
        await using var first = new CourseFactory();
        using var firstClient = first.CreateClient();
        using var created = await firstClient.PostAsJsonAsync("/courses", new {title="Only in first"});
        await using var second = new CourseFactory();
        using var secondClient = second.CreateClient();
        using var missing = await secondClient.GetAsync(created.Headers.Location);
        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
    }
}
