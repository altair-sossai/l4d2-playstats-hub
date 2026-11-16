# Paridade do site: partidas

🌐 [English](matches.md) | **Português** | [Español](matches.es.md)

Porte da área de partidas do site antigo (`Views/LastMatches/*`, `LastMatchesController`, `Api/StatisticsController`, `Api/CampaignController`) para o hub.

Os caminhos da segunda coluna são relativos a `L4D2PlayStats.Web/L4D2PlayStats.Web` no repositório antigo, a não ser que comecem com `Sdk/`, `Core/` ou `api/` (o repositório da API antiga).

Situação: `done` (mesmo conteúdo e visual), `adapted` (presente, alterado pelo motivo indicado), `not ported` (não pode existir no hub).

Os textos são citados em português, como o site antigo os mostrava com o idioma português selecionado.

Como foi conferido: a página de detalhes e a página de estatísticas de uma partida real (The Parish, 12 de novembro de 2026) foram renderizadas pelo hub a partir de uma cópia dos arquivos de estatísticas de produção e comparadas com as mesmas páginas de https://l4d2.com.br. Cada elemento (tag, classes CSS e a presença de `title`, `alt`, `style`, `data-toggle`, `data-label`, `data-placement`, `colspan`, `role`, `tabindex`, `aria-label`) e cada texto visível foram contados dos dois lados: 803 elementos e 549 textos na página de detalhes, 1.420 elementos e 865 textos na página de estatísticas, sem nenhuma diferença. A lista foi comparada por captura de tela, nas larguras de desktop e de celular.

## Rotas e API

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `GET /last-matches` (lista) | Controllers/LastMatchesController.cs:21 | adapted | A rota do hub é `/matches`. `/last-matches` responde com um redirecionamento permanente para ela. |
| `GET /last-matches/details/{start}/{end}` (partida) | Controllers/LastMatchesController.cs:32 | adapted | A rota do hub é `/matches/{id}`. O endereço antigo continua sendo atendido: as duas chaves são convertidas de volta em datas de arquivo, a partida é localizada e o visitante é redirecionado de forma permanente. |
| `GET /last-matches/details/{start}/{end}/statistics/{statisticId}` (mapa) | Controllers/LastMatchesController.cs:47 | adapted | A rota do hub é `/matches/{id}/statistics/{fileId}`. O endereço antigo redireciona de forma permanente para ela. |
| O endereço de uma partida continua funcionando ao longo do tempo | Sdk/Matches/Results/MatchResult.cs:16-17 | done | O endereço antigo era formado por chaves de arquivo, que nunca mudam. No hub, uma partida ganhava um novo id toda vez que as partidas daquela noite eram reconstruídas (um mapa novo, uma correção de pontuação, uma reconstrução completa); agora a reconstrução mantém o id de uma partida que continua existindo. |
| 404 quando a partida não existe | Controllers/LastMatchesController.cs:36, :52 | done | |
| 404 quando o arquivo de estatísticas não faz parte da partida | Controllers/LastMatchesController.cs:57 | done | |
| Item de menu "Últimos jogos" ativo só na lista | Controllers/LastMatchesController.cs:23 | done | `ViewBag.LastMatches = "active"` na lista, e não nas páginas de detalhes, como antes. |
| Lista limitada às últimas 50 partidas, em qualquer modo de jogo | Core/Matches/MatchesServiceCached.cs:19, api FunctionApp/Functions/MatchesFunction.cs:21 | adapted | 50 partidas por página, da mais nova para a mais antiga, em qualquer modo de jogo, com paginação e todas as temporadas; a página antiga parava em 50 e só mostrava a temporada atual. |
| Lista em cache por uma hora | Core/Matches/MatchesServiceCached.cs:15 | not ported | O hub lê as partidas do próprio banco de dados, então não há chamada a uma API remota para guardar em cache; a lista está sempre atualizada. |
| Avatares carregados antes da renderização | Controllers/LastMatchesController.cs:68-87 | adapted | Os avatares são servidos por `/avatar/{communityId}` e baixados sob demanda; não é preciso uma chamada de pré-carregamento. |
| `PUT /api/statistics/{statisticId}/update-score` (administrador, corpo `{ team, score }`) | Controllers/Api/StatisticsController.cs:18-45, Commands/UpdateTeamScoreCommand.cs | adapted | Mesmo endereço, verbo e corpo. O id é o id de arquivo do hub. A requisição também precisa levar o token antiforgery no cabeçalho `RequestVerificationToken`, e a resposta traz o id do arquivo, o id da partida e as duas pontuações em vez do arquivo de estatísticas inteiro. |
| update-score: 404 para um arquivo desconhecido | Controllers/Api/StatisticsController.cs:23-24 | done | |
| update-score: 400 quando o arquivo não tem pontuação | Controllers/Api/StatisticsController.cs:26-30 | done | |
| update-score: só a equipe informada muda, a outra pontuação é mantida | Controllers/Api/StatisticsController.cs:32-33 | adapted | Mesma regra. Uma equipe diferente de A ou B recebe 400; o endpoint antigo salvava o arquivo de novo sem alterá-lo. |
| update-score: pontuação entre 0 e 10.000 | api Core/Modules/Statistics/Validations/UpdateScoreCommandValidator.cs | done | Respondido com 400. |
| update-score: os caches são limpos para a partida e o ranking mostrarem a nova pontuação | Controllers/Api/StatisticsController.cs:42 | done | O arquivo armazenado é reescrito, as partidas ao redor dele são reconstruídas e os rankings em cache são invalidados. |
| update-score: só administradores | Controllers/Api/StatisticsController.cs:14, :19 | done | Um visitante é mandado para o login e um jogador logado recebe 403. |
| `GET /api/campaigns` (nome e mapas de todas as campanhas) | Controllers/Api/CampaignController.cs:10-13, Core/Campaign/CampaignName.cs:164-180 | done | Lê as campanhas cadastradas no hub (a mesma lista por padrão, editável pelo administrador). |

