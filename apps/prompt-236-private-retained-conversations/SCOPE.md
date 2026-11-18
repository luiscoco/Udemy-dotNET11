# Executable evaluation scope

.NET SDK 11.0.100. All input, identities, credentials, attacks and provider faults are synthetic fixtures. Controlled failures execute real code and return nonzero status. Evidence retains actual source, process output and measured outcomes.

Actual SQLite and authenticated loopback ASP.NET Core reads. Ephemeral HS256 tokens are synthetic test identity, not production OAuth. Time is an injected mutable fixture and seven-day expiry/purge are real SQL conditions. Raw messages are redacted before persistence; regex covers the displayed email and synthetic secret patterns only, not all personal data. No encryption-at-rest or forensic secure-erasure claim. IDs and trusted owner are parameterized; callers cannot select the owner.
