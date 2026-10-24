# I6a / T6.1–T6.2 — cámara y ayudas de legibilidad

Estado, 2026-10-24: implementación, regresión y recorrido funcional nativo entregados para revisión. Rama `firstball/i6a-legibility`, [PR #10 lista para Claude, abierta y sin fusionar](https://github.com/rafaallendes-boop/squash-clash/pull/10). Detenerse al entregar para revisión de Claude y decisión del PO, sin fusionar. No T6.3/L1–L6 ni gameplay; H1 pendiente de evaluación posterior del PO.

## Autorización y cierre previo

Claude aprobó I5b / PR #9 head 1496874 y el PO autorizó cierre: merge commit **f8920e8a99f3ebf555f53d7ead1da8677cedcce1**, main sincronizada; cierre registrado en **848fc5e**. Worktree I5b eliminado; Logs/Builds preservados en Logs/archive-i5b-20261024, caché Library/UserSettings trasladada a I6a sin versionarla. Defaults al cierre I5b **164b0c21e88b**, P2 por defecto/P1 respaldo.

La Etapa 5 termina en **T5.3**; la referencia a T5.4 era incorrecta en r2 y se corrigió en documentación de I5. La correspondencia/revisión histórica de r1 conserva su numeración original. El PO calibrará un preset propio y avisará para registrarlo: ese punto de control sigue pendiente. **Po-1 es prueba funcional, no calibración**. I6a autorizado por separado mientras el PO prepara su preset.

## Comportamiento entregado

**Cámara:** `CourtCameraRig.Apply` incorpora modo fija/seguimiento, amortiguación y límite lateral. `Tick` sigue la x de RenderPosition con SmoothDamp, limitado a ±followMaxOffset respecto del centro; cámara y punto de mira se desplazan igual, preservando orientación. Cero amortiguación permite seguimiento inmediato; delta cero se omite para evitar NaN en la velocidad interna. Modo fija conserva el encuadre de referencia y no sigue la pelota. AppBootstrap hace Tick en LateUpdate, después de la presentación del host, y lo congela en pausa. Cambios Immediate llegan sin relanzar. No existe aún apuntado/golpe: su congelación durante la mira corresponde a gameplay posterior, sin anticiparlo aquí. [SmoothDamp de Unity 6](https://docs.unity3d.com/ja/current/ScriptReference/Mathf.SmoothDamp.html).

**Ayudas:** `LegibilityAids` consume RenderPosition, ContactRaised, Prediction y TuningState.Changed del mismo host; no crea simulaciones ni colisiones. Sombra en (x; 0,001; z), disco oscuro cuyo tamaño/opacidad disminuyen con altura y conservan un mínimo visible: estilo experimental para juicio del PO. Estela TrailRenderer corta con duración independiente de τ/λ/cámara lenta del SimClock (Time.timeScale se mantiene en 1). Marcas solo en impactos no persistentes contra las cuatro paredes, orientadas por la normal y separadas 2 mm del plano para evitar z-fighting; máximo seis, desvanecimiento de 1,5 s reales. Predicción opcional con LineRenderer y los estados de la predicción aislada existente. Relanzar limpia estela/marcas y actualiza la trayectoria; destruir desuscribe eventos y libera mesh/materiales/objetos propios.

Shader URP local mínimo para color por vértice y transparencia de las ayudas; prueba de profundidad normal, sin dibujarlas siempre por encima de jugadores. No nuevos paquetes ni framework de render. [Estructura oficial URP](https://docs.unity3d.com/cn/6000.0/Manual/urp/writing-shaders-urp-basic-unlit-structure.html), [TrailRenderer](https://docs.unity3d.com/kr/6000.0/ScriptReference/TrailRenderer.html). Que las ayudas no tapen al rival ni sean imprescindibles se juzga por el PO en el gate de legibilidad; no se infiere de tests funcionales.

**Pelota:** sigue negra. `BallView.ApplyVisualScale` modifica solo el dibujo; radio físico **0,020 m** intacto, sin cambiar estados, predictor ni generación de simulación. Escala 2 dibuja diámetro 0,08 m; la escala configurable llega a 2,5. Default visual 1 según plan; ningún preset de calibración se promueve.

## Configuración

DefaultPreset.json sigue como única fuente de defaults. **Hash nuevo 0d39e6016bd8**, 41 claves. Los **32 valores anteriores son idénticos**; P2 por defecto, P1 configurable. Solo se agregan las nueve claves previstas, todas Immediate y generadas por el panel existente:

| Clave | Default | Rango |
|---|---:|---|
| camera.mode | 0, fija | 0 fija / 1 seguimiento |
| camera.followDamping | 0,3 s | 0–1 |
| camera.followMaxOffset | 1,5 m | 0–3 |
| aids.shadow | 1 | Bool |
| aids.trail | 1 | Bool |
| aids.impactMarks | 1 | Bool |
| aids.prediction | 0 | Bool |
| aids.trailSeconds | 0,15 s | 0–0,5 |
| aids.ballVisualScale | 1 | 1–2,5 |

Presets anteriores parciales reciben las nuevas claves desde Defaults mediante Overlay, sin reescribir sus archivos. No se modifica la semántica del panel ni el error de importación; esa observación de Claude queda registrada para su próxima intervención específica.

## TDD y regresión

Fuente **75072666956d6529ab120d640c3df6f40391b93d**. **262/262 EditMode y 120/120 PlayMode**, cero fallos/omitidos, XML frescos y compilación sin errores. Cinco ejecuciones RED válidas cubren estos comportamientos antes de implementarlos:

| Comportamiento | RED observado | GREEN |
|---|---|---|
| Defaults/catálogo | nueve claves ausentes, 9/9 fallos | 9/9 |
| Seguimiento | no se mueve / no responde con damping cero, 2/3 fallos | 8/8 junto a cámara existente |
| Sombra y toggle | no existe el disco, dos fallos | posición, atenuación con altura, toggle inmediato |
| Escala visual | 0,04 observado frente a 0,08 esperado | físico/predicción/estado intactos |
| Estela | no existe TrailRenderer | duración/toggle/longitud inmediata, cero la desactiva |
| Marcas | cero frente a seis; no existe marca para desvanecer | paredes no persistentes, tope seis, 1,5 s reales |
| Predicción | no existe LineRenderer | estados idénticos, toggle sin mutar predictor |
| Host y app | no llega contacto, no limpia al relanzar, cámara no hace Tick y ayudas no conectadas: 4/5 fallos | 15/15 cámara/ayudas/integración |

Intentos rechazados, **no contados como RED ni GREEN**: inicialización de MaterialPropertyBlock en un inicializador de MonoBehaviour (GREEN fallido, movida a Initialize); CS1529 por ubicación de using; CS0117 porque Assert.Multiple no existe en NUnit del proyecto (aserción sustituida, RED válido observado después y antes de conectar). Cero tests/XML viejo/CS no se aceptan. [Manifiesto con ejecuciones y hashes](../validation/evidence/2026-10-24-i6a-tests.json).

QA visual editor: imagen 720×1566 generada por integración y revisada: pelota negra con escala 2,5 y sombra en suelo, cancha/referencias conservadas. Artefacto local ignorado Logs/i6a-legibility-editor.png, hash en manifiesto. No es aceptación H1 ni medición de rendimiento.

## Build e iPhone

Exports limpios dev/prod **RELEASE-CHECK: OK**, fuente limpia 7507266. Prod sin DevPanel ni puente de prueba física; puente térmico runtime en ambos. Dev **0.1.0+7507266**, Release ARM64 firmada, firma estricta, metadata/preset exactos y 115 hashes de fuentes cotejados. Unity 6000.3.27f1 y Xcode 27.1 (27A9275) sin cambios. Perfil efectivo compatible con dispositivo/bundle, vigente hasta **2026-10-28T14:59:31Z**, comprobado de nuevo el 24 antes de instalar. [Build saneada](../validation/evidence/2026-10-24-i6a-native-build.json).

El primer intento falló CoreDeviceError 4016, sin instalar. Tras conectar el PO el teléfono, respaldo de presets e **instalación exitosa** el 2026-10-24. Apertura automática --console exitosa; log de arranque P2-Analytic, cero colliders visuales y cero escenas PhysX locales, configuración **fed535c0b990** de Po-1 (preset de prueba, no default/gate). Sin excepciones ni errores de shader observados en la instantánea de consola. Label 7507266, app y encabezado Po-1 con cero pendientes observados; DEV abierto por teclado. Ningún archivo de preset escrito por el agente.

**Recorrido funcional confirmado por el PO en el teléfono:** «HIce los cambios, funcionó, pero cuando cerré la app y volví a abrir. Partio con las configuraciones iniciales. Si era lo esperado, sigue avanzando». Respuesta al recorrido solicitado: escala 2 con pelota mayor/sombra/estela/marcas; predicción on/off; seguimiento con lanzamiento lateral temporal y retorno a fija; Cargar Po-1 y Aplicar sin guardar. Se registra como confirmación general del recorrido por el PO, sin atribuir al agente observación visual directa de cada ayuda. Duplicación rechazó clics/scroll con windowNotFoundAtPosition; el agente corroboró arranque/label/DEV y el PO realizó la prueba física funcional.

**Reinicio y persistencia:** es esperado descartar cambios temporales que no se guardaron. Al abrir se carga el último preset guardado, Po-1, y sus 32 claves se completan con las nueve nuevas desde defaults: cámara fija, escala 1 y predicción apagada. El respaldo previo contiene floor restitution=0,60; el arranque observado antes del recorrido fue Po-1/config **fed535c0b990**, distinto del default **0d39e6016bd8**. Guardar como… permite conservar ajustes posteriores; no se pidió guardar los de esta prueba ni se promueve Po-1 a calibración. El PO describió retorno a configuraciones iniciales sin identificar encabezado/hash. Las primeras copias posteriores fallaron por timeout (2) y CoreDeviceError 4016; una nueva revisión solicitada por el PO permitió recuperar presets y métricas. El archivo **Po-1 sigue idéntico** (SHA-256 9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767) y el arranque del **2026-10-24T20:28:18.6556670Z** carga Po-1/fed535c0b990 con cero pendientes. Queda corroborada la recuperación que antes no pudo verificarse; no pérdida del preset.

**Revisión posterior de archivos:** cinco JSONL de 7507266, 318 registros (57 launch y 261 frames), cero líneas JSON inválidas. Se preservan hashes/configuraciones y validez de métricas en los logs locales; no se calculan resultados de rendimiento ni se atribuye a estos datos la comprobación visual de ayudas. El recorrido visual sigue confirmado por el PO.

Después de ese arranque se observa selección de Default, configuración de repetición a 3 s y guardado/uso de **P0-calib-1**. Su archivo tiene 41 claves, hash efectivo **8de3f2ecb217**, SHA-256 **2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd**, y solo difiere de defaults en `launcher.repeatSeconds=3` (default 0). last-active.txt apunta a ese nombre y dos lanzamientos registran el mismo hash de archivo/configuración, con cero pendientes. Es evidencia de guardado/uso, **no aprobación de calibración** por su nombre; punto de control del preset propio sigue pendiente de indicación expresa del PO. No se reinició la app para probar recuperación de P0-calib-1, ni se escribieron archivos en el teléfono, reinstaló o cambió código.

[Auditoría funcional y atribución de evidencia](../validation/evidence/2026-10-24-i6a-native-functional.json). **Entrega I6a preparada; PR #10 lista para revisión de Claude, abierta y sin fusionar.** No se declara aprobación de Claude ni cierre de I6a.

No campaña de física ni rendimiento. Si se requiere otra instalación desde el 27, renovar antes y comprobar siempre vigencia. La apreciación H1 completa corresponde al PO con T6.3 posterior.

## Observaciones y límites vigentes

- Próxima intervención específica del panel: error de importación debe mostrar longitud y primeros caracteres pegados; offset 0 indica contenido no JSON. No se cambia código del panel ahora.
- T7.2 mide con panel cerrado, porque abierto refresca y ordena cada 0,5 s.
- Prueba física P2 usa configuración aplicada: preset no default queda fuera de configuración del gate. No se ejecuta aquí.
- Retención de logs antes de T8; renovar perfil antes de instalar desde el 27 de octubre, verificar siempre vigencia.
- Punto de control de Etapa 5 pendiente del preset calibrado propio del PO; Po-1 no es calibración.
- T6.3/L1–L6, H1 y gameplay no adelantados. Spec, criterios/umbrales, escenarios, motores y ProjectSettings intactos.
