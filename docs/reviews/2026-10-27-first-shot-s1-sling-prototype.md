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

## Pendiente

Integración, regresión fresca completa, check-release dev/prod, build dev Release ARM64/no Development, respaldo/perfil efectivo antes de instalar y sesión B0–B5 con PO. No hay instalación ni medición física nueva todavía. La sesión exige verificar el hash antes de cada bloque y restaurar los tres archivos al cerrar. El perfil debe estar vigente; renovación a cargo del PO. No se infiere ninguna aceptación ni dato físico.
