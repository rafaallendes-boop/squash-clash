# Camino mínimo a la primera prueba con usuarios

Propuesta de Claude del 16 nov 2026, a pedido del PO. Las decisiones que la originan están en el [registro de aprobaciones](2026-10-25-first-shot-po-approvals.md#decisiones-del-po-tras-la-sesión-1-de-45-hacia-usuarios-reales-16-nov-2026-chile). Este documento propone; no decide nada. Cada fusión, instalación y cambio de zona protegida sigue necesitando la aprobación explícita del PO.

## 1. Dónde estamos (hechos verificados hoy)

- **Juego:**
  - `main` en `a1fa915`. Ya tiene el menú de inicio, el tutorial de 3 pasos, la pausa, el fin de partido y los modos Rally y Partido.
  - También tiene sonido de cancha, música, zonas seguras y zurdo/diestro.
  - La build prod no tiene panel DEV y oculta la línea técnica (#63).
- **Configuración:**
  - Con una instalación nueva, la app arranca con `DefaultPreset.json`.
  - P0-calib-1 solo difiere de los defaults en `launcher.repeatSeconds`, que es de práctica. Los usuarios jugarían, por lo tanto, lo mismo que el PO.
  - Hoy el default es `timing.validLimit = 1` (V) y `timing.latencyCompMs = 0`.
- **Métricas:**
  - Se registran también en la build prod (`AppBootstrap`, sin `#if`): `shot`, `shot-gesture`, `rally-point`, `rally-causal`, `frames` y otras.
  - **Solo se pueden sacar del teléfono por cable**, con `devicectl`. La app no expone sus archivos a la app Archivos y no tiene botón para compartirlos.
- **Firma: bloqueante para TestFlight.**
  - Los perfiles son «iOS Team Provisioning Profile» de un **equipo personal gratuito**. Vencen a los 7 días; el actual, el 23 nov a las 15:03 en Chile.
  - TestFlight y App Store Connect exigen el **Apple Developer Program**, que es pagado y anual.
- **Release:**
  - `CFBundleVersion` es 0.
  - El ícono y la pantalla de carga de #59 existen, pero no están conectados a ProjectSettings. App Store Connect rechaza una build sin ícono de 1024 px.
  - No hay un carril de archive y exportación para distribución.

## 2. Qué bloquea y qué puede esperar

| Bloquea la prueba | Puede esperar (observación abierta) |
|---|---|
| T3 pure como default (cambio en `DefaultPreset.json`) | Dirección en SL5 y SL6 (paralela o cruzada al fondo) |
| Sacar las métricas del teléfono del tester | Posición del rival ante golpes cortos desde adelante |
| Saber quién jugó qué: identificador anónimo de instalación y eventos de menú, tutorial y partido | γ 2,0, saturación con `pullFullMm` 35, R3 |
| **Solo para TestFlight:** Developer Program, ícono conectado, número de build, firma de distribución y declaración de cifrado | Calibración de latencia por usuario (se mide, no se construye) |
| Flujo menú → tutorial → partido → fin sin cuelgues en la build prod | Condiciones NotEvaluated de S3, README, las 11 dudas de #57 |

## 3. Dos olas

**Ola 0, presencial y sin TestFlight: lo más rápido.**
- **Formato:** 3 a 5 personas juegan en el iPhone del PO con la build prod, 15 minutos cada una. El PO observa sin explicar después del tutorial.
- **Qué evita:** no necesita el Developer Program. Las métricas se sacan por cable, como hoy.
- **Requisitos:** solo el encargo U1 a Codex, una autorización de instalación prod y el formulario de la §6.
- **Para qué sirve:** detecta lo grueso, como no entender el gesto o perderse en el menú, antes de abrir la prueba a más gente.

**Ola 1, por TestFlight: 10 a 15 personas en sus propios iPhone.**
- **Requisitos:** U1, U2 y U3, más el Developer Program activo.
- **Por qué esta ola:** da diversidad de modelos de teléfono y uso sin observador, que es lo más parecido al mercado.

## 4. Encargos a Codex: tres PRs, cada una con TDD, suites completas y aprobación del PO

- **U1. T3 pure como default** (zona de timing de Codex; `DefaultPreset.json` requiere la autorización del PO, §7).
  - **Cambio:** `timing.validLimit` pasa de 1 a 0. RED por aserción: un test que lea el default y espere T3 pure. Luego GREEN.
  - **Hashes:** se registran los nuevos de defaults, P0 y SL5 y se actualiza AGENTS §5. Que cambien es esperado y no es una regresión.
  - **AutoQA** `mvp` PASS/0/1. Es obligatorio porque toca timing.
  - **Escenarios:** si algún escenario de AutoQA asume V, Codex se detiene y reporta, sin cambiar el escenario.
- **U2. Carril de release, sin tocar gameplay:**
  - conectar el ícono y la pantalla de carga de #59;
  - número de build automático (`CFBundleVersion` = número de commits, o el que proponga Codex);
  - `ITSAppUsesNonExemptEncryption = NO`;
  - `UIFileSharingEnabled` y `LSSupportsOpeningDocumentsInPlace`, para que el tester comparta la carpeta `metrics` desde la app Archivos;
  - un script `scripts/release-ios.sh` que archive y exporte para App Store Connect con firma automática del equipo pagado. La clave de API de App Store Connect queda fuera del repo, igual que `device.env`.
  - **Compuertas:** `check-release.sh` OK y build prod instalable.
- **U3. Métricas mínimas para usuarios** (solo registro, sin UI nueva):
  - un `installId` aleatorio en cada línea, sin datos personales;
  - eventos `app-session` (inicio, segundo plano, regreso), `menu`, `tutorial-step` (paso y tiempo), `match-start` y `match-end` (resultado, marcador, duración, abandono) y `pause`.
  - Lo que ya existe no se duplica.

U1 va primero porque la ola 0 depende solo de él. U2 y U3 son independientes y pueden ir en paralelo, porque tocan archivos distintos. Codex trabaja con un solo Unity a la vez, así que irán una tras otra si comparten editor.

## 5. TestFlight, paso a paso

1. **El PO** se inscribe en el Apple Developer Program con su Apple ID. Claude no puede hacer pagos.
2. **App Store Connect:** registrar el bundle `cl.rafaallendes.squashclash` en el equipo pagado y crear la ficha de la app. Si el bundle quedó tomado por el equipo personal, se resuelve ahí.
3. **Claude** compila con `scripts/release-ios.sh` y sube la build con la clave de API del PO. Requiere la autorización de cada subida.
4. **Probadores:**
   - **Internos:** hasta 100, y deben ser usuarios del equipo en App Store Connect. No pasan revisión.
   - **Externos:** hasta 10.000, por correo o enlace público. La primera build pasa por Beta App Review, que suele tardar alrededor de un día.
   - Para la ola 1 conviene un **grupo externo con enlace público**.
5. **Información de prueba:** descripción, correo de feedback y política de privacidad. Al crear el grupo externo se confirma en App Store Connect cuáles de estos campos son obligatorios. Las builds vencen a los 90 días.
6. **Feedback del tester:** TestFlight trae capturas con comentario y reportes de cuelgues. A eso se suman el formulario y la carpeta `metrics`, que el tester comparte desde la app Archivos.

## 6. Qué medimos

**Métricas, por tester y por sesión, en segundo plano:**
- **Aprendizaje:** si termina el tutorial y cuánto tarda.
- **Ritmo de golpes:** golpes por minuto y reparto Perfect/Valid/Failed/NoOrder a lo largo de la sesión. Muestra si aprende.
- **Timing:** distribución de Δ, que muestra si los usuarios también se adelantan sin calibración.
- **Partidos:** largo de los rallies, partidos terminados o abandonados, y duración de la sesión.
- **Estabilidad:** pausas, salidas, cuelgues de TestFlight y p95 de frames.

**Feedback cualitativo** (formulario de 5 minutos al terminar, en lenguaje de jugador):
1. ¿Entendiste cómo golpear? ¿Qué te costó?
2. ¿La pelota fue donde querías?
3. ¿El momento de golpear se sintió justo?
4. ¿Qué fue lo mejor y lo más frustrante?
5. Del 0 al 10, ¿volverías a jugar?
6. Edad aproximada y si juega squash o tenis. Los perfiles se comparan después.

**Observador en la ola 0:** anota dónde se traba el jugador y qué dice en voz alta. No lo ayuda.

**Consentimiento:** un texto breve que explique que se registran datos de juego anónimos para mejorar el juego. En la ola 1, la misma información va en la política de privacidad.

## 7. Iterar después de cada ola

1. **Análisis:** en un día, Claude (o un subagente Sonnet) junta métricas y formularios en un informe de una página: los 3 problemas más frecuentes, con datos.
2. **Triage del PO:** clasifica cada problema como bloqueante, mejora o idea. Por iteración entran como máximo 2 o 3 cambios.
3. **Corrección:** Codex corrige, con una PR por cambio, TDD y AutoQA cuando toca timing, gesto o contrato. Claude revisa y el PO aprueba.
4. **Nueva build:** se publica la build N+1 en TestFlight para los mismos testers, más algunos nuevos.

El ciclo apunta a una semana. No se hacen sesiones físicas de aceptación por cada cambio: la validación son los usuarios.

## 8. First Shot: qué queda pendiente y qué riesgo asumimos

- **Sin aceptar:** First Shot, First Rally y First Match siguen sin aceptación. Sus criterios no cambian.
- **Tarea 4.5:** se hizo solo la sesión 1, con calentamiento, B1 (V) y B2 (T3 pure). No se hicieron B3, B4, T3-suave, H2 ni la sesión 2 ([hoja](../validation/2026-11-16-first-shot-4-5-session-sheet.md)).
- **Riesgos asumidos:**
  1. **Evidencia débil para T3 pure:** viene de un solo jugador y está mezclada con el orden y el aprendizaje.
  2. **Sin calibrar latencia:** los usuarios juegan con `latencyCompMs = 0`, mientras que el PO usó 61 ms.
  3. **Dirección en SL5 y SL6:** puede frustrar y queda abierta.
  4. **Balance del rival y R3:** no está resuelto.
- **Mitigación:** cada riesgo tiene una métrica en la §6 que lo vigila. Si alguno afecta mucho la experiencia, pasa a bloqueante en el triage.

## 9. Lo que el PO decide para arrancar

1. **Autorizar U1:** el cambio de `timing.validLimit` en `DefaultPreset.json` (zona protegida) y el cambio de los hashes vigentes.
2. **Encargar U2 y U3 a Codex**, o solo U1 si prefiere empezar por la ola 0.
3. **Inscribirse en el Apple Developer Program:** necesario solo para la ola 1.
4. **Ola 0:** fecha y personas, y la autorización de instalar la build prod en su iPhone cuando U1 esté fusionado.
5. **Herramienta del formulario:** por ejemplo Google Forms. El PO elige.
