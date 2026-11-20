import csv, sys, statistics as st
lines = [l for l in open(sys.argv[1]) if not l.startswith('#')]
rows = list(csv.DictReader(lines, delimiter='\t'))
def pct(a, b): return round(100*a/b, 1) if b else float('nan')
for group, sel in [('non-serve', '0'), ('serve', '1')]:
    g = [r for r in rows if r['serve'] == sel]; n = len(g)
    bpv = [r for r in g if r['bouncePV'] in ('0', '1')]
    bounce = pct(sum(r['bouncePV'] == '1' for r in bpv), len(bpv))
    off = [r for r in g if r['offered'] == '1']
    leads = sorted(float(r['bounceLeadAfterSMs']) for r in off if r['bounceLeadAfterSMs'] != 'NaN')
    print(f"{group}\treceptions\t{n}\tbouncePV%\t{bounce}\toffered%\t{pct(len(off), n)}\tbounceLeadAfterS median\t{st.median(leads) if leads else 'NaN'}\tp10\t{leads[len(leads)//10] if leads else 'NaN'}")
    from collections import Counter
    print(group, 'reasons', dict(Counter(r['reason'] for r in g).most_common()))
    for case in ['150', '300', 'Po']:
        com = [r for r in g if r['c'+case] == '1']
        pv = pct(sum(r['pv'+case] == '1' for r in com), len(com))
        same = [r for r in com if r['bouncePV'] in ('0', '1')]
        bsame = pct(sum(r['bouncePV'] == '1' for r in same), len(same))
        print(f"{group}\tcase {case}\tvolleyRate%\t{pct(len(com), n)}\teffectiveOffer%(of offered)\t{pct(len(com), len(off))}\tvolleyPV%\t{pv}\tbouncePV same receptions%\t{bsame}\tgap pp\t{round(bsame-pv,1) if com else 'NaN'}")
