#!/usr/bin/env python3
"""Read MetricsLog JSONL and print Markdown. Uses only the Python standard library.

Usage: python3 scripts/metrics-summary.py BACKUP > summary.md
BACKUP may be an install evidence root, backup-before-install, or metrics directory.
An install root selects backup-before-install/metrics, never its after-install copy.
Only recorded fields are used; durations are never reconstructed from timestamps.
"""
import argparse
from collections import Counter, defaultdict
from datetime import datetime
import json
import math
from pathlib import Path
import re
import statistics
import sys

MISSING = "no registrado"


def cell(value):
    if value is None or value == "":
        return MISSING
    return str(value).replace("|", "\\|").replace("\n", " ").replace("\r", " ")


def number(value):
    return isinstance(value, (int, float)) and not isinstance(value, bool) and math.isfinite(value)


def percentage(numerator, denominator):
    return f"{100 * numerator / denominator:.2f} % ({numerator}/{denominator}" if denominator else MISSING


def find_metrics(root):
    for directory in (root / "backup-before-install/metrics", root / "metrics", root):
        files = sorted(directory.glob("metrics-*.jsonl"))
        if files:
            return files
    groups = {p.parent for p in root.rglob("metrics-*.jsonl")}
    if len(groups) != 1:
        raise ValueError("selecciona una sola carpeta de métricas; hay cero o varias candidatas")
    return sorted(groups.pop().glob("metrics-*.jsonl"))


def read_backup(root):
    streams = []
    for path in find_metrics(root):
        records = []
        with path.open(encoding="utf-8") as stream:
            for line_number, line in enumerate(stream, 1):
                try:
                    record = json.loads(line)
                    payload = json.loads(record["payload"])
                    timestamp = record["ts"]
                    # MetricsLog emits UTC ISO 8601 timestamps with seven fractional digits.
                    # Python 3.9 accepts at most six fractional digits. Normalize only
                    # for validation; retain the original timestamp in the report.
                    normalized = re.sub(r"(\.\d{6})\d(?=Z$)", r"\1", timestamp)
                    datetime.fromisoformat(normalized.replace("Z", "+00:00"))
                    if not isinstance(payload, dict) or not isinstance(record["kind"], str):
                        raise ValueError("evento o payload inválido")
                    records.append((record, payload))
                except (ValueError, TypeError, KeyError) as error:
                    raise ValueError(f"{path.name}:{line_number}: JSONL inválido ({error})") from error
        streams.append((path, records))
    if not any(records for _, records in streams):
        raise ValueError("archivos de métricas vacíos")
    return streams


def shot_key(payload, trace=False):
    fields = ("launchId", "attemptId", "contactStep") if trace else ("LaunchId", "AttemptId", "ContactStep")
    values = tuple(payload.get(field) for field in fields)
    return values if values[0] is not None and values[2] is not None else None


def describe(counter):
    return ", ".join(f"{cell(key)}={count}" for key, count in sorted(counter.items())) or MISSING


