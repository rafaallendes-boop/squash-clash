# Reglas de trabajo — Squash Clash

Versión condensada del 14–16 nov 2026. Solo contiene lo vigente. El original completo, byte a byte, está en [docs/history/AGENTS-archive-2026-11-15.md](docs/history/AGENTS-archive-2026-11-15.md) (SHA-256 en [docs/history/README.md](docs/history/README.md)); la correspondencia regla por regla, en [docs/history/condense-report.md](docs/history/condense-report.md). Entre paréntesis, la entrada del archivo de la que viene cada regla.

## 1. Fuentes de verdad

- Spec v0.3, plan First Ball r2 (`docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md`), propuesta y plan First Shot r2 con sus adendas D1–D6, decisiones D30–D36 de la spec. (Reglas de trabajo, inicio; D36 del 13 nov)
- Decisiones del PO: `docs/reviews/2026-10-25-first-shot-po-approvals.md` (First Shot en adelante) y `docs/reviews/2026-10-21-first-ball-po-approvals.md` (First Ball). Si este archivo y el registro de aprobaciones difieren, manda el registro y se avisa al PO. (Reglas de trabajo, inicio)
- Informes de cada entrega en `docs/reviews/`, evidencia en `docs/validation/`.

## 2. Roles y forma de trabajo (modo MVP)

- **PO (Rafa)** decide alcance, aceptación y fusiones. **Claude**: revisión independiente, operación física cuando el PO lo designa y dirección de subagentes Sonnet cuando el PO lo pide; si Claude dirige el trabajo, su revisión no es independiente y se declara. **Codex**: implementación y zonas protegidas (§4). **Fable**: First Rally y pase visual, por autorización propia; **terminó el 16 nov** (sin créditos), su autorización ya no está vigente. Desde el 16 nov se vuelve al modo normal: Codex implementa (código y zonas protegidas), Claude revisa de forma independiente (si dirige un subagente, se declara), subagentes Sonnet para documentos y protocolos y Haiku para el operador de AutoQA, solo a pedido del PO; el PO aprueba cada fusión y los hitos se cierran paso a paso. (S3 «Claude, ahora», 9 nov; autorización de Fable, 11 nov; B4 con subagente Sonnet, 13 nov; encargo del PO, 16 nov)
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

## 5. Hashes (U1 en esta rama; pendiente de fusión)

- Defaults T3 `430da2faa818` / 121 claves; P0 efectivo `dcc7c8cc0688`; ancla SL5 T3 `e89daab41c3e`. Medidos en Unity en U1 y confirmados con los defaults reales; #65: EditMode 738/738, PlayMode 558/558, AutoQA V/T3 PASS/0/1, check-release dev/prod OK. Pendientes de revisión independiente y fusión. El archivo P0-calib-1 no se modifica.
- Anteriores a U1, ya no vigentes en esta rama: defaults `06fed4bd4ac9`/121, P0 `0a3d3262a79d`, SL5 `8e22575422f2`. Siguen vigentes en `main` hasta fusionar U1 y como anclas de los escenarios V explícitos.
- SimulationKey `166a96e319c7`; 41 valores heredados, proyección `0d39e6016bd8`; P0 heredado, proyección `8de3f2ecb217`. (Etapa 5 y S1, 24–25 oct; confirmados el 13 nov)
- Archivo P0-calib-1 SHA-256 `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd`. (Etapa 5, 24 oct; instalación del 15 nov)
- Anteriores a #56, ya no vigentes: defaults `9041c4aafc77`/118, P0 `b6914a349068`, SL5 `e2ffa032a588`.

## 6. Estado actual (16 nov 2026)

