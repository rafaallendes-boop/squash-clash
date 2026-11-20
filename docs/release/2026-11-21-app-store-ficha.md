# Ficha de App Store y TestFlight — borrador

21 nov 2026 · Borrador de Claude para decisión del PO. Nada de esto se ha enviado ni creado en App Store Connect. Los textos son propuestas; la política de privacidad describe el comportamiento verificado en el código (ver [informe](../reviews/2026-11-21-store-prep.md)).

## 1. Datos de la ficha

| Campo | Propuesta | Límite Apple |
|---|---|---|
| Nombre | Squash Clash | 30 caracteres |
| Subtítulo (ES) | Squash de un dedo, bola a bola | 30 |
| Subtitle (EN) | One-finger squash, ball by ball | 30 |
| Categoría principal / secundaria | Juegos / Deportes | — |
| Palabras clave (ES) | squash,raqueta,deporte,arcade,rally,partido,reflejos,pared,pelota,un dedo | 100 en total |
| Keywords (EN) | squash,racquet,sports,arcade,rally,match,reflex,wall,ball,one finger | 100 |
| Precio | Gratis, sin compras ni anuncios | — |
| URL de soporte | Obligatoria (ver §6) | — |
| URL de política de privacidad | Obligatoria (ver §5) | — |
| Cifrado (exportación) | `ITSAppUsesNonExemptEncryption = false` ya está en el Info.plist: responder que la app no usa cifrado no exento (solo el de sistema) | — |

### Descripción (ES)
Squash Clash es squash arcade para jugar con un dedo. Arrastra para tensar, suelta para golpear y lee las paredes: cada bola cuenta.

- Gesto de golpe con potencia y dirección: tira hacia atrás, apunta y suelta.
- Rally y partido completo con marcador, saques y rivales con estilos distintos.
- Cansancio: tu energía y la del rival bajan con el esfuerzo; administrar el ritmo importa.
- Tres dificultades, récords personales y un tutorial corto.
- Sin cuentas, sin anuncios, sin compras y sin conexión: todo queda en tu dispositivo.

### Description (EN)
Squash Clash is arcade squash you play with one finger. Pull back to load the shot, release to hit, and read the walls: every ball counts.

- A pull-and-release stroke that sets power and direction.
- Rallies and full matches with scoring, serves and rivals with different styles.
- Fatigue: your energy and your rival's drop with effort, so pacing matters.
- Three difficulty levels, personal records and a short tutorial.
- No accounts, no ads, no purchases and no connection needed: everything stays on your device.

