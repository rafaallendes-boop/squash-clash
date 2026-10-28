# M2-4 «apuntar al apoyar» — diseño documental del experimento S1

Fecha: 2026-10-28. Rama `firstshot/s1-m2-4-design`, base main **bf607cdc235ac381799d5bbbb1a1633ca394dfe9**. **Solo documentación para revisión de Claude. No autoriza implementar, escribir tests, ejecutar suites, construir, instalar ni iniciar una sesión.** Las decisiones del PO se distinguen de las precisiones operativas propuestas por Codex para aprobación. S1 sigue sin aceptar; S2–S4/T8 no autorizados.

Fuentes: [decisiones y cierre de PR #26](2026-10-25-first-shot-po-approvals.md), [propuesta M2-4 §10 y geometría ya revisada](2026-10-28-first-shot-s1-mechanics-proposal-2.md), [spec v0.3 §6.4, intacta](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md), [propuesta First Shot r2](../superpowers/specs/2026-10-24-first-shot-design.md) y [plan r2, adenda nueva D3/Tarea 1.2c](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Las adendas de esta PR son aditivas y para revisión; el texto previo se conserva íntegro.

No se calculan geometría ni resultados nuevos, no se recorre ni reproduce ninguna de las 111 trazas. La geometría/sensibilidades/limitaciones de espacio de PR #26 se citan, no se recalculan. No se crean JSON, SVG, presets ni archivos de producto.

## 1. Decisiones del PO y frontera de aprobación

| Decisión comunicada por el PO | Aplicación al diseño |
|---|---|
| Desviación de §6.4 aceptada solo para M2-4 | Objetivo absoluto al apoyar; spec intacta, A′ por defecto. No se generaliza a juego, A′ ni controles anteriores. |
| C-A | Soltar dentro de zona muerta termina sin Swing; N2 y la fase distinguen Tap/abandono/cancelación. Corregir exige volver, soltar y un Began nuevo, conservando el primer fallo. No existe golpe intencional P≈0 sin tirón. |
| Tin/out | Mira roja, cierre inválido sin Swing, sin recorte al vecino legal ni legalizar líneas. |
| Oclusión (iii) | Primera referencia sin ayuda adicional; medir error del Began. Halo/lupa solo en prueba posterior justificada, orden predeclarado y nueva autorización. |
| Riesgo por fases | Antes de tirar fuera de zona muerta: P0, sin Swing. Distancia deliberada mantenida conserva P incluso≥0,60, sin decaimiento/límite de tiempo. Regla original 150ms/P<0,60 intacta para método B de A′. Apoyo accidental seguido de resbalón descendente es riesgo negativo obligatorio. |
| Dominio | A sobre frontis como referencia principal, B respaldo. Ubicación final después de prueba física autorizada; no elegirla definitivamente aquí. |

**Propuestas operativas de Codex para revisión:** geometría de la zona muerta, precedencia de terminales, significado de feedback, vínculo de reapoyos, **rechazo con motivo de un Began fuera del muro frontal (paredes laterales, suelo o rayo inválido)** y contratos concretos de tests futuros. Este rechazo es una política propuesta por Codex distinta de la mira roja/cierre inválido de tin/out, que sí decidió el PO. No se atribuyen al PO nuevas elecciones de umbral, tamaños, selector ni preset.

### Parámetros de referencia, sin cambios

| Dato existente | Uso documental |
|---|---|
| `power.pullDeadZoneMm=2`, `power.pullFullMm=35` | **[EXPERIMENTO, heredados de S1-sling]**; conservar curva lineal saturada por distancia actual como referencia. No nuevos valores ni promoción a defaults. |
| N2: máximo desplazamiento<3mm y duración<250ms | Criterios vigentes, inalterados; igualdad en cualquiera de los límites no es Tap por esa condición. |
| Bandas suave P<0,25; media 0,25≤P<0,60; fuerte P≥0,60 | Instrumentos existentes, sin umbrales de aceptación nuevos; advertencia de saturación>20% intacta. |
| Ganancia relativa `aim.gainMPerMm=0,12` | Intacta en A′/S1-sling; no se usa como conversión absoluta de M2-4. |
| Defaults 45c4e9af5c8f, S1-sling 7d1a61c39766 | Intactos; no se crea preset M2-4 ni se asigna valor numérico de selector en esta PR. |

La prioridad de fluidez es la declarada por el PO; no justifica cambiar N2, bandas o criterios para producir más Swing s.

## 2. Began→objetivo en el dominio A

### Captura única

En el primer Began aceptado del espacio de juego se capturan una vez: id interno de contacto/gesto, lanzamiento/recepción y versión, configuración efectiva inmutable, dominio/control, escala mm/px y procedencia de DPI, viewport/área segura, matrices y pose de cámara usadas por la vista, marco del frontis y objetivo pedido del protocolo. Ni UI ni panel se consultan después para reinterpretar ese gesto.

**Instante de proyección:** la instantánea disponible al procesar Began, con su tiempo observado/frame, no una cámara reconstruida al timestamp físico del evento. Registrar ambos instantes. No prometer que la vista coincide exactamente con el fotón que vio el PO; no crear historial/resimulación de cámara ni cálculo retroactivo para resolverlo. Primera referencia con cámara/configuración existente; sin zoom o cambio de cámara para aumentar el frontis.

### Mapeo absoluto propuesto para A

Convertir la coordenada táctil al viewport de esa instantánea, obtener el rayo de vista e intersectarlo analíticamente con el plano frontal z=0. El punto resultante (x,y) en metros es la intención y queda fijo. Respetar AX-1: +X del mundo se ve hacia la izquierda; no usar interpolación afín sobre un rectángulo cuando la proyección es un trapecio. No ejecutar una consulta contra la simulación de pelota para mapear el dedo.

El dominio A de referencia es la proyección visible del muro frontal delimitado por la geometría existente de la cancha, descontando UI/área no admisible al Began. **Debe incluir la posibilidad de apuntar a tin/out visibles**: un dominio recortado solo a lo legal haría imposible la decisión de mostrar intención inválida. Usar límites geométricos existentes del muro, no agrandar la zona legal; no aplicar el clamp del arrastre relativo para convertir una intención fuera en una legal. Contorno definitivo de entrada y convivencia con HUD requieren revisión del diseño, sin coordenadas nuevas elegidas aquí.

Rayo sin intersección frontal válida, punto no finito o apoyo fuera del muro/UI admisible: rechazo con motivo, sin inventar objetivo ni Swing. Punto mapeable sobre tin/out: **objetivo inválido rojo**, retenido hasta el cierre, sin Swing. La legalidad visual se basa en las condiciones/clasificador actuales de [AimReticleView](../../Assets/SquashClash/Runtime/Shot/AimReticleView.cs) y [CourtLines](../../Assets/SquashClash/Core/Court/CourtLines.cs); no tomar el diámetro gráfico de la mira como tolerancia de acierto ni cambiar el radio/epsilon del clasificador.

Un Began válido fija inmediatamente, con su posición cruda como ancla táctil; no hay quietud, arrastre previo≥3mm ni mínimo 3 de D2 para fijar. D2 continúa intacto en el control anterior. No fabricar samples para acreditar una fijación; el soporte es el Began real. El objetivo no cambia con Moved/Stationary/Ended ni al moverse luego la cámara; un cambio de contexto invalida, no reancla.

### Límites que siguen abiertos

La sensación del PO de llegar cómodo no mide la yema ni garantiza precisión en un frontis de pocos centímetros. Las holguras/cotas de PR #26 son nominales, no permiso para invadir DevBar ni prueba física. Para el futuro diseño de entrada hay que mantener propietario del contacto y protección de controles al tensar; no mover límites, reducir 35mm o adaptar la escala al borde para hacerlo caber. B sigue respaldo, sin implementar ni probar su panel aquí.

## 3. Máquina de gesto y potencia

Nombres siguientes son **semánticos propuestos**, no enum/clases implementados:

| Estado | Entrada y comportamiento | Salida |
|---|---|---|
| Sin contacto | No hay gesto vigente. Un Began de UI es control de UI, no trial de golpe. | Began de juego aceptado→FijadaSinTirón; objetivo tin/out→Inválida; otro rechazo→registro de rechazo. |
| FijadaSinTirón | Mira fija en objetivo; ancla en Began; P0 mientras la distancia descendente actual permanezca en zona muerta. | Salir de zona muerta→Tensando; Ended→clasificación de §4; contexto perdido→SystemCanceled. |
| Tensando | P por distancia descendente **actual** desde el ancla; lateral no suma, subir reduce, sin retener pico. | Volver a zona muerta→RetornoSinPotencia; Ended→§4; pérdida de contexto→SystemCanceled. |
| RetornoSinPotencia | P0, mostrar que soltar termina sin golpe; historial de salida de zona solo distingue el motivo de cancelación. | Volver a tirar antes de Ended permite P actual; no hay latch que condene todo el gesto a cancelarse. |
| Inválida | Mira roja en intención inicial, sin potencia utilizable ni posibilidad de volverse legal moviendo el dedo después. | Ended→InvalidAim; contexto perdido→SystemCanceled. |
| Terminal | Un resultado definitivo con id; sin segundo disparo, reanclaje ni promoción de otro dedo. | Solo un nuevo Began puede crear otro gesto. |

Propuesta coherente con C-A: **zona muerta es la del propio cálculo descendente**, d=max(0, desplazamiento hacia abajo desde ancla en mm), con d≤deadZone dentro y d>deadZone fuera. No es el círculo N1 de 6mm ni la zona superior/lateral 12/20/8 de S1-sling; esos contratos se conservan en sus controles. Un desplazamiento lateral o hacia arriba sin tirón descendente no puede acabar en SwingP0 por quedar fuera de un círculo distinto. Esta interpretación geométrica se somete a aprobación del diseño, sin umbral nuevo.

La potencia es la misma función de distancia actual con zona muerta, curva y tope de referencia; d_full no cambia según apoyo o espacio. No se incorpora al cálculo el error del objetivo, el tiempo inmóvil ni la distancia máxima alcanzada. `everOutsideDeadZone` solo describe una transición real para el motivo de cierre; **no es potencia de pico ni declaración de intención**. Un Ended con posición nueva válida también puede aportar la distancia final, sin exigir un Moved artificial previo.

### «Deliberado» no es un observable automático

La regla del PO describe intención humana. El control solo observa el apoyo y si d sale de zona muerta. Un resbalón puede producir exactamente esos eventos; **no existe un flag computable que demuestre voluntad**. No se añade quietud, velocidad, confirmación o tiempo mínimo para fingir esa distinción. La fase técnica Tensando significa «distancia fuera de zona muerta», no «el PO quiso pegar fuerte».

Temblor que permanece en zona muerta: P0/sin Swing. Si cruza la zona, debe quedar documentado como el riesgo de carga no querida, no renombrarlo como tirón deliberado para declarar seguridad. La revisión debe aceptar este límite del experimento antes de autorizar implementación; el caso accidental+resbalón no se oculta con P0 artificial ni se considera resuelto por pasar los casos de inmovilidad.

## 4. Prioridad de cierres: sistema, invalidez, N2 y C-A

**Orden operativo propuesto para aprobación; una sola resolución por contacto.** Conservar además flags diagnósticos que habrían satisfecho otra condición, sin emitir dos outcomes:

1. **SystemCanceled/contexto o datos contradictorios:** pausa, foco, DEV, nuevo lanzamiento/recepción, cambio de configuración/control/viewport o pérdida del contacto. Ningún Swing ni Ended ficticio. El motivo se distingue de cancelación voluntaria.
2. **Objetivo inválido tin/out:** en Ended, cierre InvalidAim sin Swing aunque también se cumpla N2 o se haya vuelto al ancla. Mira roja persistente; no recorte legal. Los candidatos de Tap/C-A pueden registrarse como diagnóstico, sin ocultar InvalidAim. Rechazos sin objetivo se registran aparte.
3. **N2/Tap de objetivo válido:** máximo desplazamiento euclídeo de todo el contacto<3mm y duración no negativa<250ms. Prioritario sobre abandono/cancelación C-A; mismo criterio de A′, sin ajustar sus fronteras.
4. **C-A de objetivo válido en zona muerta al Ended:** si nunca salió, AbandonedNoPull; si salió y volvió, CanceledReturnToDeadZone. Ambos sin Swing y sin consumo de intento de gameplay. No existe aquí un Swing intencional P≈0 sin tirón.
5. **Distancia actual fuera de zona muerta, objetivo/contacto/datos válidos:** Swing del gesto con objetivo y P finales inmutables; solo preview/registro en S1, nunca impulso de pelota. El paso por zona muerta antes no cancela si se volvió a salir antes del Ended.

Una vuelta rápida tras salir poco puede seguir siendo Tap por N2. Un suave con distancia apenas fuera de zona muerta pero<3mm, soltado antes de 250ms, también puede ser Tap. **Quitar el arrastre previo cambia cuántos suaves cumplen N2 aunque la curva de potencia se conserve.** Es un costo/riesgo a medir sin bajar 3mm/250ms para hacer pasar bandas. Igualdad con cualquiera de los límites N2 queda fuera de Tap; igualdad con deadZone sigue dentro de C-A. No crear tolerancia numérica nueva para favorecer un resultado.

## 5. Contacto primario, r y casos temporales

Un contacto primario por lanzamiento/configuración, capturado al Began. Began duplicado mientras sigue activo no reinicia objetivo/ancla; dedo secundario no se promueve al levantar el primero. Eventos secundarios/UI se distinguen y preservan por la ruta de captura existente, sin otra fuente de TouchSample. Mano activa declarada por bloque; dos manos sostienen y un pulgar juega como postura de referencia, sin selector de mano nuevo.

Para Swing, **r es el timestamp original del Ended de ese mismo contacto**, nunca `now` del frame ni el instante de fijación. En otros terminales se guarda el Ended original si existe, pero no se emite un r de golpe válido. SystemCanceled registra su instante/motivo y no fabrica Ended. No se integra r con SimClock/contacto retroactivo/P2: corresponde a S2, no autorizado.

Ended con mismo timestamp **y posición** que la última muestra válida cierra exactamente una vez: conserva distancia/P fiables que ya existían, sin tiempo artificial ni muestras nuevas. Began+Ended idénticos sin desplazamiento termina sin Swing según N2/C-A. Si el timestamp se repite pero cambia la posición, o retrocede, no inventar velocidad/orden/distancia fiables: salida conservadora con motivo, sin Swing. Un duplicado después del terminal no crea otro trial.

## 6. Feedback sin ayuda adicional y tercera respuesta propuesta

Primera referencia: mira actual en el frontis, forma/estado de fijación y P, sin halo nuevo, lupa, zoom, sonido ni háptica. Válida fijada: rombo/amarillo; tin/out: rojo con texto de invalidez, no tapado por un destello que aparente legalidad. Mantener los mínimos existentes de la vista sin agrandarlos para mejorar el ensayo. La huella puede tapar el Began; tirar destapa, pero no corrige el error de apoyo ya fijado.

Antes del primer Began no se presenta el punto neutro de A′ como intención ya elegida de M2-4. **Propuesta de vista:** al terminar, conservar el último objetivo con etiqueta de intento terminado, sin simular contacto activo; en B1/B3 queda visible hasta responder o interrumpir el bloque, para poder juzgar el punto al levantar el pulgar. Un nuevo Began de juego reemplaza esa intención terminal, no hereda ancla/P. No se añade un tiempo de retención, halo/lupa, cambio de tamaño ni ayuda de precisión; es la vista del resultado existente, diferenciada de una fijación nueva. A′ no cambia.

Franja compacta fuera del frontis: control/preset/hash y Listo/Pausa/pendientes; FijadaP0 («soltar abandona»), TensandoP y retorno («suelta para cancelar»), Inválida o SystemCanceled con motivo. **Ayuda de cancelación en lenguaje llano, propuesta:** «Para cancelar, vuelve a la altura donde apoyaste, o más arriba, y suelta». Con la zona muerta descendente, eso cumple d≤zona muerta; también queda dentro un pequeño descenso que no la supere. No se exige volver a un círculo ni salir por arriba/lateral hasta otra región. El mensaje final distingue Tap, abandono sin tirón, cancelación por retorno, inválido y Swing preview. No llamar «No fijó» a toda cancelación correcta. Protocolos con temporizador/fin/interrupción y protección SL-S1-04 ampliada al futuro modo: Escenario/P2/H17 no se activan mientras está preparado o en bloque; DEV invalida contacto y muestra rechazo/remedio al armar si falta precondición.

**Propuesta de instrumento para revisión, no UI autorizada:** en B1/B3, tres respuestas vinculadas al intento terminado, fuera del frontis y sin generar otro gesto:

| Respuesta | Significado predeclarado en M2-4 |
|---|---|
| OK | Se vio la fijación esperada. No significa precisión, banda conseguida ni Swing; un Tap/C-A planeado de B1 puede responderse OK. El error del Began se registra por separado. |
| **Apoyo sin querer** — texto visible propuesto | El PO declara apoyo/fijación no queridos, antes de su intención de elegir ese punto. Conservar la correspondencia interna con la respuesta histórica «Se fijó antes de tiempo» (`premature=true`), sin renombrar ni reinterpretar los datos antiguos. En M2-4 no mide un temporizador «demasiado rápido»: el Began fija por diseño. No usar para mero error de coordenadas ni ausencia de rombo. El PO aprobará el texto al autorizar implementación. |
| **No fijó / falló** | El PO no pudo identificar/obtener la fijación que esperaba o hubo fallo operativo/visual de ese intento. Registrar el motivo objetivo disponible aparte (rechazo, inválido, contexto, ausencia de indicador), sin convertir el juicio en diagnóstico. No usar solo por P errada, mira lejos o cancelación planeada. |

**Antecedente que no se corrige retrospectivamente:** en B1/B3 de S1-sling «Se fijó antes de tiempo» se usó de forma mixta, también para intentos sin fijación/fallidos, incluso tras aclararlo. El cambio de texto propuesto no convierte esos flags en una tasa de apoyos involuntarios. La tercera respuesta requiere un discriminador propio si se implementa; la correspondencia con el bool histórico no la hace representable por `Respond(bool)` sin cambios autorizados.

Las respuestas no reclasifican raw ni prueban intención; son señalamiento del PO. Antes de B1 se debe comprobar comprensión con ejemplos de las tres categorías. Si vuelve a mezclarlas, conservarlas y declarar interpretación no evaluable; no reetiquetar ni calcular una tasa favorable. No avanzar sin respuesta en B1/B3; un intento sin ella sigue pendiente, nunca OK inferido. Los 16 juicios finales originales siguen por separado/literales, sin aceptar S1 por ellos.

## 7. Trazabilidad, O2 y cancelar→reapoyar

Propuesta de campos del experimento, **no schema implementado**:

- Identidad: `attemptId`, `control=M2-4`, versión de gesto, lanzamiento/configuración/recepción, hash efectivo y capturado, preset, pendientes, bloque/slot/objetivo pedido, mano activa y condición sin ayuda. El nuevo hash/selector/preset se asignarían solo si se autoriza código; no fingir que 7d1a61c39766 ya identifica M2-4.
- Captura: punto Began crudo, timeReal/receivedReal/sequence, frame/instante observado de fijación, `lockOrigin=Began`, matrices/viewport/rect de dominio, pose/court y escala mm/px/procedencia DPI/fallback/physicallyVerified. Snapshot una vez y versión; sin datos personales del dispositivo.
- Resultado: objetivo en metros fijado por Began, legalidad/motivo e indicadores de error continuo frente a T; estados/transiciones/timestamps, d actual/máxima solo diagnóstico/lateral, P/banda, N2 candidato/máximo desplazamiento/duración, historia de salida/retorno de zona muerta, outcome y motivo único, Ended/r si procede. Duración completa, separación entre apoyos, márgenes relativos al área/DevBar con referencia explícita; no llamar al límite útil borde físico.
- Crudos y contexto: todas las muestras reales, tiempos de evento/recepción, campos de evento de UI/rechazo/contexto y transiciones. Sin remuestrear para rellenar silencio ni aplicar a las 111 trazas un contrato que no tenían. Persistencia por la ruta de métricas existente, fuera del cálculo del gesto; no prometer rendimiento sin medirlo en una futura entrega autorizada.

**O2 adaptado a fijación por Began:** para cada fijación conservar/contar por separado las muestras del contacto primario con `timeReal < lockObservedAt` que se recibieron con `receivedReal > lockObservedAt`, sus fases, secuencia y desplazamiento respecto al ancla Began enpx/mm reportados. Conservar ambos tiempos, no sustituirlos por orden de frame. Una muestra en tránsito puede cambiar P posterior; no debe mover el objetivo ya capturado. Registrar qué matriz se usó y cuándo, sin declarar que refleja el instante físico del toque. O1 de candidatos de quietud no aplica a M2-4; queda histórico, no se reporta como cero fallos del nuevo control.

**Vínculo de reapoyos propuesto:** guardar `predecessorCanceledAttemptId` y `chainId` en el siguiente Began de la misma recepción/control/configuración/contexto después de C-A, con el tipo de cierre previo y los objetivos pedidos antes/después. Sin ventana temporal nueva ni inferir intención por cercanía. Si cambia lanzamiento/configuración/contexto, cerrar cadena y no heredar candidato/ancla. Cada Began sigue siendo otro intento con otro error/outcome; nunca reemplaza al primero.

La relación temporal cancelación→nuevo apoyo no prueba por sí sola que se corrigió el mismo objetivo. En B1 la suelta sin tirón es planeada; marcarlo y no contarlo como corrección accidental. Si el protocolo cambia deT, conservar el enlace previo pero cerrar la cadena de corrección de esa tarea; no llamar al nuevoT «acierto corregido» del anterior. Cuando el objetivo/contexto permanece, informar todos los intentos enlazados y sus tiempos, además del primer intento. La declaración del PO identifica corrección querida cuando haga falta, no un algoritmo de intención.

## 8. Casos negativos predeclarados para tests futuros

No se ejecutan, simulan ni rellenan con cifras nuevas en esta PR. Entradas se parametrizan con los valores heredados de §1; casos en frontera usan la frontera existente, sin epsilon nuevo. Los 150ms son el caso solicitado, no un temporizador de armado. Los datos concretos adicionales, si una implementación los necesita, se predeclararán antes de su RED y cualquier valor experimental nuevo se marcará **[EXPERIMENTO]**.

| ID | Secuencia prevista | Expectativa/documentación de riesgo |
|---|---|---|
| M24-N01 | Began válido, sin tirar, pausa 150ms y Ended en ancla | Objetivo fijado, P0, nunca Swing. Si la duración total es150ms sin desplazamiento, Tap por N2; otra espera sin tirón que no cumpla N2 termina en abandono C-A. No registrar espera como error de fijación. |
| M24-N02 | Tap y fronteras exactas de 3mm/250ms | N2 prioritario sobre C-A en objetivo válido; igualdades no satisfacen el respectivo «<». Sin cambiar criterios. |
| M24-N03 | Temblor contenido en zona muerta, incluida su frontera | P0/no Swing; registrar desplazamiento máximo/N2/motivo. Si sale de zona, no llamarlo automáticamente deliberado: casoN04. |
| M24-N04 | Apoyo accidental dentro de dominio seguido de resbalón descendente fuera de zona y suelta | **Riesgo abierto:** puede cargar P y producir Swing sin voluntad. Test futuro debe conservar esa secuencia/resultado, no forzar P0 ni etiquetarla segura. También comprobar la variante que vuelve y cancela. No añade filtro de intención. |
| M24-N04b — variante | Deslizamiento mayormente lateral, componente descendente apenas mayor que zona muerta y desplazamiento máximo euclídeo desde el ancla≥3mm; Ended fuera de zona | Puede terminar en **Swing con P positivo muy pequeño (P≈0), no buscado**: no cumple Tap por desplazamiento y está fuera de C-A por la componente descendente. No es P=0 sin tirón. Registrar el riesgo, sin umbral lateral/angular, filtro de intención ni valor nuevo. «Total» aquí no es recorrido acumulado: se usa el máximo desplazamiento de N2 vigente. |
| M24-N05 | Tirón a distancia fuerte, mantener 150ms y Ended fuera de zona | Mantener P por distancia, incluso≥0,60, sin decaimiento/límite de espera. No Pass de la regla original por velocidad. |
| M24-N06 | Tirar, volver a zona muerta, Ended allí; variante volver a salir antes de Ended | C-A/N2 sin Swing al soltar dentro; al salir otra vez puede Swing por distancia actual. Nada de pico retenido ni latch de cancelación irreversible. |
| M24-N07 | Ended mismo tiempo/posición que muestra anterior; Began+Ended idénticos | Un terminal, sin anular P fiable ya calculada ni inventar muestras. Sin tirón, no Swing. Ended mismo tiempo/diferente posición o tiempo hacia atrás→conservador sin Swing. |
| M24-N08 | Began sobre tin, línea/out, luego arrastre y Ended | Rojo/cierre inválido sin Swing, aunque se cumpla N2/C-A; nunca clamp legal. Clasificador/fronteras actuales intactos. |
| M24-N09 | Cancelar y reapoyar, mismo contexto; variantes nuevoT/nueva config/lanzamiento | Id/error/outcome original preservado; enlace/cadena explícitos, sin heredar objetivo/ancla ni sustituir fallo. Romper cadena cuando cambia contexto. |
| M24-N10 | Pausa/foco/DEV/relanzamiento/config nueva entre Began, tirón y Ended | Invalida antes de cualquier Swing; no golpear al volver ni reinterpretar Ended viejo con nueva configuración. |
| M24-N11 | Segundo dedo, Began duplicado, Ended duplicado y contacto iniciado en UI | Sin promoción/reanclaje/doble terminal. UI/protocolo separados de intentos de juego y con auditoría; no usar botones de feedback como nuevos gestos. |
| M24-N12 | Began fuera del dominio, rayo inválido, escala/datos no fiables | Rechazo o cierre conservador con motivo, sin objetivo/Swing fabricado. Usar política existente de DPI/fallback; no prometer mm físicamente calibrados. |
| M24-N13 | Muestras anteriores al instante observado de fijación entregadas después (O2) | Contadas con sus tiempos/desplazamiento; no corregir retrospectivamente objetivo ni fabricar sincronía. |

**Riesgo por fases, aprobado por el PO:** apoyo/duda/pausa/temblor antes de tirar fuera de zona no cargan. Mantener distancia deliberada sí conserva P. La regla original 150ms/P<0,60 y sus casos siguen intactos para método B de A′. N04 reconoce la limitación de observar voluntad; no se crea un criterio numérico nuevo para eximirla ni se declara M2-4 validado por los demás tests.

## 9. Plan TDD previsto: Tarea 1.2c, no ejecutable en esta autorización

No renumerar 1.2/1.2b/1.3. Integración futura mínima sobre el flujo táctil y preview existentes, sin autoridad nueva de pelota/reloj/predicción, frameworks ni consumo del DevPanel por runtime. No hay lista de archivos creados ni código en esta PR. A′ y S1-sling conservados/por defecto y experimental respectivamente; ninguna refactorización de los hallazgosC5/C6 de Claude por esta tarea.

| Subtarea futura | RED por aserciones previsto, después de autorización de implementación |
|---|---|
| 1.2c-0 Preparación declarada | Fixtures/proyección/snapshots y rutas de prueba compilables, instrumentación/preset experimental a definir. Preparación no es RED ni autoriza catálogo/defaults ahora. |
| 1.2c-1 Entrada absoluta | `Began_MapsUsingCapturedProjection_AX1`, `Moved_DoesNotRetarget`, `TinOut_StaysRedAndInvalid`, `OutsideDomain_RejectsWithReason`; cubrirN08/N12 y UI/área sin cambiar cámara. |
| 1.2c-2 Potencia/C-A/N2 | `NoPull150ms_NoSwing`, `Tap_PrecedesDeadZoneClosure`, `DeadZoneReturn_CancelsWithoutPeak`, `HeldPull_KeepsDistancePower`, `SoftFastPull_MayStillBeTap`;N01–N06, incluidas fronteras y riesgoN04/N04b documentado (también lateral con P≈0 no buscado), sin aserción ficticia de intención ni filtro nuevo. |
| 1.2c-3 Tiempo/contexto | `IdenticalEnded_ClosesOnce`, `ContradictoryTime_NoSwing`, `ContextChange_InvalidatesContact`, `SecondaryFinger_NeverPromoted`;N07/N10/N11. |
| 1.2c-4 Métricas/cadenas/O2 | `BeganProjection_RecordsActualObservedTime`, `LatePreLockSample_IsAuditedWithoutRetarget`, `Reapply_LinksButNeverReplaces`, `PlannedB1Cancel_NotAccidentalCorrection`;N09/N13. |
| 1.2c-5 Vista/protocolo | `BeganFix_UsesExistingReticleWithoutExtraAid`, `InvalidRed_NotHiddenBySuccessFeedback`, `ThreeResponses_HaveDisjointDeclaredMeaning`, `C_A_B1Closure_NotMissingFix`, `PreparedM24_ProtectsDevExperiments`; geometría existente/frontis despejado/aviso y temporizador. |

El main actual **no implementa este contrato**: `gesture.control` ofrece A′/S1-sling; `SlingModel` fija por quietud, `SlingProtocol.Respond(bool)` solo distingue dos respuestas y los terminales semánticos nuevos no existen como tales en `GestureOutcome`. Tampoco hay snapshot de proyección M2-4 en sus logs. No basta cargar un preset para probarlo; los cambios mínimos necesarios solo se harían con autorización de código, sin reutilizar un hash de otro control.

Secuencia futura: aprobar diseño→autorizar implementación por separado→esqueleto compilable→observar RED válido por aserción→GREEN→regresión fresca adecuada y release checks si se autorizan. No erroresCS como RED ni reutilizar 411/194 anteriores para código inexistente. Un selector/clave/preset nuevo requiere alcanceR2-3 y hash real documentado conservando 41valores/SimulationKey, nunca anunciarlo sin código autorizado. Una build/instalación/sesión requiere autorización propia y perfil/cotejos; no queda desbloqueada por aprobar esta documentación.

## 10. B0–B5 adaptados a C-A, como protocolo para aprobación

**No ejecutar ahora.** Antes de cualquier bloque futuro: explicación clara de «el apoyo ya apunta; bajar solo da potencia; para cancelar, vuelve a la altura donde apoyaste, o más arriba, y suelta», mano activa declarada/agarre, condición sin ayuda adicional, nuevo build/control/preset/hash efectivo/pendientes 0/Jugando verificados, repetición temporal 0 sin guardar, DEV cerrado. No usar la app 4cec622 como si tuviera M2-4. Hash extendido esperado de P0 se determinará para la build autorizada, sin asumir que se mantiene al cambiar catálogo; los tres archivos persistidos sí deben cotejarse con respaldo.

| Bloque | Tarea candidata adaptada | Qué se informa sin alterar criterios |
|---|---|---|
| B0 | Familiarización 120s con temporizador: apoyo fija, tirar da P; para cancelar, soltar a la altura del apoyo o por encima, y reapoyar con Began nuevo. Identificar válida/roja/Tap/abandono/cancelación. | Todos como práctica. Cuenta atrás y cancelación comprendidas antes de B1; si no, detener/restaurar, sin inventar sesión completa. |
| **B1** | **Diez apoyos dirigidos T1–T5 dos veces; retirar sin buscar tirón. «Soltar suave» pasa a significar suelta sin carga/terminación C-A, no golpe suave.** | Error del objetivo del Began frente a T, oclusión inicial/destape, fijación observada y feedback. Tap o AbandonedNoPull son cierres esperados de la tarea: no exigir Swing ni usar esos cierres como prueba de no fijación. Si la intención cayó fuera, inválido/fallo se conserva. |
| B2 | T3 realmente buscado:10suaves,10medios,10fuertes de referencia. Tirón pequeño/intermedio/mayor tras apoyo, sin arrastrar para corregir la mira. | Banda/P/d/errores de apoyo, N2/Tap, C-A y saturación; resultados conjuntos y componentes separados. No reemplazar suaves cancelados ni bajar N2 para mejorar 9/10. |
| B3 | Diez medios aT1/T2 alternados, apoyo directo bajo seguido de tirón. | Separar error inicial, carga intentando «corregir», retorno C-A, apoyo accidental/resbalón y No fijó. RB6 no queda resuelto por nombre de fase; intención del PO aparte. |
| B4 | Cinco cancelaciones C-A: tirar y volver antes de soltar; corrección requiere nuevo Began, sin zona superior/lateral. | Cancelación deseada/accidental/interferencia, perdida de suave querido, motivo N2/C-A y vínculo de reapoyo; no inferir voluntad solo por outcome. |
| B5 | Diez a ritmo cómodo con objetivos/intensidades de referencia y un pulgar. | Duración completa incluidas transiciones cancelar→reapoyar, error/Banda/abandono/Tap. Sin umbral temporal nuevo ni rally real. |

Para B1, proponer metadato de tarea «apoyo sin carga» y potencia solicitada «ninguna», no conservar «soft» como si se esperara un golpe suave. Son identificadores de diseño, no cambios de código/schema; B1 no entra en el denominador de bandas B2.

Mantener secuencia T1–T5 de B1/B5 y suave/media/fuerte de B5 ya documentadas; cada intento programado se conserva aunque termine sin Swing. **No repetir hasta obtener diez éxitos.** Si se hacen reapoyos extra, llevan id propio/parent y se informan separados, sin sustituir el primer error ni reducir el denominador de fallos. Si el protocolo ya cambió de objetivo, el nuevo apoyo no demuestra corrección del anterior; guardar ambos contextos. Cualquier ensayo adicional de corrección al mismo objetivo debe predeclararse, no improvisarse en una tanda.

B1/B3: respuestas según §6; las otras series no se atribuyen esa intención a partir de P o coordenadas. Ningún tamaño/halo/lupa se añade en esta primera referencia; si los datos futuros lo justifican, prueba posterior con orden predeclarado y nueva autorización. Los 16 juicios originales por separado/literales, con fluidez como prioridad declarada del PO y sin convertirlo en aprobación automática de control.

Informes futuros: mínimo/mediana/máximo y bandas deP por serie, saturación con advertencia existente, error Began continuo sin umbral nuevo, duración completa/retocados/cancelaciones/rechazos, estado de ayuda/mano/configuración por bloque y O2. No comparación causal con las 111 trazas: pertenecen a otro contrato con desviaciones conocidas. Si cambia configuración/control durante un bloque, invalidar contacto y etiquetar afectados como no comparables; no corregir en replay para mezclarlos.

## 11. Cierre de PR #26 e invariantes de esta entrega

Claude y PO verificaron PR #26: merge **bf607cdc235ac381799d5bbbb1a1633ca394dfe9**, segundo padre/head corregido **e452cc26ced5d404542fca7d96274b16cb2c91e1**, árbol idéntico. Delta desde f2068ac solo propuesta/aprobaciones/AGENTS con tres correcciones; main sincronizada/limpia y sin worktree extra al iniciar esta entrega. Retiro sin `--force`; inventarios ignorados vacíos cotejados en `Logs/archive-s1-mechanics-proposal-2-20261028` del checkout principal; ramas remotas intactas. Este es el registro del cierre en la siguiente entrega, **sin commit aparte en main**.

A′ por defecto(`gesture.control=0`), defaults 45c4e9af5c8f, presetS1-sling 7d1a61c39766 sin promover,41heredados/proyección 0d39e6016bd8/SimulationKey 166a96e319c7 intactos. El diseño no cambia los hallazgos/juicios dePR25 ni sus muestras, ni spec v0.3, criterios, bandas o umbrales. Adendas D3 a propuesta/plan son añadidos para revisión, no edición de sus decisiones históricas ni autorización de ejecutar 1.2c.

Entrega lista para Claude en PR propia, sin fusionar y detenerse. No código/tests/presets/defaults/spec/suites/check-release/build/instalación/sesión/acceso al iPhone ni replay/cálculos nuevos sobre 111 trazas. **S1 sin aceptar; S2–S4/T8 sin autorizar.** Perfil válido registrado hasta 2026-11-03T18:29:44Z; no hay instalación autorizada.
