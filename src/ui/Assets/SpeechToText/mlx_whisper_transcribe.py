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
                # getattr: huggingface_hub can drive this class through paths where
                # the tqdm base has not populated .desc yet.
                label = (getattr(self, "desc", None) or "Downloading model").strip().rstrip(":") or "Downloading model"
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


def enable_beam_search():
    """Add beam search decoding to mlx-whisper, which only ships a greedy decoder.

    This is a port of openai/whisper's reference BeamSearchDecoder to MLX arrays,
    installed by swapping the decoder inside DecodingTask. The library is already
    prepared for it: the batch tiling for n_group > 1, the likelihood ranker, and
    Inference.rearrange_kv_cache all exist; only the decoder class is missing
    (requesting beam_size raises NotImplementedError upstream). Returns True when
    the patch is active.
    """
    import dataclasses

    import mlx.core as mx
    import numpy as np
    from mlx.utils import tree_map
    from mlx_whisper import decoding

    if getattr(decoding.DecodingTask, "_se_beam_patch", False):
        return True

    class BeamSearchDecoder(decoding.TokenDecoder):
        def __init__(self, beam_size, eot, inference, patience=None):
            self.beam_size = beam_size
            self.eot = eot
            self.inference = inference
            self.patience = patience or 1.0
            self.max_candidates = round(beam_size * self.patience)
            self.finished_sequences = None
            self._rows = None
            if self.max_candidates <= 0:
                raise ValueError("Invalid beam size / patience")

        def reset(self):
            self.finished_sequences = None
            self._rows = None

        def _rearrange_kv_cache(self, source_indices):
            # The library's rearrange_kv_cache reindexes every cached array, but
            # the cache mixes self attention entries (batch = number of beams)
            # with cross attention entries (batch = number of audios). Indexing
            # the latter with beam indices reads out of bounds, which MLX does
            # not check on the GPU, silently corrupting the decode. Only arrays
            # whose batch dimension matches the beam count are reordered; the
            # cross attention rows are identical across beams anyway.
            if source_indices == list(range(len(source_indices))):
                return
            n = len(source_indices)
            idx = mx.array(source_indices)
            self.inference.kv_cache = tree_map(
                lambda x: mx.take(x, idx, axis=0) if x.shape[0] == n else x,
                self.inference.kv_cache)

        def update(self, tokens, logits, sum_logprobs):
            if tokens.shape[0] % self.beam_size != 0:
                raise ValueError("batch size is not a multiple of beam size")
            n_audio = tokens.shape[0] // self.beam_size
            if self.finished_sequences is None:
                self.finished_sequences = [{} for _ in range(n_audio)]

            # Beam bookkeeping happens in python like the reference implementation,
            # but the top-k selection runs on the GPU first so only beam_size + 1
            # candidates per beam cross to the CPU each step instead of the whole
            # vocabulary-sized probability matrix.
            k = self.beam_size + 1
            logprobs = logits - mx.logsumexp(logits, axis=-1, keepdims=True)
            top_idx = mx.argpartition(logprobs, -k, axis=-1)[:, -k:]
            top_val = mx.take_along_axis(logprobs, top_idx, axis=-1)
            top_idx = np.array(top_idx)
            top_val = np.array(top_val)
            sums = np.array(sum_logprobs)
            # The candidate rows are rebuilt in python each step, so reuse them
            # instead of pulling the whole growing token matrix off the GPU.
            rows = self._rows
            if rows is None:
                rows = np.array(tokens).tolist()

            next_tokens, source_indices, new_sums, newly_finished = [], [], [], []
            for i in range(n_audio):
                scores, sources, finished = {}, {}, {}
                # Rank every (beam, candidate token) continuation by cumulative
                # log probability; the dict keyed by full sequence also merges
                # beams that converged onto identical hypotheses.
                for j in range(self.beam_size):
                    idx = i * self.beam_size + j
                    for token, val in zip(top_idx[idx], top_val[idx]):
                        sequence = tuple(rows[idx] + [int(token)])
                        scores[sequence] = float(sums[idx] + val)
                        sources[sequence] = idx

                saved = 0
                for sequence in sorted(scores, key=scores.get, reverse=True):
                    if sequence[-1] == self.eot:
                        finished[sequence] = scores[sequence]
                    else:
                        new_sums.append(scores[sequence])
                        next_tokens.append(list(sequence))
                        source_indices.append(sources[sequence])
                        saved += 1
                        if saved == self.beam_size:
                            break
                newly_finished.append(finished)

            self._rows = next_tokens
            tokens = mx.array(next_tokens)
            self._rearrange_kv_cache(source_indices)

            for previously, newly in zip(self.finished_sequences, newly_finished):
                for seq in sorted(newly, key=newly.get, reverse=True):
                    if len(previously) >= self.max_candidates:
                        break
                    previously[seq] = newly[seq]

            completed = all(
                len(sequences) >= self.max_candidates
                for sequences in self.finished_sequences
            )
            return tokens, completed, mx.array(new_sums)

        def finalize(self, preceding_tokens, sum_logprobs):
            # Collect the finished hypotheses, topping up from the best still
            # running beams when too few finished naturally. The caller expects
            # rectangular arrays and truncates each row at its first EOT, so
            # shorter sequences are padded with EOT.
            sums = np.array(sum_logprobs)
            preceding = np.array(preceding_tokens).tolist()
            groups = []
            for i, sequences in enumerate(self.finished_sequences):
                if len(sequences) < self.beam_size:
                    for j in list(np.argsort(sums[i]))[::-1]:
                        sequence = tuple(preceding[i][j] + [self.eot])
                        sequences.setdefault(sequence, float(sums[i][j]))
                        if len(sequences) >= self.beam_size:
                            break
                groups.append(sequences)

            count = max(len(sequences) for sequences in groups)
            length = max(len(seq) for sequences in groups for seq in sequences)
            tokens = []
            logprobs = []
            for sequences in groups:
                ordered = sorted(sequences.items(), key=lambda kv: kv[1], reverse=True)
                while len(ordered) < count:
                    ordered.append(ordered[0])
                tokens.append([list(seq) + [self.eot] * (length - len(seq))
                               for seq, _ in ordered])
                logprobs.append([score for _, score in ordered])
            return mx.array(tokens), mx.array(logprobs)

    original_init = decoding.DecodingTask.__init__

    def patched_init(self, model, options):
        beam_size = options.beam_size
        patience = options.patience
        if beam_size:
            options = dataclasses.replace(
                options, beam_size=None, patience=None, best_of=None)
        original_init(self, model, options)
        if beam_size:
            self.options = dataclasses.replace(
                self.options, beam_size=beam_size, patience=patience)
            self.n_group = beam_size
            self.decoder = BeamSearchDecoder(
                beam_size, self.tokenizer.eot, self.inference, patience)

    decoding.DecodingTask.__init__ = patched_init
    decoding.DecodingTask._se_beam_patch = True
    return True


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


