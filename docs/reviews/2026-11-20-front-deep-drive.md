# Drive profundo desde adelante (SL5/SL6) — informe

20 nov 2026 · Rama `claude/front-deep-drive` desde la integración `f0fe172` (PR #84). **Implementado por un subagente Opus dirigido por Claude; la revisión de Claude no es independiente.** Pedido del PO: «a veces adelante quería tirar una pelota al fondo y se iba por las paredes laterales». [Diseño y metas predeclaradas](../superpowers/specs/2026-11-20-front-deep-drive-design.md), commiteados antes de medir la corrección (`f6cffcc`, revisión 1 `f0cdec7`). Sin push, fusión ni instalación.

## Diagnóstico (reproducción de 120 golpes del PO en `fb80015`, P2-Analytic)

Dos procesos dieron `replay.tsv` idéntico byte a byte. 88 golpes se reconstruyen bit a bit desde la solicitud. Los otros 32 difieren en ≤ 1,8·10⁻⁵ m/s, posiblemente por la aritmética del dispositivo, y en los 120 la clase coincide.

- **Tasa de hoy** (delante de la T, intención profunda): paralelas 11/16 (68,8 %) y cruzadas 3/39 (7,7 %). Todos los fallos son «lateral antes del bote», con la lateral muy pronto: z mediana 1,76 m en paralelas y 2,82 m en cruzadas.
- **Causa: el ángulo.** La mira es un punto literal del frontis. Desde adelante, el mismo punto implica un ángulo mucho mayor, y la restitución 0,55 del frontis lo abre más al volver.
  - |pendiente| mediana 0,36 en los fallos frente a 0,23 en los éxitos (paralelas); 0,77 frente a 0,36 (cruzadas).
  - La mira quedó fuera de la ventana limpia por 0,34 m (paralelas) y 0,76 m (cruzadas), y esa ventana siempre contenía la x del contacto.
- **Causas descartadas con números:**
  - el mapeo táctil M2-4 es exacto;
  - A2: la primitiva sin intención da los mismos 11/16 y 3/39, porque su dominio lateral es de ±0,32 m y no castiga la lateral;
  - la altura causal: 0/5 y 1/36 se salvan sin ella;
  - el tope legal no actuó;
  - el vector lateral causal vale 0.
- **Muy cerca del frontis (z < 2 m):** 7 cruzadas no tenían ninguna x limpia a esa altura y velocidad, porque la recta se va por arriba del fondo.

## Corrección (solo del humano, con interruptor DEV)

`FrontDeepDrive` en Core es una guardia que se aplica después de `CausalQuality.Solve`, solo desde `ShotController`.

- **Zona:** P > 0,5, mira y ≥ 0,80 m y contacto delante de la línea corta.
- **Qué hace:** si el golpe no llegaría limpio al tercio trasero, congela la velocidad y cierra la x del frontis hacia el contacto. Hace un barrido grueso en octavos más 4 bisecciones y elige el golpe más abierto que llega limpio.
- **Respaldo de altura:** solo si ninguna x sirve, baja la altura en tres niveles hasta el tin + el radio.
- **Lo que no cambia:** fuera de la zona, y cuando el golpe ya llega limpio, devuelve la solución bit a bit.
- **Lo que ve el PO:** la retícula muestra la mira efectiva (motivo `front-deep`). Hay una línea de métricas `front-deep` y el interruptor «Drive adelante: sí/no» en el panel DEV, encendido por defecto.
- **Lo que no se toca:** física, `DefaultPreset` (`207da8f2c636`), SimulationKey (`166a96e319c7`), RallyParams (`0fe523164363`), `ContactContract`, reloj y AutoQA. El rival, `RallySim` y el pronóstico de la IA no llaman a la guardia.

**TDD:**

- **RED por aserción:** EditMode 5/8 fallan y 3 controles pasan; PlayMode 1/1. En la revisión 1 se agregó una prueba EditMode más y el fixture SL5 de PlayMode siguió fallando por aserción. La prueba del interruptor DEV falló porque no existía el botón.
- **GREEN:** EditMode 9/9 y PlayMode 3/3.
- **Revisión 1, antes de medir:** el pivote único no servía en SL5 (z ≈ 1,2 m). Se cambió a barrido grueso más alturas de respaldo. El tope de evaluaciones de la prueba pasó a la constante del diseño (40). Se declara.

## Resultados (`fix01` = `fix02` byte a byte)

| Conjunto | Delante, paralelas | Delante, cruzadas | Atrás (sin cambio) |
|---|---|---|---|
| Hoy, `fb80015` (puerta) | 11/16 → **16/16 (100 %)** | 3/39 → **37/39 (94,9 %)** | 15/18 y 7/17 |
| Retención 19–20 nov, `d11f3d9` (informativa) | 4/9 → **9/9** | 6/23 → **22/23** | 13/13 y 19/32 |

- **M1 y M2 se cumplen,** sin ninguna regresión golpe a golpe.
- **Las cruzadas siguen cruzando:** las 34 corregidas botan en la otra mitad. La x del frontis se movió 0,75 m de mediana.
- **Altura:** se bajó en 5 de 39 golpes, todos muy cerca del frontis.
- **Lo que no se resolvió:** 2 cruzadas de hoy y 1 de la retención quedan `no-deep-x`.
- **Coste en el editor:** p50 0,12 ms, p95 0,32 ms y máximo 0,49 ms, con ≤ 31 solicitudes (n = 87).

## Compuertas finales (sobre `065e630`, árbol limpio antes y después)

| Compuerta | Resultado |
|---|---|
| EditMode completo | **906/906** |
| PlayMode completo | **697/697** |
| AutoQA `--suite v` | PASS / 0 / nivel 1 (102/102, 18/18), `20261120T114809Z-065e630-v-9976`, sin dirty |
| AutoQA `--suite t3` | PASS / 0 / nivel 1 (102/102, 18/18), `20261120T114826Z-065e630-t3-5317`, sin dirty |
| AutoQA `--suite volley` | PASS / 0 / nivel 1 (102/102, 18/18), `20261120T114843Z-065e630-volley-16163`, sin dirty |
| `check-release` | dev y prod OK (build 1013) |
| `ConfirmPerStyle` | **idéntica** a `confirm01`: `cells.tsv` sin la columna de segundos y `fondista-hard.tsv` |

## Riesgos y pendientes

- **No es ciego:** la guardia usa el mismo predicado que la métrica. La retención sale del mismo jugador.
- **Muestra pequeña,** de una sola persona.
- **Discontinuidad en la línea corta.** Atrás, las cruzadas siguen en 41–59 % y quedan fuera de alcance.
- **Muy cerca del frontis,** la altura de respaldo puede volver casi paralelo un cruzado pedido.
- **Sin validar en el iPhone:** falta el juicio del PO con el interruptor.
- **Candado de Unity:** un envoltorio que espera, con la ruta de Unity en sus argumentos, bloquea el `pgrep` de quien tiene el candado. Ocurrió con `tired` y se resolvió liberando el candado.

Evidencia: `Logs/experiments/front-deep-drive/` y `Logs/evidence/front-deep-drive/` (ignorados), [resumen](../validation/evidence/2026-11-20-front-deep-drive/summary.tsv).
