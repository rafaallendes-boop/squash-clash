# First Shot S4 — timing (11 de noviembre de 2026)

## Autorización literal del PO

Autorizo a Codex la etapa S4 del plan First Shot r2, limitada a: Tarea 4.1 (calibración de latencia), Tarea 4.3 (bots de anticipación y experimento H3b en el Mac) y el Step 0 documental de la Tarea 4.2 (propuesta de niveles de ayuda adaptada a la ayuda de impacto de #44). La implementación de 4.2, las tareas 4.4 y 4.5 y cualquier sesión en el iPhone requieren autorización aparte. Spec, escenarios, umbrales y criterios no se modifican. Que no se fusione nada sin mi aprobación.

Autorizo copiar byte a byte scripts/device.env desde el worktree ~/.codex/worktrees/s3-validation/Squash Clash, sin cambios ni versionarlo.


## Base y alcance

Main local, origin/main y referencia remota comprobados en `18b796a615b5bbca856c1624829a8ddc1895e214` antes de crear el worktree. Rama propia `firstshot/s4-timing`. Este registro precede todo código. Tareas autorizadas: 4.1, 4.3 y Step 0 documental de 4.2. Sin implementación de 4.2, 4.4/4.5, iPhone, cambios de spec/planes/criterios/defaults ni fusión. No se editan AGENTS.md ni el registro común de aprobaciones. Sin tocar #32 ni áreas de Fable.

## Estado inicial

Pendiente: TDD de calibración y bots, experimento descriptivo H3b separado de regresión, propuesta de ayudas/tareas y protocolo físico, suites completas, check-release dev/prod y AutoQA. Ninguna ejecución o aceptación se infiere de este registro.

## S4-TIMING-01 — dependencia de H3b; parada antes de código

Detectada durante la lectura inicial de la base autorizada `18b796a`, antes de crear esqueletos o pruebas. El encargo exige informar el vector causal promedio en H3b, separado de contactos y fallos. La capa productora de esos vectores no está implementada: quedó **expresamente diferida fuera de S3** por la decisión 9 del PO en el [diseño de S3](2026-11-05-first-shot-s3-design-plan.md), y su tabla de alcance conserva la calidad causal de la tarea 3.1 como pendiente. El [plan r2, tarea 3.1](../superpowers/plans/2026-10-25-squash-clash-first-shot.md) describe esa implementación aparte.

Lectura de código que confirma la dependencia: [ShotController.Solve](../../Assets/SquashClash/Runtime/Shot/ShotController.cs#L150) pasa directamente posición de contacto, mira y potencia al [ShotSolver](../../Assets/SquashClash/Core/Shot/ShotSolver.cs); [ShotRecord](../../Assets/SquashClash/Core/Shot/ShotRecord.cs) registra intención, solución y contacto, sin los vectores causales de timing/E3/E5. Los ajustes de factibilidad `SpeedRaised`/`TargetLowered` del solver son otra magnitud, no reemplazan esos vectores. Tampoco la diferencia geométrica entre contacto y P* es el vector causal aplicado al golpe.

Esto es una **dependencia pendiente del alcance**, no un fallo de regresión descubierto, una medición nueva ni una revocación de la aceptación condicionada de S3. No es correcto introducir ceros como si fueran vectores medidos ni añadir la capa causal dentro de esta autorización limitada a 4.1/4.3/Step 0 de 4.2.

El encargo indica literalmente: «Ante cualquier defecto o sorpresa: registra y detente, sin ajustar parámetros». Se aplica esa parada. Consulta al PO: autorizar continuar 4.1 y los bots/protocolo de 4.3, con **vector causal promedio de H3b NotEvaluated** por capa ausente y las demás variables disponibles informadas por separado. Se pueden probar las reglas de agregación con vectores sintéticos identificados como tales; eso no acredita la métrica productiva de H3b. No se cambia la hipótesis, criterios ni umbrales, ni se implementa la tarea 3.1.

### Estado en la parada

- Primer commit `82828d9`: solo este documento con la autorización literal, antes de código.
- `scripts/device.env` copiado byte a byte desde el worktree S3 autorizado, ignorado y sin versionar.
- Sin cambios de código, tests, defaults, catálogo, plan, propuesta, spec, escenarios, tolerancias o criterios; AGENTS.md y aprobaciones comunes intactos.
- No ejecutados: RED/GREEN, regresiones, check-release, AutoQA ni experimento H3b. Todos **NotEvaluated** en esta entrega interrumpida; no se declara éxito.
- Propuesta completa de 4.2 y protocolo físico aún pendientes. No implementación de 4.2 ni tareas 4.4/4.5.
- Sin iPhone, Duplicación, builds, instalación, sesión física, cambios en #32/First Rally/First Match ni fusión.
- PR final no creada: la entrega solicitada todavía no está completa ni lista para revisión final de Claude. Rama local preservada para continuar cuando el PO decida esta dependencia.

## Decisión del PO sobre S4-TIMING-01 — continuación autorizada

Respuesta literal: **«Continuar con vector causal NotEvaluated (recomendado)»**, a la consulta que autoriza continuar 4.1 y los bots, dejando el vector causal de H3b NotEvaluated y midiendo las demás variables disponibles, sin implementar la capa causal. Sustituye la parada vigente de la sección anterior, conservada como historia. Se mantienen el resto del encargo y sus límites. Este registro precede todo código.

Método previsto de 4.1: tres series de ocho pulsos medidos, período de un segundo, cuatro pulsos de familiarización por serie; marcas reales de emisión y de evento táctil, emparejamiento al pulso más cercano (empate anterior), sin eliminar outliers. Serie incompleta, ambigua o con condición inválida no propone compensación. Media, desviación muestral por serie, intervalo/rango de medias y tiempos de separación entre series; intervalo del 95 % de la media de tres series con t de Student (2 grados de libertad). Es incertidumbre de repetición bajo este método, no de latencia del dispositivo. Sin calibración física en esta entrega.

## Método predeclarado para H3b (antes de ejecutar)

Experimento Editor explícito, fuera de `SquashClash.Core.Tests` y de las suites de regresión. Se usan las seis recetas SL1–SL6 existentes, ocho semillas 0–7 y cuatro bots (0, −60, −100, −150 ms respecto del T* del plan visible), emparejados entre T3 puro y V=100 ms. T3-suave apagado; ayuda actual de #44 completa, sin otros niveles implementados: parcial/ninguno NotEvaluated. Total previsto: 384 intentos, sin tocar recetas ni física. Mismas semillas/intenciones en ambos modos y todos los bots.

La semilla determina una intención reproducible por intento, no ruido en el desfase fijo. Fixture descriptivo de zona frontis: rectángulo cerrado x∈[2,7;3,7] m, y∈[1,7;2,7] m; mira central (3,2;2,2) con variación determinista de hasta 0,2 m por eje y potencia 0,25/0,50/0,75. Se declara aquí como **zona del arnés H3b**, no como tarea H2 aprobada ni criterio nuevo. Acierto se informa sobre contactos cuyo primer impacto observado es frontis dentro de esa zona y también sobre todos los intentos (fallos no son aciertos). Trayectoria de salida P2 hasta primer contacto/horizonte 10 s; solver original y ajustes de factibilidad registrados por separado.

Por modo/receta/desfase se conservan resultado y motivo del contrato, geometría/paso de contacto, cuantización, salida/ajustes y evento de primer impacto. Se reportan por separado contactos, fallos al aire/tarde, otras categorías, aciertos observados y zona no observada. En pares aceptados por ambos modos se coteja geometría/paso/salida: no se atribuye al límite V una penalización geométrica que no existe. Vector causal productivo: **NotEvaluated**, no cero; pruebas de agregación con vectores sintéticos solo validan el protocolo. No se fuerza desigualdad estricta ni se emite Pass de H3b con la métrica causal y otros niveles pendientes.

## T4.1 — TDD inicial

Esqueleto compilable: primera corrida 0/11 con 10 fallos por aserción y un acceso nulo en el esqueleto; se conserva como intermedia no válida para el grupo completo. Se inicializó el resultado vacío del esqueleto, sin implementar el cálculo: **RED válido 0/11, once fallos por aserción, sin errores CS**. Tras implementar: 10/11; el último caso exigía preservar el toque que invalida timestamps. Corregido el registro sin cambiar la prueba: **GREEN 11/11**. XML/log frescos separados en `Logs/evidence/s4-timing/t4-1`. Son datos sintéticos; la accesibilidad del control se comprueba en Main del Editor. No calibración física ni aceptación inferida.

## T4.3 — TDD del bot y el protocolo

Esqueleto compilable. Un primer intento no produjo XML por usar un matcher no disponible en el NUnit del proyecto; se conserva el log y **no cuenta como RED**. Corregida esa sintaxis de prueba: **RED válido 1/10, nueve fallos por aserción, sin CS**. Implementación sin alterar contrato, reloj, solver ni P2: **GREEN 10/10**. La prueba con vectores usa valores sintéticos declarados, no atribuye esos valores al producto. El reductor mantiene modos separados y ausencia de vector como null/NotEvaluated; un fallo nunca añade una muestra cero. Evidencia en `Logs/evidence/s4-timing/t4-3`.
