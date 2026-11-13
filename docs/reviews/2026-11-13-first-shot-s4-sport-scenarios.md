# First Shot — T4.4, escenarios deportivos del Anexo C

## Encargo y aprobación literal

Aprobación del PO del 12 de noviembre de 2026, recibida en el encargo adjunto y registrada en el primer commit de esta entrega:

> Apruebo el Anexo C para 4.4, con las siguientes consideraciones:
> * X1: aceptamos los 0,5 m como umbral inicial, sujeto a calibración. Buscamos un paralelo realmente pegado a la lateral.
> * X2: validamos la capacidad de generar ese cruzado ancho, pero no lo consideremos la única trayectoria válida de un buen cruzado.
> * R1 y R2: deben cumplirse como tendencias dentro de condiciones comparables y rangos válidos, no como leyes absolutas.
> * R4: queda pendiente hasta implementar la capa causal E3.
> * X6, X7, R5 y R8: fuera del alcance actual, sin descartarlos para futuras etapas.
> * R6: diferido hasta contar con la capa aleatoria.
> * C1–C5: informativas, sin bloquear el avance.
> Mantengamos los escenarios reproducibles y sus resultados documentados. Si alguno falla, presenta el diagnóstico, la causa probable y las alternativas de ajuste antes de modificar física o calibración.
> Importante: estas pruebas buscan validar técnicamente lo implementado. La evaluación de si los golpes se sienten naturales y realmente representan squash la haremos en iPhone, una vez cerrados e integrados First Shot y First Rally.
> Puedes avanzar con 4.4 bajo estos criterios, sin ampliar el alcance ni introducir mecánicas nuevas.

Base: main `8e53b41`. Solo T4.4; experimento técnico, sin aceptación de First Shot. Sin producto, física, solver, calibración, presets, spec, planes, criterios ni umbrales nuevos. Sin iPhone, builds ni instalación. PR independiente para Claude coordinada por el PO, sin fusionar. Estado al registrar el primer commit: método y resultados pendientes; el cierre de esta entrega está documentado más abajo.

## Método predeclarado (antes de ejecutar Unity)

Método vigente: el siguiente texto más las dos correcciones de arnés registradas antes de las ejecuciones definitivas. Para R1/R2, banda media existente 1,6≤y<2,6; R3 enumera las tres alturas bajas almacenadas, 42 pares.

Experimento explícito en `Tests/Experiments/EditMode`, ensamblado separado de `scripts/test.sh` y AutoQA. No cambia el plan para seguir su ruta histórica dentro de EditMode/Shot. Dos invocaciones independientes del mismo head limpio; salida determinista sin fechas/rutas, fuente y hashes incluidos. No aleatoriedad ni semillas; energía fija (sin modelo de cansancio), ayuda completa (aidLevel=0), λ=1, física/solver P2 del preset base `c0e9f00176d5` (108 claves), SimulationKey `166a96e319c7`; P0 derivado repeat=3 `42d9efbf50fc`. SHA-256 del archivo base y bounceThreshold de Unity se registran. No cambiar parámetros durante el experimento.

**Contactos sintetizados, no mediciones de recepciones SL.** Se fija T*=1 s de simulación, Δ=0, y se usa exactamente P* como centro del golpe asistido. Estado entrante común v=(0;−1;3) m/s, después de un primer bote y frontis previos asumidos por construcción del estado: no se afirma haber reconstruido esa recepción ni ejecutar el contrato/gesto extremo a extremo. La grilla ejercita solver/P2 desde estos estados, como permite el encargo. Altura cómoda/media y=0,75 m, punto medio del rango preferido 0,4–1,1 del cuerpo; dentro de legal.minHeight/maxHeight. Cuerpo estático en la misma z, separado lateralmente idealLateral=0,6 m hacia el centro, alcance/rango del preset. Se comprueban altura, esfera de alcance y holgura de pared de contacto y cuerpo; no volea ni estiramiento. Seis centros: B0=(0,35;0,75;7,5), B1=(2,6;0,75;7,5), B2=(3,8;0,75;7,5), B3=(6,05;0,75;7,5); F1=(2,6;0,75;1,5), F2=(3,8;0,75;1,5). B están detrás de short line y en ≥6 m (cuarto trasero de su mitad); F están en ≤2 m. Contactos a 0,35 m de lateral superan body.wallClearance=0,3. X5 informa por separado los seis, sin afirmar cobertura de todo el continuo de contactos legales.

