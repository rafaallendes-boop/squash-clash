# Prototipo S1-sling — entrega técnica; práctica detenida antes de B1

Base main 956cfaf, PR #25, rama firstshot/s1-sling-prototype. Interacción aprobada por Claude en d114e26 y continuación autorizada por PO. Ajustes documentales obligatorios aplicados **antes de código**, commit f7f049c. [Interacción y protocolo](2026-10-27-first-shot-s1-sling-prototype-interaction.md), [decisión literal](2026-10-25-first-shot-po-approvals.md).

S1 sin aceptar. Sin contacto con pelota ni S2–S4/T8. A′ conserva su ruta y queda por defecto.

## TDD registrado hasta ahora

- Núcleo SlingModel: esqueleto compilable, RED fresco **20/20 fallos por aserciones** → GREEN **20/20**. La primera preparación utilizaba la inicialización cero de un struct C# en vez de su constructor con argumentos; se corrigió exclusivamente el fixture antes del RED válido. Ambas ejecuciones conservadas por separado en Logs/evidence/s1-sling/tdd; la preparación no se presenta como RED válido.
- Protocolo/configuración: RED compilable **12/12 fallos por aserciones** → GREEN focal ampliado **25/25**, incluidos casos de selección/riesgo históricos intactos. Un error CS en la preparación de un test (Count de List) se corrigió antes del RED; no cuenta como evidencia RED.
- Integración Main/vista/protocolo: RED fresco **7 pruebas, 1 Pass/6 Fail**, sin errores CS. GREEN focal ampliado **8/8**. Se añadió una prueba de geometría nativa (RED1/1 por aserción de botones ausentes → GREEN). La prueba B1 comprueba además que esperar/pulsar la respuesta no crea otro gesto ni cambia el registro ya terminado.

## Compatibilidad de configuración

23 claves nuevas autorizadas de control, quietud, tirón, zona, destello y objetivos de protocolo; 85 claves totales. Defaults **45c4e9af5c8f**, con gesture.control=0 (A′); preset experimental separado **S1-sling 7d1a61c39766**, control=1/repeat=0, no promovido. Los 62 valores previos se cotejan individualmente contra una copia exacta del JSON de main en el fixture pre-sling-defaults.json. Las comprobaciones históricas de 58/62 claves usan esa proyección explícita; no cambian las aserciones de los casos de comportamiento/riesgo. Los 41 heredados/proyección **0d39e6016bd8** y SimulationKey **166a96e319c7** siguen intactos.

El destello se denomina **aim.lockFlashMs** en el catálogo, equivalente a aids.aimLockFlashMs propuesto en el diseño: queda en el snapshot de gesto y no añade una clave al grupo histórico First Ball aids. Objetivos X normalizados en coordenadas visuales de pantalla; AX-1 invierte X al convertir a metros del frontis, sin cambiar cámara.

## Integración y vista

La ruta S1-sling está en un parcial de GesturePreview; usa el mismo TouchInputAdapter y tracker de muestras, sin autoridad de pelota/predicción ni input paralelo. El modelo A′ sigue su ruta anterior. El selector y parámetros quedan capturados al apoyar. El control nuevo resuelve su Ended idéntico sin cambiar el problema de potencia pendiente en A′. DEV/foco/pausa/relanzamiento invalidan sin inventar r.

Estados/dirección de cancelación/texto de potencia en una línea runtime fuera del frontis, incluso si la build carece de DevPanel. Objetivos, protocolos y respuestas humanas quedan en la assembly DEV. B1/B3 bloquean nuevos intentos hasta una respuesta explícita vinculada por attemptId; respuesta y gesto son registros separados. Las marcas T1–T5 se dibujan finas en negro para contrastar con el frontis; el identificador/potencia pedida está fuera del frontis. Zona local opcional omitida para evitar oclusión; se conserva ✕, flecha, distancia y estado compacto.

Geometría PlayMode con Main/cámara por defecto/1206×2622 y área segura del iPhone: feedback/status sin intersección con frontis completo suelo→out, sobre DevBar; T1/T2 mantienen lado visual esperado. Captura editor sintética en Logs/evidence/reticle-visibility/editor-sling-feedback.png, no medición física. Los mínimos anteriores de mira siguen comprobados por sus pruebas sin cambiar constantes/cámara.

Revisión de borde adicional: un arrastre fuera del área útil no puede fijarse por tener espacio arriba. Test nuevo RED por aserción; guardia area.Contains añadida, sin variar dimensiones/umbrales ni cancelar automáticamente un toque al salir de la región.

## Regresión final antes de exportar

Suites completas frescas **404/404 EditMode y 180/180 PlayMode**, sin errores CS. El caso de borde del área útil pasó junto con toda la regresión. Caso adicional de protocolo: mantener visible la respuesta pendiente al cambiar selector en DEV, RED por aserción → GREEN con PlayMode completo, sin desbloquear ni inferir una respuesta. [Resumen verificable y hashes XML](../validation/evidence/2026-10-27-s1-sling-implementation.json), [captura sintética del editor](../validation/evidence/2026-10-27-s1-sling-editor-feedback.png).

