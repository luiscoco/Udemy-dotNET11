# Executable extensibility lesson scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual trusted fixture DLL loads through a collectible AssemblyLoadContext and a host-shared contract assembly. Unload is requested and actual collection is verified with a weak reference outside a non-inlined plugin scope. A static plugin root reproduces failure. The hash is a pin for this owned fixture, not proof of publisher authenticity. AssemblyLoadContext is not a security/process sandbox; untrusted plugins must not run in this host.
