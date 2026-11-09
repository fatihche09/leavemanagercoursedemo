# Architecture

> Bootstrap'ta karar verildi (2026-11-07). Ajanlar plan yapmadan önce bu dosyayı okur.

## System overview
LeaveManager, çalışanların izin talep ettiği, yöneticilerin onayladığı/reddettiği ve İK'nın izin
türleri ile yıllık hakları tanımladığı bir izin yönetimi uygulamasıdır (kurs demosu). Backend tek
bir **.NET 10 ASP.NET Core Web API**'dir ve klasik **N-Layer** mimariyle kurulur:
Api → Business → DataAccess → Entities. Kalıcılık **EF Core 10 + PostgreSQL 17**'dir. Frontend
ayrı bir **React 19 + Vite** SPA'dır ve backend'le yalnızca HTTP API üzerinden konuşur.
Neden N-Layer: kursun hedef kitlesi için en tanıdık yapı; tek yönlü proje referansları ihlallerin
çoğunu derleme zamanında yakalar (modüler monolit bilinçli olarak seçilmedi).

## Modules / components and ownership

| Module | Single responsibility | Owns |
|---|---|---|
| `LeaveManager.Api` (Presentation) | Controller'lar, kimlik doğrulama, DI kompozisyonu, hata → HTTP (ProblemDetails) eşlemesi, request/response DTO'ları | HTTP sözleşmesi (OpenAPI) |
| `LeaveManager.Business` (BLL) | İş kuralları (BR-1…BR-7), servisler, doğrulama, sahiplik/hiyerarşi yetki kararları | Talep durum makinesi, iş günü ve bakiye hesabı |
| `LeaveManager.DataAccess` (DAL) | EF Core `DbContext`, migration'lar, repository'ler | PostgreSQL şeması |
| `LeaveManager.Entities` | Entity sınıfları ve enum'lar (`LeaveRequestStatus` vb.) | Veri modeli |
| `src/frontend` (React) | Ekranlar, API istemcisi | UI durumu |

Katmanlar içinde klasörler özelliğe göre ayrılır: `Employees`, `LeaveTypes`, `Entitlements`,
`LeaveRequests`. Testler `tests/<Proje>.Tests` altında (`docs/testing.md`).

## Communication rules
- Bağımlılık tek yönlüdür: **Api → Business → DataAccess → Entities**. Frontend → Api yalnızca HTTP.
- Katmanlar arası yalnızca doğrudan, senkron çağrı. Mesajlaşma/event bus yok.
- MediatR/CQRS kütüphanesi yok; use case'ler düz servis sınıflarıdır.
- Business, DataAccess'in repository'lerini doğrudan kullanır (klasik N-Layer; Dependency
  Inversion ilk sürümde yok — değişirse ADR).

## Forbidden dependencies (make them testable)
Mimari testleri (`tests/LeaveManager.Architecture.Tests`, reflection + `.csproj` okuma) 1 ve 4'ü
doğrular; 2 (tip düzeyi) ilk controller'la birlikte test edilir:
1. Alt katman üst katmana referans veremez: DataAccess → Business/Api, Business → Api,
   Entities → herhangi bir proje yasaktır.
2. `Api` içindeki controller'lar `DbContext` veya repository tiplerini kullanmaz; yalnızca
   Business servislerini çağırır. Api'nin DataAccess referansı yalnızca DI kaydı içindir.
3. İş kuralları yalnızca Business'ta yaşar: controller'da, repository'de veya frontend'de bakiye,
   çakışma, iş günü hesabı yapılmaz (frontend en fazla kullanıcı kolaylığı için ön kontrol yapar;
   karar her zaman backend'dedir).
4. `Entities` hiçbir framework'e ve projeye bağımlı değildir. Entity'ler API'de doğrudan dönülmez;
   her yanıt bir DTO'dur.
5. Frontend veritabanına ve backend iç tiplerine erişmez; tek sözleşme API'dir.

## Deliberately out of scope
Mikroservisler, mesaj kuyruğu, event sourcing, CQRS/MediatR, çoklu tenant, bordro entegrasyonu,
resmî tatil takvimi, e-posta bildirimleri, gerçek kimlik sağlayıcısı (IdP).
