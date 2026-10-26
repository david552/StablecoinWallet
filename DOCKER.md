# Local Docker development

Requires Docker Desktop running with Linux containers.

From the repository root:

```powershell
Copy-Item .env.example .env
# Edit .env and set POSTGRES_PASSWORD to your local development password.
docker compose up --build -d
docker compose logs -f api
```

- API liveness: http://localhost:8080/health (does not test database connectivity).
- Development OpenAPI: http://localhost:8080/openapi/v1.json.
- PostgreSQL from the host: localhost:5433, database stablecoin_wallet, user wallet.
- PostgreSQL from the API container: postgres:5432.
- Ports may be changed in .env.
- The API currently has no payment endpoints.

The PostgreSQL volume persists data across container recreation.
Stop containers without deleting data:

```powershell
docker compose down
```

The stack creates the PostgreSQL database, but does not create application tables.
EF migrations and atomic ledger persistence are the next implementation step.
No EnsureCreated or automatic migration execution is enabled.

The Account concurrency token uses PostgreSQL's built-in xmin system column.
Npgsql maps the uint Version property with IsRowVersion(); application code must
not assign or increment it. Concurrent updates raise DbUpdateConcurrencyException;
handling/retrying the full operation is a later application-layer step.

This Compose setup is for local development. The .env file is ignored by Git and
excluded from the Docker build context. Do not commit real credentials.

## EF migrations from the host

Restore the pinned repository-local EF tool:

```powershell
dotnet tool restore
```

Set ConnectionStrings__WalletDatabase in the current shell (or use development
user secrets) to the host connection: Host=localhost;Port=5433;
Database=stablecoin_wallet;Username=wallet;Password=<password from your .env>.
The EF command does not automatically load .env. Do not commit the password.
If you changed POSTGRES_PORT, use that port instead.

```powershell
dotnet ef database update --project StablecoinWallet.Infrastructure --startup-project StablecoinWallet
dotnet ef migrations has-pending-model-changes --project StablecoinWallet.Infrastructure --startup-project StablecoinWallet
```

Generated migrations and the model snapshot live in
StablecoinWallet.Infrastructure/Persistence/Migrations and should be committed.