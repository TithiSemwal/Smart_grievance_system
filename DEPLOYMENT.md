# Deployment Guide

## What this sets up

A Docker-based pipeline that builds automatically on every push to `main`,
independent of which host you eventually pick:

- `SmartGrievanceSystem.web/Dockerfile` — builds the ASP.NET Core MVC app
- `ai_service/Dockerfile` — builds the FastAPI AI service
- `docker-compose.yml` — runs web + AI service + SQL Server together locally
- `.github/workflows/build-and-publish.yml` — on every push to `main`:
  1. Builds the .NET solution (fails fast on compile errors)
  2. Builds both Docker images
  3. Pushes them to GitHub Container Registry (GHCR) as
     `ghcr.io/<your-org>/smart-grievance-web` and
     `ghcr.io/<your-org>/smart-grievance-ai`

GHCR is free for public repos and needs no extra secrets — it authenticates
with the built-in `GITHUB_TOKEN`. Once images land there, any host that can
pull a Docker image (Azure, Render, Railway, Fly.io, a college VM) can run
them without a rebuild step.

## Run it locally first

```bash
docker compose up --build
```

- Web app: http://localhost:8080
- AI service: http://localhost:8000/docs (FastAPI's interactive docs)
- SQL Server: localhost:1433 (sa / YourStrong!Passw0rd — dev only, never use
  this password anywhere real)

The web app currently has no controllers beyond the default `HomeController`,
so you'll see the stock ASP.NET landing page — that's expected at this stage.

## Enable GitHub Container Registry publishing

1. Push these files to `main`.
2. Go to the repo → **Settings → Actions → General → Workflow permissions**
   and make sure "Read and write permissions" is selected (needed for the
   workflow to push to GHCR).
3. Go to repo → **Packages** after the first successful run to see the
   published images.

## Once you pick a host

**Azure (recommended if you want free SQL Server + Student credits)**
- Create an Azure SQL Database (free tier) and an Azure App Service for
  Containers (or Container Apps) pointing at
  `ghcr.io/<org>/smart-grievance-web:latest`.
- Same for the AI service as a second App Service / Container App.
- Set `ConnectionStrings__DefaultConnection` as an App Service application
  setting (not committed to the repo).
- I can write the Azure-specific GitHub Actions deploy step once you have
  the resource names / publish profile.

**Render / Railway**
- Both support "deploy from a Docker image" directly from GHCR.
- SQL Server isn't available as a managed free service on either — you'd
  either run SQL Server in a container there (works, but no free persistent
  disk on the free tier) or use Azure SQL free tier as the DB while hosting
  compute on Render/Railway.

**A VM you control**
- `docker compose up -d` on the VM works as-is once you swap the SQL
  password and point a reverse proxy (e.g. Caddy or nginx) at ports 8080/8000.

## Secrets checklist (never commit these)

- Real SQL connection string / password
- Any AI service API keys once real models are added
- These should be set as GitHub Actions secrets and/or host-level
  environment variables, referenced via `${{ secrets.NAME }}` in the workflow
  or the host's env-var settings — not written into `appsettings.json`.
