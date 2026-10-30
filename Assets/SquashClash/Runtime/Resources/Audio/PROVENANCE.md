# Audio provisional de S2

Generado originalmente para este proyecto por Codex el 2026-10-30, sin samples ni medios externos. No se atribuye una licencia CC0 de terceros. Son recursos provisionales para distinguir contacto y swing al aire, no los niveles de S3 ni una medición de latencia iOS.

PCM mono16bit/22050Hz. Contacto45ms: (sin(2π950t)+0,25sin(2π1730t))·exp(−90t)·0,38. Swish80ms: ruido LCG de32bits, semilla11, a1664525/c1013904223, normalizado a[−1,1]·sin(πt/0,08)·0,18. Cada muestra se limita a[−1,1]y se convierte con int(v·32767), little endian. Carga de audio previa; una solicitud no prueba latencia física. En batchmode se comprueba la solicitud sin reproducir sonido.
