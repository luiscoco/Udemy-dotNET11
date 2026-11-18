# .NET 11: 280 AI-Powered Implementation Prompts

Companion application sources for the Udemy .NET 11 course by Luis Coco Enríquez.

Browse [all 280 applications](LESSONS.md), from the introductory SDK exercises to the final capstone audit. Each lesson has its own folder under [apps](apps/).

## Getting started

Install the .NET SDK required by the selected lesson. Read its README.md or README.txt, global.json (when supplied), and any SCOPE.md before running it. Some lessons require Node.js, additional workloads, native tools, or external services. Package versions and SDK selection belong to each individual application.

For lessons with the verification and browser lab scripts:

```powershell
cd apps/prompt-011-shared-build
node verify.cjs
node lab-server.cjs
```

Open the loopback address printed by the lab server. Consult the lesson README for application-specific build and run commands. Use a fresh copy for exercises that deliberately introduce failures or reset source files.

## Contents

- Lessons 1–10: introductory practice projects and PowerShell exercises, preserving their original instructions and validation caveats.
- Lessons 11–280: application source, package locks, lesson documentation, verification scripts, browser labs, and required small assets where supplied.
- [LESSONS.md](LESSONS.md): complete numbered application index.

The introductory exercises were copied from their course practice bundles; lessons 11–280 were copied from the authored application folders. This publication checks source completeness and excluded files; it does not claim that all applications were rebuilt as part of publication. Follow each lesson's requirements and verification instructions.

MP4 videos, other production media, SDK installations, generated artifacts, and bin/obj outputs are excluded. Run the lesson scripts to generate fresh local evidence.
