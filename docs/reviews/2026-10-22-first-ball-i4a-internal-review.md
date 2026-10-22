# I4a — revisión interna independiente y correcciones

Fecha: 2026-10-22. Revisor fresco gpt-6-astra, high, solo lectura, rango `c396fe4..b5d0d9a`; no ejecutó Unity ni modificó Git. Es la revisión final única del flujo executing-plans, no sustituye a Claude. Contrastó T4.1–T4.2, spec §8.2–8.3, los cinco Review Focus, fuentes y XML. Fuerzas identificadas: separación pura de mediciones, preservación AX-1, construcción analítica S4/S6/S8d, penetración separada de atravesamiento, bitwise real y trazabilidad del informe.

## Hallazgos y pase único RED→GREEN

**0 Critical, 3 Important, 0 Minor.** El veredicto original fue «With fixes». No se atribuye una segunda aprobación al revisor: Codex corrigió en `980529d` y verificó las regresiones.

| ID | Hallazgo del revisor | Corrección y evidencia |
|---|---|---|
| I1 | S8 heredaba SecondFloorBounce: MaxSteps=360 no obligaba a ejecutar todos los pasos. | Un test del consumidor real TrajectoryRunner con simulación sintética de botes terminaba en 2 en vez de 360 (RED). S8 usa MaxSteps; GREEN 360. Los predictores/comparaciones de I4b podrán limitarse al segundo bote según su contrato, sin recortar la prueba de robustez. |
| I2 | Evaluated>0 admitía C10 con solo 1 de 9, una campaña S8 reducida y cobertura omitida. | Se añaden Expected y CoverageComplete. Cardinalidades fijas salen del catálogo y no pueden bajarse; elegibilidad variable debe declararse/cerrarse sobre la población completa. Tres RED reproducen 1/9, lote S8 reducido y cobertura abierta/no declarada; GREEN después. El informe muestra evaluados/no evaluables/esperados y cobertura abierta/cerrada. |
| I3 | C4 descartaba NaN/Infinity de velocidad/normal como si no hubiera vibración; normal cero tampoco medía velocidad normal. | Tres RED: NaN, Infinity y normal nula no lanzaban error. Ahora ArgumentException por datos necesarios inválidos; I4b deberá reportarlos como no evaluables. StepOnly con tiempo NaN permanece válido, pues C4 no necesita tiempo exacto. |

**RED del pase: 155 total, 7 fallos nuevos y 148 anteriores aprobados. GREEN: EditMode 155/155 y PlayMode 37/37**, cero fallos/omitidos. [XML, hashes y nombres](../validation/evidence/2026-10-22-i4a-tests.json). Sin cambios de escenarios, umbrales ni criterios físicos. No hubo experimento PhysX, build nativa, instalación ni selección de backend.

## Contrato de cobertura para el futuro runner

Coberturas fijas del catálogo: C1=10 053 (10 000 S8 +17 fijos +36 S8d), C4=4, C5=117, C6=10 017, C8=2, C10=9, C11=6, C17=200 mediciones (calentamiento excluido). C2/C3/C7 requieren Expected declarado y CoverageComplete al cerrar toda su población/elegibilidad; C2 no puede cubrir menos que la campaña base de robustez. Los casos prescritos sin medición utilizable se conservan como NotEvaluable, no desaparecen del total. Un ensayo reducido puede producir datos útiles, pero no AllGatingPass.

Esto amplía solo el DTO de resultado respecto al bloque abreviado del plan, para corregir falsos aprobados. No adelanta SpikeRun. Su implementación en I4b deberá demostrar que enumera todos los casos, cierra cobertura honestamente y transforma los errores de medición en resultados no evaluables. Los contadores/flags son un contrato de datos, no una prueba de que un productor arbitrario diga la verdad.

## Comportamientos que el revisor dejó fuera y resolución

| Comportamiento | Resolución y coste de dejarlo pendiente |
|---|---|
| Focus 1: pausa/segundo plano | I3 aceptado y sin cambios. No nueva certificación de ciclo de vida en I4a. |
| Focus 2: presets corruptos/escrituras | I2/I5, sin cambios de persistencia. Recuperación de archivos sigue para I5. |
| Focus 3: configuración en vuelo | I3/I5; no nuevo panel. No nueva validación de tuning en este incremento. |
| Focus 4: S10 real y S10b reutilizada | Ejecutar en I4b. Estabilidad y reutilización física aún sin certificar. |
| Focus 5: builds prod/artefactos nativos | No hubo builds nuevas; XML sí inspeccionados. Integración nativa del runner espera I4b. |
| S10b, C7 y agregación/reutilización C5/C8 | T4.3. Cobertura del DTO corregida ahora; el productor real aún debe probar calendarios y contadores. |
| Presupuesto/cancelación/recursos del runner | T4.3. Sin afirmación de rendimiento ni liberación de un runner todavía inexistente. |
| Resultados C1–C18 y límites PhysX | Experimentos de I4b. Tests sintéticos no certifican cumplimiento. |
| C12–C15 y tope de impactos P1 | Evidencia I4b y análisis de Claude/PO en I4c. No inventar umbrales. |
| Benchmark/dispositivo/firma/Xcode | T4.4; no se instala en I4a. Rendimiento nativo pendiente; renovar firma después del 28 de octubre antes de instalar. |
| Backend, variantes, presets, P2, I4c | Autorización posterior del PO. Ninguna elección/promoción implícita. |
| Panel, ayudas, escala, reglas e I5+ | Fuera del alcance. Legibilidad y pausa manual siguen pendientes según el PO. |
| Igualdad de otros campos del contacto | El contrato compara estado y superficie/paso de contactos. No se certifica igualdad de punto/normal/tiempo/Persistent. |
| Validación genérica de DTOs inválidos | EffectiveConfig validado sigue siendo el origen. No se crea otra capa general; C4 sí valida los datos que necesita medir. |
| Espacios finales en .meta de carpetas | Formato generado por Unity, sin efecto funcional; se conserva. El diff de rango puede avisar por estos metadatos. |
| Informe humano y PR final | Trabajo obligatorio de cierre realizado tras la revisión. Mantener cifras y alcance actuales. |

Minors de producto diferidos de esta revisión: **ninguno**. Gate I4c/P2/I5+ no autorizados. Claude debe revisar la PR I4a antes de su fusión y antes de comenzar I4b.
