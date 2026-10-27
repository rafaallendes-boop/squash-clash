# S1-sling: primera parada, diseño de interacción

Fecha 2026-10-27. Rama firstshot/s1-sling-prototype, base main **956cfafbbfea6fef418fa24e6a3b0911663746be**, tras fusionar PR #24 aprobada en 25c180d8f3aa3093209855f892969dcc40b1cbed.

**Esta entrega es un único commit solo Markdown y una PR en borrador para revisión de Claude. Detenerse después: ningún código del prototipo antes de esa aprobación.** Los pasos 2/3 de la decisión del PO están condicionados; no se ejecutan en esta parada. [Registro literal del PO](2026-10-25-first-shot-po-approvals.md), [diseño anterior](2026-10-27-first-shot-s1-control-b-design.md), [propuesta First Shot r2](../superpowers/specs/2026-10-24-first-shot-design.md) y [plan r2](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Spec v0.3 intacta. A′ sigue por defecto y conserva Flick, N1 por regreso al origen y la regla original 150 ms/P<0,60 del método de potencia por velocidad.

## 1. Qué decide el PO y qué propone esta interacción

El PO autoriza investigar el control §6.2 B, “apuntar, fijar y tensar”, mediante un prototipo S1 con cancelación por zona explícita, quietud configurable y sesión exploratoria breve. Los valores son **[EXPERIMENTO]**. S1 no queda aceptado y no hay contacto real con la pelota.

| Tema de PR #24 | Decisión aplicada al prototipo |
|---|---|
| NB0/NB1/NB2, círculo original o reanclado | Sustituidos por **zona de cancelación explícita**; no hay armado por salir/volver para este control. A′ no cambia. |
| Pausa de 120 ms como candidato abierto | Inicial120 ms; rango permitido para probar120–150 ms, con tolerancia configurable (15 mm/s/radio 1 mm iniciales), hueco≤51 ms y mínimo 3 muestras. No se cambia120→150 durante la sesión: otra decisión del PO con evidencia. |
| d_full40 mm y alternativas de escala | Inicial35 mm; rango 30–40 mm, zona muerta2 mm y curva lineal saturada. No adaptación de escala al lugar de apoyo. |
| G2/G3 como mitigaciones por decidir | **G1 visual y medición de fijaciones involuntarias**; no segundo reconocimiento del fijado, acción de armado ni nueva dirección. RB6 se registra y mide, no se declara resuelto. |
| Comparación física contemporánea con A′ y cuatro series | Sesión exploratoria del prototipo B0–B5. A′ se conserva como referencia, pero esta sesión no introduce otro brazo A′ ni permite comparación causal con campañas anteriores. |
| U1 dependiente de N1 | Tras fijar, cancelar solo al soltar en la zona explícita. Antes de fijar, abandono sin consumo, salvo Tap según N2. |
| Háptica de fijado | Fuera de este prototipo: destello/forma/texto. No sonido, háptica ni calidad de contacto. |

## 2. Gesto propuesto, sin implementación en esta parada

Un solo contacto primario del mismo lanzamiento: Began → Apuntando → quietud acreditada → Fijada → Tensando. El objetivo no cambia después de fijar. Zona de cancelación es un estado transitorio de ese mismo gesto; Inválida es una condición visible de objetivo/margen/datos, sin esconder la fase. No hay un sistema de input paralelo ni abstracciones generales: implementación futura en TouchSample/GesturePreview/Core Shot/ShotOverlay/DevPanel.

- **Apuntando:** mira libre relativa al arrastre, potencia 0; guardia de arrastre previo≥3 mm. Quietud medida entre muestras reales durante 120 ms iniciales con velocidad≤15 mm/s, radio≤1 mm, huecos≤51 ms y mínimo 3 tiempos distintos. No repetir posiciones por frame ni convertir silencio en inmovilidad. Un Ended no completa retrospectivamente la quietud.
- **Fijada:** una única transición por gesto, con destello 120 ms y cambio visible; “confirmada” significa acreditada por el detector, **no** confirmación humana adicional G2. La posición del dedo al fijar es el nuevo origen de potencia. Primera fijación puede ser involuntaria: G1 no identifica intención.
- **Tensando:** solo movimiento posterior a fijar; proyección hacia abajo y distancia **actual**, lateral sin potencia; volver arriba reduce P. Zona muerta2 mm y tope 35 mm iniciales, curva lineal con saturación; no se retiene pico ni se carga el apuntado anterior. Bandas existentes suave/media/fuerte y advertencia de saturación>20 % sin cambios.
- **Final:** Ended original del dedo primario es r para un Swing; no el frame de procesamiento. Tap, abandono, cancelación de zona y SystemCanceled no generan golpe ni consumen intento. S1 solo registra/previsualiza incluso cuando el outcome se llama Swing.

N2 mantiene el criterio vigente por desplazamiento máximo del gesto completo (<3 mm en<250 ms). Si es Tap, se registra Tap. Un arrastre que termina antes de fijar se registra Abandonado antes de fijar; no se simula regreso N1. Tras fijar, fuera de la zona se registra Swing, incluso P=0 si no se tiró; dentro, Cancelado por zona. Para el nuevo control no se aplica el Canceled de un N1 global de Began que anularía un tirón medio legítimo.

Ended idéntico en posición/timestamp cierra una sola vez sin anular potencia fiable, ni inventar velocidad, tiempo o muestras. Datos contradictorios/no monótonos siguen conservadores, con razón registrada. La futura solución se limita al control nuevo: **no cambia la ruta A′ ni corrige silenciosamente su problema pendiente del método de potencia B**. Lanzamiento, primer dedo, selector y configuración se fijan al apoyar; segundo dedo no se promueve. Pausa/foco/DEV/cambio de lanzamiento invalidan toda fijación/tensado, sin Ended ficticio ni recuperación automática al volver.

## 3. Zona de cancelación: propuesta final de Codex para revisión de Claude

### Geometría y liberación

**Propuesta de interacción, no decisión adicional atribuida al PO:** región superior relativa al dedo al fijar, no a la mira en metros. Empieza a12 mm **[EXPERIMENTO] por encima** del punto de fijación y comprende cualquier desplazamiento lateral dentro del área útil. Cancela **solo al soltar dentro**, con límites inclusivos. Entrar y salir no arma un latch: volver a tensar y soltar fuera produce el Swing correspondiente. Entre fijación y comienzo de la región superior hay franja neutraP=0; el regreso por esa franja no cancela.

La región tiene el ancho útil completo y se extiende hasta su límite superior. Para evitar una franja casi inalcanzable, se propone una **profundidad disponible mínima de 8 mm [EXPERIMENTO]**; es tamaño de diana propuesto por Codex para revisar, **no radio N1 reutilizado ni umbral aprobado de aceptación**. No se desplaza el inicio12 mm para hacerla caber.

Si no cabe esa profundidad sobre la fijación, usar **salida lateral desde 20 mm [EXPERIMENTO]** y ancho disponible mínimo 8 mm. Región lateral exterior al umbral, a cualquier altura útil; entrada/salida y Ended se juzgan igual que arriba. La neutralidad por encima de la fijación sigue dandoP=0; en lateral puede haber P candidata descendente, pero al soltar dentro prevalece Cancelado por zona. No se convierte el movimiento lateral en potencia.

Orientación lateral propuesta: preferir el lado de la mano **declarada para el protocolo**, izquierda inicialmente; si allí no hay margen, usar el opuesto solo si cabe. No inferir mano por coordenadas del dedo ni por mano del personaje. La geometría y lado se seleccionan una vez al confirmar fijación, se registran y **no cambian durante el gesto**. Si no cabe región superior ni lateral, no confirmar fijación/tensado: seguir Apuntando con aviso Inválida/margen insuficiente; soltar abandona sin consumo. No mandar al PO al borde del sistema para que consiga cancelar.

“Área útil” propuesta: área segura de la app menos la DevBar y controles que intercepten el toque. La implementación futura debe obtenerla de la geometría actual; esta entrega no mide px/mm ni presupone todo el alto físico del teléfono. Si cambia orientación/contexto durante el gesto, se cancela por sistema, no se reposiciona la zona bajo el dedo.

### Presentación sin ocultar el frontis

La detección geométrica no exige pintar una banda opaca. Desde Fijada se muestra **✕ semitransparente y flecha/texto de la región activa**; al estar dentro se resalta y aparece “Zona de cancelación · suelta para cancelar”. El indicador principal queda en la franja compacta sobre DevBar, dentro de área segura y **fuera del rectángulo proyectado del frontis**. Para izquierda se propone extremo derecho de esa franja; para derecha, espejo, sin moverlo a mitad del gesto. La mira conserva sus mínimos de PR #19.

Puede añadirse una ✕ en la región física solo si no interseca frontis, HUD o controles; si no cabe, se omite esa segunda marca y se conserva el indicador compacto con dirección explícita. No se pinta el área de detección sobre el objetivo ni debajo del dedo como única explicación. Tamaño visual candidato de la ✕ local:60 px de referencia con trazo6 px [EXPERIMENTO], subordinado a no ocluir; no reduce ni sustituye los mínimos de la mira de PR #19. El color no es el único canal: forma, texto y estado siguen visibles, incluida Inválida.

Esta separación visual tiene un costo: la indicación compacta es indirecta y el PO debe aprender dónde soltar. En B0 se pide identificar región y estado antes de B1. Si no los entiende, registrar y detenerse; no convertir una interfaz no comprendida en cinco cancelaciones “fallidas”.

### Análisis crítico y riesgos que la revisión debe resolver

| Caso | Consecuencia y propuesta |
|---|---|
| Tirón hacia abajo válido | Se aleja de la región superior; el ejemplo de apuntar arriba y volver a Began no cancela por sí mismo. En lateral, una desviación extrema sí podría entrar: medir cancelaciones accidentales, no retocar umbrales durante la sesión. |
| Reducir potencia volviendo arriba | Decrece P hasta 0 y atraviesa franja neutra; solo cancelar si se sobrepasa12 mm arriba y se suelta allí. Sobrepaso involuntario sigue posible. Pasar por la zona y salir no cancela. |
| Fijar junto al borde superior | No comprimir la zona: salida lateral 20 mm si cabe, fijada y señalada. Si tampoco cabe, no habilitar tensado y registrar margen insuficiente. |
| Apuntar bajo y seguir bajando, RB6 | La zona no lo resuelve: una fijación prematura sigue interpretando el descenso posterior como potencia. G1 y declaración del PO permiten describirlo, sin declarar seguridad ni añadir G2/G3. |
| Apoyo bajo y tirón completo | El candidato 35 mm puede no caber; registrar distancia mínima al borde inferior/área segura y posición de apoyo. No normalizar P para que el borde equivalga a 100 %, ni inventar muestras tras perder el toque. |
| Mano izquierda/derecha | Región superior simétrica; lateral y posición del indicador deben reflejarse. La sesión es solo izquierda; no inferir validez diestra de sus datos. |
| Área segura/inicio iOS | El adaptador actual filtra Began y difiere gesto inferior; no garantiza continuidad. Si hay Canceled/foco, SystemCanceled sin consumo. Una salida lateral tampoco debe exigir invadir un borde de sistema. |

La propuesta superior 12/lateral 20/profundidad 8 y presentación se someten a Claude en esta **primera parada**. No se programan ni prueban antes de su aprobación. Un cambio de interacción solicitado en esa revisión se documentará antes de código; no se trata como ajuste de sesión.

## 4. Configuración prevista: valores iniciales, no claves añadidas

Selector gesture.control capturado en Began: A′ por defecto; alternativa control §6.2 B/S1-sling. Preset experimental separado S1-sling solo se creará en el paso 2. Ningún JSON, catálogo o default cambia ahora. K2 usa unidades explícitas y no mezcla distancia con el método de potencia B por velocidad. Se proponen los nombres del diseño anterior para quietud/potencia y nombres propios para la zona:

| Grupo | Inicial [EXPERIMENTO] / margen de preparación | Condición |
|---|---|---|
| aim.holdMs | 120 ms; permitido 120–150 | Cambiar120→150 requiere decisión posterior del PO con evidencia, no durante B0–B5. |
| aim.holdMaxSpeedMmPerS / aim.holdRadiusMm | 15 mm/s /1 mm | Configurables en DEV, capturados al apoyar; no ajuste en sesión. |
| aim.holdMaxGapMs / aim.holdMinSamples | 51 ms /3 tiempos distintos | Duración real y apoyo, sin completar por silencio. |
| Guardia de arrastre | ≥3 mm previo a fijar | N2 vigente; no cargar potencia antes de fijar. |
| power.pullDeadZoneMm / power.pullFullMm | 2 mm /35 mm; tope permitido 30–40 | Curva **lineal saturada**, no otra curva por defecto ni escala según apoyo. |
| gesture.slingCancelAboveMm | 12 mm propuestos; preparación 6–30 | Revisión de la interacción antes de código; no cambia en sesión. |
| gesture.slingCancelLateralMm | 20 mm propuestos; preparación 10–40 | Solo fallback geométrico, sin variar a mitad del gesto. |
| gesture.slingCancelMinDepthMm | 8 mm propuestos; preparación 4–15 | Tamaño candidato, no N1; evitar franja inalcanzable. |
| Mano declarada/lado preferido | Izquierda en esta sesión | Dato de protocolo/configuración capturado; no mano física del jugador. |
| aids.aimLockFlashMs | 120 ms | Feedback visual; **sin háptica ni sonido** en el prototipo. |

Los rangos de cancelación son propuestas de preparación de Codex para revisar con la interacción, no decisiones ya tomadas por el PO. No sirven para buscar valores favorables durante una tanda. Se registrarán los usados y las condiciones cruzadas de margen/área útil. Las bandas existentes no cambian; saturación>20 % es advertencia descriptiva, no se redefine para mejorar la lectura.

R2-3: cuando se autorice y ejecute el paso 2, el hash global podrá cambiar solo por las claves autorizadas; registrar el nuevo hash, preservar byte a byte los 41 valores heredados y SimulationKey 166a96e319c7. Hoy defaults 6095c0b38614 siguen intactos. La ruta A′ mantiene Flick, cancelación N1 original y 150 ms/P<0,60; no se migra al prototipo por defecto.

## 5. Protocolo exploratorio definitivo propuesto para esta revisión

**Solo diseño; no sesión ni instalación en esta parada.** Estimación~15 min, no un límite que obligue a ocultar errores o acelerar al PO. Mano izquierda declarada. Preset S1-sling aplicado sin guardar, repeat=0. Antes de **CADA bloque**: build/control/preset/hash efectivo y de gesto esperado/cero pendientes/escenario/valores confirmados; DEV cerrado después de armar protocolo. Al reiniciar o interrumpir, volver a verificar. Si no coincide o aparece un defecto, registrar y detenerse; no repetir para sustituir intentos.

### Objetivos y orden predeclarado

Propuesta de marcas mínimas configurables solo en DEV/protocolo, sin afectar prod. Usar5 objetivos T1–T5 en el rectángulo legal interior del frontis de la configuración: T1 bajo-izquierda(25 % ancho,15 % alto); T2 bajo-derecha(75 %,15 %); T3 central(50 %,50 %); T4 alto-izquierda(25 %,80 %); T5 alto-derecha(75 %,80 %). Valores de ubicación [EXPERIMENTO] propuestos para revisión, **no zonas de H2 ni criterios nuevos de la spec**. Mostrar una marca activa fina con identificador; no un panel sobre el frontis. Validar que las posiciones finales sean legales con la configuración y declarar sus coordenadas; si la configuración no lo permite, detener preparación en vez de recolocarlas silenciosamente.

| Bloque | Intentos e instrucciones | Registro/métricas |
|---|---|---|
| B0 Familiarización | 2 min, práctica identificada por separado. Identificar mira/fijación/tensado/zona y realizar cancelación guiada antes de medir B1. | Todos los intentos registrados como práctica, nunca sustituyen intentos formales. Señalar si entiende el estado y la región. |
| B1 Apuntar y fijar | 10 intentos: T1,T2,T3,T4,T5 y repetir esa secuencia. Apuntar, dejar fijar y soltar suave, sin buscar tirón fuerte. | Error de mira al fijar, tiempo hasta fijar y señalamiento del PO de “lejos”/fijación involuntaria por intento. Error continuo, **sin umbral numérico de lejos**. |
| B2 Potencia | ObjetivoT3:10 suaves, luego10 medios y 10 fuertes. | Banda conseguida, P y d mínima/mediana/máxima y dispersión, saturación>20 %, margen al borde. |
| B3 Apuntar bajo y tirar | 10 medios: T1/T2 alternados. | RB6: continuación del descenso tras fijación prematura, potencia no buscada declarada por PO; error de mira y distancia al borde inferior. No confundir todo tirón posterior con intención confirmada. |
| B4 Cancelación | 5 cancelaciones voluntarias tras fijar, con zona indicada según geometría. | Outcomes y si canceló cuando quería. Cancelaciones accidentales contadas **en todos** los bloques; no completar la serie repitiendo para reemplazarlas. |
| B5 Ritmo | 10 sucesivos “lo más rápido que sea cómodo”: objetivos T1,T2,T3,T4,T5,T1,T2,T3,T4,T5; potencias suave/media/fuerte/suave/media/fuerte/suave/media/fuerte/suave. | Duración Began→Ended mediana/p90, errores y abandonos, sin objetivo temporal de aprobado. No simular rally ni contacto. |

Se conservan los 65 intentos formales y todos los de práctica, incluidos Tap/SystemCanceled/abandono/cancelación, parciales y cualquier intento adicional, siempre separado y sin sustitución. Si el PO interrumpe, se registra lo hecho; no se inventa una serie completa. La secuencia de B5 y los objetivos deben quedar visibles antes del intento correspondiente, sin navegar por DEV durante el gesto.

### Registro y lectura

Por gesto: control, transiciones con timestamp (Began, fijación, inicio de tensado, entrada/salida de zona, Ended), objetivo solicitado y posición de fijación/error en el frontis, ancla/eje/zona elegida, d actual/máxima/lateral, P y banda, outcome y motivo, duración total/tiempo hasta fijar, distancia mínima al borde inferior/área segura durante el tirón, hash/preset/escenario/bloque/intento. Identificadores de dedo/lanzamiento internos sin datos personales del dispositivo. La app actual no produce todavía todo esto: son requisitos del paso 2 condicionado, no evidencia ya existente.

RB6 no se oculta con P=0 artificial ni se transforma en “tirón querido” por estar después de fijar. Registrar el patrón de pausa/fijación/continuación, el objetivo y P realmente obtenidos, y la declaración del PO de fijación involuntaria/potencia no buscada. No fijar un umbral de error o tasa de “involuntario” antes de validarlo. mm por escala reportada no significan calibración física verificada; declarar procedencia de DPI/escala, sin asumir frecuencia táctil.

RestaurarP0-calib-1 al terminar o abortar; comprobar en **nuevo arranque** hash esperado de esa build, cero pendientes y P0/Po-1/last-active byte a byte iguales al respaldo. En el paso 2 futuro: comprobar perfil efectivo antes de instalar, renovación a cargo del PO según su decisión; esta entrega no toca el iPhone. Los inventarios/cotejos de instalación y build corresponden al futuro paso condicionado, no al cierre documental.

### Preguntas finales, cada una por separado y respuesta literal

Se incorporan literalmente las preguntas del punto 6 aclarado por el PO, más la formulación incluida en el adjunto original. No reemplazar respuestas por un sí/no inferido de los números:

1. ¿Puedo colocar la mira donde quiero?
2. ¿Puedo apuntar bajo sin activar accidentalmente la potencia?
3. ¿La mira permanece estable después de fijarse?
4. ¿Cuántas fijaciones involuntarias ocurren?
5. ¿Cuánto tiempo necesito para fijar la mira?
6. ¿Entiendo claramente cuándo quedó fijada?
7. ¿Puedo conseguir golpes suaves, medios y fuertes voluntariamente?
8. ¿La potencia responde de forma consistente a la distancia?
9. ¿Puedo alcanzar la potencia máxima cómodamente?
10. ¿Existen limitaciones por el borde inferior de la pantalla?
11. ¿Puedo cancelar cuando realmente quiero?
12. ¿Ocurren cancelaciones accidentales?
13. ¿La zona interfiere con los movimientos normales?
14. ¿Cuánto demora el gesto completo, desde apoyar el dedo hasta soltar?
15. ¿Se siente natural o demasiado mecánico?
16. ¿Podría utilizarlo repetidamente durante un rally rápido?
17. ¿lo usarías en un rally rápido?

Preguntas sobre cantidades/tiempos se conservan también como juicio literal del PO, separadas de la métrica calculada. El potencial para un juego dinámico se evalúa como juicio; no se abre un rally ni se acepta S1 por una respuesta favorable.

## 6. Criterios verificables de interacción y plan condicionado 1.2b

Estos son contratos técnicos del prototipo y condiciones de trazabilidad, **no nuevos umbrales de aceptación del experimento físico**. La tarea 1.2b se incorpora documentalmente aS1 sin renumerar las demás; código solo después de aprobación de Claude del paso 1. TDD futuro con esqueletos compilables y RED válido por aserciones; no errores CS como RED. Ninguna prueba se escribe/ejecuta en esta parada.

| ID | Contrato verificable / RED previsto |
|---|---|
| V1 Fuente/quietud | Sin repetir posición por frame ni acumular quietud por silencio; tolerancias/huecos/muestras/duración provienen del snapshot; guardia de arrastre y una sola fijación. |
| V2 Separación | Potencia0 antes de fijar, incluido apuntar hacia abajo; después, mira inmóvil y nuevo origen de distancia. Una fijación involuntaria sigue pudiendo producir RB6: se registra. |
| V3 Potencia | Zona muerta/tope 35 inicial/curva lineal/saturación; lateral sin potencia; distancia actual y regreso reducen, sin pico retenido ni adaptación al borde. |
| V4 Cancelación | Ended dentro cancela sin consumo; pasar por región y salir no; tirón válido que vuelve a Began no cancela por ese hecho. Neutral encima de fijación da P=0; criterios superior/lateral usan configuración estable. |
| V5 Espacio/vista | Fallback lateral por margen insuficiente, lado fijo por gesto; si no cabe zona, no fijar. Estado con forma/texto, frontis despejado y mira con mínimos PR #19; objetivos solo DEV/protocolo, nunca prod. |
| V6 Cierre/contexto | Abandono antes de fijar; Tap N2; r original; Ended idéntico dt=0 cierra sin anular P del nuevo control; datos contradictorios conservadores; primer dedo/lanzamiento/pausa/foco/DEV invalidan. A′ intacto. |
| V7 Registro/comparabilidad | Todas las transiciones/outcomes/casos inválidos y hash por bloque; cada intento conservado, datos y declaraciones separados; restauración P0 comprobada en arranque nuevo. |
| V8 Compatibilidad | Selector capturado al apoyar y A′ por defecto; K2/R2-3 solo claves autorizadas y hash nuevo cuando se implementen;41 heredados y SimulationKey intactos byte a byte. |
| V9 Parada | PR de esta parada en borrador tras un solo commit Markdown; no código antes de aprobación de la interacción. Defecto en trabajo/sesión posteriores: registrar y detenerse. |

Expectativas de los casos futuros RB, ahora según la decisión del PO:

- RB1–RB2: apuntado/quietud sin tirón posterior no cargan potencia; sin apoyo real, no fijación retrospectiva al soltar.
- RB3: lateral/arriba no cargan potencia descendente; si Ended está en la zona activa, Cancelado por zona; fuera, P correspondiente sin potencia por módulo.
- RB4: tirón deliberado mantenido conserva P por distancia; **no** hereda decaimiento150 ms ni equivale a pasar la regla original del método de potencia B por velocidad.
- RB5: regreso arriba reduce hasta P=0 y atraviesa franja neutra; cancelar solo si Ended queda en región explícita, no por N1 de Began ni por haber pasado por ella.
- RB6: pausa que fija prematuramente al apuntar bajo y continuación descendente se registra y se mide; G1 no garantiza eliminarla. El test futuro verifica potencia 0 antes de fijar y trazabilidad de la continuación después, **no fabrica P=0 para declarar resuelto el riesgo**.

Pasos posteriores condicionados: implementar sobre arquitectura actual, regresión fresca completa EditMode/PlayMode sin CS, check-release dev/prod, preset experimental S1-sling y build dev Release ARM64/no Development; perfil/cotejos antes de instalar y sesión B0–B5 solo bajo la secuencia autorizada del PO. No reutilizar las suites anteriores como verificación de código aún inexistente. No contacto real, S2–S4, timing ±40 ms, V/T3, sonido/háptica/calidad, jugadores/IA/rally/marcador.

## 7. Resultado de esta primera parada

Se entrega una interacción concreta para revisión de Claude, con riesgos de cancelación/reducción/espacio/oclusiones/manos y el protocolo breve. Las dimensiones de región y presentación son propuesta de Codex sometida a esa revisión; no hay implementación ni validación física. El registro literal y las adendas D1 están en esta misma PR. **Un solo commit Markdown, PR en borrador, detenerse.** S1 sigue sin aceptar; no iniciar el paso 2 por cuenta propia ni S2–S4/T8.
