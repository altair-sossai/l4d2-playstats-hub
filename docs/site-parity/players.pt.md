# Paridade do site: jogadores

🌐 [English](players.md) | **Português** | [Español](players.es.md)

Porte da área de jogadores do site antigo (`Views/Players/*`, `PlayersController`, `Api/PlayersController`, `Api/PunishmentsController`, os modelos de jogador e os metadados de página) para o hub.

Os caminhos da segunda coluna são relativos a `L4D2PlayStats.Web/L4D2PlayStats.Web` no repositório antigo, a não ser que comecem com `Core/` ou `Sdk/` (os outros projetos do site antigo) ou com `api/` (o repositório da API antiga).

Situação: `done` (mesmo conteúdo e visual), `adapted` (presente, alterado pelo motivo indicado), `not ported` (não pode existir no hub).

Conferido com as páginas renderizadas a partir de uma cópia dos dados de produção e com https://l4d2.com.br (`/players`, `/player/76561199157667941`, `/player/76561199157667941/76561199429077716`, `/player/76561190000000001`), em 1280 px e na largura de celular, em inglês, no tema escuro e (só o HTML) no tema claro. A contagem de linhas, células, tooltips, botões de copiar, gráficos, modais, cartões de relação e cartões de campanha do HTML antigo e do novo é a mesma para os mesmos dados.

## Rotas e API

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `GET /players` (lista) | Controllers/PlayersController.cs:26-39 | done | Mesmo endereço. |
| Query `orderBy` (uma das 25 estatísticas, padrão `Experience`) | Controllers/PlayersController.cs:27, Core/Players/Enums/PlayerResultProperty.cs | done | Mesmos nomes e mesma ordem (`Models/Players/PlayerResultProperty.cs`). Um valor desconhecido volta para `Experience`. Sem `orderBy`, a página agora mostra a Visão geral, a antiga tabela do histórico, ordenada com `sort`. |
| Query `asc` (padrão `false`) | Controllers/PlayersController.cs:27 | done | |
| Item de menu "Jogadores" (dentro de "Mais") ativo na lista | Controllers/PlayersController.cs:29 | done | `ViewBag.Players = "active"`; não é definido na página do jogador, como antes. |
| `GET /player/{communityId}/{compareWith?}` (jogador e comparação) | Controllers/PlayersController.cs:41-97 | done | Mesmo endereço. |
| `communityId <= 0` mostra a página "fora do ranking" | Controllers/PlayersController.cs:48-49 | adapted | Um endereço que não é um SteamID64 válido (`/player/0`, `/player/null`, `/player/5`) responde 404. Todo SteamID64 válido recebe a página "fora do ranking", também com o módulo de ranking desligado. |
| Um jogador fora do ranking mostra a página "fora do ranking" com as informações de conexão (HTTP 200) | Controllers/PlayersController.cs:55-64 | done | A primeira versão do hub respondia 404 aqui. |
| `compareWith` que não está no ranking é ignorado | Controllers/PlayersController.cs:53 | done | |
| Ranking da temporada atual como fonte das duas páginas | Controllers/PlayersController.cs:31, :51, Core/Ranking/Services/RankingServiceCached.cs:11-19 | adapted | A chamada da API antiga devolvia só os 100 primeiros jogadores do ranking, então a lista parava em 100 e o jogador 101 caía na página "fora do ranking". O hub usa o ranking inteiro da temporada. |
| Query `season` (novo) | - | adapted | Acréscimo do hub: as duas páginas aceitam `?season={id}`, `?year={year}` ou `?period=all-time` para mostrar um período passado. Uma temporada desconhecida responde 404. Sem o parâmetro, a temporada atual é mostrada, como antes. |
| Avatares carregados antes da renderização | Controllers/PlayersController.cs:33, :60-61, :66-69, :85 | adapted | Os avatares são servidos por `/avatar/{communityId}` e baixados sob demanda; não é preciso uma chamada de pré-carregamento. |
| Últimos jogos: as 50 partidas mais recentes da temporada, em qualquer modo de jogo, filtradas para aquelas em que um dos dois jogadores está em uma equipe | Controllers/PlayersController.cs:77, Core/Matches/MatchesServiceCached.cs:19, Models/PlayerDetailsModel.cs:22-32 | done | Mesma regra, sobre a temporada exibida e todos os servidores de jogo (`IPlayerMatchService`, `AppConsts.PlayerRecentMatches`). |
| Contas relacionadas a partir das informações de conexão | Controllers/PlayersController.cs:78, Core/GameInfo/Services/PlayerConnectionInfoService.cs:75-108 | adapted | `IPlayerConnectionService.RelatedAsync`. Vazio quando o módulo de servidor ao vivo está desligado, e para visitantes que não são administradores quando Configurações > Privacidade restringe as contas relacionadas aos administradores (o padrão é todos, como antes). |
| Skins e carteira de cada jogador | Controllers/PlayersController.cs:87-93, :99-106 | done | Só são lidas quando o módulo da loja de skins está ligado. |
| Progresso de patente de cada jogador | Controllers/PlayersController.cs:71-75 | done | `RankingModel` / `PatentProgressModel` da base. |
| `POST /api/punishments` (administrador, corpo `{ communityId, lostExperiencePoints }`) | Controllers/Api/PunishmentsController.cs:15-24, api Core/Modules/Punishments/Services/PunishmentsService.cs:14-31 | adapted | Mesmo endereço, verbo e corpo; os pontos são somados ao que o jogador já perdeu, como antes. O corpo também aceita `seasonId` (a temporada exibida; a atual quando ausente), e a chamada exige o token antiforgery no cabeçalho `RequestVerificationToken`. A resposta traz o total do jogador na temporada. |
| `DELETE /api/punishments/{communityId}` (administrador) | Controllers/Api/PunishmentsController.cs:26-35 | adapted | Mesmo endereço e verbo, resposta 204. Aceita `?season={id}` e o cabeçalho antiforgery. |
| API de punições: só administradores, logados | Controllers/Api/PunishmentsController.cs:11, :16, :27 | done | 401 para um visitante, 403 para um jogador. |
| API de punições: os caches são limpos para o ranking mostrar a nova experiência | Controllers/Api/PunishmentsController.cs:21, :32 | done | `HubState.DataChanged()` invalida os rankings em cache. |
| API de punições: validação do comando | api Core/Modules/Punishments/Validations/PunishmentCommandValidator.cs | done | 400 para um jogador inválido ou para pontos que não são positivos. |
| `GET /api/players/patents` | Controllers/Api/PlayersController.cs:15-31 | done | Já existe no hub (`RankingApiController`); não é chamado por estas páginas. |
| `GET /api/players/skins`, `GET /api/players/{communityId}/wallet`, `POST /api/players/{communityId}/skins/equip` | Controllers/Api/PlayersController.cs:33-69 | done | Já existem no hub (`PlayerSkinsApiController`); são endpoints do plugin e do layout, não chamados por estas páginas. |
| Ranking em cache por uma hora | Core/Ranking/Services/RankingServiceCached.cs:13-18 | adapted | O hub mantém o ranking calculado em cache na memória e o descarta sempre que os dados mudam, então as páginas nunca ficam uma hora atrasadas. |

