# Paridad del sitio: jugadores

🌐 [English](players.md) | [Português](players.pt.md) | **Español**

El área de jugadores del sitio antiguo (`Views/Players/*`, `PlayersController`, `Api/PlayersController`, `Api/PunishmentsController`, los modelos del jugador y los metadatos de página), portada al hub.

Las rutas de la segunda columna son relativas a `L4D2PlayStats.Web/L4D2PlayStats.Web` en el repositorio antiguo, salvo que empiecen por `Core/` o `Sdk/` (los otros proyectos del sitio antiguo) o por `api/` (el repositorio de la API antigua).

Estado: `done` (mismo contenido y aspecto), `adapted` (presente, cambiado por el motivo indicado), `not ported` (no puede existir en el hub).

Comprobado frente a las páginas renderizadas con una copia de los datos de producción y frente a https://l4d2.com.br (`/players`, `/player/76561199157667941`, `/player/76561199157667941/76561199429077716`, `/player/76561190000000001`), a 1280 px y con ancho de teléfono, en inglés, en el tema oscuro y (solo el HTML) en el tema claro. El número de filas, celdas, tooltips, botones de copiar, gráficos, modales, tarjetas de relaciones y tarjetas de campañas del HTML antiguo y del nuevo es el mismo para los mismos datos.

## Rutas y API

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `GET /players` (lista) | Controllers/PlayersController.cs:26-39 | done | Misma dirección. |
| Parámetro de consulta `orderBy` (una de 25 estadísticas, `Experience` de forma predeterminada) | Controllers/PlayersController.cs:27, Core/Players/Enums/PlayerResultProperty.cs | done | Mismos nombres y orden (`Models/Players/PlayerResultProperty.cs`). Un valor desconocido recurre a `Experience`. |
| Parámetro de consulta `asc` (`false` de forma predeterminada) | Controllers/PlayersController.cs:27 | done | |
| Elemento del menú "Jugadores" (dentro de "Más") activo en la lista | Controllers/PlayersController.cs:29 | done | `ViewBag.Players = "active"`; no se establece en la página del jugador, como antes. |
| `GET /player/{communityId}/{compareWith?}` (jugador y comparación) | Controllers/PlayersController.cs:41-97 | done | Misma dirección. |
| `communityId <= 0` muestra la página "fuera del ranking" | Controllers/PlayersController.cs:48-49 | done | |
| Un jugador que no está en el ranking muestra la página "fuera del ranking" con la información de conexión (HTTP 200) | Controllers/PlayersController.cs:55-64 | done | La primera versión del hub respondía 404 aquí. |
| Un `compareWith` que no está en el ranking se ignora | Controllers/PlayersController.cs:53 | done | |
| El ranking de la temporada actual como fuente de las dos páginas | Controllers/PlayersController.cs:31, :51, Core/Ranking/Services/RankingServiceCached.cs:11-19 | adapted | La llamada a la API antigua devolvía solo los 100 primeros jugadores del ranking, así que la lista se detenía en 100 y el jugador 101 recibía la página "fuera del ranking". El hub usa el ranking completo de la temporada. |
| Parámetro de consulta `season` (nuevo) | - | adapted | Añadido del hub: las dos páginas aceptan `?season={id}` para mostrar una temporada pasada. Una temporada desconocida responde 404. Sin él se muestra la temporada actual, como antes. |
| Avatares cargados antes de renderizar | Controllers/PlayersController.cs:33, :60-61, :66-69, :85 | adapted | Los avatares los sirve `/avatar/{communityId}` y se descargan bajo demanda; no hace falta ninguna llamada de precarga. |
| Últimas partidas: las 50 partidas más recientes de la temporada, de cualquier modo de juego, filtradas a aquellas en las que alguno de los dos jugadores está en un equipo | Controllers/PlayersController.cs:77, Core/Matches/MatchesServiceCached.cs:19, Models/PlayerDetailsModel.cs:22-32 | done | Misma regla, sobre la temporada mostrada y todos los servidores de juego (`IPlayerMatchService`, `AppConsts.PlayerRecentMatches`). |
| Cuentas relacionadas a partir de la información de conexión | Controllers/PlayersController.cs:78, Core/GameInfo/Services/PlayerConnectionInfoService.cs:75-108 | adapted | `IPlayerConnectionService.RelatedAsync`. Vacío cuando el módulo de servidor en vivo está desactivado, y para los visitantes que no son administradores cuando Configuración > Privacidad limita las cuentas relacionadas a los administradores (el valor predeterminado es todos, como antes). |
| Skins adquiridas y monedero de cada jugador | Controllers/PlayersController.cs:87-93, :99-106 | done | Solo se leen cuando el módulo de la tienda de skins está activado. |
| Progreso de patente de cada jugador | Controllers/PlayersController.cs:71-75 | done | `RankingModel` / `PatentProgressModel` de la base. |
| `POST /api/punishments` (administrador, cuerpo `{ communityId, lostExperiencePoints }`) | Controllers/Api/PunishmentsController.cs:15-24, api Core/Modules/Punishments/Services/PunishmentsService.cs:14-31 | adapted | Misma dirección, verbo y cuerpo; los puntos se suman a lo que el jugador ya había perdido, como antes. El cuerpo acepta también `seasonId` (la temporada mostrada; la actual cuando falta) y la llamada exige el token antiforgery en el encabezado `RequestVerificationToken`. La respuesta lleva el total del jugador en la temporada. |
| `DELETE /api/punishments/{communityId}` (administrador) | Controllers/Api/PunishmentsController.cs:26-35 | adapted | Misma dirección y verbo, respuesta 204. Acepta `?season={id}` y el encabezado antiforgery. |
| API de sanciones: solo administradores, con sesión iniciada | Controllers/Api/PunishmentsController.cs:11, :16, :27 | done | 401 para un visitante, 403 para un jugador. |
| API de sanciones: se limpian las cachés para que el ranking muestre la nueva experiencia | Controllers/Api/PunishmentsController.cs:21, :32 | done | `HubState.DataChanged()` invalida los rankings en caché. |
| API de sanciones: validación del comando | api Core/Modules/Punishments/Validations/PunishmentCommandValidator.cs | done | 400 para un jugador no válido o para puntos que no son positivos. |
| `GET /api/players/patents` | Controllers/Api/PlayersController.cs:15-31 | done | Ya está en el hub (`RankingApiController`); estas páginas no lo llaman. |
| `GET /api/players/skins`, `GET /api/players/{communityId}/wallet`, `POST /api/players/{communityId}/skins/equip` | Controllers/Api/PlayersController.cs:33-69 | done | Ya están en el hub (`PlayerSkinsApiController`); son endpoints de los plugins y del layout, que estas páginas no llaman. |
| Ranking en caché durante una hora | Core/Ranking/Services/RankingServiceCached.cs:13-18 | adapted | El hub guarda en memoria el ranking calculado y lo descarta cada vez que cambian los datos, así que las páginas nunca van una hora por detrás. |

