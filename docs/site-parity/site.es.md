# Paridad del sitio: layout, ranking, historial, patentes y Anti-Cheat

🌐 [English](site.md) | [Português](site.pt.md) | **Español**

Todos los elementos del sitio antiguo (`l4d2-playstats-web`) para estas pantallas, comprobados uno por uno frente al hub. `Dónde` apunta al archivo antiguo, relativo a `L4D2PlayStats.Web/L4D2PlayStats.Web`.

Estado: `done` (mismo contenido y aspecto), `adapted` (presente, cambiado por el motivo indicado), `not ported` (no puede existir en el hub, con el motivo).

## Layout (todas las páginas)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Metas de charset, compatibilidad con IE y viewport | Views/Shared/_Layout.cshtml:28-30 | done | |
| Meta description | _Layout.cshtml:31 | adapted | Mismo texto por página; el valor predeterminado nombra el sitio (`SiteSettings.Name`) en lugar del "L4D2 Competitive" fijo. |
| Meta keywords | _Layout.cshtml:32 | adapted | La lista fija pasó a ser el ajuste opcional `SiteSettings.MetaKeywords`. |
| Meta author | _Layout.cshtml:33 | adapted | El nombre del sitio en lugar del nombre fijo. |
| Meta robots (index o noindex según la página) | _Layout.cshtml:34 | done | |
| Enlace canónico | _Layout.cshtml:36 | adapted | Se construye a partir de la dirección pública de los ajustes, o de la solicitud cuando está vacía, en lugar del `https://l4d2.com.br` fijo. |
| Título, descripción, URL, tipo, nombre del sitio y locale de Open Graph | _Layout.cshtml:37-45 | done | |
| Imagen, URL segura y alt de Open Graph | _Layout.cshtml:41-43 | adapted | La imagen propia de la página, como antes. La imagen predeterminada era un archivo fijo en el almacenamiento antiguo; ahora es el logo del sitio cuando se ha subido uno, y las etiquetas se omiten cuando no hay ninguno. |
| Card, título, descripción, imagen y alt de Twitter | _Layout.cshtml:47-51 | done | `summary` en lugar de `summary_large_image` cuando no hay imagen. |
| Título como "Página \| Sitio" | _Layout.cshtml:53, PageMetadata/Infrastructure/PageMetadata.cs | done | |
| Favicon | _Layout.cshtml:55 | done | |
| Hoja de estilos y script de lity | _Layout.cshtml:56, 535 | done | |
| Hojas de estilos del tema oscuro (predeterminado) y del tema claro, elegidas por una cookie | _Layout.cshtml:58-65 | done | Cookie `hub_theme`. |
| Estilos de página: ancho del contenedor, color de los enlaces, navbar, logo, iconos del encabezado, utilidades (`vertical-align-middle`, `bold`, `line-break-anywhere`), ancho del tooltip, icono de copiar | _Layout.cshtml:67-121 | done | Copiados tal como estaban. |
| Estilos de la tarjeta de partida (`.match ...`) | _Layout.cshtml:123-143 | done | |
| `show-mobile` / `hide-mobile`, insignia del servidor, icono flotante, color de la fila "soy yo" según el tema | _Layout.cshtml:145-178 | done | |
| Columna `slim`, reglas para teléfonos de las tarjetas de partida y `table-responsive` con `data-label` | _Layout.cshtml:180-234 | done | |
| Padding reducido de la navbar entre 768 y 930 px | _Layout.cshtml:236-241 | done | |
| Estilos de la píldora de estrellas | _Layout.cshtml:243-278 | done | |
| Secciones `Styles` y `Scripts` | _Layout.cshtml:281, 536 | done | |
| `imgOnError` (imagen de reserva para un avatar vacío) | _Layout.cshtml:285-288 | done | |
| `copyTextToClipboard` con popover y mensaje de fallo | _Layout.cshtml:290-304 | done | |
| Botón que despliega la navbar en teléfonos | _Layout.cshtml:313-318 | done | |
| Logo que enlaza a la página de inicio | _Layout.cshtml:319-321 | adapted | El logo lo sube el administrador (era un archivo fijo con el nombre del sitio antiguo). Sin logo, el nombre del sitio se muestra como texto. |
| Insignia de jugadores en línea junto al logo, pulsante, que enlaza a la página del servidor | _Layout.cshtml:322-324 | done | Solo se muestra cuando el módulo de servidor en vivo está activado. |
| Menú: Ranking, Últimas partidas, Servidor, Skins, Anti-Cheat | _Layout.cshtml:329-343 | done | Cada elemento sigue a su módulo. Comunidad (nuevo) va justo después de Ranking. Cuando los elementos no caben en una línea, los últimos pasan solos a "Más". |
| Menú "Más": Jugadores, Patentes, Historial, con iconos y un separador | _Layout.cshtml:344-369 | done | |
| Menú "Admin" para administradores: Baneos del chat, Compras de skins, Precios de skins, con iconos | _Layout.cshtml:370-398 | done | Además, el área de Administración del hub y los ajustes del Anti-Cheat. |
| Píldora de estrellas con el saldo del jugador con sesión iniciada, tooltip y etiqueta | _Layout.cshtml:403-411, 647-681 | done | Lee `GET /api/shop/wallet`, guardado en el session storage como antes. |
| Avatar del jugador con sesión iniciada que enlaza a la página del jugador, con el nombre en teléfonos | _Layout.cshtml:412-419 | done | |
| Menú de ajustes: cambiar de tema | _Layout.cshtml:430-447, Controllers/HomeController.cs:42 | done | `/theme/{theme}`. |
| Menú de ajustes: English, Português, Español (el actual queda oculto) | _Layout.cshtml:449-475, 545-554, 690-703 | adapted | Enlaces simples (`/language?culture=`) en lugar de un POST por AJAX seguido de una recarga. Mismo resultado, y funciona sin script. |
| Menú de ajustes: iniciar sesión con Steam / cerrar sesión | _Layout.cshtml:477-494 | adapted | Mismos elementos, iconos y textos. Cerrar sesión es un `POST /logout` con el token antiforgery, enviado por un formulario oculto detrás del mismo enlace, en lugar del antiguo `GET /logout`: otro sitio ya no puede cerrar la sesión de un visitante con un enlace o una imagen. `returnUrl` sigue devolviendo al visitante a la misma página, solo con direcciones locales. |
| Enlaces del pie de página (repositorio del servidor, GitHub, Steam) | _Layout.cshtml:507-521 | adapted | Los tres enlaces fijos pasaron a ser el ajuste `SiteSettings.FooterLinks` (uno por línea). El valor predeterminado es un enlace al proyecto. |
| Icono flotante "conectar" con la dirección del servidor en el tooltip, oculto en teléfonos | _Layout.cshtml:524-529, Controllers/ConnectController.cs | adapted | La dirección viene del servidor de juego registrado que tiene más jugadores (`/connect`), no de un ajuste fijo. Se oculta cuando ningún servidor tiene dirección. |
| Scripts de jQuery, Bootstrap, diffDOM, Chart.js y lity | _Layout.cshtml:531-535 | done | |
| Jugadores en línea en el título de la pestaña del navegador ("🟢 5/8 \| ...") y la actualización cada 4 segundos | _Layout.cshtml:539-543, 585-608 | done | `GET /api/server` con los mismos campos (`players`, `max_players`). |
| Animación `pulsate` | _Layout.cshtml:556-583, 705-707 | done | |
| Gráficos creados a partir de `data-chart` cuando entran en la vista al desplazarse | _Layout.cshtml:610-641, 683-685, 713 | done | |
| Utilidad `isObject` | _Layout.cshtml:643-645 | done | |
| Tooltips en todos los `data-toggle="tooltip"` | _Layout.cshtml:688 | done | |
| Etiqueta de Google Analytics | _Layout.cshtml:717-723 | adapted | El ID fijo pasó a ser el ajuste opcional `SiteSettings.GoogleAnalyticsId`. |
| Minificación del HTML y compresión HTTP | Program.cs:9-16, 65 | adapted | Las respuestas se comprimen (Brotli/Gzip). El marcado no se minifica: no tiene efecto visible, y es una dependencia menos. |
| Advertencia en el registro por cada solicitud que termina en error, y la excepción en los fallos | Middlewares/ErrorLoggingMiddleware.cs | done | Mediante el logger de la aplicación; una instalación en Linux lo guarda en el journal del sistema en lugar de `logs/app.log`. Página no encontrada va al nivel debug; todo error del servidor va como error con la dirección original, también cuando la propia página de error falla o cuando una cancelación que no vino del visitante termina la solicitud. |
| Página de error para excepciones no controladas y para códigos como 404 | (nuevo) | adapted | Añadido del hub: el sitio antiguo respondía un 500 vacío en producción. Las páginas ahora muestran una página de error traducida en el diseño del sitio; la excepción se sigue registrando como arriba. La API, las páginas dentro del juego, los archivos estáticos y las solicitudes hechas por scripts mantienen el estado sin cuerpo, y las páginas dentro del juego reciben una línea de texto simple en caso de excepción. |
| JSON sin nulos y con los enums como texto | Program.cs:21-25 | done | |
| Inicio de sesión con Steam con una cookie deslizante de 30 días | Program.cs:28-39 | done | |
| Direcciones convencionales como `/Home/SetTheme`, `/Language/Change`, `/Account/Login` | Program.cs:67 | adapted | El hub usa direcciones explícitas (`/theme/...`, `/language`, `/login`, `POST /logout`). Las direcciones que la gente realmente abre (`/`, `/history`, `/patents`, `/players`, `/skins`, `/anti-cheat`) son las mismas. |
| `POST /api/cache/clear` | Controllers/Api/CacheController.cs | not ported | El sitio antiguo guardaba en caché lo que leía de una API separada y había que avisarle para que lo olvidara. En el hub el sitio y la API son una sola aplicación, y cada subida actualiza el ranking por sí sola. |

