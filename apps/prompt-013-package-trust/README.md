# Prompt 13 — package trust and audit lab

Requires .NET 11.0.100, Node.js, and network access to nuget.org plus its advisory data. Run `node verify.cjs` for the complete controlled exercise. Run `node lab-server.cjs` for the local browser lab at http://127.0.0.1:4177.

The baseline uses explicitly selected Newtonsoft.Json 13.0.4, rather than claiming it is the latest version or free of every vulnerability. The controlled audit experiment temporarily selects known-vulnerable 12.0.1 and checks GHSA-5crp-9r3c-p9vr. It never submits an exploit payload: the app processes a short, fixed invoice with MaxDepth 32. Do not ship the intentionally vulnerable exercise state. The runner restores the reviewed baseline in finally.

The NU1903 gate, NU1020 floating-reference failure, and repair are verified with actual NuGet output. A 72-hour publication-age setting affects supported automatic selection; it is not a general restore ban. External audit-service errors must not be described as a clean result. Read EXERCISE.txt.

References: https://learn.microsoft.com/nuget/concepts/auditing-packages and https://learn.microsoft.com/nuget/consume-packages/minimum-package-publish-age and https://github.com/advisories/GHSA-5crp-9r3c-p9vr
