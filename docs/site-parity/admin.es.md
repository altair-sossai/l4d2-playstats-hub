# Paridad del sitio: administración, configuración inicial y ranking de la comunidad

🌐 [English](admin.md) | [Português](admin.pt.md) | **Español**

Las páginas de esta área solo existen en el hub: el sitio antiguo no tiene asistente de configuración, ni área de administración para servidores, módulos, ajustes, modos de juego, campañas o administradores, ni ranking de la comunidad. No hay nada que portar del sitio antiguo, así que esta lista de verificación es un inventario de lo que cada página tenía en su primera versión (Bootstrap 5, commit `f59518f`) y de qué fue de cada elemento cuando la página se reescribió con el aspecto del sitio antiguo (Bootstrap 3, `Views/Shared/_Layout.cshtml`, `_AdminStyles`).

`Dónde hoy` es el archivo y la línea del elemento en la versión con Bootstrap 5 (rutas relativas a `src/L4D2PlayStats.Hub.Web`), o `nuevo` para algo que este trabajo añade.

Estado: `done` = presente con el mismo contenido y comportamiento, con el aspecto del sitio antiguo; `adapted` = presente pero cambiado, por el motivo indicado; `not ported` = eliminado, por el motivo indicado.

Totales: 183 done, 11 adapted, 1 not ported.

Cómo se comprobó: cada página se abrió en un navegador sobre una copia de datos reales (6.636 archivos de estadísticas) y sobre una carpeta de datos vacía (todo el asistente, desde el código de configuración hasta "Finalizar"), en el tema oscuro y en el claro, a 1280 y a 400 píxeles de ancho, en inglés y en portugués; ninguna página se desplaza hacia los lados a 400 píxeles. Cada formulario se envió al menos una vez. `AdminPagesTests`, `SiteSettingsTests` y `FooterLinksTests` cubren el layout, el menú, los ajustes nuevos y el logo.

## Asistente de configuración (`/setup`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Ruta `GET /setup`; redirige a `/` una vez terminada la configuración | `Controllers/SetupController.cs:34` | `done` |  |
| Título de página "Configuración inicial" | `Views/Setup/Index.cshtml:3` | `done` | Se establece mediante `PageMetadata`, con `noindex`. |
| Encabezado "Bienvenido" | `Views/Setup/Index.cshtml:19` | `done` |  |
| Texto introductorio "Unos pocos pasos y el sitio estará listo." | `Views/Setup/Index.cshtml:20` | `done` |  |
| Barra de pasos: Administrador, Sitio, Módulos, Servidor de juego, Finalizar, numerados, con el paso actual y los pasos terminados marcados | `Views/Setup/Index.cshtml:22` | `adapted` | Píldoras justificadas de Bootstrap 3; los pasos terminados reciben una marca de verificación. En teléfonos solo se muestra el paso actual, así que la barra ocupa una línea en lugar de cinco. |
| Paso 1: encabezado "¿Quién es el administrador?" y la explicación del código de configuración | `Views/Setup/Index.cshtml:33` | `done` |  |
| Paso 1: mensaje de error cuando el código de configuración no es válido | `Views/Setup/Index.cshtml:37` | `done` |  |
| Paso 1: formulario `POST /setup/start` con el token antiforgery | `Views/Setup/Index.cshtml:39` | `done` |  |
| Paso 1: campo "Código de configuración" (obligatorio, sin autocompletar, conserva el valor escrito, se rellena desde `?token=`) | `Views/Setup/Index.cshtml:42` | `done` |  |
| Paso 1: campo "Solo desarrollo: iniciar sesión como este SteamID64" (solo con el inicio de sesión de desarrollo activado) | `Views/Setup/Index.cshtml:47` | `done` |  |
| Paso 1: botón "Continuar con Steam" con el icono de Steam | `Views/Setup/Index.cshtml:51` | `done` |  |
| Paso 1: el código se guarda en una cookie, el visitante inicia sesión con Steam y `GET /setup/claim` convierte esa cuenta en el primer administrador | `Controllers/SetupController.cs:67` | `done` |  |
| Paso de inicio de sesión: encabezado "La configuración inicial está en curso", explicación y botón "Entrar con Steam" (`/login?returnUrl=/setup`) | `Views/Setup/Index.cshtml:60` | `done` |  |
| Paso 2: formulario `POST /setup/site` con el token antiforgery, encabezado "Acerca del sitio" | `Views/Setup/Index.cshtml:68` | `done` |  |
| Paso 2: campo "Nombre del sitio" (obligatorio) | `Views/Setup/Index.cshtml:73` | `done` |  |
| Paso 2: campo "Idioma predeterminado" (English, Português, Español) | `Views/Setup/Index.cshtml:78` | `done` |  |
| Paso 2: campo "Zona horaria" con la lista de zonas horarias y el texto de ayuda "Determina cuándo empieza y termina una temporada." | `Views/Setup/Index.cshtml:87` | `done` |  |
| Paso 2: campo "Duración de la temporada, en meses" (de 1 a 24) con su texto de ayuda | `Views/Setup/Index.cshtml:99` | `done` |  |
| Paso 2: campo "Clave de la Steam Web API (opcional)" con su texto de ayuda y el enlace a steamcommunity.com/dev/apikey | `Views/Setup/Index.cshtml:104` | `done` |  |
| Paso 2: subida del logo del sitio | nuevo | `done` | Añadido. Las mismas reglas que en la página Configuración; un archivo rechazado mantiene el asistente en este paso, con el motivo. |
| Paso 3: formulario `POST /setup/modules`, encabezado "¿Qué debe tener este sitio?", nota "Puedes cambiar esto más adelante en el área de administración.", los interruptores de los módulos y "Continuar" | `Views/Setup/Index.cshtml:113` | `done` |  |
| Paso 4: encabezado "Conectar un servidor de juego" | `Views/Setup/Index.cshtml:126` | `done` |  |
| Paso 4, sin servidor todavía: explicación, formulario `POST /setup/server` con "Nombre del servidor" (obligatorio, 100) y "Dirección a la que se conectan los jugadores (opcional)" (placeholder, 200), botón "Crear" | `Views/Setup/Index.cshtml:130` | `done` |  |
| Paso 4: enlace "Omitir por ahora" | `Views/Setup/Index.cshtml:140` | `done` |  |
| Paso 4, servidor creado: nombre del servidor, instrucciones de conexión con el token mostrado una sola vez, cuadro de estado que se actualiza solo, "Continuar" | `Views/Setup/Index.cshtml:145` | `done` | Las dos líneas de configuración reciben también el botón de copiar del sitio antiguo. htmx solo se carga en este paso. |
| Paso 5: formulario `POST /setup/finish`, encabezado "Todo listo", explicación, botón "Finalizar" (va a `/admin`) | `Views/Setup/Index.cshtml:157` | `done` |  |
| El logo y el cambio de tema responden mientras la configuración está pendiente | nuevo | `done` | Añadido: `/site/logo` y `/theme/...` ya no se envían a `/setup`, así que el logo subido se ve en el menú del asistente y allí se puede cambiar el tema. |

