# Expand, migrate and contract

1. Capture and restore-test a backup before changing schema. Record integrity and row checks.
2. Add nullable Slug while preserving Title. Deploy new readers that fall back to Title.
3. Dual-write Title and Slug. Backfill existing rows and check invariants. Old writers can still create rows, so retain fallback during coexistence.
4. Retire old readers and writers, monitor, take a new verified backup, and review the contract migration.
5. Drop Title only after the compatibility window closes. Application rollback and database rollback have different prerequisites. Restore-test backups under the intended recovery budget.
