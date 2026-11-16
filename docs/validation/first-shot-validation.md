# First Shot 4.5 — plantilla de registro de las sesiones de validación

Plantilla de la Tarea 4.5 Step 3 (plan, l. 726). **Una copia por sesión**, rellenada durante y después de ella; las celdas `…` se llenan con datos reales y nunca con valores estimados. Protocolo: [propuesta 4.5](../reviews/2026-11-16-first-shot-s4-4-5-protocol.md) (borrador, sin aprobar). Reglas: todas las métricas son **descriptivas**; no hay umbrales; un fallo nunca se registra como cero; ausencia = `NotEvaluated` con motivo (no cuenta como Pass); todos los intentos se conservan; las respuestas del PO se copian **literales** y no se reinterpretan con métricas posteriores. Los resultados no se convierten en umbrales sin decisión del PO.

## 0. Identificación de la sesión

| Campo | Valor |
|---|---|
| Sesión nº / fecha local / día distinto de la anterior (sí/no) | … |
| Operador / PO presente / revisión independiente | … |
| Build (`0.1.0+<commit> dev`), fuente `main` y head documental | … |
| Perfil incorporado: vencimiento UTC y fecha de comprobación | … (límite vigente: 2026-11-18T13:50:05Z) |
| Hora de inicio, fin de adquisición y fin de restauración (todas antes del vencimiento) | … |
| Defaults (hash/claves); P0 efectivo; SimulationKey; proyección heredada | esperado `06fed4bd4ac9`/121, `0a3d3262a79d`, `166a96e319c7`, `0d39e6016bd8` — **observado:** … |
| Respaldo: `P0-calib-1.json`, `Po-1.json`, `last-active.txt` (bytes y SHA-256, antes de instalar) | … (P0-calib-1 referencia `2f1295f2…874e4fd`) |
| Cotejo tras instalar y antes de preparar bloques | … |
| Ajustes del teléfono declarados por el PO (timbre, volumen, háptica del sistema, altavoz/sin Bluetooth) | … |
| Capas de la build: Sonido sí/no, Música sí/no (callada en práctica: confirmada de oído sí/no), mano diestro/zurdo, dificultad del menú, tutorial visto | … |
| Contexto: cámara lenta apagada, foco, térmico/batería, ruta de audio | … |

## 1. Calibración de latencia (4.1) — antes de cualquier medición de H3

Offset perceptivo conjunto; **no** es latencia del iPhone. Se repite cada sesión y tras cada relanzamiento.

| Campo | Valor |
|---|---|
| Respuestas literales: «¿Ves el destello?» / «¿Oyes el clic?» / «¿Entiendes que debes seguir el ritmo y que no estamos midiendo solo el dispositivo?» | … |
| Serie 1, 2, 3: pulsos medidos válidos, media (ms), desviación, intervalo de descanso | … |
| Rango de medias, IC95 % entre series, validez (cámara lenta, foco, duplicados, faltantes) | … |
| Literales: «¿Pudiste seguir un ritmo estable en las tres series?» / «¿Percibiste el destello y el clic juntos o separados?» | … |
| Advertencia dada (asincronía media negativa, Repp 2005) y «¿Confirmas aplicar este offset conjunto solo para esta sesión?» | … |
| Offset aplicado (ms) o no aplicado y motivo; `timing.latencyCompMs` heredada tras reiniciar | … |

## 2. Precheck por bloque (un bloque cada vez)

| Bloque | Condición (V100 / T3 puro / T3-suave) | SL y rondas | Ayuda | Tarea | Mano | Hash esperado | Hash observado (recepción) | Pend. 0 / Jugando / foco | Aceptado o rechazado y motivo | Línea `dev-control` (`devOpen=false`) y `LaunchId` inicial |
|---|---|---|---|---|---|---|---|---|---|---|
| Calentamiento | … | … | … | … | … | … | … | … | … | … |
| H3-1 … H3-4 | … | … | … | … | … | … | … | … | … | … |
| T3-suave | … | … | … | … | … | … | … | … | … | … |
| H2 | … | … | … | … | … | … | … | … | … | … |

