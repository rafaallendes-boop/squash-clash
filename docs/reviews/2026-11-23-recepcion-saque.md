# Recepción del saque: diagnóstico y opciones · 23 nov 2026

Encargo de SC-Lead (14:49) tras el playtest del PO en la build 1150 (`main` `90ddc7e`): «la recepción de saque está un poco difícil». Diagnóstico y propuesta: **no cambia nada del juego, ni umbrales ni criterios en `main`**. El análisis y el informe los hizo Claude (Opus). El arnés de simulación lo escribió un subagente Sonnet dirigido por Claude, así que **la revisión de Claude sobre ese código no es independiente**.

Niveles: uso las etiquetas del menú, **Fácil, Media y Difícil**, que en el código son `Easy`, `Medium` y `Hard`. Los porcentajes se calculan sobre los recuentos y se redondean solo al mostrarlos; las diferencias en pp salen de los recuentos, no de restar porcentajes redondeados.

## 1. Fuentes

- **D (dispositivo).** El 23 nov a las 17:50:16 Z, `xcrun devicectl device copy from … --domain-type appDataContainer --source Documents/metrics`, en solo lectura y sin reinstalar. Es el mismo método que usó la evidencia `install-90ddc7e-20261123`.
  - Hay 32 archivos y 31 son idénticos al `after-install` de esa instalación.
  - Solo creció el del playtest, `metrics-20261123T174154689…`, de 6.676 a 1.258.738 bytes. Su prefijo de 6.676 bytes tiene el mismo SHA-256 que la copia posterior a la instalación.
  - Los datos crudos no se versionan; quedan en `Logs/evidence/diag-recepcion-20261123/` (ignorado).
- **R193.** Es `scripts/metrics-summary.py` de la rama `tools/metrics-summary` (`694675c`), corrido sobre ese archivo. Resultado: 2 partidos, ambos en Media, zurdo, derrotas 1–7 y 6–8, 22 puntos. Recuento de devoluciones: «Humano ante saque rival: 13 saques; con traza de devolución: 4».
- **D+.** Tabla por saque con los campos registrados `rally-point`, `shot`, `shot-opportunity` y `shot-opportunity-choice`. Script `scripts/exp-recepcion/serve-table.py` en la rama desechable.
  - Margen = `DeltaMs` − (`KInSteps` − `TStarStep`) × ms/paso para los tempranos, y lo mismo con `KOutSteps` para los tardíos.
  - ms/paso = 1000/(120 × 0,65) ≈ 12,82 ms, comprobado contra los golpes aplicados.
- **S (simulación existente).** `RallySim new01` de [2026-11-20-serve](2026-11-20-serve.md): novato σ 90 ms que no toca, solo en Media. Cito `new01-points-summary.tsv`, la fuente del 89,7 %; ese TSV publica n y porcentaje, no el numerador.
- **S′ (simulación nueva).** Rama desechable `exp/recepcion-saque`, que **no se fusiona**. Prueba `ServeReceptionOptionsExperiment.MeasureOptions` con las semillas y el bucle de `ServeDiagnosisExperiment.PlayCell`.
  - Dos novatos: «std», con σ 90 y sin toques, que es el modelo de M1; y «toca», el bot v2.2 de `VolleyEnergyExperiment` con la anticipación muestreada del PO.
  - Dimensiones: 200 partidos por estilo en Media y en Fácil (std), y 60 por estilo en Media (toca).
  - Corrió con el lock de Unity entre las 18:02 y las 18:52 Z, en dos tramos. Las dos pasaron (1/1).
  - Resumen con numerador, denominador y porcentaje (columnas `…K`, `…N`, `…Pct`) en [sim-summary.tsv](../validation/evidence/2026-11-23-recepcion-saque/sim-summary.tsv); hashes en `source-*.tsv`.
  - El analizador `scripts/exp-recepcion/summarize-options.py` de la rama desechable reproduce ese TSV desde los `points.tsv` crudos (ignorados).

