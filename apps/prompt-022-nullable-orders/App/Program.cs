var directory = new CustomerDirectory();
var found = directory.Find("A-1");
var missing = directory.Find("missing");
Console.WriteLine(CustomerDisplay.Describe(found));
Console.WriteLine(CustomerDisplay.Describe(missing));
Check.That(CustomerDisplay.Describe(found) == "Ada / Phone pending", "optional phone has explicit presentation behavior");
Check.That(CustomerDisplay.Describe(missing) == "Customer unavailable", "missing lookup is handled before dereference");
Check.That(CustomerDisplay.RequireName(" Ada ") == "Ada", "validated name is trimmed");
try
{
    CustomerDisplay.RequireName(null);
    throw new InvalidOperationException("Missing name was accepted");
}
catch (ArgumentException)
{
    Console.WriteLine("PASS required input is rejected at the boundary");
}
