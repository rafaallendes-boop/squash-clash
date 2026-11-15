#!/usr/bin/env python3
"""Arte de release de Squash Clash: icono 1024x1024 e imagen de pantalla de carga.

Original de este proyecto, generado por este script (solo biblioteca estandar de Python 3, sin
imagenes, fuentes ni assets de terceros). Mismo motivo que el emblema del menu de inicio: el frontis
con sus lineas rojas, la pelota negra de doble punto amarillo y la estela dorada del golpe.

Uso:  python3 scripts/art/make_release_art.py [directorio_de_salida]
Salida por defecto: Assets/SquashClash/Art/Release/{icon-1024.png,launch-screen.png}
Los PNG son RGB de 8 bits, sin canal alfa y sin esquinas redondeadas (el sistema las recorta).
Es determinista: dos corridas con la misma version de Python/zlib dan los mismos bytes; imprime el SHA-256.
"""
import hashlib
import math
import struct
import sys
import zlib
from pathlib import Path

CREAM_TOP, CREAM_BOTTOM = (243, 236, 221), (228, 218, 196)
WOOD_TOP, WOOD_BOTTOM = (212, 160, 100), (184, 130, 74)
TIN_TOP, TIN_BOTTOM = (176, 188, 200), (140, 154, 170)
RED = (216, 40, 31)
GOLD = (255, 210, 63)
BALL, BALL_LIGHT = (20, 23, 29), (86, 96, 112)
NAVY_TOP, NAVY_BOTTOM = (8, 13, 22), (18, 35, 59)
WHITE = (255, 255, 255)


def clamp01(v):
    return 0.0 if v < 0 else 1.0 if v > 1 else v


def mix(a, b, t):
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t)


class Canvas:
    def __init__(self, width, height):
        self.w, self.h = width, height
        self.rows = [bytearray(3 * width) for _ in range(height)]

    def vertical_gradient(self, top, bottom):
        for y in range(self.h):
            c = mix(top, bottom, y / (self.h - 1))
            self.rows[y][:] = bytes((int(c[0] + .5), int(c[1] + .5), int(c[2] + .5))) * self.w

    def paint(self, box, cover, color):
        """Blend `color` (tuple or f(x, y)) where cover(x, y) in 0..1, inside the pixel box (x0, y0, x1, y1)."""
        x0, y0 = max(0, int(math.floor(box[0]))), max(0, int(math.floor(box[1])))
        x1, y1 = min(self.w, int(math.ceil(box[2]))), min(self.h, int(math.ceil(box[3])))
        constant = not callable(color)
        for y in range(y0, y1):
            row, py = self.rows[y], y + .5
            for x in range(x0, x1):
                a = cover(x + .5, py)
                if a <= 0:
                    continue
                c = color if constant else color(x + .5, py)
                i = 3 * x
                if a >= 1:
                    row[i], row[i + 1], row[i + 2] = int(c[0] + .5), int(c[1] + .5), int(c[2] + .5)
                else:
                    row[i] = int(row[i] + (c[0] - row[i]) * a + .5)
                    row[i + 1] = int(row[i + 1] + (c[1] - row[i + 1]) * a + .5)
                    row[i + 2] = int(row[i + 2] + (c[2] - row[i + 2]) * a + .5)

    def png(self):
        raw = b''.join(b'\x00' + bytes(row) for row in self.rows)

        def chunk(kind, data):
            return struct.pack('>I', len(data)) + kind + data + struct.pack('>I', zlib.crc32(kind + data) & 0xffffffff)
        header = struct.pack('>IIBBBBB', self.w, self.h, 8, 2, 0, 0, 0)  # 8-bit RGB: no alpha channel
        return b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', header) + chunk(b'IDAT', zlib.compress(raw, 9)) + chunk(b'IEND', b'')


def rounded_rect(cx, cy, half_w, half_h, radius):
    def cover(x, y):
        qx, qy = abs(x - cx) - (half_w - radius), abs(y - cy) - (half_h - radius)
        outside = math.hypot(max(qx, 0), max(qy, 0)) + min(max(qx, qy), 0) - radius
        return clamp01(.5 - outside)
    return cover


