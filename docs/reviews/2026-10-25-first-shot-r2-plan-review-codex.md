# First Shot — revisión de propuesta r2 y plan r1 por Codex

**Fecha:** 2026-10-25. **Alcance:** solo revisión documental. Sin implementación, prototipos, tests ejecutados, builds, instalación, medición ni renovación de firma. Propuesta y plan del autor conservados sin edición y sin commit. Este informe tampoco se publica ni se incorpora por commit durante esta revisión.

## Dictamen y fuentes

La r2 incorpora buena parte de la revisión anterior y permite mantener la arquitectura y los cuatro incrementos. **Todavía requiere ajustes antes de considerar cerrado el plan.** Hay **6 hallazgos Alta y 4 Media**; varios afectan al contrato de S2 y otros a la preparación de S1. No se cuestionan las tres decisiones del PO registradas en §11.1: se revisa si su concreción permite cumplir las demás reglas simultáneamente.

Fuentes revisadas en el worktree `firstshot/design-review`, HEAD **3220dd5748511a471ba11ee49e7e8c200ff4b667**, código de main **a8da9c43ee1b82c506d50c9d5434fe616e5957b6**:

- [Propuesta r2](../superpowers/specs/2026-10-24-first-shot-design.md), incluida §11. SHA-256 de la copia recibida: `d0a2f0dfdbab1da8b7f4b7b981ff57f5518e30b2e5deee0bbc2839f78ff9c3af`.
- [Plan r1](../superpowers/plans/2026-10-25-squash-clash-first-shot.md), 642 líneas al revisar. SHA-256: `469e7d6d9fad3aac6e737c841b09f75a40d706e3a3dbb447ba405f0d4082ccc6`.
- [Spec v0.3](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md), D30–D35, y [revisión inicial de Codex](2026-10-24-first-shot-design-review-codex.md).

Las referencias a líneas de propuesta/plan corresponden a esas copias sin commit. Son observaciones del texto y del código, no fallos reproducidos mediante una implementación. Cada propuesta de corrección es para que Claude la incorpore; no cambia la spec ni criterios/umbrales.

## Hallazgos Alta

### F01 — La legalidad propuesta permite volea, que First Shot excluye

**Evidencia:** propuesta r2, línea 288, define «frontis después del lanzamiento, como máximo un bote». El plan, líneas 323 y 333, prueba «AtMostOneBounce» y empieza el barrido tras el primer frontis. La spec §7.1.1, línea 307, exige **un bote previo cuando no hay volea**. La propia propuesta excluye volea en §2/§4.3.

**Consecuencia:** una pelota entre el frontis y su primer bote satisface el nuevo texto y puede producir K/T* y contacto. No basta con diseñar seis lanzamientos que normalmente se golpeen después del bote: el contrato seguiría admitiendo el golpe antes.

**Corrección:** en la recepción First Shot exigir frontis previo y **exactamente un bote no persistente antes del contacto**, junto con alcance y altura legal. Separar alturas legales de alturas preferidas de U: `PreferredMinHeight/MaxHeight` no definen por sí mismas `h_min/h_max`. Precisar sus valores/procedencia sin inventarlos durante la implementación. Añadir pruebas de ausencia de K/contacto antes del primer bote, entrada después de él y salida en el segundo.

### F02 — El paso más cercano puede ser ilegal aunque r sea válido

**Evidencia:** R2-1 de propuesta, línea 244, autoriza cuantizar solo el instante físico y conservar la clasificación exacta. El plan representa K solo con pasos (línea 317), excluye **todo el paso del segundo bote** (línea 325) y usa el paso más cercano a r (línea 374). P2 produce impactos dentro del paso: [AnalyticBallSimulation](../../Assets/SquashClash/Core/Simulation/AnalyticBallSimulation.cs), líneas 31–55; [ContactEvent](../../Assets/SquashClash/Core/Simulation/ContactEvent.cs) conserva `TimeInStep`.

