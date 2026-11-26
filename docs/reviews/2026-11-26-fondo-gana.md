# Por qué pegarle al fondo gana tanto

26 nov 2026 · Claude (Opus) · diagnóstico en solo lectura sobre `main` `8a32d42` (build 1230). No cambia código de producto, specs, umbrales ni pruebas. Para medir se usó un experimento temporal en `Tests/Experiments/EditMode`, que se borró al terminar. Solo queda este informe, sin commit. Evidencia local, ignorada por git: `Logs/experiments/deep-win/` (`run01`, `phaseA01`, `phaseA02`, `phaseB01`, `po1230`, cada una con `source.tsv`) y `Logs/evidence/metrics-1230-20261126/`. Todas las corridas dieron Passed con defaults `207da8f2c636` y RallyParams `ebf8f717f8c5`.

## Conclusión

El drive al fondo gana ~95 % de los puntos en los **tres** niveles. Lo hace con los tres estilos, con P = 1 o con P = 0,85 y con la guardia «Drive adelante» o sin ella. El mecanismo es un circuito cerrado:

1. **El rival no alcanza la pelota a tiempo.**
2. **Por eso entra en la defensa obligatoria.**
3. **Su golpe de defensa es corto y suelto.**
4. **El humano vuelve a pegar al fondo con tiempo de sobra.**

Ninguno de los tres ajustes pedidos rompe ese circuito sin crear algo nuevo.

## 1. La respuesta del rival a la pelota profunda (código)

- **Espera y recuperación:** el rival espera en T + `WaitBehindT`: Fondista 0,5 m, Definidor 0 y Completo 0,2 (`RivalAi.cs:88`, `RallyParams.cs:24,86,95`). Después de su golpe vuelve allí, porque la cobertura solo actúa tras su propio golpe corto (`RivalAi.cs:98`).
- **Reacción:**
  - Tiene `ReactionS` 0,18 s más la percepción del nivel por la k del estilo; en Medio, Fondista suma 0,04 × 1,75 (`RallySim.cs:200`, `RallyParams.cs:17,51,92`).
  - Los cuerpos son iguales para los dos: 4,8 m/s, 12 m/s² de aceleración y 16 m/s² de frenado (`RallyParams.cs:17`). El alcance es de 1,3 m (`body.reach`).
- **Alcance y holgura:**
  - Si no llega a tiempo, el planificador lo manda «estirado», con holgura 0 (`ReceptionPlanner.cs:121`).
  - En esa situación el golpe sale +0,4 m más alto y un 20 % más lento (`CausalQuality.cs:211`; `causal.stretch*`).
- **Calidad de la devolución:**
  - Con holgura menor que `DefenseSlackS` solo se permiten opciones altas y de potencia media, o dos paredes (`RivalAi.cs:118,126`; spec §11.3, «alta y larga»).
  - Ese globo, `AimHigh` 3 m a P 0,5 (`RallyParams.cs:59`), **bota a 4,9–5,4 m de mediana**, delante de la línea de saque o encima de ella. No es largo.
- **Volea:** la regla de defensa se calcula con la holgura **del bote** (`RivalAi.cs:118`) y también filtra las opciones de volea (`RivalAi.cs:126`). La volea que el rival saca a un drive al fondo es 100 % globo defensivo.

## 2. Simulación (P2, RallySim, sin escena)

El humano es un bot con intención fija y σ = 30 ms (o 110 ms) contra el `RivalAi` real. Se jugaron 150 partidos por celda, unos 3.300 puntos por fila. La guardia «Drive adelante» se emuló con `FrontDeepDrive.Apply` antes de soltar; es una aproximación, porque RallySim no la aplica.

| Humano gana (%) | Fácil | **Medio** | Difícil |
|---|---|---|---|
| Drive al fondo, P = 1 | 95,8 | **95,3** | 95,3 |
| Drive al fondo, P = 0,85 (= «fuerte» del rival) | 95,4 | **94,4** | 94,5 |
| Ídem con la guardia emulada | 95,9 | **95,4** | 95,2 |
| Drive al fondo, P = 1, σ 110 ms | 78,7 | **70,5** | 54,1 |
| Alto y medio (bota corto) | 50,4 | **26,7** | 8,9 |
| Medio suave | 52,3 | **29,5** | 9,6 |
| Drop | 54,8 | **42,1** | 24,8 |

