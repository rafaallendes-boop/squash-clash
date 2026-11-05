# First Shot — diagnóstico SL5/SL6 desde contactos registrados

Fecha: 2026-11-04, America/Santiago. **Entrega documental para Claude/PO, sin elegir corrección ni fusionar.** Base PR #30/dfffd58, producto 9131b1c; dev instalada 2ef4807 no tocada. [Método predeclarado](../diagnostics/2026-11-04-sl56-p2-method.md), commits **975b597/2141676 antes del primer cálculo**; auditoría del arnés4a55d80 antes de publicar. [Derivado completo](../validation/evidence/2026-11-04-sl56-p2-offline.json) y [reproducción](../diagnostics/2026-11-04-sl56-p2-reproduction.md). No es campaña nueva, replay humano, regresión ni implementación de reglas.

## Contrato de la mira cerca del frontis

| Fuente | Contrato aplicable | Límite |
|---|---|---|
| [Spec v0.3](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md)§6.4 | Mira en coordenadas del **frontis**, metros; tin/out pueden señalar error. | No define punto del primer bote ni profundidad final como el punto de la mira. |
| Spec§7.4 y [propuesta First Shot r2](../superpowers/specs/2026-10-24-first-shot-design.md)§11.5 | Potencia fija **velocidad total**, objetivoP en frontis, raíz balística baja; si infactible subir velocidad hasta tope/luego bajar punto factible, registrando intención/ajuste/salida. | Arco bajo compara las dos soluciones hacia el mismoP; no impone ángulo pequeño absoluto, llegada descendente al frontis ni retorno al fondo. |
| Propuesta§4.4/C4 y r2 | Ajuste factible visible después de soltar, con indicación clara siP cambia; mira fijada representa lo usado. | No autoriza reinterpretar silenciosamente el punto como dirección/bote. En estos 24 casos no hubo SpeedRaised ni TargetLowered. |
| D3/D4 depropuesta/plan, M2-4 aceptado | Apoyo directo fijaAim; tirón descendente posterior cargaP, objetivo inmutable. Control2 no altera contrato físico del solver. | No introduce intención táctica previa, volea ni corrección automática por estar cerca del frontis. |
| [Protocolo 2.7](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md)§4/§5 | C=(3,2;2,2)m, referencia verbal sinmarcador, también paraSL5/6; recetascongeladas. | No mide voluntad real de profundidad ni permite bajarC/retocarSL duranteesta campaña. |

[ShotSolver](../../Assets/SquashClash/Core/Shot/ShotSolver.cs) verifica que el **primer impacto** sea frontis y quede a≤5cm deTargetUsed (tolerancia existente). No condiciona la verificación a la trayectoria posterior. AX-1: LeftWall enx6,40,normal−X; RightWall enx0,normal+X. No se reflejan nombres para ajustar el relato.

Con contacto a~1m del frontis y objetivo más alto, la recta contacto→P ya es empinada. La raíz baja añade gravedad y puede llegar al frontis subiendo. **Interpretación geométrica del contrato**, no juicio de que la experiencia buscada por elPO esté satisfecha. Se preserva el literal de tanda1: no fue donde quería y esperaba profundidad/fondo. No se reclasifica esa respuesta como positiva porqueAim y primerfrontis coincidan.

## Selección y ejecución

Cuarenta recepciones 10–13, fronteras 2720/3192/3450/3917 del proceso 20261101T204549929, reconstruidas de JSONL primario y cotejadas con extracciones anteriores solo como comparación. **24Applied (2/10/7/5)** aportan centro/velocidad;16recepciones sin contacto aplicado quedan enJSON sin inventar trayectoria. Extras fuera de cuota excluidos. Versiones finales por AttemptId/LaunchId conservan linaje;H/pending correctos. La frase histórica del revisor **33contactos** no es denominador de este método: la tabla de cuotas10–13 suma24. No se reetiquetan sus mediciones ni se usa su resumen para seleccionar estados.

