# Paridade do site: skins

🌐 [English](skins.md) | **Português** | [Español](skins.es.md)

Porte da loja de skins e das páginas de administrador dela, do site antigo (`l4d2-playstats-web`) para o hub.

Os caminhos da coluna "Onde no site antigo" são relativos a `L4D2PlayStats.Web/L4D2PlayStats.Web`, a não ser que comecem com `Core/` (`L4D2PlayStats.Web/L4D2PlayStats.Core`).

Situação: `done` (mesmo conteúdo e visual), `adapted` (presente, alterado pelo motivo indicado), `not ported` (não pode existir no hub).

## Página da loja (`/skins`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Rota `GET /skins`, item de menu "Skins" ativo (`ViewBag.Skins`) | Controllers/SkinsController.cs:11-14 | done |  |
| Metadados da página: título "Skins", descrição, imagem e alt da skin mais cara | PageMetadata/SkinsPageMetadata.cs:9-18, Views/Skins/Index.cshtml:18 | adapted | A descrição recebe o nome do site das configurações em vez de citar "L4D2 Competitive". A imagem é a miniatura no site de assets, como endereço absoluto; sem um endereço de assets, não há imagem. |
| Estilos inline da página (`.sk-*`, 700 linhas) | Views/Skins/Index.cshtml:34-734 | done | Copiados sem alterações. |
| Token antiforgery na página | Views/Skins/Index.cshtml:736 | done |  |
| Hero: antetítulo "Loja de skins", título "Skins", texto de abertura | Views/Skins/Index.cshtml:738-742 | done |  |
| Hero: pílula da temporada com ícone de calendário, último dia da temporada e dias restantes | Views/Skins/Index.cshtml:744-747, Core/Skins/SkinSeason.cs:11-14 | done | As datas vêm da temporada do hub, no fuso horário do site, com qualquer duração. Oculta quando o administrador desliga o reembolso de temporada. |
| Hero: botão "Como funciona" que abre o painel recolhido (o caret gira) | Views/Skins/Index.cshtml:750-752, 80-86 | done |  |
| Hero: botão "Como uso minhas skins no jogo?" | Views/Skins/Index.cshtml:753-755 | done |  |
| Carteira, sem login: estrela, "Entre com a Steam para ver..." e o botão de login com retorno para a loja | Views/Skins/Index.cshtml:760-769, 31 | done |  |
| Carteira, estrelas indisponíveis: cartão de aviso | Views/Skins/Index.cshtml:770-778 | adapted | A marcação e o texto são mantidos, mas o hub lê as estrelas do próprio banco de dados, então a carteira de um jogador logado está sempre disponível e esse cartão nunca aparece. |
| Carteira, logado: estrela, saldo, rótulo "estrela(s) disponível(is)" (singular e plural) | Views/Skins/Index.cshtml:781-788 | done |  |
| Resumo da carteira: estrelas ganhas como MVP e estrelas gastas | Views/Skins/Index.cshtml:789 | done |  |
| Carteira: contagens de MVPs de dano em SI e de MVPs de comuns | Views/Skins/Index.cshtml:790, Core/Skins/Results/WalletResult.cs:5-6 | done | Back-end adicionado: o `SkinStarsService` divide as estrelas nos dois tipos de MVP. |
| Como funciona: caixas "1. Ganhe estrelas", "2. Compre e equipe", "3. Troque no jogo" | Views/Skins/Index.cshtml:795-809 | done | Com o reembolso de temporada desligado, a caixa 2 termina com "fica sua para sempre" em vez de "até o fim da temporada do ranking". |
| Como funciona: faixa "Skins por temporada" com o fim da temporada e os dias restantes | Views/Skins/Index.cshtml:811-817 | done | O texto diz a duração real da temporada ("a cada 2 meses" por padrão, "todo mês" ou "a cada N meses" nos outros casos). Oculta com o reembolso de temporada desligado. |
| Como funciona: linha de créditos | Views/Skins/Index.cshtml:819 | done |  |
| Abas: "Loja" com a contagem de skins | Views/Skins/Index.cshtml:1017-1022 | done |  |
| Abas: "Minhas skins" com a contagem de skins do jogador, só com login | Views/Skins/Index.cshtml:1023-1030 | done |  |
| Abas: "Autores" com a contagem de autores | Views/Skins/Index.cshtml:1031-1035 | done |  |
| Aba mantida no endereço (`#shop`, `#mine`, `#authors`) e restaurada no carregamento | Views/Skins/Index.cshtml:1765-1771 | done |  |
| Barra de ferramentas: filtro de arma com "Todas as armas" e os grupos "Armas de fogo" e "Armas brancas" | Views/Skins/Index.cshtml:1039-1054, Models/SkinsModel.cs:11-16 | done | As quinze armas do site antigo, mais qualquer outra arma que um catálogo importado trouxer. |
| Barra de ferramentas: filtro de raridade com "Todas as raridades" e as cinco raridades, a lendária primeiro | Views/Skins/Index.cshtml:1055-1062, Models/SkinsModel.cs:37 | done |  |
| Barra de ferramentas: checkbox "Só as que posso comprar", só com uma carteira | Views/Skins/Index.cshtml:1063-1068 | done |  |
| Filtros aplicados no navegador a todas as abas; seções vazias ocultas; "Nenhuma skin com esses filtros." | Views/Skins/Index.cshtml:1721-1763, 1093, 1118 | done |  |
| Aba Loja: uma seção por raridade (de lendária a comum), cabeçalho colorido | Views/Skins/Index.cshtml:1073-1092, 282-326 | done |  |
| Cabeçalho da raridade: contagem de skins e faixa de preço | Views/Skins/Index.cshtml:1083 | done |  |
| Ordem dentro de uma raridade: preço decrescente, ordem das armas, título em inglês | Models/SkinsModel.cs:70-80 | done |  |
| Cartão: `col-xs-6 col-sm-4 col-md-3`, atributos de dados `weapon`, `rarity`, `affordable` | Views/Skins/Index.cshtml:980-981 | done |  |
| Cartão: âncora `id="skin-{id}"` (só na aba Loja), borda de comprada, brilho de lendária | Views/Skins/Index.cshtml:982 | done |  |
| Cartão: miniatura, com carregamento tardio | Views/Skins/Index.cshtml:984-985 | adapted | Carregada do site de assets (`{assets}/skins/{id}.jpg`) em vez de `/imgs/skins`. Sem um endereço de assets, um placeholder transparente mantém o formato do cartão. |
| Cartão: rótulo da arma sobre a miniatura | Views/Skins/Index.cshtml:986 | done |  |
| Cartão: rótulo de status "Equipada" (com ícone de check) ou "Comprada" | Views/Skins/Index.cshtml:987-996 | done |  |
| Cartão: etiqueta de preço com a estrela | Views/Skins/Index.cshtml:997 | done |  |
| Cartão: título com tooltip, em uma única linha com reticências | Views/Skins/Index.cshtml:1000 | done |  |
| Cartão: "por {author}" com link para o perfil do autor na Steam | Views/Skins/Index.cshtml:1001-1006 | done | Um autor importado sem Steam ID aparece sem o link. |
| Cartão: nome da raridade na cor da raridade | Views/Skins/Index.cshtml:1007 | done |  |
| Cartão: fotos em cross-fade ao passar o mouse (fotos 1, 5 e 6, tamanho `-card`, 1,6 s) | Views/Skins/Index.cshtml:1408-1474 | done | Conferido com as fotos de produção por meio de um proxy local. |
| Detecção de suporte a AVIF; fotos pedidas como `.avif` quando há suporte, `.webp` caso contrário | Views/Skins/Index.cshtml:1408-1421 | done |  |
| Ação, sem login: link "Entrar com a Steam" | Views/Skins/Index.cshtml:829-833 | done |  |
| Ação, equipada: botão "Voltar para a padrão" | Views/Skins/Index.cshtml:835-841 | done |  |
| Ação, comprada: botão "Equipar" | Views/Skins/Index.cshtml:843-849 | done |  |
| Ação, sem carteira: "Comprar por {price}" desabilitado | Views/Skins/Index.cshtml:851-855 | done |  |
| Ação, estrelas insuficientes: "Comprar por {price}" desabilitado e "Faltam {n}" | Views/Skins/Index.cshtml:857-862 | done |  |
| Ação, pode comprar: botão "Comprar por {price}" com o ícone de carrinho | Views/Skins/Index.cshtml:864-866 | done |  |
| Aba Minhas skins: botão "Como uso minhas skins no jogo?" | Views/Skins/Index.cshtml:1099-1103 | done |  |
| Aba Minhas skins: texto de lista vazia "Você ainda não tem skins..." | Views/Skins/Index.cshtml:1104-1107 | done |  |
| Aba Minhas skins: skins do jogador ordenadas por arma, raridade, preço | Views/Skins/Index.cshtml:1110-1118, Models/SkinsModel.cs:45-52 | done | Uma skin que saiu do catálogo continua aqui para o dono, com o rótulo "Fora de venda" nos detalhes. |
| Aba Autores: texto de introdução | Views/Skins/Index.cshtml:1123 | done |  |
| Aba Autores: uma caixa por autor com link do perfil, contagem de skins (singular e plural) e os títulos | Views/Skins/Index.cshtml:1124-1139, 25-30 | done | Autores sem Steam ID são agrupados por nome e aparecem sem o link. |
| Modal de detalhes: abre com um clique em qualquer ponto do cartão, exceto nas ações e no link do autor | Views/Skins/Index.cshtml:1588-1598, 1622-1628 | done |  |
| Modal de detalhes: abre sozinho para `/skins#skin-{id}` | Views/Skins/Index.cshtml:1773-1778 | done |  |
| Detalhes: galeria com a foto principal e seis miniaturas quando a skin tem fotos | Views/Skins/Index.cshtml:884-903, 1476-1506 | done | Exibida quando a skin tem fotos e há um endereço de assets definido. |
| Detalhes: imagem única quando a skin não tem fotos, abre no lightbox | Views/Skins/Index.cshtml:904-911 | done |  |
| Detalhes: lightbox com zoom e pan (panzoom), clique para dar zoom, zoom pela roda do mouse | Views/Skins/Index.cshtml:1312, 1508-1542 | done |  |
| Detalhes: botões de anterior e próximo do lightbox e teclas de seta | Views/Skins/Index.cshtml:1544-1586, 606-636 | done |  |
| Detalhes: tags de raridade, arma, "Equipada" ou "Comprada" | Views/Skins/Index.cshtml:913-926 | done |  |
| Detalhes: preço | Views/Skins/Index.cshtml:927 | done |  |
| Detalhes: autor com link para o perfil na Steam | Views/Skins/Index.cshtml:928-936 | done |  |
| Detalhes: descrição localizada | Views/Skins/Index.cshtml:937-940 | done |  |
| Detalhes: título e descrição "Original", vindos do Workshop | Views/Skins/Index.cshtml:880, 941-946 | done |  |
| Detalhes: o mesmo botão de ação do cartão | Views/Skins/Index.cshtml:947-949 | done |  |
| Detalhes: "Comando no chat" com `!skin {id}` e o botão de copiar, só quando a skin é do jogador | Views/Skins/Index.cshtml:950-959 | done | Usa a partial compartilhada `_CopyButton`: mesmo ícone, mesmo popover e mesmo comportamento. |
| Detalhes: botão "Ver no Workshop" | Views/Skins/Index.cshtml:960-962 | done | Oculto para uma skin importada sem id do Workshop. |
| Detalhes: botão "Dar de presente (admin)" para administradores | Views/Skins/Index.cshtml:963-968, 1691-1699 | done | Não é oferecido para uma skin que está fora de venda. |
| Rodapé do modal de detalhes: link "É o autor?" e "Fechar" | Views/Skins/Index.cshtml:1143-1161 | done | Oculto só quando os dois endereços do aviso estão vazios nas configurações da loja. |
| Modal de aviso ao autor: quatro parágrafos | Views/Skins/Index.cshtml:1163-1180 | done |  |
| Modal de aviso ao autor: botão "Falar comigo na Steam" | Views/Skins/Index.cshtml:1182-1184 | adapted | O endereço era o perfil do dono na Steam, fixo na view. Agora é o "Perfil da Steam para contato" das configurações da loja; o botão fica oculto enquanto esse campo está vazio, que é o padrão. |
| Modal de aviso ao autor: botão "Abrir um pedido no GitHub", pré-preenchido com o título da skin, o id e o endereço do Workshop | Views/Skins/Index.cshtml:1185-1187, 1600-1608 | adapted | O endereço é o "Endereço para pedidos de remoção" das configurações da loja. Ele começa como a página de nova issue do `l4d2-zone-server-assets`, como no site antigo, e é pré-preenchido da mesma forma. |
| Modal de aviso ao autor empilhado acima do modal de detalhes | Views/Skins/Index.cshtml:1610-1620 | done |  |
| Modal de confirmação de compra: texto, saldo antes e depois, aviso de que a skin será equipada, aviso da temporada, aviso final | Views/Skins/Index.cshtml:1197-1212, 1630-1650 | done | Com o reembolso de temporada desligado, o aviso da temporada fica de fora e a última linha diz "A compra não pode ser desfeita." |
| Compra: `POST /api/shop/skins/{id}/buy`, modal de sucesso, a página recarrega ao fechar | Views/Skins/Index.cshtml:1652-1667, Controllers/Api/SkinShopController.cs:31-41 | done |  |
| Equipar: `POST /api/shop/skins/{id}/equip`, modal de sucesso, recarregamento | Views/Skins/Index.cshtml:1669-1683, Controllers/Api/SkinShopController.cs:43-53 | done |  |
| Voltar para a padrão: `POST /api/shop/weapons/{weapon}/unequip`, modal de sucesso, recarregamento | Views/Skins/Index.cshtml:1701-1714, Controllers/Api/SkinShopController.cs:55-65 | done |  |
| Modal de resultado e os textos de erro por status (`InsufficientStars`, `AlreadyOwned`, `StarsUnavailable`, `Busy`, `NotOwned`, `SkinNotFound`, genérico) | Views/Skins/Index.cshtml:1214-1226, 1333-1341, 1367-1406 | done | O hub devolve os mesmos nomes de status. |
| Modal "Como usar suas skins no jogo": introdução, tabela de seis comandos, nomes das armas, cinco regras | Views/Skins/Index.cshtml:1228-1304, 1685-1689 | done |  |
| Modal de presente disponível para administradores na página da loja | Views/Skins/Index.cshtml:1306-1309 | done |  |
| Celular: padding do hero, tamanho do título e largura da carteira abaixo de 700 px; galeria com três colunas abaixo de 480 px | Views/Skins/Index.cshtml:600-604, 719-732 | done |  |

