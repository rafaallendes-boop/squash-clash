# Prueba de física

Estado: **COMPLETO**
Criterios de gate: **NO APROBADOS / NO EVALUADOS**
Este cálculo no elige backend ni autoriza el gate I4c.

Backend: P1-PhysX; variante: solver12/ccdDyn-co1mm-it12-4; build: editor/ee76086
Dispositivo: editor; configuración: c86243b15a97
Perfil: editor; OS: Mac OS X 26.6.2; UTC: — → —
Térmico: not measured / editor → not measured / editor
ProjectPhysics: EnhancedDeterminism=True; bounceThreshold=0.1; Unity=6000.3.27f1

| ID | Requisito | Escenarios | Medición | Umbral | Veredicto | Gate | evaluados/no evaluables/esperados | Cobertura |
|---|---|---|---|---|---|---|---|---|
| C1 | Robustez (§8.2.2, §17.A.1): Crossings | S1–S10; S8 (10 000); S8d | fallos=8010; evaluados=10053; no evaluables=0; atravesamientos=137936; salidas superiores=1745; seed=20261020; S8=10000; S8d=36 | 0 atravesamientos | Fail | sí | 10053/0/10053 | cerrada |
| C2 | Estados finitos: AllFinite | Todos | fallos=0; evaluados=67787; no evaluables=0 | Todos finitos | Pass | sí | 67787/0/67787 | cerrada |
| C3 | Esquinas sin ganancia (§8.2.2): MaxRelativeEnergyGain | S4, S5, S6, S8, S8d; pasos de esquina | fallos=23; evaluados=1436; no evaluables=415; no aplica (sin pasos de esquina, fuera de población)=8188; Corner [Measured=1436, NotApplicable=8188, NotEvaluable=415]: máximo entre casos medidos: S8-4348 Measured, ganancia=0.7470622, paso=14, rapidez antes=1.022299 m/s, después=1.11270988 m/s; CornerWithImpact [Measured=1588, NotApplicable=8188, NotEvaluable=263]: máximo entre casos medidos: S8-7425 Measured, ganancia=0.0168201, paso=112, rapidez antes=0.134184122 m/s, después=0.09717035 m/s; CornerPersistentOnly [Measured=1223, NotApplicable=8401, NotEvaluable=415]: máximo entre casos medidos: S8-4348 Measured, ganancia=0.7470622, paso=14, rapidez antes=1.022299 m/s, después=1.11270988 m/s | ≤ 1e-5 (ruido float) | Fail | sí | 1436/415/1851 | cerrada |
| C4 | Sin vibración (§8.2.2): RepeatedImpacts | S4, S5, S6, S10 | fallos=0; evaluados=4; no evaluables=0; impactos repetidos=0 | 0 (\|vn\| > 0,5 m/s; no persistentes) | Pass | sí | 4/0/4 | cerrada |
| C5 | Consistencia (§17.A.2): igualdad bit a bit | S1–S7, S9, S10 nuevas; 1/100 S8 reutilizada/nueva | fallos=0; evaluados=117; no evaluables=0 | Igualdad de estados y contactos | Pass | sí | 117/0/117 | cerrada |
| C6 | Predicción desde lanzamiento (§8.2.1, §17.A.3) | S1–S10 y S8 completo; Reset(s) | fallos=96; evaluados=10017; no evaluables=0 | Igualdad bit a bit | Fail | sí | 10017/0/10017 | cerrada |
| C7 | Relanzamiento en vuelo (§17.A.3) | S1–S7, S9, S8 completo; k=30 y siguiente al primer contacto; Reset de ambos | fallos=155; evaluados=18778; no evaluables=1254 | Igualdad bit a bit | Fail | sí | 18778/1254/20032 | cerrada |
| C8 | Reutilización (§8.2.1 cond.1) | S10→S10b (S1 reutilizada), S6→S1 frente a nueva | fallos=0; evaluados=2; no evaluables=0 | Igualdad bit a bit | Pass | sí | 2/0/2 | cerrada |
| C9 | Continuación sin reiniciar el juego (D-3) | S1, S6; k=30 | fallos=0; evaluados=2; no evaluables=0 | Diagnóstico; no decide el gate | Info | no | 2/0/2 | cerrada |
| C10 | Precisión de contacto S9: ContactPrecisionMm | S9a–i | fallos=9; evaluados=9; no evaluables=0; error máximo mm=17.86533 | ≤ 1 mm [EXPERIMENTO, D-1] | Fail | sí | 9/0/9 | cerrada |
| C11 | Clasificación de líneas: punto observado | S9 fuera/dentro; borde exacto solo precisión | fallos=2; evaluados=6; no evaluables=0 | Igual a ExpectTouch | Fail | sí | 6/0/6 | cerrada |
| C12 | Primer impacto (§8.3.1) | S5, S6, S9 | fallos=0; evaluados=11; no evaluables=0 | Informe al gate; sin umbral inventado | Info | no | 11/0/11 | cerrada |
| C13 | Tiempo restante (§8.3.2): mediana y mínimo | Impactos únicos S1, S2, S7, S9 | fallos=0; evaluados=3; no evaluables=37; mínimo=1.16505456; mediana=1.2733072 | Informe al gate; ratio sin umbral | NotEvaluated | no | 3/37/40 | cerrada |
| C14 | Impactos por paso (§8.3.2) | Todos | máximo impactos no persistentes/paso=3; tope configurable=False; parámetro maxImpactsPerStep no operativo con P1 | Informe al gate; tope configurable sí/no (P1: no) | Info | no | 1/0/1 | cerrada |
| C15 | Simultáneos e instante (§8.3.3, §8.3.7) | S4, S6 | fallos=0; evaluados=2; no evaluables=0 | Mismo paso y ContactTiming; informe al gate | Info | no | 2/0/2 | cerrada |
| C16 | Penetración y muestras persistentes | Todos | penetración máxima=44927.8 mm; muestras persistentes=2158187 (todas las trayectorias ejecutadas) | Informativo | Info | no | 1/0/1 | cerrada |
| C17 | Rendimiento (§8.2.2, §17.C, D-2) | S1 hasta segundo bote; Reset incluido; 20 warmup y 200 mediciones | n=200; warmup=20; median=13.768450; p95=15.024300; p99=15.476700; max=15.703500 ms; percentiles nearest rank | p95 ≤ 1 ms en iPhone; editor informativo | Info | no | 200/0/200 | cerrada |
| C18 | Facilidad de prueba (§8.2.2) | Capabilities.RunsInEditMode | RunsInEditMode=False; timing=Estimated | Preferencia, no gate | Info | no | 1/0/1 | cerrada |

