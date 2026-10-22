# I3 — revisión interna independiente y resoluciones

Fecha 2026-10-22. Revisor fresco gpt-6-astra en solo lectura, rango `6f8da64..a2c8946`, sin ejecutar Unity/Xcode ni modificar la rama. No sustituye la revisión de Claude ni el checkpoint del PO. Contrastó plan T3.1–T3.3, spec, los cinco Review Focus, ledger, XML/hashes y fuentes IL2CPP generadas.

## Resultado y corrección

Fortalezas: una autoridad y dos escenas físicas aisladas; snapshots y gravedad protegidos entre fábricas; pausa en Runtime; contactos persistentes y precisión temporal declarada; regresiones reales y verificador de exclusión basado en módulos compilados. Evidencia revisada inicialmente: EditMode 109/109 y PlayMode 31/31; módulos dev Core/Runtime/DevPanel y prod Core/Runtime.

- **Critical:** ninguno.
- **Important I1:** `BallSimulationHost.cs:83` en a2c8946 mostraba Current en pausa y volvía a Lerp(previous,current,Alpha) al continuar. Tras Paso/Alpha=0, retrocedía visualmente un paso (unos 12,5 cm con el lanzamiento por defecto), aunque Current era correcto. El revisor lo dedujo del código; Codex lo reprodujo después con Unity. **RED 5/5**: continuación manual y del sistema con Alpha=0/0,5, más congelación de una imagen interpolada. Corrección: sin pasos, congelar la imagen de pausa; tras paso solicitado, sincronizar el historial de interpolación con Current. Nunca se cambia el estado físico ni el reloj para corregir la presentación. Suite PlayMode posterior **36/36 GREEN**; XML `Logs/i3-review-fix-{red,green}-results.xml`.
- **Minor M1 original:** el borrador versionado conservaba release-check como pendiente. **Reclasificación del ejecutor:** completar el informe es trabajo obligatorio ya previsto y una cuestión de exactitud de la entrega, no pulido opcional. Se actualiza con la evidencia final y el commit compilado; la comprobación del iPhone permanece explícitamente pendiente. No hay cambios de producto por M1 ni tests de prosa.

Un único pase de correcciones; no se encarga otra revisión interna del mismo rango. Minors de producto diferidos: ninguno. El resultado original era «lista para PR con I1 y actualización M1; no lista para fusionar sin Claude/PO». La disposición posterior de I1 se prueba con RED→GREEN; no se atribuye al revisor una segunda aprobación sobre código no revisado por él.

## Comportamientos que el revisor no evaluó y resolución del ejecutor

| Comportamiento | Resolución trazable y coste de dejarlo pendiente |
|---|---|
| C6: fidelidad de predicción durante vuelo | Se mantiene como dato de I4, sin regresión que asuma bit a bit. Coste: candidato P1 aún sin validar para predictor. |
| C8: Reset frente a instancia nueva | Reservado a I4; no se corrige por suposición la memoria de PhysX. Coste: posible divergencia tras relanzar por medir. |
| S10/S10b: reposo, rodamiento, vibración y NaN | Escenarios de I4, no implementarlos ahora. Coste: calidad física de reposo no certificada. |
| Penetración y precisión general | Observación del editor retenida en informe I3: centro y=−0,01165615 antes del callback. Medir en I4. Coste: no se afirma que P1 cumpla los invariantes. |
| Restitución/fricción/CCD/solver | Son valores candidatos del plan; no calibración ni elección. Coste: sensaciones y fidelidad aún sujetas al gate I4c. |
| Predicción ≤1 ms y rendimiento sostenido nativo | No evaluados; los FPS de arranque no los sustituyen. Coste: presupuesto de rendimiento todavía sin demostrar. |
| Legibilidad, encuadre, safe area y tacto en iPhone | Checkpoint del PO pendiente al conectar el dispositivo. Coste: ajustes visuales/táctiles pueden ser necesarios. |
| Bloqueo real, prod y mensajes devpanel del dispositivo | Pendiente de ejecución dev/prod con el PO. Coste: pausa y exclusión funcional nativas aún no certificadas. |
| Presets almacenados corruptos/escritura interrumpida | Persistencia de I5; I3 carga DefaultPreset empaquetado validado. Coste: recuperación de archivos locales externos no existe aún. |
| Panel completo/presets/ayudas/seguimiento | I5/I6 no autorizados; conservar solo barra mínima. Coste: depuración/calibración avanzada deberá esperar. |
| Reglas deportivas/raquetas/personajes/IA | Fuera de I3/First Ball según corresponda. Coste: esta entrega sigue siendo prototipo de pelota, no un partido. |
| Builds nativas concurrentes | El ejecutor verifica los resultados reales y la firma fuera de la revisión. Tras I1, recompilar con el código corregido. Coste: ningún binario previo se presenta como artefacto final. |

OverTop, límite operativo de diez segundos, auto-lanzamiento y yaw quedaron considerados como resoluciones trazables sin hallazgos adicionales. La revisión no aprueba P1 ni el gate de física.
