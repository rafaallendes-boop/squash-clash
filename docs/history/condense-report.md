# Informe de condensación de AGENTS.md — 15 nov 2026

Original: 276.945 bytes, 911 líneas, SHA-256 `286508c6…ab21f` en `7fdef02`; al integrar `main` `70fdd53` (merge de #56) el archivo quedó en 282.005 bytes, 946 líneas, SHA-256 `6207fe50…e276`, archivado byte a byte en [AGENTS-archive-2026-11-15.md](AGENTS-archive-2026-11-15.md). Las 35 líneas nuevas son las entradas de #56 (autorización, parada de adopción, decisión del 15 nov, diagnóstico de `Unreachable` y autorización de fusión), resumidas en §5–§7. Nuevo `AGENTS.md`: 8.722 bytes. Rama `docs/condense-agents` desde `main` `7fdef02`. Solo documentación: sin cambios en Assets, ProjectSettings, scripts ni tests; `docs/reviews/2026-10-25-first-shot-po-approvals.md` sin tocar.

## Reglas y autorizaciones del original

| Regla o entrada original | Resultado | Motivo / dónde quedó |
|---|---|---|
| Fuente de verdad: plan First Ball r2, spec v0.3, aprobaciones; no modificar spec/escenarios/umbrales/criterios | Queda | §1 y §3 |
| Desde I2: rama/worktree y PR, commits pequeños, revisión antes de fusionar, no escribir a Claude por herramientas | Queda | §2 |
| TDD con `scripts/test.sh`, RED válido, NotEvaluated nunca aprobado, sin éxito con cero tests/XML viejo | Queda | §3 |
| Gate PlayMode 354/354 en Mac y excepción Linux (10 nov) | Queda, con duda | §3; ver duda 2 |
| AutoQA obligatorio si toca timing, gesto o contrato (10 nov) | Queda | §3 |
| Corridas de test/check-release/AutoQA sin permiso por corrida; copia de device.env (10 nov) | Queda | §3 |
| Una sola autoridad de la pelota, predicción aislada, DefaultPreset.json única fuente | Queda | §3 |
| «Sin panel completo, ayudas visuales, reglas de punto ni gameplay posterior en I4» | Descartada | Histórico, sustituido por las etapas posteriores autorizadas |
| AX-1 (coordenadas, paredes, yaw) | Queda | §3, resumida |
| Pelota negra 20 mm y escala visual (I3/I6a) | Descartada | Histórico: implementado; el radio vive en defaults |
| Perfiles dev/prod/debug y extraScriptingDefines | Queda | §3 |
| Perfil de firma: vencimientos 28 oct y 12 nov | Descartada | Sustituida por la renovación hasta el 18 nov (11–12 nov), §8 |
| Comprobar vigencia antes de instalar, renovar el PO, no cambiar Xcode | Queda | §8 |
| «No cambiar la visibilidad privada del remoto» | Sustituida | Por la decisión del 11 nov (remoto público intencional), §3; ver duda 1 |
| No versionar device.env, credenciales, logs, builds; no exportar claves | Queda | §3 |
| Documentación en español, código en inglés, fuentes oficiales, sin frameworks | Queda | §3; ver duda 4 |
| Gate de física: P2 por defecto, P1 respaldo (23 oct) | Queda | §3 |
| P0-calib-1 aceptado, no promovido; restauración y cotejo byte a byte tras sesiones | Queda | §3 |
| Cierres I3–I7a, R1, T7.3, L2, H1/Etapa 6, Etapa 4–7 (22–24 oct) | Descartados | Histórico cerrado; First Ball cerrado (§6) |
| Observaciones de Claude I5a–I7a (panel, logs, Finish, cachés) | Descartadas | Sugerencias sin autorización; quedan en el archivo; ver duda 7 |
| First Shot S1: diseño, revalidaciones, calibraciones, sling, M2-4, PR #16–#28 (25–30 oct) | Descartados | Histórico; S1 aceptado el 30 oct (§6) |
| Decisión literal de aceptación de S1 y sus condiciones abiertas | Queda resumida | §6, observaciones abiertas |
| S2: plan D4, fases, paradas S2-TIME-01/S2-CLOCK-01, tandas 1–4, PR #29–#31 (30 oct–5 nov) | Descartados | Histórico; S2 aprobado con condiciones (§6) |
| PR #32 regla de retorno, desactivada; observaciones para activarla | Queda resumida | §6 y §7 (activación no autorizada) |
| S3: diseño, implementación, audio #35, validación física, sesión C8, aceptación con condiciones (5–11 nov) | Queda solo el estado | §6 |
| AutoQA #36–#39, checker v7, observación de salida 3/4 (6–9 nov) | Descartados | Histórico fusionado; la regla de uso queda en §3 |
| Fable: First Rally R1–R3, luego R1–R5(+R6); pase visual (11–12 nov) | Queda el estado | §2 y §6; ver duda 8 |
| S4: 4.1/4.3/Step 0, #45, O1–O6 (11 nov) | Descartados | Histórico fusionado; 4.5 no autorizada (§7) |
| D1–D4, Anexo C, aceptación reservada al playtest integrado (12 nov) | Queda | §6 y §7 |
| ClockMap #48, CRP-01 «ajeno a #48» (12 nov) | Descartado | Histórico fusionado; la regla de parada ante sorpresas queda en §2 |
| T4.2 #50, T4.4 #51, R3 «Open observation» (12–13 nov) | Queda el estado | §6 (R3 Fail 0/42 abierta) |
| B4 D36, implementación #53, H3b #54 (13 nov) | Queda el estado y los hashes | §5 y §6 |
| Diseño de potencia según la distancia #55 (14 nov) | Queda | §6 y §7, con #56 |
| Un solo Unity a la vez (13 nov) | Queda | §3 |
| Modo MVP: informes de una página, citas solo al cambiar reglas, fusión con tests y aprobación | Nueva | Encargo del PO del 15 nov, §2 |
| Zonas protegidas: timing, contrato y reloj → Codex | Nueva | Encargo del PO del 15 nov, §4 |

**Enlaces:** 39 archivos (no 38) mencionan `AGENTS.md` fuera de `docs/history/`; los 5 enlaces Markdown (`README.md` ×4, `docs/reviews/2026-10-25-first-shot-r2-plan-review-codex.md` ×1) resuelven y ninguno usa ancla. `scripts/check-release.sh`, `scripts/autoqa.sh` y el resto de `scripts/` no mencionan `AGENTS`. Se creó `CLAUDE.md` con `@AGENTS.md`.

## Dudas para el PO

1. **Remoto privado o público.** El encargo pide la regla «remoto privado», pero el 11 nov dijiste «Sí, es intencional; continuar y crear el PR» sobre el repositorio público, y `gh repo view` informa hoy `PUBLIC`. Dejé el estado público y la regla de no cambiarlo por iniciativa propia. ¿Quieres volverlo privado (lo harías tú) o corrijo la regla?
2. **Recuento del gate.** La excepción de Linux dice «siempre que el Mac dé 354/354», pero hoy PlayMode tiene 457 tests (#56). ¿Vale «el recuento completo vigente» en lugar de 354?
3. **Dueños fuera de Codex.** Física P2, solver, M2-4 y defaults quedan sin dueño. Además, #53 (vector de timing) y #56 (velocidad del golpe) los hicieron subagentes Sonnet con tu autorización. ¿#56 debe pasar a Codex por tocar timing?
4. La regla original dice «fuera de la necesidad inmediata de First Ball»; la generalicé a «necesidad inmediata». ¿Es correcto?
5. **Dónde registrar.** El modo MVP pide citas solo cuando cambia una regla. ¿Esa cita del 15 nov debe añadirse también al registro de aprobaciones en otra PR, que esta no toca?
6. **Cierres sin registrar en `main`:** #44 (merge `18b796a`, 11 nov) nunca se registró; #54/#55 solo constan en la rama de #56. ¿Se siguen registrando cierres o basta con el historial de Git?
7. ¿Se conserva en un backlog aparte la lista de observaciones de Claude sin autorizar (I5a–I7a, #50 O1–O3)?
8. La hora de tu autorización ampliada de First Rally (11 nov, 18:44:19Z) quedó «pendiente de confirmación del PO». ¿La confirmas?
9. Existe la rama local `visual/feel-pass` (`1c571be`) sin remoto ni PR. ¿Es trabajo activo de alguien que deba figurar en §6?
10. `README.md` (líneas 3, 5 y 7) enlaza a `AGENTS.md` por contenido que ahora solo está en el archivo (por ejemplo, el cierre I7a), y su estado está desactualizado (`433633f`). ¿Lo reapunto al archivo en esta PR o en otra?
11. La autorización de implementar #56 consta solo en la rama de #56; en `main` el último registro dice «La implementación no está autorizada todavía». La puse como «en curso» por ese registro. ¿Es correcto?
