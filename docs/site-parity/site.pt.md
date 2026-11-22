# Paridade do site: layout, ranking, histórico, patentes e Anti-Cheat

🌐 [English](site.md) | **Português** | [Español](site.es.md)

Todos os itens do site antigo (`l4d2-playstats-web`) nestas telas, conferidos um a um com o hub. `Onde` aponta para o arquivo antigo, relativo a `L4D2PlayStats.Web/L4D2PlayStats.Web`.

Situação: `done` (mesmo conteúdo e visual), `adapted` (presente, alterado pelo motivo indicado), `not ported` (não pode existir no hub, com o motivo).

## Layout (todas as páginas)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Metas de charset, compatibilidade com IE e viewport | Views/Shared/_Layout.cshtml:28-30 | done | |
| Meta description | _Layout.cshtml:31 | adapted | Mesmo texto por página; o padrão usa o nome do site (`SiteSettings.Name`) em vez do "L4D2 Competitive" fixo. |
| Meta keywords | _Layout.cshtml:32 | adapted | A lista fixa virou a configuração opcional `SiteSettings.MetaKeywords`. |
| Meta author | _Layout.cshtml:33 | adapted | Nome do site em vez do nome fixo. |
| Meta robots (index ou noindex por página) | _Layout.cshtml:34 | done | |
| Link canônico | _Layout.cshtml:36 | adapted | Montado a partir do endereço público das configurações, ou da requisição quando ele está vazio, em vez do `https://l4d2.com.br` fixo. |
| Open Graph: título, descrição, URL, tipo, nome do site e locale | _Layout.cshtml:37-45 | done | |
| Open Graph: imagem, URL segura e alt | _Layout.cshtml:41-43 | adapted | A imagem da própria página, como antes. A imagem padrão era um arquivo fixo no armazenamento antigo; agora é a logo do site, quando há uma enviada, e as tags ficam de fora quando não há. |
| Twitter: card, título, descrição, imagem e alt | _Layout.cshtml:47-51 | done | `summary` em vez de `summary_large_image` quando não há imagem. |
| Título no formato "Página \| Site" | _Layout.cshtml:53, PageMetadata/Infrastructure/PageMetadata.cs | done | |
| Favicon | _Layout.cshtml:55 | done | |
| Folha de estilo e script do lity | _Layout.cshtml:56, 535 | done | |
| Folhas de estilo do tema escuro (padrão) e do tema claro, escolhidas por um cookie | _Layout.cshtml:58-65 | done | Cookie `hub_theme`. |
| Estilos da página: largura do container, cor dos links, navbar, logo, ícones do cabeçalho, utilitários (`vertical-align-middle`, `bold`, `line-break-anywhere`), largura do tooltip, ícone de copiar | _Layout.cshtml:67-121 | done | Copiados como estavam. |
| Estilos do cartão de partida (`.match ...`) | _Layout.cshtml:123-143 | done | |
| `show-mobile` / `hide-mobile`, badge do servidor, ícone flutuante, cor da linha "sou eu" por tema | _Layout.cshtml:145-178 | done | |
| Coluna `slim`, regras para celular dos cartões de partida e `table-responsive` com `data-label` | _Layout.cshtml:180-234 | done | |
| Padding menor da navbar entre 768 e 930 px | _Layout.cshtml:236-241 | done | |
| Estilos da pílula de estrelas | _Layout.cshtml:243-278 | done | |
| Seções `Styles` e `Scripts` | _Layout.cshtml:281, 536 | done | |
| `imgOnError` (avatar vazio como fallback) | _Layout.cshtml:285-288 | done | |
| `copyTextToClipboard` com popover e mensagem de falha | _Layout.cshtml:290-304 | done | |
| Botão que abre a navbar no celular | _Layout.cshtml:313-318 | done | |
| Logo com link para a página inicial | _Layout.cshtml:319-321 | adapted | A logo é enviada pelo administrador (era um arquivo fixo com o nome do site antigo). Sem logo, o nome do site aparece como texto. |
| Badge de jogadores online ao lado da logo, pulsando, com link para a página do servidor | _Layout.cshtml:322-324 | done | Só aparece quando o módulo de servidor ao vivo está ligado. |
| Menu: Ranking, Últimos jogos, Servidor, Skins, Anti-Cheat | _Layout.cshtml:329-343 | done | Cada item segue o seu módulo. Comunidade (novo) vem logo depois de Ranking. Quando os itens não cabem em uma linha, os últimos vão sozinhos para dentro de "Mais". |
| Menu "Mais": Jogadores, Patentes, Histórico, com ícones e um divisor | _Layout.cshtml:344-369 | done | |
| Menu "Admin" para administradores: Bans de chat, Compras de skins, Preços das skins, com ícones | _Layout.cshtml:370-398 | done | Mais a área de Administração do hub e as configurações do Anti-Cheat. |
| Pílula de estrelas com o saldo do jogador logado, tooltip e rótulo | _Layout.cshtml:403-411, 647-681 | done | Lê `GET /api/shop/wallet`, com cache no session storage, como antes. |
| Avatar do jogador logado com link para a página do jogador, com o nome no celular | _Layout.cshtml:412-419 | done | |
| Menu de configurações: trocar o tema | _Layout.cshtml:430-447, Controllers/HomeController.cs:42 | done | `/theme/{theme}`. |
| Menu de configurações: English, Português, Español (o idioma atual fica oculto) | _Layout.cshtml:449-475, 545-554, 690-703 | adapted | Links simples (`/language?culture=`) em vez de um POST por AJAX seguido de um recarregamento. Mesmo resultado, e funciona sem script. |
| Menu de configurações: entrar com a Steam / sair | _Layout.cshtml:477-494 | adapted | Mesmos itens, ícones e textos. Sair é um `POST /logout` com o token antiforgery, enviado por um formulário oculto atrás do mesmo link, em vez do antigo `GET /logout`: outro site não consegue mais deslogar um visitante com um link ou uma imagem. O `returnUrl` continua trazendo o visitante de volta para a mesma página, só para endereços locais. |
| Links do rodapé (repositório do servidor, GitHub, Steam) | _Layout.cshtml:507-521 | adapted | Os três links fixos viraram a configuração `SiteSettings.FooterLinks` (um por linha). O padrão é um link para o projeto. |
| Ícone flutuante de "conectar" com o endereço do servidor no tooltip, oculto no celular | _Layout.cshtml:524-529, Controllers/ConnectController.cs | adapted | O endereço vem do servidor de jogo cadastrado com mais jogadores (`/connect`), e não de uma configuração fixa. Fica oculto quando nenhum servidor tem endereço. |
| Scripts do jQuery, Bootstrap, diffDOM, Chart.js e lity | _Layout.cshtml:531-535 | done | |
| Jogadores online no título da aba do navegador ("🟢 5/8 \| ...") e a atualização a cada 4 segundos | _Layout.cshtml:539-543, 585-608 | done | `GET /api/server` com os mesmos campos (`players`, `max_players`). |
| Animação `pulsate` | _Layout.cshtml:556-583, 705-707 | done | |
| Gráficos criados a partir de `data-chart` quando entram na área visível | _Layout.cshtml:610-641, 683-685, 713 | done | |
| Helper `isObject` | _Layout.cshtml:643-645 | done | |
| Tooltips em todo `data-toggle="tooltip"` | _Layout.cshtml:688 | done | |
| Tag do Google Analytics | _Layout.cshtml:717-723 | adapted | O ID fixo virou a configuração opcional `SiteSettings.GoogleAnalyticsId`. |
| Minificação do HTML e compressão HTTP | Program.cs:9-16, 65 | adapted | As respostas são comprimidas (Brotli/Gzip). O HTML não é minificado: não há efeito visível, e é uma dependência a menos. |
| Aviso no log para toda requisição que termina em erro, e a exceção no caso de falhas | Middlewares/ErrorLoggingMiddleware.cs | done | Pelo logger da aplicação; uma instalação em Linux guarda isso no journal do sistema em vez de em `logs/app.log`. Página não encontrada vai para o nível debug; todo erro de servidor vai como erro com o endereço original, também quando a própria página de erro falha ou quando um cancelamento que não veio do visitante encerra a requisição. |
| Página de erro para exceções não tratadas e para códigos como 404 | (novo) | adapted | Acréscimo do hub: o site antigo respondia um 500 vazio em produção. As páginas agora mostram uma página de erro traduzida no layout do site; a exceção continua indo para o log como acima. A API, as páginas dentro do jogo, os arquivos estáticos e as requisições feitas por scripts mantêm o status puro, e as páginas dentro do jogo recebem uma linha de texto simples no caso de exceção. |
| JSON sem nulos e com enums como texto | Program.cs:21-25 | done | |
| Login pela Steam com um cookie deslizante de 30 dias | Program.cs:28-39 | done | |
| Endereços por convenção, como `/Home/SetTheme`, `/Language/Change`, `/Account/Login` | Program.cs:67 | adapted | O hub usa endereços explícitos (`/theme/...`, `/language`, `/login`, `POST /logout`). Os endereços que as pessoas de fato abrem (`/`, `/history`, `/patents`, `/players`, `/skins`, `/anti-cheat`) são os mesmos. |
| `POST /api/cache/clear` | Controllers/Api/CacheController.cs | not ported | O site antigo guardava em cache o que lia de uma API separada e precisava ser avisado para descartar esse cache. No hub, o site e a API são uma única aplicação, e cada envio atualiza o ranking sozinho. |

