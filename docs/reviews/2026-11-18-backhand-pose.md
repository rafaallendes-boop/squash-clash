# Pose del revés (18 nov 2026)

Implementado por subagente Sonnet dirigido por Claude; la revisión de Claude no es independiente. Rama `claude/backhand-pose` desde `main` `75b9ce8`, sin fusionar ni instalar.

**Queja del PO:** «Se puede mejorar estéticamente el golpe de revés. Ya que siempre se trata de hacer golpe de drive.»

## Diagnóstico (la elección es correcta; la pose no se lee)

Regla: `BodyFrame.SideFor` (Core, intacta) elige Drive si la pelota está del lado del brazo (x ≤ x del cuerpo; el cuerpo es siempre diestro) y Revés si no; el humano, el rival y `SwingView` leen el mismo `ContactPlan.Side`. `CharacterView` dibuja el brazo según la mano elegida (cosmética) y el lado del contacto: si el brazo cruza el cuerpo, es revés dibujado.

Medición con `ZBackhandDiagnosis` (fixture de captura, no regresión): Demo seeds 9/11/13, 150 s simulados cada una, ambas manos ([TSV](../validation/evidence/2026-11-18/backhand-pose/backhand-diagnosis.tsv)):

| Figura | Mano | Revés dibujado | Derecha dibujada |
|---|---|---|---|
| Humano | diestro | 79 de 110 (72 %) | 31 |
| Humano | zurdo | 31 de 110 (28 %) | 79 |
| Rival | diestro (fijo) | 44 de 106 (42 %) | 62 |

La pelota está a ±0,6 m del cuerpo en todos los golpes (el punto de contacto ideal), a su derecha en Drive y a su izquierda en Revés. Conclusión: se elige revés con frecuencia; el problema es visual. La capa de elección no se toca. Con la cámara trasera la figura mide unos 100 px de alto en el teléfono y el brazo que cruza queda oculto por el torso; el giro de hombros era de solo 48–53°, el brazo libre apenas se movía y la preparación ponía la raqueta cerca del hombro contrario.

## Cambios (solo `CharacterView.cs`, dibujo)

- Giro de hombros del revés: contacto 62+10·potencia° (antes 48+8·p), preparación 46–70° (antes 30–48°). La derecha no cambia.
- Preparación: raqueta atrás y alta del lado no dominante; codo guía por delante del pecho; brazo libre recogido al pecho durante la preparación y abierto hacia atrás a lo largo de la línea de hombros al pasar la raqueta (en espacio del torso, sigue el giro).
- Terminación más alta y extendida (y 1,40+0,40·p).
- La mano zurda queda espejada por construcción (todo usa `sign` y `s`).

No se tocan SwingView, Core, planificación, gesto, timing, contrato, física ni `DefaultPreset.json`. La cabeza de la raqueta sigue la clave Contact de SwingView igual que en E1; solo cambian torso, codo, brazo libre y la fase posterior al contacto.

## Capturas (antes / después)

Tiras de tres cuadros: preparación, contacto, remate (644→968 px). Método de E1 con la fixture `ZBackhandPoseCapture` (seed 9, Medium, cámara sin cambios).

| | Antes | Después |
|---|---|---|
| Humano diestro, revés | [tira](../validation/evidence/2026-11-18/backhand-pose/before/human-backhand-prep-contact-follow.png) | [tira](../validation/evidence/2026-11-18/backhand-pose/after/human-backhand-prep-contact-follow.png) |
| Humano diestro, derecha | [tira](../validation/evidence/2026-11-18/backhand-pose/before/human-forehand-prep-contact-follow.png) | [tira](../validation/evidence/2026-11-18/backhand-pose/after/human-forehand-prep-contact-follow.png) |
| Humano zurdo, revés (= contacto a su derecha) | [tira](../validation/evidence/2026-11-18/backhand-pose/before/zurdo-human-backhand-prep-contact-follow.png) | [tira](../validation/evidence/2026-11-18/backhand-pose/after/zurdo-human-backhand-prep-contact-follow.png) |
| Humano zurdo, derecha | [tira](../validation/evidence/2026-11-18/backhand-pose/before/zurdo-human-forehand-prep-contact-follow.png) | [tira](../validation/evidence/2026-11-18/backhand-pose/after/zurdo-human-forehand-prep-contact-follow.png) |
| Rival, revés | [tira](../validation/evidence/2026-11-18/backhand-pose/before/rival-backhand-prep-contact-follow.png) | [tira](../validation/evidence/2026-11-18/backhand-pose/after/rival-backhand-prep-contact-follow.png) |
| Rival, derecha | [tira](../validation/evidence/2026-11-18/backhand-pose/before/rival-forehand-prep-contact-follow.png) | [tira](../validation/evidence/2026-11-18/backhand-pose/after/rival-forehand-prep-contact-follow.png) |

Pantallas completas `*-phone.png` en las mismas carpetas. Capturas de editor, no sesión física.

## Pruebas

`ZBackhandPoseTests` (PlayMode, dibujo): RED por aserción antes de implementar (3 de 4 fallaron: giro 52,7° frente a ≥62°, brazo libre 0,0 frente a ≥0,15 m detrás de la cadera, espejo zurdo 52,7°); un control (el codo guía) ya pasaba. GREEN 4/4. Compuertas finales: ver mensaje de entrega. AutoQA exento: sin cambios en SwingView, Core, gesto, timing ni contrato.

## Dudas

El cambio es moderado: con la figura tan pequeña y vista por detrás, el revés mejora en silueta (giro, brazo libre) pero sigue siendo sutil a tamaño real. Si el PO lo quiere más marcado, el siguiente paso sería inclinar el torso hacia el lado del contacto o agrandar la figura; ambos fuera de este alcance.
