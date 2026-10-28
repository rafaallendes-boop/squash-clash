# S1 — segunda propuesta de mecánica: M2-4, apuntar al apoyar

**Estado vigente tras revisión de05a3f77:** el PO decide investigar M2-4 como principal;A sobre frontis es referencia principal yB panel queda de respaldo. M2-1 contraste;M2-2/M2-3 descartadas por ahora, conservadas abajo. Dominio/ayuda/cancelación/diseño final pendientes; no autoriza código ni aceptaS1. Literales y cálculos estáticos predeclarados en§10.

Fecha: 2026-10-28. Rama `firstshot/s1-mechanics-proposal-2`, desde main tras el merge de PR #25. **Propuesta de Codex para decisión del PO, no diseño aprobado ni autorización de implementación.** Se conserva la potencia por distancia; se estudia reemplazar la fijación por quietud y separar el apuntado bajo del recorrido de potencia.

Solo este documento, AGENTS.md y el registro de aprobaciones. Sin código, tests, presets, defaults, spec, propuesta First Shot r2, planes, criterios, bandas ni umbrales modificados; sin suites, check-release, builds, instalaciones, sesiones ni acceso al iPhone. La versión revisada en05a3f77 no hizo análisis nuevo. La actualización autorizada de M2-4 predeclara abajo un cálculo geométrico estático, antes de calcularlo; su JSON derivado quedará **embebido en este mismo Markdown**, para conservar el delta de tres documentos. No se analizan ni reproducen las111trazas para inferir intención o resultados de M2-4. Originales intactos; sin archivo JSON/SVG adicional.

## 1. Cierre de PR #25 y fuentes

Claude aprobó como entrega técnica experimental el head **fe78564372ef9f38fbaea5cbc17d1fa5253bdea5**. El PO autorizó fusión condicionada; antes de ejecutarla se comprobaron head local/remoto exactos, PR abierta/no borrador/sin conflictos y ambos árboles limpios. No se añadió ningún commit a la rama aprobada.

**Merge commit: 16b8a0060f6edd86fab92b0ae6d517d11cdd3205**; segundo padre exacto fe78564, primero 956cfafbbfea6fef418fa24e6a3b0911663746be. Main sincronizada con origin/main y limpia. Worktree S1-sling retirado sin `--force`; siete paths ignorados preservados en `Logs/archive-s1-sling-20261028` del checkout principal. Inventarios antes/después idénticos: **32.480 archivos / 8.536.696.847 bytes**, 35.372 entradas incluidas carpetas, sin enlaces simbólicos. Ambos inventarios SHA-256 `37e1cdfc32b8bd3c831e4d83050c9d4685809ffbdeb42690d96b44732a99c4c5`. Se conservaron las ramas remotas. Inventarios y artefactos privados siguen ignorados; no se publican logs, credenciales ni identificadores del dispositivo.

**El cierre no acepta S1 ni S1-sling.** [DefaultPreset.json](../../Assets/SquashClash/Runtime/Resources/DefaultPreset.json) conserva `gesture.control=0` (A′); defaults **45c4e9af5c8f**, [preset S1-sling](../validation/presets/S1-sling.json) **7d1a61c39766** sin promover, 41 valores heredados/proyección **0d39e6016bd8** y SimulationKey **166a96e319c7** intactos. La comprobación de archivos es documental, no una ejecución de las suites existentes.

Fuentes de verdad y alcance de las citas:

