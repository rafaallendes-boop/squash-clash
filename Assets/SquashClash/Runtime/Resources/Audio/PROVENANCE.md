# Audio provisional de S2

Generado originalmente para este proyecto por Codex el 2026-10-30, sin samples ni medios externos. No se atribuye una licencia CC0 de terceros. Son recursos provisionales para distinguir contacto y swing al aire, no los niveles de S3 ni una medición de latencia iOS.

PCM mono16bit/22050Hz. Contacto45ms: (sin(2π950t)+0,25sin(2π1730t))·exp(−90t)·0,38. Swish80ms: ruido LCG de32bits, semilla11, a1664525/c1013904223, normalizado a[−1,1]·sin(πt/0,08)·0,18. Cada muestra se limita a[−1,1]y se convierte con int(v·32767), little endian. Carga de audio previa; una solicitud no prueba latencia física. En batchmode se comprueba la solicitud sin reproducir sonido.

# Audio de S3 (Incremento B)

Generado originalmente para este proyecto por Claude el 2026-11-05, sin samples ni medios externos. No se atribuye una licencia CC0 de terceros. Todos son PCM mono 16 bit a 22050 Hz, con muestras limitadas a [−1,1] y convertidas con int(v·32767), little endian. Se precargan en `Initialize` con `LoadAudioData`. Una solicitud de reproducción no prueba latencia física; el modo silencio y la sesión de audio de iOS no se modifican y se verificarán en el iPhone.

- `impact-perfect.wav` (60 ms, impacto Perfect, seco, de ataque inmediato): 0,55·sin(2π·200t)·exp(−70t) + 0,35·sin(2π·1500t)·exp(−300t) + 0,3·n·exp(−900t), donde n es ruido LCG de 32 bits, semilla 21, a=1664525, c=1013904223, normalizado a [−1,1].
- `impact-valid.wav` (70 ms, impacto Valid, más suave, grave y con ataque lento): (0,4·sin(2π·520t)·exp(−55t) + 0,15·sin(2π·780t)·exp(−70t))·(1−exp(−t/0,004)). Sin ruido; timbre y ataque distintos de `impact-perfect.wav`, no solo más bajo de volumen.
- `swish-provisional.wav` (80 ms): se conserva el clip de S2 como swish del resultado Failed; fórmula en la sección de S2.
- `contact-provisional.wav`: clip de S2 conservado por compatibilidad (una prueba existente lo carga) y sin uso por el feedback de S3.

# Capa de sonido de la primera experiencia (AudioScape, 15 nov 2026)

Generado originalmente para este proyecto por Claude (Fable) el 2026-11-15, sin samples, grabaciones ni medios externos. No se atribuye una licencia CC0 de terceros. Los diez archivos `scape-*.wav` los escribe `scripts/audio/make_audioscape.py` (solo biblioteca estándar de Python 3) y son reproducibles byte a byte: el script imprime el SHA-256 de cada uno. PCM mono 16 bit a 22050 Hz, muestras limitadas a [−1,1] y convertidas con int(v·32767), little endian. Ruido: LCG de 32 bits (a=1664525, c=1013904223, valor x/2³¹−1) pasado por un paso bajo de un polo. Los clips de contacto del jugador (`impact-perfect`, `impact-valid`, `contact-provisional`, `swish-provisional`) no se modifican ni se usan aquí.

