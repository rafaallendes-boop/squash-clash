# Summary of ServeReceptionOptionsExperiment points.tsv files: per variant/human/level (and style, serve type) the newcomer's
# P+V return of the rival's serve and its failure mix, the rival's return of the human's serve, the human point and match share and
# the human volley share. Every rate is published with its numerator and denominator (k/n); percentages keep four decimals.
import csv,sys,collections
rows=[r for f in sys.argv[1:] for r in csv.DictReader(open(f),delimiter='\t')]
def rate(k,n):return (k,n,f"{100*k/n:.4f}" if n else "nan")
def agg(rs):
    rv=[r for r in rs if r['server']=='Rival'];hv=[r for r in rs if r['server']=='Human']
    won=[r for r in rs if r['winner']];c=collections.Counter(r['receiverResult'] for r in rv)
    last={}
    for r in rs:last[(r['variant'],r['human'],r['level'],r['style'],r['match'])]=r
    mw=sum(int(r['matchHuman'])>int(r['matchRival']) for r in last.values())
    hc=sum(int(r['humanContacts']) for r in rs);vc=sum(int(r['humanVolleyContacts']) for r in rs);n=len(rv)
    return [rate(c['Contact'],n),rate(c['Air'],n),rate(c['Late'],n),rate(c['Unreachable'],n),rate(c['NoSwing'],n),
        rate(sum(r['receiverResult']=='Contact' for r in hv),len(hv)),rate(sum(r['winner']=='Human' for r in won),len(won)),
        rate(mw,len(last)),rate(vc,hc)]
names=['returnPV','air','late','unreach','noswing','rivalReturn','humanPoints','humanMatches','volleyShare']
print('group\tvariant\thuman\tlevel\tstyle\tserve\t'+'\t'.join(f"{m}K\t{m}N\t{m}Pct" for m in names))
def line(tag,k,style,serve,rs):print(f"{tag}\t"+'\t'.join(k)+f"\t{style}\t{serve}\t"+'\t'.join(f"{a}\t{b}\t{p}" for a,b,p in agg(rs)))
g=collections.defaultdict(list)
for r in rows:g[(r['variant'],r['human'],r['level'])].append(r)
order=['base','driveHalf','driveZero','earlySoft','noServeVolley']
keys=sorted(g,key=lambda k:(k[1],k[2],order.index(k[0])))
for k in keys:line('ALL',k,'*','*',g[k])
for k in keys:
    for st in ('Fondista','Definidor','Completo'):line('STYLE',k,st,'*',[r for r in g[k] if r['style']==st])
    # Rival-serve points of one serve kind only: the human-serve, point and match columns of these rows are not meaningful.
    for sv in ('Lob','Drive'):
        rs=[r for r in g[k] if r['server']=='Rival' and r['serveType']==sv]
        if rs:line('SERVE',k,'*',sv,rs)
