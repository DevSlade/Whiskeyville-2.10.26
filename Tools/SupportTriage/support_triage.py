#!/usr/bin/env python3
# ============================================================
# WHISKEYVILLE SUPPORT TRIAGE SYSTEM
# ============================================================
# PURPOSE:    Categorize and draft responses to support messages
#             using Claude API. Runs locally. No cloud dependency
#             beyond the Claude API call itself.
# VERSION:    v1.0
# CREATED:    May 2026
# AUTHOR:     Black Label Interactive / James Slade
#
# SETUP:
#   1. pip install anthropic
#   2. Set env variable: ANTHROPIC_API_KEY=sk-ant-...
#      Windows: setx ANTHROPIC_API_KEY "sk-ant-..."
#      Then restart terminal.
#
# USAGE:
#   python support_triage.py
#
#   Mode 1 — Drop .txt files into:
#     ./whiskeyville_support/inbox/
#   Then run the script. Files are processed and moved to /processed.
#
#   Mode 2 — Paste a message directly into the terminal.
#
# OUTPUT:
#   - HTML dashboard:  ./whiskeyville_support/triage_report.html
#   - JSON log:        ./whiskeyville_support/triage_log.json
#
# COST:
#   ~$0.001 per message using claude-haiku-3-5.
#   100 messages/month ≈ $0.10. Effectively free at low volume.
# ============================================================

import os
import json
import sys
import re
from datetime import datetime
from pathlib import Path

# ============================================================
# DEPENDENCY CHECK
# ============================================================

try:
    import anthropic
except ImportError:
    print("\n❌  Missing dependency. Run:")
    print("      pip install anthropic\n")
    sys.exit(1)

# ============================================================
# CONFIGURATION — edit these if needed
# ============================================================

ANTHROPIC_API_KEY = os.environ.get("ANTHROPIC_API_KEY", "")

# All output lives in this folder (created automatically)
OUTPUT_DIR   = Path("./whiskeyville_support")
INBOX_DIR    = OUTPUT_DIR / "inbox"
PROCESSED_DIR = OUTPUT_DIR / "processed"
REPORT_FILE  = OUTPUT_DIR / "triage_report.html"
LOG_FILE     = OUTPUT_DIR / "triage_log.json"

# Claude model to use for triage (haiku = fast + cheap; sonnet = richer drafts)
TRIAGE_MODEL = "claude-haiku-3-5"

# ============================================================
# CATEGORY + PRIORITY DEFINITIONS
# ============================================================

# Format: "KEY": (emoji, display_label, default_priority)
CATEGORIES = {
    "BUG":      ("🐛", "Bug Report",            "HIGH"),
    "CRASH":    ("💥", "Crash / Can't Open",     "HIGH"),
    "FEATURE":  ("✨", "Feature Request",         "LOW"),
    "POSITIVE": ("⭐", "Positive Feedback",       "LOW"),
    "QUESTION": ("❓", "General Question",        "MEDIUM"),
    "NEGATIVE": ("😤", "Complaint / Negative",    "MEDIUM"),
    "REVIEW":   ("📱", "App Store / Play Review", "MEDIUM"),
    "OTHER":    ("📬", "Other",                   "LOW"),
}

PRIORITY_COLORS = {
    "HIGH":   "#ff4444",
    "MEDIUM": "#ffaa00",
    "LOW":    "#44bb44",
}

# ============================================================
# SYSTEM PROMPT — gives Claude context about Whiskeyville
# ============================================================

