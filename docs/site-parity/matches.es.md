# Paridad del sitio: partidas

🌐 [English](matches.md) | [Português](matches.pt.md) | **Español**

El área de partidas del sitio antiguo (`Views/LastMatches/*`, `LastMatchesController`, `Api/StatisticsController`, `Api/CampaignController`), portada al hub.

Las rutas de la segunda columna son relativas a `L4D2PlayStats.Web/L4D2PlayStats.Web` en el repositorio antiguo, salvo que empiecen por `Sdk/`, `Core/` o `api/` (el repositorio de la API antigua).

Estado: `done` (mismo contenido y aspecto), `adapted` (presente, cambiado por el motivo indicado), `not ported` (no puede existir en el hub).

Los textos se citan en español, tal como los mostraba el sitio antiguo con el idioma español seleccionado.

Cómo se comprobó: la página de detalles y la página de estadísticas de una partida real (The Parish, 12 nov 2026) se renderizaron en el hub a partir de una copia de los archivos de estadísticas de producción y se compararon con las mismas páginas de https://l4d2.com.br. Se contó cada elemento (etiqueta, clases CSS y la presencia de `title`, `alt`, `style`, `data-toggle`, `data-label`, `data-placement`, `colspan`, `role`, `tabindex`, `aria-label`) y cada texto visible en ambos lados: 803 elementos y 549 textos en la página de detalles, 1.420 elementos y 865 textos en la página de estadísticas, sin ninguna diferencia. La lista se comparó mediante capturas de pantalla, en anchos de escritorio y de teléfono.

## Rutas y API

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `GET /last-matches` (lista) | Controllers/LastMatchesController.cs:21 | adapted | La ruta del hub es `/matches`. `/last-matches` responde con una redirección permanente hacia ella. |
| `GET /last-matches/details/{start}/{end}` (partida) | Controllers/LastMatchesController.cs:32 | adapted | La ruta del hub es `/matches/{id}`. La dirección antigua se sigue respondiendo: las dos claves se convierten de nuevo en fechas de archivo, se encuentra la partida y se redirige al visitante de forma permanente. |
| `GET /last-matches/details/{start}/{end}/statistics/{statisticId}` (mapa) | Controllers/LastMatchesController.cs:47 | adapted | La ruta del hub es `/matches/{id}/statistics/{fileId}`. La dirección antigua redirige a ella de forma permanente. |
| La dirección de una partida sigue funcionando con el tiempo | Sdk/Matches/Results/MatchResult.cs:16-17 | done | La dirección antigua estaba hecha de claves de archivo, que nunca cambian. En el hub una partida recibía un id nuevo cada vez que se reconstruían las partidas de su noche (un mapa nuevo, una corrección de marcador, una reconstrucción completa); ahora la reconstrucción conserva el id de una partida que sigue existiendo. |
| 404 cuando la partida no existe | Controllers/LastMatchesController.cs:36, :52 | done | |
| 404 cuando el archivo de estadísticas no forma parte de la partida | Controllers/LastMatchesController.cs:57 | done | |
| Elemento del menú "Últimos partidos" activo solo en la lista | Controllers/LastMatchesController.cs:23 | done | `ViewBag.LastMatches = "active"` en la lista, no en las páginas de detalles, como antes. |
| Lista limitada a las últimas 50 partidas, de cualquier modo de juego | Core/Matches/MatchesServiceCached.cs:19, api FunctionApp/Functions/MatchesFunction.cs:21 | adapted | 50 partidas por página, las más recientes primero, de cualquier modo de juego, con paginación y todas las temporadas; la página antigua se detenía en 50 y solo mostraba la temporada actual. |
| Lista en caché durante una hora | Core/Matches/MatchesServiceCached.cs:15 | not ported | El hub lee las partidas de su propia base de datos, así que no hay ninguna llamada a una API remota que guardar en caché; la lista está siempre al día. |
| Avatares cargados antes de renderizar | Controllers/LastMatchesController.cs:68-87 | adapted | Los avatares los sirve `/avatar/{communityId}` y se descargan bajo demanda; no hace falta ninguna llamada de precarga. |
| `PUT /api/statistics/{statisticId}/update-score` (administrador, cuerpo `{ team, score }`) | Controllers/Api/StatisticsController.cs:18-45, Commands/UpdateTeamScoreCommand.cs | adapted | Misma dirección, verbo y cuerpo. El id es el id de archivo del hub. La solicitud debe llevar además el token antiforgery en el encabezado `RequestVerificationToken`, y la respuesta lleva el id del archivo, el id de la partida y las dos puntuaciones en lugar del archivo de estadísticas completo. |
| update-score: 404 para un archivo desconocido | Controllers/Api/StatisticsController.cs:23-24 | done | |
| update-score: 400 cuando el archivo no tiene puntuación | Controllers/Api/StatisticsController.cs:26-30 | done | |
| update-score: solo cambia el equipo indicado, la otra puntuación se conserva | Controllers/Api/StatisticsController.cs:32-33 | adapted | Misma regla. Un equipo que no sea A o B se responde con 400; el endpoint antiguo guardaba el archivo otra vez sin cambiarlo. |
| update-score: puntuación entre 0 y 10.000 | api Core/Modules/Statistics/Validations/UpdateScoreCommandValidator.cs | done | Se responde con 400. |
| update-score: se limpian las cachés para que la partida y el ranking muestren la nueva puntuación | Controllers/Api/StatisticsController.cs:42 | done | El archivo guardado se reescribe, las partidas a su alrededor se reconstruyen y los rankings en caché se invalidan. |
| update-score: solo administradores | Controllers/Api/StatisticsController.cs:14, :19 | done | A un visitante se le envía a iniciar sesión y un jugador con sesión iniciada recibe 403. |
| `GET /api/campaigns` (nombre y mapas de cada campaña) | Controllers/Api/CampaignController.cs:10-13, Core/Campaign/CampaignName.cs:164-180 | done | Lee las campañas registradas en el hub (la misma lista de forma predeterminada, editable por el administrador). |

