#!/usr/bin/env python3
"""Tasa de lecturas erradas del rival por tipo de golpe y por posición (set22, 27 nov).

  python3 scripts/misread-by-shot.py <corrida-o-swing-read.tsv[.gz]> [...]

Lee swing-read.tsv (columnas level style intent match k humanLine humanHeight humanPower humanVolley kind ...; .gz admitido; un directorio busca swing-read*.tsv[.gz]).
Imprime (a) % errado por (línea, altura, potencia) con n, % Dirección y % Largo; (b) % errado por posición (k=1 devolución del saque, k=2 primer golpe tras el saque propio,
k>2 resto del rally), también por intención; (c) si cada grupo con n>=500 cae en 0,25 ± 3*sqrt(0,25*0,75/n), y una línea final MONEDA OK o MONEDA FUERA (<grupos>).
Solo lectura; sale siempre con 0.
"""
import csv, glob, gzip, io, math, os, sys
from collections import defaultdict

P = 0.25


def find(arg):
    if os.path.isdir(arg):
        for pat in ('swing-read.tsv', 'swing-read.tsv.gz', 'swing-read*.tsv', 'swing-read*.tsv.gz'):
            hits = sorted(glob.glob(os.path.join(arg, pat)))
            if hits:
                return hits[0]
        return None
    return arg


def read(path):
    with (gzip.open if path.endswith('.gz') else open)(path, 'rb') as fh:
        return list(csv.DictReader(io.StringIO(fh.read().decode('utf-8')), delimiter='\t'))


def pct(n, d):
    return f"{100 * n / d:.1f}".replace('.', ',') if d else '—'


def pos(r):
    try:
        k = int(r['k'])
    except (KeyError, ValueError):
        return '?'
    return 'k=1 devolución del saque' if k == 1 else 'k=2 primer golpe tras el saque propio' if k == 2 else 'k>2 resto del rally'


def table(title, groups, bad):
    print(title)
    print(f"  {'grupo':<48}{'n':>8}{'% errado':>10}{'% Dirección':>13}{'% Largo':>9}")
    for key in sorted(groups):
        rows = groups[key]
        n = len(rows)
        mis = [r for r in rows if r['kind'] != 'Right']
        d = sum(1 for r in mis if r['kind'] == 'Direction')
        l = sum(1 for r in mis if r['kind'] == 'Length')
        lim = 3 * math.sqrt(P * (1 - P) / n)
        out = n >= 500 and abs(len(mis) / n - P) > lim
        if out:
            bad.append(f"{title[:3]} {key}")
        mark = ' FUERA' if out else ''
        print(f"  {str(key):<48}{n:>8}{pct(len(mis), n):>10}{pct(d, n):>13}{pct(l, n):>9}{mark}")


def main():
    for arg in sys.argv[1:]:
        path = find(arg)
        if not path or not os.path.exists(path):
            print(f"{arg}: sin swing-read.tsv")
            continue
        rows = read(path)
        print(f"== {arg} ({len(rows)} lecturas) ==")
        bad = []
        g = defaultdict(list)
        for r in rows:
            g[(r['humanLine'], r['humanHeight'], r['humanPower'])].append(r)
        table('(a) Por tipo de golpe (línea, altura, potencia):', g, bad)
        g = defaultdict(list)
        for r in rows:
            g[pos(r)].append(r)
        table('(b) Por posición:', g, bad)
        g = defaultdict(list)
        for r in rows:
            g[(pos(r), r['intent'])].append(r)
        table('(b) Por posición e intención:', g, bad)
        print(f"Banda: grupos con n>=500 dentro de {P:.2f} ± 3*sqrt(p(1-p)/n).")
        print('MONEDA OK' if not bad else 'MONEDA FUERA (' + '; '.join(bad) + ')')
        print()


if __name__ == '__main__':
    try:
        main()
    except Exception as e:
        print('error:', e)
    sys.exit(0)
