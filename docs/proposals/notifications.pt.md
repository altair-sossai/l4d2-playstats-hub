# Notificações: estrutura proposta

Módulo separado, chamado pelos outros módulos, que registra avisos para os usuários do site (jogadores, capitães e administradores) e os mostra num sino ao lado da foto, no topo de todas as páginas.

## Como funciona

- **Uma tabela de notificações.** Cada linha tem o público, o texto, os valores do texto, o link, o módulo de origem e a data.
- **Três públicos**, para não criar uma linha por usuário quando o aviso é para muita gente:

| Público | Quem vê | Exemplo |
|---|---|---|
| Um jogador | Só o dono do Steam ID | "Você foi escolhido pela equipe Lobos no draft da Copa." |
| Administradores | Quem é administrador quando abre o sino | "Fulano pediu uma vaga na Copa." |
| Todos | Todo usuário que entra com a Steam | "Tem um campeonato novo que você pode participar: Copa 2027." |

- **Lida e não lida.** Uma segunda tabela guarda quem leu cada notificação, com a data em que leu. Um aviso para todos ou para os administradores aparece como não lido para cada um até que essa pessoa o leia.
- **Idioma.** O texto é gravado em inglês com os valores à parte e traduzido na hora de mostrar, como no histórico dos campeonatos. Cada um lê no idioma em que está navegando.
- **Limpeza.** As notificações com mais de 90 dias são apagadas por um serviço em segundo plano.

## Na tela

- **Sino** ao lado da foto, com o número de não lidas em vermelho. Ao clicar, abre a lista das 8 mais recentes, com as não lidas destacadas. Abaixo da lista ficam **Ver todas** e **Marcar todas como lidas**, que pede confirmação.
- **Clicar numa notificação** marca como lida e abre o link dela.
- **Página `/notifications`** com todas as notificações, paginada no padrão do site.
- **A torre do capitão continua.** Ela mostra o que a equipe ainda precisa fazer; o sino mostra o que aconteceu.

## Para os outros módulos

Um serviço só, com quatro chamadas:

- avisar um jogador;
- avisar vários jogadores;
- avisar os administradores;
- avisar todos.

Cada chamada recebe o módulo, o texto, os valores e o link.

## Avisos ligados nesta entrega

### Campeonatos

**Inscrição e participação**
- Pedido de vaga criado ou cancelado: aviso aos administradores.
- Pedido aprovado ou recusado: aviso ao jogador.
- Campeonato aberto a inscrições: aviso a todos.
- Participante adicionado ou removido pela administração: aviso ao jogador.
- Campeonato em andamento ou terminado: aviso aos participantes.

**Equipes e draft**
- Jogador colocado numa equipe, ou escolhido capitão ou vice: aviso ao jogador.
- Ordem do draft sorteada: aviso aos capitães.
- Vez de escolher: aviso ao capitão e ao vice da equipe da vez.
- Jogador escolhido no draft: aviso ao jogador escolhido.
- Nome, sigla ou bandeira enviados para aprovação: aviso aos administradores.
- Nome, sigla ou bandeira aprovados ou recusados: aviso ao capitão e ao vice.

**Fases, séries e jogos**
- Fase montada: aviso aos jogadores das equipes dela.
- Data proposta: aviso ao capitão e ao vice da outra equipe.
- Data aceita (série marcada): aviso aos jogadores das duas equipes.
- Data recusada: aviso a quem propôs.
- Data ou prazo marcados pela administração: aviso aos jogadores das duas equipes.
- Veto: aviso ao capitão e ao vice da outra equipe.
- Escolha de campanha: aviso ao capitão e ao vice da outra equipe.
- Jogo terminado: aviso aos jogadores das duas equipes.
- Série terminada e W.O.: aviso aos jogadores das duas equipes.

### Outros módulos

- **Administradores:** jogador adicionado como administrador, aviso a ele.
- **Chat do site:** jogador banido ou desbanido, aviso a ele.
- **Ranking da comunidade:** jogador bloqueado ou desbloqueado para votar, aviso a ele.
- **Punições:** punição aplicada ou removida, aviso ao jogador.
- **Nomes forçados:** nome forçado definido ou removido, aviso ao jogador.
- **Skins:**
  - skin recebida de presente, aviso a quem recebeu;
  - compra devolvida, aviso ao comprador.
- **Temporadas:** temporada nova, aviso a todos.
- **Backups:** backup automático com erro, aviso aos administradores.
