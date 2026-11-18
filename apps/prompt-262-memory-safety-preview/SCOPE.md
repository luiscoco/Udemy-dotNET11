# Executable preview evaluation scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

The selected compiler actually compiles a preview-language project with updated-memory-safety-rules and AllowUnsafeBlocks. A no-pointer unsafe member propagates a caller obligation, demonstrated by real compiler failure outside an unsafe block and repaired execution inside one. The assembly metadata is inspected. The bounded pointer never escapes its stack lifetime; no invalid memory is dereferenced. This is preview language evaluation, not a claim that all unsafe code becomes memory safe.
