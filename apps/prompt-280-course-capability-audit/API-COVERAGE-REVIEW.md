# Supplemental API coverage review

- **StringStream**: revalidated existing coverage. apps/prompt-057-memory-stream-adapters/App/Adapters.cs
- **ReadOnlyMemoryStream**: revalidated existing coverage. apps/prompt-057-memory-stream-adapters/App/Adapters.cs, apps/prompt-057-memory-stream-adapters/App/Program.cs
- **Process.RunAndCaptureTextAsync**: revalidated existing coverage. apps/prompt-058-child-process-capture/App/ChildRunner.cs
- **BitArray.PopCount**: new supplemental source-level omission addressed in lesson 280. 

Bounded lexical source inspection excluding lesson 280 and generated bin/obj. Earlier stream/process call sites were also manually inspected. Absence of this call pattern is a source-level gap, not proof of missing conceptual instruction. Four executable probes revalidate three previously covered APIs and add BitArray.PopCount. This is not an exhaustive semantic audit of every .NET 11 feature.

Official primary reference: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries . The current .NET 11 API probes execute in App/ApiAdditions.cs. Source hashes and the review date are retained in assets/api-coverage-review.json.
