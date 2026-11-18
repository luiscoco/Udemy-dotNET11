using System.Net.Http.Json;
using System.Text.Json;

public static class Deployment
{
    public static async Task Verify(Authority authority)
    {
        string directory = Path.GetFullPath("artifacts/published/linux-x64");
        var publish = await Harness.Run(Harness.Dotnet, "publish", "App", "-c", "Release", "-r", "linux-x64", "--self-contained", "false", "-p:UseAppHost=false", "-o", directory);
        Check.That(publish.Code == 0, "Linux framework-dependent deployment publishes", publish.Text);
        File.WriteAllText(Path.Combine(directory, "Dockerfile"), "FROM mcr.microsoft.com/dotnet/aspnet:11.0\nWORKDIR /app\nCOPY . .\nUSER root\nRUN mkdir /data && chown app /data\nUSER app\nENTRYPOINT [\"dotnet\",\"App.dll\",\"--serve\"]\n");
        string name = "dotnet-course-capstone271-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            var build = await Harness.Run("docker", "build", "-t", name, directory);
            Check.That(build.Code == 0, "owned application container image builds", build.Text);
            var run = await Harness.Run("docker", "run", "-d", "--name", name, "--label", "course.lesson=271", "-p", "127.0.0.1::8080", "-e", "CAPSTONE_PUBLIC_KEY=" + authority.PublicKey, "-e", "ASPNETCORE_URLS=http://0.0.0.0:8080", name);
            Check.That(run.Code == 0, "actual container starts", run.Text);
            var port = await Harness.Run("docker", "port", name, "8080/tcp");
            string address = "http://" + port.Text.Trim();
            using var http = new HttpClient
            {
                BaseAddress = new Uri(address)
            };
            bool ready = false;
            for (int i = 0; i < 80 && !ready; i++)
            {
                try
                {
                    ready = (int)(await http.GetAsync("/inventory")).StatusCode == 401;
                }
                catch (HttpRequestException)
                {
                }

                if (!ready)
                    await Task.Delay(250);
            }

            Check.That(ready, "actual deployed API rejects anonymous traffic");
            http.DefaultRequestHeaders.Authorization = new("Bearer", authority.Issue());
            var product = await http.GetFromJsonAsync<Product>("/inventory");
            Check.That(product?.Available == 8, "actual container serves authenticated EF-backed inventory");
            var image = await Harness.Run("docker", "image", "inspect", name, "--format", "{{.Id}}");
            Harness.Save("deployment-evidence.json", new { passed = true, actualContainer = true, image = image.Text.Trim(), anonymous = 401, authenticated = 200, port = address, scope = "owned ephemeral local container, no production deployment" });
        }
        finally
        {
            await Harness.Run("docker", "rm", "-f", name);
            await Harness.Run("docker", "image", "rm", name);
        }
    }
}