## Inicio: ranking (`/`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Metadatos de página: título "Ranking", descripción, imagen y nombre del primer jugador | PageMetadata/HomePageMetadata.cs | done | |
| Estilos de página: tamaño de las medallas, barra de progreso fina, color rojo de la barra, márgenes de la tabla y de los párrafos | Views/Home/Index.cshtml:18-46 | done | |
| Encabezado "Top 100 jugadores" | Index.cshtml:75 | done | |
| "Próximo reinicio en N día(s)" | Index.cshtml:78, Models/RankingPeriodModel.cs | adapted | Cuenta los días hasta el final de la temporada actual, sea cual sea su duración, en lugar de períodos fijos de dos meses. |
| Enlace "Ver rankings anteriores" | Index.cshtml:80 | done | |
| Tabla con franjas, hover, bordes y condensada | Index.cshtml:83 | done | |
| Columnas: #, Patente, Jugador (avatar y nombre), XP, Victorias, Derrotas, MVP de SI (veces), MVP de CI (veces), Porcentaje de victorias, Juegos, Rage quit | Index.cshtml:85-97 | done | |
| Columnas ocultas en teléfonos (XP, Derrotas, MVPs, Porcentaje de victorias, Juegos, Rage quit) | Index.cshtml:89-96 | done | |
| Se listan los 100 primeros, con los 50 primeros visibles y el botón "Mostrar más" para el resto | Index.cshtml:103, 109, 198-207, 50-57 | done | |
| Fila del jugador con sesión iniciada resaltada | Index.cshtml:109 | done | |
| Medallas para los tres primeros puestos con tooltip; "#N" para los demás | Index.cshtml:111-125 | done | |
| Imagen de la patente con el tooltip "Nivel NN - Nombre" | Index.cshtml:127-129 | done | |
| Avatar que enlaza a la página del jugador, con imagen de reserva | Index.cshtml:130-134 | done | Los avatares se descargan bajo demanda, también sin una clave de la API de Steam. |
| Nombre del jugador, acortado cuando es demasiado largo, que enlaza a la página del jugador | Index.cshtml:136 | done | |
| Etiquetas pulsantes "¡Subir de nivel!" / "¡Bajar de nivel!" con tooltip | Index.cshtml:137-148 | done | |
| Barra de progreso de XP: tipo y pulso según la situación, rayada en el nivel máximo, segundo segmento para la ganancia o la pérdida del último juego | Index.cshtml:150-157, Models/RankingModel.cs | done | |
| Tooltip del progreso: cambio de nivel, patente máxima, XP del último juego, próximo nivel, XP sobre el total, porcentaje | Models/RankingModel.cs:44-71 | done | |
| Victorias con el tooltip "Victorias - N% Porcentaje de victorias" | Index.cshtml:158-162 | done | |
| Derrotas, MVP de SI, MVP de CI, Porcentaje de victorias, Juegos, Rage quit, cada uno con su tooltip | Index.cshtml:163-192 | done | |
| Sin ranking todavía: los seis primeros del último ranking | Index.cshtml:60-65, Views/Home/_LastRankingTopSix.cshtml | done | |
| Sin ranking y sin historial: el mensaje "No existen datos en este momento..." | Index.cshtml:66-71 | done | |
| Seis primeros: trofeo, encabezado, período de referencia | _LastRankingTopSix.cshtml:36-61 | adapted | El texto del período cubre una temporada de cualquier duración. |
| Seis primeros: un panel por jugador (resaltado si es el jugador con sesión iniciada), etiqueta del puesto por color, nombre, avatar redondo, victorias y derrotas, etiquetas de porcentaje de victorias, MVP de SI, MVP de CI, juegos y rage quit | _LastRankingTopSix.cshtml:63-111 | done | |
| Seis primeros: botón "Ver ranking completo" | _LastRankingTopSix.cshtml:113-115 | done | Abre ese período en el historial. |
| Con el módulo de ranking desactivado | (nuevo) | adapted | Una página de bienvenida breve; el sitio antiguo no tenía módulos. |
| Selector de período | (nuevo) | adapted | Añadido del hub encima de la tabla, con el estilo de la página de historial. La temporada actual es la predeterminada; también ofrece las otras temporadas del año actual, un elemento por año y todo el tiempo. Un año y todo el tiempo muestran la misma tabla sin las columnas de patente y XP, que pertenecen a una temporada. |