## Números da partida (API antiga)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Equipes A e B: os jogadores de cada metade do último mapa, na ordem da lista de jogadores do arquivo | api Core/Modules/Statistics/Extensions/StatisticsExtensions.cs:53-54 | done | |
| Nome do jogador tirado do último mapa | api Core/Modules/Matches/Match.cs:163 | done | |
| Pontuação da equipe: a pontuação do último mapa | api Core/Modules/Matches/Match.cs:65 | done | |
| Totais por jogador em todos os mapas e nas duas metades (mortes, incaps, dano recebido, commons, SI mortos, dano em SI, dano no tank, pedras recebidas, dano na witch, skeets incluindo hurt e melee, melee skeets, levels incluindo hurt, crowns incluindo hurt, FF causado, dano como infectado, dano causado como tank, dano de ácido, dano de high pounce) | api Core/Modules/Matches/Match.cs:104-146 | done | |
| MVP (SI), MVP (CI) e LVP (FF) contados uma vez por metade | api Core/Modules/Matches/Match.cs:110-134 | done | |
| Totais da equipe: a soma dos jogadores da equipe | api Core/Modules/Matches/Match.cs:69-94 | done | |
| Percentuais: valor do jogador dividido pelo total da equipe, zero quando o total é zero | api Core/Modules/Matches/Match.cs:166-218 | done | |
| Vencedor: a equipe com a maior pontuação; diferença; empate | Sdk/Matches/Results/MatchResult.cs:19-29 | done | |
| Início da partida (início do primeiro mapa), fim (fim do último mapa) e tempo decorrido | api Core/Modules/Statistics/Extensions/StatisticsExtensions.cs:58-65, Match.cs:11-13 | done | |
| Horários exibidos na hora local do site | Sdk/Converters/DateTimeConverter.cs:10 | adapted | O site antigo usava o fuso horário da máquina; o hub usa o fuso horário escolhido nas configurações do site. |
| Mapas da partida, do mais antigo para o mais novo, com a pontuação depois de cada um | api Core/Modules/Matches/Match.cs:42-47, :218-223 | done | |
| "Competitiva": dois mapas ou mais e equipes de quatro | api Core/Modules/Matches/Match.cs:31 | adapted | O hub marca como competitivas as partidas que contam para o ranking, seguindo os modos de jogo que o administrador cadastrou (por padrão, a mesma regra). |
| Nome da configuração da partida | api Core/Modules/Matches/Match.cs:9 | done | |
| Arquivos de estatísticas da partida: todos os arquivos do servidor entre o primeiro e o último mapa, do mais antigo para o mais novo | api FunctionApp/Functions/StatisticsFunction.cs:64-84 | done | Um mapa repetido que a partida ignora continua listado, como antes. |

