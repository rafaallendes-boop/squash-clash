# I4b — prueba física P1 tras revisión de CCD

Estado: **cinco campañas nuevas del editor completas; detenido para revisión de Claude antes de volver al iPhone**. [PR #4 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/4), sin fusionar. I4b no está cerrado. I4c, P2/T4.5 e I5+ no autorizados.

Código de las cinco ejecuciones: **16ad653**, Unity 6000.3.27f1. Cada variante volvió a ejecutar 10.053 lanzamientos de robustez (17 fijos + 10.000 S8 con seed 20261020 + 36 S8d), comparaciones y benchmark de 20 calentamientos + 200 medidas. Configuración del preset **c86243b15a97** intacta. Corrección común de integración P1: `fastMotionThreshold=0.01 m`, registrada aparte en ProjectPhysics, porque el hash del preset no representa ese ajuste global. Ejes de las cinco variantes sin cambios. Escenarios, materiales, medidas físicas y umbrales intactos; elegibilidad C7 según decisión explícita del PO.

Las campañas son experimentos separados de la regresión: **1/1** acredita ejecución completa y limpieza, aunque sus veredictos sean Fail. Ninguna aprobación de backend ni gate se deduce de ello. Dos procesos del editor en worktrees aislados, aplicaciones del Mac activas: los tiempos del editor son informativos, no una comparación aislada de rendimiento y no certifican C17 en el iPhone.

[Manifiesto con 18 filas, XML/hash, fechas y duración de las cinco ejecuciones](../validation/evidence/2026-10-22-i4b-editor-ccd-runs.json). [Histórico anterior íntegro](2026-10-22-first-ball-physics-spike-P1-pre-ccd.md) y sus informes crudos conservados; no se sobrescriben ni se usan medianas derivadas en esta campaña nueva.

## CCD: diagnóstico y corrección separados

Reproducción mínima RED en escena local y por defecto, ambas con Simulate: esfera de radio 20 mm a 25 m/s, caja estática de espesor 1 m, 120 Hz, ContinuousDynamic. Centro a z=−0,116666675 en el paso 2; contacto en el 3. S1: centro detrás del frontis en 33–34, contacto 34. El modo del Rigidbody sí estaba configurado; el fallo se reproduce sin usar el adaptador.

El valor global heredado `m_FastMotionThreshold=3.4028235e+38` dejaba actuar el descarte geométrico de CCD. Se descartaron FreezeRotation y varios Simulate en un frame. Cambiar solo el ajuste a 0,01 m hizo pasar la reproducción; restaurarlo reprodujo los tres RED. Corrección en asset y ProjectSetup, sin parchear trayectorias ni Reset: **commit diagnóstico 8a1304d**, **corrección 492685b**. Tras corregir: mínimo z≈0,02; contacto mínimo paso 2 y S1 paso 33. No se sustituyó ContinuousDynamic por speculative.

[Diagnóstico, fuentes oficiales Unity/PhysX y trazas S1 30–36 antes/después](2026-10-22-first-ball-ccd-diagnostic.md). El valor 0,01 m es configuración de integración común, no tolerancia de aceptación ni variante nueva. Puede aumentar el coste y cambia contactos antes resueltos por penetración; no garantiza precisión ni todos los criterios.

## Baseline corregida: 18 criterios

| ID | Medición del editor | Estado | Gate en editor |
|---|---|---|---|
| C1 | fallos=0; evaluados=10053; no evaluables=0; atravesamientos=0; salidas superiores=2341; seed=20261020; S8=10000; S8d=36 | Pass | sí |
| C2 | fallos=0; evaluados=67737; no evaluables=0 | Pass | sí |
| C3 | fallos=5; evaluados=3501; no evaluables=0; no aplica (sin pasos de esquina, fuera de población)=6538; máximo global=S8-9456 Measured, ganancia=0.0273839869, paso=218, rapidez antes=0.06392491 m/s, después=0.06555371 m/s | Fail | sí |
| C4 | fallos=0; evaluados=4; no evaluables=0; impactos repetidos=0 | Pass | sí |
| C5 | fallos=0; evaluados=117; no evaluables=0 | Pass | sí |
| C6 | fallos=0; evaluados=10017; no evaluables=0 | Pass | sí |
| C7 | fallos=3; evaluados=18753; no evaluables=0; Measured=18753; NotApplicable=1279; NotEvaluable=0 (no aplica fuera de población) | Fail | sí |
| C8 | fallos=0; evaluados=2; no evaluables=0 | Pass | sí |
| C9 | fallos=0; evaluados=2; no evaluables=0 | Info | no |
| C10 | fallos=9; evaluados=9; no evaluables=0; error máximo mm=13.6164427 | Fail | sí |
| C11 | fallos=3; evaluados=6; no evaluables=0 | Fail | sí |
| C12 | fallos=0; evaluados=11; no evaluables=0 | Info | no |
| C13 | fallos=0; evaluados=35; no evaluables=1; mínimo=-0.0002015136; mediana=-1.69331327E-06 | NotEvaluated | no |
| C14 | máximo impactos no persistentes/paso=2; tope configurable=False; parámetro maxImpactsPerStep no operativo con P1 | Info | no |
| C15 | fallos=0; evaluados=2; no evaluables=0 | Info | no |
| C16 | penetración máxima=11.3993931 mm; muestras persistentes=1147120 (todas las trayectorias ejecutadas) | Info | no |
| C17 | n=200; warmup=20; median=22.451200; p95=24.120100; p99=25.168000; max=26.091300 ms; percentiles nearest rank | Info | no |
| C18 | RunsInEditMode=False; timing=Estimated | Info | no |

[Baseline íntegra](../validation/evidence/physics-spike-P1/editor-16ad653-baseline-ccd.md). Umbrales originales: C3 ≤1e-5, C10 ≤1 mm experimental, C17 p95 ≤1 ms con 200 mediciones **solo en iPhone**. C7 sigue decidiendo; C9 informativo.

## Cinco variantes regeneradas

F = fallos medidos. C1 población 10.053; C5 117; C6 10.017. C7 tiene **20.032 oportunidades**: Measured + NotApplicable + NotEvaluable. NotApplicable queda fuera de la población elegible; Expected = Measured + NotEvaluable. No evaluable no es aprobado. Contadores y umbrales contrastados automáticamente con los informes y el histórico.

| Variante | C1 F | C3 F/NE | C5 F | C6 F | C7 F | C7 M/NA/NE | C10 máx. mm | C11 F/6 | p95 editor ms (info) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| [baseline](../validation/evidence/physics-spike-P1/editor-16ad653-baseline-ccd.md) | 0 | 5/0 | 0 | 0 | 3 | 18753/1279/0 | 13.6164427 | 3 | 24.120100 |
| [speculative](../validation/evidence/physics-spike-P1/editor-16ad653-speculative-ccd.md) | 0 | 17/0 | 0 | 139 | 183 | 18779/1253/0 | 77.24857 | 3 | 17.265600 |
| [offset05](../validation/evidence/physics-spike-P1/editor-16ad653-offset05-ccd.md) | 0 | 7/0 | 0 | 0 | 3 | 18754/1278/0 | 13.6164427 | 3 | 29.577600 |
| [solver12](../validation/evidence/physics-spike-P1/editor-16ad653-solver12-ccd.md) | 0 | 7/0 | 0 | 0 | 3 | 18753/1279/0 | 13.6164427 | 3 | 25.834200 |
| [no-ed](../validation/evidence/physics-spike-P1/editor-16ad653-no-ed-ccd.md) | 0 | 5/0 | 0 | 0 | 3 | 18753/1279/0 | 13.6164427 | 3 | 19.497600 |

**Ninguna variante cumple todos los criterios de gate. No se promueve ninguna.**

### C3: impacto y solo persistentes

El umbral y la población global conservan ambos tipos de pasos. Los máximos siguientes son diagnósticos entre casos medidos, con rapidez antes/después; los conteos de las tres subpoblaciones constan en los informes crudos.

| Variante | Paso con impacto no persistente | Paso solo persistente |
|---|---|---|
| baseline | máximo entre casos medidos: S4 Measured, ganancia=0, paso=21, rapidez antes=13.5786324 m/s, después=12.3445759 m/s | máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839869, paso=218, rapidez antes=0.06392491 m/s, después=0.06555371 m/s |
| speculative | máximo entre casos medidos: S8-8469 Measured, ganancia=0.0273839571, paso=322, rapidez antes=0.06398784 m/s, después=0.06558204 m/s | máximo entre casos medidos: S8-9384 Measured, ganancia=0.0273839273, paso=344, rapidez antes=0.06382191 m/s, después=0.06550735 m/s |
| offset05 | máximo entre casos medidos: S8-3960 Measured, ganancia=0.0242687, paso=318, rapidez antes=0.0317222327 m/s, después=0.0510625839 m/s | máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839515, paso=307, rapidez antes=0.0640965551 m/s, después=0.06563096 m/s |
| solver12 | máximo entre casos medidos: S4 Measured, ganancia=0, paso=21, rapidez antes=13.5786324 m/s, después=12.3445759 m/s | máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273840241, paso=218, rapidez antes=0.06393062 m/s, después=0.06555627 m/s |
| no-ed | máximo entre casos medidos: S4 Measured, ganancia=0, paso=21, rapidez antes=13.5786324 m/s, después=12.3445759 m/s | máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839869, paso=218, rapidez antes=0.06392491 m/s, después=0.06555371 m/s |

## C5–C7 y Reset

Baseline pasa C5 y C6 completos. Conserva tres fallos de C7. El primer caso registrado es S8-109 postcontacto, primer desacuerdo en paso 127 (índice 77 del relanzamiento desde paso 50): estados previo y actual bit a bit iguales; A entrega LeftWall/Floor y B Floor/LeftWall en ese paso. Los puntos/normales de cada superficie del ejemplo también coinciden. En speculative, primeros ejemplos C6 S8-15 paso 9 y C7 S6 paso 25 muestran igualmente orden distinto con estados iguales. Es evidencia de esos ejemplos, **no clasificación automática de todas las divergencias**. El diagnóstico conserva una traza detallada del primer desacuerdo por criterio y hasta doce notas generales; los contadores son completos.

Se mantiene la comparación aprobada de la lista de contactos por superficie/paso: **no se ordena, agrupa ni suaviza para aprobar**. Determinar si esto es un defecto de la prueba o un límite de orden de callbacks de la integración requiere revisión de Claude y cualquier cambio de criterio requiere el PO.

Experimento independiente de Reset: 25/25 comparaciones iguales entre cinco objetivos, nueva/nueva, S6/nueva, S6/S6, S10/nueva y S10/S10. Historias con 8 y 653 contactos. [Datos y límites](../validation/evidence/2026-10-22-i4b-reset-history.json), [informe](../validation/evidence/physics-spike-P1/editor-reset-history-ccd.md). No demuestra ausencia de caché interna ni certifica todos los historiales/checkpoints. **Reset no se modificó**: no hay RED que justifique un parche del estado.

C7 ausente por salida superior limpia o horizonte: NotApplicable. Anomalías (cruce, no finitos, discontinuidad de pasos/contactos inválidos) u otra terminación temprana no inferida: NotEvaluable. No se excluyen silenciosamente. El informe muestra los tres estados y los 20.032 casos se contabilizan por variante.

## C10–C16: observaciones después de activar CCD

C10 baseline sigue fallando 9/9: 13,6151314–13,6164427 mm. Ya no es únicamente el contacto tardío de la configuración anterior. **Inferencia diagnóstica:** gravedad semimplícita seguida de un barrido lineal predice un sesgo vertical de 13,61628 mm para S9 (g=9,81; dt=1/120; T=4,98/15; impacto paso 40; resto 0,007 s). Desviación máxima respecto a los nueve errores: 0,0011486 mm. [Cálculo y observaciones](../validation/evidence/2026-10-22-i4b-c10-residual.json), [referencia oficial del integrador PhysX](https://github.com/NVIDIA-Omniverse/PhysX/blob/106.0-physx-5.4.1/physx/source/lowleveldynamics/src/DyBodyCoreIntegrator.h). Es consistencia con ese modelo, no inspección del binario interno exacto de Unity ni clasificación final de Claude. No se compensa velocidad, cambia gravedad/integrador o relaja 1 mm. C11 baseline falla S9c, S9d y S9i; geometría/huella/clasificador intactos.

C13 baseline: 35/36 ratios medidos, uno StepOnly no evaluable; mínimo −0,0002015136 y mediana −1,69331327e−6, próximos a cero, sin clamp. La [documentación oficial de CCD PhysX](https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/docs/AdvancedCollisionDetection.html#tuning-ccd) describe descarte del resto temporal con una pasada. Es una explicación compatible con la traza, **no se verificó el número de pasadas interno de Unity**. Se conserva Estimated y StepOnly honestos; no se inventa instante exacto ni umbral para C13.

C14 sigue sin tope configurable en P1. C15 baseline: S4 suelo paso 21/lateral derecha 22; S6 trasera 24/lateral izquierda 25; Estimated, no simultaneidad exacta certificada. C16 baseline alcanza 11,3993931 mm frente a planos aunque C1 no tenga cruces; es informativo y no se omite. C12–C16 no reciben criterios nuevos.

## Clasificación preliminar de Claude y evidencia nueva

La clasificación siguiente fue comunicada por el PO **como preliminar**; no es una aprobación de la corrección ni una GitHub Review atribuida a Claude.

| Grupo | Preliminar de Claude | Evidencia tras corrección, pendiente de su revisión |
|---|---|---|
| C1 | Adaptador/configuración probable | Reproducción A→B→A confirma defecto de configuración P1; baseline 0 cruces tras ajuste común |
| C10/C11/C13 y grandes C3 | Consecuencia probable de C1 | Desaparecen los cruces y grandes ganancias de baseline; C10/C11 persisten y C13 ahora mide resto próximo a cero. No asumir que corregir CCD resuelve todo |
| C3 residual en reposo | Posible límite del motor, reevaluar | Baseline: cinco fallos; máximo ≈0,027384 solo persistentes; máximos con impacto no persistente =0. Clasificación final pendiente |
| C5–C7 | Indeterminado | Baseline C5/C6 pasan; C7 falla tres. Primeros ejemplos detallados: orden de contactos con estados iguales. Sin cambio de Reset o comparación |
| C7 no evaluables | Definición de prueba | Decisión PO 5b implementada y contada en las cinco variantes; la elegibilidad ya no penaliza un fin superior/horizonte limpio |

No se etiqueta el conjunto de fallos como limitación definitiva de PhysX ni se elige backend.

## Decisiones del PO y estado nativo

[Decisiones registradas](2026-10-21-first-ball-po-approvals.md): robustez suspende por foco, registra UTC/duración/fase/progreso/contexto, recupera antes de 60 s y cancela al alcanzar el plazo; segundo plano y foco durante C17 cancelan. Conteo de interrupciones visible, ejecución válida solo si completa. Térmico al iniciar benchmark informativo. RED→GREEN: **EditMode 180/180, PlayMode 54/54**. [Manifiesto de la revisión](../validation/evidence/2026-10-22-i4b-review-fixes-tests.json).

**No se generaron builds ni instalaciones nuevas del iPhone, ni se ejecutó una campaña nativa en esta revisión.** La dev d14df6f y todos los intentos INCOMPLETO previos pertenecen al código/configuración anterior; se preservan como históricos, no certifican el código corregido ni C17. [Histórico nativo y diagnóstico de foco](2026-10-22-first-ball-physics-spike-P1-pre-ccd.md#resultados-en-el-iphone-16-pro).

Antes de la próxima instalación: comprobar vigencia; renovar si ocurre el **2026-10-27 o después** (perfil actual vence 2026-10-28T14:59:31Z), sin cambiar Xcode/herramientas. Próximo intento solo después de esta revisión de Claude: teléfono físico, No molestar, batería ≥80 %, bajo consumo desactivado, sin funda, térmico nominal al comenzar. Sin capturas, devicectl u otras interacciones de herramientas durante la ejecución; recuperar informe al terminar.

**Punto de detención:** entregar estos datos a Claude. PR #4 permanece borrador, sin fusionar; I4b/T4.4 no cerrado. I4c/P2/I5+ sin autorización.
