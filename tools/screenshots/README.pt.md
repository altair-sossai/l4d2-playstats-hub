# Ferramenta de capturas de tela

Tira as capturas de tela usadas pela documentação e as salva em `docs/images/<lang>/`, uma pasta por idioma da interface (`en`, `pt`, `es`). Ela controla um Edge ou Chrome headless pelo protocolo DevTools. Precisa do Node 22 ou mais novo e não tem dependências.

## O que ela captura

| Comando | Capturas | Requer |
|---|---|---|
| `terminal` | O instalador e os comandos do Docker, desenhados como janelas de terminal a partir da saída gravada em [terminal/](terminal/). As mesmas imagens vão para a pasta de cada idioma. | Nada mais. |
| `wizard` | Cada passo do assistente de primeira configuração, do código de configuração até o site pronto. | Um site iniciado com uma pasta de dados vazia, e o código de configuração dele. |
| `site` | Todas as páginas públicas, a loja de skins, o ranking da comunidade, as páginas dentro do jogo, o layout de celular, os dois temas e todas as páginas da administração. | Um site com dados, logado pelo login de desenvolvimento. |

As listas de capturas ficam em [site.js](site.js), [wizard.js](wizard.js) e [terminal-shots.json](terminal-shots.json). Cada captura tem um nome, uma página, uma largura (1280 para desktop, 390 para celular, 900x450 para as páginas dentro do jogo), as ações feitas antes da captura e o que guardar: a página inteira, a parte visível ou um elemento. As imagens são WebP com qualidade 80.

## Rodando o site para ela

Os comandos `site` e `wizard` fazem login pelo login de desenvolvimento (`/dev/sign-in/<SteamID64>`), que só existe no ambiente Development com o login de teste permitido. Publique o site e rode-o com uma pasta de dados sua:

```bash
dotnet publish src/L4D2PlayStats.Hub.Web -c Release -o ./shots-run
```

```bash
cd shots-run
ASPNETCORE_ENVIRONMENT=Development Development__AllowTestSignIn=true Hub__DataDirectory=/path/to/data ./L4D2PlayStats.Hub.Web --urls http://localhost:5103
```

Nunca aponte para uma pasta de dados de produção: o comando `site` compra uma skin e o comando `wizard` conclui a configuração.

## O assistente

Inicie o site com uma pasta de dados vazia. Ele mostra o código de configuração e o grava em `setup-code.txt` na pasta de dados. Depois:

```bash
node tools/screenshots/capture.js wizard --base http://localhost:5103 --lang en --admin 76561198000000000 --setup-code <code>
```

O assistente roda só uma vez por pasta de dados. Para cada idioma, pare o site, esvazie a pasta, inicie de novo e use o novo código. A ferramenta esconde o campo do login de desenvolvimento antes de cada captura, já que um site de produção não mostra esse campo. Depois do último passo, ela abre a página do novo servidor na área de administração e envia um heartbeat como um servidor de jogo faria, para que o status da conexão fique verde.

## O resto do site

O comando `site` espera uma cópia de dados reais com:

- todos os módulos ligados em **Administração > Módulos**;
- dois servidores de jogo, com os tokens deles;
- alguns votos da comunidade, um banimento do chat, um comando remoto e uma conexão de jogador, para que essas páginas não fiquem vazias.

```bash
node tools/screenshots/capture.js site --base http://localhost:5103 --lang en --admin <SteamID64 of an administrator> --player <SteamID64 of a player with matches and stars> --token <token of server 1> --token <token of server 2>
```

Antes das capturas do servidor ao vivo, ela envia a partida ao vivo de cada servidor, do jeito que os plugins fazem, e espera o atraso da página ao vivo. Reinicie o site antes de cada idioma, para que o chat ao vivo comece vazio. Antes das capturas da loja, ela faz o reembolso das compras do jogador e depois compra a primeira skin que o jogador consegue pagar. As capturas de banimento do chat adicionam um banimento para o `--banned`.

O login de desenvolvimento não tem nome do Steam, então o menu, as compras e os registros de auditoria mostram `Player <SteamID64>` para o usuário logado.

## As janelas de terminal

Os arquivos em [terminal/](terminal/) são a saída real dos comandos, gravada com cores em um contêiner descartável `jrei/systemd-ubuntu:24.04` (`docker exec -t`). A ferramenta reproduz essa saída em um pequeno emulador de terminal e tira uma captura do resultado. Para gravar de novo, rode o comando em um contêiner e salve a saída dele:

```bash
docker exec -t <container> bash -c "stty cols 96 rows 30; playstats-hub status" > tools/screenshots/terminal/linux-status.log
```

Depois:

```bash
node tools/screenshots/capture.js terminal
```

## Opções

| Opção | Significado |
|---|---|
| `--base` | Endereço do site. |
| `--lang` | `en`, `pt` ou `es`. Define o idioma da interface antes da primeira captura. |
| `--admin` | SteamID64 de um administrador desse site. |
| `--player` | SteamID64 usado para a página do jogador, a loja e as páginas dentro do jogo. As compras dele são reembolsadas antes das capturas da loja, para que ele possa comprar de novo. |
| `--voter` | SteamID64 de um jogador que ainda pode votar hoje, para a página de votação. Padrão: `--player`. |
| `--banned` | SteamID64 usado nas capturas de banimento do chat. Padrão: `--player`. |
| `--token` | Token de um servidor de jogo, `identifier:key`. Repita para um segundo servidor. |
| `--setup-code` | O código de configuração, para o assistente. |
| `--only` | Nomes das capturas a tirar de novo, separados por vírgula. |
| `--out` | Pasta das imagens. Padrão: `docs/images`. |
| `--langs` | Para o `terminal`: as pastas de idioma a gravar. Padrão: `en,pt,es`. |
| `--browser` | Caminho do Edge ou do Chrome, se ele não for encontrado sozinho. |
