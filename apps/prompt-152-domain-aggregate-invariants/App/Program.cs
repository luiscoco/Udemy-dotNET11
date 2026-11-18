var cohort = new Cohort(1, new Money(100, "EUR"));
cohort.Enroll("Luis");
bool rejected = false;
try { cohort.Enroll("Ana"); }
catch (InvalidOperationException exception)
{
    rejected = true;
    Console.WriteLine("domain: " + exception.Message);
}
Check.That(rejected && cohort.Students.Count == 1, "capacity rejection leaves aggregate state consistent");
if (args.FirstOrDefault() == "overbook")
    Check.That(cohort.Students.Count == 2, "incorrect expectation: a full aggregate accepts another student");
try { cohort.Enroll("LUIS"); throw new Exception("Duplicate was accepted"); }
catch (InvalidOperationException) { Console.WriteLine("PASS case-insensitive duplicate rejected"); }
Check.That(new Money(100, "EUR") == cohort.Price, "value equality compares amount and currency");
Check.That(new ScholarshipPolicy().Quote(cohort, true).Amount == 50, "the domain policy calculates an eligible quote");
bool invalid = false;
try { _ = new Money(-1, "EUR"); }
catch (ArgumentException) { invalid = true; }
Check.That(invalid, "the value object prevents a negative price");
