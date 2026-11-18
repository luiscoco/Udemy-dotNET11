using Microsoft.EntityFrameworkCore;
public sealed class UserRow { public int Id {get;set;} public string Email {get;set;}=""; }
public sealed class CourseDb(DbContextOptions<CourseDb> options) : DbContext(options)
{
    public DbSet<UserRow> Users => Set<UserRow>();
    protected override void OnModelCreating(ModelBuilder model) => model.Entity<UserRow>().HasIndex(row=>row.Email).IsUnique();
}
public sealed class ValidationGate
{
    private int races;
    private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Entered {get;} = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Cancelled {get;} = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public async Task AfterLookupAsync(string email,CancellationToken ct)
    {
        if(email=="race@example.invalid") { if(Interlocked.Increment(ref races)==2) release.TrySetResult(); await release.Task.WaitAsync(ct); }
        if(email=="wait@example.invalid")
        {
            Entered.TrySetResult();
            try { await Task.Delay(Timeout.InfiniteTimeSpan,ct); }
            catch(OperationCanceledException) when(ct.IsCancellationRequested) { Cancelled.TrySetResult(); throw; }
        }
    }
}
