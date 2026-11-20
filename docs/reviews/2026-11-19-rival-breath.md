# Resoplido del rival cansado (19 nov 2026)

Rama `claude/rival-breath` (base `f068cee`, integración de #84). **Hecho por un subagente Sonnet dirigido por Claude; la revisión de Claude no es independiente.** Sin fusionar ni instalar. Solo audio, capa de vista.

## Comportamiento

- Lee la misma energía y el mismo tono que la barra del HUD (`FatigueSettings.Tone(RivalEnergy.Energy)`): 0 neutro (≥ onset 70), 1 ámbar, 2 rojo (< 25). Nunca lee la energía del humano.
- **Tras el punto:** con tono ≥ 1, un resoplido 0,55 s después de terminar el punto (en la pausa, sin pisar el jingle).
- **Zona roja (< 25):** además, 0,3 s después de un golpe propio del rival, como máximo uno cada 3 s (cuenta desde cualquier resoplido).
- **Ventana de swing del humano:** se descarta (no se difiere) si el gesto se está dibujando (`GesturePreview.Active`), el swing fue aceptado (`SwingView.Phase != Idle`) o se espera el contacto (`ShotController.WaitingForContact`). Solo se lee; esas ventanas no cambian.
- Cansancio apagado, interruptor DEV apagado o «Sonido» apagado: no se pide ningún sonido. Con el juego en pausa no suena y se reproduce al reanudar.
- Botón DEV «Resoplido del rival: sí/no» (`audio-breath-toggle`, en «Audio de prueba»), por defecto sí; en «no» queda exactamente como antes. Estático, sin persistir, sin clave de `DefaultPreset`.

## Clips

Tres `scape-rival-breath-N.wav` (PCM 16 bit, 22 050 Hz) generados por `scripts/audio/make_audioscape.py`: ruido filtrado en banda, subida suave, caída exponencial y aleteo lento. 520 / 340 / 440 ms, pico 0,60 normalizado, volumen de fuente 0,10, variación ±20 % de nivel y ±4 % de tono; nunca la misma variante dos veces seguidas. Fórmulas y SHA-256 en `PROVENANCE.md`. Los trece WAV anteriores quedan idénticos byte a byte.

| Medida | Valor |
|---|---|
| Pico en juego (pico del clip × volumen) | 0,060 |
| `impact-valid` (contacto más bajo del jugador) | 0,29; el resoplido queda a ≈ 21 % (límite del test: 25 %) |
| Raqueta del rival en su punto más fuerte | 0,126; el resoplido queda por debajo |
| Música de partido | volumen de fuente 0,24; el resoplido es un sonido corto de fondo, bajo el tope de mezcla existente (la prueba de mezcla recorre el enum nuevo) |

## TDD y compuertas

RED por aserción (`ZRivalBreathTests`, 12 pruebas con la lógica en stub: 10 fallan por aserción, 2 controles ya pasaban: clips y cansancio apagado). Cubre: bajo/sobre el onset, ámbar sin resoplido tras golpe, zona roja y enfriamiento de 3 s, supresión en el swing, nunca para el humano, cansancio apagado, interruptor DEV y «Sonido», pausa, botón DEV, tres clips distintos y límites de volumen, variantes sin repetición, y un partido real con rival cansado y autopiloto (ningún resoplido pedido durante el swing). GREEN 12/12.

Suites completas finales (sin filtro): **EditMode 870/870, PlayMode 677/677**. AutoQA y check-release no se ejecutaron: no se toca timing, gesto ni contrato. Sin cambios en `DefaultPreset.json`, RallyParams, Core, física, `Fatigue.cs`, timing, reloj, GesturePreview, ProjectSettings ni Packages; hashes intactos.

## Riesgos

- El timbre y el nivel reales solo se juzgan en el iPhone (en batch mode el sonido es solo una solicitud).
- Si se percibe que el resoplido choca con el jingle o el «toc» del vidrio, ajustar `BreathPointDelay` (0,55 s).
- La energía que se lee tras el punto ya incluye la recuperación entre puntos, igual que la barra.
- La supresión descarta el resoplido en lugar de diferirlo: en intercambios muy seguidos puede haber menos de los esperados.
