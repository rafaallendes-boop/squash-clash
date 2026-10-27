# Prototipo S1-sling — implementación en curso

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

## Sesión física — en curso, sin resultados inferidos

Se recordó antes del primer bloque que S1 no golpea la pelota y que el nuevo control fija por quietud, sin acelerar; tensado descendente desde fijación, regreso reduceP, cancelar siguiendo✕ y soltar. Se pidió **solo B0** con izquierda, comprobar S1-sling/cero pendientes al abrir DEV, no iniciar B1 ni otros protocolos. El PO debe identificar estados y cancelación voluntaria antes de B1. No hay respuestas ni resultados de B0 registrados todavía.

Pendientes: B0–B5 bajo protocolo aprobado, hash antes de cada bloque, respuestas vinculadas en B1/B3,16 preguntas literales separadas y restauración P0 en nuevo arranque/hash **e458bfd5ce9e**/cero pendientes/tres archivos iguales. No se mezclan datos con campañas A′ anteriores ni análisis offline. Ante un defecto, registrar y detenerse. No aceptarS1 ni iniciarS2–S4/T8. PR #25 abierta en borrador mientras la validación sigue pendiente, **sin fusionar**.
