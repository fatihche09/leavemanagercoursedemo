# Security

> Bootstrap'ta karar verildi (2026-11-07). Ajanlar her planda ve review'da bu kurallara uyar.

## Secrets
- Secrets never enter the repo, specs, prompts, or chat. `.env` is gitignored; provide `.env.example`.
- Agents never print secret values, even when debugging.
- Bağlantı dizesi ve JWT imza anahtarı yalnızca `dotnet user-secrets` veya ortam değişkeninden
  okunur. `.env.example` ve `appsettings.Development.json` sır içermez.

## Input & output
- Sınırda doğrulama: request DTO'ları data annotations ile; iş kuralları Business'ta.
- Üretim kodunda tüm sorgular EF Core üzerinden parametrelidir. **Raw SQL yasaktır**
  (`FromSqlRaw`, `ExecuteSqlRaw` vb.).
  Tek istisna: girdi almayan sabit sağlık probu sorgusu (`SELECT 1`), yalnızca
  `LeaveManager.DataAccess` içinde (spec 0001, A-4).
  Testlerde girdi almayan sabit tanı sorguları (ör. `pg_stat_activity` sayımı) serbesttir
  (spec 0001, A-5).
- React varsayılan escape'i kullanır; `dangerouslySetInnerHTML` yasaktır.
- CORS yalnızca frontend origin'ine açıktır.
- Hata yanıtlarında stack trace, SQL, iç mesaj sızmaz (`docs/conventions.md`, Error handling).
- Kişisel veri: çalışan adı ve e-postası tutulur. Reddetme gerekçesini yalnızca ilgili çalışan,
  yöneticisi ve İK görür. Loglara kişisel veri değil, yalnızca ID yazılır.

## AuthN / AuthZ
- **AuthN:** API'nin imzaladığı **JWT Bearer** token'ları. Token, yalnızca Development/Test
  ortamında kayıtlı `POST /api/dev/token` endpoint'inden seed kullanıcılar için alınır;
  Production'da bu endpoint kayıtlı değildir. Gerçek IdP kapsam dışı (gerekirse ADR).
- **Default deny:** global `[Authorize]` (fallback policy). Anonim erişim yalnızca açıkça
  işaretlenen yerlerde: health ve dev token.
- **Roller:** `Employee`, `Manager`, `HR` — rol kontrolü Api katmanında.
- **Sahiplik ve hiyerarşi** ("bu talep senin mi?", "bu çalışanın yöneticisi sen misin?", BR-5)
  Business katmanında kontrol edilir; asla yalnızca UI'da ya da yalnızca rolle.
- Başkasına ait kaynağa erişim girişimi **403** döner (404 değil), varlığın gizlenmesini spec
  açıkça istemedikçe.
- Yetkilendirme bir hassas alandır (`docs/domain.md`).

## Dependencies
- Yeni NuGet/npm paketi planda gerekçesiyle önerilir; plan onayı = paket onayı.
- Kontrol: lisans MIT / Apache-2.0 / BSD (GPL ailesi yasak), son 12 ayda bakım görmüş, bilinen
  High/Critical CVE yok.
- `scripts/check` (full) bağımlılık denetimi yapar: `dotnet list package --vulnerable
  --include-transitive` ve `npm audit --audit-level=high`.

## Review lens
Security is a mandatory dimension of every independent review (see `prompts/review.md`), not a
separate afterthought phase.
