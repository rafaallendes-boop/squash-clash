# Mapa offline de factibilidad — método predeclarado

Fecha: 2026-11-04, America/Santiago. Este documento se confirma en Git **antes del primer cálculo del mapa**. Autorización del PO comunicada junto con la revisión de PR#31/ef7edb3. La mira permanece como **punto del frontis**; se investiga una futura regla de golpe jugable en el solver con ajuste visible C4, pero no se elige regla, valores de producto ni implementación.

## Prioridad y alcance

Las tandas2–4 de PR#30 y sus restauraciones antes del perfil7nov17:58:46Z van primero. La sesión sigue en pausa hasta aviso del PO. El mapa solo se ejecuta sin sesión física activa; no accede al iPhone, relanza, prepara16, instala o ejecuta tests. Si el PO retoma la campaña, se conserva el cálculo parcial y se cede prioridad, sin presentar el parcial como mapa completo.

No cambiar ShotSolver/P2, producto, tests, defaults, SL, spec, plan, bandas o criterios. Fuentes originales9131b1c y bibliotecas de Unity6000.3.27f1 ya instaladas; arnés diagnóstico y archivos completos de cálculo solo en **Logs/feasibility-20261104**, ignorado y fuera de Logs/evidence. Los datos primarios de evidencia son solo lectura.

## Contactos de entrada

**Grupo focal:** posiciones (centro físico) de los24Applied de10–13 del [derivado offline ya revisado](../validation/evidence/2026-11-04-sl56-p2-offline.json), SHA-256 **fcdcdbb8c87530bb3576380338c38e036f2e9b9f9bb009c75676cde42ad20374**. Comprobar contra las líneas primarias ya identificadas, sin recalcular su simulación anterior.

**Control:** todos los contactos Applied de las diez recepciones medidas de cada condición SL1–SL4 de tanda1:02,03b,04b,05,06,07,08,09. No FAM, parciales originales03/04, extras o gestos sin contacto aplicado. Conservar las80recepciones de control con sus exclusiones y las40 focales, sin seleccionar por buen resultado o sustituir omisiones. La tabla previa implica49Applied de control; discrepancia con primario obliga a registrar antes de calcular, sin forzar49. Si se confirma, serán **73 posiciones** y **79.497 combinaciones**.

Reconstrucción primaria con la misma regla auditada: fronteras de cierre DEV únicos; primera recepción es primer LaunchId posterior al cierre, diez sucesivos. Fronteras02/142,03b/1656,04b/2484 del proceso20261101T143555909;05/329,06/563,07/1655,08/1961,09/2243 y10/2720,11/3192,12/3450,13/3917 del proceso20261101T204549929. Tomar versión final por LaunchId/AttemptId (Version máxima, empate última línea), conservar linaje y solo Applied con estado finito/contactStep válido. Cotejar hashes externos/pending0 según anexo2.7. Los scripts/extracciones anteriores sirven solo de comparación. Registrar SHA-256 de los dos JSONL primarios, origen/ordinal/LaunchId/línea por caso; no publicar identificadores personales del iPhone.

El mapa usa **solo la posición de contacto original**, convertida a float32. La velocidad y P originales se conservan como referencia pero no se reutilizan: cada salida nueva se obtiene llamando ShotSolver con Aim/P de la rejilla. No reconstruir intención humana ni comparar como si el PO hubiese pedido esos puntos/potencias.

## Rejilla congelada de diagnóstico

Coordenadas en metros del frontis; valores decimales convertidos a float32 antes de ShotRequest. La rejilla usa puntos representativos, extremos físicos laterales y la alturaC histórica. Es un muestreo diagnóstico finito, **no un conjunto de valores de producto elegido**, un preset o una nueva tarea humana. No se refina/adapta después de ver resultados.

- **x (9):** 0.02, 0.35, 1.30, 2.60, 3.20, 3.80, 5.10, 6.05, 6.38.
- **y (11):** 0.50, 0.70, 0.90, 1.20, 1.50, 1.80, 2.20, 2.80, 3.50, 4.00, 4.55.
- **P (11):** 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0.

Producto cartesiano: **99 puntos×11P=1089 combinaciones por contacto**. Orden determinista: grupo/bloque/ordinal, x ascendente, y ascendente, P ascendente. El valorP=0 no implica gesto Tap/C-A; aquí es una solicitud matemática al solver, no un intento humano. No filtrar combinaciones que el solver rechace.

## ShotSolver y P2 originales

Compilar el arnés fuera de Assets/tests junto con los13fuentesCore del diagnóstico previo, más **ShotSolver.cs y AnalyticBallSimulationFactory.cs** originales. Usar Mono/csc, UnityEngine.CoreModule y fachada netstandard ya instalados; no Editor, nueva dependencia, Unity Test Runner, stub Vector3 ni reimplementación física Python. Registrar comando y hashes de fuentes/DLL/arnés/binario/entradas/salidas.

Para cada posición/Aim/P ejecutar **ShotSolver.Solve** con vMin6, vMax28, vCap32m/s y la física aprobada:120Hz, gravity9.81float32, radio0.020m, masa0.024kg, máximo4impactos/paso, rebote mínimo0.1m/s, piso restitución0.45/fricción0.30, paredes0.55/0.20; cancha original6.40×9.75m, paredes6m, tin0.48m. Cotejar con defaults/P0 originales sin modificar archivos. Sin causalidadS3, aleatorio, spin, corte, regulación de trayectoria posterior ni cuerpo nuevo.

