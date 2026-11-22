# Reglas de trabajo — Squash Clash

Condensado el 18 nov 2026. Solo lo vigente. Original byte a byte en [docs/history/AGENTS-archive-2026-11-18.md](docs/history/AGENTS-archive-2026-11-18.md) (anterior: `AGENTS-archive-2026-11-15.md`; SHA-256 en [docs/history/README.md](docs/history/README.md)); correspondencia en [condense-report-2026-11-18.md](docs/history/condense-report-2026-11-18.md).

## 1. Fuentes de verdad

- Spec v0.3, plan First Ball r2 (`docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md`), propuesta/plan First Shot r2 con adendas D1–D6, decisiones D30–D36, spec de cansancio v2.3 (`docs/superpowers/specs/2026-11-16-fatigue-design.md`).
- Decisiones del PO, vigentes desde el 18 nov: [docs/reviews/2026-11-18-po-approvals.md](docs/reviews/2026-11-18-po-approvals.md). Historial hasta el 18 nov: `docs/reviews/2026-10-25-first-shot-po-approvals.md` (grande: buscar por fecha) y `docs/reviews/2026-10-21-first-ball-po-approvals.md`. Si difieren, manda el registro y se avisa al PO.
- Informes por entrega en `docs/reviews/`; evidencia en `docs/validation/`.

## 2. Roles y modo de trabajo

- **PO (Rafa)**: dueño del producto; lo que sigue siendo exclusivo suyo está en §7. (15 y 22 nov)
- **SC-Lead** (bot de Grok): product lead; elige y prioriza el trabajo. **SC-PM** (bot de Grok): orquesta los encargos en Orca. (22 nov)
- **Claude**: define la arquitectura y revisa de forma independiente; el front lo hace Claude con `/frontend-design`. Cuando Claude o sus subagentes implementan, la revisión de Claude no es independiente y se declara. (22 nov)
- **Codex**: implementa, y revisa en solo lectura lo que implementan Claude o sus subagentes (revisión cruzada). (22 nov)
- El modo «Claude como product lead» (18–21 nov) terminó el 22 nov. Fable sin créditos desde el 16 nov.
- **Autonomía (22 nov).** Texto literal del PO: «cuando vaya a orca, hagan los merge y demás. No sea necesario solicitar mi permiso para aprobar en el terminal. Puede ir trabajando con autonomía» y «cuando vaya viendo avance podríamos ir haciendo las build mi celular». Efecto: con revisión aprobada y puertas en verde (EditMode, PlayMode, AutoQA si aplica según §3 y check-release) se fusiona a `main` y se instala en el iPhone sin pedir un «sí» cada vez, respetando las reglas de fusión de abajo y el procedimiento de §8.
- Informes de una página; citas literales del PO solo si cambian una regla o autorización. (15 nov)
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
- En el Mac del PO se pueden correr `scripts/test.sh`, `scripts/check-release.sh` y AutoQA sin pedir permiso; copiar `scripts/device.env` sin cambios a un worktree nuevo no exige otro «sí». Las builds nativas y las instalaciones siguen §2 (autonomía) y §8. (10 y 22 nov)
- Un solo Unity a la vez; en esperas con `pgrep` usar `[U]nity.app/...`.
- Perfiles: dev con `SQUASH_DEV`, prod sin define, ambos Release no Development; defines con `extraScriptingDefines` sin mutar los persistentes.
- Código, identificadores y commits en inglés; documentación en español; fuentes oficiales para decisiones técnicas externas; sin frameworks ni abstracciones fuera de la necesidad inmediata.
- No versionar `scripts/device.env`, credenciales, identificadores del iPhone, logs, cachés ni builds; no exportar claves privadas.
- Remoto **público**, intencional (PO, 11 nov); no cambiarlo.
- P0-calib-1 no se promueve a defaults; tras cada sesión física se restaura P0 en proceso nuevo y se cotejan byte a byte P0-calib-1, Po-1 y last-active con el respaldo.

## 4. Zonas protegidas

| Zona | Dueño |
|---|---|
| Timing, contrato de contacto y reloj (ClockMap/SimClock) | Codex (15 nov; la excepción del modo product lead terminó el 22 nov) |
| Física P2, solver, M2-4, `DefaultPreset.json`, SimulationKey | Sin dueño; solo con autorización del PO (11–14 nov; la excepción del modo product lead terminó el 22 nov); todo cambio de hashes se declara en el informe |

## 5. Hashes vigentes en `main` (desde #84)

