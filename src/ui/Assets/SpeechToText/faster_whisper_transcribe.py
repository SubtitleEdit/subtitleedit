#!/usr/bin/env python3
"""
Subtitle Edit - Faster Whisper Mac helper.

Transcribes an audio file with the `faster-whisper` pip package (SYSTRAN's
CTranslate2 implementation of Whisper) and writes a subtitle file (SRT or VTT)
into the output directory, named <audio-basename>.<ext>, which Subtitle Edit
then loads.

CTranslate2 has no Apple GPU (Metal) backend, so on macOS this always runs on
the CPU. To claw back speed without touching accuracy, the helper uses
faster-whisper's BatchedInferencePipeline when available (typically 2-4x faster
than sequential decoding with identical model weights) and pins the CTranslate2
thread count to the machine's core count. The remaining speed lever,
`--compute-type int8`, quantizes the model (roughly 2x faster, near-identical
accuracy) and is left opt-in so the default stays exactly as accurate as stock
faster-whisper.

Raw Whisper segments make poor subtitles: they are often far too long or too
short, and their boundaries ignore sentence structure. Like Purfview's
Faster-Whisper-XXL "--standard" mode on Windows, the helper therefore requests
word-level timestamps and rebuilds the cues itself: breaking at sentence-ending
punctuation, at silence gaps, and at length/duration limits, with each cue
timed from its first word's start to its last word's end. `--raw-segments`
restores the plain Whisper segmentation.

Models are resolved by name (tiny ... large-v3, large-v3-turbo) and downloaded
automatically from Hugging Face on first use. Audio decoding uses PyAV, which
faster-whisper bundles, so no system ffmpeg is needed.

Each raw segment is printed as `[MM:SS.mmm --> MM:SS.mmm] text` while decoding
so the host can show live progress.
"""
import argparse
import os
import sys


def fmt_timestamp(seconds, sep):
    if seconds < 0:
        seconds = 0.0
    total_ms = int(round(seconds * 1000))
    hours, total_ms = divmod(total_ms, 3600000)
    minutes, total_ms = divmod(total_ms, 60000)
    secs, millis = divmod(total_ms, 1000)
    return f"{hours:02d}:{minutes:02d}:{secs:02d}{sep}{millis:03d}"


def fmt_short(seconds):
    if seconds < 0:
        seconds = 0.0
    total_ms = int(round(seconds * 1000))
    minutes, total_ms = divmod(total_ms, 60000)
    secs, millis = divmod(total_ms, 1000)
    return f"{minutes:02d}:{secs:02d}.{millis:03d}"


def ensure_model_downloaded(model):
    """Pre-download the CTranslate2 model so the first run shows progress instead of looking frozen.

    faster-whisper downloads the converted model from Hugging Face on first use. The 1+ GB
    weights can take minutes and Hugging Face's tqdm bars update with carriage returns, which
    the host (reading newline-terminated stdout) never sees, so without this step a first run
    looks stalled. We resolve the model name to its Hugging Face repo, fetch the snapshot up
    front, and print our own percent-progress lines. A local model path needs no download.
    """
    if not model or os.path.isdir(model):
        return

    repo = model if "/" in model else None
    if repo is None:
        try:
            from faster_whisper.utils import _MODELS
            repo = _MODELS.get(model)
        except Exception:
            repo = None

    if not repo:
        return  # unknown name - let faster-whisper resolve or fail with its own message

    try:
        from huggingface_hub import snapshot_download
    except Exception:
        return  # faster-whisper will handle the download itself.

    try:
        from tqdm.auto import tqdm as _base_tqdm
    except Exception:
        try:
            from tqdm import tqdm as _base_tqdm
        except Exception:
            _base_tqdm = None

    tqdm_class = None
    if _base_tqdm is not None:
        class _LineTqdm(_base_tqdm):
            # Emit a newline-terminated "<file> NN%" line (only on percent change) so the host
            # shows live download progress, while the base class keeps its normal stderr bar.
            def __init__(self, *a, **k):
                self._last_pct = -1
                super().__init__(*a, **k)

            def _emit(self):
                total = self.total or 0
                if total <= 0:
                    return
                pct = int(self.n * 100 / total)
                if pct == self._last_pct:
                    return
                self._last_pct = pct
                label = (self.desc or "Downloading model").strip().rstrip(":") or "Downloading model"
                print(f"{label}: {pct}%", flush=True)

            def update(self, n=1):
                ret = super().update(n)
                self._emit()
                return ret

            def close(self):
                self._emit()
                return super().close()

        tqdm_class = _LineTqdm

    print(f"Downloading model '{repo}' from Hugging Face (first use; this can take a while)...",
          flush=True)
    try:
        snapshot_download(repo_id=repo, tqdm_class=tqdm_class)
        print("Model download complete.", flush=True)
    except Exception as e:
        # Not fatal: fall through and let faster-whisper try its own download path.
        print(f"warning: model pre-download failed ({e}); letting faster-whisper download it",
              file=sys.stderr, flush=True)


