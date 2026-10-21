# First Ball — registro de aprobaciones del Product Owner

Fecha: 2026-10-21. Origen: instrucciones directas del PO en la conversación de inicio oficial de First Ball. Este registro conserva la trazabilidad sin modificar la spec v0.3, el plan r2 ni las revisiones originales.

## Documentos de referencia

- [Spec v0.3 con enmienda E.0](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md).
- [Plan First Ball r2](../superpowers/plans/2026-10-20-squash-clash-first-ball.md).
- [Propuestas de Claude, secciones 4.1 y 4.2](2026-10-20-first-ball-plan-review-response-claude.md).
- [Revisión técnica de Codex](2026-10-20-first-ball-plan-review-codex.md).

## Decisiones aprobadas

El PO aprobó D-1 a D-6 conforme a las propuestas de Claude:

| ID | Decisión vigente | Referencia del plan |
|---|---|---|
| D-1 | S9: precisión experimental del contacto ≤1 mm; clasificación correcta en los casos a ±3 mm. Se conserva el umbral. | T4.1; C10 y C11 |
| D-2 | Predicción: p95 ≤1 ms sobre 200 mediciones en el iPhone, tras 20 de calentamiento; informar también mediana, p99 y máximo. | C17; T5.2/T7.2 |
| D-3 | C7 decide el gate de predicción mediante relanzamiento en vuelo, reiniciando juego y predictor con el mismo estado. C9, continuación sin reinicio del juego, es informativo. | T3.2/T4.2; gate |
| D-4 | Medir las limitaciones de PhysX en §8.3.1–8.3.3 y §8.3.7, sin asumir cumplimiento de requisitos no controlables ni inventar umbrales. Con P1, `physics.maxImpactsPerStep` no es operativo; la evidencia se presenta en el gate. | C12–C15; gate |
| D-5 | `DefaultPreset.json` es la única fuente de valores por defecto. El catálogo describe tipos, rangos, opciones y clase de aplicación, sin duplicar defaults. | T2.2/T5.1 |
| D-6 | La franja diagonal lateral tiene 50 mm de ancho medidos perpendicularmente a la diagonal; se mantiene el borde inferior como referencia. | T2.1 |

Estas aprobaciones no seleccionan anticipadamente P1 ni autorizan P2; la decisión del gate sigue requiriendo al PO con resultados válidos.

## Aclaraciones autorizadas para registro

El PO autorizó registrar C-1 a C-4 de forma trazable, sin modificar unilateralmente otras decisiones de gameplay. Quedan registradas aquí; Codex no edita la spec. La eventual incorporación editorial a una enmienda corresponde al flujo de Claude y PO previsto en el plan.

| ID | Discrepancia o ambigüedad | Aclaración registrada y alcance |
|---|---|---|
| C-1 | §13.4 y anexo A proponen ScriptableObjects para defaults. | Aplicar D-5: defaults en `DefaultPreset.json`, con validación de esquema, tipos y rangos. No mantener una segunda fuente. |
| C-2 | §8.3 exige requisitos para cualquier alternativa y §13.2 prevé controlar impactos por paso, pero P1 no permite imponer ese tope. | Aplicar D-4: con P1 los impactos por paso se miden; el tope no es configurable. Informar la limitación en el panel y en la decisión del gate. No declarar cumplimiento sin evidencia. |
| C-3 | §8.2.2 menciona S1–S8; §17.A.3 amplía a S1–S9 y dispositivo. | Cubrir la unión S1–S9 y S10 en el dispositivo para las comprobaciones de predicción previstas por r2, incluido S8. No reducir cobertura a la intersección. |
| C-4 | §8.7.2 permite interpretar los 50 mm de la franja diagonal como crecimiento vertical. | Aplicar D-6: ancho perpendicular de 50 mm conservando el borde inferior de referencia. |

## Autorizaciones de trabajo

Solo **Etapa 0: preparación y validación del entorno**. El PO autorizó usar su computador para completar los pasos interactivos pendientes de Unity Hub y Xcode y confirmó que el teléfono está conectado. Posteriormente informó que lo desbloqueó, aceptó confianza y ya utilizaba Modo de desarrollador con RallyIQ.

El 2026-10-21 el PO confirmó explícitamente **`cl.rafaallendes.squashclash`** como bundle ID para el primer build e informó que reconectó el cable con el iPhone desbloqueado. Las consultas posteriores verificaron la conexión de desarrollo y Modo de desarrollador habilitado; el detalle consta en el diagnóstico de E0.

Posteriormente, el PO revisó el [diagnóstico de E0](../validation/environment.md), aprobó explícitamente su cierre y autorizó **E1 / I1 (T1.1–T1.3)**: Git local, proyecto Unity 6000.3.27f1, configuración iOS vertical IL2CPP/ARM64, scripts mínimos, builds `dev` y `prod`, firma automática, instalación y pantalla inicial con versión y FPS.

