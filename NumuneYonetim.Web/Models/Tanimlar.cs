using System.ComponentModel.DataAnnotations;

namespace NumuneYonetim.Web.Models;

public class Cins
{
    public int Id { get; set; }
    [Required, MaxLength(60)] public string Ad { get; set; } = "";
    [Required, MaxLength(4)] public string Kod { get; set; } = "";
    public bool AktifMi { get; set; } = true;
}

public class Mensei
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string Ad { get; set; } = "";
    [Required, MaxLength(4)] public string Kod { get; set; } = "";
    public bool AktifMi { get; set; } = true;
}

public class AmbalajTuru
{
    public int Id { get; set; }
    [Required, MaxLength(60)] public string Ad { get; set; } = "";
    public bool AktifMi { get; set; } = true;
}
