# Squash Clash — Revisión independiente de la especificación v0.1

Fecha: 19 de octubre de 2026. Autor: Codex.

Documento leído íntegramente: [2026-10-19-squash-clash-design.md](../superpowers/specs/2026-10-19-squash-clash-design.md), versión 0.1, 597 líneas. Las referencias § corresponden a ese documento.

Estado: revisión para evaluación de Claude y del Product Owner. Ninguna recomendación de este informe constituye una decisión aprobada. No se modificó la especificación, no se creó un proyecto Unity y no se implementó código. Los hitos propuestos son experimentos de validación, no un plan de implementación definitivo.

## 1. Evaluación general

**La especificación es una buena base de diseño, pero recomiendo revisarla antes de consolidarla como base para un plan de implementación.** Expresa con claridad qué experiencia se busca y reconoce varios riesgos importantes. La mayor debilidad es que presenta algunas soluciones todavía no comprobadas con un grado de certeza superior a la evidencia disponible.

El problema principal no es la ambición del proyecto personal ni su duración. Es el orden de aprendizaje: se acumulan saque, fatiga, errores, dos estilos de IA y partido completo antes de saber si apuntar, golpear y recuperar posición resultan satisfactorios. Esto dificulta explicar por qué una prueba funciona o falla.

Tres contratos necesitan especial precisión: **cuándo puede ocurrir un contacto**, **cómo se separan dirección, potencia y timing**, y **qué información y restricciones comparten humano e IA**. La arquitectura depende de esas respuestas más que de la elección inicial entre PhysX y física propia.

Discrepo de dos premisas de la spec: la física propia no es necesaria por definición para predecir y probar trayectorias; y un modelo de errores determinista no implica un juego aburrido. Tampoco considero obligatorio añadir todos los golpes temprano: drive, cruzado y drop alcanzan para probar el gesto, aunque no para cerrar la validación del dominio de la T.

### Cómo interpretar los hallazgos

| Severidad | Significado en esta revisión |
|---|---|
| CRÍTICO | Resolver antes de aprobar el contrato afectado: permite implementaciones incompatibles o confunde quién autoriza decisiones. |
| ALTO | Riesgo significativo de rehacer sistemas centrales o de invalidar una prueba de gameplay. |
| MEDIO | Afecta calibración, verificabilidad, mantenimiento o alcance; puede resolverse antes de su hito correspondiente. |
| BAJO | Ajuste local de claridad o nomenclatura con poco impacto inmediato. |

**Comprobable** significa observable en el texto, en una deducción explícita o en documentación primaria. **Riesgo** significa una consecuencia posible, todavía no observada en ejecución. **Hipótesis** necesita una prueba jugable. **Preferencia de diseño** es una elección defendible entre alternativas. Se distinguen las omisiones comprobadas de los efectos que solo son riesgos.

No se dispone de la conversación completa de brainstorming ni de builds o mediciones. La afirmación de Claude de que algo fue aprobado no demuestra aquí su procedencia, pero tampoco demuestra que la aprobación nunca existió. Para esta revisión prevalece el encargo actual del PO.

## 2. Fortalezas que conviene conservar

- **Objetivo emocional concreto:** construir un punto, variar y terminar satisfecho es más útil para evaluar el juego que una lista extensa de funcionalidades (§1).
- **PO con criterio deportivo y acceso directo a la prueba:** permite distinguir una simplificación arcade deliberada de una pérdida accidental de identidad (§3).
- **Prioridad de gameplay sobre arte y monetización:** adecuada para un proyecto personal sin presión comercial (§2).
- **Movimiento automático como punto de partida accesible:** merece probarse; no es una deficiencia por sí mismo (§9).
- **Ejecución compartida entre humano e IA:** excelente principio para construir rivales justos y depurables (§11).
- **Dos estilos con atributos físicos y técnicos iguales:** buena forma de aislar inicialmente la táctica (§10.4).
- **Panel local, presets y práctica:** herramientas de aprendizaje útiles desde temprano, si crecen con las preguntas que se están investigando (§13).
- **Núcleo lógico separable, configuración en datos y pruebas de reglas:** una dirección razonable sin necesidad de una infraestructura grande (anexo A).
- **Riesgos reconocidos y criterio final del PO:** la flexibilidad del §17 es apropiada para la diversión; las invariantes técnicas necesitan un tratamiento distinto.

## 3. Hallazgos críticos y de alcance

### F01 · CRÍTICO · Trazabilidad de las aprobaciones

**Secciones:** etiquetas iniciales; §2.2, §5.4, §6.1, §8.3, §14.1 y §16.1. **Naturaleza:** discrepancia documental comprobable; procedencia no verificable.

**Problema:** se etiquetan como aprobados 7 puntos, 3–5 minutos y 60–70 % de velocidad, mientras el encargo actual pide tratarlos como hipótesis salvo evidencia explícita. La elección detallada del gesto y el alcance completo también remiten a un brainstorming no adjunto.

**Consecuencia:** un plan podría convertir valores experimentales en compromisos del PO o impedir reconsiderarlos.

**Recomendación:** en la siguiente revisión, añadir procedencia breve a las aprobaciones materiales: mensaje, fecha o confirmación expresa. Mantener esos tres parámetros como hipótesis en este informe. Conservar las decisiones confirmadas en el encargo actual: iPhone, vertical, Unity/C#, movimiento automático, recuperación asistida y gestos contextuales.

**Alternativa:** una tabla breve de decisiones ratificadas por el PO, sin reconstruir toda la conversación. Esta revisión no revoca aprobaciones históricas que posteriormente se acrediten.

### F02 · ALTO · El First Playable mezcla demasiadas preguntas

**Secciones:** §1, §14 y §15, especialmente M3–M4. **Naturaleza:** alcance comprobable; riesgo de diagnóstico.

**Problema:** el primer entregable de diversión incluye partido, saque, fatiga, reposicionamiento, dos IAs, tres dificultades, presets y métricas. El roadmap ya contiene incrementos útiles; no parte de cero. Sin embargo, M3 aún incorpora simultáneamente varios sistemas que alteran el rally.

**Consecuencia:** si el PO no disfruta, será difícil distinguir un mal control de una IA mal calibrada o de una penalización excesiva por cansancio.

