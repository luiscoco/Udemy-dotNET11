# Executable model lesson scope

.NET SDK 11.0.100 with separately pinned packages and model assets. All business fixtures are synthetic. Build, controlled failure and repair execute actual code.

Actual SDCA predictions through an authenticated loopback ASP.NET Core API with ephemeral local HS256 test tokens, explicit model version, finite/range input checks and synchronized inference. A bounded 20-sample window compares numeric mean with the synthetic training reference, requiring ten samples and three reference standard deviations. This simple input-shift alert is not a model-quality or concept-drift diagnosis and performs no automatic retraining. Local test tokens are not a production OAuth server. No production deployment or distributed monitoring is claimed.