**Grilla finita, orden fijo.** x de mira = {0,02;0,10;0,20;0,35;0,60;1,20;1,60;2,00;2,60;3,20;3,80;4,40;4,80;5,20;5,80;6,05;6,20;6,30;6,38}. y = tinTop+{0,05;0,10;0,15} y 13 nodos uniformes de tinTop+radio a out−radio (extremos incluidos, duplicados exactos eliminados). P=k/20, k=0…20. Todas son miras legales solicitables por M2-4/P∈[0;1]; no se exige mira por defecto. Bordes que la huella rechace quedan registrados. Banda media: tercio central del intervalo tinTop…FrontOutLower, partición provisional de observación conforme D2, no aceptación nueva. Submuestra de drop de R3: tres márgenes bajos anteriores, P≤1/3, mismo x objetivo que el contacto; se usa el mismo objetivo y P delante/detrás. Esta selección operacional del drop no define una intención nueva de producto.

**Vuelo y legalidad.** ShotSolver.Solve original con vMin/vMax/vCap del preset; guardar ajuste de velocidad/objetivo y razón. Simular salida con AnalyticBallSimulationFactory real, hasta segundo bote no persistente, fallo tin/out/NotUp, asentamiento persistente tras un bote o 10 s de simulación. Contactos ordenados en el paso tal como los emite P2. ReturnLineJudge y PointRules con `new RallyParams().FootprintRadius` (0,02 m): la misma huella que las reglas, sin implementar clasificador alternativo. Primer/segundo bote = primer/segundo evento Floor no persistente tras frontis; asentamiento se registra como tal, no se inventa un segundo bote. Horizonte, sin segundo bote o no resuelto ≠ éxito. Fuera después del primer bote conserva fallo. Al segundo bote se permite terminar normalmente por DoubleBounce. Registrar todos los eventos de testigos y las métricas por cada tiro de la grilla.

**Capa 1.** X1, por cada B: primer bote z>Length−ShortLineNearBackEdgeFromBack, retorno legal hasta segundo bote; último tramo = desde primer bote hasta segundo bote, incluidos estados y contactos intermedios. Distancia del centro a lateral propia (x=0 si contacto.x<Width/2, x=Width en otro caso) **máxima <0,5 m** durante todo ese tramo, e informar también mínima. X2, por B: contacto con lateral contraria estrictamente después del primer bote y antes del segundo; retorno legal. No es definición exhaustiva de cruzado. X3 por F: segundo bote z<Length/3, retorno legal. X4 por B: llegada válida al frontis sobre tin; testigo preferido con objetivo más bajo y potencia menor (no se añade requisito de distancia del segundo bote). X5 por los seis: llegada válida al frontis. Se informa existencia por contacto y conjunto; no existe significa **no encontrado en esta grilla**, sin demostrar imposibilidad continua. Selección de testigo determinista: X1 menor distancia máxima; X2 primer tiro que cumple; X3 menor z del segundo bote; X4/X5 primer tiro válido en orden y,P,x. No se corta la búsqueda al hallar testigo.

**Capa 2.** Guardar cada par con geometría, ajustes y motivo de no evaluación. R1: todos los pares de potencias adyacentes para contacto/mira iguales en banda media; excluir ajuste de velocidad u objetivo, retorno ilegal, pared lateral antes del primer bote, y pared posterior en el tiro menor. La pared frontal necesaria para devolver no cuenta como pared intermedia excluyente. Si el tiro mayor llega a trasera antes de su primer bote y el menor no, cumple por la excepción de C.2 sin comparar distancia; en otro caso solo comparar z de primeros botes sin paredes intermedias. R2: pares de alturas adyacentes de la grilla, mismo contacto/x/P, ambos en banda media, sin ajuste ni retorno ilegal, con primer bote observado; no impone la exclusión lateral de R1 porque C.2 no la exige. Contactos cómodos, no muy bajos. R1/R2: fracción estrictamente creciente, empates y contraejemplos; no fijar porcentaje de Pass ni llamar Fail a cada contraejemplo. R3: F1/B1 y F2/B2, submuestra de drop anterior; ambos retornos legales con segundo bote y objetivo efectivo idéntico, sin volea/contacto pegado. Informar cuántos z2(B)>z2(F), igualdades y contraejemplos; una contradicción dentro de validez queda para diagnóstico/PO. Todo caso fuera de las condiciones se cuenta NotEvaluated, con motivo.

R7 usa las seis recetas SL originales y ContactPlanner real sobre su trayectoria P2 hasta segundo bote. Comparar tempo 0,5 con 1, λ=1, slowMotion=1, sin alterar física ni el preset persistente; ClockMap convierte KIn/KOut fraccionales a segundos reales. La trayectoria y el cuerpo son los mismos objetos/datos, por lo que la geometría es idéntica y la velocidad real se duplica. Registrar ambas duraciones y T*. Falta de plan mapeable → NotEvaluated. No multiplicar velocidades iniciales manteniendo gravedad, pues cambiaría la forma y sería inválido para R7.

