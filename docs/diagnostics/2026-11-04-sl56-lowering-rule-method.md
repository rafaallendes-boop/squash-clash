# Evaluación offline de «bajar el punto» — método predeclarado

Fecha: 2026-11-04, America/Santiago. **Confirmar este método en Git antes del primer cálculo de la candidata.** El PO comunica aprobación de Claude dePR#31/head546b184 sin correcciones y autoriza evaluar la candidata: bajar el punto, conservando x y P pedidas. No es una elección de regla o parámetros de producto, ni autoriza implementación. C4 queda solo como nota de diseño.

## Prioridad, entradas y alcance

Tandas2–4 dePR#30 y restauraciones antes del perfil2026-11-07T17:58:46Z tienen prioridad. La sesión permanece pausada hasta aviso delPO; solo se calcula sin sesión física activa. Al retomarla elPO se conserva el parcial y se cede prioridad. No acceder al iPhone, relanzar, preparar16, instalar, abrir Editor, compilar producto o correr suites.

Entradas idénticas al mapa aprobado: **73posiciones de contacto (24focales SL5/SL6,49controles SL1–SL4)** y todos sus **99puntos×11P=79.497solicitudes**. Rejilla x={0.02,0.35,1.30,2.60,3.20,3.80,5.10,6.05,6.38}; y={0.50,0.70,0.90,1.20,1.50,1.80,2.20,2.80,3.50,4.00,4.55}; P={0.0,0.1,…,1.0}. Coordenadas/P convertidos a float32 al llamar ShotSolver. Usar el [JSON de posiciones y máscaras del mapa](../validation/evidence/2026-11-04-sl56-feasibility-summary.json), cotejando SHA-256 de entradas/TSV completos y fuentes. No agregar contactos, muestras o candidatos después de ver resultados.

Las lecturas exploratorias de Claude comunicadas por elPO sobre la columnaC y alternativas se conservan **atribuidas, fuera del método anterior y no formales**. No se incorporan como metas de24/24 ni se intenta reproducirlas modificando la regla. La candidata deja intacto un retorno que ya cumple la intersección, aunque sea corto; no optimiza profundidad.

Solo arnés diagnóstico fuera de Assets/tests, en **Logs/lowering-evaluation-20261104**, ignorado y fuera de Logs/evidence. ShotSolver, fábrica, P2, vectores y clasificación geométrica originales del mapa, sin cambios de producto/tests/defaults/SL/spec/plan/bandas/criterios. Biblioteca/compilador/Mono deUnity6000.3.27f1 ya instalado; misma física y horizonte del mapa (120Hz, gravedad/materiales/radios,10s o segundo bote). No variar esos datos ni añadir asistencia.

## Intersección y evaluación de una solicitud

Para cada posición y solicitud(x,y,P), ejecutar ShotSolver original y P2 con la misma función de evaluación del mapa: Solved, primer impacto frontis, primer bote observado dentro del rectángulo y sin out/tin/escape muestreado antes o en ese bote. Se llama **retorno dentro de cancha**, columna diagnóstica existente, no golpe jugable aprobado. Out/tin con huella0.010m y bordes/clasificador originales; escape al final del paso, con su limitación de muestreo. Mantener primer bote(x,z), mitad trasera z≥4.875m, paso de retorno después del primer frontis, pared trasera finita antes del bote, laterales y límites de planos no finitos.

Recalcular el brazo base con los originales y cotejar sus79.497filas con el TSV aprobado antes de publicar; discrepancia→detener/investigar sin cambiar física o baseline para pasar. Mantener el derivado anterior intacto. Registrar Unsolved/censura/cap y no inventar una salida de ceros. Permitir caché determinista **por contacto**, indexada por bits float32 de x/y/P; no fusionar denominadores de posiciones repetidas. Cada evaluación única y referencia de caché queda trazable; reducir trabajo repetido no crea nuevas muestras.

## Brazo Y — candidata de bajar el punto

