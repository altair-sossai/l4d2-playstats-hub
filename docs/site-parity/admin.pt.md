# Paridade do site: administração, configuração inicial e ranking da comunidade

🌐 [English](admin.md) | **Português** | [Español](admin.es.md)

As páginas desta área só existem no hub: o site antigo não tem assistente de configuração, nem área administrativa para servidores, módulos, configurações, modos de jogo, campanhas ou administradores, nem ranking da comunidade. Não há nada a portar do site antigo, então este checklist é um inventário do que cada página tinha na primeira versão (Bootstrap 5, commit `f59518f`) e do que aconteceu com cada item quando a página foi reescrita para o visual do site antigo (Bootstrap 3, `Views/Shared/_Layout.cshtml`, `_AdminStyles`).

`Onde hoje` é o arquivo e a linha do item na versão em Bootstrap 5 (caminhos relativos a `src/L4D2PlayStats.Hub.Web`), ou `novo` para algo que este trabalho adiciona.

Situação: `done` = presente com o mesmo conteúdo e comportamento, no visual do site antigo; `adapted` = presente, mas alterado, pelo motivo indicado; `not ported` = removido, pelo motivo indicado.

Totais: 185 done, 9 adapted, 1 not ported.

Como foi conferido: todas as páginas foram abertas em um navegador sobre uma cópia de dados reais (6.636 arquivos de estatísticas) e sobre uma pasta de dados vazia (o assistente inteiro, do código de configuração até "Concluir"), no tema escuro e no claro, com 1280 e com 400 pixels de largura, em inglês e em português; nenhuma página tem rolagem lateral em 400 pixels. Cada formulário foi enviado ao menos uma vez. `AdminPagesTests`, `SiteSettingsTests` e `FooterLinksTests` cobrem o layout, o menu, as novas configurações e a logo.

## Assistente de configuração (`/setup`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Rota `GET /setup`; redireciona para `/` quando a configuração inicial termina | `Controllers/SetupController.cs:34` | `done` |  |
| Título da página "Configuração inicial" | `Views/Setup/Index.cshtml:3` | `done` | Definido por meio de `PageMetadata`, com `noindex`. |
| Cabeçalho "Bem-vindo" | `Views/Setup/Index.cshtml:19` | `done` |  |
| Texto de abertura "Alguns passos e o site estará pronto." | `Views/Setup/Index.cshtml:20` | `done` |  |
| Barra de passos: Administrador, Site, Módulos, Servidor de jogo, Concluir, numerados, com o passo atual e os passos concluídos marcados | `Views/Setup/Index.cshtml:22` | `adapted` | Pílulas justificadas do Bootstrap 3; os passos concluídos ganham uma marca de check. No celular só o passo atual aparece, então a barra ocupa uma linha em vez de cinco. |
| Passo 1: cabeçalho "Quem é o administrador?" e a explicação do código de configuração | `Views/Setup/Index.cshtml:33` | `done` |  |
| Passo 1: mensagem de erro quando o código de configuração não é válido | `Views/Setup/Index.cshtml:37` | `done` |  |
| Passo 1: formulário `POST /setup/start` com o token antiforgery | `Views/Setup/Index.cshtml:39` | `done` |  |
| Passo 1: campo "Código de configuração" (obrigatório, sem autocomplete, mantém o valor digitado, preenchido a partir de `?token=`) | `Views/Setup/Index.cshtml:42` | `done` |  |
| Passo 1: campo "Apenas desenvolvimento: entrar como este SteamID64" (só com o login de desenvolvimento ligado) | `Views/Setup/Index.cshtml:47` | `done` |  |
| Passo 1: botão "Continuar com o Steam" com o ícone da Steam | `Views/Setup/Index.cshtml:51` | `done` |  |
| Passo 1: o código é guardado em um cookie, o visitante entra com a Steam e `GET /setup/claim` torna essa conta o primeiro administrador | `Controllers/SetupController.cs:67` | `done` |  |
| Passo de login: cabeçalho "A configuração inicial está em andamento", explicação e botão "Entrar com a Steam" (`/login?returnUrl=/setup`) | `Views/Setup/Index.cshtml:60` | `done` |  |
| Passo 2: formulário `POST /setup/site` com o token antiforgery, cabeçalho "Sobre o site" | `Views/Setup/Index.cshtml:68` | `done` |  |
| Passo 2: campo "Nome do site" (obrigatório) | `Views/Setup/Index.cshtml:73` | `done` |  |
| Passo 2: campo "Idioma padrão" (English, Português, Español) | `Views/Setup/Index.cshtml:78` | `done` |  |
| Passo 2: campo "Fuso horário" com a lista de fusos horários e o texto de ajuda "Define quando uma temporada começa e termina." | `Views/Setup/Index.cshtml:87` | `done` |  |
| Passo 2: campo "Duração da temporada, em meses" (de 1 a 24) com o texto de ajuda dele | `Views/Setup/Index.cshtml:99` | `done` |  |
| Passo 2: campo "Chave da Steam Web API (opcional)" com o texto de ajuda dele e o link para steamcommunity.com/dev/apikey | `Views/Setup/Index.cshtml:104` | `done` |  |
| Passo 2: envio da logo do site | novo | `done` | Adicionado. Mesmas regras da página Configurações; um arquivo recusado mantém o assistente neste passo, com o motivo. |
| Passo 3: formulário `POST /setup/modules`, cabeçalho "O que este site deve ter?", nota "Você pode alterar isso depois na área de administração.", os switches dos módulos e "Continuar" | `Views/Setup/Index.cshtml:113` | `done` |  |
| Passo 4: cabeçalho "Conectar um servidor de jogo" | `Views/Setup/Index.cshtml:126` | `done` |  |
| Passo 4, ainda sem servidor: explicação, formulário `POST /setup/server` com "Nome do servidor" (obrigatório, 100) e "Endereço em que os jogadores se conectam (opcional)" (placeholder, 200), botão "Criar" | `Views/Setup/Index.cshtml:130` | `done` |  |
| Passo 4: link "Pular por enquanto" | `Views/Setup/Index.cshtml:140` | `done` |  |
| Passo 4, servidor criado: nome do servidor, instruções de conexão com o token exibido uma única vez, caixa de status que se atualiza sozinha, "Continuar" | `Views/Setup/Index.cshtml:145` | `done` | As duas linhas de configuração também ganham o botão de copiar do site antigo. O htmx só é carregado neste passo. |
| Passo 5: formulário `POST /setup/finish`, cabeçalho "Tudo pronto", explicação, botão "Concluir" (vai para `/admin`) | `Views/Setup/Index.cshtml:157` | `done` |  |
| A logo e a troca de tema respondem enquanto a configuração inicial está pendente | novo | `done` | Adicionado: `/site/logo` e `/theme/...` não são mais mandados para `/setup`, então a logo enviada aparece no menu do assistente e dá para trocar o tema ali. |

