# Baneos

🌐 [English](bans.md) | [Português](bans.pt.md) | **Español**

El módulo Baneos guarda en el sitio a los jugadores baneados de los servidores. Los baneos dados en el juego con los comandos de siempre de SourceMod llegan solos, los administradores también pueden banear desde el sitio, y los servidores consultan al sitio sobre cada jugador que se conecta. Todos pueden ver quién está baneado, por qué y cuándo termina el baneo.

[Volver al manual](README.es.md)

## Contenido

- [Encender el módulo](#encender-el-módulo)
- [Para jugadores](#para-jugadores)
- [Para administradores](#para-administradores)
- [Baneos dados en el juego](#baneos-dados-en-el-juego)
- [Cómo los servidores aplican los baneos](#cómo-los-servidores-aplican-los-baneos)
- [Cuentas relacionadas y alertas](#cuentas-relacionadas-y-alertas)
- [Importar banned_user.cfg](#importar-banned_usercfg)

## Encender el módulo

El módulo viene apagado en un sitio nuevo. Enciende **Baneos** en **Administración → Módulos** e instala el plugin `l4d2_bans` en cada servidor (ver [Conectar un servidor](../game-server.es.md)). Sin el plugin la lista existe en el sitio, pero los servidores no envían sus baneos ni revisan a los jugadores.

## Para jugadores

**Más → Baneos** abre `/bans`, la lista de los jugadores baneados ahora: el jugador, el motivo, el día del baneo, cuándo termina (o **Permanente**) y quién baneó. La búsqueda encuentra un baneo por nombre, Steam ID o motivo. Los baneos terminados no están en esta lista.

La página de un jugador baneado empieza con un aviso rojo: "*Nombre* está baneado de los servidores hasta *fecha*", o "de forma permanente", con el motivo y un enlace a la lista.

Un jugador baneado que intenta entrar en un servidor es expulsado con el motivo y el fin del baneo, en el idioma del juego. No hay forma de disputar un baneo en el sitio.

Un jugador conectado al sitio recibe aviso en la campana cuando es baneado y cuando se levanta el baneo (ver [Notificaciones](notifications.es.md)).

Las direcciones IP, las cuentas relacionadas y las alertas nunca se muestran al público: solo los administradores las ven.

## Para administradores

**Administración → Baneos** (`/admin/bans`) tiene tres pestañas:

| Pestaña | Qué muestra |
|---|---|
| Activos | Los baneos vigentes, con el jugador, el motivo, el día, el fin, quién baneó (un administrador, **Servidor** para un baneo dado desde la consola o por un plugin, **Votación**, o **Importado**, con el comando y el servidor debajo) y los **Intentos** del jugador de entrar después del baneo. |
| Terminados | Los baneos que expiraron o fueron levantados, con quién los levantó y por qué. Nada se borra: este es el historial. |
| Alertas | Cuentas que comparten una dirección IP con una cuenta baneada y no fueron baneadas. Ver [Cuentas relacionadas y alertas](#cuentas-relacionadas-y-alertas). |

La búsqueda de arriba filtra las pestañas Activos y Terminados por nombre, Steam ID o motivo.

| Botón | Qué hace |
|---|---|
| **Añadir** | Abre la ventana **Banear a un jugador**: el jugador (un Steam ID, un enlace de perfil o **Buscar jugador**), la duración y el motivo. La duración es **Permanente**, 1 hora, 1 día, 3 días, 1 semana, 2 semanas, 1 mes, 3 meses, 1 año, todas contadas desde ahora, o **Hasta una fecha**, un día y hora en la zona horaria del sitio. Un jugador que ya está baneado no puede ser baneado otra vez: edita el baneo. |
| **Editar** | Cambia el fin y el motivo de un baneo activo. |
| **Desbanear** | Levanta el baneo al instante. El motivo que escribas queda en el historial y solo lo ven los administradores. |
| **Importar** | Lee un archivo `banned_user.cfg`. Ver [Importar banned_user.cfg](#importar-banned_usercfg). |

Cada cambio pide confirmación. Con el [módulo del servidor en vivo](live-server.es.md) encendido, un baneo, un cambio o un desbaneo hecho en el sitio llega a los servidores en pocos segundos: un jugador baneado que está jugando es expulsado, y un jugador desbaneado sale de la lista de baneos del servidor. Sin ese módulo, el cambio se aplica la próxima vez que el jugador se conecte.

## Baneos dados en el juego

El plugin `l4d2_bans` no añade comandos. Escucha los baneos de SourceMod, así que todo lo que banea a un jugador llega al sitio:

| En el juego | En el sitio |
|---|---|
| `sm_ban <jugador> <minutos> [motivo]`, o el baneo del menú de admin | Un baneo de esa duración (0 es permanente), con el administrador que lo dio. |
| `sm_addban <minutos> <steamid> [motivo]` | Lo mismo, para un jugador que no está en el servidor. |
| Una votación de baneo (`sm_voteban`) | Un baneo mostrado como **Votación**. |
| Un plugin que banea, como `l4d2_mix` cuando un jugador abandona el mix | Un baneo mostrado como **Servidor**, con el comando del plugin. Es un baneo normal, listado como los demás. |
| `sm_unban <steamid>` | Levanta el baneo activo del jugador. |

Banear otra vez a un jugador que ya está baneado reemplaza el fin y el motivo del baneo activo. Solo se guardan los baneos por Steam ID; un baneo por dirección IP (`sm_banip`) lo aplica el servidor, pero no llega al sitio.

El servidor sigue aplicando sus propios baneos como antes, así que un baneo dado con el sitio caído sigue valiendo en ese servidor. El plugin guarda los baneos que el sitio no recibió y los envía de nuevo cada minuto.

## Cómo los servidores aplican los baneos

Cuando un jugador se conecta, el plugin consulta al sitio sobre ese jugador, una vez. El sitio responde por un índice, así que el tamaño de la lista no importa, y el servidor no espera la respuesta: la consulta corre en segundo plano. Cuando el jugador está baneado, el plugin lo expulsa con el motivo y también añade el baneo al propio servidor por el tiempo que falta, así que un jugador que se reconecta es rechazado por el servidor sin consultar al sitio otra vez. Cada uno de estos intentos se cuenta en la pestaña **Activos**.

Si el sitio no responde, el jugador entra, salvo por los baneos que el propio servidor guarda: los dados en ese servidor y los que aprendió del sitio desde que se inició.

## Cuentas relacionadas y alertas

El sitio sabe qué cuentas de Steam se conectaron desde la misma dirección IP (ver [Conexiones de jugadores](moderation.es.md)).

Cuando baneas a un jugador desde el sitio y ese jugador tiene cuentas relacionadas, una ventana las lista y pregunta si también deben ser baneadas, con la misma duración y el mismo motivo. Desmarca una cuenta para dejarla fuera; las cuentas que ya están baneadas aparecen marcadas y no se pueden elegir.

Se crea una alerta, y los administradores reciben aviso en la campana, cuando:

- una cuenta se conecta desde la dirección de una cuenta baneada después del baneo, o el jugador baneado se conecta desde una dirección nueva donde jugaron otras cuentas;
- un baneo llega de un servidor, o se importa, y el jugador ya comparte una dirección con otras cuentas.

La pestaña **Alertas** lista la cuenta, la cuenta baneada con la que está relacionada y el motivo de ese baneo. **Banear** abre la ventana **Banear a un jugador** con la cuenta, el motivo "Cuenta relacionada de *nombre*" y el fin del baneo original. **Ignorar** cierra la alerta, y no vuelve a aparecer para ese baneo. Una alerta también sale de la lista cuando la cuenta es baneada o cuando termina el baneo original.

## Importar banned_user.cfg

**Importar** recibe el contenido del archivo `cfg/banned_user.cfg` de un servidor. Cada línea `banid` se convierte en un baneo. Los comentarios escritos encima de ella se leen cuando siguen este formato:

```text
// Name: Jugador
// Ban date: 2026-08-24
// Expire in: never
// Ban reason: Trampas
banid 0 STEAM_0:1:12345678
banid 0 STEAM_1:1:12345678
```

Las dos líneas del mismo jugador se convierten en un solo baneo. Una línea en blanco separa a los jugadores. Los jugadores que ya están baneados, y los baneos cuyo fin ya pasó, se quedan como están; las líneas que no se pueden leer se cuentan y se omiten. Los baneos importados se muestran como **Importado**.
