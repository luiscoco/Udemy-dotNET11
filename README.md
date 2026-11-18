# .NET 11 Masterclass: Build 280 Applications with AI

### Turn your next learning session into working code.

Build APIs, explore modern C#, create interactive interfaces, and develop AI applications through **280 practical implementation prompts**. Follow the Udemy lessons, open the matching project, and investigate how the code behaves—from the first successful run to the failure you learn to repair.

**By Luis Coco Enríquez · Udemy course companion repository**

**280 numbered lessons** · **Application source included** · **Guided exercises** · **Verification scripts and browser labs where supplied**

**[Explore all 280 lessons →](LESSONS.md)** · [Run your first example](#run-your-first-example) · [Choose your learning path](#choose-your-learning-path) · [Discover the capstones](#bring-the-skills-together)

---

## Build skills you can put to work

An AI assistant can help you write code. Understanding that code gives you the confidence to change it, test it, and decide whether it solves the problem.

This course brings those activities together. Use focused prompts to explore a capability, study the supplied implementation, run an experiment, and explain the result. As you progress, connect individual techniques into larger application scenarios.

| Your goal | What you will practice |
| --- | --- |
| **Write clearer C#** | Model domains, handle nullability, use collections and LINQ, manage resources, and reason about asynchronous work. |
| **Build dependable backends** | Develop ASP.NET Core APIs, validate requests, shape contracts, query data, and handle concurrency. |
| **Create interactive experiences** | Explore Blazor components, real-time communication, .NET MAUI, and Windows desktop exercises. |
| **Add useful AI capabilities** | Work with chat clients, streaming responses, structured outputs, embeddings, semantic search, and retrieval-augmented generation (RAG). |
| **Develop tool-using agents** | Explore agent workflows, Model Context Protocol (MCP), authorization, human approval, and recovery. |
| **Evaluate your decisions** | Practice testing, diagnostics, performance measurement, deployment, and AI quality checks. |

## Is this course for you?

- **C# developers** who want a structured route through .NET application development and AI integration.
- **Backend and full-stack developers** looking for practical exercises across APIs, data, security, interfaces, and operations.
- **Developers exploring AI** who want to connect models and tools to application behavior they can inspect and test.
- **Independent learners** who enjoy learning through small experiments before tackling capstone projects.

Basic programming knowledge and familiarity with C# will help you get more from the material. Start with the introductory lessons if SDK selection, project files, and the .NET CLI are new to you. Advanced lessons build on those foundations and introduce additional requirements.

## Your course roadmap

Follow the course order for a broad progression, or select a focused route below. The [complete lesson index](LESSONS.md) links directly to every application.

| Lessons | Focus | What you will explore |
| --- | --- | --- |
| **001–020** | Foundations and tooling | SDKs, project structure, debugging, build settings, package restores, and quality gates. |
| **021–040** | C# design | Domain models, validation, immutability, patterns, generics, and refactoring. |
| **041–060** | Data and I/O | Collections, LINQ, time, text, serialization, streams, files, and process output. |
| **061–080** | Async work and services | Cancellation, bounded concurrency, workers, dependency injection, configuration, resilience, and logging. |
| **081–100** | Web and real-time systems | APIs, OpenAPI, MVC, Razor Pages, gRPC, server-sent events, WebSockets, and SignalR. |
| **101–120** | Persistence and messaging | Entity Framework, SQL, transactions, caching, queues, and durable processing. |
| **121–140** | Security and Blazor | Identity, authorization, tenant boundaries, components, render modes, and browser interaction. |
| **141–160** | Client apps and architecture | MAUI, WPF, WinForms, modular design, Aspire, gateways, and recovery workflows. |
| **161–180** | Testing and performance | Unit, integration, component, browser, and contract testing; telemetry, diagnostics, benchmarks, and AOT tradeoffs. |
| **181–190** | Delivery and modernization | Publishing, containers, CI gates, Kubernetes, migrations, rollout, rollback, and upgrades. |
| **191–210** | AI applications and search | Chat clients, streaming, validated outputs, embeddings, vector search, grounded answers, and retrieval evaluation. |
| **211–230** | Agents and MCP | Sessions, memory boundaries, approvals, orchestration, tool discovery, resources, and protocol behavior. |
| **231–250** | AI evaluation and machine learning | Evaluation datasets, regression checks, telemetry, controlled rollouts, ML metrics, ONNX, and local inference. |
| **251–270** | Specialist and emerging capabilities | Analyzers, generators, interop, desktop and device scenarios, plus explicitly experimental API exercises. |
| **271–280** | Capstones | Application design, AI, real-time behavior, durability, isolation, security, and evidence review. |

## Bring the skills together

These final exercises give your learning a concrete destination. Explore their source and scope to see how several course topics meet in one scenario.

| Capstone | Skills to connect |
| --- | --- |
| [**Inventory application · 271**](apps/prompt-271-inventory-capstone/) | Apply application design techniques to an inventory scenario. |
| [**Permission-aware document assistant · 272**](apps/prompt-272-permission-aware-document-assistant/) | Connect document assistance with access boundaries. |
| [**Authorized support agent · 273**](apps/prompt-273-authorized-support-agent/) | Bring agent behavior and authorization together. |
| [**Real-time operations dashboard · 274**](apps/prompt-274-realtime-operations-dashboard/) | Explore an operations scenario with real-time updates. |
| [**Durable file pipeline · 275**](apps/prompt-275-durable-file-pipeline/) | Reason about processing that must survive interruption. |
| [**Tenant isolation, quotas, and audit · 276**](apps/prompt-276-tenant-isolation-quota-audit/) | Examine boundaries and accountability across tenants. |
| [**Authenticated offline MAUI · 277**](apps/prompt-277-authenticated-offline-maui/) | Combine authentication with an offline client scenario. |
| [**Upgrade, security review, and capability audit · 278–280**](LESSONS.md) | Assess compatibility, review an AI solution, and examine evidence for the capabilities you use. |

## Choose your learning path

| If you want to… | Suggested route |
| --- | --- |
| **Build a strong .NET foundation** | Start at 001, continue through 080, then choose a web, data, or client application topic. |
| **Focus on backend development** | Work through 071–130, then architecture in 151–160 and testing and delivery in 161–190. |
| **Build web and client interfaces** | Study web and real-time topics in 081–100, then Blazor and client apps in 131–150. |
| **Explore AI applications and agents** | Refresh hosting and configuration in 071–080, then follow 191–240 before selecting a capstone. |
| **Strengthen quality and operations** | Focus on 161–190, then AI evaluation in 231–240 and the final reviews in 278–280. |

These routes are starting points. Read each lesson's prerequisites before jumping ahead.

## Run your first example

### 1. Get the source

```powershell
git clone https://github.com/luiscoco/Udemy-dotNET11.git
cd Udemy-dotNET11
```

You can also download the repository through GitHub's **Code → Download ZIP** menu.

### 2. Prepare for your chosen lesson

Read its `README.md` or `README.txt` and inspect `global.json` when supplied. Install the SDK version that lesson requires. Check `SCOPE.md` for specialist exercises and follow any additional setup instructions.

The shared-build example below uses .NET SDK **11.0.100**, or a compatible patch allowed by its `global.json`. Its application needs no external NuGet packages. The verification script and browser lab also require Node.js.

### 3. Build and run the shared-build example

```powershell
cd apps/prompt-011-shared-build
dotnet --version
dotnet restore InvoiceSuite.slnx
dotnet build InvoiceSuite.slnx -c Release
dotnet run --project src/Checkout -c Release --no-build
dotnet run --project tests/Billing.Checks -c Release --no-build
```

**Look for:** an invoice total of `154.68` and **8 passing behavioral checks**. The [lesson README](apps/prompt-011-shared-build/README.md) explains the calculations and the build settings behind them.

To explore the same lesson through its browser lab:

```powershell
node lab-server.cjs
```

Open **http://127.0.0.1:4177**. Inspect the source, run the lesson actions, and follow the real process output. Stop the server with **Ctrl+C**.

To reproduce this lesson's scripted build, failure, and repair experiments:

```powershell
node verify.cjs
```

Run the verifier on a fresh exercise copy and stop the browser lab first. It deliberately changes controlled source files and restores them when finished.

## Make every lesson an experiment

1. **Watch and predict.** Follow the Udemy explanation, then predict the example's output before running it.
2. **Inspect the implementation.** Open the matching numbered folder and identify the files responsible for the behavior.
3. **Run and observe.** Use that lesson's commands or supplied verification script; compare the results with its expected behavior.
4. **Investigate a failure.** Follow a documented failure case and explain why the repair works.
5. **Extend the idea.** Change one requirement, add a useful check, and explain the tradeoff in your own words.

Use a separate working copy for your experiments. Some lesson scripts intentionally introduce failures or restore known source files.

### A reusable instruction for your AI assistant

Adapt this instruction when exploring a course topic:

```text
Act as a senior .NET engineer and practical teacher.

For the capability below:
1. Verify the relevant official documentation and state the SDK,
   workload, package, provider, and model versions where applicable.
2. Explain the capability and its important design tradeoffs.
3. Build the smallest complete, runnable example.
4. Provide the files, dependencies, and terminal commands.
5. Include a meaningful check, expected behavior, and one failure case.
6. Identify platform limitations and any preview or experimental APIs.
7. Distinguish code you executed from code you only generated.

Keep credentials outside source code.

Capability: [describe the lesson topic or your next requirement]
```

## What you get in this repository

- **280 numbered lesson folders** with the supplied application sources or introductory setup exercises.
- **Lesson-specific guidance**, including README files and additional scope or exercise notes where supplied.
- **Verification scripts and interactive browser labs** for lessons that include them.
- **Project configuration, package locks, and required small assets** where supplied.
- **A [complete application index](LESSONS.md)** so you can return to a topic when you need it.

Follow the video lessons on Udemy alongside these sources. This GitHub repository contains the companion code; course videos and production media are distributed separately.

<details>
<summary><strong>Setup, platforms, and verification details</strong></summary>

- Each lesson owns its SDK and package requirements. Follow its instructions rather than assuming one command builds the entire collection.
- Lessons 001–010 preserve the introductory practice bundles and their original execution caveats. Some exercises generate project templates through setup scripts.
- Later lessons may require additional workloads, native tools, containers, databases, hardware, external services, or separately configured credentials. Platform-specific examples identify their scope in the lesson files.
- Review the scope of AI exercises to understand whether they use local fixtures, a local model, or an external provider. External service access is configured separately.
- Preview and experimental exercises have their own compatibility constraints. Read their scope and version requirements before running them.
- Publishing this repository verified all 280 lesson folders and excluded videos and generated output. It did not rebuild every application. Consult each lesson's instructions and generate fresh verification evidence on your machine.
- MP4s and other production media, SDK installations, generated artifacts, and `bin`/`obj` outputs are excluded. Scripts generate local outputs as you work.

</details>

## Learn with Luis Coco Enríquez

Use the Udemy course for the guided explanations and demonstrations, and keep this repository beside your editor as your practical companion.

**Ready to build? [Choose your first lesson →](LESSONS.md)**

If you find a reproducible problem in the companion source, [open an issue](https://github.com/luiscoco/Udemy-dotNET11/issues) with the lesson number, your SDK version, the command you ran, and the relevant error output. Remove credentials and private data before sharing logs.