## Partials compartilhadas

### `_AdminNav`

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Links: Visão geral, Servidores de jogo, Módulos, Configurações, Modos de jogo, Campanhas, Administradores | `Views/Shared/_AdminNav.cshtml:8` | `done` | Pílulas do Bootstrap 3. |
| Link "Skins", só com a loja de skins ligada | `Views/Shared/_AdminNav.cshtml:18` | `done` |  |
| Link "Ranking da comunidade", só com o ranking da comunidade ligado | `Views/Shared/_AdminNav.cshtml:21` | `done` |  |
| Link "Bans de chat", só com o chat do site ligado | `Views/Shared/_AdminNav.cshtml:24` | `done` |  |
| Link "Comandos remotos", só com os comandos remotos ligados | `Views/Shared/_AdminNav.cshtml:27` | `done` |  |
| Link "Conexões de jogadores", só com o servidor ao vivo ligado | `Views/Shared/_AdminNav.cshtml:30` | `done` |  |
| Link "Anti-Cheat" (`/admin/anti-cheat`), só com o módulo Anti-Cheat ligado | novo | `done` | Adicionado. |
| O link da página atual fica destacado | `Views/Shared/_AdminNav.cshtml:35` | `done` |  |

### `_ServerStatus`

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Atualiza sozinho a cada 3 segundos (`GET /admin/servers/{id}/status`) | `Views/Shared/_ServerStatus.cshtml:30` | `done` | Continua sendo htmx, agora carregado pelas páginas que mostram o status (página do servidor, assistente) em vez de pelo layout. Conferido em um navegador: o bloco é substituído sem recarregar a página. |
| Ponto colorido e título do estado: Conectado, Offline, Chave inválida, Aguardando a primeira conexão | `Views/Shared/_ServerStatus.cshtml:32` | `done` |  |
| "Último contato" com a data e a hora | `Views/Shared/_ServerStatus.cshtml:36` | `done` |  |
| Alerta quando o identificador chegou com a chave errada | `Views/Shared/_ServerStatus.cshtml:41` | `done` |  |
| Verificação "Conexão com este site" | `Views/Shared/_ServerStatus.cshtml:44` | `done` |  |
| Verificação "Plugin de envio de estatísticas carregado" | `Views/Shared/_ServerStatus.cshtml:45` | `done` |  |
| Verificação "Gravação de estatísticas ativada (sm_stats_writestats 1)" | `Views/Shared/_ServerStatus.cshtml:46` | `done` |  |
| Verificação "Primeiro arquivo de estatísticas recebido" com o número de arquivos | `Views/Shared/_ServerStatus.cshtml:47` | `done` |  |
| Verificação "Dados da partida ao vivo chegando", só com o servidor ao vivo ligado | `Views/Shared/_ServerStatus.cshtml:50` | `done` |  |
| Verificação "Plugin de skins de armas carregado", só com a loja de skins ligada | `Views/Shared/_ServerStatus.cshtml:54` | `done` |  |
| Cada verificação mostra aprovada, reprovada ou ainda desconhecida, com a cor correspondente | `Views/Shared/_ServerStatus.cshtml:15` | `done` | Glyphicons (`ok`, `remove`, `option-horizontal`) com `text-success`, `text-danger`, `text-muted` no lugar dos símbolos de texto. |
| Linha com o que o servidor informou: host name, mapa, configuração, versão do SourceMod | `Views/Shared/_ServerStatus.cshtml:60` | `done` |  |