## Página inicial: ranking (`/`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Metadados da página: título "Ranking", descrição, imagem e nome do primeiro jogador | PageMetadata/HomePageMetadata.cs | done | |
| Estilos da página: tamanho das medalhas, barra de progresso fina, cor da barra vermelha, margens de tabela e de parágrafo | Views/Home/Index.cshtml:18-46 | done | |
| Cabeçalho "Top 100 jogadores" | Index.cshtml:75 | done | |
| "Próximo reset em N dia(s)" | Index.cshtml:78, Models/RankingPeriodModel.cs | adapted | Conta os dias até o fim da temporada atual, seja qual for a duração dela, em vez de períodos fixos de dois meses. |
| Link "Ver rankings anteriores" | Index.cshtml:80 | done | |
| Tabela zebrada, com hover, bordas e condensada | Index.cshtml:83 | done | |
| Colunas: #, Patente, Jogador (avatar e nome), XP, Vitórias, Derrotas, MVP de SI (vezes), MVP de CI (vezes), Taxa de vitória, Jogos, Rage quit | Index.cshtml:85-97 | done | Os tank wipes foram adicionados depois. |
| Colunas ocultas no celular (XP, Derrotas, MVPs, Taxa de vitória, Jogos, Rage quit) | Index.cshtml:89-96 | done | |
| Top 100 listado, os 50 primeiros visíveis, botão "Mostrar mais" para o restante | Index.cshtml:103, 109, 198-207, 50-57 | done | |
| Linha do jogador logado destacada | Index.cshtml:109 | done | |
| Medalhas para os três primeiros lugares, com tooltip; "#N" para os demais | Index.cshtml:111-125 | done | |
| Imagem da patente com o tooltip "Nível NN - Nome" | Index.cshtml:127-129 | done | |
| Avatar com link para a página do jogador, com imagem de fallback | Index.cshtml:130-134 | done | Os avatares são baixados sob demanda, mesmo sem uma chave da API da Steam. |
| Nome do jogador, encurtado quando é longo demais, com link para a página do jogador | Index.cshtml:136 | done | |
| Rótulos pulsantes "Subiu de nível!" / "Caiu de nível!" com tooltip | Index.cshtml:137-148 | done | |
| Barra de progresso de XP: tipo e pulso conforme a situação, listrada no nível máximo, segundo segmento para o ganho ou a perda do último jogo | Index.cshtml:150-157, Models/RankingModel.cs | done | |
| Tooltip do progresso: mudança de nível, patente máxima, XP do último jogo, próximo nível, XP do total, percentual | Models/RankingModel.cs:44-71 | done | |
| Vitórias com o tooltip "Vitórias - N% Taxa de vitória" | Index.cshtml:158-162 | done | |
| Derrotas, MVP de SI, MVP de CI, Taxa de vitória, Jogos, Rage quit, cada um com o seu tooltip | Index.cshtml:163-192 | done | |
| Ainda sem ranking: os seis primeiros do último ranking | Index.cshtml:60-65, Views/Home/_LastRankingTopSix.cshtml | done | |
| Sem ranking e sem histórico: a mensagem "Não existem dados no momento..." | Index.cshtml:66-71 | done | |
| Seis primeiros: troféu, cabeçalho, período de referência | _LastRankingTopSix.cshtml:36-61 | adapted | O texto do período cobre uma temporada de qualquer duração. |
| Seis primeiros: um painel por jogador (destacado se for o jogador logado), rótulo de posição por cor, nome, avatar redondo, vitórias e derrotas, rótulos de taxa de vitória, MVP de SI, MVP de CI, jogos e rage quit | _LastRankingTopSix.cshtml:63-111 | done | Os tank wipes foram adicionados depois. |
| Seis primeiros: botão "Veja o ranking completo" | _LastRankingTopSix.cshtml:113-115 | done | Abre esse período no histórico. |
| Com o módulo de ranking desligado | (novo) | adapted | Uma página curta de boas-vindas; o site antigo não tinha módulos. |
| Seletor de período | (novo) | adapted | Acréscimo do hub acima da tabela, no estilo da página de histórico. A temporada atual é o padrão; ele também oferece as outras temporadas do ano atual, um item por ano e todo o período. Um ano e todo o período mostram a mesma tabela sem as colunas de patente e XP, que pertencem a uma temporada. |