## Números de la partida (API antigua)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Equipos A y B: los jugadores de cada mitad del último mapa, en el orden de la lista de jugadores del archivo | api Core/Modules/Statistics/Extensions/StatisticsExtensions.cs:53-54 | done | |
| Nombre del jugador tomado del último mapa | api Core/Modules/Matches/Match.cs:163 | done | |
| Puntuación del equipo: la puntuación del último mapa | api Core/Modules/Matches/Match.cs:65 | done | |
| Totales por jugador sobre todos los mapas y las dos mitades (muertes, incaps, daño recibido, commons, SI muertos, daño a SI, daño al tank, rocas recibidas, daño a la witch, skeets incluidos los incompletos y los de melee, melee skeets, levels incluidos los incompletos, crowns incluidas las incompletas, FF causado, daño como infectado, daño causado como tank, daño de ácido, daño de high pounce) | api Core/Modules/Matches/Match.cs:104-146 | done | |
| MVP (SI), MVP (CI) y LVP (FF) contados una vez por mitad | api Core/Modules/Matches/Match.cs:110-134 | done | |
| Totales del equipo: la suma de los jugadores del equipo | api Core/Modules/Matches/Match.cs:69-94 | done | |
| Porcentajes: el valor del jugador dividido por el total del equipo, cero cuando el total es cero | api Core/Modules/Matches/Match.cs:166-218 | done | |
| Ganador: el equipo con la puntuación más alta; diferencia; empate | Sdk/Matches/Results/MatchResult.cs:19-29 | done | |
| Inicio de la partida (inicio del primer mapa), fin (fin del último mapa) y tiempo transcurrido | api Core/Modules/Statistics/Extensions/StatisticsExtensions.cs:58-65, Match.cs:11-13 | done | |
| Horas mostradas en la hora local del sitio | Sdk/Converters/DateTimeConverter.cs:10 | adapted | El sitio antiguo usaba la zona horaria de la máquina; el hub usa la zona horaria elegida en los ajustes del sitio. |
| Mapas de la partida, el más antiguo primero, con la puntuación después de cada uno | api Core/Modules/Matches/Match.cs:42-47, :218-223 | done | |
| "Competitiva": dos mapas o más y equipos de cuatro | api Core/Modules/Matches/Match.cs:31 | adapted | El hub marca como competitivas las partidas que cuentan para el ranking, según los modos de juego que registró el administrador (de forma predeterminada, la misma regla). |
| Nombre de la configuración de la partida | api Core/Modules/Matches/Match.cs:9 | done | |
| Archivos de estadísticas de la partida: todos los archivos del servidor entre el primer y el último mapa, el más antiguo primero | api FunctionApp/Functions/StatisticsFunction.cs:64-84 | done | Un mapa repetido que la partida ignora se sigue listando, como antes. |

