#!/usr/bin/env python3
"""Alcance del rival (set14, 26 nov): ANTES/DESPUÉS de una variante y N2.

  python3 scripts/summarize-rival-reach.py <before-metrics-run> <before-n2-cells.tsv> <variant-run>

Métricas: Logs/experiments/deep-defense/<run>/cells.tsv (Medio, bot σ 30 ms, 100 partidos por celda).
N2: Logs/experiments/rival-styles-calibration/<variant>-n2/cells.tsv frente a la base (fase confirm,
novato); una celda con |Δ| > 5 pp rompe N2 (banda vigente, sin cambios).
"""
import csv
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
BAND_PP = 5.0
STYLES = ("Fondista", "Definidor", "Completo")


def rows(path):
    with open(path, newline='', encoding='utf-8') as f:
        return list(csv.DictReader(f, delimiter='\t'))


def metrics(run):
    return {(r['style'], r['intent']): r for r in rows(ROOT / f"Logs/experiments/deep-defense/{run}/cells.tsv")}


def share(r):
    return 100 * float(r['share'])


def no_reach(r):
    return 100 * int(r['rivalNoReach']) / int(r['rivalReceptions'])


def wins_no_reach(r):
    return 100 * int(r['humanWinsNoReach']) / max(1, int(r['human']))


def n2(path):
    return {(r['level'], r['style']): r for r in rows(path) if r['phase'] == 'confirm' and r['reference'] == 'newcomer'}


def main(before, before_n2, variant):
    b, a = metrics(before), metrics(variant)
    print(f"## {variant}\n")
    print("| Estilo | Fondo P=1 % | Fondo P=0,85 % | Rival no llega (P=1), % recepciones | Puntos del humano por no llegar (P=1), % | Holgura humano tras defensa (P=1), s | Drop del humano % | Estocadas del rival (P=1) |")
    print("|---|---|---|---|---|---|---|---|")
    for s in STYLES:
        x, y = b[(s, 'fondo-max')], a[(s, 'fondo-max')]
        x85, y85 = b[(s, 'fondo-085')], a[(s, 'fondo-085')]
        xd, yd = b[(s, 'corto-drop')], a[(s, 'corto-drop')]
        print(f"| {s} | {share(x):.1f} → **{share(y):.1f}** | {share(x85):.1f} → **{share(y85):.1f}** | "
              f"{no_reach(x):.1f} → **{no_reach(y):.1f}** | {wins_no_reach(x):.1f} → **{wins_no_reach(y):.1f}** | "
              f"{float(x['humanSlackMedianS']):.2f} → **{float(y['humanSlackMedianS']):.2f}** | "
              f"{share(xd):.1f} → **{share(yd):.1f}** | {y['rivalLunges']} |")
    print("\n| Estilo | Medio suave % | No llega (P=0,85) % | n puntos P=1 antes / después |")
    print("|---|---|---|---|")
    for s in STYLES:
        x, y = b[(s, 'medio-suave')], a[(s, 'medio-suave')]
        x85, y85 = b[(s, 'fondo-085')], a[(s, 'fondo-085')]
        print(f"| {s} | {share(x):.1f} → {share(y):.1f} | {no_reach(x85):.1f} → {no_reach(y85):.1f} | "
              f"{b[(s, 'fondo-max')]['points']} / {a[(s, 'fondo-max')]['points']} |")
    bad = [k for k in a if int(a[k]['mismatch'])]
    print(f"\nCeldas con mismatch > 0: {len(bad)}")
    n2_path = ROOT / f"Logs/experiments/rival-styles-calibration/{variant}-n2/cells.tsv"
    if not n2_path.exists():
        print("\nN2: no medido")
        return
    nb, na = n2(before_n2), n2(n2_path)
    worst = 0.0
    print("\n| Novato, % de puntos | " + " | ".join(STYLES) + " |")
    print("|---|---|---|---|")
    for level, name in (("Easy", "Fácil"), ("Medium", "Medio"), ("Hard", "Difícil")):
        cells = []
        for s in STYLES:
            x, y = 100 * float(nb[(level, s)]['share']), 100 * float(na[(level, s)]['share'])
            d = y - x
            worst = max(worst, abs(d))
            mark = f"**{d:+.2f}**" if abs(d) > BAND_PP else f"{d:+.2f}"
            cells.append(f"{x:.1f} → {y:.1f} ({mark})")
        print(f"| {name} | " + " | ".join(cells) + " |")
    k = f"base {nb[('Hard', 'Fondista')]['k']}, variante {na[('Hard', 'Fondista')]['k']}"
    print(f"\nN2: máx |Δ| novato = {worst:.2f} pp → {'dentro de la banda' if worst <= BAND_PP else 'FUERA de la banda (> 5 pp)'}; k del Fondista en Difícil {k}")


if __name__ == '__main__':
    main(*sys.argv[1:4])