Se ejecutó **AnalyticBallSimulation original**, con13fuentes Core originales y UnityEngine.CoreModule de Unity 6000.3.27f1, compilador/Mono ya instalados. Primer intento decompilar arnés faltó referencia a netstandard; se añadió la fachada existente de Unity,sin cambiar P2. No hubo Editor/tests/producto instalado. Entrada float32 roundtrip,120 Hz/materiales/defaults P0 aprobados; sin variar C, P, velocidad o estado. Horizonte 10 s de simulación o paso del segundo bote completo. **Dos ejecuciones finales idénticas byte a byte**: control de reproducción offline, **no igualdad bitabit conIL2CPP/ARM64**.

El arnés inicial etiquetaba out de punto fuera de extensión longitudinal de pared; se preservó como preliminar no publicada y se corrigió para cumplir endpoints ya predeclarados. P2/entradas/eventos físicos no cambiaron. No se retocó umbral para mejorar salida.

## Resultados por bloque

Out es **geometría de impacto en pared finita** según líneas originales/huella 10 mm, no árbitro implementado ni Fail nuevo. P2 limita paredes por altura del centro 6 m y suelo no acotado: superar envolvente o caer fuera del rectángulo es limitación/escape geométrico, no bote legal. Columnas superpuestas: **no se suman**.

| Bloque | Contactos | Primer bote registrado | Lateral antes de bote | Out pared antes de bote | Supera envolvente | Bote fuera del rectángulo |
|---|---:|---:|---:|---:|---:|---:|
| 10 | 2 | 2 | 2 | 1 | 0 | 0 |
| 11 | 10 | 10 | 7 | 5 | 4 | 3 |
| 12 | 7 | 7 | 3 | 2 | 7 | 4 |
| 13 | 5 | 5 | 1 | 1 | 5 | 4 |
| Total | 24 | 24 | 13 | 9 | 16 | 11 |

En 24/24 el primer impacto es frontis; error máximo a TargetUsed **0,000005243m** (~0,0053mm), descriptivo offline. No es error humano ni precisión respecto de intención. Impact-cap: 24/24 cero. Todos alcanzaron segundo bote antes del horizonte, pero 11 primeros botes son fuera y parte delrecorrido es continuación contrafactual después de out/escape. Doscasos producen contactos de un plano fuera del ancho real de pared; no se etiquetan como out de pared finita. El JSON mantiene esos eventos/márgenes.

## Lectura conjunta descriptiva — corrección de revisión Claude

Calculada **solo desde el JSON derivado existente**, sin volver a ejecutar la simulación. La unión de las tres condiciones —out en pared antes del primer bote, envolvente superada o primer bote fuera del rectángulo— contiene **21/24 contactos**. Las condiciones se solapan; no se suman sus conteos ni se crea un criterio de aceptación.

Los **3 restantes** son los siguientes:

| Caso | Primer bote x (m) | Primer bote z desde el frontis (m) | z redondeada a una décima |
|---|---:|---:|---:|
| 10-02 | 4,660476 | 1,96420944 | 2,0 |
| 11-07 | 6,24676466 | 3,08716846 | 3,1 |
| 11-08 | 5,102349 | 4,001148 | 4,0 |

Los tres botan en la **mitad delantera** de la cancha de 9,75 m: z < 4,875 m. Ninguno de esos tres retornos sin las condiciones anteriores llega a la mitad trasera o al fondo. Los otros21 no se presentan como retornos jugables al fondo: sus botes o continuaciones posteriores al out/escape conservan las limitaciones ya declaradas.

**Precisión de la formulación recibida:** «≤4,0 m» solo describe aquí la cifra redondeada. El caso11-08 es **4,001148 m**, aproximadamente1,148 mm por encima de4,0 m; por tanto, no se afirma esa desigualdad literal para los tres. El alcance descriptivo de la observación es que los tres quedan alrededor de4m o menos y antes de4,875m, sin mover valores, bandas, criterios ni la frontera geométrica de media cancha.

