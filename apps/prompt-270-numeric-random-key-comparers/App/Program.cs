using System.Globalization;
double value=Math.PI;string hex=value.ToString("X",CultureInfo.InvariantCulture);
double parsed=double.Parse(hex,NumberStyles.HexFloat,CultureInfo.InvariantCulture);
Check.That(BitConverter.DoubleToInt64Bits(parsed)==BitConverter.DoubleToInt64Bits(value),"hexadecimal float parsing preserves the exact IEEE-754 bits");
var random=new Random(270);byte[] bytes=Enumerable.Range(0,128).Select(_=>random.NextInteger<byte>((byte)10)).ToArray();
Half[] fractions=Enumerable.Range(0,128).Select(_=>random.NextBinaryFloat<Half>()).ToArray();
Check.That(bytes.All(x=>x<10)&&fractions.All(x=>x>=Half.Zero&&x<Half.One),"generic random values respect integer and binary-float ranges");
var comparer=EqualityComparer<Person>.Create(p=>p?.Name??throw new ArgumentNullException(nameof(p)),args.Contains("problem")?StringComparer.Ordinal:StringComparer.OrdinalIgnoreCase);
var people=new HashSet<Person>(comparer){new("Ada",30),new("ADA",40),new("Grace",35)};
Console.WriteLine($"NUMERIC hex={hex} exactBits={BitConverter.DoubleToInt64Bits(parsed):X16} byteRange={bytes.Min()}..{bytes.Max()} distinctNames={people.Count}");
Check.That(people.Count==2,"case-insensitive key equality produces two identities","PREVIEW key-selector equality contract failed");
Check.That(comparer.GetHashCode(new("Ada",1))==comparer.GetHashCode(new("ADA",99)),"equal selected keys have equal hash codes independently of other fields");
Harness.Save("preview-evidence.json",new{passed=true,actualNet11Apis=true,hex,exactRoundtrip=true,integerSamples=bytes.Length,floatSamples=fractions.Length,distinct=people.Count,caseInsensitive=true});
public sealed record Person(string Name,int Age);

