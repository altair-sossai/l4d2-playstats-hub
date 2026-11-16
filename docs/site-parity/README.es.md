# Paridad del sitio

🌐 [English](README.md) | [Português](README.pt.md) | **Español**

Este proyecto reemplaza un sitio web existente, [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) (en producción en [l4d2.com.br](https://l4d2.com.br/)). La regla para las páginas es estricta: todo lo que un visitante ve en el sitio antiguo debe estar en el nuevo, con el mismo aspecto. Por eso cada página se portó a partir de la vista antigua, sin rediseñarla, y se comprobó elemento por elemento.

## Contenido

- [Las listas de verificación](#las-listas-de-verificación)
- [Cómo leerlas](#cómo-leerlas)
- [Qué no se portó](#qué-no-se-portó)
- [Qué cambió a propósito](#qué-cambió-a-propósito)
- [Cuando cambies una página](#cuando-cambies-una-página)

## Las listas de verificación

Cada lista de verificación enumera todos los elementos de las pantallas antiguas de un área: columnas, campos, etiquetas, tooltips, iconos, botones, enlaces, gráficos, modales, filtros, scripts, reglas para teléfonos, metadatos de página, direcciones y endpoints de la API.

| Lista de verificación | Pantallas | Hechos | Adaptados | No portados |
|---|---|---|---|---|
| [site.es.md](site.es.md) | Layout, ranking, historial, patentes, Anti-Cheat | 79 | 24 | 1 |
| [players.es.md](players.es.md) | Lista de jugadores, página del jugador, comparación, jugador fuera del ranking | 150 | 23 | 0 |
| [matches.es.md](matches.es.md) | Últimas partidas, detalles de la partida, estadísticas del mapa | 128 | 18 | 1 |
| [server.es.md](server.es.md) | Servidor en vivo, chat, eventos del juego, baneos de chat | 153 | 25 | 1 |
| [skins.es.md](skins.es.md) | Tienda de skins, regalos, precios, compras | 144 | 13 | 0 |
| [admin.es.md](admin.es.md) | Páginas que solo existen aquí: asistente, administración, ranking de la comunidad | 185 | 9 | 1 |

Las cinco primeras comparan el hub con el sitio antiguo. La última compara las páginas que son nuevas aquí con su primera versión, que usaba otro aspecto, para que no se perdiera nada al convertirlas.

## Cómo leerlas

Cada línea tiene un estado.

| Estado | Significado |
|---|---|
| `done` | Presente aquí con el mismo contenido y aspecto. |
| `adapted` | Presente, pero cambiado por el motivo que figura en las notas. |
| `not ported` | No puede existir aquí, por el motivo que figura en las notas. |

La columna que indica dónde está el elemento en el sitio antiguo da un archivo y una línea del repositorio antiguo.

## Qué no se portó

Tres elementos del sitio antiguo, cada uno reemplazado por la forma en que funciona esta aplicación:

- **`POST /api/cache/clear`.** El sitio antiguo guardaba en caché lo que leía de una API separada. Aquí el sitio y la API son una sola aplicación, y una subida actualiza el ranking por sí sola.
- **La caché de una hora de la lista de partidas.** La lista se lee de la base de datos de la propia aplicación.
- **`GET /api/servers` redirigiendo a `/api/server`.** Aquí `/api/servers` es la lista pública de servidores de juego.

## Qué cambió a propósito

La mayoría de las líneas `adapted` caen en unos pocos grupos:

- **Valores fijos que pasaron a ser ajustes.** El sitio antiguo pertenecía a una sola comunidad, así que su nombre, logo, enlaces del pie de página, ID de analítica, palabras clave, direcciones del Anti-Cheat y enlaces de contacto estaban escritos en el código. Aquí los define el administrador.
- **Temporadas de cualquier duración.** Los textos y contadores que asumían períodos de dos meses siguen la temporada configurada, y las temporadas pasadas se pueden consultar.
- **Varios servidores de juego.** Con un solo servidor las páginas se ven como antes. Con más de uno aparecen un selector de servidor y el nombre del servidor.
- **Módulos.** Un bloque que pertenece a un módulo (tienda de skins, servidor en vivo, chat del sitio, comandos remotos, Anti-Cheat) solo se muestra mientras ese módulo está activado.
- **Una sola aplicación.** Las direcciones que apuntaban a una API separada apuntan a esta. Las direcciones antiguas redirigen a las nuevas.
- **Formularios y scripts.** Las solicitudes que cambian datos llevan un token antiforgery.

## Cuando cambies una página

- Mantén cierto el elemento de la lista de verificación: si cambias algo que una línea describe, actualiza la línea.
- Una página nueva sigue el mismo aspecto: el layout, los temas y los patrones de las páginas que la rodean.
- No quites un campo, una columna ni un dato de una página que vino del sitio antiguo.
