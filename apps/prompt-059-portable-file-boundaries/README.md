# Portable file access / traversal and link boundaries

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/api/system.io.path.getrelativepath
- https://learn.microsoft.com/dotnet/api/system.io.filesysteminfo.resolvelinktarget

The fixture creates a Windows directory junction without requiring symbolic-link privileges, or a directory symbolic link on Unix. It points only to the sibling artifacts/imports-other demonstration folder. The application rejects links rather than resolving and trusting them. The input tree and its parent must be controlled by the application. A check followed by File.ReadAllText is not race-free against concurrent hostile filesystem changes; that scenario needs OS-specific handle-relative operations and access controls.
