# Multifactor authentication — TOTP and safe recovery

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/security/authentication/mfa
- https://learn.microsoft.com/aspnet/core/security/authentication/identity-enable-qrcodes

All users, tokens, email messages and attacks in this lesson are synthetic local fixtures. Requests target only owned loopback applications. No external account or production system is tested. Read the application-specific setup below.

The TOTP generator is an authenticator simulator used only by the verifier. Shared keys and recovery codes are never exposed by HTTP or written to logs. Real enrollment must deliver the authenticator secret securely to the verified user and store recovery codes according to the account-recovery policy.
