# G1 A1 — margen de release implementado; gates detenidos por REPORT-01

17 nov 2026 · Codex · Base #72 `1c9d2c6`. Criterios fijados en **`aa1a9f7` antes de implementar**; producto `1dba20d`, experimento `9bf1c5a`. **La política alcanza 93,26 % de contacto esperado sobre los 29 planes medios a P=1, con sigma intacto. La entrega está incompleta:** el empaquetado del manifiesto falló después de producir datos; AutoQA/check-release no se ejecutan.

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
| AutoQA V/T3, check-release | **No ejecutados**, no aprobados |

Se preservan los cinco out aparte de los 103 legales; no se excluyen stretched ni se cambia el denominador de C4. Mismas semillas/entradas que A1-bis y controles estadísticos predeclarados aprobados. Cuatro crudos y cuatro derivados coinciden byte a byte entre procesos, antes del fallo del empaquetado. No son tasas de puntos ganados contra usuarios.

**G1-A1-REPORT-01 (defecto del script experimental de Codex):** `summarize-g1-release-margin.py:196–197` conserva la búsqueda de `schema-red-runner`/`schema-green-runner` del arnés anterior. En A1, RED/GREEN viven en `Logs/evidence/g1-a1/red` y `green`. Tras verificar igualdad y copiar cuatro derivados, falla `FileNotFoundError` antes de escribir `manifest.json`. No falló la analítica ni Unity, pero el proceso de entrega **no terminó correctamente**. Los archivos científicos ya producidos no se modifican ni se regeneran. [Recibo de fallo y hashes](../validation/evidence/2026-11-17-g1-a1-release-margin/failure-receipt.json).

**Parada aplicada:** la autorización experimental no cubre corregir un defecto después de producir datos. Se detiene sin corregir el script, fabricar directorios para hacerlo pasar, repetir procesos o avanzar a AutoQA/check-release. Continuación propuesta: autorizar solo la corrección del empaquetado para referenciar los RED/GREEN reales y reanudar las puertas pendientes, conservando los datos y este intento. No modificar criterios ni resultados.

**A2 listo para revisión, separado:** [PR #73](https://github.com/rafaallendes-boop/squash-clash/pull/73), diseño `5b7f67f`, sin código ni Unity. C2 advierte que P=0,85 ya tiene cero outs superiores con miras bajas: conservar cero requiere revisión del literal de reducción estricta. A2 se mantiene detenido para revisión de Claude coordinada por el PO. A1 queda en PR borrador, sin fusión ni instalación.