## Histórico (`/history/{id?}`)

> **Juntado à lista de jogadores (novembro de 2026, a pedido do dono do site).** A tabela do histórico agora é a **Visão geral** de `/players`, que mantém todas as colunas abaixo (posição com medalhas, avatar, nome, vitórias com a dica da taxa de vitória, derrotas, jogos, MVPs de SI e de CI, taxa de vitória, rage quits) para todos os jogadores em vez dos 100 melhores, com o mesmo seletor de período do ranking (as temporadas do ano atual, cada ano e todo o período). `/history`, `/history/{id}` e os endereços `/ranking/...` redirecionam permanentemente para `/players` com o mesmo período, e o item Histórico saiu do menu Mais. As linhas abaixo descrevem a página como ela foi portada.

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Metadados da página: "Histórico - período", descrição, imagem e nome do vencedor | PageMetadata/HistoryPageMetadata.cs | done | |
| Estilos da página | Views/History/Index.cshtml:13-32 | done | |
| Cabeçalho "Histórico" | Index.cshtml:34 | done | |
| Seletor de período mostrando o período exibido | Index.cshtml:36-85 | done | Todo o período, cada ano e as temporadas encerradas do ano atual, como no site antigo. Uma temporada de um ano anterior ainda abre pelo endereço dela. |
| Nomes dos períodos: "De MM/YYYY até MM/YYYY", "Ano de YYYY", "Todo o período" | Index.cshtml:42-57 | done | |
| Período padrão: o ano atual | Controllers/HistoryController.cs:26 | done | |
| Colunas da tabela: #, Jogador (avatar e nome), Vitórias, Derrotas, Jogos, MVP de SI (vezes), MVP de CI (vezes), Taxa de vitória, Rage quit | Index.cshtml:87-101 | done | Os tank wipes foram adicionados depois. |
| Colunas ocultas no celular | Index.cshtml:93-99 | done | |
| Top 100, linha do jogador logado destacada, medalhas, avatar, nome encurtado, tooltips em todos os números | Index.cshtml:103-171 | done | |
| Período desconhecido: "Histórico não encontrado" com um botão de volta para o ranking, sem indexação | Views/History/NoHistory.cshtml, PageMetadata/HistoryNotFoundPageMetadata.cs | done | |
| Endereços `/ranking/season/{id}`, `/ranking/year/{year}`, `/ranking/all-time` | (primeira versão do hub) | adapted | Redirecionam para o histórico. |

