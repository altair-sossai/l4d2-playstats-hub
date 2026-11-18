# Paridade do site: servidor

🌐 [English](server.md) | **Português** | [Español](server.es.md)

Checklist de tudo o que o site antigo mostrava na área de **servidor**, e de como cada item está no hub.

Os arquivos antigos ficam em `l4d2-playstats-web/L4D2PlayStats.Web`. `Web/` é o projeto `L4D2PlayStats.Web`, `Core/` é `L4D2PlayStats.Core`.

Situação: `done` (mesmo conteúdo e visual), `adapted` (presente, alterado pelo motivo indicado), `not ported` (não pode existir no hub, com o motivo).

## Conteúdo

- [Rotas](#rotas)
- [Página do servidor](#página-do-servidor)
- [Cabeçalho do placar](#cabeçalho-do-placar)
- [Jogadores](#jogadores)
- [Feed e chat](#feed-e-chat)
- [Eventos do jogo](#eventos-do-jogo)
- [Terminal](#terminal)
- [Modal de informações do servidor](#modal-de-informações-do-servidor)
- [Regras do estado ao vivo](#regras-do-estado-ao-vivo)
- [Servidor de jogo e API pública](#servidor-de-jogo-e-api-pública)
- [Ganchos do layout](#ganchos-do-layout)
- [Página de banimentos de chat](#página-de-banimentos-de-chat)
- [API e regras dos banimentos de chat](#api-e-regras-dos-banimentos-de-chat)
- [Páginas só do hub](#páginas-só-do-hub)

## Rotas

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `/server` abre a página ao vivo | `Web/Controllers/ServerController.cs:16` | adapted | O hub tem vários servidores de jogo, então a página fica em `/servers`: com um servidor ativado, ela é a própria página ao vivo; com mais de um, é um seletor, e cada servidor fica em `/servers/{publicId}`. `/server` redireciona para lá de forma permanente. |
| `/servers` redireciona para `/server` | `Web/Controllers/ServerController.cs:26` | adapted | Invertido: `/servers` é a rota do hub e `/server` redireciona para ela. |
| Partial `/server/header` | `Web/Controllers/ServerController.cs:32` | adapted | Agora `/servers/{publicId}/header`. A rota antiga redireciona (302, o destino depende do servidor) para o servidor com mais jogadores. |
| Partial `/server/players` | `Web/Controllers/ServerController.cs:40` | adapted | Agora `/servers/{publicId}/players`, com o mesmo redirecionamento. |
| Partial `/server/feed?after=`, 204 quando não há nada novo | `Web/Controllers/ServerController.cs:48` | adapted | Agora `/servers/{publicId}/feed?after=`, ainda 204 quando não há nada novo, com o mesmo redirecionamento. `after` é a sequência do feed em vez dos ticks do item (veja "Novos itens acrescentados"). |
| `/connect` redireciona para `steam://connect/<ip>` | `Web/Controllers/ConnectController.cs:8` | done | Já está no `SiteController`: vai para o servidor com mais jogadores, ou para `/servers` quando nenhum servidor tem endereço. |
| Página de administrador `/chat-bans` | `Web/Controllers/ChatBansController.cs:17` | adapted | Agora `/admin/chat-bans`, como toda página de administrador do hub. `/chat-bans` redireciona para lá de forma permanente, mantendo a query string. |
| Item de menu "Servidor" ativo (`ViewBag.Server`) | `Web/Controllers/ServerController.cs:19` | done |  |
| Itens de menu "Admin" e "Bans do Chat" ativos | `Web/Controllers/ChatBansController.cs:20` | done |  |

## Página do servidor

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Metadados da página: título "Servidor" | `Web/PageMetadata/ServerPageMetadata.cs:20` | done |  |
| Metadados da página: descrição com o mapa e os jogadores conectados | `Web/PageMetadata/ServerPageMetadata.cs:21` | adapted | O texto recebe o nome do site em vez de "L4D2 Competitive" (novo texto nos fragmentos `server`). O seletor tem a própria descrição. |
| Metadados da página: a imagem é a miniatura da campanha, o alt é a campanha ou o mapa | `Web/PageMetadata/ServerPageMetadata.cs:22` | done |  |
| Título `<h4 class="text-center">Server</h4>` | `Web/Views/Server/Index.cshtml:835` | done |  |
| Container `#game-info-header` com a partial do cabeçalho | `Web/Views/Server/Index.cshtml:837` | done |  |
| Container `#game-info-players` com a partial dos jogadores | `Web/Views/Server/Index.cshtml:841` | done |  |
| Linha `#game-info-chat`, oculta enquanto ninguém está conectado | `Web/Views/Server/Index.cshtml:845` | adapted | Mesma linha. Ela só tem esse id quando há um único servidor ativado, porque o layout a mostra e a oculta de acordo com o servidor com mais jogadores; com vários servidores, a própria página faz essa alternância a partir da partial dos jogadores. |
| Botão "connect <ip>", centralizado, oculto no celular (`hide-mobile`) | `Web/Views/Server/Index.cshtml:915` | adapted | Mesmo botão. Ele leva direto a `steam://connect/<address>` do servidor em exibição (o `/connect` antigo só fazia esse redirecionamento) e fica de fora quando o servidor não tem endereço. |
| Estilos da página (tabelas, barras de progresso, latência, feed, cores dos eventos, terminal) | `Web/Views/Server/Index.cshtml:21` | done |  |
| Cabeçalho atualizado a cada 2,6 s e aplicado com o diffDOM | `Web/Views/Server/Index.cshtml:195` | done |  |
| Jogadores atualizados a cada 2,6 s e aplicados com o diffDOM | `Web/Views/Server/Index.cshtml:211` | done |  |
| Feed atualizado a cada 1,3 s, só uma requisição por vez | `Web/Views/Server/Index.cshtml:244` | done |  |
| As requisições expiram depois de 2 s, e a próxima é agendada na conclusão | `Web/Views/Server/Index.cshtml:199` | done |  |

## Cabeçalho do placar

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título do painel: nome do servidor, nome da configuração e tamanho do time ("4v4"), unidos por " - " | `Web/Views/Server/_Header.cshtml:22` | adapted | O nome é o host name que o servidor de jogo envia no heartbeat (o site antigo consultava a Steam); antes do primeiro heartbeat, é o nome com que o servidor foi cadastrado. |
| Rótulo do anti-cheat (ligado, desligado, API indisponível) com ícone de cadeado, cor e tooltip | `Web/Views/Server/_Header.cshtml:37` | done |  |
| O rótulo do anti-cheat leva à página do anti-cheat | `Web/Views/Server/_Header.cshtml:46` | adapted | O rótulo é o mesmo. O link só é escrito enquanto o módulo de anti-cheat está ligado, porque o hub ainda não tem uma página de anti-cheat (o módulo está reservado). |
| Rótulo "IP do Servidor" com o botão de ponto de interrogação que abre o modal de informações do servidor | `Web/Views/Server/_Header.cshtml:58` | done |  |
| Valor do IP do servidor com o ícone de copiar (copia `connect <ip>`, popover "Texto copiado!", ícone conforme o tema) | `Web/Views/Server/_Header.cshtml:65` | done | O endereço é o do servidor em exibição. O ícone de copiar fica de fora quando o servidor não tem endereço. |
| Jogadores: conectados / máximo | `Web/Views/Server/_Header.cshtml:73` | adapted | Conectados é o número de jogadores no estado ao vivo (sobreviventes, infectados e espectadores) em vez da consulta à Steam. Máximo são os slots que o servidor de jogo envia no heartbeat (`maxPlayers`, adicionado ao plugin); com um plugin mais antigo, é o dobro do tamanho do time. |
| Mapa: campanha, nome do mapa e round ("1º round" / "2º round") | `Web/Views/Server/_Header.cshtml:78` | adapted | O mapa vem do heartbeat do servidor de jogo (a cada 30 s) em vez da consulta à Steam. Os nomes das campanhas vêm do catálogo do hub. |
| Mapa: rótulo Pausado, Iniciando... ou Iniciado, com tooltip | `Web/Views/Server/_Header.cshtml:96` | done |  |
| Placar: rótulos de sobreviventes e infectados, verde para quem lidera, vermelho para o outro, cinza no empate, com tooltips | `Web/Views/Server/_Header.cshtml:117` | done |  |
| Placar: rótulo de diferença quando ela não é zero | `Web/Views/Server/_Header.cshtml:131` | done |  |
| Pontos de progresso: atual / máximo e rótulo de percentual | `Web/Views/Server/_Header.cshtml:139` | done |  |
| Bônus: atual / máximo e rótulo de percentual | `Web/Views/Server/_Header.cshtml:154` | done |  |
| Percentual de progresso | `Web/Views/Server/_Header.cshtml:165` | done |  |
| Tank: percentual, "Desabilitado" e os rótulos Vivo / Morto | `Web/Views/Server/_Header.cshtml:170` | done |  |
| Bruxa: percentual ou "Desabilitado" | `Web/Views/Server/_Header.cshtml:196` | done |  |
| Layout para celular: `table-responsive` com `data-label` em todas as células | `Web/Views/Server/_Header.cshtml:55` | done |  |

## Jogadores

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| O bloco inteiro fica oculto enquanto ninguém está conectado | `Web/Views/Server/_Players.cshtml:41` | done |  |
| Blocos JSON `#survivors`, `#infecteds`, `#spectators` usados pelo terminal | `Web/Views/Server/_Players.cshtml:43` | done | Os mesmos blocos com os mesmos campos de jogador, menos `avatarUrl`, que o hub não guarda (os avatares são servidos por `/avatar/{id}`). |
| Barra de pontuação: dois segmentos dimensionados pela pontuação, verde e vermelho, "+diferença" em quem lidera, marcador tracejado no meio, tooltips | `Web/Views/Server/_Players.cshtml:55` | done |  |
| Títulos dos painéis "Sobreviventes" e "Infectados" com o rótulo de pontuação enquanto uma equipe não tem pontos | `Web/Views/Server/_Players.cshtml:83` | done |  |
| Sobrevivente: imagem do personagem com tooltip, cinza quando morto | `Web/Views/Server/_Players.cshtml:101` | done |  |
| Sobrevivente: avatar com link para a página do jogador, cinza quando morto | `Web/Views/Server/_Players.cshtml:106` | done | Avatar de `/avatar/{id}`, com o avatar vazio como fallback. O link vai para o perfil da Steam quando o módulo de ranking está desligado. |
| Sobrevivente: nome com link para a página do jogador | `Web/Views/Server/_Players.cshtml:114` | done |  |
| Sobrevivente: rótulos Admin, Morto, Incapacitado, Preto e branco | `Web/Views/Server/_Players.cshtml:116` | done |  |
| Sobrevivente: ícone de latência (baixa, média, alta) com tooltip | `Web/Views/Server/_Players.cshtml:145` | done |  |
| Sobrevivente: ícones das armas dos cinco slots, conforme o tema, com o ícone da arma branca no lugar do genérico | `Web/Views/Server/_Players.cshtml:157` | done | Os caminhos das imagens são absolutos, já que a página não fica mais na raiz. |
| Quais armas têm ícone | `Web/Extensions/WeaponExtensions.cs:7`, `Web/Extensions/MeleeWeaponExtensions.cs:7` | done |  |
| Nomes de arquivo e descrições dos ícones de armas | `Core/GameInfo/Enums/Weapon.cs`, `Core/GameInfo/Enums/MeleeWeapon.cs` | done | Adicionados aos enums do hub (atributo `WeaponName`). |
| Sobrevivente: barra de vida (permanente e temporária, cor conforme a vida, esmaecida quando incapacitado) e número | `Web/Views/Server/_Players.cshtml:191` | done |  |
| Sobrevivente: barra de progresso e percentual | `Web/Views/Server/_Players.cshtml:231` | done |  |
| Sobreviventes ordenados por personagem, separados por uma linha | `Core/GameInfo/GameInfo.cs:181`, `Web/Views/Server/_Players.cshtml:248` | done |  |
| Infectado: imagem da classe com tooltip ("nenhum" enquanto ainda não deu spawn) | `Web/Views/Server/_Players.cshtml:282` | done |  |
| Infectado: avatar e nome com link para a página do jogador | `Web/Views/Server/_Players.cshtml:290` | done | As mesmas observações do avatar do sobrevivente. |
| Infectado: rótulos Admin, Spawning, Morto | `Web/Views/Server/_Players.cshtml:302` | done |  |
| Infectado: dano causado, com tooltip | `Web/Views/Server/_Players.cshtml:325` | done |  |
| Infectado: ícone de latência | `Web/Views/Server/_Players.cshtml:330` | done |  |
| Infectado: barra de vida e "vida / máximo" | `Web/Views/Server/_Players.cshtml:342` | done |  |
| Infectados ordenados por dano, separados por uma linha | `Core/GameInfo/GameInfo.cs:188`, `Web/Views/Server/_Players.cshtml:366` | done |  |
| Espectadores: contagem e nomes como rótulos com link para a página do jogador, ordenados por nome | `Web/Views/Server/_Players.cshtml:383`, `Core/GameInfo/GameInfo.cs:195` | done |  |
| Ids de linha estáveis (`tr0-survivor-0`...) para o diffDOM aplicar as mudanças no lugar | `Web/Views/Server/_Players.cshtml:100` | done |  |
| Duas colunas no desktop, empilhadas no celular (`col-xs-12 col-sm-6`) | `Web/Views/Server/_Players.cshtml:80` | done |  |

## Feed e chat

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Painel "Mensagens do chat" com o dropdown de filtro Tudo / Eventos / Mensagens | `Web/Views/Server/Index.cshtml:848` | done |  |
| Filtro lembrado no `localStorage` (`feedFilter`) e aplicado aos novos itens | `Web/Views/Server/Index.cshtml:226` | done |  |
| Caixa do feed com 400 px de altura, rolada até o fim no carregamento e mantida no fim quando chegam novos itens | `Web/Views/Server/Index.cshtml:100` | done |  |
| Um `<p>` por item com `data-feed-when` e `data-feed-kind` | `Web/Views/Server/_Feed.cshtml:6` | done | Também leva `data-feed-sequence` (veja abaixo) e o horário do item como título ao passar o mouse. |
| Novos itens acrescentados, duplicados removidos, no máximo 300 mantidos | `Web/Views/Server/Index.cshtml:264` | adapted | Mesmo comportamento. O cursor enviado em `after` é a sequência do feed da última resposta em vez dos ticks do último item na tela: as mensagens do jogo aparecem com 15 s de atraso, então, com ticks, uma mensagem do site poderia esconder uma mensagem do jogo que chegou logo antes dela. |
| Mensagem: equipe entre colchetes no chat de equipe | `Web/Views/Server/_Message.cshtml:17` | done |  |
| Mensagem: nome do jogador colorido conforme a equipe (sobrevivente, infectado, site, espectador) e com link para a página do jogador | `Web/Views/Server/_Message.cshtml:21` | done | O link vai para o perfil da Steam quando o módulo de ranking está desligado. |
| Mensagem: "(Admin)" depois do nome | `Web/Views/Server/_Message.cshtml:24` | done |  |
| Texto da mensagem | `Web/Views/Server/_Message.cshtml:29` | done |  |
| Visitante banido: "Você foi banido do chat." e o motivo, sem formulário | `Web/Views/Server/Index.cshtml:872` | done |  |
| Logado: caixa de mensagem, botão Enviar desabilitado enquanto ela está vazia, Enter envia | `Web/Views/Server/Index.cshtml:884` | done | A caixa também limita o texto a 200 caracteres. |
| Administrador: botão do terminal com ícone e tooltip | `Web/Views/Server/Index.cshtml:887` | adapted | Exibido a um administrador só enquanto o módulo de comandos remotos está ligado, já que, sem ele, os comandos não chegariam ao servidor de jogo. |
| Sem login: botão "Entre com a Steam para usar o chat." com o ícone da Steam | `Web/Views/Server/Index.cshtml:905` | done | Depois de entrar, o visitante volta para esta página. |
| Envio: POST de JSON, feed atualizado e caixa limpa em caso de sucesso, alerta com o erro caso contrário | `Web/Views/Server/Index.cshtml:293` | adapted | Envia um formulário com o token antiforgery para `/servers/{publicId}/chat` e recebe a mesma resposta `{ success, errorMessage }`. A API antiga não tinha proteção contra requisições forjadas. Os textos de erro são traduzidos. |
| Envio estando banido: alerta e recarregamento da página | `Web/Views/Server/Index.cshtml:313` | done | HTTP 403 com o motivo, como antes. |

## Eventos do jogo

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| O nome do jogador em um evento leva ao perfil da Steam, ou é texto simples quando não há perfil | `Web/Extensions/GameEventHtmlExtensions.cs:20` | done |  |
| Partes coloridas: estrelas e destaque, jogador, ação, perigo | `Web/Views/Server/Index.cshtml:121` | done |  |
| Estrelas antes dos eventos que as têm | `Core/GameInfo/Extensions/GameEventTypeExtensions.cs:7` | done |  |
| Vômito do boomer acertou | `Web/Views/Server/GameEvents/_BoomerVomitLandedEvent.cshtml` | done |  |
| Alarme de carro disparado, com o motivo e o infectado que ajudou | `Web/Views/Server/GameEvents/_CarAlarmTriggeredEvent.cshtml` | done |  |
| Level de charger | `Web/Views/Server/GameEvents/_ChargerLevelEvent.cshtml` | done |  |
| Level de charger com chip damage | `Web/Views/Server/GameEvents/_ChargerLevelHurtEvent.cshtml` | done |  |
| Death charge, com a observação de bowling e a altura | `Web/Views/Server/GameEvents/_DeathChargeEvent.cshtml` | done |  |
| Evento desconhecido | `Web/Views/Server/GameEvents/_GameEvent.cshtml` | done |  |
| High pounce de hunter, com dano e altura | `Web/Views/Server/GameEvents/_HunterHighPounceEvent.cshtml` | done |  |
| Mix iniciado | `Web/Views/Server/GameEvents/_MixStartedEvent.cshtml` | done |  |
| Pause e despause | `Web/Views/Server/GameEvents/_PauseEvent.cshtml` | done |  |
| Morte de jogador | `Web/Views/Server/GameEvents/_PlayerDeathEvent.cshtml` | done |  |
| Round encerrado, com pontuação e diferença | `Web/Views/Server/GameEvents/_RoundEndedEvent.cshtml` | done |  |
| Round ao vivo, primeiro ou segundo | `Web/Views/Server/GameEvents/_RoundLiveEvent.cshtml` | done |  |
| Skeet (melee, sniper, granada, simples) e skeet em equipe | `Web/Views/Server/GameEvents/_SkeetEvent.cshtml` | done |  |
| Skeet com chip damage e observação de overkill | `Web/Views/Server/GameEvents/_SkeetHurtEvent.cshtml` | done |  |
| Tank virou bot | `Web/Views/Server/GameEvents/_TankBecameBotEvent.cshtml` | done |  |
| Tank morreu | `Web/Views/Server/GameEvents/_TankDiedEvent.cshtml` | done |  |
| Pedra do tank acertou | `Web/Views/Server/GameEvents/_TankRockEatenEvent.cshtml` | done |  |
| Pedra do tank skeetada | `Web/Views/Server/GameEvents/_TankRockSkeetedEvent.cshtml` | done |  |
| Tank apareceu | `Web/Views/Server/GameEvents/_TankSpawnedEvent.cshtml` | done |  |
| Língua cortada | `Web/Views/Server/GameEvents/_TongueCutEvent.cshtml` | done |  |
| Crown na witch | `Web/Views/Server/GameEvents/_WitchCrownEvent.cshtml` | done |  |
| Draw crown na witch | `Web/Views/Server/GameEvents/_WitchCrownHurtEvent.cshtml` | done |  |

## Terminal

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Modal com a prévia do comando e a lista de opções | `Web/Views/Server/Index.cshtml:923` | done |  |
| Os 24 comandos com as suas descrições e argumentos | `Web/Views/Server/Index.cshtml:324` | done |  |
| Listas de argumentos: curingas, sobreviventes, infectados, espectadores, jogadores, equipe, minutos, motivo, modo de partida, estado do anti-cheat, campanha | `Web/Views/Server/Index.cshtml:422` | done |  |
| Lista de campanhas carregada de `/api/campaigns` | `Web/Views/Server/Index.cshtml:180` | done | Novo endpoint no hub, mesmo JSON (`name`, `maps`). |
| Seleção passo a passo com "Voltar", valores com espaços entre aspas | `Web/Views/Server/Index.cshtml:617` | done |  |
| Confirmação antes de executar e, depois, envio pelo formulário do chat | `Web/Views/Server/Index.cshtml:761` | done |  |
| Um comando digitado no chat por um administrador roda no servidor de jogo | `Web/Controllers/Api/ExternalChatController.cs:43` | done | Precisa do módulo de comandos remotos. Todo comando também é gravado no registro de auditoria. |

## Modal de informações do servidor

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Modal "Info. do servidor" aberto a partir do cabeçalho | `Web/Views/Server/Index.cshtml:942` | done |  |
| Linha do IP com o rótulo "Recomendado", `connect <ip>` e o ícone de copiar | `Web/Views/Server/Index.cshtml:954` | done | Exibida quando o servidor tem endereço. |
| Linha do DNS com o rótulo "Alternativo", `connect <dns>` e o ícone de copiar | `Web/Views/Server/Index.cshtml:966` | adapted | Novo campo "endereço alternativo" no servidor de jogo (definido na administração). A linha e a explicação aparecem quando ele está preenchido. |
| Explicação de quando usar o DNS | `Web/Views/Server/Index.cshtml:980` | done | Exibida junto com a linha do DNS. |

## Regras do estado ao vivo

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Tudo o que vem do jogo é exibido com 15 s de atraso | `Core/GameInfo/GameInfo.cs:28` | done |  |
| Configuração, round e jogadores expiram depois de 2 h; o placar, depois de 10 min | `Core/GameInfo/GameInfo.cs:33` | done |  |
| O feed mantém os itens por 1 h, 250 no máximo | `Core/GameInfo/GameInfo.cs:27`, `Core/GameInfo/Structures/TimedFeed.cs:5` | done |  |
| Binds de chat são descartados ("gogogo", "tank tank", "r"...) | `Core/GameInfo/Filters/ChatBindBlacklist.cs` | done |  |
| Uma mensagem de um jogador igual à anterior é descartada | `Core/GameInfo/GameInfo.cs:122` | done |  |
| Mensagem do site: 200 caracteres no máximo | `Core/GameInfo/GameInfo.cs:154` | done |  |
| Mensagem do site: 15 s entre duas quaisquer, 30 s por visitante; administradores estão isentos | `Core/GameInfo/GameInfo.cs:161` | done |  |
| As mensagens do site são mantidas por 1 h para o servidor de jogo ler | `Core/GameInfo/GameInfo.cs:96` | done |  |
| A mensagem do site aparece no feed na hora, como equipe "Externo" | `Core/GameInfo/GameInfo.cs:175`, `Core/GameInfo/Models/ExternalChatMessage.cs:17` | done |  |
| Latência: baixa abaixo de 100 ms, média abaixo de 200 ms, alta nos demais casos | `Core/GameInfo/Models/Player.cs:25` | done |  |
| Cor da vida: vermelha quando incapacitado ou abaixo de 25, laranja abaixo de 40, verde nos demais casos; máximo de 100, ou de 300 quando incapacitado | `Core/GameInfo/Models/Survivor.cs:11` | done |  |
| Valores do placar (percentuais, diferença, líder) | `Core/GameInfo/Models/Scoreboard.cs:13` | done |  |
| Tank e witch desabilitados em 0 % | `Core/GameInfo/Models/Round.cs:14` | done |  |
| Estrelas do evento por tipo; um skeet em equipe ou um hunter bot dá uma | `Core/GameInfo/Extensions/GameEventTypeExtensions.cs`, `Core/GameInfo/Models/Events/SkeetEvent.cs:11` | done |  |
| Formato do comando de servidor (`!`, `/` ou `sm_`, 256 caracteres no máximo) | `Core/GameInfo/Models/ServerCommand.cs:7` | adapted | Mesmo formato; o hub também recusa ponto e vírgula ou quebra de linha nos argumentos, para que uma mensagem não leve dois comandos. |
| Conexões de jogadores: salvas por endereço, contas relacionadas devolvidas, limpeza | `Core/GameInfo/Services/PlayerConnectionInfoService.cs` | done | As conexões são mantidas por três meses. |
| Nome do servidor, mapa, jogadores e slots lidos da Steam a cada 10 s | `Core/Steam/ServerInfo/Services/Cache/ServerInfoServiceCached.cs` | adapted | O hub não consulta a Steam (isso exige uma chave de API e um endereço fixo). Os mesmos quatro valores vêm do próprio servidor de jogo: host name, mapa e slots no heartbeat, jogadores do estado ao vivo. |

## Servidor de jogo e API pública

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `PUT /api/game-info/configuration`, `round`, `scoreboard`, `players`, `messages` (servidor de jogo) | `Web/Controllers/Api/GameInfoController.cs:33` | done |  |
| `POST /api/game-info/events` (servidor de jogo) | `Web/Controllers/Api/GameInfoController.cs:123` | done |  |
| `POST /api/game-info/player-connection-info` (servidor de jogo) | `Web/Controllers/Api/GameInfoController.cs:132` | done |  |
| `GET /api/game-info/server-command/dequeue` (servidor de jogo) | `Web/Controllers/Api/GameInfoController.cs:144` | done |  |
| `GET /api/game-info` e `/configuration`, `/round`, `/scoreboard`, `/players`, `/messages`, `/events` (público) | `Web/Controllers/Api/GameInfoController.cs:19` | adapted | Agora por servidor: `/api/servers/{publicId}/game-info` e os mesmos subcaminhos. Os caminhos antigos redirecionam (302) para o servidor com mais jogadores. |
| `GET /api/external-chat?after=` | `Web/Controllers/Api/ExternalChatController.cs:20` | adapted | Mesmo caminho e mesmo JSON, lido pelo servidor de jogo com o token dele; cada servidor recebe só as mensagens escritas na própria página. |
| `POST /api/external-chat` (visitante logado) | `Web/Controllers/Api/ExternalChatController.cs:28` | adapted | Substituído por `POST /servers/{publicId}/chat` (veja "Envio"). Um POST não pode ser redirecionado, e o caminho antigo não é usado por nenhuma página. |
| `GET /api/server` | `Web/Controllers/Api/ServerController.cs:9` | adapted | Mesmo caminho, usado pelo layout. O JSON tem `publicId`, `name`, `address`, `map`, `players` e `max_players` em vez da resposta inteira da Steam. |
| `GET /api/servers` redireciona para `/api/server` | `Web/Controllers/Api/ServerController.cs:17` | not ported | No hub, `/api/servers` é a lista pública de servidores de jogo, então não pode redirecionar para `/api/server`. |
| `GET /api/campaigns` | `Web/Controllers/Api/CampaignController.cs` | done |  |

## Ganchos do layout

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Badge da navbar "● jogadores/slots", pulsando, com link para a página do servidor, oculto quando não há ninguém conectado | `Web/Views/Shared/_Layout.cshtml:322` | done | Já está no layout. Os números são os do cabeçalho (veja "Jogadores"). |
| Título do navegador prefixado com "🟢 jogadores/slots" | `Web/Views/Shared/_Layout.cshtml:602` | done |  |
| Linha do chat exibida ou oculta pela mesma verificação, a cada 4 s | `Web/Views/Shared/_Layout.cshtml:594` | done |  |
| Ícone flutuante de conectar, oculto no celular | `Web/Views/Shared/_Layout.cshtml:526` | done | Já está no layout. Fica oculto enquanto nenhum servidor tem endereço; o antigo estava sempre lá. |

## Página de banimentos de chat

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Só administradores | `Web/Controllers/ChatBansController.cs:10` | done | Também precisa do módulo de chat do site. |
| Cabeçalho "Bans do Chat" com o botão "Adicionar ban" | `Web/Views/ChatBans/Index.cshtml:35` | done |  |
| Texto de abertura | `Web/Views/ChatBans/Index.cshtml:42` | done |  |
| Contador de jogadores banidos | `Web/Views/ChatBans/Index.cshtml:44` | done |  |
| Busca por nome, Steam ID ou motivo, com "Limpar filtros" | `Web/Views/ChatBans/Index.cshtml:51`, `Web/Controllers/ChatBansController.cs:25` | done |  |
| Item da lista: avatar, nome, Steam ID e SteamID64, motivo em duas linhas ("Sem motivo informado") | `Web/Views/ChatBans/Index.cshtml:67` | done | O Steam ID é escrito como `STEAM_1:...`, como em todo o hub. |
| Item da lista: data do ban e "por <administrador>" | `Web/Views/ChatBans/Index.cshtml:83` | done | As datas ficam no fuso horário das configurações do site em vez do fuso horário da máquina. |
| Texto de lista vazia | `Web/Views/ChatBans/Index.cshtml:63` | done |  |
| Paginação, 20 por página, mantendo a busca | `Web/Views/ChatBans/Index.cshtml:94`, `Web/Controllers/ChatBansController.cs:14` | done |  |
| Modal de detalhes: avatar, nome com link para a página do jogador, IDs, link do perfil da Steam | `Web/Views/ChatBans/Index.cshtml:96` | done | O nome leva ao perfil da Steam quando o módulo de ranking está desligado. |
| Modal de detalhes: data do ban, atualizado em, banido por | `Web/Views/ChatBans/Index.cshtml:115` | done |  |
| Modal de detalhes: editar e salvar o motivo | `Web/Views/ChatBans/Index.cshtml:123`, `:245` | done |  |
| Modal de detalhes: remover com uma segunda confirmação | `Web/Views/ChatBans/Index.cshtml:127`, `:260` | done |  |
| Modal de adição: campo de Steam ID ou URL do perfil | `Web/Views/ChatBans/Index.cshtml:153` | done |  |
| Modal de adição: um Steam ID digitado mostra a foto e o nome do jogador | `Web/Views/ChatBans/Index.cshtml:348-371` | done | A mesma busca das configurações do Anti-Cheat, `/api/admin/players/lookup`. |
| Modal de adição: "Escolher da lista" abre o seletor de jogadores | `Web/Views/ChatBans/Index.cshtml:158`, `:334` | done |  |
| Modal de adição: motivo | `Web/Views/ChatBans/Index.cshtml:165` | done |  |
| Modal de adição: remetentes recentes com avatar, nome, última mensagem e "Já banido" | `Web/Views/ChatBans/Index.cshtml:169`, `:284` | done |  |
| Mensagens de sucesso e de erro, depois o recarregamento | `Web/Views/ChatBans/Index.cshtml:201` | done |  |
| Estilos (`_AdminStyles`, limite de linhas do motivo, avatar redondo) | `Web/Views/ChatBans/Index.cshtml:16` | done |  |

## API e regras dos banimentos de chat

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `GET /api/chat-bans` (administrador) | `Web/Controllers/Api/ChatBansController.cs:16` | done |  |
| `GET /api/chat-bans/recent-senders` (administrador) | `Web/Controllers/Api/ChatBansController.cs:25` | done |  |
| `POST /api/chat-bans` cria ou atualiza um banimento (administrador) | `Web/Controllers/Api/ChatBansController.cs:34` | adapted | Mesmo caminho e mesmo JSON. Precisa do token antiforgery da página no cabeçalho `RequestVerificationToken`; a API antiga não tinha proteção contra requisições forjadas. Os textos de erro são traduzidos. |
| `DELETE /api/chat-bans/{communityId}` (administrador) | `Web/Controllers/Api/ChatBansController.cs:51` | adapted | Mesmo caminho, com o mesmo cabeçalho de token. |
| O jogador é informado como SteamID64, Steam ID, Steam3 ou URL do perfil | `Core/ChatBans/Services/ChatBanService.cs:82` | done |  |
| Um banimento novo guarda quem o criou e quando; uma atualização mantém esses dados e define "atualizado em" | `Core/ChatBans/Services/ChatBanService.cs:89` | done |  |
| O nome só é substituído quando um novo é informado | `Core/ChatBans/Services/ChatBanService.cs:109` | done |  |
| Banimentos listados do mais novo para o mais antigo | `Core/ChatBans/Services/ChatBanService.cs:39` | done |  |
| Remetentes recentes: a última mensagem de cada visitante, da mais nova para a mais antiga, 30 no máximo, marcados quando já estão banidos | `Core/ChatBans/Services/ChatBanService.cs:52` | done |  |
| Um visitante banido não consegue enviar: HTTP 403 com o motivo | `Web/Controllers/Api/ExternalChatController.cs:32` | done |  |

## Páginas só do hub

Estas não existiam no site antigo. Estão listadas para que nada do que o hub tinha se perca no porte.

| Item | Onde no hub | Situação | Observações |
|---|---|---|---|
| Seletor de servidor em `/servers` quando há mais de um servidor ativado | `Views/Servers/Index.cshtml` | done | Painéis no visual do site, atualizados a cada 10 s com o diffDOM. |
| Troca de servidor na página ao vivo quando há mais de um servidor ativado | `Views/Servers/Live.cshtml` | done | Pílulas abaixo do título. |
| Página de conexões de jogadores (administrador) | `Views/PlayerConnections/Index.cshtml` | done | Reestilizada com os estilos de administrador; a busca aceita qualquer forma de escrever um Steam ID. |
| Página de comandos remotos com o registro de auditoria (administrador) | `Views/RemoteCommands/Index.cshtml` | done | Reestilizada com os estilos de administrador. |
| JSON público de um servidor (`/api/servers/{publicId}/game-info...`) | `Controllers/Api/LiveApiController.cs` | done | Sem alterações. |

## Observações

- Totais: 154 `done`, 25 `adapted`, 1 `not ported`.
- As tabelas antigas de jogadores não mostram ícone de patente: nem `Views/Server/_Players.cshtml` nem a página em produção têm um, então não há nada a portar ali.
- As situações foram conferidas com as páginas renderizadas: lado a lado com `https://l4d2.com.br/server` (desktop, largura de celular e tema claro), com uma partida simulada que envia todos os eventos do jogo, e controlando as páginas em um navegador headless (filtro do feed, envio de uma mensagem, o terminal, o modal de informações do servidor, todos os modais da página de banimentos de chat).
- O plugin do servidor de jogo envia os slots do servidor no heartbeat (`maxPlayers`) a partir desta alteração. Um servidor de jogo que ainda roda o plugin anterior mostra o dobro do tamanho do time como máximo.
