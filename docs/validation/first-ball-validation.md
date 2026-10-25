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
- Batería final **270/270 EditMode y 136/136 PlayMode**, [evidencia](evidence/2026-10-24-i7a-tests.json), [informe I7a](../reviews/2026-10-24-first-ball-i7a-validation.md). Cualquier gate antes Pass que falle: regresión, detenerse.

## 3. Rendimiento (H17)

**Dos sesiones nativas completas; objetivo p95 Fail en ambas.** El PO repitió manualmente porque pensó haberse equivocado. Se conservan ambas, sin seleccionar la mejor ni mezclar distribuciones. [Auditoría y hashes](evidence/2026-10-24-i7a-h17.json), datos de validación JSONL [H17-1](evidence/h17/h17-e0f9071-session-1.jsonl) / [H17-2](evidence/h17/h17-e0f9071-session-2.jsonl).

| Campo | H17-1 | H17-2 |
|---|---|---|
| Build / fuente | 0.1.0+e0f9071 dev | Misma build |
| Hora local America/Santiago (24 octubre) | 20:41:23→20:56:23 | 20:58:05→21:13:05 |
| Configuración | P0-calib-1; solo repeat=3 frente a Default | Misma configuración base |
| SimulationKey / Phase | 166a96e319c7 / session | Igual |
| Duración / L1–L6 | 900,000054 s / 50 lanzamientos por caso | 900,002363 s / 50 por caso |
| Frames / FPS medio | 53.985 / 59,9833 | 53.991 / 59,9898 |
| Frame p50 / p95 / p99 | 16,7 / **17,2** / 17,3 ms | 16,7 / **16,8** / 17,1 ms |
| p95≤16,7 ms | **Fail** | **Fail** |
| p99≤25 ms | Pass | Pass |
| Frames >33 ms / minuto (≤1/min) | 1 / 0,066667: Pass | 0 / 0: Pass |
| CPU p95 / muestras | 17,2 ms / 52.491 | 16,8 ms / 52.497 |
| GPU p95 / muestras | 2,5 ms / 52.491 | 2,5 ms / 52.497 |
| Térmico inicial / peor | nominal / nominal | nominal / nominal |
| Segundos nominal | 900,000054 | 900,002363 |
| Segundos unknown/fair/serious/critical | 0/0/0/0 | 0/0/0/0 |
| DroppedSteps | 0 | 0 |
| Brillo | PO ≈60 %; nativo inicial ≈55 % | Iguales valores registrados |
| Bajo consumo | Apagado, confirmación PO | Igual |
| Panel / cámara lenta / spike / benchmark | Cerrado / ninguno | Igual |
| Foco / cancelación | Sin cancelación registrada; completa | Igual |

Condiciones restantes confirmadas globalmente por PO: teléfono físico, sin capturas/Duplicación/herramientas y sin cambios de brillo/modo durante la sesión. Sin herramientas del agente entre solicitud y aviso de término. Se conservan estimación del PO y valor nativo de brillo sin inferir la diferencia.

Percentiles de histogramas de **cada sesión completa** (borde superior de cubeta, sesgo ≤0,1 ms ya documentado), nunca promedio de percentiles de segmentos. No se resta el sesgo ni se relaja el objetivo para aprobar 16,8 ms. JSONL conserva un frame inicial adicional de segmentos frente al resumen H17 que excluye ese delta; CPU/GPU y frames largos contrastados. Dos flushes en transiciones automáticas L4→L5/L5→L6 muestran pendientes transitorios (4/6), seguidos de launch con cero; explicación y filas conservadas en auditoría. Los agregados por segmento no permiten recalcular independientemente los percentiles del histograma completo.

**Rendimiento H17: Fail por p95.** Sin nueva medición, ajuste de umbrales/defaults ni interpretación causal. Los JSON de P0-calib-1/Po-1 y selección guardada permanecen intactos.

## 4. Invariantes de First Ball (§17.A)

| Invariante | Estado y evidencia |
|---|---|
| 1. Una autoridad, cero atravesamientos, esquinas sin ganancia | Pass: aceptación nativa C1/C3 con cobertura cerrada, cero atravesamientos/ganancia; suites host/regresión 270/136 |
| 2. Consistencia en mismo binario/dispositivo | Pass: aceptación nativa C5/C8, cobertura cerrada |
| 3. Predicción bit a bit | Pass: aceptación nativa C6/C7, cobertura cerrada; C17 nativo p95 0,0188 ms≤1 ms |
| 10. Panel excluido de prod | Pass: exports limpios, prod instalada y arranque devpanel=absent; PO confirma etiqueta prod, sin DEV y tres dedos inertes |
| 14. Preset recuperado, métricas con preset/build/semilla | Pass técnico: regresión existente y JSONL I7a con preset/build/semilla; P0-calib-1/Po-1 intactos. Cargar Default eliminó last-active como corresponde |

## 5. Decisión del PO

**Pendiente PO, con incumplimiento H17 p95 explícito.** T7.1 físico/release pasa y las dos sesiones H17 completas superan p95≤16,7 ms. Elegir y comentar: continuar a First Shot / ajustar First Ball / volver. Aprobación de H1 y autorización de I7a no equivalen a esta decisión. No cerrar Etapa 7, modificar spec/defaults, ejecutar T7.3 ni iniciar gameplay sin autorización expresa.

- Decisión: pendiente.
- Observaciones/ajustes: pendiente.
- Fecha y declaración del PO: pendiente.
- Revisión de Claude de I7a: pendiente.
