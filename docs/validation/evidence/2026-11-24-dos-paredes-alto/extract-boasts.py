import json,sys,glob,os
files=sys.argv[1:]
seen={};out=[]
for f in files:
    ev=[json.loads(l) for l in open(f)]
    for i,e in enumerate(ev):
        if e['kind']!='shot-boast': continue
        b=json.loads(e['payload']); a=b['attemptId']
        if a in seen: continue
        seen[a]=1
        shot=causal=opp=None;rp=None
        for e2 in ev:
            if e2['kind'] in('shot','shot-causal','shot-opportunity-order'):
                p=json.loads(e2['payload'])
                if p.get('AttemptId',p.get('attemptId'))==a:
                    if e2['kind']=='shot' and p.get('Applied'): shot=p
                    elif e2['kind']=='shot-causal': causal=p
                    elif e2['kind']=='shot-opportunity-order': opp=p
        for e2 in ev[i:]:
            if e2['kind']=='rally-point':
                rp=json.loads(e2['payload']); rp['ts']=e2['ts']; break
        # next rival contact after this attempt
        nxt=None
        if rp:
            cs=rp['causal']; 
            for k,c in enumerate(cs):
                if c.get('attemptId')==a and k+1<len(cs): nxt=cs[k+1]
        out.append(dict(file=os.path.basename(f),ts=e['ts'],build=e['build'],phase=e['phase'],boast=b,shot=shot,causal=causal,opp=opp,
            point=None if not rp else {k:rp.get(k) for k in('ts','winner','cause','reception','server','level','style','shots','match','rallyParams')},
            pointLastCausal=None if not rp else rp['causal'][-1].get('player'), next=nxt))
json.dump(out,open('boasts.json','w'),indent=1)
for o in out:
    b=o['boast'];s=o['shot'] or {};c=o['causal'] or {}
    print(o['ts'][:19],o['build'][6:13],o['phase'],b['wall'],b['reason'],'fb' if b['fellBack'] else '',
      'aim(z,y)=%.2f,%.2f used=%.2f,%.2f front=%.2f,%.2f v=%.1f'%(b['aimZ'],b['aimY'],b['usedZ'],b['usedY'],b['frontX'],b['frontY'],b['speed']),
      'C=(%.2f,%.2f,%.2f)'%(s.get('ContactX',0),s.get('ContactY',0),s.get('ContactZ',0)),'P=%.2f'%s.get('Power',0),'dMs=%.0f'%s.get('DeltaMs',0),s.get('Result'),
      [ (v['Cause'],round(v['DeltaHeight'],2)) for v in c.get('vectors',[])], o['opp'] and o['opp']['opportunity'],
      o['point'] and (o['point']['winner'],o['point']['cause'],o['point']['reception'],o['point']['match']))
