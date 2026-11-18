# Executable model lesson scope

.NET SDK 11.0.100 with separately pinned packages and model assets. All business fixtures are synthetic. Build, controlled failure and repair execute actual code.

Actual SDCA regression, training-only normalization, a mean baseline computed only from training labels, and evaluation on identical held-out rows. Controlled inference loses its X feature by setting it to zero, exposing a real input-path regression. Model persistence is actually saved and loaded. Simple synthetic data illustrates metrics and does not establish real-world generalization.
