# Propuesta documental: mecánica de apuntado y golpe rápido en S1

Fecha: 2026-10-27. Rama `firstshot/s1-gesture-mechanics-proposal`, desde main tras el merge **31bbe03ecc6d2772c713620a6332d5b84ddb0e75** de PR #22. Decisión del PO comunicada junto con la revisión de Claude del head **7bfea922f49fc094fa59601d4e36d7a8e87025f6**.

**Estado vigente tras cambios solicitados por Claude sobre PR #23, head 55e061a:** el PO elige investigar el **control §6.2 B**, sin autorizar implementación ni la siguiente entrega. A′ se conserva como referencia. Las alternativas propias M1/M2/M3 y el control §6.2 C quedan documentados y descartados por ahora. El método de potencia B y aim.freezeOnRelease siguen desactivados por defecto; preset 08b6fe90889b sin promover, defaults 6095c0b38614 intactos. S1 sigue sin aceptar; S2–S4/T8 no autorizados.

## Decisión del PO (2026-10-27), registro literal

- Respuesta literal del PO: «Vamos con la opción 1».
- Pregunta C5: el PO considera que A′ falla en su forma actual, con la evidencia de PR #22: Flick 10/10 en fuertes, medios 3/10 en banda, saturación 30 % y su «No» a las tres intensidades voluntarias. Es un juicio del PO; no convierte la calibración en Fail medido ni cambia criterios o umbrales.
- Mecánica elegida para investigar: control §6.2 B de spec v0.3, "apuntar, fijar y tensar". A′ se conserva como referencia de comparación; no se elimina del código.
- Descartadas por ahora: dos fases con confirmación, fijación solo al soltar (equivale a la v0.1 descartada), Flick menos sensible y §6.2 C. Quedan documentadas y no se borran.
- Esta decisión NO autoriza código, cambios de spec/planes, presets, defaults, builds, instalaciones ni sesiones.

## 1. Qué demuestra la evidencia y qué queda abierto

