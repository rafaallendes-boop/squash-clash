# E1 — capturas de derecho y revés

Generadas en el Mac del PO con la escena `Main`, la cámara colocada por el juego y un rally Demo real (semilla 9, nivel Medium, σ de timing del autopiloto 0). Humano diestro y rival diestro fijo. La fixture avanza el juego y las vistas explícitamente; no impone poses ni trayectorias a los personajes. Desactiva el shake/punch de presentación para comparar desde la cámara fija. No es una sesión física.

Comando: `scripts/capture.sh e1-forehand-backhand docs/validation/evidence/2026-11-16 ZE1StrokeCapture`.

- `*-phone.png`: pantalla completa, 1206 × 2622 px, aproximadamente 260 ms después de la entrega.
- `*-contact-follow.png`: recortes de esa misma cámara sin ampliar, 644 × 450 px; contacto a la izquierda (primer fotograma a partir de 60 ms) y remate a la derecha (primer fotograma a partir de 260 ms). No representan el movimiento continuo.

| Figura | Derecho | Revés |
|---|---|---|
| Humano | [Contacto/remate](human-forehand-contact-follow.png) · [Pantalla](human-forehand-phone.png) | [Contacto/remate](human-backhand-contact-follow.png) · [Pantalla](human-backhand-phone.png) |
| Rival | [Contacto/remate](rival-forehand-contact-follow.png) · [Pantalla](rival-forehand-phone.png) | [Contacto/remate](rival-backhand-contact-follow.png) · [Pantalla](rival-backhand-phone.png) |

Fixture de captura: 1/1 Passed; separada de la regresión. Los cuatro casos de contacto (derecho/revés, diestro/zurdo) se verifican en `ZE1ContactWindowTests`, con 288 muestras y separación máxima de 0 m en el GREEN focalizado. Los XML y logs originales están en `Logs/evidence/e1-*`, ignorados por Git.
