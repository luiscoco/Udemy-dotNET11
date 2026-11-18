# Executable capstone scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Tenant identity comes from a validated signed JWT. A real EF global query filter isolates reads; SQLite transactions atomically update quota, insert data and write audit rows. Request headers cannot choose a tenant. Actual MeterListener measurements are inspected. Deliberately bypassing the EF filter reproduces a real data leak in the owned fixture. Per-tenant quota units are document counts, not billing. Global document IDs simplify this bounded sample; production identity, retention, monitoring export and operational policy require additional design.
