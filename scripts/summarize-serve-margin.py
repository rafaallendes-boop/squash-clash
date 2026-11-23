#!/usr/bin/env python3
"""Summarize the disposable bounded serve reception experiment without changing criteria."""
import csv
import collections
import json
from pathlib import Path
import random
import sys

root=Path(sys.argv[1])
out=Path(sys.argv[2])
out.mkdir(parents=True,exist_ok=True)
def read(path):
    with path.open() as f:return list(csv.DictReader(f,delimiter='\t'))
def write(name,rows):
    with (out/name).open('w') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]),delimiter='\t');w.writeheader();w.writerows(rows)
points=read(root/'Logs/experiments/serve-margin/options01/points.tsv')
groups=collections.defaultdict(list)
for r in points:
    for style in (r['style'],'ALL'):
        groups[r['variant'],r['human'],r['level'],style].append(r)
def stats(rows):
    scored=[r for r in rows if r['winner']]
    serves=[r for r in rows if r['server']=='Rival']
    returned=[r for r in serves if r['receiverResult']=='Contact']
    last={}
    for r in rows:last[r['style'],r['match']]=r
    wins=sum(int(r['matchHuman'])>int(r['matchRival']) for r in last.values())
    hp=sum(r['winner']=='Human' for r in scored)
    vc=sum(r['receiverVolley']=='1' for r in returned)
    offered=sum(r['receiverVolleyOffered']=='1' for r in serves)
    choices=sum(r['receiverVolley']=='1' for r in serves)
    pct=lambda n,d:100*n/d if d else 0
    return dict(matches=len(last),humanMatchWins=wins,matchWinPct=pct(wins,len(last)),points=len(scored),humanPoints=hp,pointWinPct=pct(hp,len(scored)),
        serves=len(serves),returns=len(returned),returnPct=pct(len(returned),len(serves)),volleyOffers=offered,volleyOfferPct=pct(offered,len(serves)),
        volleyChoices=choices,volleyChoicePct=pct(choices,len(serves)),volleyContacts=vc,volleyPerServePct=pct(vc,len(serves)),volleyAmongReturnsPct=pct(vc,len(returned)))
summary=[dict(zip(('variant','human','level','style'),k),**stats(rows)) for k,rows in groups.items()]
write('reception-summary.tsv',summary)

# Paired match-cluster bootstrap: fixed seeds identify the same (style,match) unit across variants.
# This quantifies the secondary signal; it adds no pass threshold or new oracle.
cluster={}
for variant in ('base','early25','early50'):
    for level in ('Easy','Medium','Hard'):
        units=collections.defaultdict(list)
        for r in groups[variant,'touch',level,'ALL']:units[r['style'],r['match']].append(r)
        cluster[variant,level]={k:(sum(r['receiverResult']=='Contact' for r in v if r['server']=='Rival'),sum(r['server']=='Rival' for r in v)) for k,v in units.items()}
intervals=[]
rng=random.Random(20261123)
for level in ('Easy','Medium','Hard'):
    base=cluster['base',level];keys=sorted(base)
    for variant in ('early25','early50'):
        alt=cluster[variant,level];assert set(base)==set(alt)
        deltas=[]
        for _ in range(10000):
            picks=rng.choices(keys,k=len(keys))
            b=[sum(base[k][i] for k in picks) for i in (0,1)]
            a=[sum(alt[k][i] for k in picks) for i in (0,1)]
            deltas.append(100*(a[0]/a[1]-b[0]/b[1]))
        deltas.sort()
        b=stats(groups['base','touch',level,'ALL']);a=stats(groups[variant,'touch',level,'ALL'])
        intervals.append(dict(level=level,variant=variant,deltaReturnPp=a['returnPct']-b['returnPct'],ci95LowPp=deltas[249],ci95HighPp=deltas[9749],pairedMatches=len(keys),replicates=10000,seed=20261123))
write('touch-return-bootstrap.tsv',intervals)

cells={}
for variant in ('base','early25','early50'):
    cells[variant]={(r['level'],r['style'],r['reference']):r for r in read(root/f'Logs/experiments/rival-styles-calibration/n2-{variant}01/cells.tsv') if r['phase']=='confirm'}
n2=[]
for (level,style,reference),b in cells['base'].items():
    for variant in ('base','early25','early50'):
        a=cells[variant][level,style,reference]
        delta=100*(float(a['share'])-float(b['share']))
        n2.append(dict(variant=variant,level=level,style=style,reference=reference,matches=200,k=a['k'],baseK=b['k'],points=a['points'],humanPoints=a['human'],sharePct=100*float(a['share']),baseSharePct=100*float(b['share']),deltaPp=delta,breaksN2=reference=='newcomer' and abs(delta)>5))
write('n2-shift.tsv',n2)
print(json.dumps({'pointsRows':len(points),'cells':len(summary),'n2Rows':len(n2),'n2MaxAbsPp':{v:max(abs(r['deltaPp']) for r in n2 if r['variant']==v and r['reference']=='newcomer') for v in ('early25','early50')},'secondary':intervals},indent=2))
