#!/usr/bin/env python3
"""Musica de Squash Clash: dos bucles originales sintetizados.

Original de este proyecto: cada nota se calcula aqui con senos, envolventes y ruido de un generador
congruencial lineal (LCG) de 32 bits. No usa samples, grabaciones, MIDI ni medios de terceros; solo la
biblioteca estandar de Python 3. Es determinista: imprime el SHA-256 de cada archivo.

Uso:  python3 scripts/audio/make_music.py [directorio_de_salida]
Salida por defecto: Assets/SquashClash/Runtime/Resources/Audio/music-menu.wav y music-match.wav
Formato: PCM mono de 16 bits a 22050 Hz; cada muestra se limita a [-1, 1] y se convierte con int(v*32767).

Los dos bucles no tienen costura: cada nota se suma en un bufer circular, de modo que la cola de las
ultimas notas cae sobre el comienzo. La partitura (acordes, notas y niveles) esta en las funciones de abajo
y resumida en Assets/SquashClash/Runtime/Resources/Audio/PROVENANCE.md.
"""
import hashlib
import math
import struct
import sys
import wave
from pathlib import Path

RATE = 22050
TAU = 2 * math.pi


class Lcg:
    """Ruido blanco en [-1, 1]: x <- (1664525 x + 1013904223) mod 2^32, valor = x / 2^31 - 1."""

    def __init__(self, seed):
        self.state = seed

    def __call__(self):
        self.state = (1664525 * self.state + 1013904223) & 0xffffffff
        return self.state / 2147483648.0 - 1.0


def hz(midi):
    """Temperamento igual, la4 (nota MIDI 69) = 440 Hz."""
    return 440.0 * 2 ** ((midi - 69) / 12)


class Loop:
    """Bufer circular: lo que una nota deja mas alla del final se suma al comienzo."""

    def __init__(self, seconds):
        self.count = int(round(seconds * RATE))
        self.data = [0.0] * self.count

    def add(self, start_seconds, seconds, sample):
        first, data, count = int(round(start_seconds * RATE)), self.data, self.count
        for i in range(int(seconds * RATE)):
            data[(first + i) % count] += sample(i / RATE)

    def normalized(self, peak):
        top = max(abs(v) for v in self.data)
        return [v * peak / top for v in self.data]


def tone(frequency, partials):
    """Suma de armonicos: partials = [(multiplo, nivel), ...]."""
    return lambda t: sum(level * math.sin(TAU * frequency * k * t) for k, level in partials)


def swell(loop, start, hold, frequency, level, attack, release, partials):
    """Nota sostenida: sube en `attack`, se mantiene hasta `hold` y cae en `release` (medio coseno)."""
    wave_at = tone(frequency, partials)

    def sample(t):
        if t < attack:
            gain = .5 - .5 * math.cos(math.pi * t / attack)
        elif t < hold:
            gain = 1.0
        else:
            gain = .5 + .5 * math.cos(math.pi * min(1.0, (t - hold) / release))
        return level * gain * wave_at(t)

    loop.add(start, hold + release, sample)


def pluck(loop, start, frequency, level, decay, partials, attack=.004):
    """Nota pulsada: ataque corto y caida exponencial."""
    wave_at = tone(frequency, partials)
    seconds = min(4.0, 7.0 / decay)
    loop.add(start, seconds, lambda t: level * min(1.0, t / attack) * math.exp(-decay * t) * wave_at(t))