## Parciales compartidos

### `_AdminNav`

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Enlaces: Vista general, Servidores de juego, Módulos, Configuración, Modos de juego, Campañas, Administradores | `Views/Shared/_AdminNav.cshtml:8` | `adapted` | La fila de píldoras ahora es un menú lateral (`Views/Shared/_AdminMenu.cshtml`, en `_AdminLayout`) en todas las páginas de administrador, agrupado en Sitio, Servidores de juego, Clasificación, Tienda de skins y Anti-Cheat. La fila faltaba en las páginas de baneos de chat, del Anti-Cheat y de skins, y el administrador perdía el camino de vuelta; ahora el menú siempre está ahí. En el teléfono se recoge en un control encima del contenido que muestra la página actual. |
| Enlace "Skins", solo con la tienda de skins activada | `Views/Shared/_AdminNav.cshtml:18` | `adapted` | Sustituido por las cuatro páginas de skins (Compras, Precios, Catálogo, Ajustes de la tienda) en el grupo Tienda de skins del menú lateral, que también sustituye su propia fila de pestañas. |
| Enlace "Ranking de la comunidad", solo con el ranking de la comunidad activado | `Views/Shared/_AdminNav.cshtml:21` | `done` |  |
| Enlace "Baneos de chat", solo con el chat del sitio activado | `Views/Shared/_AdminNav.cshtml:24` | `done` |  |
| Enlace "Comandos remotos", solo con los comandos remotos activados | `Views/Shared/_AdminNav.cshtml:27` | `done` |  |
| Enlace "Conexiones de jugadores", solo con el servidor en vivo activado | `Views/Shared/_AdminNav.cshtml:30` | `done` |  |
| Enlace "Anti-Cheat" (`/admin/anti-cheat`), solo con el módulo Anti-Cheat activado | nuevo | `done` | Añadido. Se llama "Ajustes del Anti-Cheat" desde que la página recibió los modos de uso y las listas. |
| El enlace de la página actual está resaltado | `Views/Shared/_AdminNav.cshtml:35` | `done` | También en las subpáginas: la página de un servidor de juego resalta Servidores de juego. |

