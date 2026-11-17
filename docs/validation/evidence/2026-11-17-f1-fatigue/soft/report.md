# F1 — experimento B: soft

Método predeclarado; 200 partidos. NotEvaluated no cuenta como aprobado.

| Criterio | Resultado | Datos |
|---|---|---|
| B1 | Pass | {"shortPoints": 312, "humanMedianNetCost": 2.3666479002146303, "rivalMedianNetCost": 1.70656140161163} |
| B2 | Pass | {"longPoints": 992, "largerMovementCostMedian": 26.01309084572126, "movementDifferenceMedian": 7.660878169350398} |
| B3 | Fail | {"lastPointStartMedian": 80.29191713928768, "matchesBelowOnset": 182, "matches": 200} |
| B4 | Pass | {"matchesStartingBelow20": 0, "matches": 200, "zeroEnergyServes": 0} |
| B5a | Pass | {"closePoints": 26, "startEnergyMedian": 77.2557875945974} |
| B5b | Indicador | {"exploratory": true, "qualifyingPoints": 0, "pairedPoints": 0, "missingPairs": 0, "fatiguedPlayerWins": 0, "samePlayerBaselineWins": 0, "percentagePointDifference": null} |
| B6 | Pass | {"meanRealSeconds": 44.133155891476015, "baselineMeanRealSeconds": 45.08358947746597, "ratio": 0.9789184136177753} |

Ajuste predeclarado propuesto: {"key": "fatigue.recoverBetween", "from": 0.35, "to": 0.25, "reason": "B3 last-point energy too high"}

Datos intactos por SHA-256.
