
<!-- BACKLOG.MD GUIDELINES START -->
<!-- backlog.md-instructions-version: 1.48.0 -->
<CRITICAL_INSTRUCTION>

## Backlog.md Workflow

This project uses Backlog.md for task and project management.

**For every user request in this project, run `backlog instructions overview` before answering or taking action.**

Use the overview to decide whether to search, read, create, or update Backlog tasks.

Before task lifecycle actions, read the matching detailed guide:
- `backlog instructions task-creation` before creating or splitting tasks
- `backlog instructions task-execution` before planning, changing status or assignee, adding a plan or implementation notes, or implementing task work
- `backlog instructions task-finalization` before checking acceptance criteria, writing final summaries, or moving tasks to terminal statuses

Use `backlog <command> --help` before running unfamiliar commands. Help shows options, fields, and examples.

Do not edit Backlog task, draft, document, decision, or milestone markdown files directly. Use the `backlog` CLI so metadata, relationships, and history stay consistent.

</CRITICAL_INSTRUCTION>
<!-- BACKLOG.MD GUIDELINES END -->

## Repository layout (READ FIRST)

The repo root is a normal **git checkout of `master`**, with **linked worktrees** under
`.worktrees/` for parallel task work:

- The root is `master`'s working tree — `git status` and builds work here. Keep `master`
  clean; **do task work in a worktree, not on `master` at the root**.
- Create a task worktree with `./scripts/wt.sh <branch> --task TASK-N`
  (or `git worktree add .worktrees/<branch> -b <branch> master`), then `cd` into it.
- `.worktrees/` is gitignored. The taskboard is `backlog/` (use the `backlog` CLI).
- See **`AGENTS.md`** at the repo root for the full layout and the
  task → worktree → Backlog workflow.