### `_ServerStatus`

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Se actualiza solo cada 3 segundos (`GET /admin/servers/{id}/status`) | `Views/Shared/_ServerStatus.cshtml:30` | `done` | Sigue siendo htmx, ahora cargado por las páginas que muestran el estado (página del servidor, asistente) y no por el layout. Comprobado en un navegador: el bloque se reemplaza sin recargar la página. |
| Punto de color y título para el estado: Conectado, Fuera de línea, Clave inválida, Esperando la primera conexión | `Views/Shared/_ServerStatus.cshtml:32` | `done` |  |
| "Último contacto" con la fecha y la hora | `Views/Shared/_ServerStatus.cshtml:36` | `done` |  |
| Alerta cuando el identificador llegó con la clave incorrecta | `Views/Shared/_ServerStatus.cshtml:41` | `done` |  |
| Comprobación "Conexión con este sitio" | `Views/Shared/_ServerStatus.cshtml:44` | `done` |  |
| Comprobación "Plugin de envío de estadísticas cargado" | `Views/Shared/_ServerStatus.cshtml:45` | `done` |  |
| Comprobación "Escritura de estadísticas activada (sm_stats_writestats 1)" | `Views/Shared/_ServerStatus.cshtml:46` | `done` |  |
| Comprobación "Primer archivo de estadísticas recibido" con el número de archivos | `Views/Shared/_ServerStatus.cshtml:47` | `done` |  |
| Comprobación "Datos de la partida en vivo llegando", solo con el servidor en vivo activado | `Views/Shared/_ServerStatus.cshtml:50` | `done` |  |
| Comprobación "Plugin de skins de armas cargado", solo con la tienda de skins activada | `Views/Shared/_ServerStatus.cshtml:54` | `done` |  |
| Cada comprobación muestra superada, fallida o aún desconocida, con su color | `Views/Shared/_ServerStatus.cshtml:15` | `done` | Glyphicons (`ok`, `remove`, `option-horizontal`) con `text-success`, `text-danger`, `text-muted` en lugar de los símbolos de texto. |
| Línea con lo que informó el servidor: nombre de host, mapa, configuración, versión de SourceMod | `Views/Shared/_ServerStatus.cshtml:60` | `done` |  |

### `_ServerInstructions`

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Paso "Descarga los plugins de PlayStats" con los enlaces "Descargar" y "Guía paso a paso" y la nota sobre la carpeta de plugins | `Views/Shared/_ServerInstructions.cshtml:8` | `done` |  |
| Paso con lo que necesita el servidor (SourceMod, REST in Pawn, l4d2_playstats) | `Views/Shared/_ServerInstructions.cshtml:15` | `done` |  |
| Paso con las dos líneas para `cfg/server.cfg` (`playstats_url`, `playstats_token`) | `Views/Shared/_ServerInstructions.cshtml:19` | `done` | Con el botón de copiar del sitio antiguo (`_CopyButton`). |
| Advertencia "Copia el token ahora..." cuando la clave se acaba de generar | `Views/Shared/_ServerInstructions.cshtml:23` | `done` |  |
| Nota "La clave no se almacena de forma legible..." en los demás casos, con `YOUR-KEY` en lugar de la clave | `Views/Shared/_ServerInstructions.cshtml:27` | `done` |  |
| Paso "Reinicia el servidor de juego o cambia de mapa..." con el consejo de `sm_playstats_test` | `Views/Shared/_ServerInstructions.cshtml:30` | `done` |  |

### `_ModuleSwitches`

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Interruptor "Ranking y estadísticas" con su descripción | `Views/Shared/_ModuleSwitches.cshtml:5` | `done` | Bootstrap 3 no tiene interruptor: los siete son checkboxes con el título en negrita y la descripción debajo. |
| Interruptor "Servidor en vivo" con su descripción | `Views/Shared/_ModuleSwitches.cshtml:6` | `done` |  |
| Interruptor "Chat del sitio" con su descripción | `Views/Shared/_ModuleSwitches.cshtml:7` | `done` |  |
| Interruptor "Comandos remotos" con su descripción | `Views/Shared/_ModuleSwitches.cshtml:8` | `done` |  |
| Interruptor "Tienda de skins de armas" con su descripción | `Views/Shared/_ModuleSwitches.cshtml:9` | `done` |  |
| Interruptor "Ranking de la comunidad" con su descripción | `Views/Shared/_ModuleSwitches.cshtml:10` | `done` |  |
| Interruptor "Anti-cheat", deshabilitado ("Reservado. Aún no disponible.") | `Views/Shared/_ModuleSwitches.cshtml:11` | `adapted` | Ya no está reservado: el interruptor está habilitado como los demás y su descripción es "Muestra la página del Anti-Cheat, con la descarga y las instrucciones. El Anti-Cheat en sí es un programa aparte." (la página en sí está portada en la rama base). |
| Valor `false` oculto después de cada interruptor, para que un interruptor sin marcar se guarde como desactivado | `Views/Shared/_ModuleSwitches.cshtml:18` | `done` |  |

