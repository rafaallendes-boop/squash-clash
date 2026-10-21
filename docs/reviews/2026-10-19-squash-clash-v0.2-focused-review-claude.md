# Squash Clash: revisión focalizada de la especificación v0.2

- **Fecha:** 2026-10-19
- **Autor:** Claude
- **Base:** [Especificación v0.2](../superpowers/specs/2026-10-19-squash-clash-design-v0.2.md). No se modificó.
- **Alcance:** los seis puntos que pidió el PO. No es una reescritura de la especificación. Las decisiones aprobadas siguen vigentes: 7 puntos con diferencia de 2, iPhone en vertical, movimiento automático, cuatro pilares y panel. La duración de 3–5 minutos sigue siendo una referencia flexible.

Cada punto sigue la misma estructura: **problema detectado → alternativas → recomendación → qué necesita la aprobación del PO**. Ningún punto se da por aprobado.

---

## 1. ¿El contacto asistido conserva una habilidad real de timing?

### 1.1 Problema detectado en la v0.2

El §7.1 de la v0.2 tiene cuatro debilidades que, juntas, pueden vaciar de contenido el timing:

1. **"Imán" hacia T\*:** si se suelta hasta 250 ms antes, el contacto ocurre *igual* en T\*, en el punto ideal. Pegar temprano solo cambia una nota de calidad, y la posición del contacto no cambia.
2. **Asimetría que premia soltar temprano:**
   - Soltar temprano tiene 250 ms de margen y, si es demasiado temprano, se **descarta y se puede volver a intentar** sin costo.
   - Soltar tarde tiene solo 150 ms de margen y después **no hay golpe**.

   La estrategia dominante sería soltar siempre un poco antes de tiempo. La habilidad se reduce a "no soltar tarde".
3. **Reintento gratis:** el caso 1 (orden descartada, se vuelve a apuntar) permite soltar a ciegas varias veces.
4. **El anillo marca exactamente T\*:** con la ayuda completa, el timing se vuelve un juego de ritmo y no se *lee* la pelota.

Además, las ventanas fijas en milisegundos no dependen de la velocidad de la pelota. Una pelota rápida y una lenta exigen el mismo timing, cuando en el squash real la presión del tiempo es parte del juego.

### 1.2 Alternativas

| | Descripción | Habilidad de timing | Frustración | Comentario |
|---|---|---|---|---|
| **T1: v0.2 con ventanas más estrechas** | Igual que la v0.2, pero con E y Lt menores y un mayor peso del timing en la calidad. | Media | Baja | No resuelve el imán ni el reintento. Sigue siendo un juego de "nota". |
| **T2: contacto físico puro** | El contacto ocurre exactamente al soltar, donde esté la pelota. Si no está al alcance, se pega al aire. | Máxima | Alta con pelotas rápidas | Con el movimiento automático, el margen útil es muy corto: con la pelota a 10 m/s y ~0,6 m de alcance útil a lo largo de su trayectoria, quedan ~60 ms. Riesgo alto para un juego móvil. |
| **T3: asistencia mínima con consecuencia física** | Dentro de una ventana chica **±A** alrededor de T\* [EXPERIMENTO: ±40 ms], el contacto se ajusta a T\*: es el "perdón" por la latencia y la precisión del dedo. Fuera de ±A, el contacto ocurre **en el instante de soltar**, donde esté la pelota, si está al alcance. Así el timing se convierte en **geometría**: pegar adelantado o atrasado respecto del cuerpo, con las consecuencias causales del §4. Si se suelta cuando la pelota todavía no entra o ya salió del alcance, se pega **al aire o no hay golpe**. **Un solo swing por recepción**, sin reintento. Se puede cancelar de forma explícita. | Alta, y crece con la velocidad de la pelota | Media, acotada por ±A y por la ayuda visual configurable | Recomendada. |

### 1.3 Recomendación: T3, con tres complementos

