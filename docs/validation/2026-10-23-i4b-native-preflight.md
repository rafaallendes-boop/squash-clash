# I4b — comprobación previa a la campaña nativa

2026-10-23. Build instalada **0.1.0+2fdede5 dev**, metadata sin cambios pendientes; campaña todavía sin iniciar. [Evidencia saneada](evidence/2026-10-23-i4b-canonical-native.json).

**ProjectPhysics: EnhancedDeterminism=true; fastMotionThreshold=0.01 m; bounceThreshold=0.1.** Comprobado en `ProjectSettings/DynamicsManager.asset`, idéntico byte a byte al código versionado de la build. Procedencia: configuración fuente del export limpio; no se presenta como una lectura de getter en runtime. El encabezado actual de la campaña nativa no imprime fastMotionThreshold; esta comprobación separada cubre el informe corto pedido por el PO.

Export Unity limpio, Unity 6000.3.27f1, SQUASH_DEV por extraScriptingDefines, Player no Development, IL2CPP Release y Xcode 27.1 (27A9275) Release, ejecutable y UnityFramework ARM64. Firma validada con codesign --verify --deep --strict. Metadata exacta encontrada en Data/resources.assets del .app firmado; hash y perfil dev verificados. Export prod nuevo del mismo commit: solo Core y Runtime, sin DevPanel, sin ThermalState.mm. No se instaló prod.

Perfil embebido comprobado inmediatamente antes de instalar: vigente hasta **2026-10-28T14:59:31Z**, incluye el teléfono y coincide con team/bundle, sin publicar identificadores personales. Instalación confirmada por devicectl; el lanzamiento fue rechazado porque el iPhone está bloqueado, pendiente de desbloqueo del PO. Si hubiera otra instalación el 27 de octubre o después, renovar antes aunque aún figure vigente. Herramientas sin cambios.

Campaña manual pendiente: teléfono físico, No molestar, batería ≥80 %, bajo consumo desactivado, sin funda, térmico nominal al inicio. Sin capturas, devicectl ni herramientas durante la ejecución. El PO debe avisar al finalizar antes de recuperar Documents/spike. Auditar 18 criterios, hash 2fdede5, configuración c86243b15a97, variante baseline/ccdDyn-co1mm-it6-1, 20 calentamientos y 200 mediciones C17, térmico al inicio del benchmark e interrupciones de foco. **C17 p95 ≤1 ms; si incumple, Fail**. No interpretar tiempos de editor ni compararlos con nativo.

Regresión previa conservada: EditMode 188/188, PlayMode 58/58; no se repiten suites por documentación y comentario de test. No hay cambio de comportamiento, escenarios, umbrales, variantes, preset, Reset o selección de backend. PR #4 borrador sin fusionar, I4b abierto. Informe final pendiente de campaña; detenerse después para Claude y PO. I4c, P2/T4.5 e I5+ sin autorizar.
