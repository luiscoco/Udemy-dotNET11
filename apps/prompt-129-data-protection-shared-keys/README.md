# Data Protection — Shared encrypted keys and rotation

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/security/data-protection/configuration/overview
- https://learn.microsoft.com/aspnet/core/security/data-protection/implementation/key-encryption-at-rest

All users, tokens, email messages and attacks in this lesson are synthetic local fixtures. Requests target only owned loopback applications. No external account or production system is tested. Read the application-specific setup below.

Separate worker processes read the same private key directory. The fixture passes an ephemeral certificate through child-process environment variables and never writes its private key into the repository. Production needs an independently protected and retained certificate or equivalent key-encryption mechanism. The fixture directory is restricted to the current Windows user, or Unix mode 0700.
