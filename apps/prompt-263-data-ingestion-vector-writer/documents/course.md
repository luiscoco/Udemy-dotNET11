# Async operations

async code must preserve cancellation and asynchronous completion. Read actual process output to verify behavior.

# Cache boundaries

cache keys include tenant and document version. cache invalidation must match the application contract.
