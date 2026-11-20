import csv, sys, statistics as st
from collections import Counter, defaultdict
path = sys.argv[1]
lines = [l for l in open(path) if not l.startswith('#')]
rows = list(csv.DictReader(lines, delimiter='\t'))
def f(x):
    try: return float(x)
    except: return float('nan')
def med(xs):
    xs = [x for x in xs if x == x]
    return st.median(xs) if xs else float('nan')
def pct(xs, q):
    xs = sorted(x for x in xs if x == x)
    if not xs: return float('nan')
    return xs[min(len(xs)-1, int(q*len(xs)))]
out = []
def p(*a): out.append('\t'.join(str(x) for x in a))
for group, sel in [('non-serve', lambda r: r['serve'] == '0'), ('serve', lambda r: r['serve'] == '1')]:
    g = [r for r in rows if sel(r)]
    n = len(g)
    p('#', group, 'receptions', n)
    c = Counter(r['v0Reason'] for r in g)
    for k, v in c.most_common(): p('v0reason', group, k, v, round(100*v/n, 1))
    sep = [f(r['gapMs']) for r in g if r['v0Reason'] == 'separation']
    if sep: p('separationGapMs', group, 'n', len(sep), 'median', round(med(sep)), 'p10', round(pct(sep, .1)), 'p90', round(pct(sep, .9)), 'max', round(max(sep)))
    npx = [f(r['noPassExcessM']) for r in g if r['v0Reason'] == 'no-pass']
    if npx:
        finite = [x for x in npx if x != float('inf')]
        p('noPassExcessM', group, 'n', len(npx), 'noLegalHeight', len(npx)-len(finite), 'median', round(med(finite), 2), 'le0.3', sum(1 for x in finite if x <= .3), 'le0.6', sum(1 for x in finite if x <= .6))
    bpv = [r['bouncePV'] for r in g if r['bouncePV'] in '01' and r['bouncePV']]
    brate = sum(1 for x in bpv if x == '1')/len(bpv) if bpv else float('nan')
    p('bounce', group, 'n', len(bpv), 'PV%', round(100*brate, 1), 'Kms median', round(med([f(r['kBounceMs']) for r in g])), 'lead median', round(med([f(r['leadBounceMs']) for r in g])))
    for v in ['v0', 'a0', 'a1', 'b']:
        off = [r for r in g if r[v+'Offered'] == '1']
        if not off: p(v, group, 'offered 0'); continue
        pv = [r for r in off if r[v+'PV'] == '1']
        leads = [f(r[v+'LeadMs']) for r in off]
        blead = [f(r['leadBounceMs']) for r in off]
        okLead = sum(1 for a, b in zip(leads, blead) if a >= 500 or (b == b and a >= b))
        row = [v, group, 'offered', len(off), 'rate%', round(100*len(off)/n, 1), 'PV%', round(100*len(pv)/len(off), 1),
               'Kms median', round(med([f(r[v+'KMs']) for r in off])), 'p10', round(pct([f(r[v+'KMs']) for r in off], .1)),
               'lead median', round(med(leads)), 'bounceLead median(same)', round(med(blead)), 'leadOK%', round(100*okLead/len(off), 1)]
        if v != 'b':
            row += ['bounceLeadAfterS median', round(med([f(r[v+'BounceLeadAfterSMs']) for r in off])), 'p10', round(pct([f(r[v+'BounceLeadAfterSMs']) for r in off], .1))]
            row += ['usedBounceInstead', sum(1 for r in off if r[v+'UsedVolley'] == '0')]
        p(*row)
print('\n'.join(out))
