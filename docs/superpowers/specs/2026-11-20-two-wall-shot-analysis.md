# Análisis: golpe de «dos paredes» (boast) — 20 nov 2026

Pedido del PO (20 nov): «También analizar el agregar dos paredes como golpe.» Documento redactado por un subagente Sonnet dirigido por Claude; **la revisión de Claude no es independiente**. Solo análisis: sin Unity, sin cambios de código. Rama `claude/two-wall-analysis`, base `f0fe172`. Rutas relativas a `Assets/SquashClash/`.

## 1. Qué es el golpe

En squash, «dos paredes» es el *boast*: la pelota golpea primero una pared lateral y después la pared frontal (variantes: de tres paredes, que añade la pared del fondo, el *reverse boast*, que cruza hacia el lado contrario, y el drive que roza la lateral tras la frontal). Es legal mientras llegue a la pared frontal sobre la tin y bajo la línea de fuera, sin tocar el suelo antes; las laterales y la del fondo valen como paredes de juego bajo sus líneas de fuera. Se usa desde los rincones traseros, a menudo bajo presión, para mover al rival hacia delante y al costado, o como golpe de sorpresa. Fuente: WSF, *Rules of Singles Squash* (worldsquash.com, sección de reglas; la apertura del jugador al golpear es la cobertura de la Regla 6.2.4 que ya cita `PointRules.cs:146`). No se citan frases textuales.

## 2. Qué hace el código hoy

- **Física P2: ya modela paredes laterales.** `Core/Simulation/AnalyticBallSimulation.cs:74-75` considera `RightWall`/`LeftWall` como candidatos de contacto, con material propio `SideWalls` (`:136`, `PhysicsParams.cs:8`). No hace falta física nueva.
- **Reglas: la secuencia raqueta → lateral → frontal es legal en juego abierto.** `PointRules.OnBallContact` (`Core/Rally/PointRules.cs:143-165`): con cero botes, una pared lateral solo se juzga (`Judge`, `:65-69`, Out si pasa la línea) y no cierra el punto; `FrontWallReached` solo se activa con la pared frontal (`:160`). Un bote antes de la frontal es `NotUp` (`:148`). **En el saque** cualquier otra pared antes que la frontal es falta (`:122-125`), correcto en squash real.
- **Solver: solo apunta a la frontal.** `ShotSolver.Solve` (`Core/Shot/ShotSolver.cs:26-62`) recibe `AimOnFrontWall` y resuelve una parábola recta; `Verify` (`:64-82`) rechaza cualquier primer contacto que no sea `FrontWall`. `ShotIntentSolver` tampoco: prefiere `FirstFront` en el orden de candidatos (`Core/Shot/ShotIntentSolver.cs:246`, calculado en `:210`) y sus ampliaciones laterales de x están acotadas a ±`alpha·Width/20` (`:95-96`). Un boast **no se puede producir** hoy: habría que apuntar a la lateral y verificar el primer contacto lateral.
- **M2-4: mapa táctil → punto de la frontal.** `Core/Shot/M24Model.cs:42` llama `FrontisProjection.TryMap` (`Core/Shot/FrontisProjection.cs:20-44`): rayo de cámara intersectado con el plano z=0, rechazo si x∉[0,Width] o y∉[0,WallHeight] (`:41`). El `AimModel` (`AimModel.cs:61`) también limita x a [0,Width]. La vía del rival (`ShotOptions.cs:21-48`, `RallyContact.SolveIntention` `:54`) construye líneas Straight/Cross/Drop, todas con `Aim` en la frontal. El rival usa el mismo solver compartido.

## 3. Opciones

Todas requieren un **modo de solver nuevo** (apuntar a un punto de pared lateral, verificar lateral→frontal, resolver el tramo con rebote): toca **solver (zona protegida §4, sin dueño)**. La física P2 y `DefaultPreset.json` no deberían cambiar, pero `PointRules` merece tests nuevos.

