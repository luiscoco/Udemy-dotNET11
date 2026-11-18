# Executable preview evaluation scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual experimental Microsoft.Extensions.AI.IImageGenerator with a custom local CPU diffusion provider. Every successful action runs the author-published Segmind Tiny-SD neural model, not a prerecorded or procedural replacement. Model revision and generated PNG hash are retained. Fixed supported capabilities are one 256x256 PNG, twelve inference steps, seed 264, text-to-image only; unsupported JPEG options actually fail. No editing, streaming, cloud-provider parity, production image quality or deterministic output across hardware is claimed. Docker and separately downloaded model weights are required. The original published model configuration is preserved; the supplied demonstration prompt is a neutral teapot. Weights are not included in the student ZIP; see Provider/model-provenance.json and the model license.
