using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
string root=Path.GetFullPath(args.FirstOrDefault()??"artifacts/foundry-cache");
await FoundryLocalManager.CreateAsync(new Configuration {AppName="course247_acquire",AppDataDir=root,
    ModelCacheDir=Path.Combine(root,"models"),DisableNonessentialTelemetry=true},NullLogger.Instance);
using var manager=FoundryLocalManager.Instance;var catalog=await manager.GetCatalogAsync();
var model=await catalog.GetModelVariantAsync("qwen2.5-0.5b-instruct-generic-cpu:4")??throw new Exception("Pinned CPU model variant unavailable");
await model.DownloadAsync();Console.WriteLine("Set COURSE_FOUNDRY_MODEL_PATH to: "+await model.GetPathAsync());
