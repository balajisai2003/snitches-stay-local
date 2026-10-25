import React from 'react';
import { HealthStatus } from './components/HealthStatus';

export const App: React.FC = () => {
  return (
    <div className="app-layout">
      <header className="navbar">
        <div className="navbar-brand">
          <div className="logo-icon">📁</div>
          <div>
            <h1 className="brand-title">Personal Document Organizer</h1>
            <span className="brand-sub">Private-First Local Intelligence</span>
          </div>
        </div>
        <div className="navbar-badge">
          <span className="phase-badge">Phase 0: Foundation</span>
        </div>
      </header>

      <main className="content">
        <HealthStatus />

        <div className="info-card">
          <h3>How to Test Health Endpoints Locally</h3>
          <ol className="steps-list">
            <li>
              <strong>Start the Main App:</strong>
              <code>dotnet run --project src/App</code>
              <span>(runs on <code>http://localhost:5000</code>)</span>
            </li>
            <li>
              <strong>Start the Processing Worker:</strong>
              <code>dotnet run --project src/Processing</code>
              <span>(runs on <code>http://localhost:5001</code>)</span>
            </li>
            <li>
              <strong>Or Run with Docker Compose:</strong>
              <code>docker compose -f infra/docker-compose.yml up -d</code>
              <span>(starts SQL Server, RabbitMQ, Jaeger, App, and Worker)</span>
            </li>
          </ol>
        </div>
      </main>

      <footer className="footer">
        Personal Document Organizer • Local AI • Zero Cloud Leakage
      </footer>
    </div>
  );
};

export default App;
