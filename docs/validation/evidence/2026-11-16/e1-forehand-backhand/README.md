# E1 — capturas y continuidad

Capturas de la escena `Main` en el Mac del PO, con la cámara del juego y un rally Demo real (semilla 9, nivel Medium, σ del autopiloto 0). Humano diestro y rival diestro fijo. La fixture avanza juego y vistas explícitamente, sin imponer poses. Desactiva shake/punch de presentación para comparar desde la cámara fija. No es una sesión física. La evidencia final se regenera sobre `main` `02f3c57` integrado en `a8c00fe`, con el default T3 aprobado en U1.

Comando: `scripts/capture.sh e1-forehand-backhand docs/validation/evidence/2026-11-16 ZE1StrokeCapture`.

- `*-phone.png`: pantalla completa, 1206 × 2622 px, primer fotograma desde 260 ms después de la entrega.
- `*-contact-follow.png`: recortes sin ampliar, 644 × 450 px; contacto a la izquierda (desde 60 ms) y remate a la derecha (desde 260 ms). No representan el movimiento continuo.

| Figura | Derecho | Revés |
|---|---|---|
| Humano | [Contacto/remate](human-forehand-contact-follow.png) · [Pantalla](human-forehand-phone.png) | [Contacto/remate](human-backhand-contact-follow.png) · [Pantalla](human-backhand-phone.png) |
| Rival | [Contacto/remate](rival-forehand-contact-follow.png) · [Pantalla](rival-forehand-phone.png) | [Contacto/remate](rival-backhand-contact-follow.png) · [Pantalla](rival-backhand-phone.png) |

## Máximos por caso

[RED completo previo a la corrección](continuity-red.tsv) y [GREEN de continuidad](continuity-green.tsv): 64 filas cada uno, con mano, golpe, cadencia de 30/60/120 Hz o 50 ms, preparación de 25/300 ms y potencia de 0,1/1. Cada fila se registra después de recorrer todos sus fotogramas; la aserción se evalúa al terminar todos los casos de cada mano/golpe.

Unidades SI. `entryStep` y `exitStep`: desplazamientos máximos por fotograma. `entryExtra`: máximo de la norma de la diferencia entre desplazamientos de la cabeza dibujada y la referencia; límite 0,005 m. `exitExcess`: máximo de `desplazamiento − 8 × dt`; límite 0,005 m, exactamente equivalente a `8 m/s × dt + 5 mm`. `exitSpeed`: máximo de desplazamiento/dt en los fotogramas con tiempo positivo. Se incluyen muestras de tiempo cero en las transiciones.

El RED, anterior a integrar U1 y con sus defaults V originales, recorrió 2.704 muestras hasta 500 ms después de la entrega: entrada extra máxima 0,9787053 m y salida máxima 10,4451075 m/s. El GREEN sobre la integración T3 conserva esa población y prolonga el recorrido hasta 620 ms para incluir el regreso a Idle: **3.120 muestras, entrada extra máxima 0 m y salida máxima 7,48577356 m/s**; `exitExcess` máximo 0 m. No se cambia ningún umbral entre RED y GREEN.

La coincidencia de contacto se mide aparte: **288 muestras, separación máxima 0 m**, ambos golpes y ambas manos, potencias 0,1/1 y las cuatro cadencias. Las muestras de continuidad y de contacto no se suman para fabricar un recuento de tests.

XML, logs y JSON originales en `Logs/evidence/e1-idle-red`, `e1-retimed` y `e1-main-integration*`, ignorados por Git. Las capturas pertenecen a la fixture separada `ZE1StrokeCapture`, sin contribuir a regresión o AutoQA.