**Capa 3 no bloqueante.** C1–C4 son métricas de los testigos X1–X4: botes xyz, distancia a trasera, distancia min/max a lateral, lateral cruzada y altura/z de contacto, margen de frontis sobre tin, segundo bote delante/atrás. C1 «cerca de trasera» y C2 «a altura del receptor» no tienen tolerancia numérica aprobada: mostrar distancia/altura, sin inventar Pass. C3 muestra margen 0,05–0,15 y z2<2; C4 margen comparado con C3 del mismo lado y z2<3. No bloquean aunque queden fuera de banda. R4/C5: diferidos por E3; X6/X7/R5/R8 fuera del alcance actual, no descartados; R6 diferido por capa aleatoria. Capa 4 pendiente de playtest integrado, no evaluada aquí.

**Verificación y parada.** Completar la toma predeclarada, sin adaptar grilla a resultados. Si falta capacidad o falla relación válida: diagnóstico, causa probable y alternativas con riesgos, sin corrección física/calibración; detener avance de producto para PO. Dos archivos por corrida (`trials.tsv`, `pairs.tsv`) y `summary.json`/`witnesses.json` byte a byte idénticos; comprobar SHA-256 externamente. Regresión completa sin añadir casos a sus ensamblados; AutoQA no aplica si solo cambia arnés experimental/docs. Un solo Unity a la vez, comprobado antes de cada invocación. XML fresco, sin CS/zero tests, ProjectSettings restaurados y árbol limpio. Sin check-release/builds/exports, pues el encargo no los solicita. No actualizar escenarios, defaults ni anclajes para obtener resultados favorables.


### Corrección metodológica antes de interpretar resultados

La revisión del arnés detectó que el tercio de tin…out propuesto arriba no coincide con la banda media ya implementada en T4.2 (`PracticeTask.FrontRow`: 1,6≤y<2,6 m). Se reutiliza esa banda existente para R1/R2, sin introducir otra partición. La primera invocación de `7025fd1` ya había terminado al detectar esto, pero **no se habían leído ni interpretado sus datos deportivos**. Se preserva íntegra como precomprobación técnica no definitiva en `Logs/experiments/sport/precheck-7025fd1/run01` y su XML/log. No se declara como corrida del método corregido ni se oculta. Antes de nuevas corridas se compromete esta corrección; las dos definitivas usarán el mismo head corregido y ninguna otra definición cambia. No se modifican física, solver, producto, grilla, contacto, parámetros ni expectativas.


### Auditoría de enumeración R3, sin cambio físico

Las dos tomas de `88391dd` resultaron idénticas byte a byte. Mostraron capacidad X1–X5, R2 creciente en 3173/3463 pares válidos (290 contraejemplos), y R3 contradictoria en sus 14 pares enumerados. Se conserva ese fallo; no se ajusta producto. La auditoría cruzada con la grilla completa detectó que la comparación entre y almacenada como float y expresiones de suma sin almacenar omitió los nodos tin+0,05 y tin+0,10: solo enumeró tin+0,15. Se corrige **solo el arnés** almacenando una vez los tres nodos en un HashSet<float>, y se compara el objetivo efectivo por componentes exactos, sin el operador aproximado de Vector2. Así se enumeran los **42 pares predeclarados** (2 lados×3 alturas×7 potencias). No se cambia la grilla, condiciones, expectativas ni se oculta R3. Ambas tomas originales/XML/logs se preservan en `precheck-88391dd`; dos nuevas tomas completas del mismo head corregido verificarán el arnés exhaustivo. Si R3 vuelve a contradecirse, se entrega así para el PO. Se trata de completar correctamente el experimento autorizado, sin corregir física/solver/calibración ni iniciar otra tarea.


## Resultados deportivos de las dos ejecuciones definitivas

Fuente limpia `fd81c2dba3500dbf7b6bd7677f853009203dd4b9`. Cada corrida: **38304 tiros** (6 contactos ×19 miras X ×16 alturas ×21 potencias); 36264 resueltos, 2040 no resueltos; 28711 retornos legales hasta segundo bote. Cero horizontes incompletos, cero topes P2 y cero instancias vivas al finalizar. No resuelto no cuenta como éxito. Terminales: Counter({'DoubleBounce': 28711, 'Out': 5636, 'Unsolved': 2040, 'Tin': 1917}).