## Vista general (`/admin`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Todas las páginas de `/admin` son solo para administradores (401 anónimo, 403 otros jugadores) | `Controllers/Admin/AdminController.cs:23` | `done` |  |
| Título de página y encabezado "Administración" | `Views/Admin/Index.cshtml:3` | `done` |  |
| Menú de administración | `Views/Admin/Index.cshtml:6` | `done` |  |
| Mensaje "Reprocesamiento finalizado. Partidas reconstruidas: N" | `Views/Admin/Index.cshtml:8` | `done` |  |
| Mosaico "Servidores de juego" | `Views/Admin/Index.cshtml:14` | `done` | Mosaicos `admin-stats`, como en la antigua página Baneos del Chat. |
| Mosaico "Archivos de estadísticas" | `Views/Admin/Index.cshtml:15` | `done` |  |
| Mosaico "Partidas" | `Views/Admin/Index.cshtml:16` | `done` |  |
| Mosaico "Partidas clasificatorias" | `Views/Admin/Index.cshtml:17` | `done` |  |
| Mosaico "Jugadores" | `Views/Admin/Index.cshtml:18` | `done` |  |
| Mosaico "Temporadas" | nuevo | `done` | Añadido: el controlador ya contaba las temporadas y la página no mostraba el número. |
| Advertencia sobre las configuraciones que no coinciden con ninguna regla, con el enlace "Revisar los modos de juego" | `Views/Admin/Index.cshtml:21` | `done` |  |
| Sección "Servidores de juego": mensaje "Aún no hay ningún servidor de juego registrado." con el enlace "Añadir uno" | `Views/Admin/Index.cshtml:32` | `done` | `admin-empty` dentro de un `admin-list`. |
| Sección "Servidores de juego": una línea por servidor con el punto de estado, el nombre (enlace a la página del servidor) y el número de archivos | `Views/Admin/Index.cshtml:39` | `done` | `admin-list` / `admin-item`, como en la antigua página Baneos del Chat. La línea muestra también el estado en palabras y el identificador. |
| Panel "Mantenimiento": explicación y botón "Reprocesar todas las partidas" (`POST /admin/reprocess`, pregunta "¿Reconstruir todas las partidas ahora?") | `Views/Admin/Index.cshtml:51` | `done` |  |

## Servidores de juego (`/admin/servers`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página y encabezado "Servidores de juego", menú de administración | `Views/Admin/Servers.cshtml:3` | `done` |  |
| Columna de la tabla "Servidor": nombre (enlace a la página del servidor) y la insignia "Desactivado" | `Views/Admin/Servers.cshtml:14` | `done` | Tabla con las clases del sitio antiguo; en teléfonos cada celda es una línea con su `data-label`. |
| Columna de la tabla "Identificador" | `Views/Admin/Servers.cshtml:15` | `done` |  |
| Columna de la tabla "Estado": punto y estado | `Views/Admin/Servers.cshtml:16` | `done` |  |
| Columna de la tabla "Archivos de estadísticas" | `Views/Admin/Servers.cshtml:17` | `done` |  |
| Panel "Añadir un servidor de juego": "Nombre del servidor" (obligatorio, 100), "Dirección a la que se conectan los jugadores (opcional)" (placeholder, 200), botón "Crear" (`POST /admin/servers`) | `Views/Admin/Servers.cshtml:44` | `done` |  |
| Error "El nombre es obligatorio."; después de crear, se abre la página del servidor con el token mostrado una sola vez | `Controllers/Admin/AdminController.cs:80` | `done` |  |

## Servidor de juego (`/admin/servers/{id}`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página y encabezado con el nombre del servidor, menú de administración | `Views/Admin/Server.cshtml:4` | `done` | Se añadió en el encabezado un botón para volver a la lista de servidores de juego. |
| Panel "Cómo conectar este servidor" con las instrucciones | `Views/Admin/Server.cshtml:12` | `done` |  |
| Panel "Estado" que se actualiza solo | `Views/Admin/Server.cshtml:18` | `done` |  |
| Panel "Detalles": formulario `POST /admin/servers/{id}` con "Nombre del servidor" (obligatorio, 100) y "Dirección a la que se conectan los jugadores (opcional)" (200) | `Views/Admin/Server.cshtml:28` | `done` |  |
| Panel "Detalles": interruptor "Activado", con el valor `false` oculto | `Views/Admin/Server.cshtml:38` | `done` | Checkbox. |
| Panel "Detalles": el identificador del servidor y el botón "Guardar" | `Views/Admin/Server.cshtml:42` | `done` |  |
| Panel "Clave": explicación y botón "Generar una nueva clave" (`POST .../key`, pregunta "¿Generar una nueva clave?") | `Views/Admin/Server.cshtml:51` | `done` | `panel-warning`. |
| Panel "Clave": botón "Eliminar" (`POST .../delete`, pregunta "¿Eliminar este servidor?") | `Views/Admin/Server.cshtml:54` | `done` |  |
| Error "Un servidor que ya ha enviado estadísticas no se puede eliminar. Desactívalo en su lugar." | `Controllers/Admin/AdminController.cs:144` | `done` |  |

