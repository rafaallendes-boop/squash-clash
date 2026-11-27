#!/usr/bin/env python3
"""Defensa alta y larga (26 nov): tablas ANTES/DESPUÉS y N2.

  python3 scripts/summarize-deep-defense.py <before-run> <after-run>

Lee Logs/experiments/deep-defense/<run>/cells.tsv y
Logs/experiments/rival-styles-calibration/<run>-n2/cells.tsv.
N2 (spec 2026-11-26-defensa-alta-larga): los nueve porcentajes de puntos del novato
(fase confirm) de la rama frente a la base; una celda con |Δ| > 5 pp rompe N2.
"""
import csv
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
BAND_PP = 5.0


def rows(path):
    with open(path, newline='', encoding='utf-8') as f:
        return list(csv.DictReader(f, delimiter='\t'))


def pct(x):
    return f"{100 * float(x):.1f}"


def num(x, fmt="{:.2f}"):
    try:
        v = float(x)
    except ValueError:
        return "—"
    return "—" if v != v else fmt.format(v)


def metrics(before, after):
    b = {(r['level'], r['style'], r['intent']): r for r in rows(ROOT / f"Logs/experiments/deep-defense/{before}/cells.tsv")}
    a = {(r['level'], r['style'], r['intent']): r for r in rows(ROOT / f"Logs/experiments/deep-defense/{after}/cells.tsv")}
    print("| Nivel | Estilo | Golpe del humano | Humano gana % antes → después | Bote mediano de la defensa (m) | Holgura mediana del humano (s) | Defensas al vidrio al vuelo | Voleas del rival (defensivas) | n puntos |")
    print("|---|---|---|---|---|---|---|---|---|")
    for key in b:
        if key not in a:
            continue
        x, y = b[key], a[key]
        def glass(r):
            n = int(r['defBounceN'])
            return f"{100 * int(r['defGlassFly']) / n:.0f} %" if n else "—"
        print(f"| {key[0]} | {key[1]} | {key[2]} | {pct(x['share'])} → **{pct(y['share'])}** | "
              f"{num(x['defBounceMedianZ'])} → **{num(y['defBounceMedianZ'])}** | "
              f"{num(x['humanSlackMedianS'])} → **{num(y['humanSlackMedianS'])}** | {glass(x)} → {glass(y)} | "
              f"{x['rivalVolleys']} ({x['rivalDefensiveVolleys']}) → {y['rivalVolleys']} ({y['rivalDefensiveVolleys']}) | "
              f"{x['points']} / {y['points']} |")
    bad = [k for k in a if int(a[k]['mismatch']) or (k in b and int(b[k]['mismatch']))]
    print(f"\nCeldas con puntos sin reconstruir (mismatch > 0): {len(bad)}")


def n2(before, after):
    def confirm(run):
        out = {}
        for r in rows(ROOT / f"Logs/experiments/rival-styles-calibration/{run}-n2/cells.tsv"):
            if r['phase'] == 'confirm':
                out[(r['level'], r['style'], r['reference'])] = r
        return out
    b, a = confirm(before), confirm(after)
    worst = 0.0
    print("| Nivel | Estilo | Novato base % | Novato rama % | Δ pp | bot30 base % | bot30 rama % | Δ pp |")
    print("|---|---|---|---|---|---|---|---|")
    for level in ("Easy", "Medium", "Hard"):
        for style in ("Fondista", "Definidor", "Completo"):
            nb, na = b[(level, style, 'newcomer')], a[(level, style, 'newcomer')]
            bb, ba = b[(level, style, 'bot30')], a[(level, style, 'bot30')]
            d = 100 * (float(na['share']) - float(nb['share']))
            d30 = 100 * (float(ba['share']) - float(bb['share']))
            worst = max(worst, abs(d))
            flag = " **fuera**" if abs(d) > BAND_PP else ""
            print(f"| {level} | {style} | {pct(nb['share'])} (n={nb['points']}) | {pct(na['share'])} (n={na['points']}) | {d:+.2f}{flag} | "
                  f"{pct(bb['share'])} | {pct(ba['share'])} | {d30:+.2f} |")
    verdict = "PASA" if worst <= BAND_PP else "FALLA"
    print(f"\nN2: máx |Δ| novato = {worst:.2f} pp (banda > {BAND_PP:g} pp rompe) → {verdict}")
    fh = lambda run: (ROOT / f"Logs/experiments/rival-styles-calibration/{run}-n2/fondista-hard.tsv").read_text().split()[-1]
    print(f"k del Fondista en Difícil: base {fh(before)}, rama {fh(after)}")
    return verdict


if __name__ == '__main__':
    before, after = sys.argv[1], sys.argv[2]
    metrics(before, after)
    print()
    n2(before, after)
