#!/usr/bin/env python3
"""
SLADE_BRIEF.PY
==============
Daily morning brief for James Slade / Black Label Interactive.
Shows date, countdown to launch, active tasks, and financial status.

Usage:
    python SLADE_BRIEF.py           — display the brief
    python SLADE_BRIEF.py update    — update tasks and financial data

No external dependencies. Python 3.6+ required. Runs on Windows.

Data lives in tasks.json (same folder as this script).
Created automatically with defaults on first run.
"""

import json
import os
import sys
from datetime import date, datetime

# ── Enable ANSI colors in Windows 10+ terminals ─────────────────────────────
os.system("")

# ── ANSI color constants ─────────────────────────────────────────────────────
RESET  = "\033[0m"
BOLD   = "\033[1m"
DIM    = "\033[2m"
AMBER  = "\033[93m"     # Whiskey amber — primary brand color
WHITE  = "\033[97m"
GREEN  = "\033[92m"
RED    = "\033[91m"
YELLOW = "\033[33m"
GRAY   = "\033[90m"

# ── Paths ────────────────────────────────────────────────────────────────────
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
TASKS_FILE = os.path.join(SCRIPT_DIR, "tasks.json")

# ── Countdown target ─────────────────────────────────────────────────────────
LAUNCH_DATE = date(2026, 6, 21)   # Whiskeyville Summer Solstice release

# ── Display width ────────────────────────────────────────────────────────────
WIDTH = 64

# ── Default tasks.json ───────────────────────────────────────────────────────
DEFAULT_TASKS = {
    "primary_task":               "Editor Session - Add FlavorTracker + WhiskeyRecipeManager to scene",
    "whiskeyville_last_completed": "WHISKEYVILLE_MASTER_DOCUMENT.md (16-part game bible)",
    "whiskeyville_next_task":     "Add FlavorTracker component to DontDestroyOnLoad GO in Unity",
    "next_payday":                "",
    "bills": [
        {
            "name":     "Debt Payoff",
            "amount":   1395.00,
            "due_date": "2026-06-15"
        }
    ]
}


# ════════════════════════════════════════════════════════════════════════════
# DATA LAYER
# ════════════════════════════════════════════════════════════════════════════

def load_tasks() -> dict:
    """Load tasks.json, creating it with defaults if it doesn't exist."""
    if not os.path.exists(TASKS_FILE):
        save_tasks(DEFAULT_TASKS)
        print(f"\n  {GRAY}Created tasks.json — run 'python SLADE_BRIEF.py update' to fill it in.{RESET}\n")
        return DEFAULT_TASKS.copy()

    try:
        with open(TASKS_FILE, "r", encoding="utf-8") as f:
            data = json.load(f)
        # Merge missing keys forward (schema updates won't break old files)
        for key, value in DEFAULT_TASKS.items():
            data.setdefault(key, value)
        return data
    except (json.JSONDecodeError, IOError) as e:
        print(f"\n  {RED}Error reading tasks.json: {e}{RESET}\n")
        return DEFAULT_TASKS.copy()


