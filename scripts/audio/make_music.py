#!/usr/bin/env python3
"""Squash Clash music: two original synthesized loops (energetic version, 16 nov 2026).

Original to this project: every note is computed here from sine sums, envelopes and a 32-bit LCG noise source.
No samples, recordings, MIDI or third-party material; Python 3 standard library only. Deterministic: prints the
SHA-256 of each file.

Usage:  python3 scripts/audio/make_music.py [output_dir]
Default output: Assets/SquashClash/Runtime/Resources/Audio/music-match.wav and music-menu.wav
  music-match.wav  16.0 s, 150 bpm, E minor, electronic pulse with arpeggios, no percussion
  music-menu.wav   19.2 s, 125 bpm, B minor, synthwave
Format: PCM mono 16 bit 22050 Hz; each sample is clamped to [-1, 1] and converted with int(v*32767).
Loops are seamless: every note is summed into a circular buffer and every periodic gain (pump, filter sweep)
is periodic over the loop. The score is summarized in Assets/SquashClash/Runtime/Resources/Audio/PROVENANCE.md.
"""
import hashlib
import math
import struct
import sys
import wave
from pathlib import Path

RATE = 22050
TAU = 2 * math.pi
sin, cos, exp, pi = math.sin, math.cos, math.exp, math.pi


class Lcg:
    def __init__(self, seed): self.state = seed

    def __call__(self):
        self.state = (1664525 * self.state + 1013904223) & 0xffffffff
        return self.state / 2147483648.0 - 1.0


def hz(m): return 440.0 * 2 ** ((m - 69) / 12)


class Loop:
    def __init__(self, seconds):
        self.count = int(round(seconds * RATE)); self.seconds = seconds
        self.data = [0.0] * self.count

    def add(self, start, seconds, sample):
        first, data, count = int(round(start * RATE)), self.data, self.count
        for i in range(int(seconds * RATE)):
            data[(first + i) % count] += sample(i / RATE)

    def mix_in(self, other, gain=1.0):
        d = self.data
        for i, v in enumerate(other.data): d[i] += gain * v

    def pump(self, segments, atk=.006):
        """Sidechain-style gain: segments = [(from_s, to_s, period_s, depth, release_s)]; periodic, no clicks."""
        d, n = self.data, self.count
        for a, b, period, depth, rel in segments:
            e0 = exp(-(period - atk) / rel)
            for i in range(int(a * RATE), min(n, int(b * RATE))):
                ph = (i / RATE - a) % period
                duck = ph / atk if ph < atk else max(0.0, (exp(-(ph - atk) / rel) - e0) / (1 - e0))
                d[i] *= 1 - depth * duck

    def finish(self, peak, rms_cap, drive=2.0):
        # Gentle tanh saturation packs the level (lower crest) before normalizing; adds a little grit, no new transients.
        t0 = max(abs(v) for v in self.data); nd = math.tanh(drive)
        self.data = [math.tanh(drive * v / t0) / nd for v in self.data]
        top = max(abs(v) for v in self.data)
        scale = peak / top
        rms = math.sqrt(sum(v * v for v in self.data) / self.count) * scale
        if rms > rms_cap: scale *= rms_cap / rms
        return [max(-1.0, min(1.0, v * scale)) for v in self.data]


def note(loop, start, dur, midi, level, attack=.01, release=.04, decay=0., tilt=1., nmax=6, detune=(0.,), vib=0., vrate=5.5):
    """Additive saw-ish voice: partials k^-tilt up to nmax (and below 8 kHz), optional detuned copies and vibrato."""
    f = hz(midi); comps = []
    for d in detune:
        fd = f * (1 + d)
        for k in range(1, max(1, min(nmax, int(8000 / fd))) + 1): comps.append((TAU * fd * k, k ** -tilt, k))
    scale = level / len(detune)

    def sample(t):
        g = .5 - .5 * cos(pi * t / attack) if t < attack else 1.0
        if decay: g *= exp(-decay * t)
        if t > dur: g *= .5 + .5 * cos(pi * min(1.0, (t - dur) / release))
        v = vib * sin(TAU * vrate * t) if vib else 0.0
        s = 0.0
        for w, l, k in comps: s += l * sin(w * t + k * v)
        return scale * g * s

    loop.add(start, dur + release, sample)


