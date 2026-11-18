# Herramientas de prueba de cansancio (18 nov 2026)

Rama `claude/fatigue-test-tools`, base `main` `7d869e8`. **Claude dirigió el trabajo (subagente Sonnet); la revisión de Claude no es independiente.** Sin fusionar ni instalar.

## Autorización del PO (literal, 18 nov)
«Dale barra + modo dev»

## Qué cambia
1. **Barra de energía (bug):** `.sc-energy-col` pasa a ancho fijo de 60 px (`align-items: stretch`, nombre centrado); la pista queda de 48 px para «TÚ» y «RIVAL». Antes la pista tomaba el ancho del texto del nombre (TÚ ≈ 20,6 px, RIVAL ≈ 39,7 px a igual energía). Solo `Hud.uss`.
2. **Cansancio de prueba (solo DEV):** `RallyDirector.SetTestEnergy(double)` guarda un valor pendiente de un solo uso (limitado a 0..100); `BeginPoint` lo aplica a ambos jugadores al inicio del siguiente punto y lo borra. Sin uso, el comportamiento es idéntico. El valor queda pendiente aunque no haya Rally/Partido y se aplica al primer punto del próximo. Botones «Energía 40», «Energía 10», «Energía 100» en `Assets/SquashClash/DevPanel/FatigueTestSection.cs` (pestaña Parámetros, tras First Shot), por `DevControlLog.Run` con nombres `fatigue-test-40/10/100` (quedan en métricas como dev-control). Solo tiene efecto visible con `fatigue.enabled=1`.

Sin cambios en `DefaultPreset.json`, claves, hashes, SimulationKey, RallyParams, `Fatigue.cs`, física, timing, ProjectSettings ni AutoQA.

## RED (por aserción, antes de implementar; 4 de 5 fallan, `3329863`)
`ZFatigueTestToolsTests` (PlayMode): ancho de pista TÚ vs RIVAL (esperado 20,65 ± 0,5, fue 39,68); `SetTestEnergy` inexistente (2 tests); botones `fatigue-test-40` ausentes. El control «sin hook empieza en 100» ya pasaba (etiquetado control). Un fallo posterior fue error del propio test (`AutoRestart=false` impedía el segundo punto), corregido en el test.

## GREEN y compuertas (head `2d7a265`, árbol limpio)
- Fixture nueva: 5/5. **EditMode 803/803. PlayMode 611/611.**
- AutoQA V `20261118T123128Z-2d7a265-v-15461` y T3 `20261118T123151Z-2d7a265-t3-12021`: PASS/0/nivel 1 (88/88 cada una).
- check-release dev/prod OK (build 863); prod sin módulo DevPanel.
- `git diff origin/main -- DefaultPreset.json` vacío: hashes de §5 sin cambio.
- Nota: Unity reescribe `ProjectSettings/GraphicsSettings.asset` y `QualitySettings.asset` en cada corrida; se revirtieron con `git checkout` antes de cada commit.

## Cómo probarlo en el celular
Con `fatigue.enabled=1` (Parámetros): abrir el panel DEV, pulsar «Energía 40» o «Energía 10», iniciar Rally/Partido (o terminar el punto en curso): el siguiente punto parte con ambas barras en 40 (ámbar) o 10 (rojo). «Energía 100» restablece. Es de un solo uso. Las barras de TÚ y RIVAL miden lo mismo.
