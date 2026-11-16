# Navegador del juego

🌐 [English](in-game-browser.md) | [Português](in-game-browser.pt.md) | **Español**

Los jugadores abren el ranking dentro de Left 4 Dead 2 mediante el plugin `l4d2_ranking`, que muestra una página web en el panel MOTD del juego. Ese panel es un navegador integrado antiguo, así que cualquier página pensada para abrirse dentro del juego tiene que respetar sus límites. Este documento registra qué soporta ese navegador.

## Contenido

- [El motor](#el-motor)
- [Cómo se midió](#cómo-se-midió)
- [JavaScript](#javascript)
- [CSS](#css)
- [Imágenes](#imágenes)
- [Red](#red)
- [Aún no medido](#aún-no-medido)
- [Qué significa esto para el front-end](#qué-significa-esto-para-el-front-end)

## El motor

| Elemento | Valor |
|---|---|
| Motor | Chromium 18.0.989.0 (WebKit 535.15), publicado en 2012 |
| Integración | CEF 1 (`libcef.dll` 1.989.464, cargado a través de `chromehtml.dll`) |
| User agent | `Mozilla/5.0 (Windows; U; Windows NT 6.2; en-US; Valve Client/0; ) AppleWebKit/535.15 (KHTML, like Gecko) Chrome/18.0.989.0 Safari/535.11` |
| Tamaño del panel | 900 × 450 píxeles CSS, medido en una pantalla de 2560 × 1080 |
| Modo de renderizado | Modo estándar (`CSS1Compat`) |

El user agent contiene `Valve Client`, y un servidor puede usarlo para reconocer una solicitud que viene de dentro del juego.

## Cómo se midió

El 2026-11-15, con el cliente de Windows, versión del juego 2.2.4.3. Se abrió una página de prueba en el panel MOTD mediante `l4d2_ranking`, y la página envió sus propios resultados a un servidor local. La misma página pasa todas las verificaciones en un navegador actual, así que cada fallo de abajo es un límite del navegador del juego.

## JavaScript

El navegador solo ejecuta ES5. Cualquier sintaxis agregada en ES2015 o posterior es un error de análisis, que detiene todo el archivo de script.

| Funciona | No funciona |
|---|---|
| `var`, expresiones de función, getters de ES5, comas finales | `let` y `const`, funciones flecha, plantillas literales, clases, parámetros predeterminados, desestructuración, spread, `for...of`, generadores, `async`/`await`, notación abreviada de objetos, `?.`, `??` |

| API que funciona | API que falta |
|---|---|
| `JSON`, `querySelectorAll`, `classList`, `dataset`, `addEventListener` | `Promise`, `fetch` |
| `Function.prototype.bind`, `Array` `forEach`/`map`/`filter`, `Object.keys`, `Object.defineProperty`, `String.prototype.trim` | `Object.assign`, `Array.from`, `Array.prototype.find`, `Array.prototype.includes`, `String.prototype.includes` |
| `XMLHttpRequest`, `FormData`, `WebSocket`, `EventSource` | `Map`, `Set`, `WeakMap`, `Symbol`, `Proxy`, `Reflect` |
| `localStorage`, `sessionStorage`, cookies, `history.pushState` | `Intl`, `toLocaleString` con soporte de configuración regional |
| `matchMedia`, canvas 2D, SVG en línea, arreglos tipados | `IntersectionObserver`, `ResizeObserver`, `CSS.supports`, elementos personalizados, `<template>` |
| `webkitRequestAnimationFrame`, `WebKitMutationObserver`, `webkitMatchesSelector` (solo con prefijo) | `requestAnimationFrame`, `MutationObserver`, `Element.matches`, `Element.closest` sin prefijo |
| | `URL`, `URLSearchParams`, `TextEncoder`, WebGL, WebAssembly, service workers, `<video>`, `<input type="date">`, API del portapapeles |

## CSS

El layout se verificó renderizando dos cajas y midiendo si quedan una al lado de la otra.

| Técnica de layout | Resultado |
|---|---|
| `float` | Funciona |
| `display: inline-block` | Funciona |
| `display: table` y `table-cell` | Funciona |
| Flexbox de 2009 (`display: -webkit-box`) | Funciona |
| Flexbox (`display: flex` y `-webkit-flex`) | No funciona |
| Grid (`display: grid`) | No funciona |

| Funciona | Funciona solo con el prefijo `-webkit-` | No funciona |
|---|---|---|
| `border-radius`, `box-shadow`, `text-shadow`, `opacity` | `transform` | Propiedades personalizadas (`var()`) |
| `rgba()`, `hsl()` | `transition` | `calc()`, `clamp()` |
| `box-sizing`, `background-size`, `text-overflow` | `animation` | Unidades `vh` y `vw` |
| `position: fixed`, unidades `rem`, `pointer-events` | `linear-gradient` | `position: sticky` |
| Media queries, `:nth-child`, `:not()`, selectores de atributo | `filter`, `column-count`, `user-select` | `object-fit`, `gap`, `aspect-ratio`, `backdrop-filter` |

## Imágenes

| Formato | Resultado |
|---|---|
| PNG, JPEG, GIF | Funciona |
| SVG en un `<img>` | Funciona |
| WebP, con pérdida | Funciona |
| WebP, sin pérdida | No funciona |
| AVIF | No funciona |

## Red

Medido contra un servidor HTTP sin cifrar en la misma máquina.

| Función | Resultado |
|---|---|
| `XMLHttpRequest` con JSON | Funciona |
| Server-Sent Events (`EventSource`) | Funciona |
| WebSocket | Funciona |

## Aún no medido

La compilación del juego usada en la prueba se cerró sola antes de que la página llegara a estos dos grupos, por un motivo ajeno al navegador.

- **HTTPS.** Un navegador de 2012 es anterior a las versiones de TLS que exigen los servidores actuales, así que se espera que las páginas HTTPS fallen. La configuración de producción lo respalda: el plugin abre el ranking por HTTP sin cifrar. Hasta que se mida, las páginas dentro del juego deberían ser accesibles por HTTP.
- **Carga de bibliotecas reales.** En la prueba no se cargó ninguna biblioteca. La siguiente sección saca sus conclusiones de las mediciones de arriba y de la sintaxis encontrada en el archivo publicado de cada biblioteca.

## Qué significa esto para el front-end

**Se sabe que funcionan dentro del juego:** Bootstrap 3.4.1 y jQuery 1.12.4, que el sitio actual usa en producción por este motivo.

**Se espera que fallen dentro del juego**, según las mediciones:

| Biblioteca | Por qué |
|---|---|
| Bootstrap 4 y 5, Bulma, Tailwind | Su layout depende de flexbox, y el de las más nuevas, de propiedades personalizadas. |
| JavaScript de Bootstrap 5, htmx 2, Alpine 3, Vue 3, Chart.js 4, cliente de SignalR 8 | Sus archivos publicados contienen sintaxis de ES2015 o posterior, que aquí es un error de análisis. |
| React 18 | Sus archivos son ES5, pero necesita `Symbol`, `Map` y `Set` en tiempo de ejecución. |

**Probablemente funcionan, sin confirmar:** jQuery 3 y 4, Chart.js 2.9, Vue 2.7 y Preact 10. Sus archivos publicados solo usan sintaxis ES5.

**Reglas para una página que tiene que abrirse dentro del juego:**

- JavaScript en ES5, sin ningún paso de compilación que pueda emitir sintaxis más nueva, y sin depender de `Promise` ni de `fetch`.
- Layout con floats, `inline-block`, tablas o `-webkit-box`.
- Sin propiedades personalizadas, `calc()` ni unidades de viewport. Usa el prefijo `-webkit-` en `transform`, `transition`, `animation` y los gradientes.
- Imágenes en PNG, JPEG, GIF, SVG o WebP con pérdida.
- Diseñada para un panel de 900 × 450.
- Servida por HTTP sin cifrar.
- Las actualizaciones en vivo pueden usar polling, Server-Sent Events o WebSocket, pero el código del cliente tiene que ser ES5 escrito a mano.

**El enfoque adoptado:** dos capas en lugar de limitar todo el sitio a 2012.

- El sitio principal mantiene la base del sitio que reemplaza: Bootstrap 3 y jQuery, que se sabe que funcionan dentro del juego, con Chart.js y diffDOM.
- Tres páginas para el juego viven bajo `/ingame`: el ranking de la temporada, el resumen del jugador que abrió el panel y las últimas partidas. Siguen las reglas de arriba y son las que abre el plugin del juego.

Las páginas disponibles dentro del juego existen por duplicado, así que ese conjunto se mantiene pequeño. Una prueba falla si alguna de ellas empieza a usar algo que este navegador no tiene. La [hoja de ruta](roadmap.es.md#front-end) tiene el resto de las decisiones de front-end.
