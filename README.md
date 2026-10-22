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

`scripts/spike.sh` se incorporará en E4 para experimentos de física separados de las pruebas de regresión. No existe aún durante E1.

I1 se preparó en `main` como excepción de bootstrap; Claude revisó los commits locales y el PO validó ambas builds en el iPhone. El remoto y E2 requieren autorización independiente.

**E1/I1 (T1.1–T1.3) cerrado el 2026-10-21**, con revisión de Claude y validación del PO de ambas builds en el iPhone, incluido el giro físico. **T1.4 completada:** [remoto privado verificado](docs/validation/github-remote.md).

**I2 cerrado el 2026-10-22**: PR #1 aprobada por Claude tras AX-1, aceptación visual del PO y merge commit `9ef7422`. Cancha, configuración y cámara fija verificadas; [informe de I2](docs/validation/i2-court.md).

**I3 (T3.1–T3.3) implementado para revisión** — [PR #2](https://github.com/rafaallendes-boop/squash-clash/pull/2) en `firstball/i3-ball`: reloj/contratos, candidato P1 con escenas aisladas, pelota negra, barra mínima dev y pausa del sistema en Runtime. **PlayMode 37/37 y RELEASE-CHECK: OK**; Core conserva EditMode 109/109 de `f075217`, sin cambios posteriores. Builds dev/prod `0.1.0+c49d99b`, firmadas ARM64, con yaw positivo hacia la derecha de pantalla (−X), probado por proyección RED→GREEN. [Informe y evidencia de I3](docs/validation/i3-ball.md).

El PO comunicó la revisión de Claude sin bugs bloqueantes y autorizó la corrección de yaw y la instalación dev/prod para su checkpoint. La pérdida de pausa manual tras «Toca para continuar» queda registrada para I5, sin corregirla ahora. La aceptación del PO sigue pendiente: I3 no está cerrado ni fusionado. P1 no constituye elección de backend: la decisión pertenece al gate I4c. I4 y posteriores sin autorización.

**Checkpoint técnico en iPhone realizado:** dev/prod arrancan como `0.1.0+c49d99b`, 60 FPS observados y Portrait. Cuatro controles dev probados desde Duplicación; prod sin barra, logs `devpanel=present|absent` y reanudación desde segundo plano verificados. El PO confirma bloqueo/desbloqueo físico y «Toca para continuar» correcto en dev. Dev restaurada. [Capturas y resultados](docs/validation/i3-ball.md#checkpoint-técnico-en-el-iphone-completado--2026-10-22).

El bloqueo previo de acceso a Duplicación quedó resuelto por el PO. Su observación de pelota demasiado pequeña queda registrada para las ayudas/escala visual de T6.2, sin cambiar el radio de 20 mm ni adelantar esa etapa. Aceptación global del PO y fusión de I3 pendientes; no se inicia I4 ni se elige backend.
