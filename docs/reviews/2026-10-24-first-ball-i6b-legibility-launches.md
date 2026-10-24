# I6b / T6.3 — lanzamientos y pausa de ayudas

Estado 2026-10-24: implementación y regresión completas; build instalada, comprobación funcional nativa pendiente del PO. [PR #11 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/11). Rama `firstball/i6b-legibility-launches`. Sin fusionar ni cerrar I6b antes de revisión de Claude y decisión del PO.

## Autorización y cierres previos

Claude aprobó I6a / PR #10 head 25fedc1, cierre autorizado por PO y merge **66bac1dc541528e74dfaa2faae1a4c657e74ee42**. Main sincronizada, cierre **77ef7c4**, worktree eliminado; artefactos preservados en Logs/archive-i6a-20261024. El PO declara correcta la física por defecto y acepta **P0-calib-1** como su preset del punto de control: **Etapa 5 cerrada**. Preset hash **8de3f2ecb217**, 41 claves, única diferencia respecto a defaults `launcher.repeatSeconds=3`; sin promoción a defaults. Po-1 continúa como prueba funcional.

I6b autorizado: L1–L6 y selector en barra, más corrección del envejecimiento de ayudas durante pausa. Sombra independiente de escala visual conservada experimental, sin cambio, para juicio del PO en H1 posterior. No H1, Etapa 7 ni gameplay; spec, criterios/umbrales, escenarios de física, motores, host y reloj intactos.

## Implementación

**LaunchPresets.All(CourtDimensions, gravity)** entrega los seis Entry del plan, con LaunchSpec y HumanXZ/RivalXZ. Ballistics resuelve velocidad hacia un centro de pelota en el plano del frontis, a partir del punto objetivo y velocidad horizontal. Radio físico 20 mm, independiente del visual. Estos números describen lanzamientos experimentales; DefaultPreset.json sigue como única fuente de defaults de la app. No hubo reajuste posterior de condiciones ni de los lanzamientos para pasar pruebas.

| Id | Inicio x/y/z (m) | Objetivo frontis x/y (m) | Velocidad horizontal (m/s) |
|---|---|---|---:|
| L1 | 1 / 1,2 / 7 | 1 / 1,2 | 30 |
| L2 | 0,3 / 0,35 / 4 | 0,3 / 0,35 | 18 |
| L3 | 2 / 1 / 3 | 2 / 0,7 | 5 |
| L4 | 3,2 / 1 / 7 | 3,2 / 4,2 | 15 |
| L5 | 3,2 / 1,1 / 5 | 3,2 / 0,9 | 12 |
| L6 | 0,3 / 1,3 / 7 | 3,64 / 1,5 | 30 |

El centro objetivo z=r. Velocidad total de LaunchSpec incluye la componente vertical resuelta, y no coincide necesariamente con la horizontal de la tabla. Humano (3,2; 5,5), rival (4,5; 6,5), salvo humano L5 (3,2; 2,5) y rival L6 (6,1; 9,1). Casos resueltos con gravedad efectiva, sin configurar otro motor.

**DevBar:** selector «Legibilidad» con Manual y L1–L6. Elegir un caso configura inicio, yaw/pitch/velocidad y posiciones de referencia a través de TuningState, aplica pendientes y lanza por el host normal; conserva física/backend, cámara y ayudas. Las ediciones numéricas manuales del lanzamiento o figuras devuelven el selector a Manual; cambiar cámara no borra la selección. No escribe presets ni defaults. Lanzar/Repetir/Pausa/Paso permanecen operativos, y repetición usa el vuelo lanzado. Parámetros físicos pendientes se aplican por el mismo contrato de lanzamiento existente.

**Pausa:** la estela pasa de TrailRenderer automático a LineRenderer con muestras de posición/edad propias. Los segundos reales solo incrementan edades cuando SimClock.Paused es falso; la distancia mínima entre muestras sigue en 15 mm, longitud desde aids.trailSeconds, estrechamiento/transparencia del trazo, toggles y limpieza al relanzar conservados. En pausa los pasos individuales pueden agregar posiciones, sin envejecer la historia. Marcas usan el mismo delta efectivo: no pierden opacidad ni expiran durante pausa; al continuar vuelven a durar los segundos activos restantes. No se cambia Time.timeScale, SimClock ni física. [TrailRenderer.time](https://docs.unity3d.com/kr/6000.0/ScriptReference/TrailRenderer.html), [LineRenderer.SetPositions](https://docs.unity3d.com/kr/current/ScriptReference/LineRenderer.SetPositions.html).

## TDD y regresión

Fuente **2109af8**. **269/269 EditMode y 130/130 PlayMode**, cero fallos/omitidos, XML frescos y sin CS; 119 hashes de fuentes coincidentes. [Manifiesto](../validation/evidence/2026-10-24-i6b-tests.json).

| Grupo | RED válido antes de comportamiento | GREEN |
|---|---|---|
| Pausa | 2/2: marca desaparece y estela pierde historia al inspeccionar paso a paso | 12/12 ayudas; espera real, pasos, envejecimiento al continuar y controles previos |
| Catálogo | 7/7: seis entradas ausentes, catálogo vacío | 7/7, inicios estrictamente dentro y primer plano frontis, gravedad 5/9,81/15 |
| Física L1–L6 | 6/6: entradas ausentes | 6/6 condiciones del plan con P2/defaults |
| Selector | 2/2: campo ausente | P1/P2, seis casos, conserva física, aplica posiciones, cámara no pierde selección, ediciones manuales sí y sin escribir presets |

Diez ejecuciones válidas archivadas. GREEN fallido de pausa 11/12 no contado como aprobado: test de duración extendida esperaba dos puntos aunque uno superaba 0,3 s de edad acumulada; se inicia una historia nueva antes de comprobar la duración configurada. Sin cambio de umbral ni corrección adicional de producción. Regresión final también conserva pruebas de host parametrizadas P1/P2 y precondición de Reset de P2; no se tocó host.

Cada trayectoria completa termina tras dos botes no persistentes y no OverTop; una trayectoria incompleta no pasa. Comparación de contactos antes del segundo bote usa Step y TimeInStep, también dentro del último paso. No confundir estos tests de regresión con una campaña C1–C18.

| Caso | Condición del plan | Resultado con P2/defaults |
|---|---|---|
| L1 | BackWall antes del segundo bote | Pass |
| L2 | x≤0,30 en ≥50 % de pasos después del frontis; y≤1 en primer bote | Pass: 100 %, primer bote bajo |
| L3 | segundo bote z≤2 | Pass: 1,00812876 m |
| L4 | altura≥4; primer bote z≥7 | Pass: 4,19999266 m; 7,63591957 m |
| L5 | cámara fija→pelota a ≤0,25 m del eje humano ≥12 pasos | Pass: 46 pasos |
| L6 | BackWall y lateral a ≤1 m de la esquina, rival a ≤1 m | Pass: LeftWall (6,40; 0,18; 8,95), BackWall (6,23; 0,26; 9,75), puntos redondeados aquí |

L5 usa un test conservador: punto del segmento finito cámara–pelota a la z del eje, distancia al eje vertical real de cápsula (y=0,28..1,52). Los 46 testigos cumplen ≤0,25 m; no se cuenta una recta prolongada detrás de la pelota. L6 verifica distancia 3D a la esquina inferior (6,4; 0; 9,75) y al centro del rival (y=0,9), más estricto que solo distancia horizontal. Números evaluados con floats completos en las aserciones. Criterios aprobados intactos; no se afirma que P1 cumpla estas seis condiciones ni que otro preset arbitrario las conserve. Si una calibración posterior las rompe, solo objetivo/velocidad se reajustan en un PR explícito, según plan.

Defaults **0d39e6016bd8**, 41 claves sin cambios, P2 por defecto/P1 configurable. Captura editor generada por regresión y revisada: pelota negra, sombra, marca y cancha conservadas, sin evaluar H1. Artefactos/logs locales ignorados; evidencia versionada saneada, sin identificadores privados.

## Build y comprobación nativa

Exports limpios dev/prod **RELEASE-CHECK: OK**, metadata limpia 2109af8; prod excluye DevPanel y puente Spike, puente térmico runtime en ambos. Dev **0.1.0+2109af8**, Release ARM64 firmada y firma estricta, metadata/preset embebidos exactos, 119 hashes de fuentes cotejados. Unity 6000.3.27f1 y Xcode 27.1 (27A9275) intactos. Perfil vigente hasta **2026-10-28T14:59:31Z**, compatible con dispositivo/bundle y comprobado antes de instalar el 24. [Build saneada](../validation/evidence/2026-10-24-i6b-native-build.json).

Respaldo de presets e instalación exitosos, sin escribir presets. Proceso lanzado automáticamente; Unity inicializa y pasa a segundo plano antes del ready gestionado, sin excepción observada en esa instantánea. **No se declara todavía verificado el arranque funcional**. Duplicación pide autenticación macOS; se pidió al PO abrir directamente en el teléfono y recorrer L1–L6 con fija/seguimiento, pausa y varios Paso conservando marca/estela, Continuar reanudando edades y restaurar P0-calib-1 sin guardar. Repetición temporal 0 para permitir terminar cada vuelo; no campaña ni valoración H1. [Auditoría pendiente](../validation/evidence/2026-10-24-i6b-native-functional.json).

**PR #11 sigue borrador** hasta la respuesta/comprobación. No campaña física ni rendimiento. Renovar perfil antes de instalación desde el 27 y verificar vigencia efectiva siempre; herramientas sin cambios.

## Entrega y límites

PR/informe para Claude y decisión del PO, sin fusionar ni cerrar I6b. H1, Etapa 7 y gameplay no autorizados. La observación de sombra sin escala sigue experimental; diagnóstico de portapapeles en próxima intervención específica del panel, T7.2 con panel cerrado, retención antes de T8 y perfil desde el 27 permanecen registrados.
