# Plugins do servidor de jogo

🌐 [English](README.md) | **Português** | [Español](README.es.md)

Os cinco plugins do SourceMod que conectam um servidor de Left 4 Dead 2 ao L4D2 PlayStats Hub. Para instalá-los, copie tudo o que está dentro da pasta `left4dead2` deste diretório (ou do zip da release) para a pasta `left4dead2` do seu servidor de jogo e depois defina dois valores em `cfg/playstats.cfg`. Para um passo a passo guiado, do cadastro do servidor no site até a primeira partida, veja [Conectando um servidor de jogo](../docs/game-server.pt.md).

## Conteúdo

- [O que há nesta pasta](#o-que-há-nesta-pasta)
- [Os plugins](#os-plugins)
- [Requisitos](#requisitos)
- [Instalação](#instalação)
- [Configurações](#configurações)
- [Modos de partida do L4D2 Competitive Rework](#modos-de-partida-do-l4d2-competitive-rework)
- [Atualização](#atualização)
- [Arquivos de skins de armas e de patentes](#arquivos-de-skins-de-armas-e-de-patentes)
- [Comandos](#comandos)
- [Verificando a instalação](#verificando-a-instalação)
- [Solução de problemas](#solução-de-problemas)
- [O que os plugins chamam](#o-que-os-plugins-chamam)
- [Compilação](#compilação)
- [Ainda não testado em um servidor de jogo](#ainda-não-testado-em-um-servidor-de-jogo)
- [Licença](#licença)

## O que há nesta pasta

A pasta `left4dead2` tem a mesma estrutura da pasta do jogo de um servidor, então cada arquivo cai no seu lugar quando você a copia:

```text
plugins/
├── left4dead2/                      copie o que está dentro para a pasta left4dead2 do servidor
│   ├── addons/sourcemod/
│   │   ├── plugins/                 l4d2_gameinfo_sync e l4d2_weapon_skins (.smx)
│   │   │   └── optional/            os outros três (.smx), carregados pelos modos de partida
│   │   ├── scripting/               os fontes (.sp) e os arquivos include para compilá-los
│   │   ├── translations/            textos do chat: inglês, pt, es, chi e zho
│   │   └── configs/weapon_skins.cfg as armas e skins que o plugin de skins conhece
│   ├── materials/sprites/           os ícones das patentes (patent_<level>_v02.vmt e .vtf)
│   └── cfg/playstats.cfg            suas configurações: o único arquivo que você edita
├── build.ps1, build.sh              compilam os plugins em left4dead2/addons/sourcemod/plugins e na pasta optional dela
├── README.md, LICENSE, NOTICE.md    este guia e a licença (não vão para o servidor)
```

A pasta do jogo do servidor é a que tem `gameinfo.txt`, `addons/` e `cfg/` dentro, por exemplo `/home/steam/l4d2/left4dead2/` ou `C:\l4d2\left4dead2\`.

Os arquivos `.smx` do repositório são sempre o resultado da compilação dos fontes ao lado deles: todo push verifica isso (veja [Compilação](#compilação)).

## Os plugins

| Plugin | O que faz | Módulo do site | Precisa de |
|---|---|---|---|
| `l4d2_playstats_sync` | Avisa o site a cada 30 segundos que o servidor está no ar e envia os arquivos de estatísticas gravados pelo `l4d2_playstats`. Tem o comando `sm_playstats_test`. Todo servidor precisa dele. | Sempre ativo | REST in Pawn |
| `l4d2_gameinfo_sync` | Envia a partida ao vivo: configuração, rodada, placar, jogadores, eventos e chat. Traz o chat do site para o jogo e executa os comandos enviados pelo site. | Servidor ao vivo, Chat do site, Comandos remotos | REST in Pawn, left4dhooks |
| `l4d2_ranking` | Abre a página do ranking dentro do jogo com `!ranking` e a página de comandos com `!comandos` ou `!commands`, e o ranking sozinho para os jogadores que entram antes de uma partida começar. | Ranking | REST in Pawn, left4dhooks |
| `l4d2_show_patent_icon` | Mostra o ícone da patente de cada jogador acima da cabeça antes de uma partida começar. | Ranking | REST in Pawn, left4dhooks, os sprites das patentes |
| `l4d2_weapon_skins` | Aplica as skins de armas que os jogadores compraram na loja. | Loja de skins de armas | REST in Pawn, os modelos e materiais das skins |

Só o `l4d2_playstats_sync` é obrigatório. Cada um dos outros funciona sem os demais; apague o `.smx` de um plugin que você não quer antes de copiar a pasta, ou depois.

## Requisitos

| O quê | Usado por | Vem com o L4D2 Competitive Rework | Onde conseguir |
|---|---|---|---|
| Servidor dedicado de Left 4 Dead 2 | Todos | Não | SteamCMD, app 222860 |
| Metamod:Source e SourceMod 1.12 | Todos | Sim | <https://www.sourcemod.net/downloads.php?branch=stable> |
| Extensão REST in Pawn 1.3 (`rip.ext`) | Os cinco | **Não, você adiciona** | <https://github.com/ErikMinekus/sm-ripext/releases> |
| Left 4 DHooks Direct (`left4dhooks`) | `l4d2_gameinfo_sync`, `l4d2_ranking`, `l4d2_show_patent_icon` | Sim | <https://github.com/SilvDev/Left4DHooks/releases> |
| `l4d2_playstats` | Os arquivos de estatísticas que o `l4d2_playstats_sync` envia | Sim, em `plugins/optional/` | [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) |
| `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod` ou `l4d2_hybrid_scoremod_zone`, `l4d2_skill_detect` | Opcionais: dados extras ao vivo no `l4d2_gameinfo_sync` (ready-up, pausas, porcentagens dos chefes, bônus, skeets e outros eventos) | Sim, em `plugins/optional/` | [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) |
| `l4d2_mix`, `l4d2_anti_cheat` | Opcionais: o evento "mix iniciado" e o status do anti-cheat no `l4d2_gameinfo_sync` | Não | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) |
| Modelos e materiais das skins, e um FastDL para eles e para os sprites das patentes (os sprites vêm neste pacote) | `l4d2_show_patent_icon`, `l4d2_weapon_skins` | Não | [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets), veja [Arquivos de skins de armas e de patentes](#arquivos-de-skins-de-armas-e-de-patentes) |
| Um servidor de jogo cadastrado no site | Todos | Não | O site: **Administração → Servidores de jogo** |

Os plugins são compilados com o SourceMod 1.12.0.7260 e precisam do SourceMod 1.12 ou mais novo. Os plugins opcionais são procurados pelo nome enquanto o servidor roda: quando um deles não está carregado, o `l4d2_gameinfo_sync` continua funcionando e só deixa de fora aquela parte. Os arquivos include em `scripting/include` só são necessários para compilar; `l4d2util` e `colors` são compilados dentro dos plugins e não precisam de nada no servidor.

Para instalar o REST in Pawn, baixe `sm-ripext-<version>-linux.zip` ou `-windows.zip` e extraia na pasta `left4dead2` do servidor. Ele adiciona `addons/sourcemod/extensions/rip.ext.so` (ou `.dll`) e `addons/sourcemod/configs/ripext/ca-bundle.crt`, a lista de autoridades certificadoras em que ele confia para HTTPS. O SourceMod o carrega sozinho quando um plugin precisa dele.

## Instalação

1. Instale o que os [requisitos](#requisitos) listam. Em um servidor com o L4D2 Competitive Rework, isso é só o REST in Pawn.
2. Copie tudo o que está dentro de `left4dead2/` para a pasta `left4dead2` do servidor. Nada nessa pasta substitui um arquivo do SourceMod ou do L4D2 Competitive Rework, exceto arquivos include em `addons/sourcemod/scripting/include` (veja [Atualização](#atualização)).
3. No site, abra **Administração → Servidores de jogo**, crie o servidor e copie as duas linhas que o site mostra (`playstats_url` e `playstats_token`).
4. Abra `cfg/playstats.cfg` no servidor e cole as duas linhas onde o arquivo indica.
5. Adicione esta linha no fim de `cfg/server.cfg`: `exec playstats.cfg`.
6. Em um servidor com o L4D2 Competitive Rework, adicione os plugins a `cfg/sharedplugins.cfg` (veja [Modos de partida do L4D2 Competitive Rework](#modos-de-partida-do-l4d2-competitive-rework)).
7. Reinicie o servidor de jogo.
8. Rode `sm_playstats_test` no console do servidor (veja [Verificando a instalação](#verificando-a-instalação)).

Copie a pasta como está: `l4d2_gameinfo_sync` e `l4d2_weapon_skins` ficam em `addons/sourcemod/plugins/`, que o SourceMod carrega quando o servidor inicia, e os outros três em `addons/sourcemod/plugins/optional/`, que só um modo de partida carrega (veja [Modos de partida do L4D2 Competitive Rework](#modos-de-partida-do-l4d2-competitive-rework)). É onde os servidores com o L4D2 Competitive Rework já os guardam, então um servidor que rodava as versões anteriores continua carregando os plugins com as linhas que já tem. Enquanto nenhum modo de partida está carregado, o `l4d2_playstats_sync` não roda e o site mostra o servidor como offline; para mantê-lo conectado, adicione `sm plugins load optional/l4d2_playstats_sync.smx` no fim do `cfg/server.cfg`.

## Configurações

Todas as configurações dos cinco plugins estão em `cfg/playstats.cfg`, com uma explicação acima de cada linha. Só a primeira parte desse arquivo precisa mudar; o resto está desligado com `//` e mostra o valor padrão.

Você precisa definir:

| Cvar | Plugin | Padrão | Valores | O que fazer |
|---|---|---|---|---|
| `playstats_url` | Os cinco | vazio | Endereço do site com `https://` ou `http://`, sem barra no fim | Cole a linha que o site mostra |
| `playstats_token` | Os cinco | vazio | `<identifier>:<key>`, como o site mostra | Cole a linha que o site mostra. Mantenha em segredo |
| `sm_stats_writestats` | `l4d2_playstats` (Competitive Rework) | `0` | `0` desligado, `1` grava os arquivos de estatísticas, `2` também grava tabelas legíveis | Mantenha o `1` que o `cfg/playstats.cfg` define, a não ser que um modo de partida já defina |

Você pode mudar (cada linha está desligada no arquivo, e o plugin usa o padrão):

| Cvar | Plugin | Padrão | Valores | Significado |
|---|---|---|---|---|
| `playstats_public_url` | `l4d2_ranking`, `l4d2_weapon_skins` | vazio | Um endereço, sem barra no fim | Endereço do site que os jogadores leem no chat: a dica do ranking e o link da loja de skins. Vazio significa `playstats_url`. Defina quando os plugins chegam ao site por um endereço IP mas os jogadores o conhecem por um nome, por exemplo `https://stats.example.com`. |
| `playstats_ingame_url` | `l4d2_ranking` | vazio | Um endereço, sem barra no fim | Endereço que o navegador do jogo abre. Vazio significa `playstats_url`. Use um endereço `http://` simples quando o painel fica em branco (veja abaixo). |
| `ranking_show_motd_panel` | `l4d2_ranking` | `1` | `0` ou `1` | `0` impede o plugin de abrir qualquer página, mesmo com `!ranking`, `!comandos` ou `!commands`. |
| `ranking_motd_path` | `l4d2_ranking` | `/` | Um caminho que começa com `/` | Página que o `!ranking` abre, a partir da raiz do site: `/` é o ranking da página inicial, `/ingame` as páginas compactas do jogo. |
| `ranking_commands_path` | `l4d2_ranking` | `/ingame/commands` | Um caminho que começa com `/` | Página que o `!comandos` e o `!commands` abrem, a partir da raiz do site. |
| `patent_icon_enabled` | `l4d2_show_patent_icon` | `0` | `0` ou `1` | Liga os ícones das patentes. Com `0` o plugin não pede as patentes ao site e os jogadores não baixam os sprites. Ligue só depois que os sprites estiverem no FastDL. |
| `patent_icon_version` | `l4d2_show_patent_icon` | `2` | `1` ou mais | Versão nos nomes dos arquivos de sprite, `patent_<level>_v<version>`, os dois com dois dígitos. Os arquivos do repositório de assets são da versão `2`. |
| `patent_icon_max_level` | `l4d2_show_patent_icon` | `15` | `1` ou mais | Maior nível de patente que tem arquivo de sprite. |
| `gameinfo_related_accounts_chat` | `l4d2_gameinfo_sync` | `1` | `0` ou `1` | Avisa no chat quando um jogador se conecta do mesmo endereço IP de outra conta já vista. `0` mostra só no console. O site tem uma configuração própria para quem vê essas contas na página do jogador ([Configurações > Privacidade](../docs/manual/administration.pt.md#privacidade)). |
| `gameinfo_boomer_vomit_min_survivors` | `l4d2_gameinfo_sync` | `3` | `1` ou mais | Mínimo de sobreviventes que um boomer precisa vomitar para o evento chegar ao site. |
| `weapon_skins_enabled` | `l4d2_weapon_skins` | `0` | `0` ou `1` | Liga as skins. Ligue só depois que os arquivos das skins estiverem no servidor e no FastDL. |
| `weapon_skins_kick_no_downloads` | `l4d2_weapon_skins` | `0` | `0` ou `1` | Expulsa os jogadores cujo jogo se recusa a baixar os arquivos do servidor (`cl_allowdownload 0`, ou `cl_downloadfilter` diferente de `all` ou `nosounds`). Com `0` eles jogam, e enquanto um deles está no servidor as skins ficam desligadas em terceira pessoa para todos. |
| `weapon_skins_pile_radius` | `l4d2_weapon_skins` | `120.0` | `0` ou mais | Raio, em unidades do jogo, usado para encontrar armas primárias largadas perto umas das outras. |
| `weapon_skins_pile_max` | `l4d2_weapon_skins` | `3` | `0` ou mais | Máximo de armas primárias largadas mantidas dentro desse raio; as mais antigas são removidas. `0` desliga. |
| `weapon_skins_preview_all` | `l4d2_weapon_skins` | `0` | `0` ou `1` | Modo de teste: todas as skins são baixadas a partir do próximo mapa, e admins root podem usar qualquer uma sem comprar e fora do ready-up. Mantenha `0` em um servidor público. |

Onde defini-las:

- Em `cfg/playstats.cfg`, executado pelo `cfg/server.cfg` com `exec playstats.cfg`. O jogo roda o `server.cfg` toda vez que um mapa carrega, então os valores são aplicados de novo depois que um modo de partida recarrega os plugins. Este é o lugar de toda configuração que é igual em todos os modos de partida.
- Com `confogl_addcvar` na configuração de um modo de partida, por exemplo `cfg/cfgogl/zonemod/zonemod.cfg`, só para uma configuração que precisa ser diferente entre os modos. Depois apague essa linha de `cfg/playstats.cfg`: uma configuração nos dois lugares muda de um valor para o outro a cada mapa, e o Confogl mostra cada mudança no chat.
- Nunca coloque `playstats_url` ou `playstats_token` em um modo de partida nem com `confogl_addcvar`: o Confogl mostra no chat o valor antigo e o novo das configurações que ele acompanha, o que mostraria o token para os jogadores.

A cvar do token é protegida: o jogo nunca envia o valor dela para os jogadores, e o `sm_cvar` do SourceMod só a mostra para admins com a flag de senha. Os plugins nunca a mostram.

A página que o `l4d2_ranking` abre é `<address><ranking_motd_path>?steamid=<SteamID64 of the player>`: a página inicial por padrão, `/ingame` para as páginas compactas. O `!comandos` e o `!commands` abrem `<address>/ingame/commands?steamid=...` (`ranking_commands_path`). O navegador dentro do jogo é o Chromium 18, de 2012, e não se espera que ele abra sites HTTPS atuais. Se o painel ficar em branco, sirva o site em HTTP simples em outro endereço ou porta e coloque esse endereço em `playstats_ingame_url`. Veja [docs/in-game-browser.pt.md](../docs/in-game-browser.pt.md).

O `l4d2_gameinfo_sync` envia ao site o endereço IP de cada jogador que se conecta, e o site o usa para encontrar contas que compartilham um endereço. Ele também executa, como comandos do servidor, os comandos que os administradores do site enviam pela página **Comandos remotos**: quem controla uma conta de administrador do site pode executar qualquer comando do servidor, como com RCON. Ele também executa o `sm_aac sync` que o site enfileira depois de uma alteração nas configurações do Anti-Cheat, mesmo com o módulo de comandos remotos desligado.

## Modos de partida do L4D2 Competitive Rework

Quando um modo de partida carrega, o gerenciador de plugins do Confogl descarrega todos os plugins e carrega só os listados nos arquivos indicados por `confogl_match_execcfg_plugins`. No L4D2 Competitive Rework essa cvar é definida em `cfg/server.cfg` como `generalfixes.cfg;confogl_plugins.cfg;sharedplugins.cfg`, e `cfg/sharedplugins.cfg` é o arquivo feito para os seus próprios plugins, carregado por todos os modos de partida.

Adicione estas linhas a `cfg/sharedplugins.cfg`, deixando de fora os plugins que você não usa:

```text
sm plugins load optional/l4d2_playstats_sync.smx
sm plugins load l4d2_gameinfo_sync.smx
sm plugins load optional/l4d2_ranking.smx
sm plugins load optional/l4d2_show_patent_icon.smx
sm plugins load l4d2_weapon_skins.smx
```

Sem essas linhas os três plugins de `optional/` nunca carregam, e os dois de `addons/sourcemod/plugins/` param quando um modo de partida carrega.

Os modos de partida do L4D2 Competitive Rework já carregam `optional/l4d2_playstats.smx` (em `confogl_plugins.cfg`) e os plugins opcionais que o `l4d2_gameinfo_sync` usa. Os modos ZoneMod não definem `sm_stats_writestats`, e é por isso que o `cfg/playstats.cfg` o define como `1`. Se os modos do seu servidor o definem com `confogl_addcvar`, apague essa linha de `cfg/playstats.cfg`.

No site, **Administração → Modos de jogo** decide quais modos de partida contam para o ranking. As estatísticas são enviadas de todos os modos.

## Atualização

Copie a nova pasta `left4dead2` por cima da antiga de novo, exceto `cfg/playstats.cfg`, que guarda o seu token. Depois reinicie o servidor enquanto ele está vazio, para que todos os plugins carreguem na versão nova.

| Arquivo | Pode ser substituído em uma atualização |
|---|---|
| `addons/sourcemod/plugins/*.smx` e `addons/sourcemod/plugins/optional/*.smx` | Sim. |
| `addons/sourcemod/translations/**` | Sim, a não ser que você tenha editado os textos. |
| `addons/sourcemod/configs/weapon_skins.cfg` | Sim, a não ser que você tenha adicionado armas ou skins próprias. |
| `addons/sourcemod/scripting/**` | Sim para o servidor: só é usado para compilar. Substitui arquivos include com o mesmo nome, como `left4dhooks.inc` ou `readyup.inc`; se você compila outros plugins no servidor, não copie esta pasta. |
| `cfg/playstats.cfg` | **Não.** Tem o seu endereço e o seu token. Pule este arquivo, ou copie de novo e cole as suas duas linhas de volta. |

No Windows, quando a cópia perguntar sobre arquivos que já existem, substitua-os e pule o `playstats.cfg`. No Linux:

```bash
rsync -a --exclude cfg/playstats.cfg left4dead2/ /path/to/server/left4dead2/
```

O plugin de skins guarda dois arquivos de cache em `addons/sourcemod/data/` (`weapon_skins_cache.json` e `weapon_skins_players_cache.json`), para poder aplicar as skins enquanto o site está fora do ar. A atualização não mexe neles.

## Arquivos de skins de armas e de patentes

O `l4d2_show_patent_icon` e o `l4d2_weapon_skins` mostram arquivos que não fazem parte do jogo, então os jogadores precisam baixá-los. Eles estão em [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets):

| Arquivos | No servidor de jogo | No FastDL |
|---|---|---|
| `models/l4d2br/weapons/<weapon>/` (um modelo combinado por arma) | Sim, na pasta do jogo | Sim |
| `materials/models/l4d2br/weapons/<weapon>_<id>/` (as texturas de cada skin) | Sim, na pasta do jogo | Sim |
| `materials/sprites/patent_<level>_v02.vmt` e `.vtf` (ícones das patentes), que também vêm neste pacote de plugins | Sim, na pasta do jogo | Sim |

1. Copie `models/` e `materials/` do repositório de assets para a pasta `left4dead2` do servidor. Todo caminho `view_model`, `world_model`, `models` e `materials` em `addons/sourcemod/configs/weapon_skins.cfg` precisa existir ali; quando um não existe, o plugin pula aquela arma ou skin e, com as skins ligadas, registra `models were not found on the server` ou `materials were not found on the server`.
2. Coloque `models/`, `materials/` e o conteúdo de `fastdl/` em um servidor web que os jogadores consigam acessar, em uma pasta com a mesma estrutura da pasta do jogo. Os arquivos são servidos como estão, sem `.bz2`.
3. Aponte o jogo para ela em `cfg/server.cfg`:

   ```text
   sm_cvar sv_allowdownload 1
   sm_cvar sv_downloadurl "https://files.example.com/fastdl"
   ```

4. Ligue as skins com `weapon_skins_enabled 1` em `cfg/playstats.cfg`, e reinicie o servidor enquanto ele está vazio, para que os jogadores baixem os arquivos ao se conectar e não durante uma troca de mapa.

Sem `sv_downloadurl`, o próprio jogo envia os arquivos, muito devagar; com modelos grandes, os jogadores podem desistir antes de terminar. Os plugins adicionam os arquivos à lista de downloads a cada mapa: os sprites das patentes sempre, e para as skins, os modelos de todas as armas e os materiais das skins que alguém comprou (todas elas com `weapon_skins_preview_all 1`).

Os jogadores precisam de `cl_allowdownload 1` e `cl_downloadfilter all` (ou `nosounds`) no jogo para baixar os arquivos. `weapon_skins_kick_no_downloads 1` expulsa quem se recusa, com uma mensagem que diz o que mudar.

As skins que um jogador pode escolher vêm do site: o `weapon_skins.cfg` liga o nome de cada skin à sua arma, à família do modelo e à pasta de materiais, e a loja do site vende as skins com os mesmos nomes.

## Comandos

| Comando | Quem | O que faz |
|---|---|---|
| `sm_playstats_test` | Console do servidor, ou admins com a flag root | Chama o site na hora e mostra o resultado (veja abaixo). |
| `sm_ranking` (`!ranking`) | Todos | Abre a página do ranking dentro do jogo. |
| `sm_comandos`, `sm_commands` (`!comandos`, `!commands`) | Todos | Abre a página com os comandos do servidor dentro do jogo. |
| `sm_skin`, `sm_skins` (`!skin`) | Todos | `!skin` mostra a ajuda; `!skin next`, `!skin prev`, `!skin <name>` e `!skin default` escolhem a skin da arma na mão, ou da arma indicada depois; `!skin status` mostra as moedas e as skins que você tem. Os sobreviventes trocam de skin durante o ready-up. |
| `sm_skin on`, `sm_skin off`, `sm_skin test <skin\|default> [weapon]` | Admins com a flag root; `on` e `off` também pelo console do servidor | `on` e `off` sobrepõem `weapon_skins_enabled` até o plugin ser carregado de novo (um reinício, ou um modo de partida que recarrega os plugins). `test` experimenta uma skin em você até o mapa mudar, sem comprar. O acesso pode ser mudado com o override `sm_weaponskins_admin` em `addons/sourcemod/configs/admin_overrides.cfg`. |

## Verificando a instalação

1. No console do servidor, rode `sm exts list`. Uma das linhas precisa ser `REST in Pawn`. Se não estiver lá, o REST in Pawn não está instalado, ou não é a versão para o seu sistema.
2. Rode `sm plugins list`. Ele precisa mostrar `L4D2 - Player Statistics Sync` e os outros plugins que você copiou, nenhum marcado como falho. Quando um plugin não consegue carregar, o SourceMod o lista com o motivo, por exemplo uma extensão ou um plugin obrigatório que não está rodando.
3. Rode `sm_playstats_test`. Ele primeiro mostra o resultado da última chamada automática, depois chama o site e mostra uma das linhas abaixo.
4. No site, abra **Administração → Servidores de jogo** e o servidor. O status muda sozinho para **Conectado**, com uma lista: conexão com este site, plugin de envio de estatísticas carregado, gravação de estatísticas ativada, primeiro arquivo de estatísticas recebido (depois do primeiro mapa completo), dados da partida ao vivo chegando, e o plugin de skins quando a loja está ligada.

| Resultado do `sm_playstats_test` | Significado |
|---|---|
| `Connected to the site "..." as the game server "..."` | Está tudo certo. As linhas seguintes listam os módulos ligados no site, os plugins carregados, se a gravação de estatísticas está ligada e quantos arquivos de estatísticas estão esperando. |
| `URL not set` | `playstats_url` está vazio. |
| `Token not set` | `playstats_token` está vazio. |
| `Cannot reach ...` | O endereço não respondeu. O texto entre parênteses é o erro da conexão. |
| `Unknown identifier` | O site não tem nenhum servidor de jogo com o identificador do token. |
| `Wrong key` | O identificador existe, mas a chave não confere. |
| `Token not accepted` | O token não está no formato `<identifier>:<key>`. |
| `This game server is disabled on the site` | Um administrador desativou o servidor no site. |
| `... answered HTTP ...` | Algo responde nesse endereço, mas não é o L4D2 PlayStats Hub. |

Pelo RCON só voltam as primeiras linhas, porque a resposta do site chega depois que o comando já terminou. Use o console do servidor, ou rode o comando dentro do jogo como admin root.

Os plugins gravam um problema uma vez no log de erros do SourceMod (`addons/sourcemod/logs/errors_<date>.log`) quando ele começa, e uma linha no log do SourceMod (`addons/sourcemod/logs/L<date>.log`) quando ele é resolvido, para que um site fora do ar não encha os logs.

## Solução de problemas

| O que você vê | Causa e solução |
|---|---|
| `sm_playstats_test` é um comando desconhecido | O `l4d2_playstats_sync.smx` não está carregado. Confira se ele está em `addons/sourcemod/plugins/optional/`, e veja o erro dele em `sm plugins list`. Com o L4D2 Competitive Rework, confira `cfg/sharedplugins.cfg`. |
| `sm plugins list` diz que um plugin precisa da extensão REST in Pawn (`rip.ext`) | O REST in Pawn está faltando, ou é a versão de outro sistema. Instale o zip certo e reinicie o servidor. |
| `sm plugins list` diz que um plugin precisa do plugin `left4dhooks` | O `left4dhooks.smx` não está carregado. Instale o Left 4 DHooks Direct com os arquivos de gamedata dele. |
| `URL not set` ou `Token not set` | As linhas não rodaram. Confira se o `cfg/server.cfg` termina com `exec playstats.cfg`, se as duas linhas em `cfg/playstats.cfg` não começam com `//`, e reinicie o servidor. |
| `Unknown identifier`, `Wrong key` ou `Token not accepted` | Cole o token de novo a partir da página do servidor no site. Se a chave se perdeu, clique em **Gerar uma nova chave** ali e cole a linha nova. |
| `Cannot reach ...` com timeout ou "Couldn't resolve host" | O servidor de jogo não consegue abrir o site. Na máquina do servidor de jogo, rode `curl -I <playstats_url>`. Confira o endereço, o DNS, o firewall, e se o site está rodando. |
| `Cannot reach ...` com "SSL certificate problem" | O certificado do site não é confiável. Use um certificado de uma autoridade pública, como o Let's Encrypt; certificados autoassinados não funcionam. Se o certificado está certo, o `addons/sourcemod/configs/ripext/ca-bundle.crt` pode estar velho: troque pelo do REST in Pawn mais recente. |
| `... answered HTTP 301`, `302`, `404` ou `405` | O endereço não é o próprio site, ou ele redireciona. Use o endereço final, normalmente com `https://`: um redirecionamento pode transformar o POST do plugin em um GET. |
| Conectado, mas nenhum arquivo de estatísticas chega | Jogue um mapa completo em versus. Confira se o `l4d2_playstats` está carregado nesse modo de partida e se o `sm_playstats_test` diz que a gravação de estatísticas está ligada. |
| As partidas chegam mas não contam para o ranking | No site, abra **Administração → Modos de jogo** e veja em qual regra a configuração se encaixa. |
| Os plugins funcionam fora das partidas mas param quando um modo de partida carrega | Eles não estão em `cfg/sharedplugins.cfg` (veja [Modos de partida do L4D2 Competitive Rework](#modos-de-partida-do-l4d2-competitive-rework)). |
| O Confogl mostra "Tracked Server CVar ... changed" a cada mapa | A mesma configuração está em `cfg/playstats.cfg` e com `confogl_addcvar`. Deixe em um lugar só. |
| `!ranking` abre um painel vazio | Defina `playstats_ingame_url` com um endereço HTTP simples do site. |
| O chat mostra o endereço IP do site em vez do nome | Defina `playstats_public_url` com o endereço que os jogadores conhecem, por exemplo `https://stats.example.com`. |
| Os ícones das patentes não aparecem | Confira `patent_icon_enabled 1`. Depois, os jogadores podem não ter os sprites: confira o `sv_downloadurl` e se o `patent_icon_version` bate com os nomes dos arquivos. |
| As skins nunca aparecem | Confira `weapon_skins_enabled 1`, se o módulo está ligado em **Administração → Módulos**, se os arquivos estão no servidor (procure `not found on the server` no log de erros) e no FastDL, e se a skin foi comprada no site. |

## O que os plugins chamam

Toda requisição leva o cabeçalho `Authorization: <playstats_token>`. Os valores SteamID64 vão como texto.

| Plugin | Requisição |
|---|---|
| `l4d2_playstats_sync` | `POST /api/servers/heartbeat`, `POST /api/statistics` |
| `l4d2_gameinfo_sync` | `PUT /api/game-info/configuration`, `PUT /api/game-info/round`, `PUT /api/game-info/scoreboard`, `PUT /api/game-info/players`, `PUT /api/game-info/messages`, `POST /api/game-info/events`, `POST /api/game-info/player-connection-info`, `GET /api/game-info/server-command/dequeue`, `GET /api/external-chat` |
| `l4d2_ranking` | Nenhuma. Ele abre `ranking_motd_path` (`/` por padrão) e `ranking_commands_path` (`/ingame/commands`), cada um com `?steamid=...`, no jogo do jogador. |
| `l4d2_show_patent_icon` | `GET /api/players/patents` |
| `l4d2_weapon_skins` | `GET /api/skins`, `GET /api/players/skins`, `GET /api/players/{id}/wallet`, `POST /api/players/{id}/skins/equip` |
| `l4d2-anti-cheat` (à parte, 1.5.0 ou mais novo, com `l4d2_web_url` e `l4d2_web_secret`) | `GET /api/anti-cheat-access/server`, `PUT /api/anti-cheat-access/server/mode`, `POST /api/anti-cheat-access/server/{list}`, `DELETE /api/anti-cheat-access/server/{list}?player=<steamid>` |

O heartbeat é enviado quando os arquivos de configuração terminam de rodar, depois a cada 30 segundos enquanto o servidor não está hibernando, e logo depois que `playstats_url` ou `playstats_token` muda. Só um heartbeat fica em andamento por vez, com os timeouts padrão do REST in Pawn (10 segundos para conectar, 30 no total).

Os arquivos de estatísticas são enviados um de cada vez, dez segundos depois de cada início de rodada. Quando o site não responde, os arquivos ficam no servidor e são tentados de novo no próximo início de rodada. Um arquivo para o qual o site respondeu "ainda não está pronto" só é enviado de novo depois que muda, ou depois de 30 minutos. Um arquivo só é apagado do servidor de jogo quando o site manda.

O `l4d2_gameinfo_sync` envia os dados ao vivo a cada 5 segundos, com timeout de 3 segundos para conectar e de 5 segundos no total. Uma requisição de configuração, rodada, placar, jogadores, chat do site ou comando não é enviada enquanto a anterior do mesmo tipo está esperando, para que um site lento não as acumule. Quando um módulo está desligado no site, as requisições dele recebem `404` e os plugins as ignoram; o plugin de skins registra isso uma vez.

O código compartilhado está em `scripting/include/l4d2_playstats_hub.inc`: as duas cvars, a requisição com o token e o heartbeat.

## Compilação

Você só precisa disto para mudar os plugins. O compilador é o `spcomp`, do pacote do SourceMod (`addons/sourcemod/scripting/`). Os plugins do repositório são compilados com o **SourceMod 1.12.0.7260**; baixe essa versão em <https://github.com/alliedmodders/sourcemod/releases/tag/1.12.0.7260> e descompacte em `plugins/.sourcemod/` (ignorada pelo Git), ou aponte para ela com `SPCOMP`.

Windows:

```powershell
./build.ps1
./build.ps1 -Spcomp "C:\sourcemod\addons\sourcemod\scripting\spcomp.exe"
```

Linux e macOS:

```bash
./build.sh
SPCOMP=~/sourcemod/addons/sourcemod/scripting/spcomp64 ./build.sh
```

Os dois compilam todo arquivo `.sp` de `left4dead2/addons/sourcemod/scripting/` direto em `left4dead2/addons/sourcemod/plugins/`, e o `l4d2_playstats_sync`, o `l4d2_ranking` e o `l4d2_show_patent_icon` na pasta `optional/` dela. Faça commit dos arquivos `.smx` junto com os fontes. Os arquivos include padrão do SourceMod são lidos da pasta `include` ao lado do compilador; defina `SOURCEMOD_INCLUDE` se eles estiverem em outro lugar.

A compilação é reproduzível: os mesmos fontes e o mesmo compilador geram os mesmos bytes no Windows e no Linux. Para isso, os scripts compilam em uma pasta temporária com caminhos de include relativos, fixam a data e a hora de compilação que o SourceMod embute (`__DATE__` e `__TIME__`), gravam os plugins sem compressão (`-z0`, porque o zlib não comprime do mesmo jeito em todo sistema) e cada plugin define o tamanho da sua pilha com `#pragma dynamic` (senão a estimativa do compilador muda de uma execução para outra). Cada plugin também é compilado uma vez com `-E`, então qualquer aviso faz a compilação falhar.

`./build.sh --check` (ou `./build.ps1 -Check`) compila na pasta temporária e falha se um `.smx` do repositório está faltando, é diferente do resultado, não tem fonte, ou não está na sua pasta. O workflow `Plugins` roda essa verificação em todo push e pull request que mexe em `plugins/`, com o SourceMod 1.12.0.7260 conferido pelo SHA-256, e guarda a pasta `left4dead2` como artefato da execução. O workflow de release roda a mesma verificação e empacota a pasta `left4dead2` do repositório, este guia, a licença e o aviso em `l4d2-playstats-hub-plugins.zip`.

Regras de código: sem comentários, constantes como `#define` no topo do arquivo, e os arquivos include de terceiros ficam exatamente como vieram.

## Ainda não testado em um servidor de jogo

Os plugins compilam sem avisos e a compilação foi verificada no Windows e no Linux, mas esta versão ainda não rodou em um servidor de jogo de verdade. Até isso acontecer, trate estes pontos como comportamento esperado, não como fatos testados:

- os plugins carregam de `addons/sourcemod/plugins/` e de `addons/sourcemod/plugins/optional/` como arquivos `.smx` sem compressão (o SourceMod aceita esses arquivos, mas esta versão ainda não foi carregada por um servidor);
- o fluxo `server.cfg` → `exec playstats.cfg` → `cfg/sharedplugins.cfg` em um servidor com o L4D2 Competitive Rework;
- as mudanças desta versão: a porcentagem do tank e da witch sem o `l4d_boss_percent`, a atualização das patentes depois que as configurações rodam, e as linhas do log de erros gravadas uma vez só;
- o texto exato das mensagens do próprio SourceMod, que este guia descreve em vez de citar.

## Licença

Os plugins são distribuídos sob a [GNU General Public License, versão 3](LICENSE), como o SourceMod exige. O [NOTICE.md](NOTICE.md) lista os arquivos include de terceiros e de onde eles vêm.