1. Si la base ya es retorno dentro de cancha, **no tocar la solicitud**, aunque su bote sea corto. Reutilizar el mismo resultado, incluidas las asistencias existentes.
2. Si no, mantener exactamente la x y la P pedidas y probar y descendente con **paso diagnóstico de0.05m**. Para entrada y decimal de la rejilla, candidato k es float32(y_decimal−k·0.05), k=1,2,…; usar cálculo desde el origen, no resta float acumulativa. No repetir valores float32 duplicados.
3. Límite inferior abierto: **borde superior del tin+huella existente**, usando TinTop original y radio0.010f. No pedir candidatos por debajo o en ese límite. Tras la secuencia de0.05m, probar una sola vez el primer float32 estrictamente mayor que dicho límite como extremo final, si no se probó ya. No introducir una separación jugable nueva: EdgeEpsilon del clasificador original sigue vigente y puede hacer que el extremo final toque tin; en ese caso falla su intersección como cualquier candidato.
4. Seleccionar **el primer candidato probado** cuyo resultado sea retorno dentro de cancha: es el punto solicitado más alto de **esta búsqueda discreta**, no garantía de máximo continuo ni del mayor TargetUsed entre todas las asistencias. No buscar un bote trasero adicional ni seguir bajando para mejorar profundidad una vez hallado.
5. Si ningún candidato pasa, marcar **sin solución encontrada en la búsqueda predeclarada**, sin clamping adicional, subir/bajarP, cambiarx o asistencia inventada. No afirmar imposibilidad continua en los huecos de0.05m.

El paso0.05m es un valor de este diagnóstico, no parámetro de producto elegido. No se afina tras observar resultados. La última separación al extremo abierto puede ser menor al paso y queda registrada.

## Brazo P — contraste de bajar la potencia

Aplicar idéntica guardia de no tocar la base que ya retorna dentro. Si no, mantener exactamente x/y pedidos y probar P descendente con **paso diagnóstico0.05**, desde la P decimal de rejilla, hasta0 inclusive. En la rejilla inicial cada P=i/10; candidatos se calculan como (2i−j)/20 en float32, j=1,…,2i, sin resta acumulativa. Para P0 no hay candidato menor.

Elegir la **P más alta entre las probadas** que dé retorno dentro de cancha, sin optimizar profundidad. Si ninguna, sin solución encontrada; no subir y, cambiarx ni permitir potencia negativa. Este contraste tampoco fija valores de producto ni reproduce por obligación la exploración informal.

## Interacción con asistencias existentes

No modificar SpeedRaised/TargetLowered ni excluir por sí sola una solución asistida. Registrar para base y cada resultado final: solicitud original, solicitud tras la regla, TargetUsed real del solver, velocidad pedida6+22P, velocidad final, flags/razón y clasificaciones P2.

Separar:
- **Δy de la regla:** y_original−y_solicitada_final (cero en el brazoP).
- **ΔP de la regla:** P_original−P_solicitada_final (cero en el brazoY).
- **Δy total visible potencial:** y_original−TargetUsed.y final, que puede incluir TargetLowered preexistente.
- Cambios de SpeedRaised/TargetLowered frente a base y coincidencia/divergencia entre punto solicitado al solver y TargetUsed.

Conservarx/P pedidas enY no implica que la velocidad final no sea elevada por la política existente; dejarlo explícito. EnP puede actuar TargetLowered preexistente aunque la regla mantenga y solicitada. No ocultar esas diferencias como si fueran una regla que solo cambia una magnitud física.

## Tablas de resultados y denominadores

Tres brazos: base, Y y P; mismos79.497identificadores, n por grupo focal/control, cada contacto yP original. Diferenciar **guardia preservada**, **búsqueda activada**, **búsqueda con solución/cambio aplicado** y **sin solución encontrada**. La frecuencia de búsqueda y la de cambio exitoso son distintas; no eliminar fallos del denominador.

Misma tabla por brazo: retorno dentro, primer bote trasero, pared trasera antes del bote, paso trasero del retorno, laterales finitas/planos fuera de extensión/out/tin/escape/primer bote fuera. Indicar cuándo un resultado es posterior a un cambio frente a base o preservado. Sin solución no tiene trayectoria final; registrar aparte de salida fallida de la base, con último candidato y cantidad de intentos. No sustituirla automáticamente por baseline.

