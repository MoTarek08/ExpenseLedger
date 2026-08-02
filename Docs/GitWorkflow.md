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
- All developers push their completed work here.
- Contains the ongoing development history before production releases.

---

# Development Flow

## Prerequisites

Before starting development:

- The repository has already been cloned.
- All project dependencies have been configured.
- The project builds and runs successfully.

---

## 1. Create a Feature Branch

When starting a new feature:

- Create a new local branch from your current working branch.

---

## 2. Implement the Feature

Complete the implementation.

After finishing:

- Merge the feature branch back into your local working branch.
- Prefer a **rebase merge**, especially if the feature contains many commits.
- Delete the feature branch if it is no longer needed.

---

## 3. Synchronize with `origin/dev` (**Important**)

Before pushing:

- Merge the latest changes from `origin/dev` into your local working branch.
- Resolve any merge conflicts before continuing.

This prevents pushing work that conflicts with changes made by other developers.

---

## 4. Push

After resolving conflicts:

- Push your local branch.

At this point your local branch should contain:

- Your completed feature.
- All latest changes from `origin/dev`.

---

## 5. Complete the Merge

Accept the merge request on the remote repository.