1. **La ventana útil escala sola con la presión.** Con una pelota rápida, el tramo al alcance dura menos milisegundos, así que el margen se achica. Con una pelota lenta, crece. No hace falta escalar las ventanas a mano.
2. **Ayuda de timing en niveles** [EXPERIMENTO], configurable en el panel y, en el futuro, por nivel del jugador:
   - **Completa:** el anillo se cierra en T\*.
   - **Parcial:** el anillo desaparece en los últimos ~150 ms y hay que *leer* la pelota.
   - **Sin anillo.**
3. **Prueba de que no hay estrategia dominante** (nueva hipótesis de §17.B): un bot que suelta siempre con un desfase fijo (por ejemplo, 80 ms antes) tiene que rendir *peor* que un bot con timing preciso, medido por la calidad promedio. Además, la dispersión de Δ del PO debería bajar con la práctica, lo que indica que hay un techo de habilidad.

**Nuevas categorías de fallo** (reemplazan los casos 1–4 de la v0.2):

| Situación | Resultado | Categoría |
|---|---|---|
| \|Δ\| ≤ A | Contacto en T\*, calidad de timing máxima | — |
| Fuera de ±A, con la pelota al alcance en r | Contacto en r (adelantado o atrasado), con la consecuencia causal correspondiente | — |
| Suelta antes de que la pelota entre al alcance | **Golpe al aire** | **Fallo de timing** (nuevo) |
| Suelta después de que la pelota sale del alcance, o no suelta | Sin golpe | Omisión |
| No hay contacto alcanzable | Sin golpe | Inalcanzable |

**Ajuste a la invariante §17.A.5:** "nunca hay fallo de ejecución con buena posición" sigue vigente, pero **no incluye el fallo de timing**. Pegarle al aire por soltar demasiado pronto es una decisión del jugador, no azar ni cansancio. Esto necesita tu aprobación, porque significa que **se puede fallar la pelota estando bien ubicado**.

**Variante más suave, por si T3 frustra:** soltar antes de que la pelota entre al alcance produce un contacto en el **primer punto alcanzable**, con la peor consecuencia de "adelantado", en lugar de un golpe al aire. Así nunca se falla la pelota por timing, pero el golpe sale muy malo.

### 1.4 Decisiones del PO
- **PO-1:** ¿aprobar T3 como hipótesis para First Shot, en lugar del modelo de la v0.2?
- **PO-2:** ¿aceptar el **golpe al aire** por soltar demasiado pronto, o prefieres la variante suave?
- **PO-3:** ¿aceptar la ayuda de timing en niveles (completa, parcial, sin anillo) como experimento?

---

## 2. ¿El jugador puede decidir tácticamente cuándo volear?

### 2.1 Problema detectado en la v0.2
En la v0.2, la variante por defecto (V1) deja que **el planificador decida** la volea. Eso le quita al jugador una de las decisiones más tácticas del squash: cortar la pelota en la T para quitarle tiempo al rival, o dejarla pasar y jugar desde atrás. La V2 (dos momentos) aparecía solo como plan de respaldo y sin reglas de movimiento.

### 2.2 Alternativas

| | Cómo decide el jugador | Ventaja | Problema |
|---|---|---|---|
| **V1: planificador** (v0.2) | No decide | Simple | Sin agencia: contradice el objetivo táctico |
| **V2: momento de soltar** | Soltar en la ventana de volea es volear. Esperar es dejar botar. | Sin gestos nuevos. La decisión es inmediata y legible. | El planificador tiene que mantener ambas opciones vivas, y las dos ventanas no pueden superponerse |
| **V3: posición de espera W** | Esperar adelante (cerca de la T o más adelante) favorece la volea. Esperar atrás la descarta. | Decisión táctica *previa*, como en el squash real | Por sí sola no permite decidir pelota a pelota |
| **V4: inicio del gesto** | Apoyar el dedo temprano es intención de volea | Sin gestos nuevos | Es ambiguo: todos apoyarán el dedo temprano para estar listos |

### 2.3 Recomendación: V2 + V3 combinadas, con V1 como control experimental

