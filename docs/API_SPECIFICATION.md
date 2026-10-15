# API specification

Version prefix `/api`. JSON DTOs, not EF entities. Unauthorized calls return 401; forbidden 403; validation failures 400 ProblemDetails; conflicts 409; missing resources 404. All list endpoints accept `page`, `pageSize` (maximum 100), filtering and a whitelist of sort keys. Mutation endpoints require authentication and role policy; device commands require Farm Manager or Admin and an audit record.

Endpoints by phase:
- P0: GET /health/live, GET /health/ready, GET /api/barns/{id}/environment/latest, GET /api/barns/{id}/environment/history.
- P1: POST /api/auth/login, POST /api/auth/logout, GET /api/auth/me; farms and barns CRUD; devices and commands; feeding schedules/logs; water consumption/statistics; alerts acknowledge/resolve; reports CSV.
- P2: additional reports and external notifications.

Command POST request: `{commandId,kind,parameters,expiresAtUtc}`; return 202 with command ID and queued state. GET command status reports published, accepted, applied, failed or timed out. No API response claims physical execution until Edge observation arrives.

SignalR hub `/hubs/monitoring` sends `telemetryUpdated`, `deviceStateChanged`, `alertChanged` with barn-scoped authorization. On reconnect, client reloads latest data through REST.