## 2. Clasificación por edades
Cuestionario de App Store Connect (versión actual): todas las preguntas de contenido en «Ninguno/No» (violencia, sexual, desnudez, lenguaje soez, terror, alcohol/tabaco/drogas, temas maduros, juego de azar simulado o real, cajas de botín), sin funciones de comunicación ni contenido generado por usuarios, sin acceso web sin restricción, sin compras. Resultado esperado: **4+**. Confirmar el resultado que muestre la herramienta, porque Apple modifica el cuestionario. Fuente: [valores y definiciones](https://developer.apple.com/help/app-store-connect/reference/app-information/age-ratings-values-and-definitions).

## 3. Privacidad de la app (App Privacy)
Respuesta: **«Datos no recopilados» (Data Not Collected)**; sin seguimiento. Base verificada (informe, §Hallazgos): ningún código de red (`UnityWebRequest`, `HttpClient`, `System.Net`, Analytics, Cloud Diagnostics, crash reports) en el código que se entrega; `submitAnalytics` pasa a 0 y `enableCrashReportAPI` está en 0; los datos (presets, métricas, GUID aleatorio, récords) se escriben solo en el dispositivo. Apple define «recopilar» como transmitir datos fuera del dispositivo ([definiciones](https://developer.apple.com/app-store/app-privacy-details/)). Manifiesto de privacidad: `Assets/Plugins/iOS/PrivacyInfo.xcprivacy` ([requisitos](https://developer.apple.com/documentation/bundleresources/privacy-manifest-files)). Si el PO añade analítica, TestFlight con envío de datos o un servidor, esta respuesta deja de valer.

## 4. Capturas y TestFlight
- Capturas de iPhone: el conjunto vigente exige la pantalla de **6,9"** (1320×2868 o 1290×2796 en vertical; 3 a 10 imágenes); 6,5" es opcional si se entrega 6,9". iPad no aplica si la app es solo iPhone (verificar el destino del proyecto antes de subir). Fuente: [especificaciones de capturas](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications). El pipeline `scripts/capture.sh` y `Tests/Capture` ya generan 1290×2796.
- **Qué probar (TestFlight, ES):** «Juega un partido completo en la dificultad Medio y cuéntanos: (1) ¿el gesto de golpe es fácil de entender sin el tutorial? (2) ¿te cansas o te aburres antes del final? (3) cualquier cierre, congelamiento o golpe que no hizo lo que esperabas. Si cierras la app a mitad de punto, avísanos cuándo ocurrió.»
- **Test Information:** descripción breve, correo de contacto de feedback y notas de revisión («Sin cuenta ni conexión; juego offline; el menú de inicio permite jugar de inmediato»). Para la beta externa Apple pide además información de contacto.

## 5. Política de privacidad (texto listo para publicar)
> **Política de privacidad de Squash Clash** · Vigente desde [fecha].
> Squash Clash no recopila, transmite ni comparte datos personales. La app funciona sin conexión y no incluye cuentas, anuncios, analítica ni servicios de terceros.
> **Qué se guarda en tu dispositivo:** tus ajustes (dificultad, sonido, música, mano), tus récords y registros de juego (puntos, golpes, duración, versión de la app y un identificador aleatorio de la instalación, que no está ligado a tu identidad) y archivos de configuración. Estos datos nunca salen del dispositivo, salvo que tú los copies. Desinstalar la app los borra.
> **Terceros:** ninguno. No usamos rastreo ni identificadores publicitarios.
> **Niños:** la app no recopila datos de nadie, incluidos menores.
> **Cambios y contacto:** si esto cambia, actualizaremos esta página. Contacto: [correo de soporte].
Condición: esta redacción solo es cierta mientras (a) no haya red ni analítica y (b) se decida la apertura de Documents (decisión 1 del §8, que expone presets y métricas en la app Archivos).

Dónde alojarla (opciones, sin decidir): GitHub Pages del repositorio, una página de Notion/Google Sites pública, o un dominio propio. Requisitos: URL pública estable, HTTPS y sin inicio de sesión. La misma página puede servir de URL de soporte si incluye un correo de contacto.

## 6. URL de soporte
Apple exige una URL con forma de contacto para la ficha (puede ser la misma página de la política con un correo). TestFlight externo exige además un correo de feedback.

## 7. Pasos que solo puede hacer el PO (en orden)
1. Inscribirse en el **Apple Developer Program** (US$99/año) con el Apple ID del equipo de distribución; el equipo personal gratuito actual no sirve para TestFlight ([programa](https://developer.apple.com/programs/)).
2. En App Store Connect: crear el **identificador de paquete** (App ID) y el **registro de la app** con nombre, idioma principal y SKU. Confirmar que el bundle ID coincide con el del proyecto.
3. Crear una **clave de API de App Store Connect** (Usuarios y acceso > Integraciones), descargar el `.p8` una sola vez y guardarlo **fuera del repositorio**; escribir `RELEASE_TEAM_ID`, `ASC_KEY_PATH`, `ASC_KEY_ID`, `ASC_ISSUER_ID` en el `scripts/release.env` ignorado (plantilla `scripts/release.env.example`). Se corrige además la precondición `BUNDLE_ID` de `release-ios.sh` (observación abierta del 17 nov).
4. Publicar la política de privacidad y la URL de soporte; rellenar App Privacy («Datos no recopilados»), clasificación por edades, categoría y derechos de exportación.
5. Decidir las preguntas del §8 y pedir a Claude/Codex el `release-ios.sh --export` (prod, sin DevPanel) y luego `--upload`. La subida y la firma requieren autorización expresa del PO.
6. **Testers internos** (hasta 100, miembros del equipo en App Store Connect): disponibles tras el procesamiento de la build, sin revisión. **Testers externos** (hasta 10.000, por correo o enlace público): requieren **Beta App Review** de la primera build y la información de prueba del §4 ([TestFlight](https://developer.apple.com/help/app-store-connect/test-a-beta-version/testflight-overview)). Recomendación: empezar con internos y pasar a externos con la segunda build.

## 8. Decisiones abiertas del PO
1. **`UIFileSharingEnabled` y `LSSupportsOpeningDocumentsInPlace`:** hoy exponen Documents (presets y métricas, incluidos informes DEV) en la app Archivos, decisión expresa del PO para las pruebas. Para una build pública, proponer **apagarlos** (la política de privacidad es más simple y se evita exponer archivos internos); si se mantienen, la política y el informe de métricas deben decirlo. Cambiarlo toca `IosReleaseSettings`, `verify-ios-export.py` y las pruebas del carril: es un encargo propio.
2. Nombre, subtítulo, descripción y palabras clave (§1) y si el idioma principal será español o inglés.
3. Dónde alojar la política y el soporte (§5), y el correo de contacto público.
4. Primera ronda: solo internos o ya externos con Beta App Review.
5. Conservar o retirar los módulos `unityanalytics`/`unitywebrequest*` del `manifest.json`: hoy no se usan, y retirarlos reduce superficie, pero es un cambio de paquetes aparte.
6. El identificador `appleDeveloperTeamID` del equipo personal figura en `ProjectSettings.asset` y en documentos del repositorio público (no es un secreto, pero identifica la cuenta); decidir si se reemplaza al pasar al equipo de pago.
