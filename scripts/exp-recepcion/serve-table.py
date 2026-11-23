# Every match point served by the rival in the given MetricsLog files: did the human return it, and if not, why (recorded fields only).
import json,sys,glob,collections
MS=1000/(120*0.65)  # real ms per step: physics.stepHz 120, pace.tempo 0.65 (DefaultPreset; checked against applied shots)
rows=[]
for f in sorted(glob.glob(sys.argv[1]+'/*.jsonl')):
    seg=[]
    for l in open(f):
        r=json.loads(l);p=json.loads(r['payload']) if r['payload'] else {}
        if r['kind']!='rally-point': seg.append((r['kind'],p)); continue
        s,seg=seg,[]
        if p.get('server')!='Rival' or not p.get('match') or p.get('autopilot'): continue
        lid=lambda e:e.get('LaunchId',e.get('launchId'))
        ids=[lid(e) for k,e in s if k in('shot','shot-opportunity','shot-order','shot-opportunity-choice') and lid(e)]
        serve=min(ids) if ids else None
        hs=[e for k,e in s if k=='shot' and lid(e)==serve and e.get('Reason')!='rival-contact']
        ch=[e for k,e in s if k=='shot-opportunity-choice' and lid(e)==serve]
        op=[e for k,e in s if k=='shot-opportunity' and lid(e)==serve]
        returned=bool(p.get('causal')) and p['causal'][0].get('player')=='Human'
        h=hs[-1] if hs else {}
        margin=None
        if h.get('Result')=='Failed':
            if h['Fail']=='Air': margin=h['DeltaMs']-(h['KInSteps']-h['TStarStep'])*MS
            else: margin=h['DeltaMs']-(h['KOutSteps']-h['TStarStep'])*MS
        rows.append(dict(file=f.split('metrics-')[1][:15],build=r.get('build'),level=p.get('level'),style=p.get('style'),serve=p.get('serveType'),
            returned=returned,shots=p.get('shots'),reception=None if returned else p.get('reception'),result=h.get('Result'),fail=h.get('Fail'),
            reason=h.get('Reason'),dms=h.get('DeltaMs'),margin=margin,choice=ch[-1]['choice'] if ch else ('no-offer' if op and not op[-1]['offered'] else None),
            offered=op[-1]['offered'] if op else None))
json.dump(rows,open(sys.argv[2],'w'),indent=1)
for x in rows:
    m='' if x['margin'] is None else f"{x['margin']:+.0f}ms"
    print(x['file'],x['build'][6:13],x['level'],x['style'],x['serve'],'RET' if x['returned'] else 'miss',x['reception'],x['result'],x['fail'],x['reason'],x['choice'],m)
