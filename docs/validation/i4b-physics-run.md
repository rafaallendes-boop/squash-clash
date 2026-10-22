# I4b — ejecución de la prueba física P1

Estado: **ENTREGA PARCIAL; pendiente de iPhone y clasificación de Claude**. [PR #4, borrador](https://github.com/rafaallendes-boop/squash-clash/pull/4), rama `firstball/i4b-physics-run`, base `6fd04e0`, código `ee76086`. I4a cerrado con merge commit `ca50142` tras las condiciones de Claude/PO (`3c66bad`, EditMode 159/159, PlayMode 37/37). S8d confirmado como velocidad total con arco bajo. I4b no se fusiona ni cierra con esta entrega.

## Implementación y regresión

Runner incremental de T4.3: presupuesto por llamada, trabajos indivisibles de trayectorias completas, dos instancias reutilizadas y una tercera nueva para comparaciones; cancelación/excepción libera todo. Esperados y cobertura explícitos: datos ausentes no aprueban. S8/S8d usan horizonte completo de tres segundos, S10 seis. C3 separa no aplicable de dato inválido y conserva globalmente todos los pasos de esquina, incluidos solo persistentes; muestra máximos medidos separados y rapidez antes/después. C15 incluye los pasos de las dos primeras superficies distintas.

Benchmark: 20 calentamientos descartados, 200 mediciones con Stopwatch; incluye un Reset (dentro de TrajectoryRunner), pasos y contactos, excluye creación de escena. Mediana convencional y percentiles por rango más próximo. C17 solo decide en el iPhone.

T4.4 añade Escenario (S1–S10, S8 representativo y nueve S9) y Prueba física con presupuesto de 8 ms/frame. Libera las escenas del host, espera descarga, muestra progreso/fase/Cancelar; después ejecuta el benchmark. Conserva pantalla encendida, restaura sleepTimeout y gravedad al terminar/cancelar; foco/segundo plano cancela con INCOMPLETO. Informe mostrado, copiado y guardado en Documents/spike. Errores de archivo/portapapeles se muestran sin impedir cerrar el informe ni reponer el host pausado. El host conserva el estado/horizonte del escenario para repetirlo, sin otra autoridad sobre la pelota.

El estado térmico iOS se lee con un puente mínimo a NSProcessInfo; inicio nativo bloqueado hasta nominal. Exclusión real del puente de prod con PluginImporter.SetIncludeInBuildDelegate, además de la exclusión del ensamblado DevPanel. Exports nuevos RED (prod incluía un archivo nativo) → GREEN (dev incluye, prod excluye; módulos C# correctos). Repetidos sobre `ee76086`, ambos metadata limpia: [dev](evidence/2026-10-22-i4b-release-dev.json) y [prod](evidence/2026-10-22-i4b-release-prod.json). No se cambian defines persistentes, presets ni configuración física base.

**Regresión final: EditMode 170/170, PlayMode 43/43**, cero omitidos/fallos. [Manifiesto RED/GREEN](evidence/2026-10-22-i4b-tests.json). Una revisión interna fresca detectó dos Important (mediana par C13, recuperación tras fallo de escritura); ambos reproducidos RED→GREEN y corregidos en un único pase, sin segunda revisión. No sustituye la revisión de Claude. [Revisión y resoluciones](../reviews/2026-10-22-first-ball-i4b-internal-review.md).

## Campaña y artefacto nativo

Cinco variantes completas, **1/1 experimento por variante**, 10.053 lanzamientos de robustez cada una; limpieza de recursos/escenas y gravedad comprobadas. Ninguna cumple todos los criterios físicos. XML 1/1 solo acredita experimento completo: los Fail y NotEvaluated están en el informe. Enhanced Determinism restaurado al finalizar. [Informe físico con comparación y clasificación pendiente](../reviews/2026-10-22-first-ball-physics-spike-P1.md), [manifiesto verificable de las cinco ejecuciones](evidence/2026-10-22-i4b-editor-runs.json).

Baseline/speculative se completaron en `cd7fc2a`; la mediana C13 par de baseline se corrige en copia derivada, recuperando sus dos muestras exactas, sin tocar ninguna otra fila/umbral/población/veredicto. Originales conservados. Speculative tiene muestra impar y no requiere corrección. Offset05/solver12/no-ed se ejecutaron en `ee76086`, que guarda ratios C13 crudos. [Procedencia y auditoría](evidence/2026-10-22-i4b-editor-existing-runs.json). Los timings del editor son informativos y concurrentes, no certifican C17 nativo.

Build dev final **0.1.0+ee76086**, limpia, Xcode 27.1 Release ARM64 con firma verificada, metadata embebida comprobada. Variante baseline/hash c86243b15a97 preparada, sin promover backend. **Instalada y abierta** en iPhone 16 Pro/iOS 26.6.2 tras reconexión del PO: Escenario S1→S2 y cancelación comprobados en Duplicación. Informe cancelado INCOMPLETO y nominal→nominal. La campaña completa y C17 nativo siguen pendientes. [Evidencia de compilación/firma sin identificadores personales](evidence/2026-10-22-i4b-native-dev.json).

Artefacto local listo en `Builds/DerivedData-dev/Build/Products/Release-iphoneos/SquashClash.app`. Perfil vence `2026-10-28T14:59:31Z`; comprobarlo antes de instalar y renovar si venció, y antes de cualquier instalación posterior al 28 de octubre. No se instaló otra herramienta ni se cambió el mínimo iOS; el warning libc++ de E1 sigue documentado y no impidió enlazar.

## Pendientes que impiden el cierre

1. Instalación/arranque, Escenario S1→S2 y Cancelar ya observados en nativo; pérdida de foco nativa también canceló un segundo intento a los 20 s (nominal→nominal). No se atribuye al callback de pausa; los tests cubren la ruta de segundo plano. Capturas e informes INCOMPLETO conservados.
2. Tres intentos INCOMPLETO conservados, cancelación manual y dos pérdidas de foco; [manifiesto nativo](evidence/2026-10-22-i4b-iphone-runs.json). Campaña completa baseline/hash c86243b15a97 en teléfono nominal al empezar; térmico final, metadata y 200 mediciones C17, recuperación del informe de Documents/spike y comprobación de sus 18 filas.
3. Clasificación de Claude del paso 6 de T4.3: adaptador, prueba o limitación del motor para cada incumplimiento; no atribuirla por cuenta propia. Si exige corrección, repetir evidencia afectada.
4. Completar informe y detenerse para el PO/Claude. No aceptación del PO, cierre/fusión de I4b ni decisión de gate implícita.

No se usa task-done para marcar completas T4.3/T4.4 mientras faltan esos entregables. El ledger/briefs de esta ejecución se conservan temporalmente para retomarla; están ignorados y las resoluciones duraderas quedan aquí. No se repiten suites por cambios solo documentales: las suites finales corresponden al código inalterado `ee76086`.

## Rulings I made

Resoluciones del ejecutor en orden, con coste si resultan incorrectas. No son nuevas decisiones de gameplay del PO.

1. C7 sin checkpoint observable conserva el caso como no evaluable, incluso por fin/salida superior legítima. Coste: no aprobación conservadora pendiente de interpretación de Claude/PO; no se excluyen casos silenciosamente.
2. Reset del benchmark es el único de TrajectoryRunner; no se añade un segundo. Coste: una definición distinta exigirá volver a medir, sin cambiar umbral.
3. Dos fixtures previos de pausa comparan DroppedSteps con el contador antes de pausar, en vez de suponer cero tras cargar escena. Reloj inalterado. Coste: esos tests no evalúan drops del arranque; sí ausencia de drops nuevos durante pausa/reanudación.
4. Tiempos concurrentes del editor solo informativos, sin extrapolar a C17 nativo. Coste: pueden ser pesimistas; decide la medición del teléfono.
5. Briefs Tarea 4.3/4.4 extraídos íntegros manualmente porque task-start reconoce encabezados ingleses. Coste: trazabilidad administrativa contrastada con plan/commits/XML.
6. Única revisión fresca adelantada al final de código GREEN mientras acababan datos y se esperaba teléfono. No implica tareas terminadas. Coste: una corrección de medida exige invalidar/corregir los datos afectados, como C13.
7. Clasificación física adaptador/prueba/motor reservada a Claude/PO. Coste: puede exigir nueva corrección y repetir campañas; informe aún parcial.
8. Gate, promoción/congelación de variantes y P2 reservados al PO/I4c. Coste: espera deliberada antes de continuar esa física.
9. Escenarios, umbrales, huella y definición de energía aprobados conservados. Coste: P1 puede incumplir; no se reinterpreta para aprobarlo.
10. Igualdad de contactos conserva superficie/paso según T4.2, junto con estados bit a bit. Coste: C5–C8 no certifican igualdad de punto/normal/tiempo; C10 y C12–C15 diagnostican aparte.
11. Pérdida de intención de pausa manual diferida a I5 conforme al PO. Coste: persiste ese comportamiento conocido.
12. Tamaño/ayudas visuales de pelota diferidos a T6.2 conforme al PO. Coste: persiste legibilidad insuficiente conocida.
13. No reabrir presets corruptos/almacenamiento de I5 no modificados en I4b. Coste: no hay cobertura nueva de ese sistema.
14. No ampliar host a Step arbitrario fuera de int: controles autorizados usan escenarios Step0. Coste: un futuro llamador deberá validar o ampliar antes de usar ese rango.
15. Reutilizar baseline/speculative anteriores con procedencia explícita: baseline deriva solo mediana C13 desde dos float32 recuperables; resto de filas auditadas iguales. Coste: no equivale a nueva corrida del binario final; una discrepancia de auditoría obligaría a repetirla.
16. Preparar baseline como referencia nativa provisional porque ninguna alternativa domina los criterios. Coste: una revisión puede pedir una segunda variante (máximo dos); no congela ni elige backend.
17. Conservar ledger y no marcar tareas completas mientras faltan clasificación y teléfono, aunque la revisión de código terminó. Coste: limpieza/cierre diferidos; no hay falsa aceptación.

**Deferred minors:** ninguno. Las áreas que el revisor dejó sin juzgar están recogidas arriba y en su informe, con su coste.

## Límites

Los fallos de P1 son datos. No se reconstruye Rigidbody, cambia integrador, calibran escenarios, relajan umbrales ni elige backend para hacer pasar resultados. I4c, P2/T4.5 e I5+ no autorizados. Spec/plan originales sin modificar. El worktree principal de I4b se conserva para Claude y el checkpoint del iPhone.
