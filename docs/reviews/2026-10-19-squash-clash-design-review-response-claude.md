# Squash Clash: respuesta de Claude a la revisión de Codex (v0.1)

- **Fecha:** 2026-10-19
- **Autor:** Claude
- **Documentos de entrada:**
  - [Especificación v0.1](../superpowers/specs/2026-10-19-squash-clash-design.md). SHA-256 `c5ea6adb…a8ae9`, el mismo que revisó Codex. No se modificó.
  - [Revisión de Codex](2026-10-19-squash-clash-design-review-codex.md), con 26 hallazgos.
  - Encargo del PO para la segunda iteración, con sus decisiones confirmadas y sus prioridades.
- **Resultado:** [Especificación v0.2](../superpowers/specs/2026-10-19-squash-clash-design-v0.2.md).

Ningún punto de este documento se da por aprobado. Lo que figura como **Validación pendiente del PO** queda abierto hasta que el PO lo confirme.

## Resumen

| Decisión | Cantidad | Hallazgos |
|---|---|---|
| ACEPTAR | 15 | F02, F03, F05, F06, F07, F08, F09, F10, F11, F12, F17, F18, F21, F23, F24 |
| ACEPTAR PARCIALMENTE | 9 | F01, F04, F14, F15, F16, F19, F20, F22, F25 |
| DISCREPAR | 0 | En varios hallazgos discrepo de *partes* concretas. Esas partes están explicadas en la justificación de cada uno. |
| DIFERIR | 2 | F13 (a First Shot), F26 (cambio de nomenclatura ahora; el efecto en el alcance se difiere al milestone de golpes ampliados) |

Valoración general: la revisión de Codex es sólida y mejora la especificación. Sus aportes principales:
- el **contrato de contacto** (F03), que era el vacío más grave de la v0.1;
- el **acoplamiento entre potencia y puntería** (F04);
- la **justificación insuficiente para descartar PhysX** (F09);
- un error aritmético mío en la duración de los partidos (F25).

Discrepo, total o parcialmente, en cuatro puntos:
1. **F01:** el formato a 7 puntos *sí* tiene procedencia verificable, y el PO lo reconfirmó en este encargo. No lo trato como hipótesis.
2. **F15:** empezar sin azar es útil como línea base para diagnosticar. Pero el principio del PO no es "cero azar", sino "errores comprensibles", así que el azar acotado vuelve en First Fun.
3. **F19:** mantener los cuatro pilares completos en el esquema de datos no cuesta casi nada y fue una decisión expresa del PO. Solo se posterga el *código* que conecta los subatributos sin efecto.
4. **F20:** acepto que el C# puro no es necesario para llevar el juego a Android. Aun así, mantengo una frontera lógica liviana porque abarata las pruebas del contrato de contacto, que ahora es el núcleo del juego.

## Matriz de respuesta

