# Notificaciones

🌐 [English](notifications.md) | [Português](notifications.pt.md) | **Español**

El sitio avisa a cada jugador conectado de lo que pasó y le concierne: un campeonato en el que puede participar, una solicitud para participar respondida, su turno en el draft, una serie programada, un castigo, un regalo. Los administradores también reciben avisos de lo que necesita su atención, como una nueva solicitud para participar o una identidad de equipo esperando aprobación.

[Volver al manual](README.es.md)

## Contenido

- [La campana](#la-campana)
- [La página de notificaciones](#la-página-de-notificaciones)
- [Quién recibe qué](#quién-recibe-qué)
- [Cuánto tiempo se guardan](#cuánto-tiempo-se-guardan)

## La campana

Un jugador conectado ve una campana en la barra superior, junto a la foto. Un número rojo en la campana cuenta las notificaciones aún no leídas. Los visitantes que no iniciaron sesión no ven la campana.

Un clic en la campana abre las 8 notificaciones más recientes. Las no leídas están en negrita, con una marca roja a la izquierda. Un clic en una abre su página, por ejemplo la página del campeonato o el área del capitán, y la marca como leída.

Al final de la lista:

| Enlace | Qué hace |
|---|---|
| Ver todas | Abre la página de notificaciones. |
| Marcar todas como leídas | Pide confirmación y marca todas las notificaciones como leídas. El número de la campana vuelve a cero. |

## La página de notificaciones

**Ver todas** abre `/notifications`, la lista completa de la más nueva a la más antigua, 20 por página. Las no leídas tienen la etiqueta roja **Nueva**. **Abrir** va a la página de la notificación y la marca como leída. **Marcar todas como leídas**, arriba, hace lo mismo que el enlace de la campana.

El texto de cada notificación se muestra en el idioma que eligió el jugador, y los jugadores mencionados aparecen con su nombre actual.

## Quién recibe qué

| Evento | Quién lo recibe |
|---|---|
| Se publica un campeonato, o se abren sus inscripciones | Todos. Cuando el campeonato acepta solicitudes para participar: "Hay un campeonato nuevo en el que puedes participar". |
| Un jugador pide una plaza, o cancela la solicitud | Los administradores. |
| Una solicitud para participar se aprueba o se rechaza | El jugador que la pidió. |
| Un jugador se añade a los participantes, se quita de ellos o se pone en un equipo | Ese jugador. |
| Un jugador pasa a ser capitán o vicecapitán | Ese jugador, con un enlace al área del capitán. |
| Se sortea el orden del draft, o se hace una elección | Los capitanes; el jugador elegido; el capitán y el vicecapitán del equipo de turno. |
| Un equipo envía un nuevo nombre, sigla o bandera | Los administradores. |
| El nuevo nombre, sigla o bandera se aprueba o se rechaza | El capitán y el vicecapitán del equipo. |
| Se crean los juegos de una fase | Los jugadores de los equipos de la fase. |
| Se propone o se rechaza una fecha, se veta o se elige una campaña | El capitán y el vicecapitán del otro equipo. |
| Se programa una serie, termina un juego, se gana una serie o se decide por incomparecencia | Los jugadores de los dos equipos. |
| El campeonato empieza, termina o se cancela | Los participantes y los jugadores de los equipos. |
| Un jugador pasa a ser administrador | Ese jugador. |
| Se da o se quita un castigo | Ese jugador. |
| Se da o se quita un baneo del chat | Ese jugador. |
| Se banea a un jugador de los servidores, o se levanta el baneo | Ese jugador. |
| Una cuenta comparte una dirección IP con una cuenta baneada (ver [Baneos](bans.es.md#cuentas-relacionadas-y-alertas)) | Los administradores. |
| Un jugador es bloqueado para votar en el ranking de la comunidad, o desbloqueado | Ese jugador. |
| Se define o se quita un nombre forzado | Ese jugador. |
| Se regala o se reembolsa una skin | Ese jugador. |
| Empieza una nueva temporada | Todos. La primera temporada, creada al configurar el sitio, no envía este aviso. |
| La copia de seguridad automática falla, o falla su verificación | Los administradores. Una copia iniciada a mano muestra su resultado en la pantalla. |

Una notificación para todos o para los administradores la lee cada persona por separado: cuando un administrador la lee, sigue sin leer para los demás.

## Cuánto tiempo se guardan

Las notificaciones con más de 90 días se borran una vez al día, junto con el registro de quién las leyó.

[Volver al manual](README.es.md)
