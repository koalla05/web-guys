# Instant Wellness Kits — Full Stack

> C# .NET 8 backend + React + TypeScript frontend for a drone delivery ordering system with geolocation-based tax calculation.

**Stack:** .NET 8 · ASP.NET Web API · React 18 · Vite · Tailwind CSS · Clean Architecture · Docker · Render.com

---

## Architecture

### Backend — Clean Architecture

The backend follows strict Clean Architecture with four layers and unidirectional dependencies:

| Layer | Project | Responsibility |
|-------|---------|---------------|
| Domain | `InstantWellness.Domain` | Core entities, value objects, interfaces. No external dependencies. |
| Application | `InstantWellness.Application` | Use cases, DTOs, service interfaces. Depends only on Domain. |
| Infrastructure | `InstantWellness.Infrastructure` | EF Core, external APIs, tax lookups. Implements Application interfaces. |
| API | `InstantWellness.Api` | ASP.NET controllers, middleware, DI composition root. |

### Frontend

React 18 + TypeScript built with Vite, styled with Tailwind CSS. Communicates with the backend via a centralized `src/lib/api.ts` REST client.

### Project Structure

```
InstantWellness/
├── InstantWellness.slnx
├── Dockerfile
├── run.sh
├── frontend/
│   ├── src/
│   │   ├── app/App.tsx
│   │   ├── lib/api.ts          ← API client
│   │   ├── components/
│   │   └── styles/
│   ├── vite.config.ts
│   └── package.json
└── src/
    ├── InstantWellness.Api/
    ├── InstantWellness.Application/
    ├── InstantWellness.Domain/
    └── InstantWellness.Infrastructure/
```

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/orders` | Create a single order with tax calculation |
| `POST` | `/orders/import` | Bulk import orders via CSV file |
| `GET` | `/orders` | List orders — supports pagination & filters |
| `GET` | `/orders/{id}` | Get full order detail with tax breakdown |

Swagger UI available at `http://localhost:5046/swagger` in development.

---

## Local Development

### Quick Start (one command)

```bash
./run.sh
```

Starts backend + frontend concurrently. Opens http://localhost:5173. Press `Ctrl+C` to stop both.

### Manual Start

**1. Backend**

```bash
cd src/InstantWellness.Api
dotnet run
```

API runs at **http://localhost:5046**

**2. Frontend**

```bash
cd frontend
npm install
npm run dev
```

Frontend runs at **http://localhost:5173**. Vite automatically proxies `/api` requests to the backend — no CORS issues in dev.

---

## Deployment

### Backend — Render Web Service

The backend is containerized with Docker and deployed as a Web Service on Render.

**Render Settings:**

| Setting | Value |
|---------|-------|
| Root Directory | *(leave empty)* |
| Dockerfile Path | `./Dockerfile` |
| Docker Build Context Directory | `.` (repo root) |
| Docker Command | *(leave empty — ENTRYPOINT defined in Dockerfile)* |
| Port | `8080` |

> ⚠️ The Root Directory must be **empty** and Build Context must be `.` (repo root). If set to `src/InstantWellness.Api/`, Docker won't be able to access other project layers and the build will fail.

**Live backend URL:** https://web-guys-back.onrender.com

**Dockerfile summary** (multi-stage build):

```dockerfile
# Build stage — SDK image compiles and publishes
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... restore, build, publish ...

# Runtime stage — slim image runs the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "InstantWellness.Api.dll"]
```

Render automatically injects the `PORT` environment variable.

---

### Frontend — Connecting to Deployed Backend

In local dev, Vite proxies `/api` to `localhost:5046`. In production the frontend must point directly to the Render URL.

**Step 1 — Create `frontend/.env.production`**

```env
VITE_API_URL=https://web-guys-back.onrender.com
```

**Step 2 — `src/lib/api.ts` already handles this**

```ts
const API_BASE = import.meta.env.VITE_API_URL || (import.meta.env.DEV ? '/api' : 'http://localhost:5046');
```

In dev, Vite proxies `/api` to the backend. In production, `VITE_API_URL` is read from ENVIRONMENT_VARIABLES on Render.com.

---

## Environment Variables

| Variable | Where | Value |
|----------|-------|-------|
| `ASPNETCORE_URLS` | Backend (Render) | `http://+:${PORT:-8080}` |
| `ASPNETCORE_ENVIRONMENT` | Backend (Render) | `Production` |
| `PORT` | Render (auto-injected) | `8080` |
| `VITE_API_URL` | Frontend | `https://web-guys-back.onrender.com` |

---
