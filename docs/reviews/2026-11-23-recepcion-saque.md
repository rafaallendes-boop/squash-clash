# Recepción del saque: diagnóstico y opciones · 23 nov 2026

Encargo de SC-Lead (14:49) tras el playtest del PO en la build 1150 (`main` `90ddc7e`): «la recepción de saque está un poco difícil». Diagnóstico y propuesta: **no cambia nada del juego, ni umbrales ni criterios en `main`**. El análisis y el informe los hizo Claude (Opus). El arnés de simulación lo escribió un subagente Sonnet dirigido por Claude, así que **la revisión de Claude sobre ese código no es independiente**.

## 1. Fuentes

- **D (dispositivo).** El 23 nov a las 17:50:16 Z, `xcrun devicectl device copy from … --domain-type appDataContainer --source Documents/metrics`, en solo lectura y sin reinstalar. Es el mismo método que usó la evidencia `install-90ddc7e-20261123`.
  - Hay 32 archivos y 31 son idénticos al `after-install` de esa instalación.
  - Solo creció el del playtest, `metrics-20261123T174154689…`, de 6.676 a 1.258.738 bytes. Su prefijo de 6.676 bytes tiene el mismo SHA-256 que la copia posterior a la instalación.
  - Los datos crudos no se versionan; quedan en `Logs/evidence/diag-recepcion-20261123/` (ignorado).
- **R193.** Es `scripts/metrics-summary.py` de la rama `tools/metrics-summary` (`694675c`), corrido sobre ese archivo. Resultado: 2 partidos, ambos en Medio, zurdo, derrotas 1–7 y 6–8, 22 puntos. Recuento de devoluciones: «Humano ante saque rival: 13 saques; con traza de devolución: 4».
- **D+.** Tabla por saque con los campos registrados `rally-point`, `shot`, `shot-opportunity` y `shot-opportunity-choice`. Script `scripts/exp-recepcion/serve-table.py` en la rama desechable. Para el margen ms/paso uso 1000/(120 × 0,65) = 12,8 ms, que comprobé contra los golpes aplicados.
- **S (simulación existente).** `RallySim new01` de [2026-11-20-serve](2026-11-20-serve.md): novato σ 90 ms que no toca, solo en Medio. Cito `new01-points-summary.tsv`, la fuente del 89,7 %.
- **S′ (simulación nueva).** Rama desechable `exp/recepcion-saque` (`7fcac96`), que **no se fusiona**. Prueba `ServeReceptionOptionsExperiment.MeasureOptions` con las semillas y el bucle de `ServeDiagnosisExperiment.PlayCell`.
  - Dos novatos: «std», con σ 90 y sin toques, que es el modelo de M1; y «toca», el bot v2.2 de `VolleyEnergyExperiment` con la anticipación muestreada del PO.
  - Dimensiones: 200 partidos por estilo en Medio y en Fácil (std), y 60 por estilo en Medio (toca).
  - Corrió con el lock de Unity entre las 18:02 y las 18:52 Z, en dos tramos. Las dos pasaron (1/1).
  - Resumen en [sim-summary.tsv](../validation/evidence/2026-11-23-recepcion-saque/sim-summary.tsv) y hashes en `source-*.tsv`.

## 2. Diagnóstico

**Tasa de devolución del saque del rival por el humano.** Devolución = golpe aplicado al saque.

| Corte | D: playtest 90ddc7e | D: 22 nov 518f9c3 (saque nuevo por DEV) | S new01 (P+V) | S′ base std (el humano saca globo o rápido 50/50) |
|---|---|---|---|---|
| Total | **4/13 (31 %)** | 3/4 | 89,7 % | 90,1 % (n 3.852) |
| Globo | 1/5 | 0/1 | 94,0 % | 93,5 % (1.932) |
| Rápido (drive) | 3/8 | 3/3 | 85,3 % | 86,6 % (1.920) |
| Completo | 2/6 | — | 89,9 % | 89,5 % (1.269) |
| Definidor | 2/7 | 3/4 | 88,1 % | 89,9 % (1.302) |
| Fondista | sin dato (n 0) | sin dato | 91,1 % | 90,7 % (1.281) |
| Fácil / Difícil | sin dato en D | sin dato | no medido | Fácil 90,3 % (2.963) / Difícil no medido |

- **Comparación en la misma sesión (D):** las recepciones del peloteo salieron bien 22 de 28 veces (79 %), frente a 4 de 13 al saque.
- **Saque original en el iPhone (`Auto`, otras builds, Medio):** 25 de 33.
- **Dónde se pierde el punto (D, playtest, los 9 saques no devueltos):**
  - *Alcance:* 0 de 9 por «no llega».
  - *Resultado registrado:* 7 tempranos (`before-k-in`), 1 tarde y 1 sin golpe.
  - *Anillo ámbar (volea):* se ofreció en los 13 saques. En 6 fallos el toque llegó cuando ya era blanco (`touch-outside-amber`, se juzga el golpe tras el bote). En 3 fallos el toque fue dentro del ámbar y quedó comprometida la volea.

| Mi clasificación de los 9 fallos (la intención no se registra) | n | Puntos y margen respecto del borde de la ventana |
|---|---|---|
| **Timing:** soltó en la oportunidad elegida, ≤ 50 ms antes del borde | 5 | −10 y −18 ms (bote, rápido); −50 ms (bote, globo); −29 y −29 ms (volea comprometida) |
| **Lectura:** golpe en el tiempo de la otra oportunidad, o > 100 ms fuera | 3 | volea comprometida y golpe +500 ms tarde, a la hora del bote; −113 y −423 ms antes del bote |
| **Sin golpe:** gesto `InvalidAim` tras tocar fuera del ámbar | 1 | — |

