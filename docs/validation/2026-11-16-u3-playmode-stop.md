# U3 — parada en test existente de retículo

16 nov 2026 · código `5d90951` sobre main `a1fa915`. Corrida única PlayMode completa de U3: **550/551**, 0 skipped, XML fresco y sin errores CS, **2026-11-16 23:10:01Z–23:12:25Z** (20:10:01–20:12:25 Chile).

Falla `SquashClash.Runtime.Tests.ReticleVisibilityTests.LocalCancelLimitAndState_AreVisibleAndNeverCoverFrontis`, `ReticleVisibilityTests.cs:186`: después de seleccionar `gesture.control=1`, `launcher.repeatSeconds=0` y lanzar, `preview.ArmSlingBlock("B0")` devuelve False donde se exige True. El fallo ocurre antes de las comprobaciones visuales posteriores. No se conoce la razón del rechazo; no se inventa a partir de la aserción.

El test conserva bytes de `a1fa915`. No es el método de CRP-01, y no se extiende a este caso la clasificación del PO. Causa no demostrada; no se atribuye al entorno o intermitencia. Detención conforme a AGENTS §2 y a la instrucción del PO de detenerse si reaparece CRP-01 u otra falla. Sin corrección, diagnóstico invasivo ni reintento.

Las tres pruebas nuevas ZUserMetricsFlowTests pasan: Interruption_RecordsOnePauseAndDeduplicatesAppCallbacks; MenuTutorialMatchEnd_RecordsTheRealFlowAndOneMatchResult; TutorialSkipAndMenuExit_RecordSkippedStepsAndAbandonment. EditMode completo **747/747**. RED previos válidos 0/10 EditMode y 0/3 integración PlayMode. Todos con XML fresco; evidencia curada en `docs/validation/evidence/2026-11-16-u3-summary.json`.

Evidencia local ignorada: `Logs/evidence/u3/playmode-first-playmode-results.xml`, SHA-256 `852c6826a547c7bc426789ebd6b0ba6468e28ca6de797eb7804ba82f5c706183`; log `playmode-first-playmode.log`, SHA-256 `666877f51fa4094721482f4c428760d0f43eeaa7310140683155a0f428b61e60`.

ProjectSettings restaurado con `git checkout -- ProjectSettings` después de la terminación completa del exec; ningún editor ni auxiliar Unity activo. Defaults, Core/Shot, Core/Simulation, Runtime/Shot y RallyDirector permanecen sin cambios respecto de main. No se ejecuta AutoQA porque el incremento solo registra eventos sin cambiar gesto/timing/contrato. Check-release pendiente por la parada. No hubo build nativa, acceso al iPhone, instalación o fusión. U2 sin iniciar. U1 #65 conserva por separado sus compuertas completas en verde. Revisión independiente pendiente, coordinada con el PO.

## Autorización posterior del PO

El PO autoriza **una sola PlayMode completa nueva**, manteniendo el test y el producto de #66 sin corrección. Si pasa completa, clasificar LocalCancelLimitAndState como intermitente sin causa demostrada, **no CRP-01**, y seguir check-release. Si falla otra vez, detenerse y reportar. El resultado previo 550/551 no se cambia; la clasificación solo se confirmará al cumplirse la condición. Literal en el registro de aprobaciones.
