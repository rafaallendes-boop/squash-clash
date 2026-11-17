"""Integra las celdas de Resolve y verifica el muestreo A1-bis; nunca ejecuta Unity."""
import csv
import hashlib
import json
import math
import statistics
import sys
from collections import Counter, defaultdict
from pathlib import Path
from xml.etree import ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / 'Logs/experiments/g1-release-margin'
RUN = sys.argv[1]
assert RUN in ('run01', 'run02')
DIRECTORY = BASE / RUN
RAW = ('source.tsv', 'plans.tsv', 'intervals.tsv', 'samples.tsv')


def read(name):
    with (DIRECTORY / name).open() as stream:
        reader = csv.DictReader(stream, delimiter='\t')
        assert all(reader.fieldnames) and len(set(reader.fieldnames)) == len(reader.fieldnames)
        rows = list(reader)
        assert all(None not in row and None not in row.values() for row in rows)
        return rows


def key(row):
    return tuple(row[k] for k in ('id', 'receiver', 'difficulty'))


def cdf(value, sigma):
    return .5 * math.erfc(-value / (sigma * math.sqrt(2)))


def write(path, rows):
    fields = list(dict.fromkeys(k for row in rows for k in row))
    assert fields and all(fields)
    with path.open('w') as stream:
        writer = csv.DictWriter(stream, fields, delimiter='\t', lineterminator='\n')
        writer.writeheader()
        writer.writerows(rows)


def file_meta(path):
    return {'sha256': hashlib.sha256(path.read_bytes()).hexdigest(), 'bytes': path.stat().st_size}


result = ET.parse(BASE / (RUN + '-runner') / 'results.xml').getroot().attrib
assert result['total'] == '1' and result['passed'] == '1' and result['result'] == 'Passed', result
source = read('source.tsv')[0]
plans, cells, samples = read('plans.tsv'), read('intervals.tsv'), read('samples.tsv')
assert len(plans) == 648 and len(samples) == 58200
bykey = {key(row): row for row in plans}
assert len(bykey) == 648
intervals = defaultdict(list)
observed = defaultdict(Counter)
for cell in cells:
    intervals[key(cell)].append(cell)
for sample in samples:
    observed[key(sample)][sample['category']] += 1

probabilities = []
max_error = 0
for row in plans:
    identity = key(row)
    usable = row['usable'] == 'true'
    p = dict.fromkeys(('Contact', 'Air', 'Late'), 0.)
    if usable:
        parts = intervals[identity]
        assert float(parts[0]['lowMs']) == -math.inf and float(parts[-1]['highMs']) == math.inf
        assert all(float(a['highMs']) == float(b['lowMs']) for a, b in zip(parts, parts[1:]))
        assert all(float(part['lowMs']) < float(part['highMs']) for part in parts)
        sigma = float(row['sigmaMs'])
        if sigma == 0:
            assert row['zeroResult'] in ('Perfect', 'Valid')
            p['Contact'] = 1.
        else:
            for part in parts:
                p[part['category']] += cdf(float(part['highMs'])-float(row['meanShiftMs']), sigma) - cdf(float(part['lowMs'])-float(row['meanShiftMs']), sigma)
            assert abs(sum(p.values()) - 1) <= 1e-12
            assert sum(observed[identity].values()) == 200
            for category, expected in p.items():
                actual = observed[identity][category] / 200
                error = abs(actual - expected)
                bound = max(.025, 5 * math.sqrt(expected * (1 - expected) / 200))
                assert error <= bound, ('STOP: sampling discrepancy', identity, category, error, bound)
                max_error = max(max_error, error)
    else:
        assert identity not in intervals and identity not in observed
    converted = dict(row)
    converted.update({'p' + category: value for category, value in p.items()})
    converted.update({'sample' + category: observed[identity][category] for category in p})
    probabilities.append(converted)

frozen_path=ROOT/'docs/validation/evidence/2026-11-17-g1-a1bis-timing/probabilities.tsv'
with frozen_path.open() as stream:
    frozen={key(r):r for r in csv.DictReader(stream,delimiter='\t')}
for row in probabilities:
    old=frozen[key(row)]
    assert row['sigmaMs']==old['sigmaMs']
    if row['receiver']=='Human':
        for column in ['plan','star','contactSpeed','kInMs','kOutMs','widthMs','zeroResult','zeroFail']:
            assert row[column]==old[column],('human changed',key(row),column)
    if row['identityEligible']=='true':
        assert row['identityVerified']=='true'
for sample in samples:
    if bykey[key(sample)]['identityEligible']=='true':
        assert sample['rawDeltaMs']==sample['deltaMs'],('identity changed',key(sample))
rates=[]
for level in ['Easy','Medium','Hard']:
    population=[r for r in probabilities if r['receiver']=='Rival' and r['difficulty']==level and r['flightCause']=='DoubleBounce']
    assert len(population)==103
    rates.append(sum(r['pContact'] for r in population)/103)
