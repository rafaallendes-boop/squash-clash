# Reglas de trabajo — Squash Clash

Versión condensada del 14–15 nov 2026. Solo contiene lo vigente. El original completo, byte a byte, está en [docs/history/AGENTS-archive-2026-11-15.md](docs/history/AGENTS-archive-2026-11-15.md) (SHA-256 en [docs/history/README.md](docs/history/README.md)); la correspondencia regla por regla, en [docs/history/condense-report.md](docs/history/condense-report.md). Entre paréntesis, la entrada del archivo de la que viene cada regla.

## 1. Fuentes de verdad

- Spec v0.3, plan First Ball r2 (`docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md`), propuesta y plan First Shot r2 con sus adendas D1–D6, decisiones D30–D36 de la spec. (Reglas de trabajo, inicio; D36 del 13 nov)
- Decisiones del PO: `docs/reviews/2026-10-25-first-shot-po-approvals.md` (First Shot en adelante) y `docs/reviews/2026-10-21-first-ball-po-approvals.md` (First Ball). Si este archivo y el registro de aprobaciones difieren, manda el registro y se avisa al PO. (Reglas de trabajo, inicio)
- Informes de cada entrega en `docs/reviews/`, evidencia en `docs/validation/`.

## 2. Roles y forma de trabajo (modo MVP)