En Medio, con todos los golpes juntos, agrupados por dónde bota la pelota del humano:

| Bote del golpe | n | Rival estirado | Defensa obligatoria (de las devueltas) | Holgura del humano después | Punto del humano |
|---|---|---|---|---|---|
| < 5,49 m | 85.012 | 18 % | 23 % | 0,17 s | 36 % |
| 7–8,5 m | 39.417 | 79 % | 98 % | 0,85 s | 97 % |
| > 8,5 m | 24.632 | 83 % | 100 % | 0,83 s | 97 % |

Contrafactuales en Medio, con el drive a P = 1:

| Variante | Humano gana |
|---|---|
| Base | 95,3 % |
| Rival esperando 0,75 m más atrás | 95,9 % (y el drop sube de 42 a 65 %) |
| Cuerpos a 5,5 m/s | 97,7 % |

No son ni la potencia 1,0 ni la guardia.

## 3. Cómo se gana: (a), (b) y (c), desde los rally-point

Las definiciones vienen de la orden de SC-PM y SC-Lead:

- **(a)** El rival no llega al drive al fondo: `Unreachable` o `NoSwing`.
- **(b)** El rival devuelve el drive al fondo y el humano gana con el golpe siguiente.
- **(c)** El rival llega al drive al fondo y falla: `Air`, `Late` o golpe fuera.

«Al fondo» significa que el golpe bota a 7 m o más, o pega en el vidrio al vuelo.

El playtest del 25 nov (build 1230) está medido en la §3b. Aquí van las sesiones del 23–24 nov (`f306d70` y anteriores), **sin** el tope de largo de 8,5 m de #120.

**Real, Medio, 23–24 nov:**

- Hubo 76 puntos de partido; el humano ganó 29.
- Los 116 golpes del humano se reparten así: largos 47 %, medios 15 %, cortos 16 % y lateral antes del bote 23 %.
- De los 29 puntos ganados, 15 salen de un drive al fondo:

  | Tipo | Puntos | Proporción de los 29 |
  |---|---|---|
  | (a) | 3 | 10 % |
  | (b) | 10 | 34 % |
  | (c) | 2 | 7 % |

- Los 14 restantes no tienen un drive al fondo justo antes.
- En todos los ganados, el rival «no llega» en 24 de 29, falla en el aire en 3, llega tarde en 1 y tira fuera en 1.
- La muestra es chica.

**Simulación, Medio, sobre los puntos que gana el humano:**

| Golpe del humano | (a) | (b) | (c) |
|---|---|---|---|
| P = 1 | 56 % | 4 % | 39 % |
| P = 0,85 | 7 % | 28 % | 59 % |

El rival casi nunca comete faltas de golpe: de 0,1 % a 0,8 %.

**Lectura:**

- Antes de #120, 23 de 126 golpes reales iban al vidrio al vuelo y 40 a la lateral. En esos casos el rival devolvía casi siempre.
- #120 convierte esos golpes en botes dentro de 8,5 m, que en la simulación son el grupo que gana el 97 % de los puntos.
- **Hipótesis:** por eso el PO lo notó recién en la 1230. La §3b no la confirma.

## 3b. Playtest del 25 nov (build 1230), medido

Métricas bajadas el 26 nov (verificación en «Límites»). Método idéntico al del 23–24: los golpes aplicados del humano, sacados de las líneas `shot`, se reproducen en P2 (`Logs/experiments/deep-win/po1230`, Passed, mismos hashes). Mismas definiciones de (a), (b), (c) y de «al fondo».

**Nivel Medio, rival Definidor, zurdo.** Un partido de 8 puntos; el humano ganó 7–1 en 91 s. **La muestra es muy chica: no sirve para proporciones, solo para ver el patrón.**

