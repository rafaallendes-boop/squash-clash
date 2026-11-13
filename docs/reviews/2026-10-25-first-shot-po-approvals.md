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


### Reanudación física de tanda2 desde16 — PO, 2026-11-04

ElPO vuelve y autoriza «Volví. Retomemos la tanda 2 de PR #30 desde el bloque 16, tal como estaba predeclarado». Exige ventana antes de7nov17:58:46Z, proceso nuevo/P0/cero pendientes/precheck. Literal vigente **«Las preguntas van literales, una a la vez y antes de mostrar resultados. Avísame qué necesitas de mí en cada paso».** Desde ahora sustituye elmodal agrupado anterior; originales de14/15 se conservan.

Preparación condev2ef4807/producto9131b1c, sinbuild/instalación nuevos ni PR#32. Perfilrevalidado23:46:12Z/firma-identidadcorrectas/cubre reserva técnica90min; disponibilidad personal solicitada,aún sinrespuesta alpreparar16. Exportnuevo/respaldo3archivosidénticos; procesonuevo23:46:49.304520Z/P0dd122831f592/pending0; B→SL1→SL1/3cmp antes ydespués deaplicar/precheck16aceptado. DEVabierto: soloPOcierra parafrontera16, aúnsin iniciar. [Informe y evidencia](2026-11-01-first-shot-s2-tanda2.md). No resultados de16 ni restauraciónfinal inferidos; tandas2–4 primero/PR30–32sinfusionar/S3–S4/T8sinimplementar.


## Modal de preguntas pedido de nuevo durante cierre16 (PO,4nov2026Chile)

Literal: **«Deja el modal completo para ir 1 a 1 las preguntas , así vamos mas rápido»**. Sustituye desdeelcierre16 la entrega de una sola pregunta pormensaje solicitada alretomar4nov; preguntas literales/campos separados, sinresultados previos. Desviación deenvío agrupado autorizada explícitamente, sin modificar protocolo/criterios ni borrar originales de14–16. Respuestas individuales de16 eninforme detanda2.


## Fin de tanda2 y continuación mañana — PO,4nov2026Chile

Literal del PO: **«Terminé 22. Hagamos las preguntas y seguimos mañana, ahí tendré mas tiempo, y dejame lo pendiente de las tandas»**. Se recogieron las cinco respuestas literales del22 antes de resultados. «Mañana» corresponde al5nov local; no se fijó hora/disponibilidad. No se pide23niPROD hoy. La solicitud no autoriza instalarPROD por inferencia.

Tanda2 completada según cuotas14–22:57recepciones/44Swing/17contactos; todos los cierres y20gestos extra separados. Desviaciones/modal retrospectivo y respuestas ambiguas conservadas en el [informe](2026-11-01-first-shot-s2-tanda2.md), para revisión Claude/PO, sin aceptarS2 ni autorizar fusión.

Primer reinicio final rechazado por bloqueo; el PO recuperó acceso: **«Listo, ya lo desbloqueé y volví a bloquear»**. Reintento aceptado y restauración final verificada en proceso nuevo **2026-11-05T00:59:07.010631Z /4nov21:59:07Chile /0.1.0+2ef4807dev /P0-calib-1/dd122831f592/pending0**. Tresarchivos byte a byte iguales porcmp tanto alrespaldo de tanda2 como al inicial de tanda1. No instalación/código/tests/defaults/SL/spec/plan/criterios/bandas/umbrales nuevos.

Pendientes: revisión de tanda2; tanda3(23CAM-F/24CAM-S,20recepciones y16preguntasS1+preferencia); tanda4PROD25(3recepciones), con autorización propia de instalación/POpresente/respaldo nuevo yfrontera§5. Cada tanda exporta/restauraP0 en proceso nuevo/cmp3/entrega yparada. Revalidar ventana/perfil antes de cada una; todo debe terminar antes del7nov2026 14:58:46Chile/17:58:46Z. PR#30/#31/#32 sinfusionar; regla experimental no autorizada para sesión física ni activación por defecto. S3–S4/T8 sinimplementación. Detenerse hoy.

## Tanda 2 aprobada por Claude en `0cc8060` — comunicado por el PO

El PO comunica: **«Claude aprobó la tanda 2 en el head 0cc8060 de PR #30, sin correcciones.»** Se registra la aprobación de **tanda 2**, sin extenderla a aceptación global de S2 o autorización de fusión. [Informe y observaciones](2026-11-01-first-shot-s2-tanda2.md).

Observaciones comunicadas, solo registro documental:

1. Modal de preguntas por petición del PO. En 17–21 la pregunta por intento se recogió al cierre del bloque. En 18 la aclaración **«A las tres recepciones que conté» llegó después de mostrar resultados**; conservar esta etiqueta y las respuestas iniciales anteriores, sin reordenarlas.
2. **«Toca para continuar» apareció 6 veces entre bloques**. Se aplicó §3.2 sin causa atribuida. Tenerlo previsto en tanda 3 y PROD con el protocolo existente.
3. Recepción 1 de 18 y 19 sin gesto registrado: **no es un defecto demostrado**. No sustituir ni fabricar acciones.
4. Los resbalones simulados de 20/21 generaron golpe **6/6, como Swing**, con 0 contactos aplicados; compatible con **N04/N04b, que siguen abiertos**. No es una tasa accidental espontánea.
5. Los «sí» del PO al salto cubren hasta unos **8 diámetros en 16** (máximo ya publicado 7,988459). No se introduce un umbral ni un criterio nuevo.

Instrucción vigente del PO: **«No toques el iPhone; detente hasta el aviso del PO.»** No hay acceso al dispositivo, reinicio, precheck, export, cálculo, build, instalación, test o cambio de producto en este registro. P0 sigue con la última restauración verificada, sin comprobarlo de nuevo. Mantener un único «Estado vigente», idéntico en PR #30/#31/#32. Todas siguen sin fusionar; tandas 3/4 pendientes al aviso, PROD con autorización propia de instalación/respaldo nuevo y perfil vigente. Activación/validación física de la regla y S3–S4/T8 sin nueva autorización.

## Reanudación y entrega de tanda 3 dev — PO, 5 de noviembre de 2026 (Chile)

El PO autorizó 23 CAM-F y 24 CAM-S en PR #30, con revisión de la ventana, P0 en proceso nuevo, precheck completo y preguntas literales en modal de campos separados antes de resultados. Al cierre se recogieron las 16 preguntas S1 y la preferencia de dirección. Se completaron 20 recepciones, 20 Swing y 14 contactos, con cuatro gestos extra separados. El PO contestó «si» sobre el salto en ambas cámaras. Respuesta literal de preferencia: «Creo que ahora me gusta como está, ya que apunto donde quiero tirar la pelota y tenso lo necesario, me gustaría probar en u npartido». El detalle y las respuestas originales quedan en el [informe para Claude/PO](2026-11-05-first-shot-s2-tanda3.md).

Duplicación mostró «iPhone en uso» durante la preparación de 24. Se preservó esa preparación y se repitió §3.2 desde P0 en proceso nuevo, sin causa atribuida. La restauración de cierre quedó verificada a las **2026-11-05T13:30:55.471226Z** con **P0-calib-1, `dd122831f592` y cero pendientes**, y los tres archivos cotejados byte a byte contra el respaldo nuevo y la referencia inicial.

La entrega se envió a revisión. Tanda 4 PROD e instalación esperan autorización aparte del PO, con respaldo nuevo, frontera §5 y perfil vigente durante el recorrido y su restauración. Se detiene la sesión. PR #30, #31 y #32 siguen sin fusionar.

## Revisión de Claude de tanda 3 — aprobación comunicada por el PO

**Aprobada sin correcciones en `a0abbe3` de PR #30.** Claude hizo un recuento independiente desde los JSONL de `after-24`. En 23 CAM-F confirmó 10 Swing (6 Valid, 3 Failed y 1 Perfect), con LaunchId 71–74 como extras. En 24 CAM-S confirmó 10 Swing (5 Valid, 3 Failed y 2 Perfect). Confirmó los hashes `dd122831f592` y `398bae34bb81` respectivamente, con cero pendientes. También coincidieron los SHA-256 de los JSONL, el texto de las preguntas predeclaradas palabra por palabra, las respuestas literales y la restauración de P0.

1. **Potencia media pedida.** Quedaron en banda media 4/10 Swing en CAM-F y 2/10 en CAM-S. Los demás quedaron en banda fuerte (6/10 y 8/10), con cero saturados. El PO declaró haber seguido la intensidad. Se conservan ambas lecturas. La observación se vincula al tema abierto de saturación. `pullFullMm` sigue en 35 mm y el PO aún no ha tomado una decisión al respecto.
2. **Salto.** En fija, p50 4,18 y máximo 8,29 diámetros. En seguimiento, p50 2,52 y máximo 3,82 diámetros. El PO juzgó aceptable el salto en ambas condiciones antes de ver las cifras. Los bloques fueron secuenciales, con siete contactos por condición, de modo que no se atribuye un efecto causal a la cámara.
3. **Conflictos documentales.** PR #31 y #32 tienen conflictos con #30 solo en `AGENTS.md` y aprobaciones. Se resolverán al fusionarlas, después de #30. La sincronización del «Estado vigente» en las tres ramas deja la resolución pendiente.

Registro documental de la revisión, con evidencia y producto intactos. La sesión física sigue detenida. Tanda 4 PROD requiere autorización aparte del PO. Las tres PR siguen abiertas.

## Renovación del perfil y tanda 4 PROD — PO, 5 de noviembre de 2026 (Chile)

El PO autorizó renovar con su asistencia, conservando equipo, bundle, dispositivo, entitlements, certificados y claves, con contraseñas a su cargo si hicieran falta. También autorizó instalar y ejecutar el bloque 25 estando presente, con fecha límite operativa **2026-11-07T17:58:46Z** incluida la restauración. El perfil anterior quedó apartado y conservado. El incorporado nuevo se creó **2026-11-05T13:55:07Z** y vence **2026-11-12T13:55:07Z**. Se verificaron firma, certificado, equipo, bundle, dispositivo y entitlements antes de instalar.

PROD firmada **`0.1.0+8f030a8 prod`**, fuente de producto **`9131b1c`**, 514 archivos cotejados, check-release prod fresco con `gitDirty=false`, Release ARM64, no Development y sin `SQUASH_DEV`. Respaldo nuevo, export DEV y cotejos posteriores contra ambas referencias. La frontera PROD se acreditó antes de LaunchId 3 y la cuota 3–5 cerró con tres recepciones, dos Swing con contacto Perfect y una sin orden ni gesto. Dos gestos extra en LaunchId 6. El PO confirmó lanzamiento, golpe y nota, y contestó sobre el sonido «tenía el cel en silencio a si que no». Queda sin verificar el audio.

P0-calib-1 **`dd122831f592`, cero pendientes** en proceso nuevo PROD a las **2026-11-05T14:27:10.0146510Z**, con los tres archivos idénticos al respaldo nuevo y a la referencia inicial. PROD quedó instalada y la sesión se detuvo. [Informe para Claude/PO](2026-11-05-first-shot-s2-tanda4-prod.md). PR #30, #31 y #32 abiertas y sin fusionar.

## Revisión de Claude de tanda 4 PROD — aprobación comunicada por el PO

**Claude aprobó la tanda 4 PROD en `e75703b`**, según comunicó el PO. El recuento independiente desde el JSONL de `after-block25` confirmó LaunchId 1 transitorio, LaunchId 2 como frontera y cuota LaunchId 3–5 con NoOrder sin gesto, Perfect y Perfect. LaunchId 6 conservó los dos extras, AbandonedNoPull y Swing. Hash `dd122831f592` y cero pendientes. `block25-retry` es prefijo exacto del log de cierre. Verificó la identidad del producto `8f030a8` con `9131b1c`, el reinicio en P0 y los tres archivos restaurados. La copia de check-release difiere del script versionado únicamente en rutas de salida, el bucle solo PROD y la ruta de `source`.

A las **2026-11-05T13:53:40.0456190Z (10:53:40 Chile)**, después de restaurar la tanda 3 y antes del respaldo nuevo, la app DEV registró en L471 `dev-control` / `legibility-launch`, `stage=activation`, `accepted=true` y `devOpen=false`. Lo precedieron toques filtrados en la parte superior de la pantalla (L467–469). En L475 el resultado cambió el hash temporal a `141920afa6af` con cero pendientes. Los tres archivos conservaron sus bytes. El reinicio posterior en P0 repuso `dd122831f592`. Según el historial de herramientas de la sesión, Codex estaba preparando el export PROD en el Mac y sus herramientas no interactuaban con el iPhone ni con Duplicación en ese momento. Ante la pregunta de si tocó el iPhone hacia las 10:53 de Chile, el PO respondió literalmente «No recuerdo». La respuesta expresa falta de recuerdo. El log conserva los toques y su origen queda sin atribuir.

El PO confirmó lanzamiento, golpe y nota del resultado. El log confirma dos contactos. **Sonido NotEvaluated** por iPhone en silencio, sin resultado Fail. Una comprobación posterior requiere autorización propia. «todo ok» es una respuesta global y el feedback individual queda «sin respuesta». La recepción 1 de PROD fue la **cuarta** sin gesto registrado en el subtotal de tandas 2–4, que incluye 16 POT-F, 18 NEG-A, 19 NEG-B y 25 PROD. Claude corrigió su error en la observación 3 de tanda 4, donde había contado «tercera» al omitir 16 POT-F. Tanda 1 queda N/Agg para este campo. Es un recuento descriptivo, sin atribución de causa.

En el estado verificado al cierre, **`0.1.0+8f030a8 prod` queda instalada y DEV no está instalada**. El perfil renovado está vigente hasta **2026-11-12T13:55:07Z**. El anterior quedó apartado y conservado. Registro documental sin acceso al iPhone. La sesión queda detenida y las tres PR siguen abiertas, sin fusionar. [Informe de tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md).

## Resumen de cierre 2.7 entregado para revisión — 5 de noviembre de 2026 (Chile)

El PO autorizó una entrega solo documental en PR #30 para su juicio global sobre S2 con M2-4, incluido el salto, antes de decidir sobre la fusión. El [resumen de cierre](2026-11-05-first-shot-s2-closing-summary.md) enlaza los cuatro informes y las revisiones registradas, consolida las cifras publicadas, separa las cifras no agregables y conserva las respuestas literales y su contexto. Tanda 1 conserva la revisión con cambios solicitados y el informe corregido. Las tandas 2–4 conservan sus aprobaciones registradas. El resumen distingue los resultados funcionales PROD, el sonido NotEvaluated y las decisiones pendientes del PO.

Entrega para Claude, sin juicio global ni fusión autorizados. PR #30, #31 y #32 abiertas y sin fusionar, con un único «Estado vigente» idéntico. Solo documentación. Sesión física detenida. PROD `0.1.0+8f030a8` queda instalada según la última verificación, DEV sin instalar y perfil renovado vigente hasta `2026-11-12T13:55:07Z`. S3–S4/T8 sin autorización de implementación.

## Revisión de Claude del resumen de cierre 2.7

El PO comunicó que Claude aprobó el resumen en `d6f3529` con dos correcciones documentales. Verificó contra las tablas publicadas 206 recepciones, 181 Swing (115 + 44 + 20 + 2), 106 contactos, 74 Valid, 32 Perfect, 7 C-A y 92 extras, además de las citas literales y la lista neutral de decisiones.

Se corrigió el error de Claude en la observación 3 de tanda 4. El subtotal de tandas 2–4 es 4 recepciones sin gesto (16 POT-F, 18 NEG-A, 19 NEG-B y 25 PROD). Tanda 1 queda N/Agg. [Resumen corregido](2026-11-05-first-shot-s2-closing-summary.md).

### Aprobación de Claude del informe corregido de tanda 1

**Claude aprobó el informe corregido de tanda 1 en `e160f98`**, según comunicó el PO al revisar el resumen de cierre. Verificó las correcciones de la revisión independiente de `66095c8`: extras 11/12/13 = 4/3/2, 02 = 27 post, 06 con 1 pre, 26 gestos antes de FAM, 9 tardíos de preparación, LaunchId L150/L151 del 03 inicial y referencia a §7. Confirmó que 03b y 04b sin precheck previo y 03b sin aplicar §3.2 quedaron declarados como desviaciones. Los datos primarios conservan sus valores.

