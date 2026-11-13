#!/usr/bin/env python3
"""Reproduce descriptive CQ-ZONE-v1 tables from the immutable Unity TSVs; no Unity invocation."""
import csv
from pathlib import Path

HERE = Path(__file__).resolve().parent

def read(name):
    with (HERE / name).open() as stream:
        return list(csv.DictReader(stream, delimiter='\t'))

def main():
    rows = read('zones.tsv')
    practice = [r for r in rows if r['origin'] == 'practice' and r['compMs'] == '0' and r['form'] == 'trajectory']
    lines = ['# Tablas derivadas CQ-ZONE-v1', '', 'Sin criterio de visibilidad ni selección de alternativa. Los datos fuente conservan formato round-trip; aquí se redondean solo para lectura.', '',
             '## SL1–SL6: zona por trayectoria, compensación 0', '',
             'K≠0 = nodos legales con vector no nulo / nodos legales. V≠0 = ms con contacto y vector no nulo / ms con contacto en ±100. Los fallos se excluyen del denominador de vector.', '',
             '| Receta | Tempo | J_A nodos | L (m) | Ancho cero (pasos / ms / arco m) | K≠0 | V≠0 (ms) | V≠0/contacto | Max altura (cm) | Max pérdida v (%) |',
             '|---|---:|---:|---:|---|---:|---|---:|---:|---:|']
    for r in sorted(practice, key=lambda r: (r['id'], float(r['tempo']))):
        lines.append(f"| {r['id']} | {r['tempo']} | {r['perfectDistinctSteps']} | {float(r['LarcM']):.6f} | {r['zeroSpanSteps']} / {float(r['zeroSpanRealMs']):.3f} / {float(r['zeroSpanArcM']):.6f} | {r['nonzeroNodes']}/{r['kNodes']} | {float(r['nonzeroContactMs']):.3f}/{float(r['contactMs']):.3f} | {100*float(r['nonzeroOverContactFraction']):.2f}% | {100*float(r['maxHeightM']):.3f} | {100*float(r['maxSpeedFractionLoss']):.3f} |")
    lines += ['', '## Rival: trayectoria y radio adaptativo, tempo 1 / compensación 0', '',
              'L/r provienen de J_A efectivo. Ambos tienen iguales conteos de nodos dentro/fuera en esta muestra; sus distancias fuera difieren. La anchura entre extremos no es un número inclusivo de nodos.', '',
              '| Muestra | L / r (m) | Ancho cero (pasos / ms / arco m) | K≠0 | V≠0 (ms / contacto) | V≠0/contacto | Max altura trayectoria (cm) |',
              '|---|---|---|---:|---|---:|---:|']
    for r in rows:
        if r['origin'] != 'rival' or r['tempo'] != '1' or r['compMs'] != '0' or r['form'] != 'trajectory':
            continue
        lines.append(f"| {r['id']} | {float(r['LarcM']):.6f} / {float(r['adaptiveRadiusM']):.6f} | {r['zeroSpanSteps']} / {float(r['zeroSpanRealMs']):.3f} / {float(r['zeroSpanArcM']):.6f} | {r['nonzeroNodes']}/{r['kNodes']} | {float(r['nonzeroContactMs']):.3f}/{float(r['contactMs']):.3f} | {100*float(r['nonzeroOverContactFraction']):.2f}% | {100*float(r['maxHeightM']):.3f} |")
    lines += ['', '## Desaparición total de timing en Valid con radio fijo', '',
              '| Tempo | Muestras con contactos Valid pero vector siempre cero |', '|---|---|']
    for tempo in ['0.3', '0.65', '1']:
        bad = [r['id'] for r in rows if r['tempo'] == tempo and r['compMs'] == '0' and r['form'] == 'fixed-radius'
               and float(r['validNonPerfectMs']) > 0 and float(r['nonzeroContactMs']) == 0]
        lines.append(f"| {tempo} | {len(bad)}/30: {', '.join(bad)} |")
    lines += ['', '## Compensación y ausencia de efecto en las alas', '']
    grouped = {}
    for r in rows:
        grouped.setdefault((r['id'], r['tempo'], r['form']), []).append(r)
    fields = ['perfectDistinctSteps', 'LarcM', 'adaptiveRadiusM', 'nonzeroNodes', 'zeroSpanSteps', 'nonzeroContactMs', 'earlyNonzeroMs', 'lateNonzeroMs']
    differences = [(key, field) for key, group in grouped.items() for field in fields
                   if max(float(r[field]) for r in group) - min(float(r[field]) for r in group) > 1e-9]
    lines.append(f'Diferencias entre compensaciones −75/0/+75 mayores que 1e-9 en estos campos descriptivos: **{len(differences)}**. Esa cifra solo filtra redondeo al resumir; no cambia contrato, ventanas ni evaluación de d>0.')
    for form in ['trajectory', 'adaptive-radius', 'fixed-radius']:
        selected = [r for r in rows if r['form'] == form and r['compMs'] == '0']
        wiped = sum(float(r['validNonPerfectMs']) > 0 and float(r['nonzeroContactMs']) == 0 for r in selected)
        early = sum(float(r['earlyContactMs']) > 1e-9 and float(r['earlyNonzeroMs']) == 0 for r in selected)
        late = sum(float(r['lateContactMs']) > 1e-9 and float(r['lateNonzeroMs']) == 0 for r in selected)
        lines.append(f'- {form}: {wiped}/90 condiciones sin vector pese a contactos Valid; alas con contactos pero sin vector: temprana {early}, tardía {late}.')
    lines += ['', '## Integridad aritmética de las duraciones', '']
    error = max(abs(float(r['contactMs']) + float(r['failedMs']) - 200) for r in rows)
    lines.append(f'Máximo residuo de contacto+fallo−200 ms: {error:.17g} ms (IEEE double; no afecta categorías ni se resta de métricas).')
    (HERE / 'tables.md').write_text('\n'.join(lines) + '\n')

if __name__ == '__main__':
    main()
