#!/usr/bin/env python3
"""Checker contracts; no Unity. Optional AUTOQA_REAL_RUNS adds copied real controls."""
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import unittest

import test_autoqa_wrapper as wrapper

ROOT = wrapper.ROOT
OUTPUT = ROOT / 'Logs/autoqa/checker-tests'
OUTPUT.mkdir(parents=True, exist_ok=True)
SESSION = Path(tempfile.mkdtemp(prefix='run-', dir=OUTPUT))
TEMPLATE = ROOT / 'docs/autoqa/operator-summary-template.md'
SCENARIOS = ('M24-NOPULL-SL1', 'M24-REJECTED-SL1', 'M24-VALID-EARLY-SL1',
             'M24-FAILED-LATE-SL1', 'M24-PERFECT-EARLY10-SL5', 'M24-PERFECT-LATE10-SL5')
REAL_RUNS = {'pass': '20261109T142141Z-7cd5a18-12190', 'fail': '20261109T120557Z-491e10d-4834',
             'canary': '20261109T120528Z-491e10d-22213', 'hash': '20261109T120612Z-491e10d-25404',
             'environment': '20261109T120626Z-491e10d-330'}


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def dump(path, value):
    path.write_text(json.dumps(value, indent=2)+'\n')


class CheckerTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.checker = SESSION / 'autoqa-check.py'
        source = ROOT / '.claude/agents/autoqa-operator.md'
        version = re.search(r'^# >>> autoqa-check (v\d+)$', source.read_text(), re.M).group(1)
        # Same extraction command documented for the operator; no imports from the checker.
        awk = '/^# >>> autoqa-check '+version+'$/{f=1;next} /^# <<< autoqa-check '+version+'$/{f=0} f'
        cls.checker.write_text(subprocess.run(['awk', awk, str(source)], check=True,
                                             capture_output=True, text=True).stdout)
        if os.environ.get('AUTOQA_CHECKER'):
            shutil.copyfile(os.environ['AUTOQA_CHECKER'], cls.checker)
        cls.factory = wrapper.WrapperTests(methodName='runTest')

    def check_run(self, run):
        summary = json.loads((run/'summary.json').read_text())
        proc = subprocess.run(['python3', '-I', str(self.checker), '--run-dir', str(run),
                               '--mode', 'read', '--exit-code', str(summary['exitCode']),
                               '--template', str(TEMPLATE)], capture_output=True, text=True, timeout=30)
        blocks = re.findall(r'```json autoqa-operator-result\n(.*?)\n```', proc.stdout, re.S)
        self.assertEqual(len(blocks), 1, proc.stdout+proc.stderr)
        result = json.loads(blocks[0])
        (SESSION/(run.name+'-operator.md')).write_text(proc.stdout+proc.stderr)
        with (SESSION/'results.jsonl').open('a') as stream:
            stream.write(json.dumps({'test': self.id(), 'run': str(run), 'result': result})+'\n')
        return result

    def assert_classification(self, result, exit_code, level, gate_pass=False):
        self.assertEqual(result['expectedExitCode'], exit_code)
        self.assertEqual(result['operatorLevel'], level)
        self.assertIs(result['gatePass'], gate_pass)

    def test_wrong_harness_schema_does_not_supply_signals(self):
        h = self.factory.harness(schema='not-autoqa', canary={'observed': 'PASS'},
                                 integrity={'hashesVerified': False}, escalationLevel=3, exitCode=3)
        _, _, run = self.factory.invoke(h)
        result = self.check_run(run)
        self.assert_classification(result, 4, 3)
        self.assertTrue(any('summary.harness.json con schema inesperado' in x for x in result['incoherences']))

    def test_harness_level_three_preserves_exit_three(self):
        _, _, run = self.factory.invoke(self.factory.harness(escalationLevel=3))
        self.assert_classification(self.check_run(run), 3, 3)

    def test_harness_exit_three_preserves_exit_three(self):
        _, _, run = self.factory.invoke(self.factory.harness(exitCode=3))
        self.assert_classification(self.check_run(run), 3, 3)

    def test_previous_wrapper_artifacts_are_verified_without_harness_signals(self):
        _, _, original = self.factory.invoke(trigger='missing-xml', malformed='missing', dirty_after=True)
        for key in ('previousErrorSummary', 'previousErrorReport'):
            with self.subTest(artifact=key):
                run = SESSION/key
                shutil.copytree(original, run)
                summary = json.loads((run/'summary.json').read_text())
                ref = summary['artifacts'][key]
                # A previous wrapper error is opaque evidence, never another harness.
                if key == 'previousErrorSummary':
                    dump(run/ref['path'], {'schema': 'autoqa-summary/1', 'exitCode': 3,
                                          'escalationLevel': 3, 'canary': {'observed': 'PASS'}})
                    ref['sha256'] = sha(run/ref['path']);dump(run/'summary.json', summary)
                result = self.check_run(run)
                self.assert_classification(result, 4, 3)  # Minimal wrapper attempts are intentionally incomplete.
                self.assertIsNone(result['harnessSummary'])
                (run/ref['path']).write_text('tampered\n')
                result = self.check_run(run)
                self.assert_classification(result, 3, 3)
                self.assertTrue(any(key in x for x in result['incoherences']))

    def contract_fixture(self, kind):
        """Complete checker-only control; never presented as a real game run."""
        run = Path(tempfile.mkdtemp(prefix=kind+'-', dir=SESSION))
        summary = self.factory.harness()
        summary.update(runId=run.name, gitHash='fixture', dirty=False, startedAt='t0', finishedAt='t1')
        summary['determinism'] = {sid: {'identical': True} for sid in SCENARIOS}
        rows = []
        for sid in SCENARIOS+('CANARY-WRONG-EXPECTATION',):
            for rep in ((1,) if sid.startswith('CANARY') else (1, 2, 3)):
                anchor = '55a85f500cf1' if sid.endswith('SL5') or sid.startswith('CANARY') else 'c0d8ec7050e0'
                rows.append({'schema': 'autoqa-attempt/1', 'runId': run.name,
                             'scenario': {'id': sid, 'repetition': rep},
                             'verdict': 'FAIL' if sid.startswith('CANARY') else 'PASS',
                             'config': {'effectiveHash': anchor, 'expectedHash': anchor,
                                        'simulationKey': '166a96e319c7'}})
        if kind == 'fail':
            rows[0]['verdict'] = 'FAIL';summary.update(gate='FAIL', exitCode=1, escalationLevel=2)
            summary['counts'].update(PASS=17, FAIL=1)
        elif kind == 'canary':
            rows[-1]['verdict'] = 'PASS';summary['canary']['observed'] = 'PASS'
            summary.update(gate='FAIL', exitCode=3, escalationLevel=3)
        elif kind == 'hash':
            rows[0]['config']['effectiveHash'] = 'unexpected'
            summary['integrity']['hashesVerified'] = False
            summary.update(gate='FAIL', exitCode=3, escalationLevel=3)
        elif kind == 'environment':
            rows = [];summary.update(gate='ERROR', exitCode=4, escalationLevel=2,
                                     executionError={'kind': 'environment', 'message': 'Unity timeout'})
            summary['counts'] = {k: 0 for k in summary['counts']}
            summary['canary']['observed'] = None
            summary['integrity'] = {k: None for k in summary['integrity']}
            summary['determinism'] = {}
        (run/'attempts.jsonl').write_text(''.join(json.dumps(row)+'\n' for row in rows))
        (run/'report.md').write_text('# Checker fixture\n\ngate: '+summary['gate']+'\n')
        summary['artifacts'] = {'attemptsSha256': sha(run/'attempts.jsonl'),
                                'reportSha256': sha(run/'report.md'), 'gameMetrics': []}
        dump(run/'summary.json', summary)
        return run

    def controls(self, factory):
        for kind, code, level in [('pass', 0, 1), ('fail', 1, 2), ('canary', 3, 3),
                                  ('hash', 3, 3), ('environment', 4, 2)]:
            with self.subTest(kind=kind):
                result = self.check_run(factory(kind))
                self.assert_classification(result, code, level, kind == 'pass')
                self.assertEqual(result['incoherences'], [])
                self.assertIs(result['retry']['eligible'], kind == 'environment')

    def test_complete_contract_controls(self):
        self.controls(self.contract_fixture)

    def test_copied_real_controls(self):
        if not os.environ.get('AUTOQA_REAL_RUNS'):
            self.skipTest('Set AUTOQA_REAL_RUNS to real Logs/autoqa for copied real controls')
        root = Path(os.environ['AUTOQA_REAL_RUNS'])
        def copy_run(kind):
            target = SESSION/('real-'+kind)
            shutil.copytree(root/REAL_RUNS[kind], target)
            return target
        self.controls(copy_run)


if __name__ == '__main__':
    print('CHECKER_FIXTURE_DIR='+str(SESSION), flush=True)
    result = unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(CheckerTests))
    dump(SESSION/'test-result.json', {'tests': result.testsRun, 'failures': len(result.failures),
                                    'errors': len(result.errors), 'skipped': len(result.skipped)})
    raise SystemExit(0 if result.wasSuccessful() else 1)