Describir Δy/ΔP y distancia del TargetUsed/primer impacto al borde tin+huella con n, mínimo, p5/p50/p95/p99/máximo, por grupo/P. Percentiles nearest-rank ceil(p·n). Separar preservadas, cambios exitosos y sin solución; no llenar ausencia con0. Dar Δy de regla y total, no mezclarlos en una sola distribución.

**Cerca del tin:** no inventar una etiqueta booleana o umbral nuevo. Reportar distancia continua al límite, distribución y los20casos más próximos (paginación, no corte de aceptación), en especial con P originales0.8/0.9/1.0. Indicar si TargetUsed y primer impacto discrepan o la huella toca el tin. No presentar proximidad como margen seguro garantizado.

## Continuidad sin criterio nuevo de «brusco»

Analizar por contacto todos los pares vecinos de la rejilla inicial, para cada brazo:
- P consecutivos (ΔP0.1), misma x/y.
- x consecutivas, misma y/P; conservar la separación real no uniforme.
- y consecutivas, misma x/P; conservar la separación real no uniforme.

Comparar solicitud final y TargetUsed de la salida, Δy de regla y ΔP. Para pares con ambos resultados disponibles reportar diferencias absolutas y con signo, pendiente respecto de la separación de entrada, y exceso de cambio de altura frente al cambio de y solicitado (para distinguir seguir la entrada de un cambio de la asistencia). Mostrar distribuciones y los20mayores cambios por eje/grupo con identificadores originales, sin seleccionar solo los casos favorables. Para P también mostrar saltos de P final en el contrasteP.

Contar transiciones preservada↔cambiada y solución↔sin solución por separado. No convertir una ausencia en y0/P0 ni incluirla en percentiles numéricos. Reportar no finito/censura explícitos. **No definir un umbral nuevo para llamar errático/brusco ni afirmar percepción humana medida**; los saltos son evidencia descriptiva para decisión posterior.

## Dejadas, controles y columna C

Subconjunto diagnóstico predeclarado de puntos bajos/P baja: **y∈{0.50,0.70,0.90} y P∈{0.0,0.1,0.2}**, todas las x y posiciones. Es una sección de la rejilla compatible con banda suave existente, no una definición nueva de intención «dejada» ni tarea humana. Comparar preservación/acción, punto/P/TargetUsed, profundidad del bote y alteraciones existentes. Si cambia algún caso, registrarlo; no forzar que no se deformen.

Reportar controlesSL1–SL4 tanto en toda la rejilla como en la **columnaC original(x3.20,y2.20)** porP. Comparar con la misma columna en24focales para contextualizar la exploración atribuida aClaude. «Fondo» se mantiene separado en bote trasero y pared trasera, con z exacta; no equiparar ambos para alcanzar24/24. La preservación de retornos cortos ya válidos puede diferir de una exploración que baje el punto aun cuando ya estaba dentro.

## Reproducción, integridad y entrega

Salida completa por solicitud/brazo y candidatos únicos en Logs/lowering-evaluation-20261104; resumen sanitizado por grupo/contacto/P, continuidad y cercanía al tin en docs, más arnés exacto documentado. Registrar hashes de fuentes, método, rejilla, entradas y salidas, y cache hits/evaluaciones únicas por contacto. Repetir la evaluación completa una vez y comparar salidas deterministas byte a byte; control de reproducción, no suite ni nueva muestra humana.

No refinar paso, endpoint, rejilla, intersección o brazoP después de resultados. Errores del arnés se conservan y documentan, sin atribuirlos aP2 ni cambiar datos para alcanzar una cifra. Código de producto/tests/defaults/SL/spec/plan/criterios y Logs/evidence originales intactos. C4 solo nota: habría que distinguir punto pedido, pedido tras regla y TargetUsed en una visualización futura, que no se diseña ni implementa aquí.

Entrega enPR#31 para nueva revisión Claude/PO, sin fusionarPR#30/#31. No aprobar regla ni elegir parámetros; S3–S4/T8 sin implementación. Sesión física y perfil conservan prioridad.