## Lista de jogadores (`/players`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título "Jogadores" | Views/Players/Index.cshtml:63 | done | |
| Estilos da página (margens de tabela, de parágrafo e de cabeçalho) | Views/Players/Index.cshtml:47-61 | done | |
| Dropdown "Ordenar por: **estatística**" com o caret, agora "Estatística: **estatística**", com um item Visão geral | Views/Players/Index.cshtml:67-71 | done | |
| Itens do dropdown: as 25 estatísticas, cada uma com link para `orderBy={statistic}&asc=False` | Views/Players/Index.cshtml:72-81 | done | O link mantém a temporada exibida. |
| Nomes das estatísticas: Experiência, Jogos, Vitórias, Derrotas, Rage quit, MVP de SI (vezes), MVP de CI (vezes), LVP de FF (vezes), Mortes, Vezes incapacitado, Dano recebido de SI, Commons mortos, SI mortos, Dano aos SI, Dano ao Tank, Pedras recebidas, Skeets totais, Melee skeets, Levels totais, Crowns totais, FF causado (sem incap.), Dano de SI (sem Tank), Dano como Tank (em pé), Dano de ácido (em pé), Dano total de pounce | Views/Players/Index.cshtml:15-27, Resources/SharedResource*.resx | done | `Helpers/PlayerProperties.cs`. Os tank wipes foram adicionados depois. |
| Seletor de período (novo) | - | adapted | Acréscimo do hub, ao lado do dropdown "Estatística" e no estilo da página de histórico: "Período exibido: **De MM/YYYY até MM/YYYY**". Lista as temporadas do ano atual e depois um item por ano ("Ano de YYYY"), que mostra o ranking e as partidas daquele ano inteiro, e "Todo o período". Só aparece quando há mais de uma opção. |
| Tabela `table-striped table-hover table-bordered table-condensed` | Views/Players/Index.cshtml:86 | done | |
| Coluna `#` (estreita, centralizada) | Views/Players/Index.cshtml:89, :113 | done | |
| Coluna "Jogador" ocupando duas células (avatar e nome) | Views/Players/Index.cshtml:90 | done | |
| Coluna com a estatística escolhida, com link para a ordem invertida e com a seta (↑ crescente, ↓ decrescente) | Views/Players/Index.cshtml:91-97 | done | |
| Coluna "Por jogo", oculta quando a estatística é "Jogos" | Views/Players/Index.cshtml:44, :98-101, :131-139 | done | |
| Posição: 1, 2, 3... na ordem decrescente; em contagem regressiva a partir do número de jogadores na crescente | Views/Players/Index.cshtml:105-107, :142 | done | |
| Páginas de 100 jogadores (novo) | - | adapted | Acréscimo do hub: a lista mostra 100 jogadores por página com os links das páginas embaixo da tabela, que mantêm `orderBy`, `asc` e o período. A posição continua de onde a página anterior parou, nas duas ordens. Com até 100 jogadores não há links de página e a página fica como antes. |
| Linha destacada (`it-is-me`) para o jogador logado | Views/Players/Index.cshtml:112 | done | |
| Avatar de 40 px, arredondado, com link, título e a imagem de fallback em caso de erro | Views/Players/Index.cshtml:114-118 | done | |
| Nome truncado (`TruncatePlayerName`) com link para a página do jogador | Views/Players/Index.cshtml:119-121 | done | O link mantém a temporada exibida. |
| Valor da estatística (`N0`) | Views/Players/Index.cshtml:124 | done | |
| Ícone de ajuda com tooltip no valor de "Dano recebido de SI", "Dano de SI (sem Tank)" e "Dano total de pounce" | Views/Players/Index.cshtml:29-38, :125-128 | done | Os mesmos três textos; `aria-label` "Mais informações". |
| Média por jogo (`N2`, `-` sem jogos) | Views/Players/Index.cshtml:135, Core/Players/Player.cs:26 | done | |
| Ícone de informação ao lado da média com o tooltip "Jogos: n" | Views/Players/Index.cshtml:136 | done | |
| Ordenação: por valor, empates pelo community id, nos dois sentidos | Core/Players/Extensions/PlayerResultExtensions.cs:13-15 | done | `PlayerProperties.Sort`; coberta por um teste. |
| Metadados da página: título "Jogadores" | PageMetadata/PlayersPageMetadata.cs:14 | done | |
| Metadados da página: descrição "Consulte as estatísticas de {n} jogadores do servidor ..." | PageMetadata/PlayersPageMetadata.cs:15 | adapted | O texto citava "L4D2 Competitive"; agora recebe o nome do site (nova chave nos fragmentos de recursos `players`). |
| Metadados da página: imagem e texto alternativo do primeiro jogador da lista | PageMetadata/PlayersPageMetadata.cs:12, :16-17 | done | |

