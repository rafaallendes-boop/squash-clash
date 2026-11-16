# First Shot 4.5 — protocolo de validación (aprobado por el PO el 16 nov 2026)

**Aprobación del PO (16 nov 2026, Chile).** A la pregunta de Claude «¿Apruebas el protocolo con mis recomendaciones y fusiono #61 y #62?», el PO respondió, literal: «Si, fusiona ambas.» Quedan aprobadas las recomendaciones de Claude de la §7 para las 13 decisiones abiertas. **Aprobar el protocolo no autoriza ninguna sesión:** la sesión 1 requiere autorización expresa del PO, y los hashes esperados por condición deben calcularse antes.

**16 nov 2026.** Redactado por un subagente Sonnet dirigido por Claude: **la revisión de Claude no es independiente** y se declara. Autorización del PO (16 nov): «Protocolo ahora, sesión después». **Ninguna sesión, build, instalación ni acceso al iPhone está autorizado.** No modifica spec, plan, escenarios, umbrales ni criterios; no introduce umbrales. Las métricas son descriptivas y nada se vuelve umbral definitivo sin decisión del PO (plan, Tarea 4.5 Step 4, l. 727). Plantilla de registro: [first-shot-validation.md](../validation/first-shot-validation.md). Lo que las fuentes no definen se marca **[INDEFINIDO]**; los valores propuestos por Claude se marcan **[PROPUESTA]**.

## 1. Requisitos de las fuentes que se trasladan

| Requisito | Fuente |
|---|---|
| Calibrar la latencia (4.1) **antes** de cualquier medición de H3 | Plan 4.5 Step 1 (l. 722); spec v0.3 §7.1.5 (l. 371+); propuesta §7.12 |
| Al menos dos sesiones en días distintos para H3 entre sesiones | Plan 4.5 Step 2 (l. 723); spec v0.3 §17.B H3 |
| Primeros 10–15 intentos de cada sesión, aparte, como ampliación y no reemplazo | Plan 4.5 Step 2; propuesta C6 (l. 207, 221) |
| Bloques alternos V / T3 puro con los mismos seis lanzamientos, tareas y nivel de ayuda; T3 frente a T3-suave aparte; calentamiento separado | Plan 4.5 Step 2; propuesta §7.9–10; spec v0.3 §6.3 (l. 244+) |
| Plantilla con preguntas del PO §6, H2, H3, H3b, H3c, H4, invariantes §17.A 1, 2, 3, 5, 10, 12, 14 y decisión final | Plan 4.5 Step 3 (l. 726); propuesta §6 (l. 168–179), §5 fila S4 |
| Antes de cada bloque: control, preset, hash, 0 pendientes, Jugando, mano, ayuda; `repeat` 0 temporal sin guardar; un bloque cada vez; hash de recepción y esperado externo en igualdad literal | Plan l. 792 y D4.7 (l. 928–957); predeclaración S2 §3.2 |
| Restauración de P0 en proceso nuevo y cotejo de los tres archivos | Plan D4.7 «Cierre futuro» (l. 958); predeclaración S3 §8 |
| Perfil vigente y sesión completa, restauración incluida, dentro de su vigencia | AGENTS §8; predeclaración S2 §2.2 |
| El PO decide: continuar a First Rally, ajustar (incluido añadir B o C) o volver | Propuesta §5, fila S4 |

## 2. Calibración de latencia (4.1): hay que hacerla

La existente **no sirve**: 4.1 solo se implementó y se probó con retardos sintéticos; el informe S4 dice «Sin calibración física en esta entrega» y deja «Calibración física y efecto del offset en el iPhone» sin evaluar ([S4 timing](2026-11-11-first-shot-s4-timing.md), l. 42 y 155). Además la confirmación vale **«solo para esta sesión»**, no guarda preset y el reinicio vuelve a compensación 0 (l. 92+, puntos 4 y 6). Por tanto: se calibra **al inicio de cada sesión**, sobre la **misma build** de las mediciones, y de nuevo tras cualquier relanzamiento. Claude añade que la build vigente cambió el audio tras escribirse 4.1 (AudioScape, música, «Sonido» silencia también el clic de calibración, sesión que mezcla con otras apps: [first-run](2026-11-15-first-run.md) §3 de decisiones; [device-robustness](2026-11-16-device-robustness.md) §8), motivo adicional para no heredar nada.

Método ya predeclarado (S4 timing, l. 42 y 92+): ruta/volumen declarados por el PO; explicación y tres preguntas de identificación; tres series de 4 pulsos de familiarización + 8 medidos, período 1 s; sin eliminar outliers; media, desviación por serie, rango de medias, IC95 % entre series; pregunta de estabilidad antes de ver números; confirmación explícita separada. Es un **offset perceptivo conjunto**, no latencia del iPhone (plan 4.1; R2-5). Se advierte la asincronía media negativa (Repp 2005) y que su traslado a interceptar la pelota no está demostrado. Fuera de −100…100 ms no se aplica: se eleva al PO. El offset distinto entre sesiones se registra como confusor de H3 (**[PROPUESTA]**, juicio de Claude).

