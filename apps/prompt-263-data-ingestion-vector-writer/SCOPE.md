# Executable preview evaluation scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual Microsoft.Extensions.DataIngestion preview library: MarkdownReader, HeaderChunker, tokenizer, custom provenance chunk processor and custom SQL vector chunk writer. Actual SQL Server vector(2) values are inserted and queried. The vectors are explicitly deterministic lexical fixtures, not neural semantic embeddings; enrichment is content hashing and source provenance, not a fabricated LLM summary. Missing enrichment makes the real pipeline result fail. Requires the owned loopback SQL Server 2025 fixture on port 14666; only a uniquely named owned tempdb table is created and removed.
