# F1 — único ajuste predeclarado, antes de datos de la variante ajustada

17 nov 2026. Se aplica exactamente la regla fijada en `2708fb4`, aclarada antes de datos en `e173ef8`, y autorizada en §7 de la spec. No cambia el método ni los criterios.

La variante suave produjo 200 partidos / 1.893 puntos, sin abortos, errores ni contactos rechazados. B3 falla por mediana de energía al empezar el último punto **80,29191713928768**, superior a 75; 182/200 partidos bajan de onset. B4 pasa (0/200 partidos con inicio <20, cero saques a E=0) y B5a pasa (26 puntos ajustados, mediana 77,2557875945974). No hay señal baja contradictoria.

**Único ajuste:** `fatigue.recoverBetween` de **0,35 a 0,25**. Se fija antes de ejecutar la variante ajustada, con las mismas 200 semillas, rivales y reglas. No cambia costPerMeter ni otro valor. Los defaults iniciales suaves se conservan en el historial y en los datos de soft. El default de F1 pasa al candidato ajustado permitido por §7; las comprobaciones nuevas de F1 se ajustan a ese valor declarado antes de medir, y los tests heredados cambian solo sus literales derivados según la autorización del PO. Los umbrales B1–B6 y los valores/rangos restantes siguen intactos.

Se conserva el SHA-256 de todos los TSV crudos anteriores en `Logs/evidence/f1-fatigue/before-adjustment-data-sha256.json`. Los datos nuevos van en carpeta adjusted; no se sobrescriben baseline/off/soft ni se vuelven a ejecutar. Si persiste cualquier fallo de B3/B4/B5a, se reporta sin un segundo ajuste. El valor 0,25 es la única alternativa calibrada, no una búsqueda. Sin fusión ni instalación.
