System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
switch (args.FirstOrDefault() ?? "verify")
{
    case "classes": ClassExample(); break;
    case "records": RecordExample(); break;
    case "structs": StructExample(); break;
    case "shallow": ShallowCopyExample(); break;
    case "verify":
        ClassExample();
        RecordExample();
        StructExample();
        ShallowCopyExample();
        break;
}

static void ClassExample()
{
    var order = new MutableOrder("Ada", 100m);
    var alias = order;
    alias.Net = 120m;
    Check.That(order.Net == 120m, "class assignment shares the order instance");
    Check.That(order != new MutableOrder("Ada", 120m), "separate class instances have separate identity");
    Console.WriteLine($"Shared class net {order.Net:0.00}");
}

static void RecordExample()
{
    var original = new OrderSnapshot("Ada", 100m);
    var revised = original with { Net = 120m };
    Check.That(original == new OrderSnapshot("Ada", 100m), "record equality compares declared values");
    Check.That(original.Net == 100m && revised.Net == 120m, "with creates a revised record");
}

static void StructExample()
{
    var original = new Money(100m, "EUR");
    var revised = original with { Amount = 120m };
    Check.That(original.Amount == 100m && revised.Amount == 120m, "value copies preserve the original money value");
    Check.That(default(Money).Currency is null, "default struct state needs consideration in domain design");
}

static void ShallowCopyExample()
{
    var original = new TaggedOrder("Ada", ["New"]);
    var revised = original with { Customer = "Grace" };
    revised.Tags.Add("Reviewed");
    Check.That(original.Tags.Count == 2, "record copies share a nested mutable list");
    var isolated = original with { Tags = new List<string>(original.Tags) };
    isolated.Tags.Add("Private");
    Check.That(original.Tags.Count == 2 && isolated.Tags.Count == 3, "explicit collection copying isolates later edits");
}
