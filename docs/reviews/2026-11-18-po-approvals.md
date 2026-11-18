# Registro de aprobaciones del PO — vigente desde el 18 nov 2026

Historial anterior: [2026-10-25-first-shot-po-approvals.md](2026-10-25-first-shot-po-approvals.md) (440 KB; buscar por fecha) y [2026-10-21-first-ball-po-approvals.md](2026-10-21-first-ball-po-approvals.md). Si hay diferencia, manda el registro; se avisa al PO.

## Resumen de autorizaciones vigentes (al 18 nov)

- Modo product lead (18 nov): Claude toma las decisiones técnicas de la tanda y orquesta subagentes; fusión e instalación requieren «sí» explícito del PO.
- Fusión de PR: solo con tests en verde y aprobación explícita del PO; la fusión no acepta la etapa.
- Corridas de `scripts/test.sh`, `scripts/check-release.sh` y AutoQA en el Mac del PO sin permiso por corrida; sin instalar ni tocar el iPhone (10 nov).
- Cada instalación en el iPhone requiere autorización propia, respaldo y cotejo (última: `0.1.0+923daa3 dev`, 18 nov).
- Cansancio v1 (spec v2.3): F1/F2 fusionadas; sin más ajustes B (17 nov).
- U1–U3 ejecutadas: `timing.validLimit` 0, AutoQA con T3 y K independiente (16–17 nov).
- G1: A1 #74 fusionada; A2 diferida el 17 nov, y reabierta dentro del encargo del 18 nov.
- T3 pure es candidata para pruebas de usuarios, no regla adoptada (16 nov).
- First Shot cerrado (16 nov); la sesión 2 de 4.5 no se agenda.
- No autorizado sin pedido expreso: activar #32, cambiar `pullFullMm`, promover P0-calib-1, aceptar First Rally/First Match.
- Remoto público, intencional (11 nov).

## 18 nov 2026 — Modo product lead

Texto literal del PO:

«Perfecto. Acabo de resetear los tokens. Por lo que quiero hacer un gran avance. Necesito que te conviertas en un product lead. Que orquestes distintos agentes (con modelos sonnet - haiku - opus) según tu lo estimes.» … «Entonces esta vez solo trabajaremos los 2. Te doy el consentimiento para que tomes decisiones.» … «Como entregable quiero al menos: G1 A2: Empieza a terminar a de mejorar los golpes al fondo / Limpieza de documentos: para que no pesen tanto las pedidas / Mejorar esteticamente con /frontend-design:frontend-design el inicio y la estetica de golpes. / Revisar lo del cansancio / Volea» … «Podemos ir iterando. Pero tu tomarás las decisiones técnicas. Luego podré probar una build con los ajustes»

Contexto: «los 2» = el sonido propio («toc») del vidrio de fondo y verificar el triángulo de impacto en golpes al vidrio. El resoplido del rival queda para después.

Efecto: Claude actúa como product lead, orquesta subagentes Opus/Sonnet/Haiku y toma las decisiones técnicas de esta tanda (G1 A2, cansancio, revés, inicio, volea, vidrio de fondo), incluidas zonas protegidas (física, M2-4, `DefaultPreset.json`, timing) cuando el entregable lo requiera. La revisión de Claude **no es independiente** y se declara. Se mantienen: tests en verde, un solo Unity a la vez, no modificar criterios ni umbrales para pasar; la fusión a `main` y la instalación siguen requiriendo el «sí» explícito del PO.
