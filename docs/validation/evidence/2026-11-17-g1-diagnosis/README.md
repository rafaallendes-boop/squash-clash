# Evidencia derivada — G1 fase A

17 de noviembre de 2026, Codex. [Informe de una página](../../../reviews/2026-11-17-g1-back-court-diagnosis.md), [método predeclarado](../../2026-11-17-g1-diagnosis-method.md). No hubo cambios en producto, parámetros, fixtures existentes ni regresión. El experimento nuevo y su invocador se confirmaron en `6feeb9442de30bd919f72b94aed149b698179de9`, después del método `786d54c`; ambos procesos corrieron sobre ese árbol limpio. Base `bedc2d7` (producto de `2b82701`). No hubo defecto crítico demostrado ni repetición por fallo.

## Recibo de ejecución

| Corrida | Inventario previo (UTC) | Experimento (UTC) | Resultado fresco |
|---|---|---|---|
| run01 | 17 nov 12:16:34, ningún Editor | 12:18:26–12:18:46 | 1/1 Passed, 0 failed/skipped |
| run02 | 17 nov 12:19:27, ningún Editor | 12:19:47–12:20:04 | 1/1 Passed, 0 failed/skipped |

Unity `6000.3.27f1`, P2, bounceThreshold 0,1; defaults `430da2faa818` (121 claves), SimulationKey `166a96e319c7`, RallyParams `7d50ddaf5d4f`. Cada corrida produce 108 golpes A1 + 5.880 A2 y 648 recepciones. Los cinco archivos deterministas coinciden en tamaño y SHA-256 entre procesos; ver [manifest.json](manifest.json). Los hashes de XML, logs e inventarios se incluyen aparte: esos archivos contienen tiempos y no se comparan como datos deterministas.

Datos completos, logs e inventarios se conservan localmente, ignorados, en `Logs/experiments/g1/run01`, `run02` y sus carpetas `*-runner` del worktree G1. No se versionan logs, rutas privadas, device.env ni identificadores del dispositivo. `scripts/diagnose-g1.sh run01|run02` niega sobrescribir una invocación. **No volver a ejecutar:** las dos corridas predeclaradas terminaron; no hay autorización de variantes. `python3 scripts/summarize-g1.py` regenera solamente estas tablas a partir de los datos guardados y verifica ambos XML y hashes, sin lanzar Unity.

## Archivos y denominadores

- [a1-shots.tsv](a1-shots.tsv): los 108 golpes con sus posiciones, velocidades, frontis, ápice muestreado, botes, vidrio, cierre y resultado geométrico.
- [a1-receptions.tsv](a1-receptions.tsv): 648 planes y cubetas, ventanas, candidatos puntuales, llegada requerida/disponible, espera, ruido, stance y contacto. Cada grupo receptor/dificultad tiene 108 golpes, de los cuales 103 son legales y 5 out. Las filas Humano Fácil/Media/Difícil son controles repetidos, **no 324 observaciones independientes**.
- [a1-tables.tsv](a1-tables.tsv): agregados por receptor, dificultad, fixture y potencia. `legalGlass` exige que el vidrio preceda al cierre y no sea out. `legalGlassReceivable` cuenta planned/stretched, no golpes ejecutados. `postGlassContact` exige contacto posterior al primer bote y a un vidrio legal; puede incluir ambos órdenes entre bote y vidrio. Los 61 vidrios: 44 bote→vidrio y 17 vidrio→bote; los 61 tienen alguna muestra legal posterior al vidrio. Ninguno tiene segundo bote a ≤0,35 m del vidrio.
- [a2-tables.tsv](a2-tables.tsv): 1.470 solicitudes por receta/modo, incluidas las duplicadas por recorte. `effectiveAims` cuenta (aimX,aimY) y `effectiveShots` añade P. Sin cap: 70 miras/1.470 golpes distintos; cap22: 28/588. Por receta, 987 solicitudes son recortadas. Los 16 cruzados directos son distintos; los 33 cap22 colapsan a 13 distintos. No interpretar porcentajes de grilla como tasa de éxito humano.
- [a2-witnesses.tsv](a2-witnesses.tsv): los 100 testigos paralelos/cruzados (17 directos + 33 cap22 por receta). El predicado diagnóstico es el fijado en el método; no es aceptación. `driveAnyLine` también cuenta trayectorias bajas profundas fuera de esas dos bandas; por eso es mayor.
- [a2-width.tsv](a2-width.tsv) y [a2-contiguous-width.tsv](a2-contiguous-width.tsv): valores testigo e intervalos contiguos **de la malla**, sin interpolación. La paralela directa aparece solo a P=0,75 y y=0,88, columna straight; un punto por cada eje. Los rangos marginales de P/y de los cruzados no implican que todo su producto cartesiano funcione.
- [a2-cap-mapping.tsv](a2-cap-mapping.tsv): mira solicitada/recortada y elevación balística a P=0,5. El mapa de cap no depende de P; la elevación del solver sí. En SL5/SL6 T*=113, contacto y=0,172054231, z=1,19793546. La misma banda de altura de opciones Low/Mid/High puede quedar colapsada por el cap, antes de pasar al solver.

