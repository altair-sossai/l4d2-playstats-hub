# Plugins del servidor de juego

🌐 [English](README.md) | [Português](README.pt.md) | **Español**

Los cinco plugins de SourceMod que conectan un servidor de Left 4 Dead 2 con L4D2 PlayStats Hub. Para instalarlos, copia todo lo que hay dentro de la carpeta `left4dead2` de este directorio (o del zip de la versión) en la carpeta `left4dead2` de tu servidor de juego, y luego define dos valores en `cfg/playstats.cfg`. Para una guía paso a paso, desde registrar el servidor en el sitio hasta la primera partida, consulta [Conectar un servidor de juego](../docs/game-server.es.md).

## Contenido

- [Qué hay en esta carpeta](#qué-hay-en-esta-carpeta)
- [Los plugins](#los-plugins)
- [Requisitos](#requisitos)
- [Instalación](#instalación)
- [Ajustes](#ajustes)
- [Modos de partida de L4D2 Competitive Rework](#modos-de-partida-de-l4d2-competitive-rework)
- [Actualizar](#actualizar)
- [Archivos de skins de armas y de patentes](#archivos-de-skins-de-armas-y-de-patentes)
- [Comandos](#comandos)
- [Verificar la instalación](#verificar-la-instalación)
- [Solución de problemas](#solución-de-problemas)
- [Qué llamadas hacen los plugins](#qué-llamadas-hacen-los-plugins)
- [Compilación](#compilación)
- [Aún no probado en un servidor de juego](#aún-no-probado-en-un-servidor-de-juego)
- [Licencia](#licencia)

## Qué hay en esta carpeta

La carpeta `left4dead2` tiene la misma estructura que la carpeta del juego de un servidor, así que cada archivo queda en su lugar al copiarla:

```text
plugins/
├── left4dead2/                      copia lo que hay dentro en la carpeta left4dead2 del servidor
│   ├── addons/sourcemod/
│   │   ├── plugins/                 l4d2_gameinfo_sync y l4d2_weapon_skins (.smx)
│   │   │   └── optional/            los otros tres (.smx), que cargan los modos de partida
│   │   ├── scripting/               sus fuentes (.sp) y los archivos include para compilarlos
│   │   ├── translations/            textos del chat: inglés, pt, es, chi y zho
│   │   └── configs/weapon_skins.cfg las armas y skins que conoce el plugin de skins
│   └── cfg/playstats.cfg            tu configuración: el único archivo que editas
├── build.ps1, build.sh              compilan los plugins en left4dead2/addons/sourcemod/plugins y en su carpeta optional
├── README.md, LICENSE, NOTICE.md    esta guía y la licencia (no van al servidor)
```

La carpeta del juego del servidor es la que tiene `gameinfo.txt`, `addons/` y `cfg/` dentro, por ejemplo `/home/steam/l4d2/left4dead2/` o `C:\l4d2\left4dead2\`.

Los archivos `.smx` del repositorio son siempre el resultado de compilar las fuentes que están a su lado: cada push lo verifica (consulta [Compilación](#compilación)).

## Los plugins

| Plugin | Qué hace | Módulo del sitio | Necesita |
|---|---|---|---|
| `l4d2_playstats_sync` | Le avisa al sitio cada 30 segundos que el servidor está activo y sube los archivos de estadísticas que escribe `l4d2_playstats`. Tiene el comando `sm_playstats_test`. Todo servidor lo necesita. | Siempre activo | REST in Pawn |
| `l4d2_gameinfo_sync` | Envía la partida en vivo: configuración, ronda, marcador, jugadores, eventos y chat. Lleva el chat del sitio al juego y ejecuta los comandos enviados desde el sitio. | Servidor en vivo, Chat del sitio, Comandos remotos | REST in Pawn, left4dhooks |
| `l4d2_ranking` | Abre la página del ranking dentro del juego con `!ranking` y la página de comandos con `!comandos` o `!commands`, y el ranking por sí solo para los jugadores que entran antes de que empiece una partida. | Ranking | REST in Pawn, left4dhooks |
| `l4d2_show_patent_icon` | Muestra el ícono de patente de cada jugador sobre su cabeza antes de que empiece una partida. | Ranking | REST in Pawn, left4dhooks, los sprites de patentes |
| `l4d2_weapon_skins` | Aplica las skins de armas que los jugadores compraron en la tienda. | Tienda de skins de armas | REST in Pawn, los modelos y materiales de las skins |

Solo `l4d2_playstats_sync` es obligatorio. Cada uno de los demás funciona sin los otros; borra el `.smx` de un plugin que no quieras antes de copiar la carpeta, o después.

## Requisitos

| Qué | Lo necesita | Viene con L4D2 Competitive Rework | Dónde conseguirlo |
|---|---|---|---|
| Servidor dedicado de Left 4 Dead 2 | Todos | No | SteamCMD, app 222860 |
| Metamod:Source y SourceMod 1.12 | Todos | Sí | <https://www.sourcemod.net/downloads.php?branch=stable> |
| Extensión REST in Pawn 1.3 (`rip.ext`) | Los cinco | **No, la agregas tú** | <https://github.com/ErikMinekus/sm-ripext/releases> |
| Left 4 DHooks Direct (`left4dhooks`) | `l4d2_gameinfo_sync`, `l4d2_ranking`, `l4d2_show_patent_icon` | Sí | <https://github.com/SilvDev/Left4DHooks/releases> |
| `l4d2_playstats` | Los archivos de estadísticas que sube `l4d2_playstats_sync` | Sí, en `plugins/optional/` | [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) |
| `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod` o `l4d2_hybrid_scoremod_zone`, `l4d2_skill_detect` | Opcional: más datos en vivo en `l4d2_gameinfo_sync` (ready-up, pausas, porcentajes de los jefes, bonus, skeets y otros eventos) | Sí, en `plugins/optional/` | [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) |
| `l4d2_mix`, `l4d2_anti_cheat` | Opcional: el evento "mix started" y el estado del anti-cheat en `l4d2_gameinfo_sync` | No | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) |
| Sprites de patentes, modelos y materiales de skins, y un FastDL | `l4d2_show_patent_icon`, `l4d2_weapon_skins` | No | [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets), consulta [Archivos de skins de armas y de patentes](#archivos-de-skins-de-armas-y-de-patentes) |
| Un servidor de juego registrado en el sitio | Todos | No | El sitio: **Administración → Servidores de juego** |

Los plugins se compilan con SourceMod 1.12.0.7260 y necesitan SourceMod 1.12 o más reciente. Los plugins opcionales se buscan por nombre mientras el servidor funciona: cuando uno de ellos no está cargado, `l4d2_gameinfo_sync` sigue funcionando y solo deja fuera esa parte. Los archivos include de `scripting/include` solo se necesitan para compilar; `l4d2util` y `colors` quedan compilados dentro de los plugins y no necesitan nada en el servidor.

Para instalar REST in Pawn, descarga `sm-ripext-<version>-linux.zip` o `-windows.zip` y extráelo en la carpeta `left4dead2` del servidor. Agrega `addons/sourcemod/extensions/rip.ext.so` (o `.dll`) y `addons/sourcemod/configs/ripext/ca-bundle.crt`, la lista de autoridades de certificación en las que confía para HTTPS. SourceMod la carga por sí solo cuando un plugin la necesita.

## Instalación

1. Instala lo que indican los [requisitos](#requisitos). En un servidor con L4D2 Competitive Rework, solo falta REST in Pawn.
2. Copia todo lo que hay dentro de `left4dead2/` en la carpeta `left4dead2` del servidor. Nada de esa carpeta reemplaza un archivo de SourceMod ni de L4D2 Competitive Rework, salvo archivos include de `addons/sourcemod/scripting/include` (consulta [Actualizar](#actualizar)).
3. En el sitio, abre **Administración → Servidores de juego**, crea el servidor y copia las dos líneas que muestra el sitio (`playstats_url` y `playstats_token`).
4. Abre `cfg/playstats.cfg` en el servidor y pega las dos líneas donde lo indica el archivo.
5. Agrega esta línea al final de `cfg/server.cfg`: `exec playstats.cfg`.
6. En un servidor con L4D2 Competitive Rework, agrega los plugins a `cfg/sharedplugins.cfg` (consulta [Modos de partida de L4D2 Competitive Rework](#modos-de-partida-de-l4d2-competitive-rework)).
7. Reinicia el servidor de juego.
8. Ejecuta `sm_playstats_test` en la consola del servidor (consulta [Verificar la instalación](#verificar-la-instalación)).

Copia la carpeta tal como está: `l4d2_gameinfo_sync` y `l4d2_weapon_skins` van en `addons/sourcemod/plugins/`, que SourceMod carga cuando arranca el servidor, y los otros tres en `addons/sourcemod/plugins/optional/`, que solo carga un modo de partida (consulta [Modos de partida de L4D2 Competitive Rework](#modos-de-partida-de-l4d2-competitive-rework)). Es donde los servidores con L4D2 Competitive Rework ya los guardan, así que un servidor que usaba las versiones anteriores sigue cargándolos con las líneas que ya tiene. Mientras no haya un modo de partida cargado, `l4d2_playstats_sync` no funciona y el sitio muestra el servidor como desconectado; para mantenerlo conectado, agrega `sm plugins load optional/l4d2_playstats_sync.smx` al final de `cfg/server.cfg`.

## Ajustes

Todos los ajustes de los cinco plugins están en `cfg/playstats.cfg`, con una explicación encima de cada línea. Solo hay que cambiar la primera parte de ese archivo; el resto está desactivado con `//` y muestra el valor por defecto.

Debes definir:

| Cvar | Plugin | Por defecto | Valores | Qué hacer |
|---|---|---|---|---|
| `playstats_url` | Los cinco | vacío | Dirección del sitio con `https://` o `http://`, sin barra al final | Pega la línea que muestra el sitio |
| `playstats_token` | Los cinco | vacío | `<identifier>:<key>`, tal como lo muestra el sitio | Pega la línea que muestra el sitio. Mantenlo en secreto |
| `sm_stats_writestats` | `l4d2_playstats` (Competitive Rework) | `0` | `0` desactivado, `1` escribe los archivos de estadísticas, `2` además escribe tablas legibles | Deja el `1` que define `cfg/playstats.cfg`, salvo que un modo de partida ya lo defina |

Puedes cambiar (cada línea está desactivada en el archivo, y el plugin usa el valor por defecto):

| Cvar | Plugin | Por defecto | Valores | Significado |
|---|---|---|---|---|
| `playstats_public_url` | `l4d2_ranking`, `l4d2_weapon_skins` | vacío | Una dirección, sin barra al final | Dirección del sitio que los jugadores leen en el chat: el aviso del ranking y el enlace de la tienda de skins. Vacío significa `playstats_url`. Ponla cuando los plugins llegan al sitio por una dirección IP pero los jugadores lo conocen por un nombre, por ejemplo `https://stats.example.com`. |
| `playstats_ingame_url` | `l4d2_ranking` | vacío | Una dirección, sin barra al final | Dirección que abre el navegador del juego. Vacío significa `playstats_url`. Pon una dirección `http://` simple cuando el panel se queda en blanco (ver más abajo). |
| `ranking_show_motd_panel` | `l4d2_ranking` | `1` | `0` o `1` | `0` hace que el plugin no abra la página del ranking, ni siquiera con `!ranking`. |
| `ranking_commands_path` | `l4d2_ranking` | `/ingame/commands` | Una ruta que empieza con `/` | Página que abren `!comandos` y `!commands`, desde la raíz del sitio. |
| `patent_icon_enabled` | `l4d2_show_patent_icon` | `0` | `0` o `1` | Activa los íconos de patente. Con `0` el plugin no pide las patentes al sitio y los jugadores no descargan los sprites. Actívalo solo cuando los sprites estén en el FastDL. |
| `patent_icon_version` | `l4d2_show_patent_icon` | `2` | `1` o más | Versión en los nombres de los archivos de sprites, `patent_<level>_v<version>`, ambos con dos dígitos. Los archivos del repositorio de assets son la versión `2`. |
| `patent_icon_max_level` | `l4d2_show_patent_icon` | `15` | `1` o más | Nivel de patente más alto que tiene un archivo de sprite. |
| `gameinfo_related_accounts_chat` | `l4d2_gameinfo_sync` | `1` | `0` o `1` | Avisa en el chat cuando un jugador se conecta desde la misma dirección IP que otra cuenta vista antes. `0` solo lo muestra en la consola. El sitio tiene su propio ajuste para quién ve estas cuentas en la página del jugador ([Configuración > Privacidad](../docs/manual/administration.es.md#privacidad)). |
| `gameinfo_boomer_vomit_min_survivors` | `l4d2_gameinfo_sync` | `3` | `1` o más | Mínimo de supervivientes que un boomer debe vomitar para que el evento llegue al sitio. |
| `weapon_skins_enabled` | `l4d2_weapon_skins` | `0` | `0` o `1` | Activa las skins. Actívalo solo cuando los archivos de las skins estén en el servidor y en el FastDL. |
| `weapon_skins_kick_no_downloads` | `l4d2_weapon_skins` | `0` | `0` o `1` | Expulsa a los jugadores cuyo juego se niega a descargar los archivos del servidor (`cl_allowdownload 0`, o `cl_downloadfilter` distinto de `all` o `nosounds`). Con `0` juegan, y mientras uno de ellos esté en el servidor las skins quedan desactivadas en tercera persona para todos. |
| `weapon_skins_pile_radius` | `l4d2_weapon_skins` | `120.0` | `0` o más | Radio, en unidades del juego, usado para encontrar armas principales tiradas cerca unas de otras. |
| `weapon_skins_pile_max` | `l4d2_weapon_skins` | `3` | `0` o más | Máximo de armas principales tiradas que se mantienen dentro de ese radio; las más antiguas se eliminan. `0` lo desactiva. |
| `weapon_skins_preview_all` | `l4d2_weapon_skins` | `0` | `0` o `1` | Modo de prueba: todas las skins se descargan a partir del próximo mapa, y los admins root pueden usar cualquiera sin comprarla y fuera del ready-up. Deja `0` en un servidor público. |

Dónde definirlos:

- En `cfg/playstats.cfg`, ejecutado desde `cfg/server.cfg` con `exec playstats.cfg`. El juego ejecuta `server.cfg` cada vez que carga un mapa, así que los valores se aplican de nuevo después de que un modo de partida recarga los plugins. Este es el lugar para todo ajuste que es igual en todos los modos de partida.
- Con `confogl_addcvar` en la configuración de un modo de partida, por ejemplo `cfg/cfgogl/zonemod/zonemod.cfg`, solo para un ajuste que debe ser distinto entre modos de partida. Entonces borra esa línea de `cfg/playstats.cfg`: un ajuste en los dos lugares cambia de un valor a otro en cada mapa, y Confogl muestra cada cambio en el chat.
- Nunca pongas `playstats_url` ni `playstats_token` en un modo de partida ni bajo `confogl_addcvar`: Confogl muestra en el chat el valor anterior y el nuevo de los ajustes que vigila, lo que mostraría el token a los jugadores.

La cvar del token está protegida: el juego nunca envía su valor a los jugadores, y el `sm_cvar` de SourceMod solo lo muestra a los admins con el flag de contraseña. Los plugins nunca lo muestran.

La página que abre `l4d2_ranking` es `<address>/ingame?steamid=<SteamID64 of the player>`. El navegador del juego es Chromium 18, de 2012, y no se espera que abra sitios HTTPS actuales. Si el panel se queda en blanco, sirve el sitio por HTTP simple en otra dirección o puerto y pon esa dirección en `playstats_ingame_url`. Consulta [docs/in-game-browser.es.md](../docs/in-game-browser.es.md).

`l4d2_gameinfo_sync` envía al sitio la dirección IP de cada jugador que se conecta, y el sitio la usa para encontrar cuentas que comparten una dirección. También ejecuta, como comandos del servidor, los comandos que los administradores del sitio envían desde la página **Comandos remotos**: quien controle una cuenta de administrador del sitio puede ejecutar cualquier comando del servidor, como con RCON. También ejecuta el `sm_aac sync` que el sitio pone en cola después de un cambio en los ajustes del Anti-Cheat, incluso con el módulo de comandos remotos desactivado.

## Modos de partida de L4D2 Competitive Rework

Cuando carga un modo de partida, el gestor de plugins de Confogl descarga todos los plugins y carga solo los que aparecen en los archivos indicados por `confogl_match_execcfg_plugins`. En L4D2 Competitive Rework esa cvar se define en `cfg/server.cfg` como `generalfixes.cfg;confogl_plugins.cfg;sharedplugins.cfg`, y `cfg/sharedplugins.cfg` es el archivo pensado para tus propios plugins, cargado por todos los modos de partida.

Agrega estas líneas a `cfg/sharedplugins.cfg`, dejando fuera los plugins que no uses:

```text
sm plugins load optional/l4d2_playstats_sync.smx
sm plugins load l4d2_gameinfo_sync.smx
sm plugins load optional/l4d2_ranking.smx
sm plugins load optional/l4d2_show_patent_icon.smx
sm plugins load l4d2_weapon_skins.smx
```

Sin esas líneas los tres plugins de `optional/` nunca se cargan, y los dos de `addons/sourcemod/plugins/` se detienen cuando se carga un modo de partida.

Los modos de partida de L4D2 Competitive Rework ya cargan `optional/l4d2_playstats.smx` (en `confogl_plugins.cfg`) y los plugins opcionales que usa `l4d2_gameinfo_sync`. Los modos ZoneMod no definen `sm_stats_writestats`, por eso `cfg/playstats.cfg` lo pone en `1`. Si los modos de tu servidor lo definen con `confogl_addcvar`, borra esa línea de `cfg/playstats.cfg`.

En el sitio, **Administración → Modos de juego** decide qué modos de partida cuentan para el ranking. Las estadísticas se envían desde todos los modos.

## Actualizar

Copia de nuevo la carpeta `left4dead2` nueva sobre la anterior, excepto `cfg/playstats.cfg`, que guarda tu token. Luego reinicia el servidor mientras está vacío, para que cada plugin cargue en su nueva versión.

| Archivo | Se puede reemplazar al actualizar |
|---|---|
| `addons/sourcemod/plugins/*.smx` y `addons/sourcemod/plugins/optional/*.smx` | Sí. |
| `addons/sourcemod/translations/**` | Sí, salvo que hayas editado los textos. |
| `addons/sourcemod/configs/weapon_skins.cfg` | Sí, salvo que hayas agregado armas o skins propias. |
| `addons/sourcemod/scripting/**` | Sí para el servidor: solo se usa para compilar. Reemplaza archivos include con el mismo nombre, como `left4dhooks.inc` o `readyup.inc`; si compilas otros plugins en el servidor, no copies esta carpeta. |
| `cfg/playstats.cfg` | **No.** Tiene tu dirección y tu token. Sáltalo, o cópialo de nuevo y vuelve a pegar tus dos líneas. |

En Windows, cuando la copia pregunte por archivos que ya existen, reemplázalos y omite `playstats.cfg`. En Linux:

```bash
rsync -a --exclude cfg/playstats.cfg left4dead2/ /path/to/server/left4dead2/
```

El plugin de skins guarda dos archivos de caché en `addons/sourcemod/data/` (`weapon_skins_cache.json` y `weapon_skins_players_cache.json`), para poder aplicar las skins mientras el sitio está caído. La actualización no los toca.

## Archivos de skins de armas y de patentes

`l4d2_show_patent_icon` y `l4d2_weapon_skins` muestran archivos que no forman parte del juego, así que los jugadores deben descargarlos. Están en [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets):

| Archivos | En el servidor de juego | En el FastDL |
|---|---|---|
| `models/l4d2br/weapons/<weapon>/` (un modelo combinado por arma) | Sí, en la carpeta del juego | Sí |
| `materials/models/l4d2br/weapons/<weapon>_<id>/` (las texturas de cada skin) | Sí, en la carpeta del juego | Sí |
| `materials/sprites/patent_<level>_v02.vmt` y `.vtf` (íconos de patente), que también vienen en este paquete de plugins | Sí, en la carpeta del juego | Sí |

1. Copia `models/` y `materials/` del repositorio de assets en la carpeta `left4dead2` del servidor. Cada ruta `view_model`, `world_model`, `models` y `materials` de `addons/sourcemod/configs/weapon_skins.cfg` debe existir allí; cuando una no existe, el plugin omite esa arma o skin y, con las skins activadas, registra `models were not found on the server` o `materials were not found on the server`.
2. Pon `models/`, `materials/` y el contenido de `fastdl/` en un servidor web al que lleguen los jugadores, en una carpeta con la misma estructura que la carpeta del juego. Los archivos se sirven tal cual, sin `.bz2`.
3. Indícale al juego esa dirección en `cfg/server.cfg`:

   ```text
   sm_cvar sv_allowdownload 1
   sm_cvar sv_downloadurl "https://files.example.com/fastdl"
   ```

4. Activa las skins con `weapon_skins_enabled 1` en `cfg/playstats.cfg`, y reinicia el servidor mientras está vacío, para que los jugadores descarguen los archivos al conectarse y no durante un cambio de mapa.

Sin `sv_downloadurl`, el propio juego envía los archivos, muy despacio; con modelos grandes, los jugadores pueden desistir antes de terminar. Los plugins agregan los archivos a la lista de descargas en cada mapa: los sprites de patentes siempre, y para las skins, los modelos de todas las armas y los materiales de las skins que alguien compró (todas con `weapon_skins_preview_all 1`).

Los jugadores necesitan `cl_allowdownload 1` y `cl_downloadfilter all` (o `nosounds`) en su juego para descargar los archivos. `weapon_skins_kick_no_downloads 1` expulsa a quienes se niegan, con un mensaje que les dice qué cambiar.

Las skins que un jugador puede elegir vienen del sitio: `weapon_skins.cfg` asocia el nombre de cada skin con su arma, su familia del modelo y su carpeta de materiales, y la tienda del sitio vende las skins con los mismos nombres.

## Comandos

| Comando | Quién | Qué hace |
|---|---|---|
| `sm_playstats_test` | Consola del servidor, o admins con el flag root | Llama al sitio en el momento y muestra el resultado (ver más abajo). |
| `sm_ranking` (`!ranking`) | Todos | Abre la página del ranking dentro del juego. |
| `sm_comandos`, `sm_commands` (`!comandos`, `!commands`) | Todos | Abre la página con los comandos del servidor dentro del juego. |
| `sm_skin`, `sm_skins` (`!skin`) | Todos | `!skin` muestra la ayuda; `!skin next`, `!skin prev`, `!skin <name>` y `!skin default` eligen la skin del arma en la mano, o del arma indicada después; `!skin status` muestra las monedas y las skins que tienes. Los supervivientes cambian de skin durante el ready-up. |
| `sm_skin on`, `sm_skin off`, `sm_skin test <skin\|default> [weapon]` | Admins con el flag root; `on` y `off` también desde la consola del servidor | `on` y `off` reemplazan `weapon_skins_enabled` hasta que el plugin se carga de nuevo (un reinicio, o un modo de partida que recarga los plugins). `test` prueba una skin en ti mismo hasta que cambia el mapa, sin comprarla. El acceso se puede cambiar con el override `sm_weaponskins_admin` en `addons/sourcemod/configs/admin_overrides.cfg`. |

## Verificar la instalación

1. En la consola del servidor, ejecuta `sm exts list`. Una línea debe ser `REST in Pawn`. Si falta, REST in Pawn no está instalado, o no es la versión para tu sistema.
2. Ejecuta `sm plugins list`. Debe mostrar `L4D2 - Player Statistics Sync` y los demás plugins que copiaste, ninguno marcado como fallido. Cuando un plugin no puede cargar, SourceMod lo muestra con el motivo, por ejemplo una extensión o un plugin necesario que no está funcionando.
3. Ejecuta `sm_playstats_test`. Primero muestra el resultado de la última llamada automática, luego llama al sitio y muestra una de las líneas de abajo.
4. En el sitio, abre **Administración → Servidores de juego** y el servidor. Su estado cambia por sí solo a **Conectado**, con una lista de verificación: conexión con este sitio, plugin de envío de estadísticas cargado, escritura de estadísticas activada, primer archivo de estadísticas recibido (después del primer mapa completo), datos de la partida en vivo llegando, y el plugin de skins cuando la tienda está activada.

| Resultado de `sm_playstats_test` | Significado |
|---|---|
| `Connected to the site "..." as the game server "..."` | Todo está bien. Las líneas siguientes muestran los módulos activados en el sitio, los plugins cargados, si la escritura de estadísticas está activada, y cuántos archivos de estadísticas están esperando. |
| `URL not set` | `playstats_url` está vacío. |
| `Token not set` | `playstats_token` está vacío. |
| `Cannot reach ...` | La dirección no respondió. El texto entre paréntesis es el error de la conexión. |
| `Unknown identifier` | El sitio no tiene ningún servidor de juego con el identificador del token. |
| `Wrong key` | El identificador existe, pero la clave no coincide. |
| `Token not accepted` | El token no tiene la forma `<identifier>:<key>`. |
| `This game server is disabled on the site` | Un administrador desactivó el servidor en el sitio. |
| `... answered HTTP ...` | Algo responde en esa dirección, pero no es L4D2 PlayStats Hub. |

Por RCON solo vuelven las primeras líneas, porque la respuesta del sitio llega después de que el comando terminó. Usa la consola del servidor, o ejecuta el comando dentro del juego como admin root.

Los plugins escriben un problema una sola vez en el registro de errores de SourceMod (`addons/sourcemod/logs/errors_<date>.log`) cuando empieza, y una línea en el registro de SourceMod (`addons/sourcemod/logs/L<date>.log`) cuando se resuelve, para que un sitio caído no llene los registros.

## Solución de problemas

| Lo que ves | Causa y solución |
|---|---|
| `sm_playstats_test` es un comando desconocido | `l4d2_playstats_sync.smx` no está cargado. Verifica que esté en `addons/sourcemod/plugins/optional/`, y busca su error en `sm plugins list`. Con L4D2 Competitive Rework, revisa `cfg/sharedplugins.cfg`. |
| `sm plugins list` dice que un plugin necesita la extensión REST in Pawn (`rip.ext`) | Falta REST in Pawn, o es la versión de otro sistema. Instala el zip correcto y reinicia el servidor. |
| `sm plugins list` dice que un plugin necesita el plugin `left4dhooks` | `left4dhooks.smx` no está cargado. Instala Left 4 DHooks Direct con sus archivos gamedata. |
| `URL not set` o `Token not set` | Las líneas no se ejecutaron. Verifica que `cfg/server.cfg` termine con `exec playstats.cfg`, que las dos líneas de `cfg/playstats.cfg` no empiecen con `//`, y reinicia el servidor. |
| `Unknown identifier`, `Wrong key` o `Token not accepted` | Pega de nuevo el token desde la página del servidor en el sitio. Si se perdió la clave, haz clic allí en **Generar una nueva clave** y pega la línea nueva. |
| `Cannot reach ...` con un timeout o "Couldn't resolve host" | El servidor de juego no puede abrir el sitio. Desde la máquina del servidor de juego, ejecuta `curl -I <playstats_url>`. Revisa la dirección, el DNS, el firewall, y que el sitio esté funcionando. |
| `Cannot reach ...` con "SSL certificate problem" | El certificado del sitio no es de confianza. Usa un certificado de una autoridad pública, como Let's Encrypt; los certificados autofirmados no funcionan. Si el certificado está bien, `addons/sourcemod/configs/ripext/ca-bundle.crt` puede estar desactualizado: reemplázalo por el de la última versión de REST in Pawn. |
| `... answered HTTP 301`, `302`, `404` o `405` | La dirección no es el propio sitio, o redirige. Usa la dirección final, normalmente con `https://`: una redirección puede convertir el POST del plugin en un GET. |
| Conectado, pero no llega ningún archivo de estadísticas | Juega un mapa completo en versus. Verifica que `l4d2_playstats` esté cargado en ese modo de partida y que `sm_playstats_test` diga que la escritura de estadísticas está activada. |
| Las partidas llegan pero no cuentan para el ranking | En el sitio, abre **Administración → Modos de juego** y mira en qué regla cae la configuración. |
| Los plugins funcionan fuera de las partidas pero se detienen cuando carga un modo de partida | No están en `cfg/sharedplugins.cfg` (consulta [Modos de partida de L4D2 Competitive Rework](#modos-de-partida-de-l4d2-competitive-rework)). |
| Confogl muestra "Tracked Server CVar ... changed" en cada mapa | El mismo ajuste está en `cfg/playstats.cfg` y bajo `confogl_addcvar`. Déjalo en un solo lugar. |
| `!ranking` abre un panel vacío | Pon en `playstats_ingame_url` una dirección HTTP simple del sitio. |
| El chat muestra la dirección IP del sitio en lugar de su nombre | Pon en `playstats_public_url` la dirección que conocen los jugadores, por ejemplo `https://stats.example.com`. |
| Los íconos de patente no aparecen | Revisa `patent_icon_enabled 1`. Después, puede que los jugadores no tengan los sprites: revisa `sv_downloadurl` y que `patent_icon_version` coincida con los nombres de los archivos. |
| Las skins nunca aparecen | Revisa `weapon_skins_enabled 1`, que el módulo esté activado en **Administración → Módulos**, que los archivos estén en el servidor (busca `not found on the server` en el registro de errores) y en el FastDL, y que la skin se haya comprado en el sitio. |

## Qué llamadas hacen los plugins

Cada solicitud lleva el encabezado `Authorization: <playstats_token>`. Los valores SteamID64 viajan como texto.

| Plugin | Solicitud |
|---|---|
| `l4d2_playstats_sync` | `POST /api/servers/heartbeat`, `POST /api/statistics` |
| `l4d2_gameinfo_sync` | `PUT /api/game-info/configuration`, `PUT /api/game-info/round`, `PUT /api/game-info/scoreboard`, `PUT /api/game-info/players`, `PUT /api/game-info/messages`, `POST /api/game-info/events`, `POST /api/game-info/player-connection-info`, `GET /api/game-info/server-command/dequeue`, `GET /api/external-chat` |
| `l4d2_ranking` | Ninguna. Abre `/ingame?steamid=...` y `/ingame/commands?steamid=...` en el juego del jugador. |
| `l4d2_show_patent_icon` | `GET /api/players/patents` |
| `l4d2_weapon_skins` | `GET /api/skins`, `GET /api/players/skins`, `GET /api/players/{id}/wallet`, `POST /api/players/{id}/skins/equip` |
| `l4d2-anti-cheat` (aparte, 1.5.0 o posterior, con `l4d2_web_url` y `l4d2_web_secret`) | `GET /api/anti-cheat-access/server`, `PUT /api/anti-cheat-access/server/mode`, `POST /api/anti-cheat-access/server/{list}`, `DELETE /api/anti-cheat-access/server/{list}?player=<steamid>` |

El heartbeat se envía cuando se ejecutaron los archivos de configuración, luego cada 30 segundos mientras el servidor no está hibernando, y justo después de que cambia `playstats_url` o `playstats_token`. Solo hay un heartbeat en curso a la vez, con los timeouts por defecto de REST in Pawn (10 segundos para conectar, 30 en total).

Los archivos de estadísticas se envían de uno en uno, diez segundos después de cada inicio de ronda. Cuando el sitio no responde, los archivos se quedan en el servidor y se intentan de nuevo en el siguiente inicio de ronda. Un archivo para el que el sitio respondió "not ready" se vuelve a enviar solo después de que cambia, o después de 30 minutos. Un archivo se borra del servidor de juego solo cuando el sitio lo indica.

`l4d2_gameinfo_sync` envía los datos en vivo cada 5 segundos, con un timeout de conexión de 3 segundos y un timeout total de 5 segundos. Una solicitud de configuración, ronda, marcador, jugadores, chat del sitio o comandos no se envía mientras la anterior del mismo tipo sigue esperando, así que un sitio lento no las acumula. Cuando un módulo está desactivado en el sitio, sus solicitudes reciben `404` y los plugins las ignoran; el plugin de skins lo registra una sola vez.

El código compartido está en `scripting/include/l4d2_playstats_hub.inc`: las dos cvars, la solicitud con el token, y el heartbeat.

## Compilación

Solo necesitas esto para modificar los plugins. El compilador es `spcomp`, del paquete de SourceMod (`addons/sourcemod/scripting/`). Los plugins del repositorio se compilan con **SourceMod 1.12.0.7260**; descarga esa versión de <https://github.com/alliedmodders/sourcemod/releases/tag/1.12.0.7260> y descomprímela en `plugins/.sourcemod/` (ignorada por Git), o indícala con `SPCOMP`.

Windows:

```powershell
./build.ps1
./build.ps1 -Spcomp "C:\sourcemod\addons\sourcemod\scripting\spcomp.exe"
```

Linux y macOS:

```bash
./build.sh
SPCOMP=~/sourcemod/addons/sourcemod/scripting/spcomp64 ./build.sh
```

Ambos compilan cada archivo `.sp` de `left4dead2/addons/sourcemod/scripting/` directamente en `left4dead2/addons/sourcemod/plugins/`, y `l4d2_playstats_sync`, `l4d2_ranking` y `l4d2_show_patent_icon` en su carpeta `optional/`. Haz commit de los archivos `.smx` junto con las fuentes. Los archivos include estándar de SourceMod se toman de la carpeta `include` que está junto al compilador; define `SOURCEMOD_INCLUDE` si están en otro lugar.

La compilación es reproducible: las mismas fuentes y el mismo compilador dan los mismos bytes en Windows y en Linux. Para lograrlo, los scripts compilan en una carpeta temporal con rutas de include relativas, fijan la fecha y la hora de compilación que SourceMod incluye (`__DATE__` y `__TIME__`), escriben los plugins sin comprimir (`-z0`, porque zlib no comprime igual en todos los sistemas) y cada plugin define el tamaño de su pila con `#pragma dynamic` (si no, la estimación del compilador cambia de una ejecución a otra). Cada plugin también se compila una vez con `-E`, así que cualquier advertencia hace fallar la compilación.

`./build.sh --check` (o `./build.ps1 -Check`) compila en la carpeta temporal y falla si un `.smx` del repositorio falta, es distinto del resultado, no tiene fuente, o no está en su carpeta. El workflow `Plugins` lo ejecuta en cada push y pull request que toca `plugins/`, con SourceMod 1.12.0.7260 verificado con su SHA-256, y guarda la carpeta `left4dead2` como artefacto de la ejecución. El workflow de versiones ejecuta la misma verificación y empaqueta la carpeta `left4dead2` del repositorio, esta guía, la licencia y el aviso en `l4d2-playstats-hub-plugins.zip`.

Reglas del código: sin comentarios, constantes como `#define` al inicio del archivo, y los archivos include de terceros se quedan exactamente como vinieron.

## Aún no probado en un servidor de juego

Los plugins compilan sin advertencias y la compilación se verificó en Windows y en Linux, pero esta versión todavía no se ejecutó en un servidor de juego real. Hasta que eso pase, toma estos puntos como el comportamiento esperado, no como hechos probados:

- los plugins cargan desde `addons/sourcemod/plugins/` y `addons/sourcemod/plugins/optional/` como archivos `.smx` sin comprimir (SourceMod los admite, pero ningún servidor cargó esta compilación);
- el flujo `server.cfg` → `exec playstats.cfg` → `cfg/sharedplugins.cfg` en un servidor con L4D2 Competitive Rework;
- los cambios de esta versión: el porcentaje del tank y de la witch sin `l4d_boss_percent`, la actualización de las patentes después de que se ejecutan las configuraciones, y las líneas del registro de errores escritas una sola vez;
- el texto exacto de los mensajes propios de SourceMod, que esta guía describe en lugar de citar.

## Licencia

Los plugins se publican bajo la [GNU General Public License, versión 3](LICENSE), como exige SourceMod. [NOTICE.md](NOTICE.md) lista los archivos include de terceros y de dónde vienen.