def disc(cx, cy, radius):
    return lambda x, y: clamp01(.5 - (math.hypot(x - cx, y - cy) - radius))


def band(y0, y1):
    return lambda x, y: clamp01(y - y0 + .5) * clamp01(y1 - y + .5)


def both(a, b):
    return lambda x, y: a(x, y) * b(x, y)


def stroke(points, radii, alphas=None):
    """Round-capped polyline whose half-width (and optional opacity) is interpolated along its length."""
    segments = []
    for i in range(len(points) - 1):
        (ax, ay), (bx, by) = points[i], points[i + 1]
        dx, dy = bx - ax, by - ay
        segments.append((ax, ay, dx, dy, dx * dx + dy * dy or 1e-9, radii[i], radii[i + 1],
                         alphas[i] if alphas else 1.0, alphas[i + 1] if alphas else 1.0))

    def cover(x, y):
        best = 0.0
        for ax, ay, dx, dy, length2, r0, r1, a0, a1 in segments:
            t = ((x - ax) * dx + (y - ay) * dy) / length2
            t = 0.0 if t < 0 else 1.0 if t > 1 else t
            d = math.hypot(x - ax - dx * t, y - ay - dy * t) - (r0 + (r1 - r0) * t)
            if d < .5:
                value = clamp01(.5 - d) * (a0 + (a1 - a0) * t)
                if value > best:
                    best = value
        return best
    reach = max(radii) + 1
    xs, ys = [p[0] for p in points], [p[1] for p in points]
    return cover, (min(xs) - reach, min(ys) - reach, max(xs) + reach, max(ys) + reach)


def bezier(p0, p1, p2, count):
    return [((1 - t) ** 2 * p0[0] + 2 * (1 - t) * t * p1[0] + t * t * p2[0],
             (1 - t) ** 2 * p0[1] + 2 * (1 - t) * t * p1[1] + t * t * p2[1]) for t in (i / count for i in range(count + 1))]


def motif(canvas, x, y, w, h, clip=None):
    """The front wall with its lines, the floor, the gold streak of a stroke and the double-yellow ball."""
    inside = clip or (lambda px, py: 1.0)
    box = (x, y, x + w, y + h)
    floor = y + h * .80
    canvas.paint(box, inside, lambda px, py: mix(CREAM_TOP, CREAM_BOTTOM, (py - y) / h))
    canvas.paint((x, floor, x + w, y + h), both(inside, band(floor, y + h + 2)),
                 lambda px, py: mix(WOOD_TOP, WOOD_BOTTOM, (py - floor) / (y + h - floor)))
    tin = y + h * .715
    canvas.paint((x, tin, x + w, floor + 1), both(inside, band(tin, floor)),
                 lambda px, py: mix(TIN_TOP, TIN_BOTTOM, (py - tin) / (floor - tin)))
    line = h * .024
    for at in (.115, .415, .715):
        canvas.paint((x, y + h * at - line, x + w, y + h * at + line), both(inside, band(y + h * at - line / 2, y + h * at + line / 2)), RED)
    # The stroke: a streak that widens and brightens toward the ball.
    cx, cy, radius = x + w * .615, y + h * .525, h * .205
    path = bezier((x + w * .06, y + h * .44), (x + w * .30, y + h * .13), (cx - radius * .25, cy - radius * .25), 28)
    count = len(path) - 1
    cover, reach = stroke(path, [h * (.012 + .100 * (i / count) ** 1.3) for i in range(count + 1)],
                          [.10 + .88 * (i / count) ** .8 for i in range(count + 1)])
    canvas.paint(reach, both(inside, cover), GOLD)
    canvas.paint((cx - radius * 1.4, cy - radius * 1.2, cx + radius * 1.6, cy + radius * 1.7),
                 both(inside, lambda px, py: .20 * clamp01(1.25 - math.hypot((px - cx - radius * .14) / (radius * 1.12), (py - cy - radius * .20) / (radius * 1.12))) ** 1.5), (30, 24, 14))

    def shade(px, py):
        glow = math.exp(-((px - cx + radius * .34) ** 2 + (py - cy + radius * .38) ** 2) / (2 * (radius * .42) ** 2))
        edge = clamp01((math.hypot(px - cx, py - cy) / radius - .72) / .28)
        return mix(mix(BALL, BALL_LIGHT, .85 * glow), (8, 9, 12), .55 * edge)
    canvas.paint((cx - radius - 1, cy - radius - 1, cx + radius + 1, cy + radius + 1), both(inside, disc(cx, cy, radius)), shade)
    for dx in (.05, .37):
        canvas.paint((cx - radius, cy - radius, cx + radius, cy + radius), both(inside, disc(cx + radius * dx, cy + radius * .06, radius * .105)), GOLD)


