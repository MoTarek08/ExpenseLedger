# Frontend Integration Conventions

This document describes the conventions the frontend must follow when integrating with ExpenseLedger. These conventions are derived from the backend implementation and represent expected client behavior rather than implementation details.

---

# Authentication

## Access Token

- Protected endpoints require a JWT access token.
- The frontend should attach it using:

```
Authorization: Bearer <access-token>
```

- Never send a user id manually. The backend always identifies the authenticated user from the JWT claims.
- Never save the JWT to the localstorage for security concerns

---

## Refresh Token

The refresh endpoint is **not** authenticated using ASP.NET authorization.

Instead:

- The refresh token is stored inside an **HttpOnly cookie**.
- The frontend should simply call the refresh endpoint when the access token expires.
- Cookies must be sent automatically.

The frontend never reads or stores the refresh token itself.

---

## Login

After a successful login:

- an access token is returned
- the refresh token cookie is attached automatically by the backend

The frontend should:

1. store the access token somewhere safe
2. allow the browser to store the refresh cookie
3. use the access token for future requests

---

## Logout

Logout invalidates the refresh token.

After logout the frontend should:

- remove the stored access token
- clear any authenticated application state
- redirect the user appropriately

---

## Unauthorized Requests

When a protected endpoint returns **401**:

1. attempt a refresh
2. retry the original request if refresh succeeds
3. otherwise redirect the user to login

Do not continuously retry failed refresh requests.

---

# Error Responses

Every endpoint returns a consistent Problem Details response for failures.

The frontend should rely on:

- HTTP status code
- ErrorCode
- validation fields (when present)

instead of parsing human-readable messages.

Messages are intended for display only.
Messages returned from the Api are user-friendly and you can safely display them to users.

---

## Validation Errors

Validation failures return a list of invalid fields.

Example:

```
{
    "errorFields": [
        {
            "fieldName": "email",
            "errorMessage": "Email is invalid."
        }
    ]
}
```

The frontend should map validation errors by `fieldName`.

---

# Pagination

Endpoints supporting pagination always return a paginated subset of data.


The frontend should:

- request only the page it needs
- never assume all data is returned in a single response

---

# Filtering

Filtering parameters are optional unless stated otherwise.

If omitted, the backend applies its default behavior.

Avoid sending empty strings or placeholder values when a filter is not selected.

---

# Stable Ordering

Whenever endpoints expose ordered collections, the backend defines the ordering.

The frontend should preserve that order.

Do not perform additional client-side sorting unless explicitly required by the UI.

---

# Notifications

Notifications are fetched through polling.

The frontend is responsible for periodically requesting new notifications.

The backend does not push notifications over WebSockets or Server-Sent Events.

Stopping polling when:

- the user logs out
- the application is inactive
- polling is no longer needed

is recommended.

* Note: Notifications are mostly pushed after certain important events when a condition is met for the first time, like after creating an expense that made the user's budget go negative, a notification will be pushed to the user that warns him of that, All subsequent requests for creating expenses won't push the same notification again, it's handled gracefully so it is pushed 1 time for the user's financial month 

---

# Background Processing

Some operations continue processing after the initial request completes.

The initial request only confirms that processing has started.

The frontend should expect eventual consistency rather than immediate completion.

---

# File Upload Flow

Expense attachments are uploaded using a presigned upload flow.

The typical sequence is:

1. Request upload permission.
2. Receive a presigned upload URL.
3. Upload the file directly to object storage.
4. Hit the confirm endpoint to confirm the upload
5. Refresh the relevant resource if necessary.

* Stale uploads are handled in the backend with watchdogs that run consistently in the background.

The backend does **not** receive the file through the API itself.

---

## Attachments
Every expense can only have 1 attachment, the currently supported attachments are:
  - "image/png"
  - "image/jpg"
  - "image/jpeg"

---

## Upload Failures

If uploading to object storage fails:

- allow retrying the upload
- avoid assuming the backend received the file

---

## Upload URL Lifetime

Presigned upload URLs expire.

If an upload URL has expired, request a new one instead of retrying with the old URL.

---


# Eventual Consistency

Some values are computed asynchronously.

Examples include:

- notifications
- background calculations
- scheduled operations

The UI should not assume every change is reflected immediately after a successful request.

Refreshing or polling may be required.

---

# Scheduled Operations

Some features rely on scheduled background jobs.

Changes caused by scheduled jobs may become visible only after those jobs execute.

The frontend should not attempt to predict or duplicate scheduled behavior.

---

# Date Handling

Dates use the backend's date representations.

The frontend should:

- preserve dates exactly
- avoid timezone conversions for date-only values
- send dates using the API format defined by OpenAPI

---

# Idempotency

Some operations are safe to retry after transient failures.

Others create new resources and should not be automatically repeated.

Retry behavior should be conservative unless the endpoint explicitly supports it.

---

# Resource Ownership

Every resource belongs to the authenticated user.

The frontend never supplies ownership information.

Resource access is always determined by the authenticated identity.

---

# OpenAPI

The backend exposes an OpenAPI specification.

Frontend API clients should preferably be generated from that specification rather than handwritten.

Whenever backend contracts change, regenerate the client to stay synchronized.

---

# General Integration Principles

- Trust backend validation.
- Never recreate business rules on the client.
- Use server-provided identifiers.
- Display backend validation messages where appropriate.
- Use `ErrorCode` for application logic.
- Preserve server ordering.
- Handle eventual consistency gracefully.
- Keep polling lightweight and stop it when no longer necessary.