## Patentes (`/patents`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Metadados da página: título, descrição, imagem e nome da patente mais alta | PageMetadata/PatentsPageMetadata.cs | adapted | A descrição usa o nome do site em vez do nome fixo. |
| Cabeçalho "Patentes" | Views/Patents/Index.cshtml:8 | done | |
| Painel "Entendendo o sistema de patentes", primeiro parágrafo | Index.cshtml:10-17 | adapted | Diz "temporada" e a duração dela em meses em vez dos períodos fixos de dois meses. |
| Segundo parágrafo com o XP de uma vitória, de uma derrota, do MVP de SI, do MVP de CI e do rage quit | Index.cshtml:18-20 | done | Valores das configurações do ranking. |
| Tabela: Nível, imagem com tooltip, descrição, experiência necessária, da mais alta para a mais baixa | Index.cshtml:24-49 | done | |
| Painel "Detalhamento dos pontos de XP" com as cinco linhas | Index.cshtml:51-64 | done | |

## Anti-Cheat (`/anti-cheat`)

A página pertence ao módulo Anti-Cheat, que vem desligado por padrão. Quando o administrador liga o módulo, a página e o item de menu dela aparecem.

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Metadados da página | PageMetadata/AntiCheatPageMetadata.cs | adapted | Usam o nome do site em vez do nome fixo. |
| Estilos da página (hero, cartões, passos, maquete da janela, FAQ, regras para celular) | Views/AntiCheat/Index.cshtml:16-306 | done | |
| Hero: antetítulo, título, texto de abertura | Index.cshtml:308-313 | adapted | O antetítulo usa o nome do site. |
| Botão de download | Index.cshtml:315-318 | adapted | Endereço vindo do serviço do Anti-Cheat ou da configuração; oculto quando nenhum dos dois fornece um endereço seguro. |
| Versão, tamanho, data de lançamento | Index.cshtml:319-333 | done | Consultados no serviço do Anti-Cheat (`/v1/client/version`), com cache de 5 minutos, ou de 30 segundos depois de uma falha. |
| "Verificar o arquivo": SHA-256 com botão de copiar e o comando do PowerShell | Index.cshtml:334-366 | done | |
| Linha de requisitos | Index.cshtml:367-370 | done | |
| Maquete da janela com as linhas de log | Index.cshtml:372-398 | done | |
| "Como usar": três passos | Index.cshtml:400-415 | done | |
| "O que a janela mostra": quatro estados com as suas cores | Index.cshtml:417-428 | done | |
| "Problemas comuns": cinco perguntas que abrem e fecham | Index.cshtml:430-447 | done | |
| "Sobre o -insecure (sem VAC)" | Index.cshtml:449-458 | adapted | Usa o nome do site em vez do endereço fixo. |
| Privacidade: o que é enviado e o que nunca é enviado | Index.cshtml:459-484 | done | |
| Jogador logado: se a conta Steam dele precisa usar o Anti-Cheat | Index.cshtml:319-325, Controllers/AntiCheatController.cs:45-55 | done | Oculto para visitantes que não estão logados. |
| Endereços do serviço e do download | Core/AppConsts.cs:15-16 | adapted | Configurações em `/admin/anti-cheat` em vez de endereços fixos. |

