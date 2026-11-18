# Prompt 18 — run environment and live watch

Requires .NET SDK 11.0.100 or compatible patch and Node.js. Run node verify.cjs for the complete verified exercise. Run node lab-server.cjs and open http://127.0.0.1:4177 for the interactive lab. Logs and exact command output are retained in artifacts/verification.json. Run one experiment at a time; preserve your own edits before rerunning an exercise. Read EXERCISE.txt.

The two watch experiments start a bounded local watcher, wait for actual application output, edit only the exercise helper, verify WATCH_V2 and unchanged application PID, then stop their own process tree and restore the helper. They time out after 120 seconds if expected evidence is absent. The plain-run comparison assumes COURSE_MODE was not already inherited from your shell.