1. **El planificador mantiene dos planes cuando ambos son factibles:**
   - **plan de volea:** contacto antes del bote;
   - **plan de bote:** contacto después del bote.

   El personaje se mueve hacia el plan de volea, porque queda en el camino, hasta que **se cierra la ventana de volea**. Si el jugador no soltó, el plan de bote pasa a ser el único y el personaje sigue hacia él.
2. **Dos anillos de colores distintos** cuando hay dos opciones: volea y bote. Si solo una es factible, se muestra un anillo.
3. **El costo es real y sale de la física:** dejar pasar la pelota desde una posición adelantada obliga a retroceder. Si no alcanza el tiempo, el plan de bote puede quedar con poca holgura o ser inalcanzable. Por eso **volear o no es una decisión con riesgo**, no un detalle de animación.
4. **Ventanas separadas:** si el tiempo entre el cierre de la ventana de volea y la apertura de la ventana de bote es menor que un umbral [EXPERIMENTO: 250 ms], se ofrece **una sola opción** (la de mayor puntuación). Así un soltar temprano para el golpe de bote no se convierte en una volea por accidente.
5. **W influye en las opciones (V3):** una posición de espera adelantada hace factibles más voleas. Esto conecta la decisión previa (dónde espero) con la decisión inmediata (corto o dejo pasar).
6. **Experimento en First Rally:** comparar sin volea, con V1 (control) y con V2 + V3. Medir el porcentaje de voleas elegidas, la holgura del rival, la ocupación de la T, los puntos perdidos por dejar pasar y la valoración del PO.

### 2.4 Decisiones del PO
- **PO-4:** ¿aprobar V2 + V3 como variante principal del experimento de volea, con V1 solo como control?
- **PO-5:** desde tu experiencia, ¿hay situaciones en las que un jugador *no puede* elegir dejar pasar la pelota (por ejemplo, un globo que no da tiempo a retroceder)? Esto define cuándo se muestra un solo anillo.

---

## 3. ¿Los escenarios del anexo C imponen resultados demasiado rígidos?

### 3.1 Problema detectado en la v0.2
El anexo C fija resultados absolutos: "1.er bote detrás de la short line", "< 0,5 m de la lateral", "2.º bote a < 2 m del frontis". Tiene tres riesgos:
1. **Imponen una física concreta.** Los resultados dependen del ritmo τ, la restitución, la altura de contacto y el objetivo. Un cambio legítimo de calibración rompería escenarios que no estaban mal.
2. **Confunden "el golpe sale así siempre" con "el golpe *se puede* hacer así".** Un buen drive pegado depende de la puntería del jugador, no del valor por defecto.
3. Con el tiempo, **se convierten en restricciones permanentes**, que es justo lo que pediste evitar.

### 3.2 Alternativas

| | Forma | Ventaja | Problema |
|---|---|---|---|
| C-A | Números absolutos (v0.2) | Fáciles de comprobar | Rígidos y dependientes de la calibración |
| C-B | Bandas con tolerancia y descriptores cualitativos ("pegado", "ancho", "suelto") | Más flexibles | Siguen siendo absolutos |
| C-C | **Relaciones comparativas y monótonas** | Robustas ante la calibración. Capturan la lógica del squash. | No dicen *cuánto* |
| C-D | **Capacidad expresiva:** existe un objetivo y una potencia que producen el resultado | Protegen que el jugador *pueda* hacer el golpe | Requieren búsqueda en las pruebas |
| C-E | Juicio del PO sobre un video del escenario (de 1 a 5) | Es la autoridad deportiva | Es manual |

### 3.3 Recomendación: tres capas

1. **Capacidad expresiva (C-D)**, como prueba automática. Para cada escenario, **existe** al menos una combinación de mira y potencia que produce el resultado: un drive que bota detrás de la short line y queda pegado, un cruzado que toca la lateral contraria después del bote, un drop que muere adelante. Así se protege que el golpe *sea posible*, sin exigir que salga solo.
2. **Relaciones comparativas (C-C)**, también automáticas, porque son la lógica del squash y casi no dependen de la calibración:
   - más potencia, con la misma mira, da una pelota más profunda;
   - una mira más alta en la banda media da una pelota más profunda;
   - el drop defensivo desde atrás es más alto y más lento que el drop desde adelante;
   - un golpe estirado sale más corto y más alto que el mismo golpe con buena posición;
   - una volea deja al rival con menos holgura que el mismo golpe tras el bote;
   - pegarle cerca del tin aumenta tanto la probabilidad de ganador como la de error.
