# Evaluación previa de margen 0,05 m — método predeclarado

2026-11-04. Confirmar enGit antes del primer cálculo. Base de productoPR#30/head8f74e5c; evaluación aprobadaPR#31/head84ad994 y método140d0ca. [Método aprobado](https://github.com/rafaallendes-boop/squash-clash/blob/84ad994/docs/diagnostics/2026-11-04-sl56-lowering-rule-method.md). Este documento incorpora por referencia **todas** sus entradas, clasificación, brazos, búsquedas, guardias, horizonte, distribuciones, continuidad, subconjunto bajo/suave, columnaC y verificación, sin otra variación.

## Única variación

En el arnés C# aprobado SHA-256be9c9ed0c54686fd535675f269c7177f7d695fa952ad7e266820f7110d6945e0, cambiar **solo** la asignación del límite de la búsquedaY:

```csharp
// anterior
double limit=(double)court.TinTop+(double).01f;
// esta evaluación
double limit=(double)court.TinTop+(double).05f;
```

El extremo final sigue siendo el primerfloat32 estrictamente superior al límite, si menor quey original y no probado. Paso0,05m, selección primera petición válida, x/P invariantes, brazoP idéntico. No modificar ShotSolver/P2 ni la intersección delmapa: huella del clasificador0,010f/EdgeEpsilon/radiofísico0,020f intactos. El margen0,05 limita **candidatos nuevos de la búsqueda**; la guardia sigue preservando una base ya dentro incluso si su objetivo es menor quetin+0,05. No imponer el margen a la base, pues cambiaría guardia/base/comportamiento desactivado autorizados.

## Entradas y salidas

Mismas73posiciones/99puntos/11P=79.497solicitudes porbrazo,238.491filas base/Y/P. Copiar positions.tsv aprobado SHA-25668ccf9fc4f1733a9199b3430f45a6623658977723e44941a8e3474ccc3fb82c1. Base igual alTSV aprobado SHA-256913551248ac43ad9e029dfbcd5d6551f9cf19cf9775faeb0e7b3a2aaaba69b43. Arnés completo/fuentes provienen del diagnóstico aprobado sin cambios adicionales; registrar diff exacto y hashes antes de ejecutar. No acceder al iPhone ni tocar Logs/evidence. Salidas nuevas ignoradas enLogs/margin005-20261104 del worktree de la regla.

Repetir las dos corridas completas y cotejar los cincoTSV byte a byte, base igual a aprobada; brazoP/resultados también iguales salvo ids de caché que puedan desplazarse por la nueva búsquedaY. Agregar como antes n porgrupo/P/contacto, sin solución/retornos dentro/bote trasero/pared/laterales, distribuciones Δy/ΔP/TargetUsed/márgenes y continuidad de todos los pares vecinos, sin cortes nuevos. Margen usado en estadísticas de cercanía sustituye0,010 por0,05, manteniendo también clara la frontera física/clasificadora original. Las bases preservadas pueden tener distancia negativa a este nuevo límite: no es out/tin nuevo.

## Lectura previa al código

Comparar con resultados aprobados, informando cambios exactos, sin seleccionar solo casos favorables. El juicio de si cambia el panorama no se convierte en un umbral numérico nuevo. Si la variación altera sustancialmente la utilidad o descubre una incompatibilidad de la recomendación (por ejemplo, deterioro de retorno/profundidad/continuidad que cambie la decisión), detenerse y avisarPO antes de implementar. No ajustar margen/paso/guardia para recuperar cifras. Cualquier duda de producto vuelve alPO. Si conserva el panorama, proseguir TDD autorizado dejando el fundamento explícito en el informe.

La lectura exploratoria/interpretaciones deClaude se registran atribuidas, sin recalcularlas como mediciones. No implementación antes del cierre de esta comparación; no aprobación física inferida. Tandas2–4 dePR#30 yrestauraciones antes de2026-11-07T17:58:46Z tienen prioridad, trabajo solo sin sesión física y pausa al avisoPO. PR#30/#31 sin fusionar; nuevaPR propia sinfusionar.