**Ejemplo documental:** r puede ocurrir justo antes del segundo bote y redondearse al extremo posterior del paso, cuando la pelota ya es ilegal. También puede estar dentro de K y redondearse fuera del alcance, o ocurrir después del segundo bote y redondearse hacia un estado anterior válido. Marcar todo el paso del segundo bote como ilegal pierde liberaciones que ocurrieron antes del impacto. Son dos problemas distintos: legalidad de r exacto y legalidad del estado físico elegido.

**Corrección:** conservar R2-1, pero añadir una tabla explícita con ambos tiempos y la precedencia frente a primer/segundo bote, pared y entrada/salida de alcance. Para Perfecto temprano, primero seleccionar el contacto asistido T*_visible y después su paso; «paso más cercano a r» no debe anular la programación a T*. Usar los tiempos de eventos existentes para decidir la legalidad exacta; eso no requiere modificar P2 ni insertar pasos parciales.

Si el paso físico elegido queda fuera de K/legalidad, las reglas actuales no determinan una solución que conserve a la vez contacto permitido, paso más cercano y error ≤medio paso. **Ese caso concreto debe elevarse al PO**; no recortar K, desplazar al vecino legal o aceptar contacto ilegal silenciosamente. Registrar el desplazamiento firmado real de cuantización, además de la cota de medio paso, y definir el empate exacto entre dos pasos. Añadir pruebas a ambos lados de cada frontera y con T3-suave.

Para entrega tras finalizar el vuelo, completar también qué es «el presente» que se reconstruye: [BallSimulationHost](../../Assets/SquashClash/Runtime/Ball/BallSimulationHost.cs), líneas 84–99, deja de avanzar al terminar. Reconstruir solo hasta su último `Current.Step` puede mantener una demora respecto al reloj real. El mapa debe registrar esa detención y la política de recuperación; un test con ese nombre no reemplaza el contrato.

### F03 — La definición normativa de Δ confunde la presentación del plan con su objetivo temporal

**Evidencia:** propuesta r2, línea 254: «Δ se calcula [...] contra el instante real en que se presentó T*». La spec §7.1.1 usa el **valor de T* que mostraba** el anillo al liberar. El plan, línea 351, distingue `Present(tReal, plan)` y `tStarReal`, por lo que las dos interpretaciones no coinciden.

**Consecuencia:** si a los 10,0 s se muestra un plan cuyo contacto ideal es a los 10,5 s, soltar a los 10,5 s debe dar Δ=0; restar el instante de presentación da +500 ms y un fallo artificial.

**Corrección:** nombrar y guardar separadamente `planPresentedAtReal`, `targetContactAtReal` y `releaseEventAtReal`. Buscar el plan que era visible al **timestamp original de liberación**; después comparar el r corregido por compensación con el instante ideal de ese plan. La compensación no debe seleccionar retrospectivamente un plan que el jugador no vio. Especificar esta precedencia en `ContactContract.Decide` y probar un cambio de plan entre el r corregido y el evento real. Mantener la conversión al reloj simulado y el error físico de cuantización como resultados distintos.

### F04 — La repetición no tiene definido el final de vuelo que preserva las métricas

**Evidencia:** propuesta r2, línea 289, y plan, línea 466: relanzar después de «resolverse el golpe». `ShotResolved` pertenece al controlador (línea 465), mientras `practice.repeatSeconds` parte de 1,5 s (línea 130). No se indica si se espera al contacto, al primer/segundo bote o al final de salida. Además sigue existiendo [AppBootstrap.TickRepeat](../../Assets/SquashClash/Runtime/App/AppBootstrap.cs), líneas 53–60, con el temporizador independiente `launcher.repeatSeconds`; P0-calib-1 lo tiene en 3 s.

**Consecuencia:** si el contador empieza al decidir o contactar, puede reemplazar una pelota cuya salida todavía no llegó al frontis o no produjo sus botes. Se pierden observación de potencia, zonas de bote y evidencia de §17.E/anexo C. Si ambos repetidores están activos, el antiguo puede cortar un gesto o un vuelo aunque el nuevo espere correctamente. Tampoco se fija el reinicio después de «Sin orden»/«No llega», que deben permitir practicar sin un swing obligatorio.