SYSTEM_PROMPT = """You are the support triage system for Whiskeyville, a mobile game.

GAME CONTEXT:
Whiskeyville is a 2D mobile tycoon/farming game about restoring a post-Prohibition ghost
town distillery. Solo-developed by James Slade (Black Label Interactive). Players grow grain
(corn, rye, barley, wheat), convert it to mash, distill whiskey, age it in barrels in a
rickhouse, and sell it at the Saloon. Key buildings: Corn Field, Mash Tun, Still, Cooperage,
Rickhouse, Saloon, Seed Shop. The game is set in 1933 Tennessee. There is a flavor system
(FlavorTags), a Whiskey Creator (player names their town, distillery, and whiskey brand),
and a Fame progression system.

DEVELOPER:
James is a solo developer who reads every message personally. Responses should be warm,
genuine, first-person, and non-corporate. He also works at Jack Daniel's distillery.

YOUR TASK:
Analyze the support message and return ONLY a valid JSON object with these exact fields:

{
  "category": one of [BUG, CRASH, FEATURE, POSITIVE, QUESTION, NEGATIVE, REVIEW, OTHER],
  "priority": one of [HIGH, MEDIUM, LOW],
  "sender_name": extracted first name, or "Player" if not given,
  "summary": one-sentence summary of the core issue or feedback (max 15 words),
  "key_detail": the single most important specific detail from the message,
  "suggested_answer": if QUESTION category, a brief factual answer using game context above; else null,
  "draft_response": a warm, personal response from James (3-5 sentences, address by first name,
                    use game-specific language naturally, never sound like a template),
  "tags": array of 1-3 lowercase tags describing the issue area
          e.g. ["ios", "production-loop", "crash", "tutorial", "save-system", "audio"]
}

Return ONLY the JSON object. No markdown. No explanation. No code block fences."""

# ============================================================
# CORE TRIAGE FUNCTION
# ============================================================

def triage_message(message_text: str, client: anthropic.Anthropic) -> dict:
    """
    Sends a support message to Claude for categorization and draft response.
    Returns a structured dict with category, priority, summary, and draft.
    Called by both process_inbox() and interactive_triage().
    """
    response = client.messages.create(
        model=TRIAGE_MODEL,
        max_tokens=1024,
        system=SYSTEM_PROMPT,
        messages=[{
            "role": "user",
            "content": f"Triage this support message:\n\n---\n{message_text.strip()}\n---"
        }]
    )

    raw = response.content[0].text.strip()

    # Strip markdown code fences if Claude wraps in them despite instructions
    if raw.startswith("```"):
        raw = re.sub(r"```(?:json)?\n?", "", raw).strip().rstrip("`").strip()

    result = json.loads(raw)

    # Defensive defaults — never let a missing field crash the report
    result.setdefault("category",         "OTHER")
    result.setdefault("priority",         "LOW")
    result.setdefault("sender_name",      "Player")
    result.setdefault("summary",          "(no summary)")
    result.setdefault("key_detail",       "")
    result.setdefault("suggested_answer", None)
    result.setdefault("draft_response",   "Thank you for reaching out. I'll look into this.")
    result.setdefault("tags",             [])

    return result

# ============================================================
# INBOX BATCH PROCESSING
# ============================================================

def process_inbox(client: anthropic.Anthropic) -> list:
    """
    Reads all .txt files from the inbox folder and triages each one.
    Successfully processed files are moved to /processed.
    Returns a list of triage result dicts.
    """
    INBOX_DIR.mkdir(parents=True, exist_ok=True)

    txt_files = list(INBOX_DIR.glob("*.txt"))

    if not txt_files:
        print(f"\n📬  Inbox is empty.")
        print(f"    Drop .txt files here: {INBOX_DIR.resolve()}")
        print(f"    Each file = one support message (paste the text, save as .txt).\n")
        return []

    results = []
    print(f"\n⚙️   Processing {len(txt_files)} file(s)...\n")

    for txt_file in txt_files:
        print(f"  → {txt_file.name}", end="", flush=True)
        message_text = txt_file.read_text(encoding="utf-8")

        try:
            result = triage_message(message_text, client)
            result["source_file"]      = txt_file.name
            result["processed_at"]     = datetime.now().isoformat()
            result["original_message"] = message_text

            results.append(result)

            # Move processed file so it isn't re-processed next run
            PROCESSED_DIR.mkdir(parents=True, exist_ok=True)
            dest = PROCESSED_DIR / txt_file.name
            # Avoid collision if file already exists in processed
            if dest.exists():
                dest = PROCESSED_DIR / f"{txt_file.stem}_{int(datetime.now().timestamp())}.txt"
            txt_file.rename(dest)

            emoji = CATEGORIES.get(result["category"], ("📬",))[0]
            print(f" {emoji} [{result['priority']}] {result['summary']}")

        except (json.JSONDecodeError, KeyError, Exception) as e:
            print(f" ⚠️  Failed — {e}")

    return results

