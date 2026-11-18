# Reglas de trabajo — Squash Clash

Condensado el 18 nov 2026. Solo lo vigente. Original byte a byte en [docs/history/AGENTS-archive-2026-11-18.md](docs/history/AGENTS-archive-2026-11-18.md) (anterior: `AGENTS-archive-2026-11-15.md`; SHA-256 en [docs/history/README.md](docs/history/README.md)); correspondencia en [condense-report-2026-11-18.md](docs/history/condense-report-2026-11-18.md).

## 1. Fuentes de verdad

- Spec v0.3, plan First Ball r2 (`docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md`), propuesta/plan First Shot r2 con adendas D1–D6, decisiones D30–D36, spec de cansancio v2.3 (`docs/superpowers/specs/2026-11-16-fatigue-design.md`).
- Decisiones del PO, vigentes desde el 18 nov: [docs/reviews/2026-11-18-po-approvals.md](docs/reviews/2026-11-18-po-approvals.md). Historial hasta el 18 nov: `docs/reviews/2026-10-25-first-shot-po-approvals.md` (grande: buscar por fecha) y `docs/reviews/2026-10-21-first-ball-po-approvals.md`. Si difieren, manda el registro y se avisa al PO.
- Informes por entrega en `docs/reviews/`; evidencia en `docs/validation/`.

## 2. Roles y modo de trabajo