Los estados de contacto son sintéticos. La condición temporal asistida está fijada por construcción (P* en T*, Δ=0), no medida por gesto/contrato ni por una recepción completa; R7 sí usa las recepciones SL y el planificador reales. Estos resultados tienen ese alcance técnico.


### X1

| Contacto | Resultado | Combinaciones encontradas /6384 | ID | Mira (x,y) m | P | Primer bote xyz m | Segundo bote xyz m |
|---|---|---:|---:|---|---:|---|---|
| B0 | Existe | 442 | 5530 | (0.100000, 3.875000) | 0.900000 | (0.020000, 0.000000, 7.002854) | (0.020000, 0.000000, 4.527035) |
| B1 | Existe | 237 | 9959 | (0.350000, 2.187500) | 1.000000 | (0.027605, 0.000000, 9.441507) | (0.043139, 0.000000, 7.999805) |
| B2 | Existe | 237 | 16355 | (6.050000, 2.187500) | 1.000000 | (6.372385, 0.000000, 9.441508) | (6.356851, 0.000000, 7.999803) |
| B3 | Existe | 351 | 24698 | (6.300000, 3.875000) | 0.900000 | (6.380000, 0.000000, 7.002855) | (6.380000, 0.000000, 4.527064) |

| Contacto | Mínima del testigo m | Máxima del testigo m | Mínima alcanzada en la búsqueda de retornos profundos m |
|---|---:|---:|---:|
| B0 | 0.020000 | 0.020000 | 0.020000 |
| B1 | 0.027605 | 0.043139 | 0.020000 |
| B2 | 0.027615 | 0.043149 | 0.020000 |
| B3 | 0.020000 | 0.020000 | 0.020000 |

### X2

| Contacto | Resultado | Combinaciones encontradas /6384 | ID | Mira (x,y) m | P | Primer bote xyz m | Segundo bote xyz m |
|---|---|---:|---:|---|---:|---|---|
| B0 | Existe | 263 | 14 | (5.800000, 0.500000) | 0.000000 | (6.056346, 0.000000, 0.265474) | (6.192985, 0.000000, 0.795854) |
| B1 | Existe | 74 | 6399 | (6.050000, 0.500000) | 0.000000 | (6.228946, 0.000000, 0.305716) | (6.274770, 0.000000, 0.795850) |
| B2 | Existe | 74 | 12771 | (0.350000, 0.500000) | 0.000000 | (0.171037, 0.000000, 0.305734) | (0.125238, 0.000000, 0.795895) |
| B3 | Existe | 263 | 19155 | (0.350000, 0.500000) | 0.000000 | (0.085547, 0.000000, 0.260668) | (0.334140, 0.000000, 0.763184) |

Los testigos cruzan la lateral contraria después del primer bote. Sus segundos botes son delanteros: satisfacen X2 literal, pero no la banda C2. Esto no define la única trayectoria de un buen cruzado.

### X3

| Contacto | Resultado | Combinaciones encontradas /6384 | ID | Mira (x,y) m | P | Primer bote xyz m | Segundo bote xyz m |
|---|---|---:|---:|---|---:|---|---|
| F1 | Existe | 2933 | 28498 | (6.300000, 1.850000) | 0.400000 | (2.572893, 0.000000, 0.040042) | (0.590934, 0.000000, 0.040042) |
| F2 | Existe | 3034 | 35150 | (0.020000, 2.187500) | 0.100000 | (1.866398, 0.000000, 0.020002) | (1.866398, 0.000000, 0.020002) |

Los testigos seleccionados por mínimo z2 tienen objetivos altos y rebote lateral: satisfacen el segundo bote en tercio delantero, pero **no acreditan el drop natural** ni cumplen el margen bajo de C3. Los dos botes son eventos P2 distintos (F1 pasos 229/296; F2 279/351), aunque su z sea igual. No se cambia la selección predeclarada para hacer favorable la banda.

### X4

| Contacto | Resultado | Combinaciones encontradas /6384 | ID | Mira (x,y) m | P | Primer bote xyz m | Segundo bote xyz m |
|---|---|---:|---:|---|---:|---|---|
| B0 | Existe | 5387 | 2 | (0.200000, 0.500000) | 0.000000 | (0.191613, 0.000000, 0.348485) | (0.179032, 0.000000, 0.841314) |
| B1 | Existe | 5379 | 6384 | (0.020000, 0.500000) | 0.000000 | (0.139290, 0.000000, 0.365850) | (0.325901, 0.000000, 0.906876) |
| B2 | Existe | 5373 | 12769 | (0.100000, 0.500000) | 0.000000 | (0.086940, 0.000000, 0.298934) | (0.225581, 0.000000, 0.631711) |
| B3 | Existe | 5398 | 19152 | (0.020000, 0.500000) | 0.000000 | (0.269092, 0.000000, 0.328990) | (0.727043, 0.000000, 0.897062) |

