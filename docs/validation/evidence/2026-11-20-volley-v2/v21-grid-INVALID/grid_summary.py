import csv, sys, glob, os, statistics as st
def q(xs, p):
    xs = sorted(xs); return xs[min(len(xs)-1, int(p*len(xs)))] if xs else float('nan')
first_ok = None
for path in sorted(glob.glob(os.path.join(sys.argv[1], 'sprint-*.tsv'))):
    rows = list(csv.DictReader([l for l in open(path) if not l.startswith('#')], delimiter='\t'))
    name = os.path.basename(path)[7:-4]
    for group, sel in [('non-serve', '0'), ('serve', '1')]:
        g = [r for r in rows if r['serve'] == sel]; n = len(g)
        off = [r for r in g if r['offered'] == '1']
        com = [r for r in g if r['committed'] == '1']
        pv = sum(r['volleyPV'] == '1' for r in com)/len(com)*100 if com else float('nan')
        same = [r for r in com if r['bouncePV'] in ('0', '1')]
        b = sum(r['bouncePV'] == '1' for r in same)/len(same)*100 if same else float('nan')
        unc = [float(r['whiteLeadAfterSPrimeMs']) for r in off if r['committed'] == '0' and r['whiteLeadAfterSPrimeMs'] != 'NaN']
        lead_ok = sum(x >= 300 for x in unc)/len(unc)*100 if unc else float('nan')
        s = [float(r['launchToSMs']) for r in off]; sp = [float(r['launchToSPrimeMs']) for r in off]
        spr = [r for r in com if r['sprint'] == '1']
        late = [float(r['lateness']) for r in spr]
        rate = len(com)/n*100
        print(f"sprint {name}\t{group}\tn {n}\toffered% {len(off)/n*100:.1f}\tvolleyRate% {rate:.1f}\tsprintShare% {len(spr)/max(1,len(com))*100:.1f}\tvolleyPV% {pv:.1f}\tbouncePVsame% {b:.1f}\tgap {b-pv:.1f}\twhiteLead>=300% {lead_ok:.1f} (n {len(unc)})\tlaunch->S ms med {st.median(s) if s else 'NaN'} p10 {q(s,.1)} p90 {q(s,.9)}\tlaunch->S' ms med {st.median(sp) if sp else 'NaN'} p10 {q(sp,.1)} p90 {q(sp,.9)}\tlateness med {st.median(late) if late else 'NaN'}")
        if group == 'non-serve':
            ok = rate >= 40 and (b - pv) <= 12 and lead_ok >= 90
            print(f"sprint {name}\tTARGETS 1-3 {'MET' if ok else 'not met'}")
            if ok and first_ok is None: first_ok = name
print('FIRST_OK', first_ok)
