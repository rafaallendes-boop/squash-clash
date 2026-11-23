#!/usr/bin/env python3
"""CLI contracts using synthetic metrics only; no Unity or device access."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

SCRIPT = Path(__file__).resolve().parents[1] / "metrics-summary.py"


def event(kind, payload, second=0, build="test-build"):
    return {"ts": f"2026-11-21T12:00:{second:02d}.0000000Z", "kind": kind,
            "build": build, "payload": json.dumps(payload), "metricsValid": False}


class MetricsSummaryTests(unittest.TestCase):
    def run_summary(self, files):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            for name, rows in files.items():
                path = root / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(rows if isinstance(rows, str) else
                                "".join(json.dumps(row) + "\n" for row in rows))
            def hashes():
                return {str(p.relative_to(root)): hashlib.sha256(p.read_bytes()).hexdigest()
                        for p in root.rglob("*") if p.is_file()}
            before = hashes()
            result = subprocess.run([sys.executable, str(SCRIPT), str(root)],
                                    capture_output=True, text=True)
            self.assertEqual(before, hashes(), "backup bytes or file set changed")
            return result

    def test_recorded_values_and_exact_joins(self):
        rows = [event("match-start", {"matchId": "m1"}),
                event("shot-opportunity-order", {"launchId": 1, "attemptId": "a", "opportunity": "Volley"}),
                event("shot", {"LaunchId": 1, "AttemptId": "a", "ContactStep": 5,
                               "Applied": True, "Result": "Perfect", "Reason": "applied"}),
                event("fatigue", {"human": {"Minimum": 60, "End": 65, "AfterRecovery": 70},
                                  "rival": {"Minimum": 80, "End": 85, "AfterRecovery": 90}}),
                event("rally-point", {"winner": "Human", "server": "Rival", "causal": [
                    {"player": "Human", "launchId": 1, "attemptId": "a", "contactStep": 5}]}),
                event("match-end", {"matchId": "m1", "durationSeconds": 12.25,
                                    "result": "win", "abandoned": False}, 20)]
        result = self.run_summary({"metrics-one.jsonl": rows})
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("Inicios de partido: 1; cierres normales: 1; abandonos: 0; sin cierre: 0", result.stdout)
        self.assertIn("12.25 s", result.stdout)
        self.assertIn("Perfectos: 100.00 % (1/1", result.stdout)
        self.assertIn("Voleas humanas: 100.00 % (1/1", result.stdout)
        self.assertIn("1:H", result.stdout)
        self.assertIn("Perfect=1", result.stdout)
        self.assertIn("Aplicación: aplicado=1", result.stdout)
        self.assertIn("60.00", result.stdout)
        self.assertIn("metricsValid=false: 6", result.stdout)

    def test_missing_values_are_not_inferred_and_files_are_isolated(self):
        result = self.run_summary({
            "metrics-a.jsonl": [event("match-start", {"matchId": "m"}),
                                event("shot-opportunity-order", {"launchId": 1, "attemptId": "a", "opportunity": "Volley"})],
            "metrics-b.jsonl": [event("shot", {"LaunchId": 1, "AttemptId": "a", "ContactStep": 5,
                                              "Applied": True, "Result": "Valid", "Reason": "applied"}),
                                event("rally-point", {"winner": "Rival", "server": "Rival", "causal": []}, 30)]})
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("sin cierre: 1", result.stdout)
        self.assertIn("Voleas humanas: no registrado", result.stdout)
        self.assertIn("Duración: no registrado", result.stdout)
        self.assertIn("Energía: no registrado", result.stdout)
        self.assertIn("Sin traza de devolución: 1", result.stdout)
        self.assertIn("sin modalidad registrada: 1/1", result.stdout)

    def test_unity_seven_digit_timestamps_remain_exact_in_report(self):
        first = event("launch", {})
        first["ts"] = "2026-11-21T12:00:00.1234567Z"
        last = event("launch", {})
        last["ts"] = "2026-11-21T12:00:00.1234569Z"
        result = self.run_summary({"metrics-a.jsonl": [first, last]})
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn(first["ts"] + " — " + last["ts"], result.stdout)

    def test_denominators_exclude_failures_and_show_missing_modes(self):
        rows = [event("shot-opportunity-order", {"launchId": 1, "attemptId": "a", "opportunity": "Volley"}),
                event("shot", {"LaunchId": 1, "AttemptId": "a", "ContactStep": 5,
                               "Applied": True, "Result": "Perfect", "Reason": "applied"}),
                event("shot", {"LaunchId": 2, "AttemptId": "b", "ContactStep": 5,
                               "Applied": True, "Result": "Valid", "Reason": "applied"}),
                event("shot", {"LaunchId": 3, "AttemptId": "c", "ContactStep": 5,
                               "Applied": False, "Result": "Failed", "Reason": "after-k-out"}),
                event("shot-order", {"LaunchId": 3, "AttemptId": "c", "Result": "Perfect"}),
                event("shot", {"LaunchId": 4, "AttemptId": "", "ContactStep": 5,
                               "Applied": True, "Result": "Valid", "Reason": "rival-contact"})]
        result = self.run_summary({"metrics-a.jsonl": rows})
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("Perfectos: 50.00 % (1/2", result.stdout)
        self.assertIn("Perfectos: 0.00 % (0/1", result.stdout)
        self.assertIn("Voleas humanas: 100.00 % (1/1", result.stdout)
        self.assertIn("sin modalidad registrada: 1/2", result.stdout)

    def test_pre_install_selection_avoids_double_counting(self):
        row = event("match-start", {"matchId": "m"})
        result = self.run_summary({"backup-before-install/metrics/metrics-a.jsonl": [row],
                                   "after-install/metrics/metrics-a.jsonl": [row, row]})
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("Inicios de partido: 1;", result.stdout)

    def test_corrupt_or_ambiguous_input_fails_without_partial_report(self):
        for files in ({"metrics-a.jsonl": '{"payload":'},
                      {"first/metrics/metrics-a.jsonl": [event("launch", {})],
                       "second/metrics/metrics-b.jsonl": [event("launch", {})]}):
            with self.subTest(files=list(files)):
                result = self.run_summary(files)
                self.assertNotEqual(result.returncode, 0)
                self.assertEqual(result.stdout, "")
                self.assertIn("Error:", result.stderr)


if __name__ == "__main__":
    unittest.main()
