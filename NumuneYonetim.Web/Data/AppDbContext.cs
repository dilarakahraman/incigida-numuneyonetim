using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Models;

namespace NumuneYonetim.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Numune> Numuneler => Set<Numune>();
    public DbSet<NumuneAnaliz> NumuneAnalizleri => Set<NumuneAnaliz>();
    public DbSet<Cins> Cinsler => Set<Cins>();
    public DbSet<Mensei> Menseiler => Set<Mensei>();
    public DbSet<AmbalajTuru> AmbalajTurleri => Set<AmbalajTuru>();
    public DbSet<EtiketBaski> EtiketBaskilari => Set<EtiketBaski>();
    public DbSet<SusamPaketi> SusamPaketleri => Set<SusamPaketi>();
    public DbSet<TahinPaketi> TahinPaketleri => Set<TahinPaketi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Numune>().HasIndex(x => x.IcTakipKodu).IsUnique();
        modelBuilder.Entity<Numune>().HasIndex(x => x.AnonimDisKod).IsUnique().HasFilter("[AnonimDisKod] IS NOT NULL");
        modelBuilder.Entity<NumuneAnaliz>().HasIndex(x => x.NumuneId).IsUnique();
        modelBuilder.Entity<Cins>().HasIndex(x => x.Kod).IsUnique();
        modelBuilder.Entity<Mensei>().HasIndex(x => x.Kod).IsUnique();

        modelBuilder.Entity<Cins>().HasData(
            new Cins { Id = 1, Ad = "Baharatlık", Kod = "BA" }, new Cins { Id = 2, Ad = "Beyaz", Kod = "BY" },
            new Cins { Id = 3, Ad = "Bisküvilik", Kod = "BK" }, new Cins { Id = 4, Ad = "Ç.Kavruk", Kod = "CK" },
            new Cins { Id = 5, Ad = "O.Pekmezli", Kod = "OP" }, new Cins { Id = 6, Ad = "Pastalık", Kod = "PA" },
            new Cins { Id = 7, Ad = "Simitlik", Kod = "S" }, new Cins { Id = 8, Ad = "Tahinlik", Kod = "T" });
        modelBuilder.Entity<Mensei>().HasData(
            new Mensei { Id = 1, Ad = "B.VICTORIA", Kod = "BV" }, new Mensei { Id = 2, Ad = "BREZİLYA K3", Kod = "B" },
            new Mensei { Id = 3, Ad = "BURKİNO FASO", Kod = "BF" }, new Mensei { Id = 4, Ad = "ÇAD-MAIDIGURİ", Kod = "CM" },
            new Mensei { Id = 5, Ad = "ETİYOPYA HUMERA", Kod = "EH" }, new Mensei { Id = 6, Ad = "FİL DİŞİ", Kod = "FD" },
            new Mensei { Id = 7, Ad = "GAMBİYA", Kod = "GA" }, new Mensei { Id = 8, Ad = "GİNE", Kod = "GI" },
            new Mensei { Id = 9, Ad = "MALAWİ", Kod = "MW" }, new Mensei { Id = 10, Ad = "MALİ", Kod = "ML" },
            new Mensei { Id = 11, Ad = "MISIR", Kod = "MI" }, new Mensei { Id = 12, Ad = "MOZAMBİK", Kod = "MZ" },
            new Mensei { Id = 13, Ad = "NİJERYA BAUCHI", Kod = "NB" }, new Mensei { Id = 14, Ad = "NİJERYA KANO", Kod = "NK" },
            new Mensei { Id = 15, Ad = "NİJERYA LAFİA", Kod = "NL" }, new Mensei { Id = 16, Ad = "PAKİSTAN", Kod = "PK" },
            new Mensei { Id = 17, Ad = "SENEGAL", Kod = "SN" }, new Mensei { Id = 18, Ad = "SUDAN GADARİF", Kod = "SG" },
            new Mensei { Id = 19, Ad = "SUDAN WHITISH", Kod = "SW" }, new Mensei { Id = 20, Ad = "UGANDA", Kod = "UG" });
        modelBuilder.Entity<AmbalajTuru>().HasData(
            new AmbalajTuru { Id = 1, Ad = "Çuval" }, new AmbalajTuru { Id = 2, Ad = "Big Bag" },
            new AmbalajTuru { Id = 3, Ad = "Kraft Torba" }, new AmbalajTuru { Id = 4, Ad = "Dökme" });
    }
}
