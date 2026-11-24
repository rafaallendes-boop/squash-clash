# «Rally aplicado»: definición de la métrica de playtest

24 nov 2026. Encargo de SC-PM (pedido de SC-Lead). Solo documentación: no cambia código, escenas, umbrales, criterios ni oráculos.

**Naturaleza del documento.** Es una definición **descriptiva** de una métrica de playtest, escrita para que dos personas que lean el mismo log obtengan el mismo número. **No es un criterio de aceptación**, no fija ningún umbral y no sustituye a las reglas de AGENTS ni a los criterios predeclarados. Quién la reconstruyó: Claude (Sonnet 5.5), sin revisión independiente.

## 0. Qué había escrito antes

- **Definición completa: ninguna.** `git grep -i "rally"` en `docs/` y en el código no encuentra una definición de «rally aplicado» ni del 22/28.
- **Mención parcial** (en la PR #105, rama `docs/recepcion-saque`, commit `76678a3`, **no fusionada en `main`**), en `docs/reviews/2026-11-23-recepcion-saque.md`: «las recepciones del peloteo salieron bien 22 de 28 veces (78,6 %), frente a 4 de 13 al saque», y en la misma tabla «Devolución = golpe aplicado al saque». No dice qué eventos entran ni qué se hace con las recepciones sin golpe.
- **Script de #105:** `scripts/exp-recepcion/serve-table.py` (rama `exp/recepcion-saque`, `40e46aa`) solo recorre los puntos con saque del rival y su primer lanzamiento. **No contiene la partición del peloteo**: el 22/28 salió de una cuenta que no quedó en el repo.
- **Conflicto con lo anterior: ninguno.** Lo que sigue es compatible con esas dos frases, pero las precisa. Sí hay una ambigüedad que el doc no resuelve: ver §2.3.

## 1. Definición operativa

### 1.1 Fuente

Un archivo de métricas del iPhone (`Documents/metrics/metrics-*.jsonl`): un JSON por línea con `kind`, `payload` (JSON como texto) y otros campos. Se usan cuatro tipos de evento:

| `kind` | Para qué |
|---|---|
| `match-start` | Marca desde dónde se cuenta. Los eventos previos al primer `match-start` (juego libre) no cuentan. |
| `shot` | Un registro por golpe resuelto. Campos usados: `LaunchId`, `Reason`, `Result`, `Fail`, `Applied`. |
| `rally-point` | Cierra un punto y lo delimita. Campos usados: `server` (`Human` o `Rival`), `match`, `autopilot`. |
| `shot-opportunity`, `shot-order`, `shot-opportunity-choice` | Solo sirven para encontrar el primer lanzamiento del punto (§1.3). |

### 1.2 Unidad: un golpe del humano por lanzamiento

Se agrupan los eventos en **puntos**: cada `rally-point` cierra el punto formado por los eventos desde el punto anterior (o desde el `match-start`). Dentro de un punto, la unidad es el **lanzamiento** (`LaunchId`): cada vez que la pelota llega a la zona del humano hay un lanzamiento.

Por cada `LaunchId` se toma el **registro `shot` del humano**, es decir, el `shot` cuyo `Reason` **no** es `rival-contact` (ese valor es el golpe del rival, `RallyDirector.cs`). Si hubiera más de uno para el mismo `LaunchId`, vale el último. En los dos archivos revisados nunca hubo más de uno.

### 1.3 Qué se excluye: la devolución del saque del rival

En un punto con `server = Rival`, el **primer lanzamiento** (el `LaunchId` más bajo entre los eventos `shot`, `shot-opportunity`, `shot-order` y `shot-opportunity-choice` del punto) es la **recepción del saque**. No es rally: pertenece a la métrica de devolución del saque (11/11 en la build 1174, 4/13 en la 1150). Se excluye del rally aunque sea un golpe del humano.

En un punto con `server = Human` no se excluye ningún lanzamiento: el saque propio no genera un `shot` del humano (en el archivo de la 1174, el `LaunchId` 3 solo tiene el `rival-contact`, línea 32) y todos los `shot` del humano son respuesta a un golpe del rival.

### 1.4 Dos variantes: amplia y «con intento»

| | Qué entra | Qué cuenta como aplicado |
|---|---|---|
| **A. Amplia** | Todo registro `shot` del humano que no sea la recepción del saque, de cualquier `Result`. | `Applied = true`. |
| **B. «Con intento»** | Lo de A, **menos** los registros con `Result` `NoOrder` o `Unreachable`. | `Applied = true`. |

- **Aplicado** es el campo `Applied` del registro (`Reason = applied`, `Result` `Valid` o `Perfect`): el contrato de contacto aceptó el golpe. No se reconstruye por tiempos.
- **Numerador (A y B):** cantidad de registros con `Applied = true`.
- **Denominador de A:** todos los registros que entran en A. **Denominador de B:** A menos `NoOrder` y `Unreachable`.
- **Qué hay en el denominador y no en el numerador:**
  - `Reason = before-k-in` (`Result` `Failed`, `Fail` `Air`): el humano soltó antes de la ventana y no hubo margen. Entra en A y B. (No apareció en el rally de la 1174; sí 3 en el de la 1150.)
  - `Reason = after-k-out` (`Failed`, `Fail` `Late`): soltó después de cerrada la ventana. Entra en A y B.
- **`NoOrder`** (`Reason = reception-ended-without-order`): había plan de golpe o volea, la recepción se cerró y el humano no completó ninguna orden. **`Unreachable`** (mismo `Reason`): no había plan ni volea, es decir, la pelota no era alcanzable. Los dos los emite `ShotController.CloseReception` cuando no se consumió ningún intento. **Solo entran en A.** Un `NoOrder` no es un intento fallido: el log no dice si el jugador quiso golpear, porque la intención no se registra.
- **Qué no es rally en ninguna variante:** la recepción del saque del rival (§1.3), el golpe del rival (`rival-contact`), los golpes de partidos con `autopilot` y los puntos fuera de `match`. Los dos archivos revisados solo tienen puntos con `autopilot = false` y `match = true`.
- **Alcance de los datos:** se limita a eventos posteriores a un `match-start`. El archivo de la 1174 empieza con unos 3 minutos de juego libre que no entran.

## 2. Cómo se calcularon las cifras

### 2.1 Build 1174: 24/25 y 24/37

Archivo `metrics-20261124T175940193-7c1ea13776bc4d29bbe08cc0aaf81b49.jsonl` (SHA-256 `b6bef4208566695763b4c46e6baba46847154a6255d7dc83fe336e84f18516fd`, 714 eventos, 2 partidos, 22 puntos). Resultado de aplicar §1:

| | Registros | Aplicados | Detalle |
|---|---|---|---|
| Variante A (amplia) | 37 | 24 | 24 `applied` (17 `Valid`, 7 `Perfect`); 1 `after-k-out`; 9 `NoOrder`; 3 `Unreachable` |
| Variante B («con intento») | 25 | 24 | 24 `applied` + 1 `after-k-out` |

Por partido: partido 1, A 16/21 y B 16/16; partido 2, A 8/16 y B 8/9.

**El 24/25 que se informó es la variante B.** La variante A da 24/37 (64,9 %); ambas están en la tabla del informe de métricas de la 1174. Ese informe llama al 24/25 «misma definición, con intento», pero la reconstrucción del 22/28 (§2.2) es la A, así que «misma definición» solo vale si se entiende «misma, salvo que se dejan fuera las recepciones sin intento».

### 2.2 Build 1150: 22/28, lo que se sabe y lo que no

**Se sabe:**
- El 22/28 y el 78,6 % están en el informe de #105 (§0), sobre el archivo `metrics-20261123T174154689-84b1a457952c4a689d5b6cffe9955788.jsonl` (1.258.738 bytes, SHA-256 `08276f5bf8153c62347d711f57f6ab702ac6ba5e03763f23c4f71df1597939c3`, 22 puntos, ambos en Media, derrotas 1–7 y 6–8).
- Aplicar §1 a ese archivo da **22/28 con las dos variantes**: 18/22 en puntos con saque del humano y 4/6 en puntos con saque del rival. De los 28: 22 aplicados (14 `Valid`, 8 `Perfect`), 3 `before-k-in` y 3 `after-k-out`. El mismo recuento da 4/13 para la recepción del saque, igual que #105.

**No se sabe:**
- Qué cuenta usó exactamente quien produjo el 22/28 en #105: no está escrita ni en el informe ni en el script publicado.
- Si ese 22/28 era la variante A o la B. **El archivo no lo distingue:** no tiene ningún `NoOrder` ni `Unreachable` en el rally, así que A y B dan lo mismo. Por eso el 22/28 es compatible con las dos y la comparación 22/28 frente a 24/25 solo es limpia si se elige una y se aplica a ambas builds: B da 22/28 y 24/25; A da 22/28 y 24/37.
- Si en #105 se excluía la recepción del saque por el mismo criterio de §1.3. La cifra coincide con esa exclusión, pero no hay prueba de que fuera la regla original.

### 2.3 Lo que queda sin decidir

Cuál de las dos variantes es «la» métrica de rally. Esta nota no elige: A es la reconstrucción más literal del 22/28 y B es la que se informó para la 1174. Los 12 casos que las separan (9 `NoOrder`, 3 `Unreachable`) son justamente lo nuevo de ese playtest, así que la elección cambia el titular (96,0 % frente a 64,9 %). Cualquier comparación entre builds debe nombrar la variante.

## 3. Ejemplos con líneas reales

Todas las líneas son del archivo de la 1174 (§2.1). Se muestran solo los campos relevantes de cada evento (el `shot` completo tiene unos 80). «Punto» es el orden de los `rally-point` en el archivo.

**1. Rally aplicado, saque del humano (cuenta: A y B, aplicado).** Línea 57, punto 1:
```
shot  LaunchId 5  Reason applied  Result Valid  Fail None  Applied true  EarlyGeometry false  DeltaMs 57.55
```
El punto 1 se cierra en la línea 95 con `server: Human`.

**2. Recepción del saque (se excluye) y el golpe siguiente (cuenta).** Punto 5, `server: Rival` (cierra en la línea 185). Hay dos golpes aplicados del humano:
```
L169  shot  LaunchId 15  Reason applied  Result Valid  Applied true   <- primer lanzamiento: recepción del saque, fuera
L182  shot  LaunchId 16  Reason applied  Result Valid  Applied true   <- segundo lanzamiento: rally, cuenta
```
Ambos son aplicados, pero solo el de la línea 182 entra en el 24/25.

**3. Golpe del rival (se excluye).** Línea 172, el rival responde a la devolución de la línea 169:
```
shot  LaunchId 15  Reason rival-contact  Result Valid  Applied true
```

**4. Golpe tardío (denominador, no numerador; A y B).** Línea 564, punto 17 (saque del rival, `LaunchId` 50 es el segundo lanzamiento):
```
shot  LaunchId 50  Reason after-k-out  Result Failed  Fail Late  Applied false  DeltaMs 121.5  KOutSteps 174.74
```
Es el único fallo con intento del rally de la 1174; el punto cierra en la línea 567 con `reception: Late`.

**5. Recepción sin golpe (solo variante A).** Línea 93, punto 1:
```
shot  LaunchId 8  Reason reception-ended-without-order  Result NoOrder  Fail ""  Applied false
```
Hay 9 así en la 1174 (líneas 93, 156, 237, 300, 335, 415, 586, 609, 630).

**6. Pelota inalcanzable (solo variante A).** Línea 436, punto 14 (saque del rival; el `LaunchId` 38 de la línea 428 es la recepción del saque):
```
shot  LaunchId 39  Reason reception-ended-without-order  Result Unreachable  Fail ""  Applied false  KInSteps 0  KOutSteps 0
```
Hay 3 así: líneas 436, 538 y 687.

**7. Cómo se reconoce de quién es el saque.** Línea 95 (`rally-point` #1): `server: Human`, `shots: 9`. Línea 185 (`rally-point` #5): `server: Rival`, `shots: 3`.

## 4. Límites

- Es una cuenta sobre un archivo de un jugador, dos partidos y un estilo de rival; no es una tasa de población.
- «Aplicado» no mide calidad del golpe ni intención: `Valid` y `Perfect` cuentan igual, y la intención del jugador no se registra.
- La métrica no se usa como criterio de aprobación de nada. Si algún día se quisiera usar como tal, necesitaría una decisión del PO y una definición predeclarada; esta nota no la sustituye.
- Los crudos del iPhone no se versionan (`AGENTS.md` §3). Los números de línea de §3 son del archivo con el SHA-256 de §2.1. Quien tenga el archivo puede reproducir el recuento con el script de la reconstrucción (`rally-definition-check.py`, fuera del repo, en el informe de la 1174).