| | 23–24 nov (antes de #120) | 25 nov (1230) |
|---|---|---|
| Puntos, ganados por el humano | 76, 29 (38 %) | 8, 7 (88 %) |
| Golpes del humano | 116 | 17 (15 con P = 1) |
| Largos (≥ 7 m o vidrio al vuelo) | 47 % | 29 % (5) |
| Medios (5,49–7 m) | 15 % | 24 % (4) |
| Cortos (< 5,49 m) | 16 % | 35 % (6) |
| Lateral antes del bote | 23 % | 12 % (2) |
| Vidrio al vuelo | 23 de 126 | 0 de 17 |
| (a) / (b) / (c) | 3 / 10 / 2 | **1 / 0 / 0** |
| Ganados sin drive al fondo justo antes | 14 | **6** |
| Recepción final en los ganados | no llega 24, aire 3, tarde 1, fuera 1 | no llega 6, aire 1 |

**Cómo se ganaron los 7 puntos:**

- **5 con un drive de P = 1 pegado desde adelante** (contacto a z = 2,8–4,7 m) mientras el rival seguía atrás: su golpe anterior lo dio a z = 6,4–8,7 m.
  - Esos drives botan a 4,6–8,5 m.
  - 4 terminan en «no llega» y 1 en «aire».
  - Solo el de 8,49 m cuenta como (a).
- **2 con un golpe suave** (P ≈ 0,2) al frente: «no llega».
- **En 3 puntos el rival solo alcanzó a devolver el saque.** El saque del humano no se registra como golpe en `shot` (en el 23–24 tampoco), así que por la definición no cuenta como drive al fondo.
- **Antes del golpe ganador, el rival devolvió en 6 de 7 puntos un globo de defensa** (mira a 3–3,9 m), **y en los 7 la pelota quedó adelante**: el humano la golpeó a z ≤ 5,5 m.

**Lo que contradice la hipótesis de #120:** de los 5 drives que botaron a 7–8,5 m, el rival devolvió 4. No ganaron de inmediato; el punto se ganó después con pelotas más cortas.

**Lectura:** lo que gana en la 1230 no es la pelota que bota al fondo. Es el ciclo del §1 visto desde el otro lado:

1. El rival está atrás.
2. Su defensa cae corta.
3. El humano pega fuerte desde adelante.
4. El rival no alcanza a volver.

Es el mismo mecanismo de la simulación: (a), que es alcance y cinemática, más la defensa corta que ataca la opción 4.

**26 nov en la mañana, no pedido, misma build:** Medio, rival Fondista, 5 puntos, partido en pausa 3–2. De los 3 ganados, 2 son (a) con drives que botan a 8,42 y 9,38 m (este último tocó la lateral, desde z = 7,2 m, fuera de la zona de la guardia) y 1 es un drive corto al que el rival no llega.

## 4. Opciones (medidas en Medio, 100 partidos por celda; N2 con el protocolo de `ConfirmPerStyle`, 200 partidos, semillas `20261219`)

El Fondista queda igual en todas. Puntos del humano: Definidor / Completo.

| Opción | Drive al fondo, P = 1 | P = 0,85 | Medio suave | Drop | N2, novato (máx. \|Δ\|) | Tipo |
|---|---|---|---|---|---|---|
| Base | 97,0 / 94,7 | 96,2 / 92,8 | 32,4 / 28,0 | 25,8 / 43,6 | — | — |
| **1. `VolleySteal`** 0,6/1,2 · 1,2/1,8 · 2/2,6 · 3/3,5 | 97,0–97,4 / 94,7–95,2 | sin cambio | sin cambio | sin cambio | no se midió (sin efecto) | IA: SC-PM y SC-Lead |
| **2. `AttackBase`/`DropWeight`**, atA 1,2/0,4 · 2,0/1,5 | 97,9 / 94,0 | 97,4 / 93,7 | 31,3 / 33,8 | 31,2 / 39,1 | — | IA: SC-PM y SC-Lead |
| atB 1,6/0,8 · 2,4/1,9 | 96,3 / 95,9 | 97,4 / 94,9 | 32,1 / **38,0** | 32,7 / 49,3 | — | IA: SC-PM y SC-Lead |
| **3. `DefenseSlackS`** 0,03/0,06 | 97,0 / 94,7 | 95,4 / 92,1 | 31,9 / 26,7 | 24,8 / 41,8 | 2,94 pp, dentro | IA: SC-PM y SC-Lead |
| `DefenseSlackS` 0/0 | 96,8 / 95,5 | **85,1 / 84,5** | 42,2 / 30,6 | 13,3 / 23,4 | **9,56 pp: fuera** (> 5 pp en Definidor Medio y Difícil y en Completo Fácil) | **Decisión del PO** (N2) |
| `DefenseSlackS` 0/0 + atB | 95,4 / 95,1 | 93,6 / 88,6 | 48,4 / 53,2 | 13,0 / 24,0 | **11,28 pp: fuera** | **Decisión del PO** |

### 1. `VolleySteal` (opción principal de la orden)

**No propongo ningún valor.** La volea está disponible en 20–27 % de los drives al fondo a P = 1 y en ~1 % de los de P = 0,85. Subir `VolleySteal` hace que el rival la tome más (Completo pasa de 1,4 a 6,3 %), pero no cambia los puntos. Esa volea es un globo defensivo (§1) que le deja ~0,6 s al humano. Para que funcione como un corte, la regla de defensa tendría que usar la holgura de la volea. Eso es crear algo, y la orden lo excluye.

### 2. `AttackBase` y `DropWeight`

**No propongo ningún valor.** El rival ya está en defensa en el 98 % de las recepciones después de un drive al fondo, y la defensa bloquea el drop y el golpe bajo. Contra la pelota suelta, atB empeora al Completo: el humano gana 38 % de los puntos con el drive suave, frente a 28 %. Las variables que ya existen solo leen si la pelota es corta (`ahead` y `DropZoneAhead`, `RivalAi.cs:191–200`). «Despegada de la pared» o «al medio» no existen como rasgo: habría que crearlas, y no lo hice.

### 3. `DefenseSlackS` (la orden lo pedía solo si 1 y 2 no bastaban, y no bastan)

- **0,03/0,06:** se mantiene dentro de N2, pero no tiene efecto.
- **0/0:** al drive de P = 0,85 le quita 11 pp, pero al de P = 1 no le cambia nada, porque ahí domina (a). Además se sale de N2 y vuelve más débil al rival frente al drive suave (Definidor de 32 a 42 %). **La decisión es del PO, y no la recomiendo.**

### 4. Fuera de lo pedido, sin medir: que la defensa sea «alta y larga» como dice §11.3

Hoy el globo de defensa bota a ~5 m. La idea es que el golpe de supervivencia se elija por la profundidad de su propio pronóstico (`RallyContact.Forecast` ya da el bote) y que la volea use su propia holgura. Es IA, así que lo deciden SC-PM y SC-Lead, pero requiere código nuevo y pasar N2. Ataca (b) y (c). **No ataca (a):** en P = 1 el 56 % de los puntos son pelotas a las que el rival no llega. Eso es cinemática o alcance, del PO; la estocada de §9.1 no está implementada. Correr el cuerpo más rápido tampoco ayudó.

## Recomendación

1. **No aplicar ahora** ninguno de los valores de las opciones 1–3: no mueven el drive de P = 1 y los que lo mueven rompen N2.
2. **Las métricas de la 1230 ya están medidas (§3b).** No confirman que #120 sea la causa. Sí muestran el ciclo «defensa corta → golpe fuerte desde adelante → no llega», con 6 de 7 ganados en «no llega». Para proporciones hacen falta más partidos de prueba en la 1230.
3. **Encargar como experimento la opción 4** (defensa larga y volea con su propia holgura), como IA de SC-PM y SC-Lead, con N2 predeclarado.
4. Para (a), si el PO quiere atacarlo, son decisiones suyas: la estocada de §9.1 o el tope de largo de #120.

No se tocaron el costo de P = 1 ni `pullFullMm`.

## Límites

- RallySim no aplica la guardia del humano (se emuló).
- El bot humano siempre llega a su pelota (sin percepción ni ruido de postura), igual que el planificador del juego.
- En `run01`, 12 puntos de 111 celdas no se pudieron reconstruir y quedaron fuera de las tablas por golpe; sí cuentan en el porcentaje de puntos.
- **Verificación de la 1230 (26 nov):**
  - App 0.1.0 (1230); el lanzamiento escribe `0.1.0+8a32d42 dev`.
  - Presets 6/6 idénticos byte a byte al respaldo de `sc-install-8a32d42` antes y después de abrir la app; P0-calib-1 con el SHA de §5 y `last-active` P0-calib-1.
  - Métricas: las que estaban en el respaldo y siguen en el teléfono son idénticas. La rotación de 32 borró en el teléfono tres de las más antiguas (17 nov), que siguen en el respaldo.
  - `device.env` se copió sin cambios desde `sc-install-518f9c3` y se borró al terminar.
  - Evidencia en `Logs/evidence/metrics-1230-20261126/` (ignorada).
- La reproducción en P2 supone que la velocidad registrada en `shot` es la aplicada, después de la guardia.
- La regla de justicia se respeta en todas las opciones medidas: el rival lee solo la pelota y su propio plan.
