# 💊 Pharmacy Stock Management System

ASP.NET Core MVC ve MSSQL kullanılarak geliştirilen, eczanelerin ürün ve stok süreçlerini yönetmesini sağlayan **katmanlı mimariye sahip stok yönetimi ve karar destek sistemi**.

Proje yalnızca klasik stok takibi yapmak yerine stok hareketlerinden elde edilen verileri analiz ederek **stok devir analizi, ABC analizi ve akıllı sipariş önerileri** üretir. Oluşturulan sipariş önerileri yetkili kullanıcı tarafından tedarikçiye PDF raporu olarak e-posta üzerinden gönderilebilir.

---

## 🎯 Projenin Amacı

Eczanelerde stok yönetiminde;

- kritik stokların takip edilmesi,
- tükenen ürünlerin belirlenmesi,
- son kullanma tarihi yaklaşan ürünlerin izlenmesi,
- hızlı ve yavaş hareket eden ürünlerin analiz edilmesi,
- ekonomik açıdan önemli ürünlerin belirlenmesi,
- sipariş ihtiyacının otomatik değerlendirilmesi,
- tedarikçi ve sipariş süreçlerinin yönetilmesi

amaçlanmıştır.

Sistem, operasyonel stok yönetimini analitik karar destek mekanizmalarıyla birleştirmektedir.

---

## ✨ Temel Özellikler

### 📦 Ürün ve Stok Yönetimi

- Ürün yönetimi
- Kategori yönetimi
- Marka yönetimi
- Barkod bilgisi
- Minimum stok seviyesi
- Son kullanma tarihi takibi
- Kritik stok takibi
- Tükenen ürünlerin takibi
- SKT yaklaşan ürünlerin takibi

Ürün stok miktarı doğrudan ürün düzenleme ekranından değiştirilmez.

Yeni ürünler:

```text
CurrentStock = 0
```

değeriyle oluşturulur ve stok yalnızca **stok hareketleri** üzerinden güncellenir.

Bu sayede stok değişikliklerinin geçmişi takip edilebilir.

---

## 🔄 Stok Hareketleri

Sistem stok giriş ve çıkış işlemlerini ayrı hareketler olarak kaydeder.

Her stok hareketinde:

- ürün,
- işlem tipi,
- miktar,
- işlem tarihi,
- seri numarası,
- açıklama,
- işlemi gerçekleştiren kullanıcı

gibi bilgiler tutulabilir.

Stok miktarının güncellenmesi Business katmanındaki stok işlem servisi tarafından gerçekleştirilir.

---

## 📊 Dashboard

Dashboard üzerinden sistemin genel stok durumu takip edilebilir.

Gösterilen bilgiler arasında:

- toplam ürün,
- toplam kategori,
- toplam marka,
- toplam stok,
- kritik stoklar,
- tükenen ürünler,
- yaklaşan son kullanma tarihleri,
- son stok hareketleri,
- yeni eklenen ürünler,
- depo özeti

bulunmaktadır.

Ayrıca **Chart.js** kullanılarak stok hareketleri görselleştirilmektedir.

---

# 📈 Analitik Karar Destek Modülleri

## 🔁 Stok Devir Analizi

Ürünlerin stok giriş ve çıkış hareketleri analiz edilerek stok devir oranları hesaplanır.

Ürünler hareket durumlarına göre:

- 🚀 Hızlı Dönen
- ✅ Normal
- 🐢 Yavaş
- 💤 Ölü Stok

olarak sınıflandırılır.

Bu analiz sayesinde hızlı tüketilen ürünlerin ve gereğinden uzun süre stokta kalan ürünlerin belirlenmesi amaçlanmaktadır.

---

## 🅰️🅱️🅲️ ABC Stok Analizi

ABC analizi ürünlerin ekonomik önemini belirlemek amacıyla kullanılmaktadır.

Her ürün için temel olarak:

```text
Tüketim Değeri = Toplam Stok Çıkışı × Ürün Fiyatı
```

hesaplanır.

Ürünler tüketim değerlerine göre büyükten küçüğe sıralanarak kümülatif yüzdeleri hesaplanır.

Yaklaşık olarak:

