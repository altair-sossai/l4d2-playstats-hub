# Página do Anti-Cheat

🌐 [English](anti-cheat.md) | **Português** | [Español](anti-cheat.es.md)

Algumas comunidades pedem que os jogadores abram o jogo por um programa de Anti-Cheat antes de entrar no servidor competitivo. A página **Anti-Cheat** do site explica como usar esse programa e oferece o download.

O Anti-Cheat em si é um programa separado para Windows, com serviço próprio. Ele não faz parte deste repositório, e este site não verifica se um jogador está rodando o programa. O site só mostra a página, e o servidor ao vivo mostra o selo **Anti-Cheat ligado** quando o servidor de jogo informa isso.

A página é um módulo, desligado por padrão.

[Voltar ao manual](README.pt.md)

## Conteúdo

- [O que os jogadores veem](#o-que-os-jogadores-veem)
- [Para administradores](#para-administradores)

## O que os jogadores veem

![A página do Anti-Cheat](../images/pt/anti-cheat.webp)

- **Baixar Anti-Cheat**, com a versão, o tamanho e a data de lançamento.
- **Verificar o arquivo**: o comando do PowerShell que calcula o hash SHA-256 do download, e o valor com que ele precisa bater.
- **Como usar**: baixe e execute, responda e espere, jogue com a janela aberta.
- **O que a janela mostra**: o significado de cada estado do programa (iniciando, atualizando, ativo, com falha).
- **Problemas comuns**: respostas para as dúvidas mais frequentes.
- **Sobre o -insecure (sem VAC)**: o programa abre o jogo sem o VAC, então não traz risco de ban VAC, e enquanto ele está aberto só dá para entrar em servidores sem VAC.
- **Privacidade**: o que o programa envia e o que ele não faz.

Os textos da página fazem parte do site e são traduzidos junto com ele.

## Para administradores

**Administração > Página do Anti-Cheat** tem dois endereços.

![As configurações do Anti-Cheat](../images/pt/admin-anti-cheat.webp)

| Configuração | O que faz |
|---|---|
| Endereço do serviço do Anti-Cheat | Opcional. A página pede a esse serviço a versão mais recente, o tamanho, a data e o checksum. Precisa começar com `https://`. |
| Endereço de download | Usado quando o serviço não responde ou não está configurado. Precisa começar com `https://`. |

**Abrir a página do Anti-Cheat** mostra o resultado. Ligue ou desligue a página em [Módulos](administration.pt.md#módulos); quando ela está desligada, o item some do menu e a página responde como não encontrada.
