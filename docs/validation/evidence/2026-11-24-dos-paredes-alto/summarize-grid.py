# Resumen de c-grid.tsv (experimento TwoWallHighDiagnosisExperiment, 24 nov 2026). Uso: python3 -I summarize-grid.py c-grid.tsv
import csv,statistics as st,collections,sys
rows=list(csv.reader(open(sys.argv[1]),delimiter='\t'));R=rows[1:]
# 0cx 1cy 2cz 3sideZ 4tapY 5power 6pace 7solved 8reason 9usedY 10seq 11fsideZ 12fsideY 13frontX 14frontY 15apex 16bX 17bZ ...
S=[r for r in R if r[7]=='True']
def summ(rs):
    ys=[float(r[14]) for r in rs]; bz=[float(r[17]) for r in rs]; d=[float(r[14])-float(r[12]) for r in rs]
    return 'n=%3d frontis mediana %.2f m | >1,78 m %3.0f%% | >2,5 m %3.0f%% | subida frontis-lateral mediana %+.2f m | bote z mediana %.2f m | bote pasada media cancha %3.0f%%'%(
        len(rs),st.median(ys),100*sum(y>1.78 for y in ys)/len(ys),100*sum(y>2.5 for y in ys)/len(ys),st.median(d),st.median(bz),100*sum(z>4.875 for z in bz)/len(bz))
print('filas',len(R),'resueltas',len(S),'exactas',sum(r[8]=='boast' for r in S))
print('TODAS',summ(S))
for k,name in ((4,'altura tocada'),(5,'potencia')):
    g=collections.defaultdict(list)
    for r in S: g[r[k]].append(r)
    for v in sorted(g,key=float): print(name,v,summ(g[v]))
g=collections.defaultdict(list)
for r in S: g[round(float(r[2])-float(r[3]),1)].append(r)
for v in sorted(g): print('punto lateral delante del contacto',v,'m',summ(g[v]))
for tp in ['0.9','1.5','2.1']:
    for pw in ['0.3','0.5','0.75','1']:
        print('tocada',tp,'P',pw,summ([r for r in S if r[4]==tp and r[5]==pw]))
