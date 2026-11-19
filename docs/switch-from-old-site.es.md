# Cambiar desde el sitio antiguo

🌐 [English](switch-from-old-site.md) | [Português](switch-from-old-site.pt.md) | **Español**

Cómo reemplazar una copia en funcionamiento del sitio anterior (`l4d2-playstats-web` con `l4d2-playstats-api`) por este, en la misma máquina, sin cambiar las direcciones que ya usan los jugadores. El ejemplo es la instalación de l4d2.com.br: un VPS Ubuntu en `124.198.128.43` donde nginx sirve `https://l4d2.com.br` y lo pasa al sitio antiguo en el puerto `5000`, el juego abre `http://124.198.128.43:5000`, y el servidor de juego se basa en [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server). Pon tus propios nombres y direcciones en su lugar.

## Contenido

- [El plan](#el-plan)
- [Parte 1: instala el sitio nuevo junto al antiguo](#parte-1-instala-el-sitio-nuevo-junto-al-antiguo)
- [Parte 2: prueba con un servidor de juego](#parte-2-prueba-con-un-servidor-de-juego)
- [Parte 3: el cambio](#parte-3-el-cambio)
- [Parte 4: volver atrás](#parte-4-volver-atrás)
- [Los datos antiguos](#los-datos-antiguos)

## El plan

| Parte | Qué cambia | Qué notan los jugadores |
|---|---|---|
| 1 | El sitio nuevo funciona en el puerto `5090`, junto al antiguo. nginx y el sitio antiguo no se tocan. | Nada. |
| 2 | Un servidor de juego envía sus partidas al sitio nuevo. | Las partidas de ese servidor dejan de llegar al sitio antiguo. |
| 3 | El sitio antiguo se detiene y el nuevo toma su puerto, el `5000`. nginx sigue pasando `https://l4d2.com.br` ahí. | Las páginas nuevas, en las mismas direcciones. |
| 4 | Solo si algo sale mal: vuelve el sitio antiguo. | Las páginas antiguas otra vez. |

Deja `/home/l4d2-playstats-web`, la carpeta del sitio antiguo, tal como está hasta que estés seguro: volver atrás la necesita.

## Parte 1: instala el sitio nuevo junto al antiguo

1. Conéctate al VPS por SSH, con un usuario que pueda usar `sudo`.
2. Ejecuta el instalador sin dominio. Sin `HUB_DOMAIN` no instala Caddy, así que nginx y los puertos 80 y 443 quedan como están:

   ```bash
   curl -fsSL https://raw.githubusercontent.com/altair-sossai/l4d2-playstats-hub/main/deploy/install.sh | sudo HUB_PORT=5090 bash
   ```

3. Abre el puerto `5090`. No es el `5080` por defecto del instalador porque, en este VPS, ese puerto ya lo usa la API del anti-cheat (`127.0.0.1:5080`). Con UFW activo, el instalador ya lo hizo; si no, ejecuta `sudo ufw allow 5090/tcp`. Si tu proveedor de hosting tiene su propio firewall, abre el puerto ahí también.
4. Abre `http://124.198.128.43:5090`, pega el código de configuración que mostró el instalador e inicia sesión con Steam. Esa cuenta pasa a ser el primer administrador.
5. Sigue el asistente ([Configuración inicial](manual/first-run.es.md)). En la página **Módulos**, activa lo que tenía el sitio antiguo: ranking, servidor en vivo, chat del sitio, comandos remotos, tienda de skins y página del Anti-Cheat. En la página **Servidor de juego**, crea el servidor de juego y copia las dos líneas que muestra, `playstats_url` y `playstats_token`: la clave se muestra una sola vez.
6. Recorre el resto de la administración ([Administración](manual/administration.es.md)): **Configuración → Privacidad** (quién ve las cuentas relacionadas), **Administración → Modos de juego** (qué configuraciones cuentan para el ranking), las temporadas, la clave de la Steam Web API, los demás administradores y el catálogo de skins de la tienda. Deja la **Dirección pública del sitio** vacía por ahora.

[Instalar en un VPS Linux](install-linux.es.md) explica cada paso del instalador y los comandos `playstats-hub`.

## Parte 2: prueba con un servidor de juego

Un servidor de juego envía sus partidas a un solo sitio. Mientras apunta al sitio nuevo, sus partidas ya no llegan al antiguo, y no van ahí después. Prueba con un segundo servidor de juego si tienes uno, o en un momento tranquilo con el principal.

### Haz una copia de lo que reemplazan los plugins

En el servidor de juego, guarda una copia de los archivos que reemplazan los nuevos plugins, para la [Parte 4](#parte-4-volver-atrás):

```bash
cd /path/to/the/server/left4dead2
tar -czf ~/playstats-plugins-backup.tar.gz addons/sourcemod/plugins addons/sourcemod/translations addons/sourcemod/configs/weapon_skins.cfg cfg
```

Si el servidor de juego se instala desde un repositorio, como `l4d2-zone-server`, haz los cambios de abajo también en el repositorio, o la próxima instalación traerá de vuelta los plugins y los ajustes antiguos.

### Copia los plugins

Copia todo lo que hay dentro de la carpeta `left4dead2` de los plugins (en el zip de la versión `l4d2-playstats-hub-plugins.zip`, o en [plugins/left4dead2](../plugins/left4dead2) en este repositorio) encima de la carpeta `left4dead2` del servidor:

| Archivo | Qué reemplaza |
|---|---|
| `addons/sourcemod/plugins/l4d2_gameinfo_sync.smx` y `l4d2_weapon_skins.smx` | Los antiguos en el mismo lugar. |
| `addons/sourcemod/plugins/optional/l4d2_playstats_sync.smx`, `l4d2_ranking.smx` y `l4d2_show_patent_icon.smx` | Los antiguos en el mismo lugar, que el `shared_plugins.cfg` de los modos de partida ya carga. |
| `addons/sourcemod/translations/**` | Los textos de las versiones antiguas. Cópialos junto con los archivos `.smx`: los textos del ranking cambiaron. |
| `addons/sourcemod/configs/weapon_skins.cfg` | Las mismas armas y skins. |
| `addons/sourcemod/scripting/**` | Las fuentes, que solo se usan para compilar. |
| `cfg/playstats.cfg` | Nada: es nuevo. |

Debe haber una sola copia de cada plugin. Ninguno de los cinco puede quedar en otra carpeta, como un `l4d2_playstats_sync.smx` antiguo en `addons/sourcemod/plugins/` mismo.

### Cambia los ajustes

| Archivo | Cambio |
|---|---|
| `cfg/cfgogl/zonemod/shared_cvars.cfg` y `cfg/cfgogl/neomod/shared_cvars.cfg` | Borra `confogl_addcvar playstats_url ...`, la API antigua, que haría que los cinco plugins volvieran a apuntar a ella en cada partida, y `confogl_addcvar ranking_url ...`, que ya no existe. Deja las demás líneas: `patent_icon_enabled`, `weapon_skins_*`, `gameinfo_*` y `anti_cheat_*` funcionan como antes. |
| `cfg/server_secrets.cfg` | Borra `playstats_access_token`. Agrega `playstats_url` y `playstats_token` (abajo). Pon en `l4d2_web_secret` el mismo token que en `playstats_token`. |
| `cfg/server.cfg` | Pon en `l4d2_web_url` la dirección del sitio nuevo. Si quieres, agrega `sm plugins load optional/l4d2_playstats_sync.smx` al final, para que el sitio vea el servidor mientras no haya un modo de partida cargado. |
| `cfg/sharedplugins.cfg` | Opcional: las líneas `sm plugins load` del [Paso 5](game-server.es.md#paso-5-carga-los-plugins-en-tus-modos-de-partida), para los modos de partida que todavía no cargan estos plugins (en `l4d2-zone-server`, todos los modos menos ZoneMod y NeoMod). |

Las líneas para `cfg/server_secrets.cfg`, con tu token en lugar del ejemplo:

```text
playstats_url "http://124.198.128.43:5090"
playstats_token "my-server:3f9c…"
l4d2_web_secret "my-server:3f9c…"
```

Y la línea para `cfg/server.cfg`:

```text
sm_cvar l4d2_web_url "http://124.198.128.43:5090"
```

- `playstats_url` es una dirección HTTP simple, así que el navegador del juego también la abre y `playstats_ingame_url` puede quedar vacío.
- `l4d2_web_url` y `l4d2_web_secret` solo los lee el plugin de anti-cheat (1.5.0 o más nuevo), para las listas de [Quién debe usar el Anti-Cheat](manual/anti-cheat.es.md#quién-debe-usar-el-anti-cheat). Las listas antiguas no se copian: agrega los jugadores otra vez en el sitio nuevo antes de activar el anti-cheat.
- Deja `playstats_public_url` desactivado por ahora. Se define en la Parte 3; antes de eso, el chat mandaría a los jugadores a la tienda del sitio antiguo.
- Con estas líneas en el archivo de secretos no necesitas `exec playstats.cfg`: el resto de `cfg/playstats.cfg` solo muestra los valores por defecto, y los modos de partida de `l4d2-zone-server` ya definen `sm_stats_writestats`.
- Nunca definas `playstats_token` ni `l4d2_web_secret` con `confogl_addcvar`: Confogl muestra en el chat los valores que sigue.

### Comprueba

1. Reinicia el servidor de juego mientras está vacío y carga un modo de partida, por ejemplo con `!match`.
2. En la consola del servidor, `sm plugins list` debe mostrar los cinco plugins, ninguno con error y ninguno dos veces. Después, `sm_playstats_test` debe responder `Connected to the site`.
3. En el sitio nuevo, abre **Administración → Servidores de juego** y el servidor. Debe decir **Conectado**, con la lista de comprobación del [Paso 6](game-server.es.md#paso-6-verifica-la-conexión).
4. Juega un mapa completo. La partida aparece en **Partidas**, la ronda en **Servidor**, y el chat del sitio llega al juego.
5. En el juego, `!ranking` abre el panel del ranking. Con `weapon_skins_enabled 1`, `!skin status` responde; con `patent_icon_enabled 1`, los íconos de patente aparecen antes de la partida.

## Parte 3: el cambio

Hazlo cuando no esté jugando nadie. Después de esto los dos sitios escuchan en el puerto `5000`, así que el antiguo se detiene primero.

1. Detén el sitio antiguo, conservando sus archivos: `screen -S l4d2-playstats-web -X quit`, con el usuario que lo inició. Si `screen -ls` no lo muestra, ejecuta los dos comandos con `sudo`.
2. Evita que arranque con el sistema: ejecuta `crontab -e` como `devops` y borra la línea `@reboot` de `l4d2-playstats-web`, o pon `#` delante de ella.
3. Desactiva la publicación automática del sitio antiguo, el pipeline de Azure Pipelines de `l4d2-playstats-web`. Si no, un push lo vuelve a iniciar en el puerto `5000`.
4. Pasa el sitio nuevo al puerto `5000`: en `/etc/l4d2-playstats-hub/hub.env`, cambia `ASPNETCORE_URLS=http://0.0.0.0:5090` por `ASPNETCORE_URLS=http://0.0.0.0:5000`, y luego ejecuta `sudo playstats-hub restart` y `playstats-hub status`.
5. Compruébalo: `curl -s http://127.0.0.1:5000/api/ping` responde, `https://l4d2.com.br` y `http://124.198.128.43:5000` muestran el sitio nuevo, y el inicio de sesión con Steam funciona en `https://l4d2.com.br`.
6. En **Administración → Configuración → Sitio**, pon la **Dirección pública del sitio** en `https://l4d2.com.br`.
7. En el servidor de juego, apunta todo al puerto `5000` y define la dirección que los jugadores leen en el chat: las dos primeras líneas en `cfg/server_secrets.cfg`, la última en `cfg/server.cfg`. Luego cambia el mapa o reinicia el servidor, y ejecuta `sm_playstats_test` otra vez:

   ```text
   playstats_url "http://124.198.128.43:5000"
   playstats_public_url "https://l4d2.com.br"
   sm_cvar l4d2_web_url "http://124.198.128.43:5000"
   ```

8. Cierra el puerto `5090`, que ya no se usa: `sudo ufw delete allow 5090/tcp`, y en el firewall del proveedor si lo abriste ahí.

Los plugins también podrían usar `https://l4d2.com.br`; la dirección simple los mantiene funcionando pase lo que pase con el certificado. nginx no necesita cambios: sigue pasando `https://l4d2.com.br` al puerto `5000`, y el sitio nuevo cree en las cabeceras `X-Forwarded-*` de un proxy en la misma máquina. Si el inicio de sesión con Steam vuelve a una dirección `http://` o a la dirección IP, el `location` de nginx debe pasar estas cabeceras:

```nginx
proxy_set_header Host $host;
proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;
```

## Parte 4: volver atrás

Si algo no funciona y no se puede arreglar en el momento:

1. Detén el sitio nuevo y evita que arranque con el sistema: `sudo systemctl disable --now l4d2-playstats-hub`. Sus datos quedan en `/var/lib/l4d2-playstats-hub`.
2. Inicia el sitio antiguo como antes: `cd /home/l4d2-playstats-web && screen -d -m -S l4d2-playstats-web sh -c './L4D2PlayStats.Web'`. Vuelve a poner su línea `@reboot` y activa otra vez su pipeline.
3. En el servidor de juego, extrae la copia en la carpeta `left4dead2` (`tar -xzf ~/playstats-plugins-backup.tar.gz`) y reinicia el servidor. Eso trae de vuelta los plugins antiguos y todos los ajustes de `cfg`.

Las partidas jugadas mientras el servidor de juego apuntaba al sitio nuevo quedan solo ahí; el sitio antiguo no las recibe. Para intentarlo de nuevo más tarde, vuelve a poner el puerto `5090` en `ASPNETCORE_URLS` de `/etc/l4d2-playstats-hub/hub.env` y ejecuta `sudo systemctl enable --now l4d2-playstats-hub`.

## Los datos antiguos

El sitio nuevo empieza vacío. Traer las estadísticas antiguas es opcional: un programa aparte sube los archivos de estadísticas antiguos por el mismo camino que usan los plugins, y el sitio arma con ellos las partidas, el ranking y el historial ([Migración de datos](roadmap.es.md#migración-de-datos)). Puede ejecutarse antes o después del cambio, y las partidas que el sitio nuevo ya tiene se quedan. Nada más del sitio antiguo se copia: las compras de skins, los regalos y las skins equipadas, los baneos del chat y las listas del Anti-Cheat empiezan vacíos, así que vuelve a registrar en el sitio nuevo los baneos del chat y los jugadores del Anti-Cheat.
