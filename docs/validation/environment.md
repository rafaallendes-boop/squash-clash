# First Ball — diagnóstico de entorno E0

Fecha: 2026-10-21. Workspace: `/Users/rafaallendes/Projects/Squash Clash`.

**Estado: E0 cerrada y aprobada por el PO; comprobación técnica de E1 completada y revisión de Claude aprobada con dos observaciones menores; pendiente la validación del PO.** T0.1 y T0.2 completadas. La reconexión del cable resolvió el bloqueo del iPhone y el PO confirmó el bundle ID. **Compatibilidad Unity 6000.3.27f1–Xcode 27.1–iOS 26.6.2 VERIFICADA** con compilación, firma, instalación y arranque reales de `dev` y `prod` en el iPhone 16 Pro. Véase el [informe de E1 con evidencia](e1-first-installation.md). No se ha avanzado a E2 ni creado un remoto.

## Alcance y fuentes

El diagnóstico inicial corresponde a E0 del [plan r2](../superpowers/plans/2026-10-20-squash-clash-first-ball.md), contrastada con la [spec v0.3](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md), la [respuesta de Claude](../reviews/2026-10-20-first-ball-plan-review-response-claude.md) y la [revisión de Codex](../reviews/2026-10-20-first-ball-plan-review-codex.md). Tras la autorización expresa de E1, se actualizaron aquí las comprobaciones antes pendientes de T1.2/T1.3. Las aprobaciones D-1–D-6 y aclaraciones C-1–C-4 están en un [registro separado](../reviews/2026-10-21-first-ball-po-approvals.md).

El PO autorizó operar su computador para completar los preparativos interactivos. Durante E0 se instaló el editor mediante Unity Hub y todavía no se creó repositorio, proyecto ni build. Durante E1 se crearon el repositorio local, proyecto y pantalla de arranque, con pruebas y builds; el remoto sigue ausente. Los cuatro documentos fuente se conservan intactos.

## Inventario verificado

| Componente | Resultado observado | Estado |
|---|---|---|
| macOS | 26.6.2, build 25G83 | VERIFICADO |
| Arquitectura del Mac | `arm64` | VERIFICADO |
| Unity Hub | 3.22.3, sesión iniciada | VERIFICADO en interfaz |
| Licencia Unity | Personal; activada el 2026-10-19 | VERIFICADO en Hub → Settings → Licenses |
| Editor elegido | **6000.3.27f1**, changeset **672d562a42a7**, Apple silicon | VERIFICADO en Hub, binario e Info.plist |
| iOS Build Support | Instalado para 6000.3.27f1; directorios `il2cpp` y `arm64` presentes | VERIFICADO; exportación y compilación completadas en T1.3 |
| Otro editor existente | 6000.6.6f1, arm64, sin módulo iOS | Presente; no elegido para First Ball |
| Xcode activo | 27.1, build 27A9275 | VERIFICADO |
| Developer directory | `/Applications/Xcode.app/Contents/Developer` | VERIFICADO |
| SDK iPhone | `iphoneos27.1` | VERIFICADO |
| Preparación inicial Xcode | `xcodebuild -checkFirstLaunchStatus`: código 0 | VERIFICADO |
| git | 2.53.0 | VERIFICADO |
| GitHub CLI | 2.96.0; sesión existente en github.com | VERIFICADO; sin creación de remoto |
| iPhone físico | iPhone 16 Pro; identificador de producto `iPhone17,1` | VERIFICADO en Finder/CoreDevice |
| iOS del iPhone | 26.6.2, build 23G90 | VERIFICADO en CoreDevice y xcdevice |
| Conexión de desarrollo | `paired`, `wired`, túnel `connected`, DDI disponible | VERIFICADO tras reconectar el cable |
| Modo de desarrollador | `enabled` | VERIFICADO mediante consulta al dispositivo |
| Bundle ID | `cl.rafaallendes.squashclash` | CONFIRMADO por el PO |

Rutas elegidas:

```text
/Applications/Unity/Hub/Editor/6000.3.27f1/Unity.app
/Applications/Unity/Hub/Editor/6000.3.27f1/PlaybackEngines/iOSSupport
```

La segunda ruta alternativa, dentro de `Unity.app/Contents/PlaybackEngines`, no existe en esta instalación. Esto es válido: el plan contempla ambas ubicaciones y solo exige una.

