# Diagnóstico: desde adelante el golpe sale largo y pega en el vidrio de atrás

25 nov 2026 · Claude (Opus), solo lectura sobre `main` `853c9bd`, rama local `rafaallendes-boop/sc-diag-adelante` sin push ni PR. No se tocó código de producto, spec, presets ni umbrales. Hallazgo del PO en el playtest de la build 1202 (`c9ed8af`).

**Clase: hueco de la spec**, con un componente de tuning (potencia saturada). Aparte, una regresión del criterio S1 (aprobado el 14 nov) que nace de un choque entre dos specs aprobadas.

## 1. Cómo se decide la profundidad desde adelante

Orden en `ShotController.Solve` para un golpe al frontis: `ShotPace` → capa causal → tope legal → intención G1 A2 → solver → guardia «Drive adelante» (`FrontDeepDrive`, solo del humano).

| Capa | Qué hace desde adelante | Spec |
|---|---|---|
| `ShotPace` | Curva por distancia: P^γ con γ ≤ 2 cerca del frontis. **P=1 siempre da vMax = 28 m/s** | Diseño 14 nov, S1–S3 |
| Causal | Un golpe Válido sube la mira hasta +0,49 m y baja la velocidad | D36 |
| G1 A2 | Con P>0,5 busca un drive con 2.º bote cerca de z\*=7,8–8,8 m, sin cambiar la velocidad | 17 nov |
| Guardia | Con P>0,5, mira ≥0,80 m y contacto delante de la línea corta: si el golpe no llega al tercio trasero sin tocar antes la lateral, cierra la x del frontis y, si hace falta, baja la altura. **El éxito no tiene tope de largo** | 20 nov §2 y §5 |

El saque no aplica: sale del cuadro, detrás de la línea corta. El golpe a dos paredes usa otro solver y queda fuera.

## 2. Reproducción (P2-Analytic, determinista)

Experimento explícito `FrontLongDiagnosisExperiment` (rama local). Hashes iguales a §5 de AGENTS: defaults `207da8f2c636`, SimulationKey `166a96e319c7`, RallyParams `ebf8f717f8c5`. Corridas repetidas idénticas byte a byte (`diag03`=`diag04`, `var01`=`var02`, `ceil01`=`ceil02`). Evidencia en `Logs/experiments/front-long/` (ignorada).

**Golpes reales.** Las métricas de la 1202 siguen en el teléfono, así que no se usaron. Se usaron las cinco sesiones del 23 y 24 nov (builds `90ddc7e`/`f306d70`), que tienen el mismo camino de golpe que `c9ed8af` (el diff es solo de presentación). Son 146 golpes aplicados del humano, 77 de ellos desde adelante.
- 99 golpes se reconstruyen bit a bit y 29 con una diferencia ≤ 2·10⁻⁵ m/s.
- 3 difieren en 0,006–0,14 m/s; en los tres actuó la guardia.
- 15 son de dos paredes y quedan fuera de los contrafactuales.

| Golpes al frontis desde adelante | n | Vidrio al vuelo | Vidrio antes del 2.º bote |
|---|---|---|---|
| P ≤ 0,5 | 15 | 0 | 0 |
| P > 0,5, hoy | 49 | **14 (29 %)** | 31 (63 %) |
| P > 0,5, sin la guardia | 49 | 6 (12 %) | 21 (43 %) |
| P > 0,5 con P recortado a 0,75 | 49 | 5 (10 %) | 18 (37 %) |
| Desde atrás, P > 0,5 (referencia) | 61 | 8 (13 %) | 47 (77 %) |

- **La potencia satura.** 44 de 77 golpes desde adelante tienen P=1. El PO tira 36–62 mm (mediana ≈ 49 mm) y P llega a 1 con 35 mm (`pullFullMm`). En 38 de esos 44 golpes el tirón supera 40 mm, el máximo del catálogo.
- **La guardia cambia una falla por otra.** Actuó en 33 golpes. Sin ella, 27 de esos 33 habrían tocado la lateral antes del bote. Con ella, 8 de esos 27 van al vidrio al vuelo y 10 lo tocan después del bote. Los otros 6 iban al vidrio al vuelo con o sin la guardia.
- **Muy cerca del frontis** (contacto a z < 2 m) el efecto es mayor: 4 de 7 golpes al vidrio al vuelo.

**Malla determinista** (contactos a z 1–5,4 m, tres x, tres alturas, 12 miras y P=k/20; timing perfecto; 13.608 golpes). Con P=1, el vidrio al vuelo es 18,8 % con la guardia, 5,2 % con la intención sola y 11,0 % con la primitiva sola. La lateral antes del bote es 11,9 % con la guardia y 54,9 % sin ella. **G1 A2 reduce el problema y la guardia lo crea.**