## Modal de presente (`_GiftModal`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Modal "Dar skin de presente" com o texto de abertura | Views/Shared/_GiftModal.cshtml:61-72 | done |  |
| Jogador: caixa do jogador escolhido (avatar, nome, Steam ID, SteamID64) ou "Nenhum jogador escolhido" | Views/Shared/_GiftModal.cshtml:74-77, 138-148 | done | O avatar vem de `/avatar/{id}`. |
| Jogador: campo de texto para um Steam ID em qualquer formato ou um link de perfil; "Steam ID digitado manualmente" | Views/Shared/_GiftModal.cshtml:78-85, 183-187 | done |  |
| Jogador: "Escolher da lista" abre o seletor de jogadores e volta | Views/Shared/_GiftModal.cshtml:166-181 | done |  |
| Skin: caixa da skin escolhida (miniatura, título, arma, raridade, preço) ou "Nenhuma skin escolhida" | Views/Shared/_GiftModal.cshtml:87-90, 150-164 | done |  |
| Skin: "Escolher skin" abre o seletor de skins e volta | Views/Shared/_GiftModal.cshtml:91-93, 189-198 | done |  |
| Confirmar: mensagens de validação quando falta o jogador ou a skin | Views/Shared/_GiftModal.cshtml:200-213 | done |  |
| Confirmar: `POST /api/shop/gift`, mensagem de sucesso e recarregamento, erro por status, mensagem de Steam ID inválido no 400 | Views/Shared/_GiftModal.cshtml:215-237, Controllers/Api/SkinShopController.cs:85-96, 130-141 | done | Back-end adicionado: o `SkinRecipient` lê `STEAM_x:y:z`, `[U:1:n]`, SteamID64 e links de perfil. |
| `GiftModal.open({ skin })` pré-seleciona uma skin | Views/Shared/_GiftModal.cshtml:239-251 | done |  |
| Presente: gratuito, equipado na hora, guarda quem deu | Core/Skins/Services/SkinShopService.cs:186-228 | done |  |

