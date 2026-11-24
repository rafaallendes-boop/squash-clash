# Recepción del saque B acotada · PR #106

Base `90ddc7e`. Medición terminada en la rama desechable `exp/serve-reception-margin` (`b4cccbd`, no se fusiona). **Elegido: 25 ms** (decisión del PO del 24 nov, abajo). Solo se aplica al golpe temprano del humano al recibir el saque del rival, incluida la volea elegida, que sigue siendo opcional en los tres niveles. Los anillos y «Espera el bote» van en otra PR con Claude.

## Decisión del 24 nov: 25 ms

Texto literal del PO (Rafael, 24 nov 09:38, chat de SC-PM): «Vamos con 25 ms como me propones». La propuesta consistía en aplicar el [protocolo fijado antes de medir](../validation/2026-11-23-recepcion-b-protocolo.md) (`b4cccbd`). Ese protocolo compara N2 con la base actual medida y elige el menor valor que mejore claramente la devolución del novato que toca sin romper N2.

- **25 ms cumple N2 según el protocolo:** como máximo se desplaza 1,40 pp (Definidor, Medio, novato), sin ninguna celda por encima de 5 pp ([comparación con la base](../validation/evidence/2026-11-23-recepcion-b-acotada/n2-vs-current-base.tsv)).
- **Mejora clara del novato que toca:** +3,92/+3,55/+2,13 pp en Fácil/Medio/Difícil, con intervalos pareados al 95 % positivos en los tres niveles ([detalle](../validation/evidence/2026-11-23-recepcion-b-acotada/touch-return-bootstrap.tsv)).
- Frente a la referencia de la spec del saque, `confirm01`, 25 ms queda fuera solo en Definidor Fácil (−5,54 pp). Esa celda mejora respecto de la base (−5,93 pp), y la base ya excede en tres celdas ([comparación con `confirm01`](../validation/evidence/2026-11-23-recepcion-b-acotada/n2-vs-confirm01.tsv)). Es la desviación histórica del saque nuevo, aceptada el 22 nov; 25 ms la reduce en vez de crearla. Se deja visible y no se declara un Pass nuevo.
- La primera versión de este informe había elegido 50 ms con la lectura de `confirm01`. Esa lectura queda superada por la decisión del PO. Se usan los mismos datos, sin volver a medir y sin cambiar umbrales, escenarios ni criterios. No se recalibra: Fondista Hard conserva k=2,5.

**M1 oficial con 25 ms (std, Medio): 93,80 % (3615/3854), fuera de la banda 80–90.** La aceptación del PO del 24 nov 07:20 («Sí, acepto que M1 quede fuera de banda solo en la build de prueba con 50 ms») se refiere a 50 ms. **Con 25 ms, M1 fuera de banda no está aceptado:** queda como desviación abierta y pendiente del PO. No cambia el criterio y no se acepta ninguna etapa.

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

Mejora de toca con 25 ms (elegido): +3,92/+3,55/+2,13 pp; con 50 ms, +6,87/+5,41/+4,77 pp. Los intervalos pareados al 95 % son positivos en los tres niveles con ambos valores ([detalle](../validation/evidence/2026-11-23-recepcion-b-acotada/touch-return-bootstrap.tsv)). La volea por saque sube en los tres. Puntos y partidos completos están en la tabla de evidencia. **M1 oficial en Medio: 89,70 % → 93,80 % con 25 ms (96,83 % con 50 ms). Queda fuera de 80–90: es una desviación abierta y no aceptada con 25 ms; el criterio no cambia y no se acepta ninguna etapa.**

**N2:** `ConfirmPerStyle` completo, 200 partidos por celda y modelo, semillas originales; 1/1 en cada variante. [Comparación oficial por estilo/nivel, con n](../validation/evidence/2026-11-23-recepcion-b-acotada/n2-vs-confirm01.tsv), [comparación incremental](../validation/evidence/2026-11-23-recepcion-b-acotada/n2-vs-current-base.tsv). La base actual también conserva una desviación histórica (máximo 5,93 pp); no se declara como nuevo Pass.

## Implementación y validación

El borde inclusivo con latencia se corrigió sin editar el test original: compensación una vez, comparación en el reloj del evento y sin epsilon. TDD del contrato: RED 11 fallos por aserción y GREEN 43/43. La integración se hizo primero con 50 ms; con la decisión del 24 nov, el valor por defecto pasa a 25 ms (TDD: RED por aserción, se esperaba 25 y salió 50; luego GREEN). Arquitectura y borde revisados en solo lectura por Claude Opus. El supervisor de medición no dejó DONE; se recuperaron resultados con los dos recibos exec exit0, sin repetir simulaciones.

