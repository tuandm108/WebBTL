# Implementation progress

| ID | Status | Files | Verification | Outstanding |
|---|---|---|---|---|
| F01 | DONE | global.json, SmartLivestock.sln, src/, simulators/, tests/ | SDK 8 build: 0 warnings, 0 errors; 6 tests pass on runtime 8.0.32 | none for solution foundation |
| F02 | DONE | scripts/setup-local-mqtt.ps1, deploy/native/ (ignored) | SQL Server LocalDB connected; native Mosquitto 2.1.2 listens on loopback port 1884 with password | service deployment and TLS are later concerns |
| F03 | DONE | AppDbContext, InitialSqlServer migration | migration applied to LocalDB; farm, barn, device seed and tables verified via API | remaining domain tables belong to later modules |
| F04 | IN_PROGRESS | TelemetryMessage, SensorSimulator | telemetry published to native broker | state, ACK and remaining fault scenarios |
| F05 | IN_PROGRESS | TelemetryMqttService | live simulator data stored in SQL Server and returned by API | durable replay and duplicate/failure tests |
| F06 | IN_PROGRESS | API Program, MonitoringHub | /health/ready returned 200; latest endpoint returned four sensor metrics | SignalR browser event and history test |
| F07 | IN_PROGRESS | Blazor Home.razor | HTTP server render returned four readings | interactive update/reconnect browser verification |
| F08–F16 | TODO | — | not run | implement in dependency order |

Status values: TODO, IN_PROGRESS, BLOCKED, DONE. No telemetry/control module is marked DONE before live integration and failure tests pass.