## Página do jogador (`/player/{communityId}/{compareWith?}`): estrutura

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título "Detalhes do jogador" | Views/Players/Details.cshtml:584 | done | |
| Seletor de período (novo) | - | adapted | Acréscimo do hub abaixo do título, o mesmo dropdown da lista. Ele mantém o jogador comparado e a aba aberta. |
| Coluna esquerda (`col-xs-2 left-side`): dropdown com o nome do jogador (10 caracteres) e o caret | Views/Players/Details.cshtml:587-592 | done | |
| Lista do dropdown esquerdo: todos os outros jogadores do ranking no formato "#posição - nome", trocando o primeiro jogador e mantendo a comparação | Views/Players/Details.cshtml:593-600 | done | Lista com rolagem de 175 px (`players-dropdown-menu`). |
| Avatar do primeiro jogador (`img-thumbnail`, abre ampliado com o lity) | Views/Players/Details.cshtml:603 | done | |
| Linha separadora | Views/Players/Details.cshtml:605 | done | |
| Segundo dropdown, em amarelo: o nome do jogador comparado ou "Comparar..." | Views/Players/Details.cshtml:607-611 | done | |
| Segundo dropdown: "Remover comparação" com o ícone de remover e um divisor, durante uma comparação | Views/Players/Details.cshtml:613-621 | done | |
| Lista do segundo dropdown: todos os outros jogadores do ranking, em amarelo, para escolher com quem comparar | Views/Players/Details.cshtml:622-629 | done | |
| Avatar do segundo jogador com a moldura amarela, abre ampliado | Views/Players/Details.cshtml:632-635 | done | |
| Abas: Geral, Campanhas (badge com a contagem), Skins (badge com as skins dos dois jogadores), Relações (badge, só quando há relações) | Views/Players/Details.cshtml:639-661 | adapted | A aba Skins só existe enquanto o módulo da loja de skins está ligado. O resto é igual. |
| Estilos da página (skins, badges, cabeçalhos, progresso, altura do dropdown, `second` amarelo, separador, nomes anteriores, blocos de sequência, abas, cartões e barras de campanha) | Views/Players/Details.cshtml:187-500 | done | Copiados como estavam; uma regra a mais para uma skin sem miniatura. |
| Celular (até 700 px): abas menores, as duas colunas em largura total, linhas `mobile-row-label` do confronto direto exibidas como rótulos em bloco | Views/Players/Details.cshtml:476-499 | done | |
| Tabelas como `table-responsive` com `data-label` em todas as células (rótulo antes do valor no celular) | Views/Players/Details.cshtml:671-1233 | done | |
| Script: abrir uma aba grava o hash dela no endereço e dispara um resize (para os gráficos com carregamento tardio carregarem) | Views/Players/Details.cshtml:549-554 | done | |
| Script: a aba indicada no hash do endereço abre no carregamento | Views/Players/Details.cshtml:556-557 | done | |
| Script: os links dos dropdowns de jogador e dos cartões de relação mantêm a aba aberta | Views/Players/Details.cshtml:559-562 | done | O seletor de período faz o mesmo. |
| Metadados da página: título "{name} - Detalhes do jogador", ou "{first} vs {second}" em uma comparação | PageMetadata/PlayerDetailsPageMetadata.cs:16-18 | done | |
| Metadados da página: descrição "Veja as estatísticas de {name}: posição #{n}, {n} jogos e {n} vitórias." | PageMetadata/PlayerDetailsPageMetadata.cs:20-21 | done | |
| Metadados da página: descrição de uma comparação "Compare o desempenho e as estatísticas de {first} e {second} no ..." | PageMetadata/PlayerDetailsPageMetadata.cs:22 | adapted | O texto citava "L4D2 Competitive"; agora recebe o nome do site. |
| Metadados da página: avatar do primeiro jogador como imagem, nome como texto alternativo, tipo Open Graph `profile` | PageMetadata/PlayerDetailsPageMetadata.cs:24-29 | done | |

