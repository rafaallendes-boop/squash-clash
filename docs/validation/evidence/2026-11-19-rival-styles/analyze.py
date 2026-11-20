import csv,sys,math,collections,statistics as st
d=sys.argv[1]
def rows(n):
    with open(f'{d}/{n}') as f: return list(csv.DictReader(f,delimiter='\t'))
def wilson(k,n,z=1.96):
    if n==0: return (0,0)
    p=k/n;den=1+z*z/n;c=(p+z*z/(2*n))/den;h=z*math.sqrt(p*(1-p)/n+z*z/(4*n*n))/den
    return (c-h,c+h)
P=rows('points.tsv');R=rows('receptions.tsv');CL=rows('cells.tsv')
key=lambda r:(r['reference'],r['level'],r['wait'])
print('== cells: human point-win % [95% CI], match-win %, rally shots median/mean')
pts=collections.defaultdict(list)
for r in P: pts[key(r)].append(r)
for c in CL:
    k=key(c);n=int(c['humanWins'])+int(c['rivalWins']);h=int(c['humanWins']);lo,hi=wilson(h,n)
    sh=[int(x['shots']) for x in pts[k]]
    print(k,'points',n,'human %.1f%% [%.1f-%.1f]'%(100*h/n,100*lo,100*hi),'matches %d/%s'%(int(c['matchesHuman']),c['matches']),'shots med %.0f mean %.1f'%(st.median(sh),st.mean(sh)),'%.0fs'%float(c['seconds']))
print('== point causes (winner, cause, lastReception) per cell, top')
for k,v in pts.items():
    cc=collections.Counter((x['winner'],x['cause'],x['lastReception']) for x in v)
    print(k,[(a,round(100*b/len(v),1)) for a,b in cc.most_common(6)])
print('== per-reception result shares')
rc=collections.defaultdict(collections.Counter)
for r in R: rc[(key(r),r['who'])][r['result']]+=1
for k,c in sorted(rc.items()):
    n=sum(c.values());print(k,n,{a:round(100*b/n,1) for a,b in c.most_common()})
print('== rival option share (contacts) per level, wait .5, newcomer')
oc=collections.defaultdict(collections.Counter)
for r in R:
    if r['who']=='Rival' and r['result']=='Contact' and r['wait']=='0.5': oc[r['level']][(r['line'],r['height'],r['power'])]+=1
for lv,c in oc.items():
    n=sum(c.values());lines=collections.Counter();hs=collections.Counter()
    for (l,h,p),b in c.items(): lines[l]+=b;hs[h]+=b
    print(lv,n,'lines',{a:round(100*b/n,1) for a,b in lines.items()},'heights',{a:round(100*b/n,1) for a,b in hs.items()},'top',[(a,round(100*b/n,1)) for a,b in c.most_common(4)])
print('== short-front positioning: receptions by striker contact z bucket')
T=5.465
def bucket(z):
    z=float(z)
    return '<2' if z<2 else '2-3.5' if z<3.5 else '3.5-T' if z<T else '>=T'
def bb(z):
    if z in('','NaN'):return 'none'
    z=float(z);return '<3' if z<3 else '3-5' if z<5 else '5-7' if z<7 else '>=7'
agg=collections.defaultdict(lambda:collections.Counter());sl=collections.defaultdict(list);sz=collections.defaultdict(list)
for r in R:
    if r['strikerZ']=='' or r['shots']=='0':continue
    k=(r['reference'],r['level'],r['wait'],r['who'],bucket(r['strikerZ']))
    agg[k][r['result']]+=1;agg[k]['n']+=1;agg[k]['offered']+=int(r['volleyOffered'])
    if r['reachable']=='1':sl[k].append(float(r['slack']))
    sz[k].append(float(r['selfZ']))
for k in sorted(agg):
    if k[0]!='newcomer' or k[1]!='Medium' and k[2]=='0.5' and False:continue
    c=agg[k];n=c['n']
    print(k,'n',n,'unreach %.1f%% air %.1f%% late %.1f%% contact %.1f%%'%(100*c['Unreachable']/n,100*c['Air']/n,100*c['Late']/n,100*c['Contact']/n),'slack med %.2f'%(st.median(sl[k]) if sl[k] else float('nan')),'selfZ med %.2f'%st.median(sz[k]),'volleyOffered %d'%c['offered'])
print('== rival receptions after a short ball (bounceZ<3) by striker z, wait variants, newcomer Medium')
a2=collections.defaultdict(collections.Counter)
for r in R:
    if r['who']!='Rival' or r['reference']!='newcomer' or r['level']!='Medium' or r['strikerZ']=='' or r['shots']=='0':continue
    a2[(r['wait'],bb(r['bounceZ']))][r['result']]+=1
for k in sorted(a2):
    c=a2[k];n=sum(c.values());print(k,n,{a:round(100*b/n,1) for a,b in c.most_common()})
print('== what the rival played before a human strike from the front (<2 m), newcomer Medium wait .5')
prev=collections.Counter();
by=collections.defaultdict(list)
for r in R: by[(key(r),r['match'],r['point'])].append(r)
for (k,m,p),lst in by.items():
    if k!=('newcomer','Medium','0.5'):continue
    lst.sort(key=lambda x:int(x['k']))
    for i,r in enumerate(lst):
        if r['who']=='Rival' and r['strikerZ']!='' and float(r['strikerZ'])<2 and i>=2:
            q=lst[i-2]  # the rival's previous reception
            if q['who']=='Rival': prev[(q['line'],q['height'],q['power'],q['result'])]+=1
n=sum(prev.values());print(n,[(a,round(100*b/n,1)) for a,b in prev.most_common(8)])