Se conservan las limitaciones de formulación y las respuestas originales, incluidas la pregunta 1 de 02 y las preguntas 1–2 de 09–13. Los juicios originales reformulados de 09–13 permanecen separados de las repreguntas literales del salto, recogidas retrospectivamente antes del bloque 14. El juicio global sobre S2 queda pendiente del PO. [Repreguntas de tanda 2](2026-11-01-first-shot-s2-tanda2.md#repreguntas-retrospectivas-sobre-el-salto-de-0913).

Se registran ambas correcciones solo en documentación. La sesión permanece detenida y las PR #30, #31 y #32 siguen abiertas y sin fusionar.

## Decisión del PO y cierre documental de S2 — 5 de noviembre de 2026 (Chile)

El PO comunicó la revisión final independiente de Claude sobre el head exacto `403f8caf22fd6fcd55fb0a182d037c8538e694dc` de PR #30, sin observaciones bloqueantes. Claude verificó que Assets, ProjectSettings, Packages y scripts coinciden con el producto `9131b1c`, con suites de 522/522 EditMode y 297/297 PlayMode sobre ese producto y check-release prod limpio de `8f030a8`. Confirmó base al día con main `8dd142d`, estado MERGEABLE/CLEAN y ausencia de CI. Las cuatro tandas quedaron documentadas y aprobadas, incluida tanda 1 corregida con sus limitaciones. La evidencia acredita P0 y los tres archivos restaurados. El diff está libre de archivos prohibidos.

**Decisión literal del PO**

```text
Claude, revisé el resumen consolidado de S2, los resultados de las cuatro tandas y las observaciones pendientes.
Como Product Owner de Squash Clash, comunico las siguientes decisiones para cerrar esta etapa y preparar la continuidad de First Shot.
1. Aceptación de S2: APROBADO CON CONDICIONES
Apruebo S2 con el control M2-4 («apuntar al apoyar») como control vigente.
La evidencia acumulada de 206 recepciones, 181 swings y 106 contactos, junto con mis evaluaciones en el iPhone, es suficiente para aceptar esta etapa y avanzar.
Considero satisfactorios:

* La precisión general del apuntado.
* La integración del gesto con el golpe real.
* El funcionamiento inicial del timing.
* La aceptabilidad del salto visual de la mira.
* La naturalidad del control, pendiente de comprobar bajo condiciones de rally.

Esta aprobación no significa que todos los problemas estén resueltos ni autoriza automáticamente nuevas funcionalidades.
Las observaciones pendientes deben conservarse explícitamente, con su evidencia y estado, sin reinterpretarlas como aprobadas o descartadas.
2. Potencia media y saturación: OBSERVAR SIN MODIFICAR
No autorizo cambios a `pullFullMm` ni a la curva de potencia en este momento.
En S2 no se observó la saturación de S1, pero permanece la dificultad para conseguir consistentemente una potencia media sin terminar ejecutando un golpe fuerte.
Quiero conservar este hallazgo como observación abierta.
La calibración definitiva deberá considerar la experiencia real del jugador y, posteriormente, el comportamiento durante los intercambios.
No ajustemos parámetros únicamente para mejorar métricas de una prueba aislada.
3. Trayectoria SL5/SL6: CONSERVAR DIAGNÓSTICO, NO ACTIVAR REGLA
Apruebo integrar la PR #31 como trabajo documental y diagnóstico, una vez integrada correctamente la PR #30.
La evaluación offline de «bajar el punto» debe conservarse como evidencia técnica, sin interpretarse como autorización para adoptar esa regla.
Respecto de la PR #32:

* Mantenerla abierta y sin fusionar.
* No activar la regla experimental de retorno.
* No modificar los valores predeterminados.
* No realizar nuevas pruebas físicas con esa regla sin autorización.
* Conservar sus resultados, pruebas y observaciones para una decisión posterior.

El problema de SL5/SL6 sigue abierto. Deberemos resolverlo o delimitar su impacto antes de considerar validada la experiencia de rally.
4. Orden de revisión y fusión de PR
Autorizo el siguiente procedimiento, sujeto a las verificaciones correspondientes.
Primero: PR #30 — S2
Antes de fusionar:

1. Claude debe realizar una revisión final independiente del último commit vigente de la PR #30, identificando su SHA exacto.
2. Confirmar que los cambios posteriores a las revisiones anteriores no introdujeron modificaciones de producto sin validar.
3. Verificar que las cuatro tandas estén correctamente documentadas y que el informe consolidado refleje las aprobaciones y limitaciones reales.
4. Confirmar que P0 y los archivos respaldados quedaron restaurados según la evidencia.
5. Comprobar el estado de integración, las pruebas exigibles y la ausencia de conflictos bloqueantes.
6. Actualizar la descripción de la PR para que refleje el estado final de S2.

Si Claude aprueba el último commit sin observaciones bloqueantes, queda autorizada la fusión de la PR #30, respetando las verificaciones anteriores.
Si aparecen cambios sustantivos, conflictos o problemas que requieran modificar código, detenerse y solicitar mi autorización antes de continuar.
Segundo: PR #31 — Diagnóstico SL5/SL6
Después de fusionar la PR #30:

1. Actualizar su base respecto de `main`.
2. Resolver únicamente los conflictos documentales necesarios, sin alterar el diagnóstico ni sus conclusiones.
3. Verificar que la evidencia y los resultados permanezcan intactos.
4. Solicitar revisión final de Claude sobre el resultado actualizado.

Si la revisión es satisfactoria y el alcance sigue siendo exclusivamente documental, autorizo su fusión.
Tercero: PR #32 — Regla experimental
No fusionar, activar ni incorporar a `main`.
Mantenerla pendiente hasta una decisión posterior del PO. No resolver sus conflictos mediante cambios de producto ni ejecutar nuevas validaciones físicas sin autorización.
5. Observaciones que deben permanecer abiertas
Mantener un registro trazable de:

* Trayectorias anómalas SL5/SL6.
* Potencia media que ocasionalmente se interpreta como fuerte.
* Diferencia de saturación entre S1 y S2.
* Resbalones simulados que generaron swings involuntarios.
* Sonido todavía no comprobado de forma adecuada en el iPhone.
* Timing pendiente de calibración.
* Comportamiento del gesto durante rallies reales.
* Evento de interfaz en «Legibilidad» cuyo origen no pudo atribuirse.

Cada observación debe conservar su evidencia, severidad o impacto conocido, y el momento propuesto para volver a evaluarla.
No convertir observaciones abiertas en defectos resueltos por el solo hecho de aceptar S2.
6. Próxima etapa: preparar S3
Una vez completado el cierre e integración de S2, autorizo exclusivamente la preparación documental de S3.
Quiero una propuesta técnica acotada para:

* Sonido de contacto según calidad del golpe.
* Vibración/háptica diferenciada.
* Feedback visual de Perfecto, Válido y Fallido.
* Swing básico de raqueta, conforme al diseño vigente.
* Validación de que el feedback corresponde al contacto real y no solamente a la intención de golpear.
* Pruebas automatizadas y protocolo breve de validación en el iPhone.

La propuesta debe identificar dependencias, riesgos, criterios de aceptación y qué podemos comprobar automáticamente antes de requerir una sesión física.
Priorizar una experiencia de golpe clara y satisfactoria, sin ampliar el alcance.
No autorizo todavía implementar S3. Primero quiero revisar el diseño y el plan.
S4, First Rally, First Match y cualquier trabajo fuera del alcance indicado siguen sin autorización.
7. Criterio general de ejecución
Mantengamos nuestra metodología:

* Codex implementa cuando existe autorización.
* Claude realiza la revisión técnica independiente.
* TDD y regresiones según el alcance de cada cambio.
* Evidencia verificable y trazabilidad de decisiones.
* Validación en iPhone cuando corresponda.
* Aprobación del PO antes de ampliar el alcance.

No es necesario repetir pruebas físicas ya aceptadas si no existen cambios que las invaliden. Tampoco debemos declarar validaciones que no se hayan ejecutado.
Mi objetivo es avanzar sin buscar una perfección prematura, pero manteniendo el rigor que nos permitió completar First Ball y llegar a golpes reales en First Shot.
Decisión final del PO:

* S2: APROBADO CON CONDICIONES.
* M2-4: CONTROL VIGENTE.
* Potencia: SIN CAMBIOS.
* PR #30: FUSIÓN AUTORIZADA TRAS REVISIÓN FINAL SATISFACTORIA.
* PR #31: FUSIÓN DOCUMENTAL AUTORIZADA DESPUÉS DE #30 Y SU REVISIÓN FINAL.
* PR #32: EN ESPERA, SIN FUSIONAR.
* S3: SOLO PREPARACIÓN DOCUMENTAL.
* S4 y etapas posteriores: NO AUTORIZADAS.

Procede con este orden, informa cualquier bloqueo y no amplíes el alcance sin consultarme.
```

El encargo inmediato autoriza un solo commit de cierre documental en PR #30 y la sincronización del «Estado vigente» en las otras dos ramas. La revisión de `403f8ca` precede al nuevo commit de cierre, cuyo SHA debe revisar Claude antes de fusionar. La PR #31 queda para integración documental después de #30 y de su propia revisión final. La PR #32 queda en espera. La preparación documental de S3 empieza después del cierre e integración de S2. [Resumen previo al juicio del PO](2026-11-05-first-shot-s2-closing-summary.md) · [Registro de observaciones abiertas](2026-11-05-first-shot-s2-open-observations.md).


## 2026-11-04 — diagnóstico SL5/SL6 y plan S3, solo documentación offline

POautoriza diagnóstico enrama/worktreepropiosdesdeheadPR30 **dfffd58**: significado de mira enspec/propuesta, simulaciónP2offline deestadosdecontacto10–13conmétodopredeclaradoantesdecalcular,primerbote/laterales/outyopcionesconventajas/riesgos,sinelegir. AutorizaademásplanS3documental3.1–3.4sinimplementación;CodexlosagrupaenmismaPRdocumentalseparadade#30. Método: [predeclaración](../diagnostics/2026-11-04-sl56-p2-method.md),commitantesdelprimer cálculo.

Sesiónfísicaenpausa;noiPhone/relanzar/preparar16. Logs/evidenceoriginalesintactos/sololectura. Sinproducto/tests/defaults/SL/spec/criteriosnuevos;PRparaClaude,sinfusionar. PreparaciónPRODocurreporseparadoenworktreeS2y **noautoriza instalar**. Perfilconocidovence7nov17:58:46Z;POpresente/autorizaciónpropia/respaldo nuevoantesdecualquierinstalación. PR30sinfusionar/S3–S4/T8sinimplementación.


### Resultado documental del diagnóstico y plan S3 — 2026-11-04

Método 975b597/2141676 anterior al primer cálculo; auditoría de endpoints del arnés 4a55d80, sin cambio P2. Selección primaria de40recepciones/24Applied, sin extras: ejecución P2 original offline repetida con salida idéntica byte a byte. [Informe de trayectoria](2026-11-04-first-shot-sl56-trajectory-diagnosis.md), [método](../diagnostics/2026-11-04-sl56-p2-method.md), [derivado](../validation/evidence/2026-11-04-sl56-p2-offline.json), [reproducción](../diagnostics/2026-11-04-sl56-p2-reproduction.md). Primer frontis respetado, pero trayectoria posterior con laterales/out/escape según tabla y limitaciones; sin atribuir campaña nueva, Fail nuevo o elección del PO. Ninguna opción seleccionada.

[Plan S3 documental](2026-11-04-first-shot-s3-documentary-plan.md), tareas3.1–3.4, dependencias sobre S2/M2-4, TDD futuro y validación propia; no modifica plan/spec ni autoriza código. Sesión física permanece en pausa, sin acceso al iPhone/relanzamiento/preparación16. PR30 sin fusionar; entrega en PR documental propia para Claude, sin fusionar ni implementación S3–S4/T8.


### Entrega para revisión de Claude/PO — PR #31, 2026-11-04

[PR #31](https://github.com/rafaallendes-boop/squash-clash/pull/31) abierta, no borrador, sobre firstshot/s2-m24/PR#30, sin fusionar. Solo siete archivos documentales respecto de #30; métodos previos y datos derivados preservados, ningún producto/test/default/SL/spec/plan/criterio modificado. Ambas historias documentales se sincronizaron conservando los commits originales de predeclaración. Fuentes y primarios cotejados por hashes;141rutas de enlaces locales comprobadas. Ningún mensaje/revisión externa solicitado por herramientas.

Preparación PROD en #30, builda11fb32firmada/sininstalar,Logs/evidenceintactos. Pausa física permanece por instrucción del PO;16sinpreparar. Perfilvence7nov17:58:46Z/14:58:46Chile;las reservas deagenda son propuesta, disponibilidad aún pendiente. No implementación S3–S4/T8 ni elección de opción de trayectoria.


## Revisión Claude de PR #31 y decisión del PO — 2026-11-04

El PO comunica **APROBADO CON CORRECCIONES** de Claude sobre head **ef7edb3**. Claude verificó método fijado antes del cálculo, estados coincidentes con el JSONL primario (SHA-256 e8d3be96…,24contactos idénticos) y cifras coherentes con una cota balística independiente. Correcciones autorizadas, solo documentales: lectura conjunta desde el JSON existente y una sola entrada de estado vigente coherente entre PR#30/#31. La lectura da21/24 por unión; el caso11-08 tienez4,001148m (4,0m redondeados), precisión preservada sin afirmar≤4,0literal ni cambiar criterios. Los tres restantes quedan antes de4,875m.

**Decisión literal del PO:**

> Mantenemos la mira como punto del frontis. La dirección a investigar es una regla de golpe jugable en el solver, con ajuste visible como el de C4. Antes de elegir la regla, haz un mapa offline de factibilidad.

Alcance autorizado del mapa, exclusivamente diagnóstico: posiciones de los24contactos10–13, control con contactosSL1–SL4 de tanda1, rejilla predeclarada de puntos del frontis/P con ShotSolver/P2 originales sin cambios; primer frontis/out antes de bote/escape/primer bote(x,z)/mitad trasera/laterales; resumen porcontacto de zonas/P que permanecen en juego y alcanzan profundidad/fondo. Método y commit anteriores al cálculo. Sin elegir regla/valores de producto ni implementación; ninguna opción de producto autorizada.

**Prioridad literal:** las tandas2–4 de PR#30 antes del vencimiento **2026-11-07T17:58:46Z** van primero; el mapa solo se hace cuando no haya sesión física. La pausa anterior continúa hasta que el PO avise, sin tocar el iPhone, relanzar o preparar16. Perfil insuficiente→avisar antes de instalar. PR#30/#31 siguen sin fusionar; sin producto/tests/defaults/SL/spec/criterios nuevos.


### Entrega del mapa offline predeclarado — 2026-11-04

Método en commits **c000f85/7b5d943 anteriores al cálculo**. Sin sesión física activa y sin acceso al iPhone: 73 posiciones registradas (24 focales,49 controles),120 recepciones preservadas y **79.497 combinaciones** de rejilla congelada. ShotSolver/P2 originales, ninguna fuente de producto, test, default, SL, spec, plan o criterio modificada. Repetición completa con archivo idéntico byte a byte; primarios y fuentes protegidas conservan sus hashes.

[Informe del mapa](2026-11-04-first-shot-sl56-feasibility-map.md), [matrices por contacto](../diagnostics/2026-11-04-sl56-feasibility-matrices.md), [resumen derivado](../validation/evidence/2026-11-04-sl56-feasibility-summary.json), [método](../diagnostics/2026-11-04-sl56-feasibility-method.md), [reproducción](../diagnostics/2026-11-04-sl56-feasibility-reproduction.md). Las soluciones con ajustes existentes se separan de las solicitudes respetadas. Retorno dentro de cancha es intersección geométrica descriptiva, **no regla de golpe jugable aprobada**. Ninguna zona/P/corte se elige como producto; decidir regla y ajuste visibleC4 queda paraPO y nueva revisión.

Corrección21/24 calculada desde el JSON anterior sin volver a simular salidas históricas; precisión4,001148m conservada. Una sola entrada vigente deAGENTS, idéntica entrePR#30/#31 y anteriores históricas. La campaña física permanece en pausa hasta aviso delPO y conserva prioridad antes del7nov17:58:46Z; mapa no prepara16 ni habilita instalaciónPROD. PR#30/#31 sin fusionar; S3–S4/T8 sin implementación.


## Aprobación Claude y autorización de evaluar candidata, PO2026-11-04

El PO comunica **APROBADO sin correcciones** de Claude sobre PR#31/head **546b184**: verificó correcciones, estado vigente único e idéntico en ambas ramas, decisión literal y mapa conforme a c000f85/7b5d943. PR#31 sigue sin fusionar. La aprobación del mapa no aprueba una regla de producto.

**Decisión literal del PO:**

> Sí perfecto. Me parece esa regla candidata para evaluarla offline

La candidata es **bajar el punto conservando x y P pedidas**. Solo diagnóstico en arnés, sin modificar ShotSolver/P2: preservar base ya dentro de cancha; si no, búsqueda descendente predeclarada del punto más alto muestreado que da retorno dentro, límite tin+huella, sin inventar otra asistencia si no hay solución. Registrar interacción con SpeedRaised/TargetLowered. Contraste de bajar potencia, mismas73posiciones/rejilla/P0–1, frecuencias/resultados/Δy/cercanía tin/continuidad/dejadas/controles. [Método a confirmar antes de calcular](../diagnostics/2026-11-04-sl56-lowering-rule-method.md); paso propuesto0.05m y contrasteP0.05 solo diagnósticos, sin valores de producto elegidos.

Lectura **exploratoria atribuida aClaude, fuera del método del mapa y no formal**, comunicada porPO: enC(3.2,2.2),24focales conP0.2 dentro/cortos, P0.3fondo17/24, P≥0.4out; bajar punto misma x/P daría24/24dentro y fondo conP0.3–0.7/y0.7–1.5, sin cambio en49controles; bajar potencia17/24fondo. Se conserva como contexto, no resultados formales ni objetivo que el arnés deba alcanzar. El método formal distingue primer bote trasero de pared trasera y conserva retornos ya dentro aunque sean cortos.

Prioridad: tandas2–4 dePR#30 y restauraciones antes de2026-11-07T17:58:46Z; evaluación solo sin sesión física. No producto/tests/defaults/SL/spec/plan/criterios ni implementación; C4 solo nota de diseño. PR#30/#31 sin fusionar.


### Entrega de evaluación offline de la candidata — 2026-11-04

Método **140d0ca confirmado antes del primer cálculo**, sin refinamientos tras resultados. Mismas73posiciones,99puntos y11P:79.497solicitudes/brazo,238.491filas base/Y/P. ShotSolver/P2 originales sin cambios; dos ejecuciones con cincoTSV idénticos byte a byte; base completa idéntica al mapa aprobado. Auditadas380.725referencias de candidatos/195.936evaluaciones únicas,521protegidos yLogs/evidence3145archivos intactos. Ninguna build/suite/instalación/acceso al iPhone nuevo.

Y conserva exactamente17.403bases focales y49.044controles ya dentro; cambia8.722/4.203 y deja11/114sin solución encontrada. Retorno dentro Y26.125/26.136focales y53.247/53.361controles; bajarP26.099/26.136 y53.089/53.361. EnC, Y llega a mitad trasera24/24focales P0,3–1,0 ybajarP17/24; los539controles deC quedan intactos, pero toda la rejilla decontroles sí tiene cambios. Retornos bajos/suaves ya válidos preservados; solicitudes antesUnsolved pueden cambiar o seguir sin solución.

Hay márgenes casi nulos junto al tin enP baja y variaciones deTargetUsed entre vecinos (focal2,95m al variarP, controles hasta3,913845m al variarx), distinguiendo búsqueda externa y asistencias originales. Son datos descriptivos, sin criterio nuevo ni percepción humana medida. C4 solo nota; **no se elige regla/parámetros de producto ni se declara aprobada la candidata**. [Informe](2026-11-04-first-shot-sl56-lowering-candidate.md), [método](../diagnostics/2026-11-04-sl56-lowering-rule-method.md), [reproducción](../diagnostics/2026-11-04-sl56-lowering-rule-reproduction.md), [resumen](../validation/evidence/2026-11-04-sl56-lowering-summary.json). Se entrega para nueva revisión Claude/PO y se detiene.

Prioridad física vigente: tandas2–4 y restauraciones antes de2026-11-07T17:58:46Z/7nov14:58:46Chile; pausa hasta aviso delPO, sin tocar iPhone/relanzar/preparar16. InstalaciónPROD requiere autorización propia/respaldo nuevo; avisar antes deinstalar si no alcanza. PR#30/#31 sin fusionar; S3–S4/T8 sin implementación.

## Cierre efectivo de PR #30 y actualización documental de PR #31 — 5 de noviembre de 2026 (Chile)

El PO comunicó la aprobación final de Claude sin bloqueantes sobre el SHA exacto `8f8a7343a5ec932ac8bbc0d32f57ca89749c23dc` de PR #30 y autorizó la fusión y la preparación de #31. Antes de fusionar se verificaron head local y remoto, árboles limpios, PR abierta y no borrador, MERGEABLE/CLEAN, main en `8dd142dff2fcdc5d52871ac3c89733b4aff88fd6` y producto intacto desde `9131b1c`.

PR #30 quedó fusionada mediante **merge commit `60ae2d6ade661859ea80b71be96f52991d52ef5d`**, primer padre `8dd142dff2fcdc5d52871ac3c89733b4aff88fd6` y segundo padre **`8f8a7343a5ec932ac8bbc0d32f57ca89749c23dc`**. Su árbol **`d4c8dadff6a2034fb576c7ba187deca878ae50cd`** es idéntico al del head aprobado. La rama local y remota `firstshot/s2-m24` se conservó en ese SHA y sigue siendo la base de #32. Main local quedó sincronizada por fast-forward y limpia. El registro de cierre va en esta entrega de #31.

Se archivaron las ocho rutas ignoradas del worktree de #30 en **`Logs/archive-s2-m24-20261105/`** bajo el checkout de main. Incluye la evidencia de las cuatro tandas, builds, exports y el perfil anterior apartado en `Logs/prod-renewal-20261105/`. El inventario de rutas y los SHA-256 antes y después coincidieron, con **40.090 archivos regulares y 17.800.549.298 bytes**. SHA-256 del inventario `de2917a42bd166a1c6bd14f9cd8e4b18ed4c567574ef17fb265b153f72f974a0`. El worktree se retiró sin `--force` tras el cotejo.

PR #31 quedó basada en `main` y se actualizó con un merge de `origin/main`. Los conflictos se limitaron a `AGENTS.md` y este registro y se conservaron ambos historiales. El diagnóstico, sus conclusiones y la evidencia quedan idénticos a `84ad994` según cotejo SHA-256. El diff contra main es exclusivamente documental. **PR #31 queda abierta para revisión final de Claude.** PR #32 conserva su base y queda abierta en espera. Solo se sincroniza su «Estado vigente». Sesión física detenida.

## Cierre efectivo de PR #31 y entrega documental de S3 — 5 de noviembre de 2026 (Chile)

El PO comunicó la aprobación final de Claude sin bloqueantes sobre el SHA exacto `739ead8d2347fef3321c2c975fd2ec2db1d7ffcb` de PR #31. Claude confirmó base main `60ae2d6`, estado MERGEABLE/CLEAN, diff solo documental, diagnóstico/evidencia/informes SL5/SL6 idénticos a `84ad994`, resolución de conflictos que conserva ambos historiales y «Estado vigente» único e idéntico en #31 y #32. También verificó la fusión de #30 y su rama conservada. Su observación no bloqueante sobre los enlaces se aplica en esta entrega, con los enlaces del estado de S2 dirigidos a `main`.

Tras cotejar head local y remoto, PR abierta y no borrador, MERGEABLE/CLEAN, árboles limpios y main en `60ae2d6ade661859ea80b71be96f52991d52ef5d`, se fusionó #31 mediante **merge commit `a82b2607afa1c597e9af45d12b710a333e62189e`**, primer padre `60ae2d6ade661859ea80b71be96f52991d52ef5d` y segundo padre **`739ead8d2347fef3321c2c975fd2ec2db1d7ffcb`**. Árbol **`b47dbc4741c21e4b2dc904cab51145fa396a08db`** idéntico al head aprobado. Main local quedó sincronizada por fast-forward y limpia. Rama remota `codex/sl56-diagnostic-s3-plan` conservada. El worktree de diagnóstico se conserva con sus ignorados. El cierre se registra en la entrega S3.

El PO autorizó solo preparación documental de S3 después de fusionar #31. La rama y el worktree `firstshot/s3-design` parten de `a82b2607afa1c597e9af45d12b710a333e62189e`. La [propuesta técnica](2026-11-05-first-shot-s3-design-plan.md) cubre sonido, háptica, visual, swing básico y feedback ligado a la aplicación final, con TDD y protocolo físico propuestos. El tratamiento de Fallido sin contacto requiere decisión del PO por la diferencia entre el disparador estricto solicitado y el swish y la nota previstos en el diseño vigente. La calidad causal de 3.1, la superposición causal de 3.3 y los cambios de trayectoria quedan fuera. El [plan documental anterior](2026-11-04-first-shot-s3-documentary-plan.md) se conserva como antecedente.

Entrega para revisión de Claude, con implementación, pruebas, builds, instalación y sesión física pendientes de autorización propia. #32 conserva su base y su estado de espera, con solo la sincronización del «Estado vigente» de `AGENTS.md`. S4 y etapas posteriores siguen sin autorizar.

## Revisión documental de Claude de S3 en PR #33 — 5 de noviembre de 2026 (Chile)

El PO comunicó **CAMBIOS SOLICITADOS** en `d64466b`, solo documentales. Claude verificó el merge de #31 (`a82b260`, segundo padre exacto `739ead8` y árbol idéntico) y confirmó en #33 el alcance documental, la entrega única, el rollback y la deduplicación, la separación Editor/iPhone, las fuentes oficiales y la exclusión de calidad causal.

Claude corrigió un error de redacción propio del encargo anterior. El requisito de contacto aplicado final se refiere a las señales de impacto. Fallido conserva swish y nota «¡Al aire!» o «¡Tarde!» desde su resultado final sin contacto, sin háptica ni señales de contacto y con la misma entrega única. El swing al soltar o al confirmar queda como decisión del PO. La propuesta registra por separado los elementos de r2 y del plan pendientes de alcance y el efecto hipotético de `feedback.haptics` en el hash de defaults según R2-3, con P0 y los 41 valores heredados intactos.

[Propuesta corregida](2026-11-05-first-shot-s3-design-plan.md) para nueva revisión de Claude. Sin implementación, pruebas, builds, instalación ni acceso al iPhone. PR #33 abierta y sin fusionar. #32 conserva su base y su estado de espera, con solo la sincronización del «Estado vigente».

## Decisión del PO sobre el diseño de S3 y aprobación de Claude — 5 de noviembre de 2026 (Chile)

El PO comunicó que Claude **aprobó sin cambios pendientes** la propuesta corregida de PR #33 en `1c29fa7530cfe2526beb18a831db8c7dbe7bd1c6`. La revisión confirmó Fallido según r2 §4.6, las opciones de swing (a) y (b), las seis decisiones de alcance listadas y el efecto de `feedback.haptics` en el hash. [Head aprobado por Claude](https://github.com/rafaallendes-boop/squash-clash/pull/33/commits/1c29fa7530cfe2526beb18a831db8c7dbe7bd1c6).

A la pregunta «¿Aceptas mis recomendaciones para el diseño de S3?», el PO respondió literalmente:

> Sí, todas

1. **Fallido.** Entregar una sola vez el swish y la nota «¡Al aire!» o «¡Tarde!» desde el `Failed` final resuelto, sin vibración ni señales de contacto, conforme a r2 §4.6.
2. **Swing.** Elegir la opción (a). Iniciar la animación de intención al aceptar la orden Swing al soltar. Reservar la pose de impacto y el destello, cuando corresponda, para contacto aplicado. Continuar el swing al aire en `Failed` final. Deduplicar por identidad de orden. Una cancelación, un rechazo, `NoOrder` o un callback obsoleto no disparan ninguna señal.
3. **Nota de Válido.** Mantener «Adelantado» o «Atrasado», como en r2.
4. **Destello.** Mostrarlo solo en Perfecto con contacto aplicado.
5. **Perfiles.** Usar dos clips de impacto distintos, seco y contundente para `Perfect` y más suave para `Valid`, y dos estilos hápticos, firme y discreto, respectivamente. Usar un swish distinto para `Failed`.
6. **Modo silencio.** Respetar el silencio del sistema. En silencio se omite el audio y se mantienen la nota, el destello y el swing cuando correspondan. La háptica sigue la configuración del sistema y de la app.
7. **Interruptor `feedback.haptics`.** Incluir el interruptor previsto en el plan r2 3.2. Al implementarlo con autorización propia, añadir la clave a defaults y registrar el cambio de hash global según R2-3. Conservar P0-calib-1, sus 41 valores heredados y la SimulationKey.
8. **Protocolo físico.** Incluir S3-A, S3-B, S3-C y S3-D (18 recepciones en conjunto), además de S3-E porque se aprueba el interruptor de la app. La ejecución del protocolo, la build y la instalación requieren autorización propia.
9. **Alcance diferido.** Quedan pendientes y fuera de S3 los sonidos de frontis, laterales y tin, la configuración de audio de menor latencia y la comparación Default/Best Latency, la instrumentación de latencias y costes, las dos sesiones de 15 min, la calidad causal de 3.1 y la superposición causal de 3.3.

La adenda a spec v0.3, propuesta r2 y plan r2 sigue sugerida, pendiente del PO. El PO autorizó un único commit documental en #33 para registrar estas decisiones y sincronizar el «Estado vigente» en #32 únicamente en `AGENTS.md`. [Decisiones en la propuesta](2026-11-05-first-shot-s3-design-plan.md#decisiones-del-po).

PR #33 queda abierta para revisión de Claude. #32 sigue en espera. La implementación de S3, S4 y las etapas posteriores quedan pendientes de autorización.

## Cierre efectivo de PR #33 — 5 de noviembre de 2026 (Chile)

PR #33 se fusionó mediante `f3b6486e1f0c45b7c1c524f06e5f9a1c5b053176`. El primer padre es `a82b2607afa1c597e9af45d12b710a333e62189e` y el segundo exactamente `69fab56b10aac6c33241e038c439427b544286e6`, el head aprobado por Claude. El árbol `eeebeff3fc77096249daa3ef78676cdee7ea04ea` es idéntico al de ese head. Claude verificó la fusión. PR #32 no cambió (head `10f162f`, base `firstshot/s2-m24`). El cierre se registra aquí, en la entrega siguiente, sin commit aparte en `main`.

## Autorización de implementación de S3 — 5 de noviembre de 2026 (Chile)

A «Sí, prepárala», Claude redactó la autorización y el PO la envió. Texto autorizado:

> Autorizo la implementación de S3 conforme al diseño decidido en PR #33 (propuesta S3 y sus «Decisiones del PO»), una vez verificada por Claude la fusión de #33 en main.
> 
> Alcance — exactamente las nueve decisiones del PO:
> - Entrega única de resultado final por identidad de lanzamiento/intento: señales de contacto (sonido de impacto, háptica, destello en Perfect, pose de impacto) solo para Perfect/Valid aplicados en su versión final; Failed final resuelto con swish y nota «¡Al aire!»/«¡Tarde!», sin háptica ni señales de contacto; nada para cancelación, rechazo, NoOrder, fallo del sistema, NotEvaluable, versión obsoleta o callback de otro lanzamiento. Rollback y resimulación sin duplicar ni contradecir señales.
> - Swing opción (a): animación de intención al aceptar la orden Swing, pose de impacto solo con contacto aplicado, seguimiento al aire en Failed final, deduplicado por identidad de orden, limpieza en relanzamiento o rechazo tardío. Brazo y raqueta procedurales simples, sin rig ni collider, lado del contrato y cuerpo del lanzamiento, AX-1 intacto.
> - Nota de Válido con «Adelantado»/«Atrasado»; destello solo en Perfect; dos clips de impacto distintos y un swish distinto, originales o CC0, precargados y con entrada en PROVENANCE; dos estilos hápticos (firme/discreto) mediante puente iOS mínimo (UIImpactFeedbackGenerator en el hilo principal, generadores reutilizados, prepare() permitido al armarse el gesto, impacto solo desde la entrega final, no-op fuera de iOS); respetar el modo silencio sin cambiar ruta, buffer ni grabación.
> - Interruptor feedback.haptics: clave nueva en catálogo y DefaultPreset.json según R2-3. Informa el hash nuevo de defaults y el hash efectivo esperado de P0, que se declarará antes de cualquier precheck futuro. P0-calib-1 conserva sus bytes, sus 41 valores heredados y su proyección; SimulationKey intacta; ninguna otra clave o valor cambia.
> 
> Fuera de alcance: sonidos de frontis/laterales/tin, configuración de audio de menor latencia y comparación Default/Best Latency, instrumentación de latencias y costes, sesiones de 15 min, calidad causal (3.1), superposición causal y poses causales (3.3). Tampoco cambios de solver, Aim, SL, SL5/SL6, P2, física, M2-4, pullFullMm, cámara, ayudas de timing, tempo, spec, propuesta r2 ni planes; la adenda sigue pendiente. PR #32 no se toca salvo sincronizar el «Estado vigente» en AGENTS.md.
> 
> Método:
> 1) Rama y worktree nuevos desde main tras #33, por ejemplo firstshot/s3-feedback; una PR para revisión de Claude, sin fusionar.
> 2) Incrementos A (entrega de resultado final), B (audio y háptica) y C (nota, destello y swing), en ese orden. En cada uno: esqueleto compilable, RED válido por aserción observado y registrado, implementación mínima, GREEN focal y regresión completa fresca EditMode y PlayMode sin errores CS. Incluir al menos los casos de la tabla TDD de la propuesta, Failed_SwishNoHaptic de 3.2 y Swing_DriveBackhandAndAir de 3.3. No modificar aserciones existentes para pasar; si un caso existente choca con el diseño, detente y repórtalo.
> 3) Si en A aparece un conflicto con el contrato de contacto, el rollback o el intento único que exija cambiar el diseño, detente antes de B y repórtalo.
> 4) Al final: regresión completa fresca, check-release dev/prod limpio con gitDirty=false, símbolo háptico presente en prod, DevPanel y framework de pruebas excluidos, y archivos protegidos (P0, Po-1, last-active) intactos.
> 5) Informe docs/reviews/2026-11-XX-first-shot-s3-implementation.md con RED/GREEN por incremento, suites, hashes de defaults y de P0 efectivo esperado, procedencia de clips, lo comprobado en Editor y lo que queda para el iPhone. Registra en aprobaciones y AGENTS el cierre de #33 (merge commit) y esta autorización, y corrige los enlaces del «Estado vigente» a main.
> 
> No autorizados en este encargo: build para dispositivo, instalación, acceso al iPhone y ejecución del protocolo físico S3-A–E, que requieren autorización propia. El perfil vigente vence 2026-11-12T13:55:07Z (10:55 Chile); si la sesión física cae después, habrá que renovarlo antes de instalar. S4, First Rally, First Match y posteriores siguen sin autorizar. Entrega la PR lista para Claude y detente.

