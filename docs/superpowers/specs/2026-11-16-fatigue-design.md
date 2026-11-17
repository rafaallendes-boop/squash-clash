# Cansancio, primera versión: diseño (v2.3)

16 nov 2026. Diseño conversado entre el PO y Claude. Parte de la spec v0.3: §9.4, «El cansancio es drama, no castigo», y «las mismas reglas para la IA y para el humano».

- **v1:** commit `e1bcef7`.
- **v2:** incorpora la revisión del PO: el enfoque general se aprueba, con ajustes antes de autorizar la implementación.
- **v2.1:** tres correcciones del PO: la instantánea del brazo es del que golpea (§4), B2 mide el consumo por desplazamiento y B5b pasa a ser exploratorio (§7). Con ellas, el diseño funcional y los cambios protegidos quedan **autorizados** (§9).
- **v2.2 (17 nov):** el PO aprueba reformular A2 (§7). Las claves `fatigue.*` cambian por fuerza las cuatro anclas de hash de AutoQA; se autoriza actualizar solo esas anclas, con una comprobación de proyección. AutoQA se corre con `--suite v` y `--suite t3`, y la barra no altera las métricas de botones y pausa de U3 (§6, §8).

- **v2.3 (17 nov):** el PO autoriza actualizar solo literales de hash/cantidad derivados de DefaultPreset en los tests enumerados por F1-PREFLIGHT-01 (incluido AutoQAT3OracleTests), a los valores medidos en Unity y 132 claves, en commit propio. Ninguna otra aserción, oráculo o test cambia. Se exige prueba de proyección excluyendo solo `fatigue.*` a las cuatro anclas históricas y SimulationKey `166a96e319c7`; si cambia, se detiene. F2 no agrega claves. El saque automático tiene P de intención = 0 únicamente para consumo por golpe fuerte, sin cambiar trayectoria, física ni solver; su desplazamiento se cobra.

**Estado:** diseño y cambios protegidos autorizados por el PO (§9). F1 implementado en su rama, sin fusionar ni aceptar; F2 espera la fusión de F1.

## 1. Para qué

El cansancio debe **generar decisiones tácticas**: cerrar el punto pronto o hacer correr al rival. No debe regalar puntos ni quitarle diversión al arcade. Hoy el personaje corre solo. Cansado, corre más lento, reacciona más tarde y su golpe sufre más cuando llega mal.

En palabras del PO: «a medida que se va cansando, va a ir yendo más lento, con menos reacción y quizás con menos precisión. […] Yo intentaría o terminar el punto más rápido, eh, o tratar de hacer correr al rival para que él también se canse.» Sobre golpear fuerte: «si uno juega fuerte todo el partido, uno se va cansando más rápido […] que sea poco».

En la revisión de la v1, el PO valoró que el cansancio sea determinista, igual para el humano y la IA, y que amplifique los errores por mala posición en vez de crear errores arbitrarios.

**Objetivos:**
- La energía baja por correr y, un poco, por golpear fuerte.
- Se recupera en parte entre puntos, así que el final del partido se juega más cansado.
- Los efectos se perciben, sin frustrar.
- Las reglas son las mismas para el humano y la IA.
- Todo se ajusta desde el panel DEV.

**No objetivos de esta versión:**
- Golpe al marco y pelota fallada (§9.4, punto 4).
- Animaciones de cansancio.
- Que la IA decida según el cansancio.
- Atributos de jugador, que llegarán con los prototipos.
- Arrastrar el cansancio entre partidos.

**Antiobjetivos:**
- **Puntos regalados:** que el cansancio decida puntos sin una causa que se entienda.
- **Azar:** cualquier componente aleatorio.
- **Golpe imposible:** que un jugador cansado pero bien ubicado no pueda hacer un golpe excelente.
- **Ventaja:** que favorezca al humano o a la IA.
- **Práctica alterada:** que cambie el modo práctica, los escenarios protegidos o AutoQA.
- **Retraso:** que demore First Rally, First Match o las pruebas con usuarios.

## 2. Enfoques considerados

