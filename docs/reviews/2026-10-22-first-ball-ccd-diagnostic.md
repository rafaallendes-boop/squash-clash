# I4b — diagnóstico de CCD (2026-10-22)

Estado: reproducción confirmada, corrección pendiente. PR #4 borrador, sin fusionar. iPhone pausado hasta revisión de Claude. I4c/P2/I5+ no autorizados.

El defecto se reproduce **también fuera del adaptador**: esfera de radio 20 mm, 25 m/s hacia −Z, caja estática de 1 m de espesor, paso 1/120 s, ContinuousDynamic. En escenas local y por defecto, el centro pasa z=0 en el paso 2 (z=−0,116666675 m) y el callback llega en el 3. S1 tiene z=−0,1890124 en el 33 y primer contacto en el 34. Speculative reporta en el 33 sin cruce. [Trazas previas](../validation/evidence/ccd-before/ccd-s1-baseline.csv), [reproducción local](../validation/evidence/ccd-before/ccd-minimal-local.csv), [escena por defecto](../validation/evidence/ccd-before/ccd-minimal-default.csv), [speculative](../validation/evidence/ccd-before/ccd-s1-speculative.csv).

Se descartaron aisladamente FreezeRotation y varios Simulate en un mismo frame: las dos reproducciones siguieron fallando al quitar la restricción o dejar un frame entre pasos. No se cambió Reset.

Causa de configuración comprobada mediante A→B→A: `m_FastMotionThreshold=3.4028235e+38` (valor heredado del proyecto). Cambiar **solo** ese ajuste a 0,01 m convierte los tres tests a GREEN (contacto mínimo en el 2; S1 en el 33 a z=0,02). Restaurarlo reproduce los tres RED. El defecto no es exclusivo de escenas locales/manuales ni requiere cambiar el modo ContinuousDynamic.

[Unity 6.3, Physics Settings](https://docs.unity3d.com/6000.3/Documentation/Manual/class-PhysicsManager.html) documenta Fast Motion Threshold como umbral de movimiento de CCD barrida y el valor predeterminado equivalente a infinito. [Nota oficial UUM-57910](https://unity.com/releases/editor/beta/6000.0.0b13) explica su introducción para cuerpos pequeños rápidos frente a otros mayores. La relación causal en este proyecto procede de la reproducción A→B→A; no se atribuye al motor un defecto interno demostrado por su código fuente.

La corrección propuesta es **configuración requerida para la integración P1**, aplicada en el asset y ProjectSetup para conservarla al regenerar el proyecto, no un cambio de escenarios, tolerancias de aceptación, preset, geometría o variantes. El valor 0,01 m es la mitad del radio físico actual; deja de excluir desplazamientos de esta escala en la reproducción. Es un ajuste técnico explícito, no un nuevo criterio de gate ni una garantía de precisión. Su riesgo es incrementar trabajo CCD y cambiar el comportamiento/contactos que antes se resolvían por penetración; deben repetirse las cinco variantes y posteriormente el iPhone. La línea base sigue siendo ContinuousDynamic y ninguna variante se promueve.

Evidencia RED: tres tests ejecutados, cero aprobados, tres fallos de aserción; un intento anterior con API NUnit incompatible fue rechazado por el contrato de XML fresco y no cuenta como RED. [Manifiesto](../validation/evidence/2026-10-22-i4b-ccd-diagnostic.json).

La clasificación de Claude continúa **preliminar**: C1 adaptador/configuración probable; C10/C11/C13 y grandes C3 posibles consecuencias; C3 residual posible límite a reevaluar; C5–C7 indeterminados. Los resultados de la campaña nueva determinarán qué permanece, sin cambiar umbrales.
