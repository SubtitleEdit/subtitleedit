---
name: subtitleedit-ai-contributor-guidelines
description: "Use when: using AI coding agents or LLM assistance for Subtitle Edit code changes, reviews, refactors, tests, or documentation updates. Provides project-specific safeguards against broad, overcomplicated, or unverified changes."
license: MIT
---

# Subtitle Edit AI Contributor Guidelines

AI-assisted contributions are becoming a normal part of open source development. This guidance gives AI tools, and contributors who use them, a shared quality bar for Subtitle Edit. The goal is not to ban AI use, but to reduce the avoidable damage that default LLM behavior can cause: wrong assumptions, broad edits, overbuilt abstractions, and unverified pull requests.

These instructions apply to code changes, reviews, refactors, tests, documentation, build scripts, packaging files, and translation-related work in this repository.

## 1. Clarify Before Changing

Before editing, identify the requested outcome, affected area, and verification path.

- If the issue can mean more than one thing, state the interpretations and ask for clarification before implementing.
- If a requested change conflicts with existing behavior, project conventions, privacy expectations, or platform support, call that out.
- Prefer the smallest change that fully solves the stated problem.
- Do not infer new product behavior, UI redesigns, settings, or compatibility promises from a vague request.

## 2. Keep Changes Surgical

Every changed line should be traceable to the request.

- Do not reformat unrelated files, reorder unrelated code, rename existing symbols, or modernize adjacent code unless it is required for the task.
- Match the local style of the file being edited, even when another style would also be valid.
- Preserve existing comments, translations, resource keys, subtitle format behavior, and platform-specific code unless the task directly requires changing them.
- If you find unrelated dead code or a possible bug, mention it in the PR notes instead of fixing it in the same change.
- Remove only the unused imports, variables, helpers, or files made unused by your own change.

## 3. Prefer Boring Code

Subtitle Edit has a large feature surface and many file-format edge cases. Avoid cleverness that makes future maintenance harder.

- Use existing helpers, patterns, forms, services, and test utilities before introducing new abstractions.
- Do not add configurability, extension points, dependencies, caches, background tasks, or generic frameworks unless the issue clearly needs them.
- Keep UI changes consistent with existing workflows. Do not redesign screens as part of a bug fix.
- Keep parsing, conversion, timing, encoding, and localization changes narrow and explicit.
- For subtitle format handling, protect backwards compatibility unless the issue is specifically about changing compatibility behavior.

## 4. Verify The Actual Risk

Define verification before implementation, then run the most relevant check that is practical.

- For bug fixes, prefer a focused regression test that fails before the fix and passes after it.
- For shared parsing, conversion, timing, waveform, OCR, translation, or export behavior, run targeted tests and expand coverage when risk is broad.
- For UI-only changes where automated tests are not practical, document the manual verification path.
- If a command cannot be run locally, say so in the PR and explain the remaining risk.
- Do not claim a change is verified unless the relevant command or manual check was actually performed.

## 5. Protect User Data And Offline Expectations

Subtitle Edit is an offline editor by default. Keep that trust boundary intact.

- Do not introduce telemetry, analytics, remote logging, model training, or network calls unless explicitly requested and reviewed.
- Be careful with subtitle text, media paths, and user-provided content in logs, exceptions, screenshots, or test artifacts.
- Keep optional third-party service integrations explicit and isolated.

## 6. Write Useful PR Notes

AI-assisted PRs should make maintainer review easier, not harder.

- Explain what changed and why.
- List the verification performed.
- Mention assumptions, tradeoffs, and known limitations.
- Keep generated descriptions factual and concise.
- Do not hide that AI assistance was used if the contribution was materially AI-assisted.

## Attribution

This is an original, project-specific adaptation inspired by `forrestchang/andrej-karpathy-skills` and Andrej Karpathy's public observations about common LLM coding failure modes.

Source inspiration: https://github.com/forrestchang/andrej-karpathy-skills

The source repository declares MIT licensing in its README and skill metadata. This file is not a vendored copy of the source text; keep this attribution if the guidance is moved or expanded.