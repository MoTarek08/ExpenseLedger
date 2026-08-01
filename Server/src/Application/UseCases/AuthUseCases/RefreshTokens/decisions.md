# Refresh Tokens — Key Decisions

## Unauthenticated Endpoint
The refresh endpoint intentionally lacks `[Authorize]` because the access token may be expired. The endpoint manually validates the Authorization header and extracts the access token for validation by the token service.

## Header Validation in a Static Helper
Authorization header parsing is extracted to `AuthHeaderValidator` to keep the controller thin. It returns `Result<string>` — failure on missing/bad header produces a structured `ProblemDetails` response instead of a raw string.

## Refresh Token Rotation
The old refresh token is revoked before issuing a new one. Both tokens share the same `SessionId`, allowing the server to link token generations. If a token is reused after rotation, the revoked check catches it.

## Ownership Mismatch Revokes All
If the access token's user ID does not match the refresh token's owner, all refresh tokens for the refresh token's user are revoked. This prevents token substitution attacks.

## Self-Invalidating Expiry
Expired tokens are revoked on detection and persisted before returning the failure response. This ensures the expired token cannot be replayed even if the DB state is stale.