## Lista de últimos jogos (`Views/LastMatches/Index.cshtml`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título da página "Últimos jogos" | PageMetadata/LastMatchesPageMetadata.cs:17 | done | |
| Meta description "Confira as partidas mais recentes do {site}, seus placares, campanhas e jogadores." | PageMetadata/LastMatchesPageMetadata.cs:18 | adapted | O texto citava "L4D2 Competitive"; agora recebe o nome do site (novo texto em `pt-BR.matches.json` e `es-ES.matches.json`). |
| Imagem social: miniatura da campanha da partida mais recente, texto alternativo com o nome dela | PageMetadata/LastMatchesPageMetadata.cs:19-20 | done | |
| Título `h4.text-center` "Últimos jogos" | Views/LastMatches/Index.cshtml:13 | done | |
| `div.match` com um cartão por partida | Views/LastMatches/Index.cshtml:15-20 | done | |
| Filtro de temporada | (novo) | adapted | Acréscimo do hub, no visual antigo: um select pequeno à direita, acima da lista ("Todas as temporadas" por padrão), para que dê para navegar pelas temporadas passadas. Oculto enquanto há uma única temporada e um único servidor. |
| Filtro de servidor | (novo) | adapted | Acréscimo do hub: um segundo select, exibido só quando há mais de um servidor de jogo. |
| Paginação | (novo) | adapted | Acréscimo do hub, com a partial compartilhada `_Pagination`, exibida quando há mais de uma página. |
| Lista vazia | Views/LastMatches/Index.cshtml:15-20 | adapted | A página antiga não mostrava nada; o hub mostra "Nenhuma partida encontrada" no mesmo estilo de título. |

## Cartão de partida (`Views/LastMatches/_Match.cshtml`)

O cartão é `Views/Matches/_Match.cshtml` com o modelo `MatchCardModel`; `IMatchViewService.CardsAsync(filter)` monta os cartões para qualquer página (a página antiga do jogador também renderizava essa partial).

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `panel panel-primary` com padding de `6px 10px` no cabeçalho | _Match.cshtml:160-161 | done | |
| Nome da campanha em `h4` negrito | _Match.cshtml:164-166 | adapted | Uma partida em mapas de nenhuma campanha cadastrada (só é possível no hub) mostra "Campanha personalizada". |
| Nome da configuração em `small` itálico | _Match.cshtml:167 | done | |
| Nome do servidor | (novo) | adapted | Acréscimo do hub: um segundo `small` itálico depois do nome da configuração, só quando há mais de um servidor de jogo. |
| Rótulo "Competitiva" (`label-success`) ou "Casual" (`label-default`) | _Match.cshtml:168-175 | done | |
| "A equipe {0} venceu por {1:N0} pontos" | _Match.cshtml:176-178 | done | |
| Ícone de calendário e início: "Hoje às {t}", "Ontem às {t}" ou "{d} às {t}" | _Match.cshtml:181-197 | done | Hoje é o dia atual no fuso horário do site. |
| Tooltip "Data e hora em que o jogo ocorreu" | _Match.cshtml:181 | done | |
| Ícone de relógio e duração `hh:mm:ss`, só quando a partida tem um fim | _Match.cshtml:199-205 | done | |
| Tooltip "Duração do jogo em horas" | _Match.cshtml:201 | done | |
| Imagem da campanha (`img-responsive img-thumbnail img-campaign`, alt e title com a campanha) abrindo em um lightbox (`data-lity`) | _Match.cshtml:212 | done | |
| Campanha sem imagem usa `no-image.jpg` | Core/Campaign/CampaignThumb.cs:7-56 | adapted | O site antigo tinha uma lista fixa de campanhas com imagem; o hub verifica se o arquivo de imagem existe (mesmos nomes de arquivo), então uma campanha adicionada pelo administrador ganha a imagem assim que o arquivo estiver lá. |
| Botão "Detalhes" (`btn btn-danger btn-xs btn-block`), oculto nas páginas de detalhes | _Match.cshtml:213-218, Models/MatchDetailsModel.cs:12 | done | |
| Dois painéis de equipe lado a lado (`col-xs-6`), verde para a vencedora e vermelho para a perdedora | _Match.cshtml:221-225 | done | |
| Título da equipe "Equipe A/B" e pontuação (`N0`) à direita | _Match.cshtml:226-235 | done | |
| Jogadores ordenados por MVPs, depois por MVP (SI), depois por MVP (CI), depois por menos LVP | _Match.cshtml:239-244 | done | |
| Linha destacada (`it-is-me`) para o jogador logado | _Match.cshtml:246 | done | |
| Avatar de 32px com cantos arredondados, link para o jogador, nome no alt e no title, imagem de fallback em caso de erro | _Match.cshtml:247-251 | done | |
| Nome do jogador truncado com link para a página do jogador, "-" quando desconhecido | _Match.cshtml:253 | done | |
| Barra vertical separadora depois do nome quando o jogador tem algum MVP | _Match.cshtml:254-257 | done | |
| Uma estrela dourada por MVP (SI) com o tooltip "MVP (SI)" | _Match.cshtml:259-262 | done | |
| Uma estrela azul por MVP (CI) com o tooltip "MVP (CI)" | _Match.cshtml:264-267 | done | |
| Linha horizontal entre as equipes e os gráficos | _Match.cshtml:281 | done | |
| Gráfico de linha "Placar": uma série por equipe ("Equipe A", "Equipe B"), pontuação depois de cada mapa, verde para a vencedora e vermelho para a perdedora, nomes dos mapas cortados em 16 caracteres, legenda embaixo, eixo começando em zero | _Match.cshtml:26-79, :284-294 | done | |
| Gráfico de barras "Pontos ganhos por mapa": uma série por equipe com os pontos de cada mapa (diferença em relação ao mapa anterior) | _Match.cshtml:81-157, :296-306 | done | |
| A cor do texto dos gráficos segue o tema (`#222222` no claro, `#c8c8c8` no escuro) | _Match.cshtml:18-24 | done | O cookie de tema é o do hub (`hub_theme`). |
| Gráficos desenhados só quando entram na área visível (`canvas.app-chart` com `data-chart`), com 200px de altura | _Match.cshtml:285-304 | done | |
| Celular: a coluna da imagem e a coluna das equipes ocupam a largura total (`left-side`, `right-side`); gráficos empilhados (`col-sm-6 col-xs-12`) | _Match.cshtml:211, :220, :284, :296 | done | |

