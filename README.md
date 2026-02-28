# Instant Wellness Kits — Full Stack

C# .NET 8 backend + React frontend for the Instant Wellness drone delivery ordering system.

## Quick Start

### One command (recommended)

```bash
./run.sh
```

Starts backend + frontend. Opens http://localhost:5173. Press Ctrl+C to stop both.

### Manual start

**1. Backend (API)**

```bash
cd src/InstantWellness.Api
dotnet run
```

API runs at **http://localhost:5046**. Swagger: http://localhost:5046/swagger

**2. Frontend**

```bash
cd frontend
npm install
npm run dev
```

Frontend runs at **http://localhost:5173**. API requests are proxied to the backend.

**3. Run both**

1. Start the backend in one terminal.
2. Start the frontend in another.
3. Open http://localhost:5173 in the browser.

## Project Structure

```
├── src/InstantWellness.Api/     # C# Web API
├── src/InstantWellness.Application/
├── src/InstantWellness.Domain/
├── src/InstantWellness.Infrastructure/
└── frontend/                    # React + Vite + Tailwind
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/orders` | Create order |
| POST | `/orders/import` | Import CSV |
| GET | `/orders` | List orders (pagination + filters) |
| GET | `/orders/{id}` | Order details with tax breakdown |

