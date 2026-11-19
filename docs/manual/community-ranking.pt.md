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

**Ver os últimos votos**, acima e abaixo da tabela, abre **Últimos votos**: um cartão por voto com quem votou, os dois jogadores lado a lado, o escolhido em verde (ou os dois em amarelo quando estão no mesmo nível) e quando. As respostas "Não conheço" não aparecem. Clicar em um jogador, ou no ícone de setas ao lado do nome na tabela, mostra só as comparações dele. Um administrador pode esconder quem votou.

No celular a tabela mostra só a posição, o jogador e a pontuação. O **?** ao lado de **Pontuação** abre uma janela que explica cada coluna.

Só aparecem na lista os jogadores com comparações suficientes: 5 por padrão. O jogador também precisa ter jogado o mínimo de partidas para ser comparado, 1 por padrão. Os dois mínimos aparecem embaixo da tabela, e a lista para nos 100 primeiros jogadores por padrão. Embaixo do quadro, **Como funciona o ranking da comunidade?** explica em quatro passos como os votos viram a lista, com os números das configurações atuais. A página de votação tem a mesma explicação, fechada embaixo de um link. **Votar agora**, no quadro acima da tabela, abre a página de votação. O mesmo quadro aparece acima do ranking de estatísticas na página inicial; lá o jogador pode fechá-lo, e ele volta depois de 7 dias.

## Votando

![A página de votação](../images/pt/community-vote.webp)

A página mostra dois jogadores com seus avatares. Responda com uma destas opções:

| Resposta | O que faz |
|---|---|
| Este é melhor (à esquerda ou à direita) | O jogador escolhido ganha a comparação, o outro perde. |
| Estão no mesmo nível | Um empate. |
| Não conheço este jogador (embaixo de um deles) | Nada muda, e esse jogador não aparece mais para você. |
| Não conheço nenhum dos dois | Nada muda, e nenhum dos dois aparece mais para você. |

Embaixo de cada nome o cartão mostra até 4 outros nomes que o jogador já usou nas partidas, do mais usado para o menos usado, para você reconhecer quem trocou de nome. Depois de cada resposta a página marca a sua escolha e a próxima dupla entra na tela. No computador, as setas ← e → escolhem o jogador da esquerda ou da direita, ↑ diz que estão no mesmo nível e ↓ que você não conhece nenhum dos dois. **Votos restantes hoje** mostra quantos votos você ainda tem.

## Quem pode votar, e quanto

| Regra | Padrão |
|---|---|
| Você precisa estar logado com o Steam. | |
| Você precisa ter jogado um número mínimo de partidas, contadas em todas as temporadas, ranqueadas ou não. | 10 |
| Você tem um número de votos por dia. O dia segue o fuso horário do site. Só as respostas que avaliam (melhor ou mesmo nível) gastam um voto; "Não conheço" não gasta. | 20 |
| Um jogador só é oferecido para comparação depois de um número mínimo de partidas. | 1 |
| Um jogador só é oferecido se a última partida dele foi dentro de um número de dias. Com 0, não importa há quanto tempo. | 0 |
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

Cada campo tem um **?** ao lado do nome: passar o mouse mostra uma explicação curta, e os campos que precisam de mais mostram uma janela com os detalhes ao clicar.

| Configuração | O que faz | Padrão |
|---|---|---|
| Mínimo de partidas para votar | Partidas que um jogador precisa ter jogado, em todas as temporadas, para votar. | 10 |
| Votos por dia | Respostas que avaliam que cada votante pode dar por dia. No mínimo 1. | 20 |
| Mínimo de partidas para ser comparado e aparecer na lista | Partidas que um jogador precisa ter jogado, em todas as temporadas, para ser oferecido para comparação e para aparecer no ranking da comunidade. | 1 |
| Máximo de dias desde a última partida para ser comparado | Um jogador cuja última partida é mais antiga que isso deixa de ser oferecido para comparação, mas continua no ranking da comunidade. 0 é sem limite. | 0 |
| Mínimo de comparações para aparecer na lista | Comparações de que um jogador precisa para aparecer no ranking da comunidade. | 5 |
| Mostrar quem votou nos últimos votos | Mostra o nome e o avatar de cada votante na página Últimos votos. Desligado, cada voto mostra só "Um jogador votou". | Ligado |
| Máximo de jogadores no ranking | O ranking da comunidade mostra no máximo esse número de jogadores, da maior pontuação para baixo. 0 mostra todos. | 100 |
| Fator K do Elo | Quanto um voto mexe nas pontuações, de 1 a 200. | 32 |
| Pontuação inicial | A pontuação de um jogador que nunca foi comparado. | 1500 |

O fator K e a pontuação inicial valem para os novos votos. **Reconstruir as pontuações** reaplica em ordem todos os votos armazenados com as configurações atuais, para que elas valham para todos os votos.

**Reiniciar o ranking da comunidade**, num painel vermelho, apaga todos os votos e todas as pontuações, e o ranking da comunidade recomeça do zero; as configurações são mantidas. Uma janela mostra quantos votos e jogadores serão apagados e pede confirmação, já que não dá para desfazer.

**Votantes** lista todo mundo que votou, com o número de votos e o último voto. **Remover votos** exclui todos os votos de um votante e reconstrói as pontuações sem eles, por exemplo quando alguém votou de má-fé.

Desligar o módulo do ranking da comunidade em [Módulos](administration.pt.md#módulos) tira **Comunidade** do menu. Os votos são mantidos.
