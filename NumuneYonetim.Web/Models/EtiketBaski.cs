using System.ComponentModel.DataAnnotations;

namespace NumuneYonetim.Web.Models;

public enum BaskiDurumu { Bekliyor = 1, YaziciyaGonderildi = 2, Basarili = 3, Hatali = 4, Iptal = 5 }
public enum EtiketTipi { Palet = 1, Numune = 2, SonUrun = 3 }

public class EtiketBaski
{
    public int Id { get; set; }
    public int NumuneId { get; set; }
    public Numune Numune { get; set; } = null!;
    public EtiketTipi EtiketTipi { get; set; }
    public BaskiDurumu Durum { get; set; } = BaskiDurumu.Bekliyor;
    public int KopyaSayisi { get; set; } = 1;
    [MaxLength(100)] public string? YaziciAdi { get; set; }
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    public DateTime? BasimTarihi { get; set; }
    [MaxLength(500)] public string? HataMesaji { get; set; }
    public int DenemeSayisi { get; set; }
}
