# Pase visual — autorización y plan

Rama `visual/art-pass`, worktree propio, base `firstrally/gameplay` en `93777e3` (PR #46, borrador). Implementa Claude (Fable). PR en borrador, sin fusionar. Sin instalación ni acceso al iPhone.

**Nota sobre la base.** El encargo pide partir de «firstrally/gameplay con la corrección de la regla». Al crear la rama, `firstrally/gameplay` local y remota estaban en `93777e3` y no contenían un commit posterior con esa corrección. Se parte de `93777e3`. Este pase no toca Core ni reglas, así que la corrección se puede integrar después con un merge o rebase sin conflicto esperado en los archivos de vista.

## Autorización literal del PO (12 de noviembre de 2026)

> AUTORIZACIÓN: autorizo un pase visual, saliendo del alcance «dirección de arte = FUTURO» solo para esta tarea. Cítala en tu informe. Mi uso de Fable es limitado (cuota de 5 horas): trabaja por bloques en este orden de prioridad, haz un commit por bloque que deje la app funcionando, y si te quedas sin tiempo lo ya entregado debe servir.

Antecedente en `AGENTS.md` (11 de noviembre): «dejo registrada mi autorización literal para First Rally y la excepción «dirección de arte» solo para el pase visual. Spec, escenarios, umbrales y criterios no se modifican».

## Zonas protegidas (no se tocan)

- Core completo (incluido `Core/Rally`), física, reglas, spec, escenarios, umbrales y criterios. Lógica de `RallyDirector`, `MatchScore`, `FondoAi`, `RallySim`.
- SimulationKey `166a96e319c7`, los 41 valores heredados y `DefaultPreset.json`. Si hace falta una clave, se pide en el informe.
- Timing (Codex): `TimingRingView`, `ImpactCue`, claves `timing.*`, contrato de contacto, reloj, compensación de latencia y el shader `Legibility`. Los shaders y materiales del pase son nuevos.
- `AGENTS.md` y el documento de aprobaciones.
- `device.env`, visibilidad del remoto, logs, builds e identificadores del iPhone.

## Plan por bloques

Un commit por bloque, con la app funcionando en cada uno. Si se acaba el tiempo, lo entregado sirve.

0. **Captura.** Fixture de PlayMode con prefijo `Z` que renderiza a PNG (práctica, rally en Demo y partido) a 1206×2622. Capturas «antes» con el aspecto actual.
1. **Personajes.** Componente nuevo que lee `PlayerBody` (posición, velocidad, fase) y los eventos de swing: figura estilizada de piezas simples, camiseta de dos colores, animación procedural de espera, carrera, golpe por el lado correcto y recuperación. `PlaceholderPlayers` conserva `Place`, `Human`, `Rival` y los nombres de GameObject.
2. **Cancha.** Materiales y texturas procedurales nuevos, líneas de alto contraste, una sola luz con una sola sombra en tiempo real como máximo, fondo sobrio.
3. **Pelota.** Contraste sobre piso y pared, estela y sombra ajustadas. Radio físico de 20 mm intacto; solo cambia lo visual.
4. **Impactos.** Destello, partículas discretas en rebotes, temblor de cámara sutil solo en golpes fuertes.
5. **HUD.** Tipografía y paleta coherentes; pantalla de resultado del partido. Se conservan los nombres `rally-hud`, `rally-status`, `rally-toggle`, `rally-pause`, `rally-rematch`, `rally-demo` y `boot-header`.

## Límites y verificación

- Rendimiento: sin postprocesado pesado, sin varias sombras en tiempo real, sin asignaciones por frame. Se mide en el editor lo que se pueda; el p95 en iPhone lo mide el PO.
- No empeorar la lectura de altura y profundidad aprobada en H1.
- Prod sin panel de desarrollador; el HUD del rally sí va en prod.
- Al cerrar cada bloque: suites completas y `scripts/autoqa.sh mvp` en PASS, salida 0, nivel 1. Nunca éxito con cero tests, XML viejo o errores CS.
- `ShotRollbackTests.ApplyShot_AfterFlightStopped_ResimulatesToRealPresentViaClockMap` es intermitente y de la zona de Codex: si falla, se repite una vez, se anota y no se toca.
- Cierre: informe en `docs/reviews/` con capturas antes/después, mediciones, decisiones con su razón y checklist de playtest. Build dev lista con `scripts/build-ios.sh dev`, sin instalar.
