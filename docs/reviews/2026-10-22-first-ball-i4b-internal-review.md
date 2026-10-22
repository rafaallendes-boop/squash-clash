# Revisión independiente de código I4b

Revisor: agente fresco `i4b_code_review`, GPT-6-astra/high; lectura estática de `6fd04e0..cd7fc2a`. Sin subagentes propios, Unity, modificaciones de archivos ni Git. Una revisión del código completo; se adelantó mientras terminaban los datos y se esperaba conexión del teléfono, sin declarar terminadas T4.3/T4.4 ni validación nativa. No sustituye a Claude/PO.

Resultado: **0 Critical, 2 Important, 0 Minor**. Fortalezas: propiedad y liberación de instancias, separación entre ejecución completa y aprobación física, poblaciones de C1/C5–C8, benchmark con Reset incluido/escena excluida y verificación real de exclusión prod. Los tests previos de pausa siguen verificando ausencia de nuevos drops.

## Important y único pase de corrección

1. `SpikeRun.cs:232` en cd7fc2a: C13 elegía el central superior en muestras pares. Con dos ratios1,237019/1,2733072, la mediana convencional es aproximadamente1,2551631. Corrección: promedio de centrales en double y retorno float; impar conserva central. Se conservan ahora los ratios ordenados con formato R para auditoría. RED válido: población par esperaba50,11364 y recibía66,81818; impar ya correcto. GREEN EditMode170/170.
2. `SpikeSection.cs:123–134` en cd7fc2a: fallo de Directory.CreateDirectory/File.WriteAllText después de IsRunning=false impedía mostrar/cerrar el informe y reponer el host; el catch posterior no podía cancelar. Corrección: recuperación del host/estado antes de salidas, informe en memoria y cierre visibles; archivo y portapapeles con errores aislados y mensajes veraces. RED válido en finalización y cancelación: IOException de archivo temporal ocupando ruta de directorio; dos fallos esperados, sin tocar informes del usuario. Se verifica host repuesto/pausado, gravedad/sleepTimeout restaurados, informe visible, cierre disponible y error explícito. GREEN PlayMode43/43; EditMode170/170.

La prueba de finalización inyecta el resultado en la frontera privada de salida por reflexión para no ejecutar10.000 lanzamientos físicos dentro de regresión. El directorio opcional de Initialize permite una ruta temporal real; el flujo nativo mantiene Documents/spike. No se añade selector de backend ni framework de DI.

## Tratamiento de datos anteriores

Las ejecuciones completas cd7fc2a de línea base y especulativa se conservan con hashes. En la línea base, N=2 permite recuperar exactamente las dos muestras float32 desde el mínimo y central superior emitidos con formato R; se deriva una copia con C13 corregido y se comprueba que no cambie ninguna otra fila, población, umbral ni veredicto. En especulativa N=15 es impar y C13 ya era correcto. El manifiesto identifica origen, método, muestras y hashes; no se presenta como una nueva ejecución completa del binario corregido. La corrección de UI no altera mediciones. La corrida solver12 apenas iniciada se detuvo; no es un resultado físico ni se utiliza como completa. Offset05, solver12 y no-ed se completaron con el código corregido ee76086; las cinco corridas válidas constan en el manifiesto del editor. Otra ejecución no-ed redundante se interrumpió y no se usa como resultado completo.

## Áreas consideradas y resoluciones del ejecutor

| Área que el revisor dejó sin juzgar | Resolución y coste si es incorrecta |
|---|---|
| Atribución de fallos a adaptador/prueba/motor | Pendiente de Claude/PO, sin inferirla de cifras. Coste: una causa distinta puede requerir corregir y repetir datos. |
| Backend, promoción de variantes y P2 | Reservado al PO/I4c; no se decide. Coste: espera deliberada antes de continuar física. |
| Escenarios, umbrales, huella y energía | Decisiones aprobadas conservadas. Coste: el candidato puede incumplir; no se compensa redefiniendo la prueba. |
| C7 sin checkpoint tras salida superior | Mantiene resolución previa: no evaluable, nunca éxito. Coste: no aprobación conservadora pendiente de interpretación. |
| Ampliar campos de igualdad de contactos | Se conserva superficie/paso del contrato T4.2. Coste: C5–C8 no certifican igualdad de punto/normal/tiempo; C10 y C12–C15 los diagnostican aparte. |
| Pausa manual tras Toca para continuar | Diferida a I5 por el PO. Coste: persiste esa pérdida de intención manual. |
| Tamaño de pelota y ayudas | Diferido a T6.2 por el PO. Coste: legibilidad insuficiente conocida. |
| Certificación de iPhone/térmica/C17 | Pendiente de conexión y ejecución real; firma no la sustituye. Coste: I4b no se cierra aún. |
| Extrapolar tiempos concurrentes del editor | Solo informativos. Coste: pueden ser pesimistas; C17 lo decide el dispositivo. |
| Reabrir pruebas de presets corruptos | Codec y defaults no modificados; almacenamiento de I5 no autorizado. Coste: no hay cobertura nueva de ese sistema en I4b. |
| Host con Step fuera de int o parámetros arbitrarios | Controles autorizados solo usan escenarios con Step0; no ampliar API ahora. Coste: un futuro llamador con contador arbitrario necesitará validar/ampliar antes de usarlo. |
| Declarar T4.3/T4.4 terminadas con datos en curso | No se declara; revisión de código adelantada y borrador explícito. Coste: no hay aceptación/fusión automática. |

No hay Minor diferidos. No habrá una segunda revisión interna: se verifican las correcciones con RED→GREEN y suites completas; Claude revisará la PR según el flujo autorizado.