**Corrección:** describir la máquina de ciclo de práctica: recepción, contacto/resultado, observación de salida hasta condición terminal acotada y espera entre pelotas. Definir qué eventos reales se necesitan antes de relanzar y un horizonte/final explícito para salidas sin botes, sin convertirlo en reglas de partido. En práctica debe haber un único dueño de la repetición; suspender el repetidor First Ball sin modificar/persistir el valor heredado. Fuera de práctica, restaurar su comportamiento. Probar vuelo lento, ausencia de orden, fallo, cancelación, P0 con repetición=3, pausa y foco.

### F05 — El método sugerido de calibración no separa las latencias que promete

**Evidencia:** tarea 4.1 del plan, línea 587, propone tocar al ritmo de destello/clic y exige `Calibration_SeparatesDeviceBiasFromRingDisplayDelay`. No define una referencia física externa ni un segundo observable independiente.

**Consecuencia:** un desfase entre cue y timestamp de toque combina demora de presentación/sonido, respuesta o anticipación humana y demora de captura. Descartar una primera serie no permite identificar cada componente. Un test con retardos sintéticos conocidos valida una fórmula, pero no demuestra que el protocolo físico pueda estimar esos retardos por separado. La compensación podría incluir un sesgo humano y presentarlo como latencia de iPhone, contaminando H3.

**Corrección:** elegir y documentar una de estas dos metas: (a) medición física de componentes con referencia externa y método autorizado, o (b) calibración de **offset perceptivo conjunto**, con incertidumbre y repetibilidad, sin atribuirlo al dispositivo ni afirmar que separa componentes. La segunda no introduce equipo por defecto; debe explicarse cómo su resultado se usa en H3 y qué limitación conserva. La spec exige calibrar antes de medir H3; no exige certificar una descomposición que este método no observa. Mantener separadas pruebas del cálculo y evidencia del protocolo.

### F06 — El primer RED previsto es un error CS; faltan prerrequisitos de InputTestFixture

**Evidencia:** plan, línea 171: «Expected: FAIL (tipos inexistentes)». [AGENTS.md](../../AGENTS.md) requiere RED válido y rechaza errores CS como evidencia; [scripts/test.sh](../../scripts/test.sh), líneas 34–46, rechaza ausencia de tests/XML y errores de compilación. Tarea 1.3 usa `InputTestFixture`, pero el [asmdef PlayMode actual](../../Assets/SquashClash/Tests/PlayMode/SquashClash.Runtime.Tests.asmdef) no referencia `Unity.InputSystem`/`Unity.InputSystem.TestFramework` y [manifest](../../Packages/manifest.json) no declara `testables`.

Unity 1.20 documenta esos requisitos en [Set up test assemblies](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/set-up-test-assemblies.html). La lista de archivos de S1 no incluye preparar esos dos archivos.

**Corrección:** añadir un paso previo mínimo para que los tests compilen y se descubran, con esqueletos de API sin comportamiento que hagan fallar **aserciones ejecutadas**. Después observar el RED de conducta y escribir la implementación. Declarar en S1 la configuración de ensamblado/testables que se necesita, manteniendo el filtro de las suites del producto y la exclusión del framework de test de prod. No contar un cambio de montaje de pruebas ni un fallo CS como ciclo RED→GREEN.

