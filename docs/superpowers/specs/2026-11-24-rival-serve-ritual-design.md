# Ritual de saque del rival — spec corta

Fecha: 24 nov 2026. Encargo SC-PM, ítem 4 del set de recepción (prioridad de SC-Lead, visto bueno de Rafael 24 nov 15:07). Solo presentación.

## Contexto

Cuando el rival tiene el saque, el jugador no ve nada que anuncie el saque: la pelota del punto anterior queda donde paró, el rival camina a su posición de espera y, al terminar la pausa, la pelota aparece de golpe saliendo desde el cuadro de saque. Se quiere que, antes de sacar, el rival bote la pelota en su cuadro, como hace un jugador real, para que el jugador anticipe el saque.

### La pausa en el código y en la spec vigente

- La pausa entre puntos es `RallyDirector.WaitBetweenPoints` (`RallyDirector.cs:90-91`). Antes de un saque del rival vale `MatchPauseSeconds` = **2,5 s** (`RallyParams.cs:32`). La spec del saque nuevo lo fija así: «La pausa antes del saque del rival sigue en 2,5 s» (`2026-11-22-front-1b-serve.md:52`, también `:34` y la tabla de `:73`).
- Los **1,6 s** del encargo son `RestartSeconds` (`RallyParams.cs:28`), la pausa del peloteo libre, sin marcador. Ahí no hay saque: el punto empieza con un lanzamiento neutro al jugador (`RallyDirector.cs:250`). El rival solo saca en partido.
- **Desviación declarada:** el ritual va dentro de la pausa real antes del saque del rival, de 2,5 s, que no cambia. El ritual dura 1,35 s, así que también cabría en una pausa de 1,6 s. Ninguna spec fija qué se ve durante la pausa, de modo que el ritual no la contradice.
- La pausa empieza con el congelado de fin de punto de 100 ms (`PointEndFreeze`), que es solo de vista y no se toca.

## Alcance

1. **Pelota visual aparte.** Es un objeto de dibujo con la misma malla, el mismo material y el mismo halo que la pelota. No tiene colisionador ni componente de física, y no lee ni escribe en el host (`BallSimulationHost`). Mientras se ve, la sombra de contacto existente (`LegibilityAids`, mismo aspecto de `BallShadowLook`) se dibuja bajo ella en lugar de bajo la pelota real; la sombra es solo dibujo. Es imposible que dispare botes, contactos, sonidos de bote, métricas o el oráculo, porque nada de eso la escucha.
2. **Cuándo.** Hay ritual solo en la pausa antes de un saque del rival en partido (`Match` no nulo y no terminado, `Match.Server == Rival`), con el interruptor de código `RivalServeRitual.Enabled` encendido (por defecto sí). Si la pausa es más corta que 0,1 s + 1,35 s, no hay ritual.
3. **Línea de tiempo** (con la pausa de 2,5 s, `t` = segundos desde el cierre del punto):
   - `t` 0 a 0,1: congelado de fin de punto, sin cambios.
   - Desde el fin del congelado, la **figura** del rival se dibuja quieta en su cuadro de saque, mirando a la pared frontal. Es lo mismo que ya hace el jugador antes de su propio saque. Solo cambia dónde se dibuja: el cuerpo simulado del rival no se mueve de su camino de hoy.
   - `t` = pausa − 1,35 = 1,15: aparece la pelota del ritual en la mano libre del rival. Desde ese instante la pelota real y su estela dejan de dibujarse (siguen existiendo en el host) y la sombra sigue a la pelota del ritual.
   - `t` 1,15 a 2,15: **dos botes** de 0,5 s cada uno, entre la altura de la mano (`ServeHandHeight`, 1,1 m) y el suelo. La pelota va rápida abajo y lenta arriba. El punto de bote queda 0,32 m hacia la mano libre y 0,10 m hacia la pared frontal desde el centro del cuadro.
   - `t` 2,15 a 2,35: la pelota pasa de la mano al punto exacto desde donde sale el saque, `(caja.x, ServeHandHeight, caja.z)`.
   - `t` 2,35 a 2,5: quieta en ese punto.
   - `t` = 2,5: empieza el saque igual que hoy. La pelota del ritual se oculta y vuelve a dibujarse la pelota real, que sale de ese mismo punto.
4. **Reloj.** El ritual se calcula con el tiempo de pausa de la propia pausa (`waited`). Si el juego se pausa (menú, foco), el ritual se detiene con él. Si la pausa se salta, salta con ella.

## Fuera de alcance

- La duración de la pausa (2,5 s; ni `RestartSeconds` 1,6 s ni `ServeKindHumanPauseSeconds` 3 s), el instante del saque, su trayectoria y su física, el tipo de saque elegido por el rival y su generador aleatorio.
- Timing, anillos, radio que juzga, contrato de contacto, M2-4, resolución del golpe, umbrales, tolerancias, oráculos y criterios.
- Hashes de §5 de AGENTS.md: sin claves nuevas en `RallyParams` ni en `DefaultPreset.json`. Las constantes del ritual son de vista y viven en `RivalServeRitual`.
- El saque del jugador, el peloteo libre y la pausa antes de un saque del jugador.
- Sonido del bote del ritual, animación del brazo y botón DEV. No hay textos nuevos.

## Criterio verificable

1. La línea de tiempo pura (`RivalServeRitual.Sample`) muestra la pelota solo dentro de la pausa, desde `pausa − 1,35`. Los dos botes tocan el suelo (centro a la altura del radio) en `inicio + 0,25` e `inicio + 0,75`, y terminan 0,35 s antes del saque. En los últimos 0,15 s la pelota está quieta en el punto de salida del saque. Fuera de la pausa, o con una pausa demasiado corta, no hay pelota.
2. En un partido real, durante la pausa antes del saque del rival, el ritual se ve (la pelota del ritual activa y la pelota real oculta) y termina antes del saque. En el primer paso del punto siguiente la pelota del ritual está oculta y la real se ve.
3. Con el ritual encendido y apagado, en un partido idéntico (piloto automático, misma semilla): las fases, el paso del host, el `LaunchId`, los cuerpos simulados, el marcador, la posición y la velocidad de la pelota real son **idénticos paso a paso**, durante el punto, la pausa y los primeros pasos del saque. Lo mismo pasa con la cantidad de eventos `ContactRaised` y con las líneas del archivo de métricas.
4. Ningún componente de la pelota del ritual es un `Collider`, un `Rigidbody` ni un `BallView`.
5. Sin cambios de hashes de §5.

## Qué se prueba sin iPhone y qué en iPhone

- **Sin iPhone (EditMode, PlayMode, AutoQA, check-release):** los criterios 1 a 5, las regresiones completas y AutoQA V/T3/volea sin cambios.
- **En iPhone:** que el ritual se lea, que la pelota se vea en la mano libre y no tapada por la figura desde la cámara, que los dos botes se distingan con la sombra, que no haya salto feo de la figura al cuadro y que la salida del saque empalme con la pelota en la mano. También que no moleste ni tape el aviso del punto.
