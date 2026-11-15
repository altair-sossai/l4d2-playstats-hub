# L4D2 PlayStats Hub

🌐 [English](README.md) | **Português** | [Español](README.es.md)

Uma única aplicação web auto-hospedada para servidores competitivos de Left 4 Dead 2: ranking, estatísticas de partidas, visualização do servidor ao vivo, loja de skins de armas e um ranking da comunidade, junto com os plugins SourceMod que a alimentam.

> **Status: início do desenvolvimento.** Nada neste repositório pode ser instalado ainda. Os repositórios existentes listados abaixo continuam rodando o site de produção ([l4d2.com.br](https://l4d2.com.br/)) e seguem como a fonte da verdade até a migração terminar.

## Conteúdo

- [Por que este projeto existe](#por-que-este-projeto-existe)
- [O que ele vai incluir](#o-que-ele-vai-incluir)
- [O que fica fora deste repositório](#o-que-fica-fora-deste-repositório)
- [Repositórios que estão sendo consolidados](#repositórios-que-estão-sendo-consolidados)
- [Roadmap](#roadmap)
- [Documentação](#documentação)

## Por que este projeto existe

O sistema começou pequeno e cresceu até virar vários repositórios: um parser de estatísticas, uma API, um site com loja de skins e um conjunto de plugins para o servidor de jogo. Rodá-lo hoje exige recursos do Azure, linhas de armazenamento criadas à mão e segredos espalhados por três projetos.

Este repositório reúne a lógica para que qualquer dono de servidor possa rodar o mesmo sistema de ranking e estatísticas com uma aplicação, um arquivo de banco de dados e um comando de instalação.

A migração acontece passo a passo. Cada funcionalidade só vem para cá quando está pronta, e os repositórios antigos são arquivados no final.

## O que ele vai incluir

- **Uma única aplicação web** (ASP.NET Core) que é o site, a API usada pelos plugins do servidor de jogo, a área administrativa e as tarefas agendadas.
- **Banco de dados relacional via Entity Framework Core.** O SQLite é o padrão, então uma instalação nova não precisa de servidor de banco de dados. SQL Server, PostgreSQL e MySQL estão planejados como opções.
- **Módulos que o administrador liga ou desliga:** ranking e estatísticas, visualização do servidor ao vivo, loja de skins de armas, ranking da comunidade, chat do site.
- **Vários servidores de jogo em um único back-end.** Cada servidor tem a sua própria chave, e as estatísticas de cada jogador se somam entre todos os servidores.
- **Temporadas configuráveis.** O administrador define quantos meses dura uma temporada, e as temporadas passadas continuam navegáveis: ranking, partidas e jogadores.
- **Modos de jogo configuráveis.** O administrador escolhe quais modos contam para o ranking e o tamanho mínimo de uma partida competitiva.
- **Ranking da comunidade.** Os jogadores votam em pares de jogadores, e os votos formam um segundo ranking ao lado do ranking de estatísticas.
- **Assistente de configuração e status dos servidores.** Um guia de primeira execução conduz a configuração, e cada servidor de jogo cadastrado mostra se está conectado e configurado corretamente.
- **Plugins SourceMod** que enviam as estatísticas, transmitem a partida ao vivo, mostram o ranking dentro do jogo e aplicam as skins de armas.
- **Implantação simples.** Um script de instalação para uma VPS Linux, com um guia para o Azure e uma imagem Docker como alternativas.
- **Testes automatizados**, com o back-end como prioridade.

## O que fica fora deste repositório

| Repositório | Por que fica separado |
|---|---|
| [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | Pacote NuGet que faz o parse dos arquivos de estatísticas. Ele é útil por si só, e este projeto o usa como dependência. |
| [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets) | Modelos, materiais e fotos das skins de armas. Eles passam de 500 MB, então só a lógica da loja de skins fica aqui. |

## Repositórios que estão sendo consolidados

| Repositório | O que é hoje | O que acontece com ele |
|---|---|---|
| [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | API em Azure Functions que armazena os arquivos de estatísticas e calcula partidas, ranking e estrelas. | Substituído por esta aplicação e depois arquivado. |
| [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | Site com ranking, estatísticas, página do servidor ao vivo e loja de skins de armas. | Substituído por esta aplicação e depois arquivado. |
| [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Configuração de servidor competitivo com plugins personalizados. | Os plugins abaixo vêm para cá. |

Plugins que saem do `l4d2-zone-server`:

| Plugin | O que faz |
|---|---|
| `l4d2_playstats_sync` | Envia os arquivos de estatísticas gravados pelo `l4d2_playstats`. |
| `l4d2_gameinfo_sync` | Transmite a partida ao vivo: placar, jogadores, eventos e chat. |
| `l4d2_ranking` | Abre o ranking dentro do jogo. |
| `l4d2_show_patent_icon` | Mostra o ícone da patente de cada jogador dentro do jogo. |
| `l4d2_weapon_skins` | Aplica as skins de armas que os jogadores possuem. |

O `l4d2_playstats` em si, que grava os arquivos de estatísticas, vem do [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) e não é copiado para cá.

## Roadmap

1. Fundação: esqueleto da aplicação, banco de dados, configurações e módulos, login com a Steam, assistente de configuração, cadastro de servidores de jogo.
2. Envio de estatísticas, partidas, temporadas e ranking.
3. Páginas do site, com filtros de temporada e de servidor.
4. Visualização do servidor ao vivo para vários servidores.
5. Loja de skins de armas.
6. Plugins do servidor de jogo.
7. Ranking da comunidade.
8. Script de instalação, guias de implantação e tutoriais.
9. Migração de dados e arquivamento dos repositórios antigos.

O plano completo, com todas as decisões tomadas até agora, está em [docs/roadmap.pt.md](docs/roadmap.pt.md).

## Documentação

| Documento | Conteúdo |
|---|---|
| [docs/roadmap.pt.md](docs/roadmap.pt.md) | Arquitetura, decisões, fases e perguntas em aberto. |
| [docs/in-game-browser.pt.md](docs/in-game-browser.pt.md) | O que o navegador embutido no Left 4 Dead 2 suporta e o que isso significa para o front-end. |

O inglês é o idioma padrão deste repositório. Todo documento também tem uma versão em português (`.pt.md`) e uma em espanhol (`.es.md`).