## Configuración (`/admin/settings`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página y encabezado "Configuración", menú de administración | `Views/Admin/Settings.cshtml:3` | `done` |  |
| Formulario `POST /admin/settings` con el token antiforgery, botón "Guardar", mensaje "Guardado." | `Views/Admin/Settings.cshtml:9` | `adapted` | El formulario ahora es `multipart/form-data`, por el logo. Además de "Guardado.", los problemas con los campos nuevos se listan en rojo (logo rechazado, líneas del pie de página omitidas, ID de Analytics no válido). |
| Panel "Sitio": "Nombre del sitio" (obligatorio) | `Views/Admin/Settings.cshtml:15` | `done` |  |
| Panel "Sitio": "Dirección pública del sitio" con placeholder y texto de ayuda | `Views/Admin/Settings.cshtml:19` | `done` |  |
| Panel "Sitio": "Idioma predeterminado" | `Views/Admin/Settings.cshtml:25` | `done` |  |
| Panel "Sitio": "Zona horaria" con la lista de zonas horarias | `Views/Admin/Settings.cshtml:34` | `done` |  |
| Panel "Sitio": "Clave de la Steam Web API (opcional)" con su texto de ayuda | `Views/Admin/Settings.cshtml:45` | `done` |  |
| Panel "Apariencia": logo del sitio, con vista previa, subida y eliminación | nuevo | `done` | Añadido. Vista previa sobre una franja con el color del menú, campo de archivo (PNG, JPG, WEBP, GIF, hasta 1 MB) y un checkbox para quitar el logo. Lo guarda `BrandingStore` y se sirve en `/site/logo`. |
| Panel "Apariencia": enlaces del pie de página | nuevo | `done` | Añadido. Uno por línea como `Text | https://address`; se guardan normalizados, las líneas que no son enlaces se omiten y se informan. Vacío significa el enlace al proyecto. |
| Panel "Apariencia": ID de Google Analytics | nuevo | `done` | Añadido. Opcional; solo se aceptan letras, dígitos y guiones. |
| Panel "Apariencia": palabras clave para buscadores | nuevo | `done` | Añadido (`SiteSettings.MetaKeywords`): se guardan sin espacios sobrantes, separadas por comas, con 500 caracteres como máximo. El `<meta name="keywords">` que las muestra está en el layout de la rama base. |
| Panel "Temporadas": "Duración de la temporada, en meses" (de 1 a 24) con su texto de ayuda | `Views/Admin/Settings.cshtml:55` | `done` |  |
| Panel "Puntos del ranking": "Victoria" | `Views/Admin/Settings.cshtml:64` | `done` |  |
| Panel "Puntos del ranking": "Derrotas" | `Views/Admin/Settings.cshtml:65` | `done` |  |
| Panel "Puntos del ranking": "Rage quit" | `Views/Admin/Settings.cshtml:66` | `done` |  |
| Panel "Puntos del ranking": "MVP en daño a infectados especiales, por ronda" | `Views/Admin/Settings.cshtml:67` | `done` |  |
| Panel "Puntos del ranking": "MVP en infectados comunes, por ronda" | `Views/Admin/Settings.cshtml:68` | `done` |  |
| Panel "Puntos del ranking": "Mínimo de partidas para aparecer en el ranking" | `Views/Admin/Settings.cshtml:69` | `done` |  |
| Panel "Puntos del ranking": interruptor "Conservar las partidas jugadas en mapas que no están en la lista de campañas", con el valor `false` oculto, y el texto de ayuda | `Views/Admin/Settings.cshtml:72` | `done` | Checkbox. |

## Módulos (`/admin/modules`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página y encabezado "Módulos", menú de administración | `Views/Admin/Modules.cshtml:3` | `done` |  |
| Formulario `POST /admin/modules`: nota "Un módulo apagado desaparece del menú...", los interruptores de los módulos, botón "Guardar", mensaje "Guardado." | `Views/Admin/Modules.cshtml:8` | `done` |  |

## Administradores (`/admin/administrators`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página y encabezado "Administradores", menú de administración | `Views/Admin/Administrators.cshtml:3` | `done` |  |
| Columna de la tabla "Nombre" ("-" cuando no hay ninguno) | `Views/Admin/Administrators.cshtml:12` | `done` | Se añadió el avatar de cada administrador (oculto en teléfonos); tu propia línea se resalta como las líneas "soy yo" de otros lugares. |
| Columna de la tabla "SteamID64", enlace al perfil de Steam | `Views/Admin/Administrators.cshtml:13` | `adapted` | El enlace va a la página del jugador en el sitio, que ofrece el perfil de Steam. |
| Columna de la tabla "Desde" | `Views/Admin/Administrators.cshtml:14` | `done` |  |
| Botón "Quitar" (`POST /admin/administrators/{id}/delete`, pregunta "¿Quitar este administrador?") | `Views/Admin/Administrators.cshtml:28` | `done` |  |
| Insignia "Tú" en tu propia línea, en lugar del botón | `Views/Admin/Administrators.cshtml:34` | `done` | `label label-default`. |
| Panel "Añadir un administrador": "SteamID64" (número, obligatorio, placeholder), "Nombre" (100), botón "Añadir" | `Views/Admin/Administrators.cshtml:46` | `done` |  |
| Errores "Ese no es un SteamID64 válido." y "No puedes quitarte a ti mismo ni al último administrador." | `Controllers/Admin/AdminController.cs:249`, `Controllers/Admin/AdminController.cs:258` | `done` |  |

