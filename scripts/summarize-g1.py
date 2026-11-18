"""Deriva las tablas G1 de las dos corridas predeclaradas, sin ejecutar Unity."""
import csv, hashlib, json
from collections import Counter, defaultdict
from pathlib import Path
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
base = root/'Logs/experiments/g1'
out = root/'docs/validation/evidence/2026-11-17-g1-diagnosis'
names = ['source.tsv', 'states.tsv', 'contacts.tsv', 'shots.tsv', 'receptions.tsv']
manifest = {'experimentSource': None, 'runs': {}, 'identical': True}
for run in ['run01','run02']:
    result = ET.parse(base/(run+'-runner')/'results.xml').getroot().attrib
    assert result['result'] == 'Passed' and result['total'] == '1' and result['passed'] == '1', result
    manifest['runs'][run] = {'xml': {k:result[k] for k in ['result','total','passed','failed','skipped']}, 'files':{}}
    for name in names:
        p = base/run/name
        manifest['runs'][run]['files'][name] = {'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size}
for name in names:
    a = manifest['runs']['run01']['files'][name]
    b = manifest['runs']['run02']['files'][name]
    if a != b:
        raise SystemExit('STOP: non-determinism in '+name)
for run in ['run01', 'run02']:
    manifest['runs'][run]['runnerFiles'] = {
        p.name: {'sha256': hashlib.sha256(p.read_bytes()).hexdigest(), 'bytes': p.stat().st_size}
        for p in sorted((base/(run+'-runner')).iterdir()) if p.is_file()
    }

def load(name):
    with (base/'run01'/name).open() as f:
        return list(csv.DictReader(f,delimiter='\t'))
def number(r,k):
    return float(r[k]) if r.get(k) else None
def count(rows,k,v='true'):
    return sum(r[k] == v for r in rows)
def write(name,rows):
    keys = list(dict.fromkeys(k for r in rows for k in r))
    with (out/name).open('w') as f:
        w = csv.DictWriter(f,keys,delimiter='\t',lineterminator='\n');w.writeheader();w.writerows(rows)
def groups(rows,keys):
    g = defaultdict(list)
    for row in rows:g[tuple(row[k] for k in keys)].append(row)
    return [(dict(zip(keys,key)),g[key]) for key in sorted(g)]
def span(values):
    return '' if not values else str(min(values))+'..'+str(max(values))

shots,receptions = load('shots.tsv'),load('receptions.tsv')
source = load('source.tsv')[0];manifest['experimentSource'] = source
byid = {r['id']:r for r in shots}
a1 = [r for r in shots if r['section']=='A1']
a2 = [r for r in shots if r['section']=='A2']
assert len(a1)==108 and len(a2)==5880 and len(receptions)==648
for row in receptions:
    shot = byid[row['id']]
    row.update({k:shot[k] for k in ['fixture','power','cause','backY','backInPlay','floorsBeforeBack','secondBackGap']})
out.mkdir(parents=True,exist_ok=True)
(out/'manifest.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False)+'\n')

tables=[]
for keys in [['receiver','difficulty'],['receiver','difficulty','fixture'],['receiver','difficulty','power']]:
    for key, rows in groups(receptions,keys):
        row={'group':'+'.join(keys),**key,'n':len(rows)}
        row.update({'bucket_'+k:count(rows,'bucket',k) for k in ['receivable','a','b','c','d','e']})
        row.update({'plan_'+k:count(rows,'plan',k) for k in ['planned','stretched','unreachable','no-legal-window','Unsolved']})
        glass=[r for r in rows if r['backInPlay']=='true']
        row.update({'legalGlass':len(glass),'legalGlassReceivable':sum(r['plan'] in ['planned','stretched'] for r in glass),'postGlassContact':count(rows,'postBackContact'),'postGlassWindow':sum(int(r['postBackWindow'])>0 for r in rows)})
        late=[int(r['lateStarSteps'])/120 for r in rows if r['bucket']=='b' and r['lateStarSteps']]
        row.update({'lateSeconds':span(late)})
        tables.append(row)
write('a1-tables.tsv',tables)
write('a1-shots.tsv',a1)
write('a1-receptions.tsv',receptions)

tables=[]
for keys in [['fixture','mode'],['fixture','mode','power'],['fixture','mode','column']]:
    for key,rows in groups(a2,keys):
        legal=[r for r in rows if r['cause']=='DoubleBounce' and r['secondZ']]
        witnesses=[r for r in rows if r['drive']=='true' and r['driveLine'] in ['straight','cross']]
        elevations=[number(r,'launchElevation') for r in rows if r['solved']=='true']
        row={'group':'+'.join(keys),**key,'n':len(rows),'solved':count(rows,'solved'),'legal':len(legal),'out':count(rows,'cause','Out'),'unsolved':count(rows,'solved','false'),'capChanged':count(rows,'capChanged'),'effectiveAims':len({(r['aimX'],r['aimY']) for r in rows}),'effectiveShots':len({(r['aimX'],r['aimY'],r['power']) for r in rows}),'driveAnyLine':count(rows,'drive'),'driveStraight':count(witnesses,'driveLine','straight'),'driveCross':count(witnesses,'driveLine','cross'),'effectiveDriveWitnesses':len({(r['aimX'],r['aimY'],r['power'],r['driveLine']) for r in witnesses}),'elevation':span(elevations),'elevationAbove22':sum(x>22 for x in elevations),'targetLowered':count(rows,'targetLowered'),'speedRaised':count(rows,'speedRaised')}
        tables.append(row)
write('a2-tables.tsv',tables)
witnesses=[r for r in a2 if r['drive']=='true' and r['driveLine'] in ['straight','cross']]
write('a2-witnesses.tsv',witnesses)
width=[]
for key,rows in groups(witnesses,['fixture','mode','column','driveLine']):
    powers=sorted({number(r,'power') for r in rows});heights=sorted({number(r,'requestedY') for r in rows});aims=sorted({number(r,'aimY') for r in rows})
    width.append({**key,'n':len(rows),'powers':','.join(map(str,powers)),'requestedHeights':','.join(map(str,heights)),'effectiveHeights':','.join(map(str,aims))})
write('a2-width.tsv',width)
# Width at each fixed power, and power coverage at each fixed requested height.
width=[]
allheights=sorted({number(r,'requestedY') for r in a2});allpowers=sorted({number(r,'power') for r in a2})
for axis,other,grid in [('requestedY','power',allheights),('power','requestedY',allpowers)]:
    for key,rows in groups(witnesses,['fixture','mode','column','driveLine',other]):
        values={number(r,axis) for r in rows};longest=streak=0
        for v in grid:
            streak=streak+1 if v in values else 0;longest=max(longest,streak)
        width.append({**key,'axis':axis,'nDistinct':len(values),'longestContiguousGridPoints':longest,'span':span(list(values))})
write('a2-contiguous-width.tsv',width)
write('a2-cap-mapping.tsv',[{k:r[k] for k in ['fixture','mode','column','requestedY','aimX','aimY','capHeight','launchElevation','targetY']} for r in a2 if r['power']=='0.5'])
assert sum(p.stat().st_size for p in out.iterdir())<1_000_000
print(json.dumps({'source':source,'nShots':len(shots),'nReceptions':len(receptions),'nDriveWitnesses':len(witnesses),'evidenceBytes':sum(p.stat().st_size for p in out.iterdir())},indent=2))
