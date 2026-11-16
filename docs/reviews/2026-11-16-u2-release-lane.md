# U2 — carril de release iOS

16 nov 2026 · Codex · `codex/u2-release-lane`, base `origin/main` 10bcaec, independiente de #65/#66. Sin cambios de gameplay ni instalación.

Alcance: conectar icon-1024.png y launch-screen.png a PlayerSettings iOS, build number automático desde número de commits de HEAD (crece con cada commit; recompilar el mismo HEAD conserva el número), postproceso plist de cifrado exento y acceso a Documents, y script de archive/exportación/subida. El número se aplica durante la exportación y se restaura después, sin editar ProjectSettings a mano por build. Los ajustes de arte se commitean antes de PlayMode.

Fuentes oficiales: [iconos Unity 6000.3](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/setplatformicons), [imagen de arranque](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/ios/setlaunchscreenimage), [compartir Documents](https://developer.apple.com/library/archive/documentation/General/Reference/InfoPlistKeyReference/Articles/LaunchServicesKeys.html), [declaración de cifrado](https://developer.apple.com/documentation/bundleresources/information-property-list/itsappusesnonexemptencryption). Archivo/exportación con firma automática y app-store-connect conforme a `xcodebuild -help` local; clave API solo por entorno o archivo ignorado.

No se validará distribución con el equipo personal gratuito. Quedan pendientes Developer Program, ficha/bundle del equipo pagado y credencial local; no se solicita ni lee ninguna clave durante la preparación. No se ejecuta ningún paso con credenciales. AutoQA no corresponde a este alcance sin gameplay. TDD, suites completas y check-release dev/prod pendientes.

## Parada antes de implementar

RED fresco **0/6 por aserción**, sin errores CS. El chequeo previo informó dos procesos de editor o auxiliares, pero la secuencia de Codex no cortó tras su error e inició el test. Sin PID/comandos guardados no se puede acreditar exclusividad; no se atribuye una causa. **Detenido por AGENTS §2, sin reintento ni implementación.** Solo tests, esqueletos y documentación; ProjectSettings y gameplay intactos. [Incidente y límites](../validation/2026-11-16-u2-operator-stop.md), [evidencia curada](../validation/evidence/2026-11-16-u2-summary.json). Suites completas, script release-ios y check-release pendientes. Ningún paso nativo o con credenciales ejecutado.
