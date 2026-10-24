# Infrastructure & Deployment

This directory contains container configurations and orchestration scripts:
- `docker-compose.yml`: Multi-container topology for local development (SQL Server 2025, RabbitMQ, Jaeger, Main App, Processing Worker).
- `Dockerfile.app`: Container build recipe for `Organizer.App`.
- `Dockerfile.processing`: Container build recipe for `Organizer.Processing`.

