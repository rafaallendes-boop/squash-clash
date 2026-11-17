# F1 — predeclaración y parada previa a Unity

17 nov 2026. Rama `codex/f1-fatigue-energy`, base main `766dedd` (#75 fusionada). Encargo y spec v2.2 leídos completos, incluida «Qué cambió en main». Método B predeclarado en commit propio `2708fb4`: [método](../validation/2026-11-17-f1-fatigue-method.md). Sin implementación, RED, experimentos, suites, AutoQA ni check-release ejecutados; ningún resultado se declara aprobado. Sin fusión, instalación ni acceso al iPhone.

## F1-PREFLIGHT-01: tests con hash completo o 121 claves

Inspección estática, no fallo de una corrida. Añadir las once claves autorizadas lleva el preset de 121 a 132 claves y cambia su hash completo. El encargo prohíbe «cambiar expectativas de tests existentes» y solo exceptúa las cuatro anclas de `Scenario.ExpectedHash`; la spec §8 exige parar si algún test necesita otro tratamiento. Se detectan aserciones incompatibles en:

- EditMode: `Shot/DefaultPresetHashTests.cs:62`, `M24ProjectionTests.cs:49`, `TimingAidConfigTests.cs:18,44`, `S3FeedbackConfigTests.cs:23`, `ContactContractTests.cs:96`, `ShotSolverTests.cs:54`, `CausalQualityTests.cs:97`, `CausalReferenceCoverageTests.cs:90`, y `Rally/RivalReleaseTimingTests.cs:38–39`.
- PlayMode: `AutoQAT3OracleTests.cs:34–35`, `ZRallyHudTests.cs:99,181`, `ZFirstRunMenuTests.cs:78,141`, `ZRallyDirectorTests.cs:164`.

`fatigue.enabled=0` no elimina claves ni restaura el hash completo, por lo que la excepción prevista para los tests de rally/partido no resuelve estas aserciones. No se modificaron tests, catálogo, defaults, hashes ni AutoQA.

**Tratamiento propuesto para decisión del PO:** conservar las expectativas históricas en tests que verifican valores heredados usando una proyección explícita que excluya únicamente `fatigue.*`; para tests de retorno al preset completo, comparar contra el hash del preset realmente cargado, junto con el test independiente de proyección histórica. Agregar tests nuevos que exijan 132 claves, los once valores/rangos y hashes completos medidos en Unity. El PO debe delimitar también el tratamiento de `AutoQAT3OracleTests`, porque A2 solo autoriza cambiar las cuatro anclas y no sus oráculos. No se aplica esta propuesta sin resolver la restricción.

## F1-PREFLIGHT-02: intención del saque automático

La spec §3 exige cobrar los golpes fuertes por P de intención, incluido el saque. `ServePlanner.Find` (`Core/Rally/ServePlanner.cs:17–37`) produce únicamente un `BallState`, buscando velocidades balísticas de una grilla; no hay P de intención elegida por gesto o IA para ese saque. El método predeclarado dice usar la P real existente del saque, pero esa magnitud no existe en main. Se conserva el método sin reescribirlo: no se elige una P ni una conversión de velocidad por iniciativa propia. Hace falta definir esa P de contabilidad o su conversión, sin cambiar trayectoria, física ni solver; debe fijarse antes del experimento.

## Siguiente paso

Resolver ambos puntos con el PO, registrar la decisión y, si corresponde, fijar la precisión del método en un commit anterior a datos. Luego captura de referencia main, TDD, implementación F1 y puertas completas, con un solo Unity e inventario por lanzamiento. La autonomía confirmada cubre fallos de código experimental/paquetización dentro de sus límites; no cubre estas decisiones de tests, oráculos o producto. F2 espera F1 fusionada por aprobación independiente del PO.

**Resolución del PO (17 nov, antes de datos):** ambos puntos resueltos según la spec v2.3 y el registro. Se autoriza solo actualizar literales de hash/cantidad en los tests listados, no la propuesta de reescribir aserciones. Saque automático P=0 para gasto fuerte. Se retoma F1.
