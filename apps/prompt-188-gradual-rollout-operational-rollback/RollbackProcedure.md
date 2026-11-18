# Rollout and rollback

Retain the stable image identity and current configuration. Establish healthy baseline traffic. Route 20 percent of the controlled sample to the candidate. Inspect HTTP error counts before promotion.

If the canary gate fails, set candidate weight to zero, retain the stable deployment, execute the recovery sample and verify all requests succeed on the stable release. Preserve observations for investigation. Promote to 100 percent only after the gate passes. Database compatibility and durable writes need their own rollback plan.
