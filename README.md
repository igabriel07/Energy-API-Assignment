# Stellar Blue Energy API Assignment

## Project Overview
This project is the implementation of the **Backend Engineer assignment** for **Stellar Blue**.  
It is an **ASP.NET Core 8 Web API** designed to:

- Integrate with an external proprietary data series API.  
- Retrieve **Market Clearing Price (MCP)** data.  
- Process timeseries data to compute **daily averages**.  
- Store results in a persistent **SQLite database** (using Entity Framework Core).  

The solution covers all **core requirements** and includes several **advanced features** such as automatic scheduling and full Docker containerization, showcasing a robust and production-ready architecture.

---

## Technologies & Implementation
- **Platform:** .NET 8 (ASP.NET Core Web API)  
- **Database:** SQLite (Entity Framework Core)  
- **API Client:** `HttpClientFactory` for connection management  
- **Authentication:** Automatic Bearer Token acquisition, caching, and refresh logic  
- **Data Persistence:** Upsert (Update or Insert) logic to prevent duplication  
- **Containerization:** Docker & Docker Compose  
- **Scheduling:** `IHostedService` (Background Service) for automated updates  

---

## Application Execution (Docker - Recommended)
The application is fully containerized, making execution portable and consistent across environments.

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running  
- Terminal access (CMD / PowerShell / Bash) in the project root (where `docker-compose.yml` is located)  

### Execution Steps
1. **Build & Run (Initial Setup):**
   ```bash
   docker compose up --build -d

2. **Verify Status:**
   Confirm that the container is running:
   ```Bash
   docker ps
3. **Access API & Swagger:**
   The API is exposed on your machine's port 8080.
   - Swagger UI (Documentation): http://localhost:8080/docs
   - Base API URL: http://localhost:8080/api/energy

### Data Persistence
A Docker Volume is configured in docker-compose.yml to persist the energy_data.db file. This ensures that all stored data is retained even if the Docker container is stopped, restarted, or removed.

## API Endpoints
The API exposes the following endpoints, documented and testable via the Swagger UI:

| Method | Route | Description |
| :--- | :---: | ---: |
| POST | /api/energy/ImportData/{dateFrom}/{dateTo} | Retrieves raw MCP data from the external API for the specified date range, calculates the daily average price, and stores/updates the results in the database. |
| GET | /api/energy/GetData/{dateFrom}/{dateTo} | Returns the stored daily average energy prices (AveragePrice) from the database for the requested range. |
