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
│  ├─ AFH.Errors/
│  ├─ AFH.Errors.AzureFunctions/
│  ├─ AFH.Errors.ApplicationInsights/
│  ├─ AFH.Errors.Email/
│  └─ AFH.Errors.EntityFramework/
├─ tests/
│  ├─ AFH.Errors.Tests/
│  ├─ AFH.Errors.AzureFunctions.Tests/
│  ├─ AFH.Errors.ApplicationInsights.Tests/
│  ├─ AFH.Errors.Email.Tests/
│  └─ AFH.Errors.EntityFramework.Tests/
├─ .editorconfig
├─ .gitignore
├─ AGENTS.md
├─ Directory.Build.props
├─ Directory.Packages.props
├─ README.md
└─ AFH.Errors.sln
```
