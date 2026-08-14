using System.ComponentModel.DataAnnotations;
using NumuneYonetim.Web.Models;

namespace NumuneYonetim.Web.ViewModels;

public class NumuneOlusturVm
{
    [Required] public NumuneTuru NumuneTuru { get; set; } = NumuneTuru.Susam;
    public int? SusamPaketiId { get; set; }
    public int? TahinPaketiId { get; set; }
    [Required] public int MenseiId { get; set; }
    [Required] public int CinsId { get; set; }
    [Range(1, 100000)] public int PaketSayisi { get; set; }
    [Required] public DateTime UretimTarihi { get; set; } = DateTime.Today;
    [Required] public DateTime NumuneAlmaTarihi { get; set; } = DateTime.Now;
    [Required] public int AmbalajTuruId { get; set; }
    [MaxLength(40)] public string? PaletNo { get; set; }
    [Required, RegularExpression("^STOK [AB]$", ErrorMessage = "Stok alanı STOK A veya STOK B olmalıdır.")]
    public string StokAlani { get; set; } = "STOK A";
    [Required, MaxLength(30)] public string StokNo { get; set; } = "";
    [Range(0.01, 100000)] public decimal PaketAgirligiKg { get; set; } = 25;
    [MaxLength(500)] public string? Aciklama { get; set; }
    [Required, MaxLength(100)] public string Kaydeden { get; set; } = "Kalite Personeli";
}

public class AnalizGirVm
{
    public int NumuneId { get; set; }
    [Range(0, 100)] public decimal NemDegeri { get; set; }
    [Range(0, 100)] public decimal? YabanciMadde { get; set; }
    [Required, MaxLength(100)] public string AnalizYapan { get; set; } = "Laboratuvar Kalite Personeli";
    [MaxLength(500)] public string? Aciklama { get; set; }
    public bool OnaylandiMi { get; set; }
}

public class NumuneDuzenleVm
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string UrunAdi { get; set; } = "";
    [Range(1, 100000)] public int PaketSayisi { get; set; }
    [Range(0.01, 100000)] public decimal PaketAgirligiKg { get; set; }
    [Required] public DateTime UretimTarihi { get; set; }
    [Required] public DateTime NumuneAlmaTarihi { get; set; }
    [Required] public int AmbalajTuruId { get; set; }
    [MaxLength(40)] public string? PaletNo { get; set; }
    [MaxLength(500)] public string? Aciklama { get; set; }
    [Required, MaxLength(100)] public string Kaydeden { get; set; } = "";
}

public class SonUrunHazirlaVm
{
    public int NumuneId { get; set; }
    [Required, MaxLength(120)] public string MusteriAdi { get; set; } = "";
    [Range(1, 1000, ErrorMessage = "Palet sayısı en az 1 olmalıdır.")]
    public int PaletSayisi { get; set; } = 1;
}

public class DashboardVm
{
    public List<Numune> SonNumuneler { get; set; } = [];
    public int BugunAlinan { get; set; }
    public int AnalizBekleyen { get; set; }
    public int Onaylanan { get; set; }
    public int BekleyenBaski { get; set; }
}
