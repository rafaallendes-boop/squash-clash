#!/usr/bin/env python3
"""Lectura del swing por el rival (set21, 27 nov): métricas por estilo y por corrida.

  python3 scripts/summarize-swing-read.py <corrida> [<corrida> ...]   (un nombre bajo Logs/experiments/deep-defense/ o un directorio absoluto)

De cada corrida lee cells.tsv, defense.tsv y swing-read.tsv (o sus equivalentes con sufijo, p. ej. metrics-q01.tsv y defense-q01.tsv.gz; .gz admitido).
Lo que no exista se imprime con '—'. Medición declarada en docs/reviews/2026-11-27-lectura-swing.md. Solo lectura.
"""
import csv, glob, gzip, io, math, os, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
STYLES = ("Fondista", "Definidor", "Completo")
FONDO = ("fondo-max", "fondo-085")
NOREACH = ("Unreachable", "NoSwing")


def find(d, base, alt=()):
    for pat in [base, base + '.gz'] + [f"{a}-*.tsv{e}" for a in alt for e in ('', '.gz')] + [f"{base[:-4]}-*.tsv{e}" for e in ('', '.gz')]:
        hits = sorted(h for h in glob.glob(os.path.join(d, pat)) if '-head' not in h and '-source' not in h and ('cells' in base or 'cells' not in os.path.basename(h)))
        if hits:
            return hits[0]
    return None


def read(path):
    if not path:
        return None
    with (gzip.open if path.endswith('.gz') else open)(path, 'rb') as fh:
        return list(csv.DictReader(io.StringIO(fh.read().decode('utf-8')), delimiter='\t'))


def num(x):
    try:
        v = float(x)
    except (TypeError, ValueError):
        return math.nan
    return v


def med(rows, key):
    v = sorted(x for x in (num(r.get(key)) for r in rows) if not math.isnan(x))
    if not v:
        return math.nan
    n = len(v)
    return v[n // 2] if n % 2 else (v[n // 2 - 1] + v[n // 2]) / 2


def fmt(x, d=3):
    return '—' if x is None or (isinstance(x, float) and math.isnan(x)) else f"{x:.{d}f}".replace('.', ',')


def pct(n, d):
    return f"{100 * n / d:.1f}".replace('.', ',') if d else '—'


def share(rows, pred):
    return pct(sum(1 for r in rows if pred(r)), len(rows))


def table(head, body):
    print("| " + " | ".join(head) + " |")
    print("|" + "---|" * len(head))
    for b in body:
        print("| " + " | ".join(str(c) for c in b) + " |")
    print()


def cell_share(cells, style, intent):
    for r in cells or ():
        if r['style'] == style and r['intent'] == intent:
            return float(r['share'])
    return None


def main(runs):
    for run in runs:
        d = run if os.path.isabs(run) else str(ROOT / 'Logs/experiments/deep-defense' / run)
        cells, dfn, sr = read(find(d, 'cells.tsv', ('metrics',))), read(find(d, 'defense.tsv')), read(find(d, 'swing-read.tsv'))
        print(f"# Lectura del swing · {os.path.basename(run.rstrip('/'))}\n")
        print(f"cells: {len(cells) if cells else '—'} filas; defense: {len(dfn) if dfn is not None else '—'}; swing-read: {len(sr) if sr is not None else '—'}\n")

        print("## 1. Celdas (cells.tsv)\n")
        body = []
        for s in STYLES:
            row = [s]
            for i in ("fondo-max", "fondo-085", "corto-drop", "medio-suave"):
                v = cell_share(cells, s, i)
                row.append('—' if v is None else pct(v * 1000, 1000))
            for i in FONDO:
                c = [r for r in cells or () if r['style'] == s and r['intent'] == i]
                row.append(pct(sum(int(r['rivalNoReach']) for r in c), sum(int(r['rivalReceptions']) for r in c)))
            body.append(row)
        table(["Estilo", "% humano fondo-max", "% humano fondo-085", "% humano corto-drop", "% humano medio-suave", "% rival no llega fondo-max", "% rival no llega fondo-085"], body)

        print("## 2. Defensa (defense.tsv; d = todas, c = contactos)\n")
        head = ["n", "holgura rival med d (s)", "% stretched d", "holgura humano med c (s)", "% perdidos en el acto d"]
        for title, intents in (("fondo-max + fondo-085", FONDO),) + tuple((i, (i,)) for i in FONDO):
            print(f"{title}:\n")
            body = []
            for s in STYLES:
                dd = [r for r in dfn or () if r['style'] == s and r['intent'] in intents]
                c = [r for r in dd if r['result'] == 'Contact']
                body.append([s, len(dd) if dfn is not None else '—', fmt(med(dd, 'rivalSlackS')), share(dd, lambda r: r['planReason'] == 'stretched'),
                             fmt(med(c, 'humanSlackS')), share(dd, lambda r: r['lostNow'] == '1')])
            table(["Estilo"] + head, body)

        print("## 3. Lecturas (swing-read.tsv)\n")
        if sr is None:
            print("Sin swing-read.tsv: —\n")
            continue
        body = []
        for s in STYLES:
            r = [x for x in sr if x['style'] == s]
            wrong = [x for x in r if x['kind'] != 'Right']
            body.append([s, len(r), pct(len(wrong), len(r)), share(r, lambda x: x['kind'] == 'Direction'), share(r, lambda x: x['kind'] == 'Length'), fmt(med(r, 'leadMs'), 1)])
        table(["Estilo", "n lecturas", "% erradas", "% Dirección", "% Largo", "anticipación med (ms)"], body)
        groups = (("fondo", lambda x: x['intent'] in FONDO), ("corto-drop", lambda x: x['intent'] == 'corto-drop'), ("medio-suave", lambda x: x['intent'] == 'medio-suave'))
        kinds = (("Right", lambda x: x['kind'] == 'Right'), ("Wrong", lambda x: x['kind'] != 'Right'),
                 ("Direction", lambda x: x['kind'] == 'Direction'), ("Length", lambda x: x['kind'] == 'Length'))
        for gname, gp in groups:
            print(f"Grupo {gname}:\n")
            body = []
            for s in STYLES:
                for kname, kp in kinds:
                    r = [x for x in sr if x['style'] == s and gp(x) and kp(x)]
                    body.append([s, kname, len(r), share(r, lambda x: x['rivalResult'] in NOREACH), share(r, lambda x: x['lostNow'] == '1'), fmt(med(r, 'rivalSlackS'))])
            table(["Estilo", "Lectura", "n", "% rival no llega", "% perdidos en el acto", "holgura rival med (s)"], body)


main(sys.argv[1:] or sys.exit(__doc__))
