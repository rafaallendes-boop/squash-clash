# Preparación para TestFlight/App Store: seguridad, privacidad y panel DEV

21 nov 2026 · rama `claude/store-prep`, base `main` `7fe6a42`. **Claude dirigió y ejecutó el trabajo: esta revisión no es independiente.** Sin push, fusión, iPhone, cuentas ni claves. `DefaultPreset.json`, física, timing y contrato de contacto sin cambios.

## Hallazgos
1. **Escaneo de secretos (solo lectura, 1.094 commits, todas las ramas).** Sin claves privadas, sin `.p8/.p12/.mobileprovision/.cer/.pem`, sin `device.env`/`release.env` versionados, sin tokens (AKIA, ghp_, sk-, xox, AIza), sin UDID reales (el único patrón es el marcador de `device.env.example`, ceros), sin correos salvo el del autor y dos de fixtures `@example.invalid`. Los 246 valores de 40 hex son objetos Git. **Un hallazgo de nivel bajo, no secreto:** el ID de equipo del Apple ID personal gratuito (`PYXN…`, enmascarado) está en `ProjectSettings/ProjectSettings.asset` y en `docs/validation/` desde el primer commit del repositorio público; los IDs de equipo no son credenciales (aparecen en toda app firmada), pero identifican la cuenta. No se reescribe el historial: decide el PO. `.gitignore` cubre `device.env`, `release.env`, `*.p8`, `Logs`, `Builds`; **faltaban** `*.p12`, `*.mobileprovision`, `*.cer`: se agregan.
2. **Red y datos.** Ningún uso de `UnityWebRequest`, `HttpClient`, `System.Net`, Analytics, Services ni CrashReport en el código entregado (prueba EditMode lo verifica). Documents se escribe en tres carpetas locales (U2). Las APIs de «motivo requerido»: Unity 6000.3 fusiona su manifiesto del motor (UserDefaults CA92.1, FileTimestamp 0A2A.1/C617.1, SystemBootTime 35F9.1, DiskSpace E174.1) con el de `Assets/Plugins/iOS` en `UnityFramework/PrivacyInfo.xcprivacy`, y su análisis de C# (`AppleRequiredReasonCSharpAPIs.txt`) quedó **vacío** para nuestro código (`LogRetention` solo lee `FileInfo.Length`; no hay marcas de tiempo, `DriveInfo` ni `systemUptime`; los `.mm` solo usan `thermalState`). Fuente: [política de manifiestos de Apple en Unity 6000.3](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/iphone/ios-building-and-delivering/apple-privacy-manifest-policy) y [Apple](https://developer.apple.com/documentation/bundleresources/privacy-manifest-files).
3. **`submitAnalytics`.** Es un campo serializado de PlayerSettings sin propiedad pública ni página en la documentación oficial 6000.3 (404 en el manual y la referencia de scripts; no se pudo contrastar con fuente oficial qué envía exactamente). Se fija en 0 por prudencia: solo puede reducir envíos, no hay efecto en el juego. `UnityConnectSettings` ya tiene Analytics, Ads, Purchasing, Cloud Diagnostics y Performance Reporting apagados, y `enableCrashReportAPI` es 0. Efecto en la etiqueta de App Store: ninguno si se mantiene 0; se responde «Datos no recopilados».

## Cambios
- `Assets/Plugins/iOS/PrivacyInfo.xcprivacy` (+ `.meta` solo iOS): sin seguimiento, sin dominios, sin datos recopilados, y `UserDefaults` CA92.1 (PlayerPrefs). No se repite `FileTimestamp`: Unity ya lo declara con C617.1 y repetirlo con otros motivos duplicaría la entrada fusionada.
- `scripts/verify-ios-export.py` (parte de `check-release`): exige el manifiesto fusionado en `UnityFramework/PrivacyInfo.xcprivacy` (sin seguimiento ni datos; CA92.1 y C617.1), que Unity registre la fusión del archivo del app, que el análisis de C# siga vacío y la referencia en el `.xcodeproj`.
- `ProjectSettings.asset`: solo `submitAnalytics 1 → 0`. `.gitignore`: `*.p12`, `*.mobileprovision`, `*.cer`.
- DEV: la barra inferior (`dev-bar`: Lanzar/Pausa/Paso/Repetir, Legibilidad, Escenario/Benchmark/Sesión H17) queda `visibility: hidden` salvo con el panel abierto o con un experimento/sesión en curso (para poder cancelar). El diseño conserva su sitio, así que el área de entrada y los demás cálculos no cambian. El botón «DEV» pasa de 58×44 a 40×28, 50 % de opacidad, en la esquina inferior derecha mientras está cerrado (zona libre de marcador, energía y cancha) y arriba a la derecha al abrir, como antes. Todas las secciones y comportamientos DEV se conservan; prod sigue sin DevPanel.
- Ficha: [docs/release/2026-11-21-app-store-ficha.md](../release/2026-11-21-app-store-ficha.md).

## RED / GREEN
- EditMode `StorePrepTests`: RED 4 de 5 por aserción (manifiesto ausente ×3, `submitAnalytics`), con la prueba de red como control que ya pasaba; GREEN 5/5.
- PlayMode `DevPanelUnobtrusiveTests`: RED 2 de 3 por aserción (barra visible cerrada y tras cerrar), el control de no solapamiento ya pasaba; GREEN 3/3.
- El export reveló que Unity fusiona los manifiestos; se corrigió el manifiesto y la verificación en un commit aparte.

## Compuertas
Ver el resultado final en el mensaje de entrega (cabeza, EditMode, PlayMode, AutoQA v/t3/volley, check-release dev/prod).

## Decisiones abiertas del PO
Las del §8 de la ficha: abrir o cerrar Documents (`UIFileSharingEnabled`/`LSSupportsOpeningDocumentsInPlace`) en la build pública; textos y correo; alojamiento de la política; internos o externos primero; retirar módulos Unity sin uso; ID de equipo en el repositorio público.
