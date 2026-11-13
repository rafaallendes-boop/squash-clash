# Simulación headless de rallies — fondo-medium-vs-bot30

Bot σ=30 ms (paralelo, altura media, potencia media) contra Fondo Medio.

- Rallies: 1000
- Defaults: `9041c4aafc77`, SimulationKey `166a96e319c7`, RallyParams `7d50ddaf5d4f`
- Tiempo de cómputo: 51.1 s

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
| DoubleBounce | Air | 593 | 59.3 |
| DoubleBounce | Late | 312 | 31.2 |
| Out | — | 59 | 5.9 |
| DoubleBounce | Unreachable | 36 | 3.6 |

## Pelotas fuera, tin y no llega (quién pegó y dónde terminó)

| Causa | Autor | Superficie | Tras el bote | Rallies |
|---|---|---|---|---:|
| Out | Rival | BackWall | no | 58 |
| Out | Rival | LeftWall | no | 1 |

## Ganador

| Ganador | Rallies | % |
|---|---:|---:|
| Human | 330 | 33.0 |
| Rival | 670 | 67.0 |

## Duración

| Medida | Golpes | Segundos reales |
|---|---:|---:|
| Media | 19.37 | 40.77 |
| Mediana | 13 | 27.71 |
| p90 | 45 | 91.69 |
| p99 | 87 | 175.51 |
| Máximo | 165 | 333.28 |

## Recepciones por jugador

| Jugador | Recepciones | Contacto | Perfecto | Al aire | Tarde | No llega | Sin golpe | Falla del sistema | Holgura media (s) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Human | 10298 | 9569 | 7825 | 367 | 270 | 92 | 0 | 0 | 0.372 |
| Rival | 10069 | 9798 | 6913 | 226 | 42 | 3 | 0 | 0 | 0.477 |

## Golpes elegidos por la IA (contactos)

| Línea | Altura | Potencia | Golpes | % |
|---|---|---|---:|---:|
| Cross | High | Medium | 1479 | 15.1 |
| Cross | Mid | Medium | 1290 | 13.2 |
| Cross | Mid | Strong | 1232 | 12.6 |
| Straight | High | Medium | 1156 | 11.8 |
| Straight | Mid | Medium | 993 | 10.1 |
| Straight | Mid | Strong | 903 | 9.2 |
| Cross | High | Strong | 523 | 5.3 |
| Straight | High | Strong | 364 | 3.7 |
| Cross | Low | Medium | 262 | 2.7 |
| Cross | Low | Strong | 236 | 2.4 |
| Drop | High | Medium | 209 | 2.1 |
| Straight | Low | Medium | 206 | 2.1 |
| Straight | Low | Strong | 200 | 2.0 |
| Drop | Mid | Medium | 194 | 2.0 |
| Drop | Mid | Strong | 167 | 1.7 |
| Drop | High | Strong | 150 | 1.5 |
| Drop | Low | Medium | 135 | 1.4 |
| Drop | Low | Strong | 99 | 1.0 |

Fallas del sistema: 0. Contactos rechazados por turno: 0. Eventos tras el cierre: 3.
