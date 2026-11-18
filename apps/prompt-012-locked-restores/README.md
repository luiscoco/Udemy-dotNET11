# Prompt 12 — reproducible restores

Requires .NET SDK 11.0.100 or compatible patch, and Node.js. All NuGet packages are built from the supplied course source into a local feed; no external package is required.

Run `node verify.cjs` for the complete build, package, restore, expected NU1004 failure, intentional lock update and verified checkout. It restores the baseline central version and lock afterward. Run `node lab-server.cjs` and open http://127.0.0.1:4177 for the same real commands in the browser. Follow the numbered buttons. Run only one experiment at a time, from an unmodified exercise copy.

Read EXERCISE.txt. Do not treat a lock file as proof of package trust or identical builds across different SDKs and environments. This exercise owns the generated artifacts/feed and package cache. Logs and pass/fail evidence are in artifacts/verification.json.
