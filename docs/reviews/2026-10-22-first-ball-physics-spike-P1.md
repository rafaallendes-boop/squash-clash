# Prueba de física P1 — I4b

**Entrega parcial: campaña del editor completa; ejecución del iPhone y clasificación de Claude pendientes. I4b no está cerrado.** [PR #4, borrador](https://github.com/rafaallendes-boop/squash-clash/pull/4). No es una decisión del gate I4c ni una elección de backend. I4a se fusionó con merge commit `ca50142` después de satisfacer las tres condiciones de Claude/PO.

## Método y procedencia

Unity 6000.3.27f1, macOS 26.6.2/ARM64, preset/hash `c86243b15a97`, seed `20261020`. Cada variante completó 10.053 lanzamientos de robustez: 17 fijos, 10.000 S8 y 36 S8d. S8/S8d observan los 360 pasos; S10, 720. La salida superior legítima conserva su parada aprobada. No se cambiaron escenarios, parámetros del preset, huella, umbrales ni criterios. Cada experimento terminó con XML nuevo **1/1**, sin fallos, y verificó instancias/descargas pendientes cero, número de escenas restituido y gravedad restaurada. Eso acredita la ejecución y limpieza, no el cumplimiento físico.

Las cinco variantes cambian un ajuste por vez frente a baseline: CCD especulativo; contact offset 0,5 mm; solver 12/4; Enhanced Determinism desactivado. Baseline conserva ContinuousDynamic, offset 1 mm, solver 6/1 y Enhanced Determinism activado. Los ajustes temporales de proyecto quedaron restaurados.

Baseline y speculative se ejecutaron en `cd7fc2a`; offset05, solver12 y no-ed en `ee76086`. La revisión interna detectó que la mediana par C13 elegía el central superior. En baseline hay exactamente dos muestras recuperables sin pérdida desde los valores float32 con formato R: 1,2370189428329468 y 1,273307204246521; su mediana float32 correcta es 1,255163073539734. Se conserva el original y una copia derivada, con auditoría de que **las otras 17 filas y todos los umbrales, poblaciones y veredictos permanecen iguales**. Speculative tiene 15 muestras y su mediana impar ya era correcta. No se presenta ese posprocesado como una nueva ejecución del binario corregido. `ee76086` conserva también las muestras C13 ordenadas. [Método, muestras y hashes](../validation/evidence/2026-10-22-i4b-editor-existing-runs.json).

La segunda corrección de la revisión recupera UI/host aunque falle guardar el informe; no altera mediciones. Ambos defectos tuvieron RED→GREEN y un único pase de corrección: **EditMode 170/170 y PlayMode 43/43**, sin omitidos. [Revisión interna](2026-10-22-first-ball-i4b-internal-review.md), [evidencia de regresión](../validation/evidence/2026-10-22-i4b-tests.json).

Hubo validación/compilación en procesos separados durante las mediciones. Todos los tiempos del editor son informativos; no se comparan como benchmark aislado ni se extrapolan al iPhone. C17 conserva 20 calentamientos descartados, 200 mediciones, Reset incluido, creación de escena excluida y p95 nativo ≤1 ms. [Manifiesto completo: fechas, duración, hashes y 18 filas por variante](../validation/evidence/2026-10-22-i4b-editor-runs.json).

## Línea base: los 18 criterios

| ID | Resultado medido en editor | Estado |
|---|---|---|
| C1 | 8.008/10.053 lanzamientos infractores; 138.736 muestras superficie/paso; 1.745 salidas superiores legítimas | Fail |
| C2 | 67.787 trayectorias con estados finitos | Pass |
| C3 | 25 fallos entre 1.443 evaluados; 414 no evaluables; 8.182 sin esquina fuera de población | Fail |
| C4 | 0 impactos repetidos; 4 casos | Pass |
| C5 | 1/117 comparaciones distintas | Fail |
| C6 | 92/10.017 comparaciones distintas desde lanzamiento | Fail |
| C7 | 144 distintas entre 18.778 evaluadas; 1.254 no evaluables; 20.032 esperadas | Fail |
| C8 | 2/2 comparaciones de reutilización iguales | Pass |
| C9 | 2/2 continuaciones iguales; informativo según D-3 | Info |
| C10 | 9/9 exceden 1 mm; máximo 17,86533 mm | Fail |
| C11 | 2/6 clasificaciones distintas, S9d y S9i | Fail |
| C12 | 11 primeros impactos registrados, con superficie/punto/paso e ideal analítico | Info; análisis pendiente |
| C13 | 2/40 ratios evaluables; 38 no evaluables; mínimo 1,237019, mediana corregida 1,25516307 | NotEvaluated |
| C14 | Máximo 3 impactos no persistentes por paso; tope no configurable en P1 | Info |
| C15 | S4: suelo paso 22, lateral derecha 23; S6: lateral izquierda y trasera paso 25; tiempos StepOnly | Info; no prueba simultaneidad exacta |
| C16 | Máximo 49.804,1445 mm frente a planos; 2.155.202 muestras persistentes | Info |
| C17 | 200 mediciones; mediana 14,03045, p95 15,5288, p99 15,9003, máximo 16,3083 ms | Info; falta iPhone |
| C18 | RunsInEditMode=False; timing=Estimated | Info; preferencia, no gate |

La medida C16 es distancia a planos: incluye separación tras escapar de la cancha; no demuestra un solapamiento real de collider de 49 m. C10 compara el primer contacto observado con el ideal balístico aprobado; integra error de trayectoria y de contacto. No se ajusta la velocidad inicial para compensarlo. C13 no permite acreditar el consumo de tiempo restante; no se inventa un instante a partir de StepOnly. C12–C15 se entregan sin umbrales nuevos.

[Baseline íntegra, con C13 corregido](../validation/evidence/physics-spike-P1/editor-cd7fc2a-baseline-c13-corrected.md) · [Original sin alterar](../validation/evidence/physics-spike-P1/editor-cd7fc2a-baseline-original.md) · [Primera corrida histórica 83a5eee, mediana anterior sin corregir](../validation/evidence/physics-spike-P1/editor-83a5eee-baseline-initial.md).

## Comparación de variantes

F = casos fallidos; NE = no evaluables, conservados en su población. C1 tiene 10.053 esperados, C5 117, C6 10.017 y C7 20.032. Los máximos C3 son entre casos medidos; no acotan los casos inválidos. C3 mantiene el umbral 1e-5 y todos los pasos de esquina, incluidos solo persistentes.

| Variante | C1 F | C3 F / NE | C5 F | C6 F | C7 F / NE | C10 máximo mm (9/9 F) | C11 F/6 | p95 editor ms, informativo |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| baseline | 8.008 | 25 / 414 | 1 | 92 | 144 / 1.254 | 17,86533 | 2 | 15,5288 |
| speculative | 0 | 17 / 0 | 0 | 139 | 183 / 1.253 | 77,24857 | 3 | 10,1670 |
| offset05 | 8.115 | 26 / 443 | 1 | 81 | 138 / 1.254 | 17,86533 | 2 | 15,6432 |
| solver12 | 8.010 | 23 / 415 | 0 | 96 | 155 / 1.254 | 17,86533 | 2 | 15,0243 |
| no-ed | 8.008 | 187 / 235 | 2 | 97 | 127 / 1.254 | 17,86533 | 2 | 9,5097 |

C2, C4 y C8 pasan en las cinco corridas. Ninguna cumple todos los criterios de gate. Speculative elimina los cruces medidos y reduce el máximo frente a planos a 0,612672448 mm, pero aumenta las divergencias C6/C7 y el error C10; no se promueve. Solver12 mejora el máximo C3, pero no lo cumple. No-ed tampoco resuelve la divergencia. Estas diferencias no prueban por sí mismas cuál es la causa.

Informes completos, 18 criterios y ejemplos por caso: [speculative](../validation/evidence/physics-spike-P1/editor-cd7fc2a-speculative-original.md), [offset05](../validation/evidence/physics-spike-P1/editor-ee76086-offset05.md), [solver12](../validation/evidence/physics-spike-P1/editor-ee76086-solver12.md), [no-ed](../validation/evidence/physics-spike-P1/editor-ee76086-no-ed.md).

### C3: máximos diagnósticos separados

Se informa ganancia relativa de energía mecánica y rapidez antes→después en m/s. La rapidez puede bajar aunque suba la energía mecánica por cambios de altura; se conserva la definición aprobada. El desglose no cambia el veredicto global ni excluye persistentes.

| Variante | Paso con impacto no persistente: caso / paso / ganancia / rapidez | Paso solo persistente: caso / paso / ganancia / rapidez |
|---|---|---|
| baseline | S8-2697 / 321 / 0,0178474355 / 0,13078326→0,0956399739 | S8-4348 / 14 / 5,92081356 / 1,05337→2,40061784 |
| speculative | S8-8469 / 322 / 0,0273839571 / 0,06398784→0,06558204 | S8-9384 / 344 / 0,0273839273 / 0,06382191→0,06550735 |
| offset05 | S8-7425 / 116 / 0,0263401587 / 0,082883954→0,0740853548 | S8-4348 / 14 / 5,92081356 / 1,05337→2,40061784 |
| solver12 | S8-7425 / 112 / 0,0168201 / 0,134184122→0,09717035 | S8-4348 / 14 / 0,7470622 / 1,022299→1,11270988 |
| no-ed | S8-6637 / 159 / 0,0209993273 / 0,1140756→0,08812153 | S8-5306 / 23 / 14,834444 / 0,3995601→0,000000476837158 |

Los informes íntegros conservan Measured/NotApplicable/NotEvaluable de cada subpoblación. Baseline global: 1.443/8.182/414; con impacto: 1.676/8.182/181; solo persistentes: 1.235/8.390/414. **NotApplicable por ausencia de esquina está fuera de C3; dato inválido permanece como no evaluable.**

## Clasificación pendiente de Claude

| Incumplimiento | Evidencia conservada | Clasificación |
|---|---|---|
| C1 cruces/escapes | S1 frontis pasos 33–34; S5 frontis/lateral 29; comparación con speculative | Pendiente: adaptador, prueba o motor |
| C3 energía/datos inválidos | Máximos separados y contadores arriba; persistentes no excluidos | Pendiente: adaptador, prueba o motor |
| C5–C7 igualdad bit a bit | Baseline C5 S8-4000 índice 39; C6 S5 índice 30, S6 índice 25; conteos completos por variante | Pendiente: adaptador, prueba o motor; no cambiar Reset por cuenta propia |
| C7 checkpoints ausentes | Baseline 1.254, incluidos fines/salidas superiores anteriores al checkpoint | Pendiente interpretación de elegibilidad; siguen no evaluables, no éxitos |
| C10 precisión y C11 clasificación | S9d toca tin donde se esperaba fuera; S9i no toca saque donde se esperaba dentro; huella 10 mm conservada | Pendiente: adaptador, prueba o motor |
| C12–C15 limitaciones observables | Paso sin instante exacto, primer impacto, ratios incompletos, tope no configurable | Análisis de Claude pendiente; sin umbral inventado |

No se atribuye a Claude una clasificación todavía no recibida. T4.3 tiene implementación y campaña completa, pero su paso 6 requiere esa revisión; no se marca la tarea completa por anticipado. Una corrección posterior del adaptador o de la prueba necesitará nueva evidencia. I4c sigue sin autorizar.

## Resultados en el iPhone 16 Pro

**CAMPAÑA COMPLETA PENDIENTE.** El PO conectó el teléfono y habilitó Duplicación; CoreDevice pasó a wired/connected con DDI disponible. Se instaló dev con perfil vigente. El primer lanzamiento en LCD falló Locked; después arrancó mediante devicectl en la pantalla Wireless de Duplicación. Codex observó versión 0.1.0+ee76086 dev, 60 FPS en reposo, Escenario S1→S2 y Cancelar. La cancelación guardó/copió informe INCOMPLETO, térmico nominal→nominal. Se pidió al PO iniciar la campaña completa porque el servidor remoto rechaza clics por coordenadas. No se sustituye el resultado nativo por firma o tests del editor.

**Segundo intento nativo:** el PO confirmó avance. El informe recuperado registra inicio `2026-10-22T23:38:40.6859740Z` y cancelación `2026-10-22T23:39:00.9817450Z` por **Pérdida de foco**, nominal→nominal, estado INCOMPLETO y C17 NotEvaluated. Duplicación indicó después «iPhone en uso». No se atribuye una causa más específica sin evidencia. Se pidió reiniciar dejando la app al frente; preferencia operativa: teléfono físico para la prueba y Mac para responder. [Intentos nativos y sus 18 filas](../validation/evidence/2026-10-22-i4b-iphone-runs.json). Esto comprueba cancelación por pérdida de foco; no es una campaña válida ni se etiqueta como una prueba explícita de OnApplicationPause.

**Tercer intento:** `2026-10-22T23:42:32.3586790Z` → `2026-10-22T23:42:34.1625440Z`, de nuevo INCOMPLETO por Pérdida de foco, nominal→nominal y C17 no evaluado. Se produjo sin acciones CUA entre el reinicio comunicado por el PO y la cancelación. El cambio de ventana de Duplicación al chat es una hipótesis operativa, no una causa probada. Para evitar repetir el problema se pidió usar el iPhone físico desbloqueado con Squash Clash al frente, sin Duplicación activa y respondiendo desde el Mac. No se cambia la política de cancelación de la app para permitir el experimento.

Build dev final `0.1.0+ee76086`, limpia, Unity 6000.3.27f1 → Xcode 27.1 → Release ARM64, firma verificada; metadata embebida comprobada en resources.assets. Variante preparada: **baseline**, mismo hash `c86243b15a97` y Enhanced Determinism activado. Es referencia provisional para comparar plataformas; ninguna alternativa domina los criterios y no se congela ni elige backend. No se añade un selector nuevo de variantes. Como máximo dos variantes nativas si la evidencia y la revisión justifican una segunda. [Evidencia nativa sin identificadores personales](../validation/evidence/2026-10-22-i4b-native-dev.json).

Perfil válido al compilar, vence **2026-10-28T14:59:31Z**. Comprobar vigencia antes de instalar y renovar si venció, y siempre antes de una instalación posterior al 28 de octubre. No se cambia Xcode ni la versión mínima iOS; persiste el warning libc++ de E1, sin error de enlace.

Pendiente: observar cancelación por segundo plano y ejecutar prueba completa con térmico **nominal** al inicio, guardar térmico final y metadata; recuperar Documents/spike y verificar sus 18 filas, hash, variante y 200 mediciones nativas. C17 nativo **no evaluado**. Después completar este informe y detenerse para el PO/Claude; no fusionar automáticamente, ni I4c, P2 o I5+.

## Cuarto intento físico y diagnóstico de ciclo de vida

El PO confirmó ejecución en el iPhone físico, sin Duplicación activa, y luego declaró **«No toqué nada; la app seguía al frente»** al preguntar por la interrupción. Codex no activó CUA ni Duplicación durante esa campaña: observó capturas directas de la pantalla primaria en 5 %, 26 %, 41 %, 54 %, 67 % y 78 %, y leyó Documents/spike periódicamente. El log adjunto al proceso anterior de Duplicación no se usa como prueba del proceso físico.

[Cuarto informe nativo sin alterar](../validation/evidence/physics-spike-P1/iphone-ee76086-P1-20261023-000205-032.md), fuente `ee76086`:

- Inicio UTC 2026-10-22T23:51:15.3341060Z; fin 2026-10-23T00:02:05.0271650Z. Hora local de Santiago del **22 de octubre**: 20:51:15 → 21:02:05, duración 649,693 s.
- **INCOMPLETO — Pérdida de foco**, 8.732/10.053 lanzamientos de robustez registrados. No fue un resultado completo ni una excepción capturada por el runner.
- Térmico **nominal → serious**. C17 **0/200**, no evaluado; no se alcanzó el benchmark. Todas las coberturas permanecen abiertas, incluso donde ya hay contadores parciales.
- La configuración existente adapta FPS a 30 en serious; coincide con los 30 FPS observados al 78 %. El código no cancela por temperatura durante la campaña; solo exige nominal al iniciar. La coincidencia no identifica la causa de pérdida de foco.

**Causa todavía no establecida.** La condición directa en código es `OnApplicationFocus(false)` → cancelación. No hay evidencia suficiente para afirmar que la app fue enviada a segundo plano, que el PO interactuó, que hubo un cierre por memoria, o que el calor provocó ese callback. La app guardó el informe de cancelación. No se pide otra repetición larga idéntica ni se desactiva la cancelación para obtener un resultado favorable.

Se añade un diagnóstico mínimo al informe de cancelación: evento exacto foco/pausa, UTC, segundos desde inicio, frame, foco de Unity, estado de UIKit, temporizador de bloqueo nativo, sleepTimeout solicitado, temperatura, fase/progreso y muestras de benchmark. El puente nativo es de lectura y sigue excluido de prod. `UIApplication.applicationState` representa la app o su escena más activa; **no identifica por sí solo la causa ni cada escena de Duplicación**. El temporizador se lee al recibir el evento, no demuestra retrospectivamente su estado durante los diez minutos.

Dos tests nuevos reproducen la ausencia de ese contexto: **RED 43/45 → GREEN 45/45 PlayMode**. Verifican tanto foco como pausa, reporte persistido, INCOMPLETO, restauración del sleepTimeout y cancelación conservada. No se modifican Core, P1, mediciones, números de escenarios, umbrales, presupuesto de 8 ms, 20 warmup o 200 mediciones. La compilación/enlace iOS y exclusión prod de este diagnóstico se validarán antes de instalarlo. Este cambio no convierte ningún intento parcial en válido ni atribuye una nueva revisión a Claude.

Fuentes: [Unity 6.3 OnApplicationFocus](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnApplicationFocus.html), [UIKit applicationState](https://developer.apple.com/documentation/uikit/uiapplication/applicationstate), [estado inactive](https://developer.apple.com/documentation/uikit/uiapplication/state/inactive), [idleTimerDisabled](https://developer.apple.com/documentation/uikit/uiapplication/isidletimerdisabled), [Apple serious](https://developer.apple.com/documentation/foundation/processinfo/thermalstate-swift.enum/serious).

## Fuentes técnicas

- [Apple ProcessInfo.ThermalState](https://developer.apple.com/documentation/foundation/processinfo/thermalstate-swift.enum): lectura real de nominal/fair/serious/critical.
- [Unity PluginImporter.SetIncludeInBuildDelegate](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PluginImporter.SetIncludeInBuildDelegate.html): exclusión del puente nativo por perfil, verificada contra exports nuevos dev/prod.
- [Unity 6.3 Physics settings](https://docs.unity3d.com/6000.3/Documentation/Manual/class-PhysicsManager.html): configuración global; Fast Motion Threshold conserva el valor existente, sin atribución causal no medida.