### `_ServerInstructions`

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Passo "Baixe os plugins do PlayStats" com os links "Baixar" e "Guia passo a passo" e a nota sobre a pasta de plugins | `Views/Shared/_ServerInstructions.cshtml:8` | `done` |  |
| Passo com o que o servidor precisa (SourceMod, REST in Pawn, l4d2_playstats) | `Views/Shared/_ServerInstructions.cshtml:15` | `done` |  |
| Passo com as duas linhas para o `cfg/server.cfg` (`playstats_url`, `playstats_token`) | `Views/Shared/_ServerInstructions.cshtml:19` | `done` | Com o botão de copiar do site antigo (`_CopyButton`). |
| Aviso "Copie o token agora..." quando a chave acabou de ser gerada | `Views/Shared/_ServerInstructions.cshtml:23` | `done` |  |
| Nota "A chave não é armazenada de forma legível..." nos outros casos, com `YOUR-KEY` no lugar da chave | `Views/Shared/_ServerInstructions.cshtml:27` | `done` |  |
| Passo "Reinicie o servidor de jogo ou troque o mapa..." com a dica do `sm_playstats_test` | `Views/Shared/_ServerInstructions.cshtml:30` | `done` |  |

### `_ModuleSwitches`

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Switch "Ranking e estatísticas" com a descrição dele | `Views/Shared/_ModuleSwitches.cshtml:5` | `done` | O Bootstrap 3 não tem switch: os sete são checkboxes com o título em negrito e a descrição abaixo. |
| Switch "Servidor ao vivo" com a descrição dele | `Views/Shared/_ModuleSwitches.cshtml:6` | `done` |  |
| Switch "Chat do site" com a descrição dele | `Views/Shared/_ModuleSwitches.cshtml:7` | `done` |  |
| Switch "Comandos remotos" com a descrição dele | `Views/Shared/_ModuleSwitches.cshtml:8` | `done` |  |
| Switch "Loja de skins de armas" com a descrição dele | `Views/Shared/_ModuleSwitches.cshtml:9` | `done` |  |
| Switch "Ranking da comunidade" com a descrição dele | `Views/Shared/_ModuleSwitches.cshtml:10` | `done` |  |
| Switch "Anti-cheat", desabilitado ("Reservado. Ainda não disponível.") | `Views/Shared/_ModuleSwitches.cshtml:11` | `adapted` | Não está mais reservado: o switch fica habilitado como os outros e a descrição dele é "Mostra a página do Anti-Cheat, com o download e as instruções. O Anti-Cheat em si é um programa à parte." (a página em si é portada na branch base). |
| Valor `false` oculto depois de cada switch, para que um switch desmarcado seja salvo como desligado | `Views/Shared/_ModuleSwitches.cshtml:18` | `done` |  |

