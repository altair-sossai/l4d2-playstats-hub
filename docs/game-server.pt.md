# Conectando um servidor de jogo

🌐 [English](game-server.md) | **Português** | [Español](game-server.es.md)

Como fazer um servidor de Left 4 Dead 2 enviar as partidas dele para o seu site. Você repete estes passos para cada servidor de jogo; um site pode receber quantos servidores você quiser.

## Conteúdo

- [O que o servidor de jogo precisa](#o-que-o-servidor-de-jogo-precisa)
- [Passo 1: cadastre o servidor no site](#passo-1-cadastre-o-servidor-no-site)
- [Passo 2: copie os plugins](#passo-2-copie-os-plugins)
- [Passo 3: cole as duas linhas](#passo-3-cole-as-duas-linhas)
- [Passo 4: verifique a conexão](#passo-4-verifique-a-conexão)
- [Passo 5: carregue os plugins nos seus modos de partida](#passo-5-carregue-os-plugins-nos-seus-modos-de-partida)
- [O ranking dentro do jogo](#o-ranking-dentro-do-jogo)
- [O que cada status significa](#o-que-cada-status-significa)
- [Vindo da configuração antiga](#vindo-da-configuração-antiga)
- [Se algo der errado](#se-algo-der-errado)

## O que o servidor de jogo precisa

Os plugins são feitos para servidores baseados no [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), como os servidores ZoneMod.

| Requisito | Por quê |
|---|---|
| SourceMod 1.12 | Executa os plugins. |
| Extensão [REST in Pawn](https://github.com/ErikMinekus/sm-ripext) | Permite que os plugins chamem o site. |
| Plugin `l4d2_playstats` | Vem com o L4D2 Competitive Rework. Ele grava o arquivo de estatísticas de cada mapa. |
| `sm_stats_writestats 1` | Diz ao `l4d2_playstats` para gravar esses arquivos. Os modos de partida do L4D2 Competitive Rework já definem isso. |

## Passo 1: cadastre o servidor no site

1. Faça login no site como administrador.
2. Abra **Administração → Servidores de jogo**.
3. Digite um nome e clique em **Criar**.

O site mostra duas linhas com o endereço do site e o token deste servidor:

```text
playstats_url "https://stats.example.com"
playstats_token "my-server:3f9c…"
```

Copie-as agora. A chave que fica dentro do token é mostrada uma única vez. Se você perdê-la, clique em **Gerar uma nova chave** na mesma página.

## Passo 2: copie os plugins

Baixe o `l4d2-playstats-hub-plugins.zip` na [página de versões](https://github.com/altair-sossai/l4d2-playstats-hub/releases). As pastas dentro dele correspondem às pastas do servidor de jogo, então extraia o arquivo por cima da pasta `left4dead2`.

| Arquivo | Para onde vai |
|---|---|
| `l4d2_playstats_sync.smx` | `addons/sourcemod/plugins/` |
| Os outros arquivos `.smx` | `addons/sourcemod/plugins/`, ou `plugins/optional/` se um modo de partida os carrega |
| `translations/` | `addons/sourcemod/translations/` |
| `configs/weapon_skins.cfg` | `addons/sourcemod/configs/` |

Mantenha o `l4d2_playstats_sync.smx` na própria pasta `plugins/`, não em uma subpasta. Ele precisa ser carregado quando o servidor inicia, para que o site enxergue o servidor mesmo quando nenhuma partida está sendo jogada.

| Plugin | O que faz | Módulo do site |
|---|---|---|
| `l4d2_playstats_sync` | Envia os arquivos de estatísticas e avisa ao site que o servidor está ativo. | Sempre necessário |
| `l4d2_ranking` | Abre o ranking dentro do jogo com `!ranking`. | Ranking |
| `l4d2_show_patent_icon` | Mostra o ícone da patente de cada jogador dentro do jogo. | Ranking |
| `l4d2_gameinfo_sync` | Envia a partida ao vivo: placar, jogadores, eventos e chat. | Servidor ao vivo |
| `l4d2_weapon_skins` | Aplica as skins de armas que os jogadores possuem. | Loja de skins de armas |

Você só precisa dos plugins dos módulos que ativou.

## Passo 3: cole as duas linhas

Abra o `cfg/server.cfg` no servidor de jogo e cole no final as duas linhas do passo 1.

Reinicie o servidor de jogo ou troque o mapa.

## Passo 4: verifique a conexão

No site, a página do servidor de jogo se atualiza sozinha. Em poucos segundos ela deve mostrar **Conectado**, com uma lista do que foi encontrado:

- Conexão com este site
- Plugin de envio de estatísticas carregado
- Gravação de estatísticas ativada
- Primeiro arquivo de estatísticas recebido (depois do primeiro mapa completo)

Você também pode verificar a partir do servidor de jogo. Digite isto no console dele:

```text
sm_playstats_test
```

Ele responde com o nome do servidor e o do site, ou diz exatamente o que está errado.

## Passo 5: carregue os plugins nos seus modos de partida

Os modos de partida do L4D2 Competitive Rework descarregam e carregam plugins quando iniciam. Se você colocou um plugin em `plugins/optional/`, adicione-o à lista de plugins de cada modo em que ele deve rodar, por exemplo em `cfg/cfgogl/zonemod/confogl_plugins.cfg`:

```text
sm plugins load optional/l4d2_ranking.smx
sm plugins load optional/l4d2_show_patent_icon.smx
```

As configurações desses plugins vão no mesmo modo, com `confogl_addcvar`. [O guia dos plugins](../plugins/README.pt.md) lista todas as configurações.

As estatísticas são enviadas a partir de todos os modos. No site, **Administração → Modos de jogo** decide quais modos contam para o ranking.

## O ranking dentro do jogo

O `!ranking` abre uma versão simples do ranking no navegador do próprio jogo. Esse navegador é de 2012 e não se espera que ele abra os sites HTTPS atuais, então informe a ele um endereço HTTP simples do seu site:

```text
confogl_addcvar playstats_ingame_url "http://203.0.113.10:5080"
```

Quando `playstats_ingame_url` está vazio, o plugin usa `playstats_url`. [Instalando em uma VPS Linux](install-linux.pt.md#o-endereço-usado-dentro-do-jogo) explica qual endereço usar.

## O que cada status significa

| Status | Significado | O que fazer |
|---|---|---|
| Aguardando | O servidor nunca chegou ao site. | Confira as duas linhas e se o `l4d2_playstats_sync.smx` está em `plugins/`. Execute `sm_playstats_test`. |
| Conectado | Um sinal chegou nos últimos segundos. | Nada. |
| Offline | O servidor já foi visto antes, mas não agora. | Normal enquanto o servidor de jogo está desligado ou vazio e hibernando. |
| Chave inválida | O identificador chegou com a chave errada. | Cole o token de novo, ou gere uma nova chave no site. |

## Vindo da configuração antiga

Se o servidor de jogo usava os plugins anteriores, remova as configurações deles, senão elas continuam apontando para os endereços antigos:

| Configuração antiga | Agora |
|---|---|
| `playstats_url` apontando para a API antiga | O endereço deste site |
| `playstats_access_token` | `playstats_token` |
| `l4d2_web_url`, `l4d2_web_secret` | Removidas. Tudo usa `playstats_url` e `playstats_token`. |
| `ranking_url` | `playstats_ingame_url`, somente se o jogo precisar de um endereço diferente |

Procure por elas no `cfg/server.cfg`, no seu arquivo de segredos e no `shared_cvars.cfg` de cada modo de partida.

## Se algo der errado

| O que você vê | O que fazer |
|---|---|
| `sm_playstats_test` é um comando desconhecido | O `l4d2_playstats_sync.smx` não está carregado. Confira a pasta e execute `sm plugins list`. |
| "URL not set" | As duas linhas não foram executadas. Elas precisam estar em um arquivo que o servidor executa ao iniciar, depois que o plugin é carregado. |
| "Cannot reach the address" | O servidor de jogo não consegue abrir o site. Teste o endereço a partir daquela máquina e verifique o firewall. |
| Conectado, mas nenhum arquivo de estatísticas chega | Jogue um mapa completo no versus. Confira o `sm_stats_writestats` e se o `l4d2_playstats` está carregado naquele modo. |
| As partidas chegam, mas não contam para o ranking | No site, abra **Administração → Modos de jogo** e veja em qual regra a configuração se encaixa. |
| `!ranking` abre um painel vazio | Defina `playstats_ingame_url` com um endereço HTTP simples do site. |
