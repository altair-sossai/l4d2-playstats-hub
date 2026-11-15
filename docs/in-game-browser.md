# In-game browser

🌐 **English** | [Português](in-game-browser.pt.md) | [Español](in-game-browser.es.md)

Players open the ranking inside Left 4 Dead 2 through the `l4d2_ranking` plugin, which shows a web page in the game's MOTD panel. That panel is an old embedded browser, so any page meant to open in game has to respect its limits. This document records what that browser supports.

## Contents

- [The engine](#the-engine)
- [How it was measured](#how-it-was-measured)
- [JavaScript](#javascript)
- [CSS](#css)
- [Images](#images)
- [Network](#network)
- [Not measured yet](#not-measured-yet)
- [What this means for the front end](#what-this-means-for-the-front-end)

## The engine

| Item | Value |
|---|---|
| Engine | Chromium 18.0.989.0 (WebKit 535.15), released in 2012 |
| Embedding | CEF 1 (`libcef.dll` 1.989.464, loaded through `chromehtml.dll`) |
| User agent | `Mozilla/5.0 (Windows; U; Windows NT 6.2; en-US; Valve Client/0; ) AppleWebKit/535.15 (KHTML, like Gecko) Chrome/18.0.989.0 Safari/535.11` |
| Panel size | 900 × 450 CSS pixels, measured on a 2560 × 1080 screen |
| Rendering mode | Standards mode (`CSS1Compat`) |

The user agent contains `Valve Client`, which a server can use to recognise a request that comes from inside the game.

## How it was measured

On 2026-11-15, with the Windows client, game version 2.2.4.3. A test page was opened in the MOTD panel through `l4d2_ranking`, and the page sent its own results back to a local server. The same page passes every check in a current browser, so each failure below is a limit of the game's browser.

## JavaScript

The browser runs ES5 only. Every piece of syntax added in ES2015 or later is a parse error, which stops the whole script file.

| Works | Does not work |
|---|---|
| `var`, function expressions, ES5 getters, trailing commas | `let` and `const`, arrow functions, template literals, classes, default parameters, destructuring, spread, `for...of`, generators, `async`/`await`, object shorthand, `?.`, `??` |

| API that works | API that is missing |
|---|---|
| `JSON`, `querySelectorAll`, `classList`, `dataset`, `addEventListener` | `Promise`, `fetch` |
| `Function.prototype.bind`, `Array` `forEach`/`map`/`filter`, `Object.keys`, `Object.defineProperty`, `String.prototype.trim` | `Object.assign`, `Array.from`, `Array.prototype.find`, `Array.prototype.includes`, `String.prototype.includes` |
| `XMLHttpRequest`, `FormData`, `WebSocket`, `EventSource` | `Map`, `Set`, `WeakMap`, `Symbol`, `Proxy`, `Reflect` |
| `localStorage`, `sessionStorage`, cookies, `history.pushState` | `Intl`, locale-aware `toLocaleString` |
| `matchMedia`, canvas 2D, inline SVG, typed arrays | `IntersectionObserver`, `ResizeObserver`, `CSS.supports`, custom elements, `<template>` |
| `webkitRequestAnimationFrame`, `WebKitMutationObserver`, `webkitMatchesSelector` (prefixed only) | Unprefixed `requestAnimationFrame`, `MutationObserver`, `Element.matches`, `Element.closest` |
| | `URL`, `URLSearchParams`, `TextEncoder`, WebGL, WebAssembly, service workers, `<video>`, `<input type="date">`, clipboard API |

## CSS

Layout was checked by rendering two boxes and measuring whether they sit side by side.

| Layout technique | Result |
|---|---|
| `float` | Works |
| `display: inline-block` | Works |
| `display: table` and `table-cell` | Works |
| 2009 flexbox (`display: -webkit-box`) | Works |
| Flexbox (`display: flex` and `-webkit-flex`) | Does not work |
| Grid (`display: grid`) | Does not work |

| Works | Works only with the `-webkit-` prefix | Does not work |
|---|---|---|
| `border-radius`, `box-shadow`, `text-shadow`, `opacity` | `transform` | Custom properties (`var()`) |
| `rgba()`, `hsl()` | `transition` | `calc()`, `clamp()` |
| `box-sizing`, `background-size`, `text-overflow` | `animation` | `vh` and `vw` units |
| `position: fixed`, `rem` units, `pointer-events` | `linear-gradient` | `position: sticky` |
| Media queries, `:nth-child`, `:not()`, attribute selectors | `filter`, `column-count`, `user-select` | `object-fit`, `gap`, `aspect-ratio`, `backdrop-filter` |

## Images

| Format | Result |
|---|---|
| PNG, JPEG, GIF | Works |
| SVG in an `<img>` | Works |
| WebP, lossy | Works |
| WebP, lossless | Does not work |
| AVIF | Does not work |

## Network

Measured against a plain HTTP server on the same machine.

| Feature | Result |
|---|---|
| `XMLHttpRequest` with JSON | Works |
| Server-Sent Events (`EventSource`) | Works |
| WebSocket | Works |

## Not measured yet

The test game build closed by itself before the page reached these two groups, for a reason unrelated to the browser.

- **HTTPS.** A browser from 2012 predates the TLS versions that current servers require, so HTTPS pages are expected to fail. The production setup supports this: the plugin opens the ranking over plain HTTP. Until it is measured, in-game pages should be reachable over HTTP.
- **Loading real libraries.** No library was loaded in the test. The next section draws its conclusions from the measurements above and from the syntax found in each library's published file.

## What this means for the front end

**Known to work in game:** Bootstrap 3.4.1 and jQuery 1.12.4, which the current site uses in production for this reason.

**Expected to fail in game**, based on the measurements:

| Library | Why |
|---|---|
| Bootstrap 4 and 5, Bulma, Tailwind | Their layout depends on flexbox, and the newer ones on custom properties. |
| Bootstrap 5 JavaScript, htmx 2, Alpine 3, Vue 3, Chart.js 4, SignalR 8 client | Their published files contain ES2015 or later syntax, which is a parse error here. |
| React 18 | Its files are ES5, but it needs `Symbol`, `Map` and `Set` at run time. |

**Likely to work, not confirmed:** jQuery 3 and 4, Chart.js 2.9, Vue 2.7 and Preact 10. Their published files use ES5 syntax only.

**Rules for a page that must open in game:**

- JavaScript in ES5, with no build step that could emit newer syntax, and no dependence on `Promise` or `fetch`.
- Layout with floats, `inline-block`, tables or `-webkit-box`.
- No custom properties, `calc()` or viewport units. Prefix `transform`, `transition`, `animation` and gradients with `-webkit-`.
- Images in PNG, JPEG, GIF, SVG or lossy WebP.
- Designed for a 900 × 450 panel.
- Served over plain HTTP.
- Live updates can use polling, Server-Sent Events or WebSocket, but the client code has to be hand-written ES5.

**Proposed approach:** two layers instead of holding the whole site to 2012.

- The main site targets current browsers and is free to use a modern stack.
- A small set of in-game pages lives under its own route, follows the rules above, and is what the game plugin opens.

The cost is that pages available in game exist twice, so that set should stay small. Which pages belong to it is an open question in the [roadmap](roadmap.md#open-questions).
