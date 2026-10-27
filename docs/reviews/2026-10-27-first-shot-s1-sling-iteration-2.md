# S1-sling, iteración 2 — adenda de diseño y predeclaración del replay

PO2026-10-27, misma rama y PR #25, base revisada **508f625982c7998f13f315cc8f7ea993f40c5c13**. Claude confirma entrega técnica y parada SL-S1-01 correctas, restauración P0 verificada y SL-S1-02 registrada. **SL-S1-03 es un defecto del diseño aprobado, responsabilidad reconocida por Claude**: el detector exigía eventos durante la quietud. No se reescribe el diseño anterior ni se presenta como error de cumplimiento de db72a00. [Diseño aprobado](2026-10-27-first-shot-s1-sling-prototype-interaction.md), [informe histórico](2026-10-27-first-shot-s1-sling-prototype.md), [registro PO](2026-10-25-first-shot-po-approvals.md).

## D2 — decisión del PO

Con contacto primario activo y sin Ended/Canceled, el reloj real cuenta la quietud desde el candidato anclado en la última posición válida. Tick por frame evalúa fijación sin añadir TouchSample. Se conservan120 ms/1 mm/15 mm/s/arrastre≥3 mm/área útil/mínimo3 muestras reales. Se conserva el mínimo por candidato de quietud del código anterior: Tick no incrementa ese contador. El hueco51 ms deja de reiniciar el candidato con contacto activo; la clave anterior permanece para compatibilidad de presets/hash, sin servir de veto al nuevo detector. Nueva muestra fuera de radio o velocidad reinicia; pausa/foco/DEV invalidan antes del Tick. Ended/Canceled procesados no pueden fijar por reloj. A′ intacto.

Registrar origen sample/timer, instante teórico de elegibilidad, instante del frame/observación y última muestra real usada. El instante teórico es el mayor entre quietAt+120 ms y el timestamp de la muestra real que completa el mínimo requerido. No se inventan muestras ni se afirma que el frame ocurrió en el instante teórico. Fijación única por gesto; potencia sigue desde el punto fijado.

Indicación local de cancelación reconocible respecto del dedo: dirección, límite y estado dentro/fuera; nunca intersectar el frontis. Dimensiones12/20/8 intactas. Guía en pantalla de una cancelación enB0 antes de finalizar. Temporizador visible, cuenta atrás B0 y fin/interrupción explícitos. Son cambios autorizados, sin promover defaults ni modificar umbrales/criterios.

## Replay diagnóstico predeclarado, antes de implementar

Fuente única [33 intentos originales de B0 db72a00](../validation/evidence/2026-10-27-s1-sling-B0.json), inmutable. Reproducir los silencios/velocidades citados por Claude y aplicar D2 con mínimo3 real por candidato. Todos los intentos, incluido Tap y preview, permanecen; nada se reemplaza.

Dos lecturas temporales, declaradas **antes de calcular**: (1) elegibilidad teórica ideal usando timestamps originales, procesando una muestra terminal antes de un temporizador coincidente; (2) grid sintético60 Hz con fase en Began y eventos procesados antes de Tick al coincidir. Este grid es una hipótesis de replay, **no frecuencia real del iPhone ni sus frames observados**. No se interpola movimiento: entre eventos se conserva la última posición reportada mientras el contacto siga activo. Tiempo de recepción se conserva en las fuentes, pero el replay principal usa tiempo del evento y no reproduce la cola del player.

Comparar instante/posición/fijación con el registro original. Señalar fijaciones nuevas/anticipadas y movimiento posterior que podría mostrar fijación involuntaria: distancia hasta el objetivo final libre calculado con la misma ganancia, descenso posterior y proximidad de nueva muestra que rompe la quietud. **Sin umbral de intención ni clasificar como involuntaria sin juicio del PO.** La cifra aproximada de Claude (~5/13, mediana~0,6s) es orientativa, no un objetivo que el auditor deba alcanzar ni medición.

Después: TDD con RED por aserciones, regresión completa, check-release dev/prod y build dev Release ARM64/noDevelopment. **No instalar hasta renovación previa del perfil por el PO y comprobación efectiva del incorporado**, además de respaldo/cotejo de los tres archivos. Sesión nueva separada: B0 completo120s/con guía; identificación antes de B1–B5/16preguntas. Restaurar P0 y cotejar al cerrar. S1 sin aceptar, S2–S4/T8 sin autorizar; PR sin fusionar.

