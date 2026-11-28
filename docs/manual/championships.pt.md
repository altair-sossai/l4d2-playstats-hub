# Campeonatos

🌐 [English](championships.md) | **Português** | [Español](championships.es.md)

O módulo Campeonatos organiza campeonatos disputados nos servidores de jogo da comunidade: os participantes, as equipes e como elas são formadas, as fases (pontos corridos, com grupos, e mata-mata), as campanhas de cada jogo, as datas, os resultados e as estatísticas. Ele vem desligado; ligue-o em [Administração > Módulos](administration.pt.md#módulos).

[Voltar ao manual](README.pt.md)

## Conteúdo

- [Para jogadores](#para-jogadores)
  - [A página de campeonatos](#a-página-de-campeonatos)
  - [A página de um campeonato](#a-página-de-um-campeonato)
  - [Pedindo para participar](#pedindo-para-participar)
  - [A página de uma série](#a-página-de-uma-série)
  - [A área do capitão](#a-área-do-capitão)
- [Para administradores](#para-administradores)
  - [Criando um campeonato](#criando-um-campeonato)
  - [Configurações](#configurações)
  - [Regras](#regras)
  - [Participantes](#participantes)
  - [Equipes](#equipes)
  - [Draft](#draft)
  - [Fases](#fases)
  - [Jogos e resultados](#jogos-e-resultados)
  - [Histórico](#histórico)
- [Como as regras funcionam](#como-as-regras-funcionam)

## Para jogadores

### A página de campeonatos

**Campeonatos** no menu mostra cada campeonato em um cartão com o banner, o logo, o status, as equipes, os jogadores e a data de início: os em andamento no topo, em cartões maiores, e abaixo o histórico dos finalizados e cancelados, com o campeão de cada um. Um campeonato sem banner ganha um fundo colorido, e um sem logo mostra as iniciais. Um campeonato em rascunho não aparece: só os administradores o veem.

### A página de um campeonato

Cada campeonato tem seu endereço, como `/championships/copa-2027`. Toda página do campeonato começa com o banner, o logo, o nome, o status e as datas, seguidos das abas.

A primeira aba, **Visão geral**, é a página do campeonato em si, lida de cima para baixo:

1. **Sobre**: a descrição e os dados principais: as datas, o número de equipes e de participantes (com as vagas, quando há limite), o tamanho de cada jogo, como 4 contra 4, a formação das equipes e o modo de jogo.
2. **Inscrições**, quando o campeonato aceita pedidos para participar: veja [Pedindo para participar](#pedindo-para-participar).
3. **Onde assistir**: botões para a transmissão e os vídeos, quando a administração informou os links.
4. **Jogos**: o próximo jogo, com uma contagem regressiva até a data e os botões **Página da série** e **Adicionar à agenda**, os outros jogos marcados e os últimos resultados. **Todos os jogos** abre a aba Jogos.
5. **Equipes**: cada equipe com o capitão e o vice. **Elencos** abre a aba Equipes.
6. **Participantes**: a foto e o nome de cada jogador do campeonato, cada um com o link para a página do jogador.
7. **Formato**: cada fase em ordem, com o tipo, os grupos, a melhor de e a regra das campanhas, se está finalizada, em andamento, a seguir ou depois, e quantas séries foram jogadas.
8. **Pódio**: o campeão, o vice e o terceiro lugar, quando o campeonato terminou.
9. **Galeria**: as fotos e artes do campeonato; um clique abre cada uma em tamanho grande.

Uma seção sem nada para mostrar fica de fora. As outras abas vão mais a fundo:

| Aba | O que mostra |
|---|---|
| Regras | As regras escritas pela administração, no idioma da página quando há tradução. |
| Equipes | Cada equipe com bandeira, capitão, vice e jogadores, com as horas de Left 4 Dead 2 de cada um. |
| Draft | A ordem dos capitães e cada escolha, quando as equipes foram formadas por draft, roleta ou potes. |
| Classificação | A tabela de cada fase de pontos corridos e de cada grupo, com as equipes que avançam para a próxima fase marcadas em verde. |
| Chave | A chave de cada fase de mata-mata, rodada a rodada, com a disputa de terceiro lugar quando existe. |
| Jogos | Todas as séries por fase e rodada, com a data, o prazo e o resultado. |
| Estatísticas | As séries e os jogos vencidos por cada equipe, e os números de cada jogador nos jogos ligados a partidas do servidor: MVPs, dano SI, comuns, skeets, levels, crowns, dano no Tank e mortes. |

Uma aba só aparece quando tem algo a mostrar.

### Pedindo para participar

Quando a administração deixa os jogadores pedirem para participar, a página do campeonato tem uma seção **Inscrições**, e o topo da página um botão **Pedir para participar**, enquanto o status é Participantes e há vagas. Entre com a Steam, escreva uma mensagem para a administração se quiser (como a sua experiência ou quando você pode jogar) e clique em **Pedir para participar**. A seção passa a dizer que o seu pedido está aguardando; **Cancelar o pedido** o retira, e você pode pedir de novo depois.

A administração aprova ou recusa cada pedido. Depois de aprovado você é participante e a seção diz isso; um pedido recusado aparece como recusado, e você não pode pedir de novo naquele campeonato. A seção também mostra quantas vagas restam, e quando não há nenhuma ela avisa.

### A página de uma série

Uma série é o confronto de duas equipes: um jogo (melhor de 1) ou mais (melhor de 3, melhor de 5). A página mostra o placar em jogos, o status, a data com os botões **Adicionar ao Google Agenda** e **Baixar .ics**, o prazo, os vetos e cada jogo com a campanha, quem a escolheu, o placar e o link para a partida do servidor.

Um jogo decidido por penalidade mostra os pontos descontados. No mata-mata, um jogo que não é mais necessário, como o terceiro depois de um 2 a 0, aparece como **Não necessário**. Um jogo sendo disputado no servidor aparece como **Em andamento**, com o placar dos mapas jogados até agora.

### A área do capitão

As páginas de um campeonato só mostram informações. O que um capitão ou um vice-capitão faz pela equipe fica numa página separada, a **área do capitão**, em `/championships/<endereço>/captain`. Entre com a Steam: quando você lidera uma equipe de um campeonato que não é rascunho nem terminou, um ícone de torre aparece ao lado da sua foto no topo de todas as páginas, com um número vermelho quando sua equipe tem algo a fazer, e as páginas do campeonato mostram **Abrir a área do capitão**.

A área do capitão começa com a lista do que sua equipe tem a fazer, cada item com um link para a aba dele, e tem estas abas:

| Aba | O que você faz nela |
|---|---|
| Equipe | **Alterar nome, sigla ou bandeira** envia um novo nome, sigla ou bandeira (uma imagem quadrada, de preferência 1024 × 1024 pixels, PNG, JPG, WEBP ou GIF de até 2 MB) para aprovação. Até a administração aprovar, a página avisa que está aguardando e os outros jogadores continuam vendo os atuais; **Retirar a alteração** desfaz o envio. Quando a administração recusa, o motivo aparece aqui. **Remover a bandeira** tira a bandeira na hora. A tabela lista os jogadores da equipe, e o capitão torna um deles o vice-capitão ou tira o vice-capitão. |
| Draft | Num draft em serpentina ou em linha, a ordem dos capitães, as escolhas feitas e os jogadores sem equipe. Quando é a vez da sua equipe, **Escolher** coloca o jogador na sua equipe e a próxima equipe escolhe. Não há limite de tempo e nada acontece ao vivo: cada capitão escolhe quando pode, no mesmo dia ou no seguinte. |
| Séries | Cada série da sua equipe, com a data e o status. **Datas** lista as propostas das duas equipes: proponha uma data (sua proposta já sai aceita pela sua equipe) e aceite ou recuse as da outra equipe. Quando as duas equipes aceitam a mesma data, a série fica marcada. Uma data confirmada não pode ser excluída pelas equipes. Cada equipe pode ter no máximo 5 datas esperando resposta, e uma data que já passou só pode ser recusada. **Vetar** (quando a fase tem vetos) veta uma campanha antes do primeiro jogo: ela não é escolhida nem sorteada na série, e o veto não pode ser mudado. **Campanhas** (quando as equipes escolhem) escolhe a campanha dos jogos da vez da sua equipe: a melhor cabeça de chave o primeiro jogo, a outra equipe o segundo, e assim por diante; o desempate é sorteado só quando é necessário. |

O vice-capitão pode fazer tudo o que o capitão faz, menos escolher o vice-capitão. Os horários estão no fuso do site, mostrado na aba Séries. Os resultados sempre vêm das partidas do servidor: os capitães não digitam nem mudam placares.

## Para administradores

**Administração > Campeonatos** lista todos os campeonatos, inclusive os rascunhos, numa tabela com status, equipes, participantes e campeão. **Adicionar** abre uma janela para criar um; **Editar** abre a página do campeonato e **Excluir** apaga o campeonato com as equipes, fases, jogos e histórico (as partidas do servidor não são alteradas).

A página de um campeonato mostra os números no topo (participantes, equipes, fases montadas e séries terminadas) e as abas Configurações, Regras, Participantes, Equipes, Draft (quando a formação das equipes não é manual), Fases, Jogos e Histórico. Cada aba começa explicando para que serve e com os botões das suas ações; toda alteração é feita numa janela, e o endereço guarda a aba, então a página recarregada depois de salvar abre na mesma aba.

### Criando um campeonato

Na janela **Adicionar**, digite o nome e escolha o formato. O formato só cria as primeiras fases; tudo pode ser mudado na aba Fases até cada fase ser montada.

| Formato | Fases |
|---|---|
| Pontos corridos | Todas as equipes se enfrentam uma vez, em melhor de 1, com uma campanha sorteada para cada rodada. |
| Pontos corridos + mata-mata | Pontos corridos, e os quatro melhores disputam um mata-mata em melhor de 3, com as equipes escolhendo as campanhas. |
| Mata-mata | Só um mata-mata em melhor de 3, com a chave sorteada. |
| Grupos + mata-mata | Dois grupos, e os dois melhores de cada grupo disputam um mata-mata em melhor de 3. |

Um campeonato novo é um rascunho, visto só pelos administradores, e a página dele abre na aba Configurações.

### Configurações

Um único formulário, salvo com o botão **Salvar** no rodapé:

- **Campeonato**: nome, endereço, descrição curta, datas de início e fim, modo de jogo (como ZoneMod v2.9.1b) e os links da transmissão e dos vídeos.
- **Status**: Rascunho, Participantes, Formação das equipes, Em andamento, Finalizado ou Cancelado, com o motivo do cancelamento. Montar a primeira fase coloca o campeonato em andamento. Marque como finalizado depois da final: o histórico passa a mostrar o campeão.
- **Equipes e jogadores**: jogadores por jogo, jogadores por equipe (o máximo de jogadores de uma equipe, contando os reservas), vagas (o máximo de participantes), a formação das equipes (manual, roleta, potes, draft em serpentina ou draft em linha) se as equipes podem ter reservas e se os **jogadores podem pedir para participar** (veja [Pedidos para participar](#participantes)).
- **Imagens**: o **banner**, uma imagem larga como 1920 × 600 mostrada no topo de toda página do campeonato e no cartão dele; o **logo**, uma imagem quadrada como 512 × 512; e a **galeria**, até 12 fotos e artes mostradas no fim da página. Cada uma é um arquivo PNG, JPG, WEBP ou GIF de até 2 MB. Escolha um arquivo só para trocar: salvar sem arquivo mantém a imagem atual. **Remover o banner**, **Remover o logo** e **Remover** embaixo de cada imagem da galeria apagam a imagem ao salvar. Um arquivo que não é uma dessas imagens, um arquivo acima do limite ou mais de 12 imagens na galeria é recusado, e aí nada do formulário é salvo. Os arquivos ficam na pasta de dados do hub, em `branding/championships`, então vão junto com os backups, e cada mudança é registrada no histórico.
- **Resultados**: se as partidas do servidor são ligadas sozinhas aos jogos, as **Horas de tolerância** (quantas horas antes ou depois da data da série a partida pode começar, 6 por padrão) e **Encerrar depois de (minutos)** (quanto tempo um jogo em andamento espera por novas estatísticas antes de terminar, 60 por padrão). Veja [Jogos e resultados](#jogos-e-resultados).

### Regras

As regras em inglês, português e espanhol, uma abaixo da outra, salvas com o botão **Salvar** no rodapé da aba. Elas são escritas em Markdown: `#` títulos, `-` listas, `**negrito**`, `*itálico*` e `[links](https://...)`. A página mostra o texto do idioma do visitante, ou o em inglês.

### Participantes

A administração adiciona os participantes, ou aprova os pedidos dos jogadores. Com **Jogadores podem pedir para participar** ligado na aba Configurações, o painel **Pedidos para participar** no topo da aba Participantes lista cada pedido aguardando resposta, com o jogador, a mensagem, a data e um contador de quantos estão aguardando. **Aprovar** torna o jogador participante, a não ser que o campeonato esteja cheio, e **Recusar** responde que não; um pedido recusado continua na lista, então ainda pode ser aprovado depois. Cada pedido, aprovação e recusa é registrado no histórico.

**Adicionar** abre uma janela onde você cola IDs da Steam, um por linha, ou usa **Escolher da lista** na janela de busca de jogadores. A tabela, com 20 jogadores por página, mostra as horas de Left 4 Dead 2 de cada jogador, a equipe e o pote; **Editar** muda o pote, usado no sorteio por potes, e **Remover** tira o jogador do campeonato e da equipe.

### Equipes

**Adicionar** cria uma equipe, e **Adicionar várias** cria Equipe A, Equipe B, Equipe C e assim por diante. A tabela mostra o cabeça de chave (1 é a melhor; ordena as equipes quando uma fase é montada pela ordem das equipes), o capitão, o vice e os jogadores de cada equipe. Em cada linha:

- **Editar**: nome, sigla, cabeça de chave, capitão, vice e bandeira. Um capitão ou vice sem equipe entra nesta equipe, e é assim que se escolhem os capitães de um draft. Um nome, sigla ou bandeira alterado aqui aparece na hora.
- **Revisar**, quando o capitão enviou um novo nome, sigla ou bandeira: a janela mostra o que a equipe tem agora ao lado do que foi proposto. **Aprovar** coloca em todas as páginas; **Recusar**, com um motivo opcional, descarta e mostra o motivo ao capitão. Aprove só nomes e imagens que não sejam ofensivos nem sensíveis: nada enviado por um capitão aparece antes de ser aprovado, e a imagem aguardando aprovação só é vista pelos administradores e pelos líderes da equipe.
- **Jogadores**: os jogadores da equipe, com **Remover**, e **Adicionar jogador** com um ID da Steam ou a janela de busca de jogadores.
- **Excluir**, enquanto a equipe não está em nenhuma fase.

A aba avisa quando há equipes aguardando aprovação. Um jogador só pode estar em uma equipe, e uma equipe não pode passar do limite de jogadores.

### Draft

Os botões no topo seguem o método escolhido nas configurações, e **Desfazer a última escolha** e **Recomeçar o draft** desfazem as escolhas. Cada escolha aparece na página pública do Draft.

| Método | Como funciona |
|---|---|
| Manual | Não há aba Draft: a administração coloca cada jogador em uma equipe na aba Equipes. |
| Roleta | **Girar a roleta** sorteia uma equipe entre as que têm menos jogadores e um jogador sem equipe. |
| Potes | Defina o pote de cada participante na aba Participantes. **Sortear os potes** dá a cada equipe um jogador de cada pote; o jogador do primeiro pote vira o capitão de uma equipe que ainda não tem. |
| Draft em serpentina | Dê um capitão a cada equipe na aba Equipes e **Sortear a ordem dos capitães**. Cada capitão então escolhe na [área do capitão](#a-área-do-capitão) quando é a vez da equipe, sem limite de tempo. A equipe da vez fica destacada aqui, e **Escolher** escolhe por ela, por exemplo quando um capitão pede no chat. A ordem é A, B, C, D e depois D, C, B, A. |
| Draft em linha | O mesmo, sempre na ordem A, B, C, D. |

### Fases

As fases são disputadas em ordem. **Adicionar** acrescenta uma no fim, com nome e tipo. A tabela mostra o formato, a regra das campanhas e se cada fase está montada, e as linhas têm:

- **Editar**: a janela com todos os campos da fase, abaixo.
- **Montar**: uma janela com as equipes que entram, na ordem delas, e o botão que cria os grupos e rodadas, ou a chave. A primeira fase recebe todas as equipes; as seguintes recebem as que avançam da fase anterior, que precisa estar montada e terminada.
- **Penalidades** (fases de pontos corridos montadas): pontos descontados da classificação, com motivo.
- **Desfazer** (fases montadas): apaga as séries e os resultados da fase para recomeçar, desde que a próxima fase não tenha sido montada.
- **Excluir** (fases não montadas).

| Campo | Significado |
|---|---|
| Tipo | Pontos corridos ou mata-mata. Uma fase de mata-mata precisa ser a última. |
| Ordem das equipes | Pela ordem das equipes (cabeças de chave), por sorteio ou pela classificação da fase anterior. |
| Melhor de | O número de jogos de cada série. O mata-mata tem a sua **Final melhor de**. |
| Grupos e Turnos | Pontos corridos: quantos grupos, e se cada equipe enfrenta as outras uma ou duas vezes. |
| Classificados por grupo | Quantas equipes de cada grupo entram na próxima fase. 0 para todas. Só quando há uma próxima fase. |
| Vitória, Empate, Derrota | Os pontos de uma série na classificação. |
| Desempates | Até três, em ordem: saldo de pontos, pontos conquistados, confronto direto, vitórias. |
| Disputa de terceiro lugar | Mata-mata: os perdedores das semifinais disputam o terceiro lugar. |
| Campanhas | Uma campanha sorteada para cada rodada, cada equipe escolhe uma com o desempate sorteado, ou a administração escolhe. |
| Mínimo de mapas | Só campanhas com pelo menos esta quantidade de mapas podem ser escolhidas ou sorteadas. |
| Vetos e uma vez por equipe | Cada equipe veta uma campanha por série, e uma equipe escolhe cada campanha uma única vez no campeonato. |

Num mata-mata com um número de equipes que não é potência de dois, os melhores cabeças de chave ganham folga para a rodada seguinte. Com seis equipes, por exemplo, o primeiro e o segundo esperam nas semifinais, o terceiro enfrenta o sexto e o quarto enfrenta o quinto. Depois de montada, o tipo, os jogos, os grupos e a regra das campanhas não mudam; os pontos, os desempates, o mínimo de mapas, os vetos e a regra de uma vez por equipe ainda podem mudar.

### Jogos e resultados

A aba Jogos lista todas as séries das fases montadas, 20 por página, com filtro por fase. No topo:

- **Prazos**: a data até a qual todas as séries de uma rodada devem ser disputadas, mostrada nas páginas dos jogos. Deixe vazio para tirar.
- **Campanhas**: sortear de novo, ou limpar, as campanhas dos jogos ainda não disputados de uma fase.

Cada série mostra a rodada, as equipes, o resultado, a data e um botão numerado por jogo: verde quando disputado, apagado quando não é necessário. O botão de um jogo abre a janela do jogo:

- **Campanha**, escolhida pela administração a qualquer momento. Numa fase em que as equipes escolhem, a escolha da administração vale como a escolha da equipe daquela vez.
- **Resultado**: ainda não disputado; **Partida do servidor**, entre as partidas dos últimos 60 dias com jogadores das duas equipes, das mais novas para as mais antigas, que dá o placar e descobre de que lado jogou cada equipe (uma partida só pode ser ligada a um jogo, e as estatísticas do campeonato vêm das partidas ligadas); ou **Digitar o resultado**, para um jogo que não tem partida no servidor.
- **Penalidade** de cada equipe e observações: pontos descontados do placar de uma equipe no jogo. Podem mudar o vencedor.

**Editar** numa série abre a janela dela: a data, o prazo, as observações, os vetos de cada equipe (quando a fase tem vetos), o W.O. (a equipe que vence sem jogar; os jogos não disputados são cancelados) e as propostas de data das equipes, que podem ser excluídas. **Ver** abre a página pública da série.

**Partidas ligadas sozinhas.** Os jogos são disputados nos servidores da comunidade, então os arquivos de estatísticas chegam sozinhos. Quando a opção está ligada nas configurações, cada partida que chega é ligada ao jogo que tem:

- a mesma campanha da partida;
- pelo menos metade dos jogadores de cada equipe na partida, em lados opostos;
- uma série com data, e a partida começou dentro das horas de tolerância antes ou depois dela (um jogo pode começar atrasado ou adiantado).

Enquanto a partida é jogada, o jogo fica **Em andamento**: o botão dele fica amarelo, as páginas públicas mostram o placar dos mapas jogados, e a classificação e a chave não mudam, então nenhuma equipe avança antes de o jogo terminar. O jogo termina, e o vencedor avança, quando:

- todos os mapas da campanha são jogados;
- nenhuma estatística da partida chega pelos minutos definidos nas configurações, para um jogo que acaba antes, como no terceiro mapa;
- ou um administrador clica em **Encerrar o jogo** na janela do jogo.

Um arquivo de estatísticas da partida que chega depois ainda atualiza o placar. Para desfazer uma ligação errada, escolha **Ainda não disputado** na janela do jogo: aquela partida não é ligada de novo àquele jogo. A administração sempre pode ligar uma partida manualmente ou digitar um resultado.

O vencedor de uma série de mata-mata avança sozinho para a próxima série. Um resultado que mudaria um vencedor que já começou a próxima série é recusado: limpe antes os resultados da próxima série.

### Histórico

Todas as alterações do campeonato, com quem as fez e quando, 20 por página.

## Como as regras funcionam

- **Uma série** termina quando uma equipe vence a maioria dos jogos, como dois de três. Nos pontos corridos ela também termina depois de todos os jogos, e um número igual de vitórias é empate.
- **Um jogo** é vencido pela equipe com mais pontos depois da penalidade. Nos pontos corridos, um empate é empate. No mata-mata, um jogo empatado não conta para ninguém, e um novo jogo é criado na série com uma campanha sorteada.
- **A classificação** dá os pontos de vitória, empate e derrota, menos os pontos de penalidade. O saldo de pontos é o total conquistado menos o sofrido menos as penalidades dos jogos. Com pontos iguais, os desempates são usados em ordem, e depois o cabeça de chave.
- **Os sorteios de campanhas** evitam as campanhas já escolhidas na série e os vetos, e só usam campanhas com o mínimo de mapas.
- **Os jogos de um campeonato** são partidas normais do servidor: contam para o ranking como qualquer outra. A página da partida mostra o campeonato e leva à série.
- **Um jogo em andamento** não conta para ninguém até terminar: a classificação, a chave e as próximas séries esperam por ele.
