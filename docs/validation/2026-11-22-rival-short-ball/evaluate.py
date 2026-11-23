import csv,json,sys,math
from collections import Counter
from fractions import Fraction
from pathlib import Path
GROUPS=('total','Fondista','Definidor','Completo')
def rows(path):
    with open(path,newline='') as f:return list(csv.DictReader(f,delimiter='\t'))
def fraction(a,b):return {'numerator':a,'denominator':b,'percent':100*a/b if b else None}
def samples(data):return [r for r in data if r['reason']=='stretched' and r['result']=='Contact']
def rate(data):return fraction(sum(r['won']=='1' for r in data),len(data))
def distribution(data):
    values=sorted(float(r['ret_fw']) for r in data if r['ret_fw'] not in ('NA','NaN',''))
    def q(p):
        if not values:return None
        x=(len(values)-1)*p;i=int(x);return values[i]+(values[min(i+1,len(values)-1)]-values[i])*(x-i)
    return {'n':len(values),'missing':len(data)-len(values),'min':q(0),'p25':q(.25),'p50':q(.5),'p75':q(.75),'p95':q(.95),'max':q(1)}
mode=sys.argv[1]
if mode=='f1':
    data=rows(sys.argv[2]);assert len(data)==9
    med={r['style']:Fraction(int(r['backBoasts']),int(r['backStrokes'])) for r in data if r['level']=='Medium'}
    passed=med['Fondista']<med['Definidor'] and Fraction(5,100)<=med['Definidor']<=Fraction(15,100) and med['Fondista']<=Fraction(5,100) and med['Fondista']<=med['Completo']<=med['Definidor']
    out={'F1':passed,'cells':[{**r,'share':fraction(int(r['backBoasts']),int(r['backStrokes']))} for r in data]}
else:
    base=samples(rows(sys.argv[2]));raw=rows(sys.argv[3]);candidate=samples(raw);points=rows(sys.argv[4])
    assert all(r['fb'] in ('0','1') for r in candidate),'Missing candidate fallback flag'
    groups={};passed=True;e1=True
    for k in GROUPS:
        b=[r for r in base if k=='total' or r['style']==k];c=[r for r in candidate if k=='total' or r['style']==k]
        pts=[r for r in points if r['winner']=='Rival' and (k=='total' or r['style']==k)]
        br,cr=rate(b),rate(c);ok=bool(b and c) and cr['numerator']*br['denominator']<=br['numerator']*cr['denominator'];passed &=ok
        f=sum(r['fb']=='1' for r in c);fw=sum(r['fb']=='1' and r['won']=='1' for r in c)
        eok=bool(c and pts) and 100*f<=5*len(c) and 100*fw<=5*len(pts);e1 &=eok
        groups[k]={'base':br,'candidate':cr,'R8_pass':ok,'E1_stretched':fraction(f,len(c)),'E1_rival_points':fraction(fw,len(pts)),'E1_pass':eok}
    detail={}
    for name,data in (('base',base),('candidate',candidate)):
        detail[name]={
            'level':{k:rate([r for r in data if r['level']==k]) for k in ('Easy','Medium','Hard')},
            'zone':{k:rate([r for r in data if (float(r['r_z'])>=7.31)==(k=='back')]) for k in ('front_middle','back')},
            'boasts':sum(r['line']=='Boast' for r in data),
            'ret_fw':{k:distribution([r for r in data if (float(r['r_z'])>=7.31)==(k=='back')]) for k in ('front_middle','back')},
        }
    out={'F2':passed,'E1':e1,'groups':groups,'detail':detail,'candidate_results':dict(Counter(r['result'] for r in raw)),'points':len(points)}
print(json.dumps(out,indent=2))
sys.exit(0 if out.get('F1',out.get('F2') and out.get('E1')) else 1)