Registrar Solved/Reason, SpeedRaised, TargetLowered, velocidad solicitada6+22P, velocidad final y TargetUsed. **Separar objetivo/P pedidos de la solución asistida existente**: un resultado asistido no prueba factibilidad exacta de la solicitud original y no se oculta al resumir zonas. Unsolved no se convierte en salida física ni en trayectoria de ceros.

Para Solved, Reset P2 en posición original y velocidad del solver, paso0 relativo al contacto. Integrar pasos completos hasta el paso del segundo bote no persistente o1200pasos/10s, lo que llegue primero. Continuar después de un out geométrico para describir el primer bote; etiquetar continuación contrafactual. Conservar contactos persistentes aparte; no son nuevos botes. Registrar cap alcanzado; no elevarlo. Equivalente real t/0.65 solo descriptivo, no nueva medición nativa.

## Clasificaciones separadas por combinación

No se programa una regla de golpe jugable. Se construyen **columnas geométricas descriptivas**, sin declarar Pass/Fail de producto ni elegir cuál debería usar una futura regla:

1. **Primer impacto:** superficie y tiempo del primer contacto no persistente, si existe; igualdad frontis por separado. Si no aparece dentro del horizonte, no observado.
2. **Out de pared:** mismo método revisado de PR#31, huella0.010m de la mira actual y franjas/bordes originales. CourtLines/LineContactClassifier originales; punto en/sobre semiplano superior limitado a extensión de pared, o huella toca franja. Marcar primer out, superficie, y si ocurre antes del primer bote. Contacto de un plano P2 fuera de su extensión real se registra como limitación, no como out de pared finita. Tin/bajo del frontis se registra también para no llamar retorno dentro de cancha a una infracción geométrica inferior.
3. **Envolvente:** si estado al final de algún paso supera altura6m o sale del rectángulox/z. Guardar primer instante muestreado, y banderas antes del primer bote y en todo horizonte. El muestreo de paso puede no detectar una excursión menor entre pasos; no se promete prueba continua de escape. No modelar techo ni paredes infinitas nuevas.
4. **Primer bote:** punto(x,z), tiempo, estado dentro/fuera del rectángulo y observación/censura. Suelo P2 no acotado: un bote fuera no es legal ni se presenta como llegada jugable.
5. **Mitad trasera:** reportar **primer bote conz≥9.75/2=4.875m** por separado de **pasóz≥4.875 antes del primer bote** (estados al final de paso/contactos). Es división geométrica, no short line ni criterio nuevo. Registrar lado de un empate exacto como≥.
6. **Fondo:** no inventar un corte de profundidad. Reportar la z exacta del primer bote y, separadamente, si hay **contacto en pared trasera finita** antes del primer bote, con legalidad geométrica disponible. No equiparar automáticamente mitad trasera, bote después de out o contacto en plano fuera de ancho con llegada válida al fondo.
7. **Laterales:** superficies/cantidad antes del primer bote y en todo horizonte, distinguiendo las que ocurren fuera de la extensión de pared. No descartar una combinación por tocar una lateral: la columna es descriptiva y el PO todavía no eligió regla.

Para poder leer el mapa sin combinar a mano las columnas, publicar una **intersección derivada descriptiva**: Solved, primer impacto frontis, sin out/tin antes del primer bote, sin escape muestreado antes del primer bote y primer bote observado dentro del rectángulo. Llamarla **retorno dentro de cancha en este diagnóstico**, no «golpe jugable aprobado». Mantener además máscaras separadas para solución sin ajustes frente a solución con asistencia. Su composición no se impone a una futura regla y no altera criteriosS2.

## Resumen y trazabilidad

Salida completa por combinación (TSV/JSONL determinista) en Logs/feasibility-20261104, con identificadorgrupo/bloque/ordinal, índicesx/y/P, solución y todas las columnas anteriores. Cálculo de resumen desde esa salida, sin repetir el solver para elegir zonas.

Versionar un JSON **derivado sanitizado** con rejilla/posiciones/orígenes/hashes y, por contacto y punto, máscaras de11bits correspondientes a los11P en orden: solved, sin ajustes, retorno dentro de cancha, ese retorno con primer bote en mitad trasera, paso a mitad trasera y pared trasera antes del bote. Máscaras de asistencia separadas y conteos por motivo; tabla con decodificador y denominadores para no confundir huecos de rejilla con imposibilidad continua. Una máscara0 no demuestra que no exista una solución entre puntos/P muestreados.

Informe por contacto: zonas deAim yP que producen retorno dentro de cancha, cuáles alcanzan mitad trasera, z observada de bote y pared trasera. Comparación focal/control con n explícitos; no tratar contactos correlacionados del mismo vuelo/SL como una población aleatoria ni inferir aceptación humana. No seleccionar una zona como regla/recomendación/default.

Repetir una vez el cálculo completo final y comparar salidas byte a byte como control de reproducción, no test ni nueva muestra. Fuentes/primarios/evidencia solo lectura, hashes antes/después y diff protegido vacío. Cualquier fallo de arnés o ajuste de método se conserva y documenta antes de publicar; no modificar datos para alcanzar una conclusión. No nueva simulación de los24estados originales de salida para la corrección21/24: esa corrección solo usa el derivado previo.

## Fin del alcance

Entregar mapa e informe documental en PR#31 para Claude/PO, sin fusionar PR#30/#31. La elección de regla, valores y C4 requiere otra decisión. S3 sigue solo plan; S3–S4/T8 sin implementación. Pausa física y prioridad del perfil vigentes.
