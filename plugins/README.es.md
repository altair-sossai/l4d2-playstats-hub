# Plugins del servidor de juego

🌐 [English](README.md) | [Português](README.pt.md) | **Español**

Los plugins de SourceMod que conectan un servidor de Left 4 Dead 2 con L4D2 PlayStats Hub. Usan una sola dirección y un solo token para todo.

## Contenido

- [Los plugins](#los-plugins)
- [Requisitos](#requisitos)
- [Instalación](#instalación)
- [Ajustes](#ajustes)
- [Comandos](#comandos)
- [Verificar la conexión](#verificar-la-conexión)
- [Cargarlos en un modo de partida de Confogl o ZoneMod](#cargarlos-en-un-modo-de-partida-de-confogl-o-zonemod)
- [Migrar desde la configuración anterior](#migrar-desde-la-configuración-anterior)
- [Qué llamadas hacen los plugins](#qué-llamadas-hacen-los-plugins)
- [Compilación](#compilación)
- [Licencia](#licencia)

## Los plugins

| Plugin | Qué hace | Módulo del sitio |
|---|---|---|
| `l4d2_playstats_sync` | Le avisa al sitio cada 30 segundos que el servidor está activo, y sube los archivos de estadísticas que escribe `l4d2_playstats`. También tiene el comando que verifica la conexión. | Siempre activo |
| `l4d2_gameinfo_sync` | Envía la partida en vivo: configuración, ronda, marcador, jugadores, eventos y chat. También lleva el chat del sitio al juego y ejecuta los comandos enviados desde el sitio. | Servidor en vivo, Chat del sitio, Comandos remotos |
| `l4d2_ranking` | Abre la página del ranking dentro del juego con `!ranking`, y por sí solo para los jugadores que entran antes de que empiece una partida. | Ranking y estadísticas |
| `l4d2_show_patent_icon` | Muestra el ícono de patente de cada jugador sobre su cabeza antes de que empiece una partida. | Ranking y estadísticas |
| `l4d2_weapon_skins` | Aplica las skins de armas que los jugadores compraron en la tienda. | Tienda de skins de armas |

`l4d2_playstats_sync` es el que todo servidor necesita. Los demás son opcionales, y cada uno funciona sin los otros.

## Requisitos

- Un servidor dedicado de Left 4 Dead 2 con [SourceMod](https://www.sourcemod.net) 1.12 o más reciente.
- La extensión [REST in Pawn](https://github.com/ErikMinekus/sm-ripext). Los cinco plugins la necesitan.
- [Left 4 DHooks Direct](https://github.com/SilvDev/Left4DHooks) (`left4dhooks`). `l4d2_gameinfo_sync`, `l4d2_ranking` y `l4d2_show_patent_icon` lo necesitan.
- El plugin `l4d2_playstats` de [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), con `sm_stats_writestats 1`. Escribe los archivos de estadísticas que `l4d2_playstats_sync` sube.
- Un servidor de juego registrado en el sitio. El sitio le da un identificador y una clave.

Los plugins están pensados para servidores basados en L4D2 Competitive Rework. `l4d2_gameinfo_sync` usa `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod` y `l4d2_skill_detect` cuando están cargados, y sigue funcionando sin ellos.

Dos plugins necesitan archivos que los jugadores descargan del servidor (FastDL, `sv_downloadurl`). Los archivos están en [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets):

- `l4d2_show_patent_icon` necesita los sprites `materials/sprites/patent_<level>_v<version>.vmt` y `.vtf`.
- `l4d2_weapon_skins` necesita los modelos y materiales listados en `configs/weapon_skins.cfg`.

## Instalación

1. Obtén los plugins compilados (`.smx`) desde las [versiones](https://github.com/altair-sossai/l4d2-playstats-hub/releases), o [compílalos](#compilación).
2. Copia los archivos al servidor de juego, dentro de la carpeta `left4dead2`:

   | Desde | Hacia |
   |---|---|
   | `l4d2_playstats_sync.smx` | `addons/sourcemod/plugins/` |
   | Los demás archivos `.smx` | `addons/sourcemod/plugins/`, o `addons/sourcemod/plugins/optional/` cuando un modo de partida los carga |
   | `translations/` | `addons/sourcemod/translations/`, conservando las carpetas de idioma |
   | `configs/weapon_skins.cfg` | `addons/sourcemod/configs/` |
   | `cfg/playstats.cfg.example` | `cfg/playstats.cfg` |

3. Abre `cfg/playstats.cfg` y completa `playstats_url` y `playstats_token` con los valores que el sitio muestra en la página del servidor de juego.
4. Agrega `exec playstats.cfg` a `cfg/server.cfg`. También puedes pegar las dos líneas directamente en `cfg/server.cfg`.
5. Reinicia el servidor de juego o cambia de mapa.
6. Ejecuta `sm_playstats_test` en la consola del servidor.

Deja `l4d2_playstats_sync.smx` directamente en `addons/sourcemod/plugins/`. Así se carga cuando el servidor se inicia, los dos cvars existen cuando se ejecuta `server.cfg`, y el sitio ve el servidor incluso fuera de una partida.

## Ajustes

Compartidos por todos los plugins:

| Cvar | Valor predeterminado | Significado |
|---|---|---|
| `playstats_url` | vacío | Dirección del sitio, sin barra al final. Ejemplo: `https://stats.example.com`. |
| `playstats_token` | vacío | Token de este servidor de juego, con el formato `<identifier>:<key>`. Está protegido, así que el juego nunca muestra su valor a los jugadores. |

`l4d2_ranking`:

| Cvar | Valor predeterminado | Significado |
|---|---|---|
| `playstats_ingame_url` | vacío | Dirección que el juego abre para las páginas dentro del juego. Cuando está vacío, se usa `playstats_url`. |
| `ranking_show_motd_panel` | `1` | `0` impide que el plugin abra la página. |

La página que abre es `<address>/ingame?steamid=<SteamID64 of the player>`.

El navegador dentro del juego es Chromium 18, de 2012, y puede que no abra un sitio HTTPS moderno. Si el panel queda en blanco, sirve el sitio por HTTP simple en otra dirección o puerto y pon esa dirección en `playstats_ingame_url`. Mantén la dirección corta. Consulta [docs/in-game-browser.es.md](../docs/in-game-browser.es.md).

`l4d2_show_patent_icon`:

| Cvar | Valor predeterminado | Significado |
|---|---|---|
| `patent_icon_version` | `1` | Versión en el nombre de los archivos de sprites. |
| `patent_icon_max_level` | `15` | Nivel de patente más alto que tiene un sprite. |

`l4d2_gameinfo_sync`:

| Cvar | Valor predeterminado | Significado |
|---|---|---|
| `gameinfo_related_accounts_chat` | `1` | Anuncia en el chat cuando un jugador se conecta desde la misma dirección IP que otra cuenta vista antes. |
| `gameinfo_boomer_vomit_min_survivors` | `3` | Cantidad mínima de supervivientes que un boomer tiene que alcanzar para que se envíe el evento. |

`l4d2_gameinfo_sync` envía al sitio la dirección IP de cada jugador que se conecta, y el sitio la usa para encontrar cuentas que comparten una dirección.

`l4d2_weapon_skins`:

| Cvar | Valor predeterminado | Significado |
|---|---|---|
| `weapon_skins_enabled` | `0` | Activa las skins. |
| `weapon_skins_kick_no_downloads` | `0` | Expulsa a los jugadores cuyo juego se niega a descargar los archivos del servidor. |
| `weapon_skins_pile_radius` | `120.0` | Radio usado para encontrar armas primarias soltadas cerca unas de otras. |
| `weapon_skins_pile_max` | `3` | Cantidad máxima de armas primarias soltadas que se conservan dentro del radio. `0` lo desactiva. |
| `weapon_skins_preview_all` | `0` | Modo de prueba: todas las skins se descargan en el siguiente mapa, y los administradores root pueden usar cualquiera. |

## Comandos

| Comando | Quién | Qué hace |
|---|---|---|
| `sm_playstats_test` | Consola del servidor, administradores root | Llama al sitio de inmediato y muestra el resultado. |
| `sm_ranking` (`!ranking`) | Jugadores | Abre la página del ranking dentro del juego. |
| `sm_skin`, `sm_skins` (`!skin`) | Jugadores | Elige una skin de arma. Escribe `!skin` dentro del juego para ver las opciones. |

## Verificar la conexión

Ejecuta `sm_playstats_test` en la consola del servidor. Primero muestra el resultado de la última llamada automática, luego llama al sitio y muestra uno de estos:

| Resultado | Significado |
|---|---|
| `Connected to the site "..." as the game server "..."` | Todo está bien. Las líneas siguientes listan los módulos activados, los plugins cargados, si la escritura de estadísticas está activada y cuántos archivos de estadísticas están en espera. |
| `URL not set` | `playstats_url` está vacío. |
| `Token not set` | `playstats_token` está vacío. |
| `Cannot reach ...` | La dirección no respondió. Revisa la dirección, la red y si el sitio está en ejecución. |
| `Unknown identifier` | El sitio no tiene ningún servidor de juego con el identificador del token. |
| `Wrong key` | El identificador existe, pero la clave no coincide. Genera una nueva clave en el sitio. |
| `Token not accepted` | El token no tiene el formato `<identifier>:<key>`. |
| `This game server is disabled on the site` | Un administrador desactivó el servidor en el sitio. |
| `... answered HTTP ...` | La dirección responde, pero no es L4D2 PlayStats Hub. Revisa `playstats_url`. |

Por RCON solo vuelven las primeras líneas, porque la respuesta del sitio llega después de que el comando ya terminó. Usa la consola del servidor, o ejecuta el comando dentro del juego como administrador root.

Los mismos problemas se escriben una vez en el registro de errores de SourceMod (`addons/sourcemod/logs/errors_*.log`) cuando empiezan.

En el sitio, abre el área de administración, luego los servidores de juego y luego el servidor. Su estado muestra En espera, En línea, Fuera de línea o Clave inválida, y una lista de verificación: conexión, plugin de envío de estadísticas cargado, escritura de estadísticas activada, primer archivo de estadísticas recibido, datos en vivo llegando, y el plugin de skins cuando la tienda está activada.

## Cargarlos en un modo de partida de Confogl o ZoneMod

Cuando un modo de partida se inicia, Confogl descarga todos los plugins y carga solo los que están listados en los archivos indicados en `confogl_match_execcfg_plugins`. Por eso, cada plugin que quieras durante una partida necesita una línea allí.

1. En la lista de plugins del modo de partida, por ejemplo `cfg/cfgogl/zonemod/confogl_plugins.cfg`, agrega estas líneas antes de `sm plugins load confoglcompmod.smx`:

   ```
   sm plugins load l4d2_playstats_sync.smx
   sm plugins load optional/l4d2_gameinfo_sync.smx
   sm plugins load optional/l4d2_ranking.smx
   sm plugins load optional/l4d2_show_patent_icon.smx
   sm plugins load optional/l4d2_weapon_skins.smx
   ```

   Omite los que no uses. El propio `l4d2_playstats` también debe estar en la lista: `sm plugins load optional/l4d2_playstats.smx`.

2. En los cvars del modo de partida, por ejemplo `cfg/cfgogl/zonemod/zonemod.cfg`, activa la escritura de estadísticas y define los demás cvars de los plugins que quieras:

   ```
   confogl_addcvar sm_stats_writestats 1
   confogl_addcvar ranking_show_motd_panel 1
   confogl_addcvar weapon_skins_enabled 1
   ```

3. Deja `playstats_url` y `playstats_token` en `cfg/server.cfg` o en `cfg/playstats.cfg`, no en el modo de partida. Sus valores se mantienen mientras los plugins se descargan y se vuelven a cargar. No uses `confogl_addcvar` para ellos, porque Confogl devuelve a su valor anterior cualquier cvar que controla cuando termina el modo de partida.

## Migrar desde la configuración anterior

Antes, los plugins se comunicaban con una API y un sitio web separados. Si el servidor tiene esa configuración:

- Quita `playstats_access_token`, `l4d2_web_url`, `l4d2_web_secret` y `ranking_url` de todos los archivos `.cfg`. Estos cvars ya no existen.
- Apunta `playstats_url` al nuevo sitio y define `playstats_token`.
- Si el panel del ranking usaba una dirección HTTP simple en `ranking_url`, pon ese tipo de dirección en `playstats_ingame_url`.
- Reemplaza los cinco archivos `.smx` y los archivos de traducción.

## Qué llamadas hacen los plugins

Cada solicitud lleva el encabezado `Authorization: <playstats_token>`. Los valores SteamID64 viajan como cadenas de texto.

| Plugin | Solicitud |
|---|---|
| `l4d2_playstats_sync` | `POST /api/servers/heartbeat`, `POST /api/statistics` |
| `l4d2_gameinfo_sync` | `PUT /api/game-info/configuration`, `PUT /api/game-info/round`, `PUT /api/game-info/scoreboard`, `PUT /api/game-info/players`, `PUT /api/game-info/messages`, `POST /api/game-info/events`, `POST /api/game-info/player-connection-info`, `GET /api/game-info/server-command/dequeue`, `GET /api/external-chat` |
| `l4d2_ranking` | Ninguna. Abre `/ingame?steamid=...` en el juego del jugador. |
| `l4d2_show_patent_icon` | `GET /api/players/patents` |
| `l4d2_weapon_skins` | `GET /api/skins`, `GET /api/players/skins`, `GET /api/players/{id}/wallet`, `POST /api/players/{id}/skins/equip` |

El heartbeat se envía cuando los archivos de configuración ya se ejecutaron, luego cada 30 segundos mientras el servidor no está hibernando, y justo después de que cambia `playstats_url` o `playstats_token`.

Los archivos de estadísticas se envían uno a la vez, diez segundos después del inicio de cada ronda. Un archivo para el que el sitio respondió "todavía no está listo" se vuelve a enviar solo después de que cambia, o después de 30 minutos. Un archivo se borra del servidor de juego solo cuando el sitio lo indica.

El código compartido está en `scripting/include/l4d2_playstats_hub.inc`: los dos cvars, la solicitud con el token y el heartbeat.

## Compilación

Necesitas el compilador de SourceMod 1.12, `spcomp`, que viene en el paquete de SourceMod dentro de `addons/sourcemod/scripting`. Los archivos include de terceros ya están en `scripting/include`.

Windows:

```powershell
./build.ps1
```

Linux y macOS:

```bash
./build.sh
```

Ambos compilan todos los archivos `.sp` de `scripting/` en `compiled/`, que Git ignora.

Los scripts buscan el compilador en el `PATH`, y `build.ps1` también busca en `C:\Program Files\SourceMod`. Para indicar dónde está, define `SPCOMP`, o pasa `-Spcomp` a `build.ps1`:

```powershell
./build.ps1 -Spcomp "C:\sourcemod\addons\sourcemod\scripting\spcomp.exe"
```

```bash
SPCOMP=~/sourcemod/addons/sourcemod/scripting/spcomp64 ./build.sh
```

Los archivos include estándar de SourceMod se toman de la carpeta `include` que está junto al compilador. Define `SOURCEMOD_INCLUDE` si están en otro lugar.

Cada push y cada pull request que toca `plugins/` compila los plugins en GitHub y conserva los archivos `.smx` como un artefacto de la ejecución.

Reglas de código: sin comentarios, constantes como `#define` al inicio del archivo, y los archivos include de terceros se quedan exactamente como vinieron.

## Licencia

Los plugins se distribuyen bajo la [GNU General Public License, versión 3](LICENSE), como lo exige SourceMod. [NOTICE.md](NOTICE.md) lista los archivos include de terceros y de dónde vienen.
