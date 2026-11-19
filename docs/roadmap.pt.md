# Roadmap

🌐 [English](roadmap.md) | **Português** | [Español](roadmap.es.md)

O plano para trazer o sistema L4D2 PlayStats para este repositório: de onde ele veio, a arquitetura, as decisões tomadas, o que está pronto e o que ainda está em aberto.

## Conteúdo

- [De onde veio](#de-onde-veio)
- [Arquitetura](#arquitetura)
- [Decisões](#decisões)
- [Módulos](#módulos)
- [Servidores de jogo](#servidores-de-jogo)
- [Temporadas e rankings](#temporadas-e-rankings)
- [Modos de jogo](#modos-de-jogo)
- [Ranking da comunidade](#ranking-da-comunidade)
- [Loja de skins de armas](#loja-de-skins-de-armas)
- [Servidor ao vivo](#servidor-ao-vivo)
- [Assistente de configuração e status do servidor](#assistente-de-configuração-e-status-do-servidor)
- [Front-end](#front-end)
- [Testes](#testes)
- [Implantação](#implantação)
- [Migração de dados](#migração-de-dados)
- [Progresso](#progresso)
- [Perguntas em aberto](#perguntas-em-aberto)

## De onde veio

| Parte | Repositório | Como funcionava |
|---|---|---|
| Parser de estatísticas | [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | Pacote NuGet que lê os arquivos gravados pelo `l4d2_playstats`. |
| API | [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | Azure Functions. Armazenava os arquivos brutos no Azure Table Storage e reconstruía partidas e ranking a partir deles a cada requisição. |
| Site | [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | ASP.NET Core MVC. Lia da API, mantinha a loja de skins no Azure Table Storage e a partida ao vivo em memória. |
| Plugins | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Cinco plugins SourceMod conversavam com a API e com o site, com endereços e segredos diferentes. |

Limites daquela estrutura, que este projeto elimina:

- A duração da temporada era fixa em dois meses, em dois lugares separados.
- As temporadas passadas existiam apenas como listas de top 100 salvas, então não dava para navegar pelas partidas delas.
- O site pressupunha um único servidor de jogo.
- Só contavam partidas 4v4 com pelo menos dois mapas, e os modos de jogo aceitos eram um padrão escrito à mão em uma linha de armazenamento.
- Mapas fora de uma lista fixa de campanhas eram descartados.
- A instalação exigia uma assinatura do Azure, linhas de armazenamento criadas à mão e segredos em três projetos.

## Arquitetura

Uma única aplicação ASP.NET Core que é o site, a API para os plugins, a área administrativa e o agendador.

- **O arquivo bruto de estatísticas continua armazenado** como a fonte da verdade. Partidas, mapas e resultados por jogador são tabelas construídas a partir dele, e um comando de reprocessamento as reconstrói quando as regras mudam.
- **As configurações ficam no banco de dados** e são editadas na área administrativa. Só o provedor de banco de dados, a connection string e a pasta de dados são definidos fora dele.
- **Uma única instância do back-end** atende todos os servidores de jogo. Rodar várias instâncias está fora do escopo.
- **A partida ao vivo fica em memória**, nunca no banco de dados, e é mantida separadamente para cada servidor de jogo.
- **Cada funcionalidade é um módulo** no código e, onde faz sentido, algo que o administrador pode desligar.

O [CONTRIBUTING.pt.md](../CONTRIBUTING.pt.md) descreve as pastas e as regras de código.

## Decisões

| Assunto | Decisão |
|---|---|
| Rankings | Dois rankings. O ranking de estatísticas é o principal e abre o site. O ranking da comunidade é secundário. |
| Escopo dos rankings | Os dois são globais. Os resultados de um jogador em todos os servidores de jogo se somam. |
| Duração da temporada | Definida em meses pelo administrador. Uma mudança vale a partir da próxima temporada. |
| Limites das temporadas | Seguem o relógio do servidor de jogo que enviou cada arquivo, como no sistema antigo. |
| Temporadas passadas | Dá para navegar pelo ranking, pelas partidas e pelos jogadores de qualquer temporada passada, além de um ano inteiro e de todo o histórico. |
| Mudanças de regra | Valem dali em diante. O administrador pode reprocessar todas as partidas. |
| Mínimos | O número mínimo de mapas em uma partida e o número mínimo de partidas para aparecer no ranking são, os dois, configurações. |
| Mapas desconhecidos | Uma partida em mapas fora da lista de campanhas é mantida. Uma configuração desliga isso. |
| Banco de dados | Sempre relacional, por meio do Entity Framework Core. O SQLite é o padrão. SQL Server, PostgreSQL e MySQL são opções. |
| Versão do Entity Framework Core | 10, a versão de suporte de longo prazo. Os providers de PostgreSQL e MySQL ainda não estão disponíveis para a versão 11. |
| Back-end | Uma única instância, seja qual for o número de servidores de jogo. |
| Partida ao vivo | Somente em memória, separada por servidor de jogo. |
| Reembolso de skins | Reembolsar as skins no fim de uma temporada é uma configuração, ligada por padrão. |
| Catálogo de skins | O hub já vem com o catálogo e as miniaturas do site atual, e importa o catálogo na primeira vez que a loja é ligada. As fotos são lidas do endereço dos assets, por padrão o do site atual. |
| Plugins | Voltados para servidores baseados no L4D2 Competitive Rework. Um endereço e um token para todos eles. |
| Comandos remotos | Enviar comandos do site para um servidor de jogo é um módulo, desligado por padrão, com log de auditoria. |
| Anti-cheat | Um módulo, desligado por padrão, que mostra a página do Anti-Cheat com o download e as instruções. O programa Anti-Cheat e o serviço dele são projetos separados. |
| Front-end | Duas camadas: o site principal, para os navegadores atuais, e algumas páginas feitas para o navegador dentro do jogo. |
| Páginas dentro do jogo | Só o essencial: o ranking da temporada, o resumo do jogador que abriu o painel e as últimas partidas. |
| Área administrativa | Renderizada no servidor, como o resto do site. Um framework de página única, como o Angular, é permitido se alguma tela precisar, e não é usado. |
| Docker | Oferecido como alternativa ao script de instalação. O contêiner roda o site com um usuário sem privilégios e tem verificação de saúde. |
| Dados antigos | Importados e recalculados. Não há compatibilidade retroativa com o armazenamento antigo. |
| Idiomas | O inglês é o padrão. As versões em português e em espanhol derivam dele. |
| Organização do código | Um tipo por arquivo, agrupados por módulo e depois por categoria. |
| Testes | Extensos no back-end. Testes de front-end só onde compensam. |
| Licença | MIT para a aplicação. Os plugins são GPL-3.0, como o SourceMod exige. |

## Módulos

O núcleo está sempre ligado: cadastro de servidores de jogo, envio de estatísticas, login com a Steam e a área administrativa.

| Módulo | Conteúdo | Padrão |
|---|---|---|
| Ranking e estatísticas | Ranking, partidas, jogadores, patentes, punições e histórico de temporadas. | Ligado |
| Servidor ao vivo | Placar, jogadores e eventos da partida em andamento. | Ligado |
| Chat do site | Chat entre o site e o jogo, com banimentos. Precisa do servidor ao vivo. | Desligado |
| Comandos remotos | Comandos enviados do site para um servidor de jogo. Precisa do servidor ao vivo. | Desligado |
| Loja de skins de armas | Estrelas, compras, presentes e skins equipadas. Precisa do ranking. | Desligado |
| Ranking da comunidade | Votação em pares e o ranking dela. | Desligado |
| Anti-Cheat | A página com o download e as instruções do Anti-Cheat. | Desligado |

Um módulo desligado some do menu, e as páginas dele respondem como não encontradas.

## Servidores de jogo

- O administrador cadastra cada servidor de jogo e recebe um identificador e uma chave. Só um hash da chave é armazenado.
- Os plugins usam um endereço e um token no formato `identifier:key`. O token diz ao site qual servidor enviou cada arquivo, então o formato do arquivo de estatísticas não mudou.
- Os arquivos são únicos por servidor e nome de arquivo.
- A partida ao vivo, o chat do site e a fila de comandos são separados por servidor. O visitante escolhe qual servidor acompanhar.

## Temporadas e rankings

- As temporadas são linhas no banco de dados, criadas a partir da duração configurada. Uma temporada de dois meses começa em janeiro, março, maio e assim por diante.
- Um arquivo com data anterior à primeira temporada cria as temporadas anteriores de que precisa.
- A pontuação mantém os valores antigos como padrões que o administrador pode editar: 85 por vitória, -80 por derrota, -150 por rage quit, 8 e 6 para cada MVP. A experiência nunca fica abaixo de zero.
- As punições pertencem a uma temporada.
- As partidas nunca atravessam um limite de temporada, uma mudança de configuração ou uma troca de servidor de jogo.

## Modos de jogo

O arquivo de estatísticas traz o nome de exibição da configuração, como `ZoneMod v2.9.2b`, e o tamanho do time. Ele não traz o nome da pasta da configuração.

- Todo nome de configuração recebido é registrado automaticamente e listado na área administrativa.
- O administrador escreve regras em cima desses nomes: um padrão em que `*` representa qualquer texto, o tamanho do time, se conta para o ranking e o número mínimo de mapas.
- A primeira regra que se encaixa em uma partida decide. Uma partida que não se encaixa em nenhuma regra não conta.
- As campanhas são uma tabela editável, usada para saber quais mapas pertencem à mesma partida.

## Ranking da comunidade

- O votante vê dois jogadores com seus avatares e responde qual é melhor: o da esquerda, o da direita, mesmo nível ou "não conheço" um deles ou os dois.
- Mesmo nível conta como empate. "Não conheço" não altera nada e esconde aquele jogador daquele votante dali em diante.
- O rating é Elo e não é zerado com a temporada.
- Votar exige um número mínimo de partidas jogadas, contadas em todas as temporadas: 10 por padrão.
- Os votos por dia são limitados: 20 por padrão. Os jogadores não podem votar em si mesmos nem duas vezes no mesmo par.
- O site escolhe o par, dando preferência a jogadores com quem ou contra quem o votante já jogou, e só aceita resposta para o par que ofereceu.

## Loja de skins de armas

- Os jogadores ganham uma estrela para cada MVP em partidas ranqueadas, ao longo de todo o histórico, e gastam estrelas em skins.
- O catálogo e as miniaturas do site atual vêm com o hub, e o catálogo é importado sozinho na primeira vez que a loja é ligada. Um administrador pode importar outro catálogo de um arquivo ou de um endereço. As fotos são carregadas de um endereço que o administrador define, por padrão o do site atual.
- No início de uma temporada, as skins compradas antes são reembolsadas e os presentes são mantidos. O administrador pode desligar o reembolso.
- Modelos, materiais e fotos ficam no [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

## Servidor ao vivo

- O servidor de jogo envia a configuração, o round, o placar, os jogadores, o chat e os eventos da partida.
- O site mostra tudo isso com alguns segundos de atraso, como o site antigo fazia.
- As conexões por endereço IP são armazenadas para apontar contas que compartilham um endereço. O endereço em si nunca é exibido.

## Assistente de configuração e status do servidor

O assistente de primeira execução roda uma vez, depois da instalação: o primeiro administrador entra com a Steam usando um código impresso pelo instalador e depois escolhe as configurações do site, os módulos e o primeiro servidor de jogo.

Para cada servidor de jogo, o site mostra as duas linhas que devem ser coladas na configuração dele e então espera o servidor responder. É o servidor de jogo que chama o site, e não o contrário, o que funciona atrás de NAT e dispensa senha de RCON.

| Estado | Significado |
|---|---|
| Aguardando | Nunca conectou. |
| Conectado | Um sinal chegou nos últimos 90 segundos. |
| Offline | Mostra quando foi visto pela última vez. |
| Chave inválida | O identificador chegou com a chave errada nos últimos 10 minutos. |

Uma checklist mostra o que foi encontrado: conexão, plugin de envio, gravação de estatísticas, primeiro arquivo recebido, dados ao vivo e o plugin de skins. O comando `sm_playstats_test` roda a mesma verificação pelo console do servidor de jogo.

## Front-end

O navegador dentro do jogo é o Chromium 18, de 2012. Ele roda apenas ES5 e não tem flexbox, grid nem propriedades personalizadas. As medições estão em [in-game-browser.pt.md](in-game-browser.pt.md).

- **Site principal.** Páginas renderizadas no servidor que mantêm o visual e o conteúdo do site que este projeto substitui: Bootstrap 3 com os temas escuro e claro, jQuery, Chart.js e diffDOM, sem etapa de build com Node. Cada página do site antigo foi portada item por item; os checklists estão em [site-parity](site-parity/README.pt.md). As páginas que são novas aqui (assistente, administração, ranking da comunidade) seguem o mesmo visual.
- **Páginas dentro do jogo.** Três páginas somente leitura em `/ingame`, sem script, que cabem no painel do jogo. Um teste falha se elas começarem a usar algo que aquele navegador não tem.

## Testes

- **Back-end.** Testes unitários para montagem de partidas, pontuação, temporadas, regras de modos de jogo, rating Elo e loja de skins. Testes via HTTP para cada página e para cada endpoint que os plugins chamam.
- **Paridade.** Um teste compara os rankings com uma cópia dos dados de produção. O resultado bate exatamente com o da API antiga nas temporadas calculadas com as regras atuais.
- **Bancos de dados.** Os mesmos testes rodam em SQLite, SQL Server, PostgreSQL e MySQL e verificam se as migrations de cada um batem com o modelo.
- **Plugins.** Todos os plugins são compilados a cada mudança.

## Implantação

- **VPS Linux.** Um comando instala uma build self-contained como serviço, com HTTPS opcional pelo Caddy, e imprime o código de configuração. Atualização, backup e restauração são comandos únicos.
- **Backup agendado.** O site envia a cada algumas horas um zip com uma cópia do banco SQLite, as chaves e o logo para um contêiner do Azure Storage, configurado em Administração > Backup.
- **Docker.** Uma imagem publicada a cada versão.
- **Azure.** Uma máquina virtual com o instalador Linux, ou o App Service com a imagem.

## Migração de dados

Um programa de console separado, fora deste repositório, move os dados de produção uma única vez:

- Ele lê os arquivos brutos de estatísticas do armazenamento antigo, sem alterá-lo, e envia cada um pelo mesmo endpoint que o plugin usa, de modo que tudo é recalculado pelo caminho normal.
- As punições antigas eram apagadas a cada troca de temporada e não voltam.
- Os rankings salvos antes de agosto de 2026 foram calculados com uma regra mais antiga para o número mínimo de mapas, então o histórico recalculado difere dessas listas.

## Progresso

| Fase | Conteúdo | Status |
|---|---|---|
| 1. Fundação | Banco de dados, configurações e módulos, login com a Steam, assistente, cadastro e status de servidores de jogo. | Concluída |
| 2. Estatísticas e ranking | Envio, montagem de partidas, pontuação, punições, temporadas e regras de modos de jogo. | Concluída |
| 3. Site | Ranking, histórico, partidas, jogadores, patentes, página do Anti-Cheat, área administrativa e as páginas dentro do jogo, em três idiomas. | Concluída; cada tela do site antigo portada e conferida item por item |
| 4. Servidor ao vivo | Estado ao vivo por servidor, seletor de servidor, chat do site, comandos remotos e banimentos do chat. | Concluída, ainda não vista com um servidor de jogo real |
| 5. Loja de skins de armas | Loja, presentes, reembolsos, importação do catálogo e reembolso de temporada. | Concluída, com o catálogo e as miniaturas do site atual; ainda não testada em um servidor de jogo |
| 6. Plugins | Os cinco plugins com um endereço e um token, heartbeat e teste de conexão. | Compilados, ainda não rodaram em um servidor de jogo |
| 7. Ranking da comunidade | Página de votação, rating, limites e página administrativa. | Concluída |
| 8. Implantação | Instalador Linux, imagem Docker, workflow de release e guias. | Concluída; publicada desde a v0.1.1, instalador e imagem testados a partir da versão publicada; backup agendado no Azure Storage desde a v0.9.0 |
| 9. Migração | Importação dos dados de produção, troca do site de produção, arquivamento dos repositórios antigos. | Dados de produção importados na VPS do l4d2.com.br, onde o site novo roda na porta 5090 ao lado do antigo ([Trocando o site antigo](switch-from-old-site.pt.md)); a troca está pendente |

## Perguntas em aberto

- **Plugins em um servidor real.** Eles compilam, e o site aceita o que enviam, mas ainda não rodaram em uma partida de verdade.
- **HTTPS dentro do jogo.** Não medido. Veja [in-game-browser.pt.md](in-game-browser.pt.md#ainda-não-medido).
- **Repositório de assets.** Resolvido: o catálogo e as 196 miniaturas do site atual vêm com o hub, como no site antigo, e as fotos são lidas de `https://l4d2playstats.blob.core.windows.net/assets/skin-shots` por padrão.
- **Ícones de patente no jogo.** Os sprites usados pelo `l4d2_show_patent_icon` não estão neste repositório.
- **Azure App Service.** O guia foi escrito a partir da documentação do Azure e não foi executado de ponta a ponta.
