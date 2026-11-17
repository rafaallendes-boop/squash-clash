#!/usr/bin/env python3
"""Comprueba sin editar que los tests heredados solo tienen los cambios autorizados."""
from pathlib import Path
import json,re,subprocess
root=Path(__file__).resolve().parent.parent
files=subprocess.check_output(['git','diff','--name-only','766dedd','--','Assets/SquashClash/Tests'],cwd=root,text=True).splitlines()
maphash={'2ec0c953786a':'430da2faa818','3262f9b89156':'430da2faa818','e8df60f37d4b':'06fed4bd4ac9','95a3f24313fe':'8e22575422f2','750c407c8a2f':'e89daab41c3e','f92f406f698a':'dcc7c8cc0688'}
keys=['armSpeedMax','costPerMeter','enabled','legsMax','onset','positionGainMax','reactionMaxMs','recoverBetween','recoverPerS','strongCost','strongFrom']
summary=[]
for name in files:
    if name.endswith('.meta') or any(x in name for x in ['Experiments/','/Fatigue','/ZFatigue']):continue
    old=subprocess.check_output(['git','show','766dedd:'+name],cwd=root,text=True)
    now=(root/name).read_text()
    normalized=now.replace('app.Tuning.Set("fatigue.enabled",0);','')
    hashes={}
    for new,previous in maphash.items():
        if '"'+new+'"' in normalized:
            hashes[previous]=new;normalized=normalized.replace('"'+new+'"','"'+previous+'"')
    normalized=normalized.replace('Is.EqualTo(132)','Is.EqualTo(121)')
    for k in keys:normalized=normalized.replace('"fatigue.'+k+'",','')
    normalized=normalized.replace('RallyFixtures.ContextWithoutFatigue()','RallyFixtures.Context()')
    if name.endswith('RallyFixtures.cs'):
        normalized=re.sub(r'        internal static RallyContext ContextWithoutFatigue\(\)\n[^\n]+\n','',normalized)
    assert normalized==old,'Unexpected existing-test change: '+name
    summary.append({'path':name,'hashLiterals':hashes,'count121to132':'Is.EqualTo(132)' in now and 'Is.EqualTo(121)' in old,'addedFatigueKeyList':name.endswith('S3FeedbackConfigTests.cs'),'explicitFatigueDisabledFixture':'app.Tuning.Set("fatigue.enabled",0);' in now or 'ContextWithoutFatigue' in now,'allOtherContentIdentical':True})
p=root/'docs/validation/evidence/2026-11-17-f1-fatigue/fixture-values.json'
p.write_text(json.dumps({'schema':'f1-fixture-values/1','base':'766dedd','initialLiteralCommit':'f2b840f','adjustedLiteralCommit':'5b1329d','files':summary},indent=2)+'\n')
print('Existing tests: only approved literals/key-list and explicit fatigue-disabled fixtures; '+str(len(summary))+' files verified')