| ID | Hallazgo | Decisión propuesta | Justificación | Cambio en la especificación | Validación pendiente del PO |
|---|---|---|---|---|---|
| F01 | Trazabilidad de aprobaciones | **ACEPTAR PARCIALMENTE** | El problema existe: la v0.1 etiquetaba como [APROBADO] decisiones sin indicar su procedencia, y mezclaba valores de partida con compromisos. Pero el formato a 7 tiene procedencia en el brainstorming ("Quizás no llegar a 11 pero si a 7", "sin tope, para que sea más realista") y el PO lo reconfirmó en el encargo v0.2. El 60–70 % fue una propuesta mía que el PO aceptó como *valor de partida*, no como requisito. La opción A se aprobó como "apuntar al frontis", no como mecanismo de potencia. | Nuevo **registro de decisiones** (§0.3) con fuente y cita de cada decisión. Nueva etiqueta **[EXPERIMENTO]** para los valores iniciales que se ajustan. 60–70 % y mecanismo de potencia pasan a [EXPERIMENTO]. 7 puntos sigue como [APROBADO]. | Revisar el registro §0.3 y corregir cualquier cita o interpretación. |
| F02 | El First Playable mezcla demasiadas preguntas | **ACEPTAR** | Coincide con la estructura que pidió el PO. Si el primer test de diversión no resulta, con todo integrado a la vez no se sabría qué sistema falló. | Roadmap First Ball → First Shot → First Rally → First Match → First Fun (§15), cada uno con su pregunta, lo que se prueba en el iPhone y su condición de salida. Cansancio y estilos se mueven a First Fun. First Rally usa energía fija. | Confirmar la asignación de sistemas a cada milestone (§14). |
| F03 | Falta un contrato único de contacto y timing | **ACEPTAR** | Era el vacío más grave. La v0.1 no definía cuándo ocurre el contacto. Además se contradecía: §17.2.8 decía "nunca fallar con buena posición" y §7.5 decía "no soltar = no golpe". | Nuevo **§7.1 Contrato de contacto y timing**, con modelo de contacto asistido, ventana limitada, estados de orden y tres categorías de fallo distintas: sin orden, inalcanzable y fallo de ejecución. Incluye 12 casos verificables. Se elimina la contradicción. | **Validación principal de esta iteración:** aprobar el modelo de contacto asistido como hipótesis para First Shot. Los valores de las ventanas se prueban en el iPhone. |
| F04 | El flick final acopla potencia y puntería | **ACEPTAR PARCIALMENTE** | El acoplamiento es real. Se puede mitigar dentro del gesto original congelando la mira al detectar el flick (variante A′). No descarto la A sin probarla: es la opción más rápida y la que pide una sola mano. | Tres candidatos en §6.2: **A′** (original con congelación de mira), **B** (apuntar, fijar y luego tensar) y **C** (potencia contextual asistida). Se suma un protocolo de comparación en First Shot (§6.3) con métricas de puntería deseada vs. final y de potencia involuntaria. | Autorizar el experimento comparativo. El PO elige el ganador con los datos y su sensación. |
| F05 | Prioridad indefinida entre toque y gesto | **ACEPTAR** | "Mientras el rival tiene la pelota" no es un estado bien definido. Y un toque que cambia de propósito a mitad de camino causa acciones que el jugador no quiso. | **Máquina de estados de entrada** (§6.5): el propósito de cada toque se fija al apoyar el dedo, hay una forma explícita de cancelar, cuenta un solo dedo, se cancela al perder el foco, los umbrales se miden en milímetros y se respetan los bordes del sistema. La lateralidad de la interfaz queda separada de la mano dominante del personaje. | Validar que el reposicionamiento solo se pueda hacer en la fase de Recuperación. |
| F06 | La volea necesita una prueba táctica temprana | **ACEPTAR** | Coincide con la Prioridad 4 del PO. Sin volea no se puede evaluar el dominio de la T. | La volea se define como **condición de contacto** antes del bote, con el mismo gesto (§7.3). Hay un experimento con interruptor sí/no en First Rally y dos variantes: el planificador decide, o el jugador decide con el timing. | Autorizar el experimento. Validar si la volea aporta profundidad táctica. |
| F07 | El movimiento automático decide demasiado | **ACEPTAR** | Para que la táctica importe, el planificador tiene que ser honesto: llegar o no a la pelota sale de la cinemática del personaje, sin ninguna ayuda oculta. | §9 reescrito con alcanzabilidad cinemática, distinción entre contacto legal y preferido, función de puntuación explícita, estabilidad del plan con bloqueo e histéresis, y "asistencia extra = 0". Se agrega §9.5 sobre profundidad táctica: posición de espera con riesgo y recompensa, tiempo de recuperación según el golpe y métrica de holgura. El radio de reposicionamiento pasa a ser [EXPERIMENTO]. | Validar que la posición de espera funcione también como "anticipación". |
| F08 | Cámara móvil y mira relativa sin contrato común | **ACEPTAR** | Si la mira usa coordenadas de pantalla y la cámara se mueve, el objetivo "se escapa" sin que el jugador mueva el dedo. | La mira usa **coordenadas del frontis en metros** con ganancia fija, independiente de la cámara (§6.4). La cámara fija es la referencia experimental y el seguimiento con amortiguación es la variante (§12). Se agrega una lista de escenarios de legibilidad para First Ball. | Validar la legibilidad en el iPhone con ambas cámaras y ambas manos. |
| F09 | Justificación insuficiente para excluir PhysX | **ACEPTAR** | Tiene razón: predecir trayectorias y probarlas no son ventajas exclusivas de una simulación propia. Mi inclinación por la pelota propia se mantiene (la cancha es una caja de planos, así que el impacto de una esfera es analítico), pero no está demostrada. | §8.2: tres alternativas con **una sola autoridad**, una tabla de criterios, ocho escenarios de prueba con umbrales [EXPERIMENTO] y una regla de decisión ("la más simple que cumpla los escenarios obligatorios"). Prueba técnica acotada en First Ball. | Autorizar la prueba técnica de física cuando se autorice el desarrollo. |
| F10 | 120 Hz no especifica la solución de colisiones | **ACEPTAR** | Es cierto. A 30 m/s se recorren 25 cm por paso, así que hace falta detectar el primer impacto dentro del intervalo. | §8.3: requisitos válidos para cualquier motor. Radio físico de 20 mm, primer impacto dentro del paso, tiempo restante, varios impactos por paso con tope, esquinas con épsilon, y líneas evaluadas por el contacto de la esfera ("tocar la línea = out"). | Ninguna. Es un requisito técnico. |
| F11 | Escalar el tiempo ≠ escalar la velocidad | **ACEPTAR** | Es correcto. Con la misma gravedad, bajar la velocidad de lanzamiento cambia la forma de la trayectoria. Ralentizar todo el tiempo no la cambia. | §8.4: tres controles separados (**ritmo global**, **escala de lanzamiento** y **cámara lenta de depuración**), un solo reloj de simulación, conversión de las marcas de tiempo del input y ventanas definidas en tiempo real percibido. Propuesta de partida: ritmo global, que conserva la forma real de las trayectorias para que el PO pueda validarlas. | Elegir el mecanismo de ralentización después de First Ball. |
| F12 | El shot solver no tiene contrato para objetivos imposibles | **ACEPTAR** | Un punto en el frontis más una potencia no siempre tienen solución, ni una única solución. | §7.4: la potencia se define como velocidad de salida en m/s dentro de un rango, la política es "se respeta el objetivo y se ajusta la potencia mínima" con el arco más bajo, la región factible se ve antes de pegar, y se separan intención, ajuste y salida física. Física continua: las bandas solo etiquetan el golpe, no cambian la trayectoria. El error se aplica *antes* de resolver y nunca se corrige después. | Ninguna para la v0.2. Se valida la sensación en First Shot. |
| F13 | El drop mezcla técnica de golpe y superficie | **DIFERIR → First Shot** | Coincido con el diagnóstico. No hace falta decidir el mecanismo antes de probar un drop con superficies consistentes. | §7.5: First Shot empieza con superficies y pelota consistentes. Si el drop no "muere", se agrega un estado explícito de la pelota (*cortado*), compartido por el predictor, el solver y la IA. | El PO juzga en First Shot si el drop se siente como un drop. |
| F14 | El sesgo direccional del error no puede ser universal | **ACEPTAR PARCIALMENTE** | Es correcto: mi regla "tarde abre al medio" no tenía en cuenta el lado ni la orientación del cuerpo. Propongo una solución distinta a eliminarlo: el sesgo se define **relativo al cuerpo** (contacto adelantado o atrasado respecto del pie delantero, en el lado del drive o del revés) y luego se proyecta a la cancha. Dejo una tabla candidata para que el PO la valide. | §7.6: la línea base de First Shot va **sin sesgo direccional** (solo altura, profundidad y dispersión). La tabla candidata de casos relativos al cuerpo, espejada para ambos lados, queda [PENDIENTE PO]. El sesgo entra solo con los casos que el PO valide. | **Completar o corregir la tabla de casos de error** (§7.6.3). |
| F15 | El modelo multiplicativo y el azar ocultan la causa | **ACEPTAR PARCIALMENTE** | Acepto: contribuciones acotadas, error anisotrópico (vertical y lateral por separado), distribución explícita, semilla registrada y comparación con y sin azar. Retiro mi afirmación de que un modelo determinista "sería aburrido": era una hipótesis. Discrepo en dejar el juego determinista de forma permanente: el principio del PO es errores *comprensibles*, no cero variación. | §7.6: fórmula **aditiva con topes** en lugar de multiplicativa, cada fuente registrada por separado, línea base determinista en First Shot y azar acotado con semilla como experimento en First Fun. La mira se declara como estimación condicionada. | Decidir en First Fun si el azar acotado mejora el juego. |
| F16 | La fatiga penaliza varias veces la misma causa | **ACEPTAR PARCIALMENTE** | La duplicación es real. Pero la cadena causal que describió el PO (piernas → llegar tarde → peor golpe) *es* el modelo correcto, siempre que la fatiga **no** tenga además un término directo de precisión. | §9.4: la fatiga actúa solo sobre el movimiento y la reacción. El brazo solo limita la potencia máxima y la regla de golpe al marco, y entra como experimento separado. La recuperación entre puntos se rige por una regla de juego y no depende de saltar la pausa. Velocidad mínima ≥ 70 % [EXPERIMENTO]. Energía fija hasta First Fun. | Validar que el desgaste se sienta "drama, no castigo". |
| F17 | Compartir la física no basta para una IA justa | **ACEPTAR** | Coincide con la Prioridad 6 del PO. | §11.2: contrato de información con lo **observable** y lo **prohibido**, latencias separadas, la IA emite órdenes por el mismo contrato de contacto, y las fuentes de error se listan con topes. Predecir la trayectoria de la pelota está permitido porque el planificador del humano también lo hace (simetría). | Validar que la IA "no hace trampa" a la vista. |
| F18 | Los estilos necesitan contexto y contrajuego | **ACEPTAR** | Una preferencia fija por golpe se vuelve predecible y explotable. | §11.3: decisión por **utilidad** sobre un conjunto pequeño de opciones factibles, rasgos de contexto (holgura, posición propia y del rival, margen al tin, tiempo de recuperación), defensa obligatoria bajo presión, variación entre opciones razonables. El estilo queda separado de la habilidad. Sin árbol de comportamiento grande ni aprendizaje automático. | Validar las tablas de estilo con su experiencia. |
| F19 | Los cuatro pilares no están validados | **ACEPTAR PARCIALMENTE** | Acepto que el MVP no valida el sistema de personajes. Discrepo en eliminar campos: el esquema completo es una decisión del PO y su costo de mantenimiento es mínimo (datos, no código). | §10: validación **parcial explícita**. El esquema conserva todos los subatributos, pero el código solo conecta los activos. Hay una tabla de atributos activos con capacidad afectada, límite y decisión que sigue siendo del usuario. Se unifica precisión con técnica por golpe para evitar el doble efecto. Validación completa en el milestone Personajes. | Confirmar que el MVP no valida los cuatro pilares. |
| F20 | La separación lógica puede volverse sobreingeniería | **ACEPTAR PARCIALMENTE** | Corrijo dos afirmaciones de la v0.1: el C# puro no es requisito para Android y el modo online no justifica abstracciones hoy. Mantengo una frontera liviana (un ensamblado lógico y uno de integración) porque los 12 casos del contrato de contacto se prueban más rápido sin escena. | Anexo A reescrito: dos ensamblados, un adaptador de física que puede usar Unity si gana PhysX, ScriptableObjects para autoría con copia en tiempo de ejecución, y la lista de herramientas que no se usan. | Ninguna. Es técnica. |
| F21 | El panel necesita reglas de cambio y persistencia | **ACEPTAR** | Un cambio físico aplicado a mitad del gesto invalida la predicción y la comparación entre sesiones. | §13.4: tres clases de parámetros (inmediatos, próximo rally, reinicio) y persistencia en JSON local versionado, con registro de preset, build y semilla junto a las métricas. Exportación por el menú de compartir, avance por pasos de simulación y prueba de exclusión en release. El panel crece por milestone (§13.2). | Ninguna. El PO prueba que un preset se recupere tras reiniciar la app. |
| F22 | FPS promedio no verifica la experiencia móvil | **ACEPTAR PARCIALMENTE** | Acepto los percentiles, los frames largos, la sesión de 15 minutos, el estado térmico y la latencia de input. Ajuste: los umbrales p95/p99 quedan como [EXPERIMENTO] hasta conocer el iPhone del PO. Agrego un riesgo que Codex mencionó al pasar: la **compatibilidad de Unity 6 LTS con Xcode 27.1** hay que verificarla al empezar First Ball. | §17.A y §17.B: 60 FPS como meta, presupuesto de 16,7 ms, percentiles, frames > 33 ms, 15 minutos con estado térmico, latencia del toque a la respuesta. Presupuesto de predicción. Riesgo §16.12. | **Indicar el modelo de iPhone y la versión de iOS.** |
| F23 | Las pruebas de aceptación no miden lo que pretenden | **ACEPTAR** | Es correcto: "<5 cm en el primer frontis" y "drop delante de la short line" se pueden cumplir sin que el golpe sirva. | §17 reestructurado en A (invariantes), B (hipótesis), C (objetivos flexibles) y D (pendientes), con eventos medibles (primer y segundo bote, margen al tin, desvío tras varios contactos). Anexo C con escenarios deportivos de referencia que el PO debe aceptar. La prueba a ciegas se marca como exploratoria. | Aceptar o corregir los escenarios de referencia (anexo C). |
| F24 | Las reglas deben separar retorno, saque y fase | **ACEPTAR** | Hay que distinguir el saque, que va directo al frontis, del retorno, que puede tocar las laterales antes. | §8.5: reglas por fase, saque directo al frontis, botes y líneas por contacto de la esfera, reinicio del conteo de botes, **un solo cierre de punto**, turno de golpe. PAR a 7 identificado como adaptación (el oficial es a 11). | Ratificar el tin de 0,48 m o 0,43 m. |
| F25 | Duración y pausas atadas a números iniciales | **ACEPTAR PARCIALMENTE** | La aritmética era incorrecta: sin 6-6 son 7–12 rallies, y desde 6-6 hacen falta ≥ 14, sin máximo. Acepto medir mediana y cola. **Discrepo** en tratar los 7 puntos como hipótesis: el PO lo aprobó y lo reconfirmó. Según el PO, la duración es una referencia flexible y la física no se modifica para forzarla. | §5.4 corregido. Duración = objetivo flexible C. Métricas de tiempo activo, espera, mediana y p90. Retroalimentación de contacto, pared, tin y punto desde First Shot, con alternativa visual si no hay audio. | Ninguna. Ya está decidido. |
| F26 | Las etiquetas confunden resultado con mecánica nueva | **DIFERIR → milestone Golpes ampliados** (corrección de nomenclatura ahora) | Tiene razón: nick es un resultado y kill es una región de intención. Corregir los nombres no cuesta nada. El efecto en el alcance se decide cuando se amplíen los golpes. | §7.2: el catálogo se separa en **intenciones**, **condiciones de contacto** y **resultados emergentes**. Se elimina la correspondencia rígida entre estilos y arquetipos (Shot ≠ Powerhouse). No se bloquea un nick que ocurra de forma natural. | Ninguna. |

