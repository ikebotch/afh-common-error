# AGENTS.md

## Purpose
- This repository defines the AFH shared error SDK.
- Follow these rules unless a task explicitly says otherwise.

## General Editing Rules
- Make the smallest possible diff.
- Do not touch unrelated files.
- Do not revert unrelated changes.
- Do not reorganise projects or folders unless explicitly asked.
- Preserve public package behaviour unless explicitly asked to change it.
- Prefer focused edits over broad rewrites.

## Repository Root Structure Standard

```text
[repo-root]/
├─ .github/
│  └─ workflows/
├─ build/
├─ docs/
│  ├─ architecture/
│  ├─ development/
│  ├─ decisions/
│  └─ diagrams/
├─ eng/
├─ src/
│  ├─ AFH.Common.Errors/
│  ├─ AFH.Common.Errors.AzureFunctions/
│  ├─ AFH.Common.Errors.ApplicationInsights/
│  ├─ AFH.Common.Errors.Email/
│  └─ AFH.Common.Errors.EntityFramework/
├─ tests/
│  ├─ AFH.Common.Errors.Tests/
│  ├─ AFH.Common.Errors.AzureFunctions.Tests/
│  ├─ AFH.Common.Errors.ApplicationInsights.Tests/
│  ├─ AFH.Common.Errors.Email.Tests/
│  └─ AFH.Common.Errors.EntityFramework.Tests/
├─ .editorconfig
├─ .gitignore
├─ AGENTS.md
├─ Directory.Build.props
├─ Directory.Packages.props
├─ README.md
└─ AFH.Common.Errors.sln
```
