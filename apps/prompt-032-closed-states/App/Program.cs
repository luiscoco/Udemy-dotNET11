var draft = new Draft(42);
var paid = Payments.Pay(draft, 154.68m);
if (args.FirstOrDefault() == "repeat")
{
    Payments.Pay(paid, 154.68m);
    return;
}
Check.That(paid.Number == 42 && paid.Total == 154.68m, "payment preserves order data");
OrderState[] states = [draft, paid, new Dispatched(42, "TRACK42"), new Cancelled(43)];
string[] expected = ["draft", "paid", "dispatched", "cancelled"];
Check.That(states.Select(StateView.Describe).SequenceEqual(expected), "every known state is described");
Check.That(StateView.Describe(null) == "missing state", "nullable state is handled");
foreach (var state in states) Console.WriteLine(StateView.Describe(state));
