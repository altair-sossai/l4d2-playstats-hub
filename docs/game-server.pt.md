# Conectando um servidor de jogo

🌐 [English](game-server.md) | **Português** | [Español](game-server.es.md)

Como fazer um servidor de Left 4 Dead 2 enviar as partidas dele para o seu site, passo a passo. Você repete estes passos para cada servidor de jogo; um site pode receber quantos servidores você quiser. Cada configuração, comando e arquivo está descrito em detalhes no [guia dos plugins](../plugins/README.pt.md).

## Conteúdo

- [O que o servidor de jogo precisa](#o-que-o-servidor-de-jogo-precisa)
- [Passo 1: cadastre o servidor no site](#passo-1-cadastre-o-servidor-no-site)
- [Passo 2: instale o REST in Pawn](#passo-2-instale-o-rest-in-pawn)
- [Passo 3: copie os plugins](#passo-3-copie-os-plugins)
- [Passo 4: defina o endereço e o token](#passo-4-defina-o-endereço-e-o-token)
- [Passo 5: carregue os plugins nos seus modos de partida](#passo-5-carregue-os-plugins-nos-seus-modos-de-partida)
- [Passo 6: verifique a conexão](#passo-6-verifique-a-conexão)
- [Opcional: skins de armas e ícones de patente](#opcional-skins-de-armas-e-ícones-de-patente)
- [Opcional: o plugin do Anti-Cheat](#opcional-o-plugin-do-anti-cheat)
- [O ranking dentro do jogo](#o-ranking-dentro-do-jogo)
- [O que cada status significa](#o-que-cada-status-significa)
- [Atualizando os plugins](#atualizando-os-plugins)
- [Vindo da configuração antiga](#vindo-da-configuração-antiga)
- [Se algo der errado](#se-algo-der-errado)

## O que o servidor de jogo precisa

Os plugins são feitos para servidores baseados no [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), como os servidores ZoneMod. O Competitive Rework já traz quase tudo o que eles precisam:

| Requisito | Por quê | Vem com o Competitive Rework |
|---|---|---|
| SourceMod 1.12 ou mais novo | Executa os plugins. | Sim |
| Extensão [REST in Pawn](https://github.com/ErikMinekus/sm-ripext/releases) | Permite que os plugins chamem o site. | **Não: passo 2** |
| [Left 4 DHooks Direct](https://github.com/SilvDev/Left4DHooks/releases) (`left4dhooks`) | Usado pelos plugins da partida ao vivo, do ranking e das patentes. | Sim |
| Plugin `l4d2_playstats` | Grava o arquivo de estatísticas de cada mapa. | Sim |
| `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod`, `l4d2_skill_detect` | Opcionais: mais detalhes na partida ao vivo. | Sim |

Em um servidor sem o Competitive Rework, instale antes o SourceMod 1.12 e o Left 4 DHooks Direct; as estatísticas precisam do `l4d2_playstats`, que só vem com o Competitive Rework. A lista completa, com links, está nos [requisitos do guia dos plugins](../plugins/README.pt.md#requisitos).

## Passo 1: cadastre o servidor no site

1. Faça login no site como administrador.
2. Abra **Administração → Servidores de jogo**.
3. Digite um nome e clique em **Criar**.

O site mostra duas linhas com o endereço do site e o token deste servidor:

```text
playstats_url "https://stats.example.com"
playstats_token "my-server:3f9c…"
```

Copie-as agora. A chave dentro do token aparece só uma vez. Se você perdê-la, clique em **Gerar uma nova chave** na mesma página; a chave antiga para de funcionar na hora.

## Passo 2: instale o REST in Pawn

1. Baixe o `sm-ripext-<version>-linux.zip` (servidor Linux) ou o `sm-ripext-<version>-windows.zip` (servidor Windows) mais recente nas [releases do REST in Pawn](https://github.com/ErikMinekus/sm-ripext/releases).
2. Extraia na pasta `left4dead2` do servidor, a que tem o `gameinfo.txt`. Ele adiciona `addons/sourcemod/extensions/rip.ext.so` (ou `.dll`) e `addons/sourcemod/configs/ripext/ca-bundle.crt`.

Pule este passo se o `sm exts list`, no console do servidor, já mostra `REST in Pawn`.

## Passo 3: copie os plugins

Pegue os plugins de um destes dois jeitos:

- baixe o `l4d2-playstats-hub-plugins.zip` na [página de releases](https://github.com/altair-sossai/l4d2-playstats-hub/releases) e extraia, ou
- baixe ou clone [este repositório](https://github.com/altair-sossai/l4d2-playstats-hub) e abra a pasta `plugins` dele.

Os dois têm uma pasta chamada `left4dead2` com a mesma estrutura da pasta do jogo do servidor. **Copie tudo o que está dentro dessa pasta `left4dead2` para a pasta `left4dead2` do servidor.** Cada arquivo cai no seu lugar:

| Arquivo | O que é |
|---|---|
| `addons/sourcemod/plugins/*.smx` | Os cinco plugins. |
| `addons/sourcemod/scripting/` | Os fontes deles, só para compilar. |
| `addons/sourcemod/translations/` | Os textos do chat. |
| `addons/sourcemod/configs/weapon_skins.cfg` | As armas e skins do plugin de skins. |
| `cfg/playstats.cfg` | Suas configurações. Você o edita no próximo passo. |

Mantenha os plugins na própria `addons/sourcemod/plugins/`, não em `optional/`, para que eles carreguem quando o servidor inicia e o site veja o servidor mesmo quando nenhuma partida está sendo jogada.

O `l4d2_playstats_sync.smx` é o único de que todo servidor precisa. Apague o `.smx` dos plugins que você não quer:

| Plugin | O que faz | Módulo do site |
|---|---|---|
| `l4d2_playstats_sync` | Envia os arquivos de estatísticas e avisa o site que o servidor está no ar. | Sempre necessário |
| `l4d2_gameinfo_sync` | Envia a partida ao vivo: placar, jogadores, eventos e chat. | Servidor ao vivo |
| `l4d2_ranking` | Abre o ranking dentro do jogo com `!ranking`. | Ranking |
| `l4d2_show_patent_icon` | Mostra o ícone da patente de cada jogador no jogo. | Ranking |
| `l4d2_weapon_skins` | Aplica as skins de armas que os jogadores têm. | Loja de skins de armas |

## Passo 4: defina o endereço e o token

1. Abra o `cfg/playstats.cfg` no servidor.
2. Cole as duas linhas do passo 1 onde o arquivo diz `Paste the two lines here`.
3. Abra o `cfg/server.cfg` e adicione esta linha no fim:

   ```text
   exec playstats.cfg
   ```

Isso é tudo o que você precisa mudar. O resto do `cfg/playstats.cfg` lista todas as outras configurações dos cinco plugins, desligadas com `//` e com o valor padrão; cada uma tem uma explicação acima.

O `cfg/playstats.cfg` também liga `sm_stats_writestats 1`, que faz o `l4d2_playstats` gravar os arquivos de estatísticas. O padrão é `0`, e os modos de partida ZoneMod do Competitive Rework não o mudam. Se os seus modos de partida já o definem com `confogl_addcvar`, apague essa linha do `cfg/playstats.cfg`.

Cada configuração fica em um lugar só: no `cfg/playstats.cfg` as que são iguais em todos os modos de partida, ou com `confogl_addcvar` em um modo de partida a que precisa ser diferente. Nunca defina `playstats_url` ou `playstats_token` com `confogl_addcvar`: o Confogl mostra no chat o valor antigo e o novo das configurações que ele acompanha, e os jogadores veriam o token.

## Passo 5: carregue os plugins nos seus modos de partida

Quando um modo de partida do Competitive Rework carrega, ele descarrega todos os plugins e carrega só os que estão nas listas dele. A lista compartilhada por todos os modos é o `cfg/sharedplugins.cfg`. Adicione estas linhas a ele, deixando de fora os plugins que você apagou:

```text
sm plugins load l4d2_playstats_sync.smx
sm plugins load l4d2_gameinfo_sync.smx
sm plugins load l4d2_ranking.smx
sm plugins load l4d2_show_patent_icon.smx
sm plugins load l4d2_weapon_skins.smx
```

Depois reinicie o servidor de jogo.

As estatísticas são enviadas de todos os modos. No site, **Administração → Modos de jogo** decide quais modos contam para o ranking.

## Passo 6: verifique a conexão

No site, a página do servidor de jogo se atualiza sozinha. Em alguns segundos ela deve mostrar **Conectado**, com uma lista do que foi encontrado:

- Conexão com este site
- Plugin de envio de estatísticas carregado
- Gravação de estatísticas ativada (ela liga quando um modo de partida carrega o `l4d2_playstats`)
- Primeiro arquivo de estatísticas recebido (depois do primeiro mapa completo)
- Dados da partida ao vivo chegando (quando o módulo de servidor ao vivo está ligado)
- Plugin de skins de armas carregado (quando o módulo da loja de skins está ligado)

Você também pode verificar pelo servidor de jogo. Digite isto no console dele:

```text
sm exts list
sm plugins list
sm_playstats_test
```

O `sm exts list` precisa mostrar `REST in Pawn`, e o `sm plugins list` precisa mostrar os plugins que você copiou, nenhum marcado como falho. O `sm_playstats_test` responde com o nome do servidor e do site, os módulos ligados e os plugins carregados, ou diz exatamente o que está errado. O [guia dos plugins](../plugins/README.pt.md#verificando-a-instalação) explica cada resposta.

## Opcional: skins de armas e ícones de patente

Os ícones de patente e as skins de armas são arquivos que não fazem parte do jogo, então os jogadores os baixam do seu servidor. Eles estão em [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

1. Copie as pastas `models/` e `materials/` dele para a pasta `left4dead2` do servidor.
2. Coloque `models/`, `materials/` e o conteúdo de `fastdl/` em um servidor web que os jogadores consigam acessar (um FastDL), com a mesma estrutura da pasta do jogo.
3. No `cfg/server.cfg`, defina `sm_cvar sv_allowdownload 1` e `sm_cvar sv_downloadurl "<address of that folder>"`.
4. No `cfg/playstats.cfg`, ligue as skins: apague o `//` na frente de `weapon_skins_enabled` e mude para `"1"`.
5. Reinicie o servidor enquanto ele está vazio.

Os detalhes, e como verificar se os arquivos foram encontrados, estão em [Arquivos de skins de armas e de patentes](../plugins/README.pt.md#arquivos-de-skins-de-armas-e-de-patentes).

## Opcional: o plugin do Anti-Cheat

O plugin de anti-cheat (`l4d2-anti-cheat.smx`, versão 1.5.0 ou mais nova) não faz parte deste pacote: ele vem com o programa Anti-Cheat e o serviço próprio dele. Conectado ao site, ele lê do site quem precisa usar o Anti-Cheat, e as alterações feitas no jogo com `!aac config` são salvas no site. Veja [Quem precisa usar o Anti-Cheat](manual/anti-cheat.pt.md#quem-precisa-usar-o-anti-cheat).

1. No site, ligue o módulo **Anti-Cheat** em **Administração → Módulos**.
2. Adicione estas duas linhas ao `cfg/playstats.cfg`, com o mesmo endereço e o mesmo token de `playstats_url` e `playstats_token`:

   ```text
   l4d2_web_url "https://stats.example.com"
   l4d2_web_secret "my-server:3f9c…"
   ```

3. Escolha o modo que o servidor de jogo usa até que um seja salvo no site, com `anti_cheat_mode` no `shared_cvars.cfg` dos seus modos de partida, por exemplo `confogl_addcvar anti_cheat_mode 1`.
4. Reinicie o servidor enquanto ele está vazio.

| Configuração | O que significa |
|---|---|
| `l4d2_web_url` | O endereço deste site. O plugin chama `<endereço>/api/anti-cheat-access/server`. |
| `l4d2_web_secret` | O token deste servidor de jogo, o mesmo valor de `playstats_token`. Cada servidor de jogo tem o seu. |
| `anti_cheat_mode` | `0`: todos precisam usar o Anti-Cheat, exceto os jogadores da whitelist. `1`: só os jogadores da lista obrigatória precisam usá-lo (o padrão do plugin). O modo salvo em **Administração → Configurações do Anti-Cheat** vale mais; este valor só conta até que um modo seja salvo lá. |

Nunca defina `l4d2_web_secret` com `confogl_addcvar`, pelo mesmo motivo do `playstats_token`. As outras configurações do plugin, como o endereço e o token do serviço próprio dele, não mudam.

Depois de uma alteração no site, o `l4d2_gameinfo_sync` roda `sm_aac sync` no servidor de jogo em segundos. Sem o módulo de servidor ao vivo, o plugin lê o site sozinho a cada minuto. Para verificar a conexão, digite `sm_aac sync` e depois `sm_aac list` no console do servidor de jogo, ou pergunte ao site a partir dessa máquina:

```text
curl -H "Authorization: my-server:3f9c…" https://stats.example.com/api/anti-cheat-access/server
```

A resposta traz o modo (`null` até que um seja salvo) e as duas listas. `401` quer dizer que o token está errado, e `404` que o módulo Anti-Cheat está desligado.

## O ranking dentro do jogo

O `!ranking` abre uma versão simples do ranking no navegador do próprio jogo. Esse navegador é de 2012 e não se espera que ele abra sites HTTPS atuais, então dê a ele um endereço HTTP simples do seu site no `cfg/playstats.cfg`:

```text
playstats_ingame_url "http://203.0.113.10:5080"
```

Quando `playstats_ingame_url` está vazio, o plugin usa `playstats_url`. [Instalando em uma VPS Linux](install-linux.pt.md#o-endereço-usado-dentro-do-jogo) explica qual endereço usar.

## O que cada status significa

| Status | Significado | O que fazer |
|---|---|---|
| Aguardando a primeira conexão | O servidor nunca chegou ao site. | Confira o `cfg/playstats.cfg` e a linha `exec playstats.cfg`, e se o `l4d2_playstats_sync.smx` está carregado. Rode `sm_playstats_test`. |
| Conectado | Um heartbeat chegou nos últimos segundos. | Nada. |
| Offline | O servidor já foi visto, mas não agora. | Normal enquanto o servidor de jogo está desligado, ou vazio e hibernando. |
| Chave inválida | O identificador chegou com a chave errada. | Cole o token de novo, ou gere uma nova chave no site. |

## Atualizando os plugins

Copie a nova pasta `left4dead2` por cima da antiga, **exceto o `cfg/playstats.cfg`**, que tem o seu endereço e o seu token. Todos os outros arquivos podem ser substituídos; veja [Atualização](../plugins/README.pt.md#atualização) para as exceções e um comando que pula esse arquivo. Depois reinicie o servidor enquanto ele está vazio.

## Vindo da configuração antiga

Se o servidor de jogo usava os plugins anteriores, remova as configurações deles, senão elas continuam apontando para os endereços antigos:

| Configuração antiga | Agora |
|---|---|
| `playstats_url` apontando para a API antiga | O endereço deste site |
| `playstats_access_token` | `playstats_token` |
| `l4d2_web_url`, `l4d2_web_secret` | Os plugins do PlayStats não as leem mais: eles usam `playstats_url` e `playstats_token`. Só o plugin de anti-cheat ainda as lê; veja [Opcional: o plugin do Anti-Cheat](#opcional-o-plugin-do-anti-cheat). |
| `ranking_url` | `playstats_ingame_url`, só se o jogo precisar de outro endereço |

Procure por elas no `cfg/server.cfg`, no seu arquivo de segredos e no `shared_cvars.cfg` de cada modo de partida. As configurações antigas definidas com `confogl_addcvar playstats_url` também precisam sair, para que o `cfg/playstats.cfg` seja o único lugar do endereço e do token. Substitua os arquivos `.smx` antigos, inclusive os de `addons/sourcemod/plugins/optional/`, pelos novos, para que exista só uma cópia de cada plugin.

## Se algo der errado

| O que você vê | O que fazer |
|---|---|
| `sm_playstats_test` é um comando desconhecido | O `l4d2_playstats_sync.smx` não está carregado. Confira a pasta e rode `sm plugins list` para ver o erro dele. |
| Um plugin precisa do REST in Pawn | Faça o passo 2 de novo com o zip do sistema do servidor. |
| "URL not set" ou "Token not set" | As linhas não rodaram. Confira se o `cfg/server.cfg` termina com `exec playstats.cfg` e se as suas duas linhas no `cfg/playstats.cfg` não começam com `//`. |
| "Unknown identifier", "Wrong key" ou "Token not accepted" | Cole o token de novo, ou gere uma nova chave no site. |
| "Cannot reach ..." | O servidor de jogo não consegue abrir o site. Teste o endereço naquela máquina com `curl -I`, confira o firewall, e veja a [Solução de problemas](../plugins/README.pt.md#solução-de-problemas) para erros de HTTPS. |
| Funciona fora das partidas, para quando um modo de partida carrega | Faça o passo 5. |
| Conectado, mas nenhum arquivo de estatísticas chega | Jogue um mapa completo em versus. Confira se o `sm_playstats_test` diz que a gravação de estatísticas está ligada durante a partida. |
| As partidas chegam mas não contam para o ranking | No site, abra **Administração → Modos de jogo** e veja em qual regra a configuração se encaixa. |
| `!ranking` abre um painel vazio | Defina `playstats_ingame_url` com um endereço HTTP simples do site. |