**Recomendación:** separar First Ball, First Shot, First Rally, First Match y First Fun con preguntas de salida concretas (§8 de este informe). Mantener el panel desde temprano, con funciones incrementales. Probar primero el rally con energía fija y una sola política rival.

**Alternativa:** conservar M0–M4, pero agregar puntos de validación internos antes de integrar saque, fatiga y dificultad. No es necesario reducir la visión final.

### F03 · CRÍTICO · Falta un contrato único de contacto y timing

**Secciones:** §7.3–7.5, §9.1, §9.4 y §17.2. **Naturaleza:** ambigüedad y contradicción comprobables; riesgo central de jugabilidad.

**Problema:** no queda claro si soltar causa contacto inmediato, programa un swing o encola un golpe para el contacto previsto. El objetivo puede cambiar mientras corre el personaje. Soltar muy temprano espera hasta el primer alcance, pero no se limita esa espera. Además, se prohíbe toda pelota fallada con buena posición (§17.2.8), aunque no soltar o soltar tarde produce ausencia de golpe (§7.5).

**Consecuencia:** distintas implementaciones podrían teletransportar el contacto, cambiar retrospectivamente el timing o permitir un golpe automático al soltar inmediatamente tras el rival. También se penalizaría a un jugador por un objetivo que el sistema movió después de su acción.

**Recomendación:** distinguir fallo de ejecución de ausencia de orden y pelota inalcanzable. Precisar ventana física de contacto, instante de referencia, duración máxima de una orden anticipada, política si cambia la intercepción y prohibición de golpear fuera de alcance o después del segundo bote. Mostrar cuándo la orden queda comprometida. Ninguna nota de timing debe sustituir esos requisitos.

**Alternativas:** contacto asistido dentro de una ventana corta con timing que modula calidad, o swing programado con preparación explícita. Ambas requieren validación del PO; los ±50/120/250 ms son hipótesis, no verdades ergonómicas.

## 4. Controles, cámara y experiencia táctica

### F04 · ALTO · El flick final acopla potencia y puntería

**Secciones:** §6.1–6.3. **Naturaleza:** acoplamiento comprobable; dificultad de uso como hipótesis.

**Problema:** el mismo desplazamiento mueve la mira y su velocidad final determina potencia, justo cuando soltar determina timing. El movimiento necesario para pegar fuerte también cambia el objetivo; una corrección rápida de puntería puede convertirse en potencia involuntaria. Tras apuntar y mantener quieto el dedo, el golpe tendería a salir suave.

**Consecuencia:** los errores podrían expresar limitaciones del gesto y no decisiones deportivas. La zona de riesgo previa tampoco puede conocer exactamente la potencia final.

**Recomendación:** probar el esquema original en una mano antes de descartarlo. Compararlo con dos alternativas pequeñas: arrastre para apuntar y longitud de una fase final para potencia, con objetivo ya fijado; o arrastre para apuntar y carga breve con indicador y límite, soltando para ejecutar. La carga tiene su propio conflicto con el tiempo disponible y debe evaluarse.

**Alternativa mínima:** potencia contextual asistida para aislar dirección y timing en First Shot; después restituir control explícito de potencia para evaluar la visión completa. Registrar objetivo deseado, objetivo final y potencia involuntaria, no solo aciertos.

### F05 · ALTO · Toque de recuperación y gesto de golpe carecen de prioridad definida

**Secciones:** §6.1, §6.4 y §9.3. **Naturaleza:** omisión comprobable; riesgo de acciones involuntarias.

**Problema:** “mientras el rival tiene la pelota” no identifica un estado inequívoco en squash. Un toque iniciado durante recuperación puede terminar cuando ya se habilitó el golpe. Umbrales en píxeles no fijan una experiencia consistente entre pantallas. No se define cancelación, segundo dedo ni pérdida de foco.

**Consecuencia:** una orden táctica puede convertirse en swing o viceversa; sostener el teléfono con una mano agrava los toques accidentales.

**Recomendación:** definir fases y prioridad: recuperación, preparación de recepción, gesto capturado y rally terminado. Un gesto conserva su propósito al cambiar de fase; cancelar no ejecuta. Normalizar umbrales por espacio de pantalla, contemplar zonas del sistema y distinguir lateralidad de interfaz de mano dominante del personaje.

**Alternativas:** reposicionamiento solo en una ventana tras el golpe propio; o sesgo limitado de espera mediante una zona dedicada. Probar pronto el conflicto real, manteniendo la recuperación automática como base.

### F06 · ALTO · Tres golpes sirven para el gesto; la volea necesita una prueba táctica temprana

**Secciones:** §7.1, §9.1, §15 M5 y §16.5. **Naturaleza:** hipótesis de diseño.

**Problema:** postergar toda volea hasta después de declarar divertido el First Playable elimina temporalmente una forma de quitar tiempo al rival y aprovechar la T. Un arquetipo adelantado puede quedar mal evaluado sin esa posibilidad.

**Consecuencia:** se podría rechazar el movimiento automático o la táctica por una carencia del repertorio de contacto. A la inversa, añadir muchos golpes de inmediato ocultaría el problema del gesto.

**Recomendación:** conservar drive, cruzado y drop en First Shot. Antes de cerrar la validación táctica de First Rally, comparar esos mismos golpes con y sin contacto antes del bote. La volea es una condición de contacto, no exige tres gestos nuevos ni una familia completa de animaciones finales.

**Alternativa:** un experimento de volea limitado a pelotas claras a media altura. Posponer boast, amague y simulación avanzada de spin. No introducir globo por obligación: adelantar una salida alta sencilla si la volea genera un dominio del centro sin contrajuego.

### F07 · ALTO · El movimiento automático puede decidir demasiado o elegir contactos inviables

**Secciones:** §9.1–9.3 y §16.3. **Naturaleza:** criterio incompleto comprobable; riesgos tácticos.

**Problema:** “momento óptimo”, “mejor punto alcanzable” y preferir cintura no definen prioridades. El mejor contacto puede ser más bajo o anterior; una pelota baja quizá nunca llegue a la cintura tras el bote. No se precisa cuándo se fija o actualiza el destino. La zona de 2 m alrededor de la T también puede recortar opciones útiles sin evidencia.