La interpretación se calcula como OR sobre firstWallOut.timeSim < firstFloor.timeSim, exceededGeometryEnvelope y firstFloorOutsideCourt. La lectura de envolvente conserva el horizonte del derivado original; no se transforma retroactivamente en una medida exclusiva anterior al primer bote.

## Detalle de los24contactos

Tiempo desde contacto en segundos de simulación; equivalente real descriptivo t/0,65, sin medición nativa. Bote=(x;z)m. Fuera mantienecálculo delsuelo no acotado y **no es bote legal**. Out indica primer contacto out en pared finita. **Se sigue calculando tras out** para localizar botes; no significa que el runtime mostrara todo hasta el relanzamiento.

| Caso / LaunchId | Elevación | Primer bote: t; (x;z) | Laterales antes de bote | Primer out: t/superficie | Envolvente superada | Bote fuera |
|---|---:|---|---:|---|---|---|
| 10-02 / 474 | 50,02° | 1,534; (4,660;1,964) | 1 | No observado | No | No |
| 10-09 / 481 | 58,69° | 1,930; (4,686;3,581) | 1 | 1,044/LeftWall | No | No |
| 11-01 / 538 | 56,24° | 1,920; (4,781;4,561) | 1 | 1,065/LeftWall | No | No |
| 11-02 / 539 | 58,69° | 1,974; (4,408;3,641) | 1 | 0,993/LeftWall | No | No |
| 11-03 / 540 | 62,53° | 2,286; (10,460;5,889) | 0 | No observado | Sí | Sí |
| 11-04 / 541 | 63,69° | 2,170; (4,412;3,342) | 1 | 1,094/LeftWall | No | No |
| 11-05 / 542 | 67,75° | 2,267; (4,963;3,505) | 1 | 1,333/LeftWall | Sí | No |
| 11-06 / 543 | 60,80° | 1,934; (5,311;3,862) | 1 | 1,259/LeftWall | No | No |
| 11-07 / 544 | 55,01° | 1,458; (6,247;3,087) | 1 | No observado | No | No |
| 11-08 / 545 | 49,18° | 1,588; (5,102;4,001) | 1 | No observado | No | No |
| 11-09 / 546 | 55,88° | 2,346; (13,156;8,822) | 0 | No observado | Sí | Sí |
| 11-10 / 547 | 53,85° | 2,404; (13,306;9,249) | 0 | No observado | Sí | Sí |
| 12-02 / 573 | 67,48° | 2,398; (0,174;6,000) | 1 | No observado | Sí | No |
| 12-03 / 574 | 62,94° | 2,256; (1,339;5,527) | 1 | 1,366/RightWall | Sí | No |
| 12-04 / 575 | 60,84° | 2,215; (-4,087;6,070) | 0 | No observado | Sí | Sí |
| 12-05 / 576 | 63,48° | 2,219; (-3,870;5,053) | 0 | No observado | Sí | Sí |
| 12-07 / 578 | 59,67° | 2,302; (-6,188;6,406) | 0 | No observado | Sí | Sí |
| 12-08 / 579 | 61,01° | 2,504; (-8,710;6,226) | 0 | No observado | Sí | Sí |
| 12-09 / 580 | 51,45° | 2,402; (5,981;8,928) | 2 | 0,523/RightWall | Sí | No |
| 13-02 / 623 | 64,64° | 2,362; (2,246;4,271) | 1 | 0,580/LeftWall | Sí | No |
| 13-04 / 625 | 60,44° | 2,595; (-10,820;6,189) | 0 | No observado | Sí | Sí |
| 13-06 / 627 | 60,28° | 2,774; (-11,028;8,810) | 0 | No observado | Sí | Sí |
| 13-07 / 628 | 59,35° | 2,678; (-9,795;9,037) | 0 | No observado | Sí | Sí |
| 13-10 / 631 | 60,47° | 3,021; (-11,729;8,810) | 0 | No observado | Sí | Sí |

