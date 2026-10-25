# Segunda revalidación física de S1

Fecha: 2026-10-25. Rama `firstshot/s1-revalidation-2`, solo documental. **Estado: sesión física en curso; 120 s y las cuatro series recuperados; distancia sin verificar, juicios del PO y restauración final pendientes. No es entrega final ni aceptación de S1.**

Base main **f7005be63218a573dd765c66b8b1e78c4d9f0213**, merge de PR #19 aprobada por Claude en **9d961743e6598b951dd9892f2aa21d2ed1417ee0**. Head exacto y ausencia de conflictos comprobados antes de fusionar; segundo padre exacto y árbol del merge idéntico al aprobado. Main sincronizada; worktree anterior retirado sin force y artefactos preservados en `Logs/archive-s1-reticle-visibility-20261025` del checkout principal (31.292 archivos, 8.218.064.359 bytes, 260 hashes de Logs/XML, inventario coincidente). [Autorización y observaciones Claude para S2](2026-10-25-first-shot-po-approvals.md#cierre-de-pr-19-y-segunda-revalidación-autorizados-por-el-po--2026-10-25).

## App y preparación

Se usa exclusivamente la app instalada **5e218ea dev**, sin build ni instalación nuevas ni nuevas suites/exports. Antes de preparar la sesión se verificaron **399 archivos de producto versionados de Assets, ProjectSettings y Packages**: diff Git vacío 5e218ead0636d4eab23f4913b8f4911c4c117dd5→f7005be, y SHA-256 locales coincidentes con el manifiesto de las fuentes probadas de la build instalada. Los metadatos generados de build no se regeneran.

P0-calib-1, Po-1 y last-active respaldados antes de comenzar. Reinicio de la app existente mediante devicectl; arranque comprobado con P0-calib-1 **9977d9ffd7e5/cero pendientes**. Desde DEV se cambia solo `launcher.repeatSeconds=0`, se aplica/relanza sin guardar y se cierra DEV. Lanzamiento de preparación **2026-10-25T22:06:38.0264320Z**, hash **00a501ad7066**, cero pendientes, `power.minCoverageMs=30`. Los tres archivos persistidos conservan sus hashes originales. [Evidencia de preparación](../validation/evidence/2026-10-25-s1-revalidation-2.json); originales privados en `Logs/evidence/s1-revalidation-2/`.

Antes del primer bloque se mostró literalmente:

> La pelota no se golpea en S1. Arrastra despacio para mover la mira; para fijarla, haz un gesto rápido y corto, como un golpe. Abajo verás el estado (Apuntando, Fijada o Inválida) y P, la potencia.

El PO ya había identificado la mira y sus estados en la comprobación de visibilidad de 5e218ea. Esa comprobación previa se mantiene separada de esta sesión.

## Protocolo y resultados en curso

1. 120 s con la **mano izquierda dominante**, desviación declarada/autorizada del plan que pedía derecha; derecha opcional, no presumida.
2. Regla física de 50 mm; distinguir distancia declarada automáticamente de recorrido confirmado por el PO.
3. Diez suaves y diez fuertes con cobertura **30 ms**.
4. Diez suaves y diez fuertes con **20 ms**, aplicado sin guardar y comprobado antes; después volver a 30. Orden **30→20**, posible aprendizaje/fatiga.

Los botones están dentro de DEV y lo cierran al activarse. Registrar todos los intentos, incluidos toques/cancelaciones, y hash por lanzamiento. Gestos con configuración controlada distinta quedan no comparables. Se solicitará confirmación de cada bloque y se recuperará evidencia antes de continuar. No se han inferido todavía frecuencias, mm físicos ni resultados.

El informe final incluirá, por serie, mínimo/mediana/máximo de potencia, conservadores por motivo, separación suave/fuerte y porcentaje Frozen; saltos al fijarse p50/p95/máximo; H3/H4 descriptivos (pico por gesto frente a 250 mm/s, proporción en y=5 m o fuera). No se cambia parámetro alguno fuera de los dos ajustes temporales autorizados. Si el PO juzga inaceptables los saltos, proponer ajuste para PR aparte antes de S2 sin implementarlo.

