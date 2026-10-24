# Setup: Troubleshooting and Operational Drills

## Common Issues and Fixes

### 1. Docker Volume Permissions on macOS
* **Symptom:** Worker throws `UnauthorizedAccessException` or `EACCES` when attempting to write extracted text to `/storage/results`.
* **Fix:** Ensure the shared volume directory in Docker has appropriate read/write permissions for the container process user:
  ```bash
  # Inside container or via compose volume configuration
  docker compose exec processing chmod -R 777 /storage
  ```

---

### 2. SQL Server Container Fails to Start or Exit Code 1
* **Symptom:** `organizer-sql` keeps restarting.
* **Root Cause:** The `MSSQL_SA_PASSWORD` does not meet Microsoft SQL Server complexity requirements (must contain at least 8 characters including uppercase, lowercase, numbers, and symbols).
* **Fix:** Use a compliant password such as `YourStrong@Passw0rd`. Check container logs:
  ```bash
  docker compose logs sqlserver
  ```

---

### 3. Port Conflicts on Local Machine
* **Symptom:** `Error response from daemon: Ports are not available: listen tcp 0.0.0.0:1433: bind: address already in use`.
* **Fix:** Another local instance of SQL Server, Postgres, or RabbitMQ is using that port. Check active ports:
  ```bash
  # macOS / Linux
  lsof -i :1433
  lsof -i :5672
  ```
  Terminate the competing process or remap the host port in `infra/docker-compose.yml` (e.g. `"11433:1433"`).

---

### 4. RabbitMQ Connection Refused on Startup
* **Symptom:** App crashes on startup with `RabbitMQ.Client.Exceptions.BrokerUnreachableException`.
* **Fix:** RabbitMQ can take 5–10 seconds to fully initialize its AMQP listeners. Ensure your `docker-compose.yml` includes a `healthcheck` on RabbitMQ and that the App specifies:
  ```yaml
  depends_on:
    rabbitmq:
      condition: service_healthy
  ```

---

## Failure Drills (Milestone Verification)

Run these operational drills to verify system resilience:

### Drill A: RabbitMQ Temporary Outage
1. Start the stack: `docker compose up -d`.
2. Stop RabbitMQ: `docker compose stop rabbitmq`.
3. Perform an upload in App: Verify the request does not crash; the `DocumentVersion` is persisted as `Queued` and the outbox records the message.
4. Restart RabbitMQ: `docker compose start rabbitmq`.
5. Verify: The outbox publisher automatically reconnects, flushes queued messages, and Processing finishes the job.

### Drill B: Mid-Job Worker Crash
1. Trigger processing on a large file.
2. Abruptly kill the worker: `docker compose kill processing`.
3. Restart worker: `docker compose up -d processing`.
4. Verify: Processing reads unacknowledged messages from the queue, checks idempotency, and safely completes extraction.

