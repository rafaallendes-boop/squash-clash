# Diseño y plan propuestos para el control §6.2 B

Fecha: 2026-10-27. Rama `firstshot/s1-control-b-design`, base main **188d59ec3bd5eb59b0340e9342e8a47afc94bdef**, tras cierre autorizado de PR #23 (head aprobado **f5b4beae3aa796c3c324efaed198ebe3f840c6b7**).

**Documento para decisión del PO, no implementación.** El PO eligió investigar el **control §6.2 B**, “apuntar, fijar y tensar”; A′ se conserva como referencia y por defecto. El **método de potencia B** por velocidad es otra cosa: sigue experimental/desactivado por defecto. Ninguna recomendación de Codex de este documento equivale a una decisión del PO. Todos los valores propuestos son **[EXPERIMENTO], no definitivos ni aplicados**. S1 sin aceptar; S2–S4/T8 no autorizados.

Fuente normativa: [spec v0.3 §6.2](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md), [First Shot r2 §4.2/C5/§11.4/M06](../superpowers/specs/2026-10-24-first-shot-design.md), [plan First Shot r2 T1.2](../superpowers/plans/2026-10-25-squash-clash-first-shot.md), [decisión del PO y propuesta de mecánica](2026-10-27-first-shot-s1-gesture-mechanics-proposal.md). Solo se citan; no se editan. Los resultados previos de PR #22 se conservan: riesgo físico del método de potencia B NotEvaluated, medios 3/10, saturación 30 % y juicio del PO de que A′ falla en su forma actual; no se convierte en Fail medido de calibración.

## Predeclaración del análisis offline descriptivo

**Se registra y se commitea antes de calcular resultados.** Fuentes inmutables: [138 gestos de revalidación n.º2](../validation/evidence/2026-10-25-s1-revalidation-2.json) y [49 de calibración](../validation/evidence/2026-10-27-s1-gesture-calibration-physical.json). Se incluyen todos los intentos, series, incidentales y cancelaciones: no se selecciona por potencia ni éxito. No se supone que ambos conjuntos tengan iguales configuración, mano o propósito.

Se estimará compatibilidad de pausas, no una tasa de fijación involuntaria ni rendimiento del control §6.2 B. Datos registrados con A′, escala DPI reportada, sin nueva verificación física. **No es medición nueva ni simulación del control elegido.**

Método predeclarado, sin ajuste posterior:

- Orden de llegada de rawSamples; mmPerPx original congelado por gesto. Solo Began/Moved/Stationary pueden sostener la ventana; Ended/Canceled no completan una quietud ni se cruzan límites de gesto. No se generan muestras por frame, no se interpola ni se prolonga quietud a través del silencio.
- Ventana anclada en una muestra real: duración entre sus extremos **≥120 ms**, al menos 3 timestamps distintos y todos los intervalos positivos **≤51 ms**. Límites inclusivos, tolerancia aritmética1e−9 s/1e−6 mm o mm/s para representación numérica, no cambio del umbral. Registros no finitos, tiempos hacia atrás o dt=0 contradictorio rompen la ventana; un duplicado Stationary idéntico se ignora sin sumar tiempo/muestra.
- **Principal QVR:** cada velocidad entre muestras ≤15 mm/s **y** cada posición a≤1 mm del ancla. **Comparador QV, ya declarado:** velocidad≤15 mm/s con el mismo apoyo temporal, sin radio. Duración 120/radio 1/velocidad 15/hueco 51 son valores analíticos exploratorios, no parámetros aprobados. No se probarán otros tras conocer el resultado.
- Un gesto cuenta como “compatible” si existe al menos una ventana; se publica la primera pareja de índices/timestamps, duración y máximos para trazabilidad, sin sumar ventanas solapadas como múltiples personas/intentos. No compatible significa “sin ventana observada con este apoyo”, no “nunca pausó”. Se informan muestras ausentes o inválidas separadas; no se eliminan intentos del denominador.
- Dos ámbitos: **interior completo del gesto** (puede incluir potencia después de fijar en A′; no prueba que se estuviera apuntando) y **apuntado observable**. En este último, cortar antes del FreezeTimeReal nativo si se conserva para Flick; si no hubo fijación antes de soltar (None/Release), usar el interior completo. Si hubo Flick y no se conserva ese timestamp, excluir solo de este ámbito y declarar su falta de observabilidad; no reconstruirlo mediante replay de AimModel. La fecha nativa del cruce puede anteceder a la confirmación: el corte es conservador.
- Presentar por fuente y ámbito: denominador total/observable, gestos compatibles con QVR/QV y porcentaje, falta de escala/muestras/timestamp de fijación. En el resultado por intento conservar hash, protocolo y outcome; no hacer equivalencia entre fuentes. No inferir intención ni causalidad. Si no aparece ninguna ventana, se registra 0 sin relajar el filtro.

La tabla agregada se incorporará aquí. Se declara un único JSON derivado nuevo, `docs/validation/evidence/2026-10-27-s1-control-b-quiet-analysis.json`, con hashes de fuentes, predeclaración, resultados por intento y agregados. Los originales quedan intactos; el script analítico transitorio no forma parte del producto ni se versiona.

Estado histórico en el commit **a37986b**, previo al cálculo: resultados todavía no calculados. El resultado posterior se presenta en §1, separado de esta predeclaración; las reglas analíticas no se han cambiado.

## 1. Dedo quieto, fijación y corrección

### Opciones para decisión del PO