- **PO (Rafa)**: decide alcance, aceptación, fusiones e instalaciones. (MVP, 15 nov)
- **Modo product lead (desde el 18 nov):** el PO delegó en Claude las decisiones técnicas de esta tanda; solo trabajan PO y Claude (Codex y Fable fuera). Claude orquesta subagentes Opus/Sonnet/Haiku según estime. Alcance: G1 A2 (golpes al fondo), cansancio, revés, inicio y estética de golpes, volea, sonido propio y triángulo de impacto del vidrio de fondo. La revisión de Claude **no es independiente** y se declara. Cita literal en el registro del 18 nov. (18 nov)
- Informes de una página; citas literales del PO solo si cambian una regla o autorización; fusionar solo con tests en verde y «sí» del PO. (15 nov)
- Un incremento = una rama/worktree y una PR; commits pequeños. (I2)
- Al fusionar: head local/remoto exacto, árboles limpios, merge commit con segundo padre = head aprobado, `--match-head-commit`; retirar worktrees sin `--force` preservando ignorados. La fusión no acepta la etapa. (#17, #46/#47)
- Ante un defecto o sorpresa: registrar y detenerse, sin corregir ni reintentar para obtener Pass. (12–13 nov)

## 3. Reglas inamovibles

- No modificar spec, escenarios, umbrales ni criterios para hacer pasar resultados.
- Una sola autoridad del estado de la pelota; juego y predicción aislados; configuración efectiva por lanzamiento; `DefaultPreset.json` única fuente de defaults.
- P2-Analytic backend por defecto; P1 respaldo configurable. (23 oct)
- AX-1: coordenadas y cámara conservadas; LeftWall x=6,40 normal −X, RightWall x=0 normal +X.
- TDD con `scripts/test.sh editmode|playmode [filtro]`: RED válido por aserción, luego GREEN. No declarar éxito con cero tests, XML viejo ni errores CS. `NotEvaluated`/cancelado/incompleto nunca es aprobado. Experimentos separados de la regresión.
- Gate oficial de PlayMode: el Mac del PO. En Linux, fallas de `GesturePreviewTests`, `ReticleVisibilityTests` o `ShotRollbackTests` son de entorno si el Mac da el recuento completo. (10 nov)
- AutoQA (`scripts/autoqa.sh mvp`, PASS/0/nivel 1) obligatorio si el cambio toca timing, gesto o contrato. (10 nov)
- En el Mac del PO se pueden correr `scripts/test.sh`, `scripts/check-release.sh` y AutoQA sin pedir permiso, sin instalar ni tocar el iPhone; copiar `scripts/device.env` sin cambios a un worktree nuevo no exige otro «sí». (10 nov)
- Un solo Unity a la vez; en esperas con `pgrep` usar `[U]nity.app/...`.
- Perfiles: dev con `SQUASH_DEV`, prod sin define, ambos Release no Development; defines con `extraScriptingDefines` sin mutar los persistentes.
- Código, identificadores y commits en inglés; documentación en español; fuentes oficiales para decisiones técnicas externas; sin frameworks ni abstracciones fuera de la necesidad inmediata.
- No versionar `scripts/device.env`, credenciales, identificadores del iPhone, logs, cachés ni builds; no exportar claves privadas.
- Remoto **público**, intencional (PO, 11 nov); no cambiarlo.
- P0-calib-1 no se promueve a defaults; tras cada sesión física se restaura P0 en proceso nuevo y se cotejan byte a byte P0-calib-1, Po-1 y last-active con el respaldo.

## 4. Zonas protegidas

| Zona | Dueño |
|---|---|
| Timing, contrato de contacto y reloj (ClockMap/SimClock) | Codex; en esta tanda Claude puede tocarla si el entregable lo exige (§2) |
| Física P2, solver, M2-4, `DefaultPreset.json`, SimulationKey | Sin dueño; en esta tanda decide Claude (§2); todo cambio de hashes se declara en el informe |

## 5. Hashes vigentes en `main` (desde #76/#77)

- Defaults T3 `2ec0c953786a`, 132 claves (con `fatigue.enabled = 0`: `3262f9b89156`).
- P0 efectivo `f92f406f698a`; SL5 T3 `750c407c8a2f`; V `e8df60f37d4b`; SL5 V `95a3f24313fe`.
- SimulationKey `166a96e319c7` (41 valores heredados, proyección `0d39e6016bd8`; P0 heredado, proyección `8de3f2ecb217`).
- RallyParams `927e304bbfec` (desde #74).
- Archivo P0-calib-1 SHA-256 `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd`.
- Proyecciones sobre las 121 claves previas a fatigue.*: T3 `430da2faa818`, P0 `dcc7c8cc0688`, SL5 T3 `e89daab41c3e`, V `06fed4bd4ac9`, SL5 V `8e22575422f2`.

## 6. Estado actual (18 nov 2026)

- `main` en `75b9ce8` (#83). Fusionadas: F1 #76, F2 #77, novedades #78, música #67, CRP-01 #80 (`7d869e8`), herramientas de cansancio #82 (`923daa3`), docs #83. G1 cerrado con A1 #74. Abierta solo #81 (borrador D1 de G1 A2, no se fusiona); #73 cerrada sin fusionar (rama `codex/g1-a2-flat-drive`).
- Puertas en `main` (#82): EditMode 803/803, PlayMode 611/611, AutoQA PASS/0/1, check-release OK.
- Dirección del PO: primera prueba con usuarios reales; T3 pure candidata, no regla. First Shot cerrado (16 nov); First Ball cerrado con desviación H17. Sesión 2 de la tarea 4.5 no se agenda.
- Cansancio en juego real (18 nov, 36 puntos): mediana 18 m/punto; la energía casi nunca baja de 70. Observación, no criterio.
- Observaciones abiertas: G1 A2 (C1 23/24; integración, retícula, C2–C8 y rendimiento pendientes); R3; rival ante golpes cortos; `pullFullMm` 35; NotEvaluated de S3; H1 y p95 en iPhone.
- Entrega en curso (18 nov): G1 A2, limpieza de documentos, estética de inicio y golpes (`/frontend-design:frontend-design`), revisar cansancio, revés, volea, sonido «toc» y triángulo de impacto del vidrio de fondo. El resoplido del rival queda para después. Luego el PO prueba una build.
- App instalada: `0.1.0+923daa3 dev` (18 nov, CFBundleVersion 865).

## 7. Autorizaciones vigentes

- **18 nov (modo product lead):** Claude toma decisiones técnicas de la tanda, incluidas zonas protegidas si el entregable lo requiere, y orquesta subagentes. Se mantienen tests en verde, un solo Unity, no tocar criterios/umbrales para pasar. **Fusión a `main` e instalación requieren «sí» explícito del PO.**
- Cansancio: spec v2.3; F1/F2 fusionadas; el ajuste B ya se agotó en `recoverBetween` 0,35→0,25. Cambios en `fatigue.*` o hashes se declaran. (17 nov)
- G1: A2 diferida; sin corridas ni implementación A2 previas al 18 nov; el encargo del 18 nov lo reabre dentro del modo product lead. Diagnóstico del PO: óptimo local de búsqueda, no el tope lateral. (17–18 nov)
- T3 pure como candidato a pruebas de usuarios; `timing.validLimit` 0 (U1). (16 nov)
- **No autorizado sin pedido expreso:** instalación, build nativa o acceso al iPhone; fusión sin aprobación del PO; sesión 2 de 4.5; activar #32; cambiar `pullFullMm`; promover P0-calib-1; aceptar First Rally/First Match.

## 8. Firma e instalación

- Perfil vigente hasta **2026-11-23T18:03:12Z** (15:03:12 Chile); equipo personal gratuito: vence cada 7 días, no sirve para TestFlight. Renueva el PO.
- Antes de instalar: comprobar vigencia y que toda la sesión, restauración incluida, termine antes del vencimiento. No cambiar Xcode ni herramientas sin autorización.
- Cada instalación requiere autorización propia, respaldo nuevo de presets/métricas y cotejo byte a byte antes y después (`scripts/install-ios.sh dev|prod`).