def summary(root):
    streams = read_backup(root)
    records = [pair for _, stream in streams for pair in stream]
    timestamps = [record["ts"] for record, _ in records]
    builds = defaultdict(list)
    for record, _ in records:
        builds[record.get("build") or MISSING].append(record["ts"])
    kinds = Counter(record["kind"] for record, _ in records)
    matches = []
    applied = defaultdict(list)
    modalities = Counter()
    returns = {player: {"points": 0, "traces": 0, "results": Counter(), "modes": Counter(), "applied": Counter()}
               for player in ("Human", "Rival")}
    for path, stream in streams:
        # LaunchId/AttemptId are local to an app process. Never join across files.
        shots = defaultdict(list)
        orders = defaultdict(set)
        for record, payload in stream:
            if record["kind"] == "shot":
                key = shot_key(payload)
                if key is not None:
                    shots[key].append(payload)
            elif record["kind"] == "shot-opportunity-order":
                key = (payload.get("launchId"), payload.get("attemptId"))
                if key[0] is not None and key[1] and payload.get("opportunity") in ("Volley", "Bounce"):
                    orders[key].add(payload["opportunity"])

        def mode(payload):
            values = orders.get((payload.get("LaunchId"), payload.get("AttemptId")), set())
            return next(iter(values)) if len(values) == 1 else MISSING

        opened = {}
        for record, payload in stream:
            kind = record["kind"]
            if kind in ("match-start", "match-end"):
                match_id = payload.get("matchId")
                if not match_id:
                    raise ValueError(f"{path.name}: {kind} sin matchId")
                if kind == "match-start":
                    if match_id in opened:
                        raise ValueError(f"{path.name}: match-start duplicado")
                    row = {"start": record["ts"], "build": record.get("build"), "end": None}
                    matches.append(row)
                    opened[match_id] = row
                else:
                    row = opened.pop(match_id, None)
                    if row is None:
                        row = {"start": None, "build": record.get("build"), "end": None}
                        matches.append(row)
                    row["end"] = payload
            elif kind == "shot" and payload.get("Applied") is True:
                reason = payload.get("Reason")
                player = "Rival" if reason == "rival-contact" else "Human" if reason else MISSING
                applied[player].append(payload)
                if player == "Human":
                    modalities[mode(payload)] += 1
            elif kind == "rally-point":
                receiver = {"Rival": "Human", "Human": "Rival"}.get(payload.get("server"))
                if receiver is None:
                    continue
                stats = returns[receiver]
                stats["points"] += 1
                causal = payload.get("causal") or []
                # The first trace is the receiver's stroke. It is not the serve itself.
                if not causal or causal[0].get("player") != receiver:
                    continue
                stats["traces"] += 1
                candidates = shots.get(shot_key(causal[0], trace=True), [])
                expected = [shot for shot in candidates
                            if (shot.get("Reason") == "rival-contact") == (receiver == "Rival")]
                if len(expected) == 1:
                    stats["results"][expected[0].get("Result") or MISSING] += 1
                    stats["modes"][mode(expected[0]) if receiver == "Human" else MISSING] += 1
                    applied_value = expected[0].get("Applied")
                    label = "aplicado" if applied_value is True else "no aplicado" if applied_value is False else MISSING
                    stats["applied"][label] += 1
                else:
                    stats["results"][MISSING] += 1
                    stats["modes"][MISSING] += 1
                    stats["applied"][MISSING] += 1

    ends = [row["end"] for row in matches if row["end"] is not None]
    normal = sum(end.get("abandoned") is False for end in ends)
    abandoned = sum(end.get("abandoned") is True for end in ends)
    unclosed = sum(row["end"] is None for row in matches)
    out = ["# Resumen de métricas del iPhone", "",
           f"Fuente: `{cell(root.name)}` → `{cell(streams[0][0].parent.relative_to(root))}`. "
           f"{len(streams)} archivos JSONL; {len(records)} eventos. Lectura del respaldo existente.", "",
           f"Rango de eventos (UTC): {min(timestamps)} — {max(timestamps)}. "
           "La fecha del respaldo no implica que haya juego ese día.", "",
           "Builds de origen (campo `build`; el nombre de la carpeta no identifica la build de los eventos):", "",
           "| Build | Primer evento UTC | Último evento UTC |", "|---|---|---|"]
    for build, dates in sorted(builds.items(), key=lambda pair: min(pair[1])):
        out.append(f"| {cell(build)} | {min(dates)} | {max(dates)} |")
    invalid = sum(record.get("metricsValid") is False for record, _ in records)
    out += ["", f"Cobertura: metricsValid=false: {invalid} eventos. Se conservan en este resumen de uso; "
            "no se declara una validación de física o rendimiento. La retención del writer es limitada; "
            "estos recuentos corresponden solo a los archivos conservados.", "",
            f"Inicios de partido: {kinds['match-start']}; cierres normales: {normal}; "
            f"abandonos: {abandoned}; sin cierre: {unclosed}. "
            f"Resultados de los cierres: {describe(Counter(end.get('result') or MISSING for end in ends))}.", "",
            "Cada fila corresponde a un matchId; la duración usa exclusivamente `match-end.durationSeconds` "
            "(tiempo real transcurrido, incluidas pausas). Sin cierre, Duración: no registrado. "
            "No se reconstruye a partir de los puntos o las marcas de fecha.", "",
            "| Partido | Inicio UTC | Build | Resultado | Marcador humano–rival | Duración |",
            "|---|---|---|---|---|---|"]
    for index, row in enumerate(matches, 1):
        end = row["end"] or {}
        duration = end.get("durationSeconds")
        seconds = f"{duration:.2f} s" if number(duration) else MISSING
        score = f"{end['humanPoints']}–{end['rivalPoints']}" if all(k in end for k in ("humanPoints", "rivalPoints")) else MISSING
        out.append(f"| {index} | {cell(row['start'])} | {cell(row['build'])} | "
                   f"{cell(end.get('result'))} | {score} | {seconds} |")
    out += ["", "Porcentajes sobre contactos aplicados (`shot.Applied=true`); se excluyen órdenes, "
            "presentaciones, fallos y recepciones sin golpe. Incluye práctica y partidos; "
            "los eventos shot no llevan matchId.", ""]
    for player, label in (("Human", "humano"), ("Rival", "rival")):
        shots = applied[player]
        known = [shot for shot in shots if shot.get("Result")]
        perfect = sum(shot["Result"] == "Perfect" for shot in known)
        value = percentage(perfect, len(known))
        if known:
            value += f" contactos del {label} con calidad registrada); sin calidad: {len(shots) - len(known)}"
        out.append(f"Perfectos: {value} ({label}).")
    known_modes = modalities["Volley"] + modalities["Bounce"]
    volley = percentage(modalities["Volley"], known_modes)
    if known_modes:
        volley += " contactos humanos con modalidad registrada)"
    out += ["", f"Voleas humanas: {volley}; sin modalidad registrada: {modalities[MISSING]}/{len(applied['Human'])}. "
            "Se une shot con shot-opportunity-order por archivo, launchId y attemptId; "
            "una oferta o un toque no cuentan como volea realizada. Voleas del rival y porcentaje global: no registrado.", "",
            f"Ganador de cada punto: {kinds['rally-point']} eventos `rally-point`. "
            "H=humano, R=rival, N=sin ganador. Numeración local por archivo, en orden de escritura. "
            "No se asigna un matchId a los puntos porque el writer no lo guarda.", "",
            "| Archivo (fecha y hora UTC de apertura) | Punto:ganador |", "|---|---|"]
    winners = Counter()
    for path, stream in streams:
        points = [payload for record, payload in stream if record["kind"] == "rally-point"]
        if not points:
            continue
        sequence = []
        for index, point in enumerate(points, 1):
            winner = point.get("winner") or MISSING
            winners[winner] += 1
            sequence.append(f"{index}:{ {'Human': 'H', 'Rival': 'R', 'None': 'N'}.get(winner, cell(winner))}")
        stamp = path.name.removeprefix("metrics-").split("-")[0]
        out.append(f"| {cell(stamp)} | {', '.join(sequence)} |")
    out += ["", f"Totales de ganadores: {describe(winners)}.", "",
            "Devoluciones de saque: solo puntos cerrados que registran `server`. "
            "Se toma la primera traza causal del receptor y se une con shot por archivo, launchId, "
            "attemptId y contactStep. Una traza no demuestra por sí sola que el golpe se aplicó. "
            "Sin traza, la devolución es no registrado; no se deduce del ganador del punto.", ""]
    for player, label in (("Human", "Humano ante saque rival"), ("Rival", "Rival ante saque humano")):
        stats = returns[player]
        out.append(f"{label}: {stats['points']} saques; con traza de devolución: {stats['traces']}. "
                   f"Sin traza de devolución: {stats['points'] - stats['traces']}. "
                   f"Aplicación: {describe(stats['applied'])}. "
                   f"Calidad: {describe(stats['results'])}. Modalidad: {describe(stats['modes'])}.")
    missing_server = kinds['rally-point'] - sum(stats['points'] for stats in returns.values())
    out += ["", f"Servidor no registrado en {missing_server} puntos. "
            "No hay etiqueta de tipo de devolución (drive, revés, etc.) en estos eventos; no registrado.", ""]
    fatigue = [payload for record, payload in records if record["kind"] == "fatigue"]
    if not fatigue:
        out.append("Energía: no registrado.")
    else:
        out += [f"Energía: {len(fatigue)} eventos `fatigue` frente a {kinds['rally-point']} puntos. "
                "Solo muestras de puntos cerrados; no se estima la energía durante intervalos sin registro. "
                "Unidades de energía 0–100; mediana calculada sobre cada campo presente.", "",
                "| Jugador | Mínimo observado (Minimum) | Mediana inicial (Start) | Mediana final (End) | Mediana tras recuperar (AfterRecovery) |",
                "|---|---|---|---|---|"]
        for player, label in (("human", "Humano"), ("rival", "Rival")):
            values = []
            for field in ("Minimum", "Start", "End", "AfterRecovery"):
                samples = [p.get(player, {}).get(field) for p in fatigue]
                samples = [sample for sample in samples if number(sample)]
                aggregate = min(samples) if samples and field == "Minimum" else statistics.median(samples) if samples else None
                values.append(f"{aggregate:.2f} (n={len(samples)})" if aggregate is not None else MISSING)
            out.append(f"| {label} | {' | '.join(values)} |")
    out += ["", "Formato contrastado con `Assets/SquashClash/Core/Diagnostics/MetricsLog.cs`, "
            "`UserMetrics.cs`, `Core/Shot/ShotRecord.cs`, `Core/Rally/Fatigue.cs`, "
            "`Runtime/Rally/RallyDirector.cs` y `Runtime/Shot/ShotController.cs` (base 518f9c3). "
            "El payload es un objeto JSON escapado dentro de cada línea JSONL. "
            "Los campos ausentes no se completan con los defaults actuales.", ""]
    return "\n".join(out)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("backup", type=Path)
    args = parser.parse_args()
    try:
        report = summary(args.backup.resolve())
    except (OSError, ValueError, TypeError) as error:
        print(f"Error: {error}", file=sys.stderr)
        return 1
    print(report, end="")
    return 0


if __name__ == "__main__":
    sys.exit(main())
