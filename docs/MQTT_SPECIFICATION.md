# MQTT contract v1

Topics: `smartlivestock/v1/{farmId}/{barnId}/{deviceId}/{telemetry|state|command|ack|config}`. IDs are immutable registered identifiers, not display names. MQTT ACL grants each device publish telemetry/state/ack and subscribe command/config for its own path only. API has the inverse rights.

Common JSON envelope: `schemaVersion` (1), `messageId` (UUID), `deviceId`, `timestampUtc` (ISO 8601), `sequence` (monotonic per device boot), `bootId` (UUID). Backend records receivedAt independently. Unknown schema versions are rejected and logged.

Telemetry payload adds `readings`: array of `{sensorId, metric, value, unit, quality}`. Allowed quality values: valid, invalid, stale. Metric units are fixed by contract: temperature C, humidity percent, nh3 ppm, h2s ppm, feed kg, water cumulative L. Device-side timestamp is never sufficient to declare freshness.

Command payload adds `commandId`, `expiresAtUtc`, `kind`, `parameters`. Edge validates expiry, authorization scope, config version and local safety interlocks before applying. No retained command. ACK payload adds `commandId`, `status` (accepted/rejected/applied/failed), `reason`, `actualState`. An accepted ACK does not mean applied.

State payload adds `online`, `observedActuators`, `lastAppliedConfigVersion` and health. Retained state may be used only with timestamp and a freshness check. Last Will publishes offline state; heartbeat interval for demo is 10 seconds, offline timeout 30 seconds.

QoS 1 for telemetry, command and ACK; consumers deduplicate by deviceId+messageId and commandId. Edge persists completed command IDs across restart before acknowledging effects. Config payload is versioned and may be retained; Edge reports applied version. Reconnect uses bounded exponential backoff.

Example telemetry:
```json
{"schemaVersion":1,"messageId":"2b9d4909-b166-4d6d-af41-e8c7fc5c3571","deviceId":"edge-001","timestampUtc":"2026-10-15T00:00:00Z","sequence":1,"bootId":"8a6d9337-6ebc-4df8-814d-df2cb41c773a","readings":[{"sensorId":"temp-01","metric":"temperature","value":27.1,"unit":"C","quality":"valid"}]}
```
