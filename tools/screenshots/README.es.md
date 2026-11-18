# Herramienta de capturas de pantalla

Toma las capturas de pantalla que usa la documentación y las guarda en `docs/images/<lang>/`, una carpeta por idioma de la interfaz (`en`, `pt`, `es`). Controla un Edge o un Chrome sin interfaz gráfica mediante el protocolo DevTools. Necesita Node 22 o más reciente y no tiene dependencias.

## Qué captura

| Comando | Capturas | Necesita |
|---|---|---|
| `terminal` | El instalador y los comandos de Docker, dibujados como ventanas de terminal a partir de la salida grabada en [terminal/](terminal/). Las mismas imágenes van a la carpeta de cada idioma. | Nada más. |
| `wizard` | Cada paso del asistente de instalación, desde el código de configuración hasta el sitio terminado. | Un sitio iniciado sobre una carpeta de datos vacía, y su código de configuración. |
| `site` | Todas las páginas públicas, la tienda de skins, el ranking de la comunidad, las páginas dentro del juego, el diseño para teléfono, los dos temas y todas las páginas de administración. | Un sitio con datos, con la sesión iniciada mediante el inicio de sesión de desarrollo. |

Las listas de capturas están en [site.js](site.js), [wizard.js](wizard.js) y [terminal-shots.json](terminal-shots.json). Cada captura tiene un nombre, una página, un ancho (1280 para escritorio, 390 para teléfono, 900x450 para las páginas dentro del juego), las acciones que se hacen antes de la captura y lo que se conserva: la página completa, la parte visible o un elemento. Las imágenes son WebP con calidad 80.

## Ejecutar el sitio para la herramienta

Los comandos `site` y `wizard` inician sesión mediante el inicio de sesión de desarrollo (`/dev/sign-in/<SteamID64>`), que solo existe en el entorno Development con el inicio de sesión de prueba permitido. Publica el sitio y ejecútalo sobre una carpeta de datos propia:

```bash
dotnet publish src/L4D2PlayStats.Hub.Web -c Release -o ./shots-run
```

```bash
cd shots-run
ASPNETCORE_ENVIRONMENT=Development Development__AllowTestSignIn=true Hub__DataDirectory=/path/to/data ./L4D2PlayStats.Hub.Web --urls http://localhost:5103
```

Nunca lo apuntes a una carpeta de datos de producción: el comando `site` compra una skin y el comando `wizard` termina la configuración inicial.

## El asistente

Inicia el sitio sobre una carpeta de datos vacía. Muestra el código de configuración y lo escribe en `setup-code.txt`, en la carpeta de datos. Luego:

```bash
node tools/screenshots/capture.js wizard --base http://localhost:5103 --lang en --admin 76561198000000000 --setup-code <code>
```

El asistente se ejecuta una sola vez por carpeta de datos. Para cada idioma, detén el sitio, vacía la carpeta, vuelve a iniciarlo y usa el código nuevo. La herramienta oculta el campo del inicio de sesión de desarrollo antes de cada captura, porque un sitio de producción no lo muestra. Después del último paso abre la página del servidor nuevo en el área de administración y envía una señal como lo haría un servidor de juego, para que el estado de la conexión se ponga en verde.

## Datos ficticios

Las capturas de pantalla de la documentación muestran jugadores ficticios. Se tomaron sobre una copia de los datos de una comunidad real en la que cada persona se reemplazó por una inventada, así que las partidas, los rankings y los números parecen reales, pero ningún apodo, avatar, SteamID ni dirección pertenece a nadie. [fictional-data.js](fictional-data.js) hace esa copia a partir de una carpeta de datos propia:

```bash
node tools/screenshots/fictional-data.js --from /path/to/real/data --to /path/to/fictional/data
```

- Solo lee la carpeta de `--from`, y se detiene si otro programa, como el sitio, tiene abierta su base de datos (se detecta en Windows y en Linux). Detén el sitio antes, o pasa una copia de la carpeta.
- La carpeta de `--to` tiene que ser nueva o estar vacía. Recibe la base de datos, el logo y avatares nuevos; los avatares reales y las claves no se copian.
- Cada SteamID pasa a uno cuyo número de cuenta empieza en 4.000.000.000, un rango que Steam todavía no usa, así que ningún enlace de perfil lleva a una persona real. Los administradores van primero, así que el primero es siempre `76561201960265728`. Los SteamID cambian en todas las tablas, en los ajustes y en los archivos de estadísticas en bruto, en todos los formatos en que aparecen (SteamID64, `STEAM_x:y:z` y `[U:1:n]`).
- Cada nombre pasa a un apodo de jugador inventado, el mismo para la misma persona en todas partes, incluidos los archivos de estadísticas en bruto y los nombres anteriores. Un jugador que usó varios nombres recibe varios nombres ficticios.
- Cada jugador que tenía avatar recibe uno nuevo, un cuadrado de color con iniciales o una forma simple, dibujado en Edge o en Chrome.
- Las direcciones IP de las conexiones de jugadores pasan a direcciones de los rangos de documentación (`198.51.100.0/24` y `192.0.2.0/24`). Los servidores de juego pasan a `L4D2 Competitive #1`, `#2`... en `203.0.113.10`, con los identificadores `l4d2-competitive-1`, `l4d2-competitive-2`...: sus tokens conservan la misma clave con el identificador nuevo.
- El catálogo de skins se queda como está, porque da el crédito a los autores de las skins en el Workshop de Steam.

