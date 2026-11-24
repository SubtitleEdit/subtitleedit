#!/usr/bin/env python3
"""Preview or apply exact short-cue rules to aligned UTF-8 SubRip files."""

import argparse
import codecs
import json
from pathlib import Path
import re
import unicodedata


DICTIONARIES = Path(__file__).with_name("dictionaries")
SEPARATOR = re.compile(r"(\r?\n[ \t]*\r?\n(?:[ \t]*\r?\n)*)")
HEADER = re.compile(
    r"(?P<number>\d+)\r?\n"
    r"(?P<timing>\d{2,}:\d{2}:\d{2},\d{3} --> \d{2,}:\d{2}:\d{2},\d{3}[^\r\n]*)"
    r"\r?\n(?P<text>.+)",
    re.DOTALL,
)
DECORATION = re.compile(r"[<>\[\]{}♪♫\r\n\u2028\u2029]|^[-–—−]")


def load_subrip(path):
    data = path.read_bytes()
    chunks = SEPARATOR.split(data.decode("utf-8-sig"))
    cues = []
    for index in range(0, len(chunks), 2):
        chunk = chunks[index]
        if not chunk.strip():
            if index == len(chunks) - 1:
                continue
            raise ValueError(f"{path}: empty cue before the end of the file")
        content = chunk.rstrip("\r\n")
        match = HEADER.fullmatch(content)
        if not match:
            raise ValueError(f"{path}: invalid SubRip cue at position {index // 2 + 1}")
        cues.append((index, match))
    return chunks, cues, data.startswith(codecs.BOM_UTF8)


def rule_key(source, draft):
    return tuple(unicodedata.normalize("NFC", text).replace("’", "'") for text in (source, draft))


def load_rules(source_language, target_language):
    path = DICTIONARIES / f"{target_language}.json"
    document = json.loads(path.read_text(encoding="utf-8"))
    if source_language not in document["rules"]:
        raise ValueError(f"No {source_language} → {target_language} rules")
    rules = {}
    for rule in document["rules"][source_language]:
        key = rule_key(rule["source"], rule["draft"])
        if key in rules:
            raise ValueError(f"{path}: duplicate rule {key!r}")
        rules[key] = rule
    return rules


def process(source_path, draft_path, rules, output_path=None):
    _, source_cues, _ = load_subrip(source_path)
    chunks, draft_cues, bom = load_subrip(draft_path)
    if [m["timing"] for _, m in source_cues] != [m["timing"] for _, m in draft_cues]:
        raise ValueError("Source and draft must have identical cue timestamps in the same order")

    matches = []
    for position, ((_, source), (index, draft)) in enumerate(zip(source_cues, draft_cues)):
        if any(DECORATION.search(m["text"]) for m in (source, draft)):
            continue
        rule = rules.get(rule_key(source["text"], draft["text"]))
        if rule is None:
            continue
        match = {"cue": draft["number"], "timing": draft["timing"], **rule}
        if "suggestions" in rule:
            if position > 0:
                match["previous_source"] = source_cues[position - 1][1]["text"]
            if position + 1 < len(source_cues):
                match["next_source"] = source_cues[position + 1][1]["text"]
        matches.append(match)
        if output_path is not None and "replacement" in rule:
            start, end = draft.span("text")
            chunks[index] = chunks[index][:start] + rule["replacement"] + chunks[index][end:]

    if output_path is not None:
        with output_path.open("xb") as stream:
            stream.write((codecs.BOM_UTF8 if bom else b"") + "".join(chunks).encode("utf-8"))
    return matches


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source_subrip", type=Path)
    parser.add_argument("draft_subrip", type=Path)
    parser.add_argument("--source-language", default="en")
    parser.add_argument("--target-language", required=True, choices=sorted(p.stem for p in DICTIONARIES.glob("*.json")))
    parser.add_argument("--output", type=Path, help="write exact replacements to a new file, leaving review suggestions unapplied")
    args = parser.parse_args()
    try:
        rules = load_rules(args.source_language, args.target_language)
        matches = process(args.source_subrip, args.draft_subrip, rules, args.output)
    except (OSError, ValueError) as error:
        parser.exit(1, f"{error}\n")
    print(json.dumps(matches, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
