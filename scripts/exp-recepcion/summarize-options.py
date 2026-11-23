# Summary of ServeReceptionOptionsExperiment points.tsv files: per variant/human/level (and style, serve type) the newcomer's
# P+V return of the rival's serve and its failure mix, the rival's return, the human point and match share and the human volley share.
import csv,sys,collections
rows=[r for f in sys.argv[1:] for r in csv.DictReader(open(f),delimiter='\t')]
def agg(rs):
    rv=[r for r in rs if r['server']=='Rival'];hv=[r for r in rs if r['server']=='Human']
    won=[r for r in rs if r['winner']];c=collections.Counter(r['receiverResult'] for r in rv)
    last={}
    for r in rs:last[(r['variant'],r['human'],r['level'],r['style'],r['match'])]=r
    mw=sum(int(r['matchHuman'])>int(r['matchRival']) for r in last.values())
    hc=sum(int(r['humanContacts']) for r in rs);vc=sum(int(r['humanVolleyContacts']) for r in rs)
    n=len(rv)
    return dict(nRivalServes=n,returnPV=c['Contact']/n if n else float('nan'),air=c['Air']/n if n else 0,late=c['Late']/n if n else 0,unreach=c['Unreachable']/n if n else 0,noswing=c['NoSwing']/n if n else 0,
        rivalReturn=sum(r['receiverResult']=='Contact' for r in hv)/len(hv) if hv else float('nan'),humanPoints=sum(r['winner']=='Human' for r in won)/len(won),
        humanMatches=mw/len(last),matches=len(last),volleyShare=vc/hc if hc else 0)
keys=['nRivalServes','returnPV','air','late','unreach','noswing','rivalReturn','humanPoints','humanMatches','matches','volleyShare']
def fmt(d):return '\t'.join(f"{d[k]*100:.1f}" if isinstance(d[k],float) else str(d[k]) for k in keys)
print('group\tvariant\thuman\tlevel\tstyle\tserve\t'+'\t'.join(keys))
g=collections.defaultdict(list)
for r in rows:g[(r['variant'],r['human'],r['level'])].append(r)
order=['base','driveHalf','driveZero','earlySoft','noServeVolley']
for k in sorted(g,key=lambda k:(k[1],k[2],order.index(k[0]))):
    print('ALL\t'+'\t'.join(k)+'\t*\t*\t'+fmt(agg(g[k])))
for k in sorted(g,key=lambda k:(k[1],k[2],order.index(k[0]))):
    for st in ('Fondista','Definidor','Completo'):
        rs=[r for r in g[k] if r['style']==st];print('STYLE\t'+'\t'.join(k)+f'\t{st}\t*\t'+fmt(agg(rs)))
    for sv in ('Lob','Drive'):
        rs=[r for r in g[k] if r['server']=='Human' or r['serveType']==sv]
        rs=[r for r in rs if r['server']=='Rival'];
        if rs:d=agg(rs);print('SERVE\t'+'\t'.join(k)+f'\t*\t{sv}\t'+fmt(d))