- [Spec v0.3 §6.2–§6.4](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md): A′, control §6.2 B y control §6.2 C; no se modifica. El control §6.2 B allí descrito fija por quietud y tensa hacia abajo: **las alternativas de este documento se apartan de esa mecánica** y necesitan decisión expresa para cualquier experimento futuro; no se presentan como cumplimiento literal de §6.2 B.
- [First Shot r2](../superpowers/specs/2026-10-24-first-shot-design.md) y [plan r2](../superpowers/plans/2026-10-25-squash-clash-first-shot.md): se citan, no se actualizan. Una futura autorización debe resolver sus adendas de interacción/C5/T1.2b antes de código, sin modificar spec v0.3 en esta entrega.
- [Diseño PR #24](2026-10-27-first-shot-s1-control-b-design.md), [interacción y adenda D2 de PR #25](2026-10-27-first-shot-s1-sling-prototype-interaction.md): antecedentes de N1, RB6, recorrido, cancelación y contacto primario. D2 queda técnicamente correcto y conservado; no se diagnostica un defecto nuevo suyo.
- [Informe físico aprobado de 4cec622](2026-10-28-first-shot-s1-sl04-session.md), [inventario/restauración](../validation/evidence/2026-10-28-s1-sl04-session-closure.json) y [16 respuestas literales](../validation/evidence/2026-10-28-s1-sl04-po-final-answers.json). Los originales de B0/B1/B2/B3–B5 enlazados allí permanecen intactos.

## 2. Diagnóstico con los 111 gestos existentes

No son 111 intentos equivalentes de una misma tarea: el inventario aprobado separa **31 de práctica B0, 65 formales previstos, 10 adicionales de B1 y 5 preview fuera de bloque**, con 10.754 muestras reales. No se agrupan para una tasa de éxito ni se elige retrospectivamente la tanda más favorable.

| Hecho ya registrado / revisión de Claude | Qué sostiene y qué no |
|---|---|
| B2: medios 9/10 en banda, fuertes 9/10, saturación fuerte 10%; suaves 5/10. El PO responde «si» a intensidades voluntarias, consistencia y máximo. | **C1: conservar potencia por distancia actual**, reducción al volver y tope. Es evidencia favorable de esta pieza; no aprobación conjunta de apuntado/potencia. B2 no buscó T3, según el PO. |
| Claude observa fijación unos 130 ms después del último movimiento en las medianas de los bloques. El PO estima «yo creo que un 30 - 40%» y dice «igual es mecanico, me gustaría algo que fuera natural». | **C2: la pausa del apuntado funciona como orden implícita de fijación.** La cifra del PO es juicio, no tasa medida. Subir holdMs puede prolongar el gesto y bajar su valor puede fijar antes; no proponemos ajustarlo como solución del desacople. |
| B3: 3 abandonos, 7 fijaciones y 0/10 medios; B5: 2 abandonos. Los abandonos de B0/B3/B5 terminan bajo yMin. O1 no aisló el mínimo de muestras con geometría elegible. | **C3: conflicto de recorrido/forma**, no fallo nuevo de D2. Apuntar bajo y tensar hacia abajo comparten dirección y espacio físico; quitar quietud, sin separar también esos recorridos, no basta. |
| B4: 5 ZoneCanceled y «Sin problemas»; el PO responde «no» a cancelaciones accidentales y «si» a interferencia de zona. | Conservar los dos juicios distintos. No asumir que los 12/20/8 mm de cancelación se pueden trasladar sin conflicto a otro eje de potencia. |
| B1/B3: «Se fijó antes de tiempo» incluyó también ausencia de rombo/fallo, incluso tras aclarar. | **C4: no hay tasa objetiva de fijaciones involuntarias deducible de esos botones.** Una tercera respuesta es necesaria para una futura evaluación interpretable; no se añade ahora. |
| B5: duración mediana 1,896 s, p90 nearest-rank 4,297 s; PO: «1 segundo creo maximo» y «si, pero se vería como pausado» en rally. | Métrica y juicio separados. No convertir una duración del prototipo en umbral de aceptación ni asumir que otra mecánica será más rápida sin probarla. |

Estos datos apoyan separar la orden de fijar del silencio y el desplazamiento de potencia del descenso usado para apuntar bajo. **No identifican qué alternativa será natural**, ni prueban que todo abandono se deba exclusivamente al área, ni permiten una comparación física entre alternativas todavía inexistentes.

## 3. Qué se conserva y qué debe decidirse

Se propone conservar la propiedad que recibió evidencia favorable: **P depende de la distancia actual posterior al armado**, con zona muerta, curva/tope, retorno que reduce P y sin reutilizar velocidad de apuntado, recorrido acumulado ni pico retenido. Los 2/35 mm actuales son referencia experimental de PR #25, no valores nuevos ni una decisión de conservarlos para cualquier geometría. No se crea catálogo ni preset.

Las tres alternativas **históricas** M2-1/M2-2/M2-3 siguientes necesitan una señal distinta de la quietud. En ellas apoyar/dudar/esperar no basta. **M2-4 de§10 cambia explícitamente esa propuesta: el Began válido dentro del dominio sí es la orden de fijación**, sin necesitar pausa ni otro movimiento; un apoyo accidental pasa a ser riesgo propio. Esperar150ms no es un evento adicional de armado. La precisión y el recorrido se resuelven explícitamente: un tensor con ancla/área propias, o un cambio de eje con margen suficiente. No se propone quitar el área segura, extender a ciegas yMin, comprimir la escala según el apoyo, introducir aceleración de mira ni retocar cámara/ganancia/umbrales existentes.

Los nombres **M2-1/M2-2/M2-3 son solo alternativas de esta propuesta**. No son selectores implementados, ni método de potencia A/B, ni controles A′/B/C de §6.2. La **potencia por distancia** y el **método de potencia B por velocidad** son conceptos diferentes.

## 4. Alternativas para decisión del PO

### M2-1 — dos contactos sucesivos: apuntar, levantar y tensar en un mando independiente

**Recorrido candidato.** Primer contacto en un área de apuntado definida: mueve la mira, sin quietud ni P. Al levantar tras un apuntado válido queda un objetivo candidato, visible; **esa liberación no es un golpe ni r**. Segundo contacto en un tensor horizontal separado captura el candidato y un origen de distancia nuevo; arrastrar lateralmente carga P y volver reduce. Su Ended es el candidato a **r final**, con el timestamp original de ese contacto, no el del primer levantamiento ni un frame. Antes del segundo contacto, se puede corregir/reemplazar el objetivo; nunca se dispara automáticamente por una espera.

**Apuntado bajo y espacio.** El gesto que coloca la mira no presta su posición física como ancla de potencia: un drop bajo no obliga a empezar el tirón desde y baja. El tensor horizontal debe admitir el recorrido completo dentro del área útil para la mano declarada. El área de apuntado debe permitir todo el frontis legal desde su propio dominio sin exigir salir por abajo; decidir su mapeo relativo/absoluto y cobertura es parte del diseño futuro, **no asumir que el arrastre relativo actual ya lo garantiza**. Ningún mando puede tapar el frontis; la viabilidad de espacio con DevBar/área segura necesita diseño y geometría antes de autorizar código.

**Ventajas.** Señal explícita fácil de auditar; no se fija por dudar; se separan coordenadas de objetivo y recorrido de potencia. Una mano/un dedo por vez. El objetivo ya puede verse antes de tensar y no se mueve durante esa fase.

**Riesgos/costos.** Dos apoyos y una transición pueden reforzar la sensación mecánica y demorar el timing. Objetivo candidato viejo, segundo toque accidental, confusión entre primera y última liberación y falta de ancho útil. No se supone un tiempo de caducidad ni un tap de confirmación nuevo: se deben decidir abandono/reemplazo y N2 por fase antes de implementar. Un candidato se invalida con cambio de lanzamiento/configuración, pausa/foco/DEV; no puede heredarse automáticamente a otro lanzamiento.

**Antecedente de decisión.** Se parece a M1 de PR #23, descartada por ahora por el PO. Se incluye como alternativa de contraste dentro de la nueva propuesta autorizada, **sin considerar revocado ese descarte**; investigarla físicamente exige que el PO decida reabrirla. No equivale a la v0.1 de flick sin congelar: el primer contacto no calcula potencia por velocidad ni golpea al soltar.

**150 ms/P<0,60 y RB6.** Arrastre de apuntado rápido, pausa y primera liberación: ningún Swing/P fuerte; falta el tensor. Un segundo toque sin recorrido debe quedar sin carga. Un tirón deliberado mantenido puede conservar P fuerte y no pasa literalmente la regla original en fase de potencia; véase §5. RB6 pierde su transición automática por pausa y el descenso de apuntado no alimenta el tensor, pero persisten armado/carga accidentales por el segundo toque; no declararlo seguro sin evaluar.

**Cómo se evaluaría.** B1 para objetivo candidato/corrección y comprensión de las dos liberaciones; B2 en el tensor con objetivo confirmado y bandas existentes; B3 para apuntado bajo seguido de potencia media, incluyendo ausencia total de segundo contacto como caso de riesgo; B4 con cancelación inequívoca; B5 para costo de transición, candidatos viejos y ritmo cómodo. Registrar ambos contactos, confirmación, reemplazo y r; no sumar el primer Ended como golpe fallido ni ocultarlo para acortar artificialmente la duración completa.

### M2-2 — segundo dedo en un tensor: fijación al contacto explícito, sin levantar el dedo que apunta

**Estado: descartada por ahora por el PO en esta revisión; se conserva como antecedente.** La omisión sobre manos se corrige en§10: el uso declarado de ambas manos de revalidación2 no demuestra comodidad de dos pulgares simultáneos; el PO ahora dice que dosmanos sostienen y uno juega, y que1y1es más incómodo. «Que se pueda elegir» es preferencia para una posible configuración futura, no implementación de selección de manos.

**Recorrido candidato.** El primer dedo mueve la mira libremente. Un segundo contacto **solo dentro de un mando tensor identificado**, mientras el primero sigue activo y el objetivo es válido, solicita fijar. La mira toma la última posición válida del flujo de apuntado disponible en ese instante; no se predice hacia atrás ni se fuerza la alineación entre eventos atrasados. El segundo dedo establece su propia ancla y controla P por distancia horizontal. Su Ended sería **r final**; si el primero se levanta antes, la propuesta es abortar el armado, no promover otro dedo ni disparar con un objetivo perdido. Debe decidirse el orden determinista ante contactos/Ended coincidentes y muestras tardías antes de un diseño de implementación.

**Apuntado bajo y espacio.** El tensor está fuera del recorrido vertical del dedo que apunta; no depende del apoyo bajo ni exige tensar más abajo. El área de apuntado aún debe demostrar cobertura legal dentro de sus límites; solo añadir el segundo dedo no corrige la cobertura del mapeo relativo actual. El mando y su recorrido deben caber sin invadir frontis ni gestos/controles del sistema. No se presupone que ambas funciones quepan cómodamente en una mano izquierda.

**Ventajas.** No hay espera de quietud ni primer levantamiento obligatorio. El contacto de armado se distingue del apuntado y se ve la mira antes de tensar; potencial de continuidad que **es una hipótesis**, no una ventaja física medida.

**Riesgos/costos.** Ergonomía de dos dedos, o necesidad de dos manos, oclusión y apoyo accidental del pulgar. El segundo dedo hoy no se promueve: esta alternativa cambia deliberadamente esa política para el nuevo control y exige aprobación explícita, dejando A′ y S1-sling intactos. Un segundo dedo fuera del mando no debe armar; resolver si se ignora o invalida antes de código. La precedencia de eventos y O2 importan: una fijación explícita tampoco garantiza disponer de la última intención física si hay muestras en tránsito. No introducir un filtro o retardo nuevo para esconderlo.

**150 ms/P<0,60 y RB6.** Un primer dedo rápido que se detiene 150 ms sin segundo contacto no arma ni carga. Después de un armado accidental, mover el segundo dedo sí puede cargar aunque el primero no lo quisiera; debe evaluarse ese caso. RB6 no se activa solo por la pausa/descenso del primer dedo, pero una activación accidental del tensor es un riesgo nuevo, no un pase automático. Tensión deliberada retenida se trata como §5.

**Cómo se evaluaría.** Primero comprobar en una práctica futura que se pueden sujetar el teléfono, apuntar bajo y recorrer el tensor con la mano declarada; si requiere otra mano, es una condición distinta, no evidencia comparable. B1–B5 como referencia, registrando los dos ids, contacto dentro/fuera del mando, quién terminó primero, r y bloqueos. Añadir observación específica de segundo apoyo accidental y objetivo que cambia antes de la fijación. Ningún estudio de ergonomía se realiza ahora.

### M2-3 — un contacto continuo: gesto lateral de armado y tensado en otro eje

**Estado: descartada por ahora por el PO, conservada como antecedente.**

**Recorrido candidato.** Mantener un único contacto. Tras apuntar, el usuario traza un tramo lateral de armado y cambia de sentido; **la inversión reconocida sería el evento de fijación**, con feedback visible y P desde un ancla nueva. El tramo de armado no carga; el recorrido horizontal después de invertir sí carga, y volver hacia el ancla reduce. El Ended del mismo dedo sería r. **Una pausa no arma.** No se fijan aquí longitud, velocidad ni tolerancias de reconocimiento del tramo/inversión: decidirlas expresamente antes de TDD, sin reutilizar o bajar `aim.flickMmPerS` a escondidas.

**Apuntado bajo y espacio.** La potencia deja de ser continuación del descenso de apuntado; la orden y el tensor necesitan recorrido lateral dentro de un dominio con cobertura legal completa. Cambiar el eje no recupera el contacto si el apuntado ya salió por yMin: se debe resolver el área/mapeo antes del comando. No elegir automáticamente dirección/escala según el lugar de apoyo a mitad del gesto.

**Ventajas.** Un dedo y una sola liberación; ninguna espera deliberada de quietud ni segundo apoyo. Es la alternativa con menos cambios de semántica de r respecto al prototipo.

**Riesgos/costos.** Un arrastre lateral para apuntar puede parecer armado: reaparece el problema de inferir intención por la forma/velocidad. Además, la orden lateral podría mover el objetivo que se quería fijar. Hay dos opciones de diseño pendientes, con costo explícito: fijar la posición al terminar el comando (posible deriva de mira) o recuperar el punto real anterior al comando desde historial (posible salto/retrospectiva). **Ninguna está elegida ni se calcula con las 111 trazas**. Sin resolver esa separación, M2-3 no está lista para implementar y no demuestra solución al acoplamiento. No añadir suavizado para ocultar un salto; si se autorizara, registrarlo y pedir juicio del PO. Invertir lateralmente para reducir/cancelar también puede confundirse con el comando o el tensor.

**150 ms/P<0,60 y RB6.** El caso vertical puro de apuntar abajo/pausar/continuar no debería armar bajo la hipótesis de comando lateral, pero una trayectoria diagonal o una corrección lateral sí podría hacerlo. El arrastre rápido de los casos originales podría contener la forma de armado; por ello **no se presume que pasa 150 ms/P<0,60 ni RB6**. La futura prueba debe incluir esos casos sin reinterpretarlos a favor del candidato y medir toda activación no buscada. Tensión mantenida se trata como §5.

**Cómo se evaluaría.** Antes de cualquier sesión, especificación observable del comando y ejemplos negativos de apuntado horizontal/diagonal. Luego B1 para objetivo antes/después del comando y feedback; B2 para distancia sin contaminación del comando; B3 para descenso con corrección lateral y RB6; B4 para interferencia de cancelación; B5 para naturalidad/ritmo y fallos de reconocimiento. Requiere nuevas entradas del PO para distinguir comando querido/no querido; las trazas existentes no contienen esa intención y no pueden etiquetarse retrospectivamente como tal.

## 5. Regla de riesgo y tensado deliberado: no renombrar un incumplimiento

La regla original **arrastre rápido + pausa inmóvil de 150 ms + soltar, P<0,60** y sus casos permanecen intactos para el **método de potencia B por velocidad de A′**. Ninguna alternativa nueva se declara Pass de esa regla en esta PR. El [diseño aprobado de S1-sling, RB4](2026-10-27-first-shot-s1-sling-prototype-interaction.md) ya distingue que retener distancia deliberada puede conservar P fuerte, sin decaimiento.

| Momento del caso futuro | Expectativa candidata / decisión pendiente |
|---|---|
| Antes de señal explícita de armado, incluyendo silencio y pausa150 ms al apuntar | Sin potencia de distancia ni golpe por la primera liberación de M2-1. M2-2 requiere segundo contacto válido. En M2-3, comprobar que el recorrido no se confunda con su comando. **Hipótesis por verificar**, no resultado medido. |
| Armado sin tirón posterior | No cargar la distancia anterior de apuntado ni la del comando de armado. Resolver tap/cancelación/Ended por fase, sin inventar un Swing o asignarle éxito. |
| Tirón deliberado a distancia fuerte, mantener150 ms y soltar | Puede dar P≥0,60 por distancia actual. **Es incompatible con exigir literalmente P<0,60 a cualquier fase.** Si el PO exige esa lectura global, estas alternativas de distancia retenida no están resueltas; no introducir decaimiento o relajar el umbral por cuenta de Codex. |
| Tirón, retorno a zona muerta y suelta | Evaluar reducción por distancia actual, sin pico retenido. Cancelación tiene política propia que debe aprobarse; no confundir P0 con gesto cancelado. |
| RB6: apuntar abajo, duda/pausa y continuación descendente sin intención de potencia | M2-1 separa contactos y coordenadas; M2-2 separa dedos/mando; M2-3 requiere comando lateral pero puede confundirse. Ninguna pausa sola debe fijar. Medir potencia no querida y declaración del PO, incluyendo ausencia de fijación; no suponer intención por estar después de armado. |

**El PO debe decidir antes de una futura implementación cómo aplicar la reinterpretación por fases a la alternativa elegida**, conservando la regla original y el tratamiento del tensado deliberado de PR #25. No se le solicita decidir aquí por un valor propuesto por Codex ni se cambia RB6/criterios para obtener Pass. Potencia por distancia puede conservar su pieza favorable sin afirmar que toda distancia es intencional.

## 6. Condiciones comunes que no quedan resueltas solo al quitar quietud

- **Área y recorrido completo:** representar antes de implementar el dominio de apuntado (cobertura de T1–T5 y extremos legales), el dominio físico del tensor y el margen de cancelación, para la mano declarada. La mira en un objetivo bajo no debe exigir un dedo fuera del área útil. Si no cabe recorrido, la alternativa es inviable en ese diseño; no hacer que el borde equivalga a100% ni modificar `pullFullMm` para que la prueba pase. Sin tamaños nuevos ni simulación geométrica en esta entrega.
- **Cancelación por fase:** abandono antes de armar, deshacer candidato en M2-1 y cancelación después de armar deben ser distinguibles. La zona actual superior/lateral no se copia automáticamente: puede coincidir con un tensor horizontal o con su retorno. Aprobar ubicación/margen/feedback antes de TDD; no rescatar N1 global de Began, que ya chocaba con el tensado en PR #24. N2 y sus cifras actuales no se cambian: decidir su aplicación a cada contacto/fase explícitamente, sin usar un tap secundario para disparar fuerte.
- **Autoridad e identidad:** mismo flujo de eventos del juego, configuración/lanzamiento capturados y ninguna simulación paralela. No trasladar un candidato a otro lanzamiento. Pausa/foco/DEV/cambio de configuración invalidan las fases, sin disparar al volver. La política de dedo/r difiere entre alternativas y necesita contrato aprobado; no implementar S2/contacto real por haberlo descrito.
- **Feedback y trazabilidad:** armado visible con forma/texto, P y razón de rechazo; ausencia de fijación diferenciada de objetivo inválido/cancelado. Frontis libre. Mantener eventos crudos/origen temporal y medir O2 también con fijación explícita. Una futura tercera respuesta, p.ej. «No fijó / falló», necesita autorización de interfaz; no cambia los antiguos feedbacks mixtos ni permite calcular su intención.

## 7. Recomendación inicial de Codex — histórica, sustituida por la decisión M2-4

**Recomiendo usar M2-1 como referencia para un próximo diseño documental**, si el PO decide reabrir los dos contactos antes descartados: separa de forma más clara el objetivo, la orden de armar y la distancia de potencia con una sola mano y sin inferir quietud ni clasificar un flick. Esa separación facilita comprobar por qué apareció potencia y si cabe el recorrido. **No la recomiendo como ganadora física ya validada:** su costo de dos contactos puede empeorar precisamente «mecánico/pausado».

M2-2 es el contraste pertinente si el PO prioriza continuidad y acepta estudiar dos dedos; la ergonomía puede descartarla antes de código. M2-3 conserva un solo dedo y liberación, pero la orden lateral vuelve a inferir intención y necesita resolver su interferencia con el objetivo; no la priorizaría sin un diseño convincente de esa separación. Cambiar holdMs, hacer Flick menos sensible o fijar todo al soltar reutilizando velocidad de apuntado no son alternativas recomendadas: no atacan a la vez C1–C3.

**La recomendación inicial no eligió por el PO. La decisión posterior explícita es investigar M2-4, conM2-1de contraste (§10); ya no se proponeM2-1como mecánica principal.** M1/M2/M3 anteriores, A′ y S1-sling quedan documentados/conservados, sin eliminar código ni cambiar defaults. Tampoco se reabre el control §6.2 C, descartado por ahora. El PO puede rechazar todas estas alternativas; el documento no desbloquea implementación ni otra sesión.

## 8. Evaluación futura propuesta, con autorización propia

No ejecutarla ahora. Antes de una PR de código, el PO debe aprobar el diseño de M2-4: dominio/recorrido, dedo/r, cancelación/N2, riesgo por fases, feedback y cualquier cambio documental de interacción/plan. La elección deM2-4para investigar ya está comunicada; no se le vuelve a pedir elegir mecánica para poder completar esta documentación. Valores y ejemplos negativos deben predeclararse antes del RED; TDD futuro con esqueletos compilables y RED por aserciones, no errores CS. No escribir tests aquí.

Si se autoriza después una sesión: una sola mano declarada por bloque (izquierda como referencia, sin asumir validez diestra), perfil efectivamente vigente antes de instalar si hiciera falta, respaldo P0/Po-1/last-active, repetición temporal0 sin guardar, hash/preset/pendientes0/Jugando verificados **antes de cada bloque**, explicación y familiarización con cuenta atrás completa/armado/cancelación entendidos antes de medir. Las16preguntas originales seguirían por separado y literales; cualquier pregunta adicional necesita quedar predeclarada, sin sustituirlas.

Usar B1–B5 como tareas comparables de referencia: mismos T1–T5, mismas bandas, todos los intentos incluidos fallos/cancelaciones, sin reemplazo. B2 debe buscar realmente T3; separar en informe si no lo hace. B3 añade la interpretación no ambigua de «No fijó / falló» frente a fijación anticipada e intención de potencia, con instrumento aprobado. B4 permite declarar cancelación voluntaria, accidental e interferencia por separado. B5 registra gesto completo (desde primer apoyo hasta última liberación en M2-1, no solo el tensor), mediana/p90, objetivo, banda y naturalidad sin imponer un tiempo nuevo de aprobado. Conservar extras y práctica separados y registrar orden como posible aprendizaje/fatiga. Si el PO autoriza comparación entre controles, orden/preparación deben fijarse antes; **los111 de4cec622 son referencia histórica, no un grupo control equivalente ni una comparación causal** por sus desviaciones.

Escenarios de riesgo adicionales de cada alternativa se predeclaran aparte de las series: rápido/pausa150/suelta antes de armar, armado accidental, descenso bajo/pausa/continuación RB6, tirón mantenido150ms, retorno sin pico, tap/Ended idéntico, segundo dedo fuera de mando, foco/DEV/pausa y lanzamiento/config nuevos entre fases. Expectativas finales solo tras decisión del PO; no se escogen casos para favorecer al candidato.

Restaurar P0 al terminar/abortar y verificar nuevo arranque/hash/cero pendientes y los tres archivos byte a byte. Ante defecto o incomprensión, registrar y detenerse. No aceptar S1 ni iniciar rally/S2 por una sesión favorable. S2–S4/T8 siguen sin autorizar.

## 9. Observaciones C1–C6, solo registro

La comunicación literal del PO/Claude queda en el [registro de aprobaciones](2026-10-25-first-shot-po-approvals.md). C1–C4 sustentan el diagnóstico anterior; C5–C6 no se corrigen en esta rama:

- **C5:** [ParamSection.TrySet](../../Assets/SquashClash/DevPanel/ParamSection.cs) audita cambios de deslizador; posible volumen alto de registros durante un arrastre. No se mide, reduce ni agrupa el registro ahora. Una futura decisión separada debe preservar activation/result y trazabilidad sin atribuir un costo de rendimiento no medido.
- **C6:** [AcceptedB0ShowsCountdownAndAuditsExactPresetHashTimeAndPause](../../Assets/SquashClash/Tests/PlayMode/SlingControlSafetyTests.cs), expresión And/Or de la cuenta atrás con precedencia ambigua según Claude, sin efecto en el resultado revisado. No se modifica ni ejecuta el test. No se reinterpreta la regresión411/411 y194/194 ya aprobada.

SL-S1-04 y los111 registros/restauraciónP0 fueron aprobados técnicamente; la causa de las activaciones originales P2/H17/Escenario sigue sin confirmar y no se atribuye al PO ni a autoarranque. Perfil vigente registrado hasta **2026-11-03T18:29:44Z**; no hay instalación autorizada ni acceso al iPhone. **Entregar PR documental lista para Claude, sin fusionar, y detenerse. S1 y S1-sling no aceptados; S2–S4/T8 no autorizados.**


## 10. Adenda M2-4 — decisión del PO y predeclaración geométrica, 2026-10-28

**Histórico de la predeclaración en60c5c28:** en ese commit todavía no había resultados calculados. Los resultados publicados abajo se calcularon después de la ampliación9d2a005; esta frase no describe el estado actual del documento. Claude revisó PR #26/head05a3f778225922b9526e613fd501358e246a8fb9 como documento correcto y honesto, tres Markdown/sin cálculos/producto; cierrePR25 verificado. Señaló la omisión del literal de manos de revalidación2; se corrige con la cita y enlace en esta adenda, sin cambiar datos históricos.

### Literales, decisión y alcance

Omisión corregida, [revalidación2](2026-10-25-first-shot-s1-revalidation-2.md): **«mano izquierda y derecha, ambas 2, me imagine juando y use ambas manos»**. Es uso declarado en aquella sesión, no prueba de comodidad con dos pulgares simultáneos ni medición de alcance para M2-2.

Respuestas del PO, cada pregunta por separado:

- Agarre en un rally real: **«Las 2 manos sostienen pero 1 juega, o puede ser 1 y 1 pero es mas incomodo»**.
- Qué pulgar apunta y cuál da potencia si usa dos: **«Que se pueda elegir»**.
- Qué sacrificaría: **«Fluidez de rally»**. Según la aclaración recibida, **prefiere fluidez antes que precisión**; no interpretar esa frase aislada como prioridad inversa.
- Preferencia inicial sobre dominio: **«tocaría el frontis pero habría que probar. Si es que no quizás ver en un panel mas abajo. Pero tendría que mirarlo primero»**. No es elección final de ubicación.

**Decisión del PO: investigar M2-4, «apuntar al apoyar», como mecánica principal**, propuesta porClaude. M2-1 queda como contraste;M2-2/M2-3 se conservan documentadas y descartadas por ahora. Sustituye la recomendación inicial de§7 como prioridad vigente; no es diseño aprobado ni autorización de TDD/código.

Actualización posterior del PO en la misma revisión: comprobación informal con la app instalada/P0, sin bloque/datos/medición, sujetando con dos manos y jugando con un pulgar. Literal: **«Si, llego comodo con el pulgar , llego a las 2 esquinas y a veces tapa el dedo otras veces no»**. Se registra como sensación del PO; no validar cobertura de extremos legales/recorrido ni extrapolar al otro pulgar. **A, apoyo directo sobre frontis, pasa a ser referencia principal;B, panel inferior, queda de respaldo y con menos profundidad.** Esta instrucción posterior sustituye la petición inicial de igual profundidad. La ubicación final y cualquier ayuda visual siguen pendientes de prueba autorizada; no se repite la comprobación ni se accede al iPhone.

Observación deClaude: al tirar hacia abajo el dedo se aparta y destapa la mira; la oclusión afecta sobre todo a la precisión del apoyo inicial. Se compararán(i)halo,(ii)lupa desplazada sobre el dedo y(iii)destape sin ayuda, sin elegir por el PO ni fijar tamaños nuevos.

### Predeclaración del único cálculo offline de esta actualización

**Objeto:** geometría nominal estática en1206×2622, no simulación del control ni replay. Salida declarada: bloque JSON `M2-4-geometry-v1` y dos representaciones estáticas Markdown dentro de§10 de este mismo archivo. No otros archivos. Se commitea esta predeclaración antes de calcular; el commit de resultados citará elSHA de esta predeclaración.

**Entradas existentes, sin ajustarlas:**

1. [CourtCameraRig](../../Assets/SquashClash/Runtime/Camera/CourtCameraRig.cs), [DefaultPreset](../../Assets/SquashClash/Runtime/Resources/DefaultPreset.json), [CourtDimensions](../../Assets/SquashClash/Core/Court/CourtDimensions.cs): cámara fija (3,2;8;14,75), mira a(3,2;0,4;0), FOVvertical90°, frontis z=0/ancho6,40/tin0,48/out4,57. Proyección pinhole analítica con ejeX de pantalla acordeAX-1; no ejecutar Unity ni cambiar cámara.
2. [Fixture existente](../../Assets/SquashClash/Tests/PlayMode/ReticleVisibilityTests.cs):1206×2622 y área segura de referencia(0,102,1206,2334), origen inferior izquierdo. Es fixture, no nueva medición física de insets. [Informe de visibilidad](2026-10-25-first-shot-s1-reticle-visibility.md) como respaldo de procedencia.
3. Área útil registrada de4cec622 en [B2](../validation/evidence/2026-10-28-s1-sl04-B2.json): x0–1206,y690,9841918945312–2436; se cita el valor existente, **sin recorrer gestos ni recalcular estadísticas**. Delimitar franja inferior reservada que contiene DevBar/margen; no inventar su borde visual exacto. [SlingInputArea](../../Assets/SquashClash/Runtime/Shot/GesturePreview.Sling.cs) explica esa reserva. Los esquemas distinguirán área segura, límite de entrada y DevBar contenida debajo.
4. Conversión existenteDPI460reportado: s=25,4/460mm/px, sin regla física ni precisión del dedo medida. Recorrido completo de referencia35mm de [S1-sling](../validation/presets/S1-sling.json), sin cambiarlo ni promoverlo; ganancia relativa existente0,12m/mm. Objetivos T1–T5 con las coordenadas publicadas en el informe aprobado.

**Cálculos fijados antes de ver resultados:**

- Proyectar las esquinas del frontis suelo→out y la envolvente tin→out, además de T1–T5; origen superior izquierdo en tablas/figuras. Bordes de tin/out no son puntos válidos por el clasificador existente; representar su límite sin escoger unepsilon que altere criterios.
- Obtener longitud enpx de35mm y, paraA, extremo inferior después del tirón desde el apoyo legal más bajo. Comparar con límite útil conocido y con el área segura; declarar cualquier falta de espacio, sin bajar el recorrido ni mover DevBar/cámara para hacerlo caber.
- ParaB, calcular solamente altura máxima disponible para un panel estrictamente bajo el frontis, dejando por debajo del apoyo más bajo el mismo recorrido completo hasta el límite útil. No elegir dimensiones nuevas: si la cota es≤0, documentar inviabilidad bajo esas restricciones; si es>0, es techo geométrico, no tamaño aprobado.
- Derivar sensibilidad localA enmm de pantalla por metro del frontis en T1–T5 y comparación con1/0,12 del control relativo. ParaB usar fórmula por ancho/alto del panel y su cota si existe; no inventar panel ni error del pulgar. Ninguna sensibilidad geométrica se denomina exactitud medida o tasa de acierto.
- Dibujar esquemas estáticos con coordenadas/fuente/límites, pulgar activo de ejemplo y dos manos sosteniendo. **Silueta/oclusión cualitativas, sin diámetro ni área de contacto asumidos**, no mapa de alcance antropométrico. ParaA la mira está en el objetivo del frontis: al apoyar directamente puede coincidir con el dedo; no prometer que ya sea visible fuera de él. Comparar las tres ayudas y sus riesgos de ocultar objetivos.

**Exclusiones:** no intención de M2-4 en las111trazas; sin resultados sintéticos de potencia/acierto/latencia, sin nuevos parámetros/umbrales, sin nuevas capturas ni análisis de píxeles de fotos reducidas. Todos los originales, spec, planes y producto intactos. La geometría nominal no acredita comodidad, precisión ni cobertura táctil real. Los apartados(a)–(g), opciones/riesgos/recomendaciones y evaluación se completarán tras este commit, antes de cualquier TDD futuro no autorizado.


**Ampliación predeclarada antes de refinar la cota de DevBar:** el límite útil incluye margen y no es el borde superior de DevBar. Se conservarán ambos límites en los resultados, sin mover ninguno. Como entrada adicional nominal se usa el root UI **389,10×845,95** ya registrado por el fixture1206×2622 en el XMLPlayMode aprobado de4cec622 (SHA-256 del XML en [evidencia técnica](../validation/evidence/2026-10-27-s1-sl04-controls-implementation.json), runplaymode; original archivado/ignorado). `SlingInputArea` añade30unidadesUI al borde superior de DevBar; por inversión de esa misma fórmula se estimará **barTop=usefulBottomTopY+30·2622/845,95** en coordenada superior izquierda. Es inferencia nominal que combina límite nativo y root del fixture redondeado, **no nueva medida del borde de DevBar en el teléfono**.

Se añadirá una segunda cota de recorrido hasta ese borde estimado, manteniendo la cota conservadora hasta el límite útil predeclarada antes. ParaB se informarán las dos cotas de altura máxima, sin escoger tamaño ni rebajar35mm. La cota hasta DevBar será optimista: no incluye silueta del pulgar, separación, línea de estado u otros controles. Se citará el adaptador actual, que filtra Began y no convierte por sí solo cruzaryMin durante el contacto enCanceled; no declarar inviableA únicamente por entrar en el margen reservado. Las políticas de captura/controles de M2-4 requieren diseño y autorización propios. Misma salidaJSONembebida/figuras, sin otros análisis ni archivos.


### Resultados del cálculo geométrico predeclarado (no medición ni replay)

Los resultados publicados en este bloque se calcularon después de9d2a005, ampliación de la predeclaración inicial60c5c28. El XMLde referencia coincide con elSHA publicado de la regresión PlayMode ya aprobada; no se ejecutó el editor. Valores redondeados solo para lectura/figuras; JSON conserva las cifras calculadas. **No son valores nuevos de configuración ni tamaños de UI elegidos.**

```json
{
  "M2-4-geometry-v1": {
    "schema": 1,
    "kind": "PREDECLARED_STATIC_GEOMETRY_NOT_CONTROL_REPLAY",
    "predeclarationCommits": [
      "60c5c288f8cc7888db89b27455f066bbe38b49be",
      "9d2a005e38e1954ddb62c08112fd7aebae89f4c7"
    ],
    "viewportPx": [
      1206,
      2622
    ],
    "coordinateSystem": "top-left origin; x right/y down; nominal analytical float64 camera projection, no Unity execution",
    "cameraPositionM": [
      3.2,
      8,
      14.75
    ],
    "cameraTargetM": [
      3.2,
      0.4,
      0
    ],
    "verticalFovDeg": 90,
    "sourceRootFixtureUIUnits": [
      389.1,
      845.95
    ],
    "sourceFixtureXmlSha256": "fb5e869a0fdaa78633f31637e88fc3d2ef9839cdc26eaa029896d0e61ea4beaf",
    "safeRectXYXY": [
      0,
      186,
      1206,
      2520
    ],
    "usefulRectXYXY": [
      0,
      186,
      1206,
      1931.0158081054688
    ],
    "reservedLowerBandXYXY": [
      0,
      1931.0158081054688,
      1206,
      2520
    ],
    "devBarTopNominalTopYPx": 2024.0000270309372,
    "devBarBottomSafeTopYPx": 2520,
    "devBarTopIsMeasuredOnDevice": false,
    "reportedDpi": 460,
    "reportedMmPerPx": 0.05521739130434782,
    "pullFullMmReference": 35,
    "pullFullPx": 633.8582677165355,
    "frontisFloorOutPolygonPx": [
      [
        317.27919456967635,
        980.0220128432195
      ],
      [
        888.7208054303237,
        980.0220128432195
      ],
      [
        853.0706908703308,
        1338.787148053348
      ],
      [
        352.92930912966915,
        1338.787148053348
      ]
    ],
    "legalTinOutEnvelopePolygonPx": [
      [
        317.27919456967635,
        980.0220128432195
      ],
      [
        888.7208054303237,
        980.0220128432195
      ],
      [
        856.3914397943773,
        1305.3687719432357
      ],
      [
        349.60856020562267,
        1305.3687719432357
      ]
    ],
    "targets": [
      {
        "id": "T1",
        "worldM": [
          4.77,
          1.1355
        ],
        "screenPx": [
          476.3837063285174,
          1258.271697591727
        ],
        "sensitivityMmPerWorldMeterXY": [
          4.453134670805399,
          4.040592952698639
        ]
      },
      {
        "id": "T2",
        "worldM": [
          1.63,
          1.1355
        ],
        "screenPx": [
          729.6162936714827,
          1258.271697591727
        ],
        "sensitivityMmPerWorldMeterXY": [
          4.453134670805399,
          4.040592952698639
        ]
      },
      {
        "id": "T3",
        "worldM": [
          3.2,
          2.525
        ],
        "screenPx": [
          603.0,
          1152.450554167591
        ],
        "sensitivityMmPerWorldMeterXY": [
          4.634581362682968,
          4.3765759083864175
        ]
      },
      {
        "id": "T4",
        "worldM": [
          4.77,
          3.716
        ],
        "screenPx": [
          466.4558137444655,
          1054.634694261855
        ],
        "sensitivityMmPerWorldMeterXY": [
          4.802301759748792,
          4.699074559018837
        ]
      },
      {
        "id": "T5",
        "worldM": [
          1.63,
          3.716
        ],
        "screenPx": [
          739.5441862555346,
          1054.634694261855
        ],
        "sensitivityMmPerWorldMeterXY": [
          4.802301759748792,
          4.699074559018837
        ]
      }
    ],
    "variantA": {
      "lowestLegalEnvelopeAnchorTopYPx": 1305.3687719432357,
      "fullPullEndTopYPx": 1939.2270396597712,
      "clearanceToUsefulBottomPx": -8.211231554302458,
      "clearanceToNominalDevBarPx": 84.77298737116598,
      "clearanceToSafeBottomPx": 580.7729603402288
    },
    "variantB": {
      "panelTopLowerBoundTopYPx": 1338.787148053348,
      "panelHeightUpperBoundBeforeUsefulLimitPx": -41.62960766441461,
      "panelHeightUpperBoundBeforeNominalDevBarPx": 51.35461126105383,
      "lowestAnchorUpperBoundTopYPx": 1390.1417593144017,
      "panelWidthUpperBoundPx": 1206,
      "maxSensitivityMmPerWorldMeterX": 10.40502717391304,
      "optimisticMaxSensitivityMmPerWorldMeterYLegalEnvelope": 0.6933172775756178,
      "optimisticMaxSensitivityMmPerWorldMeterYFullAimRange": 0.5671335330568553,
      "boundsAreNotChosenPanelDimensions": true,
      "boundsExcludeThumbFootprintUIAndSafetyGaps": true
    },
    "relativeGainMPerMmUnchanged": 0.12,
    "relativeMmPerWorldMeter": 8.333333333333334,
    "fingerFootprintAndPlacementErrorMm": null,
    "thumbReachMeasurement": null,
    "M2_4IntentOrOutcomeFrom111Traces": null,
    "originalsUnchanged": true
  }
}
```

**Fórmula reproducible:** cámaraC, direcciónF=normalize(target−C), R=(−1,0,0) y U=F×R. Para q=(x,y,0)−C, f=2622/(2·tan45°), proyección superior izquierda u=603+f·(R·q)/(F·q), v=1311−f·(U·q)/(F·q). Sensibilidades locales:25,4/460 por el módulo de la derivada proyectada por metro, manteniendo la otra coordenada del frontis fija. No mapear el trapecio deA con una interpolación rectangular: usar la inversa de la proyección sobrez=0 capturada al Began. Los límites tin/out son envolventes; no se convierten sus líneas en objetivos válidos ni se modifica el clasificador existente.

| Referencia nominal | Coordenadas de pantalla (px, origen arriba izquierda) |
|---|---|
| T1 — (4.7700;1.1355)m | (476.38;1258.27) |
| T2 — (1.6300;1.1355)m | (729.62;1258.27) |
| T3 — (3.2000;2.5250)m | (603.00;1152.45) |
| T4 — (4.7700;3.7160)m | (466.46;1054.63) |
| T5 — (1.6300;3.7160)m | (739.54;1054.63) |

Frontis suelo→out: x≈317,28–888,72/y980,02–1338,79, con anchura dependiente de altura; no es un rectángulo de apoyo afín. Área segura de referencia:y186–2520. Límite útil de entrada:y1931.02; borde nominal DevBar:y2024.00 hasta2520. El tramo entre ambos pertenece a la reserva/margen, **no se dibuja como si todo fuera botón DevBar**. Estos dos límites se mantienen, sin modificar el producto.

35mm equivalen a633.86px con el DPI reportado. Desde la envolvente del apoyo legal más bajo(y1305.37), el punto del dedo terminaría eny1939.23: 8.21px dentro de la reserva, pero84.77px antes de DevBar nominal. Desde T1/T2 terminaría eny1892.13, antes de ambos límites. **Hay recorrido nominal para el punto enA hasta DevBar; no se acredita espacio para toda la yema ni comodidad real.** El adaptador filtra Began; entrar en el margen durante un contacto activo no equivale por sí mismo aCanceled. Debe decidirse la propiedad del contacto/controles en el diseño futuro, sin extender límites ni quitar protecciones por cuenta de Codex.

EnB, un panel estrictamente bajo el frontis exige inicio≥y1338.79. Dejar35mm por debajo de su apoyo más bajo hasta el límite útil daría altura máxima-41.63px: no cabe un panel positivo bajo esa restricción conservadora. Hasta DevBar nominal la cota optimista es51.35px, antes de descontar yema, separación y estado/protocolo. **Es una cota, no un panel de ese tamaño aprobado.** No afirmar queB es cómodo por estar más abajo ni que es inviable en cualquier futura interfaz: este respaldo necesita otra solución de espacio aprobada, no acortar35mm ni mover DevBar/cámara silenciosamente.

### M2-4 — contrato candidato común (antes de cualquier TDD)

Un **Began válido del contacto primario dentro del dominio** toma la posición absoluta y define el objetivo en metros. Esa es la fijación explícita; no espera quietud, no levanta/reapoya y no clasifica velocidad o forma. No se copia el mínimo de tres muestras/arrastre previo deD2 como requisito para esa transición: sería otra mecánica, no M2-4. D2 sigue intacto en el producto y sus muestras reales siguen siendo evidencia histórica, sin reinterpretarlas como ensayos de M2-4.

El objetivo queda fijo en el frontis, no sigue el dedo al tensar. Potencia por **distancia descendente actual desde el ancla Began**, zona muerta/curva/tope y retorno que reduce, sin velocidad de apuntado, quietud, memoria de pico ni reanclaje silencioso. Ended del mismo contacto primario sería **r**, solo si la clasificación final permite Swing. Cancelación/Tap/SystemCanceled no son contacto real de pelota ni se cuentan como golpe. Es diseño candidato: identidad, cancelación y casos negativos se deciden antes de autorizar aserciones TDD.

#### a) Dominio A principal — apoyo sobre el frontis visible

**Mapeo y cobertura.** Capturar al Began la proyección de cámara/configuración del lanzamiento; invertir el punto de pantalla contra el plano del frontisz=0, respetandoAX-1. La mira representa ese punto real en el frontis. Si cambia la cámara después, no se redefine el objetivo ya fijado; el comportamiento ante un cambio de configuración/foco sigue siendo invalidar el gesto, no reproyectar el mismo dedo a otro objetivo. Los T1–T5 calculados arriba caen en la envolvente. Alcanzar geométricamente esos píxeles y todos los extremos legales no demuestra que el pulgar llegue: el PO declaró llegar cómodo a«las2esquinas», sin precisar cuáles ni medirlas. No extrapolar a todos los extremos, otra orientación, mano o agarre.

El dominio visible incluye regiones inválidas(tin/out). Opciones: rechazar el Began inválido con motivo visible, o mostrar mira roja y cerrar como inválido sin Swing. **Recomendación de Codex:** conservar el señalamiento rojo de intención inválida y una clasificación conservadora explícita, sin recortar a un vecino legal ni convertir la línea en objetivo válido. La política final necesita decisión del PO; no se alteran criterios de legalidad.

**Representación A, estática y nominal.** Viewport1206×2622; números enpx con origen superior izquierdo. El trapecio proyectado es más ancho enout(x≈317–889) que en el suelo(x≈353–853); los trazos siguientes solo indican esa forma cualitativa, no una interpolación de sus bordes. El esquema Markdown no está a escala tipográfica: la tabla/JSON dan las posiciones exactas calculadas. Los símbolos de manos/ω son cualitativos, sin tamaño de yema ni mapa de alcance. Pulgar izquierdo activo solo como ejemplo; el PO no eligió definitivamente un lado.

```text
             x=0                x=603                 x=1206
   y=0       ┌────────────────────────────────────────────┐
             │       banda superior fuera de safeArea     │
   y=186     ├──────────────── área segura ────────────────┤
             │             HUD ya existente               │
             │                                            │
   y=980     │         ┌───────── out ─────────┐          │
             │          \   T4          T5   /           │
   y=1055    │          |                       |          │
   y=1152    │          |           T3          |          │
   y=1258    │          |   (ω:T1)       T2     |          │
             │          |    ↑ pulgar activo    |          │
   y=1305    │          |·· envolvente tin ···· |          │
   y=1339    │           └─── suelo frontis ─┘            │
             │               ↓         ↓                  │
             │  mano izq.───╱ ↓         ↓    mano der.     │
             │  sostiene     ↓         ↓    sostiene      │
             │      [no se dibuja tamaño ni arco real]     │
             │               ↓         ↓                  │
   y=1892    │               ▼         ↓                  │
             │       35mm desde T1    ↓                  │
   y=1931    ├·· límite útil actual de entrada ···········─┤
   y=1939    │                         ▼                  │
             │            35mm desde apoyo legal más bajo │
             │   reserva/margen; NO todo es botón DevBar   │
   y≈2024    ├════════ DevBar: borde nominal estimado ══════┤
             │  Lanzar / Pausa / Paso / Repetir / ...       │
   y=2520    ├──────── fin inferior de área segura ─────────┤
             │              zona inferior sistema         │
   y=2622    └────────────────────────────────────────────┘
```

**Qué tapa el dedo y dónde se ve la mira.** Al tocar directamente T1, el centro de la mira real coincide inicialmente con la zona de apoyo: puede quedar tapado el entorno local de T1. No hay datos de huella para afirmar cuánto, ni que el aro siempre sobresalga. «La mira está en el frontis real» **no significa que quede mágicamente fuera del dedo en ese primer instante**. Al tirar, la mira permanece fija y el dedo baja; se destapa cuando la huella se aparta lo suficiente. Una tensión muy suave o un apoyo sin tirón pueden no destaparla completamente. No mover la mira de intención a un punto falso para hacerla visible.

**Ventajas.** Se elige dónde se quiere jugar en el mismo frontis, sin pausa ni confirmación adicional; elimina el arrastre de apuntado descendente que competía con tensar. Un único contacto/r y el agarre declarado de dos manos/un pulgar. El PO dio sensación favorable de alcance, conservada como tal.

**Riesgos.** Error inicial bajo el dedo no corregible por arrastre; al bajar para«apuntar» se carga potencia porque ya está fijado. Perspectiva/comprensión del mapeo, variación de apoyo en los extremos, retorno accidental a zona muerta y comodidad del recorrido completo. El punto de35mm desde el apoyo legal inferior entra8,21px en la reserva actual, pero queda84,77px antes de DevBar nominal. Esto **no exige mover el límite de entrada**: es un contacto que ya comenzó; tampoco acredita que toda la yema quede despejada o que no intercepteUI. El diseño futuro debe garantizar captura única y que tensar no active controles, sin reutilizar automáticamente esos84,77px como margen de seguridad elegido.

**Recomendación de Codex sobre ubicación:** profundizarA como referencia principal conforme al steering del PO, conservandoB de respaldo. Mostrar primero esta geometría y el costo de cancelar/retocar; la preferencia y la prueba informal no cierran el diseño ni eligen la ayuda de oclusión. La decisión final de ubicación será del PO tras una prueba específicamente autorizada.

#### Ayudas de oclusión A: comparación sin tamaños nuevos ni elección por el PO

Las tres conservan la mira verdadera en el objetivo fijado, no desplazan su centro. No se fija diámetro del pulgar, radio dehalo, aumento/tamaño/offset de lupa ni otro parámetro nuevo. La mira ya tiene mínimos existentes≥60px/6px del proyecto; no garantizan rodear cualquier pulgar. Un halo no cambia el área de impacto ni hace más preciso el Began.

| Ayuda candidata | Ventaja | Riesgo de oclusión/frontis/T1–T5 | Cómo se evaluaría en B1, con autorización propia |
|---|---|---|---|
| **(i) Mira grande/halo alrededor del pulgar** | Si parte del aro queda expuesta, ayuda a reconocer la fijación aun cuando se tapa el centro. | No se conoce la huella: el halo puede quedar tapado entero. Agrandarlo puede invadir objetivo vecino, tin/out u otraT; no afirmar que mejora precisión ni reutilizar el aro como tolerancia de acierto. | Mantener métrica de error del punto Began frente aT, separada de«vi el halo». Registrar dónde/si quedó visible antes del tirón, si tapó la marcaT y si aumentaron cancelaciones/retocados. Comprobar superposición de la vista con T1–T5 y límites; no elegir tamaño favorable después de ver datos. |
| **(ii) Lupa desplazada sobre el dedo durante el apoyo** | Puede mostrar el detalle del punto oculto sin mover la mira de intención real. | Si«sobre»significa encima del contacto, en T1 puede cubrir T3 u otra zona del frontis; en T4/T5 puede cruzarout/HUD. La lupa es una vista auxiliar duplicada y puede exigir otra mirada. Colocarla fuera del frontis/HUD requiere demostrar que cabe; no elegir tamaño/offset ahora ni ocultar el objetivo para enseñar su copia. | Mismo error Began, sin llamarlo corrección por un punto ya fijado. Registrar comprensión, marcas tapadas y tiempo total de apoyo→cancelar/retocar. Cotejar rectángulo auxiliar contra todo el frontis y T1–T5; si no cabe una colocación que no obstruya, esa presentación necesita rediseño/decisión, no garantía ficticia. |
| **(iii) Sin ayuda adicional, confiar en el destape al tirar** | MenosUI y menor necesidad de desviar la mirada; conserva la mira actual y permite confirmar al apartarse el dedo. | No ayuda a la precisión del apoyo inicial; tirón corto/sin tirón puede seguir tapandoT. La yema aún puede cubrir temporalmente una marca vecina; lo que se evita es una oclusión auxiliar nueva. | Medir error Began antes de toda potencia y preguntar si pudo identificarlo al destaparse. Mantener todos los fallos/retocados; no usar el buen aspecto después del tirón como prueba de que el apoyo inicial fue preciso. |

**Recomendación de Codex, pendiente del PO:** usar(iii)como referencia de mínima intervención para una futura comparación de visibilidad, sin dar por suficiente su precisión. (i)es un contraste si el aro existente no resulta reconocible; no autoriza agrandarlo ahora. Reservaría(ii)para una demostración estática de ubicación que pruebe que no tapa frontis/objetivos antes de considerar implementarla. El PO puede preferir otra; **ninguna ayuda está elegida ni autorizada**. La prioridad declarada de fluidez no permite esconder el error inicial ni cambiar criterios.

#### a) Dominio B de respaldo — panel inferior mapeado

**Alternativa, con menor profundidad por instrucción posterior.** Un dominio debajo del frontis mapea posición normalizada a(x,y)en metros; la mira real queda arriba en el frontis y el dedo no tiene que acertar el mismo píxel visual. Al Began se fija y el tirón baja desde ese ancla. No se elige el tamaño/posición del panel ni si mapear solo envolvente legal o el rango completo de mira0–5m: esa elección afectaría errores y acceso a rojo/tin/out y requiere diseño del PO, sin modificar la spec.

**Representación B.** El frontis también es más ancho enout(x≈317–889) que en el suelo(x≈353–853); contorno cualitativo, no escala tipográfica. Rectángulo mostrado como **envolvente optimista/cota**, no panel con dimensiones elegidas; misma pantalla/referencia queA. El máximo de51,35px deja cero margen para yema, separación, estado o protocolos. Antes de la reserva de entrada la cota esnegativa; no dibujar ambas restricciones como si se cumplieran a la vez.

```text
           x=0                  x=603                 x=1206
 y=0       ┌────────────────────────────────────────────┐
 y=186     ├──────────────── área segura ────────────────┤
           │                    HUD                     │
 y=980     │         ┌───────── out ─────────┐          │
           │          \   T4          T5   /           │
 y=1152    │          |          T3          |           │
 y=1258    │          |    T1          T2    |           │
 y=1339    │           └─── suelo frontis ─┘            │
           │···· techo: panel estrictamente debajo ·····│
           │  [ω pulgar / apoyo mapeado, solo cota]      │
 y≤1390    │·· apoyo más bajo posible en cota optimista ·│
           │    ↑manoizq.sostiene     manoder.sostiene↑  │
           │                    ↓                       │
           │             recorrido35mm                 │
 y=1931    ├·· límite útil entrada / inicio reserva ····─┤
           │                    ↓                       │
 y≈2024    ├════════════════════▼ DevBar nominal ════════┤
           │  cero separación en este techo matemático  │
 y=2520    ├──────── fin inferior área segura ───────────┤
 y=2622    └────────────────────────────────────────────┘
```

**Ventajas/riesgos:** separa el objetivo del contacto físico y podría aprovechar un apoyo más bajo, pero«más bajo»no demuestra comodidad. La pequeña cota vertical bajo la geometría actual comprime el mapeo; la yema puede tapar gran parte del panel y, si sobresale hacia arriba, parte baja del frontis/T1–T2. El rectángulo de panel puede estar fuera del frontis sin garantizar que todo el dedo lo esté. El tensor completo también compite con DevBar/margen/estado. No se presentaBcomo solución ya viable.

**Recomendación de Codex:** mantenerBde respaldo sujeto a resolver espacio con una propuesta explícita posterior. No recortar35mm, dar100%al llegar al borde ni mover DevBar/cámara para favorecerla. Evaluación futura: cobertura absoluta de T1–T5/extremos, separación de vista/dedo, error Began, comodidad y margen con toda la yema; los mismos criterios de error/fluidez queA, sin inferir validez por ser un panel familiar.

#### b) Sensibilidad y precisión esperada, sin cambiar ganancia

**Conversión de unidades señalada por Claude de cifras ya publicadas, no análisis nuevo.** Con s=25,4/460mm/px (DPI reportado, sin calibración física nueva):

| Referencia en pantalla | Conversión aproximada comunicada por Claude |
|---|---:|
| Ancho del frontis en el suelo | 27,6mm |
| Ancho del frontis enout | 31,6mm |
| Altura tin→out | 18,0mm |
| Separación T1–T3 | 9,1mm |
| Separación T1–T4 | 11,2mm |
| Separación T1–T2 | 14,0mm |
| Diámetro mínimo de mira60px | 3,3mm |

**Consecuencia señalada por Claude:** el frontis jugable ocupa pocos centímetros en pantalla y la mira actual es más pequeña que una yema. Es una apreciación cualitativa de la escala, **no una medición de la yema delPO ni un tamaño de contacto supuesto para los esquemas**. Los60px son el mínimo existente de la mira, no una nueva medición de su diámetro actual. El riesgo principal deAes **la precisión del apoyo inicial**, no solo que la mira quede oculta: destaparla al tirar no corrige un punto ya fijado con error. No se proponen cambios de cámara/zoom, que siguen fuera de alcance; tampoco se cambian ganancia, tamaños o criterios para reducir artificialmente ese riesgo.

El control relativo existente usa0,12m/mm, equivalente a **8,333mm por metro**. EnA, por perspectiva, las sensibilidades locales calculadas son:

| Apoyo del ejemplo | Horizontalmm/m | Verticalmm/m |
|---|---:|---:|
| T1/T2 | 4,453 | 4,041 |
| T3 | 4,635 | 4,377 |
| T4/T5 | 4,802 | 4,699 |

**Es sensibilidad, no exactitud del pulgar.** Para un error de apoyo desconocidoδmm en un eje, el error local en metros es≈δ/S; para el arrastre relativo seríaδ·0,12. EnA la mismaδde colocación produce más error geométrico por eje que esaδde arrastre relativo, pero son tareas/errores distintos: no predecir tasa de acierto ni afirmar que la precisión real empeorará en esa proporción. Fuera del centro hay acoplamiento proyectivo entre ejes; usar la inversa completa, no dos escalas constantes. ElDPI460reportado no sustituye calibración física ni mide la huella.

EnB, con anchoWp/altoHp enpx, Sx=s·Wp/6,40 y Sy=s·Hp/ΔY. La cota optimista calculada daría **Sy≤0,693mm/m** para la envolvente4,09m o≤0,567mm/m si se cubre0–5m; no es tamaño seleccionado ni precisión conseguida. El ancho podría tener techoSx≤10,405mm/m usando todo el ancho seguro, sin reservar márgenes; no prometer la misma precisión horizontal y vertical. Si no se resuelve el recorrido/altura no hay precisión usable que validar. **Recomendación de Codex:** tratar la geometría deA como hipótesis de apoyo directo y medir error Began; no cambiar `aim.gainMPerMm`, zoom/cámara ni criterios para igualar sensibilidades.

#### c) Corregir un apoyo mal colocado y costo de fluidez

Mover el dedo después de Began **no corrige la mira** en M2-4; hacia abajo cargaP. Corrección candidata: volver a zona muerta,soltar,cancelar yretocar con un Began nuevo. No introducir una ventana temporal de«ajuste inicial»o un modo de arrastre oculto: contradirían la orden de fijar al apoyar y necesitarían otra decisión.

Opciones de clasificación al volver al ancla:

- **C-A:** toda suelta en zona muerta termina cancelada/abandonada según N2 y fase; no haySwingP≈0intencional. Ventaja: corregir es inequívoco y no lanza un golpe al intentar cambiar de objetivo. Riesgo: se pierden los gestos más suaves si al soltar regresan a esa región; reducción y cancelación quedan acopladas.
- **C-B:** cancelar solo si primero se salió de la zona muerta y se volvió; un apoyo sin tirón tendría otro cierre que elPOdebe definir(SwingP≈0oabandono). Ventaja: permite distinguir una intención suave de un retorno. Riesgo: obliga a salir/volver para corregir y puede disparar algo que era un apoyo mal colocado; se parece a un armadoN1, sin autorizar copiarlo.

**Recomendación de Codex:** desarrollarC-Apara el primer diseño por su claridad, conN2prioritario y ceroSwingaccidental por corregir, pero **decisión pendiente del PO** sobreSwingP≈0. Los umbrales actuales no se cambian. Las cancelaciones/reapoyos cuentan y se vinculan como cadena de corrección, sin sustituir el primer fallo por el último acierto. Medir tanto error de cada Began como tiempo/número de apoyos hasta el objetivo que elPOacepta. Ese costo puede erosionar la fluidez que priorizó, aunque ya no exista espera de quietud.

#### d) Desviación normativa y decisiones pendientes

La [specv0.3§6.4](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md) define arrastre relativo, ganancia fija enmm y posición inicial neutra≈2,2m, independiente del movimiento de cámara. M2-4 define objetivo absoluto al Began y, enA, lo obtiene de la proyección actual capturada: **desviación explícita pendiente de aceptación delPOpara un experimento**, no cumplimiento literal ni actualización silenciosa de§6.4. También se aparta de quietud+arrastre del control§6.2B.

Ya está decidido investigarla; aún falta aprobar dominio/mapa/invalidación, presentación de oclusión, cancelación frente aP0, política N2/taps suaves y adaptación de B1–B5. Si luego se autoriza, señalar qué adendas de propuestaFirstShotr2/plan se requieren en una entrega propia; **esta PRno modifica spec/propuesta r2/planes ni convierte la investigación en diseño aprobado**. A′ sigue por defecto yS1-sling se conserva.

#### e) Riesgo por fases y negativos, sin alterar150ms/P<0,60

La regla original permanece paraA′/método de potenciaBpor velocidad. M2-4 se trata como el tensado deliberado retenido de§5/RB4: puede conservarP≥0,60al mantener distancia150ms; **no se llamaPassde la regla original**. Como la fijación ocurre al Began, ya no existe una pausa que separe«apuntar»y«tensar»en el mismo arrastre. Eso elimina esa transición de RB6, pero si elPOsigue intentando apuntar abajo mediante arrastre después del apoyo, **ese descenso carga potencia inmediatamente**. No afirmar queRB6está resuelto por cambiarle el nombre a la fase; debe comprobarse comprensión e intención de cada apoyo/tirón.

| Caso negativo/candidato | Riesgo/opciones que deben aprobarse antes de TDD |
|---|---|
| Apoyo sin tirón o pausa150ms sin desplazamiento | La mira puede estar fijada peroP0. ConC-AnoSwingal soltar; conC-Bqueda pendiente si esabandono oSwingP≈0. No acumularPpor tiempo, jitter o frames. |
| Apoyo accidental dentro del dominio | Alcanza la fijación por definición; no hay detector de intención. N2/cancelación pueden evitar el golpe, pero un desplazamiento posterior suficiente puede producirP. Registrar ese riesgo, no declarar seguridad por cumplir Began. |
| Tap | Conservar los criteriosN2vigentes(<3mm de desplazamiento máximo y<250ms), con prioridad final a definir explícitamente frente a cancelación. Al no existir arrastre de apuntado previo, **un tirón muy suave/rápido puede seguir siendo Tap**: preservar la fórmula de potencia no garantiza conservar todos los suaves dePR25. No bajarN2para lograr bandas. |
| Ended con mismo timestamp/posición que Began o muestra previa | Puede cerrar una sola vez con los datos existentes, sin inventarvelocidad/duración/muestras. Sin tirón confiable,P0y clasificación según N2/cancelación. Si cambia posición con el mismo tiempo, datos contradictorios: salida conservadora con motivo, no potencia fabricada. Contrato final pendiente. |
| Pausa/foco/DEV/relanzamiento/configuración nueva durante el contacto | Invalidar gesto/objetivo/ancla;sinSwingalvolver, sin heredar contacto ni usar elEndedviejo para el lanzamiento nuevo. No presentar la primera fijación visual como golpe confirmado. |
| Segundo dedo/elección de mano | Un solo contacto juega; no promover un dedo secundario ni rearmar al levantar el primero. Mano configurable sería preparación futura, capturada antes del gesto, no selector implementado ahora. |
| Retener tirón o volver arriba | Distancia actual mantiene/reduceP, no pico ni decaimiento nuevo. Zona muerta/cancelación ySwingP0siguen como decisión de(c), sin reutilizar la zona superior/lateral interferente a escondidas. |

**Recomendación de Codex:** predeclarar expectativas N2/cancelación/invalidación junto al diseño antes de las aserciones; no probar parámetros hasta encontrar unPass. Estos son riesgos/documentación propuesta, sin tests ejecutados ni código nuevo.

#### f) Manos, agarre y futura configuración

La postura de referencia es **dos manos sosteniendo y un pulgar jugando**, conforme al literal actual;«a una mano»describe aquí la mano activa, no afirmar que solo una sujeta el teléfono. La sensación informal de alcance es favorable para esa postura, sin identificar de forma medida el lado/recorrido. No usar el uso alternado de ambas manos de revalidación2 para declarar válidoM2-2con dospulgares simultáneos.

Opciones futuras: declarar mano activa por bloque sin cambiar el mapeo central deA, o permitir una preparación de mano que refleje únicamente disposición de ayudas/panel siBse estudiara. **Recomendación de Codex:** registrar qué pulgar juega y mantenerlo durante la comparación, dejando«Que se pueda elegir»como posibilidad de configuración futura. No hay selector nuevo, inferencia por coordenadas del dedo ni reasignación durante el gesto. Cambiar de mano sería otra condición de prueba, no corregir silenciosamente los datos.

#### g) Evaluación futura B1–B5, aún no autorizada

Primero elPOdebe ver estas representaciones y aprobar el diseño/condición que se implementaría. Ni la comprobación informal ni la elección deM2-4autorizan TDD/build/instalación/sesión. El instrumento futuro requiere la tercera respuesta exacta **«No fijó / falló»**, además deOKy«Se fijó antes de tiempo», con significado predeclarado para M2-4: elBeganválido fija inmediatamente por diseño, así que no confundir un apoyo no querido/mal colocado con una espera insuficiente de quietud. Esa adaptación de interfaz/protocolo necesita autorización propia; los antiguos flags mixtos se conservan.

- **B0:** explicación«apoya para fijar, baja para potencia, vuelve para cancelar»según clasificación aprobada; cuenta atrás completa, identificación de fijación/ayuda y cancelación entendidas antes de medir. No inducir arrastre de apuntado previo queM2-4no tiene.
- **B1:** T1–T5, error del objetivo **fijado por Began** frente a marca solicitada; revisión del puntoal destaparse, oclusión inicial y retocados por separado. Comparar(i)/(ii)/(iii)solo con orden/ayuda predeclarados antes de sesión, sin escoger tamaños tras ver errores. La tarea original de«soltar suave sin tensar»puede acabar cancelada porC-A; **debe adaptarse/documentarse con aprobación**, no llamarlo ausencia de fijación ni cambiar criterios para que cuente Swing. No medir error solo tras haber cancelado yreintentado hasta acertar.
- **B2:** objetivoT3realmente buscado,10suaves/10medios/10fuertes de referencia; mantener fallos/taps/cancelaciones. Separar apoyo correcto con banda errada, apoyo errado con banda correcta y cumplimiento conjunto. Comparar con la piezaC1como evidencia histórica con desviaciones, no nueva medición de los111.
- **B3:** T1/T2 bajos, distinguir apoyo mal colocado, arrastre para«corregir»que cargóPnoquerida,Tap, rechazo/no fijó y cancelación. Incluir comprensiónRB6y recorrido completo en extremos según un protocolo aprobado, sin inventar nuevas zonas ni umbrales de éxito. Tercerarespuesta«No fijó / falló»evita llamar anticipación a toda ausencia de rombo.
- **B4:** cancelar desde tirón y desde apoyo erróneo, registrar voluntariedad/accidentalidad/interferencia y si se perdió un suave deseado por volver a zona muerta. No sustituir los cinco intentos ni interpretarP0como prueba suficiente de cancelación.
- **B5:** mismo ritmo cómodo/secuencia de objetivos y bandas de referencia, incluyendo cadenas cancelar→retocar y tiempo completo desde el primer apoyo;ni una pausa fija ni el conteo de contactos se ocultan al informar fluidez. La prioridad de fluidez se juzga con el PO, sin imponer un tiempo nuevo ni simular rally/S2.

Mano/agarre/ayuda/dominio/hash antes de cada bloque; una condición por bloque, sin cambios en medio. Todos los intentos incluidos, extras ypráctica separados,16preguntas originales por separado yliterales. A vsB oayudas distintas no se declaran comparadas físicamente aquí. Perfil efectivo/respaldo/restauraciónP0cotejada si se autorizara una futura instalación/sesión; ninguna ahora. **No simular resultadosM2-4con las111trazas: no contienen ese contrato ni su intención.**

**Cierre documental de esta actualización:** M2-4 principal para investigar;A referencia principal/B respaldo, dominio/ayuda/cancelación final sin elegir por el PO. Recomendaciones de Codex etiquetadas; fuentes/resultadosoriginales intactos,JSON geométrico embebido conforme a predeclaración y dos figuras Markdown, sin cuarto archivo. Sin producto/TDD/suites/check-release/build/instalación/sesión/acceso al iPhone. **PR26lista paraClaude,sin fusionar ydetenerse;S1sin aceptar,S2–S4/T8sin autorizar.** Perfil registrado hasta2026-11-03T18:29:44Z,sin instalación autorizada.
