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

## I2 — entrega para revisión y punto de control

El PO confirmó después el iPhone desbloqueado y conectado para verificar el arranque de I2. Con ello se resolvió el rechazo inicial por `Locked`; Codex comprobó la cancha en una captura del LCD real, rótulo `0.1.0+1cbf00c dev`, 60 FPS y Portrait. Implementación T2.1–T2.3 y comprobación técnica registradas en el [informe de I2](../validation/i2-court.md), con [PR #1](https://github.com/rafaallendes-boop/squash-clash/pull/1) hacia main.

**El desbloqueo no es aceptación visual de I2.** Quedan revisión de Claude sobre la PR y valoración del PO de cancha, encuadre y proporción de líneas en el iPhone. No se ha fusionado ni cerrado I2. T3.1 y posteriores continúan sin autorización. La observación menor sobre la orientación horizontal se presenta en la PR para aclaración; no se invierten ejes ni se modifica el plan unilateralmente.


## I2 — revisión de Claude, aceptación visual y aclaración del eje X

El 2026-10-21, el PO transmitió la revisión de Claude sobre la PR #1: **sin bugs de implementación**, con corrección de la convención izquierda/derecha requerida antes de fusionar. Esta revisión fue comunicada en el chat; no se presenta como una aprobación registrada mediante GitHub Reviews.

El PO aceptó la cancha en el iPhone para esta primera versión: «La cancha la veo bien para una primera versión, podría cubrir un poco mas pantalla. Pero vayamos avanzando y esto lo iremos mejorando a medida que vaya avanzando la jugabilidad». **Punto de control visual de I2 aceptado.** Ampliar la ocupación de pantalla queda como mejora futura; no se cambia ahora la cámara.

**Aclaración autorizada del plan, AX-1:**

- Se conservan las coordenadas numéricas, UV, pose/proyección de cámara y posiciones de las cápsulas. Con la cámara por defecto mirando hacia −Z, +X se ve hacia la izquierda de la pantalla.
- `LeftWall`: x = 6,40 m, normal interior (−1, 0, 0). `RightWall`: x = 0, normal interior (1, 0, 0). El origen está por tanto en el suelo entre frontis y lateral derecha, visto desde atrás.
- Se intercambian los identificadores LeftBox/RightBox de las franjas de las cajas de saque y sus tests; sus rectángulos físicos y anchos no cambian. Las diagonales siguen asociadas al lado homónimo y a 50 mm perpendiculares.
- De las dos alternativas autorizadas para S3, se elige **reetiquetar «lateral derecha»**: se mantienen inicio (0,8; 1,0; 8,0), dirección (−sin 8°, 0, −cos 8°) y velocidad 20 m/s. No se refleja el escenario ni se implementa su simulación en I2.
- El PO autoriza actualizar exclusivamente estas referencias del plan r2, registrar la aclaración, añadir una prueba de proyección de `LeftWall` en la mitad izquierda con cámara por defecto y volver a instalar dev. La spec y las demás decisiones deportivas/umbrales permanecen sin cambios.

**T3.1 y etapas posteriores siguen expresamente sin autorización.** La autorización de corregir e instalar no se interpreta como orden de fusionar automáticamente la PR. Se registrarán debajo la verificación y el nuevo build antes de entregar el resultado.


**Ejecución de AX-1:** commit de producto `1b333ad`; EditMode 79/79 y PlayMode 11/11, con RED previo verificado. Build dev limpia `0.1.0+1b333ad`, código 0 y firma/ARM64 comprobados; reinstalación devicectl código 0. El PO confirmó después «Está listo!» para abrir y comprobar dev. **Nuevo arranque verificado en el iPhone:** devicectl sin consola código 0, captura nativa inspeccionada con `0.1.0+1b333ad dev`, 60 FPS y cancha en vertical (1206×2622). [Evidencia de AX-1](../validation/evidence/2026-10-21-i2-axis-dev-run.json). La confirmación de desbloqueo habilita la comprobación técnica; no se presenta como una nueva revisión de Claude o aceptación adicional del PO. La aceptación de la cancha comunicada arriba y la sugerencia de encuadre quedan registradas.


## Cierre de I2 y autorización de I3 — 2026-10-22

El PO confirmó: «Claude aprueba la PR #1 tras AX-1» y autorizó fusionar con **merge commit**, registrar el cierre de I2, sincronizar main y eliminar el worktree de I2. Ejecutado: PR #1 `MERGED`, merge commit `9ef742280fbbb7655c019d930cbcf8fb8fdeff76`; main sincronizada. Suites previas a fusión: EditMode 79/79 y PlayMode 11/11. La aceptación visual y la verificación nativa ya constan arriba. **I2 cerrado.** [Informe y evidencia](../validation/i2-court.md#cierre-de-i2--2026-10-22).

El PO autoriza **I3 / T3.1–T3.3**, rama `firstball/i3-ball`, con PR hacia main para revisión de Claude: reloj y contratos, adaptador P1 con escenas aisladas, host/vista de pelota, barra mínima de desarrollo, pausa del sistema y exclusión del panel de producción. La integración de PhysX es un candidato de la prueba, **no una elección de backend**. Esa decisión sigue reservada al gate de **I4c**; P2 y etapas posteriores no están autorizadas.

El PO conectará el iPhone al finalizar todo para realizar la verificación. Hasta entonces se completan implementación, pruebas, builds y PR; la comprobación física y el cierre de I3 quedan pendientes y no se presentan como aprobados. No se fusiona I3 automáticamente.


## I3 — entrega técnica para revisión, pendiente del iPhone (2026-10-22)

T3.1–T3.3 implementadas en `firstball/i3-ball`. Código de artefactos finales `a538bc5`: EditMode 109/109 y PlayMode 36/36, release-check de exports nuevos correcto y builds nativas dev/prod Release firmadas ARM64 con código 0. Una revisión interna independiente detectó un retroceso visual al continuar; corregido con cinco tests RED→GREEN. No sustituye la revisión de Claude. [Informe y evidencia I3](../validation/i3-ball.md).

**No es aceptación del PO ni cierre de I3.** El PO indicó que conectará el teléfono al finalizar; instalación, cuatro controles, pausa/bloqueo real, prod sin barra y logs devpanel en dispositivo siguen pendientes. La PR hacia main queda para revisión de Claude y no se fusiona automáticamente. PhysX continúa como candidato P1; no se eligió backend ni se avanzó a I4.

Entrega publicada en [PR #2](https://github.com/rafaallendes-boop/squash-clash/pull/2) hacia main, estado OPEN, sin fusionar. Worktree `firstball/i3-ball` conservado para revisión y checkpoint del iPhone; el de I2 ya fue eliminado conforme a la autorización.

## I3 — revisión de Claude y aclaración de yaw (2026-10-22)

El PO comunica que Claude revisó PR #2 **sin bugs bloqueantes**. Es una revisión transmitida en el chat, no una GitHub Review atribuida a Claude. Antes de instalar, autoriza corregir `launcher.yawDeg`: **positivo hacia la derecha de pantalla, componente −X**, coherente con AX-1 y la cámara por defecto; negativo hacia la izquierda. Yaw cero, coordenadas, cámara, preset, pitch, velocidad, escenarios y umbrales no cambian. Esta aclaración sustituye la resolución inicial del ejecutor que asociaba yaw positivo a +X.

Exige una prueba RED→GREEN por proyección, anotación trazable y posterior instalación de dev/prod para el checkpoint del iPhone. El PO informa el teléfono conectado y desbloqueado y pide revisar en dev. Se dejará dev instalada al finalizar las comprobaciones. Si una reinstalación ocurre después del 28 de octubre, se renovará primero el perfil; además se verificará siempre su vigencia efectiva antes de instalar.

**Pendiente aceptado para I5:** «Toca para continuar» pierde una pausa manual previa. Se registra para distinguir y restaurar la intención de pausa manual cuando se trabaje I5; no se corrige ni se presenta como resuelto en I3. Esta aceptación no autoriza I5. **I4 sigue sin autorizar, P1 no es una selección de backend y no se autoriza fusionar ni cerrar I3 antes del checkpoint del PO.** Los resultados de la corrección e instalación se registrarán en el informe de I3.

**Ejecución:** corrección de producto `f075217`; prueba de proyección RED 1/1 → GREEN, PlayMode completo 37/37 y EditMode 109/109. Release-check y builds dev/prod limpias, firmadas ARM64, correctos. Perfil vigente hasta `2026-10-28T14:59:31Z`, comprobado antes de instalar. Ambas instalaciones mediante devicectl terminaron correctamente; dev quedó instalada. iOS rechazó abrir ambas por **Locked** tras volver a bloquearse el teléfono; se solicitó desbloquear. No hay aún arranque funcional verificado de I3 ni aceptación del PO. [Resultado y evidencia](../validation/i3-ball.md#instalación-completada-checkpoint-bloqueado-por-el-bloqueo-del-iphone).

## I3 — observaciones visuales y pruebas desde Duplicación (2026-10-22)

El PO informa Duplicación desbloqueada y solicita que Codex realice desde allí las pruebas. Observa: **«la pelota debería ser negra y se ve muy pequeña»**. Se aplica negro al material visual; el amarillo previo era una elección del ejecutor, sin decisión deportiva que conservar. El radio físico y visual de I3 sigue siendo 20 mm, tal como exige T3.3. Se registra el tamaño insuficiente como observación de legibilidad del PO para las ayudas/escala visual de T6.2; no se adelanta ese sistema ni se interpreta la observación como autorización de I4/I6. I3 aún requiere checkpoint y aceptación expresa del PO.

**Ejecución:** `c49d99b`, solo color de material; PlayMode 37/37, release-check y ambas builds nativas Release firmadas ARM64 correctos. Dev/prod limpias reinstaladas con perfil vigente; dev quedó instalada. Los dos lanzamientos fueron rechazados por iOS con Locked. Al reconectar Duplicación apareció de nuevo la contraseña del Mac; se pidió completarla en esa ventana y bloquear el iPhone físico para mantener la sesión de duplicación, y se dejó la ventana al frente. **Las pruebas solicitadas desde Duplicación siguen pendientes de ese acceso; no hay aceptación del PO, cierre ni fusión de I3.** [Evidencia consolidada](../validation/evidence/2026-10-22-i3-black-delivery.json).

## I3 — resultado del checkpoint de iPhone (2026-10-22)

El PO confirmó **«Squash Clash ya se ve en Duplicación»**. Codex comprobó la app nativa `0.1.0+c49d99b` desde esa ventana: cuatro controles dev, pelota negra, pausa/paso/repetición/continuación, salida y vuelta desde segundo plano con aviso en ambos perfiles, prod sin barra y 60 FPS observados/Portrait. Logs nativos: `devpanel=present` en dev y `devpanel=absent` en prod. Instalación y arranque de ambos verificados mediante devicectl en la pantalla Wireless de Duplicación. Dev reinstalada y abierta al finalizar, con perfil vigente.

Para el bloqueo físico no sustituible por Duplicación, se pidió al PO probar en dev Lanzar → bloqueo con botón lateral → desbloqueo → volver → tocar. El PO respondió **«Aparece el aviso y continúa correctamente»**. **Prueba física de reanudación de dev confirmada por el PO.** No se atribuye una segunda prueba física en prod ni una medición de precisión/salto; prod sí tuvo prueba de segundo plano y su overlay Runtime.

T3.3 tiene su checkpoint técnico registrado en el [informe](../validation/i3-ball.md#checkpoint-técnico-en-el-iphone-completado--2026-10-22) y [manifiesto con capturas/logs](../validation/evidence/2026-10-22-i3-iphone-checkpoint.json). **Esta respuesta confirma esa prueba, no constituye aún aceptación global explícita de I3 ni autorización de fusionar PR #2.** Legibilidad/tamaño pendiente para T6.2; pausa manual para I5 conforme a la decisión anterior. I3 sigue abierto; I4 no autorizado y PhysX sigue como candidato hasta I4c.

## Cierre de I3 y autorización escalonada I4a/I4b — 2026-10-22

El PO declara: «El PO acepta I3 en el iPhone y Claude aprueba la PR #2». Autoriza merge commit, cierre de informe/aprobaciones/AGENTS.md, sincronizar main y eliminar worktree. **Ejecutado: PR #2 MERGED, merge `e09a5432cf2aff32cd568ca2f9408013ffd20d65`, head `f967f10f3af0bf6cd1e76d7d3984bf09539718f4`; main sincronizada, worktree eliminado y Logs archivados localmente. I3 cerrado.** Aprobación de Claude comunicada por el PO, sin atribuir una GitHub Review. [Informe y evidencia](../validation/i3-ball.md#cierre-de-i3--2026-10-22).

Autoriza **I4a / T4.1–T4.2**, rama `firstball/i4a-physics-measures` y PR, y **condicionalmente I4b / T4.3–T4.4**, rama `firstball/i4b-physics-run` con su propia PR **solo después de que I4a esté fusionado tras revisión de Claude**. Detenerse al entregar el informe de I4b. Esta autorización no permite saltar la revisión ni ejecutar I4b antes de la fusión de I4a.

**Sin cambios de escenarios, umbrales ni criterios para hacer pasar resultados. Si no cumplen, se reportan.** Gate I4c y cualquier trabajo P2/T4.5 requieren autorización expresa del PO. I5 y etapas posteriores siguen sin autorizar. Renovar el perfil antes de cualquier instalación posterior al 28 de octubre de 2026, además de verificar su vigencia efectiva.

## I4a — entrega técnica para revisión de Claude (2026-10-22)

T4.1–T4.2 implementadas en `firstball/i4a-physics-measures`, base `c396fe4`, código final `980529d`: escenarios construidos analíticamente y mediciones/informe verificados con datos sintéticos. **EditMode 155/155 y PlayMode 37/37**, cero fallos/omitidos. La revisión interna encontró tres Important, reproducidos con siete fallos RED y corregidos: horizonte S8, cobertura esperada/cerrada y datos C4 inválidos. No sustituye la revisión de Claude.

[Informe I4a](../validation/i4a-physics-measures.md), [evidencia](../validation/evidence/2026-10-22-i4a-tests.json) y [revisión interna](2026-10-22-first-ball-i4a-internal-review.md). No se modifican escenarios, umbrales, spec/plan ni criterios para hacer pasar PhysX. No se ejecuta todavía la campaña de I4b ni hay build/instalación nueva. **I4a pendiente de Claude y fusión; I4b autorizado solo después de esa condición.** I4c/P2/I5+ siguen sin autorizar y se conserva la orden de detenerse al entregar el informe de I4b.

I4a publicado en [PR #3](https://github.com/rafaallendes-boop/squash-clash/pull/3), estado OPEN hacia main. Esto no registra aprobación de Claude ni fusión de I4a; I4b sigue sin iniciar por la condición autorizada.

## I4a — revisión de Claude y autorización de fusión / I4b (2026-10-22)

El PO comunica revisión de Claude de PR #3 **sin bugs**, confirma S8d como velocidad total con arco bajo y autoriza fusionar con merge commit tras tres ajustes: distinguir ausencia de pasos de esquina (NotApplicable, fuera de C3) de dato inválido (NotEvaluable); preparar máximos separados de C3 para pasos con al menos un impacto no persistente y pasos solo persistentes, con rapidez antes/después; usar todo el horizonte de S8d, como S8. No cambian umbral, población global ni veredicto de C3: incluye ambos tipos de pasos de esquina.

Ejecutado RED→GREEN: cuatro tests nuevos, RED 155/159 → GREEN 159/159; PlayMode 37/37. La API estructurada `MaxRelativeEnergyGain` expone estado, ganancia, paso y rapideces; la firma numérica anterior se conserva como envoltura. La separación se refiere a los contactos **del paso evaluado**, aunque la esquina se detecte por superficies en pasos contiguos. S8d mantiene 36 lanzamientos y sus números; ahora Stop=MaxSteps (360 a 120 Hz; salida superior legítima conservada).

Después de la fusión se continúa con I4b/T4.3–T4.4 en `firstball/i4b-physics-run` y PR propia. Detenerse al entregar su informe. I4c, P2/T4.5 e I5+ siguen sin autorizar. Renovar perfil antes de instalar después del 28 de octubre y comprobar siempre vigencia.

**Fusión ejecutada:** PR #3 MERGED, merge commit `ca50142e4c15854b45dc5d7285d0c57bd89860a3`, head `3c66bad7ba3f9a111e824c995796a735816faae7`; main sincronizada. I4a cerrado, condición para I4b cumplida.

## I4b — entrega parcial, campaña del editor completa (2026-10-22)

Alcance autorizado T4.3–T4.4 en `firstball/i4b-physics-run`, código final `ee76086`, [PR #4 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/4). Runner, benchmark y controles implementados; **EditMode 170/170 y PlayMode 43/43**, cero fallos/omitidos. Exclusión nativa prod verificada RED→GREEN contra exports nuevos; una revisión interna fresca detectó dos Important corregidos en un único pase RED→GREEN, sin segunda revisión. No sustituye la revisión de Claude.

Cinco variantes completas del editor, 10.053 lanzamientos de robustez por variante. Ninguna cumple todos los criterios físicos; incumplimientos y no evaluables conservados. Baseline/speculative de cd7fc2a con procedencia explícita y corrección derivada de la mediana C13 par de baseline, originales intactos; otras tres ejecutadas en ee76086. No se atribuye a Claude una clasificación causal aún no recibida.

Build dev final 0.1.0+ee76086 limpia, Release ARM64 firmada y metadata embebida comprobada. **Instalada y abierta** tras conexión del PO; Escenario S1→S2 y cancelación comprobados desde Duplicación, informe INCOMPLETO guardado/copiado y térmico nominal→nominal. Campaña completa y C17 nativo pendientes. La firma no los sustituye. Perfil vigente al compilar hasta 2026-10-28T14:59:31Z; comprobar vigencia al instalar y renovar si corresponde, siempre antes de una instalación posterior al 28 de octubre.

[Informe físico parcial](2026-10-22-first-ball-physics-spike-P1.md), [validación, evidencia y resoluciones](../validation/i4b-physics-run.md). **No es aceptación del PO, revisión de Claude, cierre ni autorización de fusionar I4b.** No se modificó spec/plan, preset, escenarios numéricos, umbrales ni decisiones deportivas. I4c/P2/I5+ siguen sin autorizar; al completar el informe de I4b se detendrá para PO/Claude.

**Seguimiento nativo:** el PO confirmó que la campaña avanzaba; el archivo recuperado registra cancelación por Pérdida de foco a los 20 s, nominal→nominal, INCOMPLETO y C17 no evaluado. Se conserva separado del primer intento cancelado manualmente. Se solicita reiniciar manteniendo Squash Clash al frente, preferiblemente en el teléfono físico y respondiendo desde el Mac. Esta respuesta del PO confirma el inicio, no la terminación ni aprobación de I4b.

El PO confirmó otro inicio en Duplicación. Tercer informe: cancelado por Pérdida de foco a los 1,8 s, nominal→nominal y C17 no evaluado, sin interacción CUA entre reinicio y cancelación. Se pidió pasar la ejecución al teléfono físico sin Duplicación activa y mantener la app al frente; no se modifica el comportamiento para evitar la cancelación. [Manifiesto de intentos nativos](../validation/evidence/2026-10-22-i4b-iphone-runs.json).

**Cuarto intento, teléfono físico:** PO confirma avance y luego «No toqué nada; la app seguía al frente». Informe 20:51:15→21:02:05 del 22 de octubre (Santiago), 649,693 s, 8.732/10.053 lanzamientos, INCOMPLETO por Pérdida de foco, nominal→serious y C17 no evaluado. No hubo acciones CUA durante la campaña. Se preservan archivo/hash y captura de progreso. Causa del evento no establecida; no se culpa a interacción ni a temperatura.

Se continúa el diagnóstico dentro de T4.4 añadiendo contexto de ciclo de vida y temporizador de iOS a los informes, sin cambiar cancelación, escenarios ni criterios. Dos tests nuevos RED43/45→GREEN45/45; enlace/export/instalación nuevos pendientes. No se pide al PO repetir una campaña larga sin datos nuevos. Este registro no acepta ni cierra I4b ni autoriza I4c/P2/I5+.

**Diagnóstico nativo preparado y verificado:** `d14df6f`, PlayMode45/45, Core sin cambios conserva EditMode170/170; exports nuevos dev/prod correctos, Xcode27.1 ReleaseARM64 firmado y metadata limpia verificados. Instalación inicial falló por conexión CoreDevice inválida; reintento correcto. El PO confirmó «Sí, muestra d14df6f dev y está conectado» y ejecutó la interrupción corta solicitada, respondiendo «Informe INCOMPLETO, copiado y guardado».

Archivo recuperado: 8,320 s, nominal→nominal, contexto nativo presente (Unity foco=false; UIKit active; temporizador desactivado), S8-109 y C17=0. Es una comprobación deliberada de diagnóstico, no aceptación de física ni un intento completo. UIKit active también aparece en esa salida real a Inicio y no se usa para declarar falso el evento anterior. Causa de la cancelación larga sin confirmar. I4b sigue parcial, PR #4 borrador, sin fusionar; I4c/P2/I5+ no autorizados.


## I4b — revisión de Claude, diagnóstico CCD y decisiones del PO (2026-10-22)

Revisión de Claude comunicada por el PO, no una GitHub Review atribuida automáticamente. PR #4 sigue **borrador, sin fusionar**. Se pausan campañas largas del iPhone hasta diagnosticar CCD y aplicar las decisiones siguientes; después se repiten las cinco variantes del editor conservando los resultados actuales como históricos. Entregar diagnóstico/correcciones/editor y **detenerse para revisión de Claude antes de volver al iPhone**. I4c, P2/T4.5 e I5+ siguen sin autorizar.

El PO pide trazas de S1 pasos 30–36 (baseline y speculative), reproducción PlayMode mínima con esfera de radio 20 mm a 25 m/s, BoxCollider de espesor 1 m, 120 Hz y ContinuousDynamic, comparando escena local/manual con escena por defecto, RED antes de corregir. Identificar causa con fuente oficial Unity/PhysX; separar commits del diagnóstico y la corrección. No tocar escenarios, umbrales, medidas, preset ni variantes. Si persisten C5–C7, instrumentar la primera diferencia y comparar Reset tras muchos contactos con instancia nueva; **no cambiar Reset sin evidencia RED**.

Decisiones aprobadas, con TDD RED→GREEN:

- **Foco:** `pause(true)`/segundo plano cancela. `focus(false)` en robustez suspende el avance, registra UTC, duración, fase/progreso y contexto de ciclo de vida, y continúa al volver el foco. A los 60 s sin recuperación cancela como INCOMPLETO. En C17, `focus(false)` sigue cancelando. El informe cuenta las interrupciones; un intento con interrupciones solo es válido si termina completo.
- **C7:** checkpoint ausente por salida superior legítima aprobada o fin del horizonte: NotApplicable, fuera de población. Ausencia por cruce, estado no finito u otra anomalía: NotEvaluable. Informar Measured/NotApplicable/NotEvaluable por separado, también en las cinco variantes regeneradas. No se cambia el umbral de igualdad ni el carácter decisorio de C7.
- **Térmico:** registrar al inicio del benchmark, informativo; no añade un umbral ni convierte un intento incompleto en válido.
- **Firma:** renovar antes de la próxima instalación si ocurre el **2026-10-27 o después**; comprobar siempre vigencia antes de instalar. Expiración actual: **2026-10-28T14:59:31Z**. Sustituye el plazo operativo anterior del 28 de octubre. No cambiar Xcode ni otras herramientas.

Clasificación **preliminar**, pendiente del diagnóstico y nuevas campañas: C1 adaptador/configuración probable; C10/C11/C13 y casos grandes C3 posibles consecuencias de C1; C3 residual en reposo posible límite del motor a reevaluar; C5–C7 indeterminados; no evaluables C7 sujetos a la definición aprobada arriba. No se registra clasificación causal final ni elección de backend.

Próximo intento nativo, solo tras correcciones/editor y revisión de Claude: teléfono físico, No molestar, batería ≥80 %, bajo consumo desactivado, sin funda y térmico nominal al inicio. **Sin capturas, devicectl u otras interacciones de herramientas durante la ejecución**; recuperar informe al terminar. Esta autorización no inicia ese intento ahora.


**Ejecución de la revisión CCD / decisiones PO 5 (2026-10-22):** diagnóstico RED 8a1304d y corrección separada 492685b: el modo ContinuousDynamic sí estaba solicitado; el ajuste global Fast Motion Threshold heredado impedía barridos de la esfera frente a cajas gruesas. A→B→A reproducido en escena local y por defecto; asset/ProjectSetup corregidos a 0,01 m. Escenarios, preset, geometría, medidas, umbrales y ejes de variantes conservados. C7 (3fe6fd7) y foco/térmico (9764a88) aplican exclusivamente lo aprobado. **EditMode180/180, PlayMode54/54**, con RED→GREEN documentado. Reset no cambiado; 25 controles seleccionados iguales (16ad653).

Cinco campañas frescas del editor, mismo código **16ad653**, 1/1 completas por variante, 10.053 lanzamientos y 200 medidas cada una; historiales previos conservados. Baseline: C1=0 cruces; C5/C6 pasan; C3 cinco fallos, C7 tres/18.753 medidos y 1.279 no aplicables (NE=0), C10 máximo13,6164427mm y C11 tres/6 fallos. Ninguna variante cumple todo. Primer C7 detallado con estados idénticos y orden distinto de contactos; no se ordena ni reinterpreta para aprobar. Clasificación de Claude permanece preliminar hasta revisar estos nuevos datos. [Informe y cinco variantes](2026-10-22-first-ball-physics-spike-P1.md), [diagnóstico y commits](2026-10-22-first-ball-ccd-diagnostic.md), [validación](../validation/i4b-physics-run.md).

**Sin build/instalación/campaña nueva del iPhone. Se detiene para Claude antes de retomarlo.** La política de firma del 27 de octubre y las condiciones físicas/sin herramientas del siguiente intento quedan vigentes. No es aceptación del PO, aprobación de Claude de los cambios, cierre/fusión de I4b ni autorización de I4c/P2/I5+.


## I4b — revisión de Claude tras CCD y orden canónico aprobado (2026-10-23)

El PO comunica la revisión de Claude y aprueba expresamente el punto 1b: **el adaptador debe entregar los contactos de cada paso en un orden canónico y determinista**. Sustituye la restricción anterior de no ordenar la salida del adaptador; no autoriza ordenar/agrupar en la comparación ni cambiar `TrajectoriesBitwiseEqual`, C7, Reset, escenarios, umbrales, preset o geometría. Se conservan como aprobados CCD `fastMotionThreshold=0,01 m` en asset/ProjectSetup, elegibilidad C7, política de foco y térmico informativo de C17.

Clasificación final de Claude (sustituye a la preliminar; transcripción literal):

- C1: adaptador/configuración. Resuelto con 0 cruces.
- C10: motor. Es el sesgo del integrador semimplícito frente a la referencia analítica (g·dt·T/2 ≈ 13,6 mm). No se compensa; se decide en I4c.
- C11: consecuencia de C10. Los puntos S9c, S9d y S9i quedan unos 13,6 mm por debajo, al borde de una línea.
- C3 residual: motor. Es el solver en contacto persistente en reposo (unos 6 cm/s); con impacto, la ganancia es 0. Sigue en Fail. Un posible piso de velocidad se decide en I4c.
- C13: motor (CCD de una pasada). Informativo, sin cambios.
- C7: adaptador. Expone un orden de contactos no canónico. Los estados coinciden bit a bit, pero el orden de los pares interno de PhysX depende de la historia de cada instancia.

Orden autorizado: (1) clasificar todas las divergencias de C5–C7 antes de cambiar comportamiento, con conteos por criterio/variante; (2) localizar escenario/paso/tipo del máximo de C16; (3) RED físico con dos instancias de historias distintas, después orden en el adaptador por TimeInStep, superficie, persistencia y desempate determinista documentado; conservar divergencias de estado como Fail; (4) suites GREEN y cinco campañas nuevas del editor, conservando las de `16ad653` históricas y actualizando informe/tabla/manifiesto; (5) **detenerse para revisión de Claude antes de generar build nativa**. PR #4 sigue borrador, sin fusionar; I4c, P2/T4.5 e I5+ no autorizados.

Solo después de esa revisión: una build dev y campaña nativa completa, teléfono físico, No molestar, batería ≥80 %, bajo consumo desactivado, sin funda, térmico nominal al empezar y sin capturas/devicectl/otras herramientas durante la ejecución; recuperar informe al terminar. C17 nativo prioritario; p95 del editor 24,1 ms es informativo, no certifica el umbral de 1 ms del iPhone. Comprobar siempre vigencia de firma; renovar antes de instalar el 2026-10-27 o después. No cambiar Xcode ni herramientas.


### Resultado de la revisión de orden canónico — 2026-10-23

Ejecución técnica, **pendiente de la nueva revisión de Claude**: diagnóstico sin cambio de comportamiento 1c47621; RED físico 0ff9297; corrección del adaptador 3de1367; código final y trazas da98fd4. Regresión 188/188 EditMode y 58/58 PlayMode. Cinco campañas nuevas completas, 1/1 cada una. Baseline C7 pasa, junto con C5/C6; C1 cero cruces. C3/C10/C11 siguen Fail. Speculative conserva C6 de persistencia del contacto y distinta terminación, sin divergencia numérica en el prefijo común; C7 pasa. Los resultados restantes por variante constan en el informe. C16 baseline localizado en S8-9948, paso 246, impacto lateral RightWall con callback en 247, sin criterio nuevo.

[Informe P1](2026-10-22-first-ball-physics-spike-P1.md), [manifiesto actual](../validation/evidence/2026-10-23-i4b-editor-canonical-runs.json), [RED→GREEN](../validation/evidence/2026-10-23-i4b-contact-order-tests.json). Históricos 16ad653 conservados; no se cambian criterios, Reset, comparador, escenarios, preset o geometría. **Sin build nativa ni instalación; PR #4 sigue borrador y sin fusionar, I4b abierto.** Se detiene para Claude, sin atribuir esta ejecución a aprobación suya o del PO. I4c, P2/T4.5 e I5+ siguen sin autorizar.


### Revisión Claude de c7da32e y autorización nativa — 2026-10-23

Comunicada expresamente por el PO. Aprobados PhysXContactOrder, el ordenamiento en Step, RED físico y clasificación diagnóstica. Añadir al test físico la dependencia del orden de pares de esta versión de PhysX; una actualización de Unity puede dejar de reproducir el RED. [Clasificación vigente literal y condiciones C17 del editor en el informe P1](2026-10-22-first-ball-physics-spike-P1.md#revisión-de-claude-sobre-c7da32e-y-clasificación-vigente).

Autorizada ahora la campaña nativa: comprobar vigencia del perfil y renovarlo antes de instalar el 27 de octubre o después; sin cambiar herramientas. Build dev del código actual, export limpio, Release ARM64 firmada y metadata verificada; comprobar exclusión del puente nativo en prod. Instalar y verificar antes de la campaña, en app o informe corto, ProjectPhysics fastMotionThreshold=0.01. Pedir al PO campaña física completa: No molestar, batería ≥80 %, bajo consumo desactivado, sin funda, térmico nominal al inicio, sin capturas/devicectl/herramientas durante la ejecución. Recuperar Documents/spike al terminar; verificar 18 filas, hash, variante, 200 mediciones C17, térmico inicial del benchmark e interrupciones de foco. C17 p95 ≤1 ms; incumplimiento es Fail. Registrar condiciones del editor (34,9 ms; histórico 24,1 ms) sin interpretar ni comparar con nativo.

Completar informe I4b y detenerse para Claude/PO. PR #4 sigue borrador sin fusionar, I4b no cerrado. No elegir backend ni cambiar umbrales, escenarios, preset o Reset. I4c, P2/T4.5 e I5+ sin autorizar.


### Primera corrida nativa posterior al orden canónico — 2026-10-23

El PO informó que terminó («Prueba completa, informe copiado, informe guardado») y autorizó mirar pantalla/continuar. Declaró **75 % de batería y funda puesta**. Esto registra las condiciones reales; **no constituye una autorización explícita para relajar las condiciones anteriores**. No molestar/bajo consumo no confirmados. Sin actividad de herramientas del agente durante la ejecución; recuperación de Documents/spike posterior al aviso de término.

Corrida completa P1-20261023-133222-324, build 2fdede5, baseline, configuración c86243b15a97: 18 criterios, 20 calentamientos y 200 medidas C17; térmico nominal al inicio/final/benchmark y cero interrupciones de foco. C17 **Fail, p95 14,923400 ms**, umbral ≤1 ms intacto. Informe íntegro y auditoría preservados; sin interpretar el rendimiento ni compararlo con editor. Campaña bajo condiciones aprobadas todavía pendiente, misma build instalada para repetir. I4b abierto; PR #4 borrador sin fusionar. [Informe P1](2026-10-22-first-ball-physics-spike-P1.md).


### Confirmación de campaña nativa bajo condiciones aprobadas — 2026-10-23

Respuesta literal del PO a la solicitud de repetición: **«Prueba completa - Batería inicial 90%. Se cumplen las condiciones»**. Confirma ejecución completa con teléfono físico, No molestar, batería ≥80 %, bajo consumo desactivado, sin funda, térmico nominal al inicio y sin capturas/duplicación/devicectl/otras herramientas durante la ejecución. No hubo herramientas del agente hasta el aviso de término; después se recuperó Documents/spike. Es confirmación de condiciones, no aprobación del resultado ni decisión del gate.

Informe P1-20261023-142516-831, build 2fdede5, configuración c86243b15a97, baseline: 18 criterios únicos, 20 calentamientos/200 medidas C17. **C17 Fail, p95 14,322100 ms**, umbral ≤1 ms intacto. Térmico nominal inicial, **fair al iniciar C17 y al final**, cero interrupciones de foco. El térmico de C17 es informativo según la política ya aprobada, sin nuevo umbral ni atribución causal. C3/C10/C11 Fail, C13 NotEvaluated; sin elegir backend ni cambiar preset, Reset, escenarios, variantes o criterios. Corrida anterior 75 %/funda preservada como histórico fuera de condiciones.

[Informe I4b completado](2026-10-22-first-ball-physics-spike-P1.md), [auditoría de campaña](../validation/evidence/2026-10-23-i4b-native-compliant-run.json). Se detiene para revisión de Claude y decisión del PO; **I4b sigue abierto, PR #4 borrador sin fusionar**. I4c, P2/T4.5 e I5+ no autorizados. No se solicita nueva campaña ni se optimiza física por cuenta propia.


## Cierre de I4b y decisión PO B para I4c/P2 — 2026-10-23

El PO comunica la revisión final de Claude: **entrega T4.3–T4.4 aprobada**, y autoriza marcar PR #4 lista, fusionar con merge commit, sincronizar main, registrar el cierre y eliminar el worktree. No se atribuye una GitHub Review distinta de esta aprobación comunicada por el PO.

**Ejecutado:** PR #4 marcada lista y **MERGED**, merge commit `0e08daa8133206361a6017c2527bf84c0fcbf5f6`, head `f56328484451e68e7154b536e9e640dada772652`; main sincronizada, worktree I4b eliminado. Logs, builds y registro local de tareas preservados fuera del worktree en `Logs/archive-i4b-20261023` (ignorado, sin versionar datos locales/personales). **I4b / T4.3–T4.4 cerrado.**

Resultado nativo final P1-20261023-142516-831, dev 2fdede5, baseline, config c86243b15a97, batería inicial 90 % y condiciones aprobadas confirmadas: **C17 Fail, p95 14,3221 ms** (200 medidas; umbral ≤1 ms); **C3, C10 y C11 Fail; C1, C2 y C4–C8 Pass**. Térmico nominal inicial, fair al iniciar C17/final, cero interrupciones. C13 NotEvaluated. El cierre acepta la entrega y evidencia, no el cumplimiento de P1. [Informe P1](2026-10-22-first-ball-physics-spike-P1.md), [auditoría final](../validation/evidence/2026-10-23-i4b-native-compliant-run.json).

Observaciones menores de Claude diferidas por el PO a **la próxima campaña nativa que exista**: imprimir fastMotionThreshold en el encabezado (relevante solo para P1) y guardar las 200 muestras individuales C17. **No repetir I4b** por estas observaciones.

**I4c, decisión B del PO:** P1 no cumple; C17 está unas 14 veces por encima del umbral y C10/C11 y C3 fallan por limitaciones del motor. Autoriza evaluar **P2/T4.5, simulación analítica propia**, manteniendo escenarios, criterios y umbrales; P1 sigue de respaldo. No se elige backend final, no se cambian umbrales ni spec, no se congela ninguna variante. La elección definitiva se hará con los datos de P2 en un gate posterior.

**Orden de ejecución obligatorio:** I4c solo documentación en `firstball/i4c-physics-decision`, con informe de decisión y adenda literal de cuatro puntos a T4.5 del plan r2 aprobada el 2026-10-23. PR para Claude; **detenerse cuando esté lista y no fusionarla sin su revisión**. Solo después de fusionar I4c, P2 en `firstball/i4d-p2-analytic`, PR de física propia, TDD RED válido por comportamiento, suites GREEN, mismo experimento en EditMode/editor y una campaña nativa bajo condiciones aprobadas. Esta próxima campaña incorpora las dos observaciones menores. Comprobar perfil y renovar antes de instalar el 2026-10-27 o después; herramientas sin cambios. Entregar comparación de 18 criterios P1/P2 y detenerse para Claude/PO. I5 no autorizado.


## Cierre de I4c y comienzo de T4.5 — 2026-10-23

El PO comunica aprobación de Claude de PR #5 con precisión propuesta por Claude y aprobada por el PO el 2026-10-23. Añadida literalmente al final del punto 1 de la adenda r2: **«En paredes, un impacto con velocidad normal menor que bounceThreshold anula la componente normal sin rebote. No existe estado de reposo en paredes.»** Registrada también en el documento de decisión para coincidir con la semántica P1.

PR #5 fusionada con merge commit **ac5e195e6ac2c8f31a2491492a5980a3c7c88acd**, head c361a46, main sincronizada. **I4c cerrado.** Aprobación comunicada por el PO; sin atribuir otra GitHub Review. El PO autoriza iniciar T4.5 en firstball/i4d-p2-analytic conforme a plan/adenda y secuencia registrada: TDD, experimento EditMode/editor, una campaña nativa con condiciones aprobadas e instrumentación pendiente, comparación P1/P2 y detención para Claude/PO. P1 respaldo, backend definitivo pendiente; umbrales/criterios/escenarios/spec intactos, sin congelación ni I5.


## Entrega de T4.5 / I4d y confirmación de campaña P2 — 2026-10-23

Ejecución dentro de la autorización anterior, **pendiente de revisión de Claude y decisión del PO**: P2-Analytic implementado con adenda r2 y precisión de paredes, once grupos RED válidos antes del comportamiento y suites GREEN **221/221 EditMode, 61/61 PlayMode**. Mismo experimento completo en EditMode y PlayMode, 1/1 cada uno; 18 criterios y 200 muestras por informe. Primer intento EditMode cero tests rechazado, no contado. Escenarios, criterios, medidas, preset, umbrales, spec y configuración P1 sin cambios.

Una build dev **0.1.0+557d3ff**, export limpio Release ARM64 firmado, metadata/perfil verificados antes de instalar; prod nuevo sin DevPanel/puente. No se cambiaron herramientas ni se repitió I4b. El intento de lanzamiento por herramientas falló por Locked; apertura manual por PO, comprobada por informe completo de la build correcta. PO ejecutó la única campaña nativa P2 y respondió **«Listo. Prueba completada»**. Consultada la batería y las condiciones, confirmó **«SI, al 80% y todo según lo solicitado»**: teléfono físico, No molestar, batería inicial **80 %**, bajo consumo desactivado, sin funda y sin capturas/duplicación/devicectl/otras herramientas durante ejecución. Sin herramientas del agente hasta aviso de término; recuperación posterior tras resolver conexión.

Informe **P2-20261023-204715-409**, config **c86243b15a97**, variante **analytic/r2-20261023**, 10.053 lanzamientos, 18 criterios únicos completos. **11 criterios de gate Pass (C1–C8, C10, C11, C17)**; siete informativos. **C17 Pass, p95 0,011400 ms ≤1 ms**, 20 calentamientos/200 muestras persistidas y percentiles recalculados; mediana 0,008600 ms, p99 0,127200 ms y máximo 0,158700 ms. C3 0/1.701, C10 máximo 0,0009536743 mm y C11 0/6. **Térmico nominal inicial/benchmark/final, cero interrupciones de foco**. Encabezado fastMotionThreshold=0.01 (solo P1) y bounceThreshold efectivo P2=0.1 verificados. Observaciones menores diferidas de I4b resueltas; no se solicita otra campaña.

[Comparación completa de los 18 criterios P1/P2](2026-10-23-first-ball-physics-spike-P2.md), [auditoría nativa](../validation/evidence/2026-10-23-i4d-p2-native-run.json), [validación](../validation/i4d-p2-analytic.md). Confirmar ejecución/condiciones no equivale a revisión o elección del backend. **Se detiene para Claude/PO, I4d abierto y PR #6 borrador sin fusionar.** P1 respaldo, backend definitivo pendiente; sin congelación/promoción de variantes/preset, cambio de spec o criterios/umbrales ni inicio de I5.


## Cierre de I4d y gate de backend / autorización I4e — 2026-10-23

El PO comunica **aprobación de Claude de PR #6 (T4.5/I4d)**: P2 cumple la adenda r2, incluida precisión de paredes; evidencia verificada, C10 ≈0,00095 mm, C16 ≈0,00002 mm informativo, C17 p95 0,0114 ms con 200 muestras persistidas. Observaciones (a) Reset presupone estado dentro de cancha para futuros inyectores/golpe, (b) fricción bajo bounceThreshold literal μ·(1+e)·|v_n| para futura calibración y (c) acoplamiento C14 aceptable, registradas en [informe P2](2026-10-23-first-ball-physics-spike-P2.md#revisión-final-de-claude-y-decisión-del-po--2026-10-23), sin cambio de código. Claude verificó monotonicidad del redondeo al float más cercano frente a planos float exactos, sin riesgo de cruce por redondeo al final del paso.

**Ejecutado por autorización del PO:** PR #6 marcada lista y fusionada con merge commit **9d508595c51bbf07b381640f4ba4c65df2c07786**, head **d7687d5c4f0c1ab5f333db23925e21303c4be109**; main sincronizada. **I4d / T4.5 cerrado.** Aprobación de Claude comunicada por PO, sin atribuir otra GitHub Review. Regresión 221/221 EditMode y 61/61 PlayMode; editor completo en ambos modos y una campaña nativa completa bajo condiciones confirmadas, batería inicial 80 %, térmico nominal inicial/benchmark/final y foco 0. Nativo: 11 criterios de gate Pass (C1–C8/C10/C11/C17), C17 p95 **0,0114 ms ≤1 ms**; 200 muestras auditadas. C17 del editor conserva Info; no se compara con nativo.

**Decisión PO de §8.2.3:** P2-Analytic es el backend de First Ball, la alternativa más simple que cumple invariantes y umbrales. P1 falla C3/C10/C11/C17, se conserva en código como respaldo, sin usarse por defecto. Se autoriza I4e en `firstball/i4e-physics-backend`: documento de decisión con comparación de los 18 criterios, default physics.backend=P2 en DefaultPreset.json (única fuente de defaults), TDD de host P2 por defecto y P1 configurable, informar hash nuevo y PR para revisión de Claude. Antes de pedir revisión, verificación corta en iPhone de arranque sin errores y pelota rebotando con P2 en vista normal. **No repetir campaña**.

Spec no modificada: preparar entrada de decisión para T7.3, escribirla solo con autorización PO. Aceptación completa con P2 diferida a T7.1. **I5 no autorizado; detenerse cuando PR I4e esté lista para Claude**, sin fusionarla por cuenta propia. Sustituye las restricciones históricas de backend pendiente; no autoriza otros cambios de preset/variantes/escenarios/umbrales/criterios ni calibración.


## Entrega de I4e y comprobación corta en iPhone — 2026-10-23

Ejecución de la decisión PO de backend ya autorizada, **pendiente de revisión de Claude de I4e**. DefaultPreset.json cambia únicamente physics.backend **0 (P1)→1 (P2)**, sigue siendo la única fuente de defaults; hash efectivo **f6a394a12daf** (antes c86243b15a97). Catálogo/EffectiveConfig admiten ambas opciones y bootstrap registra ambas fábricas; P1 sigue disponible por configuración. Dos ciclos RED válidos→GREEN, suites completas **222/222 EditMode y 63/63 PlayMode**, sin fallos/omitidos; host normal P2, dos botes, predictor bit a bit y cambio P2→P1→P2 verificados. Motores, escenarios, medidas, criterios, umbrales, ProjectSettings, plan y spec sin cambios.

Verificación corta previa a revisión: dev **0.1.0+8fb5d0c**, export limpio Release ARM64 firmado, Player no Development, metadata/preset embebidos verificados y perfil vigente comprobado antes de instalar; exports dev/prod nuevos, prod sin DevPanel/puente. Instalación y lanzamiento aceptados. Log nativo: **backend=P2-Analytic, config=f6a394a12daf, localPhysicsScenes=0**, cero excepciones durante arranque observado. Se pidió al PO abrir la build instalada y pulsar «Lanzar» en vista normal para confirmar arranque/rebote, sin pulsar Prueba física P2. Respuesta literal: **«si, funciona lanzar»**. Se registra como confirmación de esa comprobación corta, sin repetir campaña ni atribuir aceptación integral, aprobación de Claude de I4e o permiso de merge.

[Decisión y comparación de 18 criterios](2026-10-23-first-ball-physics-backend-decision.md), [validación](../validation/i4e-physics-backend.md), [evidencia nativa](../validation/evidence/2026-10-23-i4e-native-smoke.json). **PR #7 lista para revisión de Claude, sin fusionar; I4e abierto. Se detiene aquí.** Entrada de spec preparada para T7.3, su escritura requiere autorización PO; aceptación completa P2 en T7.1. **I5 no autorizado ni iniciado.**


## Cierre de I4e / Etapa 4 y autorización I5a — 2026-10-23

El PO comunica **aprobación de Claude de PR #7 / I4e** y autoriza fusionar con merge commit, sincronizar main, registrar cierres y eliminar worktree. **Ejecutado:** PR #7 lista y fusionada con merge commit **50a3f4a73fde091e0a8358704d2aca4a6ca3a357**, head **d6f864016d8dd94259da7b9c57af1e08021599a1**, main sincronizada y worktree I4e eliminado, artefactos locales preservados en ruta ignorada Logs/archive-i4e-20261023. **I4e y Etapa 4 cerrados.** P2-Analytic es backend de First Ball por defecto; P1 configurable como respaldo, hash **f6a394a12daf**, suites **222/222 EditMode y 63/63 PlayMode**, arranque/rebote en iPhone confirmado por PO. Aprobación comunicada por PO, sin atribuir otra GitHub Review. Spec intacta; entrada preparada para T7.3, aceptación completa P2 para T7.1.

Observación de Claude para la próxima intervención en host (I5 o T7.1): **parametrizar BallSimulationHostTests y Launch_UsesEffectiveConfigInEveryConsumer para ambos backends**, porque los comportamientos generales del host hoy se prueban con P1. Conservar precondición de Reset P2: el estado inyectado debe estar dentro de cancha; garantizarlo en el inyector o validar cuando corresponda. No cambiar el motor para esta observación. **Desde el 2026-10-27, toda instalación requiere renovar antes el perfil**, además de comprobar vigencia efectiva; sin cambiar herramientas.

**I5a / T5.1–T5.2 autorizado** en `firstball/i5a-tuning-core`, PR para revisión de Claude. Incluye estado de edición/aplicación de parámetros, persistencia de presets, diagnóstico/métricas comparables y la pérdida de pausa manual tras «Toca para continuar» registrada para I5. Si se toca el host, atender la precondición Reset de P2 y la parametrización indicada. TDD RED válido antes de cada comportamiento y suites GREEN; entregar informe I5a y detenerse, sin fusionar por cuenta propia. **No autoriza I5b/T5.3, panel completo, calibración, gameplay posterior ni cambios de spec/criterios/umbrales/default preset.**


## Entrega de I5a / T5.1–T5.2 — 2026-10-23

Implementación dentro de la autorización anterior, **pendiente de revisión de Claude y cierre del PO**. Estado de edición inmediato/pendiente, presets atómicos y recuperación activa sobre defaults con aviso, histograma/ventana de frames, JSONL trazable, sensores térmico/CPU/GPU y segmentos de 10 s con fase/semilla, GC y descartes. DefaultPreset.json sigue único e intacto, P2 default/P1 configurable y hash **f6a394a12daf**. Sin modificar motores, escenarios, medidas, criterios, umbrales, ProjectSettings, plan o spec.

La corrección de pausa manual expresamente incluida está implementada: «Toca para continuar» conserva la pausa previa y callbacks repetidos no la sobrescriben. Al tocar el host se atiende la precondición de Reset: rechaza estados inyectados fuera de límites de centro/no finitos antes de mutar el vuelo. Límite superior derivado almacenado como float, sin ε añadido, con control del float inmediatamente exterior; OverTop conserva su semántica. Observación de Claude de PR #7 atendida: **30 casos del host (15 por backend)** y dos entradas parametrizadas de Launch_UsesEffectiveConfigInEveryConsumer cubren P1/P2.

**Nueve ciclos RED válidos→GREEN; regresión final 250/250 EditMode y 92/92 PlayMode**, cero fallos/omitidos, XML frescos y sin CS. Cero tests de un filtro incorrecto rechazado; primera regresión 90/92 detectó la frontera float y no se declaró aprobada. Exports limpios dev/prod y build dev **0.1.0+7ce86f6** Release ARM64 firmada, Player no Development, metadata/preset exactos y árbol limpio verificados. Destino físico no disponible: compilación con generic/platform=iOS, sin cambiar herramientas. Puente térmico runtime de T5.2 incluido en ambos perfiles; puente Spike/DevPanel excluido de prod. **Sin instalación, campaña ni mediciones de rendimiento/estado del dispositivo en I5a.** Desde el 27 de octubre toda instalación requiere renovación previa del perfil y siempre comprobar vigencia.

[Informe I5a](2026-10-23-first-ball-i5a-tuning-core.md), [validación](../validation/i5a-tuning-core.md), [TDD y fuentes](../validation/evidence/2026-10-23-i5a-tuning-core-tests.json), [build saneada](../validation/evidence/2026-10-23-i5a-native-build.json). **Se entrega con [PR #8 lista para revisión de Claude](https://github.com/rafaallendes-boop/squash-clash/pull/8) y se detiene. I5a abierto y sin fusionar; I5b/T5.3 y posteriores no autorizados.** No se atribuye una aprobación del PO/Claude a esta ejecución técnica. Spec/aceptación integral siguen para T7.3/T7.1 con sus autorizaciones.


## Cierre de I5a y autorización I5b — 2026-10-23

El PO comunica aprobación de Claude de **PR #8 / I5a (T5.1–T5.2), head 98b8139d779e63c25ae7b3b1bfe63e5a1b8e4977**, y autoriza el cierre. PR ya lista para revisión, fusionada con **merge commit a13ae527d88736af0380e5dc95480043eaa3ad68**, main sincronizada y worktree I5a eliminado; artefactos locales preservados en Logs/archive-i5a-20261023 (ignorado). **I5a cerrado**. Regresión 250/250 EditMode y 92/92 PlayMode; build dev 7ce86f6 Release ARM64 firmada, sin instalación ni ejecución en iPhone. Aprobación comunicada por PO, sin atribuir una GitHub Review adicional.

Observaciones de Claude registradas sin modificar código en este cierre:
1. TuningState.Set lanza con valores inválidos: el panel T5.3 debe ofrecer solo valores válidos o capturar el error.
2. Los logs de métricas se escriben también en prod y no tienen retención: definir una política antes de T8.
3. Los percentiles de segmento usan el borde superior de la cubeta (sesgo ≤0,1 ms, documentado).
4. Reservar el nombre **"Default"** para presets en T5.3.
5. Pausa manual y sensores aún sin verificar en iPhone: incluir esa verificación en I5b. Renovar el perfil antes de instalar si es el 2026-10-27 o después, y comprobar siempre vigencia.

**I5b / T5.3 autorizado** en `firstball/i5b-dev-panel`, panel autogenerado según plan, PR para revisión de Claude. Entregar informe y detenerse; no fusionar por cuenta propia. No adelantar T5.4, calibración ni gameplay. Mantener backend P2 por defecto/P1 configurable, spec, criterios, escenarios y umbrales. Solo añadir los defaults previstos expresamente para T5.3.


## I5b implementado; comprobación física pendiente — 2026-10-23

Dentro de la autorización anterior, rama firstball/i5b-dev-panel y [PR #9 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/9). Cinco grupos RED válidos→GREEN; suites **253/253 EditMode, 102/102 PlayMode**, XML frescos, cero fallos/omitidos. Panel autogenerado, LaunchCurrent, presets/portapapeles, diagnóstico y repetición; Default reservado y errores de TuningState capturados. Defaults solo añade las dos claves de T5.3 (repeatSeconds=0 y slowMotion=1), hash **164b0c21e88b**; valores físicos existentes idénticos, P2/P1 conservados. Sin cambios en motores, spec, escenarios, criterios, umbrales, plan o ProjectSettings.

Dev **0.1.0+11e505f** limpia, Release ARM64 firmada, metadata/preset exactos y perfil embebido válido comprobados antes de la instalación exitosa del 2026-10-23. Exports limpios dev/prod RELEASE-CHECK: OK, prod sin DevPanel/puente Spike y con puente térmico runtime de I5a. Lanzamiento automático rechazado por **Locked**, no un fallo comprobado de la app. Se solicitó al PO apertura manual y recorrido: tres dedos/DEV, restitución pendiente y aplicar, guardar po-1/cierre completo/recuperación, exportar/importar, pausa manual al volver y sensores. **Respuesta pendiente: no se registra esa verificación como cumplida ni la entrega como completa**. Sin herramientas sobre el iPhone durante la comprobación solicitada; recuperar después los datos para completar informe y dejar PR lista para Claude, sin fusionar.

[Informe con estado y evidencia](2026-10-23-first-ball-i5b-dev-panel.md). Sin campaña física, calibración, T5.4 ni gameplay. Política de logs/retención antes de T8 sigue pendiente. Perfil: renovar antes de instalar desde el 27 de octubre, comprobar siempre vigencia; herramientas intactas.


## Respuesta PO y recuperación parcial de I5b — 2026-10-23

El PO responde al recorrido: **«Lo hice y ha funcionado bien. Pero igual lo dejé en duplicar iphone abierto para que tu lo puedas ver. tienes permiso de usar el iphone»**. Se conserva como confirmación general del recorrido y autorización para inspeccionar. No es revisión de Claude ni permiso de merge.

Recuperados dos logs de la build 0.1.0+11e505f dev (15 registros, cuatro lanzamientos), CPU/GPU con 5.000 muestras cada uno y térmico nominal; datos de disponibilidad, sin evaluación de rendimiento. La conexión visual del agente falla native pipe startup dos veces, incluido reset; no hubo inspección visual directa. **Persistencia todavía pendiente:** todos los registros Default/config 164b0c21e88b y Documents listado sin carpeta presets; copia de esa ruta falla 7000. Se pidió al PO solo cambiar/aplicar restitución 0,60 y guardar po-1, dando el mensaje exacto. La evidencia impide registrar todavía el recorrido completo como cumplido. **PR #9 sigue borrador** hasta aclarar y corroborar guardado/reinicio. [Auditoría](../validation/evidence/2026-10-23-i5b-native-functional.json). No campaña ni cambios de código en esta recuperación.


## Aclaración del PO y corrección responsive de I5b — 2026-10-23

Respuesta a la comprobación de guardado: **«Presets guardar como no lo encontre, el resto funcionó, lo que si habría que mejorar la responsividad, ya que se sobreescribe todo»**. El guardado no se había completado; no se atribuye fallo de E/S a PresetStore. El PO autenticó Duplicación tras solicitud y respondió «listo». Permiso previo para operar el iPhone sigue vigente.

El agente observó y reprodujo contenido sobre pestañas, con toques a Presets capturados por un Foldout. Corrección con RED geométrico→GREEN, pruebas adjuntas de 390/320 puntos, campo Nombre del preset y botón explícito Guardar como…; mensajes visibles. En la build 5b9d564 se observó la corrección nativa y se detectó entrada numérica confirmada por carácter; RED real de teclado→GREEN, edición diferida y texto enfocado conservado. Fuente final **1318753**, siete grupos de comportamiento, **253/253 EditMode y 105/105 PlayMode**, sin fallos/omitidos y XML frescos. Defaults/hash 164b0c21e88b, motores/spec/settings/criterios/umbrales intactos.

Dev **0.1.0+1318753** limpia Release ARM64 firmada, metadata/preset exactos, perfil vigente comprobado antes de instalar el 2026-10-23. Exports dev/prod nuevos OK, herramientas sin cambios. Instalación exitosa; apertura automática rechazada por Locked. Duplicación se pausó durante la compilación y al reanudar pidió autenticación de macOS de nuevo. **Se pidió al PO autenticar esa ventana; falta terminar la prueba de guardado/reinicio/portapapeles**, que realizará el agente con su permiso vigente. No se pide repetir el recorrido completo. **PR #9 sigue borrador** hasta verificarlo. Sin campaña, rendimiento, T5.4, calibración ni gameplay.

## Seguimiento de verificación I5b — 2026-10-24

El PO avisó que autenticó Duplicación y mantiene vigente el permiso para operar la app. En 0.1.0+1318753 dev el agente comprobó pestañas accesibles, apertura de Presets y floor restitution=0,60 mediante slider, aplicado con cero pendientes. Se escribió el nombre del preset, pero se perdió la conexión antes de guardar. CoreDevice confirma conexión; al reintentar Duplicación macOS vuelve a pedir Touch ID/contraseña. Se solicitó al PO completar esa autenticación sin compartir credenciales. Guardado/reinicio/export-import pendientes, PR #9 todavía borrador. No hay aprobación de Claude ni cierre de I5b inferidos. La escritura decimal está cubierta por la regresión de teclado UI Toolkit, sin certificarla mediante inyección de teclado nativo. Suites finales 253/253 EditMode y 105/105 PlayMode; no se repite campaña física ni se avanza a T5.4.