def detect_speech_clips(audio):
    """Silero VAD (via faster-whisper, optional): [start1, end1, start2, ...] seconds of speech.

    mlx-whisper has no voice-activity detection, so music and long silences reach the
    decoder, which reacts with hallucination loops (repeated garbage tokens with
    zero-length timestamps), phantom music labels, and echoes of the initial prompt.
    Clipping the decode to the detected speech regions kills all of that at the root.
    Returns None when faster-whisper's VAD is not installed or detection fails.
    """
    try:
        from faster_whisper.vad import get_speech_timestamps
    except Exception:
        print("note: faster-whisper VAD not available; decoding the full audio "
              "(pip3 install faster-whisper enables music/silence skipping)", flush=True)
        return None

    try:
        regions = get_speech_timestamps(audio)
    except Exception as e:
        print(f"warning: VAD failed ({e}); decoding the full audio", file=sys.stderr, flush=True)
        return None

    if not regions:
        return None

    clips = []
    for region in regions:
        clips.extend([region["start"] / 16000.0, region["end"] / 16000.0])
    return clips


def _has_expected_script(text, language):
    """True when the text contains at least one character of the language's script."""
    if language == "ar":
        return any("\u0600" <= c <= "\u06ff" or "\u0750" <= c <= "\u077f" or c.isdigit() for c in text)
    if language in ("en", "tr"):
        return any(("a" <= c.lower() <= "z") or c.isdigit() or c in "çğıöşüÇĞİÖŞÜ" for c in text)
    return True


