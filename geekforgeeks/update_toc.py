#!/usr/bin/env python3
"""
update_toc.py — Regenerates the Table of Contents in README.md.
Includes only level-1 (#) and level-2 (##) headings.
Skips the "Table of Contents" heading itself.

Usage:
    python update_toc.py
"""

import re

README = "01-system-design-intro.md"
TOC_START = "# Table of Contents"
TOC_MARKER = "---"  # first horizontal rule after TOC marks end of TOC block


def heading_to_anchor(text: str) -> str:
    """Convert a heading text to a GitHub-style anchor."""
    # Remove markdown formatting (bold, italic, code, links)
    text = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", text)   # [label](url) → label
    text = re.sub(r"[`*_]", "", text)                       # strip ` * _
    # Remove emojis and non-ASCII characters
    text = text.encode("ascii", "ignore").decode()
    text = text.lower()
    # Replace anything that is not alphanumeric or space/hyphen with nothing
    text = re.sub(r"[^\w\s-]", "", text)
    # Collapse spaces/underscores to hyphens
    text = re.sub(r"[\s_]+", "-", text).strip("-")
    return text


def build_toc(lines: list[str]) -> list[str]:
    toc = []
    for line in lines:
        m = re.match(r"^(#{1,2})\s+(.*)", line)
        if not m:
            continue
        level, title = m.group(1), m.group(2).strip()
        # Skip the TOC heading itself
        if title.lower() == "table of contents":
            continue
        anchor = heading_to_anchor(title)
        indent = "" if len(level) == 1 else "  "
        toc.append(f"{indent}- [{title}](#{anchor})")
    return toc


def update_readme(path: str) -> None:
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    lines = content.splitlines()

    # Locate the TOC heading line
    toc_heading_idx = next(
        (i for i, l in enumerate(lines) if l.strip() == TOC_START), None
    )
    if toc_heading_idx is None:
        print("ERROR: '# Table of Contents' heading not found.")
        return

    # Locate the first '---' after the TOC heading → end of TOC block
    toc_end_idx = next(
        (i for i in range(toc_heading_idx + 1, len(lines)) if lines[i].strip() == TOC_MARKER),
        None,
    )
    if toc_end_idx is None:
        print("ERROR: Could not find the '---' separator after Table of Contents.")
        return

    # Build TOC from ALL headings in the file
    new_toc_lines = build_toc(lines)

    # Reconstruct file: TOC heading + blank line + new toc entries + blank line + rest
    new_lines = (
        lines[: toc_heading_idx + 1]          # "# Table of Contents"
        + [""]                                  # blank line
        + new_toc_lines                         # generated entries
        + [""]                                  # blank line before ---
        + lines[toc_end_idx:]                   # "---" and everything after
    )

    new_content = "\n".join(new_lines) + "\n"

    with open(path, "w", encoding="utf-8") as f:
        f.write(new_content)

    print(f"TOC updated with {len(new_toc_lines)} entries.")


if __name__ == "__main__":
    update_readme(README)
