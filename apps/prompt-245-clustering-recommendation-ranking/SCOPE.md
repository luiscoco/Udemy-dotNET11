# Executable model lesson scope

.NET SDK 11.0.100 with separately pinned packages and model assets. All business fixtures are synthetic. Build, controlled failure and repair execute actual code.

Three separate real ML.NET pipelines: KMeans, matrix factorization and LightGBM ranking. Clustering DBI is an in-sample descriptive measure; recommendation RMSE uses held-out user/item pairs with known keys, not cold-start validation. Ranking preserves entire query groups. The ranking key must preserve valid test groups (handled explicitly by a shared query-key cardinality schema if needed). Synthetic features make the tasks simple; metric values do not compare quality across tasks.
