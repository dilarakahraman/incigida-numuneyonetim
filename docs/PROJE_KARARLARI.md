# Numune Yönetim Sistemi — İş Akışı ve Proje Kararları

Bu belge, proje geliştirilirken yapılan görüşmelerde kesinleştirilen ihtiyaçları ve iş kurallarını kalıcı olarak saklar. Yeni geliştirmeler bu belgeyle uyumlu olmalıdır.

## Teknoloji

- C#
- ASP.NET Core MVC
- Entity Framework Core
- Microsoft SQL Server (`SQLEXPRESS1`)
- Razor Views
- HTML, CSS ve JavaScript
- Üretim ve laboratuvar ekranları arasında canlı bildirim
- İleride yerel C# Print Agent ile otomatik etiket baskısı

## Temel süreç

1. Üretimden numune alınır.
2. Numune bilgileri sisteme kaydedilir.
3. Sistem iç takip kodunu otomatik oluşturur.
4. İlk numune ve stok/ambalaj etiketleri hazırlanır.
5. Numune laboratuvara gider.
6. Laboratuvar analiz sonuçları girilir.
7. Numune onaylanır veya reddedilir.
8. Onaylanan numune için anlaşılmayan anonim dış kod oluşturulur.
9. Müşteri belli olduğunda müşteri adı ve sevkiyat/palet numarası girilir.
10. Son ürün/müşteri etiketi hazırlanır.

## Numune alma bilgileri

Numune alınırken aşağıdaki bilgiler girilir:

- Menşei
- Cins
- Ürün adı
- Paket sayısı
- Paket/ambalaj ağırlığı
- Ambalaj türü
- Stok numarası
- Palet numarası
- Üretim tarihi
- Numune alma tarihi ve saati
- Numuneyi alan personel
- Açıklama

Analiz sonuçları numune alma sırasında girilmez.

## Laboratuvar analizi

Laboratuvar ekranında bulunacak bilgiler:

- Nem değeri
- Yabancı madde oranı
- Analizi yapan personel
- Analiz tarihi
- Laboratuvar açıklaması
- Onay veya ret kararı

**Safiyet alanı kullanılmayacaktır ve sistemden kaldırılmıştır.**

Numune ilk alındığında nem bilinmiyorsa ilk etikette `ANALİZ BEKLİYOR` yazabilir. Analiz tamamlandıktan sonra nem değerini içeren numune etiketi yeniden basılabilir.

## Kod sistemi

### İç takip kodu

İç takip kodu menşei, cins ve sıra numarasından oluşur.

Örnek:

```text
BS20
```

- `B`: Brezilya menşei
- `S`: Simitlik cinsi
- `20`: İlgili menşei/cins grubundaki sıra numarası

Kod kullanıcı tarafından yazılmaz. Sistem otomatik oluşturur ve veritabanında benzersiz olmasını sağlar.

### Stok/parti kodu

Fotoğraflardaki örneğe göre stok etiketi şu yapıda olabilir:

```text
B/10
```

- `B`: Brezilya menşei
- `10`: Stok veya parti sıra numarası

Stok kodu ile numune takip kodu birbirinden ayrı tutulmalıdır.

### Anonim dış kod

Laboratuvar onayından sonra rastgele bir anonim dış kod oluşturulur.

```text
K7RX-4M9T
```

Bu kod aşağıdaki bilgileri içermez:

- Menşei
- Cins
- Müşteri
- Tedarikçi
- Tarih
- İç sıra numarası

## Etiket türleri

### 1. İlk numune etiketi

Numune alındığı anda numune poşetine yapıştırılır.

- İç takip kodu
- Numune alma tarihi
- Menşei
- Cins
- Paket ağırlığı
- Stok/palet numarası
- `Nem: Analiz bekliyor`

### 2. Analizli numune etiketi

Laboratuvar analizi tamamlandıktan sonra basılabilir.

- İç takip kodu
- Numune alma tarihi
- Menşei
- Cins
- Paket ağırlığı
- Stok/palet numarası
- Nem değeri (ör. `%7,52`)
- Analiz kararı

### 3. Stok/ambalaj etiketi

Palet veya ürün ambalajı üzerine yapıştırılır.

Örnek:

```text
STOK A
SİMİTLİK
B/10
```

Gerekirse palet numarası ve barkod da eklenir.

### 4. Müşteri/son ürün etiketi

Yalnızca laboratuvar onayı ve müşteri bilgisi girildikten sonra basılır.

Örnek:

```text
KARAYTAŞ
PASTALIK
2
```

- Müşteri adı zorunludur.
- Cins gösterilir.
- Büyük sayı şimdilik sevkiyat/palet numarası olarak kabul edilmektedir.
- Bu numaranın kesin anlamı saha personeliyle doğrulanmalıdır.
- Menşei müşteriye gösterilmez.
- İç takip kodu müşteriye gösterilmez.

## Müşteri bilgisi

Müşterinin laboratuvar onayı sırasında bilinmesi zorunlu değildir. Laboratuvar onayı anonim dış kodu oluşturur. Müşteri daha sonra sevkiyat aşamasında seçilir veya adı girilir.

Son ürün etiketi için:

- Müşteri adı
- Sevkiyat/palet numarası
- Cins
- Anonim dış kod

bilgileri kullanılır.

## Mobil kullanım

Arayüz telefon ve tablet ekranlarına uyumlu olmalıdır. Telefon ve sunucu bilgisayarı aynı yerel ağdaysa uygulamaya bilgisayarın yerel IP adresi üzerinden erişilebilir.

Örnek:

```text
http://192.168.1.50:5173
```

Sistem yerel ağa açılmadan önce kullanıcı girişi ve rol bazlı yetkilendirme eklenmelidir.

## Etiket yazdırma

Tarayıcıdan standart yazdırma ilk aşamada kullanılabilir. Tam otomatik baskı için üretim bilgisayarında çalışan ayrı bir C# Print Agent planlanmaktadır.

```text
Telefon / Laboratuvar bilgisayarı
            ↓
ASP.NET Core uygulaması
            ↓
MSSQL baskı kuyruğu
            ↓
Üretim bilgisayarındaki Print Agent
            ↓
Etiket yazıcısı
```

Yazıcı marka/modeli ve etiket ölçüleri kesinleştiğinde ZPL, TSPL veya Windows yazıcı sürücüsü seçeneklerinden uygun olanı seçilecektir.

## Açık konular

Aşağıdaki bilgiler saha personeliyle kesinleştirilmelidir:

- Müşteri etiketindeki büyük sayının kesin anlamı
- Stok numarasının otomatik mi yoksa elle mi girileceği
- Paket ağırlığı seçenekleri
- Her etiket türünün fiziksel ölçüsü
- Yazıcı marka ve modeli
- Bir kayıtta basılacak etiket adedi
- Son ürün etiketinde anonim dış kodun görünür olup olmayacağı

