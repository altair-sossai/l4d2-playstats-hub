# Conectar un servidor de juego

🌐 [English](game-server.md) | [Português](game-server.pt.md) | **Español**

Cómo hacer que un servidor de Left 4 Dead 2 envíe sus partidas a tu sitio, paso a paso. Repites estos pasos para cada servidor de juego; un sitio puede recibir cualquier cantidad de ellos. Cada ajuste, comando y archivo se describe en detalle en [la guía de los plugins](../plugins/README.es.md).

## Contenido

- [Lo que necesita el servidor de juego](#lo-que-necesita-el-servidor-de-juego)
- [Paso 1: registra el servidor en el sitio](#paso-1-registra-el-servidor-en-el-sitio)
- [Paso 2: instala REST in Pawn](#paso-2-instala-rest-in-pawn)
- [Paso 3: copia los plugins](#paso-3-copia-los-plugins)
- [Paso 4: define la dirección y el token](#paso-4-define-la-dirección-y-el-token)
- [Paso 5: carga los plugins en tus modos de partida](#paso-5-carga-los-plugins-en-tus-modos-de-partida)
- [Paso 6: verifica la conexión](#paso-6-verifica-la-conexión)
- [Opcional: skins de armas e íconos de patente](#opcional-skins-de-armas-e-íconos-de-patente)
- [Opcional: el plugin del Anti-Cheat](#opcional-el-plugin-del-anti-cheat)
- [El ranking dentro del juego](#el-ranking-dentro-del-juego)
- [El mix por el ranking de la comunidad](#el-mix-por-el-ranking-de-la-comunidad)
- [Qué significa cada estado](#qué-significa-cada-estado)
- [Actualizar los plugins](#actualizar-los-plugins)
- [Si vienes de la configuración anterior](#si-vienes-de-la-configuración-anterior)
- [Si algo sale mal](#si-algo-sale-mal)

## Lo que necesita el servidor de juego

Los plugins están hechos para servidores basados en [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), como los servidores ZoneMod. Competitive Rework ya trae casi todo lo que necesitan:

| Requisito | Por qué | Viene con Competitive Rework |
|---|---|---|
| SourceMod 1.12 o más reciente | Ejecuta los plugins. | Sí |
| Extensión [REST in Pawn](https://github.com/ErikMinekus/sm-ripext/releases) | Permite que los plugins llamen al sitio. | **No: paso 2** |
| [Left 4 DHooks Direct](https://github.com/SilvDev/Left4DHooks/releases) (`left4dhooks`) | Lo usan los plugins de partida en vivo, ranking y patentes. | Sí |
| Plugin `l4d2_playstats` | Escribe el archivo de estadísticas de cada mapa. | Sí |
| `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod`, `l4d2_skill_detect` | Opcional: más detalles en la partida en vivo. | Sí |

En un servidor sin Competitive Rework, instala primero SourceMod 1.12 y Left 4 DHooks Direct; las estadísticas necesitan `l4d2_playstats`, que solo viene con Competitive Rework. La lista completa, con enlaces, está en [los requisitos de la guía de los plugins](../plugins/README.es.md#requisitos).

## Paso 1: registra el servidor en el sitio

1. Inicia sesión en el sitio como administrador.
2. Abre **Administración → Servidores de juego**.
3. Haz clic en **Añadir**, escribe un nombre y haz clic en **Guardar**.

El sitio muestra dos líneas con la dirección del sitio y el token de este servidor:

```text
playstats_url "https://stats.example.com"
playstats_token "my-server:3f9c…"
```

Cópialas ahora. La clave dentro del token se muestra una sola vez. Si la pierdes, haz clic en **Generar una nueva clave** en la misma página; la clave anterior deja de funcionar en el acto.

## Paso 2: instala REST in Pawn

1. Descarga el último `sm-ripext-<version>-linux.zip` (servidor Linux) o `sm-ripext-<version>-windows.zip` (servidor Windows) de las [versiones de REST in Pawn](https://github.com/ErikMinekus/sm-ripext/releases).
2. Extráelo en la carpeta `left4dead2` del servidor, la que tiene `gameinfo.txt`. Agrega `addons/sourcemod/extensions/rip.ext.so` (o `.dll`) y `addons/sourcemod/configs/ripext/ca-bundle.crt`.

Sáltate este paso si `sm exts list`, en la consola del servidor, ya muestra `REST in Pawn`.

## Paso 3: copia los plugins

Consigue los plugins de una de estas dos formas:

- descarga `l4d2-playstats-hub-plugins.zip` de la [página de versiones](https://github.com/altair-sossai/l4d2-playstats-hub/releases) y extráelo, o
- descarga o clona [este repositorio](https://github.com/altair-sossai/l4d2-playstats-hub) y abre su carpeta `plugins`.

Ambos tienen una carpeta llamada `left4dead2` con la misma estructura que la carpeta del juego del servidor. **Copia todo lo que hay dentro de esa carpeta `left4dead2` en la carpeta `left4dead2` del servidor.** Cada archivo queda en su lugar:

| Archivo | Qué es |
|---|---|
| `addons/sourcemod/plugins/*.smx` | `l4d2_gameinfo_sync` y `l4d2_weapon_skins`. |
| `addons/sourcemod/plugins/optional/*.smx` | `l4d2_playstats_sync`, `l4d2_ranking` y `l4d2_show_patent_icon`, que cargan los modos de partida. |
| `addons/sourcemod/scripting/` | Sus fuentes, solo para compilar. |
| `addons/sourcemod/translations/` | Los textos del chat. |
| `addons/sourcemod/configs/weapon_skins.cfg` | Las armas y skins del plugin de skins. |
| `cfg/playstats.cfg` | Tus ajustes. Lo editas en el siguiente paso. |

Los tres plugins de `optional/` solo se cargan con un modo de partida (paso 5), que es donde los servidores con Competitive Rework ya los guardan. Para que el sitio vea el servidor mientras no se juega ninguna partida, el paso 4 también carga `l4d2_playstats_sync` desde `cfg/server.cfg`.

`l4d2_playstats_sync.smx` es el único que todo servidor necesita. Borra el `.smx` de los plugins que no quieras:

| Plugin | Qué hace | Módulo del sitio |
|---|---|---|
| `l4d2_playstats_sync` | Envía los archivos de estadísticas y le avisa al sitio que el servidor está activo. | Siempre necesario |
| `l4d2_gameinfo_sync` | Envía la partida en vivo: marcador, jugadores, eventos y chat. | Servidor en vivo |
| `l4d2_ranking` | Abre el ranking dentro del juego con `!ranking`. | Ranking |
| `l4d2_show_patent_icon` | Muestra el ícono de patente de cada jugador en el juego. | Ranking |
| `l4d2_weapon_skins` | Aplica las skins de armas que tienen los jugadores. | Tienda de skins de armas |

## Paso 4: define la dirección y el token

1. Abre `cfg/playstats.cfg` en el servidor.
2. Pega las dos líneas del paso 1 donde el archivo dice `Paste the two lines here`.
3. Abre `cfg/server.cfg` y agrega estas líneas al final:

   ```text
   exec playstats.cfg
   sm plugins load optional/l4d2_playstats_sync.smx
   ```

Es todo lo que debes cambiar. El resto de `cfg/playstats.cfg` lista todos los demás ajustes de los cinco plugins, desactivados con `//` y con su valor por defecto; cada uno tiene una explicación encima.

`cfg/playstats.cfg` también activa `sm_stats_writestats 1`, que hace que `l4d2_playstats` escriba los archivos de estadísticas. Su valor por defecto es `0`, y los modos de partida ZoneMod de Competitive Rework no lo cambian. Si tus modos de partida ya lo definen con `confogl_addcvar`, borra esa línea de `cfg/playstats.cfg`.

Cada ajuste va en un solo lugar: `cfg/playstats.cfg` para los que son iguales en todos los modos de partida, o `confogl_addcvar` en un modo de partida para uno que debe ser distinto. Nunca definas `playstats_url` ni `playstats_token` con `confogl_addcvar`: Confogl muestra en el chat el valor anterior y el nuevo de los ajustes que vigila, y los jugadores verían el token.

## Paso 5: carga los plugins en tus modos de partida

Cuando carga un modo de partida de Competitive Rework, descarga todos los plugins y carga solo los de sus listas. La lista compartida por todos los modos es `cfg/sharedplugins.cfg`. Agrégale estas líneas, dejando fuera los plugins que borraste:

```text
sm plugins load optional/l4d2_playstats_sync.smx
sm plugins load l4d2_gameinfo_sync.smx
sm plugins load optional/l4d2_ranking.smx
sm plugins load optional/l4d2_show_patent_icon.smx
sm plugins load l4d2_weapon_skins.smx
```

Luego reinicia el servidor de juego.

Las estadísticas se envían desde todos los modos. En el sitio, **Administración → Modos de juego** decide qué modos cuentan para el ranking.

## Paso 6: verifica la conexión

En el sitio, la página del servidor de juego se actualiza sola. En unos segundos debería decir **Conectado**, con una lista de lo que se encontró:

- Conexión con este sitio
- Plugin de envío de estadísticas cargado
- Escritura de estadísticas activada (se activa cuando un modo de partida carga `l4d2_playstats`)
- Primer archivo de estadísticas recibido (después del primer mapa completo)
- Datos de la partida en vivo llegando (cuando el módulo de servidor en vivo está activado)
- Plugin de skins de armas cargado (cuando el módulo de tienda de skins está activado)

También puedes verificarlo desde el servidor de juego. Escribe esto en su consola:

```text
sm exts list
sm plugins list
sm_playstats_test
```

`sm exts list` debe mostrar `REST in Pawn`, y `sm plugins list` debe mostrar los plugins que copiaste, ninguno marcado como fallido. `sm_playstats_test` responde con el nombre del servidor y del sitio, los módulos activados y los plugins cargados, o dice exactamente qué está mal. [La guía de los plugins](../plugins/README.es.md#verificar-la-instalación) explica cada respuesta.

## Opcional: skins de armas e íconos de patente

Los íconos de patente y las skins de armas son archivos que no forman parte del juego, así que los jugadores los descargan de tu servidor. Están en [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

1. Copia sus carpetas `models/` y `materials/` en la carpeta `left4dead2` del servidor.
2. Pon `models/`, `materials/` y el contenido de `fastdl/` en un servidor web al que lleguen los jugadores (un FastDL), con la misma estructura que la carpeta del juego.
3. En `cfg/server.cfg`, define `sm_cvar sv_allowdownload 1` y `sm_cvar sv_downloadurl "<address of that folder>"`.
4. En `cfg/playstats.cfg`, activa las skins: borra el `//` delante de `weapon_skins_enabled` y ponlo en `"1"`.
5. Para mostrar también los íconos de patente, haz lo mismo con `patent_icon_enabled`.
6. Reinicia el servidor mientras está vacío.

Los detalles, y cómo verificar que los archivos se encuentran, están en [Archivos de skins de armas y de patentes](../plugins/README.es.md#archivos-de-skins-de-armas-y-de-patentes).

## Opcional: el plugin del Anti-Cheat

El plugin de anti-cheat (`l4d2-anti-cheat.smx`, versión 1.5.0 o posterior) no forma parte de este paquete: viene con el programa Anti-Cheat y su propio servicio. Conectado al sitio, lee del sitio quién debe usar el Anti-Cheat, y los cambios hechos en el juego con `!aac config` se guardan en el sitio. Consulta [Quién debe usar el Anti-Cheat](manual/anti-cheat.es.md#quién-debe-usar-el-anti-cheat).

1. En el sitio, activa el módulo **Anti-Cheat** en **Administración → Módulos**.
2. Añade estas dos líneas a `cfg/playstats.cfg`, con la misma dirección y el mismo token que `playstats_url` y `playstats_token`:

   ```text
   l4d2_web_url "https://stats.example.com"
   l4d2_web_secret "my-server:3f9c…"
   ```

3. Elige el modo que usa el servidor de juego hasta que se guarde uno en el sitio, con `anti_cheat_mode` en el `shared_cvars.cfg` de tus modos de partida, por ejemplo `confogl_addcvar anti_cheat_mode 1`.
4. Reinicia el servidor mientras está vacío.

| Ajuste | Qué significa |
|---|---|
| `l4d2_web_url` | La dirección de este sitio. El plugin llama a `<dirección>/api/anti-cheat-access/server`. |
| `l4d2_web_secret` | El token de este servidor de juego, el mismo valor que `playstats_token`. Cada servidor de juego tiene el suyo. |
| `anti_cheat_mode` | `0`: todos deben usar el Anti-Cheat, excepto los jugadores de la whitelist. `1`: solo los jugadores de la lista obligatoria deben usarlo (el valor predeterminado del plugin). El modo guardado en **Administración → Ajustes del Anti-Cheat** prevalece; este valor solo cuenta hasta que se guarde un modo allí. |

Con `anti_cheat_enabled 0` en los modos de partida, la versión 1.6.0 del plugin permite que los jugadores activen el Anti-Cheat en la partida con `!aac vote`; ver [Activado por los jugadores](manual/anti-cheat.es.md#activado-por-los-jugadores).

Nunca definas `l4d2_web_secret` con `confogl_addcvar`, por el mismo motivo que `playstats_token`. Los demás ajustes del plugin, como la dirección y el token de su propio servicio, no cambian.

Después de un cambio en el sitio, `l4d2_gameinfo_sync` ejecuta `sm_aac sync` en el servidor de juego en segundos. Sin el módulo de servidor en vivo, el plugin lee el sitio por sí mismo cada minuto. Para verificar la conexión, escribe `sm_aac sync` y luego `sm_aac list` en la consola del servidor de juego, o pregunta al sitio desde esa máquina:

```text
curl -H "Authorization: my-server:3f9c…" https://stats.example.com/api/anti-cheat-access/server
```

La respuesta trae el modo (`null` hasta que se guarde uno) y las dos listas. `401` significa que el token está mal, y `404` que el módulo Anti-Cheat está desactivado.

## El ranking dentro del juego

`!ranking` abre el ranking de la página de inicio del sitio en el navegador del propio juego; con `ranking_motd_path "/ingame"` abre una versión más sencilla y compacta. Las páginas abiertas desde el juego no muestran la invitación a votar en el ranking de la comunidad. Ese navegador es de 2012 y no se espera que abra sitios HTTPS actuales, así que dale una dirección HTTP simple de tu sitio en `cfg/playstats.cfg`:

```text
playstats_ingame_url "http://203.0.113.10:5080"
```

Cuando `playstats_ingame_url` está vacío, el plugin usa `playstats_url`. No uses una dirección HTTPS ni una que redirija a HTTPS: el panel queda vacío. [Instalar en un VPS Linux](install-linux.es.md#la-dirección-usada-dentro-del-juego) explica qué dirección usar.

La dirección que los jugadores leen en el chat, en el aviso del ranking y en el enlace de la tienda de skins, es `playstats_public_url`; cuando está vacío, los plugins muestran `playstats_url`. Ponla cuando los plugins llegan al sitio por una dirección IP pero los jugadores lo conocen por un nombre:

```text
playstats_public_url "https://stats.example.com"
```

## El mix por el ranking de la comunidad

Con el módulo del ranking de la comunidad activado, un servidor de juego puede pedirle al sitio que divida a los jugadores en dos equipos equilibrados según su puntuación de la comunidad. El comando `!rmix` del plugin de mix lo usa.

```text
POST <playstats_url>/api/community/mix
Authorization: <el token del servidor>
{ "players": ["76561198000000001", "76561198000000002", ...] }
```

| Respuesta | Cuándo |
|---|---|
| `200` con `survivors`, `infected` y `survivorsChance` | Todos los jugadores están en el ranking de la comunidad. |
| `409` con `error: "unranked"` y `players` | Algunos jugadores no están en el ranking de la comunidad: tienen menos comparaciones que **Mínimo de comparaciones para aparecer en la lista**. El mix no empieza. |
| `400` | La lista está vacía, es impar, tiene más de 16, repite un jugador o tiene un SteamID64 inválido. |
| `404` | El módulo del ranking de la comunidad está desactivado. |

El sitio prueba todas las divisiones, se queda con las que dejan los equipos más cercanos en puntuación media, mejor jugador, dispersión e incertidumbre, y sortea una de ellas, para que los mismos jugadores no caigan siempre en los mismos equipos. Qué equipo empieza como superviviente también se sortea.

## Qué significa cada estado

| Estado | Significado | Qué hacer |
|---|---|---|
| Esperando la primera conexión | El servidor nunca llegó al sitio. | Revisa `cfg/playstats.cfg` y la línea `exec playstats.cfg`, y que `l4d2_playstats_sync.smx` esté cargado. Ejecuta `sm_playstats_test`. |
| Conectado | Llegó un heartbeat en los últimos segundos. | Nada. |
| Fuera de línea | El servidor se vio antes pero no ahora. | Normal mientras el servidor de juego está apagado, o vacío e hibernando. |
| Clave inválida | El identificador llegó con la clave equivocada. | Pega de nuevo el token, o genera una nueva clave en el sitio. |

## Actualizar los plugins

Copia la carpeta `left4dead2` nueva sobre la anterior, **excepto `cfg/playstats.cfg`**, que tiene tu dirección y tu token. Todos los demás archivos se pueden reemplazar; consulta [Actualizar](../plugins/README.es.md#actualizar) para las excepciones y un comando que omite ese archivo. Luego reinicia el servidor mientras está vacío.

## Si vienes de la configuración anterior

Si el servidor de juego usaba los plugins anteriores, quita sus ajustes, o seguirán apuntando a las direcciones antiguas:

| Ajuste anterior | Ahora |
|---|---|
| `playstats_url` apuntando a la API antigua | La dirección de este sitio |
| `playstats_access_token` | `playstats_token` |
| `l4d2_web_url`, `l4d2_web_secret` | Los plugins de PlayStats ya no los leen: usan `playstats_url` y `playstats_token`. Solo el plugin de anti-cheat todavía los lee; consulta [Opcional: el plugin del Anti-Cheat](#opcional-el-plugin-del-anti-cheat). |
| `ranking_url` | `playstats_ingame_url`, solo si el juego necesita otra dirección |
| El nombre del sitio en el aviso del ranking y la dirección del enlace de la tienda de skins (`l4d2_web_url`) | `playstats_public_url`, solo si los jugadores conocen el sitio por otra dirección |

Búscalos en `cfg/server.cfg`, en tu archivo de secretos, y en el `shared_cvars.cfg` de cada modo de partida. Los ajustes antiguos definidos con `confogl_addcvar playstats_url` también deben irse, para que `cfg/playstats.cfg` sea el único lugar de la dirección y el token. Copia los nuevos archivos `.smx` encima de los antiguos: los tres de `addons/sourcemod/plugins/optional/` reemplazan a los anteriores ahí, así que los modos de partida siguen cargándolos con las líneas que ya tienen. Si hay una copia antigua de alguno de ellos en `addons/sourcemod/plugins/` mismo, bórrala, para que haya una sola copia de cada plugin.

Para el cambio completo del sitio anterior a este, con el orden de los pasos y cómo volver atrás, consulta [Cambiar desde el sitio antiguo](switch-from-old-site.es.md).

## Si algo sale mal

| Lo que ves | Qué hacer |
|---|---|
| `sm_playstats_test` es un comando desconocido | `l4d2_playstats_sync.smx` no está cargado. Revisa la carpeta y ejecuta `sm plugins list` para ver su error. |
| Un plugin necesita REST in Pawn | Repite el paso 2 con el zip del sistema del servidor. |
| "URL not set" o "Token not set" | Las líneas no se ejecutaron. Verifica que `cfg/server.cfg` termine con `exec playstats.cfg` y que tus dos líneas de `cfg/playstats.cfg` no empiecen con `//`. |
| "Unknown identifier", "Wrong key" o "Token not accepted" | Pega de nuevo el token, o genera una nueva clave en el sitio. |
| "Cannot reach ..." | El servidor de juego no puede abrir el sitio. Prueba la dirección desde esa máquina con `curl -I`, revisa el firewall, y consulta [Solución de problemas](../plugins/README.es.md#solución-de-problemas) para los errores de HTTPS. |
| Funciona fuera de las partidas, se detiene cuando carga un modo de partida | Haz el paso 5. |
| Conectado, pero no llega ningún archivo de estadísticas | Juega un mapa completo en versus. Verifica que `sm_playstats_test` diga que la escritura de estadísticas está activada durante la partida. |
| Las partidas llegan pero no cuentan para el ranking | En el sitio, abre **Administración → Modos de juego** y mira en qué regla cae la configuración. |
| `!ranking` abre un panel vacío | Pon en `playstats_ingame_url` una dirección HTTP simple del sitio. |
