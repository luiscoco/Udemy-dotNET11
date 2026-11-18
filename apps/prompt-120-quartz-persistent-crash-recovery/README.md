# Persistent jobs — Quartz recovery after a real process restart

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://www.quartz-scheduler.net/documentation/quartz-4.x/tutorial/job-stores.html
- https://www.quartz-scheduler.net/documentation/quartz-4.x/tutorial/more-about-jobs.html

The verification intentionally starts and kills only its own worker child process after the job-entry marker. Quartz uses a disposable SQLite job-store file, one scheduler instance at a time and RequestRecovery. This does not demonstrate SQLite clustering. The marker and business effect live under the invocation-specific artifacts folder.
