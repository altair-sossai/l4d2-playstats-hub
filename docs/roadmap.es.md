# Hoja de ruta

🌐 [English](roadmap.md) | [Português](roadmap.pt.md) | **Español**

El plan para traer el sistema L4D2 PlayStats a este repositorio: de dónde viene, el diseño, las decisiones tomadas, qué está hecho y qué sigue abierto.

## Contenido

- [De dónde viene](#de-dónde-viene)
- [Diseño](#diseño)
- [Decisiones](#decisiones)
- [Módulos](#módulos)
- [Servidores de juego](#servidores-de-juego)
- [Temporadas y rankings](#temporadas-y-rankings)
- [Modos de juego](#modos-de-juego)
- [Ranking de la comunidad](#ranking-de-la-comunidad)
- [Tienda de skins de armas](#tienda-de-skins-de-armas)
- [Servidor en vivo](#servidor-en-vivo)
- [Asistente de configuración y estado del servidor](#asistente-de-configuración-y-estado-del-servidor)
- [Front-end](#front-end)
- [Pruebas](#pruebas)
- [Despliegue](#despliegue)
- [Migración de datos](#migración-de-datos)
- [Progreso](#progreso)
- [Preguntas abiertas](#preguntas-abiertas)

## De dónde viene

| Pieza | Repositorio | Cómo funcionaba |
|---|---|---|
| Analizador de estadísticas | [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | Paquete NuGet que lee los archivos que escribe `l4d2_playstats`. |
| API | [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | Azure Functions. Guardaba los archivos sin procesar en Azure Table Storage y, a partir de ellos, reconstruía las partidas y el ranking en cada solicitud. |
| Sitio web | [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | ASP.NET Core MVC. Leía de la API, guardaba la tienda de skins en Azure Table Storage y mantenía la partida en vivo en memoria. |
| Plugins | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Cinco plugins de SourceMod se comunicaban con la API y con el sitio web, con direcciones y secretos distintos. |

Limitaciones de esa configuración, que este proyecto elimina:

- La duración de la temporada estaba fija en dos meses, en dos lugares distintos.
- Las temporadas pasadas existían solo como listas guardadas de los 100 mejores, así que sus partidas no se podían consultar.
- El sitio web asumía un solo servidor de juego.
- Solo contaban las partidas 4v4 con al menos dos mapas, y los modos de juego aceptados eran un patrón escrito a mano en una fila de almacenamiento.
- Los mapas que no estaban en una lista fija de campañas se descartaban.
- Instalarlo requería una suscripción de Azure, filas de almacenamiento creadas a mano y secretos en tres proyectos.

## Diseño

Una sola aplicación ASP.NET Core que es el sitio web, la API para los plugins, el área de administración y el programador de tareas.

- **El archivo de estadísticas sin procesar se sigue guardando** como fuente de verdad. Las partidas, los mapas y los resultados por jugador son tablas construidas a partir de él, y un comando de reprocesamiento las reconstruye cuando cambian las reglas.
- **Los ajustes viven en la base de datos** y se editan en el área de administración. Fuera de ella solo se definen el proveedor de base de datos, la cadena de conexión y la carpeta de datos.
- **Una sola instancia del back-end** atiende a todos los servidores de juego. Ejecutar varias instancias queda fuera del alcance.
- **La partida en vivo se mantiene en memoria**, nunca en la base de datos, y se guarda por separado para cada servidor de juego.
- **Cada funcionalidad es un módulo** en el código y, donde tiene sentido, algo que el administrador puede desactivar.

[CONTRIBUTING.es.md](../CONTRIBUTING.es.md) describe las carpetas y las reglas de código.

## Decisiones

| Tema | Decisión |
|---|---|
| Rankings | Dos rankings. El ranking de estadísticas es el principal y es lo primero que muestra el sitio. El ranking de la comunidad es secundario. |
| Alcance de los rankings | Los dos son globales. Los resultados de un jugador en todos los servidores de juego se suman. |
| Duración de la temporada | La define el administrador, en meses. Un cambio se aplica a partir de la temporada siguiente. |
| Límites de la temporada | Siguen el reloj del servidor de juego que envió cada archivo, como hacía el sistema anterior. |
| Temporadas pasadas | Se pueden consultar el ranking, las partidas y los jugadores de cualquier temporada pasada, así como de un año completo y de todo el historial. |
| Cambios de reglas | Se aplican de ahí en adelante. El administrador puede reprocesar todas las partidas. |
| Mínimos | El número mínimo de mapas en una partida y el número mínimo de partidas para aparecer en el ranking son ajustes. |
| Mapas desconocidos | Una partida en mapas que no están en la lista de campañas se conserva. Un ajuste lo desactiva. |
| Base de datos | Siempre relacional, mediante Entity Framework Core. SQLite es la opción predeterminada. SQL Server, PostgreSQL y MySQL son opciones. |
| Versión de Entity Framework Core | 10, la versión con soporte a largo plazo. Los proveedores de PostgreSQL y MySQL todavía no están disponibles para la versión 11. |
| Back-end | Una sola instancia, sea cual sea el número de servidores de juego. |
| Partida en vivo | Solo en memoria, separada por servidor de juego. |
| Reembolso de skins | Reembolsar las skins al final de una temporada es un ajuste, activado de forma predeterminada. |
| Catálogo de skins | El catálogo y las imágenes viven en el repositorio de assets, y la aplicación importa el catálogo. |
| Plugins | Están pensados para servidores basados en L4D2 Competitive Rework. Una sola dirección y un solo token para todos. |
| Comandos remotos | Enviar comandos desde el sitio a un servidor de juego es un módulo, desactivado de forma predeterminada, con un registro de auditoría. |
| Anti-cheat | Un módulo, desactivado de forma predeterminada, que muestra la página del Anti-Cheat con la descarga y las instrucciones. El programa Anti-Cheat y su servicio son proyectos aparte. |
| Front-end | Dos capas: el sitio principal para navegadores actuales, y unas pocas páginas hechas para el navegador que está dentro del juego. |
| Páginas dentro del juego | Solo lo esencial: el ranking de la temporada, el resumen del jugador que abrió el panel y las últimas partidas. |
| Área de administración | Renderizada en el servidor, como el resto del sitio. Se permite un framework de página única como Angular si una pantalla lo necesita, pero no se usa. |
| Docker | Se ofrece como alternativa al script de instalación. |
| Datos antiguos | Se importan y se recalculan. No hay compatibilidad hacia atrás con el almacenamiento antiguo. |
| Idiomas | El inglés es el predeterminado. Las versiones en portugués y en español se derivan de él. |
| Organización del código | Un tipo por archivo, agrupados por módulo y luego por tipo. |
| Pruebas | Extensas en el back-end. Pruebas de front-end solo donde valgan la pena. |
| Licencia | MIT para la aplicación. Los plugins son GPL-3.0, como exige SourceMod. |

## Módulos

El núcleo siempre está activado: registro de servidores de juego, subida de estadísticas, inicio de sesión con Steam y el área de administración.

| Módulo | Contenido | Predeterminado |
|---|---|---|
| Ranking y estadísticas | Ranking, partidas, jugadores, patentes, sanciones e historial de temporadas. | Activado |
| Servidor en vivo | Marcador, jugadores y eventos de la partida en curso. | Activado |
| Chat del sitio | Chat entre el sitio y el juego, con baneos. Necesita el servidor en vivo. | Desactivado |
| Comandos remotos | Comandos enviados desde el sitio a un servidor de juego. Necesita el servidor en vivo. | Desactivado |
| Tienda de skins de armas | Estrellas, compras, regalos y skins equipadas. Necesita el ranking. | Desactivado |
| Ranking de la comunidad | Votación por pares y su ranking. | Desactivado |
| Anti-Cheat | La página con la descarga y las instrucciones del Anti-Cheat. | Desactivado |

Un módulo desactivado desaparece del menú y sus páginas responden como no encontradas.

## Servidores de juego

- El administrador registra cada servidor de juego y recibe un identificador y una clave. Solo se guarda un hash de la clave.
- Los plugins usan una sola dirección y un solo token con la forma `identifier:key`. El token le dice al sitio qué servidor envió cada archivo, así que el formato del archivo de estadísticas no cambió.
- Los archivos son únicos por servidor y nombre de archivo.
- La partida en vivo, el chat del sitio y la cola de comandos están separados por servidor. El visitante elige qué servidor ver.

## Temporadas y rankings

- Las temporadas son filas en la base de datos, creadas a partir de la duración configurada. Una temporada de dos meses empieza en enero, marzo, mayo y así sucesivamente.
- Un archivo con fecha anterior a la primera temporada crea las temporadas anteriores que necesita.
- La puntuación conserva los valores antiguos como predeterminados que el administrador puede editar: 85 por una victoria, -80 por una derrota, -150 por un rage quit, 8 y 6 por cada MVP. La experiencia nunca baja de cero.
- Las sanciones pertenecen a una temporada.
- Las partidas nunca cruzan un límite de temporada, un cambio de configuración ni un cambio de servidor de juego.

## Modos de juego

El archivo de estadísticas trae el nombre visible de la configuración, como `ZoneMod v2.9.2b`, y el tamaño del equipo. No trae el nombre de la carpeta de la configuración.

- Cada nombre de configuración recibido se registra automáticamente y se lista en el área de administración.
- El administrador escribe reglas sobre esos nombres: un patrón donde `*` representa cualquier texto, el tamaño del equipo, si cuenta para el ranking y el número mínimo de mapas.
- La primera regla que encaja con una partida decide. Una partida que no encaja con ninguna regla no cuenta.
- Las campañas son una tabla editable, que se usa para saber qué mapas pertenecen a la misma partida.

## Ranking de la comunidad

- El votante ve a dos jugadores con sus avatares y responde cuál es mejor: izquierda, derecha, mismo nivel, o "no conozco" a uno o a los dos.
- Mismo nivel cuenta como empate. "No conozco" no cambia nada y, de ahí en adelante, oculta a ese jugador para ese votante.
- La puntuación es Elo, y no se reinicia con la temporada.
- Para votar se necesita un número mínimo de partidas jugadas, contadas entre todas las temporadas: 10 de forma predeterminada.
- Los votos por día están limitados: 20 de forma predeterminada. Los jugadores no pueden votar por sí mismos ni dos veces por el mismo par.
- El sitio elige el par, prefiriendo a jugadores con los que el votante ha jugado o contra los que ha jugado, y acepta una respuesta solo para el par que ofreció.

## Tienda de skins de armas

- Los jugadores ganan una estrella por cada MVP en partidas clasificatorias, a lo largo de todo su historial, y gastan las estrellas en skins.
- El catálogo se importa desde un archivo del repositorio de assets. Las imágenes se cargan desde una dirección que define el administrador.
- Al inicio de una temporada, las skins compradas antes se reembolsan y los regalos se conservan. El administrador puede desactivar el reembolso.
- Los modelos, los materiales y las fotos se quedan en [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

## Servidor en vivo

- El servidor de juego envía la configuración, la ronda, el marcador, los jugadores, el chat y los eventos de la partida.
- El sitio los muestra con unos segundos de retraso, como hacía el sitio anterior.
- Las conexiones por dirección IP se guardan para señalar las cuentas que comparten una dirección. La dirección en sí nunca se muestra.

## Asistente de configuración y estado del servidor

El asistente de primera ejecución se ejecuta una sola vez, después de la instalación: el primer administrador inicia sesión con Steam usando un código que muestra el instalador, y luego elige los ajustes del sitio, los módulos y el primer servidor de juego.

Para cada servidor de juego, el sitio muestra las dos líneas que hay que pegar en su configuración y luego espera a que el servidor responda. El servidor de juego llama al sitio, y no al revés, lo que funciona detrás de NAT y no necesita contraseña de RCON.

| Estado | Significado |
|---|---|
| En espera | Nunca se conectó. |
| Conectado | Llegó una señal en los últimos 90 segundos. |
| Fuera de línea | Muestra cuándo se lo vio por última vez. |
| Clave inválida | El identificador llegó con la clave equivocada en los últimos 10 minutos. |

Una lista de verificación muestra qué se encontró: conexión, plugin de subida, escritura de estadísticas, primer archivo recibido, datos en vivo y el plugin de skins. El comando `sm_playstats_test` ejecuta la misma verificación desde la consola del servidor de juego.

## Front-end

El navegador que está dentro del juego es Chromium 18, de 2012. Solo ejecuta ES5 y no tiene flexbox, grid ni propiedades personalizadas. Las mediciones están en [in-game-browser.es.md](in-game-browser.es.md).

- **Sitio principal.** Páginas renderizadas en el servidor que mantienen el aspecto y el contenido del sitio que este proyecto reemplaza: Bootstrap 3 con sus temas oscuro y claro, jQuery, Chart.js y diffDOM, sin ningún paso de compilación con Node. Cada página del sitio antiguo se portó elemento por elemento; las listas de comprobación están en [site-parity](site-parity/README.es.md). Las páginas que son nuevas aquí (asistente, administración, ranking de la comunidad) siguen el mismo aspecto.
- **Páginas dentro del juego.** Tres páginas de solo lectura bajo `/ingame`, sin scripts, que caben en el panel del juego. Una prueba falla si empiezan a usar algo que ese navegador no tiene.

## Pruebas

- **Back-end.** Pruebas unitarias para el armado de partidas, la puntuación, las temporadas, las reglas de modos de juego, la puntuación Elo y la tienda de skins. Pruebas a través de HTTP para cada página y para cada endpoint que llaman los plugins.
- **Paridad.** Una prueba compara los rankings con una copia de los datos de producción. Coincide exactamente con la API antigua en las temporadas calculadas con las reglas actuales.
- **Bases de datos.** Las mismas pruebas se ejecutan en SQLite, SQL Server, PostgreSQL y MySQL, y verifican que las migraciones de cada una coincidan con el modelo.
- **Plugins.** Todos los plugins se compilan en cada cambio.

## Despliegue

- **VPS Linux.** Un solo comando instala una compilación autocontenida como servicio, con HTTPS opcional mediante Caddy, y muestra el código de configuración. Actualizar, hacer una copia de seguridad y restaurar son un solo comando cada uno.
- **Docker.** Una imagen que se publica con cada versión.
- **Azure.** Una máquina virtual con el instalador para Linux, o App Service con la imagen.

## Migración de datos

Un programa de consola aparte, fuera de este repositorio, mueve los datos de producción una sola vez:

- Lee los archivos de estadísticas sin procesar del almacenamiento antiguo, sin modificarlo, y sube cada uno por el mismo endpoint que usa el plugin, así que todo se recalcula por el camino normal.
- Las sanciones antiguas se borraban en cada cambio de temporada y no vuelven.
- Los rankings guardados antes de agosto de 2026 se calcularon con una regla anterior para el número mínimo de mapas, así que el historial recalculado difiere de esas listas.

## Progreso

| Fase | Contenido | Estado |
|---|---|---|
| 1. Base | Base de datos, ajustes y módulos, inicio de sesión con Steam, asistente, registro y estado de los servidores de juego. | Hecho |
| 2. Estadísticas y ranking | Subida, armado de partidas, puntuación, sanciones, temporadas y reglas de modos de juego. | Hecho |
| 3. Sitio web | Ranking, historial, partidas, jugadores, patentes, página del Anti-Cheat, área de administración y las páginas dentro del juego, en tres idiomas. | Hecho; cada pantalla del sitio antiguo portada y comprobada elemento por elemento |
| 4. Servidor en vivo | Estado en vivo por servidor, selector de servidor, chat del sitio, comandos remotos y baneos del chat. | Hecho, todavía no se ha visto con un servidor de juego real |
| 5. Tienda de skins de armas | Tienda, regalos, reembolsos, importación del catálogo y reembolso de temporada. | Hecho, todavía no se ha probado con los assets reales |
| 6. Plugins | Los cinco plugins con una sola dirección y un solo token, heartbeat y prueba de conexión. | Compilados, todavía no se han ejecutado en un servidor de juego |
| 7. Ranking de la comunidad | Página de votación, puntuación, límites y página de administración. | Hecho |
| 8. Despliegue | Instalador para Linux, imagen de Docker, workflow de publicación y guías. | Hecho; publicado desde la v0.1.1, instalador e imagen probados a partir de la versión publicada |
| 9. Migración | Importación de los datos de producción, cambio del sitio de producción y archivado de los repositorios antiguos. | Importación probada localmente; el cambio está pendiente |

## Preguntas abiertas

- **Plugins en un servidor en vivo.** Compilan, y el sitio acepta lo que envían, pero no se han ejecutado en una partida real.
- **HTTPS dentro del juego.** No se ha medido. Consulta [in-game-browser.es.md](in-game-browser.es.md#aún-no-medido).
- **Repositorio de assets.** La tienda espera las miniaturas en `skins/{id}.jpg` y las fotos en `skin-shots/{id}/`, bajo una misma dirección. El repositorio de assets todavía no tiene las miniaturas.
- **Íconos de patente dentro del juego.** Los sprites que usa `l4d2_show_patent_icon` no están en este repositorio.
- **Azure App Service.** La guía se escribió a partir de la documentación de Azure y no se ha ejecutado de principio a fin.
