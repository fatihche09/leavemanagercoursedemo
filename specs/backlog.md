# Backlog — deferred findings

Findings a human triaged as **defer**: real, but fixed later by decision (`workflows/feature-development.md`,
step 7). Nothing lands here without that decision; noise is rejected in the triage record, not parked here.

- A criterion moved out at the ship gate (R-08, `→ moved to B-<n>`) lands here too, by the
  human's ship decision; the entry names the spec and criterion.
- One entry per finding, newest last. Delete the entry in the commit of the work that closes it.
- An entry that grows into new behavior becomes a spec (`specs/active/`), not a fix.

<!-- Format (labels stay English):
- **B-<n>** — <finding, one line> — Source: spec <NNNN>, review round <n> (<file:line>) — Severity: <…>
  — Recommendation: <the reviewer's action> — Deferred by: <name>, <YYYY-MM-DD>, <why>
  A decision deferred at bootstrap (its REPORT) uses Source: bootstrap <YYYY-MM-DD>.
-->
- **B-1** — `scripts/check` ve `scripts/doctor` macOS `/bin/sh` (bash 3.2) altında ayrıştırılamıyor (`$( … )` içindeki `case` → `syntax error near unexpected token ';;'`); `residue`, `init`, `selftest` etkilenmiyor — Source: bootstrap 2026-11-07 — Severity: Medium
  — Recommendation: ANEW çekirdeği yerelde yamalanmaz; upstream'e v1.24.x LTS düzeltmesi olarak bildirilir. O zamana kadar yerelde `dash ./scripts/check` ve `dash ./scripts/doctor`; CI (Ubuntu, `sh` = dash) etkilenmez — Deferred by: fatihcelik-spine, 2026-11-07, upstream LTS düzeltmesi bekleniyor
