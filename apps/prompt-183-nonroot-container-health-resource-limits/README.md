# Containerize with a non-root user, health checks and resource limits

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/docker/
- https://docs.docker.com/reference/dockerfile/#healthcheck

Docker Desktop in Linux-container mode. The container binds an ephemeral loopback port, uses UID 1654, a read-only root filesystem, dropped capabilities, no-new-privileges, a tmpfs and explicit CPU and memory limits. The image health command executes the application health probe. The fixture removes only its uniquely named container and image tag.