## 3. Diseño de las sesiones

- **Sesiones:** dos, en días distintos (§17.B). **[INDEFINIDO]**: el número exacto, el intervalo y la cantidad de intentos por bloque no están en las fuentes; D4.7 pide predeclarar «orden y número de recepciones por bloque [EXPERIMENTO]» al autorizar. Lo siguiente es **[PROPUESTA]**.
- **Configuración fija en todos los bloques:** build de `main` vigente, P0 (SL1, V100, cámara fija, M2-4, P2), 0 pendientes, sin cámara lenta, ayuda **completa** (D1 del 12 nov: defecto), una mano declarada, tarea fija predeclarada. Defaults `06fed4bd4ac9`/121 y P0 efectivo `0a3d3262a79d` (AGENTS §5) **se verifican al arrancar, no se presumen**; los hashes esperados por condición se calculan en Unity desde el JSON real antes de la sesión y se fijan en Git (S2 §3.2).
- **Orden por sesión:** calibración → calentamiento (6 recepciones, SL1–SL6, V100, fuera de denominadores) → cuatro bloques de 12 (dos rondas SL1–SL6): sesión 1 V-T3-T3-V, sesión 2 T3-V-V-T3 (contrabalanceo, por analogía con la alternancia entre sesiones de §6.3) → bloque T3-suave de 12 (H3c) → bloque de tareas H2 de 12 con rotación predeclarada de potencia en tercios y zonas D2.
- **Primeros 10–15 intentos:** se analizan aparte los primeros 15 intentos medidos de la sesión **y** los de calentamiento, por separado. **[INDEFINIDO]** si C6 cuenta el calentamiento; se informan ambos y decide el PO.
- **T3 contra V:** misma geometría ⇒ mismo golpe (§17.A.12 con aclaración D36); se informan por separado contactos, al aire/tarde y Δ. T3-suave nunca se mezcla con la comparación V/T3.
- **Duración [PROPUESTA, estimación de Claude]:** unos 90–110 min por sesión con el PO (calibración ~10, calentamiento y 4 bloques ~35, T3-suave y H2 ~15, prechecks/preguntas ~25, descansos); el operador necesita ~20 min antes y ~15 para restaurar. Referencia: 24 recepciones con preguntas ocuparon «unos 20 minutos» en S3 (predeclaración S3, adenda C1). El PO puede parar en cualquier momento sin completar cuotas.
- **Un bloque cada vez**, frontera por la línea `dev-control` con `devOpen=false`, cuota = N `LaunchId` consecutivos, sin repetir ni sustituir hasta lograr contactos (S2 §3.2.4 y D4.7). Pregunta de comprensión previa y «Explicación previa» de D4.7 antes del primer bloque.

## 4. Capas nuevas de la build (menú, tutorial, pausa, música, zurdo)

Fuentes: [first-run](2026-11-15-first-run.md), [device-robustness](2026-11-16-device-robustness.md), `FirstRunFlow.cs`. En una build DEV el arranque abre el menú; **«Práctica (dev)»** sale a la cancha de práctica (batch mode conserva el arranque directo).

| Capa | Estado propuesto durante la sesión |
|---|---|
| Menú / dificultad | Entrar con «Práctica (dev)»; la dificultad no influye en práctica. Se registra. |
| Tutorial | No aparece en práctica. El PO lo ve una vez antes («¿Cómo se juega?») como comprensión previa; **su texto no sustituye** la explicación de D4.7. |
| Tarjeta de pausa / interrupciones | Solo existe en partido; en práctica rige la capa anterior. Toda interrupción (llamada, bloqueo, segundo plano) corta el bloque y se trata como en S2 §7. |
| Música | Calla en práctica y en pausa. **[PROPUESTA]** dejarla como esté y registrarla; comprobar de oído que no suena. «Sonido» **debe estar en sí** (silencia también contacto y clic de calibración). |
| AudioScape (sonidos de cancha) | Activo como en producción; **no oído nunca por el PO** (informe first-run): se anota si tapa Perfect/Valid. |
| Mano zurdo | Solo cosmética; las métricas siguen registrando `declaredHand`. **[PROPUESTA]** misma mano en ambas sesiones. |
| Modo práctica DEV | Obligatorio (selección de SL, V/T3, T3-suave, ayuda y tareas viven en el panel DEV; §7.9). Se mide con DEV **cerrado**. |

## 5. Registro y exportación

