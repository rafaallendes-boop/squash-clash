# Simulación headless de rallies — fondo-medium-vs-bot30

Bot σ=30 ms (paralelo, altura media, potencia media) contra Fondo Medio.

- Rallies: 1000
- Defaults: `06fed4bd4ac9`, SimulationKey `166a96e319c7`, RallyParams `7d50ddaf5d4f`
- Tiempo de cómputo: 23.5 s

## Cómo terminan

| Fin | Rallies | % |
|---|---:|---:|
| Point | 1000 | 100.0 |
| LaunchFault | 0 | 0.0 |
| Stalled | 0 | 0.0 |
| StepCap | 0 | 0.0 |
| ShotCap | 0 | 0.0 |

## Causas del punto

| Causa | Detalle de la última recepción | Rallies | % |
|---|---|---:|---:|
| DoubleBounce | Air | 406 | 40.6 |
| DoubleBounce | Late | 330 | 33.0 |
| DoubleBounce | Unreachable | 215 | 21.5 |
| Out | — | 49 | 4.9 |

## Pelotas fuera, tin y no llega (quién pegó y dónde terminó)

| Causa | Autor | Superficie | Tras el bote | Rallies |
|---|---|---|---|---:|
| Out | Rival | BackWall | no | 49 |

## Ganador

| Ganador | Rallies | % |
|---|---:|---:|
| Human | 356 | 35.6 |
| Rival | 644 | 64.4 |

## Duración

| Medida | Golpes | Segundos reales |
|---|---:|---:|
| Media | 17.98 | 37.86 |
| Mediana | 13 | 27.47 |
| p90 | 39 | 80.00 |
| p99 | 87 | 174.15 |
| Máximo | 140 | 278.14 |

## Recepciones por jugador

| Jugador | Recepciones | Contacto | Perfecto | Al aire | Tarde | No llega | Sin golpe | Falla del sistema | Holgura media (s) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Human | 9584 | 8891 | 7290 | 320 | 289 | 84 | 0 | 0 | 0.364 |
| Rival | 9391 | 9084 | 6503 | 86 | 41 | 180 | 0 | 0 | 0.490 |

## Golpes elegidos por la IA (contactos)

| Línea | Altura | Potencia | Golpes | % |
|---|---|---|---:|---:|
| Cross | High | Medium | 1397 | 15.4 |
| Cross | Mid | Medium | 1182 | 13.0 |
| Cross | Mid | Strong | 1149 | 12.6 |
| Straight | High | Medium | 1025 | 11.3 |
| Straight | Mid | Medium | 919 | 10.1 |
| Straight | Mid | Strong | 851 | 9.4 |
| Cross | High | Strong | 504 | 5.5 |
| Straight | High | Strong | 386 | 4.2 |
| Cross | Low | Medium | 241 | 2.7 |
| Cross | Low | Strong | 223 | 2.5 |
| Straight | Low | Medium | 216 | 2.4 |
| Straight | Low | Strong | 210 | 2.3 |
| Drop | Mid | Medium | 162 | 1.8 |
| Drop | High | Medium | 162 | 1.8 |
| Drop | High | Strong | 138 | 1.5 |
| Drop | Mid | Strong | 137 | 1.5 |
| Drop | Low | Medium | 94 | 1.0 |
| Drop | Low | Strong | 88 | 1.0 |

Fallas del sistema: 0. Contactos rechazados por turno: 0. Eventos tras el cierre: 0.
