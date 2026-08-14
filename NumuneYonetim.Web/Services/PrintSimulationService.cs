using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Data;
using NumuneYonetim.Web.Hubs;
using NumuneYonetim.Web.Models;

namespace NumuneYonetim.Web.Services;

public class PrintSimulationService(
    IServiceScopeFactory scopeFactory,
    CanliBildirimService bildirim,
    IHubContext<BaskiHub> hub,
    IConfiguration configuration,
    ILogger<PrintSimulationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = configuration.GetValue("PrintSimulation:Enabled", true);
        var delayMs = configuration.GetValue("PrintSimulation:DelayMs", 1800);
        if (!enabled)
        {
            logger.LogInformation("Etiket yazıcı simülasyonu kapalı.");
            return;
        }

        logger.LogInformation("Etiket yazıcı simülasyonu aktif: SIM-YAZICI-01");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int? jobId;
                using (var scope = scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var job = await db.EtiketBaskilari
                        .Where(x => x.Durum == BaskiDurumu.Bekliyor)
                        .OrderBy(x => x.OlusturmaTarihi)
                        .FirstOrDefaultAsync(stoppingToken);
                    if (job is null)
                    {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    job.Durum = BaskiDurumu.YaziciyaGonderildi;
                    job.YaziciAdi = "SIMÜLASYON ETİKET YAZICISI";
                    job.DenemeSayisi++;
                    await db.SaveChangesAsync(stoppingToken);
                    jobId = job.Id;
                }

                bildirim.Yayinla("baski-gonderildi");
                await hub.Clients.All.SendAsync("BaskiDurumuDegisti", new { Id = jobId, Durum = "Yazıcıya gönderildi" }, stoppingToken);
                await Task.Delay(delayMs, stoppingToken);

                using (var scope = scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var job = await db.EtiketBaskilari.FindAsync([jobId!.Value], stoppingToken);
                    if (job is not null && job.Durum == BaskiDurumu.YaziciyaGonderildi)
                    {
                        job.Durum = BaskiDurumu.Basarili;
                        job.BasimTarihi = DateTime.Now;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                bildirim.Yayinla("baski-basarili");
                await hub.Clients.All.SendAsync("BaskiDurumuDegisti", new { Id = jobId, Durum = "Baskı başarılı" }, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Yazıcı simülasyonunda hata oluştu.");
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}
