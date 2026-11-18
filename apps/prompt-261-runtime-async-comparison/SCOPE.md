# Executable preview evaluation scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Two isolated compiled .NET 11 variants execute the identical shared workload, report actual MethodImpl Async metadata, live stack traces and five post-warmup elapsed samples. Timings are descriptive local observations, not a general speedup claim. A deliberate application-level cancellation omission fails compatibility; it is not presented as a Runtime Async defect. Projects targeting net11.0 opt in with Features runtime-async=on; no legacy environment variable is used.
