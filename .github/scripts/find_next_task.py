#!/usr/bin/env python3
"""Find the next eligible task for Copilot agent assignment.

Resolves dependencies, skips blocked/assigned issues, and prints
the winning issue number to stdout for GitHub Actions consumption.
"""

import json
import re
import subprocess
import sys


def run_gh(args: list[str]) -> str:
    """Run a gh CLI command and return stdout."""
    result = subprocess.run(
        ["gh", *args],
        capture_output=True,
        text=True,
        check=True,
    )
    return result.stdout.strip()


def get_issues(state: str, labels: str | None = None) -> list[dict]:
    """Fetch issues via gh CLI."""
    cmd = [
        "issue", "list",
        "--state", state,
        "--json", "number,title,labels,assignees",
        "--limit", "200",
    ]
    if labels:
        cmd.extend(["--label", labels])
    return json.loads(run_gh(cmd))


def parse_code_dependencies(issue_number: int) -> list[int]:
    """Parse dependency issue numbers from the issue body's ## Dependencies section."""
    try:
        body = run_gh(["issue", "view", str(issue_number), "--json", "body", "--jq", ".body"])
    except subprocess.CalledProcessError:
        return []

    in_deps_section = False
    for line in body.splitlines():
        stripped = line.strip()
        if stripped.startswith("## Dependencies"):
            in_deps_section = True
            continue
        if in_deps_section and stripped.startswith("##"):
            break
        if in_deps_section and stripped.startswith("- **Code:**"):
            return [int(n) for n in re.findall(r"#(\d+)", stripped)]

    return []


def get_tier(labels: list[dict]) -> int:
    """Extract tier number from labels (e.g., tier:1 → 1). Default 99."""
    for label in labels:
        name = label.get("name", "")
        if name.startswith("tier:"):
            try:
                return int(name.split(":")[1])
            except (IndexError, ValueError):
                continue
    return 99


def has_needs_label(labels: list[dict]) -> bool:
    """Check if issue has any needs:* label (external blocker)."""
    return any(label.get("name", "").startswith("needs:") for label in labels)


def is_assigned(assignees: list[dict]) -> bool:
    """Check if issue is assigned to anyone."""
    return len(assignees) > 0


def main() -> None:
    dry_run = "--dry-run" in sys.argv

    # Fetch open tasks and closed issues
    open_tasks = get_issues("open", "task")
    closed_issues = get_issues("closed")
    closed_numbers = {issue["number"] for issue in closed_issues}

    candidates: list[tuple[int, int, int]] = []  # (tier, issue_number, index)

    for issue in open_tasks:
        number = issue["number"]
        labels = issue.get("labels", [])
        assignees = issue.get("assignees", [])

        # Skip assigned issues
        if is_assigned(assignees):
            if dry_run:
                print(f"  SKIP #{number}: already assigned", file=sys.stderr)
            continue

        # Skip issues with needs:* labels
        if has_needs_label(labels):
            if dry_run:
                print(f"  SKIP #{number}: has needs:* label", file=sys.stderr)
            continue

        # Check code dependencies
        deps = parse_code_dependencies(number)
        unmet = [d for d in deps if d not in closed_numbers]
        if unmet:
            if dry_run:
                print(f"  SKIP #{number}: unmet deps {unmet}", file=sys.stderr)
            continue

        tier = get_tier(labels)
        candidates.append((tier, number, number))
        if dry_run:
            print(f"  ELIGIBLE #{number}: tier={tier}, deps={deps} (all met)", file=sys.stderr)

    if not candidates:
        print("No eligible tasks found.", file=sys.stderr)
        sys.exit(0)

    # Sort: lowest tier first, then lowest issue number
    candidates.sort(key=lambda x: (x[0], x[1]))
    winner = candidates[0][1]

    if dry_run:
        print(f"\nWould assign: #{winner}", file=sys.stderr)

    # Print winner to stdout for GitHub Actions
    print(winner)


if __name__ == "__main__":
    main()
