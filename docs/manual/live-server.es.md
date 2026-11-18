# Servidor en vivo

🌐 [English](live-server.md) | [Português](live-server.pt.md) | **Español**

La página **Servidor** muestra la partida en curso en cada uno de tus servidores de juego: el marcador, los jugadores con su salud y su progreso, y los eventos y el chat de la partida. La alimenta el plugin `l4d2_gameinfo_sync`.

[Volver al manual](README.es.md)

## Contenido

- [Elegir un servidor](#elegir-un-servidor)
- [La página en vivo](#la-página-en-vivo)
- [Eventos y chat](#eventos-y-chat)
- [Escribir en el chat desde el sitio](#escribir-en-el-chat-desde-el-sitio)
- [Conectarse al servidor](#conectarse-al-servidor)
- [En el teléfono](#en-el-teléfono)
- [Para tener en cuenta](#para-tener-en-cuenta)

## Elegir un servidor

Con un solo servidor de juego, **Servidor** abre directamente su página en vivo. Con dos o más, muestra una tarjeta por servidor con su estado, mapa, configuración, marcador y cantidad de jugadores. **Seguir** abre la página en vivo de ese servidor, y el botón de al lado copia el comando `connect`.

![El selector de servidores con dos servidores de juego](../images/es/servers.webp)

Mientras un servidor está en línea, la barra del menú muestra junto al nombre del sitio una pequeña insignia verde con la cantidad de jugadores. Enlaza a la página del servidor.

Solo aparecen los servidores habilitados. Un servidor se habilita o deshabilita en su página en [Administración > Servidores de juego](administration.es.md#servidores-de-juego).

## La página en vivo

![La página en vivo de un servidor durante una partida](../images/es/server-live.webp)

De arriba abajo:

- **Pestañas de servidores**, para cambiar entre servidores cuando hay varios.
- **El encabezado**: el nombre del servidor, la configuración y el tamaño de equipo, y una insignia **Anti-Cheat activado** cuando el servidor informa que el Anti-Cheat está activo.
- **IP del Servidor**, con un botón para copiar. El ícono ⓘ abre la dirección y, si está definida, la dirección alternativa (como un nombre DNS).
- **Jugadores**: jugadores conectados y plazas.
- **Mapa**: la campaña, el mapa y la ronda, con **Iniciado** o **Pausado**.
- **Marcador**: los puntos de los supervivientes y de los infectados, y la diferencia.
- **Puntos de progreso**, **Bonificación** y **Progreso**: hasta dónde llegaron los supervivientes en este mapa y el bonus que queda.
- **Tank** y **Bruja**: en qué parte del mapa aparecen, y si el Tank está vivo o muerto.
- **La barra del marcador**: los dos equipos lado a lado.
- **Supervivientes**: el personaje, el jugador, la salud (con la salud temporal en un color más claro), las armas, el progreso y la calidad de la conexión. Los jugadores incapacitados, en blanco y negro y muertos aparecen marcados.
- **Infectado**: la clase, el jugador, la salud y el daño hecho en esta ronda. Los jugadores que todavía están por aparecer aparecen marcados.
- **Espectadores**.
- **Mensajes de chat**: los eventos y el chat de la partida.
- **Un botón `connect`** abajo de todo, que copia el comando para entrar al servidor.

La página se actualiza sola.

## Eventos y chat

El panel **Mensajes de chat** lista, con lo más reciente abajo:

- eventos de la partida, como el inicio o el fin de la ronda, las pausas, la aparición y la muerte del Tank, skeets, levels, crowns, high pounces, death charges, cortes de lengua, rocas y alarmas de autos;
- lo que los jugadores escriben en el chat del juego;
- los mensajes escritos en el sitio.

El botón **Todo / Eventos / Mensajes** filtra la lista. Los mensajes cortos de binds que se usan en el juego, como `gogogo` o `tank tank`, se dejan fuera. La lista conserva la última hora.

## Escribir en el chat desde el sitio

Con el módulo de chat del sitio activado, un jugador conectado puede escribir en el cuadro debajo del chat. El mensaje aparece en el chat del juego de ese servidor y en el panel.

- Un mensaje tiene como máximo 200 caracteres.
- Después de un mensaje, el mismo jugador espera 30 segundos antes de enviar otro. Después de cualquier mensaje, todos esperan 15 segundos.
- Un jugador [baneado del chat](moderation.es.md#baneos-del-chat) ve el motivo y no puede escribir.
- Los administradores no tienen tiempo de espera. Con el módulo de comandos remotos activado, un administrador que escribe un comando de SourceMod como `!forcestart` o `sm_kick name` lo envía al servidor de juego como un [comando remoto](moderation.es.md#comandos-remotos), y el botón **Terminal** junto al cuadro ayuda a armar uno.

## Conectarse al servidor

Cuando el administrador completó la dirección a la que se conectan los jugadores, la página la muestra con un botón para copiar, y el botón `connect` copia `connect <address>`. Pégalo en la consola del juego para entrar. El ícono ⓘ explica cuándo conviene la dirección IP y cuándo la dirección alternativa.

## En el teléfono

![La página en vivo en un teléfono](../images/es/phone-server-live.webp)

En una pantalla angosta los equipos se apilan uno debajo del otro.

## Para tener en cuenta

- La página en vivo va unos 15 segundos detrás del juego, a propósito, como en el sitio que este proyecto reemplaza.
- La partida en curso se guarda solo en memoria. Si el sitio se reinicia, la página queda vacía hasta que el servidor de juego envíe la siguiente actualización.
- Cada servidor de juego tiene su propio estado en vivo, su chat y su cola de comandos.
- El módulo de servidor en vivo se puede desactivar en [Módulos](administration.es.md#módulos). El elemento **Servidor** desaparece del menú, y también el chat del sitio y los comandos remotos, que lo necesitan.
