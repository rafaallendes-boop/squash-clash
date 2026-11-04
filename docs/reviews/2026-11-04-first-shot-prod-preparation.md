# First Shot — preparación PROD offline, 2026-11-04

## Alcance y estado inicial

Autorización del PO en [aprobaciones](2026-10-25-first-shot-po-approvals.md): check-release dev/prod y buildprodRelease desde el producto de devinstalada **9131b1c**, sin acceso al iPhone ni instalación. PR#30 sigue abierta/sinfusionar; sesiónfísica enpausa tras14/15,16sinpreparar. FuentesAssets/ProjectSettings/Packages/scripts comparadas con9131b1c sin diferencias al iniciar desde **dfffd58**. Evidencia previa íntegra se protege con inventariohash/tamaño/mtime fuera de Logs/evidence.

El script check-release versionado escribe resultados en Logs/evidence. Se ejecutará su mismo contenido con **solo redirección de los destinos de sus archivos** a `Logs/prod-preparation-20261104`, sin modificar el archivo ni sus comprobaciones. Exports/dev-prod y buildprod deben registrar gitDirty=false. Xcode usará destino **generic/platform=iOS**, sin consulta/conexión al iPhone, y firma local con perfil previamente incorporado/vigente, sin -allowProvisioningUpdates. Sin nuevas suites ni tests.

## Ventana de campaña pendiente

Vencimiento de referencia incorporado **2026-11-07T17:58:46Z**, sábado7nov**14:58:46America/Santiago**. Tanda2restante:16POT-F/17CAN/18–21NEG/22RIT, **37recepciones**; tanda3:23–24/**20recepciones**,16preguntasS1+preferencia; tanda4PROD:25/**3recepciones**,autorizaciónpropia/respaldo nuevo. Cerrar/restaurar/exportar/cotejar cada tanda y entregar paraClaude/PO antes de la siguiente, segúnordenPO.

Propuesta operativa de agenda, sin declarar disponibilidad/velocidad humana: reservar una ventana de **90min para restante tanda2**, otra de **60min para tanda3** y **45min paraPRODincluidosrespaldo/instalación/restauración**, más coordinación/revisión entre ellas. Son reservas de planificación, **no umbrales ni duración obligatoria de bloques**. Objetivo terminar como máximo el6nov para dejar el7nov de margen; disponibilidadPOporconfirmaralvolver. No comprometer instalación7novsi no queda todoeltramo+restauración cubierto. Si no se pueden fijar ventanas suficientes, avisar alPO **antesdeinstalar** y no avanzar instalación con perfil insuficiente. Revalidar perfil incorporado antesdecadatanda/instalación; no renovar por inferencia ni reanudar ahora.

## Resultados

Pendientes de ejecución. No build, instalación ni perfil nuevo se declaran completados en este registro inicial.
