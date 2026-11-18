# Executable preview evaluation scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual SQL Server 2025 Developer in an owned loopback-only Docker fixture, actual vector(3), DiskANN index and EF Core 11 VectorSearch/WithApproximate SQL execution. Exact top-five IDs are compared against measured approximate IDs. The controlled fault maps the wrong query vector; it is not presented as intrinsic ANN failure. Recall is a local synthetic fixture result, not a universal accuracy or speed claim. Requires the course SQL fixture on localhost:14666. The owned tempdb table and index are unique to each run and removed afterward. PREVIEW_FEATURES is enabled only in the owned container database; do not point this lab at production.

Observed compatibility: SQL Server 2025 RTM-CU11 17.0.5065.9 creates the actual DiskANN index but rejects EF Core 11 WITH APPROXIMATE syntax with SQL error 102. The app retains that probe and uses an explicit EF SqlQueryRaw legacy VECTOR_SEARCH adapter for actual ANN execution. The problem action disables this repair and surfaces the real dialect failure. The repaired fixture measured recall@5 against the exact IDs. The index/table lives in the owned CoursePreview266 database, not tempdb. No native-API compatibility or speedup is claimed.
