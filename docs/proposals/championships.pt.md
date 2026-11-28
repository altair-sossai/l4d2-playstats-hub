# Campeonatos: mapeamento e proposta

Levantamento de tudo o que o sistema de torneios antigo fazia, módulo por módulo, e proposta de como trazer essa lógica para o hub como um módulo novo, **Campeonatos**, com cadastro de campeonatos, fases configuráveis, equipes, sorteios, agendamento e resultados.

Este documento é um rascunho para discussão, em português. Nada é migrado: o módulo começa com o banco vazio. Quando virar recurso do site, a documentação segue a regra do repositório: inglês primeiro, depois português e espanhol.

## Situação

A primeira versão foi implementada; o manual está em [docs/manual/championships.md](../manual/championships.md). Ela difere desta proposta nestes pontos:

| Proposta | Implementado |
|---|---|
| Horários no fuso de quem vê | Horários no fuso do site, como no resto do hub, com o fuso escrito ao lado dos formulários. |
| Trava do elenco | Não há trava: só a administração mexe nos elencos, e cada mudança fica no histórico. |
| W.O., desistência, desclassificação e jogo anulado | W.O. por série. Desistência e desclassificação são dadas como W.O. nas séries restantes; anular um jogo é limpar o resultado. |
| Horário preferido e página "Meus jogos" | Não entraram como tal: as séries da equipe ficam na área do capitão. |
| Estatísticas por fase | Estatísticas do campeonato inteiro. |
| Data por jogo | Data por série: os jogos de uma série são marcados juntos. |

## Conteúdo

