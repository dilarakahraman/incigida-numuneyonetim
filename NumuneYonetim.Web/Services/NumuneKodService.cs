using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Data;

namespace NumuneYonetim.Web.Services;

public class NumuneKodService(AppDbContext db)
{
    private const string Harfler = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public async Task<(string Kod, int Sira)> YeniIcKodAsync(int menseiId, int cinsId)
    {
        var mensei = await db.Menseiler.FindAsync(menseiId) ?? throw new InvalidOperationException("Menşei bulunamadı.");
        var cins = await db.Cinsler.FindAsync(cinsId) ?? throw new InvalidOperationException("Cins bulunamadı.");
        var sonSira = await db.Numuneler.Where(x => x.MenseiId == menseiId && x.CinsId == cinsId)
            .MaxAsync(x => (int?)x.SiraNumarasi) ?? 0;
        return ($"{mensei.Kod}{cins.Kod}{sonSira + 1}", sonSira + 1);
    }

    public async Task<string> YeniAnonimKodAsync()
    {
        string kod;
        do
        {
            var chars = Enumerable.Range(0, 8).Select(_ => Harfler[RandomNumberGenerator.GetInt32(Harfler.Length)]).ToArray();
            kod = $"{new string(chars[..4])}-{new string(chars[4..])}";
        } while (await db.Numuneler.AnyAsync(x => x.AnonimDisKod == kod));
        return kod;
    }
}