## Lectura de las causas y límites

Cubetas primarias excluyentes: out en vidrio (d), otras faltas/no resuelto (e), recibible, sin ventana legal (a), candidato puntual con contacto rechazado (c), llegada tarde (b), sin candidatos (e). Dar prioridad a las faltas impide contar una pelota out como punto ganado por inalcanzable. `route` mantiene las razones reales del planificador. Hay 0 a y 0 c entre vuelos legales; el espejo no detectó casos unreachable con candidatos puntuales. En planned/stretched el resultado es un contacto **potencial**: el experimento no ejecuta swing, errores causales, latencia ni ruido de release.

`lateStarSteps` es el menor arrival−untilStar entre los candidatos tardíos, y `bestAvailableSteps`/`bestRequiredSteps` corresponden a ese candidato; dividir entre 120 da segundos de simulación. `lateAvailableSteps` es el mínimo independiente de arrival−available: puede proceder de otro candidato. No confundir ambos mínimos. No sumar retardo dos veces: ya está incluido en StepsToReach. `postBackWindow` cuenta pasos legales, no segundos ni contactos efectivos.

La trayectoria cruda conserva la continuación geométrica después de out hasta el segundo bote, over-top u horizonte. Los flags `inPlay` y `closeSteps` identifican el cierre; los botes posteriores no son juego legal. El predicado de testigo exige cierre DoubleBounce y segundo bote, así que los 594 Out A2 (297 por receta) son NotEvaluated para ese predicado, **no Pass**. Todos los demás 5.286 A2 tienen segundo bote; no hubo golpes no resueltos, horizonte ni TargetLowered. Los 5 out A1 tampoco cuentan como recibibles legales.

**Convención de PointRules:** `OnOverTop` cierra con un evento `default`, cuyo `TerminalSurface` es Floor. Por ello Out + terminalSurface Floor en este experimento significa **over-top**, no «out en el piso». Los 594 Out A2 se descomponen en 460 over-top, 90 laterales y 44 vidrio. En A1 hay 4 vidrio y 1 lateral. La distinción se obtiene del cierre de reglas y del código actual, sin cambiarlo. El ápice es el máximo de estados hasta cierre; no un máximo continuo interpolado.

`WallClearance` solo normaliza la parte de pared de la puntuación Feature en `ReceptionPlanner` y `ContactPlanner`; no hay distancia mínima a la pared que invalide por sí misma un contacto. La legalidad depende de altura, alcance, primer/segundo bote y bandas. No hay evidencia para proponer «quitar el rechazo de pared» como corrección.

El Humano aquí recibe como el sustituto Bot: percepción y ruido de postura cero, espera del humano, misma cinemática. La comparación cambia receptor, espera y sus límites reales, conservando golpe idéntico; no fuerza una simetría ficticia ni simula humanos. Media es principal; Fácil/Difícil varían percepción y ruido por dificultad, con semilla fijada por caso.

No se ejecutaron regresión, PlayMode, AutoQA, check-release, build nativa ni acceso al iPhone. Son experimentos aislados; los dos 1/1 no representan suites completas. El código de producto, DefaultPreset, Scenario.ExpectedHash, ProjectSettings y Packages tienen diff vacío contra la base.