Las cinco preguntas se recogerán por separado y literalmente: ¿Apuntas donde quieres?; ¿Entiendes siempre dónde apuntas?; ¿El número responde a suave/fuerte?; ¿Los saltos de la mira al fijarse son aceptables?; ¿Notaste diferencia entre 30 y 20 ms? No se infieren respuestas de comentarios anteriores.

## Cierre pendiente

Restaurar P0-calib-1 al acabar y comprobar **9977d9ffd7e5/cero pendientes** y los tres archivos byte a byte iguales al respaldo. Datos nuevos sin mezclar con 311cb11 (histéresis defectuosa), c5a7d1e (mira tapada) ni la comprobación breve de visibilidad anterior. Si aparece defecto, registrarlo y detenerse.

El cierre será una PR documental lista para Claude, **sin fusionar**, con AGENTS y aprobaciones actualizados. Sin código, defaults, spec, plan ni criterios. Defaults 00a501ad7066, 41 valores heredados/proyección 0d39e6016bd8 y SimulationKey 166a96e319c7 intactos. **S1 sigue sin aceptar y S2–S4/T8 sin autorizar.** Comprobar siempre perfil antes de una futura instalación y renovarlo primero desde el 27; no hay instalación en esta sesión.

### Bloque de 120 s recuperado

El PO responde literalmente «listo» al pedir completar 120 s con la izquierda. Registro completo **22:13:52.9503240Z**, **120,014203167 s**, **2449 muestras/tiempos distintos**, sin duplicados, desorden ni edades negativas; hash **00a501ad7066**, cero pendientes. Stats informa **50,611337 Hz entregados** entre intervalos de muestras de un mismo toque (no tasa hardware ni 2449/120); intervalo p50/p95 **16,7/37,6 ms**, hueco máximo **300,039 ms**, máximo **2 muestras/frame**; edad p50/p95/máxima **13,2/25,8/28,242 ms**. Resolución de histogramas 0,1 ms, sin overflow. No se presume 120 Hz.

Se conserva también una adquisición posterior **incompleta de 12,549327917 s**, 28 muestras, **system-cancel**, registro **22:15:21.0169180Z**, mismo hash/cero pendientes; no cuenta como bloque completo ni se atribuye la causa de interrupción. Hasta esta recuperación hay **92 gestos registrados** entre ambas adquisiciones, incluidos un Tap y 91 Swing; no se equiparan sus 92 intentos al único intervalo completo. Todos conservan la configuración controlada de 30 ms. Presets/selección continúan idénticos al respaldo. La regla se solicita como siguiente bloque; aún no hay series ni experimento de 20 ms.

### Distancia y series a 30 ms recuperadas

Respuesta literal del PO:

> Listo. Realicé las 3 pruebas. No tengo regla en la casa, pero veo bien la distancia. Por otro lado, los 10 suaves salieron bien. 10 fuertes, para mover ese rombo tengo que apuntar con el dedo y luego acelerar rapido, eso a veces hacia que el golpe salga debil porque levantaba el dedo. Pero reo que salió bien. Realiza la revisión

No había regla física: **verificación de mm NotEvaluated**, sin DpiSource=PhysicallyVerified ni inferencia de precisión por la estimación visual del PO. Se conservaron **6 intentos** etiquetados ruler-50mm: **29,022 / 33,735 / 38,333 / 35,284 / 46,214 / 52,242 mm calculados**. Resolución reportada 1206×2622, Screen.dpi=460, fuente **Reported**, escala 0,055217389 mm/px. La declaración automática de 50 mm del protocolo no certifica el recorrido real; mano no confirmada en esa respuesta. Los intentos posteriores a las series se conservan en su orden temporal.

| Serie a 30 ms | Intentos | P mínima / mediana / máxima | Conservadores por motivo | Fijada |
|---|---:|---|---|---:|
| Suaves | 10 Swing | 0 / 0 / 0 % | 4 few-samples, 3 sample-gap | 0/10 (0 %) |
| Fuertes | 10 Swing | 0 / 1,977 / 20,523 % | 1 few-samples, 2 sample-gap | 5/10 (50 %) |

