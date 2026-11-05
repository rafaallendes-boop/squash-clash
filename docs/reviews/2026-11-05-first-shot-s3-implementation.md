# First Shot S3 — implementación (incrementos A, B y C)

**5 de noviembre de 2026, Chile. Para revisión independiente y decisión del PO.** Rama `firstshot/s3-feedback` desde `main` en `f3b6486` (merge de PR #33). Implementación según las nueve [decisiones del PO](2026-11-05-first-shot-s3-design-plan.md#decisiones-del-po) y la autorización de implementación de S3. Sin build para dispositivo, instalación, acceso al iPhone ni protocolo físico. [Registro de aprobaciones](2026-10-25-first-shot-po-approvals.md#implementación-de-s3-por-claude-con-subagente--5-de-noviembre-de-2026-chile).

## Método y límite de independencia

El PO decidió el 5 de noviembre que los incrementos los avance Claude, con un subagente de modelo Sonnet para implementar y Claude (Opus) para revisar. Literal en el registro de aprobaciones. Codex no implementó S3.

Claude redactó los encargos, revisó cada incremento contra el diff y la evidencia, y resolvió las dudas como decisiones registradas. **Esa revisión no es independiente**, porque el mismo Claude dirigió la implementación. Se recomienda una revisión independiente de la PR antes de fusionar. Las decisiones tomadas en nombre del PO se listan al final.

## Resultado por incremento

### A. Entrega única del resultado final

- `Core/Shot/ShotPresentation.cs`: `ShotPresentation` inmutable y `ShotPresentationGate`. Identidad (lanzamiento, intento). Elegibles: Perfect o Valid aplicados (con paso, posición y lado del contacto) y Failed resuelto sin aplicación. Ninguna entrega para NoOrder, Unreachable, SystemFault, NotEvaluable, descartes, cierres, Perfect/Valid no aplicados u otro lanzamiento.
- La identidad se reserva antes de avisar a los consumidores, lo que cubre la reentrada. Una versión nueva no abre otro cupo. Un resultado distinto sobre una identidad ya entregada se conserva como discrepancia para diagnóstico.
- `ShotController` reinicia la puerta en cada lanzamiento, entrega desde `Finish` y publica `PresentationDelivered`. `ShotResolved` y las notas conservan su comportamiento.
- Métricas nuevas: una línea `shot-presentation` por entrega y `shot-presentation-discrepancy` si aparece una discrepancia, para correlacionar en el iPhone.
- RED: 16/16 en EditMode y 8/15 en PlayMode fallaron por aserción. Los 7 restantes son casos de cero entregas que el esqueleto ya cumplía. GREEN completo: 538/538 EditMode y 312/312 PlayMode.
- Corrección tras la revisión: la prueba de resbalones no comprobaba nada en un caso. Ahora cubre N04 y N04b, cada uno aplicado y fallido, con Swing afirmado sin condiciones (314/314 PlayMode).

### B. Audio diferenciado, háptica iOS e interruptor

- Perfect: `impact-perfect.wav` y háptica firme (`UIImpactFeedbackStyleHeavy`). Valid: `impact-valid.wav`, de timbre y ataque distintos, y háptica discreta (`Light`). Failed: el swish de S2, sin háptica. Los tres clips se precargan. Clips nuevos generados para el proyecto, con fórmula en [PROVENANCE](../../Assets/SquashClash/Runtime/Resources/Audio/PROVENANCE.md).
- Puente `Assets/Plugins/iOS/SquashHaptics.mm`, solo iOS. Reutiliza dos generadores en el hilo principal. `prepare()` se llama al aceptar el apoyo M2-4 e `impactOccurred` solo desde la entrega final. Fuera de iOS no hace nada, y las pruebas usan un doble que cuenta peticiones.
- Modo silencio: sin cambios en la sesión de audio, la ruta, el buffer, `AudioManager` ni la grabación. El comportamiento queda para el iPhone.
- `feedback.haptics`: clave Bool nueva, por defecto 1, leída del snapshot de cada lanzamiento. Con el interruptor apagado se omite la háptica y se mantienen audio y nota.
- **Hashes (R2-3):** defaults `915ae1e479b9`/106 → **`c0d8ec7050e0`/107**; P0 efectivo esperado `dd122831f592` → **`6ecfdfa78cbb`**. Siguen intactos: SimulationKey `166a96e319c7`, proyección First Ball `0d39e6016bd8`, proyección P0 `8de3f2ecb217`, los bytes de P0-calib-1 y sus 41 valores heredados. La prueba con `pre-s3-defaults.json` (copia exacta de defaults en `f3b6486`) confirma que el único cambio es la clave nueva. Se actualizaron solo cinco valores registrados de hash o número de claves, igual que en PR #32. **El hash `6ecfdfa78cbb` debe declararse antes de cualquier precheck en el iPhone.**
- RED por aserción: 5/8 EditMode y 9/12 PlayMode. Excepción: la prueba del interruptor falló por clave desconocida antes de existir la clave; la prueba de catálogo sí falló por aserción con la misma causa. GREEN completo: 546/546 y 326/326.

### C. Nota, destello y swing básico

- Nota sin cambios de código: «Perfecto», «Adelantado» o «Atrasado» y «¡Al aire!» o «¡Tarde!», con `timing.showNotes`. «Atrasado» se comprueba llamando directamente a la nota, porque con el lanzamiento por defecto una suelta tardía resulta en Failed.
- Destello `PerfectFlashView`: solo Perfect con contacto aplicado, en la posición del contacto, 0,12 s, sin collider, se borra al relanzar. No depende de `showNotes`.
- Swing `SwingView`, opción (a): brazo y raqueta generados por código como hijos de la cápsula del jugador, sin rig ni assets. Arranca la preparación una vez por orden Swing aceptada (evento `SwingAccepted`). La pose de impacto sale solo con la entrega aplicada y el seguimiento al aire solo en Failed. Vuelve al reposo ante un rechazo tardío sin entrega o un relanzamiento. El lado sale del contrato (`Plan.Side`, luego `Side` de la entrega) mediante `BodyFrame.IdealContactPoint`, sin cambiar AX-1.
- Pelota idéntica bit a bit con vistas visibles u ocultas. Sin colliders y sin cambios de cámara, tempo ni cámara lenta.
- RED: 8/12 fallaron por aserción; los otros 4 son salvaguardas que el esqueleto ya cumplía. GREEN completo: 546/546 y 338/338.

### D. Verificación final

> **Actualizado en E:** los totales de esta sección corresponden al head `f5bc7a2` y anteriores. Tras las correcciones: 546/546 EditMode y 353/353 PlayMode sobre `f21ed00`.

- `scripts/check-release.sh` comprueba ahora que el puente `SquashHaptics.mm` aparece exactamente una vez en dev y en prod, y que la salida IL2CPP referencia `SquashHapticsImpact` y `SquashHapticsPrepare`. Las demás comprobaciones quedan idénticas.
- Regresión completa fresca sobre `d13ddac`: **546/546 EditMode y 338/338 PlayMode**, sin errores CS. La base en `main` era 522/522 y 297/297.
- `check-release` dev y prod: **RELEASE-CHECK: OK** desde `d13ddac` limpio, con `gitDirty=false` en ambos perfiles. Prod incluye solo Core y Runtime, sin DevPanel ni framework de pruebas. El puente háptico y sus símbolos están en ambos perfiles. Los puentes térmicos no cambian.
- Archivos protegidos: ni `P0-calib-1.json`, ni `Po-1.json`, ni `last-active.txt` están en el diff. Tampoco hay cambios en spec, planes, propuesta, `ProjectSettings/`, `Packages/` ni en fuentes de física, solver, Aim o M2-4.
- La evidencia de RED y GREEN por incremento y las copias de `check-release` quedan en `Logs/evidence/s3-feedback/` (ignorado).

## E. Correcciones previas a la fusión

Revisión independiente de `f5bc7a2`: APROBADO CON OBSERVACIONES, sin bloqueantes. El PO decidió las correcciones (ver [registro](2026-10-25-first-shot-po-approvals.md#correcciones-previas-a-la-fusión-de-pr-34--5-de-noviembre-de-2026-chile)). Método: Claude (Opus) dirige y revisa, un subagente Sonnet implementa, Codex retoma después. **Esta revisión de Claude no es independiente**: se recomienda revisar el nuevo head con Codex u otra sesión de Claude antes de fusionar.

### Cambios por tarea

- **T1, pausa (`e74486d`).** `SwingView.Tick` y `PerfectFlashView.Tick` retornan mientras `app.Clock.Paused`. Una línea por vista, sin tocar el reloj. RED por aserción: 4/17 en `SwingViewTests`. GREEN focal 17/17 y PlayMode 343/343. La prueba de relanzar con pausa activa pasa antes de la corrección: es solo cobertura.
- **T2, rechazos tardíos (`d34c748`, solo pruebas).** Archivo nuevo `S3LateRejectionTests.cs`. `schedule-rejected` de extremo a extremo, reservando el host con `ScheduleShot` público. `invalid-velocity` emitido por el host. `present-not-mappable` no se puede producir con la API pública del host, así que no hay prueba a nivel de host ni se añadió un punto de inyección; se cubre con una prueba de controlador y vistas que emite `ShotApplied` por reflexión en código de prueba, igual que los efectos de `invalid-velocity` en el controlador. Tras el evento sintetizado el host aplica igualmente su golpe programado y el controlador lo ignora: esas pruebas muestran que no se presenta nada, no que la pelota siga sin golpear. Mutación: retirar el reinicio en `SwingView.OnResolved` hace fallar 3 pruebas por aserción. PlayMode 348/348.
- **T3, independencia y notas (`f21ed00`, solo pruebas).** `Ball_IsBitIdentical_WithViewsOnAndOff` se sustituye por una comparación real con vistas presentes frente a destruidas, por fotograma: bits de la pelota, `RenderPosition`, `CanonicalContacts` y `Prediction`. Se comprueba por aserción que las vistas no existen ni están suscritas en la segunda corrida. Control negativo con un suscriptor de prueba que avanza el host un paso: el comparador lo detecta. Pruebas de notas informativas para `NoOrder`, `SystemFault` y `NotEvaluable`: solo nota, sin presentación, sonido, háptica ni destello. `Prepare` con `feedback.haptics=0` no prepara. Mutaciones a, b1–b4 y c fallan por aserción y se revirtieron. PlayMode 353/353.

Único cambio de producto desde `f5bc7a2`: la línea de guarda en `SwingView.cs` y en `PerfectFlashView.cs`.

### Decisiones y aclaraciones

- **Decisión 1 (notas de resultados no elegibles): decidida por el PO.** Se mantienen «Sin golpe», «Fallo del sistema» y «No evaluable» como notas informativas, sin sonido de impacto, háptica ni destello. Sustituye la decisión 1 tomada por Claude en nombre del PO. Se distingue feedback de impacto, de fallo y notas informativas.
- **Decisión 5 (aclarada).** `feedback.haptics` se lee en cada lanzamiento. Un cambio requiere un lanzamiento nuevo y nunca afecta al contacto en curso. `ApplyClass` no cambia. No se altera el contrato de configuración sin propuesta propia.
- **Entrega única, caso inverso.** Si una identidad recibiera primero un resultado no elegible y después uno elegible: con el controlador actual no puede ocurrir, porque `Finish` se ejecuta una vez por intento. La puerta entregaría una vez, porque el resultado no elegible no reserva nada. No hay cambio de comportamiento. Lo cubre `IneligibleRecords_NeverDeliver`.
- **Sensación del swing (punto 6 del PO).** Se mantienen los 0,06 s de alcance de la raqueta y el `prepare()` existente, pendientes de validación en el iPhone (S3-A–E, con autorización propia).

### Correcciones a este informe

- «Cinco valores» en B (y en la decisión 6) son 5 líneas con 7 literales cambiados, no cinco literales.
- El RED de C para `Notes_MatchResult` falló por un error de montaje de la prueba (`Star()+.07` producía `Failed`), no por falta de comportamiento.
- La versión de `SlipGestures` con 4 casos se añadió después del GREEN, sin RED propio.

### Verificación final de E

- Regresión fresca sobre `f21ed00`: **546/546 EditMode y 353/353 PlayMode**, sin errores CS. Recorrido de PlayMode: 338, 343, 348, 353.
- `check-release` dev y prod: **RELEASE-CHECK: OK** con `gitDirty=false`; puente háptico y símbolos P/Invoke en ambos; prod sin DevPanel.
- Hashes intactos: defaults `c0d8ec7050e0`/107 claves, P0 efectivo `6ecfdfa78cbb`, SimulationKey `166a96e319c7`, proyecciones `0d39e6016bd8`/`8de3f2ecb217`. Presets protegidos sin tocar.
- Evidencia local ignorada en `Logs/evidence/s3-feedback/E/` (T1, T2, T3, final).
- PR #34 sin fusionar. S4 y First Rally sin autorizar.

## Fuera de alcance, conforme a la decisión 9

Sonidos de frontis, laterales y tin; audio de menor latencia y comparación Default/Best Latency; instrumentación de latencias y costes; sesiones de 15 min; calidad causal (3.1); superposición y poses causales (3.3). Sin cambios de solver, Aim, SL, SL5/SL6, P2, física, M2-4, `pullFullMm`, cámara, ayudas, tempo, spec, propuesta r2 ni planes. PR #32 sin tocar.

## Comprobado en Editor y pendiente para el iPhone

El Editor acredita peticiones, ausencia de duplicados, coherencia con rollback y que la pelota no cambia. No acredita lo que se oye, se siente ni se ve en el teléfono. Queda para S3-A–E, con autorización propia:
- distinguir los dos impactos y las dos hápticas;
- silencio, háptica del sistema apagada e interruptor de la app;
- legibilidad del destello (unos 18 cm) y de la raqueta (mismo material que la cápsula);
- si la preparación del swing sugiere un contacto falso;
- el desfase de la raqueta, que llega al punto de contacto 0,06 s después del frame en que suenan el impacto y la háptica;
- la oclusión.

El perfil vigente vence el **2026-11-12T13:55:07Z (10:55 Chile)**; renovarlo antes de instalar si la sesión cae después.

## Decisiones tomadas por Claude en nombre del PO

1. **Las notas de resultados no elegibles se mantienen** («No llega», «Sin golpe», «No evaluable», «Fallo del sistema»). Son texto informativo de S2, no señales de resultado de S3. Si el PO prefiere ocultarlas, el cambio es de una línea.
2. **Métricas `shot-presentation` y `shot-presentation-discrepancy`**, una línea por entrega, para correlacionar en el iPhone. Coste: una línea más por golpe en los logs.
3. **Un resultado distinto sobre una identidad ya entregada cuenta como discrepancia aunque no sea elegible** (por ejemplo, SystemFault después de Perfect). Con el flujo actual del controlador no ocurre.
4. **`Haptics.Impact(HapticStyle)` en lugar de `Impact(float)` del plan r2**, porque el PO aprobó dos estilos. Firme = Heavy y discreto = Light.
5. **`feedback.haptics` se lee por lanzamiento**, no en vivo. Un cambio desde el panel requiere aplicar y relanzar, como en S3-E.
6. **Actualizar cinco valores registrados de hash o número de claves** (`DefaultPresetHashTests`, `M24ProjectionTests`, `ShotSolverTests`, `ContactContractTests:96` y el hash de P0 en `S2DefaultPromotionTests`). Es el registro R2-3, con el precedente de PR #32. El encargo inicial omitía `ContactContractTests:96`; el subagente se detuvo y Claude lo autorizó. El resto de aserciones existentes no cambió.
7. **Se conserva `contact-provisional.wav`, sin uso en S3**, porque una aserción existente lo carga.
8. **Se acepta el RED del interruptor por excepción de clave desconocida** en lugar de por aserción, porque la falta de la clave es justo lo que se prueba y la prueba de catálogo falló por aserción.
9. **«Atrasado» se comprueba con una llamada directa a la nota**, cuyo código no cambia.
10. **La raqueta llega al punto de contacto 0,06 s después de la entrega**, y la preparación se mantiene sin límite de tiempo hasta el desenlace o un relanzamiento. Lo juzga el PO en el iPhone.
11. **Rama y PR únicas, con los incrementos como grupos de commits.** La evidencia queda en `Logs/` ignorado.