| Enfoque | Qué cambia | Decisión |
|---|---|---|
| **A. Integrado al juego, igual para ambos** | Energía por jugador; los efectos se aplican en el movimiento automático y en la capa causal; barra en el marcador; números en el panel DEV | **Elegido por el PO** |
| B. Primero solo la barra | Se muestra y se mide, sin efectos | No prueba la decisión de juego |
| C. Ruido extra en la IA | Cansancio simulado solo en el rival | Contradice «mismas reglas» y la §11.5 |

## 3. Energía: consumo y recuperación

Cada jugador tiene `E` entre 0 y 100. Empieza en 100 en cada partido. En el modo Rally empieza en 100 al entrar desde el menú.

**Consumo, solo mientras el punto está en juego (del saque al cierre):**
- **Correr:** `fatigue.costPerMeter` × la distancia recorrida por el cuerpo. Es la suma de |ΔPosición| en cada paso de simulación, incluida la vuelta a la T.
- **Golpear fuerte:** en el paso de contacto, si la P de la intención supera `fatigue.strongFrom`, se gasta `fatigue.strongCost` × (P − strongFrom)/(1 − strongFrom). La P de la intención es la del gesto M2-4 del humano o la elegida por la IA, antes de la curva de ritmo por distancia. El saque cuenta; el saque automático usa **P de intención = 0** para este consumo, sin modificar su trayectoria, física ni solver. Su desplazamiento se cobra normalmente.

**Recuperación:**
- **Entre puntos:** una sola vez, cuando el punto se cierra, `E += fatigue.recoverBetween × (100 − E)`.
- **Durante el rally:** `fatigue.recoverPerS × dt` en cada paso en que el jugador está **quieto**, según esta definición:
  - la fase del cuerpo es `Waiting` o `Set`;
  - `Speed < 0,05 m/s`;
  - no está en retraso de reacción (`Reacting == false`).

  Basta que una de las tres no se cumpla para que no recupere.

`E` siempre queda entre 0 y 100. Todo ocurre en el mismo reloj de simulación que la pelota y los cuerpos, sin tiempo real.

## 4. Efectos

**Factor de cansancio.** `f = clamp01((fatigue.onset − E) / fatigue.onset)`. Vale 0 mientras `E ≥ onset` y llega a 1 cuando `E = 0`.

**Instantánea por recepción.** El factor que gobierna una recepción se calcula **una vez**, con la `E` del receptor en el paso de contacto del golpe que debe recibir (el saque, para quien recibe el saque). Esa instantánea, `f_r`, vale para toda esa recepción, incluidas las replanificaciones que haga `RallyDirector`. Cambia recién con el siguiente golpe que deba recibir ese jugador. La energía se sigue descontando paso a paso, pero el efecto nuevo entra en la siguiente recepción.

**Piernas y reacción.** Una sola función compartida, `FatigueEffects.For(f_r, params)`, devuelve:
- un factor de movimiento `m = 1 − fatigue.legsMax · f_r`, que multiplica la aceleración y la velocidad máxima del `MoveSettings` del cuerpo durante esa recepción. El frenado no cambia.
- pasos extra de reacción, `extra = round(fatigue.reactionMaxMs · f_r / 1000 · StepHz)`.

**Cómo se incorporan, idéntico en los tres lugares** (humano en `RallyDirector`, rival en `RallyDirector` y `RallySim`):
- Hoy cada lugar calcula un único `delay` y lo pasa a `ReceptionPlanner.Plan` y a `PlayerBody.MoveTo`. El cansancio **suma `extra` a ese mismo `delay`** antes de las dos llamadas.
- En el rival se suma antes de restar `elapsed` y se conserva el `Math.Max(0, …)`.
- El factor `m` se fija en el cuerpo **antes** de planificar. `StepsToReach` y `PositionAfter`, que simulan una copia, heredan ese factor. Así, la predicción y la ejecución usan el mismo código y los mismos valores, como ya exige `PlayerBody`.
- **Prohibido:** calcular el factor por separado en cada lugar o aplicarlo solo en la ejecución.