X4/X5 verifican llegada válida al frontis. Las columnas de botes son datos adicionales; no se añaden como requisito de esas filas. Los testigos son tiros de potencia mínima con objetivo bajo.

### X5

| Contacto | Resultado | Combinaciones encontradas /6384 | ID | Mira (x,y) m | P | Primer bote xyz m | Segundo bote xyz m |
|---|---|---:|---:|---|---:|---|---|
| B0 | Existe | 5387 | 2 | (0.200000, 0.500000) | 0.000000 | (0.191613, 0.000000, 0.348485) | (0.179032, 0.000000, 0.841314) |
| B1 | Existe | 5379 | 6384 | (0.020000, 0.500000) | 0.000000 | (0.139290, 0.000000, 0.365850) | (0.325901, 0.000000, 0.906876) |
| B2 | Existe | 5373 | 12769 | (0.100000, 0.500000) | 0.000000 | (0.086940, 0.000000, 0.298934) | (0.225581, 0.000000, 0.631711) |
| B3 | Existe | 5398 | 19152 | (0.020000, 0.500000) | 0.000000 | (0.269092, 0.000000, 0.328990) | (0.727043, 0.000000, 0.897062) |
| F1 | Existe | 5413 | 25552 | (6.200000, 0.500000) | 0.000000 | (6.219448, 0.000000, 0.118200) | (6.053642, 0.000000, 0.173533) |
| F2 | Existe | 5411 | 31922 | (0.200000, 0.500000) | 0.000000 | (0.180551, 0.000000, 0.118200) | (0.346362, 0.000000, 0.173533) |

Existe al menos un retorno seguro en cada uno de los seis contactos muestreados; no demuestra la garantía sobre cualquier punto del continuo legal.

### R1

| Comparados | Válidos | Crecientes | Empates | Contraejemplos | No evaluados | Fracción creciente válida |
|---:|---:|---:|---:|---:|---:|---:|
| 36480 | 2110 | 2110 | 0 | 0 | 34370 | 100.000000 % |

| Motivo excluyente registrado (prioridad del arnés) | No evaluados |
|---|---:|
| aim-outside-middle-band | 29640 |
| lower-power-back-wall-before-first-bounce | 88 |
| not-a-legal-two-bounce-return | 92 |
| side-wall-before-first-bounce | 3640 |
| solver-adjustment | 550 |
| unsolved | 360 |

Tendencia observada en todos los pares válidos. Incluye 76 excepciones de pared trasera con la potencia mayor, clasificadas por categoría sin comparar su z de bote. Los excluidos no son Pass ni Fail.

### R2

| Comparados | Válidos | Crecientes | Empates | Contraejemplos | No evaluados | Fracción creciente válida |
|---:|---:|---:|---:|---:|---:|---:|
| 35910 | 3463 | 3173 | 0 | 290 | 32447 | 91.625758 % |

| Motivo excluyente registrado (prioridad del arnés) | No evaluados |
|---|---:|
| aim-outside-middle-band | 31122 |
| first-bounce-not-observed | 638 |
| solver-adjustment | 687 |

Tendencia creciente en 91,625758 %, con 290 contraejemplos (ninguno con ajuste del solver). No se inventa un porcentaje de aceptación. Los 290 están completos, con ambas geometrías, en `counterexamples.tsv`. Hay 123 con trasera antes del bote en alguno de los tiros; otros 113 sin trasera pero con lateral, y **54 sin ninguna de esas paredes**. R2 no excluye esos rebotes en C.2; no se eliminan para mejorar la fracción.

### R3

| Comparados | Válidos | Crecientes | Empates | Contraejemplos | No evaluados | Fracción creciente válida |
|---:|---:|---:|---:|---:|---:|---:|
| 42 | 42 | 0 | 0 | 42 | 0 | 0.000000 % |

**Fail de la relación en la muestra válida: 0/42**, dos lados ×tres márgenes ×siete potencias. Los 42 conservan el mismo objetivo efectivo y tienen segundo bote legal, altura cómoda y buena posición. En 18 pares el solver eleva velocidad del tiro trasero; en los otros 24 no: la contradicción no se explica solo por la asistencia de factibilidad. Δz2(atrás−adelante) va de −1,653623 a −0,070273 m. No se reetiquetan como fuera de validez. Detenido para decisión del PO, sin ajuste.

### R7