- Defaults T3 `207da8f2c636`, 132 claves; P0 efectivo `7a5972dacf1e`; SL5 T3 `d09119d6321b`; V `28e63d17dfbf`; SL5 V `8ec4deffc6c1`; ancla VL1 `24531a50618a`.
- SimulationKey `166a96e319c7` (41 valores heredados, proyección `0d39e6016bd8`; P0 heredado, proyección `8de3f2ecb217`).
- RallyParams `a82c34b661bf` con «Dos paredes» encendido (`bd05b443b4f9` apagado; proyección sin `Boast*` `992d0570c9de`).
- Archivo P0-calib-1 SHA-256 `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd`.
- Anteriores a #84, ya no vigentes: defaults `2ec0c953786a`, P0 `f92f406f698a`, SL5 T3 `750c407c8a2f`, V `e8df60f37d4b`, SL5 V `95a3f24313fe`, RallyParams `927e304bbfec`.

## 6. Estado actual (22 nov 2026)

- `main` en `518f9c3` (merge de #89, 21 nov, segundo padre `a031921`, árbol idéntico al head): solo AGENTS, registra la fusión de #88 y la instalación de `a7b3311`. Código y hashes iguales a `a7b3311`.
- Antes: `a7b3311` (merge de #88, 21 nov, segundo padre `92cc1fa`, árbol idéntico al head), con aprobación del PO («Fusiona e instala»): el aviso del golpe («¡Al aire!», «Perfecto», …) va centrado en la franja vacía bajo la cancha, calculada desde la cámara; una barra DEV oculta ya no lo empuja. Sin cambios de cámara, preset ni juego. Subagente Sonnet dirigido por Claude: revisión no independiente. Sobre `77c1864`: EditMode 961/961, PlayMode 746/746, AutoQA V/T3/volea PASS/0/1, check-release OK. [Informe](docs/reviews/2026-11-21-shot-note-band.md). Antes: #87 (`939d419`, solo AGENTS).
- Antes: `fceb3de` (merge de #86, 21 nov, segundo padre `226c703`, árbol idéntico al head), con aprobación del PO («Fusiona»): líneas técnicas DEV ocultas al jugar, paso «Dos paredes» en el tutorial y correo de la ficha. Antes: `5a1d98d` (merge de #85, segundo padre `cc55d1f`), aprobada con «Tienes permiso para fusionar main»: pasada de seguridad y preparación para App Store.
- Antes: `7fe6a42` (merge de #84, 20 nov, segundo padre `47ed1e5`, árbol idéntico al head), con aprobación del PO («Dale si»). Trae toda la tanda MVP 18–20 nov (siete vueltas, detalle abajo). Antes: #83 (`75b9ce8`); F1 #76, F2 #77, novedades #78, música #67, CRP-01 #80, herramientas de cansancio #82. G1 cerrado con A1 #74. Abierta solo #81 (borrador D1 de G1 A2, no se fusiona); #73 cerrada sin fusionar.
- Puertas del código de #84 (sobre `bf64f46`, árbol idéntico): EditMode 956/956, PlayMode 740/740, AutoQA V/T3/volea PASS/0/1, check-release OK (build 1051).
- Dirección del PO: primera prueba con usuarios reales; T3 pure candidata, no regla. First Shot cerrado (16 nov); First Ball cerrado con desviación H17. Sesión 2 de la tarea 4.5 no se agenda.
- Cansancio en juego real (18 nov, 36 puntos): mediana 18 m/punto; la energía casi nunca baja de 70. Observación, no criterio.
- Observaciones abiertas: G1 A2 C3 43/48 y p95 de cuadro en iPhone (integración hecha en la tanda); R3; rival ante golpes cortos; `pullFullMm` 35; NotEvaluated de S3; H1 y p95 en iPhone.
- **Tanda MVP 18–20 nov, fusionada en #84:** rama `claude/mvp-1118-integration` (docs, reajuste del cansancio, vidrio de fondo, inicio, revés, G1 A2, volea v1; segunda vuelta del 19 nov: tarjeta de resultado, golpe Perfecto propio, congelado de fin de punto, volea en el tutorial, estallido en vez del texto «Volea ·» y costo de energía de la volea). Sobre `8cceeb9`: EditMode 870/870, PlayMode 665/665, AutoQA V/T3/volea PASS/0/1, check-release OK (build 946). RallyParams pasa a `5cdbb01e4621` (proyección sin `VolleyEnergyCost` `6a7b329a7b85`). Al fusionar cambian los hashes de §5: defaults `207da8f2c636`, P0 `7a5972dacf1e`, SL5 T3 `d09119d6321b`, V `28e63d17dfbf`, SL5 V `8ec4deffc6c1`, RallyParams `6a7b329a7b85`, ancla VL1 `24531a50618a`. Tercera vuelta del 19 nov: estilos de rival Fondista/Definidor/Completo con volea y cobertura del rival y calibración por estilo y nivel (Definidor en Difícil 37,8 % frente a 20 %, desviación), menú rediseñado con créditos y resoplido del rival. Sobre `ecff541`: EditMode 883/883, PlayMode 689/689, AutoQA V/T3/volea PASS/0/1, check-release OK (build 975). RallyParams `0f58580f44ea` (proyección sin `Style*` `5cdbb01e4621`); defaults y SimulationKey sin cambio. Cuarta vuelta del 20 nov: volea v2.2, en la que el jugador elige la volea con un toque durante el anillo ámbar, con sprint y volea del saque, solo para el humano. Desviaciones: 28 % de voleas frente a ≥40 % y una diferencia de aciertos de 12,4 frente a ≤12 pp. Sobre `4593d2b`: EditMode 897/897, PlayMode 695/695, AutoQA V/T3/volea PASS/0/1, check-release OK. RallyParams `0fe523164363` (proyección `0f58580f44ea`). Quinta vuelta del 20 nov: protección del golpe al fondo desde adelante (SL5/SL6, solo humano), sprint de la volea según la energía, «Cansancio fuerte» como opción DEV, pose de volea del rival y récords personales. Sobre `12cc652`: EditMode 912/912, PlayMode 731/731, AutoQA V/T3/volea PASS/0/1, check-release OK (build 1029). RallyParams `13f88c8d1db2`. Golpe a dos paredes decidido por el PO para después (apuntar a la lateral, un solo gesto, solver autorizado, todos los jugadores). Sexta vuelta del 20 nov: saque alto y rápido tras el interruptor DEV «Saque nuevo», apagado por defecto (con el saque nuevo encendido, el Definidor se mueve −5,1/−5,5 pp). Sobre `434da69`: EditMode 924/924, PlayMode 735/735, AutoQA V/T3/volea PASS/0/1, check-release OK. RallyParams `992d0570c9de`. Séptima vuelta del 20 nov: golpe de dos paredes (apuntar a la lateral, verificado con P2, drive de respaldo sin falta) para el jugador y el rival, con el interruptor DEV «Dos paredes» encendido por M4 (−0,2/+1,8/−0,0 pp); M1b 39,2 % en desviación. Sobre `bf64f46`: EditMode 956/956, PlayMode 740/740, AutoQA V/T3/volea PASS/0/1, check-release OK (build 1051). RallyParams `a82c34b661bf`. Observaciones: B4 del cansancio 6 %, C3 de A2 43/48. [Informe](docs/reviews/2026-11-19-mvp-batch-integration.md).
- **Preparación para App Store, fusionada en #85 (21 nov):** escaneo de secretos del historial público sin secretos reales (hallazgo bajo: el ID del equipo personal está en ProjectSettings y en docs desde el primer commit); manifiesto de privacidad de la app (UserDefaults CA92.1, sin seguimiento ni datos; Unity fusiona el del motor) verificado en el export; `submitAnalytics` 0; barra DEV oculta mientras se juega y botón DEV chico; borrador de la ficha en `docs/release/2026-11-21-app-store-ficha.md`. Subagente Sonnet dirigido por Claude: revisión no independiente. Sobre `7e4e87d`: EditMode 961/961, PlayMode 743/743, AutoQA V/T3/volea PASS/0/1, check-release OK (build 1061). [Informe](docs/reviews/2026-11-21-store-prep.md). Decisiones del PO pendientes en el informe (compartir Documents en la build pública, textos de la ficha, alojamiento de la política, probadores, módulos sin uso, ID de equipo).
- **Segunda parte, fusionada en #86 (21 nov):** a pedido del PO (captura del panel), las líneas técnicas «P0-calib-1 · hash · …» y «M2-4 · …» se ocultan mientras se juega con el panel DEV cerrado (tres aserciones existentes pasan de visible a oculto por ese pedido); quinto paso del tutorial «Dos paredes»; correo de contacto y feedback de la ficha: el del PO. Subagente Sonnet dirigido por Claude: revisión no independiente. Sobre `77d3521`: EditMode 961/961, PlayMode 744/744, AutoQA V/T3/volea PASS/0/1, check-release OK. [Informe](docs/reviews/2026-11-21-hud-onboarding.md). El dibujo del paso nuevo no se vio en el iPhone.
- App instalada: `0.1.0+a7b3311 dev` (21 nov 15:35Z, CFBundleVersion 1076), `main` con #84–#88. La instaló Claude con autorización del PO («Fusiona e instala»); lanzamiento automático OK. Anterior: `0.1.0+fceb3de dev` (21 nov 10:39Z, CFBundleVersion 1071).
  - **Instalación de `518f9c3` dev (22 nov), detenida antes de compilar:** respaldo nuevo hecho (6 presets idénticos al cotejo del 21 nov, P0-calib-1 con el SHA de §5, last-active P0-calib-1; métricas 31/32 idénticas, la de `20261121T1535…` creció porque era la sesión abierta en el cotejo anterior). `build-ios.sh dev` falló en la exportación de Unity (13:43Z): licencia Personal sin validez offline (`LicenseGroupOfflineValidityPeriodIsExpired`) y Hub sin sesión. No se llegó a Xcode, el perfil no se renovó y el teléfono no se tocó. La licencia la renueva el PO en Unity Hub. Evidencia en `Logs/evidence/install-518f9c3-20261122/` (ignorada; worktree `sc-install-518f9c3`).
  - Presets (6): idénticos byte a byte; P0-calib-1 con el SHA de §5 y last-active P0-calib-1.
  - Métricas: 31 de 32 idénticas. Al abrirse la app creó una nueva y la rotación de 32 borró en el teléfono la más antigua (`metrics-20261116T143801…`), que queda en el respaldo.
  - Evidencia en `Logs/evidence/install-a7b3311-20261121/` (ignorada; worktree SC-store).
  - Anterior: `0.1.0+d11f3d9 dev` (19 nov 23:24Z, CFBundleVersion 976). Es la rama de integración con las tres vueltas del 19 nov, sin fusionar en `main`. La instaló Claude con autorización del PO («Sí, instala»); lanzamiento automático OK.
  - Presets (6): idénticos byte a byte al respaldo previo; P0-calib-1 con el SHA de §5 y last-active P0-calib-1.
  - Métricas: 31 de 32 idénticas. La rotación de 32 borró en el teléfono la más antigua (`metrics-20261113T153853…`), que queda en el respaldo.
  - Evidencia en `Logs/evidence/install-d11f3d9-20261119/` (ignorada).
  - Anterior: `0.1.0+923daa3 dev` (18 nov, CFBundleVersion 865).

## 7. Autorizaciones vigentes

- **22 nov (autonomía, §2):** fusión a `main` e instalación en el iPhone sin «sí» por cada vez, con revisión aprobada, puertas en verde, reglas de fusión de §2 y procedimiento de §8. Se mantienen un solo Unity y no tocar criterios ni umbrales para pasar. El modo product lead del 18 nov terminó.
- Cansancio: spec v2.3; F1/F2 fusionadas; el ajuste B ya se agotó en `recoverBetween` 0,35→0,25. Cambios en `fatigue.*` o hashes se declaran. (17 nov)
- G1: A2 diferida; sin corridas ni implementación A2 previas al 18 nov; el encargo del 18 nov lo reabrió dentro del modo product lead (terminado el 22 nov). Diagnóstico del PO: óptimo local de búsqueda, no el tope lateral. (17–18 nov)
- T3 pure como candidato a pruebas de usuarios; `timing.validLimit` 0 (U1). (16 nov)
- **Exclusivo del PO (no se hace sin su pedido expreso):** aceptar etapas (First Rally y First Match); cambiar `pullFullMm`; promover P0-calib-1; activar #32; cambiar umbrales o criterios; el pago de Apple y TestFlight; la ficha de App Store; la sesión 2 de 4.5. Fusiones, builds e instalaciones ya no están en esta lista: siguen §2 y §8. (22 nov)

## 8. Firma e instalación

- Perfil en la caché de Xcode vigente hasta **2026-11-23T18:03:12Z** (15:03:12 Chile; creado el 16 nov, comprobado el 22 nov); equipo personal gratuito: vence cada 7 días, no sirve para TestFlight. La app instalada (1076) deja de abrirse a esa hora si no se reinstala con un perfil nuevo.
- Renovación: se intenta con la firma automática de `scripts/build-ios.sh` (`-allowProvisioningUpdates`). Si pide que el PO inicie sesión con su Apple ID, acepte algo en Xcode o confíe en el dispositivo, se detiene y se le pide ese paso. La licencia de Unity del Hub también la renueva el PO. (22 nov)
- Antes de instalar: comprobar vigencia del perfil incorporado y que toda la sesión, restauración incluida, termine antes del vencimiento. No cambiar Xcode ni herramientas sin autorización.
- Cada instalación: respaldo nuevo de presets y métricas, cotejo byte a byte antes y después (`scripts/install-ios.sh dev|prod`), lanzamiento y evidencia en `Logs/evidence/install-<sha>-<fecha>/`. Desde el 22 nov no requiere un «sí» propio si viene de `main` con las puertas de §2 en verde.
