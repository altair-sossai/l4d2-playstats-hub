# Preguntas y problemas

🌐 [English](faq.md) | [Português](faq.pt.md) | **Español**

Preguntas frecuentes de jugadores y administradores, y qué hacer cuando algo no funciona. Los problemas de la instalación en sí también aparecen al final de cada guía de instalación: [Linux](../install-linux.es.md#si-algo-sale-mal), [Docker](../install-docker.es.md), y el servidor de juego en [Conectar un servidor de juego](../game-server.es.md).

[Volver al manual](README.es.md)

## Contenido

- [Para jugadores](#para-jugadores)
- [Para administradores](#para-administradores)
- [Solución de problemas](#solución-de-problemas)

## Para jugadores

**Jugué una partida y no aparece en el ranking.**
Solo cuentan las partidas del ranking. Una partida entra en el ranking cuando su configuración y su tamaño de equipo encajan con una regla de modo de juego que cuenta, y tiene suficientes mapas: con la regla predeterminada, 4v4 y al menos 2 mapas seguidos de la misma campaña. Los mapas sueltos, el 1v1 y otros modos aparecen como **Casual**. Además, la partida solo aparece después de que se sube su último mapa, unos segundos después de que termina el mapa.

**Mi victoria contó como rage quit.**
Un jugador que estaba en el primer mapa de la partida y no está en el último mapa cuenta como rage quit, aunque haya vuelto después o haya perdido la conexión. Consulta [Ranking > Victorias, derrotas, empates y rage quits](ranking.es.md#victorias-derrotas-empates-y-rage-quits). Cuando un sustituto ocupó el lugar, un administrador puede [cambiar al jugador](matches.es.md#cambiar-un-jugador).

**Entré en medio de una partida y no recibí la victoria.**
Las victorias y las derrotas van para los jugadores que estaban en el primer mapa. Los jugadores que entraron después reciben solo sus puntos de MVP y de tank wipe.

**Mi experiencia no cambió después de un empate.**
Un empate no da victoria ni derrota, solo los puntos de MVP y de tank wipe.

**¿Por qué se reinició mi experiencia?**
Empezó una temporada nueva. La temporada pasada está en el selector de período del ranking y de **Más > Jugadores** mientras sea del año actual; para un año anterior, elige ese año, o abre la temporada desde su dirección.

**Mi nombre es viejo o está mal.**
El sitio muestra un solo nombre en todas partes: el nombre que forzó un administrador; si no, el que más usaste en todas tus partidas, de todas las temporadas; si no, tu nombre de Steam. Tus otros nombres están en **Nombres anteriores**, en tu página. Consulta [Jugadores > El nombre del jugador](players.es.md#el-nombre-del-jugador).

**¿Cómo consigo estrellas?**
Una estrella por cada premio de MVP de SI y de MVP de CI en una partida del ranking. Consulta [Tienda de skins de armas > Estrellas](skin-shop.es.md#estrellas).

**Mis skins desaparecieron.**
Si el sitio reembolsa las skins al final de una temporada, las skins compradas se retiran cuando empieza una temporada nueva, y las estrellas vuelven a ti. Cómpralas de nuevo. Los regalos nunca se retiran.

**Compré una skin y no la veo en el juego.**
Aparece a partir del próximo cambio de mapa. En el juego, `!skin status` muestra lo que tienes.

**El sitio dice que necesito más partidas para votar.**
El ranking de la comunidad pide un número mínimo de partidas, contadas en todas las temporadas: 10 de forma predeterminada.

**La página del servidor en vivo va atrasada respecto al juego.**
Va unos 15 segundos atrasada, a propósito. Un administrador cambia el retraso en **Administración > Servidores de juego > Página del servidor en vivo**.

**No veo la campana de las notificaciones.**
La campana aparece solo después de iniciar sesión. Mira [Notificaciones](notifications.es.md).

**El número de la campana no desaparece.**
Cuenta las notificaciones que aún no abriste. Ábrelas o usa **Marcar todas como leídas** al final de la campana. Las notificaciones con más de 90 días se borran.

**Me expulsaron del servidor con un mensaje de que estoy baneado.**
El mensaje dice el motivo y cuándo termina el baneo, y **Más > Baneos** lista todos los baneos. Los baneos no se pueden disputar en el sitio; habla con los administradores de la comunidad. Ver [Baneos](bans.es.md#para-jugadores).

## Para administradores

**¿Cómo agrego otro administrador?**
En [Administración > Administradores](administration.es.md#administradores), con su SteamID64. Si ya no puedes iniciar sesión, usa `playstats-hub add-admin <SteamID64>` en Linux.

**¿Cómo organizo un campeonato?**
Activa **Campeonatos** en [Administración > Módulos](administration.es.md#módulos) y crea el campeonato en **Administración > Campeonatos**: elige el formato, añade los participantes y los equipos y arma la primera fase. Consulta [Campeonatos > Para administradores](championships.es.md#para-administradores).

**¿Los jugadores pueden inscribirse solos en un campeonato?**
Sí, cuando activas **Los jugadores pueden pedir participar** en la pestaña Configuración del campeonato. Mientras su estado es Participantes y quedan plazas, su página tiene un botón para que los jugadores conectados pidan una plaza; cada solicitud espera en la pestaña Participantes hasta que la apruebas o la rechazas. Ver [Pedir participar](championships.es.md#pedir-participar).

**¿Dónde gestiona un capitán su equipo?**
En el área del capitán del campeonato: inicia sesión con Steam y haz clic en el icono de torre junto a tu foto arriba, o en **Abrir el área del capitán** en la página del campeonato. Allí el capitán y el vicecapitán eligen los jugadores de un draft cuando es su turno, envían un nuevo nombre, sigla o bandera para la aprobación de la administración, eligen al vicecapitán, proponen y aceptan fechas, vetan y eligen campañas. Consulta [El área del capitán](championships.es.md#el-área-del-capitán).

**Un juego del campeonato se jugó fuera de nuestro servidor.**
En la pestaña Juegos del campeonato, en **Administración > Campeonatos**, haz clic en el botón del juego y elige **Escribir el resultado**. El juego cuenta en la clasificación y en el cuadro, pero no tiene estadísticas.

**¿Cómo agrego un segundo servidor de juego?**
En [Administración > Servidores de juego](administration.es.md#servidores-de-juego). Cada servidor recibe su propio token. Los resultados de los jugadores se suman entre todos los servidores.

**Perdí el token de un servidor de juego.**
El sitio no guarda la clave. Abre la página del servidor y haz clic en **Generar una nueva clave**, y luego pon el token nuevo en la configuración del servidor de juego.

**Cambié los modos de juego y las partidas antiguas no cambiaron.**
Las reglas se aplican a los archivos nuevos. Usa **Reprocesar todas las partidas** en la [vista general](administration.es.md#vista-general).

**Cambié los puntos y cambiaron todas las temporadas.**
Es lo esperado: los rankings se calculan a partir de las partidas con los puntos actuales. Las temporadas no tienen una copia congelada.

**¿Puedo cambiar la duración de la temporada en medio de una temporada?**
Sí. La temporada actual conserva sus fechas; la nueva duración se aplica a partir de la siguiente.

**¿Hay forma de eliminar una partida?**
No. Una partida sale de los archivos de estadísticas. Para corregir un resultado equivocado, [edita el marcador del mapa](matches.es.md#editar-el-marcador-de-un-mapa).

**Entró un sustituto y el jugador que salió recibió un rage quit. ¿Se puede corregir?**
Sí. En la página de cada mapa que jugó el jugador que salió, usa el lápiz junto a su nombre para poner al sustituto en su lugar. La partida y la clasificación se recalculan. Consulta [Cambiar un jugador](matches.es.md#cambiar-un-jugador).

**¿Cómo importo los datos de mi sitio anterior?**
La migración de datos es un programa aparte, fuera de este repositorio, que sube los archivos de estadísticas antiguos por el mismo camino que usan los plugins. Consulta [la hoja de ruta](../roadmap.es.md#migración-de-datos) y, para los comandos usados en l4d2.com.br, [Trae los datos del sitio antiguo](../switch-from-old-site.es.md#trae-los-datos-del-sitio-antiguo).

**¿Puedo ejecutar dos copias del sitio detrás de un balanceador de carga?**
No. La partida en vivo se guarda en la memoria de un solo proceso. Ejecuta una sola copia.

**¿Quién ve las cuentas relacionadas de un jugador?**
Todos, de forma predeterminada, como en el sitio antiguo. [Configuración > Privacidad](administration.es.md#privacidad) puede limitarlas a los administradores. El aviso en el chat del juego es un ajuste aparte del plugin, `gameinfo_related_accounts_chat`.

**Los visitantes todavía ven la página antigua por un momento.**
Las páginas de quienes no iniciaron sesión con Steam (ranking, jugadores, partidas, ranking de la comunidad y el panel del juego) se guardan hasta 60 segundos, para que muchos visitantes y buscadores cuesten poco. Los nuevos archivos de estadísticas, la configuración, los castigos y el reprocesamiento las actualizan al instante; otros cambios, como los votos de la comunidad, los avatares o los nombres de los jugadores, aparecen en menos de un minuto. Los jugadores conectados y los administradores siempre ven la página actual.

**Un baneo dado con sm_ban no aparece en la página de Baneos.**
El servidor necesita el plugin `l4d2_bans` y el módulo Baneos debe estar encendido. Los baneos por dirección IP (`sm_banip`) no se guardan. Un baneo enviado con el sitio caído se envía de nuevo cada minuto mientras el servidor funciona. Ver [Baneos > Baneos dados en el juego](bans.es.md#baneos-dados-en-el-juego).

**Desbaneé a un jugador en el sitio y el servidor todavía lo rechaza.**
El desbaneo llega a los servidores por el módulo del servidor en vivo. Con él apagado, ejecuta `sm_playstats_ban_sync <SteamID64>` en la consola del servidor, o `removeid` con el Steam ID seguido de `writeid`.

## Solución de problemas

| Lo que ves | Qué hacer |
|---|---|
| Todas las páginas llevan a la página de configuración | La configuración inicial no está terminada. Inicia sesión con la cuenta de administrador y termínala. Consulta [Configuración inicial](first-run.es.md). |
| El código de configuración es rechazado | Cópialo de nuevo: `playstats-hub setup-code`, o `docker compose logs hub \| grep "setup code"`. Solo funciona hasta que el primer administrador inicia sesión. |
| El servidor de juego sigue en **Esperando la primera conexión** | Revisa las dos líneas en `cfg/playstats.cfg` y la línea `exec playstats.cfg` en `cfg/server.cfg` (o las dos líneas en `cfg/server.cfg`), que `l4d2_playstats_sync.smx` esté directamente en `addons/sourcemod/plugins/` y se cargue en `cfg/sharedplugins.cfg`, y que el servidor de juego pueda llegar a la dirección del sitio. Ejecuta `sm_playstats_test` en la consola del servidor. |
| El servidor de juego muestra **Clave inválida** | El token de la configuración del servidor de juego no es el actual. Copia de nuevo el token completo, o genera una nueva clave. |
| Conectado, pero no llega ningún archivo de estadísticas | Revisa **Escritura de estadísticas activada** en la lista de verificación (`sm_stats_writestats 1`), y que se haya jugado un mapa hasta el final. |
| Las partidas llegan pero todas son **Casual** | Ninguna regla de modo de juego las cuenta. La vista general avisa sobre las configuraciones sin regla. Consulta [Modos de juego](game-modes-and-campaigns.es.md#modos-de-juego). |
| Una campaña personalizada forma una partida por mapa | La campaña no está en la lista, y los mapas desconocidos solo se unen cuando se siguen unos a otros dentro de 2 horas. [Agrega la campaña](game-modes-and-campaigns.es.md#campañas) y luego reprocesa. |
| La página del servidor en vivo está vacía | El módulo del servidor en vivo tiene que estar activado, `l4d2_gameinfo_sync` tiene que estar cargado y tiene que haber una partida en curso. Después de reiniciar el sitio, espera la próxima actualización del servidor de juego. |
| `!ranking` abre un panel vacío | El juego no puede abrir la dirección HTTPS. Define `playstats_ingame_url` con una dirección HTTP simple del sitio. Consulta [Páginas dentro del juego](in-game-pages.es.md#la-dirección-que-usa-el-juego). |
| Los avatares no cargan | Se descargan en segundo plano. Espera unos minutos, o define una clave de la Steam Web API en [Configuración](administration.es.md#sitio). |
| Los jugadores perdieron la sesión después de mover el sitio | No se copió la carpeta `keys`. Solo tienen que volver a iniciar sesión. |
| Las imágenes de las skins no cargan | Las miniaturas de las skins que vienen con el hub las sirve el sitio. Para las fotos, y para las miniaturas de las skins añadidas después, revisa **Dirección de los recursos** en los [ajustes de la tienda](skin-shop.es.md#ajustes-de-la-tienda); vacío usa la dirección del servidor actual. |
| Una página muestra **Página no encontrada** | La dirección está mal, o su módulo está desactivado. Consulta [Módulos](administration.es.md#módulos). |
| Una página muestra **Algo salió mal** | El sitio encontró un error inesperado. Los detalles están en el registro: `playstats-hub logs`, o `docker compose logs hub`. |
| Detrás de un proxy inverso, el sitio ve `http` en lugar de `https`, o la dirección del proxy en lugar de la del visitante | El proxy no está en la misma máquina, así que el sitio ignora las cabeceras que reenvía. Agrega su dirección o su red a `Hub__TrustedProxies`. Consulta [Instalar con Docker > Configuración](../install-docker.es.md#configuración). |
| `docker compose ps` muestra el contenedor como `unhealthy` | El sitio no responde `/api/ping` en su propio puerto. Lee `docker compose logs hub` y comprueba que `ASPNETCORE_URLS` siga teniendo una dirección `http`. |