- [De onde veio](#de-onde-veio)
- [As edições](#as-edições)
- [Mapeamento módulo a módulo](#mapeamento-módulo-a-módulo)
- [Problemas e limites do sistema antigo](#problemas-e-limites-do-sistema-antigo)
- [O que o hub já oferece](#o-que-o-hub-já-oferece)
- [Proposta para o hub](#proposta-para-o-hub)
- [De/para](#depara)
- [Desligar o site antigo](#desligar-o-site-antigo)
- [Decisões](#decisões)

## De onde veio

| Parte | Pasta | Como funcionava |
|---|---|---|
| API | `torneio-l4d2-back-end` | Azure Functions em C#, dados no Azure Table Storage, integração com a API da Steam e com a API do PlayStats. 88 commits, de setembro de 2021 a julho de 2026. |
| Site | `torneio-l4d2-front-end` | Angular com Ant Design. Site público, área administrativa e uma área para os capitães. 156 commits, de setembro de 2021 a agosto de 2026. |
| Regras | `torneio-l4d2-regras` | O regulamento de cada edição em Markdown (português, inglês e espanhol) e o registro do draft de 2026. 45 commits, de junho de 2023 a agosto de 2026. |

O sistema inteiro servia **um torneio por vez**: cada edição nova apagava ou sobrescrevia os dados da anterior, e o formato de disputa estava escrito no código.

## As edições

| Edição | Formato | Equipes | Formação das equipes | O que mudou no sistema |
|---|---|---|---|---|
| 1ª (2021) | Pontos corridos, todos contra todos, MD1 | — | Manual, pelo administrador | Jogadores, equipes, geração dos confrontos, placar, classificação, penalidades, galeria de imagens. |
| 2ª (2022) | Pontos corridos e depois mata-mata em MD3 | — | Roleta: sorteia uma equipe e um jogador | Senha e área dos capitães, sugestão de datas, cadastro do mata-mata com campanha vetada por equipe e terceira campanha sorteada, árvore do mata-mata. |
| 3ª (2023) | Pontos corridos MD1 e mata-mata MD3 com os 6 melhores (1º e 2º direto na semifinal, 3º×6º e 4º×5º), Vanilla 4 Mod | 8 de 6 jogadores (4 jogam) | Dois jogadores por equipe sorteados pela organização | Sorteio de capitães e de suportes, sorteio das campanhas por rodada só entre campanhas com 4 mapas ou mais, integração com o PlayStats, estatísticas por jogador e por equipe, "Adicionar à agenda", tema escuro. |
| 4ª (2024) | Só mata-mata, MD3, ZoneMod | 8 de 4 jogadores | 8 capitães escolhidos pela organização, ordem sorteada, escolha em serpentina | Apenas ajustes de domínio e do pipeline. O draft foi feito fora do sistema. |
| 2026 | Só mata-mata: semifinais e final em MD3, ZoneMod v2.9.1b | 4 de 4 jogadores | 4 capitães, ordem sorteada, serpentina, sem reservas | Migração para .NET 10 e Angular 22, componente de próximo jogo com contagem regressiva. O draft foi feito por mensagens e registrado em Markdown com prints. |

## Mapeamento módulo a módulo

### 1. Acesso

- **Administradores:** login com e-mail e senha, que devolve um token JWT. Os usuários eram criados direto no armazenamento; não havia tela para isso.
- **Capitães:** o administrador gerava uma senha aleatória de 12 caracteres para cada capitão e a entregava por fora. O capitão entrava com Steam ID e senha (Basic Authentication) e só via a página do próximo confronto.
- As rotas de escrita checavam o token; as de leitura eram públicas.

### 2. Jogadores

- Cadastro por Steam ID, URL do perfil (`/profiles/` ou `/id/`) ou nome personalizado, resolvido pela API da Steam.
- Guardava nome, foto, link do perfil e **horas jogadas de Left 4 Dead 2**.
- **Atualizar todos** relia os dados da Steam de todos os jogadores.
- **Jogadores disponíveis:** os que ainda não estavam em uma equipe.
- Excluir um jogador o tirava também da equipe.

### 3. Equipes

- Código (até 20 caracteres, letras, números e hífen), nome (até 50) e a lista de jogadores.
- A ordem do vínculo definia o papel: o primeiro jogador era o **capitão**, o segundo o **suporte**.
- Um jogador só podia estar em uma equipe.
- A equipe guardava também os totais da classificação (vitórias, empates, derrotas, pontos, penalidades), recalculados a cada placar salvo.

### 4. Formação das equipes

Cada edição usou um método diferente, e o sistema acompanhou só uma parte:

| Método | Edição | No sistema |
|---|---|---|
| Roleta: sorteia uma equipe entre as que têm menos jogadores e um jogador entre os que estão sem equipe, com animação e som, e vincula os dois | 2022 | Sim |
| Sortear capitães: embaralha os jogadores na posição 1 entre as equipes | 2023 | Sim |
| Sortear suportes: o mesmo para a posição 2 | 2023 | Sim |
| Capitães escolhidos pela organização, ordem de escolha sorteada, escolha em serpentina (A, B, C, D e depois D, C, B, A) | 2024 e 2026 | Não. Feito por mensagens e registrado à mão. |

### 5. Campanhas

- Catálogo fixo no código, com as 14 campanhas oficiais: código, nome em português, nome original, **pontuação máxima** e **quantidade de mapas**.
- A pontuação máxima alimentava a barra de progresso do placar.
- Os sorteios só usavam campanhas com 4 mapas ou mais.

### 6. Fase de pontos corridos

- **Gerar confrontos:** tabela de todos contra todos pelo método do círculo, com rodadas; aceitava número ímpar de equipes (uma folga por rodada). Gerar de novo apagava todos os confrontos.
- **Sortear campanhas:** uma campanha diferente para cada rodada, a mesma para todos os jogos da rodada.
- **Limpar campanhas:** tirava a campanha de todos os confrontos.
- **Confronto manual:** criar, editar e excluir um confronto avulso.
- **Status do confronto:** Aguardando, Realizado ou Cancelado. Realizado ou Cancelado exigia data e campanha. Voltar para Aguardando zerava o placar.
- **Placar:** pontos de cada equipe, **penalidade no jogo** (pontos descontados do placar daquele jogo), **penalidade nos pontos gerais** (pontos descontados da classificação), observações e o intervalo de arquivos de estatísticas do jogo.
- **Vencedor:** a equipe com mais pontos depois da penalidade do jogo; pontos iguais eram empate.
- **Classificação:** vitória 3 pontos, empate 1, derrota 0, menos as penalidades nos pontos gerais. Desempate pelo saldo (pontos conquistados menos sofridos menos penalidades).
- **Rodada atual:** a menor rodada com algum jogo aguardando; o site destacava essa rodada.

### 7. Mata-mata

- Cada **série** era um registro com rodada (fase), ordem (posição na chave), as duas equipes e exatamente 3 jogos, cada um com campanha, data, status, placar, penalidades, observações e intervalo de estatísticas.
- **Campanha vetada:** cada equipe vetava uma campanha, que não podia ser escolhida na série.
- **Terceira campanha:** quando as duas equipes e as duas primeiras campanhas estavam definidas, o sistema sorteava a terceira, fora das vetadas e das já escolhidas, entre as de 4 mapas ou mais.
- **Vitória por 2×0:** o terceiro jogo era cancelado sozinho, com a observação "O terceiro jogo não é necessário".
- Um jogo só podia ser salvo como realizado se os anteriores também estivessem.
- **Status da série:** Aguardando, Em andamento ou Finalizada; vencedor com 2 vitórias.
- **Avanço manual:** o administrador cadastrava cada série da fase seguinte e escolhia as equipes à mão. O sistema não sabia que o vencedor de uma série ia para outra.
- **Árvore:** o site desenhava a chave e abria os detalhes da série ao clicar. A árvore aparecia sozinha quando faltavam duas rodadas para o fim dos pontos corridos.

### 8. Agendamento

- **Período do confronto:** o administrador definia início e fim da janela em que o jogo podia acontecer e podia já cadastrar sugestões de data.
- **Sugestões:** o capitão propunha uma data dentro da janela; a proposta já saía aceita pela equipe dele.
- **Respostas:** o capitão adversário aceitava ou recusava.
- **Excluir:** cada capitão só excluía as próprias sugestões, nunca as do administrador.
- **Marcação automática:** quando as duas equipes aceitavam uma sugestão, o jogo era marcado na data mais cedo aceita pelas duas.
- Nada disso mudava depois que o jogo saía de Aguardando.
- **Página do capitão:** os próximos confrontos da equipe dele, a janela, as sugestões com a resposta de cada lado e os botões de sugerir, responder e excluir.

### 9. Integração com o PlayStats

- **Importar do PlayStats** (no cadastro do jogo): pegava a última partida do servidor, marcava o jogo como realizado, preenchia o intervalo de arquivos de estatísticas e o placar, descobrindo de que lado estava cada equipe pelos Steam IDs dos jogadores.
- O intervalo de arquivos era a única ligação entre o jogo do torneio e a partida; as estatísticas eram buscadas de novo a cada consulta.

### 10. Estatísticas

- **Por jogador:** soma, em todos os jogos do torneio (pontos corridos e mata-mata), de 20 números do PlayStats: mortes, incapacitações, dano recebido, comuns, especiais mortos, dano em especiais, dano no tank, pedradas, dano na witch, skeets, levels, crowns, fogo amigo, dano total, dano como tank, dano de spit, dano de pounce, pontos de MVP e de LVP.
- **Por equipe:** cada jogo realizado com o adversário e os pontos de MVP e de LVP de cada jogador da equipe.

### 11. Site público

- **Início:** rodadas com a atual destacada, classificação, árvore do mata-mata, equipes com jogadores e horas jogadas, e a lista de jogos marcados.
- **Próximo jogo:** cartão com as equipes, campanha, contagem ("Hoje", "Amanhã", "Em 3 dias", "Em 2 semanas", "Em andamento"), jogadores e link da transmissão na Twitch.
- **Adicionar à agenda:** link do Google Agenda com título, descrição, campanha e 1h30 de duração.
- **Estatísticas por jogador** e **por equipe**.
- **Regras:** link para o regulamento no GitHub (antes, um PDF).
- Tema escuro, links do YouTube e da Twitch.

### 12. Recursos que saíram

- **Galeria de imagens** (2021): listava imagens do armazenamento; removida em 2022.
- **Cache em memória** das listas públicas (2022).

## Problemas e limites do sistema antigo

- **Um torneio por vez.** Não havia entidade de torneio; cada edição apagava a anterior, e o histórico só existe no regulamento e nos prints.
- **Formato escrito no código.** Pontos corridos e mata-mata eram dois módulos separados com tabelas diferentes; a série tinha sempre 3 jogos em colunas fixas (`Confronto01`, `Confronto02`, `Confronto03`); não dava para fazer MD1, MD5 ou uma final diferente.
- **Avanço entre fases manual.** Nem a passagem dos classificados dos pontos corridos para o mata-mata nem a dos vencedores de uma fase do mata-mata para a próxima eram automáticas.
- **Draft fora do sistema.** O formato usado nas duas últimas edições (capitães, ordem sorteada, serpentina) não tinha suporte.
- **Regra "cada campanha uma vez por equipe no torneio" não verificada.** O sistema só impedia repetir campanha dentro da mesma série.
- **Escolha das campanhas pelo administrador.** As equipes escolhiam por mensagem e o administrador digitava.
- **Jogo cancelado contava na tabela.** A classificação só ignorava os jogos aguardando, então um cancelado entrava como empate (ou com o placar digitado). Não havia W.O. como resultado.
- **Capitão só por posição.** O capitão era o primeiro da lista; trocar a ordem trocava o capitão. Só ele podia agendar.
- **Senha dos capitães entregue por fora**, com Basic Authentication.
- **Fuso horário:** datas sem fuso claro, com várias correções ao longo do tempo.
- **Placar recalculado por inteiro** a cada jogo salvo, gravando em todas as equipes.
- **Ligação frágil com as estatísticas:** pelo nome do primeiro e do último arquivo, consultando a API do PlayStats a cada visita.
- **Sem histórico de alterações:** não se sabe quem mudou um placar ou aplicou uma penalidade.
- **Um único idioma** no site, embora o regulamento existisse em três.

## O que o hub já oferece

O hub resolve sozinho boa parte do que o sistema antigo fazia à mão:

| Necessidade | No hub |
|---|---|
| Login | Login pela Steam para todos os visitantes; administradores pela lista de administradores. Capitães e jogadores entram com a própria conta, sem senha gerada. |
| Jogadores | Perfil de cada Steam ID que já jogou no servidor, com nome, avatar, horas de L4D2 e dados da Steam, e a janela de busca de jogadores. |
| Partidas | Cada partida já é um registro com campanha, configuração, placar das duas equipes, jogadores de cada lado, servidor e horários. |
| Campanhas | Catálogo de campanhas e mapas editável, com modos de jogo e configurações. |
| Estatísticas | Todos os números por jogador e por partida, já no banco. |
| Idiomas e temas | Inglês, português e espanhol; tema claro e escuro. |
| Módulos | Cada recurso pode ser ligado e desligado pelo administrador. |

## Proposta para o hub

Um módulo **Campeonatos**, desligado por padrão, em que tudo o que era fixo no código vira cadastro. O nome aparece como *Championships* em inglês e *Campeonatos* em português e espanhol; no código, o módulo é `Championships`.

### Estrutura

```text
Campeonato
├── Equipes (com elenco e capitão)
├── Draft (opcional)
└── Fases, em ordem
    ├── Pontos corridos → grupos → rodadas → séries → jogos
    └── Mata-mata       → rodadas da chave → séries → jogos
```

- **Campeonato:** nome, slug, edição, descrição, regulamento em Markdown por idioma, banner, links (transmissão, YouTube, grupo), datas previstas, organizadores, configuração de jogo (por exemplo ZoneMod v2.9.1b), visibilidade e status.
- **Fase:** tipo, ordem, nome ("Fase de grupos", "Semifinal"), formato das séries, regras de campanha, prazos e regra de classificação para a próxima fase.
- **Série:** duas equipes que se enfrentam em MD1, MD3 ou MD5. Substitui tanto o "confronto" dos pontos corridos quanto o "playoff".
- **Jogo:** uma campanha dentro da série, ligado a uma partida do hub.

### Ciclo de vida do campeonato

| Status | O que acontece | O que fica travado |
|---|---|---|
| Rascunho | Só os organizadores veem. Configuram fases e regras. | — |
| Participantes | A organização cadastra os jogadores; a página pública já mostra a lista. | — |
| Formação das equipes | Draft ou cadastro manual das equipes. | Lista de participantes |
| Em andamento | Chaves e tabelas geradas, jogos sendo agendados e disputados. | Formato das fases já começadas e elencos (se a regra travar) |
| Finalizado | Campeão, vice e premiação registrados; tudo vira histórico. | Tudo, salvo correção explícita do administrador |
| Cancelado | Fica no histórico com o motivo. | Tudo |

### Participantes

- **Só a organização cadastra.** O administrador adiciona cada jogador pela busca de jogadores do hub ou por Steam ID; não há página de inscrição.
- A lista mostra a ficha que o hub já tem (horas, partidas, posição no ranking) para ajudar a escolher os capitães.
- Limite de vagas por campeonato (16 em 2026, 32 em 2024).

### Equipes

- Nome, sigla, logo opcional, **capitão e vice-capitão como campos próprios** (não pela posição na lista) e elenco. O vice é indicado pelo capitão ou pelo administrador.
- Tamanho mínimo e máximo do elenco e número de jogadores por jogo (em 2023 eram 6 no elenco e 4 em campo).
- **Reservas:** permitidos ou não, por campeonato (2026: sem reservas).
- **Trava do elenco** a partir de uma data ou do início do campeonato; trocas depois disso só pelo administrador, com registro.
- Um jogador em uma só equipe por campeonato.

### Formação das equipes

Os métodos das edições viram opções do mesmo módulo:

| Método | Como funciona |
|---|---|
| Manual | O administrador monta as equipes. |
| Roleta | O método de 2022, com a animação: sorteia equipe e jogador. |
| Potes | O método de 2023: o administrador divide os jogadores em potes (capitães, suportes...) e o sistema sorteia um de cada pote para cada equipe. |
| Draft em serpentina | O método de 2024 e 2026: o administrador escolhe os capitães, o sistema sorteia a ordem e mostra de quem é a vez (A-B-C-D, D-C-B-A). O **administrador conduz**: registra cada escolha que o capitão faz na call ou no chat, entre os participantes ainda livres, e pode desfazer a última. Uma página pública mostra o draft ao vivo. |
| Draft em linha | Mesma coisa, sem inverter a ordem. |

Tudo o que o draft de 2026 registrou à mão (ordem do sorteio, cada escolha, a equipe final) fica gravado e aparece na página do campeonato.

### Fases

**Pontos corridos**

- Turno único ou turno e returno.
- Um grupo ou vários, com sorteio dos grupos (com cabeças de chave opcionais).
- Geração das rodadas pelo método do círculo, com folga para número ímpar.
- Pontos por vitória, empate e derrota configuráveis (padrão 3, 1 e 0).
- **Critérios de desempate em ordem**, escolhidos pelo administrador: pontos gerais, saldo de pontos, pontos conquistados, confronto direto, vitórias, sorteio.
- Destaque da rodada atual, como hoje.

**Mata-mata**

- Eliminação simples (a dupla eliminação pode vir depois).
- Chave gerada com **byes**, como em 2023, quando o número de equipes não é potência de 2.
- **Semeadura:** pela classificação da fase anterior (1º×8º, 2º×7º...), por sorteio (2024 e 2026) ou manual.
- **O vencedor avança sozinho** para a série seguinte da chave.
- **Jogo empatado:** no mata-mata, um jogo empatado em pontos não conta para nenhuma equipe, e o sistema cria sozinho um novo jogo na série, com campanha sorteada pelas mesmas regras da campanha de desempate. Nos pontos corridos, o empate continua valendo 1 ponto.
- Disputa de terceiro lugar opcional.
- Formato por rodada: semifinais em MD3 e final em MD5, por exemplo.

**Passagem entre fases**

- Regra de classificação: os N melhores de cada grupo, os N melhores no geral, ou todos.
- Quem entra direto em qual rodada (em 2023, 1º e 2º direto na semifinal).
- Ao fim da fase, o sistema mostra quem passou e monta a próxima fase depois que o administrador confirma.

### Campanhas

Regras configuráveis por fase:

| Regra | Usada em |
|---|---|
| Sorteio de uma campanha por rodada, igual para todos | Pontos corridos de 2021 a 2023 |
| Cada equipe escolhe uma campanha da série | Mata-mata de 2022 a 2026 |
| Cada equipe veta uma campanha | Mata-mata de 2022 |
| Campanha de desempate sorteada, fora das escolhidas e vetadas | Mata-mata de 2022 a 2026 |
| Cada campanha só pode ser escolhida uma vez por equipe no campeonato; a sorteada não conta | 2024 e 2026 (antes só no papel; agora verificada) |
| Só campanhas oficiais, ou com pelo menos N mapas | 2023 em diante |

- **Ordem da escolha:** a equipe melhor semeada (a de cima na chave) escolhe a primeira campanha e a outra escolhe a segunda. Quando a chave é sorteada, a ordem do sorteio define quem fica em cima.
- **Escolha e veto pelos capitães:** o capitão ou o vice escolhe a campanha da equipe na página da série, dentro de um prazo; o sistema só oferece as permitidas. O administrador continua podendo escolher por eles.
- O sorteio fica registrado com data e quem acionou, e aparece para todos.
- O jogo decisivo que não é necessário (2×0 numa MD3) é dispensado sozinho, como hoje.

### Agendamento

O fluxo de 2022 continua, com o login da Steam no lugar da senha:

- **Prazo por rodada ou por série**, que pode ser estourado se as duas equipes concordarem (regra de 2026).
- O **capitão ou o vice** propõe datas pela equipe; o vice evita que tudo pare quando o capitão some.
- O capitão ou o vice da outra equipe aceita ou recusa; quando as duas aceitam, o jogo fica marcado.
- **Remarcação** só com as duas equipes de acordo, ou pelo administrador.
- Horários mostrados no fuso de quem vê, gravados em UTC.
- Horário preferido opcional (em 2023 e 2024: sextas, sábados e domingos depois das 19h), como aviso.
- Página **Meus jogos** para o jogador logado, com os jogos da equipe e o que está pendente (data, campanha).
- **Adicionar à agenda:** Google Agenda e arquivo `.ics`.

### Resultados

- O jogo do campeonato aponta para uma **partida do hub**. O administrador escolhe a partida numa lista das partidas recentes em que os jogadores das duas equipes jogaram; o sistema sugere a mais provável e descobre de que lado está cada equipe, como o "Importar do PlayStats" fazia.
- O placar vem da partida; o administrador pode ajustar.
- A partida continua sendo uma partida normal do hub e **conta para o ranking da temporada**; a página dela mostra o campeonato, a fase e a série.
- **Resultado manual:** quando não há partida no hub (o servidor caiu, o jogo foi em outro servidor), o administrador digita o placar. O jogo aparece como resultado manual e fica fora das estatísticas.
- **Penalidade no jogo** e **penalidade nos pontos gerais**, cada uma com motivo.
- Resultados especiais: **W.O.**, desistência, desclassificação e jogo anulado, cada um com o efeito certo na tabela (o cancelado deixa de contar como empate).
- Cada mudança de placar, penalidade ou elenco fica registrada com quem fez e quando.
- Classificação e chave recalculadas a partir dos jogos, sem gravar totais nas equipes.

### Estatísticas

- Como os jogos ficam ligados a partidas do hub, as estatísticas do campeonato são as mesmas do resto do site, filtradas pelas partidas do campeonato, sem chamada externa.
- Ranking de jogadores do campeonato com os números de hoje (dano, skeets, levels, crowns, MVP e os demais), por fase e no total.
- Estatísticas por equipe e por série.

### Páginas públicas

A única entrada é o link **Campeonatos** no menu principal, que aparece quando o módulo está ligado. Não há destaque na página inicial nem em outras páginas: tudo, dos campeonatos ativos ao histórico, é acessado a partir desse link.

| Página | O que mostra |
|---|---|
| Campeonatos | Os campeonatos em andamento no topo e, abaixo, o histórico dos finalizados, com campeão e vice de cada um. |
| Visão geral do campeonato | Status, próximo jogo com contagem regressiva, link da transmissão, campeão quando houver. |
| Regulamento | O Markdown do campeonato no idioma de quem vê. |
| Equipes | Elencos com capitão, avatar, horas e partidas no hub. |
| Draft | A ordem, cada escolha e as equipes formadas. |
| Tabela | A classificação de cada grupo, com os critérios de desempate. |
| Chave | A árvore do mata-mata; cada série abre os detalhes. |
| Jogos | Todos os jogos por rodada, com data, campanha, placar e link para a partida. |
| Estatísticas | Jogadores e equipes. |

### Administração

- Os campeonatos são gerenciados pelos **administradores do hub**; não há papel novo.
- Pode haver **mais de um campeonato em andamento** ao mesmo tempo.
- Assistente de criação com modelos prontos, que já preenchem as fases: "Pontos corridos", "Pontos corridos + mata-mata", "Mata-mata" e "Grupos + mata-mata".
- Ações perigosas (gerar de novo uma fase, apagar jogos) pedem confirmação e não valem depois que a fase começou, salvo por uma ação explícita de correção.

### Ideias para depois

- Dupla eliminação e sistema suíço.
- Inscrição dos jogadores pelo site, com aprovação da organização.
- Organizadores por campeonato, que só gerenciam aquele campeonato.
- Draft em que cada capitão escolhe pelo próprio login, com tempo por escolha.
- Avisos no Discord ou por WhatsApp (jogo marcado, jogo amanhã, resultado).
- Página de transmissão (overlay) com placar e equipes.
- Aba **Campeonatos** na página do jogador, com participações, equipes e títulos.
- Premiação e conquistas ligadas a campeonatos (por exemplo "Campeão"), conversando com a [proposta de conquistas](achievements.pt.md).
- Ligar o servidor de jogo ao jogo marcado: reservar o servidor e mostrar o jogo do campeonato no servidor ao vivo.

## De/para

| No sistema antigo | No hub |
|---|---|
| Usuário administrador com e-mail e senha | Administrador do hub ou organizador do campeonato, pela Steam |
| Senha do capitão | Login pela Steam; capitão e vice são campos da equipe |
| Jogador (`JogadorEntity`) | Perfil do hub, ligado ao campeonato pela lista de participantes e pela equipe |
| Time (`TimeEntity`) com totais gravados | Equipe do campeonato; totais calculados |
| Ordem 0 e 1 (capitão e suporte) | Capitão como campo; potes no sorteio |
| Campanha fixa no código | Catálogo de campanhas do hub |
| Confronto dos pontos corridos | Série MD1 numa fase de pontos corridos |
| Playoff com 3 jogos em colunas | Série com N jogos numa fase de mata-mata |
| Rodada e ordem digitadas | Posição calculada na chave |
| Campanha excluída por equipe | Veto, quando a regra da fase pede |
| Período e sugestões de data | Prazo e propostas de data |
| Início e fim de estatística | Ligação com a partida do hub |
| Estatísticas buscadas no PlayStats | Consulta das partidas ligadas, no banco |
| Regulamento no GitHub | Regulamento em Markdown no campeonato, por idioma |
| Draft registrado à mão | Draft no sistema |

## Desligar o site antigo

O subdomínio `https://torneio.l4d2.com.br/` não será mais usado. Os campeonatos passam a existir só no hub, e o site antigo é desligado por completo, sem redirecionamento e sem migrar dados.

O que desligar:

| Onde | O que |
|---|---|
| DNS de `l4d2.com.br` | O registro do subdomínio `torneio`. |
| Servidor "L4D2 - Resources" | A pasta `/home/torneio` com o site Angular, a configuração do servidor web e o certificado do subdomínio. |
| Azure | A Function App `torneio-l4d2-function` (Brazil South), a Storage Account `torneiol4d2` (tabelas e o contêiner `imgs`) e o App Service antigo `torneio-l4d2.azurewebsites.net`, se ainda existir. |
| Azure DevOps, projeto "Torneio L4D2" | Os pipelines de build e release do front-end e do back-end, e a conexão SSH "L4D2 - Resources" se nada mais a usar. |

Links que apontam para o site antigo:

| Arquivo | O que fazer |
|---|---|
| `torneio-l4d2-regras/README.md`, `README.en.md` e `README.es.md`, linha "Site oficial do torneio" | Tirar a linha ou apontar para a página de campeonatos do hub. |
| `torneio-l4d2-front-end/README.md` e `torneio-l4d2-back-end/README.md` | Avisar que o projeto foi substituído pelo hub; as capturas de tela do back-end saem do ar junto com a Storage Account. |
| Repositórios `torneio-l4d2-front-end` e `torneio-l4d2-back-end` no GitHub | Arquivar, como somente leitura. |

O repositório de regras continua como está: o regulamento de cada edição e o registro do draft de 2026 são o único histórico das edições antigas.

## Decisões

| Assunto | Decisão |
|---|---|
| Nome do módulo | Campeonatos (*Championships* em inglês; `Championships` no código). |
| Navegação | Só um link **Campeonatos** no menu principal; nada na página inicial. |
| Site antigo | O subdomínio `torneio.l4d2.com.br` é desligado, sem redirecionamento. |
| Participantes | Só a organização cadastra; não há inscrição pelo site. |
| Agendamento | O capitão e um vice-capitão propõem e aceitam datas pela equipe, na área do capitão. |
| Campanhas | Os capitães (ou vices) escolhem e vetam pelo site; o administrador pode escolher por eles. |
| Draft | Cada capitão escolhe na área do capitão quando chega a vez da equipe, sem limite de tempo e sem nada ao vivo; o administrador também pode escolher por qualquer equipe. |
| Formatos da primeira versão | Pontos corridos (com grupos) e eliminação simples. |
| Jogo sem partida no hub | Placar digitado pelo administrador, marcado como resultado manual e fora das estatísticas. |
| Resultados | Os jogos são sempre no servidor e os capitães nunca mexem no placar. A partida que chega é ligada sozinha ao jogo com a mesma campanha, as duas equipes e uma data dentro da tolerância (6 horas por padrão); o jogo fica em andamento e só termina com o último mapa, depois de 60 minutos sem estatísticas ou quando o administrador encerra. |
| Área do capitão | Página separada do site do campeonato, com aviso ao lado da foto, para o draft, a identidade da equipe, o vice e as séries. |
| Identidade da equipe | O capitão ou o vice envia nome, sigla e bandeira; nada aparece antes da aprovação da administração. O vice é escolhido pelo capitão. |
| Fora da responsabilidade dos capitães | W.O., contestações, remarcações forçadas, reservas e convites ficam com a administração. |
| Quem administra | Os administradores do hub. |
| Avisos | Nenhum canal na primeira versão. |
| Campeonatos simultâneos | Permitidos. |
| Ranking da temporada | Os jogos de campeonato contam normalmente. |
| Jogo empatado no mata-mata | Novo jogo criado sozinho na série, com campanha sorteada. |
| Primeira escolha de campanha | A equipe melhor semeada escolhe primeiro. |
| Entrega | O módulo completo numa entrega só, com testes, manual e traduções. |