## Detalhes da partida (`Views/LastMatches/Details.cshtml`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título da página "Detalhes da partida - {campaign}" | PageMetadata/MatchDetailsPageMetadata.cs:18 | done | |
| Meta description "Veja os detalhes da partida em {campaign}, disputada em {date}, com placar {A} x {B}." | PageMetadata/MatchDetailsPageMetadata.cs:19 | done | |
| Imagem social: miniatura da campanha, texto alternativo com a campanha | PageMetadata/MatchDetailsPageMetadata.cs:20-21 | done | |
| Estilo `.maps .label { margin: 0 3px; }` | Details.cshtml:17-23 | done | |
| "Nenhuma partida encontrada" quando a partida não tem arquivos de estatísticas | Details.cshtml:62-72 | done | |
| Título "Detalhes da partida" | Details.cshtml:74 | done | |
| Breadcrumb: "Últimos jogos" (link para a lista) / campanha | Details.cshtml:76-83 | done | |
| Cartão da partida sem o botão "Detalhes" | Details.cshtml:85-87 | done | |
| Painel "Mapas" com o texto "Resultados por mapa" | Details.cshtml:90-96 | done | |
| Colunas da tabela de mapas: #, Mapa, Início, Fim, Decorrido, Placar (três células) | Details.cshtml:97-105 | done | |
| `#n` e o nome do mapa levam à página de estatísticas do mapa | Details.cshtml:117-126 | done | |
| Início e fim como data curta mais `HH:mm`; decorrido como `hh:mm:ss` | Details.cshtml:127-129 | done | |
| Rótulo da pontuação da equipe A (vencedora em verde, perdedora em vermelho, empate em cinza) com o tooltip "Pontuação da equipe A" | Details.cshtml:113, :130-133 | done | |
| Rótulo da diferença de pontos com o tooltip "Diferença de pontos" | Details.cshtml:143-147 | done | |
| Rótulo da pontuação da equipe B com o tooltip "Pontuação da equipe B" | Details.cshtml:114, :148-151 | done | |
| Administrador: botão de link "Editar" (tooltip "Editar") ao lado da pontuação de cada equipe | Details.cshtml:134-141, :152-159 | done | |
| `editScore`: prompt "Score da equipe X" com o valor atual, ignora entrada vazia ou não numérica, alerta "Intervalo de score inválido" fora de 0–10.000, `PUT /api/statistics/{id}/update-score`, depois mostra a página de novo | Details.cshtml:25-60 | adapted | Mesmo fluxo. O script só é enviado a administradores e a requisição leva o token antiforgery. Depois de salvar, a página abre o endereço da partida indicada na resposta (a mesma página, já que a partida mantém o id; a lista, quando a correção não deixa nenhuma partida). |
| Celular: a tabela vira linhas com rótulo (`table-responsive`, `data-label` em todas as células) | Details.cshtml:97, :117-148 | done | |
| Por equipe, painel "Equipe X - Sobrevivente" com o texto "Estatísticas da equipe X jogando como sobreviventes" | Details.cshtml:168-174 | done | |
| Cabeçalho da tabela de sobreviventes: um par de colunas por jogador (nome truncado com link para o jogador), "Total" | Details.cshtml:177-188 | done | |
| Segunda linha do cabeçalho: "Valor" e "% da equipe" abaixo de cada jogador | Details.cshtml:189-201 | done | |
| Linha "MVP de SI (vezes)": valor, participação, total | Details.cshtml:204-216 | done | |
| Linha "MVP de CI (vezes)" | Details.cshtml:218-230 | done | |
| Linha "LVP de FF (vezes)" | Details.cshtml:232-244 | done | |
| Linha "Mortes" | Details.cshtml:246-258 | done | |
| Linha "Vezes incapacitado" | Details.cshtml:260-272 | done | |
| Linha "Dano recebido de SI" com o ícone de informação e o tooltip dele | Details.cshtml:274-289 | done | |
| Linha "Commons mortos" | Details.cshtml:291-303 | done | |
| Linha "SI mortos" | Details.cshtml:305-317 | done | |
| Linha "SI - Dano causado" | Details.cshtml:319-331 | done | |
| Linha "Dano ao Tank" | Details.cshtml:333-345 | done | |
| Linha "Dano à Witch" | Details.cshtml:347-359 | done | |
| Linha "Pedras recebidas" | Details.cshtml:361-373 | done | |
| Linha "Skeets totais" | Details.cshtml:375-387 | done | |
| Linha "Melee skeets" | Details.cshtml:389-401 | done | |
| Linha "Levels totais" | Details.cshtml:403-415 | done | |
| Linha "Crowns" | Details.cshtml:417-429 | done | |
| Linha "FF causado (sem incap.)" | Details.cshtml:431-443 | done | |
| Valores como `N0`, participações como `P0` | Details.cshtml:209-212 (e todas as linhas) | done | |
| Celular: células de participação ocultas (`hide-mobile`), células de valor rotuladas com o nome do jogador, total rotulado "Total" | Details.cshtml:208-215 (e todas as linhas) | done | |
| Por equipe, painel "Equipe X - Infectado" com o texto "Estatísticas da equipe X jogando como infectados" | Details.cshtml:449-455 | done | |
| Linhas de cabeçalho da tabela de infectados (jogadores, "Valor" / "% da equipe", "Total") | Details.cshtml:457-483 | done | |
| Linha "Dano de SI (sem Tank)" com ícone de informação e tooltip | Details.cshtml:485-500 | done | |
| Linha "Dano como Tank (em pé)" | Details.cshtml:502-514 | done | |
| Linha "Dano de ácido (em pé)" | Details.cshtml:516-528 | done | |
| Linha "Dano total de pounce" com ícone de informação e tooltip | Details.cshtml:530-545 | done | |

