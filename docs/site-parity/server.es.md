# Paridad del sitio: servidor

🌐 [English](server.md) | [Português](server.pt.md) | **Español**

Lista de verificación de todo lo que el sitio antiguo mostraba en el área del **servidor**, y de cómo está en el hub.

Los archivos antiguos están en `l4d2-playstats-web/L4D2PlayStats.Web`. `Web/` es el proyecto `L4D2PlayStats.Web` y `Core/` es `L4D2PlayStats.Core`.

Estado: `done` (mismo contenido y aspecto), `adapted` (presente, cambiado por el motivo indicado), `not ported` (no puede existir en el hub, con el motivo).

## Contenido

- [Rutas](#rutas)
- [Página del servidor](#página-del-servidor)
- [Encabezado del marcador](#encabezado-del-marcador)
- [Jugadores](#jugadores)
- [Feed y chat](#feed-y-chat)
- [Eventos del juego](#eventos-del-juego)
- [Terminal](#terminal)
- [Modal de información del servidor](#modal-de-información-del-servidor)
- [Reglas del estado en vivo](#reglas-del-estado-en-vivo)
- [Servidor de juego y API pública](#servidor-de-juego-y-api-pública)
- [Ganchos del layout](#ganchos-del-layout)
- [Página de baneos de chat](#página-de-baneos-de-chat)
- [API y reglas de los baneos de chat](#api-y-reglas-de-los-baneos-de-chat)
- [Páginas exclusivas del hub](#páginas-exclusivas-del-hub)

## Rutas

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `/server` abre la página en vivo | `Web/Controllers/ServerController.cs:16` | adapted | El hub tiene varios servidores de juego, así que la página vive en `/servers`: con un solo servidor habilitado es la propia página en vivo, con más es un selector y cada servidor está en `/servers/{publicId}`. `/server` redirige allí de forma permanente. |
| `/servers` redirige a `/server` | `Web/Controllers/ServerController.cs:26` | adapted | Al revés: `/servers` es la ruta del hub y `/server` redirige a ella. |
| Parcial `/server/header` | `Web/Controllers/ServerController.cs:32` | adapted | Ahora `/servers/{publicId}/header`. La ruta antigua redirige (302, el destino depende del servidor) al servidor con más jugadores. |
| Parcial `/server/players` | `Web/Controllers/ServerController.cs:40` | adapted | Ahora `/servers/{publicId}/players`, con la misma redirección. |
| Parcial `/server/feed?after=`, 204 cuando no hay nada nuevo | `Web/Controllers/ServerController.cs:48` | adapted | Ahora `/servers/{publicId}/feed?after=`, que sigue respondiendo 204 cuando no hay nada nuevo, con la misma redirección. `after` es la secuencia del feed en lugar de los ticks del elemento (ver "Elementos nuevos añadidos"). |
| `/connect` redirige a `steam://connect/<ip>` | `Web/Controllers/ConnectController.cs:8` | done | Ya está en `SiteController`: va al servidor con más jugadores, o a `/servers` cuando ningún servidor tiene dirección. |
| Página de administrador `/chat-bans` | `Web/Controllers/ChatBansController.cs:17` | adapted | Ahora `/admin/chat-bans`, como todas las páginas de administrador del hub. `/chat-bans` redirige allí de forma permanente, conservando la query string. |
| Elemento del menú "Servidor" activo (`ViewBag.Server`) | `Web/Controllers/ServerController.cs:19` | done |  |
| Elementos del menú "Admin" y "Baneos del chat" activos | `Web/Controllers/ChatBansController.cs:20` | done |  |

## Página del servidor

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Metadatos de página: título "Servidor" | `Web/PageMetadata/ServerPageMetadata.cs:20` | done |  |
| Metadatos de página: descripción con el mapa y los jugadores conectados | `Web/PageMetadata/ServerPageMetadata.cs:21` | adapted | El texto toma el nombre del sitio en lugar de "L4D2 Competitive" (texto nuevo en los fragmentos `server`). El selector tiene su propia descripción. |
| Metadatos de página: la imagen es la miniatura de la campaña, el alt es la campaña o el mapa | `Web/PageMetadata/ServerPageMetadata.cs:22` | done |  |
| Título `<h4 class="text-center">Server</h4>` | `Web/Views/Server/Index.cshtml:835` | done |  |
| Contenedor `#game-info-header` con el parcial del encabezado | `Web/Views/Server/Index.cshtml:837` | done |  |
| Contenedor `#game-info-players` con el parcial de jugadores | `Web/Views/Server/Index.cshtml:841` | done |  |
| Fila `#game-info-chat`, oculta mientras no hay nadie conectado | `Web/Views/Server/Index.cshtml:845` | adapted | La misma fila. Solo tiene este id cuando hay un único servidor habilitado, porque el layout la muestra y la oculta según el servidor con más jugadores; con varios servidores la propia página la alterna desde el parcial de jugadores. |
| Botón "connect <ip>", centrado, oculto en teléfonos (`hide-mobile`) | `Web/Views/Server/Index.cshtml:915` | adapted | El mismo botón. Enlaza directamente a `steam://connect/<address>` del servidor en pantalla (el antiguo `/connect` solo hacía esa redirección) y se omite cuando el servidor no tiene dirección. |
| Estilos de página (tablas, barras de progreso, latencia, feed, colores de los eventos, terminal) | `Web/Views/Server/Index.cshtml:21` | done |  |
| Encabezado actualizado cada 2,6 s y parcheado con diffDOM | `Web/Views/Server/Index.cshtml:195` | done |  |
| Jugadores actualizados cada 2,6 s y parcheados con diffDOM | `Web/Views/Server/Index.cshtml:211` | done |  |
| Feed actualizado cada 1,3 s, con una sola solicitud a la vez | `Web/Views/Server/Index.cshtml:244` | done |  |
| Las solicitudes expiran a los 2 s y la siguiente se programa al terminar | `Web/Views/Server/Index.cshtml:199` | done |  |

## Encabezado del marcador

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título del panel: nombre del servidor, nombre de la configuración y tamaño del equipo ("4v4"), unidos por " - " | `Web/Views/Server/_Header.cshtml:22` | adapted | El nombre es el nombre de host que el servidor de juego envía en su heartbeat (el sitio antiguo se lo pedía a Steam); antes del primer heartbeat es el nombre con el que se registró el servidor. |
| Etiqueta del anti-cheat (activado, desactivado, API no disponible) con icono de candado, color y tooltip | `Web/Views/Server/_Header.cshtml:37` | done |  |
| La etiqueta del anti-cheat enlaza a la página del anti-cheat | `Web/Views/Server/_Header.cshtml:46` | adapted | La etiqueta es la misma. El enlace solo se escribe mientras el módulo anti-cheat está activado, porque el hub todavía no tiene página de anti-cheat (el módulo está reservado). |
| Etiqueta "IP del Servidor" con el botón de signo de interrogación que abre el modal de información del servidor | `Web/Views/Server/_Header.cshtml:58` | done |  |
| Valor de la IP del servidor con el icono de copiar (copia `connect <ip>`, popover "¡Texto copiado!", icono según el tema) | `Web/Views/Server/_Header.cshtml:65` | done | La dirección es la del servidor en pantalla. El icono de copiar se omite cuando el servidor no tiene dirección. |
| Jugadores: conectados / máximo | `Web/Views/Server/_Header.cshtml:73` | adapted | Conectados es el número de jugadores en el estado en vivo (supervivientes, infectados y espectadores) en lugar de la consulta a Steam. Máximo son los slots que el servidor de juego envía en su heartbeat (`maxPlayers`, añadido al plugin); con un plugin más antiguo es el doble del tamaño del equipo. |
| Mapa: campaña, nombre del mapa y ronda ("1.ª ronda" / "2.ª ronda") | `Web/Views/Server/_Header.cshtml:78` | adapted | El mapa viene del heartbeat del servidor de juego (cada 30 s) en lugar de la consulta a Steam. Los nombres de las campañas vienen del catálogo del hub. |
| Mapa: etiqueta Pausado, Iniciando... o Iniciado con tooltip | `Web/Views/Server/_Header.cshtml:96` | done |  |
| Marcador: etiquetas de supervivientes e infectados, verde para el que va ganando, rojo para el otro, gris en caso de empate, con tooltips | `Web/Views/Server/_Header.cshtml:117` | done |  |
| Marcador: etiqueta de diferencia cuando no es cero | `Web/Views/Server/_Header.cshtml:131` | done |  |
| Puntos de progreso: actual / máximo y etiqueta de porcentaje | `Web/Views/Server/_Header.cshtml:139` | done |  |
| Bonificación: actual / máximo y etiqueta de porcentaje | `Web/Views/Server/_Header.cshtml:154` | done |  |
| Porcentaje de progreso | `Web/Views/Server/_Header.cshtml:165` | done |  |
| Tank: porcentaje, "Desactivado", y las etiquetas Vivo / Muerto | `Web/Views/Server/_Header.cshtml:170` | done |  |
| Bruja: porcentaje o "Desactivado" | `Web/Views/Server/_Header.cshtml:196` | done |  |
| Diseño para teléfonos: `table-responsive` con `data-label` en cada celda | `Web/Views/Server/_Header.cshtml:55` | done |  |

## Jugadores

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Todo el bloque oculto mientras no hay nadie conectado | `Web/Views/Server/_Players.cshtml:41` | done |  |
| Bloques JSON `#survivors`, `#infecteds`, `#spectators` que usa la terminal | `Web/Views/Server/_Players.cshtml:43` | done | Los mismos bloques con los mismos campos de jugador, menos `avatarUrl`, que el hub no guarda (los avatares los sirve `/avatar/{id}`). |
| Barra del marcador: dos segmentos dimensionados según la puntuación, verde y rojo, "+diferencia" en el que va ganando, marca central discontinua, tooltips | `Web/Views/Server/_Players.cshtml:55` | done |  |
| Títulos de los paneles "Supervivientes" e "Infectados" con la etiqueta de puntuación mientras un equipo no tiene puntos | `Web/Views/Server/_Players.cshtml:83` | done |  |
| Superviviente: imagen del personaje con tooltip, en gris cuando está muerto | `Web/Views/Server/_Players.cshtml:101` | done |  |
| Superviviente: avatar enlazado a la página del jugador, en gris cuando está muerto | `Web/Views/Server/_Players.cshtml:106` | done | Avatar de `/avatar/{id}` con el avatar vacío como reserva. El enlace va al perfil de Steam cuando el módulo de ranking está desactivado. |
| Superviviente: nombre enlazado a la página del jugador | `Web/Views/Server/_Players.cshtml:114` | done |  |
| Superviviente: etiquetas Admin, Muerto, Incapacitado, Blanco y negro | `Web/Views/Server/_Players.cshtml:116` | done |  |
| Superviviente: icono de latencia (baja, media, alta) con tooltip | `Web/Views/Server/_Players.cshtml:145` | done |  |
| Superviviente: iconos de armas para las cinco ranuras, según el tema, con el icono del arma cuerpo a cuerpo en lugar del genérico | `Web/Views/Server/_Players.cshtml:157` | done | Las rutas de las imágenes son absolutas, porque la página ya no está en la raíz. |
| Qué armas tienen icono | `Web/Extensions/WeaponExtensions.cs:7`, `Web/Extensions/MeleeWeaponExtensions.cs:7` | done |  |
| Nombres de archivo y descripciones de los iconos de armas | `Core/GameInfo/Enums/Weapon.cs`, `Core/GameInfo/Enums/MeleeWeapon.cs` | done | Añadidos a los enums del hub (atributo `WeaponName`). |
| Superviviente: barra de salud (permanente y temporal, color según la salud, atenuada cuando está incapacitado) y número | `Web/Views/Server/_Players.cshtml:191` | done |  |
| Superviviente: barra de progreso y porcentaje | `Web/Views/Server/_Players.cshtml:231` | done |  |
| Supervivientes ordenados por personaje, separados por una línea | `Core/GameInfo/GameInfo.cs:181`, `Web/Views/Server/_Players.cshtml:248` | done |  |
| Infectado: imagen de la clase con tooltip ("ninguno" mientras no ha aparecido) | `Web/Views/Server/_Players.cshtml:282` | done |  |
| Infectado: avatar y nombre enlazados a la página del jugador | `Web/Views/Server/_Players.cshtml:290` | done | Las mismas observaciones que para el avatar del superviviente. |
| Infectado: etiquetas Admin, Spawning, Muerto | `Web/Views/Server/_Players.cshtml:302` | done |  |
| Infectado: daño causado, con tooltip | `Web/Views/Server/_Players.cshtml:325` | done |  |
| Infectado: icono de latencia | `Web/Views/Server/_Players.cshtml:330` | done |  |
| Infectado: barra de salud y "salud / máximo" | `Web/Views/Server/_Players.cshtml:342` | done |  |
| Infectados ordenados por daño, separados por una línea | `Core/GameInfo/GameInfo.cs:188`, `Web/Views/Server/_Players.cshtml:366` | done |  |
| Espectadores: número y nombres como etiquetas enlazadas a la página del jugador, ordenados por nombre | `Web/Views/Server/_Players.cshtml:383`, `Core/GameInfo/GameInfo.cs:195` | done |  |
| Ids de fila estables (`tr0-survivor-0`...) para que diffDOM parchee en su lugar | `Web/Views/Server/_Players.cshtml:100` | done |  |
| Dos columnas en escritorio, apiladas en teléfonos (`col-xs-12 col-sm-6`) | `Web/Views/Server/_Players.cshtml:80` | done |  |

## Feed y chat

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Panel "Mensajes de chat" con el desplegable de filtro Todo / Eventos / Mensajes | `Web/Views/Server/Index.cshtml:848` | done |  |
| Filtro recordado en `localStorage` (`feedFilter`) y aplicado a los elementos nuevos | `Web/Views/Server/Index.cshtml:226` | done |  |
| Caja del feed de 400 px de alto, desplazada hasta el final al cargar y mantenida al final cuando llegan elementos nuevos | `Web/Views/Server/Index.cshtml:100` | done |  |
| Un `<p>` por elemento con `data-feed-when` y `data-feed-kind` | `Web/Views/Server/_Feed.cshtml:6` | done | También lleva `data-feed-sequence` (ver más abajo) y la hora del elemento como título al pasar el cursor. |
| Elementos nuevos añadidos, duplicados eliminados, como máximo 300 conservados | `Web/Views/Server/Index.cshtml:264` | adapted | Mismo comportamiento. El cursor que se envía en `after` es la secuencia del feed de la última respuesta en lugar de los ticks del último elemento en pantalla: los mensajes del juego aparecen con 15 s de retraso, así que con los ticks un mensaje del sitio podía ocultar un mensaje del juego que había llegado justo antes. |
| Mensaje: equipo entre corchetes para el chat de equipo | `Web/Views/Server/_Message.cshtml:17` | done |  |
| Mensaje: nombre del jugador coloreado según el equipo (superviviente, infectado, sitio, espectador) y enlazado a la página del jugador | `Web/Views/Server/_Message.cshtml:21` | done | El enlace va al perfil de Steam cuando el módulo de ranking está desactivado. |
| Mensaje: "(Admin)" después del nombre | `Web/Views/Server/_Message.cshtml:24` | done |  |
| Texto del mensaje | `Web/Views/Server/_Message.cshtml:29` | done |  |
| Visitante baneado: "Has sido baneado del chat." y el motivo, sin formulario | `Web/Views/Server/Index.cshtml:872` | done |  |
| Con sesión iniciada: caja de mensaje, botón Enviar deshabilitado mientras está vacía, Enter envía | `Web/Views/Server/Index.cshtml:884` | done | La caja también limita el texto a 200 caracteres. |
| Administrador: botón de la terminal con icono y tooltip | `Web/Views/Server/Index.cshtml:887` | adapted | Se muestra a un administrador solo mientras el módulo de comandos remotos está activado, ya que de otro modo los comandos no llegarían al servidor de juego. |
| Sin sesión iniciada: botón "Inicia sesión con Steam para usar el chat." con el icono de Steam | `Web/Views/Server/Index.cshtml:905` | done | Después de iniciar sesión el visitante vuelve a esta página. |
| Envío: POST de JSON, feed actualizado y caja vaciada si tiene éxito, alerta con el error en caso contrario | `Web/Views/Server/Index.cshtml:293` | adapted | Envía un formulario con el token antiforgery a `/servers/{publicId}/chat` y recibe la misma respuesta `{ success, errorMessage }`. La API antigua no tenía protección contra solicitudes falsificadas. Los textos de error están traducidos. |
| Envío estando baneado: alerta y recarga de la página | `Web/Views/Server/Index.cshtml:313` | done | HTTP 403 con el motivo, como antes. |

## Eventos del juego

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| El nombre del jugador en un evento enlaza al perfil de Steam, o es texto plano si no lo tiene | `Web/Extensions/GameEventHtmlExtensions.cs:20` | done |  |
| Partes coloreadas: estrellas y resaltado, jugador, acción, peligro | `Web/Views/Server/Index.cshtml:121` | done |  |
| Estrellas delante de los eventos que las tienen | `Core/GameInfo/Extensions/GameEventTypeExtensions.cs:7` | done |  |
| Vómito de boomer acertado | `Web/Views/Server/GameEvents/_BoomerVomitLandedEvent.cshtml` | done |  |
| Alarma de coche activada, con el motivo y el infectado que ayudó | `Web/Views/Server/GameEvents/_CarAlarmTriggeredEvent.cshtml` | done |  |
| Level a un charger | `Web/Views/Server/GameEvents/_ChargerLevelEvent.cshtml` | done |  |
| Level a un charger con daño previo | `Web/Views/Server/GameEvents/_ChargerLevelHurtEvent.cshtml` | done |  |
| Death charge, con la nota de bowling y la altura | `Web/Views/Server/GameEvents/_DeathChargeEvent.cshtml` | done |  |
| Evento desconocido | `Web/Views/Server/GameEvents/_GameEvent.cshtml` | done |  |
| High pounce de hunter, con el daño y la altura | `Web/Views/Server/GameEvents/_HunterHighPounceEvent.cshtml` | done |  |
| Mix iniciado | `Web/Views/Server/GameEvents/_MixStartedEvent.cshtml` | done |  |
| Pausa y reanudación | `Web/Views/Server/GameEvents/_PauseEvent.cshtml` | done |  |
| Muerte de un jugador | `Web/Views/Server/GameEvents/_PlayerDeathEvent.cshtml` | done |  |
| Ronda terminada, con la puntuación y la diferencia | `Web/Views/Server/GameEvents/_RoundEndedEvent.cshtml` | done |  |
| Ronda en marcha, primera o segunda | `Web/Views/Server/GameEvents/_RoundLiveEvent.cshtml` | done |  |
| Skeet (melee, sniper, granada, normal) y skeet en equipo | `Web/Views/Server/GameEvents/_SkeetEvent.cshtml` | done |  |
| Skeet con daño previo y nota de overkill | `Web/Views/Server/GameEvents/_SkeetHurtEvent.cshtml` | done |  |
| El tank pasó a ser un bot | `Web/Views/Server/GameEvents/_TankBecameBotEvent.cshtml` | done |  |
| El tank murió | `Web/Views/Server/GameEvents/_TankDiedEvent.cshtml` | done |  |
| Roca del tank recibida | `Web/Views/Server/GameEvents/_TankRockEatenEvent.cshtml` | done |  |
| Roca del tank skeeteada | `Web/Views/Server/GameEvents/_TankRockSkeetedEvent.cshtml` | done |  |
| Apareció el tank | `Web/Views/Server/GameEvents/_TankSpawnedEvent.cshtml` | done |  |
| Lengua cortada | `Web/Views/Server/GameEvents/_TongueCutEvent.cshtml` | done |  |
| Crown a la witch | `Web/Views/Server/GameEvents/_WitchCrownEvent.cshtml` | done |  |
| Draw crown a la witch | `Web/Views/Server/GameEvents/_WitchCrownHurtEvent.cshtml` | done |  |

## Terminal

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Modal con la vista previa del comando y la lista de opciones | `Web/Views/Server/Index.cshtml:923` | done |  |
| Los 24 comandos con sus descripciones y argumentos | `Web/Views/Server/Index.cshtml:324` | done |  |
| Listas de argumentos: comodines, supervivientes, infectados, espectadores, jugadores, equipo, minutos, motivo, modo de partida, estado del anti-cheat, campaña | `Web/Views/Server/Index.cshtml:422` | done |  |
| Lista de campañas cargada desde `/api/campaigns` | `Web/Views/Server/Index.cshtml:180` | done | Endpoint nuevo en el hub, mismo JSON (`name`, `maps`). |
| Selección paso a paso con "Volver", valores con espacios entre comillas | `Web/Views/Server/Index.cshtml:617` | done |  |
| Confirmación antes de ejecutar, y después se envía mediante el formulario del chat | `Web/Views/Server/Index.cshtml:761` | done |  |
| Un comando escrito en el chat por un administrador se ejecuta en el servidor de juego | `Web/Controllers/Api/ExternalChatController.cs:43` | done | Necesita el módulo de comandos remotos. Cada comando se escribe también en el registro de auditoría. |

## Modal de información del servidor

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Modal "Info. del servidor" abierto desde el encabezado | `Web/Views/Server/Index.cshtml:942` | done |  |
| Fila IP con la etiqueta "Recomendado", `connect <ip>` e icono de copiar | `Web/Views/Server/Index.cshtml:954` | done | Se muestra cuando el servidor tiene dirección. |
| Fila DNS con la etiqueta "Alternativa", `connect <dns>` e icono de copiar | `Web/Views/Server/Index.cshtml:966` | adapted | Campo nuevo "dirección alternativa" en el servidor de juego (se define en la administración). La fila y la explicación aparecen cuando está rellenado. |
| Explicación de cuándo usar el DNS | `Web/Views/Server/Index.cshtml:980` | done | Se muestra junto con la fila DNS. |

## Reglas del estado en vivo

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Todo lo que viene del juego se muestra con 15 s de retraso | `Core/GameInfo/GameInfo.cs:28` | done |  |
| La configuración, la ronda y los jugadores caducan a las 2 h, el marcador a los 10 min | `Core/GameInfo/GameInfo.cs:33` | done |  |
| El feed conserva los elementos durante 1 h, 250 como máximo | `Core/GameInfo/GameInfo.cs:27`, `Core/GameInfo/Structures/TimedFeed.cs:5` | done |  |
| Los binds de chat se descartan ("gogogo", "tank tank", "r"...) | `Core/GameInfo/Filters/ChatBindBlacklist.cs` | done |  |
| Un mensaje de un jugador igual al anterior se descarta | `Core/GameInfo/GameInfo.cs:122` | done |  |
| Mensaje del sitio: 200 caracteres como máximo | `Core/GameInfo/GameInfo.cs:154` | done |  |
| Mensaje del sitio: 15 s entre dos cualesquiera, 30 s por visitante; los administradores están exentos | `Core/GameInfo/GameInfo.cs:161` | done |  |
| Los mensajes del sitio se conservan 1 h para que el servidor de juego los lea | `Core/GameInfo/GameInfo.cs:96` | done |  |
| Un mensaje del sitio aparece en el feed de inmediato, como equipo "Externo" | `Core/GameInfo/GameInfo.cs:175`, `Core/GameInfo/Models/ExternalChatMessage.cs:17` | done |  |
| Latencia: baja por debajo de 100 ms, media por debajo de 200 ms, alta en los demás casos | `Core/GameInfo/Models/Player.cs:25` | done |  |
| Color de la salud: rojo cuando está incapacitado o por debajo de 25, naranja por debajo de 40, verde en los demás casos; máximo 100, o 300 incapacitado | `Core/GameInfo/Models/Survivor.cs:11` | done |  |
| Valores del marcador (porcentajes, diferencia, líder) | `Core/GameInfo/Models/Scoreboard.cs:13` | done |  |
| Tank y witch deshabilitados al 0 % | `Core/GameInfo/Models/Round.cs:14` | done |  |
| Estrellas de los eventos por tipo; un skeet en equipo o un hunter bot da una | `Core/GameInfo/Extensions/GameEventTypeExtensions.cs`, `Core/GameInfo/Models/Events/SkeetEvent.cs:11` | done |  |
| Formato de los comandos del servidor (`!`, `/` o `sm_`, 256 caracteres como máximo) | `Core/GameInfo/Models/ServerCommand.cs:7` | adapted | Mismo formato; el hub rechaza además un punto y coma o un salto de línea en los argumentos, para que un mensaje no pueda llevar dos comandos. |
| Conexiones de jugadores: guardadas por dirección, cuentas relacionadas devueltas, limpieza | `Core/GameInfo/Services/PlayerConnectionInfoService.cs` | done | Las conexiones se conservan durante tres meses. |
| Nombre del servidor, mapa, jugadores y slots leídos de Steam cada 10 s | `Core/Steam/ServerInfo/Services/Cache/ServerInfoServiceCached.cs` | adapted | El hub no consulta a Steam (eso necesita una clave de API y una dirección fija). Los mismos cuatro valores vienen del propio servidor de juego: nombre de host, mapa y slots en el heartbeat, jugadores del estado en vivo. |

## Servidor de juego y API pública

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `PUT /api/game-info/configuration`, `round`, `scoreboard`, `players`, `messages` (servidor de juego) | `Web/Controllers/Api/GameInfoController.cs:33` | done |  |
| `POST /api/game-info/events` (servidor de juego) | `Web/Controllers/Api/GameInfoController.cs:123` | done |  |
| `POST /api/game-info/player-connection-info` (servidor de juego) | `Web/Controllers/Api/GameInfoController.cs:132` | done |  |
| `GET /api/game-info/server-command/dequeue` (servidor de juego) | `Web/Controllers/Api/GameInfoController.cs:144` | done |  |
| `GET /api/game-info` y `/configuration`, `/round`, `/scoreboard`, `/players`, `/messages`, `/events` (público) | `Web/Controllers/Api/GameInfoController.cs:19` | adapted | Ahora por servidor: `/api/servers/{publicId}/game-info` y las mismas subrutas. Las rutas antiguas redirigen (302) al servidor con más jugadores. |
| `GET /api/external-chat?after=` | `Web/Controllers/Api/ExternalChatController.cs:20` | adapted | Misma ruta y JSON, leído por el servidor de juego con su token; cada servidor recibe solo los mensajes escritos en su propia página. |
| `POST /api/external-chat` (visitante con sesión iniciada) | `Web/Controllers/Api/ExternalChatController.cs:28` | adapted | Reemplazado por `POST /servers/{publicId}/chat` (ver "Envío"). Un POST no se puede redirigir, y ninguna página usa la ruta antigua. |
| `GET /api/server` | `Web/Controllers/Api/ServerController.cs:9` | adapted | Misma ruta, usada por el layout. El JSON tiene `publicId`, `name`, `address`, `map`, `players` y `max_players` en lugar de la respuesta completa de Steam. |
| `GET /api/servers` redirige a `/api/server` | `Web/Controllers/Api/ServerController.cs:17` | not ported | En el hub `/api/servers` es la lista pública de servidores de juego, así que no puede redirigir a `/api/server`. |
| `GET /api/campaigns` | `Web/Controllers/Api/CampaignController.cs` | done |  |

## Ganchos del layout

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Insignia de la navbar "● jugadores/slots", pulsante, enlazada a la página del servidor, oculta cuando no hay nadie conectado | `Web/Views/Shared/_Layout.cshtml:322` | done | Ya está en el layout. Los números son los del encabezado (ver "Jugadores"). |
| Título del navegador con el prefijo "🟢 jugadores/slots" | `Web/Views/Shared/_Layout.cshtml:602` | done |  |
| Fila del chat mostrada u oculta por la misma comprobación, cada 4 s | `Web/Views/Shared/_Layout.cshtml:594` | done |  |
| Icono flotante de conexión, oculto en teléfonos | `Web/Views/Shared/_Layout.cshtml:526` | done | Ya está en el layout. Se oculta mientras ningún servidor tiene dirección; el antiguo estaba siempre ahí. |

## Página de baneos de chat

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Solo administradores | `Web/Controllers/ChatBansController.cs:10` | done | También necesita el módulo de chat del sitio. |
| Encabezado "Baneos del chat" con el botón "Añadir baneo" | `Web/Views/ChatBans/Index.cshtml:35` | done |  |
| Texto introductorio | `Web/Views/ChatBans/Index.cshtml:42` | done |  |
| Contador de jugadores baneados | `Web/Views/ChatBans/Index.cshtml:44` | done |  |
| Búsqueda por nombre, Steam ID o motivo, con "Limpiar filtros" | `Web/Views/ChatBans/Index.cshtml:51`, `Web/Controllers/ChatBansController.cs:25` | done |  |
| Elemento de la lista: avatar, nombre, Steam ID y SteamID64, motivo en dos líneas ("Sin motivo indicado") | `Web/Views/ChatBans/Index.cshtml:67` | done | El Steam ID se escribe `STEAM_1:...`, como en todo el hub. |
| Elemento de la lista: fecha del baneo y "por <administrador>" | `Web/Views/ChatBans/Index.cshtml:83` | done | Las fechas están en la zona horaria de los ajustes del sitio en lugar de la zona horaria de la máquina. |
| Texto de la lista vacía | `Web/Views/ChatBans/Index.cshtml:63` | done |  |
| Paginación, 20 por página, que conserva la búsqueda | `Web/Views/ChatBans/Index.cshtml:94`, `Web/Controllers/ChatBansController.cs:14` | done |  |
| Modal de detalles: avatar, nombre enlazado a la página del jugador, IDs, enlace al perfil de Steam | `Web/Views/ChatBans/Index.cshtml:96` | done | El nombre enlaza al perfil de Steam cuando el módulo de ranking está desactivado. |
| Modal de detalles: fecha del baneo, actualizado el, baneado por | `Web/Views/ChatBans/Index.cshtml:115` | done |  |
| Modal de detalles: editar y guardar el motivo | `Web/Views/ChatBans/Index.cshtml:123`, `:245` | done |  |
| Modal de detalles: quitar con una segunda confirmación | `Web/Views/ChatBans/Index.cshtml:127`, `:260` | done |  |
| Modal de agregar: campo de Steam ID o URL del perfil | `Web/Views/ChatBans/Index.cshtml:153` | done |  |
| Modal de agregar: un Steam ID escrito muestra la foto y el nombre del jugador | `Web/Views/ChatBans/Index.cshtml:348-371` | done | La misma búsqueda de los ajustes del Anti-Cheat, `/api/admin/players/lookup`. |
| Modal de agregar: "Elegir de la lista" abre el selector de jugadores | `Web/Views/ChatBans/Index.cshtml:158`, `:334` | done |  |
| Modal de agregar: motivo | `Web/Views/ChatBans/Index.cshtml:165` | done |  |
| Modal de agregar: remitentes recientes con avatar, nombre, último mensaje y "Ya baneado" | `Web/Views/ChatBans/Index.cshtml:169`, `:284` | done |  |
| Mensajes de éxito y de error, y después recarga | `Web/Views/ChatBans/Index.cshtml:201` | done |  |
| Estilos (`_AdminStyles`, recorte del motivo, avatar redondo) | `Web/Views/ChatBans/Index.cshtml:16` | done |  |

## API y reglas de los baneos de chat

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `GET /api/chat-bans` (administrador) | `Web/Controllers/Api/ChatBansController.cs:16` | done |  |
| `GET /api/chat-bans/recent-senders` (administrador) | `Web/Controllers/Api/ChatBansController.cs:25` | done |  |
| `POST /api/chat-bans` crea o actualiza un baneo (administrador) | `Web/Controllers/Api/ChatBansController.cs:34` | adapted | Misma ruta y JSON. Necesita el token antiforgery de la página en el encabezado `RequestVerificationToken`; la API antigua no tenía protección contra solicitudes falsificadas. Los textos de error están traducidos. |
| `DELETE /api/chat-bans/{communityId}` (administrador) | `Web/Controllers/Api/ChatBansController.cs:51` | adapted | Misma ruta, con el mismo encabezado del token. |
| El jugador se indica como SteamID64, Steam ID, Steam3 o URL del perfil | `Core/ChatBans/Services/ChatBanService.cs:82` | done |  |
| Un baneo nuevo guarda quién lo creó y cuándo; una actualización los conserva y establece "actualizado el" | `Core/ChatBans/Services/ChatBanService.cs:89` | done |  |
| El nombre solo se reemplaza cuando se indica uno nuevo | `Core/ChatBans/Services/ChatBanService.cs:109` | done |  |
| Baneos listados del más reciente al más antiguo | `Core/ChatBans/Services/ChatBanService.cs:39` | done |  |
| Remitentes recientes: el último mensaje de cada visitante, del más reciente al más antiguo, 30 como máximo, marcados cuando ya están baneados | `Core/ChatBans/Services/ChatBanService.cs:52` | done |  |
| Un visitante baneado no puede enviar: HTTP 403 con el motivo | `Web/Controllers/Api/ExternalChatController.cs:32` | done |  |

## Páginas exclusivas del hub

Estas no existían en el sitio antiguo. Se listan para que en el port no se pierda nada de lo que el hub tenía.

| Elemento | Dónde en el hub | Estado | Notas |
|---|---|---|---|
| Selector de servidor en `/servers` cuando hay más de un servidor habilitado | `Views/Servers/Index.cshtml` | done | Paneles con el aspecto del sitio, actualizados cada 10 s con diffDOM. |
| Cambio de servidor en la página en vivo cuando hay más de un servidor habilitado | `Views/Servers/Live.cshtml` | done | Píldoras debajo del título. |
| Página de conexiones de jugadores (administrador) | `Views/PlayerConnections/Index.cshtml` | done | Adaptada a los estilos de administrador; la búsqueda acepta cualquier forma de escribir un Steam ID. |
| Página de comandos remotos con el registro de auditoría (administrador) | `Views/RemoteCommands/Index.cshtml` | done | Adaptada a los estilos de administrador. |
| JSON público de un servidor (`/api/servers/{publicId}/game-info...`) | `Controllers/Api/LiveApiController.cs` | done | Sin cambios. |

## Notas

- Totales: 154 `done`, 25 `adapted`, 1 `not ported`.
- Las tablas de jugadores antiguas no muestran ningún icono de patente: ni `Views/Server/_Players.cshtml` ni la página en producción lo tienen, así que ahí no hay nada que portar.
- Los estados se comprobaron frente a las páginas renderizadas: lado a lado con `https://l4d2.com.br/server` (escritorio, ancho de teléfono y tema claro), con una partida simulada que envía todos los eventos del juego, y manejando las páginas en un navegador headless (filtro del feed, envío de un mensaje, la terminal, el modal de información del servidor, todos los modales de la página de baneos de chat).
- El plugin del servidor de juego envía los slots del servidor en su heartbeat (`maxPlayers`) a partir de este cambio. Un servidor de juego que todavía ejecute el plugin anterior muestra como máximo el doble del tamaño del equipo.