242 fuentes protegidas cotejadas contra main: spec v0.3, documentación anterior salvo adendas/registro autorizados, scripts/Packages, EffectiveConfig, AimModel y los estimadores anteriores intactos. Los dos transitorios de PlayMode (GraphicsSettings/QualitySettings) se preservan en un patch ignorado y se restauran antes de exportar; ProjectSettings final idéntico a main. No cambian criterios/bandas/umbrales ni se recalculan trazas antiguas.

## Build, perfil y preparación física

Producto **db72a00f7f4ffebc79d3708cb8d8c8e29a7f27a8**. check-release dev/prod OK con exportaciones frescas, gitDirty=false y módulos correctos: DevPanel solo dev, ningún framework de tests en Players, puente térmico runtime presente. Build dev nativa **Release ARM64/no Development** correcta, sin errores Xcode.436 archivos de fuente cotejados con el árbol probado. Sin cambios de producto posteriores.

Perfil **incorporado en la app** comprobado antes de instalar, 2026-10-27T17:04:06.849264Z: válido hasta **2026-10-28T14:59:31Z**, certificado vigente, dispositivo incluido, equipo/bundle coincidentes y firma válida. No renovación realizada por Codex; la renovación corresponde al PO según el último encargo. No instalar si falta vigencia efectiva; la condición del encargo técnico exige renovación previa al día28.