## Modal de seleção de skin (`_SkinPickerModal`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Modal grande "Escolher skin" | Views/Shared/_SkinPickerModal.cshtml:117-126 | done |  |
| Campo de busca (nome, id ou arma), com foco quando o modal abre | Views/Shared/_SkinPickerModal.cshtml:128, 190-192, 222-224 | done |  |
| Filtro de arma e filtro de raridade | Views/Shared/_SkinPickerModal.cshtml:129-141 | done |  |
| Grade de blocos ordenados por raridade, preço e arma: miniatura, título, arma, raridade, preço | Views/Shared/_SkinPickerModal.cshtml:11-17, 143-162 | done | Miniaturas do site de assets. |
| "Nenhuma skin com esses filtros." | Views/Shared/_SkinPickerModal.cshtml:163 | done |  |
| `SkinPicker.open(callback)` e `SkinPicker.find(id)` | Views/Shared/_SkinPickerModal.cshtml:237-260 | done |  |
| Celular: dois blocos por linha abaixo de 700 px | Views/Shared/_SkinPickerModal.cshtml:111-115 | done |  |

## Preços das skins (`/admin/skins/prices`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Rota `GET /admin/skins/prices`, só administradores, menu "Admin" e "Preços das skins" ativos | Controllers/SkinsAdminController.cs:14-15, 71-75 | done |  |
| Título "Preços das skins" e texto de abertura | Views/SkinsAdmin/Prices.cshtml:11, 24-28 | done |  |
| Estatística: skins com o preço alterado | Views/SkinsAdmin/Prices.cshtml:30-35 | done |  |
| Filtros: busca por nome ou id, raridade, arma (envia ao mudar), botão "Buscar", "Limpar filtros" | Views/SkinsAdmin/Prices.cshtml:37-59, Controllers/SkinsAdminController.cs:81-86 | done |  |
| Lista ordenada por raridade, preço e arma; 20 por página | Controllers/SkinsAdminController.cs:22, 87-92 | done | O id da skin desempata, para que as páginas sejam estáveis. |
| Linha: miniatura, título, "arma · raridade · id" | Views/SkinsAdmin/Prices.cshtml:66-78 | done |  |
| Linha: preço atual; rótulo "Alterado" e o preço padrão quando ele foi alterado | Views/SkinsAdmin/Prices.cshtml:79-87 | done | Uma skin que saiu do catálogo também ganha o rótulo "Fora de venda". |
| "Nenhuma skin encontrada." | Views/SkinsAdmin/Prices.cshtml:62-65 | done |  |
| Paginação "Mostrando a–b de n" que mantém os filtros | Views/SkinsAdmin/Prices.cshtml:13, 92 | done |  |
| Modal de preço: imagem, arma, raridade, preço padrão, campo de preço (de 1 a 100.000), dica | Views/SkinsAdmin/Prices.cshtml:94-134, 181-198 | done |  |
| Modal de preço: "Salvar" (`POST /api/shop/skins/{id}/price`), mensagem e recarregamento | Views/SkinsAdmin/Prices.cshtml:157-179, 200-209, Controllers/Api/SkinShopController.cs:67-83 | done |  |
| Modal de preço: "Restaurar padrão", só para um preço alterado | Views/SkinsAdmin/Prices.cshtml:192, 219-221 | done |  |
| Modal de preço: "Dar de presente" abre o modal de presente com a skin | Views/SkinsAdmin/Prices.cshtml:211-217 | done | Oculto para uma skin que está fora de venda. |
| Modal de preço: mensagem de preço inválido, campo de preço com foco e selecionado | Views/SkinsAdmin/Prices.cshtml:203-206, 223-225 | done |  |
| Regras de preço: um preço igual ao padrão remove a alteração; guarda quem alterou e quando | Core/Skins/Services/SkinService.cs:41-64 | done |  |