## 2. Diagnóstico

**Tasa de devolución del saque del rival por el humano.** Devolución = golpe aplicado al saque.

| Corte | D: playtest 90ddc7e | D: 22 nov 518f9c3 (saque nuevo por DEV) | S new01 (P+V, n) | S′ base std (k/n; el humano saca globo o rápido 50/50) |
|---|---|---|---|---|
| Total | **4/13 (30,8 %)** | 3/4 | 89,7 % (3.873) | 3.469/3.852 (90,06 %) |
| Globo | 1/5 | 0/1 | 94,0 % (1.977) | 1.806/1.932 (93,48 %) |
| Rápido (drive) | 3/8 | 3/3 | 85,3 % (1.896) | 1.663/1.920 (86,61 %) |
| Completo | 2/6 | — | 89,9 % (1.253) | 1.136/1.269 (89,52 %) |
| Definidor | 2/7 | 3/4 | 88,1 % (1.299) | 1.171/1.302 (89,94 %) |
| Fondista | sin dato (n 0) | sin dato | 91,1 % (1.321) | 1.162/1.281 (90,71 %) |
| Fácil / Difícil | sin dato en D | sin dato | no medido | Fácil 2.677/2.963 (90,35 %) / Difícil no medido |

- **Comparación en la misma sesión (D):** las recepciones del peloteo salieron bien 22 de 28 veces (78,6 %), frente a 4 de 13 al saque.
- **Saque original en el iPhone (`Auto`, otras builds, Media):** 25 de 33.

**Observado en D (playtest, los 9 saques no devueltos).** Esto es lo que registran los eventos:
- *Resultado:* 7 tempranos (`before-k-in`), 1 tarde (`after-k-out`), 1 sin golpe y 0 «no llega».
- *Oportunidad:* el anillo ámbar (volea) se ofreció en los 13 saques. En 6 de los 9 fallos el toque llegó cuando ya no era ámbar (`touch-outside-amber`) y se juzgó el golpe tras el bote. En los otros 3, el toque fue dentro del ámbar y quedó comprometida la volea (`committed`).
- *Margen respecto del borde de la ventana juzgada, de los 7 tempranos:* −10,10; −17,80; −28,98; −29,04; −50,22; −113,02 y −422,83 ms. **4 de 7 están a 50 ms o menos**; el de −50,22 ms queda fuera de ese corte. El tardío queda a +499,62 ms del cierre de la ventana de volea.
- *Sin golpe:* gesto cerrado como `InvalidAim` tras tocar fuera del ámbar.

**Clasificación (mía, por cortes sobre los márgenes; la intención no se registra).**

| Grupo | n de 9 | Casos |
|---|---|---|
| Temprano ≤ 50 ms del borde, en la oportunidad juzgada | 4 | −10,10 y −17,80 (bote, rápido); −28,98 y −29,04 (volea comprometida) |
| Temprano entre 50 y 100 ms | 1 | −50,22 (bote, globo) |
| Temprano > 100 ms | 2 | −113,02 y −422,83 (bote, globo, tras tocar fuera del ámbar) |
| Tarde tras comprometer la volea | 1 | +499,62 ms después del cierre de la volea |
| Sin golpe (`InvalidAim`) | 1 | — |

