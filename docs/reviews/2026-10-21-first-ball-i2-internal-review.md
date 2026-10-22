# I2 — revisión interna independiente

Fecha: 2026-10-21. Una revisión de lectura del rango **6ad2961..1cbf00c**, bajo `executing-plans` y `requesting-code-review`. No es la revisión de Claude sobre la PR ni sustituye la aceptación del PO. El revisor no modificó archivos ni ejecutó Unity/tests/builds; el implementador controló esas ejecuciones.

## Resultado

**Sin Critical ni Important.** Implementación técnicamente preparada para presentar a Claude. Geometría y normal perpendicular D-6 alineadas con el plan, configuración inmutable y fuente única de defaults, guard JSON justificado por valores implícitos, recursos visuales liberados y escena sin colisionadores. Se inspeccionaron código, materiales, escena, captura y XML: EditMode 79/79, PlayMode 9/9 y ciclos RED reales; sin omitidos. Espacios finales en YAML serializado por Unity no se consideran un defecto funcional.

## Observación menor del plan — orientación horizontal

Referencias: plan r2, convención de coordenadas y T2.3; `Runtime/Camera/CourtCameraRig.cs`, pose fija y LookAt. La convención textual dice que +X va a la derecha desde atrás, pero la fórmula explícita de cámara orienta su derecha hacia −X en Unity. Resultado visible: el rival en x=4,5 aparece a la izquierda del humano en x=3,2.

El código respeta la pose explícita del plan. En I2 no hay interacción direccional ni consecuencias deportivas afectadas. **Se difiere la aclaración a Claude/PO y se expone en la PR.** No se invierten ejes ni se introduce una proyección reflejada unilateralmente. Coste de mantenerlo: la correspondencia textual izquierda/derecha debe resolverse antes de introducir controles o decisiones que dependan de ella; no se presenta como ya resuelta.

## Comportamientos apartados y resolución del implementador

| Elemento considerado por el revisor | Resolución y alcance |
|---|---|
| Pausa durante vuelo y reanudación en producción | T3.1/T3.3 no autorizadas; no hay vuelo en I2. Debe probarse en su incremento, no se atribuye cobertura ahora. |
| Último preset, escritura interrumpida y conservación del anterior | T5.1; aquí se revisan codec y overlays. Recuperación de archivos todavía no implementada ni certificada. |
| Cambios físicos durante el vuelo | T3.3/T5.1; sin reloj ni pelota, no añadir infraestructura anticipada. |
| Rodadura, reposo, relanzamiento, vibración y NaN de simulación | T4.1/T4.2; no existen simulaciones en I2. Riesgo pendiente del gate futuro. |
| Miembros Physics/Launch/Tempo de EffectiveConfig | El plan los incorpora desde T3.1/T3.3. Se aplazan los tipos y su consumo para evitar stubs de sistemas no autorizados. |
| Predictor, backend físico, reglas deportivas y panel | Fuera del permiso del PO; el clasificador actual es geométrico, sin consecuencias de punto. |
| NotEvaluated y verificación de producción con salidas limpias | Tareas posteriores; no atribuir sus garantías a esta entrega dev de cancha. |
| Arte, cámara de seguimiento y legibilidad de pelota | Incrementos posteriores. El encuadre fijo actual sí se prueba y lo acepta el PO. |
| Bandas degeneradas construidas manualmente | Ningún consumidor autorizado las genera; la geometría de Build está probada y el contrato exige crecimiento unitario perpendicular. Añadir validación si aparece entrada editable para esa geometría. |
| UI nativa, encuadre y juicio visual definitivo | Deben comprobarse en el iPhone por el implementador y el PO; la revisión estática no los acredita. |
| Resolver la orientación horizontal | Contradicción del plan registrada arriba, sin decisión unilateral. |

## Punto de control

Faltan revisión de Claude en la PR y aceptación del PO de la cancha en el iPhone. No fusionar hasta ambas. El [informe de I2](../validation/i2-court.md) registra por separado la compilación, firma, instalación y el arranque reales; su estado no se infiere de esta revisión.


## Resolución posterior de la observación menor — AX-1

El 2026-10-21, el PO transmitió la revisión de Claude sin bugs de implementación y autorizó conservar coordenadas/cámara y corregir nombres: LeftWall x=6,40, RightWall x=0, cajas intercambiadas y S3 «lateral derecha» sin reflexión. La observación queda atendida mediante esa aclaración explícita, registrada en las [aprobaciones del PO](2026-10-21-first-ball-po-approvals.md). Nuevas suites: EditMode 79/79 y PlayMode 11/11; la nueva prueba usa las mallas con SurfaceTag y la cámara del preset, y se verificó RED antes de corregir el producto. Detalle en [I2 / AX-1](../validation/i2-court.md#ax-1--convención-izquierdaderecha-tras-la-revisión-de-claude). Esta anotación no atribuye una nueva revisión independiente a los cambios posteriores al rango original de este informe.
