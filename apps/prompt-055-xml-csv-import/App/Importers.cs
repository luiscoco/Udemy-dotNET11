using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
public sealed class ImportRow
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
}
public static class Importers
{
    public static decimal ReadXml(string text)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null,
            MaxCharactersInDocument = 4096
        };
        using var input = new StringReader(text);
        using var reader = XmlReader.Create(input, settings);
        var document = XDocument.Load(reader);
        return document.Root!.Elements("item").Sum(item =>
            decimal.Parse(item.Attribute("amount")!.Value, CultureInfo.InvariantCulture));
    }
    public static ImportRow[] ReadCsv(string text)
    {
        if (text.Length > 4096) throw new InvalidDataException("CSV input exceeds limit");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            DetectColumnCountChanges = true, MaxFieldSize = 256
        };
        using var input = new StringReader(text);
        using var csv = new CsvReader(input, config);
        var rows = csv.GetRecords<ImportRow>().Take(101).ToArray();
        if (rows.Length > 100 || rows.Any(row => row.Amount < 0))
            throw new InvalidDataException("CSV validation failed");
        return rows;
    }
}