## Evidencia por caso (hasta 12 ejemplos por criterio; contadores completos arriba)

- C1 S1: FrontWall step=33: center beyond plane; FrontWall step=34: center beyond plane; BackWall step=111: center beyond plane
- C13 S1 paso=34 timing=StepOnly
- C13 S1 paso=112 timing=StepOnly
- C13 S1 paso=122 timing=StepOnly
- C13 S1 paso=148 timing=StepOnly
- C1 S2: Floor step=89: center beyond plane
- C13 S2 paso=64 timing=StepOnly
- C13 S2 paso=90 timing=StepOnly
- C1 S4: RightWall step=22: center beyond plane
- C15 S4: primer paso=22; mismo paso=False; Floor, paso=22, StepOnly, t=NaN; RightWall, paso=23, StepOnly, t=NaN
- C1 S5: FrontWall step=29: center beyond plane; RightWall step=29: center beyond plane; Floor step=105: center beyond plane
- C12 S5: observado=RightWall, punto=(0.000000, 1.486175, -0.021631), paso=30; ideal=FrontWall, t=0.2383370663362244, punto=(0.025000, 1.500000, 0.000000)
- C6 S5: primera diferencia índice=30
- C1 S6: BackWall step=24: center beyond plane; LeftWall step=24: center beyond plane; RightWall step=115: center beyond plane
- C12 S6: observado=LeftWall, punto=(6.400000, 0.987962, 9.804909), paso=25; ideal=LeftWall, t=0.19606242690409423, punto=(6.400000, 1.000000, 9.730000)
- C15 S6: primer paso=25; mismo paso=True; LeftWall, paso=25, StepOnly, t=NaN; BackWall, paso=25, StepOnly, t=NaN
- C6 S6: primera diferencia índice=25
- C1 S7: BackWall step=31: center beyond plane; Floor step=101: center beyond plane; Floor step=102: center beyond plane
- C13 S7 paso=32 timing=StepOnly
- C13 S7 paso=102 timing=StepOnly
- C13 S7 paso=164 timing=StepOnly
- C1 S9a: BackWall step=182: center beyond plane; Floor step=332: center beyond plane
- C10 S9a: error mm=1.51920319
- C12 S9a: observado=FrontWall, punto=(3.200000, 4.555481, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.557000, 0.000000)
- C13 S9a paso=41 timing=StepOnly
- C13 S9a paso=183 timing=StepOnly
- C13 S9a paso=234 timing=StepOnly
- C1 S9b: BackWall step=182: center beyond plane
- C10 S9b: error mm=1.50680542
- C12 S9b: observado=FrontWall, punto=(3.200000, 4.558493, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.560000, 0.000000)
- C1 S9c: BackWall step=182: center beyond plane; Floor step=234: center beyond plane; Floor step=235: center beyond plane
- C10 S9c: error mm=1.49536133
- C12 S9c: observado=FrontWall, punto=(3.200000, 4.561504, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.563000, 0.000000)
- C10 S9d: error mm=17.84116
- C11 S9d: superficie=FrontWall; UV=(3.200000, 0.475159); observado=True, esperado=False
- C12 S9d: observado=FrontWall, punto=(3.200000, 0.475159, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.493000, 0.000000)
- C10 S9e: error mm=17.85332
- C12 S9e: observado=FrontWall, punto=(3.200000, 0.472147, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.490000, 0.000000)
- C10 S9f: error mm=17.86533
- C12 S9f: observado=FrontWall, punto=(3.200000, 0.469135, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.487000, 0.000000)
- C10 S9g: error mm=12.72428
- C12 S9g: observado=FrontWall, punto=(3.200000, 1.754276, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.767000, 0.000000)
- C10 S9h: error mm=12.712121
- C12 S9h: observado=FrontWall, punto=(3.200000, 1.757288, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.770000, 0.000000)
- C10 S9i: error mm=12.7000809
- C11 S9i: superficie=FrontWall; UV=(3.200000, 1.760300); observado=False, esperado=True
- C12 S9i: observado=FrontWall, punto=(3.200000, 1.760300, 0.000000), paso=41; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.773000, 0.000000)
- C1 S8-0: Floor step=7: center beyond plane; Floor step=200: center beyond plane
- C1 S8-1: BackWall step=35: center beyond plane; Floor step=104: center beyond plane; FrontWall step=194: center beyond plane
- C1 S8-2: BackWall step=6: center beyond plane; BackWall step=7: center beyond plane; RightWall step=14: center beyond plane; RightWall step=15: center beyond plane
- C7 S8-4 C7 k30: checkpoint no observable antes del fin/salida superior
- C3 S8-12 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C7 S8-18 C7 postcontacto: checkpoint no observable antes del fin/salida superior
- C7 S8-25 C7 k30: checkpoint no observable antes del fin/salida superior
- C7 S8-25 C7 postcontacto: checkpoint no observable antes del fin/salida superior
- C7 S8-26 C7 k30: checkpoint no observable antes del fin/salida superior
- C7 S8-40 C7 postcontacto: checkpoint no observable antes del fin/salida superior
- C7 S8-46 C7 k30: checkpoint no observable antes del fin/salida superior
- C7 S8-46 C7 postcontacto: checkpoint no observable antes del fin/salida superior
- C7 S8-48 C7 postcontacto: checkpoint no observable antes del fin/salida superior
- C7 S8-49 C7 k30: checkpoint no observable antes del fin/salida superior
- C7 S8-49 C7 postcontacto: checkpoint no observable antes del fin/salida superior
- C7 S8-51 C7 k30: checkpoint no observable antes del fin/salida superior
- C3 S8-52 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C6 S8-62: primera diferencia índice=13
- C3 S8-89 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C3 S8-122 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C3 S8-135 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C3 S8-139 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C3 S8-183 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C3 S8-196 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C3 S8-199 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C3 S8-202 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C6 S8-202: primera diferencia índice=11
- C3 S8-218 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C3 S8-224 NotEvaluable, ganancia=NaN, paso=-1, rapidez antes=NaN m/s, después=NaN m/s
- C6 S8-243: primera diferencia índice=5
- C6 S8-251: primera diferencia índice=39
- C6 S8-317: primera diferencia índice=60
- C6 S8-367: primera diferencia índice=27
- C6 S8-487: primera diferencia índice=17
- C6 S8-598: primera diferencia índice=11
- C6 S8-649: primera diferencia índice=23
- C6 S8-654: primera diferencia índice=63
- C13 ratios medidos ordenados (float32, R): 1.16505456, 1.2733072, 1.54982209
