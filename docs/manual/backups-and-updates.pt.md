# Backups e atualizações

🌐 [English](backups-and-updates.md) | **Português** | [Español](backups-and-updates.es.md)

Tudo o que o site guarda fica em uma pasta só, a pasta de dados. Faça backup dela, guarde uma cópia da [chave de segredos](#a-chave-de-segredos), que fica fora dela, e você consegue montar o site de novo em qualquer máquina.

[Voltar ao manual](README.pt.md)

## Conteúdo

- [O que fica na pasta de dados](#o-que-fica-na-pasta-de-dados)
- [A chave de segredos](#a-chave-de-segredos)
- [Backup automático no Azure Storage](#backup-automático-no-azure-storage)
- [Fazendo backup no Linux](#fazendo-backup-no-linux)
- [Fazendo backup com Docker](#fazendo-backup-com-docker)
- [Restaurando](#restaurando)
- [Atualizando](#atualizando)
- [O que se perde ao reiniciar](#o-que-se-perde-ao-reiniciar)

## O que fica na pasta de dados

| Item | O que é |
|---|---|
| `hub.db` (e `hub.db-wal`, `hub.db-shm` enquanto o site está rodando) | O banco de dados SQLite: todos os arquivos de estatísticas recebidos, as partidas, as configurações, as skins, os votos, tudo. |
| `avatars/` | Os avatares do Steam que o site baixou. Eles podem ser baixados de novo. |
| `branding/` | A logo enviada nas configurações. |
| `keys/` | As chaves que protegem os cookies de login. Sem elas, todo mundo precisa fazer login de novo. |
| `setup-code.txt` | O código de configuração, só até a configuração ser concluída. |

A pasta é `/var/lib/l4d2-playstats-hub` com o instalador para Linux e o volume `hub-data` (montado em `/data`) com Docker. No Linux, as poucas configurações que ficam fora do banco estão em `/etc/l4d2-playstats-hub/hub.env`.

Com SQL Server, PostgreSQL ou MySQL, os dados ficam nesse banco: faça backup dele com as ferramentas desse banco, e guarde uma cópia da pasta de dados por causa das chaves e da logo. Veja [Bancos de dados](../databases.pt.md).

Como todo arquivo de estatísticas é guardado, o banco basta para montar de novo todas as partidas e rankings.

## A chave de segredos

A chave da API da Steam, o endereço ou a string de conexão do destino do backup e o código de configuração são salvos criptografados no banco de dados, com AES-256-GCM. A chave que os descriptografa é um arquivo guardado fora da pasta de dados:

| Instalação | Chave de segredos |
|---|---|
| Instalador para Linux | `/etc/l4d2-playstats-hub/secrets.key` |
| Docker | `/secrets/secrets.key`, no volume `hub-secrets` |
| Azure App Service | `/home/secrets/secrets.key`, ou a configuração do aplicativo `Hub__SecretsKey` |
| Qualquer outra | `secrets.key` ao lado da pasta de dados, ou o arquivo indicado em `Hub__SecretsKeyFile` |

- Quando não há chave, o site cria uma na primeira vez que inicia (o instalador para Linux a cria ele mesmo) e escreve no log onde ela está, nunca o valor.
- Versões antigas salvavam esses segredos em texto puro. A primeira inicialização de uma versão que os criptografa criptografa o que foi salvo antes, e um segredo ainda em texto puro é lido como está.
- O zip de backup não contém a chave, e a pasta de dados também não: quem conseguir um backup não consegue ler os segredos. A página **Backup** da administração mostra onde a chave está. O `playstats-hub backup` do Linux inclui a chave.
- Guarde uma cópia da chave em um lugar seguro, separada dos backups. Para restaurar em outra máquina, coloque a chave de volta antes de iniciar o site. Sem ela o site inicia e todos os dados estão lá, mas esses segredos ficam vazios: o log avisa, e você os digita de novo na área de administração.

## Backup automático no Azure Storage

O site pode fazer o próprio backup. Em [Administração > Backup](administration.pt.md#backup) você informa um contêiner do Azure Storage e, a cada algumas horas, 8 por padrão, ele envia um arquivo zip para lá. O site continua no ar: o banco é copiado enquanto ele funciona.

### O que o zip contém

| Arquivo | O que é |
|---|---|
| `hub.db` | Uma cópia do banco SQLite, feita com a função de backup do SQLite com o site rodando, então ela é consistente mesmo enquanto chegam estatísticas. |
| `keys/` | As chaves que protegem os cookies de login. |
| `branding/` | A logo. |
| `manifest.json` | A versão do site, a hora do backup em UTC, o provedor do banco e a lista de arquivos. |

- Os avatares ficam de fora: o site os baixa de novo.
- O `hub.env` (Linux) ou as variáveis de ambiente (Docker) ficam fora da pasta de dados e não entram no zip. Guarde uma cópia deles, ou rode `playstats-hub backup` de vez em quando, que inclui o `hub.env`.
- Com SQL Server, PostgreSQL ou MySQL o zip não tem o `hub.db`: faça o backup desse banco com as ferramentas dele. O zip continua guardando as chaves e a logo.
- A chave de segredos não está no zip, de propósito. Guarde uma cópia dela à parte: veja [A chave de segredos](#a-chave-de-segredos).

Os arquivos se chamam `<prefixo>-yyyyMMdd-HHmmss.zip`, em UTC, como `hub-backup-20261119-160000.zip`. Depois de cada envio, os arquivos mais antigos com o mesmo prefixo além da quantidade a manter (30 por padrão) são apagados. Os outros arquivos do contêiner nunca são tocados. Um backup que falha é tentado de novo 30 minutos depois, e a página mostra o erro.

### Criando o contêiner e o endereço SAS dele

O destino mais seguro é um endereço SAS de um contêiner: com ele o site só consegue usar esse contêiner.

No portal do Azure:

1. Abra a conta de armazenamento, **Armazenamento de dados > Contêineres**, e crie um contêiner, como `hub-backups`, com acesso privado.
2. Abra o contêiner e **Configurações > Tokens de acesso compartilhado**. Assine com uma chave da conta, marque as permissões **Read, Add, Create, Write, Delete e List**, escolha uma data de expiração distante e clique em **Gerar token SAS e URL**.
3. Copie a **URL SAS do blob**. Ela tem a forma `https://<conta>.blob.core.windows.net/hub-backups?sv=...&sig=...`.

Com a Azure CLI:

```bash
az storage container create --account-name <conta> --name hub-backups --auth-mode key
```

```bash
az storage container generate-sas --account-name <conta> --name hub-backups --permissions racwdl --expiry 2028-12-31T23:59Z --https-only --auth-mode key -o tsv
```

O segundo comando imprime o token. O endereço é `https://<conta>.blob.core.windows.net/hub-backups?` seguido dele.

Cole o endereço em **Administração > Backup**, marque **Fazer backup automaticamente**, salve e clique em **Testar conexão**. O SAS para de funcionar na data de expiração: crie um novo antes disso, senão os backups começam a falhar, como a página mostra.

Uma string de conexão da conta de armazenamento também funciona, junto com o nome de um contêiner; o site cria o contêiner quando ele não existe. Ela dá acesso à conta inteira, então prefira o endereço SAS.

### Restaurando um zip

Baixe o zip do contêiner: no portal, abra o arquivo e clique em **Baixar**; com a CLI, `az storage blob download`.

No Linux, pare o site, coloque os arquivos do zip na pasta de dados e inicie o site de novo. O `unzip` pode precisar de `sudo apt install unzip`.

```bash
sudo systemctl stop l4d2-playstats-hub
```

```bash
sudo rm -f /var/lib/l4d2-playstats-hub/hub.db-wal /var/lib/l4d2-playstats-hub/hub.db-shm
```

```bash
sudo unzip -o hub-backup-20261119-160000.zip -x manifest.json -d /var/lib/l4d2-playstats-hub
```

```bash
sudo chown -R playstats:playstats /var/lib/l4d2-playstats-hub
```

```bash
sudo systemctl start l4d2-playstats-hub
```

Apague os arquivos `-wal` e `-shm` antes de extrair: eles são do banco que está sendo substituído. Numa máquina nova, rode o instalador antes, coloque a sua chave de segredos em `/etc/l4d2-playstats-hub/secrets.key` (veja [A chave de segredos](#a-chave-de-segredos)), depois restaure, e coloque o seu `hub.env` de volta se você o alterou.

Com Docker, pare o contêiner, extraia o zip no volume e inicie de novo. O contêiner entrega os arquivos ao usuário dele quando inicia. Numa máquina nova, coloque também a chave de segredos no volume `hub-secrets`, como mostra [Instalando com Docker](../install-docker.pt.md#a-chave-de-segredos).

```bash
docker compose stop hub
```

```bash
docker run --rm -v playstats_hub-data:/data -v "$PWD":/backup alpine sh -c "rm -f /data/hub.db-wal /data/hub.db-shm && unzip -o /backup/hub-backup-20261119-160000.zip -x manifest.json -d /data"
```

```bash
docker compose start hub
```

## Fazendo backup no Linux

```bash
playstats-hub backup
```

![Um backup no Linux](../images/pt/install-linux-backup.webp)

O site para por alguns segundos enquanto a cópia é feita. O arquivo vai para `/root` com a data no nome e contém a pasta de dados, o `hub.env` e a chave de segredos, então mantenha-o privado. Informe outro caminho para salvar em outro lugar: `playstats-hub backup /backups/hub.tar.gz`. Copie o arquivo para outra máquina.

Para fazer um toda noite, adicione uma linha ao crontab do root (`sudo crontab -e`):

```text
0 4 * * * /usr/local/bin/playstats-hub backup
```

Os backups antigos não são apagados sozinhos.

## Fazendo backup com Docker

Pare o contêiner, copie o volume para um arquivo e inicie de novo:

![Um backup com Docker](../images/pt/install-docker-backup.webp)

O nome do volume tem o nome da sua pasta na frente, como `playstats_hub-data`. O `docker volume ls` mostra o nome. [Instalando com Docker](../install-docker.pt.md#backup) tem os comandos. Copie também o volume `hub-secrets`: ele guarda a chave de segredos.

## Restaurando

No Linux, na mesma máquina ou em uma nova onde você rodou o instalador:

```bash
playstats-hub restore /root/l4d2-playstats-hub-backup-20270115-030000.tar.gz
```

Ele substitui os dados atuais, o `hub.env` e a chave de segredos pelos que estão no arquivo e inicia o site de novo. Um backup feito antes de os segredos serem criptografados não tem chave, e a atual é mantida.

Com Docker, pare o contêiner, extraia o arquivo no volume e inicie de novo:

```bash
docker compose stop hub
```

```bash
docker run --rm -v playstats_hub-data:/data -v "$PWD":/backup alpine sh -c "rm -rf /data/* && tar -xzf /backup/hub-backup.tar.gz -C /data"
```

```bash
docker compose start hub
```

## Atualizando

As novas versões são publicadas nas [releases](https://github.com/altair-sossai/l4d2-playstats-hub/releases). Uma atualização mantém os dados e as configurações. Quando uma versão muda o banco de dados, o site atualiza as tabelas sozinho ao iniciar.

No Linux:

```bash
playstats-hub update
```

![Uma atualização no Linux](../images/pt/install-linux-update.webp)

Para instalar uma versão específica: `playstats-hub update v1.2.0`.

Com Docker:

![Uma atualização com Docker](../images/pt/install-docker-update.webp)

As imagens até a v0.3.0 rodavam o site como root; as mais novas rodam com o usuário `app`. Um volume criado por uma imagem antiga passa para esse usuário quando o novo contêiner inicia, então não é preciso mais nada. [Instalando com Docker > Atualizando](../install-docker.pt.md#atualizando) tem os detalhes.

Um backup antes de uma atualização custa um minuto e permite voltar atrás.

Os plugins dos servidores de jogo são lançados junto com o site. Confira as notas da versão para ver se eles mudaram; [Conectando um servidor de jogo](../game-server.pt.md) explica como substituí-los.

## O que se perde ao reiniciar

Reiniciar, atualizar ou restaurar mantém tudo o que está armazenado. Só o que fica na memória começa vazio:

- a partida em andamento na página do servidor ao vivo, até os servidores de jogo enviarem a próxima atualização;
- as mensagens do chat do site e os comandos esperando para ser entregues;
- o status da conexão dos servidores de jogo, até cada um enviar o próximo sinal.