## Análisis detallado de los hallazgos críticos y de las prioridades del PO

### F03: contacto y timing (Prioridad 1)

**Problema real.** La v0.1 nunca decía *en qué instante* la raqueta toca la pelota. Se podían hacer al menos tres implementaciones incompatibles:
1. Soltar el dedo provoca el contacto inmediato.
2. Soltar el dedo programa un swing.
3. Soltar el dedo deja un golpe en cola hasta el contacto previsto.

Cada una produce un juego distinto.

**Alternativas evaluadas:**

| Modelo | Cómo funciona | A favor | En contra |
|---|---|---|---|
| Contacto inmediato | El contacto ocurre al soltar, donde esté la pelota. | Máximo control. | Con el movimiento automático, el cuerpo podría no estar en su lugar. Exige teletransportar el contacto o fallar muchísimo. Muy difícil con una mano. |
| Swing programado | Soltar inicia una preparación de duración fija y el contacto ocurre al final. | Realista: hay que anticipar la preparación. | Se suma una latencia que el jugador tiene que aprender. Si la intercepción cambia, el swing queda mal. |
| **Contacto asistido con ventana limitada** | El planificador fija un instante ideal de contacto T\*. Soltar dentro de la ventana compromete la orden. El contacto ocurre en T\* si se soltó antes, o en el instante de soltar si se soltó después y la pelota sigue siendo legal. El desfase define la calidad. | Coherente con el movimiento automático. Fácil de aprender. El error tiene una causa medible (Δ). Fácil de probar. | Menos "físico" que el swing. La ventana tiene que ser lo bastante estrecha para que el timing importe. |

