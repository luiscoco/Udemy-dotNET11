# Executable extensibility lesson scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual IIncrementalGenerator uses semantic attribute discovery and validates the required event shape. Generated source is compiled to real IL and executed. Tracked incremental reasons demonstrate unchanged-input reuse within the actual Roslyn driver, not a full build-time benchmark. Invalid shape produces actual GEN252. The bounded generator supports only top-level nongeneric public partial classes with public string Id.
