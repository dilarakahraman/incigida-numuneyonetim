using System.ComponentModel.DataAnnotations;
using NumuneYonetim.Web.Models;

namespace NumuneYonetim.Web.ViewModels;

public class NumuneOlusturVm
{
    [Required] public int MenseiId { get; set; }
    [Required] public int CinsId { get; set; }
    [Required, MaxLength(120)] public string UrunAdi { get; set; } = "Susam";
    [Range(1, 100000)] public int PaketSayisi { get; set; }
    [Required] public DateTime UretimTarihi { get; set; } = DateTime.Today;
    [Required] public DateTime NumuneAlmaTarihi { get; set; } = DateTime.Now;
    [Required] public int AmbalajTuruId { get; set; }
    [MaxLength(40)] public string? PaletNo { get; set; }
    [MaxLength(500)] public string? Aciklama { get; set; }
    [Required, MaxLength(100)] public string Kaydeden { get; set; } = "Kalite Personeli";
}

public class AnalizGirVm
{
    public int NumuneId { get; set; }
    [Range(0, 100)] public decimal NemDegeri { get; set; }
    [Range(0, 100)] public decimal? Safiyet { get; set; }
    [Range(0, 100)] public decimal? YabanciMadde { get; set; }
    [Required, MaxLength(100)] public string AnalizYapan { get; set; } = "Laboratuvar Kalite Personeli";
    [MaxLength(500)] public string? Aciklama { get; set; }
    public bool OnaylandiMi { get; set; }
}

public class DashboardVm
{
    public List<Numune> SonNumuneler { get; set; } = [];
    public int BugunAlinan { get; set; }
    public int AnalizBekleyen { get; set; }
    public int Onaylanan { get; set; }
    public int BekleyenBaski { get; set; }
}