- `main` en `10bcaec` (merge de #64, 16 nov); último cambio de producto `a1fa915` (#63). PR #1–#64 fusionadas, salvo #32, que se cerró sin fusionar. U1 #65 integra ese main y sigue pendiente de aprobación de fusión.
- **#58, #59 y #60 (Fable), fusionadas el 16 nov por autorización del PO** (§7), con `--match-head-commit` y en orden de pila; en cada una, segundo padre = head aprobado y árbol del merge = árbol del head. #58 `visual/feel-pass`: merge `8f7ebc7`, head `a401e2a`. #59 `visual/first-run`: merge `27e5360`, head `98befb1` (código `0d58aeb`): menú de inicio, tutorial de 3 pasos, pausa y fin de partido, arte de release (sin conectar a ProjectSettings), AudioScape ([informe](docs/reviews/2026-11-15-first-run.md)). #60 `visual/device-robustness`: merge `22bb2d5`, head `f98c955` (código `6c47efc`): áreas seguras, interrupciones que abren la pausa, música propia con interruptor, opción zurda cosmética ([informe](docs/reviews/2026-11-16-device-robustness.md)). Verificación apoyada en las puertas de Fable sobre los heads de código (#59: EditMode 735/735, PlayMode 502/502, AutoQA PASS/0/1; #60: EditMode 737/737, PlayMode 544/544, AutoQA PASS/0/1 `20261116T100627Z-6c47efc-26272`, check-release OK); Claude no repitió las suites. Los commits posteriores a los heads de código son solo documentos y capturas. Revisión de Claude: sin cambios en Core, física, `DefaultPreset.json`, timing, contrato de contacto, reloj, GesturePreview, ProjectSettings ni Packages; la opción zurda es cosmética (solo la lee CharacterView). Fusionar no acepta ninguna etapa. (16 nov)
- **#32 cerrada sin fusionar** (16 nov). La reemplazan #56 y la capa causal; la rama se conserva en `10f162f`. Activar la regla sigue sin autorización. (16 nov)
- **#63 (C1 de Codex) fusionada en `a1fa915`:** oculta en prod la línea técnica «M2-4 · Tensando · P …» (queda en DEV), recuerda la dificultad y hace que «Mano: diestro» deje la raqueta en la mano derecha. Se mantiene: una interrupción en partida abre la pausa; «Sonido» y «Música» son interruptores independientes. (16 nov)
- **Etapas:** First Ball cerrado con desviación H17 (p95 Fail conservado). First Shot: S1 aceptado (30 oct), S2 aprobado con condiciones (5 nov), S3 aceptado con condiciones (11 nov), S4 4.1–4.4, capa causal, H3b y la curva de potencia fusionados; **First Shot cerrado por decisión del PO el 16 nov**, sin modificar sus criterios; lo pendiente queda como observación abierta ([registro](docs/reviews/2026-10-25-first-shot-po-approvals.md)). First Rally R1–R6 y los pases visuales (#46/#47, #58–#60) fusionados, sin aceptación física. Tarea 4.5: protocolo aprobado (#62); la sesión 1 se hizo el 16 nov y se cerró anticipadamente por decisión del PO, después de B1 (V) y B2 (T3 pure) ([hoja](docs/validation/2026-11-16-first-shot-4-5-session-sheet.md)). La sesión 2 no se agenda. (12–16 nov)
- **Dirección del PO desde el 16 nov:** priorizar una primera prueba con usuarios reales. T3 pure es la configuración candidata para las pruebas externas, no una regla adoptada. Las métricas se recogen en segundo plano y no se abren líneas de desarrollo ajenas a ese objetivo. Propuesta: [camino a usuarios](docs/reviews/2026-11-16-camino-a-usuarios.md). (16 nov)
- **Observaciones abiertas:** R3 Fail 0/42 antes de la curva; con γ 2,0, R3 24/42 y R3a/S3b en Fail (15 nov); posicionamiento del rival ante golpes cortos desde adelante (15 nov); saturación con `pullFullMm` 35, N04/N04b, «aún mecánica», ritmo de rally y preferencia de dirección (S1, 30 oct; S2, 5 nov); condiciones NotEvaluated de S3 (11 nov); juicio H1 y p95 en iPhone de los pases visuales (12 y 15 nov). Juego libre del PO tras instalar, literal: «Ahora estuve jugando y se ve bastante mejor.» Es una observación, no una aceptación. (16 nov)
- **App instalada:** `0.1.0+a1fa915 dev` (`main` con #63), 16 nov 18:04Z. El perfil se renovó por autorización del PO. Respaldo antes de instalar y presets idénticos byte a byte después. Tras la sesión 1 de 4.5, P0 quedó restaurado (P0-calib-1, `0a3d3262a79d`, 0 pendientes) y los tres archivos, idénticos al respaldo. La evidencia está en `Logs/evidence/` (ignorada). (16 nov)

## 7. Autorizaciones vigentes

- **U1–U3, 16 nov:** el PO autoriza `timing.validLimit` de 1 a 0 y hashes, ampliar AutoQA con K independiente y escenarios T3 predeclarados, conservando V explícito y sus expectativas/anclas. SL1 +170 ms Failed/Late, SL5 +170 ms Valid y SL1 −200 ms Valid. Una repetición del RED de defaults autorizada y ejecutada con fallo de aserción válido; ningún reintento adicional. Se trabaja en #65; U3 y U2 después, sin fusionar ni instalar. Detalle en el registro de aprobaciones y el informe U1.

- **Concedidas el 16 nov, ya ejecutadas:** fusión de #58–#60 e instalación de la build DEV, con la autorización literal del PO: «Listo, revisa @[Robustez de dispositivos, música y opción zurdo] y @[Primera experiencia Squash Clash]  ya  está ok. Con esto si está todo correcto. Has la fusión y luego la instalación en el celular. y iré a realizar una prueba completa de todo.»
- **Vigentes desde el 16 nov (decisiones del PO):** Codex implementa los ajustes de producto de §6 como entrega propia; Codex actualiza #32 contra `main`, desactivada; un subagente Sonnet redacta el protocolo y la plantilla de la tarea 4.5. Cambio de modo, literal: «Fable ya se acabaron los créditos. Ahora volvemos al modo normal. Códex también vuelve. A si que también podría tener alguna tarea para desarrollo» y «Si, lanza todo. […] Pero ya que adelantamos con fable, ahora toca volver al modo normal y vamos operando cerrando los hitos paso a paso.» Detalle en el [registro de aprobaciones](docs/reviews/2026-10-25-first-shot-po-approvals.md).
- **Concedidas y ejecutadas el 16 nov:** fusión de #63; renovación del perfil, instalación de `a1fa915` dev y sesión 1 de 4.5 («Si, utiliza mi PC y renueva el vencimiento del 18 de nov. Autorizo las sesiones y quiero hacer las pruebas. […]»); cierre anticipado tras B2 («Vamos con solo B2 y así comparas. Pero ahí cerramos. […]»).
- **Decididas el 16 nov:** T3 pure como candidato para pruebas con usuarios; la sesión 2 de 4.5 no se agenda; Claude propone el camino mínimo a usuarios. Literal en el [registro](docs/reviews/2026-10-25-first-shot-po-approvals.md).
- **Vigente desde el 16 nov:** encargo U1–U3 a Codex («Deja el encargo a codex. […] Haría U1, U2 y U3»), que incluye cambiar `timing.validLimit` de 1 a 0 en `DefaultPreset.json` (U1) y los hashes. First Shot cerrado por decisión del PO.
- **Vigente desde el 16 nov:** cansancio v1 según la spec `docs/superpowers/specs/2026-11-16-fatigue-design.md` (v2.1): diseño funcional y cambios protegidos acotados. Son las claves `fatigue.*` en `DefaultPreset.json`, el movimiento del cuerpo y su planificación, y el vector causal de cansancio, sin alterar los contratos existentes. Va en dos PRs, F1 y F2, en la cola después de U1–U3 y G1. G1 (golpes al fondo y SL5/SL6): diagnóstico primero, y física, solver y M2-4 solo después de la elección del PO. AutoQA se amplía a T3 pure, con los escenarios V fijados de forma explícita.
- **No autorizado:** sesión 2 de 4.5; ítems del backlog del MVP sin encargo propio (SL5/SL6 toca física y M2-4 y requiere autorización explícita); aceptación de First Rally y First Match; First Match más allá del R6 mínimo; activar o validar #32; cambiar `pullFullMm`; promover P0-calib-1; cualquier instalación, build nativa o acceso al iPhone sin pedido expreso; toda fusión sin aprobación explícita del PO. (12 nov, Anexo C y D1–D4; 14 y 16 nov)

## 8. Perfil de firma e instalación

- Perfil renovado en la caché de Xcode: vence el **2026-11-23T18:03:12Z (15:03:12 Chile)**. Es de un **equipo personal gratuito**: vence cada 7 días y no sirve para TestFlight, que requiere el Apple Developer Program. (Renovación del 16 nov)
- Antes de instalar: comprobar la vigencia del perfil incorporado y que toda la sesión, restauración incluida, termine antes del vencimiento; si no alcanza, avisar al PO. La renovación la hace el PO. No instalar otro Xcode ni cambiar herramientas sin autorización. (Perfil de firma, 10 nov)
- Cada instalación requiere autorización propia, respaldo nuevo de los tres archivos y cotejo antes y después. (Tanda 4, 5 nov; instalación del 12 nov)