La autorización vigente termina en E1. El PO exige registrar pruebas y evidencia y detenerse para su validación. **T1.4 (remoto GitHub) y E2 no están autorizadas.** No instalar otro Xcode ni implementar cancha, pelota, física o sistemas posteriores. Los ajustes estáticos de proyecto enumerados en T1.2 no implementan una simulación. Continúan vigentes los criterios aprobados, los incrementos pequeños, la revisión de Claude y la validación del PO en el iPhone.

Durante la primera instalación, el PO confió manualmente en su desarrollador en el iPhone y confirmó: «Confié y se pudo abrir». También abrió Duplicación del iPhone para que Codex verificara la pantalla. Se comprobaron allí ambas builds y se registró la evidencia en el [informe de E1](../validation/e1-first-installation.md). Esa confirmación resuelve la confianza inicial; no se interpreta como cierre de E1, aprobación del giro físico ni autorización de E2.

## Revisión de Claude sobre I1 recibida

El 2026-10-21 el PO transmitió y aprobó como revisión de Claude sobre I1 el resultado de la inspección de código y repositorio hasta `8b39812`: **aprobada con dos observaciones menores, ninguna bloqueante**. Se conserva el detalle en el [registro de revisión de Claude](2026-10-21-first-ball-e1-review-claude.md).

- Queda pendiente comprobar, al primer uso de `debug`, si el ajuste persistente de configuración IL2CPP modifica `ProjectSettings.asset` y afecta la marca de workspace sucio de la siguiente build.
- La presencia del Team ID en los informes se considera aceptable en el contexto del futuro repositorio privado; se mantiene explícita para la decisión de T1.4.

Al recibir esa revisión, I1 seguía pendiente de la validación física y visual del PO en ambas builds, incluido girar el teléfono. El mensaje propone crear el remoto e iniciar I2 como siguientes decisiones; no autoriza T1.4, E2 ni T3.1. Se mantiene el alcance de `AGENTS.md` en E1/I1 hasta nueva autorización expresa.

## Validación del PO en el iPhone — prod

El 2026-10-21 el PO confirmó en la build instalada `prod`:

- Título: Squash Clash — First Ball.
- Versión y perfil: `0.1.0+ea0a1e1 prod`.
- Contador: 60 FPS.
- Giro físico realizado: la app se mantiene vertical.

**Validación de prod aceptada.** Tras esta confirmación I1 seguía abierto solo por la validación equivalente de dev. Esta confirmación no autoriza T1.4, E2 ni T3.1.

## Validación del PO en el iPhone — dev

El 2026-10-21, tras instalar y lanzar `dev`, el PO confirmó el rótulo `0.1.0+ea0a1e1 dev`, 60 FPS y el mismo título Squash Clash — First Ball. En el mensaje siguiente, respondiendo específicamente sobre el giro en dev, confirmó: «Si, lo giré y se mantuvo vertical». **Validación física y visual de dev aceptada.**

## Cierre de E1/I1

**E1/I1 (T1.1–T1.3) cerrado el 2026-10-21**, con revisión de Claude aprobada y validación del PO en el iPhone de ambas builds: título, versión/perfil, 60 FPS y permanencia en vertical tras girar físicamente el teléfono. El [informe de E1](../validation/e1-first-installation.md) conserva los resultados y la evidencia técnica.

Las dos observaciones menores de Claude se mantienen para su seguimiento posterior. Este cierre no autoriza T1.4 (remoto GitHub), E2/I2 ni T3.1; no se ha autorizado ningún incremento nuevo. `AGENTS.md` refleja el cierre y la espera de la siguiente autorización.

## Autorización posterior — T1.4 e I2

El 2026-10-21, después del cierre de I1, el PO autorizó explícitamente:

1. **T1.4:** crear el remoto privado `squash-clash` mediante `gh repo create squash-clash --private --source . --push`, verificar `visibility: PRIVATE` y registrar el resultado.
2. **I2 / T2.1–T2.3:** implementar cancha y cámara fija en la rama `firstball/i2-court`, con PR hacia `main` para revisión de Claude y punto de control del PO en el iPhone.

**T3.1 y etapas posteriores siguen sin autorización.** El permiso de I2 no autoriza fusionar sin revisión y validación, ni cambiar decisiones deportivas, criterios o la spec. `AGENTS.md` se actualiza con este alcance.

T1.4 ejecutada el 2026-10-21: remoto [rafaallendes-boop/squash-clash](https://github.com/rafaallendes-boop/squash-clash) creado y publicado, con `visibility: PRIVATE` y rama predeterminada `main`. Resultado y comprobaciones en el [informe de T1.4](../validation/github-remote.md).
