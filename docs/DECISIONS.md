# Technical decisions

- D-001: Target .NET 8 with SDK 8.0.426 and runtime 8.0.32, as requested. Microsoft and EF Core packages use 8.0.32; MQTTnet 5.2.0.1603 and Serilog.AspNetCore 8.0.3 remain compatible. .NET 8 support ends 2026-11-10, so a later upgrade must be planned before production use.
- D-002: Modular monolith. Separate Web, API, Application, Domain, Infrastructure and Contracts projects. The API currently owns database writes and MQTT ingestion. Web consumes API only.
- D-003: Initial livestock species is deliberately unassigned. No universal safety thresholds are seeded. Demo sensor values are synthetic and cannot authorize real hardware control.
- D-004: Edge must enforce local minimum ventilation and interlocks. Cloud commands are requests; ACK and observed actuator state are distinct.
- D-005: MQTT commands are never retained. QoS 1 plus durable commandId deduplication is required for effects. MQTT delivery alone never guarantees exactly once physical operation.
- D-006: Persist UTC timestamps as SQL Server datetimeoffset; telemetry stores measuredAt and receivedAt separately. Water flow is line consumption, not measured drinking.
- D-007: Authentication and roles must be enforced by API for commands and management endpoints. This is planned, not yet implemented.
- D-008: MVP assumes simulator only. Real hardware integration requires an approved equipment profile, local fail-safe behavior and physical acceptance tests.
- D-009: Local development uses SQL Server LocalDB and a native Mosquitto process on loopback port 1884. Docker is optional later and no longer part of the required setup.
- D-010: Schema migration is an explicit deployment step rather than automatic API startup action.
- D-011: Current MQTT subscriber needs durable-session/replay and failure tests before ingestion is marked done.