| Archivo | Dura | Qué es | Síntesis (t en s; n = ruido filtrado; a(t) = rampa de ataque) |
|---|---|---|---|
| `scape-bounce-floor.wav` | 80 ms | Bote en el piso | a·(0,50·sin(φ 150→95 Hz)·e^(−38t) + 0,54·n(semilla 31, 1,8 kHz)·e^(−140t)) |
| `scape-bounce-wall.wav` | 70 ms | Paredes laterales | a·(0,42·sin(2π310t)·e^(−60t) + 0,16·sin(2π640t)·e^(−90t) + 0,28·n(32, 3 kHz)·e^(−200t)) |
| `scape-front-wall.wav` | 130 ms | Golpe contra el frontis | a·(0,55·sin(φ 120→80 Hz)·e^(−26t) + 0,22·sin(2π370t)·e^(−55t) + 0,50·n(33, 2,4 kHz)·e^(−110t)) |
| `scape-rival-step.wav` | 90 ms | Paso del rival | a·(0,35·sin(φ 110→70 Hz)·e^(−55t) + 0,30·n(34, 1,2 kHz)·e^(−90t)) + chirrido 0,10·sin(φ 1,9→2,5 kHz)·sin(πt/0,05) |
| `scape-rival-racket.wav` | 70 ms | Raqueta del rival | (1−e^(−t/0,002))·(0,38·sin(2π270t)·e^(−45t) + 0,14·sin(2π610t)·e^(−90t)), sin ruido |
| `scape-point-won.wav` | 420 ms | Punto ganado | dos notas que suben, mi5 (659,25 Hz) y si5 (987,77 Hz) a los 140 ms; seno + 0,25 de su octava, e^(−9t), nivel 0,30 |
| `scape-point-lost.wav` | 420 ms | Punto perdido | dos notas que bajan, sol4 (392 Hz) y re4 (293,66 Hz); e^(−8t), nivel 0,26 |
| `scape-bounce-glass.wav` | 45 ms | Vidrio de fondo (18 nov): «toc» seco de vidrio templado, más agudo y brillante que la pared lateral | a·(0,40·sin(2π1250t)·e^(−110t) + 0,24·sin(2π2750t)·e^(−170t) + 0,10·sin(2π4100t)·e^(−260t) + 0,36·n(37, 6,5 kHz)·e^(−300t)); pico 0,689, volumen de fuente 0,19 (pico en juego 0,131 ≤ 0,143); SHA-256 `6bf501d867bf64483534ff95e49edc6353f6204694b36d6d4e3a34b424314f77`. Se apaga con un interruptor solo DEV. |
| `scape-perfect-own.wav` | 40 ms | Golpe Perfecto propio, más seco y limpio que `impact-valid` (sin cuerpo grave, caída corta); lo pide `ShotFeedback` en lugar de `impact-perfect` mientras el interruptor DEV esté en sí | 0,405·ataque(0,5 ms)·cierre(4 ms)·(0,55·sin(2π1100t)·e^(−50t) + 0,25·sin(2π2300t)·e^(−80t) + 0,20·ruido(41, paso bajo 7 kHz)·2·e^(−600t)); pico 0,286 y RMS igualados a `impact-valid` |
| `scape-ui-click.wav` | 30 ms | Clic de botones | 0,30·sin(2π1150t)·e^(−170t) + 0,08·ruido(35)·e^(−600t) |
| `scape-ambience.wav` | 4 s en bucle | Ambiente de cancha | ruido (semilla 36) con dos pasos bajos de 220 Hz, normalizado a 0,26, más 0,012·sin(2π100t) + 0,006·sin(2π200t); la cola se funde con el comienzo para que el bucle no tenga costura |

Mezcla (volumen de la fuente en `AudioScape`, medido contra el clip de contacto más bajo, `impact-valid`, pico 0,29 a volumen 1): frontis 0,20, piso 0,18, paredes 0,21, raqueta del rival 0,33, paso 0,16, punto ganado 0,32, perdido 0,30, clic 0,30, ambiente 0,14. En su punto más fuerte ningún sonido de cancha supera la mitad del pico de `impact-valid` (−6 dB) y el ambiente queda bajo el 15 %; lo comprueba la prueba `ZAudioScapeTests.Mix_EveryCourtSoundPeaksUnderHalfThePlayersQuietestContact`. Cada repetición cambia el tono ±5 % y el volumen va del 60 % al 100 % del valor de la tabla según la fuerza del contacto. En batch mode se comprueba la solicitud sin reproducir sonido. La mezcla real, el interruptor de silencio y la sesión de audio de iOS se juzgan en el iPhone.

# Música (MusicPlayer, 16 nov 2026, versión enérgica)

Compuesta y generada originalmente para este proyecto el 2026-11-16 por un subagente Claude Sonnet dirigido por Claude, sin samples, grabaciones, MIDI, bibliotecas de sonido ni medios externos. No se atribuye una licencia CC0 de terceros. Esta versión reemplaza la primera, de Claude (Fable), que el PO encontró poco enérgica; el PO eligió la recomendación el 2026-11-16 (partido: candidato B; menú: candidato A; volumen del partido 0,24). Los dos archivos `music-*.wav` los escribe `scripts/audio/make_music.py` (solo biblioteca estándar de Python 3) y son reproducibles byte a byte: el script imprime el SHA-256 de cada uno. PCM mono 16 bit a 22050 Hz, muestras limitadas a [−1,1] y convertidas con int(v·32767), little endian. Notas en temperamento igual (la4 = 440 Hz), hechas con sumas de senos y sus armónicos (voces tipo diente de sierra aditivas, con copias desafinadas y vibrato); ruido: el mismo LCG de 32 bits (a=1664525, c=1013904223). Cada nota se suma en un búfer circular y toda ganancia periódica (bombeo, barrido de filtro) es periódica sobre el bucle, así el bucle no tiene costura. Una saturación tanh suave empaqueta el nivel antes de normalizar. Unity los importa como Vorbis (calidad 0,7) y los descomprime al cargar; los `.meta` no cambian.

