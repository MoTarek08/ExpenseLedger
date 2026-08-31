# Git Workflow

## Branches

### `origin/main`

The production branch.

- Should contain only production-ready code.
- Keep the commit history clean and minimal.
- Never use it for day-to-day development.

---

### `origin/dev`

The shared development branch.

- The default branch for active development.
- All developers raise PRs here.
- Contains the ongoing development history before production releases.

---

# Development Flow

## Prerequisites

Before starting development:

- The repository has already been cloned.
- All project dependencies have been configured.
- The project builds and runs successfully.

---
## 0. Pull dev Branch into your Local
- Pull the deb branch locally, so you can make sure that you are working on the up-to-date version 

## 1. Create a Feature Branch

When starting a new feature:

- Create a new local branch from your current working branch.

---

## 2. Implement the Feature

Complete the implementation.

## 3. Push

After finishing implementation:

- **Squash merge** even on local just for consistency and clarity.
- Push the local feature branch to an indetical upstream branch on remote, which will be created at this point.

---

## 4. Raise PR to dev Branch

- Raise a PR from the remote feature branch to **dev** branch, which will kick off a CI pipeline
---

## 5. Merge manually

- After status checks has passed, you can merge manually to remote **dev**.

