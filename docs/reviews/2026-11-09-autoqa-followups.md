# AutoQA — pendientes tras las PR #37 y #38

Entrega autorizada por el PO el **9 de noviembre de 2026 (Chile)**, en `autoqa/followups`, desde main `9ae34fc69806a917d8e24096fcaad5e0a5b8bcc0`. PR propia para revisión de Claude coordinada por el PO, **sin fusionar**.

## Orden y alcance

Primer commit **7d270d0**: solo AGENTS.md y [aprobaciones](2026-10-25-first-shot-po-approvals.md), registrando cierres #37/#38, literales del PO, padres y árboles idénticos, y este encargo antes de tocar código. Corrección del envoltorio **6543733**; verificador y pruebas **c2117b1**.

Solo scripts, agente operador, cambio de versión en skill y documentación. No se cambia producto, escenarios, expectativas, márgenes, tolerancias, determinismo del juego, defaults, spec, planes ni criterios. Sin iPhone, Xcode, instalación, regresión completa ni check-release, conforme al alcance. Investigador y comportamiento de `/autoqa` conservados salvo la versión autorizada del verificador.

## Envoltorio: evidencia con procedencia propia

`failure()` captura y archiva el original del arnés solo en su primera llamada. Si no existía, un segundo error conserva el primero en `summary.previous-error.json` y `report.previous-error.md`, referenciados con SHA-256 como `previousErrorSummary`/`previousErrorReport`. `executionError.previous` conserva la cadena. Se eligen archivos propios para auditar también el resumen completo y el informe exactos; no basta con atribuir al arnés una cadena de errores del envoltorio. Con arnés original y varios errores, permanece el comportamiento de #38.

TDD: nueva reproducción `trigger=missing-xml`, `malformed=missing`, `dirty_after=True`. **RED: 18 métodos, un fallo por aserción, cero errores**: aparecía summary.harness.json sin arnés. **GREEN: 18/18 métodos, 33 fixtures**, incluidas las 17 pruebas anteriores. Resultado de la reproducción: ERROR/4/nivel 2, harnessResults=null, archivos propios con SHA y cadena missing-xml→dirty-after-run. Evidencias en `Logs/autoqa-followups/wrapper-red.txt` y `wrapper-green.txt`.

## Verificador v7 y TDD

Se extrae el bloque mediante el awk documentado y se ejecuta con **python3 -I**, biblioteca estándar y plantilla canónica intacta. `scripts/tests/test_autoqa_checker.py` usa el generador versionado del envoltorio, controles sintéticos completos y copias de cinco corridas reales históricas. Los originales nunca se modifican.

**RED con v6: seis métodos, cinco fallos por aserción, cero errores** (tres discrepancias autorizadas más manipulación de cada nuevo artefacto). **GREEN con v7: 6/6 métodos, cero fallos/errores/omisiones; 17 ejecuciones del verificador**. Comando con controles reales:

```sh
AUTOQA_REAL_RUNS="/ruta/al/worktree/autoqa-mvp/Squash Clash/Logs/autoqa" python3 scripts/tests/test_autoqa_checker.py
```

| Caso autorizado | Esperado por v6 | Esperado por v7 | Nivel operador v7 |
| --- | --- | --- | --- |
| Arnés con schema desconocido y señales altas | 3 | 4 | 3; incoherencia de schema |
| Arnés declara nivel 3, salida 0 y flags true/canario FAIL | 4 | 3 | 3 |
| Arnés declara salida 3, nivel 1 y flags true/canario FAIL | 4 | 3 | 3 |
| Cada artefacto previousError manipulado | 4 | 3 | 3; SHA distinto |

El schema inválido conserva su SHA y su diagnóstico, pero no aporta señales del arnés. Solo enteros válidos nivel/salida 3 aportan categoría 3. Los nuevos artefactos se comprueban como evidencia opaca: incluso si contienen canario PASS/nivel 3, no suministran señales del arnés. Los fixtures mínimos del envoltorio tienen intentos incompletos intencionales; sus 38–41 incoherencias de formato no son mediciones del juego ni se rebaja el verificador para aceptarlos.

Controles completos y copias reales: PASS→0/nivel1, FAIL→1/nivel2, canario PASS→3/nivel3, hash inesperado→3/nivel3, environment→4/nivel2/reintento elegible. Cero incoherencias en esos controles. Invariantes de v6 conservados, incluida excepción→bloque de nivel3, SHA ausente/null como integridad, suite exacta 6×3, hashes SL1/SL5 y SimulationKey, y bloqueo de reintento con error no ambiental del arnés.

La suite externa **mutations-v6.py**, de solo lectura, se ejecutó con v6 y v7: **35/35 GREEN en ambos**. Ninguna expectativa de esos 35 casos cambia. Las cinco diferencias intencionadas anteriores están cubiertas por las nuevas pruebas. Salidas: `Logs/autoqa-followups/mutations-v6.txt` y `mutations-v7.txt`.

## Compatibilidad de todas las corridas históricas disponibles