RallyParams con 25 ms, medido en Unity: `1063d0f6cf66` → `ebf8f717f8c5`; con «Saque nuevo» apagado, `32683515d986` → `5fc9f2ff92e5`; proyección sin `HumanServeEarlyMs` `1063d0f6cf66` (ningún otro parámetro cambia). Con 50 ms habían sido `97f5b3d7627f`/`63fd5e6734e5`; esos valores ya no rigen. Solo cambian esos dos literales de la prueba de hash y la aserción del valor por defecto (50→25); no cambia ningún umbral, tolerancia, oráculo ni criterio. Defaults `207da8f2c636` y SimulationKey `166a96e319c7` sin cambios. Textos: ninguno nuevo/modificado en producto. Sin fusión ni instalación.

## Historial: parada r3, fixture inválido confirmado

PlayMode de integración queda **2/3**, no GREEN. Captura diagnóstica aislada, invocando el fixture original sin modificarlo: el saque Drive alcanza0,2988938987m después del primer bote, por debajo del mínimo artificial0,3000000119m del fixture. `Plan.Exists=false`, `Reason=unreachable`, `KIn.Step=0`, `TStarStep=0`. El fixture no comprueba la existencia del plan y convierte ese K-in por defecto a la hora del lanzamiento; al soltar después del bote ya está después de esa fecha. El pedido de golpe no llega a enviarse: la tolerancia no interviene en esta falla. Planificador, ClockMap y host no cambian frente a90ddc7e.

[Captura acotada](../validation/evidence/2026-11-23-recepcion-b-acotada/fixture-diagnosis.tsv); originales ignorados en `Logs/evidence/serve-reception-margin/diagnosis-r3.json` y fuente del diagnóstico preservada allí. El éxito de captura no es un Pass de regresión. Se aplica la instrucción de SC-PM: si la prueba está mal, no cambiarla y detenerse. No se quita el mínimo artificial, no se relajan aserciones, no se modifica la spec y no se lanza r4 ni se declaran verdes las puertas. Se preserva la implementación50ms y sus pruebas para revisión. `device.env` retirado; sin fusión ni instalación.

## Corrección del fixture (`88e77e3`)

Con aprobación del PO (23 nov 22:14: «Si, pero has todo con Claude.»), cambió una sola línea del armado de `RivalServe_AfterFirstBounce_KeepsAllowanceUntilTheOrder_ThenResetsOnTheNextFlight`: `legal.minHeight` del preset del fixture pasa de 0,30 a 0,25, porque el pico tras el bote es 0,2989 m. Con eso el plan existe y K-in queda 124,4 ms después del bote. El valor de producto sigue en 0,05, y las aserciones no cambian. [Barrido](../validation/evidence/2026-11-23-recepcion-b-acotada/fixture-height-probe.tsv). La revisión de Claude de ese commit no es independiente: la revisión cruzada le corresponde a Codex.

## Puertas finales

Sobre `4e5ef77` (25 ms; Mac del PO, lock de Unity, árbol limpio antes y después). El commit siguiente solo cambia este informe.

- EditMode **1027/1027**. PlayMode **817/817**, con las tres pruebas de `ZServeReceptionMarginTests` aprobadas. Las dos pruebas de `Screen.dpi` también pasaron, pero no se dan por resueltas: siguen pendientes de `fix/hermetic-dpi-tests`.
- AutoQA V, T3 y volea: **PASS, nivel 1**, 102/102 cada una, 18 PASS, 0 FAIL/INCONCLUSIVE/NOT_EVALUATED, canario FAIL esperado, determinismo idéntico y `dirty=false`.
- check-release **OK** (build 1160, dev y prod).
- `device.env` se copió sin cambios desde `sc-install-518f9c3` (SHA-256 idéntico, ignorado por git) y se borró al terminar.

## Nota para el PO (observación 3 de la revisión)

Al fusionar, 25 ms queda como valor por defecto de `main` y de toda build futura, no solo de la build de prueba. Antes de una build que no sea de prueba (TestFlight o pública) conviene decidir explícitamente M1 con 25 ms y dejarlo en AGENTS §7. Esta PR no edita §7: la decisión es del PO.
