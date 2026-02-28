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

# Data Preparation and Handling

The tax data for New York State was obtained from the [official website of the New York State Department of Taxation and Finance](https://www.tax.ny.gov/pdf/publications/sales/pub718.pdf).

There is a pdf file with a table containing tax rates for each county or locality. 

## PDF Parsing

The table was parsed using [python script](data-pipeline/tax_rates_retrive.py) and saved in [tax_rates.csv file](data-pipeline/tax_rates.csv). 

To run the script you should run following commands:
```bash
# create and activate virtual environment
python3 -m venv venv
source venv/bin/activate

# install dependencies
pip install -r requirements.txt

# run the script
python3 tax_rates_retrive.py
```

## Reverse Geocoding

To identify the location from coordinates (latitude and longitude) for further mapping to the locations in tax_rates.csv [the Nominatim API (OpenStreetMap)](https://nominatim.openstreetmap.org/ui/search.html) was used.

Reverce mapping endpoint:

```
https://nominatim.openstreetmap.org/reverse?lat=<your_lat>&lon=<your_lon>&format=json
```

Here is an example of usage:
```
# request
https://nominatim.openstreetmap.org/reverse?lat=42.01246326237433&lon=-78.8671866447861&format=json

# response
{"place_id":322463192,"licence":"Data © OpenStreetMap contributors, ODbL 1.0. http://osm.org/copyright","osm_type":"way","osm_id":666459766,"lat":"42.0095635","lon":"-78.8640909","class":"highway","type":"footway","place_rank":27,"importance":0.040040368505378794,"addresstype":"road","name":"Finger Lakes / North Country Trail","display_name":"Finger Lakes / North Country Trail, Town of Coldspring, Corydon Township, Cattaraugus County, New York, United States","address":{"road":"Finger Lakes / North Country Trail","hamlet":"Town of Coldspring","city":"Corydon Township","county":"Cattaraugus County","state":"New York","ISO3166-2-lvl4":"US-NY","country":"United States","country_code":"us"},"boundingbox":["41.9977901","42.0177203","-78.8982344","-78.8560915"]}
```

There are city and county fields that were used for retrieving the location. They were also parsed to map the naming from tax_rates.csv.

## Tax Calculation Strategy
Tax calculation is implemented using lazy evaluation.
 - POST /orders/import saves imported orders without calculating taxes.
 - Taxes are calculated dynamically during GET requests.

## Advantages of This Approach
 - Improved user experience (no waiting during import)
 - Reduced load on the Nominatim API
 - Lower backend resource consumption
 - No unnecessary recalculations

## Future improvements

Integration with official real-time tax data portals could improve accuracy. However, such services are typically paid. Since tax rates do not change frequently, the current solution provides a reasonable balance between performance, cost efficiency and accuracy.
