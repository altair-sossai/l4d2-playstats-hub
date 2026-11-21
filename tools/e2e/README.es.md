# Pruebas en el navegador

Verifican las páginas principales del sitio en un Chrome o Edge headless, para que una regresión de diseño o de JavaScript rompa el build. Se ejecutan en cada push y pull request en [Browser tests](../../.github/workflows/browser-tests.yml) y necesitan Node 22 o más reciente, sin dependencias. Controlan el navegador con el [cdp.js](../screenshots/cdp.js) de la herramienta de capturas de pantalla.

## Qué hacen

[run.js](run.js) inicia el sitio publicado con una carpeta de datos nueva y vacía, en el entorno Development con el inicio de sesión de desarrollo (`/dev/sign-in/<SteamID64>`), y lo llena como una comunidad real:

- completa el asistente de configuración con el código de `setup-code.txt`, activa todos los módulos y crea un servidor de juego;
- sube 14 archivos de estadísticas, cinco partidas de 16 jugadores ficticios, hechos a partir de la muestra en [tests/L4D2PlayStats.Hub.Tests/Samples](../../tests/L4D2PlayStats.Hub.Tests/Samples) con el token del servidor ([statistics.js](statistics.js), jugadores en [fixtures.js](fixtures.js));
- configura el ranking de la comunidad con 3 votos por día, envía una partida en vivo y registra votos de la comunidad con seis jugadores.

Después ejecuta las verificaciones de [checks.js](checks.js): el ranking de la página de inicio (también en 390 px, sin desplazamiento lateral), la página del jugador y sus pestañas, la página de un SteamID desconocido, las partidas, la lista de jugadores (la vista general, el orden por una columna, una estadística y la antigua dirección del historial), todas las páginas públicas en 390 px, la página de comandos (búsqueda, enlaces de las secciones y los comandos de administrador plegados), el ranking de la comunidad (invitación, "cómo funciona", ventanas de detalles del jugador y de las columnas), la votación con las flechas y un clic en la foto hasta el límite del día, los filtros de los últimos votos y la ventana de búsqueda de jugador, todas las páginas de la administración y sus íconos de ayuda, la ventana de búsqueda de jugador, el constructor de comandos remotos, los formularios de límites del chat y de ajustes en vivo, la página en vivo recibiendo datos nuevos, portugués y español, y el tema claro. Un error de JavaScript o del servidor en cualquier página también reprueba la verificación.

Todo se ejecuta en ese sitio desechable; la carpeta de datos se borra al final. Nunca lo apunte a una carpeta de datos real.

## Ejecución

```bash
dotnet publish src/L4D2PlayStats.Hub.Web -c Release -o ./e2e-site
node tools/e2e/run.js --site ./e2e-site
```

Tarda unos dos minutos. Una verificación que falla muestra el motivo y guarda una captura de pantalla, la página y el log del sitio en `e2e-output/`, que el workflow publica como el artefacto `browser-test-failures`.

| Opción | Significado |
|---|---|
| `--site` | Carpeta del sitio publicado. |
| `--port` | Puerto del sitio. Predeterminado: `5199`. |
| `--only` | Nombres de las verificaciones a ejecutar, separados por comas, como `community-vote,languages`. Los datos siempre se crean. |
| `--browser` | Ruta de Chrome o Edge, si no se encuentra solo. |
| `--out` | Carpeta de las fallas. Predeterminado: `e2e-output`. |
| `--data` | Una carpeta vacía para usar como carpeta de datos, que se conserva al final. |
| `--keep` | Conserva la carpeta de datos temporal. |
| `--serve` | Crea los datos y deja el sitio en ejecución para revisarlo en el navegador, hasta Ctrl+C. |
