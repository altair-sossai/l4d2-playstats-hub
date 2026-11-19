# Ranking da comunidade

🌐 [English](community-ranking.md) | **Português** | [Español](community-ranking.es.md)

O ranking da comunidade é um segundo ranking, formado pelo que os jogadores acham uns dos outros. O site mostra dois jogadores para quem vota e pergunta qual é o melhor. Cada resposta faz os dois jogadores subirem ou descerem. Ele não é zerado a cada temporada.

[Voltar ao manual](README.pt.md)

## Conteúdo

- [A página do ranking da comunidade](#a-página-do-ranking-da-comunidade)
- [Votando](#votando)
- [Quem pode votar, e quanto](#quem-pode-votar-e-quanto)
- [Como a dupla é escolhida](#como-a-dupla-é-escolhida)
- [Como a pontuação é calculada](#como-a-pontuação-é-calculada)
- [Para administradores](#para-administradores)

## A página do ranking da comunidade

**Comunidade**, no menu, abre a lista.

![O ranking da comunidade](../images/pt/community.webp)

| Coluna | Significado |
|---|---|
| # | Posição. Os três primeiros têm uma medalha. |
| Patente | A patente do jogador na temporada atual do ranking de estatísticas. |
| Jogador | Avatar e nome. |
| Pontuação | A pontuação formada pelos votos. Todo mundo começa com 1500. |
| Comparações | Quantas vezes o jogador foi avaliado contra outro jogador. |
| Vitórias, Empates, Derrotas | Quantas comparações o jogador ganhou, empatou e perdeu. |

Só aparecem na lista os jogadores com comparações suficientes: 5 por padrão. O mínimo aparece embaixo da tabela. **Votar** abre a página de votação.

## Votando

![A página de votação](../images/pt/community-vote.webp)

A página mostra dois jogadores com seus avatares. Responda com uma destas opções:

| Resposta | O que faz |
|---|---|
| Este é melhor (à esquerda ou à direita) | O jogador escolhido ganha a comparação, o outro perde. |
| Estão no mesmo nível | Um empate. |
| Não conheço este jogador (embaixo de um deles) | Nada muda, e esse jogador não aparece mais para você. |
| Não conheço nenhum dos dois | Nada muda, e nenhum dos dois aparece mais para você. |

Depois de cada resposta a página marca a sua escolha e a próxima dupla entra na tela. No computador, as setas ← e → escolhem o jogador da esquerda ou da direita. **Votos restantes hoje** mostra quantos votos você ainda tem.

## Quem pode votar, e quanto

| Regra | Padrão |
|---|---|
| Você precisa estar logado com o Steam. | |
| Você precisa ter jogado um número mínimo de partidas, contadas em todas as temporadas, ranqueadas ou não. | 10 |
| Você tem um número de votos por dia. O dia segue o fuso horário do site. Só as respostas que avaliam (melhor ou mesmo nível) gastam um voto; "Não conheço" não gasta. | 20 |
| Um jogador só é oferecido para comparação depois de um número mínimo de partidas. | 1 |
| Você nunca é perguntado sobre você mesmo, nem sobre a mesma dupla duas vezes. | |

Quando você não pode votar, a página diz o motivo: você precisa fazer login, precisa de mais partidas (com a sua contagem), já usou todos os votos do dia ou não há mais ninguém para você comparar.

## Como a dupla é escolhida

Quem escolhe a dupla é o site; quem vota não escolhe quem vai avaliar, e uma resposta só é aceita para a dupla que o site ofereceu.

1. Ele prefere jogadores com quem ou contra quem você jogou, porque são os que você conhece melhor. Só quando não sobra nenhuma dupla assim ele oferece outros jogadores.
2. Entre eles, começa por um dos jogadores com menos comparações, para que os jogadores novos recebam uma pontuação.
3. Ele forma a dupla desse jogador com alguém de pontuação próxima.
4. Qual deles aparece à esquerda é aleatório.

## Como a pontuação é calculada

A pontuação usa o sistema Elo, a mesma ideia do xadrez.

- Todo jogador começa com a pontuação inicial, 1500 por padrão.
- Antes de uma comparação, o site calcula a chance de cada jogador ser avaliado como melhor, a partir da diferença entre as pontuações deles. Uma diferença de 400 pontos significa cerca de 10 para 1.
- O vencedor ganha e o perdedor perde o mesmo número de pontos: o fator K (32 por padrão) vezes a diferença entre o resultado (1 para vitória, ½ para empate, 0 para derrota) e a chance esperada.
- Vencer um jogador com pontuação muito mais alta dá muitos pontos; vencer um muito mais fraco dá poucos. Um empate entre jogadores de pontuações diferentes faz o mais fraco subir.
- Respostas "Não conheço" não mudam nada.

A pontuação é permanente. Ela não recomeça com a temporada.

## Para administradores

**Administração > Ranking da comunidade** tem as configurações, a manutenção e a lista de votantes.

![As configurações do ranking da comunidade](../images/pt/admin-community.webp)

| Configuração | O que faz | Padrão |
|---|---|---|
| Mínimo de partidas para votar | Partidas que um jogador precisa ter jogado, em todas as temporadas, para votar. | 10 |
| Votos por dia | Respostas que avaliam que cada votante pode dar por dia. No mínimo 1. | 20 |
| Mínimo de partidas para ser comparado | Partidas que um jogador precisa ter jogado para ser oferecido para comparação. | 1 |
| Mínimo de comparações para aparecer na lista | Comparações que um jogador precisa para aparecer no ranking da comunidade. | 5 |
| Fator K do Elo | Quanto um voto mexe nas pontuações, de 1 a 200. | 32 |
| Pontuação inicial | A pontuação de um jogador que nunca foi comparado. | 1500 |

O fator K e a pontuação inicial valem para os novos votos. **Reconstruir as pontuações** reaplica em ordem todos os votos armazenados com as configurações atuais, para que elas valham para todos os votos.

**Votantes** lista todo mundo que votou, com o número de votos e o último voto. **Remover votos** exclui todos os votos de um votante e reconstrói as pontuações sem eles, por exemplo quando alguém votou de má-fé.

Desligar o módulo do ranking da comunidade em [Módulos](administration.pt.md#módulos) tira **Comunidade** do menu. Os votos são mantidos.
