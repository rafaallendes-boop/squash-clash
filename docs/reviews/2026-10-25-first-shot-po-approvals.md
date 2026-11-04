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

### Verificación técnica de la corrección visual — 2026-10-25

RED compilable previo a implementación **1bf5f8b**: 5 casos, 1 Pass/4 fallos por aserción. Implementación mínima **2850a56**; repetición separada con vistas antiguas y fixture nativo final confirma 1/5, después GREEN 5/5. Fuentes finales **5e218ead0636d4eab23f4913b8f4911c4c117dd5**, limpias: suites frescas **343/343 EditMode y 169/169 PlayMode**, sin CS; **check-release dev/prod OK**. Capturas editor 1206×2622 en Aiming/Frozen/inválido y medición geométrica: diámetro menor **65,44067 px**, línea **6,66443 px**, frente a **28,04248/2,08264 px** de las vistas anteriores; mínimos autorizados 60/6 cumplidos. DEV cerrado deja despejado el rectángulo completo del frontis. El área segura de la prueba es un fixture explícito, no una medición física; runtime consulta Screen.safeArea.

Build dev limpia **5e218ea**, Release ARM64/no Development, firmada e instalada el 25. Perfil efectivo cotejado antes de instalar a **21:18:56Z**, compatible y vigente hasta **2026-10-28T14:59:31Z**; renovar primero antes de instalaciones desde el 27 y verificar vigencia siempre. **625 archivos** cotejados con las fuentes probadas. El primer lanzamiento fue rechazado por iPhone bloqueado; el PO abre la app y dice «app abierta y muestra lo solicitado». Registro de arranque observado **21:21:59.8644060Z**, **P0-calib-1 / 9977d9ffd7e5 / cero pendientes**. P0-calib-1, Po-1 y last-active idénticos byte a byte al respaldo antes/después de instalación y apertura; sin aplicar ni guardar parámetros. Defaults **00a501ad7066**, proyección heredada **0d39e6016bd8**/41 valores y SimulationKey **166a96e319c7** intactos; spec/planes/ProjectSettings/Packages/scripts/cámara/física sin cambios.

[Informe](2026-10-25-first-shot-s1-reticle-visibility.md) y [evidencia](../validation/evidence/2026-10-25-s1-reticle-visibility.json). La PR #19 se abrió inicialmente en borrador pendiente de la comprobación visual del PO; ese estado queda sustituido por la entrega siguiente.

### Confirmación visual del PO y entrega para Claude — 2026-10-25

Tras abrir **5e218ea dev**, el PO confirma literalmente:

> si, si voy lento, toma el circulo azul y después el rojo si voy fuera de la cancha, y si voy rapido el rombo amarillo. LAs capturas están en el repo, las dejé desordenadas por lo que puedes ordenarals después

Se incorporan tres capturas suministradas con DEV cerrado: [círculo cian](../validation/evidence/2026-10-25-po-iphone-reticle-aiming-cyan.jpg), [círculo rojo](../validation/evidence/2026-10-25-po-iphone-reticle-invalid-red.jpg) y [rombo amarillo](../validation/evidence/2026-10-25-po-iphone-reticle-frozen-yellow.jpg). Frontis despejado y línea compacta visibles. Dimensiones reales 1193×2378, 1206×2332 y 1206×1924 respectivamente; no se usan para mediciones nativas. La amarilla es un recorte sin cabecera/build. Se eliminan metadatos APP1/APP13, sin recomprimir ni cambiar datos de imagen/ICC; hashes antes/después en evidencia. Originales preservados y ordenados en `Logs/evidence/s1-reticle-visibility/` del checkout principal; ninguna captura suelta queda en la raíz o scripts.

**Comprobación breve de visibilidad/color completada**, sin inferir aceptación de S1, juicio de precisión/potencia/saltos ni respuestas a las cinco preguntas de control. No se ejecutan los bloques de revalidación ni cobertura 20/30 ms. La recuperación posterior confirma P0-calib-1 **9977d9ffd7e5/cero pendientes**, último registro de lanzamiento **21:29:25.8202720Z**; P0-calib-1/Po-1/last-active continúan byte a byte iguales al respaldo.

