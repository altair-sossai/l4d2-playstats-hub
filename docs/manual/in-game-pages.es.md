# Páginas dentro del juego

🌐 [English](in-game-pages.md) | [Português](in-game-pages.pt.md) | **Español**

Los jugadores pueden ver el ranking sin salir de Left 4 Dead 2. El comando de chat `!ranking` abre una versión ligera del sitio en el panel del propio juego. Con el plugin `l4d2_ranking` también se abre solo para los jugadores que entran antes de que empiece una partida.

[Volver al manual](README.es.md)

## Contenido

- [Las tres páginas](#las-tres-páginas)
- [Por qué se ven distintas](#por-qué-se-ven-distintas)
- [La dirección que usa el juego](#la-dirección-que-usa-el-juego)

## Las tres páginas

El panel tiene tres pestañas. Muestran solo la temporada actual, y solo las partidas del ranking.

**Clasificación**: los 50 mejores jugadores, con patente, experiencia, victorias, derrotas, porcentaje de victorias y MVP. La fila del jugador que abrió el panel aparece resaltada.

![El ranking dentro del juego](../images/es/ingame-ranking.webp)

**Mi resumen**: la patente del jugador que abrió el panel, la experiencia y el progreso hacia la próxima patente, victorias, derrotas, porcentaje de victorias, mejor racha de victorias, MVP y rage quits, y los promedios por partida.

![El resumen del jugador que abrió el panel](../images/es/ingame-player.webp)

Un jugador sin ninguna partida del ranking en la temporada ve un mensaje corto en su lugar.

**Partidas**: las últimas 10 partidas del ranking de la temporada, con la fecha (día/mes), la campaña, los jugadores de cada equipo y el marcador. El marcador ganador está en verde.

![Las últimas partidas, dentro del juego](../images/es/ingame-matches.webp)

Las páginas están en `/ingame`, `/ingame/player` y `/ingame/matches`. El juego agrega el SteamID64 del jugador a la dirección, para que el sitio sepa quién abrió el panel. También se pueden abrir en un navegador normal, por ejemplo `/ingame?steamid=76561198000000000`.

## Por qué se ven distintas

El panel del juego es un navegador de 2012, de 900 × 450 píxeles. Solo ejecuta JavaScript antiguo y le falta la mayor parte del CSS actual. Estas páginas están hechas para él: sin scripts, con ancho fijo y solo lo que cabe en una pantalla. [El navegador dentro del juego](../in-game-browser.es.md) registra lo que ese navegador soporta.

Son de solo lectura y necesitan el módulo del ranking.

## La dirección que usa el juego

No se espera que el navegador del juego abra sitios HTTPS actuales. Dale al juego una dirección HTTP simple del sitio, como `http://203.0.113.10:5080`, en el ajuste `playstats_ingame_url` del servidor de juego. Cuando está vacío, el plugin usa `playstats_url`.

- [Instalar en un VPS Linux](../install-linux.es.md#la-dirección-usada-dentro-del-juego) explica por qué el puerto sigue abierto cuando usas un dominio con HTTPS.
- [Conectar un servidor de juego](../game-server.es.md) muestra dónde va el ajuste.
