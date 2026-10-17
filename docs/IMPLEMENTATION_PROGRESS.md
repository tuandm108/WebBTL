# Implementation progress

| ID | Status | Files | Verification | Outstanding |
|---|---|---|---|---|
| F01 | DONE | global.json, SmartLivestock.sln, src/, simulators/, tests/ | SDK 8 build: 0 warnings, 0 errors; 6 tests pass on runtime 8.0.32 | none for solution foundation |
| F02 | DONE | scripts/setup-local-mqtt.ps1, deploy/native/ (ignored) | SQL Server LocalDB connected; native Mosquitto 2.1.2 listens on loopback port 1884 with password | service deployment and TLS are later concerns |
| F03 | DONE | AppDbContext, InitialSqlServer migration | migration applied to LocalDB; farm, barn, device seed and tables verified via API | remaining domain tables belong to later modules |
| F04 | IN_PROGRESS | TelemetryMessage, SensorSimulator | telemetry published to native broker | state, ACK and remaining fault scenarios |
| F05 | IN_PROGRESS | TelemetryMqttService | live simulator data stored in SQL Server and returned by API | durable replay and duplicate/failure tests |
| F06 | IN_PROGRESS | API Program, MonitoringHub | /health/ready returned 200; latest endpoint returned four sensor metrics | SignalR browser event and history test |
| F07 | IN_PROGRESS | Blazor Home.razor (paged list of monitored barns), Monitor.razor (one barn) | HTTP server render returned four readings | interactive update/reconnect browser verification |
| F08–F09 | TODO | — | not run | implement in dependency order |
| F10 | IN_PROGRESS | BarnEndpoints, Barns.razor, SeedTwentyBarns migration | barn CRUD and monitored flag (AddBarnMonitored migration, GET /api/monitoring/barns paging) verified live over HTTP; 20 seeded barns each receive simulator telemetry | farm and device management, auth (F08) and audit; delete refused while a barn has devices |
| F13 | IN_PROGRESS | Alert entity and AddAlerts migration, AlertTracker, AlertEndpoints, AlertFeed, AlertCenter dialog, Alerts.razor history | simulator DEMO_ALERTS scenario produced raised, escalated and cleared alerts; SignalR push opened the live dialog; summary dialog on first open then the history page verified in a browser | acknowledge (needs F08), on-delay, offline alerts, real per-profile thresholds; digest schedule (daily, interval, immediate, off) plus urgent banner verified in a browser; read/unread state (AddAlertReadAt) verified over HTTP and in a browser |
| F11, F12, F14–F16 | TODO | — | not run | implement in dependency order |

Status values: TODO, IN_PROGRESS, BLOCKED, DONE. No telemetry/control module is marked DONE before live integration and failure tests pass.
