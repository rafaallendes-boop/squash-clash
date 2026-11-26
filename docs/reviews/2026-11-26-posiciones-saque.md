# Posiciones de saque · DETENIDO

26 nov 2026. Encargo `set12-posiciones-saque`, rama `rafaallendes-boop/sc-posiciones-saque` desde `origin/main` `8a32d42`. [Spec](../superpowers/specs/2026-11-26-posiciones-saque.md). Implementación hecha por Claude (Opus), por lo que la revisión de Claude no es independiente. Sin PR, sin fusión y sin instalación.

## Motivo de la parada

Con el sacador en la esquina interior delantera de su cuadro (`BoxPosition` → (1,30; 5,79) y (5,10; 5,79)), el **saque drive deja de ser legal en los dos cuadros**. `ServePlanner.Typed` lanza: «The Drive serve from the Right box is not valid for this court and physics.» (igual en el izquierdo).

Diagnóstico de solo lectura ([contactos](../validation/evidence/2026-11-26-posiciones-saque/after-serve-contacts.tsv)): las miras fijas del drive (`ServeKindDrive*AimX` 2,8/3,6, de RallyParams) se calibraron desde el centro del cuadro. Desde la esquina, la pelota pega en el frontis donde se apunta, pero **bota en la mitad del propio sacador**, así que es falta de saque:

| Cuadro | Primer bote | Exigido |
|---|---|---|
| Derecho | x=2,94, z=8,60 | x > 3,20 |
| Izquierdo | x=3,60, z=8,18 | x < 3,20 |

El globo y el saque Auto siguen siendo legales desde la esquina. Para arreglarlo hay que cambiar las miras del drive, que es un cambio de RallyParams y de hash, o separar el origen de la pelota de la posición del sacador. Ninguna de las dos cosas está en el alcance ni autorizada. Según AGENTS §2, se registra y se detiene el trabajo, sin corregir ni reintentar.

## Lo que sí quedó hecho

- `ServePlanner.ReceiverPosition(box)`: (4,80; 7,65) para el cuadro derecho y (1,60; 7,65) para el izquierdo. Se aplica como posición real del cuerpo en `RallyDirector.BeginPoint` y `RallySim.PlayPoint`. «0,3 m de la media cancha» se interpretó como la línea interior del cuadro; el motivo está en la spec.
- **TDD** ([detalle](../validation/evidence/2026-11-26-posiciones-saque/tdd.tsv)): RED válido por aserción, 12 fallos (10 EditMode y 2 PlayMode). GREEN parcial con 14/16: las 2 que faltan son `CrossCourtServe_DoesNotGoThroughTheReceiver(Right|Left, Drive)` por la excepción de arriba. Auto y globo no atraviesan al restador.
- No se corrieron la suite completa, AutoQA ni la medición DESPUÉS. La simulación de partidos no puede correr porque los estilos del rival sacan drive y eso lanza la excepción. `ServeTests` también fallaría por la misma causa.

## Medición ANTES (base `a037bbb` = `main` + experimento, mismo comportamiento que `main`)

100 partidos por estilo, nivel Medio, semillas `20261126+100000·estilo`, preset y RallyParams por defecto. El humano es el novato de σ 90 ms y saca globo. [Tabla completa](../validation/evidence/2026-11-26-posiciones-saque/before-reception.tsv).

| Restador | Saques | Contact | Tasa |
|---|---|---|---|
| Humano (recibe al rival) | 1880 | 1722 | 91,60 % |
| Rival (recibe al humano) | 1380 | 1379 | 99,93 % |

Geometría prevista ([detalle](../validation/evidence/2026-11-26-posiciones-saque/before-geometry.tsv)): desde la esquina, globo y drive pasan a 0,89 m o más del restador, por fuera de la cápsula (0,30 m). El drive pasa lejos porque no cruza la cancha, y por eso es falta.

## Decisión que necesito de Rafael

1. Recalibrar las miras del drive para la esquina nueva (cambia RallyParams y su hash) y después medir antes y después.
2. Mantener la salida de la pelota en el centro del cuadro y mover solo el cuerpo del sacador. Es la opción más simple, pero la pelota saldría a unos 0,7 m del jugador.
3. Correr al sacador a otra posición dentro del cuadro desde la que el drive siga siendo legal.