def thump(loop, at, level, f0=150., f1=52., sweep=40., decay=15., attack=.005, drive=1.8, length=.3):
    """Soft low kick: sine sweep f0 -> f1, slow attack, mild saturation. No noise, no click."""
    nd = math.tanh(drive)

    def sample(t):
        ph = TAU * (f1 * t + (f0 - f1) * (1 - exp(-sweep * t)) / sweep)
        return level * min(1.0, t / attack) * exp(-decay * t) * math.tanh(drive * sin(ph)) / nd
    loop.add(at, length, sample)


def noise_hit(loop, hiss, off, at, dur, level, decay, lp, hp, attack=.001):
    """Filtered noise burst (menus only): one-pole low-pass `lp`, minus one-pole low-pass `hp` = band-pass."""
    s1, s2, n = [0.0], [0.0], len(hiss)

    def sample(t):
        x = hiss[(int(t * RATE) + off) % n]
        s1[0] += lp * (x - s1[0]); s2[0] += hp * (s1[0] - s2[0])
        return level * min(1.0, t / attack) * exp(-decay * t) * (s1[0] - s2[0])
    loop.add(at, dur, sample)


def build_hiss(seed=41):
    r = Lcg(seed); return [r() for _ in range(RATE)]


def steps(loop_fn, bar_start, step_len, items):
    for it in items: loop_fn(bar_start + it[0] * step_len, *it[1:])


# ---------------------------------------------------------------- MATCH: electronic pulse with arpeggios (music-match.wav)
def match_b():
    """150 bpm, E minor. Em C G D x2, C D. Rolling 16th bass, filter-sweeping 16th arpeggio, echoing bell hook,
    heavily pumped pad. No percussion at all: the pulse is the bass + the pump."""
    beat = .4; s16 = beat / 4; bar = 4 * beat; bars = 10
    main, pad = Loop(bars * bar), Loop(bars * bar)
    prog = ['Em', 'C', 'G', 'D'] * 2 + ['C', 'D']
    bass = {'Em': 40, 'C': 36, 'G': 43, 'D': 38}
    tones = {'Em': (64, 67, 71, 76), 'C': (64, 67, 72, 76), 'G': (62, 67, 71, 74), 'D': (62, 66, 69, 74)}
    arp = [0, 1, 2, 3, 2, 1, 2, 3, 0, 1, 2, 3, 2, 1, 3, 2]
    mel = {'Em': [(0, 2, 83), (3, 2, 79), (6, 2, 76), (8, 4, 79)], 'C': [(0, 2, 84), (3, 2, 79), (6, 2, 76), (8, 4, 72)],
           'G': [(0, 2, 83), (3, 2, 79), (6, 2, 74), (8, 4, 79)], 'D': [(0, 2, 81), (3, 2, 78), (6, 2, 74), (8, 4, 81)]}
    for b, name in enumerate(prog):
        at = b * bar; root = bass[name]
        sweep = .5 - .5 * cos(TAU * (at / (bars * bar)))   # periodic filter sweep 0..1..0
        nmax = 3 + int(round(6 * sweep))
        for k in range(4):
            for i, off in ((1, 12), (2, 0), (3, 12)):
                note(main, at + k * beat + i * s16, 1.0 * s16, root + off, .17 if i != 2 else .14, attack=.015, release=.02, decay=10, tilt=.6, nmax=8)
        for i in range(16):
            lv = .06 if i % 4 == 0 else .045
            note(main, at + i * s16, 1.2 * s16, tones[name][arp[i]], lv, attack=.015, release=.03, decay=11, tilt=1.0, nmax=nmax, detune=(-.004, .004))
        for m in tones[name][:3]: note(pad, at, bar - .05, m - 12, .05, attack=.1, release=.2, tilt=1.2, nmax=4, detune=(-.006, .006))
        if b >= 2 or b == 0:
            for i, ln, m in mel[name]:
                for rep, g in ((0, 1.0), (3, .45), (6, .2)):
                    note(main, at + (i + rep) * s16, ln * s16 + .06, m, .06 * g, attack=.015, release=.10, tilt=.8, nmax=3, detune=(0, .003), vib=.25)
        if b in (8, 9):
            for i in range(8):
                note(main, at + i * 2 * s16, 2 * s16, [72, 76, 79, 84, 79, 76, 79, 83][i], .05, attack=.015, release=.05, tilt=.8, nmax=3, detune=(0, .003))
    pad.pump([(0, bars * bar, beat / 2, .65, .07)])
    main.mix_in(pad)
    return main.finish(.48, .15)


