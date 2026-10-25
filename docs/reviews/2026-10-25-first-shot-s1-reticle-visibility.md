# S1 — visibilidad de la mira

Fecha: 2026-10-25. Base: **a8240fe8277896015c37a66805d0d0c21c8a6b70**, merge de PR #18. Rama: `firstshot/s1-reticle-visibility`. Implementación: **2850a56**. Fuentes probadas, exportadas e instaladas: **5e218ead0636d4eab23f4913b8f4911c4c117dd5**, árbol limpio. Los commits documentales posteriores no cambian el producto instalado.

La corrección despeja el frontis con DEV cerrado y agranda la mira sin cambiar parámetros del gesto. La entrega se somete a Claude **sin fusionar**. S1 sigue sin aceptación del PO; S2–S4/T8 no autorizados. [Autorización y antecedentes](2026-10-25-first-shot-po-approvals.md#cierre-documental-de-revalidación-y-corrección-de-visibilidad-autorizada--2026-10-25); [evidencia estructurada](../validation/evidence/2026-10-25-s1-reticle-visibility.json).

## Causa y alcance

La [captura del PO de c5a7d1e](../validation/evidence/2026-10-25-po-iphone-overlay-occlusion.png), con DEV cerrado, confirma la oclusión: ShotOverlay (fondo alfa 0,86, textos y cuatro botones) tapa el frontis y la mira indicada en (2,82; 4,34) m. El archivo suministrado mide **990×2000**, no la resolución nativa; SHA-256 **8e2ee2e5b1d134978f5acd548251a2eee9d4b21f72a977bad3f57fb54a7d9f85**, verificado antes de copiarlo byte a byte. Solo contiene IHDR/iCCP/IDAT/IEND, sin metadatos personales añadidos. Las medidas siguientes proceden del editor nativo, no de escalar esa captura. Lo rojo que mencionó el PO **probablemente era la cápsula del jugador**; se conserva como interpretación.

Solo cambian dos vistas:

- `ShotOverlay.cs`: una línea «Apuntando / Fijada / Inválida · P …%», dentro del área segura y encima de DevBar. Detalles y cuatro botones de protocolo pasan al scroll de DEV. Iniciar un protocolo cierra DEV antes de armarlo, permitiendo la captura que el panel abierto bloquea; acciones, registros, mediciones y escritura conservan su implementación.
- `AimReticleView.cs`: radio visual 0,18→0,42 m y ancho de línea 0,025→0,08 m como constantes privadas. Se conservan círculo/rombo, cian/amarillo/rojo y la clasificación de zona inválida. No hay claves nuevas de tuning ni cambio de radio físico de pelota.

Ganancia, umbral de flick, lookback, cámara, demás parámetros, física y autoridad permanecen intactos. **H3** (cuatro picos previos 63–242 mm/s frente a 250, ningún Frozen) y **H4** (3/4 terminan en y=5 m) quedan para evaluar después con la mira visible; esta PR no los ajusta. La sesión 311cb11 conserva su advertencia de histéresis defectuosa y no se recalcula ni se mezcla con esta comprobación.

## TDD y geometría

`ReticleVisibilityTests` carga la escena Main, cámara por defecto (3,2; 8; 14,75), FOV 90 y GameView **1206×2622 reales**, además de cámara y UI sobre el mismo RenderTexture. Aserta también Screen.width/height. El fixture declara área segura **(0,102,1206,2334)** en píxeles, equivalente a 62 pt arriba/34 pt abajo a escala 3; no representa una medición física de insets. En runtime se consulta **Screen.safeArea**. Se restaura GameView, RenderTexture y configuración de panel al salir.

Cinco casos comprueban: ausencia de intersección con el rectángulo proyectado completo del frontis (suelo→out, todo el ancho); tamaño mínimo a altura 2,2 m centrada; formas/colores; línea compacta dentro del área segura/sobre DevBar; y acceso a las cuatro acciones desde el scroll de DEV, cerrándolo antes de armar.

| Evidencia | Resultado |
|---|---|
| RED antes de implementar, commit **1bf5f8b** | 5 casos: 1 Pass, 4 fallos por aserción |
| Repetición separada de vistas anteriores con el fixture nativo final | 5 casos: 1 Pass, 4 fallos por aserción |
| GREEN focal final | 5/5 |
| Regresión fresca sobre 5e218ea, EditMode | **343/343**, 21:13:19–21:13:26Z |
| Regresión fresca sobre 5e218ea, PlayMode | **169/169**, 21:13:34–21:14:09Z |
| check-release dev/prod, exports frescos/limpios | **OK / OK**, sin framework de tests; DevPanel solo dev |

Los errores de preparación se mantienen aparte: un intento inicial usó Assert.Multiple no disponible y produjo CS/ningún XML; otro montaje del GameView usó un índice de retirada incorrecto y falló en TearDown. Ninguno cuenta como RED de comportamiento ni como validación. Se corrigió el montaje; las suites finales no tienen errores CS. La repetición RED nativa final conserva las vistas antiguas solo durante la ejecución y restaura después exactamente los bytes GREEN.

| Proyección a 1206×2622, Aiming centro y=2,2 m | Antes | Después | Mínimo autorizado |
|---|---:|---:|---:|
| Diámetro menor | 28,04248 px | **65,44067 px** | 60 px |
| Grosor de línea | 2,08264 px | **6,66443 px** | 6 px |

Después: diámetro horizontal 69,9765 px, vertical 65,44067 px. Son medidas geométricas por proyección de los vértices y del ancho de LineRenderer en el plano de cámara; no conteo de píxeles de antialiasing. La estimación anterior de Claude (~35 px / 2–3 px) se conserva como estimación; la tabla es la medida del fixture. Se verifica que ningún elemento visible de ShotOverlay intersecta el rectángulo proyectado del frontis.

Capturas del editor a resolución nativa, DEV cerrado, inspeccionadas visualmente:

- [Aiming: círculo cian](../validation/evidence/2026-10-25-reticle-editor-aiming.png).
- [Frozen: rombo amarillo](../validation/evidence/2026-10-25-reticle-editor-frozen.png).
- [Inválida: rojo, sobre out](../validation/evidence/2026-10-25-reticle-editor-invalid.png).

Las imágenes son evidencia del editor, no capturas del iPhone. Las aserciones de color/forma comprueban ambos estados válidos y el inválido sin modificar criterios. El editor serializó campos conocidos de GraphicsSettings/QualitySettings durante las ejecuciones; se comprobaron esas diferencias y se restituyeron byte a byte. Las suites canónicas y los exports se ejecutaron desde árbol limpio, con los archivos persistentes finales intactos.

## Build, instalación y preservación

Build dev limpia de **5e218ea**, Unity 6000.3.27f1, IL2CPP/Xcode **Release ARM64**, Player **no Development**. Auditoría de 2026-10-25T21:18:56Z: firma verificada, ejecutable/UnityFramework arm64, perfil efectivo compatible con dispositivo y bundle, caducidad **2026-10-28T14:59:31Z**. La instalación fue el 25 y no requería renovación. Antes de cualquier futura instalación comprobar siempre vigencia y renovar primero si es el 27 o después. Se cotejaron **625 archivos** con las fuentes probadas; BuildInfo limpio y DefaultPreset embebido coinciden. No se publica identificación del dispositivo, perfil, firma ni logs privados.

Se respaldaron P0-calib-1, Po-1 y last-active antes de instalar y se comprobaron idénticos después de instalar y después de abrir la nueva app. El primer lanzamiento automatizado fue rechazado por iPhone bloqueado; el PO la abrió y confirmó literalmente: «app abierta y muestra lo solicitado». En el arranque observado de **5e218ea dev**, registro **2026-10-25T21:21:59.8644060Z**, P0-calib-1 está activo con **9977d9ffd7e5 y cero pendientes**. No se aplican ni guardan cambios de parámetros para esta comprobación.

| Archivo persistido | SHA-256 antes y después |
|---|---|
| P0-calib-1.json | 2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd |
| Po-1.json | 9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767 |
| last-active.txt | 5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087 |

## Comprobación visual del PO

Pendiente de incorporar la captura y respuesta literal de esta comprobación breve. No se ejecutan 2 minutos, regla, series de flicks ni experimento 20→30/30→20 ms. Esta comprobación no sustituye las cinco preguntas de control ni acepta S1.

## Invariantes y entrega

Defaults **00a501ad7066**, los **41 valores heredados / proyección 0d39e6016bd8** y **SimulationKey 166a96e319c7** intactos. DefaultPreset.json conserva SHA-256 **9993cf20bf1affc1cf22453d84b1ede13edc1b8f3ab669bc47edb2821a5af0dc**. No se modifican spec, planes, criterios, ProjectSettings, Packages, scripts, escenas, cámara, Core ni pruebas anteriores. Se añade exclusivamente la suite visual descrita y su meta, además de documentación/evidencia.

Evidencia privada local: `Logs/evidence/reticle-visibility/` (XML, logs, comparaciones de fuentes, backup y capturas del contenedor); no versionada. PR #18 quedó fusionada con merge **a8240fe**, segundo padre **4601342ffbdfe1614bb4c14182577602b86d77a4**, main sincronizada y worktree de revalidación retirado sin force; sus artefactos se conservan en `Logs/archive-s1-revalidation-20261025` del checkout principal. Inventario antes/después coincidente, 31.296 archivos / 8.214.783.234 bytes, 275 hashes de Logs/XML. Ramas remotas y visibilidad privada conservadas.

Al entregar: PR lista para Claude sin fusionar y detenerse. La futura revalidación, las respuestas del PO, H3/H4 y el experimento de cobertura siguen pendientes; no se promueve ningún default. **S1 sin aceptar; S2–S4/T8 sin autorizar.**

## Referencias de implementación

La proyección usa [Camera.WorldToScreenPoint](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Camera.WorldToScreenPoint.html); el ancho de vista se expresa mediante [LineRenderer.widthMultiplier](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LineRenderer-widthMultiplier.html). El fixture captura UI y cámara con [PanelSettings.targetTexture](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.PanelSettings-targetTexture.html); la colocación runtime utiliza [Screen.safeArea](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Screen-safeArea.html). La selección temporal de GameView y su retirada se apoyan en la implementación del editor [UnityCsReference/GameViewSizeGroup](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GameView/GameViewSizeGroup.cs); quedan confinadas a la prueba bajo UNITY_EDITOR.