Reglas: sin cambios de configuración durante un bloque; un reinicio abre segmento nuevo y no reutiliza ids; un hash distinto bloquea el avance (no se recalcula el esperado). Interrupciones, adelantamientos y extras: **…**

## 3. Resultados objetivos por bloque (todas las recepciones, preparación y extras fuera de cuota)

| Bloque | Cuota prevista / completada | Contactos | Perfect | Valid | Al aire | Tarde | Cancelaciones / Tap / C-A | Sin orden / inalcanzable / otras | Δ (media, dispersión, mediana) | Error de mira (cm, n/p50/p95/máx) | Potencia involuntaria | Acierto de zona del primer bote |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| … | … | … | … | … | … | … | … | … | … | … | … | … |

Denominadores explícitos; denominador cero = `NotEvaluated`. Informar SL5/SL6 con todas las omisiones/tempranas/tardías (S2-TIME-01 es observación conocida).

## 4. Hipótesis

### 4.1 H2 — apuntar y dar potencia con intención (M2-4; tareas 4.2)
Error en cm (pedido, mira congelada, objetivo factible, impacto en frontis), potencia pedida/congelada/saturación, primer bote por zona (3×3, tercios, bote corto/medio/profundo; bandas D2 de observación, **no** criterios); `SpeedRaised`/`TargetLowered` separados de P. Saturación sobre todos los intentos. **Datos:** … **Valoración 1–5 del PO** — control: …; comodidad de la mano: …; «se siente squash»: … **Juicio del PO (literal):** …

### 4.2 H3 — el timing mejora con la práctica (con latencia calibrada)
| Sesión | Offset aplicado | Condición | Intentos | Dispersión de Δ | Tasa de fallos de timing (al aire + tarde) | Observaciones |
|---|---|---|---|---|---|---|
| 1 | … | T3 puro / V100 | … | … | … | … |
| 2 | … | T3 puro / V100 | … | … | … | … |

Comparación entre sesiones: tablas lado a lado; el offset distinto entre sesiones es confusor y se declara. **Primeros 10–15 intentos de la sesión (aparte, ampliación, no reemplazo):** intentos 1–15 medidos y calentamiento por separado; tendencia de Δ y del error de mira: …; orden de condición del primer bloque: … **Juicio del PO («el timing se juega»), literal:** …

### 4.3 H3b — anticiparse no es dominante
Referencia: experimento con bots en el Mac (no se repite aquí). Observación física solo descriptiva: intentos con suelta anticipada fuera de ±A declarada o inferida del registro (Δ < −A), su resultado y causal por separado; no se infiere intención. **Datos:** … **Estado de la hipótesis:** juicio del PO; sin Pass automático.

### 4.4 H3c — T3 frente a T3-suave
Tasa de al aire/tarde de cada uno por separado, mismo SL/tarea/ayuda/mano, bloque separado. **Datos:** … **Valoración del PO (frustración/habilidad), literal:** …

### 4.5 H4 — los errores se entienden sin el panel
Por cada fallo del bloque (sin mostrar el panel): causa y categoría dichas por el PO frente a la categoría registrada.

| Recepción | Dicho por el PO (literal) | Registrado | Coincide (sí/no/no recuerda) |
|---|---|---|---|
| … | … | … | … |

Conteo de coincidencias sobre todos los fallos (sin umbral): …

## 5. Preguntas del PO (§6 de la propuesta), respuestas literales

Se hacen al cerrar cada bloque o sesión, **antes** de mostrar resultados. Opciones neutras «No recuerdo» y «Nada que señalar» disponibles.

