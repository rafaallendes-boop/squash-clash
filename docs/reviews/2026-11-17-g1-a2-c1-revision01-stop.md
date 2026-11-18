# G1-A2-C1 — única revisión ejecutada; parada definitiva de esta revisión

17 nov 2026 · Codex · #73 · Diseño revisado **`7653687` antes de implementar/ejecutar**, candidato **`eeb4a83`**. **C1 vuelve a fallar: 23/24 testigos. Se detiene sin otra revisión, ajuste ni corrida.** No se declara imposible toda solución bajo el tope; el resultado corresponde a esta política fijada.

**Cambio autorizado:** el coste deja de perseguir solo x2 respecto de x*. Paralela: suma de distancias de ambos botes a la pared propia, normalizada por Width. Cruzada: distancia del segundo a la pared contraria, normalizada, con cruce estricto de mitad exigido antes de comparar el coste. Se conservan los términos de profundidad y ápice, sus objetivos/techo, preferencias previas, semillas, cinco refinamientos, dominio lateral total Width/20 y 24 evaluaciones. C1–C8, malla y cuerpo de los tests permanecen idénticos.

| Comprobación | Resultado |
|---|---|
| Invocaciones C1 de esta revisión | **Una**; `scripts/test.sh editmode ShotIntentSolverTests` |
| XML fresco | **25/26 pasan, 1/26 falla por aserción C1**, sin CS ni omitidos |
| Bloque C1 / controles | **23/24 testigos**, **2/2** controles de identidad inactiva |
| Invariantes de las 24 solicitudes | Tope x total, altura solo baja y velocidad intacta; **24 evaluaciones**, DoubleBounce legal en todas |
| Candidato anterior `4fe66ad` | Código/historial conservados; todos los SHA-256 de datos/recibos originales iguales |
| Integración de main tras #74 | Merge **`46c0f54`**, segundo padre exacto **`95d75a2`** (#74), sin rebase; producto A1 intacto |

**Único fallo:** SL6, cruzada, P=0,80, mira y=0,88 m. El segundo bote queda a **1,06313968 m** de la pared contraria; el límite sigue siendo **≤1 m**, exceso **0,06313968 m**. Sí cruza la mitad y termina legalmente en doble bote. Frontis 0,766000032 m, ápice 1,49877977 m y profundidad del segundo bote 8,893886 m cumplen sus límites. Mira efectiva `(3,44985247; 0,766000032)`. No se redondea la distancia para aprobar.

La [tabla completa](../validation/evidence/2026-11-17-g1-a2-c1-revision01/witnesses.tsv) incluye los 24 casos y fallbacks. [Manifiesto](../validation/evidence/2026-11-17-g1-a2-c1-revision01/manifest.json): fuentes, padres del merge y hashes de la evidencia nueva/anterior. XML/logs e inventario vacío previo se preservan en `Logs/evidence/g1-a2/c1-revision01/`, separados de `red/` y `green/` del candidato original. No se sobrescribe esa evidencia.

**Parada conforme al PO:** no se ejecutan malla completa, segundo proceso, C2–C8, integración causal/visual, suites completas ni AutoQA V/T3/check-release de A2. El núcleo sigue aislado de la ruta de producto. No hay medición de costo por solución ni p95 de PlayMode: no se presentan los tiempos del runner dirigido como rendimiento del juego. #73 permanece borrador. No se fusiona #73, no se instala ni se accede al iPhone.
