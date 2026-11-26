# Posiciones de saque

26 nov 2026. Idea del PO, revisada por el Game Designer. Encargo `set12-posiciones-saque`. Una PR propia, rama nueva desde `origin/main` (`8a32d42`).

## Problema

Antes del saque, el sacador y el restador empiezan los dos en la T. En una cancha real nadie se coloca así.

## Comportamiento

Coordenadas de AX-1: `x` desde la pared lateral derecha (x=0) hasta la izquierda (x=6,40) y `z` desde el frontis. La línea corta tiene su borde trasero en z=5,49 y el cuadro mide 1,60 m.

| Quién | Dónde | Cuadro derecho | Cuadro izquierdo |
|---|---|---|---|
| Sacador | Adelante en su cuadro, lo más cerca de la esquina interior delantera que permiten las miras actuales: a 0,70 m de la línea interior del cuadro y 0,30 m detrás de la línea corta (`ServePlanner.BoxPosition`) | (0,90; 5,79) | (5,50; 5,79) |
| Restador | Cuarto trasero opuesto, a 1,6 m de la pared lateral y 2,1 m de la pared del fondo (`ServePlanner.ReceiverPosition`, nuevo) | (4,80; 7,65) | (1,60; 7,65) |

- **Por qué no está a 0,3 m de la esquina (camino 3 de SC-PM, 26 nov).** Desde (1,30; 5,79), el drive (miras fijas `ServeKindDrive*`) bota en la mitad del propio sacador, en x=2,94 con el cuadro derecho, y es falta. `ServePositionsSearch` recorrió en una rejilla de 5 cm las posiciones dentro del cuadro (el cuerpo a 0,30 m o más de las líneas del cuadro y a 0,35 m o más de la pared). Eligió la más cercana a la esquina en la que, en los dos cuadros, drive, globo y Auto son legales y botan al menos 0,3 m dentro de lo exigido (pasada la media cancha y detrás de la línea corta) sin atravesar al restador. Resultado: a 0,70 m de la línea interior y 0,30 m de la línea corta, a 0,40 m de la esquina pedida. Lo limita el drive del cuadro izquierdo, que bota 0,39 m dentro. RallyParams no cambia. [Rejilla](../../validation/evidence/2026-11-26-posiciones-saque/search-grid.tsv).
- **Lectura de «0,3 m de la línea de media cancha».** La línea de media cancha está en x=3,20, a 1,60 m del cuadro. Si el sacador quedara a 0,3 m de ella, estaría fuera del cuadro (pie en falta). Por eso se toma la línea del cuadro que da hacia la media cancha, coherente con «esquina interior delantera de su cuadro». La prueba existente `AutomaticServe_IsValid_AndLandsInTheReceiversQuarter` ya exige que el sacador esté dentro del cuadro.
- `BoxPosition` sigue siendo el punto de partida del saque: la pelota sale de ahí a `ServeHandHeight`, igual que hoy. Por eso cambian las trayectorias del saque. Las miras fijas del frontis (`ServeKind*`) y el saque automático (`Find`) no cambian, y cada saque se valida con las reglas del saque como hoy.
- `ReceiverPosition(box)` es la posición **real** del cuerpo simulado del restador al empezar el punto: `RallyDirector.BeginPoint` (juego) y `RallySim.PlayPoint` (simulación sin pantalla) teletransportan al restador ahí antes de planificar su recepción. No es solo un dibujo.
- **Después del saque, como hoy:** el sacador parte hacia su posición de espera en cuanto saca (sin retardo) y el restador va a su postura de recepción. Después de la devolución, los dos vuelven a la T por la recuperación de siempre.
- Las figuras que se dibujan antes del saque (el humano colocado antes en su cuadro y el ritual del rival) ya usan `BoxPosition`, así que muestran la esquina nueva sin otro cambio.

## Fuera de alcance

La ventana de 25 ms, el timing, los umbrales, los criterios y las pruebas existentes. No cambian `DefaultPreset.json`, los defaults, SimulationKey ni RallyParams (no hay parámetros nuevos: las distancias son constantes de geometría de la cancha en `ServePlanner`).

## Pruebas

- `BoxPosition` da la posición elegida en los dos cuadros y `ReceiverPosition` el cuarto trasero opuesto.
- Desde esa posición, los tres saques son legales y botan al menos 0,3 m dentro de lo exigido.
- `RallySim.PlayPoint`: al empezar la recepción del saque, el restador está en `ReceiverPosition` (humano y rival).
- **El saque cruzado no atraviesa al restador:** con los tres saques del producto (Auto con «Saque nuevo» apagado, globo y drive) desde los dos cuadros, la pelota, antes de su primer bote, nunca pasa por dentro de la cápsula de referencia del jugador puesta en `ReceiverPosition`. La cápsula es la de `PlaceholderPlayers`: 1,80 m de alto y 0,28 m de radio, más el radio de la pelota.
- Regresión completa: EditMode, PlayMode (incluido `TimingAidTests`) y AutoQA `mvp`.

## Medición de la recepción, antes y después

`ServePositionsExperiment` (explícito, fuera de la regresión) con `scripts/diagnose-serve-positions.sh before|after`. Usa el preset por defecto y RallyParams por defecto (saque nuevo encendido). Juega 100 partidos por estilo de rival (Fondista, Definidor y Completo) en nivel Medio, con las semillas fijas `20261126 + 100000·estilo`. El humano es el bot novato de σ 90 ms y saca globo, como en la medición de M1.

- **Tasa de recepción:** el primer golpe del restador al saque queda en `Contact` (golpe aplicado), sobre todos los saques que recibe. Se calcula por separado para el humano (recibe el saque del rival) y para el rival (recibe el del humano).
- **Regla predeclarada:** si la tasa cambia más de 5 pp en cualquiera de los dos, se detiene el trabajo y se reporta. Aceptar esa dificultad le corresponde a Rafael.

### Resultado y aceptación del PO

| Restador | ANTES `32f400e` | DESPUÉS `8245996` | Cambio |
|---|---|---|---|
| Humano (recibe al rival) | 91,60 % (1722/1880) | 98,93 % (1936/1957) | +7,33 pp |
| Rival (recibe al humano) | 99,93 % (1379/1380) | 100,00 % (1538/1538) | +0,07 pp |

El cambio del humano supera los 5 pp y el trabajo se detuvo. Rafael lo aceptó (26 nov 16:54, vía SC-Lead), literal: «Sí, acepto que la devolución suba al 98,9 % con las nuevas posiciones de saque». Las posiciones, umbrales, criterios y pruebas quedan como estaban al detenerse.