## Resultado previo a implementación — solo diagnóstico offline

[Auditoría/replay completo por intento](../validation/evidence/2026-10-27-s1-sling-iteration-2-replay.json), fuente original inalterada. Predeclaración commit58d037d. Se reproducen los silencios:8,16,22,25 tienen extremos separados≈0,055–0,110 mm y huecos129–1750ms;11 tiene175,041ms/0,156mm;21 y27 incluyen4596,670/3042,191ms. **1548 pasos de velocidad>15mm/s a lo largo del gesto completo,3 con≤2px**; antes de la fijación original o final hay1079. No llamar a los1548 reinicios reales del detector, porque incluyen la fase de potencia posterior a fijar.

D2 ideal: **23 fijaciones frente a17 originales**. Seis nuevas en ordinales**8,11,16,22,25,26**;17 originales anticipadas. MedianaBegan→fijación**654,221ms**. Grid sintético60Hz también23, sin afirmar frames reales. Diferencia con ~5/13 deClaude preservada, sin ajustar algoritmo para coincidir. Son6 de los13 abandonos sin ventana acreditada en el auditor anterior; incluyen el preview22. Los2 abandonos fuera de área yTap siguen sin fijar.

Candidatos a revisar por fijación involuntaria, **sin etiqueta de intención**: nueva11 separa su mira fijada del final libre≈1,142m y hay12,313mm de descenso posterior; anticipadas21/27 separan≈3,115/4,326m y tienen17,670/50,966mm de descenso posterior. Las trazas originales se hicieron con otra política de fijación, así que esos movimientos pueden incluir tensado deliberado. No son errores físicos medidos del controlD2 ni autorizan rebajar umbrales. El JSON conserva los indicadores continuos de los33 intentos.