## Historial (`/history/{id?}`)

> **Unido a la lista de jugadores (noviembre de 2026, a pedido del dueño del sitio).** La tabla del historial ahora es la **Vista general** de `/players`, que mantiene todas las columnas de abajo (posición con medallas, avatar, nombre, victorias con la ayuda del porcentaje de victorias, derrotas, juegos, premios de MVP de SI y de CI, porcentaje de victorias, rage quits) para todos los jugadores en lugar del top 100, con el mismo selector de período que el ranking (las temporadas del año actual, cada año y todo el tiempo). `/history`, `/history/{id}` y las direcciones `/ranking/...` redirigen permanentemente a `/players` con el mismo período, y la entrada Historial salió del menú Más. Las filas de abajo describen la página como fue portada.

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Metadatos de página: "Historial - período", descripción, imagen y nombre del ganador | PageMetadata/HistoryPageMetadata.cs | done | |
| Estilos de página | Views/History/Index.cshtml:13-32 | done | |
| Encabezado "Historial" | Index.cshtml:34 | done | |
| Selector de período que indica el período mostrado | Index.cshtml:36-85 | done | Todo el tiempo, cada año y las temporadas terminadas del año actual, como en el sitio antiguo. Una temporada de un año anterior se sigue abriendo por su dirección. |
| Nombres de los períodos: "De MM/YYYY a MM/YYYY", "Año de YYYY", "Todo el tiempo" | Index.cshtml:42-57 | done | |
| Período predeterminado: el año actual | Controllers/HistoryController.cs:26 | done | |
| Columnas de la tabla: #, Jugador (avatar y nombre), Victorias, Derrotas, Juegos, MVP de SI (veces), MVP de CI (veces), Porcentaje de victorias, Rage quit | Index.cshtml:87-101 | done | |
| Columnas ocultas en teléfonos | Index.cshtml:93-99 | done | |
| Top 100, fila del jugador con sesión iniciada resaltada, medallas, avatar, nombre acortado, tooltips en todos los números | Index.cshtml:103-171 | done | |
| Período desconocido: "Historial no encontrado" con un botón para volver al ranking, no indexado | Views/History/NoHistory.cshtml, PageMetadata/HistoryNotFoundPageMetadata.cs | done | |
| Direcciones `/ranking/season/{id}`, `/ranking/year/{year}`, `/ranking/all-time` | (primera versión del hub) | adapted | Redirigen al historial. |

