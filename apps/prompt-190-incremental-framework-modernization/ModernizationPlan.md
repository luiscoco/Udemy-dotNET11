# Incremental modernization plan

| Dependency or boundary | Current owner | Incremental treatment |
|---|---|---|
| System.Web.HttpUtility | net48 adapter | Use WebUtility.HtmlEncode in the modern invoice and verify contract parity. |
| System.Web.Extensions JavaScriptSerializer | net48 adapter | Replace at the modern route with System.Text.Json while retaining the legacy adapter. |
| Legacy report integration | net48 process | Keep its route on the legacy adapter until an equivalent replacement is verified. |

Inventory runtime, deployment, authentication, data ownership and dependencies before extraction. Introduce a gateway, move one route, compare consumer contracts, observe operations, and keep a route-level rollback. Retire an integration only after its replacement and consumers are verified. The unsupported project is a deliberate compilation probe.