## Lista de jugadores (`/players`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título "Jugadores" | Views/Players/Index.cshtml:63 | done | |
| Estilos de página (márgenes de la tabla, de los párrafos y del encabezado) | Views/Players/Index.cshtml:47-61 | done | |
| Desplegable "Ordenar por: **estadística**" con el caret | Views/Players/Index.cshtml:67-71 | done | |
| Elementos del desplegable: las 25 estadísticas, cada una con enlace a `orderBy={statistic}&asc=False` | Views/Players/Index.cshtml:72-81 | done | El enlace conserva la temporada mostrada. |
| Nombres de las estadísticas: Experiencia, Juegos, Victorias, Derrotas, Rage quit, MVP de SI (veces), MVP de CI (veces), LVP de FF (veces), Muertes, Veces incapacitado, Daño recibido de SI, Commons muertos, SI muertos, Daño a SI, Daño al Tank, Rocas recibidas, Skeets totales, Melee skeets, Levels totales, Crowns totales, FF causado (sin incap.), Daño de SI (sin Tank), Daño como Tank (en pie), Daño de ácido (en pie), Daño total de pounce | Views/Players/Index.cshtml:15-27, Resources/SharedResource*.resx | done | `Helpers/PlayerProperties.cs`. |
| Selector de período (nuevo) | - | adapted | Añadido del hub, junto al desplegable "Ordenar por" y con el estilo de la página de historial: "Período mostrado: **De MM/YYYY a MM/YYYY**". Lista las temporadas del año actual y después un elemento por año ("Año de YYYY"), que muestra el ranking y las partidas de todo ese año, y "Todo el tiempo". Solo se muestra cuando hay más de una opción. |
| Tabla `table-striped table-hover table-bordered table-condensed` | Views/Players/Index.cshtml:86 | done | |
| Columna `#` (estrecha, centrada) | Views/Players/Index.cshtml:89, :113 | done | |
| Columna "Jugador" sobre dos celdas (avatar y nombre) | Views/Players/Index.cshtml:90 | done | |
| Columna con la estadística elegida, que enlaza al orden invertido, con la flecha (↑ ascendente, ↓ descendente) | Views/Players/Index.cshtml:91-97 | done | |
| Columna "Por juego", oculta cuando la estadística es "Juegos" | Views/Players/Index.cshtml:44, :98-101, :131-139 | done | |
| Posición: 1, 2, 3... en orden descendente; cuenta hacia atrás desde el número de jugadores en orden ascendente | Views/Players/Index.cshtml:105-107, :142 | done | |
| Páginas de 100 jugadores (nuevo) | - | adapted | Añadido del hub: la lista muestra 100 jugadores por página con los enlaces de las páginas debajo de la tabla, que mantienen `orderBy`, `asc` y el período. La posición sigue la de la página anterior, en los dos órdenes. Con hasta 100 jugadores no hay enlaces de página y la página queda como antes. |
| Fila resaltada (`it-is-me`) para el jugador con sesión iniciada | Views/Players/Index.cshtml:112 | done | |
| Avatar de 40 px, redondeado, con enlace, título y la imagen de reserva en caso de error | Views/Players/Index.cshtml:114-118 | done | |
| Nombre truncado (`TruncatePlayerName`) con enlace a la página del jugador | Views/Players/Index.cshtml:119-121 | done | El enlace conserva la temporada mostrada. |
| Valor de la estadística (`N0`) | Views/Players/Index.cshtml:124 | done | |
| Icono de ayuda con tooltip en el valor para "Daño recibido de SI", "Daño de SI (sin Tank)" y "Daño total de pounce" | Views/Players/Index.cshtml:29-38, :125-128 | done | Los mismos tres textos; `aria-label` "Más información". |
| Promedio por juego (`N2`, `-` sin juegos) | Views/Players/Index.cshtml:135, Core/Players/Player.cs:26 | done | |
| Icono de información junto al promedio con el tooltip "Juegos: n" | Views/Players/Index.cshtml:136 | done | |
| Orden: por valor, los empates por community id, en ambos sentidos | Core/Players/Extensions/PlayerResultExtensions.cs:13-15 | done | `PlayerProperties.Sort`; cubierto por una prueba. |
| Metadatos de página: título "Jugadores" | PageMetadata/PlayersPageMetadata.cs:14 | done | |
| Metadatos de página: descripción "Consulta las estadísticas de {n} jugadores del servidor ..." | PageMetadata/PlayersPageMetadata.cs:15 | adapted | El texto nombraba "L4D2 Competitive"; ahora toma el nombre del sitio (clave nueva en los fragmentos de recursos `players`). |
| Metadatos de página: imagen y texto alternativo del primer jugador de la lista | PageMetadata/PlayersPageMetadata.cs:12, :16-17 | done | |

