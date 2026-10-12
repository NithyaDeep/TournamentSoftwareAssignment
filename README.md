# Traffic Cameras

An implementation of the three-part assignment built on `data/cameras-defb.csv`:
a **CLI** search tool, a **REST API**, and a **web application** with a map and a
grouped data table.

Built with .NET 8 and React (TypeScript, Vite, Leaflet).

---

## Tech stack

| Area | Used |
| --- | --- |
| Backend | .NET 8, ASP.NET Core Minimal API, Swagger (Swashbuckle) |
| Frontend | React 18, TypeScript, Vite, Leaflet + OpenStreetMap |
| Tests | xUnit (backend), Vitest (frontend) |
| Data | CSV file, read once into memory |

## Prerequisites

- .NET 8 SDK
- Node.js 18 or later

---

## Getting started

### Build and test

```bash
dotnet build
dotnet test          
```

### CLI

Search cameras by part of their name:

```bash
dotnet run --project src/Cameras.Cli -- --name Neude
```

```
501 | UTR-CM-501 Neude rijbaan voor Postkantoor | 52.093421 | 5.118278
503 | UTR-CM-503 Neude plein | 52.093448 | 5.118536
504 | UTR-CM-504 Neude / Schoutenstraat | 52.092995 | 5.119088
505 | UTR-CM-505 Neude / Drakenburgstraat / Vinkenurgstraat | 52.092843 | 5.118351
506 | UTR-CM-506 Vinkenburgstraat / Neude | 52.092378 | 5.117902
507 | UTR-CM-507 Vinkenburgstraat richting Neude | 52.092234 | 5.117766
```

### API

```bash
dotnet run --project src/Cameras.Api
```

Swagger UI: <http://localhost:5141/swagger>

### Web application

With the API running, in a second terminal:

```bash
cd web
npm install
npm run dev          # http://localhost:5173
npm test             
```

---

## Endpoints

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/cameras` | All cameras. |
| `GET` | `/api/cameras?name={text}` | Cameras whose name contains `{text}`, case-insensitive. |

```json
[
  {
    "number": 503,
    "name": "UTR-CM-503 Neude plein",
    "latitude": 52.093448,
    "longitude": 5.118536
  }
]
```

---

## Architecture

Clean architecture — dependencies point inwards only.

---

## Assumptions

- **Four columns, not five.** The assignment asks for five columns and then lists four rules.
  The supplied `code/index.html` contains four tables, so four columns are built.
- **Column upper bounds are exclusive**, so camera 600 would fall in the 600–700 column.
  No camera in this dataset sits on a boundary.
- **Search is case-insensitive** and matches anywhere in the full camera name, including
  the `UTR-CM-nnn` prefix.
- **Unreadable rows are skipped**, not fatal. The CLI reports them on stderr; the API logs
  them at start-up.
- **Grouping into columns happens in the front end**, since the assignment frames it as a
  display rule.

---

## Notes

- The dataset contains **88 cameras and one corrupt row** (line 54 is a database error
  message rather than a camera).
- **The 600–700 column is empty** for this data. It is still rendered, with a message,
  rather than hidden.
- **Two different cameras share the number 545**, so the camera number is never used as a
  unique key.
- Camera names use two formats — `UTR-CM-501 Neude` and `UTR-CM-544-Janskerkhof` — so the
  number is extracted with a regular expression rather than by splitting on a space.
- Coordinates are parsed and printed with `InvariantCulture`, so a machine using a decimal
  comma still reads `52.093421` correctly.
- Not included, in the interest of scope: marker clustering for the dense city-centre
  cameras, API integration tests, and Docker/CI.

Data source:
[Cameraregister Utrecht](https://data.overheid.nl/dataset/utrecht-cameraregister-utrecht)
