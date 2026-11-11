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