# Sentence-ending characters that always close a cue: Latin, Arabic (؟), Urdu (۔),
# CJK full stops and their fullwidth companions, plus ellipsis.
SENTENCE_END = ".!?…؟۔。！？"

# Soft break points preferred when a too-long sentence must be split: Latin and
# Arabic commas and semicolons, and colons.
SOFT_BREAK = ",;:،؛"


# Whisper's batched pipeline decodes VAD chunks without the running text context the
# sequential decoder keeps, and in some languages (Arabic notably) that makes it drop
# punctuation entirely, which in turn breaks sentence-based cue splitting. A short
# punctuated prompt in the audio's language restores it (verified on large-v3: zero
# punctuation marks without the prompt, normal sentence punctuation with it) at no
# speed cost. Applied only when the language is explicitly chosen; --initial-prompt
# overrides, --no-punctuation-prompt disables.
PUNCTUATION_PROMPTS = {
    "ar": "\u0645\u0631\u062d\u0628\u0627\u064b\u060c \u0643\u064a\u0641 \u062d\u0627\u0644\u0643\u061f \u0623\u0646\u0627 \u0628\u062e\u064a\u0631. \u0634\u0643\u0631\u0627\u064b \u062c\u0632\u064a\u0644\u0627\u064b!",
    "en": "Hello there. How are you? I'm fine, thanks!",
    "tr": "Merhaba, nas\u0131ls\u0131n? Ben iyiyim. Te\u015fekk\u00fcr ederim!",
}


def split_chunk(chunk, max_chars, max_duration):
    """Split one sentence's words into cue-sized parts, balanced instead of greedy.

    Cutting a sentence at the last word that still fits leaves orphan cues like a
    lone "editors." half a second long. Instead, an oversized run is split near its
    middle, preferring (in order) a soft-punctuation boundary, then the largest
    silence gap between words (a real speech pause, which is where a subtitler
    would cut), then the word boundary nearest the middle; recursing until every
    part fits, so all pieces stay readable.
    """
    total_chars = sum(len(w[2]) for w in chunk)
    duration = chunk[-1][1] - chunk[0][0]
    if (total_chars <= max_chars and duration <= max_duration) or len(chunk) == 1:
        return [chunk]

    # Candidate split points: after word i (never after the last word). Track the
    # cumulative character position of each candidate to measure distance from the
    # middle, and the pause before the next word.
    candidates = []
    acc = 0
    for i, w in enumerate(chunk[:-1]):
        acc += len(w[2])
        gap = chunk[i + 1][0] - w[1]
        candidates.append((i, acc, gap))

    total = acc + len(chunk[-1][2])
    half = total / 2

    # Keep splits inside the middle band so neither piece comes out tiny.
    band = [c for c in candidates if 0.3 * total <= c[1] <= 0.7 * total] or candidates

    soft = [c for c in band if chunk[c[0]][2].rstrip()[-1:] in SOFT_BREAK]
    if soft:
        best_i = min(soft, key=lambda c: abs(c[1] - half))[0]
    else:
        by_gap = max(band, key=lambda c: c[2])
        if by_gap[2] >= 0.15:
            best_i = by_gap[0]
        else:
            best_i = min(band, key=lambda c: abs(c[1] - half))[0]

    left, right = chunk[:best_i + 1], chunk[best_i + 1:]
    return split_chunk(left, max_chars, max_duration) + split_chunk(right, max_chars, max_duration)


