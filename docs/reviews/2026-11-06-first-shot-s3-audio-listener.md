# First Shot S3 · AudioListener en la cámara principal

**6 de noviembre de 2026, Chile.** Rama `firstshot/s3-audio-listener` desde `adb8c8f` (merge de PR #34). Entrega para revisión independiente de Claude.

## Diagnóstico y corrección

En S3-A el PO no oyó sonido con el timbre activo y sí sintió vibración. El registro conserva cinco contactos aplicados y tres `Failed`, con sus ocho presentaciones. El bloque quedó como evidencia parcial. [Decisión literal del PO](https://github.com/rafaallendes-boop/squash-clash/blob/ade5ef21cd37e0256750d9258b3e7224636b1672/docs/reviews/2026-10-25-first-shot-po-approvals.md#detención-de-s3-y-corrección-puntual-del-audio--6-de-noviembre-de-2026-chile) · [Informe de S3-A](https://github.com/rafaallendes-boop/squash-clash/blob/ade5ef21cd37e0256750d9258b3e7224636b1672/docs/reviews/2026-11-06-first-shot-s3-physical-validation.md).

La única escena de los exports es `Main`. Antes del cambio tenía una sola cámara y ningún `AudioListener`. El export DEV de la sesión física también tenía una sola `Camera` y cero objetos con ClassID 81 en `Data/level0`, comprobado con `binary2text` de Unity. `AudioManager.asset` tiene audio habilitado y volumen 1. Unity necesita un listener para recibir el audio de la escena y recomienda la cámara principal como ubicación habitual. [Manual de Unity 6.3](https://docs.unity3d.com/6000.3/Documentation/Manual/class-AudioListener.html) · [Identificadores de clase de Unity](https://docs.unity3d.com/6000.3/Documentation/Manual/ClassIDReference.html).

En `c1fbf7b` se serializa un `AudioListener` habilitado en `Main Camera`. Una línea en `ProjectSetup.cs` añade el mismo componente al crear la cámara desde cero. Si la cámara ya existe, esa rama no se ejecuta, por lo que volver a ejecutar el setup sobre `Main` no añade un segundo listener. El fixture PlayMode de rollback solo añade `Camera` si no existe `Camera.main`. Los fixtures no requirieron cambios.

`ShotFeedback.Play` omite `PlayOneShot` en batch mode. Las pruebas de feedback acreditan peticiones y la prueba nueva acredita el listener activo al cargar la escena. La salida audible en iPhone queda pendiente de una comprobación física posterior.

## RED, GREEN y regresión

Unity `6000.3.27f1`. `BootstrapSmokeTests.MainScene_HasOneActiveAudioListenerOnMainCamera` carga `Main` en modo Single, verifica que `Camera.main` pertenece a la escena activa y cuenta los listeners activos y habilitados entre los objetos cargados. Exige exactamente uno y verifica que su GameObject sea el de la cámara.

| Ejecución | Inicio UTC | Resultado |
| --- | --- | --- |
| RED focal, solo la prueba (`d4f2628`) | 12:28:03 | 0/1. La aserción de cantidad recibió 0 y esperaba 1. |
| GREEN focal, con la corrección | 12:28:32 | 1/1 |
| EditMode completo | 12:28:40 | 546/546 |
| PlayMode completo | 12:28:54 | 354/354 |

Los cuatro resultados tienen XML fresco, cero omitidos y logs sin errores CS ni avisos de listeners duplicados. Antes del RED válido hubo un intento de compilación en el que `Camera` resolvió al namespace homónimo del proyecto. Se calificó como `UnityEngine.Camera` antes de la ejecución RED. `GraphicsSettings.asset` y `QualitySettings.asset` se restauraron tras PlayMode, con comparación byte a byte de `ProjectSettings/` frente a la copia inicial.

Para reproducir el RED, ejecutar `scripts/test.sh playmode MainScene_HasOneActiveAudioListenerOnMainCamera` en `d4f2628`. Repetir en `c1fbf7b` para el GREEN. Las regresiones completas usan `scripts/test.sh editmode` y `scripts/test.sh playmode`.

## Verificación de los exports DEV y PROD

`scripts/check-release.sh` terminó con `RELEASE-CHECK: OK`. Los dos exports fueron frescos, con fuente `c1fbf7b` y metadatos `gitDirty=false`. `ProjectSettings/` también coincidió byte a byte con la copia inicial al terminar.

La verificación lee el `Data/level0` de cada export. `binary2text` lista una instancia del ClassID 81 por perfil. Como los binarios exportados omiten los árboles de tipos, se usó UnityPy 1.25.4 en un entorno local ignorado para leer los campos con `read_typetree()`. Se comprobó que `Camera` y `AudioListener` tienen `m_Enabled=1` y apuntan al mismo GameObject, que este referencia ambos componentes, se llama `Main Camera`, tiene la etiqueta `MainCamera` (5), está activo y su transform es raíz. Cada export contiene un solo archivo de nivel, una cámara y un listener. El método inspecciona el contenido serializado de cada export y la prueba PlayMode verifica la activación al cargar la escena. [UnityPy](https://github.com/K0lb3/UnityPy).

| Perfil | PathID de cámara / listener / GameObject | SHA-256 de `Data/level0` |
| --- | --- | --- |
| DEV | 11 / 14 / 1 | `55021f3f01458b75cf3dabd9461c7df2ea3fbcd6f311c95432aaaa20c9eb917f` |
| PROD | 11 / 14 / 1 | `43ee9780ff0a3576a9282e6fbf5d5160d596e22c7b2d23c3d4383eb9e5b26193` |

El check-release se ejecutó tal cual. Se conservan los XML, logs, metadatos de release, volcado de clases y resultado de la inspección bajo `Logs/audio-listener/`, fuera de Git. La PR entrega el test, la escena, la línea de setup y este informe. La verificación llegó hasta el export de Unity para Xcode, sin compilación nativa, instalación ni acceso al iPhone.

## Configuración protegida

`DefaultPreset.json` coincide byte a byte con `adb8c8f`. La regresión comprobó defaults `c0d8ec7050e0` de 107 claves, el overlay P0 `6ecfdfa78cbb` y `SimulationKey` `166a96e319c7`. El cálculo offline a partir de los defaults y la copia de los 41 valores de P0 dio los mismos hashes. Los SHA-256 de los respaldos de `P0-calib-1.json`, `Po-1.json` y `last-active.txt` coincidieron antes y después. La PR #32 conserva su head `10f162f`.
