#!/usr/bin/env bash
# wt.sh - create a task worktree from the connect-dotnet root (a master checkout).
#
# Run from the repo root. Creates a linked worktree under .worktrees/<branch>
# (gitignored) and optionally marks a Backlog task In Progress.
set -euo pipefail

usage() {
  cat <<'EOF'
Usage: scripts/wt.sh <branch> [--base <base-branch>] [--task TASK-N]

Creates a git worktree at .worktrees/<branch> from the repo root.

  <branch>        Branch / worktree name (e.g. task-6-null-business-guard)
  --base          Base branch to fork a NEW branch from (default: master)
  --task TASK-N   Mark this Backlog task In Progress after creating the worktree

Examples:
  scripts/wt.sh task-6-null-guard --task TASK-6
  scripts/wt.sh spike-caching --base master
EOF
}

[ $# -ge 1 ] || { usage; exit 1; }
case "$1" in -h|--help) usage; exit 0 ;; esac

BRANCH="$1"; shift
BASE="master"; TASK=""
while [ $# -gt 0 ]; do
  case "$1" in
    --base) BASE="${2:?--base needs a value}"; shift 2 ;;
    --task) TASK="${2:?--task needs a value}"; shift 2 ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage; exit 1 ;;
  esac
done

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
[ -e .git ] || { echo "error: not a git repo root (no .git here)" >&2; exit 1; }

DEST=".worktrees/$BRANCH"
[ -e "$DEST" ] && { echo "error: $DEST already exists" >&2; exit 1; }
mkdir -p .worktrees

if git show-ref --verify --quiet "refs/heads/$BRANCH"; then
  echo "Branch '$BRANCH' exists -> checking it out at $DEST"
  git worktree add "$DEST" "$BRANCH"
else
  git show-ref --verify --quiet "refs/heads/$BASE" \
    || { echo "error: base branch '$BASE' not found" >&2; exit 1; }
  echo "Creating new branch '$BRANCH' from '$BASE' at $DEST"
  git worktree add "$DEST" -b "$BRANCH" "$BASE"
fi

if [ -n "$TASK" ]; then
  if command -v backlog >/dev/null 2>&1; then
    backlog task edit "$TASK" -s "In Progress" >/dev/null \
      && echo "  $TASK -> In Progress" \
      || echo "  warn: could not update $TASK" >&2
  else
    echo "  warn: 'backlog' not on PATH; skipping task update" >&2
  fi
fi

cat <<EOF

Worktree ready: $ROOT/$DEST
Next steps:
  cd $DEST                 # work + build + test here (not on master at the root)
  backlog task view ${TASK:-TASK-N} --plain
  # once the branch is pushed, tie it to the task (keep existing refs):
  #   backlog task edit ${TASK:-TASK-N} --ref "https://github.com/tpham2580/connect-dotnet/tree/$BRANCH"
  # when merged:
  #   git worktree remove $DEST && git branch -d $BRANCH
EOF
