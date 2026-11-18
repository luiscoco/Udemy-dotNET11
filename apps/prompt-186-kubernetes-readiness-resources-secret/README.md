# Deploy with readiness probes, resource settings and secure configuration

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/
- https://kubernetes.io/docs/concepts/configuration/secret/

Docker Desktop Linux containers, kind 0.33.0 and kubectl compatible with Kubernetes 1.35.8. Tools are discovered on PATH or in the workspace tool directory. Each run creates and removes its own uniquely named kind cluster and image. It uses an explicit isolated kubeconfig. The mounted token is synthetic. Deployment reports retain pod readiness, security settings, resource limits and ready endpoint counts. Local kubeconfigs and published binaries are excluded from delivery bundles.