Marco vigente: [spec v0.3, §6.2](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md), [propuesta r2, C5 y §11.4](../superpowers/specs/2026-10-24-first-shot-design.md) y [plan First Shot r2, Tarea 1.2](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Se citan para identificar las decisiones que una futura autorización tendría que resolver; permanecen intactos.

Fuentes: [informe de calibración](2026-10-25-first-shot-s1-gesture-calibration.md), [49 intentos físicos conservados](../validation/evidence/2026-10-27-s1-gesture-calibration-physical.json), [selección y riesgos sintéticos](../validation/evidence/2026-10-26-s1-gc-resolution.json), [grid offline anterior](../validation/evidence/2026-10-25-s1-gesture-calibration-offline.json). Se usan datos ya registrados; **no se ha hecho replay nuevo, medición, suite, build, instalación, sesión ni acceso al iPhone** en esta entrega. No se mezclan las mediciones físicas con el cálculo offline ni con las pruebas sintéticas anteriores.

| Observación | Evidencia y alcance |
|---|---|
| Fijación de mira y gesto fuerte comparten la velocidad del dedo | En los 10 fuertes comparables, la mira se fijó por **Flick 10/10**; en medios 7/10 por Flick y 3/10 por Release; suaves 10/10 por Release. Todos 08b6fe90889b/cero pendientes. |
| El PO debe separar mentalmente dos acciones dentro del gesto | Literal: «si voy fuerte se queda al medio el rombo» y «debo apuntar lento y acelerar». La respuesta completa a tres intensidades fue «No, la de golpear rápido me cuesta lograrlo ya que debo apuntar lento y acelerar». |
| Potencia alta no acredita control de tres intensidades | Suaves 10/10 en banda, medios **3/10**, fuertes 8/10; siete medios en banda fuerte. Saturación fuerte **3/10=30 %**, supera advertencia 20 %. No se reclasifican como accidentes sin juicio explícito del PO. |
| El cuarto bloque no valida el riesgo físico del método de potencia B | Sus 19 intentos usaron P0 / fdd997f2b85a, no 08b6fe90889b; riesgo físico del método de potencia B **NotEvaluated**. Nueve intentos y después diez, con siete cancelaciones, todos conservados. |
| Pocos datos no equivalen a seguridad o consistencia | No hay una segunda campaña controlada ni atribución causal entre alternativas. DPI 460 reportado sin regla física; mm/s no son una calibración física verificada. |

### Por qué la mira puede quedarse cerca del punto inicial

[AimModel.Update](../../Assets/SquashClash/Core/Shot/AimModel.cs#L43-L92) mueve la mira a partir del desplazamiento desde Began y calcula velocidad entre muestras. Dos intervalos consecutivos sobre 250 mm/s confirman Flick; la fecha del cruce se conserva en la banda de histéresis y solo se borra por debajo de 210. La fijación recupera la historia en cruce−50 ms; sin historia suficiente usa el primer punto válido. Una vez Frozen, se ignoran nuevas actualizaciones del gesto.

Por tanto, un arranque rápido puede fijar una posición anterior al desplazamiento que el PO quería usar para apuntar, incluso cerca del inicio. Es una explicación compatible con el código y el relato; **no se afirma que los 10 fuertes se fijaran exactamente en el centro** ni que todo Flick sea indeseado. freezeOnRelease=1 solo fija al soltar si aún se está Aiming: **no elimina ni pospone el Flick anterior**. Subir la potencia después de una fijación no reposiciona la mira.

La lectura de Claude/PO es un **acoplamiento de apuntado y potencia en un mismo gesto**. La velocidad decide a la vez cuándo dejar de apuntar y cuánto vale el golpe. Cambiar únicamente la escala de potencia no cambia la regla que fija la mira. El «si» del PO sobre apuntar donde quiere no cancela su «No» sobre tres intensidades ni la dificultad de la secuencia lenta→rápida.

### Tensión entre riesgo, memoria y saturación

El requisito eliminatorio del método de potencia B por velocidad en el control A′ se conserva: en los mismos tres perfiles sintéticos de arrastre rápido, **150 ms inmóvil y después soltar**, cada P debe ser **<0,60**. De las cuatro combinaciones supervivientes predeclaradas, solo 300/0/60 lo pasó: 10,944/23,101/25,999 %. Las 400/0/120; 400/0/180 y 400/10/180 fallaron uno o más casos. El único superviviente usó una escala de 300 mm/s y memoria de 60 ms; no se eligió por una nueva preferencia física del PO.

La memoria corta reduce la potencia de una aceleración antigua, pero también penaliza al fuerte que se detiene antes de soltar: el costo sintético aceptado fue 53,959→31,267 % para 500 mm/s detenido 100 ms. La escala corta permite alcanzar la banda alta, pero en esta tanda hubo 30 % de saturación fuerte y siete medios en fuerte. **La selección que resolvió el riesgo sintético no resolvió la mecánica ni validó la escala físicamente.** No se deduce que subir o bajar una constante arregle ambos problemas. No se relaja 150 ms/P<0,60 ni se amplía el grid a posteriori; no se aplica otro candidato en este documento.

## 2. Marco normativo: control A′, control §6.2 B y control §6.2 C

La [spec v0.3, §6.2](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md) define la **congelación por Flick como protección de A′ contra el acoplamiento**: la mira toma la posición de ~50 ms antes del cruce y el flick ya no la mueve. La v0.1, flick sin congelar la mira, se descarta en favor de A′ por ese acoplamiento. Mantener una mira continua y fijarla solo al soltar (M2 abajo) equivale en la práctica a esa v0.1 descartada; no es la protección definida por A′.

La [propuesta First Shot r2, C5 (§8/§9)](../superpowers/specs/2026-10-24-first-shot-design.md) limitó el control inicial a A′ y dejó los controles §6.2 B/C como alternativas si la experiencia no funcionaba. **C5 ya tiene respuesta del PO**, registrada literalmente arriba: considera que A′ falla en su forma actual y elige investigar el control §6.2 B. Es un juicio del PO, no un nuevo resultado Fail medido ni una modificación de criterios. No se infiere aceptación de S1, del preset o del candidato 300/0/60.

| Control existente en spec v0.3 §6.2 | Mecánica documentada | Estado tras decisión del PO |
|---|---|---|
| Control A′ | Arrastre relativo; potencia por velocidad; congelación por Flick sobre la historia anterior al cruce para separar el movimiento final de la mira. | Referencia de comparación; no se elimina del código. |
| **Control §6.2 B: “apuntar, fijar y tensar”** | **Dedo quieto ≥120 ms [EXPERIMENTO]** fija la mira con destello visual. Después se arrastra **hacia abajo**, tipo honda: potencia por distancia en mm, con tope. La mira está fija mientras se tensa. | Elegido **para investigar**. Definición operativa de quietud, riesgo de fijación involuntaria y detalles de tensado pendientes; no se implementa. |
| Control §6.2 C: “potencia contextual asistida” | Potencia por contexto: banda de altura y situación; un flick rápido al soltar sube un nivel (suave→media→fuerte). Su protección es que casi no hay potencia manual; la spec advierte menor profundidad y confusión drop/kill. | Descartado por ahora por el PO; se conserva documentado. |

El control §6.2 B y M1 son **parecidos, no idénticos**: ambos separan apuntado y potencia, pero M1 propone confirmación explícita, primera liberación y segundo gesto; el control §6.2 B fija por quietud y continúa con tensado tras esa fijación, sin exigir esos dos gestos/liberaciones. No se interpreta «opción 1» como elección de M1: el PO especificó el control §6.2 B. El significado operativo de fases, cancelación y liberación r debe resolverse en el diseño futuro, no inferirse de M1.

**La regla 150 ms/P<0,60 no se traslada tal cual al control §6.2 B.** La potencia cambia de velocidad a distancia y la pausa inicial de 120 ms es el propio gesto de fijación: no se puede tratar toda pausa como olvido de una velocidad pasada. La regla original y sus casos permanecen intactos para el método de potencia B en A′; su reinterpretación para el control §6.2 B queda **pendiente de decisión del PO en la siguiente entrega propuesta**. No se relaja, no se sustituye por otra cifra y no se da por resuelta.

## 3. Alternativas propias M1/M2/M3: conservadas y descartadas por ahora

Son las hipótesis de interacción de la versión revisada por Claude, ahora descartadas por el PO para investigar. Se conservan con sus ventajas, riesgos y evaluación potencial; no son el control §6.2 B ni candidatos autorizados para una sesión. Cada una necesita una autorización que concrete semántica, cambios permitidos y pruebas antes de escribir código. Las bandas existentes suave 0 ≤ P < 0,25, media 0,25 ≤ P < 0,60, fuerte 0,60 ≤ P ≤ 1 y la advertencia de saturación>20 % permanecen como instrumentos declarados, sin convertirse en criterios nuevos de la spec.

Las referencias al riesgo de 150 ms en M1/M2/M3 describen la comparación original con el **método de potencia B por velocidad**; no definen ni trasladan esa regla al control §6.2 B.

### M1. Apuntado confirmado y gesto de potencia separado

**Interacción propuesta:** primer arrastre para elegir la mira; una confirmación explícita cierra la fase de apuntado. Un segundo gesto corto determina la potencia; la mira ya confirmada no se mueve con él. La confirmación debe diferenciarse del swing: la primera liberación no sería el golpe, y la liberación del segundo gesto sería r. No se propone que un segundo dedo se promocione automáticamente ni un selector de intención.

**Ventajas:** el PO puede apuntar rápido o lento sin congelarse por generar fuerza; el gesto de potencia no tiene que transportar la mira. La interfaz puede explicar por separado «elige dónde» y «elige cuánto».

**Riesgos y decisiones necesarias:** añade una acción y puede costar tiempo antes del contacto; una mira vieja puede sobrevivir indebidamente a un lanzamiento, pausa o pérdida de foco si no se invalida. Hace falta decidir cómo se corrige/cancela una confirmación y cómo se distinguen N1/N2 en cada fase. **Contradice la continuidad de A′ y requiere revisar con el PO el significado de liberación/r y propósito al apoyar**, antes de autorizar un cambio normativo. No basta añadir un botón a la vista actual.

**Regla de riesgo:** el movimiento de la fase de apuntado no debe alimentar el estimador de potencia. Eso separaría una fuente de interferencia, pero **el arrastre rápido + pausa 150 ms en la fase de potencia también debe cumplir P<0,60**, con los mismos tres casos. La confirmación explícita no exime a un golpe fuerte retenido de esta prueba. No se utiliza «ya había confirmado» para omitirla.

**Cómo evaluarla:** en una futura comparación autorizada, contar acciones, intentos de confirmar por error, pérdida de objetivo entre fases y dificultad de llegar a las tres bandas. Pedir apuntar rápido en la primera fase y una potencia suave en la segunda, además de ejecutar los tres casos de riesgo sin cambios sobre el estimador del segundo gesto. Registrar tiempos de ambas liberaciones y dejar explícito cuál es r; no implementar contacto de S2.

### M2. Un solo gesto: mira continua y fijación al soltar, sin fijación por Flick

**Estado: descartada por ahora; equivale en la práctica a la v0.1 descartada por §6.2.**

**Interacción propuesta:** la mira sigue el desplazamiento durante todo el arrastre; se fija en la última posición válida al soltar. Flick deja de cerrar el apuntado. La potencia se calcula al soltar mediante el método autorizado para la comparación. **No equivale a activar aim.freezeOnRelease en el código actual**, porque ahí Flick conserva prioridad.

**Ventajas:** elimina la instrucción de apuntar lento para evitar una fijación temprana; mantiene una sola liberación y una sola autoridad de la intención final. Permite estudiar si el problema principal es cuándo deja de moverse la mira, manteniendo inicialmente el estimador separado como variable controlada.

**Riesgos y decisiones necesarias:** la aceleración para dar fuerza también puede desplazar el objetivo justo antes de soltar; puede cambiar un centro deseado por tin/out. La liberación puede traer la misma marca de tiempo que la muestra anterior: se necesita acordar la última posición válida sin inventar velocidad. Se pierde el efecto de fijar antes del movimiento final que motivó el lookback. La [propuesta r2, §4.2](../superpowers/specs/2026-10-24-first-shot-design.md) dice que tras soltar la mira «permanece en el punto fijado»; §11.4 prescribe congelación por Flick y también dice «la mira de intención queda fija donde se soltó»; §11.6, **M06**, dice «la mira y la potencia se congelan al soltar». Deben leerse junto a §6.2 de la spec: las frases sobre el estado posterior a soltar no eliminan por sí mismas la protección por Flick ni autorizan volver a v0.1. M2 queda descartada por ahora; se conserva la tensión textual para el diseño, **sin resolverla cambiando spec, propuesta o plan aquí**.

**Regla de riesgo:** cambiar el evento de fijación por sí solo no altera la memoria de potencia. Con el método de potencia B sin cambios, los mismos tres perfiles de 150 ms deben seguir por debajo de 0,60; el replay de la mira no demuestra esa seguridad física. En la prueba de apuntar rápido/soltar suave se medirán separadamente potencia y error de mira; no se contarán los objetivos fuera de cancha como éxitos por tener P baja.

**Cómo evaluarla:** mismos objetivos solicitados y misma intención de potencia que la referencia; registrar objetivo antes de la aceleración y al soltar, desviación, invalidación y potencia por intento. Comparar si desaparece la fijación prematura y si el error final empeora. Antes de cualquier sesión, pruebas autorizadas deben preservar N1/N2, cancelaciones, primer dedo y lanzamiento, y resolver el caso terminal dt=0 de ambas rutas. Sin suavizar u ocultar desplazamientos en esta comparación.

### M3. Mantener Flick y revisar su sensibilidad como comparación limitada

**Interacción propuesta:** conservar el gesto único y la fijación automática, pero estudiar un umbral de Flick más alto como variable independiente, con un candidato concreto predeclarado por el PO **antes** de calcularlo o probarlo. No se fija aquí ningún valor ni se reduce la confirmación de dos intervalos. Potencia, ganancia, lookback, histéresis y demás variables quedarían controladas en esa comparación; un cambio adicional necesita otra declaración.

**Ventajas:** mantiene una interacción conocida y permite comprobar si los cruces que cortan el apuntado se pueden evitar sin otro paso explícito. Aísla el papel del umbral de mira frente a la escala de potencia, que usa otro cálculo.

**Riesgos y límite de la hipótesis:** sigue acoplando «rápido» a fijar; un fuerte que cruce el nuevo umbral volverá a congelarse y uno que no lo cruce dependerá de la semántica de Release autorizada. Puede volver la fijación más dependiente de variaciones entre intervalos y dejar de reconocer flicks deliberados; es un riesgo a evaluar, no un resultado medido. **No garantiza resolver el relato del PO ni la saturación**; no se presenta como solución suficiente. Si aumentar el umbral solo obliga a reaprender otra frontera, el resultado debe quedar visible. No se busca por tanteo un valor favorable ni se cambia el rango del catálogo o el umbral vigente en esta entrega.

**Regla de riesgo:** el umbral de mira no es la escala de potencia. Un umbral mayor no autoriza P fuerte después de 150 ms inmóvil. Deben pasar exactamente los tres casos originales y la futura tanda física, conservando el etiquetado de potencia accidental independiente de si la mira se fijó.

**Cómo evaluarla:** con un único candidato autorizado y sin ajuste en sesión, comparar porcentaje/instante de Flick, objetivo al cruce y al soltar, banda pedida y observación literal del PO sobre tener que apuntar lento. Verificar las pausas de 25–50 ms y 100 ms como pruebas separadas del riesgo de 150 ms, sin reemplazar este último. Si la mecánica continúa dependiendo de lento→rápido, documentarlo aunque el porcentaje de fijación baje.

### Estado de las opciones tras C5

| Opción | Decisión registrada |
|---|---|
| Control §6.2 B | Investigar “apuntar, fijar y tensar”; no es una autorización de diseño/plan nuevo, implementación o sesión. |
| Control A′ | Conservar como referencia, sin eliminar código. |
| M1, M2, M3 y control §6.2 C | Descartados por ahora, conservados en este documento. |

La pregunta de elección no queda abierta: la respuesta literal del PO es «Vamos con la opción 1», con el alcance específico del control §6.2 B indicado en su decisión. Los detalles pendientes se enumeran en “Siguiente entrega propuesta (no autorizada)”.

## 4. Observaciones de Claude registradas sin corrección de código

**(a) dt=0 en potencia.** AimModel trata Ended con timestamp igual como liberación cuando freezeOnRelease está activo ([líneas 52–57](../../Assets/SquashClash/Core/Shot/AimModel.cs#L52-L57)). [GesturePreview](../../Assets/SquashClash/Runtime/Shot/GesturePreview.cs#L94-L109), en cambio, solo exime el duplicado Stationary de igual posición: otro dt=0 marca repeated-time y entrega P=0 conservadora. [PeakPowerEstimator](../../Assets/SquashClash/Core/Shot/PeakPowerEstimator.cs#L27-L31) también invalida ese Ended si se llama directamente con el método de potencia B. Puede quedar una mira fijada por Release y una potencia fuerte anulada. **No apareció en los 49 gestos; es una observación de código de Claude, sin nuevo test ni reproducción ejecutada aquí.** Una futura PR de código autorizada necesitará TDD que cubra la ruta integrada y el estimador, conserve el apoyo temporal real y distinga un terminal duplicado de muestras contradictorias/no monótonas; no basta volver a probar solo AimModel. No se silencia la marca conservadora ni se inventa un intervalo positivo en este documento.

**(b) Protocolo y configuración.** 11:53:22.659306Z fue el **primer lanzamiento tras el reinicio**, con P0 y repeat=3. Después hubo relanzamientos cada 3 s. El primer gesto del cuarto bloque, 11:53:34.664976Z, ocurrió **1,2 ms después de uno de esos relanzamientos** y terminó SystemCanceled. La diferencia de 12,006 s respecto al primero no significa que hubieran pasado 12 s sin relanzar. Esta precisión procede de la revisión de Claude; no se han recalculado las trazas. No se verificó el hash antes del bloque. En futuras sesiones se debe comprobar **hash efectivo, preset, cero pendientes y repetición=0 antes de cada bloque**, incluidos reinicios, cambios de fecha o interrupciones. Como mejora futura se puede valorar mostrar preset/hash en la línea compacta; [hoy solo muestra estado y P](../../Assets/SquashClash/DevPanel/ShotOverlay.cs#L52-L59). **No se implementa ni se exige ampliar la línea actual**: la comprobación del protocolo puede hacerse desde DEV y registros, cerrando DEV antes de medir.

**(c) Mecánica.** Se registra la interpretación Claude/PO del acoplamiento de apuntado y potencia, apoyada por Flick 10/10 en fuertes y los relatos literales. La regla de riesgo llevó a escala/memoria cortas, mientras la saturación y el control medio físicos impiden dar por validado ese compromiso. Es motivo de las alternativas anteriores; no autoriza recalibrar parámetros ni atribuir el fallo solo a la escala.

## 5. Protocolo futuro: referencia conservada, no sesión autorizada

### Antes de pedir gestos

1. **Autorización propia del PO:** diseño previo del control §6.2 B, semántica de fijación/tensado/liberación y comparación con A′ como referencia, con candidato y orden de bloques predeclarados. La elección para investigar no autoriza ejecutar este protocolo. Cualquier excepción a A′/propuesta/plan se resuelve antes de una PR de código; sin reescribirlos para hacer pasar datos. Separar decisión de interacción de selección de escala/estimador. No usar el preset 08b6 como candidato ya aceptado.
2. Si requiere cambios de código, autorización y PR aparte con TDD, revisión de Claude y comprobaciones apropiadas **antes** de sesión. N1/N2 y el caso dt=0 integrado deben resolverse en el diseño y las pruebas que se autoricen. Para el control §6.2 B, primero debe resolverse con el PO la reinterpretación de 150 ms/P<0,60: no se hereda automáticamente la condición del método de potencia B por velocidad. La regla original se conserva intacta para la referencia a la que aplica; no se afirma que el nuevo control la supere. Sin esas decisiones previas, no hay sesión ni valores nuevos improvisados. No ejecutar S2 para demostrar la mecánica.
3. Ninguna instalación está autorizada aquí. Si otra decisión la autoriza, **renovar antes el perfil y comprobar su vigencia efectiva**; el registrado vence 2026-10-28T14:59:31Z. Respaldar P0-calib-1,Po-1,last-active y verificar los tres después de instalar. Identificar build y origen de producto exactos; no asumir que una app antigua contiene la alternativa.
4. Explicar físicamente la interacción y pedir al PO identificar la mira y sus estados antes de registrar. Aclarar: S1 no golpea la pelota. Declarar una sola mano para todos los intentos comparables; anotar cualquier cambio, interrupción o aprendizaje. Si la comparación pretende mm físicos, completar primero la comprobación con regla y declarar el factor; sin ella, mantenerlo como escala reportada.
5. Aplicar temporalmente la configuración autorizada, **launcher.repeatSeconds=0 sin guardar**, relanzar y comprobar. Mantener la selección persistente P0; no reiniciar inadvertidamente un ajuste solo en memoria. **Antes de cada bloque**, capturar preset, hash efectivo esperado, cero pendientes, build y parámetros críticos de mira/potencia/repetición. Si no coincide, **no comenzar**; si se descubre después, conservar y marcar no comparable, detenerse para decidir. Cerrar DEV después de comprobar, sin cambios durante la serie. Valorar preset/hash visible solo mediante otra PR autorizada, sin hacerlo condición de código de este documento.

### Bloques y análisis que se predeclararán

Para cada variante que el PO autorice: diez suaves, diez medios, diez fuertes y diez de apuntar rápido/soltar suave, con **la misma mano declarada**, objetivos y explicación consistentes. Las opciones de dos fases deben declarar qué acciones cuentan como un intento, cuándo comienzan las muestras de potencia y qué liberación es r; no sumar fases independientes como dos aciertos. Antes de empezar se decidirá el orden entre variantes para declarar aprendizaje/fatiga y el esfuerzo total; no se elige orden mirando el resultado. Si hay práctica no medida, etiquetarla por separado y no sustituir después intentos del bloque formal.

En una variante que conserve fijación por Flick, recordar literalmente antes del cuarto bloque: «Si apuntas muy rápido, la mira se fijará en ese momento (rombo amarillo); es el comportamiento actual. Lo que evaluamos es la potencia al soltar.» Para otra semántica, acordar y mostrar su explicación propia **antes** de medir, sin prometer que la mira se moverá después de fijarse. La tanda natural de apuntar rápido no certifica por sí sola los 150 ms exactos. Las tres contrapruebas originales del método de potencia B siguen separadas, con pausa/umbral intactos, en la referencia correspondiente. Las pruebas de riesgo propias del control §6.2 B dependen de la reinterpretación pendiente del PO; no se dan por definidas en este protocolo.

Conservar todos los intentos, toques, cancelaciones y parciales; numerar por variante/bloque y no repetir para sustituir fallos. Registrar intenciones solicitadas, hash de gesto y lanzamiento, rawSamples, potencia/base temporal/soporte, salidas conservadoras por motivo, tipo y momento de fijación, saltos, objetivo antes de acelerar y final, error frente a lo solicitado, tin/out e interrupciones. Si la implementación elegida requiere campos aún inexistentes, deberán autorizarse en su PR de código; no se simula que ya se guardan todos en la app actual. Un replay de la mira de trazas antiguas no aporta un nuevo objetivo deseado ni intención del PO.

Informar por serie: n y descartes de comparabilidad sin borrar intentos, P mínima/mediana/máxima, desviación estándar/IQR, porcentaje en banda solicitada y fijación por causa, saturación y fuertes fuera de la serie fuerte. Mantener los ceros conservadores identificados; los de cancelación no son una medida de potencia. Comparar mira e intensidad **por separado**: bajar Flick no acredita mejor potencia y P baja no acredita apuntar bien. No usar solo promedios para ocultar medios fuertes, saturación o pérdida de objetivo. Mantener bandas y advertencia>20 % intactas. Conservar P<0,60 tras 150 ms donde aplica al método de potencia B por velocidad; no trasladarlo al control §6.2 B sin la decisión pendiente del PO. No declarar Pass con una tanda no comparable ni con una regla de riesgo aún sin resolver.

Preguntar por separado y citar literalmente:

- ¿Consigues las tres intensidades de forma voluntaria?
- ¿Apareció algún golpe fuerte que no buscabas?
- ¿Apareció algún golpe fuerte al apuntar rápido y soltar suave?
- ¿La mira queda donde quieres apuntar?
- ¿Se siente consistente entre intentos?

Si una respuesta habla de la mira en vez de potencia, conservarla sin convertirla en sí/no; cualquier aclaración será una respuesta nueva, no una corrección retrospectiva. Añadir una pregunta propia sobre el costo de interacción de la alternativa, sin sustituir las cinco anteriores.

### Cierre y criterio de parada

Ante un defecto o cambio de configuración, registrar y detener la medición sin corregir, reconfigurar o buscar otro candidato durante el bloque. Restaurar P0-calib-1 al terminar o abortar; comprobar **en un arranque nuevo** su hash esperado (actual fdd997f2b85a; verificarlo contra la build autorizada), cero pendientes y los tres archivos byte a byte iguales al respaldo, incluida selección. Guardar evidencia nueva separada de esta propuesta y de las campañas previas. El informe vuelve a Claude y PO; ni la sesión ni un resultado favorable autorizan aceptar S1, fusionar código o comenzar S2 automáticamente.

## 6. Siguiente entrega propuesta (no autorizada)

Una **PR documental de diseño y plan para el control §6.2 B**, con TDD previsto, es el siguiente trabajo propuesto, **no autorizado por esta decisión**. Debe resolver con el PO antes de implementación:

- **Dedo quieto:** definición operativa por velocidad/desplazamiento y duración inicial **120 ms [EXPERIMENTO]**. Riesgo de fijar sin querer al pausar mientras se apunta; no se inventan aquí umbrales de quietud ni se aprueba esa duración como definitiva.
- **Feedback de fijado:** destello visual de la spec y decisión sobre respuesta háptica; estados que el PO pueda distinguir y confirmar. No se implementa ni se aprueba una forma háptica aquí.
- **Tensado y potencia:** dirección del tensado, mapeo distancia→P, tope, zona muerta y escala en mm. La spec propone hacia abajo, tipo honda; el detalle del diseño queda por resolver, sin elegir valores en esta entrega.
- **Fases y cancelación:** N1/N2 en cada fase, cancelación voluntaria y del sistema, qué liberación es r, primer dedo y lanzamiento. No extrapolar las dos liberaciones de M1 al control §6.2 B.
- **Riesgo:** reinterpretación expresa por el PO de 150 ms/P<0,60 para potencia por distancia y pausa de fijación; conservar la regla original para su contexto, sin relajarla ni declararla resuelta.
- **Comparación futura:** A′ como referencia conservada, mismas cuatro series (suaves/medios/fuertes/apuntar rápido y soltar suave), misma mano declarada y **hash efectivo comprobado antes de cada bloque**. Planificar la separación de datos y el orden, sin iniciar una sesión ni elegir ahora parámetros para la referencia.
- **Textos que requerirían aprobación expresa:** identificar los cambios propuestos en First Shot r2 §4.2, C5 (§8/§9), §11.4 y la aclaración M06, así como T1.2 y la cobertura de C5 en el plan. Presentar qué se modificaría y por qué en esa futura PR; **no tocar spec v0.3** y no editar propuesta r2 o planes en esta entrega.

El TDD previsto debe describir esqueletos compilables y RED por aserciones antes de implementación, y casos para quietud/fijación involuntaria, separación entre fijación y tensado, mapeo y límites de distancia, N1/N2, r y cancelaciones por foco/dedo/lanzamiento; el caso terminal dt=0 de potencia sigue pendiente. Las aserciones de riesgo del nuevo control requieren primero la decisión del PO; no se escriben tests ni se fijan aquí resultados esperados con cifras nuevas. Este apartado no ejecuta ni autoriza esa PR futura.

## 7. Estado de entrega

Solo esta propuesta, AGENTS.md y el registro de aprobaciones cambian. Sin código, tests, presets, defaults, spec, planes, criterios, bandas o umbrales nuevos; sin ejecución de suites/check-release, builds, instalaciones, sesiones o acceso al iPhone. Los datos originales y el preset experimental se conservan intactos.

Defaults **6095c0b38614**; proyección de las 58 claves anteriores **00a501ad7066**, 41 heredadas **0d39e6016bd8** y SimulationKey **166a96e319c7**, intactos. power.method=0 (A) y aim.freezeOnRelease=0 en defaults; preset 08b6fe90889b no promovido. Cierre técnico de PR #22 y archivo local registrados en aprobaciones. **PR documental lista para Claude, sin fusionar. Detenerse: S1 sigue sin aceptar; S2–S4/T8 no autorizados.**
