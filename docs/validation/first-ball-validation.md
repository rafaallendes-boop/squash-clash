# Validación integral de First Ball — I7a (T7.1–T7.2)

Registro iniciado el 2026-10-24. **Pendiente de completar por el PO y de revisión de Claude.** I7a no cerrado; PR sin fusionar. T7.3/gameplay no autorizados. Spec, criterios, umbrales y DefaultPreset intactos. Los campos pendientes no cuentan como aceptación.

## 1. Legibilidad (H1)

[H1 de Etapa 6](2026-10-24-first-ball-h1-stage6.md) aprobado por el PO: distingue altura y profundidad en L1–L6 con DEV cerrado, cámara fija y seguimiento («Sí, con fija y seguimiento»). Confirmación global; valoraciones, oclusión del rival y diferencias por mano no desglosadas. No se repite ni se infieren esos datos.

| Caso | Cámara | Altura | Profundidad | ¿Ayuda tapa al rival? (PO) | 1–5 (PO) | Comentario (PO) |
|---|---|---|---|---|---|---|
| L1 | Fija | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L2 | Fija | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L3 | Fija | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L4 | Fija | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L5 | Fija | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L6 | Fija | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L1 | Seguimiento | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L2 | Seguimiento | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L3 | Seguimiento | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L4 | Seguimiento | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L5 | Seguimiento | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |
| L6 | Seguimiento | Sí (confirmación global PO) | Sí (confirmación global PO) | Pendiente | Pendiente | Pendiente |

Comentarios conservados: PO ve bien los seis escenarios y las mecánicas; preferencia futura de drive inicial sin pared trasera, lets/strokes y golpes a pared. No autoriza gameplay ni modifica L1 (BackWall intencional). L2 corregido y aprobado en PR #12: y≤1 m en todo frontis→primer bote. Mano(s)/cámara preferida: **pendiente PO**.

## 2. Física

- Backend de First Ball: **P2-Analytic**, variante **analytic/r2-20261023**; [decisión del gate](../reviews/2026-10-23-first-ball-physics-backend-decision.md), [informe P2](../reviews/2026-10-23-first-ball-physics-spike-P2.md), [18 criterios nativos del gate](evidence/2026-10-23-i4d-p2-native-run.json). P1 configurable como respaldo.
- Aceptación de física T7.1: **Pass**, [campaña nativa auditada](evidence/2026-10-24-i7a-p2-native.json), 18 filas y **11 criterios de gate Pass**, C17 p95 **0,0188 ms**/200 muestras, térmico nominal inicial/benchmark/final y foco 0. Default aplicado antes de iniciar confirmado en JSONL. Editor EditMode/PlayMode completos, diez criterios aplicables de gate Pass; C17 informativo en editor. No confundir con regresión de tests.
- Configuración de campaña: cargar **Default**, hash **0d39e6016bd8**. SimulationKey efectivo P2 **166a96e319c7**. Gate histórico c86243b15a97 tenía backend=0 en el archivo y fábrica P2 inyectada; key cruda **04372f672e9e**. Con selección efectiva P2, key **166a96e319c7**: mismos valores físicos/cancha, bounceThreshold 0,1 y variante. No se afirma igualdad entre las keys crudas. El cambio de selección fue aprobado en I4e; las once claves posteriores añadidas son no físicas (repetición/cámara/depuración/ayudas), explican el resto del hash completo.
- Preset del punto de control de Etapa 5: **P0-calib-1 / 8de3f2ecb217**; solo difiere de Default en launcher.repeatSeconds=3. **No se usa para la campaña del gate ni se promueve a defaults.** Po-1 sigue prueba funcional.
- Batería final **270/270 EditMode y 136/136 PlayMode**, [evidencia](evidence/2026-10-24-i7a-tests.json), [informe I7a en curso](../reviews/2026-10-24-first-ball-i7a-validation.md). Cualquier gate antes Pass que falle: regresión, detenerse.

## 3. Rendimiento (H17)

**Pendiente sesión nativa dev de 15 min.** No inferir resultado de tests, editor, C17 o sesión funcional anterior.

| Campo | Resultado |
|---|---|
| Build dev / fuente / fecha UTC | Pendiente |
| Configuración / SimulationKey | P0-calib-1 (solo repeat=3 frente a Default), física Default / 166a96e319c7; cada L tendrá hash efectivo propio |
| Phase / duración / L1–L6 | session / ≥900 s / 150 s por caso, repetición 3 s de P0-calib-1, sin guardar cambios |
| Térmico inicial | Nominal requerido; pendiente registro |
| Brillo inicial y cambios | Pendiente PO + valor nativo Screen.brightness |
| Bajo consumo | Pendiente PO |
| Panel / cámara lenta / spike / benchmark | Panel cerrado / ninguno durante la sesión; pendiente ejecución |
| Frame p50 / p95 / p99 | Pendiente; p95≤16,7 ms, p99≤25 ms |
| Frames >33 ms por minuto | Pendiente; ≤1/min |
| CPU/GPU p95 y cantidad de muestras | Pendiente; N/D si no hay muestras, nunca cero inventado |
| Peor térmico / segundos en cada estado | Pendiente; sin serious/critical durante 15 min |
| DroppedSteps / foco / cancelación | Pendiente |
| JSONL y resumen de sesión completa | Pendiente |

Percentiles completos provienen de histogramas de toda H17, no del promedio de percentiles de segmentos. Borde superior de cubeta, sesgo ≤0,1 ms, ya documentado por Claude; null con muestras señala overflow >250 ms, null sin muestras señala ausencia. Segmentos JSONL de 10 s se mantienen para trazabilidad. Térmico de la sesión se acumula por estado en cada frame; no se mezcla con el peor estado anterior a H17.

## 4. Invariantes de First Ball (§17.A)

| Invariante | Estado y evidencia |
|---|---|
| 1. Una autoridad, cero atravesamientos, esquinas sin ganancia | Pass: aceptación nativa C1/C3 con cobertura cerrada, cero atravesamientos/ganancia; suites host/regresión 270/136 |
| 2. Consistencia en mismo binario/dispositivo | Pass: aceptación nativa C5/C8, cobertura cerrada |
| 3. Predicción bit a bit | Pass: aceptación nativa C6/C7, cobertura cerrada; C17 nativo p95 0,0188 ms≤1 ms |
| 10. Panel excluido de prod | Pass: exports limpios, prod instalada y arranque devpanel=absent; PO confirma etiqueta prod, sin DEV y tres dedos inertes |
| 14. Preset recuperado, métricas con preset/build/semilla | Pass técnico: regresión existente y JSONL I7a con preset/build/semilla; P0-calib-1/Po-1 intactos. Cargar Default eliminó last-active como corresponde |

## 5. Decisión del PO

**Pendiente PO.** Elegir y comentar: continuar a First Shot / ajustar First Ball / volver. Aprobación de H1 y autorización de I7a no equivalen a esta decisión. No cerrar Etapa 7, modificar spec/defaults, ejecutar T7.3 ni iniciar gameplay sin autorización expresa.

- Decisión: pendiente.
- Observaciones/ajustes: pendiente.
- Fecha y declaración del PO: pendiente.
- Revisión de Claude de I7a: pendiente.
