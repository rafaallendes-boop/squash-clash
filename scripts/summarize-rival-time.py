#!/usr/bin/env python3
"""Tiempo del rival (set15, 27 nov): descomposición del déficit en los drives al fondo que el rival no alcanza.

  python3 scripts/summarize-rival-time.py <run>   (Logs/experiments/deep-defense/<run>/rival-time.tsv)

Solo filas con drive = 1 (la recepción sigue a un golpe del humano Recto, Medio y Fuerte). Tiempos en s de simulación.
"""
import csv, math, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
STYLES = ("Fondista", "Definidor", "Completo")


def val(x):
    return math.inf if x == 'inf' else float(x)


def q(v, p):
    v = sorted(v)
    if not v:
        return math.nan
    i = (len(v) - 1) * p
    lo, hi = math.floor(i), math.ceil(i)
    if v[lo] == math.inf or v[hi] == math.inf:
        return v[hi] if p >= .5 else v[lo]
    return v[lo] + (v[hi] - v[lo]) * (i - lo)


def fmt(x):
    return '∞' if x == math.inf else f"{x:.3f}".replace('.', ',')


def pct(n, d):
    return f"{100 * n / d:.1f}".replace('.', ',') if d else '—'


def main(run):
    path = ROOT / f"Logs/experiments/deep-defense/{run}/rival-time.tsv"
    rows = list(csv.DictReader(open(path, encoding='utf-8'), delimiter='\t'))
    for intent in ('fondo-max', 'fondo-085'):
        sel = [r for r in rows if r['intent'] == intent]
        print(f"## {run} · {intent}\n")
        print(f"Filas que no llegan: {len(sel)}; tras drive Recto-Medio-Fuerte: {sum(r['drive'] == '1' for r in sel)}; saque: {sum(r['serve'] == '1' for r in sel)}; otras: {sum(r['drive'] != '1' and r['serve'] != '1' for r in sel)}\n")
        print("| Estilo | n | v0 mediana (m/s) | distancia mediana (m) | tiempo hasta T* mediana (s) | déficit base mediana / p90 (s) | ahorro reacción | ahorro percepción | ahorro inercia (parado) | ahorro arranque (acel. instantánea) | déficit que queda (sin latencia, parado, acel. instantánea) mediana / p90 | sin acuerdo plan sin ruido |")
        print("|---|---|---|---|---|---|---|---|---|---|---|---|")
        reach = []
        for s in STYLES:
            d = [r for r in sel if r['style'] == s and r['drive'] == '1']
            if not d:
                continue
            g = lambda k: [val(r[k]) for r in d]
            base, nr, npc, nd, rest, inst, core = (g(k) for k in ('lateBaseS', 'lateNoReactS', 'lateNoPerceptionS', 'lateNoDelayS', 'lateRestS', 'lateRestInstantS', 'lateCoreS'))
            fin = [i for i in range(len(d)) if base[i] < math.inf]
            sv = lambda a, b: q([a[i] - b[i] for i in fin if b[i] < math.inf], .5)
            disagree = sum((r['noiselessReason'] == 'planned') != (val(r['lateBaseS']) <= 0) for r in d)
            print(f"| {s} | {len(d)} | {fmt(q(g('v0'), .5))} | {fmt(q([x for x in g('distance') if not math.isnan(x)], .5))} | {fmt(q([x for x in g('untilS') if not math.isnan(x)], .5))} | "
                  f"{fmt(q(base, .5))} / {fmt(q(base, .9))} | {fmt(sv(base, nr))} | {fmt(sv(base, npc))} | {fmt(sv(base, rest))} | {fmt(sv(rest, inst))} | {fmt(q(core, .5))} / {fmt(q(core, .9))} | {disagree} |")
            n = len(d)
            cnt = lambda a: sum(1 for x in a if x <= 0)
            reach.append((s, n, cnt(base), cnt(nr), cnt(npc), cnt(nd), cnt(rest), cnt(inst), cnt(core),
                          sum(r['noiselessReason'] == 'planned' for r in d), q(g('stanceError'), .5), sum(val(r['lateBaseS']) == math.inf for r in d)))
        print("\n% de esas recepciones que llegarían (déficit ≤ 0) al quitar cada tramo:\n")
        print("| Estilo | base | sin reacción | sin percepción | sin latencia | parado | parado + acel. instantánea | además sin latencia | plan sin ruido llega | error de postura mediano (m) | censuradas (> 2 s) |")
        print("|---|---|---|---|---|---|---|---|---|---|---|")
        for s, n, *c, err, cens in reach:
            *cs, nl = c
            print(f"| {s} | " + " | ".join(pct(x, n) for x in cs) + f" | {pct(nl, n)} | {fmt(err)} | {cens} |")
        print()


if __name__ == '__main__':
    main(sys.argv[1])
