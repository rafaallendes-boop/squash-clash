# Propuesta documental: mecánica de apuntado y golpe rápido en S1

Fecha: 2026-10-27. Rama `firstshot/s1-gesture-mechanics-proposal`, desde main tras el merge **31bbe03ecc6d2772c713620a6332d5b84ddb0e75** de PR #22. Decisión del PO comunicada junto con la revisión de Claude del head **7bfea922f49fc094fa59601d4e36d7a8e87025f6**.

**No hay alternativa elegida ni implementación autorizada.** PR #22 se aprueba como entrega técnica experimental: B y aim.freezeOnRelease siguen desactivados por defecto, preset 08b6fe90889b sin promover, defaults 6095c0b38614 intactos. S1 y el candidato 300/0/60 no quedan aceptados. Esta entrega solo documenta diagnóstico, tres alternativas y un protocolo que exige autorización propia. No se cambia spec, planes, criterios, bandas ni umbrales; cualquier alternativa que los contradiga requerirá primero una decisión expresa del PO. S2–S4/T8 no autorizados.

## 1. Qué demuestra la evidencia y qué queda abierto

Marco vigente: [propuesta r2, §11.4](../superpowers/specs/2026-10-24-first-shot-design.md) y [plan First Shot r2, Tarea 1.2](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Se citan para identificar las decisiones que una futura autorización tendría que resolver; permanecen intactos.

Fuentes: [informe de calibración](2026-10-25-first-shot-s1-gesture-calibration.md), [49 intentos físicos conservados](../validation/evidence/2026-10-27-s1-gesture-calibration-physical.json), [selección y riesgos sintéticos](../validation/evidence/2026-10-26-s1-gc-resolution.json), [grid offline anterior](../validation/evidence/2026-10-25-s1-gesture-calibration-offline.json). Se usan datos ya registrados; **no se ha hecho replay nuevo, medición, suite, build, instalación, sesión ni acceso al iPhone** en esta entrega. No se mezclan las mediciones físicas con el cálculo offline ni con las pruebas sintéticas anteriores.

| Observación | Evidencia y alcance |
|---|---|
| Fijación de mira y gesto fuerte comparten la velocidad del dedo | En los 10 fuertes comparables, la mira se fijó por **Flick 10/10**; en medios 7/10 por Flick y 3/10 por Release; suaves 10/10 por Release. Todos 08b6fe90889b/cero pendientes. |
| El PO debe separar mentalmente dos acciones dentro del gesto | Literal: «si voy fuerte se queda al medio el rombo» y «debo apuntar lento y acelerar». La respuesta completa a tres intensidades fue «No, la de golpear rápido me cuesta lograrlo ya que debo apuntar lento y acelerar». |
| Potencia alta no acredita control de tres intensidades | Suaves 10/10 en banda, medios **3/10**, fuertes 8/10; siete medios en banda fuerte. Saturación fuerte **3/10=30 %**, supera advertencia 20 %. No se reclasifican como accidentes sin juicio explícito del PO. |
| El cuarto bloque no valida el riesgo físico de B | Sus 19 intentos usaron P0 / fdd997f2b85a, no 08b6fe90889b; riesgo físico de B **NotEvaluated**. Nueve intentos y después diez, con siete cancelaciones, todos conservados. |
| Pocos datos no equivalen a seguridad o consistencia | No hay una segunda campaña controlada ni atribución causal entre alternativas. DPI 460 reportado sin regla física; mm/s no son una calibración física verificada. |

### Por qué la mira puede quedarse cerca del punto inicial

[AimModel.Update](../../Assets/SquashClash/Core/Shot/AimModel.cs#L43-L92) mueve la mira a partir del desplazamiento desde Began y calcula velocidad entre muestras. Dos intervalos consecutivos sobre 250 mm/s confirman Flick; la fecha del cruce se conserva en la banda de histéresis y solo se borra por debajo de 210. La fijación recupera la historia en cruce−50 ms; sin historia suficiente usa el primer punto válido. Una vez Frozen, se ignoran nuevas actualizaciones del gesto.

Por tanto, un arranque rápido puede fijar una posición anterior al desplazamiento que el PO quería usar para apuntar, incluso cerca del inicio. Es una explicación compatible con el código y el relato; **no se afirma que los 10 fuertes se fijaran exactamente en el centro** ni que todo Flick sea indeseado. freezeOnRelease=1 solo fija al soltar si aún se está Aiming: **no elimina ni pospone el Flick anterior**. Subir la potencia después de una fijación no reposiciona la mira.

La lectura de Claude/PO es un **acoplamiento de apuntado y potencia en un mismo gesto**. La velocidad decide a la vez cuándo dejar de apuntar y cuánto vale el golpe. Cambiar únicamente la escala de potencia no cambia la regla que fija la mira. El «si» del PO sobre apuntar donde quiere no cancela su «No» sobre tres intensidades ni la dificultad de la secuencia lenta→rápida.

### Tensión entre riesgo, memoria y saturación

El requisito eliminatorio se conserva: en los mismos tres perfiles sintéticos de arrastre rápido, **150 ms inmóvil y después soltar**, cada P debe ser **<0,60**. De las cuatro combinaciones supervivientes predeclaradas, solo 300/0/60 lo pasó: 10,944/23,101/25,999 %. Las 400/0/120; 400/0/180 y 400/10/180 fallaron uno o más casos. El único superviviente usó una escala de 300 mm/s y memoria de 60 ms; no se eligió por una nueva preferencia física del PO.

La memoria corta reduce la potencia de una aceleración antigua, pero también penaliza al fuerte que se detiene antes de soltar: el costo sintético aceptado fue 53,959→31,267 % para 500 mm/s detenido 100 ms. La escala corta permite alcanzar la banda alta, pero en esta tanda hubo 30 % de saturación fuerte y siete medios en fuerte. **La selección que resolvió el riesgo sintético no resolvió la mecánica ni validó la escala físicamente.** No se deduce que subir o bajar una constante arregle ambos problemas. No se relaja 150 ms/P<0,60 ni se amplía el grid a posteriori; no se aplica otro candidato en este documento.

## 2. Tres alternativas para que el PO decida qué investigar

Son hipótesis de interacción, sin valores nuevos aplicados ni selección automática. Cada una necesita una autorización que concrete semántica, cambios permitidos y pruebas antes de escribir código. Las bandas existentes suave 0 ≤ P < 0,25, media 0,25 ≤ P < 0,60, fuerte 0,60 ≤ P ≤ 1 y la advertencia de saturación>20 % permanecen como instrumentos declarados, sin convertirse en criterios nuevos de la spec.

### A. Apuntado confirmado y gesto de potencia separado

**Interacción propuesta:** primer arrastre para elegir la mira; una confirmación explícita cierra la fase de apuntado. Un segundo gesto corto determina la potencia; la mira ya confirmada no se mueve con él. La confirmación debe diferenciarse del swing: la primera liberación no sería el golpe, y la liberación del segundo gesto sería r. No se propone que un segundo dedo se promocione automáticamente ni un selector de intención.

**Ventajas:** el PO puede apuntar rápido o lento sin congelarse por generar fuerza; el gesto de potencia no tiene que transportar la mira. La interfaz puede explicar por separado «elige dónde» y «elige cuánto».

**Riesgos y decisiones necesarias:** añade una acción y puede costar tiempo antes del contacto; una mira vieja puede sobrevivir indebidamente a un lanzamiento, pausa o pérdida de foco si no se invalida. Hace falta decidir cómo se corrige/cancela una confirmación y cómo se distinguen N1/N2 en cada fase. **Contradice la continuidad de A′ y requiere revisar con el PO el significado de liberación/r y propósito al apoyar**, antes de autorizar un cambio normativo. No basta añadir un botón a la vista actual.

**Regla de riesgo:** el movimiento de la fase de apuntado no debe alimentar el estimador de potencia. Eso separaría una fuente de interferencia, pero **el arrastre rápido + pausa 150 ms en la fase de potencia también debe cumplir P<0,60**, con los mismos tres casos. La confirmación explícita no exime a un golpe fuerte retenido de esta prueba. No se utiliza «ya había confirmado» para omitirla.

**Cómo evaluarla:** en una futura comparación autorizada, contar acciones, intentos de confirmar por error, pérdida de objetivo entre fases y dificultad de llegar a las tres bandas. Pedir apuntar rápido en la primera fase y una potencia suave en la segunda, además de ejecutar los tres casos de riesgo sin cambios sobre el estimador del segundo gesto. Registrar tiempos de ambas liberaciones y dejar explícito cuál es r; no implementar contacto de S2.

### B. Un solo gesto: mira continua y fijación al soltar, sin fijación por Flick

**Interacción propuesta:** la mira sigue el desplazamiento durante todo el arrastre; se fija en la última posición válida al soltar. Flick deja de cerrar el apuntado. La potencia se calcula al soltar mediante el método autorizado para la comparación. **No equivale a activar aim.freezeOnRelease en el código actual**, porque ahí Flick conserva prioridad.

**Ventajas:** elimina la instrucción de apuntar lento para evitar una fijación temprana; mantiene una sola liberación y una sola autoridad de la intención final. Permite estudiar si el problema principal es cuándo deja de moverse la mira, manteniendo inicialmente el estimador separado como variable controlada.

**Riesgos y decisiones necesarias:** la aceleración para dar fuerza también puede desplazar el objetivo justo antes de soltar; puede cambiar un centro deseado por tin/out. La liberación puede traer la misma marca de tiempo que la muestra anterior: se necesita acordar la última posición válida sin inventar velocidad. Se pierde el efecto de fijar antes del movimiento final que motivó el lookback. **Se aparta del congelamiento normativo por Flick de propuesta §11.4 / plan T1.2**; exige decisión expresa del PO antes de modificarlo, sin editar esos documentos aquí.

**Regla de riesgo:** cambiar el evento de fijación por sí solo no altera la memoria de potencia. Con B sin cambios, los mismos tres perfiles de 150 ms deben seguir por debajo de 0,60; el replay de la mira no demuestra esa seguridad física. En la prueba de apuntar rápido/soltar suave se medirán separadamente potencia y error de mira; no se contarán los objetivos fuera de cancha como éxitos por tener P baja.

**Cómo evaluarla:** mismos objetivos solicitados y misma intención de potencia que la referencia; registrar objetivo antes de la aceleración y al soltar, desviación, invalidación y potencia por intento. Comparar si desaparece la fijación prematura y si el error final empeora. Antes de cualquier sesión, pruebas autorizadas deben preservar N1/N2, cancelaciones, primer dedo y lanzamiento, y resolver el caso terminal dt=0 de ambas rutas. Sin suavizar u ocultar desplazamientos en esta comparación.

### C. Mantener Flick y revisar su sensibilidad como comparación limitada

**Interacción propuesta:** conservar el gesto único y la fijación automática, pero estudiar un umbral de Flick más alto como variable independiente, con un candidato concreto predeclarado por el PO **antes** de calcularlo o probarlo. No se fija aquí ningún valor ni se reduce la confirmación de dos intervalos. Potencia, ganancia, lookback, histéresis y demás variables quedarían controladas en esa comparación; un cambio adicional necesita otra declaración.

**Ventajas:** mantiene una interacción conocida y permite comprobar si los cruces que cortan el apuntado se pueden evitar sin otro paso explícito. Aísla el papel del umbral de mira frente a la escala de potencia, que usa otro cálculo.

**Riesgos y límite de la hipótesis:** sigue acoplando «rápido» a fijar; un fuerte que cruce el nuevo umbral volverá a congelarse y uno que no lo cruce dependerá de la semántica de Release autorizada. Puede volver la fijación más dependiente de variaciones entre intervalos y dejar de reconocer flicks deliberados; es un riesgo a evaluar, no un resultado medido. **No garantiza resolver el relato del PO ni la saturación**; no se presenta como solución suficiente. Si aumentar el umbral solo obliga a reaprender otra frontera, el resultado debe quedar visible. No se busca por tanteo un valor favorable ni se cambia el rango del catálogo o el umbral vigente en esta entrega.

**Regla de riesgo:** el umbral de mira no es la escala de potencia. Un umbral mayor no autoriza P fuerte después de 150 ms inmóvil. Deben pasar exactamente los tres casos originales y la futura tanda física, conservando el etiquetado de potencia accidental independiente de si la mira se fijó.

**Cómo evaluarla:** con un único candidato autorizado y sin ajuste en sesión, comparar porcentaje/instante de Flick, objetivo al cruce y al soltar, banda pedida y observación literal del PO sobre tener que apuntar lento. Verificar las pausas de 25–50 ms y 100 ms como pruebas separadas del riesgo de 150 ms, sin reemplazar este último. Si la mecánica continúa dependiendo de lento→rápido, documentarlo aunque el porcentaje de fijación baje.

### Qué información falta para elegir

| Decisión del PO | Qué discrimina |
|---|---|
| ¿Acepta dos acciones y qué evento representa el golpe? | Viabilidad de A y su costo de interacción/tiempo. |
| ¿Prefiere seguir corrigiendo la mira hasta soltar, con posible arrastre al acelerar? | Beneficio/riesgo de B frente a la fijación anticipada. |
| ¿Quiere comprobar primero el límite de un Flick menos sensible? | Alcance y candidato único para C, sin confundirlo con escala de potencia. |

**Este documento no responde esas preguntas por el PO ni elige una alternativa.** Tampoco autoriza una PR de código o una sesión por el hecho de presentar un protocolo.

## 3. Observaciones de Claude registradas sin corrección

**(a) dt=0 en potencia.** AimModel trata Ended con timestamp igual como liberación cuando freezeOnRelease está activo ([líneas 52–57](../../Assets/SquashClash/Core/Shot/AimModel.cs#L52-L57)). [GesturePreview](../../Assets/SquashClash/Runtime/Shot/GesturePreview.cs#L94-L109), en cambio, solo exime el duplicado Stationary de igual posición: otro dt=0 marca repeated-time y entrega P=0 conservadora. [PeakPowerEstimator](../../Assets/SquashClash/Core/Shot/PeakPowerEstimator.cs#L27-L31) también invalida ese Ended si se llama directamente con método B. Puede quedar una mira fijada por Release y una potencia fuerte anulada. **No apareció en los 49 gestos; es una observación de código de Claude, sin nuevo test ni reproducción ejecutada aquí.** Una futura PR de código autorizada necesitará TDD que cubra la ruta integrada y el estimador, conserve el apoyo temporal real y distinga un terminal duplicado de muestras contradictorias/no monótonas; no basta volver a probar solo AimModel. No se silencia la marca conservadora ni se inventa un intervalo positivo en este documento.

**(b) Protocolo y configuración.** El primer intento del cuarto bloque fue 11:53:34.664976Z, **12,006 s después** del lanzamiento P0 de 11:53:22.659306Z. No se verificó el hash antes del bloque. En futuras sesiones se debe comprobar **hash efectivo, preset, cero pendientes y repetición=0 antes de cada bloque**, incluidos reinicios, cambios de fecha o interrupciones. Como mejora futura se puede valorar mostrar preset/hash en la línea compacta; [hoy solo muestra estado y P](../../Assets/SquashClash/DevPanel/ShotOverlay.cs#L52-L59). **No se implementa ni se exige ampliar la línea actual**: la comprobación del protocolo puede hacerse desde DEV y registros, cerrando DEV antes de medir.

**(c) Mecánica.** Se registra la interpretación Claude/PO del acoplamiento de apuntado y potencia, apoyada por Flick 10/10 en fuertes y los relatos literales. La regla de riesgo llevó a escala/memoria cortas, mientras la saturación y el control medio físicos impiden dar por validado ese compromiso. Es motivo de las alternativas anteriores; no autoriza recalibrar parámetros ni atribuir el fallo solo a la escala.

## 4. Protocolo propuesto para una futura sesión, no autorizada

### Antes de pedir gestos

1. **Autorización propia del PO:** alternativa concreta, semántica de fijación/confirmación/liberación, variante de referencia, un candidato y orden de bloques predeclarados. Cualquier excepción a A′/propuesta/plan se resuelve antes de una PR de código; sin reescribirlos para hacer pasar datos. Separar decisión de interacción de selección de escala/estimador. No usar el preset 08b6 como candidato ya aceptado.
2. Si requiere cambios de código, autorización y PR aparte con TDD, revisión de Claude y comprobaciones apropiadas **antes** de sesión. El riesgo original 150 ms/P<0,60, N1/N2 y el caso dt=0 integrado son obligaciones previas, no ajustes de esta PR documental. Si ninguna variante supera la condición eliminatoria, detenerse sin sesión ni valores nuevos improvisados. No ejecutar S2 para demostrar la mecánica.
3. Ninguna instalación está autorizada aquí. Si otra decisión la autoriza, **renovar antes el perfil y comprobar su vigencia efectiva**; el registrado vence 2026-10-28T14:59:31Z. Respaldar P0-calib-1,Po-1,last-active y verificar los tres después de instalar. Identificar build y origen de producto exactos; no asumir que una app antigua contiene la alternativa.
4. Explicar físicamente la interacción y pedir al PO identificar la mira y sus estados antes de registrar. Aclarar: S1 no golpea la pelota. Declarar una sola mano para todos los intentos comparables; anotar cualquier cambio, interrupción o aprendizaje. Si la comparación pretende mm físicos, completar primero la comprobación con regla y declarar el factor; sin ella, mantenerlo como escala reportada.
5. Aplicar temporalmente la configuración autorizada, **launcher.repeatSeconds=0 sin guardar**, relanzar y comprobar. Mantener la selección persistente P0; no reiniciar inadvertidamente un ajuste solo en memoria. **Antes de cada bloque**, capturar preset, hash efectivo esperado, cero pendientes, build y parámetros críticos de mira/potencia/repetición. Si no coincide, **no comenzar**; si se descubre después, conservar y marcar no comparable, detenerse para decidir. Cerrar DEV después de comprobar, sin cambios durante la serie. Valorar preset/hash visible solo mediante otra PR autorizada, sin hacerlo condición de código de este documento.

### Bloques y análisis que se predeclararán

Para cada variante que el PO autorice: diez suaves, diez medios, diez fuertes y diez de apuntar rápido/soltar suave, con **la misma mano declarada**, objetivos y explicación consistentes. Las opciones de dos fases deben declarar qué acciones cuentan como un intento, cuándo comienzan las muestras de potencia y qué liberación es r; no sumar fases independientes como dos aciertos. Antes de empezar se decidirá el orden entre variantes para declarar aprendizaje/fatiga y el esfuerzo total; no se elige orden mirando el resultado. Si hay práctica no medida, etiquetarla por separado y no sustituir después intentos del bloque formal.

En una variante que conserve fijación por Flick, recordar literalmente antes del cuarto bloque: «Si apuntas muy rápido, la mira se fijará en ese momento (rombo amarillo); es el comportamiento actual. Lo que evaluamos es la potencia al soltar.» Para otra semántica, acordar y mostrar su explicación propia **antes** de medir, sin prometer que la mira se moverá después de fijarse. La tanda natural de apuntar rápido no certifica por sí sola los 150 ms exactos: las tres contrapruebas originales siguen separadas, con pausa/umbral intactos.

Conservar todos los intentos, toques, cancelaciones y parciales; numerar por variante/bloque y no repetir para sustituir fallos. Registrar intenciones solicitadas, hash de gesto y lanzamiento, rawSamples, potencia/base temporal/soporte, salidas conservadoras por motivo, tipo y momento de fijación, saltos, objetivo antes de acelerar y final, error frente a lo solicitado, tin/out e interrupciones. Si la implementación elegida requiere campos aún inexistentes, deberán autorizarse en su PR de código; no se simula que ya se guardan todos en la app actual. Un replay de la mira de trazas antiguas no aporta un nuevo objetivo deseado ni intención del PO.

Informar por serie: n y descartes de comparabilidad sin borrar intentos, P mínima/mediana/máxima, desviación estándar/IQR, porcentaje en banda solicitada y fijación por causa, saturación y fuertes fuera de la serie fuerte. Mantener los ceros conservadores identificados; los de cancelación no son una medida de potencia. Comparar mira e intensidad **por separado**: bajar Flick no acredita mejor potencia y P baja no acredita apuntar bien. No usar solo promedios para ocultar medios fuertes, saturación o pérdida de objetivo. Mantener criterio de riesgo P<0,60 tras 150 ms, bandas y advertencia>20 % intactos, sin declarar Pass con una tanda no comparable.

Preguntar por separado y citar literalmente:

- ¿Consigues las tres intensidades de forma voluntaria?
- ¿Apareció algún golpe fuerte que no buscabas?
- ¿Apareció algún golpe fuerte al apuntar rápido y soltar suave?
- ¿La mira queda donde quieres apuntar?
- ¿Se siente consistente entre intentos?

Si una respuesta habla de la mira en vez de potencia, conservarla sin convertirla en sí/no; cualquier aclaración será una respuesta nueva, no una corrección retrospectiva. Añadir una pregunta propia sobre el costo de interacción de la alternativa, sin sustituir las cinco anteriores.

### Cierre y criterio de parada

Ante un defecto o cambio de configuración, registrar y detener la medición sin corregir, reconfigurar o buscar otro candidato durante el bloque. Restaurar P0-calib-1 al terminar o abortar; comprobar **en un arranque nuevo** su hash esperado (actual fdd997f2b85a; verificarlo contra la build autorizada), cero pendientes y los tres archivos byte a byte iguales al respaldo, incluida selección. Guardar evidencia nueva separada de esta propuesta y de las campañas previas. El informe vuelve a Claude y PO; ni la sesión ni un resultado favorable autorizan aceptar S1, fusionar código o comenzar S2 automáticamente.

## 5. Estado de entrega

Solo esta propuesta, AGENTS.md y el registro de aprobaciones cambian. Sin código, tests, presets, defaults, spec, planes, criterios, bandas o umbrales nuevos; sin ejecución de suites/check-release, builds, instalaciones, sesiones o acceso al iPhone. Los datos originales y el preset experimental se conservan intactos.

Defaults **6095c0b38614**; proyección de las 58 claves anteriores **00a501ad7066**, 41 heredadas **0d39e6016bd8** y SimulationKey **166a96e319c7**, intactos. power.method=0 (A) y aim.freezeOnRelease=0 en defaults; preset 08b6fe90889b no promovido. Cierre técnico de PR #22 y archivo local registrados en aprobaciones. **PR documental lista para Claude, sin fusionar. Detenerse: S1 sigue sin aceptar; S2–S4/T8 no autorizados.**
