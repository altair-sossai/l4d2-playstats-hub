# Roadmap

🌐 [English](roadmap.md) | **Português** | [Español](roadmap.es.md)

O plano para trazer o sistema L4D2 PlayStats para este repositório: o que existe hoje, a arquitetura alvo, as decisões já tomadas, as fases e o que ainda está em aberto.

## Conteúdo

- [Como as coisas estão hoje](#como-as-coisas-estão-hoje)
- [Arquitetura alvo](#arquitetura-alvo)
- [Decisões](#decisões)
- [Módulos](#módulos)
- [Servidores de jogo](#servidores-de-jogo)
- [Temporadas e rankings](#temporadas-e-rankings)
- [Modos de jogo](#modos-de-jogo)
- [Ranking da comunidade](#ranking-da-comunidade)
- [Loja de skins de armas](#loja-de-skins-de-armas)
- [Assistente de configuração e status dos servidores](#assistente-de-configuração-e-status-dos-servidores)
- [Front-end](#front-end)
- [Testes](#testes)
- [Implantação](#implantação)
- [Migração de dados](#migração-de-dados)
- [Fases](#fases)
- [Perguntas em aberto](#perguntas-em-aberto)

## Como as coisas estão hoje

| Parte | Repositório | Como funciona hoje |
|---|---|---|
| Parser de estatísticas | [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | Pacote NuGet que lê os arquivos gravados pelo `l4d2_playstats`. |
| API | [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | Azure Functions. Armazena os arquivos brutos no Azure Table Storage e reconstrói partidas e ranking a partir deles a cada requisição. |
| Site | [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | ASP.NET Core MVC. Lê da API, mantém a loja de skins no Azure Table Storage e a partida ao vivo em memória. |
| Plugins | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Cinco plugins SourceMod conversam com a API e com o site. |

Limites que este projeto elimina:

- A duração da temporada é fixa em dois meses, em dois lugares separados.
- As temporadas passadas existem apenas como listas de top 100 salvas, então não dá para navegar pelas partidas delas.
- O site pressupõe um único servidor de jogo.
- Só contam partidas 4v4 com pelo menos dois mapas, e os modos de jogo aceitos são um padrão escrito à mão em uma linha de armazenamento.
- Mapas fora de uma lista fixa de campanhas são descartados.
- Instalar o sistema exige uma assinatura do Azure, linhas de armazenamento criadas à mão e segredos em três projetos.

## Arquitetura alvo

Uma única aplicação ASP.NET Core que é o site, a API para os plugins, a área administrativa e o agendador.

| Pasta | Conteúdo |
|---|---|
| `src/` | A aplicação, a lógica de domínio e a camada de dados com Entity Framework Core, com um projeto de migrations por banco de dados. |
| `plugins/` | Fontes SourceMod, includes, traduções e arquivos de configuração. |
| `deploy/` | Script de instalação, definição do serviço, proxy reverso e arquivos do Docker. |
| `docs/` | Guias em inglês, português e espanhol. |

Pontos principais:

- **O arquivo bruto de estatísticas continua armazenado** como a fonte da verdade. Partidas, mapas e resultados por jogador viram tabelas, então os rankings são consultas ao banco de dados, e um comando de reprocessamento pode aplicar regras novas a arquivos antigos.
- **As configurações ficam no banco de dados** e são editadas na área administrativa. Só o provedor de banco de dados e a connection string ficam em um arquivo de configuração.
- **Uma única instância do back-end** atende todos os servidores de jogo. Rodar várias instâncias está fora do escopo por enquanto.
- **A partida ao vivo fica em memória**, nunca no banco de dados. Ela é mantida separadamente para cada servidor de jogo.

## Decisões

| Tema | Decisão |
|---|---|
| Rankings | Dois rankings. O ranking de estatísticas é o principal e abre o site. O ranking da comunidade é secundário. |
| Escopo dos rankings | Ambos são globais. Os resultados de um jogador em todos os servidores de jogo se somam. |
| Duração da temporada | Definida em meses pelo administrador. Uma alteração vale a partir da próxima temporada. |
| Temporadas passadas | É possível navegar pelo ranking, pelas partidas e pelos jogadores de qualquer temporada passada. |
| Mudanças de regra | Valem dali em diante. O administrador pode reprocessar manualmente. |
| Mínimos | O número mínimo de mapas em uma partida e o número mínimo de partidas para aparecer no ranking são configurações. |
| Banco de dados | Sempre relacional, via Entity Framework Core. O SQLite é o padrão. SQL Server, PostgreSQL e MySQL são opções. |
| Back-end | Uma única instância, seja qual for o número de servidores de jogo. |
| Partida ao vivo | Somente em memória, separada por servidor de jogo. |
| Reembolso de skins | Reembolsar as skins no fim da temporada é uma configuração, ligada por padrão. |
| Catálogo de skins | O catálogo e as miniaturas ficam no repositório de assets, e a aplicação os importa. |
| Plugins | Voltados para servidores baseados no L4D2 Competitive Rework. |
| Comandos remotos | O envio de comandos do site para um servidor de jogo está incluído como módulo. |
| Anti-cheat | Reservado como módulo, desligado, sem implementação por enquanto. |
| Docker | Oferecido como alternativa ao script de instalação. |
| Dados antigos | Importados e recalculados. Não há retrocompatibilidade com o armazenamento antigo. |
| Idiomas | O inglês é o padrão. As versões em português e em espanhol derivam dele. |
| Testes | Extensos no back-end. Testes de front-end só onde compensam. |

## Módulos

O núcleo está sempre ligado: cadastro de servidores de jogo, envio de estatísticas, login com a Steam e a área administrativa.

| Módulo | Conteúdo |
|---|---|
| Ranking e estatísticas | Ranking, partidas, jogadores, patentes, punições e histórico de temporadas. |
| Servidor ao vivo | Placar, jogadores, eventos e chat da partida em andamento. |
| Loja de skins de armas | Estrelas, compras, presentes e skins equipadas. |
| Ranking da comunidade | Votação em pares e o ranking resultante. |
| Chat do site | Chat entre o site e o jogo, com banimentos. |
| Comandos remotos | Comandos enviados do site para um servidor de jogo, com log de auditoria. |
| Anti-cheat | Reservado. |

Um módulo desligado some do menu, e suas rotas respondem como não encontradas. A loja de skins depende do módulo de estatísticas, porque as estrelas vêm dos resultados das partidas.

## Servidores de jogo

- O administrador cadastra cada servidor de jogo e recebe um identificador e uma chave para ele. A chave é armazenada em forma de hash.
- Os plugins usam um único endereço e um único token no formato `id:key`. O token diz ao back-end qual servidor enviou cada arquivo, então o formato do arquivo de estatísticas não muda.
- Os arquivos são únicos por servidor e nome de arquivo.
- A partida ao vivo, o chat do site e a fila de comandos são separados por servidor. O site deixa o visitante escolher qual servidor acompanhar.

## Temporadas e rankings

- As temporadas são linhas no banco de dados, criadas a partir da duração configurada.
- Um arquivo com data anterior à primeira temporada cria as temporadas anteriores de que precisa, usando a mesma duração.
- O ranking de estatísticas mantém a pontuação atual como valores padrão que o administrador pode editar: pontos por vitória, por derrota e por rage quit, e pontos de bônus por MVPs.
- As punições pertencem a uma temporada.

## Modos de jogo

O arquivo de estatísticas traz o nome de exibição da configuração, como `ZoneMod v2.9.2b`, e o tamanho dos times. Ele não traz o nome da pasta da configuração.

- Todo nome de configuração recebido é registrado automaticamente.
- O administrador escreve regras em cima desses nomes: um padrão de nome que ignora a versão, os tamanhos de time, se conta para o ranking e o número mínimo de mapas.
- As campanhas viram uma tabela editável, e um mapa fora da tabela deixa de ser descartado.

## Ranking da comunidade

- O votante vê dois jogadores com seus avatares da Steam e responde qual deles é melhor.
- As respostas são: esquerda, direita, mesmo nível ou "não conheço um deles".
- Mesmo nível conta como empate no cálculo. "Não conheço" não altera nada.
- Para votar é preciso ter um número mínimo de partidas jogadas, contadas em todas as temporadas. É uma configuração, com 10 como padrão.
- O número de votos por dia é limitado por uma configuração. Os jogadores não podem votar em si mesmos nem repetir um par.
- Os candidatos são jogadores com partidas registradas.
- O ranking não é zerado com a temporada.

## Loja de skins de armas

- Os jogadores ganham estrelas com MVPs e as gastam em skins.
- Compras, presentes, preços e skins equipadas saem do Azure Table Storage e vão para o banco de dados.
- O reembolso de temporada segue as mesmas temporadas do ranking, e o administrador pode desligá-lo.
- Modelos, materiais e fotos continuam no [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

## Assistente de configuração e status dos servidores

Assistente de primeira execução, exibido uma vez após a instalação:

1. O primeiro administrador faz login com a Steam, usando um código impresso pelo instalador.
2. Nome do site, idioma e chave da Steam Web API.
3. Módulos a ligar.
4. Duração da temporada e regras iniciais de modos de jogo.
5. O primeiro servidor de jogo.

Assistente de servidor de jogo, usado para o primeiro servidor e para cada um adicionado depois:

1. Nome e endereço.
2. O que precisa ser instalado no servidor de jogo, com um link para baixar os plugins.
3. O bloco de configuração exato a colar e o arquivo em que ele vai.
4. Validação. A página aguarda e muda para "conectado" quando o servidor de jogo responde.

É o servidor de jogo que chama o back-end, e não o contrário. Isso funciona atrás de NAT e dispensa senha de RCON no site. Cada servidor mostra um destes estados:

| Estado | Significado |
|---|---|
| Aguardando | Nunca conectou. |
| Online | Um sinal chegou nos últimos segundos. |
| Offline | Mostra quando foi visto pela última vez. |
| Chave inválida | O identificador chegou com a chave errada. |

Cada servidor também tem uma checklist, com uma dica de correção em cada item que falhou: conexão, versões dos plugins, gravação de estatísticas habilitada, primeiro arquivo de estatísticas recebido, dados ao vivo chegando e o plugin de skins quando esse módulo está ligado. Um comando de console no servidor de jogo executa a mesma verificação a partir daquele lado.

## Front-end

O navegador embutido no jogo é o Chromium 18, de 2012. Ele roda apenas ES5 e não tem flexbox, grid nem propriedades personalizadas. As medições estão em [in-game-browser.pt.md](in-game-browser.pt.md).

Abordagem proposta, ainda a confirmar:

- O site principal é voltado para os navegadores atuais: páginas renderizadas no servidor com um framework CSS moderno e sem etapa de build com Node.
- Um pequeno conjunto de páginas para o jogo fica em uma rota própria, segue os limites daquele navegador, é servido por HTTP simples e é o que o plugin abre.

## Testes

- **Back-end, a prioridade.** Testes unitários para a montagem de partidas, a pontuação, as temporadas, as regras de modos de jogo, o rating da comunidade e a carteira da loja de skins. Testes de integração contra um banco SQLite real e contra os outros bancos de dados na integração contínua. Testes de endpoint para tudo o que os plugins chamam, para que o contrato com o servidor de jogo não possa quebrar sem que ninguém perceba.
- **Plugins.** Todo plugin é compilado na integração contínua.
- **Front-end.** Apenas smoke tests de que as páginas renderizam, além de uma verificação de que as páginas do jogo permanecem dentro dos limites do navegador antigo.

## Implantação

- **VPS Linux, o caminho principal.** Um único comando baixa um script de instalação. Ele instala um build autocontido, cria o serviço, configura o HTTPS quando um domínio é informado e imprime o endereço de configuração e o bloco de configuração para o servidor de jogo. Comandos de atualização e de backup estão incluídos.
- **Azure.** Um guia para o App Service com contêiner. Esse serviço precisa rodar uma única instância, e o SQL Server é recomendado nele porque o disco persistente é um compartilhamento de rede.
- **Docker.** Uma imagem para quem preferir.

## Migração de dados

Um programa de console separado, fora deste repositório, move os dados de produção uma única vez:

- Ele lê os arquivos brutos de estatísticas do armazenamento antigo e envia cada um pelo mesmo endpoint que o plugin usa, então tudo é recalculado pelo caminho normal.
- As punições antigas eram apagadas a cada troca de temporada e não voltam, então o histórico recalculado pode diferir um pouco das listas antigas.
- Presentes de skins, compras da temporada atual, preços e banimentos do chat são gravados no novo banco de dados.

## Fases

1. **Fundação.** Repositório, integração contínua, banco de dados, configurações e módulos, login com a Steam, assistente de primeira execução, cadastro de servidores de jogo com o sinal de conexão e um script de instalação mínimo.
2. **Estatísticas e ranking.** Envio, montagem de partidas, pontuação, punições, temporadas e regras de modos de jogo.
3. **Site.** As páginas existentes sobre os novos serviços, com filtros de temporada e de servidor.
4. **Servidor ao vivo.** Estado ao vivo por servidor, seletor de servidor, chat do site, fila de comandos e banimentos do chat.
5. **Loja de skins de armas.** Armazenamento em banco de dados, importação do catálogo e reembolso de temporada.
6. **Plugins.** Os cinco plugins, um único endereço e token, correções no plugin de envio, releases compiladas.
7. **Ranking da comunidade.** Página de votação, rating e limites.
8. **Implantação e tutoriais.** Script de instalação completo, guias de Azure e Docker, tutoriais passo a passo.
9. **Migração.** Importação dos dados de produção, troca do site de produção, arquivamento dos repositórios antigos.

## Perguntas em aberto

- **Licença.** Nenhuma escolhida ainda. Os plugins compilam contra os includes do SourceMod, que são GPL-3.0.
- **Páginas disponíveis no jogo.** Apenas o ranking, ou também detalhes do jogador, últimas partidas e a loja de skins.
- **MySQL.** Depende de haver um provedor do Entity Framework Core disponível para a versão do .NET em uso.
- **HTTPS dentro do jogo.** Ainda não medido. Veja [in-game-browser.pt.md](in-game-browser.pt.md#ainda-não-medido).