Diferencia de medianas fuerte−suave **1,977 puntos porcentuales**; los rangos se superponen en cero. Saltos de los **5 Frozen fuertes**: p50 **1,661573 m**, p95/máximo **1,816027 m** (rango más próximo, ceil(n·p/100)). Se describen datos, no aceptación ni juicio de saltos. Todos los **118 gestos registrados hasta aquí**, incluyendo 92 de adquisiciones, 6 de distancia y 20 de series, tienen hash controlado **00a501ad7066**, cobertura 30 y cero pendientes; ninguno queda excluido por cambio de configuración. Presets/selección siguen idénticos al respaldo. El comentario del PO sobre levantar el dedo y P baja se conserva como observación, sin atribuir una causa técnica no comprobada ni tratarlo como respuesta separada a las cinco preguntas.

Pendiente el experimento **20 ms**: se solicita bloquear el iPhone para ajustar solo power.minCoverageMs desde el panel y verificar antes de las dos series. No se solicita repetir distancias sin regla y no se cambia otro parámetro.

### Preparación de 20 ms comprobada

Se cambia únicamente power.minCoverageMs y se pulsa Aplicar y relanzar sin guardar: lanzamiento **2026-10-25T23:11:36.7177380Z / a49c48e3d019 / cero pendientes**, repetición=0, DEV cerrado. Se verificó la etiqueta **Minimum coverage · 20 ms** antes de relanzar; presets/selección siguen idénticos al respaldo. La entrada remota de 20 por teclado dejó transitoriamente la etiqueta en 10 ms; se corrigió con el deslizador antes del relanzamiento. No se registró lanzamiento en 10 ni aumentó el número de gestos entre las dos recuperaciones (**118 antes/después**); no hay mediciones de esa preparación para mezclar con las series. No se atribuye una causa técnica al comportamiento de la entrada remota. Se solicita únicamente 10 suaves y 10 fuertes, conservando todos los intentos y el mismo tipo de gesto, y confirmación de mano usada a 30/20. Resultados de 20 ms aún pendientes.

### Resultado de las cuatro series (30→20 ms)

El PO confirma las series de 20 ms con la respuesta literal «Listo!». Se recuperan **10 suaves y 10 fuertes**, trials 1–10 sin reemplazos, todos Swing. No respondió todavía por separado sobre la mano usada. Todos los **138 gestos / 4683 muestras** recuperados desde el límite de sesión están incluidos en el JSON; 118 a30 y20 a20, con hashes controlados y cero pendientes. **Ninguno excluido por cambio de configuración**. No se mezclan adquisiciones anteriores a esta sesión.

| Cobertura / serie | N | P mínima / mediana / máxima (%) | Conservadores por motivo | Fijada |
|---|---:|---|---|---:|
| 30 ms suaves | 10 | 0 / 0 / 0 | 4 few-samples, 3 sample-gap | 0/10 (0 %) |
| 30 ms fuertes | 10 | 0 / 1,977 / 20,523 | 1 few-samples, 2 sample-gap | 5/10 (50 %) |
| 20 ms suaves | 10 | 0 / 0 / 0 | 8 few-samples, 1 sample-gap | 0/10 (0 %) |
| 20 ms fuertes | 10 | 0 / 7,123 / 18,778 | 1 sample-gap | 9/10 (90 %) |

Mediana fuerte−suave: **1,977 puntos a30**, **7,123 puntos a20**. Rangos superpuestos en cero en ambas condiciones. Se incluyen todos los intentos, también conservadores/P=0. La mediana de potencia es el punto medio de los dos valores centrales para N par. **No se aísla un efecto causal del parámetro**: son gestos distintos y orden fijo 30→20, con posible aprendizaje/fatiga y mano aún sin confirmar. Mediana del pico de velocidad en fuertes: **290,545 mm/s a30**, **457,736 mm/s a20**. Además, minCoverageMs no interviene en AimModel: el aumento de Frozen no se atribuye a cobertura. **No se propone ni promueve un default nuevo.**