## Página del jugador (`/player/{communityId}/{compareWith?}`): marco

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título "Detalles del jugador" | Views/Players/Details.cshtml:584 | done | |
| Selector de período (nuevo) | - | adapted | Añadido del hub debajo del título, el mismo desplegable que en la lista. Conserva el jugador comparado y la pestaña abierta. |
| Columna izquierda (`col-xs-2 left-side`): desplegable con el nombre del jugador (10 caracteres) y el caret | Views/Players/Details.cshtml:587-592 | done | |
| Lista del desplegable izquierdo: todos los demás jugadores del ranking como "#posición - nombre", que cambia el primer jugador y conserva la comparación | Views/Players/Details.cshtml:593-600 | done | Lista desplazable de 175 px (`players-dropdown-menu`). |
| Avatar del primer jugador (`img-thumbnail`, se abre ampliado con lity) | Views/Players/Details.cshtml:603 | done | |
| Línea separadora | Views/Players/Details.cshtml:605 | done | |
| Segundo desplegable en amarillo: el nombre del jugador comparado o "Comparar..." | Views/Players/Details.cshtml:607-611 | done | |
| Segundo desplegable: "Quitar comparación" con el icono de quitar y un separador, al comparar | Views/Players/Details.cshtml:613-621 | done | |
| Lista del segundo desplegable: todos los demás jugadores del ranking en amarillo, para elegir el jugador con el que comparar | Views/Players/Details.cshtml:622-629 | done | |
| Avatar del segundo jugador con el marco amarillo, se abre ampliado | Views/Players/Details.cshtml:632-635 | done | |
| Pestañas: General, Campañas (insignia con el número), Skins (insignia con las skins de ambos jugadores), Relaciones (insignia, solo cuando hay relaciones) | Views/Players/Details.cshtml:639-661 | adapted | La pestaña Skins solo está mientras el módulo de la tienda de skins está activado. El resto es igual. |
| Estilos de la página (skins, insignias, encabezados, progreso, altura del desplegable, `second` amarillo, separador, nombres anteriores, mosaicos de rachas, pestañas, tarjetas y barras de campañas) | Views/Players/Details.cshtml:187-500 | done | Copiados tal como estaban; se añadió una regla para una skin sin miniatura. |
| Móvil (máximo 700 px): pestañas más pequeñas, las dos columnas a todo el ancho, filas `mobile-row-label` del cara a cara mostradas como etiquetas en bloque | Views/Players/Details.cshtml:476-499 | done | |
| Tablas como `table-responsive` con `data-label` en cada celda (la etiqueta antes del valor en un teléfono) | Views/Players/Details.cshtml:671-1233 | done | |
| Script: abrir una pestaña escribe su hash en la dirección y dispara un resize (para que se carguen los gráficos diferidos) | Views/Players/Details.cshtml:549-554 | done | |
| Script: la pestaña nombrada en el hash de la dirección se abre al cargar | Views/Players/Details.cshtml:556-557 | done | |
| Script: los enlaces de los desplegables de jugadores y de las tarjetas de relaciones conservan la pestaña abierta | Views/Players/Details.cshtml:559-562 | done | El selector de período hace lo mismo. |
| Metadatos de página: título "{name} - Detalles del jugador", o "{first} vs {second}" al comparar | PageMetadata/PlayerDetailsPageMetadata.cs:16-18 | done | |
| Metadatos de página: descripción "Consulta las estadísticas de {name}: posición #{n}, {n} partidas y {n} victorias." | PageMetadata/PlayerDetailsPageMetadata.cs:20-21 | done | |
| Metadatos de página: descripción de una comparación "Compara el rendimiento y las estadísticas de {first} y {second} en ..." | PageMetadata/PlayerDetailsPageMetadata.cs:22 | adapted | El texto nombraba "L4D2 Competitive"; ahora toma el nombre del sitio. |
| Metadatos de página: avatar del primer jugador como imagen, nombre como texto alternativo, tipo de Open Graph `profile` | PageMetadata/PlayerDetailsPageMetadata.cs:24-29 | done | |