## Lista de últimas partidas (`Views/LastMatches/Index.cshtml`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título de página "Últimos partidos" | PageMetadata/LastMatchesPageMetadata.cs:17 | done | |
| Meta description "Consulta los últimos partidos de {site}, sus marcadores, campañas y jugadores." | PageMetadata/LastMatchesPageMetadata.cs:18 | adapted | El texto nombraba "L4D2 Competitive"; ahora toma el nombre del sitio (texto nuevo en `pt-BR.matches.json` y `es-ES.matches.json`). |
| Imagen social: miniatura de la campaña de la última partida, texto alternativo con su nombre | PageMetadata/LastMatchesPageMetadata.cs:19-20 | done | |
| Título `h4.text-center` "Últimos partidos" | Views/LastMatches/Index.cshtml:13 | done | |
| `div.match` con una tarjeta por partida | Views/LastMatches/Index.cshtml:15-20 | done | |
| Filtro de temporada | (nuevo) | adapted | Añadido del hub, con el aspecto antiguo: un select pequeño a la derecha, encima de la lista ("Todas las temporadas" de forma predeterminada, después las temporadas del año actual y un elemento por año), para que las temporadas pasadas se puedan seguir consultando. Se oculta mientras hay una sola temporada y un solo servidor. |
| Filtro de servidor | (nuevo) | adapted | Añadido del hub: un segundo select, que solo se muestra cuando hay más de un servidor de juego. |
| Paginación | (nuevo) | adapted | Añadido del hub, con el parcial compartido `_Pagination`, que se muestra cuando hay más de una página. |
| Lista vacía | Views/LastMatches/Index.cshtml:15-20 | adapted | La página antigua no mostraba nada; el hub muestra "No se encontró ningún partido" con el mismo estilo de título. |

## Tarjeta de partida (`Views/LastMatches/_Match.cshtml`)

La tarjeta es `Views/Matches/_Match.cshtml` con el modelo `MatchCardModel`; `IMatchViewService.CardsAsync(filter)` construye las tarjetas para cualquier página (la antigua página del jugador también renderizaba este parcial).

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| `panel panel-primary` con un padding del encabezado de `6px 10px` | _Match.cshtml:160-161 | done | |
| Nombre de la campaña en un `h4` en negrita | _Match.cshtml:164-166 | adapted | Una partida en mapas de ninguna campaña registrada (solo posible en el hub) muestra "Campaña personalizada". |
| Nombre de la configuración en un `small` en cursiva | _Match.cshtml:167 | done | |
| Nombre del servidor | (nuevo) | adapted | Añadido del hub: un segundo `small` en cursiva después del nombre de la configuración, solo cuando hay más de un servidor de juego. |
| Etiqueta "Competitiva" (`label-success`) o "Casual" (`label-default`) | _Match.cshtml:168-175 | done | |
| "El equipo {0} ganó por {1:N0} puntos" | _Match.cshtml:176-178 | done | |
| Icono de calendario e inicio: "Hoy a las {t}", "Ayer a las {t}" o "{d} a las {t}" | _Match.cshtml:181-197 | done | Hoy es el día actual en la zona horaria del sitio. |
| Tooltip "Fecha y hora en que ocurrió el juego" | _Match.cshtml:181 | done | |
| Icono de reloj y duración `hh:mm:ss`, solo cuando la partida tiene fin | _Match.cshtml:199-205 | done | |
| Tooltip "Duración del juego en horas" | _Match.cshtml:201 | done | |
| Imagen de la campaña (`img-responsive img-thumbnail img-campaign`, alt y title con la campaña) que se abre en un lightbox (`data-lity`) | _Match.cshtml:212 | done | |
| Una campaña sin imagen recurre a `no-image.jpg` | Core/Campaign/CampaignThumb.cs:7-56 | adapted | El sitio antiguo tenía una lista fija de campañas con imagen; el hub comprueba si el archivo de imagen existe (mismos nombres de archivo), así que una campaña añadida por el administrador recibe su imagen en cuanto el archivo está ahí. |
| Botón "Detalles" (`btn btn-danger btn-xs btn-block`), oculto en las páginas de detalles | _Match.cshtml:213-218, Models/MatchDetailsModel.cs:12 | done | |
| Dos paneles de equipo, uno al lado del otro (`col-xs-6`), verde para el ganador y rojo para el perdedor | _Match.cshtml:221-225 | done | |
| Encabezado del equipo "Equipo A/B" y puntuación (`N0`) a la derecha | _Match.cshtml:226-235 | done | |
| Jugadores ordenados por MVPs, después por MVP (SI), después por MVP (CI), después por menos LVP | _Match.cshtml:239-244 | done | |
| Fila resaltada (`it-is-me`) para el jugador con sesión iniciada | _Match.cshtml:246 | done | |
| Avatar de 32px con esquinas redondeadas, enlace al jugador, nombre en alt y title, imagen de reserva en caso de error | _Match.cshtml:247-251 | done | |
| Nombre del jugador truncado que enlaza a la página del jugador, "-" cuando se desconoce | _Match.cshtml:253 | done | |
| Separador de barra vertical después del nombre cuando el jugador tiene algún MVP | _Match.cshtml:254-257 | done | |
| Una estrella dorada por cada MVP (SI) con el tooltip "MVP (SI)" | _Match.cshtml:259-262 | done | |
| Una estrella azul por cada MVP (CI) con el tooltip "MVP (CI)" | _Match.cshtml:264-267 | done | |
| Línea horizontal entre los equipos y los gráficos | _Match.cshtml:281 | done | |
| Gráfico de líneas "Marcador": una serie por equipo ("Equipo A", "Equipo B"), puntuación después de cada mapa, verde para el ganador y rojo para el perdedor, nombres de los mapas cortados a 16 caracteres, leyenda abajo, eje que empieza en cero | _Match.cshtml:26-79, :284-294 | done | |
| Gráfico de barras "Puntos ganados por mapa": una serie por equipo con los puntos de cada mapa (diferencia respecto al mapa anterior) | _Match.cshtml:81-157, :296-306 | done | |
| El color del texto de los gráficos sigue el tema (`#222222` claro, `#c8c8c8` oscuro) | _Match.cshtml:18-24 | done | La cookie del tema es la del hub (`hub_theme`). |
| Gráficos dibujados solo cuando entran en la vista (`canvas.app-chart` con `data-chart`), de 200px de alto | _Match.cshtml:285-304 | done | |
| Móvil: la columna de la imagen y la columna de los equipos ocupan todo el ancho (`left-side`, `right-side`); gráficos apilados (`col-sm-6 col-xs-12`) | _Match.cshtml:211, :220, :284, :296 | done | |