**Brazo y precisión: la instantánea es del que golpea.** El factor que modifica la potencia y los vectores causales es el del **jugador que ejecuta el contacto**, nunca el del receptor:
- Se calcula como `f_s = f(E)` con la `E` del golpeador en **su** paso de contacto, **antes** de descontar el gasto por golpe fuerte de ese mismo golpe.
- Una sola función compartida calcula `f_s` y la aplican igual el humano (`CausalQuality.Solve`, vía `RallyDirector`), la IA y `RallySim` (`RallyContact.Solve`).
- `f_r` (piernas y reacción del receptor) y `f_s` (brazo del golpeador) son instantáneas distintas y no se mezclan.

- **Potencia:** un vector causal determinista nuevo, «fatigue», reduce la velocidad del golpe en `fatigue.armSpeedMax · f_s`, como fracción. No toca la mira.
- **Mala posición:** los vectores causales **de posición** (estirarse y quedar trabado) se multiplican por `(1 + fatigue.positionGainMax · f_s)`. Los vectores de **timing** no se amplifican, porque el timing ya es responsabilidad del jugador.
- **Garantía:** con buena posición, los vectores de posición valen 0 y la amplificación no suma nada. Un jugador cansado pero bien ubicado y a tiempo conserva su golpe, con la mira intacta; solo pierde un poco de velocidad, que puede compensar tirando más.
- **Sin aleatoriedad** de ningún tipo.

**Dónde aplica.**
- Aplica en los modos Rally y Partido.
- En el modo práctica, en los lanzamientos SL, en AutoQA y en los escenarios protegidos, `f` vale siempre 0 y la energía no se calcula.
- `fatigue.enabled = 0` apaga todo, con un comportamiento idéntico al anterior.

## 5. Valores: parten suaves

Los máximos de la v1 quedan como el **nivel fuerte** de referencia. La implementación **arranca con el nivel suave**. Todos los valores se ajustan desde el panel DEV, dentro de su rango.

| Clave | Suave (default) | Fuerte (v1) | Rango DEV | Intención |
|---|---|---|---|---|
| `fatigue.enabled` | 1 | 1 | 0–1 | apagado general |
| `fatigue.costPerMeter` | 0,4 | 0,4 | 0–2 | un rally largo, unos 40 m, cuesta unos 16 |
| `fatigue.strongFrom` | 0,7 | 0,7 | 0,5–1 | desde qué potencia cansa el golpe |
| `fatigue.strongCost` | 1,0 | 1,5 | 0–5 | costo de un golpe al 100% |
| `fatigue.recoverBetween` | 0,35 | 0,35 | 0–1 | fracción de lo que falta, recuperada entre puntos |
| `fatigue.recoverPerS` | 0,5 | 0,5 | 0–5 | recuperación mínima, quieto |
| `fatigue.onset` | 70 | 70 | 10–100 | sobre este valor no se nota |
| `fatigue.legsMax` | 0,10 | 0,20 | 0–0,4 | pérdida de velocidad y aceleración con E = 0 |
| `fatigue.reactionMaxMs` | 40 | 80 | 0–200 | reacción extra con E = 0 |
| `fatigue.armSpeedMax` | 0,04 | 0,08 | 0–0,2 | pérdida de velocidad del golpe con E = 0 |
| `fatigue.positionGainMax` | 0,25 | 0,5 | 0–1 | amplificación del error por mala posición con E = 0 |

Las claves nuevas cambian el hash de los defaults, y eso es esperado.

**Aplicación de la regla §7 en F1 (17 nov):** la tabla conserva los valores iniciales. Tras B3 alto (80,29), el [único ajuste predeclarado](../../validation/2026-11-17-f1-fatigue-adjustment.md) cambia solo `recoverBetween` de 0,35 a 0,25; B3 queda en 70,68. No cambian rangos, método ni umbrales. El candidato F1 usa 0,25; los datos suaves iniciales se conservan.

## 6. Barra de energía

**Ubicación:** discreta, sin filas nuevas en la pantalla vertical. Es una línea fina, de 3 pt, bajo el nombre de cada jugador, dentro del marcador que ya existe.