def sanitize_cues(cues, language=None, prompt=None):
    """Drop decoder-hallucination artifacts before the timing pass sees them.

    Whisper reacts to music and long silences with degenerate output: zero-length
    cues, the same token repeated dozens of times, text in a completely different
    script, and echoes of the initial prompt. VAD clipping prevents most of it at
    the source; this pass is the safety net for whatever still gets through (and
    for runs where VAD was unavailable).
    """
    out = []
    repeats = 0
    for start, end, text in cues:
        if end - start < 0.05:
            continue
        if language and text and not _has_expected_script(text, language):
            continue
        if out and out[-1][2] == text:
            repeats += 1
            if repeats >= 2:
                # A third+ identical adjacent cue is a hallucination loop, not dialogue.
                continue
        else:
            repeats = 0
        out.append((start, end, text))

    # Leading cues that only quote the injected punctuation prompt are echoes the
    # decoder produced against intro music/silence, not speech.
    if prompt:
        lead = 0
        for _, _, text in out:
            if text and text in prompt:
                lead += 1
            else:
                break
        out = out[lead:]

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
    parser.add_argument("--beam-size", type=int, default=0,
                        help="Decode with beam search of this width (0 = greedy). "
                             "Higher accuracy on hard speech at roughly beam-size times the decode cost.")
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
    if use_words:
        # Built-in hallucination guard: with word timestamps available, skip silent
        # stretches longer than this when the decoder output looks hallucinated.
        transcribe_kwargs["hallucination_silence_threshold"] = 2.0

    # When a segment fails the quality checks and is retried at a higher
    # temperature, sample five candidates and keep the most likely one, matching
    # openai/whisper's default. mlx-whisper drops this option at temperature 0,
    # so the fast greedy path is unaffected; only hard segments pay for it.
    transcribe_kwargs["best_of"] = 5

    if args.beam_size and args.beam_size > 1:
        try:
            enable_beam_search()
            transcribe_kwargs["beam_size"] = args.beam_size
            print(f"Beam search enabled (beam size {args.beam_size}).", flush=True)
        except Exception as e:
            print(f"warning: beam search unavailable ({e}); decoding greedily",
                  file=sys.stderr, flush=True)

    # Whisper drops punctuation on some real-world speech (dialectal Arabic notably)
    # even with sequential decoding, and punctuation drives the sentence-based cue
    # splitting. A short punctuated prompt in the audio's language restores it, but
    # the initial prompt only reaches the first decoding window, so over a long file
    # its effect decays. Decoding each VAD speech chunk as its own call (below)
    # re-applies the prompt at every chunk, which keeps punctuation practically
    # present across a full episode instead of only near the start.
    # A user prompt (vocabulary: names, places, technical terms) combines with the
    # punctuation prompt instead of replacing it, so custom terms never cost
    # punctuation. Whisper only reads the final 224 tokens of a prompt, which both
    # of these together stay well within.
    prompt = args.initial_prompt
    if not args.no_punctuation_prompt and language:
        punctuation_prompt = PUNCTUATION_PROMPTS.get(language.lower())
        if punctuation_prompt:
            prompt = f"{punctuation_prompt} {prompt}".strip() if prompt else punctuation_prompt

    collected = []
    words = []

    def decode_piece(piece, offset, piece_prompt):
        """Transcribe one audio piece, printing and collecting absolute-time output."""
        kwargs = dict(transcribe_kwargs)
        if piece_prompt:
            kwargs["initial_prompt"] = piece_prompt
        result = mlx_whisper.transcribe(
            piece,
            path_or_hf_repo=args.model,
            language=language,
            task=args.task,
            word_timestamps=use_words,
            verbose=None,
            **kwargs,
        )
        piece_texts = []
        for seg in result.get("segments", []):
            text = (seg.get("text") or "").strip()
            start = offset + float(seg.get("start", 0.0))
            end = offset + float(seg.get("end", 0.0))
            # Live line per segment, in absolute time (the pieces are chunk-relative).
            print(f"[{fmt_short(start)} --> {fmt_short(end)}] {text}", flush=True)
            collected.append((start, end, text))
            piece_texts.append(text)
            if use_words:
                for w in seg.get("words") or []:
                    words.append((offset + float(w.get("start", 0.0)),
                                  offset + float(w.get("end", 0.0)),
                                  w.get("word") or ""))
        return " ".join(piece_texts)

    # Skip music and long silences via VAD when possible (see detect_speech_clips);
    # only possible when the audio was decoded to an array (Subtitle Edit always
    # feeds a 16 kHz mono WAV, so this is the normal path). Nearby regions merge
    # into chunks so the per-chunk overhead stays negligible, capped so the
    # punctuation prompt is refreshed at least every few minutes.
    clips = None if isinstance(audio_input, str) else detect_speech_clips(audio_input)
    if clips:
        chunks = []
        chunk_start, chunk_end = clips[0], clips[1]
        for i in range(2, len(clips), 2):
            start, end = clips[i], clips[i + 1]
            if start - chunk_end <= 3.0 and end - chunk_start <= 240.0:
                chunk_end = end
            else:
                chunks.append((chunk_start, chunk_end))
                chunk_start, chunk_end = start, end
        chunks.append((chunk_start, chunk_end))

        speech_seconds = sum(end - start for start, end in chunks)
        total_seconds = len(audio_input) / 16000.0
        print(f"VAD: {len(chunks)} speech chunk(s), "
              f"{speech_seconds / 60.0:.1f} of {total_seconds / 60.0:.1f} minutes to decode.",
              flush=True)

        # Carry a tail of the previous chunk's text so cross-chunk context (names,
        # style, and the punctuation habit) persists like sequential conditioning.
        previous_tail = ""
        for chunk_start, chunk_end in chunks:
            chunk_prompt = prompt or ""
            if previous_tail:
                chunk_prompt = (chunk_prompt + " " + previous_tail).strip()
            piece = audio_input[int(chunk_start * 16000):int(chunk_end * 16000)]
            chunk_text = decode_piece(piece, chunk_start, chunk_prompt or None)
            if chunk_text:
                previous_tail = chunk_text[-150:]
    else:
        decode_piece(audio_input, 0.0, prompt)

    if use_words and words:
        cues = resegment(words, args.max_cue_chars, args.max_cue_duration)
        cues = sanitize_cues(cues, language, prompt)
        cues = apply_standards(cues, args.max_cue_chars, args.max_cue_duration, args.max_cps)
        print(f"Resegmented {len(collected)} raw segment(s) into {len(cues)} subtitle cue(s) "
              f"(max {args.max_cue_chars} chars / {args.max_cue_duration:g}s per cue, "
              f"max {args.max_cps:g} chars/second).", flush=True)
        collected = cues
    elif use_words:
        print("No word timestamps returned; keeping Whisper's raw segments.", flush=True)
        collected = sanitize_cues(collected, language, prompt)

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
