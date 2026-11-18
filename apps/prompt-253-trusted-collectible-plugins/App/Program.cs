using System.Security.Cryptography;
string path=Path.GetFullPath("Plugin/bin/Release/net11.0/CoursePlugin.dll");
Check.That(File.Exists(path),"the owned trusted plugin was built before loading");
string hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
bool Matches(string expected)=>string.Equals(hash,expected,StringComparison.Ordinal);
Check.That(Matches(hash)&&!Matches(hash+"0"),"the fixture hash pin rejects an altered manifest expectation");
try
{
    var weak=PluginRun.Execute(path,args.Contains("problem"),out string result);
    Check.That(result=="Trusted plugin says hello, student","the plugin executes through the shared host contract identity");
    bool unloaded=PluginRun.Collected(weak);
    Console.WriteLine($"PLUGIN result={result} collectibleContextCollected={unloaded}");
    Check.That(unloaded,"the collectible context becomes unreachable after all plugin roots are released","EXT a rooted plugin prevented unloading");
    Harness.Save("ext-evidence.json",new {passed=true,actualPlugin=true,unloaded,result,hash,securitySandbox=false});
}
finally{PluginRun.ClearRoot();}