- **PO (Rafa)** decide alcance, aceptación y fusiones. **Claude**: revisión independiente, operación física cuando el PO lo designa y dirección de subagentes Sonnet cuando el PO lo pide; si Claude dirige el trabajo, su revisión no es independiente y se declara. **Codex**: implementación y zonas protegidas (§4). **Fable**: First Rally y pase visual, por autorización propia. (S3 «Claude, ahora», 9 nov; autorización de Fable, 11 nov; B4 con subagente Sonnet, 13 nov)
- **Modo MVP** (encargo del PO, 15 nov, literal: «informes de una página, citas literales solo cuando cambia una regla, fusionar con tests en verde y mi aprobación»): cada entrega lleva un informe de una página; las citas literales del PO se registran solo cuando cambian una regla o una autorización; se fusiona solo con tests en verde y aprobación explícita del PO.
- Un incremento = una rama/worktree y una PR, commits pequeños y coherentes, revisión antes de fusionar. No enviar mensajes a Claude por herramientas: la revisión se coordina con el PO. (Reglas de trabajo, «Desde I2»)
- Al fusionar: head local/remoto exacto, árboles limpios, merge commit con segundo padre = head aprobado; retirar worktrees sin `--force` preservando los ignorados. La fusión no acepta la etapa. (Cierre PR #17, 25 oct; PR #46/#47, 12 nov)
- Ante un defecto o una sorpresa: registrar y detenerse, sin corregir ni reintentar para obtener Pass. (ClockMap/CRP-01, 12 nov; B4-CQ-IMPL-01/02, 13 nov)

## 3. Reglas inamovibles vigentes

- No modificar spec, escenarios, umbrales ni criterios para hacer pasar resultados. (Reglas de trabajo, inicio)
- Una sola autoridad del estado de la pelota; juego y predicción aislados; configuración efectiva por lanzamiento; `DefaultPreset.json` es la única fuente de defaults. (Reglas de trabajo)
- P2-Analytic es el backend por defecto; P1 queda como respaldo configurable. (Gate de física, 23 oct)
- AX-1: coordenadas y cámara conservadas; LeftWall x=6,40 normal −X, RightWall x=0 normal +X. (Reglas de trabajo, AX-1)
- TDD con `scripts/test.sh editmode|playmode [filtro]`: RED válido por aserción antes de implementar y suite GREEN después. No declarar éxito con cero tests, XML viejo, errores CS ni builds anteriores. `NotEvaluated`, cancelado o incompleto nunca cuenta como aprobado. Los experimentos van separados de la regresión. (Reglas de trabajo)
- Gate oficial de PlayMode: el Mac del PO. Si en Linux fallan `GesturePreviewTests`, `ReticleVisibilityTests` o `ShotRollbackTests` son fallas de entorno siempre que el Mac dé el recuento completo (registrado como 354/354; ver dudas del informe). (Cierre de PR #39, 10 nov)
- AutoQA (`scripts/autoqa.sh mvp`, PASS/0/nivel 1) es obligatorio si el cambio toca timing, gesto o contrato. (10 nov 13:39)
- En el Mac del PO se pueden ejecutar `scripts/test.sh`, `scripts/check-release.sh` y AutoQA sin pedir permiso por corrida, sin instalar ni tocar el iPhone; copiar `scripts/device.env` sin cambios a un worktree nuevo no exige otro «sí». (10 nov 12:26)
- Un solo Unity a la vez. (B4 encargo de implementación, 13 nov)
- Perfiles: dev con `SQUASH_DEV`, prod sin define, ambos Release no Development; debug Development y Debug; defines con `extraScriptingDefines`, sin mutar los persistentes. (Reglas de trabajo, Perfiles)
- Documentación en español; código, identificadores y commits en inglés. Citar fuentes oficiales para decisiones técnicas externas. Sin frameworks ni abstracciones fuera de la necesidad inmediata. (Reglas de trabajo)
- No versionar `scripts/device.env`, credenciales, identificadores del iPhone, logs, cachés ni builds; no exportar claves privadas. (Reglas de trabajo)
- Visibilidad del remoto: hoy es **público**, confirmado por el PO como intencional («Sí, es intencional; continuar y crear el PR»). No cambiarla por iniciativa propia. Pendiente del PO: el encargo de condensación lo describe como «remoto privado» (ver dudas). (S4, 11 nov)
- P0-calib-1 no se promueve a defaults; tras cada sesión física se restaura P0 en proceso nuevo y se cotejan byte a byte P0-calib-1, Po-1 y last-active con el respaldo. (Etapa 5, 24 oct; sesiones S1–S3)

## 4. Zonas protegidas

| Zona | Dueño | Fuente |
|---|---|---|
| Timing, contrato de contacto y reloj (ClockMap/SimClock) | Codex | Encargo del PO, 15 nov |
| Física P2, solver, M2-4, `DefaultPreset.json`, SimulationKey | Sin dueño asignado; solo con autorización del PO | Encargos del 11–14 nov |

## 5. Hashes (vigentes en `main` desde #56)

- Defaults `06fed4bd4ac9` / 121 claves; P0 efectivo `0a3d3262a79d`; ancla SL5 `8e22575422f2`. Medidos en Unity en #56 y observados en el arranque de la build `0.1.0+43a39a1 dev` del 15 nov. (Potencia según la distancia, 14–15 nov)
- SimulationKey `166a96e319c7`; 41 valores heredados, proyección `0d39e6016bd8`; P0 heredado, proyección `8de3f2ecb217`. (Etapa 5 y S1, 24–25 oct; confirmados el 13 nov)
- Archivo P0-calib-1 SHA-256 `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd`. (Etapa 5, 24 oct; instalación del 15 nov)
- Anteriores a #56, ya no vigentes: defaults `9041c4aafc77`/118, P0 `b6914a349068`, SL5 `e2ffa032a588`.

## 6. Estado actual (15 nov 2026)

- `main` en el merge de esta PR, sobre `70fdd53` (merge de #56). PR #1–#56 fusionadas, salvo #32.
- **#56, potencia según la distancia:** fusionada por autorización del PO el 15 nov, tras el diagnóstico de `Unreachable` sin defecto crítico. `shot.paceGammaMax = 2,0` es **candidato para el playtest, no adoptado**; R3a (Fail 24/36) y S3b (Fail 23/54) quedan como fallos. Los 180 `Unreachable` nuevos de la simulación son golpes medios a menos de 1,5 m del frontis con el rival quieto en z = 5,97 m: a observar en el playtest. ([Diagnóstico](docs/reviews/2026-11-15-first-shot-unreachable-diagnosis.md))
- **Abiertas:** #58 `visual/feel-pass` (Fable): segundo pase visual, sin fusionar; el PO pidió probarlo en el iPhone junto con lo último de `main`. #32 `codex/sl56-return-rule` (Codex): regla experimental de retorno, desactivada, en conflicto, sin autorización de activar.
- **Etapas:** First Ball cerrado con desviación H17 (p95 Fail conservado). First Shot: S1 aceptado (30 oct), S2 aprobado con condiciones (5 nov), S3 aceptado con condiciones (11 nov), S4 4.1–4.4, capa causal, H3b y la curva de potencia fusionados; **First Shot no aceptado**: la aceptación se reserva al playtest integrado. First Rally R1–R6 y pase visual fusionados (#46/#47), sin aceptación física. (12–15 nov)
- **Observaciones abiertas:** R3 Fail 0/42 antes de la curva; con γ 2,0, R3 24/42 y R3a/S3b en Fail (15 nov); posicionamiento del rival ante golpes cortos desde adelante (15 nov); saturación con `pullFullMm` 35, N04/N04b, «aún mecánica», ritmo de rally y preferencia de dirección (S1, 30 oct; S2, 5 nov); condiciones NotEvaluated de S3 (11 nov); juicio H1 y p95 en iPhone de los pases visuales (12 y 15 nov).
- **App instalada:** `0.1.0+43a39a1 dev` (curva de potencia), 15 nov; se reemplaza por la build integrada con #58 que pidió el PO.

## 7. Autorizaciones vigentes

- **En curso:** build DEV integrada (`main` + #58) para que el PO juegue, autorizada el 15 nov («Primero 57, luego el pase visual de 58 pasalo a la build para probar todo lo mas actualizado posible»). La fusión de #58 no está autorizada.
- **No autorizado:** tarea 4.5 de S4 (sesiones físicas); playtest integrado y aceptación de First Shot, First Rally y First Match; First Match más allá del R6 mínimo; activar o validar #32; cambiar `pullFullMm`; promover P0-calib-1; cualquier instalación, build nativa o acceso al iPhone sin pedido expreso; toda fusión sin aprobación explícita del PO. (12 nov, Anexo C y D1–D4; 14 nov)

## 8. Perfil de firma e instalación

- Perfil renovado en la caché de Xcode: vence el **2026-11-18T13:50:05Z (10:50:05 Chile)**. (Cierre de #40, 11 nov; instalación del 12 nov)
- Antes de instalar: comprobar la vigencia del perfil incorporado y que toda la sesión, restauración incluida, termine antes del vencimiento; si no alcanza, avisar al PO. La renovación la hace el PO. No instalar otro Xcode ni cambiar herramientas sin autorización. (Perfil de firma, 10 nov)
- Cada instalación requiere autorización propia, respaldo nuevo de los tres archivos y cotejo antes y después. (Tanda 4, 5 nov; instalación del 12 nov)
