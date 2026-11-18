if (args.FirstOrDefault() == "construct")
{
    var attribute = Attribute.GetCustomAttribute(typeof(InvoiceRequest), typeof(RuleAttribute));
    Console.WriteLine($"Constructed attributes {RuleAttribute.Created}");
    Check.That(RuleAttribute.Created == 0, "metadata inspection should avoid attribute construction");
    return;
}
var metadata = RuleMetadata.Read(typeof(InvoiceRequest).Assembly.Location);
Check.That(metadata == ("invoice.create", 100), "encoded constructor arguments were read");
Check.That(RuleAttribute.Created == 0, "metadata reading did not execute the attribute constructor");
Console.WriteLine($"Rule {metadata.Code}, maximum {metadata.Maximum}");
