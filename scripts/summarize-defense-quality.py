#!/usr/bin/env python3
"""Calidad de la defensa del rival (set19, 27 nov): cómo defiende el rival un drive al fondo del humano y por qué no llega a los que no alcanza.

  python3 scripts/summarize-defense-quality.py <run>   (Logs/experiments/deep-defense/<run>/defense.tsv, prev-ball.tsv, defense-cells.tsv)

Medición declarada en docs/reviews/2026-11-27-calidad-defensa.md. Tiempos en s de simulación; márgenes en ms reales. Solo lectura.
"""
import csv, math, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
STYLES = ("Fondista", "Definidor", "Completo")
INTENTS = (("fondo-max", "P = 1"), ("fondo-085", "P = 0,85"))
SLACK = {"Fondista": .12, "Definidor": .06, "Completo": .12}
SHOTS = ("globo", "drive-recto", "cruzado", "boast", "drop", "otro")
LENGTHS = ("corto", "medio", "largo", "fondo-vuelo")
TIGHTS = ("pegado", "intermedio", "suelto")
DIAGS = ("serve", "short", "other-side", "not-arrived", "arrived-short", "other")


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


def fmt(x, d=3):
    if x == math.inf:
        return '∞'
    if math.isnan(x):
        return '—'
    return f"{x:.{d}f}".replace('.', ',')


def pct(n, d):
    return f"{100 * n / d:.1f}".replace('.', ',') if d else '—'


def med(rows, key):
    return q([val(r[key]) for r in rows if r[key] != 'nan' and not math.isnan(val(r[key]))], .5)


def share(rows, pred):
    return pct(sum(1 for r in rows if pred(r)), len(rows))


def read(run, name):
    return list(csv.DictReader(open(ROOT / f"Logs/experiments/deep-defense/{run}/{name}", encoding='utf-8'), delimiter='\t'))


def table(head, body):
    print("| " + " | ".join(head) + " |")
    print("|" + "---|" * len(head))
    for b in body:
        print("| " + " | ".join(str(c) for c in b) + " |")
    print()


def tight(r):
    return r['planReason'] == 'stretched' or float(r['rivalSlackS']) < SLACK[r['style']]


def healthy(r):
    return (r['shotClass'] == 'globo' and r['lengthClass'] in ('largo', 'medio')) or (r['shotClass'] == 'drive-recto' and r['lengthClass'] == 'largo' and r['tightClass'] == 'pegado')


def lost(r):
    return r['pointWinner'] == 'Human'