## Página do jogador: aba Geral, "Estatísticas gerais"

Em uma comparação, todo valor é seguido de ` | ` e do valor do segundo jogador em amarelo, como na página antiga.

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Painel "Estatísticas gerais" | Views/Players/Details.cshtml:666-669 | done | |
| Ranking (`#position`) | Views/Players/Details.cshtml:674-682 | done | |
| ID da Steam com o ícone de copiar e o popover "Texto copiado!" | Views/Players/Details.cshtml:684-700 | done | Mesmo campo, mesmo ícone (escuro ou claro conforme o tema) e mesmo comportamento, escrito como `STEAM_0:x:y`, igual ao site antigo. |
| Nome (segundo jogador em uma nova linha) | Views/Players/Details.cshtml:703-711 | adapted | O nome de exibição do cadastro de jogadores (`MainName`): o nome forçado; se não houver, o nome mais usado; se não houver, o nome Steam. |
| URL do perfil como link em uma nova aba, com o botão de copiar | Views/Players/Details.cshtml:713-733 | done | |
| Nomes anteriores: até 5 rótulos com tooltip; os do segundo jogador em rótulos amarelos depois do separador; a linha só existe quando o primeiro jogador tem nomes | Views/Players/Details.cshtml:735-754 | done | |
| Patente: imagem de 32 px com o tooltip "{name} - Nível nn - patente" (linha oculta no celular) | Views/Players/Details.cshtml:755-764 | done | |
| XP: barra de progresso em duas partes (progresso e última mudança) com o tooltip em HTML (subida ou queda de nível, último jogo, próximo nível, XP, progresso); uma segunda barra para o jogador comparado | Views/Players/Details.cshtml:766-784, Models/RankingModel.cs | done | |
| Jogos | Views/Players/Details.cshtml:787-795 | done | |
| Vitórias | Views/Players/Details.cshtml:796-804 | done | |
| Rage quit | Views/Players/Details.cshtml:805-813 | done | |
| Taxa de vitória (`P0`) | Views/Players/Details.cshtml:816-824 | done | |
| Derrotas | Views/Players/Details.cshtml:825-833 | done | |
| Punição ("0" ou "-n XP") | Views/Players/Details.cshtml:35-38, :834-836, :848 | done | |
| Administrador: botão "Editar" ao lado de cada punição (tooltip embaixo) | Views/Players/Details.cshtml:838-843, :850-855 | done | |
| Administrador: o botão pergunta "Quantos pontos de experiência o usuário deve perder?" com 150 sugerido | Views/Players/Details.cshtml:530-531, :840 | done | |
| Administrador: um número positivo adiciona a punição, 0 a remove, qualquer outra coisa não faz nada; a página recarrega em seguida | Views/Players/Details.cshtml:505-546 | adapted | Mesmo comportamento. As chamadas também enviam a temporada exibida e o token antiforgery. |
| Maior sequência de vitórias | Views/Players/Details.cshtml:860-868 | done | |
| Maior sequência de derrotas | Views/Players/Details.cshtml:869-877 | done | |
| Histórico de partidas: um bloco por resultado (vitória em verde, derrota em vermelho, empate em cinza) com tooltip; um traço quando vazio; com os nomes acima de cada linha em uma comparação | Views/Players/Details.cshtml:40-75, :880-887 | done | |
| Comparação: linha "Jogando junto" (jogos, vitórias com a taxa de vitória, derrotas com a taxa de derrotas), só quando jogaram juntos | Views/Players/Details.cshtml:889-909 | done | |
| Comparação: linha "Jogando contra" (jogos, vitórias e derrotas de cada jogador com as taxas, segundo jogador em amarelo), só quando se enfrentaram | Views/Players/Details.cshtml:911-937 | done | |