Respaldo nuevo antes de instalar; P0-calib-1, Po-1 y last-active cotejados directamente byte a byte después de instalar y después de preparar. SHA-256 en evidencia resumida. Arranque de **db72a00 dev** observado con P0-calib-1/**e458bfd5ce9e** extendido y cero pendientes. P0 conserva su JSON original41 claves, sin reescribirlo.

S1-sling copiado como archivo separado, cargado desde DEV **sin guardar**. La operación Cargar cambia la selección persistida: se repuso last-active original mientras el candidato seguía en memoria, antes de aplicar/relanzar. Lanzamiento **2026-10-27T17:09:36.0043670Z**, nombreS1-sling/hash **7d1a61c39766**, repetición0 y cero pendientes; los tres archivos originales iguales al respaldo. DEV cerrado, fila B0 accesible, **cero gestos sling antes de entregar el teléfono**.

## Sesión física B0 — detenida antes de B1

[Evidencia nueva sanitizada: todos los intentos, muestras y auditoría descriptiva](../validation/evidence/2026-10-27-s1-sling-B0.json). Builddb72a00, hash **7d1a61c39766**, cero pendientes en todos los intentos; ningún cambio de configuración ni relanzamiento durante la práctica. Las dos aperturas B0 registran repeat0/controlS1-sling/hash esperado. Datos separados de campañas A′ y análisis anteriores.

Antes de B0 se explicó quietud/fijación/tensado/cancelación y que S1 no golpea la pelota; se pidió izquierda y solo B0. El PO informa literalmente:

> Revisa lo que he hecho. pero a veces podía apuntar y se apuntaba bien, luego tensaba, otras veces se mantenía apuntando.

Hay **33 intentos:32 en B0 y uno preview entre las dos prácticas**, todos conservados, sin sustitución. No se infiere una nueva confirmación de mano de esta respuesta: izquierda era la instrucción y declaración previa. Dos adquisiciones de B0 **incompletas**, de85,918 s y35,442 s, ambas system-cancel. No se suman para declarar120 s continuos; la causa concreta de cada cancelación de adquisición no está registrada. Ningún B1–B5 iniciado.

| Resultado de los33 intentos | n | Lectura |
|---|---:|---|
| Fijación y outcomeSwing |17|14 llegaron aTensando y3 terminaronFijada/P0. Swing sigue siendo solo previsualización. |
| Abandono antes de fijar |15|14 dentroB0 y1 preview entre prácticas. |
| Tap |1|Toque conservado; no se reclasifica como intento exitoso. |
| Entrada en zona/cancelación por zona |0/0|No acredita cancelación ni constituye cinco cancelaciones fallidas. |

Fijaciones: Began→fijación mínimo/mediana/máximo **0,521/1,084/7,568 s**. Último movimiento fuera de tolerancia→fijación **120,808/137,536/1133,460 ms**. Esta segunda magnitud no es necesariamente el tramo continuo de quietud: los huecos reinician soporte sin modificar el timestamp del último movimiento fuera de tolerancia. Es descripción de17 fijaciones en práctica, sin umbral nuevo ni juicio de fluidez inferido.

### Por qué algunas veces permaneció Apuntando

Auditoría descriptiva de las muestras **con las reglas efectivamente instaladas**, sin modificar parámetros ni calcular resultados de un candidato alternativo. No son nuevas mediciones del dispositivo. Se conservan muestras/timestamps y condiciones; la reconstrucciónPython usa double y se identifica como tal en JSON.

- **13 abandonos**: no hubo una ventana real acreditada de120 ms con mínimo3 tiempos, velocidad≤15 mm/s, radio≤1 mm, hueco≤51 ms y arrastre previo≥3 mm. Las máximas quietudes apoyadas antes del final no superaron100 ms. Movimiento y huecos reinician el candidato; son causas superpuestas, no porcentajes independientes. Un silencio largo no cuenta como estabilidad demostrada.
- **2 abandonos**: hubo quietud suficiente, pero los eventos que cumplían estaban **fuera del área útil**; lockBlocked=true. La protección de margen impidió fijar y no se desplazó ni comprimió la zona para hacerla caber.
- **1 Tap**: se conservó como toque. Los17 casos con ventana válida dentro del área útil corresponden a las17 fijaciones registradas. Esta auditoría no encontró una ventana que cumpliera todas las condiciones y quedara sin fijar; no demuestra que la interacción resulte cómoda.

No se atribuye la intención de mantener quieto el dedo a los huecos ni se infiere potencia voluntaria de P. No se cambian120→150 ms, tolerancias, área, zona, escala ni límites para mejorar el resultado. No se declara validación de suave/media/fuerte, precisión por objetivo ni seguridadRB6 con práctica libre.

### Bloqueo físico SL-S1-01: cancelación no identificada

A la aclaración, el PO responde literalmente: **«No llegué a ver esa zona»**. No hay CancelEntered ni ZoneCanceled. No permite saber que la detección geométrica falló: no existe en la evidencia una liberación dentro de la zona. La UI implementa la indicación compacta indirecta✕/flecha/distancia, sin banda opaca ni marca local sobre el frontis; su reconocimiento por el PO no quedó establecido.

La [interacción aprobada, §3](2026-10-27-first-shot-s1-sling-prototype-interaction.md#presentación-sin-ocultar-el-frontis) exige identificar región/estado enB0 antes deB1: «Si no los entiende, registrar y detenerse». Se aplica esa parada; **B1–B5 no iniciados, cancelación NotEvaluated, sin aceptación deS1**. Las16 preguntas finales no se solicitan tras esta parada previa: no hay respuestas inferidas ni una campaña completa que resumir.

**Propuesta para una entrega posterior, pendiente de PO/Claude y sin implementar:** revisar la enseñanza/indicación de la zona enB0: explicar su referencia al dedo al fijar y cuándo soltar, con dirección/estado claros fuera del frontis; comprobar una cancelación guiada antes de protocolos formales. Mantener las dimensiones aprobadas hasta otra decisión expresa. No resolver este bloqueo cambiando silenciosamente la geometría.

### Observación SL-S1-02: tiempo de práctica no visible

Comentario literal del PO:

> En ninguna parte de la pantalla me aparecía el tiempo, entonces en la 1ra pensé que ya habían pasado los 2 minutos, quizás es un punto de mejora de poner un timer en estas pruebas

El overlayB0 no muestra cuenta atrás; el estado final se guarda en el detalleDEV y la franja del bloque desaparece al finalizar. No se infiere de su respuesta qué acción causó cada system-cancel ni que la segunda práctica terminara por el mismo motivo. **Propuesta del PO registrada, sin implementación:** temporizador visible/fin e interrupción inequívocos en la práctica. Cualquier cambio se somete a una PR autorizada con su verificación.

La dificultad de fijación es otra observación de uso: puede estudiarse con feedback del progreso/reinicio del detector durante práctica, sin inventar muestras porframe ni bajar umbrales a partir de esta tanda. Es una propuesta de diagnóstico, no un ajuste elegido o autorizado.

## Restauración y entrega

Tras detenerB0 se reinició la app, leyendo la selecciónP0 original. Primer lanzamiento P0 del nuevo proceso **2026-10-27T17:21:51.8776630Z**, builddb72a00 dev, hash extendido **e458bfd5ce9e**, cero pendientes. P0-calib-1,Po-1 ylast-active cotejados **directamente byte a byte** con el respaldo; selecciónP0-calib-1. Los436 archivos de producto/script/configuración coinciden con el árbol probado. Ningún cambio posterior de código, tests, defaults, spec, plan, criterios o parámetros; sin nuevas suites/builds/instalaciones ni reintentos físicos después de la parada.

PR#25 preparada para revisión técnica deClaude, **sin fusionar**. La entrega técnica y la restauración no aceptanS1 ni validan el prototipo: B0 incompleto y cancelación no identificada; B1–B5 y las16 preguntas quedan pendientes de una decisión posterior delPO. S2–S4/T8 siguen sin autorizar. Comprobar perfil efectivo antes de toda instalación futura y renovación a cargo delPO antes del límite indicado; vence2026-10-28T14:59:31Z.
