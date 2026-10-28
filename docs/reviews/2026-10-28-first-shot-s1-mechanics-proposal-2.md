# S1 — segunda propuesta de mecánica: fijación explícita y potencia por distancia

Fecha: 2026-10-28. Rama `firstshot/s1-mechanics-proposal-2`, desde main tras el merge de PR #25. **Propuesta de Codex para decisión del PO, no diseño aprobado ni autorización de implementación.** Se conserva la potencia por distancia; se estudia reemplazar la fijación por quietud y separar el apuntado bajo del recorrido de potencia.

Solo este documento, AGENTS.md y el registro de aprobaciones. Sin código, tests, presets, defaults, spec, propuesta First Shot r2, planes, criterios, bandas ni umbrales modificados; sin suites, check-release, builds, instalaciones, sesiones ni acceso al iPhone. La versión revisada en05a3f77 no hizo análisis nuevo. La actualización autorizada deM2-4 predeclara abajo un cálculo geométrico estático, antes de calcularlo; su JSON derivado quedará **embebido en este mismo Markdown**, para conservar el delta de tres documentos. No se analizan ni reproducen las111trazas para inferir intención o resultados deM2-4. Originales intactos; sin archivoJSON/SVG adicional.

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

Las tres alternativas siguientes necesitan una señal de armado observable distinta de la quietud. **Apoyar, dudar o esperar 150 ms no deben ser órdenes de fijación por sí solos.** La precisión y el recorrido se resuelven explícitamente: un tensor con ancla/área propias, o un cambio de eje con margen suficiente. No se propone quitar el área segura, extender a ciegas yMin, comprimir la escala según el apoyo, introducir aceleración de mira ni retocar cámara/ganancia/umbrales existentes.

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

**Recorrido candidato.** El primer dedo mueve la mira libremente. Un segundo contacto **solo dentro de un mando tensor identificado**, mientras el primero sigue activo y el objetivo es válido, solicita fijar. La mira toma la última posición válida del flujo de apuntado disponible en ese instante; no se predice hacia atrás ni se fuerza la alineación entre eventos atrasados. El segundo dedo establece su propia ancla y controla P por distancia horizontal. Su Ended sería **r final**; si el primero se levanta antes, la propuesta es abortar el armado, no promover otro dedo ni disparar con un objetivo perdido. Debe decidirse el orden determinista ante contactos/Ended coincidentes y muestras tardías antes de un diseño de implementación.

**Apuntado bajo y espacio.** El tensor está fuera del recorrido vertical del dedo que apunta; no depende del apoyo bajo ni exige tensar más abajo. El área de apuntado aún debe demostrar cobertura legal dentro de sus límites; solo añadir el segundo dedo no corrige la cobertura del mapeo relativo actual. El mando y su recorrido deben caber sin invadir frontis ni gestos/controles del sistema. No se presupone que ambas funciones quepan cómodamente en una mano izquierda.

**Ventajas.** No hay espera de quietud ni primer levantamiento obligatorio. El contacto de armado se distingue del apuntado y se ve la mira antes de tensar; potencial de continuidad que **es una hipótesis**, no una ventaja física medida.

**Riesgos/costos.** Ergonomía de dos dedos, o necesidad de dos manos, oclusión y apoyo accidental del pulgar. El segundo dedo hoy no se promueve: esta alternativa cambia deliberadamente esa política para el nuevo control y exige aprobación explícita, dejando A′ y S1-sling intactos. Un segundo dedo fuera del mando no debe armar; resolver si se ignora o invalida antes de código. La precedencia de eventos y O2 importan: una fijación explícita tampoco garantiza disponer de la última intención física si hay muestras en tránsito. No introducir un filtro o retardo nuevo para esconderlo.

**150 ms/P<0,60 y RB6.** Un primer dedo rápido que se detiene 150 ms sin segundo contacto no arma ni carga. Después de un armado accidental, mover el segundo dedo sí puede cargar aunque el primero no lo quisiera; debe evaluarse ese caso. RB6 no se activa solo por la pausa/descenso del primer dedo, pero una activación accidental del tensor es un riesgo nuevo, no un pase automático. Tensión deliberada retenida se trata como §5.

**Cómo se evaluaría.** Primero comprobar en una práctica futura que se pueden sujetar el teléfono, apuntar bajo y recorrer el tensor con la mano declarada; si requiere otra mano, es una condición distinta, no evidencia comparable. B1–B5 como referencia, registrando los dos ids, contacto dentro/fuera del mando, quién terminó primero, r y bloqueos. Añadir observación específica de segundo apoyo accidental y objetivo que cambia antes de la fijación. Ningún estudio de ergonomía se realiza ahora.