**S1 (spec del 14 nov, umbral ≥ 90 %).** Se usó la misma muestra: SL5/SL6 en T\*±12, miras drop, media-baja y media, P de 0,40 a 0,60.
- **Camino del 14 nov: 710/751 (94,5 %).** Es exacto lo publicado.
- **Camino de hoy: 592/794 (74,6 %), bajo el umbral.**
  - P de 0,40 a 0,50: 100 %.
  - P=0,55: 41 %, y P=0,60: 35 %.
  - La guardia actuó en 155 de los 202 fallos.
- **Nadie lo volvió a medir.** Las puertas de G1 A2 y de la guardia no incluían S1.
- **No es lo que vio el PO.** Ninguno de sus golpes desde adelante cae entre 0,5 y 0,6 (el mínimo sobre 0,5 es 0,61).

## 3. Clasificación

- **Hueco (principal).** El código cumple cada spec al pie de la letra.
  - S3a fija que P=1 sea vMax.
  - La guardia define el éxito como «llega al tercio trasero» sin techo.
  - G1 A2 apunta al fondo.
  - Ninguna spec acota el largo de un drive fuerte desde adelante ni el vidrio al vuelo. S3b incluso cuenta como éxito la «pared trasera antes del primer bote» con P=1.
- **Tuning (contribuye).** La potencia se satura con el tirón del PO, y P=1 da 28 m/s desde cualquier distancia.
- **Choque de specs (S1).** S1 dice que P entre 0,40 y 0,60 no vaya al fondo. La guardia dice que P > 0,5 es intención profunda. Hoy S1 no se cumple. Lo resuelve el PO.

## 4. Opciones de arreglo

Las estimaciones son copias de la guardia dentro del experimento, no código de producto. El tope de 8,5 m es ilustrativo y no es un criterio.

1. **Tope de largo en la guardia** (recomendada). El éxito exige además que el primer bote caiga en z ≤ Z_max y que no toque el vidrio antes. Se aplica también al golpe que hoy queda «already-deep».
   - **Estimación con Z_max = 8,5:** los golpes reales largos (primer bote > 8,5 m o vidrio al vuelo) bajan de 20/49 a 1/49. La lateral queda igual (8/49) y la velocidad no cambia. En la malla con P=1, el largo pasa de 33 % a 0 % y la lateral se queda en 12 %.
   - **Variante «solo prohibir el vuelo»: no sirve.** La pelota bota a 9,6–9,7 m, pegada al vidrio.
   - **Riesgo:** la guardia actuaría en más golpes. Baja la altura en algunos (3 de 33 quedan bajo 0,75 m en el frontis) y su coste por golpe sin tope no está medido. Todavía 32 de 49 tocan el vidrio después del bote, que es lo normal en un drive de largo (C1). Solo afecta al humano.
   - **Aprobación del PO:** sí. Cambia la definición de éxito de la spec del 20 nov y agrega un umbral (Z_max).
2. **Resolver el choque S1–guardia.** Hay dos caminos: subir la zona de la guardia a P > 0,6, o que el PO revise S1. Su efecto sobre S1 no está medido.
   - **Riesgo:** con la primera, entre 0,5 y 0,6 vuelve la lateral antes del bote.
   - **Aprobación del PO:** sí, en los dos casos; uno cambia la spec y el otro un criterio.
3. **Potencia del gesto o techo de velocidad desde adelante.** Hay tres variantes:
   - subir `pullFullMm`, aunque el catálogo (30–40) no alcanza y habría que ampliarlo;
   - que la curva de ritmo también afecte a P=1;
   - bajar la velocidad en la guardia (con la opción 1 no hizo falta).
   - **Riesgo:** cambia la sensación en toda la cancha, se pierde el golpe fuerte desde adelante y cambian los hashes de defaults/P0 y las anclas de AutoQA. La curva de ritmo también la usa el rival.
   - **Aprobación del PO:** sí. Toca `pullFullMm` (exclusivo del PO), `DefaultPreset` (§4) y el criterio S3a.

El arrastre del aire en P2 queda descartado: es First Ball y §4, y en el diseño del 14 nov ya se vio que no corrige la subida desde adelante.

## 5. Pendiente

- Preguntar al PO si «pega en el vidrio» es al vuelo o también después del bote. La opción 1 corrige el vuelo y el bote pegado al vidrio, no el toque normal después del bote.
- Bajar las métricas de la 1202 en la próxima ventana del iPhone, para confirmar con sus golpes.
