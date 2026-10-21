# First Ball I1 — revisión de Claude

Fecha: 2026-10-21. Origen: revisión de Claude transmitida por el PO en la conversación, tras inspeccionar el código y el estado del repositorio hasta `8b39812`. Este documento registra esa revisión; Codex no la presenta como una nueva ejecución propia de pruebas.

## Veredicto

**APROBADA con dos observaciones menores, ninguna bloqueante.** Al recibir la revisión quedaba pendiente la validación física y visual del PO en `prod` y `dev`; posteriormente completada, como se registra abajo.

## Comprobaciones comunicadas

- Historia de bootstrap en `main`, commits coherentes y ausencia de remoto. `scripts/device.env` ignorado; UDID ausente de los archivos versionados.
- Contrato H16 de `test.sh`: XML nuevo, total positivo, cero fallos, salida de Unity 0, ausencia de errores C# y resumen `RESULT:` incluso ante salida anticipada.
- Perfiles de `BuildScript` conformes a `AGENTS.md`, defines mediante `extraScriptingDefines`, rechazo de `SQUASH_DEV` persistente en `prod`, Development solo en `debug` y comprobación del bundle ID.
- Metadata obligatoria en Player, respaldo limitado a Editor y log de arranque completo en `AppBootstrap`.
- Autorrotación desactivada en las cuatro orientaciones y combinación Unity/Xcode/iPhone registrada como verificada.
- Desviaciones del plan justificadas. El hash `ea0a1e1` de las apps corresponde al código compilado; `8b39812` registra después documentación y evidencia.

## Observaciones y seguimiento

| ID | Observación | Resolución / momento de comprobación |
|---|---|---|
| CLAUDE-I1-01 | `SetIl2CppCompilerConfiguration` modifica un ajuste persistente. Una build `debug` podría dejar `ProjectSettings.asset` modificado y afectar la marca de workspace sucio de builds posteriores. | Pendiente de comprobar al usar `debug` por primera vez: inspeccionar el estado y diff antes/después, y la metadata de la siguiente build `dev` o `prod`. No se ejecuta `debug` ni se modifica el pipeline por esta observación durante el cierre de I1. |
| CLAUDE-I1-02 | El Team ID aparece en la documentación. No es un secreto ni identifica el iPhone. | Su presencia se considera aceptable para el futuro repositorio privado, según la revisión transmitida. Mantenerlo como decisión explícita al autorizar T1.4; no implica autorización para crear el remoto. |

La advertencia libc++ y el vencimiento del perfil el **2026-10-28T14:59:31Z** se consideran correctamente documentados. La renovación y reinstalación deben tenerse presentes para futuras pruebas; este registro no crea una tarea programada.

## Punto de control y cierre posterior

El PO completó el 2026-10-21 la comprobación de título, versión, perfil, FPS y permanencia en vertical al girar físicamente el teléfono en ambas builds. La aceptación y el cierre de I1 están registrados en el [informe de E1](../validation/e1-first-installation.md) y en el [registro del PO](2026-10-21-first-ball-po-approvals.md). Este seguimiento no amplía el rango de código revisado por Claude.

Las propuestas de crear un remoto privado, iniciar I2 en `firstball/i2-court` o adelantar T3.1 son siguientes pasos recomendados, **no autorizaciones vigentes**. I1 está cerrado; T1.4, E2 y T3.1 continúan sin autorización.
