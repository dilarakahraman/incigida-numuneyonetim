# Numune Yönetim Sistemi

Numune Yönetim Sistemi; üretimden numune alınması, numunenin laboratuvarda analiz edilmesi, kalite kararının kaydedilmesi ve farklı aşamalarda ihtiyaç duyulan etiketlerin hazırlanması süreçlerini tek merkezden yönetmek amacıyla geliştirilen bir web uygulamasıdır.

Uygulama C# ve ASP.NET Core MVC ile geliştirilir, veriler Microsoft SQL Server üzerinde saklanır. Laboratuvar ve üretim bilgisayarları aynı merkezi sisteme bağlanır. Böylece laboratuvarda oluşturulan veya güncellenen bir kayıt üretim ekranında anlık olarak görülebilir ve ilgili etiket baskı kuyruğuna otomatik eklenebilir.

## Projenin amacı

Projenin temel amacı numune süreçlerini kağıt, Excel dosyası ve sözlü bildirimlerden bağımsız hâle getirerek izlenebilir bir dijital süreç oluşturmaktır.

Sistem aşağıdaki ihtiyaçları karşılar:

- Üretimden alınan numunenin anında kaydedilmesi
- Her numune için benzersiz bir iç takip kodu oluşturulması
- Palet ve numune etiketlerinin kayıtla birlikte hazırlanması
- Numunenin laboratuvar analiz sürecinin takip edilmesi
- Analiz sonuçlarının ayrı bir laboratuvar kaydı olarak tutulması
- Numunenin onaylanması veya reddedilmesi
- Onaylanan ürün için menşei ve cins bilgisi içermeyen anonim dış kod oluşturulması
- Son ürün etiketinin yalnızca laboratuvar onayından sonra hazırlanması
- Üretim bilgisayarında kayıtların ve baskı işlerinin anlık görüntülenmesi
- Etiketlerin ne zaman, hangi yazıcıdan ve kaç adet basıldığının takip edilmesi

## İş akışı

Süreç iki temel aşamadan oluşur: **numune alma** ve **laboratuvar analizi**.

```text
Üretimden numune alınır
        ↓
Numune bilgileri sisteme kaydedilir
        ↓
İç takip kodu oluşturulur (ör. BS20)
        ↓
Palet ve numune etiketleri baskı kuyruğuna eklenir
        ↓
Numune laboratuvara gönderilir
        ↓
Analiz değerleri girilir
        ↓
Numune onaylanır veya reddedilir
        ↓
Onaylandıysa anonim dış kod oluşturulur (ör. K7RX-4M9T)
        ↓
Son ürün etiketi baskı kuyruğuna eklenir
```

### 1. Numune alma

Kalite personeli numuneyi aldığı anda aşağıdaki bilgileri kaydeder:

- Menşei
- Ürün cinsi
- Ürün adı
- Paket sayısı
- Palet numarası
- Üretim tarihi
- Numune alma tarihi ve saati
- Ambalaj türü
- Numuneyi alan personel
- Açıklama

Analiz değerleri bu aşamada girilmez. Kayıt tamamlandığında sistem otomatik olarak iç takip kodunu üretir ve iki ayrı baskı işi oluşturur:

1. Palet etiketi
2. Numune etiketi

### 2. Laboratuvar analizi

Laboratuvar personeli analiz bekleyen numuneyi iç takip koduyla açar ve analiz sonuçlarını kaydeder.

İlk sürümde bulunan analiz alanları:

- Nem değeri
- Safiyet
- Yabancı madde oranı
- Analizi yapan personel
- Analiz açıklaması
- Onay veya ret kararı

Analiz alanları ürün ve laboratuvar gereksinimlerine göre genişletilebilir.

### 3. Laboratuvar kararı

- Numune **onaylanırsa** anonim dış kod oluşturulur ve son ürün etiketi baskı kuyruğuna eklenir.
- Numune **reddedilirse** anonim dış kod ve son ürün etiketi oluşturulmaz.

## Kodlama sistemi

Sistemde aynı numune için farklı amaçlara hizmet eden iki ayrı kod bulunur.

### İç takip kodu

İç takip kodu fabrika içerisinde kullanılır. Menşei kodu, cins kodu ve ilgili gruba ait sıra numarasından oluşur.

```text
BS20
```

Örnekte:

