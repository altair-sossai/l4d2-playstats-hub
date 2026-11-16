# Navegador dentro do jogo

🌐 [English](in-game-browser.md) | **Português** | [Español](in-game-browser.es.md)

Os jogadores abrem o ranking dentro do Left 4 Dead 2 pelo plugin `l4d2_ranking`, que mostra uma página web no painel de MOTD do jogo. Esse painel é um navegador embutido antigo, então qualquer página feita para abrir dentro do jogo precisa respeitar os limites dele. Este documento registra o que esse navegador suporta.

## Conteúdo

- [O motor](#o-motor)
- [Como foi medido](#como-foi-medido)
- [JavaScript](#javascript)
- [CSS](#css)
- [Imagens](#imagens)
- [Rede](#rede)
- [Ainda não medido](#ainda-não-medido)
- [O que isso significa para o front-end](#o-que-isso-significa-para-o-front-end)

## O motor

| Item | Valor |
|---|---|
| Motor | Chromium 18.0.989.0 (WebKit 535.15), lançado em 2012 |
| Incorporação | CEF 1 (`libcef.dll` 1.989.464, carregada por meio da `chromehtml.dll`) |
| User agent | `Mozilla/5.0 (Windows; U; Windows NT 6.2; en-US; Valve Client/0; ) AppleWebKit/535.15 (KHTML, like Gecko) Chrome/18.0.989.0 Safari/535.11` |
| Tamanho do painel | 900 × 450 pixels CSS, medido em uma tela de 2560 × 1080 |
| Modo de renderização | Modo de padrões (`CSS1Compat`) |

O user agent contém `Valve Client`, que um servidor pode usar para reconhecer uma requisição que vem de dentro do jogo.

## Como foi medido

Em 2026-11-15, com o cliente Windows, versão 2.2.4.3 do jogo. Uma página de teste foi aberta no painel de MOTD pelo `l4d2_ranking`, e a página enviou os próprios resultados de volta para um servidor local. A mesma página passa em todas as verificações em um navegador atual, então cada falha abaixo é um limite do navegador do jogo.

## JavaScript

O navegador roda apenas ES5. Toda sintaxe adicionada no ES2015 ou depois é um erro de parse, que interrompe o arquivo de script inteiro.

| Funciona | Não funciona |
|---|---|
| `var`, function expressions, getters do ES5, vírgulas finais | `let` e `const`, arrow functions, template literals, classes, parâmetros padrão, desestruturação, spread, `for...of`, generators, `async`/`await`, sintaxe abreviada de objetos, `?.`, `??` |

| API que funciona | API que está ausente |
|---|---|
| `JSON`, `querySelectorAll`, `classList`, `dataset`, `addEventListener` | `Promise`, `fetch` |
| `Function.prototype.bind`, `Array` `forEach`/`map`/`filter`, `Object.keys`, `Object.defineProperty`, `String.prototype.trim` | `Object.assign`, `Array.from`, `Array.prototype.find`, `Array.prototype.includes`, `String.prototype.includes` |
| `XMLHttpRequest`, `FormData`, `WebSocket`, `EventSource` | `Map`, `Set`, `WeakMap`, `Symbol`, `Proxy`, `Reflect` |
| `localStorage`, `sessionStorage`, cookies, `history.pushState` | `Intl`, `toLocaleString` sensível à localidade |
| `matchMedia`, canvas 2D, SVG inline, typed arrays | `IntersectionObserver`, `ResizeObserver`, `CSS.supports`, custom elements, `<template>` |
| `webkitRequestAnimationFrame`, `WebKitMutationObserver`, `webkitMatchesSelector` (somente com prefixo) | `requestAnimationFrame`, `MutationObserver`, `Element.matches`, `Element.closest` sem prefixo |
| | `URL`, `URLSearchParams`, `TextEncoder`, WebGL, WebAssembly, service workers, `<video>`, `<input type="date">`, API de clipboard |

## CSS

O layout foi verificado renderizando duas caixas e medindo se elas ficam lado a lado.

| Técnica de layout | Resultado |
|---|---|
| `float` | Funciona |
| `display: inline-block` | Funciona |
| `display: table` e `table-cell` | Funciona |
| Flexbox de 2009 (`display: -webkit-box`) | Funciona |
| Flexbox (`display: flex` e `-webkit-flex`) | Não funciona |
| Grid (`display: grid`) | Não funciona |

| Funciona | Funciona somente com o prefixo `-webkit-` | Não funciona |
|---|---|---|
| `border-radius`, `box-shadow`, `text-shadow`, `opacity` | `transform` | Propriedades personalizadas (`var()`) |
| `rgba()`, `hsl()` | `transition` | `calc()`, `clamp()` |
| `box-sizing`, `background-size`, `text-overflow` | `animation` | Unidades `vh` e `vw` |
| `position: fixed`, unidades `rem`, `pointer-events` | `linear-gradient` | `position: sticky` |
| Media queries, `:nth-child`, `:not()`, seletores de atributo | `filter`, `column-count`, `user-select` | `object-fit`, `gap`, `aspect-ratio`, `backdrop-filter` |

## Imagens

| Formato | Resultado |
|---|---|
| PNG, JPEG, GIF | Funciona |
| SVG em um `<img>` | Funciona |
| WebP com perdas | Funciona |
| WebP sem perdas | Não funciona |
| AVIF | Não funciona |

## Rede

Medido contra um servidor HTTP simples na mesma máquina.

| Recurso | Resultado |
|---|---|
| `XMLHttpRequest` com JSON | Funciona |
| Server-Sent Events (`EventSource`) | Funciona |
| WebSocket | Funciona |

## Ainda não medido

A build de teste do jogo fechou sozinha antes que a página chegasse a estes dois grupos, por um motivo sem relação com o navegador.

- **HTTPS.** Um navegador de 2012 é anterior às versões de TLS que os servidores atuais exigem, então a expectativa é que páginas HTTPS falhem. O ambiente de produção reforça isso: o plugin abre o ranking por HTTP simples. Até que isso seja medido, as páginas dentro do jogo devem ser acessíveis por HTTP.
- **Carregamento de bibliotecas reais.** Nenhuma biblioteca foi carregada no teste. A próxima seção tira suas conclusões das medições acima e da sintaxe encontrada no arquivo publicado de cada biblioteca.

## O que isso significa para o front-end

**Funcionam no jogo, comprovadamente:** Bootstrap 3.4.1 e jQuery 1.12.4, que o site atual usa em produção por esse motivo.

**Devem falhar no jogo**, com base nas medições:

| Biblioteca | Por quê |
|---|---|
| Bootstrap 4 e 5, Bulma, Tailwind | O layout deles depende de flexbox, e o dos mais novos, de propriedades personalizadas. |
| JavaScript do Bootstrap 5, htmx 2, Alpine 3, Vue 3, Chart.js 4, cliente do SignalR 8 | Os arquivos publicados contêm sintaxe do ES2015 ou posterior, que aqui é um erro de parse. |
| React 18 | Os arquivos são ES5, mas ele precisa de `Symbol`, `Map` e `Set` em tempo de execução. |

**Provavelmente funcionam, sem confirmação:** jQuery 3 e 4, Chart.js 2.9, Vue 2.7 e Preact 10. Os arquivos publicados usam apenas sintaxe ES5.

**Regras para uma página que precisa abrir no jogo:**

- JavaScript em ES5, sem etapa de build que possa emitir sintaxe mais nova e sem depender de `Promise` nem de `fetch`.
- Layout com floats, `inline-block`, tabelas ou `-webkit-box`.
- Nada de propriedades personalizadas, `calc()` ou unidades de viewport. Use o prefixo `-webkit-` em `transform`, `transition`, `animation` e gradientes.
- Imagens em PNG, JPEG, GIF, SVG ou WebP com perdas.
- Projetada para um painel de 900 × 450.
- Servida por HTTP simples.
- Atualizações ao vivo podem usar polling, Server-Sent Events ou WebSocket, mas o código do cliente precisa ser ES5 escrito à mão.

**A abordagem adotada:** duas camadas, em vez de prender o site inteiro a 2012.

- O site principal mantém a base do site que ele substitui: Bootstrap 3 e jQuery, que sabidamente funcionam dentro do jogo, com Chart.js e diffDOM.
- Três páginas para o jogo ficam em `/ingame`: o ranking da temporada, o resumo do jogador que abriu o painel e as últimas partidas. Elas seguem as regras acima e são o que o plugin do jogo abre.

As páginas disponíveis no jogo existem em dobro, então esse conjunto continua pequeno. Um teste falha se uma delas começar a usar algo que este navegador não tem. O [roadmap](roadmap.pt.md#front-end) tem o restante das decisões de front-end.
