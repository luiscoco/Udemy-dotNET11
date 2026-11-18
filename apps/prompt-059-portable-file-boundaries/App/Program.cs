var fixture = Fixture.Create();
if (args.FirstOrDefault() == "traversal")
{
    _ = SafeFiles.ExistingFile(fixture.Root, "../imports-other/outside.txt");
    return;
}
if (args.FirstOrDefault() == "prefix")
{
    string outside = Path.Combine(fixture.Outside, "outside.txt");
    bool naive = outside.StartsWith(fixture.Root, StringComparison.Ordinal);
    Console.WriteLine($"Naive string-prefix decision {naive}");
    Check.That(!naive, "a shared name prefix must not imply directory containment");
    return;
}
if (args.FirstOrDefault() == "link")
{
    var link = new DirectoryInfo(Path.Combine(fixture.Root, "linked"));
    Console.WriteLine($"Resolved link target: {link.ResolveLinkTarget(true)!.Name}");
    _ = SafeFiles.ExistingFile(fixture.Root, "linked/outside.txt");
    return;
}
string valid = SafeFiles.ExistingFile(fixture.Root, "nested/lesson.txt");
Check.That(File.ReadAllText(valid) == "course input", "a normal nested file remains inside the owned import tree");
foreach (string rejected in new[] { "../imports-other/outside.txt", "linked/outside.txt" })
{
    bool blocked = false;
    try { _ = SafeFiles.ExistingFile(fixture.Root, rejected); }
    catch (UnauthorizedAccessException) { blocked = true; }
    Check.That(blocked, $"rejected {rejected}");
}
Console.WriteLine("Policy: reject links in an application-owned directory tree; concurrent hostile mutation is outside this helper's guarantee.");
