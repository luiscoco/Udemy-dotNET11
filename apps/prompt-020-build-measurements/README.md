# Prompt 20 — measured cold and warm workflows

Requires .NET SDK 11.0.100 or compatible patch and Node.js. Run node verify.cjs for the complete verified exercise. Run node lab-server.cjs and open http://127.0.0.1:4177 for the interactive lab. Logs and exact command output are retained in artifacts/verification.json. Run one experiment at a time; preserve your own edits before rerunning an exercise. Read EXERCISE.txt.

This experiment stops shared .NET build servers; run it without concurrent .NET builds. Cold means cleaned project outputs and stopped build servers, not cleared OS caches. The measured intervals include CLI and build work. Warm no-op and compiling builds perform different work. Timing values are measured live and written to artifacts/measurements.csv; no predetermined numbers or speedup are used. Binary logs are from this controlled lesson.