Fuentes de referencia: [UIKit, manejo del toque](https://developer.apple.com/documentation/uikit/handling-touches-in-your-view), [fase stationary](https://developer.apple.com/documentation/uikit/uitouch/phase-swift.enum/stationary). La fase describe un contacto sin desplazamiento; no exige un nuevo evento periódico por cada frame. El adaptador local consume eventos raw; Tick usa reloj real y estado de contacto vivo, no crea eventos táctiles. Se mantiene InputSystem1.20.1 ya instalado, sin modificar Packages.

## Implementación y TDD

RED válido del núcleo:27 casos,24Pass/3Fail por aserción →27/27. Tick evalúa candidato activo, mínimo real3 intacto; el tiempo no incrementa el contador. Elegibilidad teórica considera quietud, muestra que completa soporte y entrada en área útil; LockObservedAt conserva el instante real de evaluación y LockSampleAt identifica la muestra usada. Contacto terminado/cancelado y contexto inválido no fijan. Movimiento antes del plazo reinicia. Los campos se registran en schema2/quietPolicy=active-contact-timer-D2; ninguna muestra de reloj se añade al raw.

RED válido de Main/vista:3/3 fallos por aserción →3/3. Se conserva aparte el RED adicional de desborde de guía. Hubo una caída nativa del editor antes de ejecutar tests, durante inicialización Mono/UIElements, y errores de preparación de nombres de namespace corregidos: esas ejecuciones no produjeron XML nuevo y **no cuentan como RED ni GREEN**. La regresión final sí es fresca, sin erroresCS: **411/411 EditMode,184/184 PlayMode**. Una prueba suplementaria del modelo C# sobre el grid sintético reproduce23 fijaciones y los mismos6 casos nuevos; GREEN directo, no se inventa RED ni medición física.

El límite de cancelación se dibuja en su coordenada real relativa al dedo al fijar, recortando segmentos contra frontis/HUD/controles; una tarjeta próxima en espacio libre indica dirección, distancia y DENTRO/FUERA. No se mueve la detección12/20/8 para acomodar la vista. B0 muestra guía de entrada/suelta y registra una cancelación guiada si apareceZoneCanceled. Temporizador/estado completado-interrumpido permanecen visibles;B0 cuenta atrás120s y los otros bloques muestran tiempo e intentos. La ausencia de cancelación guiada al terminar se señala, sin aceptar automáticamente la comprensión del PO.

[Captura sintética del editor](../validation/evidence/2026-10-27-s1-sling-iteration-2-editor-cancel.png), [evidencia automatizada](../validation/evidence/2026-10-27-s1-sling-iteration-2-implementation.json). El fixture aísla el indicador con un modelo simulado; el preview de fondo no tiene gesto activo y su textoApuntando no es el estado del modelo de prueba. No es captura del iPhone ni medición nueva. PruebaMain/cámara por defecto/1206×2622: indicador local, límite, temporizador y guía fuera del frontis; guía contenida en su franja. Se corrigieron márgenes USS que podían desbordar/invadir el rectángulo. Dimensiones de mira anteriores conservadas.

No se añaden claves ni se cambian valores/rangos:85 defaults45c4e9af5c8f, preset7d1a61c39766,41 heredados/SimulationKey intactos. aim.holdMaxGapMs conserva51 en catálogo/presets con título D1 para compatibilidad; ya no veta D2. A′/estimadores anteriores/specv0.3/plan intactos; propuesta e interacción solo reciben D2aditiva y conservan íntegro el texto anterior.

## Perfil y preparación pendiente

El PO pidió asistencia: «Ayudame a hacer esa renovacion. iphone en duplicar telefono ok». Se respaldó y apartó **solo la caché del perfil de esta app**, verificando sus bytes; certificados, claves y app instalada sin cambios. Según [Apple, solicitud de perfil nuevo con firma automática](https://developer.apple.com/help/account/provisioning-profiles/edit-download-or-delete-profiles/), la próxima firma de Xcode solicita un perfil al no encontrar uno local elegible. **Esto todavía no acredita renovación**: se comprobarán fechas, correspondencia y firma del perfil incorporado. No se exportan claves ni se versionan perfiles/identificadores personales.

Respaldo nuevo de P0/Po-1/last-active recuperado, hashes iguales a los previos y selecciónP0. Los transitorios de PlayMode se preservan en patch ignorado y se restauran antes de exportar. Pendientes check-release limpio/dev-prod y builddevReleaseARM64/noDevelopment. Instalación detenida hasta perfil renovado/comprobado; luego cotejo3archivos y campaña nueva separada. No hay sesión físicaD2 todavía. PR#25sin fusionar; S1sin aceptar, S2–S4/T8sin autorizar.

## Build y renovación verificadas antes de instalar

Producto **42265ca**, fuente limpia y440 archivos cotejados. check-release dev/prod fresco/gitDirty=false/módulos correctos. Build dev **ReleaseARM64/noDevelopment**, firma válida. Sin cambios de producto después.

Renovación asistida con Xcode completada, no solo descarga del perfil anterior: nuevo perfil incorporado **creado2026-10-27T18:29:44Z**, vence **2026-11-03T18:29:44Z** (3noviembre15:29Chile). Verificación previa a instalar **2026-10-27T18:30:43.882752Z**: identificador del perfil diferente, creación y caducidad posteriores, misma lista de dispositivos, mismos certificados y entitlements, equipo/bundle/dispositivo correctos, certificado vigente y firma válida. No cambios de claves ni permisos. El perfil anterior queda respaldado fuera de la caché; no se revocó. Esta fecha reemplaza como perfil actual el vencimiento28octubre de los registros históricos. Comprobar siempre vigencia efectiva antes de instalar.

Instalación realizada **después** de esa comprobación. Los tres archivos cotejados directamente byte a byte antes/después de instalar y preparar. La solicitud inicial de arranque desdeCLI no completó; Duplicación pidió autenticación local y el PO la completó. Se abrió la app desde su icono por duplicación: **42265ca dev**, arranqueP0-calib-1/e458bfd5ce9e/cero pendientes observado. Sin gestos del protocolo durante preparación.

S1-sling existente coincide byte a byte con el preset separado de la rama. Se cargó sin guardar; last-active originalP0 se repuso antes de aplicar/relanzar. Lanzamiento preparado **2026-10-27T18:36:39.3090590Z**, S1-sling/**7d1a61c39766**,repeat0/cero pendientes,DEVcerrado, tres archivos iguales y cero registrosD2 de gesto antes del traspaso. El hash de parámetros se mantiene; build42265ca y schema2/quietPolicyD2 distinguen esta sesión delB0db72a00.

## B0 D2 solicitado — todavía sin resultados

Se pidió soloB0/izquierda tras explicar límite/FUERA-DENTRO/suelta y temporizador. Esperar mensajeCompletado sin abrirDEV/salir, y confirmar comprensión de fijación/zona y cancelación voluntaria antes deB1. Si no identifica el límite, detenerse. B1–B5/16preguntas y restauraciónP0 siguen pendientes; no se infieren datos ni aceptación. PR#25en borrador mientras continúa la campaña, sin fusionar.
