# Contribuindo

🌐 [English](CONTRIBUTING.md) | **Português** | [Español](CONTRIBUTING.es.md)

Como o código deste repositório está organizado e as regras que toda mudança segue.

## Conteúdo

- [Projetos](#projetos)
- [Como rodar](#como-rodar)
- [Regras de código](#regras-de-código)
- [Onde fica cada arquivo](#onde-fica-cada-arquivo)
- [Módulos](#módulos)
- [Banco de dados](#banco-de-dados)
- [Páginas web](#páginas-web)
- [API do servidor de jogo](#api-do-servidor-de-jogo)
- [Testes](#testes)

## Projetos

| Pasta | Conteúdo |
|---|---|
| `src/L4D2PlayStats.Hub.Core` | As entidades, o contexto do banco de dados e os serviços com as regras de negócio. |
| `src/L4D2PlayStats.Hub.Web` | A aplicação ASP.NET Core: as páginas, a API usada pelo servidor de jogo, a área administrativa e os jobs em segundo plano. |
| `src/L4D2PlayStats.Hub.Migrations.*` | Migrations do Entity Framework Core, um projeto por banco de dados: `Sqlite`, `SqlServer`, `PostgreSql`, `MySql`. |
| `tests/L4D2PlayStats.Hub.Tests` | Testes em MSTest para o núcleo e para a aplicação web. |
| `plugins/` | Os plugins SourceMod. Eles têm o próprio [guia](plugins/README.pt.md) e a própria licença. |
| `deploy/` | O instalador Linux e o arquivo do Docker Compose. |
| `docs/` | Guias em inglês, português e espanhol. |

## Como rodar

```bash
dotnet test
```

```bash
dotnet run --project src/L4D2PlayStats.Hub.Web --launch-profile http
```

O site abre em `http://localhost:5080`. Na primeira execução, ele imprime um código de configuração no log. No ambiente Development, a página de configuração permite entrar sem a Steam, com qualquer SteamID64.

## Regras de código

- **Sem comentários no código.** Os nomes e os métodos pequenos carregam o significado.
- **As constantes ficam em `AppConsts`**, na raiz do projeto que as usa. Nada de números mágicos nem de strings repetidas no código.
- **Um tipo por arquivo.** Cada classe, record, interface e enum tem o próprio arquivo, com o nome do tipo.
- **Não altere o resultado de uma chamada inline.** Escreva `var player = Find(id); player.Wins++;` em vez de `Find(id).Wins++`.
- **Inglês em tudo**: identificadores, mensagens de log, textos da interface.
- File-scoped namespaces, primary constructors e nullable reference types, como nos arquivos existentes. Os namespaces seguem as pastas.
- O pacote NuGet `L4D2PlayStats` compartilha o namespace raiz com este código. Os tipos `Statistics` e `Player` dele estão no escopo em todo lugar, então não dê o nome `Statistics` ou `Player` a um tipo nem a um segmento de namespace. Use um alias como `using ParsedStatistics = L4D2PlayStats.Statistics;` quando precisar do arquivo parseado.

## Onde fica cada arquivo

Cada funcionalidade é um módulo: uma pasta dentro de `Modules/` no núcleo. Dentro de um módulo, os arquivos são agrupados em subpastas por categoria.

| Subpasta | O que vai nela |
|---|---|
| A raiz do módulo | As entidades armazenadas no banco de dados. |
| `Configurations/` | Classes `IEntityTypeConfiguration<T>`. |
| `Enums/` | Enums. |
| `Exceptions/` | Exceções. |
| `Extensions/` | Classes estáticas com métodos de extensão. |
| `Helpers/` | Classes estáticas com lógica pura, como calculadoras. |
| `Models/` | Records e classes que não são armazenados: resultados, filtros, valores mantidos em memória. |
| `Services/` | Interfaces de serviço e suas implementações, cada uma no próprio arquivo. |
| `Settings/` | Classes de configuração salvas por meio do `ISettingsService`. |

O projeto web segue a mesma ideia: um controller por arquivo em `Controllers/`, view models em `Models/` e as peças transversais em `Infrastructure/`, agrupadas por categoria.

## Módulos

- Uma classe cujo nome termina em `Service` é registrada automaticamente, como scoped, para as interfaces que implementa. Isso vale para o núcleo e para o projeto web.
- Todo o resto, como um singleton que mantém estado em memória, é registrado à mão em `CoreInjection` ou no `Program.cs`.
- As configurações são classes simples salvas por meio do `ISettingsService`. Cada classe é uma linha, cuja chave é o nome da classe, então uma nova classe de configuração não exige mudança de esquema.
- Uma funcionalidade que o administrador pode desligar é um valor de `HubModule`. Coloque `[RequireModule(HubModule.X)]` nos controllers dela; quando o módulo está desligado, eles respondem 404.
- Dados que alteram os rankings de estatísticas chamam `HubState.DataChanged()`, que invalida os rankings em cache.
- Escritas que não podem se intercalar, como um envio de estatísticas ou uma compra, adquirem o `HubState.WriteLock`.
- O estado da partida em andamento fica em memória, um por servidor de jogo. Ele nunca é gravado no banco de dados.

## Banco de dados

- O contexto é o `HubDbContext`. Acesse uma tabela com `dbContext.Set<T>()` ou adicione uma propriedade `DbSet`.
- Configure uma entidade com uma classe `IEntityTypeConfiguration<T>`. O contexto carrega todas as configurações do assembly do núcleo.
- Use apenas LINQ que todo provider relacional consiga traduzir. Nada de SQL puro nem de funções específicas de um provider.
- Quando o modelo mudar, adicione uma migration a **cada um** dos quatro projetos. Substitua `Sqlite` por `SqlServer`, `PostgreSql` e `MySql`:

```bash
dotnet ef migrations add <Name> --project src/L4D2PlayStats.Hub.Migrations.Sqlite --startup-project src/L4D2PlayStats.Hub.Migrations.Sqlite
```

Um teste falha quando o modelo e as migrations do banco de dados em uso não batem.

## Páginas web

- As páginas são views Razor renderizadas no servidor que mantêm o visual do site que este projeto substitui: Bootstrap 3 com os temas escuro e claro, jQuery, Chart.js, diffDOM e lity, todos em `wwwroot`. Algumas páginas administrativas também carregam o htmx de `wwwroot/lib`. Não há etapa de build com Node. Uma página que existia no site antigo é um porte dela: leia [docs/site-parity](docs/site-parity/README.pt.md) antes de alterar uma.
- Todo texto exibido a um visitante passa pelo localizador: `@L["Text in English"]`. O texto em inglês é a chave. As traduções ficam em `Resources/pt-BR.json` e `Resources/es-ES.json`. Quando uma palavra em inglês precisa de duas traduções, use uma chave com um prefixo de contexto, como `Rarity.Common`, e informe o texto em inglês dela em `Resources/en-US.json`.
- Um formulário que faz POST precisa de `asp-antiforgery="true"`, e o controller dele precisa de `[AutoValidateAntiforgeryToken]`. Um checkbox vinculado a um booleano é seguido por um input hidden com o mesmo nome e o valor `false`.
- As páginas administrativas usam `[Authorize(Policy = AppConsts.AdministratorPolicy)]` e ficam em `/admin`.
- As páginas em `/ingame` abrem dentro do jogo, em um navegador de 2012. Elas não usam script, flexbox, grid nem propriedades personalizadas. Veja [docs/in-game-browser.pt.md](docs/in-game-browser.pt.md).

## API do servidor de jogo

- Os endpoints chamados pelos plugins do servidor de jogo ficam em `/api`.
- Toda chamada de um plugin leva o header `Authorization: <identifier>:<key>`. Coloque `[GameServerAuthorize]` na action e leia quem chamou com `HttpContext.GameServer()`.
- Identificadores que são valores SteamID64 trafegam como strings no JSON, porque não cabem em um number do JavaScript.
- Os plugins são lançados separadamente do site, então um endpoint que eles chamam mantém o caminho e o formato do JSON. Um teste cobre cada um.

## Testes

- O comportamento do back-end é coberto por testes. Uma mudança sem teste para aquilo que ela altera não está concluída.
- Os testes do núcleo usam o `TestHub`, que fornece um banco de dados real e um relógio controlado pelo teste.
- Os testes web usam o `HubWebFactory`, que hospeda a aplicação inteira. `Client(user)` entra como aquele SteamID64, `ServerClient(credentials)` chama a API como um servidor de jogo e `PostFormAsync` envia um formulário com o token antiforgery.
- O `StatisticsBuilder` cria arquivos de estatísticas para qualquer mapa, data e placar a partir da amostra em `Samples/`.
- Os nomes dos testes dizem o que se espera em palavras simples, como `ASingleMapDoesNotCountForTheRanking`.

Por padrão, os testes usam SQLite em memória. Para rodá-los em outro banco de dados, defina duas variáveis de ambiente. `{database}` é substituído por um nome novo a cada teste, e o banco de dados é apagado no final:

```bash
export HUB_TEST_DATABASE_PROVIDER=PostgreSql
export HUB_TEST_DATABASE_CONNECTION="Host=localhost;Database={database};Username=postgres;Password=YOUR-PASSWORD"
dotnet test
```

Os testes da categoria `Parity` comparam os rankings com uma cópia de dados reais e são ignorados, a menos que `HUB_PARITY_DUMP` aponte para essa cópia.

Os [testes no navegador](tools/e2e/README.pt.md) verificam as principais páginas num Chrome ou Edge headless, num site descartável com dados gerados. Rode-os depois de mudar uma view, um estilo ou um script.
