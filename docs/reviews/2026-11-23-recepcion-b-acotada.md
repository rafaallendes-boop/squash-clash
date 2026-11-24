# Recepción del saque B acotada · PR #106

Base `90ddc7e`. Medición terminada, rama desechable `exp/serve-reception-margin` (`b4cccbd`, no se fusiona). **Elegido: 50 ms**, solo golpe temprano del humano al recibir el saque del rival, incluida la volea elegida, opcional en los tres niveles. Los anillos y «Espera el bote» pertenecen a otra PR con Claude.

**Regla de elección:** 25 mejora claramente el novato que toca, pero no cumple N2 frente a `confirm01` (máximo 5,54 pp). 50 es el menor valor medido que mejora esa devolución y cumple N2 (máximo 3,62 pp ≤5). La comparación incremental contra la base actual da 1,40/3,38 pp: es descriptiva y no sustituye la referencia oficial. El protocolo experimental había llamado N2 a esa comparación incremental; se corrige esa interpretación, conservando datos, escenarios, referencia oficial y umbral. No se recalibra: Fondista Hard conserva k=2,5 en las tres variantes.

## Medición

7020 partidos: std σ90 ms, 200 por estilo/nivel/variante (600 por nivel); toca, 60 (180 por nivel). Tres estilos, Fácil/Medio/Difícil, mismas semillas y bots de #105; std saca globo como M1 original y toca conserva su mezcla humana 50/50. Recepción = primer golpe aplicado al saque rival. Fuente: [tabla completa](../validation/evidence/2026-11-23-recepcion-b-acotada/reception-summary.tsv), [configuración](../validation/evidence/2026-11-23-recepcion-b-acotada/options-source.tsv).

| Margen ms | Nivel | Std: P+V (n saques) | Toca: P+V (n saques) | Volea/saque, toca |
|---|---|---|---|---|
| 0 | Fácil | 89.82 % (2789/3105) | 79.40 % (844/1063) | 41.11 % (437/1063) |
| 0 | Medio | 89.70 % (3456/3853) | 81.21 % (951/1171) | 37.92 % (444/1171) |
| 0 | Difícil | 90.59 % (3562/3932) | 80.15 % (941/1174) | 38.16 % (448/1174) |
| 25 | Fácil | 93.48 % (2896/3098) | 83.32 % (894/1073) | 44.27 % (475/1073) |
| 25 | Medio | 93.80 % (3615/3854) | 84.77 % (996/1175) | 40.17 % (472/1175) |
| 25 | Difícil | 94.67 % (3711/3920) | 82.28 % (966/1174) | 39.78 % (467/1174) |
| 50 | Fácil | 96.46 % (2943/3051) | 86.27 % (917/1063) | 45.16 % (480/1063) |
| 50 | Medio | 96.83 % (3728/3850) | 86.63 % (1017/1174) | 41.14 % (483/1174) |
| 50 | Difícil | 97.19 % (3808/3918) | 84.92 % (997/1174) | 41.31 % (485/1174) |

Con 50 ms, mejora de toca: +6,87/+5,41/+4,77 pp; intervalos pareados 95 % positivos en los tres niveles ([detalle](../validation/evidence/2026-11-23-recepcion-b-acotada/touch-return-bootstrap.tsv)). La volea por saque sube en los tres. Puntos y partidos completos están en la tabla de evidencia. **M1 oficial en Medio: 89,70 →96,83 %, fuera de 80–90; desviación abierta, criterio sin cambiar y ninguna etapa aceptada.**

**N2:** `ConfirmPerStyle` completo, 200 partidos por celda y modelo, semillas originales; 1/1 en cada variante. [Comparación oficial por estilo/nivel, con n](../validation/evidence/2026-11-23-recepcion-b-acotada/n2-vs-confirm01.tsv), [comparación incremental](../validation/evidence/2026-11-23-recepcion-b-acotada/n2-vs-current-base.tsv). La base actual también conserva una desviación histórica (máximo 5,93 pp); no se declara como nuevo Pass.

## Implementación y validación

El borde inclusivo con latencia se corrigió sin editar el test original: compensación una vez, comparación en el reloj del evento y sin epsilon. TDD del contrato: RED 11 fallos por aserción y GREEN 43/43. La integración de 50 ms y las puertas completas se ejecutan a continuación; resultados pendientes. Arquitectura y borde revisados en solo lectura por Claude Opus. El supervisor de medición no dejó DONE; se recuperaron resultados con los dos recibos exec exit0, sin repetir simulaciones.

RallyParams esperado: `1063d0f6cf66` → `97f5b3d7627f`; proyección sin HumanServeEarlyMs `1063d0f6cf66`. Defaults `207da8f2c636` y SimulationKey `166a96e319c7` sin cambios. Textos: ninguno nuevo/modificado en producto. Sin fusión ni instalación.

## Parada r3: fixture inválido confirmado

PlayMode de integración queda **2/3**, no GREEN. Captura diagnóstica aislada, invocando el fixture original sin modificarlo: el saque Drive alcanza0,2988938987m después del primer bote, por debajo del mínimo artificial0,3000000119m del fixture. `Plan.Exists=false`, `Reason=unreachable`, `KIn.Step=0`, `TStarStep=0`. El fixture no comprueba la existencia del plan y convierte ese K-in por defecto a la hora del lanzamiento; al soltar después del bote ya está después de esa fecha. El pedido de golpe no llega a enviarse: la tolerancia no interviene en esta falla. Planificador, ClockMap y host no cambian frente a90ddc7e.

[Captura acotada](../validation/evidence/2026-11-23-recepcion-b-acotada/fixture-diagnosis.tsv); originales ignorados en `Logs/evidence/serve-reception-margin/diagnosis-r3.json` y fuente del diagnóstico preservada allí. El éxito de captura no es un Pass de regresión. Se aplica la instrucción de SC-PM: si la prueba está mal, no cambiarla y detenerse. No se quita el mínimo artificial, no se relajan aserciones, no se modifica la spec y no se lanza r4 ni se declaran verdes las puertas. Se preserva la implementación50ms y sus pruebas para revisión. `device.env` retirado; sin fusión ni instalación.