Inventariados todos los worktrees Git: solo autoqa-mvp (20) y autoqa-wrapper-precedence (3) contenían directorios de corridas reales en su raíz Logs/autoqa. Se excluyen repositorios sintéticos anidados de pruebas; no se excluye ninguna corrida real por resultado. **23/23 con los mismos cinco campos entre v6 y v7**: salida esperada, nivel, gatePass, objeto completo de reintento y número de incoherencias. SHA, tamaños y mtimeNs de todos sus archivos iguales antes/después. Detalle JSON y salidas individuales en `Logs/autoqa-followups/compatibility.json` y `compatibility/`.

| Corrida | Salida esperada (ambos) | Nivel (ambos) | gatePass | Reintento elegible | Incoherencias (ambos) |
| --- | --- | --- | --- | --- | --- |
| 20261109T120012Z-2cedf75-30618 | 0 | 1 | true | false | 0 |
| 20261109T120432Z-491e10d-9298 | 0 | 1 | true | false | 0 |
| 20261109T120528Z-491e10d-22213 | 3 | 3 | false | false | 0 |
| 20261109T120542Z-491e10d-9435 | 2 | 2 | false | false | 0 |
| 20261109T120557Z-491e10d-4834 | 1 | 2 | false | false | 0 |
| 20261109T120612Z-491e10d-25404 | 3 | 3 | false | false | 0 |
| 20261109T120626Z-491e10d-330 | 4 | 2 | false | true | 0 |
| 20261109T120909Z-491e10d-19260 | 0 | 1 | true | false | 0 |
| 20261109T130148Z-bc82c3e-10259 | 0 | 1 | true | false | 0 |
| 20261109T133025Z-8e187de-21128 | 3 | 3 | false | false | 0 |
| 20261109T133037Z-8e187de-26096 | 0 | 1 | true | false | 0 |
| 20261109T133200Z-8e187de-1255 | 4 | 3 | false | false | 2 |
| 20261109T133622Z-8e187de-24321 | 1 | 2 | false | false | 0 |
| 20261109T133633Z-8e187de-29193 | 1 | 2 | false | false | 0 |
| 20261109T133645Z-8e187de-24592 | 4 | 2 | false | false | 0 |
| 20261109T134237Z-a537236-28153 | 0 | 1 | true | false | 0 |
| 20261109T134252Z-a537236-4896 | 0 | 1 | true | false | 0 |
| 20261109T140748Z-7cd5a18-30940 | 0 | 1 | true | false | 0 |
| 20261109T140804Z-7cd5a18-28156 | 0 | 1 | true | false | 0 |
| 20261109T142141Z-7cd5a18-12190 | 0 | 1 | true | false | 0 |
| 20261109T195229Z-34b28c4-10968 | 0 | 1 | true | false | 0 |
| 20261109T195311Z-34b28c4-15860 | 0 | 1 | true | false | 0 |
| 20261109T200908Z-dde63e6-29660 | 0 | 1 | true | false | 0 |

La corrida histórica 20261109T133200Z conserva sus dos incoherencias con ambas versiones; no se convierte en PASS ni se reejecuta. Ninguna diferencia de compatibilidad sin justificar.

## Dos corridas reales nuevas

Pendientes de la autorización solicitada al PO para copiar scripts/device.env byte a byte desde autoqa/mvp. No se ha copiado ni modificado el archivo. No se han iniciado corridas dependientes de esa autorización.

## Huellas y archivos protegidos

585 archivos versionados de Assets/ProjectSettings/Packages y el agente investigador contrastados byte a byte con main 9ae34fc. Manifiesto local: `Logs/autoqa-followups/protected-manifest.json`, SHA-256 `19739a478595403d3acda06a6bd564c7ea05fce807e43e7171fc22aea7533661`. Logs, fixtures y evidencias permanecen ignorados.

| Evidencia local (bajo Logs/autoqa-followups) | SHA-256 |
| --- | --- |
| wrapper-red.txt | 223f15e69fd1ce017387c45a2eb2ede139a1758e702f365bec7ddce775e3c95b |
| wrapper-green.txt | c7a92e5985c64fb7733c5d3e0d9240d02e140f0124e4d8eeb62431a3f4848f99 |
| checker-red-v6.txt | e7c4cc56c4eae175d84a2bd15f76abe005898dd017281679807d05676cbedeaa |
| checker-green-v7.txt | eedb6c97f85034356997342b80230b94af5bdbc2719421444bf8cdcc5eefe779 |
| autoqa-check-v6.py | 6c73e0524229f7e80dd11754dd9c1e1255334b0958855a1bd13c776131ace62c |
| autoqa-check-v7.py | 7d9290595892ead1eba7ff632cd240e4faecb95e88a1e1c42d6ca65e36e30ea1 |
| mutations-v6.txt | 2aa87f0e0d298ae35ed22b779c317e68c0809a5f28a6b3c1fd750fd2b183a1e8 |
| mutations-v7.txt | 2aa87f0e0d298ae35ed22b779c317e68c0809a5f28a6b3c1fd750fd2b183a1e8 |
| compatibility.json | 829e47d26b87a8849c7e22a2d97e7330ee136879babac2aa6847a297c2e1ca43 |
| historical-manifest.json | b21459d14526a55138d78747ff356ea59f4b8879ae9af563ec72fe95588c7684 |
