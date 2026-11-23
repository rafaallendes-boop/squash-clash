"""Tablas descriptivas de §2.2 y §2.3; no calcula un veredicto de aceptación."""

import csv
import math
import statistics
import sys
from collections import defaultdict
from pathlib import Path


def number(value):
    try:
        value = float(value)
        return value if math.isfinite(value) else None
    except ValueError:
        return None


def median(rows, key):
    values = [number(row[key]) for row in rows]
    values = [value for value in values if value is not None]
    return statistics.median(values) if values else "NA"


def percent(rows, predicate):
    return 100 * sum(predicate(row) for row in rows) / len(rows)


def band(value, edges, labels):
    value = number(value)
    if value is None:
        return "NA"
    return next((label for edge, label in zip(edges, labels) if value < edge), labels[-1])


def write(path, header, rows):
    with path.open("w", newline="") as stream:
        writer = csv.writer(stream, delimiter="\t")
        writer.writerow(header)
        writer.writerows(rows)


def summarize(directory):
    with (directory / "rows.tsv").open(newline="") as stream:
        rows = list(csv.DictReader(stream, delimiter="\t"))
    cross = defaultdict(list)
    stretched = defaultdict(list)
    # T from the unchanged AX-1 court: L - short-line offset - half line width.
    t = 9.75 - 4.26 - .025
    for row in rows:
        b2 = band(row["b2z"], (1.5, 2.5, 3.5, t),
                  ("[0,1.5)", "[1.5,2.5)", "[2.5,3.5)", "[3.5,T)", "[T,inf)"))
        fw = band(row["fw_tin"], (.2, .4, .7),
                  ("[0,.2)", "[.2,.4)", "[.4,.7)", "[.7,inf)"))
        cross[(row["style"], row["level"], row["kind"], row["zone"], b2, fw)].append(row)
        if row["reason"] == "stretched" and row["result"] == "Contact":
            stretched[(row["style"], row["level"], row["zone"])].append(row)
    cross_rows = []
    for key, group in sorted(cross.items()):
        cross_rows.append((*key, len(group),
            percent(group, lambda r: r["result"] == "Contact"),
            percent(group, lambda r: r["reason"] == "stretched"),
            percent(group, lambda r: r["reason"] == "unreachable"),
            percent(group, lambda r: r["reason"] == "no-legal-window"),
            percent(group, lambda r: r["swing"] == "0" and number(r["path_m"]) is not None and number(r["path_m"]) < .30),
            median(group, "delay_ms"), median(group, "path_m"), median(group, "short_m")))
    write(directory / "cross.tsv", ("style", "level", "kind", "zone", "b2z_band", "fw_tin_band", "n",
        "contact_pct", "stretched_pct", "unreachable_pct", "no_window_pct", "stopped_pct",
        "delay_ms_p50", "path_m_p50", "short_m_p50"), cross_rows)
    stretched_rows = []
    for key, group in sorted(stretched.items()):
        stretched_rows.append((*key, len(group),
            percent(group, lambda r: r["def"] == "1"),
            percent(group, lambda r: r["height"] == "High"),
            percent(group, lambda r: r["line"] == "Boast"),
            percent(group, lambda r: r["height"] in ("Low", "Mid")),
            median(group, "ret_fw"), median(group, "ret_b2z"),
            percent(group, lambda r: r["next"] != "Contact"),
            percent(group, lambda r: r["won"] == "1")))
    write(directory / "stretched.tsv", ("style", "level", "zone", "n", "def_pct", "high_pct", "boast_pct",
        "low_mid_pct", "ret_fw_p50", "ret_b2z_p50", "next_not_contact_pct", "winner_pct"), stretched_rows)
    print(f"rows={len(rows)} cross_groups={len(cross)} stretched_contacts={sum(map(len, stretched.values()))}")


if __name__ == "__main__":
    summarize(Path(sys.argv[1]))
