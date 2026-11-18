# File transfers / bounded streaming, validation and cancellation

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads
- https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The endpoint accepts raw request bytes, not buffered IFormFile or multipart form data. COURSE plus LF is a custom teaching signature, not malware scanning. Application-level 64 KiB limits cover both declared and chunked bodies; a Kestrel request-body cap provides another bound. Stored names are generated and private. Add real authorization, ownership, quotas and content scanning before deploying an upload service. A production storage lifecycle also needs reconciliation after process failure.
