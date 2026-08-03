using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Data;
using NumuneYonetim.Web.Models;
using NumuneYonetim.Web.ViewModels;

namespace NumuneYonetim.Web.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var bugun = DateTime.Today;
        return View(new DashboardVm
        {
            BugunAlinan = await db.Numuneler.CountAsync(x => x.KayitTarihi >= bugun),
            AnalizBekleyen = await db.Numuneler.CountAsync(x => x.Durum == NumuneDurumu.AnalizBekliyor || x.Durum == NumuneDurumu.Analizde),
            Onaylanan = await db.Numuneler.CountAsync(x => x.Durum == NumuneDurumu.Onaylandi),
            BekleyenBaski = await db.EtiketBaskilari.CountAsync(x => x.Durum == BaskiDurumu.Bekliyor),
            SonNumuneler = await db.Numuneler.Include(x => x.Mensei).Include(x => x.Cins)
                .OrderByDescending(x => x.KayitTarihi).Take(8).ToListAsync()
        });
    }
}
