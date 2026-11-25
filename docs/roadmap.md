# Roadmap de trabajo — Squash Clash

Actualizado el 25 nov 2026 sobre `main` en `7f58679` (merge de #118). Solo documentación: no cambia criterios, umbrales, spec ni `AGENTS.md`.

**Hitos de etapa** (First Ball → First Shot → First Rally → First Match → First Fun): están en la [spec v0.3, §15](superpowers/specs/2026-10-20-squash-clash-design-v0.3.md#15-roadmap-incremental-aprobadod13). Aquí no se copian ni se modifican. Este archivo solo sigue el trabajo de las últimas jornadas.

**Cómo leerlo.** Cada dato lleva su fuente. «No verificado» significa que no pude comprobarlo en `gh`, `git` o el repo; en ese caso se anota la fuente que esperaba.

## Cerrado el 24 nov

- **PRs #106–#115, todas fusionadas el 24 nov** (verificado con `gh pr view`). `main` quedó en `c9ed8af`, el merge de #115 (verificado con `git log --first-parent`).

| PR | Tema |
|---|---|
| [#106](https://github.com/rafaallendes-boop/squash-clash/pull/106) | Recepción del saque: margen temprano de 25 ms |
| [#107](https://github.com/rafaallendes-boop/squash-clash/pull/107) | Anillos de recepción más distinguibles y «Espera el bote» |
| [#108](https://github.com/rafaallendes-boop/squash-clash/pull/108) | Pruebas de dpi herméticas |
| [#109](https://github.com/rafaallendes-boop/squash-clash/pull/109) | Anillo ámbar que cierra, anillo blanco que pulsa y texto único de la regla de recepción |
| [#110](https://github.com/rafaallendes-boop/squash-clash/pull/110) | Diagnóstico del dos paredes que sale alto |
| [#111](https://github.com/rafaallendes-boop/squash-clash/pull/111) | Definición de «rally aplicado» |
| [#112](https://github.com/rafaallendes-boop/squash-clash/pull/112) | Zona de vista previa del frontis y pista de toque bajo (dos paredes) |
| [#113](https://github.com/rafaallendes-boop/squash-clash/pull/113) | Sombra de la pelota más marcada cerca del bote (solo arte) |
| [#114](https://github.com/rafaallendes-boop/squash-clash/pull/114) | Ritual de saque del rival dentro de la pausa existente (solo vista) |
| [#115](https://github.com/rafaallendes-boop/squash-clash/pull/115) | Texto más corto del paso de dos paredes del tutorial (348 → 178 caracteres) |

- **Build de prueba 1202 instalada en el iPhone** (`0.1.0+c9ed8af dev`, 24 nov, 20:19 Chile). El primer intento falló con el error transitorio 3002 de CoreDevice; el reintento autorizado salió bien, con presets idénticos byte a byte. Fuente: `Logs/evidence/install-c9ed8af-20261124/NOTE.md`. Esa carpeta está ignorada por git y vive en el worktree `sc-install-c9ed8af`; no está en el repo.
- **M1 en 93,80 %** (3615/3854, Medio, 25 ms), fuera de la banda 80–90. El PO lo aceptó **solo para la build de prueba** (24 nov 12:07). Fuentes: [informe](reviews/2026-11-23-recepcion-b-acotada.md) y [registro del PO](reviews/2026-11-18-po-approvals.md). Para builds que no sean de prueba sigue pendiente (ver «Lo que viene»).

## Cerrado el 25 nov

| PR | Tema |
|---|---|
| [#116](https://github.com/rafaallendes-boop/squash-clash/pull/116) | Tutorial: el paso de saque explica quién saca y el partido a 7 puntos con 2 de ventaja |
| [#117](https://github.com/rafaallendes-boop/squash-clash/pull/117) | Anillos uniformes: el blanco pulsa una vez al entrar; cierre ámbar comprobado en saque y peloteo ([informe](reviews/2026-11-25-ring-entry-pulse.md)) |
| [#118](https://github.com/rafaallendes-boop/squash-clash/pull/118) | Golpe del partido en la tarjeta de resultado, con tarjeta compacta ([informe](reviews/2026-11-25-golpe-del-partido.md)) |

`main` quedó en `7f58679` (merge de #118; verificado con `git fetch` y `gh`).

## En curso

- **Tope de largo de 8,5 m para el golpe desde adelante.** Decisión del PO del 25 nov, 10:43: «Sí, tope de largo a 8,5 m». Autoriza cambiar la definición de éxito de la guardia `FrontDeepDrive` y agregar ese umbral; no autoriza ningún otro cambio de umbral o criterio.
  - Estado verificado con `git`: rama **local** `rafaallendes-boop/sc-tope-largo`, head `0812a0f`, **sin PR y sin subir a `origin`**. Nada de esto está en `main` todavía.
  - Mientras no se fusione, la spec revisada y el diagnóstico del 25 nov (`docs/reviews/2026-11-25-golpe-adelante-largo.md`) solo existen en esa rama. Por eso aquí no van como enlaces.

## Lo que viene

1. **Build de la tarde sobre la 1202.** *No verificado.* Fuente esperada: la ventana coordinada por SC-PM con Rafael (AGENTS §8). No hay registro en el repo. Además, `main` ya avanzó de `c9ed8af` a `7f58679` con #116–#118, así que «sobre la 1202» no se puede comprobar contra un SHA.
2. **Árbitro barato del frontis (reserva).** *No verificado.* No aparece en `docs/`, en `AGENTS.md` ni en los mensajes de commit. Fuente esperada: un diagnóstico o encargo de SC-Lead o SC-PM. Queda como reserva, sin trabajo iniciado que se pueda comprobar.
3. **RAL-192: renovar el perfil de firma antes del 29 nov a las 11:03 (Chile).** Verificado en Linear: Backlog, prioridad alta, fecha límite 28 nov, recurrente cada semana. Coincide con AGENTS §8: el perfil vence el 2026-11-29T14:03:33Z, que son las 11:03:33 en Chile. Se hace con una ventana con el iPhone conectado, con respaldo y cotejo antes y después.
4. **S1 en 74,6 % después de la guardia: pendiente de decisión del PO.** Fuente verificada: el diagnóstico del 25 nov, en la rama `rafaallendes-boop/sc-tope-largo` (commit `82ecfd9`, no fusionado). Con el mismo muestreo, el camino del 14 nov da 710/751 (94,5 %) y el de hoy 592/794 (74,6 %), bajo el umbral de ≥ 90 %. El tope de 8,5 m no resuelve ese choque entre S1 y la guardia. Su efecto sobre S1 no está medido. La spec revisada lo deja «pendiente del PO» y no se toca aquí.
5. **Nota en AGENTS §7 sobre M1, antes de cualquier build que no sea de prueba.** Verificado: hoy `AGENTS.md` menciona la aceptación de M1 solo en §6, y §7 no la menciona. Esta PR no toca `AGENTS.md`; la nota queda para otro cambio.

## Fuentes consultadas

`gh pr view` #106–#118, `git log --first-parent origin/main`, `git ls-remote`, Linear (RAL-192), `AGENTS.md` §6–§8, los informes citados y la evidencia local de la instalación 1202.