assert rates[0]<rates[1]<rates[2],('difficulty order',rates)
strong=[r for r in probabilities if r['receiver']=='Rival' and r['difficulty']=='Medium' and r['power']=='1' and r['usable']=='true']
assert len(strong)==29
strong_rate=sum(r['pContact'] for r in strong)/29
assert strong_rate>=.90,('P1 target',strong_rate)
assert source['rallyProjection']=='7d50ddaf5d4f'
assert source['defaults']=='430da2faa818' and source['simulationKey']=='166a96e319c7'
criteria={'identityEligiblePlans':sum(r['identityEligible']=='true' for r in probabilities),'difficultyRates':rates,'mediumP1PerPlan':strong_rate,'humanUnchanged':True}
checks = []
for level in ('Easy', 'Medium', 'Hard'):
    rows = [r for r in probabilities if r['receiver'] == 'Rival' and r['difficulty'] == level and r['usable'] == 'true']
    for category in ('Contact', 'Air', 'Late'):
        expected = sum(r['p' + category] for r in rows) / len(rows)
        actual = sum(r['sample' + category] for r in rows) / (200 * len(rows))
        variance = sum(200 * r['p' + category] * (1 - r['p' + category]) for r in rows)
        bound = .005 + 5 * math.sqrt(variance) / (200 * len(rows))
        assert abs(actual - expected) <= bound, ('STOP: aggregate discrepancy', level, category)
        checks.append({'difficulty': level, 'category': category, 'nPlans': len(rows), 'expected': expected, 'sampled': actual, 'error': abs(actual - expected), 'bound': bound})


def band(value):
    value = float(value)
    if value < 0 or value >= 32:
        return 'residual'
    low = 4 * int(value // 4)
    return f'{low:02d}-{low+4:02d}'


def aggregate(rows, dimensions, name):
    groups = defaultdict(list)
    for row in rows:
        groups[tuple(row[d] for d in dimensions)].append(row)
    output = []
    for identity in sorted(groups):
        group = groups[identity]
        legal = [r for r in group if r['flightCause'] == 'DoubleBounce']
        ready = [r for r in legal if r['usable'] == 'true']
        widths = [float(r['widthMs']) for r in ready]
        speeds = [float(r['contactSpeed']) for r in ready]
        early = [-float(r['kInMs']) for r in ready]
        late = [float(r['kOutMs']) for r in ready]
        output.append({'group': name, **dict(zip(dimensions, identity)), 'n': len(group), 'legal': len(legal), 'out': len(group) - len(legal),
                       'planned': sum(r['plan'] == 'planned' for r in legal), 'stretched': sum(r['plan'] == 'stretched' for r in legal),
                       'unreachable': len(legal) - len(ready), 'nPlans': len(ready),
                       'contactExpected': sum(r['pContact'] for r in ready), 'airExpected': sum(r['pAir'] for r in ready), 'lateExpected': sum(r['pLate'] for r in ready),
                       'contactPerLegal': sum(r['pContact'] for r in ready) / len(legal) if legal else '',
                       'contactPerPlan': sum(r['pContact'] for r in ready) / len(ready) if ready else '',
                       'contactPerAll': sum(r['pContact'] for r in ready) / len(group),
                       'sampleContact': sum(r['sampleContact'] for r in ready), 'sampleAir': sum(r['sampleAir'] for r in ready), 'sampleLate': sum(r['sampleLate'] for r in ready),
                       'widthMedianMs': statistics.median(widths) if widths else '', 'widthMinMs': min(widths) if widths else '', 'widthMaxMs': max(widths) if widths else '',
                       'earlyMedianMs': statistics.median(early) if early else '', 'lateMedianMs': statistics.median(late) if late else '',
                       'contactSpeedMedian': statistics.median(speeds) if speeds else ''})
    return output


for row in probabilities:
    row['launchBand'] = band(row['launchSpeed'])
    row['contactBand'] = band(row['contactSpeed']) if row['contactSpeed'] else 'no-contact'
tables = []
for dimensions in [('receiver', 'difficulty'), ('receiver', 'difficulty', 'power'), ('receiver', 'difficulty', 'launchBand'),
                   ('receiver', 'difficulty', 'backInPlay')]:
    tables += aggregate(probabilities, dimensions, '+'.join(dimensions))
tables += aggregate([r for r in probabilities if r['receiver'] == 'Rival'], ('difficulty', 'contactBand'), 'difficulty+contactBand')
write(DIRECTORY / 'probabilities.tsv', probabilities)
write(DIRECTORY / 'tables.tsv', tables)
write(DIRECTORY / 'checks.tsv', checks)
analysis = {'criteria':criteria,'source': source, 'nPlans': len(plans), 'nCells': len(cells), 'nSamples': len(samples), 'maxPerPlanSamplingError': max_error, 'checks': checks}
(DIRECTORY / 'analysis.json').write_text(json.dumps(analysis, indent=2) + '\n')

if RUN == 'run02':
    for name in (*RAW, 'probabilities.tsv', 'tables.tsv', 'checks.tsv', 'analysis.json'):
        assert file_meta(BASE / 'run01' / name) == file_meta(BASE / 'run02' / name), ('STOP: non-determinism', name)
    out = ROOT / 'docs/validation/evidence/2026-11-17-g1-a1-release-margin'
    out.mkdir(parents=True, exist_ok=True)
    for name in ('probabilities.tsv', 'tables.tsv', 'checks.tsv', 'analysis.json'):
        (out / name).write_bytes((DIRECTORY / name).read_bytes())
    manifest = {'source': source, 'identical': True, 'runs': {}}
    for run in ('run01', 'run02'):
        manifest['runs'][run] = {'files': {name: file_meta(BASE / run / name) for name in (*RAW, 'probabilities.tsv', 'tables.tsv', 'checks.tsv', 'analysis.json')},
                                 'runnerFiles': {p.name: file_meta(p) for p in sorted((BASE / (run + '-runner')).iterdir()) if p.is_file()}}
    for phase in ('red', 'green'):
        manifest[phase] = {p.name: file_meta(p) for p in sorted((ROOT / 'Logs/evidence/g1-a1' / phase).iterdir()) if p.is_file()}
    (out / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    assert sum(p.stat().st_size for p in out.iterdir()) < 1_000_000
print(json.dumps(analysis, indent=2))
