# Simulación headless de rallies — fondo-medium-vs-bot30

Bot σ=30 ms (paralelo, altura media, potencia media) contra Fondo Medio.

- Rallies: 1000
- Defaults: `c0d8ec7050e0`, SimulationKey `166a96e319c7`, RallyParams `23ddf7292b9e`
- Tiempo de cómputo: 15.9 s

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
| DoubleBounce | Air | 615 | 61.5 |
| DoubleBounce | Late | 316 | 31.6 |
| Out | — | 36 | 3.6 |
| DoubleBounce | Unreachable | 33 | 3.3 |

## Pelotas fuera, tin y no llega (quién pegó y dónde terminó)

| Causa | Autor | Superficie | Tras el bote | Rallies |
|---|---|---|---|---:|
| Out | Rival | BackWall | no | 36 |

## Ganador

| Ganador | Rallies | % |
|---|---:|---:|
| Human | 311 | 31.1 |
| Rival | 689 | 68.9 |

## Duración

| Medida | Golpes | Segundos reales |
|---|---:|---:|
| Media | 19.27 | 40.27 |
| Mediana | 13 | 28.21 |
| p90 | 44 | 90.49 |
| p99 | 84 | 171.82 |
| Máximo | 137 | 272.60 |

## Recepciones por jugador

| Jugador | Recepciones | Contacto | Perfecto | Al aire | Tarde | No llega | Sin golpe | Falla del sistema | Holgura media (s) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Human | 10245 | 9520 | 7785 | 388 | 274 | 63 | 0 | 0 | 0.365 |
| Rival | 10020 | 9745 | 6988 | 227 | 42 | 6 | 0 | 0 | 0.469 |

## Golpes elegidos por la IA (contactos)

| Línea | Altura | Potencia | Golpes | % |
|---|---|---|---:|---:|
| Cross | High | Medium | 1484 | 15.2 |
| Cross | Mid | Strong | 1252 | 12.8 |
| Cross | Mid | Medium | 1186 | 12.2 |
| Straight | High | Medium | 1129 | 11.6 |
| Straight | Mid | Medium | 956 | 9.8 |
| Straight | Mid | Strong | 940 | 9.6 |
| Cross | High | Strong | 532 | 5.5 |
| Straight | High | Strong | 393 | 4.0 |
| Cross | Low | Medium | 272 | 2.8 |
| Cross | Low | Strong | 245 | 2.5 |
| Drop | Mid | Medium | 228 | 2.3 |
| Straight | Low | Medium | 219 | 2.2 |
| Drop | High | Medium | 217 | 2.2 |
| Straight | Low | Strong | 175 | 1.8 |
| Drop | High | Strong | 167 | 1.7 |
| Drop | Mid | Strong | 147 | 1.5 |
| Drop | Low | Medium | 116 | 1.2 |
| Drop | Low | Strong | 87 | 0.9 |

Fallas del sistema: 0. Contactos rechazados por turno: 0. Eventos tras el cierre: 1.