Entrega en [PR #19 lista para Claude](https://github.com/rafaallendes-boop/squash-clash/pull/19), **sin fusionar**. Implementación 2850a56 y build probada/instalada 5e218ea; cierre posterior solo documental/evidencia, sin repetir suites, build o instalación. Se conservan 343/343 EditMode, 169/169 PlayMode y check-release dev/prod OK de la fuente probada. Spec/plan/defaults/criterios y parámetros intactos. **S1 sin aceptar; H3/H4, revalidación y experimento de cobertura pendientes. Detenerse; S2–S4/T8 no autorizados.** Comprobar siempre perfil antes de instalar y renovar primero desde el 27.

## Cierre de PR #19 y segunda revalidación autorizados por el PO — 2026-10-25

El PO comunica aprobación de Claude en **9d961743e6598b951dd9892f2aa21d2ed1417ee0** y autoriza fusión con merge commit condicionada a head exacto y ausencia de conflictos. Head local/remoto exacto, PR abierta/no borrador/fusionable y árboles limpios comprobados. **Merge f7005be63218a573dd765c66b8b1e78c4d9f0213**, padres a8240fe8277896015c37a66805d0d0c21c8a6b70 y head aprobado; árbol idéntico al aprobado. Main sincronizada por fast-forward y limpia. Worktree de visibilidad retirado sin `--force`; siete paths ignorados trasladados a **Logs/archive-s1-reticle-visibility-20261025** del checkout principal, incluidos Logs/evidence, XML, builds/exports y caché. Inventario antes/después idéntico (inodos/modos/tamaños/mtime/enlaces): **31.292 archivos / 8.218.064.359 bytes**, **260 hashes** de Logs/XML coincidentes. Ramas remotas y visibilidad conservadas.

Observaciones de Claude para **S2**, registradas sin cambio de código ni ampliación de esta autorización:

1. La mira visual de **0,84 m** es más grande que la zona que clasifica tin/out.
2. La línea compacta puede tapar la pelota en la zona trasera.
3. La prueba visual usa piezas internas del editor (**GameViewSizes**) y es frágil ante actualizaciones de Unity.

El PO autoriza `firstshot/s1-revalidation-2`, **solo documental**, desde main tras el merge. **Sin build ni instalación nuevas:** usar la app instalada **5e218ea dev**. Antes de preparar la sesión se comprobaron **399 archivos versionados de Assets/ProjectSettings/Packages**: diff Git vacío entre 5e218ead0636d4eab23f4913b8f4911c4c117dd5 y main f7005be, y SHA-256 locales idénticos al manifiesto de fuentes probadas de la build instalada. La identidad es de fuentes versionadas; los metadatos generados BuildInfo se conservan en los artefactos originales, sin regenerarlos. Manifiesto privado `Logs/evidence/s1-revalidation-2/product-source-equivalence.json`. No se repiten suites ni exports en esta sesión física documental.

Decisiones del PO que siguen vigentes: experimento **30→20 ms en la misma sesión**, posible sesgo de aprendizaje/fatiga declarado, volver a 30 y no promover default; bloque principal **120 s con izquierda dominante**, desviación declarada del plan (derecha opcional); si saltos inaceptables, propuesta para PR pequeña aparte antes de S2 sin implementarla; el PO decide después con el informe.

Preparación autorizada: respaldo de P0-calib-1/Po-1/last-active; arranque P0-calib-1 **9977d9ffd7e5/cero pendientes**; `launcher.repeatSeconds=0` desde panel, aplicar/relanzar sin guardar y verificar, DEV cerrado. Registrar cada lanzamiento/hash; si cambia configuración controlada, gestos afectados **no comparables**. Recordatorio antes de bloques:

> La pelota no se golpea en S1. Arrastra despacio para mover la mira; para fijarla, haz un gesto rápido y corto, como un golpe. Abajo verás el estado (Apuntando, Fijada o Inválida) y P, la potencia.

Orden: 120 s izquierda, regla física 50 mm, diez suaves/diez fuertes a 30 ms, diez suaves/diez fuertes a 20 ms aplicado sin guardar y comprobado antes. Botones dentro de DEV, que se cierra automáticamente al armarlos. Registrar todos los intentos, incluidos toques y cancelaciones. Informe por serie con potencia mínima/mediana/máxima, conservadores por motivo, separación y porcentaje Frozen; saltos al fijarse p50/p95/máximo; H3/H4 descriptivos (pico de velocidad frente a 250 mm/s, proporción en tope y=5 m o fuera), **sin ajustes**. Cinco preguntas del PO separadas/literales, sin inferencias.

Cierre: recuperar evidencia nueva separada de 311cb11 (histéresis defectuosa), c5a7d1e (mira tapada) y la comprobación breve de visibilidad anterior; restaurar P0-calib-1 **9977d9ffd7e5/cero pendientes** y tres archivos byte a byte al respaldo. Si aparece defecto, registrar y detenerse. Informe/JSON nuevos, AGENTS actualizado, PR documental lista para Claude **sin fusionar**. Sin código/defaults/spec/plan/criterios; **S1 sigue sin aceptar y S2–S4/T8 no autorizados. Detenerse al entregar.** Comprobar perfil antes de futuras instalaciones y renovar primero desde el 27; no hay instalación en esta autorización.


### Resultado final de segunda revalidación — 2026-10-25

Sesión sobre **5e218ea dev instalada**, sin build/instalación/suites/exports nuevos. Una adquisición **complete=true / 120,014203167 s / 2449 muestras**, 50,611337 Hz entregados dentro de toques (no frecuencia hardware); otra **incompleta / 12,549327917 s / system-cancel**, separada sin atribuir causa. Se conservan **138 gestos / 4683 muestras**, incluidos un Tap y 137 Swing: 92 en adquisiciones, 6 de distancia y 40 de las cuatro series. **Los mm físicos quedan NotEvaluated: el PO declara que no tiene regla.** No se cambia DpiSource a PhysicallyVerified ni se interpreta la etiqueta automática50 como recorrido real.

Cuatro series de10, sin descartar conservadores/P=0. Mediana de fuertes **1,977 % a30 / 7,123 % a20**, máximos20,523/18,778 %, suaves todosP=0; Frozen **5/10 a30 y9/10 a20**. Saltos de los54 Frozen de toda la sesión: p50 **1,570968 m**, p95 **2,166354 m**, máximo **2,667281 m**, cifras originales sin suavizado/replay. H3/H4 por gesto/grupo en informe/JSON, sin ajustes. Ninguno de los138 gestos queda excluido por configuración: hashes **00a501ad7066 (30 ms)** y **a49c48e3d019 (20 ms)**, cero pendientes. Cada lanzamiento conservado. La entrada remota dejó transitoriamente10 durante preparación, corregido antes de relanzar; ningún lanzamiento10 ni nuevos gestos en ese intervalo. Se declara, sin diagnosticar defecto del producto a partir del control remoto.

El PO responde **por separado y literalmente**:

| Pregunta | Respuesta literal |
|---|---|
| ¿Apuntas donde quieres? | si |
| ¿Entiendes siempre dónde apuntas? | si, pero para por ejemplo un drop tuve que intuir que era hacia abajo, pero igual es parte de la mecnaica |
| ¿El número responde a suave/fuerte? | si |
| ¿Los saltos de la mira al fijarse son aceptables? | si |
| ¿Notaste diferencia entre 30 y 20 ms? | si |

Mano de las series, literal:

> mano izquierda y derecha, ambas 2, me imagine juando y use ambas manos

Se registra uso de **ambas manos, sin desglose por condición/intento**. El bloque120 s fue solicitado con izquierda dominante (desviación autorizada del plan), con respuesta «listo»; no se presume una campaña diestra adicional. Orden30→20, aprendizaje/fatiga, manos mezcladas y gestos distintos limitan la comparación; no se atribuye causalidad a cobertura ni a ella el cambio de Frozen. La respuesta sobre diferencia no expresa preferencia ni decisión de default: **permanece30 ms**. Respuesta sobre saltos «si»: **no se activa la propuesta condicional de ajuste** de lookback/umbral/ganancia. La observación del drop queda como comprensión de la mecánica; S1 no golpea la pelota.

**Restauración final comprobada:** reinicio de la misma app para recargar P0 persistido (coverage30/repeat3 juntos, sin lanzamiento intermedio30/repeat0 ni guardar). Primer intento rechazado por iPhone bloqueado/CoreDevice10002; tras autenticación local completada por PO, reinicio correcto. Nuevo arranque **2026-10-25T23:24:50.6920160Z / 5e218ea dev / P0-calib-1 / 9977d9ffd7e5 / cero pendientes**, último registro **2026-10-25T23:25:43.7507310Z**. Comparación directa de bytes: **P0-calib-1, Po-1 y last-active idénticos al respaldo**. No se añadieron gestos durante el cierre.

[Informe nuevo](2026-10-25-first-shot-s1-revalidation-2.md) y [evidencia nueva](../validation/evidence/2026-10-25-s1-revalidation-2.json), separados de311cb11/histéresis defectuosa, c5a7d1e/mira tapada y visibilidad previa. Diff solo cuatro documentos, producto/spec/planes/criterios intactos. Defaults **00a501ad7066**, 41 valores/proyección **0d39e6016bd8** y SimulationKey **166a96e319c7** intactos. Las tres observaciones Claude para S2 del cierre de PR #19 siguen registradas, sin código.

Entrega en [PR #20 lista para Claude](https://github.com/rafaallendes-boop/squash-clash/pull/20), **sin fusionar**, fuente documental de cierre **e9fcb39fd893da38725d3c83bb50d650fe834c33** (el commit posterior solo enlaza la PR). **No equivale a aceptación de S1; el PO decide con el informe. S2–S4/T8 siguen sin autorizar. Detenerse.** Comprobar siempre perfil antes de instalar y renovar primero desde el27; no hubo instalación en esta sesión.


## Corrección de PR #20 y propuesta de calibración autorizadas — 2026-10-25

El PO comunica aprobación de Claude de PR #20 en **16be7977c258b8ee0ff3f5d2a1a3c3e9dfbfd33e**, con corrección documental autorizada antes de fusionar. Los40 gestos de serie tienen ventana80 ms completa y **cero short-coverage**; las salidas conservadoras son13 few-samples/minSamples4 y7 sample-gap/maxGap34, parámetros estables. **Experimento20/30 NotInformative para cobertura**; la diferencia percibida se atribuye a fuertes más rápidos en la tanda20, conservando limitaciones de orden/manos y las cinco respuestas literales. Permanece30 ms. Corrección solo en informe, JSON, este registro y AGENTS; verificar ese delta desde el head aprobado antes del merge commit.

Tras fusionar, sincronizar main y retirar worktree sin force, preservando los artefactos ignorados en **Logs/archive-s1-revalidation-2-20261025**. A continuación se autoriza rama **firstshot/s1-power-calibration-proposal**, solo propuesta documental: candidatos de full/deadZone/minSamples/maxGap y flick si procede, justificados con los40 gestos existentes; P contrafactual por gesto, etiquetada como cálculo offline, sin nueva medición. Protocolo corto de futuro preset de prueba (sin promover default), repeat0 sin guardar, diez suaves/diez fuertes con una sola mano declarada, juicios literales y restauración/cotejo habitual. No crear ni cargar preset real, código, defaults, build o sesión. PR documental lista para Claude sin fusionar; **S1 sin aceptar, S2–S4/T8 sin autorizar**. Perfil: verificar antes de cualquier futura instalación y renovar primero desde el27. Detenerse al entregar.


## Cierre de PR #20 y propuesta documental de calibración — 2026-10-25

PR #20 fusionada por autorización del PO con **merge commit 16b3a8e46a5f8961f48682f92516271ebbd09a02**, segundo padre **d58d6e335b78d308f8734c19060cebac54ab812f** (corrección de revisión sobre 16be7977c258b8ee0ff3f5d2a1a3c3e9dfbfd33e). Antes de fusionar: head remoto/local exacto, PR abierta/no borrador/fusionable y árbol limpio; delta desde 16be797 exactamente **AGENTS.md, informe, JSON y este registro**, sin alterar trazas ni respuestas del PO. Main sincronizada por fast-forward, árbol del merge idéntico al head corregido.

Worktree de revalidación 2 retirado **sin --force**. Sus dos paths ignorados (Logs y scripts/device.env) preservados en **Logs/archive-s1-revalidation-2-20261025** del checkout principal: **439 archivos /35.277.279 bytes**, SHA-256 de los 439 e inventario completo antes/después idénticos (inodos/modos/tamaños/mtime/enlaces). Ramas remotas conservadas, visibilidad sin cambios. La fusión no acepta S1.

El PO resolvió la discrepancia de recuento suave del encargo con la respuesta literal **«Usar 16/4 del JSON»**. Se documentan 16 conservadores (12 few-samples, 4 sample-gap) y 4 medidos; no 15/5. El experimento20/30 queda **NotInformative para cobertura**; las respuestas literales permanecen intactas y se mantiene 30.

Propuesta en rama **firstshot/s1-power-calibration-proposal**, desde el merge, **solo documentación y cálculo offline de las 40 trazas existentes**. [Documento](2026-10-25-first-shot-s1-power-calibration-proposal.md): candidatos de Codex **full500/deadZone0/minSamples3/maxGap51**, manteniendo flick250; **no son valores aprobados por el PO**, no se crea ni aplica un preset real. Tabla de P contrafactual para los 40, fórmula/filtros/fuente identificados y validación aritmética contra los 20 casos nativos measured. Sin medición nueva ni replay de AimModel. El preset no corrige la parada antes de soltar: quedarían 14/20 suaves a P=0; riesgos de ruido, interpolación y potencia accidental explícitos. Fijación original 14/20 fuertes y 0/20 suaves, sin cambio de umbral.

Protocolo de futura sesión corta, **pendiente de autorización**: repeat=0 temporal sin guardar, diez suaves/diez fuertes con una sola mano declarada, todos los intentos y juicio literal, restauración de P0-calib-1 **9977d9ffd7e5/cero pendientes/tres archivos idénticos**. Esta entrega no inicia esa sesión ni hace código, preset real, defaults, build o instalación. Defaults **00a501ad7066**, 41 valores heredados/proyección **0d39e6016bd8**, SimulationKey **166a96e319c7**, spec/plan/criterios intactos. PR documental lista para Claude **sin fusionar**; **S1 sigue sin aceptar y S2–S4/T8 sin autorizar. Detenerse.** Comprobar perfil antes de cualquier instalación futura y renovar primero desde el 27.

Entrega en [PR #21 lista para Claude](https://github.com/rafaallendes-boop/squash-clash/pull/21), abierta y sin borrador, **sin fusionar**. Fuente documental y numérica **480598400866ee36599818084b198f831dd04a70**; el commit posterior añade el enlace de entrega. Cotejadas las 40 filas, 20 resultados nativos reproducidos y SHA-256 de la fuente intacto; solo tres Markdown. No se han ejecutado suites ni check-release porque esta entrega no cambia producto y no autoriza pruebas nuevas. Detenerse.


## PR #21 cerrada y calibración del gesto autorizada — 2026-10-25

Claude aprobó head **2d5ea8710f63a88dd663ac52c84c8f7fc43de173**; comprobados head exacto, PR abierta/no borrador/sin conflictos y delta de tres Markdown. Fusión con merge commit **e11cc3aaa9dad7112d886e43d990732e0e198b43**, segundo padre exacto; main sincronizada y limpia. Worktree retirado sin --force; cinco archivos ignorados (51.926 bytes, inventario y SHA-256 idénticos) preservados en **Logs/archive-s1-power-calibration-proposal-20261025**. Ramas remotas y visibilidad conservadas.

S1-Power-Calib-A **500/0/3/51, flick 250** pasa a candidato A. Su protocolo anterior queda sustituido por la autorización actual de calibración del gesto. Claude corrige su recuento de suaves: **16/4, no 15/5**.

El PO autoriza comparación offline previa y después implementación **solo del método elegido** con TDD, regresión, check-release y build dev Release ARM64/no Development, perfil vigente y respaldo/cotejo. Escala fija y generosa, ajustable desde el panel, sin calibración individual. Comparar A temporal y B pico suavizado/decaimiento con los mismos gestos; preferir A si basta. Mantener flick anticipado y añadir fijación en liberación cuando no hubo flick, incluso inválida roja; todo provisional para observar de nuevo en S2 sin autorizarlo. Timing ±40 ms, ventana válida experimental, comparación con T3 puro y feedback se conservan para S2/S3. Esto es una **desviación autorizada de Tarea 1.2 del plan r2**, registrada aquí sin editar el plan ni la spec.

Defaults existentes intactos; solo claves nuevas con comportamiento anterior (A y fijación al soltar apagada), hash nuevo y extensión P0 documentados si se implementa. Preset experimental separado S1-calib-gesture, sin sobrescribir P0/Po-1. Futura sesión de esta tarea, si procede tras comparación: una mano declarada, diez suaves/medios/fuertes/apuntar rápido y soltar suave, cinco juicios separados, restauración/cotejo. Ante defecto detenerse; sin ajustes sobre la marcha. Si la comparación no permite elegir con seguridad, informar antes de implementar. S1 sigue sin aceptar, S2–S4/T8 no autorizados. Perfil siempre comprobado antes de instalar y renovado primero desde el 27.


Estado técnico previo a build de calibración: criterio fijado en 23f63e7, comparación en 04c718a. A no cumple rango/rechazos; B seleccionado **provisionalmente 400/0, suavizado 20/decaimiento 120 ms y soporte 3/30/51**, una de cuatro combinaciones de las 27 predeclaradas que cumplen los requisitos observables. No hay medios ni intención etiquetada del caso de riesgo: seguridad NotEvaluated, cuatro sustitutos cinemáticos bajos; no se infiere un Pass. Esta elección no modifica la autorización ni acepta S1. Implementación TDD de 71919fb/fe5a92b/39c3686/20271ff; regresión **359/359 EditMode y 172/172 PlayMode** sin CS. Cuatro claves nuevas compatibles (power.method=0, power.peakSmoothingMs=20, power.peakDecayMs=120, aim.freezeOnRelease=0), defaults **6095c0b38614**, P0 extendido **fdd997f2b85a**; los 58 valores existentes y proyección 00a501ad7066 intactos, 41 heredados/proyección 0d39e6016bd8 y SimulationKey166a96e319c7 intactas. Preset experimental separado, hash 6071903f8128, pendiente de carga temporal y prueba física. Protocolo de esta tarea sustituye al de PR #21. No promover defaults ni reajustar durante la sesión.


### Calibración detenida por GC-S1-01, sin instalar — 2026-10-25

La elección provisional B 400/0/20/120 queda **bloqueada**, no aprobada: al ampliar el caso obligatorio de apuntar rápido y soltar suave, dos perfiles sintéticos dentro del rango de picos registrado producen **60,472 % y 68,060 %** después de 150 ms sin movimiento; ambos debían quedar fuera de banda fuerte. Focal fresco **8/10 Pass, 2 Fail por aserción**, sin CS; producto no ajustado después. Las suites completas previas 359/359 y 172/172 no certifican verde el head con esta ampliación. Informe y JSON nuevos documentan perfiles y límites; no son accidentes observados del PO ni medición nueva.

Se cumple «ante un defecto, detenerse»: **sin xcodebuild, instalación, preset cargado ni fase C**, sin sustituir otro candidato para hacer pasar el resultado. check-release comprobó módulos dev/prod, pero los primeros exports tenían gitDirty=true por cambios transitorios de pruebas; restaurados ProjectSettings a HEAD y evidencias preservadas. No build limpia instalable declarada. Mano elegida literalmente «Izquierda, mi dominante»; cuatro series y cinco preguntas NotEvaluated/sin respuesta. P0/Po-1/last-active cotejados de nuevo byte a byte idénticos, selección P0 y app instalada **5e218ea / 9977d9ffd7e5 / cero pendientes**; hashes nuevos de esta rama no observados en iPhone. La implementación queda para revisión de Claude del bloqueo, no para aprobación ni fusión. Cualquier cambio de algoritmo requiere decisión del PO; no se autoriza desde este registro. **S1 sin aceptar; S2–S4/T8 sin autorizar. Detenerse.** Perfil verificar siempre antes de instalar y renovar primero desde el 27.

Entrega del bloqueo en [PR #22](https://github.com/rafaallendes-boop/squash-clash/pull/22), abierta/no borrador **sin fusionar**. Fuente de cierre **1eee39ef7d0de63c2745fe2cc06dd185bc02cfe1**; el commit posterior solo enlaza la entrega. Tests de reproducción en 2e94ce2; no apta para aprobación ni instalación, pendiente decisión del PO tras revisión de Claude. No continuar automáticamente.


## Resolución de GC-S1-01 autorizada por el PO — 2026-10-26

Claude revisó PR #22 head **41e9abfa3db874249034ac17140fe24366820e0c** y confirmó los tres casos sintéticos 28,649/60,472/68,060 %. El PO corrige la regla de selección: riesgo de arrastre rápido, **pausa inmóvil de 150 ms y P<0,60** es eliminatorio; mismos casos y banda, sin cambios. Probar las cuatro combinaciones predeclaradas 300/0/60, 400/0/120, 400/0/180 y 400/10/180; entre supervivientes elegir menor saturación y después menor memoria. Si ninguna pasa, detenerse sin salir del grid.

Confirmación con pruebas C# reales: solo **300/0/60** supera los tres casos (10,944/23,101/25,999 %). El resto falla; tabla completa en informe/evidencia nueva. Por quedar un único superviviente no hay empate que resolver. **Tau 20 y soporte 3/30/51 se conservan**. Preset experimental S1-calib-gesture actualizado, hash **08b6fe90889b**. Defaults **6095c0b38614**, P0 extendido **fdd997f2b85a**, 58 valores anteriores, 41 heredados/proyección 0d39e6016bd8 y SimulationKey166a96e319c7 intactos. No nuevas claves ni cambio de defaults desde el head revisado.

Costo aceptado expresamente por el PO: flick de 500 mm/s detenido 100 ms antes de soltar cae aproximadamente de 54 a 31 %; prueba real sintética 53,959/31,267 %. Claude informa 18/20 fuertes soltando a 54 ms o menos del pico y gestos de apuntado preview esperando 333 ms o más; se registra como fundamento comunicado de la decisión, sin nueva medición. Compromiso provisional para volver a observar en S2 cuando se autorice, **sin autorizar S2 ahora**.

Nueva prueba de flick fuerte detenido 25/50 ms ya GREEN (100/71,945 %); se declara GREEN directo, sin inventar un RED. Nueva prueba AimModel Ended con timestamp igual al anterior produjo RED válido por aserción; corregida fijación por Release sin dividir por dt cero. Defaults mantienen fijación al soltar apagada. Focal conjunto **23/23** (diez anteriores con mismos casos/pausas/aserciones, solo configuración del estimador al candidato elegido; nuevas pruebas y Aim). Pendientes regresión completa, exportaciones limpias gitDirty=false, build dev Release ARM64/no Development y perfil vigente antes de instalar; si ocurre desde el 27 renovarlo primero.

Fase C continúa autorizada con mano ya declarada **«Izquierda, mi dominante»**, cuatro series de diez: suaves, medios, fuertes y apuntar rápido con suelta suave, todos los intentos. Antes de la cuarta avisar literalmente: **«Si apuntas muy rápido, la mira se fijará en ese momento (rombo amarillo); es el comportamiento actual. Lo que evaluamos es la potencia al soltar.»** Cinco preguntas separadas y literales, restauración P0 fdd997f2b85a/cero pendientes/tres archivos idénticos. No ajustes durante sesión; ante otro defecto detenerse. Mantener historia del bloqueo, informe y evidencia; PR #22 para Claude sin fusionar. Spec/plan/criterios/bandas/umbrales intactos, **S1 sin aceptar y S2–S4/T8 sin autorizar**.

Regresión de resolución, producto8c0a682: **371/371 EditMode, 172/172 PlayMode**, XML frescos y sin CS. Transitorios de ProjectSettings archivados/restaurados antes de exportar. Los defaults de 41e9abf siguen byte a byte intactos. Respaldo nuevo pendiente tras error de conexión; no instalar sin recuperarlo.

Build8476dd8 dev instalada el26: Release ARM64/no Development, firma y perfil efectivo comprobados válidos hasta2026-10-28T14:59:31Z; exports dev/prod gitDirty=false. Tres archivos idénticos byte a byte tras instalar. App nueva aún no arrancada por bloqueo del teléfono; autenticación local de Duplicación solicitada al PO. Preset separado copiado, todavía sin cargar; selección P0 intacta. FaseC pendiente, no inferir datos ni juicios.

Preparación física verificada tras autenticación: arranque8476dd8/P0fdd997f2b85a/cero pendientes. Sesión temporal08b6fe90889b/cero pendientes, lanzamiento2026-10-27T00:50:35.4162550Z (26 local); last-active devuelto a P0 antes de medir y tres archivos idénticos byte a byte. S1 solo en memoria, sin guardar sobre P0/Po-1, DEV cerrado. Recordatorio literal mostrado y solicitado solo10 suaves izquierda. No hay resultados físicos nuevos todavía; los demás bloques y aviso previo a cuarta siguen pendientes.

## Entrega de resolución y fase C incompleta — 2026-10-27

Se ejecutó la decisión del26: candidato único300/0/60, preset08b6fe90889b; producto **8c0a682**, pruebas nuevas con RED/GREEN descritos en informe, regresión fresca **371/371 EditMode y172/172 PlayMode**, exports dev/prod limpios y build instalada **8476dd8 dev**. No se atribuye a Claude aprobación de estos cambios: su revisión comunicada fue del bloqueo en41e9abf; la entrega actual queda **pendiente de revisión de Claude en PR #22**, abierta/no borrador y sin fusionar.

Fase física26–27 con mano declarada «Izquierda, mi dominante»:30 intentos comparables con B (10 suaves/10 medios/10 fuertes), y19 del cuarto bloque **no comparables** con B: todosP0fdd997f2b85a. Se declara que el PO adelantó medios/fuertes tras solicitar solo suaves. Aviso literal mostrado antes de cuarta; PO repitió por interrupción al salirse el teléfono, sin excluir la primera secuencia de9 ni sus cancelaciones. No dos series completas por inferencia. El nuevo lanzamientoP0 se observó11:53:22Z del27; causa exacta no determinada. La configuración temporal se había perdido respecto a la noche anterior. **No repetir ni ajustar sin otra decisión del PO.**

Resultados B:10/10 suaves,3/10 medios y8/10 fuertes en sus bandas; tres fuertes saturados de10 (30 %, por encima de advertencia20 %). Siete medios numéricamente fuertes no equivalen por inferencia a siete accidentes. Riesgo físico de apuntar rápido/soltar suave para B **NotEvaluated**; los19 del cuarto bloque son12 Swing/7 SystemCanceled conP0, no validación del candidato. [Informe](2026-10-25-first-shot-s1-gesture-calibration.md) y [evidencia física separada](../validation/evidence/2026-10-27-s1-gesture-calibration-physical.json), conservando historia y evidencia de GC-S1-01.

Cinco preguntas y respuestas literales del PO:

| Pregunta | Respuesta |
|---|---|
| ¿Consigues las tres intensidades de forma voluntaria? | «No, la de golpear rápido me cuesta lograrlo ya que debo apuntar lento y acelerar» |
| ¿Apareció algún golpe fuerte que no buscabas? | «cuando lo logro, sale bien» |
| ¿Apareció algún golpe fuerte al apuntar rápido y soltar suave? | «cuando lo hacía el rombo amarillo uedaba al medio» |
| ¿La mira queda donde quieres apuntar? | «si» |
| ¿Se siente consistente entre intentos? | «habría que mejorar como hacer el golpe rapido» |

No convertir las respuestas segunda/tercera en una afirmación o negación de potencia accidental; la tercera describe posición de la mira y el cuarto bloque usóP0. No inferir consistencia ni aceptación. La dificultad de apuntar primero y acelerar queda registrada sin cambio de mecánica. El PO decide después del informe y la revisión; **S1 sigue sin aceptar**.

Restauración final mediante reinicio de app existente y continuación de foco, sin build ni instalación el27: **2026-10-27T11:59:47.4715270Z /8476dd8 dev /P0-calib-1 /fdd997f2b85a /cero pendientes**, corroborado en lanzamientos posteriores yUI. **P0-calib-1.json, Po-1.json y last-active.txt directamente byte a byte idénticos al respaldo previo a instalar**; DEV cerrado,49 intentos intactos y ninguna serie adicional. Defaults6095c0b38614 sin cambio desde41e9abf;58 anteriores/proyección00a501ad7066,41 heredados/proyección0d39e6016bd8, SimulationKey166a96e319c7, spec/planes/criterios/bandas/umbrales intactos. No código después de las pruebas/build indicadas. Perfil comprobado vigente antes de instalar el26, vence 2026-10-28T14:59:31Z; comprobar vigencia efectiva siempre y renovar antes de cualquier instalación desde el27.

**Entregar PR #22 lista para Claude sin fusionar y detenerse.** No promover preset/defaults, no aceptarS1 ni iniciarS2–S4/T8. La fase física queda incompleta y no se autoriza desde este registro una repetición ni un ajuste.

## Cierre técnico experimental de PR #22 y siguiente propuesta — PO, 2026-10-27

El PO comunica aprobación de Claude del head **7bfea922f49fc094fa59601d4e36d7a8e87025f6** como **entrega técnica experimental**, y autoriza fusión condicionada. Se comprobaron head remoto/local exactamente iguales, sin commits nuevos, PR abierta/no borrador/sin conflictos y ambos árboles limpios. [PR #22](https://github.com/rafaallendes-boop/squash-clash/pull/22) fusionada con **merge commit 31bbe03ecc6d2772c713620a6332d5b84ddb0e75**; segundo padre exactamente el head aprobado. Árbol del merge idéntico a ese head. Main sincronizada con origin/main y limpia.

Worktree de calibración retirado **sin --force**. Antes se preservaron siete paths ignorados en **Logs/archive-s1-gesture-calibration-20261027/artifacts**: BuildInfo.json y su meta, Builds,Library,Logs,UserSettings y scripts/device.env. Inventarios antes/después y todos los SHA-256 cotejados: **34.533 entradas,31.694 archivos,8.315.063.114 bytes**, coincidencia completa. Resumen e inventarios privados bajo el directorio de archivo; no se versionan credenciales, logs, cachés ni builds. Ramas remotas conservadas. No se volvieron a ejecutar suites, check-release, builds o instalaciones para cerrar; los resultados371/371 y 172/172 corresponden a la entrega anterior.

**Límite de la aprobación:** método B y aim.freezeOnRelease desactivados por defecto; preset 08b6fe90889b sin promover; defaults 6095c0b38614 intactos. La calibración **no** queda validada: riesgo físico de B en apuntar rápido/soltar suave **NotEvaluated** por los 19 intentos con P0, saturación fuerte 30 % frente a advertencia 20 %, medios 3/10 en banda y respuesta literal «No» a tres intensidades voluntarias. No aceptación de S1 ni del candidato 300/0/60. Las respuestas completas y 49 intentos se conservan en la evidencia original, sin reinterpretar segunda/tercera pregunta como sí/no.

### Observaciones de Claude comunicadas por el PO, sin código

- **dt=0 en potencia:** el arreglo de Ended repetido solo cubre AimModel. GesturePreview marca repeated-time/P=0 conservadora y PeakPowerEstimator también lo hace con B en su ruta directa: la mira puede fijarse y anularse una potencia fuerte. No apareció en los 49 gestos. Corregir con TDD integrado/estimador solo cuando se autorice una PR de código; no se ejecutó ni añadió una prueba aquí.
- **Protocolo:** el cuarto bloque empezó 12,006 s después del lanzamiento P0, sin comprobar el hash. Las futuras sesiones deben comprobar hash efectivo antes de **cada bloque**, además de preset, cero pendientes y repetición=0; si no coincide, no empezar. Valorar mostrar preset/hash en la línea compacta queda como opción futura, sin implementar.
- **Mecánica:** Flick 10/10 en fuertes; relatos «si voy fuerte se queda al medio el rombo» y «debo apuntar lento y acelerar». El diagnóstico comunicado es el acoplamiento de apuntado y potencia en un mismo gesto, no un problema que se resuelva solo cambiando escala. La regla de riesgo empujó escala/memoria cortas, y saturación/control medio físicos no validan ese compromiso. Sin ajuste de umbral, ganancia, lookback, memoria o potencia ahora.

### Alcance autorizado de la siguiente entrega

Rama **firstshot/s1-gesture-mechanics-proposal**, desde main 31bbe03, worktree documental en `.worktrees/firstshot-s1-gesture-mechanics-proposal`. [Propuesta nueva](2026-10-27-first-shot-s1-gesture-mechanics-proposal.md) con diagnóstico de evidencia existente y tres alternativas sin seleccionar: dos fases con confirmación explícita; fijación solo al soltar; conservar Flick con estudio acotado de su sensibilidad. Cada una expone ventajas, riesgos, relación con 150 ms/P<0,60, conflictos normativos que requerirían decisión propia y forma de evaluarla. No hay valor nuevo elegido ni replay nuevo. Protocolo futuro exige **autorización propia**, preparación/verificación por bloque y restauración de P0 con cotejo habitual; no autoriza sesión ni instalación por estar escrito.

SoloMarkdown: propuesta, AGENTS.md y este registro. Sin código/tests/presets/defaults/spec/planes/criterios/bandas/umbrales, sin nuevas ejecuciones de suites/check-release, builds, instalaciones, sesiones ni acceso al iPhone. Defaults 6095c0b38614, proyección 58 anteriores00a501ad7066,41 heredadas0d39e6016bd8 y SimulationKey166a96e319c7 intactos. **PR documental lista para Claude sin fusionar y detenerse. S1 sigue sin aceptar; S2–S4/T8 no autorizados.** Perfil registrado vence 2026-10-28T14:59:31Z; ninguna instalación autorizada, cualquiera futura exige **renovarlo antes y comprobar su vigencia efectiva**.

Entrega en [PR #23 documental lista para Claude](https://github.com/rafaallendes-boop/squash-clash/pull/23), abierta/no borrador y **sin fusionar**, fuente **b1f66c21db26b36df09744b483d3e0aaf21c5a71**. El commit posterior únicamente registra el enlace. Revisión pendiente; no aprobación de alternativa, candidato o S1. Detenerse conforme al alcance autorizado.

## Cambios solicitados por Claude en PR #23 y decisión del PO — 2026-10-27

Claude solicita cambios **solo documentales** sobre el head **55e061a877870f202c306be3bad8fe1eebeed745**. Datos, código citado, enlaces y alcance fueron verificados correctos; no se recalculan ni se vuelven a medir. El PO autoriza registrar su decisión y aplicar las correcciones en la misma rama `firstshot/s1-gesture-mechanics-proposal`; la PR sigue pendiente de revisión, sin fusionar.

### Decisión del PO, registro literal

- Respuesta literal del PO: «Vamos con la opción 1».
- Pregunta C5: el PO considera que A′ falla en su forma actual, con la evidencia de PR #22: Flick 10/10 en fuertes, medios 3/10 en banda, saturación 30 % y su «No» a las tres intensidades voluntarias. Es un juicio del PO; no convierte la calibración en Fail medido ni cambia criterios o umbrales.
- Mecánica elegida para investigar: control §6.2 B de spec v0.3, "apuntar, fijar y tensar". A′ se conserva como referencia de comparación; no se elimina del código.
- Descartadas por ahora: dos fases con confirmación, fijación solo al soltar (equivale a la v0.1 descartada), Flick menos sensible y §6.2 C. Quedan documentadas y no se borran.
- Esta decisión NO autoriza código, cambios de spec/planes, presets, defaults, builds, instalaciones ni sesiones.

### Correcciones documentales incorporadas

1. **Marco normativo y C5:** spec v0.3 §6.2 y First Shot C5 citados. Congelación por Flick identificada como protección de A′; fijación solo al soltar relacionada con la v0.1 descartada. Se añaden el control §6.2 B (quietud ≥120 ms [EXPERIMENTO], destello, tensado hacia abajo y potencia por distancia en mm con tope) y el control §6.2 C (potencia contextual asistida). El control §6.2 B se distingue de los dos gestos separados de M1. C5 ya tiene respuesta del PO: no queda elección abierta.
2. **Nombres:** alternativas propias renombradas M1/M2/M3. Se distingue siempre el **método de potencia B** del **control §6.2 B**, incluida la frase sobre mantener el estimador sin cambios. M1/M2/M3 y el control §6.2 C siguen documentados, descartados por ahora.
3. **Textos de fijación:** junto a Flick se citan §4.2 («permanece en el punto fijado»), §11.4 («la mira de intención queda fija donde se soltó») y M06 («la mira y la potencia se congelan al soltar»), sin resolver esa tensión mediante cambios normativos aquí.
4. **Protocolo:** 11:53:22.659306Z fue el primer lanzamiento tras el reinicio; repeat=3 produjo relanzamientos cada 3 s. El primer gesto, 11:53:34.664976Z, ocurrió 1,2 ms después de uno y terminó SystemCanceled. Precisión de la revisión de Claude, **sin recalcular las trazas**; verificar hash antes de cada bloque sigue obligatorio para cualquier sesión que se autorice.
5. **Siguiente entrega propuesta (no autorizada):** PR documental de diseño y plan del control §6.2 B, con TDD previsto. Debe resolver con el PO quietud/120 ms [EXPERIMENTO]/fijación involuntaria, feedback visual-háptico, dirección y mapeo distancia→P/tope/zona muerta/mm, N1/N2/cancelación/r/primer dedo/lanzamiento, reinterpretación del riesgo, comparación con A′ con las mismas cuatro series/mano/hash por bloque y textos de First Shot r2/T1.2/C5 que requerirían aprobación expresa. **Sin tocar spec v0.3.** No se inicia ni se autoriza esa siguiente PR por enumerarla.

**Regla de riesgo pendiente para el control §6.2 B:**150 ms/P<0,60 no se traslada tal cual a potencia por distancia, porque la pausa de 120 ms es el propio gesto de fijar. Su reinterpretación queda pendiente de decisión del PO en la siguiente entrega propuesta; no se relaja, modifica ni da por resuelta. La regla y casos originales del método de potencia B por velocidad permanecen intactos en su contexto. La calibración anterior no se convierte en Fail medido por el juicio sobre A′; S1 y el candidato siguen sin aceptar.

Delta desde 55e061a limitado a **AGENTS.md, este registro y la propuesta de mecánica**. Spec v0.3, propuesta First Shot r2, planes, código, tests, presets, defaults, criterios, bandas y umbrales intactos; solo se citan. Sin suites, check-release, builds, instalaciones, sesiones, replay o acceso al iPhone. Defaults 6095c0b38614, preset 08b6fe90889b sin promover y proyecciones/SimulationKey sin cambios. Verificación solo documental de alcance y enlaces. **PR #23 lista para Claude sin fusionar y detenerse; S1 sin aceptar, S2–S4/T8 no autorizados.** Perfil vence 2026-10-28T14:59:31Z; cualquier instalación futura exige renovarlo antes y comprobar vigencia efectiva.

## PR #23 cerrada y diseño/plan documental del control §6.2 B autorizado — PO, 2026-10-27

Claude aprueba PR #23 en **f5b4beae3aa796c3c324efaed198ebe3f840c6b7**, sin correcciones pendientes; el PO autoriza la fusión condicionada. Comprobados heads local/remoto exactos, sin commits posteriores, PR abierta/no borrador/sin conflictos y árboles limpios. Merge **188d59ec3bd5eb59b0340e9342e8a47afc94bdef**, segundo padre exactamente el head aprobado; main sincronizada/limpia. Worktree retirado sin --force; **no tenía paths ignorados (0 archivos/0 bytes)**. Inventarios antes/después y resumen del conjunto vacío cotejados en `Logs/archive-s1-gesture-mechanics-proposal-20261027`; no hay hashes de archivos que preservar en ese conjunto. Ramas remotas conservadas. Sin suites/check-release/builds/instalaciones/sesiones/acceso al iPhone para el cierre; el registro se hace en esta siguiente PR, sin commit aparte en main.

Solo tras completar ese cierre se crea `firstshot/s1-control-b-design`, base188d59e, worktree `.worktrees/firstshot-s1-control-b-design`. El PO autoriza el documento [diseño y plan](2026-10-27-first-shot-s1-control-b-design.md), con opciones, ventajas/riesgos y **recomendación de Codex por punto, no elección por el PO**: quietud/fijación/deshacer, feedback, tensado/potencia, fases/r/N1/N2/cancelaciones, reinterpretación pendiente del riesgo, claves propuestas/R2-3, comparación futura con A′, tareas TDD previstas dentro de S1 sin renumerar, redacciones futuras de First Shot r2/plan y lista final de decisiones del PO.

Se autoriza lectura de código/evidencia y análisis offline descriptivo **predeclarado antes de calcular**, sobre138+49 gestos existentes para pausas compatibles con≥120 ms. La predeclaración se registra primero en el documento; se declara un JSON derivado nuevo bajo docs/validation/evidence. Es indicativo de datos de A′, no medición del control §6.2 B ni medición nueva. No se modifican datos originales. La regla 150 ms/P<0,60 para A′/método de potencia B queda intacta; reinterpretarla para distancia sigue pendiente de decisión expresa del PO.

No se autoriza implementar, cambiar spec v0.3/propuesta r2/planes/código/tests/presets/defaults/criterios/bandas/umbrales, ejecutar suites/check-release/builds/instalaciones/sesiones ni acceder al iPhone. A′ sigue por defecto; defaults 6095c0b38614, valores heredados y SimulationKey intactos. PR documental para Claude sin fusionar; **S1 sin aceptar, S2–S4/T8 no autorizados**. Perfil vence 2026-10-28T14:59:31Z; esta PR no instala y cualquier instalación futura exige renovarlo antes y comprobar vigencia efectiva.

Diseño documental completado para revisión: diez puntos con opciones/ventajas/riesgos y recomendaciones de Codex pendientes del PO; no se adopta ninguna como decisión. Se proponen Q2 (velocidad+radio con muestras reales), feedback visual/forma y háptica opcional apagada, tensado descendente por distancia actual, puerta de fases/abandono antes de fijar y cancelación N1 global, riesgo R2 por fases **pendiente de aprobación expresa**, claves separadas con A′ por defecto y tareas futuras 1.2b sin renumerar el plan. Los valores iniciales/rangos son [EXPERIMENTO]; no son nuevos defaults ni criterios aplicados. El método de potencia B por velocidad se distingue del control §6.2 B por distancia.

Predeclaración del análisis: commit **a37986beffd812237213f6789bf9756bbf64965c**, anterior al cálculo. QVR fijo (≤15 mm/s, radio≤1 mm,≥120 ms, hueco≤51 ms,≥3 tiempos distintos) y comparador QV sin radio; sin modificar parámetros después. Resultado indicativo con A′: revalidación 24/138 QVR (29/138 QV), tanto interior completo como apuntado observable; calibración 7/49 en interior completo y 6/24 observables, con 25 no observables por falta de timestamp de Flick. No tasa de fijaciones involuntarias ni medición nueva/del control §6.2 B; huecos/silencio impiden concluir que no hubo pausa. Originales intactos. Tabla en documento y único JSON derivado declarado con hashes, ámbitos e índices de ventanas, sin script de producto versionado.

Alcance final: **tres Markdown (diseño/AGENTS/aprobaciones) y un JSON derivado declarado**, conforme a excepción del PO para el análisis. Textos de First Shot r2/T1.2/C5 solo sugeridos dentro del documento; los archivos normativos no cambian. Lista final numerada de decisiones del PO y recomendaciones, sin preguntas de implementación asumidas resueltas. Sin suites/check-release/build/instalación/sesión/acceso al iPhone; PR para Claude sin fusionar y detenerse, sin aceptar S1 ni autorizar S2–S4/T8.

Entrega en [PR #24 lista para Claude](https://github.com/rafaallendes-boop/squash-clash/pull/24), abierta/no borrador y **sin fusionar**, fuente documental **77463a357bdaff97031193739f02cca6ff73f67c**; predeclaración anterior al cálculo **a37986b**. El commit posterior solo registra este enlace. Lectura/verificación documental y análisis autorizado únicamente; no decisiones del PO asumidas, criterios modificados ni implementación. Detenerse conforme al alcance autorizado.

## Cambios solicitados por Claude en PR #24 — 2026-10-27, solo documentación

Claude solicita cambios sobre **9e71f438c5c33d70a2575ac4e8f3fe7419bbf3c4**; el PO autoriza aplicarlos en la rama firstshot/s1-control-b-design. No es aprobación del diseño ni de una opción. Claude verifica merge PR #23, alcance, hashes de fuentes y predeclaración (solo espacios tipográficos posteriores) y reproduce el análisis con resultado idéntico 24/29,7/7,6/6 sobre 24. **No se recalcula**: JSON derivado, fuentes y parámetros analíticos permanecen intactos.

1. **Choque N1–tensado, bloqueante:** se incorpora el ejemplo numérico de Claude, sin cálculo nuevo: ganancia 0,12, N1 de 6 mm en Began, apuntar~15 mm arriba/fijar/tirar~15 mm abajo, P≈0,34 media y regreso al origen→Canceled. Banda media candidata 11,5–24,8 mm y apuntado alto habitual 0–23 mm (2,2–5 m) solapan. U1 depende de N1. Se retira N1 global/no reanclar como recomendación general del control §6.2 B; opciones NB0/NB1/NB2 con ventajas/riesgos. Recomendación **de Codex, no decisión del PO**: investigar NB1 (reanclar y desarmar al fijar), condicionada a aprobación y a su costo al reducir potencia. NB2 propone otra cancelación explícita durante Tensando; no se define un gesto nuevo. Actualizados punto 4, RB5, tarea 1.2b-3 y decisión 5. Regla/código de A′ y R2-2 intactos.
2. **Apuntado descendente:** RB6 describe apuntar abajo/pausar 120 ms/fijar sin querer/continuar abajo/soltar; R2 por fases no distingue esa continuación de un tirón. Se enlaza la respuesta literal anterior del PO sobre el drop «hacia abajo». Opciones de feedback (insuficiente como garantía), aceptación explícita del fijado u otra dirección (fuera del tensado normativo), con ventajas/riesgos. Recomendación de Codex: no cerrar R2 sin RB6 y estudiar con el PO aceptación distinguible del fijado; no se elige acción/umbral ni se reabre automáticamente M1. Actualizados punto 5, tabla RB1–RB6, TDD de riesgo y decisión 6; no se asigna potencia nueva ni se declara caso superado.
3. **Recorrido físico:** se incorpora el ejemplo comunicado de hasta ~14 mm de apuntado bajo más tirón candidato 40 mm, que puede necesitar más de 50 mm por debajo del apoyo. Margen dependiente de apoyo y fijación, borde/área segura/inicio iOS y d_full pendientes del PO. Se distingue filtro de Began en safeArea de cancelación durante un gesto ya activo; deferSystemGesturesMode no extiende la pantalla ni elimina el gesto del sistema. Canceled/foco invalida sin swing ni Ended inventado; no extrapolar distancia/P. Opciones E1 escala fija con margen explícito frente E2 escala adaptada; recomendación de estudiar E1, **sin medir ni elegir valores nuevos**.
4. **Lectura del análisis:** 17–25 % de pausas compatibles A′ en ámbitos observables es señal relevante para decidir 120 ms y para los riesgos N1/RB6, no validación del control ni tasa de errores. Se conserva su carácter indicativo, sin ajustar parámetros por resultados.

Delta desde 9e71f43 **solo tres Markdown**: documento de diseño, AGENTS.md y este registro. **El JSON derivado y las fuentes no cambian**. Spec/propuesta r2/planes/código/tests/presets/defaults/criterios/bandas/umbrales intactos. Sin cálculos nuevos, suites/check-release/builds/instalaciones/sesiones/acceso al iPhone; verificación documental de alcance/enlaces. Se citan adaptador existente y documentación oficial de Unity para el borde, sin ejecución ni cambio runtime. PR #24 lista para Claude sin fusionar y detenerse. **S1 sin aceptar; S2–S4/T8 no autorizados.** Perfil vence 2026-10-28T14:59:31Z; cualquier instalación futura exige renovarlo antes/comprobar vigencia efectiva.

## PR #24 cerrada y primera parada S1-sling — PO, 2026-10-27

Claude aprueba **25c180d8f3aa3093209855f892969dcc40b1cbed**, sin correcciones pendientes. El PO autoriza merge condicionado; comprobados heads local/remoto exactos, PR abierta/no borrador/sin conflictos y árboles limpios. Merge **956cfafbbfea6fef418fa24e6a3b0911663746be**, segundo padre exactamente el head aprobado; main sincronizada/limpia. Worktree retirado sin --force; **sin paths ignorados (0 archivos/0 bytes)**, inventarios vacíos cotejados y resumen en Logs/archive-s1-control-b-design-20261027. Ramas remotas conservadas. Cierre registrado dentro de esta PR, sin commit aparte en main; no suites/check-release/builds/instalaciones/sesiones/acceso al iPhone.

Se crea firstshot/s1-sling-prototype desde ese main, en .worktrees/firstshot-s1-sling-prototype. **Parada vigente: paso 1 de interacción, un único commit solo Markdown y PR en borrador, detenerse. Código solo después de aprobación de Claude de esta interacción.** Los pasos 2/3 se conservan con sus condiciones literales, no se ejecutan en esta entrega.

El adjunto anterior se conserva completo como **encargo técnico**, no como texto literal de las decisiones personales del PO; la decisión literal de puntos1–7 se registra en la revisión posterior. Se conserva además la aclaración del punto 6 recibida después; sus preguntas se incorporan por separado al protocolo. El título de aceptación de esa aclaración no convierte métricas/hipótesis en umbrales definitivos ni acepta S1.


<details>
<summary>Encargo técnico del PO, 2026-10-27 — conservado como encargo, no decisión literal</summary>

```text
Decisión del PO (2026-10-27), formalizada tras revisar el diseño de PR #24 con Claude. Copiar literalmente en el registro de aprobaciones el texto completo de mis decisiones adjunto.

PASO 0 — Fusión de PR #24
Claude aprobó PR #24 en head 25c180d8f3aa3093209855f892969dcc40b1cbed. Autorizo la fusión con las condiciones habituales: verificar head remoto/local exacto, PR abierta/no borrador/sin conflictos y árboles limpios; merge commit con segundo padre exacto; sincronizar main; retirar el worktree sin --force preservando ignorados en Logs/archive-s1-control-b-design-20261027; conservar ramas remotas. Registrar el cierre en la PR del paso 1.

PASO 1 — PR del prototipo, primera parada: diseño de interacción (solo documentación)
Rama/worktree firstshot/s1-sling-prototype desde main tras la fusión. Primer commit solo Markdown:
a) Registro literal de mis decisiones en aprobaciones y AGENTS.md.
b) Adendas D1 en la propuesta First Shot r2 (§4.2, C5, §11.4, M06) y en el plan (tarea 1.2b en S1, cobertura C5), aplicando mis decisiones. Spec v0.3 intacta. A′ y sus reglas (N1 por regreso al origen, Flick, 150 ms/P<0,60) intactos para A′.
c) Tabla de cambios respecto al diseño de PR #24: zona de cancelación explícita en lugar de NB0/NB1/NB2; pausa 120–150 ms con tolerancia configurable; recorrido 30–40 mm; G1 y medición de fijaciones involuntarias en lugar de decidir ahora G2/G3; sesión exploratoria con protocolo breve.
d) Diseño de la zona de cancelación. Punto de partida propuesto por Claude: zona por encima del punto de fijación, activa desde 12 mm [EXPERIMENTO] sobre él con cualquier desplazamiento lateral; cancela solo al soltar dentro; visible desde la fijación (✕ semitransparente, resaltado al estar dentro); franja neutra entre fijación y zona = P=0; antes de fijar, soltar = abandono sin consumo. Analizar críticamente: conflicto con el tirón hacia abajo y con reducir potencia volviendo arriba, falta de espacio si se fija cerca del borde superior (alternativa: salida lateral ~20 mm), oclusión de la mira o del frontis, mano izquierda/derecha. Proponer ubicación, tamaño y mecanismo finales con riesgos.
e) Protocolo breve definitivo (ver abajo) y lista de criterios verificables.
DETENERSE tras este commit y dejar la PR en borrador para revisión de Claude de la interacción. No escribir código hasta la aprobación.

PASO 2 — Implementación (solo tras la aprobación de Claude del paso 1)
Alcance: tarea 1.2b dentro de S1, sobre la arquitectura actual (TouchSample/GesturePreview/Core Shot/ShotOverlay/DevPanel). Sin sistema paralelo ni abstracciones generales.
- Selector gesture.control capturado al apoyar; A′ por defecto. Claves nuevas K2 con unidades explícitas, ajustables en DEV, compatibles R2-3. Valores iniciales [EXPERIMENTO]: pausa 120 ms (rango permitido para probar 120–150), tolerancia de quietud configurable (velocidad 15 mm/s, radio 1 mm iniciales del diseño), huecos ≤51 ms, mínimo 3 muestras, guardia de arrastre previo ≥3 mm; zona muerta 2 mm; tope de potencia 35 mm (rango 30–40), curva lineal con saturación; zona de cancelación según el paso 1; destello 120 ms.
- Mira libre mientras se apunta; fijación con destello y cambio de estado; mira inmóvil tras fijar; nuevo origen de potencia en el punto de fijación; potencia solo después de una fijación confirmada (proyección hacia abajo, distancia actual, lateral sin potencia, volver arriba reduce).
- Estados visibles: Apuntando / Fijada / Tensando / Zona de cancelación / Inválida, con forma y texto además de color, sin tapar el frontis (mínimos de PR #19).
- El cierre con Ended idéntico (dt=0) no debe anular la potencia del control nuevo; datos contradictorios o no monótonos siguen siendo conservadores. Sin cambiar A′.
- Objetivos de protocolo: marcas mínimas en el frontis solo en DEV/protocolo, configurables, sin afectar prod.
- Registro por gesto: control, transiciones con timestamp (Began, fijación, inicio de tensado, entrada/salida de zona de cancelación, Ended), posición de fijación frente al objetivo pedido, d actual/máxima/lateral, P y banda, outcome (Swing, Cancelado por zona, Abandonado antes de fijar, SystemCanceled, Tap), duración total y tiempo hasta fijar, distancia mínima al borde inferior/área segura durante el tirón, hash/preset/escenario/intento.
- TDD con esqueletos compilables y RED por aserciones antes de implementar: quietud y tolerancia, sin acumular quietud por silencio, una sola fijación, guardia de arrastre, potencia cero antes de fijar (incluido apuntar hacia abajo), mapeo/zona muerta/tope/saturación, lateral y regreso, zona de cancelación (soltar dentro cancela, pasar por ella no; un tirón válido no cancela; cancelar no consume), abandono antes de fijar, r = Ended original, dt=0 terminal, primer dedo, lanzamiento/pausa/foco/DEV invalidan. Casos RB1–RB6 con las expectativas de esta decisión; RB6 se registra y se mide, no se oculta.
- Preset experimental separado S1-sling; defaults sin cambio de comportamiento (A′ por defecto); nuevo hash solo por claves autorizadas; 41 valores heredados y SimulationKey intactos byte a byte.
- Regresión completa fresca EditMode/PlayMode, sin errores CS; check-release dev/prod; build dev Release ARM64/no Development.
- Antes de instalar: comprobar vigencia efectiva del perfil (vence 2026-10-28T14:59:31Z; si ya venció o es día 28, renovarlo primero; la renovación la hace el PO). Respaldar y cotejar P0-calib-1, Po-1 y last-active.

PASO 3 — Sesión exploratoria en iPhone (protocolo breve, ~15 min, mano izquierda declarada)
Preparación habitual: preset S1-sling aplicado sin guardar, repeat=0, hash/preset/cero pendientes comprobados antes de CADA bloque, DEV cerrado.
- B0 Familiarización (2 min, registrada aparte como práctica).
- B1 Apuntar y fijar: 10 intentos a 5 objetivos (2 bajos tipo drop), soltar suave. Métricas: error de mira al fijar, tiempo hasta fijar, fijaciones lejos del objetivo, declaración del PO de fijación involuntaria.
- B2 Potencia: 10 suaves, 10 medios, 10 fuertes a objetivo central. Métricas: banda conseguida, dispersión de P y de d, saturación (advertencia >20 %), cercanía al borde.
- B3 Apuntar bajo y tirar: 10 a objetivo bajo con potencia media. Métricas: RB6 (potencia no buscada tras fijación prematura), error de mira, distancia al borde inferior.
- B4 Cancelación: 5 cancelaciones voluntarias tras fijar; cancelaciones accidentales contadas en todos los bloques.
- B5 Ritmo: 10 gestos seguidos lo más rápido que sea cómodo, objetivos y potencias alternados. Métrica: duración Began→Ended (mediana/p90), errores y abandonos.
Conservar todos los intentos; sin ajustes de parámetros durante la sesión (cambiar 120→150 ms requiere decisión del PO posterior, con evidencia). Las métricas se informan como descripción, sin umbrales de aceptación. Al final, preguntas separadas con respuesta literal: las de mi punto 6 (precisión, fijación, potencia, cancelación y fluidez, incluida «¿lo usarías en un rally rápido?»). Restaurar P0-calib-1 y comprobar en arranque nuevo hash/cero pendientes/tres archivos byte a byte.

Fuera de alcance: contacto real con la pelota, S2–S4, timing ±40 ms, ventana válida/T3, sonido/háptica/calidad del contacto, jugadores, IA, rallies, marcador. Ante un defecto: registrar y detenerse. PR lista para Claude sin fusionar hasta aprobación del PO. S1 sigue sin aceptar.
```

</details>

<details>
<summary>Aclaración posterior del PO — punto 6 completo, registro literal</summary>

```text
6. Criterios de aceptación del experimento
Quiero una prueba breve, repetible y con métricas que permitan comparar resultados.
Evaluaremos:
Precisión del apuntado

* ¿Puedo colocar la mira donde quiero?
* ¿Puedo apuntar bajo sin activar accidentalmente la potencia?
* ¿La mira permanece estable después de fijarse?

Fijación

* ¿Cuántas fijaciones involuntarias ocurren?
* ¿Cuánto tiempo necesito para fijar la mira?
* ¿Entiendo claramente cuándo quedó fijada?

Potencia

* ¿Puedo conseguir golpes suaves, medios y fuertes voluntariamente?
* ¿La potencia responde de forma consistente a la distancia?
* ¿Puedo alcanzar la potencia máxima cómodamente?
* ¿Existen limitaciones por el borde inferior de la pantalla?

Cancelación

* ¿Puedo cancelar cuando realmente quiero?
* ¿Ocurren cancelaciones accidentales?
* ¿La zona interfiere con los movimientos normales?

Fluidez

* ¿Cuánto demora el gesto completo, desde apoyar el dedo hasta soltar?
* ¿Se siente natural o demasiado mecánico?
* ¿Podría utilizarlo repetidamente durante un rally rápido?

Este último punto es importante: el control no solo debe funcionar en modo práctica, también debe tener potencial para un juego de squash dinámico.
Propón un protocolo breve con escenarios y métricas, sin convertir las hipótesis en umbrales definitivos antes de validarlas.
```

</details>

### Diseño documental sometido a Claude en esta primera parada

[Interacción y protocolo](2026-10-27-first-shot-s1-sling-prototype-interaction.md): tabla de cambios frente a PR #24; zona superior desde 12 mm con cualquier lateral y cancelación solo al Ended dentro; franja neutra P0. Fallback lateral 20 mm, profundidad mínima de diana8 mm y presentación fuera del frontis son **propuesta de Codex para revisión**, no decisiones adicionales atribuidas al PO. No se comprime una región para caber ni se mueve durante el gesto; si no cabe zona no se confirma fijación. Se explican retorno arriba, margen superior/inferior, oclusión/manos y RB6 no resuelto. Los rangos de preparación de la zona y las posiciones de cinco objetivos son propuestas de interacción pendientes de esa revisión, no parámetros ya implementados o ajustados en sesión.

Adendas D1 en First Shot r2 (§4.2/C5/§11.4/M06) y plan (1.2b en S1/cobertura C5), aplicando el registro: A′ permanece por defecto y conserva sus reglas; S1-sling tiene zona explícita, pausa120–150 inicial 120, tolerancia configurable, recorrido 30–40 inicial 35, G1/registro de fijaciones involuntarias y sesión B0–B5 exploratoria. Spec v0.3 intacta. No se toca código/tests/defaults/presets/criterios/bandas ni datos/JSON existentes, ni se ejecutan suites/check-release/builds/instalaciones/sesiones/replay/acceso al iPhone. Solo cinco Markdown en el primer commit; PR en borrador para Claude, no fusionar. Detenerse; S1 sigue sin aceptar y S2–S4/T8 sin autorizar. Perfil: no se instala aquí; comprobar vigencia efectiva y renovación a cargo del PO según decisión antes de cualquier instalación futura.

## PR #25: interacción aprobada y ajustes obligatorios previos al código — 2026-10-27

Claude aprueba la interacción del head **d114e26538edf766a6314e02c2b8164f3eba4272**: superior12 mm/lateral20 mm/profundidad8 mm, indicador fuera del frontis, no fijar sin zona, objetivosT1–T5 y protocoloB0–B5. El PO ordena un commit **solo Markdown** con correcciones antes de código y, después, continuar el paso2 (TDD/regresión/check-release/build), sin otra parada de interacción. La sesión posterior se realiza con el PO y la PR no se fusiona hasta su aprobación; S1 sin aceptar y S2–S4/T8 no autorizados.

Se distingue el encargo técnico conservado arriba de la **decisión literal del PO (puntos1–7)**, copiada completa entre las marcas recibidas, sin ampliar su alcance:

```text
Claude, revisé las alternativas para el nuevo control tipo honda de Squash Clash y, como Product Owner, quiero formalizar mis decisiones para continuar con S1.
Apruebo desarrollar un prototipo experimental del control tipo honda en el iPhone. No estoy aprobando todavía el control como definitivo ni autorizando S2.
El objetivo sigue siendo que el jugador pueda apuntar con precisión, controlar voluntariamente la potencia y ejecutar el gesto de manera intuitiva, sin que las distintas acciones interfieran entre sí.
1. Fijación de la mira: pausa + tolerancia de movimiento
Apruebo fijar la mira cuando el dedo permanezca relativamente estable durante un período breve.

* Probar inicialmente una pausa de 120–150 ms.
* Incorporar una tolerancia configurable de movimiento para evitar que pequeñas variaciones del dedo reinicien constantemente el contador.
* La mira debe permanecer libre mientras el jugador está apuntando.
* Al fijarse, debe existir una señal visual clara, por ejemplo un destello y cambio de estado.
* Una vez fijada, no debe desplazarse durante la fase de potencia.

Estos valores son experimentales. Quiero medir cuántas fijaciones involuntarias ocurren, especialmente cuando hago una pausa natural mientras todavía busco dónde apuntar.
2. Separar completamente apuntado y potencia
Apruebo que la potencia solo pueda comenzar después de una fijación confirmada.
El gesto será:

1. Arrastrar para apuntar al frontis.
2. Mantener el dedo relativamente quieto hasta confirmar la fijación.
3. Tirar hacia abajo para determinar la potencia.
4. Soltar para ejecutar el intento.

Al confirmar la fijación debe establecerse un nuevo origen de desplazamiento para calcular la potencia.
Quiero probar especialmente el caso de apuntar hacia abajo para ejecutar un drop. Ese movimiento no debe interpretarse como potencia antes de la fijación.
Si detectamos activaciones involuntarias, debemos registrarlas y analizar la causa, no modificar silenciosamente las reglas para conseguir una prueba favorable.
3. Cancelación mediante zona explícita
No quiero mantener la cancelación por regreso al punto inicial del toque, porque el nuevo gesto podría activar cancelaciones accidentales.
Apruebo experimentar con una zona de cancelación explícita, visualmente reconocible y distinguible del recorrido normal de potencia.
Condiciones:

* Debe ser fácil cancelar voluntariamente.
* No debe activarse accidentalmente durante un tirón válido.
* Cancelar no consume el intento.
* La zona no debe obstaculizar la mira ni los movimientos habituales del dedo.

Propón su ubicación, tamaño y mecanismo de activación para el prototipo. Prefiero que Codex y tú revisen la interacción antes de implementarla, especialmente para evitar conflictos con el gesto hacia abajo.
4. Recorrido de potencia: 30–40 mm
Apruebo experimentar con un recorrido físico de 30–40 mm para alcanzar la potencia máxima.

* La potencia debe crecer de forma predecible con la distancia del tirón.
* El valor máximo debe alcanzarse sin necesidad de movimientos exagerados.
* Al llegar al máximo, la potencia se satura.
* Los parámetros deben poder ajustarse desde el panel dev.
* No incorporar calibración individual por jugador todavía.

Quiero comprobar que puedo ejecutar intencionalmente golpes suaves, medios y fuertes.
También debemos observar qué ocurre cuando apunto bajo y el dedo queda cerca del borde inferior del iPhone. Si el espacio resulta insuficiente, necesito evidencia antes de decidir un ajuste.
5. Prototipo experimental en iPhone
No quiero seguir refinando indefinidamente el control en papel.
Autorizo un prototipo acotado dentro de S1, utilizando nuestra arquitectura y herramientas actuales, sin crear un sistema paralelo desechable ni sobrediseñar la solución.
Mantengamos nuestra metodología de desarrollo:

* TDD para la lógica del gesto y sus transiciones.
* Pruebas de regresión para no afectar First Ball.
* Parámetros experimentales en un preset separado.
* Implementación de Codex y revisión técnica de Claude.
* Instalación y validación real en el iPhone.
* PR sin fusionar hasta mi aprobación.

El prototipo debe permitir apuntar, fijar la mira, determinar potencia, cancelar y visualizar los estados del control. Todavía no debe incorporar el contacto real con la pelota.
6. Criterios de aceptación del experimento
Quiero una prueba breve, repetible y con métricas que permitan comparar resultados.
Evaluaremos:
Precisión del apuntado

* ¿Puedo colocar la mira donde quiero?
* ¿Puedo apuntar bajo sin activar accidentalmente la potencia?
* ¿La mira permanece estable después de fijarse?

Fijación

* ¿Cuántas fijaciones involuntarias ocurren?
* ¿Cuánto tiempo necesito para fijar la mira?
* ¿Entiendo claramente cuándo quedó fijada?

Potencia

* ¿Puedo conseguir golpes suaves, medios y fuertes voluntariamente?
* ¿La potencia responde de forma consistente a la distancia?
* ¿Puedo alcanzar la potencia máxima cómodamente?
* ¿Existen limitaciones por el borde inferior de la pantalla?

Cancelación

* ¿Puedo cancelar cuando realmente quiero?
* ¿Ocurren cancelaciones accidentales?
* ¿La zona interfiere con los movimientos normales?

Fluidez

* ¿Cuánto demora el gesto completo, desde apoyar el dedo hasta soltar?
* ¿Se siente natural o demasiado mecánico?
* ¿Podría utilizarlo repetidamente durante un rally rápido?

Este último punto es importante: el control no solo debe funcionar en modo práctica, también debe tener potencial para un juego de squash dinámico.
Propón un protocolo breve con escenarios y métricas, sin convertir las hipótesis en umbrales definitivos antes de validarlas.
7. Alcance y decisiones posteriores
Mantengamos intactas las decisiones pendientes de First Shot:

* Timing Perfecto ±40 ms.
* Ventana válida experimental y comparación con T3 puro.
* Sonido, háptica y calidad del contacto.
* S2–S4 sin autorización.

No quiero incorporar ahora jugadores, IA, rallies, marcador ni nuevas mecánicas.
Próximo paso: prepara el encargo técnico acotado para Codex, identificando los cambios respecto del diseño y plan vigentes, los riesgos y los criterios verificables del prototipo.
Si no hay contradicciones técnicas bloqueantes, podemos ejecutar este experimento en S1.
Después revisaré los resultados en mi iPhone y decidiré si aprobamos el control, hacemos otra iteración o reconsideramos la alternativa.
Mi prioridad es avanzar con evidencia, sin acelerar artificialmente el desarrollo y sin buscar perfección en papel antes de probar el gesto real.
```

Ajustes previos al código: literales solo en este registro, resumen/enlace en AGENTS; retirada pregunta17 por paráfrasis de Claude (la literal es16); B1/B3 con respuesta por intento OK/Se fijó antes de tiempo soloDEV/protocolo fuera del frontis, no gesto; indicadores descriptivos de tirón enB1 y error de mira, sin umbral; tiempos Began→fijación y último movimiento fuera de tolerancia→fijación. D1 del plan fuera de cita y D1/M06 fuera de lista conservan su contenido. Sin implementación antes de ese commit documental.

Antes de instalar: comprobar perfil efectivo, vence2026-10-28T14:59:31Z; renovación a cargo del PO y detenerse antes de instalar si no está vigente. Respaldar/cotejarP0-calib-1,Po-1,last-active. Esta aprobación no acepta el control definitivo ni habilitaS2.

### Continuación técnica S1-sling — 2026-10-27, sin aprobación física inferida

Ajustes obligatorios solo Markdown **f7f049c**, anteriores al primer código **7e43177**. Continúa la autorización del paso2 tras la aprobación de interacción d114e26. Modelo/control y protocolo sobre arquitectura existente; A′ por defecto, sin contacto de pelota. TDD compilable por aserciones, regresión fresca **404/404 EditMode y180/180 PlayMode**, sin CS. Defaults **45c4e9af5c8f** (85 claves, solo23 añadidas), preset experimental separado **7d1a61c39766** sin promoción;62 valores previos,41 heredados/proyección0d39e6016bd8 ySimulationKey166a96e319c7 intactos. Los casos de riesgo históricos no cambian; 150 ms/P<0,60 no se presenta como criterio del control por distancia.

Sigue pendiente export/check-release limpio dev/prod, build Release ARM64/no Development, respaldo/perfil vigente antes de instalar y sesión exploratoria B0–B5 con PO. No hay datos físicos nuevos ni aceptación de S1. Las16 preguntas literales y todos los intentos/respuestas por B1/B3 se conservan; sin ajuste120→150 ni otros parámetros durante campaña. Restaurar P0 y cotejar tres archivos en nuevo arranque. [Informe y evidencia](2026-10-27-first-shot-s1-sling-prototype.md). PR #25 sin fusionar hasta aprobación PO, S2–S4/T8 sin autorizar.

### Preparación física S1-sling — 2026-10-27

Producto **db72a00**: suites404/404 EditMode y180/180 PlayMode; check-release dev/prod gitDirty=false y build dev ReleaseARM64/noDevelopment correctos. Perfil incorporado comprobado antes de instalar17:04:06Z, vigente hasta2026-10-28T14:59:31Z/certificado y firma correctos; sin renovación porCodex.436 fuentes coincidentes. Respaldo/cotejo directo de los tres archivos antes/después de instalación/preparación. ArranqueP0 e458bfd5ce9e/cero pendientes. S1-sling separado cargado sin guardar; selección persistida original repuesta antes de aplicar/relanzar. Lanzamiento17:09:36.004367Z, **7d1a61c39766**,repeat0/cero pendientes,DEVcerrado y cero gestos nuevos antes de entregar.

Se pidió soloB0/izquierda tras explicar la mecánica, con identificación de estados/cancelación previa aB1. **No es aprobación física ni aceptación deS1.** Restan protocolo/16respuestas/restauraciónP0 en nuevo arranque y cotejo de tres archivos. [Informe](2026-10-27-first-shot-s1-sling-prototype.md). PR#25 sin fusionar; S2–S4/T8 sin autorizar.

### B0 S1-sling detenido antes de B1 — observaciones del PO, 2026-10-27

No es aceptación deS1 ni validación de calibración. Literal delPO tras práctica:

> Revisa lo que he hecho. pero a veces podía apuntar y se apuntaba bien, luego tensaba, otras veces se mantenía apuntando.

A la aclaración de cancelación: **«No llegué a ver esa zona»**. Sobre las interrupciones/tiempo:

> En ninguna parte de la pantalla me aparecía el tiempo, entonces en la 1ra pensé que ya habían pasado los 2 minutos, quizás es un punto de mejora de poner un timer en estas pruebas

Se conserva como observación/propuesta delPO, **sin ampliar la autorización a modificar interfaz o parámetros**. No dice qué acción exacta causó cada system-cancel.33 intentos registrados,32B0+1preview,17 fijaciones/15abandonos/1Tap,ninguna entrada o cancelación de zona. Dos adquisiciones incompletas85,918/35,442s, no120s continuos. Todos7d1a61c39766/cero pendientes. Auditoría descriptiva de la regla instalada, sin parámetros alternativos ni medición nueva:13 abandonos sin ventana apoyada120ms,2 con quietud solo fuera del área útil. No es prueba de un fallo geométrico de cancelación, que quedaNotEvaluated.

Se aplica la parada de la interacción aprobada§3 al no identificarse cancelación; **B1–B5 no iniciados**,16 preguntas finales no solicitadas tras esa parada, sin inferir respuestas. Propuestas de claridad de cancelación/temporizador/diagnóstico de quietud en el [informe](2026-10-27-first-shot-s1-sling-prototype.md), **pendientes de decisión y sin implementar**. Ningún ajuste120→150 ni tolerancias/escala/zona/criterios/bandas/defaults/spec/plan.

P0-calib-1 restaurado en nuevo proceso: primer lanzamiento **2026-10-27T17:21:51.8776630Z**, builddb72a00 dev, **e458bfd5ce9e**,cero pendientes. P0/Po-1/last-active directamente byte a byte iguales al respaldo;436 fuentes de producto coincidentes. Sin nuevas suites/check-release/builds/instalaciones o intentos físicos para cerrar. PR#25 lista para revisiónClaude **sin fusionar**, entrega técnica experimental con bloqueo de práctica explícito; **S1 sin aceptar**, S2–S4/T8 sin autorizar. Detenerse al entregar; perfil requiere comprobación efectiva antes de instalar y renovación porPO,vence2026-10-28T14:59:31Z.

### Autorización PO de iteración2 — 2026-10-27, PR#25

Claude valida head508f625: entrega técnica/paradaSL-S1-01/restauraciónP0 correctas;SL-S1-02 registrada. Reconoce responsabilidad de diseñoSL-S1-03 (silencio con contacto activo ignorado). El PO autoriza en la misma rama la corrección porTick/reloj real, sin inventar muestras, conservando120ms/1mm/15mm/s/arrastre3mm/mínimo de muestras/área; hueco51 no reinicia. Registrar sample/timer, elegibilidad teórica y frame. Zona visible respecto al dedo fuera del frontis,12/20/8 intactos, guía B0 y temporizador con fin/interrupción claros. AdendaD2, sin reescribir aprobación histórica.

Replay diagnóstico **anterior a implementación**, predeclarado58d037d, sin medición nueva:23 vs17 fijaciones,6 nuevas/17 anticipadas, mediana654,221ms; posibles involuntarias solo como indicadores, nunca intención inferida. Regresión/check-release dev-prod/build dev ReleaseARM64/noDevelopment autorizados conTDD. **Toda instalación desde27 exige renovación previa porPO y comprobar perfil incorporado efectivo**; perfil anterior vence2026-10-28T14:59:31Z y no habilita instalar esta iteración. Respaldar/cotejar3archivos. Sesión nueva separada: B0 completo120s y cancelación guiada; solo identificación PO de fijación/zona permiteB1–B5/16preguntas; si no, detener y restaurarP0/cotejar. PR sin fusionar, S1 sin aceptar, S2–S4/T8 sin autorizar. [Diseño D2 y diagnóstico](2026-10-27-first-shot-s1-sling-iteration-2.md).

### Asistencia para renovar y estado técnico D2 — 2026-10-27

PO literal: **«Ayudame a hacer esa renovacion. iphone en duplicar telefono ok»**. Autoriza asistencia con Xcode para la renovación requerida; no suprime el requisito de renovación previa ni la comprobación del incorporado. Se apartó con respaldo la caché de un perfil de esta app, sin tocar certificados/claves ni instalar. Renovación todavía pendiente de comprobar al firmar.

D2 implementada tras replay predeclarado58d037d/calculado88213d4 anterior al código. TDD por aserciones y regresión fresca **411/411 EditMode,184/184 PlayMode**, sin erroresCS; modeloC# reproduce23 fijaciones del diagnóstico, no medición. Defaults45c4e9af5c8f/preset7d1a61c39766,85 valores/rangos/41heredados/SimulationKey intactos; adendasD2 sin reescribir texto aprobado ni plan/specv0.3. Guía/temporizador/indicación local probados fuera del frontis. Respaldos nuevos de3archivos iguales; sin sesión nueva ni instalación. [InformeD2](2026-10-27-first-shot-s1-sling-iteration-2.md). Seguir build/check/perfil renovado antes de instalar; S1sin aceptar, sinS2–S4/T8 ni fusión.

### Renovación asistida completada e instalación D2 — 2026-10-27

La asistencia solicitada porPO permitió a Xcode generar un perfil nuevo al firmar: creado**2026-10-27T18:29:44Z**,vence**2026-11-03T18:29:44Z**. Verificación incorporada antes de instalar18:30:43Z: distinto del anterior, mismas autorizaciones/dispositivos/certificados, vigencia y firma correctas. Perfil previo respaldado, sin revocación ni exportación de claves. La restricción previa de renovación quedó satisfecha **antes** de instalar42265ca dev ReleaseARM64/noDevelopment. Nuevo vencimiento sustituye al28octubre como perfil actual; comprobar vigencia efectiva siempre.

411/411 EditMode,184/184 PlayMode,check-release limpio dev/prod y440 fuentes cotejadas. Tres archivos directamente iguales tras instalar/preparar. ArranqueP0e458bfd5ce9e/cero pendientes;S1-sling temporal7d1a61c39766/repeat0/cero pendientes, last-activeP0 repuesto antes de aplicar. Se pidió soloB0/izquierda con temporizador/cancelación guiada; **aún sin resultados ni juicios de aceptación**. ContinuaciónB1–B5 exige identificación porPO yB0completo; después16preguntas/restauraciónP0. PRsin fusionar/S1sin aceptar/S2–S4/T8sin autorizar. [Registro completo](2026-10-27-first-shot-s1-sling-iteration-2.md).


### D2 detenida antes de armar B0 — SL-S1-04, 2026-10-27

Respuesta literal del PO al pedido exclusivo de B0:

> Abrí la app, y se ejecuto un informe. Luego, presione el bo . 2 min practica, no se apuntaba ni aparecía nada, por lo que cerré la app y la volví a abrir. Apareció p0. Luego se ejcuto el mismo informe de S8. y la volví a cerrar

No se autoriza una corrección nueva por inferencia de esta respuesta. Se detiene la práctica: no hay inicioB0 ni gestos/muestreoD2 registrados; B0NotEvaluated, B1–B5 y16preguntas no iniciados. Dos P2 incidentales completos, primero con S1-sling7d1a61c39766/cero pendientes, después de reiniciar con P0e458bfd5ce9e/cero pendientes. No se registran como campaña autorizada ni como medición de gesto. El código restaura host pausado tras P2 y rechaza medir en pausa; el toque activador y el rechazo concreto de B0 no constan. No afirmar autoarranque ni culpa del PO.

[Informe D2 y propuesta de investigación sin implementar](2026-10-27-first-shot-s1-sling-iteration-2.md), [evidencia nueva separada](../validation/evidence/2026-10-27-s1-sling-iteration-2-startup-incident.json). Presets/selección directamente byte a byte iguales al respaldo; P0 observado en reinicio delPO20:12:34Z, pero el P2 posterior puede dejar el reloj pausado. Arranque final nuevo pendiente por bloqueo/autenticación local; no declarar restauración final confirmada todavía.440 fuentes intactas, ninguna suite/build/instalación ni intento físico nuevo para cerrar. Perfil renovado vence2026-11-03T18:29:44Z; comprobar vigencia efectiva antes de cualquier instalación. PR#25 sin fusionar, S1 sin aceptar yS2–S4/T8 sin autorizar; detenerse para decisiónPO/Claude.


### Cierre documental D2 tras arranque final — 2026-10-27 local

Aclaración literal del PO sobre el incidente: **«no, no vi toca para continuar, solo se abría el informe»**. Se conserva como relato; no confirma la causa alternativa ni autoriza cambios de código. Su «Listo» habilita únicamente la comprobación final de P0, sin repetir B0.

Nuevo proceso verificado **2026-10-28T01:07:52.1670880Z (27octubre22:07:52 Chile)**:42265ca dev, **P0-calib-1/e458bfd5ce9e/cero pendientes**, DEV cerrado, pantalla normal/botónPausa y sin informe visible. Cinco lanzamientos capturadosP0/free; sin nuevoP2/bloque/gesto/muestreo. La primera solicitudCLI durante reconexión falló y no cuenta como arranque; reintento tras conexión sí completó. Los tres archivos directamente byte a byte iguales al respaldo anterior a instalar, selecciónP0/repeat3 restaurados,440 fuentes intactas. [Informe y hashes](2026-10-27-first-shot-s1-sling-iteration-2.md), [evidencia sin identificadores personales](../validation/evidence/2026-10-27-s1-sling-iteration-2-startup-incident.json).

**PR#25 lista para Claude, sin fusionar. SL-S1-04 abierto; B0 D2 NotEvaluated y B1–B5 no iniciados.** Sin respuestas de aceptación inferidas, correcciones adicionales, suites/check-release/builds/instalaciones ni sesión física nueva para cerrar. Detenerse para decisiónPO/revisiónClaude; S1 sin aceptar yS2–S4/T8 sin autorizar. Perfil renovado vigente hasta2026-11-03T18:29:44Z; comprobar vigencia efectiva antes de cualquier instalación futura.


### Aprobación técnica D2 y corrección acotada SL-S1-04 — PO tras revisión cecd74c

Claude aprueba técnicamente la iteración2, sus datos/TDD/regresión411/411 y184/184 y la parada/restauración. No aceptaS1 ni resuelveSL-S1-04. PO autoriza en la misma rama/PR25: auditoría de activaciones DevBar/DEV (control/tiempo real/preset/hash/pausa), intentosB0–B5 aceptados/rechazados con motivo; protecciónEscenario/P2/H17 mientrasS1-sling esté preparado o bloque activo, rechazo visible/remedio, etiquetaP2 opcional solo texto. TDD, regresión/check-release/builddevReleaseARM64/noDevelopment; vigencia efectiva del perfil incorporado y respaldo/cotejo3archivos antes de instalar. Sin modificarD2/modelo/parámetros/zona ni atribuir causa alPO/autoarranque.

ObservacionesClaude **solo registro, sin corrección del detector**: O1 mínimo3 reales por candidato puede impedir fijación tras parada en seco; contar en sesiónD2 abandonos quietos≥120ms con<3 muestras. O2 edad13–28ms (p50/máximo citados porClaude), posible movimiento en tránsito tras fijacióntimer; contar muestras timeReal<lockObservedAt recibidas después de fijar y su desplazamiento. No son nuevas mediciones ni ajustes. Preservar resultado23frente17 y diferencia con estimaciónClaude.

Antes de entregar teléfono: preset/hash temporalS1-sling y juego sin pausa visibles; PO pulsaB0 y **confirma ver cuenta atrás antes de gestos**. B0completo120s/guía/identificación fijación-zona/cancelaciónguiada; solo comprensión permiteB1–B5 y16preguntas literales. Datos nuevos separados; al detener/terminar restaurarP0/cotejar3archivos. PRsin fusionar hastaPO, S1sin aceptar/S2–S4/T8sin autorizar. Perfil renovado hasta2026-11-03T18:29:44Z, comprobar incorporado efectivo antes de instalar.


### Avance técnico autorizado de SL-S1-04 — sin aceptación física

Fuente4cec622,9RED por aserción→9GREEN/contención adicionalRED→10/10,411/411EditMode y194/194PlayMode frescos sinCS. Exports/check-release dev-prod gitDirty=false y builddevReleaseARM64/noDevelopment;444 fuentes coincidentes. No cambiosD2/parámetros/zona/defaults/preset/41heredados/SimulationKey/spec/planes/criterios. Perfil incorporado renovado y vigente hasta2026-11-03T18:29:44Z, comprobadoantesdeinstalar2026-10-28T01:40:49Z; firma/equipo/bundle/dispositivo/certificado correctos. Instalación completada y3archivos directamente iguales al respaldo, selecciónP0; **arranque4cec622 aún no verificado**, rechazado por bloqueo y pendiente autenticación local deDuplicación. No confundir registros42265ca anteriores con esta instalación.

[Informe y evidencia](2026-10-27-first-shot-s1-sl04-controls.md). No sesión nueva ni cuenta atrás confirmada; O1/O2 pendientes de datos. Preparación/hash/juego sin pausa antes del traspaso y confirmaciónPO de cuenta atrás antes de gesto siguen obligatorias. No inferir aprobación de este head ni aceptaciónS1. PR#25en borrador/sin fusionar; S2–S4/T8sin autorizar.


### Continuación de preparación SL-S1-04 — 2026-10-28

RespuestaPO a autenticación «listo!». Arranque4cec622 confirmado09:57:01.393491Z/P0e458bfd5ce9e/cero pendientes;3archivos directamente iguales al respaldo. Dos activaciones deDEV/Presets auditadas nativamente, sinP2/H17/Escenario/bloques/gestos. S1-sling aún no cargado. Herramienta pierde procesoDuplicación durante desplazamiento; reabrir muestra otra solicitud local de contraseña, pedida alPO sin compartirla. No se identifica causa ni se amplía autorización. [Evidencia de preparación](../validation/evidence/2026-10-28-s1-sl04-preparation.json). Preparación/cuenta atrás/B0/sesión/restauración final siguen pendientes; no confundir esta preparación con prueba física. Sin código/suites/build/instalación nuevos; PR25borrador/sin fusionar/S1sin aceptar/S2–S4/T8sin autorizar.


### Traspaso S1-sling tras preparación verificada — 2026-10-28

Nueva respuestaPO de acceso «listo». Se cargóS1-sling yrestauró selección persistidaP0 original **antes** de aplicar/relanzar. Arranque/lanzamiento10:06:22.005822Z/4cec622 dev/S1-sling7d1a61c39766/cero pendientes/repeat0/sin pausa;3archivos directamente iguales al respaldo y presetS1igual a rama. DEVcerrado/líneaListo/3experimentos deshabilitados/B0accesible, sin bloques/gestos/muestreo nuevos. [Evidencia](../validation/evidence/2026-10-28-s1-sl04-preparation.json).

Pedido alPO: verificarbuild/preset/hash, pulsarB0 yconfirmar cuenta atrás desdeMac antes delprimer gesto;tras confirmar practicar izquierda hastaCompletado sin esperarotrarespuesta, con guíaDENTRO/suelta, sinDEV/salir. Si no ve o no entiende límite detener/noB1. Se recordó que no hay golpe de pelota. La confirmaciónyB0completo/comprensión/cancelación guiada todavía pendientes; no inferir aceptaciónS1. PR25borrador/sin fusionar/S2–S4/T8sin autorizar; no manipular teléfono durante práctica.


### Repreparación tras salida/reinicio del PO — 2026-10-28

Literal: «tuve que salir por lo que se reinició. Dejé duplicar abierto para que hagas toda la configuración nuevamente y haga la prueba .». El registroconfirmaP0 yningúnB0/bloque/gesto4cec622 previo. Repreparación solicitada sin instalación nueva: S1-sling cargado/selecciónP0 original repuesta antes de aplicar/relanzar11:53:11.374404Z, **7d1a61c39766/cero pendientes/repeat0/sin pausa**, DEVcerrado/Listo/experimentos protegidos yB0accesible sin pulsar.3archivos directamente iguales al respaldo/444fuentes intactas. [Evidencia con historial conservado](../validation/evidence/2026-10-28-s1-sl04-preparation.json). Confirmación de cuenta atrás porPO antes de primer gesto/B0completo/guía/comprensión/cancelación siguen pendientes. Sin código/suites/build/instalación nuevos; PRsin fusionar/S1sin aceptar/S2–S4/T8sin autorizar.


### Devolución del primer B0 D2 completo — 2026-10-28

LiteralPO:

> Listo, hice la prueba. Pero apuntar debajo de la línea como para hacer un drop me costaba apuntar o que se ponga el rombo amarillo. El resto lo entendí bien

Registros:120,001808s/31intentos B0,27fijaciones/4cancelaciones de zona/4abandonos, todos7d1a61c39766/cero pendientes/comparables. Cancelación guiada marcada porapp; no interpretar eso como declaración de voluntariedad. No se recibió confirmación separada de cuenta atrás antes delprimer gesto; se preguntó retrospectivamente si la vio y Completado. También se preguntó por comprensión/cancelación y qué línea corresponde aldrop. [Informe nuevo/evidencia/O1-O2](2026-10-28-first-shot-s1-sl04-B0.md). Respuestas pendientes/B1–B5no iniciados/16preguntas finales sin responder; no aceptaciónS1.3archivos iguales al respaldo,configS1temporal activa/restauraciónP0pendiente al cerrar; sin código/suites/build/instalación nuevos. PRsin fusionar/S2–S4/T8sin autorizar.


### Respuestas de comprensión B0 y línea del drop — 2026-10-28

Pregunta de cuenta atrás antes delprimer gesto yCompletado alfinal: **«si»**. Pregunta de reconocer fijación/zonaDENTRO ycancelar voluntariamente soltando dentro: **«así es»**. Qué línea para drop: **«si, la linea horizontal del medio del frontis»**. Literales conservados; no respuestas a las16preguntas finales ni aceptaciónS1. Confirmación de cuenta atrás retrospectiva, sin mensaje previo alprimer gesto: desviación conservada. Con B0completo/cancelación guiada ycomprensión confirmada, continuar según autorización previa aB1; no es autorización de ajuste.

Próximo bloque soloB1/10intentos/feedback individual;verificaciónPO de preset/hash/cero pendientes/Jugando antesdearmar yregistro nativo delresultado. SiP0/otrohash/pausa,detener yavisar. ConfigS1temporal activa,3archivos iguales alrespaldo;restauraciónP0pendientealcierre. Sin código/suites/build/instalación nuevos;PRsin fusionar/S1sin aceptar/S2–S4/T8sin autorizar.


### B1 original y repetición adicional — 2026-10-28

PO literal: **«Listo. Hice 2 intentos porque en el 2do intento si intente de apuntar al cuadrado»**. Registro: dos tandas10+10/20respuestas;sesión2original,sesión3adicional,no sustitución. Aclaraciónliteralobjetivos: **«Desde la 2da tanda, en la primera fue desde el gesto 5»**. Aclaraciónsobre uso de«Se fijó antes de tiempo»: **«También intentos fallidos»**. Se conservan las6pulsacionesraw sin convertirlas en tasa de6fijacionesprematuras; intenciónNotEvaluated/significadomixto. No inventarjuicios ni aceptarS1por resultados. [Informe/evidencia](2026-10-28-first-shot-s1-sl04-session.md).

Continuación yaautorizada: B2T3/10suaves→10medios→10fuertes,mismaizquierda/hash7d1a61c39766/0pendientes/Jugando comprobadosporPOantesdecadatanda;sin cambiarparámetros ni sustituirfallos. NoB3antesderevisarB2;aclararfeedbackantesdeB3. ConfigS1temporalactiva/3archivosigualesalrespaldo/restauraciónP0alcierrependiente. Sin código/suites/build/instalación nuevos;PRsin fusionar/S1sin aceptar/S2–S4/T8sin autorizar.


### B2 completo, sin decisión de aceptación — 2026-10-28

POliteral: **«Listo, hice las 3 tandas»**. Se conservan30intentos deB2 suave/medio/fuerte, sesiones4/5/6, todos7d1a61c39766/cero pendientes/configcomparables, sin cambios. Bandas solicitadasconSwing5/10,9/10,9/10; saturación0/0/10%. Abandono medios8 y fuerte3P0 por d0,607mm bajo zona muerta conservados, no sustituidos. [Informe/evidencia](2026-10-28-first-shot-s1-sl04-session.md); sin interpretación de aceptación ni nueva decisiónPO. Aclaración sobre seguirT3 pendiente. SiguienteB3según autorizaciónprevia,10mediosT1/T2alternados/izquierda,verificarhash/pendientes/Jugando yaclararfeedbackantes;noB4todavía. ConfigS1temporalactiva/3archivosiguales/restauraciónP0alcierrependiente;sin nuevos cambios deproducto/suites/build/instalación. S1sin aceptar/PRsin fusionar/S2–S4/T8sin autorizar.


### Aclaración de tarea B2 e instrucción B3 — 2026-10-28

Respuesta literalPO: **«no, no intente aputnar al cuadrado cenral, apuntabaa simulando golpes»**. B2no buscóT3 en sus tres tandas: potencias ydistancias conservadas/sin sustitución; distancia alcuadrado no se interpreta como precisiónintentandoacertarlo. Adhesión de tarea separada de comparabilidad deconfig. No nueva decisión de aceptación ni ajuste. B3solicitado según autorizaciónprevia: diezmedios buscandoT1/T2bajos,izquierda/precheckhash7d1/0pendientes/Jugando; antes se aclaró que feedback«Se fijó antes de tiempo» soloesanticipaciónobservada,OKparalosdemásresultadossinconvertirlos enacierto. Pendiente devolucióndelbloque,noB4todavía. [Informe/evidencia](2026-10-28-first-shot-s1-sl04-session.md);restauraciónP0alcierrependiente/PRsin fusionar/S1sin aceptar/S2–S4/T8sin autorizar.


### Cierre de sesión SL-S1-04, sin aceptación ni autorización adicional — 2026-10-28

[Informe deB3–B5,111registros separados,16preguntas/respuestas literales y restauración](2026-10-28-first-shot-s1-sl04-session.md). PO completóB3/B4/B5 antes de nueva instrucción separada paraB4/B5; se conserva la cronología. Literal de devolución íntegro en informe/evidencia; «Se fijó antes de tiempo» volvió a incluir ausencia de rombo, sin tasa de fijaciones prematuras. B4literal «Sin problemas»;cincoZoneCanceled. B3tresabandonos/una cancelación/B5dosabandonos, sin sustituir. Ninguna nueva decisión de aceptación, ajuste, código o práctica inferida.

Las16preguntas se presentaron cada una por separado. Se registran sin ampliación:

- ¿Puedo colocar la mira donde quiero? → **«A veces si, otras veces no»**.
- ¿Puedo apuntar bajo sin activar accidentalmente la potencia? → **«A veces si otras veces no»**.
- ¿La mira permanece estable después de fijarse? → **«si»**.
- ¿Cuántas fijaciones involuntarias ocurren? → **«yo creo que un 30 - 40%»**.
- ¿Cuánto tiempo necesito para fijar la mira? → **«poco tiempo»**.
- ¿Entiendo claramente cuándo quedó fijada? → **«si»**.
- ¿Puedo conseguir golpes suaves, medios y fuertes voluntariamente? → **«si»**.
- ¿La potencia responde de forma consistente a la distancia? → **«si»**.
- ¿Puedo alcanzar la potencia máxima cómodamente? → **«si»**.
- ¿Existen limitaciones por el borde inferior de la pantalla? → **«no»**.
- ¿Puedo cancelar cuando realmente quiero? → **«si»**.
- ¿Ocurren cancelaciones accidentales? → **«no»**.
- ¿La zona interfiere con los movimientos normales? → **«si»**.
- ¿Cuánto demora el gesto completo, desde apoyar el dedo hasta soltar? → **«1 segundo creo maximo»**.
- ¿Se siente natural o demasiado mecánico? → **«igual es mecanico, me gustaría algo que fuera natural»**.
- ¿Podría utilizarlo repetidamente durante un rally rápido? → **«si, pero se vería como pausado»**.

«30 - 40%» y«1 segundo creo maximo» son estimaciones subjetivas, no métricas/umbrales. PO no aceptaS1 por estas respuestas. Registros de potencia/cancelación conservados junto a apuntado bajo intermitente,zona que interfiere ygesto mecánico/rally pausado; no cambiar el diseño a partir de inferencias. No se solicita repetir ni continuar una sesión.

Restauración verificada: proceso nuevo2026-10-28T15:16:26.232098Z/4cec622 dev/P0-calib-1/e458bfd5ce9e/0pendientes;cinco lanzamientosP0/normal/DEVcerrado/Pausa y3archivos directamente iguales al respaldo. Primer intento coniPhonebloqueado rechazado,no éxito.444fuentes/35protegidos intactos,sin nuevos suites/check/build/instalación/código/defaults/presets/spec/planes/criterios/bandas/umbrales. Perfil renovado hasta2026-11-03T18:29:44Z;vigencia efectiva antes de cualquier futura instalación,ninguna autorizada ahora. PR#25lista paraClaude,sin fusionar;detenerse. **S1sin aceptar;S2–S4/T8sin autorizar.**


## Revisión final de Claude y autorización del PO — PR #25, 2026-10-28

Comunicación recibida del PO, conservada como revisión técnica experimental, **no aceptación de S1 ni S1-sling**:

> Revisión de Claude sobre PR #25, head fe78564372ef9f38fbaea5cbc17d1fa5253bdea5: APROBADA como entrega técnica experimental.
> - Corrección SL-S1-04 (4cec622) acotada y correcta: auditoría dev-control activation/result sin datos pegados, protección de Escenario/P2/H17 en botón y callback, rechazo visible de B0–B5 con motivo y remedio, línea preset/hash/Listo, rótulo «Benchmark P2 (dev)». D2 intacto. RED→GREEN, 411/411 y 194/194.
> - En la sesión no hubo ninguna activación de P2/H17/Escenario y todos los armados fueron sin pausa y con foco. La causa de las activaciones originales sigue sin confirmar: no atribuirla.
> - Registro de los 111 gestos, separaciones, desviaciones declaradas, O1/O2 y 16 respuestas literales: correcto. Restauración P0 e458bfd5ce9e con tres archivos byte a byte: verificada.

### Observaciones de Claude, solo registro, sin cambio de código

- **C1.** Potencia por distancia: por primera vez en S1 las tres bandas se separan (medios 9/10, fuertes 9/10, saturación 10 %) y el PO responde «si» a intensidades voluntarias, consistencia y máximo. Es la pieza a conservar.
- **C2.** La fijación por quietud se dispara unos 130 ms después del último movimiento (mediana en todos los bloques). Cualquier duda al apuntar fija. Eso es coherente con el 30–40 % estimado por el PO y con «mecánico/pausado». Ajustar holdMs no resuelve ambas cosas a la vez.
- **C3.** Apuntado bajo: los abandonos de B0/B3/B5 terminan con el dedo bajo yMin del área útil. Apuntar bajo y tensar comparten el eje vertical, un conflicto de forma ya anticipado como riesgo de recorrido en PR #24. No es un defecto nuevo de D2.
- **C4.** El feedback de dos botones se usó de forma mixta incluso tras la aclaración. Una sesión futura necesitaría una tercera respuesta (por ejemplo «No fijó / falló»).
- **C5.** ParamSection.TrySet audita cada cambio de deslizador: posible volumen alto de registros durante un arrastre.
- **C6.** El test AcceptedB0ShowsCountdown... combina And/Or con precedencia ambigua; sin efecto en el resultado.

No se convierten C1/C2 en aceptación ni los juicios del PO en tasas medidas. No se toca D2, la auditoría ni el test para incorporar estas observaciones. No se recalcula ni repite la sesión aprobada.

### Autorización y ejecución del cierre

PO autorizó merge commit solo con head local/remoto fe78564 exactos, PR abierta/no borrador/sin conflictos, árboles limpios y sin añadir commits a la rama. Condiciones verificadas; **merge16b8a0060f6edd86fab92b0ae6d517d11cdd3205**, segundo padre **fe78564372ef9f38fbaea5cbc17d1fa5253bdea5**, primero956cfafbbfea6fef418fa24e6a3b0911663746be. Main sincronizada conorigin/main y limpia. Worktree retirado sin `--force` después de preservar siete paths ignorados en **Logs/archive-s1-sling-20261028** del checkout principal. Inventarios cotejados antes/después:32.480archivos/8.536.696.847bytes/35.372entradas incluidas carpetas/sin symlinks; ambos SHA-256 **37e1cdfc32b8bd3c831e4d83050c9d4685809ffbdeb42690d96b44732a99c4c5**. Artefactos/inventarios siguen ignorados; ramas remotas intactas.

A′ por defecto (`gesture.control=0`), preset S1-sling **7d1a61c39766 sin promover**, defaults **45c4e9af5c8f**,41heredados/proyección0d39e6016bd8 ySimulationKey166a96e319c7 intactos. Cierre no aceptaS1 niS1-sling. Sin suites/check-release/build/instalación/sesión/acceso al iPhone para cerrar.

### Siguiente entrega autorizada: propuesta documental de mecánica

Rama **firstshot/s1-mechanics-proposal-2**, desde main tras la fusión. [Documento](2026-10-28-first-shot-s1-mechanics-proposal-2.md): conservar potencia por distanciaC1, reemplazar fijación por quietudC2 y resolver conflicto de apuntado bajoC3; por alternativa, ventajas/riesgos/interacción150ms-P<0,60/RB6/evaluación. Recomendación de Codex marcada como tal, **sin elegir por el PO**. Usar111gestos4cec622 como evidencia, sin mezclar práctica/repeticiones ni inferir intención del feedback mixto. Si se hiciera análisis offline, predeclaración commiteada antes de calcular yJSON derivado declarado con originales intactos. **Esta entrega no hace análisis nuevo ni replay y no crea JSON derivado**; solo cita resultados aprobados.

Solo documento/AGENTS/aprobaciones. No código/tests/presets/defaults/spec/propuesta r2/planes/criterios/bandas/umbrales ni suites/check-release/builds/instalaciones/sesiones/acceso al iPhone. Las alternativas descritas son hipótesis; reabrir dos contactos antes descartados o cambiar política de dedos/r requiere decisión expresa futura, no se infiere aquí. No se autoriza ninguna evaluación física propuesta.

Entregar PR lista paraClaude **sin fusionar** y detenerse. **S1sin aceptar;S2–S4/T8no autorizados**. Perfil vigente hasta **2026-11-03T18:29:44Z**; no hay instalación autorizada.


## Revisión de PR #26 y decisión M2-4 — PO2026-10-28

Claude revisa head **05a3f778225922b9526e613fd501358e246a8fb9**: documento correcto y honesto, delta tresMarkdown/sin cálculos/producto; cierrePR25 verificado con merge16b8a00/segundo padrefe78564/árbol idéntico. Omisión recibida y corregida: literal de revalidación2 **«mano izquierda y derecha, ambas 2, me imagine juando y use ambas manos»**, relevante paraM2-2; no constituye prueba de dos pulgares simultáneos.

Preguntas respondidas por separado y literalesPO:

- Agarre en rally real: **«Las 2 manos sostienen pero 1 juega, o puede ser 1 y 1 pero es mas incomodo»**.
- Qué pulgar apunta y cuál da potencia si usa dos: **«Que se pueda elegir»**.
- Qué sacrificaría: **«Fluidez de rally»**; la aclaración recibida dice que prefiere fluidez antes que precisión. No invertirla por la formulación aislada de la pregunta.
- Dominio de apuntado inicial: **«tocaría el frontis pero habría que probar. Si es que no quizás ver en un panel mas abajo. Pero tendría que mirarlo primero»**; preferencia inicial, no decisión final.

**Decisión: investigar como mecánica principalM2-4 «apuntar al apoyar», propuesta porClaude**. Un contacto/Began absoluto define objetivo y fija sin quietud/levantar/reconocer velocidad o forma; distancia descendente actual desde ancla daP, retorno reduce/sin pico;Ended del mismo contacto esr. Cancelación candidata al volver a zona muerta ysoltar, relación conSwingP≈0 pendiente. M2-1contraste;M2-2/M2-3conservadas/documentadas/descartadas por ahora. No diseño aprobado ni código autorizado.

Desarrollar(a)dominio directo/panel ygeometría1206×2622,(b)sensibilidadmm/m frenteagananciarelativa,(c)cancelar-retocar yfluidez,(d)desviaciónspec§6.4sin editarla,(e)riesgo150ms/RB6/negativos,(f)dosmanossostienen-unpulgarjuega/manofuturaconfigurable,(g)B1–B5/tercerarespuesta«No fijó / falló»conautorizaciónpropia. Solo propuesta/aprobaciones/AGENTS en la misma rama/PR26. Cualquier cálculo offline predeclarado ycommiteadoantesdecalcular,JSONderivado declarado/originalesintactos;111trazasno contienenintenciónM2-4,no simular susresultados.

**Steering posterior delPO en la misma actualización:** comprobación informal conappinstalada/P0,sin bloque/datos/medición, sosteniendo con dos manos yjugando con unpulgar. Literal **«Si, llego comodo con el pulgar , llego a las 2 esquinas y a veces tapa el dedo otras veces no»**. Es sensación,no medición. A sobrefrontis referencia principal;Bpanelrespaldo documentado conmenosprofundidad, reemplazando la peticióninicial deigualprofundidad. Ubicación/ayuda finalsin elegirporPO. ObservaciónClaude: tirónalejadedo/destapamira,oclusiónsobre tododelapoyoinicial;compararhalo/lupasobreeldedo/sinayudaconventajasriesgosyB1,errorinicialyposibleoclusiónT1–T5,sintamañosnuevos.

[Predeclaración geométrica§10](2026-10-28-first-shot-s1-mechanics-proposal-2.md): salidaJSON **embebida** M2-4-geometry-v1 y dosfigurasMarkdown enel documento, sin archivoextra yantesdecalcular. No replay ni análisisdeintención de111trazas,ni nuevos datos físicos. Producto/spec/planes/criterios/bandas/umbrales intactos;sin código/tests/presets/defaults/suites/check-release/build/instalación/sesión/accesoaliPhone. EntregarPR26lista paraClaude sinfusionar ydetenerse. S1sinaceptar/S2–S4/T8sin autorizar;perfilhasta2026-11-03T18:29:44Z,ninguna instalaciónautorizada.

Ampliación de predeclaración antes del cálculo refinado: rootUI389,10×845,95 del fixturePlayMode aprobado/fórmulaSlingInputArea para inferir borde nominalDevBar, separado de límite útil/margen. Publicar ambas cotas y su carácter conservador/optimista, sin mover límites ni suponer medidas físicas. MismoJSON embebido y tresMarkdown, sin archivoextra ni replay.


### Entrega documental M2-4 y resultados geométricos predeclarados

Predeclaración **60c5c288f8cc7888db89b27455f066bbe38b49be** anterior al cálculo pinhole/sensibilidad/recorrido; ampliación **9d2a005** anterior a inferir borde nominalDevBar desde rootdel fixture histórico. Resultados y JSON **M2-4-geometry-v1 embebidos** en [§10 de la propuesta](2026-10-28-first-shot-s1-mechanics-proposal-2.md), con figuras estáticasA/B; no se añadió archivoSVG/JSON ni se ejecutó Unity. El refinamiento distingue reserva de entrada de bordeDevBar, sin mover límites ni usar el resultado para modificar35mm. ArchivoXML de referencia cotejado con hashpublicado; originales intactos.

A es referencia principal por steeringdelPO, no elección final de diseño. Su punto de tirón completo nominal cae dentro de reserva pero antes deDevBar; no acredita yema/alcance físico. B queda de respaldo: altura máxima optimista muy limitada, no dimensión seleccionada ni viabilidad confirmada. Mapeo/sensibilidad comparados sin alterar ganancia; no se predice precisión del pulgar ni resultados deM2-4. Oclusión inicial/destape, halo/lupa/sin ayuda y costos de cancelar/retocar expuestos con recomendaciones deCodex etiquetadas, **sin elección de ayuda por elPO**. PolíticaSwingP≈0/cancelación/N2 y desviación de spec§6.4 siguen pendientes antes de cualquier TDD; sin editar spec. B1–B5 y tercerarespuesta«No fijó / falló» solo propuestas y necesitan autorización propia.

Las111trazas no se recorren para simularM2-4; solo se citan resultados/entrada geométrica ya existentes. Comprobación informal delPO conservada como sensación, sin datos/medición ni intervención deCodex en el teléfono. Misma rama/PR26 y delta desde05a3f77 exactamente propuesta/aprobaciones/AGENTS. Sin producto/tests/presets/defaults/spec/planes/criterios/bandas/umbrales,suites/check-release/build/instalación/sesión/acceso al iPhone. Entregar lista paraClaude,sin fusionar ydetenerse. S1sin aceptar/S2–S4/T8sin autorizar;perfilhasta2026-11-03T18:29:44Z,sin instalación autorizada.


## Aprobación de Claude y fusión condicionada de PR #26 — PO2026-10-28

Claude aprobó head **f2068ac1e6948c5746005c5aa7ec71ee97dcd7cf** con tres correcciones documentales menores. Verificó delta de tresMarkdown sin producto, predeclaraciones60c5c28/9d2a005 anteriores a resultados y proyección recalculada independientemente coincidente enT1–T5, polígono/tiny1305,37,35mm=633,86px,holgurasA−8,21/+84,77px,cotasB−41,63/+51,35px y sensibilidades4,45–4,80mm/m. **No se recalcula ni se ejecuta ningún análisis para esta corrección.**

Correcciones aplicadas únicamente a texto:

1. Predeclaración marcada como histórica de60c5c28; resultados publicados calculados después de9d2a005.
2. FigurasA/B con contorno cualitativo más ancho enout(x≈317–889) que en suelo(x≈353–853), sin cambiar coordenadas/JSON.
3. **Conversión de unidades señalada por Claude de cifras ya publicadas, no análisis nuevo**, con s=25,4/460mm/px: ancho suelo≈27,6mm/out≈31,6mm;tin→out≈18,0mm;T1–T3≈9,1mm,T1–T4≈11,2mm,T1–T2≈14,0mm;mira mínima60px≈3,3mm. Consecuencia deClaude: pocos centímetros de frontis y mira actual menor que una yema; no medición de la yema delPO. Riesgo principalAes precisión del apoyo inicial, no solo oclusión; sin proponer cámara/zoom ni cambiar tamaños/umbrales.

POautoriza fusión **condicionada**: delta desde f2068ac limitado a propuesta/aprobaciones/AGENTS, head corregido local/remoto exacto,PRabierta/no borrador/sin conflictos yárboleslimpios. Mergecommit con segundo padre=headcorregido; sincronizarmain;retirarworktree sin `--force`,preservandoignorados. **El cierre efectivo se registrará en la siguiente entrega documental, sin commit aparte enmain**; esta entrada no afirma que elmerge ya haya ocurrido. Después detenerse, sin iniciar otra entrega.

**Diseño/TDD deM2-4 no autorizados.** ElPOdecidirá dominio, ayuda de oclusión, cancelaciónC-A/C-B, política de regióninválida, desviación§6.4 y riesgo por fases. S1sin aceptar;S2–S4/T8sin autorizar. Solo texto,sin producto/cálculos nuevos/suites/check-release/build/instalación/sesión/accesoaliPhone. Perfilvigentehasta2026-11-03T18:29:44Z;ninguna instalación autorizada.


## Cierre de PR #26 verificado por Claude/PO — registro en la nueva entrega, 2026-10-28

Claude confirmó el cierre: **merge bf607cdc235ac381799d5bbbb1a1633ca394dfe9**, segundo padre exactamente **e452cc26ced5d404542fca7d96274b16cb2c91e1**, árbol idéntico. Delta desde f2068ac limitado a propuesta/aprobaciones/AGENTS con las tres correcciones aplicadas. Main sincronizada y limpia; no quedaba worktree extra al iniciar esta nueva entrega. Se registran esos hechos aquí, dentro de la nueva PR documental, **sin commit aparte en main**.

Retiro anterior sin `--force`, ramas remotas conservadas. Ignorados:0paths/0archivos/0bytes, inventarios vacíos cotejados en `Logs/archive-s1-mechanics-proposal-2-20261028` del checkout principal; SHA-256 ca3d163bab055381827226140568f3bef7eaac187cebd76878e0b63e9e442356. El archivador de la app rechazó el checkout por considerarlo principal del chat; Git confirmó un worktree adicional limpio y se retiró con comando normal desde main. No se disimula esa diferencia ni se afirma que la app lo archivara.

## Decisiones del PO para el diseño documental M2-4 — 2026-10-28

Elegidas una por una sobre la propuesta§10; se conserva el texto comunicado:

1. **Spec v0.3 §6.4:** se acepta la desviación (objetivo absoluto al apoyar, en lugar de arrastre relativo) **solo para el experimento M2-4**. La spec queda intacta y A′ sigue siendo el control por defecto.
2. **Cancelación: C-A.** Toda suelta dentro de la zona muerta cancela o abandona según N2 y la fase. Nunca hay un golpe accidental al corregir. Para corregir: volver, soltar y volver a apoyar con un Began nuevo. Las cadenas cancelar→reapoyar se registran vinculadas, sin reemplazar el primer fallo. Se acepta como costo conocido que no exista un golpe intencional con P≈0 sin tirón.
3. **Apoyo fuera de la zona válida (tin/out):** mira roja y cierre como inválido sin Swing. No se recorta al vecino legal ni la línea pasa a ser objetivo válido.
4. **Oclusión:** la primera prueba va **sin ayuda adicional** (iii) y sirve de referencia, midiendo el error del Began. Halo y lupa solo en una prueba posterior, si los datos lo justifican y con orden predeclarado.
5. **Riesgo 150 ms: por fases.**
   - Antes de un tirón deliberado fuera de la zona muerta (apoyo, duda, pausa de 150 ms, tap, temblor), P=0 y nunca hay Swing; con C-A, soltar cancela.
   - Un tirón deliberado que se aguanta y se suelta conserva P por distancia, aunque sea ≥0,60. Se acepta explícitamente como intencional, igual que RB4 de S1-sling.
   - La regla original de 150 ms/P<0,60 y sus casos siguen intactos para el método B de A′.
   - No se añade decaimiento ni límite de tiempo al aguantar.
   - Riesgo nuevo que debe cubrirse con casos negativos: un apoyo accidental que después resbala hacia abajo y carga potencia.

Además, dominio **A(frontis directo) referencia principal y B respaldo**. Ubicación final después de una prueba física autorizada. Las decisiones no autorizan ahora esa prueba ni halo/lupa, código o TDD.

### Alcance autorizado y contenido de esta entrega

Rama/worktree nuevo desde main **bf607cd**: `firstshot/s1-m2-4-design`. El gestor de worktrees no resolvió el ref local; Git lo verificó y creó el worktree desde ese commit exacto. [Diseño documental M2-4](2026-10-28-first-shot-s1-m2-4-design.md): mapeo absoluto/proyección capturada, zona muerta/N2/Tap/prioridad de cierres, dedo/r, feedback, trazabilidad/O2/cadenas, negativos predeclarados, Tarea prevista1.2c sin renumerar y B1–B5 adaptados a C-A.

Las precisiones operativas de Codex (zona muerta descendente coherente conP0, precedencia sistema→inválido→N2→C-A→Swing, semántica de tres respuestas y enlace de reapoyos) se marcan para aprobación del diseño, **sin atribuirlas como nuevas elecciones ya hechas por el PO**. «No fijó / falló» es propuesta de interfaz/protocolo, no implementada. La fase observada fuera de zona muerta no demuestra voluntad; resbalón accidental queda como caso negativo/riesgo visible, no seguridad ficticia ni P0 forzado.

**Adendas nuevas D3 aditivas** a propuestaFirstShotr2 yplan, para revisión; todo el texto anterior se conserva íntegro. Valores de referencia heredados de potencia marcados [EXPERIMENTO], sin cambiar umbrales/criterios/bandas ni elegir nuevos parámetros o selector/preset. B1 deja de exigir golpe al «soltar suave»: apoyo y retirada sin carga, Tap/C-A esperados, errorBegan/fijación independientes del cierre sinSwing.

Solo documentación: AGENTS/aprobaciones/diseño nuevo/propuesta r2/plan con añadidos. Sin código/tests/presets/defaults/specv0.3, suites/check-release/build/instalación/sesión/acceso al iPhone ni replay/cálculos nuevos sobre111trazas. A′ por defecto/defaults45c4e9af5c8f/S1-sling7d1a61c39766/41heredados/SimulationKey intactos.

Entregar **PR lista para Claude, sin fusionar y detenerse**. S1 sigue sin aceptar; S2–S4/T8 siguen sin autorizar. Perfil válido hasta **2026-11-03T18:29:44Z**; ninguna instalación autorizada.


## PR #27 — aprobación de Claude con cuatro correcciones y fusión condicionada, PO2026-10-28

Head revisado **e415bcb92f3ab8fe1050621d164a06953cd6b6ba**. Claude verificó cincoMarkdown/basebf607cd, D3 estrictamente aditivas, specv0.3/Assets/ProjectSettings/Packages/scripts intactos y cinco decisiones literales delPO. Confirmó los valores existentes2/35mm, comparacionesTapestrictas«<», dosrespuestas de`SlingProtocol.Respond(bool)`, ausencia de cierres nuevos en`GestureOutcome` y documentación correcta de voluntad no observable(N04). Sin nueva medición/replay/cálculo.

Correcciones aplicadas solo al diseño, D3, aprobaciones yAGENTS:

1. **Texto visible propuesto: «Apoyo sin querer»**, significado de apoyo no querido, correspondencia interna con la respuesta anterior «Se fijó antes de tiempo» (`premature=true`). **El PO aprobará el texto al autorizar implementación**, no se cambia UI ahora. Registrar uso mixto anterior en B1/B3 de S1-sling; no reetiquetar los flags ni convertirlos en tasa de intención.
2. **C-A en lenguaje llano:** para cancelar, volver a la altura donde se apoyó o por encima y soltar (d≤zona muerta descendente). Incluido en feedback propuesto§6, explicación previa§10 yB0; no círculo ni salida superior/lateral.
3. **N04b:** deslizamiento mayormente lateral, componente descendente apenas mayor que zona muerta y máximo desplazamiento≥3mm puede terminar en Swing con P positivo muy pequeño(P≈0) no buscado. Riesgo explícito sin umbral/filtro/valor nuevo; no es unP=0sin tirón ni se atribuye voluntad por distancia.
4. **§1, propuesta deCodex explícita:** rechazo con motivo de Began fuera del muro frontal(paredeslaterales/suelo/rayoinválido), distinto de mira roja/cierre inválido tin/out decidido por elPO.

**Fusión autorizada solo bajo condiciones:** delta desdee415bcb exactamente esos cincoMarkdown; headcorregido local/remoto coincidente, PRabierta/no borrador/sinconflictos yárboleslimpios. Mergecommit con segundo padre=headcorregido; sincronizarmain yretirarworktree sin `--force`, preservandoignorados. Elcierre efectivo se registrará **en la siguiente entrega documental, sin commit aparte enmain**; no se afirma que elmerge haya ocurrido enestaentrada ni se inicia esa entregaahora.

Después detenerse. Implementación/TDD1.2c necesitan autorización separada, igual que build/instalación/sesión. S1sigue sinaceptar;S2–S4/T8sin autorizar. Sin producto/tests/presets/defaults/specv0.3/suites/check-release/build/instalación/sesión/accesoaliPhone ni análisisnuevo de111trazas para estas correcciones.


## 2026-10-28 — cierre PR #27 y autorización de implementación M2-4

Claude/PO verificaron merge **403d213a5aecfbee0752870499bb3d7bf408cf78**, segundo padre **70576df8458b39e822abfa9b63e6a7901f03c9f9**, mismo árbol; delta desde e415bcb un commit limitado a cinco Markdown con cuatro correcciones. Main sincronizada/limpia, sin worktree extra; retiro sin `--force`, inventarios vacíos cotejados en Logs/archive-s1-m2-4-design-20261028, ramas remotas conservadas. Registro en esta entrega, sin commit aparte en main.

El PO aprueba diseño/D3 y las propuestas operativas: zona muerta descendente; sistema→inválido→N2→C-A→Swing; rechazo con motivo fuera de muro frontal; vínculo de reapoyos; tres respuestas **OK**, **Apoyo sin querer** (correspondencia histórica `premature=true`), **No fijó / falló**. N04/N04b aceptados como riesgos abiertos, no resueltos. Autoriza tarea1.2c-0…5 con TDD válido, regresión fresca, check-release dev/prod gitDirty=false, build dev Release ARM64/no Development, instalación tras verificar perfil incorporado y respaldo/cotejo de tres archivos; P0/hash extendido observado. Preset M2-4 separado sin promover, A′ por defecto/S1-sling conservado/41heredados/SimulationKey y2/35/N2 intactos.

Autoriza sesión izquierda/un pulgar/sin ayuda, un bloque B0→B5 cada vez y precheck hash/pendientes/Jugando por bloque; cuenta atrás confirmada antes del primer gesto B0; comprensión de cancelación antes de B1 y ejemplos de las tres respuestas. Cierres/O2/error Began/cadenas registrados, sin sustitución de intentos; 16 preguntas literales al final. P0 restaurado en proceso nuevo/cero pendientes/cotejo byte a byte. Ante defecto o falta de comprensión registrar/restaurar/detener, sin ajustar parámetros. Sin spec/criterios/bandas/umbrales/halo/lupa/cámara/zoom/dominioB/contacto real ni S2–S4/T8. PR única para Claude sin fusionar; S1 sin aceptar. Perfil registrado hasta2026-11-03T18:29:44Z, verificar vigencia efectiva antes de instalar.


### Avance técnico M2-4 para revisión, no aceptación

Fuenteae1b423, TDD por etapas en informe, regresión fresca441/441EditMode y207/207PlayMode sinCS. Selector2, presetS1-M2-4/hash1c8e598b099b separado, defaults45c4e9af5c8f/85claves inalterados, heredados41/SimulationKey intactos. Respaldo nuevo3archivos cotejado. Release/build/perfil efectivo/instalación/sesión/restauración aún pendientes; no hay resultado físico ni aceptaciónS1. N04/N04b siguen riesgos abiertos.


### Instalación y preparación ejecutadas, B0 pendiente del PO

Build0f8002d dev Release ARM64/no Development; checks dev/prod limpios, perfil incorporado hasta2026-11-03T18:29:44Z/firma verificados antes de instalar.3archivos cotejados tras instalación. ArranqueP0/e458bfd5ce9e/cero pendientes observado; S1-M2-4/1c8e598b099b cargado temporal, last-active repuesto/leídoP0 antes de aplicar y relanzar23:31:48.353735Z;repeat0/cero pendientes/Jugando/DEVcerrado/Listo/protección experimentos y cotejo3archivos verificados. Instrucción soloB0 con confirmación de cuenta atrás antes de primer gesto, izquierda/unpulgar120s y cancelación guiada C-A, noB1; resultado/comprensión pendientes, no inferidos. Restauración final pendiente al cerrar. Sin nuevos cambios de producto, parámetros ni umbrales. No aceptaciónS1.


### Devolución B0 M2-4 y preferencia del PO (sin nueva decisión de mecánica)

Literal completo en informe/evidencia B0; PO declara apuntar/cargar/cancelar durante2min y preferencia inicial de tensado arriba/adelante, después acostumbrado a bajar. Sensación, no cambio de autorización ni aceptación. Se mantiene el tensado descendente,2/35/N2/bandas/umbrales. Log B0 completo120,01248675s/104intentos/5retornosC-A y guiada registrada/hash1c8e598b099b/cero pendientes; ninguna tanda posterior. Se conserva desviación de no recibir confirmación previa de cuenta atrás porchat; se pregunta retrospectivamente y sobre comprensión/voluntariedad C-A, sin repetir ni inferir. Falta triada antes de B1, un bloque cada vez.3archivos byte iguales, M2-4 temporal activo/selección persistidaP0/restauración final pendiente.


### Gate B0 y aclaraciones literales, sin nueva decisión de parámetros

Cuenta atrás antes del primer gesto/Completado: PO **«Si, lo vi, preferí empezar de una después de leer el mensaje»**. Cancelación voluntaria y retorno comprendido: **«fueron todas voluntarias»**. Se conserva la desviación de no confirmar previamente porchat, sin repetir B0 ni recalificar raw. Gate de B0 confirmado; faltan respuestas a ejemplos de la triada antes de B1. Preferencia de dirección arriba/adelante no modifica el experimento descendente. Ningún bloque nuevo solicitado aún; código/configuración intactos. M2-4 temporal activo/restauración final pendiente.


### Ejemplos de tres respuestas comprendidos antes de B1

PO, por separado: «OK» al ver rombo querido aunque lejos del objetivo; «Apoyo sin querer» al apoyo accidental; «No fijó / falló» si no apareció/se identificó rombo o hubo fallo operativo. Coinciden con significados aprobados, sin inferir significado de futuros flags ni cambiar instrumento/criterios. Se solicita soloB1 con precheck1c8e598b099b/0pendientes/Jugando, pulgar izquierdo, diez apoyosT1–T5dosveces/retirada sin carga/respuesta explícita/todoscierres/no sustitución. B1 y su precheck pendientes; noB2 hasta revisión. Sin código ni parámetros nuevos; restauración final pendiente.


### B1 completo, desviación de tarea y siguiente solo B2suaves

10intentos/10respuestasOK/597muestras con1c8e598b099b/0pendientes/sinpausa al armar;2abandonos+3Swing+5retornos. PO **«intente acertar, en algunos bajaba el pulgar para cargar, retiraba sin cargar y tambien retiraba para destapar la mira y comprobarla»**: buscóT, pero B1 sin carga no aislado; no repetir/reclasificar ni contarcomoB2suave. Paso posterior pausó;PO **«probablemente fue accidental»**, juicio probabilístico/no causa confirmada. SoloB2-soft solicitado trasContinuar/verificarhash/0pendientes/Jugando, diez suavesbuscandoT3/pulgarizq/todoscierres, sin medios/fuertes/B3 hasta revisión. Sin cambios de producto/dirección/parámetros/criterios;restauración final pendiente, S1sin aceptar.


### 2026-10-29 — reconexión pedida por el PO antes de empezar B2

Literal **«Prueba conectarlo de nuevo, para empezar la prueba. Tuve que reiniciar la app»**. Se verificó B2 no armado/sin intentos y conservación de B0/B1; no repetición/sustitución. Reconectada misma0f8002d dev, P0/e458bfd5ce9e/0pendientes previsto tras reinicio. RepreparadoS1-M2-4 temporal1c8e598b099b, last-activeP0 repuesto/leído antes de aplicar/relanzar10:45:43.436823Z;0pendientes/repeat0/Jugando/DEVcerrado/protección/cotejo3archivos. Tramo nuevo/otro día y proceso, sin comparabilidad humana causal ni continuidad ficticia. SoloB2-soft solicitado de nuevo porque no había empezado;sin suites/build/instalación/código/parametrización nueva. Cierre/restauración pendientes, S1sin aceptar.


### 2026-10-29 — B2 suave/medio conservados, siguiente solo fuerte

10/10Swing en banda suave y10/10media, rangos5,48–23,89% y41,96–58,03%,0saturación/conservadoras; hash1c8e598b099b/0pendientes/armados sinpausa/foco. Error de apoyo separado, sin aceptar precisión/S1 ni cambiar criterios. SuavePO **«Listo! todo bien, aputnaba al cuadrado de al medio»**; medio **«listo»**. Un preview adicional fuera de tanda preservado/sin sustitución. SoloB2-strong solicitado después de revisar medio, diezT3/pulgarizq/tirónmayor,precheck y todoscierres;noB3/B4/B5 aún.3archivos intactos/sin código/configuración/build nuevos;restauración final pendiente.

### 2026-10-29 — B2 fuerte conservado con advertencia de saturación

PO **«listo»**; 10/10 fuertes en banda, P73,25–100%/mediana99,51%. **Saturación5/10=50%, supera advertencia20% intacta**; no ajustes/repetición/promoción/aceptación inferida. B2 total30/30 en banda/1164muestras, misma configuración/cero pendientes/sin pausa/con foco y tres archivos iguales. Error Began separado de P. Tras revisar fuerte se solicita solo B3, diez medios T1/T2/pulgarizquierdo/triada/precheck/todoscierres; no B4/B5 aún. Sin código/parámetros/criterios nuevos, restauración final pendiente, S1 sin aceptar.

### 2026-10-30 — repreparación solicitada por el PO antes de B3

Salida/reinicio y acceso a Duplicación resuelto por PO, literales en el informe. B3 no iniciado; B0/B1/B2 no se repiten. Misma app0f8002d dev sin reinstalar; S1-M2-4 temporal1c8e598b099b/0pendientes/repeat0/Jugando/DEVcerrado/Listo y protección observados; last-activeP0 repuesto antes de aplicar/relanzar,3archivos byte iguales al respaldo/456fuentes intactas. SoloB3 solicitado con triada y pulgarizquierdo; otro día/proceso sin continuidad humana inferida. Advertencia de saturación fuerte50% conservada sin ajustes. Restauración final pendiente, PR28sin fusionar/S1sin aceptar; ninguna autorización nueva de código, instalación o S2–S4/T8.

### 2026-10-30 — B3 conservado, siguiente solo B4

PO «listo». Diez intentosT1/T2/diezOK/diezSwing en media,hash1c8e598b099b/0pendientes/armado sinpausa-foco;443muestras/0O2/enlaces/saturación. Error de apoyo separado sin criterio nuevo ni aceptación inferida. Tres archivos byte iguales/456fuentes intactas. SoloB4/cinco cancelacionesC-A/pulgarizquierdo/precheck/todoscierres/sin repetición solicitado;noB5 todavía. Sin cambios de producto/parámetros ni sesiones extra, restauración final pendiente/S1sin aceptar.

### 2026-10-30 — B4/B5 completados por el PO, cierre pendiente

Literal íntegro en informe/JSON: B4 «Ningun problema, todas las cancelaciones voluntarias»; B5 «Todos los tiros aputando al cuadrado con ritmo comodo». Se conserva B5 adelantado antes de recuperación/revisiónB4 y solicitud separada como desviación, sin repetición/sustitución. B4 cuatro retornos y un abandono sin carga; B5 diezSwing/3enbandapedida, intención de variar potencia pendiente de aclaración. Hash/armados/0pendientes correctos, O2cero,3archivos byte iguales/456fuentes intactas.16preguntas originales separadas solicitadas; restauraciónP0/cotejo final pendientes. No ajustes/producto nuevo ni aceptaciónS1; PR28sin fusionar.

### 2026-10-30 — cierre de M2-4 para revisión Claude, sin aceptación S1

Las16respuestas del PO se conservan literalmente y por separado en informe/JSON. Declara colocar mira/apuntar bajo/estabilidad/intensidades/distancia/máximo/cancelación positivamente; sin recuerdo del número de fijaciones involuntarias, no inferir cero. Dice «mas natural que antes, pero aun sigue mecanico. Lo que si tendría que probar en partido real» y «habría que probar para ver que tan como es» sobre rally: no validación ni autorización de partido/S2. Preferencia inicial de dirección sin cambios. Aclaración B5 precisa: **«No, me concentré en apuntar con ritmo cómodo»**; su secuencia instrumental de intensidadesNotEvaluated como tarea voluntaria, conservando los3/10sin reclasificar. B2y advertencia50%saturación fuerte permanecen separados e intactos.

PO dejó disponible el teléfono tras posible reinicio. Restauración **en proceso nuevo2026-10-30T11:54:09.7179370Z/P0-calib-1/e458bfd5ce9e/cero pendientes/Jugando**, confirmada registro/DEV; DEVcerrado y tres archivos directamente byte a byte iguales al respaldo, sin nuevo bloque/gestoM2-4 ni instalación.169intentos programados más1extra preservados, desviaciones/reinicios registrados; empaquetadoB2 corregido incorporando últimos registros originales emitidos tras fin, resúmenes sin cambios.456fuentes intactas y evidencia histórica separada. Regresión441/441y207/207/check-release limpio/build previos, sin repetición para cierre documental.

Entrega PR#28 lista paraClaude **sin fusionar ni aceptarS1**; candidatoS1-M2-4/1c8e598b099b sin promover/A′0por defecto/defaults45c4e9af5c8f y valores heredados intactos. N04/N04b abiertos. No seguir con producto, sesiones oS2–S4/T8; próxima decisión corresponde alPO trasClaude. Perfilvigente hasta2026-11-03T18:29:44Z/comprobar efectivamente antes de instalación futura. [Informe](2026-10-28-first-shot-s1-m2-4-implementation.md), [respuestas](../validation/evidence/2026-10-30-m24-po-answers.json), [restauración](../validation/evidence/2026-10-30-m24-final-restore.json).


### 2026-10-30 — revisión Claude de PR #28 y aceptación explícita de S1 por el PO

Claude aprueba el head **af281e08e206da0d134811411de2f9cb42fd1b77** con una corrección documental enAGENTS.md: la autorización de implementación1.2c pasa aHistórico y solo queda vigente la entrada de entrega cerrada. Verificó código/diseño, casosN01–N13/N04b/fronteras exactas, clasificador tin/out idéntico aAimReticleView/radio0,01, presetS1-M2-4 distinto de defaults solo en gesture.control2 y ausencia de cambios en DefaultPreset/EffectiveConfig/SlingModel/ProjectSettings/Packages/scripts/docs/superpowers desde403d213. Recalculó independientemente los datos crudosM24 y confirmó conteos/cierres/hashes/pendientes/P/saturación/bandas/error mediano. Registro de esa revisión, sin recalcular aquí ni modificar datos o producto.

**Decisión literal del PO:**

> S1 aceptado con M2-4 como control para avanzar hacia S2.
>
> Esta aceptación se basa en:
> - Bandas de B2: 30/30, sin solapamiento.
> - B3: 10/10.
> - B4: cancelaciones voluntarias.
> - Las 16 respuestas literales del PO.
>
> Siguen abiertos y se observarán en S2 con golpe real:
> - La saturación.
> - N04/N04b.
> - La sensación «aún mecánica» y el ritmo de rally.
> - La preferencia de dirección del tirón.
>
> La aceptación no autoriza código de S2, promover M2-4 a default ni cambiar `pullFullMm`. Eso irá en una entrega propia con autorización del PO.

Esta decisión explícita sustituye el estado previo de S1 sin aceptar; **no reinterpreta los datos anteriores**, sus desviaciones, el NotEvaluated de intensidadesB5, la advertencia de saturación o las16respuestas literales.

Observaciones de Claude, sin cambios de código:

1. **Saturación fuerte del 50 %**, por encima de la advertencia del 20 %. Con `pullFullMm=35`, la mitad de los golpes fuertes saturan y se pierde la diferencia entre fuerte y máximo. Cualquier ajuste requiere una decisión del PO.
2. **Error del Began:** una mediana de 0,15–0,30 m equivale a unos 1 mm en pantalla con la sensibilidad de la PR #26, frente a unos 2 m entre objetivos. Es una descripción, sin umbral.
3. **Volumen de auditoría:** 414 registros filtrados (205 con DEV abierto) y 330 ignorados para 169 gestos. Es aceptable con la retención actual, pero conviene vigilarlo, igual que en C6.
4. **N04/N04b:** no tienen evaluación física. Que el PO responda «no recuerdo» no equivale a cero fijaciones involuntarias.
5. **Dirección del tirón:** el PO prefiere tirar hacia arriba o adelante. Queda registrado, sin cambios.

**Autorización de fusión condicionada:** delta desdeaf281e0 soloAGENTS.md y este registro; head corregido idéntico local/remoto, PRabierta/no borrador/sin conflictos y árboles limpios. Mergecommit con segundo padre=head corregido; sincronizar main y retirar worktree sinforce preservando ignorados (Logs/evidence, builds/exports). Ramas remotas conservadas. A′ sigue por defecto yS1-M2-4 no se promueve. Sin suites/check-release/build/instalación/sesión/código nuevos para el cierre. **El cierre efectivo se registrará en la siguiente entrega, sin commit aparte en main. Después detenerse: S2–S4/T8 no autorizados.**


### 2026-10-30 — cierre efectivo de PR #28, dentro de la entrega D4

Claude comunica verificación de cierre y se contrasta en Git: **merge1b0ba27c09ef61a014cee871dd306894243750d4**, segundo padre **5ef914e816b41928cc3b35be180f00182ac1046b**, árbol idéntico al head corregido. Delta desdeaf281e08e206da0d134811411de2f9cb42fd1b77 exactamenteAGENTS.md y este registro; queda una única entrada vigente. Antes de fusionar se comprobaron head local/remoto, PRabierta/no borrador/sin conflictos y árboles limpios. Main sincronizada/limpia; worktreeS1 retirado sinforce, siete paths ignorados preservados en `Logs/archive-s1-m2-4-20261030` con inventario/hashes iguales:31.636archivos/8.339.573.965bytes. Ramas remotas intactas; sin commit aparte enmain. Auditoría local ignorada `closure-audit.json`. No se ejecutaron suites/builds/instalaciones/sesiones para el cierre. La fusión conservó A′ por defecto yS1-M2-4 sin promover. La aceptación deS1 permanece con sus condiciones literales; no reinterpreta saturación50%, desviaciones,16respuestas o el NotEvaluated deB5.

### 2026-10-30 — decisiones del PO para preparación documental de S2

Decisiones literales:

> 1. M2-4 pasa a ser el control por defecto al preparar S2. A′ y S1-sling siguen en el código como alternativas configurables.
> 2. Saturación: se observa en S2 con golpe real. `pullFullMm=35` no se toca.
> 3. Alcance de esta entrega: solo plan/adenda S2, sin código. La implementación de S2 requiere otra autorización.

Se concreta en **D4 aditiva** del [plan](../superpowers/plans/2026-10-25-squash-clash-first-shot.md) y la [propuesta](../superpowers/specs/2026-10-24-first-shot-design.md), rama/worktree nueva desde1b0ba27. No se reescribe texto previo ni specv0.3. La tarea futura2.0 hará la promoción bajo TDD y registrará hashes efectivos reales de defaults/P0, conservando41heredados/SimulationKey; **no se cambia DefaultPreset ni se calcula un hashS2 en esta entrega**. La decisión nueva de default rige la siguiente implementación, no retoca la fusiónS1 ni los presets históricos.

D4 propone el mapeo aShotRequest con intenciónBegan/PdelEnded/roriginal del mismo dedo; tabla de cierres/consumo e identidad; análisis de vuelo frente a duración observada deS1 sin medir ni inventar tiemposSL; seguimiento con objetivo mundo fijo; negativos/TDD previstos y2.7con abiertosS1, un bloque cada vez/hash/triada/respuestas literales/restauraciónP0. La concreción operativa de intento por cada cierre se somete conD4 a revisión, no se atribuye como decisión adicional ya tomada por elPO. La duración del gesto no modificaA/V/K ni la regla de riesgo por fases; N04/N04b y preferencia de tirón se conservan abiertos.

Alcance comprobable: únicamenteAGENTS.md, este registro y las dos adendasMarkdown. Sin código/tests/presets/defaults, specv0.3/criterios/bandas/umbrales, suites/check-release/build/instalación/sesión/acceso al iPhone ni replay/análisis nuevos de trazas. Baselines45c4e9af5c8f/e458bfd5ce9e y41heredados/SimulationKey intactos. **Entregar PR lista paraClaude, sin fusionar y detenerse. S1aceptado; códigoS2–S4/T8 sigue sin autorizar.** Perfilactualvence2026-11-03T18:29:44Z, no hay instalación autorizada.


### 2026-10-30 — cierre PR #29 y autorización S2 fase1

Merge**8dd142dff2fcdc5d52871ac3c89733b4aff88fd6**, segundo padre**1bae4615e8bec0e9b12624e73ce240a830638dae**/mismo árbol, verificado porClaude yGit. Heads exactos,PRabierta/no borrador/sinconflictos/árboleslimpios antes de fusionar;main sincronizada/limpia,worktree retirado sinforce/noignorados,inventarios vacíos enLogs/archive-s2-m24-plan-20261030,ramas remotas conservadas. Este registro va dentro de la nueva entrega, sincommitaparte enmain. Retoque N04 b→N04b efectuado enplan/propuesta, sin otro cambio de esa adenda.

**Decisión literal del PO — tabla D4.2 aprobada tal como está en el plan:**

> - InvalidAim (incluido tin/out), Tap, AbandonedNoPull, CanceledReturnToDeadZone, Rejected y SystemCanceled no golpean ni consumen el intento. Se puede volver a apoyar si queda tiempo en la recepción.
> - Un Swing aceptado consume el intento de forma atómica aunque sea temprano, tardío, quede fuera de V o no alcance la pelota.
> - Un contacto sin Ended al cerrar la recepción no dispara. Se registra como NoOrder o como el caso que corresponda.
> - Las órdenes duplicadas, las de otro lanzamiento y las que llegan con el intento ya consumido se descartan con su motivo.
> - Esto deja de ser propuesta de Codex: pasa a decisión del PO y se registra literalmente.

**Decisión literal del PO — bloques2.7:** «no se deciden ahora. Codex los predeclara por escrito en la autorización de la fase 2 y se aprueban antes de cualquier sesión».

Autoriza tareas2.0–2.6/D4/M2S2-N01–N12 enorden,TDDcompilable/REDporaserción→GREEN.2.0promueveúnicamentecontrol2/fixturas explícitas/overlayP041claves sin reescribirarchivos/hashreales/proyecciones41ySimulationKeyintactas/2-35mm/N2estrictointactos.2.2registra vueloenrelojreal/frontis-bote/K/T*; incompatibilidad detiempo se registra ydetiene,sinreajustarvelocidad,ventanas oSL. Cierreautorizado:regresión completafresca sinCS/restauracióntransitorios/check-release limpio dev-prod gitDirtyfalse/informe nuevo/PRlistaClaude sinfusionar. SinXcodebuild/instalación/accesoiPhone/sesión2.7 ni recorridoprod físico;spec/criterios/bandas/umbrales/dirección/pullFullMm intactos,sinhalo/lupa/cámara-zoom/dominioB/decaimiento/filtrosN04/rival-rally/S3–S4/T8. Ante defecto/incompatibilidadregistrar ydetener,sinajustar. No revisión deperfilahora;vence2026-11-03T18:29:44Z. S1aceptado;fase2posterior requiereotraautorización.


### 2026-10-30 — aplicación de la parada por incompatibilidad de tiempo S2-TIME-01

Se aplica la condición de la autorización de fase1, sin una decisión nueva delPO: **registrar y detenerse, sin ajustar**.2.0completada/2.1Corecompletado;2.2focal14/14 calcula las seis recetas iniciales predeclaradas [EXPERIMENTO] encommit0c638da antes de evaluarlas. SL5/SL6 tienenT*1,448718s/Kout1,694601s reales. Comparar con la mediana existenteB2mediosS1=1,662776708s implica, paraV100ycompensación0,un Began antes de lanzamiento;T3puro deja≈31,8ms para empezar un gesto deesa duración. Es análisis determinista de editor más una duración ya medida citada,no nueva sesión,ni replay de trazasS1,ni imposibilidad general para todoslos gestos. No se inventa una duración mínima/umbral de reacción o Fail delPO.

No se ajustan recetas, velocidades, ventanas, tempo o gesto.2.3–2.6noiniciadas;2.2enprogreso/92claves, hashes actuales/expectativas yregresión/cierrecheck-releasependientes. No se afirma fasecompleta ni suiteglobalverde. Informe/evidencia entregados paraClaude/PO antes de continuar;sinmerge/buildXcode/instalación/acceso al iPhone/sesión2.7/prod físico.2.0hashes1c8e598b099b/944668168805 son los observadosUnityantes de añadir7claves;no se atribuyen alestado92claves.41heredados/SimulationKey/2-35mm/N2/spec/P2/L1-L6 intactos. S1aceptado;otra decisión delPO necesaria para resolver el margen y reanudar sin ajustes unilaterales. [Informe](2026-10-30-first-shot-s2-m24-phase1.md), [evidencia](../validation/evidence/2026-10-30-s2-time-01.json).


### 2026-10-30 — S2-TIME-01 resuelto para continuar, misma PR #30

Claude revisó cdc8e4666f2f5fa54c3b887c7dd1f366acb93ccd y confirma parada/cifras/predeclaración0c638da/2.0correcta y7claves2.2con valores/rangos delplan. **Decisión literal del PO: «Seguir sin cambiar SL».**

Interpretación de Claude, **conversión de cifras ya publicadas, no medición nueva**, paraSL5/SL6/V100:

| Serie S1 | Mediana | Último apoyo |
|---|---:|---:|
| B2 fuertes | 0,87 s | +0,68 s |
| B5 «ritmo cómodo» | 1,12 s | +0,43 s |
| B3 medios | 1,35 s | +0,20 s |
| B2 suaves | 1,42 s | +0,13 s |
| B2 medios | 1,66 s | −0,11 s |

La incompatibilidad se observó solo con el gesto más lento deS1, sin pelota/apuntando con calma; no prueba inviabilidad deM2-4enpelotas cortas. Se relaciona con el abierto «aún mecánica/ritmo de rally». No se reescriben los datos/medianas deS1 ni la parada anterior.

Decisiones delPO: recetasSL1–SL6congeladas como0c638da; sin cambiarvuelos/velocidad/tempo/V/A/compensación/N2/dirección ni apoyo dentro deRecepción (§6.5.1). **S2-TIME-01 observación predeclarada de2.7, no bloqueo:** medir con golpe real duraciónBegan→Ended, anticipación respecto a lanzamiento y llegada aV/T3puro enSL5/6, todos los intentos; si elPO no llega vuelve alPO para decidir,sin ajustes. Autoriza reanudar mismafase1/rama/PR: expectativas92claves yhashesreales conTDD, completar2.2y2.3–2.6enorden/REDporaserciones→GREEN/concasosoriginales/D4.6/M2S2-N01–N12. Suites completasfrescas/transitoriosrestaurados/check-release limpio dev-prod gitDirtyfalse/informeactualizado/PR#30listaClaude sinfusionar. SinXcodebuild/instalación/accesoiPhone/sesión2.7, cambiosspec/criterios/bandas/umbrales oS3–S4/T8. **Ante otro defecto/incompatibilidad distinto deS2-TIME-01 registrar/detener.**


## S2 fase1 — avance y nueva parada S2-CLOCK-01 (2026-10-30)

La decisión literal **«Seguir sin cambiar SL»** se aplicó: recetas0c638da intactas;S2-TIME-01 solo observación predeclarada2.7. No se cambian vuelo,velocidad,tempo,A/V,compensación,N2,dirección o regla deBegan enRecepción. La tablaD4.2 conserva su aprobación literal anterior.

2.2retomada conTDDdehash92 yregresión473/473EditMode-208/208PlayMode;2.3contrato30/30focal y503/503EditMode;2.4solver12/12y515/515EditMode;2.5rollback21casos yregresión516/516EditMode-229/229PlayMode,sinCS.2.6queda en progreso:registros2/2,controlador46/46focal y518/518EditMode. Defaults106claves**915ae1e479b9**,P0extendido**dd122831f592**,observados porUnity enpruebas,noiPhone;solo gesture.control0→2 entre los85valores existentes y21claves nuevas delplan.41heredados0d39e6016bd8/SimulationKey166a96e319c7/2-35mm/N2estricto intactos. [Informe](2026-10-30-first-shot-s2-m24-phase1.md)/[auditoría](../validation/evidence/2026-10-30-s2-protected-state.json).

**S2-CLOCK-01, confirmado y sin corregir:** la lista deClockMap crece unsegmento porframe durantepausa o después delvuelo detenido enmodoFirstBall.120frames:85→205detenido,32→152pausado,estado depelotaidéntico. Reproducciónsintética2tests/2fallos poraserción,sinCS;no memoria enbytes niH17 nativo medidos. [Evidencia](../validation/evidence/2026-10-30-s2-clock-01.json). Se aplica la condición delPO **«Ante otro defecto o incompatibilidad que no sea S2-TIME-01, registra y detente»**. No se infiere autorización para corregir/coalescer ni elegir nuevos límites. Propuesta de corrección para decisión, noimplementada,enelinforme.

No hay suitePlayModecompleta con2.6 ni check-release finalgitDirtyfalse/revisión final de rama; no se declara cerrada lafase1 ni todosD4.6/M2S2-N01–N12. RegresiónFirstBall y,eventualmente,su preparación explícitapractice0 sin tocaraserciones,siguenpendientes. Código/evidencia/RED conservados en lamisma rama/PR#30paraClaude/PO,sinfusionar. No nuevaaceptación deS2,niXcodebuild,instalación,iPhone,sesión2.7,prod físico,S3–S4/T8. S1permanece aceptado ylosbloques2.7dependen deautorización futura. Perfilhasta2026-11-03T18:29:44Z,no comprobado por noinstalar. No commit aparte enmain.


## S2-CLOCK-01 — revisión Claude y corrección acotada autorizada por PO (2026-10-30)

PO comunica revisión de Claude del head **8777f9bcd48cfe0924bf4851ae7606abe312095f**: parada correcta; mecanismo ObserveClock→Mark verificado; mapa avanzando con pelota detenida intencionado/F02. Los21defaults nuevos coinciden con el plan y entre85anteriores solo cambia gesture.control. Estimación de Claude, no medición: unos40bytes/segmento/2,4KB por segundo a60fps y mayor recorrido lineal de TrySimToReal tras pausas largas.

Autorización del PO para **la misma rama/PR #30**:

> Agrupar observaciones estables. Si una observación no cambia tempo, cámara lenta, pausa ni descartes, y cae exactamente en la continuación del segmento anterior, no crea un segmento nuevo.
>
> Mantener exactos los cambios de estado: pausa y reanudación, paso manual, tempo, cámara lenta y descartes.
>
> Sin añadir nada nuevo: ni un segundo reloj, ni parámetros, ni límites de tiempo, ni umbrales, ni un recorte por antigüedad.

Antes de implementar exige protecciones verdes para Ended tardío/r original, expiración, paso manual, F02 y golpe/rollback después de pausa larga. RED S2-CLOCK-01 2/2→GREEN; reflexión o contador de lectura sin efecto runtime permitidos. Continuar2.6: fixtures FirstBall con practice.enabled0 solo preparación/aserciones intactas, D4.6/N01–N12 pendientes, suites completas frescas/sinCS, transitorios restaurados, check-release dev-prod limpio/gitDirtyfalse e informe.

> Ante otro defecto, registra y detente. Entrega la PR #30 lista para Claude, sin fusionar, y detente.

SL1–SL6 congeladas; S2-TIME-01 observación para2.7. Sin cambiar vuelos/velocidad/tempo/V-A/compensación/N2/dirección/pullFullMm/spec/criterios/umbrales. NoXcodebuild/instalación/iPhone/2.7/S3–S4/T8.

Aplicación observada: bd84896 corrige reloj tras protecciones13/13 y16/16; RED original2/2 repetido→GREEN2/2, Core16/16/rollback25/25. Candidatos fallidos conservados en evidencia, sin tocar aserciones. Fuente final9131b1c: fixtures explícitos y cobertura adicional; guarda de hash predeclarado pendiente de D4.6 con esqueleto/RED1/1→GREEN; controlador65/65. Suites frescas522/522EditMode y297/297PlayMode, sinCS/cero omitidos; transitorios restaurados/514fuentes/40protegidos cotejados. **Check-release final dev/prod OK desde 1bbaa39 limpio, gitDirty=false ambos**; Core/Runtime en ambos, DevPanel solo dev y framework de pruebas excluido. 514 fuentes coinciden después de los exports y árbol limpio; 386 aserciones de los fixtures intactas. Alcance2.0–2.6verificado y entregado para revisión de Claude/PO en PR#30, abierta/lista/sin fusionar; no cierre ni aceptación de S2 inferidos. [Informe](2026-10-30-first-shot-s2-m24-phase1.md), [JSON final](../validation/evidence/2026-10-30-s2-task26-final.json). S1aceptado; sin aceptación de S2 inferida.


## 2026-10-31 — aprobación de fase 1 PR #30 y predeclaración documental de 2.7

PO comunica revisión aprobada de Claude del head **c2e8ee7** (producto **9131b1c**; 1bbaa39/c2e8ee7 solo documentación). Sin bloqueos: S2-CLOCK-01 correcto; fixtures solo preparación; cobertura D4.6/N01–N12 en rama; nada protegido cambia desde8777f9b; check-release dev/prod limpio. Aprobación de **fase 1 (2.0–2.6)**, no aceptación de sesión física ni cierre de S2.

Observaciones registradas, sin código: overload ArmSlingBlock(block,expectedGestureHash) solo utilizado en pruebas, panel DEV llama sin hash; el campo expectedGestureHash del registro es el observado al armar y no uno predeclarado. El mapa puede seguir sumando segmentos durante vuelo activo y se borra en el lanzamiento siguiente. Perfil conocido vence **2026-11-03T18:29:44Z**.

Autorización literal del PO:

> Autorizado, solo documento y en la misma PR #30: predeclarar por escrito los bloques de 2.7.
>
> La PR #30 sigue sin fusionar hasta que el PO juzgue la sesión 2.7. Si aparece otro defecto, regístralo y detente. Entrega la PR #30 lista para Claude y detente.

Requisitos de la predeclaración: orden/tarea/SL/instrucciones al PO; verificación de hash conforme a la observación (cualquier conexión solo propuesta, sin código); preguntas literales, métricas descriptivas, parada/restauraciónP0; antes de instalar, **perfil renovado por PO y comprobado, respaldo y cotejo de los tres archivos**. NoXcodebuild/instalación/accesoiPhone/2.7/código/tests/defaults/SL/spec/criterios/bandas/umbrales/S3–S4/T8.

Aplicación documental: [predeclaración 2.7](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md), 25 bloques propuestos (203 recepciones dev +3prod), comparación principal SL1–SL6/V100 frente a T3puro con T3 no suave, familiarización y ejercicios separados. Hoja externa/manual de esperado y observado por bloque, definición del manifiesto H(s,m,c) previa a observación y literales independientes resueltos antes de armar en una preparación futura autorizada; el campo de armado DEV no sustituye esa referencia. No botones S1 presentados como contadores S2, ni nueva conexión de hash implementada. Preguntas originales y triada por separado, métricas/ausencias/NotEvaluated, S2-TIME-01/abiertos conservados, parada y P0 en proceso nuevo/cotejo de tres archivos tras instalación y restauración. Se actualizan este registro, informe y AGENTS.md; plan/propuesta/spec y producto permanecen intactos. **Nada ejecutado en iPhone ni suites/builds/exports nuevos.** Entrega misma PR#30 paraClaude/sin fusionar y detenerse; juicioPOde2.7 sigue pendiente.


## 2026-10-31 — predeclaración 2.7 head31d7433 aprobada con ajustes documentales A1–A4

PO comunica aprobación de Claude con ajustes y autoriza aplicarlos en la misma PR#30. Verificado sin bloqueos: delta desdec2e8ee7 exactamente cuatroMarkdown/producto intacto; tablaSL igual aPracticeLaunchDefinitions; slowMotion1 normal/validLimit1V-0T3puro/earlyPolicy0T3;16+3preguntasliterales;203+3recepciones/60V100+60T3puro; anclaB→SL1→SL necesaria/correcta por reasignación al cambiarreceta/comparaciónfloat; Claude reprodujo915ae1e479b9 ydd122831f592 fueraUnity.

Ajustes autorizados/aplicados solo en Markdown:

- **A1:** inicio dev con cierre DEV porPO, usando línea auditada dev-toggle/result/completed/accepted/devOpen=false; recepción1 primerLaunchId posterior. DevHasInputFocus solo es foco deapp: práctica sigue conDEVabierto. RelojMacUTC secundario, asociación con par timeReal/ts de dispositivo, incertidumbre explícita/sin calibrar latencia; interbloque/preparación fuera denominadores. PROD no tieneDEV: excepción documental con shot-plan del SL1 posterior a inicialización de práctica, primer nuevoLaunchId posterior como recepción1, sin evento/control nuevo; para revisiónClaude.
- **A2:** triada porintento soloFAM/CAN/NEG-A–D; resto excepciones por número de recepción al cerrar, lo no señalado sinrespuesta/nuncaOK por defecto; no interrumpirV/T.
- **A3:** calcular ahora13H(s,m,c), deltas/SHA256manifiesto e inputs, fueraUnity/sinapp. Fuentes: DefaultPreset de9131b1c, respaldoP0 local SHA256**2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd**, cuerposfloat32→double/canonicalizaciónParamSet.ComputeHash. [Anexo con resultados](../validation/2026-10-31-first-shot-s2-session27-hashes.md), sin nuevos scripts/JSON. H(1,V,0)=**dd122831f592** comprobado; Claude revisa literales antes deprimerbloque; cotejoUnity solo cuando la preparaciónestéautorizada ysi no coincide registrar/detener, sin ajustar literales.
- **A4:** tandasFAM+02–13 /14–22 /23–24 /PROD (126/57/20/3). Cadauna empieza conprecheckcompleto y terminaexportando logs/restaurandoP0enproceso nuevo/cotejo byteabyte3archivos; todas dentrovigenciaperfilrenovado/comprobado. No fechaejecucióninferida.

Menores: «1 = sin cámara lenta» junto aslowMotion; errorAim(Began)→C(3,2/2,2) descriptivo, referencia verbal sinmarcador/sin umbral. [Protocolo actualizado](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md), informe y AGENTS actualizados. Cálculos documentales no son tests deproducto ni mediciones de app/dispositivo. **Solo Markdown**, sin código/tests/defaults/presets/SL/spec/plan/criterios/bandas/umbrales ni suites/check-release/build/instalación/accesoiPhone/sesión. PR#30 sigue sinfusionarhasta juicioPOde2.7;S3–S4/T8noautorizados. Otrodefecto→registrar/detener. EntregarlistaClaude ydetenerse; no inferir aprobación del nuevohead.


## 2026-10-31 — preparación de tanda 1 de2.7 autorizada, no sesión

PO autoriza misma rama firstshot/s2-m24/PR#30 sin fusionar, **solo preparación: ningún gesto al PO ni bloque iniciado**. Corrección documental: citar archivo archivado mscorlib__3.cpp SHA26bb31ee…/literales sin cambio. Producto aprobado fase1 intacto; comprobar por diffGit/SHA256 Assets/ProjectSettings/Packages idénticos a probado, discrepancia→registrar/detener. Check-release dev/prod limpio/gitDirtyfalse y builddevReleaseARM64/noDevelopment.

Perfil: **PO renueva antes de instalar** (conocido vence2026-11-03T18:29:44Z); comprobar incorporado, inicio/vencimiento/firma/equipo/dispositivo/bundle y cobertura de toda tanda1+restauración. Sin cobertura noinstalar. Antes: exportarlogs/respaldo nuevoP0-calib-1/Po-1/last-active. Después: cotejo byte a byte3archivos/proceso nuevoP0/**dd122831f592**/0pendientes/Jugando/sinpausa/DEVcerrado; es primer cotejo B enapp, diferencia→registrar/detener/sin recalcular. Otros12literales segúncondición se cotejan enprecheck con propiaapp,sin scripts/tests temporales. Registrar build/perfil/respaldos/cotejos/hash observado enAGENTS/aprobaciones/informe; dejarP0/sinconfigtemporales/singestosnuevos, PRlistaClaude ydetenerse.

Sin código/tests/defaults/SL/presets/spec/plan/criterios/bandas/umbrales nuevos. **FAM/02–13 requiere autorización posterior propia; tandas2–4/PROD/S3–S4/T8 no autorizados.** Registro inicial:506archivos deproducto Git/SHA256iguales a9131b1c y archivoCpp preservado conSHA esperado. RenovaciónPO/horario de tanda1 solicitados; aún no se afirma build/instalación/arranque ni preparación terminada.


## 2026-10-31 — renovación delegada, ventana prevista y preparación completada

Durante la preparación el PO respondió literalmente: **«Ayúdame tu a renovar el perfil Tienes el permiso de utilizar mi PC»**. Se autoriza así al agente a realizar la renovación con la cuenta/herramientas existentes; el requisito de perfil nuevo/verificado antes de instalar se conserva. Al precisar la previsión de vigencia confirmó **«Mañana 1 de noviembre, antes de las 22:00»**, America/Santiago, incluida restauración. Es previsión para cobertura, no autorización de sesión.

Resultado: Git/SHA256 de506Assets/ProjectSettings/Packages coincide con9131b1c probado antes/después. Check-release dev/prod nuevo2ef4807limpio/gitDirtyfalse ambos; build **0.1.0+2ef4807 dev**,ReleaseARM64/IL2CPP/noDevelopment. Primer nativo firmado con perfilviejo noinstalado; elperfil anterior se respaldó/retiró solo decaché deforma recuperable y se regeneró con Xcode/firmaautomática por delegaciónPO. Incorporado **2026-10-31T17:58:46Z→2026-11-07T17:58:46Z**, vigente durante todalafecha1nov+hastalímite22local/restauración; firma/certificado autorizado preexistente/equipo/dispositivo/bundle comprobados. Sinrevocación solicitada ni clavesprivadas exportadas; identidadprivada en evidenciaignorada. [Método oficial Apple](https://developer.apple.com/help/account/provisioning-profiles/edit-download-or-delete-profiles/).

Export previo31metrics+11spike/9.962.251bytes y respaldo nuevoP0/Po-1/last-active, repetido justoantesdeinstalar. P0**3614B/2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd**,Po-1**2843B/9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767**,selección**10B/5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087**. InstalaciónOK sinautolanzar; cotejosbyteabyte trasinstalar/antesabrir ytrasarranque/finalmantienenlos3archivosidénticos. No guardado/importación/cambio de presets ni selección.

CLI inicialdenegadoLocked, appnoiniciada; resuelto alabrir icono porDuplicación. **Proceso nuevodev18:05:40.5084170Z/15:05:40.508417local**,P0**dd122831f592/0pendientes**,primer cotejonativo B=H(1,V,0) satisfecho sinrecalcular. DEV abierto/cerrado solo poragente para comprobar: Jugando/P2/P0/cero pendientes; cierre18:07:20.6044640Zpausedfalse/focusedtrue/devOpenfalse, estadofinalDEVcerrado/sinpausa. Capturafinal0gestos/órdenesjuego;4eventosUI filtrados/4dev-control/32NoOrderautomáticos, todospreparaciónsinbloques. El cierreDEV delagente no eslafronteraPOdeFAM. Losotros12Hse cotejarán conappenfuturos prechecksautorizados; sin scripts/tests temporales. [Informe completo](2026-10-30-first-shot-s2-m24-phase1.md); fuenteCppSHA26bb31ee… archivadayreferenciacorregida/literalesintactos.

**Preparación terminada para Claude; PR#30 abierta/lista/sinfusionar.** AppenP0/sinconfigtemporal/ningúngesto pedido. No suites, nuevo código/tests/defaults/SL/presets/spec/plan/criterios/bandas/umbrales. No sesiónFAM/02–13 ni tandas2–4/PROD/S3–S4/T8 autorizadas. Fin deesta entrega: detenerse. ÚltimoajustedeGit documentalno cambiaappinstalada.


## 2026-10-31 — preparación head68f5383 aprobada; sesión de tanda 1 autorizada

PO comunica verificación de Claude: producto idéntico a9131b1c, perfil31oct17:58:46Z→7nov17:58:46Z,140líneas2ef4807dev/dd122831f592/pending0/singestos/cierreDEVlínea104 y tresarchivosidénticos. Autoriza soloFAM(01) y02–13 en misma rama/PR#30 sinfusionar, predeclaración sin cambios. Revalidar perfil/build, respaldo nuevo yproceso nuevoP0/dd/pending0/Jugando/sinpausa. Explicaciónliteral§4/trescomprensiones antesFAM; noentiende→restaurar/detener. Unbloque a la vez/B→SL1→SL/panel/singuardar/deltasúnicos/precheckcompleto/hashpropioapp=literal/sinrecalcular. CierreDEVporPOdevOpenfalse esfrontera, recepción1primernuevoLaunchId posterior;MacUTCsecundario/sintocarteléfonodurantegestos. FAMtriadaporintento,02–13excepcionesalcierre/restosinrespuesta/preguntatarea+tres2.7, sin16S1. Sinrepeticiones/sustituciones, conservar todoslosintentos/paradas§7. Cierre exportaciónantesnuevaapertura/P0procesonuevo/dd/pending0/3archivosidénticos, antes1nov22America/Santiago incluida restauración. Si parcial, nocompletarotro día. Informe descriptivo/respuestasliteralesseparadas/sinjuicioinferido/PRlistaClaude/detenerse. Sin tandas2–4/PROD/S3–S4/T8 ni código/tests/builds/instalaciones/defaults/presetguardado/SL/spec/plan/criterios/bandas/umbrales;otrodefecto→registrar/restaurar/detener.

Recheck y respaldo inicial completados; proceso nuevo20:20:13.1061270Z2ef4807dev/P0dd122831f592/pending0. Comprensión solicitada y pendiente; ningún bloque iniciado. [Informe de tanda1](2026-10-31-first-shot-s2-tanda1.md).

## 2026-11-01 — tanda 1 terminada; decisiones literales del PO durante la sesión

Operación continuada por Claude a pedido del PO («Ayúdame a realizar las pruebas... Usa mi PC en duplicar pantalla para revisar»), por falta de créditos de Codex; Claude no es revisor independiente de esta tanda. Decisiones literales del PO registradas:

- Comprensión: anillo no identificado → **«Aclarar el anillo»**; tras aclaración verbal: «Ah perfecto, veo el circulo celeste y el circulo que se va cerrando.»
- 03 intento 1 interrumpido/frontera ambigua → **«Sí, repetir 03»** (bloque adicional 03b; parcial conservado).
- 04 intento 1 con dos cierres completados → PO repite por iniciativa propia: **«Hice la prueba de nuevo, para que revises»** (04b; ambos candidatos conservados).
- Tras el bloque 11, segundo juicio negativo de trayectoria en SL5 escalado por la parada de §7 (la primera versión citaba §8 por error); el operador recomendó seguir → **«Seguir con 12»**: desviación de §7 autorizada por el PO. En 12 el PO difiere la revisión («eso se puede ver después»).

Resultado: FAM y 02–13 medidos, respuestas literales separadas, P0 restaurado en proceso nuevo 21:56:11.3538050Z (dd122831f592/pending 0) y tres archivos idénticos byte a byte al respaldo. Sin aceptación inferida; PR #30 sin fusionar. [Informe de tanda1](2026-10-31-first-shot-s2-tanda1.md).

## 2026-11-01 — revisión independiente de la tanda 1 y decisión del PO sobre la trayectoria

**Revisión independiente** (sesión Claude distinta del operador, solo lectura) del head `66095c8`: **CAMBIOS SOLICITADOS**. Datos primarios confirmados (fronteras, hashes, pending, conteos, contactos, P, restauración y citas literales). Hallazgo bloqueante: preguntas de cierre no literales sin declarar (pregunta 1 de 02 reformulada; las cinco de 09–13 no literales, sin triada y con la pregunta 2 sugerente), además de preguntas enviadas juntas y tras mostrar resultados. Correcciones: extras, gestos previos a FAM (26), 9 gestos tardíos de la preparación de Codex, LaunchId de 03 intento 1 (L150/L151), cita §7 en vez de §8, 03b/04b sin precheck previo y 03b sin §3.2 «Reinicio/segundo plano». El operador cotejó cada hallazgo y corrigió el [informe](2026-10-31-first-shot-s2-tanda1.md); los datos primarios no cambian.

**Pendiente de decisión del PO:** cómo tratar las respuestas a preguntas no literales, en especial el juicio del salto de 09–13 («¿Cómo viste el salto?»), que es condición de fusión.

**Decisión literal del PO (2026-11-01), posterior al head revisado:**

> Perfecto. Con respecto a mi observación, la trataremos como un posible defecto que se habra en un PR aparte. Pero las tandas 2 a 4 no se bloquea. Cuando vuelva el revisor te comento, y luego vamos avanzando hasta que tenga creditos en codex y ahí seguimos tal cual estabamos antes

Consecuencias registradas: la trayectoria SL5/SL6 se tratará como posible defecto en una PR aparte, fuera de PR #30, sin declarar defecto ni ajustar nada aquí. Las tandas 2–4 no quedan bloqueadas por esa observación. Claude sigue como operador hasta que Codex recupere créditos. PR #30 sin fusionar.


## 2026-11-01 — decisión de repreguntar el salto en tanda 2 y retorno de Codex

Decisión literal del PO, posterior al head revisado `66095c8`, comunicada a Codex al retomar desde `e160f98`:

> Repreguntemos en tanda 2

Se refiere al juicio del salto de **09 SL4/V, 10 SL5/V, 11 SL5/T, 12 SL6/T y 13 SL6/V**. La decisión sustituye la espera de una decisión sobre cómo abordar ese juicio; no convierte las respuestas a «¿Cómo viste el salto?» en respuestas a «¿el salto es aceptable?». Las respuestas originales, el envío conjunto y la exposición previa de resultados se conservan sin borrar ni reinterpretar. El juicio literal sigue pendiente; no hay Pass ni autorización de fusión inferidos.

**Propuesta operativa de Codex, pendiente de confirmación expresa del PO:** al inicio de tanda 2, antes de 14 y sin mostrar resultados, identificar cada condición anterior en ese orden y preguntar únicamente **«¿el salto es aceptable?»**, una a una, esperando cada respuesta antes de pasar a la siguiente. Serán repreguntas retrospectivas sobre la tanda 1, separadas de sus respuestas originales, con fecha/condición y limitación de recuerdo; «No recuerdo» se conserva literal y deja pendiente ese juicio. No se propone repetir los bloques ni presentar las repreguntas como una medición nueva. Codex solicita al PO confirmar este procedimiento antes de empezar; la confirmación aún no está recibida al escribir este registro.

El PO pide retomar en el mismo worktree/rama/PR #30. Siguiente trabajo: **tanda 2 dev, 14–22**, con precheck completo nuevo desde P0 verificado y la **0.1.0+2ef4807 dev instalada**, sin build ni instalación nuevas. Rigen predeclaración §3.2/§5.1/§6/§7/§8, preparación B→SL1→SL, preguntas literales una a una antes de resultados, preservación de parciales/extras y parada ante posible defecto nuevo. La trayectoria SL5/SL6 queda como observación conocida destinada por el PO a otra PR, sin corrección aquí.

Antes de cada tanda comprobar el perfil incorporado (vencimiento **2026-11-07T17:58:46Z**) y cobertura de todo el tramo, incluida restauración. Cerrar cada tanda con exportación, P0 restaurado en proceso nuevo **dd122831f592/pending 0** y comparación byte a byte de los tres archivos con respaldo/referencia inicial. Actualizar informe y entregar para revisión Claude/PO; **detenerse al cierre de tanda 2**. Tanda 3 y luego PROD siguen la secuencia indicada por el PO, después de la parada; la instalación PROD requiere autorización propia, respaldo nuevo y frontera §5. Sin código/tests/defaults/SL/presets guardados/spec/plan/criterios/bandas/umbrales nuevos. PR #30 sin fusionar; S3–S4/T8 no autorizados. Este registro inicial no acredita precheck ni bloque nuevo.


### Confirmación del procedimiento de repreguntas — PO, 2026-11-01

Respuesta literal del PO, posterior al commit documental `c336c51`:

> Confirmo el procedimiento, con estas precisiones:
> 1. En cada repregunta da solo el contexto de condición (número de bloque, SL, ventana y posición del cuerpo), sin resultados, contactos, timing ni trayectoria.
> 2. Haz las cinco repreguntas antes del precheck del bloque 14, con su hora, registradas como retrospectivas (de memoria, un día después) y separadas de las respuestas originales, que no se borran.
> 3. Si respondo «No recuerdo», queda literal y pendiente. La tanda 2 es solo SL1, así que resolverla requeriría un bloque nuevo que yo autorice aparte.
> 4. Las preguntas 1–4 de 09–13 se mantienen como están, declaradas no literales; solo se repregunta el salto.

Este literal confirma y precisa la propuesta anterior. Se recogerán las cinco respuestas una a una antes del precheck de 14; no se repiten condiciones de tanda 1 ni se autoriza un bloque adicional para resolver un recuerdo ausente. Las preguntas 1–4 y respuestas originales de 09–13 conservan su estado no literal y sus limitaciones. [Registro nuevo de tanda 2](2026-11-01-first-shot-s2-tanda2.md); todavía sin precheck ni bloque nuevo.


### Respuestas retrospectivas del salto, antes del precheck de 14 — 2026-11-01

Con el procedimiento confirmado se preguntó una condición cada vez, solo con número/SL/ventana/cuerpo nominal y sin resultados. A «¿el salto es aceptable?» el PO respondió literalmente **«si»** por separado en **R09 SL4/V100, R10 SL5/V100, R11 SL5/T3 puro, R12 SL6/T3 puro y R13 SL6/V100**. Horas Mac secundarias de formulación y recepción-registro en el [informe de tanda 2](2026-11-01-first-shot-s2-tanda2.md). Todas son retrospectivas, de memoria/un día después según el procedimiento aprobado, y anteriores al precheck de 14. Originales intactos; preguntas 1–4 de 09–13 siguen no literales, sin repreguntar. Son juicios explícitos de aceptabilidad del salto recordado, no nueva medición, aceptación global de S2 ni autorización de fusión. PR #30 permanece sin fusionar.


### Modal de preguntas solicitado por el PO durante el cierre de 14 — 2026-11-01

Literal: **«Para el resto de las preguntas dejamelas de una para ir respondiendo en un pequeño modal.»** Recepción-registro Mac **2026-11-02T00:55:27.212861+00:00 / 2026-11-01T21:55:27.212861-03:00**. Autoriza desde este punto la desviación de modalidad: agrupar las preguntas restantes en un modal con campos separados, manteniendo los literales y sin mostrar resultados previamente. Las preguntas ya respondidas no se repiten; la triada por intento solo donde §6.1 la requiere se mantiene asociada al intento. No cambia el contenido de la predeclaración ni sus criterios/umbrales, ni autoriza bloques adicionales o fusión.


## 2026-11-04 — pausa física y preparación offline autorizadas por el PO

El PO comunica que no puede completar la autenticación de Duplicación porque está en el trabajo. Orden literal: **«Deja la sesión física en pausa tal como está. No toques el iPhone, no relances la app y no prepares 16. Cuando vuelva, te aviso, y entonces verificas P0 en un proceso nuevo y preparas 16 según el protocolo.»** Sustituye la continuación física pendiente; no implica restauración observada ni cierre de tanda2.

Autoriza sin acceso al iPhone:

1. Preparar tanda4PROD: check-release dev/prod y buildprodRelease de la misma fuente de producto de devinstalada, exportslimpios/gitDirty=false; **no instalar**, instalaciónconautorizaciónpropiaPOpresente/respaldo nuevo§2/§8. **No modificar Logs/evidence.** Dejarworktree/ProjectSettingslimpios.
2. DiagnósticoSL5/SL6 solo documental en rama/worktree propios desdeheadPR30: contrato de mira en spec/propuesta; **predeclarar antes de calcular** simulaciónP2offline desdecontactosregistrados10–13 conprimerbote/laterales/out;opcionesconventajas/riesgos,sinelegir. Sinproducto/tests/defaults/SL/spec/criteriosnuevos. PRClaude sinfusionar.
3. PlanS3documental3.1–3.4a laluzdeS2/M2-4, sinimplementar/autorizarcódigo. Se agrupa con diagnóstico en PRdocumental separada de#30.

Referencia deproducto9131b1c, appdevinstalada0.1.0+2ef4807;headPR30alrecibirencargo **dfffd58cc984f3902ce823644a35fa8d4b8cac8f**. Perfilconocidovence7nov17:58:46Z/14:58:46Chile;planificarrestantestandasconsusrestauracionesantesdelfin,advertirantesdeinstalarsinoalcanza. PR30sinfusionar;S3–S4/T8sinimplementación. PreparaciónPROD se registra en [informe](2026-11-04-first-shot-prod-preparation.md); no autoriza tocar el teléfono.


### Resultado de preparación PROD offline, sin instalación — 2026-11-04

Checksdev/prodfrescosOKen **a11fb32 limpio/gitDirty=false** yprodRelease/IL2CPP/ARM64/noDevelopmentfirmada,producto9131b1c idéntico a2ef4807devinstalada. DestinoXcodegenérico/sinprovisioningupdates/sinaccesoiPhone. Perfilincorporadovence **2026-11-07T17:58:46Z**. Logs/evidence3145archivos íntegros también enmtimeNs;514fuentesprotegidas/ProjectSettingsintactos,árbollimpioalterminar. [Informe completo](2026-11-04-first-shot-prod-preparation.md). **No instalación ni campañaPROD; no relanzamiento/precheck16**. Pausafísicaseconserva,autorizaciónPRODpresente/respaldo nuevo/ventanapendientes;PR30sinfusionar/S3–S4/T8sinimplementación.


## 2026-11-04 — diagnóstico SL5/SL6 y plan S3, solo documentación offline

POautoriza diagnóstico enrama/worktreepropiosdesdeheadPR30 **dfffd58**: significado de mira enspec/propuesta, simulaciónP2offline deestadosdecontacto10–13conmétodopredeclaradoantesdecalcular,primerbote/laterales/outyopcionesconventajas/riesgos,sinelegir. AutorizaademásplanS3documental3.1–3.4sinimplementación;CodexlosagrupaenmismaPRdocumentalseparadade#30. Método: [predeclaración](../diagnostics/2026-11-04-sl56-p2-method.md),commitantesdelprimer cálculo.

Sesiónfísicaenpausa;noiPhone/relanzar/preparar16. Logs/evidenceoriginalesintactos/sololectura. Sinproducto/tests/defaults/SL/spec/criteriosnuevos;PRparaClaude,sinfusionar. PreparaciónPRODocurreporseparadoenworktreeS2y **noautoriza instalar**. Perfilconocidovence7nov17:58:46Z;POpresente/autorizaciónpropia/respaldo nuevoantesdecualquierinstalación. PR30sinfusionar/S3–S4/T8sinimplementación.


### Resultado documental del diagnóstico y plan S3 — 2026-11-04

Método 975b597/2141676 anterior al primer cálculo; auditoría de endpoints del arnés 4a55d80, sin cambio P2. Selección primaria de40recepciones/24Applied, sin extras: ejecución P2 original offline repetida con salida idéntica byte a byte. [Informe de trayectoria](2026-11-04-first-shot-sl56-trajectory-diagnosis.md), [método](../diagnostics/2026-11-04-sl56-p2-method.md), [derivado](../validation/evidence/2026-11-04-sl56-p2-offline.json), [reproducción](../diagnostics/2026-11-04-sl56-p2-reproduction.md). Primer frontis respetado, pero trayectoria posterior con laterales/out/escape según tabla y limitaciones; sin atribuir campaña nueva, Fail nuevo o elección del PO. Ninguna opción seleccionada.

[Plan S3 documental](2026-11-04-first-shot-s3-documentary-plan.md), tareas3.1–3.4, dependencias sobre S2/M2-4, TDD futuro y validación propia; no modifica plan/spec ni autoriza código. Sesión física permanece en pausa, sin acceso al iPhone/relanzamiento/preparación16. PR30 sin fusionar; entrega en PR documental propia para Claude, sin fusionar ni implementación S3–S4/T8.


### Entrega para revisión de Claude/PO — PR #31, 2026-11-04

[PR #31](https://github.com/rafaallendes-boop/squash-clash/pull/31) abierta, no borrador, sobre firstshot/s2-m24/PR#30, sin fusionar. Solo siete archivos documentales respecto de #30; métodos previos y datos derivados preservados, ningún producto/test/default/SL/spec/plan/criterio modificado. Ambas historias documentales se sincronizaron conservando los commits originales de predeclaración. Fuentes y primarios cotejados por hashes;141rutas de enlaces locales comprobadas. Ningún mensaje/revisión externa solicitado por herramientas.

Preparación PROD en #30, builda11fb32firmada/sininstalar,Logs/evidenceintactos. Pausa física permanece por instrucción del PO;16sinpreparar. Perfilvence7nov17:58:46Z/14:58:46Chile;las reservas deagenda son propuesta, disponibilidad aún pendiente. No implementación S3–S4/T8 ni elección de opción de trayectoria.