def save_tasks(data: dict) -> None:
    """Write tasks.json with readable formatting."""
    try:
        with open(TASKS_FILE, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
    except IOError as e:
        print(f"\n  {RED}Error saving tasks.json: {e}{RESET}\n")


# ════════════════════════════════════════════════════════════════════════════
# DISPLAY HELPERS
# ════════════════════════════════════════════════════════════════════════════

def rule(char="-") -> str:
    return GRAY + char * WIDTH + RESET


def heavy_rule(char="=") -> str:
    return AMBER + char * WIDTH + RESET


def section_header(label: str) -> str:
    return f"  {BOLD}{AMBER}{label}{RESET}"


def pad(text: str, spaces: int = 4) -> str:
    return " " * spaces + text


def strip_leading_zero(date_str: str) -> str:
    """'June 01, 2026' → 'June 1, 2026' (cross-platform)."""
    # strftime on Windows keeps leading zero; strip it manually.
    import re
    return re.sub(r"(\s)0(\d)", r"\1\2", date_str)


def fmt_date(date_str: str) -> str:
    """Parse YYYY-MM-DD → 'June 15, 2026'. Returns raw string on failure."""
    try:
        d = datetime.strptime(date_str, "%Y-%m-%d").date()
        return strip_leading_zero(d.strftime("%B %d, %Y"))
    except (ValueError, AttributeError, TypeError):
        return date_str or "—"


def days_badge(n: int) -> str:
    """Return colored '(X days)' badge based on urgency."""
    if n < 0:
        return f"{RED}{BOLD}OVERDUE{RESET}"
    if n == 0:
        return f"{RED}{BOLD}DUE TODAY{RESET}"
    if n == 1:
        return f"{RED}1 day{RESET}"
    if n <= 7:
        return f"{RED}{n} days{RESET}"
    if n <= 14:
        return f"{YELLOW}{n} days{RESET}"
    return f"{GRAY}{n} days{RESET}"


def countdown_color(days_left: int) -> str:
    """Pick urgency color for the launch countdown."""
    if days_left <= 7:
        return RED
    if days_left <= 21:
        return YELLOW
    return AMBER


# ════════════════════════════════════════════════════════════════════════════
# BRIEF DISPLAY
# ════════════════════════════════════════════════════════════════════════════

def show_brief() -> None:
    """Render and print the full daily brief."""
    tasks     = load_tasks()
    today     = date.today()
    day_num   = today.timetuple().tm_yday
    days_left = (LAUNCH_DATE - today).days

    # Date strings
    day_name  = today.strftime("%A")
    date_str  = strip_leading_zero(today.strftime("%B %d, %Y"))

    # ── Header ──────────────────────────────────────────────────────────────
    print()
    print(heavy_rule())
    title = "SLADE DAILY BRIEF"
    print(f"{BOLD}{AMBER}{title:^{WIDTH}}{RESET}")
    print(heavy_rule())
    print()

    # ── Date + countdown ─────────────────────────────────────────────────────
    print(f"  {BOLD}{WHITE}{day_name}, {date_str}{RESET}  {GRAY}|  Day {day_num}{RESET}")
    print()

    cc = countdown_color(days_left)
    if days_left > 0:
        print(f"  {cc}{BOLD}-->  {days_left} days until June 21st.{RESET}")
    elif days_left == 0:
        print(f"  {RED}{BOLD}-->  TODAY IS LAUNCH DAY.{RESET}")
    else:
        print(f"  {GRAY}June 21st was {abs(days_left)} day(s) ago.{RESET}")

    print()
    print(rule())
    print()

    # ── Primary Task ─────────────────────────────────────────────────────────
    print(section_header("PRIMARY TASK TODAY"))
    print()
    task = tasks.get("primary_task") or f"{GRAY}— not set —  (python SLADE_BRIEF.py update){RESET}"
    # Word-wrap at ~56 chars so long tasks don't overflow
    wrapped = _wrap(task, 56)
    for i, line_text in enumerate(wrapped):
        prefix = "  " if i == 0 else "    "
        print(pad(f"{WHITE}{line_text}{RESET}", 4))
    print()
    print(rule())
    print()

    # ── Whiskeyville Status ──────────────────────────────────────────────────
    print(section_header("WHISKEYVILLE STATUS"))
    print()
    last = tasks.get("whiskeyville_last_completed") or f"{GRAY}nothing recorded{RESET}"
    nxt  = tasks.get("whiskeyville_next_task")      or f"{GRAY}nothing recorded{RESET}"
    print(pad(f"{GREEN}[x]{RESET}  Last:  {GRAY}{last}{RESET}"))
    print()
    print(pad(f"{AMBER} > {RESET}  Next:  {WHITE}{BOLD}{nxt}{RESET}"))
    print()
    print(rule())
    print()

    # ── Financial Status ──────────────────────────────────────────────────────
    print(section_header("FINANCIAL STATUS"))
    print()

    payday_str = tasks.get("next_payday", "")
    if payday_str:
        try:
            payday   = datetime.strptime(payday_str, "%Y-%m-%d").date()
            pd_days  = (payday - today).days
            pd_fmt   = fmt_date(payday_str)
            pd_badge = days_badge(pd_days)
            print(pad(f"Next payday:  {WHITE}{pd_fmt}{RESET}   {pd_badge}"))
        except ValueError:
            print(pad(f"Next payday:  {GRAY}{payday_str}{RESET}"))
    else:
        print(pad(f"Next payday:  {GRAY}not set  (python SLADE_BRIEF.py update){RESET}"))

    bills = tasks.get("bills", [])
    if bills:
        print()
        print(pad(f"{GRAY}Bills:{RESET}"))
        print()
        for bill in bills:
            name    = bill.get("name", "?")
            amount  = bill.get("amount", 0.0)
            due_str = bill.get("due_date", "")

            try:
                due_date = datetime.strptime(due_str, "%Y-%m-%d").date()
                due_days = (due_date - today).days
                due_fmt  = fmt_date(due_str)
                badge    = days_badge(due_days)
                alert    = f"  {RED}!!!{RESET}" if due_days <= 7 else ""
                print(pad(
                    f"  {WHITE}{name:<18}{RESET}"
                    f"  {AMBER}${amount:>9,.2f}{RESET}"
                    f"  {GRAY}due {due_fmt}{RESET}"
                    f"  {badge}{alert}"
                ))
            except (ValueError, TypeError):
                print(pad(f"  {WHITE}{name:<18}{RESET}  {AMBER}${amount:>9,.2f}{RESET}  {GRAY}{due_str}{RESET}"))
    else:
        print()
        print(pad(f"{GRAY}No bills recorded.{RESET}"))

    print()

    # ── Closing line ──────────────────────────────────────────────────────────
    print(heavy_rule())
    print()
    reminder = "The laptop contains the money. Finish opening it."
    print(f"  {BOLD}{WHITE}{reminder}{RESET}")
    print()
    print(heavy_rule())
    print()


def _wrap(text: str, width: int) -> list:
    """Simple word-wrap. Returns list of lines."""
    if len(text) <= width:
        return [text]
    words, lines, current = text.split(), [], ""
    for word in words:
        if len(current) + len(word) + 1 <= width:
            current = (current + " " + word).lstrip()
        else:
            if current:
                lines.append(current)
            current = word
    if current:
        lines.append(current)
    return lines or [text]


# ════════════════════════════════════════════════════════════════════════════
# UPDATE MODE
# ════════════════════════════════════════════════════════════════════════════

def prompt_str(label: str, current: str = "") -> str:
    """Prompt for a string. Empty input keeps the current value."""
    display = f" [{GRAY}{current}{RESET}]" if current else ""
    try:
        val = input(f"  {AMBER}{label}{display}{RESET}: ").strip()
        return val if val else current
    except (KeyboardInterrupt, EOFError):
        print()
        return current


def prompt_date(label: str, current: str = "") -> str:
    """Prompt for YYYY-MM-DD date. Loops until valid or empty (keeps current)."""
    while True:
        val = prompt_str(f"{label} (YYYY-MM-DD)", current)
        if not val or val == current:
            return current
        try:
            datetime.strptime(val, "%Y-%m-%d")
            return val
        except ValueError:
            print(f"  {RED}  Bad format — use YYYY-MM-DD, e.g. 2026-06-21{RESET}")


def prompt_float(label: str, current: float = 0.0) -> float:
    """Prompt for a dollar amount. Returns current on invalid/empty input."""
    val_str = prompt_str(label, f"{current:.2f}")
    try:
        return float(val_str.replace("$", "").replace(",", ""))
    except ValueError:
        return current


def show_bills(bills: list) -> None:
    """Print numbered bill list."""
    if not bills:
        print(f"    {GRAY}(none){RESET}")
        return
    for i, b in enumerate(bills, 1):
        name    = b.get("name", "?")
        amount  = b.get("amount", 0.0)
        due_str = b.get("due_date", "?")
        due_fmt = fmt_date(due_str)
        print(f"    {AMBER}{i}.{RESET}  {WHITE}{name:<18}{RESET}  ${amount:>9,.2f}  due {due_fmt}")


def update_tasks() -> None:
    """Interactive prompt-by-prompt update flow."""
    print()
    print(heavy_rule())
    print(f"{BOLD}{AMBER}{'  UPDATE — SLADE BRIEF':^{WIDTH}}{RESET}")
    print(heavy_rule())
    print()
    print(f"  {GRAY}Press Enter to keep the current value. Ctrl+C to cancel.{RESET}")
    print()

    tasks = load_tasks()

    # ── Primary task ──────────────────────────────────────────────────────────
    print(rule())
    print(f"  {BOLD}PRIMARY TASK{RESET}")
    print()
    tasks["primary_task"] = prompt_str("Task", tasks.get("primary_task", ""))
    print()

    # ── Whiskeyville ──────────────────────────────────────────────────────────
    print(rule())
    print(f"  {BOLD}WHISKEYVILLE STATUS{RESET}")
    print()
    tasks["whiskeyville_last_completed"] = prompt_str(
        "Last completed", tasks.get("whiskeyville_last_completed", ""))
    tasks["whiskeyville_next_task"] = prompt_str(
        "Next task",      tasks.get("whiskeyville_next_task", ""))
    print()

    # ── Financials ────────────────────────────────────────────────────────────
    print(rule())
    print(f"  {BOLD}FINANCIAL STATUS{RESET}")
    print()
    tasks["next_payday"] = prompt_date("Next payday", tasks.get("next_payday", ""))
    print()

    # Bills submenu
    bills = list(tasks.get("bills", []))
    print(f"  {GRAY}Current bills:{RESET}")
    show_bills(bills)
    print()
    print(f"  {AMBER}[a]{RESET} Add   {AMBER}[e]{RESET} Edit   {AMBER}[r]{RESET} Remove   {GRAY}Enter -- keep as-is{RESET}")
    print()

    try:
        choice = input("  Choice: ").strip().lower()
    except (KeyboardInterrupt, EOFError):
        choice = ""

    if choice == "a":
        print()
        name   = prompt_str("  Bill name")
        amount = prompt_float("  Amount (no $)")
        due    = prompt_date("  Due date")
        if name:
            bills.append({"name": name, "amount": amount, "due_date": due})
            tasks["bills"] = bills
            print(f"\n  {GREEN}Bill added: {name}{RESET}")
        else:
            print(f"\n  {GRAY}No name entered — bill not added.{RESET}")

    elif choice == "e" and bills:
        print()
        show_bills(bills)
        print()
        try:
            idx = int(input("  Edit bill #: ").strip()) - 1
            if 0 <= idx < len(bills):
                b = bills[idx]
                b["name"]     = prompt_str("  Name",     b.get("name", ""))
                b["amount"]   = prompt_float("  Amount",  b.get("amount", 0.0))
                b["due_date"] = prompt_date("  Due date", b.get("due_date", ""))
                bills[idx]    = b
                tasks["bills"] = bills
                print(f"\n  {GREEN}Bill updated.{RESET}")
            else:
                print(f"\n  {RED}Invalid number.{RESET}")
        except (ValueError, IndexError, KeyboardInterrupt):
            print(f"\n  {GRAY}Cancelled.{RESET}")

    elif choice == "r" and bills:
        print()
        show_bills(bills)
        print()
        try:
            idx = int(input("  Remove bill #: ").strip()) - 1
            if 0 <= idx < len(bills):
                removed = bills.pop(idx)
                tasks["bills"] = bills
                print(f"\n  {GREEN}Removed: {removed.get('name', '?')}{RESET}")
            else:
                print(f"\n  {RED}Invalid number.{RESET}")
        except (ValueError, IndexError, KeyboardInterrupt):
            print(f"\n  {GRAY}Cancelled.{RESET}")

    # ── Save ──────────────────────────────────────────────────────────────────
    print()
    print(rule())
    save_tasks(tasks)
    print(f"\n  {GREEN}{BOLD}Saved.{RESET}")
    print(f"  {GRAY}Run  python SLADE_BRIEF.py  to see your brief.{RESET}\n")


# ════════════════════════════════════════════════════════════════════════════
# ENTRY POINT
# ════════════════════════════════════════════════════════════════════════════

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1].lower() == "update":
        update_tasks()
    else:
        show_brief()