## Detalles de la partida (`Views/LastMatches/Details.cshtml`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título de página "Detalles del partido - {campaign}" | PageMetadata/MatchDetailsPageMetadata.cs:18 | done | |
| Meta description "Consulta los detalles del partido en {campaign}, jugado el {date}, con marcador {A}–{B}." | PageMetadata/MatchDetailsPageMetadata.cs:19 | done | |
| Imagen social: miniatura de la campaña, texto alternativo con la campaña | PageMetadata/MatchDetailsPageMetadata.cs:20-21 | done | |
| Estilo `.maps .label { margin: 0 3px; }` | Details.cshtml:17-23 | done | |
| "No se encontró ningún partido" cuando la partida no tiene archivos de estadísticas | Details.cshtml:62-72 | done | |
| Título "Detalles del partido" | Details.cshtml:74 | done | |
| Migas de pan: "Últimos partidos" (enlace a la lista) / campaña | Details.cshtml:76-83 | done | |
| Tarjeta de partida sin el botón "Detalles" | Details.cshtml:85-87 | done | |
| Panel "Mapas" con el texto "Resultados del mapa" | Details.cshtml:90-96 | done | |
| Columnas de la tabla de mapas: #, Mapa, Inicio, Fin, Tiempo transcurrido, Marcador (tres celdas) | Details.cshtml:97-105 | done | |
| `#n` y el nombre del mapa enlazan a la página de estadísticas del mapa | Details.cshtml:117-126 | done | |
| Inicio y fin como fecha corta más `HH:mm`; tiempo transcurrido como `hh:mm:ss` | Details.cshtml:127-129 | done | |
| Etiqueta de la puntuación del equipo A (verde el ganador, rojo el perdedor, gris el empate) con el tooltip "Puntuación del equipo A" | Details.cshtml:113, :130-133 | done | |
| Etiqueta de la diferencia de puntuación con el tooltip "Diferencia de puntuación" | Details.cshtml:143-147 | done | |
| Etiqueta de la puntuación del equipo B con el tooltip "Puntuación del equipo B" | Details.cshtml:114, :148-151 | done | |
| Administrador: botón de enlace "Editar" (tooltip "Editar") junto a la puntuación de cada equipo | Details.cshtml:134-141, :152-159 | done | |
| `editScore`: prompt "Puntuación del equipo X" con el valor actual, ignora una entrada vacía o no numérica, alerta "Rango de puntuación no válido" fuera de 0–10.000, `PUT /api/statistics/{id}/update-score`, y después muestra la página de nuevo | Details.cshtml:25-60 | adapted | Mismo flujo. El script solo se envía a los administradores y la solicitud lleva el token antiforgery. Después de guardar, la página abre la dirección de la partida indicada en la respuesta (la misma página, ya que la partida conserva su id; la lista cuando la corrección no deja ninguna partida). |
| Móvil: la tabla se convierte en filas con etiqueta (`table-responsive`, `data-label` en cada celda) | Details.cshtml:97, :117-148 | done | |
| Por equipo, panel "Equipo X - Superviviente" con el texto "Estadísticas del equipo X jugando como supervivientes" | Details.cshtml:168-174 | done | |
| Encabezado de la tabla de supervivientes: un par de columnas por jugador (nombre truncado que enlaza al jugador), "Total" | Details.cshtml:177-188 | done | |
| Segunda fila del encabezado: "Valor" y "% del equipo" debajo de cada jugador | Details.cshtml:189-201 | done | |
| Fila "MVP de SI (veces)": valor, proporción, total | Details.cshtml:204-216 | done | |
| Fila "MVP de CI (veces)" | Details.cshtml:218-230 | done | |
| Fila "LVP de FF (veces)" | Details.cshtml:232-244 | done | |
| Fila "Muertes" | Details.cshtml:246-258 | done | |
| Fila "Veces incapacitado" | Details.cshtml:260-272 | done | |
| Fila "Daño recibido de SI" con el icono de información y su tooltip | Details.cshtml:274-289 | done | |
| Fila "Commons muertos" | Details.cshtml:291-303 | done | |
| Fila "SI muertos" | Details.cshtml:305-317 | done | |
| Fila "SI - Daño infligido" | Details.cshtml:319-331 | done | |
| Fila "Daño al Tank" | Details.cshtml:333-345 | done | |
| Fila "Daño a la Witch" | Details.cshtml:347-359 | done | |
| Fila "Rocas recibidas" | Details.cshtml:361-373 | done | |
| Fila "Skeets totales" | Details.cshtml:375-387 | done | |
| Fila "Melee skeets" | Details.cshtml:389-401 | done | |
| Fila "Levels totales" | Details.cshtml:403-415 | done | |
| Fila "Crowns" | Details.cshtml:417-429 | done | |
| Fila "FF causado (sin incap.)" | Details.cshtml:431-443 | done | |
| Valores como `N0`, proporciones como `P0` | Details.cshtml:209-212 (y todas las filas) | done | |
| Móvil: celdas de proporción ocultas (`hide-mobile`), celdas de valor etiquetadas con el nombre del jugador, total etiquetado "Total" | Details.cshtml:208-215 (y todas las filas) | done | |
| Por equipo, panel "Equipo X - Infectado" con el texto "Estadísticas del equipo X jugando como infectados" | Details.cshtml:449-455 | done | |
| Filas del encabezado de la tabla de infectados (jugadores, "Valor" / "% del equipo", "Total") | Details.cshtml:457-483 | done | |
| Fila "Daño de SI (sin Tank)" con icono de información y tooltip | Details.cshtml:485-500 | done | |
| Fila "Daño como Tank (en pie)" | Details.cshtml:502-514 | done | |
| Fila "Daño de ácido (en pie)" | Details.cshtml:516-528 | done | |
| Fila "Daño total de pounce" con icono de información y tooltip | Details.cshtml:530-545 | done | |

