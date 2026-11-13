# B4 — implementación de calidad causal (T3.1, D6/D36)

Estado: implementación en curso, sin entrega ni aceptación inferidas. Base main `09764ab`; rama independiente `firstshot/causal-quality-impl`. [Encargo literal y cierre de #52](2026-10-25-first-shot-po-approvals.md#b4--encargo-literal-de-implementación-y-cierre-de-pr-52-13-nov-2026), primer commit `1772bfc`. La revisión se coordina con el PO; sin fusión automática. First Shot sigue pendiente de prueba jugable integrada.

## Método de implementación y TDD

La referencia se calcula antes del intento y no conserva referencias a la trayectoria mutable ni al reloj vivo. Se congela una copia de ClockMap para calcular J_A con ContactContract.Decide real. Se particiona por los extremos de Perfect/Valid, T*, K, pasos y medios pasos, eventos, observaciones del reloj y presentación corregida. Se consulta cada borde y cada celda abierta; no se desplazan ventanas con epsilon. Las posiciones corporales efectivas por paso se congelan al preparar q. La curva poligonal, el arco firmado desde T* y el intervalo ±L se recortan a K, incluidos sus extremos fraccionarios.

Evaluate recibe referencia, cuerpo y punto efectivo; carece de Δ, categoría o bool perfect. El vector timing usa exclusivamente distancia euclídea mínima a Z_T. Los nodos aislados se conservan, incluido L=0. No se usa la distancia de arco como magnitud del efecto. La igualdad exigida pertenece a una misma recepción/contexto; entre tempos se documenta la diferencia.

E3/E5 usan distancia tridimensional al centro de referencia corporal y rampas lineales: E3 desde stretchStart·R hasta R; E5 desde jamDistance hasta distancia cero. Se conserva cada causa y t=0. Composición por componente antes del solver; los pesos y topes serán los diez valores autorizados [EXPERIMENTO]. Esta elección implementa el inicio de estiramiento/atoramiento sin introducir un cambio de ventanas ni una penalización por categoría.

Primer grupo: API neutra compilable y diez pruebas geométricas. Una corrida inicial sin XML por error CS1503 en una aserción nueva no cuenta como RED. Corregida la preparación de pruebas, XML fresco: **10 ejecutadas, 0 Pass, 10 fallos por aserción**, sin CS. Evidencia local ignorada `Logs/evidence/b4-causal/01-red.xml` y `01-red.log`. GREEN pendiente al guardar esta nota.

## Alcance conservado

No se modifica ContactContract, ventanas, P2, M2-4, SimulationKey, valores heredados ni rangos existentes. Sin H3b completo, medición nueva de R3, iPhone, build nativa, instalación, #32, nuevas mecánicas First Rally ni arte. Se conserva evidencia histórica CQ-ZONE-v1; no se repite su experimento. R3 conserva Fail 0/42 como observación abierta. Cualquier defecto o sorpresa se registra y detiene según el encargo.

Hashes/anclajes medidos en Unity y commit propio, integración humana/rival, pruebas D6 restantes, regresiones completas, AutoQA, check-release y restauración de transitorios: pendientes. No se anticipan resultados.