| Saltos al fijarse | Frozen N | p50 (m) | p95 (m) | Máximo (m) |
|---|---:|---:|---:|---:|
| Fuertes 30 ms | 5 | 1,661573 | 1,816027 | 1,816027 |
| Fuertes 20 ms | 9 | 1,639359 | 2,667281 | 2,667281 |
| Todos los intentos registrados de esta sesión | 54 | 1,570968 | 2,166354 | 2,667281 |

Suaves: cero Frozen, percentiles de salto **NotEvaluated**, no cero artificial. Percentiles de salto: rango más próximo, índice ceil(N·p/100); cifras de FreezeJumpMeters originales, sin suavizado ni replay. El total incluye las adquisiciones y la distancia, sin confundirlo con las series. Pendiente el juicio literal del PO sobre aceptabilidad.

H3, picos **mm/s por gesto** de las cuatro series (diferencia de posiciones de muestras consecutivas / dt positivo × mmPerPx; cálculo descriptivo offline, no replay):

| Intento | Suave 30 | Fuerte 30 | Suave 20 | Fuerte 20 |
|---|---:|---:|---:|---:|
| 1 | 88,901 | 249,043 | 103,496 | 536,766 |
| 2 | 105,274 | 478,156 | 112,462 | 270,144 |
| 3 | 106,668 | 278,337 | 92,973 | 428,144 |
| 4 | 136,205 | 263,105 | 74,386 | 595,230 |
| 5 | 160,333 | 851,869 | 117,407 | 455,988 |
| 6 | 273,047 | 717,200 | 135,816 | 459,483 |
| 7 | 100,250 | 248,673 | 71,354 | 318,787 |
| 8 | 121,750 | 351,527 | 88,877 | 443,049 |
| 9 | 103,495 | 302,754 | 129,023 | 487,678 |
| 10 | 130,922 | 172,920 | 68,246 | 532,385 |

Por encima de **250 mm/s**: suaves30 **1/10**, fuertes30 **7/10**, suaves20 **0/10**, fuertes20 **10/10**. Un pico aislado no garantiza Frozen: siguen haciendo falta dos intervalos consecutivos sobre el umbral. El JSON conserva también los picos de los otros 98 gestos y cada traza original. No se modifica umbral, histéresis ni ganancia.

H4 (posición final del centro): **y=5 m exacto**, o fuera de los límites verticales **y≤tin=0,48 / y≥out=4,57 m**; no se clasifica usando el diámetro visual de 0,84 m.

| Grupo | En y=5 m | Fuera tin/out | Unión (en tope o fuera) |
|---|---:|---:|---:|
| Suaves30 | 0/10 | 0/10 | 0 % |
| Fuertes30 | 2/10 | 4/10 | 40 % |
| Suaves20 | 0/10 | 0/10 | 0 % |
| Fuertes20 | 0/10 | 0/10 | 0 % |
| Total sesión | 16/138 (11,594 %) | 29/138 (21,014 %) | 21,014 % |

H3/H4 se registran para evaluación posterior, **sin ajustes**. Las cinco preguntas se han solicitado por separado, además de la mano usada; no se infieren respuestas de «bien» o «Listo!». Pendiente volver a30 y restaurar P0-calib-1 al terminar.

### Restauración final aún pendiente de acceso

Se opta por recargar P0-calib-1 mediante reinicio de la app existente: restaura **coverage=30/repeat=3 juntos**, sin lanzamiento intermedio de30/repeat0, sin guardar preset ni nueva instalación. El primer intento fue rechazado con **CoreDevice10002/dispositivo bloqueado**; la duplicación pide autenticación local del Mac. Se solicitó al PO completar el aviso allí, sin comunicar credenciales. **No se da por restaurado hasta recuperar un nuevo arranque9977d9ffd7e5/cero pendientes y cotejar los tres archivos.** No se atribuye el bloqueo de acceso a un defecto del producto.