El adaptador de bajo nivel también necesita un contrato preciso: [onEvent](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.InputSystem.html#UnityEngine_InputSystem_InputSystem_onEvent) se llama antes de procesar el evento y no ordena por timestamp; [Touchscreen](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.Touchscreen.html) suele recibir `TouchState` individual, no snapshot completo. Especificar lectura del payload/fase/identidad del evento, no de controles aún antiguos, y probar el formato recibido, múltiples eventos y no monotonía. La elección `onEvent` es viable; por sí sola no verifica esa lectura.

## Hallazgos Media

### F07 — S1 declara protección ante huecos/DPI incoherente, pero no la concreta

**Evidencia:** propuesta r2, línea 274, exige potencia conservadora ante huecos excesivos. Plan, líneas 192–227, registra `MaxGapMs`, pero `PowerSettings` no incluye regla/límite de hueco ni hay test de hueco excesivo. El congelamiento se decide con solo las últimas dos muestras distintas (línea 227). `ScreenMetrics` (línea 244) solo rechaza DPI no finito/≤0, aunque Review Focus pide cubrir un valor incoherente también.

**Corrección:** conservar el ajuste de Claude que admite flick breve con cobertura/muestras suficientes, y concretar validez temporal, hueco máximo y respuesta ante una ventana discontinua. Probar mínima cobertura, número de muestras suficiente con un gran hueco, y ruido en dos posiciones muy próximas temporalmente que dispara el congelamiento. La histéresis posterior no evita un primer disparo espurio cuando el congelamiento es irreversible. No impongo otra ventana ni un límite numérico aquí: deben quedar definidos y calibrables en el plan.

Para DPI añadir validación de plausibilidad/procedencia y una prueba con positivo incoherente, manteniendo el factor congelado por gesto. Distinguir «fallback nominal 460» de «calibración física confirmada»: usar el primero no demuestra por sí solo la segunda. Registrar y bloquear inferencias de mm verificados cuando corresponda.

### F08 — Los rangos individuales permiten configuraciones conjuntas inválidas

**Evidencia:** tabla del plan, líneas 107–108, permite dead zone=400 y full=300; la fórmula de línea 212 presupone `full>dead`. Líneas 125–127 permiten `vMin=15`, `vMax=10`, `vCap=10`. El [ParamDef actual](../../Assets/SquashClash/Core/Config/ParamDef.cs), líneas 38–46, solo valida un valor; [TuningState](../../Assets/SquashClash/Core/Config/TuningState.cs), líneas 19–44, aplica claves individualmente. No hay validación de estas relaciones en el plan.

**Consecuencia:** cada slider/importación es válido por separado, pero puede invertir la curva de potencia o dejar un tope inferior a la salida pedida. «Capturar ShotConfig inmutable» congela también una configuración inválida si no se valida antes.

**Corrección:** definir invariantes de configuración (`deadZone<full`, `vMin≤vMax≤vCap`, banda preferida coherente y demás relaciones necesarias) y validarlas atómicamente al capturar/aplicar/importar. Mostrar el error y conservar el snapshot válido anterior, o definir otra política explícita y trazable; no ajustar valores heredados ni silenciosamente corregir el preset. Añadir pruebas con extremos, igualdad, cambio por panel e importación. No hace falta un framework de validación genérico.

### F09 — La validación requiere completar dependencias y cobertura

**Evidencia y corrección concreta:**

- **Instrumentación S2:** tarea 2.7, línea 492, exige costo por parte/frame del golpe, pero la implementación de esos contadores y sus pruebas aparece en 3.3, líneas 550–568. Mover el mínimo necesario a 2.5/2.6, conservando su extensión posterior en S3; así el informe S2 se sustenta en datos existentes.
- **Pruebas deportivas:** no hay tarea para §17.F/anexo C, ya señalado en la revisión inicial. Añadir X1–X5 y relaciones aplicables a First Shot, con sus condiciones de validez y decisión del PO si fallan; registrar explícitamente lo no aplicable (saque/volea/IA), sin inventar umbrales. Los tests de solver/contacto no reemplazan capacidad expresiva de drive/cruzado/drop.
- **H3b:** línea 607 exige que todos los bots sean peores en vector, aire y zona. Si ambos contactan dentro de K, ambos pueden tener tasa de aire cero; si V produce fallo, no existe vector causal de contacto para comparar como cero. Definir poblaciones/ausencias y medir las dimensiones por separado, sin exigir desigualdad estricta en cada una ni diseñar la física para hacer GREEN la hipótesis. Separar pruebas del bot/protocolo de sus resultados experimentales, como exige AGENTS.
- **H3:** las líneas 616–618 no hacen explícitas varias sesiones ni su comparación. Mantener la evidencia **entre sesiones** de la spec §17.B además del aprendizaje en los primeros 10–15 intentos, que es una ampliación aprobada, no su reemplazo.
- **Prod:** el test de línea 481 remite a `check-release`, pero el [script actual](../../scripts/check-release.sh) comprueba export/metadata/ensamblados/puentes, no que una pelota reciba un golpe o que suene/vibre. Nombrar por separado verificación estática y recorrido funcional de prod. No inferir jugabilidad ni actuación física a partir de símbolos; definir evidencia autorizada del recorrido.

### F10 — El test de N2 cambia los bordes aprobados

**Evidencia:** propuesta r2, línea 245, y restricción global del plan, línea 23, dicen desplazamiento **<3 mm y <250 ms**. El test de línea 159 exige que **3,0 mm y 250 ms inclusivos** sean Tap. No es una diferencia de tolerancia de test: cambia si se ejecuta swing/consume intento en esos bordes.

**Corrección:** alinear las aserciones con la regla estricta escrita y añadir justo debajo/en/encima de cada límite por separado. Si se prefiere inclusivo, registrar primero el ajuste de contrato con el PO; no convertir el comentario de test en la aprobación. El borde inclusivo del círculo de cancelación es otro límite distinto y debe definirse por separado.

## Puntos señalados por el PO y ajustes menores

- **Barrera S4:** la decisión 11 de la propuesta pide aprobar zonas/bandas **antes de medir H2**; el plan, línea 597, la adelanta a antes de implementar. No bloquea S1–S3. Recomiendo conservar la barrera antes de la medición y permitir preparar infraestructura de tareas sin valores definitivos, salvo decisión distinta del PO. No pedir otra aprobación de lo ya delegado.
- **Valores de partida:** ganancia 0,12 m/mm, flick 250 mm/s, salida 6–28 m/s y coeficientes causales siguen siendo experimentales. No hay evidencia aquí para aceptarlos/rechazarlos como calibración. F07/F08 piden que los valores produzcan un sistema bien definido y medible, no congelarlos.
- **First Ball:** ajustar solo el montaje de tests que realmente dependen del arranque puede ser adecuado. Debe mantenerse la aserción y también el escenario que esa aserción verificaba; no basta con dejar su texto si se cambia el significado de la preparación. La prohibición global de cambiar «sus pruebas» (línea 27) y la excepción de línea 485 deben quedar conciliadas expresamente.
- **H17:** el plan conserva correctamente que la desviación de First Ball no se hereda. La comparación de audio prevista es de **dos sesiones de 15 minutos**, una por configuración (línea 573), no una única sesión total.
- **Perfil:** correcta la regla de renovar antes de instalar desde el 27 y verificar vigencia siempre. No se renueva ni instala durante esta revisión.
- **Igualdad bit a bit:** retirar «por construcción» de propuesta línea 252. Compartir pasos es una precondición, pero §8.2.1 ya retiró esa garantía y exige estado exacto, parámetros/orden y medición. Puede escribirse «se verifica bajo las precondiciones de §8.2.1».
- **Detalle del solver:** `Solved=false` existe, pero falta precisar cuándo se devuelve si ni bajar y dentro del dominio ayuda, y cómo lo presenta el controlador sin fabricar contacto ni ocultarlo como fallo del PO. Mantener acotado el número de iteraciones y la misma salida en replay.

## Estado al entregar

Las decisiones R2-1/R2-2/R2-3 se mantienen como decisiones registradas del PO, sin modificar la spec v0.3. Los problemas concretos restantes deben incorporarse al diseño/plan antes de tratarlos como contratos cerrados; la revisión no autoriza S1. Las ambigüedades heredadas de T3-suave/§17.A.12 tampoco se resuelven alterando tests para aprobarlas.

Solo se crea este informe. Se cotejan de nuevo los hashes de los dos insumos, referencias locales y `git diff --check`; no se hacen commits, push, cambios de PR ni mensajes externos. La r2 y el plan permanecen sin commit tal como se recibieron. P0-calib-1 queda pendiente de comprobar en próxima apertura; no se abre el iPhone. Main, código, defaults y herramientas permanecen intactos.