## Campañas (`/admin/campaigns`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página y encabezado "Campañas", menú de administración | `Views/Admin/Campaigns.cshtml:3` | `done` |  |
| Explicación de cómo los mapas forman una partida | `Views/Admin/Campaigns.cshtml:8` | `done` |  |
| Un formulario por campaña (`POST /admin/campaigns`): nombre (obligatorio) y los mapas, uno por línea, en orden de juego | `Views/Admin/Campaigns.cshtml:16` | `done` | Un panel por campaña, en una cuadrícula que pasa de cuatro columnas a una en teléfonos. |
| Botón "Guardar" de cada campaña | `Views/Admin/Campaigns.cshtml:21` | `done` |  |
| Botón "Eliminar" de cada campaña (`POST /admin/campaigns/{id}/delete`, pregunta "¿Eliminar esta campaña?") | `Views/Admin/Campaigns.cshtml:22` | `done` |  |
| Formulario para una campaña nueva: nombre (placeholder "Nueva campaña"), mapas (placeholder, obligatorio), botón "Añadir" | `Views/Admin/Campaigns.cshtml:30` | `done` |  |

## Modos de juego (`/admin/game-modes`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página y encabezado "Modos de juego", menú de administración | `Views/Admin/GameModes.cshtml:3` | `done` |  |
| Explicación de cómo se aplican las reglas | `Views/Admin/GameModes.cshtml:9` | `done` |  |
| Tabla "Reglas": columnas Nombre, Patrón de configuración, Tamaño del equipo, Mínimo de mapas, Prioridad, Cuenta para el ranking | `Views/Admin/GameModes.cshtml:17` | `done` |  |
| Un formulario por regla (`POST /admin/game-modes`): Nombre (obligatorio), Patrón de configuración (obligatorio), Tamaño del equipo (de 1 a 8, "Cualquiera" cuando está vacío), Mínimo de mapas (desde 1), Prioridad, Cuenta para el ranking (Sí/No) | `Views/Admin/GameModes.cshtml:32` | `done` | Cada regla es ahora una fila real de la tabla: los campos se vinculan al formulario de la fila con el atributo `form`, así que las columnas se alinean con el encabezado y el diseño para teléfonos (`data-label`) funciona. Comprobado en un navegador: guardar y eliminar una regla envían los campos correctos. |
| Botón "Guardar" de cada regla | `Views/Admin/GameModes.cshtml:45` | `done` |  |
| Botón "Eliminar" de cada regla (`POST /admin/game-modes/{id}/delete`, pregunta "¿Eliminar esta regla?") | `Views/Admin/GameModes.cshtml:46` | `done` |  |
| Línea para una regla nueva: placeholder "Nueva regla", patrón `*`, mínimo de mapas 2, prioridad 50, botón "Añadir" | `Views/Admin/GameModes.cshtml:55` | `done` |  |
| Sección "Configuraciones vistas en tus servidores" | `Views/Admin/GameModes.cshtml:74` | `done` |  |
| Mensaje "Nada todavía. Las configuraciones aparecen aquí cuando llegan los primeros archivos de estadísticas." | `Views/Admin/GameModes.cshtml:77` | `done` | `admin-empty`. |
| Columnas de la tabla Configuración, Tamaño del equipo, Visto por última vez, Regla | `Views/Admin/GameModes.cshtml:85` | `done` |  |
| Insignia de la regla: "Sin regla: fuera del ranking", el nombre de la regla, o el nombre de la regla con "Fuera del ranking" | `Views/Admin/GameModes.cshtml:101` | `done` | `label-warning`, `label-success`, `label-default`. |