Los nombres, los SteamID y los avatares salen de una semilla aleatoria, que la herramienta muestra; pásala de nuevo con `--seed` para obtener el mismo resultado a partir de los mismos datos. Necesita Node 22.16 o más reciente, sin flags. Cuando termina, inicia el sitio sobre la carpeta nueva, inicia sesión con el SteamID64 que mostró para el administrador y ejecuta **Administración > Vista general > Reprocesar todas las partidas**, para que cada partida se arme de nuevo a partir de los archivos ficticios.

## El resto del sitio

El comando `site` espera una carpeta de datos, como la copia ficticia de arriba, con:

- todos los módulos activados en **Administración > Módulos**;
- dos servidores de juego, con sus tokens;
- algunos votos de la comunidad, un baneo del chat, un comando remoto y una conexión de jugador, para que esas páginas no estén vacías.

```bash
node tools/screenshots/capture.js site --base http://localhost:5103 --lang en --admin <SteamID64 of an administrator> --player <SteamID64 of a player with matches and stars> --token <token of server 1> --token <token of server 2>
```

Antes de las capturas del servidor en vivo envía la partida en vivo de cada servidor, como lo hacen los plugins, con los jugadores de la parte de arriba del ranking del sitio, y espera el retraso de la página en vivo. Reinicia el sitio antes de cada idioma, para que el chat en vivo empiece vacío. Antes de las capturas de la tienda reembolsa las compras del jugador y luego compra la primera skin que el jugador puede pagar. Las capturas del baneo del chat agregan un baneo para `--banned`.

El inicio de sesión de desarrollo no tiene nombre de Steam, así que el menú, las compras y los registros de auditoría muestran `Player <SteamID64>` para el usuario que inició sesión.

## Las ventanas de terminal

Los archivos de [terminal/](terminal/) son la salida real de los comandos, grabada con colores en un contenedor desechable `jrei/systemd-ubuntu:24.04` (`docker exec -t`). La herramienta los reproduce en un pequeño emulador de terminal y captura el resultado. Para volver a grabarlos, ejecuta el comando en un contenedor y guarda su salida:

```bash
docker exec -t <container> bash -c "stty cols 96 rows 30; playstats-hub status" > tools/screenshots/terminal/linux-status.log
```

Para los comandos de Docker, instala Docker dentro de ese contenedor (`curl -fsSL https://get.docker.com | sh`) y dale a su `/var/lib/docker` y a su `/var/lib/containerd` volúmenes propios (`-v <volume>:/var/lib/docker`), porque las imágenes no se descomprimen en el sistema de archivos del propio contenedor.

Luego:

```bash
node tools/screenshots/capture.js terminal
```

## Opciones

| Opción | Significado |
|---|---|
| `--base` | Dirección del sitio. |
| `--lang` | `en`, `pt` o `es`. Define el idioma de la interfaz antes de la primera captura. |
| `--admin` | SteamID64 de un administrador de ese sitio. |
| `--player` | SteamID64 que se usa para la página del jugador, la tienda y las páginas dentro del juego. Sus compras se reembolsan antes de las capturas de la tienda, para que pueda volver a comprar. Predeterminado: el primer jugador del ranking. |
| `--voter` | SteamID64 de un jugador que todavía puede votar hoy, para la página de votación. Predeterminado: `--player`. |
| `--banned` | SteamID64 que se usa en las capturas del baneo del chat. Predeterminado: `--player`. |
| `--token` | Token de un servidor de juego, `identifier:key`. Repítelo para un segundo servidor. |
| `--setup-code` | El código de configuración, para el asistente. |
| `--only` | Nombres de las capturas que se vuelven a tomar, separados por comas. |
| `--out` | Carpeta para las imágenes. Predeterminado: `docs/images`. |
| `--langs` | Para `terminal`: las carpetas de idioma que se escriben. Predeterminado: `en,pt,es`. |
| `--browser` | Ruta a Edge o Chrome, si no se encuentra solo. |
