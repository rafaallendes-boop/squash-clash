# I4a — escenarios y mediciones de física

Fecha: 2026-10-22. Rama `firstball/i4a-physics-measures`, base `c396fe4`. Alcance autorizado: **T4.1–T4.2 únicamente**, con PR para revisión de Claude. Implementación inicial en `cc1e619` y `b5d0d9a`; código final `980529d` tras corregir tres hallazgos de la revisión interna. **EditMode 155/155 y PlayMode 37/37**. No se fusiona ni comienza I4b antes de la revisión de Claude y fusión de I4a. Gate I4c, P2/T4.5, I5 y posteriores sin autorización.

## Implementación

- `PhysicsScenarios`: 17 escenarios fijos (S1–S7, S9a–i, S10), generador S8 reproducible por semilla, con horizonte MaxSteps de 360 pasos a 120 Hz aunque alcance antes dos botes, y 36 lanzamientos dirigidos S8d. Construcción analítica, sin usar el adaptador PhysX para decidir si un escenario está bien construido. AX-1 conservada; S3 lateral derecha y coordenadas/dirección originales.
- S4 y S6 cumplen el requisito de mismo paso con las coordenadas originales. S6 conserva al menos 1 ms hasta los bordes del paso. **No se ajustó z ni ningún otro número de sus inicios.**
- S9: tres líneas, tres márgenes, huella de 10 mm; ±3 mm y borde exacto. Los casos exactos no tienen ExpectTouch: sirven para precisión, no para C11. Tin sigue el TinTop seleccionado. No se cambia el umbral experimental C10 ≤1 mm.
- S8 utiliza posiciones/direcciones/rapideces de la tabla, semilla 20261020 en la prueba de distribución y semillas 7/8 en reproducibilidad. S8d conserva velocidades totales 10/25/40 m/s y resuelve el arco bajo para alcanzar los centros especificados; los escenarios que dicen «horizontal» mantienen esa semántica. No se ha ejecutado una campaña de 10 000 lanzamientos PhysX.
- `PhysicsMeasures`: atravesamientos, salida superior, penetración, finitud de estados, energía mecánica de pasos con contacto, esquinas, vibración, persistencia, igualdad de bits/contactos, impactos por paso, precisión y ratio de tiempo restante.
- `SpikeCriteria` define C1–C18, referencias, escenarios y umbrales sin evaluación del motor. C7 decide; C9 diagnostica. C12–C15 se informan para análisis del gate sin inventar umbrales. C17 solo es gate en iPhone, con p95 ≤1 ms, 200 mediciones y 20 calentamientos descartados conforme a D-2.
- `SpikeResult` y `SpikeReportWriter`: sin aprobación con ejecución incompleta, filas ausentes/duplicadas, gate deshabilitado, veredicto distinto de Pass o cobertura cero/parcial. Markdown conserva requisito, escenarios, medición, umbral, veredicto, gate y evaluados/no evaluables. Una fila ausente se muestra como NotEvaluated. La propiedad no elige backend ni autoriza I4c.

## Verificación inicial

| Ejecución | Resultado | Naturaleza |
|---|---|---|
| Baseline EditMode / PlayMode | 109/109 y 37/37 | Estado de partida de I3 fusionado |
| T4.1 RED | 13 fallos de 13 | Scaffolds tipados que lanzan NotImplementedException, sin errores CS |
| T4.1 GREEN EditMode | 122/122 | Construcción de escenarios + regresión anterior |
| T4.2 RED | 26 fallos, 122 anteriores pasan; 148 total | Mediciones/informe aún no implementados |
| T4.2 GREEN EditMode | 148/148 | Trayectorias sintéticas y catálogo/informe |
| Integración PlayMode | 37/37 | Regresión I3 existente, incluida integración P1 |

Comandos: `scripts/test.sh editmode [PhysicsScenariosTests]`, `scripts/test.sh playmode`. Logs/XML ignorados `Logs/i4a-*`. El primer intento T4.2 tuvo un conflicto de nombre local CS0136 en el test; se corrigió antes del RED válido. `test.sh` rechazó correctamente el XML viejo; ese intento no se usa como prueba RED ni GREEN. Un wrapper inicial zsh rechazó la variable reservada status después de ejecutar correctamente las suites baseline; se corrigió el wrapper, conservando los XML frescos. No hubo fallos de producto en baseline.

## Resoluciones y límites de medición

Estas decisiones son públicas para la revisión de Claude; no cambian umbrales ni aprueban PhysX:

1. Helper de tareas solo reconoce encabezados ingleses: briefs exactos extraídos de «Tarea» y ledger manual. Coste: seguimiento manual respaldado por commits/XML.
2. S8d se interpreta a velocidad total, porque no tiene la calificación «horizontal» de otras filas. Se calcula arco bajo al centro declarado. Coste: Claude debe confirmar esa interpretación antes del experimento; no ajustar según resultados del motor.
3. Duraciones de escenarios en pasos derivadas de StepHz: tres segundos y seis para S10; a 120 Hz son 360/720. Coste: una corrida a otro paso debe conservar esta interpretación explícita. Escenarios de la línea base intactos.
4. Datos numéricos no evaluables → NaN: ningún contacto elegible, tiempo StepOnly/NaN, paso sin estado asociado o múltiples impactos para el ratio de impacto único. Coste: I4b deberá convertir ausencia de dato en NotEvaluated/cobertura no evaluable; nunca en cero favorable.
5. AllGatingPass exige 18 IDs únicos, flags de gate acordes al catálogo, Expected correcto, cobertura cerrada y Evaluated=Expected con cero NotEvaluable en los gates. La versión inicial solo exigía cobertura positiva y quedó corregida tras la revisión. Coste: el runner real debe declarar/cerrar la población completa y sus contadores; lotes reducidos no se aprueban.
6. C17 es informativo solo con Device=`editor` explícito; un dispositivo desconocido no se exime del requisito nativo. Coste: el runner debe etiquetar correctamente su entorno.
7. Esquina considera simétricamente ambos pasos de superficies distintas consecutivas. Stay no cuenta como nuevo impacto/vibración, sí como muestra persistente. Coste: interpretar los diagnósticos respetando esa diferencia; no confundir muestras con impactos.
8. Diferencia de contacto se localiza en el índice de estado asociado. Precisión compara el primer contacto no persistente, sin saltar uno inesperado para buscar un resultado favorable. C12 informa superficie aparte. Coste: el runner debe conservar esa trazabilidad.
9. C1 cuenta una infracción por superficie y estado final de paso, sin tolerancia de penetración. Una salida superior posterior no borra una infracción anterior. Coste: el contador representa muestras infractoras, no número de lanzamientos únicos. Segmentos/estados no reconstruyen movimientos ocultos dentro de un paso que vuelve al mismo lado; esta es la definición observable del plan.
10. AllFinite comprueba Position/Velocity del estado, sin rechazar el NaN legítimo de TimeInStep con StepOnly. Coste: las mediciones de contacto validan su propio dato; C4 ahora rechaza NaN/Infinity/normal no utilizable mediante ArgumentException. No hay certificación general implícita.

C13 aplica la fórmula diagnóstica del plan con punto/normal observados y velocidad normal de salida; no limita el ratio a [0,1] ni añade un modelo de gravedad distinto. La incertidumbre del punto o del instante seguirá visible. No se infiere precisión del centro a partir de precisión de contacto ni se compara editor con iPhone.

## Qué no significa esta entrega

Las suites verifican las mediciones con datos sintéticos y conservan la regresión de I3. **No constituyen resultados C1–C18 sobre PhysX, benchmark ni decisión de backend.** No se creó SpikeRun, PredictionBenchmarkRun, spike.sh, experimento de I4b ni botones nuevos. No hubo build/instalación de app nueva en I4a; el iPhone conserva la dev aceptada `c49d99b`.

I4b empezará solo después de revisión de Claude y fusión de esta PR. Sus resultados que no cumplan se reportarán. Detenerse al entregar el informe de I4b, sin I4c ni P2. Cualquier instalación posterior al 28 de octubre requiere renovar antes el perfil de firma, además de comprobar su vigencia efectiva.

## Revisión final y estado de entrega

La [revisión independiente](../reviews/2026-10-22-first-ball-i4a-internal-review.md) encontró tres Important y ningún Critical/Minor. En el único pase de corrección: S8 deja de terminar tras dos botes; el modelo añade Expected/CoverageComplete y cardinalidades fijas del catálogo; C4 rechaza datos necesarios inválidos. **7 fallos RED reproducidos → EditMode 155/155 GREEN; integración PlayMode 37/37 GREEN.** No se encargó una segunda revisión ni se presenta a Claude como revisor de este código todavía. [Manifiesto de diez ejecuciones XML válidas](evidence/2026-10-22-i4a-tests.json).

Dos resoluciones adicionales: (11) Expected/CoverageComplete amplían el DTO mínimo para impedir casos omitidos; coste: el futuro productor debe rellenarlos y demostrar el cierre de cobertura, sin reducirla. (12) C4 inválido lanza ArgumentException porque su firma devuelve int; coste: I4b debe convertir el caso en no evaluable, nunca en cero ni en aprobación. Las dieciséis áreas que el revisor no evaluó y sus costes se conservan en su informe.

No hay cambios a Runtime, adaptador P1, DefaultPreset, spec, plan, escenarios numéricos ni umbrales. Solo Core/Spike, tests y documentación. No hay resultados de experimentos para el gate: los C1–C18 del motor siguen sin evaluar. I4b permanece condicionado a revisión de Claude y fusión de I4a.