## Visão geral (`/admin`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Toda página de `/admin` é só para administradores (401 para anônimos, 403 para outros jogadores) | `Controllers/Admin/AdminController.cs:23` | `done` |  |
| Título da página e cabeçalho "Administração" | `Views/Admin/Index.cshtml:3` | `done` |  |
| Menu de administração | `Views/Admin/Index.cshtml:6` | `done` |  |
| Mensagem "Reprocessamento concluído. Partidas reconstruídas: N" | `Views/Admin/Index.cshtml:8` | `done` |  |
| Bloco "Servidores de jogo" | `Views/Admin/Index.cshtml:14` | `done` | Blocos `admin-stats`, como na antiga página Bans do Chat. |
| Bloco "Arquivos de estatísticas" | `Views/Admin/Index.cshtml:15` | `done` |  |
| Bloco "Partidas" | `Views/Admin/Index.cshtml:16` | `done` |  |
| Bloco "Partidas ranqueadas" | `Views/Admin/Index.cshtml:17` | `done` |  |
| Bloco "Jogadores" | `Views/Admin/Index.cshtml:18` | `done` |  |
| Bloco "Temporadas" | novo | `done` | Adicionado: o controller já contava as temporadas e a página não mostrava o número. |
| Aviso sobre configurações que não correspondem a nenhuma regra, com o link "Revisar os modos de jogo" | `Views/Admin/Index.cshtml:21` | `done` |  |
| Seção "Servidores de jogo": mensagem "Nenhum servidor de jogo cadastrado ainda." com o link "Adicionar um" | `Views/Admin/Index.cshtml:32` | `done` | `admin-empty` dentro de um `admin-list`. |
| Seção "Servidores de jogo": uma linha por servidor com o ponto de status, o nome (link para a página do servidor) e o número de arquivos | `Views/Admin/Index.cshtml:39` | `done` | `admin-list` / `admin-item`, como na antiga página Bans do Chat. A linha também mostra o estado por extenso e o identificador. |
| Painel "Manutenção": explicação e botão "Reprocessar todas as partidas" (`POST /admin/reprocess`, pergunta "Reconstruir todas as partidas agora?") | `Views/Admin/Index.cshtml:51` | `done` |  |

## Servidores de jogo (`/admin/servers`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página e cabeçalho "Servidores de jogo", menu de administração | `Views/Admin/Servers.cshtml:3` | `done` |  |
| Coluna da tabela "Servidor": nome (link para a página do servidor) e o badge "Desabilitado" | `Views/Admin/Servers.cshtml:14` | `done` | Tabela com as classes do site antigo; no celular, cada célula é uma linha com o seu `data-label`. |
| Coluna da tabela "Identificador" | `Views/Admin/Servers.cshtml:15` | `done` |  |
| Coluna da tabela "Status": ponto e estado | `Views/Admin/Servers.cshtml:16` | `done` |  |
| Coluna da tabela "Arquivos de estatísticas" | `Views/Admin/Servers.cshtml:17` | `done` |  |
| Painel "Adicionar um servidor de jogo": "Nome do servidor" (obrigatório, 100), "Endereço em que os jogadores se conectam (opcional)" (placeholder, 200), botão "Criar" (`POST /admin/servers`) | `Views/Admin/Servers.cshtml:44` | `done` |  |
| Erro "O nome é obrigatório."; depois de criar, a página do servidor abre com o token exibido uma única vez | `Controllers/Admin/AdminController.cs:80` | `done` |  |

## Servidor de jogo (`/admin/servers/{id}`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página e cabeçalho com o nome do servidor, menu de administração | `Views/Admin/Server.cshtml:4` | `done` | Adicionado um botão de volta para a lista de servidores de jogo no cabeçalho. |
| Painel "Como conectar este servidor" com as instruções | `Views/Admin/Server.cshtml:12` | `done` |  |
| Painel "Status" que se atualiza sozinho | `Views/Admin/Server.cshtml:18` | `done` |  |
| Painel "Detalhes": formulário `POST /admin/servers/{id}` com "Nome do servidor" (obrigatório, 100) e "Endereço em que os jogadores se conectam (opcional)" (200) | `Views/Admin/Server.cshtml:28` | `done` |  |
| Painel "Detalhes": switch "Ativado", com o valor `false` oculto | `Views/Admin/Server.cshtml:38` | `done` | Checkbox. |
| Painel "Detalhes": o identificador do servidor e o botão "Salvar" | `Views/Admin/Server.cshtml:42` | `done` |  |
| Painel "Chave": explicação e botão "Gerar uma nova chave" (`POST .../key`, pergunta "Gerar uma nova chave?") | `Views/Admin/Server.cshtml:51` | `done` | `panel-warning`. |
| Painel "Chave": botão "Excluir" (`POST .../delete`, pergunta "Excluir este servidor?") | `Views/Admin/Server.cshtml:54` | `done` |  |
| Erro "Um servidor que já enviou estatísticas não pode ser excluído. Desative-o em vez disso." | `Controllers/Admin/AdminController.cs:144` | `done` |  |