### M2-3 — un contacto continuo: gesto lateral de armado y tensado en otro eje

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

## 7. Recomendación de Codex, sin elegir por el PO

**Recomiendo usar M2-1 como referencia para un próximo diseño documental**, si el PO decide reabrir los dos contactos antes descartados: separa de forma más clara el objetivo, la orden de armar y la distancia de potencia con una sola mano y sin inferir quietud ni clasificar un flick. Esa separación facilita comprobar por qué apareció potencia y si cabe el recorrido. **No la recomiendo como ganadora física ya validada:** su costo de dos contactos puede empeorar precisamente «mecánico/pausado».

M2-2 es el contraste pertinente si el PO prioriza continuidad y acepta estudiar dos dedos; la ergonomía puede descartarla antes de código. M2-3 conserva un solo dedo y liberación, pero la orden lateral vuelve a inferir intención y necesita resolver su interferencia con el objetivo; no la priorizaría sin un diseño convincente de esa separación. Cambiar holdMs, hacer Flick menos sensible o fijar todo al soltar reutilizando velocidad de apuntado no son alternativas recomendadas: no atacan a la vez C1–C3.

**Ninguna elección del PO se infiere de esta recomendación.** M1/M2/M3 anteriores, A′ y S1-sling quedan documentados/conservados, sin eliminar código ni cambiar defaults. Tampoco se reabre el control §6.2 C, descartado por ahora. El PO puede rechazar todas estas alternativas; el documento no desbloquea implementación ni otra sesión.

## 8. Evaluación futura propuesta, con autorización propia

No ejecutarla ahora. Antes de una PR de código, el PO debe elegir qué alternativa investigar y aprobar dominio/recorrido, señal de armado, dedo/r, cancelación/N2, riesgo por fases, feedback y cualquier cambio documental de la interacción/plan. Valores y ejemplos negativos deben predeclararse antes del RED; TDD futuro con esqueletos compilables y RED por aserciones, no errores CS. No escribir tests aquí.

Si se autoriza después una sesión: una sola mano declarada por bloque (izquierda como referencia, sin asumir validez diestra), perfil efectivamente vigente antes de instalar si hiciera falta, respaldo P0/Po-1/last-active, repetición temporal0 sin guardar, hash/preset/pendientes0/Jugando verificados **antes de cada bloque**, explicación y familiarización con cuenta atrás completa/armado/cancelación entendidos antes de medir. Las16preguntas originales seguirían por separado y literales; cualquier pregunta adicional necesita quedar predeclarada, sin sustituirlas.

Usar B1–B5 como tareas comparables de referencia: mismos T1–T5, mismas bandas, todos los intentos incluidos fallos/cancelaciones, sin reemplazo. B2 debe buscar realmenteT3; separar en informe si no lo hace. B3 añade la interpretación no ambigua de «No fijó / falló» frente a fijación anticipada e intención de potencia, con instrumento aprobado. B4 permite declarar cancelación voluntaria, accidental e interferencia por separado. B5 registra gesto completo (desde primer apoyo hasta última liberación en M2-1, no solo el tensor), mediana/p90, objetivo, banda y naturalidad sin imponer un tiempo nuevo de aprobado. Conservar extras y práctica separados y registrar orden como posible aprendizaje/fatiga. Si el PO autoriza comparación entre controles, orden/preparación deben fijarse antes; **los111 de4cec622 son referencia histórica, no un grupo control equivalente ni una comparación causal** por sus desviaciones.

Escenarios de riesgo adicionales de cada alternativa se predeclaran aparte de las series: rápido/pausa150/suelta antes de armar, armado accidental, descenso bajo/pausa/continuaciónRB6, tirón mantenido150ms, retorno sin pico, tap/Ended idéntico, segundo dedo fuera de mando, foco/DEV/pausa y lanzamiento/config nuevos entre fases. Expectativas finales solo tras decisión del PO; no se escogen casos para favorecer al candidato.

Restaurar P0 al terminar/abortar y verificar nuevo arranque/hash/cero pendientes y los tres archivos byte a byte. Ante defecto o incomprensión, registrar y detenerse. No aceptar S1 ni iniciar rally/S2 por una sesión favorable. S2–S4/T8 siguen sin autorizar.

## 9. Observaciones C1–C6, solo registro

