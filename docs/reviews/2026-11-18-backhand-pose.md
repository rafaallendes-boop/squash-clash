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

## Cambios (iteración 2, 19 nov, tras ver que la 1 apenas se distinguía)

Alcance ampliado por el product lead: geometría del arco del revés en SwingView e inclinación del torso. Todo acotado al revés (el lado cuyo brazo cruza el cuerpo, que para el zurdo es el contacto a su derecha); la derecha no cambia.

- `SwingView`: preparación (Back) con la cabeza a 1,68 m, sobre el hombro, del lado del contacto y fuera de la silueta del torso; terminación (Follow) alta (1,56 m) y cruzada al lado del brazo que golpea. Mientras espera un revés (plan de contacto de revés), la raqueta ya está armada en alto (visual: nada lo lee). Sin tocar tiempos de claves ni la posición/tiempo de Contact.
- `CharacterView`: preparación y terminación del figurín (rival y zurdo) con las mismas formas; el remate cruza de izquierda a derecha y acaba alto a la derecha de la cabeza; torso inclinado 9° hacia el lado del contacto; giro de hombros 62–72° al contacto; brazo libre recogido y luego abierto atrás; codo guía.
- Continuidad E1: la primera versión del remate más alto rompía la salida (exitExcess 11,8 mm frente a 5 mm en 33 ms); se acortó la distancia contacto-remate de SwingView hasta cumplirla. Criterios sin cambios; `ZE1ContactWindowTests` verde.
- **Cambio de una expectativa de E1 pedido por el product lead:** `ZE1StrokePoseTests` antes exigía que el revés del rival abriera hacia el lado del contacto (x>0,20); ahora exige que acabe del lado del brazo que golpea (x<-0,25). Es la inversión explícita de lo solicitado.

Limitación: cuando aceptación y entrega ocurren en el mismo cuadro, la clave Back nunca se muestra; la preparación visible es la raqueta ya armada en espera, más el arco hasta Contact.

## Capturas (antes / después)

Tiras de tres cuadros: preparación, contacto, remate (968 px de ancho, tres cuadros de 320). Método de E1 con la fixture `ZBackhandPoseCapture` (seed 9, Medium, cámara sin cambios).

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

`ZBackhandPoseTests` y `ZBackhandSwingViewTests` (PlayMode, dibujo). Iteración 1: RED 3 de 4 (giro 52,7°, brazo libre 0,0). Iteración 2: RED por aserción en 8 casos (cabeza de preparación 1,50 m y 1,40 m frente a ≥1,60; remate x=+0,58 y 1,40 m frente a lado del brazo y ≥1,5; raqueta en espera 0,65 m); preparación alta fuera del torso, terminación alta del lado dominante, espejo zurdo, rival, y raqueta armada en espera; GREEN todos. Compuertas finales: ver mensaje de entrega. AutoQA exento: sin cambios en SwingView, Core, gesto, timing ni contrato.

## Dudas

El cambio es moderado: con la figura tan pequeña y vista por detrás, el revés mejora en silueta (giro, brazo libre) pero sigue siendo sutil a tamaño real. Si el PO lo quiere más marcado, el siguiente paso sería inclinar el torso hacia el lado del contacto o agrandar la figura; ambos fuera de este alcance.
