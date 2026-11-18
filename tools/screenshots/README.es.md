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

## El resto del sitio

El comando `site` espera una copia de datos reales con:

- todos los módulos activados en **Administración > Módulos**;
- dos servidores de juego, con sus tokens;
- algunos votos de la comunidad, un baneo del chat, un comando remoto y una conexión de jugador, para que esas páginas no estén vacías.

```bash
node tools/screenshots/capture.js site --base http://localhost:5103 --lang en --admin <SteamID64 of an administrator> --player <SteamID64 of a player with matches and stars> --token <token of server 1> --token <token of server 2>
```

Antes de las capturas del servidor en vivo envía la partida en vivo de cada servidor, como lo hacen los plugins, y espera el retraso de la página en vivo. Reinicia el sitio antes de cada idioma, para que el chat en vivo empiece vacío. Antes de las capturas de la tienda reembolsa las compras del jugador y luego compra la primera skin que el jugador puede pagar. Las capturas del baneo del chat agregan un baneo para `--banned`.

El inicio de sesión de desarrollo no tiene nombre de Steam, así que el menú, las compras y los registros de auditoría muestran `Player <SteamID64>` para el usuario que inició sesión.

## Las ventanas de terminal

Los archivos de [terminal/](terminal/) son la salida real de los comandos, grabada con colores en un contenedor desechable `jrei/systemd-ubuntu:24.04` (`docker exec -t`). La herramienta los reproduce en un pequeño emulador de terminal y captura el resultado. Para volver a grabarlos, ejecuta el comando en un contenedor y guarda su salida:

```bash
docker exec -t <container> bash -c "stty cols 96 rows 30; playstats-hub status" > tools/screenshots/terminal/linux-status.log
```

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
| `--player` | SteamID64 que se usa para la página del jugador, la tienda y las páginas dentro del juego. Sus compras se reembolsan antes de las capturas de la tienda, para que pueda volver a comprar. |
| `--voter` | SteamID64 de un jugador que todavía puede votar hoy, para la página de votación. Predeterminado: `--player`. |
| `--banned` | SteamID64 que se usa en las capturas del baneo del chat. Predeterminado: `--player`. |
| `--token` | Token de un servidor de juego, `identifier:key`. Repítelo para un segundo servidor. |
| `--setup-code` | El código de configuración, para el asistente. |
| `--only` | Nombres de las capturas que se vuelven a tomar, separados por comas. |
| `--out` | Carpeta para las imágenes. Predeterminado: `docs/images`. |
| `--langs` | Para `terminal`: las carpetas de idioma que se escriben. Predeterminado: `en,pt,es`. |
| `--browser` | Ruta a Edge o Chrome, si no se encuentra solo. |
