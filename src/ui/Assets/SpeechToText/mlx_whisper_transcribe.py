#!/usr/bin/env python3
"""
Subtitle Edit - MLX Whisper Mac helper.

Transcribes an audio file with Apple's MLX implementation of Whisper
(the `mlx-whisper` pip package) and writes a subtitle file (SRT or VTT)
into the output directory, named <audio-basename>.<ext>, which Subtitle Edit
then loads.

Unlike CTranslate2 (faster-whisper), MLX runs on the Apple Silicon GPU and
Neural Engine via Apple's MLX framework, so on M-series Macs it is typically
much faster than the CPU-only faster-whisper path. Models are MLX-format
Whisper weights hosted on Hugging Face (e.g. mlx-community/whisper-large-v3-turbo)
and are downloaded automatically on first use.

Raw Whisper segments make poor subtitles: they are often far too long or too
short, and their boundaries ignore sentence structure. The helper therefore
requests word-level timestamps and rebuilds the cues itself, exactly like the
Faster Whisper Mac helper: breaking at sentence-ending punctuation and silence
gaps, balance-splitting oversized sentences at soft punctuation or the largest
speech pause, then applying the industry conventions from the Netflix Timed
Text Style Guide and the BBC subtitle guidelines (84 characters and 7 seconds
max per cue, 5/6 second minimum with short cues merging into a neighbor, a
two-frame minimum gap, reading speed capped at 20 characters per second).
`--raw-segments` restores the plain Whisper segmentation. MLX decodes
sequentially with running text context, so punctuation (which drives the
sentence splitting) is intact in every language without extra prompting.

Each segment is printed as `[MM:SS.mmm --> MM:SS.mmm] text` so the host can show
live progress and, if needed, recover the transcript from stdout.
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


def ensure_model_downloaded(repo):
    """Pre-download the MLX model so the first run shows progress instead of looking frozen.

    mlx-whisper downloads the model from Hugging Face on first use. Unauthenticated downloads
    are throttled and the 1+ GB weights file can take minutes; the host only sees newline-
    terminated stdout, while Hugging Face's tqdm bars update with carriage returns, so without
    this step a first run looks stalled. We fetch the snapshot up front and print our own
    percent-progress lines the host can display. A local model path needs no download.
    """
    if not repo or os.path.isdir(repo) or "/" not in repo:
        return

    try:
        from huggingface_hub import snapshot_download
    except Exception:
        return  # mlx-whisper will handle the download itself.

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
        # Not fatal: fall through and let mlx-whisper try its own download path.
        print(f"warning: model pre-download failed ({e}); letting mlx-whisper download it",
              file=sys.stderr, flush=True)


def load_wav_as_array(path):
    """Read a PCM WAV directly into a 16 kHz mono float32 array.

    mlx-whisper's own loader shells out to the `ffmpeg` binary, which may be
    missing or not executable in the host's environment. Subtitle Edit always
    feeds a 16 kHz mono PCM WAV, so we decode it here with the standard library
    and hand mlx-whisper the waveform directly, avoiding ffmpeg entirely.
    Returns a float32 numpy array, or None if the file is not a WAV we can read.
    """
    import wave
    import numpy as np

    with wave.open(path, "rb") as w:
        channels = w.getnchannels()
        sample_width = w.getsampwidth()
        framerate = w.getframerate()
        frames = w.readframes(w.getnframes())

    if sample_width == 2:
        data = np.frombuffer(frames, dtype="<i2").astype(np.float32) / 32768.0
    elif sample_width == 4:
        data = np.frombuffer(frames, dtype="<i4").astype(np.float32) / 2147483648.0
    elif sample_width == 1:
        data = (np.frombuffer(frames, dtype=np.uint8).astype(np.float32) - 128.0) / 128.0
    else:
        return None  # unusual width - let mlx-whisper handle it via ffmpeg

    if channels > 1:
        data = data.reshape(-1, channels).mean(axis=1)

    if framerate != 16000 and len(data) > 0:
        target_len = int(round(len(data) * 16000 / framerate))
        if target_len > 0:
            old_idx = np.linspace(0.0, 1.0, num=len(data), endpoint=False)
            new_idx = np.linspace(0.0, 1.0, num=target_len, endpoint=False)
            data = np.interp(new_idx, old_idx, data).astype(np.float32)

    return np.ascontiguousarray(data, dtype=np.float32)


# Sentence-ending characters that always close a cue: Latin, Arabic (؟), Urdu (۔),
# CJK full stops and their fullwidth companions, plus ellipsis.
SENTENCE_END = ".!?…؟۔。！？"

# Soft break points preferred when a too-long sentence must be split: Latin and
# Arabic commas and semicolons, and colons.
SOFT_BREAK = ",;:،؛"

# Punctuated sample prompts per language, used to stop Whisper omitting punctuation
# (see main). Languages not listed rely on Whisper's normal behavior; --initial-prompt
# covers any special case.
PUNCTUATION_PROMPTS = {
    "ar": "مرحباً، كيف حالك؟ أنا بخير. شكراً جزيلاً!",
    "en": "Hello there. How are you? I'm fine, thanks!",
    "tr": "Merhaba, nasılsın? Ben iyiyim. Teşekkür ederim!",
}


def apply_standards(cues, max_chars, max_duration, max_cps,
                    min_duration=0.833, min_gap=0.083):
    """Apply the industry timing conventions to the built cues.

    Per the Netflix Timed Text Style Guide / BBC subtitle guidelines: a cue
    shorter than 5/6 second merges into its predecessor when the result still
    respects the length and duration limits; cue ends are extended when the
    reading speed would exceed max_cps characters per second (or the minimum
    duration is not met) and there is room; and a two-frame minimum gap is kept
    before the next cue.
    """
    if not cues:
        return cues

    merged = []
    for start, end, text in cues:
        if merged:
            p_start, p_end, p_text = merged[-1]
            if (end - start < min_duration
                    and start - p_end < 1.0
                    and len(p_text) + 1 + len(text) <= max_chars
                    and end - p_start <= max_duration):
                merged[-1] = (p_start, end, p_text + " " + text)
                continue
        merged.append((start, end, text))

    out = []
    for i, (start, end, text) in enumerate(merged):
        wanted = max(end, start + min_duration, start + len(text) / max_cps)
        end = min(wanted, start + max_duration)
        if i + 1 < len(merged):
            latest = merged[i + 1][0] - min_gap
            if end > latest:
                end = max(latest, start + 0.2)
        out.append((start, end, text))

    return out


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
    # When the decode produced no sentence punctuation at all (it happens with
    # dialectal speech even after prompting), rely on the speech rhythm instead:
    # a tighter pause threshold recovers sentence-like grouping from the audio.
    if not any(w[2].strip()[-1:] in SENTENCE_END for w in words if w[2].strip()):
        gap_break = min(gap_break, 0.6)

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
        description="mlx-whisper transcription helper for Subtitle Edit")
    parser.add_argument("--audio", required=True, help="Path to the input audio file")
    parser.add_argument("--model", default="mlx-community/whisper-large-v3-turbo",
                        help="MLX Whisper model (Hugging Face repo id)")
    parser.add_argument("--language", default=None, help="Language code, or omit/auto to auto-detect")
    parser.add_argument("--output-dir", default=None, help="Directory for the subtitle file (default: audio's folder)")
    parser.add_argument("--output-format", default="srt", choices=["srt", "vtt"])
    parser.add_argument("--task", default="transcribe", choices=["transcribe", "translate"])
    parser.add_argument("--max-cue-chars", type=int, default=84,
                        help="Max characters per subtitle cue before a forced break (two 42-char lines)")
    parser.add_argument("--max-cue-duration", type=float, default=7.0,
                        help="Max seconds per subtitle cue before a forced break (Netflix limit)")
    parser.add_argument("--max-cps", type=float, default=20.0,
                        help="Max reading speed in characters per second; cue ends extend to meet it")
    parser.add_argument("--initial-prompt", default=None,
                        help="Text to bias the decoder (overrides the automatic punctuation prompt)")
    parser.add_argument("--no-punctuation-prompt", action="store_true",
                        help="Disable the automatic punctuation-biasing prompt")
    parser.add_argument("--raw-segments", action="store_true",
                        help="Write Whisper's raw segments instead of word-timestamp resegmented cues")
    args, unknown = parser.parse_known_args()
    if unknown:
        print(f"warning: ignoring unrecognized argument(s): {' '.join(unknown)}",
              file=sys.stderr, flush=True)

    try:
        import mlx_whisper
    except ImportError:
        print("error: mlx-whisper not found - install it with: pip3 install mlx-whisper",
              file=sys.stderr)
        return 3

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

    # Prefer decoding the WAV ourselves so we never depend on a system ffmpeg binary
    # (mlx-whisper's path loader calls `ffmpeg`, which may be missing/non-executable).
    audio_input = args.audio
    if os.path.splitext(args.audio)[1].lower() == ".wav":
        try:
            arr = load_wav_as_array(args.audio)
            if arr is not None:
                audio_input = arr
        except Exception as e:
            print(f"warning: direct WAV load failed ({e}); falling back to ffmpeg path",
                  file=sys.stderr, flush=True)

    ensure_model_downloaded(args.model)

    print(f"Loading MLX Whisper model '{args.model}' (Apple GPU/Neural Engine)...", flush=True)
    use_words = not args.raw_segments
    transcribe_kwargs = {}
    # Whisper drops punctuation on some real-world speech (dialectal Arabic notably)
    # even with sequential decoding, and punctuation drives the sentence-based cue
    # splitting. A short punctuated prompt in the audio's language restores it
    # (verified on this model: zero marks without, normal sentence marks with).
    prompt = args.initial_prompt
    if prompt is None and not args.no_punctuation_prompt and language:
        prompt = PUNCTUATION_PROMPTS.get(language.lower())
    if prompt:
        transcribe_kwargs["initial_prompt"] = prompt
    result = mlx_whisper.transcribe(
        audio_input,
        path_or_hf_repo=args.model,
        language=language,
        task=args.task,
        word_timestamps=use_words,
        # verbose=True prints each segment as it is decoded, which (with unbuffered
        # stdout) is what streams live transcription lines into the host's console.
        verbose=True,
        **transcribe_kwargs,
    )

    detected = result.get("language", language or "?")
    print(f"Detected language: {detected}", flush=True)

    collected = []
    words = []
    for seg in result.get("segments", []):
        text = (seg.get("text") or "").strip()
        start = float(seg.get("start", 0.0))
        end = float(seg.get("end", 0.0))
        # mlx-whisper already printed each segment live (verbose=True), so only collect here.
        collected.append((start, end, text))
        if use_words:
            for w in seg.get("words") or []:
                words.append((float(w.get("start", start)), float(w.get("end", end)), w.get("word") or ""))

    if use_words and words:
        cues = resegment(words, args.max_cue_chars, args.max_cue_duration)
        cues = apply_standards(cues, args.max_cue_chars, args.max_cue_duration, args.max_cps)
        print(f"Resegmented {len(collected)} raw segment(s) into {len(cues)} subtitle cue(s) "
              f"(max {args.max_cue_chars} chars / {args.max_cue_duration:g}s per cue, "
              f"max {args.max_cps:g} chars/second).", flush=True)
        collected = cues
    elif use_words:
        print("No word timestamps returned; keeping Whisper's raw segments.", flush=True)

    sep = "," if ext == "srt" else "."
    with open(out_path, "w", encoding="utf-8") as f:
        if ext == "vtt":
            f.write("WEBVTT\n\n")
        for index, (start, end, text) in enumerate(collected, 1):
            if ext == "srt":
                f.write(f"{index}\n")
            f.write(f"{fmt_timestamp(start, sep)} --> {fmt_timestamp(end, sep)}\n{text}\n\n")

    print(f"Wrote {len(collected)} subtitle(s) to {out_path}", flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
