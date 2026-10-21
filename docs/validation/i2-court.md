# I2 — cancha y cámara fija

Fecha: 2026-10-21. Rama: `firstball/i2-court`, base `6ad2961`. Alcance autorizado: T2.1–T2.3; T3.1 y posteriores excluidas. **Implementación y comprobación técnica completadas: dev compilada, firmada, instalada y ejecutada en el iPhone, con cancha visible, versión y 60 FPS.** [PR #1](https://github.com/rafaallendes-boop/squash-clash/pull/1) hacia main. El PO comunicó revisión de Claude sin bugs de implementación y aceptó visualmente la cancha; solicitó corregir la convención izquierda/derecha antes de fusionar. La aclaración AX-1 y su verificación se registran al final. La PR continúa abierta, sin fusión automática.

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

## Vista y cámara — T2.3

La escena referencia CourtView, CourtCameraRig y PlaceholderPlayers; el arranque carga el preset estricto, crea la instantánea y aplica los mismos datos a los tres consumidores. Cinco superficies y 12 franjas a 1 mm hacia dentro, sin colisionadores. Dos cápsulas de 1,80 m, azul/humano y roja/rival, con posiciones y visibilidad del preset. Geometría y recursos generados se reemplazan y liberan al reconstruir. La etiqueta de versión/FPS de I1 pasa a una cabecera transparente para dejar visible la cancha.

Materiales URP Unlit guardados en Resources (incluida la variante transparente para evitar depender solo de Shader.Find y de variantes no referenciadas en el Player). Suelo con textura de madera clara generada, paredes blancas, mismo rojo para todas las líneas y pared trasera alfa 0,25. Las cápsulas usan malla sin el collider que añadiría CreatePrimitive.

### Ajuste de encuadre autorizado por T2.3

| Clave | Propuesta inicial del plan | Valor final de I2 |
|---|---:|---:|
| `camera.height` | 4,5 | 8 |
| `camera.backOffset` | 2 | 5 |
| `camera.lookAtHeight` | 1,6 | 0,4 |
| `camera.fovDeg` | 62 | 90 |

El preset inicial falló dos pruebas de encuadre: una esquina del frontis daba x viewport 1,00824, fuera de [0,1]. El primer ajuste (8 / 5 / 1,6 / 88) pasó los seis puntos originales, pero la inspección visual mostró los extremos de la línea trasera recortados. Se añadieron los dos extremos superiores de esa franja a 2,18 m al test: RED, x viewport 1,02180. Con los valores finales los ocho puntos quedan dentro. Solo se modificaron claves camera.* del JSON, dentro de los rangos del catálogo, según el permiso explícito de T2.3; ninguna medida o umbral deportivo cambió. Coste: perspectiva más amplia y cámara elevada, pendiente del juicio visual del PO en el teléfono.

TDD runtime: contratos RED **7/7 fallos**; implementación con preset inicial **7/9 pasan**, con dos fallos de encuadre. Test adicional de línea trasera RED **2 fallos de 3**; final PlayMode **9/9 GREEN**. Un conflicto del nombre de namespace Camera con el smoke anterior se resolvió calificando UnityEngine.Camera antes del RED válido. Captura real del render de cámara a 1206×2622: `Logs/i2-court-editor.png` (sin la UI overlay); inspeccionada visualmente. La captura no sustituye la instalación ni la aceptación del PO.

## Evidencia y dispositivo

- Suites finales: EditMode **79/79**, PlayMode **9/9**, cero fallos y cero omitidos. [Resultados y hashes de XML](evidence/2026-10-21-i2-tests.json), [captura del editor](evidence/2026-10-21-i2-editor.png).
- `scripts/build-ios.sh dev`: código 0 y `BUILD SUCCEEDED`, usando Unity 6000.3.27f1 / Xcode 27.1. Metadata `0.1.0+1cbf00c dev`, `gitDirty=false`, Player no Development y Release. Ese hash identifica el código compilado; las actualizaciones posteriores de documentación no cambian la app.
- Firma estricta, bundle, equipo, perfil, dispositivo registrado y ARM64 del ejecutable y UnityFramework comprobados. [Inspección de la app](evidence/2026-10-21-i2-dev-inspection.json). El perfil vence el 28 de octubre, igual que en I1.
- El iPhone se consultó conectado por cable, paired/connected, Modo de desarrollador enabled, DDI disponible e iOS 26.6.2. La instalación tuvo éxito; el primer lanzamiento fue rechazado como `Locked` (CoreDevice 10002 / FBS error 7). **Resuelto:** el PO confirmó el teléfono desbloqueado y conectado; el arranque posterior funcionó. No se cambiaron herramientas, firma ni ajustes de seguridad.
- Arranque real: `0.1.0+1cbf00c dev`, `development=False`, `squashDev=True`, `devpanel=absent`. El log informa 60 FPS, Portrait, 1206×2622 y configuración `007325f26460`; se comprobó ese hash también de forma independiente a partir del JSON ordenado. [Log de arranque](evidence/2026-10-21-i2-dev-boot.txt), [resumen de ejecución](evidence/2026-10-21-i2-dev-run.json).
- [Captura nativa del iPhone](evidence/2026-10-21-i2-dev.png), inspeccionada visualmente: título/versión/FPS, suelo de madera clara, paredes, líneas rojas, tin y ambas cápsulas. Procede del LCD primario activo, sin duplicación; no es la captura del editor. La proporción y legibilidad definitivas siguen sujetas al PO.
- Tras guardar la captura y los logs se detuvo la consola de diagnóstico con Ctrl-C. El intento posterior de lanzamiento sin consola volvió a ser rechazado por `Locked`: el teléfono ya se había bloqueado de nuevo. Se registra como fallo de ese último comando, sin atribuirle código 0 ni certificar que la app quede abierta. El arranque previo y la captura real sí quedaron verificados. Dev permanece instalada y el PO puede abrir Squash Clash para su aceptación.
- Logs locales: `Logs/i2-dev-build-summary.log`, `Logs/build-dev.log`, `Logs/xcode-dev.log`, `Logs/i2-device-install.log`, `Logs/i2-device-console-final.log`, `Logs/i2-launch-final.log` y JSON de instalación/lanzamiento/captura. No se publican logs con identificadores del dispositivo.

El contador a 60 FPS es una muestra de arranque de una cancha estática; no acredita percentiles, temperatura, 15 minutos ni rendimiento de física/predicción de First Ball. Esta entrega de T2.3 usa dev; no se recompiló prod ni se probó debug en I2.

## Revisión y pendientes

La [revisión independiente interna](../reviews/2026-10-21-first-ball-i2-internal-review.md) de `6ad2961..1cbf00c` no encontró Critical ni Important. **Observación menor inicial, resuelta después mediante AX-1:** el texto del plan define +X hacia la derecha desde atrás, pero la pose explícita de cámara hace que se vea hacia la izquierda; el rival x=4,5 aparece a la izquierda del humano x=3,2. Se conserva la fórmula del plan, sin invertir ejes ni reflejar proyección por cuenta propia. Claude/PO deben aclarar la correspondencia antes de controles o decisiones direccionales; no afecta a interacción deportiva en esta cancha estática.

- Revisión de Claude comunicada por el PO: sin bugs de implementación; pide AX-1 antes de fusionar. No hay aprobación formal de GitHub Reviews.
- Validación visual del PO aceptada para esta primera versión; más ocupación de pantalla se difiere a avances de jugabilidad. No se retoca la cámara en esta corrección.

La vía de entrega ya está autorizada por el PO: PR hacia main, sin menú adicional de merge/push y sin fusión automática. El worktree se conserva para esa revisión. La spec no se cambia; el plan solo recibe la aclaración AX-1 expresamente autorizada. No se avanza a T3.1.


## AX-1 — convención izquierda/derecha tras la revisión de Claude

Aclaración y aceptación visual autorizadas por el PO el 2026-10-21, registradas en [aprobaciones](../reviews/2026-10-21-first-ball-po-approvals.md#i2--revisión-de-claude-aceptación-visual-y-aclaración-del-eje-x). Se conserva la pose y proyección de cámara, coordenadas numéricas, posiciones, UV y las medidas deportivas. Cambian los nombres semánticos de los lados:

- LeftWall pasa a x=6,40 y normal (−1,0,0); RightWall pasa a x=0 y normal (1,0,0). Las mallas y diagonales usan esos planos con el mismo desplazamiento de 1 mm hacia dentro.
- LeftBoxSide/Back nombran ahora la caja adyacente a x=6,40; RightBoxSide/Back, la adyacente a x=0. Las franjas físicas no se desplazan; se intercambian sus identificadores y casos de prueba.
- S3 se reetiqueta «lateral derecha» en la tabla del plan, sin reflejar inicio, dirección ni velocidad. No se implementan todavía escenarios ni simulación.
- El plan r2 se edita exclusivamente en la nota de aclaración, convención, tabla de superficies, nombres de cajas y etiqueta de S3. No se cambia la spec ni las revisiones originales de r2. El preset/cámara y los umbrales permanecen iguales.

**Verificación TDD:** EditMode RED 7 fallos de 79 (normales y cajas), luego **79/79 GREEN**. PlayMode RED 2 fallos de 11: el centro de la malla etiquetada LeftWall se proyectaba en x viewport=0,814282596 (derecha), y su plano estaba en x=0. Después, **11/11 GREEN**, con proyección de las mallas reales etiquetadas en sus respectivas mitades de pantalla, normales interiores, planos e inset de líneas. Se conservan los ocho puntos de encuadre existentes. [Resultados y hashes de XML de AX-1](evidence/2026-10-21-i2-axis-tests.json). Se corrigió el montaje inicial del nuevo test para separar la cámara y la cancha bajo una raíz neutra antes de registrar su RED válido.

Comandos: `scripts/test.sh editmode`, `scripts/test.sh playmode`. Logs/XML locales `Logs/i2-axis-{editmode,playmode}-{red,green}-*`. **Nueva dev `0.1.0+1b333ad`, `gitDirty=false`:** `scripts/build-ios.sh dev` terminó con código 0 / BUILD SUCCEEDED y la instalación mediante devicectl terminó con código 0 / outcome success. Firma estricta, ARM64, bundle ID, orientación Portrait y módulos de producto Core/Runtime verificados en la [inspección de AX-1](evidence/2026-10-21-i2-axis-dev-inspection.json). El perfil conserva su vencimiento del 28 de octubre. El teléfono requiere desbloqueo para comprobar el nuevo arranque; resultado pendiente de esa acción manual. PR #1 abierta, T3.1 y posteriores sin autorización.