| Receta | K en pasos (entrada;salida) | Duración a τ=0,5 ms | Duración a τ=1 ms | Resultado |
|---|---|---:|---:|---|
| SL1 | (137.084789; 158.474710) | 356.498677 | 178.249338 | Menor (misma geometría) |
| SL2 | (137.084789; 158.474710) | 356.498677 | 178.249338 | Menor (misma geometría) |
| SL3 | (147.035594; 164.923561) | 298.132794 | 149.066397 | Menor (misma geometría) |
| SL4 | (147.035590; 164.923565) | 298.132911 | 149.066455 | Menor (misma geometría) |
| SL5 | (94.316734; 132.178893) | 631.035997 | 315.517998 | Menor (misma geometría) |
| SL6 | (94.316734; 132.178893) | 631.035997 | 315.517998 | Menor (misma geometría) |

Seis pares válidos; ninguno no evaluado. K dura la mitad en tiempo real al duplicar tempo. No afirma esa relación para cambios de velocidad que cambian la forma.

## Bandas C1–C4, informativas

Métricas de los testigos fijados, sin elegir otros después de ver los resultados. Las bandas no bloquean ni sustituyen el juicio del PO.

| Banda/contacto | ID | Margen frontis sobre tin m | z1 m | z2 m | Distancia z2 a trasera m | Lateral min–max m | Lateral contraria (altura;z) m | Observación |
|---|---:|---:|---:|---:|---:|---|---|---|
| C1/B0 | 5530 | 3.395000 | 7.002854 | 4.527035 | 5.222965 | 0.020000–0.020000 | No observada | Cercanía a trasera: dato sin umbral aprobado |
| C2/B0 | 14 | 0.020021 | 0.265474 | 0.795854 | 8.954146 | 6.056346–6.380000 | (0.423968; 0.575385) | z2 no está en cuarto trasero contrario |
| C4/B0 | 2 | 0.020015 | 0.348485 | 0.841314 | 8.908686 | 0.179032–0.191613 | No observada | z2<3; margen menor que C3, no mayor |
| C1/B1 | 9959 | 1.707500 | 9.441507 | 7.999805 | 1.750195 | 0.027605–0.043139 | No observada | Cercanía a trasera: dato sin umbral aprobado |
| C2/B1 | 6399 | 0.020003 | 0.305716 | 0.795850 | 8.954150 | 6.228946–6.380000 | (0.358419; 0.546869) | z2 no está en cuarto trasero contrario |
| C4/B1 | 6384 | 0.020016 | 0.365850 | 0.906876 | 8.843124 | 0.139290–0.325901 | No observada | z2<3; margen menor que C3, no mayor |
| C1/B2 | 16355 | 1.707500 | 9.441508 | 7.999803 | 1.750197 | 0.027615–0.043149 | No observada | Cercanía a trasera: dato sin umbral aprobado |
| C2/B2 | 12771 | 0.020011 | 0.305734 | 0.795895 | 8.954105 | 6.228963–6.380000 | (0.358395; 0.546871) | z2 no está en cuarto trasero contrario |
| C4/B2 | 12769 | 0.020013 | 0.298934 | 0.631711 | 9.118289 | 6.174419–6.313060 | No observada | z2<3; margen menor que C3, no mayor |
| C1/B3 | 24698 | 3.395000 | 7.002855 | 4.527064 | 5.222936 | 0.020000–0.020000 | No observada | Cercanía a trasera: dato sin umbral aprobado |
| C2/B3 | 19155 | 0.020013 | 0.260668 | 0.763184 | 8.986816 | 6.065860–6.380000 | (0.157642; 0.320316) | z2 no está en cuarto trasero contrario |
| C4/B3 | 19152 | 0.020023 | 0.328990 | 0.897062 | 8.852938 | 5.672957–6.130908 | No observada | z2<3; margen menor que C3, no mayor |
| C3/F1 | 28498 | 1.370000 | 0.040042 | 0.040042 | 9.709958 | 0.590934–2.572893 | No observada | Fuera del margen 0,05–0,15; z2<2 |
| C3/F2 | 35150 | 1.707502 | 0.020002 | 0.020002 | 9.729998 | 4.533603–4.533603 | No observada | Fuera del margen 0,05–0,15; z2<2 |

C2 muestra altura/profundidad del choque lateral; no se inventa altura ni tolerancia del receptor. C4 se compara con C3 F1 para B0/B1 y F2 para B2/B3. El mal encaje de algunos testigos con las bandas no prueba ausencia de otras combinaciones mejores en la grilla. C5/R4 diferidos por E3; X6/X7/R5/R8 fuera del alcance actual, no descartados; R6 diferido por capa aleatoria. Capa 4 no evaluada.

## Geometrías de contraejemplo y diagnóstico

