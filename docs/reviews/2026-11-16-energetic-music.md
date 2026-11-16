# Música más enérgica: informe de la entrega

Fecha: 16 nov 2026. Rama `audio/energetic-music` (PR en borrador). Autoría: subagente Claude Sonnet dirigido por Claude.

## Decisión del PO (literal)

«Lo del modelo Sonnet con respecto al a musica, Usa un sub agente para implementar la recomendación y la probamos»

La recomendación: partido = candidato B (pulso electrónico con arpegios, sin percusión), menú = candidato A (synthwave), y subir `MusicPlayer.MatchVolume` de 0,12 a 0,24, con la prueba de mezcla ajustada.

## Qué cambió

- `scripts/audio/make_music.py`: ahora escribe exactamente los dos bucles elegidos (`music-match.wav` = B, `music-menu.wav` = A). Solo biblioteca estándar, determinista, original (sin samples, MIDI ni medios de terceros). Se descartaron los candidatos no elegidos.
- `Assets/SquashClash/Runtime/Resources/Audio/music-match.wav` y `music-menu.wav` reemplazados; los `.meta` no se tocaron (mismos GUID y ajustes de importación).
- `MusicPlayer.MatchVolume` 0,12 → 0,24 y su comentario.
- `ZMusicTests.cs`: umbrales nuevos (primer commit, paso RED). La prueba de mezcla se renombra `Mix_TheMatchLoopStaysUnderThePlayersContact`.
- `PROVENANCE.md`: sección de música reescrita.

## Cambios de aserciones

| Aserción | Antes | Ahora |
|---|---|---|
| Pico del partido en juego vs. contacto más bajo | ≤ contacto·0,25 | ≤ contacto·0,5 |
| Valor eficaz del partido en juego | ≤ contacto·0,08 | ≤ contacto·0,15 |
| `MatchVolume` vs. `MenuVolume` | < `MenuVolume`·0,3 | < `MenuVolume`·0,5 (0,24 < 0,25) |
| Pico del menú vs. partido en juego | > partido·3 | > partido·2 |

La última no estaba en la propuesta: con el partido a 0,24, el menú (pico 0,310) queda solo 2,83 veces sobre el partido (0,110), así que «·3» fallaría. Se bajó a «·2», que conserva la intención (el menú suena claramente más fuerte). Si el PO prefiere otra cosa, es el único umbral que decidí yo.

## Números (réplica en Python de la mezcla de `ZMusicTests`, sobre los WAV del repo)

Contacto más bajo = `impact-valid`, pico 0,286 (`impact-perfect` 0,759). Longitudes 16,000 s y 19,200 s, mono.

- Pico del partido en juego 0,1097 ≤ 0,1430: PASA, margen 2,30 dB (8,3 dB bajo el contacto).
- Valor eficaz del partido en juego 0,0360 ≤ 0,0429: PASA, margen 1,52 dB (18,0 dB bajo el pico del contacto).
- Pico del menú en juego 0,310 ≤ 0,35: PASA; 2,83 veces el del partido, mayor que ·2: PASA (·3 fallaría).
- `MatchVolume` 0,24 < 0,25: PASA.

Nota sobre el texto de la propuesta («unos 12 dB bajo el contacto en valor eficaz»): el umbral 0,15 equivale a −16,5 dB respecto del pico del contacto, y el valor medido queda a −18 dB. Los comentarios del código dicen «unos 16 dB» para que coincidan con los números.

Hashes: `music-match.wav` `5d4214a4…ab00bc`, `music-menu.wav` `a67c2bf3…e1ec7`, idénticos a los candidatos B y A.

## Qué NO está verificado

- Las pruebas de Unity PlayMode no se ejecutaron (Unity está en uso por Codex): el paso RED (umbrales nuevos con 0,12 debería fallar en la aserción de volumen) y el GREEN no se observaron; solo existe la réplica en Python.
- La importación Vorbis: el re-codificado puede subir el pico y el margen más ajustado es el del valor eficaz, 1,5 dB.
- Build, instalación y escucha en el iPhone: nadie las ha oído en el dispositivo; la comparación de volumen, energía y convivencia con los golpes se juzga a oído.

## Independencia de la revisión

Claude dirigió este trabajo (subagente Sonnet), por lo que su revisión no es independiente. Falta la revisión de Codex o del PO y la corrida de `scripts/test.sh playmode ZMusicTests` en el Mac cuando Unity esté libre. No se fusiona sin aprobación del PO.