## Página do jogador: aba Geral, "Contas relacionadas"

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Painel (`panel-warning`) só quando outra conta se conectou pelo mesmo endereço | Views/Players/Details.cshtml:943-948 | adapted | Mesmo painel. Por padrão é exibido para todos os visitantes, como antes; a configuração Configurações > Privacidade > "Quem pode ver as contas relacionadas" pode restringi-lo aos administradores. Ele depende do módulo de servidor ao vivo, que é onde as conexões são registradas. |
| Colunas: Nome, ID da Steam, Último login, Conexões, Perfil | Views/Players/Details.cshtml:951-959 | done | |
| Avatar de 32 px e nome com link para a página daquela conta | Views/Players/Details.cshtml:964-967 | done | |
| ID da Steam com o botão de copiar | Views/Players/Details.cshtml:968-974 | done | Formato `STEAM_0:x:y`, como acima. |
| Último login (formato `g`) | Views/Players/Details.cshtml:975 | adapted | O hub guarda o momento em UTC e o mostra no fuso horário do site; o site antigo mostrava o horário local do servidor dele. |
| Número de conexões | Views/Players/Details.cshtml:976 | done | |
| URL do perfil como link (nova aba, `noopener noreferrer`) com o botão de copiar | Views/Players/Details.cshtml:977-985 | done | |

## Página do jogador: aba Geral, "Estatísticas como sobrevivente" e "Estatísticas como infectado"

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Painel "Estatísticas como sobrevivente" | Views/Players/Details.cshtml:993-996 | done | |
| MVP de SI (vezes), MVP de CI (vezes), LVP de FF (vezes) | Views/Players/Details.cshtml:999-1027 | done | |
| SI mortos, Commons mortos, Dano aos SI | Views/Players/Details.cshtml:1029-1057 | done | |
| Mortes, Vezes incapacitado, Dano recebido de SI (com o ícone de ajuda e o tooltip) | Views/Players/Details.cshtml:1059-1090 | done | |
| Dano ao Tank, Pedras recebidas, FF causado (sem incap.) | Views/Players/Details.cshtml:1092-1121 | done | |
| Skeets totais, Melee skeets, Levels totais | Views/Players/Details.cshtml:1123-1153 | done | |
| Crowns totais, linha só quando um dos jogadores tem crowns | Views/Players/Details.cshtml:1155-1172 | done | |
| Painel "Estatísticas como infectado" | Views/Players/Details.cshtml:1177-1180 | done | |
| Dano de SI (sem Tank) com o ícone de ajuda e o tooltip; Dano como Tank (em pé) | Views/Players/Details.cshtml:1183-1206 | done | |
| Dano de ácido (em pé); Dano total de pounce com o ícone de ajuda e o tooltip | Views/Players/Details.cshtml:1208-1231 | done | |

