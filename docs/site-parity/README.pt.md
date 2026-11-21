# Paridade do site

🌐 [English](README.md) | **Português** | [Español](README.es.md)

Este projeto substitui um site que já existe, o [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) (em produção em [l4d2.com.br](https://l4d2.com.br/)). A regra para as páginas é rígida: tudo o que um visitante vê no site antigo precisa estar no novo, com o mesmo visual. Por isso, cada página foi portada a partir da view antiga, e não redesenhada, e conferida item por item.

## Conteúdo

- [Os checklists](#os-checklists)
- [Como ler](#como-ler)
- [O que não foi portado](#o-que-não-foi-portado)
- [O que mudou de propósito](#o-que-mudou-de-propósito)
- [Ao alterar uma página](#ao-alterar-uma-página)

## Os checklists

Cada checklist lista todos os itens das telas antigas de uma área: colunas, campos, rótulos, tooltips, ícones, botões, links, gráficos, modais, filtros, scripts, regras para celular, metadados da página, endereços e endpoints da API.

| Checklist | Telas | Prontos | Adaptados | Não portados |
|---|---|---|---|---|
| [site.pt.md](site.pt.md) | Layout, ranking, histórico, patentes, Anti-Cheat, configurações do Anti-Cheat | 104 | 37 | 1 |
| [players.pt.md](players.pt.md) | Lista de jogadores, página do jogador, comparação, jogador fora do ranking | 150 | 23 | 0 |
| [matches.pt.md](matches.pt.md) | Últimos jogos, detalhes da partida, estatísticas do mapa | 128 | 18 | 1 |
| [server.pt.md](server.pt.md) | Servidor ao vivo, chat, eventos do jogo, banimentos de chat | 151 | 28 | 1 |
| [skins.pt.md](skins.pt.md) | Loja de skins, presentes, preços, compras | 146 | 11 | 0 |
| [admin.pt.md](admin.pt.md) | Páginas que só existem aqui: assistente, administração, ranking da comunidade | 182 | 12 | 1 |

Os cinco primeiros comparam o hub com o site antigo. O último compara as páginas que são novas aqui com a primeira versão delas, que usava outro visual, para que nada se perdesse na conversão.

## Como ler

Toda linha tem uma situação.

| Situação | Significado |
|---|---|
| `done` | Presente aqui com o mesmo conteúdo e visual. |
| `adapted` | Presente, mas alterado pelo motivo indicado nas observações. |
| `not ported` | Não pode existir aqui, pelo motivo indicado nas observações. |

A coluna que diz onde o item está no site antigo traz um arquivo e uma linha do repositório antigo.

## O que não foi portado

Três itens do site antigo, cada um substituído pela forma como esta aplicação funciona:

- **`POST /api/cache/clear`.** O site antigo guardava em cache o que lia de uma API separada. Aqui o site e a API são uma única aplicação, e um envio atualiza o ranking sozinho.
- **O cache de uma hora da lista de partidas.** A lista é lida do banco de dados da própria aplicação.
- **`GET /api/servers` redirecionando para `/api/server`.** Aqui `/api/servers` é a lista pública de servidores de jogo.

## O que mudou de propósito

A maioria das linhas `adapted` cai em poucos grupos:

- **Valores fixos que viraram configurações.** O site antigo pertencia a uma única comunidade, então o nome, a logo, os links do rodapé, o ID do analytics, as palavras-chave, os endereços do Anti-Cheat e os links de contato estavam escritos no código. Aqui é o administrador quem define tudo isso.
- **Temporadas de qualquer duração.** Os textos e contadores que pressupunham períodos de dois meses seguem a temporada configurada, e dá para navegar pelas temporadas passadas.
- **Vários servidores de jogo.** Com um servidor, as páginas ficam como antes. Com mais de um, aparecem um seletor de servidor e o nome do servidor.
- **Módulos.** Um bloco que pertence a um módulo (loja de skins, servidor ao vivo, chat do site, comandos remotos, Anti-Cheat) só aparece enquanto esse módulo está ligado.
- **Uma única aplicação.** Os endereços que apontavam para uma API separada apontam para esta. Os endereços antigos redirecionam para os novos.
- **Formulários e scripts.** As requisições que alteram dados levam um token antiforgery, inclusive a de sair.

## Ao alterar uma página

- Mantenha o item do checklist verdadeiro: se você alterar algo que uma linha descreve, atualize a linha.
- Uma página nova segue o mesmo visual: o layout, os temas e os padrões das páginas ao redor dela.
- Não remova um campo, uma coluna ou uma informação de uma página que veio do site antigo.