## Configurações do Anti-Cheat (`/admin/anti-cheat`)

O site antigo ganhou esta página com os modos de uso do plugin de anti-cheat 1.5.0. Aqui ela é o item Anti-Cheat do menu da administração, e guarda os endereços da página do Anti-Cheat abaixo das listas.

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Só administradores | Controllers/AntiCheatAdminController.cs:11-12 | adapted | Também precisa do módulo Anti-Cheat. |
| Endereço `/anti-cheat-admin` | Controllers/AntiCheatAdminController.cs:17 | adapted | A página fica em `/admin/anti-cheat`; o endereço antigo redireciona para ela, mantendo a lista e a busca. |
| Item de menu "Configurações do Anti-Cheat" | Views/Shared/_Layout.cshtml:383-388 | adapted | No grupo Anti-Cheat do menu lateral da administração em vez do menu suspenso Admin. |
| Cabeçalho com o botão "Adicionar jogador" | Views/AntiCheatAdmin/Index.cshtml:121-126 | done | |
| Texto de abertura | Index.cshtml:128 | done | |
| "Modo de uso": as duas opções com a ajuda delas, a escolhida destacada | Index.cshtml:130-146 | done | |
| Quem mudou o modo por último, no site ou no jogo, e quando; ou "Ainda não salvo no site" | Index.cshtml:147-157 | done | Datas no fuso horário das configurações do site. |
| "Salvar modo", habilitado só quando a escolha é diferente do modo salvo, depois o recarregamento | Index.cshtml:158-161, :343-361 | done | |
| Título e ajuda da lista mostrada, link para a outra lista com a contagem dela | Index.cshtml:164-168 | done | |
| Aviso quando a lista mostrada não é a que está em uso, com um link de volta | Index.cshtml:170-176 | done | |
| Contador de jogadores na lista | Index.cshtml:178-183 | done | |
| Busca por nome ou Steam ID, com "Limpar filtros" | Index.cshtml:185-195, Controllers/AntiCheatAdminController.cs:24-30 | done | |
| Item da lista: avatar, nome, Steam ID e SteamID64, data de adição e "por <administrador> no site / no jogo" | Index.cshtml:197-222 | done | O Steam ID é escrito como `STEAM_1:...`, como em todo o hub. |
| Texto de lista vazia | Index.cshtml:200 | done | |
| Paginação, 20 por página, mantendo a lista e a busca | Index.cshtml:224, Controllers/AntiCheatAdminController.cs:15 | done | |
| Modal de detalhes: avatar, nome com link para a página do jogador, IDs, link do perfil da Steam, adicionado em, adicionado por | Index.cshtml:226-250, :363-382 | done | O nome leva ao perfil da Steam quando o módulo de ranking está desligado. |
| Modal de detalhes: remover com uma segunda confirmação, depois o recarregamento | Index.cshtml:251-260, :384-406 | done | |
| Modal de adição, com a lista no título: campo de Steam ID ou URL do perfil | Index.cshtml:265-279 | done | |
| Modal de adição: prévia do jogador digitado com foto, nome, IDs e "Jogador do site" ou "Perfil Steam (nunca jogou aqui)" | Index.cshtml:285, :408-476 | done | |
| Modal de adição: "Escolher jogador" abre o seletor de jogadores | Index.cshtml:280-283, :478-496 | done | |
| Modal de adição: "Este jogador já está nesta lista." e Salvar desabilitado | Index.cshtml:425-436 | done | |
| Mensagens de sucesso e de erro, depois o recarregamento | Index.cshtml:498-520 | done | |
| Estilos (`_AdminStyles`, cartões dos modos, cabeçalho da lista, prévia, avatar redondo) | Index.cshtml:24-118 | done | |
| Painel "Página do Anti-Cheat" com os endereços do serviço e do download | novo | done | A página do hub que já existia, mantida abaixo das listas. |

