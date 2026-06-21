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
    args = parser.parse_args()

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

    print(f"Loading MLX Whisper model '{args.model}' (Apple GPU/Neural Engine)...", flush=True)
    result = mlx_whisper.transcribe(
        audio_input,
        path_or_hf_repo=args.model,
        language=language,
        task=args.task,
        verbose=False,
    )

    detected = result.get("language", language or "?")
    print(f"Detected language: {detected}", flush=True)

    collected = []
    for seg in result.get("segments", []):
        text = (seg.get("text") or "").strip()
        start = float(seg.get("start", 0.0))
        end = float(seg.get("end", 0.0))
        # Progress line; matches the host's short-timestamp parser for a stdout fallback.
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
