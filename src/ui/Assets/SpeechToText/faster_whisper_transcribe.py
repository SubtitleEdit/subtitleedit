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

Models are resolved by name (tiny ... large-v3, large-v3-turbo) and downloaded
automatically from Hugging Face on first use. Audio decoding uses PyAV, which
faster-whisper bundles, so no system ffmpeg is needed.

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
    }

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
    # gives the host genuine live progress.
    collected = []
    for seg in segments:
        text = (seg.text or "").strip()
        start = float(seg.start)
        end = float(seg.end)
        print(f"[{fmt_short(start)} --> {fmt_short(end)}] {text}", flush=True)
        collected.append((start, end, text))

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