| Opción | Ventaja | Riesgo |
|---|---|---|
| Q1: solo velocidad máxima entre muestras durante 120 ms | Fácil de explicar y causal. | Deriva lenta acumulada puede desplazar el objetivo mientras sigue cumpliéndose la velocidad. |
| Q2: velocidad **y radio respecto de un ancla**, con apoyo temporal real | Limita temblor instantáneo y deriva; deja trazabilidad de por qué se fijó. | Más exigente con muestras escasas; puede tardar en fijar o no fijar una quietud real. |
| Q3: fijar por un timeout sin movimientos recibidos | Reconoce una espera aunque no lleguen eventos. | Ausencia de eventos no prueba inmovilidad; confunde silencio del sensor/adaptador con intención. No equivale a una ventana medida. |

**Recomendación de Codex, pendiente del PO: Q2**, con velocidad≤15 mm/s, radio≤1 mm, duración inicial **120 ms [EXPERIMENTO]**, huecos≤51 ms y mínimo 3 timestamps distintos **[EXPERIMENTO]**. Son los valores exploratorios predeclarados, no una calibración elegida tras el análisis. El candidato a ventana empieza en una muestra real; exceso de velocidad/radio/hueco reinicia el candidato desde una nueva muestra válida. Se fija en la primera muestra real que acredita toda la duración, nunca hacia atrás ni por repetir la última posición en Update. Los 120 ms se miden en tiempo real entre timestamps táctiles, no en frames ni pasos de SimClock; pausa/foco invalidan la ventana. El momento de fijación es ese timestamp; la mira usa la posición válida de esa muestra, sin lookback de Flick para este control.

