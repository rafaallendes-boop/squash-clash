#!/usr/bin/env python3
# Summary of FatigueFeelExperiment (design 2026-11-20-fatigue-feel, section 4). Usage: summarize.py <dir with receptions/points/matches.tsv>
import csv,sys,statistics as st
from collections import defaultdict
d=sys.argv[1]
def rows(n):
    with open(f"{d}/{n}") as f:return list(csv.DictReader(f,delimiter="\t"))
rec,pts,mat=rows("receptions.tsv"),rows("points.tsv"),rows("matches.tsv")
src={r["arm"]:r for r in rows("source.tsv")}
arms=["A-off","A-on","B32","B43"];bands=[">=70","40-70","10-40","<10"]
pct=lambda a,b:f"{100*a/b:.1f}" if b else "-"
out=[]
def p(*a):out.append("\t".join(str(x) for x in a))
for setname in ["fixed","match"]:
    p(f"## set={setname} human receptions (no serve return) by energy band")
    p("arm","band","n","volley%","sprint%","volleyP+V%","reachable%","slackMean","meanSprintFactor")
    for arm in arms:
        for b in bands:
            r=[x for x in rec if x["arm"]==arm and x["set"]==setname and x["who"]=="Human" and x["serveReturn"]=="0" and x["band"]==b]
            n=len(r);v=[x for x in r if x["volley"]=="1"];s=[x for x in v if float(x["sprint"])>1.0001]
            pv=[x for x in v if x["result"]=="Contact"]
            reach=sum(x["reachable"]=="1" for x in r);slack=[float(x["slackS"]) for x in r if x["reachable"]=="1"]
            p(arm,b,n,pct(len(v),n),pct(len(s),n),pct(len(pv),len(v)),pct(reach,n),f"{st.mean(slack):.3f}" if slack else "-",f"{st.mean(float(x['sprint']) for x in s):.3f}" if s else "-")
p("## target A (A-on, fixed): sprint% at <10 <= 1/3 of sprint% at >=70, n(<10)>=100")
def rate(arm,b):
    r=[x for x in rec if x["arm"]==arm and x["set"]=="fixed" and x["who"]=="Human" and x["serveReturn"]=="0" and x["band"]==b]
    return sum(x["volley"]=="1" and float(x["sprint"])>1.0001 for x in r)/len(r) if r else float("nan"),len(r)
hi,nhi=rate("A-on",">=70");lo,nlo=rate("A-on","<10")
p("A-on",f"sprint>=70={100*hi:.2f}% (n {nhi})",f"sprint<10={100*lo:.2f}% (n {nlo})","ratio",f"{lo/hi:.3f}" if hi else "-","verdict","NOT-EVALUABLE" if nlo<100 else ("PASS" if lo<=hi/3 else "FAIL"))
p("## set=fixed points won by the human by start energy (rival starts at 100)")
p("arm","start","points","humanWon%","meanShots","humanMeters/point","rivalMeters/point")
for arm in arms:
    for s in ["100","55","25","5"]:
        r=[x for x in pts if x["arm"]==arm and x["set"]=="fixed" and x["start"]==s]
        p(arm,s,len(r),pct(sum(x["winner"]=="Human" for x in r),len(r)),f"{st.mean(int(x['shots']) for x in r):.2f}",f"{st.mean(float(x['humanMeters']) for x in r):.1f}",f"{st.mean(float(x['rivalMeters']) for x in r):.1f}")
p("## set=match natural games (30 per style)")
p("arm","games","humanGames%","humanPoints%","h5med","h10med","h15med","r5med","r10med","r15med")
def med(v):
    v=[float(x) for x in v if x!="NaN"];return f"{st.median(v):.1f} (n {len(v)})" if v else "-"
for arm in arms:
    r=[x for x in mat if x["arm"]==arm]
    hp=sum(int(x["human"]) for x in r);rp=sum(int(x["rival"]) for x in r)
    p(arm,len(r),pct(sum(x["winner"]=="Human" for x in r),len(r)),pct(hp,hp+rp),*[med(x[k] for x in r) for k in ["h5","h10","h15","r5","r10","r15"]])
p("## set=match natural games by style: human points %")
for arm in arms:
    p(arm,*[f"{s}={pct(sum(int(x['human']) for x in mat if x['arm']==arm and x['style']==s),sum(int(x['human'])+int(x['rival']) for x in mat if x['arm']==arm and x['style']==s))}" for s in ["Fondista","Definidor","Completo"]])
p("## set=match all receptions by band and player (share of receptions in each band)")
for arm in arms:
    for who in ["Human","Rival"]:
        r=[x for x in rec if x["arm"]==arm and x["set"]=="match" and x["who"]==who]
        p(arm,who,len(r),*[f"{b}={pct(sum(x['band']==b for x in r),len(r))}" for b in bands],"reachable%="+pct(sum(x['reachable']=='1' for x in r),len(r)))
p("## theoretical top speed (m/s) and extra reaction (ms) at the band centre (f) for each arm; sprint for A-on")
centres={">=70":0,"40-70":(70-55)/70,"10-40":(70-25)/70,"<10":(70-5)/70}
for arm in arms:
    legs=float(src[arm]["legsMax"]);reac=float(src[arm]["reactionMaxMs"])
    p(arm,*[f"{b}: {4.8*(1-legs*f):.2f} m/s +{reac*f:.0f} ms sprint {4.8*(1-legs*f)*(2 if arm=='A-off' else 1+(1-f)):.2f}" for b,f in centres.items()])
print("\n".join(out))
