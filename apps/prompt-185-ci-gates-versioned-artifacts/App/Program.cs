Directory.CreateDirectory("artifacts");
string original=File.ReadAllText("src/CourseLabel.cs");
try
{
    if(args.FirstOrDefault()=="problem")
        File.WriteAllText("src/CourseLabel.cs",original.Replace("ArgumentException.ThrowIfNullOrWhiteSpace(title);","// Controlled regression removes the null guard."));
    int exit=await Pipeline.Execute();
    Check.That(exit==0,"CI_ANALYSIS_REGRESSION all release gates must pass");
    Console.WriteLine("Verified locked restore, analysis, three tests and versioned artifact.");
}
finally { File.WriteAllText("src/CourseLabel.cs",original); }
