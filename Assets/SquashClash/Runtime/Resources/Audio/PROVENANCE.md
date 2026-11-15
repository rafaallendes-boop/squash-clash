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
