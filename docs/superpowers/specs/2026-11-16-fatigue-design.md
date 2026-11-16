# Cansancio, primera versión: diseño

16 nov 2026. Diseño conversado entre el PO y Claude. Parte de la spec v0.3 (§9.4, «El cansancio es drama, no castigo», y la regla «las mismas reglas para la IA y para el humano»). Está pendiente de la aprobación del PO, y nada se implementa antes de esa aprobación.

## 1. Para qué

Que los rallies largos importen y obliguen a decidir. Hoy el personaje corre solo. Con el cansancio va más lento, reacciona más tarde y pierde algo de precisión. El jugador elige entre **cerrar el punto pronto** o **hacer correr al rival** para cansarlo también.

En palabras del PO: «a medida que se va cansando, va a ir yendo más lento, con menos reacción y quizás con menos precisión. […] Yo intentaría o terminar el punto más rápido, eh, o tratar de hacer correr al rival para que él también se canse.» Sobre el golpe fuerte: «si uno juega fuerte todo el partido, uno se va cansando más rápido […] que sea poco».

**Objetivos:**
- La energía baja por correr y, un poco, por golpear fuerte.
- Se recupera solo en parte entre puntos, así que el final del partido se juega más cansado.
- Los efectos se notan y se entienden.
- Las reglas son las mismas para el humano y la IA.
- Todo se ajusta desde el panel DEV para afinarlo jugando.

**No objetivos de esta versión:**
- Golpe al marco y pelota fallada (§9.4, punto 4).
- Animaciones de cansancio, como la respiración o el cuerpo encorvado.
- Que la IA decida según el cansancio.
- Atributos de jugador, que llegarán con los prototipos de jugadores.
- Arrastrar el cansancio entre partidos.

**Antiobjetivos, que serían un fracaso aunque funcionen:**
- **Puntos regalados:** que el cansancio decida puntos por sí solo, sin que la causa se entienda.
- **Azar:** que meta aleatoriedad. Los efectos son deterministas.
- **Ventaja:** que favorezca al humano o a la IA.
- **Práctica alterada:** que cambie el modo práctica o los escenarios de AutoQA.

## 2. Enfoques considerados

| Enfoque | Qué cambia | Por qué se descarta o se elige |
|---|---|---|
| **A. Integrado al juego, igual para ambos** | Energía por jugador; los efectos se aplican en el movimiento automático y en la capa causal; barra en el marcador; números en el panel DEV | **Elegido por el PO**: es el único que prueba la decisión de juego desde el primer día |
| B. Primero solo la barra | Se muestra y se mide, sin efectos | No cambia el juego ni prueba la decisión |
| C. Ruido extra en la IA | Simula cansancio solo en el rival | Contradice «mismas reglas» y la §11.5, que impide que la dificultad toque atributos |

## 3. Diseño

**Energía.** Cada jugador tiene `E` entre 0 y 100. Empieza en 100 en cada partido y en cada sesión de Rally.

**Consumo, solo durante el punto:**
- **Correr:** `fatigue.costPerMeter` × metros recorridos por el cuerpo (`PlayerBody`, en el mismo reloj de simulación).
- **Golpear fuerte:** si la potencia efectiva `P` del golpe supera `fatigue.strongFrom`, se gasta `fatigue.strongCost` × (P − strongFrom)/(1 − strongFrom). Cuenta igual para el humano (P del gesto M2-4) y para la IA (P de su intención).

**Recuperación:**
- **Entre puntos:** se recupera `fatigue.recoverBetween` × (100 − E), una fracción de lo que falta. Así la recuperación es parcial y el final del partido llega más cansado.
- **Durante el rally:** `fatigue.recoverPerS` por segundo mientras el cuerpo espera quieto. Es mínima.

**Efectos.** Se usa un factor `f = clamp01((fatigue.onset − E)/fatigue.onset)`: es 0 mientras `E ≥ onset` y llega a 1 cuando `E = 0`.
- **Piernas:** aceleración y velocidad máxima × (1 − `fatigue.legsMax`·f), y retraso de reacción + `fatigue.reactionMaxMs`·f.
- **Brazo:** un vector causal nuevo, «fatigue». Reduce la velocidad del golpe en `fatigue.armSpeedMax`·f, como fracción, y **amplifica** los vectores causales de posición (estirarse y quedar trabado) por (1 + `fatigue.positionGainMax`·f). Así el cansancio pesa más justo cuando se llega mal, que es la cadena que describe la §9.4. No suma azar.