## Estadísticas del mapa (`Views/LastMatches/Statistics.cshtml`)

| Elemento | Dónde en el sitio antiguo | Estado | Notas |
|---|---|---|---|
| Título de página "{map} - {campaign}" | PageMetadata/MatchStatisticsPageMetadata.cs:19 | done | |
| Meta description "Consulta las estadísticas del mapa {map}, del partido en {campaign}, con marcador {A}–{B}." | PageMetadata/MatchStatisticsPageMetadata.cs:20 | done | |
| Imagen social: miniatura de la campaña, texto alternativo con la campaña | PageMetadata/MatchStatisticsPageMetadata.cs:21-22 | done | |
| Estilos: paddings laterales de `.round-details` a partir de 992px, ajuste de línea de `.player-name` | Statistics.cshtml:21-38 | done | |
| "No se encontró ningún partido" cuando la partida no tiene archivos de estadísticas | Statistics.cshtml:183-193 | done | |
| Título "Detalles del partido" | Statistics.cshtml:202 | done | |
| Migas de pan: "Últimos partidos" / campaña (enlace a la partida) / nombre del mapa | Statistics.cshtml:204-214 | done | |
| Tabla de mapas (#, Mapa, Inicio, Fin, Tiempo transcurrido, Marcador) con el mapa actual resaltado (`tr.info`) y enlaces a cada mapa | Statistics.cshtml:216-271 | done | |
| Etiquetas de puntuación del equipo A / diferencia / equipo B con tooltips | Statistics.cshtml:251-265 | done | Aquí no hay botón "Editar", como antes. |
| Pestañas "1º ronda" y "2º ronda", una por mitad, la primera activa | Statistics.cshtml:273-282 | done | |
| Panel de pestaña `fade`, el primero `in active` | Statistics.cshtml:309 | done | |
| Panel "Supervivientes": verde cuando ese equipo ganó el mapa, rojo en caso contrario | Statistics.cshtml:292, :314-317 | done | |
| Lista de supervivientes: avatar, nombre truncado que enlaza al jugador, resaltado `it-is-me` | Statistics.cshtml:321-331 | done | |
| Supervivientes ordenados con el MVP (SI) primero, el MVP (CI) segundo y el LVP último | Statistics.cshtml:295-303 | done | |
| Estrella dorada para el MVP (SI), estrella azul para el MVP (CI), icono de LVP para quien causó más fuego amigo, cada uno con su tooltip | Statistics.cshtml:332-345 | done | |
| Panel "Infectados": verde cuando ese equipo ganó el mapa, rojo en caso contrario | Statistics.cshtml:293, :355-358 | done | |
| Lista de infectados ordenada por daño: avatar, nombre, daño (`N0`) con el tooltip "Daño de SI (sin Tank)", resaltado `it-is-me` | Statistics.cshtml:305-307, :362-379 | done | |
| Panel "Detalles", tabla izquierda: todos los contadores de la ronda distintos de cero (reinicios, pastillas, botiquines, desfibriladores, commons, SI muertos, daño a SI, SI aparecidos, witches, tanks, incaps, muertes, FF) | Statistics.cshtml:388-407 | done | |
| Panel "Detalles", tabla derecha: inicio, fin y duración de la ronda; inicio, fin y duración de la pausa; inicio, fin y duración del tank (cada uno solo cuando existe) | Statistics.cshtml:409-476 | done | |
| Panel "Supervivientes" (tabla de estadísticas): encabezado con el nombre truncado de cada jugador, que enlaza al jugador | Statistics.cshtml:488-507 | done | |
| Filas mostradas solo cuando algún jugador tiene un valor; filas separadoras en blanco entre grupos | Statistics.cshtml:61-128 | done | |
| Grupo: muertes, incaps, daño recibido de SI (con tooltip de información), daño por caída | Statistics.cshtml:510-517 | done | |
| Por arma (escopeta, SMG/rifle, sniper, pistola): disparos, impactos, % de impactos, impactos a SI, % de impactos a SI, impactos al tank, % de impactos al tank, headshots, % de headshots, headshots a SI, % de headshots a SI | Statistics.cshtml:519-634 | done | |
| Grupo desde commons muertos hasta daño a SI durante el tank | Statistics.cshtml:637 | done | |
| Grupo desde daño al tank hasta rocas recibidas | Statistics.cshtml:638 | done | |
| Grupo desde skeets hasta despejes propios, después despejes y tiempo medio de despeje (segundos, dos decimales) | Statistics.cshtml:89-92, :639-640 | done | |
| Fuego amigo causado (por tipo, a sí mismo, total) y recibido (por tipo, total) | Statistics.cshtml:643-671 | done | |
| Panel "Infectados" (tabla de estadísticas): encabezado con el nombre de cada jugador; grupos de daño, tank, arañazo/ácido/boom (con tooltips de información), de pounces a ledged, commons, apariciones por clase | Statistics.cshtml:677-727 | done | |
| Panel "Jugadores conectados": #, Steam ID, Community ID, Nombre | Statistics.cshtml:735-748 | done | |
| Botón de copiar junto al Steam ID y al Community ID | Statistics.cshtml:755-768 | done | |
| Nombre del jugador que enlaza a la página del jugador | Statistics.cshtml:769-773 | done | |
| Números como `N0`, porcentajes como `P0` | Statistics.cshtml:95, :557 | done | |
