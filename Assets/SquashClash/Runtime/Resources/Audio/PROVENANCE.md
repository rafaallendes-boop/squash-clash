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

Generado originalmente para este proyecto por Claude (Fable) el 2026-11-15, sin samples, grabaciones ni medios externos. No se atribuye una licencia CC0 de terceros. Los nueve archivos `scape-*.wav` los escribe `scripts/audio/make_audioscape.py` (solo biblioteca estándar de Python 3) y son reproducibles byte a byte: el script imprime el SHA-256 de cada uno. PCM mono 16 bit a 22050 Hz, muestras limitadas a [−1,1] y convertidas con int(v·32767), little endian. Ruido: LCG de 32 bits (a=1664525, c=1013904223, valor x/2³¹−1) pasado por un paso bajo de un polo. Los clips de contacto del jugador (`impact-perfect`, `impact-valid`, `contact-provisional`, `swish-provisional`) no se modifican ni se usan aquí.

| Archivo | Dura | Qué es | Síntesis (t en s; n = ruido filtrado; a(t) = rampa de ataque) |
|---|---|---|---|
| `scape-bounce-floor.wav` | 80 ms | Bote en el piso | a·(0,50·sin(φ 150→95 Hz)·e^(−38t) + 0,54·n(semilla 31, 1,8 kHz)·e^(−140t)) |
| `scape-bounce-wall.wav` | 70 ms | Paredes laterales y trasera | a·(0,42·sin(2π310t)·e^(−60t) + 0,16·sin(2π640t)·e^(−90t) + 0,28·n(32, 3 kHz)·e^(−200t)) |
| `scape-front-wall.wav` | 130 ms | Golpe contra el frontis | a·(0,55·sin(φ 120→80 Hz)·e^(−26t) + 0,22·sin(2π370t)·e^(−55t) + 0,50·n(33, 2,4 kHz)·e^(−110t)) |
| `scape-rival-step.wav` | 90 ms | Paso del rival | a·(0,35·sin(φ 110→70 Hz)·e^(−55t) + 0,30·n(34, 1,2 kHz)·e^(−90t)) + chirrido 0,10·sin(φ 1,9→2,5 kHz)·sin(πt/0,05) |
| `scape-rival-racket.wav` | 70 ms | Raqueta del rival | (1−e^(−t/0,002))·(0,38·sin(2π270t)·e^(−45t) + 0,14·sin(2π610t)·e^(−90t)), sin ruido |
| `scape-point-won.wav` | 420 ms | Punto ganado | dos notas que suben, mi5 (659,25 Hz) y si5 (987,77 Hz) a los 140 ms; seno + 0,25 de su octava, e^(−9t), nivel 0,30 |
| `scape-point-lost.wav` | 420 ms | Punto perdido | dos notas que bajan, sol4 (392 Hz) y re4 (293,66 Hz); e^(−8t), nivel 0,26 |
| `scape-ui-click.wav` | 30 ms | Clic de botones | 0,30·sin(2π1150t)·e^(−170t) + 0,08·ruido(35)·e^(−600t) |
| `scape-ambience.wav` | 4 s en bucle | Ambiente de cancha | ruido (semilla 36) con dos pasos bajos de 220 Hz, normalizado a 0,26, más 0,012·sin(2π100t) + 0,006·sin(2π200t); la cola se funde con el comienzo para que el bucle no tenga costura |

Mezcla (volumen de la fuente en `AudioScape`, medido contra el clip de contacto más bajo, `impact-valid`, pico 0,29 a volumen 1): frontis 0,20, piso 0,18, paredes 0,21, raqueta del rival 0,33, paso 0,16, punto ganado 0,32, perdido 0,30, clic 0,30, ambiente 0,14. En su punto más fuerte ningún sonido de cancha supera la mitad del pico de `impact-valid` (−6 dB) y el ambiente queda bajo el 15 %; lo comprueba la prueba `ZAudioScapeTests.Mix_EveryCourtSoundPeaksUnderHalfThePlayersQuietestContact`. Cada repetición cambia el tono ±5 % y el volumen va del 60 % al 100 % del valor de la tabla según la fuerza del contacto. En batch mode se comprueba la solicitud sin reproducir sonido. La mezcla real, el interruptor de silencio y la sesión de audio de iOS se juzgan en el iPhone.

