# Conectar un servidor de juego

🌐 [English](game-server.md) | [Português](game-server.pt.md) | **Español**

Cómo hacer que un servidor de Left 4 Dead 2 envíe sus partidas a tu sitio. Repites estos pasos para cada servidor de juego; un sitio puede recibir cualquier cantidad de ellos.

## Contenido

- [Lo que necesita el servidor de juego](#lo-que-necesita-el-servidor-de-juego)
- [Paso 1: registra el servidor en el sitio](#paso-1-registra-el-servidor-en-el-sitio)
- [Paso 2: copia los plugins](#paso-2-copia-los-plugins)
- [Paso 3: pega las dos líneas](#paso-3-pega-las-dos-líneas)
- [Paso 4: verifica la conexión](#paso-4-verifica-la-conexión)
- [Paso 5: carga los plugins en tus modos de partida](#paso-5-carga-los-plugins-en-tus-modos-de-partida)
- [El ranking dentro del juego](#el-ranking-dentro-del-juego)
- [Qué significa cada estado](#qué-significa-cada-estado)
- [Si vienes de la configuración anterior](#si-vienes-de-la-configuración-anterior)
- [Si algo sale mal](#si-algo-sale-mal)

## Lo que necesita el servidor de juego

Los plugins están hechos para servidores basados en [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), como los servidores ZoneMod.

| Requisito | Por qué |
|---|---|
| SourceMod 1.12 | Ejecuta los plugins. |
| Extensión [REST in Pawn](https://github.com/ErikMinekus/sm-ripext) | Permite que los plugins llamen al sitio. |
| Plugin `l4d2_playstats` | Viene con L4D2 Competitive Rework. Escribe el archivo de estadísticas de cada mapa. |
| `sm_stats_writestats 1` | Le indica a `l4d2_playstats` que escriba esos archivos. Los modos de partida de L4D2 Competitive Rework ya lo definen. |

## Paso 1: registra el servidor en el sitio

1. Inicia sesión en el sitio como administrador.
2. Abre **Administración → Servidores de juego**.
3. Escribe un nombre y haz clic en **Crear**.

El sitio muestra dos líneas con la dirección del sitio y el token de este servidor:

```text
playstats_url "https://stats.example.com"
playstats_token "my-server:3f9c…"
```

Cópialas ahora. La clave que está dentro del token se muestra una sola vez. Si la pierdes, haz clic en **Generar una nueva clave** en la misma página.

## Paso 2: copia los plugins

Descarga `l4d2-playstats-hub-plugins.zip` desde la [página de versiones](https://github.com/altair-sossai/l4d2-playstats-hub/releases). Las carpetas que contiene coinciden con las carpetas del servidor de juego, así que extráelo sobre la carpeta `left4dead2`.

| Archivo | Dónde va |
|---|---|
| `l4d2_playstats_sync.smx` | `addons/sourcemod/plugins/` |
| Los demás archivos `.smx` | `addons/sourcemod/plugins/`, o `plugins/optional/` si un modo de partida los carga |
| `translations/` | `addons/sourcemod/translations/` |
| `configs/weapon_skins.cfg` | `addons/sourcemod/configs/` |

Deja `l4d2_playstats_sync.smx` directamente en `plugins/`, no en una subcarpeta. Tiene que cargarse cuando el servidor se inicia, para que el sitio vea el servidor incluso cuando no se está jugando ninguna partida.

| Plugin | Qué hace | Módulo del sitio |
|---|---|---|
| `l4d2_playstats_sync` | Envía los archivos de estadísticas y le avisa al sitio que el servidor está activo. | Siempre necesario |
| `l4d2_ranking` | Abre el ranking dentro del juego con `!ranking`. | Ranking |
| `l4d2_show_patent_icon` | Muestra el ícono de patente de cada jugador dentro del juego. | Ranking |
| `l4d2_gameinfo_sync` | Envía la partida en vivo: marcador, jugadores, eventos y chat. | Servidor en vivo |
| `l4d2_weapon_skins` | Aplica las skins de armas que tienen los jugadores. | Tienda de skins de armas |

Solo necesitas los plugins de los módulos que activaste.

## Paso 3: pega las dos líneas

Abre `cfg/server.cfg` en el servidor de juego y pega al final las dos líneas del paso 1.

Reinicia el servidor de juego, o cambia de mapa.

## Paso 4: verifica la conexión

En el sitio, la página del servidor de juego se actualiza sola. En unos segundos debería decir **Conectado**, con una lista de lo que se encontró:

- Conexión con este sitio
- Plugin de envío de estadísticas cargado
- Escritura de estadísticas activada
- Primer archivo de estadísticas recibido (después del primer mapa completo)

También puedes verificarlo desde el servidor de juego. Escribe esto en su consola:

```text
sm_playstats_test
```

Responde con el nombre del servidor y del sitio, o dice exactamente qué está mal.

## Paso 5: carga los plugins en tus modos de partida

Los modos de partida de L4D2 Competitive Rework descargan y cargan plugins cuando se inician. Si pusiste un plugin en `plugins/optional/`, agrégalo a la lista de plugins de cada modo donde deba ejecutarse, por ejemplo en `cfg/cfgogl/zonemod/confogl_plugins.cfg`:

```text
sm plugins load optional/l4d2_ranking.smx
sm plugins load optional/l4d2_show_patent_icon.smx
```

Los ajustes de esos plugins van en el mismo modo, con `confogl_addcvar`. [La guía de los plugins](../plugins/README.es.md) lista todos los ajustes.

Las estadísticas se envían desde todos los modos. En el sitio, **Administración → Modos de juego** decide qué modos cuentan para el ranking.

## El ranking dentro del juego

`!ranking` abre una versión simple del ranking en el navegador propio del juego. Ese navegador es de 2012 y no se espera que abra los sitios HTTPS actuales, así que dale una dirección HTTP simple de tu sitio:

```text
confogl_addcvar playstats_ingame_url "http://203.0.113.10:5080"
```

Cuando `playstats_ingame_url` está vacío, el plugin usa `playstats_url`. [Instalar en un VPS Linux](install-linux.es.md#la-dirección-usada-dentro-del-juego) explica qué dirección usar.

## Qué significa cada estado

| Estado | Significado | Qué hacer |
|---|---|---|
| En espera | El servidor nunca llegó al sitio. | Revisa las dos líneas, y que `l4d2_playstats_sync.smx` esté en `plugins/`. Ejecuta `sm_playstats_test`. |
| Conectado | Llegó una señal en los últimos segundos. | Nada. |
| Fuera de línea | El servidor se vio antes, pero ahora no. | Es normal mientras el servidor de juego está apagado, o vacío e hibernando. |
| Clave inválida | El identificador llegó con la clave equivocada. | Pega el token de nuevo, o genera una nueva clave en el sitio. |

## Si vienes de la configuración anterior

Si el servidor de juego usaba los plugins anteriores, quita sus ajustes, o seguirán apuntando a las direcciones antiguas:

| Ajuste antiguo | Ahora |
|---|---|
| `playstats_url` apuntando a la API antigua | La dirección de este sitio |
| `playstats_access_token` | `playstats_token` |
| `l4d2_web_url`, `l4d2_web_secret` | Eliminados. Todo usa `playstats_url` y `playstats_token`. |
| `ranking_url` | `playstats_ingame_url`, solo si el juego necesita una dirección distinta |

Búscalos en `cfg/server.cfg`, en tu archivo de secretos y en el `shared_cvars.cfg` de cada modo de partida.

## Si algo sale mal

| Lo que ves | Qué hacer |
|---|---|
| `sm_playstats_test` es un comando desconocido | `l4d2_playstats_sync.smx` no está cargado. Revisa la carpeta y ejecuta `sm plugins list`. |
| "URL not set" | Las dos líneas no se ejecutaron. Deben estar en un archivo que el servidor ejecute al iniciar, después de que el plugin esté cargado. |
| "Cannot reach the address" | El servidor de juego no puede abrir el sitio. Prueba la dirección desde esa máquina y revisa el firewall. |
| Conectado, pero no llega ningún archivo de estadísticas | Juega un mapa completo en versus. Revisa `sm_stats_writestats`, y que `l4d2_playstats` esté cargado en ese modo. |
| Las partidas llegan pero no cuentan para el ranking | En el sitio, abre **Administración → Modos de juego** y mira en qué regla cae la configuración. |
| `!ranking` abre un panel vacío | Define `playstats_ingame_url` con una dirección HTTP simple del sitio. |
