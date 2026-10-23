# I4c — decisión de física First Ball: evaluar P2

Fecha: **2026-10-23**. Decisión del PO tras la revisión final de Claude de I4b: **opción B, autorizar la evaluación de P2/T4.5 con los mismos escenarios, criterios y umbrales**. P1 sigue como respaldo. Backend definitivo pendiente de los datos de P2 y de un gate posterior del PO.

Esta PR `firstball/i4c-physics-decision` es exclusivamente documental. Registra la decisión comunicada por el PO y la adenda r2 aprobada; queda para revisión de Claude. **Detenerse cuando la PR esté lista y no fusionarla sin esa revisión.** La implementación P2 empieza solo después de fusionar I4c, en `firstball/i4d-p2-analytic`, con su propia PR de física. I5 no autorizado.

## Antecedente y fuentes de evidencia

Claude aprobó la entrega T4.3–T4.4 y el PO autorizó su cierre. [PR #4 fusionada](https://github.com/rafaallendes-boop/squash-clash/pull/4), merge commit **0e08daa8133206361a6017c2527bf84c0fcbf5f6**, head f563284. Main sincronizada y worktree I4b eliminado; logs/builds locales preservados fuera del worktree en ruta ignorada. **I4b cerrado**, con todos los veredictos físicos intactos. Aprobación de Claude comunicada por el PO; no se atribuye una GitHub Review distinta de esa comunicación.

- [Informe P1 completo, clasificación y campañas](2026-10-22-first-ball-physics-spike-P1.md).
- [Cinco campañas nuevas del editor](../validation/evidence/2026-10-23-i4b-editor-canonical-runs.json), código da98fd4, Unity 6000.3.27f1.
- [Informe nativo final íntegro](../validation/evidence/physics-spike-P1/iphone-2fdede5-P1-20261023-142516-831.md) y [auditoría de condiciones, 18 criterios y hash](../validation/evidence/2026-10-23-i4b-native-compliant-run.json).
- [RED físico, GREEN y regresión](../validation/evidence/2026-10-23-i4b-contact-order-tests.json), [comprobaciones de build/ProjectPhysics](../validation/2026-10-23-i4b-native-preflight.md) y [aprobaciones del PO](2026-10-21-first-ball-po-approvals.md#cierre-de-i4b-y-decisión-po-b-para-i4cp2--2026-10-23).

Las campañas son experimentos separados de la regresión: su ejecución completa no transforma Fail o NotEvaluated en Pass. Regresión registrada de código: **EditMode 188/188 y PlayMode 58/58**. No se ejecutan nuevas suites, builds ni campañas para esta PR documental.

## Evidencia P1 del editor

Cinco variantes nuevas completas, cada una con **10.053 lanzamientos** (17 fijos, 10.000 S8 y 36 S8d), seed **20261020**, configuración **c86243b15a97**, 20 calentamientos y 200 mediciones C17. ProjectPhysics **fastMotionThreshold=0.01 m** común a todas. Baseline C1 cero cruces; C5/C6/C7 pasan tras el orden canónico. C3/C10/C11 siguen Fail. Ninguna variante cumple todos los criterios de gate y no se promueve ninguna.

| Variante editor | C1 fallos | C3 fallos | C5 fallos | C6 fallos | C7 fallos | C10 máximo mm | C11 fallos/6 |
|---|---:|---:|---:|---:|---:|---:|---:|
| baseline | 0 | 5 | 0 | 0 | 0 | 13,6164427 | 3 |
| speculative | 0 | 17 | 0 | 1 | 0 | 77,24857 | 3 |
| offset05 | 0 | 7 | 0 | 0 | 0 | 13,6164427 | 3 |
| solver12 | 0 | 7 | 0 | 0 | 0 | 13,6164427 | 3 |
| no-ed | 0 | 5 | 0 | 0 | 3 | 13,6164427 | 3 |

Las poblaciones C5/C6 son 117/10.017 por variante. C7 baseline: 18.753 medidos, 1.279 no aplicables y cero no evaluables. Los conteos completos de cada variante y las divergencias están en el manifiesto. Speculative C6 conserva S8-6719, terminación distinta con el mismo prefijo; no-ed C7 conserva tres divergencias numéricas reales. No se altera Reset ni se ocultan esos fallos.

**Condiciones de la última medición C17 del editor:** baseline da98fd4, 20 calentamientos/200 mediciones, nearest rank, p95 **34,927300 ms** (34,9 ms). Tres editores inicialmente y dos después, worktrees aislados, actividad normal del Mac, sin control aislado de carga ni registro térmico del Mac. Histórico 16ad653: p95 **24,120100 ms** (24,1 ms), dos editores parcialmente concurrentes. Se registran condiciones y valores sin interpretar la diferencia ni compararlos con la medición nativa. C17 del editor es informativo.

## Evidencia P1 nativa bajo condiciones aprobadas

Build **0.1.0+2fdede5 dev**, export limpio Release ARM64 firmado, Player no Development e IL2CPP/Xcode Release; metadata, firma, instalación y arranque verificados. Prod excluye DevPanel y el puente nativo. Perfil comprobado vigente antes de instalar, herramientas sin cambios. La comprobación corta previa registra fastMotionThreshold=0.01 desde la configuración fuente de la build; el encabezado nativo actual omite ese campo y no se afirma getter en runtime.

PO: **«Prueba completa - Batería inicial 90%. Se cumplen las condiciones»**. iPhone 16 Pro físico (iPhone17,1), iOS 26.6.2, No molestar, bajo consumo desactivado, sin funda y sin capturas/duplicación/devicectl/otras herramientas durante ejecución. Misma variante **baseline/ccdDyn-co1mm-it6-1** y configuración **c86243b15a97**. UTC **14:14:06.4532240 → 14:25:16.8274640**, informe completo recuperado solo después del aviso del PO.

Térmico **nominal al inicio**, **fair al iniciar C17 y al terminar**; **0 interrupciones de foco**. El térmico al iniciar el benchmark es informativo según la política aprobada, sin umbral nuevo ni atribución causal. Archivo íntegro de 11.517 bytes, SHA-256 **87043764667f9018d6933493f83a494967c130316fbe24fabdde29cce2cc70fe**; auditadas 18 filas únicas, build/configuración/variante, 200 medidas C17, térmico y foco.

**C17 Fail: p95 14,322100 ms frente a ≤1 ms**, unas 14 veces el umbral según la decisión del PO. Mediana 13,871800; p99 14,481400; máximo 14,818100 ms. 20 calentamientos, 200 medidas, cobertura 200/0/200. Conteo/agregados verificados; las muestras individuales no se persistían y no se recalculan percentiles de forma independiente. No se atribuye causa al térmico ni se compara con el editor.

La corrida anterior con 75 % de batería y funda se mantiene íntegra como histórico fuera de condiciones, sin sustituir la final ni usarla para atribuir causas. **No se repite I4b.**

## Criterios y clasificación final de Claude

Las atribuciones a motor/adaptador son las comunicadas por el PO y registradas en el informe P1. Donde no se comunicó otra clasificación, se consigna el resultado o capacidad observada; no se inventa un dictamen adicional. Las comparaciones bit a bit se evalúan dentro del mismo binario y dispositivo, sin afirmar igualdad entre editor e iPhone.

| Criterio | Baseline editor | Baseline nativo | Clasificación final o alcance comunicado |
|---|---|---|---|
| C1 robustez | Pass, 0/10.053 cruces | Pass, 0/10.053 cruces | Adaptador/configuración: resuelto con la corrección CCD; sin relajar atravesamientos. |
| C2 estados finitos | Pass, 67.737 evaluados | Pass, 67.737 evaluados | Cumple; sin limitación adicional atribuida. |
| C3 energía en esquinas | Fail, 5/3.501 | Fail, 5/3.501 | Motor, residual en contacto persistente de baja velocidad; sin compensación ni piso de velocidad autorizado. Máximo 0,0273839869 en S8-9456. |
| C4 impactos repetidos | Pass, 0/4 fallos | Pass, 0/4 fallos | Cumple; sin clasificación adicional. |
| C5 consistencia | Pass, 0/117 fallos | Pass, 0/117 fallos | Baseline C5/C6/C7 pasan; divergencias anteriores solo de orden resueltas en adaptador. |
| C6 predicción al lanzar | Pass, 0/10.017 fallos | Pass, 0/10.017 fallos | Baseline pasa. Speculative C6 S8-6719: motor/variante, terminación distinta con mismo prefijo; permanece Fail. |
| C7 relanzamiento | Pass, 0/18.753 medidos; 1.279 NA | Pass, 0/18.753 medidos; 1.279 NA | Orden baseline resuelto en adaptador. No-ed C7: motor/configuración, divergencias numéricas reales; Enhanced Determinism necesario. |
| C8 reutilización | Pass, 0/2 fallos | Pass, 0/2 fallos | Cumple; Reset conservado. |
| C9 continuación sin reset | Info, 0/2 fallos | Info, 0/2 fallos | Diagnóstico, no gate; sin clasificación adicional. |
| C10 precisión S9 | Fail, 9/9; máx. 13,6164427 mm | Fail, 9/9; máx. 13,6164722 mm | Motor: sesgo de integración semimplícita frente a referencia analítica. No se compensa; umbral ≤1 mm intacto. |
| C11 líneas | Fail, 3/6 | Fail, 3/6 | Motor, consecuencia de C10 en S9c/S9d/S9i próximos a línea. |
| C12 primer impacto | Info, 11 evaluados | Info, 11 evaluados | Evidencia por superficie/punto/instante preservada; sin umbral ni clasificación adicional inventados. |
| C13 tiempo restante | NotEvaluated, 35 medidos/1 no evaluable | NotEvaluated, 35 medidos/1 no evaluable | Motor, CCD de una pasada; informativo, sin convertir el dato no evaluable en aprobación. |
| C14 impactos por paso | Info, máximo 2; tope no configurable | Info, máximo 2; tope no configurable | Capacidad declarada de P1: maxImpactsPerStep no operativo; se conserva el diagnóstico. |
| C15 simultáneos/tiempo | Info, 2 evaluados; Timing Estimated | Info, 2 evaluados; Timing Estimated | S4/S6 no simultáneos en datos observados; informe al gate, sin umbral nuevo. |
| C16 penetración/persistentes | Info, máx. 11,3993931 mm | Info, máx. 11,3993931 mm | Motor: falta barrido CCD suelo/RightWall con pelota rodando; paso 246 sin pared, respuesta por despenetración en 247. Afecta zona del nick; informativo. |
| C17 rendimiento | Info; condiciones documentadas arriba | **Fail**, p95 14,322100 ms, 200 medidas | Incumplimiento medido de P1 (unas 14 veces ≤1 ms); sin diagnóstico causal adicional atribuido a Claude. |
| C18 EditMode | Info, RunsInEditMode=False | Info, RunsInEditMode=False | Capacidad P1, preferencia y no gate. |

Para C3, la frase histórica de Claude «con impacto, ganancia 0» corresponde a baseline; no se generaliza a las otras variantes, cuyos máximos completos siguen en el informe. C16 corresponde a S8-9948 paso 246; no es cruce del centro C1 ni se inventa contacto de pared en ese paso. El orden canónico y su RED físico fueron aprobados; el test avisa que su reproducción depende del orden de pares de esta versión de PhysX.

## Opciones presentadas al PO

| Opción | Alcance | Decisión del PO |
|---|---|---|
| A: relajar criterios | Cambiar las exigencias para aceptar P1. | No elegida; no se autoriza relajar criterios o umbrales ni modificar spec. |
| B: P2 | Implementar una simulación analítica propia y evaluarla con los mismos escenarios, criterios y umbrales, conservando P1 de respaldo. | **Elegida y autorizada**, con contrato completado por la adenda r2; implementación después de fusionar I4c. |
| C: optimizar P1 | Continuar trabajo de optimización de P1 y volver a medirlo. | No elegida como siguiente trabajo; no se autoriza otra campaña ni optimización de P1 dentro de esta PR. |

La decisión B responde a P1 fuera de los criterios: C17 unas 14 veces por encima del umbral, C10/C11 y C3 como limitaciones del motor. Las correcciones de adaptador aprobadas ya están incorporadas y la nueva alternativa se evaluará sin ajustar la prueba para aprobarla.

## Decisión B y alcance autorizado

El PO autoriza **P2/T4.5**, una simulación analítica propia, como alternativa evaluada separadamente. **P1 sigue como respaldo.** Se añade a T4.5 la [adenda r2 aprobada el 2026-10-23](../superpowers/plans/2026-10-20-squash-clash-first-ball.md#adenda-r2--contrato-de-p2-aprobado-por-el-po-el-2026-10-23), transcripción literal de los cuatro puntos: asentamiento/deslizamiento, tope y tiempo restante, límites superiores y fricción. Completa y prevalece sobre la redacción preliminar de esos puntos; no reescribe la spec ni cambia parámetros del preset.

**No se decidió** backend definitivo, nuevos umbrales/criterios, modificación de spec, congelación/promoción de variantes o preset, ni inicio de I5. La elección final se hará con los datos de P2 en un gate posterior. La autorización de P2 no selecciona por anticipado su implementación como backend definitivo ni lo declara aprobado.

## Secuencia posterior a la revisión de esta PR

1. **Ahora, I4c documental:** dejar esta PR lista para Claude y detenerse. No fusionar sin esa revisión; no crear la rama de implementación P2 ni cambiar código en I4c.
2. **Solo después de fusionar I4c, T4.5:** rama `firstball/i4d-p2-analytic` y PR de física propia; implementar `Core/Simulation/AnalyticBallSimulation.cs` y `AnalyticBallSimulationFactory.cs` conforme al plan/adenda. `Name => "P2-Analytic"`, `Timing = Exact`, `ImpactCapConfigurable = true`, `RunsInEditMode = true`.
3. **TDD y editor:** RED válido antes de cada comportamiento (impacto dentro del paso, esquinas dentro de ε, reposo/deslizamiento, tope, límites superiores y fricción), suites GREEN. Mismo experimento T4.3 con P2 en EditMode y en el editor, sin cambiar escenarios, criterios o umbrales; resultados experimentales separados de regresión.
4. **Después, una sola campaña nativa P2:** teléfono físico, No molestar, batería ≥80 %, bajo consumo desactivado, sin funda, térmico nominal inicial y sin herramientas durante ejecución. Verificar firma antes de instalar y renovar perfil si se instala el 2026-10-27 o después; no cambiar herramientas. Recuperar informe solo tras término.
5. **Observaciones menores de Claude:** en esa próxima campaña, imprimir fastMotionThreshold en encabezado nativo (relevante solo para P1) y guardar las 200 muestras individuales C17. No repetir I4b por ellas ni cambiar 20 calentamientos/200 medidas o el umbral p95 ≤1 ms.
6. **Entrega P2:** informe comparativo P1/P2 de 18 criterios y detenerse para Claude/PO. Sin elegir backend, congelar variantes, modificar spec o iniciar I5.

Este documento registra la decisión del PO y prepara su revisión documental. No contiene implementación P2, resultados P2 ni aprobación anticipada de su gate.

## Precisión aprobada tras revisión de Claude — 2026-10-23

Claude aprobó la PR #5 y propuso precisar el tratamiento de los impactos lentos contra paredes para que la semántica coincida con P1. El PO aprobó el agregado y autorizó fusionar I4c con merge commit e iniciar T4.5 después. Se añade literalmente al final del punto 1 de la adenda r2:

> En paredes, un impacto con velocidad normal menor que bounceThreshold anula la componente normal sin rebote. No existe estado de reposo en paredes.

No introduce reposo en paredes ni parámetro nuevo. El resto de los cuatro puntos se conserva; no cambian escenarios, criterios, umbrales o spec. La aprobación fue comunicada por el PO; no se atribuye una GitHub Review adicional. Después de fusionar y registrar el cierre, T4.5 seguirá la secuencia ya registrada en su rama y PR de física propias, hasta la entrega comparativa para Claude/PO. Backend definitivo pendiente e I5 no autorizado.
