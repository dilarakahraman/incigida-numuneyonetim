using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Data;
using NumuneYonetim.Web.Models;
using NumuneYonetim.Web.Services;
using NumuneYonetim.Web.ViewModels;

namespace NumuneYonetim.Web.Controllers;

public class LaboratuvarController(AppDbContext db, NumuneKodService kodService, CanliBildirimService bildirim) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins)
        .Where(x => x.Durum == NumuneDurumu.AnalizBekliyor || x.Durum == NumuneDurumu.Analizde)
        .OrderBy(x => x.NumuneAlmaTarihi).ToListAsync());

    public async Task<IActionResult> Analiz(int id)
    {
        var numune = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).FirstOrDefaultAsync(x => x.Id == id);
        if (numune is null) return NotFound();
        ViewBag.Numune = numune;
        return View(new AnalizGirVm { NumuneId = id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Analiz(AnalizGirVm vm)
    {
        var numune = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).FirstOrDefaultAsync(x => x.Id == vm.NumuneId);
        if (numune is null) return NotFound();
        if (!ModelState.IsValid) { ViewBag.Numune = numune; return View(vm); }
        var analiz = new NumuneAnaliz { NumuneId = vm.NumuneId, NemDegeri = vm.NemDegeri, Safiyet = vm.Safiyet,
            YabanciMadde = vm.YabanciMadde, AnalizYapan = vm.AnalizYapan, Aciklama = vm.Aciklama, OnaylandiMi = vm.OnaylandiMi };
        db.NumuneAnalizleri.Add(analiz);
        numune.Durum = vm.OnaylandiMi ? NumuneDurumu.Onaylandi : NumuneDurumu.Reddedildi;
        if (vm.OnaylandiMi)
        {
            numune.AnonimDisKod = await kodService.YeniAnonimKodAsync();
            db.EtiketBaskilari.Add(new EtiketBaski { NumuneId = numune.Id, EtiketTipi = EtiketTipi.SonUrun });
        }
        await db.SaveChangesAsync();
        bildirim.Yayinla("analiz-tamamlandi");
        TempData["Basarili"] = vm.OnaylandiMi ? $"Numune onaylandı. Anonim kod: {numune.AnonimDisKod}" : "Numune reddedildi; dış kod oluşturulmadı.";
        return RedirectToAction("Detay", "Numuneler", new { id = numune.Id });
    }
}
