# Trocando o site antigo

🌐 [English](switch-from-old-site.md) | **Português** | [Español](switch-from-old-site.es.md)

Como substituir uma cópia em funcionamento do site anterior (`l4d2-playstats-web` com `l4d2-playstats-api`) por este, na mesma máquina, sem mudar os endereços que os jogadores já usam. O exemplo é a instalação do l4d2.com.br: uma VPS Ubuntu em `124.198.128.43` onde o nginx serve `https://l4d2.com.br` e repassa para o site antigo na porta `5000`, o jogo abre `http://124.198.128.43:5000`, e o servidor de jogo é baseado no [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server). Troque os nomes e endereços pelos seus.

## Conteúdo

- [O plano](#o-plano)
- [Parte 1: instale o site novo ao lado do antigo](#parte-1-instale-o-site-novo-ao-lado-do-antigo)
- [Parte 2: teste com um servidor de jogo](#parte-2-teste-com-um-servidor-de-jogo)
- [Parte 3: a troca](#parte-3-a-troca)
- [Parte 4: voltando atrás](#parte-4-voltando-atrás)
- [Os dados antigos](#os-dados-antigos)

## O plano

| Parte | O que muda | O que os jogadores percebem |
|---|---|---|
| 1 | O site novo roda na porta `5090`, ao lado do antigo. O nginx e o site antigo não são mexidos. | Nada. |
| 2 | Um servidor de jogo envia as partidas dele para o site novo. | As partidas desse servidor deixam de chegar ao site antigo. |
| 3 | O site antigo para e o novo assume a porta dele, a `5000`. O nginx continua repassando `https://l4d2.com.br` para ela. | As páginas novas, nos mesmos endereços. |
| 4 | Só se algo der errado: o site antigo volta. | As páginas antigas de novo. |

Mantenha `/home/l4d2-playstats-web`, a pasta do site antigo, como está até ter certeza: voltar atrás precisa dela.

## Parte 1: instale o site novo ao lado do antigo

1. Conecte na VPS por SSH, com um usuário que possa usar `sudo`.
2. Rode o instalador sem domínio. Sem `HUB_DOMAIN` ele não instala o Caddy, então o nginx e as portas 80 e 443 ficam como estão:

   ```bash
   curl -fsSL https://raw.githubusercontent.com/altair-sossai/l4d2-playstats-hub/main/deploy/install.sh | sudo HUB_PORT=5090 bash
   ```

3. Abra a porta `5090`. Ela não é a `5080` padrão do instalador porque, nesta VPS, essa porta já é usada pela API do anti-cheat (`127.0.0.1:5080`). Com o UFW ativo, o instalador já fez isso; senão rode `sudo ufw allow 5090/tcp`. Se o seu provedor de hospedagem tem um firewall próprio, abra a porta nele também.
4. Abra `http://124.198.128.43:5090`, cole o código de configuração que o instalador mostrou e entre com a Steam. Essa conta vira o primeiro administrador.
5. Siga o assistente ([Configuração inicial](manual/first-run.pt.md)). Na página **Módulos**, ligue o que o site antigo tinha: ranking, servidor ao vivo, chat do site, comandos remotos, loja de skins e página do Anti-Cheat. Na página **Servidor de jogo**, crie o servidor de jogo e copie as duas linhas que ela mostra, `playstats_url` e `playstats_token`: a chave aparece só uma vez.
6. Passe pelo resto da administração ([Administração](manual/administration.pt.md)): **Configurações → Privacidade** (quem vê as contas relacionadas), **Administração → Modos de jogo** (quais configurações contam para o ranking), as temporadas, a chave da Steam Web API, os outros administradores e o catálogo de skins da loja. Deixe o **Endereço público do site** vazio por enquanto.

[Instalando em uma VPS Linux](install-linux.pt.md) explica cada passo do instalador e os comandos `playstats-hub`.

## Parte 2: teste com um servidor de jogo

Um servidor de jogo envia as partidas para um site só. Enquanto ele aponta para o site novo, as partidas dele não chegam mais ao antigo, e não vão para lá depois. Teste com um segundo servidor de jogo se você tiver um, ou num horário tranquilo com o principal.

### Faça backup do que os plugins substituem

No servidor de jogo, guarde uma cópia dos arquivos que os novos plugins substituem, para a [Parte 4](#parte-4-voltando-atrás):

```bash
cd /path/to/the/server/left4dead2
tar -czf ~/playstats-plugins-backup.tar.gz addons/sourcemod/plugins addons/sourcemod/translations addons/sourcemod/configs/weapon_skins.cfg cfg
```

Se o servidor de jogo é instalado a partir de um repositório, como o `l4d2-zone-server`, faça as mudanças abaixo no repositório também, senão a próxima instalação traz de volta os plugins e as configurações antigas.

### Copie os plugins

Copie tudo o que está dentro da pasta `left4dead2` dos plugins (no zip da release `l4d2-playstats-hub-plugins.zip`, ou em [plugins/left4dead2](../plugins/left4dead2) neste repositório) por cima da pasta `left4dead2` do servidor:

| Arquivo | O que ele substitui |
|---|---|
| `addons/sourcemod/plugins/l4d2_gameinfo_sync.smx` e `l4d2_weapon_skins.smx` | Os antigos no mesmo lugar. |
| `addons/sourcemod/plugins/optional/l4d2_playstats_sync.smx`, `l4d2_ranking.smx` e `l4d2_show_patent_icon.smx` | Os antigos no mesmo lugar, que o `shared_plugins.cfg` dos modos de partida já carrega. |
| `addons/sourcemod/translations/**` | Os textos das versões antigas. Copie junto com os arquivos `.smx`: os textos do ranking mudaram. |
| `addons/sourcemod/configs/weapon_skins.cfg` | As mesmas armas e skins. |
| `addons/sourcemod/scripting/**` | Os fontes, usados só para compilar. |
| `cfg/playstats.cfg` | Nada: ele é novo. |

Precisa existir só uma cópia de cada plugin. Nenhum dos cinco pode ficar em outra pasta, como um `l4d2_playstats_sync.smx` antigo na própria `addons/sourcemod/plugins/`.

### Mude as configurações

| Arquivo | Mudança |
|---|---|
| `cfg/cfgogl/zonemod/shared_cvars.cfg` e `cfg/cfgogl/neomod/shared_cvars.cfg` | Apague `confogl_addcvar playstats_url ...`, a API antiga, que faria os cinco plugins apontarem de volta para ela em toda partida, e `confogl_addcvar ranking_url ...`, que não existe mais. Mantenha as outras linhas: `patent_icon_enabled`, `weapon_skins_*`, `gameinfo_*` e `anti_cheat_*` funcionam como antes. |
| `cfg/server_secrets.cfg` | Apague `playstats_access_token`. Adicione `playstats_url` e `playstats_token` (abaixo). Defina `l4d2_web_secret` com o mesmo token do `playstats_token`. |
| `cfg/server.cfg` | Defina `l4d2_web_url` com o endereço do site novo. Se quiser, adicione `sm plugins load optional/l4d2_playstats_sync.smx` no fim, para que o site veja o servidor enquanto nenhum modo de partida está carregado. |
| `cfg/sharedplugins.cfg` | Opcional: as linhas `sm plugins load` do [Passo 5](game-server.pt.md#passo-5-carregue-os-plugins-nos-seus-modos-de-partida), para os modos de partida que ainda não carregam estes plugins (no `l4d2-zone-server`, todos os modos menos ZoneMod e NeoMod). |

As linhas para o `cfg/server_secrets.cfg`, com o seu token no lugar do exemplo:

```text
playstats_url "http://124.198.128.43:5090"
playstats_token "my-server:3f9c…"
l4d2_web_secret "my-server:3f9c…"
```

E a linha para o `cfg/server.cfg`:

```text
sm_cvar l4d2_web_url "http://124.198.128.43:5090"
```

- `playstats_url` é um endereço HTTP simples, então o navegador dentro do jogo também o abre e o `playstats_ingame_url` pode ficar vazio.
- `l4d2_web_url` e `l4d2_web_secret` são lidos só pelo plugin de anti-cheat (1.5.0 ou mais novo), para as listas de [Quem precisa usar o Anti-Cheat](manual/anti-cheat.pt.md#quem-precisa-usar-o-anti-cheat). As listas antigas não são copiadas: adicione os jogadores de novo no site novo antes de ligar o anti-cheat.
- Deixe o `playstats_public_url` desligado por enquanto. Ele é definido na Parte 3; antes disso, o chat mandaria os jogadores para a loja do site antigo.
- Com estas linhas no arquivo de segredos, você não precisa do `exec playstats.cfg`: o resto do `cfg/playstats.cfg` só mostra os padrões, e os modos de partida do `l4d2-zone-server` já definem o `sm_stats_writestats`.
- Nunca defina `playstats_token` ou `l4d2_web_secret` com `confogl_addcvar`: o Confogl mostra no chat os valores que ele acompanha.

### Confira

1. Reinicie o servidor de jogo enquanto ele está vazio e carregue um modo de partida, por exemplo com `!match`.
2. No console do servidor, `sm plugins list` precisa mostrar os cinco plugins, nenhum com falha e nenhum duas vezes. Depois, `sm_playstats_test` precisa responder `Connected to the site`.
3. No site novo, abra **Administração → Servidores de jogo** e o servidor. Ele precisa dizer **Conectado**, com a lista de conferência do [Passo 6](game-server.pt.md#passo-6-verifique-a-conexão).
4. Jogue um mapa completo. A partida aparece em **Partidas**, a rodada em **Servidor**, e o chat do site chega ao jogo.
5. No jogo, `!ranking` abre o painel do ranking. Com `weapon_skins_enabled 1`, `!skin status` responde; com `patent_icon_enabled 1`, os ícones das patentes aparecem antes da partida.

## Parte 3: a troca

Faça enquanto ninguém está jogando. Depois disso os dois sites escutam na porta `5000`, então o antigo para primeiro.

1. Pare o site antigo, mantendo os arquivos dele: `screen -S l4d2-playstats-web -X quit`, com o usuário que o iniciou. Se o `screen -ls` não o mostrar, rode os dois comandos com `sudo`.
2. Impeça que ele inicie no boot: rode `crontab -e` como `devops` e apague a linha `@reboot` do `l4d2-playstats-web`, ou coloque `#` na frente dela.
3. Desligue a publicação automática do site antigo, o pipeline do Azure Pipelines do `l4d2-playstats-web`. Senão um push o inicia de novo na porta `5000`.
4. Passe o site novo para a porta `5000`: no `/etc/l4d2-playstats-hub/hub.env`, troque `ASPNETCORE_URLS=http://0.0.0.0:5090` por `ASPNETCORE_URLS=http://0.0.0.0:5000`, depois rode `sudo playstats-hub restart` e `playstats-hub status`.
5. Confira: `curl -s http://127.0.0.1:5000/api/ping` responde, `https://l4d2.com.br` e `http://124.198.128.43:5000` mostram o site novo, e o login com a Steam funciona em `https://l4d2.com.br`.
6. Em **Administração → Configurações → Site**, defina o **Endereço público do site** como `https://l4d2.com.br`.
7. No servidor de jogo, aponte tudo para a porta `5000` e defina o endereço que os jogadores leem no chat: as duas primeiras linhas no `cfg/server_secrets.cfg`, a última no `cfg/server.cfg`. Depois troque o mapa ou reinicie o servidor, e rode o `sm_playstats_test` de novo:

   ```text
   playstats_url "http://124.198.128.43:5000"
   playstats_public_url "https://l4d2.com.br"
   sm_cvar l4d2_web_url "http://124.198.128.43:5000"
   ```

8. Feche a porta `5090`, que não é mais usada: `sudo ufw delete allow 5090/tcp`, e no firewall do provedor se você a abriu lá.

Os plugins também poderiam usar `https://l4d2.com.br`; o endereço simples os mantém funcionando aconteça o que acontecer com o certificado. O nginx não precisa de mudança: ele continua repassando `https://l4d2.com.br` para a porta `5000`, e o site novo acredita nos cabeçalhos `X-Forwarded-*` de um proxy na mesma máquina. Se o login com a Steam voltar para um endereço `http://` ou para o endereço IP, o `location` do nginx precisa repassar estes cabeçalhos:

```nginx
proxy_set_header Host $host;
proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;
```

## Parte 4: voltando atrás

Se algo não funcionar e não der para corrigir na hora:

1. Pare o site novo e impeça que ele inicie no boot: `sudo systemctl disable --now l4d2-playstats-hub`. Os dados dele ficam em `/var/lib/l4d2-playstats-hub`.
2. Inicie o site antigo como antes: `cd /home/l4d2-playstats-web && screen -d -m -S l4d2-playstats-web sh -c './L4D2PlayStats.Web'`. Coloque a linha `@reboot` dele de volta e ligue o pipeline dele de novo.
3. No servidor de jogo, extraia o backup na pasta `left4dead2` (`tar -xzf ~/playstats-plugins-backup.tar.gz`) e reinicie o servidor. Isso traz de volta os plugins antigos e todas as configurações em `cfg`.

As partidas jogadas enquanto o servidor de jogo apontava para o site novo ficam só nele; o site antigo não as recebe. Para tentar de novo depois, volte o `ASPNETCORE_URLS` para a porta `5090` no `/etc/l4d2-playstats-hub/hub.env` e rode `sudo systemctl enable --now l4d2-playstats-hub`.

## Os dados antigos

O site novo começa vazio. Trazer as estatísticas antigas é opcional: um programa separado envia os arquivos de estatísticas antigos pelo mesmo caminho que os plugins usam, e o site monta a partir deles as partidas, o ranking e o histórico ([Migração de dados](roadmap.pt.md#migração-de-dados)). Ele pode rodar antes ou depois da troca, e as partidas que o site novo já tem ficam. Nada mais do site antigo é copiado: as compras de skins, os presentes e as skins equipadas, os banimentos do chat e as listas do Anti-Cheat começam vazios, então cadastre de novo no site novo os banimentos do chat e os jogadores do Anti-Cheat.
