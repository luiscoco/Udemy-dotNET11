using Api;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/config", () => new { Configured = ConfigurationCheck.HasApiKey(builder.Configuration["Demo:ApiKey"]) });
app.Run();
