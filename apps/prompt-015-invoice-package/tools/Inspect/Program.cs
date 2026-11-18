using System.IO.Compression;

var main = args[0];
var symbols = args[1];
using var package = ZipFile.OpenRead(main);
using var pdbs = ZipFile.OpenRead(symbols);
foreach (var entry in package.Entries) Console.WriteLine(entry.FullName);
if (!package.Entries.Any(e => e.FullName.EndsWith(".dll"))) throw new Exception("DLL missing");
if (!package.Entries.Any(e => e.FullName.EndsWith(".xml"))) throw new Exception("XML documentation missing");
if (!pdbs.Entries.Any(e => e.FullName.EndsWith(".pdb"))) throw new Exception("PDB missing");
Console.WriteLine("PASS: assembly, XML documentation and portable symbols found");