## Página do jogador: aba Geral, "Últimos jogos"

O cartão é o antigo `Views/LastMatches/_Match.cshtml`. A página do jogador usa o cartão de partidas compartilhado, `Views/Matches/_Match.cshtml`, o mesmo da lista de partidas; ela não tem mais uma cópia própria.

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título "Últimos jogos" e a lista, só quando há partidas | Views/Players/Details.cshtml:1236-1246 | done | |
| Cabeçalho do cartão: campanha, modo de jogo em itálico, rótulo "Competitiva" (verde) ou "Casual" (cinza) | Views/LastMatches/_Match.cshtml:157-171 | adapted | "Competitiva" é uma partida que conta para o ranking pelas regras de modo de jogo do hub; a regra antiga era fixa em dois ou mais mapas e equipes de quatro. |
| "A equipe X venceu por n pontos" | Views/LastMatches/_Match.cshtml:172-174 | done | |
| Ícone de calendário com "Hoje às", "Ontem às" ou data e hora, e o tooltip dele | Views/LastMatches/_Match.cshtml:176-193 | done | Exibido no fuso horário do site. |
| Ícone de relógio com a duração (`hh:mm:ss`) e o tooltip dele | Views/LastMatches/_Match.cshtml:195-201 | done | |
| Capa da campanha (abre ampliada) | Views/LastMatches/_Match.cshtml:208, Core/Campaign/CampaignThumb.cs | done | Mesmas imagens e mesmos nomes de arquivo; uma campanha sem capa usa `no-image.jpg`. |
| Botão "Detalhes" | Views/LastMatches/_Match.cshtml:209-214 | adapted | Leva à página da partida no hub, `/matches/{id}`. |
| Dois painéis de equipe (vencedora em verde, perdedora em vermelho) com "Equipe X" e a pontuação | Views/LastMatches/_Match.cshtml:217-232 | done | |
| Jogadores de cada equipe ordenados pelos prêmios de MVP, com avatar, nome com link e a linha destacada para o jogador logado | Views/LastMatches/_Match.cshtml:233-250 | done | Os links mantêm a temporada exibida. |
| Estrela amarela por MVP de SI e estrela azul por MVP de CI, com tooltips, depois de um separador | Views/LastMatches/_Match.cshtml:251-264 | done | |
| Gráfico "Placar" (linha): uma série por equipe com a pontuação acumulada por mapa, verde para a vencedora e vermelho para a perdedora, legenda embaixo | Views/LastMatches/_Match.cshtml:24-78, :278-289 | done | Mesma configuração do Chart.js, carregado sob demanda por `.app-chart` + `data-chart`; cor da fonte conforme o tema. |
| Gráfico "Pontos ganhos por mapa" (barras): uma série por equipe com os pontos de cada mapa | Views/LastMatches/_Match.cshtml:80-153, :291-302 | done | |
| Nomes dos mapas no eixo truncados em 16 caracteres | Views/LastMatches/_Match.cshtml:31, :85 | done | |
| Celular: capa e equipes empilhadas em largura total | Views/Shared/_Layout.cshtml (`.match .left-side`, `.match .right-side`) | done | Regras do layout portado. |

## Página do jogador: aba Campanhas

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| "Ainda não há estatísticas de campanhas para este jogador." quando vazio | Views/Players/Details.cshtml:1250-1253 | done | |
| Linha de ajuda com o ícone de informação | Views/Players/Details.cshtml:1256-1258 | done | |
| Grade de cartões de campanha, da mais jogada para a menos jogada (depois pelos jogos do primeiro jogador, depois pelo nome); campanhas sem nome ficam de fora | Views/Players/Details.cshtml:1259-1283, Models/PlayerDetailsModel.cs:104-120 | done | |
| Cartão: capa, nome da campanha, "n Jogos" à direita (só com um jogador) | Views/Players/Details.cshtml:1262-1271 | done | |
| Por jogador: barra em três partes (vitórias, empates, derrotas) com tooltips | Views/Players/Details.cshtml:77-80, :101-105 | done | |
| Por jogador: três caixas com vitórias, empates e derrotas, cada uma com o seu percentual | Views/Players/Details.cshtml:106-119 | done | |
| Por jogador: "Rage quit: n" quando houver algum | Views/Players/Details.cshtml:120-123 | done | |
| Comparação: o nome de cada jogador com "· n Jogos" acima da barra; "Ainda não jogou" para quem não tem jogos | Views/Players/Details.cshtml:85-98, :1273-1279 | done | |
| Jogos e taxas de vitória, derrota e empate de uma campanha | Sdk/Ranking/Results/PlayerCampaignResult.cs:9-18 | done | Adicionados ao `PlayerCampaign` do hub. |