## Página del jugador: pestaña General, "Estadísticas generales"

Cada valor va seguido de ` | ` y del valor del segundo jugador en amarillo al comparar, como en la página antigua.

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Panel "Estadísticas generales" | Views/Players/Details.cshtml:666-669 | done | |
| Ranking (`#position`) | Views/Players/Details.cshtml:674-682 | done | |
| Steam ID con el icono de copiar y el popover "¡Texto copiado!" | Views/Players/Details.cshtml:684-700 | done | Mismo campo, icono (oscuro o claro según el tema) y comportamiento, escrito como `STEAM_0:x:y` igual que en el sitio antiguo. |
| Nombre (el segundo jugador en una línea nueva) | Views/Players/Details.cshtml:703-711 | done | El nombre más usado (`MainName`), como antes. |
| URL del perfil como enlace en una pestaña nueva, con el botón de copiar | Views/Players/Details.cshtml:713-733 | done | |
| Nombres anteriores: hasta 5 etiquetas con tooltip; las del segundo jugador en etiquetas amarillas después del separador; la fila solo existe cuando el primer jugador tiene nombres | Views/Players/Details.cshtml:735-754 | done | |
| Patente: imagen de 32 px con el tooltip "{name} - Nivel nn - patente" (fila oculta en un teléfono) | Views/Players/Details.cshtml:755-764 | done | |
| XP: barra de progreso en dos partes (progreso y último cambio) con el tooltip HTML (subida o bajada de nivel, último juego, próximo nivel, XP, progreso); una segunda barra para el jugador comparado | Views/Players/Details.cshtml:766-784, Models/RankingModel.cs | done | |
| Juegos | Views/Players/Details.cshtml:787-795 | done | |
| Victorias | Views/Players/Details.cshtml:796-804 | done | |
| Rage quit | Views/Players/Details.cshtml:805-813 | done | |
| Porcentaje de victorias (`P0`) | Views/Players/Details.cshtml:816-824 | done | |
| Derrotas | Views/Players/Details.cshtml:825-833 | done | |
| Castigo ("0" o "-n XP") | Views/Players/Details.cshtml:35-38, :834-836, :848 | done | |
| Administrador: botón "Editar" junto a cada castigo (tooltip abajo) | Views/Players/Details.cshtml:838-843, :850-855 | done | |
| Administrador: el botón pregunta "¿Cuántos puntos de experiencia debería perder el usuario?" con 150 como sugerencia | Views/Players/Details.cshtml:530-531, :840 | done | |
| Administrador: un número positivo añade la sanción, 0 la quita, cualquier otra cosa no hace nada; después la página se recarga | Views/Players/Details.cshtml:505-546 | adapted | Mismo comportamiento. Las llamadas envían también la temporada mostrada y el token antiforgery. |
| Racha de victorias más larga | Views/Players/Details.cshtml:860-868 | done | |
| Racha de derrotas más larga | Views/Players/Details.cshtml:869-877 | done | |
| Historial de partidas: un mosaico por resultado (victoria verde, derrota roja, empate gris) con tooltip; un guion cuando está vacío; con los nombres encima de cada fila al comparar | Views/Players/Details.cshtml:40-75, :880-887 | done | |
| Comparación: fila "Jugando juntos" (juegos, victorias con su porcentaje, derrotas con su porcentaje), solo cuando jugaron juntos | Views/Players/Details.cshtml:889-909 | done | |
| Comparación: fila "Jugando en contra" (juegos, victorias y derrotas de cada jugador con sus porcentajes, el segundo jugador en amarillo), solo cuando se enfrentaron | Views/Players/Details.cshtml:911-937 | done | |

