Line[] input = [new(2, 10m), new(1, 5m)];
IEnumerable<Line> values = new ObservedLines(input);
var observed = (ObservedLines)values;
if (args.FirstOrDefault() == "repeated")
{
    decimal first = values.NetTotal;
    decimal second = values.NetTotal;
    Console.WriteLine($"Two property reads visited {observed.Visits} lines");
    Check.That(observed.Visits == 2, "expected one traversal for the displayed total");
    return;
}
decimal total = values.CalculateNetTotal();
string firstDisplay = total.ToString();
string secondDisplay = total.ToString();
Check.That(total == 25m && firstDisplay == secondDisplay, "computed total is reused");
Check.That(observed.Visits == 2, "one explicit calculation traverses two lines");
Check.That(input[0] == new Line(2, 10m) && input[1] == new Line(1, 5m),
    "calculation preserves the invoice lines");
Console.WriteLine($"Net total {total}, visits {observed.Visits}");