## Patentes (`/patents`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Metadatos de página: título, descripción, imagen y nombre de la patente más alta | PageMetadata/PatentsPageMetadata.cs | adapted | La descripción nombra el sitio en lugar del nombre fijo. |
| Encabezado "Patentes" | Views/Patents/Index.cshtml:8 | done | |
| Panel "Entendiendo el sistema de patentes", primer párrafo | Index.cshtml:10-17 | adapted | Dice "temporada" y su duración en meses en lugar de los períodos fijos de dos meses. |
| Segundo párrafo con el XP de una victoria, una derrota, MVP SI, MVP CI y rage quit | Index.cshtml:18-20 | done | Valores tomados de los ajustes del ranking. |
| Tabla: Nivel, imagen con tooltip, descripción, experiencia requerida, de la más alta a la más baja | Index.cshtml:24-49 | done | |
| Panel "Desglose de puntos de XP" con las cinco líneas | Index.cshtml:51-64 | done | |

## Anti-Cheat (`/anti-cheat`)

La página pertenece al módulo Anti-Cheat, que está desactivado de forma predeterminada. Cuando el administrador lo activa, aparecen la página y su elemento del menú.

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Metadatos de página | PageMetadata/AntiCheatPageMetadata.cs | adapted | Nombra el sitio en lugar del nombre fijo. |
| Estilos de página (hero, tarjetas, pasos, maqueta de la ventana, FAQ, reglas para teléfonos) | Views/AntiCheat/Index.cshtml:16-306 | done | |
| Hero: antetítulo, título, texto introductorio | Index.cshtml:308-313 | adapted | El antetítulo nombra el sitio. |
| Botón de descarga | Index.cshtml:315-318 | adapted | La dirección viene del servicio del Anti-Cheat o del ajuste; se oculta cuando ninguno da una dirección segura. |
| Versión, tamaño, fecha de publicación | Index.cshtml:319-333 | done | Se consultan al servicio del Anti-Cheat (`/v1/client/version`), con caché de 5 minutos, o de 30 segundos tras un fallo. |
| "Verificar el archivo": SHA-256 con botón de copiar y el comando de PowerShell | Index.cshtml:334-366 | done | |
| Línea de requisitos | Index.cshtml:367-370 | done | |
| Maqueta de la ventana con las líneas del registro | Index.cshtml:372-398 | done | |
| "Cómo usarlo": tres pasos | Index.cshtml:400-415 | done | |
| "Qué muestra la ventana": cuatro estados con sus colores | Index.cshtml:417-428 | done | |
| "Problemas comunes": cinco preguntas que se abren y se cierran | Index.cshtml:430-447 | done | |
| "Sobre -insecure (sin VAC)" | Index.cshtml:449-458 | adapted | Nombra el sitio en lugar de la dirección fija. |
| Privacidad: qué se envía y qué no se envía nunca | Index.cshtml:459-484 | done | |
| Jugador que inició sesión: si su cuenta de Steam debe usar el Anti-Cheat | Index.cshtml:319-325, Controllers/AntiCheatController.cs:45-55 | done | Oculto para los visitantes que no iniciaron sesión. |
| Direcciones del servicio y de la descarga | Core/AppConsts.cs:15-16 | adapted | Ajustes en `/admin/anti-cheat` en lugar de direcciones fijas. |

