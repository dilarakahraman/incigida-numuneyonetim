using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Data;
using NumuneYonetim.Web.Models;
using NumuneYonetim.Web.Services;
using NumuneYonetim.Web.ViewModels;

namespace NumuneYonetim.Web.Controllers;

public class NumunelerController(AppDbContext db, NumuneKodService kodService, CanliBildirimService bildirim) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        var sorgu = db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).Include(x => x.AmbalajTuru).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) sorgu = sorgu.Where(x => x.IcTakipKodu.Contains(q) || (x.AnonimDisKod != null && x.AnonimDisKod.Contains(q)) || x.UrunAdi.Contains(q));
        ViewBag.Q = q;
        return View(await sorgu.OrderByDescending(x => x.KayitTarihi).ToListAsync());
    }

    public async Task<IActionResult> Yeni()
    {
        await SecimleriYukle();
        return View(new NumuneOlusturVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Yeni(NumuneOlusturVm vm)
    {
        if (!ModelState.IsValid) { await SecimleriYukle(); return View(vm); }
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var (kod, sira) = await kodService.YeniIcKodAsync(vm.MenseiId, vm.CinsId);
        var numune = new Numune
        {
            IcTakipKodu = kod, SiraNumarasi = sira, MenseiId = vm.MenseiId, CinsId = vm.CinsId,
            UrunAdi = vm.UrunAdi, PaketSayisi = vm.PaketSayisi, UretimTarihi = vm.UretimTarihi,
            NumuneAlmaTarihi = vm.NumuneAlmaTarihi, AmbalajTuruId = vm.AmbalajTuruId,
            PaletNo = vm.PaletNo, Aciklama = vm.Aciklama, Kaydeden = vm.Kaydeden
        };
        db.Numuneler.Add(numune);
        await db.SaveChangesAsync();
        db.EtiketBaskilari.AddRange(
            new EtiketBaski { NumuneId = numune.Id, EtiketTipi = EtiketTipi.Palet },
            new EtiketBaski { NumuneId = numune.Id, EtiketTipi = EtiketTipi.Numune });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        bildirim.Yayinla("numune-yeni");
        TempData["Basarili"] = $"{kod} kodlu numune alındı. Palet ve numune etiketleri baskı kuyruğuna eklendi.";
        return RedirectToAction(nameof(Etiket), new { id = numune.Id, tip = EtiketTipi.Numune });
    }

    public async Task<IActionResult> Detay(int id)
    {
        var numune = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).Include(x => x.AmbalajTuru)
            .Include(x => x.Analiz).Include(x => x.BaskiKayitlari).FirstOrDefaultAsync(x => x.Id == id);
        return numune is null ? NotFound() : View(numune);
    }

    public async Task<IActionResult> Etiket(int id, EtiketTipi tip = EtiketTipi.Numune)
    {
        var numune = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).Include(x => x.AmbalajTuru).FirstOrDefaultAsync(x => x.Id == id);
        if (numune is null) return NotFound();
        if (tip == EtiketTipi.SonUrun && string.IsNullOrEmpty(numune.AnonimDisKod)) return BadRequest("Son ürün etiketi için laboratuvar onayı gerekir.");
        ViewBag.EtiketTipi = tip;
        return View(numune);
    }

    private async Task SecimleriYukle()
    {
        ViewBag.Menseiler = new SelectList(await db.Menseiler.Where(x => x.AktifMi).OrderBy(x => x.Ad).ToListAsync(), "Id", "Ad");
        ViewBag.Cinsler = new SelectList(await db.Cinsler.Where(x => x.AktifMi).OrderBy(x => x.Ad).ToListAsync(), "Id", "Ad");
        ViewBag.Ambalajlar = new SelectList(await db.AmbalajTurleri.Where(x => x.AktifMi).OrderBy(x => x.Ad).ToListAsync(), "Id", "Ad");
    }
}