| Archivo | Dura | SHA-256 | Qué es |
|---|---|---|---|
| `music-menu.wav` | 19,2 s (10 compases de 4/4 a 125 ppm) | `a67c2bf39ad389a7683dc621639d0c97d8269c01864616de87ac5475b81e1ec7` | Menú y tutorial, «synthwave». Si menor: Bm – Sol – Re – La, dos vueltas, y Sol – La. Bombo grave cuatro por piso, palmada de ruido filtrado en 2 y 4, platillo en los contratiempos, bajo rodante en semicorcheas, arpegio en semicorcheas, colchón con bombeo y melodía desde el compás 5. Pico 0,620, valor eficaz 0,229. |
| `music-match.wav` | 16,0 s (10 compases de 4/4 a 150 ppm) | `5d4214a48673e1f694e8cce223162ad3e9b3f836407fd3cfa2bc8f2b23ab00bc` | Partido, pulso electrónico con arpegios. Mi menor: Em – Do – Sol – Re, dos vueltas, y Do – Re. Bajo rodante en semicorcheas, arpegio en semicorcheas con barrido de filtro periódico, gancho de campana con eco y colchón con bombeo en corcheas. Sin percusión alguna: el pulso lo dan el bajo y el bombeo. Pico 0,457, valor eficaz 0,150. |

Mezcla (volumen de la fuente en `MusicPlayer`): menú 0,50 (pico en juego 0,310) y partido 0,24 (pico en juego 0,110, valor eficaz 0,036). Contra el clip de contacto más bajo (`impact-valid`, pico 0,286): el pico del bucle del partido queda en 0,110 ≤ 0,143 (la mitad del contacto; margen 2,3 dB, 8,3 dB bajo el contacto) y su valor eficaz en 0,036 ≤ 0,043 (15 % del contacto; margen 1,5 dB, 18 dB bajo el pico del contacto). Réplica en Python de `ZMusicTests.Mix_TheMatchLoopStaysUnderThePlayersContact` sobre los WAV de origen, sin pasar por Vorbis: el re-codificado puede subir el pico unas décimas de dB y el margen de 1,5 dB del valor eficaz es el más ajustado. `MatchVolume` queda bajo la mitad de `MenuVolume` (0,24 < 0,25) y el pico del menú queda 2,8 veces sobre el del partido (la prueba exige más de 2; antes, más de 3). Las fuentes de la música ignoran el volumen del oyente (que es lo que mueve el interruptor «Sonido») y viven en un objeto hijo propio; `MusicPlayer` no se suscribe a ningún evento del juego ni toca `ShotFeedback` ni `AudioScape`. En batch mode se comprueba la solicitud sin reproducir sonido. **Nadie las ha escuchado todavía en el iPhone:** están verificadas solo con números (el generador, los hashes y la réplica de la mezcla); la prueba de Unity, la importación Vorbis, la composición, la mezcla real y la convivencia con música de otra app se juzgan en el iPhone. Sin percusión en el partido para que nada se confunda con un golpe o un bote; el menú sí lleva ruido filtrado, que no suena durante el partido.

# Resoplido del rival (19 nov 2026)

Generado originalmente para este proyecto por un subagente Sonnet dirigido por Claude el 2026-11-19, sin samples, grabaciones ni medios externos. No se atribuye una licencia CC0 de terceros. Los tres `scape-rival-breath-N.wav` los escribe `scripts/audio/make_audioscape.py` (mismo formato y ruido LCG que los demás `scape-*`; los trece archivos anteriores quedan idénticos byte a byte). Fórmula: ruido de semilla s con paso bajo en `high` menos el mismo ruido con paso bajo en `low` (banda), por (min(1, t/rise))²·e^(−fall·t)·(1 + 0,25·sin(2π·flutter·t))·cierre de 30 ms, normalizado a pico 0,60.

| Archivo | Dura | Semilla | Banda | rise | fall | aleteo | SHA-256 |
|---|---|---|---|---|---|---|---|
| `scape-rival-breath-1.wav` | 520 ms | 51 | 400–2600 Hz | 0,10 s | 4,5 | 17 Hz | `fdfe25bceb597503245b57ffcd32f38fd8f86a040e348506a7eb42dff3ecef94` |
| `scape-rival-breath-2.wav` | 340 ms | 52 | 650–3400 Hz | 0,05 s | 8,0 | 23 Hz | `7d4e7542e35f35320460495c8437452186e8756a397e0903af84d772079e138a` |
| `scape-rival-breath-3.wav` | 440 ms | 53 | 300–2000 Hz | 0,14 s | 5,5 | 13 Hz | `2b152ce5473f181b0b878bc48d99b934a1f2be90bb8c28a837889913ec6e1079` |

Mezcla: volumen de fuente 0,10 (pico en juego 0,060), muy por debajo del pico de `impact-valid` (0,29) y de la raqueta del rival (0,126); la música de partido suena a 0,24. Lo comprueba `ZRivalBreathTests`.
