# Árbitro simple: informe

26 nov 2026. Rama `rafaallendes-boop/sc-arbitro-simple`, base `origin/main` `8a32d42` (#120). Encargo `set11-arbitro`. [Spec](../superpowers/specs/2026-11-26-arbitro-simple.md).

**Estado: DETENIDO.** Hay una falla en la suite completa de EditMode, en una prueba existente que no se tocó. Según AGENTS §2, no se reintentó ni se corrigió, y no se corrió la AutoQA. No se fusiona.

## Qué cambia

Entre puntos de un partido sale una pastilla de vidrio con el canto del árbitro:

- el marcador con el del **sacador primero** (WSF);
- «¡Tin!» cuando el tin decide el punto, en el peloteo o en el saque;
- «Match ball» cuando el siguiente punto puede cerrar el partido, según `MatchTarget` y `MatchMargin`.

Va bajo la cabecera. En la pausa del saque del jugador va bajo «Toca abajo: saque rápido». Hay un interruptor DEV `RallyHud.RefereeOn`, encendido por defecto.

No cambian el preset, `RallyParams`, `DefaultPreset.json`, la física, el solver, el timing ni las pruebas existentes. Los hashes de AGENTS §5 siguen iguales: no se agregó ningún parámetro.

Implementó un subagente Sonnet dirigido por Claude. La revisión de Claude (Opus) no es independiente y queda pendiente la revisión cruzada de Codex.

## Pruebas, en este Mac

| Suite | Resultado |
|---|---|
| EditMode completo | `RESULT: total=1060 passed=1059 failed=1 skipped=0` — **no verde** |
| PlayMode completo | `RESULT: total=891 passed=891 failed=0 skipped=0` |
| AutoQA `mvp` | **No corrida** (detenido por la falla; el cambio no toca timing, gesto ni contrato) |
| check-release | No corrido |

La falla es `SquashClash.Core.Tests.Spike.SpikeRunTests.Benchmark_DiscardsWarmup`: «Expected: less than 1, But was: 1.2267000000000001d». Es un tope de reloj real (máximo menor que 1 ms) sobre una fábrica falsa. No comparte código con este cambio. Que el equipo estuviera cargado es una **hipótesis sin verificar**: ese día otra sesión (`sc-diag-fondo`) usó Unity, pero no se comprobó que coincidiera con la corrida. XML: `Logs/editmode-results.xml`, de 14:39:51Z a 14:41:36Z.

Pruebas nuevas:

- `RefereeCallTests`: 6 casos.
- `PointRulesTinTouchedTests`: 5 casos.
- `ZRefereeTests`: 2 casos.
- `ZRefereeLayoutTests`: 8 casos, que cubren los 4 perfiles con el cartel y con la guía de saque.

El RED salió por error de compilación, porque faltaban los miembros nuevos. Fue un RED de compilación, no de aserción, y se declara así.

## Desviaciones registradas en la spec

- **Revisión 1.** En la pausa del saque del jugador, la pastilla se cruzaba con `serve-guide-lob` en Mini, Pro y Pro Max. Se movió bajo `serve-guide-drive`.
- **Revisión 2.** No salió ningún punto con tin en escena en 74 partidos con piloto automático. El test en escena verifica la regla en cada pausa y no exige un tin. La rama del tin queda probada en EditMode y en el layout.

## Ancho medido de la pastilla (texto más largo «¡Tin!  Match ball  99–98», unos 22 px de alto)

| Teléfono | Ancho | Con el cartel (x, y) | En la pausa de saque (x, y) |
|---|---|---|---|
| SE 375×667 | 186,7 | 121,0, 218,1 | 121,0, 384,7 |
| Mini 375×812 | 186,5 | 101,9, 302,2 | 101,9, 420,0 |
| Pro 402×874 | 184,9 | 102,3, 307,1 | 102,3, 420,7 |
| Pro Max 430×932 | 184,6 | 102,7, 301,8 | 102,7, 419,9 |

## Qué decidir

Si la falla del benchmark se trata como de entorno y se repite la suite de EditMode con el equipo sin carga. Eso exige el visto bueno de quien orquesta, porque AGENTS §2 prohíbe reintentar por cuenta propia. Después se correrían la AutoQA y check-release.
