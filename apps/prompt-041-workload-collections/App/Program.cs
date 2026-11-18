Job[] jobs = [new("A", 3), new("B", 1), new("C", 1)];
var ordered = new List<Job>(jobs);
var byId = jobs.ToDictionary(job => job.Id, StringComparer.OrdinalIgnoreCase);
var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
Check.That(ordered[0].Id == "A" && byId["b"].Urgency == 1, "list position and keyed lookup serve different needs");
Check.That(seen.Add("A") && !seen.Add("a"), "set rejects an equivalent duplicate");
var fifo = new Queue<Job>(jobs);
Check.That(fifo.Dequeue().Id == "A", "queue dispatches the first arrival");
var urgent = Dispatch.OrderedUrgentJobs(jobs);
Check.That(urgent.Select(job => job.Id).SequenceEqual(new[] { "B", "C", "A" }),
    "lower urgency numbers win and sequence breaks ties");
Console.WriteLine("Priority order " + string.Join(", ", urgent.Select(job => job.Id)));
if (args.FirstOrDefault() == "reversed")
{
    var wrong = new PriorityQueue<string, int>();
    wrong.Enqueue("critical", 9);
    wrong.Enqueue("routine", 1);
    string next = wrong.Dequeue();
    Console.WriteLine($"Observed next job {next}");
    Check.That(next == "critical", "expected the highest number to win");
}
