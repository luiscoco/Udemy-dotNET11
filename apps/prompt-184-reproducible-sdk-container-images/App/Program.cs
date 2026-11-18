using System.Security.Cryptography;
Directory.CreateDirectory("artifacts/published");
var digests=new List<string>();
var observations=new List<object>();
for(int index=0;index<2;index++)
{
    string epoch=args.FirstOrDefault()=="problem"&&index==1?"1704067201":"1704067200";
    string archive=Path.GetFullPath($"artifacts/published/image-{index}.tar.gz");
    string digestFile=Path.GetFullPath($"artifacts/image-{index}-digest.txt");
    await DeploymentTools.Dotnet("build","Receipt","-c","Release","-t:Rebuild");
    await DeploymentTools.Dotnet("publish","Receipt","-c","Release","-r","linux-x64","--self-contained","false","/t:PublishContainer","-p:SOURCE_DATE_EPOCH="+epoch,"-p:ContainerArchiveOutputPath="+archive,"-p:DigestRecordPath="+digestFile);
    string digest=File.ReadAllText(digestFile).Trim();
    Check.That(digest.StartsWith("sha256:"),"SDK records an actual generated container digest");
    digests.Add(digest);
    string archiveHash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(archive)));
    observations.Add(new {index,epoch,digest,archiveHash});
    Console.WriteLine($"Publish {index+1}, SOURCE_DATE_EPOCH={epoch}, digest={digest}");
}
Harness.Save("reproducible-images.json",observations);
Check.That(digests[0]==digests[1],"IMAGE_DIGEST_REGRESSION equivalent source inputs reproduce the image identity");