**Color:**
- neutro mientras `E ≥ onset`, porque aún no se nota;
- ámbar bajo `onset`;
- rojo bajo 25.

**Sin números ni parpadeos.** Se anima de forma suave al cambiar.

**Métricas de U3 (v2.2):** la barra no recibe toques (`pickingMode = Ignore`), no agrega botones ni renombra los que existen. U3 registra el nombre de cada botón del HUD y el motivo de cada pausa como eventos de usuario, y esos eventos no cambian.

```
 TÚ  5          RIVAL  4
 ▔▔▔▔▔▔▔▔░░      ▔▔▔▔▔░░░░░
```

**Comprensión:** el tutorial y el menú no se tocan en esta versión. Si las pruebas con usuarios muestran que no se entiende, se agrega una línea al tutorial.

## 7. Criterios de aceptación (predeclarados, sin cambiarlos después)

**A. Contratos y reversibilidad** (tests; todos obligatorios):
1. **A1:** con `fatigue.enabled = 0`, los tests existentes de rally y partido pasan **sin cambiar sus expectativas** y los resultados con semillas fijas son idénticos bit a bit a los de `main`. Los tests que midan otra cosa fijan `fatigue.enabled = 0` de forma explícita.
2. **A2 (v2.2):** el modo práctica, los SL y AutoQA `--suite v` y `--suite t3` quedan PASS/0/1 con los mismos escenarios, oráculos, expectativas y checker. Se autoriza actualizar solo las cuatro anclas de `Scenario.ExpectedHash` a los hashes nuevos medidos en Unity, con la condición de que la proyección del hash sobre las 121 claves previas a F1 dé exactamente las anclas actuales (V `06fed4bd4ac9` y `8e22575422f2`; T3 `430da2faa818` y `e89daab41c3e`). La versión anterior pedía «los mismos hashes de escenario», lo que no se puede cumplir al agregar claves a `DefaultPreset.json` (§5).
3. **A3:** con cansancio activo, la predicción coincide con la ejecución. Para varias instantáneas `f_r` y los tres lugares, `StepsToReach` y `PositionAfter` coinciden paso a paso con el movimiento real.
4. **A4:** el humano y la IA, con la misma `E`, obtienen el mismo `m`, el mismo `extra` y la misma amplificación.
5. **A5:** jugador con E = 0, bien ubicado (vectores de posición nulos) y con timing perfecto: la mira queda intacta y la velocidad solo cae `armSpeedMax`. No hay ningún otro vector.
6. **A6:** con la misma semilla y la misma entrada, el resultado es el mismo. No hay azar nuevo.
7. **A7:** el brazo usa la instantánea del golpeador. Si se cambia solo la energía del receptor, la potencia y los vectores del golpe no cambian. Si se cambia solo la del golpeador, sí cambian, igual en el humano, la IA y `RallySim`.

**B. Simulación de partidos completos** (experimento separado de la regresión, `RallySim`, 200 partidos a 7 puntos por diferencia de 2, IA Media contra el Bot, valores suaves):
1. **B1, rallies cortos (≤ 4 golpes):** cuestan como mediana ≤ 4 de energía por jugador. Apenas se notan.
2. **B2, rallies largos (≥ 15 golpes), con el consumo atribuible al desplazamiento** (`costPerMeter` × metros de ese rally, sin contar golpes ni recuperación, e independiente de la energía con que empezó cada uno):
   - el jugador que más corrió consume por desplazamiento, en mediana, ≥ 8 puntos;
   - la mediana de la diferencia de consumo por desplazamiento entre ambos es ≥ 4 puntos.

   Hacer correr al rival le cuesta de forma apreciable.
