# G1 A1-bis — resultado del diagnóstico de timing

17 nov 2026 · Codex · #72 sobre #71 · Código `fe7633d`. **Hipótesis respaldada en esta grilla:** el sigma fijo pierde más recepciones al subir la potencia. Importan ancho **y asimetría**; no basta la velocidad de salida. No se reproduce el punto casi seguro por vidrio.

[Método `73d86a2`](../validation/2026-11-17-g1-a1bis-timing-method.md) intacto. Dos procesos **1/1**, 648 planes y 58.200 releases cada uno; ocho archivos idénticos byte a byte. Compilación previa aprobada, sin Unity concurrente. [Evidencia e intentos conservados](../validation/evidence/2026-11-17-g1-a1bis-timing/README.md).

**Recepción esperada con ruido; 103 vuelos legales, otros cinco out separados:**

| Rival / sigma real | Planes / 103 | Air / Late entre planes | Contacto esperado / 103 |
|---|---:|---:|---:|
| Fácil / 55 ms | 93 | 10,57 / 4,08 % | 77,06 % |
| Media / 38 ms | 97 | 9,40 / 3,55 % | **81,98 %** |
| Difícil / 22 ms | 101 | 6,73 / 1,86 % | 89,63 % |

Media: sin ruido 97/103; timing pierde **12,56 recepciones esperadas** (9,12 Air + 3,44 Late), además de seis Unreachable. Por P=0,70/0,85/1,00, contacto sobre vuelos legales: **87,80/83,43/73,84 %**, sumando probabilidades individuales.

**Media, por velocidad nominal de salida:**

| m/s | Legales / planes | Ventana mediana real | Fallo de timing entre planes | Contacto total esperado |
|---|---:|---:|---:|---:|
| [20,24) | 35 / 34 | 395,70 ms | 8,62 % | 88,77 % |
| [24,28) | 28 / 27 | 372,84 ms | 12,90 % | 83,99 % |
| [28,32)¹ | 32 / 29 | 285,60 ms | 18,52 % | 73,84 % |

¹ Solo 28 m/s nominales. [16,20), ocho vuelos, consta en tablas. Velocidad mediana en T*: 7,19/6,06/7,48 m/s; P2 frenó la pelota. Tiempo real, tempo 0,65, LatencyCompMs=0, sin ventana simétrica ni límite Valid ±100 ms.

**Causa:** 28/97 planes medios tienen algún margen menor que sigma=38 ms. `A1-076`, con vidrio: K relativo **[−1,07; +217,77] ms**, contacto 51,12 %, Air 48,88 %. `A1-083`: [−7,31; +14,40] ms y 22,39 % de contacto. Un ancho total grande no protege si T* queda junto al borde.

P=1 con vidrio legal: **23/25 planes → 19,11/25 contactos esperados (76,45 %)**. Humano/Bot, mismo contrato/geometría: ventana mediana 347,07 ms frente a 348,37 del Rival; delta=0 recibe 99/103. Esperas/planes distintos y Bot sin ruido: no estima usuarios reales. Referencia histórica del PO: 3/18/24 % de punto directo, 13 timing y nueve Unreachable en 22 puntos rápidos. Otra población/builds y sin bandera de vidrio: no valida estas tasas.

**Opciones; ninguna implementada:**

| Opción | Efecto y límite | Hashes y AutoQA |
|---|---|---|
| Sigma según ambos márgenes temporales | Ajustar precisión al contacto disponible; puede hacer demasiado preciso al Rival en bordes | Nuevos parámetros solo en RallyParams moverían su hash propio; H0 |
| Release del Rival hacia el interior de la ventana, con sigma acotado | Trata la asimetría; cambia distribución Perfect/Valid y exige diseño previo | Código solo: hash propio intacto; si parametriza RallyParams, cambia ese hash; H0 |
| Mantener política y contrastar sensación en próxima prueba autorizada | Conserva las pérdidas medidas; no acredita que sean deseables | Hashes intactos; sin implementación ni gates nuevos |

**H0:** DefaultPreset/P0, SimulationKey y cuatro `Scenario.ExpectedHash` intactos; escenarios/oráculos/expectativas sin cambiar. Las correcciones exigen TDD, suites, AutoQA **V/T3 PASS/0/1**, check-release y revisión. El PO elige. **A2 mantiene su parada de diseño antes de implementar; sin fusión ni instalación.**
