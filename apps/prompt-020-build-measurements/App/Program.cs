System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
const string scenario = "BASELINE";
var total = 128.90m * 1.20m;
if (total != 154.68m) throw new Exception("Wrong receipt");
Console.WriteLine($"{scenario}: receipt {total:0.00}");