3. **Bandas de calibración (C-B)**, como objetivos flexibles de la clase C. Son los números que hoy figuran en el anexo C, convertidos en rangos que **tú ajustas con presets** según lo que veas en el iPhone. No son pruebas que bloqueen.
4. **Tu juicio (C-E)** como palabra final: si un escenario cumple todo pero no se siente squash, gana tu juicio.

### 3.4 Decisión del PO
- **PO-6:** ¿aprobar el anexo C en tres capas (capacidad + relaciones como pruebas, bandas como objetivos flexibles, tu juicio como decisión final)? ¿Agregarías o quitarías alguna relación de la lista?

---

## 4. ¿El modelo de error separa la consecuencia causal de la variabilidad aleatoria?

### 4.1 Problema detectado en la v0.2
En el §7.6.2 de la v0.2, **la misma `carga`** escala tanto el componente determinista como la dispersión aleatoria (σ = base·carga). Esto tiene tres efectos:
1. Las mismas causas (timing, posición) generan **a la vez** una consecuencia explicable y más azar. El jugador no puede saber qué parte del error fue su culpa y qué parte fue suerte.
2. Hay doble conteo: la causa ya produjo su efecto y además infla el ruido.
3. El "componente determinista según la causa" quedó vago: no dice qué efecto produce cada causa.

Además, con T3 (punto 1), el timing ya tiene una consecuencia **física** (contacto adelantado o atrasado). Si además suma un término de error, se cuenta dos veces.

### 4.2 Alternativas

| | Capa causal | Capa aleatoria | Comentario |
|---|---|---|---|
| E-A | Sí | No | Totalmente explicable, pero podría sentirse mecánico. Sirve como línea base. |
| E-B | Sí | Constante y pequeña, igual para todos | Simple. No distingue entre personajes ni golpes. |
| **E-C** | **Sí** | **Depende solo de la dificultad inherente del golpe y de la *consistencia* del personaje. Nunca del timing ni de la posición.** | Separa la culpa de la suerte, y le da al pilar **Mental** su primer efecto. |
| E-D | Sí | Escalada por la carga total (v0.2) | Mezcla las capas. Se descarta. |

### 4.3 Recomendación: E-C, con estas reglas

**Capa 1, causal (determinista).** Cada causa produce un **vector de efecto con signo** (cambio de altura, cambio lateral relativo al cuerpo y cambio de velocidad):

| Causa | Cómo actúa | Efecto |
|---|---|---|
| **Timing** | **Solo a través de la geometría del contacto** (T3) | Adelantado o atrasado → filas E1/E2 de la tabla §7.6.3, una vez que las valides |
| **Posición** | Estirado, atorado, poca holgura | Filas E3/E5: menos velocidad, pelota más alta |
| **Dificultad situacional** | Pegado a la pared, pelota baja | Filas E4/E6 |
| **Técnica** | Atributo | Reduce la magnitud de los vectores causales (mejor técnica → errores causales más chicos) |

Los efectos de cada causa se suman, con un tope total. Cada uno se registra por separado.

**Capa 2, aleatoria (variabilidad):**
- σ vertical y σ lateral dependen **solo** de la dificultad inherente de la intención (un drop a 5 cm del tin es más variable que un drive a media altura) y del subatributo **Consistencia** (pilar Mental). Nunca dependen del timing, la posición ni el cansancio.
- Distribución normal truncada (±2σ) con **semilla registrada**.
- **Apagada en la línea base** (First Shot y First Rally) y probada como experimento en First Fun, como en la v0.2.