3. **B3, progresión:** la mediana de E al empezar el último punto queda entre 45 y 75, y en al menos el 50% de los partidos algún jugador baja de `onset`. El cansancio se percibe.
4. **B4, sin agotarse:** en ≤ 5% de los partidos algún jugador empieza un punto con E < 20, y nadie saca con E = 0.
5. **B5, finales ajustados (desde 6-6, incluidas las extensiones):**
   - **B5a, compuerta:** la mediana de E al empezar cada punto es ≥ 35.
   - **B5b, indicador exploratorio, no compuerta:** se reporta, sin umbral de aprobación. La muestra puede ser chica y las trayectorias divergen entre escenarios. En los puntos con una diferencia de energía ≥ 20, el porcentaje de puntos ganados por el jugador más cansado baja **entre 3 y 15 puntos porcentuales** respecto del mismo jugador en el mismo tramo sin cansancio, con las mismas semillas. Se compara así porque la IA y el Bot no son simétricos. Referencia esperada: el cansancio pesa, pero no decide. El informe incluye el tamaño de la muestra.
6. **B6, ritmo arcade:** la duración media de los rallies con cansancio queda dentro del ±15% de la duración sin cansancio, con las mismas semillas.

**Regla de parada** (la prioridad es avanzar, no calibrar):
- Si B3, B4 o B5a fallan, se permite **un solo ajuste**, de `recoverBetween` o `costPerMeter`, registrado con su motivo.
- Si después siguen fallando, se reporta al PO y se pasa a jugar con los valores más cercanos. No se cambian los criterios.

**C. Jugabilidad en el iPhone** (el PO, con la build DEV; observación sin umbral):
- **Prueba:** 3 partidos completos.
- **Preguntas:**
  - ¿El cansancio se nota en los rallies largos?
  - ¿Alguna vez sentiste que perdiste un punto «por el cansancio» sin entender por qué?
  - ¿Cambió cómo jugaste, por ejemplo cerrar antes o hacer correr al rival?
  - ¿La barra se entiende sin explicación?
  - ¿Un final ajustado se sintió más tenso o más frustrante?
- **Después:** una ronda de ajuste por juego desde el panel DEV, y luego a usuarios.

## 8. Implementación: incremento pequeño y reversible (Codex)

Son dos PRs pequeñas. Cada una se revierte sola y queda apagable con `fatigue.enabled`.

1. **F1, energía y piernas:**
   - modelo de energía (§3), instantánea por recepción, piernas y reacción (§4) en los tres lugares;
   - métricas: un evento `fatigue` por punto (E inicial y final de cada jugador, metros, gasto por golpes fuertes, recuperación) y `f_r` en cada `shot`;
   - criterios A1–A4 y A6, y el experimento B.
2. **F2, brazo y barra:** el vector causal «fatigue», la amplificación de los vectores de posición, la barra (§6) y los criterios A5 y A7. Repite A1, A2 y A6, y el experimento B con todo activo.

**Compuertas de cada PR:**
- TDD con RED por aserción;
- EditMode y PlayMode completos;
- AutoQA `scripts/autoqa.sh mvp --suite v` y `--suite t3`, ambos PASS/0/1, porque F2 toca la capa causal y el contrato del golpe, y F1 lo corre igual para demostrar A2;
- si cambian los hashes, solo se actualizan las cuatro anclas de `Scenario.ExpectedHash`, con la comprobación de proyección de A2, en un commit propio;
- `check-release.sh` OK;
- los hashes nuevos en AGENTS §5;
- un informe de una página.

**Si surge una sorpresa:** cualquier test o escenario que necesite otro tratamiento se detiene y se reporta.

## 9. Autorizaciones y cola

**Autorizado por el PO (16 nov), con las correcciones de la v2.1:**
- el diseño funcional;
- las claves de cansancio en `DefaultPreset.json`;
- modificaciones controladas del movimiento del cuerpo y su planificación;
- el vector causal de cansancio en la capa del golpe, sin alterar los contratos protegidos existentes.

**Autorizado por el PO (17 nov), v2.2:** la reformulación de A2 (§7). En AutoQA solo se actualizan las cuatro anclas de `Scenario.ExpectedHash`; escenarios, oráculos, expectativas y checker no cambian.

El texto literal está en el [registro de aprobaciones](../../reviews/2026-10-25-first-shot-po-approvals.md).

**Cola:** después de U1–U3 y G1. F1 y F2 son PRs independientes. No bloquea First Rally, First Match ni las primeras pruebas con usuarios.