Muestra ilustrativa; `counterexamples.tsv` contiene **todos los 332** contraejemplos con la fila completa de ambos tiros, sin redondeo. `pairs.tsv.gz` conserva además todos los pares crecientes y no evaluados.

| Relación | IDs A/B | Contacto A/B | Mira A/B (x;y) m | P A/B | Velocidad A/B m/s | z1 A/B m | z2 A/B m |
|---|---|---|---|---|---|---|---|
| R2 | 3553/3952 | B0/B0 | (0.020000;2.187500)/(0.020000;2.525000) | 0.950000/0.950000 | 26.900000/26.900000 | 9.561423/9.132709 | 9.144466/7.303523 |
| R2 | 28354/28753 | F1/F1 | (1.600000;1.850000)/(1.600000;2.187500) | 0.050000/0.050000 | 7.100000/7.100000 | 1.681939/1.580042 | 1.804816/1.580042 |
| R3 | 25943/6791 | F1/B1 | (2.600000;0.530000)/(2.600000;0.530000) | 0.000000/0.000000 | 6.000000/8.441111 | 1.003480/0.368886 | 1.560931/0.857742 |
| R3 | 26000/6848 | F1/B1 | (2.600000;0.530000)/(2.600000;0.530000) | 0.150000/0.150000 | 9.300000/9.300000 | 1.665938/0.814916 | 2.747953/1.831051 |

**Diagnóstico de R3 (S4-44-R3-01).** La relación falla en toda la submuestra predeclarada, también con P=0,15…0,30 sin ajustes. Por ejemplo, a P≈0,15, mira (2,6;0,53), z2 delante=2,747953 m y atrás=1,831051 m, velocidad total 9,3 m/s en ambos. Cambia solo la distancia del contacto al mismo objetivo. La física mantiene sus parámetros; no hay penalización causal ni azar. Esto es una discrepancia técnica observada, no una aceptación por tendencia ni un fallo de regresión.

**Causa probable, no demostración de un bug.** El solver conserva la velocidad total y escoge arco bajo. La distancia mayor cambia la inclinación inicial y la velocidad vertical con que se llega al frontis; la restitución normal y la fricción tangencial pueden dejar una salida más descendente/corta después de ese impacto. A potencia baja, además, la factibilidad eleva la velocidad trasera hacia un arco próximo al límite balístico. Los datos son compatibles con ese mecanismo: en P=0, delante v=(0;0,321021;−5,991406) y atrás v=(0;5,878435;−6,057752) m/s con el mismo objetivo bajo; el tiro trasero llega más descendente al frontis. La elevación de velocidad solo aparece en 18/42 pares: **no basta por sí sola para explicar los 42**. No se afirma fidelidad deportiva ni que deba corregirse el motor; tampoco se cambia la condición de R3 para excluirlos.

**R2, explicación parcial.** Los rebotes intermedios pueden plegar la profundidad y explicar parte de los 290 contraejemplos; hay 54 sin lateral ni trasera antes del bote, por lo que no se atribuyen todos a las paredes. A velocidad total fija, elevar objetivo también redistribuye componentes vertical y horizontal. Se conserva la fracción de tendencia observada y todas las geometrías; no se cambia R2 a una ley absoluta ni se etiqueta su 91,6 % como umbral aprobado.

Alternativas para que decida el PO **antes de modificar física/calibración**:

| Alternativa | Ventaja | Riesgo o coste |
|---|---|---|
| Investigar mediante trazas de velocidad de entrada/salida en impactos y análisis de los parámetros existentes, en entrega separada | Distingue solver, geometría y material antes de elegir una corrección; conserva esta línea base | Requiere autorización y revisión; no asegura que una única calibración satisfaga R3 y las otras capacidades |
| Evaluar calibración de velocidades/rango y restitución/fricción ya configurables, solo con autorización | No añade mecánicas; permite medir el compromiso con presets separados | Afecta drive/cruzado/drop y los gates de física; puede degradar capacidades que aquí existen. No se propone ni aplica ningún valor nuevo |
| Revisar con el PO la condición deportiva de R3 o su definición operativa de drop, y contrastarla en el playtest integrado | Permite decidir si la relación representa la intención deportiva deseada | Podría reducir lo que se exige al producto; no autoriza reescribir criterios para convertir este Fail en Pass. Exigiría decisión registrada y entrega separada |

No se elige ni ejecuta ninguna alternativa. **Se detiene el avance ante S4-44-R3-01 para decisión del PO (§17.F).** Esta entrega solo termina documentación, reproducibilidad y verificaciones de regresión ya autorizadas; no corrige el comportamiento ni abre otra tarea.

## Reproducibilidad y verificación