## Página del jugador: pestaña General, "Cuentas relacionadas"

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Panel (`panel-warning`) solo cuando otra cuenta se conectó desde la misma dirección | Views/Players/Details.cshtml:943-948 | adapted | El mismo panel. De forma predeterminada se muestra a todos los visitantes, como antes; el ajuste Configuración > Privacidad > "Quién puede ver las cuentas relacionadas" puede limitarlo a los administradores. Necesita el módulo de servidor en vivo, que es donde se registran las conexiones. |
| Columnas: Nombre, Steam ID, Último acceso, Conexiones, URL del perfil | Views/Players/Details.cshtml:951-959 | done | |
| Avatar de 32 px y nombre que enlazan a la página de esa cuenta | Views/Players/Details.cshtml:964-967 | done | |
| Steam ID con el botón de copiar | Views/Players/Details.cshtml:968-974 | done | Formato `STEAM_0:x:y`, como arriba. |
| Último acceso (formato `g`) | Views/Players/Details.cshtml:975 | adapted | El hub guarda el momento en UTC y lo muestra en la zona horaria del sitio; el sitio antiguo mostraba la hora local de su servidor. |
| Número de conexiones | Views/Players/Details.cshtml:976 | done | |
| URL del perfil como enlace (pestaña nueva, `noopener noreferrer`) con el botón de copiar | Views/Players/Details.cshtml:977-985 | done | |

## Página del jugador: pestaña General, "Estadísticas como superviviente" y "Estadísticas como infectado"

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Panel "Estadísticas como superviviente" | Views/Players/Details.cshtml:993-996 | done | |
| MVP de SI (veces), MVP de CI (veces), LVP de FF (veces) | Views/Players/Details.cshtml:999-1027 | done | |
| SI muertos, Commons muertos, Daño a SI | Views/Players/Details.cshtml:1029-1057 | done | |
| Muertes, Veces incapacitado, Daño recibido de SI (con el icono de ayuda y el tooltip) | Views/Players/Details.cshtml:1059-1090 | done | |
| Daño al Tank, Rocas recibidas, FF causado (sin incap.) | Views/Players/Details.cshtml:1092-1121 | done | |
| Skeets totales, Melee skeets, Levels totales | Views/Players/Details.cshtml:1123-1153 | done | |
| Crowns totales, fila solo cuando alguno de los dos jugadores tiene crowns | Views/Players/Details.cshtml:1155-1172 | done | |
| Panel "Estadísticas como infectado" | Views/Players/Details.cshtml:1177-1180 | done | |
| Daño de SI (sin Tank) con el icono de ayuda y el tooltip; Daño como Tank (en pie) | Views/Players/Details.cshtml:1183-1206 | done | |
| Daño de ácido (en pie); Daño total de pounce con el icono de ayuda y el tooltip | Views/Players/Details.cshtml:1208-1231 | done | |