# ============================================================
# INTERACTIVE / PASTE MODE
# ============================================================

def interactive_triage(client: anthropic.Anthropic) -> list:
    """
    Prompts the user to paste a message directly into the terminal.
    On Windows, finish with Ctrl+Z then Enter. On Mac/Linux, use Ctrl+D.
    Returns a list with one triage result dict.
    """
    print("\n📋  Paste the support message below.")
    print("    Finish: Ctrl+Z then Enter (Windows) | Ctrl+D (Mac/Linux)\n")

    lines = []
    try:
        while True:
            lines.append(input())
    except EOFError:
        pass

    message_text = "\n".join(lines).strip()

    if not message_text:
        print("⚠️  No message entered.")
        return []

    print("\n⚙️   Triaging...", end="", flush=True)
    result = triage_message(message_text, client)
    result["source_file"]      = "interactive"
    result["processed_at"]     = datetime.now().isoformat()
    result["original_message"] = message_text

    emoji = CATEGORIES.get(result["category"], ("📬",))[0]
    print(f" {emoji} [{result['priority']}] {result['summary']}")

    return [result]

# ============================================================
# JSON LOG — appends every run; nothing is lost
# ============================================================

def append_to_log(results: list):
    """
    Appends new triage results to the running JSON log file.
    The log is the source of truth for all historical messages.
    """
    existing = []
    if LOG_FILE.exists():
        try:
            existing = json.loads(LOG_FILE.read_text(encoding="utf-8"))
        except json.JSONDecodeError:
            existing = []  # corrupt log — start fresh rather than crash

    existing.extend(results)
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    LOG_FILE.write_text(json.dumps(existing, indent=2, ensure_ascii=False), encoding="utf-8")

# ============================================================
# HTML DASHBOARD GENERATOR
# ============================================================

