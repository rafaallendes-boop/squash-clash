#!/usr/bin/env python3
"""Separa por nivel los archivos de una corrida de deep-defense (set25, 28 nov). Solo lectura de la corrida.

  python3 scripts/slice-level.py <corrida> <nivel> <salida>

<corrida>: directorio con cells.tsv, rival-time.tsv, prev-ball.tsv, defense.tsv, defense-cells.tsv y swing-read.tsv (.gz admitido).
<nivel>: Easy, Medium o Hard. Escribe en <salida> (que no debe existir) cada archivo con su cabecera y solo las filas cuya primera
columna (level) es ese nivel, byte a byte (mismas líneas, mismo fin de línea). Imprime cuántas filas salen de cada archivo.
"""
import gzip, os, sys

FILES = ('cells.tsv', 'rival-time.tsv', 'prev-ball.tsv', 'defense.tsv', 'defense-cells.tsv', 'swing-read.tsv')


def raw(path):
    with (gzip.open if path.endswith('.gz') else open)(path, 'rb') as fh:
        return fh.read()


def main(argv):
    if len(argv) != 3:
        sys.exit(__doc__)
    src, level, out = argv
    if level not in ('Easy', 'Medium', 'Hard'):
        sys.exit('nivel: Easy, Medium o Hard')
    if os.path.exists(out):
        sys.exit(f'ya existe: {out}')
    os.makedirs(out)
    key = level.encode() + b'\t'
    for name in FILES:
        path = os.path.join(src, name)
        if not os.path.exists(path) and os.path.exists(path + '.gz'):
            path += '.gz'
        if not os.path.exists(path):
            print(f'{name}: no existe')
            continue
        lines = raw(path).splitlines(keepends=True)
        if not lines:
            sys.exit(f'{name}: vacío, sin cabecera')
        if not lines[0].startswith(b'level\t'):
            sys.exit(f'{name}: la primera columna no es level')
        keep = [l for l in lines[1:] if l.startswith(key)]
        with open(os.path.join(out, name), 'wb') as fh:
            fh.write(lines[0] + b''.join(keep))
        print(f'{name}: {len(keep)} filas de {level} (de {len(lines) - 1})')


main(sys.argv[1:])