## Ranking de la comunidad (`/community`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Las páginas solo responden con el ranking de la comunidad activado | `Controllers/CommunityController.cs:14` | `done` |  |
| Título de página y encabezado "Ranking de la comunidad" | `Views/Community/Index.cshtml:3` | `done` | `PageMetadata` con la descripción y el avatar del primer puesto; encabezado como en la página del ranking (`h4.text-center`). `ViewBag.Community` marca el menú. |
| Texto "Formado con los votos de los jugadores. No se reinicia con la temporada." | `Views/Community/Index.cshtml:8` | `done` |  |
| Botón "Votar" | `Views/Community/Index.cshtml:10` | `done` |  |
| Mensaje "Aún nadie tiene suficientes comparaciones para aparecer en la lista." | `Views/Community/Index.cshtml:15` | `done` | Jumbotron, como el ranking vacío de la página de inicio. |
| Columna de la tabla "#" (posición; el primer puesto destaca) | `Views/Community/Index.cshtml:23` | `done` | Como en el ranking de inicio: medallas para los tres primeros puestos, `#N` para los demás. |
| Columna de la tabla "Jugador": avatar y nombre, enlazados a la página del jugador | `Views/Community/Index.cshtml:24` | `adapted` | Las mismas dos celdas que en el ranking de inicio (avatar, nombre). El nombre se acorta con `TruncatePlayerName`, como en el ranking de inicio; el nombre completo está en el título del avatar. |
| Columna de la tabla "Patente" | nuevo | `done` | Añadida, como en el ranking de inicio: la patente del jugador en la temporada actual. Solo con el módulo de ranking activado. |
| Columna de la tabla "Puntuación" | `Views/Community/Index.cshtml:25` | `done` |  |
| Columna de la tabla "Comparaciones" | `Views/Community/Index.cshtml:26` | `adapted` | Columna en escritorio. En teléfonos la columna se oculta (`hide-mobile`) y el número es una línea debajo del nombre; la página con Bootstrap 5, en cambio, se desplazaba hacia los lados. |
| Columna de la tabla "Victorias" | `Views/Community/Index.cshtml:27` | `done` |  |
| Columna de la tabla "Empates" | `Views/Community/Index.cshtml:28` | `adapted` | Igual que "Comparaciones". |
| Columna de la tabla "Derrotas" | `Views/Community/Index.cshtml:29` | `adapted` | Igual que "Comparaciones". |
| La línea del jugador con sesión iniciada está resaltada | `Views/Community/Index.cshtml:35` | `done` | `it-is-me`, la clase que usa el sitio antiguo. |
| Nota "Mínimo de comparaciones para aparecer en la lista: N" | `Views/Community/Index.cshtml:67` | `done` |  |

## Votar (`/community/vote`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página y encabezado "¿Quién es mejor?" | `Views/Community/Vote.cshtml:3` | `done` | `PageMetadata`; `ViewBag.Community` marca el menú. |
| Texto "Tus respuestas hacen subir y bajar a los jugadores en el ranking de la comunidad." | `Views/Community/Vote.cshtml:8` | `done` |  |
| Botón "Ranking de la comunidad" (vuelve al ranking) | `Views/Community/Vote.cshtml:10` | `done` |  |
| Advertencia cuando la respuesta fue rechazada ("Esa pareja no es la que espera...", "Elige una de las respuestas.") | `Views/Community/_Ballot.cshtml:6` | `done` |  |
| Sin sesión iniciada: "Inicia sesión con Steam para votar." con el botón de Steam, que vuelve a esta página | `Views/Community/_Ballot.cshtml:14` | `done` | Jumbotron con el botón de Steam. |
| Muy pocas partidas: mensaje con "Partidas jugadas: N / mínimo" | `Views/Community/_Ballot.cshtml:23` | `done` | Jumbotron. |
| Límite diario: mensaje con "Votos por día: N" | `Views/Community/_Ballot.cshtml:30` | `done` | Jumbotron. |
| No queda nadie: mensaje | `Views/Community/_Ballot.cshtml:36` | `done` | Jumbotron. |
| Formulario de la papeleta (`POST /community/vote`, token antiforgery); con scripts la respuesta se envía en segundo plano y solo se reemplaza la papeleta | `Views/Community/_Ballot.cshtml:43` | `done` | Sigue siendo htmx, cargado solo por esta página. Comprobado en un navegador: la respuesta se registra y la siguiente pareja aparece sin recargar. |
| Sin scripts el formulario se envía normalmente y la página se recarga, con el rechazo mostrado como un error | `Controllers/CommunityController.cs:60` | `done` |  |
| Los dos jugadores uno al lado del otro, cada uno con avatar y nombre | `Views/Community/_Ballot.cshtml:50` | `done` | Dos paneles de Bootstrap 3, uno al lado del otro también en teléfonos. |
| Botón "Este es mejor" debajo de cada jugador | `Views/Community/_Ballot.cshtml:52` | `done` |  |
| Botón "No conozco a este jugador" debajo de cada jugador | `Views/Community/_Ballot.cshtml:53` | `done` |  |
| Botón "Están al mismo nivel" | `Views/Community/_Ballot.cshtml:69` | `done` |  |
| Botón "No conozco a ninguno de los dos" | `Views/Community/_Ballot.cshtml:70` | `done` |  |
| Contador "Votos restantes hoy: N / por día" | `Views/Community/_Ballot.cshtml:74` | `done` |  |

