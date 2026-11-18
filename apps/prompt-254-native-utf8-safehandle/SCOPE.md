# Executable extensibility lesson scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual locally compiled Windows C DLL uses heap-owned UTF-8 buffers, explicit lengths, bounds checks and a live allocation counter. LibraryImport source marshalling and SafeHandle release are actually executed. The controlled omission is detected before a finally block cleans up the owned resource. This is not a claim of arbitrary native-library safety or cross-platform ABI support.
