# webFairAI

FairAI is a full-stack AI governance and fairness chat application built with ASP.NET Core and Next.js.

## Preview
https://xoarty1002-001-site1.ctempurl.com/

## Stack
- ASP.NET Core Web API
- Entity Framework Core with MySQL support and in-memory fallback for local development
- Next.js frontend with a chat interface
- Docker Compose for local development
- GitHub Actions for deployment automation

## Local development

### Backend
```bash
dotnet restore
DOTNET_ENVIRONMENT=Development dotnet run --project FairAI.Api --urls http://localhost:5124
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

The frontend expects the API at:
- http://localhost:5124

## Docker
```bash
docker compose up --build
```

This starts:
- MySQL on port 3306
- API on port 5124
- Frontend on port 3000

## Deployment notes
- The app is configured for GitHub Actions deployment to myasp.net.
- The hosting environment expects a branch named `xoarty1002-001` to exist on the remote repository.
- The workflow triggers on both `main` and `xoarty1002-001`.

## Verification
The project has been validated with:
- `dotnet test FairAI.Tests/FairAI.Tests.csproj`
- `npm run build --prefix frontend`
- API health check and chat endpoint response tests
