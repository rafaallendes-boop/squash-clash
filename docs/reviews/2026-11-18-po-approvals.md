# Registro de aprobaciones del PO — vigente desde el 18 nov 2026

Historial anterior: [2026-10-25-first-shot-po-approvals.md](2026-10-25-first-shot-po-approvals.md) (440 KB; buscar por fecha) y [2026-10-21-first-ball-po-approvals.md](2026-10-21-first-ball-po-approvals.md). Si hay diferencia, manda el registro; se avisa al PO.

## Resumen de autorizaciones vigentes (al 22 nov)

- Autonomía (22 nov): con revisión aprobada y puertas en verde se fusiona a `main` sin «sí» por cada vez (reglas de fusión de AGENTS §2). Instalaciones: SC-PM coordina con Rafael una ventana antes de cada build, Rafael conecta el iPhone y esa ventana es la autorización de esa instalación; no hace falta otro «sí» aparte. Roles: SC-Lead prioriza, SC-PM orquesta en Orca, Claude arquitectura, revisión independiente y front, Codex implementación y revisión cruzada en solo lectura.
- Modelos (22 nov): Claude implementa código con Sonnet; arquitectura y revisión con Opus. Si se agota el límite de créditos de 5 horas de cualquier agente, se detiene todo hasta que se renueve, sin cambiar de modelo ni forzar nada.
- Modo product lead (18 nov): terminado el 22 nov.
- Fusión de PR: con tests en verde y revisión aprobada (22 nov); la fusión no acepta la etapa.
- Corridas de `scripts/test.sh`, `scripts/check-release.sh` y AutoQA en el Mac del PO sin permiso por corrida (10 nov).
- Cada instalación en el iPhone: ventana coordinada por SC-PM con Rafael, que conecta el iPhone (esa ventana es la autorización), más respaldo y cotejo (22 nov; última instalada: `0.1.0+518f9c3 dev`, CFBundleVersion 1078, 22 nov).
- Cansancio v1 (spec v2.3): F1/F2 fusionadas; sin más ajustes B (17 nov).
- U1–U3 ejecutadas: `timing.validLimit` 0, AutoQA con T3 y K independiente (16–17 nov).
- G1: A1 #74 fusionada; A2 diferida el 17 nov, y reabierta dentro del encargo del 18 nov.
- T3 pure es candidata para pruebas de usuarios, no regla adoptada (16 nov).
- First Shot cerrado (16 nov); la sesión 2 de 4.5 no se agenda.
- Exclusivo del PO: aceptar First Rally/First Match, cambiar `pullFullMm`, promover P0-calib-1, activar #32, cambiar umbrales o criterios, pago de Apple y TestFlight, ficha de App Store, y la reserva de AGENTS §4 (física, solver, M2-4, defaults, SimulationKey), salvo la delegación de P1–P6 de #92 a SC-Lead (22 nov).
- Spec del rival ante el golpe corto, #92 (23 nov): Claude decide con SC-Lead, sin esperar al PO. Los umbrales y criterios siguen siendo del PO.
- Remoto público, intencional (11 nov).

## 18 nov 2026 — Modo product lead

Texto literal del PO:

«Perfecto. Acabo de resetear los tokens. Por lo que quiero hacer un gran avance. Necesito que te conviertas en un product lead. Que orquestes distintos agentes (con modelos sonnet - haiku - opus) según tu lo estimes.» … «Entonces esta vez solo trabajaremos los 2. Te doy el consentimiento para que tomes decisiones.» … «Como entregable quiero al menos: G1 A2: Empieza a terminar a de mejorar los golpes al fondo / Limpieza de documentos: para que no pesen tanto las pedidas / Mejorar esteticamente con /frontend-design:frontend-design el inicio y la estetica de golpes. / Revisar lo del cansancio / Volea» … «Podemos ir iterando. Pero tu tomarás las decisiones técnicas. Luego podré probar una build con los ajustes»

Contexto: «los 2» = el sonido propio («toc») del vidrio de fondo y verificar el triángulo de impacto en golpes al vidrio. El resoplido del rival queda para después.

Efecto: Claude actúa como product lead, orquesta subagentes Opus/Sonnet/Haiku y toma las decisiones técnicas de esta tanda (G1 A2, cansancio, revés, inicio, volea, vidrio de fondo), incluidas zonas protegidas (física, M2-4, `DefaultPreset.json`, timing) cuando el entregable lo requiera. La revisión de Claude **no es independiente** y se declara. Se mantienen: tests en verde, un solo Unity a la vez, no modificar criterios ni umbrales para pasar; la fusión a `main` y la instalación siguen requiriendo el «sí» explícito del PO.