## Compras de skins (`/admin/skins/purchases`)

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Rota `GET /admin/skins/purchases`, só administradores, menu "Admin" e "Compras de skins" ativos | Controllers/SkinsAdminController.cs:24-28 | adapted | O endereço do hub é `/admin/skins`; `/admin/skins/purchases` redireciona para ele de forma permanente e mantém a query string. |
| Título "Compras de skins" | Views/SkinsAdmin/Purchases.cshtml:14, 269-270 | done |  |
| Cabeçalho: botão "Estornar tudo", só quando há algo a reembolsar | Views/SkinsAdmin/Purchases.cshtml:271-276 | done |  |
| Cabeçalho: o botão "Dar skin de presente" abre o modal de presente | Views/SkinsAdmin/Purchases.cshtml:277-279 | done |  |
| Estatísticas: compras, estrelas gastas, presentes dados | Views/SkinsAdmin/Purchases.cshtml:282-295 | done |  |
| Aviso: as compras são reembolsadas no fim da temporada, com a data | Views/SkinsAdmin/Purchases.cshtml:297-300 | done | Oculto com o reembolso de temporada desligado. |
| Filtros: busca por jogador, Steam ID ou skin; botão "Buscar" | Views/SkinsAdmin/Purchases.cshtml:302-307, Models/SkinPurchaseRowModel.cs:20-31 | done | O nome é o que foi salvo com a compra, depois o nome do perfil do jogador, depois o SteamID64. |
| Filtros: "Filtrar por jogador" abre o seletor de jogadores | Views/SkinsAdmin/Purchases.cshtml:308-313, 601-608 | done |  |
| Filtros: chip do jogador escolhido com avatar, nome, trocar e remover | Views/SkinsAdmin/Purchases.cshtml:314-326, 22-56 | done |  |
| Filtros: "Limpar filtros" | Views/SkinsAdmin/Purchases.cshtml:327-330 | done |  |
| Lista ordenada por data, da mais nova para a mais antiga; 20 por página | Controllers/SkinsAdminController.cs:22, 43-54 | done |  |
| Linha: miniatura da skin, título, "arma · id" | Views/SkinsAdmin/Purchases.cshtml:356-360 | done |  |
| Linha: avatar do jogador, nome e SteamID64 | Views/SkinsAdmin/Purchases.cshtml:361-367 | done |  |
| Linha: preço, ou o rótulo "Presente" e "por {administrator}"; data | Views/SkinsAdmin/Purchases.cshtml:368-382 | adapted | A data é exibida no fuso horário das configurações do site; o site antigo usava a hora local do servidor web. |
| "Nenhuma compra encontrada." | Views/SkinsAdmin/Purchases.cshtml:334-337 | done |  |
| Paginação que mantém a busca e o jogador | Views/SkinsAdmin/Purchases.cshtml:16, 387 | done |  |
| Modal de detalhes: hero na cor da raridade, capa com link para a skin na loja, raridade, badge de presente, título, "arma · id", "Ver na loja" | Views/SkinsAdmin/Purchases.cshtml:389-419, 558-576, 596-599 | done |  |
| Modal de detalhes: caixa do jogador com avatar, nome, id e os botões "só as compras deste jogador" e "perfil do jogador" | Views/SkinsAdmin/Purchases.cshtml:420-433, 577-581 | done | O botão de perfil abre `/player/{id}`, portado pela área de jogadores. |
| Modal de detalhes: dados de preço, saldo antes e depois (compras), presente dado por (presentes), data | Views/SkinsAdmin/Purchases.cshtml:434-453, 582-588 | done |  |
| Modal de detalhes: "Estornar compra", texto de confirmação, "Confirmar estorno" (`POST /api/shop/refund`), mensagem e recarregamento | Views/SkinsAdmin/Purchases.cshtml:455-466, 610-637, Controllers/Api/SkinShopController.cs:98-109 | done |  |
| Modal de reembolsar tudo: introdução com os totais, aviso, checkbox que habilita o botão, rótulo de execução em andamento | Views/SkinsAdmin/Purchases.cshtml:471-498, 643-660 | done |  |
| Reembolsar tudo: `POST /api/shop/refund-all`, resumo, mensagem de itens com falha, recarregamento | Views/SkinsAdmin/Purchases.cshtml:662-686, Controllers/Api/SkinShopController.cs:111-119 | adapted | O hub reembolsa tudo em uma única transação de banco de dados, então `failed` é sempre 0 e a mensagem "itens não puderam ser estornados", mantida na página, nunca aparece. |
| Modal de presente na página | Views/SkinsAdmin/Purchases.cshtml:500 | done |  |
| Celular: as linhas quebram e o jogador vai para uma segunda linha abaixo de 700 px; o hero empilha abaixo de 480 px | Views/Shared/_AdminStyles.cshtml, Views/SkinsAdmin/Purchases.cshtml:254-263 | done |  |

