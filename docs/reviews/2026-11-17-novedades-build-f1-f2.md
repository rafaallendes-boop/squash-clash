# Novedades para probar: build DEV con G1 A1, F1 y F2 (17 nov 2026)

Esta build sale de `main` en `84c2926`, con F2 fusionada. La app instalada hoy es `0.1.0+2b82701 dev`. Esta página lista lo que cambió desde entonces y cómo probarlo. La implementación de F1-AUTOQA-01 y de F2 la hicieron subagentes Sonnet dirigidos por Claude, así que la revisión de Claude no es independiente (queda declarado en el [registro](2026-10-25-first-shot-po-approvals.md)).

## Cambios de juego

### 1. El Rival ya no regala los golpes fuertes (G1 A1, #74)
- **Qué cambia:** antes, cuando el instante de contacto quedaba pegado al borde de su ventana, el Rival fallaba aunque le sobrara tiempo, y por eso los golpes al 100 % al vidrio parecían «punto casi seguro». Ahora, solo en esos casos, desplaza un poco su golpe hacia adentro de la ventana. El error de timing de cada dificultad no cambia.
- **Efecto medido:** el contacto esperado del Rival medio a P=1 sube de alrededor de 81 % a 93,3 %. Pegar fuerte sigue rindiendo, pero menos.
- **Para probar:** juega golpes al 100 % que toquen el vidrio del fondo y fíjate si el punto todavía se siente regalado o si ahora se gana con justicia.

### 2. Cansancio, piernas y reacción (F1, #76)
- **Energía:** cada jugador tiene una energía de 0 a 100. Empieza en 100 en cada partido y al entrar al modo Rally desde el menú.
- **Qué gasta:**
  - correr: 0,4 por metro, incluida la vuelta a la T;
  - golpear fuerte: si P > 0,7, hasta 1 punto por golpe. El saque automático cuenta con P=0, así que no gasta por fuerza.
- **Cómo se recupera:**
  - entre puntos, una vez, el 25 % de lo que falta;
  - durante el rally, quieto, 0,5 por segundo.
- **Efectos bajo 70 de energía:** hasta 10 % menos de velocidad del cuerpo y hasta 40 ms más de reacción cuando la energía llega a 0. Valen igual para el Humano y el Rival.
- **Métricas:** un evento `fatigue` por punto y `f_r` en los golpes de Rally y Partido. La práctica no cambia.

### 3. Cansancio en el brazo y barra de energía (F2, #77)
- **Brazo:** cuando el que golpea está cansado, su golpe sale hasta 4 % más lento. Si además está mal ubicado (estirado o trabado), el error de posición crece hasta 25 %. La mira y el timing no cambian y no hay azar nuevo.
- **Barra:** una línea fina de 3 pt bajo cada nombre en el marcador:
  - neutra con 70 o más;
  - ámbar bajo 70;
  - roja bajo 25.
  - No muestra números, no parpadea, no recibe toques y solo aparece en Rally y Partido con el cansancio activo.

## Lo que no cambia
- **Práctica, SL y AutoQA:** idénticos. AutoQA V y T3 dan PASS en F1 y en F2.
- **Con `fatigue.enabled = 0`:** el juego es idéntico bit a bit al anterior. Se comprobó en 1.927 puntos.
- **Sin cambios en:** física, solver, timing, contrato de contacto, reloj ni gesto.
- **La paralela desde SL5/SL6 (G1 A2):** no entra. Quedó diferida como observación abierta.

## Cómo probar (pauta de la spec, §7 C)
1. **3 partidos completos** con la build DEV.
2. **Preguntas:**
   - ¿El cansancio se nota en los rallies largos?
   - ¿Sentiste alguna vez que perdiste un punto «por el cansancio» sin entender por qué?
   - ¿Cambió cómo jugaste, por ejemplo cerrar antes o hacer correr al rival?
   - ¿La barra se entiende sin explicación?
   - ¿Un final ajustado se sintió más tenso o más frustrante?
3. **Ajuste:** las 11 claves `fatigue.*` se pueden editar desde el panel DEV para una ronda de ajuste jugando. `fatigue.enabled = 0` apaga todo para comparar.

## Puertas de la build
- **F1:** EditMode 790/790 y PlayMode 583/583.
- **F2:** EditMode 803/803 y PlayMode 593/593.
- **AutoQA:** V y T3 en PASS/0/1 en las dos.
- **check-release:** dev/prod OK.
- **Experimento B con todo activo, sin ajuste:** pasa B1–B6. El ritmo queda en 0,94 de la duración sin cansancio. B5b no tiene muestra suficiente.
- **Hashes:** defaults `2ec0c953786a` (132 claves), P0 `f92f406f698a`, `SimulationKey` `166a96e319c7`, `RallyParams` `927e304bbfec`.
