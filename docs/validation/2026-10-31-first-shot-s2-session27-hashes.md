# First Shot S2 — 13 hashes esperados de 2.7 (A3)

Fecha: 2026-10-31. Cálculo documental fuera de Unity y sin ejecutar la app. Revisión de Claude requerida **antes del primer bloque**. No cotejado todavía con Unity/iPhone; no son mediciones de sesión. [Protocolo y condiciones](2026-10-31-first-shot-s2-session27-predeclaration.md).

## Entradas y procedencia

Los archivos de producto coinciden byte a byte con la fuente aprobada `9131b1c`. El respaldo P0 se lee de la copia local histórica ya existente, sin acceder al dispositivo, modificarlo ni versionarlo. No se añade ningún script ni JSON al repositorio.

| Entrada | Origen | SHA-256 de bytes originales |
|---|---|---|
| DefaultPreset.json (9131b1c) | `Assets/SquashClash/Runtime/Resources/DefaultPreset.json` | `5f3108287f59ed17de8e5b504547dfd9384f69c1fd35e82678141f797beb4109` |
| P0-calib-1.json (respaldo local, 3614 bytes) | `Logs/h1-stage6-setup-presets/P0-calib-1.json` | `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd` |
| ParamSet.cs | `Assets/SquashClash/Core/Config/ParamSet.cs` | `86105715dbd4e8a83d39e582143f67fd055e9136c38e384b0429c85f5773c51d` |
| PracticeLaunchDefinitions.cs | `Assets/SquashClash/Core/Shot/PracticeLaunchDefinitions.cs` | `e5992f5402ba4441ec7d0dcc3dd00069d1d0c1b6ab98e8e72c90c4b67283d4c6` |
| PracticeMode.cs | `Assets/SquashClash/Runtime/Shot/PracticeMode.cs` | `225d1be27cc472a49dfa658278bee8ed69b37498be9fa607986fee3553b05514` |
| CourtDimensions.cs (Width=6.40f) | `Assets/SquashClash/Core/Court/CourtDimensions.cs` | `fd7b4c9d4574421b32374fec81da954726af664148d1cf3c460a314091a41546` |
| EffectiveConfig.cs (Court.WithTinTop) | `Assets/SquashClash/Core/Config/EffectiveConfig.cs` | `746f97550b2ef6f6a73e24f5d7d578b3e03d1320d7db945e65b7bd672bb4a98c` |
| Number.FormatDouble, export prod anterior 1bbaa39 | `Logs/evidence/s2-preparation-tanda1-20261031/archive-1bbaa39/mscorlib__3.cpp` | `26bb31ee63f0b5b8daa34728d475b2d7029cb19d41cd8f00228568c4a0148eef` |

La copia archivada de mscorlib__3.cpp se preservó y cotejó con el export anterior antes de generar exports nuevos; su SHA-256 es el mismo 26bb31ee… y los 13 literales no cambian. La ruta del archivo archivado es relativa a este worktree y queda ignorada, sin versionar el C++.

El origen del respaldo es relativo al checkout principal `/Users/rafaallendes/Projects/Squash Clash`, no un archivo del iPhone leído ahora. Sus 41 valores se superponen a los 106 defaults; se comprobó que la única diferencia de valores respecto de defaults es `launcher.repeatSeconds: 0 → 3`. Se preservan el preset y los 41 valores heredados.

## Canonicalización y cálculo

Se reproduce `ParamSet.ComputeHash`: claves ordenadas con comparación ordinal; cada línea `key=value` seguida de LF; UTF-8 sin BOM; double con formato `R`/InvariantCulture; SHA-256 completo del manifiesto; H son los primeros seis bytes, 12 hex minúsculos. No hay CRLF ni línea de metadatos en el manifiesto. Todos los conjuntos contienen 106 claves.