## Configurações (`/admin/settings`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página e cabeçalho "Configurações", menu de administração | `Views/Admin/Settings.cshtml:3` | `done` |  |
| Formulário `POST /admin/settings` com o token antiforgery, botão "Salvar", mensagem "Salvo." | `Views/Admin/Settings.cshtml:9` | `adapted` | O formulário agora é `multipart/form-data`, por causa da logo. Além de "Salvo.", os problemas com os novos campos são listados em vermelho (logo recusada, linhas do rodapé deixadas de fora, ID do Analytics inválido). |
| Painel "Site": "Nome do site" (obrigatório) | `Views/Admin/Settings.cshtml:15` | `done` |  |
| Painel "Site": "Endereço público do site" com placeholder e texto de ajuda | `Views/Admin/Settings.cshtml:19` | `done` |  |
| Painel "Site": "Idioma padrão" | `Views/Admin/Settings.cshtml:25` | `done` |  |
| Painel "Site": "Fuso horário" com a lista de fusos horários | `Views/Admin/Settings.cshtml:34` | `done` |  |
| Painel "Site": "Chave da Steam Web API (opcional)" com o texto de ajuda dele | `Views/Admin/Settings.cshtml:45` | `done` |  |
| Painel "Aparência": logo do site, com prévia, envio e remoção | novo | `done` | Adicionado. Prévia sobre uma faixa com a cor do menu, campo de arquivo (PNG, JPG, WEBP, GIF, até 1 MB) e um checkbox para remover a logo. Armazenada pelo `BrandingStore`, servida em `/site/logo`. |
| Painel "Aparência": links do rodapé | novo | `done` | Adicionado. Um por linha, como `Text | https://address`; salvos normalizados, as linhas que não são links ficam de fora e são informadas. Vazio significa o link para o projeto. |
| Painel "Aparência": ID do Google Analytics | novo | `done` | Adicionado. Opcional; só letras, dígitos e hifens são aceitos. |
| Painel "Aparência": palavras-chave para buscadores | novo | `done` | Adicionado (`SiteSettings.MetaKeywords`): salvas sem espaços nas pontas, separadas por vírgulas, com no máximo 500 caracteres. O `<meta name="keywords">` que as mostra está no layout da branch base. |
| Painel "Temporadas": "Duração da temporada, em meses" (de 1 a 24) com o texto de ajuda dele | `Views/Admin/Settings.cshtml:55` | `done` |  |
| Painel "Pontos do ranking": "Vitória" | `Views/Admin/Settings.cshtml:64` | `done` |  |
| Painel "Pontos do ranking": "Derrotas" | `Views/Admin/Settings.cshtml:65` | `done` |  |
| Painel "Pontos do ranking": "Rage quit" | `Views/Admin/Settings.cshtml:66` | `done` |  |
| Painel "Pontos do ranking": "MVP em dano em infectados especiais, por rodada" | `Views/Admin/Settings.cshtml:67` | `done` |  |
| Painel "Pontos do ranking": "MVP em infectados comuns, por rodada" | `Views/Admin/Settings.cshtml:68` | `done` |  |
| Painel "Pontos do ranking": "Mínimo de partidas para aparecer no ranking" | `Views/Admin/Settings.cshtml:69` | `done` |  |
| Painel "Pontos do ranking": switch "Manter partidas jogadas em mapas que não estão na lista de campanhas", com o valor `false` oculto, e o texto de ajuda | `Views/Admin/Settings.cshtml:72` | `done` | Checkbox. |

## Módulos (`/admin/modules`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página e cabeçalho "Módulos", menu de administração | `Views/Admin/Modules.cshtml:3` | `done` |  |
| Formulário `POST /admin/modules`: nota "Um módulo desligado some do menu...", os switches dos módulos, botão "Salvar", mensagem "Salvo." | `Views/Admin/Modules.cshtml:8` | `done` |  |

## Administradores (`/admin/administrators`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página e cabeçalho "Administradores", menu de administração | `Views/Admin/Administrators.cshtml:3` | `done` |  |
| Coluna da tabela "Nome" ("-" quando não há) | `Views/Admin/Administrators.cshtml:12` | `done` | Adicionado o avatar de cada administrador (oculto no celular); a sua própria linha é destacada como as linhas "sou eu" do resto do site. |
| Coluna da tabela "SteamID64", link para o perfil da Steam | `Views/Admin/Administrators.cshtml:13` | `done` |  |
| Coluna da tabela "Desde" | `Views/Admin/Administrators.cshtml:14` | `done` |  |
| Botão "Remover" (`POST /admin/administrators/{id}/delete`, pergunta "Remover este administrador?") | `Views/Admin/Administrators.cshtml:28` | `done` |  |
| Badge "Você" na sua própria linha, no lugar do botão | `Views/Admin/Administrators.cshtml:34` | `done` | `label label-default`. |
| Painel "Adicionar um administrador": "SteamID64" (número, obrigatório, placeholder), "Nome" (100), botão "Adicionar" | `Views/Admin/Administrators.cshtml:46` | `done` |  |
| Erros "Esse não é um SteamID64 válido." e "Você não pode remover a si mesmo nem o último administrador." | `Controllers/Admin/AdminController.cs:249`, `Controllers/Admin/AdminController.cs:258` | `done` |  |

