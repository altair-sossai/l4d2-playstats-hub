# Instalando no Azure

🌐 [English](install-azure.md) | **Português** | [Español](install-azure.es.md)

Há duas formas de rodar o L4D2 PlayStats Hub no Azure. A primeira é igual à de qualquer VPS e é a que recomendamos.

> **Situação deste guia.** O caminho da máquina virtual usa o instalador para Linux, que é testado. O caminho do App Service foi escrito a partir da documentação do Azure e ainda não foi executado de ponta a ponta, por isso esta página não tem capturas de tela dele. Se algum passo não corresponder ao que você vê, por favor abra uma issue.

## Conteúdo

- [Opção 1: uma máquina virtual](#opção-1-uma-máquina-virtual)
- [Opção 2: App Service com um contêiner](#opção-2-app-service-com-um-contêiner)
- [Usando o Azure SQL](#usando-o-azure-sql)

## Opção 1: uma máquina virtual

Crie uma máquina virtual Linux e siga [Instalando em uma VPS Linux](install-linux.pt.md). Nada muda, e as capturas de tela desse guia mostram o que você vai ver.

| Configuração | Valor sugerido |
|---|---|
| Imagem | Ubuntu Server 24.04 LTS |
| Tamanho | B1s (1 CPU, 1 GB) é suficiente para começar |
| Portas de entrada | 22 para SSH e 5080 para o site, ou 80 e 443 se você usar um domínio |

No portal, as portas de entrada ficam na página **Rede** da máquina virtual. O site não abre enquanto a porta não for liberada ali.

## Opção 2: App Service com um contêiner

O App Service roda a imagem Docker que cada release publica. Você precisa da [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli), com login feito por `az login`.

Escolha um nome para o site. Ele precisa ser único no Azure, porque o endereço passa a ser `https://NAME.azurewebsites.net`. Os comandos abaixo usam `my-playstats`.

Crie um grupo de recursos e um plano Linux:

```bash
az group create --name playstats --location brazilsouth
```

```bash
az appservice plan create --resource-group playstats --name playstats-plan --is-linux --sku B1
```

Crie o site a partir da imagem:

```bash
az webapp create --resource-group playstats --plan playstats-plan --name my-playstats --container-image-name ghcr.io/altair-sossai/l4d2-playstats-hub:latest
```

Informe ao App Service em qual porta a aplicação escuta e mantenha os dados no disco persistente:

```bash
az webapp config appsettings set --resource-group playstats --name my-playstats --settings WEBSITES_PORT=8080 WEBSITES_ENABLE_APP_SERVICE_STORAGE=true Hub__DataDirectory=/home/data
```

A chave da API da Steam, o destino do backup e o código de configuração são salvos criptografados no banco de dados, com uma chave de segredos guardada fora da pasta de dados, para que o zip de backup do site não os revele. No App Service a imagem guarda essa chave em `/home/secrets/secrets.key`, no disco persistente, e a cria na primeira vez que inicia. Para ficar você mesmo com a chave, o que facilita guardar uma cópia, passe-a como configuração do aplicativo antes da primeira inicialização, 32 bytes aleatórios em base64:

```bash
az webapp config appsettings set --resource-group playstats --name my-playstats --settings Hub__SecretsKey="$(openssl rand -base64 32)"
```

Guarde uma cópia desse valor em um lugar seguro, ou use uma referência do Key Vault para ele. Se o site já criou `/home/secrets/secrets.key`, deixe essa configuração de fora ou use o conteúdo desse arquivo como valor: uma chave diferente não consegue ler os segredos salvos antes. Um backup restaurado sem a chave original fica com a chave da API da Steam e o destino do backup vazios, e você os digita de novo na área de administração.

Mantenha o site em uma única instância. A partida ao vivo fica guardada na memória, e duas instâncias teriam cada uma a sua:

```bash
az appservice plan update --resource-group playstats --name playstats-plan --number-of-workers 1
```

Leia o código de configuração no log:

```bash
az webapp log config --resource-group playstats --name my-playstats --docker-container-logging filesystem
```

```bash
az webapp log tail --resource-group playstats --name my-playstats
```

Procure a linha que diz `Setup is pending`. Depois abra `https://my-playstats.azurewebsites.net`, cole o código, faça login com o Steam e siga as páginas. A [Configuração inicial](manual/first-run.pt.md) mostra cada uma delas.

Duas coisas que você precisa saber sobre o App Service:

- **O banco de dados.** O disco persistente do App Service é um compartilhamento de rede, e o SQLite não é confiável em compartilhamentos de rede. Para qualquer coisa além de um teste, use o Azure SQL como descrito abaixo.
- **O jogo.** O comando `!ranking` dentro do jogo precisa de um endereço HTTP simples, porque não se espera que o navegador embutido no jogo abra os sites HTTPS atuais. Libere o HTTP simples no site com o comando abaixo e use `http://my-playstats.azurewebsites.net` como o endereço para o jogo.

```bash
az webapp update --resource-group playstats --name my-playstats --https-only false
```

## Usando o Azure SQL

Crie um servidor e um banco de dados. A camada Basic é suficiente.

```bash
az sql server create --resource-group playstats --name my-playstats-sql --admin-user hubadmin --admin-password "CHOOSE-A-STRONG-PASSWORD"
```

```bash
az sql db create --resource-group playstats --server my-playstats-sql --name hub --service-objective Basic
```

Permita que os serviços do Azure acessem o servidor:

```bash
az sql server firewall-rule create --resource-group playstats --server my-playstats-sql --name azure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

Aponte o site para ele:

```bash
az webapp config appsettings set --resource-group playstats --name my-playstats --settings Database__Provider=SqlServer Database__ConnectionString="Server=tcp:my-playstats-sql.database.windows.net,1433;Database=hub;User ID=hubadmin;Password=CHOOSE-A-STRONG-PASSWORD;Encrypt=True"
```

A aplicação cria as tabelas dela ao iniciar. [Bancos de dados](databases.pt.md) traz mais informações sobre cada banco de dados suportado.
