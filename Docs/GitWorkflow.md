# Note:
- Branch "origin/main" is the production branch, it should have very minimal commits and should only have production-ready versions of the application
- Branch "origin/dev" is the only development branch, this is the default branch that should have the different commits you do locally, it's a shared environment that should be shared between all developers


# The Flow:
0- You should've cloned the repo already, and configured the dependencies, and you have a fully working clone of the repository locally
1- When you're about to implement a new feature, create locally a new branch from the main branch you are working on locally
2- After finishing the feature, merge the branch into the local main branch (and delete the branch if you want), rebase merge is preferred specially if you comitted a lot
**IMPORTANT** -> 3- Merge the remote "origin/dev" into the local main branch, because there could be changes that happened after your last pull that would make conflict if you pushed directly,
if any conflicts occured, they should be resolved gracefully first
4- Push the local main branch (should be up to date with the "origin/dev" without conflicts, just the "origin/dev" is a few commits behind)
5- Accept the request from the remote repo