## Campanhas (`/admin/campaigns`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página e cabeçalho "Campanhas", menu de administração | `Views/Admin/Campaigns.cshtml:3` | `done` |  |
| Explicação de como os mapas formam uma partida | `Views/Admin/Campaigns.cshtml:8` | `done` |  |
| Um formulário por campanha (`POST /admin/campaigns`): nome (obrigatório) e os mapas, um por linha, na ordem em que são jogados | `Views/Admin/Campaigns.cshtml:16` | `done` | Um painel por campanha, em uma grade que vai de quatro colunas a uma no celular. |
| Botão "Salvar" de cada campanha | `Views/Admin/Campaigns.cshtml:21` | `done` |  |
| Botão "Excluir" de cada campanha (`POST /admin/campaigns/{id}/delete`, pergunta "Excluir esta campanha?") | `Views/Admin/Campaigns.cshtml:22` | `done` |  |
| Formulário para uma nova campanha: nome (placeholder "Nova campanha"), mapas (placeholder, obrigatório), botão "Adicionar" | `Views/Admin/Campaigns.cshtml:30` | `done` |  |

## Modos de jogo (`/admin/game-modes`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página e cabeçalho "Modos de jogo", menu de administração | `Views/Admin/GameModes.cshtml:3` | `done` |  |
| Explicação de como as regras são aplicadas | `Views/Admin/GameModes.cshtml:9` | `done` |  |
| Tabela "Regras": colunas Nome, Padrão de configuração, Tamanho do time, Mínimo de mapas, Prioridade, Conta para o ranking | `Views/Admin/GameModes.cshtml:17` | `done` |  |
| Um formulário por regra (`POST /admin/game-modes`): Nome (obrigatório), Padrão de configuração (obrigatório), Tamanho do time (de 1 a 8, "Qualquer" quando vazio), Mínimo de mapas (a partir de 1), Prioridade, Conta para o ranking (Sim/Não) | `Views/Admin/GameModes.cshtml:32` | `done` | Cada regra agora é uma linha de tabela de verdade: os campos são ligados ao formulário da linha pelo atributo `form`, então as colunas se alinham com o cabeçalho e o layout para celular (`data-label`) funciona. Conferido em um navegador: salvar e excluir uma regra enviam os campos certos. |
| Botão "Salvar" de cada regra | `Views/Admin/GameModes.cshtml:45` | `done` |  |
| Botão "Excluir" de cada regra (`POST /admin/game-modes/{id}/delete`, pergunta "Excluir esta regra?") | `Views/Admin/GameModes.cshtml:46` | `done` |  |
| Linha para uma nova regra: placeholder "Nova regra", padrão `*`, mínimo de mapas 2, prioridade 50, botão "Adicionar" | `Views/Admin/GameModes.cshtml:55` | `done` |  |
| Seção "Configurações vistas nos seus servidores" | `Views/Admin/GameModes.cshtml:74` | `done` |  |
| Mensagem "Nada ainda. As configurações aparecem aqui quando os primeiros arquivos de estatísticas chegarem." | `Views/Admin/GameModes.cshtml:77` | `done` | `admin-empty`. |
| Colunas da tabela: Configuração, Tamanho do time, Visto por último, Regra | `Views/Admin/GameModes.cshtml:85` | `done` |  |
| Badge da regra: "Sem regra: fora do ranking", o nome da regra, ou o nome da regra com "Fora do ranking" | `Views/Admin/GameModes.cshtml:101` | `done` | `label-warning`, `label-success`, `label-default`. |

