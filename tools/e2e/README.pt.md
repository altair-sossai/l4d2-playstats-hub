# Testes no navegador

Verificam as principais páginas do site num Chrome ou Edge headless, para que uma regressão de layout ou de JavaScript quebre o build. Rodam em todo push e pull request em [Browser tests](../../.github/workflows/browser-tests.yml) e precisam do Node 22 ou mais novo, sem dependências. Controlam o navegador com o [cdp.js](../screenshots/cdp.js) da ferramenta de capturas de tela.

## O que fazem

O [run.js](run.js) inicia o site publicado com uma pasta de dados nova e vazia, no ambiente Development com o login de desenvolvimento (`/dev/sign-in/<SteamID64>`), e o preenche como uma comunidade de verdade:

- conclui o assistente de configuração com o código de `setup-code.txt`, liga todos os módulos e cria um servidor de jogo;
- envia 14 arquivos de estatísticas, cinco partidas de 16 jogadores fictícios, feitos a partir da amostra em [tests/L4D2PlayStats.Hub.Tests/Samples](../../tests/L4D2PlayStats.Hub.Tests/Samples) com o token do servidor ([statistics.js](statistics.js), jogadores em [fixtures.js](fixtures.js));
- configura o ranking da comunidade com 3 votos por dia, envia uma partida ao vivo e registra votos da comunidade com seis jogadores.

Depois roda as verificações de [checks.js](checks.js): o ranking da página inicial (também em 390 px, sem rolagem lateral), a página do jogador e suas abas, a página de um SteamID desconhecido, as partidas, a lista de jogadores (a visão geral, a ordem por uma coluna, uma estatística e o antigo endereço do histórico), todas as páginas públicas em 390 px, a página de comandos (busca, links das seções e os comandos de administrador recolhidos), o ranking da comunidade (convite, "como funciona", janelas de detalhes do jogador e das colunas), a votação com as setas e um clique na foto até o limite do dia, os filtros dos últimos votos e a janela de busca de jogador, todas as páginas da administração e seus ícones de ajuda, a janela de busca de jogador, o montador de comandos remotos, os formulários de limites do chat e de configurações ao vivo, a página ao vivo recebendo dados novos, português e espanhol, e o tema claro. Um erro de JavaScript ou do servidor em qualquer página também reprova a verificação.

Tudo roda nesse site descartável; a pasta de dados é apagada no fim. Nunca aponte para uma pasta de dados real.

## Rodando

```bash
dotnet publish src/L4D2PlayStats.Hub.Web -c Release -o ./e2e-site
node tools/e2e/run.js --site ./e2e-site
```

Leva cerca de dois minutos. Uma verificação que falha mostra o motivo e salva uma captura de tela, a página e o log do site em `e2e-output/`, que o workflow publica como o artefato `browser-test-failures`.

| Opção | Significado |
|---|---|
| `--site` | Pasta do site publicado. |
| `--port` | Porta do site. Padrão: `5199`. |
| `--only` | Nomes das verificações a rodar, separados por vírgula, como `community-vote,languages`. Os dados são sempre criados. |
| `--browser` | Caminho do Chrome ou Edge, se não for encontrado sozinho. |
| `--out` | Pasta das falhas. Padrão: `e2e-output`. |
| `--data` | Uma pasta vazia para usar como pasta de dados, mantida no fim. |
| `--keep` | Mantém a pasta de dados temporária. |
| `--serve` | Cria os dados e deixa o site rodando para dar uma olhada no navegador, até o Ctrl+C. |