## Página del jugador: pestaña General, "Últimas partidas"

La tarjeta es la antigua `Views/LastMatches/_Match.cshtml`. La página del jugador usa la tarjeta de partidas compartida, `Views/Matches/_Match.cshtml`, la misma de la lista de partidas; ya no tiene su propia copia.

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título "Últimas partidas" y la lista, solo cuando hay partidas | Views/Players/Details.cshtml:1236-1246 | done | |
| Encabezado de la tarjeta: campaña, modo de juego en cursiva, etiqueta "Competitiva" (verde) o "Casual" (gris) | Views/LastMatches/_Match.cshtml:157-171 | adapted | "Competitiva" es una partida que cuenta para el ranking según las reglas de modos de juego del hub; la regla antigua era fija: dos o más mapas y equipos de cuatro. |
| "El equipo X ganó por n puntos" | Views/LastMatches/_Match.cshtml:172-174 | done | |
| Icono de calendario con "Hoy a las", "Ayer a las" o la fecha y la hora, y su tooltip | Views/LastMatches/_Match.cshtml:176-193 | done | Se muestra en la zona horaria del sitio. |
| Icono de reloj con la duración (`hh:mm:ss`) y su tooltip | Views/LastMatches/_Match.cshtml:195-201 | done | |
| Portada de la campaña (se abre ampliada) | Views/LastMatches/_Match.cshtml:208, Core/Campaign/CampaignThumb.cs | done | Las mismas imágenes y nombres de archivo; una campaña sin portada recurre a `no-image.jpg`. |
| Botón "Detalles" | Views/LastMatches/_Match.cshtml:209-214 | adapted | Enlaza a la página de la partida del hub, `/matches/{id}`. |
| Dos paneles de equipo (verde el ganador, rojo el perdedor) con "Equipo X" y la puntuación | Views/LastMatches/_Match.cshtml:217-232 | done | |
| Jugadores de cada equipo ordenados por MVPs, con avatar, nombre con enlace y la fila resaltada para el jugador con sesión iniciada | Views/LastMatches/_Match.cshtml:233-250 | done | Los enlaces conservan la temporada mostrada. |
| Estrella amarilla por cada MVP de SI y estrella azul por cada MVP de CI, con tooltips, después de un separador | Views/LastMatches/_Match.cshtml:251-264 | done | |
| Gráfico "Marcador" (líneas): una serie por equipo con la puntuación acumulada por mapa, verde para el ganador y rojo para el perdedor, leyenda debajo | Views/LastMatches/_Match.cshtml:24-78, :278-289 | done | La misma configuración de Chart.js, con carga diferida mediante `.app-chart` + `data-chart`; color de la fuente según el tema. |
| Gráfico "Puntos ganados por mapa" (barras): una serie por equipo con los puntos de cada mapa | Views/LastMatches/_Match.cshtml:80-153, :291-302 | done | |
| Nombres de los mapas en el eje truncados a 16 caracteres | Views/LastMatches/_Match.cshtml:31, :85 | done | |
| Móvil: la portada y los equipos se apilan a todo el ancho | Views/Shared/_Layout.cshtml (`.match .left-side`, `.match .right-side`) | done | Reglas del layout portado. |

