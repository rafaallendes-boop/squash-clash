# T4.5 / I4d — comprobación previa nativa P2

Build **0.1.0+557d3ff dev instalada**, sin cambios pendientes al compilar. Export limpio Unity 6000.3.27f1, Player no Development, SQUASH_DEV por extraScriptingDefines, IL2CPP Release, Xcode 27.1 (27A9275) Release y ejecutable/UnityFramework **ARM64**. Firma estricta y metadata exacta embebida en Data/resources.assets verificadas. [Evidencia saneada](evidence/2026-10-23-i4d-native-build.json).

**ProjectPhysics embebido:** EnhancedDeterminism=True; **fastMotionThreshold=0.01**; bounceThreshold=0.1; Unity=6000.3.27f1. Capturado por BuildScript desde la configuración del export e impreso por SpikeSection; fastMotionThreshold relevante solo para P1, sin afirmar getter en runtime. P2 recibe bounceThreshold efectivo de UnityEngine.Physics. Backend P2-Analytic, variante analytic/r2-20261023 y texto de guardado de muestras verificados también en metadata IL2CPP. El informe completo guarda las **200 muestras C17** en orden de adquisición, con precisión round-trip, además de agregados.

Prod nuevo del mismo commit contiene solo Core/Runtime, sin DevPanel ni ThermalState.mm. No se instaló prod. El host normal conserva P1; solo el botón **Prueba física P2** ejecuta la campaña P2. Física y mediciones idénticas a las dos campañas c8e5923 del editor; cambio posterior solo de prefijo de archivo guardado en DevPanel (P2-).

Perfil embebido vigente hasta **2026-10-28T14:59:31Z**, coincide con dispositivo/bundle y comprobado inmediatamente antes de instalar, sin publicar identificadores personales. Fecha de instalación 2026-10-23: no aplica renovación obligatoria desde el 27; si hubiera otra instalación el 27 o después, renovar antes. Herramientas sin cambios.

Instalación confirmada. iOS rechazó el intento de lanzamiento por herramientas por **teléfono bloqueado**; no se reescribe como exitoso. El PO abrió manualmente Squash Clash y ejecutó la única campaña P2 autorizada. El informe completo de build **0.1.0+557d3ff dev** verifica el arranque correcto. Sin herramientas del agente durante ejecución; recuperación de Documents/spike posterior al aviso de término. Dos intentos de conexión fallidos antes de recuperar el archivo, sin repetir la campaña ni reinstalar.

## Resultado y auditoría posteriores

El PO confirmó «Listo. Prueba completada» y luego «SI, al 80% y todo según lo solicitado»: **batería inicial 80 %**, teléfono físico, No molestar, bajo consumo desactivado, sin funda y sin capturas/duplicación/devicectl/otras herramientas. Informe **P2-20261023-204715-409**, configuración **c86243b15a97**, variante **analytic/r2-20261023**, **18 criterios únicos y cobertura cerrada**; térmico nominal inicial/benchmark/final, cero interrupciones de foco.

**C17 Pass, p95 0,011400 ms ≤1 ms**; 20 calentamientos y 200 muestras individuales persistidas, finitas, percentiles/mediana recalculados coincidentes con agregados. Los 11 criterios de gate pasan; siete informativos. Encabezado efectivo incluye **fastMotionThreshold=0.01 (solo P1)** y bounceThreshold efectivo P2=0.1. SHA-256 de informe `4a350b411f348c00d43aaf4f08704e78b0536c5269f6b11e3eb65c1b56472b99`, 16.649 bytes.

[Auditoría y condiciones](evidence/2026-10-23-i4d-p2-native-run.json), [informe íntegro](evidence/physics-spike-P2/iphone-557d3ff-P2-20261023-204715-409.md), [comparación P1/P2 completa](../reviews/2026-10-23-first-ball-physics-spike-P2.md). Se detiene para revisión de Claude/decisión PO; **I4d abierto, PR #6 borrador sin fusionar**, backend final pendiente e I5 no autorizado. No se repite I4b ni se pide otra campaña.
