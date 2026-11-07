# Spec 0001 — Walking skeleton

- Status: In progress
- Mode: lite
- Plan: `specs/plans/0001-plan.md`
- Self-critique: done

## Intent
Geliştirici ekip (ve kurs katılımcıları), ilk iş özelliğine başlamadan önce uçtan uca çalışan,
doğrulanabilir en küçük sistemi istiyor: tarayıcıdaki uygulama, API ve veritabanı birbirine
bağlı çalışmalı ve bunu tek komutla (`./scripts/check`) kanıtlayabilmeli. Başarı; bootstrap'ta
planlanan tüm doğrulama adımlarının gerçek kodla çalışması, hızlı ve tam profillerin doğru
testleri koşması ve bir doğrulama çalıştırmasının makinede hiçbir şey bırakmamasıdır. Bilinçli
olarak hiçbir izin yönetimi davranışı (talep, onay, bakiye), kimlik doğrulama veya iş ekranı
yapılmaz.

## Requirements
- R-1: Sistem, kendisinin ve veritabanı bağlantısının sağlıklı olup olmadığını bildiren bir
  sağlık durumu sunar. Bu durum kimlik doğrulama gerektirmez.
- R-2: Veritabanına ulaşılamadığında sağlık durumu "sağlıksız" olarak bildirilir; yanıt hiçbir
  teknik ayrıntı (bağlantı dizesi, sunucu adı, hata yığını) içermez.
- R-3: Veritabanı şeması boş bir veritabanında, sürümlenmiş şema geçişleriyle (migration)
  oluşturulur; sistem elle kurulum gerektirmez.
- R-4: `docs/architecture.md`'deki katman kuralları (yasak bağımlılık 1 ve 4) otomatik bir
  testle denetlenir; bir ihlal doğrulamayı kırmızıya düşürür.
- R-5: Web uygulamasının açılış sayfası, uygulama adını ve API'nin sağlık durumunu kullanıcıya
  Türkçe olarak gösterir.
- R-6: `scripts/check.conf`'ta bootstrap'ta planlanan adımlar etkinleşir; `./scripts/check`
  tam profilde hepsini, `--fast` profilinde yalnızca `[fast]` etiketlileri çalıştırır.
- R-7: Bir doğrulama çalıştırması (iki profilde de) makinede çalışır hâlde süreç veya container
  bırakmaz ve sonraki çalıştırmaya veri taşımaz.

## User experience
- Flow: Kullanıcı web uygulamasının kök adresini açar → sayfa "LeaveManager" başlığını ve API
  durumunu gösterir. Başka etkileşim yoktur.
- States:
  - Yükleniyor: durum sorgusu sürerken "API durumu kontrol ediliyor…" metni.
  - Başarılı: "API çalışıyor" metni.
  - Hata: API'ye ulaşılamıyor ya da sağlıksız yanıt veriyor → "API'ye şu anda ulaşılamıyor."
    metni; teknik ayrıntı gösterilmez.
  - Boş: uygulanamaz (gösterilecek veri yok). Yetkisiz: uygulanamaz (kimlik doğrulama yok).
- Design reference: none — the flow above is the design.
- Accessibility: durum metni `role="status"` ile ekran okuyucuya duyurulur; durum yalnızca
  renkle değil metinle ifade edilir; sayfada klavye ile erişilmesi gereken etkileşim yoktur.

## Constraints & out of scope
- Stack ve sürümler: `docs/conventions.md`; katmanlar: `docs/architecture.md`; test katmanları
  ve fast/full ayrımı: `docs/testing.md`; Known traps (xUnit v3 filtresi) dikkate alınır.
- Doğrulama yerelde `dash ./scripts/check` ile çalışır (backlog B-1).
- Sensitive area: none.
- Fast profil süresi kabul kriteri değildir; BUILD kapısında 3 çalıştırmanın medyanıyla
  raporlanır. AC-7'deki toplam test sayısının ölçüm yöntemini plan belirler.
- OUT OF SCOPE: kimlik doğrulama/yetkilendirme (JWT, default-deny), ProblemDetails hata
  eşlemesi, CORS, OpenAPI, herhangi bir iş entity'si veya ekranı, seed verisi,
  `.github/workflows/check.yml` toolchain kurulumu (push ertelendiği için; B-2), deploy/container imajı.

## Acceptance criteria
- [ ] AC-1 — Uygulama ve veritabanı ayaktayken sağlık durumu kimlik doğrulamasız istenir ve
  "sağlıklı" döner; veritabanı, boş bir veritabanına migration'lar uygulanarak hazırlanmıştır.
- [ ] AC-2 — Veritabanına ulaşılamadığında sağlık durumu en geç 5 saniye içinde "sağlıksız" döner ve yanıt gövdesi
  bağlantı dizesi, sunucu adı veya hata yığını içermez.
- [ ] AC-3 — Bir alt katmanın üst katmana bağımlılığı veya `Entities`'in başka bir projeye
  bağımlılığı mimari testiyle yakalanır (test, kasıtlı bir ihlalle kırmızıya düştüğü gösterilerek
  kanıtlanır).
- [ ] AC-4 — Açılış sayfası, durum sorgusu sürerken yükleniyor metnini gösterir.
- [ ] AC-5 — API sağlıklı yanıt verdiğinde açılış sayfası "API çalışıyor" metnini ve
  "LeaveManager" başlığını gösterir.
- [ ] AC-6 — API'ye ulaşılamadığında, sağlıksız yanıt verdiğinde veya 5 saniye içinde yanıt
  vermediğinde açılış sayfası hata metnini gösterir, teknik ayrıntı göstermez; durum metni `role="status"` ile sunulur.
- [ ] AC-7 — `./scripts/check` tam profilde `check.conf`'taki tüm adımları, `--fast` yalnızca
  `[fast]` adımlarını yeşil çalıştırır; fast'te çalışan test sayısı + yalnızca-full test sayısı =
  toplam test sayısı ve hiçbir test adımı 0 test çalıştırmaz.
- [ ] AC-8 — `./scripts/residue` ve `./scripts/residue --fast` temiz çıkar
  (`Residue: clean (fast + full)`); arka arkaya iki tam çalıştırma ikincisinde de yeşildir.

## Definition of Done
- [ ] Every acceptance criterion mapped to proof (test or reproducible observation)
- [ ] `scripts/check` green
- [ ] Independent review done; real findings fixed, noise rejected with written rationale, deferred ones in `specs/backlog.md`
- [ ] Docs / ADRs updated if behavior or architecture changed
- [ ] Spec moved to `specs/done/` (it becomes immutable there)

## Scorecard (fill at ship — honest numbers make the process improvable)
| Metric | Value |
|---|---|
| Spec revisions | |
| Fix rounds | |
| Review findings: real / noise / deferred | |
| Regressions introduced | |
| Bugs escaped to production | |