## Ranking da comunidade (`/community`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| As páginas só respondem com o ranking da comunidade ligado | `Controllers/CommunityController.cs:14` | `done` |  |
| Título da página e cabeçalho "Ranking da comunidade" | `Views/Community/Index.cshtml:3` | `done` | `PageMetadata` com a descrição e o avatar do primeiro colocado; cabeçalho como na página do ranking (`h4.text-center`). `ViewBag.Community` marca o menu. |
| Texto "Formado pelos votos dos jogadores. Não é zerado a cada temporada." | `Views/Community/Index.cshtml:8` | `done` |  |
| Botão "Votar" | `Views/Community/Index.cshtml:10` | `done` |  |
| Mensagem "Ninguém tem comparações suficientes para aparecer na lista ainda." | `Views/Community/Index.cshtml:15` | `done` | Jumbotron, como o ranking vazio da página inicial. |
| Coluna da tabela "#" (posição; o primeiro lugar se destaca) | `Views/Community/Index.cshtml:23` | `done` | Como no ranking da página inicial: medalhas para os três primeiros lugares, `#N` para os demais. |
| Coluna da tabela "Jogador": avatar e nome, com link para a página do jogador quando o módulo de ranking está ligado | `Views/Community/Index.cshtml:24` | `adapted` | As mesmas duas células do ranking da página inicial (avatar, nome). O nome é encurtado por `TruncatePlayerName`, como no ranking da página inicial; o nome completo fica no título do avatar. |
| Coluna da tabela "Patente" | novo | `done` | Adicionada, como no ranking da página inicial: a patente do jogador na temporada atual. Só com o módulo de ranking ligado. |
| Coluna da tabela "Pontuação" | `Views/Community/Index.cshtml:25` | `done` |  |
| Coluna da tabela "Comparações" | `Views/Community/Index.cshtml:26` | `adapted` | Coluna no desktop. No celular, a coluna fica oculta (`hide-mobile`) e o número vira uma linha abaixo do nome; a página em Bootstrap 5, em vez disso, tinha rolagem lateral. |
| Coluna da tabela "Vitórias" | `Views/Community/Index.cshtml:27` | `done` |  |
| Coluna da tabela "Empates" | `Views/Community/Index.cshtml:28` | `adapted` | Igual a "Comparações". |
| Coluna da tabela "Derrotas" | `Views/Community/Index.cshtml:29` | `adapted` | Igual a "Comparações". |
| A linha do jogador logado fica destacada | `Views/Community/Index.cshtml:35` | `done` | `it-is-me`, a classe que o site antigo usa. |
| Nota "Mínimo de comparações para aparecer na lista: N" | `Views/Community/Index.cshtml:67` | `done` |  |

## Votar (`/community/vote`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página e cabeçalho "Quem é melhor?" | `Views/Community/Vote.cshtml:3` | `done` | `PageMetadata`; `ViewBag.Community` marca o menu. |
| Texto "Suas respostas fazem os jogadores subir e descer no ranking da comunidade." | `Views/Community/Vote.cshtml:8` | `done` |  |
| Botão "Ranking da comunidade" (volta para o ranking) | `Views/Community/Vote.cshtml:10` | `done` |  |
| Aviso quando a resposta foi recusada ("Essa dupla não é a que está aguardando...", "Escolha uma das respostas.") | `Views/Community/_Ballot.cshtml:6` | `done` |  |
| Sem login: "Entre com o Steam para votar." com o botão da Steam, que volta para esta página | `Views/Community/_Ballot.cshtml:14` | `done` | Jumbotron com o botão da Steam. |
| Poucas partidas: mensagem com "Partidas jogadas: N / mínimo" | `Views/Community/_Ballot.cshtml:23` | `done` | Jumbotron. |
| Limite diário: mensagem com "Votos por dia: N" | `Views/Community/_Ballot.cshtml:30` | `done` | Jumbotron. |
| Não sobrou ninguém: mensagem | `Views/Community/_Ballot.cshtml:36` | `done` | Jumbotron. |
| Formulário da cédula (`POST /community/vote`, token antiforgery); com scripts, a resposta é enviada em segundo plano e só a cédula é substituída | `Views/Community/_Ballot.cshtml:43` | `done` | Continua sendo htmx, carregado só por esta página. Conferido em um navegador: a resposta é registrada e a próxima dupla aparece sem recarregar. |
| Sem scripts, o formulário é enviado normalmente e a página recarrega, com a recusa exibida como erro | `Controllers/CommunityController.cs:60` | `done` |  |
| Os dois jogadores lado a lado, cada um com avatar e nome | `Views/Community/_Ballot.cshtml:50` | `done` | Dois painéis do Bootstrap 3, lado a lado também no celular. |
| Botão "Este é melhor" abaixo de cada jogador | `Views/Community/_Ballot.cshtml:52` | `done` |  |
| Botão "Não conheço este jogador" abaixo de cada jogador | `Views/Community/_Ballot.cshtml:53` | `done` |  |
| Botão "Estão no mesmo nível" | `Views/Community/_Ballot.cshtml:69` | `done` |  |
| Botão "Não conheço nenhum dos dois" | `Views/Community/_Ballot.cshtml:70` | `done` |  |
| Contador "Votos restantes hoje: N / por dia" | `Views/Community/_Ballot.cshtml:74` | `done` |  |