Los dos procesos definitivos ejecutaron el head limpio **fd81c2dba3500dbf7b6bd7677f853009203dd4b9**, después de guardar la aprobación, el método y sus correcciones. Corrida 1: **13 nov 2026, 07:52:23–07:52:25 America/Santiago**; corrida 2: **07:52:43–07:52:45**. Cada fixture ejecutó 1/1 y completó la toma; esto significa adquisición completa, **no Pass de R3 ni aceptación deportiva**. Dos ejecuciones independientes, sin simultaneidad Unity y sin usar datos de builds anteriores.

| Archivo | Bytes | SHA-256 (idéntico en las dos corridas) |
|---|---:|---|
| summary.json | 13508 | `731c91e196be2b9ef626ad2a43927fb66a0840f85191ffa1e05a8e4eba581a01` |
| witnesses.json | 68890 | `33834b9dee8a9b818fb0b28d3e2d815e2d4b9b5e4b57b578612c763a175f08c6` |
| trials.tsv | 13344347 | `36fb4b1ac133806dd02edee23f9edf7a211065804e0569a4617004d1cc4fbb2c` |
| pairs.tsv | 5841367 | `208440a787f21591cf54d857fbbd6685add2825ac743b9696e11c4dabde73149` |

[Paquete portátil y reproducción](../validation/evidence/2026-11-13-first-shot-s4-sport/README.md), [manifiesto SHA-256](../validation/evidence/2026-11-13-first-shot-s4-sport/manifest.json), [inventario de ejecuciones](../validation/evidence/2026-11-13-first-shot-s4-sport/execution-manifest.json), [todas las geometrías de contraejemplo](../validation/evidence/2026-11-13-first-shot-s4-sport/counterexamples.tsv). Los TSV se comprimen sin fecha, con hashes tanto comprimidos como originales; JSON/testigos sin recorte. El script derivador coteja los archivos completos, no solo sus agregados. Los tres prechecks (uno en 7025fd1 y dos en 88391dd) permanecen identificados como tales, con sus XML/logs y hashes; no se mezclan con los dos definitivos ni se esconden las correcciones del arnés.

Regresión en la misma fuente: **691/691 EditMode** (07:53:17–07:53:55 Chile), **450/450 PlayMode** (07:55:44–07:57:11 Chile); ambas sin fallos/skipped/CS, XML nuevos verificados por `scripts/test.sh`. Recuentos conservados respecto de la base de T4.2/#50, sin modificar sus ensamblados/casos ni incluir el experimento. No hay RED de comportamiento de producto: esta entrega solo agrega el experimento descriptivo fuera de regresión. La verificación del arnés está en su enumeración completa y el cotejo externo de datos/pares.

**AutoQA no aplica:** el delta de C# se limita al nuevo fixture explícito. No cambia timing, gesto ni contrato productivos; no se modifica su lector, anclajes, scripts o escenarios. No se ejecuta check-release, exports, builds ni instalación porque están fuera de este encargo.

ProjectSettings/GraphicsSettings.asset y QualitySettings.asset, modificados transitoriamente por PlayMode, se respaldaron como diff ignorado y restauraron byte a byte a la fuente. `scripts/device.env` se copió sin cambios, sigue ignorado y se cotejó sin exponer valores. Ningún otro archivo del producto difiere de main base, incluidos DefaultPreset.json, física, solver, contrato, gesto, spec, planes, ProjectSettings, scripts y Packages. SHA-256 del archivo DefaultPreset.json: **68a17d60090583fc87ff24a79c8e8141f2131ec7c53e8ca111ce5e8497981319**; defaults **c0e9f00176d5**, P0 derivado **42d9efbf50fc**, SimulationKey **166a96e319c7**. Se registran bounceThreshold=0,10000000149011612 m/s y huella float=0,019999999552965164 m, leídos de Unity/reglas, sin cambiar configuración.

## Estado de entrega

T4.4 implementada como arnés experimental y evaluada en su alcance predeclarado: X1–X5 con testigos; R1/R2 como tendencias; **R3 Fail / S4-44-R3-01**; R7 menor duración con misma geometría; C1–C4 informativas. Informe listo para revisión independiente de Claude coordinada por el PO en PR propia, **sin fusionar**. El cierre o cualquier ajuste de producto por R3 queda pendiente de decisión del PO (§17.F). Árbol final limpio después del commit documental, con el arnés probado conservado byte a byte.

No se acepta First Shot ni se inicia T4.5, capa causal, #32, First Rally/gameplay o visual/art-pass. Sin acceso al iPhone. Capa 4 y la evaluación de naturalidad se reservan al playtest integrado First Shot + First Rally. **Detenerse.**