## Página del jugador: pestaña Campañas

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| "Todavía no hay estadísticas de campañas para este jugador." cuando está vacía | Views/Players/Details.cshtml:1250-1253 | done | |
| Línea de ayuda con el icono de información | Views/Players/Details.cshtml:1256-1258 | done | |
| Cuadrícula de tarjetas de campañas, la más jugada primero (después por los juegos del primer jugador, después por nombre); las campañas sin nombre se omiten | Views/Players/Details.cshtml:1259-1283, Models/PlayerDetailsModel.cs:104-120 | done | |
| Tarjeta: portada, nombre de la campaña, "n Juegos" a la derecha (solo con un único jugador) | Views/Players/Details.cshtml:1262-1271 | done | |
| Por jugador: barra en tres partes (victorias, empates, derrotas) con tooltips | Views/Players/Details.cshtml:77-80, :101-105 | done | |
| Por jugador: tres recuadros con victorias, empates y derrotas, cada uno con su porcentaje | Views/Players/Details.cshtml:106-119 | done | |
| Por jugador: "Rage quit: n" cuando hay alguno | Views/Players/Details.cshtml:120-123 | done | |
| Comparación: el nombre de cada jugador con "· n Juegos" encima de la barra; "Aún no la jugó" para el que no tiene juegos | Views/Players/Details.cshtml:85-98, :1273-1279 | done | |
| Juegos y porcentajes de victorias, derrotas y empates de una campaña | Sdk/Ranking/Results/PlayerCampaignResult.cs:9-18 | done | Añadidos al `PlayerCampaign` del hub. |

## Página del jugador: pestaña Skins (módulo de la tienda de skins)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título del panel "Skins" con una insignia por jugador (amarilla para el segundo) | Views/Players/Details.cshtml:1288-1295 | done | |
| Enlace "Ver la tienda de skins" | Views/Players/Details.cshtml:1296 | done | |
| Por jugador: nombre con el número de skins (al comparar) | Views/Players/Details.cshtml:148-156 | done | |
| Por jugador: píldora del monedero "★ n estrella(s) disponible(s) · n ganadas, n gastadas" | Views/Players/Details.cshtml:128-141, :157 | done | |
| "Este jugador todavía no tiene skins." | Views/Players/Details.cshtml:160-163 | done | |
| Skins adquiridas, las más raras y caras primero: miniatura, título localizado, enlace a la skin en la tienda (`/skins#skin-{id}`) | Views/Players/Details.cshtml:166-181, Models/PlayerSkinsModel.cs:8-14 | adapted | Las miniaturas vienen de la dirección configurada para los assets de las skins (no se incluyen con el hub); sin ella se muestra un recuadro gris del mismo tamaño. |
| Marco verde y etiqueta "Equipada" en las skins equipadas | Views/Players/Details.cshtml:170-178 | done | |

## Página del jugador: pestaña Relaciones

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título "Relaciones con jugadores" con los nombres de los jugadores | Views/Players/Details.cshtml:1383-1394 | done | |
| Una tarjeta por cada jugador con el que coincidió, el jugador comparado primero, después por juegos y por victorias | Views/Players/Details.cshtml:1400-1403, Models/PlayerDetailsModel.cs:55-69, :71-96 | done | |
| Tarjeta en amarillo (`panel-warning`) para el jugador comparado | Views/Players/Details.cshtml:1403 | done | |
| Encabezado de la tarjeta: nombre (enlace que cambia la página a ese jugador, conservando la comparación, cuando está en el ranking), "n Juegos" | Views/Players/Details.cshtml:1408-1416 | adapted | Un jugador que no está en el ranking se mostraba por el community id; el hub muestra el último nombre conocido cuando lo tiene. |
| Botón de ayuda (signo de interrogación, tooltip "No entiendo, explícamelo") | Views/Players/Details.cshtml:1405-1407 | done | |
| Avatar | Views/Players/Details.cshtml:1420-1422 | done | |
| Tabla: "Jugando juntos" y "Jugando en contra" con Juegos, Victorias y Derrotas, cantidades y porcentajes con tooltips, una columna por jugador al comparar (el segundo en amarillo) | Views/Players/Details.cshtml:1313-1335, :1424-1455 | done | |
| Modal de ayuda por tarjeta: "Entendiendo los números", las tres frases con los nombres en color y los números en negrita, separadas por una línea para el segundo jugador, "Cerrar" | Views/Players/Details.cshtml:1337-1373, :1460-1493 | done | |
| Solo 12 tarjetas al principio; el botón "Ver más jugadores" / "Ocultar" alterna el resto | Views/Players/Details.cshtml:564-579, :1402, :1498-1507, Core/AppConsts.cs:7 | done | `AppConsts.PlayerRelationsVisible`. |

