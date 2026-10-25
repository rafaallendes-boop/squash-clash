# First Shot — aprobaciones del PO

## Cierre de diseño y autorización S1 — 2026-10-25

El PO aprueba la [propuesta r2](../superpowers/specs/2026-10-24-first-shot-design.md) y el [plan r2](../superpowers/plans/2026-10-25-squash-clash-first-shot.md), comunica la aprobación de Claude del head **da2390b2586b13aeeacd2b30df45b409a5a1b0de** y autoriza fusionar [PR #16](https://github.com/rafaallendes-boop/squash-clash/pull/16) únicamente si su diff sigue limitado a los cuatro Markdown aprobados.

Condición comprobada contra main `a8da9c43ee1b82c506d50c9d5434fe616e5957b6`: exactamente propuesta First Shot, plan First Shot e informes Codex del 24 y 25. Diff SHA-256 `83e5b55dcace4f5cc5afa0f2a3f5ce106efdf143a6d4266f1de9bbf0387527b1`. Head remoto y local coincidentes; PR ya lista. Se fusiona mediante **merge commit b43159e958429671210771b75681670045a7534c**, padres `a8da9c43ee1b82c506d50c9d5434fe616e5957b6` y head aprobado `da2390b2586b13aeeacd2b30df45b409a5a1b0de`. Main sincronizada por fast-forward. El worktree `firstshot/design-review` se elimina sin force; no contiene archivos ignorados que trasladar. Auditoría local preservada en `Logs/archive-firstshot-design-20261025`.

Decisiones aprobadas, con texto normativo en propuesta §11.1:

| Decisión | Registro |
|---|---|
| R2-1 | Clasificación con timestamp exacto; contacto físico en paso más cercano, sin integración parcial ni cambios P2. Error de cuantización registrado. |
| R2-2 | N1 exige salir y volver al círculo; N2 usa desplazamiento máximo del recorrido, estrictamente <3 mm y <250 ms. |
| R2-3 | Se autorizan claves nuevas en DefaultPreset.json y nuevo hash global; 41 valores heredados y SimulationKey física intactos. No promover P0-calib-1. |
| R2-4 | En borde, paso legal vecino del mismo lado; nunca contacto ilegal, error máximo de un paso con signo; empate al anterior. r ilegal no se rescata. |
| R2-5 | Calibración como offset perceptivo conjunto, con incertidumbre/repetibilidad; no se atribuye a latencia del dispositivo. |

**S1 autorizado:** Tareas 1.0–1.4, rama/worktree `firstshot/s1-gesture`, PR para revisión de Claude. Tarea 1.0 es preparación de manifest/asmdef, no RED. Conducta: esqueletos compilables, RED por aserciones observado y GREEN con regresión completa. Spec v0.3, criterios/umbrales y valores heredados intactos; registrar claves nuevas/hash. Al entregar, check-release dev/prod, informe y PR lista **sin fusionar**, y detenerse. **S2–S4 y T8 no autorizados.**

Tarea 1.4: comprobar vigencia efectiva antes de instalar, renovar antes de instalaciones desde 2026-10-27. El PO realiza comprobación con regla y flicks; registrar datos medidos sin asumir frecuencias ni inventar evidencia. P0-calib-1 pendiente de comprobar en la próxima apertura; sirve la instalación S1. El cierre documental no incluye tests/builds/instalación ni afirma aceptación de S1.

## Ejecución S1 — evidencia técnica del 2026-10-25

Tareas 1.0–1.3 implementadas y probadas, sin aceptación de S1 inferida. Fuente funcional b37078f, 330/330 EditMode y 161/161 PlayMode; check-release dev/prod OK. R2-3 aplicado: 17 claves nuevas (58 en total), hash global 00a501ad7066; 41 valores heredados/proyección 0d39e6016bd8 y SimulationKey física 166a96e319c7 intactos. Adaptación exacta de dos sujetos de aserciones heredadas declarada en [informe S1](2026-10-25-first-shot-s1-gesture.md), sin cambiar umbrales ni criterios.

Dev 311cb11, árbol limpio, Release ARM64 no Development, instalada tras verificar firma/perfil efectivos el 25 a las 04:51:18Z: válido hasta 2026-10-28T14:59:31Z, sin renovación necesaria hoy. 199 archivos coinciden con las fuentes probadas. Arranque 04:51:39Z: P0-calib-1 activo, hash extendido 9977d9ffd7e5/proyección heredada 8de3f2ecb217, cero pendientes. P0-calib-1, Po-1 y last-active idénticos antes/después de instalar. El pendiente de próxima apertura queda satisfecho.

Tarea 1.4 en curso: mediciones físicas y juicio del PO pendientes; ninguna frecuencia, mm reales ni veinte flicks dados por comprobados. Sin fusión de S1 ni autorización de S2–S4. Renovar el perfil antes de instalar desde el 27 y comprobar siempre vigencia.

## Devolución del PO y corrección S1 — 2026-10-25

El PO comunica que hizo los bloques varias veces, con mano dominante izquierda y regla física. Al medir, a veces el dedo se interrumpía o paraba antes; considera bien una lectura aproximada 48–55 mm. Dice que, al no salir la pelota y ver solo datos, no entendió bien la prueba. **No es una aceptación de S1, de la precisión de apuntado ni del control de potencia.** La comprobación física aproximada no certifica una referencia exacta de 50 mm por intento, ni se asume DpiSource=PhysicallyVerified.

Datos y limitaciones en el [informe S1](2026-10-25-first-shot-s1-gesture.md) y su JSON: 105 registros, una adquisición de 120 s y dos incompletas; una serie de diez soft y dos de diez strong más parciales. La repetición se mantuvo en 3 s y cortó gestos por relanzamiento; presets/selección byte a byte intactos. No se declara prueba diestra ni frecuencia del sensor.

Dentro de la autorización S1 se corrige un error de borde en potencia (340ca2c): una pausa previa que terminaba en la muestra exacta del inicio de W' invalidaba tres fuertes. Se conserva el registro original de 311cb11 y se reproducen las trazas tras corregir, sin modificar parámetros, criterios, umbrales o defaults: 65,193/58,865/40,255% calculados offline. RED válido con seis pruebas compilables/cinco fallos por aserción; GREEN y regresión fresca 336/336 EditMode y 161/161 PlayMode. Se prepara revisión Claude con estas limitaciones; no hay autorización de fusión ni S2–S4. No se pide al PO repetir ahora las pruebas que no entendió.

Entrega técnica final de S1: check-release dev/prod repetido OK, build corregida c6c68c1 limpia Release ARM64 firmada e instalada, 200 archivos cotejados. Perfil efectivo verificado el 25 a las 14:30:45Z, vigente hasta 2026-10-28T14:59:31Z. El primer lanzamiento fue rechazado por iPhone bloqueado; tras apertura del PO, arranque corregido observado 2026-10-25T14:37:43.8924920Z (11:37:43 America/Santiago): c6c68c1 dev, P0-calib-1 activo / 9977d9ffd7e5 y cero pendientes. Presets/selección posteriores intactos byte a byte. Queda satisfecha esta comprobación, sin repetir la campaña ni cambiar configuración. PR #17 lista para revisión Claude, sin fusionar; no equivale a aceptación de S1 y no autoriza S2–S4.

## Correcciones de revisión Claude autorizadas por el PO — 2026-10-25

El PO autoriza corregir PR #17 desde head 157d905ee925852b427d9e6567201289e1ed0879, en firstshot/s1-gesture, **sin fusionar**. Alcance: histéresis de flick conforme al paso 3 de Tarea 1.2, excepciones de carga/importación de presets con aviso visible y atomicidad conservada, y marcar histórica la entrada S1 de b37078f/330. TDD por aserciones, suites completas frescas y check-release dev/prod. No cambiar defaults/hash 00a501ad7066, rangos, criterios/umbrales, spec v0.3, plan, ProjectSettings ni física/SimulationKey. **Sin build nativa, instalación o sesión física nuevas.** Los exports de check-release son la verificación autorizada; no se compilan con Xcode ni se instalan.

Heads de código: **df65fc8** conserva el cruce en la banda y lo reinicia por debajo del límite inferior/Began; **e08fc7c** captura InvalidDataException de presets, muestra longitud/prefijo en importación y preserva estado/selección. RED→GREEN observado: mira 16 casos (13 Pass/3 fallos por aserción →16/16), presets 3 casos (0/3→3/3). El informe detalla las nuevas pruebas y sus límites.

La evidencia 311cb11 se conserva sin recalcular saltos de mira, marcando que usaba histéresis defectuosa. La build instalada c6c68c1 no contiene las correcciones de esta revisión. No se infiere aceptación de S1 del PO ni se inicia S2–S4/T8. Comprobar siempre el perfil y renovarlo antes de instalaciones desde el 27.

Verificación intermedia de las correcciones: EditMode completa 343/343; primera PlayMode 163/164 por una prueba previa de ventana vacía con origen fraccionario del reloj simulado. Ajuste **b5dd6ec**, solo preparación de GesturePreviewTests a un origen entero exacto; aserciones/runtime/criterio de 120 s intactos. Repetición completa PlayMode 164/164, sin errores CS. XML de la ejecución intermedia preservado, sin contarlo como RED de comportamiento.

Verificación final de estos heads: **343/343 EditMode y 164/164 PlayMode**, frescas/sin CS; **check-release dev/prod OK**, exports de 771bd8f limpio, sin framework de pruebas en Players y DevPanel solo dev. 201 archivos cotejados con las fuentes probadas; 31 archivos protegidos conservan sus bytes frente a 157d905, incluidos DefaultPreset.json, spec, planes, ProjectSettings y EffectiveConfig.cs. Hash 00a501ad7066/proyección 0d39e6016bd8/41 valores heredados/SimulationKey 166a96e319c7 intactos. Registros nativos originales preservados, con advertencia de histéresis defectuosa en 311cb11 y sin replay de mira. Entrega de correcciones en PR #17 lista para Claude, **sin fusionar ni aceptar S1**. Sin build nativa, instalación ni sesión física nuevas. Detenerse; S2–S4/T8 no autorizados.

## Aprobación de Claude y cierre técnico S1 autorizado por el PO — 2026-10-25

El PO comunica la aprobación de Claude de [PR #17](https://github.com/rafaallendes-boop/squash-clash/pull/17) en **5010d042a044f23d36b660cc97a37a89b080fc34**, incluidas las correcciones **df65fc8** (histéresis), **e08fc7c** (presets) y **b5dd6ec** (preparación del reloj de prueba) desde 157d905. Claude comprobó antes de aprobar que spec, planes, ProjectSettings y EffectiveConfig.cs no cambian frente a main; DefaultPreset.json solo incorpora las 17 claves de R2-3 y no cambia desde 157d905. Las verificaciones técnicas anteriores se conservan como evidencia, sin ejecutarlas de nuevo en este cierre.

El PO autoriza la fusión condicionada, el retiro del worktree preservando los artefactos y un único commit documental de cierre en main. Condiciones comprobadas y cumplidas:

- Head remoto de `firstshot/s1-gesture` exactamente **5010d042a044f23d36b660cc97a37a89b080fc34**; PR abierta, no borrador y fusionable sin conflictos. Main local/remota en 9a14f9b199c2b47a06fbd5625d930060d7289962; ambos árboles limpios. No se añadieron commits a la rama antes de fusionar.
- Fusión mediante **merge commit e26717bbcc19f195697d8c56e92fb7691cfcecdf**, sin squash ni rebase. Primer padre **9a14f9b199c2b47a06fbd5625d930060d7289962** y segundo padre exactamente **5010d042a044f23d36b660cc97a37a89b080fc34**. Árbol del merge idéntico al head aprobado.
- Main sincronizada por fast-forward y árbol limpio antes de redactar este registro. Worktree S1 limpio y eliminado sin `--force`; ramas remotas conservadas, sin cambiar visibilidad.
- Antes de retirarlo se trasladaron sus siete paths ignorados a **Logs/archive-s1-20261025**, preservando estructura, incluidos **Logs/evidence/s1-tdd**, **Logs/evidence/s1-claude-fixes**, builds, exports y XML; también caché y configuración local ignoradas. Inventario completo antes/después coincidente: **31.490 archivos / 8.223.583.327 bytes**, con inodos, tamaños, modos, tiempos y enlaces conservados; SHA-256 coincidentes para **485 archivos de Logs/XML**. Auditorías locales `archive-inventory.json` y `archive-verification.json` dentro del archivo ignorado; ningún artefacto ni dato privado se versiona.

**La fusión cierra la entrega técnica y no equivale a aceptación de S1 por el PO.** Las tres preguntas de control siguen sin respuesta separada: ¿apuntas donde quieres?, ¿entiendes la mira?, ¿la potencia distingue suave/fuerte? La app instalada **c6c68c1** es anterior a las correcciones de histéresis y presets. Los datos y saltos de mira medidos en **311cb11** se conservan con la advertencia de histéresis defectuosa; no se recalculan como medición nueva ni se realiza replay de mira.

Pendientes de una **futura validación física, no autorizada ahora**:

- Explicar antes la prueba, incluido que S1 no golpea la pelota.
- Poner la repetición temporal en **0** y restaurar **P0-calib-1** al final.
- Revisar los saltos de mira con la versión corregida.
- Experimento de cobertura mínima **20 frente a 30 ms** (decisión del PO); sin cambiar ahora parámetros ni criterios.

Defaults **00a501ad7066**, los **41 valores heredados** (proyección **0d39e6016bd8**) y **SimulationKey 166a96e319c7** permanecen intactos. El cierre solo modifica **AGENTS.md** y este registro; sin cambios adicionales de código, tests, defaults, spec o plan, ni nuevas ejecuciones de tests/check-release, builds, instalaciones o sesiones físicas. Comprobar siempre la vigencia del perfil y **renovarlo antes de cualquier instalación desde el 2026-10-27**; el perfil actual vence **2026-10-28T14:59:31Z**.

**Detenerse tras el registro documental. S2–S4 y T8 siguen sin autorizar.**

## Revalidación física S1 autorizada por el PO — 2026-10-25

Base **c5a7d1e83e9f339b043c38d5436e660142973cd2**, rama/worktree `firstshot/s1-revalidation`, solo documentación. Se autoriza check-release dev/prod y build dev limpia Release ARM64/no Development desde esa base, respaldo de P0-calib-1/Po-1/last-active antes de instalar y cotejo posterior byte a byte. Perfil efectivo comprobado antes de instalar; renovar primero si la instalación ocurre desde el 27.

Decisiones expresas del PO:

1. El experimento **power.minCoverageMs 20 frente a 30 ms se realiza en esta misma sesión**; orden **30→20**, con posible sesgo de aprendizaje/fatiga declarado. Dos series por configuración, 10 suaves y 10 fuertes, incluidos toques/cancelaciones; informar mínimo/mediana/máximo, salidas conservadoras por motivo y separación. Aplicar desde panel sin guardar y volver a 30 después. No promover un default sin decisión del PO.
2. Si el PO juzga inaceptables los saltos de mira al fijarse, registrar su juicio y proponer ajuste de lookback, umbral o ganancia para una **PR pequeña aparte con TDD y revisión de Claude antes de S2**. No se ajusta ninguno en esta sesión.
3. Bloque principal de **2 minutos con mano izquierda dominante**, desviación declarada frente a la derecha exigida en Tarea 1.4. Derecha opcional; no se presume realizada. Después, regla física de 50 mm.
4. Al terminar **no se inicia nada más**; el PO decide con el informe. S2–S4/T8 no autorizados.

Antes de los bloques: comprobar arranque con **P0-calib-1 / 9977d9ffd7e5 / cero pendientes**; poner `launcher.repeatSeconds=0` desde panel sin guardar, aplicar/relanzar, comprobarlo y cerrar DEV. Mostrar literalmente la explicación autorizada de S1 sin golpe; el PO debe identificar la mira y su cambio de estado antes de empezar. Registrar después las cinco respuestas por separado y literalmente, sin inferencias.

Cierre: recuperar evidencia nueva separada de 311cb11 (histéresis defectuosa), restaurar P0-calib-1 y comprobar en un arranque hash **9977d9ffd7e5**, cero pendientes y los tres archivos byte a byte iguales al respaldo. Si aparece un defecto, registrarlo y detenerse. Sin cambios de código, tests, defaults, spec, plan ni criterios. Informe/evidencia nuevos, alcance actualizado en AGENTS.md y PR documental lista para Claude sin fusionar. Esta autorización sustituye la pausa física consignada en el cierre anterior; **no es aceptación anticipada de S1**.

### Resultado: detención en la identificación y restauración — 2026-10-25

Preparación técnica sobre c5a7d1e limpia: **343/343 EditMode, 164/164 PlayMode y check-release dev/prod OK**; build dev Release ARM64/no Development, firma/perfil efectivos comprobados 18:57:48Z, vigencia hasta 2026-10-28T14:59:31Z. Presets/selección respaldados e idénticos tras instalar. Arranque 18:59:18.366698Z P0-calib-1 / 9977d9ffd7e5 / cero pendientes; repetición=0 preparada desde panel, aplicada/relanzada (19:00:36.578744Z / 00a501ad7066), cobertura=30, DEV cerrado y sin guardar preset.

Tras la explicación literal autorizada y una aclaración, el PO responde respectivamente: «no veo nada, solo veo omo va cambiando la mira» y «no veo ningun color , la pelota eso si la veo un poco roja, pero nada mas». No se confirma que identifique el círculo cian y su paso a rombo amarillo; **RV-S1-01: incidencia de visibilidad con causa técnica aún no demostrada**. Se detienen los bloques conforme a la instrucción de detenerse ante un defecto/incidencia. No se infiere juicio sobre saltos ni aceptación de S1.

Se conserva toda la evidencia incidental de c5a7d1e: **4 gestos/134 muestras, dos ventanas incompletas**, tres preview y un soft ordinal 1, ningún Frozen. Saltos 0 en Aiming no son mediciones al fijarse. Hay hashes temporales distintos y un nuevo arranque durante la familiarización; no se atribuyen claves concretas ni intención. No se completan 2 minutos, regla o series válidas; experimento **30→20 NotEvaluated**, sin proponer default nuevo. Las cinco preguntas de control quedan sin respuesta separada; los dos comentarios de familiarización no las sustituyen. El orden 30→20 y la mano izquierda principal siguen como decisiones autorizadas, no como bloques ejecutados.

**Restauración final verificada por nuevo arranque 2026-10-25T19:06:44.809232Z:** c5a7d1e dev, P0-calib-1 **9977d9ffd7e5/cero pendientes**, repetición=3/cobertura=30, tres archivos byte a byte iguales al respaldo. Reinicio mediante devicectl sin activar primer plano, recargando el preset persistido; sin guardar modificaciones. Defaults **00a501ad7066**, 41 valores heredados/proyección **0d39e6016bd8** y SimulationKey **166a96e319c7** intactos. Datos nuevos separados de 311cb11 con su advertencia de histéresis defectuosa; sin comparación causal válida ni replay nuevo.

[Informe de revalidación](2026-10-25-first-shot-s1-revalidation.md) y [evidencia nueva](../validation/evidence/2026-10-25-s1-revalidation.json) para Claude/PO. Propuesta documental: diagnosticar primero la visibilidad/posible oclusión y, si procede, corregir en una PR pequeña aparte con TDD y Claude **previa autorización**; no se implementa aquí ni se modifica lookback/umbral/ganancia. No se activa la condición de ajuste por saltos inaceptables, pues falta ese juicio. Solo documentación, PR lista sin fusionar. **Detenerse para decisión del PO; S2–S4/T8 no autorizados.**

## Cierre documental de revalidación y corrección de visibilidad autorizada — 2026-10-25

El PO autoriza fusionar PR #18 con merge commit, sincronizar main y retirar el worktree sin force preservando sus artefactos. Verificado head **4601342ffbdfe1614bb4c14182577602b86d77a4**, PR abierta/no borrador/sin conflictos y diff de los cuatro documentos entregados, sin cambios locales. Merge **a8240fe8277896015c37a66805d0d0c21c8a6b70**, padres c5a7d1e83e9f339b043c38d5436e660142973cd2 y head entregado. Árbol idéntico al head; main sincronizada/limpia. Worktree eliminado sin `--force`, siete paths ignorados preservados en **Logs/archive-s1-revalidation-20261025**, inventario coincidente: 31.296 archivos/8.214.783.234 bytes y 275 SHA-256 de Logs/XML. Ramas remotas conservadas. La fusión documental no acepta S1 ni completa el experimento 30→20.

**Causa confirmada comunicada por el PO:** captura de iPhone c5a7d1e dev con DEV cerrado; ShotOverlay, fondo alfa 0,86/textos/cuatro botones, tapa el frontis proyectado y la posición de mira (2,82,4,34) m. [Captura original suministrada](../validation/evidence/2026-10-25-po-iphone-overlay-occlusion.png), **990×2000**, SHA-256 **8e2ee2e5b1d134978f5acd548251a2eee9d4b21f72a977bad3f57fb54a7d9f85**, copiada byte a byte, sin EXIF/textos/fechas personales; perfil ICC de color conservado. Es una imagen reducida, no 1206×2622; las medidas nativas se harán en el editor a la resolución requerida. La estimación de Claude de unos 35 px de diámetro y 2–3 px de línea permanece estimación hasta medir. El objeto rojo que vio el PO **probablemente era la cápsula del jugador**, interpretación explícita.

Se autoriza **firstshot/s1-reticle-visibility**, desde main fusionada, con RED por aserciones/esqueletos compilables antes de implementar. Alcance:

- Main/cámara por defecto/viewport 1206×2622 y área segura iPhone 16 Pro: con DEV cerrado, ningún elemento de ShotOverlay se superpone al rectángulo completo del frontis proyectado, del suelo hasta out; mira Aiming en centro a 2,2 m con diámetro ≥60 px y línea ≥6 px. Conservar círculo/rombo y colores cian/amarillo/rojo.
- Vista mínima: una línea compacta debajo de la cancha, sobre DevBar y dentro del área segura, con Apuntando/Fijada/Inválida y P%; protocolo y detalle solo en DEV abierto, sin perder función. Aumentar tamaño con constantes de vista, sin claves nuevas; **DefaultPreset 00a501ad7066, registros/mediciones/escritura sin cambios**.
- **H3/H4 fuera de esta corrección:** en los cuatro gestos previos los picos calculados de las muestras originales fueron 143,057/241,833/227,880/62,936 mm/s, todos bajo flick=250; ninguno se fijó. Tres terminan en y=5 m. Evaluar después con la mira visible; sin tocar ganancia, flick, lookback, cámara ni ningún parámetro. No se presenta este cálculo descriptivo como medición nueva ni replay de mira.
- Suites completas/check-release dev/prod; capturas editor a 1206×2622 con DEV cerrado en Aiming/Frozen/inválido, mira visible/frontis despejado. Build dev Release ARM64/no Development; comprobar perfil efectivo, renovar primero si se instala desde el 27; respaldar P0-calib-1/Po-1/last-active, cotejar byte a byte y verificar arranque **9977d9ffd7e5/cero pendientes**.
- En iPhone **solo captura y confirmación PO de que ve la mira y su cambio de color**, sin revalidación de 2 min/regla/flicks/20–30 ms. Informe, PR lista para Claude sin fusionar y detenerse. **S1 sigue sin aceptar; S2–S4/T8 no autorizados.** Sin cambios de spec, plan, defaults ni criterios.
