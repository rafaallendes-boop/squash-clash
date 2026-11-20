# Récords personales — informe

Rama `claude/personal-records` desde `f0fe172`. **Sin push, PR, fusión ni instalación.** Lo hizo un subagente Sonnet dirigido por Claude: **la revisión de Claude no es independiente.** Idea del explorador creativo, aprobada por el PO («avanza en las 3 ideas»).

## Qué hace

- **Persistencia** (`PersonalRecords`, PlayerPrefs `sc.records.*`): rally más largo, mejor racha y racha actual, victorias por nivel, mayor diferencia en una victoria y victoria más rápida.
- **Tarjeta de resultado:** una línea en rojo de línea, «¡Nuevo récord! Rally más largo: 23 golpes · Mejor racha: 3 victorias», con todos los récords del partido en la misma línea. Si no cae ninguno, no hay línea.
- **Menú:** tercer enlace «Récords» junto a «¿Cómo se juega?» y «Créditos» y pantalla «Tus récords» con el estilo de Créditos (valores «—» mientras no hay datos). Captura: `docs/validation/evidence/2026-11-20-personal-records/` (menú con el enlace y pantalla de récords).
- **DEV:** sección «Récords personales» con «Récords: sí/no» (apagado no escribe ni muestra) y «Borrar récords». `ZFirstRunFixture.ClearPrefs` limpia las claves nuevas.

## Reglas

Solo cuentan los partidos iniciados con Jugar (`RallyHud.MenuMatch`, que se apaga al dejar de haber partido activo) y que nunca corrieron en autopilot. Práctica, demo, el botón dev «Partido», AutoQA y partidos abandonados no escriben nada (se escribe solo al terminar el partido). Decisiones mías: un empate no es récord; el primer valor sí se anuncia, salvo la racha, que se anuncia desde 2 victorias; las victorias por nivel son contador, no récord. Revancha desde la tarjeta cuenta como partido nuevo.

## Pruebas (TDD)

Nuevas, ninguna existente modificada salvo el fixture (ClearPrefs) y una prueba de ajuste en `ZDeviceLayoutTests`: `ZRecordsTests` (reglas puras, línea en la tarjeta, sin línea, autopilot, partido fuera del menú, abandonado, interruptor apagado, pantalla, botones de 44 pt, panel DEV, limpieza del fixture) y `Records_FitInsideTheSafeArea` (SE a Pro Max). **RED por aserción:** 15 de 31 fallos con un `Apply` vacío y sin UI (`missing element menu-records`, `match-result-record`, listas vacías); la lógica pura se escribió antes que sus pruebas y se verificó el RED con un stub temporal. **GREEN:** filtros dirigidos (récords, `ZFirstRunMenuTests`, `ZMatchCardTests`, `ZDeviceLayoutTests`, `ZRallyHudTests`) **73/73**.

## No ejecutado (instrucción del PO)

Por la prioridad del PO (mejorar lo existente antes que lo nuevo) **no se corrieron las suites completas EditMode/PlayMode, AutoQA (v, t3, volley) ni check-release**; las corre el PO una vez sobre la integración. Sin cambios en Core, física, contrato, timing, `DefaultPreset.json`, SimulationKey ni RallyParams; solo vistas y PlayerPrefs.

## Dudas

- La tarjeta con la línea no tiene captura (el arnés de captura no avanza el HUD y la derrota pasiva da rally 0); la cubre una prueba.
- Falta verlo en el iPhone: tres plaquitas en el SE (375 pt), donde «¿Cómo se juega?» toma el doble de ancho.