## Estatísticas do mapa (`Views/LastMatches/Statistics.cshtml`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título da página "{map} - {campaign}" | PageMetadata/MatchStatisticsPageMetadata.cs:19 | done | |
| Meta description "Veja as estatísticas do mapa {map}, da partida em {campaign}, com placar {A} x {B}." | PageMetadata/MatchStatisticsPageMetadata.cs:20 | done | |
| Imagem social: miniatura da campanha, texto alternativo com a campanha | PageMetadata/MatchStatisticsPageMetadata.cs:21-22 | done | |
| Estilos: paddings laterais de `.round-details` a partir de 992px, quebra de linha em `.player-name` | Statistics.cshtml:21-38 | done | |
| "Nenhuma partida encontrada" quando a partida não tem arquivos de estatísticas | Statistics.cshtml:183-193 | done | |
| Título "Detalhes da partida" | Statistics.cshtml:202 | done | |
| Breadcrumb: "Últimos jogos" / campanha (link para a partida) / nome do mapa | Statistics.cshtml:204-214 | done | |
| Tabela de mapas (#, Mapa, Início, Fim, Decorrido, Placar) com o mapa atual destacado (`tr.info`) e links para cada mapa | Statistics.cshtml:216-271 | done | |
| Rótulos de pontuação da equipe A / diferença / equipe B com tooltips | Statistics.cshtml:251-265 | done | Sem o botão "Editar" aqui, como antes. |
| Abas "1º round" e "2º round", uma por metade, a primeira ativa | Statistics.cshtml:273-282 | done | |
| Painel de aba `fade`, o primeiro `in active` | Statistics.cshtml:309 | done | |
| Painel "Sobreviventes": verde quando essa equipe venceu o mapa, vermelho caso contrário | Statistics.cshtml:292, :314-317 | done | |
| Lista de sobreviventes: avatar, nome truncado com link para o jogador, destaque `it-is-me` | Statistics.cshtml:321-331 | done | |
| Sobreviventes ordenados com o MVP (SI) primeiro, o MVP (CI) em segundo e o LVP por último | Statistics.cshtml:295-303 | done | |
| Estrela dourada para o MVP (SI), estrela azul para o MVP (CI), ícone de LVP para quem causou mais fogo amigo, cada um com o seu tooltip | Statistics.cshtml:332-345 | done | |
| Painel "Infectados": verde quando essa equipe venceu o mapa, vermelho caso contrário | Statistics.cshtml:293, :355-358 | done | |
| Lista de infectados ordenada por dano: avatar, nome, dano (`N0`) com o tooltip "Dano de SI (sem Tank)", destaque `it-is-me` | Statistics.cshtml:305-307, :362-379 | done | |
| Painel "Detalhes", tabela da esquerda: todos os contadores do round diferentes de zero (reinícios, pílulas, kits, desfibriladores, commons, SI mortos, dano em SI, SI spawnados, witches, tanks, incaps, mortes, FF) | Statistics.cshtml:388-407 | done | |
| Painel "Detalhes", tabela da direita: início, fim e duração do round; início, fim e duração do pause; início, fim e duração do tank (cada um só quando existe) | Statistics.cshtml:409-476 | done | |
| Painel "Sobreviventes" (tabela de estatísticas): cabeçalho com o nome truncado de cada jogador com link para o jogador | Statistics.cshtml:488-507 | done | |
| Linhas exibidas só quando algum jogador tem valor; linhas separadoras em branco entre os grupos | Statistics.cshtml:61-128 | done | |
| Grupo: mortes, incaps, dano recebido de SI (com tooltip de informação), dano por queda | Statistics.cshtml:510-517 | done | |
| Por arma (shotgun, SMG/rifle, sniper, pistol): disparos, acertos, % de acertos, acertos em SI, % de acertos em SI, acertos no tank, % de acertos no tank, headshots, % de headshots, headshots em SI, % de headshots em SI | Statistics.cshtml:519-634 | done | |
| Grupo de commons mortos até dano em SI durante o tank | Statistics.cshtml:637 | done | |
| Grupo de dano no tank até pedras recebidas | Statistics.cshtml:638 | done | |
| Grupo de skeets até self clears, depois clears e tempo médio de clear (segundos, duas casas decimais) | Statistics.cshtml:89-92, :639-640 | done | |
| Fogo amigo causado (por tipo, a si mesmo, total) e recebido (por tipo, total) | Statistics.cshtml:643-671 | done | |
| Painel "Infectados" (tabela de estatísticas): cabeçalho com o nome de cada jogador; grupos de dano, tank, arranhões/ácido/boom (com tooltips de informação), de pounces até pendurados, commons, spawns por classe | Statistics.cshtml:677-727 | done | |
| Painel "Jogadores Conectados": #, ID da Steam, Community ID, Nome | Statistics.cshtml:735-748 | done | |
| Botão de copiar ao lado do ID da Steam e do Community ID | Statistics.cshtml:755-768 | done | |
| Nome do jogador com link para a página do jogador | Statistics.cshtml:769-773 | done | |
| Números como `N0`, percentuais como `P0` | Statistics.cshtml:95, :557 | done | |
