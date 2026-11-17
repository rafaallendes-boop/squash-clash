# G1 — diagnóstico de vidrio y SL5/SL6 (fase A)

17 nov 2026 · Codex · Base `bedc2d7`, experimento `6feeb94`. **El vidrio ya es jugable; la paralela baja desde SL5/SL6 tiene una combinación estrecha.** No se reproduce el «punto casi seguro». Se mide intención limpia y planificación desde reposo, no probabilidad de ganar contra usuarios.

[Método](../validation/2026-11-17-g1-diagnosis-method.md) predeclarado en `786d54c`; [evidencia](../validation/evidence/2026-11-17-g1-diagnosis/README.md). Dos procesos: **1/1 experimento completo cada uno**, 5.988 trayectorias y 648 planes por corrida; cinco archivos idénticos byte a byte. Producto intacto.

**A1. 108 golpes fuertes; Rival en su espera, Media como principal.**

| Medida | Rival Media | Humano/Bot |
|---|---:|---:|
| Recibibles / 103 vuelos legales | 97 (79 planned + 18 stretched) | 99 (78 + 21) |
| Vidrios legales: recibibles / 61 | 59 | 59 |
| Contactos planificados tras bote y vidrio | 34 | 35 |
| P=1: recibibles / 25 vidrios legales | 23 | 23 |
| Inalcanzables por llegada tarde (b) | 6 | 4 |

Otros cinco vuelos: cuatro out en vidrio (d), uno lateral (e). En vuelos legales: **a=0, c=0**. `WallClearance` puntúa, no veta. Los 61 vidrios dejan ventana posterior: 44 después del primer bote, 17 antes. Los dos fallos del Rival con vidrio bajo (0,195/0,157 m), desde espera `(3,60;5,965)` hacia `(0,985;8,524)` / `(1,328;8,733)`, requieren 154/152 pasos frente a 117/118 disponibles: **0,308/0,283 s tarde**. Segundo bote a ≈1,20 m del vidrio. Fácil/Media/Difícil reciben 57/59/61. El Humano falla en otros vidrios: cambian espera, percepción y ruido. **El golpe de fondo ya existe implícitamente.**

**A2. Por receta (SL5 y SL6 dan los mismos recuentos):**

| Entrada | Resueltas / legales | Paralelas / cruzadas bajas | Entradas efectivas distintas |
|---|---:|---:|---:|
| Mira directa | 1.470 / 1.173 | 1 / 16 | 1.470 |
| Recorte actual 22° | 1.470 / 1.470 | 0 / 33 | 588 |

Los 33 cruzados recortados son **13 golpes efectivos**. Paralelo SL5 `A2-1962`: contacto `(2,60;0,172;1,198)`, P=0,75, mira y=0,88; frontis 0,88, ápice 1,837; botes `(x,z)=(0,915;8,910)` y `(0,780;8,956)`. SL6 es espejo. Solo una altura/potencia de la malla; sin extrapolar. Cruzados directos: P=0,60–0,95, y=0,53–1,08 según columna, no todas las parejas. El cap reduce la mira paralela a 0,664: ápice 0,853, botes x=1,833/1,563, fuera de banda paralela. El arco directo necesita **31,59°**; cap22 limita pendiente de mira, no elevación balística (23,33° recortada). Los arcos altos causan 297 out por receta; ningún TargetLowered.

**Opciones para decisión del PO; ninguna implementada:**

| Problema / opción | Efecto y riesgo | Hashes / AutoQA |
|---|---|---|
| A1: anticipar espera con trayectoria observable | Reduce viaje; afecta posicionamiento y otros SL | H0 |
| A1: preferir ventanas amplias tras vidrio | Revisar selección existente; puede retrasar contactos | H0 |
| A1: variar restitución trasera | Cambia altura/tiempo; reabre física First Ball | HP |
| A2: corregir dirección lateral de intención usando P2 | Buscar banda profunda alcanzable; afecta opciones de otros SL | H0 |
| A2: revisar cap22 según trayectoria resultante | Recupera testigo; cap mayor aislado arriesga out | HR |
| A2: solver de trayectoria según intención | Amplía cobertura potencial; cambia mira y otros SL | H0* |

**H0:** defaults/P0, SimulationKey y cuatro `Scenario.ExpectedHash` intactos; `RallyParams` tiene hash propio. **HR:** H0 cambiando solo `MaxAimElevationDeg`. **HP:** cambia defaults/P0/SimulationKey; cuatro anclas V/T3 requieren decisión del PO; A2 de cansancio no cubre G1. **H0\*:** H0 sin nuevas claves; agregarlas exige decidir hashes/anclas. Cada opción requiere TDD, suites completas, AutoQA **V y T3 PASS/0/1**, check-release y revisión, conservando escenarios/oráculos/expectativas; resultados por medir. **Fin de fase A: el PO elige fase B; sin fusión ni instalación.**
