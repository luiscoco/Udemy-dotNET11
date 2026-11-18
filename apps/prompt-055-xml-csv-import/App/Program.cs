string csv = "Id,Name,Amount\n1,\"Coco, Luis\",12.50\n2,\"Line\nbreak\",7.50\n";
if (args.FirstOrDefault() == "split")
{
    var fields = csv.Split('\n')[1].Split(',');
    Console.WriteLine($"Naive first-row field count {fields.Length}");
    Check.That(fields.Length == 3, "quoted commas must remain inside a field");
    return;
}
if (args.FirstOrDefault() == "dtd")
{
    _ = Importers.ReadXml("<!DOCTYPE items [<!ENTITY external SYSTEM 'file:///not-a-real-input'>]><items>&external;</items>");
    return;
}
if (args.FirstOrDefault() == "malformed")
{
    _ = Importers.ReadCsv("Id,Name,Amount\n1,\"unfinished,12.50\n");
    return;
}
var rows = Importers.ReadCsv(csv);
Check.That(rows.Length == 2 && rows[0].Name == "Coco, Luis" && rows[1].Name.Contains('\n'),
    "the CSV parser preserves quoted commas and embedded newlines");
Check.That(rows.Sum(row => row.Amount) == 20m, "CSV decimals follow the declared invariant culture");
decimal xmlTotal = Importers.ReadXml("<items><item amount='12.50'/><item amount='7.50'/></items>");
Check.That(xmlTotal == 20m, "bounded XML imports the intended values with DTDs prohibited");
Console.WriteLine($"CSV rows {rows.Length}; CSV/XML totals 20.00");