Para estos valores finitos se usa la ruta R del runtime exportado aprobado: decimal general de 15 dígitos significativos si su lectura devuelve el mismo double; si no, general de 17. Se leyó esa rama de `Number.FormatDouble` en el export prod **anterior** citado, sin abrir Unity, regenerar ni compilar. No se usa indiscriminadamente la representación decimal más corta de Python: en los doubles procedentes de float32 puede diferir del formato R de ese runtime. El cálculo externo usa redondeo binario32 explícito para cada operación float y conversión exacta de ese resultado a double.

Comprobaciones documentales: defaults → `915ae1e479b9`; B → **`dd122831f592`**; **H(1,V,0) = `dd122831f592`**. No son nuevas pruebas de producto ni cotejo con la app.

## Cuerpo y ancla B→SL1→SL

Cada condición parte de B recargado, aplica SL1 como ancla y después la receta de la fila. La comparación del cuerpo en `PracticeMode` convierte el valor aplicado a float antes de decidir si llama Set. Por ello human.x permanece double `3.2` en SL1/2/5/6, aunque el x float32 de la receta sea 3.2000000476837158. No se fuerza una asignación adicional. El cuerpo solo se reasigna al cambiar de receta y diferir en float.

| SL | x/z float32 de receta, bits hex IEEE-754 | human.x en manifiesto (R) | human.z en manifiesto (R) |
|---|---|---|---|
| SL1 | `0x404ccccd` / `0x40b00000` | `3.2` | `5.5` |
| SL2 | `0x404ccccd` / `0x40b00000` | `3.2` | `5.5` |
| SL3 | `0x3fa66666` / `0x4104cccd` | `1.2999999523162842` | `8.3000001907348633` |
| SL4 | `0x40a33334` / `0x4104cccd` | `5.1000003814697266` | `8.3000001907348633` |
| SL5 | `0x404ccccd` / `0x3fcccccd` | `3.2` | `1.6000000238418579` |
| SL6 | `0x404ccccd` / `0x3fcccccd` | `3.2` | `1.6000000238418579` |

En SL4, x se obtiene por resta **float32** `court.Width − 1.3f`, no convirtiendo el literal decimal 5.1 a float. Son las representaciones existentes de las recetas, no cambios de cuerpo/SL ni ajustes para hacer coincidir un hash.

### Manifiesto de cuerpos derivado

SHA-256 del siguiente texto UTF-8 sin BOM, con LF final: **`6d8da12c8623fcfb2f463b871c3d0b621ed385c351dfd1214d91ff81105da7f1`**. Las columnas x32/z32 son los bits IEEE-754 de la receta; human.x/z son los valores double que realmente quedan en ParamSet después de la comparación/asignación. Es una entrada derivada publicada aquí, no un archivo nuevo de presets.

```text
SL1|x32=404ccccd|z32=40b00000|human.x=3.2|human.z=5.5
SL2|x32=404ccccd|z32=40b00000|human.x=3.2|human.z=5.5
SL3|x32=3fa66666|z32=4104cccd|human.x=1.2999999523162842|human.z=8.3000001907348633
SL4|x32=40a33334|z32=4104cccd|human.x=5.1000003814697266|human.z=8.3000001907348633
SL5|x32=404ccccd|z32=3fcccccd|human.x=3.2|human.z=1.6000000238418579
SL6|x32=404ccccd|z32=3fcccccd|human.x=3.2|human.z=1.6000000238418579
```

## Literales por condición y deltas exactos frente a B

V: validLimit=1; T: validLimit=0; c=0 fija/c=1 seguimiento. La columna delta contiene **todas y solo** las claves cuyo valor difiere de B; el resto conserva el manifiesto B que se publica abajo. SHA-256 corresponde al manifiesto canónico completo, no solo al delta ni al Markdown.

