# Executable model lesson scope

.NET SDK 11.0.100 with separately pinned packages and model assets. All business fixtures are synthetic. Build, controlled failure and repair execute actual code.

Actual ONNX Runtime 1.24.4 MNIST inference with a held-out image. CPU and requested DirectML sessions are warmed and timed for 20 calls; ORT profiling must show DmlExecutionProvider nodes to claim execution. The Intel Iris Xe driver on the recording machine executes DML nodes but returns an incorrect digit despite graph fusion being disabled. The strict numerical check rejects it and the application uses the validated CPU result. Executed hardware is not equivalent to accepted acceleration. No driver cause, GPU quality or speedup is claimed. Exact outputs and timings are retained; unsupported devices are recorded separately. Do not loosen output tolerance to hide this mismatch.
