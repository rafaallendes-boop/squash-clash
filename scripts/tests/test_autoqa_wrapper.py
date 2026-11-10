#!/usr/bin/env python3
"""Deterministic wrapper tests. Fixtures simulate process/files, never game results."""
import copy
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = Path(os.environ.get('AUTOQA_WRAPPER_TEST_OUTPUT', ROOT / 'Logs/autoqa/wrapper-tests'))
OUTPUT.mkdir(parents=True, exist_ok=True)
SESSION = Path(tempfile.mkdtemp(prefix='run-', dir=OUTPUT))

FAKE_UNITY = r'''#!/usr/bin/env python3
import datetime,hashlib,json,os,sys,time
from pathlib import Path
root=Path.cwd();run=Path(os.environ['AUTOQA_RUN_DIR']);case=json.loads(os.environ['AUTOQA_FIXTURE_CASE'])
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
now=datetime.datetime.now(datetime.timezone.utc).isoformat()
s=case['harness'];s.update(runId=run.name,gitHash=os.environ['AUTOQA_GIT_HASH'],dirty=os.environ['AUTOQA_DIRTY']=='true',startedAt=now,finishedAt=now)
rows=[{'schema':'autoqa-attempt/1','scenario':{'id':'synthetic-'+str(i),'repetition':1,'canary':False},'verdict':'PASS'} for i in range(18)]
rows.append({'schema':'autoqa-attempt/1','scenario':{'id':'synthetic-canary','repetition':1,'canary':True},'verdict':s['canary']['observed']})
if case['trigger']=='attempt-schema':rows[0]['schema']='invalid-attempt-schema'
(run/'attempts.jsonl').write_text(''.join(json.dumps(x)+'\n' for x in rows))
(run/'report.md').write_text('# Synthetic harness fixture\n\ngate: '+s['gate']+'\n')
s['artifacts']={'attemptsSha256':sha(run/'attempts.jsonl'),'reportSha256':sha(run/'report.md'),'gameMetrics':[]}
if case['trigger']=='artifact-path':s['artifacts']['gameMetrics']=[{'path':'../outside.jsonl','sha256':'0'*64}]
if case['trigger']=='summary-schema':s.pop('command',None)
for key in case.get('drop_integrity',[]):s['integrity'].pop(key,None)
if case['malformed']=='corrupt':(run/'summary.json').write_text('{broken-json')
elif case['malformed']!='missing':(run/'summary.json').write_text(json.dumps(s)+'\n')
if case['trigger']=='missing-summary' and (run/'summary.json').exists():os.utime(run/'summary.json',(1,1))
if '--prepare' in sys.argv:sys.exit(0)
trigger=case['trigger'];bad=trigger=='tests'
name='Synthetic.AutoQAGuardTests.Contract' if case['guard'] else 'Synthetic.NonGuard.Test'
if case['guard']:bad=True
xml='<test-run total="1" passed="%s" failed="%s" skipped="0"><test-case fullname="%s" result="%s"/></test-run>' % ('0' if bad else '1','1' if bad else '0',name,'Failed' if bad else 'Passed')
if trigger!='missing-xml':(run/'tests.xml').write_text(xml)
(run/'unity.log').write_text('error CS1234 synthetic compilation failure' if trigger=='compilation' else 'Synthetic process fixture; Unity was not invoked.\n')
for path in ['ProjectSettings/GraphicsSettings.asset','ProjectSettings/QualitySettings.asset']:(root/path).write_text('synthetic transient\n')
if case['dirty_after']:(root/'unrelated.txt').write_text('synthetic unrelated change\n')
'''