**Consecuencia:** cambios continuos de destino, desplazamientos extraños o asistencia que devuelve cualquier pelota y anula el valor del golpe profundo. El usuario puede sentir que acertó la táctica pero el piloto automático decidió el punto.

**Recomendación:** definir alcanzabilidad con aceleración, frenado, alcance y tiempo; diferenciar contacto preferido de contacto legal. Establecer prioridad y estabilidad de la intercepción. Medir si profundidad, ancho y cambio de ritmo producen ventajas reales con atributos iguales.

**Alternativa:** asistencia con margen limitado y sesgo táctico del usuario. Si la agencia falla, revisar el control antes de cerrar First Fun: §16.3 no debería obligar a aplazar la solución central solo para terminar M4.

### F08 · ALTO · Cámara móvil y mira relativa necesitan un contrato común

**Secciones:** §6.1, §8.5 y §12.1. **Naturaleza:** riesgo de legibilidad y control.

**Problema:** el seguimiento lateral cambia la proyección del frontis durante el arrastre. No se define si la mira conserva coordenadas del frontis o de pantalla. Un frontis en el tercio superior puede comprimir la precisión vertical; transparencia de personajes no resuelve por sí sola la lectura de una pelota en la esquina trasera.

**Consecuencia:** objetivo aparentemente móvil, confusión altura/profundidad y necesidad de mirar simultáneamente pelota, mira y anillo de timing.

**Recomendación:** usar coordenadas de objetivo estables en el frontis y explicitar su proyección. Tomar cámara fija como referencia experimental, dentro de la perspectiva aprobada, y compararla con seguimiento. Probar ambos fondos, rebote en pared trasera, pelota baja junto a pared y oclusiones en el iPhone real, con manos derecha e izquierda.

**Alternativas:** limitar o suspender seguimiento durante apuntado; ajustar encuadre, escala visual de pelota, sombra y estela. Las ayudas no deben ser un requisito para comprender cada rebote ni ruido que tape al rival.

## 5. Física y resolución de golpes

### F09 · ALTO · La justificación para excluir PhysX es insuficiente

**Secciones:** §8.2 y anexo A. **Naturaleza:** omisión de alternativas comprobable; elección técnica pendiente.