La captura seguiría la [fuente única de GesturePreview](../../Assets/SquashClash/Runtime/Shot/GesturePreview.cs#L71-L111): Input System ya produce TouchSample con fase, posición, tiempo y secuencia. El detector propuesto consumiría esos eventos una sola vez; no mezclaría polling, callbacks de acciones o muestras artificiales por frame. La escala mmPerPx se congela al apoyar, con su procedencia registrada. Timestamps no finitos/no monótonos o contradictorios no aportan quietud; terminales no completan la ventana. Un Stationary duplicado idéntico no suma muestras ni duración. No se presupone60/120 Hz ni que un dedo quieto genere eventos suficientes.

**Guardia propuesta adicional, no aprobada:** empezar a buscar fijación solo después de reconocer arrastre según N2 vigente (desplazamiento máximo del gesto≥3 mm). Protege el toque/espera inicial; una persona que apoya y no arrastra no fija por accidente. Es una condición del diseño propuesto, no una modificación ya aprobada de §6.2. Si no hay apoyo real durante 120 ms, seguir Apuntando y explicar “sin muestras suficientes” en diagnóstico; no convertir silencio en posición repetida. Antes de una implementación, el PO debe aceptar esa limitación o autorizar estudiar otra captura; no se presume necesario ni autorizado un puente nativo.

Incluso con Q2, pausar voluntariamente mientras se busca una zona puede fijar sin querer: **quietud no identifica intención**. Opciones de corrección: (U1) cancelar por N1 y empezar un gesto nuevo sobre el mismo lanzamiento; (U2) gesto inverso para desbloquear, que permitiría corregir más rápido pero se confundiría con bajar la potencia o cancelar. **Recomendación de Codex: U1 inicialmente**, sin desbloqueo automático durante el mismo gesto. Mostrar claramente el estado y enseñar salir/volver al círculo original. Si N1 aún no se armó, debe salir de él antes de regresar. El costo y las confusiones de este recorrido deben preguntarse al PO; no se dan por aceptados. Una cancelación del sistema también invalida toda fijación, sin consumir un golpe.

### Resultado descriptivo offline, no validación de Q2

Predeclaración **a37986beffd812237213f6789bf9756bbf64965c**, anterior al cálculo. [JSON derivado declarado](../validation/evidence/2026-10-27-s1-control-b-quiet-analysis.json): hashes de fuentes, parámetros fijos, índices de la primera ventana por intento y ambos ámbitos. No se ajustaron parámetros ni se reprodujo el nuevo control.

| Fuente y ámbito | Total / observable | QVR: velocidad+radio | QV: solo velocidad |
|---|---:|---:|---:|
| Revalidación2, interior completo | 138/138 | 24/138 (17,39 %) | 29/138 (21,01 %) |
| Revalidación2, apuntado observable | 138/138 | 24/138 (17,39 %) | 29/138 (21,01 %) |
| Calibración, interior completo | 49/49 | 7/49 (14,29 %) | 7/49 (14,29 %) |
| Calibración, apuntado observable | 49/24 | 6/24 (25,00 % de observables;6/49 del total) | 6/24 (25,00 % de observables;6/49 del total) |

En25/49 faltaba el timestamp nativo de Flick y no se reconstruyó: quedan fuera **solo** del ámbito de apuntado observable. En revalidación se conservaba el timestamp cuando hubo fijación; se recorta antes del cruce, incluso si la confirmación ocurrió después. Hubo al menos un hueco>51 ms en 99/138 y 30/49 interiores completos; no se prolongaron ventanas por esos huecos. Un gesto por fuente tenía menos de 3 muestras interiores utilizables en total; en el recorte de apuntado son10 en revalidación y 1 entre los 24 observables de calibración. No faltó escala ni hubo intervalos inválidos bajo el método declarado. Estas son limitaciones de observación, no fallos del futuro control.

**Lectura de Codex:** hay pausas compatibles en trazas tomadas con A′ que justifican estudiar fijación no deseada y enseñar cómo corregirla. Las frecuencias no son probabilidades de error del control §6.2 B: no incluyen su guardia de arrastre ni intención, y el interior completo puede corresponder a tensado/flick posterior a la fijación de A′. Tampoco una ausencia observada acredita que no se pausó: pueden faltar eventos. No se combinan fuentes para una tasa única ni se decide Q2 por un porcentaje favorable.

## 2. Feedback de fijado y estados

| Opción | Ventaja | Riesgo |
|---|---|---|
| F1: destello visual y forma/texto por estado | Cumple el destello de §6.2; observable sin sonido/vibración. | El PO puede no ver un cambio breve mientras mira el dedo. |
| F2: F1 más pulso háptico opcional, una vez al fijar | Segundo canal de confirmación. | No debe ser requisito para comprender; requiere autorización de runtime nativo y prueba propia. No se presume disponible ni latencia concreta. |

**Recomendación de Codex: F1 obligatorio en el diseño y F2 opcional/desactivado inicialmente [EXPERIMENTO]**, sin implementarlo. Apuntando: círculo y etiqueta; Fijada: rombo con destello y etiqueta; Tensando: mismo objetivo fijo, progreso/distancia/P y etiqueta; Inválida: rojo más forma/etiqueta, conservando además la fase de gesto. “Inválida” es una dimensión ortogonal: puede haber objetivo inválido en cualquiera de las tres fases, sin perder qué está haciendo el dedo. No basarse solo en colores. Destello propuesto 120 ms [EXPERIMENTO], sin ocultar el frontis ni reducir mínimos de mira/área segura de PR #19.

El pulso, si se autoriza, se emitiría una vez por transición efectiva a Fijada, no por frame, por cada muestra quieta o durante cancelación. [First Shot r2 §11.6/M02](../superpowers/specs/2026-10-24-first-shot-design.md) es la referencia para puente runtime/interruptor y alternativa visual; no se ejecuta ni amplía esa implementación en esta PR. La línea compacta con preset/hash sigue siendo una mejora futura por decidir, no está implementada ni sustituye la verificación por bloque.

## 3. Tensado y potencia por distancia

| Opción | Ventaja | Riesgo |
|---|---|---|
| P1: proyección vertical hacia abajo desde el punto de fijación | Alineada con la honda de §6.2; el movimiento lateral no aumenta potencia. | La mano que tira en diagonal aprovecha menos recorrido. |
| P2: módulo del desplazamiento desde fijación | Tolera cualquier dirección y da más recorrido. | Un barrido lateral puede producir fuerte; se aparta del tensado hacia abajo. |
| P3: recorrido máximo alcanzado, en vez de posición actual | Conserva un fuerte aunque el dedo regrese. | Retiene energía que se quiso reducir: difícil soltar suave tras apuntar/tensar rápido. |

**Recomendación de Codex: P1 con distancia actual**, pendiente del PO. En coordenadas de pantalla+y hacia arriba: `d=max(0,(y_fijación−y_actual)·mmPerPx)`. El origen de potencia se establece **al fijar**: todo recorrido de apuntado anterior aporta0. La mira permanece en el objetivo fijado aunque el dedo tire abajo. Volver arriba reduce d y P; pasar por encima del origen da P=0, no fuerza negativa ni desbloqueo. El movimiento lateral no aporta potencia ni desbloquea; se registra para evaluar comodidad y sigue sometido a N1 respecto del origen original del gesto.

Mapeo propuesto: `P=clamp((d−d0)/(d_full−d0),0,1)^γ`, con **d0=2 mm, d_full=40 mm, γ=1 [EXPERIMENTO]**. Opciones: curva lineal (predecible), exponente>1 (más recorrido suave/menos alta potencia) o<1 (más potencia al principio/riesgo de saturación). **Recomendación de Codex: lineal inicialmente**, con zona muerta y tope explícitos, sin curva por jugador ni memoria de velocidad. Debe validarse0≤d0<d_full y γ>0 en una futura configuración; no se aceptan pares incoherentes silenciosamente.

Ejemplo algebraico, no medición: con esos candidatos, suave 0≤P<0,25 corresponde a d<11,5 mm; media 0,25≤P<0,60 a 11,5≤d<24,8; fuerte 0,60≤P≤1 a d≥24,8 y satura desde 40 mm. Las bandas existentes no cambian. El ejemplo solo explica el mapeo candidato; no demuestra alcance físico cómodo ni que el PO consiga esas distancias. La advertencia de saturación **>20 %** sigue intacta: tope cómodo no significa éxito si provoca saturación frecuente. No se cambia esa advertencia para elegir escala.

Para iniciar Tensando se propone superar la zona muerta hacia abajo con al menos 2 muestras de posiciones distintas posteriores a fijación, contando el ancla como primera [EXPERIMENTO]. Un Ended idéntico no inventa la segunda. Sin tensado válido, soltar después de fijar sería P=0 si el PO acepta la opción de liberación del punto 4; no se transforma la quietud en potencia fuerte. Volver a zona muerta no desbloquea ni borra la intención final; N1 puede cancelar independientemente.

## 4. Fases, liberación y cancelación

| Decisión | Opciones y ventajas/riesgos | Recomendación de Codex, pendiente del PO |
|---|---|---|
| Liberación r | Una liberación final conserva timing de §6.2; dos liberaciones separan acciones pero serían M1, descartada por ahora. | **Ended original del primer dedo después de fijar**, con o sin tensado reconocido. Su timestamp es r; no el frame de render ni la muestra que fijó. EnS1 solo registro/previsualización, sin contacto ni cambio de host. |
| Soltar antes de fijar | Swing sin fijar evita perder intención pero vuelve al problema de mira/potencia; cancelar siempre es seguro pero puede sentirse lento. | Primero evaluar N2/N1 existentes; si de otro modo sería Swing, registrar **AbandonadoAntesDeFijar**, sin swing ni consumo. Resultado de fase propuesto distinto de N1, no fingir que ocurrió salir/volver. Necesita decisión expresa del PO. |
| Soltar fijada sin tirar | P=0 puede ser suave deliberado; exigir tirón evita esos swings pero agrega otro abandono. | Swing suave P=0 **solo si** arrastre válido/N2 y no N1/sistema; no golpe fuerte por pausa. R válido final, con distancia/soporte registrados. |
| Deshacer fijación | Cancelar/reempezar mantiene estados simples; desbloquear por movimiento inverso permite corrección rápida pero confunde bajar P. | Cancelar por N1 y reempezar, sin volver automáticamente a Apuntando. |
| N1 | Círculo del Began mantieneR2-2; reanclar al fijar facilitaría reducir/cancelar tensión pero cambiaría la regla. | Mantener **origen Began** y armar solo después de salir>6 mm; al soltar dentro≤6 mm cancelar, en cualquier fase. No reanclar a la honda. Valores existentes intactos. |
| N2 | Medir por fase podría salvar gestos breves de tensado, pero un toque podría generar swing. | Mantener desplazamiento máximo del **gesto completo**, <3 mm y<250 ms, con prioridad antes de N1; no llamar Tap a todo P=0. Quietud inicial sin arrastre no debe armar fijación según recomendación 1. |

Estas reglas son propuestas para revisión, no nuevas reglas operativas. [GestureTracker](../../Assets/SquashClash/Core/Shot/GestureTracker.cs) hoy distingue Tap/Canceled/SystemCanceled/Swing: no tiene AbandonadoAntesDeFijar ni fases de tensado. Una futura implementación debe preservar su clasificación para A′ y componer la puerta de fases del control §6.2 B, no reclasificar todos los controles globalmente.

La prioridad propuesta es sistema/UI/pausa/foco/Cancelled → intento ya consumido → primer dedo y lanzamiento/configuración fijados al apoyar → N2 → N1 → puerta de fase → swing. Primer dedo secundario ignorado, sin promoción; ningún estado sobrevive al cambio de lanzamiento. Pausa, foco, apertura de DEV, reanudación bloqueada, cambio deτ/cámara lenta o contexto de pantalla ya invalidan/cancelan gestos en [GesturePreview](../../Assets/SquashClash/Runtime/Shot/GesturePreview.cs#L69-L136): se propone conservar esa política y emitir una cancelación, sin Ended ficticio ni r inventado. Fijación, candidato de quietud, ancla de potencia y efecto de fijado quedan invalidados; no se restaura el tensado al recuperar foco. Parámetros y selector se capturan al apoyar, nunca cambian a mitad del gesto.

**Caso dt=0 pendiente de PR #22:** un Ended con misma posición y timestamp que la muestra anterior debe poder cerrar **una sola vez** con r exacto, sin aportar velocidad, quietud, duración o muestras nuevas; si ya había fijación/tensado válidos se usan sus datos previos. Un terminal de igual tiempo con posición contradictoria, un tiempo hacia atrás o no finito deben seguir marcados como no fiables, no normalizados inventando dt>0. Opciones: mantener el rechazo conservador de todo dt=0 (simple, pero anula fuertes legítimos), o tratar el terminal idéntico como evento de cierre y mantener rechazo de datos contradictorios. **Recomendación de Codex: segunda opción**, con TDD integrado y para el método de potencia B, además del control por distancia. No se corrige nada aquí. Si se suelta antes de acreditar fijación, Ended no la crea retroactivamente para evitar el abandono.

## 5. Reinterpretación del riesgo: decisión expresa pendiente

**La regla original150 ms/P<0,60 y sus casos siguen intactos para A′ y el método de potencia B por velocidad.** El control §6.2 B usa distancia, y 120 ms es su gesto de fijar; este documento no declara aplicable ni superada la regla original en ese nuevo control.

| Opción para reinterpretar, no aprobada | Ventaja | Riesgo |
|---|---|---|
| R1: exigirP<0,60 después de toda pausa150 ms también con distancia | Regla uniforme y conservadora. | Un tirón deliberado fuerte mantenido quedaría debilitado; necesitaría expiración/decaimiento adicional, no es pura potencia por distancia de §6.2. |
| R2: riesgo definido por fases e intención solicitada, sin que apuntado/fijación carguen potencia | Separa el arrastre para apuntar de un tirón posterior; permite mantener tensión deliberada. | Un tirón posterior hecho sin querer puede generar fuerte; la máquina no conoce intención humana y la sesión debe evaluarlo. No basta pasar casos sintéticos. |
| R3: confirmación adicional para armar potencia fuerte | Puede impedir fuertes por pausa/arrastre accidental. | Añade otra acción, costo de timing y reglas; puede volver a dos fases con confirmación explícita, descartadas por ahora. |

**Recomendación de Codex para decisión del PO: R2**, conservando bandas y sin aflojar la regla original. Propuesta de definición de riesgo: P≥0,60 sin tensado descendente posterior a fijar válido, o en un intento cuya tarea era “apuntar rápido y soltar suave”. El primer caso es un fallo verificable de separación de fases; el segundo es una discrepancia de tarea que se registra y se pregunta al PO como posible fuerte involuntario, no una prueba automática de su intención. Reinterpretar no significa renombrar como seguro el mismo fallo del método de potencia B.

Casos sintéticos **propuestos para R2, sus aserciones requieren aprobación del PO**:

| Caso | Recorrido / expectativa candidata |
|---|---|
| RB1: apuntado rápido, pausa150 ms, soltar sin tirar después de fijar | Movimiento anterior a fijar no carga P; una fijación al completar quietud reinicia ancla. Si se acredita fijación, P=0 sin tirón; si faltan muestras de quietud, abandono antes de fijar. Ninguna rama debe producir fuerte. Reusar los tres perfiles anteriores como entrada cuando proceda, sin llamarlo repetición de la misma regla semántica. |
| RB2: quietud/fijación con jitter dentro del radio y sin tirón | No tensado, P=0; nunca fuerte por acumular ruido o frames sin muestra. |
| RB3: tras fijar, movimiento lateral o hacia arriba, luego pausa150 ms y soltar | Proyección descendente0 → P=0; no potencia por módulo/recorrido máximo. |
| RB4: fijar, tirar deliberadamente hasta banda fuerte, mantener 150 ms y soltar | Mantiene la potencia de distancia: **puede ser≥0,60**, distinto de R1 y de la regla original por velocidad. Solo vale si el PO elige R2; no se presenta como Pass de 150 ms/P<0,60. |
| RB5: fijar, tirar fuerte, volver hacia arriba a zona muerta y soltar | P=0; si el recorrido arma y completa N1, cancelación prevalece. Nunca retener el pico como fuerte. |

No se ejecutan estos casos ni se fijan los datos sintéticos concretos aquí. El PO debe aprobar definición, expectativas y casos antes de programar las aserciones. Si prefiere R1, debe decidir explícitamente el costo sobre tensión mantenida y cómo compatibilizarlo con §6.2 sin editar spec v0.3. Si no hay decisión, el plan de riesgo y la sesión quedan bloqueados, no se ensaya por tanteo otro umbral.

## 6. Configuración propuesta y compatibilidad R2-3

Opciones: (K1) reutilizar claves de velocidad para distancia, con menos catálogo pero unidades y controles mezclados; (K2) **claves nuevas con unidades explícitas**, manteniendo A′ y sus valores. **Recomendación de Codex: K2**, captura inmutable al Began y selector por gesto/lanzamiento preparado, no cambio en medio del gesto. Cambiar desde DEV ya cancela el gesto; una API no debe permitir que el selector reinterprete una muestra iniciada con otro control.

**Tabla candidata, no añadida. Todos los rangos y valores iniciales son [EXPERIMENTO], no definitivos ni criterios de aceptación.** Proponen un conjunto coherente para que el PO lo apruebe o cambie antes de implementar; no son claves existentes ni una promoción de preset.

| Clave nueva propuesta | Tipo y rango propuesto | Inicial [EXPERIMENTO] | Uso |
|---|---|---:|---|
| `gesture.control` | Choice 0=A′,1=control §6.2 B | 0=A′ | Selector capturado al apoyar; A′ continúa por defecto. |
| `aim.holdMs` | Float 60–300 ms | 120 | Duración de quietud. |
| `aim.holdMaxSpeedMmPerS` | Float 0–100 mm/s | 15 | Límite entre muestras reales. |
| `aim.holdRadiusMm` | Float 0–5 mm | 1 | Radio desde ancla de quietud. |
| `aim.holdMaxGapMs` | Float 10–100 ms | 51 | Calidad temporal; no prolonga por silencio. |
| `aim.holdMinSamples` | Int 3–12 | 3 | Mínimo de tiempos distintos, además de duración/hueco. |
| `power.pullDeadZoneMm` | Float 0–10 mm | 2 | Zona muerta, desde fijación. |
| `power.pullFullMm` | Float 10–100 mm | 40 | Tope de distancia. |
| `power.pullExponent` | Float 0,5–3 | 1 | Curva de distancia. |
| `power.pullMinSamples` | Int 2–8 | 2 | Ancla+muestra posterior distinta para reconocer tirón. |
| `aids.aimLockFlashMs` | Float 50–400 ms | 120 | Feedback visual. |
| `aids.aimLockHaptic` | Bool 0/1 | 0 | Opción futura, requiere implementación/autorización propia. |

Las claves aim/power/gesture encajan en los prefijos que [GesturePreview.IsGestureKey](../../Assets/SquashClash/Runtime/Shot/GesturePreview.cs#L114) captura hoy; las de vista no influyen en el cálculo físico. Deben registrarse también para reproducir condiciones de feedback, sin hacer depender Core de DevPanel. Rango propuesto no elimina validación cruzada: d_full > d0; entradas finitas; cero de velocidad/radio significa quietud exacta y debe explicarse, no activar heurísticas ocultas.

Hoy **no cambia ninguna clave**: defaults 6095c0b38614,62 valores actuales, proyección de 58 anteriores 00a501ad7066,41 heredadas 0d39e6016bd8 y SimulationKey 166a96e319c7 intactos. En una implementación posterior expresamente autorizada, R2-3 permite que claves nuevas cambien el hash global por cambiar su conjunto; se debe registrar el nuevo hash y comprobar byte a byte los valores heredados y la SimulationKey física. No prometer conservar 6095c0b38614 cuando se añadan claves; sí conservarlo en esta PR. El preset 08b6fe90889b no se promueve y el método de potencia B sigue desactivado por defecto.

## 7. Comparación física futura, solo protocolo

| Opción | Ventaja | Riesgo |
|---|---|---|
| C1: una tanda por control con orden fijo | Sesión corta y fácil de ejecutar. | Aprendizaje/fatiga pueden explicar diferencias. |
| C2: orden de controles alternado en bloques predeclarados | Expone efecto de orden. | Mayor carga; puede fatigar al PO y exige más sesiones autorizadas. |

**Recomendación de Codex: C2 si el PO acepta la carga; si elige C1, declarar la limitación sin corregir resultados.** Predeclarar antes el orden exacto, práctica separada y esfuerzo; no elegirlo tras ver datos. Referencia propuesta: A′ con configuración existente por defecto (método de potencia A/net-window), sin promover 08b6fe90889b. Alternativa: A′ con método de potencia B experimental de la campaña anterior, que permite comparación con ella pero no es calibración validada. **Recomendación de Codex: referencia existente por defecto**, documentando su posible falta de rango y que la comparación cambia mecanismo de potencia; el PO debe aprobar la referencia y ambos hashes, no se eligen aquí como condición de aceptación.

Para cada control autorizado: mismas cuatro series de 10 (suaves/medios/fuertes/apuntar rápido y soltar suave), misma mano declarada para toda comparación, mismos objetivos e instrucciones adaptadas a la mecánica. Si cambia mano, configuración, app, fecha o foco, tratarlo como interrupción: verificar antes de continuar, sin unir datos como controlados automáticamente. La cuarta serie del control §6.2 B debe explicar qué acciones corresponden a apuntar/fijar/tirar/soltar suave según decisión de riesgo; no reutilizar un aviso de Flick que ya no describe ese control.

**Antes de cada bloque:** comprobar build, preset, hash efectivo y hash de gesto esperado, cero pendientes, selector, parámetros de potencia/quietud y launcher.repeatSeconds=0 aplicado sin guardar; cerrar DEV. Capturar esas condiciones en un registro de inicio de bloque. No basta el nombre del preset ni un hash de un lanzamiento de la noche anterior. Si al recibir datos no coinciden, conservar todos los intentos como no comparables y detener para decisión; no repetir para sustituirlos ni asumir que el preset temporal sobrevivió a un reinicio.

Campos propuestos además de los ya registrados: control al Began, estado/causa/timestamp de cada transición, ancla y ventana de quietud con índices reales, motivo de reinicio, target solicitado/fijado/final e invalidez, posición de fijación y eje de tensado, d actual/d máxima/d lateral, P/base/soporte, dedo/lanzamiento/configuración identificados de forma no personal, Ended original/r y outcome con N1/N2/abandono/cancelación, intento/bloque/tarea, feedback solicitado/emitido. Los identificadores de dispositivo y datos personales no se versionan. La app actual **no registra todas esas fases**: se requieren cambios de código futuros autorizados, no se inventan en el informe.

Informar por control/serie todos los intentos: P mín/med/máx, dispersión/IQR, banda pedida, saturación con advertencia>20 %, cancelaciones/abandonos por motivo, fijaciones no deseadas declaradas por PO, tiempos desde apoyar a fijar/soltar, error de mira y fuertes fuera de tarea suave. Separar mismatch numérico de juicio de potencia involuntaria. No agrupar manos ni variantes; DPI reportado sigue sin regla hasta verificación física propia. Recomendación de Codex: verificar con regla física antes de interpretar las distancias de una comparación autorizada como mm físicos; sin ella, etiquetar la escala nominal y no declarar calibración física. Los casos sintéticos del punto 5 son aparte y no validan por sí solos intención en iPhone.

Preguntas literales por separado: ¿Consigues las tres intensidades de forma voluntaria?; ¿Apareció algún golpe fuerte que no buscabas?; ¿Apareció algún golpe fuerte al apuntar rápido y soltar suave?; ¿La mira queda donde quieres apuntar?; ¿Se siente consistente entre intentos? Añadir si la pausa fija cuando aún quería apuntar y si pudo cancelar/corregir; no inferir respuestas por estadísticas. Citar las respuestas completas sin corregir retrospectivamente su significado.

Parada: defecto, configuración no coincidente o interrupción no resuelta → conservar datos y detener, sin ajustes de parámetros en campaña. Al terminar/abortar restaurar P0-calib-1 y comprobar en nuevo arranque hash esperado de esa build, cero pendientes y P0/Po-1/last-active byte a byte iguales al respaldo. Ninguna sesión/instalación está autorizada aquí; si en el futuro se instala, **renovar primero el perfil y comprobar su vigencia efectiva**, vencimiento registrado 2026-10-28T14:59:31Z. **Recomendación de Codex: este control de comparabilidad y parada debe ser requisito operativo de toda sesión autorizada**, no una corrección posterior de datos.

## 8. Plan TDD previsto dentro de S1, sin renumerar

Opciones: (T1) tarea adicional **1.2b** para el control §6.2 B, manteniendo 1.2/A′ como referencia; (T2) reemplazar el contenido de 1.2 por un único modelo configurable. T2 reduce repetición documental, pero oculta la regresión de la referencia. **Recomendación de Codex: T1**, con aprobación expresa de las adendas de texto del punto 9. Nada de esto se añade al plan actual ni autoriza su ejecución.

En toda tarea de comportamiento: primero interfaces/esqueletos **compilables**, luego RED observado por aserción semántica y GREEN tras implementación. Un error CS, cero tests o XML viejo no es RED válido. Preparación de catálogo/estructura se declara como tal, no se presenta como RED. Estos nombres son previsiones documentales, **no tests escritos o ejecutados**.

| Tarea futura pequeña | Contrato y RED por aserciones previsto | Dependencia |
|---|---|---|
| 1.2b-0: selector y snapshot | `Control_DefaultRemainsAPrime`; `Control_CapturedAtBegan_DoesNotChangeMidGesture`; `LegacyValuesAndSimulationKey_Unchanged`. Selector incorrecto/snapshot mutable producen fallos semánticos, tras compilar. | PO aprueba claves y estrategia de compatibilidad. Preparación declarada separada. |
| 1.2b-1: quietud y fijación | `Lock_RequiresRealSupportedWindow`; `NoSamples_DoNotAccumulateQuiet`; límites de 120 ms, velocidad/radio/hueco, pausa incidental, guardia N2 y una sola fijación. | PO decideQ/U; sin afirmar que120 ms es definitivo. |
| 1.2b-2: tensado | `PreLockMotion_DoesNotChargePower`; `PullDown_CurrentDistance_MapsAndClamps`; volver arriba/lateral, zona muerta/curva/tope, no velocidad ni pico retenido. | PO decideP y parámetros candidatos. |
| 1.2b-3: fases, r, N1/N2 | Soltar antes de fijar no crea fijación; caso N1 armado/sin salir, N2 por máximo y duración, fijo sin tirar, r original, un intento, cancelación sin consumo. `ReleasePhase_DoesNotBypassCancellation`. | PO decide abandono y cero suave, origen N1, expectativas de fase. |
| 1.2b-4: fuente de eventos/cancelaciones/dt=0 | Ended idéntico cierra una vez sin falsear tiempo ni anular P fiable; contradictorio/no monótono conservador. Primer dedo, no promoción, lanzamiento, pausa/foco/DEV/τ invalidan. Cubrir GesturePreview y método de potencia B, no solo AimModel. | Autorización expresa de la corrección pendiente PR #22; coexistencia con A′. |
| 1.2b-5: feedback y registros | Estados/formas distinguibles; destello/pulso una vez, no efectos tras cancelación, vista sin tapar frontis, registro de transiciones/hashes/bloques. | PO decideF y alcance háptico; puente runtime no se presupone. |
| 1.2b-6: riesgo | RB1–RB5 u otros casos que el PO apruebe; aserciones y datos sintéticos concretos **pendientes**, sin ejecutar un candidato para inventar el criterio. Mantener pruebas originales150 ms/P<0,60 para A′/método de potencia B. | **Bloqueada hasta decisión expresa del punto 5.** |
| 1.2b-7: regresión y entrega de código, futura | A′ conserva sus aserciones, frames/launcher/config no son una segunda autoridad de pelota. Suites completas frescas y check-release dev/prod si se autoriza esa PR de implementación; no promover control/preset por pasar tests. | RevisiónClaude, PO y alcance de implementación propios. Sesión/build/instalación aparte, si se autorizan. |

**Recomendación de Codex:** cerrar decisiones de fases/riesgo antes de fijar RED de comportamiento, mantener constantes las aserciones existentes y separar el experimento físico de la regresión. Ningún nombre de tarea constituye permiso para iniciar S2 ni implementar contacto real.

## 9. Redacciones futuras propuestas, no aplicadas

Opciones: (D1) añadir adenda explícita del control §6.2 B y mantener la referencia A′; (D2) sustituir las descripciones por texto genérico de controles con ramas. D2 es más corto, pero puede borrar diferencias de fijación/potencia/abandono. **Recomendación de Codex: D1**, condicionada a decisiones del PO y aprobación expresa de los textos. Solo se proponen aquí; **spec v0.3 no se toca**.

| Archivo/sección que se propondría actualizar | Redacción sugerida, con decisiones abiertas | Motivo |
|---|---|---|
| First Shot r2 §4.2 | “A′ se conserva como referencia. El control §6.2 B apunta por arrastre relativo, fija por quietud 120 ms [EXPERIMENTO, definición pendiente PO] y tensa hacia abajo por distancia en mm con tope [mapeo pendiente PO]. La selección se captura al apoyar. La liberación y cancelación se rigen por la adenda de fases aprobada.” | Separar controles, unidades y liberación, sin hacer Flick universal. |
| C5 §8/§9 | “El PO considera que A′ falla en su forma actual y elige investigar el control §6.2 B; A′ permanece como referencia. Es un juicio del PO, no aceptación de S1 ni Fail medido de calibración. C se conserva descartado por ahora. Implementación y comparación física requieren autorización propia.” | Incorporar la respuesta C5 sin cambiar evidencia/criterios. |
| §11.4 | “Fuente única de muestras reales y escala congelada comunes. En A′ se conserva la protección por Flick. En el control §6.2 B, quietud [definición pendiente PO] fija la mira; la potencia procede del tensado posterior por distancia [mapeo pendiente PO]. No se hereda la regla de riesgo por velocidad sin la reinterpretación expresa del PO. N1/N2 y fases se detallan en su adenda aprobada.” | Evitar que la fórmula de velocidad y el congelamiento por Flick se impongan a distancia. |
| M06 §11.6 | “Configuración/control capturados al apoyar y estables durante el gesto. La mira se fija según el control (Flick en A′, quietud en control §6.2 B); al soltar se capturan mira y potencia finales inmutables para el intento. Cancelación no inventa r ni salida física. El rollback no lee cambios posteriores del panel.” | Aclarar “se congelan al soltar” frente a fijación anterior, sin crear otro estado físico. |
| Plan T1.2 | “Mantener tarea 1.2 y pruebas A′. Añadir tarea 1.2b del control §6.2 B según diseño aprobado, con preparación declarada, esqueletos compilables, RED semántico→GREEN, casos de coexistencia y riesgo pendientes de decisión previa.” | Extensión dentro de S1 sin renumerar el plan ni cambiar aserciones históricas. |
| CoberturaC5 del plan | “C5: juicio del PO sobre A′ registrado; control §6.2 B por investigar con referencia A′ conservada. T1.2b para implementación si se autoriza; comparación futura con cuatro series/mano/hash por bloque, sin sustituir ni declarar ejecutados H2/S4.” | Distinguir investigación S1 de validación de gameplay/H2. |

La especificación v0.3 ya contempla el control elegido; ninguna de estas redacciones pretende cambiarla. Una futura PR debe someter los textos finales al PO; esta propuesta no resuelve los marcadores pendientes ni modifica archivos normativos.

## Estado de entrega y límites

Cierre de PR #23 registrado en AGENTS/aprobaciones dentro de esta PR, sin commit en main. Diff: **tres Markdown y un JSON derivado declarado**. Solo lectura de código/evidencia y análisis descriptivo predeclarado; no código de producto, tests, presets/defaults, spec, propuesta r2, planes, criterios/bandas/umbrales existentes modificados. Sin suites/check-release/builds/instalaciones/sesiones/acceso al iPhone. Recomendaciones no elegidas por el PO; valores candidatos no definitivos. PR lista para Claude **sin fusionar**. S1 sigue sin aceptar; S2–S4/T8 no autorizados. Detenerse al entregar; cualquier instalación futura exige renovar antes el perfil y comprobar su vigencia efectiva.

## 10. Decisiones pendientes del PO

1. **Quietud:** ¿Q1, Q2 o Q3, y qué definición inicial? **Recomendación de Codex:** Q2 con velocidad+radio,120 ms [EXPERIMENTO] y apoyo real; candidatos 15 mm/s,1 mm,51 ms y 3 muestras, sin tratar silencio como inmovilidad. Aprobar o cambiar antes de código; no son valores definitivos.
2. **Protección/corrección:** ¿exigir arrastre reconocido antes de fijar y cancelar/reempezar, o permitir desbloqueo en el mismo gesto? **Recomendación de Codex:** guardia N2 y U1; evaluar costo de N1 global sin introducir un desbloqueo ambiguo.
3. **Feedback:** ¿F1 visual o F2 con háptica opcional? **Recomendación de Codex:** destello/forma/texto por estado, duración 120 ms [EXPERIMENTO]; opción háptica inicialmente apagada y separada, nunca único indicador.
4. **Tensado/mapeo:** ¿proyección descendente, distancia radial o pico retenido, y cómo reducir? **Recomendación de Codex:** P1 con distancia actual, lateral sin potencia y regreso arriba reduce; candidatosd0=2 mm/d_full=40 mm/γ=1 [EXPERIMENTO], sin cambiar bandas ni advertencia>20 %.
5. **Liberación/cancelación:** ¿r al Ended posterior a fijar, abandono antes de fijar y P=0 suave al soltar fijada? **Recomendación de Codex:** esas opciones, N1 global sin reanclar y N2 por gesto completo, un dedo/lanzamiento/configuración estables. Deben aprobarse las expectativas de cada fase antes de TDD.
6. **Riesgo:** ¿R1, R2 u otra reinterpretación explícita y qué casos/aserciones? **Recomendación de Codex:** R2 y RB1–RB5 como hipótesis; un tirón fuerte mantenido puede seguir fuerte, por lo que **no es la regla original150 ms/P<0,60**. Nada resuelto hasta decisión; referencia original intacta.
7. **Configuración:** ¿K2 y las claves/rangos iniciales propuestos? **Recomendación de Codex:** claves separadas, selector `gesture.control=0` (A′) por defecto, snapshot al apoyar y compatibilidad R2-3. Aprobar antes de añadir; hash nuevo futuro solo por claves autorizadas, no cambiar6095c0b38614 aquí.
8. **Comparación:** ¿qué configuración exacta de A′ y qué orden/esfuerzo, con la misma mano y cuatro series? **Recomendación de Codex:** referencia existente por defecto y orden alternadoC2 si acepta la carga; valores/hashes y práctica predeclarados antes de una sesión con autorización propia.
9. **Plan/corrección compartida:** ¿autorizar en el futuro1.2b y el alcance del terminaldt=0 integrado/método de potencia B? **Recomendación de Codex:** resolver fases/riesgo primero, TDD de cierre terminal idéntico sin falsa velocidad y preservar A′; no iniciar ninguna tarea ahora.
10. **Textos y siguiente autorización:** ¿aprobar después las adendas propuestas de First Shot r2/T1.2/C5 y una PR de implementación separada? **Recomendación de Codex:** adendas explícitasD1 conservando A′, sin tocar spec v0.3; **esta entrega no autoriza código, build, instalación o sesión y no acepta S1**.