def arc(cx, cy, rx, ry, start, end, count):
    return [(cx + rx * math.cos(math.radians(start + (end - start) * i / count)),
             cy + ry * math.sin(math.radians(start + (end - start) * i / count))) for i in range(count + 1)]


# A stroke alphabet for the wordmark: polylines on a unit box (x right, y down), drawn by this script.
LETTERS = {
    'S': [arc(.5, .26, .36, .26, -35, -270, 14) + arc(.5, .74, .36, .26, -90, 145, 14)],
    'Q': [arc(.5, .5, .42, .5, 0, 360, 28), [(.56, .70), (.92, 1.04)]],
    'U': [[(.08, 0)] + arc(.5, .62, .42, .38, 180, 0, 14) + [(.92, 0)]],
    'A': [[(.04, 1), (.5, 0), (.96, 1)], [(.21, .66), (.79, .66)]],
    'H': [[(.08, 0), (.08, 1)], [(.92, 0), (.92, 1)], [(.08, .5), (.92, .5)]],
    'C': [arc(.5, .5, .42, .5, -42, -318, 22)],
    'L': [[(.10, 0), (.10, 1), (.86, 1)]],
}


def wordmark(canvas, text, centre_x, top, height, weight, color):
    width, gap, space = height * .70, height * .26, height * .50
    total = sum(space if ch == ' ' else width for ch in text) + gap * (len(text) - 1)
    x = centre_x - total / 2
    for ch in text:
        if ch != ' ':
            for line in LETTERS[ch]:
                points = [(x + px * width, top + py * height) for px, py in line]
                cover, reach = stroke(points, [weight / 2] * len(points))
                canvas.paint(reach, cover, color)
        x += (space if ch == ' ' else width) + gap


def icon():
    canvas = Canvas(1024, 1024)
    motif(canvas, 0, 0, 1024, 1024)
    return canvas


def launch_screen():
    canvas = Canvas(1290, 2796)
    canvas.vertical_gradient(NAVY_TOP, NAVY_BOTTOM)
    w, h = 690, 438
    x, y = (canvas.w - w) / 2, 980
    cx, cy = x + w / 2, y + h / 2
    canvas.paint((x - 14, y - 14, x + w + 14, y + h + 14), rounded_rect(cx, cy, w / 2 + 12, h / 2 + 12, 58), GOLD)
    motif(canvas, x, y, w, h, rounded_rect(cx, cy, w / 2, h / 2, 46))
    wordmark(canvas, 'SQUASH CLASH', canvas.w / 2, y + h + 150, 86, 17, WHITE)
    return canvas


def main():
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parents[2] / 'Assets/SquashClash/Art/Release'
    out.mkdir(parents=True, exist_ok=True)
    for name, build in (('icon-1024.png', icon), ('launch-screen.png', launch_screen)):
        canvas = build()
        data = canvas.png()
        (out / name).write_bytes(data)
        print(f'{name} {canvas.w}x{canvas.h} RGB8 {len(data)} bytes sha256={hashlib.sha256(data).hexdigest()}')


if __name__ == '__main__':
    main()
