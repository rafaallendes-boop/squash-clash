# Sombra de la pelota más marcada cerca del bote — spec corta

Fecha: 24 nov 2026. Encargo SC-PM, ítem 3 del set de recepción (prioridad de SC-Lead). Solo arte.

## Contexto

La pelota proyecta una sombra de contacto (`LegibilityAids`, objeto «Ball shadow», disco suave con borde que se desvanece, interruptor `aids.shadow`). Hoy `Tick` calcula `proximity = 1 / (1 + h · 0,4)` con `h` = altura de la parte baja de la pelota sobre el suelo, y pinta `alpha = 0,22 + 0,40 · proximity` y `escala = 0,085 + 0,085 · proximity`.

Con esa curva la opacidad casi no cambia donde importa: 0,62 en el suelo, 0,58 a 0,25 m y 0,47 a 1,5 m. El jugador no distingue «está por botar» de «viene alta». Se quiere que la sombra se marque más cuando la pelota está cerca de botar, para leer mejor dónde y cuándo va a botar.

## Alcance

- La **opacidad** de la sombra existente crece de forma suave y monótona al bajar la altura de la pelota.
- Curva: `n = 1 / (1 + (h / 0,7)²)` y `alpha = 0,22 + (0,72 − 0,22) · n`.
  - Tope 0,72 con `h = 0` (el tope evita una mancha negra opaca bajo la pelota y deja ver el suelo).
  - Piso 0,22 hacia `h → ∞` (el mismo mínimo visible de hoy; la sombra nunca desaparece).
  - Pendiente cero en el suelo (sin arista al tocar) y cola larga: estrictamente decreciente para toda altura finita.
  - Valores de referencia: h 0 → 0,72; 0,25 m → 0,66; 0,5 m → 0,57; 1 m → 0,39; 1,5 m → 0,31; 3 m → 0,25.
- La función vive en una clase estática pura (`BallShadowLook.Alpha(float height)`), sin estado ni asignaciones; `LegibilityAids.Tick` solo cambia la línea que pinta el alfa.
- La «nitidez» se entiende como mayor opacidad del núcleo; la malla del borde difuso y el tamaño del disco no cambian.

## Fuera de alcance

- Física, bote, timing, anillos, radio que juzga, contrato de contacto, umbrales, tolerancias, oráculos y criterios; M2-4 y la resolución del golpe.
- Hashes de §5 de AGENTS.md (defaults, SimulationKey, RallyParams): no hay claves de configuración nuevas ni cambios en `DefaultPreset.json`.
- Tamaño de la sombra, malla, color, shader, posición y el interruptor `aids.shadow`.
- Otras ayudas (estela, predicción, marcas de pared, retícula). No hay textos nuevos.
- La sombra solo **lee** la posición de render de la pelota; no escribe en el host.

## Criterio verificable

1. `Alpha(0) = 0,72` y `Alpha(h) ≤ 0,72` para toda altura; una altura negativa vale como 0.
2. `Alpha(h) > 0,22` para toda altura finita hasta 10 m, y `Alpha(∞) = 0,22` (piso).
3. Estrictamente decreciente en la malla de 0 a 6 m con paso de 0,05 m.
4. Suave: entre dos puntos de la malla de 5 cm la diferencia no supera 0,03.
5. Contraste: `Alpha(0,25) − Alpha(1,5) ≥ 0,30` (hoy 0,11).
6. Pura y sin asignaciones: 10 000 llamadas no asignan memoria gestionada.
7. En `LegibilityAids.Tick`: el alfa pintado coincide con `Alpha(h)` con `h = max(0, y − radio)`; la escala del disco sigue siendo `0,085 + 0,085 · proximity` y la posición `(x, 0,001, z)`; `Tick` no cambia `host.Current` ni `host.RenderPosition`.
8. El test existente `Shadow_FollowsBallXZAndRemainsVisibleAsHeightIncreases` sigue pasando sin cambios.

## Qué se prueba sin iPhone y qué en iPhone

Sin iPhone (PlayMode, `Tests/PlayMode/BallShadowNearBounceTests.cs`, más las puertas EditMode, AutoQA V/T3/volea y check-release): los criterios 1–8. AutoQA y las puertas confirman que nada de juego ni de hashes cambió.

En iPhone (Rafael, build de prueba; no sustituye a las puertas):
- La sombra se lee más oscura cuando la pelota está a punto de botar y se aclara al subir, sin saltos.
- Sobre el suelo de la cancha no ensucia ni tapa las líneas ni los anillos de recepción.
- Con luz exterior sigue visible en el piso (alfa mínimo 0,22) y no se confunde con la estela.
- Con el interruptor `aids.shadow` apagado no aparece.

## Textos

Ninguno: no se agregan ni cambian textos de la interfaz.
