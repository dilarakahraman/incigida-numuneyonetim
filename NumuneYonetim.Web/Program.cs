using Microsoft.EntityFrameworkCore;
using NumuneYonetim.Web.Data;
using NumuneYonetim.Web.Hubs;
using NumuneYonetim.Web.Services;

var currentDirectory = Directory.GetCurrentDirectory();
var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
var contentRoot = Directory.Exists(Path.Combine(currentDirectory, "wwwroot"))
    ? currentDirectory
    : projectDirectory;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = "wwwroot"
});
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!));
builder.Services.AddScoped<NumuneKodService>();
builder.Services.AddSingleton<CanliBildirimService>();
builder.Services.AddHostedService<PrintSimulationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapHub<BaskiHub>("/hubs/baski");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('dbo.Numuneler', 'MusteriAdi') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [MusteriAdi] nvarchar(120) NULL;
        IF COL_LENGTH('dbo.Numuneler', 'SevkiyatPaletNo') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [SevkiyatPaletNo] nvarchar(30) NULL;
        IF COL_LENGTH('dbo.Numuneler', 'StokAlani') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [StokAlani] nvarchar(30) NULL;
        IF COL_LENGTH('dbo.Numuneler', 'StokNo') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [StokNo] nvarchar(30) NULL;
        IF COL_LENGTH('dbo.Numuneler', 'PaketAgirligiKg') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [PaketAgirligiKg] decimal(8,2) NULL;
        IF COL_LENGTH('dbo.Numuneler', 'MusteriPaletSayisi') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [MusteriPaletSayisi] int NULL;
        IF COL_LENGTH('dbo.EtiketBaskilari', 'PaletSiraNo') IS NULL
            ALTER TABLE [dbo].[EtiketBaskilari] ADD [PaletSiraNo] int NULL;
        IF OBJECT_ID('dbo.SusamPaketleri', 'U') IS NULL
            CREATE TABLE [dbo].[SusamPaketleri] ([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY, [Ad] nvarchar(100) NOT NULL, [AktifMi] bit NOT NULL);
        IF OBJECT_ID('dbo.TahinPaketleri', 'U') IS NULL
            CREATE TABLE [dbo].[TahinPaketleri] ([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY, [Ad] nvarchar(120) NOT NULL, [AktifMi] bit NOT NULL);
        IF COL_LENGTH('dbo.Numuneler', 'NumuneTuru') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [NumuneTuru] int NOT NULL CONSTRAINT [DF_Numuneler_NumuneTuru] DEFAULT 1;
        IF COL_LENGTH('dbo.Numuneler', 'SusamPaketiId') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [SusamPaketiId] int NULL;
        IF COL_LENGTH('dbo.Numuneler', 'TahinPaketiId') IS NULL
            ALTER TABLE [dbo].[Numuneler] ADD [TahinPaketiId] int NULL;
        IF NOT EXISTS (SELECT 1 FROM [dbo].[SusamPaketleri])
            INSERT INTO [dbo].[SusamPaketleri] ([Ad],[AktifMi]) VALUES
            (N'İnci Kraft 25 kg',1),(N'İnci Kraft 10 kg',1),(N'İnci Çuval 25 kg',1),(N'İnci Çuval 10 kg',1),(N'Hayat Kraft 25 kg',1),(N'Kale Çuval 25 kg',1),(N'Kale Çuval 10 kg',1);
        IF NOT EXISTS (SELECT 1 FROM [dbo].[TahinPaketleri])
            INSERT INTO [dbo].[TahinPaketleri] ([Ad],[AktifMi]) VALUES
            (N'İnci Kova 10 kg',1),(N'İnci Kova 18 kg',1),(N'İnci PET 1 kg',1),(N'İnci PET 1,75 kg',1),(N'Harras 500 gr Cam Kavanoz Standart',1),(N'Harras 500 gr Cam Kavanoz Çifte Kavruk',1),(N'Hün 600 gr Cam Kavanoz',1),(N'Serel 600 gr Cam Kavanoz',1),(N'İnci 300 gr Cam Kavanoz',1),(N'İnci 600 gr Cam Kavanoz',1);
        UPDATE [dbo].[SusamPaketleri] SET [Ad] = N'İNCİ Kraft 25 kg' WHERE [Id] = 1;
        UPDATE [dbo].[SusamPaketleri] SET [Ad] = N'İNCİ Kraft 10 kg' WHERE [Id] = 2;
        UPDATE [dbo].[SusamPaketleri] SET [Ad] = N'İNCİ Çuval 25 kg' WHERE [Id] = 3;
        UPDATE [dbo].[SusamPaketleri] SET [Ad] = N'İNCİ Çuval 10 kg' WHERE [Id] = 4;
        UPDATE [dbo].[SusamPaketleri] SET [Ad] = N'HAYAT Kraft 25 kg' WHERE [Id] = 5;
        UPDATE [dbo].[SusamPaketleri] SET [Ad] = N'KALE Çuval 25 kg' WHERE [Id] = 6;
        UPDATE [dbo].[SusamPaketleri] SET [Ad] = N'KALE Çuval 10 kg' WHERE [Id] = 7;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'İNCİ Kova 10 kg' WHERE [Id] = 1;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'İNCİ Kova 18 kg' WHERE [Id] = 2;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'İNCİ PET 1 kg' WHERE [Id] = 3;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'İNCİ PET 1,75 kg' WHERE [Id] = 4;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'HARRAS 500 gr Cam Kavanoz Standart' WHERE [Id] = 5;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'HARRAS 500 gr Cam Kavanoz Çifte Kavruk' WHERE [Id] = 6;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'HUN 600 gr Cam Kavanoz' WHERE [Id] = 7;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'SEREL 600 gr Cam Kavanoz' WHERE [Id] = 8;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'İNCİ 300 gr Cam Kavanoz' WHERE [Id] = 9;
        UPDATE [dbo].[TahinPaketleri] SET [Ad] = N'İNCİ 600 gr Cam Kavanoz' WHERE [Id] = 10;
        """);
}

app.Run();