## Página de jugador fuera del ranking (`Views/Players/PlayerNotFound.cshtml`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título "Detalles del jugador" | Views/Players/PlayerNotFound.cshtml:36 | done | |
| Cuadro de advertencia: "Jugador fuera de la clasificación" y las dos líneas de explicación | Views/Players/PlayerNotFound.cshtml:38-42 | done | |
| Selector de período (nuevo) | - | adapted | Añadido del hub: un jugador que no está en el ranking de la temporada mostrada puede estarlo en otra. |
| Avatar (184 px como máximo, se abre ampliado, imagen de reserva) | Views/Players/PlayerNotFound.cshtml:45-47 | done | |
| Panel "Información disponible" | Views/Players/PlayerNotFound.cshtml:49-52 | done | |
| Nombre, cuando se conoce | Views/Players/PlayerNotFound.cshtml:56-62 | adapted | Tomado de la información de conexión, como antes, o del nombre que el hub registró en una partida pasada. |
| Community ID con el botón de copiar | Views/Players/PlayerNotFound.cshtml:63-69 | done | |
| Steam ID con el botón de copiar | Views/Players/PlayerNotFound.cshtml:70-79 | done | Formato `STEAM_0:x:y`, como en la página del jugador. |
| Último acceso, cuando se conoce | Views/Players/PlayerNotFound.cshtml:80-86 | adapted | Zona horaria del sitio, como arriba. |
| Conexiones, cuando se conocen | Views/Players/PlayerNotFound.cshtml:87-93 | done | |
| URL del perfil (pestaña nueva) con el botón de copiar | Views/Players/PlayerNotFound.cshtml:94-105 | done | |
| Panel "Cuentas relacionadas" con las mismas cinco columnas que en la página del jugador | Views/Players/PlayerNotFound.cshtml:112-160 | adapted | Con el módulo de servidor en vivo, y siguiendo el mismo ajuste de privacidad que la página del jugador. |
| Botón "Volver a la página de clasificación" | Views/Players/PlayerNotFound.cshtml:162-164 | done | |
| Estilos (tamaño del avatar, celdas de encabezado, margen en teléfonos) | Views/Players/PlayerNotFound.cshtml:15-34 | done | |
| Metadatos de página: título "{name or id} - Detalles del jugador" | PageMetadata/PlayerNotFoundPageMetadata.cs:14 | done | |
| Metadatos de página: descripción "Consulta la información disponible sobre el jugador {name} en ..." | PageMetadata/PlayerNotFoundPageMetadata.cs:15 | adapted | El texto nombraba "L4D2 Competitive"; ahora toma el nombre del sitio. |
| Metadatos de página: avatar como imagen, tipo de Open Graph `profile`, no indexada | PageMetadata/PlayerNotFoundPageMetadata.cs:16-19 | done | |

## Modelos y datos

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `PlayersModel` (jugadores, orden, sentido) | Models/PlayersModel.cs | done | `Models/Players/PlayersModel.cs`, más el período mostrado. |
| Fila `Player` de la lista (valor por estadística, juegos, promedio) | Core/Players/Player.cs | done | `Models/Players/PlayerListRow.cs` (el nombre `Player` ya lo usa el paquete de estadísticas). |
| `PlayerDetailsModel` (rankings, jugadores, últimas partidas, cara a cara, relaciones, campañas, skins, cuentas relacionadas) | Models/PlayerDetailsModel.cs | done | `Models/Players/PlayerDetailsModel.cs`. |
| `PlayerRelationModel`, `PlayerRelationRowModel` | Models/PlayerRelationModel.cs, Models/PlayerRelationRowModel.cs | done | |
| `PlayerCampaignRowModel` | Models/PlayerCampaignRowModel.cs | done | |
| `PlayerSkinsModel` | Models/PlayerSkinsModel.cs | done | |
| `PlayerNotFoundModel` | Models/PlayerNotFoundModel.cs | done | |
| `PlayerPatentModel` | Models/PlayerPatentModel.cs | done | Forma de `GET /api/players/patents`, que ya está en el hub. |
| `PlayerResult` y sus estadísticas, nombres, relaciones, campañas, resultados y rachas | Sdk/Ranking/Results/*.cs | done | `RankingPlayer` de la base. |
| `PunishmentCommand`, `PunishmentResult` | Sdk/Punishments/* | adapted | `Models/Players/PunishmentCommand.cs`, con el `seasonId` opcional. La respuesta conserva `communityId` y `lostExperiencePoints`; `server` dio paso a `seasonId`, porque en el hub una sanción pertenece a una temporada y no a un servidor de juego. |
| `PlayerStarsResult` (estrellas de un jugador) | Sdk/Stars/* | done | El monedero que se muestra en la pestaña Skins (`SkinWallet`). |