**Lectura del diagnóstico.** Separo lo demostrado de lo que es hipótesis:
- *Demostrado por los registros:* ninguno de los 9 fallos fue por alcance; 7 de 9 se registraron como tempranos; 4 de esos 7 quedaron a 50 ms o menos del borde; el saque ofrece dos oportunidades seguidas (ámbar y luego blanco) y los registros dicen cuál se eligió y cuál se juzgó.
- *Hipótesis compatible con esos eventos, no demostrada:* parte de los fallos vendría de leer mal la doble oportunidad del saque, es decir, soltar a la hora de una oportunidad cuando se juzga la otra. Encajan con ella los dos tempranos > 100 ms, el tardío tras comprometer la volea y quizá el sin golpe. Los registros no acreditan la intención del jugador.
- El aviso «¡Temprano!» (#96) se mostró y los fallos tempranos siguieron.
- La simulación no reproduce el problema. Con el novato que toca, la base da 951/1.171 (81,21 %), no un 31 %. Ningún modelo actual suelta a la hora de una oportunidad cuando se juzga la otra.
- Con el iPhone: n = 13, un jugador, dos partidos y solo Media. Es una señal para este jugador, no una tasa de población.

## 3. Opciones medidas (S′)

Devolución P+V del novato al saque del rival, como k/n (porcentaje):

| Variante en S′ (`RallyParams` o defaults) | std Media | std Fácil | toca Media |
|---|---|---|---|
| base (`1063d0f6cf66`) | 3.469/3.852 (90,06) | 2.677/2.963 (90,35) | 951/1.171 (81,21) |
| **A1:** la mitad de saques rápidos (`178fae835db3`) | 3.543/3.870 (91,55) | 2.670/2.899 (92,10) | 944/1.163 (81,17) |
| **A2:** sin saque rápido (`95b99006230e`) | 3.587/3.812 (94,10) | 2.765/2.939 (94,08) | 959/1.175 (81,62) |
| **B:** `timing.earlyPolicy` = 1, «T3 soft», global (defaults `11191fc66880`) | 3.658/3.670 (99,67) | 2.719/2.724 (99,82) | 1.042/1.175 (88,68) |
| **C:** sin volea al saque, `HumanVolleyServe` = 0 (`fd5cd7772125`) | 3.469/3.852 (90,06, idéntico) | 2.677/2.963 (90,35, idéntico) | 1.044/1.170 (89,23) |

Indicadores de riesgo, como k/n (porcentaje). «Puntos» y «Partidos» son un indicador de calibración dentro de esta misma corrida, **no** son N2 (`ConfirmPerStyle`):

| Variante | Puntos del humano, std Media | Partidos ganados, std Media | Puntos del humano, toca Media | Contactos de volea del humano, toca Media |
|---|---|---|---|---|
| base | 2.503/6.545 (38,24) | 110/600 (18,33) | 507/1.761 (28,79) | 2.668/10.054 (26,54) |
| A1 | 2.644/6.699 (39,47) | 115/600 (19,17) | 503/1.750 (28,74) | 2.526/9.788 (25,81) |
| A2 | 2.741/6.709 (40,86) | 144/600 (24,00) | 501/1.762 (28,43) | 2.435/9.820 (24,80) |
| B | 3.150/6.888 (45,73) | 231/600 (38,50) | 551/1.804 (30,54) | 4.357/16.768 (25,98) |
| C | 2.503/6.545 (38,24) | 110/600 (18,33) | 544/1.794 (30,32) | 2.377/10.325 (23,02) |

Devolución del rival al saque humano: en los agregados de **Media** va de 99,36 a 100 % en todas las variantes; base std 2.681/2.693 (99,55 %). En **Fácil** (std) va de 97,48 a 99,92 %: base 3.697/3.788 (97,60 %), A2 3.745/3.842 (97,48 %) y B 3.877/3.880 (99,92 %). Detalle en `sim-summary.tsv`.

**A. Menos saque rápido en Fácil y Media.**
- *Efecto:* con std, +1,49 pp (A1) y +4,04 pp (A2) en Media, y +1,75 y +3,73 pp en Fácil. Con el novato que toca, −0,04 y +0,40 pp: los tempranos bajan, pero suben los tardíos del globo (de 109/1.171 a 159/1.175 en A2).
- *Contra el iPhone:* el globo salió **peor** que el rápido (1/5 frente a 3/8). Es la opción con menos respaldo real.
- *Riesgo:* el Definidor pierde su rasgo (75 % de saques rápidos). Puntos del humano en std Media con A2: contra el Definidor, de 767/2.141 (35,82 %) a 835/2.195 (38,04 %), +2,22 pp; en el agregado, de 2.503/6.545 a 2.741/6.709, +2,61 pp. Para dejarlo solo en Fácil y Media hay que añadir código, con un parámetro por nivel.
- **Toca un criterio de la spec:** M1 (80–90 %) queda en 91,55 y 94,10 %. Hoy la base ya está en 90,06 %.

**B. Más ventana para el golpe temprano en la recepción.**
- *Efecto en simulación:* el mayor, +9,62 pp con std y +7,47 pp con el que toca, en Media. Pero B es global: rige para todos los golpes y para el rival.
- *Riesgo de calibración en std Media:* el humano gana +7,49 pp de puntos (2.503/6.545 → 3.150/6.888) y +20,17 pp de partidos (110/600 → 231/600). Es muy superior a la parada de 5 pp de N2.
- *Variante acotada (solo el saque, solo el humano, ≤ 50 ms):* necesita código en `ContactContract` y no la simulé. Aplicando la regla del contrato a los márgenes registrados del playtest, el contrato habría aceptado 2 de los 13 saques más con 25 ms y 4 con 50 ms. Sería contacto en K-in, Valid; no se sabe si la pelota habría llegado bien.
- **Toca tolerancias y criterios:** la política de timing (T3 pure, U1), `DefaultPreset.json`, la zona de timing de Codex, M1 y N2. Lo decide Rafael.

**C. Un solo anillo en la recepción del saque, sin volea al saque.**
- *Efecto en simulación:* +8,02 pp con el novato que toca, porque desaparecen sus fallos tardíos tras comprometer la volea (de 109/1.171 a 0/1.170). Con std, identidad exacta: M1 tal como se mide hoy no se mueve.
- *En el iPhone (contrafactual, no observado):* si la hipótesis de lectura de la doble oportunidad es correcta, C quita su origen. No hay mejora observada con C en el dispositivo.
  - Solo un caso tiene compatibilidad temporal con la ventana del bote: el tardío +499,62 ms soltó en el paso 151,44, dentro de la ventana del bote registrada (pasos 141,81 a 172,6).
  - Ese contrafactual supone la misma suelta y el plan de bote registrado. No garantiza un golpe aplicado, porque sin volea cambian la elección y el recorrido.
  - Los otros casos dependen de cómo juegue la persona y solo se miden en el iPhone.
- *Riesgo:* el humano voleará menos, de 2.668/10.054 (26,54 %) a 2.377/10.325 (23,02 %) de sus contactos, −3,51 pp. Eso va contra la meta de ≥ 40 %, que ya está en desviación (28 %). Se pierde una decisión de diseño de la volea v2.2 que el PO aceptó. Para limitarla a Fácil y Media, o dejar la volea al saque solo en Difícil, hay que añadir código.
- **No toca umbrales ni la física.** Cambia `RallyParams` y una decisión de diseño aceptada por el PO.
- *Complemento visual sin medir:* en el saque, mostrar «Espera el bote» sobre el anillo blanco. Solo se juzga en el iPhone.

## 4. Recomendación

- **C en Fácil y Media:** sin volea al saque y con un solo anillo blanco.
- **Validación:** un playtest corto del PO. Propongo como referencia, no como criterio aprobado, llevar 4/13 hacia la tasa del peloteo de la misma sesión (22/28, 78,6 %).
- **Sobre A y B:** A no tiene respaldo en el iPhone. B solo en su versión acotada y con decisión de Rafael sobre la tolerancia.
- **Decisiones de Rafael:** si se acepta perder la volea al saque en esos niveles, y si M1 se sigue juzgando con el novato «std», que hoy no reproduce lo observado en el iPhone.

Limpieza: `scripts/device.env` se borró de los dos worktrees al terminar. La rama `exp/recepcion-saque` queda en el remoto como evidencia y no se fusiona.