# Música (MusicPlayer, 16 nov 2026)

Compuesta y generada originalmente para este proyecto por Claude (Fable) el 2026-11-16, sin samples, grabaciones, MIDI, bibliotecas de sonido ni medios externos. No se atribuye una licencia CC0 de terceros. Los dos archivos `music-*.wav` los escribe `scripts/audio/make_music.py` (solo biblioteca estándar de Python 3) y son reproducibles byte a byte: el script imprime el SHA-256 de cada uno. PCM mono 16 bit a 22050 Hz, muestras limitadas a [−1,1] y convertidas con int(v·32767), little endian. Notas en temperamento igual (la4 = 440 Hz), hechas con sumas de senos y sus armónicos; ruido: el mismo LCG de 32 bits (a=1664525, c=1013904223). Cada nota se suma en un búfer circular, así la cola de las últimas cae sobre el comienzo y el bucle no tiene costura. Unity los importa como Vorbis (calidad 0,7) y los descomprime al cargar.

| Archivo | Dura | SHA-256 | Qué es |
|---|---|---|---|
| `music-menu.wav` | 19,2 s (8 compases de 4/4 a 100 ppm) | `73acf9bffaee8590331f81235a5e0170c1b3435f5197fee87a9edde5de11b4b8` | Menú y tutorial. La menor – Fa – Do – Sol, dos vueltas. Colchón de acordes (seno + armónicos 2 y 3, entrada de 0,25 s), bajo en los tiempos 1 y 3 con una corchea de entrada (armónicos 2 y 3 fuertes para que se oiga en el parlante del teléfono), arpegio en corcheas una octava arriba (caída e^(−6t)), melodía de cuatro compases solo en la segunda vuelta, y percusión suave: bombo (seno de 105 a 48 Hz, e^(−17t)) en 1 y 3, escobilla de ruido filtrado en 2 y 4, platillo cerrado en los contratiempos. Normalizado a pico 0,60. |
| `music-match.wav` | 16 s (8 compases de 4/4 a 120 ppm) | `1a9a0aaa5d90d1d32891ba0ec9e44a703a82438b474b427a7c5b27cd937e90ef` | Partido. Sin percusión ni ataques bruscos, para que nada se confunda con un golpe o un bote: quintas sostenidas (raíz, quinta y octava: la, la, fa, fa, do, do, sol, sol) que laten en corcheas 3+3+2 sin llegar a cero, con entrada y salida de 0,35 s, y una nota de campana lenta cada dos compases (entrada de 50 ms, e^(−1,6t)). Normalizado a pico 0,50. |

Mezcla (volumen de la fuente en `MusicPlayer`): menú 0,50 (pico 0,30) y partido 0,12 (pico 0,06, valor eficaz 0,018). El bucle del partido queda al menos 12 dB bajo el pico del clip de contacto más bajo (`impact-valid`, 0,29) y su nivel medio unos 22 dB bajo ese pico; lo comprueba `ZMusicTests.Mix_TheMatchLoopStaysFarUnderThePlayersContact`. Las fuentes de la música ignoran el volumen del oyente (que es lo que mueve el interruptor «Sonido») y viven en un objeto hijo propio; `MusicPlayer` no se suscribe a ningún evento del juego ni toca `ShotFeedback` ni `AudioScape`. En batch mode se comprueba la solicitud sin reproducir sonido. **Nadie la ha escuchado todavía:** está verificada solo con números; la composición, la mezcla real y la convivencia con música de otra app se juzgan en el iPhone.
