# Hoja de ruta

🌐 [English](roadmap.md) | [Português](roadmap.pt.md) | **Español**

El plan para traer el sistema L4D2 PlayStats a este repositorio: qué existe hoy, el diseño objetivo, las decisiones ya tomadas, las fases y lo que sigue abierto.

## Contenido

- [Situación actual](#situación-actual)
- [Diseño objetivo](#diseño-objetivo)
- [Decisiones](#decisiones)
- [Módulos](#módulos)
- [Servidores de juego](#servidores-de-juego)
- [Temporadas y rankings](#temporadas-y-rankings)
- [Modos de juego](#modos-de-juego)
- [Ranking de la comunidad](#ranking-de-la-comunidad)
- [Tienda de skins de armas](#tienda-de-skins-de-armas)
- [Asistente de configuración y estado del servidor](#asistente-de-configuración-y-estado-del-servidor)
- [Front-end](#front-end)
- [Pruebas](#pruebas)
- [Despliegue](#despliegue)
- [Migración de datos](#migración-de-datos)
- [Fases](#fases)
- [Preguntas abiertas](#preguntas-abiertas)

## Situación actual

| Pieza | Repositorio | Cómo funciona hoy |
|---|---|---|
| Analizador de estadísticas | [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | Paquete NuGet que lee los archivos que escribe `l4d2_playstats`. |
| API | [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | Azure Functions. Guarda los archivos sin procesar en Azure Table Storage y, a partir de ellos, reconstruye las partidas y el ranking en cada solicitud. |
| Sitio web | [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | ASP.NET Core MVC. Lee de la API, guarda la tienda de skins en Azure Table Storage y mantiene la partida en vivo en memoria. |
| Plugins | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Cinco plugins de SourceMod se comunican con la API y con el sitio web. |

Limitaciones que este proyecto elimina:

- La duración de la temporada está fija en dos meses, en dos lugares distintos.
- Las temporadas pasadas existen solo como listas guardadas de los 100 mejores, así que sus partidas no se pueden consultar.
- El sitio web asume un solo servidor de juego.
- Solo cuentan las partidas 4v4 con al menos dos mapas, y los modos de juego aceptados son un patrón escrito a mano en una fila de almacenamiento.
- Los mapas que no están en una lista fija de campañas se descartan.
- Instalarlo requiere una suscripción de Azure, filas de almacenamiento creadas a mano y secretos en tres proyectos.

## Diseño objetivo

Una sola aplicación ASP.NET Core que es el sitio web, la API para los plugins, el área de administración y el programador de tareas.

| Carpeta | Contenido |
|---|---|
| `src/` | La aplicación, la lógica de dominio y la capa de datos con Entity Framework Core, con un proyecto de migraciones por cada base de datos. |
| `plugins/` | Fuentes de SourceMod, includes, traducciones y archivos de configuración. |
| `deploy/` | Script de instalación, definición del servicio, proxy inverso y archivos de Docker. |
| `docs/` | Guías en inglés, portugués y español. |

Puntos principales:

- **El archivo de estadísticas sin procesar se sigue guardando** como fuente de verdad. Las partidas, los mapas y los resultados por jugador pasan a ser tablas, así que los rankings son consultas a la base de datos, y un comando de reprocesamiento puede aplicar reglas nuevas a archivos antiguos.
- **La configuración vive en la base de datos** y se edita en el área de administración. En un archivo de configuración solo quedan el proveedor de base de datos y la cadena de conexión.
- **Una sola instancia del back-end** atiende a todos los servidores de juego. Ejecutar varias instancias queda fuera del alcance por ahora.
- **La partida en vivo se mantiene en memoria**, nunca en la base de datos. Se guarda por separado para cada servidor de juego.

## Decisiones

| Tema | Decisión |
|---|---|
| Rankings | Dos rankings. El ranking de estadísticas es el principal y es la página de inicio del sitio. El ranking de la comunidad es secundario. |
| Alcance de los rankings | Ambos son globales. Los resultados de un jugador en todos los servidores de juego se suman. |
| Duración de la temporada | La define el administrador, en meses. Un cambio se aplica a partir de la temporada siguiente. |
| Temporadas pasadas | Se pueden consultar el ranking, las partidas y los jugadores de cualquier temporada pasada. |
| Cambios de reglas | Se aplican de ahí en adelante. El administrador puede reprocesar manualmente. |
| Mínimos | El número mínimo de mapas en una partida y el número mínimo de partidas para aparecer en el ranking son ambos configurables. |
| Base de datos | Siempre relacional, mediante Entity Framework Core. SQLite es la opción predeterminada. SQL Server, PostgreSQL y MySQL son opciones. |
| Back-end | Una sola instancia, sin importar la cantidad de servidores de juego. |
| Partida en vivo | Solo en memoria, separada por servidor de juego. |
| Reembolso de skins | Reembolsar las skins al final de una temporada es una opción de configuración, activada por defecto. |
| Catálogo de skins | El catálogo y las miniaturas viven en el repositorio de assets, y la aplicación los importa. |
| Plugins | Apuntan a servidores basados en L4D2 Competitive Rework. |
| Comandos remotos | El envío de comandos desde el sitio a un servidor de juego se incluye como módulo. |
| Anti-cheat | Reservado como módulo, desactivado, sin implementación por ahora. |
| Docker | Se ofrece como alternativa al script de instalación. |
| Datos antiguos | Se importan y se recalculan. No hay retrocompatibilidad con el almacenamiento antiguo. |
| Idiomas | El inglés es el predeterminado. Las versiones en portugués y en español se derivan de él. |
| Pruebas | Extensas en el back-end. Pruebas de front-end solo donde valgan la pena. |
| Front-end | Dos capas. El sitio principal apunta a navegadores actuales. Un conjunto separado de páginas respeta los límites del navegador del juego. |
| Páginas dentro del juego | Solo lo esencial: el ranking de la temporada, el resumen del jugador que abrió el panel y las últimas partidas. Todo lo demás está en el sitio principal. |
| Área de administración | Renderizada en el servidor, como el resto del sitio. Se permite un framework de página única, como Angular, si alguna pantalla lo necesita, y no se usa al principio. |
| Licencia | MIT para la aplicación. Los plugins son GPL-3.0, como lo exige SourceMod. |

## Módulos

El núcleo está siempre activo: registro de servidores de juego, subida de estadísticas, inicio de sesión con Steam y el área de administración.

| Módulo | Contenido |
|---|---|
| Ranking y estadísticas | Ranking, partidas, jugadores, patentes, sanciones e historial de temporadas. |
| Servidor en vivo | Marcador, jugadores, eventos y chat de la partida en curso. |
| Tienda de skins de armas | Estrellas, compras, regalos y skins equipadas. |
| Ranking de la comunidad | Votación por pares y su ranking. |
| Chat del sitio | Chat entre el sitio y el juego, con baneos. |
| Comandos remotos | Comandos enviados desde el sitio a un servidor de juego, con un registro de auditoría. |
| Anti-cheat | Reservado. |

Un módulo desactivado desaparece del menú y sus rutas responden como no encontradas. La tienda de skins depende del módulo de estadísticas, porque las estrellas salen de los resultados de las partidas.

## Servidores de juego

- El administrador registra cada servidor de juego y recibe un identificador y una clave para él. La clave se guarda con hash.
- Los plugins usan una sola dirección y un solo token con la forma `id:key`. El token le indica al back-end qué servidor envió cada archivo, así que el formato del archivo de estadísticas no cambia.
- Los archivos son únicos por servidor y por nombre de archivo.
- La partida en vivo, el chat del sitio y la cola de comandos están separados por servidor. El sitio le permite al visitante elegir qué servidor ver.

## Temporadas y rankings

- Las temporadas son filas en la base de datos, creadas a partir de la duración configurada.
- Un archivo con fecha anterior a la primera temporada crea las temporadas anteriores que necesita, usando la misma duración.
- El ranking de estadísticas conserva la puntuación actual como valores predeterminados que el administrador puede editar: puntos por victoria, por derrota y por rage quit, y puntos extra por MVP.
- Las sanciones pertenecen a una temporada.

## Modos de juego

El archivo de estadísticas trae el nombre visible de la configuración, como `ZoneMod v2.9.2b`, y el tamaño del equipo. No trae el nombre de la carpeta de la configuración.

- Cada nombre de configuración recibido se registra automáticamente.
- El administrador escribe reglas sobre esos nombres: un patrón de nombre que ignora la versión, los tamaños de equipo, si cuenta para el ranking y el número mínimo de mapas.
- Las campañas pasan a ser una tabla editable, y un mapa que no esté en la tabla ya no se descarta.

## Ranking de la comunidad

- El votante ve a dos jugadores con sus avatares de Steam y responde cuál es mejor.
- Las respuestas son: izquierda, derecha, mismo nivel o "No conozco a uno de ellos".
- Mismo nivel cuenta como empate en el cálculo. "No conozco" no cambia nada.
- Para votar se requiere un número mínimo de partidas jugadas, contadas entre todas las temporadas. Es una opción de configuración, con 10 como valor predeterminado.
- El número de votos por día está limitado por una opción de configuración. Los jugadores no pueden votar por sí mismos ni repetir un par.
- Los candidatos son jugadores con partidas registradas.
- El ranking no se reinicia con la temporada.

## Tienda de skins de armas

- Los jugadores ganan estrellas con los MVP y las gastan en skins.
- Las compras, los regalos, los precios y las skins equipadas pasan de Azure Table Storage a la base de datos.
- El reembolso de temporada sigue las mismas temporadas que el ranking, y el administrador puede desactivarlo.
- Los modelos, los materiales y las fotos se quedan en [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

## Asistente de configuración y estado del servidor

Asistente de primera ejecución, que se muestra una sola vez después de la instalación:

1. El primer administrador inicia sesión con Steam, usando un código que imprime el instalador.
2. Nombre del sitio, idioma y clave de la Steam Web API.
3. Módulos que se van a activar.
4. Duración de la temporada y reglas iniciales de modos de juego.
5. El primer servidor de juego.

Asistente de servidor de juego, que se usa para el primer servidor y para cada uno que se agregue después:

1. Nombre y dirección.
2. Qué hay que instalar en el servidor de juego, con un enlace de descarga de los plugins.
3. El bloque de configuración exacto que hay que pegar y el archivo donde va.
4. Validación. La página espera y cambia a "conectado" cuando el servidor de juego responde.

Es el servidor de juego el que llama al back-end, y no al revés. Eso funciona detrás de NAT y no requiere ninguna contraseña de RCON en el sitio. Cada servidor muestra uno de estos estados:

| Estado | Significado |
|---|---|
| En espera | Nunca se conectó. |
| En línea | Llegó una señal en los últimos segundos. |
| Fuera de línea | Muestra cuándo se lo vio por última vez. |
| Clave inválida | El identificador llegó con la clave incorrecta. |

Cada servidor tiene además una lista de verificación, con una sugerencia de corrección en cada punto que falla: conexión, versiones de los plugins, escritura de estadísticas habilitada, primer archivo de estadísticas recibido, llegada de datos en vivo y el plugin de skins cuando ese módulo está activo. Un comando de consola en el servidor de juego ejecuta la misma verificación desde ese lado.

## Front-end

El navegador integrado en el juego es Chromium 18, de 2012. Solo ejecuta ES5 y no tiene flexbox, grid ni propiedades personalizadas. Las mediciones están en [in-game-browser.es.md](in-game-browser.es.md).

El front-end tiene dos capas:

- **Sitio principal.** Apunta a navegadores actuales: páginas renderizadas en el servidor con un framework CSS moderno y sin paso de compilación con Node. El área de administración se construye de la misma manera.
- **Páginas del juego.** Un conjunto pequeño bajo su propia ruta, que es lo que abre el plugin. Respetan los límites de ese navegador y se sirven por HTTP sin cifrar.

Las páginas del juego son de solo lectura y no requieren iniciar sesión:

| Página | Contenido |
|---|---|
| Ranking | La temporada actual, con posición, patente y puntos. |
| Resumen del jugador | Posición, puntos, victorias y derrotas, y progreso de patente del jugador que abrió el panel. El plugin pasa el identificador del jugador en la dirección. |
| Últimas partidas | Una lista corta con equipos y marcadores. |

La tienda de skins, el ranking de la comunidad, el servidor en vivo y el área de administración están solo en el sitio principal.

## Pruebas

- **Back-end, la prioridad.** Pruebas unitarias para el armado de partidas, la puntuación, las temporadas, las reglas de modos de juego, la calificación de la comunidad y la billetera de la tienda de skins. Pruebas de integración contra una base de datos SQLite real, y contra las demás bases de datos en la integración continua. Pruebas de endpoints para todo lo que llaman los plugins, para que el contrato con el servidor de juego no pueda romperse sin que nadie lo note.
- **Plugins.** Cada plugin se compila en la integración continua.
- **Front-end.** Solo pruebas de humo que comprueban que las páginas se renderizan, más una verificación de que las páginas dentro del juego se mantienen dentro de los límites del navegador antiguo.

## Despliegue

- **VPS Linux, el camino principal.** Un solo comando descarga un script de instalación. El script instala una compilación autocontenida, crea el servicio, configura HTTPS cuando se indica un dominio e imprime la dirección de configuración y el bloque de configuración para el servidor de juego. Se incluyen comandos de actualización y de respaldo.
- **Azure.** Una guía para App Service con un contenedor. Ese servicio debe ejecutar una sola instancia, y ahí se recomienda SQL Server porque su disco persistente es un recurso compartido de red.
- **Docker.** Una imagen para quienes la prefieran.

## Migración de datos

Un programa de consola aparte, fuera de este repositorio, mueve los datos de producción una sola vez:

- Lee los archivos de estadísticas sin procesar del almacenamiento antiguo y sube cada uno por el mismo endpoint que usa el plugin, así que todo se recalcula por el camino normal.
- Las sanciones antiguas se borraban en cada cambio de temporada y no vuelven, así que el historial recalculado puede diferir un poco de las listas antiguas.
- Los regalos de skins, las compras de la temporada actual, los precios y los baneos del chat se escriben en la nueva base de datos.

## Fases

1. **Base.** Repositorio, integración continua, base de datos, configuración y módulos, inicio de sesión con Steam, asistente de primera ejecución, registro de servidores de juego con la señal de conexión y un script de instalación mínimo.
2. **Estadísticas y ranking.** Subida, armado de partidas, puntuación, sanciones, temporadas y reglas de modos de juego.
3. **Sitio web.** Las páginas existentes sobre los nuevos servicios, con filtros por temporada y por servidor.
4. **Servidor en vivo.** Estado en vivo por servidor, selector de servidor, chat del sitio, cola de comandos y baneos del chat.
5. **Tienda de skins de armas.** Almacenamiento en la base de datos, importación del catálogo y reembolso de temporada.
6. **Plugins.** Los cinco plugins, una sola dirección y un solo token, correcciones al plugin de subida, versiones compiladas.
7. **Ranking de la comunidad.** Página de votación, calificación y límites.
8. **Despliegue y tutoriales.** Script de instalación completo, guías de Azure y Docker, tutoriales paso a paso.
9. **Migración.** Importación de los datos de producción, cambio del sitio de producción, archivado de los repositorios antiguos.

## Preguntas abiertas

- **MySQL.** Depende de que haya un proveedor de Entity Framework Core disponible para la versión de .NET en uso.
- **HTTPS dentro del juego.** Aún no medido. Ver [in-game-browser.es.md](in-game-browser.es.md#aún-no-medido).
