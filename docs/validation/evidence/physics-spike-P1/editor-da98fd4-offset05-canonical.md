# Prueba de física

Estado: **COMPLETO**
Criterios de gate: **NO APROBADOS / NO EVALUADOS**
Este cálculo no elige backend ni autoriza el gate I4c.

Backend: P1-PhysX; variante: offset05/ccdDyn-co0.5mm-it6-1; build: editor/da98fd4
Dispositivo: editor; configuración: c86243b15a97
Perfil: editor; OS: Mac OS X 26.6.2; UTC: — → —
Térmico: not measured / editor → not measured / editor
Térmico al iniciar benchmark C17 (informativo): not measured / editor
Interrupciones de foco: 0; solo una ejecución completa permite evaluar sus criterios.
ProjectPhysics: EnhancedDeterminism=True; fastMotionThreshold=0.01; bounceThreshold=0.1; Unity=6000.3.27f1

| ID | Requisito | Escenarios | Medición | Umbral | Veredicto | Gate | evaluados/no evaluables/esperados | No aplica (fuera de población) | Cobertura |
|---|---|---|---|---|---|---|---|---|---|
| C1 | Robustez (§8.2.2, §17.A.1): Crossings | S1–S10; S8 (10 000); S8d | fallos=0; evaluados=10053; no evaluables=0; atravesamientos=0; salidas superiores=2349; seed=20261020; S8=10000; S8d=36 | 0 atravesamientos | Pass | sí | 10053/0/10053 | 0 | cerrada |
| C2 | Estados finitos: AllFinite | Todos | fallos=0; evaluados=67739; no evaluables=0 | Todos finitos | Pass | sí | 67739/0/67739 | 0 | cerrada |
| C3 | Esquinas sin ganancia (§8.2.2): MaxRelativeEnergyGain | S4, S5, S6, S8, S8d; pasos de esquina | fallos=7; evaluados=3562; no evaluables=0; no aplica (sin pasos de esquina, fuera de población)=6477; Corner [Measured=3562, NotApplicable=6477, NotEvaluable=0]: máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839515, paso=307, rapidez antes=0.0640965551 m/s, después=0.06563096 m/s; CornerWithImpact [Measured=3562, NotApplicable=6477, NotEvaluable=0]: máximo entre casos medidos: S8-3960 Measured, ganancia=0.0242687, paso=318, rapidez antes=0.0317222327 m/s, después=0.0510625839 m/s; CornerPersistentOnly [Measured=3386, NotApplicable=6653, NotEvaluable=0]: máximo entre casos medidos: S8-9456 Measured, ganancia=0.0273839515, paso=307, rapidez antes=0.0640965551 m/s, después=0.06563096 m/s | ≤ 1e-5 (ruido float) | Fail | sí | 3562/0/3562 | 6477 | cerrada |
| C4 | Sin vibración (§8.2.2): RepeatedImpacts | S4, S5, S6, S10 | fallos=0; evaluados=4; no evaluables=0; impactos repetidos=0 | 0 (\|vn\| > 0,5 m/s; no persistentes) | Pass | sí | 4/0/4 | 0 | cerrada |
| C5 | Consistencia (§17.A.2): igualdad bit a bit | S1–S7, S9, S10 nuevas; 1/100 S8 reutilizada/nueva | fallos=0; evaluados=117; no evaluables=0 | Igualdad de estados y contactos | Pass | sí | 117/0/117 | 0 | cerrada |
| C6 | Predicción desde lanzamiento (§8.2.1, §17.A.3) | S1–S10 y S8 completo; Reset(s) | fallos=0; evaluados=10017; no evaluables=0 | Igualdad bit a bit | Pass | sí | 10017/0/10017 | 0 | cerrada |
| C7 | Relanzamiento en vuelo (§17.A.3) | S1–S7, S9, S8 completo; k=30 y siguiente al primer contacto; Reset de ambos | fallos=0; evaluados=18754; no evaluables=0; Measured=18754; NotApplicable=1278; NotEvaluable=0 (no aplica fuera de población) | Igualdad bit a bit | Pass | sí | 18754/0/18754 | 1278 | cerrada |
| C8 | Reutilización (§8.2.1 cond.1) | S10→S10b (S1 reutilizada), S6→S1 frente a nueva | fallos=0; evaluados=2; no evaluables=0 | Igualdad bit a bit | Pass | sí | 2/0/2 | 0 | cerrada |
| C9 | Continuación sin reiniciar el juego (D-3) | S1, S6; k=30 | fallos=0; evaluados=2; no evaluables=0 | Diagnóstico; no decide el gate | Info | no | 2/0/2 | 0 | cerrada |
| C10 | Precisión de contacto S9: ContactPrecisionMm | S9a–i | fallos=9; evaluados=9; no evaluables=0; error máximo mm=13.6164427 | ≤ 1 mm [EXPERIMENTO, D-1] | Fail | sí | 9/0/9 | 0 | cerrada |
| C11 | Clasificación de líneas: punto observado | S9 fuera/dentro; borde exacto solo precisión | fallos=3; evaluados=6; no evaluables=0 | Igual a ExpectTouch | Fail | sí | 6/0/6 | 0 | cerrada |
| C12 | Primer impacto (§8.3.1) | S5, S6, S9 | fallos=0; evaluados=11; no evaluables=0 | Informe al gate; sin umbral inventado | Info | no | 11/0/11 | 0 | cerrada |
| C13 | Tiempo restante (§8.3.2): mediana y mínimo | Impactos únicos S1, S2, S7, S9 | fallos=0; evaluados=35; no evaluables=1; mínimo=-0.0002015136; mediana=-1.69331327E-06 | Informe al gate; ratio sin umbral | NotEvaluated | no | 35/1/36 | 0 | cerrada |
| C14 | Impactos por paso (§8.3.2) | Todos | máximo impactos no persistentes/paso=3; tope configurable=False; parámetro maxImpactsPerStep no operativo con P1 | Informe al gate; tope configurable sí/no (P1: no) | Info | no | 1/0/1 | 0 | cerrada |
| C15 | Simultáneos e instante (§8.3.3, §8.3.7) | S4, S6 | fallos=0; evaluados=2; no evaluables=0 | Mismo paso y ContactTiming; informe al gate | Info | no | 2/0/2 | 0 | cerrada |
| C16 | Penetración y muestras persistentes | Todos | penetración máxima=11.5012636 mm; muestras persistentes=898887 (todas las trayectorias ejecutadas) | Informativo | Info | no | 1/0/1 | 0 | cerrada |
| C17 | Rendimiento (§8.2.2, §17.C, D-2) | S1 hasta segundo bote; Reset incluido; 20 warmup y 200 mediciones | n=200; warmup=20; median=31.851450; p95=36.710500; p99=42.555400; max=79.240800 ms; percentiles nearest rank | p95 ≤ 1 ms en iPhone; editor informativo | Info | no | 200/0/200 | 0 | cerrada |
| C18 | Facilidad de prueba (§8.2.2) | Capabilities.RunsInEditMode | RunsInEditMode=False; timing=Estimated | Preferencia, no gate | Info | no | 1/0/1 | 0 | cerrada |