| Condición | H esperado | Delta exacto frente a B (valores R) | Bytes UTF-8 | SHA-256 del manifiesto completo |
|---|---|---|---:|---|
| H(1,V,0) | **`dd122831f592`** | Ninguno | 2288 | `dd122831f59295bba71678b03ffffff5c86b54780bae43e8896ca6e71191a470` |
| H(1,T,0) | **`2505640b1d88`** | `timing.validLimit=0` | 2288 | `2505640b1d88257c874bbbeb5d78abc8cb299313a5867c6dd5dc73b633012420` |
| H(2,V,0) | **`3e555176e725`** | `practice.launch=1` | 2288 | `3e555176e725117e6aa66a19b34648b81e57c54cd3f565cd6f357b3b2c9fe9d5` |
| H(2,T,0) | **`b4fc81c8d4d5`** | `practice.launch=1`; `timing.validLimit=0` | 2288 | `b4fc81c8d4d57bf7d09bdecc373c24573bfc52b1ffff07a691047e95db3f5936` |
| H(3,V,0) | **`1d47540a023e`** | `human.x=1.2999999523162842`; `human.z=8.3000001907348633`; `practice.launch=2` | 2318 | `1d47540a023eb084551f05e383f894401d4ed8d069be14e032c2fa509abcbc2d` |
| H(3,T,0) | **`76e313deebb2`** | `human.x=1.2999999523162842`; `human.z=8.3000001907348633`; `practice.launch=2`; `timing.validLimit=0` | 2318 | `76e313deebb2992c7ef33fd270a9b8ce0ce63b2ad4f4b3d3acb7804f07577bf4` |
| H(4,V,0) | **`d765ec727b4b`** | `human.x=5.1000003814697266`; `human.z=8.3000001907348633`; `practice.launch=3` | 2318 | `d765ec727b4bb58dd8262ae8d0909be48e5ac63d1bc61aafc25d3df6e3f8531f` |
| H(4,T,0) | **`122f7a4b16b4`** | `human.x=5.1000003814697266`; `human.z=8.3000001907348633`; `practice.launch=3`; `timing.validLimit=0` | 2318 | `122f7a4b16b47a28b11a4287b6322400e241a52c06c6c9ac3c15b27ad8d5e91f` |
| H(5,V,0) | **`85f1c31ed871`** | `human.z=1.6000000238418579`; `practice.launch=4` | 2303 | `85f1c31ed871313c8609a2d1d2d4031c7ee352816f970cc62753863a495a7198` |
| H(5,T,0) | **`5fb30723ab9b`** | `human.z=1.6000000238418579`; `practice.launch=4`; `timing.validLimit=0` | 2303 | `5fb30723ab9b94610c292d8256ac0f160502a0f1262c8f57b6cce807796d230c` |
| H(6,V,0) | **`18bbcbe64304`** | `human.z=1.6000000238418579`; `practice.launch=5` | 2303 | `18bbcbe64304f79ac6a59b6663ae23a48ba9b189fa55b15064650f053c7a3391` |
| H(6,T,0) | **`3b1ed25fbab0`** | `human.z=1.6000000238418579`; `practice.launch=5`; `timing.validLimit=0` | 2303 | `3b1ed25fbab04f337b4b0b98861b1176af56d01e99526aa30044875b0f7ff97e` |
| H(1,V,1) | **`398bae34bb81`** | `camera.mode=1` | 2288 | `398bae34bb81e553bfbf1f6f442c2b3e9598dba678fcfc69d336f9bb0462ebe4` |

Los 25 bloques usan estas 13 condiciones según la tabla del protocolo. El SHA-256 de cada manifiesto identifica íntegramente sus 106 valores; H es su prefijo de 12 hex. `presetFileHash` del respaldo, H efectivo y SHA-256 completo del manifiesto son identificadores distintos.

**Cotejo futuro obligatorio:** Claude revisa ahora estos literales/deltas; cuando la preparación esté autorizada, cotejar efectivo/recepción y luego gesto con el literal correspondiente. Si Unity o el dispositivo no coincide —incluida cualquier diferencia de formato R o representación float— registrar las entradas y el observado y **detenerse**. No sustituir el literal por el leído al armar, redondear el cuerpo ni cambiar una clave para lograr igualdad. No se afirma que ese cotejo ya haya ocurrido.

