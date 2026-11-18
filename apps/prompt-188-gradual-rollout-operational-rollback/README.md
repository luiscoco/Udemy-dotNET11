# Implement gradual rollout, operational gates and rollback

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/azure/architecture/framework/devops/deployment-strategies
- https://learn.microsoft.com/azure/well-architected/operational-excellence/safe-deployments

Two actual versioned Docker application images run on separate ephemeral loopback ports. A local Kestrel gateway routes a deterministic 100-request sample. The canary threshold requires zero errors in this controlled fixture. A failed candidate is rolled back by restoring the stable route and verifying another sample. This local exercise records release identity, routing weights, status counts and recovery. It uses a synthetic business fault.
