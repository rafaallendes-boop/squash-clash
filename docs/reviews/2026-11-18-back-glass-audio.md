# Sonido propio del vidrio de fondo y medición de los indicadores de impacto (18 nov 2026)

Rama `claude/back-glass-audio` (desde `main` `75b9ce8`). **Implementado por subagente Sonnet dirigido por Claude; la revisión de Claude no es independiente.** Sin fusionar ni instalar.

## Tarea 1: «toc» del vidrio de fondo

- `AudioScape` mapeaba `BackWall` a `BounceWall`. Ahora `SurfaceId.BackWall` pide `ScapeSound.BounceGlass` (nuevo, índice 8); laterales, piso y frontis no cambian. Escala por velocidad como los demás.
- Clip `scape-bounce-glass.wav` (45 ms, PCM 16 bit 22 050 Hz) generado por `scripts/audio/make_audioscape.py`, determinista: senos de 1250/2750/4100 Hz con caída rápida más ruido filtrado a 6,5 kHz. Es más corto y más agudo que la pared lateral (310/640 Hz). SHA-256 `6bf501d8…4f77`; los nueve WAV anteriores quedan idénticos byte a byte. Fórmula en `PROVENANCE.md`.
- Mezcla: volumen de fuente 0,19, pico del clip 0,689, pico en juego 0,131 frente al límite 0,143 (mitad de `impact-valid`). La prueba existente de mezcla recorre todo el enum, así que ya cubre el clip nuevo.
- Interruptor solo DEV: botón «Vidrio de fondo propio: sí/no» (`audio-glass-toggle`, sección «Audio de prueba», pestaña Parámetros, `AudioDevSection`), por defecto sí; en «no» el fondo vuelve a `BounceWall`. Estático, sin persistir, sin clave de `DefaultPreset` (hashes intactos).
- Tests (`ZAudioScapeTests`): el fondo pide el clip nuevo; laterales siguen en `BounceWall`; interruptor apagado devuelve `BounceWall`; el vidrio es más corto y más brillante que la pared (cruces por cero 1,5 veces mayores); botón DEV existe y alterna; mezcla. RED por aserción (3 fallos: fondo, interruptor y botón) y GREEN. Se ajustó una suma existente (`InAMatch…`) para incluir el vidrio.

## Tarea 2: ¿desalineación del indicador en golpes al vidrio?

«Triángulo» = el triángulo fijo del anillo de timing (`TimingRingView`, dibujado en `plan.PStar`); también se midió el disco de impacto en pared (`LegibilityAids`). Pruebas nuevas en `ZImpactMarkerAlignmentTests`, con umbral de aserción 2 cm.

| Medición | Muestras | Error máximo |
|---|---|---|
| Disco de impacto frente al punto del evento (+2 mm de levantamiento a propósito), fondo / frontis / izquierda / derecha, 3 alturas (0,8–2,6 m), 3 lados, ambos laterales | 27 / 27 / 39 / 39 | 0,000 cm |
| Punto de contacto frente al plano de su pared | las mismas | 0,000 cm |
| Orientación del disco frente a la normal | las mismas | 0,0000° |
| Pelota real frente a la predicción en 90 trayectorias de 150 pasos | 90 | 0,000 cm |
| Partido autopiloto: P* planeado frente a la pelota en T*, sin paredes / tras vidrio de fondo | 24 / 6 | 0,000 / 0,000 cm |
| Posición dibujada del triángulo frente a `plan.PStar` | 6.857 fotogramas | 0,000 cm |

Con paredes laterales no hubo recepciones en el partido de demostración (n=0); se cubren por la predicción idéntica bit a bit a la pelota (`PredictionMatchesBitwise`) en los lanzamientos guionados a ambos laterales. Una primera versión de la medición mostró errores de hasta 110 cm en recepciones «sin paredes»: eran golpes propios del autopiloto que cambian la pelota antes de T* (la predicción se reemplaza); se excluyen por diseño y no son un error del plan. **Conclusión: no hay desalineación medible; no se cambió nada.** Si el PO la percibe, es de cámara (el vidrio es casi transparente y la cámara mira a través de él) y no de posición; no es verificable con estos datos.

## Compuertas

EditMode 803/803; PlayMode 616/616 (611 anteriores + 5 nuevas); `scripts/check-release.sh` dev/prod OK (build 871). AutoQA no se ejecutó: no se toca timing, gesto ni contrato. Sin cambios en `DefaultPreset.json`, física, timing, ProjectSettings ni Packages (restaurados tras cada corrida). Suena en batch mode solo como solicitud; la mezcla real y el timbre se juzgan en el iPhone.