**Lectura del diagnóstico.**
- En el iPhone, el saque no falla por alcance. Falla por soltar antes de tiempo.
- La mitad de esos fallos tempranos quedan a 50 ms o menos del borde. La otra parte nace de cómo se lee la doble oportunidad del saque: primero el anillo ámbar y después el blanco.
- El aviso «¡Temprano!» (#96) se mostró y los fallos tempranos siguieron.
- La simulación no reproduce el problema. Con el novato que toca, la base da 81,2 % y no 31 %: ningún modelo actual simula soltar a la hora de la volea cuando se juzga el bote.
- Con el iPhone: n = 13, un jugador, dos partidos y solo Medio. Es una señal fuerte para este jugador, no una tasa de población.

## 3. Opciones medidas (S′, devolución P+V del novato al saque del rival)

| Variante en S′ | std Medio | std Fácil | toca Medio | Puntos del humano, std Medio (base 38,2) | Partidos ganados, std Medio (base 18,3) | Volea humana, toca (base 26,5 %) |
|---|---|---|---|---|---|---|
| base (`RallyParams` `1063d0f6cf66`) | 90,1 | 90,3 | 81,2 | 38,2 | 18,3 | 26,5 |
| **A1:** la mitad de saques rápidos (`178fae835db3`) | 91,6 | 92,1 | 81,2 | 39,5 | 19,2 | 25,8 |
| **A2:** sin saque rápido (`95b99006230e`) | 94,1 | 94,1 | 81,6 | 40,9 | 24,0 | 24,8 |
| **B:** `timing.earlyPolicy` = 1, «T3 soft», global (defaults `11191fc66880`) | 99,7 | 99,8 | 88,7 | 45,7 | 38,5 | 26,0 |
| **C:** sin volea al saque, `HumanVolleyServe` = 0 (`fd5cd7772125`) | 90,1 (idéntico) | 90,3 | **89,2** | 38,2 (toca: 30,3 frente a 28,8) | 18,3 | **23,0** |

Notas de la tabla:
- «Puntos» y «Partidos» son un indicador de calibración dentro de esta misma corrida, **no** son N2 (`ConfirmPerStyle`).
- La devolución del rival al saque humano no se mueve: 99,4–100 %.

**A. Menos saque rápido en Fácil y Normal.**
- *Efecto:* +1,5 pp (A1) y +4,0 pp (A2) con std. Con el novato que toca, ~0: pasa a fallar tarde el globo.
- *Contra el iPhone:* el globo salió **peor** que el rápido (1/5 frente a 3/8). Es la opción con menos respaldo real.
- *Riesgo:* el Definidor pierde su rasgo (75 % de saques rápidos) y se ablanda: +2,7 pp de puntos del humano en A2. Para dejarlo solo en Fácil y Normal hay que añadir código, con un parámetro por nivel.
- **Toca un criterio de la spec:** M1 (80–90 %) queda en 91,6 y 94,1 %. Hoy la base ya está en 90,1 %.

**B. Más ventana para el golpe temprano en la recepción.**
- *Efecto en simulación:* el mayor, +9,6 pp con std y +7,5 pp con el que toca. Pero B es global: rige para todos los golpes y para el rival.
- *Riesgo de calibración:* el humano gana 7,5 pp más de puntos y 20,2 pp más de partidos en Medio. Es muy superior a la parada de 5 pp de N2.
- *Variante acotada (solo el saque, solo el humano, ≤ 50 ms):* necesita código en `ContactContract` y no la simulé. Aplicando la regla del contrato a los campos registrados del iPhone, habría aceptado 2 fallos más con 25 ms y 4 con 50 ms, de 13. Sería contacto en K-in, Valid; si la pelota llega bien no se sabe.
- **Toca tolerancias y criterios:** la política de timing (T3 pure, U1), `DefaultPreset.json`, la zona de timing de Codex, M1 y N2. Lo decide Rafael.

**C. Un solo anillo en la recepción del saque, sin volea al saque.**
- *Efecto:* +8,0 pp con el novato que toca, porque desaparecen los 9,3 % de fallos tardíos tras comprometer la volea. Con std, identidad exacta: M1 tal como se mide hoy no se mueve.
- *En el iPhone:* elimina la confusión ámbar/blanco, que explica 4 de los 9 fallos según la tabla de arriba. Por regla del contrato es seguro solo +1: el golpe +500 ms habría caído dentro de la ventana del bote. El resto depende de cómo juegue la persona y solo se mide en el iPhone.
- *Riesgo:* el humano voleará menos, de 26,5 a 23,0 % de sus contactos, frente a la meta de ≥ 40 % que ya está en desviación (28 %). Se pierde una decisión de diseño de la volea v2.2 que el PO aceptó. Para limitarla a Fácil y Normal, o dejar la volea al saque solo desde Difícil, hay que añadir código.
- **No toca umbrales ni la física.** Cambia `RallyParams` y una decisión de diseño aceptada por el PO.
- *Complemento visual sin medir:* en el saque, mostrar «Espera el bote» sobre el anillo blanco. Solo se juzga en el iPhone.

## 4. Recomendación

- **C en Fácil y Normal:** sin volea al saque y con un solo anillo blanco.
- **Validación:** un playtest corto del PO, con la meta de llevar 4/13 hacia la tasa del peloteo (~79 %).
- **Sobre A y B:** A no tiene respaldo en el iPhone. B solo en su versión acotada y con decisión de Rafael sobre la tolerancia.
- **Decisiones de Rafael:** si se acepta perder la volea al saque en esos niveles, y si M1 se sigue juzgando con el novato «std», que hoy no reproduce el problema real.

Limpieza: `scripts/device.env` se borra de los dos worktrees al terminar. La rama `exp/recepcion-saque` queda en el remoto como evidencia y no se fusiona.
