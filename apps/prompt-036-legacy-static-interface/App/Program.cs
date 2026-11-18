string language = args.FirstOrDefault() == "previous" ? "14.0" : "15.0";
int exit = await LegacyRunner.Run(language);
if (exit != 0) Environment.Exit(exit);
Check.That(language == "15.0", "C# 15 compilation and the older runtime execution succeeded");