# ---------------------------------------------------------------- MENU: synthwave drive (music-menu.wav)
def menu_a():
    """125 bpm, B minor. Bm G D A x2, G A. Four-on-floor thump, clap on 2/4, 16th hat, rolling bass, pumped pad, bright arp, melody."""
    beat = .48; s16 = beat / 4; bar = 4 * beat; bars = 10
    main, pad = Loop(bars * bar), Loop(bars * bar); hiss = build_hiss()
    prog = ['Bm', 'G', 'D', 'A'] * 2 + ['G', 'A']
    bass = {'Bm': 47, 'G': 43, 'D': 50, 'A': 45}
    tri = {'Bm': (59, 62, 66, 71), 'G': (59, 62, 67, 71), 'D': (62, 66, 69, 74), 'A': (61, 64, 69, 73)}
    mel = {'Bm': [(0, 4, 78), (4, 2, 74), (6, 2, 78), (8, 4, 81), (12, 4, 78)], 'G': [(0, 4, 79), (4, 2, 76), (6, 2, 79), (8, 4, 83), (12, 4, 79)],
           'D': [(0, 4, 81), (4, 2, 78), (6, 2, 81), (8, 4, 86), (12, 4, 81)], 'A': [(0, 4, 85), (4, 2, 81), (6, 2, 85), (8, 6, 88), (14, 2, 85)]}
    for b, name in enumerate(prog):
        at = b * bar
        for k in range(4):
            thump(main, at + k * beat, .22, f0=160, f1=55, decay=14)
            for i in (1, 2, 3): note(main, at + k * beat + i * s16, s16 * 1.2, bass[name] + (12 if i == 2 else 0), .15, attack=.01, release=.02, decay=9, tilt=.6, nmax=7)
        for k in (1, 3): 
            for j, g in ((0, .5), (.011, .6), (.022, 1.0)): noise_hit(main, hiss, 3000 + b * 400 + k * 91, at + k * beat + j, .25, .06 * g, 16 if j == .022 else 70, .55, .09)
        for i in range(1, 16, 2): noise_hit(main, hiss, 9000 + i * 37, at + i * s16, .06, .018, 90, .6, .18)
        for i in range(16):
            note(main, at + i * s16, 1.3 * s16, tri[name][[0, 1, 2, 3, 2, 1, 2, 3, 0, 1, 2, 3, 2, 1, 3, 2][i]], .05, attack=.01, release=.03, decay=11, tilt=1.0, nmax=6, detune=(-.004, .004))
        for m in tri[name][:3]: note(pad, at, bar - .05, m - 12, .05, attack=.2, release=.25, tilt=1.2, nmax=4, detune=(-.006, .006))
        if b >= 4:
            for i, ln, m in mel[name]: note(main, at + i * s16, ln * s16 - .01, m, .07, attack=.015, release=.08, tilt=.9, nmax=4, detune=(0, .003), vib=.3)
    pad.pump([(0, bars * bar, beat, .55, .14)])
    main.mix_in(pad)
    return main.finish(.62, .25)


LOOPS = {'music-match': match_b, 'music-menu': menu_a}
DEFAULT_OUT = Path(__file__).resolve().parents[2] / 'Assets' / 'SquashClash' / 'Runtime' / 'Resources' / 'Audio'


def write_wav(path, samples):
    with wave.open(str(path), 'wb') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(b''.join(struct.pack('<h', int(max(-1.0, min(1.0, v)) * 32767)) for v in samples))


def main():
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_OUT
    out.mkdir(parents=True, exist_ok=True)
    for name, make in LOOPS.items():
        s = make()
        path = out / f'{name}.wav'
        write_wav(path, s)
        peak = max(abs(v) for v in s)
        rms = math.sqrt(sum(v * v for v in s) / len(s))
        print(f'{name}.wav {len(s) / RATE:.3f} s peak={peak:.3f} rms={rms:.3f} sha256={hashlib.sha256(path.read_bytes()).hexdigest()}', flush=True)


if __name__ == '__main__':
    main()
