# Evidencia de parada G1 A2 C1

Primera política fijada, sin reintentos. Diseño aprobado `2b686d8`, método/RED `009342a`, implementación `4fe66ad`. [Informe](../../../reviews/2026-11-17-g1-a2-c1-stop.md).

- `red-witnesses.tsv`: las 24 solicitudes C1 contra delegación al solver previo; 6 testigos, 18 fallos. Dos controles de identidad adicionales pasan.
- `green-witnesses.tsv`: mismas solicitudes tras implementar; 20 testigos, 4 fallos. **Candidato fallido**, el nombre de la fase no significa aprobación.
- `manifest.json`: recuentos, fuentes, SHA-256 de derivados y de todos los recibos originales preservados localmente. No se versionan logs, cachés ni builds.

XML/log/console e inventarios: `Logs/evidence/g1-a2/red/` y `Logs/evidence/g1-a2/green/`. Cada corrida usó un solo Unity y contó con inventario global vacío antes de lanzar. No se repite la geometría para obtener PASS; no se inició C8 ni el resto de las puertas A2 después de este fallo. No hay cambio a datos históricos de fase A/A1-bis/A1.
