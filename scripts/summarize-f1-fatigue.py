#!/usr/bin/env python3
"""Resume B1–B6 con el método predeclarado; nunca modifica los datos."""
import csv,hashlib,json,statistics,sys
from pathlib import Path
root=Path(__file__).resolve().parent.parent
variant=sys.argv[1]
assert variant in ('off','soft','adjusted')
data=root/'Logs/experiments/f1-fatigue'/variant
baseline=root/'docs/validation/evidence/2026-11-17-f1-baseline'
output=root/'Logs/experiments/f1-fatigue'/f'{variant}-summary'
assert not output.exists(),'Do not overwrite summaries'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def rows(path):
    with path.open() as f:return list(csv.DictReader(f,delimiter='\t'))
manifest=json.loads((baseline/'manifest.json').read_text())
for name,digest in manifest['files'].items():assert sha(baseline/name)==digest,(name,'baseline changed')
files=[*data.glob('*.tsv'),*baseline.glob('*.tsv')]
before={str(p.relative_to(root)):sha(p) for p in files}
points=rows(data/'points.tsv');matches=rows(data/'matches.tsv');old=rows(baseline/'points.tsv')
assert len(matches)==200 and {int(m['match']) for m in matches}==set(range(200))
assert sum(int(m['points']) for m in matches)==len(points)
prior={(p['match'],p['point']):p for p in old}
identity=[]
if variant=='off':
    assert len(points)==len(old)
    for p in points:
        q=prior[p['match'],p['point']]
        assert p['signature']==q['signature'],(p['match'],p['point'],'bitwise mismatch')
    identity={'points':len(points),'bitwiseIdentical':True}
for p in points:
    p['fatigue']=[json.loads(p[k]) for k in ('fatigueHuman','fatigueRival')]
    assert all(0<=f['Minimum']<=100 and 0<=f['Start']<=100 and 0<=f['End']<=100 and 0<=f['AfterRecovery']<=100 for f in p['fatigue'])
median=lambda a:statistics.median(a) if a else None
check=lambda value:None if value is None else bool(value)
short=[p for p in points if int(p['shots'])<=4];long=[p for p in points if int(p['shots'])>=15]
net=[median([p['fatigue'][i]['Start']-p['fatigue'][i]['End'] for p in short]) for i in (0,1)]
longer=median([max(f['MovementCost'] for f in p['fatigue']) for p in long])
difference=median([abs(p['fatigue'][0]['MovementCost']-p['fatigue'][1]['MovementCost']) for p in long])
by_match={str(i):[p for p in points if p['match']==str(i)] for i in range(200)}
last=median([f['Start'] for ps in by_match.values() for f in ps[-1]['fatigue']])
onset=70
under=sum(any(f['Minimum']<onset for p in ps for f in p['fatigue']) for ps in by_match.values())
low=sum(any(f['Start']<20 for p in ps for f in p['fatigue']) for ps in by_match.values())
zero_serve=sum(p['fatigue'][0 if p['server']=='Human' else 1]['Start']==0 for p in points)
close=[p for p in points if int(p['humanBefore'])>=6 and int(p['rivalBefore'])>=6]
close_energy=median([f['Start'] for p in close for f in p['fatigue']])
qualifying=[];paired=[]
for p in close:
    energies=[f['Start'] for f in p['fatigue']]
    if abs(energies[0]-energies[1])<20:continue
    who='Human' if energies[0]<energies[1] else 'Rival';qualifying.append((p,who))
    q=prior.get((p['match'],p['point']))
    if q and int(q['humanBefore'])>=6 and int(q['rivalBefore'])>=6:paired.append((p,q,who))
wins=sum(p['winner']==who for p,q,who in paired);old_wins=sum(q['winner']==who for p,q,who in paired)
seconds=statistics.mean(float(p['realSeconds']) for p in points);old_seconds=statistics.mean(float(p['realSeconds']) for p in old)
ratio=seconds/old_seconds
criteria={
 'B1':{'shortPoints':len(short),'humanMedianNetCost':net[0],'rivalMedianNetCost':net[1],'pass':all(v<=4 for v in net) if all(v is not None for v in net) else None},
 'B2':{'longPoints':len(long),'largerMovementCostMedian':longer,'movementDifferenceMedian':difference,'pass':longer>=8 and difference>=4 if longer is not None else None},
 'B3':{'lastPointStartMedian':last,'matchesBelowOnset':under,'matches':200,'pass':45<=last<=75 and under>=100},
 'B4':{'matchesStartingBelow20':low,'matches':200,'zeroEnergyServes':zero_serve,'pass':low<=10 and zero_serve==0},
 'B5a':{'closePoints':len(close),'startEnergyMedian':close_energy,'pass':check(close_energy>=35) if close_energy is not None else None},
 'B5b':{'exploratory':True,'qualifyingPoints':len(qualifying),'pairedPoints':len(paired),'missingPairs':len(qualifying)-len(paired),'fatiguedPlayerWins':wins,'samePlayerBaselineWins':old_wins,'percentagePointDifference':100*(wins-old_wins)/len(paired) if paired else None},
 'B6':{'meanRealSeconds':seconds,'baselineMeanRealSeconds':old_seconds,'ratio':ratio,'pass':.85<=ratio<=1.15}}
# Predeclared one-step adjustment. No tuning is performed by this script.
high=last>75
low_signal=last<45 or criteria['B4']['pass'] is False or criteria['B5a']['pass'] is False
trigger=any(criteria[k]['pass'] is False for k in ('B3','B4','B5a'))
adjustment=None
if variant=='soft' and trigger:
    if high and low_signal:adjustment={'action':'report','reason':'contradictory energy signals'}
    elif low_signal:adjustment={'key':'fatigue.recoverBetween','from':.35,'to':.45,'reason':'B3/B4/B5a low-energy failure'}
    elif high:adjustment={'key':'fatigue.recoverBetween','from':.35,'to':.25,'reason':'B3 last-point energy too high'}
    else:adjustment={'action':'report','reason':'B3 fraction below onset failed without high/low median signal'}
assert {str(p.relative_to(root)):sha(p) for p in files}==before,'Data changed during summary'
output.mkdir()
report={'schema':'f1-fatigue-summary/1','variant':variant,'matches':200,'points':len(points),'identity':identity,'criteria':criteria,'predeclaredAdjustment':adjustment,'dataSha256':before,'dataUnchanged':True}
(output/'summary.json').write_text(json.dumps(report,indent=2)+'\n')
text=['# F1 — experimento B: '+variant,'','Método predeclarado; 200 partidos. NotEvaluated no cuenta como aprobado.','', '| Criterio | Resultado | Datos |','|---|---|---|']
for k,v in criteria.items():
    state='Indicador' if k=='B5b' else 'NotEvaluated' if v['pass'] is None else 'Pass' if v['pass'] else 'Fail'
    text.append('| '+k+' | '+state+' | '+json.dumps({a:b for a,b in v.items() if a!='pass'},ensure_ascii=False)+' |')
text+=['','Ajuste predeclarado propuesto: '+json.dumps(adjustment,ensure_ascii=False),'','Datos intactos por SHA-256.']
(output/'report.md').write_text('\n'.join(text)+'\n')
print(json.dumps({k:v for k,v in report.items() if k!='dataSha256'},indent=2))
