# Registro de aprobaciones del PO — vigente desde el 18 nov 2026

Historial anterior: [2026-10-25-first-shot-po-approvals.md](2026-10-25-first-shot-po-approvals.md) (440 KB; buscar por fecha) y [2026-10-21-first-ball-po-approvals.md](2026-10-21-first-ball-po-approvals.md). Si hay diferencia, manda el registro; se avisa al PO.

## Resumen de autorizaciones vigentes (al 22 nov)

- Autonomía (22 nov): con revisión aprobada y puertas en verde se fusiona a `main` y se instala en el iPhone sin «sí» por cada vez (reglas de fusión y §8 de AGENTS). Roles: SC-Lead prioriza, SC-PM orquesta en Orca, Claude arquitectura, revisión independiente y front, Codex implementación y revisión cruzada en solo lectura.
- Modelos (22 nov): Claude implementa código con Sonnet; arquitectura y revisión con Opus. Si se agota el límite de créditos de 5 horas de cualquier agente, se detiene todo hasta que se renueve, sin cambiar de modelo ni forzar nada.
- Modo product lead (18 nov): terminado el 22 nov.
- Fusión de PR: con tests en verde y revisión aprobada (22 nov); la fusión no acepta la etapa.
- Corridas de `scripts/test.sh`, `scripts/check-release.sh` y AutoQA en el Mac del PO sin permiso por corrida (10 nov).
- Cada instalación en el iPhone: respaldo y cotejo; desde el 22 nov sin autorización propia si viene de `main` con las puertas en verde (última instalada: `0.1.0+518f9c3 dev`, CFBundleVersion 1078, 22 nov).
- Cansancio v1 (spec v2.3): F1/F2 fusionadas; sin más ajustes B (17 nov).
- U1–U3 ejecutadas: `timing.validLimit` 0, AutoQA con T3 y K independiente (16–17 nov).
- G1: A1 #74 fusionada; A2 diferida el 17 nov, y reabierta dentro del encargo del 18 nov.
- T3 pure es candidata para pruebas de usuarios, no regla adoptada (16 nov).
- First Shot cerrado (16 nov); la sesión 2 de 4.5 no se agenda.
- Exclusivo del PO: aceptar First Rally/First Match, cambiar `pullFullMm`, promover P0-calib-1, activar #32, cambiar umbrales o criterios, pago de Apple y TestFlight, ficha de App Store.
- Remoto público, intencional (11 nov).

## 18 nov 2026 — Modo product lead

Texto literal del PO:

«Perfecto. Acabo de resetear los tokens. Por lo que quiero hacer un gran avance. Necesito que te conviertas en un product lead. Que orquestes distintos agentes (con modelos sonnet - haiku - opus) según tu lo estimes.» … «Entonces esta vez solo trabajaremos los 2. Te doy el consentimiento para que tomes decisiones.» … «Como entregable quiero al menos: G1 A2: Empieza a terminar a de mejorar los golpes al fondo / Limpieza de documentos: para que no pesen tanto las pedidas / Mejorar esteticamente con /frontend-design:frontend-design el inicio y la estetica de golpes. / Revisar lo del cansancio / Volea» … «Podemos ir iterando. Pero tu tomarás las decisiones técnicas. Luego podré probar una build con los ajustes»

Contexto: «los 2» = el sonido propio («toc») del vidrio de fondo y verificar el triángulo de impacto en golpes al vidrio. El resoplido del rival queda para después.

Efecto: Claude actúa como product lead, orquesta subagentes Opus/Sonnet/Haiku y toma las decisiones técnicas de esta tanda (G1 A2, cansancio, revés, inicio, volea, vidrio de fondo), incluidas zonas protegidas (física, M2-4, `DefaultPreset.json`, timing) cuando el entregable lo requiera. La revisión de Claude **no es independiente** y se declara. Se mantienen: tests en verde, un solo Unity a la vez, no modificar criterios ni umbrales para pasar; la fusión a `main` y la instalación siguen requiriendo el «sí» explícito del PO.

## 22 nov 2026 — Autonomía y roles nuevos

Texto literal del PO (transmitido por SC-PM en el encargo del 22 nov):

«cuando vaya a orca, hagan los merge y demás. No sea necesario solicitar mi permiso para aprobar en el terminal. Puede ir trabajando con autonomía» y «cuando vaya viendo avance podríamos ir haciendo las build mi celular»

Efecto: con revisión aprobada y puertas en verde (EditMode, PlayMode, AutoQA si aplica y check-release) se fusiona a `main` y se instala en el iPhone sin pedir un «sí» cada vez, respetando las reglas de fusión (head exacto, merge commit con segundo padre igual al head aprobado) y el procedimiento de instalación de AGENTS §8. Roles: SC-Lead (bot de Grok) es el product lead y elige y prioriza; SC-PM (bot de Grok) orquesta en Orca; Claude define la arquitectura, revisa de forma independiente y hace el front con `/frontend-design`; Codex implementa y revisa en solo lectura lo que implementan Claude o sus subagentes. Termina el modo product lead del 18 nov. Sigue siendo exclusivo del PO: aceptar etapas (First Rally y First Match), cambiar `pullFullMm`, promover P0-calib-1, activar #32, cambiar umbrales o criterios, el pago de Apple y TestFlight, y la ficha de App Store.

## 22 nov 2026 — Modelos y límite de créditos

Texto literal del PO (transmitido por SC-PM):

«Otras reglas, cuando Claude le toca implementar código lo hace con modelo sonnet. Por otro lado, si se acaban los créditos de 5 horas. Nos detenemos hasta que vuelvan.»

Efecto: (1) cuando Claude implementa código, usa un modelo Sonnet; la arquitectura y la revisión siguen con Opus (precisión de SC-PM en el encargo). (2) Si se agota el límite de créditos de 5 horas de cualquier agente, se detiene todo hasta que se renueve: no se cambia de modelo ni se fuerza nada, y el trabajo queda registrado tal como esté.
