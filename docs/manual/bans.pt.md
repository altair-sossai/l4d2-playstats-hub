# Banimentos

🌐 [English](bans.md) | **Português** | [Español](bans.es.md)

O módulo Banimentos guarda no site os jogadores banidos dos servidores. Os banimentos dados no jogo com os comandos de sempre do SourceMod chegam sozinhos, os administradores também podem banir pelo site, e os servidores consultam o site sobre cada jogador que se conecta. Todos podem ver quem está banido, por quê e quando o banimento termina.

[Voltar ao manual](README.pt.md)

## Conteúdo

- [Ligando o módulo](#ligando-o-módulo)
- [Para jogadores](#para-jogadores)
- [Para administradores](#para-administradores)
- [Banimentos dados no jogo](#banimentos-dados-no-jogo)
- [Como os servidores aplicam os banimentos](#como-os-servidores-aplicam-os-banimentos)
- [Contas relacionadas e alertas](#contas-relacionadas-e-alertas)
- [Importando o banned_user.cfg](#importando-o-banned_usercfg)

## Ligando o módulo

O módulo vem desligado em um site novo. Ligue **Banimentos** em **Administração → Módulos** e instale o plugin `l4d2_bans` em cada servidor (veja [Conectando um servidor](../game-server.pt.md)). Sem o plugin a lista existe no site, mas os servidores não enviam os banimentos nem verificam os jogadores.

## Para jogadores

**Mais → Banimentos** abre `/bans`, a lista dos jogadores banidos agora: o jogador, o motivo, o dia do banimento, quando termina (ou **Permanente**) e quem baniu. A busca encontra um banimento por nome, Steam ID ou motivo. Banimentos encerrados não aparecem nessa lista. No celular cada linha mostra a foto, o nome e o botão **Detalhes**, que abre uma janela com tudo.

A página de um jogador banido começa com um aviso vermelho: "*Nome* está banido dos servidores até *data*", ou "permanentemente", com o motivo e um link para a lista.

Um jogador banido que tenta entrar em um servidor é expulso com o motivo e o fim do banimento, no idioma do jogo. Não há como contestar um banimento pelo site.

Um jogador conectado ao site é avisado no sino quando é banido e quando o banimento é retirado (veja [Notificações](notifications.pt.md)).

Os endereços IP, as contas relacionadas e os alertas nunca aparecem para o público: só os administradores os veem.

## Para administradores

**Administração → Banimentos** (`/admin/bans`) tem três abas:

| Aba | O que mostra |
|---|---|
| Ativos | Os banimentos em vigor, com o jogador, o motivo, o dia, o fim, quem baniu (um administrador, **Servidor** para um banimento dado pelo console ou por um plugin, **Votação**, ou **Importado**, com o comando e o servidor embaixo) e as **Tentativas** do jogador de entrar depois do banimento. |
| Encerrados | Os banimentos que expiraram ou foram retirados, com quem os retirou e por quê. Nada é apagado: este é o histórico. |
| Alertas | Contas que compartilham um endereço IP com uma conta banida e não foram banidas. Veja [Contas relacionadas e alertas](#contas-relacionadas-e-alertas). |

A busca no topo filtra as abas Ativos e Encerrados por nome, Steam ID ou motivo.

| Botão | O que faz |
|---|---|
| **Adicionar** | Abre a janela **Banir um jogador**: o jogador (um Steam ID, um link de perfil ou **Buscar jogador**), a duração e o motivo. A duração é **Permanente**, 1 hora, 1 dia, 3 dias, 1 semana, 2 semanas, 1 mês, 3 meses, 1 ano, todas contadas a partir de agora, ou **Até uma data**, um dia e horário no fuso do site. Um jogador que já está banido não pode ser banido de novo: edite o banimento. |
| **Editar** | Muda o fim e o motivo de um banimento ativo. |
| **Desbanir** | Retira o banimento na hora. O motivo que você escrever fica no histórico e só os administradores o veem. |
| **Importar** | Lê um arquivo `banned_user.cfg`. Veja [Importando o banned_user.cfg](#importando-o-banned_usercfg). |

Toda alteração pede confirmação. Com o [módulo do servidor ao vivo](live-server.pt.md) ligado, um banimento, uma alteração ou um desbanimento feito no site chega aos servidores em poucos segundos: um jogador banido que está jogando é expulso, e um jogador desbanido sai da lista de banimentos do servidor. Sem esse módulo, a alteração vale na próxima vez que o jogador se conectar.

## Banimentos dados no jogo

O plugin `l4d2_bans` não acrescenta comandos. Ele escuta os banimentos do SourceMod, então tudo o que bane um jogador chega ao site:

| No jogo | No site |
|---|---|
| `sm_ban <jogador> <minutos> [motivo]`, ou o banimento do menu de admin | Um banimento com essa duração (0 é permanente), com o administrador que o deu. |
| `sm_addban <minutos> <steamid> [motivo]` | O mesmo, para um jogador que não está no servidor. |
| Uma votação de banimento (`sm_voteban`) | Um banimento mostrado como **Votação**. |
| Um plugin que bane, como o `l4d2_mix` quando um jogador abandona o mix | Um banimento mostrado como **Servidor**, com o comando do plugin. É um banimento normal, listado como os outros. |
| `sm_unban <steamid>` | Retira o banimento ativo do jogador. |

Banir de novo um jogador que já está banido substitui o fim e o motivo do banimento ativo. Só os banimentos por Steam ID são guardados; um banimento por endereço IP (`sm_banip`) é aplicado pelo servidor, mas não chega ao site.

O servidor continua aplicando os próprios banimentos como antes, então um banimento dado com o site fora do ar continua valendo naquele servidor. O plugin guarda os banimentos que o site não recebeu e os envia de novo a cada minuto.

## Como os servidores aplicam os banimentos

Quando um jogador se conecta, o plugin consulta o site sobre aquele jogador, uma vez. O site responde por um índice, então o tamanho da lista não importa, e o servidor não espera a resposta: a consulta roda em segundo plano. Quando o jogador está banido, o plugin o expulsa com o motivo e também acrescenta o banimento ao próprio servidor pelo tempo que falta, então um jogador que reconecta é recusado pelo servidor sem consultar o site de novo. Cada uma dessas tentativas é contada na aba **Ativos**.

Se o site não responder, o jogador entra, exceto pelos banimentos que o próprio servidor guarda: os dados naquele servidor e os que ele aprendeu com o site desde que foi iniciado.

## Contas relacionadas e alertas

O site sabe quais contas Steam se conectaram do mesmo endereço IP (veja [Conexões de jogadores](moderation.pt.md)).

Quando você bane um jogador pelo site e ele tem contas relacionadas, uma janela as lista e pergunta se elas também devem ser banidas, com a mesma duração e o mesmo motivo. Desmarque uma conta para deixá-la de fora; contas que já estão banidas aparecem marcadas e não podem ser escolhidas.

Um alerta é criado, e os administradores são avisados no sino, quando:

- uma conta se conecta do endereço de uma conta banida depois do banimento, ou o jogador banido se conecta de um endereço novo onde outras contas jogaram;
- um banimento chega de um servidor, ou é importado, e o jogador já compartilha um endereço com outras contas.

A aba **Alertas** lista a conta, a conta banida a que ela está relacionada e o motivo desse banimento. **Banir** abre a janela **Banir um jogador** preenchida com a conta, o motivo "Conta relacionada de *nome*" e o fim do banimento original. **Ignorar** fecha o alerta, e ele não volta para aquele banimento. Um alerta também sai da lista quando a conta é banida ou quando o banimento original termina.

## Importando o banned_user.cfg

**Importar** recebe o conteúdo do arquivo `cfg/banned_user.cfg` de um servidor. Cada linha `banid` vira um banimento. Os comentários escritos acima dela são lidos quando seguem este formato:

```text
// Name: Jogador
// Ban date: 2026-08-24
// Expire in: never
// Ban reason: Trapaça
banid 0 STEAM_0:1:12345678
banid 0 STEAM_1:1:12345678
```

As duas linhas do mesmo jogador viram um banimento só. Uma linha em branco separa os jogadores. Jogadores que já estão banidos, e banimentos cujo fim já passou, ficam como estão; linhas que não podem ser lidas são contadas e ignoradas. Banimentos importados aparecem como **Importado**.
