#!/usr/bin/env python3
"""Capa de sonido de Squash Clash (AudioScape): sonidos originales sintetizados.

Original de este proyecto: cada clip se calcula aqui con senos, envolventes exponenciales y ruido de
un generador congruencial lineal (LCG) de 32 bits. No usa samples, grabaciones ni medios de terceros;
solo la biblioteca estandar de Python 3. Es determinista: imprime el SHA-256 de cada archivo.

Uso:  python3 scripts/audio/make_audioscape.py [directorio_de_salida]
Salida por defecto: Assets/SquashClash/Runtime/Resources/Audio/scape-*.wav
Formato: PCM mono de 16 bits a 22050 Hz; cada muestra se limita a [-1, 1] y se convierte con int(v*32767).

No toca impact-perfect, impact-valid, contact-provisional ni swish-provisional (audio de contacto del jugador).
Las formulas estan en Assets/SquashClash/Runtime/Resources/Audio/PROVENANCE.md y en las funciones de abajo.
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


def lowpass(samples, cutoff_hz):
    """Paso bajo de un polo: y += k (x - y), k = 1 - exp(-2 pi fc / fs)."""
    k, y, out = 1 - math.exp(-TAU * cutoff_hz / RATE), 0.0, []
    for x in samples:
        y += k * (x - y)
        out.append(y)
    return out


def noise(seed, count, cutoff_hz):
    source = Lcg(seed)
    return lowpass([source() for _ in range(count)], cutoff_hz)


def render(seconds, sample):
    return [sample(i / RATE, i) for i in range(int(round(seconds * RATE)))]


def sweep(f0, f1, rate):
    """Fase de un seno cuya frecuencia baja de f0 a f1 con constante `rate` (1/s): integral de f1 + (f0-f1) e^(-rate t)."""
    return lambda t: TAU * (f1 * t + (f0 - f1) * (1 - math.exp(-rate * t)) / rate)


def attack(t, seconds):
    return min(1.0, t / seconds)


def bounce_floor():  # bote en el piso: golpe sordo y grave
    n, phase = noise(31, RATE, 1800), sweep(150, 95, 30)
    return render(.080, lambda t, i: attack(t, .0015) * (.50 * math.sin(phase(t)) * math.exp(-38 * t) + .18 * n[i] * 3 * math.exp(-140 * t)))


def bounce_wall():  # paredes laterales y trasera: toque medio, mas seco
    n = noise(32, RATE, 3000)
    return render(.070, lambda t, i: attack(t, .0010) * (.42 * math.sin(TAU * 310 * t) * math.exp(-60 * t) + .16 * math.sin(TAU * 640 * t) * math.exp(-90 * t) + .14 * n[i] * 2 * math.exp(-200 * t)))


def front_wall():  # frontis: el golpe mas lleno de la cancha
    n, phase = noise(33, RATE, 2400), sweep(120, 80, 22)
    return render(.130, lambda t, i: attack(t, .0015) * (.55 * math.sin(phase(t)) * math.exp(-26 * t) + .22 * math.sin(TAU * 370 * t) * math.exp(-55 * t) + .20 * n[i] * 2.5 * math.exp(-110 * t)))


def rival_step():  # paso del rival: pisada sorda con un chirrido corto de zapatilla
    n, thud, squeak = noise(34, RATE, 1200), sweep(110, 70, 40), sweep(1900, 2500, -18)
    return render(.090, lambda t, i: attack(t, .002) * (.35 * math.sin(thud(t)) * math.exp(-55 * t) + .10 * n[i] * 3 * math.exp(-90 * t))
                  + (.10 * math.sin(squeak(t)) * math.sin(math.pi * t / .050) if t < .050 else 0.0))


def rival_racket():  # raqueta del rival: grave y redonda, sin ruido de ataque (distinta de los impactos del jugador)
    return render(.070, lambda t, i: (1 - math.exp(-t / .002)) * (.38 * math.sin(TAU * 270 * t) * math.exp(-45 * t) + .14 * math.sin(TAU * 610 * t) * math.exp(-90 * t)))


def two_notes(first_hz, second_hz, level, decay):
    def note(t, hz):
        return 0.0 if t < 0 else attack(t, .008) * (math.sin(TAU * hz * t) + .25 * math.sin(TAU * 2 * hz * t)) * math.exp(-decay * t)
    return render(.420, lambda t, i: level * (note(t, first_hz) + note(t - .140, second_hz)) * min(1.0, (.420 - t) / .030))


def point_won():  # punto ganado: dos notas que suben (mi5, si5)
    return two_notes(659.25, 987.77, .30, 9)


def point_lost():  # punto perdido: dos notas que bajan (sol4, re4), mas suaves
    return two_notes(392.00, 293.66, .26, 8)


def ui_click():  # clic de interfaz
    n = Lcg(35)
    return render(.030, lambda t, i: .30 * math.sin(TAU * 1150 * t) * math.exp(-170 * t) + .08 * n() * math.exp(-600 * t))


def ambience():  # ambiente de cancha: aire de sala grave y un zumbido leve; 4 s en bucle sin costura
    seconds, fade = 4.0, .5
    count, overlap = int(seconds * RATE), int(fade * RATE)
    raw = lowpass(noise(36, count + overlap, 220), 220)
    peak = max(abs(v) for v in raw)
    air = []
    for i in range(count):
        if i < overlap:  # la cola se funde con el comienzo (potencia constante) para cerrar el bucle
            a = i / overlap
            air.append((raw[i] * math.sin(a * math.pi / 2) + raw[count + i] * math.cos(a * math.pi / 2)) / peak)
        else:
            air.append(raw[i] / peak)
    return [.26 * air[i] + .012 * math.sin(TAU * 100 * i / RATE) + .006 * math.sin(TAU * 200 * i / RATE) for i in range(count)]


CLIPS = (('scape-bounce-floor', bounce_floor), ('scape-bounce-wall', bounce_wall), ('scape-front-wall', front_wall),
         ('scape-rival-step', rival_step), ('scape-rival-racket', rival_racket), ('scape-point-won', point_won),
         ('scape-point-lost', point_lost), ('scape-ui-click', ui_click), ('scape-ambience', ambience))


def main():
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parents[2] / 'Assets/SquashClash/Runtime/Resources/Audio'
    out.mkdir(parents=True, exist_ok=True)
    for name, build in CLIPS:
        samples = build()
        data = b''.join(struct.pack('<h', int(max(-1.0, min(1.0, v)) * 32767)) for v in samples)
        path = out / (name + '.wav')
        with wave.open(str(path), 'wb') as f:
            f.setnchannels(1)
            f.setsampwidth(2)
            f.setframerate(RATE)
            f.writeframes(data)
        peak = max(abs(v) for v in samples)
        print(f'{name}.wav {len(samples) / RATE * 1000:.0f} ms peak={peak:.3f} sha256={hashlib.sha256(path.read_bytes()).hexdigest()}')


if __name__ == '__main__':
    main()