La comunicación literal del PO/Claude queda en el [registro de aprobaciones](2026-10-25-first-shot-po-approvals.md). C1–C4 sustentan el diagnóstico anterior; C5–C6 no se corrigen en esta rama:

- **C5:** [ParamSection.TrySet](../../Assets/SquashClash/DevPanel/ParamSection.cs) audita cambios de deslizador; posible volumen alto de registros durante un arrastre. No se mide, reduce ni agrupa el registro ahora. Una futura decisión separada debe preservar activation/result y trazabilidad sin atribuir un costo de rendimiento no medido.
- **C6:** [AcceptedB0ShowsCountdownAndAuditsExactPresetHashTimeAndPause](../../Assets/SquashClash/Tests/PlayMode/SlingControlSafetyTests.cs), expresión And/Or de la cuenta atrás con precedencia ambigua según Claude, sin efecto en el resultado revisado. No se modifica ni ejecuta el test. No se reinterpreta la regresión411/411 y194/194 ya aprobada.

SL-S1-04 y los111 registros/restauraciónP0 fueron aprobados técnicamente; la causa de las activaciones originales P2/H17/Escenario sigue sin confirmar y no se atribuye al PO ni a autoarranque. Perfil vigente registrado hasta **2026-11-03T18:29:44Z**; no hay instalación autorizada ni acceso al iPhone. **Entregar PR documental lista para Claude, sin fusionar, y detenerse. S1 y S1-sling no aceptados; S2–S4/T8 no autorizados.**


## 10. Adenda M2-4 — decisión del PO y predeclaración geométrica, 2026-10-28

**Predeclaración: todavía sin resultados calculados en este commit.** Claude revisó PR #26/head05a3f778225922b9526e613fd501358e246a8fb9 como documento correcto y honesto, tresMarkdown/sin cálculos/producto; cierrePR25 verificado. Señaló la omisión del literal de manos de revalidación2; se corrige con la cita y enlace en esta adenda, sin cambiar datos históricos.

### Literales, decisión y alcance

Omisión corregida, [revalidación2](2026-10-25-first-shot-s1-revalidation-2.md): **«mano izquierda y derecha, ambas 2, me imagine juando y use ambas manos»**. Es uso declarado en aquella sesión, no prueba de comodidad con dos pulgares simultáneos ni medición de alcance paraM2-2.

Respuestas delPO, cada pregunta por separado:

- Agarre en un rally real: **«Las 2 manos sostienen pero 1 juega, o puede ser 1 y 1 pero es mas incomodo»**.
- Qué pulgar apunta y cuál da potencia si usa dos: **«Que se pueda elegir»**.
- Qué sacrificaría: **«Fluidez de rally»**. Según la aclaración recibida, **prefiere fluidez antes que precisión**; no interpretar esa frase aislada como prioridad inversa.
- Preferencia inicial sobre dominio: **«tocaría el frontis pero habría que probar. Si es que no quizás ver en un panel mas abajo. Pero tendría que mirarlo primero»**. No es elección final de ubicación.

**Decisión delPO: investigarM2-4, «apuntar al apoyar», como mecánica principal**, propuesta porClaude. M2-1queda como contraste;M2-2/M2-3se conservan documentadas y descartadas por ahora. Sustituye la recomendación inicial de§7 como prioridad vigente; no es diseño aprobado ni autorización deTDD/código.

Actualización posterior delPO en la misma revisión: comprobación informal con la app instalada/P0, sin bloque/datos/medición, sujetando con dos manos y jugando con un pulgar. Literal: **«Si, llego comodo con el pulgar , llego a las 2 esquinas y a veces tapa el dedo otras veces no»**. Se registra como sensación delPO; no validar cobertura de extremos legales/recorrido ni extrapolar al otro pulgar. **A, apoyo directo sobre frontis, pasa a ser referencia principal;B, panel inferior, queda de respaldo y con menos profundidad.** Esta instrucción posterior sustituye la petición inicial de igual profundidad. La ubicación final y cualquier ayuda visual siguen pendientes de prueba autorizada; no se repite la comprobación ni se accede al iPhone.

Observación deClaude: al tirar hacia abajo el dedo se aparta y destapa la mira; la oclusión afecta sobre todo a la precisión del apoyo inicial. Se compararán(i)halo,(ii)lupa desplazada sobre el dedo y(iii)destape sin ayuda, sin elegir por elPO ni fijar tamaños nuevos.

### Predeclaración del único cálculo offline de esta actualización