## Versión y compatibilidad

La [página oficial de soporte](https://unity.com/releases/unity-6/support) identifica Unity 6.3 como la LTS más reciente, con soporte hasta diciembre de 2027. La [API oficial de releases](https://services.api.unity.com/unity/editor/release/v1/releases?limit=3&version=6000.3&order=RELEASE_DATE_DESC&platform=MAC_OS&architecture=ARM64) devolvió 6000.3.27f1, changeset `672d562a42a7`, publicada el 2026-10-20. Hub → Archive permitió seleccionar ese parche, aunque la lista inicial de instalaciones recomendadas todavía ofrecía 6000.3.26f1.

| Relación | Evidencia | Conclusión |
|---|---|---|
| Unity 6.3 / Mac arm64 | Editor nativo; proyecto creado, configuración aplicada dos veces y tests ejecutados | VERIFICADO — T1.2/T1.3 |
| Unity 6.3 / plataforma iOS | Módulo iOS; exportación dev/prod y compilación nativa completadas | VERIFICADO — T1.3 |
| Xcode 27.1 / macOS 26.6.2 | Apple requiere macOS 26.6 o posterior; CLI ejecutable | Requisito de host VERIFICADO |
| Xcode 27.1 / iOS 26.6.2 | CoreDevice conectado; instalación y lanzamiento por devicectl de ambas builds | VERIFICADO — T1.3 |
| Unity 6000.3.27f1 / Xcode 27.1 / iPhone físico | dev/prod: exportación, compilación con código 0, firma verificada, instalación y arranque con versión y 60 FPS | **VERIFICADO — T1.3** para esta combinación y pantalla de arranque |
| IL2CPP/ARM64 | Player IL2CPP Release; ejecutable y UnityFramework ARM64 inspeccionados en ambas apps | **VERIFICADO — T1.2/T1.3** |

Fuentes: [requisitos de Unity 6.3](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html), [matriz de SDK y soporte de Xcode](https://developer.apple.com/xcode/system-requirements). El rango publicado no prueba que la combinación exacta de exportación de Unity y toolchain funcione. El SDK iOS 27.1 del Mac y el sistema iOS 26.6.2 del teléfono son datos distintos.

No se instaló otra versión de Xcode. No se detectó una incompatibilidad que impidiera compilar y ejecutar. Se observó una advertencia libc++ por flags internos de IL2CPP con mínimo iOS 11, pese a que el proyecto, enlace y plist final usan iOS 15. Está descrita en el [informe de E1](e1-first-installation.md#advertencia-del-compilador-observada); no se ocultó ni se extrapola soporte a otros dispositivos o sistemas. La pantalla inicial a 60 FPS no evalúa todavía los criterios de física o rendimiento de First Ball.

## iPhone: conexión verificada e incidencia resuelta

El PO confirmó que desbloqueó el iPhone, aceptó confiar en el Mac y, después de la incidencia inicial, desconectó y volvió a conectar el cable. La consulta posterior confirma Modo de desarrollador habilitado.

**Resultado actual, después de reconectar:**

```text
Modelo: iPhone 16 Pro (iPhone17,1)
iOS: 26.6.2 (23G90)
pairingState: paired
transportType: wired
tunnelState: connected
developerModeStatus: enabled
ddiServicesAvailable: true
xcdevice: available=true, interface=usb, error=null
```

`xctrace` incluye ahora el iPhone bajo **Devices**, sin colocarlo en Offline. `devicectl device info details --device <UDID>` termina con código 0 y devuelve información completa. El dispositivo declara soporte tanto de `arm64e` como de `arm64`; el target de First Ball sigue siendo ARM64.

**Historial de la incidencia, anterior a la reconexión:**

1. `ioreg` identifica un iPhone conectado al bus USB, con el identificador de hardware correspondiente al teléfono. Finder muestra **iPhone 16 Pro**, batería y almacenamiento. La detección USB está confirmada.
2. Al principio `xctrace` mostró el nombre, iOS 26.6.2 y el UDID del teléfono, pero bajo **Devices Offline**. Tras aceptar confianza, mostró `Unknown` bajo **Devices Offline**.
3. La tabla inicial de `devicectl` llegó a mostrar `available`, pero la consulta de detalles devolvió `pairingState: unsupported`, `tunnelState: unavailable` y `ddiServicesAvailable: false`. La tabla resumida no basta para aprobar la conexión.
4. `devicectl manage pair --device <CoreDevice ID> --timeout 20` falló con **CoreDeviceError 1014**: no encontró `RemotePairingDeviceRepresentation`; solo había `RestorableDeviceRefDeviceRepresentation`. No se consiguió emparejar.
5. Device Hub → filtro Physical Devices mostró **No Physical Devices**. Se restauró el filtro All Devices. El iPhone 17 Pro con iOS 26.5 que aparece allí es un simulador, no el teléfono del PO.
6. `devicectl list preferredDDI` informa que la imagen iOS del host es compatible y utilizable. Esto no demuestra que esté montada ni disponible en el teléfono.
7. `xcrun xcdevice list --timeout 10` solo devolvió simuladores y el Mac; no devolvió el iPhone físico como destino.

El fallo inicial se clasificó como descubrimiento/emparejamiento del destino, antes de Unity y antes de la firma. **Quedó resuelto tras la reconexión**, sin que fuera necesario reiniciar el iPhone. No se determinó una causa interna más específica.

Se siguió el flujo de conexión por cable y confianza de [Device Hub](https://developer.apple.com/documentation/xcode/pairing-your-devices-with-your-mac). No se restablecieron ajustes de privacidad, no se borraron emparejamientos ni se modificaron firewall/VPN.

### Identificadores

Se observaron dos identificadores distintos: UDID de hardware y UUID CoreDevice. La consulta inicial con UDID falló mientras el dispositivo no estaba conectado correctamente. **Después de reconectar, `devicectl --device <UDID>` sí funciona**. Se puede usar el mismo UDID como `DEVICE_UDID` para Xcode y como `DEVICE_ID` para `devicectl`, conforme a T0.2.3; no es necesario configurar dos valores distintos en este entorno.

Los identificadores completos del dispositivo no se copian a este documento ni al ejemplo versionable. En E1 se creó `scripts/device.env` después de verificar su exclusión en `.gitignore`; es local y tiene permisos 0600.

## Cuenta y requisitos de firma

- Xcode → Settings → Apple Accounts contiene la cuenta del PO y muestra **Personal Team**. No se acreditó una membresía de pago; no es un requisito que se haya añadido a First Ball.
- El llavero tiene **una identidad Apple Development válida**, con clave de firma utilizable según `security find-identity -v -p codesigning`.
- El certificado tiene **Team ID `PYXNMVF47Z`** en el campo OU, válido desde 2026-07-10 hasta 2027-07-10. El sufijo entre paréntesis del nombre del certificado no es el Team ID. Ambas apps firmadas incluyen efectivamente ese equipo.
- En E0, el único perfil local encontrado era `iOS Team Provisioning Profile: cl.rallyiq.app`, vencido el **2026-08-11**. No se borró ni se reutilizó.
- En T1.3 la firma automática con `-allowProvisioningUpdates` resolvió un perfil nuevo: `iOS Team Provisioning Profile: cl.rafaallendes.squashclash`, equipo correcto y dispositivo registrado. Vence el **2026-10-28T14:59:31Z**; después será necesario renovar el perfil y reinstalar. La firma es Apple Development/Personal Team en ambos perfiles de Player; `prod` aquí es una instalación local, no una distribución App Store.
- Bundle ID **confirmado explícitamente por el PO el 2026-10-21**: `cl.rafaallendes.squashclash`.
- Firma, instalación y arranque de `dev` y `prod`: **VERIFICADOS — T1.3**. El primer lanzamiento requirió confiar manualmente en el desarrollador en el iPhone; el PO lo hizo y confirmó que abrió. Los lanzamientos posteriores por CLI funcionaron. No se creó un certificado de distribución.

No se exportaron claves privadas ni credenciales. El ejemplo de entorno contiene placeholders para los identificadores personales.

## Acciones realizadas en las aplicaciones durante E0

- Unity Hub: instalación oficial de 6000.3.27f1 Apple silicon con **solo iOS Build Support** como módulo adicional; comprobación de licencia Personal existente.
- Xcode: finalización de las pantallas iniciales; no se descargó el runtime adicional de simulador iOS 27.1, porque el SDK para dispositivo ya está instalado. Se eligió **Never** en la pantalla inicial de acceso de agentes externos. La automatización CLI del plan no requiere conceder ese acceso.
- Xcode: inspección de cuenta y equipo; apertura de Device Hub. No se añadieron cuentas, certificados ni perfiles.
- Finder: consulta del teléfono. Apareció una pregunta sobre cifrado de respaldos; no se eligió cifrar ni dejar de cifrar, y no se inició un respaldo o sincronización. Ese aviso es ajeno al build.

## Evidencia reproducible de comandos

Las salidas se resumen y omiten emails, tokens e identificadores completos del teléfono.

| Comando / comprobación | Resultado |
|---|---|
| `sw_vers` | macOS 26.6.2 (25G83) |
| `uname -m` | `arm64` |
| `xcode-select -p` | `/Applications/Xcode.app/Contents/Developer` |
| `xcodebuild -version` | Xcode 27.1 (27A9275) |
| `xcodebuild -showsdks` | Incluye `iphoneos27.1` e `iphonesimulator27.1` |
| `xcodebuild -checkFirstLaunchStatus` | Código 0, sin salida |
| `file <editor>/Contents/MacOS/Unity` | `Mach-O 64-bit executable arm64` |
| `plutil -p <editor>/Contents/Info.plist` | `CFBundleVersion=6000.3.27f1`, `UnityBuildNumber=672d562a42a7` |
| Comprobación de directorios iOS | `iOSSupport`, `il2cpp`, `arm64`: presentes |
| `git --version` / `gh --version` | 2.53.0 / 2.96.0 |
| `gh auth status` | Sesión existente; token no transcrito |
| `security find-identity -v -p codesigning` | Una identidad válida |
| Certificado público → `openssl x509 -noout -subject -dates` | OU y vigencia indicados arriba |
| Perfil local → `security cms -D` | E0: RallyIQ expirado. E1: perfil nuevo Squash Clash, equipo y dispositivo comprobados; vencimiento indicado arriba |
| `xcrun devicectl list devices --json-output -` | Tras reconectar: iPhone físico conectado, emparejado y por cable |
| `xcrun xctrace list devices` | Tras reconectar: iPhone 26.6.2 bajo Devices |
| `xcrun xcdevice list --timeout 5` | Tras reconectar: iPhone físico, `available=true`, `interface=usb`, sin error |
| `xcrun devicectl device info details --device <UDID> --json-output -` | Código 0; Modo de desarrollador enabled y DDI disponible |
| `xcrun devicectl manage pair --device <CoreDevice ID> --timeout 20` | Intento inicial: código 1, CoreDeviceError 1014; incidencia resuelta al reconectar |
| `xcrun devicectl list preferredDDI` | DDI iOS del host utilizable; posteriormente se verificó `ddiServicesAvailable=true` en el teléfono |
| `ioreg -p IOUSB -l -w 0` | Nodo iPhone USB presente |

## Checklist y siguiente revisión

- [x] T0.1.1: línea LTS y parche exacto confirmados.
- [x] T0.1.2: Hub, licencia e instalación con soporte iOS.
- [x] T0.1.3: binario arm64 y módulo iOS comprobados.
- [x] T0.1.4: versiones, fuentes y compatibilidad registradas.
- [x] T0.2.1: cable, confianza, Modo de desarrollador y conexión de desarrollo operativa comprobados.
- [x] T0.2.2: cuenta presente, Team ID obtenido del certificado y bundle ID confirmado por el PO. La firma efectiva corresponde a T1.3.
- [x] T0.2.3: iPhone conectado/emparejado, con identificadores consultados y UDID aceptado por ambas herramientas.
- [x] T0.2.4: `scripts/device.env.example` creado con versión fijada y placeholders.

Se comprobaron los SHA-256 de los cuatro documentos fuente antes y después de E0: sin cambios. Se verificaron los enlaces locales de los documentos nuevos y la sintaxis de `device.env.example` con `zsh -n` (código 0). Al cierre de E0 todavía no existían `.git`, `Assets`, `Packages` ni `ProjectSettings`; se crearon posteriormente en E1 autorizada.

No quedan acciones manuales pendientes para la preparación de E0. El PO revisó este diagnóstico y autorizó E1 el 2026-10-21. La comprobación técnica de E1 está completada y la revisión de Claude aprobada; queda la validación del PO, incluido el giro físico en ambas builds. El detalle, comandos, resultados y capturas están en el [informe de E1](e1-first-installation.md). Se detiene el trabajo antes de E2 y de crear un remoto.