**Visualización:**
- La mira muestra **solo la variabilidad** (capa 2), es decir, el riesgo propio del golpe que elegiste.
- La superposición de depuración muestra **las dos capas por separado**. Por ejemplo: "causal: +18 cm de altura por estirado; aleatorio: −3 cm".
- Así se puede responder "¿fue mi culpa o mala suerte?" mirando un solo golpe.

**Efecto en los cuatro pilares:** Consistencia pasa a ser el **primer atributo Mental con efecto**, y es fácil de observar. Se mantiene el límite de la v0.2: es un efecto acotado y nunca produce fallos inevitables.

### 4.4 Decisiones del PO
- **PO-7:** ¿aprobar el modelo de dos capas E-C, con la variabilidad ligada solo a la dificultad del golpe y a la Consistencia?
- **PO-8:** como jugador, ¿la variabilidad que viviste dependía más de lo difícil del golpe o de tu estado mental del día? Esto define si Consistencia es la variable correcta.

---

## 5. La afirmación de "fidelidad garantizada" de la simulación propia

### 5.1 Problema
El §8.2 de la v0.2 dice que con la simulación propia (P2) la fidelidad de la predicción está "garantizada por construcción". **La afirmación está exagerada.** Compartir el código es necesario, pero no basta. La predicción coincide con la simulación solo si se cumplen **todas** estas condiciones:

1. **El mismo estado inicial exacto:** la predicción tiene que partir del estado de la simulación, no de una posición interpolada para el render.
2. **El mismo paso fijo y el mismo orden de integración.** Un predictor que usa pasos más grandes o fórmulas analíticas para ahorrar costo ya no es idéntico.
3. **Los mismos parámetros:** un cambio del panel a mitad del rally invalida la predicción. La v0.2 ya lo restringe (§13.4), pero hay que convertirlo en invariante.
4. **Ninguna influencia externa entre la predicción y el evento:** un golpe de raqueta cambia la trayectoria, como es obvio. La predicción solo vale hasta el próximo contacto con raqueta.
5. **El mismo binario y el mismo dispositivo.** No se promete igualdad entre dispositivos ni entre builds distintos: los floats y las optimizaciones del compilador pueden diferir.

### 5.2 Corrección propuesta
- **Reemplazar** "garantizada por construcción" por: *"con P2 la igualdad entre predicción y simulación es **alcanzable y verificable a bajo costo**, si se cumplen las condiciones 1–5"*.
- **Nueva invariante (§17.A):** en el dispositivo, la predicción desde el estado de simulación coincide **bit a bit** con la simulación hasta el siguiente contacto con raqueta, para los escenarios S1–S8. Si para ahorrar costo se usa un predictor más grueso, se aplican los umbrales de la v0.2 (≤ 1 cm, ≤ 2 ms) en lugar de la igualdad bit a bit.
- **Igual para P1 (PhysX):** se mide con los mismos umbrales. La inclinación por P2 se mantiene, pero ahora se justifica porque la igualdad es **fácil de lograr y de probar**, no porque esté garantizada.

### 5.3 Decisión del PO
Ninguna. Es una corrección técnica. Se incorpora en la v0.3.

---

## 6. Tratamiento de los contactos con las líneas

### 6.1 Problema detectado en la v0.2
El §8.3.5 de la v0.2 define una "huella de contacto con radio ~r (20 mm)". Tiene cuatro problemas:
1. **Una huella de ~20 mm sobrestima** el contacto: en la práctica, las líneas quedan ~2 cm más gruesas. La pelota de squash se deforma al impactar, pero su huella real es menor que su radio.
2. **No se definen la geometría ni el ancho de las líneas.** Las líneas reglamentarias tienen ~50 mm de ancho, y las alturas (4,57 m, 1,78 m, 0,48 m y 2,13 m) se miden **hasta un borde específico de la línea**. Hay que fijar a qué borde corresponde cada medida, porque cambia el resultado cerca de la línea. *[SUPUESTO: las alturas se miden hasta el borde superior. Se verifica con la especificación de cancha de la WSF.]*
3. **Faltan las líneas del suelo:** en el saque, botar sobre la línea del cuarto receptor cuenta como fallo.
4. **No se resuelve la percepción:** en el teléfono, una diferencia de 1 cm no se ve. Una decisión de "out" sin retroalimentación visual se siente injusta, aunque sea correcta.

