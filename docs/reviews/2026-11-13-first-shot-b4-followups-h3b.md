# First Shot — observaciones de #53 y H3b completo (13 de noviembre de 2026)

Rama `firstshot/b4-followups-h3b` desde main `78b2b65`. Operador: subagente Sonnet dirigido por Claude; la revisión de Claude no es independiente de la dirección del encargo.

## Registro del encargo

Instrucción literal del PO:

> «Realiza el mergeo de 53. Luego Realiza un build al iphone, para probar como está actualment (no es prueba oficial). Luego lanza un sub agente sonnet para realizar las observaciones y con h3b y luego dime cuales serían los siguientes pasos. El iphone está desbloqueado»

Hechos verificados por Claude: PR #53 fusionada (merge `78b2b656b3d3bffee9f43d3ac35c99c22e055b94`, padres `09764ab1…` y exactamente `7fb7315a8165697dccdb3bbc67effb421e1a677b`, árbol `971c70a4487b5a4b7bb118f62d34fab63d64542a` idéntico al head; no acepta First Shot). Build informal `0.1.0+78b2b65 dev` instalada el 2026-11-13 ~18:38Z, arranque P0-calib-1, hash efectivo `b6914a349068`, cero pendientes, presets idénticos byte a byte al respaldo; no es prueba oficial ni aceptación. Esta entrega no toca el iPhone ni instala nada.

Alcance: Parte A (A1/A2 con TDD, A3 solo registro) y Parte B (H3b completo como experimento, no regresión). Sin cambios en `causal.*`, defaults, presets, solver, contrato, ventanas ni física P2.

## Método predeclarado para H3b (escrito y commiteado antes de ejecutar)

Contexto: el experimento del 2026-11-11 dejó el vector causal y los niveles de ayuda parcial/ninguna como NotEvaluated. Ambos existen ahora en main (capa causal de #53, niveles 0/1/2 de #50). Plan First Shot r2, Tarea 4.3 Step 2: «con T3 puro y con V, bots a −60, −100 y −150 ms frente a uno preciso, para cada nivel de ayuda; se informan por separado vector causal promedio, tasa de al aire/tarde y acierto de zona. La hipótesis se juzga con esos datos; no se exige desigualdad estricta en cada dimensión ni se ajusta la física para que pase.»

**Se conserva del método original** (`docs/reviews/2026-11-11-first-shot-s4-timing.md`): recetas SL1–SL6, semillas 0–7, bots 0/−60/−100/−150 ms respecto del T* del plan visible, T3 puro frente a V=100 ms, mismas semillas/intenciones en todos los modos, zona del arnés H3b (frontis x∈[2,7;3,7], y∈[1,7;2,7] m; es del arnés, no H2 ni criterio), P2 hasta el primer contacto/horizonte de 10 s, 384 intentos (6×8×4×2). Fixture de intención: mira central (3,2;2,2) con variación determinista de hasta 0,2 m y potencia 0,25/0,50/0,75.

**Se añade:**

1. *Vector causal productivo por la ruta compartida.* Por receta y modo se llama una vez a `CausalQuality.Prepare` con el contexto congelado (trayectoria de llegada, plan, ClockMap, TimingSettings del modo, instante presentado, tempo, marco corporal del plan), y cada intento contactado se resuelve con `CausalQuality.Solve` (la misma función que usa `RallyContact.Solve` y por tanto el juego), con el marco corporal congelado en el paso de contacto. Se informan por separado, solo sobre contactos aplicados (un fallo nunca aporta una muestra cero): media de la altura total y de la fracción de velocidad total, media por causa (timing/stretch/jam), distancia a la zona muerta `d` (media, y recuento con d=0), recuento de objetivo acotado por el límite legal del frontis, y SpeedRaised/TargetLowered del solver. Estos números se presentan **separados** de contactos/al aire/tarde y del acierto de zona.
2. *Acierto de zona por dos rutas.* Se conservan las columnas del método original (solver sin capa causal, para comparabilidad histórica) y se añaden las de la ruta productiva con capa causal. Un intento causal no resuelto se registra como no resuelto (no se descarta ni se cuenta como acierto).
3. *Niveles de ayuda 0 (Completa), 1 (Parcial), 2 (Ninguna) como dimensión.* **Limitación declarada:** los bots no perciben la vista; leen el plan visible y sueltan en T*+Δ fijo. El nivel de ayuda solo cambia la vista (`TimingRingView`: parcial oculta desde `now ≥ target−0,15 s`, ninguna oculta siempre) y no entra en el contrato, el solver ni la capa causal. Por construcción los resultados de un bot son idénticos en los tres niveles; **no se inventa un efecto**. La dimensión se informa como (a) tablas por nivel con las mismas cifras, rotuladas como no independientes, y (b) visibilidad del anillo en el instante de soltar: completa siempre visible; parcial visible solo si el instante de soltar es estrictamente anterior a `T*−0,15 s` (con Δ=−150 exacto queda oculto, igual que la regla de la vista); ninguna nunca visible. Esa visibilidad es un dato derivado aritmético, no un efecto sobre el bot. El efecto de la ayuda sobre una persona solo puede evaluarse en el iPhone.

**Qué se reporta:** por modo × desfase (y por nivel de ayuda, con la salvedad anterior): intentos, contactos, al aire, tarde, otros; acierto de zona observado/contactos y sobre intentos para ambas rutas; el bloque causal de arriba; pares T3/V con contacto en ambos y diferencias de geometría (debe ser cero, como antes). Denominador cero se informa como NotEvaluated, no 0 %.

**Qué no se hace:** no se fuerza Pass/Fail de H3b, no se exige desigualdad estricta en ninguna dimensión, no se cambia la zona del arnés ni física, solver, valores causales o ventanas tras ver los datos. El juicio de H3b es del PO.

**Verificación de reproducibilidad:** dos ejecuciones en procesos Unity distintos y secuenciales, con el árbol limpio (`gitDirty=false`) y el mismo head; los archivos de salida deben ser idénticos byte a byte (SHA-256). Salidas crudas en `Logs/experiments/h3b-causal/` (ignorado); solo se versiona un JSON derivado en `docs/validation/evidence/2026-11-13-b4-h3b/` con los SHA-256 de los archivos crudos. Un solo Unity a la vez. Guarda de configuración: defaults `9041c4aafc77`/118 claves, P0 `b6914a349068`, SimulationKey `166a96e319c7`.
