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

*Revisión 2 (25 nov), decisión del PO:* el éxito de la **guardia** (§5) exige además el tope de largo de §7. Las clases y las metas M1–M6 de esta sección y de §3 son las del 20 nov y no se recalculan.

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
2. **Sin cambio si ya sirve:** si la solución actual ya es un éxito (definición de §2 más el tope de largo de §7, con P2 y las reglas del punto), se devuelve tal cual.
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

## 7. Revisión 2 (25 nov): tope de largo

**Decisión del PO** (Rafael, 25 nov, 10:43, chat de SC-PM): «Sí, tope de largo a 8,5 m». Responde a la opción 1 del [diagnóstico del 25 nov](../../reviews/2026-11-25-golpe-adelante-largo.md) (desde adelante, con P alta, la pelota sale larga y pega en el vidrio de atrás). Autoriza cambiar la definición de éxito de esta guardia y agregar ese umbral. **No autoriza ningún otro cambio de umbral o criterio.** [Registro](../../reviews/2026-11-18-po-approvals.md#25-nov-2026--tope-de-largo-de-85-m-en-la-guardia-drive-adelante).

1. **Éxito de la guardia (`FrontDeepDrive.Deep`)**: lo de §2 (legal hasta el segundo bote, primer contacto en el frontis, sin lateral entre el frontis y el primer bote, llega al tercio trasero antes del segundo bote) **y además**:
   - el primer bote cae en **z ≤ `FrontDeepDrive.MaxFirstBounceZ` = 8,5 m**, una constante con nombre en `FrontDeepDrive` (no es clave de `DefaultPreset`);
   - **no toca la pared trasera (el vidrio) entre el frontis y el primer bote**.
2. Vale en los dos usos del predicado: la comprobación del paso 2 de §5 (lo que antes salía «already-deep» y era largo ahora pasa al barrido) y cada candidato del barrido (pasos 3–5).
3. **Sin cambios:** la zona (P > 0,5, mira ≥ 0,80 m, delante de la línea corta), el barrido, la bisección, las tres alturas de respaldo, el tope de 40 evaluaciones, la velocidad congelada, el solo humano, el interruptor DEV y la línea de métricas `front-deep`. Si ninguna x ni altura cumple, se devuelve la solución original bit a bit (§5, paso 5).
4. **Fuera de alcance, sin tocar:** S1 y S3a (diseño del 14 nov), `pullFullMm`, `DefaultPreset.json`, SimulationKey, RallyParams y los hashes de AGENTS §5. El choque S1–guardia (opción 2 del diagnóstico) y la potencia (opción 3) siguen pendientes del PO.

**Medición** (experimento explícito `FrontLengthCeilingExperiment`, P2-Analytic, corridas `before01`=`before02` y `after01`=`after02` byte a byte; [informe](../../reviews/2026-11-25-tope-largo.md)):

- Golpes reales del PO del 23–24 nov desde adelante con P > 0,5 (49, sin intentos a dos paredes): largos (primer bote > 8,5 m o vidrio al vuelo) **20 → 1**; vidrio al vuelo **14 → 0**; lateral antes del bote **8 → 8**. El largo que queda está fuera de la zona (mira < 0,80 m).
- Coste por golpe dentro de la zona (39): evaluaciones media 7,97 → 11,46, mediana 8 → 9, p95 21 → 30, máximo 22 → 31 (cota 40 sin cambio). En el editor del Mac, mediana ≈ 0,13 ms por golpe, igual que antes.

**Riesgos declarados de la revisión 2:**

- **Muy cerca del frontis** (contacto a z ≤ 1,5 m), con P alta, puede no existir ninguna x ni altura que bote dentro del tope y llegue al tercio trasero a esa velocidad. Entonces la guardia devuelve el original y vuelve la lateral antes del bote. En la malla determinista de la zona (1458 golpes) la lateral antes del bote pasa de 7 a 21, todos `no-deep-x` con contacto a z = 1,0–1,5 m; en los golpes reales del PO no aparece. Es el caso del fixture `VeryFront` (contacto a 0,77 m). Esas 14 laterales nuevas **botan corto**, a z ≈ 0,14–2,4 m (antes la guardia las convertía en un drive largo o al vidrio), así que el drive fuerte pegado al frontis termina como una pelota corta. En la muestra S1 (810) aparecen además 6 laterales nuevas `no-deep-x` (primer bote a 1,5–4,0 m, una de ellas Out). Ninguna lateral anterior desaparece.
- **Golpes que vuelven a salir fuera de la cancha.** Sin el rescate de la guardia (`no-deep-x`, se devuelve el original), algunos golpes que antes eran `front-deep` y legales salen **Out** sin botar: en la malla, legales en zona **1452 → 1451** (`cx 3,2 · cz 1 · mira (3,2; 2) · P 0,70`; antes botaba a 9,37 m); en la muestra S1, legales **794 → 790** (cuatro golpes `SL5/SL6-Tstar-12 · mid · mira (2,6 | 4; 2,55) · P 0,60`; antes botaban a 8,7–9,0 m). En los golpes reales del PO no ocurre (49/49 legales). Falta saber si el PO prefiere en ese caso un largo legal al vidrio o el golpe original fuera: es una decisión de producto que esta revisión no toma.
- **El denominador de S1 baja** de 794 a 790 por esos cuatro Out, así que 74,6 % → 75,6 % no es una mejora: sobre los 810 golpes, S1 pasa de 592 a 597 (73,1 % → 73,7 %).
- **Baja la altura más a menudo:** en los golpes reales en zona, la guardia baja la altura en 12 de 39 frente a 2 de 39; la mínima sigue en 0,63 m.
- **Muestra pequeña** (49 golpes de cinco sesiones) y el mecanismo usa el mismo predicado que la medición del largo (el juez del experimento es independiente del código, pero mide lo mismo).
