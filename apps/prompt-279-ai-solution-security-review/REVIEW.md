# Review of the representative candidate

This authored fixture imitates a generated data-access proposal; no external solution is represented as audited.

Architecture: keep SQL in a repository with explicit tenant input from the authenticated caller. Connections are scoped to operations. Do not expose this input directly to an unauthenticated UI.
Security: SQL001 is demonstrated by a real injection into synthetic data, then repaired with parameters. Tests preserve the tenant predicate.
Maintainability: separate query policy from syntax inspection and host orchestration. The single static rule is intentionally narrow and requires reviewer judgment.
Compatibility: pinned SDK 11.0.100, locked dependencies, real compilation and execution.
Failure handling: test a missing record and cancelled operation. Production retry, timeout, audit and availability policies remain explicit review work.