**Problema:** predecir trayectorias y automatizar pruebas no son capacidades exclusivas de una simulación propia. Unity documenta escenas físicas independientes para predicción y simulación manual con paso fijo. Eso no promete coincidencia universal ni determinismo entre plataformas. Véanse [Multi-scene physics](https://docs.unity3d.com/6000.0/Documentation/Manual/physics-multi-scene.html) y [PhysicsScene.Simulate](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PhysicsScene.Simulate.html).

**Consecuencia:** se asumiría el mantenimiento de un motor pequeño sin demostrar que su ventaja compensa el costo.

**Recomendación:** elegir según la comparación del apartado siguiente y una prueba acotada futura con escenarios idénticos. Mi inclinación provisional es una pelota propia y presentación Unity si la cancha sigue siendo geometría estática simple y la predicción resulta central; no la considero aprobada ni superior sin contraste.

**Alternativa:** Rigidbody con CCD y predicción en escena independiente. No desarrollar tres motores completos para resolver esta decisión.

### Comparación técnica solicitada

“Híbrida” significa aquí **una sola autoridad sobre la pelota** y Unity para presentación, personajes y, si conviene, consultas geométricas. No significa que Rigidbody y un integrador propio corrijan alternativamente la misma pelota.

| Criterio | Rigidbody / PhysX | Pelota con simulación propia | Solución híbrida definida arriba |
|---|---|---|---|
| Predicción | Posible con escena física independiente; requiere reproducir estado, materiales y pasos relevantes. | Puede reutilizar el mismo avance de estado sin objetos de escena; igualdad depende también de estado y configuración. | Igual ventaja si predicción y juego usan la autoridad propia; consultas Unity añaden dependencias. |
| Consistencia | Paso fijo ayuda; evitar prometer determinismo entre dispositivos. | Control directo de orden y fórmulas; usar C# y floats tampoco garantiza determinismo multiplataforma. | Depende de qué parte resuelva contactos y de evitar dos autoridades. |
| Colisiones | Resuelve muchas situaciones ya; CCD y esquinas requieren validación. | Hay que resolver esfera contra superficies, tiempos de impacto, simultaneidad y separación numérica. | Puede reutilizar consultas; hay que precisar quién calcula tiempo y respuesta de impacto. |
| Rendimiento | Una pelota y cancha simple pueden ser viables; predicciones repetidas añaden costo. | Puede ser liviana en esta geometría; optimización y robustez corren por cuenta del proyecto. | Integración y consultas tienen costo; no hay ahorro garantizado por llamarla híbrida. |
| Complejidad inicial | Menor en rebotes básicos; aumenta al sincronizar predictor, solver y reglas. | Mayor en robustez numérica; la forma rectangular acota el problema. | Moderada si las fronteras son claras; alta si mezcla dos respuestas de colisión. |
| Mantenimiento | Dependencia de versión/configuración Unity y regresiones del motor. | Dependencia de código propio y de sus casos límite; obliga a documentar invariantes. | Ambos tipos de dependencia, pero acotados por responsabilidad. |
| Pruebas | Pruebas de integración en Unity y lógica de reglas separable. | Pruebas numéricas pequeñas y rápidas más integración visual. | Pruebas de lógica más contratos entre simulación y escena. |
| Futuro online | Compatible con distintas estrategias de autoridad y reconciliación. | Estado compacto puede ayudar a repetir y corregir; no elimina problemas de red. | Buena separación si se conserva una autoridad; tampoco decide el modelo de red. |

Las valoraciones de costo y conveniencia son juicio de ingeniería para este proyecto, no resultados de un benchmark. La predicción PhysX está respaldada por la documentación citada; su precisión aquí sigue sin medirse.

La futura comparación debería usar: drive rápido, drop bajo, rebote lateral rasante, esquina lateral/suelo, frontis/lateral y dos contactos cercanos dentro de un paso. Comparar desvío espacial y temporal en varios rebotes, problemas de colisión, costo de predecir y esfuerzo de depuración. No basta la precisión en el primer frontis. Elegir la opción más simple que satisfaga los escenarios acordados; no fijar todavía arquitectura de red, rollback o aritmética determinista.

### F10 · ALTO · 120 Hz no especifica una solución de colisiones

**Secciones:** §8.1–8.2, §8.4 y §17.1. **Naturaleza:** omisión técnica comprobable; riesgo numérico.

**Problema:** faltan radio físico de pelota, tratamiento de líneas, orden de contactos simultáneos y comportamiento ante más de un rebote por paso. Como ejemplo ilustrativo, a 30 m/s se recorren 25 cm en un paso de 120 Hz: aumentar frecuencia no basta si solo se detectan solapamientos al final.

**Consecuencia:** atravesar superficies, rebotes diferentes según el paso y puntos disputables junto al tin o las líneas.

**Recomendación:** exigir detección del primer impacto dentro del intervalo, tratamiento consistente del tiempo restante y política de tolerancias y esquinas. Definir las reglas sobre el contacto de la esfera, no solo sobre su centro. Si se usa PhysX, evaluar CCD; Unity lo describe como protección entre pasos, con costo adicional y límites de precisión en [Continuous collision detection](https://docs.unity3d.com/6000.0/Documentation/Manual/ContinuousCollisionDetection.html).

**Alternativa:** subdivisión adaptativa con límites y pruebas explícitas. Reservar el nick fiel para después; eso no permite ignorar la estabilidad básica de la unión pared/suelo.

### F11 · ALTO · Escalar el tiempo y escalar la velocidad no son equivalentes

**Secciones:** §8.3 y §13.2. **Naturaleza:** diferencia física comprobable; decisión de diseño pendiente.

**Problema:** “escala de tiempo y/o velocidad” mezcla dos experimentos. Reducir velocidad inicial manteniendo gravedad altera la trayectoria espacial; reproducir una misma trayectoria más lentamente no. Ralentizar solo la pelota también cambia su relación con los desplazamientos y las ventanas de contacto.

**Consecuencia:** calibración confusa: un preset más lento puede volver inalcanzable el frontis, acortar un drive o facilitar todas las recuperaciones. El 60–70 % carece además de una referencia de velocidad definida.

**Recomendación:** separar velocidad de lanzamiento, ritmo global y cámara lenta de depuración. Explicitar qué reloj usan pelota, jugadores, fatiga y timing, y cómo se relacionan los milisegundos de entrada real con el tiempo simulado. No aplicar dos escalas inadvertidamente.

**Alternativas:** física arcade calibrada con gravedad y lanzamientos propios; o ralentización coherente de todo el rally. Evaluarlas por sensación y legibilidad, sin exigir un porcentaje del squash real.

### F12 · ALTO · El shot solver no tiene contrato para objetivos imposibles

**Secciones:** §6.3 y §7.2–7.3. **Naturaleza:** especificación incompleta comprobable.

**Problema:** punto en frontis y potencia no siempre definen una solución físicamente posible ni única. Faltan semántica de potencia, elección entre arcos, límites de alcance y política para trayectorias que botan antes del frontis. Una mira más grande anuncia imprecisión, pero no anuncia que el sistema cambió el destino o transformó un drop en otra trayectoria.

**Consecuencia:** resultados discontinuos al cruzar bandas o asistencia invisible que invalida la elección del jugador.

**Recomendación:** definir potencia como magnitud concreta, política de solución y región de objetivos factibles. Diferenciar intención, ajuste asistido y salida física; previsualizar el ajuste antes de comprometer el golpe. Tras aplicar error a objetivo y potencia, resolver una salida consistente con ambos, sin corregirla después para forzar el acierto.

**Alternativas:** potencia que controla tiempo de vuelo dentro de límites, o un conjunto pequeño de perfiles continuos de trayectoria. Son compromisos arcade posibles, pendientes de prueba; las etiquetas de golpes no deberían introducir saltos ocultos.

### F13 · MEDIO · El drop mezcla técnica de golpe y propiedad de superficie

**Secciones:** §7.2 y §8.2. **Naturaleza:** ambigüedad comprobable; preferencia de aproximación física.

**Problema:** se propone representar el corte con “restitución y velocidad menores”, pero la restitución está definida por superficie. No se aclara si cambiará la pared, la pelota o un estado temporal del golpe.

**Consecuencia:** dos impactos físicamente equivalentes podrían rebotar distinto solo por la etiqueta “drop”, sin que predictor o jugador sepan por qué.

**Recomendación:** comenzar con lanzamiento y superficies consistentes. Si no basta para un drop satisfactorio, documentar un estado simplificado de la pelota que explique la diferencia, compartido por simulación, solver e IA. No es necesario implementar Magnus para hacerlo explícito.

**Alternativa:** aceptar una simplificación arcade dependiente del golpe, con alcance y duración precisos. Debe probarse como decisión de sensación, no presentarse como efecto físico ya demostrado.

## 6. Errores, fatiga, personajes e inteligencia artificial

### F14 · ALTO · El sesgo direccional del error no puede ser universal

**Secciones:** §7.4 y §18.1–18.2. **Naturaleza:** generalización no sustentada; hipótesis deportiva.

**Problema:** “tarde abre al medio” y “temprano cruza de más” no especifican derecha/revés, lateralidad, orientación corporal, posición de contacto ni golpe solicitado. Un desplazamiento fijo en coordenadas de cancha no conserva el mismo significado al espejar la situación.

**Consecuencia:** el jugador experto podría reconocer errores invertidos o incoherentes. Se introduciría una dependencia de lateralidad mientras su modelado sigue pendiente.

**Recomendación:** construir con el PO una pequeña tabla de casos: paralelo y cruzado desde ambos lados, contacto adelantado/atrasado y golpe estirado. Expresar el sesgo en relación con cuerpo, raqueta o dirección pretendida y luego proyectarlo a cancha. Validar casos espejados. Esto puede modelarse sin diferenciar todavía la calidad de derecha y revés.

**Alternativa:** error inicialmente sin sesgo direccional universal, con consecuencias claras en altura, profundidad y calidad; incorporar sesgos solo cuando exista una explicación deportiva aceptada.

### F15 · MEDIO · El modelo multiplicativo y el azar pueden ocultar la causa del fallo

**Secciones:** §7.4, §16.7 y §17.2. **Naturaleza:** riesgo de calibración; hipótesis no demostrada sobre diversión.

**Problema:** los factores multiplicativos pueden amplificarse y contar dos veces la misma dificultad. Un radio circular no representa que un drop tiene tolerancias verticales y laterales diferentes; esa menor tolerancia no determina por sí misma el error de ejecución. La distribución aleatoria tampoco está definida. §16.7 asume sin prueba que el error determinista sería aburrido.

**Consecuencia:** pequeños cambios producen muchos tins y una etiqueta de “cansancio” explica poco. Mostrar una zona de riesgo no garantiza que el fallo se perciba justo.

**Recomendación:** partir con error causal reproducible, acotar contribuciones y registrar qué cambió realmente cada una. Separar sesgo, dispersión y margen respecto de tin/out; si hace falta, usar dispersión anisotrópica. Comparar después con una variación pequeña y registrada por semilla. La incertidumbre del rival puede aportar variedad sin lotería de precisión.

**Alternativa:** mantener azar acotado desde el inicio, con distribución explícita y comparación contra una versión sin él. La mira previa debe declararse estimación condicionada al timing y potencia aún desconocidos.

### F16 · MEDIO · La fatiga puede penalizar varias veces la misma causa

**Secciones:** §5.2, §9.4 y §11.3. **Naturaleza:** riesgo de balance; omisión de recuperación comprobable.

**Problema:** menor velocidad, mayor reacción, peor posición, peor timing y factor de brazo pueden castigar una sola pérdida de energía en cadena. No se define si saltar la pausa reduce la recuperación ni qué parte pertenece al esfuerzo de piernas o brazo cuando solo hay una barra.

**Consecuencia:** espiral de derrota y rallies que terminan por una penalización opaca; quien espera la animación podría recuperar más que quien la omite.

**Recomendación:** probar primero fatiga solo sobre movimiento, con energía fija como control experimental. Incorporar el efecto de brazo únicamente si aporta una sensación diferenciable. Determinar recuperación por regla de juego, independiente de omitir presentación, y límites para evitar personajes prácticamente inmóviles.

**Alternativa:** conservar una sola barra con componentes derivados documentados. No recomiendo simular varios sistemas fisiológicos en esta etapa. La experiencia del PO debe decidir cuánto desgaste entre puntos mejora el juego.

### F17 · ALTO · Compartir física no basta para una IA justa

**Secciones:** §9.1, §11.1–11.4. **Naturaleza:** contrato de información incompleto; riesgo de ventaja invisible.

**Problema:** la IA podría conocer la mira, el input aún no ejecutado o la salida aleatoria futura. “Mismo retraso que el humano” confunde el retraso artificial del personaje con la reacción real del usuario. La distribución de notas de timing no especifica una acción que deba alcanzar físicamente la pelota.

**Consecuencia:** una IA aparentemente legal anticiparía decisiones privadas o acertaría un contacto imposible. En fácil podría acumular demasiadas penalizaciones de reacción y predicción.

**Recomendación:** restringir percepción a estado observable y pasado con latencia definida; no leer intención privada ni próximos valores aleatorios. La IA elige y programa la acción; el sistema compartido determina si hubo contacto y su calidad. Separar sesgo de anticipación, retraso de movimiento y error de ejecución para no sumarlos sin control.

**Alternativa:** asistencia de percepción simétrica para ambos personajes, con dificultad en elección y ejecución. Es aceptable simplificar la inteligencia; la simplificación debe conservar alcanzabilidad, tiempos y reglas visibles.

### F18 · MEDIO · Los estilos son viables, pero necesitan decisiones contextuales y contrajuego

**Secciones:** §10.4, §11.2–11.4 y §17.3. **Naturaleza:** riesgo de predictibilidad; hipótesis de estilo.

**Problema:** preferir un golpe por estilo puede acabar en alternancias fáciles de explotar o drops automáticos desde posiciones malas. El jugador Shot tiene pocos recursos si no puede volear; una distribución aleatoria de decisiones tampoco crea personalidad táctica.

**Consecuencia:** los rivales serían distintos pero poco creíbles, o difíciles únicamente por precisión.

**Recomendación:** conservar percepción–decisión–ejecución–recuperación. Ponderar un conjunto pequeño de opciones factibles por presión, espacio, riesgo y recuperación; variar entre opciones razonables. Separar estilo de habilidad y medir elecciones además de duración del rally. Incluir recuperación defensiva cuando atacar no es viable.

**Alternativa:** pocas reglas explícitas por estilo y ajustes de pesos, sin árbol de comportamiento grande ni aprendizaje automático. Reaccionar al contexto sí es necesario; aprender patrones durante el partido puede seguir siendo futuro. No exigir un tin por partido para demostrar que la IA es humana.

### F19 · MEDIO · Los cuatro pilares están documentados, pero no están validados

**Secciones:** §10.1–10.4 y anexo A. **Naturaleza:** cobertura comprobable; riesgo de expectativas.

**Problema:** el MVP activa atributos físicos, técnicos y anticipación; el pilar mental no tiene efectos. Tener campos en datos no demuestra que los cuatro pilares diferencien personajes. La spec reconoce esta limitación, de modo que no es una contradicción oculta.

**Consecuencia:** declarar validado el sistema de personajes sería prematuro. Precisión general y técnica por golpe también pueden duplicar efectos; anticipación automática puede quitar decisión táctica al usuario.

**Recomendación:** aceptar expresamente la validación parcial inicial. Antes del hito de personajes, definir para cada atributo activo capacidad afectada, límite y decisión que sigue en manos del usuario. Probar pares con ventajas y debilidades compensables. Posponer campos sin uso cuando solo añadan mantenimiento, conservando los cuatro pilares en diseño.

**Alternativa:** añadir luego un efecto mental pequeño, observable y recuperable; evitar convertir concentración en fallos aleatorios inevitables. Diferenciar estilos de IA no sustituye probar capacidades de personajes.

## 7. Arquitectura, rendimiento y verificabilidad

### F20 · MEDIO · La separación lógica es adecuada; su rigidez podría convertirse en sobreingeniería

**Secciones:** anexo A y §15. **Naturaleza:** preferencia arquitectónica y riesgo de costo.

**Problema:** C# puro facilita pruebas, pero no es requisito para Android; Unity ya es multiplataforma. Prohibir dependencias Unity en toda física impondría de hecho la solución propia antes de compararla. Prepararse para online puede justificar abstracciones que hoy no resuelven un problema.

**Consecuencia:** retrasar la sensación del primer golpe por construir infraestructura o duplicar tipos matemáticos y serialización.

**Recomendación:** separar reglas, intención, resolución y presentación con responsabilidades claras y pocas fronteras. Permitir que un adaptador de física use Unity si se elige PhysX. ScriptableObjects para autoría de configuración, convertidos a datos de ejecución donde convenga; no mezclar energía actual o marcador con configuración compartida.

**Alternativas:** un ensamblado lógico y otro de integración pueden bastar. No hay evidencia para ECS/DOTS, contenedor de inyección, bus global, framework genérico de habilidades, motor de red o un motor físico de propósito general. El anexo no exige esas herramientas: se enumeran como expansiones que conviene evitar, no como defectos ya presentes.

### F21 · MEDIO · El panel necesita reglas de cambio y persistencia

**Secciones:** §13 y anexo A. **Naturaleza:** omisión comprobable; riesgo de reproducibilidad.

**Problema:** cambiar gravedad, bandas de golpe o alcance a mitad del gesto invalida predicción y comparación de resultados. Guardar presets no está resuelto solo por usar ScriptableObjects, que son activos de autoría y datos compartidos según el [manual de Unity](https://docs.unity3d.com/6000.0/Documentation/Manual/class-ScriptableObject.html).

**Consecuencia:** un preset puede no reproducir la sesión; valores temporales pueden confundirse con valores base.

**Recomendación:** distinguir cambios visuales inmediatos de parámetros aplicados al siguiente rally; si se permiten cambios físicos instantáneos, invalidar explícitamente predicciones y marcar la sesión. Registrar preset, versión y semilla junto con métricas; exportar datos con unidades y rangos. Guardar en archivo local independiente y verificar recarga al reiniciar la app. Avanzar por paso de simulación, no por frame de render.

**Alternativa:** congelar y reiniciar automáticamente el rally al modificar física. Panel mínimo primero; sin editor administrativo genérico. Mantener y verificar su exclusión de release como requisito técnico real.

### F22 · ALTO · FPS promedio no verifica la experiencia móvil

**Secciones:** §13.5, §16.11, §17.1 y §18.5. **Naturaleza:** criterio insuficiente comprobable; rendimiento sin medir.

**Problema:** ≥55 FPS promedio permite tirones que arruinan timing. No se especifica dispositivo, sesión sostenida, costo de predicción ni latencia táctil. Una escena simple no garantiza bajo costo si se simulan muchos futuros por frame.

**Consecuencia:** una build puede aprobar y volverse imprecisa o caliente tras varias revanchas.

**Recomendación:** identificar iPhone e iOS; proponer 60 FPS como objetivo inicial, con presupuesto de 16,7 ms por frame y registro de percentiles de tiempo de frame, frames largos y CPU/GPU. Probar una sesión de unos 15 minutos, temperatura y consumo aproximado, con registro del contexto. Comparar carga con panel y trazas habilitados/deshabilitados. Limitar horizonte y frecuencia de predicción antes de optimizar a ciegas.

El input debe conservar sus timestamps y tener una relación definida con la simulación; el Input System documenta el tiempo de generación del evento en [Input events](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/manual/Events.html). No hace falta procesar eventos de bajo nivel manualmente para fijar ese requisito.

**Alternativas y límites:** comparar render a 60 FPS con simulación a 60/120 Hz; no exigir render a 120. Priorizar materiales simples, pocas transparencias y sombras antes de posprocesado. Los umbrales p95/p99 definitivos dependen del dispositivo. Unity recomienda perfilar en hardware objetivo en [Best practices for profiling game performance](https://unity.com/how-to/best-practices-for-profiling-game-performance); Apple expone el [estado térmico](https://developer.apple.com/documentation/foundation/processinfo/thermalstate-swift.property?changes=_6). Esta revisión no afirma compatibilidad de una versión específica de Unity con el Xcode declarado: debe verificarse al fijar herramientas.

### F23 · MEDIO · Varias pruebas de aceptación no miden lo que pretenden

**Secciones:** §17.1–17.3. **Naturaleza:** ambigüedades comprobables.

**Problema:** <5 cm en el primer frontis no valida intercepción después de varios rebotes ni tiempo de llegada. “Drop delante de la short line” admite pelotas muy sueltas si solo se mide el primer bote. “Termina en el cuarto trasero” no define primer bote, segundo bote o posición final. Estar dentro de una zona en 90 % de los casos es débil si el generador ya restringe todos los errores a ella.

**Consecuencia:** aprobar tests sin demostrar precisión, profundidad o un drop útil.

**Recomendación:** separar invariantes técnicas obligatorias de metas de sensación flexibles. Medir eventos concretos: lugar/tiempo de primer y segundo bote, margen al tin, desvío espacial y temporal tras varios contactos. Para repetibilidad, fijar condiciones y semilla. Añadir pruebas con valor real: segundo bote simultáneo con orden de golpe, objetivo inalcanzable, líneas, gestos cancelados y coincidencia de restricciones humano/IA.

**Alternativa:** pocos escenarios deportivos de referencia aceptados por el PO, más pruebas numéricas de borde. Seis partidos a ciegas son una comprobación exploratoria de estilo; no evidencia estadística sólida. No exigir perder jugando bien como prueba suficiente de una IA justa.

### F24 · MEDIO · Las reglas necesitan separar retorno, saque y fase del rally

**Secciones:** §8.4 y §17.1. **Naturaleza:** omisión reglamentaria comprobable.

**Problema:** un retorno puede tocar otra pared antes del frontis; el saque debe llegar directamente al frontis. La descripción del saque no explicita esa excepción ni todos los contactos de línea. Tampoco se precisa reinicio del conteo de botes, alternancia y fin único del punto ante eventos cercanos.

**Consecuencia:** un saque vía pared lateral o un bote sobre línea podría aceptarse por reutilizar la validación general; contar dos veces un evento podría duplicar puntuación.

**Recomendación:** precisar fases saque/retorno y criterios de borde. Las reglas 5.7–5.8 exigen frontis directo, condiciones de pie y bote sin tocar líneas; §6.2 distingue el retorno. Referencia: [World Squash Officiating — Rules of Squash](https://worldsquashofficiating.com/rules-of-squash/). Definir quién puede golpear y cuándo el punto se cierra una sola vez. PAR a 7 debe identificarse como adaptación del juego.

**Alternativa:** mantener simplificaciones de colocación y cuadro derecho automático. No hace falta let/stroke para este prototipo. Que la IA despeje puede preservar lectura y movimiento plausibles aunque no existan colisiones; no debe anunciarse como validación de interferencias reales.

### F25 · MEDIO · Duración, pausas y diversión están demasiado vinculadas a números iniciales

**Secciones:** §5.2–5.4, §16.1–16.3 y §17.2–17.4. **Naturaleza:** deducción numérica incorrecta y riesgos de validación.

**Problema:** a 7 con diferencia de 2, los games sin llegar a 6–6 contienen entre 7 y 12 rallies; desde 6–6 hacen falta al menos 14 en total y no hay máximo. “7–13” no es una cota correcta. Tampoco 3–5 minutos implica por sí solo 15–25 s por rally: sin pausas, 180/12 = 15 s y 300/7 ≈ 43 s. Saques y pausas cambian ese reparto; el número de golpes exige otra medición.

**Consecuencia:** se podría alterar una buena física para forzar duración o culpar al gameplay por una media mal interpretada. El promedio de diez partidos oculta partidos extremos; querer revancha una vez no demuestra profundidad.

**Recomendación:** tratar 7 puntos y 3–5 minutos como hipótesis; medir tiempo activo, espera, mediana y cola de duración. Evaluar si el jugador entiende el error sin abrir el panel, recuerda una decisión táctica útil y quiere revancha en varias sesiones. Dar feedback de contacto, pared/tin y punto desde First Shot, con alternativa visible si juega sin audio o háptica.

**Alternativas:** comparar formatos cortos después de tener rallies satisfactorios. Si la duración se dispara por deuce, revisar formato antes de tocar una física que funciona. Si falta diversión por volea o agencia, permitir volver a ese experimento antes de cerrar First Fun; no convertir el orden M4→M5 en una barrera.

### F26 · BAJO · Algunas etiquetas de catálogo confunden resultado con mecánica nueva

**Secciones:** §7.1–7.2, §10.3 y §15 M5. **Naturaleza:** inconsistencia de nomenclatura comprobable.

**Problema:** nick aparece correctamente como resultado, pero también como contenido por agregar en M5. Un drive bajo ya cubre parte del espacio físico de un kill, y la correspondencia Shot player→Powerhouse puede sugerir potencia donde se quiere riesgo y selección ofensiva.

**Consecuencia:** pequeñas duplicaciones de alcance o balance basadas en nombres.

**Recomendación:** distinguir en el catálogo nuevas intenciones, condiciones de contacto y resultados emergentes. No bloquear un rebote natural porque su nombre figure como futuro. Un nick auténtico puede requerir luego mayor fidelidad; no hace falta un comando “nick”.

**Alternativa:** conservar nombres provisionales sin correspondencia rígida entre arquetipos. No afecta la primera prueba del gesto.

## 8. Propuesta de hitos de validación

No se fijan plazos, tareas de programación ni compromisos de arquitectura. Todas las pruebas jugables siguientes ocurrirían **después de la autorización explícita para desarrollar**. Cada hito debería terminar con una decisión del PO: continuar, ajustar o volver a una pregunta anterior.

| Hito | Pregunta que resuelve | Alcance mínimo propuesto | Evidencia para continuar |
|---|---|---|---|
| Revisión consolidada | ¿Qué está aprobado y qué estamos probando? | Decisiones trazables; contrato de contacto; hipótesis y escenarios deportivos. | Claude responde hallazgos y el PO ratifica decisiones. Todavía sin desarrollo. |
| First Ball | ¿Se lee y se reconoce la pelota en vertical? | Cancha simple, cámara, lanzamientos repetibles, rebotes y panel básico; comparación física acotada. | El PO distingue altura/profundidad en ambos fondos; rebotes estables y costo medido en su iPhone. |
| First Shot | ¿Puedo intentar un golpe y entender su resultado? | Práctica, drive/cruzado/drop, gesto, contacto, feedback e instrumentación de entrada. Energía fija, sin azar como referencia. | Dirección y potencia deliberadas con una mano; timing comprensible; fallos con causa visible. Comparación breve de gestos. |
| First Rally | ¿Hay intercambio, agencia y decisiones que cambian el punto? | Una IA básica, movimiento y T, recuperación manual limitada, reinicio rápido con alimentación neutral. Experimento de volea con los golpes existentes. | Profundidad y cambio de dirección abren espacio; la asistencia no devuelve todo; hay respuesta al dominio del centro. |
| First Match | ¿Puntuación, saque y ritmo sostienen el intercambio? | Saque, fin de punto, marcador, formato provisional, resultado y revancha. | Reglas estables y reinicio inmediato; duración observada sin forzar longitud de rallies. |
| First Fun | ¿Quiero repetir porque aprendo y puedo jugar distinto? | Dos estilos; introducir fatiga y variación de error por separado; comparar dificultades necesarias. | El PO reconoce estilos y contrajuego, comprende errores sin depuración y pide revancha en varias sesiones. Si falla, se reabre la mecánica responsable. |
| Personajes y ampliación | ¿Las capacidades permiten estilos distintos sin sustituir decisiones? | Validación de los cuatro pilares, compensaciones de personajes y nuevos golpes según vacíos observados. | Ventajas y debilidades legibles; cada añadido crea una decisión o respuesta táctica nueva. |

First Fun también se observa desde First Shot; el nombre del hito significa consolidar evidencia, no posponer la diversión hasta el final. Una IA de alimentación neutral no prueba todavía la calidad del saque ni la del rival competitivo.

El panel crece con estos hitos: lanzamiento/reinicio y cámara en First Ball; captura de input y contacto en First Shot; intercepciones y alcance en First Rally; métricas de ritmo en First Match; presets comparables de balance en First Fun. Guardar y exportar configuraciones debe estar listo cuando comiencen comparaciones repetidas, no después de perder los parámetros de una buena sesión.

## 9. Recomendaciones priorizadas

| Prioridad | Acción de revisión | Hallazgos | Resultado esperado |
|---|---|---|---|
| P0 · Antes de aprobar la base | Aclarar aprobaciones y contrato de contacto; separar fallo físico de fallo de orden. | F01, F03 | Decisiones autorizadas e invariantes sin contradicción. |
| P0 · Antes del plan técnico | Acordar separación de hitos, alternativas de gesto, política de intercepción y criterios para elegir física. | F02, F04, F05, F07, F09–F12 | Saber qué se probará y qué evidencia decide, sin elegir todo por anticipado. |
| P1 · Primeras pruebas jugables | Resolver legibilidad, medir input/rendimiento, comparar volea y recuperación. | F06, F08, F22 | Control y cancha legibles en el dispositivo real; táctica evaluable. |
| P1 · Antes de cerrar el rally | Validar errores con el PO, información de IA y reglas; especificar pruebas no tautológicas. | F14, F15, F17, F23, F24 | Resultados explicables y restricciones compartidas. |
| P2 · Una vez satisfactorio el intercambio | Calibrar fatiga, estilos, presets, duración y experiencia de revancha. | F13, F16, F18, F21, F25 | Variación con causalidad y posibilidad de reproducir una buena sesión. |
| P2 · Antes del hito de personajes | Definir efectos observables de los cuatro pilares y mantener la arquitectura proporcional. | F19, F20 | Capacidades diferenciadas sin desplazar la habilidad del usuario. |
| P3 · Edición de claridad | Ajustar catálogo y nombres. | F26 | Alcance sin duplicaciones semánticas. |

La revisión no requiere resolver ahora todos los umbrales de balance ni diseñar red, progresión o monetización. Sí requiere distinguir qué es invariante técnica, qué es valor inicial y qué es juicio de diversión.

## 10. Decisiones que debe tomar el Product Owner

Son temas para evaluar con Claude; no una solicitud de autorización para implementar en este momento.

| Decisión | Recomendación de esta revisión | Cuándo decidir |
|---|---|---|
| ¿Qué aprobaciones históricas se ratifican? | Confirmar procedencia de 7 puntos, 3–5 minutos, 60–70 % y gesto detallado. Mantenerlos como hipótesis si no hay evidencia. | Al consolidar la spec. |
| ¿Qué significa buen timing? | Elegir entre contacto asistido acotado y swing programado; definir cómo fallar voluntariamente o por demora. | Antes del plan de contacto/input. |
| ¿El gesto original es obligatorio o una opción a comparar? | Conservar apuntado al frontis como candidato principal y permitir un contraste pequeño de potencia. | Al consolidar First Shot. |
| ¿Cuándo puede corregirse la posición de espera? | Ventana y zona explícitas; nunca reinterpretar un gesto ya iniciado. | Antes de probar recuperación. |
| ¿Cuándo se valida la volea? | Experimento durante First Rally, antes de declarar validada la profundidad táctica. | Al acordar hitos. |
| ¿Qué simplificaciones deportivas se aceptan? | Ratificar ausencia de interferencia y contacto con cuerpos; decidir tin y validar errores en ambos lados. | Antes de sus pruebas de reglas/error. |
| ¿Cómo se elige la física? | Aceptar escenarios y criterios comparables; la elección técnica se revisa con evidencia. | Criterios antes del plan; elección definitiva tras la prueba autorizada. |
| ¿Qué papel tienen azar y fatiga al inicio? | Baseline reproducible sin azar y energía fija; añadir ambos por separado. | Antes de evaluar calidad del rally. |
| ¿Qué dispositivo y experiencia de rendimiento buscamos? | Identificar modelo/iOS; partir de 60 FPS estables y prueba sostenida. | Antes de fijar versiones y presupuestos. |
| ¿Qué significa validar los cuatro pilares? | Aceptar que First Rally no los valida todos; definir un hito posterior con diferencias observables. | Al aprobar alcance incremental. |
| ¿Qué prevalece si duración y rally entran en tensión? | Conservar primero el intercambio satisfactorio y comparar formatos; la duración es objetivo, no garantía. | Tras medir First Match. |

## 11. Respuesta independiente a las observaciones iniciales

- **Alcance:** coincido parcialmente. El roadmap ya es incremental; el problema es la cantidad de variables incorporadas antes de aislar diversión y contacto, no que todo deba desaparecer del MVP.
- **Física propia:** merece consideración seria por la geometría simple y la predicción compartida. No está suficientemente justificado excluir Rigidbody/PhysX ni atribuir a C# puro ventajas automáticas de portabilidad o red.
- **Volea:** debe probarse antes de cerrar la evaluación táctica; no hace falta convertirla en un sistema de gestos adicional ni adelantar todo el catálogo.
- **Errores direccionales:** necesitan validación contextual y lateral, no una aprobación universal “sí/no”. El PO aporta el modelo deportivo; la implementación futura debe conservar su simetría y causalidad.
- **Recuperación:** el conflicto no se resuelve solo con umbrales de toque. Requiere estados y prioridad de órdenes.
- **Valores aprobados:** existe un problema de trazabilidad respecto del encargo actual. No hay evidencia suficiente para afirmar que Claude inventó una aprobación, ni para asumirla como vigente aquí.

Recomiendo que Claude responda cada hallazgo con **aceptar, discrepar con fundamento o diferir hasta un hito identificado**; después, el PO resuelve las decisiones de producto. Una versión consolidada podrá sustentar el plan de implementación cuando exista autorización explícita.

## 12. Alcance de la verificación realizada

Se leyó el documento completo y se contrastaron afirmaciones sobre predicción física, simulación manual, CCD, timestamps de input, configuración Unity, rendimiento móvil y reglas de saque con las fuentes primarias enlazadas junto a cada afirmación. La documentación de Unity 6.0 e Input System 1.11 se usa para verificar capacidades existentes, no para seleccionar esas versiones para el proyecto.

No se midieron rendimiento, ergonomía, diversión, compatibilidad del conjunto Unity/Xcode ni fidelidad de rebotes: todavía no hay prototipo evaluado. Las pruebas y comparaciones propuestas son hipótesis de trabajo pendientes de autorización y ejecución.

Huella SHA-256 de la especificación leída: `c5ea6adb82ee7a29564132f18a29e09f75e537e5699a05c321b8e4d34d1a8ae9`.
