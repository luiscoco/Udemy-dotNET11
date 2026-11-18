using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
bool invalid = args.FirstOrDefault() == "invalid";
var builder = Host.CreateApplicationBuilder();
builder.Logging.ClearProviders();
builder.Configuration.Sources.Clear();
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Import:Endpoint"] = invalid ? "" : "https://example.invalid/import",
    ["Import:BatchSize"] = invalid ? "0" : "3"
});
builder.Services.AddOptions<ImportOptions>().Bind(builder.Configuration.GetSection("Import"))
    .ValidateDataAnnotations()
    .Validate(options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var uri) && uri.Scheme == "https",
        "Import.Endpoint must use an absolute HTTPS URL.")
    .ValidateOnStart();
using var host = builder.Build();
try { await host.StartAsync(); }
catch (OptionsValidationException error)
{
    foreach (string message in error.Failures) Console.WriteLine("VALIDATION " + message);
    throw;
}
try
{
    var options = host.Services.GetRequiredService<IOptions<ImportOptions>>().Value;
    Check.That(options.BatchSize == 3 && options.Endpoint == "https://example.invalid/import",
        "validated startup exposes the intended strongly typed settings");
    Console.WriteLine($"Validated batch size {options.BatchSize}; scheme {new Uri(options.Endpoint).Scheme}");
}
finally { await host.StopAsync(); }
