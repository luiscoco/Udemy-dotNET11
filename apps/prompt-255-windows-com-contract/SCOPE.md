# Executable extensibility lesson scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual Windows COM registration, Scripting.Dictionary activation and IDispatch automation on a dedicated STA thread. CompareMode is configured before adding keys; the controlled omission changes observed case behavior. The RCW is private, unshared and released exactly once on its owner thread. Built-in Windows scripting component registration and x64 deployment are required. No registry changes, shell execution or external side effects. This JIT/RCW example does not claim Native AOT compatibility.
