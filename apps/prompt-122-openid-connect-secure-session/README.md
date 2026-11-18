# OpenID Connect — Real provider login and secure sessions

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/security/authentication/configure-oidc-web-authentication
- https://www.keycloak.org/securing-apps/oidc-layers

All users, tokens, email messages and attacks in this lesson are synthetic local fixtures. Requests target only owned loopback applications. No external account or production system is tested. Read the application-specific setup below.

Start docker compose -f infrastructure/compose.yaml up -d. Wait for http://127.0.0.1:18088/realms/course/.well-known/openid-configuration to respond. The RP uses fixed HTTPS port 18122; run only one copy of this lesson at a time. Keycloak start-dev and HTTP discovery are strictly loopback teaching settings. Production authority metadata must use HTTPS. The student is a synthetic fixture account; no real credentials are used. Install Chrome and npm install --prefix tools for the browser step. Stop the owned stack with docker compose -f infrastructure/compose.yaml down.
