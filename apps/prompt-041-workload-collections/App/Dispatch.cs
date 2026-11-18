public sealed record Job(string Id, int Urgency);
public static class Dispatch
{
    public static Job[] OrderedUrgentJobs(Job[] jobs)
    {
        var queue = new PriorityQueue<Job, (int Urgency, int Sequence)>();
        for (int index = 0; index < jobs.Length; index++)
            queue.Enqueue(jobs[index], (jobs[index].Urgency, index));
        var result = new List<Job>();
        while (queue.TryDequeue(out var job, out _)) result.Add(job);
        return result.ToArray();
    }
}
