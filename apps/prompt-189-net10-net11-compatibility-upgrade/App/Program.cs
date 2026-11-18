using System.IO.Compression;
using System.Buffers.Binary;
Directory.CreateDirectory("artifacts");
string valid=Path.GetFullPath("artifacts/valid.zip"),corrupt=Path.GetFullPath("artifacts/crc-mismatch.zip");
using(var stream=File.Create(valid))
using(var archive=new ZipArchive(stream,ZipArchiveMode.Create))
using(var writer=new StreamWriter(archive.CreateEntry("course.txt",CompressionLevel.NoCompression).Open())) writer.Write("Course 11");
byte[] bytes=File.ReadAllBytes(valid);
for(int index=0;index<=bytes.Length-20;index++)
{
    uint signature=BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(index));
    if(signature==0x04034b50) bytes[index+14]^=1;
    if(signature==0x02014b50) bytes[index+16]^=1;
}
File.WriteAllBytes(corrupt,bytes);
var observations=new List<object>();
foreach(string target in new[]{"net10.0","net11.0"})
{
    string folder=Path.GetFullPath("artifacts/published/"+target);
    await DeploymentTools.Dotnet("publish","Reader","-c","Release","-f",target,"-r","win-x64","--self-contained","true","-o",folder);
    string executable=Path.Combine(folder,"Reader.exe");
    var accepted=await Harness.Run(executable,valid,"safe");
    Check.That(accepted.Code==0&&accepted.Text.Contains("Course 11"),target+" preserves valid archive behavior");
    string mode=args.FirstOrDefault()=="problem"?"legacy":"safe";
    var rejected=await Harness.Run(executable,corrupt,mode);
    Console.WriteLine(target+" "+rejected.Text);
    observations.Add(new {target,validExit=accepted.Code,corruptExit=rejected.Code,output=rejected.Text});
    Harness.Save("upgrade-observations.json",observations);
    int expected=target=="net10.0"?0:2;
    Check.That(rejected.Code==expected,"UPGRADE_COMPATIBILITY_REGRESSION import boundary handles the runtime behavior");
}