def resegment(words, max_chars, max_duration, gap_break=1.0):
    """Rebuild subtitle cues from word-level timestamps.

    First group words into sentences (closed by sentence-ending punctuation or a
    silence gap), then balance-split any sentence exceeding the length or duration
    limit via split_chunk. Each cue is timed first-word-start to last-word-end, so
    cue timecodes hug the actual speech instead of inheriting Whisper's coarse
    segment bounds. Returns a list of (start, end, text).
    """
    sentences = []
    cur = []

    for start, end, token in words:
        stripped = token.strip()
        if not stripped:
            continue

        if cur and start - cur[-1][1] >= gap_break:
            sentences.append(cur)
            cur = []

        cur.append((start, end, token))

        if stripped[-1] in SENTENCE_END:
            sentences.append(cur)
            cur = []

    if cur:
        sentences.append(cur)

    cues = []
    for sentence in sentences:
        for part in split_chunk(sentence, max_chars, max_duration):
            text = "".join(w[2] for w in part).strip()
            if text:
                cues.append((part[0][0], part[-1][1], text))

    return cues


def main():
    parser = argparse.ArgumentParser(
        description="faster-whisper transcription helper for Subtitle Edit")
    parser.add_argument("--audio", required=True, help="Path to the input audio file")
    parser.add_argument("--model", default="large-v3",
                        help="Model name (tiny ... large-v3, large-v3-turbo), local path, or Hugging Face repo id")
    parser.add_argument("--language", default=None, help="Language code, or omit/auto to auto-detect")
    parser.add_argument("--output-dir", default=None, help="Directory for the subtitle file (default: audio's folder)")
    parser.add_argument("--output-format", default="srt", choices=["srt", "vtt"])
    parser.add_argument("--task", default="transcribe", choices=["transcribe", "translate"])
    parser.add_argument("--device", default="auto",
                        help="CTranslate2 device (auto/cpu; there is no Apple GPU backend)")
    parser.add_argument("--compute-type", default="auto",
                        help="CTranslate2 compute type; int8 is ~2x faster on CPU with near-identical accuracy")
    parser.add_argument("--batch-size", type=int, default=8,
                        help="Batched-inference batch size; 0 or 1 disables batching")
    parser.add_argument("--cpu-threads", type=int, default=0,
                        help="CTranslate2 CPU threads; 0 = one per core")
    parser.add_argument("--max-cue-chars", type=int, default=84,
                        help="Max characters per subtitle cue before a forced break (two 42-char lines)")
    parser.add_argument("--max-cue-duration", type=float, default=6.0,
                        help="Max seconds per subtitle cue before a forced break")
    parser.add_argument("--raw-segments", action="store_true",
                        help="Write Whisper's raw segments instead of word-timestamp resegmented cues")
    parser.add_argument("--initial-prompt", default=None,
                        help="Text to bias the decoder (overrides the automatic punctuation prompt)")
    parser.add_argument("--no-punctuation-prompt", action="store_true",
                        help="Disable the automatic punctuation-biasing prompt")
    # Tolerate unknown extra flags from the user's advanced-settings box instead of dying on them.
    args, unknown = parser.parse_known_args()
    if unknown:
        print(f"warning: ignoring unrecognized argument(s): {' '.join(unknown)}",
              file=sys.stderr, flush=True)

    try:
        from faster_whisper import WhisperModel
    except ImportError:
        print("error: faster-whisper not found - install it with: pip3 install faster-whisper",
              file=sys.stderr)
        return 3

    try:
        from faster_whisper import BatchedInferencePipeline
    except ImportError:
        BatchedInferencePipeline = None  # older faster-whisper; fall back to sequential decoding

    if not os.path.isfile(args.audio):
        print(f"error: audio file not found: {args.audio}", file=sys.stderr)
        return 2

    language = args.language
    if language in (None, "", "auto"):
        language = None

    out_dir = args.output_dir or os.path.dirname(os.path.abspath(args.audio))
    os.makedirs(out_dir, exist_ok=True)
    base = os.path.splitext(os.path.basename(args.audio))[0]
    ext = args.output_format
    out_path = os.path.join(out_dir, base + "." + ext)

    cpu_threads = args.cpu_threads if args.cpu_threads > 0 else (os.cpu_count() or 4)
    use_words = not args.raw_segments

    ensure_model_downloaded(args.model)

    print(f"Loading faster-whisper model '{args.model}' "
          f"(device={args.device}, compute_type={args.compute_type}, cpu_threads={cpu_threads})...",
          flush=True)
    model = WhisperModel(
        args.model,
        device=args.device,
        compute_type=args.compute_type,
        cpu_threads=cpu_threads,
    )

    transcribe_kwargs = {
        "language": language,
        "task": args.task,
        "word_timestamps": use_words,
    }

    prompt = args.initial_prompt
    if prompt is None and not args.no_punctuation_prompt and language:
        prompt = PUNCTUATION_PROMPTS.get(language.lower())
    if prompt:
        transcribe_kwargs["initial_prompt"] = prompt

    use_batching = BatchedInferencePipeline is not None and args.batch_size > 1
    if use_batching:
        print(f"Using batched inference (batch_size={args.batch_size}).", flush=True)
        pipeline = BatchedInferencePipeline(model=model)
        segments, info = pipeline.transcribe(
            args.audio, batch_size=args.batch_size, **transcribe_kwargs)
    else:
        if BatchedInferencePipeline is None and args.batch_size > 1:
            print("Batched inference not available in this faster-whisper version; "
                  "decoding sequentially (pip3 install --upgrade faster-whisper for a 2-4x speedup).",
                  flush=True)
        segments, info = model.transcribe(
            args.audio, vad_filter=True, **transcribe_kwargs)

    detected = getattr(info, "language", language or "?")
    probability = getattr(info, "language_probability", None)
    if probability is not None:
        print(f"Detected language: {detected} (probability {probability:.2f})", flush=True)
    else:
        print(f"Detected language: {detected}", flush=True)

    # segments is a generator that yields while decoding, so printing as we iterate
    # gives the host genuine live progress. Words are collected for resegmentation.
    raw = []
    words = []
    for seg in segments:
        text = (seg.text or "").strip()
        start = float(seg.start)
        end = float(seg.end)
        print(f"[{fmt_short(start)} --> {fmt_short(end)}] {text}", flush=True)
        raw.append((start, end, text))
        if use_words and getattr(seg, "words", None):
            for w in seg.words:
                words.append((float(w.start), float(w.end), w.word))

    if use_words and words:
        cues = resegment(words, args.max_cue_chars, args.max_cue_duration)
        print(f"Resegmented {len(raw)} raw segment(s) into {len(cues)} subtitle cue(s) "
              f"(max {args.max_cue_chars} chars / {args.max_cue_duration:g}s per cue).", flush=True)
    else:
        if use_words:
            print("No word timestamps returned; keeping Whisper's raw segments.", flush=True)
        cues = raw

    sep = "," if ext == "srt" else "."
    with open(out_path, "w", encoding="utf-8") as f:
        if ext == "vtt":
            f.write("WEBVTT\n\n")
        for index, (start, end, text) in enumerate(cues, 1):
            if ext == "srt":
                f.write(f"{index}\n")
            f.write(f"{fmt_timestamp(start, sep)} --> {fmt_timestamp(end, sep)}\n{text}\n\n")

    print(f"Wrote {len(cues)} subtitle(s) to {out_path}", flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