**Coherencia entre predicción y ejecución.** `PlayerBody` exige que la predicción y la ejecución no diverjan.
- El factor de piernas se **congela** cuando se planifica cada recepción (el `MoveTo`), y la predicción usa ese mismo valor.
- La energía se descuenta paso a paso, pero el efecto nuevo entra en la siguiente planificación.
- Lo mismo vale para el humano, la IA y `RallySim`.

**Dónde aplica.**
- Aplica en los modos Rally y Partido.
- En el modo práctica, en los lanzamientos SL y en AutoQA la energía queda fija en 100 (f = 0). El comportamiento es idéntico al actual.
- `fatigue.enabled` en 0 apaga todo.

**Interfaz.** El marcador del partido muestra una barra fina por jugador, con el color de cada uno, junto a los puntos:

```
 TÚ  5 ▕██████████░░░░▏      RIVAL  4 ▕███████░░░░░░░▏
```

**Métricas** (para afinar):
- un evento `fatigue` por punto, con la energía de cada jugador al empezar y al terminar, los metros, el gasto por golpes fuertes y la recuperación;
- el valor de f en cada `shot`.

## 4. Parámetros (claves nuevas en `DefaultPreset.json`, ajustables desde el panel DEV)

Valores iniciales propuestos por Claude para empezar a jugar. Se espera ajustarlos.

| Clave | Inicial | Intención |
|---|---|---|
| `fatigue.enabled` | 1 | apagado general |
| `fatigue.costPerMeter` | 0,4 | un rally largo, unos 40 m, cuesta unos 16 |
| `fatigue.strongFrom` | 0,7 | desde qué potencia cansa el golpe |
| `fatigue.strongCost` | 1,5 | un golpe al 100% cuesta 1,5 |
| `fatigue.recoverBetween` | 0,35 | entre puntos se recupera el 35% de lo que falta |
| `fatigue.recoverPerS` | 0,5 | recuperación mínima esperando quieto |
| `fatigue.onset` | 70 | sobre 70 no se nota nada |
| `fatigue.legsMax` | 0,20 | con E = 0, un 20% menos de velocidad y aceleración |
| `fatigue.reactionMaxMs` | 80 | con E = 0, 80 ms más de reacción |
| `fatigue.armSpeedMax` | 0,08 | con E = 0, el golpe sale un 8% más lento |
| `fatigue.positionGainMax` | 0,5 | con E = 0, los errores por mala posición pesan un 50% más |

Las claves nuevas cambian el hash de los defaults, que es esperado. Con `fatigue.enabled = 0`, el juego debe comportarse exactamente como sin la función.

## 5. Lo que queda para quien lo construya (Codex)

- **TDD por pieza:** consumo, recuperación, factor f, piernas con predicción congelada, vector causal, HUD, métricas e inmunidad del modo práctica. Las suites van completas.
- **Tests existentes de rally** (por ejemplo `RallySimTests` con semillas fijas) que midan otras cosas: fijan `fatigue.enabled = 0` de forma explícita, sin cambiar sus expectativas. La función se prueba con tests nuevos. Si algún test o escenario necesita otro tratamiento, se detiene y reporta.
- **AutoQA** `mvp` PASS/0/1. Es obligatorio, porque se toca la capa causal y el contrato del golpe. Debe quedar idéntico, porque usa práctica.
- **Decisiones menores** de implementación, como los nombres o dónde se guarda la energía. Las declara en el informe.

## 6. Decisiones que tomó Claude (el PO puede corregirlas)

- **Amplificación en vez de error propio:** que el cansancio amplifique los errores por mala posición en vez de agregar uno independiente. Es la cadena de la §9.4 y evita el azar.
- **Factor congelado:** el factor de piernas se congela al planificar cada recepción, para no romper la predicción.
- **Energía en 100 en práctica y AutoQA.**
- **Valores iniciales** de la §4.
- **Barra simple** en el marcador, sin animaciones todavía.
- **Modo Rally:** la energía vuelve a 100 al entrar al modo desde el menú, y entre puntos se recupera igual que en un partido.
- **Qué P cuenta:** la P de la intención, es decir, la del gesto del humano o la elegida por la IA, antes de la curva de ritmo por distancia. El saque cuenta como cualquier golpe.
- **Frenado:** las piernas cansadas reducen la aceleración y la velocidad máxima, no el frenado.
- **La IA no ve las barras** en esta versión. Usarlas para decidir queda para después.

## 7. Autorización necesaria

Para encargarlo hace falta:
- la aprobación de este diseño;
- una autorización explícita para tocar la capa causal del golpe, que está junto al contrato de contacto, y para agregar claves a `DefaultPreset.json`.

Va en la cola de Codex después de U1–U3 y G1, salvo que el PO decida otro orden.
