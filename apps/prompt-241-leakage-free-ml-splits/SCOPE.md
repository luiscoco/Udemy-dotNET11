# Executable model lesson scope

.NET SDK 11.0.100 with separately pinned packages and model assets. All business fixtures are synthetic. Build, controlled failure and repair execute actual code.

Actual ML.NET grouped TrainTestSplit and fitted MinMax transformer on 120 synthetic rows. Entity IDs remain disjoint. A held-out extreme-value sentinel makes global preprocessing leakage observable. Fractions are approximate because whole groups are assigned together. Seeded repeatability is checked for identity inventories, not a guarantee of unchanged behavior across package versions. No statistical representativeness or real customer data.
