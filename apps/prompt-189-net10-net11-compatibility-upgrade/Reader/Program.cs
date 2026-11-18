using System.IO.Compression;
using System.Runtime.InteropServices;
Console.WriteLine(RuntimeInformation.FrameworkDescription);
try
{
    using var archive=ZipFile.OpenRead(args[0]);
    using var reader=new StreamReader(archive.GetEntry("course.txt")!.Open());
    string content=reader.ReadToEnd();
    Console.WriteLine("VALIDATED_CONTENT="+content);
    return 0;
}
catch(InvalidDataException error) when(args[1]=="safe")
{
    Console.WriteLine("REJECTED_CORRUPT_ARCHIVE "+error.GetType().Name);
    return 2;
}
