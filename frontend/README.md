# Frontend (React + TypeScript + Vite)

This is the web UI for the Personal Document Organizer.

---

## Features Implemented in Phase 0

* **Live System Health Monitor (`src/components/HealthStatus.tsx`):**
  * Polls and monitors the diagnostic health of **Main App** (`http://localhost:5000/health`) and **Processing Worker** (`http://localhost:5001/health`).
  * Real-time latency calculation (ms).
  * Auto-refresh toggle (every 5 seconds) and manual "Check Now" button.
  * Status badges (`Healthy`, `Degraded`, `Offline`).
  * Live response JSON payload viewer.

---

## Running Locally

1. **Install dependencies:**
   ```bash
   cd frontend
   npm install
   ```

2. **Start the development server:**
   ```bash
   npm run dev
   ```
   Open **[http://localhost:5173](http://localhost:5173)** in your browser.

3. **Proxy Configuration:**
   `vite.config.ts` automatically proxies:
   * `/api/app-health` $\rightarrow$ `http://localhost:5000/health`
   * `/api/processing-health` $\rightarrow$ `http://localhost:5001/health`
