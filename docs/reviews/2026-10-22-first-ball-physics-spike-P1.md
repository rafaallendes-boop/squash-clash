# I4b — P1 tras orden canónico de contactos

Estado: **orden canónico de c7da32e aprobado por Claude, comunicado por el PO el 2026-10-23; campaña nativa completa bajo condiciones aprobadas auditada e informe I4b completado para revisión de Claude y decisión del PO; ejecución detenida**. [PR #4 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/4), sin fusionar. I4b no cerrado; I4c, P2/T4.5 e I5+ no autorizados. PhysX sigue como candidato, sin elección de backend.

Código de las cinco ejecuciones: **da98fd4**, Unity 6000.3.27f1. Preset **c86243b15a97**, seed **20261020**, 10.053 lanzamientos de robustez por variante (17 fijos + 10.000 S8 + 36 S8d), 20 calentamientos y 200 mediciones C17. `fastMotionThreshold=0.01 m` en ProjectPhysics. Se mantienen escenarios, umbrales, geometría, preset, Reset y comparador. El único cambio de salida física es el orden de los contactos de cada paso, aprobado expresamente por el PO.

[Manifiesto de las cinco variantes nuevas](../validation/evidence/2026-10-23-i4b-editor-canonical-runs.json). Histórico `16ad653`: [informe íntegro](2026-10-23-first-ball-physics-spike-P1-pre-canonical.md), [manifiesto y cinco campañas](../validation/evidence/2026-10-22-i4b-editor-ccd-runs.json). También se conserva el [histórico anterior a CCD](2026-10-22-first-ball-physics-spike-P1-pre-ccd.md). No se sustituyen sus resultados.

Las suites de regresión y las campañas son poblaciones distintas: 1/1 en cada experimento significa ejecución completa y limpieza de recursos; no convierte fallos físicos en Pass. Los editores concurrentes usan worktrees aislados y actividad normal del Mac; los tiempos son **informativos**, sin comparación aislada ni certificación de C17 nativo.

## Diagnóstico completo antes del cambio de comportamiento

Código diagnóstico **1c47621**, adaptador aún idéntico al de `16ad653`. Dos campañas completas nuevas (baseline y speculative), no una extrapolación del primer ejemplo. [Manifiesto con cada divergencia, checkpoint y primer paso](../validation/evidence/2026-10-23-i4b-editor-contact-diagnostic-runs.json).

«Solo orden» exige igualdad de todos los estados (Step y seis float32 bit a bit) y del multiconjunto de **todos** los campos de ContactEvent, con multiplicidad y bits exactos, incluidos NaN. Es un diagnóstico adicional de fallos ya declarados; no reordena las trayectorias ni participa en el comparador o el veredicto. Una diferencia de estado, valor o multiplicidad se clasifica como «estado/contactos distintos».

| Variante | Criterio | Solo orden | Estado/contactos distintos | Trayectoria de estado distinta (incl. longitud) | Total fallos |
|---|---|---:|---:|---:|---:|
| [baseline](../validation/evidence/physics-spike-P1/editor-1c47621-baseline-contact-diagnostic.md) | C5 | 0 | 0 | 0 | 0 |
| [baseline](../validation/evidence/physics-spike-P1/editor-1c47621-baseline-contact-diagnostic.md) | C6 | 0 | 0 | 0 | 0 |
| [baseline](../validation/evidence/physics-spike-P1/editor-1c47621-baseline-contact-diagnostic.md) | C7 | 3 | 0 | 0 | 3 |
| [speculative](../validation/evidence/physics-spike-P1/editor-1c47621-speculative-contact-diagnostic.md) | C5 | 0 | 0 | 0 | 0 |
| [speculative](../validation/evidence/physics-spike-P1/editor-1c47621-speculative-contact-diagnostic.md) | C6 | 138 | 1 | 1 | 139 |
| [speculative](../validation/evidence/physics-spike-P1/editor-1c47621-speculative-contact-diagnostic.md) | C7 | 183 | 0 | 0 | 183 |

## C16: procedencia del máximo

`MaxPenetration` no cambia: mismos cinco planos y dominio de altura, sin criterio nuevo. El máximo baseline de **11,3993931 mm** está en **S8-9948 robustez, paso 246, RightWall**. Es **impacto lateral, no reposo**: rapidez 1,77920949 m/s y vx=−1,69069779 m/s. En ese paso solo se reporta Floor; RightWall aparece como impacto no persistente en 247.

| Paso | Centro x (m) | vx (m/s) | Contactos reportados |
|---|---:|---:|---|
| 246 | 0,008600607 | −1,69069779 | Floor no persistente; sin RightWall |
| 247 | 0,01634964 | +0,9298841 | Floor persistente; RightWall no persistente |

El centro sigue positivo en 246: hay penetración de la esfera frente al plano, sin cruce del centro C1. La traza del paso siguiente confirma el impacto lateral; no se inventa contacto en 246. [Traza anterior al orden](../validation/evidence/physics-spike-P1/editor-1c47621-baseline-contact-diagnostic.md) y [traza nueva con estados/bits/contactos en 245–247](../validation/evidence/physics-spike-P1/editor-da98fd4-baseline-canonical.md).

Speculative tiene otro máximo: S8-3821 robustez, paso 193, Floor, 0,612672448 mm, solo contactos persistentes. La rapidez es 5,8087306 m/s: contacto sostenido en movimiento, no se deduce reposo de `Persistent`. Los cinco argmax y contextos completos están en el [manifiesto](../validation/evidence/2026-10-23-i4b-editor-canonical-runs.json).

## Orden canónico y TDD

[RED→GREEN, hashes y regresión](../validation/evidence/2026-10-23-i4b-contact-order-tests.json). El RED usa dos instancias PhysX reales con historias distintas y el mismo checkpoint; observa los estados iguales y contactos intercambiados antes de la corrección. No fabrica una permutación en el comparador. Los datos de la reproducción y el alcance de las suites constan en el manifiesto.

El adaptador ordena la lista al terminar correctamente Simulate. Claves: (1) TimeInStep numérico ascendente, tiempo desconocido NaN al final, sin inventar tiempo; (2) SurfaceId en su orden enum Floor, FrontWall, LeftWall, RightWall, BackWall; (3) Persistent false antes de true. Desempate lexicográfico: Step, Timing, bits sin signo de TimeInStep, Point, UV, Normal y ApproachVelocity. Así se distinguen ±0 y payloads NaN; eventos idénticos en todos sus bits son indistinguibles y no requieren estabilidad. Sin IDs de instancia ni orden de callbacks, sin eliminar/agrupar contactos o modificar valores.

`TrajectoriesBitwiseEqual` y C7 permanecen intactos. Tampoco se reinicia o recrea el cuerpo en Reset, se cambia el integrador ni se compensa la gravedad. C17 incluye el coste de ordenar dentro de Step; la clasificación diagnóstica de la campaña no está dentro del benchmark.

## Baseline nueva: 18 criterios

| ID | Medición | Veredicto | Gate editor |
|---|---|---|---|
| C1 | fallos=0; evaluados=10053; no evaluables=0; atravesamientos=0; salidas superiores=2341; seed=20261020; S8=10000; S8d=36 | Pass | sí |
| C2 | fallos=0; evaluados=67737; no evaluables=0 | Pass | sí |
| C3 | fallos=5; evaluados=3501; no evaluables=0; no aplica (sin pasos de esquina, fuera de población)=6538; máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839869, paso=218, rapidez antes=0.06392491 m/s, después=0.06555371 m/s | Fail | sí |
| C4 | fallos=0; evaluados=4; no evaluables=0; impactos repetidos=0 | Pass | sí |
| C5 | fallos=0; evaluados=117; no evaluables=0 | Pass | sí |
| C6 | fallos=0; evaluados=10017; no evaluables=0 | Pass | sí |
| C7 | fallos=0; evaluados=18753; no evaluables=0; Measured=18753; NotApplicable=1279; NotEvaluable=0 (no aplica fuera de población) | Pass | sí |
| C8 | fallos=0; evaluados=2; no evaluables=0 | Pass | sí |
| C9 | fallos=0; evaluados=2; no evaluables=0 | Info | no |
| C10 | fallos=9; evaluados=9; no evaluables=0; error máximo mm=13.6164427 | Fail | sí |
| C11 | fallos=3; evaluados=6; no evaluables=0 | Fail | sí |
| C12 | fallos=0; evaluados=11; no evaluables=0 | Info | no |
| C13 | fallos=0; evaluados=35; no evaluables=1; mínimo=-0.0002015136; mediana=-1.69331327E-06 | NotEvaluated | no |
| C14 | máximo impactos no persistentes/paso=2; tope configurable=False; parámetro maxImpactsPerStep no operativo con P1 | Info | no |
| C15 | fallos=0; evaluados=2; no evaluables=0 | Info | no |
| C16 | penetración máxima=11.3993931 mm; muestras persistentes=1147120 (todas las trayectorias ejecutadas) | Info | no |
| C17 | n=200; warmup=20; median=33.391100; p95=34.927300; p99=35.498200; max=35.722500 ms; percentiles nearest rank | Info | no |
| C18 | RunsInEditMode=False; timing=Estimated | Info | no |

[Baseline íntegra](../validation/evidence/physics-spike-P1/editor-da98fd4-baseline-canonical.md). C3 ≤1e−5, C10 ≤1 mm experimental, C17 p95 ≤1 ms con 200 mediciones en el iPhone. C7 decide el gate; C9 informativo.

## Cinco variantes nuevas

F = fallos. C1: 10.053; C5: 117; C6: 10.017. C7: 20.032 oportunidades = Measured + NotApplicable + NotEvaluable; Expected = Measured + NotEvaluable. NotApplicable fuera de población; NotEvaluable no aprueba.

| Variante | C1 F | C3 F/NE | C5 F | C6 F | C7 F | C7 M/NA/NE | C10 máx. mm | C11 F/6 | p95 editor ms (info) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| [baseline](../validation/evidence/physics-spike-P1/editor-da98fd4-baseline-canonical.md) | 0 | 5/0 | 0 | 0 | 0 | 18753/1279/0 | 13.6164427 | 3 | 34.927300 |
| [speculative](../validation/evidence/physics-spike-P1/editor-da98fd4-speculative-canonical.md) | 0 | 17/0 | 0 | 1 | 0 | 18779/1253/0 | 77.24857 | 3 | 27.595400 |
| [offset05](../validation/evidence/physics-spike-P1/editor-da98fd4-offset05-canonical.md) | 0 | 7/0 | 0 | 0 | 0 | 18754/1278/0 | 13.6164427 | 3 | 36.710500 |
| [solver12](../validation/evidence/physics-spike-P1/editor-da98fd4-solver12-canonical.md) | 0 | 7/0 | 0 | 0 | 0 | 18753/1279/0 | 13.6164427 | 3 | 24.032000 |
| [no-ed](../validation/evidence/physics-spike-P1/editor-da98fd4-no-ed-canonical.md) | 0 | 5/0 | 0 | 0 | 3 | 18753/1279/0 | 13.6164427 | 3 | 18.628600 |

**Ninguna variante cumple todos los criterios de gate. No se promueve ninguna.**

### Divergencias después del orden canónico

Se clasifican todos los fallos restantes con el mismo diagnóstico completo. Las divergencias de estado se conservan como Fail y se reportan como límite del motor para esa variante y población; ordenar no las corrige ni las oculta.

| Variante | Criterio | Solo orden | Estado/contactos distintos | Trayectoria de estado distinta (incl. longitud) | Bits distintos en tramo común |
|---|---|---:|---:|---:|---:|
| baseline | C5 | 0 | 0 | 0 | 0 |
| baseline | C6 | 0 | 0 | 0 | 0 |
| baseline | C7 | 0 | 0 | 0 | 0 |
| speculative | C5 | 0 | 0 | 0 | 0 |
| speculative | C6 | 0 | 1 | 1 | 0 |
| speculative | C7 | 0 | 0 | 0 | 0 |
| offset05 | C5 | 0 | 0 | 0 | 0 |
| offset05 | C6 | 0 | 0 | 0 | 0 |
| offset05 | C7 | 0 | 0 | 0 | 0 |
| solver12 | C5 | 0 | 0 | 0 | 0 |
| solver12 | C6 | 0 | 0 | 0 | 0 |
| solver12 | C7 | 0 | 0 | 0 | 0 |
| no-ed | C5 | 0 | 0 | 0 | 0 |
| no-ed | C6 | 0 | 0 | 0 | 0 |
| no-ed | C7 | 0 | 3 | 3 | 3 |

Speculative C6 conserva S8-6719: estados 226/156, **prefijo común bit a bit igual**, sin diferencia numérica en ese tramo. En el paso 1 el mismo contacto Floor es Persistent=True en A y False en B; B cuenta antes el segundo bote y termina en 155, A en 225. Es un límite observado del ciclo Enter/Stay de PhysX frente a la historia de la instancia; no se modifica Reset ni el criterio y permanece Fail. No se atribuye una divergencia numérica a ese caso. Las diferencias numéricas que aparezcan en otras variantes se cuentan en la última columna y se conservan como Fail.

**No-ed C7 conserva tres divergencias numéricas de estado**, con multiconjunto completo de contactos igual: S8-109 C7 postcontacto (checkpoint 50, paso 127), S8-3429 C7 k30 (checkpoint 30, paso 101), S8-3429 C7 postcontacto (checkpoint 9, paso 101). Se reportan como límite observado del motor con Enhanced Determinism desactivado; quedan en Fail. El orden canónico no modifica el orden interno del solver ni corrige esas diferencias. No se generaliza este resultado a baseline ni se cambia Reset.

### C3: máximos con impacto y solo persistentes

Umbral y población global conservan ambos. Máximos medidos y rapidez antes/después; conteos Measured/NotApplicable/NotEvaluable completos en cada informe.

| Variante | Con impacto no persistente | Solo persistentes |
|---|---|---|
| baseline | máximo entre casos medidos: S4 Measured, ganancia=0, paso=21, rapidez antes=13.5786324 m/s, después=12.3445759 m/s | máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839869, paso=218, rapidez antes=0.06392491 m/s, después=0.06555371 m/s |
| speculative | máximo entre casos medidos: S8-8469 Measured, ganancia=0.0273839571, paso=322, rapidez antes=0.06398784 m/s, después=0.06558204 m/s | máximo entre casos medidos: S8-9384 Measured, ganancia=0.0273839273, paso=344, rapidez antes=0.06382191 m/s, después=0.06550735 m/s |
| offset05 | máximo entre casos medidos: S8-3960 Measured, ganancia=0.0242687, paso=318, rapidez antes=0.0317222327 m/s, después=0.0510625839 m/s | máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839515, paso=307, rapidez antes=0.0640965551 m/s, después=0.06563096 m/s |
| solver12 | máximo entre casos medidos: S4 Measured, ganancia=0, paso=21, rapidez antes=13.5786324 m/s, después=12.3445759 m/s | máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273840241, paso=218, rapidez antes=0.06393062 m/s, después=0.06555627 m/s |
| no-ed | máximo entre casos medidos: S4 Measured, ganancia=0, paso=21, rapidez antes=13.5786324 m/s, después=12.3445759 m/s | máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839869, paso=218, rapidez antes=0.06392491 m/s, después=0.06555371 m/s |

## Clasificación histórica de Claude previa a c7da32e

Comunicada por el PO, sustituye a la preliminar y se transcribe tal cual; no se atribuye una GitHub Review:

- C1: adaptador/configuración. Resuelto con 0 cruces.
- C10: motor. Es el sesgo del integrador semimplícito frente a la referencia analítica (g·dt·T/2 ≈ 13,6 mm). No se compensa; se decide en I4c.
- C11: consecuencia de C10. Los puntos S9c, S9d y S9i quedan unos 13,6 mm por debajo, al borde de una línea.
- C3 residual: motor. Es el solver en contacto persistente en reposo (unos 6 cm/s); con impacto, la ganancia es 0. Sigue en Fail. Un posible piso de velocidad se decide en I4c.
- C13: motor (CCD de una pasada). Informativo, sin cambios.
- C7: adaptador. Expone un orden de contactos no canónico. Los estados coinciden bit a bit, pero el orden de los pares interno de PhysX depende de la historia de cada instancia.

La clasificación se conserva literal con el contexto baseline de la revisión; la tabla nueva informa separadamente cualquier divergencia de otras variantes, sin generalizar igualdad a todas ellas. C3/C10/C11 no se compensan ni se suavizan. [Cálculo C10](../validation/evidence/2026-10-22-i4b-c10-residual.json), [integrador PhysX oficial](https://github.com/NVIDIA-Omniverse/PhysX/blob/106.0-physx-5.4.1/physx/source/lowleveldynamics/src/DyBodyCoreIntegrator.h), [CCD oficial](https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/docs/AdvancedCollisionDetection.html#tuning-ccd). [Diagnóstico CCD previo](2026-10-22-first-ball-ccd-diagnostic.md). No se afirma inspección del binario interno de Unity.

## Revisión de Claude sobre c7da32e y clasificación vigente

Comunicada por el PO el 2026-10-23; no se atribuye una GitHub Review. Aprobados PhysXContactOrder, el ordenamiento en Step, el RED físico y la clasificación diagnóstica. El test DifferentHistories_SameCheckpoint_ReturnSameContactOrder documenta que su reproducción del RED depende del orden de pares de esta versión de PhysX y podría dejar de reproducirlo si Unity lo cambia.

- Baseline C5/C6/C7 pasan; las divergencias anteriores eran solo de orden y se resolvieron en el adaptador.
- no-ed, C7: motor/configuración. Son divergencias numéricas reales y confirman que Enhanced Determinism es necesario.
- speculative, C6 (S8-6719): motor/variante, terminación distinta con el mismo prefijo.
- C10/C11 y C3 residual: sin cambios (motor).
- C16: motor, falta el barrido CCD en la esquina suelo/RightWall con la pelota rodando (paso 246, sin contacto de pared; respuesta por despenetración en el 247). Sigue siendo informativo y queda anotado para el análisis de I4c porque afecta a la zona del nick. Este registro no inicia ni autoriza I4c.

## Condiciones de la última medición C17 en el editor

Baseline da98fd4, Unity 6000.3.27f1, preset c86243b15a97, seed 20261020, 20 calentamientos y 200 mediciones; p95 nearest rank **34,927300 ms** (34,9 ms). Campañas en worktrees aislados, tres editores al principio y dos después, con actividad normal del Mac. Sin control aislado de carga ni registro térmico del Mac. El histórico 16ad653 registró **24,120100 ms** (24,1 ms), con dos editores parcialmente concurrentes. Se documentan las condiciones y ambos valores; no se atribuye causa a la diferencia ni se interpretan o comparan con la medición nativa.

## Campaña nativa bajo condiciones aprobadas — entrega I4b

El PO confirmó literalmente: **«Prueba completa - Batería inicial 90%. Se cumplen las condiciones»**. Se registran cumplidas las condiciones manuales solicitadas: teléfono físico, No molestar, batería ≥80 %, bajo consumo desactivado, sin funda y sin capturas/duplicación/devicectl/otras herramientas durante la ejecución. No hubo herramientas del agente hasta este aviso de término. Es confirmación de ejecución y condiciones, no aprobación de los resultados ni decisión del gate.

Build **0.1.0+2fdede5 dev**, P1-PhysX, variante **baseline/ccdDyn-co1mm-it6-1**, configuración **c86243b15a97**, Unity 6000.3.27f1, iPhone17,1 físico, iOS 26.6.2. Misma build instalada, sin nuevo export, instalación ni cambio de código/configuración. Ejecución **COMPLETA**, UTC **14:14:06.4532240 → 14:25:16.8274640**. Informe recuperado de Documents/spike después del aviso del PO y copiado sin editar.

Térmico al inicio de la campaña: **nominal**. Al iniciar el benchmark C17: **fair**; al final: **fair**. **0 interrupciones de foco**. El térmico del benchmark es informativo según lo aprobado: no se añade requisito de nominal en C17, no invalida esta ejecución ni explica por sí solo el tiempo.

[Informe nativo íntegro](../validation/evidence/physics-spike-P1/iphone-2fdede5-P1-20261023-142516-831.md) · [Auditoría y condiciones](../validation/evidence/2026-10-23-i4b-native-compliant-run.json) · [Build/ProjectPhysics previo](../validation/2026-10-23-i4b-native-preflight.md). **11.517 bytes**, SHA-256 **87043764667f9018d6933493f83a494967c130316fbe24fabdde29cce2cc70fe**. Verificados 18 criterios únicos C1–C18, hash de build y configuración, variante, 200 mediciones C17, térmico inicial del benchmark e interrupciones. El encabezado nativo omite fastMotionThreshold; el informe corto previo documenta 0.01 m desde la configuración fuente usada por esa build, sin afirmar getter en runtime.

### 18 criterios de la campaña nativa aprobada para ejecución

| ID | Medición nativa | Veredicto | Gate |
|---|---|---|---|
| C1 | 0 fallos / 10.053; 0 atravesamientos; S8=10.000, S8d=36; seed=20261020 | Pass | sí |
| C2 | 0 fallos / 67.737 | Pass | sí |
| C3 | 5 fallos / 3.501; 6.538 no aplicables; ganancia máxima 0,0273839869, S8-9456 paso 218 | Fail | sí |
| C4 | 0 fallos / 4; 0 impactos repetidos | Pass | sí |
| C5 | 0 fallos / 117 | Pass | sí |
| C6 | 0 fallos / 10.017 | Pass | sí |
| C7 | 0 fallos / 18.753 medidos; 1.279 no aplicables; 0 no evaluables | Pass | sí |
| C8 | 0 fallos / 2 | Pass | sí |
| C9 | 0 fallos / 2 | Info | no |
| C10 | 9 fallos / 9; error máximo 13,6164722 mm | Fail | sí |
| C11 | 3 fallos / 6 | Fail | sí |
| C12 | 11 evaluados, 0 no evaluables | Info | no |
| C13 | 35 evaluados, 1 no evaluable / 36; mínimo −0,0002015136, mediana −1,69331327E−06 | NotEvaluated | no |
| C14 | máximo 2 impactos no persistentes/paso; tope no configurable | Info | no |
| C15 | 2 evaluados; instantes Estimated y contactos en pasos distintos, detalle íntegro preservado | Info | no |
| C16 | penetración máxima 11,3993931 mm; 1.147.120 muestras persistentes | Info | no |
| C17 | 20 calentamientos; 200 medidas; mediana 13,871800, p95 **14,322100**, p99 14,481400, máximo 14,818100 ms | **Fail** | sí |
| C18 | RunsInEditMode=False; Timing=Estimated | Info | no |

**C17: Fail**, p95 **14,322100 ms** frente al umbral aprobado **≤1 ms**. Se comprobaron 20 calentamientos, n=200 y cobertura 200/0/200 en el archivo; las muestras individuales no se persisten y no se recalculan los percentiles de manera independiente. No se interpreta ni compara esta medición con tiempos del editor. C3/C10/C11 también quedan Fail y C13 NotEvaluated; no se convierte ninguno en aprobado por haber terminado la campaña.

C16 conserva S8-9948 robustez, paso 246, RightWall, 11,3993931 mm sin contacto de pared en ese paso y respuesta por despenetración en 247. Informativo, registrado para el análisis posterior de I4c porque afecta al nick; ese análisis no se inicia. C5/C6/C7 pasan como comparación dentro del mismo binario/dispositivo, sin afirmar igualdad entre plataformas.

La corrida anterior con 75 % y funda se conserva íntegra debajo como histórico fuera de condiciones. No se usa para sustituir esta campaña ni se comparan sus tiempos para atribuir causas.

## Corrida nativa completa del 2026-10-23 — condiciones no conformes

El PO comunicó «Prueba completa, informe copiado, informe guardado» y autorizó recuperar el resultado después de terminar. Declaró **75 % de batería y funda puesta**, frente a ≥80 % y sin funda exigidos. No confirmó No molestar ni bajo consumo desactivado. El térmico nominal sí consta en el informe. Se conserva íntegra la corrida como evidencia, pero **no sustituye la campaña bajo las condiciones aprobadas**. No se atribuye el tiempo a la batería, la funda u otra causa ni se cambia su veredicto.

Build **0.1.0+2fdede5 dev**, P1-PhysX, variante **baseline/ccdDyn-co1mm-it6-1**, configuración **c86243b15a97**, Unity 6000.3.27f1, iPhone17,1 físico, iOS 26.6.2. Export limpio Release ARM64 firmado y metadata previamente comprobados; código de producción/física idéntico a c7da32e. UTC **13:20:23.0247000 → 13:32:22.3195200**, ejecución completa. Térmico **nominal → nominal**, también **nominal al iniciar C17**; **0 interrupciones de foco**. Sin herramientas del agente durante la ejecución; recuperación de Documents/spike posterior al aviso del PO. La actividad de herramientas del lado del PO no fue confirmada expresamente.

[Informe guardado íntegro](../validation/evidence/physics-spike-P1/iphone-2fdede5-P1-20261023-133222-324.md) · [Auditoría de 18 filas, metadata y condiciones](../validation/evidence/2026-10-23-i4b-native-canonical-runs.json) · [Build y ProjectPhysics previo](../validation/2026-10-23-i4b-native-preflight.md). Archivo de 11.523 bytes, SHA-256 **53208d4c848d556a9d73a542e4e570bc73a890872a8b47c3d6aa8ac2cc1a7a55**, copia exacta sin editar. El encabezado nativo no imprime fastMotionThreshold; el informe corto separado registra 0.01 m desde la configuración fuente de esa build, sin afirmar lectura de getter en runtime.

### 18 criterios de la corrida nativa

Son los veredictos de esta corrida, sin convertir ejecución completa en aceptación de las condiciones ni en decisión de gate.

| ID | Medición nativa | Veredicto | Gate |
|---|---|---|---|
| C1 | 0 fallos / 10.053; 0 atravesamientos; S8=10.000, S8d=36; seed=20261020 | Pass | sí |
| C2 | 0 fallos / 67.737 | Pass | sí |
| C3 | 5 fallos / 3.501; 6.538 no aplicables; ganancia máxima 0,0273839869, S8-9456 paso 218 | Fail | sí |
| C4 | 0 fallos / 4; 0 impactos repetidos | Pass | sí |
| C5 | 0 fallos / 117 | Pass | sí |
| C6 | 0 fallos / 10.017 | Pass | sí |
| C7 | 0 fallos / 18.753 medidos; 1.279 no aplicables; 0 no evaluables | Pass | sí |
| C8 | 0 fallos / 2 | Pass | sí |
| C9 | 0 fallos / 2 | Info | no |
| C10 | 9 fallos / 9; error máximo 13,6164722 mm | Fail | sí |
| C11 | 3 fallos / 6 | Fail | sí |
| C12 | 11 evaluados, 0 no evaluables | Info | no |
| C13 | 35 evaluados, 1 no evaluable / 36; mínimo −0,0002015136, mediana −1,69331327E−06 | NotEvaluated | no |
| C14 | máximo 2 impactos no persistentes/paso; tope no configurable | Info | no |
| C15 | 2 evaluados; instantes Estimated y contactos en pasos distintos, detalle íntegro preservado | Info | no |
| C16 | penetración máxima 11,3993931 mm; 1.147.120 muestras persistentes | Info | no |
| C17 | 20 calentamientos; 200 medidas; mediana 14,569850, p95 **14,923400**, p99 14,984100, máximo 15,201600 ms | **Fail** | sí |
| C18 | RunsInEditMode=False; Timing=Estimated | Info | no |

C17 se evalúa con el umbral aprobado **p95 ≤1 ms** y queda **Fail**. Se verificaron n=200, warmup=20 y cobertura 200/0/200 en el archivo; no se persisten las muestras individuales, por lo que no se recalculan los percentiles de manera independiente. El térmico al iniciar el benchmark es informativo, sin umbral nuevo. No se interpreta ni compara esta medición con los tiempos del editor.

C16 conserva la procedencia S8-9948, paso 246, RightWall, sin contacto de pared en ese paso y respuesta en 247; queda informativo para la revisión posterior de I4c, sin iniciarla. C3/C10/C11 siguen Fail en esta corrida. Baseline C5/C6/C7 pasan dentro del mismo binario y dispositivo; no se afirma igualdad bit a bit entre plataformas.

## Entrega y detención

**Informe I4b completado y entregado para revisión de Claude y decisión del PO.** La campaña nativa bajo condiciones aprobadas está completa y auditada; su C17 queda Fail. Las cinco campañas del editor, el RED físico, la clasificación de Claude, las verificaciones dev/prod/firma/metadata y ambas corridas nativas quedan trazables en sus manifiestos. La regresión de código previamente registrada es EditMode 188/188 y PlayMode 58/58; no se repiten suites por documentación/evidencia y el comentario del test posteriores.

**Se detiene la ejecución. I4b sigue abierto, PR #4 en borrador y sin fusionar.** No se elige backend, congela o promueve ninguna variante/preset, compensa física ni cambia Reset, escenarios, umbrales o criterios. I4c, P2/T4.5 e I5+ no autorizados. No se solicita otra campaña ni se implementa una optimización por cuenta propia. La revisión de Claude se coordina con el PO, sin mensajes externos por herramientas. [Decisiones y confirmaciones del PO](2026-10-21-first-ball-po-approvals.md).
