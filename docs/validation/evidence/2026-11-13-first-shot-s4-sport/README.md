# Evidencia derivada — First Shot T4.4

Fuente del experimento: `fd81c2dba3500dbf7b6bd7677f853009203dd4b9` (árbol limpio). Método y diagnóstico: [informe](../../../reviews/2026-11-13-first-shot-s4-sport-scenarios.md). Dos ejecuciones definitivas, 38.304 tiros y 72.432 pares cada una. **R3 Fail; no aceptación de First Shot.**

- `summary.json`: configuración, contactos, existencia por escenario, agregados y R7.
- `witnesses.json`: tiros seleccionados y sus eventos P2 ordenados, con huella/veredicto.
- `trials.tsv.gz`: todos los tiros de la grilla; `pairs.tsv.gz`: todos los pares y motivos de no evaluación. Los IDs de par referencian `id` en trials. Bote ausente tiene flag `Observed=0`; sus coordenadas cero **no son mediciones**. `sideMin=-1` significa tramo no observado. `legal` significa hasta segundo bote; `goodFront` solo llegada al frontis válida.
- `counterexamples.tsv`: los 290 contraejemplos de R2 y 42 de R3, con todos los campos de ambos tiros unidos por ID, sin redondear. La columna `status` de `pairs` distingue Increasing/Equal/Counterexample/NotEvaluated.
- `manifest.json`: SHA-256 comprimidos y descomprimidos, recuentos y razones. `execution-manifest.json`: inventario de las cinco invocaciones (tres precomprobaciones, dos definitivas), XML y logs ignorados, más suites completas.
- `derive.py`: comprueba igualdad de los cuatro archivos entre ambas corridas, exhaustividad de grilla/pares y agrega evidencia portátil. Gzip sin fecha (`mtime=0`); sin cambios de datos deportivos.

## Reproducir

En un checkout limpio de la fuente, con la versión Unity de `ProjectVersion.txt` y `scripts/device.env` local ignorado, sin otro Unity activo. El experimento no forma parte de `scripts/test.sh` ni de AutoQA. No crea build ni toca iPhone.

```bash
source scripts/unity-env.sh
mkdir -p Logs/evidence/sport
# Ejecutar secuencialmente, una invocación por vez; comprobar antes que no exista otro Unity.
SC_SPORT_RUN=run01 "$UNITY" -batchmode -nographics -projectPath "$ROOT" \
  -runTests -testPlatform EditMode -assemblyNames SquashClash.EditMode.Experiments \
  -testFilter SquashClash.Experiments.EditMode.SportScenarioExperiment.WriteAnnexCSportReport \
  -testResults "$ROOT/Logs/evidence/sport/run01.xml" \
  -logFile "$ROOT/Logs/evidence/sport/run01.log"
# Tras terminar, repetir con SC_SPORT_RUN=run02 y XML/log run02.
python3 docs/validation/evidence/2026-11-13-first-shot-s4-sport/derive.py \
  Logs/experiments/sport/run01 Logs/experiments/sport/run02 /tmp/sport-derived
```

No sobrescribe directorios existentes. Preservar antes de una ejecución nueva. La fuente Git forma parte de summary.json: cambiar de commit cambia esa metainformación aunque el producto sea idéntico. Los SHA de esta entrega corresponden al head fuente citado. Las comparaciones byte a byte se hicieron entre invocaciones independientes de ese mismo head, no entre commits distintos.

Los datos originales y XML/logs permanecen ignorados en `Logs/experiments/sport/` y `Logs/evidence/sport/`. Ningún log Unity, caché, build, credencial, device.env o identificador de iPhone se versiona. Solo estos datos derivados del experimento y su inventario criptográfico se entregan.