## Ajustes del Anti-Cheat (`/admin/anti-cheat`)

El sitio antiguo añadió esta página con los modos de uso del plugin de anti-cheat 1.5.0. Aquí es el elemento Anti-Cheat del menú de administración, y conserva las direcciones de la página del Anti-Cheat debajo de las listas.

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Solo administradores | Controllers/AntiCheatAdminController.cs:11-12 | adapted | También necesita el módulo Anti-Cheat. |
| Dirección `/anti-cheat-admin` | Controllers/AntiCheatAdminController.cs:17 | adapted | La página está en `/admin/anti-cheat`; la dirección antigua redirige a ella, manteniendo la lista y la búsqueda. |
| Elemento de menú "Ajustes del Anti-Cheat" | Views/Shared/_Layout.cshtml:383-388 | adapted | En el grupo Anti-Cheat del menú lateral de administración en lugar del menú desplegable Admin. |
| Encabezado con el botón "Añadir jugador" | Views/AntiCheatAdmin/Index.cshtml:121-126 | done | |
| Texto introductorio | Index.cshtml:128 | done | |
| "Modo de uso": las dos opciones con su ayuda, la elegida resaltada | Index.cshtml:130-146 | done | |
| Quién cambió el modo por última vez, en el sitio o en el juego, y cuándo; o "Aún no guardado en el sitio" | Index.cshtml:147-157 | done | Fechas en la zona horaria de los ajustes del sitio. |
| "Guardar modo", habilitado solo cuando la opción difiere del modo guardado, luego la recarga | Index.cshtml:158-161, :343-361 | done | |
| Título y ayuda de la lista mostrada, enlace a la otra lista con su cantidad | Index.cshtml:164-168 | done | |
| Aviso cuando la lista mostrada no es la que está en uso, con un enlace de vuelta | Index.cshtml:170-176 | done | |
| Contador de jugadores en la lista | Index.cshtml:178-183 | done | |
| Búsqueda por nombre o Steam ID, con "Limpiar filtros" | Index.cshtml:185-195, Controllers/AntiCheatAdminController.cs:24-30 | done | |
| Elemento de la lista: avatar, nombre, Steam ID y SteamID64, fecha en que se añadió y "por <administrador> en el sitio / en el juego" | Index.cshtml:197-222 | done | El Steam ID se escribe `STEAM_1:...`, como en todo el hub. |
| Texto de lista vacía | Index.cshtml:200 | done | |
| Paginación, 20 por página, manteniendo la lista y la búsqueda | Index.cshtml:224, Controllers/AntiCheatAdminController.cs:15 | done | |
| Modal de detalles: avatar, nombre con enlace a la página del jugador, IDs, enlace al perfil de Steam, añadido el, añadido por | Index.cshtml:226-250, :363-382 | done | El nombre lleva al perfil de Steam cuando el módulo de clasificación está desactivado. |
| Modal de detalles: quitar con una segunda confirmación, luego la recarga | Index.cshtml:251-260, :384-406 | done | |
| Modal de añadir, con la lista en el título: campo de Steam ID o URL del perfil | Index.cshtml:265-279 | done | |
| Modal de añadir: vista previa del jugador escrito con foto, nombre, IDs y "Jugador del sitio" o "Perfil de Steam (nunca jugó aquí)" | Index.cshtml:285, :408-476 | done | |
| Modal de añadir: "Elegir jugador" abre el selector de jugadores | Index.cshtml:280-283, :478-496 | done | |
| Modal de añadir: "Este jugador ya está en esta lista." y Guardar deshabilitado | Index.cshtml:425-436 | done | |
| Mensajes de éxito y de error, luego la recarga | Index.cshtml:498-520 | done | |
| Estilos (`_AdminStyles`, tarjetas de los modos, encabezado de la lista, vista previa, avatar redondo) | Index.cshtml:24-118 | done | |
| Panel "Página del Anti-Cheat" con las direcciones del servicio y de la descarga | nuevo | done | La página del hub que ya existía, conservada debajo de las listas. |

