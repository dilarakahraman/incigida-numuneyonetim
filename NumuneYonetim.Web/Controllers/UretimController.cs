using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Data;
using NumuneYonetim.Web.Models;
using NumuneYonetim.Web.Services;

namespace NumuneYonetim.Web.Controllers;

public class UretimController(AppDbContext db, CanliBildirimService bildirim) : Controller
{
    public async Task<IActionResult> Index() => View(await db.EtiketBaskilari.Include(x => x.Numune).ThenInclude(x => x.Cins)
        .OrderByDescending(x => x.OlusturmaTarihi).Take(50).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Basildi(int id)
    {
        var baski = await db.EtiketBaskilari.FindAsync(id);
        if (baski is null) return NotFound();
        baski.Durum = BaskiDurumu.Basarili; baski.BasimTarihi = DateTime.Now; baski.DenemeSayisi++;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("canli-akis")]
    public async Task Akis(CancellationToken cancellationToken)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        var (id, reader) = bildirim.AboneOl();
        try
        {
            await foreach (var olay in reader.ReadAllAsync(cancellationToken))
            {
                await Response.WriteAsync($"data: {olay}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
        finally { bildirim.AboneliktenCik(id); }
    }
}