La observación del PO sobre lateral/elevación es **compatible** con el modelo actual y estos estados. En 13/24 hay lateral antes del primer bote; 9/24 out geométrico en pared finita antes de bote;otros atraviesan regiones no acotadas por la pared planar al superar 6 m. Esto localiza diferencia entre contrato de impacto en frontis y expectativa de retorno; **no adjudica causa perceptiva completa**, no demuestra recorrido realmente visto ni clasifica un bug cerrado. Captura real/contraste humano requerirían autorización propia.

## Opciones registradas antes de la decisión posterior del PO

| Opción posible | Ventaja | Riesgo/coste | Autorización previa necesaria |
|---|---|---|---|
| Mantener contrato y explicar punto del frontis | Conserva continuidad física y significado de Aim. | Puede seguir sin satisfacer profundidad/legibilidad buscada aunque solvercumpla. | JuicioPO; eventualtexto/feedback en PRpropia. |
| Probar referencia más baja en SL5/SL6, campaña aparte | Separar geometría decontacto deintención profunda. | SustituirC durante2.7 sesgaría comparación; no demuestra control bueno. | Protocolo/tareapredeclaradosPO, sin repetir 10–13por inferencia. No se simularon candidatos. |
| Mostrar predicción de salida/bote/riesgo, mismo solver | Hace visible elresultado posible. | Oclusión/carga/coste; puede ocultar ambigüedad envezderesolverla. | Diseño/validación/TDDpropios; ayudasnuevas/halo/lupa/zoom noautorizadosS2. |
| Redefinir mira como dirección/profundidad/bote | Alinearía entrada con otra intención táctica. | Cambia§6.4/§7.4,aprendizaje/solver/mediciones; no arreglo local. | DecisiónPO/adenda revisada, sinmodificarspecahora. |
| Agregar restricciones posteriores al solver (ángulo/descenso/bote) | Explicita una garantía hoy ausente. | Puede volver incompatiblesAim,P ytrayectoria; implicaría cambiarobjetivo/velocidad/fallar oasistenciaoculta. | Contrato/precedencias/criteriosPO yTDDenotraPR; sinvalor elegido. |
| Ampliar representación del límite físico | Evita interpretar planos fuera decancha como retorno normal. | Alcance mayor/nuevosestados; no resuelve significado de Aim. | Diseño/autorizaciónpropia; no reabrirgateP2por inferencia. |

**Histórico de la entrega ef7edb3:** entonces no había una opción seleccionada. La decisión posterior mantiene la mira en el frontis y pide mapa antes de elegir una regla; no hay tuning ni objetivo bajo promovido. S3calidad/feedback no debe corregirSL5/SL6silenciosamente ni introducir desvíolateral. [PlanS3documental](2026-11-04-first-shot-s3-documentary-plan.md) conserva la frontera.

## Integridad y entrega

JSON derivado: 40 recepciones, 24 estados, eventos, hashes/configuración/limitaciones. Sin identificadores personalesdel iPhone,logs primarios versionados ni claves. Outputs nuevos soloenLogs/offline-sl56-20261104delworktree diagnóstico,ignorados. Producto/tests/defaults/SL/spec/plan originales intactos. PR documental Claude,sin fusionar;PR #30sin fusionar/S3–S4/T8sinimplementación. Sesión física en pausa;16 no preparado/restauración tanda 2 pendiente.

**Entrega:** [PR #31 para Claude/PO](https://github.com/rafaallendes-boop/squash-clash/pull/31), apilada sobre PR #30; ambas abiertas y sin fusionar. Base documental actualizada sin cambios de producto, conservando los commits de método anteriores al cálculo.

## Decisión posterior del PO — 2026-11-04

El PO mantiene la mira como punto del frontis y autoriza investigar una regla de golpe jugable en el solver, con ajuste visible como C4, **sin elegir aún la regla**. Literal completo y alcance del mapa en [aprobaciones](2026-10-25-first-shot-po-approvals.md). Las opciones anteriores se conservan como historial, no como alternativas todavía abiertas para redefinir la mira. El nuevo [método de mapa offline](../diagnostics/2026-11-04-sl56-feasibility-method.md) se confirma en Git antes de calcular; ninguna opción de producto ni implementación queda autorizada.
