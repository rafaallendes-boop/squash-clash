# G1 A1 — margen de release y puertas completas

17 nov 2026 · Codex · #74 sobre #72. Criterios **`aa1a9f7` previos a implementar**; producto `1dba20d`, experimento `9bf1c5a`, puertas finales `0703a42`. **Media P=1: 93,26 % de contacto esperado sobre sus 29 planes, sigma intacto y todas las puertas completas.** REPORT-01 se corrigió con autorización propia; datos y fallo original preservados.

**Cambio mínimo:** FondoAi conserva la misma extracción gaussiana y añade solo la media necesaria para dejar 1,5sigma al borde que aprieta; ventana menor que 3sigma, al centro. En ventanas seguras devuelve el double original, incluido cero con signo. No cambia T*, plan, sigma, opción, aim, potencia, RNG, Humano/Bot/Touch ni contrato/reloj. Único parámetro: `FondoReleaseMarginSigma=1,5` en RallyParams.

| Comprobación | Resultado conservado |
|---|---|
| TDD RED, `3eef19d` | 10 tests: 2 controles pasan, 8 fallan por aserción; XML fresco, sin CS |
| GREEN, mismos tests | **10/10** |
| EditMode completo | **764/764**; tests existentes intactos, incluido rally |
| PlayMode completo | **581/581**; ProjectSettings restaurado |
| Experimento, procesos separados | **1/1 + 1/1**, 648 planes y 58.200 releases cada uno, sin fallos |
| Identidad en ventanas seguras | 207 planes, **41.400 releases idénticos bit a bit**; RNG y Bot verificados por tests |
| Contacto esperado / 103 legales | Fácil **84,46 %** < Media **89,98 %** < Difícil **95,80 %** |
| Media P=1, contacto / 29 planes | Analítico **93,2646 %**; muestra **5.401/5.800 = 93,1207 %**, criterio ≥90 % |
| Configuración | RallyParams **`927e304bbfec`**, proyección anterior `7d50ddaf5d4f`; defaults `430da2faa818`/121 y SimulationKey `166a96e319c7` intactos |
| AutoQA V y T3 | **PASS/0/1 ambos**, 88/88 tests cada uno, 18/18 escenarios-repeticiones aprobados por conjunto |
| check-release dev/prod | **OK ambos**, exportaciones frescas, módulos/arte verificados, gitDirty=false |

Se preservan los cinco out aparte de los 103 legales; no se excluyen stretched ni se cambia el denominador de C4. Mismas semillas/entradas que A1-bis y controles estadísticos predeclarados aprobados. Cuatro crudos y cuatro derivados coinciden byte a byte entre procesos, antes del fallo del empaquetado. No son tasas de puntos ganados contra usuarios.

**REPORT-01, conservado:** el empaquetador buscaba carpetas schema-red/schema-green del arnés anterior; falló después de producir datos. Se detuvo y se preservó el [recibo original](../validation/evidence/2026-11-17-g1-a1-release-margin/failure-receipt.json). El PO autorizó corregir solo las rutas y continuar las puertas. `0703a42` cambia esas dos líneas; los **20 archivos de datos** crudos/derivados conservan sus SHA-256 antes/después. Se generó el [manifiesto](../validation/evidence/2026-11-17-g1-a1-release-margin/manifest.json) sin repetir procesos científicos ni modificar método/criterios. [Prueba de integridad](../validation/evidence/2026-11-17-g1-a1-release-margin/packaging-integrity.json).

AutoQA se ejecutó secuencialmente V→T3→check-release, con inventarios vacíos antes de cada Unity, incluidos dev y prod por separado; ProjectSettings restaurado tras PlayMode/AutoQA. [Recibo de puertas](../validation/evidence/2026-11-17-g1-a1-release-margin/gates.json), fuente limpia y sin nuevos incidentes. No se cambió ningún test existente ni expectativa; no se usaron hashes/anclas nuevos para aprobar.

**A2 separado:** [#73](https://github.com/rafaallendes-boop/squash-clash/pull/73), diseño aprobado y ajustes previos fijados en `2b686d8`: búsqueda completa al release, caché en arrastre, tope lateral total Width/20, C2 0/≤1/≤1/≤2 y C8 p95 sin empeorar la base. No empezó Unity A2 antes de completar estas puertas A1. A1 queda listo para revisión independiente coordinada por el PO. Sin fusión ni instalación.
