# Tarjeta de resultado extendida y golpe Perfecto propio (19 nov 2026)

Rama `claude/match-card-perfect-sound` (desde `7e8c64c`). **Implementado por un subagente Sonnet dirigido por Claude; la revisión de Claude no es independiente.** Sin fusionar ni instalar.

## Qué cambió

**Tarjeta de resultado (solo UI, sin tocar Core).** `RallyHud` agrega dos líneas: «Duración: m:ss» y «Rally más largo: N golpes». La duración es tiempo real sin escalar desde el primer saque hasta el punto de partido; no cuenta con el juego en pausa ni después de terminar. El rally más largo es el mayor `PointResult.Shots` visto al cerrarse cada punto. Ambos se reinician con la revancha o al salir. El botón dice «Revancha 5–7» (puntos del jugador, luego del rival, guion largo) solo si el jugador perdió; si ganó, «Revancha». `RallyText.cs` no cambia.

**Golpe Perfecto propio (solo audio).** Clip nuevo `scape-perfect-own.wav` (40 ms), generado por `make_audioscape.py` (SHA-256 `20518478…263c`): más seco y limpio que `impact-valid` (sin cuerpo grave, caída corta, ataque nítido), con pico 0,2862 y RMS 0,0804 frente a 0,2860 y 0,0804 del Valid. `ShotFeedback` lo pide en lugar de `impact-perfect` para el Perfecto del jugador; Valid, Failed y el rival no cambian. No se tocan ventanas ni timing: solo se elige otro clip en el mismo punto de entrega.

## Interruptores DEV (solo con SQUASH_DEV, por defecto sí, sin persistir ni clave de DefaultPreset)

- «Tarjeta extendida: sí/no» (`match-card-toggle`, sección «Tarjeta de resultado»). No: la tarjeta de antes, sin las dos líneas y con «Revancha» simple.
- «Golpe Perfecto propio: sí/no» (`audio-perfect-toggle`, en `AudioDevSection`). No: suena `impact-perfect` como hoy.

## Evidencia

- RED por aserción antes de implementar (con esbozos solo para compilar): 9 fallas de 24 en `ZMatchCardTests` y `ZAudioScapeTests` (clip ausente, etiquetas ausentes, formatos vacíos, botones DEV ausentes). GREEN: `ZMatchCardTests` 6/6.
- Suites completas finales: **EditMode 860/860, PlayMode 649/649**.
- Una prueba existente (`ZFirstRunMatchFlowTests.MatchOver_OffersRematchAndBackToTheMenu`) exigía «Revancha» exacto tras una derrota 3–7; ahora espera la regla nueva (derrota: con marcador; victoria: simple). Es el cambio pedido, no un umbral.
- Sin cambios en Core, `DefaultPreset.json`, física, timing, contrato, reloj, GesturePreview, ProjectSettings ni Packages. AutoQA no se ejecutó (no se toca timing, gesto ni contrato).

## Riesgos y dudas

- **Volumen del Perfecto:** el pedido es «el mismo volumen» que el Valid; el `impact-perfect` actual tiene pico 0,76 y RMS 0,14, así que el nuevo Perfecto suena claramente más bajo que el de hoy (igual que el Valid). Si el PO quería conservar el volumen actual, hay que reescalar el clip (una constante).
- Se implementó en `ShotFeedback` (Runtime/Feedback) y no en `AudioScape`, porque ahí vive el audio de contacto del jugador y `AudioScape` no se suscribe a esa entrega; solo se agregó el interruptor estático en `AudioScape`.
- El timbre se juzga en el iPhone; aquí está verificado solo con números. La duración usa pasos de HUD acotados a 50 ms, así que con tirones largos puede quedar unos segundos corta.
