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
- **B-1** — `scripts/check` ve `scripts/doctor` macOS `/bin/sh` (bash 3.2) altında ayrıştırılamıyor (`$( … )` içindeki `case` → `syntax error near unexpected token ';;'`); `init` ve `selftest` etkilenmiyor; `residue` check'i PATH'teki `sh` ile çağırdığı için o da düşer (VERIFY 0001, V-2) — Source: bootstrap 2026-11-07 — Severity: Medium
  — Recommendation: ANEW çekirdeği yerelde yamalanmaz; upstream'e v1.24.x LTS düzeltmesi olarak bildirilir. O zamana kadar yerelde `dash ./scripts/check`, `dash ./scripts/doctor`; residue için PATH'e `sh → /bin/dash` bağlantısı olan bir klasör eklenir; CI (Ubuntu, `sh` = dash) etkilenmez — Deferred by: fatihcelik-spine, 2026-11-07, upstream LTS düzeltmesi bekleniyor
- **B-2** — `.github/workflows/check.yml`: `actions/checkout@v4` Node 20 hedefliyor (deprecation uyarısı); toolchain kurulumu GEREKMİYOR — PR #2 CI'ında ubuntu-latest .NET 10, Node 24 ve Docker'ı zaten sağladı (önkoşullar ok, 2026-11-09) — Source: spec 0001, self-critique G-2 — Severity: Medium
  — Recommendation: `actions/checkout`'u Node 24 hedefleyen sürüme yükselt; runner imajı değişirse (Ubuntu 26) sürümleri `actions/setup-dotnet`/`setup-node` ile sabitle — Deferred by: fatihcelik-spine, 2026-11-07, push ertelendi
- **B-3** — `scripts/residue` macOS'ta `launchd`'nin başlattığı sistem ajanlarını (mdworker, Microsoft AutoUpdate, mdmclient, corecaptured…) ebeveyni PID 1 olduğu için "orphan / check'in" sayıyor; temiz bir check'te bile RESIDUE FOUND verir — Source: spec 0001, BUILD residue (2026-11-07) — Severity: Low
  — Recommendation: upstream'e ANEW v1.24.x LTS düzeltmesi olarak bildir (örn. macOS'ta sistem yollarındaki launchd çocuklarını "not from check" say); o zamana kadar survivor listesi elle incelenir, toolchain süreci (dotnet/MSBuild/node/docker) yoksa "not from check" — Deferred by: fatihcelik-spine, 2026-11-07, araç sınırı, check'in sızıntısı değil
- **B-4** — Frontend testlerinde locale `tr-TR` sabitlenmemiş (`docs/testing.md`, Determinism); yalnızca `TZ=UTC` var — Source: spec 0001, review round 1 (`src/frontend/vite.config.ts`) — Severity: Info
  — Recommendation: tarih/sayı biçimlendiren ilk ekranla birlikte test ortamında locale'i `tr-TR`'ye sabitle — Deferred by: fatihcelik-spine, 2026-11-07, şu an locale'e bağlı kod yok
- **B-5** — AC-6 zaman aşımı testi 5 s'yi üretim sabitinden (`HEALTH_TIMEOUT_MS`) okuyor; sabit 30 s yapılsa test yeşil kalır — Source: spec 0001, VERIFY (V-1, `src/frontend/src/ApiStatus.test.tsx`) — Severity: Low
  — Recommendation: testte `5000` sabitini kullan ya da `expect(HEALTH_TIMEOUT_MS).toBe(5000)` ekle; frontend'e dokunan ilk spec'te — Deferred by: fatihcelik-spine, 2026-11-07, dördüncü fix round R-06 gerektirirdi; AC-6 kasıtlı kırmalar ve S-5 ile kanıtlı
- **B-6** — Spec 0001 AC-8 (`Residue: clean (fast + full)`) ship'te kanıtlanamadı: macOS'ta residue launchd ajanlarını orphan sayıyor (B-3) ve `/bin/sh` altında koşamıyor (B-1) — Source: spec 0001, AC-8 moved out at SHIP (R-08) — Severity: Medium
  — Recommendation: B-1/B-3 upstream'de düzelince ya da CI (Ubuntu, B-2) residue koşunca `Residue: clean (fast + full)` kanıtını al; gerçek sızıntı (dotnet list → MSBuild node) A-2 ile kapatıldı — Deferred by: fatihcelik-spine, 2026-11-07, ship kararı
