# OrbitView

A full-stack satellite tracking application that visualizes real-time orbital data in 3D. OrbitView fetches live TLE (Two-Line Element) data from CelesTrak, stores it in a database, and renders satellite positions on an interactive 3D globe.

---

## What it does

- Tracks real satellites using live TLE data fetched automatically every hour from CelesTrak
- Renders satellites on an interactive 3D globe using Three.js and React Three Fiber
- Lets users browse, search, and filter satellites by category, name, or active status
- Allows authenticated users to save favourite satellites with personal notes
- Admin panel for managing satellite metadata and manually triggering TLE refreshes
- JWT-based authentication with role-based access control (User / Admin)

---

## Tech Stack

**Backend** — ASP.NET Core (.NET 8)
- REST API with JWT authentication and RBAC
- Entity Framework Core with MySQL
- Background service for automated hourly TLE fetching
- Repository pattern for clean data access
- Swagger UI for API documentation

**Frontend** — React + Vite
- Three.js / React Three Fiber for 3D satellite rendering
- `satellite.js` for orbital propagation from TLE data
- React Leaflet for 2D map view
- React Router for client-side navigation
- Axios for API communication

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- MySQL database

### Backend Setup

```bash
cd OrbitView.Api
```

Create an `appsettings.json` with the following structure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=orbitview;user=root;password=yourpassword"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-chars",
    "Issuer": "OrbitView",
    "Audience": "OrbitViewUsers",
    "ExpiryHours": 12
  }
}
```

Run migrations and start the API:

```bash
dotnet ef database update
dotnet run
```

The API will be available at `https://localhost:7110`. Swagger UI at `/swagger`.

### Frontend Setup

```bash
cd orbitview-frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173`.

---

## API Overview

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | Public | Register a new user |
| POST | `/api/auth/login` | Public | Login and receive JWT |
| GET | `/api/auth/me` | User | Get current user profile |
| GET | `/api/satellites` | Public | List satellites with filters |
| GET | `/api/satellites/{id}` | Public | Get satellite detail |
| GET | `/api/favourites` | User | Get saved favourites |
| POST | `/api/favourites` | User | Save a satellite |
| DELETE | `/api/favourites/{id}` | User | Remove a favourite |
| GET | `/api/admin/status` | Admin | System status and TLE logs |
| POST | `/api/admin/tle-refresh` | Admin | Manually trigger TLE fetch |

---

## How TLE fetching works

A background service (`TleFetcherService`) runs on startup and then every hour. It pulls GP data in JSON format from CelesTrak for each active satellite, parses the orbital elements, reconstructs TLE lines, and stores the record in the database. Previous TLE records are marked as non-current rather than deleted, preserving orbital history.

---

## Project Structure

```
OrbitView.Api/
├── BackgroundServices/   # Automated TLE fetcher
├── Controllers/          # API endpoints
├── Data/                 # EF Core DbContext
├── DTOs/                 # Request/response models
├── Models/               # Database entities
├── Repositories/         # Data access layer
└── Services/             # Business logic

orbitview-frontend/
└── src/                  # React components and pages
```

---

## Author

Jori — [github.com/JoriLilo](https://github.com/JoriLilo)
