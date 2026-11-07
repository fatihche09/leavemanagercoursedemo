# Conventions

> Bootstrap'ta karar verildi (2026-11-07). Buradaki her kural ya araçla zorlanır (tercih) ya da
> review'da kontrol edilir.

## Language
- Chat language (interviews, sessions): Türkçe (`tr`)
- Document language (docs/, specs, plans, ADRs, review and verify reports): Türkçe (`tr`)
- Always English (protocol, not prose): ANEW core files, template headings and field labels,
  `Status:` values (Draft / Approved / In progress / Shipped), `Approved by / on:`, `Source:`.
- Code identifiers, branch names, commit messages: İngilizce. API alanları ve DB adları da
  İngilizce; UI metinleri Türkçe. PR açıklaması Türkçe olabilir.
- Agent messages (handoff summaries, refusals, `Next:` lines): chat language; command names, file
  paths and protocol fields verbatim.

## Language & framework versions
- .NET SDK 10.0.x (`global.json` ile sabitlenir), C# varsayılan dil sürümü, ASP.NET Core 10.
- EF Core 10 + Npgsql provider; PostgreSQL 17.
- xUnit v3, `Microsoft.AspNetCore.Mvc.Testing`, Testcontainers.PostgreSql, NetArchTest.
- Node.js 24 LTS, npm 11, TypeScript 5 (`strict`), React 19, Vite 7, Vitest 3, React Testing
  Library, MSW.
- Kesin patch sürümleri walking skeleton'da sabitlenir (`global.json`, `Directory.Packages.props`,
  `package-lock.json`).

## Naming
- C#: standart .NET konvansiyonları, file-scoped namespace. Servis `LeaveRequestService`, DTO
  `CreateLeaveRequestDto`, repository `LeaveRequestRepository`.
- Testler: `Method_Condition_ExpectedResult`; test projeleri `<Proje>.Tests`
  (entegrasyon: `LeaveManager.Api.IntegrationTests`).
- React: bileşen dosyaları PascalCase (`LeaveRequestForm.tsx`), hook'lar `useXxx`, testler
  `*.test.tsx` yan yana.
- API: kebab-case çoğul kaynaklar (`/api/leave-requests`), JSON alanları camelCase.
- Branch'ler ve commit'ler: `docs/git.md`.

## Error handling
- Business, iş kuralı ihlallerini **`Result<T>` + hata kodu** olarak döner; exception yalnızca
  beklenmeyen durumlar içindir.
- Api, sonuçları **RFC 9457 ProblemDetails**'e çevirir: doğrulama 400, yetkisiz 403, bulunamadı
  404, iş kuralı/durum çakışması 409. Yanıt her zaman bir hata kodu taşır (ör.
  `leave.insufficient_balance`).
- Stack trace, SQL, iç mesaj asla kullanıcıya sızmaz; beklenmeyen hata 500 + `traceId`.

## Data rules
- İzin tarihleri `DateOnly` (DB: `date`) — saat dilimi yok. Audit zaman damgaları
  (`CreatedAt`, `DecidedAt`) UTC `DateTimeOffset` (DB: `timestamptz`).
- Gün sayıları ve bakiye `int` (tam gün). Bakiye saklanmaz, her seferinde hesaplanır.
- ID'ler `Guid` (UUID v7, `Guid.CreateVersion7()`).
- Optimistic concurrency: `LeaveRequest` üzerinde PostgreSQL `xmin` concurrency token.
- Enum'lar DB'ye string olarak yazılır.
- Zaman `TimeProvider` üzerinden okunur; `DateTime.Now`/`UtcNow` doğrudan kullanılmaz.

## Known traps
- xUnit v3 (Microsoft Testing Platform) VSTest'in `--filter "Category!=Integration"` sözdizimini
  değil `--filter-trait` / `--filter-not-trait` kullanır; yanlış filtre sessizce 0 test çalıştırır —
  filtreyi sabitle ve her iki profilde test sayısını doğrula — from bootstrap 2026-11-07.
- Npgsql 6+ `timestamptz`'ye `Kind=Local/Unspecified` `DateTime` yazınca exception atar —
  `DateTimeOffset` (UTC) kullan, `EnableLegacyTimestampBehavior` asla açma — from bootstrap 2026-11-07.
- EF Core'da concurrency token yoksa iki eşzamanlı onay ikisi de başarılı olur, bakiye iki kez
  düşer — `LeaveRequest`'te `xmin`'i `IsRowVersion()` ile işaretle ve entegrasyon testiyle kanıtla —
  from bootstrap 2026-11-07.

## Enforced by tooling
<!-- Walking skeleton bunları scripts/check'e bağladığında geçerli olur. -->
- `<Nullable>enable</Nullable>` ve `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
  (`Directory.Build.props`).
- `dotnet format --verify-no-changes` (`.editorconfig`).
- Katman bağımlılıkları: proje referansları + mimari testleri (`docs/architecture.md`).
- TypeScript `strict`, ESLint (typescript-eslint), Prettier denetimi.
