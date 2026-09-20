"""Build per-language TTS reference-voice packs from Multilingual LibriSpeech (CC BY 4.0).

Uses the Hugging Face datasets-server rows API (no dataset download), picks clean
6-12 s clips from distinct speakers, converts to 22.05 kHz mono 16-bit WAV, guesses
speaker gender from median pitch, writes a .txt transcript sidecar per voice, an
ATTRIBUTION.txt, and zips each language into voices-<lang>.zip.
"""
import json, os, sys, time, subprocess, urllib.request, urllib.parse, wave, zipfile, shutil
import numpy as np

LANGS = {
    "german": ("German", "de"), "french": ("French", "fr"), "spanish": ("Spanish", "es"),
    "italian": ("Italian", "it"), "dutch": ("Dutch", "nl"), "polish": ("Polish", "pl"),
    "portuguese": ("Portuguese", "pt"),
}
PER_LANG = 8          # voices per pack
MIN_S, MAX_S = 6.0, 11.0
ROWS_TO_SCAN = 1200
OUT = os.path.dirname(os.path.abspath(__file__))


def rows(config, offset, length=100):
    url = ("https://datasets-server.huggingface.co/rows?dataset=facebook%2Fmultilingual_librispeech"
           f"&config={config}&split=test&offset={offset}&length={length}")
    for attempt in range(8):
        try:
            with urllib.request.urlopen(url, timeout=60) as r:
                return json.load(r)["rows"]
        except urllib.error.HTTPError as e:
            if e.code != 429:
                raise
            time.sleep(20 * (attempt + 1))
    raise RuntimeError("rate limited")


def median_f0(wav_path):
    with wave.open(wav_path) as w:
        sr = w.getframerate()
        x = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16).astype(np.float32) / 32768
    frame, hop = int(sr * 0.04), int(sr * 0.02)
    f0s = []
    for i in range(0, len(x) - frame, hop):
        seg = x[i:i + frame]
        if np.sqrt(np.mean(seg ** 2)) < 0.02:
            continue
        seg = seg - seg.mean()
        ac = np.correlate(seg, seg, "full")[frame - 1:]
        lo, hi = int(sr / 400), int(sr / 70)
        k = lo + int(np.argmax(ac[lo:hi]))
        if ac[k] > 0.3 * ac[0]:
            f0s.append(sr / k)
    return float(np.median(f0s)) if f0s else 0.0


def tidy(t):
    t = t.strip()
    if t and t[0].islower():
        t = t[0].upper() + t[1:]
    if t and t[-1] not in ".!?":
        t += "."
    return t


def build(config):
    lang_name, code = LANGS[config]
    if os.path.exists(os.path.join(OUT, f"voices-{code}.zip")):
        print(config, "already built"); return
    folder = os.path.join(OUT, "build", code)
    shutil.rmtree(folder, ignore_errors=True)
    os.makedirs(folder)
    seen, picked = set(), []
    for off in range(0, ROWS_TO_SCAN, 100):
        for r in rows(config, off):
            row = r["row"]
            d, spk = row["audio_duration"], row["speaker_id"]
            if spk in seen or not (MIN_S <= d <= MAX_S):
                continue
            words = row["transcript"].split()
            if len(words) < 12:
                continue
            seen.add(spk)
            picked.append(row)
        if len(picked) >= PER_LANG * 3:
            break
    voices = []
    for row in picked:
        src = row["audio"][0]["src"]
        tmp = os.path.join(folder, "tmp.opus")
        urllib.request.urlretrieve(src, tmp); time.sleep(1.5)
        wav = os.path.join(folder, f"cand_{len(voices):02d}.wav")
        subprocess.run(["ffmpeg", "-y", "-v", "error", "-i", tmp, "-ac", "1", "-ar", "22050",
                        "-sample_fmt", "s16", "-af", "loudnorm=I=-18:TP=-1.5", wav], check=True)
        f0 = median_f0(wav)
        voices.append((f0, wav, row))
        os.remove(tmp)
        if len(voices) >= PER_LANG * 3:
            break
    females = sorted([v for v in voices if v[0] >= 185], key=lambda v: -v[0])
    males = sorted([v for v in voices if 0 < v[0] < 150], key=lambda v: v[0])
    half = PER_LANG // 2
    chosen = [("female", v) for v in females[:half]] + [("male", v) for v in males[:half]]
    # top up from whichever side has spares
    spare = [("female", v) for v in females[half:]] + [("male", v) for v in males[half:]]
    chosen += spare[:PER_LANG - len(chosen)]
    counters, attrib = {"female": 0, "male": 0}, []
    for gender, (f0, wav, row) in chosen:
        counters[gender] += 1
        base = f"{lang_name}_{gender}_{counters[gender]:02d}"
        os.replace(wav, os.path.join(folder, base + ".wav"))
        with open(os.path.join(folder, base + ".txt"), "w", encoding="utf-8", newline="\n") as f:
            f.write(tidy(row["transcript"]))
        attrib.append(f"{base}.wav: MLS {config} test set, speaker {row['speaker_id']}, {row['original_path']} (median F0 {f0:.0f} Hz)")
    for stale in ("tmp.wav",):
        p = os.path.join(folder, stale)
        if os.path.exists(p):
            os.remove(p)
    for _, wav, _ in voices:
        if os.path.exists(wav):
            os.remove(wav)
    with open(os.path.join(folder, "ATTRIBUTION.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write(f"{lang_name} reference voices for Subtitle Edit text-to-speech voice cloning.\n\n"
                "Source: Multilingual LibriSpeech (MLS), Pratap et al. 2020, derived from LibriVox\n"
                "public-domain audiobook recordings. Licensed under CC BY 4.0\n"
                "(https://creativecommons.org/licenses/by/4.0/). https://www.openslr.org/94/\n\n"
                "Each .wav has a .txt sidecar with the spoken transcript (ref-text).\n\n" + "\n".join(attrib) + "\n")
    with open(os.path.join(folder, "pack.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump({"name": f"{lang_name} voices", "language": code, "voices": len(chosen),
                   "license": "CC BY 4.0", "source": "Multilingual LibriSpeech (LibriVox)"}, f, indent=2)
    zip_path = os.path.join(OUT, f"voices-{code}.zip")
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as z:
        for name in sorted(os.listdir(folder)):
            z.write(os.path.join(folder, name), name)
    print(f"{config}: {len(chosen)} voices -> {zip_path} ({os.path.getsize(zip_path)//1024} KB)")


if __name__ == "__main__":
    for cfg in (sys.argv[1:] or LANGS):
        build(cfg)