## Página do jogador: aba Skins (módulo da loja de skins)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título do painel "Skins" com um badge por jogador (amarelo para o segundo) | Views/Players/Details.cshtml:1288-1295 | done | |
| Link "Ver loja de skins" | Views/Players/Details.cshtml:1296 | done | |
| Por jogador: nome com o número de skins (em uma comparação) | Views/Players/Details.cshtml:148-156 | done | |
| Por jogador: pílula da carteira "★ n estrela(s) disponível(is) · n ganhas, n gastas" | Views/Players/Details.cshtml:128-141, :157 | done | |
| "Este jogador ainda não tem skins." | Views/Players/Details.cshtml:160-163 | done | |
| Skins do jogador, das mais raras e mais caras para as demais: miniatura, título localizado, link para a skin na loja (`/skins#skin-{id}`) | Views/Players/Details.cshtml:166-181, Models/PlayerSkinsModel.cs:8-14 | adapted | As miniaturas vêm do endereço configurado para os assets de skins (eles não vêm junto com o hub); sem esse endereço, aparece uma caixa cinza do mesmo tamanho. |
| Moldura verde e rótulo "Equipada" nas skins equipadas | Views/Players/Details.cshtml:170-178 | done | |

## Página do jogador: aba Relações

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título "Relações com jogadores" com os nomes dos jogadores | Views/Players/Details.cshtml:1383-1394 | done | |
| Um cartão por jogador encontrado em partidas, o jogador comparado primeiro, depois por jogos e por vitórias | Views/Players/Details.cshtml:1400-1403, Models/PlayerDetailsModel.cs:55-69, :71-96 | done | |
| Cartão em amarelo (`panel-warning`) para o jogador comparado | Views/Players/Details.cshtml:1403 | done | |
| Título do cartão: nome (link que troca a página para aquele jogador, mantendo a comparação, quando ele está no ranking), "n Jogos" | Views/Players/Details.cshtml:1408-1416 | adapted | Um jogador que não está no ranking aparecia pelo community id; o hub mostra o último nome conhecido quando tem um. |
| Botão de ajuda (ponto de interrogação, tooltip "Não entendi, me explique melhor") | Views/Players/Details.cshtml:1405-1407 | done | |
| Avatar | Views/Players/Details.cshtml:1420-1422 | done | |
| Tabela: "Jogando junto" e "Jogando contra" com Jogos, Vitórias e Derrotas, contagens e taxas com tooltips, uma coluna por jogador em uma comparação (o segundo em amarelo) | Views/Players/Details.cshtml:1313-1335, :1424-1455 | done | |
| Modal de ajuda por cartão: "Entendendo os números", as três frases com os nomes coloridos e os números em negrito, separadas por uma linha para o segundo jogador, "Fechar" | Views/Players/Details.cshtml:1337-1373, :1460-1493 | done | |
| Só 12 cartões no início; o botão "Ver mais jogadores" / "Ocultar" alterna o restante | Views/Players/Details.cshtml:564-579, :1402, :1498-1507, Core/AppConsts.cs:7 | done | `AppConsts.PlayerRelationsVisible`. |