**Objeto:** geometría nominal estática en1206×2622, no simulación del control ni replay. Salida declarada: bloqueJSON `M2-4-geometry-v1` y dos representaciones estáticas Markdown dentro de§10 de este mismo archivo. No otros archivos. Se commitea esta predeclaración antes de calcular; el commit de resultados citará elSHA de esta predeclaración.

**Entradas existentes, sin ajustarlas:**

1. [CourtCameraRig](../../Assets/SquashClash/Runtime/Camera/CourtCameraRig.cs), [DefaultPreset](../../Assets/SquashClash/Runtime/Resources/DefaultPreset.json), [CourtDimensions](../../Assets/SquashClash/Core/Court/CourtDimensions.cs): cámara fija (3,2;8;14,75), mira a(3,2;0,4;0), FOVvertical90°, frontis z=0/ancho6,40/tin0,48/out4,57. Proyección pinhole analítica con ejeX de pantalla acordeAX-1; no ejecutarUnity ni cambiar cámara.
2. [Fixture existente](../../Assets/SquashClash/Tests/PlayMode/ReticleVisibilityTests.cs):1206×2622 y área segura de referencia(0,102,1206,2334), origen inferior izquierdo. Es fixture, no nueva medición física de insets. [Informe de visibilidad](2026-10-25-first-shot-s1-reticle-visibility.md) como respaldo de procedencia.
3. Área útil registrada de4cec622 en [B2](../validation/evidence/2026-10-28-s1-sl04-B2.json): x0–1206,y690,9841918945312–2436; se cita el valor existente, **sin recorrer gestos ni recalcular estadísticas**. Delimitar franja inferior reservada que contieneDevBar/margen; no inventar su borde visual exacto. [SlingInputArea](../../Assets/SquashClash/Runtime/Shot/GesturePreview.Sling.cs) explica esa reserva. Los esquemas distinguirán área segura, límite de entrada yDevBar contenida debajo.
4. Conversión existenteDPI460reportado: s=25,4/460mm/px, sin regla física ni precisión del dedo medida. Recorrido completo de referencia35mm de [S1-sling](../validation/presets/S1-sling.json), sin cambiarlo ni promoverlo; ganancia relativa existente0,12m/mm. ObjetivosT1–T5 con las coordenadas publicadas en el informe aprobado.

**Cálculos fijados antes de ver resultados:**

- Proyectar las esquinas del frontis suelo→out y la envolvente tin→out, además deT1–T5; origen superior izquierdo en tablas/figuras. Bordes de tin/out no son puntos válidos por el clasificador existente; representar su límite sin escoger unepsilon que altere criterios.
- Obtener longitud enpx de35mm y, paraA, extremo inferior después del tirón desde el apoyo legal más bajo. Comparar con límite útil conocido y con el área segura; declarar cualquier falta de espacio, sin bajar el recorrido ni moverDevBar/cámara para hacerlo caber.
- ParaB, calcular solamente altura máxima disponible para un panel estrictamente bajo el frontis, dejando por debajo del apoyo más bajo el mismo recorrido completo hasta el límite útil. No elegir dimensiones nuevas: si la cota es≤0, documentar inviabilidad bajo esas restricciones; si es>0, es techo geométrico, no tamaño aprobado.
- Derivar sensibilidad localA enmm de pantalla por metro del frontis enT1–T5 y comparación con1/0,12 del control relativo. ParaB usar fórmula por ancho/alto del panel y su cota si existe; no inventar panel ni error del pulgar. Ninguna sensibilidad geométrica se denomina exactitud medida o tasa de acierto.
- Dibujar esquemas estáticos con coordenadas/fuente/límites, pulgar activo de ejemplo y dos manos sosteniendo. **Silueta/oclusión cualitativas, sin diámetro ni área de contacto asumidos**, no mapa de alcance antropométrico. ParaA la mira está en el objetivo del frontis: al apoyar directamente puede coincidir con el dedo; no prometer que ya sea visible fuera de él. Comparar las tres ayudas y sus riesgos de ocultar objetivos.

**Exclusiones:** no intención deM2-4 en las111trazas; sin resultados sintéticos de potencia/acierto/latencia, sin nuevos parámetros/umbrales, sin nuevas capturas ni análisis de píxeles de fotos reducidas. Todos los originales, spec, planes y producto intactos. La geometría nominal no acredita comodidad, precisión ni cobertura táctil real. Los apartados(a)–(g), opciones/riesgos/recomendaciones y evaluación se completarán tras este commit, antes de cualquierTDD futuro no autorizado.
