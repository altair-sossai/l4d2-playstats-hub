# Bancos de dados

🌐 [English](databases.md) | **Português** | [Español](databases.es.md)

O L4D2 PlayStats Hub armazena os dados em um banco de dados relacional. Você escolhe qual com duas configurações.

## Conteúdo

- [Qual usar](#qual-usar)
- [As duas configurações](#as-duas-configurações)
- [SQLite](#sqlite)
- [PostgreSQL](#postgresql)
- [MySQL](#mysql)
- [SQL Server](#sql-server)
- [Trocar de banco de dados depois](#trocar-de-banco-de-dados-depois)

## Qual usar

| Banco de dados | Quando |
|---|---|
| SQLite | O padrão. Nada para instalar, e o banco de dados inteiro é um único arquivo. Serve para quase toda comunidade. |
| PostgreSQL, MySQL, SQL Server | Você já roda um deles, ou o site está hospedado onde um arquivo em disco não é confiável, como no Azure App Service. |

Os mesmos testes automatizados passam nos quatro. As versões usadas nesses testes são PostgreSQL 16, MySQL 8.4 e SQL Server 2022.

## As duas configurações

| Configuração | Valores |
|---|---|
| `Database__Provider` | `Sqlite`, `PostgreSql`, `MySql` ou `SqlServer` |
| `Database__ConnectionString` | Como chegar ao banco de dados. O formato depende do provedor. |

Onde colocá-las:

| Instalação | Lugar |
|---|---|
| Instalador Linux | `/etc/l4d2-playstats-hub/hub.env`, depois `playstats-hub restart` |
| Docker | Em `environment:` no arquivo do compose |
| Azure App Service | Configurações do aplicativo |

A aplicação cria o banco de dados se ele não existir e cria ou atualiza as tabelas toda vez que inicia. O usuário da connection string precisa de permissão para isso.

## SQLite

```text
Database__Provider=Sqlite
Database__ConnectionString="Data Source=hub.db"
```

Um nome de arquivo sem pasta é criado dentro da pasta de dados (`Hub__DataDirectory`). Para fazer backup, use `playstats-hub backup`, ou pare a aplicação e copie o arquivo.

## PostgreSQL

```text
Database__Provider=PostgreSql
Database__ConnectionString="Host=localhost;Database=hub;Username=hub;Password=YOUR-PASSWORD"
```

## MySQL

```text
Database__Provider=MySql
Database__ConnectionString="Server=localhost;Database=hub;User=hub;Password=YOUR-PASSWORD"
```

## SQL Server

```text
Database__Provider=SqlServer
Database__ConnectionString="Server=localhost;Database=hub;User ID=hub;Password=YOUR-PASSWORD;TrustServerCertificate=True"
```

Para o Azure SQL, use a connection string que o portal mostra em **Connection strings → ADO.NET**. O guia [Instalando no Azure](install-azure.pt.md#usando-o-azure-sql) tem os comandos.

## Trocar de banco de dados depois

Não existe ferramenta que copie os dados de um banco de dados para outro. Quando você troca o provedor, a aplicação inicia com um banco de dados vazio e mostra a página de configuração de novo.

Os arquivos de estatísticas são a origem de tudo o que os rankings mostram, mas o servidor de jogo apaga cada arquivo depois de enviá-lo, e a única cópia passa a estar dentro do banco de dados. Mantenha um backup do banco de dados antigo até ter certeza de que não precisa mais dele. Compras na loja de skins, votos da comunidade, punições e configurações não estão nesses arquivos e teriam de ser informados de novo.
