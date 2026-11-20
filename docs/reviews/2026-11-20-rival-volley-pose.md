# Pose de volea del rival (2026-11-20)

Rama `claude/rival-volley-pose`, desde `f0fe172` (#84). Implementada por un subagente Sonnet dirigido por Claude: **la revisión de Claude no es independiente**.

## Qué cambia
Solo la figura. Cuando el rival volea, `CharacterView` dibuja la misma pose que el humano (diseño de volea v1, fila «Figura»):
- backswing corto, armado antes del golpe si la volea está prevista (`RallyDirector.RivalVolleyIntended`);
- contacto a la altura de P*v: alta (≥ 1,3 m) con la raqueta sobre el hombro, baja con la raqueta abierta a media altura;
- golpe corto (0,4 s) en lugar del follow-through completo;
- derecha o revés según el lado del plan.

`RallyDirector` solo expone `RivalSwingVolley`, `RivalSwingHeight` y `RivalVolleyIntended`, escritos justo antes de `RivalSwing` y que nada lee. Toggle DEV «Pose de volea del rival» (`RivalVolleyPose.Enabled`, por defecto sí, no persiste).

Sin cambios en Core, física, contrato, timing, `DefaultPreset.json`, SimulationKey ni RallyParams.

## Pruebas (TDD)
RED por aserción (5 de 6 fallaban; la de determinismo ya pasaba, es la guarda) y luego GREEN: `ZRivalVolleyPoseTests` 7/7, que incluye alta/baja, lado, golpe corto, toggle DEV y un partido real con el Definidor. **Mismo seed con pose sí/no: 240 s de simulación idénticos línea a línea** (posiciones de ambos, golpes, puntos y voleas del rival).

## No hecho, por indicación de Claude (liberar el Unity compartido)
Suites completas EditMode/PlayMode, AutoQA V/T3/volley y check-release: **no corridos**; los corre Claude sobre la integración. Capturas de pantalla de volea alta y baja: **no hechas**. La ráfaga ámbar del rival tampoco (opcional; requiere acoplarla al `ShotController`, no es trivial).