Por intento, desde el JSONL del juego (`Documents/metrics`): `shot-plan`, `shot-gesture`, `shot-order`, `shot-presentation`/`-discrepancy`, `shot-causal`, `shot-aid-cutoff`, `practice-task`/`task-captured`, `latency-calibration`, `dev-control` (nombres en `Assets/`; **[INDEFINIDO]** lista completa vigente, el operador la confirma al exportar). Exportar con `xcrun devicectl device copy from … --domain-type appDataContainer --source Documents/metrics` antes de cualquier reapertura (retención 30 archivos/50 MB por carpeta; S2 §2.3). Preguntas literales **antes** de ver resultados. Datos del iPhone no se versionan (AGENTS §3).

Hipótesis y métricas (propuesta §6; spec v0.3 §17.B): **H2** error de mira en cm, potencia involuntaria, acierto de zona del primer bote; **H3** dispersión de Δ y tasa de fallos de timing entre sesiones, con latencia calibrada, más juicio «el timing se juega»; **H3b** ya medida con bots en el Mac ([causal](2026-11-13-first-shot-b4-followups-h3b.md) Parte B; **no hay Pass**; juicio del PO) y en iPhone solo observación descriptiva; **H3c** tasa de al aire y valoración T3 frente a T3-suave; **H4** el PO explica causa y categoría sin el panel. **«Bajan», «mejora» y «aprende» no tienen umbral [INDEFINIDO]**: se informan tablas y el PO juzga. Invariantes §17.A: 1, 2, 3, 5, 10, 12, 14 (qué se comprueba en el log, qué en el Mac y qué no es observable en iPhone, en la plantilla).

## 6. Restauración y cierre

Perfil: vence **2026-11-18T13:50:05Z (10:50:05 Chile)**; sesión completa y restauración terminan antes (AGENTS §8). Respaldo nuevo y SHA de `P0-calib-1.json`, `Po-1.json`, `last-active.txt`; cotejo antes y después de instalar; al terminar, exportar, restaurar ajustes del teléfono, proceso nuevo en P0, hash y 0 pendientes y cotejo byte a byte **también después del arranque** (S2 §8; S3 §8). P0-calib-1 no se promueve. Informe en PR sin fusionar, con respuestas literales y NotEvaluated intactos. El resultado es una **recomendación de Claude y decisión del PO**: continuar a First Rally, ajustar (incluido B o C) o volver.

## 7. Para aprobar por el PO

| # | Decisión abierta | Recomendación de Claude |
|---|---|---|
| 1 | **Calendario y perfil.** Dos días distintos exigen sesiones el 16–17 nov o renovar el perfil que vence el 18 nov 10:50 Chile. | Renovar el perfil y sesionar 2 días distintos tras renovar; no comprimir en el mismo día. |
| 2 | **Relación con el playtest integrado.** El PO reservó la aceptación de First Shot al playtest integrado (registro, 12 nov). ¿4.5 es ese playtest o un paso previo de medición? | 4.5 = medición estructurada previa; «aceptado» solo tras el playtest integrado. |
| 3 | **Sentido de «continuar a First Rally»**, pues R1–R6 ya están fusionados sin aceptación física. | Reinterpretar como «pasar a validar el rally integrado»; el PO lo confirma. |
| 4 | Tamaño de sesión: calentamiento 6 + 4 bloques × 12 + T3-suave 12 + H2 12. | Aprobar tal cual; acortar si el PO se cansa. |
| 5 | Nivel de ayuda de H3: completa en ambas sesiones; parcial/ninguna solo como bloque corto y separado. | Completa; parcial/ninguna fuera de H3, opcional en sesión 2. |
| 6 | Tarea de los bloques H3: una fija (columna central, altura media, potencia media, bote medio) frente a rotación por recepción. | Fija en H3; rotación solo en el bloque H2. |
| 7 | Contrabalanceo V/T3 entre sesiones (V-T3-T3-V / T3-V-V-T3). | Aprobar. |
| 8 | Calentamiento dentro o fuera de «primeros 10–15 intentos». | Informar ambos, separados. |
| 9 | Música, «Sonido», AudioScape y mano (§4). | Sonido sí; música como esté (callada en práctica); misma mano en ambas sesiones. |
| 10 | Incluir bloques de las 5 condiciones pendientes de S3 (Perfect/Valid, silencio, `feedback.haptics=0`, háptica de sistema, pausa) como anexo opcional. | Incluir solo si salen Perfect; si no, quedan NotEvaluated. |
| 11 | Pasada PROD corta (§17.A.10) al final de la sesión 2, como en S2 tanda 4. | Sí, separada de dev. |
| 12 | Quién opera (Claude con el PO presente, como S3, o Codex). | Claude con el PO presente; revisión independiente aparte. |
| 13 | γ 2,0 de la curva de potencia sigue **candidato, no adoptado**: ¿se observa en estas sesiones o solo en el playtest integrado? | Registrar SL5/SL6 y `Unreachable` descriptivamente; sin juzgar γ aquí. |
