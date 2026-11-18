# Executable model lesson scope

.NET SDK 11.0.100 with separately pinned packages and model assets. All business fixtures are synthetic. Build, controlled failure and repair execute actual code.

Actual Foundry Local 2.5.0 CPU inference uses a pre-acquired qwen2.5-0.5b-instruct-generic-cpu version 4 model registered through the local catalog. A fresh self-contained Windows AppContainer worker has zero network capabilities and must observe OS SocketError.AccessDenied before inference; this is not a proxy-only or already-loaded-model offline claim. Temporary scoped ACL grants and profile are cleaned up. The SDK may attempt minimal telemetry but OS network denial applies to the worker. CPU variant selection and model acquisition occur separately online. The model is unloaded after session disposal. No remote provider fallback or fabricated answer.
