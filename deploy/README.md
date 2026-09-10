# MailForge Deployment

## CI/CD Overview

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `.github/workflows/ci.yml` | PR + push to `main` / `cursor/**` | Build, test, lint, verify Docker images |
| `.github/workflows/cd.yml` | Push to `main`, tags `v*.*.*`, manual | Publish images to GHCR, staging/production deploy hooks |
| `.github/workflows/release.yml` | Tags `v*.*.*` | GitHub Release + publish artifacts |

## Container images

Images are published to GitHub Container Registry:

```text
ghcr.io/<owner>/mailforge-api:<tag>
ghcr.io/<owner>/mailforge-worker:<tag>
ghcr.io/<owner>/mailforge-frontend:<tag>
```

Tags:
- `latest` — latest `main` build
- `sha-<commit>` — immutable commit build
- `v1.2.3` — semver release tag

## Deploy to a server with Docker Compose

1. Copy `.env.example` to `.env` and set production secrets.
2. Set image variables:

```bash
export IMAGE_API=ghcr.io/premprakaash/mailforge-api:latest
export IMAGE_WORKER=ghcr.io/premprakaash/mailforge-worker:latest
export IMAGE_FRONTEND=ghcr.io/premprakaash/mailforge-frontend:latest
```

3. Start stack:

```bash
docker compose -f docker-compose.yml -f deploy/docker-compose.prod.yml pull
docker compose -f docker-compose.yml -f deploy/docker-compose.prod.yml up -d
```

## GitHub configuration

### Repository variables (optional)

| Variable | Example |
|----------|---------|
| `PUBLIC_API_URL` | `https://api.mailforge.example.com` |
| `STAGING_URL` | `https://staging.mailforge.example.com` |
| `PRODUCTION_URL` | `https://app.mailforge.example.com` |

### Secrets for automated SSH deploy (optional)

| Secret | Description |
|--------|-------------|
| `STAGING_HOST` | Staging server hostname |
| `STAGING_USER` | SSH user |
| `STAGING_SSH_KEY` | Private key for staging deploy |
| `PRODUCTION_HOST` | Production server hostname |
| `PRODUCTION_USER` | SSH user |
| `PRODUCTION_SSH_KEY` | Private key for production deploy |

## Release process

```bash
git checkout main
git pull
git tag v1.0.0
git push origin v1.0.0
```

This triggers:
1. CD workflow — publish tagged images to GHCR
2. Release workflow — create GitHub Release with artifacts
