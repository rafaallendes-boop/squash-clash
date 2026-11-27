#!/usr/bin/env python3
"""Δ del novato en N2 (fase confirm) frente a una o más referencias, en pp.

  python3 scripts/n2-delta.py <n2.tsv> <ref.tsv|etiqueta=ref.tsv> [...]

Filas con phase == confirm y reference == newcomer; Δ = 100 · (share - share de la referencia) por (nivel, estilo). Banda vigente: > 5 pp = parada. Solo lectura.
"""
import csv, sys
from pathlib import Path


def load(path):
    return {(r['level'], r['style']): float(r['share']) for r in csv.DictReader(open(path, encoding='utf-8'), delimiter='\t')
            if r['phase'] == 'confirm' and r['reference'] == 'newcomer'}


def main(argv):
    if len(argv) < 2:
        sys.exit(__doc__)
    cur = load(argv[0])
    for ref in argv[1:]:
        name, _, path = ref.partition('=') if '=' in ref else (Path(ref).stem, '', ref)
        base = load(path)
        delta = {k: 100 * (v - base[k]) for k, v in cur.items() if k in base}
        if not delta:
            print(f"{name}: sin celdas comunes")
            continue
        worst = max(delta, key=lambda k: abs(delta[k]))
        print(f"{name} max|Δ|={abs(delta[worst]):.2f} {worst}")
        print("  " + " ".join(f"{l[0]}/{s[:3]} {delta[(l, s)]:+.2f}" for l, s in sorted(delta)))


main(sys.argv[1:])
