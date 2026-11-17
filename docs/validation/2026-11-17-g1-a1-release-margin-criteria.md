# G1 A1 — media mínima de release del Rival (criterios fijados)

17 nov 2026 · Codex · Base `1c9d2c6` (#72 abierta); rama `codex/g1-a1-release-margin`, PR propia apilada. Este documento se confirma **antes de tests nuevos, implementación y Unity**. No modifica el método histórico A1-bis `73d86a2` ni sus datos.

## Regla exacta

Solo FondoAi del Rival. Se conserva T*, plan, geometría y extracción gaussiana original `g~N(0,sigma)` en el mismo punto de la secuencia aleatoria. Fácil/Media/Difícil conservan sigma=55/38/22 ms. Sin extracciones nuevas ni cambios de opción, aim, power o ruido de postura.

Ventana `[L,U]` en ms **reales corregidos** respecto de T*: `L=(KIn−T*)×1000/(StepHz×Tempo)` y análogamente U. Si ValidLimit está activo, intersectar con `[−ValidMs,+ValidMs]`. LatencyComp no añade margen: Resolve lo suma y el contrato lo resta. No ampliar K/V ni cambiar EarlySoft. En planes utilizables, cero pertenece a la ventana.

`m=FondoReleaseMarginSigma×sigma`, campo nuevo de RallyParams con **default 1,5**, fuera de DefaultPreset/catálogo.

1. Si `L≤−m` y `U≥m`, devolver **el mismo double g**, sin sumarle cero ni reconstruirlo.
2. Si `U−L<2m` (menor que 3sigma), media `mu=(L+U)/2`.
3. En otro caso, `mu=clamp(0,L+m,U−m)`, desplazamiento mínimo para quedar a 1,5sigma del borde que aprieta. Igualdad cuenta como margen suficiente; ancho exactamente 3sigma no es ventana corta.
4. Si mu=0 devolver g; de otro modo g+mu. No recortar muestras ni reducir sigma: conserva colas.

Sin plan utilizable, se conserva la rama original sin swing. Parámetro 0 solo como referencia de la política anterior. No cambiar ContactPlanner, ReceptionPlanner, ContactContract, ClockMap ni Humano/Touch/Bot.

## Criterios inmutables tras este commit

| ID | Criterio y denominador |
|---|---|
| A1-C1 | Para ambos márgenes ≥1,5sigma: DeltaMs idéntico **bit a bit**, incluidas igualdad de bordes y cero con signo; aim/P/opción y estado posterior del RNG iguales. |
| A1-C2 | Población A1-bis: contacto esperado total sobre los **103 vuelos legales** en orden estricto Fácil < Media < Difícil. Informar también tasa entre planes. |
| A1-C3 | Humano intacto: diff vacío en Touch/Bot/contrato; test Bot con misma observación/plan/semilla y parámetro 0 frente a 1,5, decisiones idénticas bit a bit. Suites existentes sin actualizar expectativas. |
| A1-C4 | Rival medio P=1: contacto esperado **sobre sus 29 planes utilizables ≥90 %** (32 vuelos legales, 36 entradas). No excluir stretched ni cambiar denominador. |
| A1-C5 | Sin azar nuevo: mismas extracciones y orden. Comparar parámetro 0/1,5, mismas entradas/semillas: salvo DeltaMs desplazado, decisiones y ruido de postura posterior iguales. Dos procesos experimentales deterministas. |
| A1-C6 | DefaultPreset/P0/SimulationKey y cuatro Scenario.ExpectedHash intactos. Medir hash propio nuevo de RallyParams en Unity; su proyección excluyendo el campo nuevo debe ser `7d50ddaf5d4f`. |
| A1-C7 | Tests existentes sin cambio de veredicto ni de aserciones, especialmente rally. EditMode/PlayMode completos, AutoQA V y T3 **PASS/0/1**, check-release dev/prod **OK**. Si falla un test existente: detenerse sin corregirlo ni actualizar expectativas. |

Los números descriptivos de las simulaciones de rally pueden cambiar por esta política y se informan; no se exige la misma trayectoria con una política diferente. La parada se refiere al veredicto de los tests y a sus aserciones intactas.

## TDD, experimento y gates predeclarados

- Esqueleto compilable que devuelve g; tests nuevos de borde temprano/tardío y ventana corta con RED válido por aserción, usando `scripts/test.sh editmode <filtro>`. Identidad segura, sigma/azar y Humano desde el comienzo. Implementar solo después del RED y repetir el filtro GREEN sin cambiar tests.
- EditMode completo antes de PlayMode; ante fallo existente detenerse. PlayMode completo y `git checkout -- ProjectSettings` al terminar, incluso si falla. Experimento aislado después de suites; AutoQA V/T3 y check-release después de experimento aprobado. No más puertas tras un fallo.
- Experimento propio (sin editar el histórico): mismos **108 golpes, 648 planes, entradas y semillas de A1-bis**, con código real. Comparar los campos nombrados congelados de #72; no reejecutar fase A. Hash de RallyParams nuevo, demás configuración idéntica.
- Por plan, registrar K/T*/sigma, g, mu, release nuevo, Contact/Air/Late y Perfect/Valid por Resolve real. Mu sale de la misma función de producto usada por FondoAi. Analítica: integrar celdas del contrato con N(mu,sigma), sin renormalizar ni descartar categorías.
- 200 muestras por plan del Rival, `RallyRng(20261117+índice)`, sigma original y mu determinista. Misma comprobación estadística de A1-bis: por categoría diferencia ≤max(0,025;5sqrt(p(1−p)/200)); agregado por dificultad ≤0,005+5sqrt(sum(200p(1−p)))/(200n). Sin ajustes posteriores.
- Comprobar C1–C6 sobre todas las filas aplicables. Reportar P/vidrio, planned/stretched, no evaluados y los cinco out separados de los 103 legales. Bot/Humano: delta=0 y ventana original, sin modelar ruido de usuarios.
- Dos procesos sobre mismo head limpio; cuatro archivos crudos y derivados sin timestamps, SHA-256 iguales. XML fresco, tests>0, sin CS. Inventario PID/comando/proyecto antes de **cada** Unity; si existe Editor no lanzar ni cerrarlo. Preservar RED/fallos/gates separados.

La autonomía excepcional previa a datos sigue limitada a código experimental G1 y scripts diagnose-g1*/summarize-g1*. **No permite reparar producto por cuenta propia**, cambiar estos criterios ni corregir un test existente que falle. Defectos posteriores a datos o sorpresas se registran y detienen. Informe de una página y PR propia, sin fusión, instalación, build nativa ni acceso al iPhone. A2 va por separado: solo diseño sin Unity y parada para revisión de Claude.