**Recomendación:** contacto asistido con ventana limitada, como **hipótesis para First Shot**. No está aprobado. El swing programado queda como variante si el PO siente que el timing "no se juega". El detalle completo y los 12 casos verificables están en el §7.1 de la v0.2.

### F04: controles (Prioridad 2)

Las tres alternativas que pidió el PO, con un ajuste mío a la A:

| Criterio | A′: gesto original con mira congelada | B: apuntar, fijar y tensar | C: potencia contextual asistida |
|---|---|---|---|
| Una mano | Sí | Sí | Sí |
| Ergonomía | Un solo trazo | Dos fases en un trazo, con una pausa breve | Un solo trazo, más corto |
| Precisión de dirección | Media: el flick puede mover la mira (mitigado) | Alta: la mira queda fija antes de tensar | Alta |
| Control de potencia | Continuo pero ruidoso | Continuo y deliberado | Solo grueso (un flick suma +1 nivel) |
| Curva de aprendizaje | Media | Media-alta | Baja |
| Velocidad de rally | Alta | Media: la pausa cuesta tiempo con pelotas rápidas | Alta |
| Vertical | El flick hacia arriba es natural | El tensado necesita espacio vertical | Sin problemas |
| Riesgo principal | Potencia involuntaria | Lento bajo presión | Poca profundidad: drop y kill se confunden |