def generate_html_report():
    """
    Reads the full JSON log and regenerates the HTML dashboard.
    The dashboard is a single self-contained .html file — open in any browser.
    Sorted: HIGH priority first, then MEDIUM, then LOW. Newest within each tier.
    Filterable by category or priority via buttons.
    Each row has a "Copy Draft" button that copies the draft response to clipboard.
    """

    # Load all historical results
    all_results = []
    if LOG_FILE.exists():
        try:
            all_results = json.loads(LOG_FILE.read_text(encoding="utf-8"))
        except json.JSONDecodeError:
            all_results = []

    if not all_results:
        print("⚠️   No messages in log to report on.")
        return

    # Sort by priority (HIGH first) then by date descending
    priority_order = {"HIGH": 0, "MEDIUM": 1, "LOW": 2}
    all_results.sort(key=lambda x: (
        priority_order.get(x.get("priority", "LOW"), 2),
        x.get("processed_at", "")
    ), reverse=False)
    # Within same priority, newest first
    all_results.sort(key=lambda x: (
        priority_order.get(x.get("priority", "LOW"), 2),
        x.get("processed_at", "")
    ))

    # ---- Stats bar ----
    category_counts = {}
    for r in all_results:
        cat = r.get("category", "OTHER")
        category_counts[cat] = category_counts.get(cat, 0) + 1

    stats_html = "".join([
        f'<div class="stat">'
        f'<span class="stat-emoji">{CATEGORIES.get(k, ("📬","",""))[0]}</span>'
        f'<strong>{v}</strong> {CATEGORIES.get(k, ("📬", k, ""))[1]}'
        f'</div>'
        for k, v in sorted(category_counts.items(), key=lambda x: -x[1])
    ])

    # ---- Table rows ----
    rows_html = ""
    for idx, r in enumerate(all_results):
        cat       = r.get("category", "OTHER")
        emoji, label, _ = CATEGORIES.get(cat, ("📬", cat, "LOW"))
        priority  = r.get("priority", "LOW")
        pcolor    = PRIORITY_COLORS.get(priority, "#999999")

        tags_html = " ".join([
            f'<span class="tag">{t}</span>' for t in r.get("tags", [])
        ])

        draft_html    = r.get("draft_response", "").replace("\n", "<br>").replace('"', "&quot;")
        original_html = r.get("original_message", "")[:600].replace("\n", "<br>")
        date_str      = r.get("processed_at", "")[:16].replace("T", " ")
        source        = r.get("source_file", "")

        # Escape draft for data attribute (used by copy button)
        draft_plain = r.get("draft_response", "").replace("\\", "\\\\").replace("`", "\\`")

        rows_html += f"""
        <tr class="msg-row" data-category="{cat}" data-priority="{priority}" data-idx="{idx}">
          <td class="col-priority">
            <span style="color:{pcolor}; font-size:1.2em;">●</span><br>
            <small style="color:{pcolor};">{priority}</small>
          </td>
          <td class="col-type">{emoji}<br><small>{label}</small></td>
          <td class="col-from">
            <strong>{r.get('sender_name', 'Player')}</strong><br>
            <small class="dim">{date_str}</small><br>
            <small class="dim">{source}</small>
          </td>
          <td class="col-summary">
            {r.get('summary', '')}
            <div class="tags">{tags_html}</div>
            {f'<div class="key-detail dim">↳ {r.get("key_detail","")}</div>' if r.get("key_detail") else ""}
          </td>
          <td class="col-response">
            <details>
              <summary class="sec-label">📩 Original Message</summary>
              <div class="msg-box">{original_html}</div>
            </details>
            <details open>
              <summary class="sec-label">✍️ Draft Response</summary>
              <div class="draft-box" id="draft-{idx}">{draft_html}</div>
              <button class="copy-btn" onclick="copyDraft({idx})">Copy Draft</button>
            </details>
          </td>
        </tr>"""

    # ---- Full HTML ----
    html = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Whiskeyville Support Triage</title>
  <style>
    *, *::before, *::after {{ box-sizing: border-box; margin: 0; padding: 0; }}
    body {{
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      background: #140d04;
      color: #f0e0c0;
      font-size: 14px;
      line-height: 1.5;
    }}
    /* ---- Header ---- */
    header {{
      background: #1e1208;
      border-bottom: 2px solid #c8813d;
      padding: 18px 28px;
      display: flex;
      align-items: center;
      gap: 16px;
    }}
    header h1 {{ color: #c8813d; font-size: 1.5em; letter-spacing: 0.02em; }}
    header p  {{ color: #8a6848; font-size: 0.88em; margin-top: 3px; }}

    /* ---- Stats ---- */
    .stats {{
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      padding: 16px 28px;
      background: #18100a;
      border-bottom: 1px solid #2a1a0a;
    }}
    .stat {{
      background: #22160a;
      border: 1px solid #3a2512;
      border-radius: 8px;
      padding: 8px 14px;
      font-size: 0.88em;
    }}
    .stat-emoji {{ margin-right: 5px; }}

    /* ---- Filters ---- */
    .filters {{
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      padding: 12px 28px;
      background: #140d04;
      border-bottom: 1px solid #221408;
    }}
    .filter-btn {{
      background: #22160a;
      border: 1px solid #3a2512;
      color: #c8a878;
      padding: 5px 14px;
      border-radius: 20px;
      cursor: pointer;
      font-size: 0.83em;
      transition: all 0.15s;
    }}
    .filter-btn:hover  {{ background: #c8813d; border-color: #c8813d; color: #fff; }}
    .filter-btn.active {{ background: #c8813d; border-color: #c8813d; color: #fff; }}

    /* ---- Table ---- */
    table {{ width: 100%; border-collapse: collapse; }}
    th {{
      background: #1e1208;
      color: #c8813d;
      padding: 10px 14px;
      text-align: left;
      font-size: 0.8em;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      border-bottom: 1px solid #3a2512;
      position: sticky;
      top: 0;
      z-index: 10;
    }}
    td {{
      padding: 12px 14px;
      border-bottom: 1px solid #1e1208;
      vertical-align: top;
    }}
    .msg-row:hover td {{ background: #1c1208; }}

    .col-priority {{ width: 70px;  text-align: center; }}
    .col-type     {{ width: 120px; text-align: center; font-size: 1.4em; }}
    .col-from     {{ width: 140px; }}
    .col-summary  {{ width: 220px; }}
    .col-response {{ }}

    /* ---- Tags ---- */
    .tags {{ margin-top: 6px; }}
    .tag {{
      display: inline-block;
      background: #2a1a0a;
      border: 1px solid #4a2a10;
      border-radius: 10px;
      padding: 1px 8px;
      font-size: 0.78em;
      margin: 2px 2px 0 0;
      color: #a07848;
    }}
    .key-detail {{ font-size: 0.83em; margin-top: 5px; font-style: italic; }}
    .dim {{ color: #705848; }}

    /* ---- Message / Draft boxes ---- */
    .msg-box, .draft-box {{
      background: #0e0904;
      border: 1px solid #2a1a0a;
      border-radius: 6px;
      padding: 10px 14px;
      margin: 6px 0;
      font-size: 0.85em;
      line-height: 1.65;
      max-width: 520px;
      white-space: pre-wrap;
    }}
    .draft-box {{
      border-color: #c8813d44;
      background: #120a04;
    }}
    details summary.sec-label {{
      cursor: pointer;
      color: #c8813d;
      font-size: 0.83em;
      margin: 6px 0 2px;
      user-select: none;
    }}
    details summary.sec-label:hover {{ text-decoration: underline; }}

    /* ---- Copy button ---- */
    .copy-btn {{
      background: #c8813d;
      border: none;
      color: #fff;
      padding: 5px 14px;
      border-radius: 4px;
      cursor: pointer;
      font-size: 0.82em;
      margin-top: 6px;
      transition: background 0.15s;
    }}
    .copy-btn:hover {{ background: #e09a55; }}

    /* ---- Footer ---- */
    footer {{
      text-align: center;
      padding: 20px;
      color: #403020;
      font-size: 0.8em;
      border-top: 1px solid #1e1208;
      margin-top: 20px;
    }}
  </style>
</head>
<body>

<header>
  <div>
    <h1>🥃 Whiskeyville Support Triage</h1>
    <p>
      Generated {datetime.now().strftime("%B %d, %Y at %I:%M %p")} &nbsp;·&nbsp;
      {len(all_results)} total message(s) in log
    </p>
  </div>
</header>

<div class="stats">{stats_html}</div>

<div class="filters">
  <button class="filter-btn active" onclick="filterAll(this)">All</button>
  <button class="filter-btn" onclick="filterBy(this,'priority','HIGH')">🔴 High Priority</button>
  <button class="filter-btn" onclick="filterBy(this,'category','BUG')">🐛 Bugs</button>
  <button class="filter-btn" onclick="filterBy(this,'category','CRASH')">💥 Crashes</button>
  <button class="filter-btn" onclick="filterBy(this,'category','FEATURE')">✨ Features</button>
  <button class="filter-btn" onclick="filterBy(this,'category','POSITIVE')">⭐ Positive</button>
  <button class="filter-btn" onclick="filterBy(this,'category','QUESTION')">❓ Questions</button>
  <button class="filter-btn" onclick="filterBy(this,'category','NEGATIVE')">😤 Complaints</button>
</div>

<table>
  <thead>
    <tr>
      <th>Priority</th>
      <th>Type</th>
      <th>From</th>
      <th>Summary</th>
      <th>Response</th>
    </tr>
  </thead>
  <tbody id="msg-table">
{rows_html}
  </tbody>
</table>

<footer>
  Whiskeyville Support &nbsp;·&nbsp; Black Label Interactive &nbsp;·&nbsp;
  Local Triage System v1.0 &nbsp;·&nbsp; Powered by Claude API
</footer>

<script>
  // Store all draft text in a JS map for reliable clipboard copy
  const drafts = {{}};
  document.querySelectorAll("[id^='draft-']").forEach(el => {{
    const idx = el.id.replace("draft-", "");
    drafts[idx] = el.innerText;
  }});

  function copyDraft(idx) {{
    const text = drafts[String(idx)] || "";
    navigator.clipboard.writeText(text).then(() => {{
      const btn = document.querySelector(`button[onclick="copyDraft(${{idx}})"]`);
      if (btn) {{
        const orig = btn.textContent;
        btn.textContent = "✓ Copied!";
        setTimeout(() => {{ btn.textContent = orig; }}, 2000);
      }}
    }});
  }}

  function filterAll(btn) {{
    document.querySelectorAll(".msg-row").forEach(r => r.style.display = "");
    document.querySelectorAll(".filter-btn").forEach(b => b.classList.remove("active"));
    btn.classList.add("active");
  }}

  function filterBy(btn, attr, val) {{
    document.querySelectorAll(".msg-row").forEach(r => {{
      r.style.display = (r.dataset[attr] === val) ? "" : "none";
    }});
    document.querySelectorAll(".filter-btn").forEach(b => b.classList.remove("active"));
    btn.classList.add("active");
  }}
</script>

</body>
</html>"""

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    REPORT_FILE.write_text(html, encoding="utf-8", errors="replace")
    print(f"\n📊  Dashboard updated: {REPORT_FILE.resolve()}")

# ============================================================
# MAIN ENTRY POINT
# ============================================================

def main():
    # API key check
    if not ANTHROPIC_API_KEY:
        print("\n❌  ANTHROPIC_API_KEY environment variable not set.")
        print("\n    Windows:  setx ANTHROPIC_API_KEY \"sk-ant-...\"")
        print("              (restart terminal after setx)")
        print("\n    Mac/Linux: export ANTHROPIC_API_KEY=\"sk-ant-...\"\n")
        sys.exit(1)

    client = anthropic.Anthropic(api_key=ANTHROPIC_API_KEY)

    # Create output structure
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    print("\n🥃  Whiskeyville Support Triage System  v1.0")
    print("    Black Label Interactive\n")
    print("=" * 44)
    print("\n  1 — Process inbox folder")
    print(f"      (drop .txt files in: {INBOX_DIR.resolve()})")
    print("\n  2 — Paste a message now\n")

    try:
        choice = input("Choose mode (1 or 2): ").strip()
    except (KeyboardInterrupt, EOFError):
        print("\nCancelled.")
        return

    if choice == "1":
        results = process_inbox(client)
    elif choice == "2":
        results = interactive_triage(client)
    else:
        print(f"⚠️  '{choice}' is not a valid choice. Run again and enter 1 or 2.")
        return

    if not results:
        # Still regenerate the report from existing log (no new messages)
        generate_html_report()
        return

    # Persist results and regenerate dashboard
    append_to_log(results)
    generate_html_report()

    # Terminal summary
    print(f"\n✅  Processed {len(results)} message(s):\n")
    for r in results:
        emoji = CATEGORIES.get(r.get("category", "OTHER"), ("📬",))[0]
        print(f"  {emoji}  [{r.get('priority','?')}]  {r.get('sender_name','Player')}  —  {r.get('summary','')}")

    print(f"\n🌐  Open your dashboard:")
    print(f"    {REPORT_FILE.resolve()}\n")


if __name__ == "__main__":
    main()