## Página de jogador fora do ranking (`Views/Players/PlayerNotFound.cshtml`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Título "Detalhes do jogador" | Views/Players/PlayerNotFound.cshtml:36 | done | |
| Caixa de aviso: "Jogador fora do ranking" e as duas linhas de explicação | Views/Players/PlayerNotFound.cshtml:38-42 | done | |
| Seletor de período (novo) | - | adapted | Acréscimo do hub: um jogador que está fora do ranking na temporada exibida pode estar no ranking de outra. |
| Avatar (184 px no máximo, abre ampliado, imagem de fallback) | Views/Players/PlayerNotFound.cshtml:45-47 | done | |
| Painel "Informações disponíveis" | Views/Players/PlayerNotFound.cshtml:49-52 | done | |
| Nome, quando conhecido | Views/Players/PlayerNotFound.cshtml:56-62 | adapted | Vem das informações de conexão, como antes, ou do nome que o hub registrou em uma partida passada. |
| Community ID com o botão de copiar | Views/Players/PlayerNotFound.cshtml:63-69 | done | |
| ID da Steam com o botão de copiar | Views/Players/PlayerNotFound.cshtml:70-79 | done | Formato `STEAM_0:x:y`, como na página do jogador. |
| Último login, quando conhecido | Views/Players/PlayerNotFound.cshtml:80-86 | adapted | Fuso horário do site, como acima. |
| Conexões, quando conhecidas | Views/Players/PlayerNotFound.cshtml:87-93 | done | |
| URL do perfil (nova aba) com o botão de copiar | Views/Players/PlayerNotFound.cshtml:94-105 | done | |
| Painel "Contas relacionadas" com as mesmas cinco colunas da página do jogador | Views/Players/PlayerNotFound.cshtml:112-160 | adapted | Com o módulo de servidor ao vivo, e seguindo a mesma configuração de privacidade da página do jogador. |
| Botão "Retornar à página de ranking" | Views/Players/PlayerNotFound.cshtml:162-164 | done | "Voltar para a página inicial" com o módulo de ranking desligado. |
| Botão "Abrir perfil na Steam" abaixo do avatar, ID da Steam na forma `STEAM_1`, Steam3 ID (novo) | - | adapted | Acréscimo do hub: todo link de jogador do site abre esta página em vez do perfil Steam, então a página oferece o caminho para a Steam. |
| Aviso "Ainda sem estatísticas" com o módulo de ranking desligado (novo) | - | adapted | Acréscimo do hub. |
| Estilos (tamanho do avatar, células de cabeçalho, margem no celular) | Views/Players/PlayerNotFound.cshtml:15-34 | done | |
| Metadados da página: título "{name or id} - Detalhes do jogador" | PageMetadata/PlayerNotFoundPageMetadata.cs:14 | done | |
| Metadados da página: descrição "Veja as informações disponíveis sobre o jogador {name} no ..." | PageMetadata/PlayerNotFoundPageMetadata.cs:15 | adapted | O texto citava "L4D2 Competitive"; agora recebe o nome do site. |
| Metadados da página: avatar como imagem, tipo Open Graph `profile`, sem indexação | PageMetadata/PlayerNotFoundPageMetadata.cs:16-19 | done | |

## Modelos e dados

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `PlayersModel` (jogadores, ordem, sentido) | Models/PlayersModel.cs | done | `Models/Players/PlayersModel.cs`, mais o período exibido. |
| Linha `Player` da lista (valor por estatística, jogos, média) | Core/Players/Player.cs | done | `Models/Players/PlayerListRow.cs` (o nome `Player` já é usado pelo pacote de estatísticas). |
| `PlayerDetailsModel` (rankings, jogadores, últimos jogos, confronto direto, relações, campanhas, skins, contas relacionadas) | Models/PlayerDetailsModel.cs | done | `Models/Players/PlayerDetailsModel.cs`. |
| `PlayerRelationModel`, `PlayerRelationRowModel` | Models/PlayerRelationModel.cs, Models/PlayerRelationRowModel.cs | done | |
| `PlayerCampaignRowModel` | Models/PlayerCampaignRowModel.cs | done | |
| `PlayerSkinsModel` | Models/PlayerSkinsModel.cs | done | |
| `PlayerNotFoundModel` | Models/PlayerNotFoundModel.cs | done | |
| `PlayerPatentModel` | Models/PlayerPatentModel.cs | done | Formato de `GET /api/players/patents`, que já existe no hub. |
| `PlayerResult` e as suas estatísticas, nomes, relações, campanhas, resultados e sequências | Sdk/Ranking/Results/*.cs | done | `RankingPlayer` da base. |
| `PunishmentCommand`, `PunishmentResult` | Sdk/Punishments/* | adapted | `Models/Players/PunishmentCommand.cs`, com o `seasonId` opcional. A resposta mantém `communityId` e `lostExperiencePoints`; `server` deu lugar a `seasonId`, porque no hub uma punição pertence a uma temporada, e não a um servidor de jogo. |
| `PlayerStarsResult` (estrelas de um jogador) | Sdk/Stars/* | done | A carteira exibida na aba Skins (`SkinWallet`). |
