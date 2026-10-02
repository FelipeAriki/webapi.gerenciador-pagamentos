# Gerenciador de pagamentos — API

ASP.NET Core 10, Dapper e PostgreSQL 17. Controller → Service → repositories de
consulta/comando, com DTOs de entrada separados do modelo persistido. Versão local
sem login, mantendo totais acumulados por jogador e comprovantes por URL.

## Preparar e executar

Requer .NET SDK 10 e PostgreSQL 17 (ou Docker Desktop em execução).

1. Copie `.env.example` para `.env` na raiz e escolha `POSTGRES_PASSWORD`.
   Se já existe um volume PostgreSQL, use a senha com que ele foi criado:
   mudar a variável não muda a senha dentro do volume. Preserve o volume existente.
2. Suba somente o banco:

   ```powershell
   docker compose up -d postgres
   ```

   A porta fica vinculada a `127.0.0.1:5432`. Em volumes novos o Compose executa
   `Database/001_create_jogador.sql`, que cria a tabela, índice e jogador de exemplo.
   Para um banco existente que ainda não possui a tabela, execute esse script
   usando seu cliente SQL. Não apague o volume para inicializar a tabela.
3. Configure `ConnectionStrings:Postgres`. O `appsettings.Development.json` local
   existente continua funcionando e é ignorado pelo Git. Para uma instalação nova,
   crie esse arquivo dentro de `gerenciador-pagamentos.webapi/` com:

   ```json
   {
     "ConnectionStrings": {
       "Postgres": "Host=localhost;Port=5432;Database=gerenciador_pagamentos;Username=postgres;Password=SUA_SENHA_LOCAL"
     }
   }
   ```

   Use os mesmos usuário, banco e senha do `.env`. Também é possível fornecer
   `ConnectionStrings__Postgres` por variável de ambiente. Não versione credenciais.
4. Execute:

   ```powershell
   dotnet restore .\gerenciador-pagamentos.webapi.slnx
   dotnet run --project .\gerenciador-pagamentos.webapi --launch-profile http
   ```

API: http://localhost:5277. Em desenvolvimento o documento OpenAPI está em
http://localhost:5277/openapi/v1.json. O front-end roda em http://localhost:5173.
`Cors:Origins` em `appsettings.json` permite os dois endereços locais do front.

## Contrato HTTP

| Operação | Rota | Resposta |
|---|---|---|
| Listar | `GET /api/Jogador?pagina=1&tamanhoPagina=10&busca=ana&situacao=pendente` | 200, página e resumo |
| Consultar | `GET /api/Jogador/{id}` | 200 ou 404 |
| Cadastrar | `POST /api/Jogador` | 201, jogador e cabeçalho Location |
| Editar | `PUT /api/Jogador/{id}` | 200 ou 404 |
| Excluir | `DELETE /api/Jogador/{id}` | 204 ou 404 |
| Processo ativo | `GET /health` | 200 |
| Banco/tabela disponível | `GET /health/ready` | 200 ou 503 |

Cadastro/edição recebem `nome`, `sobrenome`, `totalPago`, `totalPagar`,
`urlImagem` e `urlImagemComprovante`. Nomes são obrigatórios e têm até 100 caracteres;
totais são obrigatórios, não negativos, com até duas casas decimais e máximo
99.999.999,99. URLs opcionais aceitam apenas http/https e até 500 caracteres.
PUT substitui os seis campos; não aceita situação ou saldo calculados pelo cliente.

O GET da listagem retorna `{ itens, pagina, tamanhoPagina, total, totalPaginas, resumo }`.
Esse contrato paginado substitui a lista simples anterior. O tamanho da página
varia de 1 a 100. Situação: `todos`, `quitado` ou `pendente`. A busca é literal,
ignora maiúsculas/minúsculas e pesquisa nome e sobrenome. O resumo abrange todos
os resultados do filtro, inclusive os que não estão na página atual.

Quitado significa `totalPago >= totalPagar`; saldo pendente é
`max(totalPagar - totalPago, 0)`. Pagamento maior que o previsto é permitido,
conforme a regra original, e não compensa o saldo de outro jogador no resumo.

Validações retornam 400 com `ValidationProblemDetails.errors` por campo.
Exceções retornam `ProblemDetails` sem stack trace/detalhes internos, com `traceId`.
Consultas e comandos propagam cancelamento e usam parâmetros SQL. Cada gravação
é um único comando atômico; itens e resumo são consultados na mesma transação de leitura.

## Validar

```powershell
dotnet build .\gerenciador-pagamentos.webapi.slnx --no-restore
dotnet test .\gerenciador-pagamentos.webapi.slnx --no-restore
dotnet format .\gerenciador-pagamentos.webapi.slnx --verify-no-changes --no-restore
```

Os testes exercitam as rotas reais via WebApplicationFactory, validações, cálculo
de saldo, status HTTP, cancelamento e falhas. Os repositories são substituídos
por memória isolada: os testes não acessam nem alteram seu PostgreSQL.
O teste de repositories PostgreSQL é opt-in: defina `TEST_POSTGRES_CONNECTION_STRING`
para um banco **local de testes**, com permissão de criar schemas, e execute `dotnet test`.
Ele cria um schema com nome aleatório, testa CRUD, filtros, paginação, resumo e busca
literal, e remove somente esse schema ao terminar. Sem essa variável, o teste é
explicitamente marcado como ignorado. Não utilize uma conexão de produção.
O arquivo `gerenciador-pagamentos.webapi.http` contém exemplos manuais do CRUD.

Não há histórico de parcelas nem upload nesta versão. A aplicação não aplica
migrações automaticamente sobre bancos existentes. A API não exige autenticação;
utilize-a localmente e defina autenticação/permissões antes de publicar na internet.
