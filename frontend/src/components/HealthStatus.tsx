import React, { useState, useEffect, useCallback } from 'react';

export interface HealthData {
  status: 'Healthy' | 'Degraded' | 'Unhealthy' | 'Offline' | 'Loading';
  serviceName: string;
  endpointUrl: string;
  latencyMs?: number;
  lastChecked?: string;
  payload?: Record<string, unknown>;
  error?: string;
}

interface ServiceConfig {
  name: string;
  url: string;
  fallbackUrl: string;
}

const SERVICES: ServiceConfig[] = [
  {
    name: 'Main App (Web API)',
    url: '/api/app-health',
    fallbackUrl: 'http://localhost:5000/health',
  },
  {
    name: 'Processing Worker Service',
    url: '/api/processing-health',
    fallbackUrl: 'http://localhost:5001/health',
  },
];

export const HealthStatus: React.FC = () => {
  const [healthStates, setHealthStates] = useState<Record<string, HealthData>>({
    'Main App (Web API)': {
      status: 'Loading',
      serviceName: 'Main App (Web API)',
      endpointUrl: 'http://localhost:5000/health',
    },
    'Processing Worker Service': {
      status: 'Loading',
      serviceName: 'Processing Worker Service',
      endpointUrl: 'http://localhost:5001/health',
    },
  });

  const [isRefreshing, setIsRefreshing] = useState(false);
  const [autoRefresh, setAutoRefresh] = useState(true);

  const checkServiceHealth = async (svc: ServiceConfig): Promise<HealthData> => {
    const startTime = performance.now();
    try {
      // Attempt Vite proxy first, fallback to direct URL
      let response: Response;
      try {
        response = await fetch(svc.url, { cache: 'no-store' });
      } catch {
        response = await fetch(svc.fallbackUrl, { credentials: 'omit', cache: 'no-store' });
      }

      const elapsed = Math.round(performance.now() - startTime);

      if (!response.ok) {
        return {
          status: 'Unhealthy',
          serviceName: svc.name,
          endpointUrl: svc.fallbackUrl,
          latencyMs: elapsed,
          lastChecked: new Date().toLocaleTimeString(),
          error: `HTTP Error: ${response.status} ${response.statusText}`,
        };
      }

      const data = await response.json();
      return {
        status: data.status === 'Healthy' ? 'Healthy' : 'Degraded',
        serviceName: svc.name,
        endpointUrl: svc.fallbackUrl,
        latencyMs: elapsed,
        lastChecked: new Date().toLocaleTimeString(),
        payload: data,
      };
    } catch (err) {
      const elapsed = Math.round(performance.now() - startTime);
      return {
        status: 'Offline',
        serviceName: svc.name,
        endpointUrl: svc.fallbackUrl,
        latencyMs: elapsed,
        lastChecked: new Date().toLocaleTimeString(),
        error: err instanceof Error ? err.message : 'Connection refused / service stopped',
      };
    }
  };

  const refreshAll = useCallback(async () => {
    setIsRefreshing(true);
    const results = await Promise.all(SERVICES.map((s) => checkServiceHealth(s)));
    const updated: Record<string, HealthData> = {};
    results.forEach((r) => {
      updated[r.serviceName] = r;
    });
    setHealthStates(updated);
    setIsRefreshing(false);
  }, []);

  useEffect(() => {
    refreshAll();

    if (!autoRefresh) return;
    const interval = setInterval(refreshAll, 5000);
    return () => clearInterval(interval);
  }, [refreshAll, autoRefresh]);

  const getStatusBadge = (status: HealthData['status']) => {
    switch (status) {
      case 'Healthy':
        return <span className="badge badge-healthy">● Healthy</span>;
      case 'Degraded':
        return <span className="badge badge-degraded">▲ Degraded</span>;
      case 'Offline':
      case 'Unhealthy':
        return <span className="badge badge-offline">✕ Offline</span>;
      default:
        return <span className="badge badge-loading">◌ Checking...</span>;
    }
  };

  return (
    <div className="health-container">
      <div className="health-header">
        <div>
          <h2>System Health Status</h2>
          <p className="subtitle">
            Live diagnostic status for Main App and Processing Worker.
          </p>
        </div>

        <div className="health-controls">
          <label className="toggle-label">
            <input
              type="checkbox"
              checked={autoRefresh}
              onChange={(e) => setAutoRefresh(e.target.checked)}
            />
            Auto-refresh (5s)
          </label>

          <button
            className="refresh-btn"
            onClick={refreshAll}
            disabled={isRefreshing}
          >
            {isRefreshing ? 'Refreshing...' : '↻ Check Now'}
          </button>
        </div>
      </div>

      <div className="cards-grid">
        {SERVICES.map((svc) => {
          const item = healthStates[svc.name];
          const isOk = item?.status === 'Healthy';

          return (
            <div key={svc.name} className={`card ${isOk ? 'card-ok' : 'card-warn'}`}>
              <div className="card-top">
                <div>
                  <h3 className="card-title">{svc.name}</h3>
                  <code className="card-url">{item?.endpointUrl}</code>
                </div>
                <div>{getStatusBadge(item?.status || 'Loading')}</div>
              </div>

              <div className="card-metrics">
                <div className="metric">
                  <span className="metric-label">Latency:</span>
                  <span className="metric-value">
                    {item?.latencyMs !== undefined ? `${item.latencyMs} ms` : '—'}
                  </span>
                </div>
                <div className="metric">
                  <span className="metric-label">Last Checked:</span>
                  <span className="metric-value">{item?.lastChecked || '—'}</span>
                </div>
              </div>

              {item?.error && (
                <div className="error-box">
                  <strong>Warning:</strong> {item.error}
                  <div className="hint">
                    Check if the process is running on port{' '}
                    {svc.name.includes('App') ? '5000' : '5001'}.
                  </div>
                </div>
              )}

              {item?.payload && (
                <div className="payload-box">
                  <span className="payload-title">Response JSON:</span>
                  <pre>{JSON.stringify(item.payload, null, 2)}</pre>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};
