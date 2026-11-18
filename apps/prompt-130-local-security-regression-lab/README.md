# Security regression tests — Injection, SSRF, access and logs

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/ef/core/querying/sql-queries
- https://cheatsheetseries.owasp.org/cheatsheets/Server_Side_Request_Forgery_Prevention_Cheat_Sheet.html
- https://learn.microsoft.com/aspnet/core/security/authorization/resourcebased

All users, tokens, email messages and attacks in this lesson are synthetic local fixtures. Requests target only owned loopback applications. No external account or production system is tested. Read the application-specific setup below.

All injection inputs, internal endpoints and credential sentinels are synthetic. SSRF requests can reach only the owned fixed-IP TLS catalog fixture; no arbitrary external URL is fetched. This is an exact-endpoint client, not a general public-URL downloader. Cloud metadata addresses are rejected before any connection.
