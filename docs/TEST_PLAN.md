# Test plan

P0 acceptance:
1. Solution restores and builds without warnings treated as errors.
2. Domain and simulator contract unit tests pass.
3. SQL Server LocalDB accepts the migration; native Mosquitto accepts authenticated loopback connections on port 1884.
4. A simulator reading travels through MQTT, backend, SQL Server, SignalR and Blazor; measuredAt, receivedAt and quality remain intact.
5. Duplicate and stale messages do not create duplicate readings or trigger control.
6. API readiness fails when required dependencies fail; liveness remains available.

Further tests:
- Identity login, roles and backend command authorization.
- Feeding occurrence key survives backend/Edge restart and duplicate delivery.
- ACK timeout, rejected command, actuator state mismatch and offline handling.
- Sensor fault, network loss, broker loss and database loss.
- Profile-specific threshold and hysteresis unit tests. No tests assert a universal livestock threshold.
- Baseline and anomaly tests with missing data and changing batch count.
- SQL Server migration and backup restore drill.
- Target performance: specify test load and dataset before measuring p95 API latency and dashboard delay.

Use xUnit for unit tests, WebApplicationFactory for HTTP integration and native LocalDB/Mosquitto for local integration. Docker and Testcontainers are optional later. Record actual command output and environment in IMPLEMENTATION_PROGRESS.md; never mark unrun tests as passed.