def menu_loop():
    """Menu: 8 compases de 4/4 a 100 pulsos por minuto (19,2 s). La menor - Fa - Do - Sol, dos veces."""
    beat, bars = .6, 8
    loop = Loop(bars * 4 * beat)
    # Por compas: bajo, y las tres notas del acorde (numeros MIDI).
    chords = [(45, (57, 60, 64)), (41, (57, 60, 65)), (48, (55, 60, 64)), (43, (55, 59, 62))] * 2
    soft, round_, bright = [(1, 1), (2, .28), (3, .08)], [(1, 1), (2, .22)], [(1, 1), (2, .40), (3, .15), (4, .05)]
    # El bajo lleva armonicos fuertes: el parlante de un telefono casi no reproduce su fundamental (87 a 131 Hz).
    low = [(1, 1), (2, .50), (3, .25)]
    for bar, (bass, notes) in enumerate(chords):
        at = bar * 4 * beat
        # Colchon: el acorde entero, sostenido todo el compas.
        for note in notes:
            swell(loop, at, 4 * beat - .1, hz(note), .060, .25, .55, soft)
        # Bajo: tiempos 1 y 3 y una corchea de entrada al compas siguiente.
        for offset, length in ((0, 1.4), (2, .9), (3.5, .45)):
            swell(loop, at + offset * beat, length * beat, hz(bass), .15, .012, .12, low)
        # Arpegio en corcheas una octava arriba: sube y baja por el acorde; acento en los tiempos.
        for eighth, step in enumerate((0, 1, 2, 1, 0, 1, 2, 1)):
            pluck(loop, at + eighth * beat / 2, hz(notes[step] + 12), .075 if eighth % 2 == 0 else .050, 6.0, bright)
    # Melodia, solo en la segunda vuelta (compases 5 a 8): (tiempo dentro del compas, duracion en tiempos, nota).
    melody = [
        [(0, 1, 76), (1, .5, 74), (1.5, .5, 72), (2, 2, 69)],
        [(0, 1, 69), (1, 1, 72), (2, 2, 77)],
        [(0, 1, 79), (1, 1, 76), (2, 1, 72), (3, 1, 76)],
        [(0, 2, 74), (2, 1, 71), (3, 1, 67)],
    ]
    for bar, notes in enumerate(melody):
        for offset, length, note in notes:
            pluck(loop, (bar + 4) * 4 * beat + offset * beat, hz(note), .10, 2.6 / length ** .5, round_, attack=.015)
    # Percusion suave: bombo en 1 y 3, escobilla en 2 y 4, platillo cerrado en los contratiempos.
    noise = Lcg(41)
    hiss = [noise() for _ in range(RATE)]
    for bar in range(bars):
        at = bar * 4 * beat
        for b in (0, 2):
            loop.add(at + b * beat, .22, lambda t: .15 * math.exp(-17 * t) * math.sin(TAU * (48 * t + (105 - 48) * (1 - math.exp(-30 * t)) / 30)))
        for b in (1, 3):
            state = [0.0]

            def brush(t, state=state):
                x = hiss[int(t * RATE)]
                state[0] += .45 * (x - state[0])
                return .045 * min(1.0, t / .003) * math.exp(-34 * t) * state[0]

            loop.add(at + b * beat, .16, brush)
        for eighth in (1, 3, 5, 7):
            state = [0.0]

            def hat(t, state=state):
                x = hiss[int(t * RATE) + 7000]
                state[0] += .35 * (x - state[0])
                return .030 * math.exp(-70 * t) * (x - state[0])

            loop.add(at + eighth * beat / 2, .08, hat)
    return loop.normalized(.60)


def match_loop():
    """Partido: 8 compases de 4/4 a 120 pulsos por minuto (16 s). Sin percusion ni ataques bruscos: nada que
    se pueda confundir con un golpe o un bote. Un colchon de quintas que late en corcheas y una campana lenta."""
    beat, bars = .5, 8
    loop = Loop(bars * 4 * beat)
    roots = [57, 57, 53, 53, 60, 60, 55, 55]
    bells = {0: 76, 2: 72, 4: 79, 6: 74}
    soft = [(1, 1), (2, .25), (3, .06)]
    for bar, root in enumerate(roots):
        at = bar * 4 * beat
        for note, level in ((root, .10), (root + 7, .07), (root + 12, .04)):
            frequency = hz(note)
            wave_at = tone(frequency, soft)

            def pulse(t, wave_at=wave_at, level=level):
                # Entra y sale en medio coseno; late en corcheas (3+3+2) sin llegar nunca a cero.
                edge = .5 - .5 * math.cos(math.pi * min(1.0, t / .35, max(0.0, (4 * beat + .35 - t) / .35)))
                eighth = (t / (beat / 2)) % 8
                accent = 1.0 if int(eighth) in (0, 3, 6) else .55
                throb = .55 + .45 * accent * (.5 - .5 * math.cos(TAU * (eighth % 1)))
                return level * edge * throb * wave_at(t)

            loop.add(at, 4 * beat + .35, pulse)
        if bar in bells:
            frequency = hz(bells[bar])
            loop.add(at + beat, 3.2, lambda t, f=frequency: .055 * min(1.0, t / .05) * math.exp(-1.6 * t) * (math.sin(TAU * f * t) + .2 * math.sin(TAU * 2 * f * t)))
    return loop.normalized(.50)


LOOPS = {'music-menu': menu_loop, 'music-match': match_loop}


def main():
    root = Path(__file__).resolve().parents[2]
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else root / 'Assets/SquashClash/Runtime/Resources/Audio'
    out.mkdir(parents=True, exist_ok=True)
    for name, make in LOOPS.items():
        samples = make()
        path = out / f'{name}.wav'
        with wave.open(str(path), 'wb') as f:
            f.setnchannels(1)
            f.setsampwidth(2)
            f.setframerate(RATE)
            f.writeframes(b''.join(struct.pack('<h', int(max(-1.0, min(1.0, v)) * 32767)) for v in samples))
        peak = max(abs(v) for v in samples)
        rms = math.sqrt(sum(v * v for v in samples) / len(samples))
        print(f'{name}.wav {len(samples) / RATE:.2f} s peak={peak:.3f} rms={rms:.3f} sha256={hashlib.sha256(path.read_bytes()).hexdigest()}')


if __name__ == '__main__':
    main()
