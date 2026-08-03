using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NumuneYonetim.Web.Models;

public enum NumuneDurumu { AnalizBekliyor = 1, Analizde = 2, Onaylandi = 3, Reddedildi = 4 }

public class Numune
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string IcTakipKodu { get; set; } = "";
    [MaxLength(20)] public string? AnonimDisKod { get; set; }
    public int SiraNumarasi { get; set; }
    public int MenseiId { get; set; }
    public Mensei Mensei { get; set; } = null!;
    public int CinsId { get; set; }
    public Cins Cins { get; set; } = null!;
    [Required, MaxLength(120)] public string UrunAdi { get; set; } = "";
    public int PaketSayisi { get; set; }
    [Column(TypeName = "date")] public DateTime UretimTarihi { get; set; }
    public DateTime NumuneAlmaTarihi { get; set; }
    public int AmbalajTuruId { get; set; }
    public AmbalajTuru AmbalajTuru { get; set; } = null!;
    [MaxLength(40)] public string? PaletNo { get; set; }
    [MaxLength(500)] public string? Aciklama { get; set; }
    public NumuneDurumu Durum { get; set; } = NumuneDurumu.AnalizBekliyor;
    [MaxLength(100)] public string Kaydeden { get; set; } = "Kalite Personeli";
    public DateTime KayitTarihi { get; set; } = DateTime.Now;
    public NumuneAnaliz? Analiz { get; set; }
    public ICollection<EtiketBaski> BaskiKayitlari { get; set; } = [];
}

public class NumuneAnaliz
{
    public int Id { get; set; }
    public int NumuneId { get; set; }
    public Numune Numune { get; set; } = null!;
    [Column(TypeName = "decimal(5,2)")] public decimal NemDegeri { get; set; }
    [Column(TypeName = "decimal(6,2)")] public decimal? Safiyet { get; set; }
    [Column(TypeName = "decimal(6,2)")] public decimal? YabanciMadde { get; set; }
    [MaxLength(100)] public string AnalizYapan { get; set; } = "Laboratuvar Kalite Personeli";
    public DateTime AnalizTarihi { get; set; } = DateTime.Now;
    [MaxLength(500)] public string? Aciklama { get; set; }
    public bool OnaylandiMi { get; set; }
}
