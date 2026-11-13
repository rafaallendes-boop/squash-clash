"""Derive portable annex C evidence; no Unity invocation and no product mutation."""
import argparse, collections, csv, gzip, hashlib, io, json
from pathlib import Path

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('run01',type=Path)
parser.add_argument('run02',type=Path)
parser.add_argument('output',type=Path)
a=parser.parse_args()
a.output.mkdir(parents=True,exist_ok=True)
files=['trials.tsv','pairs.tsv','summary.json','witnesses.json']
manifest={'files':{},'identicalExecutions':True}
for name in files:
    raw=(a.run01/name).read_bytes()
    if raw!=(a.run02/name).read_bytes():raise ValueError('Executions differ: '+name)
    target=name+'.gz' if name.endswith('.tsv') else name
    data=gzip.compress(raw,mtime=0) if target.endswith('.gz') else raw
    (a.output/target).write_bytes(data)
    manifest['files'][target]={'sha256':hashlib.sha256(data).hexdigest(),'bytes':len(data),'uncompressedSha256':hashlib.sha256(raw).hexdigest(),'uncompressedBytes':len(raw)}
trials=list(csv.DictReader((a.run01/'trials.tsv').open(),delimiter='\t'))
pairs=list(csv.DictReader((a.run01/'pairs.tsv').open(),delimiter='\t'))
summary=json.loads((a.run01/'summary.json').read_text())
if len(trials)!=6*19*16*21 or len(trials)!=summary['trialCount']:raise ValueError('Incomplete grid')
if collections.Counter(r['relation'] for r in pairs)!={'R1':36480,'R2':35910,'R3':42}:raise ValueError('Incomplete comparison enumeration')
for relation in summary['relations']:
    rows=[p for p in pairs if p['relation']==relation['id']]
    counts=collections.Counter(r['status'] for r in rows)
    for field,status in [('increasing','Increasing'),('equal','Equal'),('counterexamples','Counterexample'),('notEvaluated','NotEvaluated')]:
        if relation[field]!=counts[status]:raise ValueError('Relation count mismatch')
    relation['notEvaluatedReasons']=dict(sorted(collections.Counter(r['reason'] for r in rows if r['status']=='NotEvaluated').items()))
manifest['source']=summary['source']
manifest['gridTrials']=len(trials)
manifest['relations']=summary['relations']
manifest['terminalCounts']=dict(sorted(collections.Counter(t['terminal'] for t in trials).items()))
# Every counterexample is joined to the complete geometry of both shots, without rounding.
joined=io.StringIO(newline='')
columns=list(pairs[0])+['a_'+k for k in trials[0]]+['b_'+k for k in trials[0]]
w=csv.DictWriter(joined,fieldnames=columns,delimiter='\t',lineterminator='\n');w.writeheader()
for pair in pairs:
    if pair['status']!='Counterexample':continue
    row=dict(pair)
    row.update({'a_'+k:v for k,v in trials[int(pair['a'])].items()})
    row.update({'b_'+k:v for k,v in trials[int(pair['b'])].items()})
    w.writerow(row)
data=joined.getvalue().encode();(a.output/'counterexamples.tsv').write_bytes(data)
manifest['files']['counterexamples.tsv']={'sha256':hashlib.sha256(data).hexdigest(),'bytes':len(data)}
(a.output/'manifest.json').write_text(json.dumps(manifest,indent=2,sort_keys=True)+'\n')
print(json.dumps({'source':manifest['source'],'trials':len(trials),'pairs':len(pairs),'identical':True,'files':len(manifest['files'])}))
