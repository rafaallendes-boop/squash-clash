# I2 — cancha y cámara fija

Fecha: 2026-10-21. Rama: `firstball/i2-court`, base `6ad2961`. Alcance autorizado: T2.1–T2.3; T3.1 y posteriores excluidas. **En implementación.** La revisión de Claude y la validación del PO en el iPhone serán el punto de control antes de fusionar.

## Geometría — T2.1

Se aplica la convención de coordenadas y las 12 franjas del plan r2, la enmienda E.0 de la spec y D-6 del PO: 50 mm perpendiculares para la diagonal, conservando su borde inferior. La normal se calcula normalizada a partir de los extremos, evitando el redondeo ilustrativo de la tabla. Tin WSF 0,48 m y variante PSA 0,43 m. El clasificador calcula distancia al rectángulo finito de cada franja, incluido el extremo; distingue el disco de una simple expansión rectangular y filtra por superficie. No implementa consecuencias de saque o rally.

TDD: con contratos mínimos sin comportamiento, geometría RED 11/11 fallos; GREEN 11/11. Clasificador RED 35/35 fallos; suite EditMode completa GREEN **49/49**. Las pruebas incluyen las 15 filas del plan, la otra lateral, pared trasera, líneas de suelo, esquinas de franja y reutilización de la lista de solapamientos. Logs/XML locales: `Logs/i2-geometry-{red,green}-summary.log`, `Logs/i2-classifier-red-summary.log`, `Logs/i2-t21-green-summary.log` y XML asociados. Baseline del worktree: EditMode 3/3 y PlayMode 2/2.

## Resoluciones de implementación

- Worktree separado `.worktrees/i2-court`, ignorado, conservando `main` y su proyecto Unity; configuración local del dispositivo copiada ignorada con permisos 0600. Coste: importación y caché Unity separadas.
- El helper de tareas de `executing-plans` busca encabezados `Task`; se lee cada sección `Tarea` directamente y se mantiene un ledger local sin modificar el plan. Coste: seguimiento manual respaldado por commits y evidencia.
- Los contratos mínimos que lanzan `NotImplementedException` permiten ejecutar primero los tests tipados. Solo se implementa comportamiento después del RED; no se conservan stubs al completar cada tarea.
- Los miembros de `EffectiveConfig` indicados para T3.1/T3.3 se incorporarán entonces. En I2 solo se construye la configuración que la cancha y la cámara consumen; no se crean tipos vacíos de pelota o simulación.

## Configuración — T2.2

Diez parámetros de I2, sin defaults en el catálogo. `DefaultPreset.json` contiene todos los valores; `ParamSet` conserva una copia inmutable ordenada y hashes SHA-256 de 12 hex independientes del orden y la cultura. `EffectiveConfig` deriva la cancha y separa el hash de simulación del de cámara. Una copia mutable de `CourtDimensions` no puede alterar la instantánea.

`JsonUtility` serializa/deserializa el DTO. Se añade una comprobación interna limitada a la forma del esquema v1 para rechazar campos requeridos omitidos, tipos incorrectos, JSON truncado, basura al final y campos JSON duplicados; así no se convierten silenciosamente en valores cero. Las claves repetidas de `values` se comprueban después de deserializar. No se añade una librería JSON ni un framework de serialización. Coste: mantener este lector pequeño junto al esquema v1; no hay migraciones.

TDD: **30/30 fallos RED** por comportamiento pendiente; suite EditMode completa **79/79 GREEN**. Se cubren schema 0/2, JSON corrupto, claves duplicadas/desconocidas/ausentes, tipos numéricos incorrectos, booleanos fraccionarios, no finitos en overlays, recorte con avisos, elecciones inválidas, inmutabilidad y hash independiente de cultura/orden. Un error de nombre en el helper de tests impidió la primera compilación; el runner rechazó correctamente el XML anterior y se corrigió antes del RED válido. Logs/XML: `Logs/i2-config-red-*`, `Logs/i2-t22-green-*`.
