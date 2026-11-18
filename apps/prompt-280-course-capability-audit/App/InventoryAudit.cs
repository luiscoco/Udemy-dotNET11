using System.Text.Json;

public static class InventoryAudit
{
    public static object Run()
    {
        using var catalog = JsonDocument.Parse(File.ReadAllText("assets/catalog.json"));
        var entries = catalog.RootElement.EnumerateArray().ToArray();
        Check.That(entries.Length == 280 && entries.Select(e => e.GetProperty("number").GetInt32()).SequenceEqual(Enumerable.Range(1, 280)), "catalog accounts for all 280 requested prompts without gaps");
        string? workspace = null;
        for (var dir = new DirectoryInfo(Environment.CurrentDirectory); dir != null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "course-production")) && Directory.Exists(Path.Combine(dir.FullName, "apps")))
            {
                workspace = dir.FullName;
                break;
            }

        var rows = new List<object>();
        int missing = 0, verified = 0;
        using var snapshot = JsonDocument.Parse(File.ReadAllText("assets/inventory-snapshot.json"));
        foreach (var entry in entries)
        {
            int number = entry.GetProperty("number").GetInt32();
            string? app = null;
            bool passed = false;
            if (workspace != null)
            {
                app = Directory.GetDirectories(Path.Combine(workspace, "apps"), "prompt-" + number.ToString("000") + "-*").FirstOrDefault();
                string file = Path.Combine(app ?? "__absent__", "artifacts", "verification.json");
                if (File.Exists(file))
                {
                    using var report = JsonDocument.Parse(File.ReadAllText(file));
                    passed = report.RootElement.GetProperty("passed").GetBoolean();
                }
            }
            else
            {
                var saved = snapshot.RootElement.GetProperty("lessons")[number - 1];
                app = saved.GetProperty("application").GetString();
                passed = saved.GetProperty("verified").GetBoolean();
            }

            if (app == null)
                missing++;
            if (passed)
                verified++;
            rows.Add(new { number, application = app == null ? null : Path.GetFileName(app), verified = passed, gap = app == null ? "no runnable application found" : passed ? null : "verification missing or not yet passed" });
        }

        Console.WriteLine($"AUDIT catalog=280 verifiedApps={verified} missingApps={missing} source={(workspace == null ? "frozen provided snapshot" : "live workspace scan")}");
        var result = new
        {
            catalogCount = 280,
            verifiedApplications = verified,
            missingApplications = missing,
            source = workspace == null ? "frozen provided snapshot" : "live workspace scan",
            rows,
            scope = "Inventory verifies file/evidence presence, not semantic completeness of all 280 topics. First ten legacy lessons need the runnable-app upgrade."
        };
        Harness.Save("coverage-gaps.json", result);
        return result;
    }
}