**Recomendación:** comparar A′, B y C en First Shot con el protocolo del §6.3 de la v0.2. C sirve también como **línea base de diagnóstico**: si C ya resulta divertido, el problema de A o B está en el control de potencia y no en el timing ni en la dirección.

### F07: movimiento automático (Prioridad 3)

**Problema real.** Si el planificador siempre llega, o llega siempre en buena posición, los golpes profundos y abiertos no le cuestan nada al rival y la táctica desaparece.

**Resolución:**
1. Alcanzabilidad **honesta**, que sale de la cinemática del personaje, sin bonos de asistencia.
2. **Holgura** (tiempo de margen al llegar) como variable central: un buen golpe del rival reduce tu holgura y eso empeora tu golpe (§7.6).
3. Tres decisiones del usuario con costo real:
   - la **posición de espera**: ir hacia un lado es una apuesta, que es justo el "adivinar" que describió el PO;
   - la **elección de golpe**: los golpes altos o lentos te dan tiempo para recuperar, los rápidos se lo quitan al rival;
   - el **riesgo cerca del tin**.
4. Una métrica de validación: la holgura del rival tiene que bajar de forma visible después de los golpes profundos y abiertos. Si no baja, no hay profundidad táctica.

### F09: física (Prioridad 5)

No se decide de antemano. La v0.2 define una sola autoridad sobre la pelota, ocho escenarios, umbrales y una regla de decisión. La decisión se toma después de una prueba técnica acotada en First Ball, autorizada por el PO. Mi inclinación (no aprobada) es una pelota con simulación propia: la cancha es una caja de planos alineados con los ejes, y el impacto de una esfera contra un plano se resuelve con fórmulas exactas, de modo que la predicción y la simulación comparten el mismo código. PhysX gana si la prueba muestra que su predicción en escena separada cumple los umbrales con menos código.

## Trazabilidad

El anexo D de la v0.2 indica, para cada hallazgo, la sección de la v0.2 que lo resuelve.