## 22 nov 2026 — Autonomía y roles nuevos

Texto literal del PO (transmitido por SC-PM en el encargo del 22 nov):

«Agregar que el PM cuando vaya a orca, hagan los merge y demás. No sea necesario solicitar mi permiso para aprobar en el terminal. Puede ir trabajando con autonomía. Cuando vaya viendo avance podríamos ir haciendo las build mi celular.»

Efecto: con revisión aprobada y puertas en verde (EditMode, PlayMode, AutoQA si aplica y check-release) se fusiona a `main` sin pedir un «sí» cada vez, respetando las reglas de fusión (head exacto, merge commit con segundo padre igual al head aprobado). Roles: SC-Lead (bot de Grok) es el product lead y elige y prioriza; SC-PM (bot de Grok) orquesta en Orca; Claude define la arquitectura, revisa de forma independiente y hace el front con `/frontend-design`; Codex implementa y revisa en solo lectura lo que implementan Claude o sus subagentes. Termina el modo product lead del 18 nov. Sigue siendo exclusivo del PO: aceptar etapas (First Rally y First Match), cambiar `pullFullMm`, promover P0-calib-1, activar #32, cambiar umbrales o criterios, el pago de Apple y TestFlight, y la ficha de App Store.

Condición de las instalaciones, (pedido posterior del PO, 22 nov, vía SC-Lead, transmitido por SC-PM): SC-PM coordina con Rafael una ventana antes de cada build, Rafael conecta el iPhone y esa ventana es la autorización de esa instalación; no hace falta otro «sí» aparte. La cita sola no se lee como autorización general de instalaciones (revisión de Codex en #90).

## 22 nov 2026 — Modelos y límite de créditos

Texto literal del PO (transmitido por SC-PM):

«Otras reglas, cuando Claude le toca implementar código lo hace con modelo sonnet. Por otro lado, si se acaban los créditos de 5 horas. Nos detenemos hasta que vuelvan.»

Efecto: (1) cuando Claude implementa código, usa un modelo Sonnet; la arquitectura y la revisión siguen con Opus (precisión de SC-PM en el encargo). (2) Si se agota el límite de créditos de 5 horas de cualquier agente, se detiene todo hasta que se renueve: no se cambia de modelo ni se fuerza nada, y el trabajo queda registrado tal como esté.

## 22 nov 2026 — Delegación de los presets P1–P6 de #92

Texto literal del PO (22 nov, chat de SC-Lead; SC-PM verificó el texto en esa transcripción):

«Te doy el consentimiento para que puedas tomar la decisión. Iré adelantando lo de testflight quizás. Voy a ir viendo. Entonces vamos bien»

Efecto: las propuestas de preset P1 a P6 de la PR #92 (rival ante el golpe corto: espera, percepción, `ReactionS`, llegada en frenada, globo desde adelante y cobertura) las decide SC-Lead. Es una excepción acotada a la reserva de AGENTS §4. `pullFullMm`, P0-calib-1 y la aceptación de etapas siguen siendo de Rafael.

## 22 nov 2026 — Saque nuevo encendido por defecto (RAL-177)

El PO dijo tras probar los saques en `518f9c3`:

> «Los saques están buenos. Podríamos hacer esa mejora y dejarlo encendido»

Sobre encenderlo con la desviación medida, el PO dijo en el chat de SC-Lead:

> «Está bien la decisión. Igual hay que probar después con los usuarios reales. A si que todo bien.»

Se aprueba encender «Saque nuevo» por defecto sin recalibrar. Se acepta la desviación del Definidor de −5,1 pp en Medio y −5,5 pp en Difícil, y la caída de P+V del novato al devolver el saque del rival, de 97,5 % a 89,7 %. **Observación abierta:** revisar ambas cifras y la devolución del saque con los primeros usuarios reales de TestFlight. [Medición del 20 nov](2026-11-20-serve.md).

## 22 nov 2026 — Precisión del borde en ShotSolver (RAL-177)

El PO autorizó el cambio el 22 de noviembre de 2026 a las 23:03 (Chile, UTC−03), en el chat de SC-PM.

> «Sí, autorizo corregir la precisión del borde en ShotSolver para el saque nuevo»

La autorización de AGENTS §4 cubre igualar la precisión de los límites de pared de `ShotSolver` a la que usa P2. [PR #100](https://github.com/rafaallendes-boop/squash-clash/pull/100).

## 23 nov 2026 — Spec del rival ante el golpe corto (#92)

El PO autorizó el 23 de noviembre de 2026 a las 08:43 (Chile, UTC−03), en el chat de SC-PM. Lo transmitió SC-PM en el encargo a Claude.

> «para la spec del rival. Toma la mejor decisión con el project lead y no se traben por mi, así pueden seguir»

Efecto:

- Las decisiones de la spec de la PR #92 las toma Claude con SC-Lead, sin esperar al PO. Esto incluye incorporar el criterio de producto de SC-Lead de RAL-174 (camino B) y atender la revisión de Codex.
- La fusión sigue la autonomía del 22 nov (AGENTS §2/§7), sin «sí» por cada vez.
- No cambia lo exclusivo del PO: si M3 no vuelve con I2–I3, la decisión de umbrales es de Rafael. No se cambian umbrales, oráculos ni tolerancias existentes. `pullFullMm`, P0-calib-1 y la aceptación de etapas siguen siendo suyos.

## 23 nov 2026 — Margen temprano acotado en la recepción del saque (#106)

Texto literal del PO (Rafael, 16:33, chat de SC-PM), transmitido en `/Users/rafaallendes/sc-specs/impl-recepcion-b-acotada.txt`:

> «Sí, confirmo hasta 50 ms solo en mi recepción del saque y los anillos con «Espera el bote»».

Se autoriza medir 25 y 50 ms frente a la base, solo para el golpe temprano del humano al recibir el saque. La regla de SC-Lead (16:32), incorporada al encargo, elige el menor valor que mejore claramente la devolución del novato que toca sin romper N2; si ninguno pasa dentro de 50 ms, se detiene el trabajo. Se mantienen los umbrales, criterios y oráculos vigentes, y se informan M1 oficial, la señal secundaria, puntos, partidos y volea por nivel con n y fuente.

La continuación explícita de SC-PM en `/Users/rafaallendes/sc-specs/continua-106.txt` autoriza corregir el defecto del prototipo sin cambiar `ServeEarlyAllowance_LatencyCompensationIsAppliedOnce` ni su aserción; fija el borde inclusivo, con compensación de latencia una sola vez. Confirma que el margen incluye el golpe temprano de la recepción y conserva la volea opcional del saque en Fácil, Medio y Difícil. Los anillos y «Espera el bote» van en otra PR con Claude. La PR #106 lleva la medición, el margen elegido y las puertas completas; **no se fusiona** en este encargo. [Informe](2026-11-23-recepcion-b-acotada.md).

## 24 nov 2026 — Margen de 25 ms en la recepción del saque (#106)

Textos literales del PO (Rafael, chat de SC-PM), transmitidos por SC-PM:

> 23 nov 22:14, sobre corregir solo la jugada del fixture inválido sin tocar la aserción: «Si, pero has todo con Claude.»
>
> 24 nov 07:20: «Sí, acepto que M1 quede fuera de banda solo en la build de prueba con 50 ms.»
>
> 24 nov 09:38: «Vamos con 25 ms como me propones».
>
> 24 nov 12:07, directa en el chat de SC-PM, en respuesta a «¿Aceptas que M1 quede fuera de banda (93,80 %) solo en la build de prueba con 25 ms, igual que lo aceptaste con 50 ms?»: «Acepto la prueba que me mandó el lead»

La propuesta del 09:38 consistía en aplicar el protocolo fijado antes de medir (`docs/validation/2026-11-23-recepcion-b-protocolo.md`, `b4cccbd`), cuya regla de elección da 25 ms: N2 frente a la base actual medida, con un máximo de 1,40 pp, y una mejora clara del novato que toca. El valor por defecto pasa a 25 ms con la medición existente, sin volver a medir y sin cambiar umbrales ni criterios. La aceptación de M1 de las 07:20 se refería a 50 ms. **Con la respuesta de las 12:07, M1 con 25 ms en Medio, 93,80 % (3615/3854), fuera de 80–90, queda aceptado SOLO para la build de prueba.** El criterio no cambia y no se acepta ninguna etapa. AGENTS §7 no se edita: la decisión para builds que no sean de prueba (TestFlight o pública) sigue pendiente del PO. [Informe](2026-11-23-recepcion-b-acotada.md).
