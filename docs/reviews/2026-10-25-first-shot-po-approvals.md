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