```text
A → İlk %80
B → Sonraki %15
C → Kalan %5
```

şeklinde sınıflandırılır.

Bu sayede ekonomik açıdan kritik ürünlere daha sıkı stok kontrolü uygulanabilir.

---

# 🧠 Akıllı Sipariş Karar Destek Sistemi

Projenin önemli modüllerinden biri **kural tabanlı sipariş karar destek sistemidir**.

Sistem ürünlerin stok ve satış verilerini analiz ederek sipariş ihtiyacını değerlendirir.

Değerlendirmede kullanılan faktörler arasında:

- mevcut stok,
- minimum stok,
- son 30 günlük stok çıkışı,
- günlük ortalama tüketim,
- tahmini kalan stok günü,
- stok devir oranı,
- son sipariş tarihi

bulunmaktadır.

Bu bilgiler kullanılarak ürün için bir **sipariş öncelik skoru** oluşturulur.

Sistem sonucunda ürünler:

```text
Acil Sipariş
Sipariş Ver
Takip Et
```

gibi kategorilere ayrılabilir.

> Bu modül bir makine öğrenmesi modeli değildir. Mevcut sürüm, açıklanabilir iş kurallarına dayanan kural tabanlı bir karar destek sistemidir.

---

# 📧 Tedarikçiye Otomatik Sipariş Gönderimi

Yetkili kullanıcı sistem tarafından oluşturulan sipariş önerilerini kayıtlı bir tedarikçiye gönderebilir.

Akış:

```text
Stok Verileri
      ↓
Karar Destek Sistemi
      ↓
Sipariş Önerisi
      ↓
Tedarikçi Seçimi
      ↓
PDF Sipariş Raporu
      ↓
SMTP / E-posta
      ↓
Purchase Order Kaydı
      ↓
Sipariş Geçmişi
```

Sistem:

1. Sipariş önerilerini oluşturur.
2. Kullanıcının tedarikçi seçmesine izin verir.
3. Sipariş raporunu otomatik olarak PDF formatında üretir.
4. PDF'i seçilen tedarikçinin kayıtlı e-posta adresine gönderir.
5. Gönderilen siparişi sipariş geçmişine kaydeder.

PDF raporlarının oluşturulmasında **QuestPDF** kullanılmaktadır.

---

# 🚚 Sipariş Takibi

Gönderilen siparişler sistem içerisinde takip edilebilir.

Sipariş geçmişinde:

- tedarikçi,
- sipariş tarihi,
- sipariş durumu,
- ürünler,
- miktarlar,
- toplam tutar

gibi bilgiler görüntülenebilir.

Sipariş teslim alındığında ilgili süreç stok yönetimiyle tekrar ilişkilendirilerek stokların güncellenmesi sağlanabilir.

---

# 🔐 Authentication & Authorization

Kullanıcı yönetiminde **ASP.NET Core Identity** kullanılmaktadır.

Sistem rol bazlı yetkilendirmeyi desteklemektedir.

Örneğin kritik sipariş işlemleri yalnızca yetkili kullanıcıların erişimine açılabilir.

Identity işlemleri yeni DataAccess katmanındaki:

```text
PharmacyStock.DataAccess.Context.ApplicationDbContext
```

üzerinden çalışmaktadır.

---

# 🏗️ Katmanlı Mimari

Proje sorumlulukların ayrılması amacıyla katmanlı mimariye taşınmıştır.

```text
PharmacyStockManagement
│
├── PharmacyStock.Entities
│
├── PharmacyStock.DTO
│
├── PharmacyStock.DataAccess
│
├── PharmacyStock.Business
│
└── pharmacystock
      └── Web / UI
```

### PharmacyStock.Entities

Domain entity'lerini içerir.

Örneğin:

- Product
- Category
- Brand
- StockTransaction
- Supplier
- PurchaseOrder
- PurchaseOrderItem

---

### PharmacyStock.DTO

Katmanlar arasında taşınan veri modellerini içerir.

Entity modellerinin doğrudan UI katmanına bağımlılığını azaltmak amacıyla DTO yapısı kullanılmaktadır.

---

### PharmacyStock.DataAccess

Veri erişiminden sorumludur.

Bu katmanda:

- ApplicationDbContext
- Repository yapıları
- Unit of Work

bulunmaktadır.

---

### PharmacyStock.Business

Uygulamanın iş kurallarını içerir.

Örneğin:

- ürün işlemleri,
- stok hareketleri,
- dashboard hesaplamaları,
- ABC analizi,
- stok devir analizi,
- sipariş önerileri,
- tedarikçi işlemleri,
- Purchase Order süreçleri,
- PDF ve e-posta işlemleri

bu katmanda yönetilmektedir.

---

### pharmacystock — Web/UI

ASP.NET Core MVC uygulamasıdır.

Bu katmanda:

- Controllers
- Razor Views
- ViewModels
- CSS
- JavaScript

gibi kullanıcı arayüzü bileşenleri bulunmaktadır.

Controller'ların doğrudan `ApplicationDbContext` kullanması yerine Business servisleri üzerinden işlem yapılması hedeflenmiştir.

---

# 🔄 Uygulama Akışı

Genel mimari akış:

```text
┌──────────────────────┐
│       Razor View     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│      Controller      │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│   Business Service   │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ UnitOfWork/Repository│
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ ApplicationDbContext │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│      SQL Server      │
└──────────────────────┘
```

---

# 🛠️ Kullanılan Teknolojiler

| Teknoloji | Kullanım |
|---|---|
| ASP.NET Core MVC | Web uygulaması |
| .NET 8 | Uygulama platformu |
| C# | Backend geliştirme |
| Entity Framework Core | ORM / veri erişimi |
| Microsoft SQL Server | Veritabanı |
| ASP.NET Core Identity | Authentication / Authorization |
| Razor Views | UI |
| Bootstrap | Responsive arayüz |
| JavaScript | Frontend işlemleri |
| Chart.js | Dashboard grafikleri |
| QuestPDF | PDF rapor oluşturma |
| SMTP | E-posta gönderimi |
| ClosedXML | Excel raporlama |

---

# 🧩 Kullanılan Tasarım Yaklaşımları

Projede aşağıdaki yazılım geliştirme yaklaşımları uygulanmıştır:

- Layered Architecture
- Dependency Injection
- Repository Pattern
- Unit of Work Pattern
- DTO Pattern
- Service Layer
- Separation of Concerns
- Role-Based Authorization

---

# ⚙️ Kurulum

Projeyi klonlayın:

```bash
git clone <repository-url>
```

Solution dizinine geçin:

```bash
cd PharmacyStockManagement
```

Bağımlılıkları yükleyin:

```bash
dotnet restore
```

`appsettings.json` içerisinde SQL Server bağlantı ayarlarınızı yapılandırın.

Ardından:

```bash
dotnet build
```

ve uygulamayı çalıştırın:

```bash
dotnet run --project pharmacystock
```

> Güvenlik nedeniyle gerçek veritabanı veya SMTP kimlik bilgilerinin public repository içerisinde paylaşılmaması önerilir.

---

# 🗄️ Veritabanı

Proje Microsoft SQL Server ve Entity Framework Core kullanmaktadır.

Temel ilişkiler:

```text
Category ─────┐
              ├── Product ─── StockTransaction
Brand ────────┘

Supplier ─── PurchaseOrder ─── PurchaseOrderItem
```

---

# 🚀 Gelecek Geliştirmeler

Proje ilerleyen aşamalarda aşağıdaki özelliklerle genişletilebilir:

- satış tahminleme modeli,
- mevsimsel talep analizi,
- otomatik sipariş optimizasyonu,
- barkod okuyucu entegrasyonu,
- alternatif/eşdeğer ürün önerileri,
- gelişmiş audit log sistemi,
- daha detaylı rol ve yetki yönetimi,
- API katmanı,
- Docker deployment,
- otomatik test kapsamının artırılması.

---

# 📌 Proje Durumu

```text
Build:   Successful
Errors:  0
Warnings: 0
```

Temel stok yönetimi, analitik modüller, sipariş karar destek sistemi, rol bazlı yetkilendirme ve PDF/e-posta sipariş akışı çalışır durumdadır.

---

## 👩‍💻 Geliştirici

**Seher Polat**  
Computer Engineering