## Evidencia por caso (hasta 12 notas generales por criterio; clasificación de todas las divergencias C5–C7 y máximo C16 sin ese límite)

- C13 S2 paso=80 timing=StepOnly
- C15 S4: primer paso=21; mismo paso=False; Floor, paso=21, Estimated, t=0.007877153; RightWall, paso=22, Estimated, t=0.00111106213
- C12 S5: observado=FrontWall, punto=(0.025000, 1.490176, 0.000000), paso=29; ideal=FrontWall, t=0.2383370663362244, punto=(0.025000, 1.500000, 0.000000)
- C12 S6: observado=BackWall, punto=(6.379999, 0.991901, 9.749999), paso=24; ideal=LeftWall, t=0.19606242690409423, punto=(6.400000, 1.000000, 9.730000)
- C15 S6: primer paso=24; mismo paso=False; BackWall, paso=24, Estimated, t=0.004395852; LeftWall, paso=25, Estimated, t=5.76502366E-08
- C10 S9a: error mm=13.6156082
- C12 S9a: observado=FrontWall, punto=(3.200000, 4.543385, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.557000, 0.000000)
- C10 S9b: error mm=13.6151314
- C12 S9b: observado=FrontWall, punto=(3.200000, 4.546385, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.560000, 0.000000)
- C10 S9c: error mm=13.6160851
- C11 S9c: superficie=FrontWall; UV=(3.200000, 4.549384); observado=False, esperado=True
- C12 S9c: observado=FrontWall, punto=(3.200000, 4.549384, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.563000, 0.000000)
- C10 S9d: error mm=13.6162338
- C11 S9d: superficie=FrontWall; UV=(3.200000, 0.479384); observado=True, esperado=False
- C12 S9d: observado=FrontWall, punto=(3.200000, 0.479384, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.493000, 0.000000)
- C10 S9e: error mm=13.616353
- C12 S9e: observado=FrontWall, punto=(3.200000, 0.476384, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.490000, 0.000000)
- C10 S9f: error mm=13.6164427
- C12 S9f: observado=FrontWall, punto=(3.200000, 0.473384, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.487000, 0.000000)
- C10 S9g: error mm=13.6158466
- C12 S9g: observado=FrontWall, punto=(3.200000, 1.753384, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.767000, 0.000000)
- C10 S9h: error mm=13.6157274
- C12 S9h: observado=FrontWall, punto=(3.200000, 1.756384, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.770000, 0.000000)
- C10 S9i: error mm=13.6157274
- C11 S9i: superficie=FrontWall; UV=(3.200000, 1.759384); observado=False, esperado=True
- C12 S9i: observado=FrontWall, punto=(3.200000, 1.759384, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.773000, 0.000000)
- C7 S8-4 C7 k30: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-18 C7 postcontacto: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-25 C7 k30: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-25 C7 postcontacto: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-26 C7 k30: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-40 C7 postcontacto: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-46 C7 k30: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-46 C7 postcontacto: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-48 C7 postcontacto: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-49 C7 k30: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-49 C7 postcontacto: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-51 C7 k30: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C3 S8-314 Measured, ganancia=0.0273817144, paso=324, rapidez antes=0.06292927 m/s, después=0.06510566 m/s
- C3 S8-2902 Measured, ganancia=2.31695121E-05, paso=150, rapidez antes=5.06831074 m/s, después=5.062523 m/s
- C3 S8-3489 Measured, ganancia=0.000216089073, paso=261, rapidez antes=3.67615843 m/s, después=3.67064452 m/s
- C3 S8-3960 Measured, ganancia=0.027383836, paso=284, rapidez antes=0.06400508 m/s, después=0.0655898452 m/s
- C3 S8-7529 Measured, ganancia=0.0005172113, paso=190, rapidez antes=3.70238352 m/s, después=3.69076514 m/s
- C3 S8-7645 Measured, ganancia=0.000133205875, paso=178, rapidez antes=8.594683 m/s, después=8.591605 m/s
- C3 S8-9456 Measured, ganancia=0.0273839515, paso=307, rapidez antes=0.0640965551 m/s, después=0.06563096 m/s
- C13 ratios medidos ordenados (float32, R): -0.0002015136, -0.000104294857, -1.22524871E-05, -1.21322282E-05, -1.20591649E-05, -1.14091608E-05, -1.115016E-05, -9.025962E-06, -8.971909E-06, -6.727583E-06, -4.27332134E-06, -3.8606463E-06, -3.31149431E-06, -3.18564162E-06, -3.16796763E-06, -2.80696963E-06, -1.78454331E-06, -1.69331327E-06, -1.69331327E-06, -1.69331327E-06, -1.69331327E-06, -1.69331327E-06, -1.69331327E-06, -1.69331327E-06, -1.69331327E-06, -1.69331327E-06, -1.062513E-06, -7.40375356E-07, -4.200796E-07, -2.62972463E-07, -2.22060066E-07, -1.62029934E-07, 1.19438992E-05, 2.16999688E-05, 3.239899E-05
- C16 máximo: S8-8740 robustez; paso=284; superficie=Floor; tipo=sin contacto reportado; penetración=11.5012636 mm; rapidez=1.53928173 m/s; previo=paso=283, p=(1.40128982, 0.0210720859, 7.13596869), v=(0.0287570618, -1.4270519, -0.30344376), bits=3FB35D77,3CAC9F5E,40E459DB,3CEB93EE,BFB6A9A3,BE9B5CFB; estado=paso=284, p=(1.40152943, 0.008498736, 7.13344), v=(0.0287570618, -1.50880194, -0.30344376), bits=3FB36551,3C0B3E48,40E44524,3CEB93EE,BFC1206C,BE9B5CFB; contactos=[]; siguiente=paso=285, p=(1.40153134, 0.0144633064, 7.13342), v=(4.601106E-05, 0.7157484, -0.000485509634), bits=3FB36561,3C6CF781,40E444FA,3840FC00,3F373B4A,B9FE8C00; contactos siguientes=[Floor@285 persistent=False, StepOnly, t=NaN, p=(1.40152943, 0, 7.13344), n=(0, 1, 0), approach=(0.0287570618, -1.50880194, -0.30344376)]
