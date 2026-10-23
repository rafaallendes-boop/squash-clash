# Prueba de física

Estado: **COMPLETO**
Criterios de gate: **NO APROBADOS / NO EVALUADOS**
Este cálculo no elige backend ni autoriza el gate I4c.

Backend: P1-PhysX; variante: speculative/ContinuousSpeculative-co1mm-it6-1; build: editor/da98fd4
Dispositivo: editor; configuración: c86243b15a97
Perfil: editor; OS: Mac OS X 26.6.2; UTC: — → —
Térmico: not measured / editor → not measured / editor
Térmico al iniciar benchmark C17 (informativo): not measured / editor
Interrupciones de foco: 0; solo una ejecución completa permite evaluar sus criterios.
ProjectPhysics: EnhancedDeterminism=True; fastMotionThreshold=0.01; bounceThreshold=0.1; Unity=6000.3.27f1

| ID | Requisito | Escenarios | Medición | Umbral | Veredicto | Gate | evaluados/no evaluables/esperados | No aplica (fuera de población) | Cobertura |
|---|---|---|---|---|---|---|---|---|---|
| C1 | Robustez (§8.2.2, §17.A.1): Crossings | S1–S10; S8 (10 000); S8d | fallos=0; evaluados=10053; no evaluables=0; atravesamientos=0; salidas superiores=2105; seed=20261020; S8=10000; S8d=36 | 0 atravesamientos | Pass | sí | 10053/0/10053 | 0 | cerrada |
| C2 | Estados finitos: AllFinite | Todos | fallos=0; evaluados=67789; no evaluables=0 | Todos finitos | Pass | sí | 67789/0/67789 | 0 | cerrada |
| C3 | Esquinas sin ganancia (§8.2.2): MaxRelativeEnergyGain | S4, S5, S6, S8, S8d; pasos de esquina | fallos=17; evaluados=2772; no evaluables=0; no aplica (sin pasos de esquina, fuera de población)=7267; Corner [Measured=2772, NotApplicable=7267, NotEvaluable=0]: máximo entre casos medidos: S8-8469 Measured, ganancia=0.0273839571, paso=322, rapidez antes=0.06398784 m/s, después=0.06558204 m/s; CornerWithImpact [Measured=2772, NotApplicable=7267, NotEvaluable=0]: máximo entre casos medidos: S8-8469 Measured, ganancia=0.0273839571, paso=322, rapidez antes=0.06398784 m/s, después=0.06558204 m/s; CornerPersistentOnly [Measured=2689, NotApplicable=7350, NotEvaluable=0]: máximo entre casos medidos: S8-9384 Measured, ganancia=0.0273839273, paso=344, rapidez antes=0.06382191 m/s, después=0.06550735 m/s | ≤ 1e-5 (ruido float) | Fail | sí | 2772/0/2772 | 7267 | cerrada |
| C4 | Sin vibración (§8.2.2): RepeatedImpacts | S4, S5, S6, S10 | fallos=0; evaluados=4; no evaluables=0; impactos repetidos=0 | 0 (\|vn\| > 0,5 m/s; no persistentes) | Pass | sí | 4/0/4 | 0 | cerrada |
| C5 | Consistencia (§17.A.2): igualdad bit a bit | S1–S7, S9, S10 nuevas; 1/100 S8 reutilizada/nueva | fallos=0; evaluados=117; no evaluables=0 | Igualdad de estados y contactos | Pass | sí | 117/0/117 | 0 | cerrada |
| C6 | Predicción desde lanzamiento (§8.2.1, §17.A.3) | S1–S10 y S8 completo; Reset(s) | fallos=1; evaluados=10017; no evaluables=0 | Igualdad bit a bit | Fail | sí | 10017/0/10017 | 0 | cerrada |
| C7 | Relanzamiento en vuelo (§17.A.3) | S1–S7, S9, S8 completo; k=30 y siguiente al primer contacto; Reset de ambos | fallos=0; evaluados=18779; no evaluables=0; Measured=18779; NotApplicable=1253; NotEvaluable=0 (no aplica fuera de población) | Igualdad bit a bit | Pass | sí | 18779/0/18779 | 1253 | cerrada |
| C8 | Reutilización (§8.2.1 cond.1) | S10→S10b (S1 reutilizada), S6→S1 frente a nueva | fallos=0; evaluados=2; no evaluables=0 | Igualdad bit a bit | Pass | sí | 2/0/2 | 0 | cerrada |
| C9 | Continuación sin reiniciar el juego (D-3) | S1, S6; k=30 | fallos=0; evaluados=2; no evaluables=0 | Diagnóstico; no decide el gate | Info | no | 2/0/2 | 0 | cerrada |
| C10 | Precisión de contacto S9: ContactPrecisionMm | S9a–i | fallos=9; evaluados=9; no evaluables=0; error máximo mm=77.24857 | ≤ 1 mm [EXPERIMENTO, D-1] | Fail | sí | 9/0/9 | 0 | cerrada |
| C11 | Clasificación de líneas: punto observado | S9 fuera/dentro; borde exacto solo precisión | fallos=3; evaluados=6; no evaluables=0 | Igual a ExpectTouch | Fail | sí | 6/0/6 | 0 | cerrada |
| C12 | Primer impacto (§8.3.1) | S5, S6, S9 | fallos=0; evaluados=11; no evaluables=0 | Informe al gate; sin umbral inventado | Info | no | 11/0/11 | 0 | cerrada |
| C13 | Tiempo restante (§8.3.2): mediana y mínimo | Impactos únicos S1, S2, S7, S9 | fallos=0; evaluados=15; no evaluables=16; mínimo=1.32343864; mediana=8.159093 | Informe al gate; ratio sin umbral | NotEvaluated | no | 15/16/31 | 0 | cerrada |
| C14 | Impactos por paso (§8.3.2) | Todos | máximo impactos no persistentes/paso=2; tope configurable=False; parámetro maxImpactsPerStep no operativo con P1 | Informe al gate; tope configurable sí/no (P1: no) | Info | no | 1/0/1 | 0 | cerrada |
| C15 | Simultáneos e instante (§8.3.3, §8.3.7) | S4, S6 | fallos=0; evaluados=2; no evaluables=0 | Mismo paso y ContactTiming; informe al gate | Info | no | 2/0/2 | 0 | cerrada |
| C16 | Penetración y muestras persistentes | Todos | penetración máxima=0.612672448 mm; muestras persistentes=1774348 (todas las trayectorias ejecutadas) | Informativo | Info | no | 1/0/1 | 0 | cerrada |
| C17 | Rendimiento (§8.2.2, §17.C, D-2) | S1 hasta segundo bote; Reset incluido; 20 warmup y 200 mediciones | n=200; warmup=20; median=26.528100; p95=27.595400; p99=28.095200; max=28.691200 ms; percentiles nearest rank | p95 ≤ 1 ms en iPhone; editor informativo | Info | no | 200/0/200 | 0 | cerrada |
| C18 | Facilidad de prueba (§8.2.2) | Capabilities.RunsInEditMode | RunsInEditMode=False; timing=Estimated | Preferencia, no gate | Info | no | 1/0/1 | 0 | cerrada |

