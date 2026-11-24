# Anillo ámbar que se cierra, anillo blanco que pulsa y un texto único de la regla

24 nov 2026. Rama `feat/ring-close-pulse-rule-text` desde `f306d70`. Encargo de SC-PM, ítem 1 del set de recepción (`/Users/rafaallendes/sc-specs/set1-anillos.txt`). Spec: [2026-11-24-ring-close-pulse-design.md](../superpowers/specs/2026-11-24-ring-close-pulse-design.md). Arquitectura y spec: Claude Opus. Implementación: Claude Sonnet con TDD (AGENTS §2). **La revisión de Claude no es independiente**: la revisión independiente queda para Codex. No fusionar.

## Qué cambia (solo cómo se ve y el texto)

- **El anillo ámbar se cierra.** Mientras la volea no está elegida, el radio dibujado baja de 0,45 m a 0,20 m a lo largo de la ventana de toque (desde el lanzamiento hasta S′). 0,20 m es el círculo del triángulo, el doble del calce, para no confundirse con la señal de soltar. Si el radio de `ImpactCue` es menor, se usa ese. El halo sigue al anillo. Si no hay ventana finita, el anillo se dibuja como hoy.
- **El anillo blanco pulsa mientras espera** en el radio exterior: el alfa va de 1 a 0,45 y vuelve, a 2 Hz. Cuando empieza a converger (los últimos 500 ms) queda fijo en 1. El trazo, el radio, el triángulo y las bandas no cambian.
- **Un texto único de la regla** en `ReceptionRule`, que se usa en el paso 4 del tutorial, en la pista H1 y en la pastilla del anillo blanco (ver «Textos»).
- `ShotController.UpdateRingClosing()` pasa a la vista la ventana `TryVolleyWindow`, solo con la oferta ámbar sin elegir, antes de `ring.Tick` en `LateUpdate`. No cambia ninguna lógica.
- **Sin cambios:**
  - Cuándo aparece cada anillo o la pastilla.
  - El radio de calce, las bandas, el anillo ya elegido y el radio de toque del gesto.
  - Timing, contrato, reloj, solver, presets, umbrales y criterios. Los hashes de §5 no cambian.
  - `TimingAidTests` y `ZVolleyV2Tests`, sin tocar.

## Textos

| Texto | Dónde | Estado | Veredicto del autor |
|---|---|---|---|
| «Anillo ámbar: toca y volea en el aire. Anillo blanco: espera el bote y golpea.» | Tutorial, paso 4 (`FirstRunFlow`) | Cambia (antes: «Anillo ámbar visible: toca y voleas en el aire. Espera al anillo blanco: golpeas después del bote.») | OK: «volea» y «bote» son términos de squash; imperativo con tú; sin voseo. |
| La misma regla | Pista H1 (`FirstMatchHints`) | Cambia (antes: «Anillo ámbar: toca y volea antes del bote.») | OK. Entra en el banner de 280 px en unas tres líneas, como la pista de saque. |
| «Anillo blanco: espera el bote y golpea.» | Pastilla del anillo blanco (`ReceptionCaptionView`) | Cambia (antes: «Espera el bote», aprobado por el PO el 23 nov) | OK: es la mitad blanca de la regla, palabra por palabra, y conserva «espera el bote». |

**Ajuste respecto de la propuesta de la revisión de #107** («Anillo ámbar visible: toca y volea en el aire. Espera el anillo blanco: golpea después del bote.»):

- La pastilla aparece cuando el anillo blanco ya está visible, y ahí «Espera el anillo blanco» no tiene sentido.
- Con una mitad por anillo, la pastilla repite la regla tal cual.

**Preguntas para SC-Lead (no bloquean):**
1. ¿La pastilla debe mostrar la regla completa en lugar de su mitad? No se recomienda: son dos frases que tapan cancha y no se alcanzan a leer en la espera.
2. ¿Agregar «bote» al glosario de `regla-textos.txt`? Ya se usa y lo preguntó la revisión de #107.
3. ¿Una pastilla con la mitad ámbar sobre el anillo ámbar? Queda fuera de alcance, porque cambiaría cuándo aparece una pastilla.

## Pruebas (TDD)

- Prueba nueva `ZRingClosePulseTests.cs` (A1–A5, B1–B3, C1–C2 de la spec).
- **RED por aserción, con stubs vacíos de la API:**
  - Primera corrida `ZRingClosePulse`: 6 de 8 fallaron por aserción, por ejemplo A1 «expected 0.325, was 0.45» y B1 «expected 0.45, was 1.0». A3 y B2 pasan con los stubs, como se esperaba, porque describen lo que no cambia.
  - Las C fallaron por aserción con el texto viejo.
  - A5 se reescribió, porque el saque del rival con semilla 3 no ofrecía ámbar. La versión reescrita se corrió contra los stubs (`8106c7c`): 1 de 1 falló por aserción («it really closes», esperado < 0,449, obtuvo 0,45).
- **Cadenas de pruebas existentes cambiadas**, porque son los textos que este encargo reemplaza (no cambia ningún otro criterio):
  - `ZFirstMatchHintsTests.cs:26`: `VolleyText`.
  - `ZReceptionRingsTests.cs:104` y `:119`: el `Is.EqualTo` del texto de la pastilla.
- **GREEN de la implementación (filtro de 97 pruebas):** 97/97.
- **Inestabilidad observada por el implementador:** `ZFirstMatchHintsTests.NoAddedTime_EachPointStartsAtTheSameContinuousStep_OnAndOff` falló dos veces en corridas filtradas por una diferencia de un paso (3772 frente a 3771). Pasó sola y en la corrida de 97. No depende del texto. Se registra sin diagnosticar; ver la corrida completa abajo.

## Puertas

Sobre `cbabb4c`, en serie con el lock de Unity, en el Mac del PO:

| Puerta | Resultado |
|---|---|
| EditMode | **1027/1027** |
| PlayMode | **837/837**, incluida `NoAddedTime_…` |
| AutoQA `mvp` v / t3 / volley | PASS / 0 / nivel 1 en las tres (18 escenarios PASS, 102/102 pruebas cada una) |
| check-release | OK (build 1177, dev y prod) |

AutoQA marca `dirty: true`: el único cambio sin commitear era este informe (`docs/`). El código era idéntico a `cbabb4c`. `scripts/device.env` se copió sin cambios desde `sc-install-518f9c3` (SHA-256 `0b0c94db…`, idéntico al origen), no se versionó y se borró al terminar.

Evidencia local, ignorada por git: `Logs/evidence/ring-close-pulse/` (XML de RED de A5, EditMode, PlayMode, salidas de AutoQA y de check-release).

## Checklist para el iPhone

- [ ] El ámbar se ve cerrarse hacia el punto de golpe y se entiende como «se acaba el tiempo para tocar».
- [ ] Al elegir la volea, el anillo ámbar vuelve al radio exterior. Comprobar que no confunda.
- [ ] El blanco pulsa mientras espera, sin molestar, y deja de pulsar claramente al converger.
- [ ] La pastilla «Anillo blanco: espera el bote y golpea.» entra en una línea y no tapa la pelota.
- [ ] La pista H1 entra completa en el banner.
- [ ] El ámbar y el blanco se distinguen a pleno sol.