## API de acesso ao Anti-Cheat e regras

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `GET /api/anti-cheat-access` (administrador) | Controllers/Api/AntiCheatAccessController.cs:20 | adapted | Os mesmos dados; o modo é escrito `everyone` ou `listed`. |
| `PUT /api/anti-cheat-access/mode` (administrador) | Controllers/Api/AntiCheatAccessController.cs:28 | adapted | Mesmo caminho e mesmo JSON. Precisa do token antiforgery da página no cabeçalho `RequestVerificationToken`. Os textos de erro são traduzidos. |
| `POST /api/anti-cheat-access/{whitelist ou required}` (administrador) | Controllers/Api/AntiCheatAccessController.cs:41 | adapted | Mesmo caminho e mesmo JSON, com o mesmo cabeçalho de token. O jogador é informado como SteamID64, Steam ID, Steam3 ou URL do perfil. |
| `DELETE /api/anti-cheat-access/{list}/{communityId}` (administrador) | Controllers/Api/AntiCheatAccessController.cs:61 | adapted | Mesmo caminho, com o mesmo cabeçalho de token. |
| `GET /api/admin/players/lookup?id=` (administrador) | Controllers/Api/AdminPlayersApiController.cs, Core/Players/Services/PlayerLookupService.cs | adapted | O nome vem do ranking, depois dos perfis dos jogadores e das conexões dos servidores de jogo, senão da Steam com a chave do site ou o perfil público, guardado por 4 horas. A foto é a cópia do site, ou a foto da Steam até o site ter uma. |
| `GET /api/anti-cheat-access/server` (servidor de jogo) | Controllers/Api/AntiCheatAccessController.cs:74 | adapted | Autorizado com o token de um servidor de jogo cadastrado, o valor de `l4d2_web_secret`, em vez do segredo único do site. Mesmo JSON; `mode` é `null` até um modo ser salvo. |
| `PUT /api/anti-cheat-access/server/mode` (servidor de jogo) | Controllers/Api/AntiCheatAccessController.cs:81 | adapted | O mesmo token acima. Mesmo JSON. |
| `POST /api/anti-cheat-access/server/{list}` e `DELETE /api/anti-cheat-access/server/{list}?player=` (servidor de jogo) | Controllers/Api/AntiCheatAccessController.cs:93, :112 | adapted | O mesmo token acima. Mesmo JSON, com o Steam ID escrito `STEAM_1:...`. |
| O modo e as duas listas em tabelas próprias, com quem as mudou e de onde | Core/AntiCheatAccess/Repositories | adapted | Tabelas `AntiCheatAccessSettings`, `AntiCheatWhitelist` e `AntiCheatRequired` no banco de dados do site em vez de tabelas do Azure com cache de 12 horas. |
| Nomes cortados em 64 caracteres; adicionar um jogador que já está na lista mantém a primeira entrada | Core/AntiCheatAccess/Services/AntiCheatAccessService.cs:73-111, :184 | done | |
| Uma alteração vinda do jogo registra "no jogo" e o administrador, `Console` por padrão | Controllers/Api/AntiCheatAccessController.cs:126-136 | done | |
| Uma alteração no site enfileira `sm_aac sync`, nunca duas vezes | Core/AntiCheatAccess/Services/AntiCheatAccessService.cs:170-182 | adapted | Enfileirado para cada servidor de jogo habilitado em vez do único servidor, gravado no registro de comandos remotos e entregue mesmo com o módulo de comandos remotos desligado. Precisa do módulo de servidor ao vivo, cujo plugin busca os comandos. Alterações vindas do jogo não o enfileiram. |