## Implementación de S3 por Claude con subagente — 5 de noviembre de 2026 (Chile)

Después de la verificación de #33, el PO cambió quién implementa. Literal:

> Me gustaría que estos incrementos los avances tu. Utiliza un sub agente con modelo sonnet para realizar la implementación y luego realiza la revisión. Y vamos avanzando. O dejame un prompt y abro una nueva conversación con Claude para que realice los avances

Claude (Opus) dirigió y revisó. Un subagente Sonnet implementó cada incremento en la rama `firstshot/s3-feedback`, desde `f3b6486`. Codex no inició S3. Se mantienen la autorización anterior y sus límites: TDD por incremento, sin build para dispositivo, sin instalación, sin acceso al iPhone y PR sin fusionar.

**Límite de independencia.** La revisión de Claude de cada incremento no es independiente, porque el mismo Claude dirigió la implementación. Antes de fusionar se recomienda una revisión independiente (otra sesión de Claude o Codex). Las decisiones que Claude tomó en nombre del PO están en el [informe](2026-11-05-first-shot-s3-implementation.md#decisiones-tomadas-por-claude-en-nombre-del-po).

**Resultado entregado.** Incrementos A, B y C y verificación final: 546/546 EditMode y 338/338 PlayMode, `check-release` dev/prod OK con `gitDirty=false` y puente háptico presente en prod. Defaults `c0d8ec7050e0`/107 claves. **P0 efectivo esperado `6ecfdfa78cbb`**, que debe declararse antes de cualquier precheck futuro. SimulationKey `166a96e319c7`, las proyecciones `0d39e6016bd8`/`8de3f2ecb217`, los bytes de P0-calib-1 y sus 41 valores siguen intactos. [Informe de implementación](2026-11-05-first-shot-s3-implementation.md).

La PR queda abierta y sin fusionar. La build, la instalación y el protocolo S3-A–E requieren autorización propia. El perfil vence el 2026-11-12T13:55:07Z. S4, First Rally, First Match y etapas posteriores siguen sin autorizar.


## Correcciones previas a la fusión de PR #34 — 5 de noviembre de 2026 (Chile)

**Revisión independiente.** Otra sesión de Claude revisó el head `f5bc7a2` de PR #34: **APROBADO CON OBSERVACIONES**, sin bloqueantes. Cuatro hallazgos importantes: (1) las notas informativas de resultados no elegibles contradecían la tabla de diseño; (2) `SwingView` y `PerfectFlashView` avanzaban durante la pausa; (3) faltaban pruebas de rechazos programados tardíos (`schedule-rejected`, `invalid-velocity`, `present-not-mappable`); (4) la prueba de independencia de la pelota solo alternaba `placeholders.visible`. El revisor volvió a ejecutar 546/546 EditMode y 338/338 PlayMode sobre `f5bc7a2`.

**Decisión literal del PO** (primer mensaje):

> Claude, gracias por la revisión independiente de la PR #34 sobre el head f5bc7a2.
>
> Como Product Owner, acepto tu evaluación técnica y los resultados de las suites. Sin embargo, antes de autorizar la fusión quiero resolver los hallazgos importantes identificados.
>
> Decisión general: APROBACIÓN TÉCNICA CON CORRECCIONES PREVIAS A LA FUSIÓN.
>
> No autorizo todavía el merge de la PR #34.
>
> 1. Notas de resultados no elegibles — MANTENER
>
> Decido conservar las notas informativas de S2:
>
> * SystemFault → «Fallo del sistema».
> * NotEvaluable → «No evaluable».
> * NoOrder → «Sin golpe».
>
> Considero que ayudan al jugador a entender qué ocurrió y evitan que una recepción termine sin explicación.
>
> Sin embargo, debemos distinguir explícitamente entre:
>
> * Feedback de impacto: sonido, háptica y destello asociados a un contacto aplicado.
> * Feedback de fallo: swish y nota correspondientes a un Failed final sin contacto.
> * Notas informativas: mensajes de estado que no representan un golpe ni una entrega de impacto.
>
> Las notas informativas no deben generar sonido de impacto, vibración ni destello.
>
> Solicito corregir la contradicción documental con la tabla aprobada, preservando la trazabilidad de la decisión original y registrando esta nueva decisión del PO.
>
> No quiero alterar el contrato de contacto ni la física.
>
> 2. Animaciones durante pausa — CORREGIR
>
> Estoy de acuerdo con el hallazgo.
>
> Si la simulación está pausada, la animación de swing y el destello deben quedar congelados, igual que la pelota y las ayudas visuales.
>
> No quiero que la raqueta vuelva al reposo mientras la pelota está detenida.
>
> Solicito:
>
> * Respetar Clock.Paused en SwingView y PerfectFlashView.
> * Conservar el estado visual durante la pausa.
> * Reanudar correctamente al continuar.
> * Añadir pruebas automatizadas para ambos componentes.
> * Evitar modificar el reloj global o el comportamiento de la simulación.
>
> Esta corrección debe ser mínima y localizada.
>
> 3. Cobertura de rechazos tardíos — COMPLETAR
>
> Solicito incorporar las pruebas faltantes identificadas:
>
> * Rechazo programado por invalid-velocity.
> * Rechazo por present-not-mappable.
> * Caso schedule-rejected.
> * Verificación de que la intención visual termina correctamente.
> * Ausencia de feedback de impacto para intentos sin contacto aplicado.
>
> Mantener la separación entre intención, resultado final y aplicación real del contacto.
>
> No introducir nuevas reglas de gameplay.
>
> 4. Prueba de independencia de las vistas — CORREGIR
>
> Coincido en que cambiar únicamente placeholders.visible no demuestra que las vistas estén realmente activadas o desactivadas.
>
> Solicito una prueba que compare ambas condiciones efectivamente y confirme que la presentación visual no altera el estado autoritativo de la pelota.
>
> La prueba debe detectar una regresión real y no limitarse a una comparación que pueda pasar con las mismas vistas activas.
>
> 5. Observaciones menores
>
> * feedback.haptics: mantener por ahora la lectura por lanzamiento, pero registrar claramente que el cambio requiere un nuevo lanzamiento y no se aplica inmediatamente al contacto en curso. No cambiar el contrato de configuración sin una propuesta específica.
> * Entrega única: revisar el caso inverso en que una identidad recibe primero un resultado no elegible y después uno elegible. Documentar el comportamiento esperado y añadir cobertura si corresponde, sin introducir una segunda entrega ni alterar el controlador sin autorización.
> * Corregir las imprecisiones del informe sobre literales, RED de Notes_MatchResult y cobertura de SlipGestures.
> * Añadir una prueba del interruptor háptico apagado durante Prepare, si puede realizarse sin ampliar el alcance.
>
> 6. Sensación del swing y háptica
>
> Mantengamos provisionalmente el desfase de 0,06 segundos de la raqueta y la preparación háptica existente.
>
> No autorizo recalibrarlos basándonos únicamente en una revisión estática.
>
> Quiero evaluar en el iPhone:
>
> * Si la raqueta acompaña visualmente el contacto.
> * Si Perfect y Valid se distinguen claramente.
> * Si el swing se siente natural.
> * Si el sonido y la vibración están sincronizados.
> * Si hay oclusiones o distracciones visuales.
>
> Cualquier ajuste perceptivo posterior deberá basarse en esa validación.
>
> 7. Próximos pasos
>
> Solicito que transmitas estas decisiones a Codex para preparar las correcciones acotadas dentro de la PR #34, siguiendo nuestra metodología TDD.
>
> Antes de implementar, identifica si alguna corrección implica una decisión de arquitectura o modificación de contrato que no esté cubierta por estas autorizaciones. En ese caso, detente y consúltame.
>
> Una vez completadas:
>
> 1. Codex entrega el nuevo commit y la evidencia de pruebas.
> 2. Claude revisa independientemente el nuevo head.
> 3. Se verifica que las regresiones estén en verde.
> 4. Se confirma que P2, M2-4, el solver, la trayectoria, el timing y los presets protegidos permanecen intactos.
> 5. Se comprueba que el árbol y las configuraciones queden restaurados.
> 6. Se presenta la PR nuevamente para mi aprobación final.
>
> No fusionar todavía la PR #34. No iniciar S4, First Rally ni trabajos fuera de S3.
>
> El objetivo es cerrar S3 con una implementación sólida, sin convertir esta etapa en un rediseño innecesario.
>
> Avancemos con estas correcciones y luego pasamos a la validación física en el iPhone.

**Cambio de método, literal del PO (mismo día):**

> Realiza tu los cambios, con un Sub agente que haga los cambios y luego tú revisas. Sigamos avanzando juntos y luego códex retoma

**Método.** Claude (Opus) dirige y revisa. Un subagente Sonnet implementa. Codex retoma después. **La revisión de Claude de estas correcciones vuelve a no ser independiente.** Se recomienda una revisión independiente (Codex u otra sesión de Claude) del nuevo head antes de fusionar. PR #34 sigue sin fusionar. S4 y First Rally no están autorizados. Detalle en la sección E del [informe de implementación](2026-11-05-first-shot-s3-implementation.md#e-correcciones-previas-a-la-fusión).

## Revisión de las correcciones de PR #34 y observaciones 1 y 3 — 5 de noviembre de 2026 (Chile)

**Revisión.** Otra sesión de Claude revisó el delta `f5bc7a2..47be753`: **APROBADO CON OBSERVACIONES**, sin bloqueantes ni hallazgos importantes. Su independencia es parcial: no hizo estas correcciones, pero dirigió la implementación original de S3 hasta `f5bc7a2`. Volvió a ejecutar 546/546 EditMode y 353/353 PlayMode sobre `47be753`. Cuatro observaciones menores:

1. el nombre de la prueba de inalcanzabilidad de `present-not-mappable` afirma más de lo que comprueba;
2. el rechazo emitido por reflexión deja un estado que el host real no produce (artefacto de prueba ya declarado);
3. el bucle de intención de `ScheduledThenRejected` se ejecuta una sola vez y podría no ejecutarse sin que nada falle;
4. un golpe aplicado durante la pausa suena y vibra, y deja el destello congelado; la raqueta espera a reanudar y «Paso» no avanza las vistas.

**Decisión del PO, literal:**

> Corregir 1 y 3

**Aplicado.** Solo cambia `S3LateRejectionTests.cs`: la prueba de la observación 1 se renombra y su comentario se aclara, y la observación 3 suelta en `Star()-.035` con una aserción de al menos una iteración. Claude hizo los cambios directamente, sin subagente Sonnet, por su tamaño; se declara como desviación del método y sin revisión independiente. Las observaciones 2 y 4 quedan registradas sin cambio; la 4 pasa a lo pendiente para el iPhone. Detalle y verificación en la sección F del [informe de implementación](2026-11-05-first-shot-s3-implementation.md#f-revisión-de-las-correcciones-y-observaciones-1-y-3). PR #34 sin fusionar hasta la decisión del PO. S4 y First Rally sin autorizar.

## Cierre efectivo de PR #34 — 5 de noviembre de 2026 (Chile)

El PO autorizó la fusión con «Sí, fusionar». Antes de fusionar se comprobó que el head local y el remoto eran exactamente `067ca9184af2a8256e49519a37098bc7ce44e34b`, que la PR estaba abierta, no era borrador y no tenía conflictos, y que ambos árboles estaban limpios. Merge **`adb8c8fd5fcf2c081d0633670a3faa7b4d6af677`** (2026-11-06T02:12:30Z), segundo padre exactamente `067ca91`, árbol `d63c94c5227c98fa4382fd9b30f83018e8e47805` idéntico al del head aprobado. Main sincronizada y limpia.

Worktree `firstshot-s3-feedback` retirado sin `--force` después de mover sus ocho rutas ignoradas (ledger `.superpowers/`, `BuildInfo.json` y su `.meta`, `Builds/`, `Library/`, `Logs/`, `UserSettings/` y `scripts/device.env`) a `Logs/archive-s3-feedback-20261105/artifacts` del checkout principal. Inventario por ruta, tamaño y SHA-256 idéntico antes y después: 30.338 archivos, 4.608.523.699 bytes, sin enlaces simbólicos; SHA-256 del inventario `17b2234fd067f644c91eb0b226683b35c15e8b5a3fef3f80e04d73fc8746cd3c`. Rama `firstshot/s3-feedback` conservada en local y remoto. El cierre se registra en esta entrega, sin commit aparte en main.

**La fusión no acepta S3.** Su aceptación funcional depende de la validación física.

## Autorización de la validación física de S3 — 6 de noviembre de 2026 (Chile)

**Decisión literal del PO:**

> Claude, gracias por el resumen de First Ball, S1, S2 y S3, y por consolidar el estado del proyecto.
> Como Product Owner de Squash Clash, confirmo las siguientes decisiones.
> 1. S3: autorizo la validación física en el iPhone
> Apruebo ejecutar exclusivamente la Fase 1 propuesta, correspondiente a la validación física de S3, con sus 21 recepciones distribuidas en los bloques S3-A, S3-B, S3-C, S3-D y S3-E.
> El objetivo es comprobar la sensación real del golpe, especialmente:
>
> * Diferencia perceptible entre Perfect y Valid.
> * Sincronización del sonido y la vibración con el contacto.
> * Comportamiento de Failed sin contacto.
> * Naturalidad del swing y posible desfase de 0,06 segundos.
> * Legibilidad del destello y de las notas.
> * Comportamiento con modo silencio y háptica desactivada.
> * Correcta congelación y reanudación del feedback durante pausas.
>
> Quiero conservar las preguntas literales y las respuestas originales, sin reinterpretarlas a partir de métricas posteriores.
> 2. Preparación y seguridad
> Autorizo preparar, compilar e instalar la build dev de S3 en mi iPhone, una vez comprobados el perfil de firma, la fuente exacta y los requisitos de instalación.
> Antes de comenzar:
>
> * Respaldar P0-calib-1, Po-1 y last-active.
> * Confirmar el hash esperado `6ecfdfa78cbb` y cero cambios pendientes.
> * Verificar que la build corresponde a S3 fusionado en `main`.
> * Comprobar que la aplicación inicia y que la configuración protegida está intacta.
>
> Al terminar, restaurar y verificar los tres archivos byte a byte contra sus respaldos, documentar las incidencias y preparar el informe para revisión independiente.
> No modificar parámetros de física, potencia, timing o trayectoria durante esta validación.
> 3. Cierre documental
> Autorizo incorporar el cierre de la PR #34 a AGENTS.md y al registro de aprobaciones, conservando la trazabilidad de las revisiones y correcciones realizadas.
> La implementación de S3 está fusionada, pero su aceptación funcional definitiva queda pendiente de mis pruebas en el iPhone.
> 4. S4 y decisiones diferidas
> No autorizo todavía S4 ni la implementación de las cinco piezas diferidas.
> Después de la validación física de S3 revisaremos:
>
> * Sonidos de superficies.
> * Audio de baja latencia.
> * Instrumentación de latencias.
> * Calidad causal.
> * Superposición causal.
>
> La PR #32 debe permanecer abierta, desactivada y sin fusionar. Su evaluación será una decisión independiente.
> 5. Criterio de cierre
> Una vez terminados los cinco bloques, quiero recibir:
>
> 1. Resultados objetivos de cada bloque.
> 2. Mis respuestas literales.
> 3. Problemas observados y su impacto.
> 4. Verificaciones de configuración y restauración.
> 5. Recomendación de Claude: aceptar S3, aceptarlo con condiciones o solicitar correcciones.
>
> No iniciar trabajos adicionales de producto hasta mi decisión posterior.
> Decisión final: validación física de S3 AUTORIZADA. S4 y First Rally NO AUTORIZADOS.
> Procedamos de forma incremental, manteniendo el rigor de las etapas anteriores.

**Operación.** Claude opera la sesión: build, instalación, exportación, precheck y restauración. Rama documental `firstshot/s3-validation` desde `main` en `adb8c8f`. La predeclaración se confirma en Git antes del primer bloque: [predeclaración de la sesión S3](../validation/2026-11-06-first-shot-s3-physical-predeclaration.md). Esa predeclaración no tuvo revisión independiente previa; se declara como desviación y queda para la revisión del informe. El objetivo de pausas no forma parte de los bloques S3-A–E aprobados; la predeclaración lo propone como observación opcional fuera de cuota, sujeta a la decisión del PO.

## Detención de S3 y corrección puntual del audio — 6 de noviembre de 2026 (Chile)

**Contexto.** S3-A se jugó sin la marca de DEV y el PO no oyó ningún sonido con el timbre activo. Diagnóstico estático de Claude: `Main.unity` no tiene `AudioListener`. Sesión detenida antes de S3-B. [Informe de la sesión](2026-11-06-first-shot-s3-physical-validation.md).

**Decisión literal del PO** (recibida primero truncada en el punto 4 y después completa; se registra la versión completa):

> Claude, gracias por detener la sesión y documentar el problema.
> Como Product Owner de Squash Clash, apruebo la recomendación de detener S3 después del bloque S3-A.
> Decisión: AUTORIZO UNA CORRECCIÓN PUNTUAL DEL AUDIO.
> 1. Diagnóstico
> Considero que la ausencia de `AudioListener` es una causa probable del silencio observado, pero debemos confirmarla mediante pruebas.
> Los resultados de S3-A son útiles: el sistema registró correctamente los ocho intentos, las entregas de feedback y los contactos aplicados.
> Conservemos el bloque como evidencia parcial, incluyendo la desviación por ausencia de la marca DEV, las respuestas literales y el golpe adicional fuera de cuota.
> No debe contabilizarse como un bloque S3-A válido y completo.
> 2. Corrección autorizada
> Autorizo a Codex a preparar una PR independiente para corregir exclusivamente el problema del audio.
> Solicito:
>
> * Verificar que la escena activa no dispone de un `AudioListener` funcional.
> * Añadirlo a la cámara correspondiente, siempre que esa sea la solución mínima adecuada.
> * Garantizar que exista un único listener activo.
> * Incorporar una prueba automatizada que reproduzca el defecto y falle antes de la corrección.
> * Verificar que el componente permanezca presente y activo en las builds DEV y PROD.
> * No modificar clips, volúmenes, timing, háptica, física, M2-4 ni lógica de contacto.
>
> Si la investigación demuestra otra causa, detenerse y presentar el diagnóstico antes de ampliar el alcance.
> 3. Revisión independiente
> Una vez implementada la corrección:
>
> 1. Codex entrega la PR y los resultados de las pruebas.
> 2. Claude revisa independientemente el último commit.
> 3. Se ejecutan las regresiones correspondientes.
> 4. Se verifica que los hashes y presets protegidos no cambien.
> 5. Se comprueba que la modificación no introduce listeners duplicados ni regresiones en las escenas.
>
> No fusionar sin mi aprobación final.
> 4. Validación física
> Después de aprobar la corrección, autorizo preparar una nueva build para verificar el audio en el iPhone.
> Primero quiero un smoke test breve que confirme que se escucha un sonido de impacto con el timbre activado.
> Si el audio funciona correctamente, podremos repetir los cinco bloques S3-A–E con el protocolo predeclarado y la marca DEV correspondiente.
> No reutilizar las recepciones de hoy como parte de la nueva cuota.
> 5. Restauración pendiente
> Antes de continuar, completar la verificación de P0 en un proceso nuevo y registrar el resultado.
> Mantener los respaldos y comprobar nuevamente la restauración al finalizar la próxima sesión.
> 6. Alcance
> La corrección autorizada se limita al audio y a las pruebas necesarias.
> No autorizo cambios adicionales en S3, calibraciones, PR #32, S4 ni First Rally.
> Decisión final: S3 DETENIDO; CORRECCIÓN PUNTUAL DE AUDIO AUTORIZADA; NUEVA VALIDACIÓN SUJETA A REVISIÓN Y APROBACIÓN.
> Procedamos con la corrección mínima y su revisión independiente.

**Registro.** S3-A queda como evidencia parcial, no válida. Claude entregó al PO el encargo para Codex. La verificación de P0 en proceso nuevo se completó a las 2026-11-06T12:27:11Z: `6ecfdfa78cbb`, cero pendientes y tres archivos idénticos al respaldo. Los respaldos se conservan en `Logs/evidence/s3-physical-20261106/`.

## Cierre de la corrección del audio (PR #35) y continuación de S3 — 6 de noviembre de 2026 (Chile)

Claude aprobó la revisión independiente del head `636aa9e813fd89a2a8b4a240577cebcde7e985c0`. Reprodujo el RED en `d4f2628` y verificó 546/546 EditMode, 354/354 PlayMode y un único listener habilitado en la cámara principal de los exports DEV y PROD. Evidencia local en `Logs/archive-review-pr35-20261106/`. La PR #35 se fusionó en `27058e82674f633879a7a4f7cabc4f1306a6db6b` con segundo padre igual al head revisado y árbol idéntico `30982184171c34b0bf015b1bf9343b13ba2a32d3`. [PR #35](https://github.com/rafaallendes-boop/squash-clash/pull/35).

El PO encarga a Codex la continuación autorizada en los puntos 4 y 5 de la decisión anterior. Primero se prueba el sonido con un impacto aplicado, fuera de cuota. Solo después de la confirmación explícita del PO se inicia S3-A–E desde cero. El PO autorizó la observación de pausa fuera de cuota después de S3-E: «Sí, incluirla». [Adenda predeclarada](../validation/2026-11-06-first-shot-s3-physical-predeclaration.md#adenda-del-6-de-noviembre-de-2026--prueba-breve-de-sonido-y-repetición-con-audio-corregido).

## Cambio de operador para terminar S3 — 9 de noviembre de 2026 (Chile)

Tras la pausa del 6 de noviembre («Dejemos en pausa mientras y después retomamos» y «Dejemos la restauración para cuando retomemos»), Claude preguntó al PO quién operaría la sesión física de S3. Respuesta literal del PO: **«Claude, ahora»**.

**Alcance.** Claude retoma la continuación autorizada el 6 de noviembre en el mismo punto, en la rama `firstshot/s3-validation` y con la misma evidencia: S3-D y S3-E según la predeclaración (§4, §6–§8 y la adenda), la observación de pausa fuera de cuota ya autorizada («Sí, incluirla») y la restauración final con cotejo de los tres archivos. Las cuotas válidas de S3-A, S3-B y S3-C registradas por Codex se conservan sin repetirlas ni reinterpretarlas. El intento de S3-D detenido en preparación el 6 de noviembre no tiene frontera y no cuenta. Sin build ni instalación nuevas: queda instalada `0.1.0+097eab9 dev`, con perfil incorporado vigente hasta el 2026-11-12T13:55:07Z (10:55 Chile). Sin cambios de producto, hashes esperados, criterios ni umbrales. S4, First Rally y PR #32 siguen sin autorizar. El informe se entrega en una PR para revisión, sin fusionar.

## Autorización del PO para el MVP de AutoQA — 6 de noviembre de 2026, Chile

Decisión literal del PO, Puerta 0 satisfecha.

> GO CON CONDICIONES — AUTORIZO IMPLEMENTAR EL MVP DE AUTOQA. Apruebo la opción B: arnés PlayMode determinista dentro de Unity Editor, utilizando el juego real y sin modificaciones de producción. Apruebo expresamente el contrato de observables y escrituras de la sección C.4 del estudio técnico, incluyendo sus restricciones y APIs prohibidas. Codex será el ejecutor principal del MVP. Claude será el revisor técnico independiente. No quiero dos implementaciones paralelas. Unity y el oráculo determinista deben decidir PASS/FAIL, nunca el modelo de lenguaje.

El PO prefiere Luna 6 Low para nivel 1, QA rutinario y resumen de artefactos. Prefiere Luna 6 Medium para nivel 2, investigación de fallos, precondiciones, repetibilidad y errores de ejecución a partir de evidencia, sin cambiar producción ni criterios. Para nivel 3, problemas de contratos, física, timing, arquitectura, integridad o alcance, prefiere Sol 6.1 High u Opus 5.5 High. La elección depende de la disponibilidad de cada herramienta. Los modelos operan fuera de la ejecución y no sustituyen la revisión independiente.

La Puerta 1 exige una PR abierta y lista para Claude, coordinada por el PO. Cualquier fusión requiere autorización explícita posterior a la revisión, cotejo del head local y remoto, PR abierta sin borrador ni conflictos, y árboles limpios. El merge commit debe tener como segundo padre el head aprobado. Luego se sincroniza main y se retira el worktree sin forzar, previa preservación de ignorados.

El estudio recibido coincide antes y después de la copia, SHA-256 `fac103f0bbdc5a3777103de8d59118a52707fea2eb83aabede3fb03410b386b0`. [Estudio técnico aprobado](2026-11-06-autoqa-technical-study.md).


## Continuación del MVP de AutoQA — decisiones del PO, 2026-11-08 (Chile)

Literales del PO y consecuencias comunicadas en el encargo de continuación:

Registra ambas literalmente en el registro de aprobaciones vigente, como primer commit documental de esta continuación, con fecha 2026-11-08 y atribuidas al PO.

1. Margen y línea de lanzamiento: «Sí, a SL5». Respuesta a la propuesta de Claude de exigir el margen de dos pasos reales también respecto de KIn/KOut y de mover a SL5 los escenarios con Δ=0. Consecuencias:
- El margen de dos pasos reales (25,641 ms con los defaults actuales) se exige respecto de todas las fronteras: bordes de asistencia y validez, y KIn/KOut.
- M24-PERFECT-SL1 pasa a M24-PERFECT-SL5 y el canario CANARY-WRONG-EXPECTATION usa el mismo gesto en SL5, ambos con Δ=0. Según la parada, en SL5 la ventana alrededor de T* va de −239,5 a +245,9 ms; recalcúlala con los valores reales y regístrala.
- M24-NOPULL-SL1 y M24-REJECTED-SL1 siguen en SL1.
- M24-VALID-EARLY-SL1 (Δ=−70 ms) y M24-FAILED-LATE-SL1 (Δ=+170 ms) siguen en SL1, porque ya cumplen el margen. Compruébalo con los valores reales.
- El oráculo puede leer KIn/KOut del shot-plan registrado en el JSONL del juego, solo para invalidar un escenario por margen, nunca para decidir el resultado esperado. Sigue prohibido llamar a ContactPlanner, ContactContract o ShotSolver.
- La línea se elige con Tuning.Set("practice.launch", n), que ya está en la lista blanca. El hash efectivo cambia con la línea: declara el hash esperado de SL1 y el de SL5, calculados una vez desde los defaults y registrados, y verifícalos en cada intento.
2. Hash inesperado: «Nivel 3». El prompt definitivo se contradecía: listaba el hash inesperado como precondición INCONCLUSIVE y como fallo de integridad. Decisión: un hash efectivo inesperado es fallo de integridad, con integrity.hashesVerified=false, escalationLevel 3 y salida 3. El intento puede seguir marcándose INCONCLUSIVE, pero el gate y el nivel los decide la integridad. Consecuencia para el criterio 11: el INCONCLUSIVE forzado que debe dar salida 2 tiene que usar una precondición que no sea de integridad, por ejemplo el tamaño de pantalla.

Confirmación adicional del PO del mismo día («Sí, confirmo»): un canario que no da FAIL es fallo de integridad (nivel 3), salvo que la corrida no haya llegado a ejecutarse (executionError o gate ERROR). En ese caso un canario sin resultado no cuenta como integridad. Un canario en PASS da siempre nivel 3. Hazlo así en la regla de triaje y cúbrelo con TDD.

Se autoriza continuar desde la puerta POC en `autoqa/mvp`, mismo worktree y PR #36, sin fusionar. La rama operadora #37 es conocida y queda fuera de esta implementación. Ante una parada del contrato, documentar, hacer commit/push y detenerse; el PO no estará disponible. Sin iPhone ni cambios de producción.


### Resultado de la continuación — AUTOQA-MVP-02 (2026-11-08, Chile)

No es una aprobación del PO ni del MVP. Se resuelve AUTOQA-MVP-01 con las decisiones anteriores, pero la POC revela Scheduled=true/false/true en tres repeticiones de la misma intención SL5 Δ=0. El contrato exige detenerse ante falta de determinismo; no se cambian escenarios, oráculo ni producto y no se repite Unity para buscar un resultado favorable. Se conserva el fixture bfe1474, evidencia primaria y auditoría offline. Escalamiento técnico documental 3; `autoqa.sh`, sus salidas y el MVP completo siguen pendientes. [Informe de continuación y decisión pendiente](2026-11-08-autoqa-mvp.md). PR #36 abierta/lista/sin fusionar para revisión independiente coordinada por el PO; ninguna nueva autorización inferida.


## Continuación AutoQA tras AUTOQA-MVP-02 — decisión del PO, 2026-11-09 (Chile)

El PO autoriza ejecutar el prompt preparado por Claude: «Si, ejecuta esto. Es el prompt que armó claude». Decisión y consecuencias literales comunicadas en ese prompt:

Regístrala literalmente en el registro de aprobaciones vigente, como primer commit documental de esta continuación, con fecha 2026-11-09 y atribuida al PO.

**«Sí, ±10 ms».** Respuesta a la pregunta de Claude: «¿Cambiamos PERFECT a Δ=−10 ms y añadimos otro caso PERFECT a +10 ms?», tras el diagnóstico anterior. Consecuencias:

- `M24-PERFECT-SL5` pasa a **Δ=−10 ms**. Esperado: Swing/Perfect/Applied, **`Scheduled=true`**, una presentación Perfect y el resto de expectativas de PERFECT sin cambios.
- Se añade **un segundo caso PERFECT en SL5 con Δ=+10 ms**. Esperado: Swing/Perfect/Applied, **`Scheduled=false`**, una presentación Perfect y las mismas expectativas restantes. Elige un nombre coherente con los existentes; propuesta: `M24-PERFECT-EARLY10-SL5` y `M24-PERFECT-LATE10-SL5`. Documenta el que uses.
- `CANARY-WRONG-EXPECTATION` usa el mismo gesto que el caso Δ=−10 en SL5, con el esperado deliberadamente falso ya aprobado (Failed).
- **Δ=0 queda como frontera interna de `Scheduled`.** No se le aplica la regla de dos pasos. Se le exige una holgura numérica mínima de **1 ms**; ±10 ms la cumple. El margen de dos pasos frente a −A/+A, −V/+V y KIn/KOut sigue exigido sin cambios.
- Los otros cuatro escenarios no cambian: VALID-EARLY y FAILED-LATE en SL1 con −70 y +170 ms, NOPULL y REJECTED en SL1.
- El hash esperado de SL5 (`55a85f500cf1`) y el de SL1 (`c0d8ec7050e0`) no cambian, porque Δ no es un parámetro.
- Recalcula y registra la tabla de distancias con los valores reales. Como referencia: −10 queda a 30/50 ms de −A/+A, 90/110 ms de −V/+V y 10 ms de la frontera interna; +10 queda en el simétrico.

Diagnóstico de Claude, solo lectura: variación del origen absoluto y comparación corrected<=target coherentes con Scheduled true/false/true. No se rastreó la conversión del tiempo en InputSystem; no es causa demostrada ni evidencia de defecto de producto. La regla de dos pasos no cubre la frontera interna Scheduled=0; allí rige la holgura PO de 1 ms. Misma rama/worktree y PR #36 abierta/lista/sin fusionar. Ante nueva parada: registrar, commit/push y detenerse.


### Entrega AutoQA del 9 de noviembre de 2026 — revisión independiente pendiente

Implementación y comprobaciones autorizadas ejecutadas por Codex, sin nueva aprobación del PO inferida. Predeclaración 9c476bc; POC nueva 2/2 con seis instancias ±10, Scheduled estable true/false. Fuente final 491e10d: corrida final 18/18 PASS, canario FAIL, RESULT=PASS/salida 0/nivel 1 en 14,497 s; suite propia 76/76 y regresión fresca 546/546 EditMode, 354/354 PlayMode. Check-release dev/prod OK/gitDirty=false, sin AutoQA en Players. Salidas forzadas 1/2/3/4 verificadas y cambios restaurados byte a byte. Primer export falló por BUNDLE_ID ausente en entorno local; valor temporal del proyecto sin cambiar producto, entorno restaurado y fallo preservado. 312 archivos protegidos idénticos a main; sin iPhone/build nativa/instalación ni cambios en #32/#37/S3-validation. [Informe, decisiones, huellas y consumibilidad](2026-11-09-autoqa-mvp.md). PR #36 abierta/lista para Claude coordinado por el PO, sin fusionar. AUTOQA-MVP-01/02 resueltos; ninguna parada nueva. Detenerse tras entregar y esperar revisión/PO.


## Correcciones de revisión de AutoQA PR #36 — decisión del PO, 2026-11-09 (Chile)

Decisión literal del PO: **«Sí, redáctalo»**. Respuesta a la pregunta de Claude **«¿Redacto el prompt para que Codex corrija estos dos puntos?»**, tras su revisión independiente de bc82c3ee570df837142f423d736f6bf893ef7e40: **CAMBIOS SOLICITADOS**. Autoriza corregir con TDD el canario tras FAIL/determinismo y la restauración selectiva de los dos transitorios PlayMode. No cambia ninguna otra decisión.

Claude comunicó una corrida limpia con PASS/salida0/nivel1, 76/76 y18/18, canario FAIL; evidencia local Logs/autoqa/20261109T130148Z-bc82c3e-10259. Tuvo que restaurar GraphicsSettings/QualitySettings. Continuar en mismo worktree/rama/PR #36, sin fusionar; revisión posterior de Claude coordinada por el PO. No cambiar escenarios/expectativas/márgenes/tolerancias/determinismo, producción ni device.env. Ante nueva parada, documentar/commit/push y detenerse.


### Entrega de correcciones AutoQA — 2026-11-09 (Chile), revisión pendiente

Codex aplica opción A: canario ejecutado tras FAIL normal/determinismo; omitido ante guardia roja/executionError, sin cambiar Report.Triage. RED de ejecución salida3/nivel3 con canario omitido y RED unitario82/85→GREEN9/9; inyecciones GREEN de oráculo/determinismo dan canario FAIL y salida1/nivel2. Script restaura solo GraphicsSettings/QualitySettings inicialmente limpios en finally; registro/SHA, preservación de cambios previos y ERROR4/dirty-after-run para diferencias posteriores. Observación opcional realizada: summary.harness.json e informe previo con referencias SHA ante ERROR. Fuente a537236: **dos corridas consecutivas PASS/0/nivel1/dirty=false**,85/85 cada una,18/18/canario FAIL, árbol limpio sin limpieza entre/después. Regresión fresca546/546 EditMode y354/354 PlayMode.312protegidos iguales a main; escenarios/expectativas/márgenes/tolerancia/predeclaración y device.env intactos. Check-release no repetido: solo tests/script/docs, sin efecto en Players. [Informe nuevo y huellas](2026-11-09-autoqa-mvp-corrections.md). PR #36 abierta/lista/sin fusionar para Claude coordinado por PO; no es aprobación ni fusión. Ninguna parada nueva.

## Cierre de AutoQA PR #36 y alineación de la PR #37 — decisiones del PO, 2026-11-09 (Chile)

**Cierre de #36.** Claude revisó las correcciones en el head `7cd5a18b04618ee0cc8d6c9efdd79ac8a5a153dc` y las aprobó, con una observación menor. Volvió a correr `scripts/autoqa.sh mvp` dos veces seguidas: PASS, salida 0, nivel 1, 85/85, `dirty=false` y árbol limpio. A la pregunta «¿Autorizas fusionar la PR #36 tal como está?», el PO respondió literalmente **«Sí, fusionar»**.

Condiciones comprobadas antes de fusionar:
- PR abierta, no borrador, `MERGEABLE`/`CLEAN`, sin CI;
- head local y remoto exacto `7cd5a18`, sin commits nuevos, y ambos árboles limpios.

Se fusionó con `--match-head-commit`. Merge commit **`e18d3e169beb01569d11b1fbbce7f26d130670a1`**, segundo padre exactamente `7cd5a18` y árbol `538696b6b26857cfb31f0358ed49476187f7ca23`, idéntico al head aprobado. Main local sincronizada y limpia. La rama `autoqa/mvp` y el worktree de Codex se conservan, porque los gestiona Codex. El cierre se registra aquí, dentro de la siguiente entrega, sin un commit aparte en main.

Observación menor abierta, sin corrección en #36: si coinciden un fallo de integridad y `dirty-after-run`, el script sale con 4 aunque conserva el nivel 3. Eso contradice la precedencia 3 > 4. *(Corregido el 2026-11-09, ver abajo: con `v4` el verificador no lo detectaba en general; lo detecta desde `v5`.)*

**Alineación de #37.** A la pregunta «¿Alineo ahora la PR #37 con el arnés fusionado?», el PO respondió literalmente **«Sí, alinear #37»**. Autoriza adaptar la capa de operador a la implementación real, que manda. No cambia el arnés, sus escenarios ni sus criterios.

## Revisión independiente de la PR #37 y correcciones `v5` — decisión del PO, 2026-11-09 (Chile)

El PO pidió la revisión «antes con un sub agente de claude». Un subagente de Claude revisó `324ceed..34693ce` en solo lectura: **CAMBIOS SOLICITADOS**. El bloqueante B1: ante un ERROR del envoltorio, el verificador `v4` usaba `summary.harness.json` solo para los recuentos. Un canario PASS o una integridad en rojo del arnés daban nivel 2, y un FAIL del arnés bajo un error de entorno quedaba elegible para reintento. Claude lo reprodujo. La revisión también mostró que era falsa la afirmación de esta entrega de que el verificador detectaba la observación menor de #36: la copia sintética que la probó no reproducía el `failure()` real.

A la pregunta «¿Corrijo ahora el bloqueante y los menores de la PR #37?», el PO respondió literalmente **«Sí, corrige»**. Esa respuesta autoriza el cambio del verificador, que según `docs/autoqa/claude-operator.md` requiere aprobación del PO. Solo cambia la capa de operador: el arnés, sus escenarios, criterios y umbrales quedan intactos.

Resultado: verificador `autoqa-check v5`. Las 20 corridas reales dan lo mismo que con `v4`; 24/24 mutaciones correctas (`v4` fallaba 19); fixtures F1–F10 regenerados con el esquema real y los mismos niveles; 105 pares correctos. Detalle en el [informe](2026-11-06-autoqa-claude-operator.md#correcciones-de-la-revisión-independiente-v5). La PR #37 sigue sin fusionar hasta una nueva revisión independiente y la aprobación del PO.

## Revisión independiente de `v5` y correcciones `v6` de la PR #37 — decisión del PO, 2026-11-09 (Chile)

El PO pidió literalmente **«Si, revisa v5»**. Un subagente de Claude revisó `0652660` en solo lectura: **APROBADO CON OBSERVACIONES**, con el bloqueante B1 resuelto y tres menores nuevos (M1: un SHA-256 declarado ausente o `null` no bloqueaba un PASS; M2: un PASS no exigía los seis escenarios por tres repeticiones; M3: un marcador `<summary.*>` desconocido no era incoherencia).

Claude ofreció corregir M1–M3 e incluir también O1 (bloque de nivel 3 cuando el verificador falla con una excepción) y la opción prudente de O2 (sin reintento si `summary.harness.json` tiene su propio `executionError` no ambiental). A la pregunta «¿Corrijo M1–M3 en la #37 antes de fusionar?», el PO respondió literalmente **«Sí, corrige M1–M3»**. La opción prudente de O2 se aplica en esa respuesta; si el PO prefiere permitir el reintento en ese caso, se revierte en una entrega aparte.

Solo cambia la capa de operador: el arnés, sus escenarios, criterios y umbrales quedan intactos. Resultado: `autoqa-check v6`, once casos nuevos RED con `v5` y suite 35/35 GREEN; corridas reales y fixtures sin cambios de nivel ni salida. Detalle en el [informe](2026-11-06-autoqa-claude-operator.md#revisión-independiente-de-v5-y-correcciones-v6). La PR #37 sigue sin fusionar hasta la aprobación del PO.


## AutoQA — autorización del PO para precedencia del envoltorio (2026-11-09, Chile)

El PO autoriza corregir en `scripts/autoqa.sh` el origen por el que el envoltorio, al reemplazar summary.json por un ERROR, rebaja nivel/salida y pierde señales ya demostradas por el arnés. Rama/worktree nueva `autoqa/wrapper-precedence`, desde main e18d3e1. Este registro corresponde solo a esta entrega y no duplica el cierre de PR #36 registrado en la rama operadora #37.

**Decisión confirmada por el PO al enviar el encargo:** «con integridad demostrada, manda la precedencia: **salida 3, nivel 3**, gate ERROR y `executionError.kind=dirty-after-run`, que se conserva visible. Sin integridad, dirty-after-run sigue siendo ERROR/salida 4». Resuelve la contradicción de OPERATIONS sin cambiar las demás decisiones de juego. Se conservan señales conocidas del arnés, sus razones y artefactos; los datos desconocidos no pasan a true.

TDD determinista sin Unity antes del fix, pruebas versionadas ejecutables y dos corridas reales consecutivas limpias PASS/0/nivel1. Alcance solo scripts y documentación; no tocar Assets/ProjectSettings/Packages ni device.env, escenarios, expectativas, márgenes, tolerancias, determinismo, defaults, spec/planes/criterios, PR #32/#37 ni S3/S4/First Rally/First Match. Sin iPhone/build/instalación. PR propia lista para revisión de Claude coordinada por el PO, sin fusionar. Ante otro defecto, registrar y detenerse.


### Preparación del worktree de precedencia — confirmación adicional del PO

El worktree nuevo no incluía scripts/device.env, requerido por el lanzador. Consultado por esa restricción, el PO autorizó literalmente **«Sí, copiarlo sin cambios»** desde autoqa/mvp. Se copia byte a byte, con SHA-256 coincidente, sin cambiar valores ni modificar el original; archivo ignorado, no versionado. No autoriza otros cambios de configuración.


### Entrega de precedencia del envoltorio — 2026-11-09 (Chile), revisión pendiente

Fuente34b28c4 en autoqa/wrapper-precedence desde main e18d3e1. RED válido17métodos/21fallos por aserción/cero errores → GREEN17métodos/32fixtures. El ERROR conserva integridades booleanas conocidas/canario/razones, máximo de niveles y salida3>4>1>2; dirty-after-run con integridad sale3/nivel3 manteniendo kind/gateERROR. Counts/determinism del arnés separados en harnessResults; original, informe y SHA conservados; previous encadenado. Dos corridas nuevas85/85,18/18PASS/canarioFAIL, PASS/0/nivel1/dirty=false y árbol limpio sin checkout manual. Solo scripts/docs: Assets/ProjectSettings/Packages intactos, sin regresión completa ni check-release según autorización. Copia idéntica de device.env autorizada expresamente por PO, sin valores cambiados. V5 histórico/v6 actual de #37 solo lectura: cero incoherencias en corridas reales; fixtures sintéticos generan incidencias de formato documentadas, salidas esperadas coinciden. [Informe y huellas](2026-11-09-autoqa-wrapper-precedence.md). PR propia para Claude coordinado por PO, sin fusionar; no duplica cierre #36 ni toca #37. Sin nueva parada.


### Integración de main durante la entrega de precedencia

Al abrir PR #38, main avanzó a d1d7401 (merge externo de #37). Integrado en e928c66; conflictos solo en AGENTS.md y este registro, resueltos preservando íntegros el contenido de main y las entradas propias, sin duplicar la base ni el cierre de #36. No se solicitó/ejecutó la fusión de #37 ni se modificó su rama. Scripts probados y Assets/ProjectSettings/Packages byte a byte sin cambio; nueva revisión de #38 pendiente, sin fusionar. Auditoría local de combinación y huellas en el informe.


## AutoQA — cierres de PR #37/#38 y pendientes autorizados (2026-11-09, Chile)

PR #37: PO literal «Si, fusionar». Merge `d1d740130cafce5e1c44aa874aa015c214dbbd5c`, segundo padre exacto `9a3eb3782372efd8c8561ac013b828f019145af6`; árbol idéntico al head aprobado. Capa de operador con `autoqa-check v6` cerrada.

PR #38: Claude **APROBADO CON OBSERVACIONES** sobre `dde63e6`: 17/17 pruebas del autor, RED de 21 aserciones contra el script anterior y ocho casos adicionales del revisor GREEN. Primera corrida real de `/autoqa` con `autoqa-operator` cargado: `20261109T200908Z-dde63e6-29660`, PASS/0/nivel 1, 41/41 SHA-256; comprobación independiente coincidente campo a campo. PO literal «Sí, fusionar #38». Merge `9ae34fc69806a917d8e24096fcaad5e0a5b8bcc0`, segundo padre exacto `dde63e631b58ec4d4ea556b18cd54ed9549712c9`; árbol idéntico al head aprobado.

El PO autoriza esta entrega en rama/worktree nuevos `autoqa/followups` desde main `9ae34fc`: impedir que el envoltorio archive su propio ERROR como arnés; alinear schema y declaraciones de nivel/salida 3 en `autoqa-check v7`, incluida autorización expresa para editar el verificador. Este registro documental precede todo cambio de código. TDD por aserción, pruebas versionadas, 35 mutaciones de v6, compatibilidad sobre todas las corridas reales de nuestros worktrees y dos corridas reales consecutivas limpias PASS/0/nivel 1 con v7 sin incoherencias. Alcance solo scripts, agente operador, versión en skill y documentación; sin Assets/ProjectSettings/Packages, investigador ni otros cambios de `/autoqa`. Sin producto, escenarios, umbrales, tolerancias, defaults, spec/planes/criterios, iPhone, Xcode, instalación ni etapas ajenas. No modificar device.env; copia idéntica requiere autorización propia. Informe propio y PR lista para Claude coordinado por el PO, **sin fusionar**. Ante otro defecto o diferencia de compatibilidad sin justificar, registrar y detenerse.


### Histórico — AutoQA followups: avance verificable, corridas nuevas pendientes (2026-11-09, Chile)

Rama autoqa/followups desde main 9ae34fc; primer commit documental 7d270d0 antes de código. Fix del envoltorio 6543733: primer ERROR sin arnés conservado como previous-error con SHA, sin atribuirlo al arnés. V7 c2117b1 alinea schema desconocido y declaraciones nivel/salida3; comprueba SHA de los archivos propios sin usar sus señales. TDD: envoltorio 18 métodos/1 fallo por aserción→18/18 GREEN (33 fixtures); verificador 6 métodos/5 fallos por aserción→6/6 GREEN, 17 ejecuciones y controles copiados de cinco corridas reales, sin omisiones. Mutaciones v6: 35/35 GREEN con ambas versiones. Compatibilidad de las 23 corridas históricas disponibles: cero diferencias en salida esperada/nivel/gatePass/reintento/incoherencias y originales intactos. 585 archivos protegidos iguales a main. [Informe](2026-11-09-autoqa-followups.md).

Pendiente la respuesta del PO a la autorización solicitada para copiar device.env byte a byte desde autoqa/mvp. No copiado ni modificado. Las dos corridas Unity nuevas y la entrega final lista para Claude siguen pendientes; no inferir aceptación ni completar la entrega con pruebas sintéticas. Sin otro defecto ni diferencias de compatibilidad sin justificar. Sin iPhone/build/instalación ni cambios fuera de alcance; no fusionar.


### AutoQA followups — entrega técnica completa para revisión de PR #39 (2026-11-09, Chile)

Resuelta la espera anterior: el PO autorizó literalmente «Sí, copiarlo sin cambios»; device.env copiado byte a byte desde autoqa/mvp, SHA-256 de origen/copia 7fbd47039b15d9ca89dc70250193590a41eeff79b22833872e7fadce701dc44f e idéntico al terminar, ignorado/sin versionar. Dos corridas consecutivas desde 72b2b3f limpio: 20261109T232416Z-72b2b3f-12736 (41,95 s) y 20261109T232458Z-72b2b3f-19058 (14,29 s); cada una 85/85 pruebas frescas, 18 PASS/canario FAIL esperado, PASS/0/nivel1/dirty=false, árbol limpio antes/entre/después y restauración automática sin errores, sin checkout manual. V7 en ambas: 41/41 SHA, cero incoherencias, gatePass=true. Compatibilidad final v6/v7 25/25 corridas reales sin diferencias; 845 archivos históricos y 585 archivos protegidos intactos. Permanecen GREEN las pruebas ya registradas: 18/18 envoltorio, 6/6 verificador y 35/35 mutaciones con ambas versiones.

[PR #39](https://github.com/rafaallendes-boop/squash-clash/pull/39) preparada para revisión de Claude coordinada por el PO, **sin fusionar**. [Informe y SHA-256](2026-11-09-autoqa-followups.md). Este registro sustituye el estado pendiente de la entrada anterior, conservada como histórica. Solo documentación desde el código probado c2117b1. Sin cambios de producto/escenarios/criterios, investigador, iPhone/Xcode/instalación, regresión completa ni check-release, conforme al alcance. Sin otra parada ni diferencias sin justificar.


## S3 — diagnóstico previo y predeclaración complementaria: decisión del PO, 9 de noviembre de 2026 (Chile)

> «Haría la sesión complementaría pero ya hoy no tengo tiempo, entonces por eso está la idea del QA. O bien lo hacemos mañana, igual seguiría avanzando»
>
> «Deja el prompt para que lo haga codex. Y mañana haría una prueba mas y luego ya seguiría avanznado»

Registro literal como primer commit documental del encargo, antes de cualquier otro cambio. Base local/remota comprobada `bacf39f7651e3fa1307c26b9a200b92498d22e86`, rama/worktree `firstshot/s3-validation`, PR #40 abierta y sin fusionar. Autorizado ahora: diagnóstico por lectura de código, logs y fuentes oficiales, e informe/adenda documental para revisión de Claude coordinada por el PO. Hoy sin acceso al iPhone, builds ni instalaciones; la sesión complementaria requiere autorización del PO al comenzar y elección de operador. Ante defecto demostrado u otra sorpresa, registrar y detenerse. Sin cambios de producto/tests/defaults/presets/spec/planes/criterios/umbrales; sin tocar #32, S4, First Rally ni First Match. No recalcular hashes esperados `6ecfdfa78cbb`/`4f33dff747fb` ni versionar logs/evidencia/device.env/identificadores del iPhone. Perfil conocido hasta `2026-11-12T13:55:07Z` (12 de noviembre, 10:55:07 Chile); toda sesión futura, incluida restauración, debe terminar antes de esa hora. La propuesta de mañana se refiere al 10 de noviembre; no consta autorización de ejecución ni aceptación de S3.


## S3 — diagnóstico y sesión complementaria predeclarada (9 de noviembre de 2026, Chile)

Primer commit documental `7d00ef3` conserva las dos frases literales del encargo antes de cualquier otro cambio, sobre `bacf39f` local/remoto comprobado. Diagnóstico offline de código/logs/fuentes oficiales en el [informe S3](2026-11-06-first-shot-s3-physical-validation.md#diagnóstico-previo-a-la-sesión-complementaria): PerfectFlash exige Perfect con contacto; Failed puede mostrar nota/raqueta/mira/anillo y la pelota puede dejar marcas de pared. El flash blanco de bloqueo existe solo en la ruta Sling, no en M2-4 usado en la sesión. Ninguna de esas rutas identifica lo percibido por el PO. UIKit usa UIImpactFeedbackGenerator Light para Valid; Apple no aclara en las fuentes consultadas su relación exacta con «Háptica del sistema». Ambas percepciones siguen abiertas, sin defecto nuevo demostrado ni Pass/aceptación inferidos.

[Adenda complementaria](../validation/2026-11-06-first-shot-s3-physical-predeclaration.md#s3-complementaria-20261109) añadida sin alterar el protocolo anterior: F→P→G→H→I, máximo 12/4/4/4/4 recepciones (28 en total), corte F al primer Perfect+Valid aplicados; G/H/I al primer contacto aplicado; P observación de pausa separada. F necesita ambos resultados o queda contraste NotEvaluated. I incluido por observación del sistema no resuelta. Precheck completo en proceso nuevo desde P0 por bloque; H cambia app haptics en ese mismo proceso, desplegando First Shot / feedback y usando como export anterior uno donde el proceso aún no existía. Frontera por cierre DEV del PO; preguntas literales de §6 una a una/antes de resultados, extras Perfect de S3-A en F y pregunta literal de §5 en P. Sin repeticiones para conseguir categorías; parciales/extras conservados. Aproximadamente 20 min con 4 reservados para restauración; si falta tiempo, conservar NotEvaluated.

Preparación documental para PR #40, sin fusionar; Claude revisa por coordinación del PO. La sesión propuesta del 10 de noviembre **no está ejecutada ni autorizada al momento de empezar**: requiere autorización expresa y operador elegido por el PO. App instalada 0.1.0+097eab9 dev, sin nueva build/instalación. Proponer bloqueo automático Nunca temporal; solo PO toca Ajustes y repone estado previo. Perfil conocido vence 12nov2026 13:55:07Z/10:55:07Chile; toda sesión/restauración debe caber antes. Restauración futura P0 en proceso nuevo, hash literal 6ecfdfa78cbb/pending0 y tres archivos idénticos al respaldo y referencia inicial; H único hash alternativo 4f33dff747fb, sin recalcular. Sin iPhone hoy ni tests/Unity/build/instalación; sin cambios de producto/defaults/presets/spec/planes/criterios/umbrales, #32/S4/First Rally/First Match. Ante defecto demostrado/sorpresa o §8, detener/exportar/restaurar; ninguna corrección en #40. Las 21 recepciones históricas y sus NotEvaluated permanecen intactos.

Diagnóstico confirmado en `8bb4b7a`; adenda y registro de preparación confirmados en el commit de esta entrada, antes de cualquier acceso futuro al iPhone. Comprobación documental: originales conservados como prefijo en cuatro Markdown, 7+2+1 preguntas literales y 30 referencias locales cotejadas; 2.808 archivos de evidencia con rutas/SHA-256 intactos. Cotejo offline de tres archivos en after-D2/after-E/after-pause/restored-final igual a ambos respaldos. Sin tests/check-release nuevos por delta exclusivamente documental; ninguna observación física nueva. Entrega para revisión en #40, sin fusionar; autorización de ejecución y designación de operador aún pendientes del PO.

## Revisión de Claude de la preparación complementaria y acortamiento de la sesión — 9 de noviembre de 2026 (Chile)

Claude revisó en la PR #40 el head `6a147ba` (Codex): **APROBADO CON OBSERVACIONES**. La entrega cambia solo cuatro Markdown y conserva el texto anterior. El diagnóstico coincide con el código: el destello de Perfect exige una presentación Perfect con contacto, y el puente háptico usa `UIImpactFeedbackGenerator`. La adenda cumple el encargo.

Observaciones:
1. C4 pedía una pregunta por diálogo, por una redacción ambigua del encargo de Claude. Eso son más de 35 diálogos.
2. Los 20 minutos previstos no alcanzan para cinco bloques.

Claude preguntó: «¿Corrijo la adenda antes de mañana para usar dos diálogos por bloque?». **Respuesta literal del PO: «Si, algo que sea lo más corto posible. Y ya para que avancemos a S4».** Claude agrega la sección C8 a la predeclaración, sin modificar C1–C7:
- dos diálogos por bloque;
- la pausa dentro del proceso de F, fuera de su cuota;
- orden F → G → H, con I opcional;
- unos 30 minutos.

Esta respuesta expresa la intención del PO de avanzar a S4. No es una autorización de S4: falta la decisión del PO sobre S3 después de la sesión complementaria.

## S3 — sesión complementaria C8 operada por Claude (11 de noviembre de 2026, Chile)

**Autorización literal del PO al empezar:** «Ahora estoy listo para hacer el smoke» y, a continuación, «Me refería a la prueba nuevamente de S3». Operó Claude con el PO presente y la app instalada `0.1.0+097eab9 dev`, sin build ni instalación; la PR #44 no se incluyó. El PO informó que renovó el perfil en caché, con el nuevo venciendo el 18 de noviembre a las 10:50:05 Chile. La app instalada conserva el perfil que vence el 2026-11-12T13:55:07Z y la sesión terminó antes.

Decisiones literales del PO durante la sesión:
- «Jugar S3-I», sobre el bloque opcional;
- «me equivoque, el modo silencio, ya está ok», que corrige su primera declaración de G con timbre; el bloque no empezó hasta esa corrección.

Resultados de las cuotas:
- **F:** 12 recepciones, 4 Valid, 8 Failed y 0 Perfect;
- **G:** 4 Failed;
- **H:** 3 Failed y 1 NoOrder, con `4f33dff747fb`;
- **I:** 4 Failed.

Hubo una presentación concordante por intento, cero discrepancias y cero pendientes, sin condiciones de §8.

**Pausa (extras de F).** Hubo dos pausas, después de los Valid L63 y L69. Son posteriores al máximo de 4 recepciones de C8.2; se conservan como desviación y el PO decide si cuentan.

**H.** El Valid L43 con `feedback.haptics=0` es preparación, porque estaba en curso al cerrar DEV. El PO dijo «logré golepar 1 vez» y respondió vibración «no hubo».

**Restauración.** Proceso nuevo a las 2026-11-11T14:35:33.0147180Z en P0 `6ecfdfa78cbb`, con cero pendientes y los tres archivos idénticos al respaldo y a la referencia inicial.

**Siguen NotEvaluated:**
- contraste y destello Perfect;
- silencio con impacto;
- háptica de app desactivada con contacto en cuota;
- háptica del sistema desactivada con contacto;
- congelación del destello.

Recomendación de Claude: aceptar S3 con condiciones, o integrar antes una ayuda de timing con autorización propia, como la #44. **Decide el PO.** S4 no está autorizado. Detalle en el [informe](2026-11-06-first-shot-s3-physical-validation.md#sesión-complementaria-c8--11-de-noviembre-de-2026-chile).

## Decisión del PO sobre S3 — 11 de noviembre de 2026 (Chile)

Claude preguntó, tras la sesión complementaria: «¿Cómo cierras S3?», con dos opciones: aceptar con condiciones o probar antes la PR #44. **Respuesta literal del PO: «Aceptar con condiciones».**

**S3 queda aceptado con condiciones.** Estas son las condiciones, tal como Claude las presentó en la opción (a); quedan pendientes para observarlas en S4 o cuando mejore el timing:
1. contraste Perfect/Valid y destello de Perfect (0 Perfect en las 45 recepciones de cuota de S3);
2. silencio con impacto aplicado;
3. `feedback.haptics=0` con contacto dentro de cuota (solo existe L43, que es preparación);
4. háptica del sistema desactivada con contacto, y la observación (b) de S3-D: vibración percibida con «Háptica del sistema» desactivada;
5. congelación del destello de Perfect en pausa.

La aceptación no reinterpreta los datos: los NotEvaluated siguen siéndolo y no cuentan como Pass. El PO no decidió si las dos pausas fuera del máximo de C8.2 cuentan como evidencia, así que se conservan como desviación registrada. La observación del destello percibido sin Perfect (9 nov) queda abierta sin defecto demostrado.

**Esta decisión no autoriza:**
- S4, First Rally ni First Match;
- fusionar #40;
- fusionar, compilar o instalar la #44;
- cambiar parámetros.

Cada una de esas acciones requiere autorización propia del PO.

## Cierre de PR #40 y autorización de First Rally R1–R3 y pase visual con Fable — 11 de noviembre de 2026 (Chile)

**Cierre de #40.** Respuesta literal del PO: «Sí, fusionar #40». Merge `433633f0c96f3a4b75c2d40b5520c807ce54e98a`, con segundo padre exacto `e606361ae759e2f7f85ec2b4e42ad99f72339520` y árbol idéntico al head aprobado. Antes de fusionar se comprobó que el head local y el remoto eran el mismo, que la PR estaba abierta, no era borrador y no tenía conflictos, y que los árboles estaban limpios. Main quedó sincronizada. Rama y worktree de Codex conservados con su evidencia ignorada.

**Perfil de firma.** Informe del PO: perfil renovado el 2026-11-11T13:50:05Z, vence el 2026-11-18T13:50:05Z (10:50:05 Chile), con el mismo certificado, dispositivo y permisos. Claude lo comprobó en la caché de Xcode. La app instalada sigue firmada con el perfil que vence el 2026-11-12T13:55:07Z y no se reinstaló.

**Autorización del PO para Fable**, en su «Aviso para el revisor — plan con Fable (11-nov-2026)». Citas literales:

> Contexto: probé S3 en el iPhone y no salió bien: [El timing para golpear me está dando dificultades].
>
> Decisión del PO: aprovechar Fable desde Claude Code, en mi Mac, con dos prompts en paralelo y ramas/worktrees separados, ambos con esfuerzo high:
>
> 1) Gameplay (First Rally): R1 reglas del punto, R2 movimiento automático, R3 IA básica «Fondo». Se detiene tras R3 con informe. R4–R5 (loop de rally, HUD y UX mínimos) y R6 (First Match) quedan para una segunda ronda, previa revisión.
> 2) Visual: personajes low-poly, cancha, iluminación, pelota, impactos y UI. Solo capa Runtime/vista; sin tocar Core, física, SimulationKey 166a96e319c7 ni DefaultPreset.json. Sin empeorar el p95 ni la legibilidad de altura/profundidad aprobada en H1. Con capturas antes/después.
>
> Autorización: dejo registrada mi autorización literal para First Rally y la excepción «dirección de arte» solo para el pase visual. Spec, escenarios, umbrales y criterios no se modifican.
>
> Lo que te pido:
> - Revisión independiente de ambas ramas cuando Fable entregue, como en los cierres anteriores.
> - Que no se fusione nada sin mi aprobación.
> - Tener presente cuando vence el perfil de firma

**Alcance autorizado:**
- First Rally R1–R3: reglas del punto, movimiento automático e IA básica «Fondo», con informe y parada después de R3;
- pase visual en la capa Runtime/vista, con la excepción de dirección de arte.

**Sin autorizar:**
- R4–R6, que requieren revisión previa;
- First Match;
- S4;
- cambios de spec, escenarios, umbrales o criterios;
- cualquier fusión sin aprobación del PO.

El pase visual no toca Core, física, SimulationKey `166a96e319c7` ni `DefaultPreset.json`. La base de las ramas quedó por completar: Claude recomienda `main` en el momento de abrir las ramas y no `firstshot/s3-feedback`, que ya está fusionada en `main` y desactualizada. Las observaciones de Claude como revisor están en el registro de la sesión y en `AGENTS.md`.


## S4 — autorización vigente, revisión de PR #45 y visibilidad pública (PO, 11 de noviembre de 2026)

- **Alcance S4 autorizado por el PO:** solo 4.1 (calibración del offset perceptivo conjunto), 4.3 (bots/experimento H3b en el Mac) y Step 0 documental de 4.2 adaptado a #44. **La implementación de 4.2, 4.4/4.5 y toda actividad en iPhone siguen sin autorización.** Spec/planes/escenarios/umbrales/criterios intactos. El PO autorizó continuar con el vector causal H3b NotEvaluated, sin implementar la capa diferida de S3; la hipótesis completa sigue NotEvaluated.
- **Visibilidad vigente del remoto:** PO literal **«Sí, es intencional; continuar y crear el PR»** sobre el repositorio público `rafaallendes-boop/squash-clash`. Esta decisión sustituye **«remoto privado»** y **«No cambiar la visibilidad privada del remoto»** como descripción del estado vigente, sin reescribir el histórico. No autoriza cambiar la visibilidad por iniciativa del agente; se conserva el estado público confirmado. Sin versionar device.env, credenciales, identificadores del iPhone, logs o builds.
- **Revisión de Claude:** PR #45/head `8e6e1c25d5ef33b7c56b58cb7993517f0dc88f29`, APROBADO CON OBSERVACIONES, sin bloqueantes; 562/562 EditMode y 368/368 PlayMode reproducidos. Autoqa-check v7 coincide con el operador: PASS/0/nivel 1 y 41/41 SHA de la corrida `20261111T153744Z-2fc2afe-25993`.
- **Observaciones autorizadas:** O1 bloquear DEV/tres dedos durante el modal y reponer BlockedByUi según DEV real, con TDD; O2 sobre metricsValid según reloj, validez de calibración solo en payload, con TDD; O3 aclarar el borde inclusivo −100 ms y las pérdidas de SL3/SL4 sin reejecutar H3b; O4 H3b completo NotEvaluated sin cambio; O5 documentar asincronía media negativa (Repp, 2005) y transferencia a intercepción no demostrada, sin nuevo umbral; O6 esta entrada vigente idéntica en AGENTS.md y aprobaciones. [Informe S4](https://github.com/rafaallendes-boop/squash-clash/blob/firstshot/s4-timing/docs/reviews/2026-11-11-first-shot-s4-timing.md).
- **Autorización literal del PO para corregir y fusionar:** «Dejame el prompt para que codex arregle las observaciones y luego haga el merge». Misma rama/PR, delta limitado a LatencyCalibration.cs, DevPanelView.cs, LatencyCalibrationTests.cs y documentación autorizada; RED por aserción/GREEN, suites frescas, check-release dev/prod limpio y AutoQA PASS/0/1 antes del merge commit. Head exacto y árboles limpios; segundo padre=head verificado y árbol idéntico; sincronizar main y preservar ignorados con inventario/SHA antes de retirar worktree sin force, rama remota conservada. El cierre del merge se registra en la siguiente entrega, **sin commit aparte en main**. La fusión no acepta S4. Sin contrato/reloj/renderers #44/solver/P2/AutoQA/spec/planes/criterios nuevos ni #32/áreas de Fable/First Rally/First Match, iPhone, build nativa o instalación. Ante defecto/sorpresa/RED no basado en aserción, registrar y detener sin fusionar.


## Encargo documental del PO — 12 de noviembre de 2026

Primer commit documental de `docs/register-fable-round`, desde `a844c76`. Fuente: mensaje del PO en este chat del 12 de noviembre de 2026. Texto íntegro del encargo recibido, sin convertir los hechos solicitados en hechos verificados antes de la auditoría:

````text
Encargo del PO (Squash Clash), autorización literal del 12 de noviembre de 2026: «dejame los encargos a codex para ir avanzando».

TAREA: entrega SOLO documental (AGENTS.md y docs/reviews/2026-10-25-first-shot-po-approvals.md) que registre lo hecho entre el 11 y el 12 de noviembre. Primer commit: este encargo literal.

BASE: main a844c76. Rama y worktree propios, por ejemplo docs/register-fable-round.

REGISTRAR, con citas literales tomadas de los archivos y sin parafrasear las autorizaciones:
1. Autorización de First Rally que ejecutó Fable. Literal en docs/reviews/2026-11-11-first-rally-plan.md §1 (rama firstrally/gameplay): «autorizo First Rally, etapas R1–R5 y, si queda tiempo, R6 (First Match)…».
   - Hoy AGENTS.md y aprobaciones (línea ~2112) registran solo R1–R3 con parada tras R3.
   - Registrar la autorización ampliada como posterior, sin borrar la anterior. Si no puedes fechar la ampliación con evidencia, dilo y deja la fecha como pendiente de confirmación del PO.
2. Pase visual. Literal en docs/reviews/2026-11-12-visual-pass-plan.md (rama visual/art-pass).
3. Revisión independiente de Claude (sesión del PO, 12 nov). Sin bloqueantes de código, salvo:
   - Defecto en la regla «fuera tras el primer bote»: la regla WSF 6.2.4 y la definición OUT dicen que pierde quien pegó. Corregido en c9e2488 (head firstrally/gameplay d9605b5) e integrado con merge c92c64c en visual/art-pass (head #47 a96de94).
   - Suites reproducidas por Claude: #46 676/676 EditMode y 393/393 PlayMode (sobre f57a09c); #47 676/676 y 400/400 (sobre 62e2f4f).
   - Tras la corrección, según la sesión de First Rally: 678/678 y 393/393, AutoQA PASS/0/1. Según la sesión del pase visual: 678/678 y 400/400, AutoQA PASS/0/1, check-release OK.
   - Observaciones del pase visual: H1, temblor de cámara (no se congela en pausa; afecta la proyección de M2-4 durante 0,18 s), p95 y TDD posterior. Están en el §6 bis del informe del pase.
4. Instalación autorizada por el PO el 12 nov: «Realiza la instalación en el iphone. lo tengo desbloqueado».
   - Build 0.1.0+c92c64c dev, perfil incorporado vigente hasta 2026-11-18T13:50:05Z.
   - Respaldo previo: seis presets idénticos a la restauración del 11 nov. SHA-256 de P0-calib-1 2f1295f2…e4fd, Po-1 9008c33a…1767 y last-active 5885ce8a…6087.
   - Primer lanzamiento rechazado por iOS: «profile has not been explicitly trusted by the user». El PO verificó la app en Ajustes.
   - Proceso nuevo 2026-11-12T20:18:23Z: P0-calib-1, 6ecfdfa78cbb, pending 0, presets idénticos byte a byte.
   - Evidencia ignorada en Logs/evidence/install-c92c64c-20261112 del worktree visual-art-pass.
5. Cierres de #41 (cb145c3), #42 (7d83f4a) y #43 (98701de), que no figuran en AGENTS.md. Verifica con git el merge commit, el segundo padre y el árbol de cada uno antes de escribirlos. Revisa también si el cierre de #45 (a844c76) quedó registrado.
6. Cierres de #46 y #47 (12 nov 2026). Aprobación literal del PO: «Si, apruebo fusionar» (fusionar #46 y #47 antes de la capa causal).
   - #46: merge 2fbb5be72041b59a25231815270d173d2673ebc5, segundo padre exacto d9605b50d1b6337d7a08549473b0a9da91d25770, árbol idéntico.
   - #47: base cambiada de firstrally/gameplay a main; merge 9af2b0c8bd92fd5d27f9b2486ea7b626da903874, segundo padre exacto a96de94bf9a53ed4a3272c67adf4b98ed530ef12, árbol idéntico.
   - Ramas remotas y worktrees conservados (build instalada y evidencia en el worktree visual-art-pass).
   - La build instalada 0.1.0+c92c64c dev tiene el mismo producto que main 9af2b0c (después solo hay documentación).
   - La fusión no acepta First Shot, First Rally ni First Match: la aceptación queda para el playtest integrado.
   Verificar con git los padres y árboles antes de escribirlos.

LÍMITES:
- Solo esos dos Markdown. Sin código, tests, spec, planes, criterios ni defaults.
- No tocar las ramas de #46/#47 ni #32.
- Sin iPhone.
- Documentación en español.

7. Decisiones del PO del 12 de noviembre de 2026 sobre el cierre de First Shot. Registrar LITERALES, sin parafrasear:

«Decisiones D1–D4:
* D1: Aprobada opción B. Ocultar completamente el indicador de timing durante el intervalo propuesto, sin pistas residuales. Mantener ayuda completa por defecto y actualizar los hashes y anclajes de AutoQA correspondientes.
* D2: Aprobada la propuesta de Codex: 3×3 zonas, potencia en tercios y clasificación del primer bote. Son bandas provisionales de observación, no criterios de aceptación.
* D3: Aprobada la implementación de la capa causal no direccional. Prefiero completar H3b antes de cerrar First Shot, manteniéndola como una entrega independiente.
* D4: Aprobadas ambas correcciones del indicador mediante TDD, sin modificar las ventanas ni sus umbrales.»

«Las pruebas que realizaremos ahora en el iPhone estarán enfocadas principalmente en revisar y validar lo que hizo Fable, incluyendo personajes, cancha, animaciones, efectos visuales, HUD y su integración con los controles. Las pruebas reales de jugabilidad las quiero realizar cuando tengamos First Shot y First Rally cerrados e integrados. […] No quiero declarar First Shot aceptado solo por pruebas técnicas o visuales. Podemos dejar implementadas y verificadas sus tareas, pero reservar su aceptación física definitiva para el playtest integrado.»

«Objetivo: dejar First Shot y First Rally técnicamente completos, integrados y listos para una sesión real de juego en iPhone. […] Mantengamos el enfoque incremental, con pruebas automatizadas, revisión independiente y aprobación del PO antes de cada fusión.»

Contexto de D1–D4: propuesta del Step 0 de 4.2 en docs/reviews/2026-11-11-first-shot-s4-timing.md (§T4.2) y observaciones 2 y 3 de la revisión de #44.
Estado que resulta:
- 4.2 (implementación) y la capa causal (Tarea 3.1 del plan r2) quedan AUTORIZADAS como entregas propias.
- 4.4 se implementa solo tras la validación del anexo C por el PO.
- 4.5 (sesiones físicas) sigue sin autorizar.
- La aceptación de First Shot queda reservada al playtest integrado.

ENTREGA:
- PR documental lista para revisión de Claude coordinada por el PO, sin fusionar.
- Detenerse.
````


## Registro verificado del 11–12 de noviembre de 2026 — Fable, cierres y decisiones de First Shot

Entrega documental desde `a844c76cfadb1e1c5321ff4d185d9b57f77fb79b`. Primer commit **`385b8ef`**: encargo literal completo anterior a este registro. Fuentes: archivos de las ramas indicadas, Git local, estado de PR consultado en GitHub, evidencia local ignorada y hechos/citas de la sesión del PO transmitidos literalmente en el encargo anterior. Las suites y la instalación siguientes son **históricas**: no se ejecutaron en esta entrega. Las entradas anteriores se conservan; este registro actualiza sus estados pendientes.

### Autorizaciones posteriores de Fable

La entrada del 11 de noviembre que limitaba First Rally a R1–R3 y ordenaba parar después de R3 se conserva como histórica. La autorización ampliada que ejecutó Fable figura literalmente en [plan First Rally, §1](https://github.com/rafaallendes-boop/squash-clash/blob/d9605b50d1b6337d7a08549473b0a9da91d25770/docs/reviews/2026-11-11-first-rally-plan.md#1-autorización-literal-del-po):

> AUTORIZACIÓN: autorizo First Rally, etapas R1–R5 y, si queda tiempo, R6 (First Match). Haz un commit por etapa con tests verdes, para que yo pueda cortar donde quiera. Mi uso de Fable es limitado: prioriza avance funcional y commitea seguido.

El plan la registra como del 11 de noviembre; Git acredita su incorporación en `f271125f50b60d2e0337c8348a56cecf8c9c8791`, el **11 de noviembre de 2026 a las 15:52:57 America/Santiago**. Esto fecha el registro. **Hora del mensaje según la transcripción, comunicada en el encargo de corrección de PR #49: 2026-11-11T18:44:19Z (15:44:19 America/Santiago), anterior a `f271125`; pendiente de confirmación del PO.** No se infiere esa hora del commit. Es posterior a la autorización limitada y habilita R1–R5 y R6 si quedaba tiempo; el [informe de gameplay](https://github.com/rafaallendes-boop/squash-clash/blob/d9605b50d1b6337d7a08549473b0a9da91d25770/docs/reviews/2026-11-11-first-rally-gameplay.md) declara R1–R6 implementadas, con R6 mínimo y saque automático. No acredita aceptación física.

Autorización del pase visual del **12 de noviembre de 2026**, copiada de [su plan](https://github.com/rafaallendes-boop/squash-clash/blob/a96de94bf9a53ed4a3272c67adf4b98ed530ef12/docs/reviews/2026-11-12-visual-pass-plan.md#autorización-literal-del-po-12-de-noviembre-de-2026):

> AUTORIZACIÓN: autorizo un pase visual, saliendo del alcance «dirección de arte = FUTURO» solo para esta tarea. Cítala en tu informe. Mi uso de Fable es limitado (cuota de 5 horas): trabaja por bloques en este orden de prioridad, haz un commit por bloque que deje la app funcionando, y si te quedas sin tiempo lo ya entregado debe servir.

Los límites de ambos planes se conservan: spec, escenarios, umbrales y criterios intactos; el pase visual protege Core/física/reglas/timing, SimulationKey y defaults. La instalación y las fusiones posteriores tienen autorizaciones propias, registradas abajo.

### Revisión independiente de Claude — 12 de noviembre

Según la sesión independiente del PO, comunicada en el encargo literal: sin bloqueantes de código salvo la regla «fuera tras el primer bote». Claude reprodujo **#46: 676/676 EditMode y 393/393 PlayMode sobre `f57a09c`**; **#47: 676/676 y 400/400 sobre `62e2f4f`**. No son nuevas corridas de Codex ni resultados posteriores a la corrección.

El [informe First Rally, §11](https://github.com/rafaallendes-boop/squash-clash/blob/d9605b50d1b6337d7a08549473b0a9da91d25770/docs/reviews/2026-11-11-first-rally-gameplay.md#11-corrección-de-la-regla-fuera-tras-el-primer-bote-12-de-noviembre) registra la referencia WSF 6.2.4 y definición OUT: también después del primer bote **pierde quien pegó**, en lugar del receptor. Corregido en `c9e2488fbf76ed73f615330dcdb2b40219736a91`; head final de gameplay `d9605b50d1b6337d7a08549473b0a9da91d25770`. Integrado al pase visual por merge `c92c64c58a0062bf6ef2875c588093c211914256` (padres `5d0d511` y `d9605b5`), head final `a96de94bf9a53ed4a3272c67adf4b98ed530ef12`.

Resultados posteriores, atribuidos a sus sesiones e informes:

| Fuente | Commit probado | EditMode | PlayMode | AutoQA | check-release |
|---|---|---|---|---|---|
| First Rally, §11 | `c9e2488` | 678/678 | 393/393 | PASS / 0 / nivel 1 | No se atribuye una corrida nueva tras el fix |
| [Pase visual, §9](https://github.com/rafaallendes-boop/squash-clash/blob/a96de94bf9a53ed4a3272c67adf4b98ed530ef12/docs/reviews/2026-11-12-visual-pass.md#9-build-dev) | `c92c64c` | 678/678 | 400/400 | PASS / 0 / nivel 1 | OK dev/prod, árbol limpio |

Observaciones preservadas del [pase visual, §6 bis](https://github.com/rafaallendes-boop/squash-clash/blob/a96de94bf9a53ed4a3272c67adf4b98ed530ef12/docs/reviews/2026-11-12-visual-pass.md#6-bis-observaciones-de-la-revisión-independiente-12-de-noviembre):

- El temblor de cámara **no se congela en pausa**; avanza con tiempo no escalado durante los 0,18 s posteriores a un golpe fuerte.
- Un apoyo durante esos **0,18 s** puede fijar la mira M2-4 con la proyección desplazada hasta unos **2,8 cm**; no se corrige por esta documentación.
- Sombra y estela cambiaron ayudas aprobadas en H1. No empeorar altura/profundidad requiere juicio en iPhone con L1–L6 y DEV cerrado; no se infiere validación de H1.
- p95 del editor **0,51→0,81 ms**, batches aproximadamente **27→58**. El p95 de iPhone sigue pendiente; no se extrapola del editor.
- Desviación de TDD declarada en **§2** del mismo informe: las pruebas de vistas se escribieron después del código, sin RED previo por aserción de producto. §6 bis contiene las cuatro observaciones independientes; no se atribuye allí una quinta observación ni se presenta ese TDD posterior como RED→GREEN.

### Instalación autorizada y arranque comprobado — 12 de noviembre

Autorización literal del PO, transmitida en este encargo: **«Realiza la instalación en el iphone. lo tengo desbloqueado»**. Instalación histórica de **`0.1.0+c92c64c dev`**, Release ARM64, IL2CPP, no Development, SQUASH_DEV. Perfil incorporado creado el **2026-11-11T13:50:05Z**, vigente hasta **2026-11-18T13:50:05Z (10:50:05 America/Santiago)**; fechas cotejadas offline en la build conservada. Sustituye como dato vigente de la app instalada al perfil anterior que vencía el 12 de noviembre; ese histórico permanece.

Respaldo previo: los seis archivos de presets/selección son idénticos a `complement-20261111-restored` del worktree `s3-validation`. Se cotejaron de nuevo offline en esta entrega, byte a byte, contra el export posterior a la instalación: **6/6 idénticos** (`P0-calib-1.json`, `Po-1.json`, `S1-M2-4.json`, `S1-calib-gesture.json`, `S1-sling.json`, `last-active.txt`). SHA-256:

| Archivo | SHA-256 |
|---|---|
| P0-calib-1.json | `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd` |
| Po-1.json | `9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767` |
| last-active.txt | `5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087` |

Según la sesión del PO, el primer lanzamiento fue rechazado por iOS: **«profile has not been explicitly trusted by the user»**. El PO verificó la app en Ajustes. El export conservado acredita un **proceso nuevo 2026-11-12T20:18:23Z** (17:18:23 Chile; primer launch `20:18:23.1814010Z`), build `0.1.0+c92c64c dev`, **P0-calib-1 / `6ecfdfa78cbb` / pending 0**, presets y selección idénticos byte a byte. Esto acredita arranque/restauración, no aceptación ni una sesión 4.5.

Evidencia ignorada conservada en **`.claude/worktrees/visual-art-pass/Logs/evidence/install-c92c64c-20261112`**: `backup-before-install`, `after-launch` y registros de instalación/arranque. No se versiona evidencia, logs, perfiles, builds ni identificadores personales. Este encargo no accedió al iPhone.

### Cierres auditados con Git y estado de las PR

#41, #42 y #43 quedaron fusionadas el **11 de noviembre de 2026**, respectivamente a las **11:59:31, 11:59:42 y 11:59:52 America/Santiago según el commit de merge** (GitHub marca #41 a las 11:59:32; un segundo de diferencia de registro). #45 se fusionó el mismo día a las **13:18:17**. En la base `a844c76`, ninguno de los dos Markdown registraba estos cuatro cierres efectivos; #45 conservaba la autorización/revisión y el merge pendiente. Este registro completa esos cierres sin inventar otra aprobación literal.

- **#41:** sincronización documental de onboarding/estado y reglas del flujo.
- **#42:** test `HapticsOff_FailedAttempt_NoImpactAudioNoHaptic`, sin cambio de producto.
- **#43:** experimento explícito descriptivo SL5/SL6 post-frontis; no es aceptación ni regresión ordinaria.
- **#45:** cierre técnico S4 4.1/4.3 y Step 0 de 4.2, con correcciones de Claude. El [informe S4](https://github.com/rafaallendes-boop/squash-clash/blob/f359f7c039ad79f4096990a70dae91628950f229/docs/reviews/2026-11-11-first-shot-s4-timing.md#correcciones-de-claude--verificación-final-previa-al-merge) conserva 562/562 EditMode, 372/372 PlayMode, check-release dev/prod OK y AutoQA PASS/0/nivel 1 desde `bf0cd2a`; **la fusión no acepta S4**.

Padres completos comprobados con `git show -s --format=%P`; estado fusionado cotejado en GitHub:

| PR | Merge commit | Primer padre | Segundo padre exacto |
|---|---|---|---|
| [#41](https://github.com/rafaallendes-boop/squash-clash/pull/41) | `cb145c31837cb344e464d60675217b5c46bdd20b` | `433633f0c96f3a4b75c2d40b5520c807ce54e98a` | `d7cdc44f5243bcbc5b023f70882e506410c1aa63` |
| [#42](https://github.com/rafaallendes-boop/squash-clash/pull/42) | `7d83f4a96a2d02ed4965953e32661b12ea36b6d6` | `cb145c31837cb344e464d60675217b5c46bdd20b` | `689cc6612757500a8e84b783f4bdea6d6225f431` |
| [#43](https://github.com/rafaallendes-boop/squash-clash/pull/43) | `98701deaa4c52cebb0a855cfe0896e2c97d416c4` | `7d83f4a96a2d02ed4965953e32661b12ea36b6d6` | `5331667045fdcadd84488e75c0f2baf3cac577fc` |
| [#45](https://github.com/rafaallendes-boop/squash-clash/pull/45) | `a844c76cfadb1e1c5321ff4d185d9b57f77fb79b` | `18b796a615b5bbca856c1624829a8ddc1895e214` | `f359f7c039ad79f4096990a70dae91628950f229` |
| [#46](https://github.com/rafaallendes-boop/squash-clash/pull/46) | `2fbb5be72041b59a25231815270d173d2673ebc5` | `a844c76cfadb1e1c5321ff4d185d9b57f77fb79b` | `d9605b50d1b6337d7a08549473b0a9da91d25770` |
| [#47](https://github.com/rafaallendes-boop/squash-clash/pull/47) | `9af2b0c8bd92fd5d27f9b2486ea7b626da903874` | `2fbb5be72041b59a25231815270d173d2673ebc5` | `a96de94bf9a53ed4a3272c67adf4b98ed530ef12` |

Árboles obtenidos con `git rev-parse <commit>^{tree}` y `<commit>^2^{tree}`:

| PR | Árbol del merge | Comparación con el árbol del segundo padre |
|---|---|---|
| #41 | `82844ca5200c230f10dcb3e735be440ab7e6ee47` | Idéntico |
| #42 | `0d769c028c1e19518139513966056fe9cbdf130b` | **Distinto**: `a67d6af74ca8b71e59fc0eec35780b3bb59d4199` |
| #43 | `0602177e45e15d120402812c37c3d941bdd97915` | **Distinto**: `92e1570565f41c5919446473a88f259c2bf88675` |
| #45 | `d9bbdccdbe6720453ad3865975b2644991c0c835` | Idéntico |
| #46 | `f74e4825ecbad712b3da8869bb4f5b26b01fe436` | Idéntico |
| #47 | `a3b69342d9b4e1f63feadc5a299d9f17239d8ae5` | Idéntico |

**#42 y #43 no tienen árbol idéntico al segundo padre.** En #42, los ocho archivos distintos respecto del segundo padre son idénticos al primer padre; el delta del merge respecto del primer padre es solo `ShotFeedbackTests.cs`. En #43 son nueve archivos heredados idénticos al primer padre; el delta nuevo son `Sl56PostFrontisTrajectoryReport.cs`, su `.meta` y el asmdef de experimentos. Cotejado por objetos Git: las diferencias proceden de los cambios ya integrados en main, no se declara falsamente igualdad de árboles.

### Fusiones #46 y #47 — 12 de noviembre

Aprobación literal del PO, comunicada en el encargo: **«Si, apruebo fusionar»**, para fusionar **#46 y #47 antes de la capa causal**. Cierres técnicos:

- **#46**, a las **17:36:38 America/Santiago** (`20:36:38Z`): merge `2fbb5be72041b59a25231815270d173d2673ebc5`, segundo padre exacto `d9605b50d1b6337d7a08549473b0a9da91d25770`, árbol idéntico.
- **#47**, a las **17:37:09 America/Santiago** (`20:37:09Z`): base cambiada de `firstrally/gameplay` a **`main`**; el plan/PR apilada documentan la base anterior y GitHub confirma la final. Merge `9af2b0c8bd92fd5d27f9b2486ea7b626da903874`, segundo padre exacto `a96de94bf9a53ed4a3272c67adf4b98ed530ef12`, árbol idéntico.

Ramas remotas `firstrally/gameplay` y `visual/art-pass` y sus worktrees **conservados**, heads cotejados con `git ls-remote` y los checkouts locales. La build instalada y su evidencia permanecen en el worktree `visual-art-pass`. No se modificaron esas ramas ni #32.

**Mismo producto instalado que main `9af2b0c`:** `git diff --name-only c92c64c 9af2b0c` devuelve únicamente `docs/reviews/2026-11-12-visual-pass.md`. Por tanto, el código/recursos versionados coinciden; la etiqueta de la build sigue siendo `0.1.0+c92c64c dev`. Esto no afirma identidad binaria con otra build ni mediciones nuevas. Los informes de Fable que aún dicen «sin fusionar»/«sin instalar» describen su entrega anterior y quedan actualizados por este registro posterior.

**La fusión no acepta First Shot, First Rally ni First Match. Su aceptación queda para el playtest integrado.** Las observaciones H1/p95/temblor/TDD y los NotEvaluated siguen abiertos.

### Decisiones literales D1–D4 y aceptación reservada — PO, 12 de noviembre

Fuente de las tres citas siguientes: mensaje del PO del encargo documental, reproducido íntegro en el primer commit. Se conservan los signos **[…]** tal como fueron recibidos; no se reconstruye texto omitido. Contexto: [Step 0 de T4.2](https://github.com/rafaallendes-boop/squash-clash/blob/f359f7c039ad79f4096990a70dae91628950f229/docs/reviews/2026-11-11-first-shot-s4-timing.md#t42--step-0-documental-implementación-no-autorizada) y observaciones **2 y 3** de la revisión de #44 recogidas en ese informe (borde tardío y alpha cero en +ValidMs inclusivo).

> Decisiones D1–D4:
> * D1: Aprobada opción B. Ocultar completamente el indicador de timing durante el intervalo propuesto, sin pistas residuales. Mantener ayuda completa por defecto y actualizar los hashes y anclajes de AutoQA correspondientes.
> * D2: Aprobada la propuesta de Codex: 3×3 zonas, potencia en tercios y clasificación del primer bote. Son bandas provisionales de observación, no criterios de aceptación.
> * D3: Aprobada la implementación de la capa causal no direccional. Prefiero completar H3b antes de cerrar First Shot, manteniéndola como una entrega independiente.
> * D4: Aprobadas ambas correcciones del indicador mediante TDD, sin modificar las ventanas ni sus umbrales.

> Las pruebas que realizaremos ahora en el iPhone estarán enfocadas principalmente en revisar y validar lo que hizo Fable, incluyendo personajes, cancha, animaciones, efectos visuales, HUD y su integración con los controles. Las pruebas reales de jugabilidad las quiero realizar cuando tengamos First Shot y First Rally cerrados e integrados. […] No quiero declarar First Shot aceptado solo por pruebas técnicas o visuales. Podemos dejar implementadas y verificadas sus tareas, pero reservar su aceptación física definitiva para el playtest integrado.

> Objetivo: dejar First Shot y First Rally técnicamente completos, integrados y listos para una sesión real de juego en iPhone. […] Mantengamos el enfoque incremental, con pruebas automatizadas, revisión independiente y aprobación del PO antes de cada fusión.

**Histórico — estado anterior a la aprobación del Anexo C:** implementación de 4.2 y capa causal no direccional (Tarea 3.1 del plan r2) autorizadas como **entregas propias**, con pruebas automatizadas, revisión independiente y aprobación del PO antes de cada fusión. 4.4 se implementa solo después de que el PO valide el anexo C. **4.5, sesiones físicas, sigue sin autorizar.** La aceptación física definitiva de First Shot se reserva al playtest integrado; las pruebas técnicas y visuales no la sustituyen. H3b completo permanece NotEvaluated hasta completar y verificar lo pendiente. Este registro no implementa ninguna de esas tareas ni inicia sesiones.

### Aprobación literal del Anexo C — PO, 12 de noviembre de 2026

El PO aprobó el Anexo C el **2026-11-12T20:33:07Z (17:33:07 America/Santiago)**, después del encargo original de esta PR. Fuente: encargo de corrección de PR #49 tras la revisión de Claude del head `1127fbf` (**CAMBIOS SOLICITADOS**). Literal completo:

> Apruebo el Anexo C para 4.4, con las siguientes consideraciones:
>
> * X1: aceptamos los 0,5 m como umbral inicial, sujeto a calibración. Buscamos un paralelo realmente pegado a la lateral.
> * X2: validamos la capacidad de generar ese cruzado ancho, pero no lo consideremos la única trayectoria válida de un buen cruzado.
> * R1 y R2: deben cumplirse como tendencias dentro de condiciones comparables y rangos válidos, no como leyes absolutas.
> * R4: queda pendiente hasta implementar la capa causal E3.
> * X6, X7, R5 y R8: fuera del alcance actual, sin descartarlos para futuras etapas.
> * R6: diferido hasta contar con la capa aleatoria.
> * C1–C5: informativas, sin bloquear el avance.
>
> Mantengamos los escenarios reproducibles y sus resultados documentados. Si alguno falla, presenta el diagnóstico, la causa probable y las alternativas de ajuste antes de modificar física o calibración.
> Importante: estas pruebas buscan validar técnicamente lo implementado. La evaluación de si los golpes se sienten naturales y realmente representan squash la haremos en iPhone, una vez cerrados e integrados First Shot y First Rally.
> Puedes avanzar con 4.4 bajo estos criterios, sin ampliar el alcance ni introducir mecánicas nuevas.

**Estado vigente tras la aprobación del Anexo C:** **4.4 queda AUTORIZADA como entrega propia bajo todas las condiciones del literal anterior**, sin ampliar el alcance ni introducir mecánicas nuevas. Sustituye la condición pendiente del párrafo histórico anterior. Implementación de 4.2 y capa causal no direccional (Tarea 3.1 del plan r2) continúan autorizadas como entregas propias, con pruebas automatizadas, revisión independiente y aprobación del PO antes de cada fusión. **4.5, sesiones físicas, sigue sin autorizar.** La aceptación física definitiva de First Shot se reserva al playtest integrado una vez cerrados e integrados First Shot y First Rally; H3b completo permanece NotEvaluated hasta completar y verificar lo pendiente. Esta PR solo registra la autorización, sin implementar esas entregas ni iniciar sesiones.

### Verificación y entrega de este registro

Delta propio limitado a **AGENTS.md** y **docs/reviews/2026-10-25-first-shot-po-approvals.md**, conservando los originales como prefijo. Autorizaciones de los planes copiadas literalmente y decisiones D1–D4 cotejadas con el encargo del primer commit; padres/árboles, producto versionado y seis archivos de presets cotejados por lectura offline. Sin código/tests/spec/planes/criterios/defaults modificados, sin suites/AutoQA/check-release/builds/instalaciones nuevos ni acceso al iPhone. PR documental lista para revisión de Claude **coordinada por el PO**, **sin fusionar**. **Detenerse**; las entregas de implementación autorizadas requieren encargos propios.


## Encargo literal T4.2 — PO, 12 de noviembre de 2026

Encargo del PO (Squash Clash): implementar la Tarea 4.2 del plan First Shot r2 (docs/superpowers/plans/2026-10-25-squash-clash-first-shot.md, Etapa S4), con las decisiones literales del PO del 12 de noviembre de 2026:
«D1: Aprobada opción B. Ocultar completamente el indicador de timing durante el intervalo propuesto, sin pistas residuales. Mantener ayuda completa por defecto y actualizar los hashes y anclajes de AutoQA correspondientes.
D2: Aprobada la propuesta de Codex: 3×3 zonas, potencia en tercios y clasificación del primer bote. Son bandas provisionales de observación, no criterios de aceptación.
D4: Aprobadas ambas correcciones del indicador mediante TDD, sin modificar las ventanas ni sus umbrales.»
Primer commit: este encargo literal en el registro de aprobaciones (o en el informe de la entrega si A2 aún no está fusionado).

FUENTE DEL DISEÑO: tu propia propuesta del Step 0, en docs/reviews/2026-11-11-first-shot-s4-timing.md §T4.2 (niveles, clave, tareas provisionales y observaciones de #44).

BASE: main vigente al empezar. Rama y worktree propios, por ejemplo firstshot/s4-aids-tasks. Puedes copiar device.env sin cambios.

ALCANCE
1. timing.aidLevel: 0 = completa (default), 1 = parcial, 2 = ninguna. Se captura por recepción.
   - Parcial = opción B: desde T*−150 ms y hasta resolver o cerrar la recepción, no se dibuja ningún elemento de timing (anillo, triángulo fijo, bandas). Sin fade dependiente de T*. No reaparece si cambia el plan. Se registran el target y la versión que activaron el corte.
   - Ninguna: sin anillo, triángulo ni bandas desde el inicio. ImpactCue y el contrato se siguen calculando para la traza. No se ocultan mira, potencia, feedback ni otra UI.
   - Instante ideal, ventanas, compensación y contrato: sin cambios.
2. Tareas en pantalla (PracticeTask + TaskSection en DevPanel) con las bandas provisionales D2:
   - frontis en columnas X [0,20;2,20) [2,20;4,20) [4,20;6,20] e y [0,60;1,60) [1,60;2,60) [2,60;3,60] (AX-1);
   - P en tercios;
   - primer bote: X en tres franjas iguales y Z corto [0,20;3,20), medio [3,20;6,40), profundo [6,40;9,55].
   Se registran pedido, mira congelada, objetivo factible, impacto en el frontis, primer bote, potencia pedida/congelada/saturación y fallos de captura, en campos separados. Un fallo nunca se convierte en error cero. Son observación, no criterio: sin umbrales de aceptación.
   Si un borde exacto o la política de bordes no está definida en tu propuesta, decláralo en el informe y elige la regla más simple, documentada, sin presentarla como aprobada.
3. Observaciones 2 y 3 de #44 (D4), con TDD:
   - representar también el borde TARDÍO de las ventanas A/V;
   - en Δ=+ValidMs la visibilidad tiene que coincidir con el contrato (borde inclusivo).
   Probar −V, −A, 0, +A, +V y puntos a ambos lados de cada borde, cotejando estado y visibilidad contra el contrato. No recortar la inclusión para que pase el dibujo.
4. Pruebas del plan: Task_RecordsRequestedVsFinalAimInCm, Task_RecordsInvoluntaryPower, Task_RecordsFirstBounceZoneHit, AidPartial_HidesRingLast150ms (adaptada a la opción B: todo el conjunto), AidNone_ShowsOnlyBallAndShadow (respecto de las ayudas de timing). RED por aserción antes de cada GREEN.

HASHES Y ANCLAJES (D1 autoriza actualizarlos)
- La clave nueva lleva el catálogo de 107 a 108 claves.
- Calcular en Unity, desde el JSON real, el nuevo hash de defaults y el nuevo P0 efectivo. No inventarlos.
- Actualizar los anclajes de AutoQA (Tests/AutoQA/Scenario.cs, hashes SL1/SL5) y el verificador del operador (autoqa-check v7, en .claude/agents y en el skill autoqa) en un commit propio y trazable, con antes/después.
- Deben quedar idénticos: los 41 valores heredados y su proyección 0d39e6016bd8, SimulationKey 166a96e319c7 y los bytes de P0-calib-1 (no se reescribe ni se promueve).
- Declarar el nuevo P0 efectivo en el informe, para cualquier precheck futuro.
- Escenarios, expectativas, márgenes y tolerancias de AutoQA no cambian. Si un escenario cambia de resultado, detente y avisa.

VERIFICACIÓN
- scripts/test.sh editmode y playmode completos, sin otro Unity en paralelo.
- AutoQA obligatorio (toca timing y contrato): PASS / 0 / nivel 1 con los anclajes nuevos.
- scripts/check-release.sh dev/prod con gitDirty=false.
- Restaurar los ProjectSettings transitorios.

LÍMITES
- Sin iPhone, build para instalar, instalación ni sesión física (4.5 no está autorizada).
- Sin capa causal (es otra entrega).
- Sin tocar spec, criterios, umbrales, ventanas, pullFullMm, M2-4, #32, firstrally/gameplay ni visual/art-pass.
- Ante otro defecto o sorpresa: registrar y detenerse.
- Esta entrega no acepta First Shot.

ENTREGA
- Informe en docs/reviews/ con decisiones, RED→GREEN, hashes antes/después y corridas.
- PR lista para revisión independiente de Claude coordinada por el PO, sin fusionar.
- Detenerse.

Base comprobada main/origin/main: `ef9be56b382936fe948217c0efe4ee02cce28b4c`. Rama propia `firstshot/s4-aids-tasks`; primer commit exclusivamente documental.


### Resultado detenido T4.2 — 12 de noviembre de 2026

El encargo literal anterior se registró en primer commit 1f9c8fb. Implementación con TDD; fuente limpia verificada 6c149ad: 691/691 EditMode, 450/450 PlayMode. AutoQA ERROR/salida 3/nivel 3: nuevo ShotRecord incompatible con lector estricto y anclaje SL5 calculado sin la preparación human.z (esperado introducido 1877d083c345, observado Unity 50efa6d33c0c). Se registra y detiene conforme al encargo, sin reintentar/corregir tras el ERROR; check-release pendiente. Defaults c0e9f00176d5 / P0 efectivo 42d9efbf50fc; bytes/proyecciones heredadas intactos. Ver [informe con evidencia y límites](2026-11-12-first-shot-s4-aids-tasks.md). Sin fusión, aceptación ni iPhone; continuación pendiente de decisión del PO.

PR T4.2 **[#50](https://github.com/rafaallendes-boop/squash-clash/pull/50)** abierta/no borrador, marcada BLOCKED para revisión, **sin fusionar ni aptitud de fusión**. Informe y evidencia de la parada preservados; detenerse.


## Continuación literal T4.2 / PR #50 — PO, 13 de noviembre de 2026

Encargo del PO (Squash Clash) para PR #50, rama de 4.2. Continuación tras la parada S4-42-AQ-01.

DECISIÓN DEL PO: opción A. Autorizo ampliar la lista de campos predeclarados del lector de AutoQA (PocObservation) solo para los seis campos nuevos de 4.2. Primer commit: registrar esta decisión literal.

TAREA:
1. Clasificar los seis campos en PocObservation, sin tocar escenarios, expectativas, márgenes ni tolerancias:
   - AidLevel, AidCutoff, AidCutoffPlanVersion, CuePhase y Solved: comparación exacta.
   - AidCutoffTarget es un instante absoluto de reloj real: va a los excluidos, igual que TargetContactAtReal. No puede entrar en la comparación exacta.
   El fallo actual de AutoQAPocTests es el RED; documentarlo y llegar a GREEN.
2. Anclaje SL5: recalcularlo en Unity con la preparación COMPLETA del escenario (preset más human.z=(double)1.6f) y corregir el diagnóstico de TimingAidConfigTests para que calcule ese hash efectivo. No copiar el valor observado en la corrida fallida: calcularlo y después comprobar que coincide con 50efa6d33c0c. Si no coincide, registrar y detenerse.
   Corregir los cuatro lugares: Scenario.cs, autoqa-check v7 en .claude/agents/autoqa-operator.md, .claude/skills/autoqa/SKILL.md y scripts/tests/test_autoqa_checker.py.
3. Sobre el head final, un solo Unity a la vez: EditMode y PlayMode completos, AutoQA mvp (debe dar PASS/0/nivel 1, con el canario FAIL esperado), y check-release dev/prod con gitDirty=false. Restaurar los ProjectSettings transitorios.
4. Ante otro defecto o sorpresa: registrar y detenerse, sin reintentar para sacar un Pass.

LÍMITES: sin cambios de producto aparte de los ya hechos en 4.2, ni de escenarios, criterios, umbrales o defaults. Sin iPhone, builds ni instalación. Quitar [BLOCKED] del título solo si todo queda verde. PR lista para revisión de Claude, SIN FUSIONAR.

Registro previo a toda modificación de esta continuación, sobre head `58adfc9`, en la misma rama/worktree `firstshot/s4-aids-tasks`. Los resultados originales de S4-42-AQ-01 se conservan; la autorización anterior sustituye su pausa solo dentro de este alcance. Ningún check nuevo se declara ejecutado en este registro.


## First Shot T4.4 — experimento Anexo C, R3 pendiente del PO (13 nov 2026)

Encargo propio desde main **8e53b41**, rama/worktree **firstshot/s4-sport-scenarios**. Aprobación literal del PO del 12 nov registrada primero en **e65e289**; método **4d71717** y correcciones de arnés antes de las corridas definitivas. Fuente limpia **fd81c2d**: **38.304 tiros** y **72.432 pares** por corrida; cuatro archivos idénticos byte a byte en dos procesos independientes. X1–X5 con testigos para los seis contactos sintetizados aplicables; R1 **2110/2110**, R2 **3173/3463** crecientes (290 contraejemplos, sin porcentaje de aceptación nuevo); **R3 Fail, 0/42**, S4-44-R3-01, sin modificar física/solver/calibración. R7 menor duración K en **6/6** recetas SL con geometría P2 idéntica a tempo 0,5/1. C1–C4 informativas; R4/C5 E3 pendientes, X6/X7/R5/R8 fuera del alcance actual y R6 aleatoria diferida. Contacto asistido P*/T*/Δ=0 fijado por construcción, no prueba extremo a extremo del contrato.

Regresión fresca **691/691 EditMode y 450/450 PlayMode**, recuentos intactos y experimento fuera de suites/AutoQA; sin CS/skipped. AutoQA no aplica: solo arnés/docs, sin timing/gesto/contrato productivos. ProjectSettings restaurados; defaults **c0e9f00176d5**, P0 derivado **42d9efbf50fc**, SimulationKey **166a96e319c7** intactos. Datos derivados completos con SHA-256 y geometrías de los 332 contraejemplos en [informe](2026-11-13-first-shot-s4-sport-scenarios.md) y paquete versionado; prechecks preservados, sin ocultar correcciones de banda/enumeración. PR lista para revisión independiente de Claude coordinada por PO, **SIN FUSIONAR**. **Detener avance para decisión PO sobre R3 (§17.F)**; no se implementa ninguna alternativa. Sin spec/planes/criterios/umbrales/defaults/producto nuevos, #32, gameplay/arte, iPhone, build/export/instalación ni aceptación de First Shot; capa 4 reservada al playtest integrado.


## PR #51 — decisión literal del PO sobre R3 (13 de noviembre de 2026)

Fuente: encargo documental del PO del 13 de noviembre, recibido sobre head `578dfdf`, en la misma rama/worktree `firstshot/s4-sport-scenarios`. Este registro es el primer commit de la continuación, anterior al registro de cierres y observaciones.

Pregunta de Claude: **«¿Cómo tratamos R3?»**. Respuesta literal del PO: **«Open observation»**.

Significado de esa opción, según la recomendación de Claude que el PO eligió:

- R3 queda registrado como **Fail 0/42** y como **observación abierta**, igual que la saturación.
- No se cambian física, solver, calibración ni presets ahora.
- R3 se vuelve a medir después de la capa causal (**entrega propia**). Este encargo documental no ejecuta ni inicia esa medición.
- En el playtest integrado se observa si el drop desde atrás resulta demasiado letal.
- No convierte el Fail en Pass ni cambia la condición de R3.

Esta decisión sustituye el estado pendiente de decisión del PO sobre S4-44-R3-01 consignado en el registro anterior, que se conserva como histórico. **R3 sigue Fail 0/42; observación abierta, sin ajuste de producto.** Solo documentación en AGENTS.md, aprobaciones e informe de #51; arnés y evidencia intactos. Sin Unity, AutoQA, check-release, iPhone, builds ni instalación. PR #51 para verificación de Claude coordinada por el PO, **SIN FUSIONAR**; detenerse al entregar.


## PR #51 — cierres verificados y observaciones de Claude (13 de noviembre de 2026)

Continuación **solo documental** desde `578dfdf`, en la misma rama/worktree y PR #51. La decisión literal del PO «Open observation» se guardó primero en **`b4c763e`**, antes de este registro. Se conservan íntegros los registros históricos; las menciones anteriores de PR abiertas o de decisión R3 pendiente describen su estado de entonces.

### Cierres pendientes de registro, comprobados con Git

Se verificaron dos padres por merge, segundo padre exacto y la igualdad del árbol del merge con el de ese segundo padre mediante `git show -s --format=%P` y `git rev-parse <commit>^{tree}`. **Los tres datos coinciden; no hay discrepancia.** No se ejecutó ninguna fusión en esta continuación.

| PR ya fusionada | Merge | Primer padre | Segundo padre | Árbol idéntico al segundo padre |
|---|---|---|---|---|
| #48 | `5e7e7e3` | `9af2b0c` | `61afe61` | `918b047` |
| #49 | `ef9be56` | `5e7e7e3` | `4d0a353` | `97b5952` |
| #50 | `8e53b41` | `ef9be56` | `cdc7bbf` | `1c41c08` |

### Observaciones de Claude sobre #50, sin código

Fuente: revisión comunicada por el PO en este encargo; no son mediciones nuevas de esta continuación.

- **O1:** `shot-plan` se escribe en cada re-presentación; distinguir las líneas por `planVersion`.
- **O2:** `CuePhase` ignora el nivel de ayuda; leerlo junto con `AidLevel`/`AidCutoff`. No interpretar `CuePhase` por sí solo como indicador visible.
- **O3:** el corte parcial se fija por frame (hasta ~1 frame tarde, ~16 ms); observar en iPhone. Es una observación pendiente, sin sesión ni umbral de aceptación nuevo en este registro.

### Revisión de Claude sobre #51: APROBADO CON OBSERVACIONES, sin bloqueantes

Revisión del head **`578dfdfaa9c452e826f961e78a3af6ea5aee1d13`**, comunicada por el PO. Los recuentos que siguen pertenecen a Claude; **Codex no los recalcula ni ejecuta el experimento en esta continuación**. Arnés, tablas originales, testigos y evidencia permanecen intactos.

- **R3, recuento independiente:** desde `trials.tsv`, resultado idéntico **0/42**, con **18 pares con velocidad elevada y 24 sin ella**. El Fail se conserva.
- **Interpretación de la intención:** la lectura «misma intención = misma potencia» es de **Codex y no está aprobada por el PO**. Es la operacionalización de la muestra original, no una decisión normativa del PO sobre R3. La lectura alternativa —drop legal más suave de cada lado al mismo objetivo— también falla según Claude: adelante **z2≈1,56–1,68 m**, atrás **≈0,86–0,88 m**. Se registra sin sustituir ni reetiquetar la muestra original.
- **Hipótesis no demostrada:** Claude señala que P2 no modela resistencia del aire (solo gravedad, restitución y fricción) como posible explicación a investigar; no se demuestra aquí que esa ausencia cause R3. Agregar resistencia del aire sería **una mecánica nueva, fuera del alcance aprobado**. No se implementa ni se autoriza por este registro.
- **X1, representatividad:** los testigos de **B0/B3** llegan a la pared trasera sin botar, luego tocan la lateral y se deslizan pegados a ella (**x=radio**, expresado como distancia a su lateral propia en el lado espejado). Son testigos poco representativos. Según la aclaración del PO del 13 nov, los recuentos de Claude son **273 tiros X1 más limpios en B0 y en B3, y 87 en B1 y en B2** (la atribución anterior interpretó una redacción ambigua); esa caracterización no introduce un umbral aprobado ni reemplaza los testigos predeclarados.
- **Bandas alcanzables, recuentos derivados de Claude sin umbrales aprobados:** X2 con segundo bote en el cuarto trasero **49/10/10/49** para B0/B1/B2/B3; unos **430 tiros tipo C3** y unos **1.000 tipo C4**. Las bandas son alcanzables en la grilla según ese análisis; no se convierten los recuentos en criterios ni Pass nuevos y no se alteran las tablas de los testigos originales.
- **Alcance de R7:** se cumple **por construcción**, porque el tempo escala el reloj. La comparación prueba la **conversión de tiempo, no el planificador**. El uso de K obtenido del planificador real no implica que esta comparación valide su corrección. Se conserva la medición histórica con ese límite interpretativo.

### Estado vigente y límites de esta continuación

**R3 sigue Fail 0/42 y observación abierta**, igual que la saturación, por decisión literal del PO en `b4c763e`. Sin cambios de física, solver, calibración ni presets; se vuelve a medir después de la capa causal en **entrega propia**, y en el playtest integrado se observa si el drop desde atrás resulta demasiado letal. La decisión no convierte Fail en Pass ni cambia la condición de R3. Esta continuación no inicia la entrega causal, esa medición ni el playtest.

Solo se añaden registros en **AGENTS.md, aprobaciones First Shot e informe de #51**. Sin reescribir historia ni modificar arnés/evidencia/producto/tests/spec/planes/criterios/defaults. No se ejecutan Unity, suites, AutoQA o check-release; las cifras anteriores siguen siendo históricas, sin corridas nuevas. Sin iPhone, builds ni instalación. Delta documental comprobado contra `578dfdf`; todo archivo versionado fuera de los tres permitidos conserva sus bytes, y el árbol de trabajo queda limpio al entregar.

**PR #51 lista para la verificación de Claude coordinada por el PO, SIN FUSIONAR.** La aprobación de la revisión comunicada no se trata como autorización de fusión. No acepta First Shot ni el juicio deportivo reservado al playtest integrado. **Detenerse al entregar.**


## Continuación B4 / CQ-01 — decisión literal del PO (13 nov 2026)

> Apruebo conceptualmente la opción C: zona muerta geométrica, con validación técnica previa.
> Como PO, quiero priorizar una mecánica consistente, predecible y justa. Dos golpes con idéntica geometría e intención deben producir el mismo resultado, independientemente de su clasificación temporal.
> Condiciones:
> 1. Mantener las ventanas actuales de Perfecto (±40 ms) y Valid, sin modificar el contrato de contacto.
> 2. El vector de timing dependerá exclusivamente de la geometría efectiva del contacto, sin bonificaciones ni penalizaciones adicionales por categoría.
> 3. Definir una zona muerta geométrica donde el vector sea cero, con crecimiento continuo fuera de ella.
> 4. Verificar que todos los contactos Perfecto alcanzables queden cubiertos, considerando diferentes trayectorias, velocidades y estados de contacto.
> 5. Mantener independientes los efectos de posición E3/E5.
> 6. Aceptar que un Valid cercano al límite de Perfecto pueda tener idéntica salida si su geometría es equivalente.
> 7. Actualizar explícitamente §7.1 y §17.A.12 para eliminar la contradicción, sin alterar otros criterios aprobados.
> 8. Implementar TDD para continuidad, determinismo, límites y ausencia de ventajas artificiales por anticipación.
> Antes de modificar código, presentar la definición matemática propuesta, los casos límite y una evaluación del tamaño de la zona muerta. Si resulta demasiado amplia o elimina la penalización que buscamos, detenerse y presentar alternativas.
> Una vez validada la propuesta técnica, podremos autorizar la implementación mediante el flujo habitual de Codex, r

El mensaje del PO llegó cortado al final; se registra tal cual, sin completarlo. Esta continuación autoriza **solo propuesta técnica y experimento explícito** en Tests/Experiments. La implementación sigue detenida y requiere autorización nueva del PO después de la revisión de Claude. Sin producto, claves/defaults, spec/plan/criterios/ventanas/contrato, AutoQA, check-release, iPhone, builds o instalación. Unity solo para el experimento y una instancia a la vez. Método en commit previo a ejecutar; dos corridas idénticas byte a byte con SHA-256. Entrega en esta misma rama, SIN FUSIONAR.

### Cierre verificado de PR #51 y corrección X1

PO: **«Yes, merge #51»**. Git verificado antes de este registro: merge **11ac6da80a8ac04ff727cbf55f1da47396d1c725**, padres **8e53b41ce3119c03951cce3aefe5fa55a1be4f5f** y **e4386525d13815925c94e10288595ec4749e9e6f** (segundo padre exacto). Árbol de merge y segundo padre idéntico: **2665cd7315589daa721196feaf07df4fc2811207**. No se ejecuta ninguna fusión en esta continuación. No acepta First Shot; **R3 sigue Fail 0/42 como observación abierta**.

Corrección de atribución comunicada por el PO: los tiros X1 más limpios señalados por Claude son **273 en B0 y en B3, y 87 en B1 y en B2**. La atribución anterior «273/87, respectivamente B0/B3» interpretó mal una redacción ambigua de Claude. Se corrige en los tres registros, sin recalcular ni alterar arnés, evidencia, testigos o criterios de #51.


## B4 / CQ-01 — propuesta técnica entregada para revisión (13 nov 2026)

Continuación en `codex/firstshot-causal-quality`, sin producto. Primer commit **d2690c8**: decisión literal del PO, incluido final cortado «Codex, r», cierre verificado de #51 (merge 11ac6da80a8ac04ff727cbf55f1da47396d1c725 / segundo padre e4386525d13815925c94e10288595ec4749e9e6f / árbol idéntico 2665cd7315589daa721196feaf07df4fc2811207; PO «Yes, merge #51») y atribución X1 corregida: 273 en B0/B3, 87 en B1/B2. R3 conserva Fail 0/42, observación abierta; no acepta First Shot.

Método **e8a26c1**, arnés explícito/fuente limpia **ad3d259**. Dos procesos Unity secuenciales completan 1/1 cada uno, cero failed/skipped/CS, código 0; **8/8 TSV idénticos byte a byte con SHA-256**. 6 recetas SL + 24 recepciones del rival / 8 rallies, 3 tempos × 3 compensaciones: 270 condiciones, 810 zonas. Cobertura de la imagen Perfect por recepción en zonas adaptativas; no es prueba universal de ClockMap/estados. Radio fijo medido **0,31612509229966818 m**: elimina el vector de timing en Valid para SL5/SL6 en todos los tempos y para 30/30 trayectorias a tempo 0,3. Zonas adaptativas conservan parte de la penalización, pero muestran dependencia indirecta del tempo: mismo estado SL3/paso151, cero a tempo1 frente a d=0,126390479 m a tempo0,3. No se estrecha la invariante a un mismo reloj/contexto. Visibilidad perceptiva NotEvaluated. Alternativas, adenda §7.1/§17.A.12 y TDD **solo propuestos, sin elegir ni implementar**.

706 archivos protegidos intactos, ningún ProjectSettings transitorio pendiente; defaults c0e9f00176d5/108, P0 derivado 42d9efbf50fc, SimulationKey 166a96e319c7 y RallyParams actual 7d50ddaf5d4f intactos. Solo docs/evidencia y arnés Tests/Experiments. Sin producto/claves/spec/plan/criterios/ventanas/contrato, AutoQA/check-release/regresiones/H3b/RallySimTests, iPhone/build/instalación. Propuesta lista para revisión de Claude coordinada por PO, **SIN FUSIONAR**. Implementación sigue detenida y requiere autorización nueva del PO después de la revisión. **Detenerse al entregar.**

[Informe B4 y evidencia](2026-11-13-first-shot-causal-quality.md).
