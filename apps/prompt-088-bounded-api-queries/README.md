# Bounded queries / validate pagination, filtering and sorting before SQL

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/ef/core/querying/pagination
- https://learn.microsoft.com/ef/core/performance/efficient-querying

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

SQLite is used as a real relational fixture. The read interceptor verifies that rejected inputs do not execute a database read. Fixed bounds control this request shape; they do not guarantee a universal query cost on an arbitrarily large table. Contains filtering and count queries need production data and index analysis. SQLite asynchronous APIs do not imply asynchronous underlying file I/O.