## APIs

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| `GET /api/shop/wallet` para o jogador logado: `balance`, `stars`, `spent` (usado pelas estrelas na navbar) | Controllers/Api/SkinShopController.cs:18-29 | done | Novo no hub. Nunca responde 503, porque as estrelas são locais. |
| `POST /api/shop/skins/{skinId}/buy`: `status`, `succeeded`, `price`, `balance` | Controllers/Api/SkinShopController.cs:31-41, 121-129 | done |  |
| `POST /api/shop/skins/{skinId}/equip` | Controllers/Api/SkinShopController.cs:43-53 | done |  |
| `POST /api/shop/weapons/{weapon}/unequip` | Controllers/Api/SkinShopController.cs:55-65 | done |  |
| `POST /api/shop/skins/{skinId}/price` (administrador): 400 fora da faixa, 404 para skin desconhecida, `price`, `defaultPrice`, `custom` | Controllers/Api/SkinShopController.cs:67-83 | done |  |
| `POST /api/shop/gift` (administrador): Steam ID em qualquer formato | Controllers/Api/SkinShopController.cs:85-96, 130-141 | done |  |
| `POST /api/shop/refund` (administrador) | Controllers/Api/SkinShopController.cs:98-109 | done |  |
| `POST /api/shop/refund-all` (administrador): `purchases`, `gifts`, `stars`, `players`, `failed` | Controllers/Api/SkinShopController.cs:111-119, Core/Skins/Results/RefundAllResult.cs | adapted | `failed` é sempre 0, veja a página de compras. |
| Token antiforgery obrigatório no cabeçalho `RequestVerificationToken` em todo POST | Controllers/Api/SkinShopController.cs:32, 44, 56, 69, 87, 100, 113 | done |  |
| `GET /api/skins`: as skins que alguém tem (`id`, `weapon`), para o servidor de jogo | Controllers/Api/SkinsController.cs:10-22 | done | Já existe no hub, sem alterações. |
| `GET /api/players/skins`: skins e equipamento de todos os jogadores, para o servidor de jogo | Controllers/Api/PlayersController.cs:33-46 | done | Já existe no hub, sem alterações. |
| `GET /api/players/{communityId}/wallet`: `mvpSiDamage`, `mvpCommon`, `stars`, `spent`, `balance` | Controllers/Api/PlayersController.cs:48-56 | done | `mvpSiDamage` e `mvpCommon` adicionados à resposta. |
| `POST /api/players/{communityId}/skins/equip` vindo do servidor de jogo: equipar ou voltar para a padrão | Controllers/Api/PlayersController.cs:58-69 | adapted | Mesmo caminho e mesmo corpo. O servidor de jogo prova quem é com o token dele no hub (cabeçalho `Authorization`) em vez do antigo segredo compartilhado. |

