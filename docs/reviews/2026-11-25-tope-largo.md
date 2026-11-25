# Tope de largo de 8,5 m en la guardia «Drive adelante»

25 nov 2026 · Claude (Opus) · rama `rafaallendes-boop/sc-tope-largo` desde `main` `1dc9dcb`. **Claude implementa y mide, así que esta revisión no es independiente.** Falta la revisión cruzada de Codex en solo lectura.

## Decisión del PO

Rafael, 25 nov, 10:43, chat de SC-PM: «Sí, tope de largo a 8,5 m». Es la opción 1 del [diagnóstico del 25 nov](2026-11-25-golpe-adelante-largo.md), que este PR incluye. Autoriza cambiar la definición de éxito de la guardia del 20 nov y agregar ese umbral, y nada más. [Registro](2026-11-18-po-approvals.md#25-nov-2026--tope-de-largo-de-85-m-en-la-guardia-drive-adelante) · [spec, §7](../superpowers/specs/2026-11-20-front-deep-drive-design.md#7-revisión-2-25-nov-tope-de-largo).

## Cambio

`FrontDeepDrive.Deep` exige además que el primer bote caiga en z ≤ `MaxFirstBounceZ` (8,5 m, constante con nombre) y que la pelota no toque el vidrio de atrás antes de ese bote. Son 2 líneas del predicado y la constante. Vale también para el golpe que antes salía «already-deep». No cambian la zona, el barrido, las alturas de respaldo, la cota de 40 evaluaciones, la velocidad, el solo humano ni el interruptor DEV.

**Sin cambios:** S1, S3a, `pullFullMm`, `DefaultPreset.json`, SimulationKey, RallyParams y los hashes de §5 (defaults `207da8f2c636`, SimulationKey `166a96e319c7` y RallyParams `ebf8f717f8c5`, comprobados en `source.tsv` de las cuatro corridas). El tope no obligó a tocar ninguno.

## Números antes y después

Experimento explícito `FrontLengthCeilingExperiment` (`scripts/diagnose-front-length-ceiling.sh`), P2-Analytic, mismo método sobre `f4844f3` (antes) y `357d2ab` (después; el código de producto es el mismo que en el head). Las corridas `before01`=`before02` y `after01`=`after02` dieron TSV idénticos byte a byte. El juez del largo es independiente del código: es la función `Measure` del diagnóstico, con 8,5 m propio.

**Golpes reales del PO, 23–24 nov**: los 49 golpes al frontis desde adelante con P > 0,5 (sin intentos a dos paredes), la misma población del diagnóstico. 39 caen en la zona de la guardia.

| | Antes | Después |
|---|---|---|
| Largos (primer bote > 8,5 m o vidrio al vuelo) | **20/49** | **1/49** (fuera de la zona: mira < 0,80) |
| Vidrio al vuelo | 14/49 | **0/49** |
| Lateral antes del bote | 8/49 | **8/49** (los mismos) |
| Vidrio después del bote, antes del 2.º | 30/49 | 29/49 |
| Legales | 49/49 | 49/49 |
| Primer bote, mediana (zona) | 6,93 m | 7,17 m |
| La guardia baja la altura (zona) | 2/39 | 12/39 (mínima 0,63 m en los dos casos) |

**Costo por golpe en la zona** (evaluaciones = solicitudes al solver más trayectorias P2; tiempo en el editor del Mac, mejor de 3):

| | Antes | Después |
|---|---|---|
| Reales (39): media / mediana / p95 / máx. | 7,97 / 8 / 21 / 22 | 11,46 / 9 / 30 / 31 |
| Malla (1458): media / p95 / máx. | 7,02 / 21 / 36 | 8,70 / 30 / 36 |
| Tiempo, reales: mediana / p95 | 0,12–0,17 / 0,28–0,46 ms | 0,13 / 0,37–0,40 ms |

La cota sigue en 40. El predicado nuevo corta antes la misma simulación, así que cada evaluación cuesta lo mismo. Lo que sube es el número de evaluaciones. En el iPhone el costo queda en `SolverCostMs` de la línea `shot`.

**Malla determinista de la zona**: contactos a z de 1 a 5,4 m, P = 0,70/0,85/1, timing perfecto, 1458 golpes.

| | Antes | Después |
|---|---|---|
| Largos | 441 | 7 |
| Vidrio al vuelo | 239 | 0 |
| Lateral antes del bote | 7 | **21** |
| Sin solución (`no-deep-x`, devuelve el original) | 17 | 32 |

**Las 14 laterales nuevas de la malla** son todas `no-deep-x` con contacto a z = 1,0–1,5 m. Pegado al frontis y a esa velocidad, ninguna x ni altura bota dentro de 8,5 m y llega al tercio trasero, así que la guardia devuelve el golpe original, que va a la lateral. Antes la guardia las convertía en un drive largo o al vidrio; ahora tocan la lateral y **botan corto, a z ≈ 0,14–2,4 m**: el drive fuerte pegado al frontis termina como una pelota corta. En la muestra S1 (810) hay además 6 laterales nuevas `no-deep-x` (primer bote a 1,5–4,0 m, una de ellas Out). Ninguna lateral anterior desaparece. En los golpes reales no ocurre. Queda declarado en la spec, sin compensarlo.

**Golpes que pasan de legales a fuera de la cancha** (lo señaló la revisión cruzada; recontado desde los TSV). Sin el rescate de la guardia (`no-deep-x`, se devuelve el original), algunos golpes que antes eran `front-deep` y legales vuelven a salir **Out** sin botar:

| | Antes | Después | Golpes |
|---|---|---|---|
| Malla, legales en zona (1458) | 1452 | **1451** | `cx 3,2 · cz 1 · mira (3,2; 2) · P 0,70`; antes botaba a 9,37 m |
| Muestra S1, legales (810) | 794 | **790** | `SL5/SL6-Tstar-12 · mid · mira (2,6 \| 4; 2,55) · P 0,60`; antes botaban a 8,7–9,0 m |
| Golpes reales del PO (49) | 49 | 49 | no ocurre |

Es un riesgo de producto que el PO debe ver antes de fusionar: falta saber si en ese caso prefiere un largo legal al vidrio o el golpe original fuera. Este PR no toma esa decisión.

**S1, solo informativo** (no se toca): con la misma muestra y el mismo juez del diagnóstico, 592/794 (74,6 %) antes y 597/790 (75,6 %) después. **No es una mejora:** el denominador baja de 794 a 790 por los cuatro golpes que ahora salen Out; sobre los 810 golpes, S1 pasa de 592 a 597 (73,1 % → 73,7 %). Sigue bajo el 90 % por el choque S1–guardia, que es la opción 2 y sigue pendiente del PO.

## Pruebas (TDD)

- **RED** en `7d145ff` con el filtro `FrontDeepDriveTests`: 13 pruebas, 7 pasaron y 6 fallaron por aserción, sin errores CS. La constante `MaxFirstBounceZ` se agregó como stub. Fallaron las dos pruebas nuevas con golpes del PO (vidrio al vuelo y bote a 9,46 m), la nueva `Deep_RejectsALongDrive…` y las tres pruebas de éxito existentes, a las que se agregó la aserción del tope.
- **GREEN**: el cambio va en `357d2ab` y los fixtures se alinean en `87a1bfc`; `FrontDeepDriveTests` da 13/13 y `ShotControllerTests` 79/79.
- **Aserciones existentes que cambian** (todas por la definición autorizada, y ninguna es un umbral):
  - `Straight…`: la altura ya no se conserva. A su altura ninguna x cumple el tope, así que baja de 1,96 a 1,47 m sin bajar del mínimo legal. `Cross` sigue conservando la altura.
  - `VeryFront…` pasa a `VeryFront_NoXWithinTheCeiling_ReturnsTheOriginal`. Con contacto a 0,77 m ya no hay solución dentro del tope: devuelve el original bit a bit (`no-deep-x`) y la lateral vuelve. Es el riesgo declarado.
  - PlayMode `IntentDrive_ShowsTheEffectiveAim…` (G1 A2, 18 nov): su golpe SL6 con P = 0,8 y mira 0,88 cae en la zona. Antes era «already-deep» y ahora es largo, así que la guardia actúa. La prueba corre con el interruptor DEV apagado, en try/finally, para aislar A2. No cambia ninguna aserción.

## Textos

Sin cambios de texto. El interruptor DEV «Drive adelante: sí/no», la retícula y los avisos del golpe siguen igual.

## Puertas

Sobre `0812a0f` (el código de este PR; después solo se agregan este informe y, tras la revisión cruzada, declaraciones en el informe y la spec §7, sin tocar código ni pruebas), con el árbol limpio, en serie y con el lock de Unity, en el Mac del PO:

| Puerta | Resultado |
|---|---|
| EditMode | **1036/1036** (0 fallidas, 0 omitidas) |
| PlayMode | **863/863** (0 fallidas, 0 omitidas) |
| AutoQA `mvp` v / t3 / volley | PASS / exit 0 / nivel 1 en las tres, 102/102 cada una, `dirty: false` |
| check-release | OK (build 1217, dev y prod) |

`scripts/device.env` se copió sin cambios desde `sc-install-518f9c3`, el origen autorizado (SHA-256 `0b0c94db…`, idéntico). No se versionó y se borró al terminar. Evidencia local, ignorada por git: `Logs/evidence/tope-largo/` (XML, salidas de AutoQA y check-release, `gates.txt`) y `Logs/experiments/front-length-ceiling/` (las cuatro corridas).

## Qué mirar en el iPhone

- [ ] Desde adelante y con tirón fuerte, el drive al fondo ya no pega en el vidrio al vuelo y bota antes de llegar al vidrio.
- [ ] El drive sigue sintiéndose fuerte y profundo, no corto ni bombeado.
- [ ] Pegado al frontis (a menos de 1,5 m), con tirón fuerte: ¿vuelve a irse a la lateral y bota corto?
- [ ] Desde adelante con mira alta (≈ 2–2,5 m) y P media-alta: ¿algún drive sale fuera de la cancha sin botar?
- [ ] La retícula marca dónde pega de verdad en el frontis cuando la guardia cierra el ángulo o baja la altura.