| Opción | Qué toca | Zonas protegidas | Riesgo | Esfuerzo |
|---|---|---|---|---|
| **(a) Gesto: apuntar a zona lateral de la pantalla** | `FrontisProjection` (aceptar rayos que cortan x=0/Width), `AimModel`/`M24Model`, reticle, solver | M2-4, solver; contrato de contacto/timing intactos si el aim sigue siendo un punto | Alto: el aim actual es un punto de la frontal en pantalla; ampliar a laterales confunde con el apuntado normal y cambia la admisión del gesto | 2–3 semanas |
| **(b) Automático por ángulo extremo desde un rincón** | `ShotIntentSolver`/`ShotSolver` (alternativa cuando el aim exige ángulo extremo), vista previa | Solver; M2-4 sin cambios | Medio: sorprende al jugador («yo apunté a la frontal»); contradice «una autoridad y la retícula muestra TargetUsed» | 1–2 semanas |
| **(c) Modificador: gesto separado (p. ej. segundo dedo o botón)** | Entrada nueva, `GesturePreview.*`, `ShotController`, solver | M2-4/contrato si cambia la admisión; solver | Medio-alto: carga de UX, y toca el pipeline del gesto aprobado | 2–3 semanas |
| **IA (`RivalAi`)** | `ShotOptions` agrega `ShotLine.Boast`; el Definidor lo pondera con `StyleShooter*` (`RivalAi.cs:32`) | Solver (el mismo modo); `RallyParams` → hash propio | Medio: cambia hashes de RallyParams/estilos; puntuar bien un golpe de riesgo | 1 semana tras el solver |

**Efectos transversales.** *Vista previa/reticle:* hoy la retícula marca un punto de la frontal y se fija tras soltar (`AimReticleView.cs`, `ShotIntentSolver.cs:20`); un boast necesita marcar dos puntos (lateral y frontal) y la previa de trayectoria deja de ser una recta. *AutoQA:* los escenarios V/T3 fijan hashes y el oráculo con la primera pared frontal; un modo nuevo exige escenarios nuevos sin tocar los vigentes (el scheme de `ShotRecord` es cerrado, ver parada F1-AUTOQA-01), y si el boast es automático cambian las trayectorias de V/T3 existentes. *Cansancio y volea:* se acoplan igual que un drive, sin cambios propios.

## 4. Recomendación y decisiones del PO

Recomendado: **(c) con alcance mínimo, tras una fase de solver aislado**, y la IA al final. Razón: (a) y (b) mezclan el golpe con el apuntado normal o con la autoridad del solver y rompen el contrato de «lo que apuntas es lo que ves»; (c) deja intacto el gesto actual.

Fases:
1. **Solver boast aislado** (sin UI ni IA): `ShotSolver.SolveBoast` con verificación lateral→frontal, tests EditMode con RED por aserción, sin tocar el camino actual; `DefaultPreset` sin cambios y hashes intactos.
2. **Entrada humana** (c) con vista previa de dos puntos; AutoQA con escenarios nuevos añadidos.
3. **IA:** `ShotLine.Boast` para el Definidor, con medición de hashes de RallyParams.
4. **Prueba física** en el iPhone, sin aceptar etapa.

**Decide el PO:** (1) si hace el golpe o lo difiere tras la prueba con usuarios (el camino a usuarios no lo exige); (2) la opción a/b/c; (3) autorizar tocar el solver (§4, sin dueño) y quién lo implementa (Codex por defecto); (4) si cambian los hashes de RallyParams para la IA; (5) si el boast queda como golpe solo del Definidor o también humano desde el inicio.

## Decisión del PO (20 nov)

Literal: «Arreglemos primero lo otro. El gesto es apuntar a la pared lateral. Solo 1 Gesto. autorizo calcular el golpe. Lo usen todos los jugadores.»

- **Gesto:** opción (a), apuntar a la pared lateral. Es un solo gesto, no uno aparte.
- **Autorización:** el PO autoriza tocar el cálculo del golpe (solver).
- **Quién lo usa:** el jugador y el rival.
- **Cuándo:** después de corregir lo que ya existe (SL5/SL6, el cansancio y la volea).