## Administración del ranking de la comunidad (`/admin/community`)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Solo para administradores, y solo con el ranking de la comunidad activado | `Controllers/Admin/CommunityAdminController.cs:16` | `done` |  |
| Título de página y encabezado "Ranking de la comunidad", menú de administración | `Views/CommunityAdmin/Index.cshtml:3` | `done` | Se añadió en el encabezado un botón hacia el ranking público. |
| Mensaje "Puntuaciones reconstruidas. Votos reaplicados: N" | `Views/CommunityAdmin/Index.cshtml:9` | `done` |  |
| Mensaje "Puntuaciones reconstruidas sin ese votante. Votos quitados: N" | `Views/CommunityAdmin/Index.cshtml:13` | `done` |  |
| Mosaico "Votos" | `Views/CommunityAdmin/Index.cshtml:19` | `done` | `admin-stats`. |
| Mosaico "Jugadores puntuados" | `Views/CommunityAdmin/Index.cshtml:20` | `done` |  |
| Formulario `POST /admin/community` con el token antiforgery, botón "Guardar", mensaje "Guardado." | `Views/CommunityAdmin/Index.cshtml:23` | `done` |  |
| Panel "Votación": "Mínimo de partidas para votar" | `Views/CommunityAdmin/Index.cshtml:30` | `done` |  |
| Panel "Votación": "Votos por día" (con su mínimo) | `Views/CommunityAdmin/Index.cshtml:34` | `done` |  |
| Panel "Votación": "Mínimo de partidas para ser comparado" y el texto de ayuda | `Views/CommunityAdmin/Index.cshtml:38` | `done` |  |
| Panel "Puntuación": "Mínimo de comparaciones para aparecer en la lista" | `Views/CommunityAdmin/Index.cshtml:51` | `done` |  |
| Panel "Puntuación": "Factor K de Elo" (con sus límites) | `Views/CommunityAdmin/Index.cshtml:55` | `done` |  |
| Panel "Puntuación": "Puntuación inicial" y el texto de ayuda | `Views/CommunityAdmin/Index.cshtml:59` | `done` |  |
| Panel "Mantenimiento": explicación y botón "Reconstruir las puntuaciones" (pregunta "¿Reconstruir todas las puntuaciones ahora?") | `Views/CommunityAdmin/Index.cshtml:73` | `done` |  |
| Panel "Votantes": explicación y el formulario para quitar los votos de un SteamID64 (pregunta "¿Quitar todos los votos de este votante?") | `Views/CommunityAdmin/Index.cshtml:85` | `done` |  |
| Panel "Votantes": mensaje "Aún no ha votado nadie." | `Views/CommunityAdmin/Index.cshtml:94` | `done` |  |
| Panel "Votantes": columna de la tabla "Jugador", avatar y nombre enlazados al perfil de Steam | `Views/CommunityAdmin/Index.cshtml:102` | `adapted` | El nombre enlaza a la página del jugador en el sitio, que ofrece el perfil de Steam. El avatar se oculta en teléfonos, donde cada celda es una línea con su etiqueta. |
| Panel "Votantes": columna de la tabla "Votos" | `Views/CommunityAdmin/Index.cshtml:103` | `done` |  |
| Panel "Votantes": columna de la tabla "Último voto" | `Views/CommunityAdmin/Index.cshtml:104` | `done` |  |
| Panel "Votantes": botón "Quitar votos" en cada línea (pide confirmación) | `Views/CommunityAdmin/Index.cshtml:122` | `done` |  |
| Error "Ese no es un SteamID64 válido." | `Controllers/Admin/CommunityAdminController.cs:64` | `done` |  |

## Página de bienvenida (`/` con el módulo de ranking desactivado)

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Título de página: el nombre del sitio | `Views/Home/Welcome.cshtml:4` | `done` | Ya estaba convertida en la rama base; se dejó como estaba. |
| Cuadro con el nombre del sitio y "Usa el menú para ver lo que está disponible en este sitio." | `Views/Home/Welcome.cshtml:6` | `done` |  |

## Layout

| Elemento | Dónde hoy | Estado | Notas |
|---|---|---|---|
| Estilos de Bootstrap 5 y `bootstrap.bundle` | `Views/Shared/_Bootstrap5Layout.cshtml:19` | `not ported` | Es el objetivo de este trabajo: las páginas usan ahora los temas de Bootstrap 3 de `_Layout`. `_Bootstrap5Layout` y `wwwroot/lib/bootstrap` siguen en el repositorio para las carpetas que convierten otras ramas. |
| htmx cargado en todas las páginas | `Views/Shared/_Bootstrap5Layout.cshtml:114` | `adapted` | `_Layout` no lo carga. Los tres lugares que lo necesitan lo cargan en su sección `Scripts`: la página del servidor de juego, el paso del servidor en el asistente y la página de votación. |
| Clases de `site.css` usadas por estas páginas (punto de estado, pasos del asistente, bloque de configuración, avatares) | `Views/Shared/_Bootstrap5Layout.cshtml:20` | `adapted` | Lo que estas páginas todavía necesitan (punto de estado, pasos del asistente, bloque de configuración, cuadrícula de paneles) está en el parcial `Views/Shared/_AdministrationStyles.cshtml`, junto a `_AdminStyles`; los estilos de la papeleta están en la página de votación. |
| Mensajes de error y "Guardado." en la parte superior de la página | `Views/Shared/_Bootstrap5Layout.cshtml:100` | `done` | Mediante `_Layout`. |
