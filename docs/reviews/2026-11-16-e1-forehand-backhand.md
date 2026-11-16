# E1 — derecho y revés estéticos (16 nov 2026)

**Estado: INCOMPLETO tras revisión de Claude sobre `ab0f08e`. No listo para revisión ni fusión.** PR #68, rama `codex/e1-forehand-backhand`; tests nuevos en `3a1607f`, corrección parcial en `b9dad57`. Sin fusión ni instalación. Unity libre.

## Método y alcance

Se conserva la preparación propia mientras el humano apunta o se planta. Desde `Intention`, la cabeza sigue la referencia de `SwingView`, porque la entrega puede interrumpir esa fase antes de sus 150 ms; no se presupone una preparación completa antes del contacto. El torso sigue diferenciando derecho/revés y el remate propio comienza después de la ventana aprobada. La ventana sigue siendo ±60 ms respecto de la clave `Contact` de `SwingView`, con 5 mm de tolerancia: no cambia.

Continuidad propuesta: en la entrada, el desplazamiento vectorial de la cabeza no añade más de 5 mm al de la referencia; en la salida, el desplazamiento por fotograma no supera `8 m/s × dt + 5 mm`. El límite escala con 30/60/120 Hz y 50 ms; no es un límite físico del juego. Se mide también la transición con tiempo cero. Los tests cubren preparación de 25 y 300 ms, ambos golpes y ambas manos. La población comienza después del primer dibujo de la orden aceptada: **no acredita la transición anterior de Idle a Intention**.

Además: el rival usa el `side` del evento (Drive → −1), igual que su arco; edad de contacto ausente cierra el bloqueo; escala humana constante; la ventana solo se arma con el intento aceptado y una entrega que `SwingView` haya procesado. Como su listener corre primero, se comprueba identidad pendiente y su estado posterior (`Impact`), consumiendo una sola vez la intención local.

Solo cambian `CharacterView` y tests E1 respecto de `ab0f08e`. `StrokeFx` ya usa el lado entregado. `SwingView`, Core/`BodyFrame`, planificación, gesto, reloj, timing, física, IA y defaults permanecen intactos. **AutoQA sigue exento** dentro de este alcance.

## Resultados y detención

- **RED válido antes de corregir:** `ZE1ContactWindowTests`, **17 total: 8 Passed / 9 Failed**, por las nueve aserciones previstas; sin errores CS. Evidencia ignorada en `Logs/evidence/e1-review-red`.
- **Primer GREEN focalizado:** `ZE1`, **20 total: 18 Passed / 2 Failed**, 0 omitidos. Fallan los derechos humanos diestro y zurdo en la salida: **284,35 mm** en un fotograma de 33,33 ms, frente a **271,67 mm** permitidos. Los cuatro tests de coincidencia, ambos reveses de continuidad, el lado del rival, el filtro, la escala y el cierre de ventana pasan. Los tests fallidos interrumpen su población en la primera infracción: no se acredita su cobertura restante. Evidencia en `Logs/evidence/e1-review-first-green`.
- Se restauró `ProjectSettings` después de ambas corridas. **Detención conforme a AGENTS §2:** no se corrige otra vez, no se cambia el umbral y no se reintenta para obtener Pass.
- **Pendientes para este código:** GREEN focalizado, EditMode/PlayMode completos, `check-release` DEV/PROD, regenerar capturas y nueva revisión de Claude por medio del PO. Los verdes y las [capturas anteriores](../validation/evidence/2026-11-16/e1-forehand-backhand/README.md) corresponden a `ab0f08e`; no validan esta corrección.

**Propuesta para reanudar, sin ejecutar:** conservar el umbral y acotar el desplazamiento del remate humano fuera de la ventana a 8 m/s. La posición durante el contacto seguiría copiada exactamente de `SwingView`. Después, ejecutar las compuertas completas; requiere que el PO levante esta detención.