### 6.2 Alternativas

| | Regla | Ventaja | Problema |
|---|---|---|---|
| L-A | Solo el punto de contacto (proyección del centro) contra la franja de la línea, sin huella | La más simple. El ancho de la línea ya da tolerancia. | Ignora la deformación |
| **L-B** | Huella de radio fijo configurable [EXPERIMENTO: ~10 mm] contra la franja de la línea | Simple, aproxima la deformación y se puede ajustar | El valor es arbitrario hasta calibrarlo |
| L-C | Huella que depende de la velocidad del impacto | Más realista | Más compleja y sin una ventaja de juego clara hoy |

### 6.3 Recomendación: L-B, más geometría explícita y una "marca de cantado"

1. **Regla:** la pelota es **out**, o **baja** en el caso del tin, si su huella de contacto se **superpone** con la franja de la línea o queda por encima de ella. Tocar la línea es out.
2. **Geometría de las líneas en los datos:** posición del borde de referencia, ancho (~50 mm) y lado hacia el que crece la línea. Medidas a verificar con la especificación WSF antes de First Ball.
3. **Superficies que se juzgan:**
   - en el frontis: tin, out y línea de saque (esta solo en el saque);
   - en las laterales: la diagonal de out;
   - en la pared trasera: la línea de out;
   - en el suelo, solo en el saque: las líneas del cuarto receptor y la short line.
4. **Marca de cantado:** cuando se canta out, tin o saque fallado, aparece durante 1 s una marca ampliada del punto de contacto sobre la línea, tipo ojo de halcón. Así una decisión de milímetros se *ve*.
5. **La mira previa** usa la misma huella, de modo que la mira se pone roja de forma coherente con la regla.
6. **Pruebas en el borde** (§17.A): el contacto justo debajo de la línea es bueno, el contacto que roza la línea es out y el contacto exacto sobre el borde cumple la regla de superposición. Se prueba en cada superficie.

### 6.4 Decisiones del PO
- **PO-9:** ¿confirmas que, como en el reglamento, **tocar la línea es out** en todas las líneas, incluido el tin y las líneas del suelo en el saque?
- **PO-10:** ¿aprobar la marca de cantado tipo ojo de halcón en las decisiones de línea?

---

## Resumen de decisiones para el PO

| ID | Decisión | Recomendación |
|---|---|---|
| PO-1 | Modelo de timing | T3: asistencia mínima ±A y, fuera de ella, consecuencia física. Un swing por recepción. |
| PO-2 | ¿Golpe al aire por soltar demasiado pronto? | Sí, como *fallo de timing*, distinto del fallo de ejecución. Variante suave si frustra. |
| PO-3 | Ayuda de timing en niveles | Sí, como experimento. |
| PO-4 | Decisión de volear | V2 + V3: dos anillos, la elección es el momento de soltar y W influye. V1 como control. |
| PO-5 | Situaciones en que no se puede dejar pasar la pelota | Lo responde tu experiencia. |
| PO-6 | Anexo C en tres capas | Sí. |
| PO-7 | Modelo de error en dos capas (E-C) | Sí. |
| PO-8 | Origen de la variabilidad (¿Consistencia?) | Lo responde tu experiencia. |
| PO-9 | Tocar la línea es out en todas las líneas | Confirmar. |
| PO-10 | Marca de cantado tipo ojo de halcón | Sí. |

El punto 5 (fidelidad de la predicción) es una corrección técnica y no requiere decisión.

**Siguiente paso propuesto:** con tus respuestas, consolidar la **v0.3** de la especificación como base funcional. Solo se tocan las secciones afectadas (§7.1, §7.3, §7.6, §8.2, §8.3, §17 y anexo C) y se mantiene la trazabilidad con este documento.
