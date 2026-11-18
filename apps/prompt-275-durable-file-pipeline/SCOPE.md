# Executable capstone scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual file IO and separate worker processes share a durable SQLite queue. Controlled exits occur after a committed claim and after output creation. Restart recovery requeues expired leases. Cancellation and a transient IO failure are retained as pending work; retry count is bounded. A content hash makes existing output replay safe and a unique effect key prevents duplicate logical records. Single-host teaching scope, no distributed exactly-once claim or filesystem/database atomic transaction.
