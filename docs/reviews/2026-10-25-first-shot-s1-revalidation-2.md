# Segunda revalidación física de S1

Fecha: 2026-10-25. Rama `firstshot/s1-revalidation-2`, solo documental. **Estado: preparación comprobada; bloques físicos pendientes. No es entrega final ni aceptación de S1.**

Base main **f7005be63218a573dd765c66b8b1e78c4d9f0213**, merge de PR #19 aprobada por Claude en **9d961743e6598b951dd9892f2aa21d2ed1417ee0**. Head exacto y ausencia de conflictos comprobados antes de fusionar; segundo padre exacto y árbol del merge idéntico al aprobado. Main sincronizada; worktree anterior retirado sin force y artefactos preservados en `Logs/archive-s1-reticle-visibility-20261025` del checkout principal (31.292 archivos, 8.218.064.359 bytes, 260 hashes de Logs/XML, inventario coincidente). [Autorización y observaciones Claude para S2](2026-10-25-first-shot-po-approvals.md#cierre-de-pr-19-y-segunda-revalidación-autorizados-por-el-po--2026-10-25).

## App y preparación

Se usa exclusivamente la app instalada **5e218ea dev**, sin build ni instalación nuevas ni nuevas suites/exports. Antes de preparar la sesión se verificaron **399 archivos de producto versionados de Assets, ProjectSettings y Packages**: diff Git vacío 5e218ead0636d4eab23f4913b8f4911c4c117dd5→f7005be, y SHA-256 locales coincidentes con el manifiesto de las fuentes probadas de la build instalada. Los metadatos generados de build no se regeneran.

P0-calib-1, Po-1 y last-active respaldados antes de comenzar. Reinicio de la app existente mediante devicectl; arranque comprobado con P0-calib-1 **9977d9ffd7e5/cero pendientes**. Desde DEV se cambia solo `launcher.repeatSeconds=0`, se aplica/relanza sin guardar y se cierra DEV. Lanzamiento de preparación **2026-10-25T22:06:38.0264320Z**, hash **00a501ad7066**, cero pendientes, `power.minCoverageMs=30`. Los tres archivos persistidos conservan sus hashes originales. [Evidencia de preparación](../validation/evidence/2026-10-25-s1-revalidation-2.json); originales privados en `Logs/evidence/s1-revalidation-2/`.

Antes del primer bloque se mostró literalmente:

> La pelota no se golpea en S1. Arrastra despacio para mover la mira; para fijarla, haz un gesto rápido y corto, como un golpe. Abajo verás el estado (Apuntando, Fijada o Inválida) y P, la potencia.

El PO ya había identificado la mira y sus estados en la comprobación de visibilidad de 5e218ea. Esa comprobación previa se mantiene separada de esta sesión.

## Protocolo y resultados pendientes

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
