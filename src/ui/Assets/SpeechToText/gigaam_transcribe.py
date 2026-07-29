#!/usr/bin/env python3
"""
Subtitle Edit - GigaAM (onnx-asr) helper.

Transcribes an audio file with Sber's GigaAM Russian speech models through the
`onnx-asr` pip package (https://github.com/istupakov/onnx-asr) and writes an
SRT into the output directory, named <audio-basename>.srt, which Subtitle Edit
then loads.

GigaAM accepts short utterances only, so long-form audio is segmented with the
Silero VAD bundled with onnx-asr; each speech segment comes back with start/end
times in seconds. VAD segments can still be far longer than a subtitle should
be, so oversized segments are split into cues at word boundaries, allocating
time proportionally to character counts (GigaAM's plain CTC/RNNT heads expose
no word timestamps; the v3 "e2e" heads add punctuation, which improves the
split points).

Each cue is printed as `[MM:SS.mmm --> MM:SS.mmm] text` so the host can show
live progress and, if needed, recover the transcript from stdout.
"""
import argparse
import os
import sys


def fmt_timestamp(seconds, sep=","):
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


def split_segment(start, end, text, max_chars, max_duration):
    """Split one VAD segment into subtitle-sized cues at word boundaries.

    Without word timestamps the only usable clock is the segment span, so each
    cue's times are interpolated proportionally to its share of the characters.
    Break points prefer sentence-ending punctuation, then commas, then the
    max-chars limit.
    """
    text = " ".join(text.split())
    duration = max(end - start, 0.001)
    if not text:
        return []
    if len(text) <= max_chars and duration <= max_duration:
        return [(start, end, text)]

    words = text.split(" ")

    # Target cue count from both limits, then greedy-fill by words, preferring
    # to end a cue after punctuation once it is at least half full.
    cues = []
    current = []
    current_len = 0
    max_cue_chars = max(max_chars, 1)
    for i, word in enumerate(words):
        extra = len(word) + (1 if current else 0)
        if current and current_len + extra > max_cue_chars:
            cues.append(" ".join(current))
            current = [word]
            current_len = len(word)
            continue

        current.append(word)
        current_len += extra

        at_punct = word and word[-1] in ".!?…"
        soft_punct = word and word[-1] in ",;:"
        if current_len >= max_cue_chars // 2 and at_punct:
            cues.append(" ".join(current))
            current = []
            current_len = 0
        elif current_len >= (max_cue_chars * 3) // 4 and soft_punct:
            cues.append(" ".join(current))
            current = []
            current_len = 0

    if current:
        cues.append(" ".join(current))

    # Interpolate times proportionally to character counts.
    total_chars = sum(len(c) for c in cues) or 1
    result = []
    t = start
    for cue in cues:
        share = duration * len(cue) / total_chars
        cue_end = min(t + share, end)
        result.append((t, cue_end, cue))
        t = cue_end

    # Cues longer than max_duration (a slow-speech VAD span with little text)
    # are clamped by ear: keep the start, cap the display duration.
    clamped = []
    for s, e, cue in result:
        if e - s > max_duration:
            e = s + max_duration
        clamped.append((s, e, cue))

    return clamped


def main():
    parser = argparse.ArgumentParser(
        description="GigaAM (onnx-asr) transcription helper for Subtitle Edit")
    parser.add_argument("--audio", required=True, help="Path to the input audio file (wav)")
    parser.add_argument("--model", default="gigaam-v3-e2e-rnnt",
                        help="onnx-asr model id (e.g. gigaam-v3-e2e-rnnt, gigaam-v3-ctc, gigaam-v2-rnnt)")
    parser.add_argument("--quantization", default=None,
                        help="Model quantization, e.g. int8 (smaller download, faster on CPU); omit for full precision")
    parser.add_argument("--output-dir", default=None,
                        help="Directory for the subtitle file (default: audio's folder)")
    parser.add_argument("--max-cue-chars", type=int, default=84,
                        help="Max characters per subtitle cue before a forced break (two 42-char lines)")
    parser.add_argument("--max-cue-duration", type=float, default=7.0,
                        help="Max seconds per subtitle cue")
    args, unknown = parser.parse_known_args()
    if unknown:
        print(f"warning: ignoring unknown arguments: {' '.join(unknown)}", flush=True)

    try:
        import onnx_asr
    except ImportError:
        print('error: onnx-asr not found - install it with: pip3 install "onnx-asr[cpu,hub]"',
              file=sys.stderr)
        return 3

    if not os.path.isfile(args.audio):
        print(f"error: audio file not found: {args.audio}", file=sys.stderr)
        return 2

    quantization = args.quantization
    if quantization in ("", "none", "None"):
        quantization = None

    print(f"Loading model '{args.model}' (downloaded from Hugging Face on first use; "
          "this can take a while)...", flush=True)
    try:
        model = onnx_asr.load_model(args.model, quantization=quantization)
    except Exception as e:
        print(f"error: could not load model '{args.model}': {e}", file=sys.stderr)
        return 4

    print("Loading Silero VAD...", flush=True)
    try:
        vad = onnx_asr.load_vad("silero")
    except Exception as e:
        print(f"error: could not load Silero VAD: {e}", file=sys.stderr)
        return 5

    print("Transcribing...", flush=True)
    cues = []
    try:
        for res in model.with_vad(vad).recognize(args.audio):
            for s, e, text in split_segment(
                    res.start, res.end, res.text, args.max_cue_chars, args.max_cue_duration):
                cues.append((s, e, text))
                print(f"[{fmt_short(s)} --> {fmt_short(e)}] {text}", flush=True)
    except Exception as e:
        print(f"error: transcription failed: {e}", file=sys.stderr)
        return 6

    out_dir = args.output_dir or os.path.dirname(os.path.abspath(args.audio))
    base = os.path.splitext(os.path.basename(args.audio))[0]
    out_path = os.path.join(out_dir, base + ".srt")

    with open(out_path, "w", encoding="utf-8") as f:
        for i, (s, e, text) in enumerate(cues, start=1):
            f.write(f"{i}\n{fmt_timestamp(s)} --> {fmt_timestamp(e)}\n{text}\n\n")

    print(f"Wrote {len(cues)} cues to {out_path}", flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
