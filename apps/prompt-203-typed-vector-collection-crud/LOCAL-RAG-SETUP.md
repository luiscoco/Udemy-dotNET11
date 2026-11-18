# Local retrieval lab setup

Install .NET SDK 11.0.100, Node.js 24, Docker and Ollama. Start Ollama with ollama serve; download all-minilm, nomic-embed-text and qwen2.5:0.5b with ollama pull. Set COURSE_AI_ENDPOINT to http://127.0.0.1:11434 for a standard installation. The recording uses port 11439.

Start the pinned isolated database (demonstration credentials only):

```powershell
docker run -d --name dotnet-course-rag-student -p 127.0.0.1:15439:5432 -e POSTGRES_USER=course -e POSTGRES_PASSWORD=Course_Rag_11_local! -e POSTGRES_DB=course --memory=384m pgvector/pgvector@sha256:b513c48566e4777cd3370451400000c78eda6a08131d68f4c31d4a76f27bd8fd
docker exec dotnet-course-rag-student psql -U course -d course -c "CREATE EXTENSION IF NOT EXISTS vector;"
node verify.cjs
```

Wait for PostgreSQL readiness before the extension command. COURSE_RAG_CONNECTION can select a compatible local fixture database. Each run creates a uniquely named owned schema and removes only that schema. Use a dedicated teaching database: the fixture login owns its schemas. No production credentials are included. Embedding model metadata records actual digests, dimensions, normalization and version. Model output is untrusted. Model weights are downloaded separately and are not in the ZIP. Read PROVIDER-SCOPE.md for exact lesson limits.

For the browser lesson, install Chrome and run npm install --prefix tools. CHROME_PATH can select another installed Chrome executable. Run dotnet run --project App -c Release -- serve to keep its actual RAG interface running.
