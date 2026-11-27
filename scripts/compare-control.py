#!/usr/bin/env python3
"""Control de la corrida de set20 contra la de q01 (27 nov): lo que no depende de la variante tiene que dar igual.

  python3 scripts/compare-control.py --cells A B --rival-time A B --prev-ball A B --defense A B   (rutas explícitas; .gz admitido)

cells.tsv: igual en todas las columnas salvo `seconds`; rival-time.tsv y prev-ball.tsv: idénticos byte a byte;
defense.tsv: iguales las 30 primeras columnas (A puede tener 30 y B 34). Opcional --empty FILE (swing-read.tsv): solo la cabecera. Imprime las diferencias y sale con 1 si hay alguna. Solo lectura.
"""
import argparse, gzip, sys


def raw(path):
    with (gzip.open if path.endswith('.gz') else open)(path, 'rb') as fh:
        return fh.read()


def rows(path):
    return [l.split('\t') for l in raw(path).decode('utf-8').splitlines()]


def drop(table, skip):
    return [[c for i, c in enumerate(r) if i != skip] for r in table]


def diff_rows(name, a, b):
    n = abs(len(a) - len(b))
    if n:
        print(f"{name}: filas {len(a)} frente a {len(b)}")
    for i, (x, y) in enumerate(zip(a, b)):
        if x != y:
            n += 1
            if n <= 5:
                print(f"{name}: fila {i}: {x} != {y}")
    print(f"{name}: DIFF {n}")
    return n


def main():
    ap = argparse.ArgumentParser()
    for k in ('cells', 'rival-time', 'prev-ball', 'defense'):
        ap.add_argument('--' + k, nargs=2, metavar=('A', 'B'), required=True)
    ap.add_argument('--empty', metavar='FILE', help='el archivo debe tener solo su línea de cabecera')
    a = ap.parse_args()
    bad = 0
    ca, cb = rows(a.cells[0]), rows(a.cells[1])
    sec = ca[0].index('seconds') if 'seconds' in ca[0] else -1
    bad += diff_rows('cells.tsv (sin seconds)', drop(ca, sec), drop(cb, sec))
    for key, name in ((a.rival_time, 'rival-time.tsv'), (a.prev_ball, 'prev-ball.tsv')):
        same = raw(key[0]) == raw(key[1])
        print(f"{name}: {'idéntico' if same else 'DIFF byte a byte'}")
        bad += 0 if same else 1
    da, db = rows(a.defense[0]), rows(a.defense[1])
    bad += diff_rows('defense.tsv (30 primeras columnas)', [r[:30] for r in da], [r[:30] for r in db])
    print(f"columnas de defense.tsv: A {len(da[0])}, B {len(db[0])}")
    if a.empty:
        n = len(rows(a.empty)) - 1
        print(f"swing-read.tsv: {'vacío' if n == 0 else f'{n} filas'}")
        bad += 0 if n == 0 else 1
    print("CONTROL " + ("OK" if bad == 0 else f"FALLA ({bad})"))
    sys.exit(1 if bad else 0)


main()
