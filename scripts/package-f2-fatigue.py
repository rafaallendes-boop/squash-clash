#!/usr/bin/env python3
"""Empaqueta la evidencia sanitizada del experimento B de F2 (derivados < 1 MB) con SHA-256 de los datos crudos."""
import csv,hashlib,json,shutil,sys
from pathlib import Path
root=Path(__file__).resolve().parent.parent
logs=root/'Logs/experiments/f2-fatigue';out=root/'docs/validation/evidence/2026-11-17-f2-fatigue'
baseline=root/'docs/validation/evidence/2026-11-17-f1-baseline'
assert not out.exists(),'Do not overwrite evidence'
sha=lambda p:hashlib.sha256(Path(p).read_bytes()).hexdigest()
def rows(path):
    with Path(path).open() as f:return list(csv.DictReader(f,delimiter='\t'))
manifest={'schema':'f2-fatigue-evidence/1','method':'docs/validation/2026-11-17-f2-fatigue-method.md','variants':{}}
for variant in ('f2off','f2all'):
    data=logs/variant;target=out/variant;target.mkdir(parents=True)
    raw={n:sha(data/n) for n in ('points.tsv','receptions.tsv','matches.tsv','source.tsv')}
    runner={n:sha(logs/f'{variant}-runner'/n) for n in ('unity-processes.tsv','results.xml','unity.log','head.txt','status.txt')}
    for n in ('matches.tsv','source.tsv'):shutil.copy(data/n,target/n)
    for n in ('summary.json','report.md'):shutil.copy(logs/f'{variant}-summary'/n,target/n)
    points=rows(data/'points.tsv')
    if variant=='f2off':
        prior={(p['match'],p['point']):p['signature'] for p in rows(baseline/'points.tsv')}
        with (target/'identity.tsv').open('w') as f:
            f.write('match\tpoint\tsignature\tbaselineSignature\tequal\n')
            for p in points:
                q=prior[p['match'],p['point']];f.write(f"{p['match']}\t{p['point']}\t{p['signature']}\t{q}\t{p['signature']==q}\n")
    else:
        import json as j
        with (target/'points-lite.tsv').open('w') as f:
            f.write('match\tpoint\thumanBefore\trivalBefore\tserver\twinner\tshots\tsteps\trealSeconds\tsignature\thumanStart\trivalStart\n')
            for p in points:
                f.write('\t'.join([p['match'],p['point'],p['humanBefore'],p['rivalBefore'],p['server'],p['winner'],p['shots'],p['steps'],p['realSeconds'],p['signature'],str(j.loads(p['fatigueHuman'])['Start']),str(j.loads(p['fatigueRival'])['Start'])])+'\n')
    manifest['variants'][variant]={'head':(logs/f'{variant}-runner'/'head.txt').read_text().strip(),'gitStatusBefore':(logs/f'{variant}-runner'/'status.txt').read_text(),
        'rawDataSha256':raw,'runnerSha256':runner,'dataUnchangedAfterPackaging':all(sha(data/n)==raw[n] for n in raw)}
attempt=logs/'f2off-runner-attempt1'
manifest['attempts']={'F2-EXP-01':{'what':'first f2off invocation stopped by its own expected-hash assertion before any data (see the method erratum)','sha256':{n:sha(attempt/n) for n in ('results.xml','unity.log','head.txt','unity-processes.tsv')}}}
out.mkdir(exist_ok=True);(out/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps(manifest['variants']['f2all']['rawDataSha256'],indent=1))