## API de acceso al Anti-Cheat y reglas

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `GET /api/anti-cheat-access` (administrador) | Controllers/Api/AntiCheatAccessController.cs:20 | adapted | Los mismos datos; el modo se escribe `everyone` o `listed`. |
| `PUT /api/anti-cheat-access/mode` (administrador) | Controllers/Api/AntiCheatAccessController.cs:28 | adapted | Misma ruta y mismo JSON. Necesita el token antifalsificación de la página en el encabezado `RequestVerificationToken`. Los textos de error se traducen. |
| `POST /api/anti-cheat-access/{whitelist o required}` (administrador) | Controllers/Api/AntiCheatAccessController.cs:41 | adapted | Misma ruta y mismo JSON, con el mismo encabezado de token. El jugador se indica como SteamID64, Steam ID, Steam3 o URL del perfil. |
| `DELETE /api/anti-cheat-access/{list}/{communityId}` (administrador) | Controllers/Api/AntiCheatAccessController.cs:61 | adapted | Misma ruta, con el mismo encabezado de token. |
| `GET /api/admin/players/lookup?id=` (administrador) | Controllers/Api/AdminPlayersApiController.cs, Core/Players/Services/PlayerLookupService.cs | adapted | El nombre viene de la clasificación, luego de los perfiles de los jugadores y de las conexiones de los servidores de juego, si no de Steam con la clave del sitio o el perfil público, guardado 4 horas. La foto es la copia del sitio, o la foto de Steam hasta que el sitio tenga una. |
| `GET /api/anti-cheat-access/server` (servidor de juego) | Controllers/Api/AntiCheatAccessController.cs:74 | adapted | Autorizado con el token de un servidor de juego registrado, el valor de `l4d2_web_secret`, en lugar del secreto único del sitio. Mismo JSON; `mode` es `null` hasta que se guarde un modo. |
| `PUT /api/anti-cheat-access/server/mode` (servidor de juego) | Controllers/Api/AntiCheatAccessController.cs:81 | adapted | El mismo token de arriba. Mismo JSON. |
| `POST /api/anti-cheat-access/server/{list}` y `DELETE /api/anti-cheat-access/server/{list}?player=` (servidor de juego) | Controllers/Api/AntiCheatAccessController.cs:93, :112 | adapted | El mismo token de arriba. Mismo JSON, con el Steam ID escrito `STEAM_1:...`. |
| El modo y las dos listas en sus propias tablas, con quién los cambió y desde dónde | Core/AntiCheatAccess/Repositories | adapted | Tablas `AntiCheatAccessSettings`, `AntiCheatWhitelist` y `AntiCheatRequired` en la base de datos del sitio en lugar de tablas de Azure con caché de 12 horas. |
| Nombres cortados en 64 caracteres; añadir un jugador que ya está en la lista mantiene la primera entrada | Core/AntiCheatAccess/Services/AntiCheatAccessService.cs:73-111, :184 | done | |
| Un cambio desde el juego registra "en el juego" y el administrador, `Console` por defecto | Controllers/Api/AntiCheatAccessController.cs:126-136 | done | |
| Un cambio en el sitio pone en cola `sm_aac sync`, nunca dos veces | Core/AntiCheatAccess/Services/AntiCheatAccessService.cs:170-182 | adapted | En cola para cada servidor de juego habilitado en lugar del único servidor, escrito en el registro de comandos remotos y entregado incluso con el módulo de comandos remotos desactivado. Necesita el módulo de servidor en vivo, cuyo plugin busca los comandos. Los cambios desde el juego no lo ponen en cola. |
