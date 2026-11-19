# Informe de condensación de AGENTS.md — 18 nov 2026

Original: 26.075 bytes, 95 líneas, SHA-256 `d2da9e25…8655`, archivado byte a byte en [AGENTS-archive-2026-11-18.md](AGENTS-archive-2026-11-18.md) (`main` `75b9ce8`). Nuevo `AGENTS.md`: 8.126 bytes (meta de ≤ 8 KB cumplida; la de ~6 KB no, por conservar fuentes y hashes). Solo documentación.

| Regla o entrada del original | Resultado |
|---|---|
| §1 fuentes de verdad | Queda (§1); agrega spec de cansancio v2.3 y el registro nuevo `2026-11-18-po-approvals.md` |
| §2 roles; Fable terminó el 16 nov; vuelta al modo normal | Sustituida por el modo product lead del 18 nov (§2); Fable/Codex fuera de la tanda |
| Modo MVP, un incremento = una PR, fusión con merge commit y segundo padre | Queda (§2) |
| Ante un defecto: registrar y detenerse | Queda (§2) |
| §3 reglas inamovibles (spec, autoridad de la pelota, P2, AX-1, TDD, gate Mac, AutoQA, Unity único, perfiles, idioma, no versionar, remoto público, P0-calib-1) | Quedan (§3), acortadas |
| Excepción Linux (3 clases de tests) | Queda (§3); se omite la cifra 354/354 (ver dudas) |
| §4 zonas protegidas | Queda (§4), con la nota del 18 nov |
| §5 hashes F2/F1 «sin fusionar» | Descartado como estado; los valores son los vigentes en `main` (§5) |
| §5 G1 A1 / #74: proyección `7d5f…`, medidos sobre `9bf1c5a` | Solo en archivo; queda RallyParams `927e304bbfec` |
| §5 hashes anteriores a U1 y a #56 | Solo en archivo, ya no vigentes |
| §5 SimulationKey, P0-calib-1 SHA-256, proyecciones de 121 claves | Quedan (§5) |
| §6 F1/F2: RED, suites, Experimento B, parada F1-AUTOQA-01 | Solo en archivo; fusionadas (#76/#77) |
| §6 herramientas de cansancio #82; observación en juego real | Quedan resumidas (§6) |
| §6 historial de merges #58–#80, U1–U3, E1, #63 | Solo en archivo (ver `git log`) |
| §6 worktree U1 retirado | Solo en archivo, ya no vigente |
| §6 etapas First Ball / First Shot / Rally; dirección del PO; observaciones abiertas | Quedan en una línea cada una (§6) |
| §6 app instalada `0.1.0+923daa3` | Queda (§6) |
| §7 F1/F2 (PREFLIGHT, A2 v2.2) | Resumida (§7); detalle en archivo y registro histórico |
| §7 G1 cerrado por decisión del PO | Resumida (§7) |
| §7 U1–U3, fusiones de #58–#60, #63, sesión 1 de 4.5, instalaciones | Solo en archivo, ya ejecutadas |
| §7 «No autorizado» | Queda (§7) |
| §8 perfil de firma e instalación | Queda (§8) |
| Citas literales del PO en §6/§7 | Solo en archivo y en el registro histórico |

## Dudas y contradicciones detectadas (no resueltas)

1. §3 del original dice que el recuento de PlayMode en Mac es «354/354», cifra de hace meses; las suites actuales son 803/611. Se omitió la cifra.
2. §6 del original dice «F1 en curso», «#67 borrador» y «CRP-01 pendiente de fusión»; la historia de `main` muestra todo fusionado. Se corrigió.
3. El original llama «remoto privado» al encargo del 15 nov pero §3 lo declara público intencional; se conserva «público».
4. §7 del original dice que G1 A2 «no tiene autorización para más corridas»; el encargo del 18 nov (modo product lead) incluye G1 A2. Se anotó que lo reabre, sin que el PO lo haya dicho literalmente para A2 más allá de la lista de entregables.
