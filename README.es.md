# L4D2 PlayStats Hub

🌐 [English](README.md) | [Português](README.pt.md) | **Español**

Una sola aplicación web autoalojada para servidores competitivos de Left 4 Dead 2: ranking, estadísticas de partidas, vista del servidor en vivo, tienda de skins de armas y un ranking de la comunidad, junto con los plugins de SourceMod que la alimentan.

> **Estado: desarrollo inicial.** Todavía no se puede instalar nada de este repositorio. Los repositorios existentes que se listan más abajo siguen ejecutando el sitio de producción ([l4d2.com.br](https://l4d2.com.br/)) y siguen siendo la fuente de verdad hasta que termine la migración.

## Contenido

- [Por qué existe este proyecto](#por-qué-existe-este-proyecto)
- [Qué incluirá](#qué-incluirá)
- [Qué queda fuera de este repositorio](#qué-queda-fuera-de-este-repositorio)
- [Repositorios que se están consolidando](#repositorios-que-se-están-consolidando)
- [Hoja de ruta](#hoja-de-ruta)
- [Documentación](#documentación)

## Por qué existe este proyecto

El sistema empezó pequeño y creció hasta repartirse en varios repositorios: un analizador de estadísticas, una API, un sitio web con una tienda de skins y un conjunto de plugins para el servidor de juego. Hoy, ponerlo en marcha requiere recursos de Azure, filas de almacenamiento creadas a mano y secretos repartidos en tres proyectos.

Este repositorio reúne la lógica para que cualquier dueño de servidor pueda ejecutar el mismo sistema de ranking y estadísticas con una sola aplicación, un solo archivo de base de datos y un solo comando de instalación.

La migración se hace paso a paso. Cada funcionalidad se trae aquí solo cuando está lista, y los repositorios antiguos se archivan al final.

## Qué incluirá

- **Una sola aplicación web** (ASP.NET Core) que es el sitio web, la API que usan los plugins del servidor de juego, el área de administración y las tareas programadas.
- **Base de datos relacional mediante Entity Framework Core.** SQLite es la opción predeterminada, así que una instalación nueva no necesita un servidor de base de datos. SQL Server, PostgreSQL y MySQL están previstos como opciones.
- **Módulos que el administrador activa o desactiva:** ranking y estadísticas, vista del servidor en vivo, tienda de skins de armas, ranking de la comunidad, chat del sitio.
- **Varios servidores de juego en un solo back-end.** Cada servidor tiene su propia clave, y las estadísticas de cada jugador se suman entre todos los servidores.
- **Temporadas configurables.** El administrador define cuántos meses dura una temporada, y las temporadas pasadas se pueden seguir consultando: ranking, partidas y jugadores.
- **Modos de juego configurables.** El administrador elige qué modos cuentan para el ranking y el tamaño mínimo de una partida competitiva.
- **Ranking de la comunidad.** Los jugadores votan sobre pares de jugadores, y los votos forman un segundo ranking junto al de estadísticas.
- **Asistente de configuración y estado del servidor.** Una guía de primera ejecución recorre la configuración, y cada servidor de juego registrado muestra si está conectado y bien configurado.
- **Plugins de SourceMod** que suben las estadísticas, envían la partida en vivo, muestran el ranking dentro del juego y aplican las skins de armas.
- **Despliegue sencillo.** Un script de instalación para un VPS Linux, con una guía para Azure y una imagen de Docker como alternativas.
- **Pruebas automatizadas**, con el back-end como prioridad.

## Qué queda fuera de este repositorio

| Repositorio | Por qué sigue separado |
|---|---|
| [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | Paquete NuGet que analiza los archivos de estadísticas. Es útil por sí solo y este proyecto lo usa como dependencia. |
| [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets) | Modelos, materiales y fotos de las skins de armas. Ocupan más de 500 MB, así que aquí solo vive la lógica de la tienda de skins. |

## Repositorios que se están consolidando

| Repositorio | Qué es hoy | Qué pasará con él |
|---|---|---|
| [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | API en Azure Functions que guarda los archivos de estadísticas y calcula partidas, ranking y estrellas. | Se reemplaza por esta aplicación y luego se archiva. |
| [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | Sitio web con ranking, estadísticas, página del servidor en vivo y tienda de skins de armas. | Se reemplaza por esta aplicación y luego se archiva. |
| [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Configuración de servidor competitivo con plugins propios. | Los plugins de abajo se mueven aquí. |

Plugins que se mueven desde `l4d2-zone-server`:

| Plugin | Qué hace |
|---|---|
| `l4d2_playstats_sync` | Sube los archivos de estadísticas que escribe `l4d2_playstats`. |
| `l4d2_gameinfo_sync` | Envía la partida en vivo: marcador, jugadores, eventos y chat. |
| `l4d2_ranking` | Abre el ranking dentro del juego. |
| `l4d2_show_patent_icon` | Muestra el ícono de patente de cada jugador dentro del juego. |
| `l4d2_weapon_skins` | Aplica las skins de armas que tienen los jugadores. |

El propio `l4d2_playstats`, que escribe los archivos de estadísticas, viene de [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) y no se copia aquí.

## Hoja de ruta

1. Base: esqueleto de la aplicación, base de datos, configuración y módulos, inicio de sesión con Steam, asistente de configuración, registro de servidores de juego.
2. Subida de estadísticas, partidas, temporadas y ranking.
3. Páginas del sitio web, con filtros por temporada y por servidor.
4. Vista del servidor en vivo para varios servidores.
5. Tienda de skins de armas.
6. Plugins del servidor de juego.
7. Ranking de la comunidad.
8. Script de instalación, guías de despliegue y tutoriales.
9. Migración de datos y archivado de los repositorios antiguos.

El plan completo, con todas las decisiones tomadas hasta ahora, está en [docs/roadmap.es.md](docs/roadmap.es.md).

## Documentación

| Documento | Contenido |
|---|---|
| [docs/roadmap.es.md](docs/roadmap.es.md) | Arquitectura, decisiones, fases y preguntas abiertas. |
| [docs/in-game-browser.es.md](docs/in-game-browser.es.md) | Qué soporta el navegador integrado en Left 4 Dead 2 y qué significa eso para el front-end. |

El inglés es el idioma predeterminado de este repositorio. Cada documento tiene también una versión en portugués (`.pt.md`) y otra en español (`.es.md`).
