# Domain

> Bootstrap'ta karar verildi (2026-11-07). Burada olmayan bir terim için ajan kendi anlamını
> uydurur — yeni terimi önce buraya ekleyin.

## Ubiquitous language

| Term | Meaning | Notes / not to be confused with |
|---|---|---|
| Çalışan (`Employee`) | İzin talep eden kişi; tam olarak bir yöneticiye bağlıdır | Kullanıcı hesabı ≠ çalışan kaydı |
| Yönetici (`Manager`) | Doğrudan bağlı çalışanlarının taleplerini onaylar/reddeder | Yalnızca *doğrudan* bağlılar; dolaylı hiyerarşi yok |
| İK (`HR`) | İzin türlerini ve yıllık hakları tanımlar | Talep onaylamaz (kendi yöneticisi değilse) |
| İzin türü (`LeaveType`) | Yıllık izin, Mazeret izni vb.; `DeductsFromBalance` bakiyeden düşüp düşmediğini belirler | |
| İzin hakkı (`Entitlement`) | Çalışan × yıl × izin türü için hak edilen gün sayısı | Bakiye değildir |
| Bakiye (`Balance`) | Hak − onaylanan günler − bekleyen günler; **hesaplanır, saklanmaz** | |
| İzin talebi (`LeaveRequest`) | Tarih aralığı + tür + durum: `Pending`, `Approved`, `Rejected`, `Cancelled` | |
| İş günü (`Working day`) | Pazartesi–Cuma; ilk sürümde resmî tatiller yok sayılır | Takvim günü değil |
| Talep süresi | Aralıktaki iş günü sayısı (tam gün) | Yarım gün yok |

## Business rules
- **BR-1:** Talep süresi yalnızca iş günleriyle, tam gün birimiyle hesaplanır.
- **BR-2:** Bitiş tarihi başlangıçtan önce olamaz; geçmiş tarihe talep açılamaz.
- **BR-3:** Bakiyeden düşen türlerde talep edilen gün bakiyeyi aşamaz; `Pending` talepler bakiyeyi
  rezerve eder.
- **BR-4:** Bir çalışanın `Pending` veya `Approved` talepleri tarih olarak çakışamaz.
- **BR-5:** Bir talebi yalnızca çalışanın yöneticisi onaylar/reddeder; kimse kendi talebini onaylayamaz.
- **BR-6:** Yalnızca `Pending` bir talep onaylanabilir, reddedilebilir veya iptal edilebilir (iptal
  yalnızca talep sahibi tarafından); `Rejected` ve `Cancelled` nihaidir.
- **BR-7:** Reddetme gerekçesi zorunludur.

## Key domain invariants
- Bakiye hiçbir zaman negatif olamaz (BR-3), eşzamanlı onaylar altında da.
- `Approved` bir talep ilk sürümde değiştirilemez.
- Bir talep aynı anda iki kez karara bağlanamaz (optimistic concurrency, `docs/conventions.md`).

## Sensitive areas
- **Yetkilendirme** — kimin hangi talebi görebildiği, onaylayabildiği, iptal edebildiği (BR-5, BR-6;
  `docs/security.md`).
- **Bakiye hesabı** — hak, rezervasyon ve düşüm (BR-1, BR-3, eşzamanlılık).
