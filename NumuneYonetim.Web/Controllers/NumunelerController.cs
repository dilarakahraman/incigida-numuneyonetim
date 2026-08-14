using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Data;
using NumuneYonetim.Web.Hubs;
using NumuneYonetim.Web.Models;
using NumuneYonetim.Web.Services;
using NumuneYonetim.Web.ViewModels;

namespace NumuneYonetim.Web.Controllers;

public class NumunelerController(AppDbContext db, NumuneKodService kodService, CanliBildirimService bildirim, IHubContext<BaskiHub> hub) : Controller
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
        return View(new NumuneOlusturVm { StokNo = await SonrakiStokNoAsync("STOK A") });
    }

    [HttpGet]
    public async Task<IActionResult> SonrakiStokNo(string stokAlani)
    {
        var alan = (stokAlani ?? "").Trim().ToUpperInvariant();
        if (alan is not ("STOK A" or "STOK B")) return BadRequest();
        return Json(new { stokNo = await SonrakiStokNoAsync(alan) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Yeni(NumuneOlusturVm vm)
    {
        if (vm.NumuneTuru == NumuneTuru.Susam && vm.SusamPaketiId is null)
            ModelState.AddModelError(nameof(vm.SusamPaketiId), "Susam paketlemesi seçilmelidir.");
        if (vm.NumuneTuru == NumuneTuru.Tahin && vm.TahinPaketiId is null)
            ModelState.AddModelError(nameof(vm.TahinPaketiId), "Tahin paketlemesi seçilmelidir.");
        var secilenCins = await db.Cinsler.FirstOrDefaultAsync(x => x.Id == vm.CinsId && x.AktifMi);
        if (secilenCins is null)
            ModelState.AddModelError(nameof(vm.CinsId), "Geçerli bir cins seçilmelidir.");
        if (!ModelState.IsValid) { await SecimleriYukle(); return View(vm); }
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        vm.StokAlani = vm.StokAlani.Trim().ToUpperInvariant();
        vm.StokNo = await SonrakiStokNoAsync(vm.StokAlani);
        var dahiliAmbalajId = await db.AmbalajTurleri.Where(x => x.AktifMi).Select(x => x.Id).FirstAsync();
        var (kod, sira) = await kodService.YeniIcKodAsync(vm.MenseiId, vm.CinsId);
        var numune = new Numune
        {
            IcTakipKodu = kod, SiraNumarasi = sira, MenseiId = vm.MenseiId, CinsId = vm.CinsId,
            NumuneTuru = vm.NumuneTuru,
            SusamPaketiId = vm.NumuneTuru == NumuneTuru.Susam ? vm.SusamPaketiId : null,
            TahinPaketiId = vm.NumuneTuru == NumuneTuru.Tahin ? vm.TahinPaketiId : null,
            UrunAdi = secilenCins!.Ad, PaketSayisi = vm.PaketSayisi, UretimTarihi = vm.UretimTarihi,
            NumuneAlmaTarihi = vm.NumuneAlmaTarihi, AmbalajTuruId = dahiliAmbalajId,
            PaletNo = vm.PaletNo, Aciklama = vm.Aciklama, Kaydeden = vm.Kaydeden,
            StokAlani = vm.StokAlani.Trim().ToUpperInvariant(), StokNo = vm.StokNo.Trim(), PaketAgirligiKg = vm.PaketAgirligiKg
        };
        db.Numuneler.Add(numune);
        await db.SaveChangesAsync();
        db.EtiketBaskilari.AddRange(
            new EtiketBaski { NumuneId = numune.Id, EtiketTipi = EtiketTipi.Palet },
            new EtiketBaski { NumuneId = numune.Id, EtiketTipi = EtiketTipi.Numune });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        bildirim.Yayinla("numune-yeni");
        await hub.Clients.Group("EtiketYazicilari").SendAsync("YeniBaskiIsi", new { NumuneId = numune.Id, Kod = numune.IcTakipKodu });
        TempData["Basarili"] = $"{kod} kodlu numune alındı. Palet ve numune etiketleri baskı kuyruğuna eklendi.";
        return RedirectToAction(nameof(Etiket), new { id = numune.Id, tip = EtiketTipi.Numune });
    }

    public async Task<IActionResult> Detay(int id)
    {
        var numune = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).Include(x => x.AmbalajTuru)
            .Include(x => x.Analiz).Include(x => x.BaskiKayitlari).FirstOrDefaultAsync(x => x.Id == id);
        return numune is null ? NotFound() : View(numune);
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var numune = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).FirstOrDefaultAsync(x => x.Id == id);
        if (numune is null) return NotFound();
        ViewBag.Numune = numune;
        ViewBag.Ambalajlar = new SelectList(await db.AmbalajTurleri.Where(x => x.AktifMi).OrderBy(x => x.Ad).ToListAsync(), "Id", "Ad");
        ViewBag.SusamPaketleri = new SelectList(await db.SusamPaketleri.Where(x => x.AktifMi).OrderBy(x => x.Id).ToListAsync(), "Id", "Ad");
        ViewBag.TahinPaketleri = new SelectList(await db.TahinPaketleri.Where(x => x.AktifMi).OrderBy(x => x.Id).ToListAsync(), "Id", "Ad");
        return View(new NumuneDuzenleVm
        {
            Id = numune.Id, UrunAdi = numune.UrunAdi, PaketSayisi = numune.PaketSayisi,
            PaketAgirligiKg = numune.PaketAgirligiKg ?? 25, UretimTarihi = numune.UretimTarihi,
            NumuneAlmaTarihi = numune.NumuneAlmaTarihi, AmbalajTuruId = numune.AmbalajTuruId,
            PaletNo = numune.PaletNo, Aciklama = numune.Aciklama, Kaydeden = numune.Kaydeden
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(NumuneDuzenleVm vm)
    {
        var numune = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (numune is null) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Numune = numune;
            ViewBag.Ambalajlar = new SelectList(await db.AmbalajTurleri.Where(x => x.AktifMi).OrderBy(x => x.Ad).ToListAsync(), "Id", "Ad");
            return View(vm);
        }
        numune.UrunAdi = vm.UrunAdi.Trim();
        numune.PaketSayisi = vm.PaketSayisi;
        numune.PaketAgirligiKg = vm.PaketAgirligiKg;
        numune.UretimTarihi = vm.UretimTarihi;
        numune.NumuneAlmaTarihi = vm.NumuneAlmaTarihi;
        numune.AmbalajTuruId = vm.AmbalajTuruId;
        numune.PaletNo = string.IsNullOrWhiteSpace(vm.PaletNo) ? null : vm.PaletNo.Trim();
        numune.Aciklama = string.IsNullOrWhiteSpace(vm.Aciklama) ? null : vm.Aciklama.Trim();
        numune.Kaydeden = vm.Kaydeden.Trim();
        await db.SaveChangesAsync();
        bildirim.Yayinla("numune-duzenlendi");
        TempData["Basarili"] = $"{numune.IcTakipKodu} kaydı güncellendi.";
        return RedirectToAction(nameof(Detay), new { id = numune.Id });
    }

    public async Task<IActionResult> SonUrunHazirla(int id)
    {
        var numune = await db.Numuneler.Include(x => x.Cins).FirstOrDefaultAsync(x => x.Id == id);
        if (numune is null) return NotFound();
        if (numune.Durum != NumuneDurumu.Onaylandi || string.IsNullOrEmpty(numune.AnonimDisKod))
            return BadRequest("Son ürün etiketi için laboratuvar onayı gerekir.");
        ViewBag.Numune = numune;
        return View(new SonUrunHazirlaVm { NumuneId = id, MusteriAdi = numune.MusteriAdi ?? "", PaletSayisi = numune.MusteriPaletSayisi ?? 1 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SonUrunHazirla(SonUrunHazirlaVm vm)
    {
        var numune = await db.Numuneler.Include(x => x.Cins).FirstOrDefaultAsync(x => x.Id == vm.NumuneId);
        if (numune is null) return NotFound();
        if (numune.Durum != NumuneDurumu.Onaylandi) return BadRequest("Numune onaylanmamış.");
        if (!ModelState.IsValid) { ViewBag.Numune = numune; return View(vm); }
        numune.MusteriAdi = vm.MusteriAdi.Trim().ToUpperInvariant();
        numune.MusteriPaletSayisi = vm.PaletSayisi;
        numune.SevkiyatPaletNo = vm.PaletSayisi.ToString();
        for (var paletNo = 1; paletNo <= vm.PaletSayisi; paletNo++)
            db.EtiketBaskilari.Add(new EtiketBaski { NumuneId = numune.Id, EtiketTipi = EtiketTipi.SonUrun, PaletSiraNo = paletNo });
        await db.SaveChangesAsync();
        bildirim.Yayinla("son-urun-hazir");
        await hub.Clients.Group("EtiketYazicilari").SendAsync("YeniBaskiIsi", new { NumuneId = numune.Id, EtiketTipi = "SonUrun" });
        TempData["Basarili"] = "Müşteri bilgileri kaydedildi ve son ürün etiketi baskı kuyruğuna eklendi.";
        return RedirectToAction(nameof(Etiket), new { id = numune.Id, tip = EtiketTipi.SonUrun });
    }

    public async Task<IActionResult> Etiket(int id, EtiketTipi tip = EtiketTipi.Numune, int? paletSiraNo = null)
    {
        var numune = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins).Include(x => x.AmbalajTuru)
            .Include(x => x.Analiz).FirstOrDefaultAsync(x => x.Id == id);
        if (numune is null) return NotFound();
        if (tip == EtiketTipi.SonUrun && string.IsNullOrEmpty(numune.AnonimDisKod)) return BadRequest("Son ürün etiketi için laboratuvar onayı gerekir.");
        if (tip == EtiketTipi.SonUrun)
        {
            if (string.IsNullOrWhiteSpace(numune.MusteriAdi) || numune.MusteriPaletSayisi is null)
                return RedirectToAction(nameof(SonUrunHazirla), new { id });
            ViewBag.PaletSiraNo = paletSiraNo ?? 1;
            return View("MusteriEtiket", numune);
        }
        if (tip == EtiketTipi.Palet) return View("StokEtiket", numune);
        return View("NumuneEtiket", numune);
    }

    private async Task SecimleriYukle()
    {
        ViewBag.Menseiler = new SelectList(await db.Menseiler.Where(x => x.AktifMi).OrderBy(x => x.Ad).ToListAsync(), "Id", "Ad");
        ViewBag.Cinsler = new SelectList(await db.Cinsler.Where(x => x.AktifMi).OrderBy(x => x.Ad).ToListAsync(), "Id", "Ad");
        ViewBag.Ambalajlar = new SelectList(await db.AmbalajTurleri.Where(x => x.AktifMi).OrderBy(x => x.Ad).ToListAsync(), "Id", "Ad");
        ViewBag.SusamPaketleri = new SelectList(await db.SusamPaketleri.Where(x => x.AktifMi).OrderBy(x => x.Id).ToListAsync(), "Id", "Ad");
        ViewBag.TahinPaketleri = new SelectList(await db.TahinPaketleri.Where(x => x.AktifMi).OrderBy(x => x.Id).ToListAsync(), "Id", "Ad");
    }

    private async Task<string> SonrakiStokNoAsync(string stokAlani)
    {
        var mevcutNumaralar = await db.Numuneler
            .Where(x => x.StokAlani == stokAlani && x.StokNo != null)
            .Select(x => x.StokNo!)
            .ToListAsync();

        var enBuyuk = mevcutNumaralar
            .Select(x => int.TryParse(x, out var no) ? no : 0)
            .DefaultIfEmpty(0)
            .Max();
        return (enBuyuk + 1).ToString();
    }
}