class WrapperTests(unittest.TestCase):
    def harness(self, **overrides):
        data = {'schema': 'autoqa-summary/1', 'command': 'scripts/autoqa.sh mvp', 'gate': 'PASS',
                'counts': {'PASS': 18, 'FAIL': 0, 'INCONCLUSIVE': 0, 'NOT_EVALUATED': 0},
                'canary': {'expectedFail': True, 'observed': 'FAIL'},
                'determinism': {'synthetic': {'identical': True}},
                'integrity': {k: True for k in ['hashesVerified','presetsUnchanged','guardPassed','evidenceSha256Ok']},
                'executionError': None, 'escalationLevel': 1, 'exitCode': 0, 'escalationReasons': [],
                'syntheticFixture': True}
        for key, value in overrides.items():
            if isinstance(value, dict) and isinstance(data.get(key), dict): data[key].update(copy.deepcopy(value))
            else: data[key] = copy.deepcopy(value)
        return data

    def invoke(self, harness=None, trigger='tests', malformed=None, dirty_after=False, guard=False, drop_integrity=()):
        fixture = Path(tempfile.mkdtemp(prefix=self._testMethodName+'-', dir=SESSION))
        (fixture/'scripts').mkdir(); (fixture/'ProjectSettings').mkdir()
        shutil.copyfile(ROOT/'scripts/autoqa.sh', fixture/'scripts/autoqa.sh')
        (fixture/'scripts/fake-unity').write_text(FAKE_UNITY);(fixture/'scripts/fake-unity').chmod(0o755)
        (fixture/'scripts/unity-env.sh').write_text('UNITY="$ROOT/scripts/fake-unity"\nexport UNITY\nif [[ "$AUTOQA_FIXTURE_ENV_ERROR" == 1 ]]; then "$UNITY" --prepare; return 1; fi\n')
        for name in ['GraphicsSettings.asset','QualitySettings.asset']:(fixture/'ProjectSettings'/name).write_text('original fixture\n')
        (fixture/'unrelated.txt').write_text('original unrelated\n');(fixture/'.gitignore').write_text('Logs/\n')
        subprocess.run(['git','init','-qb','codex/wrapper-tests'],cwd=fixture,check=True,capture_output=True)
        subprocess.run(['git','add','.'],cwd=fixture,check=True,capture_output=True)
        subprocess.run(['git','-c','user.name=AutoQA Fixture','-c','user.email=autoqa@example.invalid','commit','-qm','Initialize wrapper fixture'],cwd=fixture,check=True,capture_output=True)
        data=copy.deepcopy(harness if harness is not None else self.harness())
        case={'harness':data,'trigger':trigger,'malformed':malformed,'dirty_after':dirty_after,'guard':guard,'drop_integrity':list(drop_integrity)}
        env=dict(os.environ,AUTOQA_FIXTURE_CASE=json.dumps(case),AUTOQA_FIXTURE_ENV_ERROR='1' if trigger=='environment' else '0')
        result=subprocess.run(['bash','scripts/autoqa.sh','mvp'],cwd=fixture,env=env,capture_output=True,text=True,timeout=30)
        (fixture/'Logs/console.txt').write_text(result.stdout+'\nSTDERR\n'+result.stderr)
        paths=[line.split('=',1)[1] for line in result.stdout.splitlines() if line.startswith('RUN_DIR=')]
        self.assertEqual(len(paths),1,result.stdout+result.stderr)
        run=Path(paths[0]);summary=json.loads((run/'summary.json').read_text())
        record={'test':self.id(),'fixture':str(fixture.relative_to(SESSION)),'trigger':trigger,'exit':result.returncode,'level':summary['escalationLevel'],'gate':summary['gate'],'kind':summary.get('executionError',{}).get('kind') if summary.get('executionError') else None}
        with (SESSION/'cases.jsonl').open('a') as stream:stream.write(json.dumps(record)+'\n')
        self.assertEqual(result.returncode,summary['exitCode'])
        for key in ['harnessSummary','harnessReport','previousErrorSummary','previousErrorReport','restoration']:
            if key in summary['artifacts']:
                ref=summary['artifacts'][key];self.assertEqual(hashlib.sha256((run/ref['path']).read_bytes()).hexdigest(),ref['sha256'])
        return summary,result,run

    def assert_error(self, summary, result, kind, level=3, exit_code=3):
        self.assertEqual(summary['gate'],'ERROR');self.assertEqual(summary['executionError']['kind'],kind)
        self.assertEqual(summary['escalationLevel'],level);self.assertEqual(result.returncode,exit_code)

    def test_canary_pass_survives_tests_error(self):
        h=self.harness(canary={'observed':'PASS'},gate='FAIL',escalationLevel=3,exitCode=3,escalationReasons=['canary-not-failed'])
        s,p,r=self.invoke(h);self.assert_error(s,p,'tests');self.assertEqual(s['canary']['observed'],'PASS')
        self.assertIn('canary-not-failed',s['escalationReasons']);self.assertIn('exitCode: 3',(r/'report.md').read_text())

    def test_presets_failure_survives_tests_error(self):
        h=self.harness(integrity={'presetsUnchanged':False},gate='FAIL',escalationLevel=3,exitCode=3,escalationReasons=['presets-altered'])
        s,p,_=self.invoke(h);self.assert_error(s,p,'tests');self.assertIs(s['integrity']['presetsUnchanged'],False)
        self.assertIn('presets-altered',s['escalationReasons'])

    def test_fail_then_environment_uses_four_without_integrity(self):
        h=self.harness(gate='FAIL',counts={'PASS':17,'FAIL':1},escalationLevel=2,exitCode=1,escalationReasons=['non-pass-attempts'])
        s,p,_=self.invoke(h,trigger='environment');self.assert_error(s,p,'environment',2,4)
        self.assertFalse(any(v is False for v in s['integrity'].values()));self.assertEqual(s['canary']['observed'],'FAIL')

    def test_integrity_then_dirty_after_run_uses_three(self):
        h=self.harness(integrity={'hashesVerified':False},gate='FAIL',escalationLevel=3,exitCode=3,escalationReasons=['hash-mismatch'])
        s,p,_=self.invoke(h,trigger='ok',dirty_after=True);self.assert_error(s,p,'dirty-after-run')
        self.assertIs(s['integrity']['hashesVerified'],False);self.assertIn('hash-mismatch',s['escalationReasons'])

    def test_pass_then_dirty_after_run_uses_four(self):
        s,p,_=self.invoke(trigger='ok',dirty_after=True);self.assert_error(s,p,'dirty-after-run',2,4)
        self.assertEqual(s['canary']['observed'],'FAIL');self.assertTrue(all(v is True for v in s['integrity'].values()))

    def test_corrupt_harness_is_unknown(self):
        s,p,_=self.invoke(malformed='corrupt');self.assert_error(s,p,'tests',2,4)
        self.assertTrue(all(v is None for v in s['integrity'].values()));self.assertIsNone(s['canary']['observed'])

    def test_two_failures_keep_harness_and_previous_chain(self):
        h=self.harness(canary={'observed':'PASS'},gate='ERROR',escalationLevel=3,exitCode=3,escalationReasons=['canary-not-failed'],executionError={'kind':'harness-error','message':'synthetic'})
        s,p,r=self.invoke(h,dirty_after=True);self.assert_error(s,p,'dirty-after-run')
        self.assertEqual(s['executionError']['previous']['kind'],'tests')
        self.assertEqual(s['executionError']['previous']['previous']['kind'],'harness-error')
        self.assertEqual(s['canary']['observed'],'PASS');self.assertIn('canary-not-failed',s['escalationReasons'])
        self.assertEqual(json.loads((r/'summary.harness.json').read_text())['canary']['observed'],'PASS')

    def test_two_wrapper_errors_without_harness_keep_their_own_evidence(self):
        s,p,r=self.invoke(trigger='missing-xml',malformed='missing',dirty_after=True)
        self.assert_error(s,p,'dirty-after-run',2,4)
        self.assertEqual(s['executionError']['previous']['kind'],'missing-xml')
        self.assertIsNone(s['harnessResults'])
        for name in ['summary.harness.json','report.harness.md']:
            self.assertFalse((r/name).exists(),name+' must contain original harness evidence only')
        self.assertNotIn('harnessSummary',s['artifacts']);self.assertNotIn('harnessReport',s['artifacts'])
        prior=json.loads((r/s['artifacts']['previousErrorSummary']['path']).read_text())
        self.assertEqual(prior['executionError']['kind'],'missing-xml')
        self.assertIn('missing-xml',(r/s['artifacts']['previousErrorReport']['path']).read_text())

    def test_all_error_paths_preserve_demonstrated_integrity(self):
        for kind in ['tests','compilation','summary-schema','attempt-schema','artifact-path','incomplete-run','missing-summary']:
            with self.subTest(kind=kind):
                h=self.harness(integrity={'evidenceSha256Ok':False},gate='FAIL',escalationLevel=3,exitCode=3,escalationReasons=['evidence-bad'])
                if kind=='incomplete-run':h['exitCode']=0;h['counts']['PASS']=17
                s,p,_=self.invoke(h,trigger=kind);self.assert_error(s,p,kind)
                self.assertIs(s['integrity']['evidenceSha256Ok'],False)

    def test_explicit_false_is_detected_even_without_declared_level_three(self):
        for key in ['hashesVerified','presetsUnchanged','guardPassed','evidenceSha256Ok']:
            with self.subTest(key=key):
                s,p,_=self.invoke(self.harness(integrity={key:False}));self.assert_error(s,p,'tests')
                self.assertIs(s['integrity'][key],False)

    def test_known_and_unknown_integrities_stay_known_or_unknown(self):
        s,p,_=self.invoke(self.harness(),drop_integrity=['hashesVerified','presetsUnchanged'])
        self.assert_error(s,p,'tests',2,4);self.assertIsNone(s['integrity']['hashesVerified']);self.assertIsNone(s['integrity']['presetsUnchanged'])
        self.assertIs(s['integrity']['guardPassed'],True);self.assertIs(s['integrity']['evidenceSha256Ok'],True)

    def test_unknown_values_are_not_truthy_integrity(self):
        h=self.harness(integrity={k:'unverified' for k in ['hashesVerified','presetsUnchanged','guardPassed','evidenceSha256Ok']})
        s,p,_=self.invoke(h);self.assert_error(s,p,'tests',2,4);self.assertTrue(all(v is None for v in s['integrity'].values()))

    def test_wrong_schema_does_not_import_signals(self):
        h=self.harness(schema='not-autoqa',canary={'observed':'PASS'},integrity={'hashesVerified':False},escalationLevel=3,exitCode=3)
        s,p,_=self.invoke(h);self.assert_error(s,p,'tests',2,4);self.assertTrue(all(v is None for v in s['integrity'].values()))
        self.assertIsNone(s['canary']['observed'])

    def test_declared_harness_level_and_exit_three_are_not_lowered(self):
        for field in ['escalationLevel','exitCode']:
            with self.subTest(field=field):
                s,p,_=self.invoke(self.harness(**{field:3},escalationReasons=['declared-level-three']))
                self.assert_error(s,p,'tests');self.assertIn('declared-level-three',s['escalationReasons'])

    def test_counts_and_determinism_are_explicitly_harness_results(self):
        h=self.harness(gate='FAIL',counts={'PASS':17,'FAIL':1},determinism={'synthetic':{'identical':False}},escalationLevel=2,exitCode=1)
        s,p,_=self.invoke(h);self.assert_error(s,p,'tests',2,4)
        self.assertEqual(s['counts'],{k:0 for k in ['PASS','FAIL','INCONCLUSIVE','NOT_EVALUATED']});self.assertEqual(s['determinism'],{})
        self.assertIn('harnessResults',s);self.assertEqual(s['harnessResults']['source'],'summary.harness.json')
        self.assertEqual(s['harnessResults']['counts'],h['counts']);self.assertEqual(s['harnessResults']['determinism'],h['determinism'])

    def test_guard_failure_keeps_fail_gate_and_dominates_known_true(self):
        s,p,_=self.invoke(guard=True);self.assertEqual(s['gate'],'FAIL');self.assertEqual((p.returncode,s['escalationLevel']),(3,3))
        self.assertIs(s['integrity']['guardPassed'],False)

    def test_no_harness_remains_unknown(self):
        s,p,_=self.invoke(malformed='missing');self.assert_error(s,p,'tests',2,4)
        self.assertTrue(all(v is None for v in s['integrity'].values()));self.assertIsNone(s['canary']['observed'])

    def test_existing_success_and_failure_paths_keep_their_triples(self):
        cases=[self.harness(),self.harness(gate='FAIL',counts={'PASS':17,'FAIL':1},escalationLevel=2,exitCode=1),
               self.harness(gate='INCONCLUSIVE',counts={'PASS':17,'INCONCLUSIVE':1},escalationLevel=2,exitCode=2),
               self.harness(gate='FAIL',canary={'observed':'PASS'},escalationLevel=3,exitCode=3),
               self.harness(gate='FAIL',integrity={'hashesVerified':False},escalationLevel=3,exitCode=3)]
        for h in cases:
            with self.subTest(gate=h['gate'],exit=h['exitCode']):
                s,p,_=self.invoke(h,trigger='ok');self.assertEqual((s['gate'],p.returncode,s['escalationLevel']),(h['gate'],h['exitCode'],h['escalationLevel']))
        s,p,_=self.invoke(trigger='missing-xml',malformed='missing');self.assert_error(s,p,'missing-xml',2,4)

if __name__ == '__main__':
    print('FIXTURE_DIR='+str(SESSION),flush=True)
    result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(WrapperTests))
    (SESSION/'test-result.json').write_text(json.dumps({'tests':result.testsRun,'failures':len(result.failures),'errors':len(result.errors),'success':result.wasSuccessful()},indent=2)+'\n')
    sys.exit(0 if result.wasSuccessful() else 1)