## Regras da loja e trabalho em segundo plano

| Item | Onde no site antigo | Situação | Observações |
|---|---|---|---|
| Estrelas: uma para cada MVP de dano em SI e cada MVP de comuns de todo o histórico | Core/Skins/Services/SkinShopService.cs:29-38, 373-382 | done |  |
| Compra: verifica a skin, a posse e o saldo; salva o preço, o saldo anterior e o nome do jogador; equipa na hora | Core/Skins/Services/SkinShopService.cs:69-138 | done | Uma transação de banco de dados sob a trava de escrita substitui a transação do table storage com novas tentativas. |
| Equipar exige a posse; uma skin por arma | Core/Skins/Services/SkinShopService.cs:140-157 | done |  |
| Voltar para a padrão remove o equipamento da arma | Core/Skins/Services/SkinShopService.cs:159-184 | done |  |
| Reembolso: devolve as estrelas, remove a skin e o equipamento dela | Core/Skins/Services/SkinShopService.cs:230-311 | done |  |
| Reembolso de temporada: a cada dez minutos, as compras feitas antes da temporada atual são reembolsadas; os presentes são mantidos | BackgroundServices/SkinSeasonRefundBackgroundService.cs:8-35, Core/Skins/Services/SkinShopService.cs:246-260, 313-316 | adapted | Segue as temporadas do hub, de qualquer duração, e é uma configuração da loja que começa ligada. |
| Catálogo: id, arma, raridade, preço, id do Workshop, fotos, autor, títulos e descrições em três idiomas, título e descrição originais | Core/Skins/Skin.cs, Core/Skins/SkinAuthor.cs, Core/Skins/Data/skins.json | adapted | Diferença proposital do hub: o catálogo não vem junto, o administrador importa o `skins.json` na página do catálogo. Todos os campos são mantidos. |
| Miniaturas em `/imgs/skins/{id}.jpg`, fotos em `{SkinShotsUrl}/{id}/` | Core/Skins/Skin.cs:33, Views/Skins/Index.cshtml:871-874 | adapted | Diferença proposital do hub: `{assets}/skins/{id}.jpg` e `{assets}/skin-shots/{id}/`, com o endereço de assets das configurações da loja. |

