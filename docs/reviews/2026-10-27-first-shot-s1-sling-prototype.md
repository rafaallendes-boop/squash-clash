# Prototipo S1-sling — implementación en curso

Base main 956cfaf, PR #25, rama firstshot/s1-sling-prototype. Interacción aprobada por Claude en d114e26 y continuación autorizada por PO. Ajustes documentales obligatorios aplicados **antes de código**, commit f7f049c. [Interacción y protocolo](2026-10-27-first-shot-s1-sling-prototype-interaction.md), [decisión literal](2026-10-25-first-shot-po-approvals.md).

S1 sin aceptar. Sin contacto con pelota ni S2–S4/T8. A′ conserva su ruta y queda por defecto.

## TDD registrado hasta ahora

- Núcleo SlingModel: esqueleto compilable, RED fresco **20/20 fallos por aserciones** → GREEN **20/20**. La primera preparación utilizaba la inicialización cero de un struct C# en vez de su constructor con argumentos; se corrigió exclusivamente el fixture antes del RED válido. Ambas ejecuciones conservadas por separado en Logs/evidence/s1-sling/tdd; la preparación no se presenta como RED válido.
- Protocolo/configuración: RED compilable **12/12 fallos por aserciones** → GREEN focal ampliado **25/25**, incluidos casos de selección/riesgo históricos intactos. Un error CS en la preparación de un test (Count de List) se corrigió antes del RED; no cuenta como evidencia RED.
- Integración Main/vista/protocolo: RED fresco **7 pruebas, 1 Pass/6 Fail**, sin errores CS. Implementación pendiente de verificar.

## Compatibilidad de configuración

23 claves nuevas autorizadas de control, quietud, tirón, zona, destello y objetivos de protocolo; 85 claves totales. Defaults **45c4e9af5c8f**, con gesture.control=0 (A′); preset experimental separado **S1-sling 7d1a61c39766**, control=1/repeat=0, no promovido. Los 62 valores previos se cotejan individualmente contra una copia exacta del JSON de main en el fixture pre-sling-defaults.json. Las comprobaciones históricas de 58/62 claves usan esa proyección explícita; no cambian las aserciones de los casos de comportamiento/riesgo. Los 41 heredados/proyección **0d39e6016bd8** y SimulationKey **166a96e319c7** siguen intactos.

El destello se denomina **aim.lockFlashMs** en el catálogo, equivalente a aids.aimLockFlashMs propuesto en el diseño: queda en el snapshot de gesto y no añade una clave al grupo histórico First Ball aids. Objetivos X normalizados en coordenadas visuales de pantalla; AX-1 invierte X al convertir a metros del frontis, sin cambiar cámara.

## Pendiente

Integración, regresión fresca completa, check-release dev/prod, build dev Release ARM64/no Development, respaldo/perfil efectivo antes de instalar y sesión B0–B5 con PO. No hay instalación ni medición física nueva todavía. La sesión exige verificar el hash antes de cada bloque y restaurar los tres archivos al cerrar. El perfil debe estar vigente; renovación a cargo del PO. No se infiere ninguna aceptación ni dato físico.
