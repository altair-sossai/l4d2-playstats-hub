# Plugins do servidor de jogo

🌐 [English](README.md) | **Português** | [Español](README.es.md)

Os plugins de SourceMod que conectam um servidor de Left 4 Dead 2 ao L4D2 PlayStats Hub. Eles usam um único endereço e um único token para tudo.

## Conteúdo

- [Os plugins](#os-plugins)
- [Requisitos](#requisitos)
- [Instalando](#instalando)
- [Configurações](#configurações)
- [Comandos](#comandos)
- [Verificando a conexão](#verificando-a-conexão)
- [Carregando os plugins em um modo de partida do Confogl ou do ZoneMod](#carregando-os-plugins-em-um-modo-de-partida-do-confogl-ou-do-zonemod)
- [Migrando da configuração antiga](#migrando-da-configuração-antiga)
- [O que os plugins chamam](#o-que-os-plugins-chamam)
- [Compilando](#compilando)
- [Licença](#licença)

## Os plugins

| Plugin | O que faz | Módulo do site |
|---|---|---|
| `l4d2_playstats_sync` | Avisa ao site a cada 30 segundos que o servidor está ativo e envia os arquivos de estatísticas gravados pelo `l4d2_playstats`. Também tem o comando que verifica a conexão. | Sempre ativo |
| `l4d2_gameinfo_sync` | Envia a partida ao vivo: configuração, rodada, placar, jogadores, eventos e chat. Também traz o chat do site para dentro do jogo e executa os comandos enviados pelo site. | Servidor ao vivo, Chat do site, Comandos remotos |
| `l4d2_ranking` | Abre a página do ranking dentro do jogo com `!ranking`, e sozinho para os jogadores que entram antes de uma partida começar. | Ranking e estatísticas |
| `l4d2_show_patent_icon` | Mostra o ícone da patente de cada jogador acima da cabeça antes de uma partida começar. | Ranking e estatísticas |
| `l4d2_weapon_skins` | Aplica as skins de armas que os jogadores compraram na loja. | Loja de skins de armas |

O `l4d2_playstats_sync` é o plugin de que todo servidor precisa. Os outros são opcionais, e cada um funciona sem os demais.

## Requisitos

- Um servidor dedicado de Left 4 Dead 2 com o [SourceMod](https://www.sourcemod.net) 1.12 ou mais recente.
- A extensão [REST in Pawn](https://github.com/ErikMinekus/sm-ripext). Os cinco plugins precisam dela.
- O [Left 4 DHooks Direct](https://github.com/SilvDev/Left4DHooks) (`left4dhooks`). O `l4d2_gameinfo_sync`, o `l4d2_ranking` e o `l4d2_show_patent_icon` precisam dele.
- O plugin `l4d2_playstats` do [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), com `sm_stats_writestats 1`. Ele grava os arquivos de estatísticas que o `l4d2_playstats_sync` envia.
- Um servidor de jogo cadastrado no site. O site entrega a ele um identificador e uma chave.

Os plugins são voltados para servidores baseados no L4D2 Competitive Rework. O `l4d2_gameinfo_sync` usa `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod` e `l4d2_skill_detect` quando eles estão carregados, e continua funcionando sem eles.

Dois plugins precisam de arquivos que os jogadores baixam do servidor (FastDL, `sv_downloadurl`). Os arquivos estão em [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets):

- O `l4d2_show_patent_icon` precisa dos sprites `materials/sprites/patent_<level>_v<version>.vmt` e `.vtf`.
- O `l4d2_weapon_skins` precisa dos modelos e materiais listados em `configs/weapon_skins.cfg`.

## Instalando

1. Baixe os plugins compilados (`.smx`) nas [versões](https://github.com/altair-sossai/l4d2-playstats-hub/releases), ou [compile-os](#compilando).
2. Copie os arquivos para o servidor de jogo, dentro da pasta `left4dead2`:

   | De | Para |
   |---|---|
   | `l4d2_playstats_sync.smx` | `addons/sourcemod/plugins/` |
   | Os outros arquivos `.smx` | `addons/sourcemod/plugins/`, ou `addons/sourcemod/plugins/optional/` quando um modo de partida os carrega |
   | `translations/` | `addons/sourcemod/translations/`, mantendo as pastas de idioma |
   | `configs/weapon_skins.cfg` | `addons/sourcemod/configs/` |
   | `cfg/playstats.cfg.example` | `cfg/playstats.cfg` |

3. Abra o `cfg/playstats.cfg` e preencha `playstats_url` e `playstats_token` com os valores que o site mostra na página do servidor de jogo.
4. Adicione `exec playstats.cfg` ao `cfg/server.cfg`. Você também pode colar as duas linhas diretamente no `cfg/server.cfg`.
5. Reinicie o servidor de jogo ou troque o mapa.
6. Execute `sm_playstats_test` no console do servidor.

Mantenha o `l4d2_playstats_sync.smx` na própria pasta `addons/sourcemod/plugins/`. Assim ele é carregado quando o servidor inicia, as duas cvars já existem quando o `server.cfg` é executado, e o site enxerga o servidor mesmo fora de uma partida.

## Configurações

Compartilhadas por todos os plugins:

| Cvar | Padrão | Significado |
|---|---|---|
| `playstats_url` | vazio | Endereço do site, sem barra no final. Exemplo: `https://stats.example.com`. |
| `playstats_token` | vazio | Token deste servidor de jogo, no formato `<identifier>:<key>`. Ele é protegido, então o jogo nunca mostra o valor dele aos jogadores. |

`l4d2_ranking`:

| Cvar | Padrão | Significado |
|---|---|---|
| `playstats_ingame_url` | vazio | Endereço que o jogo abre para as páginas dentro do jogo. Quando está vazio, o `playstats_url` é usado. |
| `ranking_show_motd_panel` | `1` | `0` impede que o plugin abra a página. |

A página que ele abre é `<address>/ingame?steamid=<SteamID64 of the player>`.

O navegador dentro do jogo é o Chromium 18, de 2012, e pode não abrir um site HTTPS moderno. Se o painel ficar em branco, sirva o site por HTTP simples em outro endereço ou em outra porta e coloque esse endereço em `playstats_ingame_url`. Mantenha o endereço curto. Veja [docs/in-game-browser.pt.md](../docs/in-game-browser.pt.md).

`l4d2_show_patent_icon`:

| Cvar | Padrão | Significado |
|---|---|---|
| `patent_icon_version` | `1` | Versão no nome dos arquivos de sprite. |
| `patent_icon_max_level` | `15` | Nível de patente mais alto que tem um sprite. |

`l4d2_gameinfo_sync`:

| Cvar | Padrão | Significado |
|---|---|---|
| `gameinfo_related_accounts_chat` | `1` | Avisa no chat quando um jogador se conecta a partir do mesmo endereço IP de outra conta já vista antes. |
| `gameinfo_boomer_vomit_min_survivors` | `3` | Número mínimo de sobreviventes que um boomer precisa atingir para que o evento seja enviado. |

O `l4d2_gameinfo_sync` envia ao site o endereço IP de cada jogador que se conecta, e o site o usa para encontrar contas que compartilham um mesmo endereço.

`l4d2_weapon_skins`:

| Cvar | Padrão | Significado |
|---|---|---|
| `weapon_skins_enabled` | `0` | Ativa as skins. |
| `weapon_skins_kick_no_downloads` | `0` | Expulsa os jogadores cujo jogo se recusa a baixar os arquivos do servidor. |
| `weapon_skins_pile_radius` | `120.0` | Raio usado para encontrar armas primárias largadas perto umas das outras. |
| `weapon_skins_pile_max` | `3` | Número máximo de armas primárias largadas mantidas dentro do raio. `0` desativa. |
| `weapon_skins_preview_all` | `0` | Modo de teste: todas as skins são baixadas no próximo mapa, e os administradores root podem usar qualquer uma delas. |

## Comandos

| Comando | Quem | O que faz |
|---|---|---|
| `sm_playstats_test` | Console do servidor, administradores root | Chama o site na hora e mostra o resultado. |
| `sm_ranking` (`!ranking`) | Jogadores | Abre a página do ranking dentro do jogo. |
| `sm_skin`, `sm_skins` (`!skin`) | Jogadores | Escolhe uma skin de arma. Digite `!skin` dentro do jogo para ver as opções. |

## Verificando a conexão

Execute `sm_playstats_test` no console do servidor. Primeiro ele mostra o resultado da última chamada automática, depois chama o site e mostra um destes resultados:

| Resultado | Significado |
|---|---|
| `Connected to the site "..." as the game server "..."` | Está tudo certo. As linhas seguintes listam os módulos ativados, os plugins carregados, se a gravação de estatísticas está ativada e quantos arquivos de estatísticas estão aguardando. |
| `URL not set` | O `playstats_url` está vazio. |
| `Token not set` | O `playstats_token` está vazio. |
| `Cannot reach ...` | O endereço não respondeu. Verifique o endereço, a rede e se o site está rodando. |
| `Unknown identifier` | O site não tem nenhum servidor de jogo com o identificador que está no token. |
| `Wrong key` | O identificador existe, mas a chave não confere. Gere uma nova chave no site. |
| `Token not accepted` | O token não está no formato `<identifier>:<key>`. |
| `This game server is disabled on the site` | Um administrador desativou o servidor no site. |
| `... answered HTTP ...` | O endereço responde, mas não é o L4D2 PlayStats Hub. Confira o `playstats_url`. |

Pelo RCON só as primeiras linhas voltam, porque a resposta do site chega depois que o comando já retornou. Use o console do servidor, ou execute o comando dentro do jogo como administrador root.

Os mesmos problemas são gravados uma vez no log de erros do SourceMod (`addons/sourcemod/logs/errors_*.log`) quando começam.

No site, abra a área de administração, depois os servidores de jogo, depois o servidor. O status dele mostra Aguardando, Online, Offline ou Chave inválida, e uma lista de verificação: conexão, plugin de envio de estatísticas carregado, gravação de estatísticas ativada, primeiro arquivo de estatísticas recebido, dados ao vivo chegando e o plugin de skins quando a loja está ativada.

## Carregando os plugins em um modo de partida do Confogl ou do ZoneMod

Quando um modo de partida inicia, o Confogl descarrega todos os plugins e carrega apenas os que estão listados nos arquivos indicados em `confogl_match_execcfg_plugins`. Por isso, cada plugin que você quer durante uma partida precisa de uma linha ali.

1. Na lista de plugins do modo de partida, por exemplo `cfg/cfgogl/zonemod/confogl_plugins.cfg`, adicione estas linhas antes de `sm plugins load confoglcompmod.smx`:

   ```
   sm plugins load l4d2_playstats_sync.smx
   sm plugins load optional/l4d2_gameinfo_sync.smx
   sm plugins load optional/l4d2_ranking.smx
   sm plugins load optional/l4d2_show_patent_icon.smx
   sm plugins load optional/l4d2_weapon_skins.smx
   ```

   Deixe de fora os que você não usa. O próprio `l4d2_playstats` também precisa estar na lista: `sm plugins load optional/l4d2_playstats.smx`.

2. Nas cvars do modo de partida, por exemplo `cfg/cfgogl/zonemod/zonemod.cfg`, ative a gravação de estatísticas e defina as outras cvars dos plugins que você quiser:

   ```
   confogl_addcvar sm_stats_writestats 1
   confogl_addcvar ranking_show_motd_panel 1
   confogl_addcvar weapon_skins_enabled 1
   ```

3. Mantenha `playstats_url` e `playstats_token` no `cfg/server.cfg` ou no `cfg/playstats.cfg`, não no modo de partida. Os valores delas permanecem enquanto os plugins são descarregados e carregados de novo. Não use `confogl_addcvar` para elas, porque o Confogl devolve ao valor antigo uma cvar que ele acompanha quando o modo de partida termina.

## Migrando da configuração antiga

Antes, os plugins conversavam com uma API e um site separados. Se o servidor tem essa configuração:

- Remova `playstats_access_token`, `l4d2_web_url`, `l4d2_web_secret` e `ranking_url` de todos os arquivos `.cfg`. Essas cvars não existem mais.
- Aponte `playstats_url` para o novo site e defina `playstats_token`.
- Se o painel do ranking usava um endereço HTTP simples em `ranking_url`, coloque esse tipo de endereço em `playstats_ingame_url`.
- Substitua os cinco arquivos `.smx` e os arquivos de tradução.

## O que os plugins chamam

Toda requisição leva o cabeçalho `Authorization: <playstats_token>`. Os valores de SteamID64 trafegam como strings.

| Plugin | Requisição |
|---|---|
| `l4d2_playstats_sync` | `POST /api/servers/heartbeat`, `POST /api/statistics` |
| `l4d2_gameinfo_sync` | `PUT /api/game-info/configuration`, `PUT /api/game-info/round`, `PUT /api/game-info/scoreboard`, `PUT /api/game-info/players`, `PUT /api/game-info/messages`, `POST /api/game-info/events`, `POST /api/game-info/player-connection-info`, `GET /api/game-info/server-command/dequeue`, `GET /api/external-chat` |
| `l4d2_ranking` | Nenhuma. Ele abre `/ingame?steamid=...` no jogo do jogador. |
| `l4d2_show_patent_icon` | `GET /api/players/patents` |
| `l4d2_weapon_skins` | `GET /api/skins`, `GET /api/players/skins`, `GET /api/players/{id}/wallet`, `POST /api/players/{id}/skins/equip` |

O heartbeat é enviado quando os arquivos de configuração terminam de ser executados, depois a cada 30 segundos enquanto o servidor não está hibernando, e logo após uma alteração em `playstats_url` ou `playstats_token`.

Os arquivos de estatísticas são enviados um de cada vez, dez segundos depois do início de cada rodada. Um arquivo para o qual o site respondeu "ainda não está pronto" só é enviado de novo depois que ele muda, ou depois de 30 minutos. Um arquivo só é apagado do servidor de jogo quando o site diz que pode.

O código compartilhado está em `scripting/include/l4d2_playstats_hub.inc`: as duas cvars, a requisição com o token e o heartbeat.

## Compilando

Você precisa do compilador do SourceMod 1.12, o `spcomp`, que vem no pacote do SourceMod em `addons/sourcemod/scripting`. Os arquivos de include de terceiros já estão em `scripting/include`.

Windows:

```powershell
./build.ps1
```

Linux e macOS:

```bash
./build.sh
```

Os dois compilam todos os arquivos `.sp` de `scripting/` para `compiled/`, que o Git ignora.

Os scripts procuram o compilador no `PATH`, e o `build.ps1` também procura em `C:\Program Files\SourceMod`. Para indicar onde ele está, defina `SPCOMP`, ou passe `-Spcomp` para o `build.ps1`:

```powershell
./build.ps1 -Spcomp "C:\sourcemod\addons\sourcemod\scripting\spcomp.exe"
```

```bash
SPCOMP=~/sourcemod/addons/sourcemod/scripting/spcomp64 ./build.sh
```

Os arquivos de include padrão do SourceMod são lidos da pasta `include` que fica ao lado do compilador. Defina `SOURCEMOD_INCLUDE` se eles estiverem em outro lugar.

Todo push e todo pull request que mexe em `plugins/` compila os plugins no GitHub e guarda os arquivos `.smx` como um artefato da execução.

Regras de código: sem comentários, constantes como `#define` no topo do arquivo, e os arquivos de include de terceiros ficam exatamente como vieram.

## Licença

Os plugins são distribuídos sob a [GNU General Public License, versão 3](LICENSE), como o SourceMod exige. O [NOTICE.md](NOTICE.md) lista os arquivos de include de terceiros e de onde eles vêm.
