# Project plan and backlog

Scope: simulator-first research MVP for one farm, barn, batch and Edge controller; model supports many. Species and operating thresholds are configurable and remain unset until reviewed. No machine learning in MVP.

| ID | Priority | Task | Depends on | Acceptance |
|---|---|---|---|---|
| F01 | P0 | Solution, package versions, build | none | 10 projects restore and build |
| F02 | P0 | SQL Server LocalDB and native Mosquitto | F01 | healthy services on Windows host |
| F03 | P0 | schema and migration | F01,F02 | migration applies and seed demo topology |
| F04 | P0 | MQTT contracts and simulator | F01,F02 | valid telemetry, state, ACK and fault scenarios |
| F05 | P0 | ingestion and dedupe | F03,F04 | one stored reading per messageId; invalid/stale isolated |
| F06 | P0 | monitoring API and SignalR | F05 | latest/history and pushed event agree |
| F07 | P0 | Blazor monitoring screen | F06 | shows quality, age and reconnect state |
| F08 | P0 | authentication and roles | F03 | unauthorized commands rejected by API |
| F09 | P0 | command lifecycle and Edge safety simulation | F04,F08 | ACK, timeout and local interlock tests |
| F10 | P1 | farm, barn and device management | F08 | CRUD validation and audit |
| F11 | P1 | feeding schedule and logs | F09 | no duplicate occurrence after restart |
| F12 | P1 | water aggregation | F05 | resets, gaps and line-loss quality handled |
| F13 | P1 | anomaly detection and alerts | F11,F12 | configurable baselines and alert dedupe |
| F14 | P1 | CSV reports | F06,F11,F12,F13 | bounded, filtered exports |
| F15 | P2 | PDF/external notifications | F14 | approved requirements |
| F16 | P2 | real hardware integration | F09 | engineering safety review and physical tests |

Delivery sequence: foundation → simulator/telemetry vertical slice → authorization/control → feeding/water → anomalies/alerts → reports → optional hardware. Sprint dates are planning estimates, not evidence of completion.
