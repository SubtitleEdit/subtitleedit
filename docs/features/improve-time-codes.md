# Improve Time Codes (forced alignment)

Tighten the time codes of a subtitle that is already roughly in sync, by letting a forced aligner listen for every line in the audio. Nothing is transcribed and no text is changed - only the in- and out-cues move.

- **Menu:** Tools → Improve time codes (forced alignment)…
- **Requires:** a video or audio file loaded in the main window, and the Crisp ASR engine (downloaded on first use).

The subtitle has to be close to begin with: each line is looked for round its current position only. If the whole file is off by seconds, use [Adjust all times](adjust-all-times.md), [Point sync](point-sync.md) or [Visual sync](visual-sync.md) first.

## Setup bar

- **Engine status + gear icon** — shows the installed Crisp ASR version. The gear opens the engine settings (backend, update / re-download), the same dialog as in Speech to text.
- **Aligner** — the model that does the listening. The list is ordered best first for the language of the subtitle:
  1. the *wav2vec2 aligner* for that language, when there is one (English, German, French, Spanish, Italian, Japanese, Chinese, Dutch, Portuguese, Arabic, Ukrainian, Czech) — the most precise, and its line ends stop where the speech stops;
  2. the *Canary CTC aligner* — one model for 25 European languages, accurate to about 80 ms;
  3. the *Qwen3 forced aligner* — for the languages only it covers (e.g. Korean).

  A model that is not installed yet says how much will be downloaded; the download starts when you press **Align**.
- **Max shift (seconds)** — a start or end that the aligner wants to move further than this is left where it was and flagged. This guards against lines whose text is not actually what is said.
- **Adjust start times / Adjust end times** — untick one to leave that side of every line alone.
- **Align** — extracts the audio and runs the aligner. Length is not a limit: the audio is processed in short windows of a few lines each, so long films use no more memory than short clips. **Cancel** stops a running alignment.

## Preview

Two stacked waveforms show the same stretch of audio:

- **Original** (blue) — the time codes as they are now.
- **Aligned** (green) — the time codes after alignment.

The line being looked at is amber in both. Scrolling or zooming either waveform moves the other, and clicking a line in a waveform selects it in the list.

Under the waveforms:

- **▲ / ▼** (or **F7** / **F8**) step to the previous / next re-timed line, and *Change X of Y* shows where you are.
- The summary on the right counts re-timed lines, lines that were kept, lines with no speech, and the mean shift.

## Line list

One row per subtitle line, with the shift of its start and end in milliseconds and a status:

| Status | Meaning |
|---|---|
| Re-timed | The aligner moved the line. |
| Already in place | The aligner agrees with the current time codes. |
| No speech | Nothing to listen for - blank lines, `[sound descriptions]`, `(sighs)`, `♪`. These lines are never moved. |
| Kept - shift too large | The line would have moved more than *Max shift*, so it was left alone. |
| Kept - aligner failed | The aligner could not process this group of lines. |

Untick **Apply** on a row (or press **Space**) to keep that line's original time codes; the green waveform updates at once.

Press **OK** to write the ticked changes to the subtitle (one undo step), or **Cancel** to discard.

## How the new time codes are chosen

- The start is where the first word of the line begins.
- The end is where the last word ends - but a line is never cut shorter than the minimum display time or the optimal reading speed from Settings → General, as long as that does not run past where the line used to end or into the next line.
- Overlaps that were in the subtitle on purpose are left alone; the tool never creates new ones.

Run [Beautify time codes](beautify-time-codes.md) afterwards to snap the result to frames and shot changes.
