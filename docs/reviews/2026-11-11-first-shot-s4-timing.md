# First Shot S4 — timing (11 de noviembre de 2026)

## Autorización literal del PO

Autorizo a Codex la etapa S4 del plan First Shot r2, limitada a: Tarea 4.1 (calibración de latencia), Tarea 4.3 (bots de anticipación y experimento H3b en el Mac) y el Step 0 documental de la Tarea 4.2 (propuesta de niveles de ayuda adaptada a la ayuda de impacto de #44). La implementación de 4.2, las tareas 4.4 y 4.5 y cualquier sesión en el iPhone requieren autorización aparte. Spec, escenarios, umbrales y criterios no se modifican. Que no se fusione nada sin mi aprobación.

Autorizo copiar byte a byte scripts/device.env desde el worktree ~/.codex/worktrees/s3-validation/Squash Clash, sin cambios ni versionarlo.


## Base y alcance

Main local, origin/main y referencia remota comprobados en `18b796a615b5bbca856c1624829a8ddc1895e214` antes de crear el worktree. Rama propia `firstshot/s4-timing`. Este registro precede todo código. Tareas autorizadas: 4.1, 4.3 y Step 0 documental de 4.2. Sin implementación de 4.2, 4.4/4.5, iPhone, cambios de spec/planes/criterios/defaults ni fusión. No se editan AGENTS.md ni el registro común de aprobaciones. Sin tocar #32 ni áreas de Fable.

## Estado inicial

Pendiente: TDD de calibración y bots, experimento descriptivo H3b separado de regresión, propuesta de ayudas/tareas y protocolo físico, suites completas, check-release dev/prod y AutoQA. Ninguna ejecución o aceptación se infiere de este registro.

## S4-TIMING-01 — dependencia de H3b; parada antes de código

Detectada durante la lectura inicial de la base autorizada `18b796a`, antes de crear esqueletos o pruebas. El encargo exige informar el vector causal promedio en H3b, separado de contactos y fallos. La capa productora de esos vectores no está implementada: quedó **expresamente diferida fuera de S3** por la decisión 9 del PO en el [diseño de S3](2026-11-05-first-shot-s3-design-plan.md), y su tabla de alcance conserva la calidad causal de la tarea 3.1 como pendiente. El [plan r2, tarea 3.1](../superpowers/plans/2026-10-25-squash-clash-first-shot.md) describe esa implementación aparte.

Lectura de código que confirma la dependencia: [ShotController.Solve](../../Assets/SquashClash/Runtime/Shot/ShotController.cs#L150) pasa directamente posición de contacto, mira y potencia al [ShotSolver](../../Assets/SquashClash/Core/Shot/ShotSolver.cs); [ShotRecord](../../Assets/SquashClash/Core/Shot/ShotRecord.cs) registra intención, solución y contacto, sin los vectores causales de timing/E3/E5. Los ajustes de factibilidad `SpeedRaised`/`TargetLowered` del solver son otra magnitud, no reemplazan esos vectores. Tampoco la diferencia geométrica entre contacto y P* es el vector causal aplicado al golpe.

Esto es una **dependencia pendiente del alcance**, no un fallo de regresión descubierto, una medición nueva ni una revocación de la aceptación condicionada de S3. No es correcto introducir ceros como si fueran vectores medidos ni añadir la capa causal dentro de esta autorización limitada a 4.1/4.3/Step 0 de 4.2.

El encargo indica literalmente: «Ante cualquier defecto o sorpresa: registra y detente, sin ajustar parámetros». Se aplica esa parada. Consulta al PO: autorizar continuar 4.1 y los bots/protocolo de 4.3, con **vector causal promedio de H3b NotEvaluated** por capa ausente y las demás variables disponibles informadas por separado. Se pueden probar las reglas de agregación con vectores sintéticos identificados como tales; eso no acredita la métrica productiva de H3b. No se cambia la hipótesis, criterios ni umbrales, ni se implementa la tarea 3.1.

### Estado en la parada

- Primer commit `82828d9`: solo este documento con la autorización literal, antes de código.
- `scripts/device.env` copiado byte a byte desde el worktree S3 autorizado, ignorado y sin versionar.
- Sin cambios de código, tests, defaults, catálogo, plan, propuesta, spec, escenarios, tolerancias o criterios; AGENTS.md y aprobaciones comunes intactos.
- No ejecutados: RED/GREEN, regresiones, check-release, AutoQA ni experimento H3b. Todos **NotEvaluated** en esta entrega interrumpida; no se declara éxito.
- Propuesta completa de 4.2 y protocolo físico aún pendientes. No implementación de 4.2 ni tareas 4.4/4.5.
- Sin iPhone, Duplicación, builds, instalación, sesión física, cambios en #32/First Rally/First Match ni fusión.
- PR final no creada: la entrega solicitada todavía no está completa ni lista para revisión final de Claude. Rama local preservada para continuar cuando el PO decida esta dependencia.

## Decisión del PO sobre S4-TIMING-01 — continuación autorizada

Respuesta literal: **«Continuar con vector causal NotEvaluated (recomendado)»**, a la consulta que autoriza continuar 4.1 y los bots, dejando el vector causal de H3b NotEvaluated y midiendo las demás variables disponibles, sin implementar la capa causal. Sustituye la parada vigente de la sección anterior, conservada como historia. Se mantienen el resto del encargo y sus límites. Este registro precede todo código.

Método previsto de 4.1: tres series de ocho pulsos medidos, período de un segundo, cuatro pulsos de familiarización por serie; marcas reales de emisión y de evento táctil, emparejamiento al pulso más cercano (empate anterior), sin eliminar outliers. Serie incompleta, ambigua o con condición inválida no propone compensación. Media, desviación muestral por serie, intervalo/rango de medias y tiempos de separación entre series; intervalo del 95 % de la media de tres series con t de Student (2 grados de libertad). Es incertidumbre de repetición bajo este método, no de latencia del dispositivo. Sin calibración física en esta entrega.

## Método predeclarado para H3b (antes de ejecutar)

Experimento Editor explícito, fuera de `SquashClash.Core.Tests` y de las suites de regresión. Se usan las seis recetas SL1–SL6 existentes, ocho semillas 0–7 y cuatro bots (0, −60, −100, −150 ms respecto del T* del plan visible), emparejados entre T3 puro y V=100 ms. T3-suave apagado; ayuda actual de #44 completa, sin otros niveles implementados: parcial/ninguno NotEvaluated. Total previsto: 384 intentos, sin tocar recetas ni física. Mismas semillas/intenciones en ambos modos y todos los bots.

La semilla determina una intención reproducible por intento, no ruido en el desfase fijo. Fixture descriptivo de zona frontis: rectángulo cerrado x∈[2,7;3,7] m, y∈[1,7;2,7] m; mira central (3,2;2,2) con variación determinista de hasta 0,2 m por eje y potencia 0,25/0,50/0,75. Se declara aquí como **zona del arnés H3b**, no como tarea H2 aprobada ni criterio nuevo. Acierto se informa sobre contactos cuyo primer impacto observado es frontis dentro de esa zona y también sobre todos los intentos (fallos no son aciertos). Trayectoria de salida P2 hasta primer contacto/horizonte 10 s; solver original y ajustes de factibilidad registrados por separado.

Por modo/receta/desfase se conservan resultado y motivo del contrato, geometría/paso de contacto, cuantización, salida/ajustes y evento de primer impacto. Se reportan por separado contactos, fallos al aire/tarde, otras categorías, aciertos observados y zona no observada. En pares aceptados por ambos modos se coteja geometría/paso/salida: no se atribuye al límite V una penalización geométrica que no existe. Vector causal productivo: **NotEvaluated**, no cero; pruebas de agregación con vectores sintéticos solo validan el protocolo. No se fuerza desigualdad estricta ni se emite Pass de H3b con la métrica causal y otros niveles pendientes.

## T4.1 — TDD inicial

Esqueleto compilable: primera corrida 0/11 con 10 fallos por aserción y un acceso nulo en el esqueleto; se conserva como intermedia no válida para el grupo completo. Se inicializó el resultado vacío del esqueleto, sin implementar el cálculo: **RED válido 0/11, once fallos por aserción, sin errores CS**. Tras implementar: 10/11; el último caso exigía preservar el toque que invalida timestamps. Corregido el registro sin cambiar la prueba: **GREEN 11/11**. XML/log frescos separados en `Logs/evidence/s4-timing/t4-1`. Son datos sintéticos; la accesibilidad del control se comprueba en Main del Editor. No calibración física ni aceptación inferida.

## T4.3 — TDD del bot y el protocolo

Esqueleto compilable. Un primer intento no produjo XML por usar un matcher no disponible en el NUnit del proyecto; se conserva el log y **no cuenta como RED**. Corregida esa sintaxis de prueba: **RED válido 1/10, nueve fallos por aserción, sin CS**. Implementación sin alterar contrato, reloj, solver ni P2: **GREEN 10/10**. La prueba con vectores usa valores sintéticos declarados, no atribuye esos valores al producto. El reductor mantiene modos separados y ausencia de vector como null/NotEvaluated; un fallo nunca añade una muestra cero. Evidencia en `Logs/evidence/s4-timing/t4-3`.

## T4.2 — Step 0 documental; implementación no autorizada

La ayuda actual #44 es la referencia **completa** del experimento. No se incorpora ninguna clave ni filtro de vista en esta rama. Propuesta para decisión del PO:

| Nivel propuesto | Representación sobre ImpactCue / TimingRingView | Ventajas | Riesgos / decisión pendiente |
|---|---|---|---|
| Completo | Mantener anillo convergente, triángulo fijo y bandas Perfect/Valid de #44, incluyendo la supresión de la banda Valid en T3 puro. | Referencia ya implementada; enseña aproximación y punto ideal. | Visibilidad de 2 px y asimetría de las bandas pendientes; no equivale a validación física de #44. |
| Parcial, opción A | Ocultar solo anillo móvil y bandas desde T*−150 ms; conservar triángulo fijo sin destello ni desvanecimiento dependiente de T*. | Conserva referencia espacial, reduce información temporal final. | El triángulo revela P*; si heredase el fade actual filtraría tiempo. Requiere separar estilos y comprobarlo con TDD. |
| Parcial, opción B **recomendada para discutir** | Ocultar todo el conjunto de timing desde T*−150 ms hasta resolver/cerrar la recepción. Tras el corte no reaparece por un cambio de plan. | Respeta la intención de retirar la señal temporal final sin pistas residuales del triángulo/fade. | Más exigente; retirar también P* cambia la referencia espacial. Se registran el target y versión que activaron el corte y los planes posteriores, sin cambiar el contrato. |
| Ninguno | No dibujar anillo, triángulo ni bandas desde el inicio de la recepción; ImpactCue/contrato siguen calculándose para trazabilidad. | Compara dependencia del indicador sin modificar timing. | Se pierde P*. «Pelota y sombra» del test del plan debe entenderse respecto de las ayudas de timing; no autoriza borrar mira/potencia/feedback/UI ajena. Confirmar esa interpretación antes de implementar. |

El PO debe elegir A/B u otra definición antes de codificar 4.2. Todos los niveles conservarían la selección del plan visible en el timestamp original, los ms reales y la compensación aplicada una sola vez. El instante ideal y las ventanas no se desplazan para facilitar el test. No se implementa aquí el «últimos 150 ms» del plan sobre un diseño anterior de anillo.

### Clave propuesta y compatibilidad

`timing.aidLevel`: elección **0=completo (default propuesto), 1=parcial, 2=ninguno**, capturada por recepción; con un preset anterior sin esa clave se superpone el default 0. La clave nueva exige autorización de implementación 4.2 y R2-3: pasaría de 107 a 108 claves y cambiarían los hashes globales de defaults y P0 efectivo. Se calcularían y registrarían entonces desde el JSON real, sin inventarlos ahora. Los 41 valores heredados/proyección y SimulationKey deben seguir idénticos; P0-calib-1 guardado no se promueve ni reescribe. En esta rama **no se añade la clave**: defaults `c0d8ec7050e0`, P0 `6ecfdfa78cbb`, 107 claves, SimulationKey `166a96e319c7` y proyección heredada `0d39e6016bd8` permanecen intactos.

### Tareas provisionales para aprobación antes de H2

No son criterios nuevos ni zonas medidas de H2. Propuesta de etiquetas/valores iniciales, pendiente del PO (decisión 11):

- Frontis: tres columnas en coordenadas AX-1, x∈[0,20;2,20), [2,20;4,20), [4,20;6,20] m, etiquetadas por intervalos X para evitar invertir derecha/izquierda de pantalla. Tres alturas: y∈[0,60;1,60), [1,60;2,60), [2,60;3,60] m. Una tarea marca un rectángulo concreto; fuera del margen legal no se presenta como zona válida.
- Potencia normalizada P: suave [0;1/3), media [1/3;2/3), fuerte [2/3;1]. Registrar potencia pedida y congelada, saturación y fallos de captura; sin cambiar `pullFullMm=35`, curva ni clamp. No equivalen a velocidad física fija si el solver eleva v por factibilidad.
- Primer bote posterior al golpe: dividir X en tres franjas iguales dentro de [0,20;6,20] m y Z en corto [0,20;3,20), medio [3,20;6,40), profundo [6,40;9,55] m. Registrar centro del primer bote, toque de línea/superficie previa y out por separado, sin convertirlo en regla de punto. Límites exactos y política de bordes deben aprobarse antes de H2.
- Ejemplo de tarea combinada: «columna X baja, altura media, potencia fuerte, primer bote profundo». Pedido, mira congelada, objetivo factible, impacto de frontis y primer bote son campos distintos; un fallo no se reemplaza por un error cero. La zona del arnés H3b predeclarada arriba es independiente de estas propuestas H2.

### Observaciones de revisión de #44 preservadas

1. `StrokePixels=2`: puede ser demasiado fino en el iPhone. Proponer una comparación corta 2/4/6 px con fuente/cámara iguales en una autorización posterior; no se aumenta aquí ni se afirma mejora de legibilidad.
2. Las bandas actuales sitúan solo el borde **temprano** de A y V; después de T* el anillo ya llegó al radio de encaje y el fade no representa simétricamente el borde tardío. Proponer explicar ambos bordes o añadir representación tardía en 4.2, con TDD y decisión del PO; sin modificar ahora las ventanas.
3. En Δ=+ValidMs, el contrato todavía es Valid por borde inclusivo, pero el alpha del renderer llega a 0. Es una observación conocida del encargo, conservada sin corregirla en este Step 0. En la futura implementación probar −V, −A, 0, +A, +V y puntos a ambos lados, cotejando estado/visibilidad con el contrato, sin recortar la inclusión para hacer pasar el dibujo.

## Sesión física futura propuesta — no autorizada ni ejecutada

Requiere PO presente, autorización propia, revisión de Claude y build DEV nueva Release/ARM64/no Development que incorpore la calibración y #44. La app instalada conserva el perfil que vence el **12 de noviembre a las 10:55:07 Chile**. El perfil renovado en caché vence el **18 de noviembre a las 10:50:05 Chile**; verificar fecha efectiva del perfil incorporado, certificado/firma y tiempo para toda la sesión/restauración antes de instalar. No asumir que la renovación en caché actualizó la app instalada.

1. Exportar logs antes de nuevas aperturas por la retención existente. Respaldo nuevo de `P0-calib-1.json`, `Po-1.json` y `last-active.txt`, SHA-256 y bytes; cotejarlos después de instalar sin desinstalar/borrar datos. Proceso nuevo con P0, hash efectivo `6ecfdfa78cbb`, cero pendientes, backend P2, M2-4, sin cámara lenta, foco y térmico/contexto registrados. Altavoz, sin Bluetooth, timbre y volumen confirmados por el PO; registrar buffer DSP/frecuencia/ruta declarada y ajustes de audio, sin afirmar latencia de pantalla/altavoz medida.
2. Explicar antes de empezar: «Toca al ritmo del destello y clic. Esto estima un offset conjunto de presentación, sonido, captura y tu respuesta, no la latencia del iPhone. No golpea la pelota y no se aplica nada hasta que confirmes». Preguntar una a una: **«¿Ves el destello?»**, **«¿Oyes el clic?»**, **«¿Entiendes que debes seguir el ritmo y que no estamos midiendo solo el dispositivo?»**. Si falta identificación o hay defecto, conservar parcial y detenerse.
3. Desde First Shot / S4 en DEV: tres series de 4 pulsos de familiarización + 8 medidos, período nominal 1 s, espera inicial 2 s. El PO inicia cada serie tras un descanso libre, cuyo intervalo queda registrado. Se usan timestamps reales de emisión lógica del frame y `InputEvent.time` de Began; registro nominal/real y edad al recibir. La emisión lógica no acredita presentación óptica ni salida acústica. Emparejar cada toque con el pulso real más cercano (empate anterior, máximo medio período); duplicados, faltantes, ambigüedad, cámara lenta, pérdida de foco/segundo plano o pulso perdido invalidan, sin eliminar outliers ni repetir para lograr un número. Guardar todos los intentos.
4. Antes de mostrar números, preguntar **«¿Pudiste seguir un ritmo estable en las tres series?»** y **«¿Percibiste el destello y el clic juntos o separados?»**, una a una y conservando literales. Mostrar media, desviaciones por serie, rango de medias, IC95% entre series e intervalos de descanso. No hay umbral de aceptación nuevo. Fuera del rango existente −100…100 ms no se recorta ni se aplica: elevar al PO. Antes de confirmar, explicar también: **al seguir un ritmo regular, la persona suele tocar antes del pulso (asincronía media negativa)**, fenómeno revisado por [Repp (2005), §3](https://link.springer.com/content/pdf/10.3758/BF03206433.pdf). El offset conjunto puede incluir esa anticipación humana. Su traslado a la intercepción de la pelota **no está demostrado**; es una limitación para que el PO decida al confirmar, no un umbral ni criterio nuevo. Pregunta final separada **«¿Confirmas aplicar este offset conjunto solo para esta sesión?»**; solo una confirmación explícita habilita el botón. Cancelar deja `timing.latencyCompMs` intacto. No guardar preset.
5. Chequeo breve de #44 propuesto, sujeto a cuota/autorización futura: misma SL1 y cámara, ayuda actual completa; primero identificar **«¿Ves el triángulo fijo, el anillo que converge y las bandas?»**, después hasta 6 recepciones sin repetir para conseguir un Perfect. Preguntas una a una, antes de resultados: **«¿Entiendes cuándo encaja el anillo en el triángulo?»**, **«¿El trazo se ve con claridad?»**, **«¿El indicador te ayuda a decidir cuándo soltar?»**. Sin inferir H3/H2, aprendizaje ni condiciones pendientes de S3 por ese chequeo. Perfect/silencio/háptica/destello en pausa requieren sus bloques y autorización propios.
6. Exportar antes de otra apertura. Restaurar transitorios sin guardar y reiniciar el proceso desde el P0 guardado; comprobar **P0 `6ecfdfa78cbb`, cero pendientes**, compensación heredada 0, y cotejar los tres archivos byte a byte contra el respaldo y sus SHA-256. Registrar arranque nuevo, build/fecha/perfil, literales, parciales y límites. Si no alcanza la vigencia/tiempo reservado para restaurar, no iniciar el bloque. No hay instalación ni acción de iPhone en esta entrega Mac.

## Fuentes técnicas del método

- [Unity Input System 1.20, InputEvent.time](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.LowLevel.InputEvent.html): timestamp del evento en la misma línea temporal real de Unity; se conserva en lugar de usar el frame de procesamiento como hora del toque. No separa el retardo de captura del resto del offset conjunto.
- [NIST, tabla t de Student](https://www.itl.nist.gov/div898/handbook/eda/section3/eda3672.htm): IC bilateral 95 %, t(0,975;2)≈4,303 para tres medias de serie. El intervalo supone repetición independiente y no captura sesgo sistemático, acoplamiento audio/visual ni latencia individual de los componentes. La incertidumbre física sigue NotEvaluated hoy.

## Resultado del experimento H3b en el Mac

Fuente `5f3321c21698ce3d69a3fd7b7cfba74b2b08bb99`, árbol limpio. Ejecución explícita terminada, XML fresco 1/1 **solo como ejecución del arnés**, no como Pass de la hipótesis. [Resumen derivado y SHA-256 de los datos](../validation/evidence/2026-11-11-first-shot-s4-h3b-summary.json). Originales locales ignorados: `Logs/experiments/h3b/20261111T152312Z-5f3321c`.

| Modo | Δ fijo ms | Intentos | Contactos | Al aire | Tarde | Otros | Zona/aciertos observados | Aciertos/intentos | Causal |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---|
| T3 | 0 | 48 | 48 | 0 | 0 | 0 | 48/48 | 48/48 | NotEvaluated |
| T3 | -60 | 48 | 32 | 16 | 0 | 0 | 32/32 | 32/48 | NotEvaluated |
| T3 | -100 | 48 | 32 | 16 | 0 | 0 | 32/32 | 32/48 | NotEvaluated |
| T3 | -150 | 48 | 32 | 16 | 0 | 0 | 32/32 | 32/48 | NotEvaluated |
| V | 0 | 48 | 48 | 0 | 0 | 0 | 48/48 | 48/48 | NotEvaluated |
| V | -60 | 48 | 32 | 16 | 0 | 0 | 32/32 | 32/48 | NotEvaluated |
| V | -100 | 48 | 32 | 16 | 0 | 0 | 32/32 | 32/48 | NotEvaluated |
| V | -150 | 48 | 0 | 48 | 0 | 0 | 0/0 | 0/48 | NotEvaluated |

Son 384 intentos, 256 contactos y 128 al aire, cero tarde/otras categorías. Los 256 contactos tuvieron primer impacto observado en la zona de frontis del fixture. Denominador cero en V/−150: tasa de zona entre contactos **NotEvaluated**, no 0 %; la tasa sobre 48 intentos sí es 0/48. **112 pares con contacto en ambos modos, cero diferencias de paso, geometría y salida**. Desglose por SL/semilla/desfase y ajustes de factibilidad en los originales; totales por SL en el JSON derivado.

Interpretación limitada: frente al preciso, los bots anticipados pierden contactos por K; V filtra además −150 ms. Entre los contactos aceptados no aparece una pérdida de acierto en esta zona: con capa causal ausente, el solver vigente resuelve la intención factible. No se inventa una penalización ni se exige empeorar cada dimensión. La separación del límite V y la geometría permite observar qué cambió sin atribuirlo a un vector inexistente. El bot lee el plan presentado y usa ImpactCue como referencia lógica; no mide percepción humana, render, pantalla, audio ni captura. **H3b permanece incompleto/NotEvaluated como hipótesis completa** por el vector causal y los niveles parcial/ninguno pendientes.

## T4.1 — cierre de ciclo y comprobación de UI

Ampliada la verificación: emisión sintética con jitter real frente al pulso nominal; toque duplicado/faltante con timestamps monótonos; modal que bloquea juego y cancela sin aplicar; resultado ya confirmado que no debe registrarse como cancelado al destruir la vista. La preparación de UI necesita desplegar los foldouts y esperar layout/foco para enviar NavigationSubmit, como el resto de pruebas del panel; intermedios preservados. Con la UI preparada: **RED 12/13, un fallo por aserción** al cancelar un resultado confirmado. Guarda `!confirmed` en Cancel, sin alterar aserciones/cálculo: **GREEN 13/13**. El resumen visible ahora incluye medias/DE por serie y descansos, además del IC/rango. Evidencia en `Logs/evidence/s4-timing/calibration-lifecycle`.

## Regresión completa fresca

Fuente `f1870da`: **562/562 EditMode y 368/368 PlayMode**, cero skipped/fallos/errores CS, XML nuevos. PlayMode incluye 355 existentes en la base #44 + 13 de calibración. EditMode incluye 10 del bot sobre la base vigente. Originales preservados en `Logs/evidence/s4-timing/regression`; GraphicsSettings/QualitySettings se restauran al terminar conforme al encargo. No se ejecutaron todavía check-release ni AutoQA; no se declaran aprobados.

## S4-TIMING-02 — discrepancia de visibilidad del remoto; pausa antes de publicar

Durante la revisión previa a publicar, `gh repo view --json isPrivate,viewerPermission` informó `isPrivate=false`. Confirmación explícita independiente de la selección implícita: `gh repo view rafaallendes-boop/squash-clash --json nameWithOwner,isPrivate,url` devolvió `rafaallendes-boop/squash-clash`, `isPrivate=false`, URL `https://github.com/rafaallendes-boop/squash-clash`. El origin local apunta a ese mismo repositorio y no hay override GH_REPO. Es el estado leído de GitHub, no un cambio realizado por esta rama. Las instrucciones describen un remoto privado y prohíben cambiar su visibilidad. No se modificó la visibilidad, no se hizo push de esta rama ni se creó el PR. No se atribuye quién/cuándo cambió ese estado.

Se aplica la instrucción del encargo «Ante cualquier defecto o sorpresa: registra y detente». Consulta al PO: confirmar que la visibilidad pública es intencional y autorizar continuar los checks pendientes y publicar el PR allí conservándola. **Ningún cambio de visibilidad autorizado ni realizado por inferencia.** Rama y evidencia local preservadas. Implementación y experimento terminados según se informa arriba; entrega final de PR todavía pendiente, sin fusión.

## Decisión del PO sobre S4-TIMING-02 — publicación autorizada

Respuesta literal: **«Sí, es intencional; continuar y crear el PR»** a la consulta sobre la visibilidad pública de `rafaallendes-boop/squash-clash`, continuación de checks y publicación conservando esa visibilidad. Sustituye la pausa vigente de la sección anterior. No se cambia visibilidad. Pendientes inmediatos: check-release dev/prod desde árbol limpio, AutoQA PASS/salida 0/nivel 1 y PR a main listo para revisión coordinada por el PO, sin fusionar.

## Verificación final y entrega técnica

- Regresión fresca sobre `f1870da`: **562/562 EditMode, 368/368 PlayMode**, sin errores CS ni skipped. Producto idéntico hasta el head de exports/AutoQA `2fc2afef651ca39d3fd4dc6441eab7e87ba65549`; solo documentación entre ambos. Esqueletos/intermedios y RED válidos preservados según las secciones TDD.
- `scripts/check-release.sh`: **dev/prod RELEASE-CHECK: OK**, metadata `gitHash=2fc2afe`, **gitDirty=false** ambos. DEV: Core/Runtime/DevPanel; PROD: solo Core/Runtime. Código generado de LatencyCalibration presente en DEV y ausente en PROD; framework de pruebas excluido, puentes térmico/háptico y símbolos comprobados por el script. Exports Unity/IL2CPP, **sin build nativa Xcode ni instalación**. Logs/metadata copiados a `Logs/evidence/s4-timing/release`.
- `scripts/autoqa.sh mvp`: **PASS, salida 0, nivel 1**, `dirty=false`, 85/85 tests, **18/18 PASS**, canario FAIL esperado. Determinismo de los seis escenarios idéntico; hashes/presets/guard/SHA de evidencia correctos. Corrida `20261111T153744Z-2fc2afe-25993`, sin cambiar escenarios, márgenes ni tolerancias. Restauración automática de transitorios sin errores, estado limpio antes/después; GraphicsSettings/QualitySettings también cotejados con Git al cerrar. No fue necesaria repetición.
- **607 archivos existentes protegidos** (incluyendo spec/planes, AGENTS/aprobaciones comunes, código ajeno y scripts) cotejados byte a byte con `18b796a`; se exceptuó exclusivamente la inserción de la rutina de timing en ShotSection. **609 hashes de fuentes actuales** conservados en evidencia local y cotejados tras AutoQA. Los 13 paths de la entrega están dentro del alcance autorizado. `device.env` sigue idéntico al origen autorizado, ignorado y sin versionar.
- Invariantes comprobadas por regresión/experimento y cotejo de bytes: defaults **c0d8ec7050e0 / 107**, P0 **6ecfdfa78cbb**, SimulationKey **166a96e319c7**, proyección 41 heredados **0d39e6016bd8**, M2-4, pullFullMm **35**, P2. Sin clave nueva, promoción de presets ni cambio de contrato/reloj, ImpactCue/TimingRingView, spec/planes/criterios/umbrales.

### Lo que sigue NotEvaluated o pendiente de decisión

Calibración física y efecto del offset en el iPhone; legibilidad/uso real de #44; vector causal productivo y niveles parcial/ninguno de H3b, y por tanto H3b como hipótesis completa; H2/zonas/bandas propuestas; implementación de 4.2; 4.4/4.5; condiciones pendientes de S3. Las pruebas sintéticas no acreditan el protocolo físico. El Step 0 y el protocolo de arriba son propuestas, no autorizaciones de ejecución. Sin iPhone, Duplicación, sesión, instalación, #32, First Rally/First Match, cambios de visibilidad o fusión.

Entrega para revisión de Claude coordinada por el PO, **sin fusionar ni declarar aceptación de S4**. Se detiene el trabajo al entregar el PR; decisiones de 4.2 y toda actividad física requieren autorización propia.

## Revisión de Claude y correcciones autorizadas de PR #45

Claude revisó el head `8e6e1c25d5ef33b7c56b58cb7993517f0dc88f29`: **APROBADO CON OBSERVACIONES, sin bloqueantes**. Reprodujo 562/562 EditMode y 368/368 PlayMode. El verificador autoqa-check v7 coincide con el operador en `20261111T153744Z-2fc2afe-25993`: PASS/salida 0/nivel 1, 41/41 SHA.

Autorización literal del PO: **«Dejame el prompt para que codex arregle las observaciones y luego haga el merge»**. Este primer commit registra la autorización antes de tocar código. Misma rama/PR; solo las correcciones siguientes, verificación completa y merge commit condicionado a los controles del encargo, sin aceptación de S4.

1. **O1, código/TDD:** CanToggle debe rechazar mientras el overlay de calibración está activo y conservar el diagnóstico dev-control existente; al cerrar, BlockedByUi debe reflejar el estado real de DEV, no blockedBefore. PlayMode debe probar Toggle durante modal y alineación tras Cancelar.
2. **O2, código/TDD:** metricsValid del sobre de latency-calibration debe ser `!app.Clock.MetricsInvalid`. La validez de la calibración queda en payload valid/kind/reason. Probar ambos sin confundirlos.
3. **O3, documentación:** −100 ms es exactamente el borde inclusivo de V; V/−100=T3/−100 confirma su inclusión. Las 16 pérdidas por desfase anticipado pertenecen íntegramente a SL3/SL4. Sin reejecutar ni cambiar el experimento, desfases o recetas.
4. **O4, sin cambios:** H3b sigue NotEvaluated como hipótesis completa.
5. **O5, documentación:** protocolo futuro debe advertir la asincronía media negativa de tareas de seguir un ritmo (Repp, 2005): el offset puede incluir ese sesgo y no trasladarse igual a intercepción. Información para la confirmación del PO, sin nuevo umbral/criterio.
6. **O6, documentación:** entrada vigente idéntica en AGENTS.md y aprobaciones First Shot con autorización S4 limitada, decisión literal de visibilidad pública intencional que sustituye el estado privado histórico y observaciones de Claude. Sin reescribir histórico. Cierre del merge se registra en la siguiente entrega, sin commit aparte en main.

Delta permitido desde `8e6e1c2`: LatencyCalibration.cs, DevPanelView.cs, LatencyCalibrationTests.cs (test nuevo/.meta solo si necesario), informe S4, AGENTS.md y aprobaciones. Sin contrato/reloj/ImpactCue/TimingRingView/solver/P2/AutoQA/spec/planes/criterios/umbrales, 4.2/4.4/4.5, iPhone, build nativa, instalación, #32 ni áreas de Fable. Ante defecto, sorpresa, archivo ajeno o RED no basado en aserción, registrar/detener sin fusionar.

Pendiente: RED/GREEN de O1/O2, suites completas frescas, restauración de transitorios, check-release dev/prod limpio, AutoQA PASS/0/1, cotejo de invariantes/delta/head/PR/árboles; solo entonces merge commit con segundo padre exacto y árbol idéntico, main sincronizada y preservación verificada de ignorados antes de retirar worktree sin force, conservando rama remota.

### O1 — RED/GREEN

Esqueleto existente y tres pruebas nuevas: **RED válido 14/16**, dos fallos por aserción (Toggle cambiaba DEV y cierre con DEV cerrado dejaba input bloqueado), sin CS. Propiedad IsActive del modal en CanToggle, motivo `dev-unavailable` existente y restitución según DevPanelView.IsOpen: **GREEN 16/16**, XML/log frescos. La misma guarda es la que consulta ThreeFingerHold; se comprueba el motivo registrado. Sin cambios de reloj/gesto/contrato ni parámetros. Evidencia `Logs/evidence/s4-timing/claude-fixes/o1`.

### O3/O4/O5 — precisiones documentales sin nueva medición

**O3:** el desfase −100 ms cae exactamente en el borde temprano inclusivo de V=100 ms. La igualdad V/−100=T3/−100 confirma **inclusión del borde**; no demuestra permisividad más allá de V. Las 16 pérdidas por K repetidas en los desfases anticipados son **8 SL3 + 8 SL4**, íntegramente esas dos recetas. En V/−150 hay además 32 rechazos del límite V en las otras recetas, para 48 fallos totales; no se confunden con esas 16 pérdidas geométricas. Cotejo por lectura del JSON derivado existente; no se reejecutó H3b ni se cambiaron datos, desfases, recetas o arnés.

**O4:** H3b conserva **NotEvaluated como hipótesis completa**. Sin nueva implementación causal ni niveles de ayuda.

**O5:** incorporada al punto 4 del protocolo futuro la advertencia sobre asincronía media negativa, con la revisión original de Repp (2005), §3, pp. 973–974. La extrapolación del offset de seguir un ritmo a la intercepción de una pelota queda como limitación no demostrada. No se resta ese sesgo, no se cambia compensación/default ni se añade criterio.

### O2 — RED/GREEN

Nueva prueba del writer con las cuatro combinaciones reloj válido/inválido × payload válido/inválido: **RED válido 16/17**, un fallo por aserción del sobre, sin CS. Writer corregido a `!app.Clock.MetricsInvalid`, sin modificar payload: **GREEN 17/17**, XML/log frescos. La prueba conserva valid/kind/reason; líneas de contexto ya no implican por sí mismas métricas inválidas del reloj. Evidencia `Logs/evidence/s4-timing/claude-fixes/o2`.
