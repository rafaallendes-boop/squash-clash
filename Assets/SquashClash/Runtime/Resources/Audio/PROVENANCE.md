# Audio provisional de S2

Generado originalmente para este proyecto por Codex el 2026-10-30, sin samples ni medios externos. No se atribuye una licencia CC0 de terceros. Son recursos provisionales para distinguir contacto y swing al aire, no los niveles de S3 ni una medición de latencia iOS.

PCM mono16bit/22050Hz. Contacto45ms: (sin(2π950t)+0,25sin(2π1730t))·exp(−90t)·0,38. Swish80ms: ruido LCG de32bits, semilla11, a1664525/c1013904223, normalizado a[−1,1]·sin(πt/0,08)·0,18. Cada muestra se limita a[−1,1]y se convierte con int(v·32767), little endian. Carga de audio previa; una solicitud no prueba latencia física. En batchmode se comprueba la solicitud sin reproducir sonido.

# Audio de S3 (Incremento B)

Generado originalmente para este proyecto por Claude el 2026-11-05, sin samples ni medios externos. No se atribuye una licencia CC0 de terceros. Todos son PCM mono 16 bit a 22050 Hz, con muestras limitadas a [−1,1] y convertidas con int(v·32767), little endian. Se precargan en `Initialize` con `LoadAudioData`. Una solicitud de reproducción no prueba latencia física; el modo silencio y la sesión de audio de iOS no se modifican y se verificarán en el iPhone.

- `impact-perfect.wav` (60 ms, impacto Perfect, seco, de ataque inmediato): 0,55·sin(2π·200t)·exp(−70t) + 0,35·sin(2π·1500t)·exp(−300t) + 0,3·n·exp(−900t), donde n es ruido LCG de 32 bits, semilla 21, a=1664525, c=1013904223, normalizado a [−1,1].
- `impact-valid.wav` (70 ms, impacto Valid, más suave, grave y con ataque lento): (0,4·sin(2π·520t)·exp(−55t) + 0,15·sin(2π·780t)·exp(−70t))·(1−exp(−t/0,004)). Sin ruido; timbre y ataque distintos de `impact-perfect.wav`, no solo más bajo de volumen.
- `swish-provisional.wav` (80 ms): se conserva el clip de S2 como swish del resultado Failed; fórmula en la sección de S2.
- `contact-provisional.wav`: clip de S2 conservado por compatibilidad (una prueba existente lo carga) y sin uso por el feedback de S3.
