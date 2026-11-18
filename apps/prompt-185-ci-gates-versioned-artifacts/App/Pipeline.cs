using System.IO.Compression;
using System.Security.Cryptography;
public static class Pipeline
{
    public static async Task<int> Execute()
    {
        string version=File.ReadAllText("release-version.txt").Trim();
        Check.That(System.Text.RegularExpressions.Regex.IsMatch(version,@"^[0-9]+[.][0-9]+[.][0-9]+$"),"release version is valid");
        string[][] gates=[
            ["restore","Tests","--locked-mode"],
            ["build","Tests","-c","Release","--no-restore","-warnaserror"],
            ["test","--project","Tests/Tests.csproj","-c","Release","--no-build","--report-trx","--results-directory","artifacts/tests"],
            ["publish","src","-c","Release","--no-restore","-r","win-x64","--self-contained","false","-p:Version="+version,"-o","artifacts/published/catalog"]
        ];
        foreach(var gate in gates)
        {
            var result=await Harness.Run(Harness.Dotnet,gate);
            Console.WriteLine("GATE "+gate[0]+" exit="+result.Code);
            File.WriteAllText("artifacts/gate-"+gate[0]+".txt",result.Text);
            if(result.Code!=0) { Console.WriteLine(result.Text); return result.Code; }
        }
        string archive="artifacts/course-"+version+".zip";
        using(var stream=File.Create(archive))
        using(var zip=new ZipArchive(stream,ZipArchiveMode.Create))
            foreach(string file in Directory.EnumerateFiles("artifacts/published/catalog"))
                zip.CreateEntryFromFile(file,Path.GetFileName(file));
        var files=Directory.EnumerateFiles("artifacts/published/catalog").Select(file=>new {name=Path.GetFileName(file),sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)))}).ToArray();
        Harness.Save("artifact-manifest.json",new {version,rid="win-x64",archive,sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(archive))),files});
        Check.That(files.Length>0,"versioned deployment payload and hashes are recorded");
        return 0;
    }
}
