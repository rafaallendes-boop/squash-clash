# Drive profundo desde adelante (SL5/SL6) — diseño y metas predeclaradas

20 nov 2026 · Rama `claude/front-deep-drive` desde la integración `f0fe172` (PR #84, sin fusionar). **Trabajo de un subagente Opus dirigido por Claude; la revisión de Claude no es independiente.** Este documento se commitea **antes** de medir cualquier corrección.

## 1. Pedido y alcance

El PO (20 nov, tras 3–4 partidos en `fb80015`): «aún falta por mejorar la pasada de SL5 y Sl6, a veces adelante quería tirar una pelota al fondo y se iba por las paredes laterales» y «la prioridad es mejorar lo que te dije». Autoriza tocar el mapeo de mira M2-4 y la selección del solver **solo en esta rama**, sin fusión.

Fuera de alcance, y parada si hiciera falta: física P2, `DefaultPreset.json`, SimulationKey, `ContactContract.cs`, ClockMap/SimClock y escenarios, oráculos o anclas de AutoQA. El rival, `RallySim` y la calibración de estilos quedan idénticos bit a bit.

## 2. Población y clases (fijadas antes de medir)

- **Golpes:** las líneas `shot` con `Reason=applied` del humano en `metrics-20261120T102118232-*` y `metrics-20261120T103647667-*` (ignorados, no se versionan), con la traza `shot-causal` del mismo intento.
- **Adelante:** contacto con z menor que la línea corta (9,75 − 4,26 = 5,49 m).
- **Intención profunda:** P > 0,5 y mira pedida y ≥ 0,80 m.
- **Intención cruzada:** la recta desplegada desde el contacto por la mira pedida llega a la otra mitad en la pared trasera (regla `xStar` de `ShotIntentSolver`). Si no, es paralela.

**Reproducción:** P2-Analytic desde el contacto y la velocidad registrados. La misma velocidad se reconstruye con el pipeline del producto: ritmo → causal → tope legal → intención → solver. Se registra la desviación y que la clase coincida.

**Clases:**

| Clase | Definición |
|---|---|
| `out` | Termina en Out o Tin, o no llega legal al frontis |
| `side-first` | El primer contacto es una lateral |
| `side-before-bounce` | Toca una lateral después del frontis y antes del primer bote |
| `short` | No llega a z ≥ 6,5 m (tercio trasero) antes del segundo bote |
| `side-before-back` | Bota primero, toca una lateral antes del tercio trasero y luego llega a él |
| `clean` | Llega limpio al tercio trasero |

**Éxito** = `clean` o `side-before-back`: legal, sin lateral antes del primer bote y llegando al tercio trasero.

## 3. Metas predeclaradas

- **M1 (puerta):** paralelas profundas desde adelante con éxito en **≥ 85 %** de la reproducción del PO. Hoy: 11/16 (68,8 %).
- **M2 (puerta):** cruzadas profundas desde adelante, se informa la tasa; **sin regresión de más de 5 pp**. Hoy: 3/39 (7,7 %).
- **M3 (puerta):** AutoQA `--suite v`, `t3` y `volley` PASS/0/1, sin dirty, con los mismos escenarios, oráculos, expectativas y anclas. Mismo requisito para SL1–SL6.
- **M4 (puerta):** con la corrección apagada, la solución del humano es idéntica bit a bit a la actual. El rival y `RallySim` no cambian: `ConfirmPerStyle` igual a `confirm01`, comparando `cells.tsv` sin la columna de segundos y `fondista-hard.tsv`.
- **M5 (informativa, no es puerta):** las mismas tasas en las sesiones del PO del 19–20 nov en `d11f3d9` (`metrics-20261119T232451136-*`, `metrics-20261120T010202037-*`). Son retención: no se usan para diseñar.
- **M6 (informativa):** el coste por solicitud en el editor y el número de trayectorias.

## 4. Diagnóstico medido (fase 1, `baseline02` = `baseline03` byte a byte)

Se reprodujeron 120 golpes: 88 bit a bit y 32 con una desviación de velocidad ≤ 1,8·10⁻⁵ m/s. Esa desviación se atribuye, como hipótesis, a la aritmética del dispositivo. La clase coincide en los 120.

**Fallos desde adelante (5 paralelas y 36 cruzadas), todos `side-before-bounce`:**

- La lateral llega muy pronto: z mediana 1,76 m en paralelas y 2,82 m en cruzadas.
- **Ángulo:** |pendiente de mira| mediana 0,36 en los fallos frente a 0,23 en los éxitos (paralelas); 0,77 frente a 0,36 (cruzadas).
- Existe una x limpia a la misma altura y velocidad en 5/5 paralelas y 29/36 cruzadas, siempre con la x del contacto dentro.
- **Desvío de la mira:** queda fuera de la ventana limpia por 0,34 m de mediana (paralelas) y 0,76 m (cruzadas).
- **Fracción del desvío lateral que sí entraba:** 0,72–0,98 (paralelas) y 0,20–0,93 (cruzadas).

**Causas descartadas con números:**

- **Mapeo táctil:** M2-4 es exacto. El frontis se toca donde se pidió y el fallo es geométrico: el mismo punto del frontis implica un ángulo mayor cuanto más adelante está el contacto. Además, la restitución 0,55 del frontis frena vz y abre aún más el ángulo de vuelta.
- **A2:** la primitiva sin intención tampoco salva los fallos (paralelas 11/16 igual; cruzadas 3/39 igual). El dominio lateral de A2 es ±Width/20 ≈ 0,32 m y su coste no castiga la lateral antes del bote.
- **Altura causal:** sin vectores causales fallan igual (0/5 y 1/36 se salvan).
- **Tope legal:** no actuó en ningún fallo.
- **Vector lateral causal:** vale 0 siempre.

Atrás de la T: paralelas 15/18 y cruzadas 7/17. Es informativo y queda fuera de alcance.

## 5. Diseño de la corrección: guardia de lateral del drive desde adelante

Es una capa nueva **solo del humano**, aplicada en `ShotController.Solve` después de `CausalQuality.Solve`. La clase `FrontDeepDrive` vive en Core y no la llaman ni el rival ni `RallySim` ni el pronóstico de la IA.

1. **Zona:** P > 0,5, mira pedida y ≥ 0,80 m, contacto delante de la línea corta y solución resuelta. Fuera de la zona devuelve la solución bit a bit.
2. **Sin cambio si ya sirve:** si la solución actual ya es un éxito (definición de §2, con P2 y las reglas del punto), se devuelve tal cual.
3. **Barrido grueso en x:** misma altura efectiva `TargetUsed.y` y velocidad congelada, con x = x_contacto + t·(TargetUsed.x − x_contacto) para t = 7/8, 6/8, … 0. Se toma el mayor t con éxito: el golpe más abierto que todavía llega limpio en la dirección pedida. Una solicitud que el solver solo resuelve bajando el objetivo no es candidata. El barrido no supone monotonía.
4. **Refinamiento:** 4 pasos de bisección entre ese t y el siguiente octavo, con resolución 1/128.
5. **Solo si ninguna x sirve a esa altura:** pasa sobre todo muy cerca del frontis, donde a velocidad alta la recta se va por arriba del fondo. Se repite el barrido (t = 1 … 0) en tres alturas más bajas, y = y₀ − (y₀ − y_legal)·j/3 con j = 1..3, donde y_legal es la parte superior del tin + el radio, como en el solver. Se toma la primera altura que sirve. Si ninguna sirve, devuelve la original.
6. **Coste:** a lo sumo 40 solicitudes y 80 trayectorias P2 adicionales.
7. **Resultado:** el motivo es `front-deep` y la retícula muestra la mira efectiva, como con `intent-drive`.

*Revisión 1, antes de medir la corrección sobre la reproducción:* la primera versión (pivote en la x del contacto y bisección) no actuaba en el fixture de PlayMode SL5 (contacto z ≈ 1,2 m, P = 1, y = 1,5). Ahí la recta desde el contacto se va por arriba de la pared de fondo, así que el pivote no sirve. Eso coincide con las 7 cruzadas sin x limpia del diagnóstico (z < 2 m). Por eso se cambió a barrido grueso más alturas de respaldo. Las metas de §3 no cambian.

No cambia la física, los defaults, el contrato de contacto, el reloj ni la velocidad. Cambia la x del frontis y, solo en el paso 5, baja la altura.

**Interruptor DEV:** «Drive adelante: sí/no» en el panel DEV, encendido por defecto y sin recordarse entre lanzamientos. No es una clave de `DefaultPreset`. Cada acción escribe una línea de métricas `front-deep` con `on`, `acted`, `t` y la x original y elegida, solo dentro de la zona.

## 6. Riesgos declarados

- **No es ciego:** el mecanismo usa el mismo predicado de éxito que la métrica, así que M1/M2 miden sobre todo cobertura (si existe una x limpia entre el contacto y la mira). La retención M5 y la clase `side-before-back` dan una lectura adicional.
- **Muestra pequeña:** 16 paralelas y 39 cruzadas de una sola sesión.
- **Discontinuidad en la línea corta:** detrás de ella la guardia no actúa.
- **Cruzadas muy adelante (z < 2 m):** sin x limpia a esa altura y velocidad (7/36). La altura de respaldo puede convertirlas en un golpe casi paralelo y más bajo: se da prioridad a la profundidad sobre la dirección.