## Manifiesto canónico B completo

SHA-256: `dd122831f59295bba71678b03ffffff5c86b54780bae43e8896ca6e71191a470`; 2288 bytes UTF-8; 106 líneas, incluida LF final. La valla Markdown no forma parte de los bytes. Aplicar los deltas de cada fila y volver a ordenar las claves para reconstruir cualquier manifiesto H.

```text
aids.ballVisualScale=1
aids.impactMarks=1
aids.prediction=0
aids.shadow=1
aids.trail=1
aids.trailSeconds=0.15
aim.flickConfirmSamples=2
aim.flickHysteresisMmPerS=40
aim.flickMmPerS=250
aim.freezeLookbackMs=50
aim.freezeOnRelease=0
aim.gainMPerMm=0.12
aim.holdMaxGapMs=51
aim.holdMaxSpeedMmPerS=15
aim.holdMinSamples=3
aim.holdMs=120
aim.holdRadiusMm=1
aim.lockFlashMs=120
aim.startHeight=2.2
body.idealLateral=0.6
body.preferredMaxHeight=1.1
body.preferredMinHeight=0.4
body.reach=1.3
body.wallClearance=0.3
camera.backOffset=5
camera.followDamping=0.3
camera.followMaxOffset=1.5
camera.fovDeg=90
camera.height=8
camera.lookAtHeight=0.4
camera.mode=0
court.tinTop=0.48
debug.slowMotion=1
gesture.cancelRadiusMm=6
gesture.control=2
gesture.slingCancelAboveMm=12
gesture.slingCancelLateralMm=20
gesture.slingCancelMinDepthMm=8
gesture.slingPreferLeft=1
gesture.slingTarget1X=0.25
gesture.slingTarget1Y=0.15
gesture.slingTarget2X=0.75
gesture.slingTarget2Y=0.15
gesture.slingTarget3X=0.5
gesture.slingTarget3Y=0.5
gesture.slingTarget4X=0.25
gesture.slingTarget4Y=0.8
gesture.slingTarget5X=0.75
gesture.slingTarget5Y=0.8
gesture.tapMaxMm=3
gesture.tapMaxMs=250
human.x=3.2
human.z=5.5
input.fallbackDpi=460
launcher.pitchDeg=10
launcher.repeatSeconds=3
launcher.speed=15
launcher.x=3.2
launcher.y=1
launcher.yawDeg=0
launcher.z=7
legal.maxHeight=2
legal.minHeight=0.05
pace.launchScale=1
pace.tempo=0.65
physics.back.friction=0.2
physics.back.restitution=0.55
physics.backend=1
physics.floor.friction=0.3
physics.floor.restitution=0.45
physics.front.friction=0.2
physics.front.restitution=0.55
physics.gravity=9.81
physics.maxImpactsPerStep=4
physics.side.friction=0.2
physics.side.restitution=0.55
physics.stepHz=120
placeholders.visible=1
power.curveExponent=1
power.deadZoneMmPerS=100
power.fullMmPerS=1500
power.maxGapMs=34
power.method=0
power.minCoverageMs=30
power.minSamples=4
power.peakDecayMs=120
power.peakSmoothingMs=20
power.pullDeadZoneMm=2
power.pullFullMm=35
power.windowMs=80
practice.enabled=1
practice.launch=0
practice.observeMaxSeconds=4
practice.repeatSeconds=1.5
rival.x=4.5
rival.z=6.5
rollback.historyMs=250
shot.vCap=32
shot.vMax=28
shot.vMin=6
timing.assistMs=40
timing.earlyPolicy=0
timing.latencyCompMs=0
timing.showNotes=1
timing.validLimit=1
timing.validMs=100
```
