# Dúvidas e problemas

🌐 [English](faq.md) | **Português** | [Español](faq.es.md)

Dúvidas comuns de jogadores e administradores, e o que fazer quando algo não funciona. Os problemas da instalação em si também estão listados no fim de cada guia de instalação: [Linux](../install-linux.pt.md#se-algo-der-errado), [Docker](../install-docker.pt.md), e os do servidor de jogo em [Conectando um servidor de jogo](../game-server.pt.md).

[Voltar ao manual](README.pt.md)

## Conteúdo

- [Para jogadores](#para-jogadores)
- [Para administradores](#para-administradores)
- [Resolvendo problemas](#resolvendo-problemas)

## Para jogadores

**Joguei uma partida e ela não está no ranking.**
Só as partidas ranqueadas contam. Uma partida é ranqueada quando a configuração e o tamanho do time dela se encaixam em uma regra de modo de jogo que conta, e ela tem mapas suficientes: com a regra padrão, 4v4 e pelo menos 2 mapas da mesma campanha em sequência. Mapas avulsos, 1v1 e outros modos aparecem como **Casual**. A partida também só aparece depois que o último mapa dela é enviado, alguns segundos depois de o mapa terminar.

**Minha vitória contou como rage quit.**
Um jogador que estava no primeiro mapa da partida e não está no último mapa é contado como rage quit, mesmo que tenha voltado depois ou perdido a conexão. Veja [Ranking > Vitórias, derrotas, empates e rage quits](ranking.pt.md#vitórias-derrotas-empates-e-rage-quits). Quando um substituto entrou no lugar, um administrador pode [trocar o jogador](matches.pt.md#trocando-um-jogador).

**Entrei no meio de uma partida e não recebi a vitória.**
Vitórias e derrotas vão para os jogadores que estavam no primeiro mapa. Quem entrou depois ganha só os pontos de MVP e de tank wipe.

**Minha experiência não mudou depois de um empate.**
Um empate não dá vitória nem derrota, só os pontos de MVP e de tank wipe.

**Por que a minha experiência zerou?**
Começou uma nova temporada. A temporada passada fica no seletor de período do ranking e de **Mais > Jogadores** enquanto ela for do ano atual; para um ano anterior, escolha esse ano, ou abra a temporada pelo endereço dela.

**Meu nome está antigo ou errado.**
O site mostra um só nome em todo lugar: o nome que um administrador forçou; se não houver, o nome que você mais usou em todas as suas partidas, de todas as temporadas; se não houver, o seu nome Steam. Os seus outros nomes ficam em **Nomes anteriores**, na sua página. Veja [Jogadores > O nome do jogador](players.pt.md#o-nome-do-jogador).

**Como eu ganho estrelas?**
Uma estrela para cada prêmio de MVP de SI e de MVP de CI em uma partida ranqueada. Veja [Loja de skins de armas > Estrelas](skin-shop.pt.md#estrelas).

**Minhas skins sumiram.**
Se o site faz o reembolso das skins no fim da temporada, as skins compradas são recolhidas quando começa uma nova temporada, e as estrelas voltam para você. Compre-as de novo. Presentes nunca são recolhidos.

**Comprei uma skin e não a vejo no jogo.**
Ela aparece a partir da próxima troca de mapa. No jogo, `!skin status` mostra o que você tem.

**O site diz que preciso de mais partidas para votar.**
O ranking da comunidade pede um número mínimo de partidas, contadas em todas as temporadas: 10 por padrão.

**A página do servidor ao vivo está atrasada em relação ao jogo.**
Ela fica cerca de 15 segundos atrás, de propósito. Um administrador muda o atraso em **Administração > Servidores de jogo > Página do servidor ao vivo**.

## Para administradores

**Como eu adiciono outro administrador?**
Em [Administração > Administradores](administration.pt.md#administradores), com o SteamID64 dele. Se você não conseguir mais fazer login, use `playstats-hub add-admin <SteamID64>` no Linux.

**Como eu adiciono um segundo servidor de jogo?**
Em [Administração > Servidores de jogo](administration.pt.md#servidores-de-jogo). Cada servidor recebe o próprio token. Os resultados dos jogadores se somam entre todos os servidores.

**Perdi o token de um servidor de jogo.**
O site não guarda a chave. Abra a página do servidor e clique em **Gerar uma nova chave**, depois coloque o novo token na configuração do servidor de jogo.

**Mudei os modos de jogo e as partidas antigas não mudaram.**
As regras valem para os arquivos novos. Use **Reprocessar todas as partidas** na [visão geral](administration.pt.md#visão-geral).

**Mudei os pontos e todas as temporadas mudaram.**
É o esperado: os rankings são calculados a partir das partidas com os pontos atuais. As temporadas não têm uma cópia congelada.

**Posso mudar a duração da temporada no meio de uma temporada?**
Sim. A temporada atual mantém as datas dela; a nova duração vale a partir da próxima.

**Tem como remover uma partida?**
Não. Uma partida vem dos arquivos de estatísticas. Para corrigir um resultado errado, [edite o placar do mapa](matches.pt.md#editando-o-placar-de-um-mapa).

**Um substituto entrou e o jogador que saiu levou rage quit. Dá para corrigir?**
Sim. [Troque o jogador](matches.pt.md#trocando-um-jogador) que saiu pelo substituto nos mapas que o jogador que saiu jogou. A partida e o ranking são recalculados.

**Como eu importo os dados do meu site antigo?**
A migração de dados é um programa separado, fora deste repositório, que envia os arquivos de estatísticas antigos pelo mesmo caminho que os plugins usam. Veja [o roadmap](../roadmap.pt.md#migração-de-dados) e, para os comandos usados no l4d2.com.br, [Traga os dados do site antigo](../switch-from-old-site.pt.md#traga-os-dados-do-site-antigo).

**Posso rodar duas cópias do site atrás de um balanceador de carga?**
Não. A partida ao vivo fica na memória de um processo só. Rode uma cópia.

**Quem vê as contas relacionadas de um jogador?**
Todo mundo, por padrão, como no site antigo. [Configurações > Privacidade](administration.pt.md#privacidade) pode restringi-las aos administradores. O aviso no chat do jogo é uma configuração separada do plugin, `gameinfo_related_accounts_chat`.

**Os visitantes ainda veem a página antiga por um momento.**
As páginas de quem não entrou com a Steam (ranking, jogadores, partidas, ranking da comunidade e o painel do jogo) ficam guardadas por até 60 segundos, para que muitos visitantes e buscadores custem pouco. Novos arquivos de estatísticas, configurações, punições e o reprocessamento as atualizam na hora; outras mudanças, como votos da comunidade, avatares ou nomes de jogadores, aparecem em até um minuto. Jogadores logados e administradores sempre veem a página atual.

## Resolvendo problemas

| O que você vê | O que fazer |
|---|---|
| Toda página leva para a página de configuração | A configuração não foi concluída. Faça login com a conta de administrador e conclua. Veja [Configuração inicial](first-run.pt.md). |
| O código de configuração é recusado | Copie de novo: `playstats-hub setup-code`, ou `docker compose logs hub \| grep "setup code"`. Ele só funciona até o primeiro administrador fazer login. |
| O servidor de jogo fica em **Aguardando a primeira conexão** | Confira as duas linhas no `cfg/playstats.cfg` e a linha `exec playstats.cfg` no `cfg/server.cfg` (ou as duas linhas no `cfg/server.cfg`), se o `l4d2_playstats_sync.smx` está na própria pasta `addons/sourcemod/plugins/` e é carregado no `cfg/sharedplugins.cfg` e se o servidor de jogo consegue alcançar o endereço do site. Rode `sm_playstats_test` no console do servidor. |
| O servidor de jogo mostra **Chave inválida** | O token na configuração do servidor de jogo não é o atual. Copie o token completo de novo, ou gere uma nova chave. |
| Conectado, mas nenhum arquivo de estatísticas chega | Confira **Gravação de estatísticas ativada** na lista de verificação (`sm_stats_writestats 1`), e se um mapa foi jogado até o fim. |
| As partidas chegam, mas são todas **Casual** | Nenhuma regra de modo de jogo as conta. A visão geral avisa sobre configurações sem regra. Veja [Modos de jogo](game-modes-and-campaigns.pt.md#modos-de-jogo). |
| Uma campanha personalizada gera uma partida por mapa | A campanha não está na lista, e mapas desconhecidos só são juntados quando vêm um depois do outro em até 2 horas. [Adicione a campanha](game-modes-and-campaigns.pt.md#campanhas) e depois reprocesse. |
| A página do servidor ao vivo está vazia | O módulo do servidor ao vivo precisa estar ligado, o `l4d2_gameinfo_sync` precisa estar carregado e precisa haver uma partida em andamento. Depois de reiniciar o site, espere a próxima atualização do servidor de jogo. |
| O `!ranking` abre um painel vazio | O jogo não consegue abrir o endereço HTTPS. Defina `playstats_ingame_url` com um endereço HTTP simples do site. Veja [Páginas dentro do jogo](in-game-pages.pt.md#o-endereço-usado-pelo-jogo). |
| Os avatares não carregam | Eles são baixados em segundo plano. Espere alguns minutos, ou defina uma chave da Steam Web API em [Configurações](administration.pt.md#site). |
| Os jogadores foram deslogados depois de mudar o site de lugar | A pasta `keys` não foi copiada. Eles só precisam fazer login de novo. |
| As imagens das skins não carregam | As miniaturas das skins que vêm com o hub são servidas pelo site. Para as fotos, e para as miniaturas de skins adicionadas depois, confira **Endereço dos assets** nas [configurações da loja](skin-shop.pt.md#configurações-da-loja); em branco, o site usa o endereço do servidor atual. |
| Uma página mostra **Página não encontrada** | O endereço está errado, ou o módulo dela está desligado. Veja [Módulos](administration.pt.md#módulos). |
| Uma página mostra **Algo deu errado** | O site encontrou um erro inesperado. Os detalhes estão no log: `playstats-hub logs`, ou `docker compose logs hub`. |
| Atrás de um proxy reverso, o site vê `http` em vez de `https`, ou o endereço do proxy em vez do visitante | O proxy não está na mesma máquina, então o site ignora os cabeçalhos que ele repassa. Adicione o endereço ou a rede dele em `Hub__TrustedProxies`. Veja [Instalando com Docker > Configuração](../install-docker.pt.md#configuração). |
| O `docker compose ps` mostra o contêiner como `unhealthy` | O site não responde `/api/ping` na própria porta. Leia `docker compose logs hub` e confira se `ASPNETCORE_URLS` ainda tem um endereço `http`. |