def main(run):
    dfn = read(run, 'defense.tsv')
    prev = read(run, 'prev-ball.tsv')
    dcells = read(run, 'defense-cells.tsv')
    print(f"# Calidad de la defensa · {run}\n")
    print(f"Defensas alcanzadas (defense.tsv): {len(dfn)}; no llegan tras drive (prev-ball.tsv): {len(prev)}; newMismatch total: {sum(int(c['newMismatch']) for c in dcells)}\n")

    print("## 1. Por estilo e intención\n")
    t1, t2, t3 = [], [], []
    for s in STYLES:
        for intent, label in INTENTS:
            d = [r for r in dfn if r['style'] == s and r['intent'] == intent]
            if not d:
                continue
            c = [r for r in d if r['result'] == 'Contact']
            t1.append([s, label, len(d), share(d, lambda r: r['result'] != 'Contact'), len(c)] + [share(c, lambda r, k=k: r['shotClass'] == k) for k in SHOTS]
                      + [share(c, lambda r: r['volley'] == '1'), share(c, lambda r: r['defensive'] == '1')])
            t2.append([s, label] + [share(c, lambda r, k=k: r['lengthClass'] == k) for k in LENGTHS] + [share(c, lambda r, k=k: r['tightClass'] == k) for k in TIGHTS]
                      + [fmt(med(c, 'sideDist')), fmt(med(c, 'humanSlackS')), share(d, lost), share(d, lambda r: r['lostNow'] == '1')])
            t3.append([s, label, fmt(med(d, 'rivalSlackS')), share(d, tight), share(d, lambda r: r['planReason'] == 'stretched'),
                       fmt(med(d, 'reachMargin')), fmt(med(d, 'windowMs'), 1), fmt(med(d, 'releaseMarginMs'), 1)])
    print("Golpe elegido (% de los contactos):\n")
    table(["Estilo", "Intención", "n", "% falla", "n contacto"] + [f"% {k}" for k in SHOTS] + ["% volea", "% defensivo"], t1)
    print("Bote de su pelota (% de los contactos) y punto:\n")
    table(["Estilo", "Intención"] + [f"% {k}" for k in LENGTHS] + [f"% {k}" for k in TIGHTS] + ["sideDist med (m)", "holgura humano med (s)", "% perdidos (todas)", "% perdidos en el acto"], t2)
    print("Margen del rival:\n")
    table(["Estilo", "Intención", "holgura rival med (s)", "% justo", "% stretched", "margen de alcance med (m)", "ventana med (ms)", "margen de suelta med (ms)"], t3)

    print("## 2. Por estilo y golpe (solo contactos, ambas intenciones)\n")
    body = []
    for s in STYLES:
        c = [r for r in dfn if r['style'] == s and r['result'] == 'Contact']
        for k in SHOTS:
            d = [r for r in c if r['shotClass'] == k]
            if d:
                body.append([s, k, len(d), pct(len(d), len(c)), fmt(med(d, 'humanSlackS')), share(d, lost), share(d, lambda r: r['lostNow'] == '1'),
                             share(d, lambda r: r['lengthClass'] == 'corto'), share(d, lambda r: r['lengthClass'] == 'largo'), share(d, lambda r: r['tightClass'] == 'pegado')])
    table(["Estilo", "Golpe", "n", "% de contactos", "holgura humano med (s)", "% perdidos", "% en el acto", "% corto", "% largo", "% pegado"], body)

    print("## 3. Sano frente a resto (solo contactos, ambas intenciones)\n")
    print("Sano: globo que bota medio o largo, o drive recto largo y pegado.\n")
    body = []
    for s in STYLES:
        c = [r for r in dfn if r['style'] == s and r['result'] == 'Contact']
        for name, d in (("sano", [r for r in c if healthy(r)]), ("resto", [r for r in c if not healthy(r)])):
            body.append([s, name, len(d), fmt(med(d, 'humanSlackS')), share(d, lost), share(d, lambda r: r['lostNow'] == '1')])
    table(["Estilo", "Grupo", "n", "holgura humano med (s)", "% perdidos", "% en el acto"], body)

    print("## 4. Justo frente a holgado (ambas intenciones)\n")
    print("Justo: holgura del rival menor que la de defensa de su estilo (0,12 / 0,06 / 0,12 s) o plan stretched. % falla sobre todas las defensas; el resto sobre los contactos.\n")
    body = []
    for s in STYLES:
        a = [r for r in dfn if r['style'] == s]
        for name, d in (("justo", [r for r in a if tight(r)]), ("holgado", [r for r in a if not tight(r)])):
            c = [r for r in d if r['result'] == 'Contact']
            body.append([s, name, len(d), share(d, lambda r: r['result'] != 'Contact')] + [share(c, lambda r, k=k: r['shotClass'] == k) for k in ('globo', 'drive-recto', 'boast')]
                        + [share(c, lambda r: r['lengthClass'] == 'corto'), share(c, lambda r: r['tightClass'] == 'suelto'), fmt(med(c, 'releaseMarginMs'), 1),
                           fmt(med(c, 'humanSlackS')), share(c, lost)])
    table(["Estilo", "Grupo", "n", "% falla", "% globo", "% drive recto", "% boast", "% corto", "% suelto", "margen de suelta med (ms)", "holgura humano med (s)", "% perdidos"], body)

    print("## 5. Pelota anterior del rival en los drives que no alcanza\n")
    body, late = [], []
    for s in STYLES:
        for intent, label in INTENTS:
            d = [r for r in prev if r['style'] == s and r['intent'] == intent]
            if not d:
                continue
            body.append([s, label, len(d)] + [f"{sum(r['diagClass'] == k for r in d)} ({pct(sum(r['diagClass'] == k for r in d), len(d))} %)" for k in DIAGS]
                        + [fmt(q([val(r['lateBaseS']) for r in d], .5)), fmt(med(d, 'lateAtTargetS')), pct(sum(1 for r in d if r['lateAtTargetS'] != 'nan' and val(r['lateAtTargetS']) > 0), sum(1 for r in d if r['lateAtTargetS'] != 'nan'))])
            for k in DIAGS:
                g = [r for r in d if r['diagClass'] == k]
                if g:
                    late.append([s, label, k, len(g), fmt(q([val(r['lateBaseS']) for r in g], .5)), fmt(med(g, 'lateAtTargetS')), fmt(med(g, 'lateDriveSideS'))])
    table(["Estilo", "Intención", "n"] + list(DIAGS) + ["déficit base med (s)", "déficit quieto en objetivo med (s)", "% aún tarde quieto en objetivo"], body)
    print("Déficit mediano por clase (s; ∞ = no llega en 2 s):\n")
    table(["Estilo", "Intención", "Clase", "n", "base", "quieto en objetivo", "quieto del lado del drive"], late)


if __name__ == '__main__':
    main(sys.argv[1])