## Páginas só do hub

| Item | Onde no hub | Situação | Observações |
|---|---|---|---|
| Página do catálogo de skins (`/admin/skins/catalog`): totais, última alteração, importação a partir de um arquivo, importação a partir do endereço configurado | Views/SkinsAdmin/Catalog.cshtml | done | Reestilizada com `_AdminStyles` (cabeçalho, estatísticas, painéis). A última alteração é exibida no fuso horário do site. |
| Página de configurações da loja (`/admin/skins/settings`): reembolso no fim de uma temporada, endereço dos assets, endereço do catálogo | Views/SkinsAdmin/Settings.cshtml | done | Reestilizada com painéis do Bootstrap 3. Novo: os dois endereços do aviso para autores de skins. |
| Abas que ligam as quatro páginas de administrador | Views/Shared/_SkinsAdminNav.cshtml | adapted | As abas saíram: as quatro páginas estão no grupo Loja de skins do menu lateral da administração (`Views/Shared/_AdminMenu.cshtml`), que toda página de administrador mostra, então o caminho para o resto da administração não se perde nessas páginas. O conteúdo das páginas não mudou. |

## Textos

Os nomes das raridades em português e em espanhol voltaram às formas femininas do site antigo ("Lendária", "Épica", "Rara"; "Legendaria", "Épica", "Rara"), por meio dos arquivos de fragmento `pt-BR.skins.json` e `es-ES.skins.json`.

## Como foi conferido

- A página da loja, sem login e em português, foi lida em um navegador no hub e em https://l4d2.com.br/skins, e os dois documentos foram comparados nó a nó (tags, classes, atributos e textos do corpo da página e de todos os modais, cerca de 13.700 linhas cada). As únicas diferenças que restam são o token antiforgery, os links do rodapé e o nome do site, que vêm das configurações.
- Capturas de tela com 1280 e 400 pixels de largura, sem login, mostram a mesma página nos dois sites (só os itens de menu de outras áreas diferem).
- Logado como administrador em uma cópia dos dados de demonstração: reembolso de uma compra, compra com a confirmação, voltar para a padrão, equipar, presente com o seletor de jogadores e o seletor de skins, alteração e restauração de preço, filtros, abas mantidas no endereço, links `#skin-{id}`, fotos ao passar o mouse, galeria e lightbox com as fotos de produção, tema claro, português e espanhol.
- As páginas de preços e de compras exigem um administrador, então não puderam ser abertas no site em produção; foram conferidas com as views antigas linha por linha.