## Administração do ranking da comunidade (`/admin/community`)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Só para administradores, e só com o ranking da comunidade ligado | `Controllers/Admin/CommunityAdminController.cs:16` | `done` |  |
| Título da página e cabeçalho "Ranking da comunidade", menu de administração | `Views/CommunityAdmin/Index.cshtml:3` | `done` | Adicionado um botão para o ranking público no cabeçalho. |
| Mensagem "Pontuações reconstruídas. Votos reaplicados: N" | `Views/CommunityAdmin/Index.cshtml:9` | `done` |  |
| Mensagem "Pontuações reconstruídas sem esse votante. Votos removidos: N" | `Views/CommunityAdmin/Index.cshtml:13` | `done` |  |
| Bloco "Votos" | `Views/CommunityAdmin/Index.cshtml:19` | `done` | `admin-stats`. |
| Bloco "Jogadores pontuados" | `Views/CommunityAdmin/Index.cshtml:20` | `done` |  |
| Formulário `POST /admin/community` com o token antiforgery, botão "Salvar", mensagem "Salvo." | `Views/CommunityAdmin/Index.cshtml:23` | `done` |  |
| Painel "Votação": "Mínimo de partidas para votar" | `Views/CommunityAdmin/Index.cshtml:30` | `done` |  |
| Painel "Votação": "Votos por dia" (com o mínimo dele) | `Views/CommunityAdmin/Index.cshtml:34` | `done` |  |
| Painel "Votação": "Mínimo de partidas para ser comparado" e o texto de ajuda | `Views/CommunityAdmin/Index.cshtml:38` | `done` |  |
| Painel "Pontuação": "Mínimo de comparações para aparecer na lista" | `Views/CommunityAdmin/Index.cshtml:51` | `done` |  |
| Painel "Pontuação": "Fator K do Elo" (com os limites dele) | `Views/CommunityAdmin/Index.cshtml:55` | `done` |  |
| Painel "Pontuação": "Pontuação inicial" e o texto de ajuda | `Views/CommunityAdmin/Index.cshtml:59` | `done` |  |
| Painel "Manutenção": explicação e botão "Reconstruir as pontuações" (pergunta "Reconstruir todas as pontuações agora?") | `Views/CommunityAdmin/Index.cshtml:73` | `done` |  |
| Painel "Votantes": explicação e o formulário para remover os votos de um SteamID64 (pergunta "Remover todos os votos deste votante?") | `Views/CommunityAdmin/Index.cshtml:85` | `done` |  |
| Painel "Votantes": mensagem "Ninguém votou ainda." | `Views/CommunityAdmin/Index.cshtml:94` | `done` |  |
| Painel "Votantes": coluna da tabela "Jogador", avatar e nome com link para o perfil da Steam | `Views/CommunityAdmin/Index.cshtml:102` | `done` | O avatar fica oculto no celular, onde cada célula é uma linha com o seu rótulo. |
| Painel "Votantes": coluna da tabela "Votos" | `Views/CommunityAdmin/Index.cshtml:103` | `done` |  |
| Painel "Votantes": coluna da tabela "Último voto" | `Views/CommunityAdmin/Index.cshtml:104` | `done` |  |
| Painel "Votantes": botão "Remover votos" em cada linha (pede confirmação) | `Views/CommunityAdmin/Index.cshtml:122` | `done` |  |
| Erro "Esse não é um SteamID64 válido." | `Controllers/Admin/CommunityAdminController.cs:64` | `done` |  |

## Página de boas-vindas (`/` com o módulo de ranking desligado)

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Título da página: o nome do site | `Views/Home/Welcome.cshtml:4` | `done` | Já convertida na branch base; ficou como estava. |
| Caixa com o nome do site e "Use o menu para ver o que está disponível neste site." | `Views/Home/Welcome.cshtml:6` | `done` |  |

## Layout

| Item | Onde hoje | Situação | Observações |
|---|---|---|---|
| Estilos do Bootstrap 5 e `bootstrap.bundle` | `Views/Shared/_Bootstrap5Layout.cshtml:19` | `not ported` | É o objetivo deste trabalho: as páginas agora usam os temas em Bootstrap 3 do `_Layout`. `_Bootstrap5Layout` e `wwwroot/lib/bootstrap` continuam no repositório para as pastas que outras branches convertem. |
| htmx carregado em todas as páginas | `Views/Shared/_Bootstrap5Layout.cshtml:114` | `adapted` | O `_Layout` não o carrega. Os três lugares que precisam dele o carregam na seção `Scripts`: a página do servidor de jogo, o passo de servidor do assistente e a página de votação. |
| Classes do `site.css` usadas por estas páginas (ponto de status, passos do assistente, bloco de configuração, avatares) | `Views/Shared/_Bootstrap5Layout.cshtml:20` | `adapted` | O que estas páginas ainda precisam (ponto de status, passos do assistente, bloco de configuração, grade de painéis) está na partial `Views/Shared/_AdministrationStyles.cshtml`, ao lado de `_AdminStyles`; os estilos da cédula ficam na página de votação. |
| Mensagens de erro e de "Salvo." no topo da página | `Views/Shared/_Bootstrap5Layout.cshtml:100` | `done` | Pelo `_Layout`. |