## Evidencia por caso (hasta 12 notas generales por criterio; clasificación de todas las divergencias C5–C7 y máximo C16 sin ese límite)

- C13 S1 paso=112 timing=StepOnly
- C13 S1 paso=185 timing=StepOnly
- C13 S2 paso=127 timing=StepOnly
- C15 S4: primer paso=20; mismo paso=False; Floor, paso=20, StepOnly, t=NaN; RightWall, paso=21, StepOnly, t=NaN
- C12 S5: observado=RightWall, punto=(0.000000, 1.505522, 0.186758), paso=28; ideal=FrontWall, t=0.2383370663362244, punto=(0.025000, 1.500000, 0.000000)
- C12 S6: observado=LeftWall, punto=(6.400000, 1.003953, 9.487833), paso=23; ideal=LeftWall, t=0.19606242690409423, punto=(6.400000, 1.000000, 9.730000)
- C15 S6: primer paso=23; mismo paso=False; LeftWall, paso=23, StepOnly, t=NaN; BackWall, paso=24, Estimated, t=0.004395852
- C13 S7 paso=104 timing=StepOnly
- C13 S7 paso=165 timing=StepOnly
- C10 S9a: error mm=77.1217346
- C12 S9a: observado=FrontWall, punto=(3.200000, 4.479878, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.557000, 0.000000)
- C10 S9b: error mm=77.18468
- C12 S9b: observado=FrontWall, punto=(3.200000, 4.482815, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.560000, 0.000000)
- C10 S9c: error mm=77.24857
- C11 S9c: superficie=FrontWall; UV=(3.200000, 4.485751); observado=False, esperado=True
- C12 S9c: observado=FrontWall, punto=(3.200000, 4.485751, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 4.563000, 0.000000)
- C10 S9d: error mm=8.564293
- C12 S9d: observado=FrontWall, punto=(3.200000, 0.501564, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.493000, 0.000000)
- C13 S9d paso=54 timing=StepOnly
- C13 S9d paso=102 timing=StepOnly
- C10 S9e: error mm=8.627444
- C12 S9e: observado=FrontWall, punto=(3.200000, 0.498627, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.490000, 0.000000)
- C13 S9e paso=54 timing=StepOnly
- C13 S9e paso=102 timing=StepOnly
- C10 S9f: error mm=8.690596
- C11 S9f: superficie=FrontWall; UV=(3.200000, 0.495691); observado=False, esperado=True
- C12 S9f: observado=FrontWall, punto=(3.200000, 0.495691, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 0.487000, 0.000000)
- C13 S9f paso=53 timing=StepOnly
- C13 S9f paso=98 timing=StepOnly
- C10 S9g: error mm=18.2968369
- C12 S9g: observado=FrontWall, punto=(3.200000, 1.748703, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.767000, 0.000000)
- C13 S9g paso=119 timing=StepOnly
- C10 S9h: error mm=18.3599
- C12 S9h: observado=FrontWall, punto=(3.200000, 1.751640, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.770000, 0.000000)
- C10 S9i: error mm=18.4231987
- C11 S9i: superficie=FrontWall; UV=(3.200000, 1.754577); observado=False, esperado=True
- C12 S9i: observado=FrontWall, punto=(3.200000, 1.754577, 0.000000), paso=40; ideal=FrontWall, t=0.33200000002980234, punto=(3.200000, 1.773000, 0.000000)
- C7 S8-4 C7 k30: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C7 S8-18 C7 postcontacto: NotApplicable; checkpoint fuera de trayectoria limpia por salida superior
- C3 S8-22 Measured, ganancia=0.02738387, paso=237, rapidez antes=0.06412517 m/s, después=0.06564382 m/s
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
- C3 S8-1009 Measured, ganancia=0.0273837652, paso=335, rapidez antes=0.06366151 m/s, después=0.065435186 m/s
- C3 S8-1352 Measured, ganancia=0.0273570064, paso=346, rapidez antes=0.0603772365 m/s, después=0.06395726 m/s
- C3 S8-1508 Measured, ganancia=0.0168146, paso=275, rapidez antes=0.114391387 m/s, después=0.08151313 m/s
- C3 S8-2452 Measured, ganancia=0.02738362, paso=321, rapidez antes=0.06443971 m/s, después=0.06578538 m/s
- C3 S8-3045 Measured, ganancia=0.0273813829, paso=351, rapidez antes=0.0628606 m/s, después=0.06507477 m/s
- C3 S8-4048 Measured, ganancia=0.0273828115, paso=356, rapidez antes=0.06323272 m/s, después=0.06524223 m/s
- C3 S8-4194 Measured, ganancia=0.0273838285, paso=350, rapidez antes=0.06370747 m/s, después=0.0654558539 m/s
- C3 S8-5267 Measured, ganancia=0.0273838174, paso=357, rapidez antes=0.06382197 m/s, después=0.06550744 m/s
- C3 S8-5580 Measured, ganancia=0.0273835734, paso=348, rapidez antes=0.0635587 m/s, después=0.0653889 m/s
- C3 S8-6591 Measured, ganancia=0.0273839235, paso=271, rapidez antes=0.06410801 m/s, después=0.0656361 m/s
- C6 S8-6719: primera diferencia índice=156
- C6 valores distintos: S8-6719; prefijoEstadosBitsIgual=True; primerEstadoNuméricoDiferente=-1; estados=226/156; previo A=paso=155, p=(5.42853975, 0.07237575, 4.62354326), v=(-2.587671, 2.80801582, 0.627886), bits=40ADB699,3D9439BD,4093F411,C0259C67,4033B688,3F20BD23; previo B=paso=155, p=(5.42853975, 0.07237575, 4.62354326), v=(-2.587671, 2.80801582, 0.627886), bits=40ADB699,3D9439BD,4093F411,C0259C67,4033B688,3F20BD23; A=paso=156, p=(5.40697575, 0.0950946361, 4.6287756), v=(-2.587671, 2.726266, 0.627886), bits=40AD05F2,3DC2C0FA,40941EEE,C0259C67,402E7B24,3F20BD23; B=ausente; secuenciaContactosCompletosBits A=[1/Floor/True/StepOnly/FFC00000,40568E11,00000000,403F6E5C,40568E11,403F6E5C,00000000,3F800000,00000000,41FB440C,C158056E,3FA24C46; 2/Floor/True/StepOnly/FFC00000,40674E56,00000000,40401B7A,40674E56,40401B7A,00000000,3F800000,00000000,41FB440C,C1595447,3FA24C46; 3/Floor/True/StepOnly/FFC00000,40780E9B,00000000,4040C898,40780E9B,4040C898,00000000,3F800000,00000000,41FB440C,C15AA320,3FA24C46; 4/Floor/True/StepOnly/FFC00000,40846770,00000000,404175B6,40846770,404175B6,00000000,3F800000,00000000,41FB440C,40C5F35E,3FA24C46; 5/Floor/True/StepOnly/FFC00000,408CC793,00000000,404222D4,408CC793,404222D4,00000000,3F800000,00000000,41FB440C,40C355AC,3FA24C46; 6/Floor/True/StepOnly/FFC00000,409527B6,00000000,4042CFF2,409527B6,4042CFF2,00000000,3F7FFFFF,00000000,41FB440C,40C0B7FA,3FA24C46; 7/Floor/True/StepOnly/FFC00000,409D87D9,00000000,40437D10,409D87D9,40437D10,00000000,3F7FFFFF,00000000,41FB440C,40BE1A48,3FA24C46; 8/Floor/True/StepOnly/FFC00000,40A5E7FC,00000000,40442A2E,40A5E7FC,40442A2E,00000000,3F7FFFFF,00000000,41FB440C,40BB7C96,3FA24C46; 12/LeftWall/False/Estimated/3B9AEB29,40CCCCCD,3EF11610,4046DEA6,4046DEA6,3EF11610,BF7FFFFF,00000000,00000000,41FB440C,40B105CE,3FA24C46; 55/RightWall/False/Estimated/3C050943,00000000,3FE78350,4063F2B0,4063F2B0,3FE78350,3F7FFFFF,00000000,00000000,C18A3238,400111D0,3FA24C46; 134/LeftWall/False/Estimated/3C050226,40CCCCCD,3F7BCE14,408CAF79,408CAF79,3F7BCE14,BF800001,00000000,00000000,41180408,C08E2112,3FA24C46; 135/LeftWall/True/StepOnly/FFC00000,40CCCCCD,3F7227C4,408D0608,408D0608,3F7227C4,BF800000,00000000,00000000,C0A737A4,C090BEC4,3FA24C46; 155/Floor/False/Estimated/3B999ACB,40AE6740,00000000,4093C934,40AE6740,4093C934,00000000,3F800000,00000000,C0A737A4,C0C510AC,3FA24C46; 225/Floor/False/Estimated/3BF88481,407C332C,00000000,409F81A2,407C332C,409F81A2,00000000,3F7FFFFF,00000000,C0259C67,C0354B83,3F20BD23]; B=[1/Floor/False/StepOnly/FFC00000,40568E11,00000000,403F6E5C,40568E11,403F6E5C,00000000,3F800000,00000000,41FB440C,C158056E,3FA24C46; 2/Floor/True/StepOnly/FFC00000,40674E56,00000000,40401B7A,40674E56,40401B7A,00000000,3F800000,00000000,41FB440C,C1595447,3FA24C46; 3/Floor/True/StepOnly/FFC00000,40780E9B,00000000,4040C898,40780E9B,4040C898,00000000,3F800000,00000000,41FB440C,C15AA320,3FA24C46; 4/Floor/True/StepOnly/FFC00000,40846770,00000000,404175B6,40846770,404175B6,00000000,3F800000,00000000,41FB440C,40C5F35E,3FA24C46; 5/Floor/True/StepOnly/FFC00000,408CC793,00000000,404222D4,408CC793,404222D4,00000000,3F800000,00000000,41FB440C,40C355AC,3FA24C46; 6/Floor/True/StepOnly/FFC00000,409527B6,00000000,4042CFF2,409527B6,4042CFF2,00000000,3F7FFFFF,00000000,41FB440C,40C0B7FA,3FA24C46; 7/Floor/True/StepOnly/FFC00000,409D87D9,00000000,40437D10,409D87D9,40437D10,00000000,3F7FFFFF,00000000,41FB440C,40BE1A48,3FA24C46; 8/Floor/True/StepOnly/FFC00000,40A5E7FC,00000000,40442A2E,40A5E7FC,40442A2E,00000000,3F7FFFFF,00000000,41FB440C,40BB7C96,3FA24C46; 12/LeftWall/False/Estimated/3B9AEB29,40CCCCCD,3EF11610,4046DEA6,4046DEA6,3EF11610,BF7FFFFF,00000000,00000000,41FB440C,40B105CE,3FA24C46; 55/RightWall/False/Estimated/3C050943,00000000,3FE78350,4063F2B0,4063F2B0,3FE78350,3F7FFFFF,00000000,00000000,C18A3238,400111D0,3FA24C46; 134/LeftWall/False/Estimated/3C050226,40CCCCCD,3F7BCE14,408CAF79,408CAF79,3F7BCE14,BF800001,00000000,00000000,41180408,C08E2112,3FA24C46; 135/LeftWall/True/StepOnly/FFC00000,40CCCCCD,3F7227C4,408D0608,408D0608,3F7227C4,BF800000,00000000,00000000,C0A737A4,C090BEC4,3FA24C46; 155/Floor/False/Estimated/3B999ACB,40AE6740,00000000,4093C934,40AE6740,4093C934,00000000,3F800000,00000000,C0A737A4,C0C510AC,3FA24C46]
- C6 clasificación: S8-6719; tipo=estado/contactos distintos; estadosBitsIguales=False; multiconjuntoContactosCompletosIgual=False; primerPaso=156; índice=156; checkpoint=0; estados=226/156; contactos=14/13
- C6 diagnóstico: S8-6719; primer paso divergente=156; índice=156; previo A=paso=155, p=(5.42853975, 0.07237575, 4.62354326), v=(-2.587671, 2.80801582, 0.627886), bits=40ADB699,3D9439BD,4093F411,C0259C67,4033B688,3F20BD23; previo B=paso=155, p=(5.42853975, 0.07237575, 4.62354326), v=(-2.587671, 2.80801582, 0.627886), bits=40ADB699,3D9439BD,4093F411,C0259C67,4033B688,3F20BD23; A=paso=156, p=(5.40697575, 0.0950946361, 4.6287756), v=(-2.587671, 2.726266, 0.627886), bits=40AD05F2,3DC2C0FA,40941EEE,C0259C67,402E7B24,3F20BD23; B=ausente; contactos A=[]; contactos B=[]
- C3 S8-7989 Measured, ganancia=0.0273838323, paso=337, rapidez antes=0.0636903048 m/s, después=0.0654481351 m/s
- C13 ratios medidos ordenados (float32, R): 1.32343864, 2.03264618, 2.9091382, 8.159093, 8.159093, 8.159093, 8.159093, 8.159093, 8.159093, 8.159093, 8.159093, 8.159093, 9.831801, 15.6146765, 35.7052078
- C16 máximo: S8-3821 robustez; paso=193; superficie=Floor; tipo=solo persistentes (reposo/contacto sostenido); penetración=0.612672448 mm; rapidez=5.8087306 m/s; previo=paso=192, p=(0.033009734, 0.02016184, 5.663484), v=(-2.65445018, -0.07766926, 6.766849), bits=3D073537,3CA52A71,40B53B43,C029E283,BD9F1110,40D88A07; estado=paso=193, p=(0.0451759659, 0.0193873271, 5.71094227), v=(1.45994782, -0.09155713, 5.621523), bits=3D390A6F,3C9ED22C,40B6C00A,3FBADF92,BDBB824E,40B3E384; contactos=[Floor@193 persistent=True, StepOnly, t=NaN, p=(0.0330097675, 0, 5.663484), n=(0, 0.99999994, 0), approach=(-2.65445018, -0.07766926, 6.766849); RightWall@193 persistent=True, StepOnly, t=NaN, p=(0, 0.0201618671, 5.663484), n=(1, 0, 0), approach=(-2.65445018, -0.07766926, 6.766849)]; siguiente=paso=194, p=(0.05718428, 0.0200372282, 5.75718), v=(1.44099748, 0.07798819, 5.548555), bits=3D6A3A10,3CA4251D,40B83AD2,3FB8729B,3D9FB846,40B18DC3; contactos siguientes=[Floor@194 persistent=True, StepOnly, t=NaN, p=(0.04517603, 0, 5.71094227), n=(0, 1, 0), approach=(1.45994782, -0.09155713, 5.621523); RightWall@194 persistent=True, StepOnly, t=NaN, p=(0, 0.0193872452, 5.71094227), n=(1, 0, 0), approach=(1.45994782, -0.09155713, 5.621523)]
