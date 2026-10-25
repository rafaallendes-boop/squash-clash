# Segunda revalidación física de S1

Fecha: 2026-10-25 (horas de registros en UTC). Rama `firstshot/s1-revalidation-2`, **solo documentación**. App instalada **5e218ea dev**, sin build ni instalación nuevas. Base **f7005be63218a573dd765c66b8b1e78c4d9f0213**.

Se recuperaron una adquisición completa de 120 s, seis intentos de distancia y las cuatro series de diez gestos a 30/20 ms. El PO respondió por separado a las cinco preguntas, acepta los saltos y declara ambas manos en las series sin desglose. **La distancia en mm queda NotEvaluated por falta de regla física.** P0-calib-1 restaurado y cotejado. Entrega documental para Claude **sin fusionar**; **S1 sigue sin aceptar y S2–S4/T8 sin autorizar**.

[Evidencia nueva, trazas y cálculos](../validation/evidence/2026-10-25-s1-revalidation-2.json) · [Autorización y observaciones Claude para S2](2026-10-25-first-shot-po-approvals.md#cierre-de-pr-19-y-segunda-revalidación-autorizados-por-el-po--2026-10-25).

## Base e identidad del producto

Claude aprobó PR #19 en **9d961743e6598b951dd9892f2aa21d2ed1417ee0**, según comunicación del PO. Antes de fusionar se comprobaron head local/remoto exacto, PR abierta/no borrador/fusionable y árboles limpios. **Merge f7005be63218a573dd765c66b8b1e78c4d9f0213**, padres a8240fe8277896015c37a66805d0d0c21c8a6b70 y head aprobado; árbol idéntico al aprobado. Main sincronizada. Worktree de visibilidad retirado sin `--force`, siete paths ignorados preservados en `Logs/archive-s1-reticle-visibility-20261025` del checkout principal: **31.292 archivos / 8.218.064.359 bytes**, inventario antes/después idéntico y **260 hashes de Logs/XML** coincidentes. Ramas remotas y visibilidad sin cambios.

Antes de preparar la sesión se verificaron **399 archivos versionados de Assets, ProjectSettings y Packages**: diff Git vacío entre **5e218ead0636d4eab23f4913b8f4911c4c117dd5** y main f7005be, SHA-256 locales coincidentes con las fuentes probadas de la build instalada. Los árboles Git de esos tres directorios se registran en el JSON; el manifiesto completo queda privado. BuildInfo generado no se regenera. No se ejecutaron suites, exports, builds ni instalaciones en esta sesión; no se atribuyen a ella verificaciones de la PR anterior.

## Preparación y trazabilidad de configuración

P0-calib-1, Po-1 y last-active respaldados antes de medir. Nuevo arranque de la app existente con **P0-calib-1 / 9977d9ffd7e5 / cero pendientes**. Desde panel se puso `launcher.repeatSeconds=0`, se aplicó/relanzó sin guardar y se cerró DEV. Antes del primer bloque se mostró literalmente:

> La pelota no se golpea en S1. Arrastra despacio para mover la mira; para fijarla, haz un gesto rápido y corto, como un golpe. Abajo verás el estado (Apuntando, Fijada o Inválida) y P, la potencia.

| Etapa | Lanzamiento | Hash | Pendientes | Configuración |
|---|---|---|---:|---|
| Preparación de 30 ms | 22:06:38.0264320Z | 00a501ad7066 | 0 | repeat=0, coverage=30 |
| Preparación de 20 ms | 23:11:36.7177380Z | a49c48e3d019 | 0 | repeat=0, coverage=20 |
| Restauración P0, primer arranque recuperado | 2026-10-25T23:24:50.6920160Z | 9977d9ffd7e5 | 0 | repeat=3, coverage=30 |

El JSON conserva **cada lanzamiento** desde el límite de esta sesión, incluidas repeticiones de P0 durante preparación/cierre, además del hash por gesto. Los **138 gestos / 4683 muestras** registrados conservan la configuración controlada de su condición: **118 a 30, 20 a 20**, cero pendientes, **ninguno excluido por cambio de configuración**. No se mezclan datos de 311cb11 (histéresis defectuosa), c5a7d1e (mira tapada) ni la comprobación previa de visibilidad de 5e218ea. Los archivos del contenedor privado pueden contener sesiones anteriores; la selección de registros usa build y límite temporal explícitos.

Incidencia de preparación: al intentar introducir20 por teclado la etiqueta mostró transitoriamente10 ms; se corrigió con el deslizador, se verificó20 y solo entonces se relanzó. **Sin lanzamiento10 ni nuevos gestos entre recuperaciones (118 antes/después)**. Se documenta la entrada remota, sin diagnosticar un defecto del producto ni mezclarla con las mediciones.

## Adquisición de 120 s y distancia

Se pidió el bloque principal con **izquierda dominante**, desviación autorizada respecto a la derecha pedida por el plan. El PO respondió literalmente «listo». No se presume una campaña diestra adicional ni se traslada a este bloque el uso de manos declarado después para las series.

| Adquisición | Completa | Duración | Muestras | Hash / pendientes |
|---|---|---:|---:|---|
| 22:13:52.9503240Z | Sí, elapsed | 120,014203167 s | 2449 | 00a501ad7066 / 0 |
| 22:15:21.0169180Z | **No**, system-cancel | 12,549327917 s | 28 | 00a501ad7066 / 0 |

La primera informa **50,611337 Hz entregados**, intervalo p50/p95 **16,7/37,6 ms**, hueco máximo **300,039 ms**, máximo **2 muestras/frame**; edad p50/p95/máxima **13,2/25,8/28,242 ms**. Sin duplicados, desorden, edades negativas ni overflow; histogramas de 0,1 ms. [TouchSamplingStats](../../Assets/SquashClash/Runtime/Input/TouchSamplingStats.cs) calcula DeliveredHz con intervalos positivos dentro de un mismo toque: **no es frecuencia del sensor ni 2449/120**. No se asumen 120 Hz. La segunda adquisición se conserva separada; no cuenta como completa ni se atribuye su interrupción. Hay **92 gestos** capturados entre ambas (91 Swing y 1 Tap), sin equipararlos al único intervalo completo.

Respuesta literal del PO sobre distancia y primeras series:

> Listo. Realicé las 3 pruebas. No tengo regla en la casa, pero veo bien la distancia. Por otro lado, los 10 suaves salieron bien. 10 fuertes, para mover ese rombo tengo que apuntar con el dedo y luego acelerar rapido, eso a veces hacia que el golpe salga debil porque levantaba el dedo. Pero reo que salió bien. Realiza la revisión

Se conservaron **6 intentos ruler-50mm**: **29,022 / 33,735 / 38,333 / 35,284 / 46,214 / 52,242 mm calculados**. Resolución reportada 1206×2622, Screen.dpi=460, fuente **Reported**, escala 0,055217389 mm/px. **No había regla física: mm físicos NotEvaluated**, sin PhysicallyVerified ni inferencia de precisión a partir de la estimación visual del PO. La distancia declarada 50 del protocolo es automática, no certifica el recorrido real. No se confirma la mano de estos intentos. Se mantiene el orden original, incluidos dos intentos de distancia posteriores a las series30.

## Potencia, fijación y comparación 30→20

Orden autorizado **30→20** en la misma sesión; posible sesgo de aprendizaje/fatiga. Los protocolos están dentro de DEV y lo cierran al armarse. Cuatro series completas, **trials 1–10**, todos Swing; el análisis no descarta intentos débiles o conservadores. El PO confirma la tanda 20 con «Listo!».

| Cobertura / serie | N | P mínima / mediana / máxima (%) | Conservadores por motivo | Fijada |
|---|---:|---|---|---:|
| 30 ms suaves | 10 | 0,000 / 0,000 / 0,000 | 4 few-samples, 3 sample-gap | 0/10 (0 %) |
| 30 ms fuertes | 10 | 0,000 / 1,977 / 20,523 | 1 few-samples, 2 sample-gap | 5/10 (50 %) |
| 20 ms suaves | 10 | 0,000 / 0,000 / 0,000 | 8 few-samples, 1 sample-gap | 0/10 (0 %) |
| 20 ms fuertes | 10 | 0,000 / 7,123 / 18,778 | 1 sample-gap | 9/10 (90 %) |

Separación de medianas fuerte−suave: **1,977 puntos porcentuales a 30**, **7,123 a 20**; los rangos se superponen en cero en ambas condiciones. Se incluye todo intento, también P=0. Mediana de potencia: punto medio de los dos valores centrales con N par.

Mano, respuesta literal a la pregunta sobre las series 30/20:

> mano izquierda y derecha, ambas 2, me imagine juando y use ambas manos

**Ambas manos, sin asignación por condición/intento.** La comparación no aísla el efecto de minCoverageMs: son gestos distintos, orden fijo y manos mezcladas. La mediana del pico de velocidad en fuertes fue **290,545 mm/s a 30** y **457,736 a 20**. Además, [AimModel](../../Assets/SquashClash/Core/Shot/AimModel.cs) no consulta minCoverageMs: no se atribuye a cobertura el aumento de Frozen. El PO nota diferencia, pero no expresa preferencia ni decide un nuevo default. **Se mantiene 30 ms.**

| Saltos al fijarse | Frozen N | p50 (m) | p95 (m) | Máximo (m) |
|---|---:|---:|---:|---:|
| Fuertes 30 | 5 | 1,661573 | 1,816027 | 1,816027 |
| Fuertes 20 | 9 | 1,639359 | 2,667281 | 2,667281 |
| Todos los intentos de esta sesión | 54 | 1,570968 | 2,166354 | 2,667281 |

Suaves: cero Frozen; percentiles de salto **NotEvaluated**, sin introducir ceros artificiales. Percentiles de salto: rango más próximo, índice ceil(N·p/100); con 5/9 Frozen el p95 coincide con el máximo. Son FreezeJumpMeters originales, **sin suavizado ni replay**. El total incluye adquisición/distancia y no se presenta como otra serie. El PO responde «si» a aceptabilidad; **no se activa la propuesta condicional de ajustar lookback/umbral/ganancia**. No hay ajuste implementado ni propuesto en esta entrega.

## H3/H4: datos para evaluación posterior, sin ajustes

Picos en **mm/s por gesto** de las series: distancia entre posiciones consecutivas / dt positivo × mmPerPx. Es un cálculo descriptivo offline sobre las trazas, no resimulación ni nueva medición física. La escala depende del DPI reportado, sin verificación física con regla. El JSON contiene también los picos y muestras de los otros 98 gestos.

| Intento | Suave 30 | Fuerte 30 | Suave 20 | Fuerte 20 |
|---|---:|---:|---:|---:|
| 1 | 88,901 | 249,043 | 103,496 | 536,766 |
| 2 | 105,274 | 478,156 | 112,462 | 270,144 |
| 3 | 106,668 | 278,337 | 92,973 | 428,144 |
| 4 | 136,205 | 263,105 | 74,386 | 595,230 |
| 5 | 160,333 | 851,869 | 117,407 | 455,988 |
| 6 | 273,047 | 717,200 | 135,816 | 459,483 |
| 7 | 100,250 | 248,673 | 71,354 | 318,787 |
| 8 | 121,750 | 351,527 | 88,877 | 443,049 |
| 9 | 103,495 | 302,754 | 129,023 | 487,678 |
| 10 | 130,922 | 172,920 | 68,246 | 532,385 |

Picos sobre **250 mm/s**: suaves 30 **1/10**, fuertes 30 **7/10**, suaves 20 **0/10**, fuertes 20 **10/10**. Un pico aislado no garantiza Frozen: siguen haciendo falta **dos intervalos consecutivos** sobre el umbral. Umbral, histéresis, lookback y ganancia intactos.

H4 usa posición final del **centro**: tope y=5 m exacto, o fuera de los límites verticales **y≤tin=0,48 / y≥out=4,57 m**. No se clasifica por el diámetro visual 0,84 m ni se presenta como replay del clasificador de huella.

| Grupo | En y=5 m | Fuera tin/out | Unión (tope o fuera) |
|---|---:|---:|---:|
| Suaves 30 | 0/10 | 0/10 | 0 % |
| Fuertes 30 | 2/10 | 4/10 | 40 % |
| Suaves 20 | 0/10 | 0/10 | 0 % |
| Fuertes 20 | 0/10 | 0/10 | 0 % |
| Total sesión | 16/138 (11,594 %) | 29/138 (21,014 %) | 21,014 % |

## Juicio del PO, literal y separado

| Pregunta | Respuesta literal |
|---|---|
| ¿Apuntas donde quieres? | si |
| ¿Entiendes siempre dónde apuntas? | si, pero para por ejemplo un drop tuve que intuir que era hacia abajo, pero igual es parte de la mecnaica |
| ¿El número responde a suave/fuerte? | si |
| ¿Los saltos de la mira al fijarse son aceptables? | si |
| ¿Notaste diferencia entre 30 y 20 ms? | si |

La observación sobre intuir un drop hacia abajo queda como comentario de comprensión de la mecánica; **S1 no ejecuta ese golpe**. Estas cinco respuestas no se convierten en aceptación global de S1 ni en autorización de S2. No se completa ni se sustituye el juicio del PO con los números.

## Restauración, límites y entrega

Se recargó el preset persistido mediante reinicio de la **misma app instalada**, restaurando **coverage=30/repeat=3 juntos**, sin lanzamiento intermedio 30/repeat=0 ni guardado de preset. El primer intento fue rechazado con CoreDevice10002/dispositivo bloqueado. Tras la autenticación local completada por el PO, el reinicio terminó correctamente. Nuevo arranque recuperado **2026-10-25T23:24:50.6920160Z**: **5e218ea dev / P0-calib-1 / 9977d9ffd7e5 / cero pendientes**; último registro comprobado **2026-10-25T23:25:43.7507310Z**. No se realizaron más gestos durante el cierre: los 138 registros coinciden con los recuperados al terminar 20.

Comparación directa de bytes del respaldo y recuperación final, **tres archivos idénticos**:

| Archivo | SHA-256 antes/después |
|---|---|
| P0-calib-1.json | 2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd |
| Po-1.json | 9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767 |
| last-active.txt | 5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087 |

Limitaciones declaradas: falta de referencia física de 50 mm; manos mezcladas sin desglose en series; orden30→20; segunda adquisición incompleta separada; incidencias de entrada remota/autenticación registradas sin atribuir defecto de producto no demostrado. Las diferencias descriptivas no cambian criterios ni autorizan un default. El PO decide con este informe.

Las tres observaciones de Claude para S2 quedan en aprobaciones: mira visual 0,84 m mayor que huella de clasificación; línea compacta puede tapar pelota trasera; prueba dependiente de GameViewSizes/internos frágiles ante actualización. **Sin código para ellas en esta tarea.**

Diff de la rama limitado a **AGENTS.md, aprobaciones, este informe y JSON nuevo**. Assets, ProjectSettings, Packages, scripts, spec, planes y criterios intactos. Defaults **00a501ad7066**, 41 valores heredados/proyección **0d39e6016bd8** y **SimulationKey 166a96e319c7** intactos. Evidencia y helpers privados en `Logs/evidence/s1-revalidation-2/`; no se versionan logs originales, identificadores personales, credenciales ni builds.

Entrega: **PR documental lista para Claude, sin fusionar**. **S1 sigue sin aceptar; S2–S4/T8 sin autorizar. Detenerse.** Comprobar siempre vigencia de perfil antes de instalar y renovarlo primero desde el 27; vence 2026-10-28T14:59:31Z. No hubo instalación en esta sesión.
