# L4D2 PlayStats Hub

🌐 [English](README.md) | **Português** | [Español](README.es.md)

Uma única aplicação web auto-hospedada para servidores competitivos de Left 4 Dead 2: ranking, estatísticas de partidas, visualização do servidor ao vivo, campeonatos, banimentos, loja de skins de armas e um ranking da comunidade, junto com os plugins SourceMod que a alimentam.

> **Status: primeira versão publicada, migração concluída.** A aplicação, os plugins, o instalador para Linux e a imagem Docker estão disponíveis nas [releases](https://github.com/altair-sossai/l4d2-playstats-hub/releases). O site de produção ([l4d2.com.br](https://l4d2.com.br/)) roda nesta aplicação, e o servidor de jogo dele com estes plugins, desde 19 de novembro de 2026, e os [repositórios que ela substituiu](#repositórios-que-este-substituiu) foram arquivados em 22 de novembro de 2026.

## Conteúdo

- [Por que este projeto existe](#por-que-este-projeto-existe)
- [O que ele faz](#o-que-ele-faz)
- [Módulos](#módulos)
- [Um passeio rápido](#um-passeio-rápido)
- [Instalação](#instalação)
- [Como tudo se encaixa](#como-tudo-se-encaixa)
- [O que fica fora deste repositório](#o-que-fica-fora-deste-repositório)
- [Repositórios que este substituiu](#repositórios-que-este-substituiu)
- [Documentação](#documentação)
- [Licença](#licença)

## Por que este projeto existe

O sistema começou pequeno e cresceu até virar vários repositórios: um parser de estatísticas, uma API, um site com loja de skins e um conjunto de plugins para o servidor de jogo. Rodá-lo exigia recursos do Azure, linhas de armazenamento criadas à mão e segredos espalhados por três projetos.

Este repositório reúne a lógica para que qualquer dono de servidor possa rodar o mesmo sistema de ranking e estatísticas com uma aplicação, um arquivo de banco de dados e um comando de instalação.

## O que ele faz

- **Ranking e estatísticas.** Cada partida jogada nos seus servidores vira ranking, com patentes, detalhes das partidas e uma página para cada jogador mostrando resultados, médias e com quem e contra quem ele joga.
- **Temporadas que você configura.** Você escolhe quantos meses dura uma temporada. O ranking recomeça a cada temporada, e as temporadas passadas continuam navegáveis: ranking, partidas e jogadores.
- **Vários servidores de jogo, um único site.** Cada servidor tem a própria chave. Os resultados de um jogador se somam entre todos eles.
- **Modos de jogo que você configura.** Você escolhe quais modos contam para o ranking e quantos mapas formam uma partida ranqueada.
- **Servidor ao vivo.** Placar, jogadores, eventos e chat da partida em andamento, para cada servidor. Jogadores logados podem escrever no chat do jogo pelo site, e os administradores podem enviar comandos ao servidor.
- **Loja de skins de armas.** Os jogadores ganham estrelas com MVPs e as gastam em skins.
- **Ranking da comunidade.** Os jogadores votam em duplas de jogadores, e os votos formam um segundo ranking.
- **Campeonatos.** Participantes que pedem para jogar, equipes formadas por draft, roleta ou potes, fases de pontos corridos e mata-mata, uma área do capitão para datas, vetos e campanhas, e resultados ligados às partidas do servidor.
- **Banimentos.** Os banimentos de todos os servidores de jogo em uma lista pública, com os dados no jogo chegando sozinhos e alertas de contas relacionadas.
- **Anti-Cheat.** A página que oferece o Anti-Cheat e quem precisa usá-lo nos servidores de jogo.
- **Notificações.** Um sino avisa cada jogador do que diz respeito a ele: campeonatos, séries, punições, banimentos, presentes.
- **Páginas para dentro do jogo.** O `!ranking` abre o ranking no navegador do próprio jogo, ou uma versão leve feita para ele, e `!comandos` / `!commands` abrem os comandos do servidor.
- **Módulos.** O administrador liga ou desliga o ranking, o servidor ao vivo, o chat do site, os comandos remotos, a loja de skins, o ranking da comunidade, os campeonatos, os banimentos e o Anti-Cheat. Veja [Módulos](#módulos).
- **Configuração guiada.** Um assistente de primeira configuração e uma página de status que mostra se cada servidor de jogo está conectado e o que está faltando.
- **Três idiomas.** Inglês, português e espanhol.
- **Banco de dados à sua escolha.** SQLite por padrão, sem servidor para instalar. SQL Server, PostgreSQL e MySQL são suportados.

## Módulos

Cada recurso abaixo é um módulo que o administrador liga ou desliga. O [guia dos módulos](docs/modules.pt.md) explica cada um com capturas de tela: para que serve, quem usa e do que precisa.

| Módulo | Para que serve | Padrão |
|---|---|---|
| [Ranking e estatísticas](docs/modules.pt.md#ranking-e-estatísticas) | O ranking, as partidas, os jogadores e as temporadas. | Ligado |
| [Servidor ao vivo](docs/modules.pt.md#servidor-ao-vivo) | A partida em andamento em cada servidor de jogo. | Ligado |
| [Chat do site](docs/modules.pt.md#chat-do-site) | Escrever no chat do jogo pelo site. | Desligado |
| [Comandos remotos](docs/modules.pt.md#comandos-remotos) | Enviar comandos do site para um servidor de jogo. | Desligado |
| [Loja de skins de armas](docs/modules.pt.md#loja-de-skins-de-armas) | Gastar as estrelas dos MVPs em skins de armas. | Desligado |
| [Ranking da comunidade](docs/modules.pt.md#ranking-da-comunidade) | Um segundo ranking, votado pelos jogadores. | Desligado |
| [Campeonatos](docs/modules.pt.md#campeonatos) | Campeonatos com equipes, draft, fases e resultados. | Desligado |
| [Banimentos](docs/modules.pt.md#banimentos) | Os jogadores banidos dos servidores de jogo. | Desligado |
| [Anti-Cheat](docs/modules.pt.md#anti-cheat) | A página do Anti-Cheat e quem precisa usá-lo. | Desligado |

As notificações, a página de comandos, os nomes forçados, a administração e os backups estão [sempre ligados](docs/modules.pt.md#sempre-ligado).

## Um passeio rápido

O ranking da temporada abre o site. Cada jogador tem uma página com os resultados, as estatísticas e as relações dele, e cada partida tem os seus detalhes.

![O ranking da temporada atual](docs/images/pt/ranking.webp)

A página do servidor ao vivo acompanha a partida em andamento em cada servidor de jogo.

![A página do servidor ao vivo](docs/images/pt/server-live.webp)

Na loja de skins de armas, os jogadores gastam as estrelas que ganharam como MVP.

![A loja de skins de armas](docs/images/pt/skins.webp)

Os campeonatos têm as próprias páginas, com as equipes, a classificação, a chave e as datas.

![A página de um campeonato](docs/images/pt/championship.webp)

O [guia dos módulos](docs/modules.pt.md) mostra todos os recursos, e o [manual do usuário](docs/manual/README.pt.md) explica cada página, como os números são calculados e cada configuração, com capturas de tela.

## Instalação

Em uma VPS Linux, um único comando instala o site:

```bash
curl -fsSL https://raw.githubusercontent.com/altair-sossai/l4d2-playstats-hub/main/deploy/install.sh | sudo bash
```

Ele mostra o endereço do site e um código de configuração. Abra o endereço, cole o código, entre com a Steam, e o site guia você pelo resto.

Esse comando baixa a versão publicada mais recente. A aplicação também pode ser [executada a partir do código-fonte](CONTRIBUTING.pt.md#como-rodar).

| Guia | Para |
|---|---|
| [Instalando em uma VPS Linux](docs/install-linux.pt.md) | O jeito recomendado. |
| [Instalando com Docker](docs/install-docker.pt.md) | Se você já usa contêineres. |
| [Instalando no Azure](docs/install-azure.pt.md) | Uma máquina virtual ou o App Service. |
| [Conectando um servidor de jogo](docs/game-server.pt.md) | Os plugins e as configurações deles. |
| [Trocando o site antigo](docs/switch-from-old-site.pt.md) | Substituir uma cópia em funcionamento do site anterior, passo a passo. |
| [Manual do usuário](docs/manual/README.pt.md) | Todos os recursos do site, para administradores e jogadores. |

## Como tudo se encaixa

```text
 servidor de jogo 1 ─┐
 servidor de jogo 2 ─┼─ plugins ──>  L4D2 PlayStats Hub  ──>  visitantes, em um navegador
 servidor de jogo N ─┘               (site + API + jobs)  ──>  jogadores, dentro do jogo
                                             │
                                     um banco de dados
```

Os plugins enviam ao site o arquivo de estatísticas de cada mapa e a partida ao vivo. O site armazena os arquivos, monta as partidas e os rankings a partir deles e serve as páginas.

## O que fica fora deste repositório

| Repositório | Por que fica separado |
|---|---|
| [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | Pacote NuGet que faz o parse dos arquivos de estatísticas. Ele é útil por si só, e este projeto o usa como dependência. |
| [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets) | Modelos, materiais e fotos das skins de armas. Eles passam de 500 MB, então só a lógica da loja de skins fica aqui. |

## Repositórios que este substituiu

A migração aconteceu passo a passo. O l4d2-playstats-api e o l4d2-playstats-web foram arquivados em 22 de novembro de 2026 e apontam para este repositório.

| Repositório | O que é | O que acontece com ele |
|---|---|---|
| [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | API em Azure Functions que armazena os arquivos de estatísticas e calcula partidas, ranking e estrelas. | Substituído por esta aplicação; arquivado em 22 de novembro de 2026. |
| [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | Site com ranking, estatísticas, página do servidor ao vivo e loja de skins de armas. | Substituído por esta aplicação; arquivado em 22 de novembro de 2026. |
| [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Configuração de servidor competitivo com plugins personalizados. | Os plugins do PlayStats dele agora ficam em [plugins/](plugins/README.pt.md). |

## Documentação

| Documento | Conteúdo |
|---|---|
| [docs/manual/](docs/manual/README.pt.md) | O manual do usuário: cada página e configuração do site, com capturas de tela. |
| [docs/modules.pt.md](docs/modules.pt.md) | Para que serve cada módulo, o que faz e do que precisa, com capturas de tela. |
| [docs/install-linux.pt.md](docs/install-linux.pt.md) | Instalação, atualização e backup em uma VPS Linux. |
| [docs/install-docker.pt.md](docs/install-docker.pt.md) | Como rodar a imagem publicada. |
| [docs/install-azure.pt.md](docs/install-azure.pt.md) | Como rodar no Azure. |
| [docs/game-server.pt.md](docs/game-server.pt.md) | Como conectar um servidor de jogo, passo a passo. |
| [docs/switch-from-old-site.pt.md](docs/switch-from-old-site.pt.md) | A mudança do site anterior para este, e como voltar atrás. |
| [docs/databases.pt.md](docs/databases.pt.md) | Como usar SQL Server, PostgreSQL ou MySQL. |
| [plugins/README.pt.md](plugins/README.pt.md) | Todos os plugins, configurações e comandos. |
| [docs/roadmap.pt.md](docs/roadmap.pt.md) | Arquitetura, decisões, o que está pronto e o que está em aberto. |
| [docs/in-game-browser.pt.md](docs/in-game-browser.pt.md) | O que o navegador dentro do jogo suporta. |
| [docs/site-parity](docs/site-parity/README.pt.md) | Cada tela do site que está sendo substituído, item por item, e a situação dela aqui. |
| [CONTRIBUTING.pt.md](CONTRIBUTING.pt.md) | Como o código está organizado e as regras para mudanças. |
| [tools/screenshots](tools/screenshots/README.pt.md) | A ferramenta que tira as capturas de tela da documentação. |
| [tools/e2e](tools/e2e/README.pt.md) | Os testes no navegador das principais páginas, rodados em todo push. |

O inglês é o idioma padrão deste repositório. Todo documento também tem uma versão em português (`.pt.md`) e uma em espanhol (`.es.md`), exceto as propostas em [docs/proposals](docs/proposals), que são rascunhos em português.

## Licença

A aplicação é distribuída sob a [Licença MIT](LICENSE). Qualquer pessoa pode usar, alterar e compartilhar, inclusive comercialmente, desde que o aviso de copyright seja mantido.

Os plugins SourceMod em [plugins/](plugins/) são distribuídos sob a [GPL-3.0](plugins/LICENSE), porque o SourceMod exige isso dos plugins compilados com os seus includes.
