# AGENTS.md — connect-dotnet

> This repo is a normal **git checkout of `master` at the root**, plus **linked worktrees**
> under `.worktrees/` for parallel task work. Start at the root.

## TL;DR for agents

- **Start at the repo root.** The root is `master`'s working tree; the agent docs, taskboard,
  and helper scripts live here and are version-controlled.
- The **taskboard** is `backlog/` (Backlog.md). Check it before doing anything.
- **Don't do task work directly on `master` at the root.** For each task, create a worktree
  under `.worktrees/<branch>`, work there, finalize, PR, then remove it.
- One worktree per task.

## Layout

```
connect-dotnet/            <- ROOT: git checkout of `master` (start here)
├── .git/                  <- normal git dir
├── AGENTS.md              <- this file (tracked)
├── scripts/wt.sh          <- helper: create a task worktree (tracked)
├── backlog/               <- Backlog.md taskboard (tracked)
├── .github/
│   ├── copilot-instructions.md   <- agent nudge (tracked)
│   └── workflows/                <- CI
├── BusinessService/  LocationService/  RestAPI/  postgres/  redis/   <- services
├── docker-compose.yml  MySolution.sln  README.md
├── .worktrees/           <- linked task worktrees (gitignored, local)
│   └── <branch>/
└── .env                  <- secrets (gitignored)
```

`.worktrees/*` are linked worktrees that share history in `.git`. They are **gitignored** and
local to your machine; their contents are committed via their own branch, never via `master`.

## Taskboard: check / start / stop / finish

Drive the board through the `backlog` CLI (never hand-edit files in `backlog/`).

| Action | Command |
| --- | --- |
| See the board | `backlog board` |
| List / filter | `backlog task list --plain` · `backlog task list -s "In Progress" --plain` |
| View one | `backlog task view TASK-N --plain` |
| **Start** | `backlog task edit TASK-N -s "In Progress" -a @you` |
| **Stop / park** | `backlog task edit TASK-N -s "To Do"` |
| Record progress | `backlog task edit TASK-N --append-notes "..."` |
| **Finish** | (read the guide first) `backlog task edit TASK-N --check-ac 1 --final-summary "..." -s Done` |

Read the guides before lifecycle actions: `backlog instructions overview`, then
`task-creation` / `task-execution` / `task-finalization`.

## Working a task (from the root)

1. Pick a task from `backlog board`.
2. Create its worktree + branch:
   ```bash
   ./scripts/wt.sh <branch> --base master --task TASK-N
   # manual: git worktree add .worktrees/<branch> -b <branch> master
   #         backlog task edit TASK-N -s "In Progress"
   ```
3. `cd .worktrees/<branch>` — do all code, build, and test work **here**.
4. Record notes on the task as you go (`--append-notes`).
5. Finalize per `backlog instructions task-finalization`; push the branch; open a PR; tie the
   branch to the task (keep existing refs):
   ```bash
   backlog task edit TASK-N --ref "https://github.com/tpham2580/connect-dotnet/tree/<branch>"
   ```
6. After merge: `git worktree remove .worktrees/<branch> && git branch -d <branch>`.

## Git notes

- The root is a normal `master` checkout — `git status`, builds, and `git worktree ...` all
  work here. Keep `master` clean; make task changes in a worktree, not at the root.
- `.worktrees/` is gitignored, so task worktrees never appear in `master`'s status.
- Keep shared agent files (`AGENTS.md`, `scripts/`, `backlog/`, `.github/copilot-instructions.md`)
  updated on `master`, then rebase task branches on top to propagate them.

## Backlog + worktrees (read before editing tasks)

`backlog/` is **tracked**, so every worktree checks out its own copy of the board. The single
source of truth is git history, but the *working copies* diverge until branches merge.

Rules that keep that divergence harmless:

- **Edit only your own task inside your own worktree.** A worker owns exactly the task it was
  briefed with. Never edit another task from a worktree — do that from the root on `master`.
- **Board-wide changes happen at the root** on `master`: creating tasks, closing unrelated
  tasks, editing `backlog/config.yml`, or reordering the board.
- **Never run `backlog task edit` for a branch's task from the root.** The root holds `master`'s
  older copy, so the edit lands on the wrong version and gets clobbered on merge.
- Your task's file rides along in your PR, so task state is reviewed with the code.

Cross-branch visibility is **enabled** (`check_active_branches: true`, `filesystem_only: false`
in `backlog/config.yml`). This lets a worktree resolve tasks that live only on other branches —
e.g. `backlog task view TASK-10` works from a worktree even when that file exists only on
`master`. Without it the lookup fails outright, and `backlog task create` re-uses an existing ID
because it only sees the local checkout. Leave these enabled; if IDs ever collide anyway, repair
with `backlog doctor --fix`.

Note `backlog task list` / `backlog board` still show only the current checkout's tasks. For the
full board, run them from the root, or use `backlog browser` there.

## Orchestrator convention (Tmux Orchestrator → workers)

- Run the orchestrator from the root.
- For each task: create a worktree (`scripts/wt.sh`) and spawn **one worker per task**.
- Brief every worker with: the **task ID**, its **worktree path** under `.worktrees/`, and
  "start by reading `AGENTS.md` and running `backlog instructions overview`".
- A worker sets its task **In Progress** on start, works only inside its worktree, and drives
  the task to **Done** via the Backlog CLI. One task per worker; don't silently expand scope.
- A worker edits **only its own task file**; board-wide Backlog changes are the orchestrator's
  job, made at the root (see "Backlog + worktrees").