- `B`: BREZİLYA K3
- `S`: Simitlik
- `20`: Bu menşei ve cins grubundaki sıra numarası

Başka örnekler:

```text
BBA8   → BREZİLYA K3 / Baharatlık / 8. numune
NKS14  → NİJERYA KANO / Simitlik / 14. numune
EHT7   → ETİYOPYA HUMERA / Tahinlik / 7. numune
```

Kod kullanıcı tarafından yazılmaz. Sistem, MSSQL üzerinde ilgili menşei ve cins grubunun son sıra numarasını güvenli bir işlem içerisinde kontrol ederek yeni kodu oluşturur. İç takip kodu veritabanında benzersizdir.

### Anonim dış kod

Anonim dış kod yalnızca laboratuvar onayından sonra oluşturulur. Kod aşağıdaki bilgileri içermez:

- Menşei
- Ürün cinsi
- Müşteri
- Tedarikçi
- Tarih
- İç sıra numarası
- İç takip kodu

Örnek:

```text
K7RX-4M9T
```

Kod kriptografik rastgelelik kullanılarak oluşturulur. Okunabilirliği artırmak amacıyla `0/O` ve `1/I` gibi birbirine benzeyen karakterler kullanılmaz. Müşterinin kim olduğu anonim kod oluşturulurken bilinmek zorunda değildir. Müşteri veya sevkiyat bağlantısı gerektiğinde daha sonraki bir süreçte ayrıca kurulabilir.

## Başlangıç tanımları

### Cinsler

| Cins | Kod |
|---|---:|
| Baharatlık | BA |
| Beyaz | BY |
| Bisküvilik | BK |
| Ç.Kavruk | CK |
| O.Pekmezli | OP |
| Pastalık | PA |
| Simitlik | S |
| Tahinlik | T |

### Menşeiler

| Menşei | Kod |
|---|---:|
| B.VICTORIA | BV |
| BREZİLYA K3 | B |
| BURKİNO FASO | BF |
| ÇAD-MAIDIGURİ | CM |
| ETİYOPYA HUMERA | EH |
| FİL DİŞİ | FD |
| GAMBİYA | GA |
| GİNE | GI |
| MALAWİ | MW |
| MALİ | ML |
| MISIR | MI |
| MOZAMBİK | MZ |
| NİJERYA BAUCHI | NB |
| NİJERYA KANO | NK |
| NİJERYA LAFİA | NL |
| PAKİSTAN | PK |
| SENEGAL | SN |
| SUDAN GADARİF | SG |
| SUDAN WHITISH | SW |
| UGANDA | UG |

Menşei ve cins kodları yönetim ekranından değiştirilebilir şekilde tasarlanacaktır. Kullanılmış bir tanım doğrudan silinmek yerine pasif duruma alınmalıdır; böylece geçmiş kayıtların bütünlüğü korunur.

### Ambalaj türleri

Başlangıçta aşağıdaki ambalaj türleri tanımlıdır:

- Çuval
- Big Bag
- Kraft Torba
- Dökme

## Etiket türleri

| Etiket | Oluşturulma zamanı | Gösterilen kod |
|---|---|---|
| Palet etiketi | Numune alındığında | İç takip kodu |
| Numune etiketi | Numune alındığında | İç takip kodu |
| Son ürün etiketi | Laboratuvar onayından sonra | Anonim dış kod |

### Palet ve numune etiketi

Fabrika içinde kullanıldığı için aşağıdaki bilgileri içerebilir:

- İç takip kodu
- Menşei
- Cins
- Ürün adı
- Palet numarası
- Paket sayısı
- Üretim tarihi
- Numune alma tarihi
- Ambalaj türü
- Barkod veya QR kod

### Son ürün etiketi

Müşteriye veya fabrika dışına çıkabilecek etiket yalnızca anonim kodu ve paylaşılması uygun ürün bilgilerini içerir. İç takip kodu, menşei ve tedarikçiyi belli edebilecek bilgiler etikette bulunmaz.

## Anlık veri akışı

Laboratuvar ve üretim bilgisayarları merkezi ASP.NET Core uygulamasına bağlanır. Uygulamada Server-Sent Events tabanlı canlı bildirim altyapısı bulunur.

Yeni numune veya tamamlanan analiz sonrasında:

1. Kayıt MSSQL'e yazılır.
2. Bağlı üretim ekranlarına canlı bildirim gönderilir.
3. Üretim ekranı yeni kayıtları ve baskı kuyruğunu yeniler.
4. Otomatik baskı aktifse yerel yazdırma uygulaması işi yazıcıya gönderir.

Canlı bağlantı koparsa veriler kaybolmaz; asıl kayıt MSSQL üzerinde saklandığı için ekran yeniden bağlandığında güncel baskı kuyruğunu tekrar yükleyebilir.

## Etiket baskı kuyruğu

Her baskı talebi ayrı bir kayıt olarak tutulur. Bir baskı kaydı aşağıdaki bilgileri içerir:

- İlgili numune
- Etiket türü
- Kopya sayısı
- Hedef yazıcı
- Baskı durumu
- Oluşturulma zamanı
- Basılma zamanı
- Deneme sayısı
- Hata mesajı

Baskı durumları:

```text
Bekliyor
Yazıcıya Gönderildi
Başarılı
Hatalı
İptal Edildi
```

## Yazıcı entegrasyonu

Yazıcının marka ve modeli henüz belli olmadığı için ilk sürümde baskı kuyruğu ve standart tarayıcı yazdırma görünümü hazırlanır.

Tarayıcılar güvenlik nedeniyle kullanıcı onayı olmadan doğrudan ve sessiz baskı yapamaz. Tam otomatik baskı için üretim bilgisayarında arka planda çalışacak ayrı bir C# yazdırma uygulaması planlanmaktadır:

```text
ASP.NET Core uygulaması
        ↓
MSSQL baskı kuyruğu
        ↓
NumuneYonetim.PrintAgent
        ↓
Etiket yazıcısı
```

Yazıcı modeli belli olduğunda aşağıdaki yöntemlerden uygun olanı kullanılabilir:

- Windows yazıcı sürücüsü
- Zebra yazıcılarda ZPL
- TSC yazıcılarda TSPL
- Yazıcı üreticisinin SDK veya servis yazılımı

Print Agent aynı baskı işinin yanlışlıkla iki kez yazdırılmasını engellemeli ve baskı sonucunu merkezi sisteme bildirmelidir.

## Uygulama ekranları

### Genel Bakış

- Bugün alınan numune sayısı
- Analiz bekleyen numune sayısı
- Onaylanan numune sayısı
- Bekleyen baskı işi sayısı
- Son hareketler

### Numune Al

- Numune bilgilerinin kaydedilmesi
- İç kodun otomatik oluşturulması
- Palet ve numune etiketlerinin hazırlanması

### Laboratuvar

- Analiz bekleyen numunelerin listelenmesi
- Analiz sonuçlarının girilmesi
- Onay veya ret kararının verilmesi
- Onayda anonim dış kod oluşturulması

### Üretim ve Baskı

- Yeni kayıtların anlık görüntülenmesi
- Baskı kuyruğunun takip edilmesi
- Yazdırma ve tekrar yazdırma işlemleri
- Yazıcı ve hata durumlarının görüntülenmesi

### Numune Kayıtları

- İç veya dış kodla arama
- Ürün ve tarihe göre filtreleme
- Numune, analiz ve baskı geçmişini görüntüleme

## Veri modeli

Temel tablolar:

```text
Numuneler
NumuneAnalizleri
Cinsler
Menseiler
AmbalajTurleri
EtiketBaskilari
```

Temel ilişki yapısı:

```text
Mensei ───────┐
Cins ─────────┼── Numune ─── NumuneAnaliz
AmbalajTuru ──┘       │
                      └────── EtiketBaski (birden fazla)
```

## Kullanılan teknolojiler

- .NET 9
- C#
- ASP.NET Core MVC
- Razor Views
- Entity Framework Core
- Microsoft SQL Server
- HTML, CSS ve JavaScript
- Server-Sent Events
- IIS / Windows Server

## Proje yapısı

```text
NumuneYonetim.sln
└── NumuneYonetim.Web
    ├── Controllers
    ├── Data
    ├── Models
    ├── Services
    ├── ViewModels
    ├── Views
    ├── wwwroot
    ├── Program.cs
    └── appsettings.json
```

İlk çalışan sürüm tek ASP.NET Core projesi içinde tutulmaktadır. Proje büyüdüğünde iş kuralları ve veri erişimi ayrı class library projelerine ayrılabilir:

```text
NumuneYonetim.Web
NumuneYonetim.Application
NumuneYonetim.Domain
NumuneYonetim.Infrastructure
NumuneYonetim.PrintAgent
```

## Gereksinimler

- .NET 9 SDK
- Microsoft SQL Server veya SQL Server Express
- Visual Studio 2022, JetBrains Rider ya da Visual Studio Code
- Geliştirme için Windows, Linux veya macOS
- Etiket baskısı için üretimde Windows bilgisayar ve uyumlu yazıcı sürücüsü

## Veritabanı bağlantısı

Varsayılan geliştirme bağlantısı `NumuneYonetim.Web/appsettings.json` dosyasındadır:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=NumuneYonetimDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Kullanılan SQL Server örneğine göre `Server` değeri değiştirilmelidir. Üretim ortamında bağlantı bilgileri kaynak kodda tutulmamalı; ortam değişkeni, IIS yapılandırması veya güvenli bir secret store kullanılmalıdır.

## Kurulum ve çalıştırma

Depoyu klonlayın:

```bash
git clone <repository-url>
cd numune-yonetim-sistemi
```

Paketleri geri yükleyin:

```bash
dotnet restore
```

Projeyi derleyin:

```bash
dotnet build
```

Uygulamayı çalıştırın:

```bash
dotnet run --project NumuneYonetim.Web
```

Uygulama ilk çalıştırmada yapılandırılmış MSSQL sunucusunda `NumuneYonetimDb` veritabanını ve başlangıç tanımlarını oluşturur.

## Güvenlik ve iş kuralları

- İç ve anonim dış kodlar veritabanında benzersiz olmalıdır.
- Anonim dış kod yalnızca başarılı laboratuvar onayından sonra oluşturulmalıdır.
- Reddedilen numune için son ürün etiketi basılamamalıdır.
- Analiz ve onay işlemini yapan kullanıcı ile işlem zamanı kaydedilmelidir.
- Kullanılmış menşei, cins ve ambalaj kayıtları fiziksel olarak silinmemeli; pasif duruma alınmalıdır.
- Etiket tekrar basıldığında yeni bir baskı geçmişi kaydı oluşturulmalıdır.
- Müşteriye giden etikette iç kod, menşei veya tedarikçiyi belli eden bilgiler bulunmamalıdır.
- Aynı baskı işi Print Agent tarafından birden fazla kez işlenmemelidir.
- Üretim ortamında HTTPS, kimlik doğrulama ve rol bazlı yetkilendirme zorunlu olmalıdır.

## Planlanan kullanıcı rolleri

- **Numune alan kalite personeli:** Numune kaydı oluşturur ve iç etiketleri hazırlar.
- **Laboratuvar kalite personeli:** Analiz sonuçlarını girer ve karar verir.
- **Üretim personeli:** Baskı kuyruğunu görür ve etiketleri yazdırır.
- **Yönetici:** Tanımları, kullanıcıları ve sistem ayarlarını yönetir.

## Geliştirme yol haritası

- [x] ASP.NET Core MVC proje yapısı
- [x] MSSQL ve Entity Framework Core veri modeli
- [x] Cins, menşei ve ambalaj başlangıç verileri
- [x] İç takip kodu üretim servisi
- [x] Numune alma ekranı
- [x] Laboratuvar analiz ekranı
- [x] Onay sonrası anonim dış kod üretimi
- [x] Etiket baskı kuyruğu modeli
- [x] Canlı üretim bildirimi altyapısı
- [ ] Numune detay ekranının tamamlanması
- [ ] Üretim ve baskı ekranının tamamlanması
- [ ] Etiket şablonlarının yazıcı ölçüsüne göre hazırlanması
- [ ] Kullanıcı girişi ve rol bazlı yetkilendirme
- [ ] Menşei, cins ve ambalaj yönetim ekranları
- [ ] Gelişmiş arama ve raporlama
- [ ] Excel ve PDF dışa aktarma
- [ ] Print Agent uygulaması
- [ ] Gerçek etiket yazıcısı entegrasyonu
- [ ] Otomatik testler
- [ ] IIS üretim kurulumu

## Durum

Proje aktif geliştirme aşamasındadır. Veritabanı modeli ve temel iş akışı oluşturulmuştur. Etiket yazıcısının marka/modeli ve kullanılacak etiket ölçüsü netleştikten sonra gerçek otomatik baskı entegrasyonu tamamlanacaktır.

