# Squash Clash

Prototipo de squash para iPhone en Unity 6.3 LTS: validación progresiva, empezando por First Ball.

[Spec v0.3](docs/superpowers/specs/2026-10-20-squash-clash-design-v0.3.md) · [Plan First Ball r2](docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md) · [Entorno](docs/validation/environment.md) · [E1: instalación y evidencia](docs/validation/e1-first-installation.md) · [I2: cancha y cámara](docs/validation/i2-court.md)

Editor fijado: **6000.3.27f1 (Apple silicon)** con iOS Build Support. Configurar `scripts/device.env` a partir de `scripts/device.env.example`; el archivo real queda excluido de Git. Requiere la cuenta del PO en Xcode y el iPhone conectado, desbloqueado y con Modo de desarrollador.

```bash
scripts/test.sh editmode
scripts/test.sh playmode
scripts/check-release.sh
scripts/test.sh editmode BuildInfoTests
scripts/build-ios.sh dev
scripts/install-ios.sh dev --console
scripts/build-ios.sh prod
scripts/install-ios.sh prod
```

`dev` usa `SQUASH_DEV`, IL2CPP Release y Xcode Release, sin Development Player. `prod` omite el define. `debug` es Development Player e IL2CPP/Xcode Debug; sus métricas no son comparables. `--console` mantiene la sesión hasta cerrar la app; sin esa opción la instalación y el lanzamiento retornan.

`scripts/spike.sh baseline|speculative|offset05|solver12|no-ed` ejecuta la campaña de física del editor separada de la regresión. Salida 0 exige informe completo y limpieza de recursos; los veredictos físicos pueden ser Fail y se reportan como datos.

I1 se preparó en `main` como excepción de bootstrap; Claude revisó los commits locales y el PO validó ambas builds en el iPhone. El remoto y E2 requieren autorización independiente.

**E1/I1 (T1.1–T1.3) cerrado el 2026-10-21**, con revisión de Claude y validación del PO de ambas builds en el iPhone, incluido el giro físico. **T1.4 completada:** [remoto privado verificado](docs/validation/github-remote.md).

**I2 cerrado el 2026-10-22**: PR #1 aprobada por Claude tras AX-1, aceptación visual del PO y merge commit `9ef7422`. Cancha, configuración y cámara fija verificadas; [informe de I2](docs/validation/i2-court.md).

**I3 (T3.1–T3.3) implementado para revisión** — [PR #2](https://github.com/rafaallendes-boop/squash-clash/pull/2) en `firstball/i3-ball`: reloj/contratos, candidato P1 con escenas aisladas, pelota negra, barra mínima dev y pausa del sistema en Runtime. **PlayMode 37/37 y RELEASE-CHECK: OK**; Core conserva EditMode 109/109 de `f075217`, sin cambios posteriores. Builds dev/prod `0.1.0+c49d99b`, firmadas ARM64, con yaw positivo hacia la derecha de pantalla (−X), probado por proyección RED→GREEN. [Informe y evidencia de I3](docs/validation/i3-ball.md).

El PO comunicó la revisión de Claude sin bugs bloqueantes y autorizó la corrección de yaw y la instalación dev/prod para su checkpoint. La pérdida de pausa manual tras «Toca para continuar» queda registrada para I5, sin corregirla ahora. La aceptación del PO sigue pendiente: I3 no está cerrado ni fusionado. P1 no constituye elección de backend: la decisión pertenece al gate I4c. I4 y posteriores sin autorización.

**Checkpoint técnico en iPhone realizado:** dev/prod arrancan como `0.1.0+c49d99b`, 60 FPS observados y Portrait. Cuatro controles dev probados desde Duplicación; prod sin barra, logs `devpanel=present|absent` y reanudación desde segundo plano verificados. El PO confirma bloqueo/desbloqueo físico y «Toca para continuar» correcto en dev. Dev restaurada. [Capturas y resultados](docs/validation/i3-ball.md#checkpoint-técnico-en-el-iphone-completado--2026-10-22).

El bloqueo previo de acceso a Duplicación quedó resuelto por el PO. Su observación de pelota demasiado pequeña queda registrada para las ayudas/escala visual de T6.2, sin cambiar el radio de 20 mm ni adelantar esa etapa. Aceptación global del PO y fusión de I3 pendientes; no se inicia I4 ni se elige backend.

**I3 cerrado el 2026-10-22:** aceptado por el PO en el iPhone y PR #2 aprobada por Claude, fusionada con merge commit `e09a543`. Main sincronizada y worktree I3 retirado. El texto de entrega anterior describe su historial previo al cierre.

**Siguiente incremento autorizado: I4a / T4.1–T4.2**, escenarios y mediciones puras en `firstball/i4a-physics-measures`, con PR para Claude. I4b solo después de su revisión y fusión, en `firstball/i4b-physics-run`; detenerse al entregar su informe. Gate I4c, P2 e I5+ sin autorización. Los resultados que no cumplan se reportan sin cambiar criterios.

**I4a preparado para revisión:** T4.1–T4.2 en `firstball/i4a-physics-measures`; escenarios analíticos y mediciones/informe sintéticos, **155/155 EditMode y 37/37 PlayMode**, código `980529d`. [Informe y evidencia](docs/validation/i4a-physics-measures.md). I4b aún no empieza: requiere revisión de Claude y fusión de I4a. No hay campaña PhysX, gate, elección P1/P2 ni instalación nueva en esta entrega.

Entrega I4a publicada en [PR #3](https://github.com/rafaallendes-boop/squash-clash/pull/3), abierta hacia main. Worktree conservado para la revisión; I4b no iniciado.

I4a cerrado: PR #3 fusionada (`ca50142`), ajustes de Claude verificados RED→GREEN, EditMode 159/159 y PlayMode 37/37. I4b autorizado; gate I4c/P2/I5+ pendientes de autorización.

**Estado actual — I4b, entrega parcial:** [PR #4 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/4), código `ee76086`, EditMode170/170 y PlayMode43/43. Runner incremental, benchmark y controles de escenario/prueba en dev; exclusión real de prod verificada. Cinco variantes completas del editor, ninguna cumple todos los criterios: [informe físico](docs/reviews/2026-10-22-first-ball-physics-spike-P1.md). Build dev Release ARM64 firmada instalada/abierta; escenario/cancelación comprobados en iPhone, falta campaña completa y C17 nativo. Clasificación de Claude pendiente. [Validación y resoluciones](docs/validation/i4b-physics-run.md). I4b no cerrado; I4c/P2/I5+ siguen sin autorizar.