| # | Pregunta | Qué se observa o mide | Respuesta literal | Datos relacionados |
|---|---|---|---|---|
| 1 | ¿La pelota va donde apunto? | Error en cm y acierto de zona (H2) | … | … |
| 2 | ¿Distingo y controlo suave y fuerte? | Potencia involuntaria en tareas de potencia (H2) | … | … |
| 3 | ¿El timing es exigente pero aprendible? | Dispersión de Δ y tasa de fallidos por sesión y nivel de ayuda; «el timing se juega» (H3, H3b, H3c) | … | … |
| 4 | ¿Un perfecto se siente mejor? | Juicio y valoración 1–5 (si no hay Perfect: `NotEvaluated`) | … | … |
| 5 | ¿Tras 10–15 intentos siento que aprendo? | Tendencia de Δ y error de mira dentro de la sesión (extensión de H3) | … | … |
| 6 | ¿Entiendo por qué falló? | Causa y categoría sin panel (H4) | … | … |

Preguntas adicionales de las capas nuevas (comprensión del menú/tutorial, ¿se oye el sonido de cancha sobre el contacto?, ¿la música se oye en práctica?): … Preguntas heredadas de la sesión: …

## 6. Invariantes técnicas §17.A aplicables

| # | Invariante | Cómo se comprueba | Resultado |
|---|---|---|---|
| 1 | Una sola autoridad sobre la pelota | Pruebas EditMode/PlayMode y AutoQA de la build; en sesión, ningún atravesamiento observado | … |
| 2 | Consistencia (mismo estado inicial, mismo resultado) | AutoQA/determinismo en el Mac; en sesión, `LaunchId`/semilla/hash en cada registro | … |
| 3 | Predicción = simulación | Pruebas en el Mac; **no observable en sesión** salvo evidencia nativa; si falta: `NotEvaluated` | … |
| 5 | Contrato de contacto: ≤ 1 swing por recepción; cancelar no golpea; sin contacto tras segundo bote | Log (`shot-order`/`shot-presentation`) por recepción | … |
| 10 | Panel excluido de producción | `scripts/check-release.sh` prod (gitDirty=false); pasada PROD separada si se autoriza | … |
| 12 | Sin penalización duplicada del timing (D36: mismo contexto, misma geometría, distinto Δ ⇒ mismo golpe) | Prueba automática; en sesión, pares comparables si existen, y si no: `NotEvaluated` | … |
| 14 | Preset recuperado tras reiniciar; el registro incluye preset, build y semilla | Arranque nuevo en P0 y líneas del log | … |

## 7. Desviaciones, NotEvaluated y defectos

| Id | Qué ocurrió | Efecto en comparabilidad | Acción (registrar y detenerse; no corregir sobre la marcha) |
|---|---|---|---|
| … | … | … | … |

Condiciones de S3 aún pendientes (anexo **opcional**, solo si el PO lo aprueba): contraste Perfect/Valid y destello de Perfect, impacto en silencio, `feedback.haptics=0`, háptica del sistema desactivada, destello de Perfect en pausa. Estado: … Otras observaciones abiertas que se registran sin resolver: saturación con `pullFullMm` 35, N04/N04b, «aún mecánica», ritmo de rally, preferencia de dirección, posicionamiento del rival ante golpes cortos, `Unreachable` con γ 2,0 (candidato, no adoptado).

## 8. Exportación, restauración y cierre

| Paso | Resultado |
|---|---|
| Exportación de `Documents/metrics` (y `presets`) antes de reabrir la app: ruta local ignorada, archivos, SHA-256, líneas por proceso | … |
| Ajustes del teléfono repuestos por el PO | … |
| Proceso nuevo en P0: build, preset, hash efectivo, 0 pendientes, selección `last-active` | … |
| Cotejo byte a byte de `P0-calib-1.json`, `Po-1.json`, `last-active.txt` con el respaldo y la referencia inicial, **antes y después del arranque** | … |
| Informe de la sesión, PR sin fusionar | … |

## 9. Decisión del PO al terminar las sesiones

Se registra **literal**, separada de la recomendación de Claude. Los resultados no se convierten en umbrales definitivos sin esta decisión.

| Opción | Marcada |
|---|---|
| Continuar a First Rally | ☐ |
| Ajustar (incluido añadir B o C), con alcance: … | ☐ |
| Volver, a: … | ☐ |

Decisión literal del PO y fecha: … **Recomendación de Claude (separada, indicando si su revisión es o no independiente):** …
